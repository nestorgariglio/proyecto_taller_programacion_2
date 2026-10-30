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
    /// Pantalla de carga de compras a proveedores organizada en tres bloques:
    /// producto y proveedor (búsquedas a demanda), compra actual y barra de
    /// acciones. El producto y el proveedor se buscan a demanda (RF-9 … RF-17)
    /// sobre catálogos independientes y la selección es unidireccional: elegir un
    /// producto propone los proveedores que ya lo vendieron (RF-12), mientras que
    /// elegir un proveedor solo actualiza el proveedor elegido sin tocar el
    /// listado de productos. El historial ya no se lista dentro de la pantalla:
    /// la barra de acciones abre <see cref="DetalleCompraForm"/> en modo lista
    /// (RF-52 … RF-54). La pantalla no valida, no calcula importes ni redacta
    /// mensajes: solo invoca <see cref="CompraNegocio"/> y muestra el
    /// <c>Mensaje</c> que este devuelve (RF-55, RNF-6 y RNF-13).
    /// </summary>
    public partial class ComprasForm : UserControl
    {
        private const string ColumnaLineaProducto = "colLineaProducto";
        private const string ColumnaLineaPrecioCompra = "colLineaPrecioCompra";
        private const string ColumnaLineaPrecioVenta = "colLineaPrecioVenta";
        private const string ColumnaLineaSubtotal = "colLineaSubtotal";
        private const string ColumnaLineaEditar = "colLineaEditar";
        private const string ColumnaLineaQuitar = "colLineaQuitar";

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

        /// <summary>
        /// Nivel de supresión de eventos de las grillas. Es un contador para que
        /// las supresiones anidadas (limpieza, rebind y selección) no se pisen:
        /// los manejadores salen mientras el valor sea mayor que cero.
        /// </summary>
        private int _nivelActualizando;

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
            btnVerHistorial.Click += btnVerHistorial_Click;

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
            btnVerHistorial.Enabled = habilitado;
            ActualizarEstadoAcciones();
        }

        /// <summary>
        /// Habilita la acción primaria solo si la sesión puede operar el módulo, no
        /// hay un registro en curso y la compra en carga tiene proveedor e ítems
        /// (RF-58 y <c>desktop-ux-patterns</c> §3). No valida ni redacta mensajes:
        /// solo refleja el estado de la pantalla.
        /// </summary>
        private void ActualizarEstadoAcciones()
        {
            btnRegistrar.Enabled = PoliticaAcceso.PuedeOperarCompras(_rolActual)
                && !_registrando
                && _idProveedorElegido.HasValue
                && _lineas.Count > 0;
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

            // Filas alternadas para leer renglones largos (desktop-ux-patterns §2).
            // Solo cambia el fondo; la alineación se hereda de DefaultCellStyle y de
            // la alineación propia de cada columna numérica.
            grilla.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(56, 56, 56),
                ForeColor = Color.White,
                SelectionBackColor = Color.FromArgb(255, 87, 34),
                SelectionForeColor = Color.White
            };

            grilla.GridColor = Color.FromArgb(80, 80, 80);
        }

        // =====================================================================
        // 2. BÚSQUEDAS A DEMANDA Y SELECCIÓN PRODUCTO → PROVEEDORES
        // =====================================================================

        private void txtBuscarProveedor_TextChanged(object? sender, EventArgs e)
        {
            if (_nivelActualizando > 0)
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
            string texto = txtBuscarProveedor.Text;

            if (string.IsNullOrWhiteSpace(texto))
            {
                using (SuspenderEventos())
                {
                    _proveedoresBuscados.Clear();
                    _proveedoresHistoricos.Clear();
                    MostrarProveedores(null);
                }

                lblMensajeProveedores.Text = string.Empty;
                return;
            }

            RespuestaBusquedaProveedoresCompra respuesta =
                await _negocio.BuscarProveedoresAsync(texto);

            if (EsRespuestaObsoleta(texto, txtBuscarProveedor.Text))
            {
                return;
            }

            using (SuspenderEventos())
            {
                _proveedoresBuscados.Clear();
                _proveedoresBuscados.AddRange(respuesta.Proveedores);
                _proveedoresHistoricos.Clear();
                MostrarProveedores(_proveedoresBuscados.ToList());
            }

            lblMensajeProveedores.Text = respuesta.Mensaje;
        }

        private void txtBuscarProducto_TextChanged(object? sender, EventArgs e)
        {
            if (_nivelActualizando > 0)
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
            string texto = txtBuscarProducto.Text;

            if (string.IsNullOrWhiteSpace(texto))
            {
                using (SuspenderEventos())
                {
                    _productosBuscados.Clear();
                    MostrarProductos(null);
                }

                lblMensajeProductos.Text = string.Empty;
                return;
            }

            RespuestaBusquedaProductosCompra respuesta =
                await _negocio.BuscarProductosAsync(texto);

            if (EsRespuestaObsoleta(texto, txtBuscarProducto.Text))
            {
                return;
            }

            using (SuspenderEventos())
            {
                _productosBuscados.Clear();
                _productosBuscados.AddRange(respuesta.Productos);
                MostrarProductos(_productosBuscados.ToList());
            }

            lblMensajeProductos.Text = respuesta.Mensaje;
        }

        private async void gridProductos_SelectionChanged(object? sender, EventArgs e)
        {
            if (_nivelActualizando > 0 || gridProductos.DataSource is null)
            {
                return;
            }

            DataGridViewRow? fila = gridProductos.CurrentRow;

            if (fila is null || fila.Index < 0)
            {
                return;
            }

            if (!TryObtenerProductoSeleccionado(fila.DataBoundItem, out int idProducto,
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

                if (IsDisposed || _idProductoElegido != idProducto)
                {
                    return;
                }

                // RF-38 / RF-39: el foco queda en Cantidad con el texto seleccionado.
                txtCantidad.Focus();
                txtCantidad.SelectAll();

                await ResolverProveedoresDelProductoAsync(idProducto);
            });
        }

        private void gridProveedores_SelectionChanged(object? sender, EventArgs e)
        {
            if (_nivelActualizando > 0 || gridProveedores.DataSource is null)
            {
                return;
            }

            DataGridViewRow? fila = gridProveedores.CurrentRow;

            if (fila is null || fila.Index < 0)
            {
                return;
            }

            if (!TryObtenerProveedorSeleccionado(fila.DataBoundItem,
                    out int idProveedor, out string razonSocial, out string documento))
            {
                return;
            }

            if (_idProveedorElegido == idProveedor)
            {
                return;
            }

            ActualizarProveedorSeleccionado(idProveedor, razonSocial, documento);
        }

        /// <summary>
        /// Fija el proveedor elegido y refresca solo sus rótulos descriptivos y el
        /// estado de la acción primaria (RF-12 y RF-58). No invoca el negocio ni
        /// toca el listado de productos: las dos búsquedas son independientes
        /// (RF-15, RF-16 y RF-17).
        /// </summary>
        /// <param name="idProveedor">Identificador del proveedor elegido.</param>
        /// <param name="razonSocial">Razón social del proveedor elegido.</param>
        /// <param name="documento">Documento del proveedor elegido.</param>
        private void ActualizarProveedorSeleccionado(
            int idProveedor, string razonSocial, string documento)
        {
            _idProveedorElegido = idProveedor;
            _razonSocialProveedor = razonSocial;
            _documentoProveedor = documento;

            // MostrarDatosProveedor refresca los dos rótulos y, con ellos, el
            // estado de «REGISTRAR COMPRA» (RF-58).
            MostrarDatosProveedor();
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

            if (IsDisposed || _idProductoElegido != idProducto)
            {
                return;
            }

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

            if (IsDisposed || _idProductoElegido != idProducto)
            {
                return;
            }

            using (SuspenderEventos())
            {
                _proveedoresBuscados.Clear();
                _proveedoresHistoricos.Clear();

                switch (respuesta.Resultado)
                {
                    case ResultadoConsultaHistoricaCompra.ConResultados:
                        _proveedoresHistoricos.AddRange(respuesta.Proveedores);
                        MostrarProveedores(_proveedoresHistoricos.ToList());

                        if (_proveedoresHistoricos.Count == 1)
                        {
                            ProveedorDeProductoDto unico = _proveedoresHistoricos[0];

                            // RF-12: con un solo proveedor histórico se autocompleta,
                            // salvo que ya hubiera uno elegido, que no se descarta (RF-13).
                            if (!_idProveedorElegido.HasValue)
                            {
                                ActualizarProveedorSeleccionado(
                                    unico.IdProveedor, unico.RazonSocial, unico.Documento);
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
            if (_nivelActualizando > 0 || gridLineas.DataSource is null)
            {
                return;
            }

            if (e.RowIndex < 0 || e.ColumnIndex < 0
                || e.RowIndex >= gridLineas.Rows.Count
                || e.ColumnIndex >= gridLineas.Columns.Count)
            {
                return;
            }

            if (gridLineas.Rows[e.RowIndex].DataBoundItem is not LineaCompraDto linea)
            {
                return;
            }

            int indice = _lineas.IndexOf(linea);

            if (indice < 0)
            {
                return;
            }

            string columna = gridLineas.Columns[e.ColumnIndex].Name;

            if (columna == ColumnaLineaQuitar)
            {
                QuitarLineaDeGrilla(indice);
            }
            else if (columna == ColumnaLineaEditar)
            {
                EditarLineaEnCaptura(indice);
            }
        }

        /// <summary>
        /// Quita la línea indicada con el resultado del negocio: si falla muestra
        /// su mensaje y no toca la grilla (RF-29). Sin un índice válido no hace nada.
        /// </summary>
        /// <param name="indice">Fila de la grilla seleccionada.</param>
        private void QuitarLineaDeGrilla(int indice)
        {
            if (_nivelActualizando > 0 || indice < 0 || indice >= _lineas.Count)
            {
                return;
            }

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
        /// (RF-30 y RF-62). No consulta la base ni recalcula importes. Sin un
        /// índice válido no hace nada.
        /// </summary>
        /// <param name="indice">Fila de la grilla seleccionada.</param>
        private void EditarLineaEnCaptura(int indice)
        {
            if (_nivelActualizando > 0 || indice < 0 || indice >= _lineas.Count)
            {
                return;
            }

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
            using (SuspenderEventos())
            {
                gridLineas.CurrentCell = null;
                gridLineas.DataSource = null;
                gridLineas.DataSource = _lineas.ToList();
            }

            decimal montoTotal =
                CalculoImportesCompra.CalcularMontoTotal(_lineas.Select(linea => linea.Subtotal));

            lblMontoTotal.Text = $"Total: {montoTotal.ToString("N2", CultureInfo.CurrentCulture)}";

            ActualizarEstadoAcciones();
        }

        // =====================================================================
        // 4. REGISTRO, LIMPIEZA E HISTORIAL (el historial se abre en el modal)
        // =====================================================================

        private async void btnRegistrar_Click(object? sender, EventArgs e)
        {
            await EjecutarAsync(RegistrarCompraAsync);
        }

        /// <summary>
        /// Registra la compra con el proveedor elegido y las líneas de la grilla.
        /// Mientras dura la operación el botón queda deshabilitado y la bandera
        /// ignora reentradas (RF-58). En éxito descarta la compra en carga (RF-32);
        /// el historial ya no se refresca en esta pantalla (RF-52 se cumple desde
        /// el modal). En rechazo conserva todo (RNF-11).
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
            }
            finally
            {
                _registrando = false;
                ActualizarEstadoAcciones();
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

        private void btnVerHistorial_Click(object? sender, EventArgs e)
        {
            // El historial vive ahora en el modal (RF-49 … RF-54): se abre en modo
            // lista y cada fila elegida carga su detalle. Al cerrarse no altera la
            // compra en carga.
            using DetalleCompraForm historial = new DetalleCompraForm(_negocio, _rolActual);
            historial.ShowDialog(this);
        }

        /// <summary>
        /// Descarta el proveedor elegido, las líneas y el área de captura, tanto
        /// tras registrar (RF-32) como tras confirmar la limpieza (RF-31).
        /// </summary>
        private void LimpiarTrasRegistro()
        {
            using (SuspenderEventos())
            {
                _idProveedorElegido = null;
                _razonSocialProveedor = null;
                _documentoProveedor = null;
                _proveedoresBuscados.Clear();
                _proveedoresHistoricos.Clear();
                _temporizadorProveedores.Stop();
                txtBuscarProveedor.Text = string.Empty;
                MostrarProveedores(null);
                lblMensajeProveedores.Text = string.Empty;
                MostrarDatosProveedor();
                _lineas.Clear();
                LimpiarCaptura();
                RefrescarGrillaLineas();
            }
        }

        /// <summary>
        /// Deja el área de captura vacía y sin modo edición, y vuelve a dejar el
        /// listado de productos vacío hasta la próxima búsqueda (RF-16).
        /// </summary>
        private void LimpiarCaptura()
        {
            using (SuspenderEventos())
            {
                _indiceEdicion = null;
                _idProductoElegido = null;
                _codigoProductoElegido = null;
                _nombreProductoElegido = null;
                _productosBuscados.Clear();
                _temporizadorProductos.Stop();
                txtBuscarProducto.Text = string.Empty;
                txtCantidad.Text = string.Empty;
                txtPrecioCompra.Text = string.Empty;
                txtPrecioVenta.Text = string.Empty;
                MostrarProductos(null);
                lblMensajeProductos.Text = string.Empty;
                MostrarDatosProducto();
                btnAgregarLinea.Text = TextoBotonAgregar;
            }

            ActualizarSubtotalPreparacion();
        }

        // =====================================================================
        // 5. PRESENTACIÓN DE LOS LISTADOS Y DATOS SELECCIONADOS
        // =====================================================================

        /// <summary>
        /// Vuelve a enlazar el listado de productos sin disparar la selección
        /// automática de la primera fila. El enlace usa siempre una copia nueva de
        /// los resultados, de modo que vaciar la lista de campo no altera el
        /// <c>DataSource</c> ni deja el <c>CurrencyManager</c> en una fila inválida.
        /// </summary>
        /// <param name="productos">Resultados a mostrar o <see langword="null"/> para vaciarlo.</param>
        private void MostrarProductos(IEnumerable<ProductoBuscadoDto>? productos)
        {
            using (SuspenderEventos())
            {
                gridProductos.CurrentCell = null;
                gridProductos.DataSource = null;
                gridProductos.DataSource = productos?.ToList();
                gridProductos.ClearSelection();
                gridProductos.CurrentCell = null;
            }
        }

        /// <summary>
        /// Vuelve a enlazar el listado de proveedores sin disparar la selección
        /// automática de la primera fila. El enlace usa siempre una copia nueva de
        /// los resultados, de modo que vaciar la lista de campo no altera el
        /// <c>DataSource</c> ni deja el <c>CurrencyManager</c> en una fila inválida.
        /// </summary>
        /// <param name="origen">Lista de resultados o <see langword="null"/> para vaciarlo.</param>
        private void MostrarProveedores(object? origen)
        {
            object? copia = origen switch
            {
                IEnumerable<ProveedorSeleccionadoDto> buscados => buscados.ToList(),
                IEnumerable<ProveedorDeProductoDto> historicos => historicos.ToList(),
                _ => null
            };

            using (SuspenderEventos())
            {
                gridProveedores.CurrentCell = null;
                gridProveedores.DataSource = null;
                gridProveedores.DataSource = copia;
                gridProveedores.ClearSelection();
                gridProveedores.CurrentCell = null;
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

            // RF-58: sin proveedor elegido la acción primaria queda deshabilitada.
            ActualizarEstadoAcciones();
        }

        private void SeleccionarProveedorEnGrilla(int idProveedor)
        {
            if (IsDisposed || !gridProveedores.IsHandleCreated)
            {
                return;
            }

            if (gridProveedores.Columns.Count == 0 || gridProveedores.Rows.Count == 0)
            {
                return;
            }

            using (SuspenderEventos())
            {
                foreach (DataGridViewRow fila in gridProveedores.Rows)
                {
                    if (fila.DataBoundItem is ProveedorDeProductoDto proveedor
                        && proveedor.IdProveedor == idProveedor
                        && fila.Cells.Count > 0)
                    {
                        gridProveedores.CurrentCell = fila.Cells[0];
                        break;
                    }
                }
            }
        }

        private static bool TryObtenerProductoSeleccionado(
            object? item, out int idProducto, out string codigo, out string nombre)
        {
            if (item is ProductoBuscadoDto producto)
            {
                idProducto = producto.IdProducto;
                codigo = producto.Codigo;
                nombre = producto.Nombre;
                return true;
            }

            idProducto = 0;
            codigo = string.Empty;
            nombre = string.Empty;
            return false;
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
            if (_nivelActualizando > 0 || gridLineas.DataSource is null)
            {
                return;
            }

            if (e.RowIndex < 0 || e.ColumnIndex < 0
                || e.RowIndex >= gridLineas.Rows.Count
                || e.ColumnIndex >= gridLineas.Columns.Count)
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

        // =====================================================================
        // 7. AYUDA COMÚN DE LOS MANEJADORES
        // =====================================================================

        /// <summary>
        /// Indica si la respuesta de una búsqueda llegó tarde: la pantalla ya se
        /// liberó o el texto consultado ya no es el que está escrito. La respuesta
        /// obsoleta se descarta sin tocar la grilla (RF-10, RF-16, RNF-11).
        /// </summary>
        /// <param name="textoConsultado">Texto con el que se hizo la consulta.</param>
        /// <param name="textoActual">Texto vigente en el cuadro de búsqueda.</param>
        /// <returns><see langword="true"/> si la respuesta debe descartarse.</returns>
        private bool EsRespuestaObsoleta(string textoConsultado, string textoActual)
            => IsDisposed || !string.Equals(textoConsultado, textoActual, StringComparison.Ordinal);

        /// <summary>
        /// Suspende los manejadores de las grillas mientras dure la mutación de
        /// datos y el rebindeo. Se apoya en un contador para que las supresiones
        /// anidadas restauren el nivel previo en lugar de forzarlo a cero.
        /// </summary>
        /// <returns>Objeto desechable que restaura el nivel al liberarse.</returns>
        private IDisposable SuspenderEventos() => new Suspension(this);

        /// <summary>
        /// Restauración del nivel de supresión al liberar el bloque <c>using</c>.
        /// </summary>
        private sealed class Suspension : IDisposable
        {
            private readonly ComprasForm _formulario;
            private bool _liberada;

            /// <summary>
            /// Incrementa el nivel de supresión del formulario.
            /// </summary>
            /// <param name="formulario">Formulario que suspende sus manejadores.</param>
            public Suspension(ComprasForm formulario)
            {
                ArgumentNullException.ThrowIfNull(formulario);

                _formulario = formulario;
                _formulario._nivelActualizando++;
            }

            /// <summary>
            /// Decrementa el nivel de supresión sin ponerlo nunca por debajo de cero.
            /// </summary>
            public void Dispose()
            {
                if (_liberada)
                {
                    return;
                }

                _liberada = true;

                if (_formulario._nivelActualizando > 0)
                {
                    _formulario._nivelActualizando--;
                }
            }
        }

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
