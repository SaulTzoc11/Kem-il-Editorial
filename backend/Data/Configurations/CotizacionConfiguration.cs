using KemilEditorial.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KemilEditorial.API.Data.Configurations
{
    public class CotizacionConfiguration : IEntityTypeConfiguration<Cotizacion>
    {
        public void Configure(EntityTypeBuilder<Cotizacion> e)
        {
            e.ToTable("cotizaciones");
            e.HasKey(c => c.IdCotizacion);
            e.Property(c => c.IdCotizacion).HasColumnName("id_cotizacion");
            e.Property(c => c.IdUsuario).HasColumnName("id_usuario");
            e.Property(c => c.IdAsesor).HasColumnName("id_asesor");
            e.Property(c => c.TituloObra).HasColumnName("titulo_obra").HasMaxLength(200);
            e.Property(c => c.CantidadEstimada).HasColumnName("cantidad_estimada");
            e.Property(c => c.AcabadoPortada).HasColumnName("acabado_portada").HasMaxLength(100);
            e.Property(c => c.TipoPapel).HasColumnName("tipo_papel").HasMaxLength(100);
            e.Property(c => c.FechaRequeridaEntrega).HasColumnName("fecha_requerida_entrega").HasColumnType("date");
            e.Property(c => c.FechaRespuesta).HasColumnName("fecha_respuesta").HasColumnType("date");
            e.Property(c => c.FechaSolicitud).HasColumnName("fecha_solicitud").HasColumnType("date");
            e.Property(c => c.MontoCotizado).HasColumnName("monto_cotizado").HasColumnType("decimal(10,2)");
            e.Property(c => c.ObservacionesAsesor).HasColumnName("observaciones_asesor").HasColumnType("text");
            e.Property(c => c.ComentariosCliente).HasColumnName("comentarios_cliente").HasColumnType("text");
            e.Property(c => c.Estado).HasColumnName("estado").HasMaxLength(50);

            e.HasOne(c => c.Usuario).WithMany().HasForeignKey(c => c.IdUsuario);
        }
    }
}