using Microsoft.Data.SqlClient;

namespace CapaNegocio.IntegrationTests;

/// Guarda de destino: solo localhost\SQLEXPRESS / DB_SISTEMA_VENTA_TEST con autenticación integrada.
/// Previene errores accidentales; NO es una frontera de seguridad. Nunca imprime la cadena.
internal static class GuardaConexion
{
    public const string ServidorPermitido = @"localhost\SQLEXPRESS";
    public const string BasePermitida = "DB_SISTEMA_VENTA_TEST";

    public static string ObtenerCadenaValidada()
    {
        string? cadena = Environment.GetEnvironmentVariable(FactIntegracionAttribute.VariableConexion);
        if (string.IsNullOrWhiteSpace(cadena))
        {
            throw new InvalidOperationException("Falta la variable G70_TEST_CONNECTION.");
        }

        SqlConnectionStringBuilder constructor;
        try
        {
            constructor = new SqlConnectionStringBuilder(cadena);
        }
        catch (Exception)
        {
            throw new InvalidOperationException("G70_TEST_CONNECTION no es una cadena de conexión válida.");
        }

        if (!string.Equals(constructor.DataSource, ServidorPermitido, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Destino rechazado: el servidor debe ser exactamente {ServidorPermitido}.");
        }

        if (!string.Equals(constructor.InitialCatalog, BasePermitida, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Destino rechazado: la base debe ser exactamente {BasePermitida}.");
        }

        if (!string.IsNullOrEmpty(constructor.AttachDBFilename))
        {
            throw new InvalidOperationException("Destino rechazado: AttachDBFilename no está permitido.");
        }

        if (!constructor.IntegratedSecurity || !string.IsNullOrEmpty(constructor.UserID) || !string.IsNullOrEmpty(constructor.Password))
        {
            throw new InvalidOperationException("Destino rechazado: solo se admite Integrated Security=true, sin usuario ni clave en la cadena.");
        }

        constructor.ConnectTimeout = 15;
        return constructor.ConnectionString;
    }

    /// Comprueba por SQL que la base realmente conectada es la permitida, antes de cualquier operación.
    public static async Task VerificarBaseConectadaAsync(string cadena, CancellationToken ct)
    {
        await using var conexion = new SqlConnection(cadena);
        await conexion.OpenAsync(ct);
        await using var comando = new SqlCommand("SELECT DB_NAME(), @@SERVERNAME;", conexion) { CommandTimeout = 30 };
        string? baseActual, servidorActual;
        await using (SqlDataReader lector = await comando.ExecuteReaderAsync(ct))
        {
            await lector.ReadAsync(ct);
            baseActual = lector.GetString(0);
            servidorActual = lector.GetString(1);
        }

        if (!string.Equals(baseActual, BasePermitida, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Destino rechazado: DB_NAME() no es exactamente {BasePermitida}.");
        }

        // @@SERVERNAME devuelve el nombre real de la máquina (no el alias "localhost").
        string esperado = Environment.MachineName + @"\SQLEXPRESS";
        if (!string.Equals(servidorActual, esperado, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(@"Destino rechazado: @@SERVERNAME no corresponde a la instancia local SQLEXPRESS.");
        }
    }
}
