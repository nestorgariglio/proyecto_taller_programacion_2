namespace CapaNegocio.DTOs.Productos;

/// <summary>
/// Datos de lectura de un producto para la interfaz (RF-14, RF-15, RF-17, RF-20).
/// No expone datos sensibles y la categoría se identifica por su Id, con el
/// nombre resuelto por la lógica de negocio.
/// </summary>
public class ProductoRespuestaDto
{
	public int IdProducto { get; set; }
	public string? Codigo { get; set; }
	public string? Nombre { get; set; }
	public string? Descripcion { get; set; }
	public int IdCategoria { get; set; }
	public string? NombreCategoria { get; set; }
	public decimal PrecioCompra { get; set; }
	public decimal PrecioVenta { get; set; }
	public int Stock { get; set; }
	public bool Estado { get; set; }
	public DateTime FechaRegistro { get; set; }
	public DateTime? FechaModificacion { get; set; }
	public int? IdUsuarioModifica { get; set; }
}
