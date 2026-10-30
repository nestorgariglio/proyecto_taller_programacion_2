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
            lblNombreProducto = new MaterialSkin.Controls.MaterialLabel();
            lblCodigoProducto = new MaterialSkin.Controls.MaterialLabel();
            txtCantidad = new MaterialSkin.Controls.MaterialTextBox2();
            txtPrecioCompra = new MaterialSkin.Controls.MaterialTextBox2();
            txtPrecioVenta = new MaterialSkin.Controls.MaterialTextBox2();
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
            tblBloqueProducto.SuspendLayout();
            pnlDatosProducto.SuspendLayout();
            tblDatosProducto.SuspendLayout();
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
            tblPrincipal.ColumnCount = 2;
            // Bloques gemelos simétricos (winforms-ui-ux §2): exactamente 50/50, sin
            // contenedor intermedio y sin el reparto desigual 52/48 de T18.
            tblPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblPrincipal.Controls.Add(pnlEncabezado, 0, 0);
            tblPrincipal.Controls.Add(tblBloqueProducto, 0, 1);
            tblPrincipal.Controls.Add(tblBloqueProveedor, 1, 1);
            tblPrincipal.Controls.Add(tblCompraActual, 0, 2);
            tblPrincipal.Controls.Add(flpAcciones, 0, 3);
            // Encabezado, compra actual y pie de acciones ocupan el ancho completo.
            tblPrincipal.SetColumnSpan(pnlEncabezado, 2);
            tblPrincipal.SetColumnSpan(tblCompraActual, 2);
            tblPrincipal.SetColumnSpan(flpAcciones, 2);
            tblPrincipal.Dock = DockStyle.Fill;
            tblPrincipal.Location = new Point(12, 12);
            tblPrincipal.Margin = new Padding(0);
            tblPrincipal.MinimumSize = new Size(980, 650);
            tblPrincipal.Name = "tblPrincipal";
            tblPrincipal.Padding = new Padding(0);
            // Filas: encabezado 35, bloques de búsqueda 230, compra actual con todo
            // el resto y pie de acciones 55 ⇒ 35 + 230 + 55 = 320 px fijos. El mínimo
            // de contenido de la fila 2 es 22 (título) + 180 (mínimo de gridLineas) +
            // 50 (tblPieCompra) = 252, así que el mínimo vigente de 650 px (≥ 320 +
            // 252 = 572) y el AutoScrollMinSize del contenedor siguen sincronizados.
            tblPrincipal.RowCount = 4;
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 230F));
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
            // tblBloqueProducto
            //
            // Bloque producto con datos a la izquierda (220 px fijos, ≤ 222) y
            // listado a la derecha con el resto del ancho: la grilla crece con la
            // ventana y la columna de datos nunca supera el ancho previsto.
            tblBloqueProducto.ColumnCount = 2;
            tblBloqueProducto.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
            tblBloqueProducto.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblBloqueProducto.Controls.Add(pnlDatosProducto, 0, 0);
            tblBloqueProducto.Controls.Add(pnlListadoProductos, 1, 0);
            tblBloqueProducto.Dock = DockStyle.Fill;
            tblBloqueProducto.Location = new Point(0, 35);
            tblBloqueProducto.Margin = new Padding(0, 0, 8, 0);
            tblBloqueProducto.Name = "tblBloqueProducto";
            tblBloqueProducto.RowCount = 1;
            tblBloqueProducto.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblBloqueProducto.Size = new Size(482, 230);
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
            pnlDatosProducto.Size = new Size(221, 230);
            pnlDatosProducto.TabIndex = 0;
            //
            // tblDatosProducto
            //
            tblDatosProducto.ColumnCount = 1;
            tblDatosProducto.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblDatosProducto.Controls.Add(lblTituloDatosProducto, 0, 0);
            tblDatosProducto.Controls.Add(lblNombreProducto, 0, 1);
            tblDatosProducto.Controls.Add(lblCodigoProducto, 0, 2);
            tblDatosProducto.Controls.Add(txtCantidad, 0, 3);
            tblDatosProducto.Controls.Add(txtPrecioCompra, 0, 4);
            tblDatosProducto.Controls.Add(txtPrecioVenta, 0, 5);
            tblDatosProducto.Controls.Add(lblSubtotalPreparacion, 0, 6);
            tblDatosProducto.Controls.Add(btnAgregarLinea, 0, 7);
            tblDatosProducto.Dock = DockStyle.Fill;
            tblDatosProducto.Location = new Point(10, 2);
            tblDatosProducto.Margin = new Padding(0);
            tblDatosProducto.Name = "tblDatosProducto";
            // Filas: título 20, nombre y código 22 cada uno, las tres entradas 30 cada
            // una, subtotal 22 y botón 40 ⇒ 216 px (≤ 222 de la fila 1 menos el
            // Padding del panel). Sin fila de relleno: el bloque termina en el botón
            // y no deja hueco muerto antes de «Compra Actual».
            tblDatosProducto.RowCount = 8;
            tblDatosProducto.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tblDatosProducto.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            tblDatosProducto.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            tblDatosProducto.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tblDatosProducto.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tblDatosProducto.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tblDatosProducto.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            tblDatosProducto.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tblDatosProducto.Size = new Size(201, 226);
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
            lblTituloDatosProducto.Size = new Size(211, 20);
            lblTituloDatosProducto.TabIndex = 0;
            lblTituloDatosProducto.Text = "Datos del producto";
            lblTituloDatosProducto.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblNombreProducto
            //
            lblNombreProducto.AutoSize = false;
            lblNombreProducto.Depth = 0;
            lblNombreProducto.Dock = DockStyle.Fill;
            lblNombreProducto.Font = new Font("Roboto", 15F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblNombreProducto.Location = new Point(0, 20);
            lblNombreProducto.Margin = new Padding(0);
            lblNombreProducto.MouseState = MaterialSkin.MouseState.HOVER;
            lblNombreProducto.Name = "lblNombreProducto";
            lblNombreProducto.Size = new Size(201, 22);
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
            lblCodigoProducto.Location = new Point(0, 44);
            lblCodigoProducto.Margin = new Padding(0);
            lblCodigoProducto.MouseState = MaterialSkin.MouseState.HOVER;
            lblCodigoProducto.Name = "lblCodigoProducto";
            lblCodigoProducto.Size = new Size(201, 22);
            lblCodigoProducto.TabIndex = 1;
            lblCodigoProducto.Text = "Código: —";
            lblCodigoProducto.TextAlign = ContentAlignment.MiddleLeft;
            // Cada entrada ocupa su propia fila de 30 px: el ancho útil de la columna
            // de datos (211 px) no admite tres campos en línea.
            txtCantidad.AnimateReadOnly = false;
            txtCantidad.BackgroundImageLayout = ImageLayout.None;
            txtCantidad.CharacterCasing = CharacterCasing.Normal;
            txtCantidad.Depth = 0;
            txtCantidad.Dock = DockStyle.Fill;
            txtCantidad.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtCantidad.Hint = "Cantidad";
            txtCantidad.Location = new Point(0, 68);
            txtCantidad.Margin = new Padding(0);
            txtCantidad.MaxLength = 32767;
            txtCantidad.MinimumSize = new Size(100, 30);
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
            txtPrecioCompra.Location = new Point(0, 98);
            txtPrecioCompra.Margin = new Padding(0);
            txtPrecioCompra.MaxLength = 32767;
            txtPrecioCompra.MinimumSize = new Size(100, 30);
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
            txtPrecioVenta.Location = new Point(0, 128);
            txtPrecioVenta.Margin = new Padding(0);
            txtPrecioVenta.MaxLength = 32767;
            txtPrecioVenta.MinimumSize = new Size(100, 30);
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
            // lblSubtotalPreparacion
            //
            lblSubtotalPreparacion.AutoSize = false;
            lblSubtotalPreparacion.Depth = 0;
            lblSubtotalPreparacion.Dock = DockStyle.Fill;
            lblSubtotalPreparacion.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblSubtotalPreparacion.Location = new Point(0, 158);
            lblSubtotalPreparacion.Margin = new Padding(0);
            lblSubtotalPreparacion.MouseState = MaterialSkin.MouseState.HOVER;
            lblSubtotalPreparacion.Name = "lblSubtotalPreparacion";
            lblSubtotalPreparacion.Size = new Size(201, 22);
            lblSubtotalPreparacion.TabIndex = 6;
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
            btnAgregarLinea.Location = new Point(0, 186);
            btnAgregarLinea.Margin = new Padding(0);
            btnAgregarLinea.MinimumSize = new Size(0, 40);
            btnAgregarLinea.MouseState = MaterialSkin.MouseState.HOVER;
            btnAgregarLinea.Name = "btnAgregarLinea";
            btnAgregarLinea.NoAccentTextColor = Color.Empty;
            btnAgregarLinea.Padding = new Padding(10, 0, 10, 0);
            btnAgregarLinea.Size = new Size(190, 40);
            btnAgregarLinea.TabIndex = 7;
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
            pnlListadoProductos.Location = new Point(231, 0);
            pnlListadoProductos.Margin = new Padding(0);
            pnlListadoProductos.Name = "pnlListadoProductos";
            pnlListadoProductos.Padding = new Padding(10, 2, 10, 2);
            pnlListadoProductos.Size = new Size(281, 230);
            pnlListadoProductos.TabIndex = 1;
            //
            // tblListadoProductos
            //
            // Cuatro filas en una sola columna: rótulo, buscador propio (32 px) y
            // grilla, de modo que el buscador nunca comparte fila con la grilla ni
            // la tapa. Todo se posiciona con Dock = Fill: sin Location absoluto.
            tblListadoProductos.ColumnCount = 1;
            tblListadoProductos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblListadoProductos.Controls.Add(lblTituloListadoProductos, 0, 0);
            tblListadoProductos.Controls.Add(txtBuscarProducto, 0, 1);
            tblListadoProductos.Controls.Add(gridProductos, 0, 2);
            tblListadoProductos.Controls.Add(lblMensajeProductos, 0, 3);
            tblListadoProductos.Dock = DockStyle.Fill;
            tblListadoProductos.Location = new Point(10, 2);
            tblListadoProductos.Margin = new Padding(0);
            tblListadoProductos.Name = "tblListadoProductos";
            tblListadoProductos.RowCount = 4;
            tblListadoProductos.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tblListadoProductos.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tblListadoProductos.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblListadoProductos.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tblListadoProductos.Size = new Size(261, 226);
            tblListadoProductos.TabIndex = 0;
            //
            // lblTituloListadoProductos
            //
            lblTituloListadoProductos.Anchor = AnchorStyles.Left;
            lblTituloListadoProductos.AutoSize = true;
            lblTituloListadoProductos.Depth = 0;
            lblTituloListadoProductos.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblTituloListadoProductos.Margin = new Padding(0);
            lblTituloListadoProductos.MouseState = MaterialSkin.MouseState.HOVER;
            lblTituloListadoProductos.Name = "lblTituloListadoProductos";
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
            txtBuscarProducto.Margin = new Padding(0);
            txtBuscarProducto.MaxLength = 32767;
            // El buscador ocupa su propia fila de 32 px con Dock = Fill; el mínimo
            // nunca puede superar el alto de la fila (si lo supera, se derrama
            // sobre el encabezado de la grilla).
            txtBuscarProducto.MinimumSize = new Size(100, 32);
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
            gridProductos.Margin = new Padding(0);
            // La grilla ocupa la fila "Percent 100" del listado y su propio alto se
            // resuelve por Dock; el mínimo garantiza cabecera y primeras filas. Con
            // ScrollBars = Vertical el alto del contenido no ensancha las columnas.
            gridProductos.MinimumSize = new Size(0, 100);
            gridProductos.MultiSelect = false;
            gridProductos.Name = "gridProductos";
            gridProductos.ReadOnly = true;
            gridProductos.RowHeadersVisible = false;
            gridProductos.RowTemplate.Height = 21;
            gridProductos.ScrollBars = ScrollBars.Vertical;
            gridProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
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
            lblMensajeProductos.Margin = new Padding(0);
            lblMensajeProductos.MinimumSize = new Size(0, 18);
            lblMensajeProductos.MouseState = MaterialSkin.MouseState.HOVER;
            lblMensajeProductos.Name = "lblMensajeProductos";
            lblMensajeProductos.Size = new Size(261, 18);
            lblMensajeProductos.TabIndex = 3;
            lblMensajeProductos.TextAlign = ContentAlignment.MiddleLeft;
            //
            // tblBloqueProveedor
            //
            // Bloque proveedor simétrico al de producto: datos a la izquierda
            // (220 px fijos) y listado a la derecha con el resto del ancho.
            tblBloqueProveedor.ColumnCount = 2;
            tblBloqueProveedor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
            tblBloqueProveedor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblBloqueProveedor.Controls.Add(pnlDatosProveedor, 0, 0);
            tblBloqueProveedor.Controls.Add(pnlListadoProveedores, 1, 0);
            tblBloqueProveedor.Dock = DockStyle.Fill;
            tblBloqueProveedor.Location = new Point(498, 35);
            tblBloqueProveedor.Margin = new Padding(8, 0, 0, 0);
            tblBloqueProveedor.Name = "tblBloqueProveedor";
            tblBloqueProveedor.RowCount = 1;
            tblBloqueProveedor.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblBloqueProveedor.Size = new Size(482, 230);
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
            pnlDatosProveedor.Size = new Size(207, 230);
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
            // Título 20 y los dos datos del proveedor elegido 30 cada uno ⇒ 80 px
            // fijos (≤ 222). Sin fila de relleno: el bloque termina en el dato del
            // documento y no deja hueco muerto.
            tblDatosProveedor.RowCount = 3;
            tblDatosProveedor.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tblDatosProveedor.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tblDatosProveedor.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tblDatosProveedor.Size = new Size(187, 226);
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
            lblTituloDatosProveedor.Size = new Size(187, 20);
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
            lblRazonSocialProveedor.Size = new Size(187, 30);
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
            lblDocumentoProveedor.Location = new Point(0, 50);
            lblDocumentoProveedor.Margin = new Padding(0);
            lblDocumentoProveedor.MouseState = MaterialSkin.MouseState.HOVER;
            lblDocumentoProveedor.Name = "lblDocumentoProveedor";
            lblDocumentoProveedor.Size = new Size(187, 30);
            lblDocumentoProveedor.TabIndex = 2;
            lblDocumentoProveedor.Text = "Documento: —";
            lblDocumentoProveedor.TextAlign = ContentAlignment.MiddleLeft;
            //
            // pnlListadoProveedores
            //
            pnlListadoProveedores.BackColor = Color.FromArgb(45, 45, 45);
            pnlListadoProveedores.Controls.Add(tblListadoProveedores);
            pnlListadoProveedores.Dock = DockStyle.Fill;
            pnlListadoProveedores.Location = new Point(216, 0);
            pnlListadoProveedores.Margin = new Padding(0);
            pnlListadoProveedores.Name = "pnlListadoProveedores";
            pnlListadoProveedores.Padding = new Padding(10, 2, 10, 2);
            pnlListadoProveedores.Size = new Size(263, 230);
            pnlListadoProveedores.TabIndex = 1;
            //
            // tblListadoProveedores
            //
            // Cuatro filas en una sola columna: rótulo, buscador propio (32 px) y
            // grilla, simétrico al listado de productos. Sin Location absoluto.
            tblListadoProveedores.ColumnCount = 1;
            tblListadoProveedores.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblListadoProveedores.Controls.Add(lblTituloListadoProveedores, 0, 0);
            tblListadoProveedores.Controls.Add(txtBuscarProveedor, 0, 1);
            tblListadoProveedores.Controls.Add(gridProveedores, 0, 2);
            tblListadoProveedores.Controls.Add(lblMensajeProveedores, 0, 3);
            tblListadoProveedores.Dock = DockStyle.Fill;
            tblListadoProveedores.Location = new Point(10, 2);
            tblListadoProveedores.Margin = new Padding(0);
            tblListadoProveedores.Name = "tblListadoProveedores";
            tblListadoProveedores.RowCount = 4;
            tblListadoProveedores.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tblListadoProveedores.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tblListadoProveedores.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblListadoProveedores.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tblListadoProveedores.Size = new Size(243, 226);
            tblListadoProveedores.TabIndex = 0;
            //
            // lblTituloListadoProveedores
            //
            lblTituloListadoProveedores.Anchor = AnchorStyles.Left;
            lblTituloListadoProveedores.AutoSize = true;
            lblTituloListadoProveedores.Depth = 0;
            lblTituloListadoProveedores.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblTituloListadoProveedores.Margin = new Padding(0);
            lblTituloListadoProveedores.MouseState = MaterialSkin.MouseState.HOVER;
            lblTituloListadoProveedores.Name = "lblTituloListadoProveedores";
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
            txtBuscarProveedor.Margin = new Padding(0);
            txtBuscarProveedor.MaxLength = 32767;
            // El buscador ocupa su propia fila de 32 px con Dock = Fill; el mínimo
            // nunca puede superar el alto de la fila.
            txtBuscarProveedor.MinimumSize = new Size(100, 32);
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
            gridProveedores.Margin = new Padding(0);
            // La grilla ocupa la fila "Percent 100" del listado; el alto se resuelve
            // por Dock y el mínimo garantiza cabecera y primeras filas. Con
            // ScrollBars = Vertical el contenido no ensancha las columnas.
            gridProveedores.MinimumSize = new Size(0, 100);
            gridProveedores.MultiSelect = false;
            gridProveedores.Name = "gridProveedores";
            gridProveedores.ReadOnly = true;
            gridProveedores.RowHeadersVisible = false;
            gridProveedores.RowTemplate.Height = 21;
            gridProveedores.ScrollBars = ScrollBars.Vertical;
            gridProveedores.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
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
            lblMensajeProveedores.Margin = new Padding(0);
            lblMensajeProveedores.MinimumSize = new Size(0, 18);
            lblMensajeProveedores.MouseState = MaterialSkin.MouseState.HOVER;
            lblMensajeProveedores.Name = "lblMensajeProveedores";
            lblMensajeProveedores.Size = new Size(243, 18);
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
            tblCompraActual.Location = new Point(0, 265);
            tblCompraActual.Margin = new Padding(0);
            tblCompraActual.Name = "tblCompraActual";
            tblCompraActual.RowCount = 3;
            tblCompraActual.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            tblCompraActual.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblCompraActual.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tblCompraActual.Size = new Size(980, 335);
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
            gridLineas.Margin = new Padding(0);
            // Mínimo de 180 px: cabecera (40) más al menos tres líneas de 34. La
            // barra vertical aparece sola y la horizontal solo si el ancho lo exige.
            gridLineas.MinimumSize = new Size(0, 180);
            gridLineas.MultiSelect = false;
            gridLineas.Name = "gridLineas";
            gridLineas.ReadOnly = true;
            gridLineas.RowHeadersVisible = false;
            gridLineas.RowTemplate.Height = 34;
            gridLineas.ScrollBars = ScrollBars.Both;
            gridLineas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
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
            colLineaCantidad.DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight };
            colLineaCantidad.FillWeight = 10F;
            colLineaCantidad.HeaderText = "Cantidad";
            colLineaCantidad.Name = "colLineaCantidad";
            colLineaCantidad.ReadOnly = true;
            colLineaCantidad.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colLineaPrecioCompra
            //
            colLineaPrecioCompra.DataPropertyName = "PrecioCompra";
            colLineaPrecioCompra.DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight };
            colLineaPrecioCompra.FillWeight = 14F;
            colLineaPrecioCompra.HeaderText = "Precio compra";
            colLineaPrecioCompra.Name = "colLineaPrecioCompra";
            colLineaPrecioCompra.ReadOnly = true;
            colLineaPrecioCompra.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colLineaPrecioVenta
            //
            colLineaPrecioVenta.DataPropertyName = "PrecioVenta";
            colLineaPrecioVenta.DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight };
            colLineaPrecioVenta.FillWeight = 14F;
            colLineaPrecioVenta.HeaderText = "Precio venta";
            colLineaPrecioVenta.Name = "colLineaPrecioVenta";
            colLineaPrecioVenta.ReadOnly = true;
            colLineaPrecioVenta.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colLineaSubtotal
            //
            colLineaSubtotal.DataPropertyName = "Subtotal";
            colLineaSubtotal.DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight };
            colLineaSubtotal.FillWeight = 14F;
            colLineaSubtotal.HeaderText = "Subtotal";
            colLineaSubtotal.Name = "colLineaSubtotal";
            colLineaSubtotal.ReadOnly = true;
            colLineaSubtotal.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colLineaEditar
            //
            colLineaEditar.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colLineaEditar.FlatStyle = FlatStyle.Flat;
            colLineaEditar.HeaderText = "";
            colLineaEditar.MinimumWidth = 60;
            colLineaEditar.Name = "colLineaEditar";
            colLineaEditar.ReadOnly = true;
            colLineaEditar.Resizable = DataGridViewTriState.False;
            colLineaEditar.SortMode = DataGridViewColumnSortMode.NotSortable;
            colLineaEditar.Text = "Editar";
            colLineaEditar.UseColumnTextForButtonValue = true;
            colLineaEditar.Width = 60;
            //
            // colLineaQuitar
            //
            colLineaQuitar.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colLineaQuitar.FlatStyle = FlatStyle.Flat;
            colLineaQuitar.HeaderText = "";
            colLineaQuitar.MinimumWidth = 60;
            colLineaQuitar.Name = "colLineaQuitar";
            colLineaQuitar.ReadOnly = true;
            colLineaQuitar.Resizable = DataGridViewTriState.False;
            colLineaQuitar.SortMode = DataGridViewColumnSortMode.NotSortable;
            colLineaQuitar.Text = "Quitar";
            colLineaQuitar.UseColumnTextForButtonValue = true;
            colLineaQuitar.Width = 60;
            //
            // tblPieCompra
            //
            tblPieCompra.ColumnCount = 1;
            tblPieCompra.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblPieCompra.Controls.Add(lblMontoTotal, 0, 0);
            tblPieCompra.Controls.Add(lblMensaje, 0, 1);
            tblPieCompra.Dock = DockStyle.Fill;
            tblPieCompra.Location = new Point(0, 285);
            tblPieCompra.Margin = new Padding(0);
            tblPieCompra.Name = "tblPieCompra";
            tblPieCompra.RowCount = 2;
            tblPieCompra.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tblPieCompra.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
            tblPieCompra.Size = new Size(980, 50);
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
            lblMontoTotal.Size = new Size(980, 32);
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
            lblMensaje.Location = new Point(0, 32);
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
            btnRegistrar.Margin = new Padding(6, 2, 6, 2);
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
            btnLimpiar.Margin = new Padding(6, 2, 6, 2);
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
            btnVerHistorial.Margin = new Padding(6, 2, 6, 2);
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
            tblBloqueProducto.ResumeLayout(false);
            pnlDatosProducto.ResumeLayout(false);
            tblDatosProducto.ResumeLayout(false);
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
        private TableLayoutPanel tblBloqueProducto;
        private Panel pnlDatosProducto;
        private TableLayoutPanel tblDatosProducto;
        private MaterialSkin.Controls.MaterialLabel lblTituloDatosProducto;
        private MaterialSkin.Controls.MaterialLabel lblNombreProducto;
        private MaterialSkin.Controls.MaterialLabel lblCodigoProducto;
        private MaterialSkin.Controls.MaterialTextBox2 txtCantidad;
        private MaterialSkin.Controls.MaterialTextBox2 txtPrecioCompra;
        private MaterialSkin.Controls.MaterialTextBox2 txtPrecioVenta;
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
