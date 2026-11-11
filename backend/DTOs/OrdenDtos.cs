namespace KemilEditorial.API.DTOs
{
    // Renglón del carrito. En POST /api/ordenes/calcular el cliente puede enviar Nombre y Precio,
    // pero el servidor los IGNORA y usa el precio de la base de datos (OrdenService).
    public class ItemCarritoDto
    {
        public int IdLibro { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
    }

    // Renglón que sí envía el cliente al cerrar la orden: solo qué libro y cuántos.
    public class ItemOrdenDto
    {
        public int IdLibro { get; set; }
        public int Cantidad { get; set; }
    }

    public class CrearOrdenRequestDto
    {
        public List<ItemOrdenDto> Items { get; set; } = new();
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

    // Respuesta de POST /api/ordenes: el desglose completo más los datos de la factura generada.
    public class OrdenCreadaDto : ResumenOrdenDto
    {
        public int IdFactura { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
    }

    public class LineaPedidoDto
    {
        public int IdLibro { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubtotalLinea { get; set; }
    }

    // Respuesta de GET /api/ordenes/mis-pedidos
    public class PedidoDto
    {
        public int IdFactura { get; set; }
        public DateTime FechaEmision { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int TotalUnidades { get; set; }
        public decimal SubtotalBase { get; set; }
        public string? DescuentoAplicado { get; set; }
        public decimal MontoDescuento { get; set; }
        public decimal Impuestos { get; set; }
        public decimal TotalFinal { get; set; }
        public List<LineaPedidoDto> Lineas { get; set; } = new();
    }
}
