using KemilEditorial.API.DTOs;
using KemilEditorial.API.Models;
using Xunit;

namespace KemilEditorial.Tests;

/// <summary>Pruebas del motor de descuentos (RF-03.2 / RF-03.3). Lógica pura: sin base de datos.</summary>
public class DescuentoServiceTests
{
    private readonly DescuentoService _motor = new();

    private static CalcularOrdenRequestDto Carrito(string tipo, params (int cantidad, decimal precio)[] renglones) =>
        new()
        {
            TipoCliente = tipo,
            Items = renglones
                .Select((r, i) => new ItemCarritoDto { IdLibro = i + 1, Nombre = $"Libro {i + 1}", Cantidad = r.cantidad, Precio = r.precio })
                .ToList()
        };

    // ---------- Escala de volumen (límites de cada tramo) ----------

    [Theory]
    [InlineData(1, 0)]
    [InlineData(5, 0)]
    [InlineData(6, 5)]
    [InlineData(20, 5)]
    [InlineData(21, 10)]
    [InlineData(50, 10)]
    [InlineData(51, 15)]
    [InlineData(100, 15)]
    public void Volumen_AplicaElPorcentajeCorrectoEnLosLimites(int unidades, int porcentajeEsperado)
    {
        var r = _motor.CalcularOrden(Carrito("Minorista", (unidades, 100m)));

        Assert.Equal(unidades, r.TotalUnidades);
        Assert.Equal((decimal)porcentajeEsperado, r.PorcentajeVolumen);
        Assert.Equal(unidades * 100m * porcentajeEsperado / 100m, r.DescuentoVolumenMonto);
    }

    [Fact]
    public void Volumen_CuentaLasUnidadesDeToda_LaCombinacion_DeTitulos()
    {
        // 2 + 2 + 2 = 6 unidades en tres títulos distintos -> entra al tramo de 5%
        var r = _motor.CalcularOrden(Carrito("Minorista", (2, 50m), (2, 60m), (2, 70m)));

        Assert.Equal(6, r.TotalUnidades);
        Assert.Equal(5m, r.PorcentajeVolumen);
        Assert.Equal(360m, r.SubtotalBase);
        Assert.Equal(18m, r.DescuentoVolumenMonto);
    }

    // ---------- Descuento por tipo de cliente ----------

    [Fact]
    public void Minorista_NoRecibeDescuentoAdicional()
    {
        // 10 x Q100 = 1000 | volumen 5% = 50 | 950 | IVA 12% = 114 | total 1064
        var r = _motor.CalcularOrden(Carrito("Minorista", (10, 100m)));

        Assert.Equal(1000m, r.SubtotalBase);
        Assert.Equal(50m, r.DescuentoVolumenMonto);
        Assert.Equal(0m, r.PorcentajeCliente);
        Assert.Equal(0m, r.DescuentoClienteMonto);
        Assert.Equal(950m, r.SubtotalConDescuentos);
        Assert.Equal(114m, r.Impuestos);
        Assert.Equal(1064m, r.TotalFinal);
    }

    [Fact]
    public void Institucional_AplicaOchoPorCientoAcumulativoTrasElVolumen()
    {
        // 10 x Q100 = 1000 | volumen 5% = 50 | 950 | cliente 8% de 950 = 76 | 874 | IVA = 104.88 | total 978.88
        var r = _motor.CalcularOrden(Carrito("Institucional", (10, 100m)));

        Assert.Equal(1000m, r.SubtotalBase);
        Assert.Equal(50m, r.DescuentoVolumenMonto);
        Assert.Equal(8m, r.PorcentajeCliente);
        Assert.Equal(76m, r.DescuentoClienteMonto);   // 8% de 950, NO de 1000 (80)
        Assert.Equal(126m, r.TotalDescuentos);
        Assert.Equal(874m, r.SubtotalConDescuentos);
        Assert.Equal(104.88m, r.Impuestos);
        Assert.Equal(978.88m, r.TotalFinal);
    }

