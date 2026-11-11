namespace CapaDatos.Respaldo;

/// <summary>Resultado de ubicar la carpeta de respaldos.</summary>
public sealed record ResultadoRutaRespaldo(bool Disponible, string? Carpeta, string Mensaje);

/// <summary>Ubica la carpeta donde se guardan los respaldos.</summary>
public interface IProveedorRutaRespaldo
{
    ResultadoRutaRespaldo ObtenerCarpeta();
}

/// <summary>
/// Provee la ruta para almacenar respaldos según configuración o raíz de la solución.
/// </summary>
public sealed class ProveedorRutaRespaldo : IProveedorRutaRespaldo
{
    public const string ArchivoRaiz = "SistemaVentaStock.slnx";
    public const string NombreCarpeta = "backup_restore";

    public const string MensajeSinRaiz =
        "No se pudo determinar la carpeta de respaldos porque esta instalación no incluye la solución del sistema (SistemaVentaStock.slnx) ni una ruta configurada. La función de respaldo no está disponible.";

    public const string MensajeRutaConfiguradaInvalida =
        "No se pudo acceder o crear la carpeta de respaldos configurada. Verifique la ruta y los permisos del sistema.";

    private readonly string _directorioInicio;
    private readonly string? _rutaConfigurada;

    public ProveedorRutaRespaldo(string? directorioInicio = null, string? rutaConfigurada = null)
    {
        _directorioInicio = directorioInicio ?? AppContext.BaseDirectory;
        _rutaConfigurada = string.IsNullOrWhiteSpace(rutaConfigurada) ? null : rutaConfigurada.Trim();
    }

    /// <inheritdoc />
    public ResultadoRutaRespaldo ObtenerCarpeta()
    {
        if (_rutaConfigurada is not null)
        {
            try
            {
                string rutaAbsoluta = Path.GetFullPath(_rutaConfigurada);
                Directory.CreateDirectory(rutaAbsoluta);
                return new ResultadoRutaRespaldo(true, rutaAbsoluta, string.Empty);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                return new ResultadoRutaRespaldo(false, null, MensajeRutaConfiguradaInvalida);
            }
        }

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
