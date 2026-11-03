using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Proveedores;

public class RespuestaModificacionProveedor
{
	public ResultadoModificacionProveedor Resultado { get; set; }

	public ProveedorRespuestaDto? Proveedor { get; set; }

	public string Mensaje { get; set; } = string.Empty;
}
