using KemilEditorial.API.DTOs;

namespace KemilEditorial.API.Services
{
    public class DescuentoService
    {
        public ResumenOrdenDto CalcularOrden(CalcularOrdenRequestDto request)
        {
            decimal subtotalBase = request.Items.Sum(i => i.Precio * i.Cantidad);
            int totalUnidades = request.Items.Sum(i => i.Cantidad);

            // 1. Matriz de Descuento por Volumen (Unidades por título o combinación)
            decimal porcentajeVolumen = 0m;
            string detalleVolumen = "1 a 5 unidades: 0% de descuento";

            if (totalUnidades > 50)
            {
                porcentajeVolumen = 0.15m;
                detalleVolumen = "Más de 50 unidades: 15% de descuento sobre el subtotal";
            }
            else if (totalUnidades >= 21)
            {
                porcentajeVolumen = 0.10m;
                detalleVolumen = "21 a 50 unidades: 10% de descuento sobre el subtotal";
            }
            else if (totalUnidades >= 6)
            {
                porcentajeVolumen = 0.05m;
                detalleVolumen = "6 a 20 unidades: 5% de descuento sobre el subtotal";
            }

            decimal descuentoVolumenMonto = subtotalBase * porcentajeVolumen;
            decimal subtotalTrasVolumen = subtotalBase - descuentoVolumenMonto;

            // 2. Descuento Adicional por Tipo de Cliente (Acumulativo tras el descuento por volumen)
            decimal porcentajeCliente = 0m;
            string detalleCliente = "Cliente Minorista: 0% adicional";

            string tipoNormalizado = request.TipoCliente?.ToLower().Trim() ?? "minorista";
            if (tipoNormalizado == "institucional" || tipoNormalizado == "mayorista")
            {
                porcentajeCliente = 0.08m;
                detalleCliente = "Cliente Mayorista/Institucional: 8% adicional acumulativo";
            }

            decimal descuentoClienteMonto = subtotalTrasVolumen * porcentajeCliente;
            decimal subtotalConDescuentos = subtotalTrasVolumen - descuentoClienteMonto;

            // 3. Impuestos (IVA 12%) y Total Final (RF-03.3)
            decimal impuestos = subtotalConDescuentos * 0.12m;
            decimal totalFinal = subtotalConDescuentos + impuestos;

            return new ResumenOrdenDto
            {
                SubtotalBase = subtotalBase,
                TotalUnidades = totalUnidades,
                PorcentajeVolumen = porcentajeVolumen * 100,
                DescuentoVolumenMonto = Math.Round(descuentoVolumenMonto, 2),
                DetalleReglaVolumen = detalleVolumen,
                PorcentajeCliente = porcentajeCliente * 100,
                DescuentoClienteMonto = Math.Round(descuentoClienteMonto, 2),
                DetalleReglaCliente = detalleCliente,
                TotalDescuentos = Math.Round(descuentoVolumenMonto + descuentoClienteMonto, 2),
                SubtotalConDescuentos = Math.Round(subtotalConDescuentos, 2),
                Impuestos = Math.Round(impuestos, 2),
                TotalFinal = Math.Round(totalFinal, 2)
            };
        }
    }
}