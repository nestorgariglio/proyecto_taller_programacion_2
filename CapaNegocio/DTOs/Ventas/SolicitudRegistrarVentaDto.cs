namespace CapaNegocio.DTOs.Ventas;

/// <summary>
/// Solicitud para procesar el cobro y registro de una venta.
/// </summary>
public record SolicitudRegistrarVentaDto
{
    public int? IdCliente { get; init; }
    public string? DocumentoCliente { get; init; }
    public string? NombreCliente { get; init; }
    public int? IdTipo { get; init; }
    public decimal MontoPago { get; init; }
    public IReadOnlyList<LineaCarritoDto>? Lineas { get; init; }
}
