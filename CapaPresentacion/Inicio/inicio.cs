using CapaDatos;
using CapaNegocio;
using CapaNegocio.DTOs.Usuarios;
using CapaPresentacion.Clientes;
using CapaPresentacion.Compras;
using CapaPresentacion.Info;
using CapaPresentacion.Productos;
using CapaPresentacion.Proveedores;
using CapaPresentacion.Reportes;
using CapaPresentacion.Tema;
using CapaPresentacion.Ventas;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.EntityFrameworkCore;
using System;
using System.Drawing;
using System.Windows.Forms;

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
        public inicio(
            AppDbContext db,
            GestionUsuariosControl gestionUsuariosControl)
        {
            InitializeComponent();

            //LA CONEXIÓN DEL EVENTO
            this.Load += inicio_Load;

            _db = db;
            _gestionUsuariosControl = gestionUsuariosControl;
            _gestionUsuariosControl.Dock = DockStyle.Fill;
            _gestionUsuariosControl.BackColor = TemaAplicacion.FondoPrincipal;
            tab_usuarios.Controls.Add(_gestionUsuariosControl);
            tab_usuarios.Enter += tab_usuarios_Enter;

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            ConfigurarFondosDeNavegacion();
        }
        private void ConfigurarFondosDeNavegacion()
        {
            Color fondoPrincipal = TemaAplicacion.FondoPrincipal;

            materialTabControl1.BackColor = fondoPrincipal;

            foreach (TabPage tabPage in materialTabControl1.TabPages)
            {
                tabPage.UseVisualStyleBackColor = false;
                tabPage.BackColor = fondoPrincipal;
            }
        }

        // =========================================================================
        // 1. CARGA INICIAL Y VISTAS (MOCKEADO Y SUBSISTEMAS)
        // =========================================================================

        private void inicio_Load(object sender, EventArgs e)
        {
            // A. Construir visualmente el Dashboard en tab_inicio
            ArmarDashboardInicio();

            // B. Incrustar los subformularios en sus correspondientes TabPages
            AbrirVistaEnTab(new ProductosForm(), tab_productos);
            AbrirVistaEnTab(new VentasForm(), tab_ventas);
            AbrirVistaEnTab(new ComprasForm(), tab_compras);
            AbrirVistaEnTab(new ClientesForm(), tab_clientes);
            AbrirVistaEnTab(new ProveedoresForm(), tab_proveedores);
            AbrirVistaEnTab(new ReportesForm(), tab_reportes);
            AbrirVistaEnTab(new InfoForm(), tab_info);
        }

        private void AbrirVistaEnTab(UserControl vista, TabPage tabPage)
        {
            if (tabPage == null) return;

            tabPage.Controls.Clear();
            vista.Dock = DockStyle.Fill;
            tabPage.Controls.Add(vista);
            tabPage.Tag = vista;
        }

        private void ArmarDashboardInicio()
        {
            tab_inicio.Controls.Clear();

            Panel panelContenedor = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = TemaAplicacion.FondoPrincipal,
                Padding = new Padding(15)
            };

            // --- 1. CONTENEDOR FLUIDO PARA LOS 3 KPIs ---
            TableLayoutPanel tableKpis = new TableLayoutPanel
            {
                Location = new Point(15, 15),
                Height = 110,
                Width = panelContenedor.ClientSize.Width - 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ColumnCount = 3,
                RowCount = 1
            };
            tableKpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tableKpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tableKpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));

            // KPI 1: Ventas
            MaterialCard cardVentas = new MaterialCard { Dock = DockStyle.Fill, Margin = new Padding(5) };
            MaterialLabel lblTituloVentas = new MaterialLabel { Text = "💳 VENTAS DEL DÍA", FontType = MaterialSkinManager.fontType.Subtitle2, Location = new Point(15, 15), AutoSize = true };
            MaterialLabel lblMontoVentas = new MaterialLabel { Text = "$ 185.400,00", FontType = MaterialSkinManager.fontType.H5, Location = new Point(15, 45), AutoSize = true, UseAccent = true };
            cardVentas.Controls.Add(lblTituloVentas);
            cardVentas.Controls.Add(lblMontoVentas);

            // KPI 2: Stock
            MaterialCard cardStock = new MaterialCard { Dock = DockStyle.Fill, Margin = new Padding(5) };
            MaterialLabel lblTituloStock = new MaterialLabel { Text = "⚠️ ALERTA DE STOCK", FontType = MaterialSkinManager.fontType.Subtitle2, Location = new Point(15, 15), AutoSize = true };
            MaterialLabel lblCantStock = new MaterialLabel { Text = "3 Prod. Críticos", FontType = MaterialSkinManager.fontType.H5, Location = new Point(15, 45), AutoSize = true };
            cardStock.Controls.Add(lblTituloStock);
            cardStock.Controls.Add(lblCantStock);

            // KPI 3: Clientes
            MaterialCard cardClientes = new MaterialCard { Dock = DockStyle.Fill, Margin = new Padding(5) };
            MaterialLabel lblTituloClientes = new MaterialLabel { Text = "👥 CLIENTES ATENDIDOS", FontType = MaterialSkinManager.fontType.Subtitle2, Location = new Point(15, 15), AutoSize = true };
            MaterialLabel lblCantClientes = new MaterialLabel { Text = "14 Atendidos", FontType = MaterialSkinManager.fontType.H5, Location = new Point(15, 45), AutoSize = true };
            cardClientes.Controls.Add(lblTituloClientes);
            cardClientes.Controls.Add(lblCantClientes);

            tableKpis.Controls.Add(cardVentas, 0, 0);
            tableKpis.Controls.Add(cardStock, 1, 0);
            tableKpis.Controls.Add(cardClientes, 2, 0);

            // --- 2. TARJETA DE ACCESOS RÁPIDOS RESPONSIVA ---
            MaterialCard cardAccesos = new MaterialCard
            {
                Location = new Point(15, 135),
                Height = 190,
                Width = panelContenedor.ClientSize.Width - 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(15)
            };

            MaterialLabel lblTituloAccesos = new MaterialLabel
            {
                Text = "⚡ ACCESOS RÁPIDOS DEL SISTEMA",
                FontType = MaterialSkinManager.fontType.H6,
                Location = new Point(15, 15),
                AutoSize = true
            };

            TableLayoutPanel tableBotones = new TableLayoutPanel
            {
                Location = new Point(15, 55),
                Height = 115,
                Width = cardAccesos.Width - 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ColumnCount = 2,
                RowCount = 2
            };
            tableBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            MaterialButton btnVentas = new MaterialButton { Text = "🛒 Punto de Venta (POS)", Dock = DockStyle.Fill, Margin = new Padding(5), Type = MaterialButton.MaterialButtonType.Contained, UseAccentColor = true };
            btnVentas.Click += (s, e) => { if (materialTabControl1.TabPages.Contains(tab_ventas)) materialTabControl1.SelectedTab = tab_ventas; };

            MaterialButton btnProductos = new MaterialButton { Text = "📦 Catálogo de Productos", Dock = DockStyle.Fill, Margin = new Padding(5), Type = MaterialButton.MaterialButtonType.Contained };
            btnProductos.Click += (s, e) => { if (materialTabControl1.TabPages.Contains(tab_productos)) materialTabControl1.SelectedTab = tab_productos; };

            MaterialButton btnClientes = new MaterialButton { Text = "👥 Padrón de Clientes", Dock = DockStyle.Fill, Margin = new Padding(5), Type = MaterialButton.MaterialButtonType.Contained };
            btnClientes.Click += (s, e) => { if (materialTabControl1.TabPages.Contains(tab_clientes)) materialTabControl1.SelectedTab = tab_clientes; };

            MaterialButton btnInfo = new MaterialButton { Text = "❓ Centro de Ayuda y Guía", Dock = DockStyle.Fill, Margin = new Padding(5), Type = MaterialButton.MaterialButtonType.Contained };
            btnInfo.Click += (s, e) => { if (materialTabControl1.TabPages.Contains(tab_info)) materialTabControl1.SelectedTab = tab_info; };

            tableBotones.Controls.Add(btnVentas, 0, 0);
            tableBotones.Controls.Add(btnProductos, 1, 0);
            tableBotones.Controls.Add(btnClientes, 0, 1);
            tableBotones.Controls.Add(btnInfo, 1, 1);

            cardAccesos.Controls.Add(lblTituloAccesos);
            cardAccesos.Controls.Add(tableBotones);

            panelContenedor.Controls.Add(tableKpis);
            panelContenedor.Controls.Add(cardAccesos);

            tab_inicio.Controls.Add(panelContenedor);
        }

        // =========================================================================
        // 2. GESTIÓN DE SESIÓN Y PERMISOS DE ACCESO (RBAC)
        // =========================================================================

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
                RefrescarDrawer();
            }
            finally
            {
                _reconstruyendoNavegacion = false;
            }
        }

        private void RefrescarDrawer()
        {
            materialTabControl1.ImageList = imageList1;
            DrawerTabControl = materialTabControl1;
            DrawerShowIconsWhenHidden = true;
            materialTabControl1.Invalidate(true);
        }

        private void RestaurarNavegacionBase()
        {
            TabPage[] navegacionBase =
            {
                tab_inicio, tab_usuarios, tab_productos, tab_ventas, tab_compras,
                tab_clientes, tab_proveedores, tab_reportes, tab_info, tab_salir
            };

            for (int indice = 0; indice < navegacionBase.Length; indice++)
            {
                TabPage tab = navegacionBase[indice];

                if (!materialTabControl1.TabPages.Contains(tab))
                    materialTabControl1.TabPages.Insert(indice, tab);
            }
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

        // =========================================================================
        // 3. EVENTOS DEL NAVEGADOR Y FORMULARIO
        // =========================================================================

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
                // En modo mock ignora la falta de conexión
            }
        }

        private void inicio_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
