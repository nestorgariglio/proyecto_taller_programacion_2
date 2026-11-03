using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Resultado del registro de la compra (RF-44 … RF-46). No incluye la lista de
/// líneas: ante un rechazo la pantalla conserva las suyas (RNF-11). Los campos
/// de línea y de precio identifican el primer error según RF-28.
/// </summary>
public class RespuestaRegistroCompra
{
	public ResultadoRegistroCompra Resultado { get; set; }

	public string Mensaje { get; set; } = string.Empty;

	public int? IdCompra { get; set; }

	public DateTime? FechaRegistro { get; set; }

	public decimal MontoTotal { get; set; }

	public int? IndiceLineaAfectada { get; set; }

	public int? IdProductoAfectado { get; set; }

	public CampoPrecio? CampoPrecio { get; set; }

	public MotivoPrecioInvalido? MotivoPrecio { get; set; }
}
