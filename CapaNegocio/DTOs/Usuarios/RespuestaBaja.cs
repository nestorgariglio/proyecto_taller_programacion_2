using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Usuarios;

public class RespuestaBaja
{
	public ResultadoBaja Resultado { get; set; }
	public UsuarioRespuestaDto? Usuario { get; set; }
	public string Mensaje { get; set; } = string.Empty;
}
