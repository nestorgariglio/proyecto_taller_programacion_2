namespace CapaNegocio.DTOs.Compras;

public class SolicitudRegistrarCompraDto
{
	public int? IdProveedor { get; set; }

	public IReadOnlyList<LineaCompraDto> Lineas { get; set; } = Array.Empty<LineaCompraDto>();

	public string EstadoSolicitud { get; set; } = "Recibida";

	public string? Observaciones { get; set; }
}
