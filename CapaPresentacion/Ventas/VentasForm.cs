using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CapaPresentacion.Tema;
using MaterialSkin;
using MaterialSkin.Controls;
using CapaNegocio;
using CapaNegocio.DTOs.Ventas;
using System.Globalization;

namespace CapaPresentacion.Ventas
{
    public partial class VentasForm : UserControl
    {
        private readonly CarritoVentaNegocio _carritoVentaNegocio;
        private readonly BusquedaProductosVenta _busqueda;
        private readonly System.Windows.Forms.Timer _temporizadorBusqueda;
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
        private bool _ocupadoCarrito;
        private bool _pintandoCarrito;

        public VentasForm(CarritoVentaNegocio carritoVentaNegocio)
        {
            _carritoVentaNegocio = carritoVentaNegocio;
            _busqueda = new BusquedaProductosVenta(_carritoVentaNegocio.BuscarProductosAsync);
            InitializeComponent();
            components ??= new Container();
            _temporizadorBusqueda = new System.Windows.Forms.Timer(components) { Interval = 300 };
            BackColor = TemaAplicacion.FondoPrincipal;
            ArmarInterfazMaterial();
            ConfigurarBusqueda();
            ConfigurarCarrito();
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

            // ================= PANAL IZQUIERDO: CARGA =================
            MaterialCard cardCarga = new MaterialCard
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(5),
                Padding = new Padding(15)
            };

            var layoutCarga = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Margin = Padding.Empty
            };
            layoutCarga.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutCarga.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            layoutCarga.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            layoutCarga.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            layoutCarga.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            layoutCarga.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutCarga.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));

            MaterialLabel lblIngreso = new MaterialLabel
            {
                Text = "🛒 INGRESO DE PRODUCTOS",
                FontType = MaterialSkinManager.fontType.H6,
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            MaterialTextBox2 txtCliente = new MaterialTextBox2
            {
                Hint = "DNI Cliente / Consumidor Final",
                Text = "20-3811223-4 - Carlos Gómez",
                Dock = DockStyle.Top,
                AccessibleName = "Cliente"
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
            layoutCarga.Controls.Add(txtCliente, 0, 1);
            layoutCarga.Controls.Add(layoutBusqueda, 0, 2);
            layoutCarga.Controls.Add(_lblEstadoBusqueda, 0, 3);
            layoutCarga.Controls.Add(_grillaProductos, 0, 4);
            layoutCarga.Controls.Add(_btnAgregar, 0, 5);
            cardCarga.Controls.Add(layoutCarga);

            // ================= PANEL DERECHO: CARRITO Y COBRO =================
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
                RowCount = 6,
                Margin = Padding.Empty
            };
            layoutCarrito.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutCarrito.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            layoutCarrito.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutCarrito.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            layoutCarrito.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            layoutCarrito.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            layoutCarrito.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));

            MaterialLabel lblCarrito = new MaterialLabel
            {
                Text = "📋 DETALLE DEL CARRITO",
                FontType = MaterialSkinManager.fontType.H6,
                Dock = DockStyle.Fill,
                AutoSize = true
            };

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
                _grillaCarrito.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = titulo, ReadOnly = true, ValueType = typeof(decimal), FillWeight = 23,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" }
                });

            _btnQuitar = new MaterialButton
            {
                Text = "Quitar seleccionado",
                Dock = DockStyle.Fill,
                Enabled = false,
                Type = MaterialButton.MaterialButtonType.Outlined
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

            MaterialButton btnCobrar = new MaterialButton
            {
                Text = "💳 CONFIRMAR Y FACTURAR",
                Dock = DockStyle.Fill,
                Enabled = false,
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true
            };

            layoutCarrito.Controls.Add(lblCarrito, 0, 0);
            layoutCarrito.Controls.Add(_grillaCarrito, 0, 1);
            layoutCarrito.Controls.Add(_btnQuitar, 0, 2);
            layoutCarrito.Controls.Add(_lblEstadoCarrito, 0, 3);
            layoutCarrito.Controls.Add(_lblTotal, 0, 4);
            layoutCarrito.Controls.Add(btnCobrar, 0, 5);
            cardCarrito.Controls.Add(layoutCarrito);

            layout.Controls.Add(cardCarga, 0, 0);
            layout.Controls.Add(cardCarrito, 1, 0);

            Controls.Add(layout);
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
                    foreach (DataGridViewRow fila in _grillaCarrito.Rows)
                        if ((int)fila.Tag! == idSeleccionado.Value)
                        {
                            _grillaCarrito.CurrentCell = fila.Cells[0];
                            fila.Selected = true;
                            break;
                        }
                _lblTotal.Text = "TOTAL: " + _estadoCarrito.Carrito.Total.ToString("C2", CultureInfo.CurrentCulture);
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
