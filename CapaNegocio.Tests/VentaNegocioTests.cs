using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CapaDatos;
using CapaNegocio;
using CapaNegocio.DTOs.Ventas;
using Xunit;

namespace CapaNegocio.Tests;

public class VentaNegocioTests
{
    private sealed class RepositorioVentaFalso : IVentaRepositorio
    {
        public ParametrosRegistroVentaDatos? UltimosParametros { get; private set; }
        public ResultadoVentaDatos ResultadoADevolver { get; set; } = new(true, 101, 0, "Éxito");
        public bool DebeLanzarExcepcion { get; set; }

        public Task<ResultadoVentaDatos> RegistrarVentaAsync(
            ParametrosRegistroVentaDatos parametros,
            CancellationToken ct = default)
        {
            if (DebeLanzarExcepcion)
                throw new InvalidOperationException("Fallo simulado de conexión.");

            UltimosParametros = parametros;
            return Task.FromResult(ResultadoADevolver);
        }
    }

    [Fact]
    public async Task RegistrarVentaAsync_SolicitudNula_DevuelveError()
    {
        var repo = new RepositorioVentaFalso();
        var negocio = new VentaNegocio(repo);

        var respuesta = await negocio.RegistrarVentaAsync(null!);

        Assert.False(respuesta.Exito);
        Assert.Equal("La solicitud de venta es requerida.", respuesta.Mensaje);
    }

    [Fact]
    public async Task RegistrarVentaAsync_SinLineas_DevuelveErrorCarritoVacio()
    {
        var repo = new RepositorioVentaFalso();
        var negocio = new VentaNegocio(repo);

        var solicitud = new SolicitudRegistrarVentaDto
        {
            MontoPago = 1000m,
            Lineas = Array.Empty<LineaCarritoDto>()
        };

        var respuesta = await negocio.RegistrarVentaAsync(solicitud);

        Assert.False(respuesta.Exito);
        Assert.Equal("El carrito está vacío. Agregue productos antes de cobrar.", respuesta.Mensaje);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(-1, 100)]
    [InlineData(1, 0)]
    [InlineData(1, -50)]
    public async Task RegistrarVentaAsync_LineasConCantidadOPrecioInvalido_DevuelveError(int cantidad, decimal precio)
    {
        var repo = new RepositorioVentaFalso();
        var negocio = new VentaNegocio(repo);

        var solicitud = new SolicitudRegistrarVentaDto
        {
            MontoPago = 500m,
            Lineas = new[]
            {
                new LineaCarritoDto
                {
                    IdProducto = 1,
                    Nombre = "Yerba",
                    Cantidad = cantidad,
                    PrecioUnitario = precio
                }
            }
        };

        var respuesta = await negocio.RegistrarVentaAsync(solicitud);

        Assert.False(respuesta.Exito);
        Assert.Equal("Todas las líneas deben tener una cantidad y un precio mayores a cero.", respuesta.Mensaje);
    }

    [Fact]
    public async Task RegistrarVentaAsync_PagoInsuficiente_DevuelveErrorYMontos()
    {
        var repo = new RepositorioVentaFalso();
        var negocio = new VentaNegocio(repo);

        var solicitud = new SolicitudRegistrarVentaDto
        {
            MontoPago = 150m,
            Lineas = new[]
            {
                new LineaCarritoDto
                {
                    IdProducto = 1,
                    Nombre = "Azúcar",
                    Cantidad = 2,
                    PrecioUnitario = 100m
                }
            }
        };

        var respuesta = await negocio.RegistrarVentaAsync(solicitud);

        Assert.False(respuesta.Exito);
        Assert.Equal(200m, respuesta.MontoTotal);
        Assert.Equal(150m, respuesta.MontoPago);
        Assert.Equal("El monto ingresado es insuficiente para cubrir el total de la venta.", respuesta.Mensaje);
    }

