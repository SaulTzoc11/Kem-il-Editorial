using System.ComponentModel.DataAnnotations;

namespace KemilEditorial.API.DTOs
{
    public class RegistroDto
    {
        [Required, StringLength(150, MinimumLength = 2)]
        public string Nombre { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(150)]
        public string Correo { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;
    }

    // Solo lo usa un Administrador para crear asesores u otros administradores
    public class CrearUsuarioAdminDto : RegistroDto
    {
        [Required]
        public int IdRol { get; set; }
    }

    public class LoginDto
    {
        [Required, EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class UsuarioDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expira { get; set; }
        public UsuarioDto Usuario { get; set; } = new();
    }
}