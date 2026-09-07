namespace CapaNegocio;

/// <summary>
/// Define los módulos disponibles para cada rol del sistema.
/// </summary>
public static class PoliticaAcceso
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> Accesos =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["administrador"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Usuarios", "Productos", "Categorías", "Ventas", "Compras", "Clientes", "Proveedores", "Reportes"
            },
            ["encargado"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Productos", "Categorías", "Proveedores", "Compras", "Reportes"
            },
            ["vendedor"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Ventas", "Clientes"
            }
        };

    /// <summary>
    /// Indica si un rol puede acceder al módulo solicitado.
    /// </summary>
    /// <param name="rol">Descripción del rol del usuario.</param>
    /// <param name="modulo">Nombre del módulo que se desea consultar.</param>
    /// <returns><see langword="true"/> si el rol tiene acceso; de lo contrario, <see langword="false"/>.</returns>
    public static bool TieneAcceso(string? rol, string? modulo)
    {
        return !string.IsNullOrWhiteSpace(rol)
            && !string.IsNullOrWhiteSpace(modulo)
            && Accesos.TryGetValue(rol.Trim(), out var modulos)
            && modulos.Contains(modulo.Trim());
    }
}
