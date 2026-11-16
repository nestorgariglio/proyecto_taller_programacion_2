using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CapaNegocio.DTOs.Compras;

namespace CapaNegocio.Compras;

/// <summary>
/// Contrato para el envío de notificaciones y solicitudes de reabastecimiento por correo.
/// </summary>
public interface IServicioCorreoProveedor
{
    Task<ResultadoEnvioCorreo> EnviarSolicitudReabastecimientoAsync(
        string? correoDestino,
        string razonSocialProveedor,
        int idCompra,
        IReadOnlyList<LineaDetalleSolicitudCorreoDto> lineas,
        string? observaciones = null,
        CancellationToken ct = default);
}
