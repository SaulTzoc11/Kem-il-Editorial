namespace KemilEditorial.API.Models
{
    // Autor de una obra (tabla autor)
    public class Autor
    {
        public int IdAutor { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public bool Estado { get; set; } = true;
    }

    // Relación muchos a muchos entre libro y autor (tabla libro_autor)
    public class LibroAutor
    {
        public int IdLibroAutor { get; set; }
        public int IdAutor { get; set; }
        public int IdLibro { get; set; }
    }
}
