using KemilEditorial.API.Auth;
using KemilEditorial.API.Data;
using KemilEditorial.API.DTOs;
using KemilEditorial.API.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KemilEditorial.Tests;

/// <summary>Pruebas del cierre de orden y del historial (RF-03.3). Usan SQLite en memoria (ver TestDb).</summary>
public class OrdenServiceTests : IDisposable
{
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    private OrdenService Servicio(AppDbContext ctx) => new(ctx, new DescuentoService());

    private static List<ItemOrdenDto> Items(params (int idLibro, int cantidad)[] renglones) =>
        renglones.Select(r => new ItemOrdenDto { IdLibro = r.idLibro, Cantidad = r.cantidad }).ToList();

    private static CrearOrdenRequestDto Orden(params (int idLibro, int cantidad)[] renglones) =>
        new() { Items = Items(renglones) };

    private int StockDe(int idLibro)
    {
        using var ctx = _db.Crear();
        return ctx.Inventarios.Where(i => i.IdLibro == idLibro).Sum(i => i.StockDisponible);
    }

    private int TotalFacturas()
    {
        using var ctx = _db.Crear();
        return ctx.Facturas.Count();
    }

    // ---------- Vista previa: el precio sale de la base de datos ----------

    [Fact]
    public async Task Calcular_UsaElPrecioDeLaBaseDeDatos()
    {
        using var ctx = _db.Crear();

        var r = await Servicio(ctx).CalcularAsync(Items((1, 2)), "Minorista");

        Assert.Equal(200m, r.SubtotalBase);   // 2 x Q100 (precio en BD)
    }

    [Fact]
    public async Task Calcular_UnificaLosRenglonesDelMismoLibro()
    {
        using var ctx = _db.Crear();

        // 3 + 3 del mismo libro = 6 unidades -> tramo de 5%
        var r = await Servicio(ctx).CalcularAsync(Items((1, 3), (1, 3)), "Minorista");

        Assert.Equal(6, r.TotalUnidades);
        Assert.Equal(5m, r.PorcentajeVolumen);
    }

    [Theory]
    [InlineData(999)]   // no existe
    [InlineData(4)]     // dado de baja
    public async Task Calcular_LibroInexistenteODeBaja_Lanza(int idLibro)
    {
        using var ctx = _db.Crear();

        var ex = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => Servicio(ctx).CalcularAsync(Items((idLibro, 1)), "Minorista"));

