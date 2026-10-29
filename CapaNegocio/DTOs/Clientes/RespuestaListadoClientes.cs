using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Clientes;

public class RespuestaListadoClientes
{
	public ResultadoListadoClientes Resultado { get; set; }

	/// <summary>
	/// Mensaje redactado por la lógica de negocio; siempre informado, también en el
	/// caso de «sin resultados» (RF-19).
	/// </summary>
	public string Mensaje { get; set; } = string.Empty;

	public IReadOnlyList<ClienteRespuestaDto> Clientes { get; set; } = Array.Empty<ClienteRespuestaDto>();
}
