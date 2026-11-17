using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Services;

namespace WebApi.Controllers
{
    /// <summary>
    /// Servicio SOA: Gestión de Clientes (a. Registro y autenticación)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Servicio 12: Iniciar Sesión de Usuario
        /// </summary>
        /// <param name="request">Credenciales de inicio de sesión (correo electrónico y contraseña).</param>
        /// <returns>Datos del cliente autenticado y token de sesión, o 401 si las credenciales son inválidas.</returns>
        /// <remarks>
        /// Regla de negocio: Obligatorio correo y contraseña. Solo clientes activos (cliente.activo = 1) pueden iniciar sesión.
        /// </remarks>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new LoginResponseDto
                {
                    Exito = false,
                    Mensaje = "Datos de entrada no válidos."
                });
            }

            var resultado = await _authService.LoginAsync(request);
            if (!resultado.Exito)
            {
                return Unauthorized(resultado);
            }

            return Ok(resultado);
        }

        /// <summary>
        /// Servicio 13: Registro de Nuevo Usuario
        /// </summary>
        /// <param name="request">Datos del nuevo cliente (documento, nombres, apellidos, correo, celular, contraseña).</param>
        /// <returns>Cliente creado y sesión iniciada, o 400 si el correo o documento ya existen.</returns>
        /// <remarks>
        /// Regla de negocio: Correo y número de documento no deben existir previamente. El cliente se crea activo con fecha del día.
        /// </remarks>
        [HttpPost("registro")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Registro([FromBody] RegistroClienteDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new LoginResponseDto
                {
                    Exito = false,
                    Mensaje = "Verifique los datos obligatorios del formulario de registro."
                });
            }

            var resultado = await _authService.RegistroAsync(request);
            if (!resultado.Exito)
            {
                return BadRequest(resultado);
            }

            return Ok(resultado);
        }

        /// <summary>
        /// Servicio 14: Listar Direcciones Guardadas del Usuario
        /// </summary>
        /// <param name="clienteId">Identificador del cliente.</param>
        /// <returns>Listado de direcciones activas registradas por el cliente.</returns>
        /// <remarks>
        /// Regla de negocio: El usuario consulta las direcciones registradas en su cuenta con zona de despacho asignada.
        /// </remarks>
        [HttpGet("clientes/{clienteId:int}/direcciones")]
        [ProducesResponseType(typeof(IEnumerable<DireccionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarDirecciones([FromRoute] int clienteId)
        {
            var direcciones = await _authService.ListarDireccionesAsync(clienteId);
            return Ok(direcciones);
        }

        /// <summary>
        /// Registrar nueva dirección para el usuario autenticado
        /// </summary>
        /// <param name="clienteId">Identificador del cliente.</param>
        /// <param name="request">Datos de la nueva dirección física.</param>
        /// <returns>Dirección creada y vinculada.</returns>
        [HttpPost("clientes/{clienteId:int}/direcciones")]
        [ProducesResponseType(typeof(DireccionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GuardarDireccion([FromRoute] int clienteId, [FromBody] CrearDireccionDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var direccionCreada = await _authService.CrearDireccionAsync(clienteId, request);
            if (direccionCreada == null)
            {
                return NotFound(new { mensaje = "Cliente no encontrado." });
            }

            return CreatedAtAction(nameof(ListarDirecciones), new { clienteId }, direccionCreada);
        }
    }
}
