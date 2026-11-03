using CapaNegocio.Stock;
using Xunit;

namespace CapaNegocio.Tests;

/// <summary>
/// Pruebas de la resolución del umbral mínimo global de stock (spec 004, T1):
/// RF-9, RF-10 y RF-20, según D-9, D-10 y D-15. Es lógica pura sobre el valor
/// crudo del parámetro: no intervienen EF Core ni el proveedor de configuración.
/// </summary>
public class ResolucionUmbralStockTests
{
    // ---------- RF-9 / RF-20: valores definidos válidos y parámetro ausente ----------

    [Theory]
    [InlineData(null, 5)]   // D-10: parámetro no definido ⇒ se asume 5, sin degradar
    [InlineData(" 5 ", 5)]  // RF-20: se aplica Recorte antes de interpretar el valor
    [InlineData("0", 0)]    // D-9: 0 es un umbral válido
    [InlineData("1000", 1000)]
    public void Resolver_ValorValido_DevuelveElUmbralSinDegradar(
        string? valorCrudo, int umbralEsperado)
    {
        ResolucionUmbralStock resolucion = ResolucionUmbralStock.Resolver(valorCrudo);

        Assert.False(resolucion.EsDegradado);
        Assert.Equal(umbralEsperado, resolucion.Valor);
    }

    // ---------- RF-10 / RF-20 / D-9: valores definidos inválidos ⇒ modo degradado ----------

    [Theory]
    [InlineData("")]           // vacío
    [InlineData("   ")]        // solo espacios: queda vacío tras el Recorte
    [InlineData("5.0")]        // decimal con punto (D-9)
    [InlineData("5,0")]        // decimal con coma (D-9)
    [InlineData("+5")]         // signo positivo
    [InlineData("-5")]         // signo negativo (D-9)
    [InlineData("abc")]        // no numérico
    [InlineData("2147483648")] // fuera del rango de int
    public void Resolver_ValorInvalido_DegradaSinValor(string valorCrudo)
    {
        ResolucionUmbralStock resolucion = ResolucionUmbralStock.Resolver(valorCrudo);

        Assert.True(resolucion.EsDegradado);
        Assert.Null(resolucion.Valor);
    }
}
