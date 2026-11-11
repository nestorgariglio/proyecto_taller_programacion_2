using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Respaldos;

/// <summary>Identificación y estado de un archivo de respaldo.</summary>
public sealed record HuellaRespaldo(
    Guid BackupSetGuid,
    DateTime FechaBackup,
    long TamanoBytes,
    DateTime FechaModificacionArchivoUtc);

/// <summary>Respaldo válido listado para su restauración.</summary>
public sealed class RespaldoDisponibleDto
{
    public string NombreArchivo { get; set; } = string.Empty;

    public string RutaArchivo { get; set; } = string.Empty;

    /// <summary>Fecha y hora de fin del backup (BackupFinishDate real).</summary>
    public DateTime FechaBackup { get; set; }

    /// <summary>«Manual» o «Automático».</summary>
    public string Tipo { get; set; } = "Manual";

    public long TamanoBytes { get; set; }

    public string TamanoLegible => TamanoBytes >= 1024L * 1024L
        ? $"{TamanoBytes / (1024d * 1024d):N1} MB"
        : $"{Math.Max(1, TamanoBytes / 1024):N0} KB";

    public HuellaRespaldo Huella { get; set; } = new(Guid.Empty, DateTime.MinValue, 0, DateTime.MinValue);
}

public class RespuestaListadoRespaldos
{
    public ResultadoRespaldo Resultado { get; set; }

    public string Mensaje { get; set; } = string.Empty;

    public IReadOnlyList<RespaldoDisponibleDto> Respaldos { get; set; } = Array.Empty<RespaldoDisponibleDto>();

    /// <summary>Archivos .bak omitidos por ilegibles o ajenos a la base.</summary>
    public int ArchivosNoLegibles { get; set; }
}

public class ValidacionRespaldoDto
{
    public ResultadoRespaldo Resultado { get; set; }

    public string Mensaje { get; set; } = string.Empty;

    public RespaldoDisponibleDto? Respaldo { get; set; }

    public HuellaRespaldo? Huella => Respaldo?.Huella;
}

public class RespuestaGeneracionRespaldo
{
    public ResultadoRespaldo Resultado { get; set; }

    public string Mensaje { get; set; } = string.Empty;

    public string? RutaArchivo { get; set; }
}

public class RespuestaRestauracion
{
    public ResultadoRespaldo Resultado { get; set; }

    public string Mensaje { get; set; } = string.Empty;

    /// <summary>Ruta del backup automático de seguridad, si llegó a crearse.</summary>
    public string? RutaBackupSeguridad { get; set; }

    /// <summary>Estado final de acceso de la base (p. ej. «ONLINE / MULTI_USER»), si pudo leerse.</summary>
    public string? EstadoAccesoFinal { get; set; }
}
