namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Precios confirmados por el proveedor para una línea de compra.
/// </summary>
public class LineaConfirmacionCompraDto
{
    public int IdProducto { get; set; }

    public decimal PrecioCompra { get; set; }

    public decimal PrecioVenta { get; set; }
}
