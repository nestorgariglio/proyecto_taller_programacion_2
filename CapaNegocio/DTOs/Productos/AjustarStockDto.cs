namespace CapaNegocio.DTOs.Productos;

/// <summary>
/// Datos de entrada para el ajuste manual de stock de un producto (RF-10/RF-11).
/// El stock se informa como <b>valor absoluto</b> (no como diferencia) y el
/// motivo es de texto libre: el Recorte de bordes lo aplica <c>ProductoNegocio</c>
/// antes de validar y persistir.
/// </summary>
public class AjustarStockDto
{
	/// <summary>
	/// Identificador del producto cuyo stock se desea ajustar.
	/// </summary>
	public int IdProducto { get; set; }

	/// <summary>
	/// Nuevo stock del producto como valor absoluto. Debe ser mayor o igual a 0
	/// (D-14): no se admite stock negativo.
	/// </summary>
	public int NuevoStock { get; set; }

	/// <summary>
	/// Motivo del ajuste, de texto libre y obligatorio, de hasta 300 caracteres
	/// tras el Recorte (RF-10).
	/// </summary>
	public string? Motivo { get; set; }

	/// <summary>
	/// Usuario identificado que realiza el ajuste (auditoría, RF-20/D-21). Es
	/// obligatorio y debe existir: <c>ProductoNegocio</c> resuelve su rol para
	/// validar el permiso de gestión (RF-11/D-18-bis). El rol no viaja en el DTO.
	/// </summary>
	public int IdUsuario { get; set; }
}
