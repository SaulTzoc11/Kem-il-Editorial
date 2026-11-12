using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KemilEditorial.API.Auth;
using KemilEditorial.API.DTOs;
using KemilEditorial.API.Models;

namespace KemilEditorial.API.Controllers
{
    // Controlador delgado: recibe la petición, llama al Modelo (LibroService) y traduce el resultado a HTTP.
    // Las reglas de validación viven en LibroService.
    [ApiController]
    [Route("api/[controller]")]
    public class LibrosController : ControllerBase
    {
        private readonly LibroService _libros;

        public LibrosController(LibroService libros)
        {
            _libros = libros;
        }

        // El catálogo es público: cualquier visitante puede verlo
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<LibroDto>>> GetLibros() =>
            Ok(await _libros.ListarAsync());

        // Solo un Administrador puede agregar libros al catálogo
        [HttpPost]
        [Authorize(Roles = Roles.Administrador)]
        public Task<ActionResult<LibroDto>> CrearLibro([FromBody] CrearLibroDto dto) =>
            Ejecutar(async () =>
            {
                var libro = await _libros.CrearAsync(dto);
                return Created($"api/libros/{libro.IdLibro}", libro);
            });

        // PUT api/libros/{id} -> editar la ficha del libro
        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.Administrador)]
        public Task<ActionResult<LibroDto>> ActualizarLibro(int id, [FromBody] CrearLibroDto dto) =>
            Ejecutar(async () => Ok(await _libros.ActualizarAsync(id, dto)));

        // DELETE api/libros/{id} -> eliminación lógica
        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Administrador)]
        public async Task<IActionResult> EliminarLibro(int id)
        {
            try
            {
                await _libros.EliminarAsync(id);
                return NoContent();
            }
            catch (NoEncontradoException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        // POST api/libros/{id}/portada (multipart/form-data, campo "archivo") -> sube la imagen de portada
        [HttpPost("{id:int}/portada")]
        [Authorize(Roles = Roles.Administrador)]
        [RequestSizeLimit(3 * 1024 * 1024)]
        public Task<ActionResult<LibroDto>> SubirPortada(int id, [FromForm] IFormFile? archivo) =>
            Ejecutar(async () =>
            {
                if (archivo is null)
                    return BadRequest(new { mensaje = "Selecciona una imagen de portada." });

                await using var contenido = archivo.OpenReadStream();
                return Ok(await _libros.GuardarPortadaAsync(id, contenido, archivo.Length));
            });

        // Traduce las excepciones del Modelo a respuestas HTTP
        private static async Task<ActionResult<LibroDto>> Ejecutar(Func<Task<ActionResult<LibroDto>>> accion)
        {
            try
            {
                return await accion();
            }
            catch (NoEncontradoException ex)
            {
                return new NotFoundObjectResult(new { mensaje = ex.Message });
            }
            catch (ReglaNegocioException ex)
            {
                return new BadRequestObjectResult(new { mensaje = ex.Message });
            }
        }
    }
}
