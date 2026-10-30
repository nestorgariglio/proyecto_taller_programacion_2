namespace CapaPresentacion.Compras
{
    partial class ComprasForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tblPrincipal = new TableLayoutPanel();
            pnlContenedor = new Panel();
            pnlEncabezado = new Panel();
            lblTitulo = new MaterialSkin.Controls.MaterialLabel();
            tblBloques = new TableLayoutPanel();
            tblBloqueProducto = new TableLayoutPanel();
            pnlDatosProducto = new Panel();
            tblDatosProducto = new TableLayoutPanel();
            lblTituloDatosProducto = new MaterialSkin.Controls.MaterialLabel();
            tblProductoSeleccionado = new TableLayoutPanel();
            lblNombreProducto = new MaterialSkin.Controls.MaterialLabel();
            lblCodigoProducto = new MaterialSkin.Controls.MaterialLabel();
            tblCaptura = new TableLayoutPanel();
            txtCantidad = new MaterialSkin.Controls.MaterialTextBox2();
            txtPrecioCompra = new MaterialSkin.Controls.MaterialTextBox2();
            txtPrecioVenta = new MaterialSkin.Controls.MaterialTextBox2();
            tblSubtotalAccion = new TableLayoutPanel();
            lblSubtotalPreparacion = new MaterialSkin.Controls.MaterialLabel();
            btnAgregarLinea = new MaterialSkin.Controls.MaterialButton();
            pnlListadoProductos = new Panel();
            tblListadoProductos = new TableLayoutPanel();
            lblTituloListadoProductos = new MaterialSkin.Controls.MaterialLabel();
            txtBuscarProducto = new MaterialSkin.Controls.MaterialTextBox2();
            gridProductos = new DataGridView();
            colProductoCodigo = new DataGridViewTextBoxColumn();
            colProductoNombre = new DataGridViewTextBoxColumn();
            lblMensajeProductos = new MaterialSkin.Controls.MaterialLabel();
            tblBloqueProveedor = new TableLayoutPanel();
            pnlDatosProveedor = new Panel();
            tblDatosProveedor = new TableLayoutPanel();
            lblTituloDatosProveedor = new MaterialSkin.Controls.MaterialLabel();
            lblRazonSocialProveedor = new MaterialSkin.Controls.MaterialLabel();
            lblDocumentoProveedor = new MaterialSkin.Controls.MaterialLabel();
            pnlListadoProveedores = new Panel();
            tblListadoProveedores = new TableLayoutPanel();
            lblTituloListadoProveedores = new MaterialSkin.Controls.MaterialLabel();
            txtBuscarProveedor = new MaterialSkin.Controls.MaterialTextBox2();
            gridProveedores = new DataGridView();
            colProveedorRazonSocial = new DataGridViewTextBoxColumn();
            colProveedorDocumento = new DataGridViewTextBoxColumn();
            lblMensajeProveedores = new MaterialSkin.Controls.MaterialLabel();
            tblCompraActual = new TableLayoutPanel();
            lblTituloCompraActual = new MaterialSkin.Controls.MaterialLabel();
            gridLineas = new DataGridView();
            colLineaProducto = new DataGridViewTextBoxColumn();
            colLineaCantidad = new DataGridViewTextBoxColumn();
            colLineaPrecioCompra = new DataGridViewTextBoxColumn();
            colLineaPrecioVenta = new DataGridViewTextBoxColumn();
            colLineaSubtotal = new DataGridViewTextBoxColumn();
            colLineaEditar = new DataGridViewButtonColumn();
            colLineaQuitar = new DataGridViewButtonColumn();
            tblPieCompra = new TableLayoutPanel();
            lblMontoTotal = new MaterialSkin.Controls.MaterialLabel();
            lblMensaje = new MaterialSkin.Controls.MaterialLabel();
            flpAcciones = new FlowLayoutPanel();
            btnRegistrar = new MaterialSkin.Controls.MaterialButton();
            btnLimpiar = new MaterialSkin.Controls.MaterialButton();
            btnVerHistorial = new MaterialSkin.Controls.MaterialButton();
            tblPrincipal.SuspendLayout();
            pnlContenedor.SuspendLayout();
            pnlEncabezado.SuspendLayout();
            tblBloques.SuspendLayout();
            tblBloqueProducto.SuspendLayout();
            pnlDatosProducto.SuspendLayout();
            tblDatosProducto.SuspendLayout();
            tblProductoSeleccionado.SuspendLayout();
            tblCaptura.SuspendLayout();
            tblSubtotalAccion.SuspendLayout();
            pnlListadoProductos.SuspendLayout();
            tblListadoProductos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridProductos).BeginInit();
            tblBloqueProveedor.SuspendLayout();
            pnlDatosProveedor.SuspendLayout();
            tblDatosProveedor.SuspendLayout();
            pnlListadoProveedores.SuspendLayout();
            tblListadoProveedores.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridProveedores).BeginInit();
            tblCompraActual.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridLineas).BeginInit();
            tblPieCompra.SuspendLayout();
            flpAcciones.SuspendLayout();
            SuspendLayout();
            //
            // pnlContenedor
            //
            pnlContenedor.AutoScroll = true;
            pnlContenedor.AutoScrollMinSize = new Size(0, 650);
            pnlContenedor.BackColor = Color.FromArgb(33, 33, 33);
            pnlContenedor.Controls.Add(tblPrincipal);
            pnlContenedor.Dock = DockStyle.Fill;
            pnlContenedor.Location = new Point(0, 0);
            pnlContenedor.Margin = new Padding(0);
            pnlContenedor.Name = "pnlContenedor";
            pnlContenedor.Padding = new Padding(12);
            pnlContenedor.Size = new Size(1083, 508);
            pnlContenedor.TabIndex = 0;
            //
            // tblPrincipal
            //
            tblPrincipal.AutoSize = false;
            tblPrincipal.ColumnCount = 1;
            tblPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblPrincipal.Controls.Add(pnlEncabezado, 0, 0);
            tblPrincipal.Controls.Add(tblBloques, 0, 1);
            tblPrincipal.Controls.Add(tblCompraActual, 0, 2);
            tblPrincipal.Controls.Add(flpAcciones, 0, 3);
            tblPrincipal.Dock = DockStyle.Fill;
            tblPrincipal.Location = new Point(12, 12);
            tblPrincipal.Margin = new Padding(0);
            tblPrincipal.MinimumSize = new Size(980, 650);
            tblPrincipal.Name = "tblPrincipal";
            tblPrincipal.Padding = new Padding(0);
            tblPrincipal.RowCount = 4;
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 270F));
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));
            tblPrincipal.Size = new Size(980, 650);
            tblPrincipal.TabIndex = 0;
            //
            // pnlEncabezado
            //
            pnlEncabezado.Controls.Add(lblTitulo);
            pnlEncabezado.Dock = DockStyle.Fill;
            pnlEncabezado.Location = new Point(0, 0);
            pnlEncabezado.Margin = new Padding(0);
            pnlEncabezado.Name = "pnlEncabezado";
            pnlEncabezado.Size = new Size(980, 35);
            pnlEncabezado.TabIndex = 0;
            //
            // lblTitulo
            //
            lblTitulo.AutoSize = false;
            lblTitulo.Depth = 0;
            lblTitulo.Dock = DockStyle.Fill;
            lblTitulo.Font = new Font("Roboto", 24F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblTitulo.Location = new Point(0, 0);
            lblTitulo.MouseState = MaterialSkin.MouseState.HOVER;
            lblTitulo.Name = "lblTitulo";
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Registro de Compras a Proveedores";
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            //
            // tblBloques
            //
            tblBloques.ColumnCount = 2;
            tblBloques.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));
            tblBloques.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));
            tblBloques.Controls.Add(tblBloqueProducto, 0, 0);
            tblBloques.Controls.Add(tblBloqueProveedor, 1, 0);
            tblBloques.Dock = DockStyle.Fill;
            tblBloques.Location = new Point(0, 35);
            tblBloques.Margin = new Padding(0);
            tblBloques.Name = "tblBloques";
            tblBloques.RowCount = 1;
            tblBloques.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblBloques.Size = new Size(980, 270);
            tblBloques.TabIndex = 1;
            //
            // tblBloqueProducto
            //
            tblBloqueProducto.ColumnCount = 1;
            tblBloqueProducto.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblBloqueProducto.Controls.Add(pnlDatosProducto, 0, 0);
            tblBloqueProducto.Controls.Add(pnlListadoProductos, 0, 1);
            tblBloqueProducto.Dock = DockStyle.Fill;
            tblBloqueProducto.Location = new Point(0, 0);
            tblBloqueProducto.Margin = new Padding(0, 0, 8, 0);
            tblBloqueProducto.Name = "tblBloqueProducto";
            tblBloqueProducto.RowCount = 2;
            tblBloqueProducto.RowStyles.Add(new RowStyle(SizeType.Absolute, 136F));
            tblBloqueProducto.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblBloqueProducto.Size = new Size(502, 270);
            tblBloqueProducto.TabIndex = 0;
            //
            // pnlDatosProducto
            //
            pnlDatosProducto.BackColor = Color.FromArgb(45, 45, 45);
            pnlDatosProducto.Controls.Add(tblDatosProducto);
            pnlDatosProducto.Dock = DockStyle.Fill;
            pnlDatosProducto.Location = new Point(0, 0);
            pnlDatosProducto.Margin = new Padding(0);
            pnlDatosProducto.Name = "pnlDatosProducto";
            pnlDatosProducto.Padding = new Padding(10, 2, 10, 2);
            pnlDatosProducto.Size = new Size(502, 136);
            pnlDatosProducto.TabIndex = 0;
            //
            // tblDatosProducto
            //
            tblDatosProducto.ColumnCount = 1;
            tblDatosProducto.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblDatosProducto.Controls.Add(lblTituloDatosProducto, 0, 0);
            tblDatosProducto.Controls.Add(tblProductoSeleccionado, 0, 1);
            tblDatosProducto.Controls.Add(tblCaptura, 0, 2);
            tblDatosProducto.Controls.Add(tblSubtotalAccion, 0, 3);
            tblDatosProducto.Dock = DockStyle.Fill;
            tblDatosProducto.Location = new Point(10, 2);
            tblDatosProducto.Margin = new Padding(0);
            tblDatosProducto.Name = "tblDatosProducto";
            tblDatosProducto.RowCount = 4;
            tblDatosProducto.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tblDatosProducto.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tblDatosProducto.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            tblDatosProducto.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            tblDatosProducto.Size = new Size(482, 132);
            tblDatosProducto.TabIndex = 0;
            //
            // lblTituloDatosProducto
            //
            lblTituloDatosProducto.AutoSize = false;
            lblTituloDatosProducto.Depth = 0;
            lblTituloDatosProducto.Dock = DockStyle.Fill;
            lblTituloDatosProducto.Font = new Font("Roboto", 18F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblTituloDatosProducto.Location = new Point(0, 0);
            lblTituloDatosProducto.Margin = new Padding(0);
            lblTituloDatosProducto.MouseState = MaterialSkin.MouseState.HOVER;
            lblTituloDatosProducto.Name = "lblTituloDatosProducto";
            lblTituloDatosProducto.Size = new Size(482, 20);
            lblTituloDatosProducto.TabIndex = 0;
            lblTituloDatosProducto.Text = "Datos del producto";
            lblTituloDatosProducto.TextAlign = ContentAlignment.MiddleLeft;
            //
            // tblProductoSeleccionado
            //
            tblProductoSeleccionado.ColumnCount = 2;
            tblProductoSeleccionado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));
            tblProductoSeleccionado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
            tblProductoSeleccionado.Controls.Add(lblNombreProducto, 0, 0);
            tblProductoSeleccionado.Controls.Add(lblCodigoProducto, 1, 0);
            tblProductoSeleccionado.Dock = DockStyle.Fill;
            tblProductoSeleccionado.Location = new Point(0, 20);
            tblProductoSeleccionado.Margin = new Padding(0);
            tblProductoSeleccionado.Name = "tblProductoSeleccionado";
            tblProductoSeleccionado.RowCount = 1;
            tblProductoSeleccionado.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblProductoSeleccionado.Size = new Size(482, 20);
            tblProductoSeleccionado.TabIndex = 1;
            //
            // lblNombreProducto
            //
            lblNombreProducto.AutoSize = false;
            lblNombreProducto.Depth = 0;
            lblNombreProducto.Dock = DockStyle.Fill;
            lblNombreProducto.Font = new Font("Roboto", 15F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblNombreProducto.Location = new Point(0, 0);
            lblNombreProducto.Margin = new Padding(0);
            lblNombreProducto.MouseState = MaterialSkin.MouseState.HOVER;
            lblNombreProducto.Name = "lblNombreProducto";
            lblNombreProducto.Size = new Size(298, 20);
            lblNombreProducto.TabIndex = 0;
            lblNombreProducto.Text = "(Sin producto seleccionado)";
            lblNombreProducto.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblCodigoProducto
            //
            lblCodigoProducto.AutoSize = false;
            lblCodigoProducto.Depth = 0;
            lblCodigoProducto.Dock = DockStyle.Fill;
            lblCodigoProducto.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblCodigoProducto.Location = new Point(298, 0);
            lblCodigoProducto.Margin = new Padding(0);
            lblCodigoProducto.MouseState = MaterialSkin.MouseState.HOVER;
            lblCodigoProducto.Name = "lblCodigoProducto";
            lblCodigoProducto.Size = new Size(184, 20);
            lblCodigoProducto.TabIndex = 1;
            lblCodigoProducto.Text = "Código: —";
            lblCodigoProducto.TextAlign = ContentAlignment.MiddleLeft;
            //
            // tblCaptura
            //
            tblCaptura.ColumnCount = 3;
            tblCaptura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tblCaptura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tblCaptura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tblCaptura.Controls.Add(txtCantidad, 0, 0);
            tblCaptura.Controls.Add(txtPrecioCompra, 1, 0);
            tblCaptura.Controls.Add(txtPrecioVenta, 2, 0);
            tblCaptura.Dock = DockStyle.Fill;
            tblCaptura.Location = new Point(0, 40);
            tblCaptura.Margin = new Padding(0);
            tblCaptura.Name = "tblCaptura";
            tblCaptura.RowCount = 1;
            tblCaptura.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblCaptura.Size = new Size(482, 48);
            tblCaptura.TabIndex = 2;
            //
            // txtCantidad
            //
            txtCantidad.AnimateReadOnly = false;
            txtCantidad.BackgroundImageLayout = ImageLayout.None;
            txtCantidad.CharacterCasing = CharacterCasing.Normal;
            txtCantidad.Depth = 0;
            txtCantidad.Dock = DockStyle.Fill;
            txtCantidad.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtCantidad.Hint = "Cantidad";
            txtCantidad.Location = new Point(0, 0);
            txtCantidad.Margin = new Padding(0, 0, 6, 0);
            txtCantidad.MaxLength = 32767;
            txtCantidad.MinimumSize = new Size(100, 48);
            txtCantidad.MouseState = MaterialSkin.MouseState.OUT;
            txtCantidad.Name = "txtCantidad";
            txtCantidad.Padding = new Padding(2, 0, 2, 0);
            txtCantidad.PasswordChar = '\0';
            txtCantidad.PrefixSuffixText = null;
            txtCantidad.ReadOnly = false;
            txtCantidad.RightToLeft = RightToLeft.No;
            txtCantidad.SelectedText = "";
            txtCantidad.SelectionLength = 0;
            txtCantidad.SelectionStart = 0;
            txtCantidad.ShortcutsEnabled = true;
            txtCantidad.TabIndex = 0;
            txtCantidad.TextAlign = HorizontalAlignment.Left;
            txtCantidad.TrailingIcon = null;
            txtCantidad.UseSystemPasswordChar = false;
            //
            // txtPrecioCompra
            //
            txtPrecioCompra.AnimateReadOnly = false;
            txtPrecioCompra.BackgroundImageLayout = ImageLayout.None;
            txtPrecioCompra.CharacterCasing = CharacterCasing.Normal;
            txtPrecioCompra.Depth = 0;
            txtPrecioCompra.Dock = DockStyle.Fill;
            txtPrecioCompra.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtPrecioCompra.Hint = "Precio compra";
            txtPrecioCompra.Location = new Point(144, 0);
            txtPrecioCompra.Margin = new Padding(0, 0, 6, 0);
            txtPrecioCompra.MaxLength = 32767;
            txtPrecioCompra.MinimumSize = new Size(110, 48);
            txtPrecioCompra.MouseState = MaterialSkin.MouseState.OUT;
            txtPrecioCompra.Name = "txtPrecioCompra";
            txtPrecioCompra.Padding = new Padding(2, 0, 2, 0);
            txtPrecioCompra.PasswordChar = '\0';
            txtPrecioCompra.PrefixSuffixText = null;
            txtPrecioCompra.ReadOnly = false;
            txtPrecioCompra.RightToLeft = RightToLeft.No;
            txtPrecioCompra.SelectedText = "";
            txtPrecioCompra.SelectionLength = 0;
            txtPrecioCompra.SelectionStart = 0;
            txtPrecioCompra.ShortcutsEnabled = true;
            txtPrecioCompra.TabIndex = 1;
            txtPrecioCompra.TextAlign = HorizontalAlignment.Left;
            txtPrecioCompra.TrailingIcon = null;
            txtPrecioCompra.UseSystemPasswordChar = false;
            //
            // txtPrecioVenta
            //
            txtPrecioVenta.AnimateReadOnly = false;
            txtPrecioVenta.BackgroundImageLayout = ImageLayout.None;
            txtPrecioVenta.CharacterCasing = CharacterCasing.Normal;
            txtPrecioVenta.Depth = 0;
            txtPrecioVenta.Dock = DockStyle.Fill;
            txtPrecioVenta.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtPrecioVenta.Hint = "Precio venta";
            txtPrecioVenta.Location = new Point(312, 0);
            txtPrecioVenta.Margin = new Padding(0);
            txtPrecioVenta.MaxLength = 32767;
            txtPrecioVenta.MinimumSize = new Size(110, 48);
            txtPrecioVenta.MouseState = MaterialSkin.MouseState.OUT;
            txtPrecioVenta.Name = "txtPrecioVenta";
            txtPrecioVenta.Padding = new Padding(2, 0, 2, 0);
            txtPrecioVenta.PasswordChar = '\0';
            txtPrecioVenta.PrefixSuffixText = null;
            txtPrecioVenta.ReadOnly = false;
            txtPrecioVenta.RightToLeft = RightToLeft.No;
            txtPrecioVenta.SelectedText = "";
            txtPrecioVenta.SelectionLength = 0;
            txtPrecioVenta.SelectionStart = 0;
            txtPrecioVenta.ShortcutsEnabled = true;
            txtPrecioVenta.TabIndex = 2;
            txtPrecioVenta.TextAlign = HorizontalAlignment.Left;
            txtPrecioVenta.TrailingIcon = null;
            txtPrecioVenta.UseSystemPasswordChar = false;
            //
            // tblSubtotalAccion
            //
            tblSubtotalAccion.ColumnCount = 2;
            tblSubtotalAccion.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblSubtotalAccion.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblSubtotalAccion.Controls.Add(lblSubtotalPreparacion, 0, 0);
            tblSubtotalAccion.Controls.Add(btnAgregarLinea, 1, 0);
            tblSubtotalAccion.Dock = DockStyle.Fill;
            tblSubtotalAccion.Location = new Point(0, 88);
            tblSubtotalAccion.Margin = new Padding(0);
            tblSubtotalAccion.Name = "tblSubtotalAccion";
            tblSubtotalAccion.RowCount = 1;
            tblSubtotalAccion.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblSubtotalAccion.Size = new Size(482, 44);
            tblSubtotalAccion.TabIndex = 3;
            //
            // lblSubtotalPreparacion
            //
            lblSubtotalPreparacion.AutoSize = false;
            lblSubtotalPreparacion.Depth = 0;
            lblSubtotalPreparacion.Dock = DockStyle.Fill;
            lblSubtotalPreparacion.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblSubtotalPreparacion.Location = new Point(0, 0);
            lblSubtotalPreparacion.Margin = new Padding(0);
            lblSubtotalPreparacion.MouseState = MaterialSkin.MouseState.HOVER;
            lblSubtotalPreparacion.Name = "lblSubtotalPreparacion";
            lblSubtotalPreparacion.Size = new Size(272, 44);
            lblSubtotalPreparacion.TabIndex = 0;
            lblSubtotalPreparacion.Text = "Subtotal: —";
            lblSubtotalPreparacion.TextAlign = ContentAlignment.MiddleLeft;
            //
            // btnAgregarLinea
            //
            btnAgregarLinea.AutoSize = true;
            btnAgregarLinea.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnAgregarLinea.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnAgregarLinea.Depth = 0;
            btnAgregarLinea.HighEmphasis = true;
            btnAgregarLinea.Icon = null;
            btnAgregarLinea.Location = new Point(276, 2);
            btnAgregarLinea.Margin = new Padding(4, 2, 4, 2);
            btnAgregarLinea.MinimumSize = new Size(0, 38);
            btnAgregarLinea.MouseState = MaterialSkin.MouseState.HOVER;
            btnAgregarLinea.Name = "btnAgregarLinea";
            btnAgregarLinea.NoAccentTextColor = Color.Empty;
            btnAgregarLinea.Padding = new Padding(10, 0, 10, 0);
            btnAgregarLinea.Size = new Size(202, 38);
            btnAgregarLinea.TabIndex = 1;
            btnAgregarLinea.Text = "Agregar a la compra";
            btnAgregarLinea.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnAgregarLinea.UseAccentColor = true;
            btnAgregarLinea.UseVisualStyleBackColor = true;
            //
            // pnlListadoProductos
            //
            pnlListadoProductos.BackColor = Color.FromArgb(45, 45, 45);
            pnlListadoProductos.Controls.Add(tblListadoProductos);
            pnlListadoProductos.Dock = DockStyle.Fill;
            pnlListadoProductos.Location = new Point(0, 136);
            pnlListadoProductos.Margin = new Padding(0);
            pnlListadoProductos.Name = "pnlListadoProductos";
            pnlListadoProductos.Padding = new Padding(10, 2, 10, 2);
            pnlListadoProductos.Size = new Size(502, 134);
            pnlListadoProductos.TabIndex = 1;
            //
            // tblListadoProductos
            //
            tblListadoProductos.ColumnCount = 2;
            tblListadoProductos.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblListadoProductos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblListadoProductos.Controls.Add(lblTituloListadoProductos, 0, 0);
            tblListadoProductos.Controls.Add(txtBuscarProducto, 1, 0);
            tblListadoProductos.Controls.Add(gridProductos, 0, 1);
            tblListadoProductos.Controls.Add(lblMensajeProductos, 0, 2);
            tblListadoProductos.Dock = DockStyle.Fill;
            tblListadoProductos.Location = new Point(10, 2);
            tblListadoProductos.Margin = new Padding(0);
            tblListadoProductos.Name = "tblListadoProductos";
            tblListadoProductos.RowCount = 3;
            tblListadoProductos.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            tblListadoProductos.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblListadoProductos.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
            tblListadoProductos.SetColumnSpan(gridProductos, 2);
            tblListadoProductos.SetColumnSpan(lblMensajeProductos, 2);
            tblListadoProductos.Size = new Size(482, 130);
            tblListadoProductos.TabIndex = 0;
            //
            // lblTituloListadoProductos
            //
            lblTituloListadoProductos.Anchor = AnchorStyles.Left;
            lblTituloListadoProductos.AutoSize = true;
            lblTituloListadoProductos.Depth = 0;
            lblTituloListadoProductos.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblTituloListadoProductos.Location = new Point(0, 14);
            lblTituloListadoProductos.Margin = new Padding(0, 0, 12, 0);
            lblTituloListadoProductos.MouseState = MaterialSkin.MouseState.HOVER;
            lblTituloListadoProductos.Name = "lblTituloListadoProductos";
            lblTituloListadoProductos.Size = new Size(59, 16);
            lblTituloListadoProductos.TabIndex = 0;
            lblTituloListadoProductos.Text = "Listado productos";
            lblTituloListadoProductos.TextAlign = ContentAlignment.MiddleLeft;
            //
            // txtBuscarProducto
            //
            txtBuscarProducto.AnimateReadOnly = false;
            txtBuscarProducto.BackgroundImageLayout = ImageLayout.None;
            txtBuscarProducto.CharacterCasing = CharacterCasing.Normal;
            txtBuscarProducto.Depth = 0;
            txtBuscarProducto.Dock = DockStyle.Fill;
            txtBuscarProducto.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtBuscarProducto.Hint = "Buscar producto";
            txtBuscarProducto.Location = new Point(71, 0);
            txtBuscarProducto.Margin = new Padding(0);
            txtBuscarProducto.MaxLength = 32767;
            txtBuscarProducto.MinimumSize = new Size(200, 44);
            txtBuscarProducto.MouseState = MaterialSkin.MouseState.OUT;
            txtBuscarProducto.Name = "txtBuscarProducto";
            txtBuscarProducto.Padding = new Padding(2, 0, 2, 0);
            txtBuscarProducto.PasswordChar = '\0';
            txtBuscarProducto.PrefixSuffixText = null;
            txtBuscarProducto.ReadOnly = false;
            txtBuscarProducto.RightToLeft = RightToLeft.No;
            txtBuscarProducto.SelectedText = "";
            txtBuscarProducto.SelectionLength = 0;
            txtBuscarProducto.SelectionStart = 0;
            txtBuscarProducto.ShortcutsEnabled = true;
            txtBuscarProducto.TabIndex = 1;
            txtBuscarProducto.TextAlign = HorizontalAlignment.Left;
            txtBuscarProducto.TrailingIcon = null;
            txtBuscarProducto.UseSystemPasswordChar = false;
            //
            // gridProductos
            //
            gridProductos.AllowUserToAddRows = false;
            gridProductos.AllowUserToDeleteRows = false;
            gridProductos.AllowUserToResizeRows = false;
            gridProductos.AutoGenerateColumns = false;
            gridProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridProductos.ColumnHeadersHeight = 26;
            gridProductos.Columns.AddRange(new DataGridViewColumn[] { colProductoCodigo, colProductoNombre });
            gridProductos.Dock = DockStyle.Fill;
            gridProductos.Location = new Point(0, 44);
            gridProductos.Margin = new Padding(0);
            gridProductos.MultiSelect = false;
            gridProductos.Name = "gridProductos";
            gridProductos.ReadOnly = true;
            gridProductos.RowHeadersVisible = false;
            gridProductos.RowTemplate.Height = 21;
            gridProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridProductos.Size = new Size(482, 68);
            gridProductos.TabIndex = 2;
            //
            // colProductoCodigo
            //
            colProductoCodigo.DataPropertyName = "Codigo";
            colProductoCodigo.FillWeight = 30F;
            colProductoCodigo.HeaderText = "Código";
            colProductoCodigo.Name = "colProductoCodigo";
            colProductoCodigo.ReadOnly = true;
            colProductoCodigo.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colProductoNombre
            //
            colProductoNombre.DataPropertyName = "Nombre";
            colProductoNombre.FillWeight = 70F;
            colProductoNombre.HeaderText = "Nombre";
            colProductoNombre.Name = "colProductoNombre";
            colProductoNombre.ReadOnly = true;
            colProductoNombre.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // lblMensajeProductos
            //
            lblMensajeProductos.AutoSize = false;
            lblMensajeProductos.Depth = 0;
            lblMensajeProductos.Dock = DockStyle.Fill;
            lblMensajeProductos.Font = new Font("Roboto", 13F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblMensajeProductos.Location = new Point(0, 112);
            lblMensajeProductos.Margin = new Padding(0);
            lblMensajeProductos.MouseState = MaterialSkin.MouseState.HOVER;
            lblMensajeProductos.Name = "lblMensajeProductos";
            lblMensajeProductos.Size = new Size(482, 18);
            lblMensajeProductos.TabIndex = 3;
            lblMensajeProductos.TextAlign = ContentAlignment.MiddleLeft;
            //
            // tblBloqueProveedor
            //
            tblBloqueProveedor.ColumnCount = 1;
            tblBloqueProveedor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblBloqueProveedor.Controls.Add(pnlDatosProveedor, 0, 0);
            tblBloqueProveedor.Controls.Add(pnlListadoProveedores, 0, 1);
            tblBloqueProveedor.Dock = DockStyle.Fill;
            tblBloqueProveedor.Location = new Point(510, 0);
            tblBloqueProveedor.Margin = new Padding(0);
            tblBloqueProveedor.Name = "tblBloqueProveedor";
            tblBloqueProveedor.RowCount = 2;
            tblBloqueProveedor.RowStyles.Add(new RowStyle(SizeType.Absolute, 136F));
            tblBloqueProveedor.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblBloqueProveedor.Size = new Size(470, 270);
            tblBloqueProveedor.TabIndex = 1;
            //
            // pnlDatosProveedor
            //
            pnlDatosProveedor.BackColor = Color.FromArgb(45, 45, 45);
            pnlDatosProveedor.Controls.Add(tblDatosProveedor);
            pnlDatosProveedor.Dock = DockStyle.Fill;
            pnlDatosProveedor.Location = new Point(0, 0);
            pnlDatosProveedor.Margin = new Padding(0);
            pnlDatosProveedor.Name = "pnlDatosProveedor";
            pnlDatosProveedor.Padding = new Padding(10, 2, 10, 2);
            pnlDatosProveedor.Size = new Size(470, 136);
            pnlDatosProveedor.TabIndex = 0;
            //
            // tblDatosProveedor
            //
            tblDatosProveedor.ColumnCount = 1;
            tblDatosProveedor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblDatosProveedor.Controls.Add(lblTituloDatosProveedor, 0, 0);
            tblDatosProveedor.Controls.Add(lblRazonSocialProveedor, 0, 1);
            tblDatosProveedor.Controls.Add(lblDocumentoProveedor, 0, 2);
            tblDatosProveedor.Dock = DockStyle.Fill;
            tblDatosProveedor.Location = new Point(10, 2);
            tblDatosProveedor.Margin = new Padding(0);
            tblDatosProveedor.Name = "tblDatosProveedor";
            tblDatosProveedor.RowCount = 3;
            tblDatosProveedor.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tblDatosProveedor.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tblDatosProveedor.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tblDatosProveedor.Size = new Size(450, 132);
            tblDatosProveedor.TabIndex = 0;
            //
            // lblTituloDatosProveedor
            //
            lblTituloDatosProveedor.AutoSize = false;
            lblTituloDatosProveedor.Depth = 0;
            lblTituloDatosProveedor.Dock = DockStyle.Fill;
            lblTituloDatosProveedor.Font = new Font("Roboto", 18F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblTituloDatosProveedor.Location = new Point(0, 0);
            lblTituloDatosProveedor.Margin = new Padding(0);
            lblTituloDatosProveedor.MouseState = MaterialSkin.MouseState.HOVER;
            lblTituloDatosProveedor.Name = "lblTituloDatosProveedor";
            lblTituloDatosProveedor.Size = new Size(450, 20);
            lblTituloDatosProveedor.TabIndex = 0;
            lblTituloDatosProveedor.Text = "Datos del proveedor";
            lblTituloDatosProveedor.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblRazonSocialProveedor
            //
            lblRazonSocialProveedor.AutoSize = false;
            lblRazonSocialProveedor.Depth = 0;
            lblRazonSocialProveedor.Dock = DockStyle.Fill;
            lblRazonSocialProveedor.Font = new Font("Roboto", 15F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblRazonSocialProveedor.Location = new Point(0, 20);
            lblRazonSocialProveedor.Margin = new Padding(0);
            lblRazonSocialProveedor.MouseState = MaterialSkin.MouseState.HOVER;
            lblRazonSocialProveedor.Name = "lblRazonSocialProveedor";
            lblRazonSocialProveedor.Size = new Size(450, 56);
            lblRazonSocialProveedor.TabIndex = 1;
            lblRazonSocialProveedor.Text = "Sin proveedor seleccionado";
            lblRazonSocialProveedor.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblDocumentoProveedor
            //
            lblDocumentoProveedor.AutoSize = false;
            lblDocumentoProveedor.Depth = 0;
            lblDocumentoProveedor.Dock = DockStyle.Fill;
            lblDocumentoProveedor.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblDocumentoProveedor.Location = new Point(0, 76);
            lblDocumentoProveedor.Margin = new Padding(0);
            lblDocumentoProveedor.MouseState = MaterialSkin.MouseState.HOVER;
            lblDocumentoProveedor.Name = "lblDocumentoProveedor";
            lblDocumentoProveedor.Size = new Size(450, 56);
            lblDocumentoProveedor.TabIndex = 2;
            lblDocumentoProveedor.Text = "Documento: —";
            lblDocumentoProveedor.TextAlign = ContentAlignment.MiddleLeft;
            //
            // pnlListadoProveedores
            //
            pnlListadoProveedores.BackColor = Color.FromArgb(45, 45, 45);
            pnlListadoProveedores.Controls.Add(tblListadoProveedores);
            pnlListadoProveedores.Dock = DockStyle.Fill;
            pnlListadoProveedores.Location = new Point(0, 136);
            pnlListadoProveedores.Margin = new Padding(0);
            pnlListadoProveedores.Name = "pnlListadoProveedores";
            pnlListadoProveedores.Padding = new Padding(10, 2, 10, 2);
            pnlListadoProveedores.Size = new Size(470, 134);
            pnlListadoProveedores.TabIndex = 1;
            //
            // tblListadoProveedores
            //
            tblListadoProveedores.ColumnCount = 2;
            tblListadoProveedores.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblListadoProveedores.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblListadoProveedores.Controls.Add(lblTituloListadoProveedores, 0, 0);
            tblListadoProveedores.Controls.Add(txtBuscarProveedor, 1, 0);
            tblListadoProveedores.Controls.Add(gridProveedores, 0, 1);
            tblListadoProveedores.Controls.Add(lblMensajeProveedores, 0, 2);
            tblListadoProveedores.Dock = DockStyle.Fill;
            tblListadoProveedores.Location = new Point(10, 2);
            tblListadoProveedores.Margin = new Padding(0);
            tblListadoProveedores.Name = "tblListadoProveedores";
            tblListadoProveedores.RowCount = 3;
            tblListadoProveedores.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            tblListadoProveedores.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblListadoProveedores.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
            tblListadoProveedores.SetColumnSpan(gridProveedores, 2);
            tblListadoProveedores.SetColumnSpan(lblMensajeProveedores, 2);
            tblListadoProveedores.Size = new Size(450, 130);
            tblListadoProveedores.TabIndex = 0;
            //
            // lblTituloListadoProveedores
            //
            lblTituloListadoProveedores.Anchor = AnchorStyles.Left;
            lblTituloListadoProveedores.AutoSize = true;
            lblTituloListadoProveedores.Depth = 0;
            lblTituloListadoProveedores.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblTituloListadoProveedores.Location = new Point(0, 14);
            lblTituloListadoProveedores.Margin = new Padding(0, 0, 12, 0);
            lblTituloListadoProveedores.MouseState = MaterialSkin.MouseState.HOVER;
            lblTituloListadoProveedores.Name = "lblTituloListadoProveedores";
            lblTituloListadoProveedores.Size = new Size(72, 16);
            lblTituloListadoProveedores.TabIndex = 0;
            lblTituloListadoProveedores.Text = "Listado proveedores";
            lblTituloListadoProveedores.TextAlign = ContentAlignment.MiddleLeft;
            //
            // txtBuscarProveedor
            //
            txtBuscarProveedor.AnimateReadOnly = false;
            txtBuscarProveedor.BackgroundImageLayout = ImageLayout.None;
            txtBuscarProveedor.CharacterCasing = CharacterCasing.Normal;
            txtBuscarProveedor.Depth = 0;
            txtBuscarProveedor.Dock = DockStyle.Fill;
            txtBuscarProveedor.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtBuscarProveedor.Hint = "Buscar proveedor";
            txtBuscarProveedor.Location = new Point(84, 0);
            txtBuscarProveedor.Margin = new Padding(0);
            txtBuscarProveedor.MaxLength = 32767;
            txtBuscarProveedor.MinimumSize = new Size(200, 44);
            txtBuscarProveedor.MouseState = MaterialSkin.MouseState.OUT;
            txtBuscarProveedor.Name = "txtBuscarProveedor";
            txtBuscarProveedor.Padding = new Padding(2, 0, 2, 0);
            txtBuscarProveedor.PasswordChar = '\0';
            txtBuscarProveedor.PrefixSuffixText = null;
            txtBuscarProveedor.ReadOnly = false;
            txtBuscarProveedor.RightToLeft = RightToLeft.No;
            txtBuscarProveedor.SelectedText = "";
            txtBuscarProveedor.SelectionLength = 0;
            txtBuscarProveedor.SelectionStart = 0;
            txtBuscarProveedor.ShortcutsEnabled = true;
            txtBuscarProveedor.TabIndex = 1;
            txtBuscarProveedor.TextAlign = HorizontalAlignment.Left;
            txtBuscarProveedor.TrailingIcon = null;
            txtBuscarProveedor.UseSystemPasswordChar = false;
            //
            // gridProveedores
            //
            gridProveedores.AllowUserToAddRows = false;
            gridProveedores.AllowUserToDeleteRows = false;
            gridProveedores.AllowUserToResizeRows = false;
            gridProveedores.AutoGenerateColumns = false;
            gridProveedores.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridProveedores.ColumnHeadersHeight = 26;
            gridProveedores.Columns.AddRange(new DataGridViewColumn[] { colProveedorRazonSocial, colProveedorDocumento });
            gridProveedores.Dock = DockStyle.Fill;
            gridProveedores.Location = new Point(0, 44);
            gridProveedores.Margin = new Padding(0);
            gridProveedores.MultiSelect = false;
            gridProveedores.Name = "gridProveedores";
            gridProveedores.ReadOnly = true;
            gridProveedores.RowHeadersVisible = false;
            gridProveedores.RowTemplate.Height = 21;
            gridProveedores.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridProveedores.Size = new Size(450, 68);
            gridProveedores.TabIndex = 2;
            //
            // colProveedorRazonSocial
            //
            colProveedorRazonSocial.DataPropertyName = "RazonSocial";
            colProveedorRazonSocial.FillWeight = 70F;
            colProveedorRazonSocial.HeaderText = "Razón social";
            colProveedorRazonSocial.Name = "colProveedorRazonSocial";
            colProveedorRazonSocial.ReadOnly = true;
            colProveedorRazonSocial.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colProveedorDocumento
            //
            colProveedorDocumento.DataPropertyName = "Documento";
            colProveedorDocumento.FillWeight = 30F;
            colProveedorDocumento.HeaderText = "Documento";
            colProveedorDocumento.Name = "colProveedorDocumento";
            colProveedorDocumento.ReadOnly = true;
            colProveedorDocumento.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // lblMensajeProveedores
            //
            lblMensajeProveedores.AutoSize = false;
            lblMensajeProveedores.Depth = 0;
            lblMensajeProveedores.Dock = DockStyle.Fill;
            lblMensajeProveedores.Font = new Font("Roboto", 13F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblMensajeProveedores.Location = new Point(0, 112);
            lblMensajeProveedores.Margin = new Padding(0);
            lblMensajeProveedores.MouseState = MaterialSkin.MouseState.HOVER;
            lblMensajeProveedores.Name = "lblMensajeProveedores";
            lblMensajeProveedores.Size = new Size(450, 18);
            lblMensajeProveedores.TabIndex = 3;
            lblMensajeProveedores.TextAlign = ContentAlignment.MiddleLeft;
            //
            // tblCompraActual
            //
            tblCompraActual.ColumnCount = 1;
            tblCompraActual.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblCompraActual.Controls.Add(lblTituloCompraActual, 0, 0);
            tblCompraActual.Controls.Add(gridLineas, 0, 1);
            tblCompraActual.Controls.Add(tblPieCompra, 0, 2);
            tblCompraActual.Dock = DockStyle.Fill;
            tblCompraActual.Location = new Point(0, 305);
            tblCompraActual.Margin = new Padding(0);
            tblCompraActual.Name = "tblCompraActual";
            tblCompraActual.RowCount = 3;
            tblCompraActual.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            tblCompraActual.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblCompraActual.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            tblCompraActual.Size = new Size(980, 290);
            tblCompraActual.TabIndex = 2;
            //
            // lblTituloCompraActual
            //
            lblTituloCompraActual.AutoSize = false;
            lblTituloCompraActual.Depth = 0;
            lblTituloCompraActual.Dock = DockStyle.Fill;
            lblTituloCompraActual.Font = new Font("Roboto", 18F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblTituloCompraActual.Location = new Point(0, 0);
            lblTituloCompraActual.Margin = new Padding(0);
            lblTituloCompraActual.MouseState = MaterialSkin.MouseState.HOVER;
            lblTituloCompraActual.Name = "lblTituloCompraActual";
            lblTituloCompraActual.Size = new Size(980, 22);
            lblTituloCompraActual.TabIndex = 0;
            lblTituloCompraActual.Text = "Compra Actual";
            lblTituloCompraActual.TextAlign = ContentAlignment.MiddleLeft;
            //
            // gridLineas
            //
            gridLineas.AllowUserToAddRows = false;
            gridLineas.AllowUserToDeleteRows = false;
            gridLineas.AllowUserToResizeRows = false;
            gridLineas.AutoGenerateColumns = false;
            gridLineas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridLineas.ColumnHeadersHeight = 40;
            gridLineas.Columns.AddRange(new DataGridViewColumn[] { colLineaProducto, colLineaCantidad, colLineaPrecioCompra, colLineaPrecioVenta, colLineaSubtotal, colLineaEditar, colLineaQuitar });
            gridLineas.Dock = DockStyle.Fill;
            gridLineas.Location = new Point(0, 22);
            gridLineas.Margin = new Padding(0);
            gridLineas.MinimumSize = new Size(0, 180);
            gridLineas.MultiSelect = false;
            gridLineas.Name = "gridLineas";
            gridLineas.ReadOnly = true;
            gridLineas.RowHeadersVisible = false;
            gridLineas.RowTemplate.Height = 34;
            gridLineas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridLineas.Size = new Size(980, 208);
            gridLineas.TabIndex = 1;
            //
            // colLineaProducto
            //
            colLineaProducto.DataPropertyName = "NombreProducto";
            colLineaProducto.FillWeight = 34F;
            colLineaProducto.HeaderText = "Producto";
            colLineaProducto.Name = "colLineaProducto";
            colLineaProducto.ReadOnly = true;
            colLineaProducto.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colLineaCantidad
            //
            colLineaCantidad.DataPropertyName = "Cantidad";
            colLineaCantidad.FillWeight = 10F;
            colLineaCantidad.HeaderText = "Cantidad";
            colLineaCantidad.Name = "colLineaCantidad";
            colLineaCantidad.ReadOnly = true;
            colLineaCantidad.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colLineaPrecioCompra
            //
            colLineaPrecioCompra.DataPropertyName = "PrecioCompra";
            colLineaPrecioCompra.FillWeight = 14F;
            colLineaPrecioCompra.HeaderText = "Precio compra";
            colLineaPrecioCompra.Name = "colLineaPrecioCompra";
            colLineaPrecioCompra.ReadOnly = true;
            colLineaPrecioCompra.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colLineaPrecioVenta
            //
            colLineaPrecioVenta.DataPropertyName = "PrecioVenta";
            colLineaPrecioVenta.FillWeight = 14F;
            colLineaPrecioVenta.HeaderText = "Precio venta";
            colLineaPrecioVenta.Name = "colLineaPrecioVenta";
            colLineaPrecioVenta.ReadOnly = true;
            colLineaPrecioVenta.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colLineaSubtotal
            //
            colLineaSubtotal.DataPropertyName = "Subtotal";
            colLineaSubtotal.FillWeight = 14F;
            colLineaSubtotal.HeaderText = "Subtotal";
            colLineaSubtotal.Name = "colLineaSubtotal";
            colLineaSubtotal.ReadOnly = true;
            colLineaSubtotal.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colLineaEditar
            //
            colLineaEditar.FillWeight = 7F;
            colLineaEditar.FlatStyle = FlatStyle.Flat;
            colLineaEditar.HeaderText = "";
            colLineaEditar.Name = "colLineaEditar";
            colLineaEditar.ReadOnly = true;
            colLineaEditar.SortMode = DataGridViewColumnSortMode.NotSortable;
            colLineaEditar.Text = "Editar";
            colLineaEditar.UseColumnTextForButtonValue = true;
            //
            // colLineaQuitar
            //
            colLineaQuitar.FillWeight = 7F;
            colLineaQuitar.FlatStyle = FlatStyle.Flat;
            colLineaQuitar.HeaderText = "";
            colLineaQuitar.Name = "colLineaQuitar";
            colLineaQuitar.ReadOnly = true;
            colLineaQuitar.SortMode = DataGridViewColumnSortMode.NotSortable;
            colLineaQuitar.Text = "Quitar";
            colLineaQuitar.UseColumnTextForButtonValue = true;
            //
            // tblPieCompra
            //
            tblPieCompra.ColumnCount = 1;
            tblPieCompra.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblPieCompra.Controls.Add(lblMontoTotal, 0, 0);
            tblPieCompra.Controls.Add(lblMensaje, 0, 1);
            tblPieCompra.Dock = DockStyle.Fill;
            tblPieCompra.Location = new Point(0, 230);
            tblPieCompra.Margin = new Padding(0);
            tblPieCompra.Name = "tblPieCompra";
            tblPieCompra.RowCount = 2;
            tblPieCompra.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            tblPieCompra.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
            tblPieCompra.Size = new Size(980, 60);
            tblPieCompra.TabIndex = 2;
            //
            // lblMontoTotal
            //
            lblMontoTotal.AutoSize = false;
            lblMontoTotal.Depth = 0;
            lblMontoTotal.Dock = DockStyle.Fill;
            lblMontoTotal.Font = new Font("Roboto", 20F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblMontoTotal.Location = new Point(0, 0);
            lblMontoTotal.Margin = new Padding(0);
            lblMontoTotal.MouseState = MaterialSkin.MouseState.HOVER;
            lblMontoTotal.Name = "lblMontoTotal";
            lblMontoTotal.Size = new Size(980, 42);
            lblMontoTotal.TabIndex = 0;
            lblMontoTotal.Text = "Total: 0,00";
            lblMontoTotal.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblMensaje
            //
            lblMensaje.AutoSize = false;
            lblMensaje.Depth = 0;
            lblMensaje.Dock = DockStyle.Fill;
            lblMensaje.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblMensaje.Location = new Point(0, 42);
            lblMensaje.Margin = new Padding(0);
            lblMensaje.MouseState = MaterialSkin.MouseState.HOVER;
            lblMensaje.Name = "lblMensaje";
            lblMensaje.Size = new Size(980, 18);
            lblMensaje.TabIndex = 1;
            lblMensaje.TextAlign = ContentAlignment.MiddleLeft;
            //
            // flpAcciones
            //
            flpAcciones.AutoSize = false;
            flpAcciones.Controls.Add(btnRegistrar);
            flpAcciones.Controls.Add(btnLimpiar);
            flpAcciones.Controls.Add(btnVerHistorial);
            flpAcciones.Dock = DockStyle.Fill;
            flpAcciones.FlowDirection = FlowDirection.RightToLeft;
            flpAcciones.Location = new Point(0, 595);
            flpAcciones.Margin = new Padding(0);
            flpAcciones.Name = "flpAcciones";
            flpAcciones.Size = new Size(980, 55);
            flpAcciones.TabIndex = 3;
            flpAcciones.WrapContents = false;
            //
            // btnRegistrar
            //
            btnRegistrar.AutoSize = true;
            btnRegistrar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnRegistrar.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnRegistrar.Depth = 0;
            btnRegistrar.HighEmphasis = true;
            btnRegistrar.Icon = null;
            btnRegistrar.Margin = new Padding(6, 4, 6, 4);
            btnRegistrar.MinimumSize = new Size(0, 38);
            btnRegistrar.MouseState = MaterialSkin.MouseState.HOVER;
            btnRegistrar.Name = "btnRegistrar";
            btnRegistrar.NoAccentTextColor = Color.Empty;
            btnRegistrar.Padding = new Padding(10, 0, 10, 0);
            btnRegistrar.Size = new Size(170, 38);
            btnRegistrar.TabIndex = 0;
            btnRegistrar.Text = "REGISTRAR COMPRA";
            btnRegistrar.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnRegistrar.UseAccentColor = true;
            btnRegistrar.UseVisualStyleBackColor = true;
            //
            // btnLimpiar
            //
            btnLimpiar.AutoSize = true;
            btnLimpiar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnLimpiar.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnLimpiar.Depth = 0;
            btnLimpiar.HighEmphasis = true;
            btnLimpiar.Icon = null;
            btnLimpiar.Margin = new Padding(6, 4, 6, 4);
            btnLimpiar.MinimumSize = new Size(0, 38);
            btnLimpiar.MouseState = MaterialSkin.MouseState.HOVER;
            btnLimpiar.Name = "btnLimpiar";
            btnLimpiar.NoAccentTextColor = Color.Empty;
            btnLimpiar.Padding = new Padding(10, 0, 10, 0);
            btnLimpiar.Size = new Size(170, 38);
            btnLimpiar.TabIndex = 1;
            btnLimpiar.Text = "CANCELAR COMPRA";
            btnLimpiar.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            btnLimpiar.UseAccentColor = false;
            btnLimpiar.UseVisualStyleBackColor = true;
            //
            // btnVerHistorial
            //
            btnVerHistorial.AutoSize = true;
            btnVerHistorial.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnVerHistorial.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnVerHistorial.Depth = 0;
            btnVerHistorial.HighEmphasis = true;
            btnVerHistorial.Icon = null;
            btnVerHistorial.Margin = new Padding(6, 4, 6, 4);
            btnVerHistorial.MinimumSize = new Size(0, 38);
            btnVerHistorial.MouseState = MaterialSkin.MouseState.HOVER;
            btnVerHistorial.Name = "btnVerHistorial";
            btnVerHistorial.NoAccentTextColor = Color.Empty;
            btnVerHistorial.Padding = new Padding(10, 0, 10, 0);
            btnVerHistorial.Size = new Size(130, 38);
            btnVerHistorial.TabIndex = 2;
            btnVerHistorial.Text = "Ver Historial";
            btnVerHistorial.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            btnVerHistorial.UseAccentColor = false;
            btnVerHistorial.UseVisualStyleBackColor = true;
            //
            // ComprasForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(33, 33, 33);
            Controls.Add(pnlContenedor);
            Name = "ComprasForm";
            Size = new Size(1083, 700);
            pnlContenedor.ResumeLayout(false);
            tblPrincipal.ResumeLayout(false);
            pnlEncabezado.ResumeLayout(false);
            tblBloques.ResumeLayout(false);
            tblBloqueProducto.ResumeLayout(false);
            pnlDatosProducto.ResumeLayout(false);
            tblDatosProducto.ResumeLayout(false);
            tblProductoSeleccionado.ResumeLayout(false);
            tblCaptura.ResumeLayout(false);
            tblSubtotalAccion.ResumeLayout(false);
            tblSubtotalAccion.PerformLayout();
            pnlListadoProductos.ResumeLayout(false);
            tblListadoProductos.ResumeLayout(false);
            tblListadoProductos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)gridProductos).EndInit();
            tblBloqueProveedor.ResumeLayout(false);
            pnlDatosProveedor.ResumeLayout(false);
            tblDatosProveedor.ResumeLayout(false);
            pnlListadoProveedores.ResumeLayout(false);
            tblListadoProveedores.ResumeLayout(false);
            tblListadoProveedores.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)gridProveedores).EndInit();
            tblCompraActual.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridLineas).EndInit();
            tblPieCompra.ResumeLayout(false);
            flpAcciones.ResumeLayout(false);
            flpAcciones.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlContenedor;
        private TableLayoutPanel tblPrincipal;
        private Panel pnlEncabezado;
        private MaterialSkin.Controls.MaterialLabel lblTitulo;
        private TableLayoutPanel tblBloques;
        private TableLayoutPanel tblBloqueProducto;
        private Panel pnlDatosProducto;
        private TableLayoutPanel tblDatosProducto;
        private MaterialSkin.Controls.MaterialLabel lblTituloDatosProducto;
        private TableLayoutPanel tblProductoSeleccionado;
        private MaterialSkin.Controls.MaterialLabel lblNombreProducto;
        private MaterialSkin.Controls.MaterialLabel lblCodigoProducto;
        private TableLayoutPanel tblCaptura;
        private MaterialSkin.Controls.MaterialTextBox2 txtCantidad;
        private MaterialSkin.Controls.MaterialTextBox2 txtPrecioCompra;
        private MaterialSkin.Controls.MaterialTextBox2 txtPrecioVenta;
        private TableLayoutPanel tblSubtotalAccion;
        private MaterialSkin.Controls.MaterialLabel lblSubtotalPreparacion;
        private MaterialSkin.Controls.MaterialButton btnAgregarLinea;
        private Panel pnlListadoProductos;
        private TableLayoutPanel tblListadoProductos;
        private MaterialSkin.Controls.MaterialLabel lblTituloListadoProductos;
        private MaterialSkin.Controls.MaterialTextBox2 txtBuscarProducto;
        private DataGridView gridProductos;
        private DataGridViewTextBoxColumn colProductoCodigo;
        private DataGridViewTextBoxColumn colProductoNombre;
        private MaterialSkin.Controls.MaterialLabel lblMensajeProductos;
        private TableLayoutPanel tblBloqueProveedor;
        private Panel pnlDatosProveedor;
        private TableLayoutPanel tblDatosProveedor;
        private MaterialSkin.Controls.MaterialLabel lblTituloDatosProveedor;
        private MaterialSkin.Controls.MaterialLabel lblRazonSocialProveedor;
        private MaterialSkin.Controls.MaterialLabel lblDocumentoProveedor;
        private Panel pnlListadoProveedores;
        private TableLayoutPanel tblListadoProveedores;
        private MaterialSkin.Controls.MaterialLabel lblTituloListadoProveedores;
        private MaterialSkin.Controls.MaterialTextBox2 txtBuscarProveedor;
        private DataGridView gridProveedores;
        private DataGridViewTextBoxColumn colProveedorRazonSocial;
        private DataGridViewTextBoxColumn colProveedorDocumento;
        private MaterialSkin.Controls.MaterialLabel lblMensajeProveedores;
        private TableLayoutPanel tblCompraActual;
        private MaterialSkin.Controls.MaterialLabel lblTituloCompraActual;
        private DataGridView gridLineas;
        private DataGridViewTextBoxColumn colLineaProducto;
        private DataGridViewTextBoxColumn colLineaCantidad;
        private DataGridViewTextBoxColumn colLineaPrecioCompra;
        private DataGridViewTextBoxColumn colLineaPrecioVenta;
        private DataGridViewTextBoxColumn colLineaSubtotal;
        private DataGridViewButtonColumn colLineaEditar;
        private DataGridViewButtonColumn colLineaQuitar;
        private TableLayoutPanel tblPieCompra;
        private MaterialSkin.Controls.MaterialLabel lblMontoTotal;
        private MaterialSkin.Controls.MaterialLabel lblMensaje;
        private FlowLayoutPanel flpAcciones;
        private MaterialSkin.Controls.MaterialButton btnRegistrar;
        private MaterialSkin.Controls.MaterialButton btnLimpiar;
        private MaterialSkin.Controls.MaterialButton btnVerHistorial;
    }
}
