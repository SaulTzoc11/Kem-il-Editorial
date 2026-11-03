namespace KemilEditorial.API.DTOs
{
    public class ItemCarritoDto
    {
        public int IdLibro { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
    }

    public class CalcularOrdenRequestDto
    {
        public List<ItemCarritoDto> Items { get; set; } = new();
        public string TipoCliente { get; set; } = "Minorista"; // "Minorista" o "Mayorista" / "Institucional"
    }

    public class ResumenOrdenDto
    {
        public decimal SubtotalBase { get; set; }
        public int TotalUnidades { get; set; }
        
        public decimal PorcentajeVolumen { get; set; }
        public decimal DescuentoVolumenMonto { get; set; }
        public string DetalleReglaVolumen { get; set; } = string.Empty;

        public decimal PorcentajeCliente { get; set; }
        public decimal DescuentoClienteMonto { get; set; }
        public string DetalleReglaCliente { get; set; } = string.Empty;

        public decimal TotalDescuentos { get; set; }
        public decimal SubtotalConDescuentos { get; set; }
        public decimal Impuestos { get; set; } // IVA (12%)
        public decimal TotalFinal { get; set; }
    }
}