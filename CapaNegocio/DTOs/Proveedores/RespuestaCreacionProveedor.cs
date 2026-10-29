using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Proveedores;

/// <summary>
/// Respuesta del alta de un proveedor (RF-1 a RF-11). El alta nace siempre activa
/// (Estado = true) y el mensaje siempre informado.
/// </summary>
public class RespuestaCreacionProveedor
{
	public ResultadoCreacionProveedor Resultado { get; set; }

	public ProveedorRespuestaDto? Proveedor { get; set; }

	public string Mensaje { get; set; } = string.Empty;
}
