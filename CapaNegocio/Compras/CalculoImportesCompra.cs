namespace CapaNegocio.Compras;

/// <summary>
/// Reglas de cálculo de subtotales y montos totales para operaciones de compras.
/// </summary>
public static class CalculoImportesCompra
{
    public const int CantidadMinima = 1;
    public const int CantidadMaxima = 1_000_000;
    public const decimal PrecioMinimo = 0.01m;
    public const decimal PrecioMaximo = 999_999_999.99m;
    public const decimal MontoTotalMaximo = 9_999_999_999_999_999.99m;
    public const int DecimalesPrecio = 2;

    /// <summary>
    /// Calcula el subtotal de una línea multiplicando cantidad por precio unitario de compra.
    /// </summary>
    public static decimal CalcularSubtotal(int cantidad, decimal precioCompra)
        => cantidad * precioCompra;

    /// <summary>
    /// Calcula el monto total acumulado sumando los subtotales de las líneas.
    /// </summary>
    public static decimal CalcularMontoTotal(IEnumerable<decimal> subtotales)
    {
        ArgumentNullException.ThrowIfNull(subtotales);

        decimal total = 0m;

        foreach (decimal subtotal in subtotales)
        {
            total += subtotal;
        }

        return total;
    }
}
