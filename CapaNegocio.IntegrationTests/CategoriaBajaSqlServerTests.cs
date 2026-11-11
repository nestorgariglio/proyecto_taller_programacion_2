using System.Data.Common;
using CapaDatos;
using CapaNegocio.DTOs.Categorias;
using CapaNegocio.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace CapaNegocio.IntegrationTests;

/// <summary>
/// Baja de categorías contra SQL Server real (DB_SISTEMA_VENTA_TEST), sin residuos:
/// los escenarios (a)-(c) corren dentro de una transacción compartida que se revierte;
/// el escenario de concurrencia (d) revierte su propia transacción de forma deliberada.
/// </summary>
public class CategoriaBajaSqlServerTests
{
    private const int TimeoutBloqueoMs = 1500;
    private const int ErrorTimeoutDeBloqueo = 1222;

    private static string DescripcionUnica() => $"ITEST-{Guid.NewGuid():N}"[..20];

    private static async Task<bool> EstadoAsync(AppDbContext db, int idCategoria) =>
        await db.Categorias.AsNoTracking().Where(c => c.IdCategoria == idCategoria)
            .Select(c => c.Estado).SingleAsync();

    [FactSqlServer]
    public async Task Baja_ConConteoConfirmado_DaDeBajaYRevierteSinResiduos()
    {
        string descripcion = DescripcionUnica();
        var fabrica = await FabricaSqlServerTransaccional.CrearAsync();
        int idCategoria;

        await using (fabrica)
        {
            var negocio = new CategoriaNegocio(fabrica);
            var alta = await negocio.CrearCategoriaAsync(new CrearCategoriaDto { Descripcion = descripcion });
            Assert.Equal(ResultadoCreacionCategoria.Exito, alta.Resultado);
            idCategoria = alta.Categoria!.IdCategoria;
            await fabrica.InsertarProductoAsync(idCategoria, activo: true);

            var previa = await negocio.PrevisualizarBajaCategoriaAsync(idCategoria);
            Assert.Equal(ResultadoPrevisualizacionBaja.Disponible, previa.Resultado);
            Assert.Equal(1, previa.ProductosActivos);

            var baja = await negocio.DarBajaCategoriaAsync(idCategoria, previa.ProductosActivos);

            Assert.Equal(ResultadoBajaCategoria.Exito, baja.Resultado);
            await using var lectura = fabrica.CreateDbContext();
            Assert.False(await EstadoAsync(lectura, idCategoria));
        }

        // (c) Tras el rollback de la fábrica no queda rastro de la categoría.
        await using var verificacion = BaseSqlServerPruebas.CrearFabrica().CreateDbContext();
        Assert.False(await verificacion.Categorias.AsNoTracking().AnyAsync(c => c.Descripcion == descripcion));
    }

    [FactSqlServer]
    public async Task Baja_ConConteoViejo_RequiereConfirmacionYNoEscribe()
    {
        await using var fabrica = await FabricaSqlServerTransaccional.CrearAsync();
        var negocio = new CategoriaNegocio(fabrica);
        var alta = await negocio.CrearCategoriaAsync(new CrearCategoriaDto { Descripcion = DescripcionUnica() });
        int idCategoria = alta.Categoria!.IdCategoria;
        await fabrica.InsertarProductoAsync(idCategoria, activo: true);
        var previa = await negocio.PrevisualizarBajaCategoriaAsync(idCategoria);
        Assert.Equal(1, previa.ProductosActivos);

        await fabrica.InsertarProductoAsync(idCategoria, activo: true);

        var baja = await negocio.DarBajaCategoriaAsync(idCategoria, previa.ProductosActivos);

        Assert.Equal(ResultadoBajaCategoria.ConfirmacionRequerida, baja.Resultado);
        Assert.Equal(2, baja.ProductosActivosAfectados);
        await using var lectura = fabrica.CreateDbContext();
        Assert.True(await EstadoAsync(lectura, idCategoria));
    }

    /// <summary>
    /// Protección de concurrencia (D-3): con la baja en curso (recuento ya hecho,
    /// UPDATE por ejecutarse) otra conexión real intenta insertar un producto
    /// activo en la categoría; debe quedar bloqueada (timeout de bloqueo 1222) y no
    /// puede colarse entre el recuento y el UPDATE. La baja usa su propia
    /// transacción Serializable, que el interceptor revierte al confirmar para no
    /// dejar residuos (la categoría usada es una existente y su Estado no cambia).
    /// </summary>
    [FactSqlServer]
    public async Task Baja_ConInsercionConcurrenteDeProductoActivo_LaInsercionQuedaBloqueada()
    {
        int idCategoria = await ObtenerCategoriaActivaExistenteAsync();
        var negocioLectura = new CategoriaNegocio(BaseSqlServerPruebas.CrearFabrica());
        int activos = (await negocioLectura.PrevisualizarBajaCategoriaAsync(idCategoria)).ProductosActivos;

        var competidor = new InsercionConcurrente(idCategoria);
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable(BaseSqlServerPruebas.VariableDeEntorno))
            .AddInterceptors(competidor, new RevertirAlConfirmar())
            .Options;
        var negocio = new CategoriaNegocio(new FabricaConOpciones(opciones));

