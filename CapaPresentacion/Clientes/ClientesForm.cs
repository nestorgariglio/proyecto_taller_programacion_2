using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CapaNegocio;
using CapaNegocio.DTOs.Clientes;
using CapaNegocio.Enums;
using CapaPresentacion.Tema;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CapaPresentacion.Clientes
{
    /// <summary>
    /// Vista del padrón de clientes: grilla real, buscador y filtro de estado, con el
    /// alta y la edición en el modal <see cref="ClienteForm"/> y la baja/reactivación
    /// con confirmación única (RF-21, RF-22). La pantalla no valida ni redacta
    /// mensajes de negocio (RF-26, D-T9): solo muestra el texto que devuelve la lógica
    /// de negocio.
    /// </summary>
    public partial class ClientesForm : UserControl
    {
        private readonly ClienteNegocio _clienteNegocio;
        private readonly System.Windows.Forms.Timer _temporizadorBusqueda =
            new() { Interval = 300 };

        private MaterialTextBox2 _txtBuscar = null!;
        private MaterialComboBox _cboEstado = null!;
        private DataGridView _gridClientes = null!;
        private MaterialLabel _lblEstadoListado = null!;
        private MaterialButton _btnNuevo = null!;
        private MaterialButton _btnEditar = null!;
        private MaterialButton _btnDesactivar = null!;
        private MaterialButton _btnReactivar = null!;
        private MaterialButton _btnActualizar = null!;

        private string? _rolActual;
        private bool _cargando;

        /// <summary>
        /// Inicializa la vista del padrón de clientes.
        /// </summary>
        /// <param name="clienteNegocio">Servicio que lista y opera sobre los clientes.</param>
        public ClientesForm(ClienteNegocio clienteNegocio)
        {
            ArgumentNullException.ThrowIfNull(clienteNegocio);

            _clienteNegocio = clienteNegocio;
            InitializeComponent();
            BackColor = TemaAplicacion.FondoPrincipal;
            ArmarInterfazMaterial();

            _temporizadorBusqueda.Tick += temporizadorBusqueda_Tick;
            this.Disposed += (_, _) => _temporizadorBusqueda.Dispose();
        }

        /// <summary>
        /// Carga la grilla con el filtro de estado y el texto del buscador actuales.
        /// Con el buscador vacío el negocio lista todo el filtro (D-12); el mensaje que
        /// se muestra es siempre el del negocio (RF-19, RF-26).
        /// </summary>
        /// <returns>Tarea que representa la carga de clientes.</returns>
        public async Task CargarClientesAsync()
        {
            if (_cargando)
            {
                return;
            }

            _cargando = true;
            _btnActualizar.Enabled = false;

            try
            {
                FiltroEstadoCliente filtro = _cboEstado.SelectedIndex == 1
                    ? FiltroEstadoCliente.Inactivos
                    : FiltroEstadoCliente.Activos;

                RespuestaListadoClientes respuesta =
                    await _clienteNegocio.ListarClientesAsync(filtro, _txtBuscar.Text);

                _gridClientes.DataSource = respuesta.Clientes.ToList();
                ActualizarEstadoBotones();
                _lblEstadoListado.Text = respuesta.Mensaje;
                _lblEstadoListado.ForeColor =
                    respuesta.Resultado == ResultadoListadoClientes.ErrorPersistencia
                        ? Color.FromArgb(220, 110, 110)
                        : Color.Gainsboro;
            }
            finally
            {
                _btnActualizar.Enabled = true;
                _cargando = false;
            }
        }

        /// <summary>
        /// Aplica el gating por rol de las acciones del padrón (RF-27, D-5, D-10):
        /// «Desactivar» y «Reactivar» solo se muestran a Administrador y Encargado;
        /// «Nuevo» y «Editar» quedan disponibles para los tres roles. Ante un rol nulo
        /// o desconocido las acciones administrativas quedan ocultas (fail-safe).
        /// </summary>
        /// <param name="rol">Descripción del rol del usuario (puede ser null o vacío).</param>
        public void AplicarRolUsuario(string? rol)
        {
            _rolActual = rol;

            bool puedeAdministrar = PoliticaAcceso.PuedeAdministrarClientes(_rolActual);
            _btnDesactivar.Visible = puedeAdministrar;
            _btnReactivar.Visible = puedeAdministrar;
        }

        private void ArmarInterfazMaterial()
        {
            Controls.Clear();

            // 1. Contenedor principal de la vista
            Panel panelPrincipal = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = TemaAplicacion.FondoPrincipal,
                Padding = new Padding(15)
            };

            // 2. Encabezado
            MaterialLabel lblTitulo = new MaterialLabel
            {
                Text = "GESTIÓN Y PADRÓN DE CLIENTES",
                FontType = MaterialSkinManager.fontType.H6,
                Location = new Point(20, 15),
                AutoSize = true
            };

            // 3. Buscador del padrón y filtro de estado (inicia en Activos)
            _txtBuscar = new MaterialTextBox2
            {
                Hint = "Buscar por Documento o Nombre completo...",
                Location = new Point(20, 55),
                Width = 350,
                AnimateReadOnly = true
            };

            _cboEstado = new MaterialComboBox
            {
                Hint = "Estado",
                Location = new Point(385, 55),
                Width = 150
            };
            _cboEstado.Items.AddRange(new object[] { "Activos", "Inactivos" });
            _cboEstado.SelectedIndex = 0;

            // Mensaje de estado del listado: siempre el texto del negocio (RF-26).
            _lblEstadoListado = new MaterialLabel
            {
                Location = new Point(560, 62),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FontType = MaterialSkinManager.fontType.Body2
            };

            // 4. Acciones del padrón
            FlowLayoutPanel panelAcciones = new FlowLayoutPanel
            {
                Location = new Point(20, 110),
                Width = panelPrincipal.Width - 40,
                Height = 55,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 2, 0, 0)
            };

            _btnNuevo = new MaterialButton
            {
                Text = "Nuevo Cliente",
                Width = 150,
                Height = 48,
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true
            };

            _btnEditar = new MaterialButton
            {
                Text = "Editar",
                Width = 120,
                Height = 48,
                Type = MaterialButton.MaterialButtonType.Contained
            };

            // Nacen ocultas: AplicarRolUsuario las muestra solo a Administrador y
            // Encargado; si el gating no se aplicara, ningún otro rol las ve (RF-27).
            _btnDesactivar = new MaterialButton
            {
                Text = "Desactivar",
                Width = 150,
                Height = 48,
                Type = MaterialButton.MaterialButtonType.Contained,
                Visible = false
            };

            _btnReactivar = new MaterialButton
            {
                Text = "Reactivar",
                Width = 150,
                Height = 48,
                Type = MaterialButton.MaterialButtonType.Contained,
                Visible = false
            };

            _btnActualizar = new MaterialButton
            {
                Text = "Actualizar",
                Width = 135,
                Height = 48,
                Type = MaterialButton.MaterialButtonType.Contained
            };

            panelAcciones.Controls.Add(_btnNuevo);
            panelAcciones.Controls.Add(_btnEditar);
            panelAcciones.Controls.Add(_btnDesactivar);
            panelAcciones.Controls.Add(_btnReactivar);
            panelAcciones.Controls.Add(_btnActualizar);

            // 5. Grilla real de clientes (sin datos de ejemplo)
            _gridClientes = new DataGridView
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
                BackgroundColor = Color.FromArgb(40, 40, 40),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                EnableHeadersVisualStyles = false
            };

            _gridClientes.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            _gridClientes.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _gridClientes.ColumnHeadersDefaultCellStyle.Font = new Font("Roboto", 10, FontStyle.Bold);
            _gridClientes.ColumnHeadersHeight = 40;

            _gridClientes.DefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
            _gridClientes.DefaultCellStyle.ForeColor = Color.White;
            _gridClientes.DefaultCellStyle.SelectionBackColor = Color.FromArgb(80, 80, 80);
            _gridClientes.DefaultCellStyle.SelectionForeColor = Color.White;
            _gridClientes.RowTemplate.Height = 35;

            _gridClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDocumento",
                HeaderText = "Documento",
                DataPropertyName = nameof(ClienteRespuestaDto.Documento),
                FillWeight = 18
            });
            _gridClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colNombreCompleto",
                HeaderText = "Nombre completo",
                DataPropertyName = nameof(ClienteRespuestaDto.NombreCompleto),
                FillWeight = 28
            });
            _gridClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCorreo",
                HeaderText = "Correo",
                DataPropertyName = nameof(ClienteRespuestaDto.Correo),
                FillWeight = 28
            });
            _gridClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTelefono",
                HeaderText = "Teléfono",
                DataPropertyName = nameof(ClienteRespuestaDto.Telefono),
                FillWeight = 16
            });
            _gridClientes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colEstado",
                HeaderText = "Estado",
                DataPropertyName = nameof(ClienteRespuestaDto.Estado),
                FillWeight = 10
            });

            // Ensamblar
            panelPrincipal.Controls.Add(lblTitulo);
            panelPrincipal.Controls.Add(_txtBuscar);
            panelPrincipal.Controls.Add(_cboEstado);
            panelPrincipal.Controls.Add(_lblEstadoListado);
            panelPrincipal.Controls.Add(panelAcciones);
            panelPrincipal.Controls.Add(_gridClientes);

            Controls.Add(panelPrincipal);

            // Eventos: buscador con debounce, filtro de estado, acciones del padrón
            // y recarga manual.
            _txtBuscar.TextChanged += txtBuscar_TextChanged;
            _txtBuscar.KeyDown += txtBuscar_KeyDown;
            _cboEstado.SelectedIndexChanged += cboEstado_SelectedIndexChanged;
            _btnNuevo.Click += btnNuevo_Click;
            _btnEditar.Click += btnEditar_Click;
            _btnDesactivar.Click += btnDesactivar_Click;
            _btnReactivar.Click += btnReactivar_Click;
            _btnActualizar.Click += btnActualizar_Click;
            _gridClientes.CellFormatting += gridClientes_CellFormatting;
            _gridClientes.SelectionChanged += gridClientes_SelectionChanged;

            ActualizarEstadoBotones();
        }

        /// <summary>
        /// Habilita las acciones según la fila seleccionada: «Editar» con cualquier
        /// cliente, «Desactivar» solo sobre uno activo y «Reactivar» solo sobre uno
        /// inactivo. No cambia la visibilidad, que depende del rol (RF-27, D-10).
        /// </summary>
        private void ActualizarEstadoBotones()
        {
            ClienteRespuestaDto? cliente = _gridClientes.CurrentRow?.DataBoundItem
                as ClienteRespuestaDto;

            _btnEditar.Enabled = cliente is not null;
            _btnDesactivar.Enabled = cliente?.Estado == true;
            _btnReactivar.Enabled = cliente?.Estado == false;
        }

        /// <summary>
        /// Deja el filtro en Activos sin disparar la recarga, para que el alta
        /// recién creada quede visible. El llamado posterior a
        /// <see cref="CargarClientesAsync"/> es el que consulta el padrón.
        /// </summary>
        private void SeleccionarFiltroActivos()
        {
            _cboEstado.SelectedIndexChanged -= cboEstado_SelectedIndexChanged;
            _cboEstado.SelectedIndex = 0;
            _cboEstado.SelectedIndexChanged += cboEstado_SelectedIndexChanged;
        }

        private async void btnNuevo_Click(object? sender, EventArgs e)
        {
            using var formulario = new ClienteForm(_clienteNegocio);

            if (formulario.ShowDialog(FindForm()) != DialogResult.OK)
            {
                return;
            }

            // RF-20: el padrón queda en el filtro Activos y sin búsqueda, para que el
            // cliente recién creado quede visible de inmediato.
            _temporizadorBusqueda.Stop();
            LimpiarBuscador();
            SeleccionarFiltroActivos();
            await CargarClientesAsync();
        }

        /// <summary>
        /// Limpia el buscador sin disparar <c>TextChanged</c>, para que la recarga del
        /// flujo de alta sea una sola: la que se hace explícitamente después.
        /// </summary>
        private void LimpiarBuscador()
        {
            _txtBuscar.TextChanged -= txtBuscar_TextChanged;
            _txtBuscar.Text = string.Empty;
            _txtBuscar.TextChanged += txtBuscar_TextChanged;

            // Fuerza el repintado para que el Hint vuelva a mostrarse al quedar vacío.
            _txtBuscar.Refresh();
        }

        private async void btnEditar_Click(object? sender, EventArgs e)
        {
            if (_gridClientes.CurrentRow?.DataBoundItem is not ClienteRespuestaDto cliente)
            {
                // Texto de guía de la interfaz, no una validación de negocio.
                MaterialMessageBox.Show(this, "Seleccione un cliente para editar.", "Aviso");
                return;
            }

            // Se permite editar tanto activos como inactivos (RF-13); al volver se
            // recarga con el filtro y el buscador vigentes.
            using var formulario = new ClienteForm(_clienteNegocio, cliente);

            if (formulario.ShowDialog(FindForm()) != DialogResult.OK)
            {
                return;
            }

            await CargarClientesAsync();
        }

        private async void btnDesactivar_Click(object? sender, EventArgs e)
        {
            if (_gridClientes.CurrentRow?.DataBoundItem is not ClienteRespuestaDto cliente)
            {
                MaterialMessageBox.Show(this, "Seleccione un cliente para dar de baja.", "Aviso");
                return;
            }

            // RF-21/RF-22: el negocio entrega el texto de la confirmación única,
            // que ya incluye la cantidad de ventas asociadas cuando corresponde.
            _btnDesactivar.Enabled = false;

            try
            {
                RespuestaConfirmacionBajaCliente confirmacion =
                    await _clienteNegocio.ObtenerConfirmacionBajaClienteAsync(
                        cliente.IdCliente,
                        _rolActual);

                if (confirmacion.Resultado != ResultadoConfirmacionBajaCliente.ConfirmacionRequerida)
                {
                    MaterialMessageBox.Show(this, confirmacion.Mensaje, "Atención");
                    return;
                }

                if (FindForm() is not Form formularioPrincipal)
                {
                    return;
                }

                var dialogo = new MaterialDialog(
                    formularioPrincipal,
                    "Confirmar baja",
                    confirmacion.Mensaje,
                    "DAR DE BAJA",
                    true,
                    "CANCELAR");

                // Cancelar: no se escribe nada y el cliente queda sin cambios.
                if (dialogo.ShowDialog(formularioPrincipal) != DialogResult.OK)
                {
                    return;
                }

                RespuestaBajaCliente respuesta =
                    await _clienteNegocio.DarBajaClienteAsync(cliente.IdCliente, _rolActual);

                MaterialMessageBox.Show(
                    this,
                    respuesta.Mensaje,
                    respuesta.Resultado == ResultadoBajaCliente.Exito
                        ? "Cliente dado de baja"
                        : "Atención");

                await CargarClientesAsync();
            }
            finally
            {
                ActualizarEstadoBotones();
            }
        }

        private async void btnReactivar_Click(object? sender, EventArgs e)
        {
            if (_gridClientes.CurrentRow?.DataBoundItem is not ClienteRespuestaDto cliente)
            {
                MaterialMessageBox.Show(this, "Seleccione un cliente para reactivarlo.", "Aviso");
                return;
            }

            // RF-26: la vista no redacta confirmaciones; se ejecuta la reactivación y
            // se muestra el mensaje que devuelve el negocio (éxito o rechazo).
            _btnReactivar.Enabled = false;

            try
            {
                RespuestaActivacionCliente respuesta =
                    await _clienteNegocio.ReactivarClienteAsync(cliente.IdCliente, _rolActual);

                MaterialMessageBox.Show(
                    this,
                    respuesta.Mensaje,
                    respuesta.Resultado == ResultadoActivacionCliente.Exito
                        ? "Cliente reactivado"
                        : "Atención");

                await CargarClientesAsync();
            }
            finally
            {
                ActualizarEstadoBotones();
            }
        }

        private async void btnActualizar_Click(object? sender, EventArgs e)
        {
            await CargarClientesAsync();
        }

        private void txtBuscar_TextChanged(object? sender, EventArgs e)
        {
            // Debounce: no se consulta el negocio en cada pulsación.
            _temporizadorBusqueda.Stop();
            _temporizadorBusqueda.Start();
        }

        private async void txtBuscar_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;
            _temporizadorBusqueda.Stop();
            await CargarClientesAsync();
        }

        private async void temporizadorBusqueda_Tick(object? sender, EventArgs e)
        {
            _temporizadorBusqueda.Stop();
            await CargarClientesAsync();
        }

        private async void cboEstado_SelectedIndexChanged(object? sender, EventArgs e)
        {
            await CargarClientesAsync();
        }

        private void gridClientes_SelectionChanged(object? sender, EventArgs e)
        {
            ActualizarEstadoBotones();
        }

        private void gridClientes_CellFormatting(
            object? sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            if (_gridClientes.Columns[e.ColumnIndex].Name == "colEstado" && e.Value is bool estado)
            {
                e.Value = estado ? "Activo" : "Inactivo";
                e.FormattingApplied = true;
            }
        }
    }
}
