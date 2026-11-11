using CapaNegocio.DTOs.Ventas;
using CapaPresentacion.Ventas;
using Xunit;

namespace CapaNegocio.Tests;

public class BusquedaProductosVentaTests
{
    [Fact]
    public async Task Buscar_RespuestaVigente_ConservaTerminoYResultados()
    {
        IReadOnlyList<ProductoVentaDto> productos = [new() { IdProducto = 1, Codigo = "00123" }];
        string? recibido = null;
        var busqueda = new BusquedaProductosVenta(texto =>
        {
            recibido = texto;
            return Task.FromResult(productos);
        });

        Assert.Same(productos, await busqueda.BuscarAsync("00123"));
        Assert.Equal("00123", recibido);
    }

    [Fact]
    public async Task Buscar_SinCoincidencias_DevuelveListaVaciaNoRespuestaObsoleta()
    {
        var busqueda = new BusquedaProductosVenta(_ => Task.FromResult<IReadOnlyList<ProductoVentaDto>>([]));

        var resultado = await busqueda.BuscarAsync("inexistente");

        Assert.NotNull(resultado);
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task Buscar_RespuestaAnteriorLlegaDespues_SeDescarta()
    {
        var primera = Pendiente();
        var segunda = Pendiente();
        var busqueda = new BusquedaProductosVenta(t => t == "le" ? primera.Task : segunda.Task);
        var anterior = busqueda.BuscarAsync("le");
        var actual = busqueda.BuscarAsync("leche");
        IReadOnlyList<ProductoVentaDto> productos = [new() { IdProducto = 2 }];

        segunda.SetResult(productos);
        Assert.Same(productos, await actual);
        primera.SetResult([new() { IdProducto = 1 }]);

        Assert.Null(await anterior);
    }

    [Fact]
    public async Task Buscar_ErrorAnteriorNoReemplazaResultadoActual()
    {
        var primera = Pendiente();
        var busqueda = new BusquedaProductosVenta(t => t == "vieja" ? primera.Task :
            Task.FromResult<IReadOnlyList<ProductoVentaDto>>([new() { IdProducto = 2 }]));
        var anterior = busqueda.BuscarAsync("vieja");
        var actual = await busqueda.BuscarAsync("actual");

        primera.SetException(new InvalidOperationException("Fallo anterior"));

        Assert.Null(await anterior);
        Assert.Equal(2, Assert.Single(actual!).IdProducto);
    }

    [Fact]
    public async Task Buscar_ErrorVigente_SePropagaParaInformarloEnLaVista()
    {
        var error = new InvalidOperationException("Fallo vigente");
        var busqueda = new BusquedaProductosVenta(_ => Task.FromException<IReadOnlyList<ProductoVentaDto>>(error));

        Assert.Same(error, await Assert.ThrowsAsync<InvalidOperationException>(() => busqueda.BuscarAsync("leche")));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Invalidar_AntesDeLaProximaConsulta_DescartaRespuestaOErrorPendiente(bool falla)
    {
        var pendiente = Pendiente();
        var busqueda = new BusquedaProductosVenta(_ => pendiente.Task);
        var anterior = busqueda.BuscarAsync("le");

        busqueda.Invalidar();
        if (falla)
            pendiente.SetException(new InvalidOperationException("Fallo obsoleto"));
        else
            pendiente.SetResult([new() { IdProducto = 1 }]);

        Assert.Null(await anterior);
    }

    [Fact]
    public async Task Invalidar_PermiteNuevaConsultaVigente()
    {
        var busqueda = new BusquedaProductosVenta(_ => Task.FromResult<IReadOnlyList<ProductoVentaDto>>([]));
        busqueda.Invalidar();

        Assert.NotNull(await busqueda.BuscarAsync("nueva"));
    }

    [Fact]
    public async Task VistasDistintas_NoCompartenLaVigenciaDeSusConsultas()
    {
        var pendiente = Pendiente();
        var primeraVista = new BusquedaProductosVenta(_ => pendiente.Task);
        var segundaVista = new BusquedaProductosVenta(_ => Task.FromResult<IReadOnlyList<ProductoVentaDto>>([]));
        var primera = primeraVista.BuscarAsync("primera");

        segundaVista.Invalidar();
        await segundaVista.BuscarAsync("segunda");
        pendiente.SetResult([]);

        Assert.NotNull(await primera);
    }

    [Fact]
    public async Task Invalidar_RespuestaYaEntregada_PermiteRevalidarAntesDePintarla()
    {
        var busqueda = new BusquedaProductosVenta(_ => Task.FromResult<IReadOnlyList<ProductoVentaDto>>([]));
        var consulta = busqueda.BuscarAsync("leche");
        int version = busqueda.Version;
        Assert.NotNull(await consulta);

        busqueda.Invalidar();

        Assert.NotEqual(version, busqueda.Version);
    }

    [Fact]
    public async Task Buscar_ErrorVigente_NoPermiteEntregarExitoAnteriorTardio()
    {
        var primera = Pendiente();
        var segunda = Pendiente();
        var busqueda = new BusquedaProductosVenta(t => t == "vieja" ? primera.Task : segunda.Task);
        var anterior = busqueda.BuscarAsync("vieja");
        var actual = busqueda.BuscarAsync("nueva");

        segunda.SetException(new InvalidOperationException("Fallo vigente"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => actual);
        primera.SetResult([new() { IdProducto = 1 }]);

        Assert.Null(await anterior);
    }

    [Fact]
    public async Task Buscar_ConsultorFallaSincronicamente_EntregaTareaFallidaALaVista()
    {
        var busqueda = new BusquedaProductosVenta(_ => throw new InvalidOperationException("Fallo vigente"));
        Task<IReadOnlyList<ProductoVentaDto>?>? consulta = null;

        Assert.Null(Record.Exception(() => { consulta = busqueda.BuscarAsync("texto"); }));

        await Assert.ThrowsAsync<InvalidOperationException>(() => consulta!);
    }

    private static TaskCompletionSource<IReadOnlyList<ProductoVentaDto>> Pendiente()
        => new(TaskCreationOptions.RunContinuationsAsynchronously);
}
