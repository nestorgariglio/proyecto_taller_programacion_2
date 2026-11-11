namespace CapaNegocio.Enums;

/// <summary>
/// Resultado de la consulta del historial de compras registradas
/// (RF-49 … RF-51, RF-53).
/// </summary>
public enum ResultadoHistorialCompras
{
	ConCompras,
	SinCompras,
	PermisoDenegado,
	ErrorPersistencia
}
