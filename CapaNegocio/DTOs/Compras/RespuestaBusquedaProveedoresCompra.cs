using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Resultado de la búsqueda acotada de proveedores (RF-9 … RF-12).
/// </summary>
public class RespuestaBusquedaProveedoresCompra
{
	public ResultadoBusquedaCompras Resultado { get; set; }

	public string Mensaje { get; set; } = string.Empty;

	public IReadOnlyList<ProveedorSeleccionadoDto> Proveedores { get; set; } = Array.Empty<ProveedorSeleccionadoDto>();
}
