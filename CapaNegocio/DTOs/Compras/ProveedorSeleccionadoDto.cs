namespace CapaNegocio.DTOs.Compras;

public class ProveedorSeleccionadoDto
{
	public int IdProveedor { get; set; }

	public string RazonSocial { get; set; } = string.Empty;

	public string Documento { get; set; } = string.Empty;
}
