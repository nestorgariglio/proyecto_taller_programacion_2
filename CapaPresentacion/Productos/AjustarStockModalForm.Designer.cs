namespace CapaPresentacion.Productos
{
    partial class AjustarStockModalForm
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
            lblProducto = new MaterialSkin.Controls.MaterialLabel();
            lblStockActual = new MaterialSkin.Controls.MaterialLabel();
            txtNuevoStock = new MaterialSkin.Controls.MaterialTextBox2();
            txtMotivo = new MaterialSkin.Controls.MaterialTextBox2();
            panelAcciones = new FlowLayoutPanel();
            btnGuardar = new MaterialSkin.Controls.MaterialButton();
            btnCancelar = new MaterialSkin.Controls.MaterialButton();
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
            labelTitulo.Size = new Size(141, 24);
            labelTitulo.TabIndex = 0;
            labelTitulo.Text = "Ajustar stock";
            //
            // lblProducto
            //
            lblProducto.AutoSize = true;
            lblProducto.Depth = 0;
            lblProducto.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblProducto.Location = new Point(24, 58);
            lblProducto.MouseState = MaterialSkin.MouseState.HOVER;
            lblProducto.Name = "lblProducto";
            lblProducto.Size = new Size(0, 18);
            lblProducto.TabIndex = 1;
            //
            // lblStockActual
            //
            lblStockActual.AutoSize = true;
            lblStockActual.Depth = 0;
            lblStockActual.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblStockActual.Location = new Point(24, 84);
            lblStockActual.MouseState = MaterialSkin.MouseState.HOVER;
            lblStockActual.Name = "lblStockActual";
            lblStockActual.Size = new Size(0, 18);
            lblStockActual.TabIndex = 2;
            //
            // txtNuevoStock
            //
            txtNuevoStock.AnimateReadOnly = false;
            txtNuevoStock.BackgroundImageLayout = ImageLayout.None;
            txtNuevoStock.CharacterCasing = CharacterCasing.Normal;
            txtNuevoStock.Depth = 0;
            txtNuevoStock.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtNuevoStock.Hint = "Nuevo stock * (0 o más)";
            txtNuevoStock.Location = new Point(24, 120);
            txtNuevoStock.MaxLength = 10;
            txtNuevoStock.MouseState = MaterialSkin.MouseState.OUT;
            txtNuevoStock.Name = "txtNuevoStock";
            txtNuevoStock.PasswordChar = '\0';
            txtNuevoStock.PrefixSuffixText = null;
            txtNuevoStock.ReadOnly = false;
            txtNuevoStock.RightToLeft = RightToLeft.No;
            txtNuevoStock.SelectedText = "";
            txtNuevoStock.SelectionLength = 0;
            txtNuevoStock.SelectionStart = 0;
            txtNuevoStock.ShortcutsEnabled = true;
            txtNuevoStock.Size = new Size(512, 48);
            txtNuevoStock.TabIndex = 3;
            txtNuevoStock.TextAlign = HorizontalAlignment.Left;
            txtNuevoStock.TrailingIcon = null;
            txtNuevoStock.UseSystemPasswordChar = false;
            //
            // txtMotivo
            //
            txtMotivo.AnimateReadOnly = false;
            txtMotivo.BackgroundImageLayout = ImageLayout.None;
            txtMotivo.CharacterCasing = CharacterCasing.Normal;
            txtMotivo.Depth = 0;
            txtMotivo.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtMotivo.Hint = "Motivo * (obligatorio, hasta 300 caracteres)";
            txtMotivo.Location = new Point(24, 188);
            txtMotivo.MaxLength = 300;
            txtMotivo.MouseState = MaterialSkin.MouseState.OUT;
            txtMotivo.Name = "txtMotivo";
            txtMotivo.PasswordChar = '\0';
            txtMotivo.PrefixSuffixText = null;
            txtMotivo.ReadOnly = false;
            txtMotivo.RightToLeft = RightToLeft.No;
            txtMotivo.SelectedText = "";
            txtMotivo.SelectionLength = 0;
            txtMotivo.SelectionStart = 0;
            txtMotivo.ShortcutsEnabled = true;
            txtMotivo.Size = new Size(512, 48);
            txtMotivo.TabIndex = 4;
            txtMotivo.TextAlign = HorizontalAlignment.Left;
            txtMotivo.TrailingIcon = null;
            txtMotivo.UseSystemPasswordChar = false;
            //
            // panelAcciones
            //
            panelAcciones.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            panelAcciones.AutoSize = true;
            panelAcciones.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelAcciones.Controls.Add(btnCancelar);
            panelAcciones.Controls.Add(btnGuardar);
            panelAcciones.FlowDirection = FlowDirection.RightToLeft;
            panelAcciones.Location = new Point(285, 258);
            panelAcciones.Margin = new Padding(0);
            panelAcciones.Name = "panelAcciones";
            panelAcciones.Size = new Size(251, 48);
            panelAcciones.TabIndex = 5;
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
            // AjustarStockModalForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(560, 320);
            Controls.Add(panelAcciones);
            Controls.Add(txtMotivo);
            Controls.Add(txtNuevoStock);
            Controls.Add(lblStockActual);
            Controls.Add(lblProducto);
            Controls.Add(labelTitulo);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AjustarStockModalForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Ajustar stock";
            panelAcciones.ResumeLayout(false);
            panelAcciones.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MaterialSkin.Controls.MaterialLabel labelTitulo;
        private MaterialSkin.Controls.MaterialLabel lblProducto;
        private MaterialSkin.Controls.MaterialLabel lblStockActual;
        private MaterialSkin.Controls.MaterialTextBox2 txtNuevoStock;
        private MaterialSkin.Controls.MaterialTextBox2 txtMotivo;
        private FlowLayoutPanel panelAcciones;
        private MaterialSkin.Controls.MaterialButton btnGuardar;
        private MaterialSkin.Controls.MaterialButton btnCancelar;
    }
}
