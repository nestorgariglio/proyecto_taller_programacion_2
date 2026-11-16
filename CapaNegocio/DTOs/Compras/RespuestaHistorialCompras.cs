using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Compras;

public class RespuestaHistorialCompras
{
	public ResultadoHistorialCompras Resultado { get; set; }

	public string Mensaje { get; set; } = string.Empty;

	public IReadOnlyList<HistorialCompraDto> Compras { get; set; } = Array.Empty<HistorialCompraDto>();
}
