using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CapaDatos;

/// <summary>
/// Contrato de persistencia transaccional de ventas mediante procedimiento almacenado y consultas de historial/anulación.
/// </summary>
public interface IVentaRepositorio
{
    /// <summary>
    /// Registra la venta y sus detalles de forma atómica en el motor de base de datos.
    /// </summary>
    /// <param name="parametros">Datos de cabecera y líneas de venta.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resultado de la operación con identificador generado o mensaje de error.</returns>
    Task<ResultadoVentaDatos> RegistrarVentaAsync(
        ParametrosRegistroVentaDatos parametros,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene el listado histórico de ventas ordenado de la más reciente a la más antigua.
    /// </summary>
    Task<IReadOnlyList<ResumenVentaDatos>> ObtenerHistorialVentasAsync(
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene las líneas de detalle de una venta específica.
    /// </summary>
    Task<IReadOnlyList<DetalleLineaVentaDatos>> ObtenerDetalleVentaAsync(
        int idVenta,
        CancellationToken ct = default);

    /// <summary>
    /// Anula una venta activa de forma atómica, registrando auditoría y restituyendo el stock de cada producto.
    /// </summary>
    Task<ResultadoVentaDatos> AnularVentaAsync(
        int idVenta,
        int idUsuarioAnula,
        string motivoAnulacion,
        CancellationToken ct = default);
}
