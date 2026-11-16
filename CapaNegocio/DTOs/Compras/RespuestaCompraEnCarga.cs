namespace CapaNegocio.DTOs.Compras;

public class RespuestaCompraEnCarga
{
	public bool Exito { get; set; }

	public string Mensaje { get; set; } = string.Empty;

	public IReadOnlyList<LineaCompraDto> Lineas { get; set; } = Array.Empty<LineaCompraDto>();

	public decimal MontoTotal { get; set; }
}
