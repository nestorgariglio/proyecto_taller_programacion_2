using CapaNegocio.Compras;

namespace CapaNegocio.DTOs.Compras;

public class RespuestaAccionCompra
{
    public bool Exitoso { get; set; }

    public string Mensaje { get; set; } = string.Empty;

    public int IdCompra { get; set; }

    public string EstadoSolicitud { get; set; } = string.Empty;

    public ResultadoEnvioCorreo? InfoCorreo { get; set; }
}
