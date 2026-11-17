using WebApi.DTOs;

namespace WebApi.Services
{
    /// <summary>
    /// Contrato del servicio de checkout, validación logística, inventario y pasarela de pago.
    /// </summary>
    public interface ICheckoutService
    {
        /// <summary>
        /// Servicio 4: Valida la cobertura de delivery por distrito o geolocalización y asigna la tienda física correspondiente.
        /// </summary>
        /// <param name="request">Distrito, dirección y coordenadas opcionales.</param>
        /// <returns>Estado de cobertura, tienda física asignada, costo y tiempo estimado.</returns>
        Task<CoberturaResponseDto> ValidarCoberturaAsync(ValidarCoberturaRequestDto request);

        /// <summary>
        /// Servicio 5: Lista las tiendas habilitadas para retiro en tienda física.
        /// </summary>
        /// <param name="busqueda">Término opcional de búsqueda por nombre o distrito.</param>
        /// <returns>Colección de tiendas con retiro activo.</returns>
        Task<IEnumerable<TiendaRetiroDto>> ListarTiendasRetiroAsync(string? busqueda = null);

        /// <summary>
        /// Servicio 15: Lista los tipos de comprobantes fiscales habilitados (Boleta / Factura).
        /// </summary>
        /// <returns>Colección de comprobantes tributarios.</returns>
        Task<IEnumerable<TipoComprobanteDto>> ListarTiposComprobanteAsync();

        /// <summary>
        /// Servicio 16: Lista los métodos de pago habilitados tras verificar stock en la tienda asignada.
        /// </summary>
        /// <param name="request">Tienda asignada y lista de items del pedido para validación de inventario.</param>
        /// <returns>Métodos de pago disponibles o listado de errores de stock insuficiente.</returns>
        Task<ConsultarMetodosPagoResponseDto> ConsultarMetodosPagoDisponiblesAsync(ConsultarMetodosPagoRequestDto request);

        /// <summary>
        /// Servicio 3: Genera la orden de pedido en estado inicial, reserva el stock y aplica cupones.
        /// </summary>
        /// <param name="request">Detalle completo de la orden, modalidad, dirección/tienda y comprobante.</param>
        /// <returns>Información del pedido generado y totales calculados.</returns>
        Task<PedidoCreadoResponseDto> CrearPedidoCheckoutAsync(CrearPedidoCheckoutDto request);

        /// <summary>
        /// Servicio 17: Procesa el cobro con tarjeta mediante pasarela de pagos virtual.
        /// </summary>
        /// <param name="request">Datos del titular y tarjeta de crédito/débito.</param>
        /// <returns>Resultado de autorización bancaria y código de transacción.</returns>
        Task<PagoResponseDto> ProcesarPagoTarjetaAsync(ProcesarPagoTarjetaDto request);

        /// <summary>
        /// Servicio 18: Consulta el estado de pago y resumen de una orden en la pasarela.
        /// </summary>
        /// <param name="pedidoId">Identificador único del pedido.</param>
        /// <returns>Resumen detallado de la orden o null si no existe.</returns>
        Task<ResumenOrdenPasarelaDto?> ConsultarDetalleOrdenPasarelaAsync(int pedidoId);

        /// <summary>
        /// Servicio 19: Consulta el ciclo de vida y seguimiento en tiempo real del pedido.
        /// </summary>
        /// <param name="numeroPedido">Código alfanumérico del pedido (ej. TMB-202611-0001).</param>
        /// <returns>Estado actual, historial de hitos y datos de repartidor.</returns>
        Task<SeguimientoPedidoDto?> ObtenerSeguimientoPedidoAsync(string numeroPedido);
    }
}
