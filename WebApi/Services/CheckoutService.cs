using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.DTOs;
using WebApi.Models;

namespace WebApi.Services
{
    /// <summary>
    /// Implementación de la lógica de negocio para Checkout, Logística, Validación de Stock y Pasarela de Pagos.
    /// </summary>
    public class CheckoutService : ICheckoutService
    {
        private readonly TamboDbContext _context;
        private readonly ILogger<CheckoutService> _logger;

        /// <summary>
        /// Constructor con inyección de dependencias de TamboDbContext y Logger.
        /// </summary>
        /// <param name="context">Contexto de acceso a datos.</param>
        /// <param name="logger">Servicio de registro de eventos.</param>
        public CheckoutService(TamboDbContext context, ILogger<CheckoutService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Servicio 4: Valida la cobertura de delivery para la dirección solicitada y determina la tienda Tambo+ activa más idónea.
        /// </summary>
        /// <param name="request">Parámetros de dirección y distrito de entrega.</param>
        /// <returns>Detalles de cobertura, tienda asignada y costo de envío.</returns>
        public async Task<CoberturaResponseDto> ValidarCoberturaAsync(ValidarCoberturaRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Distrito) && string.IsNullOrWhiteSpace(request.DireccionTexto))
            {
                return new CoberturaResponseDto
                {
                    TieneCobertura = false,
                    Mensaje = "Debe ingresar el distrito y dirección de entrega para validar cobertura."
                };
            }

            string distritoNormalizado = request.Distrito.Trim().ToLower();

            // Buscar zona de despacho activa que coincida con el distrito o nombre
            var zona = await _context.ZonasDespacho
                .FirstOrDefaultAsync(z => z.Activo && (z.Distrito.ToLower().Contains(distritoNormalizado) || z.Nombre.ToLower().Contains(distritoNormalizado)));

            if (zona == null)
            {
                return new CoberturaResponseDto
                {
                    TieneCobertura = false,
                    Mensaje = $"Lo sentimos, la dirección en el distrito de '{request.Distrito}' no cuenta actualmente con cobertura de delivery."
                };
            }

            // Regla de Negocio: Solo se puede realizar el delivery si la dirección se encuentra dentro de la zona de una tienda activa con delivery_activo = 1
            var tiendaAsignada = await _context.Tiendas
                .FirstOrDefaultAsync(t => t.ZonaId == zona.ZonaId && t.DeliveryActivo && !t.Borrado && t.Activo);

            if (tiendaAsignada == null)
            {
                // Buscar cualquier tienda activa en el mismo distrito
                tiendaAsignada = await _context.Tiendas
                    .FirstOrDefaultAsync(t => t.Distrito.ToLower().Contains(distritoNormalizado) && t.DeliveryActivo && !t.Borrado && t.Activo);
            }

            if (tiendaAsignada == null)
            {
                return new CoberturaResponseDto
                {
                    TieneCobertura = false,
                    Mensaje = $"En este momento no hay tiendas Tambo+ con servicio de delivery disponible para '{zona.Nombre}'."
                };
            }

            return new CoberturaResponseDto
            {
                TieneCobertura = true,
                Mensaje = $"¡Excelente! Tu dirección cuenta con cobertura atendida por la tienda {tiendaAsignada.Nombre}.",
                TiendaAsignadaId = tiendaAsignada.TiendaId,
                NombreTienda = tiendaAsignada.Nombre,
                DireccionTienda = tiendaAsignada.Direccion,
                ZonaId = zona.ZonaId,
                ZonaNombre = zona.Nombre,
                CostoDelivery = zona.CostoDelivery,
                TiempoEstimadoMin = zona.TiempoEstimadoMin
            };
        }

