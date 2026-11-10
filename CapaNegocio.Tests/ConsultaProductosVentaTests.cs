using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CapaNegocio.Tests;

public class ConsultaProductosVentaTests
{
    [Theory]
    [InlineData("00123")]
    [InlineData("leche")]
    [InlineData("  LeChE  ")]
    public async Task Buscar_CodigoONombre_DevuelveSoloActivosConDatosDelPos(string busqueda)
    {
        var opciones = await PrepararAsync();
        var negocio = CrearNegocio(opciones);

        var productos = await negocio.BuscarProductosAsync(busqueda);

        var producto = Assert.Single(productos);
        Assert.Equal(1, producto.IdProducto);
        Assert.Equal("00123", producto.Codigo);
        Assert.Equal("Leche entera", producto.Nombre);
        Assert.Equal(5, producto.Stock);
        Assert.Equal(1200.50m, producto.PrecioVenta);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("inexistente")]
    public async Task Buscar_SinCoincidencias_DevuelveListaVacia(string? busqueda)
    {
        var negocio = CrearNegocio(await PrepararAsync());

        Assert.Empty(await negocio.BuscarProductosAsync(busqueda));
    }

    [Fact]
    public async Task Buscar_CoincidenciasMultiples_TieneOrdenEstablePorNombreEId()
    {
        var opciones = await PrepararAsync();
        await using (var db = new AppDbContext(opciones))
        {
            db.Productos.Add(new Producto { IdProducto = 3, Codigo = "003", Nombre = "Leche descremada" });
            db.Productos.Add(new Producto { IdProducto = 4, Codigo = "004", Nombre = "Leche descremada" });
            await db.SaveChangesAsync();
        }

        var productos = await CrearNegocio(opciones).BuscarProductosAsync("leche");

        Assert.Equal(new[] { 3, 4, 1 }, productos.Select(p => p.IdProducto));
    }

    [Fact]
    public async Task Obtener_Activo_DevuelveDatosActualesDelPos()
    {
        var negocio = CrearNegocio(await PrepararAsync());

        var producto = await negocio.ObtenerProductoAsync(1);

        Assert.NotNull(producto);
        Assert.Equal("00123", producto.Codigo);
        Assert.Equal("Leche entera", producto.Nombre);
        Assert.Equal(5, producto.Stock);
        Assert.Equal(1200.50m, producto.PrecioVenta);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(99)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Obtener_InactivoOInexistente_NoLoOfreceParaVenta(int idProducto)
    {
        var negocio = CrearNegocio(await PrepararAsync());

        Assert.Null(await negocio.ObtenerProductoAsync(idProducto));
    }

    [Fact]
    public async Task ConsultarDeNuevo_CambioExterno_LeePrecioStockYEstadoActuales()
    {
        var opciones = await PrepararAsync();
        var negocio = CrearNegocio(opciones);
        var anterior = await negocio.ObtenerProductoAsync(1);
        Assert.NotNull(anterior);

        await using (var db = new AppDbContext(opciones))
        {
            var producto = await db.Productos.SingleAsync(p => p.IdProducto == 1);
            producto.Stock = 2;
            producto.PrecioVenta = 1400m;
            await db.SaveChangesAsync();
        }

        var actual = await negocio.ObtenerProductoAsync(1);
        Assert.NotNull(actual);
        Assert.Equal(2, actual.Stock);
        Assert.Equal(1400m, actual.PrecioVenta);
        Assert.Equal(actual, Assert.Single(await negocio.BuscarProductosAsync("00123")));
        Assert.Equal(5, anterior.Stock);
        Assert.Equal(1200.50m, anterior.PrecioVenta);

        await using (var db = new AppDbContext(opciones))
        {
            var producto = await db.Productos.SingleAsync(p => p.IdProducto == 1);
            producto.Estado = false;
            await db.SaveChangesAsync();
        }

        Assert.Null(await negocio.ObtenerProductoAsync(1));
        Assert.Empty(await negocio.BuscarProductosAsync("00123"));
    }

    [Fact]
    public async Task Datos_LecturasDesconectadas_NoPersistenCambiosAlObjetoDevuelto()
    {
        var opciones = await PrepararAsync();
        var datos = new ConsultaProductosVentaDatos(opciones);
        var producto = await datos.ObtenerActivoAsync(1);
        Assert.NotNull(producto);
        producto.Stock = 999;
        var resultados = await datos.BuscarActivosAsync("00123");
        Assert.Equal(5, Assert.Single(resultados).Stock);
        Assert.NotSame(producto, resultados[0]);

        await using var db = new AppDbContext(opciones);
        Assert.Equal(2, await db.Productos.CountAsync());
        Assert.Equal(5, (await db.Productos.SingleAsync(p => p.IdProducto == 1)).Stock);
    }

    private static CarritoVentaNegocio CrearNegocio(DbContextOptions<AppDbContext> opciones)
        => new(new ConsultaProductosVentaDatos(opciones));

    private static async Task<DbContextOptions<AppDbContext>> PrepararAsync()
    {
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new AppDbContext(opciones);
        db.Productos.AddRange(
            new Producto { IdProducto = 1, Codigo = "00123", Nombre = "Leche entera", Stock = 5, PrecioVenta = 1200.50m },
            new Producto { IdProducto = 2, Codigo = "00999", Nombre = "Leche inactiva", Stock = 10, Estado = false });
        await db.SaveChangesAsync();
        return opciones;
    }
}
