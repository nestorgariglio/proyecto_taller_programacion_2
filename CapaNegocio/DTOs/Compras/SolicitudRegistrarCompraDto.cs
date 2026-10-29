namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Solicitud de confirmación de la compra (RF-44). El usuario y el rol no
/// viajan aquí: son parámetros del método de negocio.
/// </summary>
public class SolicitudRegistrarCompraDto
{
	public int? IdProveedor { get; set; }

	public IReadOnlyList<LineaCompraDto> Lineas { get; set; } = Array.Empty<LineaCompraDto>();
}
