using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CapaDatos.Respaldo;

/// <summary>
/// Implementación de <see cref="IRespaldoRepositorio"/> sobre <see cref="AppDbContext"/>
/// con opciones propias contra <c>master</c> (solo cambia el Initial Catalog de la
/// cadena configurada). No usa SqlConnection/SqlCommand directos.
/// </summary>
/// <remarks>
/// Tiempo de espera: cada sesión crea su propio contexto con un
/// <c>CommandTimeout</c> de 2 horas (<see cref="TiempoEsperaOperacion"/>), porque
/// BACKUP/RESTORE y sus verificaciones pueden tardar mucho más que el valor por
/// defecto de 30 s. El valor aplica solo a estos contextos; no modifica la
/// configuración global de <c>UseSqlServer</c> de la aplicación.
/// Autenticación integrada: el usuario de Windows debe tener permisos SQL
/// (BACKUP DATABASE, ALTER sobre la base, dbcreator para RESTORE WITH REPLACE,
/// acceso a master); los perfiles de G70 no otorgan permisos SQL.
/// Excepción al principio 5: BACKUP/RESTORE no admiten transacción de usuario.
/// El semáforo del negocio protege dentro del proceso y sp_getapplock entre
/// instancias; SINGLE_USER no sustituye esa coordinación.
/// </remarks>
public sealed class RespaldoRepositorio : IRespaldoRepositorio
{
    /// <summary>Tiempo de espera de los comandos de respaldo (documentado arriba).</summary>
    public static readonly TimeSpan TiempoEsperaOperacion = TimeSpan.FromHours(2);

    private const string PrefijoRecursoLock = "G70_RESPALDO_";

    private readonly string _cadenaMaster;

    /// <param name="cadenaConexion">Cadena configurada (<c>CadenaSQL</c>); se usa su catálogo como base objetivo.</param>
    public RespaldoRepositorio(string cadenaConexion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cadenaConexion);

        var constructor = new SqlConnectionStringBuilder(cadenaConexion);
        if (string.IsNullOrWhiteSpace(constructor.InitialCatalog)
            || constructor.InitialCatalog.Contains('{') || constructor.InitialCatalog.Contains('}'))
        {
            throw new ArgumentException("La cadena de conexión no define una base de datos válida.", nameof(cadenaConexion));
        }

