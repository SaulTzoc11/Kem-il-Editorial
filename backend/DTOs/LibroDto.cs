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
        public int IdGenero { get; set; }
        public int IdEditorial { get; set; }
        public string NombreGenero { get; set; } = string.Empty;
        public string? Portada { get; set; }

        // Ficha técnica (RF-01.3)
        public string Autor { get; set; } = string.Empty;       // varios autores separados por coma
        public string Editorial { get; set; } = string.Empty;
        public int? AnioEdicion { get; set; }
        public string? Edicion { get; set; }
        public decimal? Valoracion { get; set; }
        public int? Stock { get; set; }                         // null = libro digital, sin existencias físicas
        public int StockMinimo { get; set; }
    }

    // Cuerpo de POST (crear) y PUT (editar) api/libros. La portada se sube aparte: POST api/libros/{id}/portada
    public class CrearLibroDto
    {
        public int IdGenero { get; set; }
        public int IdEditorial { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Formato { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string ISBN { get; set; } = string.Empty;

        // Opcionales: si no se envían (null) no se modifican. Stock = existencias totales del libro.
        // Autor: uno o varios nombres separados por coma ("Lucía Tzoc, Mateo Xicay"); vacío = sin autor.
        public int? Stock { get; set; }
        public string? Autor { get; set; }
    }
}
