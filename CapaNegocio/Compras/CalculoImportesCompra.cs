namespace CapaNegocio.Compras;

/// <summary>
/// Regla única de cálculo de importes de Compras (RF-33, RF-34 y RNF-13):
/// subtotal de la línea y monto total de la compra. Solo aritmética decimal,
/// sin redondeos ni truncamientos intermedios (RNF-1), sin EF Core, sin
/// mensajes y sin textos. También concentra los límites de D-15 y D-20.
/// </summary>
public static class CalculoImportesCompra
{
    /// <summary>Cantidad mínima de una línea (D-15 / RF-20).</summary>
    public const int CantidadMinima = 1;

    /// <summary>Cantidad máxima de una línea o de una cantidad acumulada (D-15 / RF-21 y RF-63).</summary>
    public const int CantidadMaxima = 1_000_000;

    /// <summary>Precio mínimo admitido (D-15 / RF-22 y RF-23).</summary>
    public const decimal PrecioMinimo = 0.01m;

    /// <summary>Precio máximo admitido para un precio de compra o de venta (D-15 / RF-24).</summary>
    public const decimal PrecioMaximo = 999_999_999.99m;

    /// <summary>Tope del monto total de la compra (D-20 / RF-64).</summary>
    public const decimal MontoTotalMaximo = 9_999_999_999_999_999.99m;

    /// <summary>Cantidad máxima de dígitos decimales de un precio (D-12 / RF-26).</summary>
    public const int DecimalesPrecio = 2;

    /// <summary>
    /// Subtotal de una línea: cantidad por precio de compra, en decimal y sin
    /// redondeo intermedio (RF-33). El precio de venta no interviene (RF-35).
    /// </summary>
    public static decimal CalcularSubtotal(int cantidad, decimal precioCompra)
        => cantidad * precioCompra;

    /// <summary>
    /// Monto total de la compra: suma exacta de los subtotales recibidos
    /// (RF-7 y RF-34). Es la única firma del total: la pantalla le pasa los
    /// subtotales de la grilla y el registro los subtotales recalculados.
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
