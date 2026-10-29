namespace CapaNegocio.Enums;

/// <summary>
/// Resultado de la consulta de una compra con su detalle completo
/// (RF-53 y RF-54).
/// </summary>
public enum ResultadoCompraConDetalle
{
	Encontrada,
	CompraNoEncontrada,
	PermisoDenegado,
	ErrorPersistencia
}
