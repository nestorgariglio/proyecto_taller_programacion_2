using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Proveedores;

public class RespuestaActivacionProveedor
{
	public ResultadoActivacionProveedor Resultado { get; set; }

	public ProveedorRespuestaDto? Proveedor { get; set; }

	public string Mensaje { get; set; } = string.Empty;
}
