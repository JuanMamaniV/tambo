namespace WebApi.DTOs
{
    // Servicio 1: Listar Categorías Principales
    public class CategoriaDto
    {
        public int CategoriaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? IconoUrl { get; set; }
        public string? ImagenUrl { get; set; }
        public int Orden { get; set; }
    }

    // Servicio 2: Listar Subcategorías
    public class SubcategoriaDto
    {
        public int CategoriaId { get; set; }
        public int CategoriaPadreId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? IconoUrl { get; set; }
        public string? ImagenUrl { get; set; }
        public int Orden { get; set; }
    }

    // Servicio 6: Buscar Productos por Término (Buscador Predictivo)
    public class ProductoBuscadorDto
    {
        public int ProductoId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public decimal Precio { get; set; }
        public decimal? PrecioOferta { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
    }

    // Servicio 7: Listar Productos por Categoría o Subcategoría
    public class ProductoCatalogoDto
    {
        public int ProductoId { get; set; }
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty;
        public string MarcaNombre { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? ImagenUrl { get; set; }
        public decimal Precio { get; set; }
        public decimal? PrecioOferta { get; set; }
        public decimal? DescuentoPorcentaje { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public bool TienePromocion { get; set; }
        public string? NombrePromocion { get; set; }
    }

    // Servicio 9: Consultar Detalle del Producto
    public class DetalleProductoResponseDto
    {
        public int ProductoId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string MarcaNombre { get; set; } = string.Empty;
        public string CategoriaNombre { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal? PrecioOferta { get; set; }
        public int CantidadDeseada { get; set; }
        public decimal PrecioTotalCalculado { get; set; }
        
        // Reglas de negocio del servicio 9:
        public int? TiendaId { get; set; }
        public int StockDisponible { get; set; }
        public bool SuperaStock { get; set; }
        public bool EnPromocion { get; set; }
        public int? MaxPorCliente { get; set; }
        public bool SuperaMaxPorCliente { get; set; }
        public bool PuedeAgregar { get; set; }
        public string MensajeValidacion { get; set; } = string.Empty;
    }
}
