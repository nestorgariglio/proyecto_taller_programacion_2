using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Compras;

public class RespuestaPreciosProducto
{
	public ResultadoPreciosProducto Resultado { get; set; }

	public string Mensaje { get; set; } = string.Empty;

	public PreciosProductoDto? Precios { get; set; }
}
