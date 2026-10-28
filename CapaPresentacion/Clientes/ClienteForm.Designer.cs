namespace CapaPresentacion.Clientes
{
    partial class ClienteForm
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
            txtDocumento = new MaterialSkin.Controls.MaterialTextBox2();
            txtNombre = new MaterialSkin.Controls.MaterialTextBox2();
            txtApellido = new MaterialSkin.Controls.MaterialTextBox2();
            txtCorreo = new MaterialSkin.Controls.MaterialTextBox2();
            txtTelefono = new MaterialSkin.Controls.MaterialTextBox2();
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
            labelTitulo.Text = "Nuevo cliente";
            //
            // panelCampos
            //
            panelCampos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelCampos.ColumnCount = 2;
            panelCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            panelCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            panelCampos.Controls.Add(txtDocumento, 0, 0);
            panelCampos.Controls.Add(txtNombre, 1, 0);
            panelCampos.Controls.Add(txtApellido, 0, 1);
            panelCampos.Controls.Add(txtCorreo, 1, 1);
            panelCampos.Controls.Add(txtTelefono, 0, 2);
            panelCampos.Location = new Point(24, 62);
            panelCampos.Name = "panelCampos";
            panelCampos.RowCount = 3;
            panelCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            panelCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            panelCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            panelCampos.Size = new Size(652, 174);
            panelCampos.TabIndex = 1;
            panelCampos.SetColumnSpan(txtTelefono, 2);
            //
            // txtDocumento
            //
            txtDocumento.AnimateReadOnly = false;
            txtDocumento.BackgroundImageLayout = ImageLayout.None;
            txtDocumento.CharacterCasing = CharacterCasing.Normal;
            txtDocumento.Depth = 0;
            txtDocumento.Dock = DockStyle.Fill;
            txtDocumento.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtDocumento.Hint = "Documento *";
            txtDocumento.Margin = new Padding(0, 0, 10, 10);
            txtDocumento.MaxLength = 32767;
            txtDocumento.MouseState = MaterialSkin.MouseState.OUT;
            txtDocumento.Name = "txtDocumento";
            txtDocumento.PasswordChar = '\0';
            txtDocumento.PrefixSuffixText = null;
            txtDocumento.ReadOnly = false;
            txtDocumento.RightToLeft = RightToLeft.No;
            txtDocumento.SelectedText = "";
            txtDocumento.SelectionLength = 0;
            txtDocumento.SelectionStart = 0;
            txtDocumento.ShortcutsEnabled = true;
            txtDocumento.TabIndex = 0;
            txtDocumento.TextAlign = HorizontalAlignment.Left;
            txtDocumento.TrailingIcon = null;
            txtDocumento.UseSystemPasswordChar = false;
            //
            // txtNombre
            //
            txtNombre.AnimateReadOnly = false;
            txtNombre.BackgroundImageLayout = ImageLayout.None;
            txtNombre.CharacterCasing = CharacterCasing.Normal;
            txtNombre.Depth = 0;
            txtNombre.Dock = DockStyle.Fill;
            txtNombre.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtNombre.Hint = "Nombre *";
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
            txtNombre.TabIndex = 1;
            txtNombre.TextAlign = HorizontalAlignment.Left;
            txtNombre.TrailingIcon = null;
            txtNombre.UseSystemPasswordChar = false;
            //
            // txtApellido
            //
            txtApellido.AnimateReadOnly = false;
            txtApellido.BackgroundImageLayout = ImageLayout.None;
            txtApellido.CharacterCasing = CharacterCasing.Normal;
            txtApellido.Depth = 0;
            txtApellido.Dock = DockStyle.Fill;
            txtApellido.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtApellido.Hint = "Apellido *";
            txtApellido.Margin = new Padding(0, 0, 10, 10);
            txtApellido.MaxLength = 32767;
            txtApellido.MouseState = MaterialSkin.MouseState.OUT;
            txtApellido.Name = "txtApellido";
            txtApellido.PasswordChar = '\0';
            txtApellido.PrefixSuffixText = null;
            txtApellido.ReadOnly = false;
            txtApellido.RightToLeft = RightToLeft.No;
            txtApellido.SelectedText = "";
            txtApellido.SelectionLength = 0;
            txtApellido.SelectionStart = 0;
            txtApellido.ShortcutsEnabled = true;
            txtApellido.TabIndex = 2;
            txtApellido.TextAlign = HorizontalAlignment.Left;
            txtApellido.TrailingIcon = null;
            txtApellido.UseSystemPasswordChar = false;
            //
            // txtCorreo
            //
            txtCorreo.AnimateReadOnly = false;
            txtCorreo.BackgroundImageLayout = ImageLayout.None;
            txtCorreo.CharacterCasing = CharacterCasing.Normal;
            txtCorreo.Depth = 0;
            txtCorreo.Dock = DockStyle.Fill;
            txtCorreo.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtCorreo.Hint = "Correo (opcional)";
            txtCorreo.Margin = new Padding(0, 0, 0, 10);
            txtCorreo.MaxLength = 32767;
            txtCorreo.MouseState = MaterialSkin.MouseState.OUT;
            txtCorreo.Name = "txtCorreo";
            txtCorreo.PasswordChar = '\0';
            txtCorreo.PrefixSuffixText = null;
            txtCorreo.ReadOnly = false;
            txtCorreo.RightToLeft = RightToLeft.No;
            txtCorreo.SelectedText = "";
            txtCorreo.SelectionLength = 0;
            txtCorreo.SelectionStart = 0;
            txtCorreo.ShortcutsEnabled = true;
            txtCorreo.TabIndex = 3;
            txtCorreo.TextAlign = HorizontalAlignment.Left;
            txtCorreo.TrailingIcon = null;
            txtCorreo.UseSystemPasswordChar = false;
            //
            // txtTelefono
            //
            txtTelefono.AnimateReadOnly = false;
            txtTelefono.BackgroundImageLayout = ImageLayout.None;
            txtTelefono.CharacterCasing = CharacterCasing.Normal;
            txtTelefono.Depth = 0;
            txtTelefono.Dock = DockStyle.Fill;
            txtTelefono.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtTelefono.Hint = "Teléfono (opcional)";
            txtTelefono.Margin = new Padding(0, 0, 0, 10);
            txtTelefono.MaxLength = 32767;
            txtTelefono.MouseState = MaterialSkin.MouseState.OUT;
            txtTelefono.Name = "txtTelefono";
            txtTelefono.PasswordChar = '\0';
            txtTelefono.PrefixSuffixText = null;
            txtTelefono.ReadOnly = false;
            txtTelefono.RightToLeft = RightToLeft.No;
            txtTelefono.SelectedText = "";
            txtTelefono.SelectionLength = 0;
            txtTelefono.SelectionStart = 0;
            txtTelefono.ShortcutsEnabled = true;
            txtTelefono.TabIndex = 4;
            txtTelefono.TextAlign = HorizontalAlignment.Left;
            txtTelefono.TrailingIcon = null;
            txtTelefono.UseSystemPasswordChar = false;
            //
            // panelAcciones
            //
            panelAcciones.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            panelAcciones.AutoSize = true;
            panelAcciones.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelAcciones.Controls.Add(btnCancelar);
            panelAcciones.Controls.Add(btnGuardar);
            panelAcciones.FlowDirection = FlowDirection.RightToLeft;
            panelAcciones.Location = new Point(425, 252);
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
            // ClienteForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(700, 320);
            Controls.Add(panelAcciones);
            Controls.Add(panelCampos);
            Controls.Add(labelTitulo);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ClienteForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Nuevo cliente";
            panelCampos.ResumeLayout(false);
            panelAcciones.ResumeLayout(false);
            panelAcciones.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MaterialSkin.Controls.MaterialLabel labelTitulo;
        private TableLayoutPanel panelCampos;
        private MaterialSkin.Controls.MaterialTextBox2 txtDocumento;
        private MaterialSkin.Controls.MaterialTextBox2 txtNombre;
        private MaterialSkin.Controls.MaterialTextBox2 txtApellido;
        private MaterialSkin.Controls.MaterialTextBox2 txtCorreo;
        private MaterialSkin.Controls.MaterialTextBox2 txtTelefono;
        private FlowLayoutPanel panelAcciones;
        private MaterialSkin.Controls.MaterialButton btnGuardar;
        private MaterialSkin.Controls.MaterialButton btnCancelar;
    }
}
