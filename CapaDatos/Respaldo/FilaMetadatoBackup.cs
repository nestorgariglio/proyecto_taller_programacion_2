namespace CapaDatos.Respaldo;

/// <summary>
/// Fila (columnas utilizadas) devuelta por <c>RESTORE HEADERONLY</c>. Tipo sin
/// clave para <c>SqlQueryRaw</c>: NO se registra en el modelo de
/// <see cref="AppDbContext"/> y no genera tablas ni migraciones.
/// </summary>
public sealed class FilaMetadatoBackup
{
    /// <summary>Base de datos de origen del backup.</summary>
    public string? DatabaseName { get; set; }

    /// <summary>Tipo de backup (1 = base de datos completa).</summary>
    public byte BackupType { get; set; }

    /// <summary>Fecha y hora de fin del backup (hora del servidor).</summary>
    public DateTime? BackupFinishDate { get; set; }

    /// <summary>Identificador único del conjunto de backup.</summary>
    public Guid? BackupSetGUID { get; set; }

    /// <summary>Nombre del conjunto de backup.</summary>
    public string? BackupName { get; set; }

    /// <summary>Indica si el backup contiene sumas de verificación.</summary>
    public bool HasBackupChecksums { get; set; }
}

/// <summary>
/// Fila de <c>RESTORE FILELISTONLY</c> o de <c>sys.master_files</c> (nombre lógico,
/// ruta física y tipo: D = datos, L = log). Tipo sin clave fuera del modelo.
/// </summary>
public sealed class FilaArchivoBackup
{
    public string LogicalName { get; set; } = string.Empty;

    public string PhysicalName { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}

/// <summary>Estado de la base objetivo leído de <c>sys.databases</c>.</summary>
public sealed class FilaEstadoBaseDatos
{
    public string Estado { get; set; } = string.Empty;

    public string AccesoUsuarios { get; set; } = string.Empty;

    public bool EstadisticasAsincronas { get; set; }
}
