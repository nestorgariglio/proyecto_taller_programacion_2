namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Línea de la compra en carga. Vive en la pantalla y viaja en cada solicitud.
/// El <see cref="Subtotal"/> lo calcula el negocio; la pantalla nunca lo
/// recalcula (RNF-13). <see cref="PrecioVenta"/> solo existe mientras la compra
/// se está cargando: no se persiste (D-2).
/// </summary>
public class LineaCompraDto
{
	public int IdProducto { get; set; }

	public string CodigoProducto { get; set; } = string.Empty;

	public string NombreProducto { get; set; } = string.Empty;

	public int Cantidad { get; set; }

	public decimal PrecioCompra { get; set; }

	public decimal PrecioVenta { get; set; }

	public decimal Subtotal { get; set; }
}
