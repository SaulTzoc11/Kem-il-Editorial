namespace KemilEditorial.API.Models
{
    // Renglón de una factura. Guarda el precio unitario del momento de la compra.
    public class DetalleFactura
    {
        public int IdDetalle { get; set; }
        public int IdFactura { get; set; }
        public int IdLibro { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubtotalLinea { get; set; }

        public Libro? Libro { get; set; }
    }
}
