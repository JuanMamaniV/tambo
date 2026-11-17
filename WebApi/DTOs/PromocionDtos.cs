using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs
{
    // Servicio 8: Listar Promociones y Combos ("Tambo Packs")
    public class PromocionPackDto
    {
        public int PromocionId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string TipoPromocion { get; set; } = string.Empty;
        public decimal? PrecioPromocional { get; set; }
        public decimal? PorcentajeDescuento { get; set; }
        public int CantidadPack { get; set; }
        public int MaxPorCliente { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string? BannerTitulo { get; set; }
        public string? BannerImagenUrl { get; set; }
        public List<PromocionProductoItemDto> ProductosIncluidos { get; set; } = new();
    }

    public class PromocionProductoItemDto
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public decimal PrecioRegular { get; set; }
        public int Cantidad { get; set; }
    }

    // Servicio 11: Validar Cupón de Descuento
    public class ValidarCuponRequestDto
    {
        [Required(ErrorMessage = "El código de cupón es obligatorio.")]
        public string Codigo { get; set; } = string.Empty;

        [Range(0, 100000, ErrorMessage = "El subtotal debe ser positivo.")]
        public decimal SubtotalActual { get; set; }
    }

    public class ValidarCuponResponseDto
    {
        public bool EsValido { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public int? CuponId { get; set; }
        public string? Codigo { get; set; }
        public string? TipoDescuento { get; set; }
        public decimal ValorDescuento { get; set; }
        public decimal DescuentoCalculado { get; set; }
        public decimal SubtotalOriginal { get; set; }
        public decimal TotalConDescuento { get; set; }
    }
}
