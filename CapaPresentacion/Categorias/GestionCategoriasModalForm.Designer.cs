namespace CapaPresentacion
{
    partial class GestionCategoriasModalForm
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
            SuspendLayout();
            //
            // GestionCategoriasModalForm
            //
            // Nota: el control GestionCategoriasControl no se crea aquí porque su
            // constructor exige CategoriaNegocio; se instancia y se aloja a
            // Dock = Fill en el constructor del formulario (D-5 del plan).
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1120, 620);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "GestionCategoriasModalForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Gestión de categorías";
            ResumeLayout(false);
        }

        #endregion
    }
}
