using CapaDatos;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CapaNegocio.IntegrationTests;

/// <summary>
/// Acceso a la BD de pruebas real. La cadena se toma de la variable de entorno
/// <c>G70_TEST_CONNECTION</c> (nunca se imprime ni se versiona) y solo se acepta
/// si apunta a <c>DB_SISTEMA_VENTA_TEST</c>.
/// </summary>
internal static class BaseSqlServerPruebas
{
    public const string VariableDeEntorno = "G70_TEST_CONNECTION";
    public const string BasePermitida = "DB_SISTEMA_VENTA_TEST";

    /// <summary>Motivo por el que no se puede ejecutar, o <c>null</c> si está disponible.</summary>
    public static string? MotivoDeOmision()
    {
        const string motivo =
            "G70_TEST_CONNECTION no definida o no apunta a DB_SISTEMA_VENTA_TEST: prueba NO ejecutada";

        string? cadena = Environment.GetEnvironmentVariable(VariableDeEntorno);

        if (string.IsNullOrWhiteSpace(cadena))
        {
            return motivo;
        }

        try
        {
            var builder = new SqlConnectionStringBuilder(cadena);
            return string.Equals(builder.InitialCatalog, BasePermitida, StringComparison.OrdinalIgnoreCase)
                ? null
                : motivo;
        }
        catch (ArgumentException)
        {
            return motivo;
        }
    }

    public static IDbContextFactory<AppDbContext> CrearFabrica()
    {
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable(VariableDeEntorno))
            .Options;
        return new FabricaSqlServer(opciones);
    }

    private sealed class FabricaSqlServer : IDbContextFactory<AppDbContext>
    {
        private readonly DbContextOptions<AppDbContext> _opciones;

        public FabricaSqlServer(DbContextOptions<AppDbContext> opciones) => _opciones = opciones;

        public AppDbContext CreateDbContext() => new(_opciones);
    }
}

/// <summary>Fact que se omite si la BD de pruebas no está configurada.</summary>
internal sealed class FactSqlServerAttribute : FactAttribute
{
    public FactSqlServerAttribute()
    {
        Skip = BaseSqlServerPruebas.MotivoDeOmision();
    }
}
