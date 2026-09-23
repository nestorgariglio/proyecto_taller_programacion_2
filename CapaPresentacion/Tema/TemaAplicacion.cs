using System.Drawing;
using MaterialSkin;

namespace CapaPresentacion.Tema;

/// <summary>
/// Configuración visual compartida por las vistas de la aplicación.
/// </summary>
internal static class TemaAplicacion
{
    /// <summary>
    /// Inicializa el tema oscuro global de MaterialSkin.
    /// </summary>
    public static void Configurar()
    {
        var materialSkinManager = MaterialSkinManager.Instance;
        materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
        materialSkinManager.ColorScheme = new ColorScheme(
            Primary.BlueGrey900,
            Primary.BlueGrey900,
            Primary.BlueGrey500,
            Accent.DeepOrange700,
            TextShade.WHITE);
    }

    /// <summary>
    /// Obtiene el color de fondo principal definido por el tema actual.
    /// </summary>
    public static Color FondoPrincipal => MaterialSkinManager.Instance.BackdropColor;
}
