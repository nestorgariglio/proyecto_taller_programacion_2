namespace CapaNegocio.DTOs.Proveedores;

public class CrearProveedorDto
{
	/// <summary>Texto crudo del campo CUIT, sin recortar (RF-3).</summary>
	public string? Cuit { get; set; }

	/// <summary>Texto crudo del campo Razón Social, sin recortar (RF-3).</summary>
	public string? RazonSocial { get; set; }

	/// <summary>Correo informado por el usuario; puede venir vacío (dato no informado).</summary>
	public string? Correo { get; set; }

	/// <summary>Teléfono informado por el usuario; puede venir vacío (dato no informado).</summary>
	public string? Telefono { get; set; }
}
