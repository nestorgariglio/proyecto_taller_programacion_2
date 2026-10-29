using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Resultado de la búsqueda acotada de productos (RF-15 … RF-17).
/// </summary>
public class RespuestaBusquedaProductosCompra
{
	public ResultadoBusquedaCompras Resultado { get; set; }

	public string Mensaje { get; set; } = string.Empty;

	public IReadOnlyList<ProductoBuscadoDto> Productos { get; set; } = Array.Empty<ProductoBuscadoDto>();
}
