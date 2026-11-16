using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CapaNegocio.DTOs.Compras;

namespace CapaNegocio.Compras;

/// <summary>
/// Implementación de <see cref="IServicioCorreoProveedor"/> mediante SMTP con soporte de simulación local.
/// </summary>
public class ServicioCorreoProveedor : IServicioCorreoProveedor
{
    private readonly ConfiguracionSmtp _config;

    public ServicioCorreoProveedor(ConfiguracionSmtp? config = null)
    {
        _config = config ?? new ConfiguracionSmtp();
    }

    public async Task<ResultadoEnvioCorreo> EnviarSolicitudReabastecimientoAsync(
        string? correoDestino,
        string razonSocialProveedor,
        int idCompra,
        IReadOnlyList<LineaDetalleSolicitudCorreoDto> lineas,
        string? observaciones = null,
        CancellationToken ct = default)
    {
        string asunto = $"[Sistema Venta y Stock] Solicitud de Reabastecimiento N° {idCompra:D6}";
        string cuerpo = GenerarCuerpoSolicitud(razonSocialProveedor, idCompra, lineas, observaciones);

        bool destinatarioValido = !string.IsNullOrWhiteSpace(correoDestino) && correoDestino.Contains('@');

        if (_config.SimularEnvio || string.IsNullOrWhiteSpace(_config.Servidor) || !destinatarioValido)
        {
            string motivo = !destinatarioValido
                ? "proveedor sin correo electrónico configurado"
                : (_config.SimularEnvio ? "modo simulación activo" : "servidor SMTP no configurado");

            return new ResultadoEnvioCorreo
            {
                Exitoso = true,
                Simulado = true,
                Destinatario = correoDestino ?? "(Sin correo)",
                Asunto = asunto,
                ContenidoVistaPrevia = cuerpo,
                Mensaje = $"Solicitud N° {idCompra:D6} generada en modo local ({motivo})."
            };
        }

        try
        {
            using var mensaje = new MailMessage
            {
                From = new MailAddress(_config.Remitente, _config.NombreRemitente),
                Subject = asunto,
                Body = cuerpo,
                IsBodyHtml = false,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            mensaje.To.Add(correoDestino!.Trim());

            using var clienteSmtp = new SmtpClient(_config.Servidor, _config.Puerto)
            {
                EnableSsl = _config.HabilitarSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            if (!string.IsNullOrWhiteSpace(_config.Usuario))
            {
                clienteSmtp.Credentials = new NetworkCredential(_config.Usuario, _config.Clave);
            }

            await clienteSmtp.SendMailAsync(mensaje, ct);

            return new ResultadoEnvioCorreo
            {
                Exitoso = true,
                Simulado = false,
                Destinatario = correoDestino.Trim(),
                Asunto = asunto,
                ContenidoVistaPrevia = cuerpo,
                Mensaje = $"Correo enviado exitosamente a {correoDestino.Trim()}."
            };
        }
        catch (Exception ex)
        {
            return new ResultadoEnvioCorreo
            {
                Exitoso = true,
                Simulado = true,
                Destinatario = correoDestino?.Trim() ?? string.Empty,
                Asunto = asunto,
                ContenidoVistaPrevia = cuerpo,
                Mensaje = $"Fallo en despacho SMTP ({ex.Message}). Se conservó vista previa local de la solicitud."
            };
        }
    }

    public static string GenerarCuerpoSolicitud(
        string razonSocialProveedor,
        int idCompra,
        IReadOnlyList<LineaDetalleSolicitudCorreoDto> lineas,
        string? observaciones)
    {
        var sb = new StringBuilder();
        sb.AppendLine("================================================================================");
        sb.AppendLine("                         ORDEN DE REABASTECIMIENTO");
        sb.AppendLine("================================================================================");
        sb.AppendLine($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}");
        sb.AppendLine($"Solicitud N°: {idCompra:D6}");
        sb.AppendLine($"Proveedor: {razonSocialProveedor}");
        sb.AppendLine("--------------------------------------------------------------------------------");
        sb.AppendLine("Estimado proveedor,\n");
        sb.AppendLine("Por medio de la presente, solicitamos cotización y confirmación de disponibilidad");
        sb.AppendLine("para los siguientes productos:\n");
        sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0,-15} {1,-40} {2,10}", "CÓDIGO", "PRODUCTO", "CANTIDAD"));
        sb.AppendLine(new string('-', 70));

        foreach (var linea in lineas)
        {
            string nombre = linea.NombreProducto.Length > 38
                ? linea.NombreProducto.Substring(0, 35) + "..."
                : linea.NombreProducto;

            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0,-15} {1,-40} {2,10}",
                linea.CodigoProducto,
                nombre,
                linea.Cantidad));
        }

        sb.AppendLine(new string('-', 70));

        if (!string.IsNullOrWhiteSpace(observaciones))
        {
            sb.AppendLine("\nObservaciones / Condiciones especiales:");
            sb.AppendLine(observaciones.Trim());
        }

        sb.AppendLine("\nAgradecemos confirmar a la brevedad costos unitarios y plazos estimados de entrega.");
        sb.AppendLine("Saludos cordiales,");
        sb.AppendLine("Departamento de Compras y Abastecimiento");
        sb.AppendLine("================================================================================");

        return sb.ToString();
    }
}