        NombreBaseDatos = constructor.InitialCatalog;
        constructor.InitialCatalog = "master";
        _cadenaMaster = constructor.ConnectionString;
    }

    /// <inheritdoc />
    public string NombreBaseDatos { get; }

    /// <inheritdoc />
    public async Task<ISesionRespaldo> AbrirSesionAsync(CancellationToken ct)
        => await SesionRespaldo.CrearAsync(_cadenaMaster, NombreBaseDatos, ct);

    /// <inheritdoc />
    public async Task<AperturaSesionExclusiva> AbrirSesionExclusivaAsync(CancellationToken ct)
    {
        SesionRespaldo sesion = await SesionRespaldo.CrearAsync(_cadenaMaster, NombreBaseDatos, ct);
        try
        {
            int codigo = await sesion.TomarLockAsync(PrefijoRecursoLock + NombreBaseDatos, ct);
            if (codigo < 0)
            {
                await sesion.DisposeAsync();
                return new AperturaSesionExclusiva(null, codigo);
            }

            return new AperturaSesionExclusiva(sesion, codigo);
        }
        catch
        {
            await sesion.DisposeAsync();
            throw;
        }
    }

    private static AppDbContext CrearContexto(string cadenaMaster)
    {
        DbContextOptions<AppDbContext> opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(cadenaMaster, o => o.CommandTimeout((int)TiempoEsperaOperacion.TotalSeconds))
            .Options;
        return new AppDbContext(opciones);
    }

    private static string Corchetes(string nombre) => "[" + nombre.Replace("]", "]]") + "]";

    private sealed class SesionRespaldo : ISesionRespaldo
    {
        private readonly string _cadenaMaster;
        private readonly string _baseDatos;
        private readonly AppDbContext _db;
        private string? _recursoLock;

        private SesionRespaldo(string cadenaMaster, string baseDatos, AppDbContext db)
        {
            _cadenaMaster = cadenaMaster;
            _baseDatos = baseDatos;
            _db = db;
        }

        public static async Task<SesionRespaldo> CrearAsync(string cadenaMaster, string baseDatos, CancellationToken ct)
        {
            AppDbContext db = CrearContexto(cadenaMaster);
            try
            {
                // Una sola conexión abierta durante toda la sesión.
                await Traducir(() => db.Database.OpenConnectionAsync(ct), ct);
                return new SesionRespaldo(cadenaMaster, baseDatos, db);
            }
            catch
            {
                await db.DisposeAsync();
                throw;
            }
        }

        public async Task<int> TomarLockAsync(string recurso, CancellationToken ct)
        {
            List<int> resultado = await Traducir(() => _db.Database.SqlQueryRaw<int>(
                "DECLARE @r int; EXEC @r = sys.sp_getapplock @Resource = {0}, @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = 0; SELECT @r AS [Value];",
                recurso).ToListAsync(ct), ct);

            int codigo = resultado.Count == 1 ? resultado[0] : -999;
            if (codigo >= 0)
            {
                _recursoLock = recurso;
            }

            return codigo;
        }

        public Task EjecutarBackupAsync(string rutaArchivo, string nombreBackup, string descripcion, CancellationToken ct)
            => Traducir(() => _db.Database.ExecuteSqlRawAsync(
                $"BACKUP DATABASE {Corchetes(_baseDatos)} TO DISK = {{0}} WITH CHECKSUM, INIT, NAME = {{1}}, DESCRIPTION = {{2}}",
                new object[] { rutaArchivo, nombreBackup, descripcion }, ct), ct);

        public Task VerificarBackupAsync(string rutaArchivo, CancellationToken ct)
            => Traducir(() => _db.Database.ExecuteSqlRawAsync(
                "RESTORE VERIFYONLY FROM DISK = {0} WITH CHECKSUM", new object[] { rutaArchivo }, ct), ct);

        public async Task<IReadOnlyList<FilaMetadatoBackup>> LeerCabecerasAsync(string rutaArchivo, CancellationToken ct)
            => await Traducir(() => _db.Database.SqlQueryRaw<FilaMetadatoBackup>(
                "RESTORE HEADERONLY FROM DISK = {0}", rutaArchivo).ToListAsync(ct), ct);

        public async Task<IReadOnlyList<FilaArchivoBackup>> LeerListaArchivosAsync(string rutaArchivo, CancellationToken ct)
            => await Traducir(() => _db.Database.SqlQueryRaw<FilaArchivoBackup>(
                "RESTORE FILELISTONLY FROM DISK = {0}", rutaArchivo).ToListAsync(ct), ct);

        public async Task<IReadOnlyList<FilaArchivoBackup>> LeerArchivosActualesAsync(CancellationToken ct)
            => await Traducir(() => _db.Database.SqlQueryRaw<FilaArchivoBackup>(
                "SELECT name AS LogicalName, physical_name AS PhysicalName, CAST(CASE WHEN type = 1 THEN N'L' ELSE N'D' END AS nvarchar(1)) AS [Type] FROM sys.master_files WHERE database_id = DB_ID({0})",
                _baseDatos).ToListAsync(ct), ct);

        public async Task<EstadoBaseDatos?> LeerEstadoAsync(CancellationToken ct)
            => await LeerEstadoAsync(_db, ct);

        private async Task<EstadoBaseDatos?> LeerEstadoAsync(AppDbContext db, CancellationToken ct)
        {
            List<FilaEstadoBaseDatos> filas = await Traducir(() => db.Database.SqlQueryRaw<FilaEstadoBaseDatos>(
                "SELECT state_desc AS Estado, user_access_desc AS AccesoUsuarios, is_auto_update_stats_async_on AS EstadisticasAsincronas FROM sys.databases WHERE name = {0}",
                _baseDatos).ToListAsync(ct), ct);

            return filas.Count == 0
                ? null
                : new EstadoBaseDatos(filas[0].Estado, filas[0].AccesoUsuarios, filas[0].EstadisticasAsincronas);
        }

        public Task EstablecerUsuarioUnicoAsync(CancellationToken ct)
            => Traducir(() => _db.Database.ExecuteSqlRawAsync(
                $"ALTER DATABASE {Corchetes(_baseDatos)} SET SINGLE_USER WITH ROLLBACK IMMEDIATE", ct), ct);

        public async Task RestaurarAsync(string rutaArchivo, IReadOnlyList<MapeoArchivo> mapeo, CancellationToken ct)
        {
            var parametros = new List<object> { rutaArchivo };
            var moves = new System.Text.StringBuilder();
            foreach (MapeoArchivo m in mapeo)
            {
                moves.Append(", MOVE {").Append(parametros.Count).Append("} TO {").Append(parametros.Count + 1).Append('}');
                parametros.Add(m.NombreLogico);
                parametros.Add(m.RutaFisica);
            }

            try
            {
                await Traducir(() => _db.Database.ExecuteSqlRawAsync(
                    $"RESTORE DATABASE {Corchetes(_baseDatos)} FROM DISK = {{0}} WITH REPLACE, CHECKSUM{moves}",
                    parametros, ct), ct);
            }
            finally
            {
                // Las conexiones agrupadas pudieron quedar apuntando a una base reemplazada.
                SqlConnection.ClearAllPools();
            }
        }

        public async Task<EstadoBaseDatos?> RecuperarMultiUsuarioAsync()
        {
            string sql = $"ALTER DATABASE {Corchetes(_baseDatos)} SET MULTI_USER";

            try
            {
                await Traducir(() => _db.Database.ExecuteSqlRawAsync(sql), CancellationToken.None);
                return await LeerEstadoAsync(_db, CancellationToken.None);
            }
            catch (Exception)
            {
                // La sesión pudo romperse: se reintenta con una conexión nueva.
            }

            try
            {
                await using AppDbContext nuevo = CrearContexto(_cadenaMaster);
                await Traducir(() => nuevo.Database.ExecuteSqlRawAsync(sql), CancellationToken.None);
                return await LeerEstadoAsync(nuevo, CancellationToken.None);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_recursoLock is not null)
            {
                string recurso = _recursoLock;
                _recursoLock = null;
                try
                {
                    // Liberación explícita antes de cerrar la conexión.
                    await _db.Database.ExecuteSqlRawAsync(
                        "EXEC sys.sp_releaseapplock @Resource = {0}, @LockOwner = 'Session'", recurso);
                }
                catch (Exception)
                {
                    // Si la sesión está rota, el lock se libera al cerrarse la conexión.
                }
            }

            try
            {
                await _db.Database.CloseConnectionAsync();
            }
            catch (Exception)
            {
                // Sin acción: se descarta el contexto igualmente.
            }

            await _db.DisposeAsync();
        }

        private static async Task Traducir(Func<Task> accion, CancellationToken ct)
        {
            try
            {
                await accion();
            }
            catch (SqlException ex)
            {
                // Cancelación solicitada: el comando ya terminó en el servidor (abortado).
                if (ct.IsCancellationRequested)
                {
                    throw new OperationCanceledException(ct);
                }

                throw new RespaldoBdException(ex.Number, ex);
            }
        }

        private static async Task<T> Traducir<T>(Func<Task<T>> accion, CancellationToken ct)
        {
            try
            {
                return await accion();
            }
            catch (SqlException ex)
            {
                // Cancelación solicitada: el comando ya terminó en el servidor (abortado).
                if (ct.IsCancellationRequested)
                {
                    throw new OperationCanceledException(ct);
                }

                throw new RespaldoBdException(ex.Number, ex);
            }
        }
    }
}
