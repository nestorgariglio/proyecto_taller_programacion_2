using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Clientes;

/// <summary>
/// Resultado de la confirmación única de baja lógica (RF-21, RF-22, RF-26). El
/// mensaje ya viene redactado por la lógica de negocio, incluida la cantidad de
/// ventas asociadas cuando corresponde.
/// </summary>
public class RespuestaConfirmacionBajaCliente
{
	public ResultadoConfirmacionBajaCliente Resultado { get; set; }

	public string Mensaje { get; set; } = string.Empty;

	/// <summary>
	/// Cantidad de ventas históricas asociadas al cliente (vigentes y anuladas, D-11).
	/// </summary>
	public int VentasAsociadas { get; set; }
}
