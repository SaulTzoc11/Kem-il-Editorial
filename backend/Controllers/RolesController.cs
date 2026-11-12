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
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public RolesController(AppDbContext db) => _db = db;

        // GET api/roles -> para el selector del formulario "Nuevo usuario"
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RolDto>>> Listar()
        {
            var roles = await _db.Roles
                .AsNoTracking()
                .OrderBy(r => r.IdRol)
                .Select(r => new RolDto { IdRol = r.IdRol, NombreRol = r.NombreRol })
                .ToListAsync();

            return Ok(roles);
        }
    }
}