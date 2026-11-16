namespace CapaNegocio.Compras;

/// <summary>
/// Resultado del proceso de envío o simulación de correo electrónico.
/// </summary>
public class ResultadoEnvioCorreo
{
    public bool Exitoso { get; set; }

    public bool Simulado { get; set; }

    public string Mensaje { get; set; } = string.Empty;

    public string Destinatario { get; set; } = string.Empty;

    public string Asunto { get; set; } = string.Empty;

    public string ContenidoVistaPrevia { get; set; } = string.Empty;
}
