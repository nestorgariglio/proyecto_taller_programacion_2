using CapaNegocio.Enums;
namespace CapaNegocio.DTOs.Usuarios;

public class RespuestaAutenticacion
{
	public ResultadoAutenticacion Resultado { get; set; }
	public UsuarioRespuestaDto? Usuario { get; set; }
	public string Mensaje { get; set; } = string.Empty;
}
