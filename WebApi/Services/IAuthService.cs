using WebApi.DTOs;

namespace WebApi.Services
{
    /// <summary>
    /// Contrato del servicio de autenticación y gestión de cuentas de clientes.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Servicio 12: Inicia sesión de un cliente validando credenciales y estado activo.
        /// </summary>
        /// <param name="request">Credenciales del cliente (correo y contraseña).</param>
        /// <returns>Resultado de autenticación con datos del cliente y token simulado.</returns>
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

        /// <summary>
        /// Servicio 13: Registra un nuevo cliente validando unicidad de correo y documento.
        /// </summary>
        /// <param name="request">Datos personales y credenciales del nuevo cliente.</param>
        /// <returns>Resultado del registro con los datos creados y sesión iniciada.</returns>
        Task<LoginResponseDto> RegistroAsync(RegistroClienteDto request);

        /// <summary>
        /// Servicio 14: Lista las direcciones activas guardadas por un cliente autenticado.
        /// </summary>
        /// <param name="clienteId">Identificador único del cliente.</param>
        /// <returns>Colección de direcciones con información de zona de despacho.</returns>
        Task<IEnumerable<DireccionDto>> ListarDireccionesAsync(int clienteId);

        /// <summary>
        /// Registra una nueva dirección para el cliente y gestiona la dirección principal.
        /// </summary>
        /// <param name="clienteId">Identificador del cliente propietario de la dirección.</param>
        /// <param name="request">Datos de la dirección física, distrito y coordenadas.</param>
        /// <returns>Dirección creada o null si el cliente no existe.</returns>
        Task<DireccionDto?> CrearDireccionAsync(int clienteId, CrearDireccionDto request);
    }
}
