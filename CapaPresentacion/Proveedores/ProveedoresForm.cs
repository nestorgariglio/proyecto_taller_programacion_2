using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CapaPresentacion.Tema;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CapaPresentacion.Proveedores
{
    public partial class ProveedoresForm : UserControl
    {
        public ProveedoresForm()
        {
            InitializeComponent();
            BackColor = TemaAplicacion.FondoPrincipal;
            ArmarInterfazMaterial();
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
                Text = "🏭 GESTIÓN DE PROVEEDORES",
                FontType = MaterialSkinManager.fontType.H6,
                Location = new Point(20, 15),
                AutoSize = true
            };

            // 3. Controles
            MaterialTextBox2 txtBuscar = new MaterialTextBox2
            {
                Hint = "Buscar por Razón Social o CUIT...",
                Location = new Point(20, 55),
                Width = 350,
                AnimateReadOnly = true
            };

            MaterialButton btnNuevo = new MaterialButton
            {
                Text = "➕ Registrar Proveedor",
                Height = 48,
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true
            };

            MaterialComboBox cboEstado = new MaterialComboBox
            {
                Hint = "Estado",
                Location = new Point(385, 55),
                Width = 150,
                StartIndex = 0
            };
            cboEstado.Items.AddRange(new object[] { "Activos", "Inactivos" });

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

            panelAcciones.Controls.Add(btnNuevo);
            panelAcciones.Controls.Add(btnEditar);
            panelAcciones.Controls.Add(btnDesactivar);
            panelAcciones.Controls.Add(btnActualizar);

            // 4. DataGridView Estilizado para Dark Mode
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
            dt.Columns.Add("CUIT");
            dt.Columns.Add("Razón Social");
            dt.Columns.Add("Rubro");
            dt.Columns.Add("Teléfono");
            dt.Columns.Add("Estado");

            dt.Rows.Add("30-5000123-8", "Logitech Argentina S.A.", "Periféricos", "011-4555-0000", "Activo");
            dt.Rows.Add("30-7112233-4", "Samsung Elec. Arg.", "Monitores/TV", "011-4000-1111", "Activo");
            dt.Rows.Add("30-6899112-1", "Distribuidora Norte SRL", "Varios", "3624-441122", "Activo");

            dgv.DataSource = dt;

            // Ensamblar
            panelPrincipal.Controls.Add(lblTitulo);
            panelPrincipal.Controls.Add(txtBuscar);
            panelPrincipal.Controls.Add(cboEstado);
            panelPrincipal.Controls.Add(panelAcciones);
            panelPrincipal.Controls.Add(dgv);

            Controls.Add(panelPrincipal);
        }
    }
}
