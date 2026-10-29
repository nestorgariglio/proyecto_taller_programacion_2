namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Producto devuelto por la búsqueda acotada de Compras (RF-15 … RF-17). Sin
/// precios ni stock: el prellenado es una consulta aparte (RNF-5).
/// </summary>
public class ProductoBuscadoDto
{
	public int IdProducto { get; set; }

	public string Codigo { get; set; } = string.Empty;

	public string Nombre { get; set; } = string.Empty;
}
