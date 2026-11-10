using CapaDatos;
using CapaEntidad;
using CapaNegocio.DTOs.Compras;
using CapaNegocio.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Xunit;

namespace CapaNegocio.Tests;

/// <summary>
/// Lecturas simultáneas de la pantalla de compras: un <c>DbContext</c> compartido
/// no admite dos consultas activas a la vez. Se pausa de forma determinista una
/// consulta real (dentro de su ejecución) mientras se inicia otra.
/// </summary>
public class CompraNegocioLecturasConcurrentesTests
{
    private const string RolCompras = "Administrador";

    private const string MensajeErrorBuscarProveedores = "No se pudo buscar proveedores por un problema con el origen de datos. Intente nuevamente.";
    private const string MensajeErrorBuscarProductos = "No se pudo buscar productos por un problema con el origen de datos. Intente nuevamente.";

    // ---------- Solapamiento real con el contexto compartido (defecto previo) ----------

    [Fact]
    public async Task Compartido_ProveedoresLuegoProductos_LaSegundaFallaPorUsoConcurrente()
    {
        await using Escenario e = await Escenario.CrearAsync();
        var negocio = new CompraNegocio(e.NuevoContexto());

        (RespuestaBusquedaProveedoresCompra pausada, RespuestaBusquedaProductosCompra solapada) =
            await e.SolaparAsync(
                () => negocio.BuscarProveedoresParaCompraAsync("ferre", null, false, RolCompras),
                () => negocio.BuscarProductosParaCompraAsync("uno", null, false, RolCompras));

        Assert.Equal(ResultadoBusquedaCompras.ConResultados, pausada.Resultado);
        Assert.Equal(ResultadoBusquedaCompras.ErrorPersistencia, solapada.Resultado);
        Assert.Equal(MensajeErrorBuscarProductos, solapada.Mensaje);
    }

    [Fact]
    public async Task Compartido_ProductosLuegoProveedores_LaSegundaFallaPorUsoConcurrente()
    {
        await using Escenario e = await Escenario.CrearAsync();
        var negocio = new CompraNegocio(e.NuevoContexto());

        (RespuestaBusquedaProductosCompra pausada, RespuestaBusquedaProveedoresCompra solapada) =
            await e.SolaparAsync(
                () => negocio.BuscarProductosParaCompraAsync("uno", null, false, RolCompras),
                () => negocio.BuscarProveedoresParaCompraAsync("ferre", null, false, RolCompras));

        Assert.Equal(ResultadoBusquedaCompras.ConResultados, pausada.Resultado);
        Assert.Equal(ResultadoBusquedaCompras.ErrorPersistencia, solapada.Resultado);
        Assert.Equal(MensajeErrorBuscarProveedores, solapada.Mensaje);
    }

    [Fact]
    public async Task Compartido_PreciosLuegoProveedores_LaSegundaFallaPorUsoConcurrente()
    {
        await using Escenario e = await Escenario.CrearAsync();
        var negocio = new CompraNegocio(e.NuevoContexto());

        (RespuestaPreciosProducto pausada, RespuestaBusquedaProveedoresCompra solapada) =
            await e.SolaparAsync(
                () => negocio.ObtenerPreciosProductoAsync(e.IdProducto),
                () => negocio.BuscarProveedoresParaCompraAsync("ferre", null, false, RolCompras));

        Assert.Equal(ResultadoPreciosProducto.Exito, pausada.Resultado);
        Assert.Equal(ResultadoBusquedaCompras.ErrorPersistencia, solapada.Resultado);
        Assert.Equal(MensajeErrorBuscarProveedores, solapada.Mensaje);
    }

    // ---------- Solapamiento real con la fábrica (corrección) ----------

