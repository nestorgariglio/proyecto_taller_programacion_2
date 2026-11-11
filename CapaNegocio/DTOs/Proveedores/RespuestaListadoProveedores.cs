using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Proveedores;

public class RespuestaListadoProveedores
{
	public ResultadoListadoProveedores Resultado { get; set; }

	public string Mensaje { get; set; } = string.Empty;

	public IReadOnlyList<ProveedorRespuestaDto> Proveedores { get; set; } = Array.Empty<ProveedorRespuestaDto>();
}
