using CapaDatos;
using CapaEntidad;
using CapaNegocio.DTOs.Ventas;
using CapaNegocio;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CapaNegocio.Tests;

public class AgregarProductoCarritoTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public async Task Agregar_ProductoActivo_CreaFilaYCalculaImportes(int cantidad)
    {
        var escenario = await CrearEscenarioAsync();
        var anterior = new CarritoVentaDto();

        var resultado = await escenario.AgregarAsync(anterior, 1, cantidad);

        Assert.True(resultado.Exito);
        Assert.Null(resultado.Mensaje);
        Assert.NotSame(anterior, resultado.Carrito);
        var linea = Assert.Single(resultado.Carrito.Lineas);
        Assert.Equal(1, linea.IdProducto);
        Assert.Equal("00123", linea.Codigo);
        Assert.Equal("Leche", linea.Nombre);
        Assert.Equal(cantidad, linea.Cantidad);
        Assert.Equal(10.25m, linea.PrecioUnitario);
        Assert.Equal(cantidad * 10.25m, linea.Subtotal);
        Assert.Equal(linea.Subtotal, resultado.Carrito.Total);
        Assert.Empty(anterior.Lineas);
        Assert.Equal(0m, anterior.Total);
    }

    [Fact]
    public async Task Agregar_Repetido_AcumulaEnUnaFilaHastaElStockExacto()
    {
        var escenario = await CrearEscenarioAsync();
        var anterior = (await escenario.AgregarAsync(new(), 1, 2)).Carrito;

        var resultado = await escenario.AgregarAsync(anterior, 1, 3);

        Assert.True(resultado.Exito);
        var linea = Assert.Single(resultado.Carrito.Lineas);
        Assert.Equal(5, linea.Cantidad);
        Assert.Equal(51.25m, linea.Subtotal);
        Assert.Equal(51.25m, resultado.Carrito.Total);
        Assert.Equal(2, Assert.Single(anterior.Lineas).Cantidad);
        Assert.Equal(20.50m, anterior.Total);
    }

    [Fact]
    public async Task Agregar_OtroProducto_ConservaFilaAnteriorYAcumulaTotal()
    {
        var escenario = await CrearEscenarioAsync();
        var anterior = (await escenario.AgregarAsync(new(), 1, 2)).Carrito;

        var resultado = await escenario.AgregarAsync(anterior, 2, 3);

        Assert.True(resultado.Exito);
        Assert.Equal(new[] { 1, 2 }, resultado.Carrito.Lineas.Select(l => l.IdProducto));
        Assert.Equal(anterior.Lineas[0], resultado.Carrito.Lineas[0]);
        Assert.Equal(34m, resultado.Carrito.Total);
        Assert.Single(anterior.Lineas);
        Assert.Equal(20.50m, anterior.Total);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task Agregar_CantidadNoPositiva_RechazaSinCambiarCarrito(int cantidad)
    {
        var escenario = await CrearEscenarioAsync();
        var anterior = (await escenario.AgregarAsync(new(), 1, 1)).Carrito;

        var resultado = await escenario.AgregarAsync(anterior, 1, cantidad);

        ComprobarRechazo(resultado, anterior, "La cantidad debe ser mayor a cero.");
    }

    [Theory]
    [InlineData(3)]
    [InlineData(99)]
    [InlineData(0)]
    public async Task Agregar_InactivoOInexistente_RechazaSinCambiarCarrito(int idProducto)
    {
        var escenario = await CrearEscenarioAsync();
        var anterior = (await escenario.AgregarAsync(new(), 1, 1)).Carrito;

        var resultado = await escenario.AgregarAsync(anterior, idProducto, 1);

        ComprobarRechazo(resultado, anterior, "El producto no existe o está inactivo.");
    }

    [Theory]
    [InlineData(0, 6, 5)]
    [InlineData(4, 2, 1)]
    [InlineData(5, 1, 0)]
    [InlineData(1, int.MaxValue, 4)]
    public async Task Agregar_SuperaStockAcumulado_InformaDisponibleAdicional(
        int cantidadAnterior, int cantidadNueva, int disponible)
    {
        var escenario = await CrearEscenarioAsync();
        var anterior = cantidadAnterior == 0 ? new CarritoVentaDto() :
            (await escenario.AgregarAsync(new(), 1, cantidadAnterior)).Carrito;

        var resultado = await escenario.AgregarAsync(anterior, 1, cantidadNueva);

        ComprobarRechazo(resultado, anterior, $"Stock insuficiente. Disponible: {disponible}");
    }

    [Fact]
    public async Task Agregar_PrecioCambio_ActualizaTodaLaFilaSinCambiarOtrasNiSnapshotAnterior()
    {
        var escenario = await CrearEscenarioAsync();
        var primero = (await escenario.AgregarAsync(new(), 1, 2)).Carrito;
        var anterior = (await escenario.AgregarAsync(primero, 2, 1)).Carrito;
        await escenario.ActualizarProductoAsync(1, p => p.PrecioVenta = 12m);

        var resultado = await escenario.AgregarAsync(anterior, 1, 1);

        Assert.True(resultado.Exito);
        Assert.Equal(2, resultado.Carrito.Lineas.Count);
        var linea = resultado.Carrito.Lineas[0];
        Assert.Equal(3, linea.Cantidad);
        Assert.Equal(12m, linea.PrecioUnitario);
        Assert.Equal(36m, linea.Subtotal);
        Assert.Equal(40.50m, resultado.Carrito.Total);
        Assert.Equal(anterior.Lineas[1], resultado.Carrito.Lineas[1]);
        Assert.Equal(10.25m, anterior.Lineas[0].PrecioUnitario);
        Assert.Equal(25m, anterior.Total);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(0)]
    public async Task Agregar_StockBajaExternamente_RechazaSinActualizarPrecioNiDarDisponibleNegativo(int stock)
    {
        var escenario = await CrearEscenarioAsync();
        var anterior = (await escenario.AgregarAsync(new(), 1, 4)).Carrito;
        await escenario.ActualizarProductoAsync(1, p => { p.Stock = stock; p.PrecioVenta = 12m; });

        var resultado = await escenario.AgregarAsync(anterior, 1, 1);

        ComprobarRechazo(resultado, anterior, "Stock insuficiente. Disponible: 0");
        Assert.Equal(10.25m, anterior.Lineas[0].PrecioUnitario);
        Assert.Equal(41m, anterior.Total);
    }

    [Fact]
    public async Task Agregar_DesactivadoDespuesDelPrimerAgregado_ConservaCarrito()
    {
        var escenario = await CrearEscenarioAsync();
        var anterior = (await escenario.AgregarAsync(new(), 1, 1)).Carrito;
        await escenario.ActualizarProductoAsync(1, p => p.Estado = false);

        var resultado = await escenario.AgregarAsync(anterior, 1, 1);

        ComprobarRechazo(resultado, anterior, "El producto no existe o está inactivo.");
    }

    [Fact]
    public async Task Agregar_CarritosDistintos_NoComparteEstadoNiReservaStock()
    {
        var escenario = await CrearEscenarioAsync();
        var vacio = new CarritoVentaDto();

        var primero = await escenario.AgregarAsync(vacio, 1, 5);
        var segundo = await escenario.AgregarAsync(vacio, 1, 5);

        Assert.True(primero.Exito);
        Assert.True(segundo.Exito);
        Assert.NotSame(primero.Carrito, segundo.Carrito);
        Assert.Equal(5, Assert.Single(primero.Carrito.Lineas).Cantidad);
        Assert.Equal(5, Assert.Single(segundo.Carrito.Lineas).Cantidad);
        Assert.Empty(vacio.Lineas);
        var lista = Assert.IsAssignableFrom<IList<LineaCarritoDto>>(primero.Carrito.Lineas);
        Assert.Throws<NotSupportedException>(() => lista.Clear());
    }

    private static void ComprobarRechazo(ResultadoCarritoDto resultado, CarritoVentaDto anterior, string mensaje)
    {
        Assert.False(resultado.Exito);
        Assert.Equal(mensaje, resultado.Mensaje);
        Assert.Same(anterior, resultado.Carrito);
    }

    private static async Task<Escenario> CrearEscenarioAsync()
    {
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new AppDbContext(opciones);
        db.Productos.AddRange(
            new Producto { IdProducto = 1, Codigo = "00123", Nombre = "Leche", Stock = 5, PrecioVenta = 10.25m },
            new Producto { IdProducto = 2, Codigo = "002", Nombre = "Pan", Stock = 10, PrecioVenta = 4.50m },
            new Producto { IdProducto = 3, Nombre = "Inactivo", Stock = 10, Estado = false });
        await db.SaveChangesAsync();
        return new Escenario(opciones);
    }

    private sealed class Escenario(DbContextOptions<AppDbContext> opciones)
    {
        private readonly CarritoVentaNegocio negocio = new(new ConsultaProductosVentaDatos(opciones));

        public async Task<ResultadoCarritoDto> AgregarAsync(CarritoVentaDto carrito, int idProducto, int cantidad)
        {
            await using var db = new AppDbContext(opciones);
            var anterior = await db.Productos.AsNoTracking().OrderBy(p => p.IdProducto)
                .Select(p => new { p.IdProducto, p.Codigo, p.Nombre, p.Stock, p.PrecioVenta, p.Estado }).ToArrayAsync();

            var resultado = await negocio.AgregarProductoAsync(carrito, idProducto, cantidad);

            var actual = await db.Productos.AsNoTracking().OrderBy(p => p.IdProducto)
                .Select(p => new { p.IdProducto, p.Codigo, p.Nombre, p.Stock, p.PrecioVenta, p.Estado }).ToArrayAsync();
            Assert.Equal(anterior, actual);
            Assert.Equal(0, await db.Ventas.CountAsync());
            Assert.Equal(0, await db.DetallesVenta.CountAsync());
            return resultado;
        }

        public async Task ActualizarProductoAsync(int idProducto, Action<Producto> actualizar)
        {
            await using var db = new AppDbContext(opciones);
            actualizar(await db.Productos.SingleAsync(p => p.IdProducto == idProducto));
            await db.SaveChangesAsync();
        }
    }
}
