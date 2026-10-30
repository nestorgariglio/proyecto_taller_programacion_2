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
            flpAcciones = new FlowLayoutPanel();
            btnRegistrar = new MaterialSkin.Controls.MaterialButton();
            btnLimpiar = new MaterialSkin.Controls.MaterialButton();
            lblMensaje = new MaterialSkin.Controls.MaterialLabel();
            pnlHistorial = new Panel();
            tblHistorial = new TableLayoutPanel();
            lblTituloHistorial = new MaterialSkin.Controls.MaterialLabel();
            lblMensajeHistorial = new MaterialSkin.Controls.MaterialLabel();
            btnVerDetalle = new MaterialSkin.Controls.MaterialButton();
            gridHistorial = new DataGridView();
            colHistorialNumero = new DataGridViewTextBoxColumn();
            colHistorialFecha = new DataGridViewTextBoxColumn();
            colHistorialProveedor = new DataGridViewTextBoxColumn();
            colHistorialUsuario = new DataGridViewTextBoxColumn();
            colHistorialTotal = new DataGridViewTextBoxColumn();
            colHistorialEstado = new DataGridViewTextBoxColumn();
            tblPrincipal.SuspendLayout();
            pnlContenedor.SuspendLayout();
            pnlEncabezado.SuspendLayout();
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
            pnlHistorial.SuspendLayout();
            tblHistorial.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridHistorial).BeginInit();
            SuspendLayout();
            //
            // pnlContenedor
            //
            pnlContenedor.AutoScroll = true;
            pnlContenedor.BackColor = Color.FromArgb(33, 33, 33);
            pnlContenedor.Controls.Add(tblPrincipal);
            pnlContenedor.Dock = DockStyle.Fill;
            pnlContenedor.Location = new Point(0, 0);
            pnlContenedor.Margin = new Padding(0);
            pnlContenedor.Name = "pnlContenedor";
            pnlContenedor.Size = new Size(1083, 508);
            pnlContenedor.TabIndex = 0;
            //
            // tblPrincipal
            //
            tblPrincipal.ColumnCount = 1;
            tblPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblPrincipal.Controls.Add(pnlEncabezado, 0, 0);
            tblPrincipal.Controls.Add(tblBloqueProducto, 0, 1);
            tblPrincipal.Controls.Add(tblBloqueProveedor, 0, 2);
            tblPrincipal.Controls.Add(tblCompraActual, 0, 3);
            tblPrincipal.Controls.Add(pnlHistorial, 0, 4);
            tblPrincipal.Dock = DockStyle.Top;
            tblPrincipal.Location = new Point(0, 0);
            tblPrincipal.Margin = new Padding(0);
            tblPrincipal.MinimumSize = new Size(900, 760);
            tblPrincipal.Name = "tblPrincipal";
            tblPrincipal.Padding = new Padding(8, 6, 8, 6);
            tblPrincipal.RowCount = 5;
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 184F));
            tblPrincipal.Size = new Size(1083, 800);
            tblPrincipal.TabIndex = 0;
            //
            // pnlEncabezado
            //
            pnlEncabezado.Controls.Add(lblTitulo);
            pnlEncabezado.Dock = DockStyle.Fill;
            pnlEncabezado.Location = new Point(8, 6);
            pnlEncabezado.Margin = new Padding(0);
            pnlEncabezado.Name = "pnlEncabezado";
            pnlEncabezado.Size = new Size(1164, 42);
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
            lblTitulo.Size = new Size(1164, 42);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Registro de Compras a Proveedores";
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            //
            // tblBloqueProducto
            //
            tblBloqueProducto.ColumnCount = 2;
            tblBloqueProducto.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));
            tblBloqueProducto.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));
            tblBloqueProducto.Controls.Add(pnlDatosProducto, 0, 0);
            tblBloqueProducto.Controls.Add(pnlListadoProductos, 1, 0);
            tblBloqueProducto.Dock = DockStyle.Fill;
            tblBloqueProducto.Location = new Point(8, 48);
            tblBloqueProducto.Margin = new Padding(0);
            tblBloqueProducto.Name = "tblBloqueProducto";
            tblBloqueProducto.RowCount = 1;
            tblBloqueProducto.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblBloqueProducto.Size = new Size(1164, 176);
            tblBloqueProducto.TabIndex = 1;
            //
            // pnlDatosProducto
            //
            pnlDatosProducto.BackColor = Color.FromArgb(45, 45, 45);
            pnlDatosProducto.Controls.Add(tblDatosProducto);
            pnlDatosProducto.Dock = DockStyle.Fill;
            pnlDatosProducto.Location = new Point(0, 0);
            pnlDatosProducto.Margin = new Padding(0, 0, 6, 0);
            pnlDatosProducto.Name = "pnlDatosProducto";
            pnlDatosProducto.Padding = new Padding(12, 8, 12, 8);
            pnlDatosProducto.Size = new Size(599, 176);
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
            tblDatosProducto.Location = new Point(12, 8);
            tblDatosProducto.Margin = new Padding(0);
            tblDatosProducto.Name = "tblDatosProducto";
            tblDatosProducto.RowCount = 4;
            tblDatosProducto.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            tblDatosProducto.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            tblDatosProducto.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            tblDatosProducto.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblDatosProducto.Size = new Size(575, 160);
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
            lblTituloDatosProducto.Size = new Size(575, 26);
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
            tblProductoSeleccionado.Location = new Point(0, 26);
            tblProductoSeleccionado.Margin = new Padding(0);
            tblProductoSeleccionado.Name = "tblProductoSeleccionado";
            tblProductoSeleccionado.RowCount = 1;
            tblProductoSeleccionado.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblProductoSeleccionado.Size = new Size(575, 26);
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
            lblNombreProducto.Size = new Size(356, 26);
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
            lblCodigoProducto.Location = new Point(356, 0);
            lblCodigoProducto.Margin = new Padding(0);
            lblCodigoProducto.MouseState = MaterialSkin.MouseState.HOVER;
            lblCodigoProducto.Name = "lblCodigoProducto";
            lblCodigoProducto.Size = new Size(219, 26);
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
            tblCaptura.Location = new Point(0, 52);
            tblCaptura.Margin = new Padding(0);
            tblCaptura.Name = "tblCaptura";
            tblCaptura.RowCount = 1;
            tblCaptura.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblCaptura.Size = new Size(575, 62);
            tblCaptura.TabIndex = 2;
            //
            // txtCantidad
            //
            txtCantidad.AnimateReadOnly = false;
            txtCantidad.BackgroundImageLayout = ImageLayout.None;
            txtCantidad.CharacterCasing = CharacterCasing.Normal;
            txtCantidad.Depth = 0;
            txtCantidad.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtCantidad.Hint = "Cantidad";
            txtCantidad.Location = new Point(0, 6);
            txtCantidad.Margin = new Padding(0, 4, 6, 0);
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
            txtCantidad.Size = new Size(166, 48);
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
            txtPrecioCompra.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtPrecioCompra.Hint = "Precio compra";
            txtPrecioCompra.Location = new Point(172, 6);
            txtPrecioCompra.Margin = new Padding(0, 4, 6, 0);
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
            txtPrecioCompra.Size = new Size(195, 48);
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
            txtPrecioVenta.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtPrecioVenta.Hint = "Precio venta";
            txtPrecioVenta.Location = new Point(373, 6);
            txtPrecioVenta.Margin = new Padding(0, 4, 0, 0);
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
            txtPrecioVenta.Size = new Size(202, 48);
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
            tblSubtotalAccion.Location = new Point(0, 114);
            tblSubtotalAccion.Margin = new Padding(0);
            tblSubtotalAccion.Name = "tblSubtotalAccion";
            tblSubtotalAccion.RowCount = 1;
            tblSubtotalAccion.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblSubtotalAccion.Size = new Size(575, 46);
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
            lblSubtotalPreparacion.Size = new Size(381, 46);
            lblSubtotalPreparacion.TabIndex = 0;
            lblSubtotalPreparacion.Text = "Subtotal: —";
            lblSubtotalPreparacion.TextAlign = ContentAlignment.MiddleLeft;
            //
            // btnAgregarLinea
            //
            btnAgregarLinea.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnAgregarLinea.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnAgregarLinea.Depth = 0;
            btnAgregarLinea.HighEmphasis = true;
            btnAgregarLinea.Icon = null;
            btnAgregarLinea.Location = new Point(385, 4);
            btnAgregarLinea.Margin = new Padding(4, 4, 0, 4);
            btnAgregarLinea.MinimumSize = new Size(190, 38);
            btnAgregarLinea.MouseState = MaterialSkin.MouseState.HOVER;
            btnAgregarLinea.Name = "btnAgregarLinea";
            btnAgregarLinea.NoAccentTextColor = Color.Empty;
            btnAgregarLinea.Size = new Size(190, 38);
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
            pnlListadoProductos.Location = new Point(611, 0);
            pnlListadoProductos.Margin = new Padding(6, 0, 0, 0);
            pnlListadoProductos.Name = "pnlListadoProductos";
            pnlListadoProductos.Padding = new Padding(12, 8, 12, 8);
            pnlListadoProductos.Size = new Size(553, 176);
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
            tblListadoProductos.Location = new Point(12, 8);
            tblListadoProductos.Margin = new Padding(0);
            tblListadoProductos.Name = "tblListadoProductos";
            tblListadoProductos.RowCount = 3;
            tblListadoProductos.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            tblListadoProductos.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblListadoProductos.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
            tblListadoProductos.SetColumnSpan(gridProductos, 2);
            tblListadoProductos.SetColumnSpan(lblMensajeProductos, 2);
            tblListadoProductos.Size = new Size(529, 143);
            tblListadoProductos.TabIndex = 0;
            //
            // lblTituloListadoProductos
            //
            lblTituloListadoProductos.Anchor = AnchorStyles.Left;
            lblTituloListadoProductos.AutoSize = true;
            lblTituloListadoProductos.Depth = 0;
            lblTituloListadoProductos.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblTituloListadoProductos.Location = new Point(0, 17);
            lblTituloListadoProductos.Margin = new Padding(0, 0, 16, 0);
            lblTituloListadoProductos.MouseState = MaterialSkin.MouseState.HOVER;
            lblTituloListadoProductos.Name = "lblTituloListadoProductos";
            lblTituloListadoProductos.Size = new Size(59, 16);
            lblTituloListadoProductos.TabIndex = 0;
            lblTituloListadoProductos.Text = "Listado productos";
            lblTituloListadoProductos.TextAlign = ContentAlignment.MiddleLeft;
            //
            // txtBuscarProducto
            //
            txtBuscarProducto.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtBuscarProducto.AnimateReadOnly = false;
            txtBuscarProducto.BackgroundImageLayout = ImageLayout.None;
            txtBuscarProducto.CharacterCasing = CharacterCasing.Normal;
            txtBuscarProducto.Depth = 0;
            txtBuscarProducto.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtBuscarProducto.Hint = "Buscar producto";
            txtBuscarProducto.Location = new Point(75, 0);
            txtBuscarProducto.Margin = new Padding(16, 0, 0, 4);
            txtBuscarProducto.MaxLength = 32767;
            txtBuscarProducto.MinimumSize = new Size(200, 48);
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
            txtBuscarProducto.Size = new Size(529, 48);
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
            gridProductos.ColumnHeadersHeight = 28;
            gridProductos.Columns.AddRange(new DataGridViewColumn[] { colProductoCodigo, colProductoNombre });
            gridProductos.Dock = DockStyle.Fill;
            gridProductos.Location = new Point(0, 78);
            gridProductos.Margin = new Padding(0);
            gridProductos.MultiSelect = false;
            gridProductos.Name = "gridProductos";
            gridProductos.ReadOnly = true;
            gridProductos.RowHeadersVisible = false;
            gridProductos.RowTemplate.Height = 28;
            gridProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridProductos.Size = new Size(529, 60);
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
            lblMensajeProductos.Location = new Point(0, 138);
            lblMensajeProductos.Margin = new Padding(0);
            lblMensajeProductos.MouseState = MaterialSkin.MouseState.HOVER;
            lblMensajeProductos.Name = "lblMensajeProductos";
            lblMensajeProductos.Size = new Size(529, 22);
            lblMensajeProductos.TabIndex = 3;
            lblMensajeProductos.TextAlign = ContentAlignment.MiddleLeft;
            //
            // tblBloqueProveedor
            //
            tblBloqueProveedor.ColumnCount = 2;
            tblBloqueProveedor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));
            tblBloqueProveedor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));
            tblBloqueProveedor.Controls.Add(pnlDatosProveedor, 0, 0);
            tblBloqueProveedor.Controls.Add(pnlListadoProveedores, 1, 0);
            tblBloqueProveedor.Dock = DockStyle.Fill;
            tblBloqueProveedor.Location = new Point(8, 224);
            tblBloqueProveedor.Margin = new Padding(0);
            tblBloqueProveedor.Name = "tblBloqueProveedor";
            tblBloqueProveedor.RowCount = 1;
            tblBloqueProveedor.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblBloqueProveedor.Size = new Size(1164, 164);
            tblBloqueProveedor.TabIndex = 2;
            //
            // pnlDatosProveedor
            //
            pnlDatosProveedor.BackColor = Color.FromArgb(45, 45, 45);
            pnlDatosProveedor.Controls.Add(tblDatosProveedor);
            pnlDatosProveedor.Dock = DockStyle.Fill;
            pnlDatosProveedor.Location = new Point(0, 0);
            pnlDatosProveedor.Margin = new Padding(0, 0, 6, 0);
            pnlDatosProveedor.Name = "pnlDatosProveedor";
            pnlDatosProveedor.Padding = new Padding(12, 8, 12, 8);
            pnlDatosProveedor.Size = new Size(599, 164);
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
            tblDatosProveedor.Location = new Point(12, 8);
            tblDatosProveedor.Margin = new Padding(0);
            tblDatosProveedor.Name = "tblDatosProveedor";
            tblDatosProveedor.RowCount = 4;
            tblDatosProveedor.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            tblDatosProveedor.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tblDatosProveedor.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tblDatosProveedor.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblDatosProveedor.Size = new Size(575, 148);
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
            lblTituloDatosProveedor.Size = new Size(575, 26);
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
            lblRazonSocialProveedor.Location = new Point(0, 26);
            lblRazonSocialProveedor.Margin = new Padding(0);
            lblRazonSocialProveedor.MouseState = MaterialSkin.MouseState.HOVER;
            lblRazonSocialProveedor.Name = "lblRazonSocialProveedor";
            lblRazonSocialProveedor.Size = new Size(575, 28);
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
            lblDocumentoProveedor.Location = new Point(0, 54);
            lblDocumentoProveedor.Margin = new Padding(0);
            lblDocumentoProveedor.MouseState = MaterialSkin.MouseState.HOVER;
            lblDocumentoProveedor.Name = "lblDocumentoProveedor";
            lblDocumentoProveedor.Size = new Size(575, 26);
            lblDocumentoProveedor.TabIndex = 2;
            lblDocumentoProveedor.Text = "Documento: —";
            lblDocumentoProveedor.TextAlign = ContentAlignment.MiddleLeft;
            //
            // pnlListadoProveedores
            //
            pnlListadoProveedores.BackColor = Color.FromArgb(45, 45, 45);
            pnlListadoProveedores.Controls.Add(tblListadoProveedores);
            pnlListadoProveedores.Dock = DockStyle.Fill;
            pnlListadoProveedores.Location = new Point(611, 0);
            pnlListadoProveedores.Margin = new Padding(6, 0, 0, 0);
            pnlListadoProveedores.Name = "pnlListadoProveedores";
            pnlListadoProveedores.Padding = new Padding(12, 8, 12, 8);
            pnlListadoProveedores.Size = new Size(553, 164);
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
            tblListadoProveedores.Location = new Point(12, 8);
            tblListadoProveedores.Margin = new Padding(0);
            tblListadoProveedores.Name = "tblListadoProveedores";
            tblListadoProveedores.RowCount = 3;
            tblListadoProveedores.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            tblListadoProveedores.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblListadoProveedores.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
            tblListadoProveedores.SetColumnSpan(gridProveedores, 2);
            tblListadoProveedores.SetColumnSpan(lblMensajeProveedores, 2);
            tblListadoProveedores.Size = new Size(529, 143);
            tblListadoProveedores.TabIndex = 0;
            //
            // lblTituloListadoProveedores
            //
            lblTituloListadoProveedores.Anchor = AnchorStyles.Left;
            lblTituloListadoProveedores.AutoSize = true;
            lblTituloListadoProveedores.Depth = 0;
            lblTituloListadoProveedores.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblTituloListadoProveedores.Location = new Point(0, 17);
            lblTituloListadoProveedores.Margin = new Padding(0, 0, 16, 0);
            lblTituloListadoProveedores.MouseState = MaterialSkin.MouseState.HOVER;
            lblTituloListadoProveedores.Name = "lblTituloListadoProveedores";
            lblTituloListadoProveedores.Size = new Size(72, 16);
            lblTituloListadoProveedores.TabIndex = 0;
            lblTituloListadoProveedores.Text = "Listado proveedores";
            lblTituloListadoProveedores.TextAlign = ContentAlignment.MiddleLeft;
            //
            // txtBuscarProveedor
            //
            txtBuscarProveedor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtBuscarProveedor.AnimateReadOnly = false;
            txtBuscarProveedor.BackgroundImageLayout = ImageLayout.None;
            txtBuscarProveedor.CharacterCasing = CharacterCasing.Normal;
            txtBuscarProveedor.Depth = 0;
            txtBuscarProveedor.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtBuscarProveedor.Hint = "Buscar proveedor";
            txtBuscarProveedor.Location = new Point(88, 0);
            txtBuscarProveedor.Margin = new Padding(16, 0, 0, 4);
            txtBuscarProveedor.MaxLength = 32767;
            txtBuscarProveedor.MinimumSize = new Size(200, 48);
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
            txtBuscarProveedor.Size = new Size(529, 48);
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
            gridProveedores.ColumnHeadersHeight = 28;
            gridProveedores.Columns.AddRange(new DataGridViewColumn[] { colProveedorRazonSocial, colProveedorDocumento });
            gridProveedores.Dock = DockStyle.Fill;
            gridProveedores.Location = new Point(0, 78);
            gridProveedores.Margin = new Padding(0);
            gridProveedores.MultiSelect = false;
            gridProveedores.Name = "gridProveedores";
            gridProveedores.ReadOnly = true;
            gridProveedores.RowHeadersVisible = false;
            gridProveedores.RowTemplate.Height = 28;
            gridProveedores.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridProveedores.Size = new Size(529, 48);
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
            lblMensajeProveedores.Location = new Point(0, 126);
            lblMensajeProveedores.Margin = new Padding(0);
            lblMensajeProveedores.MouseState = MaterialSkin.MouseState.HOVER;
            lblMensajeProveedores.Name = "lblMensajeProveedores";
            lblMensajeProveedores.Size = new Size(529, 22);
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
            tblCompraActual.Location = new Point(8, 388);
            tblCompraActual.Margin = new Padding(0);
            tblCompraActual.Name = "tblCompraActual";
            tblCompraActual.RowCount = 3;
            tblCompraActual.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            tblCompraActual.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblCompraActual.RowStyles.Add(new RowStyle(SizeType.Absolute, 86F));
            tblCompraActual.Size = new Size(1164, 236);
            tblCompraActual.TabIndex = 3;
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
            lblTituloCompraActual.Size = new Size(1164, 26);
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
            gridLineas.Location = new Point(0, 26);
            gridLineas.Margin = new Padding(0);
            gridLineas.MultiSelect = false;
            gridLineas.Name = "gridLineas";
            gridLineas.ReadOnly = true;
            gridLineas.RowHeadersVisible = false;
            gridLineas.RowTemplate.Height = 34;
            gridLineas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridLineas.Size = new Size(1164, 124);
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
            tblPieCompra.ColumnCount = 2;
            tblPieCompra.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblPieCompra.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblPieCompra.Controls.Add(lblMontoTotal, 0, 0);
            tblPieCompra.Controls.Add(flpAcciones, 1, 0);
            tblPieCompra.Controls.Add(lblMensaje, 0, 1);
            tblPieCompra.Dock = DockStyle.Fill;
            tblPieCompra.Location = new Point(0, 150);
            tblPieCompra.Margin = new Padding(0);
            tblPieCompra.Name = "tblPieCompra";
            tblPieCompra.RowCount = 2;
            tblPieCompra.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            tblPieCompra.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblPieCompra.SetColumnSpan(lblMensaje, 2);
            tblPieCompra.Size = new Size(1164, 86);
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
            lblMontoTotal.Size = new Size(996, 50);
            lblMontoTotal.TabIndex = 0;
            lblMontoTotal.Text = "Total: 0,00";
            lblMontoTotal.TextAlign = ContentAlignment.MiddleLeft;
            //
            // flpAcciones
            //
            flpAcciones.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            flpAcciones.AutoSize = true;
            flpAcciones.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpAcciones.Controls.Add(btnRegistrar);
            flpAcciones.Controls.Add(btnLimpiar);
            flpAcciones.FlowDirection = FlowDirection.RightToLeft;
            flpAcciones.Location = new Point(996, 0);
            flpAcciones.Margin = new Padding(0);
            flpAcciones.Name = "flpAcciones";
            flpAcciones.Size = new Size(168, 50);
            flpAcciones.TabIndex = 1;
            flpAcciones.WrapContents = false;
            //
            // btnRegistrar
            //
            btnRegistrar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnRegistrar.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnRegistrar.Depth = 0;
            btnRegistrar.HighEmphasis = true;
            btnRegistrar.Icon = null;
            btnRegistrar.Margin = new Padding(6, 4, 0, 4);
            btnRegistrar.MinimumSize = new Size(170, 38);
            btnRegistrar.MouseState = MaterialSkin.MouseState.HOVER;
            btnRegistrar.Name = "btnRegistrar";
            btnRegistrar.NoAccentTextColor = Color.Empty;
            btnRegistrar.Size = new Size(170, 38);
            btnRegistrar.TabIndex = 0;
            btnRegistrar.Text = "REGISTRAR COMPRA";
            btnRegistrar.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnRegistrar.UseAccentColor = true;
            btnRegistrar.UseVisualStyleBackColor = true;
            //
            // btnLimpiar
            //
            btnLimpiar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnLimpiar.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnLimpiar.Depth = 0;
            btnLimpiar.HighEmphasis = true;
            btnLimpiar.Icon = null;
            btnLimpiar.Margin = new Padding(6, 4, 0, 4);
            btnLimpiar.MinimumSize = new Size(170, 38);
            btnLimpiar.MouseState = MaterialSkin.MouseState.HOVER;
            btnLimpiar.Name = "btnLimpiar";
            btnLimpiar.NoAccentTextColor = Color.Empty;
            btnLimpiar.Size = new Size(170, 38);
            btnLimpiar.TabIndex = 1;
            btnLimpiar.Text = "CANCELAR COMPRA";
            btnLimpiar.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            btnLimpiar.UseAccentColor = false;
            btnLimpiar.UseVisualStyleBackColor = true;
            //
            // lblMensaje
            //
            lblMensaje.AutoSize = false;
            lblMensaje.Depth = 0;
            lblMensaje.Dock = DockStyle.Fill;
            lblMensaje.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblMensaje.Location = new Point(0, 50);
            lblMensaje.Margin = new Padding(0);
            lblMensaje.MouseState = MaterialSkin.MouseState.HOVER;
            lblMensaje.Name = "lblMensaje";
            lblMensaje.Size = new Size(1164, 36);
            lblMensaje.TabIndex = 2;
            lblMensaje.TextAlign = ContentAlignment.MiddleLeft;
            //
            // pnlHistorial
            //
            pnlHistorial.BackColor = Color.FromArgb(45, 45, 45);
            pnlHistorial.Controls.Add(tblHistorial);
            pnlHistorial.Dock = DockStyle.Fill;
            pnlHistorial.Location = new Point(8, 624);
            pnlHistorial.Margin = new Padding(0);
            pnlHistorial.Name = "pnlHistorial";
            pnlHistorial.Padding = new Padding(12, 4, 12, 4);
            pnlHistorial.Size = new Size(1067, 184);
            pnlHistorial.TabIndex = 4;
            //
            // tblHistorial
            //
            tblHistorial.ColumnCount = 3;
            tblHistorial.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblHistorial.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblHistorial.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblHistorial.Controls.Add(lblTituloHistorial, 0, 0);
            tblHistorial.Controls.Add(lblMensajeHistorial, 1, 0);
            tblHistorial.Controls.Add(btnVerDetalle, 2, 0);
            tblHistorial.Controls.Add(gridHistorial, 0, 1);
            tblHistorial.Dock = DockStyle.Fill;
            tblHistorial.Location = new Point(12, 4);
            tblHistorial.Margin = new Padding(0);
            tblHistorial.Name = "tblHistorial";
            tblHistorial.RowCount = 2;
            tblHistorial.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tblHistorial.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblHistorial.SetColumnSpan(gridHistorial, 3);
            tblHistorial.Size = new Size(1043, 176);
            tblHistorial.TabIndex = 0;
            //
            // lblTituloHistorial
            //
            lblTituloHistorial.Anchor = AnchorStyles.Left;
            lblTituloHistorial.AutoSize = true;
            lblTituloHistorial.Depth = 0;
            lblTituloHistorial.Font = new Font("Roboto", 17F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblTituloHistorial.Location = new Point(0, 8);
            lblTituloHistorial.Margin = new Padding(0, 0, 16, 0);
            lblTituloHistorial.MouseState = MaterialSkin.MouseState.HOVER;
            lblTituloHistorial.Name = "lblTituloHistorial";
            lblTituloHistorial.Size = new Size(186, 20);
            lblTituloHistorial.TabIndex = 0;
            lblTituloHistorial.Text = "Historial de compras";
            //
            // lblMensajeHistorial
            //
            lblMensajeHistorial.AutoSize = false;
            lblMensajeHistorial.Depth = 0;
            lblMensajeHistorial.Dock = DockStyle.Fill;
            lblMensajeHistorial.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblMensajeHistorial.Location = new Point(202, 0);
            lblMensajeHistorial.Margin = new Padding(0);
            lblMensajeHistorial.MouseState = MaterialSkin.MouseState.HOVER;
            lblMensajeHistorial.Name = "lblMensajeHistorial";
            lblMensajeHistorial.Size = new Size(702, 36);
            lblMensajeHistorial.TabIndex = 1;
            lblMensajeHistorial.TextAlign = ContentAlignment.MiddleLeft;
            //
            // btnVerDetalle
            //
            btnVerDetalle.Anchor = AnchorStyles.Right;
            btnVerDetalle.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnVerDetalle.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnVerDetalle.Depth = 0;
            btnVerDetalle.HighEmphasis = true;
            btnVerDetalle.Icon = null;
            btnVerDetalle.Location = new Point(910, 1);
            btnVerDetalle.Margin = new Padding(6, 2, 0, 2);
            btnVerDetalle.MinimumSize = new Size(133, 34);
            btnVerDetalle.MouseState = MaterialSkin.MouseState.HOVER;
            btnVerDetalle.Name = "btnVerDetalle";
            btnVerDetalle.NoAccentTextColor = Color.Empty;
            btnVerDetalle.Size = new Size(133, 34);
            btnVerDetalle.TabIndex = 2;
            btnVerDetalle.Text = "Ver detalle";
            btnVerDetalle.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            btnVerDetalle.UseAccentColor = false;
            btnVerDetalle.UseVisualStyleBackColor = true;
            //
            // gridHistorial
            //
            gridHistorial.AllowUserToAddRows = false;
            gridHistorial.AllowUserToDeleteRows = false;
            gridHistorial.AllowUserToResizeRows = false;
            gridHistorial.AutoGenerateColumns = false;
            gridHistorial.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridHistorial.ColumnHeadersHeight = 34;
            gridHistorial.Columns.AddRange(new DataGridViewColumn[] { colHistorialNumero, colHistorialFecha, colHistorialProveedor, colHistorialUsuario, colHistorialTotal, colHistorialEstado });
            gridHistorial.Dock = DockStyle.Fill;
            gridHistorial.Location = new Point(0, 36);
            gridHistorial.Margin = new Padding(0, 4, 0, 0);
            gridHistorial.MultiSelect = false;
            gridHistorial.Name = "gridHistorial";
            gridHistorial.ReadOnly = true;
            gridHistorial.RowHeadersVisible = false;
            gridHistorial.RowTemplate.Height = 30;
            gridHistorial.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridHistorial.Size = new Size(1043, 136);
            gridHistorial.TabIndex = 3;
            //
            // colHistorialNumero
            //
            colHistorialNumero.DataPropertyName = "IdCompra";
            colHistorialNumero.FillWeight = 10F;
            colHistorialNumero.HeaderText = "N° compra";
            colHistorialNumero.Name = "colHistorialNumero";
            colHistorialNumero.ReadOnly = true;
            colHistorialNumero.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colHistorialFecha
            //
            colHistorialFecha.DataPropertyName = "FechaRegistro";
            colHistorialFecha.FillWeight = 18F;
            colHistorialFecha.HeaderText = "Fecha y hora";
            colHistorialFecha.Name = "colHistorialFecha";
            colHistorialFecha.ReadOnly = true;
            colHistorialFecha.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colHistorialProveedor
            //
            colHistorialProveedor.DataPropertyName = "RazonSocialProveedor";
            colHistorialProveedor.FillWeight = 30F;
            colHistorialProveedor.HeaderText = "Proveedor";
            colHistorialProveedor.Name = "colHistorialProveedor";
            colHistorialProveedor.ReadOnly = true;
            colHistorialProveedor.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colHistorialUsuario
            //
            colHistorialUsuario.DataPropertyName = "NombreUsuario";
            colHistorialUsuario.FillWeight = 18F;
            colHistorialUsuario.HeaderText = "Usuario";
            colHistorialUsuario.Name = "colHistorialUsuario";
            colHistorialUsuario.ReadOnly = true;
            colHistorialUsuario.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colHistorialTotal
            //
            colHistorialTotal.DataPropertyName = "MontoTotal";
            colHistorialTotal.FillWeight = 14F;
            colHistorialTotal.HeaderText = "Total";
            colHistorialTotal.Name = "colHistorialTotal";
            colHistorialTotal.ReadOnly = true;
            colHistorialTotal.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colHistorialEstado
            //
            colHistorialEstado.DataPropertyName = "Estado";
            colHistorialEstado.FillWeight = 10F;
            colHistorialEstado.HeaderText = "Estado";
            colHistorialEstado.Name = "colHistorialEstado";
            colHistorialEstado.ReadOnly = true;
            colHistorialEstado.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // ComprasForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(33, 33, 33);
            Controls.Add(pnlContenedor);
            Name = "ComprasForm";
            Size = new Size(1083, 760);
            pnlContenedor.ResumeLayout(false);
            tblPrincipal.ResumeLayout(false);
            pnlEncabezado.ResumeLayout(false);
            tblBloqueProducto.ResumeLayout(false);
            pnlDatosProducto.ResumeLayout(false);
            tblDatosProducto.ResumeLayout(false);
            tblProductoSeleccionado.ResumeLayout(false);
            tblCaptura.ResumeLayout(false);
            tblSubtotalAccion.ResumeLayout(false);
            tblSubtotalAccion.PerformLayout();
            pnlListadoProductos.ResumeLayout(false);
            tblListadoProductos.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridProductos).EndInit();
            tblBloqueProveedor.ResumeLayout(false);
            pnlDatosProveedor.ResumeLayout(false);
            tblDatosProveedor.ResumeLayout(false);
            pnlListadoProveedores.ResumeLayout(false);
            tblListadoProveedores.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridProveedores).EndInit();
            tblCompraActual.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridLineas).EndInit();
            tblPieCompra.ResumeLayout(false);
            tblPieCompra.PerformLayout();
            flpAcciones.ResumeLayout(false);
            flpAcciones.PerformLayout();
            pnlHistorial.ResumeLayout(false);
            tblHistorial.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridHistorial).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlContenedor;
        private TableLayoutPanel tblPrincipal;
        private Panel pnlEncabezado;
        private MaterialSkin.Controls.MaterialLabel lblTitulo;
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
        private FlowLayoutPanel flpAcciones;
        private MaterialSkin.Controls.MaterialButton btnRegistrar;
        private MaterialSkin.Controls.MaterialButton btnLimpiar;
        private MaterialSkin.Controls.MaterialLabel lblMensaje;
        private Panel pnlHistorial;
        private TableLayoutPanel tblHistorial;
        private MaterialSkin.Controls.MaterialLabel lblTituloHistorial;
        private MaterialSkin.Controls.MaterialLabel lblMensajeHistorial;
        private MaterialSkin.Controls.MaterialButton btnVerDetalle;
        private DataGridView gridHistorial;
        private DataGridViewTextBoxColumn colHistorialNumero;
        private DataGridViewTextBoxColumn colHistorialFecha;
        private DataGridViewTextBoxColumn colHistorialProveedor;
        private DataGridViewTextBoxColumn colHistorialUsuario;
        private DataGridViewTextBoxColumn colHistorialTotal;
        private DataGridViewTextBoxColumn colHistorialEstado;
    }
}
