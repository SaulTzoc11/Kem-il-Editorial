using Microsoft.EntityFrameworkCore;
using KemilEditorial.API.Models;

namespace KemilEditorial.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Libro> Libros { get; set; }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<Editorial> Editoriales { get; set; }
                public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }

        // Facturación e inventario (mapeo en Data/Configurations)
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<DetalleFactura> DetallesFactura { get; set; }
         public DbSet<InventarioLibro> Inventarios { get; set; }
          public DbSet<Cotizacion> Cotizaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplica todas las clases IEntityTypeConfiguration<> del proyecto (Data/Configurations)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            
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
            modelBuilder.Entity<Libro>().Property(l => l.Portada).HasColumnName("portada");
            modelBuilder.Entity<Libro>().Property(l => l.Estado).HasColumnName("estado");

            // Mapeo exacto de la tabla y columnas de Genero
            modelBuilder.Entity<Genero>().ToTable("genero");
            modelBuilder.Entity<Genero>().HasKey(g => g.IdGenero);
            modelBuilder.Entity<Genero>().Property(g => g.IdGenero).HasColumnName("id_genero");
            modelBuilder.Entity<Genero>().Property(g => g.Nombre).HasColumnName("nombre");

            // Mapeo de la tabla editorial
            modelBuilder.Entity<Editorial>().ToTable("editorial");
            modelBuilder.Entity<Editorial>().HasKey(e => e.IdEditorial);
            modelBuilder.Entity<Editorial>().Property(e => e.IdEditorial).HasColumnName("id_editorial");
            modelBuilder.Entity<Editorial>().Property(e => e.Nombre).HasColumnName("nombre");

            // Relación
            modelBuilder.Entity<Libro>()
                .HasOne(l => l.Genero)
                .WithMany()
                .HasForeignKey(l => l.IdGenero);
                            // ---- Autenticación: tablas rol y usuarios ----
            modelBuilder.Entity<Rol>().ToTable("rol");
            modelBuilder.Entity<Rol>().HasKey(r => r.IdRol);
            modelBuilder.Entity<Rol>().Property(r => r.IdRol).HasColumnName("id_rol");
            modelBuilder.Entity<Rol>().Property(r => r.NombreRol).HasColumnName("nombre_rol");
            modelBuilder.Entity<Rol>().Property(r => r.Permisos).HasColumnName("permisos");

            modelBuilder.Entity<Usuario>().ToTable("usuarios");
            modelBuilder.Entity<Usuario>().HasKey(u => u.IdUsuario);
            modelBuilder.Entity<Usuario>().Property(u => u.IdUsuario).HasColumnName("id_usuario");
            modelBuilder.Entity<Usuario>().Property(u => u.IdRol).HasColumnName("id_rol");
            modelBuilder.Entity<Usuario>().Property(u => u.Nombre).HasColumnName("nombre");
            modelBuilder.Entity<Usuario>().Property(u => u.Correo).HasColumnName("correo");
            modelBuilder.Entity<Usuario>().Property(u => u.PasswordHash).HasColumnName("password_hash");
            modelBuilder.Entity<Usuario>().Property(u => u.FechaRegistro)
                .HasColumnName("fecha_registro").HasColumnType("date").HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Usuario>().Property(u => u.Estado).HasColumnName("estado");
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol).WithMany().HasForeignKey(u => u.IdRol);
        }
    }
}