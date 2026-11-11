namespace CapaNegocio.DTOs.Proveedores;

/// <summary>Proveedor existente cuya Razón Social es posiblemente equivalente a la ingresada.</summary>
public class CoincidenciaRazonSocialDto
{
	public int IdProveedor { get; set; }

	public string RazonSocial { get; set; } = string.Empty;

	public string Cuit { get; set; } = string.Empty;

	public bool Estado { get; set; }
}
