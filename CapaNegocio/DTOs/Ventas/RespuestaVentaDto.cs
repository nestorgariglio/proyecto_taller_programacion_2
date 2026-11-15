using System;

namespace CapaNegocio.DTOs.Ventas;

/// <summary>
/// Resultado del procesamiento de cobro y registro de una venta.
/// </summary>
public record RespuestaVentaDto
{
    public bool Exito { get; init; }
    public int? IdVenta { get; init; }
    public decimal MontoTotal { get; init; }
    public decimal MontoPago { get; init; }
    public decimal MontoCambio { get; init; }
    public string? Mensaje { get; init; }
    public DateTime? FechaRegistro { get; init; }
}
