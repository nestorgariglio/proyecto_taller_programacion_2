namespace CapaNegocio;

/// <summary>
/// Define las reglas de acceso a los módulos y operaciones según el rol del usuario.
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
                "Productos", "Categorías", "Proveedores", "Compras", "Reportes", "Clientes"
            },
            ["vendedor"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Ventas", "Clientes"
            }
        };

    /// <summary>
    /// Determina si un rol tiene permiso para acceder a un módulo determinado.
    /// </summary>
    /// <param name="rol">Rol del usuario.</param>
    /// <param name="modulo">Nombre del módulo a consultar.</param>
    /// <returns>True si tiene acceso; en caso contrario, false.</returns>
    public static bool TieneAcceso(string? rol, string? modulo)
    {
        return !string.IsNullOrWhiteSpace(rol)
            && !string.IsNullOrWhiteSpace(modulo)
            && Accesos.TryGetValue(rol.Trim(), out var modulos)
            && modulos.Contains(modulo.Trim());
    }

    /// <summary>
    /// Determina si un rol tiene autorización para administrar clientes.
    /// </summary>
    /// <param name="rol">Rol del usuario.</param>
    /// <returns>True si el rol es Administrador o Encargado; en caso contrario, false.</returns>
    public static bool PuedeAdministrarClientes(string? rol)
    {
        return !string.IsNullOrWhiteSpace(rol)
            && (string.Equals(rol.Trim(), "administrador", StringComparison.OrdinalIgnoreCase)
                || string.Equals(rol.Trim(), "encargado", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determina si un rol tiene autorización para gestionar productos.
    /// </summary>
    /// <param name="rol">Rol del usuario.</param>
    /// <returns>True si tiene acceso; en caso contrario, false.</returns>
    public static bool PuedeGestionarProductos(string? rol)
    {
        return TieneAcceso(rol, "Productos");
    }

    /// <summary>
    /// Determina si un rol tiene autorización para operar el módulo de compras.
    /// </summary>
    /// <param name="rol">Rol del usuario.</param>
    /// <returns>True si tiene acceso; en caso contrario, false.</returns>
    public static bool PuedeOperarCompras(string? rol)
    {
        return TieneAcceso(rol, "Compras");
    }

    /// <summary>
    /// Determina si un rol tiene autorización para administrar proveedores.
    /// </summary>
    /// <param name="rol">Rol del usuario.</param>
    /// <returns>True si el rol es Administrador o Encargado; en caso contrario, false.</returns>
    public static bool PuedeAdministrarProveedores(string? rol)
    {
        return !string.IsNullOrWhiteSpace(rol)
            && (string.Equals(rol.Trim(), "administrador", StringComparison.OrdinalIgnoreCase)
                || string.Equals(rol.Trim(), "encargado", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determina si un rol de usuario tiene autorización para gestionar respaldos.
    /// </summary>
    /// <param name="rol">Descripción del rol a verificar.</param>
    /// <returns>True si el rol es Administrador; en caso contrario, false.</returns>
    public static bool PuedeGestionarRespaldos(string? rol)
    {
        return !string.IsNullOrWhiteSpace(rol)
            && string.Equals(rol.Trim(), "administrador", StringComparison.OrdinalIgnoreCase);
    }
}
