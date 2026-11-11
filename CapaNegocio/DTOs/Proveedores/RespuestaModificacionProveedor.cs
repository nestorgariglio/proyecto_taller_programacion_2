using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Proveedores;

public class RespuestaModificacionProveedor
{
	public ResultadoModificacionProveedor Resultado { get; set; }

	public ProveedorRespuestaDto? Proveedor { get; set; }

	/// <summary>Proveedor existente con Razón Social posiblemente duplicada (solo con ese resultado).</summary>
	public CoincidenciaRazonSocialDto? Coincidencia { get; set; }

	public string Mensaje { get; set; } = string.Empty;
}