        /// <summary>
        /// Servicio 5: Lista las tiendas habilitadas para retiro en tienda física.
        /// </summary>
        /// <param name="busqueda">Criterio opcional de búsqueda por nombre, distrito o dirección.</param>
        /// <returns>Colección de tiendas con modalidad de retiro activa.</returns>
        public async Task<IEnumerable<TiendaRetiroDto>> ListarTiendasRetiroAsync(string? busqueda = null)
        {
            var query = _context.Tiendas
                .Where(t => !t.Borrado && t.Activo && t.RetiroActivo);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                string termino = busqueda.Trim().ToLower();
                query = query.Where(t => t.Nombre.ToLower().Contains(termino) ||
                                         t.Direccion.ToLower().Contains(termino) ||
                                         t.Distrito.ToLower().Contains(termino));
            }

            return await query
                .OrderBy(t => t.Distrito)
                .ThenBy(t => t.Nombre)
                .Select(t => new TiendaRetiroDto
                {
                    TiendaId = t.TiendaId,
                    CodigoTienda = t.CodigoTienda,
                    Nombre = t.Nombre,
                    Direccion = t.Direccion,
                    Distrito = t.Distrito,
                    Telefono = t.Telefono,
                    HorarioAtencion = t.HorarioAtencion,
                    RetiroActivo = t.RetiroActivo,
                    Latitud = t.Latitud,
                    Longitud = t.Longitud
                })
                .ToListAsync();
        }

        /// <summary>
        /// Servicio 15: Obtiene el catálogo de comprobantes de pago (Boleta y Factura).
        /// </summary>
        /// <returns>Lista de comprobantes de pago activos.</returns>
        public async Task<IEnumerable<TipoComprobanteDto>> ListarTiposComprobanteAsync()
        {
            return await _context.TiposComprobante
                .Where(tc => tc.Activo)
                .Select(tc => new TipoComprobanteDto
                {
                    TipoComprobanteId = tc.TipoComprobanteId,
                    Codigo = tc.Codigo,
                    Nombre = tc.Nombre,
                    RequiereRuc = tc.RequiereRuc
                })
                .ToListAsync();
        }

        /// <summary>
        /// Servicio 16: Lista los métodos de pago habilitados verificando previamente el stock disponible de cada producto en la tienda física asignada.
        /// </summary>
        /// <param name="request">Parámetros con tiendaId y los productos/cantidades a validar.</param>
        /// <returns>Resultado con validación de inventario y catálogo de métodos de pago activos.</returns>
        public async Task<ConsultarMetodosPagoResponseDto> ConsultarMetodosPagoDisponiblesAsync(ConsultarMetodosPagoRequestDto request)
        {
            var response = new ConsultarMetodosPagoResponseDto();

            // 1. Obtener la tienda
            var tienda = await _context.Tiendas.FindAsync(request.TiendaId);
            if (tienda == null || tienda.Borrado || !tienda.Activo)
            {
                response.StockDisponibleValido = false;
                response.MensajeValidacionStock = "La tienda asignada no está disponible.";
                return response;
            }

            // 2. Determinar los items a validar (directos o del pedido existente)
            var itemsAValidar = new List<ValidarStockItemDto>();

            if (request.Items != null && request.Items.Count > 0)
            {
                itemsAValidar.AddRange(request.Items);
            }
            else if (request.PedidoId.HasValue)
            {
                var detalles = await _context.DetallesPedido
                    .Where(dp => dp.PedidoId == request.PedidoId.Value)
                    .Select(dp => new ValidarStockItemDto { ProductoId = dp.ProductoId, Cantidad = dp.Cantidad })
                    .ToListAsync();

                itemsAValidar.AddRange(detalles);
            }

            // 3. Regla de Negocio: Si algún producto del pedido no tiene stock suficiente en la tienda asignada, no se puede continuar
            bool todosTienenStock = true;
            foreach (var item in itemsAValidar)
            {
                var inventario = await _context.Inventarios
                    .Include(i => i.Producto)
                    .FirstOrDefaultAsync(i => i.TiendaId == request.TiendaId && i.ProductoId == item.ProductoId);

                string nombreProducto = inventario?.Producto?.Nombre ?? $"Producto ID #{item.ProductoId}";

                if (inventario == null || inventario.StockDisponible < item.Cantidad)
                {
                    todosTienenStock = false;
                    int disponible = inventario?.StockDisponible ?? 0;
                    response.ErroresStock.Add($"El producto '{nombreProducto}' no tiene stock suficiente en {tienda.Nombre} (Solicitado: {item.Cantidad}, Disponible: {disponible}).");
                }
            }

            if (!todosTienenStock)
            {
                response.StockDisponibleValido = false;
                response.MensajeValidacionStock = "No se puede continuar con el pago debido a stock insuficiente en la tienda asignada.";
                return response; // No lista métodos de pago si falla el stock
            }

            response.StockDisponibleValido = true;
            response.MensajeValidacionStock = "Stock validado correctamente para todos los productos.";

            // 4. Regla de Negocio: Solo se listan métodos de pago activos y no borrados
            response.MetodosPago = await _context.MetodosPago
                .Where(m => !m.Borrado && m.Activo)
                .Select(m => new MetodoPagoDto
                {
                    MetodoPagoId = m.MetodoPagoId,
                    Codigo = m.Codigo,
                    Nombre = m.Nombre,
                    Descripcion = m.Descripcion,
                    IconoUrl = m.IconoUrl,
                    EsOnline = m.EsOnline
                })
                .ToListAsync();

            return response;
        }

        /// <summary>
        /// Servicio 3: Valida modalidad (Delivery/Retiro), disponibilidad de stock, aplica cupones de descuento, calcula costos de envío y genera el pedido.
        /// </summary>
        /// <param name="request">Datos del pedido, productos, modalidad y comprobante.</param>
        /// <returns>Resumen del pedido creado y totales calculados.</returns>
        public async Task<PedidoCreadoResponseDto> CrearPedidoCheckoutAsync(CrearPedidoCheckoutDto request)
        {
            // Validar cliente
            var cliente = await _context.Clientes.FindAsync(request.ClienteId);
            if (cliente == null || !cliente.Activo || cliente.Borrado)
            {
                return new PedidoCreadoResponseDto { Exito = false, Mensaje = "Cliente inválido o inactivo." };
            }

            // Validar tienda
            var tienda = await _context.Tiendas.FindAsync(request.TiendaId);
            if (tienda == null || !tienda.Activo || tienda.Borrado)
            {
                return new PedidoCreadoResponseDto { Exito = false, Mensaje = "Tienda seleccionada inválida o inactiva." };
            }

            // Validar modalidad y dirección
            decimal costoEnvio = 0.00m;
            if (request.TipoEntrega == "DELIVERY")
            {
                if (!tienda.DeliveryActivo)
                {
                    return new PedidoCreadoResponseDto { Exito = false, Mensaje = $"La tienda {tienda.Nombre} no cuenta con delivery activo." };
                }

                if (!request.DireccionId.HasValue)
                {
                    return new PedidoCreadoResponseDto { Exito = false, Mensaje = "Para entrega por delivery es obligatorio seleccionar una dirección de entrega." };
                }

                var dir = await _context.Direcciones.Include(d => d.Zona).FirstOrDefaultAsync(d => d.DireccionId == request.DireccionId.Value && d.ClienteId == request.ClienteId);
                if (dir == null || dir.Borrado)
                {
                    return new PedidoCreadoResponseDto { Exito = false, Mensaje = "Dirección de entrega inválida o no pertenece al cliente." };
                }

                costoEnvio = dir.Zona?.CostoDelivery ?? 5.00m;
            }
            else // RETIRO_TIENDA
            {
                if (!tienda.RetiroActivo)
                {
                    return new PedidoCreadoResponseDto { Exito = false, Mensaje = $"La tienda {tienda.Nombre} no tiene habilitado el retiro en tienda." };
                }
            }

            // Validar comprobante
            var tipoComp = await _context.TiposComprobante.FindAsync(request.TipoComprobanteId);
            if (tipoComp == null || !tipoComp.Activo)
            {
                return new PedidoCreadoResponseDto { Exito = false, Mensaje = "Tipo de comprobante no válido." };
            }

            if (tipoComp.RequiereRuc)
            {
                if (string.IsNullOrWhiteSpace(request.DocComprobante) || request.DocComprobante.Trim().Length != 11)
                {
                    return new PedidoCreadoResponseDto { Exito = false, Mensaje = "Para Factura es obligatorio ingresar un RUC válido de 11 dígitos." };
                }
                if (string.IsNullOrWhiteSpace(request.RazonSocial))
                {
                    return new PedidoCreadoResponseDto { Exito = false, Mensaje = "Para Factura es obligatoria la Razón Social." };
                }
            }

            // Validar stock y calcular subtotal
            decimal subtotal = 0.00m;
            var detallesParaInsertar = new List<DetallePedido>();

            foreach (var item in request.Items)
            {
                var producto = await _context.Productos.FindAsync(item.ProductoId);
                if (producto == null || !producto.Activo || producto.Borrado)
                {
                    return new PedidoCreadoResponseDto { Exito = false, Mensaje = $"Producto con ID {item.ProductoId} no existe o no está activo." };
                }

                var inventario = await _context.Inventarios
                    .FirstOrDefaultAsync(i => i.TiendaId == request.TiendaId && i.ProductoId == item.ProductoId);

                if (inventario == null || inventario.StockDisponible < item.Cantidad)
                {
                    int disp = inventario?.StockDisponible ?? 0;
                    return new PedidoCreadoResponseDto
                    {
                        Exito = false,
                        Mensaje = $"Stock insuficiente para '{producto.Nombre}' en {tienda.Nombre}. Solicitado: {item.Cantidad}, Disponible: {disp}."
                    };
                }

                decimal precioEfectivo = producto.PrecioOferta ?? producto.Precio;
                decimal itemSubtotal = precioEfectivo * item.Cantidad;
                subtotal += itemSubtotal;

                detallesParaInsertar.Add(new DetallePedido
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = precioEfectivo,
                    Subtotal = itemSubtotal
                });
            }

            // Validar y aplicar cupón si se proporcionó
            decimal descuento = 0.00m;
            int? cuponIdAplicado = null;

            if (!string.IsNullOrWhiteSpace(request.CuponCodigo))
            {
                var cupon = await _context.Cupones
                    .FirstOrDefaultAsync(c => c.Codigo.ToUpper() == request.CuponCodigo.Trim().ToUpper() &&
                                              c.Activo &&
                                              c.FechaInicio <= DateTime.UtcNow &&
                                              c.FechaFin >= DateTime.UtcNow);

                if (cupon != null && subtotal >= cupon.MontoMinimoCompra)
                {
                    if (cupon.TipoDescuento == "PORCENTAJE")
                    {
                        descuento = Math.Round(subtotal * (cupon.ValorDescuento / 100m), 2);
                    }
                    else // MONTO_FIJO
                    {
                        descuento = Math.Min(subtotal, cupon.ValorDescuento);
                    }
                    cuponIdAplicado = cupon.CuponId;
                    cupon.UsosActuales += 1;
                }
            }

            decimal total = Math.Max(0, subtotal - descuento + costoEnvio);

            // Obtener estado inicial (CONFIRMADO)
            var estadoInicial = await _context.EstadosPedido
                .FirstOrDefaultAsync(e => e.Codigo == "CONFIRMADO") ?? await _context.EstadosPedido.FirstAsync();

            string numeroPedido = $"TMB-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

            var pedido = new Pedido
            {
                NumeroPedido = numeroPedido,
                ClienteId = request.ClienteId,
                TiendaId = request.TiendaId,
                TipoEntrega = request.TipoEntrega,
                DireccionId = request.DireccionId,
                TipoComprobanteId = request.TipoComprobanteId,
                DocComprobante = request.DocComprobante.Trim(),
                RazonSocial = request.RazonSocial?.Trim(),
                DireccionFiscal = request.DireccionFiscal?.Trim(),
                Subtotal = subtotal,
                Descuento = descuento,
                CostoEnvio = costoEnvio,
                MontoTotal = total,
                CuponId = cuponIdAplicado,
                EstadoPedidoId = estadoInicial.EstadoPedidoId,
                Notas = request.Notas?.Trim(),
                FechaPedido = DateTime.UtcNow
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            // Insertar detalles
            foreach (var det in detallesParaInsertar)
            {
                det.PedidoId = pedido.PedidoId;
                _context.DetallesPedido.Add(det);

                // Reservar/descontar stock de inventario
                var inv = await _context.Inventarios
                    .FirstAsync(i => i.TiendaId == request.TiendaId && i.ProductoId == det.ProductoId);
                inv.StockDisponible -= det.Cantidad;
                inv.StockReservado += det.Cantidad;
                inv.UltimaActualizacion = DateTime.UtcNow;
            }

            // Registrar historial de estado
            _context.HistorialesEstadoPedido.Add(new HistorialEstadoPedido
            {
                PedidoId = pedido.PedidoId,
                EstadoPedidoId = estadoInicial.EstadoPedidoId,
                FechaCambio = DateTime.UtcNow,
                Comentario = "Pedido generado y listo para pago en pasarela."
            });

            await _context.SaveChangesAsync();

            return new PedidoCreadoResponseDto
            {
                Exito = true,
                Mensaje = "Pedido generado exitosamente.",
                PedidoId = pedido.PedidoId,
                NumeroPedido = pedido.NumeroPedido,
                Subtotal = pedido.Subtotal,
                Descuento = pedido.Descuento,
                CostoEnvio = pedido.CostoEnvio,
                MontoTotal = pedido.MontoTotal,
                Estado = estadoInicial.Nombre
            };
        }

        /// <summary>
        /// Servicio 17: Procesa el pago con tarjeta bancaria, verifica que el pedido no esté pagado previamente, cobra el monto total y actualiza el ciclo de vida del pedido.
        /// </summary>
        /// <param name="request">Datos de la tarjeta de crédito/débito y del titular.</param>
        /// <returns>Resultado de la transacción bancaria y código de autorización.</returns>
        public async Task<PagoResponseDto> ProcesarPagoTarjetaAsync(ProcesarPagoTarjetaDto request)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Pagos)
                .Include(p => p.EstadoPedido)
                .FirstOrDefaultAsync(p => p.PedidoId == request.PedidoId);

            if (pedido == null)
            {
                return new PagoResponseDto
                {
                    Aprobado = false,
                    Mensaje = "El pedido indicado no existe."
                };
            }

            // Regla de Negocio: El pedido no debe tener ya un pago aprobado
            bool yaPagado = pedido.Pagos.Any(p => p.Estado == "APROBADO");
            if (yaPagado)
            {
                return new PagoResponseDto
                {
                    Aprobado = false,
                    Mensaje = "El pedido ya cuenta con un pago aprobado previamente."
                };
            }

            // Obtener método de pago 'TARJETA'
            var metodoTarjeta = await _context.MetodosPago
                .FirstOrDefaultAsync(m => m.Codigo == "TARJETA" && !m.Borrado)
                ?? await _context.MetodosPago.FirstAsync();

            string ultimos4 = request.NumeroTarjeta.Length >= 4
                ? request.NumeroTarjeta[^4..]
                : "0000";

            string marcaTarjeta = request.NumeroTarjeta.StartsWith("4") ? "VISA" :
                                  request.NumeroTarjeta.StartsWith("5") ? "MASTERCARD" : "TARJETA";

            // Simulación de pasarela de pago: si tarjeta termina en '0000' se rechaza por simulación de fondos insuficientes
            bool transaccionAprobada = !ultimos4.Equals("0000");

            string codigoTransaccion = transaccionAprobada
                ? $"TXN-NIUBIZ-{Guid.NewGuid().ToString("N")[..12].ToUpper()}"
                : $"REJ-NIUBIZ-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

            string mensajeRespuesta = transaccionAprobada
                ? "Transacción bancaria aprobada exitosamente."
                : "Operación rechazada por la entidad bancaria (Fondos insuficientes o tarjeta no habilitada).";

            var pago = new Pago
            {
                PedidoId = pedido.PedidoId,
                MetodoPagoId = metodoTarjeta.MetodoPagoId,
                Monto = pedido.MontoTotal, // Regla de negocio: el monto cobrado es el monto_total del pedido
                Estado = transaccionAprobada ? "APROBADO" : "RECHAZADO",
                CodigoTransaccion = codigoTransaccion,
                MensajeRespuesta = mensajeRespuesta,
                TarjetaMarca = marcaTarjeta,
                TarjetaUltimosDigitos = ultimos4,
                TitularNombre = request.TitularNombre.Trim(),
                TitularApellido = request.TitularApellido.Trim(),
                TitularCorreo = request.TitularCorreo.Trim(),
                FechaPago = DateTime.UtcNow
            };

            _context.Pagos.Add(pago);

            if (transaccionAprobada)
            {
                var estadoPreparando = await _context.EstadosPedido
                    .FirstOrDefaultAsync(e => e.Codigo == "PREPARANDO")
                    ?? pedido.EstadoPedido;

                if (estadoPreparando != null)
                {
                    pedido.EstadoPedidoId = estadoPreparando.EstadoPedidoId;
                }

                _context.HistorialesEstadoPedido.Add(new HistorialEstadoPedido
                {
                    PedidoId = pedido.PedidoId,
                    EstadoPedidoId = pedido.EstadoPedidoId,
                    FechaCambio = DateTime.UtcNow,
                    Comentario = $"Pago aprobado vía tarjeta terminada en {ultimos4}. Pedido pasa a preparación."
                });

                // Crear registro inicial de entrega
                if (pedido.TipoEntrega == "DELIVERY")
                {
                    var motorizadoDisponible = await _context.Motorizados.FirstOrDefaultAsync(m => m.Activo && !m.Borrado);
                    _context.Entregas.Add(new Entrega
                    {
                        PedidoId = pedido.PedidoId,
                        MotorizadoId = motorizadoDisponible?.MotorizadoId,
                        TiempoEstimadoLlegada = DateTime.UtcNow.AddMinutes(35),
                        FechaAsignacion = DateTime.UtcNow,
                        Observaciones = "Entrega programada con prioridad."
                    });
                }
            }

            await _context.SaveChangesAsync();

            return new PagoResponseDto
            {
                Aprobado = transaccionAprobada,
                Mensaje = mensajeRespuesta,
                CodigoTransaccion = codigoTransaccion,
                MontoCobrado = pedido.MontoTotal,
                TarjetaMarca = marcaTarjeta,
                TarjetaUltimosDigitos = ultimos4,
                FechaPago = pago.FechaPago,
                NuevoEstadoPedido = transaccionAprobada ? "PREPARANDO" : pedido.EstadoPedido?.Nombre
            };
        }

        /// <summary>
        /// Servicio 18: Consulta el detalle consolidado de la orden en la pasarela de pagos.
        /// </summary>
        /// <param name="pedidoId">Identificador del pedido.</param>
        /// <returns>Resumen de orden y estado de pago.</returns>
        public async Task<ResumenOrdenPasarelaDto?> ConsultarDetalleOrdenPasarelaAsync(int pedidoId)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Tienda)
                .Include(p => p.EstadoPedido)
                .Include(p => p.Detalles).ThenInclude(d => d.Producto)
                .Include(p => p.Pagos)
                .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);

            if (pedido == null) return null;

            var ultimoPago = pedido.Pagos.OrderByDescending(p => p.FechaPago).FirstOrDefault();

            return new ResumenOrdenPasarelaDto
            {
                PedidoId = pedido.PedidoId,
                NumeroPedido = pedido.NumeroPedido,
                FechaPedido = pedido.FechaPedido,
                TiendaNombre = pedido.Tienda?.Nombre ?? "Tambo+",
                TipoEntrega = pedido.TipoEntrega,
                EstadoPedido = pedido.EstadoPedido?.Nombre ?? "Pendiente",
                Subtotal = pedido.Subtotal,
                Descuento = pedido.Descuento,
                CostoEnvio = pedido.CostoEnvio,
                MontoTotal = pedido.MontoTotal,
                EstadoUltimoPago = ultimoPago?.Estado ?? "SIN_INTENTO",
                CodigoTransaccionPago = ultimoPago?.CodigoTransaccion,
                Items = pedido.Detalles.Select(d => new DetalleItemResumenDto
                {
                    ProductoId = d.ProductoId,
                    NombreProducto = d.Producto?.Nombre ?? "Producto",
                    ImagenUrl = d.Producto?.ImagenUrl,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }).ToList()
            };
        }

        /// <summary>
        /// Servicio 19: Obtiene el seguimiento del pedido, historial de transiciones y datos del motorizado si aplica.
        /// </summary>
        /// <param name="numeroPedido">Código del pedido (ej. TMB-202611-0001).</param>
        /// <returns>Información de seguimiento o null si no existe.</returns>
        public async Task<SeguimientoPedidoDto?> ObtenerSeguimientoPedidoAsync(string numeroPedido)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.EstadoPedido)
                .Include(p => p.Detalles).ThenInclude(d => d.Producto)
                .Include(p => p.Pagos).ThenInclude(pg => pg.MetodoPago)
                .Include(p => p.Entrega).ThenInclude(e => e!.Motorizado)
                .Include(p => p.HistorialEstados).ThenInclude(h => h.EstadoPedido)
                .FirstOrDefaultAsync(p => p.NumeroPedido.ToLower() == numeroPedido.Trim().ToLower());

            if (pedido == null) return null;

            var pagoAprobado = pedido.Pagos.FirstOrDefault(p => p.Estado == "APROBADO")
                               ?? pedido.Pagos.OrderByDescending(p => p.FechaPago).FirstOrDefault();

            RepartidorContactoDto? repartidorDto = null;
            if (pedido.Entrega?.Motorizado != null)
            {
                var mot = pedido.Entrega.Motorizado;
                repartidorDto = new RepartidorContactoDto
                {
                    NombreCompleto = $"{mot.Nombres} {mot.Apellidos}",
                    Celular = mot.Celular,
                    Placa = mot.PlacaVehiculo,
                    TipoVehiculo = mot.TipoVehiculo
                };
            }

            return new SeguimientoPedidoDto
            {
                NumeroPedido = pedido.NumeroPedido,
                EstadoActual = pedido.EstadoPedido?.Nombre ?? "Confirmado",
                TiempoEstimadoLlegada = pedido.Entrega?.TiempoEstimadoLlegada?.ToString("HH:mm") ?? "30-45 min",
                Repartidor = repartidorDto,
                TotalPagado = pedido.MontoTotal,
                MedioPago = pagoAprobado?.MetodoPago?.Nombre ?? "Tarjeta / Online",
                Productos = pedido.Detalles.Select(d => new DetalleItemResumenDto
                {
                    ProductoId = d.ProductoId,
                    NombreProducto = d.Producto?.Nombre ?? "Producto",
                    ImagenUrl = d.Producto?.ImagenUrl,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }).ToList(),
                HistorialEstados = pedido.HistorialEstados
                    .OrderBy(h => h.FechaCambio)
                    .Select(h => new HistorialEstadoDto
                    {
                        Estado = h.EstadoPedido?.Nombre ?? "Estado",
                        FechaHora = h.FechaCambio,
                        Comentario = h.Comentario
                    }).ToList()
            };
        }
    }
}
