using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Models
{
    [Table("cliente")]
    public class Cliente
    {
        [Key]
        [Column("cliente_id")]
        public int ClienteId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("tipo_documento")]
        public string TipoDocumento { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Column("numero_documento")]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("nombres")]
        public string Nombres { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("correo")]
        public string Correo { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Column("celular")]
        public string Celular { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("contrasena")]
        public string Contrasena { get; set; } = string.Empty;

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("borrado")]
        public bool Borrado { get; set; } = false;

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [Column("ultimo_acceso")]
        public DateTime? UltimoAcceso { get; set; }

        public ICollection<Direccion> Direcciones { get; set; } = new List<Direccion>();
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }

    [Table("zona_despacho")]
    public class ZonaDespacho
    {
        [Key]
        [Column("zona_id")]
        public int ZonaId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("distrito")]
        public string Distrito { get; set; } = string.Empty;

        [MaxLength(20)]
        [Column("codigo_postal")]
        public string? CodigoPostal { get; set; }

        [Column("costo_delivery", TypeName = "decimal(10,2)")]
        public decimal CostoDelivery { get; set; }

        [Column("tiempo_estimado_min")]
        public int TiempoEstimadoMin { get; set; } = 45;

        [Column("activo")]
        public bool Activo { get; set; } = true;

        public ICollection<Tienda> Tiendas { get; set; } = new List<Tienda>();
    }

    [Table("tienda")]
    public class Tienda
    {
        [Key]
        [Column("tienda_id")]
        public int TiendaId { get; set; }

        [Column("zona_id")]
        public int? ZonaId { get; set; }

        [ForeignKey("ZonaId")]
        public ZonaDespacho? Zona { get; set; }

        [Required]
        [MaxLength(30)]
        [Column("codigo_tienda")]
        public string CodigoTienda { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("direccion")]
        public string Direccion { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("distrito")]
        public string Distrito { get; set; } = string.Empty;

        [MaxLength(20)]
        [Column("telefono")]
        public string? Telefono { get; set; }

        [Column("latitud", TypeName = "decimal(9,6)")]
        public decimal? Latitud { get; set; }

        [Column("longitud", TypeName = "decimal(9,6)")]
        public decimal? Longitud { get; set; }

        [MaxLength(100)]
        [Column("horario_atencion")]
        public string HorarioAtencion { get; set; } = "24 Horas";

        [Column("delivery_activo")]
        public bool DeliveryActivo { get; set; } = true;

        [Column("retiro_activo")]
        public bool RetiroActivo { get; set; } = true;

        [Column("borrado")]
        public bool Borrado { get; set; } = false;

        [Column("activo")]
        public bool Activo { get; set; } = true;

        public ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();
    }

    [Table("direccion")]
    public class Direccion
    {
        [Key]
        [Column("direccion_id")]
        public int DireccionId { get; set; }

        [Column("cliente_id")]
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }

        [Column("zona_id")]
        public int? ZonaId { get; set; }

        [ForeignKey("ZonaId")]
        public ZonaDespacho? Zona { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("alias")]
        public string Alias { get; set; } = "Mi Dirección";

        [Required]
        [MaxLength(100)]
        [Column("distrito")]
        public string Distrito { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("direccion_texto")]
        public string DireccionTexto { get; set; } = string.Empty;

        [MaxLength(20)]
        [Column("numero")]
        public string? Numero { get; set; }

        [MaxLength(20)]
        [Column("departamento_interior")]
        public string? DepartamentoInterior { get; set; }

        [MaxLength(255)]
        [Column("referencia")]
        public string? Referencia { get; set; }

        [Column("latitud", TypeName = "decimal(9,6)")]
        public decimal? Latitud { get; set; }

        [Column("longitud", TypeName = "decimal(9,6)")]
        public decimal? Longitud { get; set; }

        [Column("principal")]
        public bool Principal { get; set; } = false;

        [Column("borrado")]
        public bool Borrado { get; set; } = false;

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }

    [Table("categoria")]
    public class Categoria
    {
        [Key]
        [Column("categoria_id")]
        public int CategoriaId { get; set; }

        [Column("categoria_padre_id")]
        public int? CategoriaPadreId { get; set; }

        [ForeignKey("CategoriaPadreId")]
        public Categoria? CategoriaPadre { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [MaxLength(500)]
        [Column("icono_url")]
        public string? IconoUrl { get; set; }

        [MaxLength(500)]
        [Column("imagen_url")]
        public string? ImagenUrl { get; set; }

        [Column("orden")]
        public int Orden { get; set; } = 0;

        [Column("borrado")]
        public bool Borrado { get; set; } = false;

        [Column("activo")]
        public bool Activo { get; set; } = true;

        public ICollection<Categoria> Subcategorias { get; set; } = new List<Categoria>();
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }

    [Table("marca")]
    public class Marca
    {
        [Key]
        [Column("marca_id")]
        public int MarcaId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("borrado")]
        public bool Borrado { get; set; } = false;

        [Column("activo")]
        public bool Activo { get; set; } = true;

        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }

    [Table("producto")]
    public class Producto
    {
        [Key]
        [Column("producto_id")]
        public int ProductoId { get; set; }

        [Column("categoria_id")]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public Categoria? Categoria { get; set; }

        [Column("marca_id")]
        public int MarcaId { get; set; }

        [ForeignKey("MarcaId")]
        public Marca? Marca { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("sku")]
        public string Sku { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [MaxLength(500)]
        [Column("imagen_url")]
        public string? ImagenUrl { get; set; }

        [Column("precio", TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        [Column("precio_oferta", TypeName = "decimal(10,2)")]
        public decimal? PrecioOferta { get; set; }

        [MaxLength(50)]
        [Column("unidad_medida")]
        public string UnidadMedida { get; set; } = "Unidad";

        [Column("es_destacado")]
        public bool EsDestacado { get; set; } = false;

        [Column("borrado")]
        public bool Borrado { get; set; } = false;

        [Column("activo")]
        public bool Activo { get; set; } = true;

        public ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();
    }

    [Table("inventario")]
    public class Inventario
    {
        [Key]
        [Column("inventario_id")]
        public int InventarioId { get; set; }

        [Column("tienda_id")]
        public int TiendaId { get; set; }

        [ForeignKey("TiendaId")]
        public Tienda? Tienda { get; set; }

        [Column("producto_id")]
        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public Producto? Producto { get; set; }

        [Column("stock_disponible")]
        public int StockDisponible { get; set; } = 0;

        [Column("stock_reservado")]
        public int StockReservado { get; set; } = 0;

        [Column("stock_minimo")]
        public int StockMinimo { get; set; } = 3;

        [Column("ultima_actualizacion")]
        public DateTime UltimaActualizacion { get; set; } = DateTime.UtcNow;
    }

    [Table("tipo_comprobante")]
    public class TipoComprobante
    {
        [Key]
        [Column("tipo_comprobante_id")]
        public int TipoComprobanteId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("requiere_ruc")]
        public bool RequiereRuc { get; set; } = false;

        [Column("activo")]
        public bool Activo { get; set; } = true;
    }

    [Table("metodo_pago")]
    public class MetodoPago
    {
        [Key]
        [Column("metodo_pago_id")]
        public int MetodoPagoId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [MaxLength(500)]
        [Column("icono_url")]
        public string? IconoUrl { get; set; }

        [Column("es_online")]
        public bool EsOnline { get; set; } = true;

        [Column("borrado")]
        public bool Borrado { get; set; } = false;

        [Column("activo")]
        public bool Activo { get; set; } = true;
    }

    [Table("estado_pedido")]
    public class EstadoPedido
    {
        [Key]
        [Column("estado_pedido_id")]
        public int EstadoPedidoId { get; set; }

        [Required]
        [MaxLength(30)]
        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("orden_secuencia")]
        public int OrdenSecuencia { get; set; }

        [MaxLength(255)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }
    }

    [Table("motorizado")]
    public class Motorizado
    {
        [Key]
        [Column("motorizado_id")]
        public int MotorizadoId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombres")]
        public string Nombres { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Column("celular")]
        public string Celular { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Column("placa_vehiculo")]
        public string PlacaVehiculo { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("tipo_vehiculo")]
        public string TipoVehiculo { get; set; } = "Moto lineal";

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("borrado")]
        public bool Borrado { get; set; } = false;
    }

    [Table("cupon")]
    public class Cupon
    {
        [Key]
        [Column("cupon_id")]
        public int CuponId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [MaxLength(200)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("tipo_descuento")]
        public string TipoDescuento { get; set; } = "PORCENTAJE";

        [Column("valor_descuento", TypeName = "decimal(10,2)")]
        public decimal ValorDescuento { get; set; }

        [Column("monto_minimo_compra", TypeName = "decimal(10,2)")]
        public decimal MontoMinimoCompra { get; set; } = 0.00m;

        [Column("usos_maximos")]
        public int? UsosMaximos { get; set; }

        [Column("usos_actuales")]
        public int UsosActuales { get; set; } = 0;

        [Column("fecha_inicio")]
        public DateTime FechaInicio { get; set; }

        [Column("fecha_fin")]
        public DateTime FechaFin { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;
    }

    [Table("pedido")]
    public class Pedido
    {
        [Key]
        [Column("pedido_id")]
        public int PedidoId { get; set; }

        [Required]
        [MaxLength(30)]
        [Column("numero_pedido")]
        public string NumeroPedido { get; set; } = string.Empty;

        [Column("cliente_id")]
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }

        [Column("tienda_id")]
        public int TiendaId { get; set; }

        [ForeignKey("TiendaId")]
        public Tienda? Tienda { get; set; }

        [Required]
        [MaxLength(30)]
        [Column("tipo_entrega")]
        public string TipoEntrega { get; set; } = "DELIVERY"; // 'DELIVERY' o 'RETIRO_TIENDA'

        [Column("direccion_id")]
        public int? DireccionId { get; set; }

        [ForeignKey("DireccionId")]
        public Direccion? Direccion { get; set; }

        [Column("tipo_comprobante_id")]
        public int TipoComprobanteId { get; set; }

        [ForeignKey("TipoComprobanteId")]
        public TipoComprobante? TipoComprobante { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("doc_comprobante")]
        public string DocComprobante { get; set; } = string.Empty;

        [MaxLength(150)]
        [Column("razon_social")]
        public string? RazonSocial { get; set; }

        [MaxLength(255)]
        [Column("direccion_fiscal")]
        public string? DireccionFiscal { get; set; }

        [Column("subtotal", TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        [Column("descuento", TypeName = "decimal(10,2)")]
        public decimal Descuento { get; set; } = 0.00m;

        [Column("costo_envio", TypeName = "decimal(10,2)")]
        public decimal CostoEnvio { get; set; } = 0.00m;

        [Column("monto_total", TypeName = "decimal(10,2)")]
        public decimal MontoTotal { get; set; }

        [Column("cupon_id")]
        public int? CuponId { get; set; }

        [ForeignKey("CuponId")]
        public Cupon? Cupon { get; set; }

        [Column("estado_pedido_id")]
        public int EstadoPedidoId { get; set; }

        [ForeignKey("EstadoPedidoId")]
        public EstadoPedido? EstadoPedido { get; set; }

        [MaxLength(500)]
        [Column("notas")]
        public string? Notas { get; set; }

        [Column("fecha_pedido")]
        public DateTime FechaPedido { get; set; } = DateTime.UtcNow;

        public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
        public Entrega? Entrega { get; set; }
        public ICollection<HistorialEstadoPedido> HistorialEstados { get; set; } = new List<HistorialEstadoPedido>();
    }

    [Table("detalle_pedido")]
    public class DetallePedido
    {
        [Key]
        [Column("detalle_pedido_id")]
        public int DetallePedidoId { get; set; }

        [Column("pedido_id")]
        public int PedidoId { get; set; }

        [ForeignKey("PedidoId")]
        public Pedido? Pedido { get; set; }

        [Column("producto_id")]
        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public Producto? Producto { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("precio_unitario", TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column("subtotal", TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }
    }

    [Table("pago")]
    public class Pago
    {
        [Key]
        [Column("pago_id")]
        public int PagoId { get; set; }

        [Column("pedido_id")]
        public int PedidoId { get; set; }

        [ForeignKey("PedidoId")]
        public Pedido? Pedido { get; set; }

        [Column("metodo_pago_id")]
        public int MetodoPagoId { get; set; }

        [ForeignKey("MetodoPagoId")]
        public MetodoPago? MetodoPago { get; set; }

        [Column("monto", TypeName = "decimal(10,2)")]
        public decimal Monto { get; set; }

        [Required]
        [MaxLength(30)]
        [Column("estado")]
        public string Estado { get; set; } = "PENDIENTE"; // 'APROBADO', 'RECHAZADO', 'CANCELADO', 'PENDIENTE'

        [MaxLength(100)]
        [Column("codigo_transaccion")]
        public string? CodigoTransaccion { get; set; }

        [MaxLength(255)]
        [Column("mensaje_respuesta")]
        public string? MensajeRespuesta { get; set; }

        [MaxLength(50)]
        [Column("tarjeta_marca")]
        public string? TarjetaMarca { get; set; }

        [MaxLength(4)]
        [Column("tarjeta_ultimos_digitos")]
        public string? TarjetaUltimosDigitos { get; set; }

        [MaxLength(100)]
        [Column("titular_nombre")]
        public string? TitularNombre { get; set; }

        [MaxLength(100)]
        [Column("titular_apellido")]
        public string? TitularApellido { get; set; }

        [MaxLength(150)]
        [Column("titular_correo")]
        public string? TitularCorreo { get; set; }

        [Column("fecha_pago")]
        public DateTime FechaPago { get; set; } = DateTime.UtcNow;
    }

    [Table("entrega")]
    public class Entrega
    {
        [Key]
        [Column("entrega_id")]
        public int EntregaId { get; set; }

        [Column("pedido_id")]
        public int PedidoId { get; set; }

        [ForeignKey("PedidoId")]
        public Pedido? Pedido { get; set; }

        [Column("motorizado_id")]
        public int? MotorizadoId { get; set; }

        [ForeignKey("MotorizadoId")]
        public Motorizado? Motorizado { get; set; }

        [Column("tiempo_estimado_llegada")]
        public DateTime? TiempoEstimadoLlegada { get; set; }

        [Column("fecha_asignacion")]
        public DateTime? FechaAsignacion { get; set; }

        [Column("fecha_salida")]
        public DateTime? FechaSalida { get; set; }

        [Column("fecha_entrega_real")]
        public DateTime? FechaEntregaReal { get; set; }

        [MaxLength(500)]
        [Column("observaciones")]
        public string? Observaciones { get; set; }
    }

    [Table("historial_estado_pedido")]
    public class HistorialEstadoPedido
    {
        [Key]
        [Column("historial_id")]
        public int HistorialId { get; set; }

        [Column("pedido_id")]
        public int PedidoId { get; set; }

        [ForeignKey("PedidoId")]
        public Pedido? Pedido { get; set; }

        [Column("estado_pedido_id")]
        public int EstadoPedidoId { get; set; }

        [ForeignKey("EstadoPedidoId")]
        public EstadoPedido? EstadoPedido { get; set; }

        [Column("fecha_cambio")]
        public DateTime FechaCambio { get; set; } = DateTime.UtcNow;

        [MaxLength(255)]
        [Column("comentario")]
        public string? Comentario { get; set; }
    }

    [Table("banner")]
    public class Banner
    {
        [Key]
        [Column("banner_id")]
        public int BannerId { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        [Column("imagen_url")]
        public string ImagenUrl { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("enlace_destino")]
        public string? EnlaceDestino { get; set; }

        [Column("posicion")]
        public int Posicion { get; set; } = 1;

        [Column("fecha_inicio")]
        public DateTime FechaInicio { get; set; }

        [Column("fecha_fin")]
        public DateTime FechaFin { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;
    }

    [Table("promocion")]
    public class Promocion
    {
        [Key]
        [Column("promocion_id")]
        public int PromocionId { get; set; }

        [Column("banner_id")]
        public int? BannerId { get; set; }

        [ForeignKey("BannerId")]
        public Banner? Banner { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("tipo_promocion")]
        public string TipoPromocion { get; set; } = "TAMBO_PACK";

        [Column("precio_promocional", TypeName = "decimal(10,2)")]
        public decimal? PrecioPromocional { get; set; }

        [Column("porcentaje_descuento", TypeName = "decimal(5,2)")]
        public decimal? PorcentajeDescuento { get; set; }

        [Column("cantidad_pack")]
        public int CantidadPack { get; set; } = 1;

        [Column("max_por_cliente")]
        public int MaxPorCliente { get; set; } = 3;

        [Column("fecha_inicio")]
        public DateTime FechaInicio { get; set; }

        [Column("fecha_fin")]
        public DateTime FechaFin { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;

        public ICollection<PromocionProducto> Productos { get; set; } = new List<PromocionProducto>();
    }

    [Table("promocion_producto")]
    public class PromocionProducto
    {
        [Column("promocion_id")]
        public int PromocionId { get; set; }

        [ForeignKey("PromocionId")]
        public Promocion? Promocion { get; set; }

        [Column("producto_id")]
        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public Producto? Producto { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; } = 1;
    }

    [Table("carrito")]
    public class Carrito
    {
        [Key]
        [Column("carrito_id")]
        public int CarritoId { get; set; }

        [Column("cliente_id")]
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Column("fecha_actualizacion")]
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        [Column("activo")]
        public bool Activo { get; set; } = true;

        public ICollection<DetalleCarrito> Detalles { get; set; } = new List<DetalleCarrito>();
    }

    [Table("detalle_carrito")]
    public class DetalleCarrito
    {
        [Key]
        [Column("detalle_carrito_id")]
        public int DetalleCarritoId { get; set; }

        [Column("carrito_id")]
        public int CarritoId { get; set; }

        [ForeignKey("CarritoId")]
        public Carrito? Carrito { get; set; }

        [Column("producto_id")]
        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public Producto? Producto { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; } = 1;

        [Column("precio_unitario", TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }
    }
}
