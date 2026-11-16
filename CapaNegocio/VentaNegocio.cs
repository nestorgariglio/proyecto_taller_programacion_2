using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CapaDatos;
using CapaNegocio.DTOs.Ventas;

namespace CapaNegocio;

/// <summary>
/// Procesa las operaciones de validación de cobro, cálculo de vuelto y registro transaccional de ventas.
/// </summary>
public class VentaNegocio
{
    private readonly IVentaRepositorio _ventaRepositorio;

    /// <summary>
    /// Inicializa una nueva instancia con el repositorio de ventas indicado.
    /// </summary>
    /// <param name="ventaRepositorio">Repositorio para la persistencia transaccional.</param>
    public VentaNegocio(IVentaRepositorio ventaRepositorio)
    {
        _ventaRepositorio = ventaRepositorio ?? throw new ArgumentNullException(nameof(ventaRepositorio));
    }

    /// <summary>
    /// Valida los datos del cobro, calcula el vuelto y registra la venta de forma atómica.
    /// </summary>
    /// <param name="solicitud">Datos de la venta, cliente, pago y productos.</param>
    /// <param name="ct">Token de cancelación opcional.</param>
    /// <returns>Resultado del registro con montos calculados o mensajes de error.</returns>
    public async Task<RespuestaVentaDto> RegistrarVentaAsync(
        SolicitudRegistrarVentaDto solicitud,
        CancellationToken ct = default)
    {
        if (solicitud is null)
        {
            return new RespuestaVentaDto
            {
                Exito = false,
                Mensaje = "La solicitud de venta es requerida."
            };
        }

        var lineas = solicitud.Lineas;
        if (lineas is null || lineas.Count == 0)
        {
            return new RespuestaVentaDto
            {
                Exito = false,
                Mensaje = "El carrito está vacío. Agregue productos antes de cobrar."
            };
        }

        if (lineas.Any(l => l.Cantidad <= 0 || l.PrecioUnitario <= 0))
        {
            return new RespuestaVentaDto
            {
                Exito = false,
                Mensaje = "Todas las líneas deben tener una cantidad y un precio mayores a cero."
            };
        }

        decimal montoTotal = lineas.Sum(l => l.Subtotal);

        if (solicitud.MontoPago < montoTotal)
        {
            return new RespuestaVentaDto
            {
                Exito = false,
                MontoTotal = montoTotal,
                MontoPago = solicitud.MontoPago,
                Mensaje = "El monto ingresado es insuficiente para cubrir el total de la venta."
            };
        }

        decimal montoCambio = solicitud.MontoPago - montoTotal;

        string? documento = solicitud.DocumentoCliente;
        string? nombre = solicitud.NombreCliente;

        if (!solicitud.IdCliente.HasValue || solicitud.IdCliente.Value <= 0)
        {
            documento = string.IsNullOrWhiteSpace(documento) ? "0" : documento.Trim();
            nombre = string.IsNullOrWhiteSpace(nombre) ? "Consumidor Final" : nombre.Trim();
        }

        var detallesParametro = lineas.Select(l => new LineaDetalleVentaParametro(
            l.IdProducto,
            l.Cantidad,
            l.PrecioUnitario,
            l.Subtotal
        )).ToList();

        var parametros = new ParametrosRegistroVentaDatos(
            solicitud.IdCliente,
            documento,
            nombre,
            solicitud.IdTipo,
            solicitud.MontoPago,
            montoCambio,
            montoTotal,
            detallesParametro
        );

        try
        {
            var resultado = await _ventaRepositorio.RegistrarVentaAsync(parametros, ct);

            if (resultado.Exito)
            {
                return new RespuestaVentaDto
                {
                    Exito = true,
                    IdVenta = resultado.IdVenta,
                    MontoTotal = montoTotal,
                    MontoPago = solicitud.MontoPago,
                    MontoCambio = montoCambio,
                    Mensaje = $"Venta N° {resultado.IdVenta} registrada con éxito.",
                    FechaRegistro = DateTime.Now
                };
            }

            return new RespuestaVentaDto
            {
                Exito = false,
                MontoTotal = montoTotal,
                MontoPago = solicitud.MontoPago,
                MontoCambio = montoCambio,
                Mensaje = string.IsNullOrWhiteSpace(resultado.Mensaje)
                    ? "No se pudo registrar la venta en la base de datos."
                    : resultado.Mensaje
            };
        }
        catch (Exception)
        {
            return new RespuestaVentaDto
            {
                Exito = false,
                MontoTotal = montoTotal,
                MontoPago = solicitud.MontoPago,
                MontoCambio = montoCambio,
                Mensaje = "Ocurrió un error inesperado al procesar la venta. Intente nuevamente."
            };
        }
    }
}
