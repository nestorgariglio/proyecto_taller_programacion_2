using CapaNegocio.Enums;
namespace CapaNegocio.DTOs.Productos;

/// <summary>
/// Confirmación del cambio de estado de un producto (RF-21, RF-22, RF-23, RF-24):
/// la acción aplicable, el título del diálogo y el mensaje ya compuestos en la
/// lógica de negocio, junto con la indicación de si la operación puede ejecutarse.
/// La interfaz solo presenta estos datos y, tras la confirmación del usuario,
/// ejecuta la acción informada.
/// </summary>
public class ConfirmacionCambioEstadoProducto
{
	/// <summary>
	/// Acción que corresponde ejecutar si el usuario confirma (RF-21);
	/// <see cref="AccionCambioEstadoProducto.Ninguna"/> cuando no hay nada que ejecutar.
	/// </summary>
	public AccionCambioEstadoProducto Accion { get; set; }

	/// <summary>
	/// <c>true</c> solo si <see cref="Accion"/> es distinta de
	/// <see cref="AccionCambioEstadoProducto.Ninguna"/>; la interfaz no recalcula
	/// la condición (RF-22).
	/// </summary>
	public bool PuedeEjecutarse { get; set; }

	/// <summary>
	/// Identificador del producto consultado; <c>null</c> si el identificador es
	/// inválido o el producto no existe (RF-23).
	/// </summary>
	public int? IdProducto { get; set; }

	/// <summary>
	/// Nombre del producto consultado; <c>null</c> en los casos sin producto (RF-23).
	/// </summary>
	public string? NombreProducto { get; set; }

	/// <summary>
	/// Título del diálogo de confirmación, ya compuesto en español (RF-21).
	/// </summary>
	public string TituloDialogo { get; set; } = string.Empty;

	/// <summary>
	/// Pregunta de confirmación o aviso, ya compuesto en español y listo para
	/// mostrar (RF-21, RF-23).
	/// </summary>
	public string Mensaje { get; set; } = string.Empty;
}
