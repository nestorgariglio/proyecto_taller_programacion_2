using CapaDatos;
using CapaNegocio;
using CapaNegocio.DTOs.Home;
using CapaNegocio.DTOs.Usuarios;
using CapaPresentacion.Clientes;
using CapaPresentacion.Compras;
using CapaPresentacion.Info;
using CapaPresentacion.Productos;
using CapaPresentacion.Proveedores;
using CapaPresentacion.Reportes;
using CapaPresentacion.Respaldos;
using CapaPresentacion.Tema;
using CapaPresentacion.Ventas;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.EntityFrameworkCore;
using System;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion
{
    /// <summary>
    /// Formulario principal del sistema después de iniciar sesión.
    /// </summary>
    public partial class inicio : MaterialForm
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly GestionUsuariosControl _gestionUsuariosControl;
        private readonly ProductosForm _productosForm;
        private readonly ClientesForm _clientesForm;
        private readonly ProveedoresForm _proveedoresForm;
        private readonly ComprasForm _comprasForm;
        private readonly VentasForm _ventasForm;
        private readonly Func<GenerarBackupForm> _generarBackupFactory;
        private readonly Func<RestaurarBackupForm> _restaurarBackupFactory;
        private readonly HomeResumenNegocio _homeResumenNegocio;
        private Panel? _contenedorBackup;
        private MaterialLabel? _lblMontoVentas;
        private MaterialLabel? _lblCantStock;
        private MaterialLabel? _lblCantClientes;
        private System.Windows.Forms.Timer? _timerCambioDia;
        private CancellationTokenSource? _ctsResumen;
        private DateTime _fechaUltimaConsulta = DateTime.MinValue;
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
            IDbContextFactory<AppDbContext> dbFactory,
            GestionUsuariosControl gestionUsuariosControl,
            ProductosForm productosForm,
            ClientesForm clientesForm,
            ProveedoresForm proveedoresForm,
            ComprasForm comprasForm,
            VentasForm ventasForm,
            Func<GenerarBackupForm> generarBackupFactory,
            Func<RestaurarBackupForm> restaurarBackupFactory,
            HomeResumenNegocio homeResumenNegocio)
        {
            _ventasForm = ventasForm;
            _homeResumenNegocio = homeResumenNegocio;
            _generarBackupFactory = generarBackupFactory;
            _restaurarBackupFactory = restaurarBackupFactory;

            InitializeComponent();

            //LA CONEXIÓN DEL EVENTO
            this.Load += inicio_Load;

            _dbFactory = dbFactory;
            _gestionUsuariosControl = gestionUsuariosControl;
            _gestionUsuariosControl.Dock = DockStyle.Fill;
            _gestionUsuariosControl.BackColor = TemaAplicacion.FondoPrincipal;
            tab_usuarios.Controls.Add(_gestionUsuariosControl);
            tab_usuarios.Enter += tab_usuarios_Enter;

            // La vista de productos llega por DI (patrón de los controles de
            // gestión) y refresca su combo de categorías al entrar en la pestaña.
            _productosForm = productosForm;
            tab_productos.Enter += tab_productos_Enter;

            // La vista del padrón de clientes también llega por DI (D-T8) y recarga
            // los datos reales al entrar en la pestaña.
            _clientesForm = clientesForm;
            tab_clientes.Enter += tab_clientes_Enter;

            _proveedoresForm = proveedoresForm;
            tab_proveedores.Enter += tab_proveedores_Enter;

            // La pantalla de compras también llega por DI (plan §10.1). Ya no
            // recarga ningún historial al entrar: el historial se consulta dentro
            // del modal DetalleCompraForm, así que la pestaña no necesita manejador.
            _comprasForm = comprasForm;
            _ventasForm = ventasForm;

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
            IniciarActualizacionResumen();

            // B. Incrustar los subformularios en sus correspondientes TabPages
            AbrirVistaEnTab(_productosForm, tab_productos);
            AbrirVistaEnTab(_ventasForm, tab_ventas);
            AbrirVistaEnTab(_comprasForm, tab_compras);
            AbrirVistaEnTab(_clientesForm, tab_clientes);
            AbrirVistaEnTab(_proveedoresForm, tab_proveedores);
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
            _lblMontoVentas = lblMontoVentas;
            cardVentas.Controls.Add(lblTituloVentas);
            cardVentas.Controls.Add(lblMontoVentas);

            // KPI 2: Stock
            MaterialCard cardStock = new MaterialCard { Dock = DockStyle.Fill, Margin = new Padding(5) };
            MaterialLabel lblTituloStock = new MaterialLabel { Text = "⚠️ ALERTA DE STOCK", FontType = MaterialSkinManager.fontType.Subtitle2, Location = new Point(15, 15), AutoSize = true };
            MaterialLabel lblCantStock = new MaterialLabel { Text = "3 Prod. Críticos", FontType = MaterialSkinManager.fontType.H5, Location = new Point(15, 45), AutoSize = true };
            _lblCantStock = lblCantStock;
            cardStock.Controls.Add(lblTituloStock);
            cardStock.Controls.Add(lblCantStock);

            // KPI 3: Clientes
            MaterialCard cardClientes = new MaterialCard { Dock = DockStyle.Fill, Margin = new Padding(5) };
            MaterialLabel lblTituloClientes = new MaterialLabel { Text = "👥 CLIENTES ATENDIDOS", FontType = MaterialSkinManager.fontType.Subtitle2, Location = new Point(15, 15), AutoSize = true };
            MaterialLabel lblCantClientes = new MaterialLabel { Text = "14 Atendidos", FontType = MaterialSkinManager.fontType.H5, Location = new Point(15, 45), AutoSize = true };
            _lblCantClientes = lblCantClientes;
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

            // --- 3. TARJETA «GESTIÓN DE BACKUP» ---
            // Dentro del mismo panel con scroll, inmediatamente debajo de los accesos
            // rápidos (sin Dock inferior): no se solapa y se desplaza con el contenido.
            _contenedorBackup = ConstruirContenedorBackup();
            _contenedorBackup.Location = new Point(15, cardAccesos.Bottom + 10);
            _contenedorBackup.Width = panelContenedor.ClientSize.Width - 30;
            _contenedorBackup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelContenedor.Controls.Add(_contenedorBackup);

            tab_inicio.Controls.Add(panelContenedor);
            ActualizarVisibilidadBackup();
        }

        private Panel ConstruirContenedorBackup()
        {
            Panel contenedor = new Panel
            {
                Height = 155,
                BackColor = TemaAplicacion.FondoPrincipal,
                Padding = new Padding(0),
                Visible = false
            };

            MaterialCard cardBackup = new MaterialCard { Dock = DockStyle.Fill, Padding = new Padding(15) };

            TableLayoutPanel tabla = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2
            };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            MaterialLabel lblTitulo = new MaterialLabel
            {
                Text = "💾 GESTIÓN DE BACKUP",
                FontType = MaterialSkinManager.fontType.H6,
                AutoSize = true
            };
            tabla.Controls.Add(lblTitulo, 0, 0);
            tabla.SetColumnSpan(lblTitulo, 2);

            MaterialButton btnGenerar = new MaterialButton { Text = "Generar backup", Dock = DockStyle.Fill, Margin = new Padding(5), Type = MaterialButton.MaterialButtonType.Contained };
            btnGenerar.Click += (s, e) => AbrirModalBackup(generar: true);

            MaterialButton btnRestaurar = new MaterialButton { Text = "Restaurar backup", Dock = DockStyle.Fill, Margin = new Padding(5), Type = MaterialButton.MaterialButtonType.Contained };
            btnRestaurar.Click += (s, e) => AbrirModalBackup(generar: false);

            tabla.Controls.Add(btnGenerar, 0, 1);
            tabla.Controls.Add(btnRestaurar, 1, 1);

            cardBackup.Controls.Add(tabla);
            contenedor.Controls.Add(cardBackup);
            return contenedor;
        }

        private void ActualizarVisibilidadBackup()
        {
            if (_contenedorBackup != null)
                _contenedorBackup.Visible = PoliticaAcceso.PuedeGestionarRespaldos(_usuarioActual?.RolDescripcion);
        }

        private void AbrirModalBackup(bool generar)
        {
            string? rol = _usuarioActual?.RolDescripcion;
            if (!PoliticaAcceso.PuedeGestionarRespaldos(rol)) return;

            if (generar)
            {
                using GenerarBackupForm modal = _generarBackupFactory();
                modal.AplicarSesion(rol);
                modal.ShowDialog(this);
            }
            else
            {
                using RestaurarBackupForm modal = _restaurarBackupFactory();
                modal.AplicarSesion(rol);
                modal.ShowDialog(this);
            }
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

                // Gestión de backup: solo Administrador y Encargado.
                ActualizarVisibilidadBackup();

                // RF-12: el botón "Gestionar categorías" de la vista de productos
                // se muestra solo a quien puede acceder al módulo "Categorías"
                // (Administrador/Encargado); al Vendedor le queda oculto. También
                // se aplica el gating del ABM de productos (RF-11/RF-18).
                _productosForm.AplicarRolUsuario(rol, _usuarioActual?.IdUsuario ?? 0);

                // RF-27/D-5: en el padrón, «Desactivar» y «Reactivar» se muestran solo
                // a Administrador y Encargado; el Vendedor conserva Nuevo y Editar.
                _clientesForm.AplicarRolUsuario(rol);
                _proveedoresForm.AplicarRolUsuario(rol);

                // RF-8/RF-59: la pantalla de compras recibe el usuario responsable y
                // el rol; solo Administrador y Encargado tienen acciones habilitadas.
                _comprasForm.AplicarSesion(_usuarioActual?.IdUsuario ?? 0, rol);

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

                // Al volver a mostrar Home se refrescan los indicadores (ventas/stock pudieron cambiar).
                if (materialTabControl1.SelectedTab == tab_inicio)
                    _ = ActualizarResumenAsync();
            }
        }

        // =========================================================================
        // INDICADORES (KPIs) DE HOME
        // =========================================================================

        private void IniciarActualizacionResumen()
        {
            _ = ActualizarResumenAsync();

            // Timer lento: solo consulta si cambió el día con la aplicación abierta.
            _timerCambioDia = new System.Windows.Forms.Timer { Interval = 60_000 };
            _timerCambioDia.Tick += (s, e) =>
            {
                if (DateTime.Today != _fechaUltimaConsulta)
                    _ = ActualizarResumenAsync();
            };
            _timerCambioDia.Start();
        }

        private async Task ActualizarResumenAsync()
        {
            if (_lblMontoVentas == null || _lblCantStock == null || _lblCantClientes == null)
                return;

            _ctsResumen?.Cancel();
            _ctsResumen?.Dispose();
            CancellationTokenSource cts = new CancellationTokenSource();
            _ctsResumen = cts;

            try
            {
                DateTime dia = DateTime.Today;
                ResumenHomeDto resumen = await _homeResumenNegocio.ObtenerResumenAsync(cts.Token);
                if (cts.IsCancellationRequested || IsDisposed)
                    return;

                if (resumen.Disponible)
                {
                    _fechaUltimaConsulta = dia;
                    _lblMontoVentas.Text = "$ " + resumen.VentasDelDia.ToString("N2", CultureInfo.CurrentCulture);
                    _lblCantStock.Text = $"{resumen.ProductosStockCritico} Prod. Críticos";
                    _lblCantClientes.Text = $"{resumen.ClientesAtendidos} Atendidos";
                }
                else
                {
                    // Error de lectura: sin texto técnico y sin romper Home; reintenta en el próximo refresco.
                    _fechaUltimaConsulta = DateTime.MinValue;
                    _lblMontoVentas.Text = "$ --";
                    _lblCantStock.Text = "-- Prod. Críticos";
                    _lblCantClientes.Text = "-- Atendidos";
                }
            }
            catch (OperationCanceledException)
            {
                // Reemplazada por una consulta más reciente o cierre del formulario.
            }
        }

        private async void tab_usuarios_Enter(object? sender, EventArgs e)
        {
            if (PoliticaAcceso.TieneAcceso(_usuarioActual?.RolDescripcion, "Usuarios"))
            {
                await _gestionUsuariosControl.CargarUsuariosAsync();
            }
        }

        private async void tab_productos_Enter(object? sender, EventArgs e)
        {
            // RF-11/RNF-3: el combo de categorías de la vista de productos se
            // recarga con las categorías activas al entrar en la pestaña,
            // reflejando el ABM sin reiniciar la aplicación (D-7).
            if (PoliticaAcceso.TieneAcceso(_usuarioActual?.RolDescripcion, "Productos"))
            {
                await _productosForm.CargarCategoriasAsync();
            }
        }

        private async void tab_clientes_Enter(object? sender, EventArgs e)
        {
            // RF-16/RF-20: la grilla del padrón se recarga con datos reales al entrar
            // en la pestaña, solo para los roles con acceso al módulo (RF-27).
            if (PoliticaAcceso.TieneAcceso(_usuarioActual?.RolDescripcion, "Clientes"))
            {
                await _clientesForm.CargarClientesAsync();
            }
        }

        private async void tab_proveedores_Enter(object? sender, EventArgs e)
        {
            if (PoliticaAcceso.TieneAcceso(_usuarioActual?.RolDescripcion, "Proveedores"))
            {
                await _proveedoresForm.CargarProveedoresAsync();
            }
        }

        private async void inicio_Shown(object sender, EventArgs e)
        {
            this.ActiveControl = null;

            try
            {
                await using AppDbContext db = await _dbFactory.CreateDbContextAsync();
                await db.Database.OpenConnectionAsync();
                await db.Database.CloseConnectionAsync();
            }
            catch (Exception ex)
            {
                // En modo mock ignora la falta de conexión
            }
        }

        private void inicio_FormClosed(object sender, FormClosedEventArgs e)
        {
            _timerCambioDia?.Stop();
            _ctsResumen?.Cancel();
            Application.Exit();
        }
    }
}
