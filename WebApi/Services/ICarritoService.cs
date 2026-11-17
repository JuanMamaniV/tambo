using WebApi.DTOs;

namespace WebApi.Services
{
    /// <summary>
    /// Contrato del servicio de gestión de carrito de compras y verificación de condiciones para checkout.
    /// </summary>
    public interface ICarritoService
    {
        /// <summary>
        /// Obtiene el carrito activo del cliente calculando subtotales y disponibilidad de stock en tienda.
        /// </summary>
        /// <param name="clienteId">Identificador del cliente.</param>
        /// <param name="tiendaId">Identificador de la tienda para consultar stock físico.</param>
        /// <param name="direccionId">Identificador de la dirección de entrega elegida.</param>
        /// <param name="tiendaRetiroId">Identificador del local de retiro seleccionado.</param>
        /// <returns>Estado actual del carrito con subtotales y validación de continuación de compra.</returns>
        Task<CarritoResponseDto> ObtenerCarritoAsync(int clienteId, int? tiendaId = null, int? direccionId = null, int? tiendaRetiroId = null);

        /// <summary>
        /// Servicio 10: Permite agregar, editar cantidad o eliminar productos del carrito activo.
        /// </summary>
        /// <param name="request">Parámetros de acción (AGREGAR, EDITAR, ELIMINAR), producto, cantidad y tienda/dirección.</param>
        /// <returns>Carrito actualizado y subtotal recalculado.</returns>
        Task<CarritoResponseDto> GestionarCarritoAsync(GestionarCarritoRequestDto request);
    }
}
