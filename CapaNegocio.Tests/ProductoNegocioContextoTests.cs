using System.Data.Common;
using CapaDatos;
using CapaNegocio.DTOs.Productos;
using CapaNegocio.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace CapaNegocio.Tests;

public partial class ProductoNegocioTests
{
    [Fact]
    public async Task Contexto_AltaFallida_NoSeGuardaEnLaSiguienteOperacion()
    {
        var fallo = new FalloGuardadoUnaVez();
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"Productos-aislados-{Guid.NewGuid()}")
            .AddInterceptors(fallo).Options;
        await using var db = new AppDbContext(opciones);
        var categoria = await SembrarCategoriaAsync(db, "Bebidas", true);
        var usuario = await SembrarUsuarioAsync(db);
        var negocio = CrearNegocioContexto(opciones);
        fallo.Activar = true;

        await Assert.ThrowsAsync<DbUpdateException>(() => negocio.CrearProductoAsync(
            CrearDtoValido(categoria.IdCategoria, usuario.IdUsuario)));
        await using (var observador = new AppDbContext(opciones))
            Assert.Empty(await observador.Productos.AsNoTracking().ToListAsync());

        var entrada = CrearDtoValido(categoria.IdCategoria, usuario.IdUsuario);
        entrada.Codigo = "7790001000028";
        var respuesta = await negocio.CrearProductoAsync(entrada);

        Assert.Equal(ResultadoCreacionProducto.Exito, respuesta.Resultado);
        await using var final = new AppDbContext(opciones);
        var producto = Assert.Single(await final.Productos.AsNoTracking().ToListAsync());
        Assert.Equal("7790001000028", producto.Codigo);
    }

    [Fact]
    public async Task Contexto_Reactivar_ConsultaEstadoActualDespuesDeOtraSesion()
    {
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"Productos-vigentes-{Guid.NewGuid()}").Options;
        await using var db = new AppDbContext(opciones);
        var categoria = await SembrarCategoriaAsync(db, "Bebidas", true);
        var usuario = await SembrarUsuarioAsync(db);
        var producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", categoria.IdCategoria, usuario.IdUsuario);
        var negocio = CrearNegocioContexto(opciones);
        var modificacion = await negocio.ModificarProductoAsync(new ModificarProductoDto
        {
            IdProducto = producto.IdProducto, Nombre = "Teclado nuevo",
            IdCategoria = categoria.IdCategoria, PrecioCompra = 1000m,
            PrecioVenta = 1500m, IdUsuario = usuario.IdUsuario
        });
        Assert.Equal(ResultadoModificacionProducto.Exito, modificacion.Resultado);
        var otraSesion = CrearNegocioContexto(opciones);
        var baja = await otraSesion.DarBajaProductoAsync(producto.IdProducto, usuario.IdUsuario);
        Assert.Equal(ResultadoBajaProducto.Exito, baja.Resultado);

        var respuesta = await negocio.ReactivarProductoAsync(producto.IdProducto, usuario.IdUsuario);

        Assert.Equal(ResultadoReactivacionProducto.Exito, respuesta.Resultado);
        await using var observador = new AppDbContext(opciones);
        Assert.True((await observador.Productos.AsNoTracking().SingleAsync()).Estado);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Contexto_ConsultasSimultaneas_NoCompartenContexto(bool consultarCategorias)
    {
        var barrera = new BarreraConexion();
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=localhost;Database=PruebaNoConecta;Integrated Security=true;Encrypt=false")
            .AddInterceptors(barrera).Options;
        var negocio = CrearNegocioContexto(opciones);
        Task primera = negocio.ListarProductosAsync(new FiltroProductosDto());
        await barrera.PrimeraConexion.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Task segunda = consultarCategorias
            ? new CategoriaNegocio(new FabricaContextosPruebas(opciones)).ListarCategoriasActivasAsync()
            : negocio.ContarProductosAsync();
        var errores = await Task.WhenAll(Record.ExceptionAsync(() => primera), Record.ExceptionAsync(() => segunda));

        Assert.Equal(2, barrera.Conexiones);
        Assert.All(errores, error => Assert.IsType<ConexionInterceptadaException>(error));
    }

    private static ProductoNegocio CrearNegocioContexto(DbContextOptions<AppDbContext> opciones)
        => new(new FabricaContextosPruebas(opciones));

    private sealed class FalloGuardadoUnaVez : SaveChangesInterceptor
    {
        public bool Activar { get; set; }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (Activar)
            {
                Activar = false;
                throw new DbUpdateException("Fallo de prueba antes de persistir.");
            }
            return ValueTask.FromResult(result);
        }
    }

    private sealed class ConexionInterceptadaException : Exception;

    private sealed class BarreraConexion : DbConnectionInterceptor
    {
        private readonly TaskCompletionSource _ambas = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource PrimeraConexion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int Conexiones;

        public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection, ConnectionEventData eventData, InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            if (Interlocked.Increment(ref Conexiones) == 2)
                _ambas.TrySetResult();
            PrimeraConexion.TrySetResult();
            await _ambas.Task.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);
            throw new ConexionInterceptadaException();
        }
    }
}
