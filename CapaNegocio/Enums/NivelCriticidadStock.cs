namespace CapaNegocio.Enums;

/// <summary>
/// Clasificación excluyente de criticidad de stock de un producto (RF-8, RF-12).
/// Es un concepto de negocio, no un dato persistido: el producto no guarda
/// ningún stock mínimo y el nivel se calcula con el umbral global vigente.
/// </summary>
public enum NivelCriticidadStock
{
	/// <summary>
	/// Stock suficiente: stock mayor o igual al umbral, o cualquier stock
	/// cuando el umbral está en modo degradado (RF-10).
	/// </summary>
	Normal,

	/// <summary>
	/// Bajo el umbral: stock mayor que 0 y estrictamente menor que el umbral (RF-12).
	/// </summary>
	BajoUmbral,

	/// <summary>
	/// Agotado: stock 0 o negativo (D-11). Es el único nivel crítico en modo
	/// degradado (RF-10).
	/// </summary>
	Agotado
}
