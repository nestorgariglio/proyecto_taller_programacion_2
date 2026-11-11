using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Clientes;

public class RespuestaModificacionCliente
{
	public ResultadoModificacionCliente Resultado { get; set; }

	public ClienteRespuestaDto? Cliente { get; set; }

	public string Mensaje { get; set; } = string.Empty;
}
