using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Clientes;

public class RespuestaActivacionCliente
{
	public ResultadoActivacionCliente Resultado { get; set; }

	public ClienteRespuestaDto? Cliente { get; set; }

	public string Mensaje { get; set; } = string.Empty;
}
