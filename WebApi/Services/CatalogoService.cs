using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.DTOs;

namespace WebApi.Services
{
    /// <summary>
    /// Implementación de la lógica de negocio para Catálogo de productos, categorías e inventario de detalle.
    /// </summary>
    public class CatalogoService : ICatalogoService
    {
        private readonly TamboDbContext _context;

        /// <summary>
        /// Constructor con inyección del contexto de datos TamboDbContext.
        /// </summary>
        /// <param name="context">Contexto de base de datos.</param>
        public CatalogoService(TamboDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Servicio 1: Lista las categorías principales disponibles en el menú superior (categoria_padre_id IS NULL).
        /// </summary>
        /// <returns>Categorías raíz ordenadas.</returns>
        public async Task<IEnumerable<CategoriaDto>> ListarCategoriasAsync()
        {
            return await _context.Categorias
                .Where(c => c.CategoriaPadreId == null && !c.Borrado && c.Activo)
                .OrderBy(c => c.Orden)
                .Select(c => new CategoriaDto
                {
                    CategoriaId = c.CategoriaId,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    IconoUrl = c.IconoUrl,
                    ImagenUrl = c.ImagenUrl,
                    Orden = c.Orden
                })
                .ToListAsync();
        }

        /// <summary>
        /// Servicio 2: Lista las subcategorías que pertenecen a la categoría elegida.
        /// </summary>
        /// <param name="categoriaId">Identificador de la categoría padre.</param>
        /// <returns>Lista de subcategorías activas.</returns>
        public async Task<IEnumerable<SubcategoriaDto>> ListarSubcategoriasAsync(int categoriaId)
        {
            return await _context.Categorias
                .Where(c => c.CategoriaPadreId == categoriaId && !c.Borrado && c.Activo)
                .OrderBy(c => c.Orden)
                .Select(c => new SubcategoriaDto
                {
                    CategoriaId = c.CategoriaId,
                    CategoriaPadreId = c.CategoriaPadreId!.Value,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    IconoUrl = c.IconoUrl,
                    ImagenUrl = c.ImagenUrl,
                    Orden = c.Orden
                })
                .ToListAsync();
        }

        /// <summary>
        /// Servicio 6: Busca productos en tiempo real por término de texto (nombre, marca o SKU) sin productos borrados.
        /// </summary>
        /// <param name="termino">Palabra clave de búsqueda.</param>
        /// <returns>Primeros 15 resultados coincidentes.</returns>
        public async Task<IEnumerable<ProductoBuscadorDto>> BuscarProductosPredictivoAsync(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
            {
                return Enumerable.Empty<ProductoBuscadorDto>();
            }

            string q = termino.Trim().ToLower();

            return await _context.Productos
                .Include(p => p.Marca)
                .Where(p => !p.Borrado && p.Activo &&
                           (p.Nombre.ToLower().Contains(q) || (p.Marca != null && p.Marca.Nombre.ToLower().Contains(q)) || p.Sku.ToLower().Contains(q)))
                .OrderBy(p => p.Nombre)
                .Take(15)
                .Select(p => new ProductoBuscadorDto
                {
                    ProductoId = p.ProductoId,
                    Sku = p.Sku,
                    Nombre = p.Nombre,
                    Marca = p.Marca != null ? p.Marca.Nombre : string.Empty,
                    ImagenUrl = p.ImagenUrl,
                    Precio = p.Precio,
                    PrecioOferta = p.PrecioOferta,
                    UnidadMedida = p.UnidadMedida
                })
                .ToListAsync();
        }

        /// <summary>
        /// Servicio 7: Lista los productos de una categoría o subcategoría con sus precios y ofertas aplicadas.
        /// </summary>
        /// <param name="categoriaId">Identificador de la sección.</param>
        /// <returns>Colección de productos con detalles comerciales.</returns>
        public async Task<IEnumerable<ProductoCatalogoDto>> ListarProductosPorCategoriaAsync(int categoriaId)
        {
            // Soporta tanto si seleccionan una subcategoría directa o una categoría padre (incluyendo sus hijas)
            var categoriasIds = await _context.Categorias
                .Where(c => (c.CategoriaId == categoriaId || c.CategoriaPadreId == categoriaId) && !c.Borrado && c.Activo)
                .Select(c => c.CategoriaId)
                .ToListAsync();

            if (categoriasIds.Count == 0)
            {
                categoriasIds.Add(categoriaId);
            }

            var ahora = DateTime.UtcNow;

            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .Where(p => categoriasIds.Contains(p.CategoriaId) && !p.Borrado && p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            // Consultar promociones activas para asociar descuentos
            var promocionesActivas = await _context.PromocionesProductos
                .Include(pp => pp.Promocion)
                .Where(pp => pp.Promocion!.Activo && pp.Promocion.FechaInicio <= ahora && pp.Promocion.FechaFin >= ahora)
                .ToListAsync();

            var resultado = new List<ProductoCatalogoDto>();

            foreach (var p in productos)
            {
                var promo = promocionesActivas.FirstOrDefault(pp => pp.ProductoId == p.ProductoId)?.Promocion;

                decimal? descuentoPorcentaje = null;
                if (p.PrecioOferta.HasValue && p.Precio > 0)
                {
                    descuentoPorcentaje = Math.Round(((p.Precio - p.PrecioOferta.Value) / p.Precio) * 100m, 0);
                }
                else if (promo?.PorcentajeDescuento.HasValue == true)
                {
                    descuentoPorcentaje = promo.PorcentajeDescuento;
                }

                resultado.Add(new ProductoCatalogoDto
                {
                    ProductoId = p.ProductoId,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.Categoria?.Nombre ?? string.Empty,
                    MarcaNombre = p.Marca?.Nombre ?? string.Empty,
                    Sku = p.Sku,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    ImagenUrl = p.ImagenUrl,
                    Precio = p.Precio,
                    PrecioOferta = p.PrecioOferta ?? promo?.PrecioPromocional,
                    DescuentoPorcentaje = descuentoPorcentaje,
                    UnidadMedida = p.UnidadMedida,
                    TienePromocion = promo != null || p.PrecioOferta.HasValue,
                    NombrePromocion = promo?.Nombre
                });
            }

            return resultado;
        }

        /// <summary>
        /// Servicio 9: Obtiene la ficha técnica del producto y valida que la cantidad deseada no supere el stock disponible ni el tope de promoción por cliente.
        /// </summary>
        /// <param name="productoId">Identificador del producto.</param>
        /// <param name="cantidad">Cantidad de unidades requeridas.</param>
        /// <param name="tiendaId">Identificador de la tienda para consultar stock local.</param>
        /// <returns>Información de ficha de producto y estado de stock.</returns>
        public async Task<DetalleProductoResponseDto?> ConsultarDetalleProductoAsync(int productoId, int cantidad, int? tiendaId)
        {
            var p = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .FirstOrDefaultAsync(p => p.ProductoId == productoId && !p.Borrado && p.Activo);

            if (p == null) return null;

            int cantidadSolicitada = Math.Max(1, cantidad);
            decimal precioUnitarioEfectivo = p.PrecioOferta ?? p.Precio;
            decimal totalCalculado = precioUnitarioEfectivo * cantidadSolicitada;

            // Verificar si tiene promoción activa
            var ahora = DateTime.UtcNow;
            var promo = await _context.PromocionesProductos
                .Include(pp => pp.Promocion)
                .Where(pp => pp.ProductoId == productoId &&
                             pp.Promocion!.Activo &&
                             pp.Promocion.FechaInicio <= ahora &&
                             pp.Promocion.FechaFin >= ahora)
                .Select(pp => pp.Promocion)
                .FirstOrDefaultAsync();

            int? maxPorCliente = promo?.MaxPorCliente;
            bool superaMaxPorCliente = maxPorCliente.HasValue && cantidadSolicitada > maxPorCliente.Value;

            // Verificar inventario en la tienda seleccionada
            int stockDisponible = 0;
            bool superaStock = false;

            if (tiendaId.HasValue)
            {
                var inv = await _context.Inventarios
                    .FirstOrDefaultAsync(i => i.TiendaId == tiendaId.Value && i.ProductoId == productoId);

                stockDisponible = inv?.StockDisponible ?? 0;
                superaStock = cantidadSolicitada > stockDisponible;
            }

            bool puedeAgregar = (!tiendaId.HasValue || !superaStock) && !superaMaxPorCliente;

            string mensajeValidacion = "Producto disponible.";
            if (superaMaxPorCliente)
            {
                mensajeValidacion = $"La cantidad solicitada ({cantidadSolicitada}) supera el límite de {maxPorCliente} unidades por cliente para esta promoción.";
            }
            else if (tiendaId.HasValue && superaStock)
            {
                mensajeValidacion = $"Stock insuficiente en la tienda seleccionada (Solicitado: {cantidadSolicitada}, Disponible: {stockDisponible}).";
            }

            return new DetalleProductoResponseDto
            {
                ProductoId = p.ProductoId,
                Sku = p.Sku,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                MarcaNombre = p.Marca?.Nombre ?? string.Empty,
                CategoriaNombre = p.Categoria?.Nombre ?? string.Empty,
                ImagenUrl = p.ImagenUrl,
                PrecioUnitario = p.Precio,
                PrecioOferta = p.PrecioOferta,
                CantidadDeseada = cantidadSolicitada,
                PrecioTotalCalculado = totalCalculado,
                TiendaId = tiendaId,
                StockDisponible = stockDisponible,
                SuperaStock = superaStock,
                EnPromocion = promo != null,
                MaxPorCliente = maxPorCliente,
                SuperaMaxPorCliente = superaMaxPorCliente,
                PuedeAgregar = puedeAgregar,
                MensajeValidacion = mensajeValidacion
            };
        }
    }
}
