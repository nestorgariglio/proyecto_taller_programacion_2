namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Representa una línea en la orden o solicitud de reabastecimiento enviada por correo.
/// </summary>
public class LineaDetalleSolicitudCorreoDto
{
    public string CodigoProducto { get; set; } = string.Empty;

    public string NombreProducto { get; set; } = string.Empty;

    public int Cantidad { get; set; }

    public decimal? PrecioEstimado { get; set; }
}
