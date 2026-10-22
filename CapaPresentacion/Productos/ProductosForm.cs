using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CapaNegocio;
using CapaNegocio.DTOs.Categorias;
using MaterialSkin;
using MaterialSkin.Controls;
using CapaPresentacion.Tema;

namespace CapaPresentacion.Productos
{
    public partial class ProductosForm : UserControl
    {
        private readonly CategoriaNegocio _categoriaNegocio;
        private MaterialComboBox _cboCategoria = null!;
        private MaterialButton _btnGestionCategorias = null!;
        private bool _cargandoCategorias;
        private string? _rolActual;

        /// <summary>
        /// Inicializa la vista de productos.
        /// </summary>
        /// <param name="categoriaNegocio">Servicio que lista las categorías activas del combo (RF-11).</param>
        public ProductosForm(CategoriaNegocio categoriaNegocio)
        {
            ArgumentNullException.ThrowIfNull(categoriaNegocio);

            _categoriaNegocio = categoriaNegocio;
            InitializeComponent();
            BackColor = TemaAplicacion.FondoPrincipal;
            ArmarInterfazMaterial();
        }

        /// <summary>
        /// Carga en el combo de categorías únicamente las categorías activas
        /// (RF-11) más la opción "Todas". Se invoca al entrar en la pestaña de
        /// productos para reflejar los cambios del ABM de categorías sin
        /// reiniciar la aplicación (RNF-3).
        /// </summary>
        /// <returns>Tarea que representa la carga del combo.</returns>
        public async Task CargarCategoriasAsync()
        {
            if (_cargandoCategorias)
            {
                return;
            }

            _cargandoCategorias = true;

            try
            {
                IReadOnlyList<CategoriaRespuestaDto> categoriasActivas =
                    await _categoriaNegocio.ListarCategoriasActivasAsync();

                _cboCategoria.Items.Clear();

                // La opción "Todas" siempre queda presente, aun sin categorías
                // activas: el combo no debe quedar vacío ni fallar (caso límite).
                _cboCategoria.Items.Add("Todas");

                foreach (CategoriaRespuestaDto categoria in categoriasActivas)
                {
                    if (!string.IsNullOrWhiteSpace(categoria.Descripcion))
                    {
                        _cboCategoria.Items.Add(categoria.Descripcion);
                    }
                }

                _cboCategoria.SelectedIndex = 0;
            }
            catch (Exception exception)
            {
                // Caso límite: fallo o desconexión de la BD ⇒ se conserva al
                // menos la opción "Todas" y se informa el detalle en español.
                if (!_cboCategoria.Items.Contains("Todas"))
                {
                    _cboCategoria.Items.Add("Todas");
                    _cboCategoria.SelectedIndex = 0;
                }

                MaterialMessageBox.Show(
                    this,
                    $"No se pudieron cargar las categorías del combo.\nDetalle: {exception.Message}",
                    "Error SQL");
            }
            finally
            {
                _cargandoCategorias = false;
            }
        }

        /// <summary>
        /// Aplica el gating por rol al botón "Gestionar categorías" (RF-12).
        /// El botón solo es visible para Administrador y Encargado, que son los
        /// roles con acceso al módulo "Categorías" según <see cref="PoliticaAcceso"/>;
        /// para cualquier otro rol (p. ej. Vendedor) queda oculto.
        /// </summary>
        /// <param name="rol">Descripción del rol del usuario (puede ser null o vacío).</param>
        /// <remarks>
        /// Lo invoca <c>inicio.AplicarPermisosPorRol()</c> con el rol de la sesión
        /// ya resuelto, evitando cambiar el ctor (y el registro de DI) de la vista.
        /// </remarks>
        public void AplicarRolUsuario(string? rol)
        {
            _rolActual = rol;
            _btnGestionCategorias.Visible = PoliticaAcceso.TieneAcceso(rol, "Categorías");
        }

        private async void btnGestionCategorias_Click(object? sender, EventArgs e)
        {
            // RF-12: guard defensivo en tiempo de ejecución. Aunque el botón se
            // oculta por rol, si un rol no autorizado lograra invocar el ABM (por
            // ejemplo, disparando el handler por código y no por clic), el sistema
            // lo rechaza aquí antes de abrir el modal.
            if (!PoliticaAcceso.TieneAcceso(_rolActual, "Categorías"))
            {
                MaterialMessageBox.Show(
                    this,
                    "Acceso no autorizado. Este módulo solo está disponible para Administrador o Encargado.",
                    "Acceso denegado");
                return;
            }

            // RF-10: abre el ABM de categorías en una ventana modal que reutiliza
            // GestionCategoriasControl sin duplicar lógica (reencuadre aprobado D-4).
            using (var modal = new GestionCategoriasModalForm(_categoriaNegocio, _rolActual))
            {
                // El modal no distingue si hubo operaciones ni fuerza DialogResult
                // (cerrarlo con la X devuelve Cancel), así que el combo se refresca
                // SIEMPRE al volver, sin condicionarlo al resultado (D-7 / RNF-3).
                modal.ShowDialog(this);
            }

            await CargarCategoriasAsync();
        }

