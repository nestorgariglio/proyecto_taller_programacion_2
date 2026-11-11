namespace CapaNegocio.DTOs.Ventas;

public sealed class CarritoVentaDto
{
    public IReadOnlyList<LineaCarritoDto> Lineas { get; }
    public decimal Total { get; }

    public CarritoVentaDto() : this(Array.Empty<LineaCarritoDto>()) { }

    internal CarritoVentaDto(IEnumerable<LineaCarritoDto> lineas)
    {
        Lineas = Array.AsReadOnly(lineas.ToArray());
        Total = Lineas.Sum(linea => linea.Subtotal);
    }
}
