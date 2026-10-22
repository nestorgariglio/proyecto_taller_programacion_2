namespace CapaPresentacion
{
    partial class CategoriaForm
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
            txtDescripcion = new MaterialSkin.Controls.MaterialTextBox2();
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
            labelTitulo.Text = "Nueva categoría";
            //
            // panelCampos
            //
            panelCampos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelCampos.ColumnCount = 1;
            panelCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            panelCampos.Controls.Add(txtDescripcion, 0, 0);
            panelCampos.Location = new Point(24, 62);
            panelCampos.Name = "panelCampos";
            panelCampos.RowCount = 1;
            panelCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            panelCampos.Size = new Size(632, 58);
            panelCampos.TabIndex = 1;
            //
            // txtDescripcion
            //
            txtDescripcion.AnimateReadOnly = false;
            txtDescripcion.BackgroundImageLayout = ImageLayout.None;
            txtDescripcion.CharacterCasing = CharacterCasing.Normal;
            txtDescripcion.Depth = 0;
            txtDescripcion.Dock = DockStyle.Fill;
            txtDescripcion.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtDescripcion.Hint = "Descripción * (de 3 a 100 caracteres)";
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
            txtDescripcion.TabIndex = 0;
            txtDescripcion.TextAlign = HorizontalAlignment.Left;
            txtDescripcion.TrailingIcon = null;
            txtDescripcion.UseSystemPasswordChar = false;
            //
            // panelAcciones
            //
            panelAcciones.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            panelAcciones.AutoSize = true;
            panelAcciones.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelAcciones.Controls.Add(btnCancelar);
            panelAcciones.Controls.Add(btnGuardar);
            panelAcciones.FlowDirection = FlowDirection.RightToLeft;
            panelAcciones.Location = new Point(405, 150);
            panelAcciones.Margin = new Padding(0);
            panelAcciones.Name = "panelAcciones";
            panelAcciones.Size = new Size(251, 48);
            panelAcciones.TabIndex = 2;
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
            // CategoriaForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(680, 220);
            Controls.Add(panelAcciones);
            Controls.Add(panelCampos);
            Controls.Add(labelTitulo);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CategoriaForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Nueva categoría";
            panelCampos.ResumeLayout(false);
            panelAcciones.ResumeLayout(false);
            panelAcciones.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MaterialSkin.Controls.MaterialLabel labelTitulo;
        private TableLayoutPanel panelCampos;
        private MaterialSkin.Controls.MaterialTextBox2 txtDescripcion;
        private FlowLayoutPanel panelAcciones;
        private MaterialSkin.Controls.MaterialButton btnGuardar;
        private MaterialSkin.Controls.MaterialButton btnCancelar;
    }
}
