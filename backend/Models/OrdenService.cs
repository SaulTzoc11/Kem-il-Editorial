using System.Data;
using System.Globalization;
using KemilEditorial.API.Auth;
using KemilEditorial.API.Data;
using KemilEditorial.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace KemilEditorial.API.Models
{
    /// <summary>
    /// Lógica de negocio de las órdenes (capa Modelo): resuelve precios desde la base de datos,
    /// delega el cálculo a <see cref="DescuentoService"/>, valida stock y guarda la factura.
    /// El cliente solo decide QUÉ libros y CUÁNTOS; nunca el precio ni el tipo de descuento.
    /// </summary>
    public class OrdenService
    {
        public const string EstadoInicial = "Pendiente";

        private readonly AppDbContext _db;
        private readonly DescuentoService _descuentos;

        public OrdenService(AppDbContext db, DescuentoService descuentos)
        {
            _db = db;
            _descuentos = descuentos;
        }

        /// <summary>Vista previa del carrito (no guarda nada). Usa los precios de la base de datos.</summary>
        public async Task<ResumenOrdenDto> CalcularAsync(IEnumerable<ItemOrdenDto>? items, string? tipoCliente)
        {
            var (_, request) = await ArmarSolicitudAsync(items, tipoCliente ?? "Minorista");
            return _descuentos.CalcularOrden(request);
        }

        /// <summary>
        /// Cierra la orden (RF-03.3): recalcula en el servidor, verifica y descuenta stock y guarda
        /// factura + detalle, todo en una sola transacción (o se guarda todo o no se guarda nada).
        /// </summary>
        public async Task<OrdenCreadaDto> CrearAsync(int idUsuario, string rol, CrearOrdenRequestDto? solicitud)
        {
            // Tipo de cliente según el rol del token, nunca según lo que diga el cuerpo de la petición
            string tipoCliente = TipoClienteDesdeRol(rol);

            await using var transaccion = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var (libros, request) = await ArmarSolicitudAsync(solicitud?.Items, tipoCliente);
            var resumen = _descuentos.CalcularOrden(request);

            await DescontarStockAsync(request.Items, libros);

            var factura = new Factura
            {
                IdUsuario = idUsuario,
                FechaEmision = DateTime.Now,
                SubtotalBase = resumen.SubtotalBase,
                DescuentoAplicado = DescribirDescuento(resumen),
                Impuestos = resumen.Impuestos,
                TotalFinal = resumen.TotalFinal,
                Estado = EstadoInicial,
                DescuentoVolumenPct = resumen.PorcentajeVolumen,
                DescuentoClientePct = resumen.PorcentajeCliente,
                MontoDescuento = resumen.TotalDescuentos,
                Detalles = request.Items.Select(i => new DetalleFactura
                {
                    IdLibro = i.IdLibro,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.Precio,
                    SubtotalLinea = Math.Round(i.Precio * i.Cantidad, 2, MidpointRounding.AwayFromZero)
                }).ToList()
            };

            _db.Facturas.Add(factura);
            await _db.SaveChangesAsync();
            await transaccion.CommitAsync();

            return new OrdenCreadaDto
            {
                IdFactura = factura.IdFactura,
                Estado = factura.Estado,
                FechaEmision = factura.FechaEmision,
                SubtotalBase = resumen.SubtotalBase,
                TotalUnidades = resumen.TotalUnidades,
                PorcentajeVolumen = resumen.PorcentajeVolumen,
                DescuentoVolumenMonto = resumen.DescuentoVolumenMonto,
                DetalleReglaVolumen = resumen.DetalleReglaVolumen,
                PorcentajeCliente = resumen.PorcentajeCliente,
                DescuentoClienteMonto = resumen.DescuentoClienteMonto,
                DetalleReglaCliente = resumen.DetalleReglaCliente,
                TotalDescuentos = resumen.TotalDescuentos,
                SubtotalConDescuentos = resumen.SubtotalConDescuentos,
                Impuestos = resumen.Impuestos,
                TotalFinal = resumen.TotalFinal
            };
        }

        /// <summary>Historial de compras del cliente, más reciente primero.</summary>
        public async Task<List<PedidoDto>> MisPedidosAsync(int idUsuario)
        {
            var facturas = await _db.Facturas
                .AsNoTracking()
                .Where(f => f.IdUsuario == idUsuario)
                .Include(f => f.Detalles)
                    .ThenInclude(d => d.Libro)
                .OrderByDescending(f => f.IdFactura)
                .ToListAsync();

            return facturas.Select(f => new PedidoDto
            {
                IdFactura = f.IdFactura,
                FechaEmision = f.FechaEmision,
                Estado = f.Estado,
                TotalUnidades = f.Detalles.Sum(d => d.Cantidad),
                SubtotalBase = f.SubtotalBase,
                DescuentoAplicado = f.DescuentoAplicado,
                MontoDescuento = f.MontoDescuento,
                Impuestos = f.Impuestos,
                TotalFinal = f.TotalFinal,
                Lineas = f.Detalles.Select(d => new LineaPedidoDto
                {
                    IdLibro = d.IdLibro,
                    Nombre = d.Libro?.Nombre ?? string.Empty,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    SubtotalLinea = d.SubtotalLinea
                }).ToList()
            }).ToList();
        }

        /// <summary>Mayorista e Institucional reciben el descuento adicional (RF-03.2); el resto, no.</summary>
        public static string TipoClienteDesdeRol(string? rol) =>
            string.Equals(rol, Roles.Institucional, StringComparison.OrdinalIgnoreCase)
            || string.Equals(rol, Roles.Mayorista, StringComparison.OrdinalIgnoreCase)
                ? "Institucional"
                : "Minorista";

        // Los e-books no tienen existencias físicas: no se validan ni se descuentan.
        private static bool EsDigital(Libro libro) =>
            string.Equals(libro.Formato?.Trim(), "E-book", StringComparison.OrdinalIgnoreCase);

        private static string DescribirDescuento(ResumenOrdenDto r) =>
            string.Create(CultureInfo.InvariantCulture,
                $"Volumen {r.PorcentajeVolumen:0.##}% + Cliente {r.PorcentajeCliente:0.##}%");

        // Une los renglones repetidos, valida que los libros existan y arma la solicitud con el PRECIO DE LA BD.
        private async Task<(Dictionary<int, Libro> Libros, CalcularOrdenRequestDto Request)> ArmarSolicitudAsync(
            IEnumerable<ItemOrdenDto>? items, string tipoCliente)
        {
            var pedidos = (items ?? Enumerable.Empty<ItemOrdenDto>())
                .GroupBy(i => i.IdLibro)
                .Select(g => (IdLibro: g.Key, Cantidad: g.Sum(i => i.Cantidad)))
                .ToList();

            if (pedidos.Count == 0)
                throw new ReglaNegocioException("El carrito no contiene productos.");

            var ids = pedidos.Select(p => p.IdLibro).ToList();
            var libros = await _db.Libros
                .Where(l => ids.Contains(l.IdLibro) && l.Estado)
                .ToDictionaryAsync(l => l.IdLibro);

            foreach (var pedido in pedidos)
                if (!libros.ContainsKey(pedido.IdLibro))
                    throw new ReglaNegocioException(
                        $"El libro {pedido.IdLibro} no existe o ya no está disponible.");

            var request = new CalcularOrdenRequestDto
            {
                TipoCliente = tipoCliente,
                Items = pedidos.Select(p => new ItemCarritoDto
                {
                    IdLibro = p.IdLibro,
                    Nombre = libros[p.IdLibro].Nombre,
                    Precio = libros[p.IdLibro].Precio,
                    Cantidad = p.Cantidad
                }).ToList()
            };

            return (libros, request);
        }

        // Verifica y descuenta existencias. Si un libro está en varias bodegas, toma primero de la que más tiene.
        private async Task DescontarStockAsync(IEnumerable<ItemCarritoDto> items, Dictionary<int, Libro> libros)
        {
            var fisicos = items.Where(i => !EsDigital(libros[i.IdLibro])).ToList();
            if (fisicos.Count == 0) return;

            var ids = fisicos.Select(i => i.IdLibro).ToList();
            var inventarios = await _db.Inventarios
                .Where(inv => ids.Contains(inv.IdLibro) && inv.Estado)
                .ToListAsync();

            // Pasada 1: verificar TODO antes de tocar nada, para no dejar descuentos a medias
            var plan = new List<(ItemCarritoDto Item, List<InventarioLibro> Bodegas)>();
            foreach (var item in fisicos)
            {
                var bodegas = inventarios
                    .Where(inv => inv.IdLibro == item.IdLibro)
                    .OrderByDescending(inv => inv.StockDisponible)
                    .ToList();

                int disponible = bodegas.Sum(inv => inv.StockDisponible);
                if (disponible < item.Cantidad)
                    throw new StockInsuficienteException(
                        $"Stock insuficiente de «{item.Nombre}»: hay {disponible} y solicitaste {item.Cantidad}.");

                plan.Add((item, bodegas));
            }

            // Pasada 2: descontar
            foreach (var (item, bodegas) in plan)
            {
                int pendiente = item.Cantidad;
                foreach (var bodega in bodegas)
                {
                    int tomar = Math.Min(bodega.StockDisponible, pendiente);
                    bodega.StockDisponible -= tomar;
                    pendiente -= tomar;
                    if (pendiente == 0) break;
                }
            }
        }
    }
}
