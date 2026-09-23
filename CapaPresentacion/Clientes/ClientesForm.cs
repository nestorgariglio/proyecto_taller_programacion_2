using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CapaPresentacion.Clientes
{
    public partial class ClientesForm : Form
    {
        public ClientesForm()
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
                Text = "👥 GESTIÓN Y PADRÓN DE CLIENTES",
                FontType = MaterialSkinManager.fontType.H6,
                Location = new Point(20, 15),
                AutoSize = true
            };

            // 3. Controles
            MaterialTextBox2 txtBuscar = new MaterialTextBox2
            {
                Hint = "Buscar por DNI, CUIT o Apellido...",
                Location = new Point(20, 55),
                Width = 350,
                AnimateReadOnly = true
            };

            MaterialButton btnNuevo = new MaterialButton
            {
                Text = "➕ Nuevo Cliente",
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
            dt.Columns.Add("DNI / CUIT");
            dt.Columns.Add("Nombre y Apellido");
            dt.Columns.Add("Teléfono");
            dt.Columns.Add("Correo");
            dt.Columns.Add("Estado");

            dt.Rows.Add("20-3811223-4", "Carlos Gómez", "3624-112233", "cgomez@mail.com", "Activo");
            dt.Rows.Add("27-4099887-1", "María Fernández", "3794-889900", "mfernandez@gmail.com", "Activo");
            dt.Rows.Add("20-1544332-9", "Roberto Rossi", "3624-554433", "rrossi@hotmail.com", "Inactivo");

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