using System.Linq.Expressions;
using System.Security.Claims;
using KemilEditorial.API.Auth;
using KemilEditorial.API.Data;
using KemilEditorial.API.DTOs;
using KemilEditorial.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KemilEditorial.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CotizacionesController : ControllerBase
    {
        private readonly AppDbContext _db;

        private static readonly string[] EstadosRespuesta = { "Respondida", "Aprobada", "Rechazada" };
                // Valores tal como se guardan en la base de datos (CK_cotizaciones_estado)
        private static readonly string[] EstadosCerrados = { "Cotizada", "Rechazada" };

        // Proyección compartida entre bandeja, creación y respuesta
               private static readonly Expression<Func<Cotizacion, CotizacionDto>> ADto = c => new CotizacionDto
        {
            IdCotizacion = c.IdCotizacion,
            Cliente = c.Usuario!.Nombre,
            Contacto = c.Usuario!.Correo,
            TituloObra = c.TituloObra,
            CantidadEstimada = c.CantidadEstimada,
            AcabadoPortada = c.AcabadoPortada,
            TipoPapel = c.TipoPapel,
            FechaRequeridaEntrega = c.FechaRequeridaEntrega,
            FechaSolicitud = c.FechaSolicitud,
            ComentariosCliente = c.ComentariosCliente,
            Estado = c.Estado == "Cotizada" ? "Respondida"
                   : c.Estado == "En revision" ? "En revisión"
                   : c.Estado,
            MontoCotizado = c.MontoCotizado,
            ObservacionesAsesor = c.ObservacionesAsesor
        };

        public CotizacionesController(AppDbContext db) => _db = db;

        private int IdActual => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // POST api/cotizaciones -> un cliente Mayorista o Institucional solicita un pedido especial
        [HttpPost]
        [Authorize(Roles = Roles.SolicitantesCotizacion)]
        public async Task<ActionResult<CotizacionDto>> Crear([FromBody] CrearCotizacionDto dto)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            if (dto.FechaRequeridaEntrega is { } fecha && fecha < hoy)
                return BadRequest(new { mensaje = "La fecha requerida de entrega no puede estar en el pasado." });

            var cotizacion = new Cotizacion
            {
                IdUsuario = IdActual,          // sale del token, nunca del cuerpo de la petición
                TituloObra = dto.TituloObra.Trim(),
                CantidadEstimada = dto.CantidadEstimada,
                AcabadoPortada = dto.AcabadoPortada,
                TipoPapel = dto.TipoPapel,
                FechaRequeridaEntrega = dto.FechaRequeridaEntrega,
                ComentariosCliente = dto.ComentariosCliente,
                FechaSolicitud = hoy,
                Estado = "Pendiente"
            };

            _db.Cotizaciones.Add(cotizacion);
            await _db.SaveChangesAsync();

            var creada = await _db.Cotizaciones.AsNoTracking()
                .Where(c => c.IdCotizacion == cotizacion.IdCotizacion)
                .Select(ADto)
                .FirstAsync();

            return Ok(creada);
        }

        // GET api/cotizaciones/bandeja -> solicitudes para el Asesor / Administrador
        [HttpGet("bandeja")]
        [Authorize(Roles = Roles.Personal)]
        public async Task<ActionResult<List<CotizacionDto>>> Bandeja()
        {
            var lista = await _db.Cotizaciones.AsNoTracking()
                .OrderByDescending(c => c.FechaSolicitud)
                .ThenByDescending(c => c.IdCotizacion)
                .Select(ADto)
                .ToListAsync();

            return Ok(lista);
        }

        // PUT api/cotizaciones/{id}/responder
        [HttpPut("{id:int}/responder")]
        [Authorize(Roles = Roles.Personal)]
        public async Task<ActionResult<CotizacionDto>> Responder(int id, [FromBody] ResponderCotizacionDto dto)
        {
            if (!EstadosRespuesta.Contains(dto.Estado))
                return BadRequest(new { mensaje = "Estado inválido. Usa Respondida, Aprobada o Rechazada." });

            if (dto.Estado == "Respondida" && !(dto.MontoCotizado > 0))
                return BadRequest(new { mensaje = "Para responder con una cotización indica un monto mayor que cero." });

            var cotizacion = await _db.Cotizaciones.FindAsync(id);
            if (cotizacion is null)
                return NotFound(new { mensaje = "Cotización no encontrada." });

            if (EstadosCerrados.Contains(cotizacion.Estado))
                return Conflict(new { mensaje = "Esta cotización ya está cerrada y no se puede modificar." });

                   // El panel envía "Respondida"; la base de datos lo guarda como "Cotizada"
        cotizacion.Estado = dto.Estado == "Respondida" ? "Cotizada" : dto.Estado;
            cotizacion.MontoCotizado = dto.MontoCotizado;
            cotizacion.ObservacionesAsesor = dto.ObservacionesAsesor;
            cotizacion.IdAsesor = IdActual;
            cotizacion.FechaRespuesta = DateOnly.FromDateTime(DateTime.Today);
            await _db.SaveChangesAsync();

            var actualizada = await _db.Cotizaciones.AsNoTracking()
                .Where(c => c.IdCotizacion == id)
                .Select(ADto)
                .FirstAsync();

            return Ok(actualizada);
        }
    }
}