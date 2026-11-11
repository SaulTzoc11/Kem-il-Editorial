namespace KemilEditorial.API.Models
{
    // Existencias de un libro en una bodega (tabla inventario_libro). Un libro puede estar en varias bodegas.
    public class InventarioLibro
    {
        public int IdInventario { get; set; }
        public int IdStock { get; set; }          // bodega
        public int IdLibro { get; set; }
        public int StockDisponible { get; set; }
        public int StockMinimo { get; set; }
        public bool Estado { get; set; } = true;
    }
}
