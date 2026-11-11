namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Resultado de quitar una línea de la compra en carga (RF-29). <see cref="Exito"/>
/// y <see cref="Mensaje"/> cubren el índice fuera de rango; no toca la base
/// (RNF-4).
/// </summary>
public class RespuestaCompraEnCarga
{
	public bool Exito { get; set; }

	public string Mensaje { get; set; } = string.Empty;

	public IReadOnlyList<LineaCompraDto> Lineas { get; set; } = Array.Empty<LineaCompraDto>();

	public decimal MontoTotal { get; set; }
}
