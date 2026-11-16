using System;
using System.Collections.Generic;

namespace CapaNegocio.DTOs.Compras;

public class SolicitudConfirmarCompraDto
{
    public int IdCompra { get; set; }

    public IReadOnlyList<LineaConfirmacionCompraDto> Lineas { get; set; } = Array.Empty<LineaConfirmacionCompraDto>();

    public string? Observaciones { get; set; }
}
