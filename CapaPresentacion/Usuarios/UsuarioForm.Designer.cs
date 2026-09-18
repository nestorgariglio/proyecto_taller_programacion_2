namespace CapaPresentacion
{
    partial class UsuarioForm
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
            txtDni = new MaterialSkin.Controls.MaterialTextBox2();
            comboSexo = new MaterialSkin.Controls.MaterialComboBox();
            txtNombre = new MaterialSkin.Controls.MaterialTextBox2();
            txtApellido = new MaterialSkin.Controls.MaterialTextBox2();
            txtCorreo = new MaterialSkin.Controls.MaterialTextBox2();
            comboRol = new MaterialSkin.Controls.MaterialComboBox();
            txtClave = new MaterialSkin.Controls.MaterialTextBox2();
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
            labelTitulo.Size = new Size(190, 24);
            labelTitulo.TabIndex = 0;
            labelTitulo.Text = "Nuevo usuario";
            //
            // panelCampos
            //
            panelCampos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelCampos.ColumnCount = 2;
            panelCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            panelCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            panelCampos.Controls.Add(txtDni, 0, 0);
            panelCampos.Controls.Add(comboSexo, 1, 0);
            panelCampos.Controls.Add(txtNombre, 0, 1);
            panelCampos.Controls.Add(txtApellido, 1, 1);
            panelCampos.Controls.Add(txtCorreo, 0, 2);
            panelCampos.Controls.Add(comboRol, 1, 2);
            panelCampos.Controls.Add(txtClave, 0, 3);
            panelCampos.SetColumnSpan(txtClave, 2);
            panelCampos.Location = new Point(24, 62);
            panelCampos.Name = "panelCampos";
            panelCampos.RowCount = 4;
            panelCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            panelCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            panelCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            panelCampos.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            panelCampos.Size = new Size(632, 232);
            panelCampos.TabIndex = 1;
            //
            // txtDni
            //
            txtDni.AnimateReadOnly = false;
            txtDni.BackgroundImageLayout = ImageLayout.None;
            txtDni.CharacterCasing = CharacterCasing.Normal;
            txtDni.Depth = 0;
            txtDni.Dock = DockStyle.Fill;
            txtDni.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtDni.Hint = "DNI *";
            txtDni.Margin = new Padding(0, 0, 8, 10);
            txtDni.MaxLength = 32767;
            txtDni.MouseState = MaterialSkin.MouseState.OUT;
            txtDni.Name = "txtDni";
            txtDni.PasswordChar = '\0';
            txtDni.PrefixSuffixText = null;
            txtDni.ReadOnly = false;
            txtDni.RightToLeft = RightToLeft.No;
            txtDni.SelectedText = "";
            txtDni.SelectionLength = 0;
            txtDni.SelectionStart = 0;
            txtDni.ShortcutsEnabled = true;
            txtDni.TabIndex = 0;
            txtDni.TextAlign = HorizontalAlignment.Left;
            txtDni.TrailingIcon = null;
            txtDni.UseSystemPasswordChar = false;
            //
            // comboSexo
            //
            comboSexo.AutoResize = false;
            comboSexo.BackColor = Color.FromArgb(255, 255, 255);
            comboSexo.Depth = 0;
            comboSexo.Dock = DockStyle.Fill;
            comboSexo.DrawMode = DrawMode.OwnerDrawVariable;
            comboSexo.DropDownHeight = 174;
            comboSexo.DropDownStyle = ComboBoxStyle.DropDownList;
            comboSexo.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold, GraphicsUnit.Pixel);
            comboSexo.FormattingEnabled = true;
            comboSexo.IntegralHeight = false;
            comboSexo.ItemHeight = 43;
            comboSexo.Margin = new Padding(8, 0, 0, 10);
            comboSexo.MaxDropDownItems = 3;
            comboSexo.MouseState = MaterialSkin.MouseState.OUT;
            comboSexo.Name = "comboSexo";
            comboSexo.Size = new Size(308, 48);
            comboSexo.StartIndex = 0;
            comboSexo.TabIndex = 1;
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
            txtNombre.Margin = new Padding(0, 0, 8, 10);
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
            // txtApellido
            //
            txtApellido.AnimateReadOnly = false;
            txtApellido.BackgroundImageLayout = ImageLayout.None;
            txtApellido.CharacterCasing = CharacterCasing.Normal;
            txtApellido.Depth = 0;
            txtApellido.Dock = DockStyle.Fill;
            txtApellido.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtApellido.Hint = "Apellido *";
            txtApellido.Margin = new Padding(8, 0, 0, 10);
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
            txtApellido.TabIndex = 3;
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
            txtCorreo.Margin = new Padding(0, 0, 8, 10);
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
            txtCorreo.TabIndex = 4;
            txtCorreo.TextAlign = HorizontalAlignment.Left;
            txtCorreo.TrailingIcon = null;
            txtCorreo.UseSystemPasswordChar = false;
            //
            // comboRol
            //
            comboRol.AutoResize = false;
            comboRol.BackColor = Color.FromArgb(255, 255, 255);
            comboRol.Depth = 0;
            comboRol.Dock = DockStyle.Fill;
            comboRol.DrawMode = DrawMode.OwnerDrawVariable;
            comboRol.DropDownHeight = 174;
            comboRol.DropDownStyle = ComboBoxStyle.DropDownList;
            comboRol.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold, GraphicsUnit.Pixel);
            comboRol.FormattingEnabled = true;
            comboRol.IntegralHeight = false;
            comboRol.ItemHeight = 43;
            comboRol.Margin = new Padding(8, 0, 0, 10);
            comboRol.MaxDropDownItems = 5;
            comboRol.MouseState = MaterialSkin.MouseState.OUT;
            comboRol.Name = "comboRol";
            comboRol.Size = new Size(308, 48);
            comboRol.StartIndex = 0;
            comboRol.TabIndex = 5;
            //
            // txtClave
            //
            txtClave.AnimateReadOnly = false;
            txtClave.BackgroundImageLayout = ImageLayout.None;
            txtClave.CharacterCasing = CharacterCasing.Normal;
            txtClave.Depth = 0;
            txtClave.Dock = DockStyle.Fill;
            txtClave.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtClave.Hint = "Clave *";
            txtClave.Margin = new Padding(0, 0, 0, 10);
            txtClave.MaxLength = 32767;
            txtClave.MouseState = MaterialSkin.MouseState.OUT;
            txtClave.Name = "txtClave";
            txtClave.PasswordChar = '*';
            txtClave.PrefixSuffixText = null;
            txtClave.ReadOnly = false;
            txtClave.RightToLeft = RightToLeft.No;
            txtClave.SelectedText = "";
            txtClave.SelectionLength = 0;
            txtClave.SelectionStart = 0;
            txtClave.ShortcutsEnabled = true;
            txtClave.TabIndex = 6;
            txtClave.TextAlign = HorizontalAlignment.Left;
            txtClave.TrailingIcon = null;
            txtClave.UseSystemPasswordChar = false;
            //
            // panelAcciones
            //
            panelAcciones.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            panelAcciones.AutoSize = true;
            panelAcciones.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelAcciones.Controls.Add(btnCancelar);
            panelAcciones.Controls.Add(btnGuardar);
            panelAcciones.FlowDirection = FlowDirection.RightToLeft;
            panelAcciones.Location = new Point(405, 325);
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
            btnGuardar.Enabled = false;
            btnGuardar.HighEmphasis = true;
            btnGuardar.Icon = null;
            btnGuardar.Margin = new Padding(4, 6, 4, 6);
            btnGuardar.MouseState = MaterialSkin.MouseState.HOVER;
            btnGuardar.Name = "btnGuardar";
            btnGuardar.NoAccentTextColor = Color.Empty;
            btnGuardar.Size = new Size(103, 36);
            btnGuardar.TabIndex = 8;
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
            btnCancelar.TabIndex = 7;
            btnCancelar.Text = "Cancelar";
            btnCancelar.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            btnCancelar.UseAccentColor = false;
            btnCancelar.UseVisualStyleBackColor = true;
            //
            // UsuarioForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(680, 400);
            Controls.Add(panelAcciones);
            Controls.Add(panelCampos);
            Controls.Add(labelTitulo);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "UsuarioForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Nuevo usuario";
            panelCampos.ResumeLayout(false);
            panelAcciones.ResumeLayout(false);
            panelAcciones.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MaterialSkin.Controls.MaterialLabel labelTitulo;
        private TableLayoutPanel panelCampos;
        private MaterialSkin.Controls.MaterialTextBox2 txtDni;
        private MaterialSkin.Controls.MaterialComboBox comboSexo;
        private MaterialSkin.Controls.MaterialTextBox2 txtNombre;
        private MaterialSkin.Controls.MaterialTextBox2 txtApellido;
        private MaterialSkin.Controls.MaterialTextBox2 txtCorreo;
        private MaterialSkin.Controls.MaterialComboBox comboRol;
        private MaterialSkin.Controls.MaterialTextBox2 txtClave;
        private FlowLayoutPanel panelAcciones;
        private MaterialSkin.Controls.MaterialButton btnGuardar;
        private MaterialSkin.Controls.MaterialButton btnCancelar;
    }
}
