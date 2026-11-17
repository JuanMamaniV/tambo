using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Services;

namespace WebApi.Controllers
{
    /// <summary>
    /// Servicio SOA: Checkout, Delivery y Pagos (f. Checkout y pago)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CheckoutController : ControllerBase
    {
        private readonly ICheckoutService _checkoutService;

        /// <summary>
        /// Constructor del controlador de checkout.
        /// </summary>
        /// <param name="checkoutService">Servicio de operaciones de compra, pagos y logística.</param>
        public CheckoutController(ICheckoutService checkoutService)
        {
            _checkoutService = checkoutService;
        }

        /// <summary>
        /// Servicio 4: Validar Cobertura por Dirección / Geolocalización
        /// </summary>
        /// <param name="request">Dirección de entrega, distrito y coordenadas.</param>
        /// <returns>Confirmación de cobertura, tienda asignada y costo de despacho.</returns>
        /// <remarks>
        /// Regla de negocio: Solo se puede realizar el delivery si la dirección se encuentra dentro de la zona de cobertura de una tienda activa con delivery_activo = 1.
        /// </remarks>
        [HttpPost("cobertura")]
        [ProducesResponseType(typeof(CoberturaResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidarCobertura([FromBody] ValidarCoberturaRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var resultado = await _checkoutService.ValidarCoberturaAsync(request);
            return Ok(resultado);
        }

        /// <summary>
        /// Servicio 5: Listar Tiendas para Retiro en Tienda
        /// </summary>
        /// <param name="busqueda">Filtro opcional por nombre o distrito de la tienda.</param>
        /// <returns>Lista de tiendas habilitadas para retiro en local.</returns>
        /// <remarks>
        /// Regla de negocio: Para retirar el pedido se debe seleccionar una tienda habilitada (activa, no borrada y retiro_activo = 1).
        /// </remarks>
        [HttpGet("tiendas-retiro")]
        [ProducesResponseType(typeof(IEnumerable<TiendaRetiroDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarTiendasRetiro([FromQuery] string? busqueda = null)
        {
            var tiendas = await _checkoutService.ListarTiendasRetiroAsync(busqueda);
            return Ok(tiendas);
        }

        /// <summary>
        /// Servicio 15: Seleccionar Tipo de Comprobante de Pago
        /// </summary>
        /// <returns>Catálogo de comprobantes de pago (Boleta / Factura).</returns>
        /// <remarks>
        /// Lista los comprobantes habilitados (Boleta de Venta / Factura Electrónica).
        /// </remarks>
        [HttpGet("tipos-comprobante")]
        [ProducesResponseType(typeof(IEnumerable<TipoComprobanteDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarTiposComprobante()
        {
            var tipos = await _checkoutService.ListarTiposComprobanteAsync();
            return Ok(tipos);
        }

        /// <summary>
        /// Servicio 16: Listar Métodos de Pago Disponibles
        /// </summary>
        /// <param name="request">Tienda física asignada y productos a validar.</param>
        /// <returns>Métodos de pago habilitados o error 400 si falta stock.</returns>
        /// <remarks>
        /// Regla de negocio: Solo se listan métodos no borrados. Si algún producto no tiene stock suficiente en la tienda asignada, no se puede continuar con el pago.
        /// </remarks>
        [HttpPost("metodos-pago")]
        [ProducesResponseType(typeof(ConsultarMetodosPagoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ConsultarMetodosPagoResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConsultarMetodosPago([FromBody] ConsultarMetodosPagoRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var resultado = await _checkoutService.ConsultarMetodosPagoDisponiblesAsync(request);
            if (!resultado.StockDisponibleValido)
            {
                return BadRequest(resultado);
            }

            return Ok(resultado);
        }

        /// <summary>
        /// Servicio 3: Seleccionar Modalidad de Entrega y Generar Pedido de Checkout
        /// </summary>
        /// <param name="request">Parámetros del pedido: cliente, tienda, modalidad, dirección, comprobante y productos.</param>
        /// <returns>Pedido generado con cálculo de subtotal, descuento por cupón y costo de envío.</returns>
        /// <remarks>
        /// Regla de negocio: Determina tienda, valida stock disponible, calcula envío según zona y genera la orden en estado inicial.
        /// </remarks>
        [HttpPost("crear-pedido")]
        [ProducesResponseType(typeof(PedidoCreadoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PedidoCreadoResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CrearPedido([FromBody] CrearPedidoCheckoutDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var resultado = await _checkoutService.CrearPedidoCheckoutAsync(request);
            if (!resultado.Exito)
            {
                return BadRequest(resultado);
            }

            return Ok(resultado);
        }

        /// <summary>
        /// Servicio 17: Procesar Pago con Tarjeta
        /// </summary>
        /// <param name="request">Datos de la tarjeta bancaria, titular y código del pedido.</param>
        /// <returns>Confirmación de pago aprobado o mensaje de rechazo bancario.</returns>
        /// <remarks>
        /// Regla de negocio: Datos de tarjeta y titular obligatorios. El pedido no debe tener ya un pago aprobado. El monto cobrado es el monto_total.
        /// </remarks>
        [HttpPost("pagar-tarjeta")]
        [ProducesResponseType(typeof(PagoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PagoResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PagarConTarjeta([FromBody] ProcesarPagoTarjetaDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var resultado = await _checkoutService.ProcesarPagoTarjetaAsync(request);
            if (!resultado.Aprobado)
            {
                return BadRequest(resultado);
            }

            return Ok(resultado);
        }

        /// <summary>
        /// Servicio 18: Consultar Detalle de Orden en Pasarela
        /// </summary>
        /// <param name="pedidoId">Identificador único del pedido.</param>
        /// <returns>Resumen de orden y estado de pago en pasarela.</returns>
        /// <remarks>
        /// Regla de negocio: Resumen del pedido, monto a pagar y estado del intento de pago. Si fue cancelado/rechazado, permite seleccionar otro medio.
        /// </remarks>
        [HttpGet("orden/{pedidoId:int}")]
        [ProducesResponseType(typeof(ResumenOrdenPasarelaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConsultarDetalleOrdenPasarela([FromRoute] int pedidoId)
        {
            var orden = await _checkoutService.ConsultarDetalleOrdenPasarelaAsync(pedidoId);
            if (orden == null)
            {
                return NotFound(new { mensaje = "Pedido no encontrado." });
            }

            return Ok(orden);
        }

        /// <summary>
        /// Servicio 19: Seguimiento del Pedido (Mis Pedidos)
        /// </summary>
        /// <param name="numeroPedido">Código alfanumérico del pedido.</param>
        /// <returns>Información de seguimiento cronológico y contacto del repartidor.</returns>
        /// <remarks>
        /// Regla de negocio: Muestra estado actual, historial ordenado según secuencia Confirmado -> Preparando -> En camino -> Entregado, y repartidor asignado.
        /// </remarks>
        [HttpGet("seguimiento/{numeroPedido}")]
        [ProducesResponseType(typeof(SeguimientoPedidoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SeguimientoPedido([FromRoute] string numeroPedido)
        {
            var seguimiento = await _checkoutService.ObtenerSeguimientoPedidoAsync(numeroPedido);
            if (seguimiento == null)
            {
                return NotFound(new { mensaje = $"No se encontró ningún pedido con el número '{numeroPedido}'." });
            }

            return Ok(seguimiento);
        }
    }
}
