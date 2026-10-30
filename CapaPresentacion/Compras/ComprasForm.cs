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
    /// Pantalla de carga de compras a proveedores organizada en cuatro bloques:
    /// producto, proveedor, compra actual e historial. El producto y el proveedor
    /// se buscan a demanda (RF-9 … RF-17) y la selección es bidireccional: elegir
    /// un producto propone los proveedores que ya lo vendieron y elegir un
    /// proveedor acota el listado a los productos de su historial (RF-12 y RF-53).
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
        private const string FormatoFecha = "dd/MM/yyyy HH:mm";
        private const string FormatoImporte = "N2";

        private const string TextoProveedorSinElegir = "Sin proveedor seleccionado";
        private const string TextoDocumentoSinElegir = "Documento: —";
        private const string TextoProductoSinElegir = "(Sin producto seleccionado)";
        private const string TextoCodigoSinElegir = "Código: —";
        private const string TextoSubtotalInvalido = "Subtotal: —";
        private const string TextoSeleccioneProveedor = "Seleccione el proveedor...";
        private const string TextoBotonAgregar = "Agregar a la compra";
        private const string TextoBotonGuardar = "Guardar línea";

        private readonly CompraNegocio _negocio;
        private readonly System.Windows.Forms.Timer _temporizadorProveedores =
            new() { Interval = 350 };
        private readonly System.Windows.Forms.Timer _temporizadorProductos =
            new() { Interval = 350 };
        private readonly List<LineaCompraDto> _lineas = new();
        private readonly List<ProveedorSeleccionadoDto> _proveedoresBuscados = new();
        private readonly List<ProveedorDeProductoDto> _proveedoresHistoricos = new();
        private readonly List<ProductoBuscadoDto> _productosBuscados = new();
        private readonly List<ProductoDeProveedorDto> _productosHistoricos = new();

        private int? _idProveedorElegido;
        private string? _razonSocialProveedor;
        private string? _documentoProveedor;
        private int? _idProductoElegido;
        private string? _codigoProductoElegido;
        private string? _nombreProductoElegido;
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

            InitializeComponent();

            BackColor = TemaAplicacion.FondoPrincipal;
            AplicarEstiloGrilla(gridProductos);
            AplicarEstiloGrilla(gridProveedores);
            AplicarEstiloGrilla(gridLineas);
            AplicarEstiloGrilla(gridHistorial);

            // Los listados arrancan vacíos: solo se llenan al tipear (RF-10, RF-16).
            MostrarProductos(null);
            MostrarProveedores(null);
            MostrarDatosProducto();
            MostrarDatosProveedor();

            _temporizadorProveedores.Tick += temporizadorProveedores_Tick;
            _temporizadorProductos.Tick += temporizadorProductos_Tick;
            this.Disposed += (_, _) =>
            {
                _temporizadorProveedores.Dispose();
                _temporizadorProductos.Dispose();
            };

            txtBuscarProveedor.TextChanged += txtBuscarProveedor_TextChanged;
            txtBuscarProducto.TextChanged += txtBuscarProducto_TextChanged;
            gridProveedores.SelectionChanged += gridProveedores_SelectionChanged;
            gridProductos.SelectionChanged += gridProductos_SelectionChanged;
            txtCantidad.TextChanged += captura_TextChanged;
            txtPrecioCompra.TextChanged += captura_TextChanged;
            txtPrecioVenta.TextChanged += captura_TextChanged;
            btnAgregarLinea.Click += btnAgregarLinea_Click;
            gridLineas.CellContentClick += gridLineas_CellContentClick;
            gridLineas.CellFormatting += gridLineas_CellFormatting;
            btnRegistrar.Click += btnRegistrar_Click;
            btnLimpiar.Click += btnLimpiar_Click;
            btnVerDetalle.Click += btnVerDetalle_Click;
            gridHistorial.CellFormatting += gridHistorial_CellFormatting;
            gridHistorial.CellDoubleClick += gridHistorial_CellDoubleClick;

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
            txtBuscarProveedor.Enabled = habilitado;
            gridProveedores.Enabled = habilitado;
            txtBuscarProducto.Enabled = habilitado;
            gridProductos.Enabled = habilitado;
            txtCantidad.Enabled = habilitado;
            txtPrecioCompra.Enabled = habilitado;
            txtPrecioVenta.Enabled = habilitado;
            btnAgregarLinea.Enabled = habilitado;
            btnLimpiar.Enabled = habilitado;
            btnVerDetalle.Enabled = habilitado;
            btnRegistrar.Enabled = habilitado && !_registrando;
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

                _actualizandoControles = true;

                try
                {
                    gridHistorial.DataSource = null;
                    gridHistorial.DataSource = respuesta.Compras.ToList();
                    gridHistorial.ClearSelection();
                    gridHistorial.CurrentCell = null;
                }
                finally
                {
                    _actualizandoControles = false;
                }

                lblMensajeHistorial.Text = respuesta.Mensaje;
            }
            finally
            {
                _cargandoHistorial = false;
            }
        }

        // =====================================================================
        // 1. ESTILO DE LAS GRILLAS (MaterialSkin 2, dark)
        // =====================================================================

        /// <summary>
        /// Aplica el estilo oscuro compartido por las pantallas de la aplicación a
        /// una grilla ya declarada en el diseñador (RNF-7).
        /// </summary>
        /// <param name="grilla">Grilla a estilizar.</param>
        private static void AplicarEstiloGrilla(DataGridView grilla)
        {
            grilla.BackgroundColor = Color.FromArgb(48, 48, 48);
            grilla.BorderStyle = BorderStyle.None;
            grilla.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grilla.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grilla.EnableHeadersVisualStyles = false;

            grilla.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(55, 71, 79),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                SelectionBackColor = Color.FromArgb(55, 71, 79),
                SelectionForeColor = Color.White,
                WrapMode = DataGridViewTriState.True
            };

            grilla.DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(48, 48, 48),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.White,
                SelectionBackColor = Color.FromArgb(255, 87, 34),
                SelectionForeColor = Color.White,
                WrapMode = DataGridViewTriState.False
            };

            grilla.GridColor = Color.FromArgb(80, 80, 80);
        }

        // =====================================================================
        // 2. BÚSQUEDAS A DEMANDA Y SELECCIÓN BIDIRECCIONAL
        // =====================================================================

        private void txtBuscarProveedor_TextChanged(object? sender, EventArgs e)
        {
            if (_actualizandoControles)
            {
                return;
            }

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
        /// se invoca el negocio y el listado queda vacío (RF-10).
        /// </summary>
        /// <returns>Tarea que representa la búsqueda.</returns>
        private async Task BuscarProveedoresAsync()
        {
            if (string.IsNullOrWhiteSpace(txtBuscarProveedor.Text))
            {
                _proveedoresBuscados.Clear();
                _proveedoresHistoricos.Clear();
                MostrarProveedores(null);
                lblMensajeProveedores.Text = string.Empty;
                return;
            }

            RespuestaBusquedaProveedoresCompra respuesta =
                await _negocio.BuscarProveedoresAsync(txtBuscarProveedor.Text);

            _proveedoresBuscados.Clear();
            _proveedoresBuscados.AddRange(respuesta.Proveedores);
            _proveedoresHistoricos.Clear();
            MostrarProveedores(_proveedoresBuscados);
            lblMensajeProveedores.Text = respuesta.Mensaje;
        }

        private void txtBuscarProducto_TextChanged(object? sender, EventArgs e)
        {
            if (_actualizandoControles)
            {
                return;
            }

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
        /// invoca el negocio y el listado queda vacío (RF-16).
        /// </summary>
        /// <returns>Tarea que representa la búsqueda.</returns>
        private async Task BuscarProductosAsync()
        {
            if (string.IsNullOrWhiteSpace(txtBuscarProducto.Text))
            {
                _productosBuscados.Clear();
                _productosHistoricos.Clear();
                MostrarProductos(null);
                lblMensajeProductos.Text = string.Empty;
                return;
            }

            RespuestaBusquedaProductosCompra respuesta =
                await _negocio.BuscarProductosAsync(txtBuscarProducto.Text);

            _productosBuscados.Clear();
            _productosBuscados.AddRange(respuesta.Productos);
            _productosHistoricos.Clear();
            MostrarProductos(_productosBuscados);
            lblMensajeProductos.Text = respuesta.Mensaje;
        }

        private async void gridProductos_SelectionChanged(object? sender, EventArgs e)
        {
            if (_actualizandoControles || gridProductos.CurrentRow is null)
            {
                return;
            }

            if (!TryObtenerProductoSeleccionado(gridProductos.CurrentRow.DataBoundItem, out int idProducto,
                    out string codigo, out string nombre))
            {
                return;
            }

            if (_idProductoElegido == idProducto)
            {
                return;
            }

            _idProductoElegido = idProducto;
            _codigoProductoElegido = codigo;
            _nombreProductoElegido = nombre;
            MostrarDatosProducto();

            await EjecutarAsync(async () =>
            {
                await PrellenarPreciosAsync(idProducto);

                // RF-38 / RF-39: el foco queda en Cantidad con el texto seleccionado.
                txtCantidad.Focus();
                txtCantidad.SelectAll();

                await ResolverProveedoresDelProductoAsync(idProducto);
            });
        }

        private async void gridProveedores_SelectionChanged(object? sender, EventArgs e)
        {
            if (_actualizandoControles || gridProveedores.CurrentRow is null)
            {
                return;
            }

            if (!TryObtenerProveedorSeleccionado(gridProveedores.CurrentRow.DataBoundItem,
                    out int idProveedor, out string razonSocial, out string documento))
            {
                return;
            }

            if (_idProveedorElegido == idProveedor)
            {
                return;
            }

            _idProveedorElegido = idProveedor;
            _razonSocialProveedor = razonSocial;
            _documentoProveedor = documento;
            MostrarDatosProveedor();

            await EjecutarAsync(() => FiltrarProductosDelProveedorAsync(idProveedor));
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

            lblMensajeProductos.Text = respuesta.Mensaje;

            if (respuesta.Precios is null)
            {
                return;
            }

            txtPrecioCompra.Text =
                respuesta.Precios.PrecioCompra.ToString("0.00", CultureInfo.CurrentCulture);
            txtPrecioVenta.Text =
                respuesta.Precios.PrecioVenta.ToString("0.00", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Resuelve los proveedores históricos del producto elegido (RF-12): con
        /// uno solo se autocompleta el proveedor activo; con varios se acota el
        /// listado y se pide elegir; sin históricos el listado queda libre.
        /// </summary>
        /// <param name="idProducto">Producto elegido en la búsqueda.</param>
        /// <returns>Tarea que representa la consulta.</returns>
        private async Task ResolverProveedoresDelProductoAsync(int idProducto)
        {
            RespuestaProveedoresDeProducto respuesta =
                await _negocio.ObtenerProveedoresDeProductoAsync(idProducto, _rolActual);

            _proveedoresBuscados.Clear();
            _proveedoresHistoricos.Clear();

            switch (respuesta.Resultado)
            {
                case ResultadoConsultaHistoricaCompra.ConResultados:
                    _proveedoresHistoricos.AddRange(respuesta.Proveedores);
                    MostrarProveedores(_proveedoresHistoricos);

                    if (_proveedoresHistoricos.Count == 1)
                    {
                        ProveedorDeProductoDto unico = _proveedoresHistoricos[0];

                        // RF-12: con un solo proveedor histórico se autocompleta,
                        // salvo que ya hubiera uno elegido, que no se descarta (RF-13).
                        if (!_idProveedorElegido.HasValue)
                        {
                            _idProveedorElegido = unico.IdProveedor;
                            _razonSocialProveedor = unico.RazonSocial;
                            _documentoProveedor = unico.Documento;
                            MostrarDatosProveedor();
                            SeleccionarProveedorEnGrilla(unico.IdProveedor);
                        }
                    }

                    lblMensajeProveedores.Text = _proveedoresHistoricos.Count == 1
                        ? respuesta.Mensaje
                        : TextoSeleccioneProveedor;
                    return;

                case ResultadoConsultaHistoricaCompra.SinResultados:
                    MostrarProveedores(null);
                    lblMensajeProveedores.Text = respuesta.Mensaje;
                    return;

                default:
                    lblMensajeProveedores.Text = respuesta.Mensaje;
                    return;
            }
        }

        /// <summary>
        /// Acota el listado de productos a los que el proveedor elegido ya vendió
        /// (RF-15). Si no hay históricos, el listado queda libre para la búsqueda
        /// por texto y se muestra el mensaje del negocio.
        /// </summary>
        /// <param name="idProveedor">Proveedor elegido.</param>
        /// <returns>Tarea que representa la consulta.</returns>
        private async Task FiltrarProductosDelProveedorAsync(int idProveedor)
        {
            RespuestaProductosDeProveedor respuesta =
                await _negocio.ObtenerProductosDeProveedorAsync(idProveedor, _rolActual);

            _productosBuscados.Clear();
            _productosHistoricos.Clear();

            if (respuesta.Resultado == ResultadoConsultaHistoricaCompra.ConResultados)
            {
                _productosHistoricos.AddRange(respuesta.Productos);
                MostrarProductos(_productosHistoricos);
            }
            else if (respuesta.Resultado == ResultadoConsultaHistoricaCompra.SinResultados)
            {
                MostrarProductos(null);
            }

            lblMensajeProductos.Text = respuesta.Mensaje;
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
                _idProductoElegido,
                txtCantidad.Text,
                txtPrecioCompra.Text,
                txtPrecioVenta.Text);

            lblSubtotalPreparacion.Text =
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
            if (!_idProveedorElegido.HasValue)
            {
                lblMensajeProveedores.Text = TextoSeleccioneProveedor;
                txtBuscarProveedor.Focus();
                return;
            }

            IReadOnlyList<LineaCompraDto> lineasActuales = _lineas.ToList();
            int idProducto = _idProductoElegido ?? 0;
            RespuestaOperacionLineaCompra respuesta;

            if (_indiceEdicion is int indiceEdicion)
            {
                respuesta = await _negocio.EditarLineaAsync(new SolicitudEditarLineaCompraDto
                {
                    IndiceLinea = indiceEdicion,
                    IdProducto = idProducto,
                    Cantidad = txtCantidad.Text,
                    PrecioCompra = txtPrecioCompra.Text,
                    PrecioVenta = txtPrecioVenta.Text,
                    LineasActuales = lineasActuales
                });
            }
            else
            {
                respuesta = await _negocio.AgregarLineaAsync(new SolicitudAgregarLineaCompraDto
                {
                    IdProducto = idProducto,
                    Cantidad = txtCantidad.Text,
                    PrecioCompra = txtPrecioCompra.Text,
                    PrecioVenta = txtPrecioVenta.Text,
                    LineasActuales = lineasActuales
                });
            }

            lblMensaje.Text = respuesta.Mensaje;

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

            string columna = gridLineas.Columns[e.ColumnIndex].Name;

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

            lblMensaje.Text = respuesta.Mensaje;

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
            _idProductoElegido = linea.IdProducto;
            _codigoProductoElegido = linea.CodigoProducto;
            _nombreProductoElegido = linea.NombreProducto;
            MostrarDatosProducto();

            txtCantidad.Text = linea.Cantidad.ToString(CultureInfo.CurrentCulture);
            txtPrecioCompra.Text = linea.PrecioCompra.ToString("0.00", CultureInfo.CurrentCulture);
            txtPrecioVenta.Text = linea.PrecioVenta.ToString("0.00", CultureInfo.CurrentCulture);
            btnAgregarLinea.Text = TextoBotonGuardar;
            ActualizarSubtotalPreparacion();
        }

        /// <summary>
        /// Vuelve a enlazar la grilla de líneas y actualiza el total general con la
        /// única fórmula del negocio sobre los subtotales de las líneas (RF-7).
        /// </summary>
        private void RefrescarGrillaLineas()
        {
            gridLineas.DataSource = null;
            gridLineas.DataSource = _lineas.ToList();

            decimal montoTotal =
                CalculoImportesCompra.CalcularMontoTotal(_lineas.Select(linea => linea.Subtotal));

            lblMontoTotal.Text = $"Total: {montoTotal.ToString("N2", CultureInfo.CurrentCulture)}";
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

            if (!_idProveedorElegido.HasValue)
            {
                lblMensajeProveedores.Text = TextoSeleccioneProveedor;
                txtBuscarProveedor.Focus();
                return;
            }

            _registrando = true;
            btnRegistrar.Enabled = false;

            try
            {
                SolicitudRegistrarCompraDto solicitud = new()
                {
                    IdProveedor = _idProveedorElegido,
                    Lineas = _lineas.ToList()
                };

                RespuestaRegistroCompra respuesta =
                    await _negocio.RegistrarCompraAsync(solicitud, _idUsuario, _rolActual);

                lblMensaje.Text = respuesta.Mensaje;

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
                btnRegistrar.Enabled = PoliticaAcceso.PuedeOperarCompras(_rolActual);
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
            lblMensaje.Text = string.Empty;
        }

        private void btnVerDetalle_Click(object? sender, EventArgs e)
        {
            AbrirDetalleSeleccionado();
        }

        private void gridHistorial_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            AbrirDetalleSeleccionado();
        }

        /// <summary>
        /// Abre el modal de solo lectura con el detalle de la compra marcada en la
        /// grilla del historial (RF-53 y RF-54). Sin fila seleccionada no hace nada
        /// y no altera la compra en carga.
        /// </summary>
        private void AbrirDetalleSeleccionado()
        {
            if (gridHistorial.CurrentRow?.DataBoundItem is not HistorialCompraDto compra)
            {
                return;
            }

            using DetalleCompraForm detalle = new DetalleCompraForm(_negocio, compra.IdCompra, _rolActual);
            detalle.ShowDialog(this);
        }

        /// <summary>
        /// Descarta el proveedor elegido, las líneas y el área de captura, tanto
        /// tras registrar (RF-32) como tras confirmar la limpieza (RF-31).
        /// </summary>
        private void LimpiarTrasRegistro()
        {
            _idProveedorElegido = null;
            _razonSocialProveedor = null;
            _documentoProveedor = null;
            _proveedoresBuscados.Clear();
            _proveedoresHistoricos.Clear();
            _temporizadorProveedores.Stop();

            _actualizandoControles = true;

            try
            {
                txtBuscarProveedor.Text = string.Empty;
            }
            finally
            {
                _actualizandoControles = false;
            }

            MostrarProveedores(null);
            lblMensajeProveedores.Text = string.Empty;
            MostrarDatosProveedor();
            _lineas.Clear();
            LimpiarCaptura();
            RefrescarGrillaLineas();
        }

        /// <summary>
        /// Deja el área de captura vacía y sin modo edición, y vuelve a dejar el
        /// listado de productos vacío hasta la próxima búsqueda (RF-16).
        /// </summary>
        private void LimpiarCaptura()
        {
            _indiceEdicion = null;
            _idProductoElegido = null;
            _codigoProductoElegido = null;
            _nombreProductoElegido = null;
            _productosBuscados.Clear();
            _productosHistoricos.Clear();
            _temporizadorProductos.Stop();

            _actualizandoControles = true;

            try
            {
                txtBuscarProducto.Text = string.Empty;
                txtCantidad.Text = string.Empty;
                txtPrecioCompra.Text = string.Empty;
                txtPrecioVenta.Text = string.Empty;
            }
            finally
            {
                _actualizandoControles = false;
            }

            MostrarProductos(null);
            lblMensajeProductos.Text = string.Empty;
            MostrarDatosProducto();
            btnAgregarLinea.Text = TextoBotonAgregar;
            ActualizarSubtotalPreparacion();
        }

        // =====================================================================
        // 5. PRESENTACIÓN DE LOS LISTADOS Y DATOS SELECCIONADOS
        // =====================================================================

        /// <summary>
        /// Vuelve a enlazar el listado de productos sin disparar la selección
        /// automática de la primera fila.
        /// </summary>
        /// <param name="origen">Lista de resultados o <see langword="null"/> para vaciarlo.</param>
        private void MostrarProductos(object? origen)
        {
            _actualizandoControles = true;

            try
            {
                gridProductos.DataSource = null;
                gridProductos.DataSource = origen;
                gridProductos.ClearSelection();
                gridProductos.CurrentCell = null;
            }
            finally
            {
                _actualizandoControles = false;
            }
        }

        /// <summary>
        /// Vuelve a enlazar el listado de proveedores sin disparar la selección
        /// automática de la primera fila.
        /// </summary>
        /// <param name="origen">Lista de resultados o <see langword="null"/> para vaciarlo.</param>
        private void MostrarProveedores(object? origen)
        {
            _actualizandoControles = true;

            try
            {
                gridProveedores.DataSource = null;
                gridProveedores.DataSource = origen;
                gridProveedores.ClearSelection();
                gridProveedores.CurrentCell = null;
            }
            finally
            {
                _actualizandoControles = false;
            }
        }

        private void MostrarDatosProducto()
        {
            bool hayProducto = _idProductoElegido.HasValue;
            lblNombreProducto.Text = hayProducto ? _nombreProductoElegido : TextoProductoSinElegir;
            lblCodigoProducto.Text = hayProducto
                ? $"Código: {_codigoProductoElegido}"
                : TextoCodigoSinElegir;
        }

        private void MostrarDatosProveedor()
        {
            bool hayProveedor = _idProveedorElegido.HasValue;
            lblRazonSocialProveedor.Text = hayProveedor ? _razonSocialProveedor : TextoProveedorSinElegir;
            lblDocumentoProveedor.Text = hayProveedor
                ? $"Documento: {_documentoProveedor}"
                : TextoDocumentoSinElegir;
        }

        private void SeleccionarProveedorEnGrilla(int idProveedor)
        {
            _actualizandoControles = true;

            try
            {
                foreach (DataGridViewRow fila in gridProveedores.Rows)
                {
                    if (fila.DataBoundItem is ProveedorDeProductoDto proveedor
                        && proveedor.IdProveedor == idProveedor)
                    {
                        gridProveedores.CurrentCell = fila.Cells[0];
                        break;
                    }
                }
            }
            finally
            {
                _actualizandoControles = false;
            }
        }

        private static bool TryObtenerProductoSeleccionado(
            object? item, out int idProducto, out string codigo, out string nombre)
        {
            switch (item)
            {
                case ProductoBuscadoDto producto:
                    idProducto = producto.IdProducto;
                    codigo = producto.Codigo;
                    nombre = producto.Nombre;
                    return true;

                case ProductoDeProveedorDto producto:
                    idProducto = producto.IdProducto;
                    codigo = producto.Codigo;
                    nombre = producto.Nombre;
                    return true;

                default:
                    idProducto = 0;
                    codigo = string.Empty;
                    nombre = string.Empty;
                    return false;
            }
        }

        private static bool TryObtenerProveedorSeleccionado(
            object? item, out int idProveedor, out string razonSocial, out string documento)
        {
            switch (item)
            {
                case ProveedorSeleccionadoDto proveedor:
                    idProveedor = proveedor.IdProveedor;
                    razonSocial = proveedor.RazonSocial;
                    documento = proveedor.Documento;
                    return true;

                case ProveedorDeProductoDto proveedor:
                    idProveedor = proveedor.IdProveedor;
                    razonSocial = proveedor.RazonSocial;
                    documento = proveedor.Documento;
                    return true;

                default:
                    idProveedor = 0;
                    razonSocial = string.Empty;
                    documento = string.Empty;
                    return false;
            }
        }

        // =====================================================================
        // 6. FORMATO DE PRESENTACIÓN (RNF-12)
        // =====================================================================

        private void gridLineas_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columna = gridLineas.Columns[e.ColumnIndex].Name;

            if (columna == ColumnaLineaProducto
                && e.Value is string nombreProducto
                && gridLineas.Rows[e.RowIndex].DataBoundItem is LineaCompraDto linea)
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

        /// <summary>
        /// Aplica el formato de presentación de la barra de historial: fecha y
        /// hora, importes con <c>N2</c> y estado como «Activa»/«Inactiva»
        /// (RNF-12). La grilla solo presenta los datos del negocio.
        /// </summary>
        private void gridHistorial_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columna = gridHistorial.Columns[e.ColumnIndex].Name;

            if (columna == ColumnaHistorialFecha && e.Value is DateTime fecha)
            {
                e.Value = fecha.ToString(FormatoFecha, CultureInfo.CurrentCulture);
                e.FormattingApplied = true;
            }
            else if (columna == ColumnaHistorialTotal && e.Value is decimal total)
            {
                e.Value = total.ToString(FormatoImporte, CultureInfo.CurrentCulture);
                e.FormattingApplied = true;
            }
            else if (columna == ColumnaHistorialEstado && e.Value is bool estado)
            {
                e.Value = estado ? "Activa" : "Inactiva";
                e.FormattingApplied = true;
            }
        }

        // =====================================================================
        // 7. AYUDA COMÚN DE LOS MANEJADORES
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
                lblMensaje.Text = excepcion.Message;
            }
        }
    }
}
