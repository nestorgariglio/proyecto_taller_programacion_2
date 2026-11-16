using System;
using System.Collections.Generic;
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
            detallesParametro,
            solicitud.IdUsuario
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

    /// <summary>
    /// Obtiene el listado histórico de ventas registradas.
    /// </summary>
    public async Task<IReadOnlyList<ResumenVentaDatos>> ObtenerHistorialVentasAsync(
        CancellationToken ct = default)
    {
        try
        {
            return await _ventaRepositorio.ObtenerHistorialVentasAsync(ct);
        }
        catch (Exception)
        {
            return Array.Empty<ResumenVentaDatos>();
        }
    }

    /// <summary>
    /// Obtiene los renglones de detalle de una venta específica.
    /// </summary>
    public async Task<IReadOnlyList<DetalleLineaVentaDatos>> ObtenerDetalleVentaAsync(
        int idVenta,
        CancellationToken ct = default)
    {
        if (idVenta <= 0)
            return Array.Empty<DetalleLineaVentaDatos>();

        try
        {
            return await _ventaRepositorio.ObtenerDetalleVentaAsync(idVenta, ct);
        }
        catch (Exception)
        {
            return Array.Empty<DetalleLineaVentaDatos>();
        }
    }

    /// <summary>
    /// Anula una venta activa restituyendo el stock al inventario (exclusivo para el rol Encargado).
    /// </summary>
    public async Task<RespuestaVentaDto> AnularVentaAsync(
        int idVenta,
        int idUsuarioAnula,
        string? rol,
        string? motivo,
        CancellationToken ct = default)
    {
        if (!PoliticaAcceso.PuedeAnularOperaciones(rol))
        {
            return new RespuestaVentaDto
            {
                Exito = false,
                Mensaje = "Solo el Encargado tiene permisos para anular ventas."
            };
        }

        if (idVenta <= 0)
        {
            return new RespuestaVentaDto
            {
                Exito = false,
                Mensaje = "Seleccione una venta válida para anular."
            };
        }

        if (idUsuarioAnula <= 0)
        {
            return new RespuestaVentaDto
            {
                Exito = false,
                Mensaje = "No se pudo identificar al usuario responsable de la anulación."
            };
        }

        string motivoRecortado = motivo?.Trim() ?? string.Empty;
        if (motivoRecortado.Length < 3)
        {
            return new RespuestaVentaDto
            {
                Exito = false,
                Mensaje = "Ingrese un motivo de anulación de al menos 3 caracteres."
            };
        }

        if (motivoRecortado.Length > 300)
        {
            return new RespuestaVentaDto
            {
                Exito = false,
                Mensaje = "El motivo de anulación no puede superar los 300 caracteres."
            };
        }

        try
        {
            var resultado = await _ventaRepositorio.AnularVentaAsync(idVenta, idUsuarioAnula, motivoRecortado, ct);
            return new RespuestaVentaDto
            {
                Exito = resultado.Exito,
                IdVenta = idVenta,
                Mensaje = string.IsNullOrWhiteSpace(resultado.Mensaje)
                    ? (resultado.Exito ? "Venta anulada con éxito." : "No se pudo anular la venta.")
                    : resultado.Mensaje
            };
        }
        catch (Exception)
        {
            return new RespuestaVentaDto
            {
                Exito = false,
                Mensaje = "Ocurrió un error inesperado al intentar anular la venta."
            };
        }
    }
}
