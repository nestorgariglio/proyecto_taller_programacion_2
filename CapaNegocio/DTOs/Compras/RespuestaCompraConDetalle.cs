using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Resultado de la consulta de una compra con su detalle (RF-53 y RF-54).
/// </summary>
public class RespuestaCompraConDetalle
{
	public ResultadoCompraConDetalle Resultado { get; set; }

	public string Mensaje { get; set; } = string.Empty;

	public CompraConDetalleDto? Compra { get; set; }
}
