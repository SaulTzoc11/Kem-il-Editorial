using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KemilEditorial.API.Auth;
using KemilEditorial.API.Data;
using KemilEditorial.API.DTOs;
using KemilEditorial.API.Models;

namespace KemilEditorial.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LibrosController : ControllerBase
    {
        private readonly AppDbContext _context; 

        public LibrosController(AppDbContext context) 
        {
            _context = context;
        }

        // El catálogo es público: cualquier visitante puede verlo
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<LibroDto>>> GetLibros()
        {
            var libros = await _context.Libros
                .Include(l => l.Genero)
                .Where(l => l.Estado)
                .Select(l => new LibroDto
                {
                    IdLibro = l.IdLibro,
                    Nombre = l.Nombre,
                    Descripcion = l.Descripcion,
                    Formato = l.Formato,
                    Precio = l.Precio,
                    ISBN = l.ISBN,
                    Portada = l.Portada,
                    NombreGenero = l.Genero != null ? l.Genero.Nombre : "Sin categoría"
                })
                .ToListAsync();

            return Ok(libros);
        }

        // Solo un Administrador puede agregar libros al catálogo
        [HttpPost]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<ActionResult> CrearLibro([FromBody] CrearLibroDto dto)
        {
            var nuevoLibro = new Libro
            {
                IdGenero = dto.IdGenero,
                IdEditorial = dto.IdEditorial,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Formato = dto.Formato,
                Precio = dto.Precio,
                ISBN = dto.ISBN,
                Estado = true
            };

            _context.Libros.Add(nuevoLibro);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Libro creado con éxito" });
        }
    }
}