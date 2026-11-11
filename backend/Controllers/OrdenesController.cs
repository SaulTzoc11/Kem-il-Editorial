using System.Security.Claims;
using KemilEditorial.API.Auth;
using KemilEditorial.API.DTOs;
using KemilEditorial.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KemilEditorial.API.Controllers
{
    // Controlador delgado: lee la petición y el token, llama al Modelo (OrdenService) y traduce el resultado a HTTP.
    // No calcula descuentos ni toca la base de datos.
    [ApiController]
    [Route("api/[controller]")]
    public class OrdenesController : ControllerBase
    {
        private readonly OrdenService _ordenes;

        public OrdenesController(OrdenService ordenes)
        {
            _ordenes = ordenes;
        }

        // POST api/ordenes/calcular -> vista previa del carrito. Público: el precio sale de la BD, no del cliente.
        [HttpPost("calcular")]
        [AllowAnonymous]
        public async Task<ActionResult<ResumenOrdenDto>> Calcular([FromBody] CalcularOrdenRequestDto request)
        {
            try
            {
                var items = request.Items.Select(i => new ItemOrdenDto { IdLibro = i.IdLibro, Cantidad = i.Cantidad });
                return Ok(await _ordenes.CalcularAsync(items, request.TipoCliente));
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // POST api/ordenes -> cierre de orden (RF-03.3). Solo clientes con sesión.
        [HttpPost]
        [Authorize(Roles = Roles.Compradores)]
        public async Task<ActionResult<OrdenCreadaDto>> Crear([FromBody] CrearOrdenRequestDto request)
        {
            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            string rol = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            try
            {
                return Ok(await _ordenes.CrearAsync(idUsuario, rol, request));
            }
            catch (StockInsuficienteException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (ReglaNegocioException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // GET api/ordenes/mis-pedidos -> historial del cliente autenticado
        [HttpGet("mis-pedidos")]
        [Authorize(Roles = Roles.Compradores)]
        public async Task<ActionResult<List<PedidoDto>>> MisPedidos()
        {
            int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _ordenes.MisPedidosAsync(idUsuario));
        }
    }
}
