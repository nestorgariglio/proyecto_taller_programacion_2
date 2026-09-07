using CapaNegocio;
using Xunit;

namespace CapaNegocio.Tests;

public class PoliticaAccesoTests
{
    [Fact]
    public void AdministradorTieneTodosLosModulos()
    {
        foreach (string modulo in new[] { "Usuarios", "Productos", "Categorías", "Ventas", "Compras", "Clientes", "Proveedores", "Reportes" })
            Assert.True(PoliticaAcceso.TieneAcceso("Administrador", modulo));
    }

    [Fact]
    public void EncargadoTieneModulosOperativosAsignados()
    {
        foreach (string modulo in new[] { "Productos", "Categorías", "Proveedores", "Compras", "Reportes" })
            Assert.True(PoliticaAcceso.TieneAcceso("Encargado", modulo));
        foreach (string modulo in new[] { "Usuarios", "Ventas", "Clientes" })
            Assert.False(PoliticaAcceso.TieneAcceso("Encargado", modulo));
    }

    [Fact]
    public void VendedorTieneVentasYClientes()
    {
        foreach (string modulo in new[] { "Ventas", "Clientes" })
            Assert.True(PoliticaAcceso.TieneAcceso("Vendedor", modulo));
        foreach (string modulo in new[] { "Usuarios", "Productos", "Categorías", "Compras", "Proveedores", "Reportes" })
            Assert.False(PoliticaAcceso.TieneAcceso("Vendedor", modulo));
    }

    [Fact]
    public void RolDesconocidoNoTieneAccesos()
    {
        foreach (string? rol in new[] { null, "", "   ", "Otro" })
            Assert.False(PoliticaAcceso.TieneAcceso(rol, "Productos"));
        Assert.False(PoliticaAcceso.TieneAcceso("Administrador", null));
        Assert.False(PoliticaAcceso.TieneAcceso("Administrador", ""));
        Assert.False(PoliticaAcceso.TieneAcceso("Administrador", "   "));
    }

    [Fact]
    public void RolYModuloSeComparanSinDistinguirMayusculasNiEspacios()
    {
        Assert.True(PoliticaAcceso.TieneAcceso("  aDmInIsTrAdOr ", "  rEpOrTeS "));
    }
}
