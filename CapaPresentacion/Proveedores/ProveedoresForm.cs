using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CapaPresentacion.Proveedores
{
    public partial class ProveedoresForm : Form
    {
        public ProveedoresForm()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            ArmarInterfazMaterial();
        }

        private void ArmarInterfazMaterial()
        {
            Controls.Clear();

            // 1. Tarjeta Contenedora Principal
            MaterialCard cardPrincipal = new MaterialCard
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(15),
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
                Location = new Point(385, 60),
                Height = 48,
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true
            };

            // 4. DataGridView Estilizado para Dark Mode
            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 130),
                Width = cardPrincipal.Width - 40,
                Height = 360,
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
            cardPrincipal.Controls.Add(lblTitulo);
            cardPrincipal.Controls.Add(txtBuscar);
            cardPrincipal.Controls.Add(btnNuevo);
            cardPrincipal.Controls.Add(dgv);

            Controls.Add(cardPrincipal);
        }
    }
}