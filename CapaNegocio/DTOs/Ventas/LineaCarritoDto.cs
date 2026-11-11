namespace CapaNegocio.DTOs.Ventas;

public sealed record LineaCarritoDto
{
    public int IdProducto { get; init; }
    public string? Codigo { get; init; }
    public string? Nombre { get; init; }
    public int Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
}
