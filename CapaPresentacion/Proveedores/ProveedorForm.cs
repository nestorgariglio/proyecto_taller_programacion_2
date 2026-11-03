using System;
using System.Drawing;
using System.Windows.Forms;
using CapaNegocio;
using CapaNegocio.DTOs.Proveedores;
using CapaNegocio.Enums;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CapaPresentacion.Proveedores
{
    public partial class ProveedorForm : MaterialForm
    {
        private readonly ProveedorNegocio _negocio;
        private readonly ProveedorRespuestaDto? _editar;
        private bool _guardando;

        private MaterialTextBox2 txtCuit = null!;
        private MaterialTextBox2 txtRazonSocial = null!;
        private MaterialTextBox2 txtCorreo = null!;
        private MaterialTextBox2 txtTelefono = null!;
        private MaterialButton btnGuardar = null!;
        private MaterialButton btnCancelar = null!;
        private MaterialLabel lblTitulo = null!;

        public ProveedorForm(ProveedorNegocio negocio, ProveedorRespuestaDto? editar = null)
        {
            ArgumentNullException.ThrowIfNull(negocio);
            _negocio = negocio;
            _editar = editar;
            InitializeComponent();
            PrepararDatos();
            btnGuardar.Click += btnGuardar_Click;
            btnCancelar.Click += btnCancelar_Click;
            MaterialSkinManager.Instance.AddFormToManage(this);
        }

        private void InitializeComponent()
        {
            ClientSize = new Size(720, 380);
            Text = _editar is null ? "Registrar proveedor" : "Editar proveedor";
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(30, 30, 30);

            lblTitulo = new MaterialLabel
            {
                Text = _editar is null ? "Registrar proveedor" : "Editar proveedor",
                FontType = MaterialSkinManager.fontType.H6,
                Location = new Point(24, 18),
                AutoSize = true,
                ForeColor = Color.White,
                Depth = 0
            };

            var panelCampos = new TableLayoutPanel
            {
                Location = new Point(24, 70),
                Size = new Size(660, 180),
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Color.FromArgb(30, 30, 30),
                ColumnStyles = { new ColumnStyle(SizeType.Percent, 50F), new ColumnStyle(SizeType.Percent, 50F) },
                RowStyles = { new RowStyle(SizeType.Absolute, 58F), new RowStyle(SizeType.Absolute, 58F) }
            };

            txtCuit = new MaterialTextBox2
            {
                Hint = "CUIT *",
                Dock = DockStyle.Fill,
                AnimateReadOnly = false,
                Depth = 0,
                Margin = new Padding(0, 0, 10, 10)
            };
            txtRazonSocial = new MaterialTextBox2
            {
                Hint = "Razón Social *",
                Dock = DockStyle.Fill,
                AnimateReadOnly = false,
                Depth = 0,
                Margin = new Padding(0, 0, 0, 10)
            };
            txtCorreo = new MaterialTextBox2
            {
                Hint = "Correo (opcional)",
                Dock = DockStyle.Fill,
                AnimateReadOnly = false,
                Depth = 0,
                Margin = new Padding(0, 0, 10, 0)
            };
            txtTelefono = new MaterialTextBox2
            {
                Hint = "Teléfono (opcional)",
                Dock = DockStyle.Fill,
                AnimateReadOnly = false,
                Depth = 0,
                Margin = new Padding(0, 0, 0, 0)
            };

            panelCampos.Controls.Add(txtCuit, 0, 0);
            panelCampos.Controls.Add(txtRazonSocial, 1, 0);
            panelCampos.Controls.Add(txtCorreo, 0, 1);
            panelCampos.Controls.Add(txtTelefono, 1, 1);

            var panelAcciones = new FlowLayoutPanel
            {
                Location = new Point(380, 270),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.RightToLeft,
                BackColor = Color.FromArgb(30, 30, 30),
                Margin = new Padding(0)
            };

            btnGuardar = new MaterialButton
            {
                Text = "Guardar",
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Height = 36,
                Type = MaterialButton.MaterialButtonType.Contained,
                HighEmphasis = true,
                UseAccentColor = false,
                Depth = 0,
                Margin = new Padding(4, 6, 4, 6)
            };
            btnCancelar = new MaterialButton
            {
                Text = "Cancelar",
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Height = 36,
                Type = MaterialButton.MaterialButtonType.Outlined,
                HighEmphasis = true,
                UseAccentColor = false,
                Depth = 0,
                Margin = new Padding(4, 6, 4, 6)
            };

            panelAcciones.Controls.Add(btnCancelar);
            panelAcciones.Controls.Add(btnGuardar);

            Controls.Add(lblTitulo);
            Controls.Add(panelCampos);
            Controls.Add(panelAcciones);
        }

        private void PrepararDatos()
        {
            if (_editar is null) return;
            txtCuit.Text = _editar.Cuit ?? string.Empty;
            txtRazonSocial.Text = _editar.RazonSocial ?? string.Empty;
            txtCorreo.Text = _editar.Correo ?? string.Empty;
            txtTelefono.Text = _editar.Telefono ?? string.Empty;
        }

        private async void btnGuardar_Click(object? sender, EventArgs e)
        {
            if (_guardando) return;
            _guardando = true;
            btnGuardar.Enabled = false;
            try
            {
                if (_editar is null)
                {
                    var dto = new CrearProveedorDto
                    {
                        Cuit = txtCuit.Text,
                        RazonSocial = txtRazonSocial.Text,
                        Correo = txtCorreo.Text,
                        Telefono = txtTelefono.Text
                    };
                    var resp = await _negocio.CrearProveedorAsync(dto);
                    if (resp.Resultado != ResultadoCreacionProveedor.Exito)
                    {
                        MaterialMessageBox.Show(this, resp.Mensaje, "Atención");
                        txtCuit.Focus();
                        return;
                    }
                    MaterialMessageBox.Show(this, resp.Mensaje, "Proveedor creado");
                }
                else
                {
                    var dto = new ModificarProveedorDto
                    {
                        IdProveedor = _editar.IdProveedor,
                        Cuit = txtCuit.Text,
                        RazonSocial = txtRazonSocial.Text,
                        Correo = txtCorreo.Text,
                        Telefono = txtTelefono.Text
                    };
                    var resp = await _negocio.ModificarProveedorAsync(dto);
                    if (resp.Resultado != ResultadoModificacionProveedor.Exito)
                    {
                        MaterialMessageBox.Show(this, resp.Mensaje, "Atención");
                        txtCuit.Focus();
                        return;
                    }
                    MaterialMessageBox.Show(this, resp.Mensaje, "Proveedor modificado");
                }
                DialogResult = DialogResult.OK;
                Close();
            }
            finally
            {
                _guardando = false;
                btnGuardar.Enabled = true;
            }
        }

        private void btnCancelar_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
