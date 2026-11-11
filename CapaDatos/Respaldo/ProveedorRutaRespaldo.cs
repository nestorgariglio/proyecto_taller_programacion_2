namespace CapaDatos.Respaldo;

/// <summary>Resultado de ubicar la carpeta de respaldos.</summary>
/// <param name="Disponible">Indica si la carpeta pudo determinarse y existe.</param>
/// <param name="Carpeta">Ruta absoluta de la carpeta cuando está disponible.</param>
/// <param name="Mensaje">Mensaje en español cuando no está disponible.</param>
public sealed record ResultadoRutaRespaldo(bool Disponible, string? Carpeta, string Mensaje);

/// <summary>Ubica la carpeta donde se guardan los respaldos.</summary>
public interface IProveedorRutaRespaldo
{
    /// <summary>Determina (y crea si falta) la carpeta de respaldos.</summary>
    ResultadoRutaRespaldo ObtenerCarpeta();
}

/// <summary>
/// Ubica <c>backup_restore</c> en la raíz de la solución: sube desde el directorio
/// base de la aplicación hasta hallar <c>SistemaVentaStock.slnx</c>. No usa el
/// directorio de trabajo ni ofrece una ubicación alternativa silenciosa. No
/// modifica permisos de Windows (los gestiona el usuario).
/// </summary>
public sealed class ProveedorRutaRespaldo : IProveedorRutaRespaldo
{
    /// <summary>Archivo que identifica la raíz de la solución.</summary>
    public const string ArchivoRaiz = "SistemaVentaStock.slnx";

    /// <summary>Nombre de la carpeta de respaldos dentro de la raíz.</summary>
    public const string NombreCarpeta = "backup_restore";

    public const string MensajeSinRaiz =
        "No se pudo determinar la carpeta de respaldos porque esta instalación no incluye la solución del sistema (SistemaVentaStock.slnx). La función de respaldo no está disponible.";

    private readonly string _directorioInicio;

    /// <param name="directorioInicio">Directorio desde donde se asciende; por defecto el directorio base de la aplicación.</param>
    public ProveedorRutaRespaldo(string? directorioInicio = null)
    {
        _directorioInicio = directorioInicio ?? AppContext.BaseDirectory;
    }

    /// <inheritdoc />
    public ResultadoRutaRespaldo ObtenerCarpeta()
    {
        DirectoryInfo? actual;
        try
        {
            actual = new DirectoryInfo(Path.GetFullPath(_directorioInicio));
        }
        catch (Exception)
        {
            return new ResultadoRutaRespaldo(false, null, MensajeSinRaiz);
        }

        while (actual is not null)
        {
            if (File.Exists(Path.Combine(actual.FullName, ArchivoRaiz)))
            {
                string carpeta = Path.Combine(actual.FullName, NombreCarpeta);
                try
                {
                    Directory.CreateDirectory(carpeta);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    return new ResultadoRutaRespaldo(false, null,
                        "No se pudo crear la carpeta de respaldos. Verifique los permisos de la carpeta de la solución.");
                }

                return new ResultadoRutaRespaldo(true, carpeta, string.Empty);
            }

            actual = actual.Parent;
        }

        return new ResultadoRutaRespaldo(false, null, MensajeSinRaiz);
    }
}
