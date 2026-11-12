namespace KemilEditorial.API.Models
{
    // Bodega donde se guardan las existencias (tabla stock)
    public class Bodega
    {
        public int IdStock { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}
