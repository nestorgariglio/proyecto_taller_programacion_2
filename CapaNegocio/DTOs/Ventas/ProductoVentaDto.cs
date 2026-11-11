namespace CapaNegocio.DTOs.Ventas;

public sealed record ProductoVentaDto
{
    public int IdProducto { get; init; }
    public string? Codigo { get; init; }
    public string? Nombre { get; init; }
    public int Stock { get; init; }
    public decimal PrecioVenta { get; init; }
}
