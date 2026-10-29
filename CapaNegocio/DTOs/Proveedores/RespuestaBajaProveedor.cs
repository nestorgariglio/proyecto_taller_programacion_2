using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Proveedores;

public class RespuestaBajaProveedor
{
	public ResultadoBajaProveedor Resultado { get; set; }

	public ProveedorRespuestaDto? Proveedor { get; set; }

	public string Mensaje { get; set; } = string.Empty;
}
