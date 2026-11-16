using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaDatos;
using CapaNegocio;
using CapaNegocio.DTOs.Clientes;
using CapaNegocio.DTOs.Ventas;
using CapaPresentacion.Tema;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CapaPresentacion.Ventas
{
    public partial class VentasForm : UserControl
    {
        private readonly CarritoVentaNegocio _carritoVentaNegocio;
        private readonly VentaNegocio _ventaNegocio;
        private readonly ClienteNegocio _clienteNegocio;
        private readonly BusquedaProductosVenta _busqueda;
        private readonly System.Windows.Forms.Timer _temporizadorBusqueda;
        private readonly System.Windows.Forms.Timer _temporizadorCliente;

        private MaterialCard _cardCarga = null!;
        private ClienteRespuestaDto? _clienteSeleccionado;
        private MaterialTextBox2 _txtCliente = null!;
        private MaterialButton _btnConsumidorFinal = null!;
        private MaterialLabel _lblClienteSeleccionado = null!;
        private ListBox _listaClientes = null!;
        private bool _buscandoCliente;
        private bool _busquedaClientePendiente;
        private bool _actualizandoTextoCliente;

        private MaterialTextBox2 _txtCodigo = null!;
        private MaterialTextBox2 _txtCantidad = null!;
        private DataGridView _grillaProductos = null!;
        private MaterialLabel _lblEstadoBusqueda = null!;
        private ProductoVentaDto? _productoSeleccionado;

        private readonly EstadoCarritoVenta _estadoCarrito = new();
        private MaterialButton _btnAgregar = null!;
        private MaterialButton _btnQuitar = null!;
        private DataGridView _grillaCarrito = null!;
        private MaterialLabel _lblTotal = null!;
        private MaterialLabel _lblEstadoCarrito = null!;

        private MaterialTextBox2 _txtPagaCon = null!;
        private MaterialLabel _lblVuelto = null!;
        private MaterialButton _btnCobrar = null!;
        private MaterialButton _btnVerHistorial = null!;

        private TableLayoutPanel _layoutPos = null!;
        private TableLayoutPanel _layoutHistorial = null!;
        private DataGridView _grillaHistorialVentas = null!;
        private DataGridView _grillaDetalleHistorial = null!;
        private MaterialLabel _lblEstadoHistorial = null!;
        private MaterialLabel _lblResumenSeleccionada = null!;
        private TableLayoutPanel _panelAnulacion = null!;
        private MaterialTextBox2 _txtMotivoAnulacion = null!;
        private MaterialButton _btnAnularVenta = null!;
        private MaterialButton _btnActualizarHistorial = null!;
        private MaterialButton _btnVolverACaja = null!;
        private ResumenVentaDatos? _ventaHistorialSeleccionada;
        private bool _cargandoHistorial;

        private bool _ocupadoCarrito;
        private bool _pintandoCarrito;
        private int _idUsuarioActual;
        private string? _rolActual;

        public void AplicarSesion(int idUsuario, string? rol = null)
        {
            _idUsuarioActual = idUsuario;
            _rolActual = rol;

            bool puedeOperarCaja = string.IsNullOrWhiteSpace(_rolActual) || PoliticaAcceso.PuedeOperarVentas(_rolActual);
            bool puedeAnular = PoliticaAcceso.PuedeAnularOperaciones(_rolActual);

            _panelAnulacion.Visible = puedeAnular;
            _btnVolverACaja.Visible = puedeOperarCaja;

            if (puedeOperarCaja)
            {
                _layoutHistorial.Visible = false;
                _layoutPos.Visible = true;
            }
            else
            {
                _layoutPos.Visible = false;
                _layoutHistorial.Visible = true;
                _ = CargarHistorialVentasAsync();
            }
        }

        public VentasForm(
            CarritoVentaNegocio carritoVentaNegocio,
            VentaNegocio ventaNegocio,
            ClienteNegocio clienteNegocio)
        {
            _carritoVentaNegocio = carritoVentaNegocio;
            _ventaNegocio = ventaNegocio;
            _clienteNegocio = clienteNegocio;
            _busqueda = new BusquedaProductosVenta(_carritoVentaNegocio.BuscarProductosAsync);

            InitializeComponent();
            components ??= new Container();
            _temporizadorBusqueda = new System.Windows.Forms.Timer(components) { Interval = 300 };
            _temporizadorCliente = new System.Windows.Forms.Timer(components) { Interval = 300 };

            BackColor = TemaAplicacion.FondoPrincipal;
            ArmarInterfazMaterial();
            ConfigurarBusqueda();
            ConfigurarCarrito();
            ConfigurarCliente();
            ConfigurarCobro();
            ConfigurarHistorial();
            PintarCarrito();
            ActualizarControlesCarrito();

            Disposed += (_, _) => _busqueda.Invalidar();
        }

        private void ArmarInterfazMaterial()
        {
            Controls.Clear();

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10),
                BackColor = TemaAplicacion.FondoPrincipal
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));

            MaterialCard cardCarga = new MaterialCard
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(5),
                Padding = new Padding(15)
            };
            _cardCarga = cardCarga;

            var layoutCarga = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 7,
                Margin = Padding.Empty
            };
            layoutCarga.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutCarga.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            layoutCarga.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));
            layoutCarga.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            layoutCarga.RowStyles.Add(new RowStyle(SizeType.Absolute, 65F));
            layoutCarga.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            layoutCarga.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutCarga.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));

            MaterialLabel lblIngreso = new MaterialLabel
            {
                Text = "🛒 INGRESO DE PRODUCTOS",
                FontType = MaterialSkinManager.fontType.H6,
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            var layoutCliente = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty
            };
            layoutCliente.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            layoutCliente.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));

            _txtCliente = new MaterialTextBox2
            {
                Hint = "DNI o Nombre Cliente",
                Dock = DockStyle.Top,
                Margin = new Padding(0, 3, 6, 3),
                AccessibleName = "Buscar cliente por DNI o nombre"
            };

            _btnConsumidorFinal = new MaterialButton
            {
                Text = "Cons. Final",
                Dock = DockStyle.Fill,
                Type = MaterialButton.MaterialButtonType.Outlined,
                NoAccentTextColor = Color.FromArgb(224, 224, 224),
                Margin = new Padding(0, 3, 0, 3)
            };
            layoutCliente.Controls.Add(_txtCliente, 0, 0);
            layoutCliente.Controls.Add(_btnConsumidorFinal, 1, 0);

            _lblClienteSeleccionado = new MaterialLabel
            {
                Text = "Cliente: Consumidor Final",
                Dock = DockStyle.Fill,
                FontType = MaterialSkinManager.fontType.Body2,
                UseAccent = true
            };

            var layoutBusqueda = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty
            };
            layoutBusqueda.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 73F));
            layoutBusqueda.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27F));

            _txtCodigo = new MaterialTextBox2
            {
                Hint = "Código de barras o nombre",
                Dock = DockStyle.Top,
                Margin = new Padding(0, 3, 6, 3),
                TabIndex = 0,
                AccessibleName = "Buscar producto por código de barras o nombre"
            };

            _txtCantidad = new MaterialTextBox2
            {
                Hint = "Cant.",
                Text = "1",
                Dock = DockStyle.Top,
                Margin = new Padding(0, 3, 0, 3),
                TabIndex = 1,
                AccessibleName = "Cantidad"
            };
            layoutBusqueda.Controls.Add(_txtCodigo, 0, 0);
            layoutBusqueda.Controls.Add(_txtCantidad, 1, 0);

            _lblEstadoBusqueda = new MaterialLabel
            {
                Text = "Escriba un código o nombre para buscar.",
                Dock = DockStyle.Fill,
                AutoSize = false,
                AccessibleName = "Estado de la búsqueda"
            };

            _grillaProductos = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.FromArgb(40, 40, 40),
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                TabIndex = 3,
                AccessibleName = "Productos encontrados"
            };
            _grillaProductos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _grillaProductos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            _grillaProductos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _grillaProductos.ColumnHeadersDefaultCellStyle.Font = new Font("Roboto", 9, FontStyle.Bold);
            _grillaProductos.DefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
            _grillaProductos.DefaultCellStyle.ForeColor = Color.White;
            _grillaProductos.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Código", DataPropertyName = nameof(ProductoVentaDto.Codigo), FillWeight = 25
            });
            _grillaProductos.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Producto", DataPropertyName = nameof(ProductoVentaDto.Nombre), FillWeight = 40
            });
            _grillaProductos.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Stock", DataPropertyName = nameof(ProductoVentaDto.Stock), FillWeight = 12
            });
            _grillaProductos.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Precio", DataPropertyName = nameof(ProductoVentaDto.PrecioVenta), FillWeight = 23,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" }
            });

            _btnAgregar = new MaterialButton
            {
                Text = "➕ Agregar al Carrito",
                Dock = DockStyle.Fill,
                Enabled = false,
                Type = MaterialButton.MaterialButtonType.Contained
            };

            layoutCarga.Controls.Add(lblIngreso, 0, 0);
            layoutCarga.Controls.Add(layoutCliente, 0, 1);
            layoutCarga.Controls.Add(_lblClienteSeleccionado, 0, 2);
            layoutCarga.Controls.Add(layoutBusqueda, 0, 3);
            layoutCarga.Controls.Add(_lblEstadoBusqueda, 0, 4);
            layoutCarga.Controls.Add(_grillaProductos, 0, 5);
            layoutCarga.Controls.Add(_btnAgregar, 0, 6);

            _listaClientes = new ListBox
            {
                Visible = false,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Roboto", 9.5F),
                IntegralHeight = false,
                AccessibleName = "Coincidencias de clientes"
            };

            cardCarga.Controls.Add(_listaClientes);
            cardCarga.Controls.Add(layoutCarga);
            _listaClientes.BringToFront();

            MaterialCard cardCarrito = new MaterialCard
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(5),
                Padding = new Padding(15)
            };

            var layoutCarrito = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 7,
                Margin = Padding.Empty
            };
            layoutCarrito.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutCarrito.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            layoutCarrito.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutCarrito.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            layoutCarrito.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            layoutCarrito.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            layoutCarrito.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            layoutCarrito.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));

            var layoutEncabezadoCarrito = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty
            };
            layoutEncabezadoCarrito.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            layoutEncabezadoCarrito.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));

            MaterialLabel lblCarrito = new MaterialLabel
            {
                Text = "📋 DETALLE DEL CARRITO",
                FontType = MaterialSkinManager.fontType.H6,
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            _btnVerHistorial = new MaterialButton
            {
                Text = "📜 Historial",
                Dock = DockStyle.Fill,
                Type = MaterialButton.MaterialButtonType.Outlined,
                NoAccentTextColor = Color.FromArgb(224, 224, 224),
                Margin = new Padding(4, 0, 0, 0)
            };
            layoutEncabezadoCarrito.Controls.Add(lblCarrito, 0, 0);
            layoutEncabezadoCarrito.Controls.Add(_btnVerHistorial, 1, 0);

            _grillaCarrito = new GrillaCarritoVenta
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.FromArgb(40, 40, 40),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                AccessibleName = "Carrito de venta; la cantidad se puede editar"
            };

            _grillaCarrito.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            _grillaCarrito.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _grillaCarrito.ColumnHeadersDefaultCellStyle.Font = new Font("Roboto", 9, FontStyle.Bold);
            _grillaCarrito.DefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
            _grillaCarrito.DefaultCellStyle.ForeColor = Color.White;
            _grillaCarrito.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Cant.", ValueType = typeof(string), FillWeight = 12,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            _grillaCarrito.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Producto", ReadOnly = true, FillWeight = 42,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            foreach (string titulo in new[] { "P. Unit.", "Subtotal" })
            {
                _grillaCarrito.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = titulo, ReadOnly = true, ValueType = typeof(decimal), FillWeight = 23,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" }
                });
            }

            _btnQuitar = new MaterialButton
            {
                Text = "Quitar seleccionado",
                Dock = DockStyle.Fill,
                Enabled = false,
                Type = MaterialButton.MaterialButtonType.Outlined,
                NoAccentTextColor = Color.FromArgb(224, 224, 224)
            };

            _lblEstadoCarrito = new MaterialLabel
            {
                Text = "Seleccione un producto y agréguelo al carrito.",
                Dock = DockStyle.Fill,
                AutoSize = false,
                AccessibleName = "Estado del carrito"
            };

            _lblTotal = new MaterialLabel
            {
                FontType = MaterialSkinManager.fontType.H4,
                Dock = DockStyle.Fill,
                AutoSize = true,
                UseAccent = true
            };

            var layoutPago = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty
            };
            layoutPago.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layoutPago.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            _txtPagaCon = new MaterialTextBox2
            {
                Hint = "Paga con $",
                Dock = DockStyle.Top,
                Margin = new Padding(0, 3, 6, 3),
                AccessibleName = "Monto con el que paga el cliente"
            };

            _lblVuelto = new MaterialLabel
            {
                Text = "Vuelto: $0.00",
                FontType = MaterialSkinManager.fontType.H6,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                UseAccent = true,
                AccessibleName = "Vuelto calculado"
            };
            layoutPago.Controls.Add(_txtPagaCon, 0, 0);
            layoutPago.Controls.Add(_lblVuelto, 1, 0);

            _btnCobrar = new MaterialButton
            {
                Text = "💳 CONFIRMAR Y FACTURAR",
                Dock = DockStyle.Fill,
                Enabled = false,
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true
            };

            layoutCarrito.Controls.Add(layoutEncabezadoCarrito, 0, 0);
            layoutCarrito.Controls.Add(_grillaCarrito, 0, 1);
            layoutCarrito.Controls.Add(_btnQuitar, 0, 2);
            layoutCarrito.Controls.Add(_lblEstadoCarrito, 0, 3);
            layoutCarrito.Controls.Add(_lblTotal, 0, 4);
            layoutCarrito.Controls.Add(layoutPago, 0, 5);
            layoutCarrito.Controls.Add(_btnCobrar, 0, 6);
            cardCarrito.Controls.Add(layoutCarrito);

            layout.Controls.Add(cardCarga, 0, 0);
            layout.Controls.Add(cardCarrito, 1, 0);

            _layoutPos = layout;
            _layoutHistorial = ConstruirPanelHistorial();

            Controls.Add(_layoutHistorial);
            Controls.Add(_layoutPos);
        }

        private TableLayoutPanel ConstruirPanelHistorial()
        {
            var layoutHist = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10),
                BackColor = TemaAplicacion.FondoPrincipal,
                Visible = false
            };
            layoutHist.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            layoutHist.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));

            var cardLista = new MaterialCard
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(5),
                Padding = new Padding(15)
            };

            var layoutLista = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Margin = Padding.Empty
            };
            layoutLista.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutLista.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            layoutLista.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutLista.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));

            var barraSuperior = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = Padding.Empty
            };
            barraSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            barraSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            barraSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145F));

            var lblTituloHist = new MaterialLabel
            {
                Text = "📜 HISTORIAL DE VENTAS",
                FontType = MaterialSkinManager.fontType.H6,
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            _btnActualizarHistorial = new MaterialButton
            {
                Text = "🔄 Actualizar",
                Dock = DockStyle.Fill,
                Type = MaterialButton.MaterialButtonType.Outlined,
                NoAccentTextColor = Color.FromArgb(224, 224, 224),
                Margin = new Padding(4, 2, 4, 2)
            };

            _btnVolverACaja = new MaterialButton
            {
                Text = "⬅ Volver a Caja",
                Dock = DockStyle.Fill,
                Type = MaterialButton.MaterialButtonType.Contained,
                Margin = new Padding(4, 2, 0, 2)
            };

            barraSuperior.Controls.Add(lblTituloHist, 0, 0);
            barraSuperior.Controls.Add(_btnActualizarHistorial, 1, 0);
            barraSuperior.Controls.Add(_btnVolverACaja, 2, 0);

            _grillaHistorialVentas = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.FromArgb(40, 40, 40),
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                AccessibleName = "Historial de ventas registradas"
            };
            _grillaHistorialVentas.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            _grillaHistorialVentas.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _grillaHistorialVentas.ColumnHeadersDefaultCellStyle.Font = new Font("Roboto", 9, FontStyle.Bold);
            _grillaHistorialVentas.DefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
            _grillaHistorialVentas.DefaultCellStyle.ForeColor = Color.White;
            _grillaHistorialVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Comprobante", DataPropertyName = nameof(ResumenVentaDatos.NroComprobante), FillWeight = 20
            });
            _grillaHistorialVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Fecha", DataPropertyName = nameof(ResumenVentaDatos.FechaRegistro), FillWeight = 22,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
            });
            _grillaHistorialVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Cliente", DataPropertyName = nameof(ResumenVentaDatos.NombreCliente), FillWeight = 26
            });
            _grillaHistorialVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Total", DataPropertyName = nameof(ResumenVentaDatos.MontoTotal), FillWeight = 17,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" }
            });
            _grillaHistorialVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colEstadoVenta", HeaderText = "Estado", DataPropertyName = nameof(ResumenVentaDatos.Estado), FillWeight = 15
            });

            _lblEstadoHistorial = new MaterialLabel
            {
                Text = "Seleccione una venta para ver su detalle.",
                Dock = DockStyle.Fill,
                AutoSize = false
            };

            layoutLista.Controls.Add(barraSuperior, 0, 0);
            layoutLista.Controls.Add(_grillaHistorialVentas, 0, 1);
            layoutLista.Controls.Add(_lblEstadoHistorial, 0, 2);
            cardLista.Controls.Add(layoutLista);

            var cardDetalle = new MaterialCard
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(5),
                Padding = new Padding(15)
            };

            var layoutDetalle = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Margin = Padding.Empty
            };
            layoutDetalle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutDetalle.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            layoutDetalle.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
            layoutDetalle.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutDetalle.RowStyles.Add(new RowStyle(SizeType.Absolute, 118F));

            var lblTituloDet = new MaterialLabel
            {
                Text = "🧾 DETALLE DEL COMPROBANTE",
                FontType = MaterialSkinManager.fontType.H6,
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            _lblResumenSeleccionada = new MaterialLabel
            {
                Text = "Ninguna venta seleccionada.",
                Dock = DockStyle.Fill,
                AutoSize = false,
                FontType = MaterialSkinManager.fontType.Body2,
                UseAccent = true
            };

            _grillaDetalleHistorial = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.FromArgb(40, 40, 40),
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                AccessibleName = "Renglones del comprobante seleccionado"
            };
            _grillaDetalleHistorial.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            _grillaDetalleHistorial.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _grillaDetalleHistorial.ColumnHeadersDefaultCellStyle.Font = new Font("Roboto", 9, FontStyle.Bold);
            _grillaDetalleHistorial.DefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
            _grillaDetalleHistorial.DefaultCellStyle.ForeColor = Color.White;
            _grillaDetalleHistorial.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Código", DataPropertyName = nameof(DetalleLineaVentaDatos.Codigo), FillWeight = 22
            });
            _grillaDetalleHistorial.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Producto", DataPropertyName = nameof(DetalleLineaVentaDatos.NombreProducto), FillWeight = 38
            });
            _grillaDetalleHistorial.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Cant.", DataPropertyName = nameof(DetalleLineaVentaDatos.Cantidad), FillWeight = 12
            });
            _grillaDetalleHistorial.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "P. Unit.", DataPropertyName = nameof(DetalleLineaVentaDatos.PrecioVenta), FillWeight = 14,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" }
            });
            _grillaDetalleHistorial.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Subtotal", DataPropertyName = nameof(DetalleLineaVentaDatos.Subtotal), FillWeight = 14,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" }
            });

            _panelAnulacion = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = Padding.Empty,
                Visible = false
            };
            _panelAnulacion.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _panelAnulacion.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            _panelAnulacion.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));

            _txtMotivoAnulacion = new MaterialTextBox2
            {
                Hint = "Motivo de anulación (obligatorio para Encargado)",
                Dock = DockStyle.Top,
                Margin = new Padding(0, 4, 0, 4),
                AccessibleName = "Motivo de anulación de la venta"
            };

            _btnAnularVenta = new MaterialButton
            {
                Text = "🚫 ANULAR VENTA Y RESTITUIR STOCK",
                Dock = DockStyle.Fill,
                Enabled = false,
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true
            };

            _panelAnulacion.Controls.Add(_txtMotivoAnulacion, 0, 0);
            _panelAnulacion.Controls.Add(_btnAnularVenta, 0, 1);

            layoutDetalle.Controls.Add(lblTituloDet, 0, 0);
            layoutDetalle.Controls.Add(_lblResumenSeleccionada, 0, 1);
            layoutDetalle.Controls.Add(_grillaDetalleHistorial, 0, 2);
            layoutDetalle.Controls.Add(_panelAnulacion, 0, 3);
            cardDetalle.Controls.Add(layoutDetalle);

            layoutHist.Controls.Add(cardLista, 0, 0);
            layoutHist.Controls.Add(cardDetalle, 1, 0);
            return layoutHist;
        }

        private void ConfigurarHistorial()
        {
            _btnVerHistorial.Click += async (_, _) =>
            {
                _layoutPos.Visible = false;
                _layoutHistorial.Visible = true;
                await CargarHistorialVentasAsync();
            };

            _btnVolverACaja.Click += (_, _) =>
            {
                if (!PoliticaAcceso.PuedeOperarVentas(_rolActual) && !string.IsNullOrWhiteSpace(_rolActual))
                    return;

                _layoutHistorial.Visible = false;
                _layoutPos.Visible = true;
            };

            _btnActualizarHistorial.Click += async (_, _) => await CargarHistorialVentasAsync();

            _grillaHistorialVentas.CellFormatting += (_, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0 &&
                    _grillaHistorialVentas.Columns[e.ColumnIndex].Name == "colEstadoVenta" &&
                    e.Value is bool activa)
                {
                    e.Value = activa ? "Activa" : "Anulada";
                    e.FormattingApplied = true;
                }
            };

            _grillaHistorialVentas.SelectionChanged += async (_, _) => await CargarDetalleVentaSeleccionadaAsync();

            _btnAnularVenta.Click += async (_, _) => await ConfirmarAnulacionVentaAsync();
        }

        private async Task CargarHistorialVentasAsync()
        {
            if (_cargandoHistorial || IsDisposed || Disposing) return;
            _cargandoHistorial = true;
            _btnActualizarHistorial.Enabled = false;
            _lblEstadoHistorial.Text = "Cargando historial de ventas...";

            try
            {
                var ventas = await _ventaNegocio.ObtenerHistorialVentasAsync();
                if (IsDisposed || Disposing) return;

                _grillaHistorialVentas.DataSource = ventas.ToList();
                _lblEstadoHistorial.Text = ventas.Count == 0
                    ? "No hay ventas registradas todavía."
                    : $"{ventas.Count} venta(s) registrada(s).";

                if (ventas.Count == 0)
                {
                    _ventaHistorialSeleccionada = null;
                    _grillaDetalleHistorial.DataSource = null;
                    _lblResumenSeleccionada.Text = "Ninguna venta seleccionada.";
                    _btnAnularVenta.Enabled = false;
                }
            }
            finally
            {
                _cargandoHistorial = false;
                if (!IsDisposed && !Disposing)
                    _btnActualizarHistorial.Enabled = true;
            }
        }

        private async Task CargarDetalleVentaSeleccionadaAsync()
        {
            if (IsDisposed || Disposing) return;

            _ventaHistorialSeleccionada = _grillaHistorialVentas.SelectedRows.Count == 1
                ? _grillaHistorialVentas.SelectedRows[0].DataBoundItem as ResumenVentaDatos
                : null;

            if (_ventaHistorialSeleccionada is null)
            {
                _grillaDetalleHistorial.DataSource = null;
                _lblResumenSeleccionada.Text = "Ninguna venta seleccionada.";
                _btnAnularVenta.Enabled = false;
                return;
            }

            string estadoTexto = _ventaHistorialSeleccionada.Estado
                ? "ACTIVA"
                : $"ANULADA ({_ventaHistorialSeleccionada.FechaAnulacion:dd/MM/yyyy HH:mm} por {_ventaHistorialSeleccionada.NombreUsuarioAnula ?? "Encargado"} — Motivo: {_ventaHistorialSeleccionada.MotivoAnulacion})";

            _lblResumenSeleccionada.Text =
                $"{_ventaHistorialSeleccionada.NroComprobante} | Cliente: {_ventaHistorialSeleccionada.NombreCliente} (DNI {_ventaHistorialSeleccionada.DocumentoCliente})\n" +
                $"Cajero: {_ventaHistorialSeleccionada.NombreOperador} | Estado: {estadoTexto}";

            _btnAnularVenta.Enabled = _ventaHistorialSeleccionada.Estado && PoliticaAcceso.PuedeAnularOperaciones(_rolActual);

            var detalles = await _ventaNegocio.ObtenerDetalleVentaAsync(_ventaHistorialSeleccionada.IdVenta);
            if (!IsDisposed && !Disposing)
            {
                _grillaDetalleHistorial.DataSource = detalles.ToList();
            }
        }

        private async Task ConfirmarAnulacionVentaAsync()
        {
            if (_ventaHistorialSeleccionada is null || !_ventaHistorialSeleccionada.Estado)
                return;

            string motivo = _txtMotivoAnulacion.Text.Trim();
            if (motivo.Length < 3)
            {
                MessageBox.Show(
                    "Ingrese un motivo de anulación de al menos 3 caracteres antes de confirmar.",
                    "Motivo requerido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                _txtMotivoAnulacion.Focus();
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Confirma la anulación de la venta {_ventaHistorialSeleccionada.NroComprobante} por {_ventaHistorialSeleccionada.MontoTotal:C2}?\n\n" +
                $"Se restituirán automáticamente las existencias de todos sus productos al stock.",
                "Confirmar Anulación de Venta",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes)
                return;

            _btnAnularVenta.Enabled = false;
            var respuesta = await _ventaNegocio.AnularVentaAsync(
                _ventaHistorialSeleccionada.IdVenta,
                _idUsuarioActual,
                _rolActual,
                motivo);

            if (respuesta.Exito)
            {
                _txtMotivoAnulacion.Text = string.Empty;
                MessageBox.Show(
                    respuesta.Mensaje ?? "Venta anulada con éxito.",
                    "Venta Anulada",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                await CargarHistorialVentasAsync();
            }
            else
            {
                _btnAnularVenta.Enabled = true;
                MessageBox.Show(
                    respuesta.Mensaje ?? "No se pudo anular la venta.",
                    "Error al Anular",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void ConfigurarCliente()
        {
            _btnConsumidorFinal.Click += (_, _) =>
            {
                _temporizadorCliente.Stop();
                _busquedaClientePendiente = false;
                _clienteSeleccionado = null;
                EstablecerTextoClienteSinEvento(string.Empty);
                OcultarListaClientes();
                _lblClienteSeleccionado.Text = "Cliente: Consumidor Final";
            };

            _txtCliente.TextChanged += (_, _) =>
            {
                if (IsDisposed || Disposing || _actualizandoTextoCliente) return;
                _temporizadorCliente.Stop();
                string texto = _txtCliente.Text.Trim();
                if (string.IsNullOrWhiteSpace(texto))
                {
                    _busquedaClientePendiente = false;
                    _clienteSeleccionado = null;
                    OcultarListaClientes();
                    _lblClienteSeleccionado.Text = "Cliente: Consumidor Final";
                    return;
                }
                _lblClienteSeleccionado.Text = "Buscando clientes...";
                _temporizadorCliente.Start();
            };

            _temporizadorCliente.Tick += async (_, _) =>
            {
                _temporizadorCliente.Stop();
                await BuscarClienteAsync();
            };

            _txtCliente.KeyDown += async (_, e) =>
            {
                if (e.KeyCode == Keys.Down && _listaClientes.Visible && _listaClientes.Items.Count > 0)
                {
                    e.SuppressKeyPress = true;
                    _listaClientes.SelectedIndex = Math.Min(
                        _listaClientes.Items.Count - 1,
                        _listaClientes.SelectedIndex + 1);
                }
                else if (e.KeyCode == Keys.Up && _listaClientes.Visible && _listaClientes.Items.Count > 0)
                {
                    e.SuppressKeyPress = true;
                    _listaClientes.SelectedIndex = Math.Max(0, _listaClientes.SelectedIndex - 1);
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    _temporizadorCliente.Stop();
                    if (_listaClientes.Visible && _listaClientes.SelectedItem is ItemClientePos item)
                    {
                        SeleccionarCliente(item.Cliente);
                    }
                    else
                    {
                        await BuscarClienteAsync(seleccionarUnico: true);
                    }
                }
                else if (e.KeyCode == Keys.Escape && _listaClientes.Visible)
                {
                    e.SuppressKeyPress = true;
                    OcultarListaClientes();
                    ActualizarEtiquetaClienteActual();
                }
            };

            _listaClientes.MouseClick += (_, _) =>
            {
                if (_listaClientes.SelectedItem is ItemClientePos item)
                {
                    SeleccionarCliente(item.Cliente);
                }
            };

            _listaClientes.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter && _listaClientes.SelectedItem is ItemClientePos item)
                {
                    e.SuppressKeyPress = true;
                    SeleccionarCliente(item.Cliente);
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    e.SuppressKeyPress = true;
                    OcultarListaClientes();
                    ActualizarEtiquetaClienteActual();
                    _txtCliente.Focus();
                }
            };

            _txtCliente.SizeChanged += (_, _) =>
            {
                if (_listaClientes.Visible) PosicionarListaClientes();
            };

            _txtCodigo.Enter += (_, _) =>
            {
                if (_listaClientes.Visible)
                {
                    OcultarListaClientes();
                    ActualizarEtiquetaClienteActual();
                }
            };
        }

        private async Task BuscarClienteAsync(bool seleccionarUnico = false)
        {
            if (IsDisposed || Disposing) return;

            if (_buscandoCliente)
            {
                _busquedaClientePendiente = true;
                return;
            }

            string busqueda = _txtCliente.Text.Trim();
            if (string.IsNullOrWhiteSpace(busqueda))
            {
                _clienteSeleccionado = null;
                OcultarListaClientes();
                _lblClienteSeleccionado.Text = "Cliente: Consumidor Final";
                return;
            }

            _buscandoCliente = true;
            try
            {
                var respuesta = await _clienteNegocio.BuscarClientesParaPosAsync(busqueda);
                if (IsDisposed || Disposing) return;

                if (!string.Equals(busqueda, _txtCliente.Text.Trim(), StringComparison.Ordinal))
                {
                    _busquedaClientePendiente = true;
                    return;
                }

                if (respuesta.Resultado == CapaNegocio.Enums.ResultadoListadoClientes.ConResultados && respuesta.Clientes.Count > 0)
                {
                    if (seleccionarUnico && respuesta.Clientes.Count == 1)
                    {
                        SeleccionarCliente(respuesta.Clientes[0]);
                        return;
                    }

                    MostrarListaClientes(respuesta.Clientes);
                    _lblClienteSeleccionado.Text = $"{respuesta.Clientes.Count} coincidencia(s). Elija con ↓/Enter o clic.";
                }
                else
                {
                    _clienteSeleccionado = null;
                    OcultarListaClientes();
                    _lblClienteSeleccionado.Text = "Cliente no encontrado. Se asigna Consumidor Final.";
                }
            }
            catch (Exception)
            {
                if (!IsDisposed && !Disposing)
                {
                    _clienteSeleccionado = null;
                    OcultarListaClientes();
                    _lblClienteSeleccionado.Text = "Error al consultar cliente. Se asigna Consumidor Final.";
                }
            }
            finally
            {
                _buscandoCliente = false;
                if (_busquedaClientePendiente && !IsDisposed && !Disposing)
                {
                    _busquedaClientePendiente = false;
                    await BuscarClienteAsync(seleccionarUnico);
                }
            }
        }

        private void MostrarListaClientes(IReadOnlyList<ClienteRespuestaDto> clientes)
        {
            _listaClientes.BeginUpdate();
            try
            {
                _listaClientes.Items.Clear();
                foreach (var cliente in clientes)
                {
                    _listaClientes.Items.Add(new ItemClientePos(cliente));
                }
                if (_listaClientes.Items.Count > 0)
                {
                    _listaClientes.SelectedIndex = 0;
                }
            }
            finally
            {
                _listaClientes.EndUpdate();
            }

            PosicionarListaClientes();
            _listaClientes.Visible = true;
            _listaClientes.BringToFront();
        }

        private void PosicionarListaClientes()
        {
            if (_cardCarga is null || _txtCliente?.Parent is null || _listaClientes is null) return;

            Point ubicacionEnCard = _cardCarga.PointToClient(
                _txtCliente.Parent.PointToScreen(_txtCliente.Location));

            int altoFila = Math.Max(18, _listaClientes.ItemHeight);
            int filasVisibles = Math.Clamp(_listaClientes.Items.Count, 1, 6);
            int alto = (filasVisibles * altoFila) + 8;

            _listaClientes.Bounds = new Rectangle(
                ubicacionEnCard.X,
                ubicacionEnCard.Y + _txtCliente.Height + 2,
                Math.Max(_txtCliente.Width, 240),
                alto);
        }

        private void OcultarListaClientes()
        {
            if (_listaClientes is not null)
            {
                _listaClientes.Visible = false;
            }
        }

        private void SeleccionarCliente(ClienteRespuestaDto cliente)
        {
            _temporizadorCliente.Stop();
            _busquedaClientePendiente = false;
            _clienteSeleccionado = cliente;
            EstablecerTextoClienteSinEvento(cliente.NombreCompleto);
            OcultarListaClientes();
            ActualizarEtiquetaClienteActual();
            _txtCodigo.Focus();
        }

        private void ActualizarEtiquetaClienteActual()
        {
            _lblClienteSeleccionado.Text = _clienteSeleccionado is not null
                ? $"Cliente: {_clienteSeleccionado.NombreCompleto} (DNI: {_clienteSeleccionado.Documento})"
                : "Cliente: Consumidor Final";
        }

        private void EstablecerTextoClienteSinEvento(string texto)
        {
            _actualizandoTextoCliente = true;
            try
            {
                _txtCliente.Text = texto;
                _txtCliente.Refresh();
            }
            finally
            {
                _actualizandoTextoCliente = false;
            }
        }

        private void ConfigurarCobro()
        {
            _txtPagaCon.TextChanged += (_, _) => ActualizarCalculoVuelto();
            _btnCobrar.Click += async (_, _) => await ConfirmarCobroAsync();
        }

        private void ActualizarCalculoVuelto()
        {
            if (IsDisposed || Disposing) return;

            decimal total = _estadoCarrito.Carrito.Total;
            string texto = _txtPagaCon.Text?.Trim().Replace(',', '.') ?? string.Empty;
            bool esValido = decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal pago);

            if (total <= 0)
            {
                _lblVuelto.Text = "Vuelto: $0.00";
                _btnCobrar.Enabled = false;
                return;
            }

            if (esValido && pago >= total)
            {
                decimal vuelto = pago - total;
                _lblVuelto.Text = "Vuelto: " + vuelto.ToString("C2", CultureInfo.CurrentCulture);
                _btnCobrar.Enabled = !_ocupadoCarrito;
            }
            else if (esValido && pago < total)
            {
                decimal falta = total - pago;
                _lblVuelto.Text = "Falta: " + falta.ToString("C2", CultureInfo.CurrentCulture);
                _btnCobrar.Enabled = false;
            }
            else
            {
                _lblVuelto.Text = "Vuelto: $0.00";
                _btnCobrar.Enabled = false;
            }
        }

        private async Task ConfirmarCobroAsync()
        {
            if (_ocupadoCarrito || _estadoCarrito.Carrito.Lineas.Count == 0 || IsDisposed || Disposing)
                return;

            decimal total = _estadoCarrito.Carrito.Total;
            string texto = _txtPagaCon.Text?.Trim().Replace(',', '.') ?? string.Empty;
            if (!decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal pago) || pago < total)
            {
                _lblEstadoCarrito.Text = "Ingrese un monto de pago suficiente para cubrir el total.";
                return;
            }

            _ocupadoCarrito = true;
            ActualizarControlesCarrito();
            _lblEstadoCarrito.Text = "Procesando cobro y registrando venta en base de datos...";

            try
            {
                var solicitud = new SolicitudRegistrarVentaDto
                {
                    IdUsuario = _idUsuarioActual > 0 ? _idUsuarioActual : null,
                    IdCliente = _clienteSeleccionado?.IdCliente,
                    DocumentoCliente = _clienteSeleccionado?.Documento ?? "0",
                    NombreCliente = _clienteSeleccionado?.NombreCompleto ?? "Consumidor Final",
                    MontoPago = pago,
                    Lineas = _estadoCarrito.Carrito.Lineas
                };

                var respuesta = await _ventaNegocio.RegistrarVentaAsync(solicitud);

                if (respuesta.Exito)
                {
                    _estadoCarrito.Limpiar();
                    PintarCarrito();
                    _txtPagaCon.Text = string.Empty;
                    _lblVuelto.Text = "Vuelto: $0.00";
                    _lblEstadoCarrito.Text = $"✅ ¡Venta N° {respuesta.IdVenta} registrada! Vuelto: {respuesta.MontoCambio:C2}";

                    MessageBox.Show(
                        $"Venta N° {respuesta.IdVenta} registrada con éxito.\n\n" +
                        $"Cliente: {solicitud.NombreCliente}\n" +
                        $"Total: {respuesta.MontoTotal:C2}\n" +
                        $"Abonado: {respuesta.MontoPago:C2}\n" +
                        $"Vuelto: {respuesta.MontoCambio:C2}",
                        "Venta Confirmada",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    _txtCodigo.Focus();
                }
                else
                {
                    _lblEstadoCarrito.Text = "❌ " + (respuesta.Mensaje ?? "No se pudo registrar la venta.");
                    MessageBox.Show(
                        respuesta.Mensaje ?? "No se pudo registrar la venta en la base de datos.",
                        "Fallo al Registrar Venta",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception)
            {
                _lblEstadoCarrito.Text = "❌ Error inesperado al procesar la venta.";
            }
            finally
            {
                _ocupadoCarrito = false;
                if (!IsDisposed && !Disposing)
                {
                    ActualizarControlesCarrito();
                }
            }
        }

        private void ConfigurarBusqueda()
        {
            _txtCodigo.TextChanged += (_, _) =>
            {
                if (IsDisposed || Disposing) return;
                _busqueda.Invalidar();
                _temporizadorBusqueda.Stop();
                _productoSeleccionado = null;
                _grillaProductos.DataSource = null;
                ActualizarControlesCarrito();
                bool vacia = string.IsNullOrWhiteSpace(_txtCodigo.Text);
                _lblEstadoBusqueda.Text = vacia
                    ? "Escriba un código o nombre para buscar."
                    : "Buscando productos...";
                if (!vacia) _temporizadorBusqueda.Start();
            };
            _temporizadorBusqueda.Tick += async (_, _) =>
            {
                _temporizadorBusqueda.Stop();
                await BuscarProductosAsync();
            };
            _txtCodigo.KeyDown += async (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    _temporizadorBusqueda.Stop();
                    await BuscarProductosAsync(seleccionarUnico: true);
                }
                else if (e.KeyCode == Keys.Down && _grillaProductos.Rows.Count > 0)
                {
                    e.SuppressKeyPress = true;
                    EnfocarResultados();
                }
            };
            _grillaProductos.SelectionChanged += (_, _) =>
            {
                _productoSeleccionado = _grillaProductos.SelectedRows.Count == 1
                    ? _grillaProductos.SelectedRows[0].DataBoundItem as ProductoVentaDto
                    : null;
                if (_productoSeleccionado is not null)
                    _lblEstadoBusqueda.Text = $"Seleccionado: {_productoSeleccionado.Nombre}";
                ActualizarControlesCarrito();
            };
            _grillaProductos.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex >= 0 && _productoSeleccionado is not null)
                    _txtCantidad.Focus();
            };
            _grillaProductos.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter && _productoSeleccionado is not null)
                {
                    e.SuppressKeyPress = true;
                    _txtCantidad.Focus();
                }
            };
        }

        private async Task BuscarProductosAsync(bool seleccionarUnico = false)
        {
            if (IsDisposed || Disposing || string.IsNullOrWhiteSpace(_txtCodigo.Text)) return;
            _productoSeleccionado = null;
            _grillaProductos.DataSource = null;
            ActualizarControlesCarrito();
            _lblEstadoBusqueda.Text = "Buscando productos...";
            var consulta = _busqueda.BuscarAsync(_txtCodigo.Text.Trim());
            int version = _busqueda.Version;
            try
            {
                var productos = await consulta;
                if (productos is null || version != _busqueda.Version || IsDisposed || Disposing) return;

                _grillaProductos.DataSource = productos.ToList();
                _grillaProductos.ClearSelection();
                _productoSeleccionado = null;
                ActualizarControlesCarrito();
                _lblEstadoBusqueda.Text = productos.Count == 0
                    ? "Sin coincidencias. Pruebe otro código o nombre."
                    : $"{productos.Count} producto(s). Seleccione uno de la lista.";
                if (seleccionarUnico && productos.Count > 0)
                {
                    EnfocarResultados();
                    if (productos.Count == 1) _txtCantidad.Focus();
                }
            }
            catch (Exception)
            {
                if (version == _busqueda.Version && !IsDisposed && !Disposing)
                    _lblEstadoBusqueda.Text = "No se pudo buscar. Intente nuevamente.";
            }
        }

        private void EnfocarResultados()
        {
            _grillaProductos.Focus();
            _grillaProductos.CurrentCell = _grillaProductos.Rows[0].Cells[0];
            _grillaProductos.Rows[0].Selected = true;
        }

        private void ConfigurarCarrito()
        {
            _btnAgregar.Click += async (_, _) => await AgregarSeleccionadoAsync();
            _txtCantidad.KeyDown += async (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    await AgregarSeleccionadoAsync();
                }
            };
            _btnQuitar.Click += async (_, _) =>
            {
                if (_ocupadoCarrito || _grillaCarrito.SelectedRows.Count != 1) return;
                int id = (int)_grillaCarrito.SelectedRows[0].Tag!;
                await AplicarCambioCarritoAsync(c => Task.FromResult(_carritoVentaNegocio.QuitarProducto(c, id)), id);
            };
            _grillaCarrito.SelectionChanged += (_, _) => ActualizarControlesCarrito();
            _grillaCarrito.CellEndEdit += async (_, e) =>
            {
                if (_pintandoCarrito || _ocupadoCarrito || e.RowIndex < 0 || e.ColumnIndex != 0) return;
                var fila = _grillaCarrito.Rows[e.RowIndex];
                int id = (int)fila.Tag!;
                int cantidadAnterior = _estadoCarrito.Carrito.Lineas.Single(l => l.IdProducto == id).Cantidad;
                if (!int.TryParse(Convert.ToString(fila.Cells[0].Value), out int cantidad))
                {
                    fila.Cells[0].Value = cantidadAnterior.ToString(CultureInfo.CurrentCulture);
                    _lblEstadoCarrito.Text = "Ingrese una cantidad entera válida.";
                    return;
                }
                if (cantidad == cantidadAnterior) return;
                await AplicarCambioCarritoAsync(c => _carritoVentaNegocio.CambiarCantidadAsync(c, id, cantidad), id);
            };
        }

        private async Task AgregarSeleccionadoAsync()
        {
            if (_ocupadoCarrito || _productoSeleccionado is null) return;
            if (!int.TryParse(_txtCantidad.Text, out int cantidad))
            {
                _lblEstadoCarrito.Text = "Ingrese una cantidad entera válida.";
                return;
            }
            int id = _productoSeleccionado.IdProducto;
            await AplicarCambioCarritoAsync(c => _carritoVentaNegocio.AgregarProductoAsync(c, id, cantidad), id, true);
        }

        private async Task AplicarCambioCarritoAsync(
            Func<CarritoVentaDto, Task<ResultadoCarritoDto>> operacion, int idProducto, bool enfocarBusqueda = false)
        {
            if (_ocupadoCarrito || IsDisposed || Disposing) return;
            _ocupadoCarrito = true;
            ActualizarControlesCarrito();
            _lblEstadoCarrito.Text = "Actualizando carrito...";
            try
            {
                bool exito = false;
                string mensaje;
                try
                {
                    var resultado = await _estadoCarrito.EjecutarAsync(operacion);
                    exito = resultado.Exito;
                    mensaje = exito ? "Carrito actualizado." : resultado.Mensaje ?? "No se pudo actualizar el carrito.";
                }
                catch (Exception)
                {
                    mensaje = "No se pudo actualizar el carrito. Intente nuevamente.";
                }

                await Task.Yield();
                if (IsDisposed || Disposing) return;
                PintarCarrito(idProducto);
                _lblEstadoCarrito.Text = mensaje;
                if (exito && enfocarBusqueda)
                {
                    _txtCantidad.Text = "1";
                    _txtCodigo.Focus();
                    _txtCodigo.SelectAll();
                }
            }
            finally
            {
                _ocupadoCarrito = false;
                if (!IsDisposed && !Disposing) ActualizarControlesCarrito();
            }
        }

        private void PintarCarrito(int? idSeleccionado = null)
        {
            _pintandoCarrito = true;
            try
            {
                _grillaCarrito.Rows.Clear();
                foreach (var linea in _estadoCarrito.Carrito.Lineas)
                {
                    int indice = _grillaCarrito.Rows.Add(linea.Cantidad.ToString(CultureInfo.CurrentCulture),
                        linea.Nombre ?? string.Empty, linea.PrecioUnitario, linea.Subtotal);
                    _grillaCarrito.Rows[indice].Tag = linea.IdProducto;
                }
                _grillaCarrito.ClearSelection();
                if (idSeleccionado.HasValue)
                {
                    foreach (DataGridViewRow fila in _grillaCarrito.Rows)
                    {
                        if ((int)fila.Tag! == idSeleccionado.Value)
                        {
                            _grillaCarrito.CurrentCell = fila.Cells[0];
                            fila.Selected = true;
                            break;
                        }
                    }
                }
                _lblTotal.Text = "TOTAL: " + _estadoCarrito.Carrito.Total.ToString("C2", CultureInfo.CurrentCulture);
                ActualizarCalculoVuelto();
            }
            finally { _pintandoCarrito = false; }
        }

        private void ActualizarControlesCarrito()
        {
            if (IsDisposed || Disposing) return;
            _btnAgregar.Enabled = !_ocupadoCarrito && _productoSeleccionado is not null;
            _btnQuitar.Enabled = !_ocupadoCarrito && _grillaCarrito.SelectedRows.Count == 1;
            _txtCantidad.Enabled = !_ocupadoCarrito;
            _grillaProductos.Enabled = !_ocupadoCarrito;
            _grillaCarrito.Enabled = !_ocupadoCarrito;
            _txtCliente.Enabled = !_ocupadoCarrito;
            _btnConsumidorFinal.Enabled = !_ocupadoCarrito;
            _listaClientes.Enabled = !_ocupadoCarrito;
            if (_ocupadoCarrito) OcultarListaClientes();
            _txtPagaCon.Enabled = !_ocupadoCarrito && _estadoCarrito.Carrito.Lineas.Count > 0;
            ActualizarCalculoVuelto();
        }

        private sealed record ItemClientePos(ClienteRespuestaDto Cliente)
        {
            public override string ToString() => $"{Cliente.NombreCompleto} — DNI: {Cliente.Documento}";
        }

        private class GrillaCarritoVenta : DataGridView
        {
            protected override bool ProcessDialogKey(Keys keyData)
            {
                return ConfirmarCantidad(keyData) || base.ProcessDialogKey(keyData);
            }

            protected override bool ProcessDataGridViewKey(KeyEventArgs e)
            {
                return ConfirmarCantidad(e.KeyData) || base.ProcessDataGridViewKey(e);
            }

            private bool ConfirmarCantidad(Keys tecla)
            {
                if (tecla != Keys.Enter || CurrentCell?.ColumnIndex != 0) return false;

                EndEdit();
                return true;
            }
        }
    }
}
