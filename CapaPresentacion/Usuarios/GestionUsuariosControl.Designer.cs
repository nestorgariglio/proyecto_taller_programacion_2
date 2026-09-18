namespace CapaPresentacion
{
    partial class GestionUsuariosControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Releases resources used by the control.
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support; do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            labelTitulo = new MaterialSkin.Controls.MaterialLabel();
            panelFiltros = new TableLayoutPanel();
            txtBuscar = new MaterialSkin.Controls.MaterialTextBox2();
            comboEstado = new MaterialSkin.Controls.MaterialComboBox();
            panelAcciones = new FlowLayoutPanel();
            btnNuevo = new MaterialSkin.Controls.MaterialButton();
            btnEditar = new MaterialSkin.Controls.MaterialButton();
            btnDesactivar = new MaterialSkin.Controls.MaterialButton();
            btnActualizar = new MaterialSkin.Controls.MaterialButton();
            gridUsuarios = new DataGridView();
            colDni = new DataGridViewTextBoxColumn();
            colNombre = new DataGridViewTextBoxColumn();
            colApellido = new DataGridViewTextBoxColumn();
            colSexo = new DataGridViewTextBoxColumn();
            colCorreo = new DataGridViewTextBoxColumn();
            colRol = new DataGridViewTextBoxColumn();
            colEstado = new DataGridViewTextBoxColumn();
            labelCantidad = new MaterialSkin.Controls.MaterialLabel();
            ((System.ComponentModel.ISupportInitialize)gridUsuarios).BeginInit();
            panelFiltros.SuspendLayout();
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
            labelTitulo.Size = new Size(210, 24);
            labelTitulo.TabIndex = 0;
            labelTitulo.Text = "Gestión de usuarios";
            //
            // panelFiltros
            //
            panelFiltros.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelFiltros.ColumnCount = 3;
            panelFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            panelFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            panelFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            panelFiltros.Controls.Add(txtBuscar, 0, 0);
            panelFiltros.Controls.Add(comboEstado, 1, 0);
            panelFiltros.Controls.Add(panelAcciones, 2, 0);
            panelFiltros.Location = new Point(24, 55);
            panelFiltros.Name = "panelFiltros";
            panelFiltros.RowCount = 1;
            panelFiltros.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            panelFiltros.Size = new Size(1029, 48);
            panelFiltros.TabIndex = 1;
            //
            // txtBuscar
            //
            txtBuscar.AnimateReadOnly = false;
            txtBuscar.BackgroundImageLayout = ImageLayout.None;
            txtBuscar.CharacterCasing = CharacterCasing.Normal;
            txtBuscar.Depth = 0;
            txtBuscar.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtBuscar.HideSelection = true;
            txtBuscar.Hint = "Buscar por DNI, nombre o correo";
            txtBuscar.LeadingIcon = null;
            txtBuscar.Margin = new Padding(0, 0, 8, 0);
            txtBuscar.MaxLength = 32767;
            txtBuscar.MouseState = MaterialSkin.MouseState.OUT;
            txtBuscar.Name = "txtBuscar";
            txtBuscar.PasswordChar = '\0';
            txtBuscar.PrefixSuffixText = null;
            txtBuscar.ReadOnly = false;
            txtBuscar.RightToLeft = RightToLeft.No;
            txtBuscar.SelectedText = "";
            txtBuscar.SelectionLength = 0;
            txtBuscar.SelectionStart = 0;
            txtBuscar.ShortcutsEnabled = true;
            txtBuscar.Size = new Size(558, 48);
            txtBuscar.Dock = DockStyle.Fill;
            txtBuscar.TabIndex = 2;
            txtBuscar.TabStop = false;
            txtBuscar.TextAlign = HorizontalAlignment.Left;
            txtBuscar.TrailingIcon = null;
            txtBuscar.UseSystemPasswordChar = false;
            //
            // comboEstado
            //
            comboEstado.AutoResize = false;
            comboEstado.BackColor = Color.FromArgb(255, 255, 255);
            comboEstado.Depth = 0;
            comboEstado.DrawMode = DrawMode.OwnerDrawVariable;
            comboEstado.DropDownHeight = 174;
            comboEstado.DropDownStyle = ComboBoxStyle.DropDownList;
            comboEstado.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold, GraphicsUnit.Pixel);
            comboEstado.FormattingEnabled = true;
            comboEstado.IntegralHeight = false;
            comboEstado.ItemHeight = 43;
            comboEstado.Margin = new Padding(8, 0, 8, 0);
            comboEstado.MaxDropDownItems = 4;
            comboEstado.MouseState = MaterialSkin.MouseState.OUT;
            comboEstado.Name = "comboEstado";
            comboEstado.Size = new Size(241, 48);
            comboEstado.Dock = DockStyle.Fill;
            comboEstado.StartIndex = 0;
            comboEstado.TabIndex = 3;
            //
            // panelAcciones
            //
            panelAcciones.AutoScroll = true;
            panelAcciones.Controls.Add(btnNuevo);
            panelAcciones.Controls.Add(btnEditar);
            panelAcciones.Controls.Add(btnDesactivar);
            panelAcciones.Controls.Add(btnActualizar);
            panelAcciones.Dock = DockStyle.Fill;
            panelAcciones.FlowDirection = FlowDirection.LeftToRight;
            panelAcciones.Location = new Point(474, 3);
            panelAcciones.Margin = new Padding(0);
            panelAcciones.Name = "panelAcciones";
            panelAcciones.Padding = new Padding(0);
            panelAcciones.TabIndex = 4;
            panelAcciones.WrapContents = false;
            //
            // btnNuevo
            //
            btnNuevo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnNuevo.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnNuevo.Depth = 0;
            btnNuevo.HighEmphasis = true;
            btnNuevo.Icon = null;
            btnNuevo.Margin = new Padding(4, 6, 4, 6);
            btnNuevo.MouseState = MaterialSkin.MouseState.HOVER;
            btnNuevo.Name = "btnNuevo";
            btnNuevo.NoAccentTextColor = Color.Empty;
            btnNuevo.Size = new Size(90, 36);
            btnNuevo.TabIndex = 5;
            btnNuevo.Text = "Nuevo";
            btnNuevo.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnNuevo.UseAccentColor = false;
            btnNuevo.UseVisualStyleBackColor = true;
            //
            // btnEditar
            //
            btnEditar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnEditar.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnEditar.Depth = 0;
            btnEditar.Enabled = false;
            btnEditar.HighEmphasis = true;
            btnEditar.Icon = null;
            btnEditar.Margin = new Padding(4, 6, 4, 6);
            btnEditar.MouseState = MaterialSkin.MouseState.HOVER;
            btnEditar.Name = "btnEditar";
            btnEditar.NoAccentTextColor = Color.Empty;
            btnEditar.Size = new Size(90, 36);
            btnEditar.TabIndex = 6;
            btnEditar.Text = "Editar";
            btnEditar.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnEditar.UseAccentColor = false;
            btnEditar.UseVisualStyleBackColor = true;
            //
            // btnDesactivar
            //
            btnDesactivar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnDesactivar.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnDesactivar.Depth = 0;
            btnDesactivar.Enabled = false;
            btnDesactivar.HighEmphasis = true;
            btnDesactivar.Icon = null;
            btnDesactivar.Margin = new Padding(4, 6, 4, 6);
            btnDesactivar.MouseState = MaterialSkin.MouseState.HOVER;
            btnDesactivar.Name = "btnDesactivar";
            btnDesactivar.NoAccentTextColor = Color.Empty;
            btnDesactivar.Size = new Size(112, 36);
            btnDesactivar.TabIndex = 7;
            btnDesactivar.Text = "Desactivar";
            btnDesactivar.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnDesactivar.UseAccentColor = false;
            btnDesactivar.UseVisualStyleBackColor = true;
            //
            // btnActualizar
            //
            btnActualizar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnActualizar.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnActualizar.Depth = 0;
            btnActualizar.HighEmphasis = true;
            btnActualizar.Icon = null;
            btnActualizar.Margin = new Padding(4, 6, 0, 6);
            btnActualizar.MouseState = MaterialSkin.MouseState.HOVER;
            btnActualizar.Name = "btnActualizar";
            btnActualizar.NoAccentTextColor = Color.Empty;
            btnActualizar.Size = new Size(110, 36);
            btnActualizar.TabIndex = 8;
            btnActualizar.Text = "Actualizar";
            btnActualizar.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnActualizar.UseAccentColor = false;
            btnActualizar.UseVisualStyleBackColor = true;
            //
            // gridUsuarios
            //
            gridUsuarios.AllowUserToAddRows = false;
            gridUsuarios.AllowUserToDeleteRows = false;
            gridUsuarios.AllowUserToResizeRows = false;
            gridUsuarios.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridUsuarios.AutoGenerateColumns = false;
            gridUsuarios.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridUsuarios.BackgroundColor = Color.FromArgb(48, 48, 48);
            gridUsuarios.BorderStyle = BorderStyle.None;
            gridUsuarios.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            gridUsuarios.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            gridUsuarios.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(55, 71, 79),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                SelectionBackColor = Color.FromArgb(55, 71, 79),
                SelectionForeColor = Color.White,
                WrapMode = DataGridViewTriState.True
            };
            gridUsuarios.ColumnHeadersHeight = 42;
            gridUsuarios.Columns.AddRange(new DataGridViewColumn[]
            {
                colDni,
                colNombre,
                colApellido,
                colSexo,
                colCorreo,
                colRol,
                colEstado
            });
            gridUsuarios.DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(48, 48, 48),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.White,
                SelectionBackColor = Color.FromArgb(255, 87, 34),
                SelectionForeColor = Color.White,
                WrapMode = DataGridViewTriState.False
            };
            gridUsuarios.EnableHeadersVisualStyles = false;
            gridUsuarios.GridColor = Color.FromArgb(80, 80, 80);
            gridUsuarios.Location = new Point(24, 125);
            gridUsuarios.MultiSelect = false;
            gridUsuarios.Name = "gridUsuarios";
            gridUsuarios.ReadOnly = true;
            gridUsuarios.RowHeadersVisible = false;
            gridUsuarios.RowTemplate.Height = 36;
            gridUsuarios.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridUsuarios.Size = new Size(1029, 350);
            gridUsuarios.TabIndex = 9;
            gridUsuarios.CellFormatting += gridUsuarios_CellFormatting;
            //
            // colDni
            //
            colDni.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colDni.DataPropertyName = "Dni";
            colDni.FillWeight = 70F;
            colDni.HeaderText = "DNI";
            colDni.Name = "colDni";
            colDni.ReadOnly = true;
            //
            // colNombre
            //
            colNombre.DataPropertyName = "Nombre";
            colNombre.FillWeight = 100F;
            colNombre.HeaderText = "Nombre";
            colNombre.Name = "colNombre";
            colNombre.ReadOnly = true;
            //
            // colApellido
            //
            colApellido.DataPropertyName = "Apellido";
            colApellido.FillWeight = 100F;
            colApellido.HeaderText = "Apellido";
            colApellido.Name = "colApellido";
            colApellido.ReadOnly = true;
            //
            // colSexo
            //
            colSexo.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colSexo.DataPropertyName = "Sexo";
            colSexo.FillWeight = 50F;
            colSexo.HeaderText = "Sexo";
            colSexo.Name = "colSexo";
            colSexo.ReadOnly = true;
            //
            // colCorreo
            //
            colCorreo.DataPropertyName = "Correo";
            colCorreo.FillWeight = 130F;
            colCorreo.HeaderText = "Correo";
            colCorreo.Name = "colCorreo";
            colCorreo.ReadOnly = true;
            //
            // colRol
            //
            colRol.DataPropertyName = "RolDescripcion";
            colRol.FillWeight = 100F;
            colRol.HeaderText = "Rol";
            colRol.Name = "colRol";
            colRol.ReadOnly = true;
            //
            // colEstado
            //
            colEstado.DataPropertyName = "Estado";
            colEstado.FillWeight = 75F;
            colEstado.HeaderText = "Estado";
            colEstado.Name = "colEstado";
            colEstado.ReadOnly = true;
            //
            // labelCantidad
            //
            labelCantidad.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            labelCantidad.AutoSize = true;
            labelCantidad.Depth = 0;
            labelCantidad.Font = new Font("Roboto", 14F, FontStyle.Regular, GraphicsUnit.Pixel);
            labelCantidad.Location = new Point(24, 490);
            labelCantidad.MouseState = MaterialSkin.MouseState.HOVER;
            labelCantidad.Name = "labelCantidad";
            labelCantidad.Size = new Size(102, 19);
            labelCantidad.TabIndex = 10;
            labelCantidad.Text = "0 usuario(s)";
            //
            // GestionUsuariosControl
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(labelCantidad);
            Controls.Add(gridUsuarios);
            Controls.Add(panelFiltros);
            Controls.Add(labelTitulo);
            Name = "GestionUsuariosControl";
            Size = new Size(1077, 538);
            ((System.ComponentModel.ISupportInitialize)gridUsuarios).EndInit();
            panelFiltros.ResumeLayout(false);
            panelFiltros.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MaterialSkin.Controls.MaterialLabel labelTitulo;
        private TableLayoutPanel panelFiltros;
        private MaterialSkin.Controls.MaterialTextBox2 txtBuscar;
        private MaterialSkin.Controls.MaterialComboBox comboEstado;
        private FlowLayoutPanel panelAcciones;
        private MaterialSkin.Controls.MaterialButton btnNuevo;
        private MaterialSkin.Controls.MaterialButton btnEditar;
        private MaterialSkin.Controls.MaterialButton btnDesactivar;
        private MaterialSkin.Controls.MaterialButton btnActualizar;
        private DataGridView gridUsuarios;
        private DataGridViewTextBoxColumn colDni;
        private DataGridViewTextBoxColumn colNombre;
        private DataGridViewTextBoxColumn colApellido;
        private DataGridViewTextBoxColumn colSexo;
        private DataGridViewTextBoxColumn colCorreo;
        private DataGridViewTextBoxColumn colRol;
        private DataGridViewTextBoxColumn colEstado;
        private MaterialSkin.Controls.MaterialLabel labelCantidad;
    }
}
