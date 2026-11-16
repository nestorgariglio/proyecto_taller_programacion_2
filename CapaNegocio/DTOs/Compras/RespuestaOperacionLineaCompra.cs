using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Compras;

public class RespuestaOperacionLineaCompra
{
	public ResultadoOperacionLineaCompra Resultado { get; set; }

	public string Mensaje { get; set; } = string.Empty;

	public IReadOnlyList<LineaCompraDto> Lineas { get; set; } = Array.Empty<LineaCompraDto>();

	public int? IndiceLineaAfectada { get; set; }

	public decimal MontoTotal { get; set; }

	public CampoPrecio? CampoPrecio { get; set; }

	public MotivoPrecioInvalido? MotivoPrecio { get; set; }
}
