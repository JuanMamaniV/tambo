using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Services;

namespace WebApi.Controllers
{
    /// <summary>
    /// Servicio SOA: Catálogo de Productos y Navegación
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CatalogoController : ControllerBase
    {
        private readonly ICatalogoService _catalogoService;

        /// <summary>
        /// Constructor del controlador de catálogo de productos.
        /// </summary>
        /// <param name="catalogoService">Servicio de catálogo y navegación.</param>
        public CatalogoController(ICatalogoService catalogoService)
        {
            _catalogoService = catalogoService;
        }

        /// <summary>
        /// Servicio 1: Listar Categorías Principales
        /// </summary>
        /// <returns>Lista de categorías principales activas ordenadas por visualización.</returns>
        /// <remarks>
        /// Regla de negocio: Solo se pueden seleccionar las categorías disponibles en el menú de navegación (categoria_padre_id IS NULL, borrado = 0, orden).
        /// </remarks>
        [HttpGet("categorias")]
        [ProducesResponseType(typeof(IEnumerable<CategoriaDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarCategorias()
        {
            var categorias = await _catalogoService.ListarCategoriasAsync();
            return Ok(categorias);
        }

        /// <summary>
        /// Servicio 2: Listar Subcategorías
        /// </summary>
        /// <param name="categoriaId">Identificador de la categoría padre seleccionada.</param>
        /// <returns>Lista de subcategorías vinculadas a la categoría elegida.</returns>
        /// <remarks>
        /// Regla de negocio: Las subcategorías mostradas deben pertenecer a la categoría seleccionada (categoria_padre_id = categoría elegida).
        /// </remarks>
        [HttpGet("categorias/{categoriaId:int}/subcategorias")]
        [ProducesResponseType(typeof(IEnumerable<SubcategoriaDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarSubcategorias([FromRoute] int categoriaId)
        {
            var subcategorias = await _catalogoService.ListarSubcategoriasAsync(categoriaId);
            return Ok(subcategorias);
        }

        /// <summary>
        /// Servicio 6: Buscar Productos por Término (Buscador Predictivo)
        /// </summary>
        /// <param name="termino">Texto a buscar en el nombre del producto o marca (ej. 'cerveza').</param>
        /// <returns>Listado predictivo de productos con imagen y precio.</returns>
        /// <remarks>
        /// Regla de negocio: Los resultados deben corresponder al término ingresado (búsqueda por nombre del producto o de la marca, sin productos borrados).
        /// </remarks>
        [HttpGet("productos/buscar")]
        [ProducesResponseType(typeof(IEnumerable<ProductoBuscadorDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BuscarPredictivo([FromQuery] string termino)
        {
            var resultados = await _catalogoService.BuscarProductosPredictivoAsync(termino);
            return Ok(resultados);
        }

        /// <summary>
        /// Servicio 7: Listar Productos por Categoría o Subcategoría
        /// </summary>
        /// <param name="categoriaId">Identificador de la sección o categoría a listar.</param>
        /// <returns>Productos pertenecientes a la sección con precios, ofertas y fotos.</returns>
        /// <remarks>
        /// Regla de negocio: Solo deben mostrarse los productos que pertenecen a la sección elegida con su imagen, nombre, precio y descuento cuando corresponda.
        /// </remarks>
        [HttpGet("productos/categoria/{categoriaId:int}")]
        [ProducesResponseType(typeof(IEnumerable<ProductoCatalogoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarPorCategoria([FromRoute] int categoriaId)
        {
            var productos = await _catalogoService.ListarProductosPorCategoriaAsync(categoriaId);
            return Ok(productos);
        }

        /// <summary>
        /// Servicio 9: Consultar Detalle del Producto
        /// </summary>
        /// <param name="productoId">Identificador del producto.</param>
        /// <param name="cantidad">Cantidad deseada por el cliente (por defecto 1).</param>
        /// <param name="tiendaId">Identificador de la tienda para consultar stock local disponible.</param>
        /// <returns>Ficha técnica del producto, total calculado y estado de validación de stock y promociones.</returns>
        /// <remarks>
        /// Regla de negocio: La cantidad no debe superar el stock disponible del producto (inventario.stock_disponible) ni, si está en promoción, el máximo por cliente (promocion.max_por_cliente).
        /// </remarks>
        [HttpGet("productos/{productoId:int}/detalle")]
        [ProducesResponseType(typeof(DetalleProductoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConsultarDetalle(
            [FromRoute] int productoId,
            [FromQuery] int cantidad = 1,
            [FromQuery] int? tiendaId = null)
        {
            var detalle = await _catalogoService.ConsultarDetalleProductoAsync(productoId, cantidad, tiendaId);
            if (detalle == null)
            {
                return NotFound(new { mensaje = $"El producto con ID {productoId} no fue encontrado o no está activo." });
            }

            return Ok(detalle);
        }
    }
}
