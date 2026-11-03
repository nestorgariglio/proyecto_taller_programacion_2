namespace CapaNegocio.Enums;

/// <summary>
/// Resultado del registro transaccional de una compra (RF-4 … RF-8, RF-14,
/// RF-18 … RF-28, RF-37, RF-44 … RF-46, RF-64).
/// </summary>
public enum ResultadoRegistroCompra
{
	Exito,
	PermisoDenegado,
	ProveedorInvalido,
	UsuarioInvalido,
	ProductoInvalido,
	CantidadInvalida,
	PrecioInvalido,
	SinItems,
	MontoTotalSuperaMaximo,
	ErrorPersistencia
}
