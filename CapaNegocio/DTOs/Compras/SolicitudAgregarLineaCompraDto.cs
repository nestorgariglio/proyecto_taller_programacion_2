namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Solicitud para agregar una línea desde el área de captura (RF-18 … RF-27).
/// Los valores numéricos viajan crudos porque la validación de formato es del
/// negocio (RF-55).
/// </summary>
public class SolicitudAgregarLineaCompraDto
{
	public int IdProducto { get; set; }

	public string? Cantidad { get; set; }

	public string? PrecioCompra { get; set; }

	public string? PrecioVenta { get; set; }

	public IReadOnlyList<LineaCompraDto> LineasActuales { get; set; } = Array.Empty<LineaCompraDto>();
}
