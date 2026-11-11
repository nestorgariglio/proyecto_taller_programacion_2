using Xunit;

namespace CapaNegocio.IntegrationTests;

public sealed class FactIntegracionAttribute : FactAttribute
{
    public const string VariableActivacion = "G70_INTEGRACION";
    public const string VariableConexion = "G70_TEST_CONNECTION";

    public FactIntegracionAttribute()
    {
        if (Environment.GetEnvironmentVariable(VariableActivacion) != "1")
        {
            Skip = "Integración desactivada: defina G70_INTEGRACION=1.";
        }
        else if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(VariableConexion)))
        {
            Skip = "Integración sin destino: defina G70_TEST_CONNECTION.";
        }
    }
}
