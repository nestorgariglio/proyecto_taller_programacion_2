namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Producto que el proveedor elegido ya vendió, según el historial de compras
/// (RF-15). Proyección mínima y propia del módulo: sin stock, sin precios y sin
/// la entidad de datos (RNF-5).
/// </summary>
public class ProductoDeProveedorDto
{
	public int IdProducto { get; set; }

	public string Codigo { get; set; } = string.Empty;

	public string Nombre { get; set; } = string.Empty;
}
