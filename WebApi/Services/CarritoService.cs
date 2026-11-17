using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.DTOs;
using WebApi.Models;

namespace WebApi.Services
{
    /// <summary>
    /// Implementación de la lógica de negocio para la gestión de carrito de compras y verificación de pre-requisitos de compra.
    /// </summary>
    public class CarritoService : ICarritoService
    {
        private readonly TamboDbContext _context;

        /// <summary>
        /// Constructor con inyección del DbContext.
        /// </summary>
        /// <param name="context">Contexto de base de datos TamboDB.</param>
        public CarritoService(TamboDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene el carrito activo del usuario, totalizando items y calculando el subtotal general.
        /// </summary>
        /// <param name="clienteId">Identificador del cliente.</param>
        /// <param name="tiendaId">Tienda para verificar stock disponible de cada item.</param>
        /// <param name="direccionId">Dirección de entrega asignada si la modalidad es delivery.</param>
        /// <param name="tiendaRetiroId">Tienda asignada si la modalidad es retiro en tienda.</param>
        /// <returns>Estructura del carrito y estado de validación para proceder a checkout.</returns>
        public async Task<CarritoResponseDto> ObtenerCarritoAsync(int clienteId, int? tiendaId = null, int? direccionId = null, int? tiendaRetiroId = null)
        {
            var carrito = await ObtenerOCrearCarritoActivoAsync(clienteId);
            return await ConstruirCarritoResponseAsync(carrito, tiendaId, direccionId, tiendaRetiroId);
        }

        /// <summary>
        /// Servicio 10: Ejecuta operaciones de agregar, editar cantidad o remover productos en el carrito.
        /// Aplica la regla: "Para continuar con la compra es obligatorio ingresar una dirección de entrega o seleccionar un local de retiro".
        /// </summary>
        /// <param name="request">Parámetros de la acción y producto.</param>
        /// <returns>Carrito actualizado con subtotales recalculados.</returns>
        public async Task<CarritoResponseDto> GestionarCarritoAsync(GestionarCarritoRequestDto request)
        {
            var carrito = await ObtenerOCrearCarritoActivoAsync(request.ClienteId);

            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.ProductoId == request.ProductoId && !p.Borrado && p.Activo);

            if (producto == null)
            {
                throw new ArgumentException($"El producto con ID {request.ProductoId} no existe o no se encuentra activo.");
            }

            var detalle = await _context.DetallesCarrito
                .FirstOrDefaultAsync(d => d.CarritoId == carrito.CarritoId && d.ProductoId == request.ProductoId);

            decimal precioVenta = producto.PrecioOferta ?? producto.Precio;

            string accion = request.Accion.ToUpper().Trim();

            switch (accion)
            {
                case "AGREGAR":
                    int cantidadAAgregar = Math.Max(1, request.Cantidad);
                    if (detalle == null)
                    {
                        detalle = new DetalleCarrito
                        {
                            CarritoId = carrito.CarritoId,
                            ProductoId = request.ProductoId,
                            Cantidad = cantidadAAgregar,
                            PrecioUnitario = precioVenta
                        };
                        _context.DetallesCarrito.Add(detalle);
                    }
                    else
                    {
                        detalle.Cantidad += cantidadAAgregar;
                        detalle.PrecioUnitario = precioVenta; // Actualiza precio vigente
                    }
                    break;

                case "EDITAR":
                    if (detalle != null)
                    {
                        if (request.Cantidad <= 0)
                        {
                            _context.DetallesCarrito.Remove(detalle);
                        }
                        else
                        {
                            detalle.Cantidad = request.Cantidad;
                            detalle.PrecioUnitario = precioVenta;
                        }
                    }
                    break;

                case "ELIMINAR":
                    if (detalle != null)
                    {
                        _context.DetallesCarrito.Remove(detalle);
                    }
                    break;

                default:
                    throw new ArgumentException($"Acción '{request.Accion}' no reconocida. Utilice AGREGAR, EDITAR o ELIMINAR.");
            }

            carrito.FechaActualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await ConstruirCarritoResponseAsync(carrito, request.TiendaId, request.DireccionEntregaId, request.TiendaRetiroId);
        }

