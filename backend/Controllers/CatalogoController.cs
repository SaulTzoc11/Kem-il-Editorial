using KemilEditorial.API.Auth;
using KemilEditorial.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KemilEditorial.API.Controllers
{
    // Listas de apoyo del catálogo: alimentan los selectores del formulario de libros.
    [ApiController]
    [Authorize(Roles = Roles.Personal)]
    public class CatalogoController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CatalogoController(AppDbContext db) => _db = db;

        // GET api/generos -> [{ idGenero, nombre }]
        [HttpGet("api/generos")]
        public async Task<IActionResult> Generos() =>
            Ok(await _db.Generos.AsNoTracking()
                .Where(g => g.Estado)
                .OrderBy(g => g.Nombre)
                .Select(g => new { g.IdGenero, g.Nombre })
                .ToListAsync());

        // GET api/editoriales -> [{ idEditorial, nombre }]
        [HttpGet("api/editoriales")]
        public async Task<IActionResult> Editoriales() =>
            Ok(await _db.Editoriales.AsNoTracking()
                .OrderBy(e => e.Nombre)
                .Select(e => new { e.IdEditorial, e.Nombre })
                .ToListAsync());
    }
}
