namespace CapaNegocio.DTOs.Usuarios;

public class CrearUsuarioDto
{
	public int Dni { get; set; }
	public string Nombre { get; set; } = string.Empty;
	public string Apellido { get; set; } = string.Empty;
	public string Sexo { get; set; } = string.Empty;
	public string? Correo { get; set; }
	public string Clave { get; set; } = string.Empty;
	public int IdRol { get; set; }
}
