namespace CapaNegocio.Compras;

/// <summary>
/// Parámetros de configuración para el envío de correos mediante SMTP.
/// </summary>
public class ConfiguracionSmtp
{
    public string Servidor { get; set; } = string.Empty;

    public int Puerto { get; set; } = 587;

    public bool HabilitarSsl { get; set; } = true;

    public string Usuario { get; set; } = string.Empty;

    public string Clave { get; set; } = string.Empty;

    public string Remitente { get; set; } = "compras@sistemaventa.com";

    public string NombreRemitente { get; set; } = "Sistema Venta y Stock - Compras";

    public bool SimularEnvio { get; set; } = true;
}
