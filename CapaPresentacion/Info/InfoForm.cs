using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CapaPresentacion.Info
{
    public partial class InfoForm : Form
    {
        public InfoForm()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            ArmarInterfazMaterial();
        }

        private void ArmarInterfazMaterial()
        {
            Controls.Clear();

            Panel panelScroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(15)
            };

            // --- TARJETA 1: ENCABEZADO ---
            MaterialCard cardHeader = new MaterialCard
            {
                Location = new Point(15, 15),
                Height = 90,
                Width = panelScroll.ClientSize.Width - 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(15)
            };

            MaterialLabel lblTitulo = new MaterialLabel
            {
                Text = "ℹ️ CENTRO DE AYUDA Y GUÍA DEL SISTEMA",
                FontType = MaterialSkinManager.fontType.H6,
                Location = new Point(15, 15),
                AutoSize = true
            };

            MaterialLabel lblSubtitulo = new MaterialLabel
            {
                Text = "Documentación rápida de operación y niveles de acceso (Sistema G70)",
                FontType = MaterialSkinManager.fontType.Body2,
                Location = new Point(15, 48),
                AutoSize = true
            };

            cardHeader.Controls.Add(lblTitulo);
            cardHeader.Controls.Add(lblSubtitulo);

            // --- TARJETA 2: MÓDULOS DEL SISTEMA ---
            MaterialCard cardModulos = new MaterialCard
            {
                Location = new Point(15, 120),
                Height = 200,
                Width = panelScroll.ClientSize.Width - 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(15)
            };

            MaterialLabel lblTitModulos = new MaterialLabel
            {
                Text = "🛒 MÓDULOS OPERATIVOS",
                FontType = MaterialSkinManager.fontType.Subtitle1,
                Location = new Point(15, 15),
                AutoSize = true,
                UseAccent = true
            };

            MaterialLabel lblDescVentas = new MaterialLabel
            {
                Text = "• Ventas (POS): Ingreso rápido de cliente por DNI, búsqueda de productos y cobro.",
                FontType = MaterialSkinManager.fontType.Body1,
                Location = new Point(15, 50),
                AutoSize = true
            };

            MaterialLabel lblDescProductos = new MaterialLabel
            {
                Text = "• Productos: Consulta de catálogo, precios de venta/costo e indicador de stock crítico (< 5 un).",
                FontType = MaterialSkinManager.fontType.Body1,
                Location = new Point(15, 88),
                AutoSize = true
            };

            MaterialLabel lblDescClientes = new MaterialLabel
            {
                Text = "• Clientes & Proveedores: Padrón general para altas, modificaciones y datos de contacto.",
                FontType = MaterialSkinManager.fontType.Body1,
                Location = new Point(15, 126),
                AutoSize = true
            };

            cardModulos.Controls.Add(lblTitModulos);
            cardModulos.Controls.Add(lblDescVentas);
            cardModulos.Controls.Add(lblDescProductos);
            cardModulos.Controls.Add(lblDescClientes);

            // --- TARJETA 3: NIVELES DE ACCESO (RBAC) ---
            MaterialCard cardSeguridad = new MaterialCard
            {
                Location = new Point(15, 335),
                Height = 180,
                Width = panelScroll.ClientSize.Width - 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(15)
            };

            MaterialLabel lblTitSeguridad = new MaterialLabel
            {
                Text = "🛡️ PERMISOS Y ROLES DE USUARIO",
                FontType = MaterialSkinManager.fontType.Subtitle1,
                Location = new Point(15, 15),
                AutoSize = true,
                UseAccent = true
            };

            MaterialLabel lblAdmin = new MaterialLabel
            {
                Text = "• Administrador: Control total del sistema, gestión de usuarios y tableros ejecutivos.",
                FontType = MaterialSkinManager.fontType.Body1,
                Location = new Point(15, 50),
                AutoSize = true
            };

            MaterialLabel lblEncargado = new MaterialLabel
            {
                Text = "• Encargado: Gestión de compras, proveedores e ingreso de stock de mercadería.",
                FontType = MaterialSkinManager.fontType.Body1,
                Location = new Point(15, 88),
                AutoSize = true
            };

            MaterialLabel lblVendedor = new MaterialLabel
            {
                Text = "• Vendedor: Emisión de facturas diarias, consulta de catálogo y registro de clientes.",
                FontType = MaterialSkinManager.fontType.Body1,
                Location = new Point(15, 126),
                AutoSize = true
            };

            cardSeguridad.Controls.Add(lblTitSeguridad);
            cardSeguridad.Controls.Add(lblAdmin);
            cardSeguridad.Controls.Add(lblEncargado);
            cardSeguridad.Controls.Add(lblVendedor);

            panelScroll.Controls.Add(cardHeader);
            panelScroll.Controls.Add(cardModulos);
            panelScroll.Controls.Add(cardSeguridad);

            Controls.Add(panelScroll);
        }
    }
}