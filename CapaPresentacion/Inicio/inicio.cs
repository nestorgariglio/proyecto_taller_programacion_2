using System;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.EntityFrameworkCore;
using CapaDatos;
using CapaNegocio;
using CapaNegocio.DTOs.Usuarios;

namespace CapaPresentacion
{
    /// <summary>
    /// Formulario principal del sistema después de iniciar sesión.
    /// </summary>
    public partial class inicio : MaterialForm
    {
        private readonly AppDbContext _db;
        private readonly GestionUsuariosControl _gestionUsuariosControl;
        private TabPage? _pestañaAnterior;
        private UsuarioRespuestaDto? _usuarioActual;
        private bool _reconstruyendoNavegacion;

        /// <summary>
        /// Se produce cuando el usuario solicita cerrar su sesión y volver al login.
        /// </summary>
        public event EventHandler? CierreSesionSolicitado;

        /// <summary>
        /// Inicializa el formulario principal.
        /// </summary>
        /// <param name="db">Contexto de datos utilizado por el formulario.</param>
        /// <param name="gestionUsuariosControl">Control de gestión de usuarios.</param>
        public inicio(
            AppDbContext db,
            GestionUsuariosControl gestionUsuariosControl)
        {
            InitializeComponent();
            _db = db;
            _gestionUsuariosControl = gestionUsuariosControl;
            _gestionUsuariosControl.Dock = DockStyle.Fill;
            tab_usuarios.Controls.Add(_gestionUsuariosControl);
            tab_usuarios.Enter += tab_usuarios_Enter;

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(
                Primary.BlueGrey900, Primary.BlueGrey900, Primary.BlueGrey500, Accent.DeepOrange700, TextShade.WHITE
            );
        }

        /// <summary>
        /// Establece el usuario autenticado y actualiza la navegación disponible.
        /// </summary>
        /// <param name="usuario">Usuario autenticado que inicia la sesión.</param>
        public void EstablecerSesionUsuario(UsuarioRespuestaDto usuario)
        {
            _usuarioActual = usuario;

            string nombreUsuario = $"{_usuarioActual.Nombre} {_usuarioActual.Apellido}".Trim();
            string rolDescripcion = _usuarioActual.RolDescripcion ?? "Sin Rol";

            this.Text = $"Sistema de Ventas - Usuario: {nombreUsuario} ({rolDescripcion})";

            AplicarPermisosPorRol();
        }

        private void AplicarPermisosPorRol()
        {
            TabPage? pestañaSeleccionada = materialTabControl1.SelectedTab;
            if (pestañaSeleccionada != null && pestañaSeleccionada != tab_salir)
                _pestañaAnterior = pestañaSeleccionada;
            _reconstruyendoNavegacion = true;

            try
            {
                RestaurarNavegacionBase();

                string? rol = _usuarioActual?.RolDescripcion;
                OcultarSiNoAutorizada("tab_usuarios", "Usuarios", rol);
                OcultarSiNoAutorizada("tab_productos", "Productos", rol);
                OcultarSiNoAutorizada("tab_ventas", "Ventas", rol);
                OcultarSiNoAutorizada("tab_compras", "Compras", rol);
                OcultarSiNoAutorizada("tab_clientes", "Clientes", rol);
                OcultarSiNoAutorizada("tab_proveedores", "Proveedores", rol);
                OcultarSiNoAutorizada("tab_reportes", "Reportes", rol);

                TabPage? pestañaASeleccionar = _pestañaAnterior;
                if (pestañaASeleccionar == null ||
                    !materialTabControl1.TabPages.Contains(pestañaASeleccionar))
                {
                    pestañaASeleccionar = ObtenerPestañaVisibleSegura();
                }

                if (pestañaASeleccionar != null)
                    materialTabControl1.SelectedTab = pestañaASeleccionar;

                ActualizarReferenciaPestañaAnterior();
            }
            finally
            {
                _reconstruyendoNavegacion = false;
            }
        }

        private void RestaurarNavegacionBase()
        {
            TabPage[] navegacionBase =
            {
                tab_inicio, tab_usuarios, tab_productos, tab_ventas, tab_compras,
                tab_clientes, tab_proveedores, tab_reportes, tab_info, tab_salir
            };

            foreach (TabPage tab in navegacionBase)
                materialTabControl1.TabPages.Remove(tab);

            materialTabControl1.TabPages.AddRange(navegacionBase);
            AsegurarReferenciaPestañaAnterior();
        }

        private void ActualizarReferenciaPestañaAnterior()
        {
            TabPage? seleccionada = materialTabControl1.SelectedTab;
            if (seleccionada != null && seleccionada != tab_salir &&
                materialTabControl1.TabPages.Contains(seleccionada))
            {
                _pestañaAnterior = seleccionada;
                return;
            }

            if (_pestañaAnterior == null ||
                !materialTabControl1.TabPages.Contains(_pestañaAnterior) ||
                _pestañaAnterior == tab_salir)
            {
                _pestañaAnterior = ObtenerPestañaVisibleSegura();
            }
        }

        private TabPage? ObtenerPestañaVisibleSegura()
        {
            foreach (TabPage tab in materialTabControl1.TabPages)
            {
                if (tab != tab_salir)
                    return tab;
            }

            return null;
        }

        private void AsegurarReferenciaPestañaAnterior()
        {
            if (_pestañaAnterior == null ||
                !materialTabControl1.TabPages.Contains(_pestañaAnterior) ||
                _pestañaAnterior == tab_salir)
            {
                _pestañaAnterior = ObtenerPestañaVisibleSegura();
            }
        }

        private void OcultarSiNoAutorizada(string nombreTab, string modulo, string? rol)
        {
            if (!PoliticaAcceso.TieneAcceso(rol, modulo)) OcultarTabSiExiste(nombreTab);
        }

        private void OcultarTabSiExiste(string nombreTab)
        {
            if (materialTabControl1.TabPages.ContainsKey(nombreTab))
            {
                materialTabControl1.TabPages.RemoveByKey(nombreTab);
            }
        }

        private void materialTabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_reconstruyendoNavegacion)
                return;

            if (materialTabControl1.SelectedTab == tab_salir)
            {
                this.BeginInvoke(new Action(() =>
                {
                    MaterialDialog dialog = new MaterialDialog(
                        this,
                        "Salir del sistema",
                        "¿Desea cerrar la aplicación o cerrar la sesión?",
                        "SALIR",
                        true,
                        "CERRAR SESIÓN"
                    );

                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        Application.Exit();
                    }
                    else
                    {
                        _usuarioActual = null;
                        CierreSesionSolicitado?.Invoke(this, EventArgs.Empty);
                    }
                }));
            }
            else
            {
                ActualizarReferenciaPestañaAnterior();
            }
        }

        private async void tab_usuarios_Enter(object? sender, EventArgs e)
        {
            if (PoliticaAcceso.TieneAcceso(_usuarioActual?.RolDescripcion, "Usuarios"))
            {
                await _gestionUsuariosControl.CargarUsuariosAsync();
            }
        }

        private async void inicio_Shown(object sender, EventArgs e)
        {
            this.ActiveControl = null;

            try
            {
                await _db.Database.OpenConnectionAsync();
                await _db.Database.CloseConnectionAsync();
            }
            catch (Exception ex)
            {
                MaterialMessageBox.Show(this, "Detalle del error:\n" + ex.Message, "Error SQL");
            }
        }

        private void inicio_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
