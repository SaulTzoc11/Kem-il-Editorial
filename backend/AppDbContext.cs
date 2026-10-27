using Microsoft.EntityFrameworkCore;
using KemilEditorial.API.Models;

namespace KemilEditorial.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Libro> Libros { get; set; }
        public DbSet<Genero> Generos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Mapeo exacto de la tabla y columnas de Libro
            modelBuilder.Entity<Libro>().ToTable("libro");
            modelBuilder.Entity<Libro>().HasKey(l => l.IdLibro);
            modelBuilder.Entity<Libro>().Property(l => l.IdLibro).HasColumnName("id_libro");
            modelBuilder.Entity<Libro>().Property(l => l.IdGenero).HasColumnName("id_genero");
            modelBuilder.Entity<Libro>().Property(l => l.IdEditorial).HasColumnName("id_editorial");
            modelBuilder.Entity<Libro>().Property(l => l.Nombre).HasColumnName("nombre");
            modelBuilder.Entity<Libro>().Property(l => l.Descripcion).HasColumnName("descripcion");
            modelBuilder.Entity<Libro>().Property(l => l.Formato).HasColumnName("formato");
            modelBuilder.Entity<Libro>().Property(l => l.Precio).HasColumnName("precio");
            modelBuilder.Entity<Libro>().Property(l => l.ISBN).HasColumnName("ISBN");
            modelBuilder.Entity<Libro>().Property(l => l.Estado).HasColumnName("estado");

            // Mapeo exacto de la tabla y columnas de Genero
            modelBuilder.Entity<Genero>().ToTable("genero");
            modelBuilder.Entity<Genero>().HasKey(g => g.IdGenero);
            modelBuilder.Entity<Genero>().Property(g => g.IdGenero).HasColumnName("id_genero");
            modelBuilder.Entity<Genero>().Property(g => g.Nombre).HasColumnName("nombre");

            // Relación
            modelBuilder.Entity<Libro>()
                .HasOne(l => l.Genero)
                .WithMany()
                .HasForeignKey(l => l.IdGenero);
        }
    }
}