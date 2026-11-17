using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.DTOs;

namespace WebApi.Services
{
    /// <summary>
    /// Implementación de la lógica de negocio para promociones, combos Tambo Packs y cupones de descuento.
    /// </summary>
    public class PromocionService : IPromocionService
    {
        private readonly TamboDbContext _context;

        /// <summary>
        /// Constructor con inyección del DbContext.
        /// </summary>
        /// <param name="context">Contexto de acceso a base de datos.</param>
        public PromocionService(TamboDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Servicio 8: Obtiene las promociones y combos vigentes a la fecha actual con los productos que incluye cada combo.
        /// </summary>
        /// <returns>Lista de promociones vigentes.</returns>
        public async Task<IEnumerable<PromocionPackDto>> ListarPromocionesCombosAsync()
        {
            var ahora = DateTime.UtcNow;

            var promociones = await _context.Promociones
                .Include(p => p.Banner)
                .Include(p => p.Productos)
                    .ThenInclude(pp => pp.Producto)
                .Where(p => p.Activo && p.FechaInicio <= ahora && p.FechaFin >= ahora)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return promociones.Select(p => new PromocionPackDto
            {
                PromocionId = p.PromocionId,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                TipoPromocion = p.TipoPromocion,
                PrecioPromocional = p.PrecioPromocional,
                PorcentajeDescuento = p.PorcentajeDescuento,
                CantidadPack = p.CantidadPack,
                MaxPorCliente = p.MaxPorCliente,
                FechaInicio = p.FechaInicio,
                FechaFin = p.FechaFin,
                BannerTitulo = p.Banner?.Titulo,
                BannerImagenUrl = p.Banner?.ImagenUrl,
                ProductosIncluidos = p.Productos.Select(pp => new PromocionProductoItemDto
                {
                    ProductoId = pp.ProductoId,
                    NombreProducto = pp.Producto?.Nombre ?? "Producto",
                    ImagenUrl = pp.Producto?.ImagenUrl,
                    PrecioRegular = pp.Producto?.Precio ?? 0.00m,
                    Cantidad = pp.Cantidad
                }).ToList()
            });
        }

        /// <summary>
        /// Servicio 11: Valida el código de cupón, su estado activo, fechas de inicio/fin, cantidad máxima de usos y monto mínimo de compra.
        /// </summary>
        /// <param name="request">Código y subtotal a verificar.</param>
        /// <returns>Descuento aplicado y total recalculado.</returns>
        public async Task<ValidarCuponResponseDto> ValidarCuponAsync(ValidarCuponRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Codigo))
            {
                return new ValidarCuponResponseDto
                {
                    EsValido = false,
                    Mensaje = "Debe ingresar un código de cupón de descuento.",
                    SubtotalOriginal = request.SubtotalActual,
                    TotalConDescuento = request.SubtotalActual
                };
            }

            var cupon = await _context.Cupones
                .FirstOrDefaultAsync(c => c.Codigo.ToUpper() == request.Codigo.Trim().ToUpper());

            if (cupon == null || !cupon.Activo)
            {
                return new ValidarCuponResponseDto
                {
                    EsValido = false,
                    Mensaje = "El código de cupón ingresado no existe o no se encuentra activo.",
                    SubtotalOriginal = request.SubtotalActual,
                    TotalConDescuento = request.SubtotalActual
                };
            }

            var ahora = DateTime.UtcNow;
            if (ahora < cupon.FechaInicio || ahora > cupon.FechaFin)
            {
                return new ValidarCuponResponseDto
                {
                    EsValido = false,
                    Mensaje = "El cupón ha vencido o aún no entra en vigencia.",
                    SubtotalOriginal = request.SubtotalActual,
                    TotalConDescuento = request.SubtotalActual
                };
            }

            if (cupon.UsosMaximos.HasValue && cupon.UsosActuales >= cupon.UsosMaximos.Value)
            {
                return new ValidarCuponResponseDto
                {
                    EsValido = false,
                    Mensaje = "El cupón ha alcanzado el límite máximo de canjes permitidos.",
                    SubtotalOriginal = request.SubtotalActual,
                    TotalConDescuento = request.SubtotalActual
                };
            }

            if (request.SubtotalActual < cupon.MontoMinimoCompra)
            {
                return new ValidarCuponResponseDto
                {
                    EsValido = false,
                    Mensaje = $"El subtotal mínimo para aplicar este cupón es de S/ {cupon.MontoMinimoCompra:N2}.",
                    SubtotalOriginal = request.SubtotalActual,
                    TotalConDescuento = request.SubtotalActual
                };
            }

            // Calcular descuento
            decimal descuentoCalculado = 0.00m;
            if (cupon.TipoDescuento == "PORCENTAJE")
            {
                descuentoCalculado = Math.Round(request.SubtotalActual * (cupon.ValorDescuento / 100m), 2);
            }
            else // MONTO_FIJO
            {
                descuentoCalculado = Math.Min(request.SubtotalActual, cupon.ValorDescuento);
            }

            decimal totalConDescuento = Math.Max(0.00m, request.SubtotalActual - descuentoCalculado);

            return new ValidarCuponResponseDto
            {
                EsValido = true,
                Mensaje = $"¡Cupón aplicado con éxito! Ahorraste S/ {descuentoCalculado:N2}.",
                CuponId = cupon.CuponId,
                Codigo = cupon.Codigo,
                TipoDescuento = cupon.TipoDescuento,
                ValorDescuento = cupon.ValorDescuento,
                DescuentoCalculado = descuentoCalculado,
                SubtotalOriginal = request.SubtotalActual,
                TotalConDescuento = totalConDescuento
            };
        }
    }
}
