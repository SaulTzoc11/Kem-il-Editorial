// DTOs/LibroDto.cs
namespace KemilEditorial.API.DTOs
{
    public class LibroDto
    {
        public int IdLibro { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Formato { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string ISBN { get; set; } = string.Empty;
        public string NombreGenero { get; set; } = string.Empty;
    }

    public class CrearLibroDto
    {
        public int IdGenero { get; set; }
        public int IdEditorial { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Formato { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string ISBN { get; set; } = string.Empty;
    }
}