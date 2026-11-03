using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Proveedores que ya vendieron un producto según el historial de compras
/// (RF-12). La lista nunca es <see langword="null"/>: ante un rechazo o un
/// fallo se devuelve vacía.
/// </summary>
public class RespuestaProveedoresDeProducto
{
	public ResultadoConsultaHistoricaCompra Resultado { get; set; }

	public string Mensaje { get; set; } = string.Empty;

	public IReadOnlyList<ProveedorDeProductoDto> Proveedores { get; set; } = Array.Empty<ProveedorDeProductoDto>();
}
