using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Clientes;

public class RespuestaCreacionCliente
{
	public ResultadoCreacionCliente Resultado { get; set; }

	public ClienteRespuestaDto? Cliente { get; set; }

	public string Mensaje { get; set; } = string.Empty;
}
