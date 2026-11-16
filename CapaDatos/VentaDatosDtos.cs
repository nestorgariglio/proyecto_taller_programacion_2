using System;
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
    IReadOnlyList<LineaDetalleVentaParametro> Detalles,
    int? IdUsuario = null);

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

/// <summary>
/// Fila de cabecera de venta para el historial y supervisión de anulaciones.
/// </summary>
public record ResumenVentaDatos(
    int IdVenta,
    string NroComprobante,
    DateTime FechaRegistro,
    string DocumentoCliente,
    string NombreCliente,
    string NombreOperador,
    decimal MontoTotal,
    decimal MontoPago,
    decimal MontoCambio,
    bool Estado,
    string? MotivoAnulacion = null,
    DateTime? FechaAnulacion = null,
    string? NombreUsuarioAnula = null);

/// <summary>
/// Renglón de detalle de una venta registrada.
/// </summary>
public record DetalleLineaVentaDatos(
    int IdProducto,
    string Codigo,
    string NombreProducto,
    int Cantidad,
    decimal PrecioVenta,
    decimal Subtotal);
