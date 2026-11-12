namespace KemilEditorial.API.Models
{
    // Pedido especial (tabla cotizaciones)
    public class Cotizacion
    {
        public int IdCotizacion { get; set; }
        public int IdUsuario { get; set; }
        public int? IdAsesor { get; set; }
        public string TituloObra { get; set; } = string.Empty;
        public int CantidadEstimada { get; set; }
        public string? AcabadoPortada { get; set; }
        public string? TipoPapel { get; set; }
        public DateOnly? FechaRequeridaEntrega { get; set; }
        public DateOnly? FechaRespuesta { get; set; }
        public DateOnly? FechaSolicitud { get; set; }
        public decimal? MontoCotizado { get; set; }
        public string? ObservacionesAsesor { get; set; }
        public string? ComentariosCliente { get; set; }
        public string Estado { get; set; } = "Pendiente";

        public Usuario? Usuario { get; set; }
    }
}