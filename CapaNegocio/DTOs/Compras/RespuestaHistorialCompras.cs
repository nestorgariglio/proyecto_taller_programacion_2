using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Resultado del historial de compras (RF-49 … RF-51).
/// </summary>
public class RespuestaHistorialCompras
{
	public ResultadoHistorialCompras Resultado { get; set; }

	public string Mensaje { get; set; } = string.Empty;

	public IReadOnlyList<HistorialCompraDto> Compras { get; set; } = Array.Empty<HistorialCompraDto>();
}
