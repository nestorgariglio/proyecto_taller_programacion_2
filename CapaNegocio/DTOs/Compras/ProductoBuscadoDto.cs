namespace CapaNegocio.DTOs.Compras;

public class ProductoBuscadoDto
{
	public int IdProducto { get; set; }

	public string Codigo { get; set; } = string.Empty;

	public string Nombre { get; set; } = string.Empty;

	public int Stock { get; set; }

	public int StockMinimo { get; set; } = 5;

	public bool EsBajoStockMinimo => Stock <= StockMinimo;
}
