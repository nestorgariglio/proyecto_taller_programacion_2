using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using CapaNegocio.DTOs.Ventas;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CapaNegocio.Tests;

public class QuitarProductoCarritoTests
{
    [Theory]
    [InlineData(1, 2, 450)]
    [InlineData(2, 1, 2050)]
    public async Task Quitar_FilaExistente_RecalculaTotalYConservaOtrasFilas(
        int idProducto, int idRestante, int totalEsperadoEnCentavos)
    {
        var escenario = await CrearEscenarioAsync();

        var resultado = await escenario.QuitarAsync(escenario.Carrito, idProducto);

        Assert.True(resultado.Exito);
        Assert.Null(resultado.Mensaje);
        Assert.NotSame(escenario.Carrito, resultado.Carrito);
        var restante = Assert.Single(resultado.Carrito.Lineas);
        Assert.Equal(idRestante, restante.IdProducto);
        Assert.Equal(escenario.Carrito.Lineas.Single(l => l.IdProducto == idRestante), restante);
        Assert.Equal(totalEsperadoEnCentavos / 100m, resultado.Carrito.Total);
        Assert.Equal(new[] { 1, 2 }, escenario.Carrito.Lineas.Select(l => l.IdProducto));
        Assert.Equal(25m, escenario.Carrito.Total);
    }

    [Fact]
    public async Task Quitar_UltimaFila_DejaCarritoVacioConTotalCero()
    {
        var escenario = await CrearEscenarioAsync();
        var anterior = (await escenario.QuitarAsync(escenario.Carrito, 2)).Carrito;

        var resultado = await escenario.QuitarAsync(anterior, 1);

        Assert.True(resultado.Exito);
        Assert.Null(resultado.Mensaje);
        Assert.Empty(resultado.Carrito.Lineas);
        Assert.Equal(0m, resultado.Carrito.Total);
        Assert.Single(anterior.Lineas);
        Assert.Equal(20.50m, anterior.Total);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(99)]
    public async Task Quitar_FilaInexistente_RechazaSinCambiarCarrito(int idProducto)
    {
        var escenario = await CrearEscenarioAsync();

        var resultado = await escenario.QuitarAsync(escenario.Carrito, idProducto);

        Assert.False(resultado.Exito);
        Assert.Equal("El producto no está en el carrito.", resultado.Mensaje);
        Assert.Same(escenario.Carrito, resultado.Carrito);
    }

    [Fact]
    public async Task Quitar_CarritoVacio_RechazaSinCambiarlo()
    {
        var escenario = await CrearEscenarioAsync();
        var vacio = new CarritoVentaDto();

        var resultado = await escenario.QuitarAsync(vacio, 1);

        Assert.False(resultado.Exito);
        Assert.Equal("El producto no está en el carrito.", resultado.Mensaje);
        Assert.Same(vacio, resultado.Carrito);
        Assert.Empty(resultado.Carrito.Lineas);
        Assert.Equal(0m, resultado.Carrito.Total);
    }

    [Fact]
    public async Task Quitar_ProductoDesactivadoYSinStock_SePermiteSinActualizarOtrasFilas()
    {
        var escenario = await CrearEscenarioAsync();
        await using (var db = new AppDbContext(escenario.Opciones))
        {
            var producto = await db.Productos.SingleAsync(p => p.IdProducto == 1);
            producto.Estado = false;
            producto.Stock = 0;
            producto.PrecioVenta = 99m;
            await db.SaveChangesAsync();
        }

        var resultado = await escenario.QuitarAsync(escenario.Carrito, 1);

        Assert.True(resultado.Exito);
        Assert.Equal(escenario.Carrito.Lineas[1], Assert.Single(resultado.Carrito.Lineas));
        Assert.Equal(4.50m, resultado.Carrito.Total);
        Assert.Equal(25m, escenario.Carrito.Total);
    }

    [Fact]
    public async Task Quitar_SinProveedorDeBaseDeDatos_NoNecesitaConsultarLaDal()
    {
        var escenario = await CrearEscenarioAsync();
        var sinProveedor = new DbContextOptionsBuilder<AppDbContext>().Options;
        var negocio = new CarritoVentaNegocio(new ConsultaProductosVentaDatos(sinProveedor));

        var resultado = negocio.QuitarProducto(escenario.Carrito, 1);

        Assert.True(resultado.Exito);
        Assert.Equal(2, Assert.Single(resultado.Carrito.Lineas).IdProducto);
        Assert.Equal(4.50m, resultado.Carrito.Total);
    }

    private static async Task<Escenario> CrearEscenarioAsync()
    {
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new AppDbContext(opciones);
        db.Productos.AddRange(
            new Producto { IdProducto = 1, Nombre = "Leche", Stock = 5, PrecioVenta = 10.25m },
            new Producto { IdProducto = 2, Nombre = "Pan", Stock = 10, PrecioVenta = 4.50m });
        await db.SaveChangesAsync();
        var negocio = new CarritoVentaNegocio(new ConsultaProductosVentaDatos(opciones));
        var primero = await negocio.AgregarProductoAsync(new(), 1, 2);
        var segundo = await negocio.AgregarProductoAsync(primero.Carrito, 2, 1);
        Assert.True(primero.Exito);
        Assert.True(segundo.Exito);
        return new Escenario(opciones, negocio, segundo.Carrito);
    }

    private sealed record Escenario(
        DbContextOptions<AppDbContext> Opciones, CarritoVentaNegocio Negocio, CarritoVentaDto Carrito)
    {
        public async Task<ResultadoCarritoDto> QuitarAsync(CarritoVentaDto carrito, int idProducto)
        {
            await using var db = new AppDbContext(Opciones);
            var anterior = await db.Productos.AsNoTracking().OrderBy(p => p.IdProducto)
                .Select(p => new { p.IdProducto, p.Stock, p.PrecioVenta, p.Estado }).ToArrayAsync();

            var resultado = Negocio.QuitarProducto(carrito, idProducto);

            var actual = await db.Productos.AsNoTracking().OrderBy(p => p.IdProducto)
                .Select(p => new { p.IdProducto, p.Stock, p.PrecioVenta, p.Estado }).ToArrayAsync();
            Assert.Equal(anterior, actual);
            Assert.Equal(0, await db.Ventas.CountAsync());
            Assert.Equal(0, await db.DetallesVenta.CountAsync());
            return resultado;
        }
    }
}
