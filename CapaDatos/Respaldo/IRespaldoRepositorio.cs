namespace CapaDatos.Respaldo;

/// <summary>Estado de la base objetivo (de <c>sys.databases</c>).</summary>
public sealed record EstadoBaseDatos(string Estado, string AccesoUsuarios, bool EstadisticasAsincronas)
{
    /// <summary>Indica ONLINE y MULTI_USER.</summary>
    public bool EstaOperativa =>
        string.Equals(Estado, "ONLINE", StringComparison.OrdinalIgnoreCase)
        && string.Equals(AccesoUsuarios, "MULTI_USER", StringComparison.OrdinalIgnoreCase);
}

/// <summary>Correspondencia nombre lógico a ruta física para <c>RESTORE ... MOVE</c>.</summary>
public sealed record MapeoArchivo(string NombreLogico, string RutaFisica);

/// <summary>Resultado de abrir una sesión exclusiva (sp_getapplock).</summary>
/// <param name="Sesion">Sesión con el lock tomado; <see langword="null"/> si no se obtuvo.</param>
/// <param name="CodigoApplock">Código de <c>sp_getapplock</c> (0/1 concedido, -1 ocupado, menor que -1 error).</param>
public sealed record AperturaSesionExclusiva(ISesionRespaldo? Sesion, int CodigoApplock);

/// <summary>
/// Error de base de datos ya traducido: solo expone el número de error de SQL
/// Server, nunca el texto crudo, la cadena de conexión ni el servidor.
/// </summary>
public sealed class RespaldoBdException : Exception
{
    public RespaldoBdException(int numeroError, Exception interna)
        : base("Error de base de datos durante la operación de respaldo.", interna)
    {
        NumeroError = numeroError;
    }

    /// <summary>Número de error de SQL Server.</summary>
    public int NumeroError { get; }
}

/// <summary>
/// Sesión de respaldo: UNA conexión abierta al catálogo <c>master</c> que se
/// mantiene durante toda la operación (así, un <c>sp_getapplock</c> con
/// <c>@LockOwner='Session'</c> vive tanto como la sesión). Todas las operaciones
/// usan un tiempo de espera largo propio de esta sesión, sin alterar los valores
/// globales de la aplicación.
/// </summary>
public interface ISesionRespaldo : IAsyncDisposable
{
    /// <summary>Genera <c>BACKUP DATABASE ... WITH CHECKSUM, INIT</c>.</summary>
    Task EjecutarBackupAsync(string rutaArchivo, string nombreBackup, string descripcion, CancellationToken ct);

    /// <summary><c>RESTORE VERIFYONLY ... WITH CHECKSUM</c>.</summary>
    Task VerificarBackupAsync(string rutaArchivo, CancellationToken ct);

    /// <summary><c>RESTORE HEADERONLY</c>.</summary>
    Task<IReadOnlyList<FilaMetadatoBackup>> LeerCabecerasAsync(string rutaArchivo, CancellationToken ct);

    /// <summary><c>RESTORE FILELISTONLY</c>.</summary>
    Task<IReadOnlyList<FilaArchivoBackup>> LeerListaArchivosAsync(string rutaArchivo, CancellationToken ct);

    /// <summary>Archivos actuales de la base objetivo (<c>sys.master_files</c>).</summary>
    Task<IReadOnlyList<FilaArchivoBackup>> LeerArchivosActualesAsync(CancellationToken ct);

    /// <summary>Estado actual de la base objetivo; <see langword="null"/> si no existe.</summary>
    Task<EstadoBaseDatos?> LeerEstadoAsync(CancellationToken ct);

    /// <summary><c>ALTER DATABASE SET SINGLE_USER WITH ROLLBACK IMMEDIATE</c>.</summary>
    Task EstablecerUsuarioUnicoAsync(CancellationToken ct);

    /// <summary><c>RESTORE DATABASE ... WITH REPLACE, CHECKSUM, MOVE ...</c> y limpieza de pools.</summary>
    Task RestaurarAsync(string rutaArchivo, IReadOnlyList<MapeoArchivo> mapeo, CancellationToken ct);

    /// <summary>
    /// Intenta <c>SET MULTI_USER</c> (con una conexión nueva si la sesión se rompió)
    /// y devuelve el estado verificado, o <see langword="null"/> si no pudo leerse.
    /// </summary>
    Task<EstadoBaseDatos?> RecuperarMultiUsuarioAsync();
}

/// <summary>
/// Acceso a datos del respaldo. Todo acceso a la BD se hace con
/// <see cref="AppDbContext"/> creado contra el catálogo <c>master</c>.
/// </summary>
/// <remarks>
/// Con autenticación integrada, el usuario de Windows que ejecuta G70 debe tener
/// permisos SQL propios (BACKUP DATABASE, ALTER sobre la base, CREATE DATABASE /
/// dbcreator para RESTORE WITH REPLACE y acceso a master); los perfiles de G70
/// no otorgan permisos SQL.
/// Excepción al principio 5 de la constitución: BACKUP y RESTORE no admiten
/// transacción de usuario, por lo que no se usan transacciones explícitas.
/// </remarks>
public interface IRespaldoRepositorio
{
    /// <summary>Nombre de la base objetivo (catálogo configurado).</summary>
    string NombreBaseDatos { get; }

    /// <summary>Abre una sesión sin lock (lecturas y validaciones).</summary>
    Task<ISesionRespaldo> AbrirSesionAsync(CancellationToken ct);

    /// <summary>
    /// Abre una sesión y toma <c>sys.sp_getapplock</c> exclusivo (Session, timeout 0)
    /// con un recurso fijo que incluye la base objetivo. El lock coordina entre
    /// instancias de G70; SINGLE_USER no sustituye esa coordinación.
    /// </summary>
    Task<AperturaSesionExclusiva> AbrirSesionExclusivaAsync(CancellationToken ct);
}
