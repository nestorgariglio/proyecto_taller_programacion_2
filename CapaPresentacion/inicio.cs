using System;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.EntityFrameworkCore;
using CapaDatos;
using CapaEntidad;

namespace CapaPresentacion
{
    public partial class inicio : MaterialForm
    {
        private readonly AppDbContext _db;
        private int indiceAnterior = 0;
        private Usuario? _usuarioActual;

        // Inyectamos el DbContext desde el contenedor DI
        public inicio(AppDbContext db)
        {
            InitializeComponent();
            _db = db;

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(
                Primary.BlueGrey900, Primary.BlueGrey900, Primary.BlueGrey500, Accent.DeepOrange700, TextShade.WHITE
            );
        }

        /// <summary>
        /// Recibe el usuario autenticado desde el Login y configura la sesión
        /// </summary>
        public void EstablecerSesionUsuario(Usuario usuario)
        {
            _usuarioActual = usuario;

            // Muestra en la barra del formulario el nombre del usuario y su Rol
            string nombreUsuario = $"{_usuarioActual.Nombre} {_usuarioActual.Apellido}".Trim();
            string rolDescripcion = _usuarioActual.Rol?.Descripcion ?? "Sin Rol";

            this.Text = $"Sistema de Ventas - Usuario: {nombreUsuario} ({rolDescripcion})";

            // Aplica la restricción de pestañas según el ROL
            AplicarPermisosPorRol();
        }

        private void AplicarPermisosPorRol()
        {
            if (_usuarioActual?.Rol == null) return;

            string rol = _usuarioActual.Rol.Descripcion?.ToLower() ?? "";

            // Oculta/Remueve pestañas según la jerarquía de roles
            switch (rol)
            {
                case "vendedor":
                    // El vendedor solo opera Ventas y Clientes; se ocultan administración y reportes
                    OcultarTabSiExiste("tab_usuarios");
                    OcultarTabSiExiste("tab_compras");
                    OcultarTabSiExiste("tab_proveedores");
                    OcultarTabSiExiste("tab_reportes");
                    break;

                case "encargado":
                    // El encargado gestiona Productos, Categorías, Compras y Reportes, pero no Usuarios del sistema
                    OcultarTabSiExiste("tab_usuarios");
                    break;

                case "administrador":
                    // Acceso total a todas las pestañas
                    break;
            }
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
            if (materialTabControl1.SelectedTab == tab_salir)
            {
                this.BeginInvoke(new Action(() =>
                {
                    MaterialDialog dialog = new MaterialDialog(
                        this, "Cerrar Aplicación", "¿Está seguro que desea salir del sistema?", "SÍ", true, "NO"
                    );

                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        Application.Exit();
                    }
                    else
                    {
                        materialTabControl1.SelectedIndex = indiceAnterior;
                    }
                }));
            }
            else
            {
                indiceAnterior = materialTabControl1.SelectedIndex;
            }
        }

        private async void inicio_Shown(object sender, EventArgs e)
        {
            this.ActiveControl = null;

            try
            {
                // Intentamos abrir la conexión directamente para capturar la excepción exacta
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
            // Asegura cerrar el proceso si el usuario cierra el Form desde la 'X' superior
            Application.Exit();
        }
    }
}