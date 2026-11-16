using CapaNegocio.Compras;

namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Respuesta general para operaciones de cambio de estado sobre compras (confirmación, recepción, anulación).
/// </summary>
public class RespuestaAccionCompra
{
    public bool Exitoso { get; set; }

    public string Mensaje { get; set; } = string.Empty;

    public int IdCompra { get; set; }

    public string EstadoSolicitud { get; set; } = string.Empty;

    public ResultadoEnvioCorreo? InfoCorreo { get; set; }
}
