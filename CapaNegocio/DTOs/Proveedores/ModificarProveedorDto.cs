namespace CapaNegocio.DTOs.Proveedores;

public class ModificarProveedorDto
{
	/// <summary>Identificador del proveedor existente que se desea editar.</summary>
	public int IdProveedor { get; set; }

	/// <summary>Texto crudo del campo CUIT, sin recortar (RF-3).</summary>
	public string? Cuit { get; set; }

	/// <summary>Texto crudo del campo Razón Social, sin recortar (RF-3).</summary>
	public string? RazonSocial { get; set; }

	/// <summary>Correo informado; vacío o solo espacios significa «no informado».</summary>
	public string? Correo { get; set; }

	/// <summary>Teléfono informado; vacío o solo espacios significa «no informado».</summary>
	public string? Telefono { get; set; }
}