        Assert.Contains("no existe", ex.Message);
    }

    [Fact]
    public async Task Calcular_CarritoVacioONulo_Lanza()
    {
        using var ctx = _db.Crear();
        var servicio = Servicio(ctx);

        await Assert.ThrowsAsync<ReglaNegocioException>(() => servicio.CalcularAsync(Items(), "Minorista"));
        await Assert.ThrowsAsync<ReglaNegocioException>(() => servicio.CalcularAsync(null, "Minorista"));
    }

    // ---------- Cierre de orden ----------

    [Fact]
    public async Task Crear_GuardaFacturaYDetalleYDescuentaElStock()
    {
        using (var ctx = _db.Crear())
        {
            // 2 x Q100 + 1 x Q50 = 250 | 3 unidades: sin descuento | IVA 30 | total 280
            var orden = await Servicio(ctx).CrearAsync(TestDb.Minorista, Roles.Cliente, Orden((1, 2), (2, 1)));

            Assert.True(orden.IdFactura > 0);
            Assert.Equal("Pendiente", orden.Estado);
            Assert.Equal(250m, orden.SubtotalBase);
            Assert.Equal(280m, orden.TotalFinal);
        }

        using var verificacion = _db.Crear();
        var factura = verificacion.Facturas.Include(f => f.Detalles).Single();

        Assert.Equal(TestDb.Minorista, factura.IdUsuario);
        Assert.Equal(280m, factura.TotalFinal);
        Assert.Equal(2, factura.Detalles.Count);

        var detalle = factura.Detalles.Single(d => d.IdLibro == 1);
        Assert.Equal(2, detalle.Cantidad);
        Assert.Equal(100m, detalle.PrecioUnitario);
        Assert.Equal(200m, detalle.SubtotalLinea);

        Assert.Equal(13, StockDe(1));   // 15 - 2
        Assert.Equal(2, StockDe(2));    // 3 - 1
    }

    [Fact]
    public async Task Crear_TomaElStockDeVariasBodegas()
    {
        using (var ctx = _db.Crear())
            await Servicio(ctx).CrearAsync(TestDb.Minorista, Roles.Cliente, Orden((1, 12)));

        using var verificacion = _db.Crear();
        var bodega1 = verificacion.Inventarios.Single(i => i.IdInventario == 1);
        var bodega2 = verificacion.Inventarios.Single(i => i.IdInventario == 2);

        Assert.Equal(0, bodega1.StockDisponible);   // la que más tenía se vacía primero (10)
        Assert.Equal(3, bodega2.StockDisponible);   // y se completa con 2 de la otra (5 - 2)
    }

    [Theory]
    [InlineData("Institucional")]
    [InlineData("Mayorista")]
    public async Task Crear_InstitucionalOMayorista_RecibeOchoPorCientoAdicional(string rol)
    {
        using var ctx = _db.Crear();

        // 10 x Q100 = 1000 | volumen 5% -> 950 | cliente 8% -> 874 | IVA 104.88 | total 978.88
        var orden = await Servicio(ctx).CrearAsync(TestDb.Institucional, rol, Orden((1, 10)));

        Assert.Equal(8m, orden.PorcentajeCliente);
        Assert.Equal(874m, orden.SubtotalConDescuentos);
        Assert.Equal(978.88m, orden.TotalFinal);
    }

    [Theory]
    [InlineData("Cliente")]
    [InlineData("Asesor")]
    public async Task Crear_ClienteNoInstitucional_NoRecibeDescuentoAdicional(string rol)
    {
        using var ctx = _db.Crear();

        var orden = await Servicio(ctx).CrearAsync(TestDb.Minorista, rol, Orden((1, 10)));

        Assert.Equal(0m, orden.PorcentajeCliente);
        Assert.Equal(1064m, orden.TotalFinal);
    }

    [Fact]
    public async Task Crear_StockInsuficiente_LanzaYNoGuardaNada()
    {
        using var ctx = _db.Crear();

        var ex = await Assert.ThrowsAsync<StockInsuficienteException>(
            () => Servicio(ctx).CrearAsync(TestDb.Minorista, Roles.Cliente, Orden((1, 16))));   // hay 15

        Assert.Contains("Stock insuficiente", ex.Message);
        Assert.Equal(0, TotalFacturas());
        Assert.Equal(15, StockDe(1));
    }

    [Fact]
    public async Task Crear_SiUnRenglonNoTieneStock_NoDescuentaLosDemas()
    {
        using var ctx = _db.Crear();

        // el libro 1 sí alcanza (2 de 15) pero el libro 2 no (5 de 3)
        await Assert.ThrowsAsync<StockInsuficienteException>(
            () => Servicio(ctx).CrearAsync(TestDb.Minorista, Roles.Cliente, Orden((1, 2), (2, 5))));

        Assert.Equal(0, TotalFacturas());
        Assert.Equal(15, StockDe(1));
        Assert.Equal(3, StockDe(2));
    }

    [Fact]
    public async Task Crear_EBook_NoRequiereNiDescuentaStock()
    {
        using var ctx = _db.Crear();

        // 4 x Q40 = 160 | IVA 19.20 | total 179.20 (el e-book no tiene inventario y aun así se vende)
        var orden = await Servicio(ctx).CrearAsync(TestDb.Minorista, Roles.Cliente, Orden((3, 4)));

        Assert.Equal(179.20m, orden.TotalFinal);
    }

    [Fact]
    public async Task Crear_CarritoVacio_Lanza()
    {
        using var ctx = _db.Crear();

        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => Servicio(ctx).CrearAsync(TestDb.Minorista, Roles.Cliente, Orden()));
        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => Servicio(ctx).CrearAsync(TestDb.Minorista, Roles.Cliente, null));
    }

    // ---------- Historial ----------

    [Fact]
    public async Task MisPedidos_SoloDevuelveLosDelUsuario_ElMasRecientePrimero()
    {
        using (var ctx = _db.Crear())
        {
            var servicio = Servicio(ctx);
            await servicio.CrearAsync(TestDb.Minorista, Roles.Cliente, Orden((2, 1)));
            await servicio.CrearAsync(TestDb.Minorista, Roles.Cliente, Orden((1, 2)));
            await servicio.CrearAsync(TestDb.OtroMinorista, Roles.Cliente, Orden((1, 1)));
        }

        using var consulta = _db.Crear();
        var servicioConsulta = Servicio(consulta);

        var deAna = await servicioConsulta.MisPedidosAsync(TestDb.Minorista);
        Assert.Equal(2, deAna.Count);
        Assert.True(deAna[0].IdFactura > deAna[1].IdFactura);   // más reciente primero
        Assert.Equal(2, deAna[0].TotalUnidades);                // la última compra fue de 2 unidades
        Assert.Equal(1, deAna[0].Lineas.Count);

        Assert.Equal(1, (await servicioConsulta.MisPedidosAsync(TestDb.OtroMinorista)).Count);
        Assert.Equal(0, (await servicioConsulta.MisPedidosAsync(TestDb.Institucional)).Count);
    }

    // ---------- Rol -> tipo de cliente ----------

    [Theory]
    [InlineData("Institucional", "Institucional")]
    [InlineData("institucional", "Institucional")]
    [InlineData("Mayorista", "Institucional")]
    [InlineData("Cliente", "Minorista")]
    [InlineData("Administrador", "Minorista")]
    public void TipoClienteDesdeRol_InstitucionalYMayoristaRecibenDescuento(string rol, string esperado)
    {
        Assert.Equal(esperado, OrdenService.TipoClienteDesdeRol(rol));
    }
}
