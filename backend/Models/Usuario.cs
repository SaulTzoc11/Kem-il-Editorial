namespace KemilEditorial.API.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public int IdRol { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // columna "contraseña"
        public DateTime FechaRegistro { get; set; }
        public bool Estado { get; set; } = true;

        public Rol? Rol { get; set; }
    }
}