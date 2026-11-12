using System.ComponentModel.DataAnnotations;

namespace KemilEditorial.API.DTOs
{
    public class CrearCotizacionDto
    {
        [Required, StringLength(200, MinimumLength = 2)]
        public string TituloObra { get; set; } = string.Empty;

        [Range(1, 1_000_000)]
        public int CantidadEstimada { get; set; }

        [StringLength(100)]
        public string? AcabadoPortada { get; set; }

        [StringLength(100)]
        public string? TipoPapel { get; set; }

        public DateOnly? FechaRequeridaEntrega { get; set; }

        [StringLength(2000)]
        public string? ComentariosCliente { get; set; }
    }

    public class ResponderCotizacionDto
    {
        // Respondida | Aprobada | Rechazada
        [Required]
        public string Estado { get; set; } = string.Empty;

        public decimal? MontoCotizado { get; set; }

        [StringLength(2000)]
        public string? ObservacionesAsesor { get; set; }
    }

    public class CotizacionDto
    {
        public int IdCotizacion { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string Contacto { get; set; } = string.Empty;
        public string TituloObra { get; set; } = string.Empty;
        public int CantidadEstimada { get; set; }
        public string? AcabadoPortada { get; set; }
        public string? TipoPapel { get; set; }
        public DateOnly? FechaRequeridaEntrega { get; set; }
        public DateOnly? FechaSolicitud { get; set; }
        public string? ComentariosCliente { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal? MontoCotizado { get; set; }
        public string? ObservacionesAsesor { get; set; }
    }
}