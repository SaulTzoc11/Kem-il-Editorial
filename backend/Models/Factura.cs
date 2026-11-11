namespace KemilEditorial.API.Models
{
    // Orden de compra (tabla factura). El desglose queda guardado tal como se mostró al cliente.
    public class Factura
    {
        public int IdFactura { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal SubtotalBase { get; set; }
        public string? DescuentoAplicado { get; set; }   // detalle legible de las reglas aplicadas
        public decimal Impuestos { get; set; }
        public decimal TotalFinal { get; set; }
        public string Estado { get; set; } = "Pendiente"; // Pendiente | Aprobado | Enviado | Cancelado (RF-05.2)
        public decimal DescuentoVolumenPct { get; set; }
        public decimal DescuentoClientePct { get; set; }
        public decimal MontoDescuento { get; set; }

        public ICollection<DetalleFactura> Detalles { get; set; } = new List<DetalleFactura>();
    }
}
