using KemilEditorial.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KemilEditorial.API.Data.Configurations
{
    // Datos de la ficha técnica del libro (autor, año, edición, valoración). Vive aparte de AppDbContext
    // para no mezclarse con el mapeo de libros y usuarios (menos conflictos al trabajar en equipo).

    public class LibroFichaConfiguration : IEntityTypeConfiguration<Libro>
    {
        public void Configure(EntityTypeBuilder<Libro> e)
        {
            e.Property(l => l.FechaLanzamiento).HasColumnName("fecha_lanzamiento").HasColumnType("date");
            e.Property(l => l.Edicion).HasColumnName("edicion").HasMaxLength(50);
            e.Property(l => l.Valoracion).HasColumnName("valoracion").HasColumnType("decimal(3,2)");
        }
    }

    public class AutorConfiguration : IEntityTypeConfiguration<Autor>
    {
        public void Configure(EntityTypeBuilder<Autor> e)
        {
            e.ToTable("autor");
            e.HasKey(a => a.IdAutor);
            e.Property(a => a.IdAutor).HasColumnName("id_autor");
            e.Property(a => a.Nombre).HasColumnName("nombre").HasMaxLength(100);
            e.Property(a => a.Apellido).HasColumnName("apellido").HasMaxLength(100);
            e.Property(a => a.Estado).HasColumnName("estado");
        }
    }

    public class BodegaConfiguration : IEntityTypeConfiguration<Bodega>
    {
        public void Configure(EntityTypeBuilder<Bodega> e)
        {
            e.ToTable("stock");
            e.HasKey(b => b.IdStock);
            e.Property(b => b.IdStock).HasColumnName("id_stock");
            e.Property(b => b.Nombre).HasColumnName("nombre").HasMaxLength(100);
        }
    }

    public class LibroAutorConfiguration : IEntityTypeConfiguration<LibroAutor>
    {
        public void Configure(EntityTypeBuilder<LibroAutor> e)
        {
            e.ToTable("libro_autor");
            e.HasKey(la => la.IdLibroAutor);
            e.Property(la => la.IdLibroAutor).HasColumnName("id_libro_autor");
            e.Property(la => la.IdAutor).HasColumnName("id_autor");
            e.Property(la => la.IdLibro).HasColumnName("id_libro");
        }
    }
}
