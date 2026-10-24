using Microsoft.EntityFrameworkCore;

namespace KemilEditorial.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Aquí se agregará los DbSet para las tablas, por ejemplo:
        // public DbSet<Libro> Libros { get; set; }
    }
}