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

/// <summary>Resultado de abrir una sesión con bloqueo de aplicación.</summary>
public sealed record AperturaSesionExclusiva(ISesionRespaldo? Sesion, int CodigoApplock);

/// <summary>
/// Excepción producida durante operaciones de respaldo en la base de datos.
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
/// Sesión de trabajo para ejecutar operaciones de respaldo y restauración.
/// </summary>
public interface ISesionRespaldo : IAsyncDisposable
{
    Task EjecutarBackupAsync(string rutaArchivo, string nombreBackup, string descripcion, CancellationToken ct);

    Task VerificarBackupAsync(string rutaArchivo, CancellationToken ct);

    Task<IReadOnlyList<FilaMetadatoBackup>> LeerCabecerasAsync(string rutaArchivo, CancellationToken ct);

    Task<IReadOnlyList<FilaArchivoBackup>> LeerListaArchivosAsync(string rutaArchivo, CancellationToken ct);

    Task<IReadOnlyList<FilaArchivoBackup>> LeerArchivosActualesAsync(CancellationToken ct);

    Task<EstadoBaseDatos?> LeerEstadoAsync(CancellationToken ct);

    Task EstablecerUsuarioUnicoAsync(CancellationToken ct);

    Task RestaurarAsync(string rutaArchivo, IReadOnlyList<MapeoArchivo> mapeo, CancellationToken ct);

    Task<EstadoBaseDatos?> RecuperarMultiUsuarioAsync();
}

/// <summary>
/// Contrato para el repositorio de operaciones de respaldo y restauración.
/// </summary>
public interface IRespaldoRepositorio
{
    string NombreBaseDatos { get; }

    Task<ISesionRespaldo> AbrirSesionAsync(CancellationToken ct);

    Task<AperturaSesionExclusiva> AbrirSesionExclusivaAsync(CancellationToken ct);
}