    [Theory]
    [InlineData("Mayorista")]
    [InlineData("institucional")]
    [InlineData("  INSTITUCIONAL  ")]
    public void Institucional_EsInsensibleAMayusculasYAceptaMayorista(string tipo)
    {
        var r = _motor.CalcularOrden(Carrito(tipo, (10, 100m)));

        Assert.Equal(8m, r.PorcentajeCliente);
    }

    [Theory]
    [InlineData("")]
    [InlineData("minorista")]
    public void TipoVacioOMinorista_EsMinorista(string tipo)
    {
        var r = _motor.CalcularOrden(Carrito(tipo, (10, 100m)));

        Assert.Equal(0m, r.PorcentajeCliente);
    }

    // ---------- Redondeo ----------

    [Fact]
    public void Redondeo_ElMedioCentavoSube()
    {
        // 6 x 1.15 = 6.90 | 5% = 0.345 -> 0.35 (el redondeo bancario daría 0.34)
        var r = _motor.CalcularOrden(Carrito("Minorista", (6, 1.15m)));

        Assert.Equal(6.90m, r.SubtotalBase);
        Assert.Equal(0.35m, r.DescuentoVolumenMonto);
        Assert.Equal(6.55m, r.SubtotalConDescuentos);
        Assert.Equal(0.79m, r.Impuestos);
        Assert.Equal(7.34m, r.TotalFinal);
    }

    [Fact]
    public void Totales_SiempreCuadranAlCentavo()
    {
        foreach (var tipo in new[] { "Minorista", "Institucional" })
        foreach (var precio in new[] { 0.01m, 9.99m, 33.33m, 125.50m, 1234.57m })
        for (int unidades = 1; unidades <= 120; unidades++)
        {
            var r = _motor.CalcularOrden(Carrito(tipo, (unidades, precio)));
            string caso = $"{tipo}, {unidades} x Q{precio}";

            Assert.True(r.SubtotalBase - r.TotalDescuentos == r.SubtotalConDescuentos, "Descuentos no cuadran: " + caso);
            Assert.True(r.SubtotalConDescuentos + r.Impuestos == r.TotalFinal, "Total no cuadra: " + caso);
            Assert.True(r.DescuentoVolumenMonto + r.DescuentoClienteMonto == r.TotalDescuentos, "Suma de descuentos: " + caso);
            Assert.True(Math.Round(r.TotalFinal, 2) == r.TotalFinal, "Más de 2 decimales: " + caso);
        }
    }

    // ---------- Validaciones ----------

    [Fact]
    public void CarritoVacio_Lanza()
    {
        var ex = Assert.Throws<ReglaNegocioException>(() => _motor.CalcularOrden(new CalcularOrdenRequestDto()));

        Assert.Contains("no contiene productos", ex.Message);
    }

    [Fact]
    public void RequestNulo_Lanza()
    {
        Assert.Throws<ReglaNegocioException>(() => _motor.CalcularOrden(null!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void CantidadNoPositiva_Lanza(int cantidad)
    {
        var ex = Assert.Throws<ReglaNegocioException>(() => _motor.CalcularOrden(Carrito("Minorista", (cantidad, 10m))));

        Assert.Contains("mayor que cero", ex.Message);
    }

    [Fact]
    public void PrecioNegativo_Lanza()
    {
        var ex = Assert.Throws<ReglaNegocioException>(() => _motor.CalcularOrden(Carrito("Minorista", (1, -5m))));

        Assert.Contains("negativo", ex.Message);
    }

    [Fact]
    public void TipoDeClienteDesconocido_Lanza()
    {
        var ex = Assert.Throws<ReglaNegocioException>(() => _motor.CalcularOrden(Carrito("VIP", (1, 10m))));

        Assert.Contains("Tipo de cliente no válido", ex.Message);
    }
}
