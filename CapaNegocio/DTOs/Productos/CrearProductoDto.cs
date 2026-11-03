namespace CapaNegocio.DTOs.Productos;

/// <summary>
/// Datos de entrada para el alta manual de un producto (RF-1 … RF-7, RF-20).
/// Los textos viajan crudos: el Recorte de bordes lo aplica <c>ProductoNegocio</c>
/// antes de validar y persistir.
/// </summary>
public class CrearProductoDto
{
	/// <summary>
	/// Código funcional del producto, sin recortar. Debe quedar numérico de
	/// 8 a 13 dígitos tras el Recorte (RF-2/D-9). Se guarda como texto para
	/// preservar los ceros a la izquierda.
	/// </summary>
	public string? Codigo { get; set; }

	/// <summary>
	/// Nombre del producto, sin recortar. Obligatorio, de 3 a 100 caracteres
	/// tras el Recorte (RF-4/D-10).
	/// </summary>
	public string? Nombre { get; set; }

	/// <summary>
	/// Descripción del producto, sin recortar. Opcional, hasta 300 caracteres
	/// tras el Recorte (RF-5/D-10).
	/// </summary>
	public string? Descripcion { get; set; }

	/// <summary>
	/// Identificador de la categoría existente y activa a la que se vincula
	/// el producto (RF-6/D-5). La relación se expresa por Id, no por descripción.
	/// </summary>
	public int IdCategoria { get; set; }

	/// <summary>
	/// Precio de compra del producto; debe ser mayor o igual a 0 (RF-7).
	/// </summary>
	public decimal PrecioCompra { get; set; }

	/// <summary>
	/// Precio de venta del producto; debe ser mayor o igual a 0 (RF-7). Si es
	/// menor que el de compra, no se bloquea el alta: solo se advierte (RF-8/D-19).
	/// </summary>
	public decimal PrecioVenta { get; set; }

	/// <summary>
	/// Usuario identificado que registra el producto (auditoría, RF-20). Es
	/// obligatorio: sin él no se persiste la operación.
	/// </summary>
	public int IdUsuario { get; set; }
}
