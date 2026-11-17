using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs
{
    // Servicio 4: Validar Cobertura por Dirección / Geolocalización
    public class ValidarCoberturaRequestDto
    {
        [Required(ErrorMessage = "El distrito es obligatorio.")]
        public string Distrito { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección de entrega es obligatoria.")]
        public string DireccionTexto { get; set; } = string.Empty;

        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
    }

    public class CoberturaResponseDto
    {
        public bool TieneCobertura { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public int? TiendaAsignadaId { get; set; }
        public string? NombreTienda { get; set; }
        public string? DireccionTienda { get; set; }
        public int? ZonaId { get; set; }
        public string? ZonaNombre { get; set; }
        public decimal CostoDelivery { get; set; }
        public int TiempoEstimadoMin { get; set; }
    }

    // Servicio 5: Listar Tiendas para Retiro en Tienda
    public class TiendaRetiroDto
    {
        public int TiendaId { get; set; }
        public string CodigoTienda { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Distrito { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string HorarioAtencion { get; set; } = string.Empty;
        public bool RetiroActivo { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
    }

    // Servicio 15: Seleccionar Tipo de Comprobante de Pago
    public class TipoComprobanteDto
    {
        public int TipoComprobanteId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public bool RequiereRuc { get; set; }
    }

    // Servicio 16: Listar Métodos de Pago Disponibles
    public class ValidarStockItemDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class ConsultarMetodosPagoRequestDto
    {
        [Required]
        public int TiendaId { get; set; }

        public int? PedidoId { get; set; }

        // Si se valida antes de generar el pedido:
        public List<ValidarStockItemDto> Items { get; set; } = new();
    }

    public class MetodoPagoDto
    {
        public int MetodoPagoId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? IconoUrl { get; set; }
        public bool EsOnline { get; set; }
    }

    public class ConsultarMetodosPagoResponseDto
    {
        public bool StockDisponibleValido { get; set; }
        public string? MensajeValidacionStock { get; set; }
        public List<string> ErroresStock { get; set; } = new();
        public List<MetodoPagoDto> MetodosPago { get; set; } = new();
    }

    // Servicio 3 y Crear Pedido (Checkout previo a pasarela)
    public class ItemPedidoRequestDto
    {
        [Required]
        public int ProductoId { get; set; }

        [Range(1, 100, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Cantidad { get; set; }
    }

    public class CrearPedidoCheckoutDto
    {
        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int TiendaId { get; set; }

        [Required]
        [RegularExpression("^(DELIVERY|RETIRO_TIENDA)$", ErrorMessage = "Tipo de entrega debe ser 'DELIVERY' o 'RETIRO_TIENDA'.")]
        public string TipoEntrega { get; set; } = "DELIVERY";

        public int? DireccionId { get; set; } // Requerido si es DELIVERY

        [Required]
        public int TipoComprobanteId { get; set; }

        [Required]
        public string DocComprobante { get; set; } = string.Empty; // DNI o RUC

        public string? RazonSocial { get; set; } // Requerido si comprobante es FACTURA

        public string? DireccionFiscal { get; set; } // Requerido si comprobante es FACTURA

        public string? CuponCodigo { get; set; }

        public string? Notas { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "El pedido debe contener al menos un producto.")]
        public List<ItemPedidoRequestDto> Items { get; set; } = new();
    }

    public class PedidoCreadoResponseDto
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public int? PedidoId { get; set; }
        public string? NumeroPedido { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal CostoEnvio { get; set; }
        public decimal MontoTotal { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    // Servicio 17: Procesar Pago con Tarjeta
    public class ProcesarPagoTarjetaDto
    {
        [Required(ErrorMessage = "El ID del pedido es obligatorio.")]
        public int PedidoId { get; set; }

        [Required(ErrorMessage = "El número de tarjeta es obligatorio.")]
        [CreditCard(ErrorMessage = "Número de tarjeta inválido.")]
        public string NumeroTarjeta { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria (MM/AA o MM/AAAA).")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{4}|[0-9]{2})$", ErrorMessage = "Formato de vencimiento debe ser MM/AA.")]
        public string FechaVencimiento { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código CVV es obligatorio.")]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "El CVV debe tener 3 o 4 dígitos.")]
        public string Cvv { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del titular es obligatorio.")]
        public string TitularNombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido del titular es obligatorio.")]
        public string TitularApellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo del titular es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string TitularCorreo { get; set; } = string.Empty;
    }

    public class PagoResponseDto
    {
        public bool Aprobado { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string? CodigoTransaccion { get; set; }
        public decimal MontoCobrado { get; set; }
        public string? TarjetaMarca { get; set; }
        public string? TarjetaUltimosDigitos { get; set; }
        public DateTime FechaPago { get; set; }
        public string? NuevoEstadoPedido { get; set; }
    }

    // Servicio 18: Consultar Detalle de Orden en Pasarela
    public class ResumenOrdenPasarelaDto
    {
        public int PedidoId { get; set; }
        public string NumeroPedido { get; set; } = string.Empty;
        public DateTime FechaPedido { get; set; }
        public string TiendaNombre { get; set; } = string.Empty;
        public string TipoEntrega { get; set; } = string.Empty;
        public string EstadoPedido { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal CostoEnvio { get; set; }
        public decimal MontoTotal { get; set; }
        public string? EstadoUltimoPago { get; set; }
        public string? CodigoTransaccionPago { get; set; }
        public List<DetalleItemResumenDto> Items { get; set; } = new();
    }

    public class DetalleItemResumenDto
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    // Servicio 19: Seguimiento del Pedido
    public class SeguimientoPedidoDto
    {
        public string NumeroPedido { get; set; } = string.Empty;
        public string EstadoActual { get; set; } = string.Empty;
        public string? TiempoEstimadoLlegada { get; set; }
        public RepartidorContactoDto? Repartidor { get; set; }
        public decimal TotalPagado { get; set; }
        public string MedioPago { get; set; } = string.Empty;
        public List<DetalleItemResumenDto> Productos { get; set; } = new();
        public List<HistorialEstadoDto> HistorialEstados { get; set; } = new();
    }

    public class RepartidorContactoDto
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public string TipoVehiculo { get; set; } = string.Empty;
    }

    public class HistorialEstadoDto
    {
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public string? Comentario { get; set; }
    }
}
