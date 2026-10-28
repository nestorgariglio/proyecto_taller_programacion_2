namespace CapaPresentacion.Productos
{
    partial class ProductoModalForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Releases resources used by the form.
        /// </summary>
        /// <param name="disposing">true to release managed resources; otherwise, false.</param>
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
            labelTitulo = new MaterialSkin.Controls.MaterialLabel();
            panelCampos = new TableLayoutPanel();
            txtCodigo = new MaterialSkin.Controls.MaterialTextBox2();
            cboCategoria = new MaterialSkin.Controls.MaterialComboBox();
            txtNombre = new MaterialSkin.Controls.MaterialTextBox2();
            txtDescripcion = new MaterialSkin.Controls.MaterialTextBox2();
            txtPrecioCompra = new MaterialSkin.Controls.MaterialTextBox2();
            txtPrecioVenta = new MaterialSkin.Controls.MaterialTextBox2();
            lblAvisoCategoria = new MaterialSkin.Controls.MaterialLabel();
            panelAcciones = new FlowLayoutPanel();
            btnGuardar = new MaterialSkin.Controls.MaterialButton();
            btnCancelar = new MaterialSkin.Controls.MaterialButton();
            panelCampos.SuspendLayout();
            panelAcciones.SuspendLayout();
            SuspendLayout();
            //
            // labelTitulo
            //
            labelTitulo.AutoSize = true;
            labelTitulo.Depth = 0;
            labelTitulo.Font = new Font("Roboto", 20F, FontStyle.Regular, GraphicsUnit.Pixel);
            labelTitulo.Location = new Point(24, 18);
            labelTitulo.MouseState = MaterialSkin.MouseState.HOVER;
            labelTitulo.Name = "labelTitulo";
            labelTitulo.Size = new Size(155, 24);
            labelTitulo.TabIndex = 0;
            labelTitulo.Text = "Nuevo producto";
            //
            // panelCampos
            //
            panelCampos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelCampos.ColumnCount = 2;
            panelCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            panelCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            panelCampos.Controls.Add(txtCodigo, 0, 0);
            panelCampos.Controls.Add(cboCategoria, 1, 0);
            panelCampos.Controls.Add(txtNombre, 0, 1);
            panelCampos.Controls.Add(txtDescripcion, 0, 2);
            panelCampos.Controls.Add(txtPrecioCompra, 0, 3);
            panelCampos.Controls.Add(txtPrecioVenta, 1, 3);
            panelCampos.Location = new Point(24, 60);
            panelCampos.Name = "panelCampos";
            panelCampos.RowCount = 4;
            panelCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            panelCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            panelCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            panelCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            panelCampos.Size = new Size(732, 232);
            panelCampos.TabIndex = 1;
            //
            // txtCodigo
            //
            txtCodigo.AnimateReadOnly = false;
            txtCodigo.BackgroundImageLayout = ImageLayout.None;
            txtCodigo.CharacterCasing = CharacterCasing.Normal;
            txtCodigo.Depth = 0;
            txtCodigo.Dock = DockStyle.Fill;
            txtCodigo.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtCodigo.Hint = "Código * (8 a 13 dígitos)";
            txtCodigo.Margin = new Padding(0, 0, 10, 10);
            txtCodigo.MaxLength = 13;
            txtCodigo.MouseState = MaterialSkin.MouseState.OUT;
            txtCodigo.Name = "txtCodigo";
            txtCodigo.PasswordChar = '\0';
            txtCodigo.PrefixSuffixText = null;
            txtCodigo.ReadOnly = false;
            txtCodigo.RightToLeft = RightToLeft.No;
            txtCodigo.SelectedText = "";
            txtCodigo.SelectionLength = 0;
            txtCodigo.SelectionStart = 0;
            txtCodigo.ShortcutsEnabled = true;
            txtCodigo.TabIndex = 0;
            txtCodigo.TextAlign = HorizontalAlignment.Left;
            txtCodigo.TrailingIcon = null;
            txtCodigo.UseSystemPasswordChar = false;
            //
            // cboCategoria
            //
            cboCategoria.Depth = 0;
            cboCategoria.Dock = DockStyle.Fill;
            cboCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCategoria.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            cboCategoria.FormattingEnabled = true;
            cboCategoria.Hint = "Categoría *";
            cboCategoria.Margin = new Padding(10, 0, 0, 10);
            cboCategoria.MouseState = MaterialSkin.MouseState.OUT;
            cboCategoria.Name = "cboCategoria";
            cboCategoria.Size = new Size(356, 36);
            cboCategoria.TabIndex = 1;
            //
            // txtNombre
            //
            panelCampos.SetColumnSpan(txtNombre, 2);
            txtNombre.AnimateReadOnly = false;
            txtNombre.BackgroundImageLayout = ImageLayout.None;
            txtNombre.CharacterCasing = CharacterCasing.Normal;
            txtNombre.Depth = 0;
            txtNombre.Dock = DockStyle.Fill;
            txtNombre.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtNombre.Hint = "Nombre * (de 3 a 100 caracteres)";
            txtNombre.Margin = new Padding(0, 0, 0, 10);
            txtNombre.MaxLength = 32767;
            txtNombre.MouseState = MaterialSkin.MouseState.OUT;
            txtNombre.Name = "txtNombre";
            txtNombre.PasswordChar = '\0';
            txtNombre.PrefixSuffixText = null;
            txtNombre.ReadOnly = false;
            txtNombre.RightToLeft = RightToLeft.No;
            txtNombre.SelectedText = "";
            txtNombre.SelectionLength = 0;
            txtNombre.SelectionStart = 0;
            txtNombre.ShortcutsEnabled = true;
            txtNombre.TabIndex = 2;
            txtNombre.TextAlign = HorizontalAlignment.Left;
            txtNombre.TrailingIcon = null;
            txtNombre.UseSystemPasswordChar = false;
            //
            // txtDescripcion
            //
            panelCampos.SetColumnSpan(txtDescripcion, 2);
            txtDescripcion.AnimateReadOnly = false;
            txtDescripcion.BackgroundImageLayout = ImageLayout.None;
            txtDescripcion.CharacterCasing = CharacterCasing.Normal;
            txtDescripcion.Depth = 0;
            txtDescripcion.Dock = DockStyle.Fill;
            txtDescripcion.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtDescripcion.Hint = "Descripción (opcional, hasta 300 caracteres)";
            txtDescripcion.Margin = new Padding(0, 0, 0, 10);
            txtDescripcion.MaxLength = 32767;
            txtDescripcion.MouseState = MaterialSkin.MouseState.OUT;
            txtDescripcion.Name = "txtDescripcion";
            txtDescripcion.PasswordChar = '\0';
            txtDescripcion.PrefixSuffixText = null;
            txtDescripcion.ReadOnly = false;
            txtDescripcion.RightToLeft = RightToLeft.No;
            txtDescripcion.SelectedText = "";
            txtDescripcion.SelectionLength = 0;
            txtDescripcion.SelectionStart = 0;
            txtDescripcion.ShortcutsEnabled = true;
            txtDescripcion.TabIndex = 3;
            txtDescripcion.TextAlign = HorizontalAlignment.Left;
            txtDescripcion.TrailingIcon = null;
            txtDescripcion.UseSystemPasswordChar = false;
            //
            // txtPrecioCompra
            //
            txtPrecioCompra.AnimateReadOnly = false;
            txtPrecioCompra.BackgroundImageLayout = ImageLayout.None;
            txtPrecioCompra.CharacterCasing = CharacterCasing.Normal;
            txtPrecioCompra.Depth = 0;
            txtPrecioCompra.Dock = DockStyle.Fill;
            txtPrecioCompra.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtPrecioCompra.Hint = "Precio de compra *";
            txtPrecioCompra.Margin = new Padding(0, 0, 10, 10);
            txtPrecioCompra.MaxLength = 32767;
            txtPrecioCompra.MouseState = MaterialSkin.MouseState.OUT;
            txtPrecioCompra.Name = "txtPrecioCompra";
            txtPrecioCompra.PasswordChar = '\0';
            txtPrecioCompra.PrefixSuffixText = null;
            txtPrecioCompra.ReadOnly = false;
            txtPrecioCompra.RightToLeft = RightToLeft.No;
            txtPrecioCompra.SelectedText = "";
            txtPrecioCompra.SelectionLength = 0;
            txtPrecioCompra.SelectionStart = 0;
            txtPrecioCompra.ShortcutsEnabled = true;
            txtPrecioCompra.TabIndex = 4;
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
            txtPrecioVenta.Hint = "Precio de venta *";
            txtPrecioVenta.Margin = new Padding(10, 0, 0, 10);
            txtPrecioVenta.MaxLength = 32767;
            txtPrecioVenta.MouseState = MaterialSkin.MouseState.OUT;
            txtPrecioVenta.Name = "txtPrecioVenta";
            txtPrecioVenta.PasswordChar = '\0';
            txtPrecioVenta.PrefixSuffixText = null;
            txtPrecioVenta.ReadOnly = false;
            txtPrecioVenta.RightToLeft = RightToLeft.No;
            txtPrecioVenta.SelectedText = "";
            txtPrecioVenta.SelectionLength = 0;
            txtPrecioVenta.SelectionStart = 0;
            txtPrecioVenta.ShortcutsEnabled = true;
            txtPrecioVenta.TabIndex = 5;
            txtPrecioVenta.TextAlign = HorizontalAlignment.Left;
            txtPrecioVenta.TrailingIcon = null;
            txtPrecioVenta.UseSystemPasswordChar = false;
            //
            // lblAvisoCategoria
            //
            lblAvisoCategoria.AutoSize = true;
            lblAvisoCategoria.Depth = 0;
            lblAvisoCategoria.Font = new Font("Roboto", 11F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblAvisoCategoria.ForeColor = Color.FromArgb(255, 193, 7);
            lblAvisoCategoria.Location = new Point(24, 300);
            lblAvisoCategoria.MouseState = MaterialSkin.MouseState.HOVER;
            lblAvisoCategoria.Name = "lblAvisoCategoria";
            lblAvisoCategoria.Size = new Size(0, 15);
            lblAvisoCategoria.TabIndex = 2;
            lblAvisoCategoria.Visible = false;
            //
            // panelAcciones
            //
            panelAcciones.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            panelAcciones.AutoSize = true;
            panelAcciones.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelAcciones.Controls.Add(btnCancelar);
            panelAcciones.Controls.Add(btnGuardar);
            panelAcciones.FlowDirection = FlowDirection.RightToLeft;
            panelAcciones.Location = new Point(505, 336);
            panelAcciones.Margin = new Padding(0);
            panelAcciones.Name = "panelAcciones";
            panelAcciones.Size = new Size(251, 48);
            panelAcciones.TabIndex = 3;
            panelAcciones.WrapContents = false;
            //
            // btnGuardar
            //
            btnGuardar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnGuardar.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnGuardar.Depth = 0;
            btnGuardar.HighEmphasis = true;
            btnGuardar.Icon = null;
            btnGuardar.Margin = new Padding(4, 6, 4, 6);
            btnGuardar.MouseState = MaterialSkin.MouseState.HOVER;
            btnGuardar.Name = "btnGuardar";
            btnGuardar.NoAccentTextColor = Color.Empty;
            btnGuardar.Size = new Size(103, 36);
            btnGuardar.TabIndex = 1;
            btnGuardar.Text = "Guardar";
            btnGuardar.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnGuardar.UseAccentColor = false;
            btnGuardar.UseVisualStyleBackColor = true;
            //
            // btnCancelar
            //
            btnCancelar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnCancelar.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnCancelar.Depth = 0;
            btnCancelar.HighEmphasis = true;
            btnCancelar.Icon = null;
            btnCancelar.Margin = new Padding(4, 6, 4, 6);
            btnCancelar.MouseState = MaterialSkin.MouseState.HOVER;
            btnCancelar.Name = "btnCancelar";
            btnCancelar.NoAccentTextColor = Color.Empty;
            btnCancelar.Size = new Size(103, 36);
            btnCancelar.TabIndex = 0;
            btnCancelar.Text = "Cancelar";
            btnCancelar.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            btnCancelar.UseAccentColor = false;
            btnCancelar.UseVisualStyleBackColor = true;
            //
            // ProductoModalForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(780, 400);
            Controls.Add(panelAcciones);
            Controls.Add(lblAvisoCategoria);
            Controls.Add(panelCampos);
            Controls.Add(labelTitulo);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ProductoModalForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Nuevo producto";
            panelCampos.ResumeLayout(false);
            panelCampos.PerformLayout();
            panelAcciones.ResumeLayout(false);
            panelAcciones.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MaterialSkin.Controls.MaterialLabel labelTitulo;
        private TableLayoutPanel panelCampos;
        private MaterialSkin.Controls.MaterialTextBox2 txtCodigo;
        private MaterialSkin.Controls.MaterialComboBox cboCategoria;
        private MaterialSkin.Controls.MaterialTextBox2 txtNombre;
        private MaterialSkin.Controls.MaterialTextBox2 txtDescripcion;
        private MaterialSkin.Controls.MaterialTextBox2 txtPrecioCompra;
        private MaterialSkin.Controls.MaterialTextBox2 txtPrecioVenta;
        private MaterialSkin.Controls.MaterialLabel lblAvisoCategoria;
        private FlowLayoutPanel panelAcciones;
        private MaterialSkin.Controls.MaterialButton btnGuardar;
        private MaterialSkin.Controls.MaterialButton btnCancelar;
    }
}
