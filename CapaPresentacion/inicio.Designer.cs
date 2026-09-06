namespace CapaPresentacion
{
    partial class inicio
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(inicio));
            materialTabControl1 = new MaterialSkin.Controls.MaterialTabControl();
            tab_inicio = new TabPage();
            tab_usuarios = new TabPage();
            tab_productos = new TabPage();
            tab_ventas = new TabPage();
            tab_compras = new TabPage();
            tab_clientes = new TabPage();
            tab_proveedores = new TabPage();
            tab_reportes = new TabPage();
            tab_info = new TabPage();
            tab_salir = new TabPage();
            imageList1 = new ImageList(components);
            materialTabControl1.SuspendLayout();
            SuspendLayout();
            // 
            // materialTabControl1
            // 
            materialTabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            materialTabControl1.Controls.Add(tab_inicio);
            materialTabControl1.Controls.Add(tab_usuarios);
            materialTabControl1.Controls.Add(tab_productos);
            materialTabControl1.Controls.Add(tab_ventas);
            materialTabControl1.Controls.Add(tab_compras);
            materialTabControl1.Controls.Add(tab_clientes);
            materialTabControl1.Controls.Add(tab_proveedores);
            materialTabControl1.Controls.Add(tab_reportes);
            materialTabControl1.Controls.Add(tab_info);
            materialTabControl1.Controls.Add(tab_salir);
            materialTabControl1.Depth = 0;
            materialTabControl1.ImageList = imageList1;
            materialTabControl1.Location = new Point(3, 64);
            materialTabControl1.MouseState = MaterialSkin.MouseState.HOVER;
            materialTabControl1.Multiline = true;
            materialTabControl1.Name = "materialTabControl1";
            materialTabControl1.SelectedIndex = 0;
            materialTabControl1.Size = new Size(1091, 586);
            materialTabControl1.TabIndex = 0;
            materialTabControl1.TabStop = false;
            materialTabControl1.SelectedIndexChanged += materialTabControl1_SelectedIndexChanged;
            // 
            // tab_inicio
            // 
            tab_inicio.ImageKey = "home.png";
            tab_inicio.Location = new Point(4, 84);
            tab_inicio.Name = "tab_inicio";
            tab_inicio.Padding = new Padding(3);
            tab_inicio.Size = new Size(1083, 498);
            tab_inicio.TabIndex = 0;
            tab_inicio.Text = "Inicio";
            tab_inicio.UseVisualStyleBackColor = true;
            // 
            // tab_usuarios
            // 
            tab_usuarios.ImageKey = "users.png";
            tab_usuarios.Location = new Point(4, 44);
            tab_usuarios.Name = "tab_usuarios";
            tab_usuarios.Padding = new Padding(3);
            tab_usuarios.Size = new Size(1083, 538);
            tab_usuarios.TabIndex = 1;
            tab_usuarios.Text = "Usuarios";
            tab_usuarios.UseVisualStyleBackColor = true;
            // 
            // tab_productos
            // 
            tab_productos.ImageKey = "products.png";
            tab_productos.Location = new Point(4, 44);
            tab_productos.Name = "tab_productos";
            tab_productos.Padding = new Padding(3);
            tab_productos.Size = new Size(1083, 538);
            tab_productos.TabIndex = 2;
            tab_productos.Text = "Productos";
            tab_productos.UseVisualStyleBackColor = true;
            // 
            // tab_ventas
            // 
            tab_ventas.ImageKey = "sales.png";
            tab_ventas.Location = new Point(4, 44);
            tab_ventas.Name = "tab_ventas";
            tab_ventas.Padding = new Padding(3);
            tab_ventas.Size = new Size(1083, 538);
            tab_ventas.TabIndex = 3;
            tab_ventas.Text = "Ventas";
            tab_ventas.UseVisualStyleBackColor = true;
            // 
            // tab_compras
            // 
            tab_compras.ImageKey = "shopping.png";
            tab_compras.Location = new Point(4, 44);
            tab_compras.Name = "tab_compras";
            tab_compras.Padding = new Padding(3);
            tab_compras.Size = new Size(1083, 538);
            tab_compras.TabIndex = 4;
            tab_compras.Text = "Compras";
            tab_compras.UseVisualStyleBackColor = true;
            // 
            // tab_clientes
            // 
            tab_clientes.ImageKey = "customers.png";
            tab_clientes.Location = new Point(4, 44);
            tab_clientes.Name = "tab_clientes";
            tab_clientes.Padding = new Padding(3);
            tab_clientes.Size = new Size(1083, 538);
            tab_clientes.TabIndex = 5;
            tab_clientes.Text = "Clientes";
            tab_clientes.UseVisualStyleBackColor = true;
            // 
            // tab_proveedores
            // 
            tab_proveedores.ImageKey = "supplier.png";
            tab_proveedores.Location = new Point(4, 44);
            tab_proveedores.Name = "tab_proveedores";
            tab_proveedores.Padding = new Padding(3);
            tab_proveedores.Size = new Size(1083, 538);
            tab_proveedores.TabIndex = 6;
            tab_proveedores.Text = "Proveedores";
            tab_proveedores.UseVisualStyleBackColor = true;
            // 
            // tab_reportes
            // 
            tab_reportes.ImageKey = "charts.png";
            tab_reportes.Location = new Point(4, 84);
            tab_reportes.Name = "tab_reportes";
            tab_reportes.Padding = new Padding(3);
            tab_reportes.Size = new Size(1083, 498);
            tab_reportes.TabIndex = 7;
            tab_reportes.Text = "Reportes";
            tab_reportes.UseVisualStyleBackColor = true;
            // 
            // tab_info
            // 
            tab_info.ImageKey = "info.png";
            tab_info.Location = new Point(4, 84);
            tab_info.Name = "tab_info";
            tab_info.Padding = new Padding(3);
            tab_info.Size = new Size(1083, 498);
            tab_info.TabIndex = 8;
            tab_info.Text = "Info";
            tab_info.UseVisualStyleBackColor = true;
            // 
            // tab_salir
            // 
            tab_salir.ImageKey = "off.png";
            tab_salir.Location = new Point(4, 84);
            tab_salir.Name = "tab_salir";
            tab_salir.Padding = new Padding(3);
            tab_salir.Size = new Size(1083, 498);
            tab_salir.TabIndex = 9;
            tab_salir.Text = "Salir";
            tab_salir.UseVisualStyleBackColor = true;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = Color.Transparent;
            imageList1.Images.SetKeyName(0, "customers.png");
            imageList1.Images.SetKeyName(1, "home.png");
            imageList1.Images.SetKeyName(2, "info.png");
            imageList1.Images.SetKeyName(3, "products.png");
            imageList1.Images.SetKeyName(4, "sales.png");
            imageList1.Images.SetKeyName(5, "shopping.png");
            imageList1.Images.SetKeyName(6, "supplier.png");
            imageList1.Images.SetKeyName(7, "users.png");
            imageList1.Images.SetKeyName(8, "charts.png");
            imageList1.Images.SetKeyName(9, "off.png");
            // 
            // inicio
            // 
            AutoScaleDimensions = new SizeF(14F, 35F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1097, 653);
            Controls.Add(materialTabControl1);
            DrawerShowIconsWhenHidden = true;
            DrawerTabControl = materialTabControl1;
            Font = new Font("Segoe UI", 15F);
            Margin = new Padding(5, 6, 5, 6);
            Name = "inicio";
            Text = "Sistema de Ventas";
            Shown += inicio_Shown;
            materialTabControl1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private MaterialSkin.Controls.MaterialTabControl materialTabControl1;
        private TabPage tab_inicio;
        private TabPage tab_usuarios;
        private ImageList imageList1;
        private TabPage tab_productos;
        private TabPage tab_ventas;
        private TabPage tab_compras;
        private TabPage tab_clientes;
        private TabPage tab_proveedores;
        private TabPage tab_reportes;
        private TabPage tab_info;
        private TabPage tab_salir;
    }
}
