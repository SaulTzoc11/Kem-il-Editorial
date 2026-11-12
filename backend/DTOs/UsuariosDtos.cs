using System.ComponentModel.DataAnnotations;

namespace KemilEditorial.API.DTOs
{
    public class UsuarioListaDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Estado { get; set; }
    }

    public class RolDto
    {
        public int IdRol { get; set; }
        public string NombreRol { get; set; } = string.Empty;
    }

    public class CambiarRolDto
    {
        [Required]
        public int IdRol { get; set; }
    }

    public class CambiarEstadoDto
    {
        [Required]
        public bool Estado { get; set; }
    }
}