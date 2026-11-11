using KemilEditorial.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KemilEditorial.API.Data.Configurations
{
    // Mapeo de las tablas de facturación e inventario. Vive aparte para no mezclarse con
    // el mapeo de libros y usuarios de AppDbContext (menos conflictos al trabajar en equipo).
    public class FacturaConfiguration : IEntityTypeConfiguration<Factura>
    {
        public void Configure(EntityTypeBuilder<Factura> e)
        {
            e.ToTable("factura");
            e.HasKey(f => f.IdFactura);
            e.Property(f => f.IdFactura).HasColumnName("id_factura");
            e.Property(f => f.IdUsuario).HasColumnName("id_usuario");
            e.Property(f => f.FechaEmision).HasColumnName("fecha_emision").HasColumnType("date");
            e.Property(f => f.SubtotalBase).HasColumnName("subtotal_base").HasColumnType("decimal(10,2)");
            e.Property(f => f.DescuentoAplicado).HasColumnName("descuento_aplicado").HasMaxLength(100);
            e.Property(f => f.Impuestos).HasColumnName("impuestos").HasColumnType("decimal(10,2)");
            e.Property(f => f.TotalFinal).HasColumnName("total_final").HasColumnType("decimal(10,2)");
            e.Property(f => f.Estado).HasColumnName("estado").HasMaxLength(50);
            e.Property(f => f.DescuentoVolumenPct).HasColumnName("descuento_volumen_pct").HasColumnType("decimal(5,2)");
            e.Property(f => f.DescuentoClientePct).HasColumnName("descuento_cliente_pct").HasColumnType("decimal(5,2)");
            e.Property(f => f.MontoDescuento).HasColumnName("monto_descuento").HasColumnType("decimal(10,2)");

            e.HasOne<Usuario>().WithMany().HasForeignKey(f => f.IdUsuario);
            e.HasMany(f => f.Detalles).WithOne().HasForeignKey(d => d.IdFactura);
        }
    }

    public class DetalleFacturaConfiguration : IEntityTypeConfiguration<DetalleFactura>
    {
        public void Configure(EntityTypeBuilder<DetalleFactura> e)
        {
            e.ToTable("detalle_factura");
            e.HasKey(d => d.IdDetalle);
            e.Property(d => d.IdDetalle).HasColumnName("id_detalle");
            e.Property(d => d.IdFactura).HasColumnName("id_factura");
            e.Property(d => d.IdLibro).HasColumnName("id_libro");
            e.Property(d => d.Cantidad).HasColumnName("cantidad");
            e.Property(d => d.PrecioUnitario).HasColumnName("precio_unitario").HasColumnType("decimal(10,2)");
            e.Property(d => d.SubtotalLinea).HasColumnName("subtotal_linea").HasColumnType("decimal(10,2)");

            e.HasOne(d => d.Libro).WithMany().HasForeignKey(d => d.IdLibro);
        }
    }

    public class InventarioLibroConfiguration : IEntityTypeConfiguration<InventarioLibro>
    {
        public void Configure(EntityTypeBuilder<InventarioLibro> e)
        {
            e.ToTable("inventario_libro");
            e.HasKey(i => i.IdInventario);
            e.Property(i => i.IdInventario).HasColumnName("id_inventario");
            e.Property(i => i.IdStock).HasColumnName("id_stock");
            e.Property(i => i.IdLibro).HasColumnName("id_libro");
            e.Property(i => i.StockDisponible).HasColumnName("stock_disponible");
            e.Property(i => i.StockMinimo).HasColumnName("stock_minimo");
            e.Property(i => i.Estado).HasColumnName("estado");

            e.HasOne<Libro>().WithMany().HasForeignKey(i => i.IdLibro);
        }
    }
}