        private async Task<Carrito> ObtenerOCrearCarritoActivoAsync(int clienteId)
        {
            var carrito = await _context.Carritos
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(c => c.ClienteId == clienteId && c.Activo);

            if (carrito == null)
            {
                carrito = new Carrito
                {
                    ClienteId = clienteId,
                    FechaCreacion = DateTime.UtcNow,
                    FechaActualizacion = DateTime.UtcNow,
                    Activo = true
                };
                _context.Carritos.Add(carrito);
                await _context.SaveChangesAsync();
            }

            return carrito;
        }

        private async Task<CarritoResponseDto> ConstruirCarritoResponseAsync(Carrito carrito, int? tiendaId, int? direccionId, int? tiendaRetiroId)
        {
            var detalles = await _context.DetallesCarrito
                .Include(d => d.Producto)
                .Where(d => d.CarritoId == carrito.CarritoId)
                .ToListAsync();

            var itemsDto = new List<CarritoItemDto>();
            decimal subtotalGeneral = 0.00m;

            // Consultar stock en tienda si está asignada
            Dictionary<int, int> stocksPorProducto = new();
            if (tiendaId.HasValue)
            {
                var productoIds = detalles.Select(d => d.ProductoId).ToList();
                stocksPorProducto = await _context.Inventarios
                    .Where(i => i.TiendaId == tiendaId.Value && productoIds.Contains(i.ProductoId))
                    .ToDictionaryAsync(i => i.ProductoId, i => i.StockDisponible);
            }

            foreach (var d in detalles)
            {
                decimal subtotalItem = d.Cantidad * d.PrecioUnitario;
                subtotalGeneral += subtotalItem;

                int? stock = null;
                bool stockSuficiente = true;

                if (tiendaId.HasValue)
                {
                    stock = stocksPorProducto.TryGetValue(d.ProductoId, out int disp) ? disp : 0;
                    stockSuficiente = stock >= d.Cantidad;
                }

                itemsDto.Add(new CarritoItemDto
                {
                    DetalleCarritoId = d.DetalleCarritoId,
                    ProductoId = d.ProductoId,
                    Sku = d.Producto?.Sku ?? string.Empty,
                    NombreProducto = d.Producto?.Nombre ?? "Producto",
                    ImagenUrl = d.Producto?.ImagenUrl,
                    PrecioUnitario = d.PrecioUnitario,
                    Cantidad = d.Cantidad,
                    SubtotalItem = subtotalItem,
                    StockDisponibleEnTienda = stock,
                    StockSuficiente = stockSuficiente
                });
            }

            // Regla de Negocio: Para continuar con la compra es obligatorio ingresar una dirección de entrega o seleccionar un local de retiro
            bool tieneModalidadValida = direccionId.HasValue || tiendaRetiroId.HasValue;
            bool tieneItems = itemsDto.Count > 0;
            bool puedeContinuar = tieneItems && tieneModalidadValida;

            string mensaje = puedeContinuar
                ? "Carrito listo para proceder al checkout."
                : (!tieneItems ? "El carrito de compras está vacío."
                              : "Para continuar con la compra es obligatorio ingresar una dirección de entrega o seleccionar un local de retiro.");

            return new CarritoResponseDto
            {
                CarritoId = carrito.CarritoId,
                ClienteId = carrito.ClienteId,
                TotalItems = itemsDto.Sum(i => i.Cantidad),
                Subtotal = subtotalGeneral,
                Items = itemsDto,
                PuedeContinuarCompra = puedeContinuar,
                MensajeContinuarCompra = mensaje
            };
        }
    }
}
