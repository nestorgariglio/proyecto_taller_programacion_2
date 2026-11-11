using System.Text.RegularExpressions;

namespace CapaNegocio;

/// <summary>
/// Comparación de Razón Social solo para advertir posibles duplicados (la Razón Social no es única).
/// La normalización no modifica el texto guardado.
/// </summary>
public static class ComparadorRazonSocialProveedor
{
    private static readonly Regex EspaciosConsecutivos = new(@"\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex FinSociedadAnonima = new(@"(?<![\p{L}\p{N}])S\.?\s?A\.?$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex FinSrl = new(@"(?<![\p{L}\p{N}])S\.?\s?R\.?\s?L\.?$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string Normalizar(string? razonSocial)
    {
        string texto = EspaciosConsecutivos.Replace((razonSocial ?? string.Empty).Trim(), " ").ToUpperInvariant();
        texto = FinSociedadAnonima.Replace(texto, "SA");
        texto = FinSrl.Replace(texto, "SRL");
        return texto;
    }

    public static bool SonEquivalentes(string? a, string? b)
    {
        return string.Equals(Normalizar(a), Normalizar(b), StringComparison.Ordinal);
    }
}
