using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Compras;

public class RespuestaBusquedaProveedoresCompra
{
	public ResultadoBusquedaCompras Resultado { get; set; }

	public string Mensaje { get; set; } = string.Empty;

	public IReadOnlyList<ProveedorSeleccionadoDto> Proveedores { get; set; } = Array.Empty<ProveedorSeleccionadoDto>();
}
