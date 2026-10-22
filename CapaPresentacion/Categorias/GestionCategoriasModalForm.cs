using CapaNegocio;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CapaPresentacion;

/// <summary>
/// Ventana modal que aloja la gestión de categorías/rubros para abrirla desde
/// la pantalla de Productos (RF-10, reencuadre aprobado D-4).
/// </summary>
/// <remarks>
/// Contenedor liviano: su único contenido es una instancia del control
/// existente <see cref="GestionCategoriasControl"/> alojada a <c>Dock = Fill</c>.
/// No duplica grilla, filtros, botones ni reglas de negocio (RNF-3): reutiliza
/// el control tal cual. La carga inicial del listado (activas e inactivas) se
/// dispara en <see cref="System.Windows.Forms.Form.Shown"/>, cuando la ventana
/// ya es visible.
/// El modal no distingue si el usuario realizó operaciones: quien lo abre debe
/// refrescar lo que corresponda al volver de <c>ShowDialog</c> (por ejemplo, el
/// combo de categorías activas de <c>ProductosForm</c>, RNF-3 / D-7).
/// Se instancia directamente con la dependencia ya inyectada
/// (<c>new GestionCategoriasModalForm(_categoriaNegocio)</c>), mismo patrón que
/// <c>new CategoriaForm(_categoriaNegocio)</c>; no requiere registro adicional
/// en <c>Program.cs</c>.
/// </remarks>
public partial class GestionCategoriasModalForm : MaterialForm
{
    private readonly GestionCategoriasControl _gestionCategoriasControl;
    private readonly string? _rol;

    /// <summary>
    /// Inicializa la ventana modal de gestión de categorías.
    /// </summary>
    /// <param name="categoriaNegocio">Servicio que ejecuta las operaciones de categorías.</param>
    /// <param name="rol">
    /// Descripción del rol del usuario que abre el modal. Es opcional para no
    /// romper invocaciones existentes; cuando se informa, el modal aplica
    /// defensa en profundidad y rechaza abrirse si el rol no tiene acceso al
    /// módulo "Categorías" (RF-12).
    /// </param>
    public GestionCategoriasModalForm(CategoriaNegocio categoriaNegocio, string? rol = null)
    {
        ArgumentNullException.ThrowIfNull(categoriaNegocio);

        _rol = rol;

        InitializeComponent();

        // RF-10: reutilización total del control de gestión ya construido; el
        // modal solo lo aloja a pantalla completa y no repite su lógica.
        _gestionCategoriasControl = new GestionCategoriasControl(categoriaNegocio)
        {
            Dock = DockStyle.Fill
        };

        Controls.Add(_gestionCategoriasControl);

        Shown += GestionCategoriasModalForm_Shown;

        var materialSkinManager = MaterialSkinManager.Instance;
        materialSkinManager.AddFormToManage(this);
    }

    private async void GestionCategoriasModalForm_Shown(object? sender, EventArgs e)
    {
        // RF-12: defensa en profundidad. El punto de entrada (ProductosForm) ya
        // valida el rol, pero si por cualquier vía se abre el ABM con un rol no
        // autorizado, el sistema lo rechaza: informa en español y cierra el
        // modal sin cargar el listado.
        if (_rol is not null && !PoliticaAcceso.TieneAcceso(_rol, "Categorías"))
        {
            MaterialMessageBox.Show(
                this,
                "Acceso no autorizado. Este módulo solo está disponible para Administrador o Encargado.",
                "Acceso denegado");
            Close();
            return;
        }

        // RF-10: al abrirse el modal se listan todas las categorías, activas e
        // inactivas, con su estado (el control aplica además sus filtros).
        await _gestionCategoriasControl.CargarCategoriasAsync();
    }
}
