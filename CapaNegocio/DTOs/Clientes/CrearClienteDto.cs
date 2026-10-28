namespace CapaNegocio.DTOs.Clientes;

public class CrearClienteDto
{
	/// <summary>
	/// Texto crudo del campo de la interfaz, sin recortar. El Recorte de bordes
	/// (espacios, tabuladores y saltos de línea) lo aplica CapaNegocio antes de
	/// validar y persistir (RF-3).
	/// </summary>
	public string? Documento { get; set; }

	/// <summary>Texto crudo del campo de la interfaz, sin recortar (RF-3).</summary>
	public string? Nombre { get; set; }

	/// <summary>Texto crudo del campo de la interfaz, sin recortar (RF-3).</summary>
	public string? Apellido { get; set; }

	/// <summary>Correo informado por el usuario; puede venir vacío (dato no informado).</summary>
	public string? Correo { get; set; }

	/// <summary>Teléfono informado por el usuario; puede venir vacío (dato no informado).</summary>
	public string? Telefono { get; set; }
}
