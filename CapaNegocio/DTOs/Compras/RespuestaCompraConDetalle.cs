using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Compras;

public class RespuestaCompraConDetalle
{
	public ResultadoCompraConDetalle Resultado { get; set; }

	public string Mensaje { get; set; } = string.Empty;

	public CompraConDetalleDto? Compra { get; set; }
}
