namespace CapaNegocio.Enums;

/// <summary>
/// Resultado del alta de un producto (RF-1 … RF-8).
/// </summary>
public enum ResultadoCreacionProducto
{
	Exito,
	DatosInvalidos,
	CodigoDuplicado,
	CategoriaInvalida,

	/// <summary>
	/// El usuario que ejecuta el alta no tiene permiso de escritura del ABM
	/// (solo Administrador o Encargado, RF-11/RF-18/D-18-bis).
	/// </summary>
	SinPermiso
}