        private void ArmarInterfazMaterial()
        {
            Controls.Clear();

            // 1. Contenedor principal de la vista
            Panel panelPrincipal = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = TemaAplicacion.FondoPrincipal,
                Padding = new Padding(15)
            };

            // 2. Encabezado
            MaterialLabel lblTitulo = new MaterialLabel
            {
                Text = "📦 CATÁLOGO Y GESTIÓN DE PRODUCTOS",
                FontType = MaterialSkinManager.fontType.H6,
                Location = new Point(20, 15),
                AutoSize = true
            };

            // 3. Controles MaterialSkin 2 (Buscador, Combo y Botón)
            MaterialTextBox2 txtBuscar = new MaterialTextBox2
            {
                Hint = "Buscar por código o nombre...",
                Location = new Point(20, 55),
                Width = 280,
                AnimateReadOnly = true
            };

            // El combo se puebla con las categorías activas reales en
            // CargarCategoriasAsync() (RF-11), no con datos de prueba.
            _cboCategoria = new MaterialComboBox
            {
                Hint = "Categoría",
                Location = new Point(310, 55),
                Width = 180,
                StartIndex = 0
            };
            MaterialComboBox cboEstado = new MaterialComboBox
            {
                Hint = "Estado",
                Location = new Point(505, 55),
                Width = 150,
                StartIndex = 0
            };
            cboEstado.Items.AddRange(new object[] { "Activos", "Inactivos" });

            MaterialButton btnNuevo = new MaterialButton
            {
                Text = "➕ Nuevo Producto",
                Height = 48,
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true
            };

            FlowLayoutPanel panelAcciones = new FlowLayoutPanel
            {
                Location = new Point(20, 110),
                Width = panelPrincipal.Width - 40,
                Height = 55,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 2, 0, 0)
            };

            MaterialButton btnEditar = new MaterialButton
            {
                Text = "✏️ Editar",
                Width = 120,
                Height = 48,
                Type = MaterialButton.MaterialButtonType.Contained
            };

            MaterialButton btnDesactivar = new MaterialButton
            {
                Text = "⛔ Desactivar",
                Width = 150,
                Height = 48,
                Type = MaterialButton.MaterialButtonType.Contained
            };

            MaterialButton btnActualizar = new MaterialButton
            {
                Text = "🔄 Actualizar",
                Width = 135,
                Height = 48,
                Type = MaterialButton.MaterialButtonType.Contained
            };

            // Botón para abrir el ABM de categorías en ventana modal (RF-10).
            // Nace oculto: AplicarRolUsuario lo muestra solo a Administrador y
            // Encargado (RF-12). Así, aunque nunca se invoque el gating (rol
            // ausente), ningún otro rol ve la acción.
            _btnGestionCategorias = new MaterialButton
            {
                Text = "🏷️ Gestionar categorías",
                Width = 180,
                Height = 48,
                Type = MaterialButton.MaterialButtonType.Contained,
                Visible = false
            };
            _btnGestionCategorias.Click += btnGestionCategorias_Click;

            panelAcciones.Controls.Add(btnNuevo);
            panelAcciones.Controls.Add(btnEditar);
            panelAcciones.Controls.Add(btnDesactivar);
            panelAcciones.Controls.Add(btnActualizar);
            panelAcciones.Controls.Add(_btnGestionCategorias);

            // 4. DataGridView Estilizado para Dark Mode de MaterialSkin
            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 180),
                Width = panelPrincipal.Width - 40,
                Height = 300,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.FromArgb(40, 40, 40),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EnableHeadersVisualStyles = false
            };

            // Estilos oscuros del DataGridView para integrarse con Material
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Roboto", 10, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 40;

            dgv.DefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
            dgv.DefaultCellStyle.ForeColor = Color.White;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(80, 80, 80);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.RowTemplate.Height = 35;

            // Datos de prueba
            DataTable dt = new DataTable();
            dt.Columns.Add("Código");
            dt.Columns.Add("Producto");
            dt.Columns.Add("Categoría");
            dt.Columns.Add("P. Costo");
            dt.Columns.Add("P. Venta");
            dt.Columns.Add("Stock", typeof(int));
            dt.Columns.Add("Estado");

            dt.Rows.Add("PROD-001", "Teclado Mecánico RGB", "Periféricos", "$ 25.000", "$ 45.000", 12, "Activo");
            dt.Rows.Add("PROD-002", "Mouse Inalámbrico", "Periféricos", "$ 12.000", "$ 22.500", 3, "Activo");
            dt.Rows.Add("PROD-003", "Monitor 24\" 144Hz", "Monitores", "$ 140.000", "$ 210.000", 8, "Activo");

            dgv.DataSource = dt;

            // Ensamblar la vista
            panelPrincipal.Controls.Add(lblTitulo);
            panelPrincipal.Controls.Add(txtBuscar);
            panelPrincipal.Controls.Add(_cboCategoria);
            panelPrincipal.Controls.Add(cboEstado);
            panelPrincipal.Controls.Add(panelAcciones);
            panelPrincipal.Controls.Add(dgv);

            Controls.Add(panelPrincipal);
        }
    }
}
