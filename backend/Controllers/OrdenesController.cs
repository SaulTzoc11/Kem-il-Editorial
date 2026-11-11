using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KemilEditorial.API.DTOs;
using KemilEditorial.API.Services;

namespace KemilEditorial.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdenesController : ControllerBase
    {
        private readonly DescuentoService _descuentoService;

        public OrdenesController(DescuentoService descuentoService)
        {
            _descuentoService = descuentoService;
        }

        // Solo calcula totales, no modifica datos: queda público para que
        // el carrito funcione también para visitantes
        [HttpPost("calcular")]
        [AllowAnonymous]
        public IActionResult Calcular([FromBody] CalcularOrdenRequestDto request)
        {
            if (request.Items == null || !request.Items.Any())
            {
                return BadRequest("El carrito no contiene productos.");
            }

            var resumen = _descuentoService.CalcularOrden(request);
            return Ok(resumen);
        }
    }
}