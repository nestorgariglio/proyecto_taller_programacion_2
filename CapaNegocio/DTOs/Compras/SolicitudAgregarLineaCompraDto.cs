namespace CapaNegocio.DTOs.Compras;

public class SolicitudAgregarLineaCompraDto
{
	public int IdProducto { get; set; }

	public string? Cantidad { get; set; }

	public string? PrecioCompra { get; set; }

	public string? PrecioVenta { get; set; }

	public IReadOnlyList<LineaCompraDto> LineasActuales { get; set; } = Array.Empty<LineaCompraDto>();
}
