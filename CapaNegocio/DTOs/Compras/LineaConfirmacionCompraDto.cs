namespace CapaNegocio.DTOs.Compras;

public class LineaConfirmacionCompraDto
{
    public int IdProducto { get; set; }

    public decimal PrecioCompra { get; set; }

    public decimal PrecioVenta { get; set; }
}
