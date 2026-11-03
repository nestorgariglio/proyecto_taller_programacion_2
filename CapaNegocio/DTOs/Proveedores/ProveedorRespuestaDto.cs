namespace CapaNegocio.DTOs.Proveedores;

/// <summary>
/// Datos de salida de un proveedor del padrón. No expone la entidad de datos ni
/// datos internos (RNF-2).
/// </summary>
public class ProveedorRespuestaDto
{
	public int IdProveedor { get; set; }

	/// <summary>Documento del proveedor (CUIT); se mapea desde la columna existente (D-1).</summary>
	public string Cuit { get; set; } = string.Empty;

	public string RazonSocial { get; set; } = string.Empty;

	public string? Correo { get; set; }

	public string? Telefono { get; set; }

	/// <summary>Estado del proveedor: <see langword="true"/> activo, <see langword="false"/> inactivo.</summary>
	public bool Estado { get; set; }

	public DateTime FechaRegistro { get; set; }
}
