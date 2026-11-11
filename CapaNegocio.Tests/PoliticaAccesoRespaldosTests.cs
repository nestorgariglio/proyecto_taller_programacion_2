using Xunit;

namespace CapaNegocio.Tests;

public class PoliticaAccesoRespaldosTests
{
    [Theory]
    [InlineData("Administrador", true)]
    [InlineData("administrador", true)]
    [InlineData(" Encargado ", true)]
    [InlineData("Vendedor", false)]
    [InlineData("Auditor", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    public void PuedeGestionarRespaldos_SoloAdministradorYEncargado(string? rol, bool esperado)
    {
        Assert.Equal(esperado, PoliticaAcceso.PuedeGestionarRespaldos(rol));
    }
}
