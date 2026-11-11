using CapaNegocio;
using Xunit;

namespace CapaNegocio.Tests;

public class ComparadorRazonSocialProveedorTests
{
    [Theory]
    [InlineData("Logitech Argentina S.A.", "Logitech Argentina S.A.", true)]
    [InlineData("Logitech Argentina S.A.", "Logitech Argentina SA", true)]
    [InlineData("Logitech Argentina S.A", "Logitech Argentina S. A.", true)]
    [InlineData("LOGITECH ARGENTINA S.A.", "Logitech Argentina S.A.", true)]
    [InlineData("Logitech Argentina S.R.L.", "Logitech Argentina SRL", true)]
    [InlineData("Logitech Argentina S. R. L.", "Logitech Argentina S.R.L", true)]
    [InlineData("Logitech Argentina S.A.", "Logitech Argentina S.R.L.", false)]
    [InlineData("Logitech Argentina", "Logitech Argentina S.A.", false)]
    [InlineData("Pérez & Hnos. (Sur)", "Pérez & Hnos. (Sur)", true)]
    [InlineData("Perez", "Pérez", false)]
    [InlineData("  Logitech   Argentina  ", "Logitech Argentina", true)]
    [InlineData("Casa", "Casa", true)]
    [InlineData("Casa", "Ca S.A.", false)]
    [InlineData("Logitech S.A.S.", "Logitech SA", false)]
    public void SonEquivalentes_SegunTabla(string a, string b, bool esperado)
    {
        Assert.Equal(esperado, ComparadorRazonSocialProveedor.SonEquivalentes(a, b));
    }

    [Fact]
    public void Normalizar_NoAlteraCasaNiAbreviaturaEnMedio()
    {
        Assert.Equal("CASA", ComparadorRazonSocialProveedor.Normalizar("Casa"));
        Assert.Equal("S.A. LOGITECH", ComparadorRazonSocialProveedor.Normalizar("S.A. Logitech"));
        Assert.Equal("LOGITECH S.A.S.", ComparadorRazonSocialProveedor.Normalizar("Logitech S.A.S."));
    }
}
