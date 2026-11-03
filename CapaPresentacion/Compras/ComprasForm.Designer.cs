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
            pnlContenedor = new Panel();
            tblPrincipal = new TableLayoutPanel();
            lblTitulo = new MaterialSkin.Controls.MaterialLabel();
            tblCatalogo = new TableLayoutPanel();
            cardProveedor = new MaterialSkin.Controls.MaterialCard();
            tblListadoProveedores = new TableLayoutPanel();
            tblTituloProveedores = new TableLayoutPanel();
            lblTituloListadoProveedores = new MaterialSkin.Controls.MaterialLabel();
            chkMostrarTodosProveedores = new MaterialSkin.Controls.MaterialCheckbox();
            txtBuscarProveedor = new MaterialSkin.Controls.MaterialTextBox2();
            gridProveedores = new DataGridView();
            colProveedorRazonSocial = new DataGridViewTextBoxColumn();
            colProveedorDocumento = new DataGridViewTextBoxColumn();
            lblMensajeProveedores = new MaterialSkin.Controls.MaterialLabel();
            cardProducto = new MaterialSkin.Controls.MaterialCard();
            tblListadoProductos = new TableLayoutPanel();
            tblTituloProductos = new TableLayoutPanel();
            lblTituloListadoProductos = new MaterialSkin.Controls.MaterialLabel();
            chkMostrarTodosProductos = new MaterialSkin.Controls.MaterialCheckbox();
            txtBuscarProducto = new MaterialSkin.Controls.MaterialTextBox2();
            gridProductos = new DataGridView();
            colProductoCodigo = new DataGridViewTextBoxColumn();
            colProductoNombre = new DataGridViewTextBoxColumn();
            lblMensajeProductos = new MaterialSkin.Controls.MaterialLabel();
            tblDocumento = new TableLayoutPanel();
            cardDatosProveedor = new MaterialSkin.Controls.MaterialCard();
            tblDatosProveedor = new TableLayoutPanel();
            lblTituloDatosProveedor = new MaterialSkin.Controls.MaterialLabel();
            lblRazonSocialProveedor = new MaterialSkin.Controls.MaterialLabel();
            lblDocumentoProveedor = new MaterialSkin.Controls.MaterialLabel();
            cardDatosProducto = new MaterialSkin.Controls.MaterialCard();
            tblDatosProducto = new TableLayoutPanel();
            lblTituloDatosProducto = new MaterialSkin.Controls.MaterialLabel();
            lblNombreProducto = new MaterialSkin.Controls.MaterialLabel();
            lblCodigoProducto = new MaterialSkin.Controls.MaterialLabel();
            lblSubtotalPreparacion = new MaterialSkin.Controls.MaterialLabel();
            lblRotuloCantidad = new MaterialSkin.Controls.MaterialLabel();
            lblRotuloPrecioCompra = new MaterialSkin.Controls.MaterialLabel();
            lblRotuloPrecioVenta = new MaterialSkin.Controls.MaterialLabel();
            txtCantidad =new MaterialSkin.Controls.MaterialTextBox2();
            txtPrecioCompra = new MaterialSkin.Controls.MaterialTextBox2();
            txtPrecioVenta = new MaterialSkin.Controls.MaterialTextBox2();
            btnAgregarLinea = new MaterialSkin.Controls.MaterialButton();
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
            lblMensaje = new MaterialSkin.Controls.MaterialLabel();
            lblMontoTotal = new MaterialSkin.Controls.MaterialLabel();
            flpAcciones = new TableLayoutPanel();
            btnVerHistorial = new MaterialSkin.Controls.MaterialButton();
            btnLimpiar = new MaterialSkin.Controls.MaterialButton();
            btnRegistrar = new MaterialSkin.Controls.MaterialButton();
            pnlContenedor.SuspendLayout();
            tblPrincipal.SuspendLayout();
            tblCatalogo.SuspendLayout();
            cardProveedor.SuspendLayout();
            tblListadoProveedores.SuspendLayout();
            tblTituloProveedores.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridProveedores).BeginInit();
            cardProducto.SuspendLayout();
            tblListadoProductos.SuspendLayout();
            tblTituloProductos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridProductos).BeginInit();
            tblDocumento.SuspendLayout();
            cardDatosProveedor.SuspendLayout();
            tblDatosProveedor.SuspendLayout();
            cardDatosProducto.SuspendLayout();
            tblDatosProducto.SuspendLayout();
            tblCompraActual.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridLineas).BeginInit();
            tblPieCompra.SuspendLayout();
            flpAcciones.SuspendLayout();
            SuspendLayout();
            //
            // pnlContenedor
            //
            // Único contenedor con desplazamiento: solo vertical (el ancho 0 impide la
            // barra horizontal). El alto mínimo real depende del DPI y de la
            // disposición vigente, de modo que lo fija ComprasForm.cs tras
            // InitializeComponent; el valor de aquí es la base a 96 DPI.
            pnlContenedor.AutoScroll = true;
            pnlContenedor.AutoScrollMinSize = new Size(0, 456);
            pnlContenedor.BackColor = Color.FromArgb(33, 33, 33);
            pnlContenedor.Controls.Add(tblPrincipal);
            pnlContenedor.Dock = DockStyle.Fill;
            pnlContenedor.Location = new Point(0, 0);
            pnlContenedor.Margin = new Padding(0);
            pnlContenedor.Name = "pnlContenedor";
            pnlContenedor.Padding = new Padding(12);
            pnlContenedor.Size = new Size(1083, 700);
            pnlContenedor.TabIndex = 0;
            //
            // tblPrincipal
            //
            // Catálogo a la izquierda (32 %) y documento a la derecha (68 %). La fila 2
            // (alto 0) solo se usa cuando ComprasForm.cs apila las columnas en una.
            tblPrincipal.AutoSize = false;
            tblPrincipal.ColumnCount = 2;
            tblPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tblPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            tblPrincipal.Controls.Add(lblTitulo, 0, 0);
            tblPrincipal.Controls.Add(tblCatalogo, 0, 1);
            tblPrincipal.Controls.Add(tblDocumento, 1, 1);
            tblPrincipal.SetColumnSpan(lblTitulo, 2);
            tblPrincipal.Dock = DockStyle.Fill;
            tblPrincipal.Location = new Point(12, 12);
            tblPrincipal.Margin = new Padding(0);
            tblPrincipal.Name = "tblPrincipal";
            tblPrincipal.Padding = new Padding(0);
            tblPrincipal.RowCount = 3;
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 0F));
            tblPrincipal.Size = new Size(1059, 676);
            tblPrincipal.TabIndex = 0;
            //
            // lblTitulo
            //
            lblTitulo.AutoSize = false;
            lblTitulo.Depth = 0;
            lblTitulo.Dock = DockStyle.Fill;
            lblTitulo.FontType = MaterialSkin.MaterialSkinManager.fontType.H6;
            lblTitulo.Location = new Point(0, 0);
            lblTitulo.Margin = new Padding(0);
            lblTitulo.MouseState = MaterialSkin.MouseState.HOVER;
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(1059, 40);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Registro de Compras a Proveedores";
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            //
            // tblCatalogo
            //
            // Columna «Catálogo»: tarjeta de proveedor (arriba) y de producto (abajo).
            tblCatalogo.ColumnCount = 1;
            tblCatalogo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblCatalogo.Controls.Add(cardProveedor, 0, 0);
            tblCatalogo.Controls.Add(cardProducto, 0, 1);
            tblCatalogo.Dock = DockStyle.Fill;
            tblCatalogo.Location = new Point(0, 40);
            tblCatalogo.Margin = new Padding(0, 0, 6, 0);
            tblCatalogo.Name = "tblCatalogo";
            tblCatalogo.RowCount = 2;
            tblCatalogo.RowStyles.Add(new RowStyle(SizeType.Percent, 46F));
            tblCatalogo.RowStyles.Add(new RowStyle(SizeType.Percent, 54F));
            tblCatalogo.Size = new Size(333, 636);
            tblCatalogo.TabIndex = 1;
            //
            // cardProveedor
            //
            cardProveedor.Controls.Add(tblListadoProveedores);
            cardProveedor.Depth = 0;
            cardProveedor.Dock = DockStyle.Fill;
            cardProveedor.Location = new Point(0, 0);
            cardProveedor.Margin = new Padding(0, 0, 0, 4);
            cardProveedor.MouseState = MaterialSkin.MouseState.HOVER;
            cardProveedor.Name = "cardProveedor";
            cardProveedor.Padding = new Padding(12, 8, 12, 8);
            cardProveedor.Size = new Size(333, 263);
            cardProveedor.TabIndex = 0;
            //
            // tblListadoProveedores
            //
            // Título de tarjeta, buscador, grilla (absorbe el alto) y mensaje.
            tblListadoProveedores.ColumnCount = 1;
            tblListadoProveedores.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblListadoProveedores.Controls.Add(tblTituloProveedores, 0, 0);
            tblListadoProveedores.Controls.Add(txtBuscarProveedor, 0, 1);
            tblListadoProveedores.Controls.Add(gridProveedores, 0, 2);
            tblListadoProveedores.Controls.Add(lblMensajeProveedores, 0, 3);
            tblListadoProveedores.Dock = DockStyle.Fill;
            tblListadoProveedores.Location = new Point(12, 8);
            tblListadoProveedores.Margin = new Padding(0);
            tblListadoProveedores.Name = "tblListadoProveedores";
            tblListadoProveedores.RowCount = 4;
            tblListadoProveedores.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tblListadoProveedores.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tblListadoProveedores.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblListadoProveedores.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tblListadoProveedores.Size = new Size(309, 247);
            tblListadoProveedores.TabIndex = 0;
            //
            // tblTituloProveedores
            //
            // Fila del título: el título absorbe el ancho y el checkbox se ajusta a la derecha.
            tblTituloProveedores.ColumnCount = 2;
            tblTituloProveedores.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblTituloProveedores.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblTituloProveedores.Controls.Add(lblTituloListadoProveedores, 0, 0);
            tblTituloProveedores.Controls.Add(chkMostrarTodosProveedores, 1, 0);
            tblTituloProveedores.Dock = DockStyle.Fill;
            tblTituloProveedores.Margin = new Padding(0);
            tblTituloProveedores.Name = "tblTituloProveedores";
            tblTituloProveedores.RowCount = 1;
            tblTituloProveedores.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblTituloProveedores.Size = new Size(309, 24);
            tblTituloProveedores.TabIndex = 0;
            //
            //
            // lblTituloListadoProveedores
            //
            lblTituloListadoProveedores.AutoSize = false;
            lblTituloListadoProveedores.Depth = 0;
            lblTituloListadoProveedores.Dock = DockStyle.Fill;
            lblTituloListadoProveedores.FontType = MaterialSkin.MaterialSkinManager.fontType.Subtitle1;
            lblTituloListadoProveedores.Margin = new Padding(0);
            lblTituloListadoProveedores.MouseState = MaterialSkin.MouseState.HOVER;
            lblTituloListadoProveedores.Name = "lblTituloListadoProveedores";
            lblTituloListadoProveedores.Size = new Size(309, 24);
            lblTituloListadoProveedores.TabIndex = 0;
            lblTituloListadoProveedores.Text = "1 · Proveedor";
            lblTituloListadoProveedores.TextAlign = ContentAlignment.MiddleLeft;
            //
            // chkMostrarTodosProveedores
            //
            chkMostrarTodosProveedores.AutoSize = false;
            chkMostrarTodosProveedores.Depth = 0;
            chkMostrarTodosProveedores.Dock = DockStyle.Fill;
            chkMostrarTodosProveedores.Margin = new Padding(0);
            chkMostrarTodosProveedores.MouseLocation = new Point(-1, -1);
            chkMostrarTodosProveedores.MouseState = MaterialSkin.MouseState.HOVER;
            chkMostrarTodosProveedores.Name = "chkMostrarTodosProveedores";
            chkMostrarTodosProveedores.Ripple = true;
            chkMostrarTodosProveedores.Size = new Size(124, 24);
            chkMostrarTodosProveedores.TabIndex = 1;
            chkMostrarTodosProveedores.Text = "Mostrar todos";
            chkMostrarTodosProveedores.UseVisualStyleBackColor = true;
            //
            // txtBuscarProveedor
            //
            txtBuscarProveedor.AnimateReadOnly = false;
            txtBuscarProveedor.BackgroundImageLayout = ImageLayout.None;
            txtBuscarProveedor.CharacterCasing = CharacterCasing.Normal;
            txtBuscarProveedor.Depth = 0;
            txtBuscarProveedor.Dock = DockStyle.Fill;
            txtBuscarProveedor.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point);
            txtBuscarProveedor.Hint = "Buscar proveedor";
            txtBuscarProveedor.Margin = new Padding(0, 2, 0, 0);
            txtBuscarProveedor.MaxLength = 32767;
            txtBuscarProveedor.MinimumSize = new Size(100, 36);
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
            txtBuscarProveedor.Size = new Size(309, 36);
            txtBuscarProveedor.TabIndex = 1;
            txtBuscarProveedor.TextAlign = HorizontalAlignment.Left;
            txtBuscarProveedor.TrailingIcon = null;
            txtBuscarProveedor.UseSystemPasswordChar = false;
            txtBuscarProveedor.UseTallSize = false;
            //
            // gridProveedores
            //
            gridProveedores.AllowUserToAddRows = false;
            gridProveedores.AllowUserToDeleteRows = false;
            gridProveedores.AllowUserToResizeRows = false;
            gridProveedores.AutoGenerateColumns = false;
            gridProveedores.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridProveedores.ColumnHeadersHeight = 30;
            gridProveedores.Columns.AddRange(new DataGridViewColumn[] { colProveedorRazonSocial, colProveedorDocumento });
            gridProveedores.Dock = DockStyle.Fill;
            gridProveedores.Margin = new Padding(0, 4, 0, 0);
            gridProveedores.MinimumSize = new Size(0, 72);
            gridProveedores.MultiSelect = false;
            gridProveedores.Name = "gridProveedores";
            gridProveedores.ReadOnly = true;
            gridProveedores.RowHeadersVisible = false;
            gridProveedores.RowTemplate.Height = 26;
            gridProveedores.ScrollBars = ScrollBars.Vertical;
            gridProveedores.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridProveedores.Size = new Size(309, 161);
            gridProveedores.TabIndex = 2;
            //
            // colProveedorRazonSocial
            //
            colProveedorRazonSocial.DataPropertyName = "RazonSocial";
            colProveedorRazonSocial.FillWeight = 50F;
            colProveedorRazonSocial.HeaderText = "Razón social";
            colProveedorRazonSocial.Name = "colProveedorRazonSocial";
            colProveedorRazonSocial.ReadOnly = true;
            colProveedorRazonSocial.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colProveedorDocumento
            //
            colProveedorDocumento.DataPropertyName = "Documento";
            colProveedorDocumento.FillWeight = 50F;
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
            lblMensajeProveedores.FontType = MaterialSkin.MaterialSkinManager.fontType.Caption;
            lblMensajeProveedores.Margin = new Padding(0);
            lblMensajeProveedores.MouseState = MaterialSkin.MouseState.HOVER;
            lblMensajeProveedores.Name = "lblMensajeProveedores";
            lblMensajeProveedores.Size = new Size(309, 20);
            lblMensajeProveedores.TabIndex = 3;
            lblMensajeProveedores.TextAlign = ContentAlignment.MiddleLeft;
            //
            // cardProducto
            //
            cardProducto.Controls.Add(tblListadoProductos);
            cardProducto.Depth = 0;
            cardProducto.Dock = DockStyle.Fill;
            cardProducto.Location = new Point(0, 267);
            cardProducto.Margin = new Padding(0, 4, 0, 0);
            cardProducto.MouseState = MaterialSkin.MouseState.HOVER;
            cardProducto.Name = "cardProducto";
            cardProducto.Padding = new Padding(12, 8, 12, 8);
            cardProducto.Size = new Size(333, 369);
            cardProducto.TabIndex = 1;
            //
            // tblListadoProductos
            //
            tblListadoProductos.ColumnCount = 1;
            tblListadoProductos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblListadoProductos.Controls.Add(tblTituloProductos, 0, 0);
            tblListadoProductos.Controls.Add(txtBuscarProducto, 0, 1);
            tblListadoProductos.Controls.Add(gridProductos, 0, 2);
            tblListadoProductos.Controls.Add(lblMensajeProductos, 0, 3);
            tblListadoProductos.Dock = DockStyle.Fill;
            tblListadoProductos.Location = new Point(12, 8);
            tblListadoProductos.Margin = new Padding(0);
            tblListadoProductos.Name = "tblListadoProductos";
            tblListadoProductos.RowCount = 4;
            tblListadoProductos.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tblListadoProductos.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tblListadoProductos.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblListadoProductos.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tblListadoProductos.Size = new Size(309, 353);
            tblListadoProductos.TabIndex = 0;
            //
            // tblTituloProductos
            //
            // Fila del título: el título absorbe el ancho y el checkbox se ajusta a la derecha.
            tblTituloProductos.ColumnCount = 2;
            tblTituloProductos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblTituloProductos.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblTituloProductos.Controls.Add(lblTituloListadoProductos, 0, 0);
            tblTituloProductos.Controls.Add(chkMostrarTodosProductos, 1, 0);
            tblTituloProductos.Dock = DockStyle.Fill;
            tblTituloProductos.Margin = new Padding(0);
            tblTituloProductos.Name = "tblTituloProductos";
            tblTituloProductos.RowCount = 1;
            tblTituloProductos.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblTituloProductos.Size = new Size(309, 24);
            tblTituloProductos.TabIndex = 0;
            //
            //
            // lblTituloListadoProductos
            //
            lblTituloListadoProductos.AutoSize = false;
            lblTituloListadoProductos.Depth = 0;
            lblTituloListadoProductos.Dock = DockStyle.Fill;
            lblTituloListadoProductos.FontType = MaterialSkin.MaterialSkinManager.fontType.Subtitle1;
            lblTituloListadoProductos.Margin = new Padding(0);
            lblTituloListadoProductos.MouseState = MaterialSkin.MouseState.HOVER;
            lblTituloListadoProductos.Name = "lblTituloListadoProductos";
            lblTituloListadoProductos.Size = new Size(309, 24);
            lblTituloListadoProductos.TabIndex = 0;
            lblTituloListadoProductos.Text = "2 · Producto";
            lblTituloListadoProductos.TextAlign = ContentAlignment.MiddleLeft;
            //
            // chkMostrarTodosProductos
            //
            chkMostrarTodosProductos.AutoSize = false;
            chkMostrarTodosProductos.Depth = 0;
            chkMostrarTodosProductos.Dock = DockStyle.Fill;
            chkMostrarTodosProductos.Margin = new Padding(0);
            chkMostrarTodosProductos.MouseLocation = new Point(-1, -1);
            chkMostrarTodosProductos.MouseState = MaterialSkin.MouseState.HOVER;
            chkMostrarTodosProductos.Name = "chkMostrarTodosProductos";
            chkMostrarTodosProductos.Ripple = true;
            chkMostrarTodosProductos.Size = new Size(124, 24);
            chkMostrarTodosProductos.TabIndex = 1;
            chkMostrarTodosProductos.Text = "Mostrar todos";
            chkMostrarTodosProductos.UseVisualStyleBackColor = true;
            //
            // txtBuscarProducto
            //
            txtBuscarProducto.AnimateReadOnly = false;
            txtBuscarProducto.BackgroundImageLayout = ImageLayout.None;
            txtBuscarProducto.CharacterCasing = CharacterCasing.Normal;
            txtBuscarProducto.Depth = 0;
            txtBuscarProducto.Dock = DockStyle.Fill;
            txtBuscarProducto.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point);
            txtBuscarProducto.Hint = "Buscar producto";
            txtBuscarProducto.Margin = new Padding(0, 2, 0, 0);
            txtBuscarProducto.MaxLength = 32767;
            txtBuscarProducto.MinimumSize = new Size(100, 36);
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
            txtBuscarProducto.Size = new Size(309, 36);
            txtBuscarProducto.TabIndex = 1;
            txtBuscarProducto.TextAlign = HorizontalAlignment.Left;
            txtBuscarProducto.TrailingIcon = null;
            txtBuscarProducto.UseSystemPasswordChar = false;
            txtBuscarProducto.UseTallSize = false;
            //
            // gridProductos
            //
            gridProductos.AllowUserToAddRows = false;
            gridProductos.AllowUserToDeleteRows = false;
            gridProductos.AllowUserToResizeRows = false;
            gridProductos.AutoGenerateColumns = false;
            gridProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridProductos.ColumnHeadersHeight = 30;
            gridProductos.Columns.AddRange(new DataGridViewColumn[] { colProductoCodigo, colProductoNombre });
            gridProductos.Dock = DockStyle.Fill;
            gridProductos.Margin = new Padding(0, 4, 0, 0);
            gridProductos.MinimumSize = new Size(0, 72);
            gridProductos.MultiSelect = false;
            gridProductos.Name = "gridProductos";
            gridProductos.ReadOnly = true;
            gridProductos.RowHeadersVisible = false;
            gridProductos.RowTemplate.Height = 26;
            gridProductos.ScrollBars = ScrollBars.Vertical;
            gridProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridProductos.Size = new Size(309, 267);
            gridProductos.TabIndex = 2;
            //
            // colProductoCodigo
            //
            colProductoCodigo.DataPropertyName = "Codigo";
            colProductoCodigo.FillWeight = 32F;
            colProductoCodigo.HeaderText = "Código";
            colProductoCodigo.Name = "colProductoCodigo";
            colProductoCodigo.ReadOnly = true;
            colProductoCodigo.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colProductoNombre
            //
            colProductoNombre.DataPropertyName = "Nombre";
            colProductoNombre.FillWeight = 68F;
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
            lblMensajeProductos.FontType = MaterialSkin.MaterialSkinManager.fontType.Caption;
            lblMensajeProductos.Margin = new Padding(0);
            lblMensajeProductos.MouseState = MaterialSkin.MouseState.HOVER;
            lblMensajeProductos.Name = "lblMensajeProductos";
            lblMensajeProductos.Size = new Size(309, 20);
            lblMensajeProductos.TabIndex = 3;
            lblMensajeProductos.TextAlign = ContentAlignment.MiddleLeft;
            //
            // tblDocumento
            //
            // Columna «Compra actual»: proveedor, producto a cargar, líneas (absorbe
            // el alto) y pie del documento. Los alto de fila incluyen el espacio de 6.
            tblDocumento.ColumnCount = 1;
            tblDocumento.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblDocumento.Controls.Add(cardDatosProveedor, 0, 0);
            tblDocumento.Controls.Add(cardDatosProducto, 0, 1);
            tblDocumento.Controls.Add(tblCompraActual, 0, 2);
            tblDocumento.Controls.Add(tblPieCompra, 0, 3);
            tblDocumento.Dock = DockStyle.Fill;
            tblDocumento.Location = new Point(339, 40);
            tblDocumento.Margin = new Padding(6, 0, 0, 0);
            tblDocumento.Name = "tblDocumento";
            tblDocumento.RowCount = 4;
            tblDocumento.RowStyles.Add(new RowStyle(SizeType.Absolute, 66F));
            tblDocumento.RowStyles.Add(new RowStyle(SizeType.Absolute, 126F));
            tblDocumento.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblDocumento.RowStyles.Add(new RowStyle(SizeType.Absolute, 82F));
            tblDocumento.Size = new Size(720, 636);
            tblDocumento.TabIndex = 2;
            //
            // cardDatosProveedor
            //
            cardDatosProveedor.Controls.Add(tblDatosProveedor);
            cardDatosProveedor.Depth = 0;
            cardDatosProveedor.Dock = DockStyle.Fill;
            cardDatosProveedor.Location = new Point(0, 0);
            cardDatosProveedor.Margin = new Padding(0, 0, 0, 6);
            cardDatosProveedor.MouseState = MaterialSkin.MouseState.HOVER;
            cardDatosProveedor.Name = "cardDatosProveedor";
            cardDatosProveedor.Padding = new Padding(12, 6, 12, 6);
            cardDatosProveedor.Size = new Size(720, 60);
            cardDatosProveedor.TabIndex = 0;
            //
            // tblDatosProveedor
            //
            // Rótulo pequeño arriba; razón social (crece) y documento (ancho propio,
            // alineado a la derecha) en la misma línea.
            tblDatosProveedor.ColumnCount = 2;
            tblDatosProveedor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblDatosProveedor.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblDatosProveedor.Controls.Add(lblTituloDatosProveedor, 0, 0);
            tblDatosProveedor.Controls.Add(lblRazonSocialProveedor, 0, 1);
            tblDatosProveedor.Controls.Add(lblDocumentoProveedor, 1, 1);
            tblDatosProveedor.SetColumnSpan(lblTituloDatosProveedor, 2);
            tblDatosProveedor.Dock = DockStyle.Fill;
            tblDatosProveedor.Location = new Point(12, 6);
            tblDatosProveedor.Margin = new Padding(0);
            tblDatosProveedor.Name = "tblDatosProveedor";
            tblDatosProveedor.RowCount = 2;
            tblDatosProveedor.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
            tblDatosProveedor.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tblDatosProveedor.Size = new Size(696, 48);
            tblDatosProveedor.TabIndex = 0;
            //
            // lblTituloDatosProveedor
            //
            lblTituloDatosProveedor.AutoSize = false;
            lblTituloDatosProveedor.Depth = 0;
            lblTituloDatosProveedor.Dock = DockStyle.Fill;
            lblTituloDatosProveedor.FontType = MaterialSkin.MaterialSkinManager.fontType.Caption;
            lblTituloDatosProveedor.Location = new Point(0, 0);
            lblTituloDatosProveedor.Margin = new Padding(0);
            lblTituloDatosProveedor.MouseState = MaterialSkin.MouseState.HOVER;
            lblTituloDatosProveedor.Name = "lblTituloDatosProveedor";
            lblTituloDatosProveedor.Size = new Size(696, 18);
            lblTituloDatosProveedor.TabIndex = 0;
            lblTituloDatosProveedor.Text = "Datos del proveedor";
            lblTituloDatosProveedor.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblRazonSocialProveedor
            //
            lblRazonSocialProveedor.AutoSize = false;
            lblRazonSocialProveedor.Depth = 0;
            lblRazonSocialProveedor.Dock = DockStyle.Fill;
            lblRazonSocialProveedor.FontType = MaterialSkin.MaterialSkinManager.fontType.H6;
            lblRazonSocialProveedor.Location = new Point(0, 18);
            lblRazonSocialProveedor.Margin = new Padding(0);
            lblRazonSocialProveedor.MouseState = MaterialSkin.MouseState.HOVER;
            lblRazonSocialProveedor.Name = "lblRazonSocialProveedor";
            lblRazonSocialProveedor.Size = new Size(500, 30);
            lblRazonSocialProveedor.TabIndex = 1;
            lblRazonSocialProveedor.Text = "Sin proveedor seleccionado";
            lblRazonSocialProveedor.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblDocumentoProveedor
            //
            lblDocumentoProveedor.Anchor = AnchorStyles.Right;
            lblDocumentoProveedor.AutoSize = true;
            lblDocumentoProveedor.Depth = 0;
            lblDocumentoProveedor.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            lblDocumentoProveedor.Location = new Point(600, 24);
            lblDocumentoProveedor.Margin = new Padding(12, 0, 0, 0);
            lblDocumentoProveedor.MouseState = MaterialSkin.MouseState.HOVER;
            lblDocumentoProveedor.Name = "lblDocumentoProveedor";
            lblDocumentoProveedor.Size = new Size(96, 19);
            lblDocumentoProveedor.TabIndex = 2;
            lblDocumentoProveedor.Text = "Documento: —";
            lblDocumentoProveedor.TextAlign = ContentAlignment.MiddleRight;
            //
            // cardDatosProducto
            //
            cardDatosProducto.Controls.Add(tblDatosProducto);
            cardDatosProducto.Depth = 0;
            cardDatosProducto.Dock = DockStyle.Fill;
            cardDatosProducto.Location = new Point(0, 66);
            cardDatosProducto.Margin = new Padding(0, 0, 0, 6);
            cardDatosProducto.MouseState = MaterialSkin.MouseState.HOVER;
            cardDatosProducto.Name = "cardDatosProducto";
            cardDatosProducto.Padding = new Padding(12, 4, 12, 4);
            cardDatosProducto.Size = new Size(720, 120);
            cardDatosProducto.TabIndex = 1;
            //
            // tblDatosProducto
            //
            // Fila 0 rótulo; fila 1 nombre del producto; fila 2 código (izquierda) y
            // subtotal (derecha); fila 3 en una sola línea: cantidad, precio de
            // compra, precio de venta y «Agregar» (ancho propio).
            tblDatosProducto.ColumnCount = 4;
            tblDatosProducto.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tblDatosProducto.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27F));
            tblDatosProducto.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27F));
            tblDatosProducto.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblDatosProducto.Controls.Add(lblTituloDatosProducto, 0, 0);
            tblDatosProducto.Controls.Add(lblNombreProducto, 0, 1);
            tblDatosProducto.Controls.Add(lblCodigoProducto, 0, 2);
            tblDatosProducto.Controls.Add(lblSubtotalPreparacion, 2, 2);
            tblDatosProducto.Controls.Add(lblRotuloCantidad, 0, 3);
            tblDatosProducto.Controls.Add(lblRotuloPrecioCompra, 1, 3);
            tblDatosProducto.Controls.Add(lblRotuloPrecioVenta, 2, 3);
            tblDatosProducto.Controls.Add(txtCantidad, 0, 4);
            tblDatosProducto.Controls.Add(txtPrecioCompra, 1, 4);
            tblDatosProducto.Controls.Add(txtPrecioVenta, 2, 4);
            tblDatosProducto.Controls.Add(btnAgregarLinea, 3, 4);
            tblDatosProducto.SetColumnSpan(lblTituloDatosProducto, 4);
            tblDatosProducto.SetColumnSpan(lblNombreProducto, 4);
            tblDatosProducto.SetColumnSpan(lblCodigoProducto, 2);
            tblDatosProducto.SetColumnSpan(lblSubtotalPreparacion, 2);
            tblDatosProducto.Dock = DockStyle.Fill;
            tblDatosProducto.Location = new Point(12, 6);
            tblDatosProducto.Margin = new Padding(0);
            tblDatosProducto.Name = "tblDatosProducto";
            tblDatosProducto.RowCount = 5;
            tblDatosProducto.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F));
            tblDatosProducto.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            tblDatosProducto.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tblDatosProducto.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F));
            tblDatosProducto.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tblDatosProducto.Size = new Size(696, 112);
            tblDatosProducto.TabIndex = 0;
            //
            // lblTituloDatosProducto
            //
            lblTituloDatosProducto.AutoSize = false;
            lblTituloDatosProducto.Depth = 0;
            lblTituloDatosProducto.Dock = DockStyle.Fill;
            lblTituloDatosProducto.FontType = MaterialSkin.MaterialSkinManager.fontType.Caption;
            lblTituloDatosProducto.Location = new Point(0, 0);
            lblTituloDatosProducto.Margin = new Padding(0);
            lblTituloDatosProducto.MouseState = MaterialSkin.MouseState.HOVER;
            lblTituloDatosProducto.Name = "lblTituloDatosProducto";
            lblTituloDatosProducto.Size = new Size(696, 18);
            lblTituloDatosProducto.TabIndex = 0;
            lblTituloDatosProducto.Text = "Producto a cargar";
            lblTituloDatosProducto.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblNombreProducto
            //
            lblNombreProducto.AutoSize = false;
            lblNombreProducto.Depth = 0;
            lblNombreProducto.Dock = DockStyle.Fill;
            lblNombreProducto.FontType = MaterialSkin.MaterialSkinManager.fontType.Subtitle1;
            lblNombreProducto.Location = new Point(0, 18);
            lblNombreProducto.Margin = new Padding(0);
            lblNombreProducto.MouseState = MaterialSkin.MouseState.HOVER;
            lblNombreProducto.Name = "lblNombreProducto";
            lblNombreProducto.Size = new Size(696, 24);
            lblNombreProducto.TabIndex = 1;
            lblNombreProducto.Text = "(Sin producto seleccionado)";
            lblNombreProducto.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblCodigoProducto
            //
            lblCodigoProducto.AutoSize = false;
            lblCodigoProducto.Depth = 0;
            lblCodigoProducto.Dock = DockStyle.Fill;
            lblCodigoProducto.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            lblCodigoProducto.Location = new Point(0, 42);
            lblCodigoProducto.Margin = new Padding(0);
            lblCodigoProducto.MouseState = MaterialSkin.MouseState.HOVER;
            lblCodigoProducto.Name = "lblCodigoProducto";
            lblCodigoProducto.Size = new Size(327, 22);
            lblCodigoProducto.TabIndex = 2;
            lblCodigoProducto.Text = "Código: —";
            lblCodigoProducto.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblSubtotalPreparacion
            //
            lblSubtotalPreparacion.AutoSize = false;
            lblSubtotalPreparacion.Depth = 0;
            lblSubtotalPreparacion.Dock = DockStyle.Fill;
            lblSubtotalPreparacion.FontType = MaterialSkin.MaterialSkinManager.fontType.Subtitle1;
            lblSubtotalPreparacion.Location = new Point(327, 42);
            lblSubtotalPreparacion.Margin = new Padding(0);
            lblSubtotalPreparacion.MouseState = MaterialSkin.MouseState.HOVER;
            lblSubtotalPreparacion.Name = "lblSubtotalPreparacion";
            lblSubtotalPreparacion.Size = new Size(369, 22);
            lblSubtotalPreparacion.TabIndex = 3;
            lblSubtotalPreparacion.Text = "Subtotal: —";
            lblSubtotalPreparacion.TextAlign = ContentAlignment.MiddleRight;
            //
            // lblRotuloCantidad
            //
            // Rótulos fijos sobre los campos: con valor, el hint de MaterialTextBox2 se
            // oculta, así que el nombre del campo vive en una etiqueta propia.
            lblRotuloCantidad.AutoSize = false;
            lblRotuloCantidad.Depth = 0;
            lblRotuloCantidad.Dock = DockStyle.Fill;
            lblRotuloCantidad.FontType = MaterialSkin.MaterialSkinManager.fontType.Caption;
            lblRotuloCantidad.Location = new Point(0, 62);
            lblRotuloCantidad.Margin = new Padding(2, 0, 8, 0);
            lblRotuloCantidad.MouseState = MaterialSkin.MouseState.HOVER;
            lblRotuloCantidad.Name = "lblRotuloCantidad";
            lblRotuloCantidad.Size = new Size(100, 16);
            lblRotuloCantidad.TabIndex = 8;
            lblRotuloCantidad.Text = "Cantidad";
            lblRotuloCantidad.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblRotuloPrecioCompra
            //
            lblRotuloPrecioCompra.AutoSize = false;
            lblRotuloPrecioCompra.Depth = 0;
            lblRotuloPrecioCompra.Dock = DockStyle.Fill;
            lblRotuloPrecioCompra.FontType = MaterialSkin.MaterialSkinManager.fontType.Caption;
            lblRotuloPrecioCompra.Location = new Point(108, 62);
            lblRotuloPrecioCompra.Margin = new Padding(2, 0, 8, 0);
            lblRotuloPrecioCompra.MouseState = MaterialSkin.MouseState.HOVER;
            lblRotuloPrecioCompra.Name = "lblRotuloPrecioCompra";
            lblRotuloPrecioCompra.Size = new Size(130, 16);
            lblRotuloPrecioCompra.TabIndex = 9;
            lblRotuloPrecioCompra.Text = "Precio compra";
            lblRotuloPrecioCompra.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblRotuloPrecioVenta
            //
            lblRotuloPrecioVenta.AutoSize = false;
            lblRotuloPrecioVenta.Depth = 0;
            lblRotuloPrecioVenta.Dock = DockStyle.Fill;
            lblRotuloPrecioVenta.FontType = MaterialSkin.MaterialSkinManager.fontType.Caption;
            lblRotuloPrecioVenta.Location = new Point(246, 62);
            lblRotuloPrecioVenta.Margin = new Padding(2, 0, 8, 0);
            lblRotuloPrecioVenta.MouseState = MaterialSkin.MouseState.HOVER;
            lblRotuloPrecioVenta.Name = "lblRotuloPrecioVenta";
            lblRotuloPrecioVenta.Size = new Size(130, 16);
            lblRotuloPrecioVenta.TabIndex = 10;
            lblRotuloPrecioVenta.Text = "Precio venta";
            lblRotuloPrecioVenta.TextAlign = ContentAlignment.MiddleLeft;
            //
            // txtCantidad
            //
            txtCantidad.AnimateReadOnly = false;
            txtCantidad.BackgroundImageLayout = ImageLayout.None;
            txtCantidad.CharacterCasing = CharacterCasing.Normal;
            txtCantidad.Depth = 0;
            txtCantidad.Dock = DockStyle.Fill;
            txtCantidad.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point);
            txtCantidad.Hint = "";
            txtCantidad.Location = new Point(0, 64);
            txtCantidad.Margin = new Padding(0, 0, 8, 0);
            txtCantidad.MaxLength = 32767;
            txtCantidad.MinimumSize = new Size(64, 36);
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
            txtCantidad.Size = new Size(100, 40);
            txtCantidad.TabIndex = 4;
            txtCantidad.TextAlign = HorizontalAlignment.Left;
            txtCantidad.TrailingIcon = null;
            txtCantidad.UseSystemPasswordChar = false;
            txtCantidad.UseTallSize = false;
            //
            // txtPrecioCompra
            //
            txtPrecioCompra.AnimateReadOnly = false;
            txtPrecioCompra.BackgroundImageLayout = ImageLayout.None;
            txtPrecioCompra.CharacterCasing = CharacterCasing.Normal;
            txtPrecioCompra.Depth = 0;
            txtPrecioCompra.Dock = DockStyle.Fill;
            txtPrecioCompra.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point);
            txtPrecioCompra.Hint = "";
            txtPrecioCompra.Location = new Point(108, 64);
            txtPrecioCompra.Margin = new Padding(0, 0, 8, 0);
            txtPrecioCompra.MaxLength = 32767;
            txtPrecioCompra.MinimumSize = new Size(64, 36);
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
            txtPrecioCompra.Size = new Size(130, 40);
            txtPrecioCompra.TabIndex = 5;
            txtPrecioCompra.TextAlign = HorizontalAlignment.Left;
            txtPrecioCompra.TrailingIcon = null;
            txtPrecioCompra.UseSystemPasswordChar = false;
            txtPrecioCompra.UseTallSize = false;
            //
            // txtPrecioVenta
            //
            txtPrecioVenta.AnimateReadOnly = false;
            txtPrecioVenta.BackgroundImageLayout = ImageLayout.None;
            txtPrecioVenta.CharacterCasing = CharacterCasing.Normal;
            txtPrecioVenta.Depth = 0;
            txtPrecioVenta.Dock = DockStyle.Fill;
            txtPrecioVenta.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point);
            txtPrecioVenta.Hint = "";
            txtPrecioVenta.Location = new Point(246, 64);
            txtPrecioVenta.Margin = new Padding(0, 0, 8, 0);
            txtPrecioVenta.MaxLength = 32767;
            txtPrecioVenta.MinimumSize = new Size(64, 36);
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
            txtPrecioVenta.Size = new Size(130, 40);
            txtPrecioVenta.TabIndex = 6;
            txtPrecioVenta.TextAlign = HorizontalAlignment.Left;
            txtPrecioVenta.TrailingIcon = null;
            txtPrecioVenta.UseSystemPasswordChar = false;
            txtPrecioVenta.UseTallSize = false;
            //
            // btnAgregarLinea
            //
            btnAgregarLinea.AutoSize = true;
            btnAgregarLinea.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnAgregarLinea.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnAgregarLinea.Depth = 0;
            btnAgregarLinea.Dock = DockStyle.Fill;
            btnAgregarLinea.HighEmphasis = true;
            btnAgregarLinea.Icon = null;
            btnAgregarLinea.Location = new Point(504, 64);
            btnAgregarLinea.Margin = new Padding(0);
            btnAgregarLinea.MinimumSize = new Size(0, 36);
            btnAgregarLinea.MouseState = MaterialSkin.MouseState.HOVER;
            btnAgregarLinea.Name = "btnAgregarLinea";
            btnAgregarLinea.NoAccentTextColor = Color.Empty;
            btnAgregarLinea.Padding = new Padding(10, 0, 10, 0);
            btnAgregarLinea.Size = new Size(192, 40);
            btnAgregarLinea.TabIndex = 7;
            btnAgregarLinea.Text = "Agregar a la compra";
            btnAgregarLinea.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnAgregarLinea.UseAccentColor = true;
            btnAgregarLinea.UseVisualStyleBackColor = true;
            //
            // tblCompraActual
            //
            // Título de sección y grilla de líneas, que absorbe todo el alto sobrante.
            tblCompraActual.ColumnCount = 1;
            tblCompraActual.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblCompraActual.Controls.Add(lblTituloCompraActual, 0, 0);
            tblCompraActual.Controls.Add(gridLineas, 0, 1);
            tblCompraActual.Dock = DockStyle.Fill;
            tblCompraActual.Location = new Point(0, 192);
            tblCompraActual.Margin = new Padding(0, 0, 0, 6);
            tblCompraActual.Name = "tblCompraActual";
            tblCompraActual.RowCount = 2;
            tblCompraActual.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            tblCompraActual.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblCompraActual.Size = new Size(720, 356);
            tblCompraActual.TabIndex = 2;
            //
            // lblTituloCompraActual
            //
            lblTituloCompraActual.AutoSize = false;
            lblTituloCompraActual.Depth = 0;
            lblTituloCompraActual.Dock = DockStyle.Fill;
            lblTituloCompraActual.FontType = MaterialSkin.MaterialSkinManager.fontType.Subtitle1;
            lblTituloCompraActual.Location = new Point(0, 0);
            lblTituloCompraActual.Margin = new Padding(0);
            lblTituloCompraActual.MouseState = MaterialSkin.MouseState.HOVER;
            lblTituloCompraActual.Name = "lblTituloCompraActual";
            lblTituloCompraActual.Size = new Size(720, 26);
            lblTituloCompraActual.TabIndex = 0;
            lblTituloCompraActual.Text = "Compra actual";
            lblTituloCompraActual.TextAlign = ContentAlignment.MiddleLeft;
            //
            // gridLineas
            //
            gridLineas.AllowUserToAddRows = false;
            gridLineas.AllowUserToDeleteRows = false;
            gridLineas.AllowUserToResizeRows = false;
            gridLineas.AutoGenerateColumns = false;
            gridLineas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridLineas.ColumnHeadersHeight = 32;
            gridLineas.Columns.AddRange(new DataGridViewColumn[] { colLineaProducto, colLineaCantidad, colLineaPrecioCompra, colLineaPrecioVenta, colLineaSubtotal, colLineaEditar, colLineaQuitar });
            gridLineas.Dock = DockStyle.Fill;
            gridLineas.Location = new Point(0, 26);
            gridLineas.Margin = new Padding(0);
            gridLineas.MinimumSize = new Size(0, 84);
            gridLineas.MultiSelect = false;
            gridLineas.Name = "gridLineas";
            gridLineas.ReadOnly = true;
            gridLineas.RowHeadersVisible = false;
            gridLineas.RowTemplate.Height = 32;
            gridLineas.ScrollBars = ScrollBars.Vertical;
            gridLineas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridLineas.Size = new Size(720, 330);
            gridLineas.TabIndex = 1;
            //
            // colLineaProducto
            //
            colLineaProducto.DataPropertyName = "NombreProducto";
            colLineaProducto.FillWeight = 52F;
            colLineaProducto.HeaderText = "Producto";
            colLineaProducto.Name = "colLineaProducto";
            colLineaProducto.ReadOnly = true;
            colLineaProducto.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colLineaCantidad
            //
            colLineaCantidad.DataPropertyName = "Cantidad";
            colLineaCantidad.DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight };
            colLineaCantidad.FillWeight = 8F;
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
            colLineaPrecioVenta.FillWeight = 13F;
            colLineaPrecioVenta.HeaderText = "Precio venta";
            colLineaPrecioVenta.Name = "colLineaPrecioVenta";
            colLineaPrecioVenta.ReadOnly = true;
            colLineaPrecioVenta.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colLineaSubtotal
            //
            colLineaSubtotal.DataPropertyName = "Subtotal";
            colLineaSubtotal.DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight };
            colLineaSubtotal.FillWeight = 13F;
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
            colLineaEditar.MinimumWidth = 56;
            colLineaEditar.Name = "colLineaEditar";
            colLineaEditar.ReadOnly = true;
            colLineaEditar.Resizable = DataGridViewTriState.False;
            colLineaEditar.SortMode = DataGridViewColumnSortMode.NotSortable;
            colLineaEditar.Text = "Editar";
            colLineaEditar.UseColumnTextForButtonValue = true;
            colLineaEditar.Width = 56;
            //
            // colLineaQuitar
            //
            colLineaQuitar.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colLineaQuitar.FlatStyle = FlatStyle.Flat;
            colLineaQuitar.HeaderText = "";
            colLineaQuitar.MinimumWidth = 56;
            colLineaQuitar.Name = "colLineaQuitar";
            colLineaQuitar.ReadOnly = true;
            colLineaQuitar.Resizable = DataGridViewTriState.False;
            colLineaQuitar.SortMode = DataGridViewColumnSortMode.NotSortable;
            colLineaQuitar.Text = "Quitar";
            colLineaQuitar.UseColumnTextForButtonValue = true;
            colLineaQuitar.Width = 56;
            //
            // tblPieCompra
            //
            // Pie del documento: fila 0 con el estado en línea (izquierda) y el total
            // destacado (derecha); fila 1 con las acciones.
            tblPieCompra.ColumnCount = 2;
            tblPieCompra.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblPieCompra.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblPieCompra.Controls.Add(lblMensaje, 0, 0);
            tblPieCompra.Controls.Add(lblMontoTotal, 1, 0);
            tblPieCompra.Controls.Add(flpAcciones, 0, 1);
            tblPieCompra.SetColumnSpan(flpAcciones, 2);
            tblPieCompra.Dock = DockStyle.Fill;
            tblPieCompra.Location = new Point(0, 554);
            tblPieCompra.Margin = new Padding(0);
            tblPieCompra.Name = "tblPieCompra";
            tblPieCompra.RowCount = 2;
            tblPieCompra.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tblPieCompra.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            tblPieCompra.Size = new Size(720, 82);
            tblPieCompra.TabIndex = 3;
            //
            // lblMensaje
            //
            lblMensaje.AutoSize = false;
            lblMensaje.Depth = 0;
            lblMensaje.Dock = DockStyle.Fill;
            lblMensaje.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            lblMensaje.Location = new Point(0, 0);
            lblMensaje.Margin = new Padding(0, 0, 12, 0);
            lblMensaje.MouseState = MaterialSkin.MouseState.HOVER;
            lblMensaje.Name = "lblMensaje";
            lblMensaje.Size = new Size(520, 36);
            lblMensaje.TabIndex = 0;
            lblMensaje.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblMontoTotal
            //
            lblMontoTotal.Anchor = AnchorStyles.Right;
            lblMontoTotal.AutoSize = true;
            lblMontoTotal.Depth = 0;
            lblMontoTotal.FontType = MaterialSkin.MaterialSkinManager.fontType.H5;
            lblMontoTotal.Location = new Point(560, 2);
            lblMontoTotal.Margin = new Padding(0);
            lblMontoTotal.MouseState = MaterialSkin.MouseState.HOVER;
            lblMontoTotal.Name = "lblMontoTotal";
            lblMontoTotal.Size = new Size(160, 32);
            lblMontoTotal.TabIndex = 1;
            lblMontoTotal.Text = "Total: 0,00";
            lblMontoTotal.TextAlign = ContentAlignment.MiddleRight;
            //
            // flpAcciones
            //
            // Historial (terciario) a la izquierda; Cancelar y Registrar a la derecha,
            // con Registrar como acción de mayor énfasis. Columnas que se ajustan al
            // contenido: ningún botón se recorta.
            flpAcciones.ColumnCount = 4;
            flpAcciones.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            flpAcciones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            flpAcciones.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            flpAcciones.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            flpAcciones.Controls.Add(btnVerHistorial, 0, 0);
            flpAcciones.Controls.Add(btnLimpiar, 2, 0);
            flpAcciones.Controls.Add(btnRegistrar, 3, 0);
            flpAcciones.Dock = DockStyle.Fill;
            flpAcciones.Location = new Point(0, 36);
            flpAcciones.Margin = new Padding(0);
            flpAcciones.Name = "flpAcciones";
            flpAcciones.RowCount = 1;
            flpAcciones.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            flpAcciones.Size = new Size(720, 46);
            flpAcciones.TabIndex = 2;
            //
            // btnVerHistorial
            //
            btnVerHistorial.Dock = DockStyle.Fill;
            btnVerHistorial.AutoSize = true;
            btnVerHistorial.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnVerHistorial.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnVerHistorial.Depth = 0;
            btnVerHistorial.HighEmphasis = false;
            btnVerHistorial.Icon = null;
            btnVerHistorial.Margin = new Padding(0, 3, 6, 3);
            btnVerHistorial.MinimumSize = new Size(0, 36);
            btnVerHistorial.MouseState = MaterialSkin.MouseState.HOVER;
            btnVerHistorial.Name = "btnVerHistorial";
            btnVerHistorial.NoAccentTextColor = Color.Empty;
            btnVerHistorial.Padding = new Padding(10, 0, 10, 0);
            btnVerHistorial.Size = new Size(130, 36);
            btnVerHistorial.TabIndex = 0;
            btnVerHistorial.Text = "Ver Historial";
            btnVerHistorial.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Text;
            btnVerHistorial.UseAccentColor = false;
            btnVerHistorial.UseVisualStyleBackColor = true;
            //
            // btnLimpiar
            //
            btnLimpiar.Dock = DockStyle.Fill;
            btnLimpiar.AutoSize = true;
            btnLimpiar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnLimpiar.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnLimpiar.Depth = 0;
            btnLimpiar.HighEmphasis = false;
            btnLimpiar.Icon = null;
            btnLimpiar.Margin = new Padding(6, 3, 8, 3);
            btnLimpiar.MinimumSize = new Size(0, 36);
            btnLimpiar.MouseState = MaterialSkin.MouseState.HOVER;
            btnLimpiar.Name = "btnLimpiar";
            btnLimpiar.NoAccentTextColor = Color.White;
            btnLimpiar.Padding = new Padding(10, 0, 10, 0);
            btnLimpiar.Size = new Size(170, 36);
            btnLimpiar.TabIndex = 1;
            btnLimpiar.Text = "CANCELAR COMPRA";
            btnLimpiar.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            btnLimpiar.UseAccentColor = false;
            btnLimpiar.UseVisualStyleBackColor = true;
            //
            // btnRegistrar
            //
            btnRegistrar.Dock = DockStyle.Fill;
            btnRegistrar.AutoSize = true;
            btnRegistrar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnRegistrar.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnRegistrar.Depth = 0;
            btnRegistrar.HighEmphasis = true;
            btnRegistrar.Icon = null;
            btnRegistrar.Margin = new Padding(0, 3, 0, 3);
            btnRegistrar.MinimumSize = new Size(0, 40);
            btnRegistrar.MouseState = MaterialSkin.MouseState.HOVER;
            btnRegistrar.Name = "btnRegistrar";
            btnRegistrar.NoAccentTextColor = Color.Empty;
            btnRegistrar.Padding = new Padding(14, 0, 14, 0);
            btnRegistrar.Size = new Size(176, 40);
            btnRegistrar.TabIndex = 2;
            btnRegistrar.Text = "REGISTRAR COMPRA";
            btnRegistrar.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnRegistrar.UseAccentColor = true;
            btnRegistrar.UseVisualStyleBackColor = true;
            //
            // ComprasForm
            //
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(33, 33, 33);
            Controls.Add(pnlContenedor);
            Name = "ComprasForm";
            Size = new Size(1083, 700);
            SizeChanged += ComprasForm_SizeChanged;
            pnlContenedor.ResumeLayout(false);
            tblPrincipal.ResumeLayout(false);
            tblCatalogo.ResumeLayout(false);
            cardProveedor.ResumeLayout(false);
            tblTituloProveedores.ResumeLayout(false);
            tblTituloProveedores.PerformLayout();
            tblListadoProveedores.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridProveedores).EndInit();
            cardProducto.ResumeLayout(false);
            tblTituloProductos.ResumeLayout(false);
            tblTituloProductos.PerformLayout();
            tblListadoProductos.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridProductos).EndInit();
            tblDocumento.ResumeLayout(false);
            cardDatosProveedor.ResumeLayout(false);
            tblDatosProveedor.ResumeLayout(false);
            tblDatosProveedor.PerformLayout();
            cardDatosProducto.ResumeLayout(false);
            tblDatosProducto.ResumeLayout(false);
            tblDatosProducto.PerformLayout();
            tblCompraActual.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridLineas).EndInit();
            tblPieCompra.ResumeLayout(false);
            tblPieCompra.PerformLayout();
            flpAcciones.ResumeLayout(false);
            flpAcciones.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlContenedor;
        private TableLayoutPanel tblPrincipal;
        private MaterialSkin.Controls.MaterialLabel lblTitulo;
        private TableLayoutPanel tblCatalogo;
        private MaterialSkin.Controls.MaterialCard cardProveedor;
        private TableLayoutPanel tblListadoProveedores;
        private TableLayoutPanel tblTituloProveedores;
        private MaterialSkin.Controls.MaterialCheckbox chkMostrarTodosProveedores;
        private MaterialSkin.Controls.MaterialLabel lblTituloListadoProveedores;
        private MaterialSkin.Controls.MaterialTextBox2 txtBuscarProveedor;
        private DataGridView gridProveedores;
        private DataGridViewTextBoxColumn colProveedorRazonSocial;
        private DataGridViewTextBoxColumn colProveedorDocumento;
        private MaterialSkin.Controls.MaterialLabel lblMensajeProveedores;
        private MaterialSkin.Controls.MaterialCard cardProducto;
        private TableLayoutPanel tblListadoProductos;
        private TableLayoutPanel tblTituloProductos;
        private MaterialSkin.Controls.MaterialCheckbox chkMostrarTodosProductos;
        private MaterialSkin.Controls.MaterialLabel lblTituloListadoProductos;
        private MaterialSkin.Controls.MaterialTextBox2 txtBuscarProducto;
        private DataGridView gridProductos;
        private DataGridViewTextBoxColumn colProductoCodigo;
        private DataGridViewTextBoxColumn colProductoNombre;
        private MaterialSkin.Controls.MaterialLabel lblMensajeProductos;
        private TableLayoutPanel tblDocumento;
        private MaterialSkin.Controls.MaterialCard cardDatosProveedor;
        private TableLayoutPanel tblDatosProveedor;
        private MaterialSkin.Controls.MaterialLabel lblTituloDatosProveedor;
        private MaterialSkin.Controls.MaterialLabel lblRazonSocialProveedor;
        private MaterialSkin.Controls.MaterialLabel lblDocumentoProveedor;
        private MaterialSkin.Controls.MaterialCard cardDatosProducto;
        private TableLayoutPanel tblDatosProducto;
        private MaterialSkin.Controls.MaterialLabel lblTituloDatosProducto;
        private MaterialSkin.Controls.MaterialLabel lblNombreProducto;
        private MaterialSkin.Controls.MaterialLabel lblCodigoProducto;
        private MaterialSkin.Controls.MaterialLabel lblSubtotalPreparacion;
        private MaterialSkin.Controls.MaterialLabel lblRotuloCantidad;
        private MaterialSkin.Controls.MaterialLabel lblRotuloPrecioCompra;
        private MaterialSkin.Controls.MaterialLabel lblRotuloPrecioVenta;
        private MaterialSkin.Controls.MaterialTextBox2 txtCantidad;
        private MaterialSkin.Controls.MaterialTextBox2 txtPrecioCompra;
        private MaterialSkin.Controls.MaterialTextBox2 txtPrecioVenta;
        private MaterialSkin.Controls.MaterialButton btnAgregarLinea;
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
        private MaterialSkin.Controls.MaterialLabel lblMensaje;
        private MaterialSkin.Controls.MaterialLabel lblMontoTotal;
        private TableLayoutPanel flpAcciones;
        private MaterialSkin.Controls.MaterialButton btnVerHistorial;
        private MaterialSkin.Controls.MaterialButton btnLimpiar;
        private MaterialSkin.Controls.MaterialButton btnRegistrar;
    }
}
