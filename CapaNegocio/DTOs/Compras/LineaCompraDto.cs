namespace CapaNegocio.DTOs.Compras;

public class LineaCompraDto
{
	public int IdProducto { get; set; }

	public string CodigoProducto { get; set; } = string.Empty;

	public string NombreProducto { get; set; } = string.Empty;

	public int Cantidad { get; set; }

	public decimal PrecioCompra { get; set; }

	public decimal PrecioVenta { get; set; }

	public decimal Subtotal { get; set; }
}
