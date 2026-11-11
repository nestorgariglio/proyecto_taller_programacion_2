namespace CapaNegocio.Enums;

/// <summary>
/// Acción aplicable al cambio de estado de un producto (RF-21/RF-22).
/// La determina la CapaNegocio a partir del estado <b>actual</b> del producto
/// registrado en la base de datos, no del estado que muestra el listado.
/// </summary>
public enum AccionCambioEstadoProducto
{
	/// <summary>
	/// No hay acción aplicable (identificador inválido o producto inexistente).
	/// Siempre viaja con <c>PuedeEjecutarse = false</c> (RF-23).
	/// </summary>
	Ninguna = 0,

	/// <summary>
	/// El producto está activo: corresponde la baja lógica (RF-12).
	/// </summary>
	Baja = 1,

	/// <summary>
	/// El producto está inactivo: corresponde la reactivación (RF-13).
	/// </summary>
	Reactivacion = 2
}
