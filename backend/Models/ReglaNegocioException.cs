namespace KemilEditorial.API.Models
{
    /// <summary>
    /// Se lanza cuando una petición rompe una regla de negocio (carrito vacío, cantidad inválida,
    /// libro inexistente...). El controlador la traduce a HTTP 400 con { mensaje }.
    /// </summary>
    public class ReglaNegocioException : Exception
    {
        public ReglaNegocioException(string mensaje) : base(mensaje) { }
    }

    /// <summary>Se lanza cuando no hay unidades suficientes. El controlador la traduce a HTTP 409.</summary>
    public class StockInsuficienteException : ReglaNegocioException
    {
        public StockInsuficienteException(string mensaje) : base(mensaje) { }
    }
}
