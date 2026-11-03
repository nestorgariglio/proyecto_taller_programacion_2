namespace CapaPresentacion.Compras
{
    partial class DetalleCompraForm
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
            lblTituloDetalle = new MaterialSkin.Controls.MaterialLabel();
            gridListaCompras = new DataGridView();
            colListaNumero = new DataGridViewTextBoxColumn();
            colListaFecha = new DataGridViewTextBoxColumn();
            colListaProveedor = new DataGridViewTextBoxColumn();
            colListaUsuario = new DataGridViewTextBoxColumn();
            colListaTotal = new DataGridViewTextBoxColumn();
            colListaEstado = new DataGridViewTextBoxColumn();
            tblCabecera = new TableLayoutPanel();
            lblNumeroCompra = new MaterialSkin.Controls.MaterialLabel();
            lblFechaDetalle = new MaterialSkin.Controls.MaterialLabel();
            lblEstadoDetalle = new MaterialSkin.Controls.MaterialLabel();
            lblTotalDetalle = new MaterialSkin.Controls.MaterialLabel();
            lblProveedorDetalle = new MaterialSkin.Controls.MaterialLabel();
            lblUsuarioDetalle = new MaterialSkin.Controls.MaterialLabel();
            gridDetalle = new DataGridView();
            colDetalleProducto = new DataGridViewTextBoxColumn();
            colDetalleCantidad = new DataGridViewTextBoxColumn();
            colDetallePrecioCompra = new DataGridViewTextBoxColumn();
            colDetalleSubtotal = new DataGridViewTextBoxColumn();
            lblMensajeDetalle = new MaterialSkin.Controls.MaterialLabel();
            tblPrincipal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridListaCompras).BeginInit();
            tblCabecera.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridDetalle).BeginInit();
            SuspendLayout();
            //
            // tblPrincipal
            //
            tblPrincipal.ColumnCount = 1;
            tblPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblPrincipal.Controls.Add(lblTituloDetalle, 0, 0);
            tblPrincipal.Controls.Add(gridListaCompras, 0, 1);
            tblPrincipal.Controls.Add(tblCabecera, 0, 2);
            tblPrincipal.Controls.Add(gridDetalle, 0, 3);
            tblPrincipal.Controls.Add(lblMensajeDetalle, 0, 4);
            tblPrincipal.Dock = DockStyle.Fill;
            tblPrincipal.Location = new Point(0, 0);
            tblPrincipal.Margin = new Padding(0);
            tblPrincipal.Name = "tblPrincipal";
            tblPrincipal.Padding = new Padding(16, 12, 16, 12);
            tblPrincipal.RowCount = 5;
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 170F));
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblPrincipal.Size = new Size(900, 560);
            tblPrincipal.TabIndex = 0;
            //
            // lblTituloDetalle
            //
            lblTituloDetalle.AutoSize = false;
            lblTituloDetalle.Depth = 0;
            lblTituloDetalle.Dock = DockStyle.Fill;
            lblTituloDetalle.Font = new Font("Roboto", 18F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblTituloDetalle.Location = new Point(16, 12);
            lblTituloDetalle.Margin = new Padding(0);
            lblTituloDetalle.MouseState = MaterialSkin.MouseState.HOVER;
            lblTituloDetalle.Name = "lblTituloDetalle";
            lblTituloDetalle.Size = new Size(868, 34);
            lblTituloDetalle.TabIndex = 0;
            lblTituloDetalle.Text = "Detalle de la compra";
            lblTituloDetalle.TextAlign = ContentAlignment.MiddleLeft;
            //
            // gridListaCompras
            //
            gridListaCompras.AllowUserToAddRows = false;
            gridListaCompras.AllowUserToDeleteRows = false;
            gridListaCompras.AllowUserToResizeRows = false;
            gridListaCompras.AutoGenerateColumns = false;
            gridListaCompras.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridListaCompras.ColumnHeadersHeight = 32;
            gridListaCompras.Columns.AddRange(new DataGridViewColumn[] { colListaNumero, colListaFecha, colListaProveedor, colListaUsuario, colListaTotal, colListaEstado });
            gridListaCompras.Dock = DockStyle.Fill;
            gridListaCompras.Location = new Point(16, 46);
            gridListaCompras.Margin = new Padding(0);
            gridListaCompras.MultiSelect = false;
            gridListaCompras.Name = "gridListaCompras";
            gridListaCompras.ReadOnly = true;
            gridListaCompras.RowHeadersVisible = false;
            gridListaCompras.RowTemplate.Height = 26;
            gridListaCompras.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridListaCompras.Size = new Size(868, 170);
            gridListaCompras.TabIndex = 1;
            gridListaCompras.Visible = false;
            //
            // colListaNumero
            //
            colListaNumero.DataPropertyName = "IdCompra";
            colListaNumero.FillWeight = 10F;
            colListaNumero.HeaderText = "N° compra";
            colListaNumero.Name = "colListaNumero";
            colListaNumero.ReadOnly = true;
            colListaNumero.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colListaFecha
            //
            colListaFecha.DataPropertyName = "FechaRegistro";
            colListaFecha.FillWeight = 18F;
            colListaFecha.HeaderText = "Fecha y hora";
            colListaFecha.Name = "colListaFecha";
            colListaFecha.ReadOnly = true;
            colListaFecha.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colListaProveedor
            //
            colListaProveedor.DataPropertyName = "RazonSocialProveedor";
            colListaProveedor.FillWeight = 30F;
            colListaProveedor.HeaderText = "Proveedor";
            colListaProveedor.Name = "colListaProveedor";
            colListaProveedor.ReadOnly = true;
            colListaProveedor.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colListaUsuario
            //
            colListaUsuario.DataPropertyName = "NombreUsuario";
            colListaUsuario.FillWeight = 18F;
            colListaUsuario.HeaderText = "Usuario";
            colListaUsuario.Name = "colListaUsuario";
            colListaUsuario.ReadOnly = true;
            colListaUsuario.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colListaTotal
            //
            colListaTotal.DataPropertyName = "MontoTotal";
            colListaTotal.FillWeight = 14F;
            colListaTotal.HeaderText = "Total";
            colListaTotal.Name = "colListaTotal";
            colListaTotal.ReadOnly = true;
            colListaTotal.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colListaEstado
            //
            colListaEstado.DataPropertyName = "Estado";
            colListaEstado.FillWeight = 10F;
            colListaEstado.HeaderText = "Estado";
            colListaEstado.Name = "colListaEstado";
            colListaEstado.ReadOnly = true;
            colListaEstado.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // tblCabecera
            //
            tblCabecera.ColumnCount = 4;
            tblCabecera.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tblCabecera.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tblCabecera.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tblCabecera.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tblCabecera.Controls.Add(lblNumeroCompra, 0, 0);
            tblCabecera.Controls.Add(lblFechaDetalle, 1, 0);
            tblCabecera.Controls.Add(lblEstadoDetalle, 2, 0);
            tblCabecera.Controls.Add(lblTotalDetalle, 3, 0);
            tblCabecera.Controls.Add(lblProveedorDetalle, 0, 1);
            tblCabecera.Controls.Add(lblUsuarioDetalle, 2, 1);
            tblCabecera.Dock = DockStyle.Fill;
            tblCabecera.Location = new Point(16, 216);
            tblCabecera.Margin = new Padding(0);
            tblCabecera.Name = "tblCabecera";
            tblCabecera.RowCount = 2;
            tblCabecera.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tblCabecera.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tblCabecera.SetColumnSpan(lblProveedorDetalle, 2);
            tblCabecera.SetColumnSpan(lblUsuarioDetalle, 2);
            tblCabecera.Size = new Size(868, 68);
            tblCabecera.TabIndex = 1;
            //
            // lblNumeroCompra
            //
            lblNumeroCompra.AutoSize = false;
            lblNumeroCompra.Depth = 0;
            lblNumeroCompra.Dock = DockStyle.Fill;
            lblNumeroCompra.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblNumeroCompra.Location = new Point(0, 0);
            lblNumeroCompra.Margin = new Padding(0);
            lblNumeroCompra.MouseState = MaterialSkin.MouseState.HOVER;
            lblNumeroCompra.Name = "lblNumeroCompra";
            lblNumeroCompra.Size = new Size(217, 34);
            lblNumeroCompra.TabIndex = 0;
            lblNumeroCompra.Text = "N° compra: —";
            lblNumeroCompra.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblFechaDetalle
            //
            lblFechaDetalle.AutoSize = false;
            lblFechaDetalle.Depth = 0;
            lblFechaDetalle.Dock = DockStyle.Fill;
            lblFechaDetalle.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblFechaDetalle.Location = new Point(217, 0);
            lblFechaDetalle.Margin = new Padding(0);
            lblFechaDetalle.MouseState = MaterialSkin.MouseState.HOVER;
            lblFechaDetalle.Name = "lblFechaDetalle";
            lblFechaDetalle.Size = new Size(217, 34);
            lblFechaDetalle.TabIndex = 1;
            lblFechaDetalle.Text = "Fecha: —";
            lblFechaDetalle.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblEstadoDetalle
            //
            lblEstadoDetalle.AutoSize = false;
            lblEstadoDetalle.Depth = 0;
            lblEstadoDetalle.Dock = DockStyle.Fill;
            lblEstadoDetalle.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblEstadoDetalle.Location = new Point(434, 0);
            lblEstadoDetalle.Margin = new Padding(0);
            lblEstadoDetalle.MouseState = MaterialSkin.MouseState.HOVER;
            lblEstadoDetalle.Name = "lblEstadoDetalle";
            lblEstadoDetalle.Size = new Size(217, 34);
            lblEstadoDetalle.TabIndex = 2;
            lblEstadoDetalle.Text = "Estado: —";
            lblEstadoDetalle.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblTotalDetalle
            //
            lblTotalDetalle.AutoSize = false;
            lblTotalDetalle.Depth = 0;
            lblTotalDetalle.Dock = DockStyle.Fill;
            lblTotalDetalle.Font = new Font("Roboto", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblTotalDetalle.Location = new Point(651, 0);
            lblTotalDetalle.Margin = new Padding(0);
            lblTotalDetalle.MouseState = MaterialSkin.MouseState.HOVER;
            lblTotalDetalle.Name = "lblTotalDetalle";
            lblTotalDetalle.Size = new Size(217, 34);
            lblTotalDetalle.TabIndex = 3;
            lblTotalDetalle.Text = "Total: —";
            lblTotalDetalle.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblProveedorDetalle
            //
            lblProveedorDetalle.AutoSize = false;
            lblProveedorDetalle.Depth = 0;
            lblProveedorDetalle.Dock = DockStyle.Fill;
            lblProveedorDetalle.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblProveedorDetalle.Location = new Point(0, 34);
            lblProveedorDetalle.Margin = new Padding(0);
            lblProveedorDetalle.MouseState = MaterialSkin.MouseState.HOVER;
            lblProveedorDetalle.Name = "lblProveedorDetalle";
            lblProveedorDetalle.Size = new Size(434, 34);
            lblProveedorDetalle.TabIndex = 4;
            lblProveedorDetalle.Text = "Proveedor: —";
            lblProveedorDetalle.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblUsuarioDetalle
            //
            lblUsuarioDetalle.AutoSize = false;
            lblUsuarioDetalle.Depth = 0;
            lblUsuarioDetalle.Dock = DockStyle.Fill;
            lblUsuarioDetalle.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblUsuarioDetalle.Location = new Point(434, 34);
            lblUsuarioDetalle.Margin = new Padding(0);
            lblUsuarioDetalle.MouseState = MaterialSkin.MouseState.HOVER;
            lblUsuarioDetalle.Name = "lblUsuarioDetalle";
            lblUsuarioDetalle.Size = new Size(434, 34);
            lblUsuarioDetalle.TabIndex = 5;
            lblUsuarioDetalle.Text = "Usuario: —";
            lblUsuarioDetalle.TextAlign = ContentAlignment.MiddleLeft;
            //
            // gridDetalle
            //
            gridDetalle.AllowUserToAddRows = false;
            gridDetalle.AllowUserToDeleteRows = false;
            gridDetalle.AllowUserToResizeRows = false;
            gridDetalle.AutoGenerateColumns = false;
            gridDetalle.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridDetalle.ColumnHeadersHeight = 38;
            gridDetalle.Columns.AddRange(new DataGridViewColumn[] { colDetalleProducto, colDetalleCantidad, colDetallePrecioCompra, colDetalleSubtotal });
            gridDetalle.Dock = DockStyle.Fill;
            gridDetalle.Location = new Point(16, 284);
            gridDetalle.Margin = new Padding(0);
            gridDetalle.MultiSelect = false;
            gridDetalle.Name = "gridDetalle";
            gridDetalle.ReadOnly = true;
            gridDetalle.RowHeadersVisible = false;
            gridDetalle.RowTemplate.Height = 32;
            gridDetalle.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridDetalle.Size = new Size(868, 236);
            gridDetalle.TabIndex = 2;
            //
            // colDetalleProducto
            //
            colDetalleProducto.DataPropertyName = "NombreProducto";
            colDetalleProducto.FillWeight = 46F;
            colDetalleProducto.HeaderText = "Producto";
            colDetalleProducto.Name = "colDetalleProducto";
            colDetalleProducto.ReadOnly = true;
            colDetalleProducto.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colDetalleCantidad
            //
            colDetalleCantidad.DataPropertyName = "Cantidad";
            colDetalleCantidad.FillWeight = 14F;
            colDetalleCantidad.HeaderText = "Cantidad";
            colDetalleCantidad.Name = "colDetalleCantidad";
            colDetalleCantidad.ReadOnly = true;
            colDetalleCantidad.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colDetallePrecioCompra
            //
            colDetallePrecioCompra.DataPropertyName = "PrecioCompra";
            colDetallePrecioCompra.FillWeight = 20F;
            colDetallePrecioCompra.HeaderText = "Precio compra";
            colDetallePrecioCompra.Name = "colDetallePrecioCompra";
            colDetallePrecioCompra.ReadOnly = true;
            colDetallePrecioCompra.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // colDetalleSubtotal
            //
            colDetalleSubtotal.DataPropertyName = "Subtotal";
            colDetalleSubtotal.FillWeight = 20F;
            colDetalleSubtotal.HeaderText = "Subtotal";
            colDetalleSubtotal.Name = "colDetalleSubtotal";
            colDetalleSubtotal.ReadOnly = true;
            colDetalleSubtotal.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // lblMensajeDetalle
            //
            lblMensajeDetalle.AutoSize = false;
            lblMensajeDetalle.Depth = 0;
            lblMensajeDetalle.Dock = DockStyle.Fill;
            lblMensajeDetalle.Font = new Font("Roboto", 13F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblMensajeDetalle.Location = new Point(16, 520);
            lblMensajeDetalle.Margin = new Padding(0);
            lblMensajeDetalle.MouseState = MaterialSkin.MouseState.HOVER;
            lblMensajeDetalle.Name = "lblMensajeDetalle";
            lblMensajeDetalle.Size = new Size(868, 28);
            lblMensajeDetalle.TabIndex = 3;
            lblMensajeDetalle.TextAlign = ContentAlignment.MiddleLeft;
            //
            // DetalleCompraForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 560);
            Controls.Add(tblPrincipal);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(720, 480);
            Name = "DetalleCompraForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Detalle de compra";
            tblPrincipal.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridListaCompras).EndInit();
            tblCabecera.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridDetalle).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tblPrincipal;
        private MaterialSkin.Controls.MaterialLabel lblTituloDetalle;
        private DataGridView gridListaCompras;
        private DataGridViewTextBoxColumn colListaNumero;
        private DataGridViewTextBoxColumn colListaFecha;
        private DataGridViewTextBoxColumn colListaProveedor;
        private DataGridViewTextBoxColumn colListaUsuario;
        private DataGridViewTextBoxColumn colListaTotal;
        private DataGridViewTextBoxColumn colListaEstado;
        private TableLayoutPanel tblCabecera;
        private MaterialSkin.Controls.MaterialLabel lblNumeroCompra;
        private MaterialSkin.Controls.MaterialLabel lblFechaDetalle;
        private MaterialSkin.Controls.MaterialLabel lblEstadoDetalle;
        private MaterialSkin.Controls.MaterialLabel lblTotalDetalle;
        private MaterialSkin.Controls.MaterialLabel lblProveedorDetalle;
        private MaterialSkin.Controls.MaterialLabel lblUsuarioDetalle;
        private DataGridView gridDetalle;
        private DataGridViewTextBoxColumn colDetalleProducto;
        private DataGridViewTextBoxColumn colDetalleCantidad;
        private DataGridViewTextBoxColumn colDetallePrecioCompra;
        private DataGridViewTextBoxColumn colDetalleSubtotal;
        private MaterialSkin.Controls.MaterialLabel lblMensajeDetalle;
    }
}
