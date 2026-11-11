using KemilEditorial.API.DTOs;

namespace KemilEditorial.API.Models
{
    /// <summary>
    /// Motor de descuentos (RF-03.2 y RF-03.3). Capa Modelo: no conoce HTTP ni la base de datos,
    /// solo recibe renglones con su precio ya resuelto y devuelve el desglose.
    ///
    /// Orden de cálculo (cada monto se redondea a 2 decimales, de modo que las cifras
    /// que ve el usuario siempre suman exactamente el total):
    ///   1. Descuento por volumen sobre el subtotal base.
    ///   2. Descuento de cliente (institucional) sobre lo que queda tras el volumen (acumulativo).
    ///   3. IVA sobre el subtotal con descuentos.
    /// </summary>
    public class DescuentoService
    {
        public const decimal TasaIva = 0.12m;
        public const decimal TasaClienteInstitucional = 0.08m;

        // Escala por unidades totales del carrito. Se evalúa de mayor a menor.
        private static readonly (int DesdeUnidades, decimal Tasa, string Detalle)[] EscalaVolumen =
        {
            (51, 0.15m, "Más de 50 unidades: 15% de descuento sobre el subtotal"),
            (21, 0.10m, "21 a 50 unidades: 10% de descuento sobre el subtotal"),
            (6,  0.05m, "6 a 20 unidades: 5% de descuento sobre el subtotal"),
            (1,  0.00m, "1 a 5 unidades: 0% de descuento"),
        };

        public ResumenOrdenDto CalcularOrden(CalcularOrdenRequestDto request)
        {
            Validar(request);

            bool esInstitucional = EsInstitucional(request.TipoCliente);

            decimal subtotalBase = Redondear(request.Items.Sum(i => i.Precio * i.Cantidad));
            int totalUnidades = request.Items.Sum(i => i.Cantidad);

            // 1. Volumen
            var (tasaVolumen, detalleVolumen) = ReglaDeVolumen(totalUnidades);
            decimal descuentoVolumen = Redondear(subtotalBase * tasaVolumen);
            decimal subtotalTrasVolumen = subtotalBase - descuentoVolumen;

            // 2. Cliente (acumulativo, tras el volumen)
            decimal tasaCliente = esInstitucional ? TasaClienteInstitucional : 0m;
            string detalleCliente = esInstitucional
                ? "Cliente Mayorista/Institucional: 8% adicional acumulativo"
                : "Cliente Minorista: 0% adicional";
            decimal descuentoCliente = Redondear(subtotalTrasVolumen * tasaCliente);
            decimal subtotalConDescuentos = subtotalTrasVolumen - descuentoCliente;

            // 3. Impuestos y total
            decimal impuestos = Redondear(subtotalConDescuentos * TasaIva);
            decimal totalFinal = subtotalConDescuentos + impuestos;

            return new ResumenOrdenDto
            {
                SubtotalBase = subtotalBase,
                TotalUnidades = totalUnidades,
                PorcentajeVolumen = tasaVolumen * 100,
                DescuentoVolumenMonto = descuentoVolumen,
                DetalleReglaVolumen = detalleVolumen,
                PorcentajeCliente = tasaCliente * 100,
                DescuentoClienteMonto = descuentoCliente,
                DetalleReglaCliente = detalleCliente,
                TotalDescuentos = descuentoVolumen + descuentoCliente,
                SubtotalConDescuentos = subtotalConDescuentos,
                Impuestos = impuestos,
                TotalFinal = totalFinal
            };
        }

        // Redondeo comercial (0.5 sube). Math.Round por defecto usa redondeo bancario (0.345 -> 0.34).
        private static decimal Redondear(decimal valor) =>
            Math.Round(valor, 2, MidpointRounding.AwayFromZero);

        private static (decimal Tasa, string Detalle) ReglaDeVolumen(int unidades)
        {
            foreach (var regla in EscalaVolumen)
                if (unidades >= regla.DesdeUnidades)
                    return (regla.Tasa, regla.Detalle);

            return (0m, EscalaVolumen[^1].Detalle);
        }

        private static bool EsInstitucional(string? tipoCliente)
        {
            string tipo = (tipoCliente ?? string.Empty).Trim().ToLowerInvariant();
            return tipo switch
            {
                "" or "minorista" => false,
                "institucional" or "mayorista" => true,
                _ => throw new ReglaNegocioException(
                    $"Tipo de cliente no válido: «{tipoCliente}». Use Minorista o Institucional.")
            };
        }

        private static void Validar(CalcularOrdenRequestDto? request)
        {
            if (request?.Items is null || request.Items.Count == 0)
                throw new ReglaNegocioException("El carrito no contiene productos.");

            foreach (var item in request.Items)
            {
                if (item.Cantidad <= 0)
                    throw new ReglaNegocioException(
                        $"La cantidad de «{item.Nombre}» debe ser mayor que cero.");
                if (item.Precio < 0)
                    throw new ReglaNegocioException(
                        $"El precio de «{item.Nombre}» no puede ser negativo.");
            }
        }
    }
}