    [Fact]
    public async Task Fabrica_ProveedoresLuegoProductos_AmbasTerminanBien()
    {
        await using Escenario e = await Escenario.CrearAsync();
        var negocio = new CompraNegocio(e.NuevoContexto(), e.Fabrica);

        (RespuestaBusquedaProveedoresCompra pausada, RespuestaBusquedaProductosCompra solapada) =
            await e.SolaparAsync(
                () => negocio.BuscarProveedoresParaCompraAsync("ferre", null, false, RolCompras),
                () => negocio.BuscarProductosParaCompraAsync("uno", null, false, RolCompras));

        Assert.Equal(ResultadoBusquedaCompras.ConResultados, pausada.Resultado);
        Assert.Equal("Ferretería Central", Assert.Single(pausada.Proveedores).RazonSocial);
        Assert.Equal(ResultadoBusquedaCompras.ConResultados, solapada.Resultado);
        Assert.Equal("Uno", Assert.Single(solapada.Productos).Nombre);
    }

    [Fact]
    public async Task Fabrica_ProductosLuegoProveedores_AmbasTerminanBien()
    {
        await using Escenario e = await Escenario.CrearAsync();
        var negocio = new CompraNegocio(e.NuevoContexto(), e.Fabrica);

        (RespuestaBusquedaProductosCompra pausada, RespuestaBusquedaProveedoresCompra solapada) =
            await e.SolaparAsync(
                () => negocio.BuscarProductosParaCompraAsync("uno", null, false, RolCompras),
                () => negocio.BuscarProveedoresParaCompraAsync("ferre", null, false, RolCompras));

        Assert.Equal(ResultadoBusquedaCompras.ConResultados, pausada.Resultado);
        Assert.Equal("Uno", Assert.Single(pausada.Productos).Nombre);
        Assert.Equal(ResultadoBusquedaCompras.ConResultados, solapada.Resultado);
        Assert.Equal("Ferretería Central", Assert.Single(solapada.Proveedores).RazonSocial);
    }

    [Fact]
    public async Task Fabrica_PreciosLuegoProveedores_AmbasTerminanBien()
    {
        await using Escenario e = await Escenario.CrearAsync();
        var negocio = new CompraNegocio(e.NuevoContexto(), e.Fabrica);

        (RespuestaPreciosProducto pausada, RespuestaBusquedaProveedoresCompra solapada) =
            await e.SolaparAsync(
                () => negocio.ObtenerPreciosProductoAsync(e.IdProducto),
                () => negocio.BuscarProveedoresParaCompraAsync("ferre", null, false, RolCompras));

        Assert.Equal(ResultadoPreciosProducto.Exito, pausada.Resultado);
        Assert.Equal(10.5m, pausada.Precios!.PrecioCompra);
        Assert.Equal(ResultadoBusquedaCompras.ConResultados, solapada.Resultado);
        Assert.Single(solapada.Proveedores);
    }

    // ---------- Registro de producción (réplica del de Program.cs) ----------

    [Fact]
    public void Contenedor_ResuelveContextoFabricaYCompraNegocioConFabrica()
    {
        // Réplica de Program.cs: se sustituye solo UseSqlServer por InMemory
        // (el proyecto de pruebas no abre la BD real ni referencia SQL Server).
        var services = new ServiceCollection();
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseInMemoryDatabase($"Contenedor-{Guid.NewGuid()}"));
        services.AddTransient<CompraNegocio>();

        using ServiceProvider raiz = services.BuildServiceProvider();

        Assert.NotNull(raiz.GetRequiredService<IDbContextFactory<AppDbContext>>());

        using IServiceScope ambito = raiz.CreateScope();
        Assert.NotNull(ambito.ServiceProvider.GetRequiredService<AppDbContext>());

