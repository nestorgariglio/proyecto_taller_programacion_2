using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Compras;

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

	public string EstadoSolicitud { get; set; } = "Recibida";

	public CapaNegocio.Compras.ResultadoEnvioCorreo? InfoCorreo { get; set; }
}
