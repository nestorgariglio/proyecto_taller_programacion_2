namespace CapaNegocio.Enums;

/// <summary>
/// Resultado de la reactivación de un producto (RF-13).
/// </summary>
public enum ResultadoReactivacionProducto
{
	Exito,
	DatosInvalidos,
	ProductoNoEncontrado,
	ProductoYaActivo,
	CategoriaInactiva,

	/// <summary>
	/// El usuario que ejecuta la reactivación no tiene permiso de escritura del
	/// ABM (solo Administrador o Encargado, RF-11/RF-18/D-18-bis).
	/// </summary>
	SinPermiso
}
