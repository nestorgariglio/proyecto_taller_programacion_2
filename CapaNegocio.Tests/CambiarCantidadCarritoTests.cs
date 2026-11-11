using CapaDatos;
using CapaEntidad;
using CapaNegocio.DTOs.Ventas;
using CapaNegocio;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CapaNegocio.Tests;

public class CambiarCantidadCarritoTests
{
    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    public async Task Aumentar_StockSuficiente_UsaCantidadFinalYPrecioActualEnTodaLaFila(int cantidad)
    {
        var escenario = await CrearEscenarioAsync(2);
        var anterior = escenario.Carrito;
        await escenario.ActualizarProductoAsync(p => p.PrecioVenta = 12m);

        var resultado = await escenario.CambiarAsync(1, cantidad);

        Assert.True(resultado.Exito);
        Assert.Null(resultado.Mensaje);
        Assert.NotSame(anterior, resultado.Carrito);
        Assert.Equal(new[] { 1, 2 }, resultado.Carrito.Lineas.Select(l => l.IdProducto));
        var linea = resultado.Carrito.Lineas[0];
        Assert.Equal(cantidad, linea.Cantidad);
        Assert.Equal(12m, linea.PrecioUnitario);
        Assert.Equal(cantidad * 12m, linea.Subtotal);
        Assert.Equal(cantidad * 12m + 4.50m, resultado.Carrito.Total);
        Assert.Equal(anterior.Lineas[1], resultado.Carrito.Lineas[1]);
        Assert.Equal(2, anterior.Lineas[0].Cantidad);
        Assert.Equal(10.25m, anterior.Lineas[0].PrecioUnitario);
        Assert.Equal(25m, anterior.Total);
    }

