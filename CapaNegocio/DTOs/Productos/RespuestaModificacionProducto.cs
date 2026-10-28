using CapaNegocio.Enums;
namespace CapaNegocio.DTOs.Productos;

/// <summary>
/// Respuesta de la edición de un producto (RF-9).
/// </summary>
public class RespuestaModificacionProducto
{
	public ResultadoModificacionProducto Resultado { get; set; }
	public ProductoRespuestaDto? Producto { get; set; }
	public string Mensaje { get; set; } = string.Empty;

	/// <summary>
	/// Indica que el PrecioVenta informado es menor que el PrecioCompra. La
	/// advertencia NO bloquea el guardado (RF-8/D-19): la interfaz la usa para
	/// preguntar al usuario ANTES de persistir.
	/// </summary>
	public bool AdvertenciaPrecioVentaMenorCompra { get; set; }
}
