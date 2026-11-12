using KemilEditorial.API.Auth;
using KemilEditorial.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KemilEditorial.API.Controllers
{
    // Panel del administrador (RF-05): métricas del dashboard y monitoreo de órdenes.
    // Solo lectura. Solo rol Administrador.
    [ApiController]
    [Authorize(Roles = Roles.Administrador)]
    public class AdminController : ControllerBase
    {
        private static readonly string[] Meses =
            { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

        private readonly AppDbContext _db;

        public AdminController(AppDbContext db) => _db = db;

        // GET api/admin/resumen -> formato que espera AdminPortal (mockResumen)
        [HttpGet("api/admin/resumen")]
        public async Task<IActionResult> Resumen()
        {
            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var inicioMesAnterior = inicioMes.AddMonths(-1);
            var inicioSemestre = inicioMes.AddMonths(-5);
            var inicioSemestreAnterior = inicioMes.AddMonths(-11);

            // Ventas = órdenes no canceladas, últimos 12 meses (se agrupa en memoria)
            var ventas = await _db.Facturas
                .Where(f => f.FechaEmision >= inicioSemestreAnterior && f.Estado != "Cancelado")
                .Select(f => new { f.FechaEmision, f.TotalFinal })
                .ToListAsync();

            decimal Suma(DateTime desde, DateTime hasta) =>
                ventas.Where(v => v.FechaEmision >= desde && v.FechaEmision < hasta).Sum(v => v.TotalFinal);

            var manana = hoy.AddDays(1);
            var ventasMes = Suma(inicioMes, manana);
            var ventasMesAnterior = Suma(inicioMesAnterior, inicioMes);
            var ventasSemestre = Suma(inicioSemestre, manana);
            var ventasSemestreAnterior = Suma(inicioSemestreAnterior, inicioSemestre);

            var porMes = Enumerable.Range(0, 6)
                .Select(i => inicioSemestre.AddMonths(i))
                .Select(m => new { Mes = Meses[m.Month - 1], Total = Suma(m, m.AddMonths(1)) })
                .ToList();
            var maximo = porMes.Max(m => m.Total);

            var pedidosActivos = await _db.Facturas.CountAsync(f => f.Estado == "Pendiente" || f.Estado == "Aprobado");
            var pedidosAtencion = await _db.Facturas.CountAsync(f => f.Estado == "Pendiente");
            var usuarios = await _db.Usuarios.CountAsync(u => u.Estado);
            var usuariosNuevos = await _db.Usuarios.CountAsync(u => u.FechaRegistro >= inicioMes);

            return Ok(new
            {
                ventasMes,
                variacionVentas = Variacion(ventasMes, ventasMesAnterior),
                pedidosActivos,
                pedidosAtencion,
                usuarios,
                usuariosNuevos,
                ventasSemestre,
                variacionSemestre = Variacion(ventasSemestre, ventasSemestreAnterior),
                // valor = altura de la barra en % respecto al mejor mes
                ventasPorMes = porMes.Select(m => new
                {
                    mes = m.Mes,
                    valor = maximo > 0 ? Math.Round(m.Total / maximo * 100, 0) : 0
                })
            });
        }

        // GET api/ordenes?estado=Pendiente -> monitoreo de órdenes (RF-05.2)
        [HttpGet("api/ordenes")]
        public async Task<IActionResult> Ordenes([FromQuery] string? estado)
        {
            var consulta = _db.Facturas.AsQueryable();
            if (!string.IsNullOrWhiteSpace(estado))
                consulta = consulta.Where(f => f.Estado == estado);

            var lista = await (
                from f in consulta
                join u in _db.Usuarios on f.IdUsuario equals u.IdUsuario
                join r in _db.Roles on u.IdRol equals r.IdRol
                orderby f.IdFactura descending
                select new
                {
                    idFactura = f.IdFactura,
                    cliente = u.Nombre,
                    rol = r.NombreRol,
                    total = f.TotalFinal,
                    estado = f.Estado,
                    fecha = f.FechaEmision
                }).ToListAsync();

            // El frontend muestra "Minorista" para el rol Cliente
            return Ok(lista.Select(o => new
            {
                o.idFactura,
                o.cliente,
                tipo = o.rol == Roles.Cliente ? "Minorista" : o.rol,
                o.total,
                o.estado,
                o.fecha
            }));
        }

        private static decimal Variacion(decimal actual, decimal anterior) =>
            anterior > 0 ? Math.Round((actual - anterior) / anterior * 100, 1) : 0;
    }
}