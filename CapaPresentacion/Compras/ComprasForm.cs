using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CapaNegocio;
using CapaNegocio.Compras;
using CapaNegocio.DTOs.Compras;
using CapaNegocio.Enums;
using CapaPresentacion.Tema;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CapaPresentacion.Compras
{
    /// <summary>
    /// Pantalla de carga de compras a proveedores: proveedor elegido, búsqueda de
    /// productos, área de captura con prellenado editable, grilla de líneas con
    /// edición y quita, total general y historial de compras registradas.
    /// La pantalla no valida, no calcula importes ni redacta mensajes: solo invoca
    /// <see cref="CompraNegocio"/> y muestra el <c>Mensaje</c> que este devuelve
    /// (RF-55, RNF-6 y RNF-13).
    /// </summary>
    public partial class ComprasForm : UserControl
    {
        private const string ColumnaLineaProducto = "colLineaProducto";
        private const string ColumnaLineaPrecioCompra = "colLineaPrecioCompra";
        private const string ColumnaLineaPrecioVenta = "colLineaPrecioVenta";
        private const string ColumnaLineaSubtotal = "colLineaSubtotal";
        private const string ColumnaLineaEditar = "colLineaEditar";
        private const string ColumnaLineaQuitar = "colLineaQuitar";
        private const string ColumnaHistorialFecha = "colHistorialFecha";
        private const string ColumnaHistorialTotal = "colHistorialTotal";
        private const string ColumnaHistorialEstado = "colHistorialEstado";
        private const string TextoProveedorSinElegir = "Proveedor: —";
        private const string TextoSubtotalInvalido = "Subtotal: —";

        private readonly CompraNegocio _negocio;
        private readonly System.Windows.Forms.Timer _temporizadorProveedores =
            new() { Interval = 350 };
        private readonly System.Windows.Forms.Timer _temporizadorProductos =
            new() { Interval = 350 };
        private readonly List<LineaCompraDto> _lineas = new();
        private readonly List<ProveedorSeleccionadoDto> _proveedoresEncontrados = new();
        private readonly List<ProductoBuscadoDto> _productosEncontrados = new();

        private MaterialLabel _lblTitulo = null!;
        private MaterialTextBox2 _txtBuscarProveedor = null!;
        private MaterialComboBox _cboResultadosProveedor = null!;
        private MaterialLabel _lblProveedorElegido = null!;
        private MaterialTextBox2 _txtBuscarProducto = null!;
        private MaterialComboBox _cboResultadosProducto = null!;
        private MaterialTextBox2 _txtCantidad = null!;
        private MaterialTextBox2 _txtPrecioCompra = null!;
        private MaterialTextBox2 _txtPrecioVenta = null!;
        private MaterialLabel _lblSubtotalPreparacion = null!;
        private MaterialButton _btnAgregarLinea = null!;
        private DataGridView _gridLineas = null!;
        private MaterialLabel _lblMontoTotal = null!;
        private MaterialButton _btnRegistrar = null!;
        private MaterialButton _btnLimpiar = null!;
        private MaterialLabel _lblMensaje = null!;
        private MaterialLabel _lblTituloHistorial = null!;
        private DataGridView _gridHistorial = null!;
        private MaterialLabel _lblMensajeHistorial = null!;

        private ProveedorSeleccionadoDto? _proveedorElegido;
        private ProductoBuscadoDto? _productoElegido;
        private int? _indiceEdicion;
        private int _idUsuario;
        private string? _rolActual;
        private bool _registrando;
        private bool _cargandoHistorial;
        private bool _actualizandoControles;

        /// <summary>
        /// Inicializa la pantalla de compras con el servicio de negocio inyectado
        /// (mismo patrón que <see cref="Proveedores.ProveedoresForm"/>).
        /// </summary>
        /// <param name="negocio">Lógica de negocio de compras.</param>
        public ComprasForm(CompraNegocio negocio)
        {
            ArgumentNullException.ThrowIfNull(negocio);

            _negocio = negocio;
            BackColor = TemaAplicacion.FondoPrincipal;
            ArmarInterfazMaterial();

            _temporizadorProveedores.Tick += temporizadorProveedores_Tick;
            _temporizadorProductos.Tick += temporizadorProductos_Tick;
            this.Disposed += (_, _) =>
            {
                _temporizadorProveedores.Dispose();
                _temporizadorProductos.Dispose();
            };

            RefrescarGrillaLineas();
        }

        /// <summary>
        /// Guarda la sesión con la que se registran las compras y se consultan el
        /// historial y el detalle. La pantalla queda habilitada solo para los roles
        /// que operan el módulo (RF-8, RF-59, RF-60 y RF-61); con otro rol no hay
        /// acciones disponibles y la pestaña ya viene oculta desde el inicio.
        /// </summary>
        /// <param name="idUsuario">Identificador del usuario responsable.</param>
        /// <param name="rol">Descripción del rol de la sesión.</param>
        public void AplicarSesion(int idUsuario, string? rol)
        {
            _idUsuario = idUsuario;
            _rolActual = rol;

            bool habilitado = PoliticaAcceso.PuedeOperarCompras(_rolActual);
            _txtBuscarProveedor.Enabled = habilitado;
            _cboResultadosProveedor.Enabled = habilitado;
            _txtBuscarProducto.Enabled = habilitado;
            _cboResultadosProducto.Enabled = habilitado;
            _txtCantidad.Enabled = habilitado;
            _txtPrecioCompra.Enabled = habilitado;
            _txtPrecioVenta.Enabled = habilitado;
            _btnAgregarLinea.Enabled = habilitado;
            _btnLimpiar.Enabled = habilitado;
            _btnRegistrar.Enabled = habilitado && !_registrando;
        }

        /// <summary>
        /// Recarga el historial de compras con el rol de la sesión. El mensaje que
        /// se muestra es siempre el que devuelve el negocio (RF-49 … RF-52).
        /// </summary>
        /// <returns>Tarea que representa la consulta del historial.</returns>
        public async Task CargarHistorialAsync()
        {
            if (_cargandoHistorial)
            {
                return;
            }

            _cargandoHistorial = true;

            try
            {
                RespuestaHistorialCompras respuesta =
                    await _negocio.ObtenerHistorialAsync(_rolActual);

                _gridHistorial.DataSource = null;
                _gridHistorial.DataSource = respuesta.Compras.ToList();
                _lblMensajeHistorial.Text = respuesta.Mensaje;
            }
            finally
            {
                _cargandoHistorial = false;
            }
        }

        // =====================================================================
        // 1. CONSTRUCCIÓN DE LA INTERFAZ (MaterialSkin 2, dark)
        // =====================================================================

        private void ArmarInterfazMaterial()
        {
            Controls.Clear();

            Panel panelPrincipal = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = TemaAplicacion.FondoPrincipal,
                Padding = new Padding(15),
                AutoScroll = true
            };

            // Encabezado: rótulo propio de la carga de compras (RF-2).
            _lblTitulo = new MaterialLabel
            {
                Text = "Registro de Compras a Proveedores",
                FontType = MaterialSkinManager.fontType.H6,
                Location = new Point(20, 15),
                AutoSize = true,
                ForeColor = Color.White,
                Depth = 0
            };

            // Bloque de proveedor: búsqueda rápida, resultados y proveedor elegido
            // (RF-12 y RF-13). Elegir otro proveedor no toca las líneas cargadas.
            _txtBuscarProveedor = new MaterialTextBox2
            {
                Hint = "Buscar proveedor...",
                Location = new Point(20, 52),
                Width = 300,
                AnimateReadOnly = true,
                Depth = 0
            };

            _cboResultadosProveedor = new MaterialComboBox
            {
                Hint = "Resultados",
                Location = new Point(330, 52),
                Width = 360,
                Depth = 0
            };

            _lblProveedorElegido = new MaterialLabel
            {
                Text = TextoProveedorSinElegir,
                Location = new Point(20, 106),
                AutoSize = true,
                FontType = MaterialSkinManager.fontType.Body1,
                ForeColor = Color.Gainsboro,
                Depth = 0
            };

            // Área de captura en una fila: producto, cantidad, precios y subtotal.
            TableLayoutPanel panelCaptura = new TableLayoutPanel
            {
                Location = new Point(20, 140),
                Width = 940,
                Height = 64,
                ColumnCount = 7,
                RowCount = 1,
                BackColor = TemaAplicacion.FondoPrincipal,
                Margin = new Padding(0)
            };
            panelCaptura.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
            panelCaptura.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
            panelCaptura.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            panelCaptura.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115F));
            panelCaptura.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115F));
            panelCaptura.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            panelCaptura.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            panelCaptura.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));

            _txtBuscarProducto = new MaterialTextBox2
            {
                Hint = "Buscar producto...",
                Width = 184,
                AnimateReadOnly = true,
                Depth = 0,
                Margin = new Padding(0, 6, 6, 0)
            };

            _cboResultadosProducto = new MaterialComboBox
            {
                Hint = "Resultados",
                Width = 214,
                Depth = 0,
                Margin = new Padding(0, 6, 6, 0)
            };

            _txtCantidad = new MaterialTextBox2
            {
                Hint = "Cantidad",
                Width = 74,
                Depth = 0,
                Margin = new Padding(0, 6, 6, 0)
            };

            _txtPrecioCompra = new MaterialTextBox2
            {
                Hint = "Precio compra",
                Width = 109,
                Depth = 0,
                Margin = new Padding(0, 6, 6, 0)
            };

            _txtPrecioVenta = new MaterialTextBox2
            {
                Hint = "Precio venta",
                Width = 109,
                Depth = 0,
                Margin = new Padding(0, 6, 6, 0)
            };

            _lblSubtotalPreparacion = new MaterialLabel
            {
                Text = TextoSubtotalInvalido,
                Width = 94,
                Height = 48,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                FontType = MaterialSkinManager.fontType.Body2,
                ForeColor = Color.Gainsboro,
                Depth = 0,
                Margin = new Padding(0, 6, 6, 0)
            };

            _btnAgregarLinea = new MaterialButton
            {
                Text = "Agregar línea",
                Width = 116,
                Height = 40,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Type = MaterialButton.MaterialButtonType.Contained,
                HighEmphasis = true,
                UseAccentColor = true,
                Depth = 0,
                Margin = new Padding(0, 8, 0, 0)
            };

            panelCaptura.Controls.Add(_txtBuscarProducto, 0, 0);
            panelCaptura.Controls.Add(_cboResultadosProducto, 1, 0);
            panelCaptura.Controls.Add(_txtCantidad, 2, 0);
            panelCaptura.Controls.Add(_txtPrecioCompra, 3, 0);
            panelCaptura.Controls.Add(_txtPrecioVenta, 4, 0);
            panelCaptura.Controls.Add(_lblSubtotalPreparacion, 5, 0);
            panelCaptura.Controls.Add(_btnAgregarLinea, 6, 0);

            // Mensaje: siempre el texto del negocio, tal cual (RF-55).
            _lblMensaje = new MaterialLabel
            {
                Location = new Point(20, 208),
                Width = 1000,
                Height = 38,
                AutoSize = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                FontType = MaterialSkinManager.fontType.Body2,
                ForeColor = Color.Gainsboro,
                Depth = 0
            };

            // Grilla de líneas en carga: producto, cantidad, precios, subtotal y
            // las acciones Editar/Quitar por fila (RF-29 y RF-30).
            _gridLineas = CrearGrilla();
            _gridLineas.Location = new Point(20, 250);
            _gridLineas.Height = 160;
            _gridLineas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = ColumnaLineaProducto,
                HeaderText = "Producto",
                DataPropertyName = nameof(LineaCompraDto.NombreProducto),
                FillWeight = 34,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            _gridLineas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colLineaCantidad",
                HeaderText = "Cantidad",
                DataPropertyName = nameof(LineaCompraDto.Cantidad),
                FillWeight = 10,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            _gridLineas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = ColumnaLineaPrecioCompra,
                HeaderText = "Precio compra",
                DataPropertyName = nameof(LineaCompraDto.PrecioCompra),
                FillWeight = 14,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            _gridLineas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = ColumnaLineaPrecioVenta,
                HeaderText = "Precio venta",
                DataPropertyName = nameof(LineaCompraDto.PrecioVenta),
                FillWeight = 14,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            _gridLineas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = ColumnaLineaSubtotal,
                HeaderText = "Subtotal",
                DataPropertyName = nameof(LineaCompraDto.Subtotal),
                FillWeight = 14,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            _gridLineas.Columns.Add(new DataGridViewButtonColumn
            {
                Name = ColumnaLineaEditar,
                HeaderText = string.Empty,
                Text = "Editar",
                UseColumnTextForButtonValue = true,
                FillWeight = 7,
                FlatStyle = FlatStyle.Flat,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            _gridLineas.Columns.Add(new DataGridViewButtonColumn
            {
                Name = ColumnaLineaQuitar,
                HeaderText = string.Empty,
                Text = "Quitar",
                UseColumnTextForButtonValue = true,
                FillWeight = 7,
                FlatStyle = FlatStyle.Flat,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });

            // Total general: solo los subtotales de las líneas de la grilla
            // (RF-7 y RF-65). La línea en preparación no suma.
            _lblMontoTotal = new MaterialLabel
            {
                Text = "Total: 0,00",
                Location = new Point(20, 420),
                AutoSize = true,
                FontType = MaterialSkinManager.fontType.H6,
                ForeColor = Color.White,
                Depth = 0
            };

            FlowLayoutPanel panelAcciones = new FlowLayoutPanel
            {
                Location = new Point(300, 412),
                Width = 720,
                Height = 55,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoScroll = true,
                BackColor = TemaAplicacion.FondoPrincipal,
                Padding = new Padding(0, 2, 0, 2)
            };

            _btnRegistrar = new MaterialButton
            {
                Text = "Registrar compra",
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Height = 40,
                Type = MaterialButton.MaterialButtonType.Contained,
                HighEmphasis = true,
                UseAccentColor = true,
                Depth = 0,
                Margin = new Padding(4, 6, 4, 6)
            };

            _btnLimpiar = new MaterialButton
            {
                Text = "Cancelar compra",
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Height = 40,
                Type = MaterialButton.MaterialButtonType.Contained,
                HighEmphasis = true,
                UseAccentColor = false,
                Depth = 0,
                Margin = new Padding(4, 6, 4, 6)
            };

            panelAcciones.Controls.Add(_btnRegistrar);
            panelAcciones.Controls.Add(_btnLimpiar);

            // Historial de compras registradas (RF-49 … RF-52).
            _lblTituloHistorial = new MaterialLabel
            {
                Text = "Historial de compras",
                Location = new Point(20, 475),
                AutoSize = true,
                FontType = MaterialSkinManager.fontType.Subtitle1,
                ForeColor = Color.White,
                Depth = 0
            };

            _lblMensajeHistorial = new MaterialLabel
            {
                Location = new Point(210, 482),
                AutoSize = true,
                FontType = MaterialSkinManager.fontType.Body2,
                ForeColor = Color.Gainsboro,
                Depth = 0
            };

            _gridHistorial = CrearGrilla();
            _gridHistorial.Location = new Point(20, 505);
            _gridHistorial.Height = 180;
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colHistorialNumero",
                HeaderText = "N° Compra",
                DataPropertyName = nameof(HistorialCompraDto.IdCompra),
                FillWeight = 10
            });
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = ColumnaHistorialFecha,
                HeaderText = "Fecha y hora",
                DataPropertyName = nameof(HistorialCompraDto.FechaRegistro),
                FillWeight = 18
            });
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colHistorialProveedor",
                HeaderText = "Proveedor",
                DataPropertyName = nameof(HistorialCompraDto.RazonSocialProveedor),
                FillWeight = 30
            });
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colHistorialUsuario",
                HeaderText = "Usuario",
                DataPropertyName = nameof(HistorialCompraDto.NombreUsuario),
                FillWeight = 18
            });
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = ColumnaHistorialTotal,
                HeaderText = "Total",
                DataPropertyName = nameof(HistorialCompraDto.MontoTotal),
                FillWeight = 14
            });
            _gridHistorial.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = ColumnaHistorialEstado,
                HeaderText = "Estado",
                DataPropertyName = nameof(HistorialCompraDto.Estado),
                FillWeight = 10
            });

            panelPrincipal.Controls.Add(_lblTitulo);
            panelPrincipal.Controls.Add(_txtBuscarProveedor);
            panelPrincipal.Controls.Add(_cboResultadosProveedor);
            panelPrincipal.Controls.Add(_lblProveedorElegido);
            panelPrincipal.Controls.Add(panelCaptura);
            panelPrincipal.Controls.Add(_lblMensaje);
            panelPrincipal.Controls.Add(_gridLineas);
            panelPrincipal.Controls.Add(_lblMontoTotal);
            panelPrincipal.Controls.Add(panelAcciones);
            panelPrincipal.Controls.Add(_lblTituloHistorial);
            panelPrincipal.Controls.Add(_lblMensajeHistorial);
            panelPrincipal.Controls.Add(_gridHistorial);

            Controls.Add(panelPrincipal);

            // Eventos delgados (plan §10.3): cada manejador solo orquesta y
            // bindea; las reglas viven en CompraNegocio.
            _txtBuscarProveedor.TextChanged += txtBuscarProveedor_TextChanged;
            _txtBuscarProducto.TextChanged += txtBuscarProducto_TextChanged;
            _cboResultadosProveedor.SelectedIndexChanged += cboResultadosProveedor_SelectedIndexChanged;
            _cboResultadosProducto.SelectedIndexChanged += cboResultadosProducto_SelectedIndexChanged;
            _txtCantidad.TextChanged += captura_TextChanged;
            _txtPrecioCompra.TextChanged += captura_TextChanged;
            _txtPrecioVenta.TextChanged += captura_TextChanged;
            _btnAgregarLinea.Click += btnAgregarLinea_Click;
            _gridLineas.CellContentClick += gridLineas_CellContentClick;
            _gridLineas.CellFormatting += gridLineas_CellFormatting;
            _btnRegistrar.Click += btnRegistrar_Click;
            _btnLimpiar.Click += btnLimpiar_Click;
            _gridHistorial.CellFormatting += gridHistorial_CellFormatting;
        }

        /// <summary>
        /// Crea una grilla de solo lectura con el estilo oscuro compartido por las
        /// pantallas de la aplicación (RNF-7).
        /// </summary>
        /// <returns>Grilla configurada, sin columnas.</returns>
        private static DataGridView CrearGrilla()
        {
            DataGridView grilla = new DataGridView
            {
                Width = 1000,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
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

            grilla.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(55, 71, 79),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                SelectionBackColor = Color.FromArgb(55, 71, 79),
                SelectionForeColor = Color.White,
                WrapMode = DataGridViewTriState.True
            };
            grilla.ColumnHeadersHeight = 42;

            grilla.DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(48, 48, 48),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.White,
                SelectionBackColor = Color.FromArgb(255, 87, 34),
                SelectionForeColor = Color.White,
                WrapMode = DataGridViewTriState.False
            };
            grilla.GridColor = Color.FromArgb(80, 80, 80);
            grilla.RowTemplate.Height = 36;

            return grilla;
        }

        // =====================================================================
        // 2. BÚSQUEDAS Y PRELLENADO
        // =====================================================================

        private void txtBuscarProveedor_TextChanged(object? sender, EventArgs e)
        {
            _temporizadorProveedores.Stop();
            _temporizadorProveedores.Start();
        }

        private async void temporizadorProveedores_Tick(object? sender, EventArgs e)
        {
            _temporizadorProveedores.Stop();
            await EjecutarAsync(BuscarProveedoresAsync);
        }

        /// <summary>
        /// Consulta proveedores activos con el texto actual. Con el texto vacío no
        /// se invoca el negocio (RF-10).
        /// </summary>
        /// <returns>Tarea que representa la búsqueda.</returns>
        private async Task BuscarProveedoresAsync()
        {
            if (string.IsNullOrWhiteSpace(_txtBuscarProveedor.Text))
            {
                return;
            }

            RespuestaBusquedaProveedoresCompra respuesta =
                await _negocio.BuscarProveedoresAsync(_txtBuscarProveedor.Text);

            _proveedoresEncontrados.Clear();
            _proveedoresEncontrados.AddRange(respuesta.Proveedores);

            _actualizandoControles = true;

            try
            {
                _cboResultadosProveedor.Items.Clear();

                foreach (ProveedorSeleccionadoDto proveedor in _proveedoresEncontrados)
                {
                    _cboResultadosProveedor.Items.Add(
                        $"{proveedor.RazonSocial} — {proveedor.Documento}");
                }

                _cboResultadosProveedor.SelectedIndex = -1;
            }
            finally
            {
                _actualizandoControles = false;
            }

            _lblMensaje.Text = respuesta.Mensaje;
        }

        private void cboResultadosProveedor_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_actualizandoControles)
            {
                return;
            }

            int indice = _cboResultadosProveedor.SelectedIndex;

            if (indice < 0 || indice >= _proveedoresEncontrados.Count)
            {
                return;
            }

            // RF-12 y RF-13: fijar el proveedor no limpia las líneas en carga.
            _proveedorElegido = _proveedoresEncontrados[indice];
            _lblProveedorElegido.Text =
                $"Proveedor: {_proveedorElegido.RazonSocial} — {_proveedorElegido.Documento}";
        }

        private void txtBuscarProducto_TextChanged(object? sender, EventArgs e)
        {
            _temporizadorProductos.Stop();
            _temporizadorProductos.Start();
        }

        private async void temporizadorProductos_Tick(object? sender, EventArgs e)
        {
            _temporizadorProductos.Stop();
            await EjecutarAsync(BuscarProductosAsync);
        }

        /// <summary>
        /// Consulta productos activos con el texto actual. Con el texto vacío no se
        /// invoca el negocio (RF-16).
        /// </summary>
        /// <returns>Tarea que representa la búsqueda.</returns>
        private async Task BuscarProductosAsync()
        {
            if (string.IsNullOrWhiteSpace(_txtBuscarProducto.Text))
            {
                return;
            }

            RespuestaBusquedaProductosCompra respuesta =
                await _negocio.BuscarProductosAsync(_txtBuscarProducto.Text);

            _productosEncontrados.Clear();
            _productosEncontrados.AddRange(respuesta.Productos);

            _actualizandoControles = true;

            try
            {
                _cboResultadosProducto.Items.Clear();

                foreach (ProductoBuscadoDto producto in _productosEncontrados)
                {
                    _cboResultadosProducto.Items.Add($"{producto.Codigo} — {producto.Nombre}");
                }

                _cboResultadosProducto.SelectedIndex = -1;
            }
            finally
            {
                _actualizandoControles = false;
            }

            _lblMensaje.Text = respuesta.Mensaje;
        }

        private async void cboResultadosProducto_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_actualizandoControles)
            {
                return;
            }

            int indice = _cboResultadosProducto.SelectedIndex;

            if (indice < 0 || indice >= _productosEncontrados.Count)
            {
                return;
            }

            _productoElegido = _productosEncontrados[indice];
            int idProducto = _productoElegido.IdProducto;

            await EjecutarAsync(() => PrellenarPreciosAsync(idProducto));
        }

        /// <summary>
        /// Propone los precios actuales del producto en el área de captura. El
        /// usuario puede corregirlos antes de agregar la línea (RF-38 y RF-39).
        /// </summary>
        /// <param name="idProducto">Producto elegido en la búsqueda.</param>
        /// <returns>Tarea que representa la consulta del prellenado.</returns>
        private async Task PrellenarPreciosAsync(int idProducto)
        {
            RespuestaPreciosProducto respuesta =
                await _negocio.ObtenerPreciosProductoAsync(idProducto);

            _lblMensaje.Text = respuesta.Mensaje;

            if (respuesta.Precios is null)
            {
                return;
            }

            _txtPrecioCompra.Text =
                respuesta.Precios.PrecioCompra.ToString("0.00", CultureInfo.CurrentCulture);
            _txtPrecioVenta.Text =
                respuesta.Precios.PrecioVenta.ToString("0.00", CultureInfo.CurrentCulture);
        }

        // =====================================================================
        // 3. ÁREA DE CAPTURA Y GRILLA DE LÍNEAS
        // =====================================================================

        private void captura_TextChanged(object? sender, EventArgs e)
        {
            ActualizarSubtotalPreparacion();
        }

        /// <summary>
        /// Muestra el subtotal de la línea en preparación. Se calcula solo con el
        /// negocio y vale «—» mientras la entrada no sea válida (RF-65 / D-21).
        /// </summary>
        private void ActualizarSubtotalPreparacion()
        {
            SubtotalLineaDto subtotal = _negocio.CalcularSubtotalEnPreparacion(
                _productoElegido?.IdProducto,
                _txtCantidad.Text,
                _txtPrecioCompra.Text,
                _txtPrecioVenta.Text);

            _lblSubtotalPreparacion.Text =
                subtotal.EsValido && subtotal.Subtotal.HasValue
                    ? $"Subtotal: {subtotal.Subtotal.Value.ToString("N2", CultureInfo.CurrentCulture)}"
                    : TextoSubtotalInvalido;
        }

        private async void btnAgregarLinea_Click(object? sender, EventArgs e)
        {
            await EjecutarAsync(GuardarLineaAsync);
        }

        /// <summary>
        /// Agrega una línea nueva o guarda la corrección de la línea en edición,
        /// según el estado del área de captura, y rebindea la grilla con la lista
        /// que devuelve el negocio.
        /// </summary>
        /// <returns>Tarea que representa la operación de línea.</returns>
        private async Task GuardarLineaAsync()
        {
            IReadOnlyList<LineaCompraDto> lineasActuales = _lineas.ToList();
            int idProducto = _productoElegido?.IdProducto ?? 0;
            RespuestaOperacionLineaCompra respuesta;

            if (_indiceEdicion is int indiceEdicion)
            {
                respuesta = await _negocio.EditarLineaAsync(new SolicitudEditarLineaCompraDto
                {
                    IndiceLinea = indiceEdicion,
                    IdProducto = idProducto,
                    Cantidad = _txtCantidad.Text,
                    PrecioCompra = _txtPrecioCompra.Text,
                    PrecioVenta = _txtPrecioVenta.Text,
                    LineasActuales = lineasActuales
                });
            }
            else
            {
                respuesta = await _negocio.AgregarLineaAsync(new SolicitudAgregarLineaCompraDto
                {
                    IdProducto = idProducto,
                    Cantidad = _txtCantidad.Text,
                    PrecioCompra = _txtPrecioCompra.Text,
                    PrecioVenta = _txtPrecioVenta.Text,
                    LineasActuales = lineasActuales
                });
            }

            _lblMensaje.Text = respuesta.Mensaje;

            bool esExito =
                respuesta.Resultado == ResultadoOperacionLineaCompra.Exito
                || respuesta.Resultado == ResultadoOperacionLineaCompra.ExitoConAcumulacion;

            if (!esExito)
            {
                // En rechazo se conserva la captura y la grilla (RNF-11).
                return;
            }

            _lineas.Clear();
            _lineas.AddRange(respuesta.Lineas);
            RefrescarGrillaLineas();
            LimpiarCaptura();
        }

        private void gridLineas_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columna = _gridLineas.Columns[e.ColumnIndex].Name;

            if (columna == ColumnaLineaQuitar)
            {
                QuitarLineaDeGrilla(e.RowIndex);
            }
            else if (columna == ColumnaLineaEditar)
            {
                EditarLineaEnCaptura(e.RowIndex);
            }
        }

        /// <summary>
        /// Quita la línea indicada con el resultado del negocio: si falla muestra
        /// su mensaje y no toca la grilla (RF-29).
        /// </summary>
        /// <param name="indice">Fila de la grilla seleccionada.</param>
        private void QuitarLineaDeGrilla(int indice)
        {
            RespuestaCompraEnCarga respuesta = _negocio.QuitarLinea(_lineas.ToList(), indice);

            _lblMensaje.Text = respuesta.Mensaje;

            if (!respuesta.Exito)
            {
                return;
            }

            _lineas.Clear();
            _lineas.AddRange(respuesta.Lineas);

            if (_indiceEdicion is int indiceEditado && indiceEditado >= indice)
            {
                LimpiarCaptura();
            }

            RefrescarGrillaLineas();
        }

        /// <summary>
        /// Carga en el área de captura la línea indicada y activa el modo edición
        /// (RF-30 y RF-62). No consulta la base ni recalcula importes.
        /// </summary>
        /// <param name="indice">Fila de la grilla seleccionada.</param>
        private void EditarLineaEnCaptura(int indice)
        {
            LineaCompraDto linea = _lineas[indice];

            _indiceEdicion = indice;
            _productoElegido = new ProductoBuscadoDto
            {
                IdProducto = linea.IdProducto,
                Codigo = linea.CodigoProducto,
                Nombre = linea.NombreProducto
            };

            _productosEncontrados.Clear();
            _productosEncontrados.Add(_productoElegido);

            _actualizandoControles = true;

            try
            {
                _cboResultadosProducto.Items.Clear();
                _cboResultadosProducto.Items.Add($"{linea.CodigoProducto} — {linea.NombreProducto}");
                _cboResultadosProducto.SelectedIndex = 0;
            }
            finally
            {
                _actualizandoControles = false;
            }

            _txtCantidad.Text = linea.Cantidad.ToString(CultureInfo.CurrentCulture);
            _txtPrecioCompra.Text = linea.PrecioCompra.ToString("0.00", CultureInfo.CurrentCulture);
            _txtPrecioVenta.Text = linea.PrecioVenta.ToString("0.00", CultureInfo.CurrentCulture);
            _btnAgregarLinea.Text = "Guardar línea";
            ActualizarSubtotalPreparacion();
        }

        /// <summary>
        /// Vuelve a enlazar la grilla de líneas y actualiza el total general con la
        /// única fórmula del negocio sobre los subtotales de las líneas (RF-7).
        /// </summary>
        private void RefrescarGrillaLineas()
        {
            _gridLineas.DataSource = null;
            _gridLineas.DataSource = _lineas.ToList();

            decimal montoTotal =
                CalculoImportesCompra.CalcularMontoTotal(_lineas.Select(linea => linea.Subtotal));

            _lblMontoTotal.Text = $"Total: {montoTotal.ToString("N2", CultureInfo.CurrentCulture)}";
        }

        // =====================================================================
        // 4. REGISTRO, LIMPIEZA E HISTORIAL
        // =====================================================================

        private async void btnRegistrar_Click(object? sender, EventArgs e)
        {
            await EjecutarAsync(RegistrarCompraAsync);
        }

        /// <summary>
        /// Registra la compra con el proveedor elegido y las líneas de la grilla.
        /// Mientras dura la operación el botón queda deshabilitado y la bandera
        /// ignora reentradas (RF-58). En éxito descarta la compra en carga (RF-32)
        /// y recarga el historial (RF-52); en rechazo conserva todo (RNF-11).
        /// </summary>
        /// <returns>Tarea que representa el registro de la compra.</returns>
        private async Task RegistrarCompraAsync()
        {
            if (_registrando)
            {
                return;
            }

            _registrando = true;
            _btnRegistrar.Enabled = false;

            try
            {
                SolicitudRegistrarCompraDto solicitud = new()
                {
                    IdProveedor = _proveedorElegido?.IdProveedor,
                    Lineas = _lineas.ToList()
                };

                RespuestaRegistroCompra respuesta =
                    await _negocio.RegistrarCompraAsync(solicitud, _idUsuario, _rolActual);

                _lblMensaje.Text = respuesta.Mensaje;

                if (respuesta.Resultado != ResultadoRegistroCompra.Exito)
                {
                    return;
                }

                LimpiarTrasRegistro();
                await CargarHistorialAsync();
            }
            finally
            {
                _registrando = false;
                _btnRegistrar.Enabled = PoliticaAcceso.PuedeOperarCompras(_rolActual);
            }
        }

        private void btnLimpiar_Click(object? sender, EventArgs e)
        {
            if (FindForm() is not Form propietario)
            {
                return;
            }

            // El texto de la confirmación lo redacta el negocio (RF-31).
            DialogResult resultado = MaterialMessageBox.Show(
                propietario,
                _negocio.ObtenerMensajeConfirmacionCancelarCompra(),
                "Cancelar compra",
                MessageBoxButtons.YesNo,
                false,
                FlexibleMaterialForm.ButtonsPosition.Right);

            if (resultado != DialogResult.Yes)
            {
                return;
            }

            LimpiarTrasRegistro();
            _lblMensaje.Text = string.Empty;
        }

        /// <summary>
        /// Descarta el proveedor elegido, las líneas y el área de captura, tanto
        /// tras registrar (RF-32) como tras confirmar la limpieza (RF-31).
        /// </summary>
        private void LimpiarTrasRegistro()
        {
            _proveedorElegido = null;
            _proveedoresEncontrados.Clear();

            _actualizandoControles = true;

            try
            {
                _cboResultadosProveedor.Items.Clear();
                _cboResultadosProveedor.SelectedIndex = -1;
                _txtBuscarProveedor.Text = string.Empty;
            }
            finally
            {
                _actualizandoControles = false;
            }

            _lblProveedorElegido.Text = TextoProveedorSinElegir;
            _lineas.Clear();
            LimpiarCaptura();
            RefrescarGrillaLineas();
        }

        /// <summary>
        /// Deja el área de captura vacía y sin modo edición.
        /// </summary>
        private void LimpiarCaptura()
        {
            _indiceEdicion = null;
            _productoElegido = null;
            _productosEncontrados.Clear();

            _actualizandoControles = true;

            try
            {
                _cboResultadosProducto.Items.Clear();
                _cboResultadosProducto.SelectedIndex = -1;
                _txtBuscarProducto.Text = string.Empty;
                _txtCantidad.Text = string.Empty;
                _txtPrecioCompra.Text = string.Empty;
                _txtPrecioVenta.Text = string.Empty;
            }
            finally
            {
                _actualizandoControles = false;
            }

            _btnAgregarLinea.Text = "Agregar línea";
            ActualizarSubtotalPreparacion();
        }

        // =====================================================================
        // 5. FORMATO DE PRESENTACIÓN (RNF-12)
        // =====================================================================

        private void gridLineas_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columna = _gridLineas.Columns[e.ColumnIndex].Name;

            if (columna == ColumnaLineaProducto
                && e.Value is string nombreProducto
                && _gridLineas.Rows[e.RowIndex].DataBoundItem is LineaCompraDto linea)
            {
                e.Value = $"{linea.CodigoProducto} — {nombreProducto}";
                e.FormattingApplied = true;
                return;
            }

            bool esImporte =
                columna == ColumnaLineaPrecioCompra
                || columna == ColumnaLineaPrecioVenta
                || columna == ColumnaLineaSubtotal;

            if (esImporte && e.Value is decimal importe)
            {
                e.Value = importe.ToString("N2", CultureInfo.CurrentCulture);
                e.FormattingApplied = true;
            }
        }

        private void gridHistorial_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columna = _gridHistorial.Columns[e.ColumnIndex].Name;

            if (columna == ColumnaHistorialFecha && e.Value is DateTime fecha)
            {
                e.Value = fecha.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture);
                e.FormattingApplied = true;
            }
            else if (columna == ColumnaHistorialTotal && e.Value is decimal total)
            {
                e.Value = total.ToString("N2", CultureInfo.CurrentCulture);
                e.FormattingApplied = true;
            }
            else if (columna == ColumnaHistorialEstado && e.Value is bool estado)
            {
                e.Value = estado ? "Activa" : "Inactiva";
                e.FormattingApplied = true;
            }
        }

        // =====================================================================
        // 6. AYUDA COMÚN DE LOS MANEJADORES
        // =====================================================================

        /// <summary>
        /// Ejecuta una operación asíncrona de la pantalla sin bloquear el hilo de
        /// interfaz y sin propagar una excepción inesperada desde un manejador
        /// <c>async void</c>.
        /// </summary>
        /// <param name="operacion">Operación a ejecutar.</param>
        /// <returns>Tarea que representa la operación.</returns>
        private async Task EjecutarAsync(Func<Task> operacion)
        {
            try
            {
                await operacion();
            }
            catch (Exception excepcion)
            {
                _lblMensaje.Text = excepcion.Message;
            }
        }
    }
}
