namespace CapaPresentacion
{
    partial class login
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
            label_dni = new MaterialSkin.Controls.MaterialLabel();
            btn_ingresar = new MaterialSkin.Controls.MaterialButton();
            pictureBox1 = new PictureBox();
            label_pass = new MaterialSkin.Controls.MaterialLabel();
            btn_cancelar = new MaterialSkin.Controls.MaterialButton();
            textbox_dni = new MaterialSkin.Controls.MaterialTextBox2();
            textbox_clave = new MaterialSkin.Controls.MaterialTextBox2();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // label_dni
            // 
            label_dni.AutoSize = true;
            label_dni.Depth = 0;
            label_dni.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            label_dni.Location = new Point(217, 105);
            label_dni.MouseState = MaterialSkin.MouseState.HOVER;
            label_dni.Name = "label_dni";
            label_dni.Size = new Size(31, 19);
            label_dni.TabIndex = 0;
            label_dni.Text = "DNI:";
            // 
            // btn_ingresar
            // 
            btn_ingresar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btn_ingresar.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btn_ingresar.Depth = 0;
            btn_ingresar.HighEmphasis = true;
            btn_ingresar.Icon = null;
            btn_ingresar.Location = new Point(262, 215);
            btn_ingresar.Margin = new Padding(4, 6, 4, 6);
            btn_ingresar.MouseState = MaterialSkin.MouseState.HOVER;
            btn_ingresar.Name = "btn_ingresar";
            btn_ingresar.NoAccentTextColor = Color.Empty;
            btn_ingresar.Size = new Size(91, 36);
            btn_ingresar.TabIndex = 2;
            btn_ingresar.Text = "ingresar";
            btn_ingresar.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btn_ingresar.UseAccentColor = false;
            btn_ingresar.UseVisualStyleBackColor = true;
            btn_ingresar.Click += btn_ingresar_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Dock = DockStyle.Left;
            pictureBox1.Image = Properties.Resources.store;
            pictureBox1.Location = new Point(3, 64);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(157, 193);
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox1.TabIndex = 3;
            pictureBox1.TabStop = false;
            // 
            // label_pass
            // 
            label_pass.AutoSize = true;
            label_pass.Depth = 0;
            label_pass.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            label_pass.Location = new Point(217, 151);
            label_pass.MouseState = MaterialSkin.MouseState.HOVER;
            label_pass.Name = "label_pass";
            label_pass.Size = new Size(40, 19);
            label_pass.TabIndex = 4;
            label_pass.Text = "Pass:";
            // 
            // btn_cancelar
            // 
            btn_cancelar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btn_cancelar.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btn_cancelar.Depth = 0;
            btn_cancelar.HighEmphasis = true;
            btn_cancelar.Icon = null;
            btn_cancelar.Location = new Point(373, 215);
            btn_cancelar.Margin = new Padding(4, 6, 4, 6);
            btn_cancelar.MouseState = MaterialSkin.MouseState.HOVER;
            btn_cancelar.Name = "btn_cancelar";
            btn_cancelar.NoAccentTextColor = Color.Empty;
            btn_cancelar.Size = new Size(96, 36);
            btn_cancelar.TabIndex = 5;
            btn_cancelar.Text = "cancelar";
            btn_cancelar.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btn_cancelar.UseAccentColor = false;
            btn_cancelar.UseVisualStyleBackColor = true;
            // 
            // textbox_dni
            // 
            textbox_dni.AnimateReadOnly = false;
            textbox_dni.BackgroundImageLayout = ImageLayout.None;
            textbox_dni.CharacterCasing = CharacterCasing.Normal;
            textbox_dni.Depth = 0;
            textbox_dni.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            textbox_dni.HideSelection = true;
            textbox_dni.LeadingIcon = null;
            textbox_dni.Location = new Point(262, 88);
            textbox_dni.MaxLength = 32767;
            textbox_dni.MouseState = MaterialSkin.MouseState.OUT;
            textbox_dni.Name = "textbox_dni";
            textbox_dni.PasswordChar = '\0';
            textbox_dni.PrefixSuffixText = null;
            textbox_dni.ReadOnly = false;
            textbox_dni.RightToLeft = RightToLeft.No;
            textbox_dni.SelectedText = "";
            textbox_dni.SelectionLength = 0;
            textbox_dni.SelectionStart = 0;
            textbox_dni.ShortcutsEnabled = true;
            textbox_dni.Size = new Size(207, 48);
            textbox_dni.TabIndex = 6;
            textbox_dni.TabStop = false;
            textbox_dni.TextAlign = HorizontalAlignment.Left;
            textbox_dni.TrailingIcon = null;
            textbox_dni.UseSystemPasswordChar = false;
            // 
            // textbox_clave
            // 
            textbox_clave.AnimateReadOnly = false;
            textbox_clave.BackgroundImageLayout = ImageLayout.None;
            textbox_clave.CharacterCasing = CharacterCasing.Normal;
            textbox_clave.Depth = 0;
            textbox_clave.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            textbox_clave.HideSelection = true;
            textbox_clave.LeadingIcon = null;
            textbox_clave.Location = new Point(262, 142);
            textbox_clave.MaxLength = 32767;
            textbox_clave.MouseState = MaterialSkin.MouseState.OUT;
            textbox_clave.Name = "textbox_clave";
            textbox_clave.PasswordChar = '*';
            textbox_clave.PrefixSuffixText = null;
            textbox_clave.ReadOnly = false;
            textbox_clave.RightToLeft = RightToLeft.No;
            textbox_clave.SelectedText = "";
            textbox_clave.SelectionLength = 0;
            textbox_clave.SelectionStart = 0;
            textbox_clave.ShortcutsEnabled = true;
            textbox_clave.Size = new Size(207, 48);
            textbox_clave.TabIndex = 7;
            textbox_clave.TabStop = false;
            textbox_clave.TextAlign = HorizontalAlignment.Left;
            textbox_clave.TrailingIcon = null;
            textbox_clave.UseSystemPasswordChar = false;
            // 
            // login
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(589, 260);
            Controls.Add(textbox_clave);
            Controls.Add(textbox_dni);
            Controls.Add(btn_cancelar);
            Controls.Add(label_pass);
            Controls.Add(pictureBox1);
            Controls.Add(btn_ingresar);
            Controls.Add(label_dni);
            Margin = new Padding(3, 4, 3, 4);
            Name = "login";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "        Login";
            Load += login_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MaterialSkin.Controls.MaterialLabel label_dni;
        private MaterialSkin.Controls.MaterialButton btn_ingresar;
        private PictureBox pictureBox1;
        private MaterialSkin.Controls.MaterialLabel label_pass;
        private MaterialSkin.Controls.MaterialButton btn_cancelar;
        private MaterialSkin.Controls.MaterialTextBox2 textbox_dni;
        private MaterialSkin.Controls.MaterialTextBox2 textbox_clave;
    }
}