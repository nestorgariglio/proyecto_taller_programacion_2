namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Compra recuperada con todas sus líneas como fuente de datos de Reportes
/// (RF-53 y RF-54). Mismos datos de cabecera que el historial; el detalle no
/// incluye el precio de venta, que no se persiste (D-2 y D-17).
/// </summary>
public class CompraConDetalleDto
{
	public int IdCompra { get; set; }

	public DateTime FechaRegistro { get; set; }

	public string RazonSocialProveedor { get; set; } = string.Empty;

	public string NombreUsuario { get; set; } = string.Empty;

	public decimal MontoTotal { get; set; }

	/// <summary><see langword="true"/> = activa; <see langword="false"/> = inactiva.</summary>
	public bool Estado { get; set; }

	public IReadOnlyList<LineaCompraDetalleDto> Lineas { get; set; } = Array.Empty<LineaCompraDetalleDto>();
}
