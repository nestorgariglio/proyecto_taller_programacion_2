namespace CapaNegocio.DTOs.Compras;

public class CompraConDetalleDto
{
	public int IdCompra { get; set; }

	public DateTime FechaRegistro { get; set; }

	public string RazonSocialProveedor { get; set; } = string.Empty;

	public string NombreUsuario { get; set; } = string.Empty;

	public decimal MontoTotal { get; set; }

	public bool Estado { get; set; }

	public string EstadoSolicitud { get; set; } = "Recibida";

	public DateTime? FechaConfirmacion { get; set; }

	public DateTime? FechaRecepcion { get; set; }

	public DateTime? FechaAnulacion { get; set; }

	public string? MotivoAnulacion { get; set; }

	public string? Observaciones { get; set; }

	public IReadOnlyList<LineaCompraDetalleDto> Lineas { get; set; } = Array.Empty<LineaCompraDetalleDto>();
}
