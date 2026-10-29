using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Resultado de la consulta de precios para prellenar la línea (RF-38 … RF-41).
/// </summary>
public class RespuestaPreciosProducto
{
	public ResultadoPreciosProducto Resultado { get; set; }

	public string Mensaje { get; set; } = string.Empty;

	public PreciosProductoDto? Precios { get; set; }
}
