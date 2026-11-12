using KemilEditorial.API.DTOs;
using KemilEditorial.API.Models;
using Xunit;

namespace KemilEditorial.Tests;

/// <summary>Pruebas de validación y guardado del catálogo (LibroService).</summary>
public class LibroServiceTests : IDisposable
{
    private readonly TestDb _bd = new();
    private readonly string _carpeta = Path.Combine(Path.GetTempPath(), "kemil-portadas-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        _bd.Dispose();
        if (Directory.Exists(_carpeta)) Directory.Delete(_carpeta, recursive: true);
    }

    private LibroService Servicio(KemilEditorial.API.Data.AppDbContext ctx) => new(ctx, _carpeta);

    private static CrearLibroDto Valido(Action<CrearLibroDto>? cambiar = null)
    {
        var d = new CrearLibroDto
        {
            Nombre = "Cuentos del Volcán", Descripcion = "Relatos breves.", Formato = "Tapa blanda",
            Precio = 85.50m, ISBN = "978-99922-0-777-1", IdGenero = 1, IdEditorial = 1
        };
        cambiar?.Invoke(d);
        return d;
    }

    private async Task<string> ErrorAlCrear(CrearLibroDto d)
    {
        using var ctx = _bd.Crear();
        var ex = await Assert.ThrowsAsync<ReglaNegocioException>(() => Servicio(ctx).CrearAsync(d));
        return ex.Message;
    }

