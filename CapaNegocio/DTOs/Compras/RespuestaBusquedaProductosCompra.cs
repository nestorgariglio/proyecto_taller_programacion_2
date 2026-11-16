using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Compras;

public class RespuestaBusquedaProductosCompra
{
	public ResultadoBusquedaCompras Resultado { get; set; }

	public string Mensaje { get; set; } = string.Empty;

	public IReadOnlyList<ProductoBuscadoDto> Productos { get; set; } = Array.Empty<ProductoBuscadoDto>();
}
