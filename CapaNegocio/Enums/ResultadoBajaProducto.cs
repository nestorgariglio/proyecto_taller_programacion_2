namespace CapaNegocio.Enums;

/// <summary>
/// Resultado de la baja lógica de un producto (RF-12).
/// </summary>
public enum ResultadoBajaProducto
{
	Exito,
	DatosInvalidos,
	ProductoNoEncontrado,
	ProductoYaInactivo,

	/// <summary>
	/// El usuario que ejecuta la baja no tiene permiso de escritura del ABM
	/// (solo Administrador o Encargado, RF-11/RF-18/D-18-bis).
	/// </summary>
	SinPermiso
}
