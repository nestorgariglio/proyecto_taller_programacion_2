namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Solicitud de anulación de una compra o solicitud de reabastecimiento (HU-14).
/// </summary>
public class SolicitudAnularCompraDto
{
    public int IdCompra { get; set; }

    public string Motivo { get; set; } = string.Empty;
}