    [Theory]
    [InlineData(2, 0)]
    [InlineData(3, 1)]
    public async Task Reducir_AunSuperandoStockActual_ConservaPrecioYRecalculaImportes(int cantidad, int stock)
    {
        var escenario = await CrearEscenarioAsync(4);
        var anterior = escenario.Carrito;
        await escenario.ActualizarProductoAsync(p => { p.Stock = stock; p.PrecioVenta = 12m; });

        var resultado = await escenario.CambiarAsync(1, cantidad);

        Assert.True(resultado.Exito);
        Assert.Null(resultado.Mensaje);
        Assert.NotSame(anterior, resultado.Carrito);
        Assert.Equal(2, resultado.Carrito.Lineas.Count);
        var linea = resultado.Carrito.Lineas[0];
        Assert.Equal(anterior.Lineas[0] with { Cantidad = cantidad }, linea);
        Assert.Equal(cantidad * 10.25m, linea.Subtotal);
        Assert.Equal(cantidad * 10.25m + 4.50m, resultado.Carrito.Total);
        Assert.Equal(anterior.Lineas[1], resultado.Carrito.Lineas[1]);
        Assert.Equal(4, anterior.Lineas[0].Cantidad);
        Assert.Equal(45.50m, anterior.Total);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task CantidadNoPositiva_RechazaSinEliminarFila(int cantidad)
    {
        var escenario = await CrearEscenarioAsync(2);

        var resultado = await escenario.CambiarAsync(1, cantidad);

        ComprobarRechazo(resultado, escenario.Carrito, "La cantidad debe ser mayor a cero.");
        Assert.Equal(2, resultado.Carrito.Lineas.Count);
        Assert.Equal(2, resultado.Carrito.Lineas[0].Cantidad);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(99)]
    [InlineData(0)]
    public async Task ProductoSinFila_RechazaSinAgregarlo(int idProducto)
    {
        var escenario = await CrearEscenarioAsync(2);

        var resultado = await escenario.CambiarAsync(idProducto, 1);

        ComprobarRechazo(resultado, escenario.Carrito, "El producto no está en el carrito.");
    }

    [Fact]
    public async Task CarritoVacio_NoConvierteEdicionEnAgregado()
    {
        var escenario = await CrearEscenarioAsync(2);
        var vacio = new CarritoVentaDto();

        var resultado = await escenario.Negocio.CambiarCantidadAsync(vacio, 1, 1);

        ComprobarRechazo(resultado, vacio, "El producto no está en el carrito.");
        Assert.Empty(resultado.Carrito.Lineas);
        Assert.Equal(0m, resultado.Carrito.Total);
    }

    [Theory]
    [InlineData(4, 5, 6, 1)]
    [InlineData(2, 3, 4, 1)]
    [InlineData(2, 1, 3, 0)]
    [InlineData(2, 5, int.MaxValue, 3)]
    public async Task Aumentar_SuperaStockActual_ConservaCantidadPrecioYTotal(
        int inicial, int stock, int cantidad, int disponible)
    {
        var escenario = await CrearEscenarioAsync(inicial);
        await escenario.ActualizarProductoAsync(p => { p.Stock = stock; p.PrecioVenta = 12m; });

        var resultado = await escenario.CambiarAsync(1, cantidad);

        ComprobarRechazo(resultado, escenario.Carrito, $"Stock insuficiente. Disponible: {disponible}");
        Assert.Equal(inicial, resultado.Carrito.Lineas[0].Cantidad);
        Assert.Equal(10.25m, resultado.Carrito.Lineas[0].PrecioUnitario);
        Assert.Equal(inicial * 10.25m + 4.50m, resultado.Carrito.Total);
    }

    [Fact]
    public async Task Aumentar_DesactivadoExternamente_ConservaCarrito()
    {
        var escenario = await CrearEscenarioAsync(2);
        await escenario.ActualizarProductoAsync(p => p.Estado = false);

        var resultado = await escenario.CambiarAsync(1, 3);

        ComprobarRechazo(resultado, escenario.Carrito, "El producto no existe o está inactivo.");
    }

    [Fact]
    public async Task MismaCantidad_NoRefrescaPrecioNiRechazaPorStockReducido()
    {
        var escenario = await CrearEscenarioAsync(4);
        await escenario.ActualizarProductoAsync(p => { p.Stock = 0; p.PrecioVenta = 12m; });

        var resultado = await escenario.CambiarAsync(1, 4);

        Assert.True(resultado.Exito);
        Assert.Null(resultado.Mensaje);
        Assert.Same(escenario.Carrito, resultado.Carrito);
        Assert.Equal(10.25m, resultado.Carrito.Lineas[0].PrecioUnitario);
        Assert.Equal(45.50m, resultado.Carrito.Total);
    }

    private static void ComprobarRechazo(ResultadoCarritoDto resultado, CarritoVentaDto anterior, string mensaje)
    {
        Assert.False(resultado.Exito);
        Assert.Equal(mensaje, resultado.Mensaje);
        Assert.Same(anterior, resultado.Carrito);
    }

    private static async Task<Escenario> CrearEscenarioAsync(int cantidad)
    {
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new AppDbContext(opciones);
        db.Productos.AddRange(
            new Producto { IdProducto = 1, Nombre = "Leche", Stock = 5, PrecioVenta = 10.25m },
            new Producto { IdProducto = 2, Nombre = "Pan", Stock = 10, PrecioVenta = 4.50m },
            new Producto { IdProducto = 3, Nombre = "Otro", Stock = 5 });
        await db.SaveChangesAsync();
        var negocio = new CarritoVentaNegocio(new ConsultaProductosVentaDatos(opciones));
        var primero = await negocio.AgregarProductoAsync(new(), 1, cantidad);
        var segundo = await negocio.AgregarProductoAsync(primero.Carrito, 2, 1);
        Assert.True(primero.Exito);
        Assert.True(segundo.Exito);
        return new Escenario(opciones, negocio, segundo.Carrito);
    }

    private sealed record Escenario(
        DbContextOptions<AppDbContext> Opciones, CarritoVentaNegocio Negocio, CarritoVentaDto Carrito)
    {
        public async Task<ResultadoCarritoDto> CambiarAsync(int idProducto, int cantidad)
        {
            await using var db = new AppDbContext(Opciones);
            var anterior = await db.Productos.AsNoTracking().OrderBy(p => p.IdProducto)
                .Select(p => new { p.IdProducto, p.Stock, p.PrecioVenta, p.Estado }).ToArrayAsync();

            var resultado = await Negocio.CambiarCantidadAsync(Carrito, idProducto, cantidad);

            var actual = await db.Productos.AsNoTracking().OrderBy(p => p.IdProducto)
                .Select(p => new { p.IdProducto, p.Stock, p.PrecioVenta, p.Estado }).ToArrayAsync();
            Assert.Equal(anterior, actual);
            Assert.Equal(0, await db.Ventas.CountAsync());
            Assert.Equal(0, await db.DetallesVenta.CountAsync());
            return resultado;
        }

        public async Task ActualizarProductoAsync(Action<Producto> actualizar)
        {
            await using var db = new AppDbContext(Opciones);
            actualizar(await db.Productos.SingleAsync(p => p.IdProducto == 1));
            await db.SaveChangesAsync();
        }
    }
}
