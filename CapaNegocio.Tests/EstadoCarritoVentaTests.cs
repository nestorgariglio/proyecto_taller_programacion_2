using CapaDatos;
using CapaEntidad;
using CapaNegocio.DTOs.Ventas;
using CapaPresentacion.Ventas;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CapaNegocio.Tests;

public class EstadoCarritoVentaTests
{
    [Fact]
    public async Task Ejecutar_Exito_PublicaSoloElNuevoSnapshot()
    {
        var estado = new EstadoCarritoVenta();
        var anterior = estado.Carrito;
        var nuevo = new CarritoVentaDto();

        var resultado = await estado.EjecutarAsync(carrito =>
        {
            Assert.Same(anterior, carrito);
            return Task.FromResult(new ResultadoCarritoDto(true, null, nuevo));
        });

        Assert.True(resultado.Exito);
        Assert.Same(nuevo, estado.Carrito);
        Assert.NotSame(anterior, estado.Carrito);
    }

    [Fact]
    public async Task Ejecutar_Rechazo_NoPublicaUnCarritoDistinto()
    {
        var estado = new EstadoCarritoVenta();
        var anterior = estado.Carrito;

        var resultado = await estado.EjecutarAsync(_ => Task.FromResult(
            new ResultadoCarritoDto(false, "Rechazado", new CarritoVentaDto())));

        Assert.False(resultado.Exito);
        Assert.Equal("Rechazado", resultado.Mensaje);
        Assert.Same(anterior, estado.Carrito);
    }

    [Fact]
    public async Task Ejecutar_OperacionPendiente_LaSiguienteEsperaYRecibeEstadoActual()
    {
        var estado = new EstadoCarritoVenta();
        var liberar = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var nuevo = new CarritoVentaDto();
        bool segundaIniciada = false;
        var primera = estado.EjecutarAsync(async _ =>
        {
            await liberar.Task;
            return new ResultadoCarritoDto(true, null, nuevo);
        });
        var segunda = estado.EjecutarAsync(carrito =>
        {
            segundaIniciada = true;
            Assert.Same(nuevo, carrito);
            return Task.FromResult(new ResultadoCarritoDto(true, null, carrito));
        });

        Assert.False(segundaIniciada);
        Assert.False(segunda.IsCompleted);
        liberar.SetResult();
        await primera;
        await segunda;
        Assert.True(segundaIniciada);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Ejecutar_Excepcion_LiberaColaYConservaEstado(bool fallaSincronica)
    {
        var estado = new EstadoCarritoVenta();
        var anterior = estado.Carrito;
        var liberar = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var primera = estado.EjecutarAsync(async _ =>
        {
            await liberar.Task;
            return new ResultadoCarritoDto(true, null, anterior);
        });
        var fallida = estado.EjecutarAsync(_ =>
        {
            if (fallaSincronica) throw new InvalidOperationException("Fallo");
            return Task.FromException<ResultadoCarritoDto>(new InvalidOperationException("Fallo"));
        });
        var segunda = estado.EjecutarAsync(carrito =>
        {
            Assert.Same(anterior, carrito);
            return Task.FromResult(new ResultadoCarritoDto(true, null, carrito));
        });

        liberar.SetResult();
        await primera;
        await Assert.ThrowsAsync<InvalidOperationException>(() => fallida);
        Assert.True((await segunda).Exito);
        Assert.Same(anterior, estado.Carrito);
    }

    [Fact]
    public async Task Ejecutar_Cancelacion_NoBloqueaLaSiguienteOperacion()
    {
        var estado = new EstadoCarritoVenta();
        var cancelar = new TaskCompletionSource<ResultadoCarritoDto>(TaskCreationOptions.RunContinuationsAsynchronously);
        var primera = estado.EjecutarAsync(_ => cancelar.Task);
        var segunda = estado.EjecutarAsync(c => Task.FromResult(new ResultadoCarritoDto(true, null, c)));

        cancelar.SetCanceled();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => primera);
        Assert.True((await segunda).Exito);
    }

    [Fact]
    public async Task Ejecutar_AgregadosRapidos_SeAcumulanSinPerderCantidadNiEscribirStock()
    {
        var (opciones, negocio) = await CrearNegocioAsync();
        var estado = new EstadoCarritoVenta();
        var liberar = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var vacio = estado.Carrito;
        var primera = estado.EjecutarAsync(async carrito =>
        {
            await liberar.Task;
            return await negocio.AgregarProductoAsync(carrito, 1, 2);
        });
        var segunda = estado.EjecutarAsync(carrito => negocio.AgregarProductoAsync(carrito, 1, 3));

        Assert.Empty(estado.Carrito.Lineas);
        liberar.SetResult();
        Assert.True((await primera).Exito);
        Assert.True((await segunda).Exito);
        Assert.Equal(5, Assert.Single(estado.Carrito.Lineas).Cantidad);
        Assert.Equal(50m, estado.Carrito.Total);
        Assert.Empty(vacio.Lineas);
        await using var db = new AppDbContext(opciones);
        Assert.Equal(5, (await db.Productos.SingleAsync()).Stock);
        Assert.Empty(await db.Ventas.ToListAsync());
    }

    [Fact]
    public async Task Ejecutar_RechazoDeStock_PermiteEditarYQuitarDespues()
    {
        var (_, negocio) = await CrearNegocioAsync();
        var estado = new EstadoCarritoVenta();
        await estado.EjecutarAsync(c => negocio.AgregarProductoAsync(c, 1, 4));
        var anterior = estado.Carrito;

        var rechazo = await estado.EjecutarAsync(c => negocio.AgregarProductoAsync(c, 1, 2));

        Assert.False(rechazo.Exito);
        Assert.Equal("Stock insuficiente. Disponible: 1", rechazo.Mensaje);
        Assert.Same(anterior, estado.Carrito);
        Assert.True((await estado.EjecutarAsync(c => negocio.CambiarCantidadAsync(c, 1, 2))).Exito);
        Assert.Equal(20m, estado.Carrito.Total);
        Assert.True((await estado.EjecutarAsync(c => Task.FromResult(negocio.QuitarProducto(c, 1)))).Exito);
        Assert.Empty(estado.Carrito.Lineas);
        Assert.Equal(0m, estado.Carrito.Total);
        Assert.Equal(4, anterior.Lineas[0].Cantidad);
    }

    [Fact]
    public async Task VistasDistintas_NoCompartenCarrito()
    {
        var (_, negocio) = await CrearNegocioAsync();
        var primera = new EstadoCarritoVenta();
        var segunda = new EstadoCarritoVenta();

        await primera.EjecutarAsync(c => negocio.AgregarProductoAsync(c, 1, 2));

        Assert.Equal(2, Assert.Single(primera.Carrito.Lineas).Cantidad);
        Assert.Empty(segunda.Carrito.Lineas);
        Assert.Equal(0m, segunda.Carrito.Total);
    }

    private static async Task<(DbContextOptions<AppDbContext>, CarritoVentaNegocio)> CrearNegocioAsync()
    {
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new AppDbContext(opciones);
        db.Productos.Add(new Producto { IdProducto = 1, Nombre = "Producto", Stock = 5, PrecioVenta = 10m });
        await db.SaveChangesAsync();
        return (opciones, new CarritoVentaNegocio(new ConsultaProductosVentaDatos(opciones)));
    }
}
