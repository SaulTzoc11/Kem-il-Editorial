// Models/Genero.cs
namespace KemilEditorial.API.Models
{
    public class Genero
    {
        public int IdGenero { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Estado { get; set; }
    }
}