using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs
{
    // Servicio 10: Gestión de Carrito de Compras
    public class GestionarCarritoRequestDto
    {
        [Required(ErrorMessage = "El cliente es obligatorio.")]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "El producto es obligatorio.")]
        public int ProductoId { get; set; }

        public int Cantidad { get; set; } = 1;

        [Required(ErrorMessage = "La acción es obligatoria: AGREGAR, EDITAR o ELIMINAR.")]
        [RegularExpression("^(AGREGAR|EDITAR|ELIMINAR)$", ErrorMessage = "Acción debe ser 'AGREGAR', 'EDITAR' o 'ELIMINAR'.")]
        public string Accion { get; set; } = "AGREGAR";

        // Tienda para validar stock opcionalmente durante la gestión del carrito
        public int? TiendaId { get; set; }

        // Datos para validar la regla de negocio: "Para continuar con la compra es obligatorio ingresar una dirección de entrega o seleccionar un local de retiro"
        public int? DireccionEntregaId { get; set; }
        public int? TiendaRetiroId { get; set; }
    }

    public class CarritoItemDto
    {
        public int DetalleCarritoId { get; set; }
        public int ProductoId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
        public decimal SubtotalItem { get; set; }
        public int? StockDisponibleEnTienda { get; set; }
        public bool StockSuficiente { get; set; } = true;
    }

    public class CarritoResponseDto
    {
        public int CarritoId { get; set; }
        public int ClienteId { get; set; }
        public int TotalItems { get; set; }
        public decimal Subtotal { get; set; }
        public List<CarritoItemDto> Items { get; set; } = new();

        // Regla de Negocio Servicio 10:
        public bool PuedeContinuarCompra { get; set; }
        public string MensajeContinuarCompra { get; set; } = string.Empty;
    }
}