    [Fact]
    public async Task RegistrarVentaAsync_ConsumidorFinal_AsignaValoresPorDefectoYRegistraExitosamente()
    {
        var repo = new RepositorioVentaFalso { ResultadoADevolver = new(true, 50, 0, "OK") };
        var negocio = new VentaNegocio(repo);

        var solicitud = new SolicitudRegistrarVentaDto
        {
            IdCliente = null,
            MontoPago = 1000m,
            Lineas = new[]
            {
                new LineaCarritoDto
                {
                    IdProducto = 10,
                    Nombre = "Leche",
                    Cantidad = 3,
                    PrecioUnitario = 250m
                }
            }
        };

        var respuesta = await negocio.RegistrarVentaAsync(solicitud);

        Assert.True(respuesta.Exito);
        Assert.Equal(50, respuesta.IdVenta);
        Assert.Equal(750m, respuesta.MontoTotal);
        Assert.Equal(1000m, respuesta.MontoPago);
        Assert.Equal(250m, respuesta.MontoCambio);
        Assert.NotNull(repo.UltimosParametros);
        Assert.Null(repo.UltimosParametros!.IdCliente);
        Assert.Equal("0", repo.UltimosParametros.DocumentoCliente);
        Assert.Equal("Consumidor Final", repo.UltimosParametros.NombreCliente);
        Assert.Single(repo.UltimosParametros.Detalles);
        Assert.Equal(10, repo.UltimosParametros.Detalles[0].IdProducto);
        Assert.Equal(3, repo.UltimosParametros.Detalles[0].Cantidad);
        Assert.Equal(250m, repo.UltimosParametros.Detalles[0].PrecioVenta);
        Assert.Equal(750m, repo.UltimosParametros.Detalles[0].Subtotal);
    }

    [Fact]
    public async Task RegistrarVentaAsync_ClienteRegistrado_PasaDatosYCalculaVueltoExacto()
    {
        var repo = new RepositorioVentaFalso { ResultadoADevolver = new(true, 88, 0, "OK") };
        var negocio = new VentaNegocio(repo);

        var solicitud = new SolicitudRegistrarVentaDto
        {
            IdUsuario = 12,
            IdCliente = 5,
            DocumentoCliente = "30123456",
            NombreCliente = "Ana Pérez",
            MontoPago = 500.50m,
            Lineas = new[]
            {
                new LineaCarritoDto
                {
                    IdProducto = 1,
                    Nombre = "Café",
                    Cantidad = 1,
                    PrecioUnitario = 350.25m
                }
            }
        };

        var respuesta = await negocio.RegistrarVentaAsync(solicitud);

        Assert.True(respuesta.Exito);
        Assert.Equal(88, respuesta.IdVenta);
        Assert.Equal(350.25m, respuesta.MontoTotal);
        Assert.Equal(500.50m, respuesta.MontoPago);
        Assert.Equal(150.25m, respuesta.MontoCambio);
        Assert.NotNull(repo.UltimosParametros);
        Assert.Equal(12, repo.UltimosParametros!.IdUsuario);
        Assert.Equal(5, repo.UltimosParametros.IdCliente);
        Assert.Equal("30123456", repo.UltimosParametros.DocumentoCliente);
        Assert.Equal("Ana Pérez", repo.UltimosParametros.NombreCliente);
    }

    [Fact]
    public async Task RegistrarVentaAsync_FalloDeStock_PropagaMensajeDeNegocio()
    {
        var repo = new RepositorioVentaFalso
        {
            ResultadoADevolver = new(false, 0, 50005, "Stock insuficiente para el producto Café. Disponible: 2, Solicitado: 5.")
        };
        var negocio = new VentaNegocio(repo);

        var solicitud = new SolicitudRegistrarVentaDto
        {
            MontoPago = 1000m,
            Lineas = new[]
            {
                new LineaCarritoDto
                {
                    IdProducto = 1,
                    Nombre = "Café",
                    Cantidad = 5,
                    PrecioUnitario = 100m
                }
            }
        };

        var respuesta = await negocio.RegistrarVentaAsync(solicitud);

        Assert.False(respuesta.Exito);
        Assert.Contains("Stock insuficiente", respuesta.Mensaje);
    }

    [Fact]
    public async Task RegistrarVentaAsync_ExcepcionInesperada_CapturaYDevuelveMensajeAmigable()
    {
        var repo = new RepositorioVentaFalso { DebeLanzarExcepcion = true };
        var negocio = new VentaNegocio(repo);

        var solicitud = new SolicitudRegistrarVentaDto
        {
            MontoPago = 1000m,
            Lineas = new[]
            {
                new LineaCarritoDto
                {
                    IdProducto = 1,
                    Nombre = "Café",
                    Cantidad = 1,
                    PrecioUnitario = 100m
                }
            }
        };

        var respuesta = await negocio.RegistrarVentaAsync(solicitud);

        Assert.False(respuesta.Exito);
        Assert.Equal("Ocurrió un error inesperado al procesar la venta. Intente nuevamente.", respuesta.Mensaje);
    }
}
