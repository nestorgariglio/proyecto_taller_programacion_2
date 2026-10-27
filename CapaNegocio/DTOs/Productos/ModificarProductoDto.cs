namespace CapaNegocio.DTOs.Productos;

/// <summary>
/// Datos de entrada para la edición de un producto existente (RF-9).
/// Los textos viajan crudos: el Recorte de bordes lo aplica <c>ProductoNegocio</c>
/// antes de validar y persistir. No incluye el Código: es inmutable (D-11), y
/// tampoco el Stock: solo se ajusta con la operación de ajuste de stock.
/// </summary>
public class ModificarProductoDto
{
	/// <summary>
	/// Identificador del producto existente que se desea editar (activo o inactivo: D-20).
	/// </summary>
	public int IdProducto { get; set; }

	/// <summary>
	/// Nombre del producto, sin recortar. Obligatorio, de 3 a 100 caracteres
	/// tras el Recorte (RF-4/RF-9).
	/// </summary>
	public string? Nombre { get; set; }

	/// <summary>
	/// Descripción del producto, sin recortar. Opcional, hasta 300 caracteres
	/// tras el Recorte (RF-5/RF-9).
	/// </summary>
	public string? Descripcion { get; set; }

	/// <summary>
	/// Identificador de la categoría existente y activa a la que se vincula
	/// el producto (RF-6/RF-9). La relación se expresa por Id, no por descripción.
	/// </summary>
	public int IdCategoria { get; set; }

	/// <summary>
	/// Precio de compra del producto; debe ser mayor o igual a 0 (RF-7/RF-9).
	/// </summary>
	public decimal PrecioCompra { get; set; }

	/// <summary>
	/// Precio de venta del producto; debe ser mayor o igual a 0 (RF-7/RF-9). Si es
	/// menor que el de compra, no se bloquea la edición: solo se advierte (RF-8/D-19).
	/// </summary>
	public decimal PrecioVenta { get; set; }

	/// <summary>
	/// Usuario identificado que realiza la modificación (auditoría, RF-20/D-17).
	/// Es obligatorio y se persiste en <c>Producto.IdUsuarioModifica</c>; el
	/// usuario creador original no se altera.
	/// </summary>
	public int IdUsuario { get; set; }
}