    // PNG mínimo válido (solo importa la firma de los primeros bytes)
    private static readonly byte[] Png = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0 };
    private static readonly byte[] Jpg = { 0xFF, 0xD8, 0xFF, 0xE0, 0, 0, 0, 0 };

    // ---------- Crear ----------

    [Fact]
    public async Task Crear_GuardaElLibroActivoConDatosLimpios()
    {
        using (var ctx = _bd.Crear())
        {
            var creado = await Servicio(ctx).CrearAsync(Valido(d =>
            {
                d.Nombre = "  Cuentos del Volcán  ";
                d.Formato = "tapa DURA";
            }));
            Assert.True(creado.IdLibro > 0);
            Assert.Equal("Cuentos del Volcán", creado.Nombre);
            Assert.Equal("Tapa dura", creado.Formato);
        }

        using var lectura = _bd.Crear();
        var guardado = Servicio(lectura).ListarAsync().GetAwaiter().GetResult().Single(l => l.ISBN == "978-99922-0-777-1");
        Assert.Equal(85.50m, guardado.Precio);
        Assert.Equal(1, guardado.IdGenero);
    }

    [Fact]
    public async Task Crear_AceptaIsbn10YIsbnSinGuiones()
    {
        using var ctx = _bd.Crear();
        var servicio = Servicio(ctx);
        await servicio.CrearAsync(Valido(d => d.ISBN = "0-306-40615-2"));
        await servicio.CrearAsync(Valido(d => d.ISBN = "9789992207772"));
        await servicio.CrearAsync(Valido(d => d.ISBN = "080442957X"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Crear_RechazaTituloVacio(string nombre)
    {
        Assert.Contains("título es obligatorio", await ErrorAlCrear(Valido(d => d.Nombre = nombre)));
    }

    [Fact]
    public async Task Crear_RechazaTituloMuyLargo()
    {
        Assert.Contains("200 caracteres", await ErrorAlCrear(Valido(d => d.Nombre = new string('a', 201))));
    }

    [Fact]
    public async Task Crear_RechazaMarcadoHtmlEnTituloYSinopsis()
    {
        Assert.Contains("< ni >", await ErrorAlCrear(Valido(d => d.Nombre = "<script>alert(1)</script>")));
        Assert.Contains("< ni >", await ErrorAlCrear(Valido(d => d.Descripcion = "Hola <b>mundo</b>")));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Crear_RechazaPrecioNoPositivo(int precio)
    {
        Assert.Contains("mayor que cero", await ErrorAlCrear(Valido(d => d.Precio = precio)));
    }

    [Fact]
    public async Task Crear_RechazaPrecioConMasDeDosDecimalesOEnormeComoErrorDeNegocio()
    {
        Assert.Contains("2 decimales", await ErrorAlCrear(Valido(d => d.Precio = 10.999m)));
        Assert.Contains("demasiado alto", await ErrorAlCrear(Valido(d => d.Precio = 100_000_000m)));
    }

    [Theory]
    [InlineData("Pasta blanda")]
    [InlineData("")]
    [InlineData("Audiolibro")]
    public async Task Crear_RechazaFormatoFueraDeLaLista(string formato)
    {
        Assert.Contains("Formato no válido", await ErrorAlCrear(Valido(d => d.Formato = formato)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("12345")]
    [InlineData("978-99922-0-777-X")]   // la X solo se permite al final de un ISBN de 10
    [InlineData("123456789012345")]
    public async Task Crear_RechazaIsbnInvalido(string isbn)
    {
        Assert.Contains("ISBN debe tener 10 o 13", await ErrorAlCrear(Valido(d => d.ISBN = isbn)));
    }

    [Fact]
    public async Task Crear_RechazaIsbnRepetidoAunqueCambienLosGuiones()
    {
        using (var ctx = _bd.Crear())
            await Servicio(ctx).CrearAsync(Valido());

        Assert.Contains("Ya existe un libro con ese ISBN", await ErrorAlCrear(Valido(d => d.ISBN = "9789992207771")));
    }

    [Theory]
    [InlineData(99, 1, "género")]
    [InlineData(3, 1, "género")]        // género dado de baja
    [InlineData(1, 99, "editorial")]
    public async Task Crear_RechazaGeneroOEditorialInexistente(int genero, int editorial, string esperado)
    {
        var mensaje = await ErrorAlCrear(Valido(d => { d.IdGenero = genero; d.IdEditorial = editorial; }));
        Assert.Contains(esperado, mensaje);
    }

    [Fact]
    public async Task Crear_ReportaTodosLosErroresJuntos()
    {
        var mensaje = await ErrorAlCrear(Valido(d => { d.Nombre = ""; d.Precio = 0; d.Formato = "x"; }));

        Assert.Contains("título es obligatorio", mensaje);
        Assert.Contains("mayor que cero", mensaje);
        Assert.Contains("Formato no válido", mensaje);
    }

    [Fact]
    public async Task Crear_ConDatosNulos_Lanza()
    {
        using var ctx = _bd.Crear();
        await Assert.ThrowsAsync<ReglaNegocioException>(() => Servicio(ctx).CrearAsync(null));
    }

    // ---------- Actualizar y eliminar ----------

    [Fact]
    public async Task Actualizar_CambiaLosDatosYPermiteConservarSuPropioIsbn()
    {
        using var ctx = _bd.Crear();
        var servicio = Servicio(ctx);
        var creado = await servicio.CrearAsync(Valido());

        var editado = await servicio.ActualizarAsync(creado.IdLibro, Valido(d =>
        {
            d.Precio = 99m;
            d.IdGenero = 2;
        }));

        Assert.Equal(99m, editado.Precio);
        Assert.Equal(2, editado.IdGenero);
    }

    [Fact]
    public async Task Actualizar_NoPermiteQuedarseConElIsbnDeOtroLibro()
    {
        using var ctx = _bd.Crear();
        var servicio = Servicio(ctx);
        var a = await servicio.CrearAsync(Valido(d => d.ISBN = "978-99922-0-801-1"));
        var b = await servicio.CrearAsync(Valido(d => d.ISBN = "978-99922-0-802-8"));

        var ex = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => servicio.ActualizarAsync(b.IdLibro, Valido(d => d.ISBN = a.ISBN)));
        Assert.Contains("Ya existe un libro con ese ISBN", ex.Message);
    }

    [Fact]
    public async Task Actualizar_LibroInexistenteODadoDeBaja_NoEncontrado()
    {
        using var ctx = _bd.Crear();
        var servicio = Servicio(ctx);

        await Assert.ThrowsAsync<NoEncontradoException>(() => servicio.ActualizarAsync(999, Valido()));
        await Assert.ThrowsAsync<NoEncontradoException>(() => servicio.ActualizarAsync(4, Valido())); // libro 4: Estado = false
    }

    [Fact]
    public async Task Eliminar_EsLogico_ElLibroDejaDeListarseYSigueEnLaBase()
    {
        using (var ctx = _bd.Crear())
        {
            await Servicio(ctx).EliminarAsync(2);
            await Assert.ThrowsAsync<NoEncontradoException>(() => Servicio(ctx).EliminarAsync(2)); // ya estaba dado de baja
            await Assert.ThrowsAsync<NoEncontradoException>(() => Servicio(ctx).EliminarAsync(999));
        }

        using var lectura = _bd.Crear();
        var activos = await Servicio(lectura).ListarAsync();
        Assert.True(activos.All(l => l.IdLibro != 2));
        Assert.True(lectura.Libros.Any(l => l.IdLibro == 2 && !l.Estado));
    }

    // ---------- Portada ----------

    [Fact]
    public async Task Portada_GuardaElArchivoYLaRutaPublica()
    {
        using var ctx = _bd.Crear();
        var libro = await Servicio(ctx).GuardarPortadaAsync(1, new MemoryStream(Png), Png.Length);

        Assert.True(libro.Portada!.StartsWith("/uploads/portadas/1-"));
        Assert.True(libro.Portada.EndsWith(".png"));
        Assert.True(File.Exists(Path.Combine(_carpeta, Path.GetFileName(libro.Portada))));
    }

    [Fact]
    public async Task Portada_ElTipoSaleDeLosBytesNoDelNombre()
    {
        using var ctx = _bd.Crear();
        var libro = await Servicio(ctx).GuardarPortadaAsync(1, new MemoryStream(Jpg), Jpg.Length);

        Assert.True(libro.Portada!.EndsWith(".jpg"));
    }

    [Fact]
    public async Task Portada_RechazaArchivosQueNoSonImagenPermitida()
    {
        using var ctx = _bd.Crear();
        var servicio = Servicio(ctx);
        var svg = System.Text.Encoding.UTF8.GetBytes("<svg xmlns='http://www.w3.org/2000/svg'><script>alert(1)</script></svg>");
        var html = System.Text.Encoding.UTF8.GetBytes("<html></html>");

        var ex1 = await Assert.ThrowsAsync<ReglaNegocioException>(() => servicio.GuardarPortadaAsync(1, new MemoryStream(svg), svg.Length));
        var ex2 = await Assert.ThrowsAsync<ReglaNegocioException>(() => servicio.GuardarPortadaAsync(1, new MemoryStream(html), html.Length));

        Assert.Contains("JPG, PNG o WebP", ex1.Message);
        Assert.Contains("JPG, PNG o WebP", ex2.Message);
    }

    [Fact]
    public async Task Portada_RechazaVaciaYMuyPesada()
    {
        using var ctx = _bd.Crear();
        var servicio = Servicio(ctx);
        var grande = new byte[(int)LibroService.PortadaMaxBytes + 1];
        Array.Copy(Png, grande, Png.Length);

        var vacia = await Assert.ThrowsAsync<ReglaNegocioException>(() => servicio.GuardarPortadaAsync(1, new MemoryStream(), 0));
        var pesada = await Assert.ThrowsAsync<ReglaNegocioException>(() => servicio.GuardarPortadaAsync(1, new MemoryStream(grande), grande.Length));

        Assert.Contains("vacío", vacia.Message);
        Assert.Contains("2 MB", pesada.Message);
    }

    [Fact]
    public async Task Portada_AlReemplazarBorraLaSubidaAnteriorPeroNoLasDelFrontend()
    {
        using var ctx = _bd.Crear();
        var servicio = Servicio(ctx);

        var primera = await servicio.GuardarPortadaAsync(1, new MemoryStream(Png), Png.Length);
        var segunda = await servicio.GuardarPortadaAsync(1, new MemoryStream(Jpg), Jpg.Length);

        Assert.True(!File.Exists(Path.Combine(_carpeta, Path.GetFileName(primera.Portada!))));
        Assert.True(File.Exists(Path.Combine(_carpeta, Path.GetFileName(segunda.Portada!))));

        // Una portada original del frontend (/portadas/...) no es un archivo del servidor: no se borra nada
        ctx.Libros.Single(l => l.IdLibro == 2).Portada = "/portadas/978-99922-0-002-8.svg";
        ctx.SaveChanges();
        var tercera = await servicio.GuardarPortadaAsync(2, new MemoryStream(Png), Png.Length);
        Assert.True(tercera.Portada!.StartsWith("/uploads/portadas/2-"));
    }

    [Fact]
    public async Task Portada_LibroInexistente_NoEncontrado()
    {
        using var ctx = _bd.Crear();
        await Assert.ThrowsAsync<NoEncontradoException>(
            () => Servicio(ctx).GuardarPortadaAsync(999, new MemoryStream(Png), Png.Length));
    }

    // ---------- Ficha técnica (RF-01.3) ----------

    [Fact]
    public async Task Listar_IncluyeAutorEditorialAnioYExistencias()
    {
        using var ctx = _bd.Crear();
        var libros = await Servicio(ctx).ListarAsync();

        var antologia = libros.Single(l => l.IdLibro == 1);
        Assert.Equal("Lucía Tzoc, Mateo Xicay", antologia.Autor);   // el autor inactivo no aparece
        Assert.Equal("Kem'il Editorial", antologia.Editorial);
        Assert.Equal(2023, antologia.AnioEdicion);
        Assert.Equal("Primera", antologia.Edicion);
        Assert.Equal(4.6m, antologia.Valoracion);
        Assert.Equal(15, antologia.Stock);                           // 10 + 5 en dos bodegas
        Assert.Equal(2, antologia.StockMinimo);
    }

    [Fact]
    public async Task Listar_LibroSinAutoresNiAnio_QuedaVacioSinFallar()
    {
        using var ctx = _bd.Crear();
        var historia = (await Servicio(ctx).ListarAsync()).Single(l => l.IdLibro == 2);

        Assert.Equal(string.Empty, historia.Autor);
        Assert.Null(historia.AnioEdicion);
        Assert.Equal(3, historia.Stock);
    }

    [Fact]
    public async Task Listar_EbookNoTieneExistencias_StockEsNulo()
    {
        using var ctx = _bd.Crear();
        var atlas = (await Servicio(ctx).ListarAsync()).Single(l => l.IdLibro == 3);

        Assert.Null(atlas.Stock);
    }

    [Fact]
    public async Task Crear_LibroNuevoSinInventario_MuestraStockCero()
    {
        using var ctx = _bd.Crear();
        var creado = await Servicio(ctx).CrearAsync(Valido());

        Assert.Equal(0, creado.Stock);
        Assert.Equal("Kem'il Editorial", creado.Editorial);
    }

    // ---------- Stock y autor editables ----------

    private async Task<int> StockTotal(int idLibro)
    {
        using var ctx = _bd.Crear();
        return ctx.Inventarios.Where(i => i.IdLibro == idLibro && i.Estado).Sum(i => i.StockDisponible);
    }

    [Fact]
    public async Task Crear_ConStock_CreaElInventarioEnLaPrimeraBodega()
    {
        using var ctx = _bd.Crear();
        var creado = await Servicio(ctx).CrearAsync(Valido(d => d.Stock = 40));

        Assert.Equal(40, creado.Stock);
        using var lectura = _bd.Crear();
        var fila = lectura.Inventarios.Single(i => i.IdLibro == creado.IdLibro);
        Assert.Equal(1, fila.IdStock);
        Assert.Equal(LibroService.StockMinimoPorDefecto, fila.StockMinimo);
    }

    [Fact]
    public async Task Actualizar_SubirStock_SumaALaPrimeraBodega()
    {
        using (var ctx = _bd.Crear())
            await Servicio(ctx).ActualizarAsync(1, Valido(d => d.Stock = 25));

        Assert.Equal(25, await StockTotal(1)); // eran 10 + 5
        using var lectura = _bd.Crear();
        Assert.Equal(20, lectura.Inventarios.Single(i => i.IdInventario == 1).StockDisponible);
        Assert.Equal(5, lectura.Inventarios.Single(i => i.IdInventario == 2).StockDisponible);
    }

    [Fact]
    public async Task Actualizar_BajarStock_DescuentaPrimeroDeLaBodegaConMasUnidades()
    {
        using (var ctx = _bd.Crear())
            await Servicio(ctx).ActualizarAsync(1, Valido(d => d.Stock = 4));

        Assert.Equal(4, await StockTotal(1));
        using var lectura = _bd.Crear();
        Assert.Equal(0, lectura.Inventarios.Single(i => i.IdInventario == 1).StockDisponible); // la de 10
        Assert.Equal(4, lectura.Inventarios.Single(i => i.IdInventario == 2).StockDisponible); // la de 5
    }

    [Fact]
    public async Task Actualizar_StockNulo_NoTocaLasExistencias()
    {
        using (var ctx = _bd.Crear())
            await Servicio(ctx).ActualizarAsync(1, Valido(d => d.Stock = null));

        Assert.Equal(15, await StockTotal(1));
    }

    [Fact]
    public async Task Stock_Negativo_EsRechazado()
    {
        string mensaje = await ErrorAlCrear(Valido(d => d.Stock = -1));

        Assert.Contains("stock no puede ser negativo", mensaje);
    }

    [Fact]
    public async Task Stock_Excesivo_EsRechazado()
    {
        string mensaje = await ErrorAlCrear(Valido(d => d.Stock = LibroService.StockMaximo + 1));

        Assert.Contains("stock no puede pasar", mensaje);
    }

    [Fact]
    public async Task Ebook_IgnoraElStock()
    {
        using var ctx = _bd.Crear();
        var creado = await Servicio(ctx).CrearAsync(Valido(d => { d.Formato = "E-book"; d.Stock = 99; }));

        Assert.Null(creado.Stock);
        Assert.Equal(0, await StockTotal(creado.IdLibro));
    }

    [Fact]
    public async Task Crear_ConAutores_CreaLosNuevosYReutilizaLosExistentes()
    {
        using var ctx = _bd.Crear();
        var creado = await Servicio(ctx).CrearAsync(Valido(d => d.Autor = "Lucía Tzoc,  Ana María  Coyoy"));

        Assert.Equal("Lucía Tzoc, Ana María Coyoy", creado.Autor);
        using var lectura = _bd.Crear();
        Assert.Equal(1, lectura.Set<KemilEditorial.API.Models.Autor>().Count(a => a.Nombre == "Lucía" && a.Apellido == "Tzoc"));
        Assert.Equal(1, lectura.Set<KemilEditorial.API.Models.Autor>().Count(a => a.Nombre == "Ana María" && a.Apellido == "Coyoy"));
    }

    [Fact]
    public async Task Actualizar_ReemplazaLosAutoresDelLibro()
    {
        using (var ctx = _bd.Crear())
        {
            var editado = await Servicio(ctx).ActualizarAsync(1, Valido(d => d.Autor = "Mateo Xicay"));
            Assert.Equal("Mateo Xicay", editado.Autor);
        }
    }

    [Fact]
    public async Task Actualizar_AutorVacio_QuitaTodosLosAutores()
    {
        using var ctx = _bd.Crear();
        var editado = await Servicio(ctx).ActualizarAsync(1, Valido(d => d.Autor = ""));

        Assert.Equal(string.Empty, editado.Autor);
    }

    [Fact]
    public async Task Actualizar_AutorNulo_ConservaLosAutores()
    {
        using var ctx = _bd.Crear();
        var editado = await Servicio(ctx).ActualizarAsync(1, Valido(d => d.Autor = null));

        Assert.Equal("Lucía Tzoc, Mateo Xicay", editado.Autor);
    }

    [Fact]
    public async Task Autor_ConDemasiadosNombres_EsRechazado()
    {
        string mensaje = await ErrorAlCrear(Valido(d => d.Autor = "A B, C D, E F, G H, I J, K L"));

        Assert.Contains("máximo", mensaje);
    }

    [Fact]
    public async Task Autor_ConMarcado_EsRechazado()
    {
        string mensaje = await ErrorAlCrear(Valido(d => d.Autor = "<script>x</script>"));

        Assert.Contains("< ni >", mensaje);
    }
}
