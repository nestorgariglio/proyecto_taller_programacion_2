namespace CapaPresentacion
{
    partial class GestionCategoriasControl
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
        /// Required method for Designer support - do not modify
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
            btnDarDebaja = new MaterialSkin.Controls.MaterialButton();
            btnReactivar = new MaterialSkin.Controls.MaterialButton();
            btnActualizar = new MaterialSkin.Controls.MaterialButton();
            gridCategorias = new DataGridView();
            colDescripcion = new DataGridViewTextBoxColumn();
            colEstado = new DataGridViewTextBoxColumn();
            colFechaRegistro = new DataGridViewTextBoxColumn();
            labelCantidad = new MaterialSkin.Controls.MaterialLabel();
            ((System.ComponentModel.ISupportInitialize)gridCategorias).BeginInit();
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
            labelTitulo.Size = new Size(222, 24);
            labelTitulo.TabIndex = 0;
            labelTitulo.Text = "Gestión de categorías";
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
            panelFiltros.Controls.Add(panelAcciones, 0, 1);
            panelFiltros.Location = new Point(24, 55);
            panelFiltros.Name = "panelFiltros";
            panelFiltros.RowCount = 2;
            panelFiltros.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            panelFiltros.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            panelFiltros.Size = new Size(1029, 104);
            panelFiltros.TabIndex = 1;
            panelFiltros.SetColumnSpan(panelAcciones, 3);
            //
            // txtBuscar
            //
            txtBuscar.AnimateReadOnly = false;
            txtBuscar.BackgroundImageLayout = ImageLayout.None;
            txtBuscar.CharacterCasing = CharacterCasing.Normal;
            txtBuscar.Depth = 0;
            txtBuscar.Dock = DockStyle.Fill;
            txtBuscar.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtBuscar.HideSelection = true;
            txtBuscar.Hint = "Buscar por descripción";
            txtBuscar.LeadingIcon = null;
            txtBuscar.Margin = new Padding(0, 0, 8, 8);
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
            txtBuscar.Size = new Size(455, 48);
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
            comboEstado.Dock = DockStyle.Fill;
            comboEstado.DrawMode = DrawMode.OwnerDrawVariable;
            comboEstado.DropDownHeight = 174;
            comboEstado.DropDownStyle = ComboBoxStyle.DropDownList;
            comboEstado.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold, GraphicsUnit.Pixel);
            comboEstado.FormattingEnabled = true;
            comboEstado.IntegralHeight = false;
            comboEstado.ItemHeight = 43;
            comboEstado.Margin = new Padding(8, 0, 0, 8);
            comboEstado.MaxDropDownItems = 4;
            comboEstado.MouseState = MaterialSkin.MouseState.OUT;
            comboEstado.Name = "comboEstado";
            comboEstado.Size = new Size(198, 48);
            comboEstado.StartIndex = 0;
            comboEstado.TabIndex = 3;
            //
            // panelAcciones
            //
            panelAcciones.AutoScroll = true;
            panelAcciones.Controls.Add(btnNuevo);
            panelAcciones.Controls.Add(btnEditar);
            panelAcciones.Controls.Add(btnDarDebaja);
            panelAcciones.Controls.Add(btnReactivar);
            panelAcciones.Controls.Add(btnActualizar);
            panelAcciones.Dock = DockStyle.Fill;
            panelAcciones.FlowDirection = FlowDirection.LeftToRight;
            panelAcciones.Location = new Point(0, 56);
            panelAcciones.Margin = new Padding(0);
            panelAcciones.Name = "panelAcciones";
            panelAcciones.Padding = new Padding(0);
            panelAcciones.Size = new Size(1029, 48);
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
            // btnDarDebaja
            //
            btnDarDebaja.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnDarDebaja.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnDarDebaja.Depth = 0;
            btnDarDebaja.Enabled = false;
            btnDarDebaja.HighEmphasis = true;
            btnDarDebaja.Icon = null;
            btnDarDebaja.Margin = new Padding(4, 6, 4, 6);
            btnDarDebaja.MouseState = MaterialSkin.MouseState.HOVER;
            btnDarDebaja.Name = "btnDarDebaja";
            btnDarDebaja.NoAccentTextColor = Color.Empty;
            btnDarDebaja.Size = new Size(118, 36);
            btnDarDebaja.TabIndex = 7;
            btnDarDebaja.Text = "Dar de baja";
            btnDarDebaja.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnDarDebaja.UseAccentColor = false;
            btnDarDebaja.UseVisualStyleBackColor = true;
            //
            // btnReactivar
            //
            btnReactivar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnReactivar.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnReactivar.Depth = 0;
            btnReactivar.Enabled = false;
            btnReactivar.HighEmphasis = true;
            btnReactivar.Icon = null;
            btnReactivar.Margin = new Padding(4, 6, 4, 6);
            btnReactivar.MouseState = MaterialSkin.MouseState.HOVER;
            btnReactivar.Name = "btnReactivar";
            btnReactivar.NoAccentTextColor = Color.Empty;
            btnReactivar.Size = new Size(110, 36);
            btnReactivar.TabIndex = 8;
            btnReactivar.Text = "Reactivar";
            btnReactivar.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnReactivar.UseAccentColor = false;
            btnReactivar.UseVisualStyleBackColor = true;
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
            btnActualizar.TabIndex = 9;
            btnActualizar.Text = "Actualizar";
            btnActualizar.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnActualizar.UseAccentColor = false;
            btnActualizar.UseVisualStyleBackColor = true;
            //
            // gridCategorias
            //
            gridCategorias.AllowUserToAddRows = false;
            gridCategorias.AllowUserToDeleteRows = false;
            gridCategorias.AllowUserToResizeRows = false;
            gridCategorias.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridCategorias.AutoGenerateColumns = false;
            gridCategorias.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridCategorias.BackgroundColor = Color.FromArgb(48, 48, 48);
            gridCategorias.BorderStyle = BorderStyle.None;
            gridCategorias.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            gridCategorias.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            gridCategorias.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(55, 71, 79),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                SelectionBackColor = Color.FromArgb(55, 71, 79),
                SelectionForeColor = Color.White,
                WrapMode = DataGridViewTriState.True
            };
            gridCategorias.ColumnHeadersHeight = 42;
            gridCategorias.Columns.AddRange(new DataGridViewColumn[]
            {
                colDescripcion,
                colEstado,
                colFechaRegistro
            });
            gridCategorias.DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(48, 48, 48),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.White,
                SelectionBackColor = Color.FromArgb(255, 87, 34),
                SelectionForeColor = Color.White,
                WrapMode = DataGridViewTriState.False
            };
            gridCategorias.EnableHeadersVisualStyles = false;
            gridCategorias.GridColor = Color.FromArgb(80, 80, 80);
            gridCategorias.Location = new Point(24, 181);
            gridCategorias.MultiSelect = false;
            gridCategorias.Name = "gridCategorias";
            gridCategorias.ReadOnly = true;
            gridCategorias.RowHeadersVisible = false;
            gridCategorias.RowTemplate.Height = 36;
            gridCategorias.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridCategorias.Size = new Size(1029, 294);
            gridCategorias.TabIndex = 10;
            gridCategorias.CellFormatting += gridCategorias_CellFormatting;
            //
            // colDescripcion
            //
            colDescripcion.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colDescripcion.DataPropertyName = "Descripcion";
            colDescripcion.FillWeight = 200F;
            colDescripcion.HeaderText = "Descripción";
            colDescripcion.Name = "colDescripcion";
            colDescripcion.ReadOnly = true;
            //
            // colEstado
            //
            colEstado.DataPropertyName = "Estado";
            colEstado.FillWeight = 60F;
            colEstado.HeaderText = "Estado";
            colEstado.Name = "colEstado";
            colEstado.ReadOnly = true;
            //
            // colFechaRegistro
            //
            colFechaRegistro.DataPropertyName = "FechaRegistro";
            colFechaRegistro.FillWeight = 90F;
            colFechaRegistro.HeaderText = "Fecha de registro";
            colFechaRegistro.Name = "colFechaRegistro";
            colFechaRegistro.ReadOnly = true;
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
            labelCantidad.Size = new Size(112, 19);
            labelCantidad.TabIndex = 11;
            labelCantidad.Text = "0 categoría(s)";
            //
            // GestionCategoriasControl
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(labelCantidad);
            Controls.Add(gridCategorias);
            Controls.Add(panelFiltros);
            Controls.Add(labelTitulo);
            Name = "GestionCategoriasControl";
            Size = new Size(1077, 538);
            ((System.ComponentModel.ISupportInitialize)gridCategorias).EndInit();
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
        private MaterialSkin.Controls.MaterialButton btnDarDebaja;
        private MaterialSkin.Controls.MaterialButton btnReactivar;
        private MaterialSkin.Controls.MaterialButton btnActualizar;
        private DataGridView gridCategorias;
        private DataGridViewTextBoxColumn colDescripcion;
        private DataGridViewTextBoxColumn colEstado;
        private DataGridViewTextBoxColumn colFechaRegistro;
        private MaterialSkin.Controls.MaterialLabel labelCantidad;
    }
}
