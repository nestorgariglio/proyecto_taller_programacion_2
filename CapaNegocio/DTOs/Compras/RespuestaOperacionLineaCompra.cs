using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Resultado de agregar o guardar una línea. <see cref="Lineas"/> es siempre la
/// grilla resultante: ante un rechazo vuelve intacta (RNF-11). El mensaje lo
/// redacta el negocio (RF-55).
/// </summary>
public class RespuestaOperacionLineaCompra
{
	public ResultadoOperacionLineaCompra Resultado { get; set; }

	public string Mensaje { get; set; } = string.Empty;

	public IReadOnlyList<LineaCompraDto> Lineas { get; set; } = Array.Empty<LineaCompraDto>();

	public int? IndiceLineaAfectada { get; set; }

	public decimal MontoTotal { get; set; }

	/// <summary>Campo de precio rechazado, cuando <see cref="Resultado"/> es <c>PrecioInvalido</c>.</summary>
	public CampoPrecio? CampoPrecio { get; set; }

	/// <summary>Motivo del rechazo del precio (RF-22 … RF-26).</summary>
	public MotivoPrecioInvalido? MotivoPrecio { get; set; }
}
