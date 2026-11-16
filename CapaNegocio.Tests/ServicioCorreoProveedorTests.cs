using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CapaNegocio.Compras;
using CapaNegocio.DTOs.Compras;
using Xunit;

namespace CapaNegocio.Tests;

public class ServicioCorreoProveedorTests
{
    [Fact]
    public async Task EnviarSolicitudReabastecimientoAsync_ModoSimulado_GeneraVistaPreviaYReportaExito()
    {
        // Arrange
        var config = new ConfiguracionSmtp
        {
            SimularEnvio = true
        };
        var servicio = new ServicioCorreoProveedor(config);

        var lineas = new List<LineaDetalleSolicitudCorreoDto>
        {
            new() { CodigoProducto = "PROD-001", NombreProducto = "Teclado Mecánico RGB", Cantidad = 10 },
            new() { CodigoProducto = "PROD-002", NombreProducto = "Mouse Inalámbrico Ergonómico 16000 DPI", Cantidad = 25 }
        };

        // Act
        var resultado = await servicio.EnviarSolicitudReabastecimientoAsync(
            "proveedor@distribuidora.com",
            "Distribuidora Mayorista SA",
            123,
            lineas,
            "Entrega urgente en sucursal central.");

        // Assert
        Assert.True(resultado.Exitoso);
        Assert.True(resultado.Simulado);
        Assert.Equal("proveedor@distribuidora.com", resultado.Destinatario);
        Assert.Contains("123", resultado.Asunto);
        Assert.Contains("Distribuidora Mayorista SA", resultado.ContenidoVistaPrevia);
        Assert.Contains("PROD-001", resultado.ContenidoVistaPrevia);
        Assert.Contains("Teclado Mecánico RGB", resultado.ContenidoVistaPrevia);
        Assert.Contains("Entrega urgente en sucursal central.", resultado.ContenidoVistaPrevia);
        Assert.Contains("modo simulación activo", resultado.Mensaje);
    }

    [Fact]
    public async Task EnviarSolicitudReabastecimientoAsync_ProveedorSinCorreo_EjecutaEnModoSimulado()
    {
        // Arrange
        var config = new ConfiguracionSmtp { SimularEnvio = false, Servidor = "smtp.test.com" };
        var servicio = new ServicioCorreoProveedor(config);
        var lineas = new List<LineaDetalleSolicitudCorreoDto>
        {
            new() { CodigoProducto = "PROD-010", NombreProducto = "Monitor 24 IPS", Cantidad = 5 }
        };

        // Act
        var resultado = await servicio.EnviarSolicitudReabastecimientoAsync(
            null,
            "Proveedor Sin Email SRL",
            456,
            lineas);

        // Assert
        Assert.True(resultado.Exitoso);
        Assert.True(resultado.Simulado);
        Assert.Equal("(Sin correo)", resultado.Destinatario);
        Assert.Contains("proveedor sin correo", resultado.Mensaje);
    }

    [Fact]
    public void GenerarCuerpoSolicitud_FormateaCorrectamenteContenidoYObservaciones()
    {
        // Arrange
        var lineas = new List<LineaDetalleSolicitudCorreoDto>
        {
            new() { CodigoProducto = "ABC-123", NombreProducto = "Un producto con un nombre extraordinariamente largo que excede los límites", Cantidad = 100 }
        };

        // Act
        string cuerpo = ServicioCorreoProveedor.GenerarCuerpoSolicitud(
            "TecnoGlobal SA", 789, lineas, "Favor de cotizar con flete incluido.");

        // Assert
        Assert.Contains("TecnoGlobal SA", cuerpo);
        Assert.Contains("000789", cuerpo);
        Assert.Contains("ABC-123", cuerpo);
        Assert.Contains("Favor de cotizar con flete incluido.", cuerpo);
        Assert.Contains("...", cuerpo); // Truncamiento en tabla
    }
}
