using System;
using System.Collections.Generic;

namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Solicitud de confirmación de costos de una compra en estado Pendiente.
/// </summary>
public class SolicitudConfirmarCompraDto
{
    public int IdCompra { get; set; }

    public IReadOnlyList<LineaConfirmacionCompraDto> Lineas { get; set; } = Array.Empty<LineaConfirmacionCompraDto>();

    public string? Observaciones { get; set; }
}
