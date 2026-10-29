namespace CapaNegocio.DTOs.Clientes;

public class ModificarClienteDto
{
	/// <summary>
	/// Identificador del cliente existente que se desea editar.
	/// </summary>
	public int IdCliente { get; set; }

	/// <summary>
	/// Texto crudo del campo de la interfaz, sin recortar. El Recorte de bordes
	/// lo aplica CapaNegocio antes de validar y persistir (RF-3).
	/// </summary>
	public string? Documento { get; set; }

	/// <summary>Texto crudo del campo de la interfaz, sin recortar (RF-3).</summary>
	public string? Nombre { get; set; }

	/// <summary>Texto crudo del campo de la interfaz, sin recortar (RF-3).</summary>
	public string? Apellido { get; set; }

	/// <summary>Correo informado; vacío o solo espacios significa «no informado» (RF-13).</summary>
	public string? Correo { get; set; }

	/// <summary>Teléfono informado; vacío o solo espacios significa «no informado» (RF-13).</summary>
	public string? Telefono { get; set; }
}
