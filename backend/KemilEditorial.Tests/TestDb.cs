using KemilEditorial.API.Auth;
using KemilEditorial.API.Data;
using KemilEditorial.API.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace KemilEditorial.Tests;

/// <summary>
/// Base de datos SQLite en memoria con el modelo real de la API y datos de prueba conocidos.
/// Cada instancia es una base nueva y aislada. <see cref="Crear"/> devuelve contextos distintos sobre
/// la misma base, para comprobar lo que realmente quedó guardado (no lo que el contexto tiene en memoria).
///
/// Datos sembrados:
///   Géneros  : 1 y 2 activos, 3 inactivo | Editoriales: 1 y 2
///   Usuarios : 1 = Cliente, 2 = Institucional, 3 = otro Cliente
///   Libros   : 1 = Q100 tapa blanda (stock 10 en bodega 1 + 5 en bodega 2 = 15)
///              2 = Q50 tapa dura (stock 3)
///              3 = Q40 E-book (sin existencias físicas)
///              4 = Q10 dado de baja (Estado = false)
///   Autores  : 1 y 2 activos, 3 inactivo | el libro 1 tiene los tres (año 2023, edición "Primera")
/// </summary>
public sealed class TestDb : IDisposable
{
    public const int Minorista = 1;
    public const int Institucional = 2;
    public const int OtroMinorista = 3;

    private readonly SqliteConnection _conexion = new("DataSource=:memory:");
    private readonly DbContextOptions<AppDbContext> _opciones;

    public TestDb()
    {
        _conexion.Open();
        _opciones = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conexion).Options;

        using var ctx = Crear();
        ctx.Database.EnsureCreated();
        Sembrar(ctx);
    }

    public AppDbContext Crear() => new ContextoSqlite(_opciones);

    public void Dispose() => _conexion.Dispose();

    // SQLite no entiende GETDATE(): se quita el valor por defecto solo en las pruebas
    private sealed class ContextoSqlite : AppDbContext
    {
        public ContextoSqlite(DbContextOptions<AppDbContext> opciones) : base(opciones) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Usuario>().Property(u => u.FechaRegistro).HasDefaultValueSql(null);
        }
    }

    private static void Sembrar(AppDbContext ctx)
    {
        ctx.Roles.Add(new Rol { IdRol = 1, NombreRol = Roles.Cliente });
        ctx.Roles.Add(new Rol { IdRol = 2, NombreRol = Roles.Institucional });
        ctx.Generos.Add(new Genero { IdGenero = 1, Nombre = "Poesía", Estado = true });
        ctx.Generos.Add(new Genero { IdGenero = 2, Nombre = "Historia", Estado = true });
        ctx.Generos.Add(new Genero { IdGenero = 3, Nombre = "Género retirado", Estado = false });
        ctx.Editoriales.Add(new Editorial { IdEditorial = 1, Nombre = "Kem'il Editorial" });
        ctx.Editoriales.Add(new Editorial { IdEditorial = 2, Nombre = "Ediciones del Altiplano" });

        ctx.Set<Bodega>().Add(new Bodega { IdStock = 1, Nombre = "Bodega Central" });
        ctx.Set<Bodega>().Add(new Bodega { IdStock = 2, Nombre = "Bodega Quetzaltenango" });

        ctx.Usuarios.Add(Usuario(Minorista, 1, "ana@prueba.com"));
        ctx.Usuarios.Add(Usuario(Institucional, 2, "biblioteca@prueba.com"));
        ctx.Usuarios.Add(Usuario(OtroMinorista, 1, "luis@prueba.com"));

        ctx.Set<Autor>().Add(new Autor { IdAutor = 1, Nombre = "Lucía", Apellido = "Tzoc", Estado = true });
        ctx.Set<Autor>().Add(new Autor { IdAutor = 2, Nombre = "Mateo", Apellido = "Xicay", Estado = true });
        ctx.Set<Autor>().Add(new Autor { IdAutor = 3, Nombre = "Autor", Apellido = "Retirado", Estado = false });

        var antologia = Libro(1, "Antología Poética", "Tapa blanda", 100m, activo: true);
        antologia.FechaLanzamiento = new DateTime(2023, 3, 15);
        antologia.Edicion = "Primera";
        antologia.Valoracion = 4.6m;
        ctx.Libros.Add(antologia);
        ctx.Libros.Add(Libro(2, "Historia y Tradición", "Tapa dura", 50m, activo: true));
        ctx.Libros.Add(Libro(3, "Atlas Digital", "E-book", 40m, activo: true));
        ctx.Libros.Add(Libro(4, "Edición agotada", "Tapa dura", 10m, activo: false));
        ctx.SaveChanges();

        ctx.Set<LibroAutor>().Add(new LibroAutor { IdLibroAutor = 1, IdLibro = 1, IdAutor = 1 });
        ctx.Set<LibroAutor>().Add(new LibroAutor { IdLibroAutor = 2, IdLibro = 1, IdAutor = 2 });
        ctx.Set<LibroAutor>().Add(new LibroAutor { IdLibroAutor = 3, IdLibro = 1, IdAutor = 3 });
        ctx.Inventarios.Add(new InventarioLibro { IdInventario = 1, IdStock = 1, IdLibro = 1, StockDisponible = 10, StockMinimo = 2 });
        ctx.Inventarios.Add(new InventarioLibro { IdInventario = 2, IdStock = 2, IdLibro = 1, StockDisponible = 5, StockMinimo = 2 });
        ctx.Inventarios.Add(new InventarioLibro { IdInventario = 3, IdStock = 1, IdLibro = 2, StockDisponible = 3, StockMinimo = 1 });
        ctx.SaveChanges();
    }

    private static Usuario Usuario(int id, int idRol, string correo) => new()
    {
        IdUsuario = id, IdRol = idRol, Nombre = correo, Correo = correo,
        PasswordHash = "x", FechaRegistro = DateTime.Today, Estado = true
    };

    private static Libro Libro(int id, string nombre, string formato, decimal precio, bool activo) => new()
    {
        IdLibro = id, IdGenero = 1, IdEditorial = 1, Nombre = nombre, Formato = formato,
        Precio = precio, ISBN = $"978-0-{id:000}", Estado = activo
    };
}
