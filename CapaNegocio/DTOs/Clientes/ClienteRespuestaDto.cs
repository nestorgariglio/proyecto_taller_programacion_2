namespace CapaNegocio.DTOs.Clientes;

/// <summary>
/// Datos de salida de un cliente del padrón. No expone la entidad de datos ni
/// datos internos (RNF-2).
/// </summary>
public class ClienteRespuestaDto
{
	public int IdCliente { get; set; }

	/// <summary>Documento del cliente; se mapea desde la columna de documento existente (D-1).</summary>
	public string? Documento { get; set; }

	public string? Nombre { get; set; }

	public string? Apellido { get; set; }

	/// <summary>
	/// Nombre completo del cliente: «Nombre + un espacio + Apellido» (D-9).
	/// Es una propiedad calculada de solo lectura: nunca se asigna desde una
	/// proyección ni se persiste.
	/// </summary>
	public string NombreCompleto => Nombre + " " + Apellido;

	public string? Correo { get; set; }

	public string? Telefono { get; set; }

	/// <summary>Estado del cliente: <see langword="true"/> activo, <see langword="false"/> inactivo.</summary>
	public bool Estado { get; set; }

	public DateTime FechaRegistro { get; set; }
}
