namespace CapaNegocio.Enums;

/// <summary>
/// Resultado de agregar o editar una línea de la compra en carga
/// (RF-18, RF-20 … RF-30, RF-36, RF-37, RF-62, RF-63).
/// </summary>
public enum ResultadoOperacionLineaCompra
{
	Exito,
	ExitoConAcumulacion,
	LineaNoEncontrada,
	ProductoInvalido,
	ProductoDuplicadoEnEdicion,
	CantidadInvalida,
	PrecioInvalido,
	CantidadAcumuladaSuperaMaximo,
	ErrorPersistencia
}
