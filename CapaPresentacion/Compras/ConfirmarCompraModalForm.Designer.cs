namespace CapaPresentacion.Compras
{
    partial class ConfirmarCompraModalForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlContenedor = new Panel();
            tblLayout = new TableLayoutPanel();
            lblTituloModal = new MaterialSkin.Controls.MaterialLabel();
            lblInfoProveedor = new MaterialSkin.Controls.MaterialLabel();
            lblInstruccion = new MaterialSkin.Controls.MaterialLabel();
            gridLineas = new DataGridView();
            txtObservaciones = new MaterialSkin.Controls.MaterialTextBox2();
            lblMensajeError = new Label();
            pnlPie = new TableLayoutPanel();
            lblTotalAcordado = new MaterialSkin.Controls.MaterialLabel();
            pnlBotones = new FlowLayoutPanel();
            btnCancelar = new MaterialSkin.Controls.MaterialButton();
            btnConfirmar = new MaterialSkin.Controls.MaterialButton();
            pnlContenedor.SuspendLayout();
            tblLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridLineas).BeginInit();
            pnlPie.SuspendLayout();
            pnlBotones.SuspendLayout();
            SuspendLayout();
            // 
            // pnlContenedor
            // 
            pnlContenedor.BackColor = Color.FromArgb(48, 48, 48);
            pnlContenedor.Controls.Add(tblLayout);
            pnlContenedor.Dock = DockStyle.Fill;
            pnlContenedor.Location = new Point(3, 64);
            pnlContenedor.Name = "pnlContenedor";
            pnlContenedor.Padding = new Padding(16);
            pnlContenedor.Size = new Size(714, 453);
            pnlContenedor.TabIndex = 0;
            // 
            // tblLayout
            // 
            tblLayout.ColumnCount = 1;
            tblLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblLayout.Controls.Add(lblTituloModal, 0, 0);
            tblLayout.Controls.Add(lblInfoProveedor, 0, 1);
            tblLayout.Controls.Add(lblInstruccion, 0, 2);
            tblLayout.Controls.Add(gridLineas, 0, 3);
            tblLayout.Controls.Add(txtObservaciones, 0, 4);
            tblLayout.Controls.Add(lblMensajeError, 0, 5);
            tblLayout.Controls.Add(pnlPie, 0, 6);
            tblLayout.Dock = DockStyle.Fill;
            tblLayout.Location = new Point(16, 16);
            tblLayout.Name = "tblLayout";
            tblLayout.RowCount = 7;
            tblLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tblLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            tblLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            tblLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            tblLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            tblLayout.Size = new Size(682, 421);
            tblLayout.TabIndex = 0;
            // 
            // lblTituloModal
            // 
            lblTituloModal.AutoSize = true;
            lblTituloModal.Depth = 0;
            lblTituloModal.Dock = DockStyle.Fill;
            lblTituloModal.Font = new Font("Roboto Medium", 14F, FontStyle.Bold, GraphicsUnit.Pixel);
            lblTituloModal.ForeColor = Color.White;
            lblTituloModal.Location = new Point(0, 0);
            lblTituloModal.Margin = new Padding(0);
            lblTituloModal.MouseState = MaterialSkin.MouseState.HOVER;
            lblTituloModal.Name = "lblTituloModal";
            lblTituloModal.Size = new Size(682, 28);
            lblTituloModal.TabIndex = 0;
            lblTituloModal.Text = "Confirmación de costos acordados";
            lblTituloModal.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblInfoProveedor
            // 
            lblInfoProveedor.AutoSize = true;
            lblInfoProveedor.Depth = 0;
            lblInfoProveedor.Dock = DockStyle.Fill;
            lblInfoProveedor.Font = new Font("Roboto", 12F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblInfoProveedor.ForeColor = Color.FromArgb(200, 200, 200);
            lblInfoProveedor.Location = new Point(0, 28);
            lblInfoProveedor.Margin = new Padding(0);
            lblInfoProveedor.MouseState = MaterialSkin.MouseState.HOVER;
            lblInfoProveedor.Name = "lblInfoProveedor";
            lblInfoProveedor.Size = new Size(682, 24);
            lblInfoProveedor.TabIndex = 1;
            lblInfoProveedor.Text = "Solicitud N° — | Proveedor: —";
            lblInfoProveedor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblInstruccion
            // 
            lblInstruccion.AutoSize = true;
            lblInstruccion.Depth = 0;
            lblInstruccion.Dock = DockStyle.Fill;
            lblInstruccion.Font = new Font("Roboto", 11F, FontStyle.Italic, GraphicsUnit.Pixel);
            lblInstruccion.ForeColor = Color.FromArgb(170, 170, 170);
            lblInstruccion.Location = new Point(0, 52);
            lblInstruccion.Margin = new Padding(0);
            lblInstruccion.MouseState = MaterialSkin.MouseState.HOVER;
            lblInstruccion.Name = "lblInstruccion";
            lblInstruccion.Size = new Size(682, 22);
            lblInstruccion.TabIndex = 2;
            lblInstruccion.Text = "Ingrese en la columna 'Costo unitario' el monto por unidad acordado con el proveedor.";
            lblInstruccion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // gridLineas
            // 
            gridLineas.AllowUserToAddRows = false;
            gridLineas.AllowUserToDeleteRows = false;
            gridLineas.AllowUserToResizeRows = false;
            gridLineas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridLineas.ColumnHeadersHeight = 30;
            gridLineas.Dock = DockStyle.Fill;
            gridLineas.Location = new Point(0, 78);
            gridLineas.Margin = new Padding(0, 4, 0, 4);
            gridLineas.MultiSelect = false;
            gridLineas.Name = "gridLineas";
            gridLineas.RowHeadersVisible = false;
            gridLineas.RowTemplate.Height = 28;
            gridLineas.SelectionMode = DataGridViewSelectionMode.CellSelect;
            gridLineas.Size = new Size(682, 147);
            gridLineas.TabIndex = 3;
            // 
            // txtObservaciones
            // 
            txtObservaciones.AnimateReadOnly = false;
            txtObservaciones.BackgroundImageLayout = ImageLayout.None;
            txtObservaciones.CharacterCasing = CharacterCasing.Normal;
            txtObservaciones.Depth = 0;
            txtObservaciones.Dock = DockStyle.Fill;
            txtObservaciones.Font = new Font("Roboto", 12F, FontStyle.Regular, GraphicsUnit.Pixel);
            txtObservaciones.Hint = "Observaciones de confirmación (ej: presupuesto o remito del proveedor)";
            txtObservaciones.Location = new Point(0, 233);
            txtObservaciones.Margin = new Padding(0, 4, 0, 4);
            txtObservaciones.MaxLength = 200;
            txtObservaciones.MouseState = MaterialSkin.MouseState.OUT;
            txtObservaciones.Name = "txtObservaciones";
            txtObservaciones.PasswordChar = '\0';
            txtObservaciones.PrefixSuffixText = null;
            txtObservaciones.ReadOnly = false;
            txtObservaciones.RightToLeft = RightToLeft.No;
            txtObservaciones.SelectedText = "";
            txtObservaciones.SelectionLength = 0;
            txtObservaciones.SelectionStart = 0;
            txtObservaciones.ShortcutsEnabled = true;
            txtObservaciones.Size = new Size(682, 44);
            txtObservaciones.TabIndex = 4;
            txtObservaciones.TabStop = false;
            txtObservaciones.TextAlign = HorizontalAlignment.Left;
            txtObservaciones.TrailingIcon = null;
            txtObservaciones.UseSystemPasswordChar = false;
            // 
            // lblMensajeError
            // 
            lblMensajeError.AutoSize = true;
            lblMensajeError.Dock = DockStyle.Fill;
            lblMensajeError.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblMensajeError.ForeColor = Color.FromArgb(244, 67, 54);
            lblMensajeError.Location = new Point(0, 281);
            lblMensajeError.Margin = new Padding(0);
            lblMensajeError.Name = "lblMensajeError";
            lblMensajeError.Size = new Size(682, 22);
            lblMensajeError.TabIndex = 5;
            lblMensajeError.Text = "Mensaje de error";
            lblMensajeError.TextAlign = ContentAlignment.MiddleLeft;
            lblMensajeError.Visible = false;
            // 
            // pnlPie
            // 
            pnlPie.ColumnCount = 2;
            pnlPie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlPie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlPie.Controls.Add(lblTotalAcordado, 0, 0);
            pnlPie.Controls.Add(pnlBotones, 1, 0);
            pnlPie.Dock = DockStyle.Fill;
            pnlPie.Location = new Point(0, 303);
            pnlPie.Margin = new Padding(0);
            pnlPie.Name = "pnlPie";
            pnlPie.RowCount = 1;
            pnlPie.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            pnlPie.Size = new Size(682, 44);
            pnlPie.TabIndex = 6;
            // 
            // lblTotalAcordado
            // 
            lblTotalAcordado.AutoSize = true;
            lblTotalAcordado.Depth = 0;
            lblTotalAcordado.Dock = DockStyle.Fill;
            lblTotalAcordado.Font = new Font("Roboto", 14F, FontStyle.Bold, GraphicsUnit.Pixel);
            lblTotalAcordado.ForeColor = Color.White;
            lblTotalAcordado.Location = new Point(0, 0);
            lblTotalAcordado.Margin = new Padding(0);
            lblTotalAcordado.MouseState = MaterialSkin.MouseState.HOVER;
            lblTotalAcordado.Name = "lblTotalAcordado";
            lblTotalAcordado.Size = new Size(341, 44);
            lblTotalAcordado.TabIndex = 0;
            lblTotalAcordado.Text = "Total acordado: $ 0,00";
            lblTotalAcordado.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlBotones
            // 
            pnlBotones.Controls.Add(btnConfirmar);
            pnlBotones.Controls.Add(btnCancelar);
            pnlBotones.Dock = DockStyle.Fill;
            pnlBotones.FlowDirection = FlowDirection.RightToLeft;
            pnlBotones.Location = new Point(341, 0);
            pnlBotones.Margin = new Padding(0);
            pnlBotones.Name = "pnlBotones";
            pnlBotones.Size = new Size(341, 44);
            pnlBotones.TabIndex = 1;
            // 
            // btnCancelar
            // 
            btnCancelar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnCancelar.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnCancelar.Depth = 0;
            btnCancelar.HighEmphasis = false;
            btnCancelar.Icon = null;
            btnCancelar.Location = new Point(130, 4);
            btnCancelar.Margin = new Padding(4);
            btnCancelar.MouseState = MaterialSkin.MouseState.HOVER;
            btnCancelar.Name = "btnCancelar";
            btnCancelar.NoAccentTextColor = Color.FromArgb(224, 224, 224);
            btnCancelar.Size = new Size(96, 36);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "CANCELAR";
            btnCancelar.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            btnCancelar.UseAccentColor = false;
            btnCancelar.UseVisualStyleBackColor = true;
            // 
            // btnConfirmar
            // 
            btnConfirmar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnConfirmar.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            btnConfirmar.Depth = 0;
            btnConfirmar.HighEmphasis = true;
            btnConfirmar.Icon = null;
            btnConfirmar.Location = new Point(234, 4);
            btnConfirmar.Margin = new Padding(4);
            btnConfirmar.MouseState = MaterialSkin.MouseState.HOVER;
            btnConfirmar.Name = "btnConfirmar";
            btnConfirmar.NoAccentTextColor = Color.Empty;
            btnConfirmar.Size = new Size(103, 36);
            btnConfirmar.TabIndex = 0;
            btnConfirmar.Text = "CONFIRMAR";
            btnConfirmar.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            btnConfirmar.UseAccentColor = false;
            btnConfirmar.UseVisualStyleBackColor = true;
            // 
            // ConfirmarCompraModalForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(720, 520);
            Controls.Add(pnlContenedor);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ConfirmarCompraModalForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Confirmar Solicitud de Compra";
            pnlContenedor.ResumeLayout(false);
            tblLayout.ResumeLayout(false);
            tblLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)gridLineas).EndInit();
            pnlPie.ResumeLayout(false);
            pnlPie.PerformLayout();
            pnlBotones.ResumeLayout(false);
            pnlBotones.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlContenedor;
        private TableLayoutPanel tblLayout;
        private MaterialSkin.Controls.MaterialLabel lblTituloModal;
        private MaterialSkin.Controls.MaterialLabel lblInfoProveedor;
        private MaterialSkin.Controls.MaterialLabel lblInstruccion;
        private DataGridView gridLineas;
        private MaterialSkin.Controls.MaterialTextBox2 txtObservaciones;
        private Label lblMensajeError;
        private TableLayoutPanel pnlPie;
        private MaterialSkin.Controls.MaterialLabel lblTotalAcordado;
        private FlowLayoutPanel pnlBotones;
        private MaterialSkin.Controls.MaterialButton btnConfirmar;
        private MaterialSkin.Controls.MaterialButton btnCancelar;
    }
}
