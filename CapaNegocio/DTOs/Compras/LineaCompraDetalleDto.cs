namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Línea del detalle de una compra ya registrada (RF-54). Sin precio de venta:
/// no se persiste (D-2 y D-17).
/// </summary>
public class LineaCompraDetalleDto
{
	public string CodigoProducto { get; set; } = string.Empty;

	public string NombreProducto { get; set; } = string.Empty;

	public int Cantidad { get; set; }

	public decimal PrecioCompra { get; set; }

	public decimal Subtotal { get; set; }
}
