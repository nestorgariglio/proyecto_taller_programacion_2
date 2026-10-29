namespace CapaNegocio.Enums;

/// <summary>
/// Resultado de la consulta de precios del producto para prellenar la línea
/// (RF-38 … RF-41).
/// </summary>
public enum ResultadoPreciosProducto
{
	Exito,
	ProductoNoEncontrado,
	ProductoInactivo,
	ErrorPersistencia
}
