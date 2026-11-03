using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Productos que un proveedor ya vendió según el historial de compras (RF-15).
/// La lista nunca es <see langword="null"/>: ante un rechazo o un fallo se
/// devuelve vacía.
/// </summary>
public class RespuestaProductosDeProveedor
{
	public ResultadoConsultaHistoricaCompra Resultado { get; set; }

	public string Mensaje { get; set; } = string.Empty;

	public IReadOnlyList<ProductoDeProveedorDto> Productos { get; set; } = Array.Empty<ProductoDeProveedorDto>();
}
