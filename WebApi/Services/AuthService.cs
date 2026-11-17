using Microsoft.EntityFrameworkCore;
using WebApi.Common;
using WebApi.Data;
using WebApi.DTOs;
using WebApi.Models;

namespace WebApi.Services
{
    /// <summary>
    /// Implementación de la lógica de negocio para autenticación de clientes y direcciones.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly TamboDbContext _context;
        private readonly ILogger<AuthService> _logger;

        /// <summary>
        /// Constructor con inyección de dependencias del DbContext y Logger.
        /// </summary>
        /// <param name="context">Contexto de base de datos TamboDB.</param>
        /// <param name="logger">Servicio de logging del sistema.</param>
        public AuthService(TamboDbContext context, ILogger<AuthService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Servicio 12: Inicia sesión validando correo, contraseña hasheada y estado activo del cliente.
        /// </summary>
        /// <param name="request">Credenciales del cliente.</param>
        /// <returns>Respuesta con datos de usuario y token de sesión si las credenciales son válidas.</returns>
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Correo) || string.IsNullOrWhiteSpace(request.Contrasena))
            {
                return new LoginResponseDto
                {
                    Exito = false,
                    Mensaje = "Para continuar es obligatorio ingresar correo electrónico y contraseña."
                };
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Correo.ToLower() == request.Correo.Trim().ToLower() && !c.Borrado);

            if (cliente == null)
            {
                return new LoginResponseDto
                {
                    Exito = false,
                    Mensaje = "Credenciales incorrectas o cliente no registrado."
                };
            }

            // Regla de Negocio: Solo pueden iniciar sesión los clientes activos (cliente.activo = 1)
            if (!cliente.Activo)
            {
                return new LoginResponseDto
                {
                    Exito = false,
                    Mensaje = "La cuenta de usuario se encuentra inactiva. Comuníquese con soporte de Tambo+."
                };
            }

            bool passwordValida = PasswordHasher.Verify(request.Contrasena, cliente.Contrasena);
            if (!passwordValida)
            {
                return new LoginResponseDto
                {
                    Exito = false,
                    Mensaje = "Contraseña incorrecta."
                };
            }

            cliente.UltimoAcceso = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generación de token de sesión representativo
            string token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{cliente.ClienteId}:{cliente.Correo}:{DateTime.UtcNow.Ticks}"));

            return new LoginResponseDto
            {
                Exito = true,
                Mensaje = "Inicio de sesión exitoso.",
                Token = token,
                Cliente = new ClienteResponseDto
                {
                    ClienteId = cliente.ClienteId,
                    TipoDocumento = cliente.TipoDocumento,
                    NumeroDocumento = cliente.NumeroDocumento,
                    Nombres = cliente.Nombres,
                    Apellidos = cliente.Apellidos,
                    Correo = cliente.Correo,
                    Celular = cliente.Celular,
                    Activo = cliente.Activo,
                    FechaRegistro = cliente.FechaRegistro
                }
            };
        }

        /// <summary>
        /// Servicio 13: Registra un nuevo usuario verificando que ni el correo ni el documento existan previamente.
        /// Aplica hashing criptográfico a la contraseña antes del almacenamiento.
        /// </summary>
        /// <param name="request">Formulario de registro del cliente.</param>
        /// <returns>Resultado del registro con sesión iniciada.</returns>
        public async Task<LoginResponseDto> RegistroAsync(RegistroClienteDto request)
        {
            // Regla de Negocio: El correo no debe estar registrado previamente
            bool correoExiste = await _context.Clientes
                .AnyAsync(c => c.Correo.ToLower() == request.Correo.Trim().ToLower());

            if (correoExiste)
            {
                return new LoginResponseDto
                {
                    Exito = false,
                    Mensaje = "El correo electrónico ya se encuentra registrado."
                };
            }

            // Regla de Negocio: El número de documento no debe estar registrado previamente
            bool documentoExiste = await _context.Clientes
                .AnyAsync(c => c.TipoDocumento == request.TipoDocumento && c.NumeroDocumento == request.NumeroDocumento.Trim());

            if (documentoExiste)
            {
                return new LoginResponseDto
                {
                    Exito = false,
                    Mensaje = $"El {request.TipoDocumento} N° {request.NumeroDocumento} ya se encuentra registrado."
                };
            }

            // Hashing seguro de contraseña con PBKDF2 / SHA-256
            string passwordHash = PasswordHasher.Hash(request.Contrasena);

            var nuevoCliente = new Cliente
            {
                TipoDocumento = request.TipoDocumento.Trim().ToUpper(),
                NumeroDocumento = request.NumeroDocumento.Trim(),
                Nombres = request.Nombres.Trim(),
                Apellidos = request.Apellidos.Trim(),
                Correo = request.Correo.Trim().ToLower(),
                Celular = request.Celular.Trim(),
                Contrasena = passwordHash,
                Activo = true, // Creado activo por regla de negocio
                Borrado = false,
                FechaRegistro = DateTime.UtcNow
            };

            _context.Clientes.Add(nuevoCliente);
            await _context.SaveChangesAsync();

            string token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{nuevoCliente.ClienteId}:{nuevoCliente.Correo}:{DateTime.UtcNow.Ticks}"));

            return new LoginResponseDto
            {
                Exito = true,
                Mensaje = "Cliente registrado con éxito.",
                Token = token,
                Cliente = new ClienteResponseDto
                {
                    ClienteId = nuevoCliente.ClienteId,
                    TipoDocumento = nuevoCliente.TipoDocumento,
                    NumeroDocumento = nuevoCliente.NumeroDocumento,
                    Nombres = nuevoCliente.Nombres,
                    Apellidos = nuevoCliente.Apellidos,
                    Correo = nuevoCliente.Correo,
                    Celular = nuevoCliente.Celular,
                    Activo = nuevoCliente.Activo,
                    FechaRegistro = nuevoCliente.FechaRegistro
                }
            };
        }

        /// <summary>
        /// Servicio 14: Lista las direcciones guardadas del cliente ordenadas priorizando la principal.
        /// </summary>
        /// <param name="clienteId">Identificador del cliente.</param>
        /// <returns>Lista de direcciones activas.</returns>
        public async Task<IEnumerable<DireccionDto>> ListarDireccionesAsync(int clienteId)
        {
            var direcciones = await _context.Direcciones
                .Include(d => d.Zona)
                .Where(d => d.ClienteId == clienteId && !d.Borrado)
                .OrderByDescending(d => d.Principal)
                .ThenByDescending(d => d.FechaCreacion)
                .Select(d => new DireccionDto
                {
                    DireccionId = d.DireccionId,
                    ClienteId = d.ClienteId,
                    ZonaId = d.ZonaId,
                    ZonaNombre = d.Zona != null ? d.Zona.Nombre : null,
                    Alias = d.Alias,
                    Distrito = d.Distrito,
                    DireccionTexto = d.DireccionTexto,
                    Numero = d.Numero,
                    DepartamentoInterior = d.DepartamentoInterior,
                    Referencia = d.Referencia,
                    Latitud = d.Latitud,
                    Longitud = d.Longitud,
                    Principal = d.Principal
                })
                .ToListAsync();

            return direcciones;
        }

        /// <summary>
        /// Agrega una dirección a la libreta del cliente y actualiza el indicador de dirección principal.
        /// </summary>
        /// <param name="clienteId">Identificador del cliente.</param>
        /// <param name="request">Datos de la nueva dirección.</param>
        /// <returns>Dirección creada y vinculada.</returns>
        public async Task<DireccionDto?> CrearDireccionAsync(int clienteId, CrearDireccionDto request)
        {
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null || cliente.Borrado) return null;

            // Asociar zona si coincide con el distrito
            var zona = await _context.ZonasDespacho
                .FirstOrDefaultAsync(z => z.Distrito.ToLower() == request.Distrito.Trim().ToLower() && z.Activo);

            if (request.Principal)
            {
                var principalesPrevias = await _context.Direcciones
                    .Where(d => d.ClienteId == clienteId && d.Principal)
                    .ToListAsync();

                foreach (var dir in principalesPrevias)
                {
                    dir.Principal = false;
                }
            }

            var nuevaDireccion = new Direccion
            {
                ClienteId = clienteId,
                ZonaId = zona?.ZonaId,
                Alias = request.Alias.Trim(),
                Distrito = request.Distrito.Trim(),
                DireccionTexto = request.DireccionTexto.Trim(),
                Numero = request.Numero?.Trim(),
                DepartamentoInterior = request.DepartamentoInterior?.Trim(),
                Referencia = request.Referencia?.Trim(),
                Latitud = request.Latitud,
                Longitud = request.Longitud,
                Principal = request.Principal,
                Borrado = false,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Direcciones.Add(nuevaDireccion);
            await _context.SaveChangesAsync();

            return new DireccionDto
            {
                DireccionId = nuevaDireccion.DireccionId,
                ClienteId = nuevaDireccion.ClienteId,
                ZonaId = nuevaDireccion.ZonaId,
                ZonaNombre = zona?.Nombre,
                Alias = nuevaDireccion.Alias,
                Distrito = nuevaDireccion.Distrito,
                DireccionTexto = nuevaDireccion.DireccionTexto,
                Numero = nuevaDireccion.Numero,
                DepartamentoInterior = nuevaDireccion.DepartamentoInterior,
                Referencia = nuevaDireccion.Referencia,
                Latitud = nuevaDireccion.Latitud,
                Longitud = nuevaDireccion.Longitud,
                Principal = nuevaDireccion.Principal
            };
        }
    }
}
