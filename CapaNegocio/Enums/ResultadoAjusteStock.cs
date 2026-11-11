namespace CapaNegocio.Enums;

/// <summary>
/// Resultado del ajuste manual de stock de un producto (RF-10/RF-11).
/// </summary>
public enum ResultadoAjusteStock
{
	Exito,
	DatosInvalidos,
	ProductoNoEncontrado,
	MotivoRequerido,
	StockInvalido,
	SinPermiso
}
