namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Solicitud para guardar la corrección de una línea existente
/// (RF-30 y RF-62). Mismos valores crudos que el alta (RF-55).
/// </summary>
public class SolicitudEditarLineaCompraDto
{
	public int IndiceLinea { get; set; }

	public int IdProducto { get; set; }

	public string? Cantidad { get; set; }

	public string? PrecioCompra { get; set; }

	public string? PrecioVenta { get; set; }

	public IReadOnlyList<LineaCompraDto> LineasActuales { get; set; } = Array.Empty<LineaCompraDto>();
}
