namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Subtotal del área de captura (RF-65): solo es válido cuando producto,
/// cantidad y ambos precios son válidos; si no, la pantalla muestra «—».
/// </summary>
public class SubtotalLineaDto
{
	public bool EsValido { get; set; }

	public decimal? Subtotal { get; set; }
}
