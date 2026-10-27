namespace CapaNegocio.Enums;

/// <summary>
/// Resultado de la edición de un producto (RF-9).
/// </summary>
public enum ResultadoModificacionProducto
{
	Exito,
	DatosInvalidos,
	ProductoNoEncontrado,

	/// <summary>
	/// Se conserva por coherencia con el contrato aprobado del plan, pero la
	/// edición no puede producirla: el Código es inmutable y no viaja en el
	/// DTO de modificación (D-11).
	/// </summary>
	CodigoDuplicado,

	CategoriaInvalida,

	/// <summary>
	/// El usuario que ejecuta la modificación no tiene permiso de escritura del
	/// ABM (solo Administrador o Encargado, RF-11/RF-18/D-18-bis).
	/// </summary>
	SinPermiso
}
