namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Proveedor elegido en la pantalla de Compras. Proyección mínima y propia del
/// módulo: sin correo, sin teléfono, sin fecha de registro y sin la entidad de
/// datos (RNF-5).
/// </summary>
public class ProveedorSeleccionadoDto
{
	public int IdProveedor { get; set; }

	public string RazonSocial { get; set; } = string.Empty;

	/// <summary>Documento del proveedor (CUIT).</summary>
	public string Documento { get; set; } = string.Empty;
}
