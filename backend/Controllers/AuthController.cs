using System.Security.Claims;
using KemilEditorial.API.Auth;
using KemilEditorial.API.Data;
using KemilEditorial.API.DTOs;
using KemilEditorial.API.Models;
using KemilEditorial.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace KemilEditorial.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly TokenService _tokens;

        public AuthController(AppDbContext db, TokenService tokens)
        {
            _db = db;
            _tokens = tokens;
        }

        // POST api/auth/registro -> registro público, SIEMPRE crea rol "Cliente"
        [HttpPost("registro")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Registro([FromBody] RegistroDto dto)
        {
            var rolCliente = await _db.Roles.FirstOrDefaultAsync(r => r.NombreRol == Roles.Cliente);
            if (rolCliente is null)
                return Problem("El rol 'Cliente' no existe en la base de datos.");

            var (usuario, error) = await CrearUsuario(dto, rolCliente);
            if (error is not null) return Conflict(new { mensaje = error });

            return Ok(Respuesta(usuario!, rolCliente.NombreRol));
        }

        // POST api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            var correo = dto.Correo.Trim().ToLowerInvariant();

            var usuario = await _db.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == correo);

            // Mismo mensaje si el correo no existe o la contraseña falla
            var valido = usuario is not null
                         && usuario.Estado
                         && BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);

            if (!valido)
                return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });

            return Ok(Respuesta(usuario!, usuario!.Rol!.NombreRol));
        }

        // GET api/auth/me -> quién soy según mi token
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UsuarioDto>> Me()
        {
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var u = await _db.Usuarios.Include(x => x.Rol).FirstOrDefaultAsync(x => x.IdUsuario == id);
            if (u is null || !u.Estado) return Unauthorized();

            return Ok(new UsuarioDto { IdUsuario = u.IdUsuario, Nombre = u.Nombre, Correo = u.Correo, Rol = u.Rol!.NombreRol });
        }

        // POST api/auth/usuarios -> solo Administrador crea asesores u otros admins
        [HttpPost("usuarios")]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<ActionResult<UsuarioDto>> CrearUsuarioAdmin([FromBody] CrearUsuarioAdminDto dto)
        {
            var rol = await _db.Roles.FindAsync(dto.IdRol);
            if (rol is null) return BadRequest(new { mensaje = "El rol indicado no existe." });

            var (usuario, error) = await CrearUsuario(dto, rol);
            if (error is not null) return Conflict(new { mensaje = error });

            return Ok(new UsuarioDto { IdUsuario = usuario!.IdUsuario, Nombre = usuario.Nombre, Correo = usuario.Correo, Rol = rol.NombreRol });
        }

        // ---------- helpers ----------
        private async Task<(Usuario? usuario, string? error)> CrearUsuario(RegistroDto dto, Rol rol)
        {
            var correo = dto.Correo.Trim().ToLowerInvariant();

            if (await _db.Usuarios.AnyAsync(u => u.Correo == correo))
                return (null, "Ya existe una cuenta con ese correo.");

            var usuario = new Usuario
            {
                IdRol = rol.IdRol,
                Nombre = dto.Nombre.Trim(),
                Correo = correo,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12),
                Estado = true
            };

            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();
            return (usuario, null);
        }

        private AuthResponseDto Respuesta(Usuario u, string nombreRol)
        {
            var (token, expira) = _tokens.Generar(u, nombreRol);
            return new AuthResponseDto
            {
                Token = token,
                Expira = expira,
                Usuario = new UsuarioDto { IdUsuario = u.IdUsuario, Nombre = u.Nombre, Correo = u.Correo, Rol = nombreRol }
            };
        }
    }
}