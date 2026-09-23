using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CapaPresentacion.Productos
{
    public partial class ProductosForm : Form
    {
        public ProductosForm()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            ArmarInterfazMaterial();
        }

        private void ArmarInterfazMaterial()
        {
            Controls.Clear();

            // 1. Tarjeta Contenedora Principal (Otorga la elevación y bordes de MaterialSkin)
            MaterialCard cardPrincipal = new MaterialCard
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(15),
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

            MaterialComboBox cboCategoria = new MaterialComboBox
            {
                Hint = "Categoría",
                Location = new Point(310, 55),
                Width = 180,
                StartIndex = 0
            };
            cboCategoria.Items.AddRange(new object[] { "Todas", "Periféricos", "Monitores", "Audio" });

            MaterialButton btnNuevo = new MaterialButton
            {
                Text = "➕ Nuevo Producto",
                Location = new Point(505, 60),
                Height = 48,
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true
            };

            // 4. DataGridView Estilizado para Dark Mode de MaterialSkin
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

            // Ensamblar la tarjeta
            cardPrincipal.Controls.Add(lblTitulo);
            cardPrincipal.Controls.Add(txtBuscar);
            cardPrincipal.Controls.Add(cboCategoria);
            cardPrincipal.Controls.Add(btnNuevo);
            cardPrincipal.Controls.Add(dgv);

            Controls.Add(cardPrincipal);
        }
    }
}