using System.Collections.Generic;

namespace CapaDatos;

/// <summary>
/// Parámetros para registrar una venta en el origen de datos.
/// </summary>
public record ParametrosRegistroVentaDatos(
    int? IdCliente,
    string? DocumentoCliente,
    string? NombreCliente,
    int? IdTipo,
    decimal MontoPago,
    decimal MontoCambio,
    decimal MontoTotal,
    IReadOnlyList<LineaDetalleVentaParametro> Detalles);

/// <summary>
/// Línea de producto enviada al procedimiento almacenado de venta.
/// </summary>
public record LineaDetalleVentaParametro(
    int IdProducto,
    int Cantidad,
    decimal PrecioVenta,
    decimal Subtotal);

/// <summary>
/// Resultado devuelto por la persistencia de una venta.
/// </summary>
public record ResultadoVentaDatos(
    bool Exito,
    int IdVenta,
    int CodigoResultado,
    string Mensaje);
