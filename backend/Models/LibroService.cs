using System.Linq.Expressions;
using KemilEditorial.API.Data;
using KemilEditorial.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace KemilEditorial.API.Models
{
    /// <summary>
    /// Lógica de negocio del catálogo (capa Modelo): valida los datos de un libro, lo guarda,
    /// lo da de baja (eliminación lógica) y guarda su portada. No conoce HTTP: el controlador
    /// le entrega datos y un flujo de bytes, y traduce las excepciones a códigos de respuesta.
    /// </summary>
    public class LibroService
    {
        public static readonly string[] Formatos = { "Tapa blanda", "Tapa dura", "E-book" };

        public const decimal PrecioMaximo = 99_999_999.99m; // columna DECIMAL(10,2)
        public const int NombreMaximo = 200;
        public const int DescripcionMaxima = 2000;
        public const int StockMaximo = 1_000_000;
        public const int AutoresMaximo = 5;
        public const int StockMinimoPorDefecto = 5;
        public const long PortadaMaxBytes = 2 * 1024 * 1024;
        public const string RutaPublicaPortadas = "/uploads/portadas/";

        private readonly AppDbContext _db;
        private readonly string _carpetaPortadas;

        public LibroService(AppDbContext db, string carpetaPortadas)
        {
            _db = db;
            _carpetaPortadas = carpetaPortadas;
        }

        private static readonly Expression<Func<Libro, LibroDto>> ADto = l => new LibroDto
        {
            IdLibro = l.IdLibro,
            Nombre = l.Nombre,
            Descripcion = l.Descripcion,
            Formato = l.Formato,
            Precio = l.Precio,
            ISBN = l.ISBN,
            IdGenero = l.IdGenero,
            IdEditorial = l.IdEditorial,
            NombreGenero = l.Genero != null ? l.Genero.Nombre : "Sin categoría",
            Portada = l.Portada,
            AnioEdicion = l.FechaLanzamiento != null ? l.FechaLanzamiento.Value.Year : (int?)null,
            Edicion = l.Edicion,
            Valoracion = l.Valoracion
        };

        /// <summary>Catálogo público: solo libros activos.</summary>
        public async Task<List<LibroDto>> ListarAsync()
        {
            var libros = await _db.Libros.AsNoTracking().Where(l => l.Estado).Select(ADto).ToListAsync();
            await CompletarFichaAsync(libros);
            return libros;
        }

        public async Task<LibroDto> CrearAsync(CrearLibroDto? datos)
        {
            var limpio = await ValidarAsync(datos, idActual: null);

            await using var transaccion = await _db.Database.BeginTransactionAsync();
            var libro = new Libro { Estado = true };
            Aplicar(libro, limpio);
            _db.Libros.Add(libro);
            await _db.SaveChangesAsync();

            await AplicarExistenciasYAutoresAsync(libro, limpio);
            await _db.SaveChangesAsync();
            await transaccion.CommitAsync();

            return await ObtenerAsync(libro.IdLibro);
        }

        public async Task<LibroDto> ActualizarAsync(int idLibro, CrearLibroDto? datos)
        {
            var libro = await BuscarActivoAsync(idLibro);
            var limpio = await ValidarAsync(datos, idActual: idLibro);

            await using var transaccion = await _db.Database.BeginTransactionAsync();
            Aplicar(libro, limpio);
            await AplicarExistenciasYAutoresAsync(libro, limpio);
            await _db.SaveChangesAsync();
            await transaccion.CommitAsync();

            return await ObtenerAsync(idLibro);
        }

        /// <summary>Eliminación lógica (estado = 0): el libro deja de verse pero sus facturas siguen intactas.</summary>
        public async Task EliminarAsync(int idLibro)
        {
            var libro = await BuscarActivoAsync(idLibro);
            libro.Estado = false;
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Guarda la imagen de portada. El tipo se deduce de los primeros bytes del archivo (JPG, PNG o WebP),
        /// nunca del nombre ni del tipo que declare el cliente, y el nombre del archivo lo genera el servidor.
        /// </summary>
        public async Task<LibroDto> GuardarPortadaAsync(int idLibro, Stream contenido, long tamano)
        {
            var libro = await BuscarActivoAsync(idLibro);

            if (tamano <= 0)
                throw new ReglaNegocioException("El archivo de portada está vacío.");
            if (tamano > PortadaMaxBytes)
                throw new ReglaNegocioException("La portada no puede pesar más de 2 MB.");

            using var memoria = new MemoryStream();
            await contenido.CopyToAsync(memoria);
            byte[] bytes = memoria.ToArray();
            if (bytes.Length > PortadaMaxBytes)
                throw new ReglaNegocioException("La portada no puede pesar más de 2 MB.");

            string? extension = DetectarExtension(bytes);
            if (extension is null)
                throw new ReglaNegocioException("La portada debe ser una imagen JPG, PNG o WebP.");

            Directory.CreateDirectory(_carpetaPortadas);
            string nombreArchivo = $"{idLibro}-{Guid.NewGuid():N}{extension}";
            string rutaFisica = Path.Combine(_carpetaPortadas, nombreArchivo);
            string? portadaAnterior = libro.Portada;

            await File.WriteAllBytesAsync(rutaFisica, bytes);
            try
            {
                libro.Portada = RutaPublicaPortadas + nombreArchivo;
                await _db.SaveChangesAsync();
            }
            catch
            {
                File.Delete(rutaFisica); // si no se pudo guardar en la base, no dejar el archivo huérfano
                throw;
            }

            BorrarPortadaSubida(portadaAnterior);
            return await ObtenerAsync(idLibro);
        }

        // ---------- Validación ----------

        private sealed record LibroLimpio(
            string Nombre, string? Descripcion, string Formato, decimal Precio,
            string Isbn, int IdGenero, int IdEditorial,
            int? Stock, List<(string Nombre, string Apellido)>? Autores);

        private async Task<LibroLimpio> ValidarAsync(CrearLibroDto? d, int? idActual)
        {
            if (d is null)
                throw new ReglaNegocioException("Faltan los datos del libro.");

            var errores = new List<string>();

            string nombre = (d.Nombre ?? string.Empty).Trim();
            if (nombre.Length == 0) errores.Add("El título es obligatorio.");
            else if (nombre.Length > NombreMaximo) errores.Add($"El título no puede pasar de {NombreMaximo} caracteres.");
            else if (TieneMarcado(nombre)) errores.Add("El título no puede contener los caracteres < ni >.");

            string? descripcion = string.IsNullOrWhiteSpace(d.Descripcion) ? null : d.Descripcion.Trim();
            if (descripcion is not null)
            {
                if (descripcion.Length > DescripcionMaxima)
                    errores.Add($"La sinopsis no puede pasar de {DescripcionMaxima} caracteres.");
                else if (TieneMarcado(descripcion))
                    errores.Add("La sinopsis no puede contener los caracteres < ni >.");
            }

            string? formato = Formatos.FirstOrDefault(f =>
                string.Equals(f, (d.Formato ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase));
            if (formato is null)
                errores.Add($"Formato no válido. Use: {string.Join(", ", Formatos)}.");

            if (d.Precio <= 0)
                errores.Add("El precio debe ser mayor que cero.");
            else if (d.Precio > PrecioMaximo)
                errores.Add("El precio es demasiado alto.");
            else if (decimal.Round(d.Precio, 2) != d.Precio)
                errores.Add("El precio solo puede tener hasta 2 decimales.");

            string isbn = (d.ISBN ?? string.Empty).Trim();
            string isbnDigitos = SoloDigitosIsbn(isbn);
            bool isbnValido = isbn.Length > 0 && isbn.Length <= 50
                && isbn.All(c => char.IsDigit(c) || c == '-' || c == ' ' || c == 'X' || c == 'x')
                && EsLongitudIsbn(isbnDigitos);
            if (!isbnValido)
                errores.Add("El ISBN debe tener 10 o 13 dígitos (se permiten guiones).");

            if (!await _db.Generos.AnyAsync(g => g.IdGenero == d.IdGenero && g.Estado))
                errores.Add("El género seleccionado no existe.");
            if (!await _db.Editoriales.AnyAsync(e => e.IdEditorial == d.IdEditorial))
                errores.Add("La editorial seleccionada no existe.");

            if (isbnValido)
            {
                // Se compara por dígitos: 978-99922-0-001-1 y 9789992200011 son el mismo ISBN
                var existentes = await _db.Libros.Select(l => new { l.IdLibro, l.ISBN }).ToListAsync();
                if (existentes.Any(l => l.IdLibro != idActual && SoloDigitosIsbn(l.ISBN) == isbnDigitos))
                    errores.Add("Ya existe un libro con ese ISBN.");
            }

            if (d.Stock is < 0)
                errores.Add("El stock no puede ser negativo.");
            else if (d.Stock > StockMaximo)
                errores.Add($"El stock no puede pasar de {StockMaximo:N0} unidades.");

            var autores = ParsearAutores(d.Autor, errores);

            if (errores.Count > 0)
                throw new ReglaNegocioException(string.Join(" ", errores));

            return new LibroLimpio(nombre, descripcion, formato!, d.Precio, isbn, d.IdGenero, d.IdEditorial,
                d.Stock, autores);
        }

        // "Lucía Tzoc, Mateo Xicay" -> [(Lucía, Tzoc), (Mateo, Xicay)]. La última palabra es el apellido.
        // null = no se envió el campo (no se cambian los autores); lista vacía = quitar todos.
        private static List<(string Nombre, string Apellido)>? ParsearAutores(string? texto, List<string> errores)
        {
            if (texto is null) return null;

            var resultado = new List<(string Nombre, string Apellido)>();
            foreach (var crudo in texto.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                string completo = string.Join(' ', crudo.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                if (TieneMarcado(completo))
                {
                    errores.Add("El autor no puede contener los caracteres < ni >.");
                    return resultado;
                }

                int corte = completo.LastIndexOf(' ');
                var autor = corte < 0 ? (completo, string.Empty) : (completo[..corte], completo[(corte + 1)..]);

                if (autor.Item1.Length > 100 || autor.Item2.Length > 100)
                {
                    errores.Add("El nombre de un autor es demasiado largo.");
                    return resultado;
                }
                if (!resultado.Any(a => string.Equals(a.Nombre, autor.Item1, StringComparison.OrdinalIgnoreCase)
                                     && string.Equals(a.Apellido, autor.Item2, StringComparison.OrdinalIgnoreCase)))
                    resultado.Add(autor);
            }

            if (resultado.Count > AutoresMaximo)
                errores.Add($"Se permiten como máximo {AutoresMaximo} autores por libro.");
            return resultado;
        }

        private static bool TieneMarcado(string texto) => texto.Contains('<') || texto.Contains('>');

        private static string SoloDigitosIsbn(string? isbn) =>
            string.Concat((isbn ?? string.Empty)
                .Where(c => char.IsDigit(c) || c == 'X' || c == 'x')
                .Select(char.ToUpperInvariant));

        private static bool EsLongitudIsbn(string digitos) =>
            digitos.Length == 13 ? digitos.All(char.IsDigit)
            : digitos.Length == 10 && digitos[..9].All(char.IsDigit);

        private static void Aplicar(Libro libro, LibroLimpio d)
        {
            libro.Nombre = d.Nombre;
            libro.Descripcion = d.Descripcion;
            libro.Formato = d.Formato;
            libro.Precio = d.Precio;
            libro.ISBN = d.Isbn;
            libro.IdGenero = d.IdGenero;
            libro.IdEditorial = d.IdEditorial;
        }

        // ---------- Existencias y autores ----------

        private async Task AplicarExistenciasYAutoresAsync(Libro libro, LibroLimpio d)
        {
            await AjustarStockAsync(libro, d.Stock);
            await AplicarAutoresAsync(libro.IdLibro, d.Autores);
        }

        // Deja las existencias totales del libro en `stock`. Si sube, suma a la primera bodega;
        // si baja, descuenta empezando por la bodega con más unidades. Los e-books no tienen existencias físicas.
        private async Task AjustarStockAsync(Libro libro, int? stock)
        {
            if (stock is null) return;
            if (string.Equals(libro.Formato?.Trim(), "E-book", StringComparison.OrdinalIgnoreCase)) return;

            var filas = await _db.Inventarios
                .Where(i => i.IdLibro == libro.IdLibro && i.Estado)
                .OrderBy(i => i.IdStock)
                .ToListAsync();

            int diferencia = stock.Value - filas.Sum(i => i.StockDisponible);
            if (diferencia == 0) return;

            if (filas.Count == 0)
            {
                var bodega = await _db.Set<Bodega>().OrderBy(b => b.IdStock).FirstOrDefaultAsync()
                    ?? throw new ReglaNegocioException("No hay bodegas registradas para guardar el stock.");
                _db.Inventarios.Add(new InventarioLibro
                {
                    IdStock = bodega.IdStock,
                    IdLibro = libro.IdLibro,
                    StockDisponible = stock.Value,
                    StockMinimo = StockMinimoPorDefecto,
                    Estado = true
                });
                return;
            }

            if (diferencia > 0)
            {
                filas[0].StockDisponible += diferencia;
                return;
            }

            int pendiente = -diferencia;
            foreach (var fila in filas.OrderByDescending(i => i.StockDisponible))
            {
                int quitar = Math.Min(fila.StockDisponible, pendiente);
                fila.StockDisponible -= quitar;
                pendiente -= quitar;
                if (pendiente == 0) break;
            }
        }

        // Reemplaza los autores del libro por los indicados (los crea si no existen)
        private async Task AplicarAutoresAsync(int idLibro, List<(string Nombre, string Apellido)>? autores)
        {
            if (autores is null) return;

            _db.Set<LibroAutor>().RemoveRange(await _db.Set<LibroAutor>().Where(la => la.IdLibro == idLibro).ToListAsync());

            foreach (var (nombre, apellido) in autores)
            {
                var autor = await _db.Set<Autor>().FirstOrDefaultAsync(a => a.Nombre == nombre && a.Apellido == apellido);
                if (autor is null)
                {
                    autor = new Autor { Nombre = nombre, Apellido = apellido, Estado = true };
                    _db.Set<Autor>().Add(autor);
                    await _db.SaveChangesAsync(); // para obtener su id
                }
                else
                {
                    autor.Estado = true;
                }
                _db.Set<LibroAutor>().Add(new LibroAutor { IdLibro = idLibro, IdAutor = autor.IdAutor });
            }
        }

        // ---------- Apoyo ----------

        private async Task<Libro> BuscarActivoAsync(int idLibro) =>
            await _db.Libros.FirstOrDefaultAsync(l => l.IdLibro == idLibro && l.Estado)
                ?? throw new NoEncontradoException($"El libro {idLibro} no existe.");

        private async Task<LibroDto> ObtenerAsync(int idLibro)
        {
            var libro = await _db.Libros.AsNoTracking().Where(l => l.IdLibro == idLibro).Select(ADto).FirstAsync();
            await CompletarFichaAsync(new List<LibroDto> { libro });
            return libro;
        }

        // Agrega autor, editorial y existencias con unas pocas consultas (no una por libro)
        private async Task CompletarFichaAsync(List<LibroDto> libros)
        {
            if (libros.Count == 0) return;

            var ids = libros.Select(l => l.IdLibro).ToList();
            var idsEditorial = libros.Select(l => l.IdEditorial).Distinct().ToList();

            var existencias = (await _db.Inventarios.AsNoTracking()
                    .Where(i => ids.Contains(i.IdLibro) && i.Estado)
                    .GroupBy(i => i.IdLibro)
                    .Select(g => new { IdLibro = g.Key, Total = g.Sum(i => i.StockDisponible), Minimo = g.Max(i => i.StockMinimo) })
                    .ToListAsync())
                .ToDictionary(x => x.IdLibro);

            var autores = (await (
                    from la in _db.Set<LibroAutor>().AsNoTracking()
                    join a in _db.Set<Autor>().AsNoTracking() on la.IdAutor equals a.IdAutor
                    where ids.Contains(la.IdLibro) && a.Estado
                    orderby la.IdLibroAutor
                    select new { la.IdLibro, a.Nombre, a.Apellido })
                    .ToListAsync())
                .GroupBy(x => x.IdLibro)
                .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(x => $"{x.Nombre} {x.Apellido}".Trim())));

            var editoriales = await _db.Editoriales.AsNoTracking()
                .Where(e => idsEditorial.Contains(e.IdEditorial))
                .ToDictionaryAsync(e => e.IdEditorial, e => e.Nombre);

            foreach (var libro in libros)
            {
                libro.Autor = autores.GetValueOrDefault(libro.IdLibro, string.Empty);
                libro.Editorial = editoriales.GetValueOrDefault(libro.IdEditorial, string.Empty);

                bool digital = string.Equals(libro.Formato?.Trim(), "E-book", StringComparison.OrdinalIgnoreCase);
                if (digital)
                {
                    libro.Stock = null; // los e-books no tienen existencias físicas
                }
                else if (existencias.TryGetValue(libro.IdLibro, out var e))
                {
                    libro.Stock = e.Total;
                    libro.StockMinimo = e.Minimo;
                }
                else
                {
                    libro.Stock = 0;
                    libro.StockMinimo = 5;
                }
            }
        }

        private static string? DetectarExtension(byte[] b)
        {
            if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF) return ".jpg";
            if (b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47
                && b[4] == 0x0D && b[5] == 0x0A && b[6] == 0x1A && b[7] == 0x0A) return ".png";
            if (b.Length >= 12 && b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F'
                && b[8] == 'W' && b[9] == 'E' && b[10] == 'B' && b[11] == 'P') return ".webp";
            return null;
        }

        // Solo borra portadas que subió la aplicación; las de /portadas/ (frontend) no se tocan
        private void BorrarPortadaSubida(string? portada)
        {
            if (portada is null || !portada.StartsWith(RutaPublicaPortadas, StringComparison.Ordinal)) return;
            string archivo = Path.GetFileName(portada); // GetFileName evita rutas con ../
            string ruta = Path.Combine(_carpetaPortadas, archivo);
            if (File.Exists(ruta)) File.Delete(ruta);
        }
    }
}
