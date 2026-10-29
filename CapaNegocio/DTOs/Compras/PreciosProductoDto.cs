namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Precios del producto usados para prellenar el área de captura (RF-38). Solo
/// valores; nunca la entidad de datos.
/// </summary>
public class PreciosProductoDto
{
	public int IdProducto { get; set; }

	public decimal PrecioCompra { get; set; }

	public decimal PrecioVenta { get; set; }
}
