using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CapaNegocio;
using CapaNegocio.DTOs.Proveedores;
using CapaNegocio.Enums;
using CapaPresentacion.Tema;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CapaPresentacion.Proveedores
{
    public partial class ProveedoresForm : UserControl
    {
        private readonly ProveedorNegocio _negocio;
        private readonly System.Windows.Forms.Timer _temporizadorBusqueda = new() { Interval = 300 };

        private MaterialTextBox2 _txtBuscar = null!;
        private MaterialComboBox _cboEstado = null!;
        private DataGridView _grid = null!;
        private MaterialLabel _lblEstadoListado = null!;
        private MaterialButton _btnNuevo = null!;
        private MaterialButton _btnEditar = null!;
        private MaterialButton _btnDesactivar = null!;
        private MaterialButton _btnReactivar = null!;
        private MaterialButton _btnActualizar = null!;

        private string? _rolActual;
        private bool _cargando;

        public ProveedoresForm(ProveedorNegocio negocio)
        {
            ArgumentNullException.ThrowIfNull(negocio);
            _negocio = negocio;
            BackColor = TemaAplicacion.FondoPrincipal;
            ArmarInterfazMaterial();
            _temporizadorBusqueda.Tick += temporizadorBusqueda_Tick;
            this.Disposed += (_, _) => _temporizadorBusqueda.Dispose();
        }

        public async Task CargarProveedoresAsync()
        {
            if (_cargando) return;
            _cargando = true;
            _btnActualizar.Enabled = false;
            try
            {
                FiltroEstadoProveedor filtro = _cboEstado.SelectedIndex == 1
                    ? FiltroEstadoProveedor.Inactivos : FiltroEstadoProveedor.Activos;
                RespuestaListadoProveedores resp = await _negocio.ListarProveedoresAsync(filtro, _txtBuscar.Text);
                _grid.DataSource = resp.Proveedores.ToList();
                ActualizarEstadoBotones();
                _lblEstadoListado.Text = resp.Mensaje;
                _lblEstadoListado.ForeColor = resp.Resultado == ResultadoListadoProveedores.ErrorPersistencia
                    ? Color.FromArgb(220, 110, 110) : Color.Gainsboro;
            }
            finally
            {
                _btnActualizar.Enabled = true;
                _cargando = false;
            }
        }

        public void AplicarRolUsuario(string? rol)
        {
            _rolActual = rol;
            bool puedeAdministrar = PoliticaAcceso.PuedeAdministrarProveedores(_rolActual);
            _btnDesactivar.Visible = puedeAdministrar;
            _btnReactivar.Visible = puedeAdministrar;
        }

        private void ArmarInterfazMaterial()
        {
            Controls.Clear();
            Panel panelPrincipal = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = TemaAplicacion.FondoPrincipal,
                Padding = new Padding(15)
            };

            MaterialLabel lblTitulo = new MaterialLabel
            {
                Text = "GESTIÓN DE PROVEEDORES",
                FontType = MaterialSkinManager.fontType.H6,
                Location = new Point(20, 15),
                AutoSize = true,
                ForeColor = Color.White,
                Depth = 0
            };

            _txtBuscar = new MaterialTextBox2
            {
                Hint = "Buscar por Documento o Razón Social...",
                Location = new Point(20, 55),
                Width = 350,
                AnimateReadOnly = true,
                Depth = 0
            };

            _cboEstado = new MaterialComboBox
            {
                Hint = "Estado",
                Location = new Point(385, 55),
                Width = 150,
                StartIndex = 0,
                Depth = 0
            };
            _cboEstado.Items.AddRange(new object[] { "Activos", "Inactivos" });
            _cboEstado.SelectedIndex = 0;

            _lblEstadoListado = new MaterialLabel
            {
                Location = new Point(560, 62),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FontType = MaterialSkinManager.fontType.Body2,
                ForeColor = Color.Gainsboro,
                Depth = 0
            };

            FlowLayoutPanel panelAcciones = new FlowLayoutPanel
            {
                Location = new Point(20, 110),
                Width = panelPrincipal.Width - 40,
                Height = 55,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 2, 0, 2),
                BackColor = TemaAplicacion.FondoPrincipal
            };

            _btnNuevo = new MaterialButton
            {
                Text = "➕ Registrar Proveedor",
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Height = 36,
                Type = MaterialButton.MaterialButtonType.Contained,
                HighEmphasis = true,
                UseAccentColor = true,
                Depth = 0,
                Margin = new Padding(4, 6, 4, 6)
            };
            _btnEditar = new MaterialButton
            {
                Text = "✏️ Editar",
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Height = 36,
                Type = MaterialButton.MaterialButtonType.Contained,
                HighEmphasis = true,
                UseAccentColor = false,
                Depth = 0,
                Margin = new Padding(4, 6, 4, 6)
            };
            _btnDesactivar = new MaterialButton
            {
                Text = "⛔ Desactivar",
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Height = 36,
                Type = MaterialButton.MaterialButtonType.Contained,
                HighEmphasis = true,
                UseAccentColor = false,
                Depth = 0,
                Visible = false,
                Margin = new Padding(4, 6, 4, 6)
            };
            _btnReactivar = new MaterialButton
            {
                Text = "🔄 Reactivar",
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Height = 36,
                Type = MaterialButton.MaterialButtonType.Contained,
                HighEmphasis = true,
                UseAccentColor = false,
                Depth = 0,
                Visible = false,
                Margin = new Padding(4, 6, 4, 6)
            };
            _btnActualizar = new MaterialButton
            {
                Text = "🔄 Actualizar",
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Height = 36,
                Type = MaterialButton.MaterialButtonType.Contained,
                HighEmphasis = true,
                UseAccentColor = false,
                Depth = 0,
                Margin = new Padding(4, 6, 4, 6)
            };

            panelAcciones.Controls.Add(_btnNuevo);
            panelAcciones.Controls.Add(_btnEditar);
            panelAcciones.Controls.Add(_btnDesactivar);
            panelAcciones.Controls.Add(_btnReactivar);
            panelAcciones.Controls.Add(_btnActualizar);

            _grid = new DataGridView
            {
                Location = new Point(20, 180),
                Width = panelPrincipal.Width - 40,
                Height = 300,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.FromArgb(48, 48, 48),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                EnableHeadersVisualStyles = false
            };

            _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(55, 71, 79),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                SelectionBackColor = Color.FromArgb(55, 71, 79),
                SelectionForeColor = Color.White,
                WrapMode = DataGridViewTriState.True
            };
            _grid.ColumnHeadersHeight = 42;

            _grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(48, 48, 48),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.White,
                SelectionBackColor = Color.FromArgb(255, 87, 34),
                SelectionForeColor = Color.White,
                WrapMode = DataGridViewTriState.False
            };
            _grid.DefaultCellStyle.SelectionForeColor = Color.White;
            _grid.GridColor = Color.FromArgb(80, 80, 80);
            _grid.RowTemplate.Height = 36;

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDocumento",
                HeaderText = "Documento",
                DataPropertyName = nameof(ProveedorRespuestaDto.Cuit),
                FillWeight = 18
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colRazonSocial",
                HeaderText = "Razón Social",
                DataPropertyName = nameof(ProveedorRespuestaDto.RazonSocial),
                FillWeight = 28
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCorreo",
                HeaderText = "Correo",
                DataPropertyName = nameof(ProveedorRespuestaDto.Correo),
                FillWeight = 20
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTelefono",
                HeaderText = "Teléfono",
                DataPropertyName = nameof(ProveedorRespuestaDto.Telefono),
                FillWeight = 16
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colEstado",
                HeaderText = "Estado",
                DataPropertyName = nameof(ProveedorRespuestaDto.Estado),
                FillWeight = 10
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colFecha",
                HeaderText = "Fecha Registro",
                DataPropertyName = nameof(ProveedorRespuestaDto.FechaRegistro),
                FillWeight = 8,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "g" }
            });

            panelPrincipal.Controls.Add(lblTitulo);
            panelPrincipal.Controls.Add(_txtBuscar);
            panelPrincipal.Controls.Add(_cboEstado);
            panelPrincipal.Controls.Add(_lblEstadoListado);
            panelPrincipal.Controls.Add(panelAcciones);
            panelPrincipal.Controls.Add(_grid);
            Controls.Add(panelPrincipal);

            _txtBuscar.TextChanged += txtBuscar_TextChanged;
            _txtBuscar.KeyDown += txtBuscar_KeyDown;
            _cboEstado.SelectedIndexChanged += cboEstado_SelectedIndexChanged;
            _btnNuevo.Click += btnNuevo_Click;
            _btnEditar.Click += btnEditar_Click;
            _btnDesactivar.Click += btnDesactivar_Click;
            _btnReactivar.Click += btnReactivar_Click;
            _btnActualizar.Click += btnActualizar_Click;
            _grid.CellFormatting += grid_CellFormatting;
            _grid.SelectionChanged += grid_SelectionChanged;

            ActualizarEstadoBotones();
        }

        private void ActualizarEstadoBotones()
        {
            ProveedorRespuestaDto? p = _grid.CurrentRow?.DataBoundItem as ProveedorRespuestaDto;
            _btnEditar.Enabled = p is not null;
            bool puedeAdmin = PoliticaAcceso.PuedeAdministrarProveedores(_rolActual);
            _btnDesactivar.Enabled = p?.Estado == true && puedeAdmin;
            _btnReactivar.Enabled = p?.Estado == false && puedeAdmin;
        }

        private async void btnActualizar_Click(object? sender, EventArgs e)
        {
            await CargarProveedoresAsync();
        }

        private async void btnNuevo_Click(object? sender, EventArgs e)
        {
            if (!PoliticaAcceso.PuedeAdministrarProveedores(_rolActual) && _rolActual != null && !PoliticaAcceso.TieneAcceso(_rolActual, "Proveedores"))
            {
                // Defensa defensiva: si no tiene acceso al módulo, no abre
                MaterialMessageBox.Show(this, "Acceso no autorizado.", "Atención");
                return;
            }
            using var form = new ProveedorForm(_negocio);
            if (form.ShowDialog(FindForm()) == DialogResult.OK)
            {
                LimpiarBuscador();
                SeleccionarFiltroActivos();
                await CargarProveedoresAsync();
            }
        }

        private async void btnEditar_Click(object? sender, EventArgs e)
        {
            if (_grid.CurrentRow?.DataBoundItem is not ProveedorRespuestaDto p)
            {
                MaterialMessageBox.Show(this, "Seleccione un proveedor para editar.", "Aviso");
                return;
            }
            using var form = new ProveedorForm(_negocio, p);
            if (form.ShowDialog(FindForm()) == DialogResult.OK)
            {
                await CargarProveedoresAsync();
            }
        }

        private async void btnDesactivar_Click(object? sender, EventArgs e)
        {
            if (_grid.CurrentRow?.DataBoundItem is not ProveedorRespuestaDto p)
            {
                MaterialMessageBox.Show(this, "Seleccione un proveedor para dar de baja.", "Aviso");
                return;
            }
            if (!PoliticaAcceso.PuedeAdministrarProveedores(_rolActual))
            {
                MaterialMessageBox.Show(this, "No tiene permisos para dar de baja proveedores.", "Atención");
                return;
            }
            _btnDesactivar.Enabled = false;
            try
            {
                var confirmacion = await _negocio.ObtenerConfirmacionBajaProveedorAsync(p.IdProveedor, _rolActual);
                if (confirmacion.Resultado != ResultadoConfirmacionBajaProveedor.ConfirmacionRequerida)
                {
                    MaterialMessageBox.Show(this, confirmacion.Mensaje, "Atención");
                    return;
                }
                if (FindForm() is not Form principal) return;
                var dialogo = new MaterialDialog(principal, "Confirmar baja", confirmacion.Mensaje, "DAR DE BAJA", true, "CANCELAR");
                if (dialogo.ShowDialog(principal) != DialogResult.OK) return;
                var baja = await _negocio.DarBajaProveedorAsync(p.IdProveedor, _rolActual);
                MaterialMessageBox.Show(this, baja.Mensaje, baja.Resultado == ResultadoBajaProveedor.Exito ? "Proveedor dado de baja" : "Atención");
                await CargarProveedoresAsync();
            }
            finally
            {
                ActualizarEstadoBotones();
            }
        }

        private async void btnReactivar_Click(object? sender, EventArgs e)
        {
            if (_grid.CurrentRow?.DataBoundItem is not ProveedorRespuestaDto p)
            {
                MaterialMessageBox.Show(this, "Seleccione un proveedor para reactivarlo.", "Aviso");
                return;
            }
            if (!PoliticaAcceso.PuedeAdministrarProveedores(_rolActual))
            {
                MaterialMessageBox.Show(this, "No tiene permisos para reactivar proveedores.", "Atención");
                return;
            }
            _btnReactivar.Enabled = false;
            try
            {
                var resp = await _negocio.ReactivarProveedorAsync(p.IdProveedor, _rolActual);
                MaterialMessageBox.Show(this, resp.Mensaje, resp.Resultado == ResultadoActivacionProveedor.Exito ? "Proveedor reactivado" : "Atención");
                await CargarProveedoresAsync();
            }
            finally
            {
                ActualizarEstadoBotones();
            }
        }

        private void txtBuscar_TextChanged(object? sender, EventArgs e)
        {
            _temporizadorBusqueda.Stop();
            _temporizadorBusqueda.Start();
        }

        private void txtBuscar_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                _temporizadorBusqueda.Stop();
                CargarProveedoresAsync(); // fire-and-forget async en evento
            }
        }

        private async void temporizadorBusqueda_Tick(object? sender, EventArgs e)
        {
            _temporizadorBusqueda.Stop();
            await CargarProveedoresAsync();
        }

        private async void cboEstado_SelectedIndexChanged(object? sender, EventArgs e)
        {
            await CargarProveedoresAsync();
        }

        private void grid_SelectionChanged(object? sender, EventArgs e)
        {
            ActualizarEstadoBotones();
        }

        private void grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "colEstado") return;
            if (e.Value is bool estado)
            {
                e.Value = estado ? "Activo" : "Inactivo";
                e.FormattingApplied = true;
            }
        }

        private void LimpiarBuscador()
        {
            _txtBuscar.TextChanged -= txtBuscar_TextChanged;
            _txtBuscar.Text = string.Empty;
            _txtBuscar.TextChanged += txtBuscar_TextChanged;
            _txtBuscar.Refresh();
        }

        private void SeleccionarFiltroActivos()
        {
            _cboEstado.SelectedIndexChanged -= cboEstado_SelectedIndexChanged;
            _cboEstado.SelectedIndex = 0;
            _cboEstado.SelectedIndexChanged += cboEstado_SelectedIndexChanged;
        }

    }
}