        CompraNegocio negocio = ambito.ServiceProvider.GetRequiredService<CompraNegocio>();
        FieldInfo campo = typeof(CompraNegocio)
            .GetField("_fabricaLectura", BindingFlags.Instance | BindingFlags.NonPublic)!;
        Assert.NotNull(campo.GetValue(negocio));
    }

    // ---------- Infraestructura de prueba ----------

    /// <summary>
    /// Pausa de una sola vez: el primer paso por <see cref="Punto"/> (ejecutado
    /// dentro de la consulta, con la sección crítica de EF tomada) avisa y espera.
    /// </summary>
    private sealed class PausaConsulta
    {
        private static readonly TimeSpan Limite = TimeSpan.FromSeconds(30);

        private static PausaConsulta? _activa;

        private readonly TaskCompletionSource _entrada =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly ManualResetEventSlim _puerta = new(false);

        private int _armada = 1;

        public static void Activar(PausaConsulta pausa) => _activa = pausa;

        public static void Desactivar() => _activa = null;

        public static bool Punto(int id)
        {
            _activa?.Pasar();
            return true;
        }

        public Task Entrada => _entrada.Task.WaitAsync(Limite);

        public void Liberar() => _puerta.Set();

        private void Pasar()
        {
            if (Interlocked.Exchange(ref _armada, 0) == 1)
            {
                _entrada.TrySetResult();
                if (!_puerta.Wait(Limite))
                {
                    throw new TimeoutException("La pausa de la consulta no fue liberada.");
                }
            }
        }
    }

    /// <summary>Contexto cuyas lecturas de producto y proveedor pasan por la pausa.</summary>
    private sealed class PausableDbContext : AppDbContext
    {
        public PausableDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Producto>().HasQueryFilter(p => PausaConsulta.Punto(p.IdProducto));
            modelBuilder.Entity<Proveedor>().HasQueryFilter(p => PausaConsulta.Punto(p.IdProveedor));
        }
    }

    private sealed class FabricaPausable : IDbContextFactory<AppDbContext>
    {
        private readonly DbContextOptions<AppDbContext> _opciones;

        public FabricaPausable(DbContextOptions<AppDbContext> opciones) => _opciones = opciones;

        public AppDbContext CreateDbContext() => new PausableDbContext(_opciones);
    }

    private sealed class Escenario : IAsyncDisposable
    {
        private readonly DbContextOptions<AppDbContext> _opciones;
        private readonly PausaConsulta _pausa = new();

        private Escenario(DbContextOptions<AppDbContext> opciones, int idProducto)
        {
            _opciones = opciones;
            IdProducto = idProducto;
            Fabrica = new FabricaPausable(opciones);
        }

        public int IdProducto { get; }

        public IDbContextFactory<AppDbContext> Fabrica { get; }

        public AppDbContext NuevoContexto() => new PausableDbContext(_opciones);

        public static async Task<Escenario> CrearAsync()
        {
            DbContextOptions<AppDbContext> opciones = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"LecturasConcurrentes-{Guid.NewGuid()}")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            await using var db = new PausableDbContext(opciones);
            await db.Database.EnsureCreatedAsync();

            db.Proveedores.Add(new Proveedor
            {
                Cuit = "20451234567",
                RazonSocial = "Ferretería Central",
                Correo = "proveedor@mail.com",
                Telefono = "1145678901",
                Estado = true
            });

            Producto producto = NuevoProducto("COD1", "Uno");
            db.Productos.Add(producto);
            db.Productos.Add(NuevoProducto("COD2", "Dos"));
            await db.SaveChangesAsync();

            return new Escenario(opciones, producto.IdProducto);
        }

        /// <summary>
        /// Inicia <paramref name="pausada"/> en otro hilo y espera a que quede
        /// detenida dentro de su consulta; entonces ejecuta <paramref name="solapada"/>
        /// y recién después libera la primera.
        /// </summary>
        public async Task<(T1 Pausada, T2 Solapada)> SolaparAsync<T1, T2>(
            Func<Task<T1>> pausada, Func<Task<T2>> solapada)
        {
            PausaConsulta.Activar(_pausa);
            Task<T1> primera = Task.Run(pausada);

            try
            {
                await _pausa.Entrada;
                T2 resultadoSolapada = await solapada();

                _pausa.Liberar();
                return (await primera, resultadoSolapada);
            }
            finally
            {
                _pausa.Liberar();
            }
        }

        public ValueTask DisposeAsync()
        {
            PausaConsulta.Desactivar();
            return ValueTask.CompletedTask;
        }

        private static Producto NuevoProducto(string codigo, string nombre) => new()
        {
            IdCategoria = 1,
            Codigo = codigo,
            Nombre = nombre,
            Descripcion = "Dato previo de la prueba",
            PrecioCompra = 10.5m,
            PrecioVenta = 12m,
            Stock = 5,
            Estado = true,
            IdUsuario = 1
        };
    }
}
