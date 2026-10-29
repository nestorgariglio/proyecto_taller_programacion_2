using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Clientes;

public class RespuestaBajaCliente
{
	public ResultadoBajaCliente Resultado { get; set; }

	public ClienteRespuestaDto? Cliente { get; set; }

	public string Mensaje { get; set; } = string.Empty;
}
