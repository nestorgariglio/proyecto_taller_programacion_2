using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Proveedores;

/// <summary>
/// Resultado de la confirmación única de baja lógica (RF-20, RF-21, RF-26). El
/// mensaje ya viene redactado por la lógica de negocio, incluida la cantidad de
/// compras asociadas.
/// </summary>
public class RespuestaConfirmacionBajaProveedor
{
	public ResultadoConfirmacionBajaProveedor Resultado { get; set; }

	public int ComprasAsociadas { get; set; }

	public string Mensaje { get; set; } = string.Empty;
}
