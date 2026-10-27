namespace KemilEditorial.API.Models
{
    public class Libro
    {
        public int IdLibro { get; set; }
        public int IdGenero { get; set; }
        public int IdEditorial { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Formato { get; set; } = string.Empty; // "Tapa dura", "E-book", etc.
        public decimal Precio { get; set; }
        public string ISBN { get; set; } = string.Empty;
        public bool Estado { get; set; }

        // Propiedades de navegación (si se usa EF Core)
        public Genero? Genero { get; set; }
    }
}