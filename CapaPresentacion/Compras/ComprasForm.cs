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

namespace CapaPresentacion.Compras
{
    public partial class ComprasForm : UserControl
    {
        public ComprasForm()
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
                Text = "📑 REGISTRO DE COMPRAS Y RECEPCIÓN DE STOCK",
                FontType = MaterialSkinManager.fontType.H6,
                Location = new Point(20, 15),
                AutoSize = true
            };

            // 3. Controles
            MaterialComboBox cboProveedor = new MaterialComboBox
            {
                Hint = "Seleccionar Proveedor",
                Location = new Point(20, 55),
                Width = 260,
                StartIndex = 0
            };
            cboProveedor.Items.AddRange(new object[] { "Logitech Argentina S.A.", "Samsung Elec. Arg.", "Distribuidora Norte SRL" });

            MaterialTextBox2 txtFactura = new MaterialTextBox2
            {
                Hint = "N° Factura / Comprobante",
                Location = new Point(290, 55),
                Width = 220
            };

            MaterialButton btnRegistrar = new MaterialButton
            {
                Text = "📥 Nueva Orden de Compra",
                Location = new Point(520, 60),
                Height = 48,
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true
            };

            // 4. DataGridView Estilizado para Dark Mode
            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 130),
                Width = panelPrincipal.Width - 40,
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
            dt.Columns.Add("Proveedor");
            dt.Columns.Add("N° Factura");
            dt.Columns.Add("Fecha");
            dt.Columns.Add("Total");
            dt.Columns.Add("Estado");

            dt.Rows.Add("LogiTech Argentina S.A.", "FC-0001-4452", "20/09/2026", "$ 680.000", "Recibido");
            dt.Rows.Add("Samsung Elec. Arg.", "FC-0002-8810", "18/09/2026", "$ 1.250.000", "Recibido");

            dgv.DataSource = dt;

            // Ensamblar
            panelPrincipal.Controls.Add(lblTitulo);
            panelPrincipal.Controls.Add(cboProveedor);
            panelPrincipal.Controls.Add(txtFactura);
            panelPrincipal.Controls.Add(btnRegistrar);
            panelPrincipal.Controls.Add(dgv);

            Controls.Add(panelPrincipal);
        }
    }
}