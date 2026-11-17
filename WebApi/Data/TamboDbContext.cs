using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Data
{
    public class TamboDbContext : DbContext
    {
        public TamboDbContext(DbContextOptions<TamboDbContext> options) : base(options)
        {
        }

        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Direccion> Direcciones => Set<Direccion>();
        public DbSet<ZonaDespacho> ZonasDespacho => Set<ZonaDespacho>();
        public DbSet<Tienda> Tiendas => Set<Tienda>();
        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Marca> Marcas => Set<Marca>();
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Inventario> Inventarios => Set<Inventario>();
        public DbSet<TipoComprobante> TiposComprobante => Set<TipoComprobante>();
        public DbSet<MetodoPago> MetodosPago => Set<MetodoPago>();
        public DbSet<EstadoPedido> EstadosPedido => Set<EstadoPedido>();
        public DbSet<Motorizado> Motorizados => Set<Motorizado>();
        public DbSet<Cupon> Cupones => Set<Cupon>();
        public DbSet<Pedido> Pedidos => Set<Pedido>();
        public DbSet<DetallePedido> DetallesPedido => Set<DetallePedido>();
        public DbSet<Pago> Pagos => Set<Pago>();
        public DbSet<Entrega> Entregas => Set<Entrega>();
        public DbSet<HistorialEstadoPedido> HistorialesEstadoPedido => Set<HistorialEstadoPedido>();
        public DbSet<Banner> Banners => Set<Banner>();
        public DbSet<Promocion> Promociones => Set<Promocion>();
        public DbSet<PromocionProducto> PromocionesProductos => Set<PromocionProducto>();
        public DbSet<Carrito> Carritos => Set<Carrito>();
        public DbSet<DetalleCarrito> DetallesCarrito => Set<DetalleCarrito>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Clave compuesta PromocionProducto
            modelBuilder.Entity<PromocionProducto>()
                .HasKey(pp => new { pp.PromocionId, pp.ProductoId });

            // Unicidad Carrito - Producto
            modelBuilder.Entity<DetalleCarrito>()
                .HasIndex(dc => new { dc.CarritoId, dc.ProductoId })
                .IsUnique();

            // Unicidad Cliente
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => new { c.TipoDocumento, c.NumeroDocumento })
                .IsUnique();

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Correo)
                .IsUnique();

            // Unicidad Inventario por Tienda y Producto
            modelBuilder.Entity<Inventario>()
                .HasIndex(i => new { i.TiendaId, i.ProductoId })
                .IsUnique();

            // Restricción única número de pedido
            modelBuilder.Entity<Pedido>()
                .HasIndex(p => p.NumeroPedido)
                .IsUnique();

            // Restricción de borrado lógico por defecto en consultas si se desea o filtros globales
            modelBuilder.Entity<MetodoPago>()
                .HasIndex(m => m.Codigo)
                .IsUnique();

            modelBuilder.Entity<TipoComprobante>()
                .HasIndex(t => t.Codigo)
                .IsUnique();

            modelBuilder.Entity<EstadoPedido>()
                .HasIndex(e => e.Codigo)
                .IsUnique();
        }
    }
}
