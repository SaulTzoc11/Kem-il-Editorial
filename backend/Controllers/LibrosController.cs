using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet]
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
                    NombreGenero = l.Genero != null ? l.Genero.Nombre : "Sin categoría"
                })
                .ToListAsync();

            return Ok(libros);
        }

        [HttpPost]
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