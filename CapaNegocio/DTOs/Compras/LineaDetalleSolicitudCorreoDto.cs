namespace CapaNegocio.DTOs.Compras;

public class LineaDetalleSolicitudCorreoDto
{
    public string CodigoProducto { get; set; } = string.Empty;

    public string NombreProducto { get; set; } = string.Empty;

    public int Cantidad { get; set; }

    public decimal? PrecioEstimado { get; set; }
}
