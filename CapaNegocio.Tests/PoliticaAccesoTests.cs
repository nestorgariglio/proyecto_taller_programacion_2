using CapaNegocio;
using Xunit;

namespace CapaNegocio.Tests;

public class PoliticaAccesoTests
{
    [Fact]
    public void TieneAcceso_Administrador_DevuelveTrueParaTodosLosModulos()
    {
        foreach (string modulo in new[] { "Usuarios", "Productos", "Categorías", "Ventas", "Compras", "Clientes", "Proveedores", "Reportes" })
            Assert.True(PoliticaAcceso.TieneAcceso("Administrador", modulo));
    }

    [Fact]
    public void TieneAcceso_Encargado_DevuelveSoloModulosOperativos()
    {
        foreach (string modulo in new[] { "Productos", "Categorías", "Proveedores", "Compras", "Reportes", "Clientes" })
            Assert.True(PoliticaAcceso.TieneAcceso("Encargado", modulo));
        foreach (string modulo in new[] { "Usuarios", "Ventas" })
            Assert.False(PoliticaAcceso.TieneAcceso("Encargado", modulo));
    }

    [Fact]
    public void TieneAcceso_Vendedor_DevuelveSoloVentasYClientes()
    {
        foreach (string modulo in new[] { "Ventas", "Clientes" })
            Assert.True(PoliticaAcceso.TieneAcceso("Vendedor", modulo));
        foreach (string modulo in new[] { "Usuarios", "Productos", "Categorías", "Compras", "Proveedores", "Reportes" })
            Assert.False(PoliticaAcceso.TieneAcceso("Vendedor", modulo));
    }

    [Fact]
    public void TieneAcceso_RolDesconocido_DevuelveFalse()
    {
        foreach (string? rol in new[] { null, "", "   ", "Otro" })
            Assert.False(PoliticaAcceso.TieneAcceso(rol, "Productos"));
        Assert.False(PoliticaAcceso.TieneAcceso("Administrador", null));
        Assert.False(PoliticaAcceso.TieneAcceso("Administrador", ""));
        Assert.False(PoliticaAcceso.TieneAcceso("Administrador", "   "));
    }

    [Fact]
    public void TieneAcceso_RolYModuloConEspaciosOMayusculas_DevuelveTrue()
    {
        Assert.True(PoliticaAcceso.TieneAcceso("  aDmInIsTrAdOr ", "  rEpOrTeS "));
    }

    [Fact]
    public void TieneAcceso_EncargadoConEspaciosYMinusculas_DevuelveTrueParaClientes()
    {
        Assert.True(PoliticaAcceso.TieneAcceso("  eNcArGaDo ", "clientes"));
    }

    [Theory]
    [InlineData("Administrador")]
    [InlineData("administrador")]
    [InlineData("  aDmInIsTrAdOr  ")]
    [InlineData("Encargado")]
    [InlineData("encargado")]
    [InlineData("  ENCARGADO ")]
    public void PuedeAdministrarClientes_AdministradorOEncargado_DevuelveTrue(string rol)
    {
        Assert.True(PoliticaAcceso.PuedeAdministrarClientes(rol));
    }

    [Theory]
    [InlineData("Vendedor")]
    [InlineData("vendedor")]
    [InlineData("  vEnDeDoR ")]
    [InlineData("Otro")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void PuedeAdministrarClientes_VendedorRolNuloOVacio_DevuelveFalse(string? rol)
    {
        Assert.False(PoliticaAcceso.PuedeAdministrarClientes(rol));
    }

    [Theory]
    [InlineData("Administrador")]
    [InlineData("administrador")]
    [InlineData("  aDmInIsTrAdOr  ")]
    [InlineData("Encargado")]
    [InlineData("encargado")]
    [InlineData("  ENCARGADO ")]
    public void PuedeAdministrarProveedores_AdministradorOEncargado_DevuelveTrue(string rol)
    {
        Assert.True(PoliticaAcceso.PuedeAdministrarProveedores(rol));
    }

    [Theory]
    [InlineData("Vendedor")]
    [InlineData("vendedor")]
    [InlineData("  vEnDeDoR ")]
    [InlineData("Otro")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void PuedeAdministrarProveedores_VendedorONulo_DevuelveFalse(string? rol)
    {
        Assert.False(PoliticaAcceso.PuedeAdministrarProveedores(rol));
    }
}
