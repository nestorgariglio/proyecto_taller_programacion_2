namespace CapaNegocio.DTOs.Usuarios;

public class UsuarioRespuestaDto
{
	public int IdUsuario { get; set; }
	public int Dni { get; set; }
	public string Nombre { get; set; } = string.Empty;
	public string Apellido { get; set; } = string.Empty;
	public string Sexo { get; set; } = string.Empty;
	public string? Correo { get; set; }
	public int IdRol { get; set; }
	public string? RolDescripcion { get; set; }
	public bool Estado { get; set; }
}