        await Assert.ThrowsAsync<RollbackDeliberadoException>(
            () => negocio.DarBajaCategoriaAsync(idCategoria, activos));

        Assert.True(competidor.SeEjecuto, "El hook previo al UPDATE no se ejecutó: la prueba no demuestra nada.");
        Assert.Equal(ErrorTimeoutDeBloqueo, competidor.NumeroDeErrorSql);
        Assert.False(competidor.InsercionExitosa, "La inserción concurrente no fue bloqueada.");

        // Sin residuos: la categoría sigue activa y su conteo no cambió.
        Assert.Equal(activos, (await negocioLectura.PrevisualizarBajaCategoriaAsync(idCategoria)).ProductosActivos);
        await using var verificacion = BaseSqlServerPruebas.CrearFabrica().CreateDbContext();
        Assert.True(await EstadoAsync(verificacion, idCategoria));
    }

    /// <summary>
    /// Control de la sonda: con aislamiento READ COMMITTED (sin la protección) la
    /// misma inserción concurrente tras el recuento NO se bloquea. Demuestra que la
    /// prueba anterior discrimina. Todo se revierte.
    /// </summary>
    [FactSqlServer]
    public async Task Control_SinSerializable_LaInsercionConcurrenteNoSeBloquea()
    {
        int idCategoria = await ObtenerCategoriaActivaExistenteAsync();
        string cadena = Environment.GetEnvironmentVariable(BaseSqlServerPruebas.VariableDeEntorno)!;

        await using var conexion = new SqlConnection(cadena);
        await conexion.OpenAsync();
        await using SqlTransaction transaccion = conexion.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
        await using (var conteo = new SqlCommand(
            "SELECT COUNT(*) FROM PRODUCTO WHERE id_categoria = @c AND estado = 1", conexion, transaccion))
        {
            conteo.Parameters.AddWithValue("@c", idCategoria);
            await conteo.ExecuteScalarAsync();
        }

        var competidor = new InsercionConcurrente(idCategoria);
        await competidor.EjecutarAsync();

        Assert.True(competidor.InsercionExitosa, "Sin Serializable la inserción debería poder colarse.");
        await transaccion.RollbackAsync();
    }

    private static async Task<int> ObtenerCategoriaActivaExistenteAsync()
    {
        await using var db = BaseSqlServerPruebas.CrearFabrica().CreateDbContext();
        int? id = await db.Categorias.AsNoTracking().Where(c => c.Estado)
            .OrderBy(c => c.IdCategoria).Select(c => (int?)c.IdCategoria).FirstOrDefaultAsync();
        Assert.True(id.HasValue, "La BD de pruebas no tiene ninguna categoría activa para la prueba.");
        return id!.Value;
    }

    private sealed class RollbackDeliberadoException : Exception
    {
    }

    private sealed class FabricaConOpciones : IDbContextFactory<AppDbContext>
    {
        private readonly DbContextOptions<AppDbContext> _opciones;

        public FabricaConOpciones(DbContextOptions<AppDbContext> opciones) => _opciones = opciones;

        public AppDbContext CreateDbContext() => new(_opciones);
    }

    /// <summary>Inserta un producto activo desde OTRA conexión con un timeout de bloqueo corto; siempre revierte.</summary>
    private sealed class InsercionConcurrente : SaveChangesInterceptor
    {
        private readonly int _idCategoria;

        public InsercionConcurrente(int idCategoria) => _idCategoria = idCategoria;

        public bool SeEjecuto { get; private set; }
        public bool InsercionExitosa { get; private set; }
        public int? NumeroDeErrorSql { get; private set; }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            await EjecutarAsync();
            return result;
        }

        public async Task EjecutarAsync()
        {
            SeEjecuto = true;
            await using var conexion = new SqlConnection(
                Environment.GetEnvironmentVariable(BaseSqlServerPruebas.VariableDeEntorno));
            await conexion.OpenAsync();

            await using (var timeout = new SqlCommand($"SET LOCK_TIMEOUT {TimeoutBloqueoMs}", conexion))
            {
                await timeout.ExecuteNonQueryAsync();
            }

            await using SqlTransaction transaccion = conexion.BeginTransaction();

            try
            {
                await using SqlCommand insert = ProductoSql.CrearInsert(conexion, transaccion, _idCategoria, activo: true);
                await insert.ExecuteNonQueryAsync();
                InsercionExitosa = true;
            }
            catch (SqlException ex)
            {
                NumeroDeErrorSql = ex.Number;
            }
            finally
            {
                await transaccion.RollbackAsync();
            }
        }
    }

    /// <summary>Provoca el rollback de la transacción propia de la baja justo antes de confirmarla.</summary>
    private sealed class RevertirAlConfirmar : DbTransactionInterceptor
    {
        public override ValueTask<InterceptionResult> TransactionCommittingAsync(
            DbTransaction transaction, TransactionEventData eventData, InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            throw new RollbackDeliberadoException();
        }
    }
}
