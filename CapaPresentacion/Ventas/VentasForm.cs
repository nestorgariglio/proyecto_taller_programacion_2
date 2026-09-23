using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CapaPresentacion.Ventas
{
    public partial class VentasForm : Form
    {
        public VentasForm()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            ArmarInterfazMaterial();
        }

        private void ArmarInterfazMaterial()
        {
            Controls.Clear();

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));

            // ================= PANAL IZQUIERDO: CARGA =================
            MaterialCard cardCarga = new MaterialCard
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(5)
            };

            MaterialLabel lblIngreso = new MaterialLabel
            {
                Text = "🛒 INGRESO DE PRODUCTOS",
                FontType = MaterialSkinManager.fontType.H6,
                Location = new Point(15, 15),
                AutoSize = true
            };

            MaterialTextBox2 txtCliente = new MaterialTextBox2
            {
                Hint = "DNI Cliente / Consumidor Final",
                Text = "20-3811223-4 - Carlos Gómez",
                Location = new Point(15, 55),
                Width = 320
            };

            MaterialTextBox2 txtCodigo = new MaterialTextBox2
            {
                Hint = "Código de Producto / Barra",
                Location = new Point(15, 125),
                Width = 210
            };

            MaterialTextBox2 txtCantidad = new MaterialTextBox2
            {
                Hint = "Cant.",
                Text = "1",
                Location = new Point(235, 125),
                Width = 100
            };

            MaterialButton btnAgregar = new MaterialButton
            {
                Text = "➕ Agregar al Carrito",
                Location = new Point(15, 195),
                Width = 320,
                Type = MaterialButton.MaterialButtonType.Contained
            };

            cardCarga.Controls.Add(lblIngreso);
            cardCarga.Controls.Add(txtCliente);
            cardCarga.Controls.Add(txtCodigo);
            cardCarga.Controls.Add(txtCantidad);
            cardCarga.Controls.Add(btnAgregar);

            // ================= PANEL DERECHO: CARRITO Y COBRO =================
            MaterialCard cardCarrito = new MaterialCard
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(5)
            };

            MaterialLabel lblCarrito = new MaterialLabel
            {
                Text = "📋 DETALLE DEL CARRITO",
                FontType = MaterialSkinManager.fontType.H6,
                Location = new Point(15, 15),
                AutoSize = true
            };

            DataGridView dgvCarrito = new DataGridView
            {
                Location = new Point(15, 55),
                Width = 380,
                Height = 220,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.FromArgb(40, 40, 40),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false
            };

            dgvCarrito.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            dgvCarrito.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCarrito.ColumnHeadersDefaultCellStyle.Font = new Font("Roboto", 9, FontStyle.Bold);
            dgvCarrito.DefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
            dgvCarrito.DefaultCellStyle.ForeColor = Color.White;

            DataTable dt = new DataTable();
            dt.Columns.Add("Cant.");
            dt.Columns.Add("Producto");
            dt.Columns.Add("P. Unit.");
            dt.Columns.Add("Subtotal");
            dt.Rows.Add(1, "Teclado RGB", "$ 45.000", "$ 45.000");
            dt.Rows.Add(2, "Mouse Inalámbrico", "$ 22.500", "$ 45.000");
            dgvCarrito.DataSource = dt;

            MaterialLabel lblTotal = new MaterialLabel
            {
                Text = "TOTAL: $ 90.000,00",
                FontType = MaterialSkinManager.fontType.H4,
                Location = new Point(15, 290),
                AutoSize = true,
                UseAccent = true
            };

            MaterialButton btnCobrar = new MaterialButton
            {
                Text = "💳 CONFIRMAR Y FACTURAR",
                Location = new Point(15, 345),
                Width = 380,
                Height = 50,
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true
            };

            cardCarrito.Controls.Add(lblCarrito);
            cardCarrito.Controls.Add(dgvCarrito);
            cardCarrito.Controls.Add(lblTotal);
            cardCarrito.Controls.Add(btnCobrar);

            layout.Controls.Add(cardCarga, 0, 0);
            layout.Controls.Add(cardCarrito, 1, 0);

            Controls.Add(layout);
        }
    }
}