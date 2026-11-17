using WebApi.DTOs;

namespace WebApi.Services
{
    /// <summary>
    /// Contrato del servicio de catálogo de productos, categorías y navegación.
    /// </summary>
    public interface ICatalogoService
    {
        /// <summary>
        /// Servicio 1: Lista las categorías principales habilitadas para el menú de navegación superior.
        /// </summary>
        /// <returns>Colección de categorías raíz ordenadas.</returns>
        Task<IEnumerable<CategoriaDto>> ListarCategoriasAsync();

        /// <summary>
        /// Servicio 2: Lista las subcategorías que pertenecen a una categoría padre elegida.
        /// </summary>
        /// <param name="categoriaId">Identificador de la categoría padre.</param>
        /// <returns>Colección de subcategorías dependientes.</returns>
        Task<IEnumerable<SubcategoriaDto>> ListarSubcategoriasAsync(int categoriaId);

        /// <summary>
        /// Servicio 6: Realiza una búsqueda predictiva de productos por nombre, marca o SKU.
        /// </summary>
        /// <param name="termino">Término de búsqueda ingresado por el usuario.</param>
        /// <returns>Productos coincidentes con nombre, imagen y precio.</returns>
        Task<IEnumerable<ProductoBuscadorDto>> BuscarProductosPredictivoAsync(string termino);

        /// <summary>
        /// Servicio 7: Lista los productos de una categoría o subcategoría con sus precios y promociones.
        /// </summary>
        /// <param name="categoriaId">Identificador de la categoría o subcategoría.</param>
        /// <returns>Productos asociados a la sección solicitada.</returns>
        Task<IEnumerable<ProductoCatalogoDto>> ListarProductosPorCategoriaAsync(int categoriaId);

        /// <summary>
        /// Servicio 9: Consulta el detalle completo de un producto y valida el stock en tienda física y límite por cliente en promoción.
        /// </summary>
        /// <param name="productoId">Identificador único del producto.</param>
        /// <param name="cantidad">Cantidad deseada por el cliente.</param>
        /// <param name="tiendaId">Identificador opcional de la tienda física para validar inventario.</param>
        /// <returns>Detalle del producto y resultado de validaciones de stock y promociones.</returns>
        Task<DetalleProductoResponseDto?> ConsultarDetalleProductoAsync(int productoId, int cantidad, int? tiendaId);
    }
}
