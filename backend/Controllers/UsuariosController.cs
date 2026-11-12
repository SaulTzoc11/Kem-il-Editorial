using System.Security.Claims;
using KemilEditorial.API.Auth;
using KemilEditorial.API.Data;
using KemilEditorial.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KemilEditorial.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.Administrador)]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _db;

        public UsuariosController(AppDbContext db) => _db = db;

        private int IdActual => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET api/usuarios -> lista de cuentas (nunca incluye la contraseña)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioListaDto>>> Listar()
        {
            var lista = await _db.Usuarios
                .AsNoTracking()
                .OrderBy(u => u.IdUsuario)
                .Select(u => new UsuarioListaDto
                {
                    IdUsuario = u.IdUsuario,
                    Nombre = u.Nombre,
                    Correo = u.Correo,
                    Rol = u.Rol!.NombreRol,
                    Estado = u.Estado
                })
                .ToListAsync();

            return Ok(lista);
        }

        // PUT api/usuarios/{id}/rol
        [HttpPut("{id:int}/rol")]
        public async Task<IActionResult> CambiarRol(int id, [FromBody] CambiarRolDto dto)
        {
            if (id == IdActual)
                return BadRequest(new { mensaje = "No puedes cambiar tu propio rol." });

            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario is null)
                return NotFound(new { mensaje = "Usuario no encontrado." });

            var rol = await _db.Roles.FindAsync(dto.IdRol);
            if (rol is null)
                return BadRequest(new { mensaje = "El rol indicado no existe." });

            usuario.IdRol = rol.IdRol;
            await _db.SaveChangesAsync();

            return Ok(new { mensaje = $"Rol actualizado a {rol.NombreRol}." });
        }

        // PUT api/usuarios/{id}/estado  -> activar o desactivar la cuenta
        [HttpPut("{id:int}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoDto dto)
        {
            if (id == IdActual && !dto.Estado)
                return BadRequest(new { mensaje = "No puedes desactivar tu propia cuenta." });

            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario is null)
                return NotFound(new { mensaje = "Usuario no encontrado." });

            usuario.Estado = dto.Estado;
            await _db.SaveChangesAsync();

            return Ok(new { mensaje = dto.Estado ? "Cuenta activada." : "Cuenta desactivada." });
        }
    }
}