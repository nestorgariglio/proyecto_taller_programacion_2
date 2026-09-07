using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CapaDatos;

/// <summary>
/// Crea <see cref="AppDbContext"/> para las operaciones de diseño de Entity Framework Core.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string ConnectionEnvironmentVariable = "ConnectionStrings__CadenaSQL";

    /// <summary>
    /// Construye un contexto usando la cadena de conexión configurada para el entorno.
    /// </summary>
    /// <param name="args">Argumentos recibidos por la herramienta de Entity Framework Core.</param>
    /// <returns>Contexto configurado para SQL Server.</returns>
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = ReadConnectionStringFromAppSettings();
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"No se encontró la cadena 'CadenaSQL'. Definí la variable de entorno " +
                $"{ConnectionEnvironmentVariable} o verificá CapaPresentacion/appsettings.json.");
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }

    private static string? ReadConnectionStringFromAppSettings()
    {
        foreach (var directory in ParentDirectories(AppContext.BaseDirectory))
        {
            var path = Path.Combine(directory, "CapaPresentacion", "appsettings.json");
            if (!File.Exists(path))
            {
                continue;
            }

            try
            {
                using var document = JsonDocument.Parse(File.ReadAllText(path));
                if (document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings) &&
                    connectionStrings.TryGetProperty("CadenaSQL", out var value))
                {
                    return value.GetString();
                }
            }
            catch (JsonException exception)
            {
                throw new InvalidOperationException($"No se pudo leer {path}: JSON inválido.", exception);
            }
        }

        return null;
    }

    private static IEnumerable<string> ParentDirectories(string startingDirectory)
    {
        var directory = new DirectoryInfo(startingDirectory);
        while (directory is not null)
        {
            yield return directory.FullName;
            directory = directory.Parent;
        }
    }
}
