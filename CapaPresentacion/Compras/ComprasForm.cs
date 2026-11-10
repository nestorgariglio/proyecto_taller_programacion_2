using System;
using System.Collections;
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
    /// sobre catálogos independientes. El historial de compras solo prioriza:
    /// elegir un producto muestra primero los proveedores que ya lo vendieron y
    /// elegir un proveedor muestra primero sus productos habituales; las casillas
    /// «Mostrar todos» amplían cada listado a todos los activos y la búsqueda por
    /// texto se aplica sobre el universo vigente. Nunca se autoselecciona una
    /// fila ni se descarta lo elegido. Cada selector tiene una única rutina de
    /// refresco, sin encadenar consultas ni bucles de eventos; ninguna selección
    /// programática dispara el flujo inverso. El
    /// historial ya no se lista dentro de la pantalla: la barra de acciones abre
    /// <see cref="DetalleCompraForm"/> en modo lista (RF-52 … RF-54). La pantalla
    /// no valida, no calcula importes ni redacta mensajes: solo invoca
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

        private const string TextoProveedorSinElegir = "Sin proveedor seleccionado";
        private const string TextoDocumentoSinElegir = "CUIT: —";
        private const string TextoProductoSinElegir = "(Sin producto seleccionado)";
        private const string TextoCodigoSinElegir = "Código: —";
        private const string TextoSubtotalInvalido = "Subtotal: —";
        private const string TextoSeleccioneProveedor = "Seleccione el proveedor...";
        private const string TextoProveedorFueraDelListado = "El proveedor elegido no figura en este listado.";
        private const string TextoProductoFueraDelListado = "El producto elegido no figura en este listado.";
        private const string TextoBotonAgregar = "Agregar a la compra";
        private const string TextoBotonGuardar = "Guardar línea";

        private readonly CompraNegocio _negocio;
        private readonly System.Windows.Forms.Timer _temporizadorProveedores =
            new() { Interval = 350 };
        private readonly System.Windows.Forms.Timer _temporizadorProductos =
            new() { Interval = 350 };
        private readonly List<LineaCompraDto> _lineas = new();
        private readonly List<ProveedorSeleccionadoDto> _proveedoresBuscados = new();
        private readonly List<ProductoBuscadoDto> _productosBuscados = new();

        /// <summary>
        /// Último listado completo de productos (texto vacío) con la clave que lo
        /// produjo: permite restituirlo sin consultar al limpiar la captura (RF-16).
        /// </summary>
        private readonly List<ProductoBuscadoDto> _cacheProductos = new();
        private (int? IdProveedor, bool MostrarTodos)? _claveCacheProductos;

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
        private bool _actualizandoSeleccion;

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
            AplicarColorFilaElegida(gridProveedores);
            AplicarColorFilaElegida(gridProductos);
            AlinearCabecerasNumericas();
            AplicarEscalaYDisposicion();

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
            chkMostrarTodosProveedores.CheckedChanged += chkMostrarTodosProveedores_CheckedChanged;
            chkMostrarTodosProductos.CheckedChanged += chkMostrarTodosProductos_CheckedChanged;
            lnkDeseleccionarProveedor.LinkClicked += lnkDeseleccionarProveedor_LinkClicked;
            lnkDeseleccionarProducto.LinkClicked += lnkDeseleccionarProducto_LinkClicked;
            ConectarAccionDeFila(gridProveedores);
            ConectarAccionDeFila(gridProductos);

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
            chkMostrarTodosProveedores.Enabled = habilitado;
            chkMostrarTodosProductos.Enabled = habilitado;
            txtCantidad.Enabled = habilitado;
            txtPrecioCompra.Enabled = habilitado;
            txtPrecioVenta.Enabled = habilitado;
            btnAgregarLinea.Enabled = habilitado;
            btnLimpiar.Enabled = habilitado;
            btnVerHistorial.Enabled = habilitado;
            ActualizarEstadoAcciones();
            ActualizarAccionesDeSeleccion();
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

            // Los estilos se clonan desde el diseñador y solo se mutan los colores,
            // la fuente y el ajuste de línea: así se conservan la alineación de los
            // importes a la derecha y el formato que aporta cada columna.
            grilla.ColumnHeadersDefaultCellStyle = grilla.ColumnHeadersDefaultCellStyle.Clone();
            grilla.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(55, 71, 79);
            grilla.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grilla.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(55, 71, 79);
            grilla.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            grilla.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grilla.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

            grilla.DefaultCellStyle = grilla.DefaultCellStyle.Clone();
            grilla.DefaultCellStyle.BackColor = Color.FromArgb(48, 48, 48);
            grilla.DefaultCellStyle.ForeColor = Color.White;
            grilla.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 87, 34);
            grilla.DefaultCellStyle.SelectionForeColor = Color.White;
            grilla.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            grilla.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            // Filas alternadas para leer renglones largos (desktop-ux-patterns §2).
            // Se hereda de DefaultCellStyle y solo cambia el fondo. La alineación se deja
            // sin definir (NotSet): el estilo de filas alternadas tiene prioridad sobre el
            // de columna, y si copiara MiddleLeft de DefaultCellStyle los importes
            // alineados a la derecha quedarían a la izquierda en las filas impares.
            grilla.AlternatingRowsDefaultCellStyle = grilla.DefaultCellStyle.Clone();
            grilla.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(56, 56, 56);
            grilla.AlternatingRowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.NotSet;

            grilla.GridColor = Color.FromArgb(80, 80, 80);
        }

        /// <summary>
        /// Pinta la fila elegida de los selectores con el color de acento oscuro
        /// <see cref="PintorAccionFila.ColorFilaElegida"/> y texto blanco (5,60:1), también en
        /// las filas alternadas. Una grilla de WinForms no atenúa la selección al perder el
        /// foco: mantiene <c>SelectionBackColor</c>. Solo se aplica a los dos selectores.
        /// </summary>
        /// <param name="grilla">Grilla de proveedores o de productos.</param>
        private static void AplicarColorFilaElegida(DataGridView grilla)
        {
            grilla.DefaultCellStyle.SelectionBackColor = PintorAccionFila.ColorFilaElegida;
            grilla.DefaultCellStyle.SelectionForeColor = PintorAccionFila.ColorTextoFilaElegida;
            grilla.AlternatingRowsDefaultCellStyle.SelectionBackColor = PintorAccionFila.ColorFilaElegida;
            grilla.AlternatingRowsDefaultCellStyle.SelectionForeColor = PintorAccionFila.ColorTextoFilaElegida;
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
            await EjecutarAsync(RefrescarProveedoresAsync);
        }

        private async void chkMostrarTodosProveedores_CheckedChanged(object? sender, EventArgs e)
        {
            if (_nivelActualizando > 0)
            {
                return;
            }

            _temporizadorProveedores.Stop();
            await EjecutarAsync(RefrescarProveedoresAsync);
        }

        /// <summary>
        /// Única rutina de refresco del selector de proveedores. Depende del texto
        /// de búsqueda, del producto elegido (solo prioriza con el historial) y de
        /// la casilla «Mostrar todos los proveedores». No modifica la selección: tras
        /// enlazar el listado restituye la fila del proveedor elegido si está en él,
        /// sin disparar el manejador de selección, y nunca selecciona otra fila.
        /// </summary>
        /// <returns>Tarea que representa el refresco.</returns>
        private async Task RefrescarProveedoresAsync()
        {
            string texto = txtBuscarProveedor.Text;
            int? idProducto = _idProductoElegido;
            bool mostrarTodos = chkMostrarTodosProveedores.Checked;

            RespuestaBusquedaProveedoresCompra respuesta =
                await _negocio.BuscarProveedoresParaCompraAsync(texto, idProducto, mostrarTodos, _rolActual);

            if (EsRespuestaObsoleta(texto, txtBuscarProveedor.Text)
                || idProducto != _idProductoElegido
                || mostrarTodos != chkMostrarTodosProveedores.Checked)
            {
                return;
            }

            if (respuesta.Resultado == ResultadoBusquedaCompras.ErrorPersistencia)
            {
                // Solo se informa el mensaje del negocio, sin tocar el listado (RF-57).
                lblMensajeProveedores.Text = respuesta.Mensaje;
                return;
            }

            using (SuspenderSeleccion())
            {
                _proveedoresBuscados.Clear();
                _proveedoresBuscados.AddRange(respuesta.Proveedores);
                MostrarProveedores(_proveedoresBuscados.ToList());

                if (_idProveedorElegido.HasValue)
                {
                    SeleccionarProveedorEnGrilla(_idProveedorElegido.Value);
                }
            }

            // El elegido sigue vigente en sus rótulos aunque no figure en el listado;
            // se avisa para que la grilla sin fila marcada no resulte confusa.
            bool elegidoFuera = _idProveedorElegido.HasValue
                && respuesta.Proveedores.Count > 0
                && !respuesta.Proveedores.Any(p => p.IdProveedor == _idProveedorElegido.Value);

            lblMensajeProveedores.Text = elegidoFuera ? TextoProveedorFueraDelListado : respuesta.Mensaje;
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
            await EjecutarAsync(RefrescarProductosAsync);
        }

        private async void chkMostrarTodosProductos_CheckedChanged(object? sender, EventArgs e)
        {
            if (_nivelActualizando > 0)
            {
                return;
            }

            _temporizadorProductos.Stop();
            await EjecutarAsync(RefrescarProductosAsync);
        }

        /// <summary>
        /// Única rutina de refresco del selector de productos. Depende del texto de
        /// búsqueda, del proveedor elegido (solo prioriza con el historial) y de la
        /// casilla «Mostrar todos los productos». No modifica la selección: tras
        /// enlazar el listado restituye la fila del producto elegido si está en él,
        /// sin disparar el manejador de selección, y nunca selecciona otra fila.
        /// </summary>
        /// <returns>Tarea que representa el refresco.</returns>
        private async Task RefrescarProductosAsync()
        {
            string texto = txtBuscarProducto.Text;
            int? idProveedor = _idProveedorElegido;
            bool mostrarTodos = chkMostrarTodosProductos.Checked;

            RespuestaBusquedaProductosCompra respuesta =
                await _negocio.BuscarProductosParaCompraAsync(texto, idProveedor, mostrarTodos, _rolActual);

            if (EsRespuestaObsoleta(texto, txtBuscarProducto.Text)
                || idProveedor != _idProveedorElegido
                || mostrarTodos != chkMostrarTodosProductos.Checked)
            {
                return;
            }

            if (respuesta.Resultado == ResultadoBusquedaCompras.ErrorPersistencia)
            {
                // Solo se informa el mensaje del negocio, sin tocar el listado (RF-57).
                lblMensajeProductos.Text = respuesta.Mensaje;
                return;
            }

            using (SuspenderSeleccion())
            {
                _productosBuscados.Clear();
                _productosBuscados.AddRange(respuesta.Productos);
                MostrarProductos(_productosBuscados.ToList());
                SeleccionarProductoEnGrilla(_idProductoElegido);
            }

            if (string.IsNullOrWhiteSpace(texto))
            {
                _cacheProductos.Clear();
                _cacheProductos.AddRange(respuesta.Productos);
                _claveCacheProductos = (idProveedor, mostrarTodos);
            }

            // El elegido sigue vigente en sus rótulos aunque no figure en el listado;
            // se avisa para que la grilla sin fila marcada no resulte confusa.
            bool elegidoFuera = _idProductoElegido.HasValue
                && respuesta.Productos.Count > 0
                && !respuesta.Productos.Any(p => p.IdProducto == _idProductoElegido.Value);

            lblMensajeProductos.Text = elegidoFuera ? TextoProductoFueraDelListado : respuesta.Mensaje;
        }

        private async void gridProductos_SelectionChanged(object? sender, EventArgs e)
        {
            if (SeleccionSuspendida || gridProductos.DataSource is null)
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

                // El historial solo prioriza: se refresca el listado de proveedores
                // sin tocar el proveedor elegido ni autoseleccionar ninguno.
                await RefrescarProveedoresAsync();
            });
        }

        private async void gridProveedores_SelectionChanged(object? sender, EventArgs e)
        {
            if (SeleccionSuspendida || gridProveedores.DataSource is null)
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

            // Elegir proveedor refresca solo el listado de productos (prioriza los
            // habituales); no cambia el producto elegido ni encadena otros eventos.
            await EjecutarAsync(RefrescarProductosAsync);
        }

        /// <summary>
        /// Fija el proveedor elegido y refresca solo sus rótulos descriptivos y el
        /// estado de la acción primaria (RF-12 y RF-58). No invoca el negocio ni
        /// toca por sí mismo el listado de productos: el refresco del listado
        /// de productos lo resuelve su manejador.
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
            ActualizarAccionesDeSeleccion();

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
                ActualizarAccionesDeSeleccion();
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
                chkMostrarTodosProveedores.Checked = false;
                chkMostrarTodosProductos.Checked = false;
                _cacheProductos.Clear();
                _claveCacheProductos = null;
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
        /// Deja el área de captura vacía y sin modo edición. El listado de
        /// productos se restituye desde la caché si coincide con el proveedor vigente y
        /// la casilla «Mostrar todos los productos»; si no hay caché coherente queda
        /// vacío, sin consultar el origen de datos (RF-16). No modifica las casillas
        /// ni el listado de proveedores.
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

            // Sin consultar: restituye desde la caché el listado del proveedor vigente.
            RestituirProductosDesdeCache();
            ActualizarSubtotalPreparacion();
        }

        /// <summary>
        /// Restituye el último listado completo de productos si su clave (proveedor y
        /// casilla) coincide con el estado actual. Sin caché coherente no hace nada:
        /// el listado queda vacío. Nunca consulta el origen de datos (RF-16).
        /// </summary>
        private void RestituirProductosDesdeCache()
        {
            if (!string.IsNullOrWhiteSpace(txtBuscarProducto.Text)
                || _claveCacheProductos is not { } clave
                || clave.IdProveedor != _idProveedorElegido
                || clave.MostrarTodos != chkMostrarTodosProductos.Checked
                || _cacheProductos.Count == 0)
            {
                return;
            }

            using (SuspenderSeleccion())
            {
                MostrarProductos(_cacheProductos.ToList());
                SeleccionarProductoEnGrilla(_idProductoElegido);
            }
        }

        /// <summary>
        /// Deselecciona el proveedor elegido sin tocar textos, casillas, producto,
        /// captura ni líneas, y refresca el listado de productos sin contexto de
        /// proveedor. Cambiar el contexto invalida por sí solo las respuestas pendientes.
        /// </summary>
        /// <returns>Tarea que representa el refresco del listado de productos.</returns>
        private async Task QuitarProveedorAsync()
        {
            _idProveedorElegido = null;
            _razonSocialProveedor = null;
            _documentoProveedor = null;
            MostrarDatosProveedor();
            QuitarMarcaDeGrilla(gridProveedores);

            await RefrescarProductosAsync();
        }

        /// <summary>
        /// Deselecciona el producto elegido, limpia la captura prellenada y refresca
        /// el listado de proveedores sin contexto de producto. No toca al proveedor
        /// elegido, los textos de búsqueda, las casillas ni las líneas.
        /// </summary>
        /// <returns>Tarea que representa el refresco del listado de proveedores.</returns>
        private async Task QuitarProductoAsync()
        {
            _idProductoElegido = null;
            _codigoProductoElegido = null;
            _nombreProductoElegido = null;

            txtCantidad.Text = string.Empty;
            txtPrecioCompra.Text = string.Empty;
            txtPrecioVenta.Text = string.Empty;

            MostrarDatosProducto();
            QuitarMarcaDeGrilla(gridProductos);
            ActualizarSubtotalPreparacion();

            await RefrescarProveedoresAsync();
        }

        /// <summary>
        /// Quita la fila marcada de una grilla sin disparar su manejador de selección.
        /// </summary>
        /// <param name="grilla">Grilla cuya marca se quita.</param>
        private void QuitarMarcaDeGrilla(DataGridView grilla)
        {
            using (SuspenderSeleccion())
            {
                grilla.CurrentCell = null;
                grilla.ClearSelection();
            }
        }

        // =====================================================================
        // 4b. ACCIÓN «DESELECCIONAR» DENTRO DE LA FILA ELEGIDA
        // =====================================================================

        private const string TextoDeseleccionarProveedor = "Deseleccionar proveedor";
        private const string TextoDeseleccionarProducto = "Deseleccionar producto";

        /// <summary>
        /// Estado del puntero sobre el botón de una grilla: fila con el puntero sobre el
        /// botón y fila con el botón presionado (<c>-1</c> si no hay ninguna).
        /// </summary>
        private sealed class EstadoAccionGrilla
        {
            public int FilaSobreBoton { get; set; } = -1;

            public int FilaPresionada { get; set; } = -1;
        }

        private readonly EstadoAccionGrilla _accionProveedores = new();
        private readonly EstadoAccionGrilla _accionProductos = new();

        /// <summary>
        /// Conecta a una grilla de selección los eventos que dibujan y operan el botón
        /// «Deseleccionar» de su última columna.
        /// </summary>
        /// <param name="grilla">Grilla de proveedores o de productos.</param>
        private void ConectarAccionDeFila(DataGridView grilla)
        {
            grilla.CellPainting += gridAccion_CellPainting;
            grilla.CellMouseMove += gridAccion_CellMouseMove;
            grilla.CellMouseLeave += gridAccion_CellMouseLeave;
            grilla.CellMouseDown += gridAccion_CellMouseDown;
            grilla.CellMouseUp += gridAccion_CellMouseUp;
            grilla.CellMouseClick += gridAccion_CellMouseClick;
            grilla.CellToolTipTextNeeded += gridAccion_CellToolTipTextNeeded;
            grilla.SizeChanged += gridAccion_SizeChanged;
        }

        private EstadoAccionGrilla EstadoAccionDe(DataGridView grilla)
            => ReferenceEquals(grilla, gridProveedores) ? _accionProveedores : _accionProductos;

        private DataGridViewColumn ColumnaAccionDe(DataGridView grilla)
            => ReferenceEquals(grilla, gridProveedores) ? colProveedorAccion : colProductoAccion;

        /// <summary>
        /// Indica si la sesión puede operar el módulo y no hay un registro en curso.
        /// </summary>
        private bool PuedeOperarSelectores
            => PoliticaAcceso.PuedeOperarCompras(_rolActual) && !_registrando;

        private bool PuedeDeseleccionarProveedor()
            => PuedeOperarSelectores && _idProveedorElegido.HasValue;

        /// <summary>
        /// «Deseleccionar producto» no se ofrece mientras se edita una línea.
        /// </summary>
        private bool PuedeDeseleccionarProducto()
            => PuedeOperarSelectores && _idProductoElegido.HasValue && !_indiceEdicion.HasValue;

        /// <summary>
        /// Obtiene el identificador de la entidad que muestra una fila leyendo la lista
        /// enlazada, sin tocar <c>Rows[i]</c>: así el dibujo no desacopla filas compartidas.
        /// </summary>
        /// <param name="grilla">Grilla de proveedores o de productos.</param>
        /// <param name="indiceFila">Índice de la fila en la grilla.</param>
        /// <returns>El identificador o <see langword="null"/> si la fila no existe.</returns>
        private int? ObtenerIdDeFila(DataGridView grilla, int indiceFila)
        {
            if (grilla.DataSource is not IList lista || indiceFila < 0 || indiceFila >= lista.Count)
            {
                return null;
            }

            object? elemento = lista[indiceFila];

            if (ReferenceEquals(grilla, gridProveedores))
            {
                return TryObtenerProveedorSeleccionado(elemento, out int idProveedor, out _, out _)
                    ? idProveedor
                    : null;
            }

            return TryObtenerProductoSeleccionado(elemento, out int idProducto, out _, out _)
                ? idProducto
                : null;
        }

        /// <summary>
        /// Indica si la entidad elegida de la grilla figura en su listado vigente
        /// (comparación por identificador).
        /// </summary>
        /// <param name="grilla">Grilla de proveedores o de productos.</param>
        private bool ElegidoEstaEnGrilla(DataGridView grilla)
        {
            int? idElegido = ReferenceEquals(grilla, gridProveedores)
                ? _idProveedorElegido
                : _idProductoElegido;

            if (!idElegido.HasValue || grilla.DataSource is not IList lista)
            {
                return false;
            }

            for (int indice = 0; indice < lista.Count; indice++)
            {
                if (ObtenerIdDeFila(grilla, indice) == idElegido)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Indica si el botón de la columna de acción de una fila debe mostrarse: la
        /// columna cabe en la grilla, se puede operar y la fila es la de la entidad elegida
        /// (por identificador, nunca por posición ni por <c>Selected</c>).
        /// </summary>
        /// <param name="grilla">Grilla de proveedores o de productos.</param>
        /// <param name="indiceFila">Índice de la fila en la grilla.</param>
        private bool EsAccionVisibleEnFila(DataGridView grilla, int indiceFila)
        {
            bool esProveedores = ReferenceEquals(grilla, gridProveedores);
            bool puede = esProveedores ? PuedeDeseleccionarProveedor() : PuedeDeseleccionarProducto();
            int? idElegido = esProveedores ? _idProveedorElegido : _idProductoElegido;

            return puede
                && ColumnaAccionDe(grilla).Visible
                && idElegido.HasValue
                && ObtenerIdDeFila(grilla, indiceFila) == idElegido;
        }

        /// <summary>
        /// Refresca todo lo que depende de qué está elegido y de si se puede operar: invalida
        /// la columna de acción de ambas grillas, restablece el hover y el cursor y muestra el
        /// enlace «Deseleccionar» de respaldo solo cuando hay algo para deseleccionar y su
        /// botón no está disponible en la grilla (el elegido no figura en el listado o la
        /// columna no cabe).
        /// </summary>
        private void ActualizarAccionesDeSeleccion()
        {
            RestablecerEstadoAccion(gridProveedores);
            RestablecerEstadoAccion(gridProductos);

            lnkDeseleccionarProveedor.Visible = PuedeDeseleccionarProveedor()
                && !BotonEnFilaDisponible(gridProveedores);
            lnkDeseleccionarProducto.Visible = PuedeDeseleccionarProducto()
                && !BotonEnFilaDisponible(gridProductos);
        }

        private bool BotonEnFilaDisponible(DataGridView grilla)
            => ColumnaAccionDe(grilla).Visible && ElegidoEstaEnGrilla(grilla);

        private void RestablecerEstadoAccion(DataGridView grilla)
        {
            EstadoAccionGrilla estado = EstadoAccionDe(grilla);
            estado.FilaSobreBoton = -1;
            estado.FilaPresionada = -1;
            grilla.Cursor = Cursors.Default;

            DataGridViewColumn columna = ColumnaAccionDe(grilla);

            if (columna.Visible && columna.Index >= 0)
            {
                grilla.InvalidateColumn(columna.Index);
            }
        }

        /// <summary>
        /// Muestra u oculta la columna de acción según el ancho disponible: cabe si, con la
        /// barra vertical, quedan los anchos mínimos de las demás columnas y el ancho fijo del
        /// botón. Si no cabe, la acción queda en el enlace de respaldo en lugar de recortarse.
        /// </summary>
        /// <param name="grilla">Grilla de proveedores o de productos.</param>
        /// <returns><see langword="true"/> si cambió la visibilidad de la columna.</returns>
        private bool AjustarVisibilidadAccion(DataGridView grilla)
        {
            DataGridViewColumn accion = ColumnaAccionDe(grilla);
            int minimoDemasColumnas = 0;

            foreach (DataGridViewColumn columna in grilla.Columns)
            {
                if (!ReferenceEquals(columna, accion) && columna.Visible)
                {
                    minimoDemasColumnas += columna.MinimumWidth;
                }
            }

            int barraVertical = SystemInformation.GetVerticalScrollBarWidthForDpi(grilla.DeviceDpi);
            bool cabe = grilla.ClientSize.Width - barraVertical >= minimoDemasColumnas + accion.Width;

            if (accion.Visible == cabe)
            {
                return false;
            }

            accion.Visible = cabe;
            return true;
        }

        private void gridAccion_SizeChanged(object? sender, EventArgs e)
        {
            if (sender is DataGridView grilla && AjustarVisibilidadAccion(grilla))
            {
                ActualizarAccionesDeSeleccion();
            }
        }

        /// <summary>
        /// Dibuja la columna de acción: siempre el fondo y el borde de la celda y, solo en
        /// la fila de la entidad elegida, el botón «Deseleccionar» con el estado del puntero.
        /// </summary>
        private void gridAccion_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (sender is not DataGridView grilla
                || e.RowIndex < 0
                || e.ColumnIndex < 0
                || e.ColumnIndex != ColumnaAccionDe(grilla).Index)
            {
                return;
            }

            e.PaintBackground(e.ClipBounds, true);
            e.Paint(e.ClipBounds, DataGridViewPaintParts.Border);
            e.Handled = true;

            if (!EsAccionVisibleEnFila(grilla, e.RowIndex))
            {
                return;
            }

            EstadoAccionGrilla estado = EstadoAccionDe(grilla);
            EstadoAccionFila estadoBoton = estado.FilaPresionada == e.RowIndex
                ? EstadoAccionFila.Presionado
                : estado.FilaSobreBoton == e.RowIndex
                    ? EstadoAccionFila.Hover
                    : EstadoAccionFila.Reposo;

            if (e.Graphics is null)
            {
                return;
            }

            PintorAccionFila.Pintar(
                e.Graphics, e.CellBounds, estadoBoton, e.CellStyle?.Font ?? grilla.Font, grilla.DeviceDpi / 96F);
        }

        /// <summary>
        /// Indica si el puntero está sobre el botón visible de una fila (hit-test con el mismo
        /// rectángulo que usa el dibujo).
        /// </summary>
        private bool EstaSobreBotonVisible(DataGridView grilla, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0
                || e.ColumnIndex < 0
                || e.ColumnIndex != ColumnaAccionDe(grilla).Index
                || !EsAccionVisibleEnFila(grilla, e.RowIndex))
            {
                return false;
            }

            Rectangle celda = grilla.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
            Rectangle boton = PintorAccionFila.ObtenerRectanguloBoton(
                new Rectangle(Point.Empty, celda.Size), grilla.DeviceDpi / 96F);

            return boton.Contains(e.Location);
        }

        private void gridAccion_CellMouseMove(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (sender is not DataGridView grilla)
            {
                return;
            }

            EstadoAccionGrilla estado = EstadoAccionDe(grilla);
            int filaNueva = EstaSobreBotonVisible(grilla, e) ? e.RowIndex : -1;

            if (estado.FilaSobreBoton == filaNueva)
            {
                return;
            }

            InvalidarCeldaDeAccion(grilla, estado.FilaSobreBoton);
            estado.FilaSobreBoton = filaNueva;
            InvalidarCeldaDeAccion(grilla, filaNueva);
            grilla.Cursor = filaNueva >= 0 ? Cursors.Hand : Cursors.Default;
        }

        private void gridAccion_CellMouseLeave(object? sender, DataGridViewCellEventArgs e)
        {
            if (sender is not DataGridView grilla)
            {
                return;
            }

            EstadoAccionGrilla estado = EstadoAccionDe(grilla);
            InvalidarCeldaDeAccion(grilla, estado.FilaSobreBoton);
            InvalidarCeldaDeAccion(grilla, estado.FilaPresionada);
            estado.FilaSobreBoton = -1;
            estado.FilaPresionada = -1;
            grilla.Cursor = Cursors.Default;
        }

        private void gridAccion_CellMouseDown(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (sender is not DataGridView grilla
                || e.Button != MouseButtons.Left
                || !EstaSobreBotonVisible(grilla, e))
            {
                return;
            }

            EstadoAccionDe(grilla).FilaPresionada = e.RowIndex;
            InvalidarCeldaDeAccion(grilla, e.RowIndex);
        }

        private void gridAccion_CellMouseUp(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (sender is not DataGridView grilla)
            {
                return;
            }

            EstadoAccionGrilla estado = EstadoAccionDe(grilla);
            int filaPrevia = estado.FilaPresionada;
            estado.FilaPresionada = -1;
            InvalidarCeldaDeAccion(grilla, filaPrevia);
        }

        /// <summary>
        /// Deselecciona la entidad elegida al hacer clic sobre el botón de su fila. Un clic
        /// fuera del botón o en la celda de acción de una fila no elegida no deselecciona.
        /// </summary>
        private async void gridAccion_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (sender is not DataGridView grilla
                || e.Button != MouseButtons.Left
                || !EstaSobreBotonVisible(grilla, e))
            {
                return;
            }

            if (ReferenceEquals(grilla, gridProveedores))
            {
                await EjecutarAsync(QuitarProveedorAsync);
            }
            else
            {
                await EjecutarAsync(QuitarProductoAsync);
            }
        }

        private void gridAccion_CellToolTipTextNeeded(object? sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (sender is DataGridView grilla
                && e.RowIndex >= 0
                && e.ColumnIndex == ColumnaAccionDe(grilla).Index
                && EstadoAccionDe(grilla).FilaSobreBoton == e.RowIndex)
            {
                e.ToolTipText = ReferenceEquals(grilla, gridProveedores)
                    ? TextoDeseleccionarProveedor
                    : TextoDeseleccionarProducto;
            }
        }

        /// <summary>
        /// Invalida solo la celda de acción de una fila (no toda la grilla).
        /// </summary>
        private void InvalidarCeldaDeAccion(DataGridView grilla, int indiceFila)
        {
            DataGridViewColumn columna = ColumnaAccionDe(grilla);

            if (indiceFila >= 0 && indiceFila < grilla.Rows.Count && columna.Visible)
            {
                grilla.InvalidateCell(columna.Index, indiceFila);
            }
        }

        private async void lnkDeseleccionarProveedor_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            if (PuedeDeseleccionarProveedor())
            {
                await EjecutarAsync(QuitarProveedorAsync);
            }
        }

        private async void lnkDeseleccionarProducto_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            if (PuedeDeseleccionarProducto())
            {
                await EjecutarAsync(QuitarProductoAsync);
            }
        }

        // =====================================================================
        // 5. PRESENTACIÓN DE LOS LISTADOS Y DATOS SELECCIONADOS
        // =====================================================================

        /// <summary>
        /// Vuelve a enlazar el listado de productos sin disparar la selección
        /// automática de la primera fila. El enlace usa siempre una copia nueva de
        /// los resultados, de modo que vaciar la lista de campo no altera el
        /// <c>DataSource</c> ni deja el <c>CurrencyManager</c> en una fila inválida.
        /// Admite tanto la búsqueda global como los habituales del proveedor
        /// (RF-15 y RF-66).
        /// </summary>
        /// <param name="origen">Lista de resultados o <see langword="null"/> para vaciarlo.</param>
        private void MostrarProductos(object? origen)
        {
            object? copia = origen switch
            {
                IEnumerable<ProductoBuscadoDto> buscados => buscados.ToList(),
                IEnumerable<ProductoDeProveedorDto> habituales => habituales.ToList(),
                _ => null
            };

            using (SuspenderEventos())
            {
                gridProductos.CurrentCell = null;
                gridProductos.DataSource = null;
                gridProductos.DataSource = copia;
                gridProductos.ClearSelection();
                gridProductos.CurrentCell = null;
            }

            ActualizarAccionesDeSeleccion();
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

            ActualizarAccionesDeSeleccion();
        }

        private void MostrarDatosProducto()
        {
            bool hayProducto = _idProductoElegido.HasValue;
            lblNombreProducto.Text = hayProducto ? _nombreProductoElegido : TextoProductoSinElegir;
            lblCodigoProducto.Text = hayProducto
                ? $"Código: {_codigoProductoElegido}"
                : TextoCodigoSinElegir;

            ActualizarAccionesDeSeleccion();
        }

        private void MostrarDatosProveedor()
        {
            bool hayProveedor = _idProveedorElegido.HasValue;
            lblRazonSocialProveedor.Text = hayProveedor ? _razonSocialProveedor : TextoProveedorSinElegir;
            lblDocumentoProveedor.Text = hayProveedor
                ? $"CUIT: {_documentoProveedor}"
                : TextoDocumentoSinElegir;

            // RF-58: sin proveedor elegido la acción primaria queda deshabilitada.
            ActualizarEstadoAcciones();
            ActualizarAccionesDeSeleccion();
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
                    if (fila.Cells.Count > 0
                        && TryObtenerProveedorSeleccionado(fila.DataBoundItem,
                            out int idFila, out _, out _)
                        && idFila == idProveedor)
                    {
                        gridProveedores.CurrentCell = fila.Cells[0];
                        break;
                    }
                }
            }

            ActualizarAccionesDeSeleccion();
        }

        /// <summary>
        /// Restituye por identificador la fila del producto elegido en el listado
        /// vigente, sin disparar el manejador de selección. Si el producto no está
        /// en el listado no se toca la selección: sus rótulos y la captura siguen
        /// intactos (RF-66 y RNF-11).
        /// </summary>
        /// <param name="idProducto">Producto elegido o <see langword="null"/>.</param>
        private void SeleccionarProductoEnGrilla(int? idProducto)
        {
            if (idProducto is null || IsDisposed || !IsHandleCreated || gridProductos.IsDisposed)
            {
                return;
            }

            if (gridProductos.Columns.Count == 0 || gridProductos.Rows.Count == 0)
            {
                return;
            }

            using (SuspenderSeleccion())
            {
                foreach (DataGridViewRow fila in gridProductos.Rows)
                {
                    if (fila.Cells.Count == 0
                        || !TryObtenerProductoSeleccionado(fila.DataBoundItem,
                            out int idFila, out _, out _)
                        || idFila != idProducto)
                    {
                        continue;
                    }

                    gridProductos.CurrentCell = fila.Cells[0];
                    gridProductos.ClearSelection();
                    fila.Selected = true;
                    break;
                }
            }

            ActualizarAccionesDeSeleccion();
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
        // 7. ADAPTACIÓN AL DPI Y AL ANCHO DISPONIBLE
        // =====================================================================

        // Valores lógicos (96 DPI). Los píxeles que no escalan solos (alto de
        // cabeceras y filas de las grillas, anchos de columna, alto mínimo de
        // desplazamiento y relleno de celda) se calculan aquí con el factor de
        // escala actual; los tamaños del diseñador sí los escala AutoScaleMode.Dpi.
        private const float UmbralApiladoLogico = 840F;

        // A 100 % el título «1 · Proveedor» y el checkbox de 140 px ya no caben en la columna
        // del catálogo con 840 lógicos de ancho: la disposición apilada empieza antes.
        private const float UmbralApiladoCienPorCientoLogico = 950F;
        private const int AltoMinimoDosColumnasLogico = 456;
        private const int AltoMinimoApiladoLogico = 1080;
        private const int AltoCatalogoApiladoLogico = 460;
        private const int SeparacionLogica = 6;

        // El MaterialCheckbox dibuja su caja y su texto sin escalar con el DPI: centrados a
        // 15 px del borde superior (se recorta con menos de 30 px de alto) y ocupando unos
        // 140 px de ancho con «Mostrar todos». La fila del título de cada tarjeta no baja de
        // 30 px y el checkbox no baja de 140 px: a 150 % (24 y 124 lógicos = 36 y 186 px) no
        // cambia nada y a 100 % la fila pasa de 24 a 30 px y el ancho de 124 a 140 px.
        private const int AltoMinimoFilaTituloPx = 30;
        private const int AnchoMinimoCheckboxPx = 140;

        private bool? _disposicionApilada;
        private float _escalaDisposicion;
        private float _escalaGrillas;

        private static int Escalar(float valorLogico, float factor)
            => (int)Math.Round(valorLogico * factor);

        private void ComprasForm_SizeChanged(object? sender, EventArgs e)
        {
            AplicarEscalaYDisposicion();
        }

        /// <summary>
        /// Ajusta lo que depende del DPI y elige la disposición: dos columnas
        /// (catálogo a la izquierda, compra actual a la derecha) o una sola columna
        /// apilada cuando el ancho lógico no alcanza. El desplazamiento es solo
        /// vertical y vive en <c>pnlContenedor</c>.
        /// </summary>
        private void AplicarEscalaYDisposicion()
        {
            if (IsDisposed || ClientSize.Width <= 0 || ClientSize.Height <= 0)
            {
                return;
            }

            float factor = DeviceDpi / 96F;

            AjustarGrillas(factor);

            float umbralApilado = factor <= 1F ? UmbralApiladoCienPorCientoLogico : UmbralApiladoLogico;
            bool apilar = ClientSize.Width < Escalar(umbralApilado, factor);
            AplicarDisposicion(apilar, factor);

            pnlContenedor.AutoScrollMinSize = new Size(
                0,
                Escalar(apilar ? AltoMinimoApiladoLogico : AltoMinimoDosColumnasLogico, factor));
        }

        /// <summary>
        /// Garantiza que la fila del título de las dos tarjetas del catálogo, que aloja el
        /// checkbox «Mostrar todos», mida al menos <see cref="AltoMinimoFilaTituloPx"/> píxeles
        /// reales y que el checkbox mida al menos <see cref="AnchoMinimoCheckboxPx"/> de ancho. Al cargar, el escalado automático del diseñador todavía no se aplicó a las
        /// filas, así que se fija el valor lógico que, ya escalado, cumple el mínimo (24 a
        /// 150 %, 30 a 100 %); tras un cambio de DPI las filas ya están escaladas y se fijan
        /// en píxeles reales.
        /// </summary>
        /// <param name="yaEscalado">Indica si las filas ya reflejan el DPI actual.</param>
        private void AjustarFilaTitulo(bool yaEscalado)
        {
            float factor = DeviceDpi / 96F;
            float alto = yaEscalado
                ? Math.Max(Escalar(24, factor), AltoMinimoFilaTituloPx)
                : Math.Max(24, (float)Math.Ceiling(AltoMinimoFilaTituloPx / factor));
            int ancho = yaEscalado
                ? Math.Max(Escalar(124, factor), AnchoMinimoCheckboxPx)
                : Math.Max(124, (int)Math.Ceiling(AnchoMinimoCheckboxPx / factor));
            tblListadoProveedores.RowStyles[0].Height = alto;
            tblListadoProductos.RowStyles[0].Height = alto;
            chkMostrarTodosProveedores.Width = ancho;
            chkMostrarTodosProductos.Width = ancho;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            AjustarFilaTitulo(false);
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            AjustarFilaTitulo(true);
        }
        private void AplicarDisposicion(bool apilar, float factor)
        {
            if (_disposicionApilada == apilar && _escalaDisposicion == factor)
            {
                return;
            }

            _disposicionApilada = apilar;
            _escalaDisposicion = factor;
            int separacion = Escalar(SeparacionLogica, factor);

            tblPrincipal.SuspendLayout();

            if (apilar)
            {
                tblPrincipal.SetColumn(tblDocumento, 0);
                tblPrincipal.SetRow(tblDocumento, 2);
                tblPrincipal.ColumnStyles[0].SizeType = SizeType.Percent;
                tblPrincipal.ColumnStyles[0].Width = 100F;
                tblPrincipal.ColumnStyles[1].SizeType = SizeType.Absolute;
                tblPrincipal.ColumnStyles[1].Width = 0F;
                tblPrincipal.RowStyles[1].SizeType = SizeType.Absolute;
                tblPrincipal.RowStyles[1].Height = Escalar(AltoCatalogoApiladoLogico, factor);
                tblPrincipal.RowStyles[2].SizeType = SizeType.Percent;
                tblPrincipal.RowStyles[2].Height = 100F;
                tblCatalogo.Margin = new Padding(0, 0, 0, separacion);
                tblDocumento.Margin = new Padding(0, separacion, 0, 0);
            }
            else
            {
                tblPrincipal.SetColumn(tblDocumento, 1);
                tblPrincipal.SetRow(tblDocumento, 1);
                tblPrincipal.ColumnStyles[0].SizeType = SizeType.Percent;
                tblPrincipal.ColumnStyles[0].Width = 30F;
                tblPrincipal.ColumnStyles[1].SizeType = SizeType.Percent;
                tblPrincipal.ColumnStyles[1].Width = 70F;
                tblPrincipal.RowStyles[1].SizeType = SizeType.Percent;
                tblPrincipal.RowStyles[1].Height = 100F;
                tblPrincipal.RowStyles[2].SizeType = SizeType.Absolute;
                tblPrincipal.RowStyles[2].Height = 0F;
                tblCatalogo.Margin = new Padding(0, 0, separacion, 0);
                tblDocumento.Margin = new Padding(separacion, 0, 0, 0);
            }

            tblPrincipal.ResumeLayout(true);
        }

        /// <summary>
        /// Fija en píxeles reales las medidas de las grillas que
        /// <c>AutoScaleMode</c> no escala (cabeceras sin recorte de descendentes,
        /// filas, anchos mínimos y relleno de celda).
        /// </summary>
        /// <param name="factor">Factor de escala (DPI actual / 96).</param>
        private void AjustarGrillas(float factor)
        {
            if (_escalaGrillas == factor)
            {
                return;
            }

            _escalaGrillas = factor;

            AjustarGrilla(gridProveedores, 30, 26, factor);
            AjustarGrilla(gridProductos, 30, 26, factor);
            AjustarGrilla(gridLineas, 32, 32, factor);

            // Con los mínimos ajustados al ancho disponible, el relleno horizontal de
            // esta grilla baja a 4 (celdas) y 3 (cabeceras) lógicos para que los textos
            // de las cabeceras no se trunquen.
            Padding rellenoCelda = new(Escalar(4, factor), 0, Escalar(4, factor), 0);
            gridLineas.DefaultCellStyle.Padding = rellenoCelda;
            gridLineas.AlternatingRowsDefaultCellStyle.Padding = rellenoCelda;
            gridLineas.ColumnHeadersDefaultCellStyle.Padding =
                new Padding(Escalar(3, factor), 0, Escalar(3, factor), 0);

            // Anchos de la grilla de proveedores (lógicos). El CUIT formateado mide 95 px a
            // 100 % de DPI y con el relleno reducido necesita 103-104; la columna de acción
            // («Deseleccionar» mide 89 px) es fija. Razón social 80 + CUIT 104 + acción 104 +
            // barra vertical 17 = 305, que cabe en el ancho de diseño (309). Con menos ancho
            // la columna de acción se oculta (AjustarVisibilidadAccion) y queda el enlace.
            colProveedorRazonSocial.MinimumWidth = Escalar(80, factor);
            colProveedorDocumento.MinimumWidth = Escalar(104, factor);
            gridProveedores.DefaultCellStyle.Padding = rellenoCelda;
            gridProveedores.AlternatingRowsDefaultCellStyle.Padding = rellenoCelda;
            gridProveedores.ColumnHeadersDefaultCellStyle.Padding =
                new Padding(Escalar(3, factor), 0, Escalar(3, factor), 0);
            colProductoCodigo.MinimumWidth = Escalar(80, factor);
            colProductoNombre.MinimumWidth = Escalar(90, factor);

            // Columna del botón «Deseleccionar»: ancho fijo y estable, escalado con el DPI.
            int anchoAccion = Escalar(PintorAccionFila.AnchoColumnaLogico, factor);
            colProveedorAccion.MinimumWidth = anchoAccion;
            colProveedorAccion.Width = anchoAccion;
            colProductoAccion.MinimumWidth = anchoAccion;
            colProductoAccion.Width = anchoAccion;

            if (AjustarVisibilidadAccion(gridProveedores) | AjustarVisibilidadAccion(gridProductos))
            {
                ActualizarAccionesDeSeleccion();
            }

            // Suma de mínimos = 544 lógicos: con la barra vertical (~17) cabe en la
            // grilla en el umbral de apilado (≈563) y a 1366x768 a 150 % (≈579).
            colLineaProducto.MinimumWidth = Escalar(114, factor);
            colLineaCantidad.MinimumWidth = Escalar(70, factor);
            colLineaPrecioCompra.MinimumWidth = Escalar(100, factor);
            colLineaPrecioVenta.MinimumWidth = Escalar(92, factor);
            colLineaSubtotal.MinimumWidth = Escalar(76, factor);
            colLineaEditar.MinimumWidth = Escalar(46, factor);
            colLineaEditar.Width = Escalar(46, factor);
            colLineaQuitar.MinimumWidth = Escalar(46, factor);
            colLineaQuitar.Width = Escalar(46, factor);
        }

        private static void AjustarGrilla(
            DataGridView grilla, int altoCabecera, int altoFila, float factor)
        {
            Padding relleno = new(Escalar(6, factor), 0, Escalar(6, factor), 0);
            int altoFilaReal = Escalar(altoFila, factor);

            grilla.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grilla.ColumnHeadersHeight = Escalar(altoCabecera, factor);
            grilla.RowTemplate.Height = altoFilaReal;
            grilla.ColumnHeadersDefaultCellStyle.Padding = relleno;
            grilla.DefaultCellStyle.Padding = relleno;
            grilla.AlternatingRowsDefaultCellStyle.Padding = relleno;

            foreach (DataGridViewRow fila in grilla.Rows)
            {
                fila.Height = altoFilaReal;
            }
        }

        /// <summary>
        /// Alinea a la derecha las cabeceras de las columnas numéricas para que
        /// coincidan con sus importes (desktop-ux-patterns §2).
        /// </summary>
        private void AlinearCabecerasNumericas()
        {
            colLineaCantidad.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            colLineaPrecioCompra.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            colLineaPrecioVenta.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            colLineaSubtotal.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        // =====================================================================
        // 8. AYUDA COMÚN DE LOS MANEJADORES
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
        /// Indica si la respuesta de una consulta por proveedor llegó tarde: la
        /// pantalla ya se liberó o el proveedor vigente ya no es el consultado.
        /// </summary>
        /// <param name="idConsultado">Proveedor con el que se hizo la consulta.</param>
        /// <param name="idVigente">Proveedor elegido actualmente.</param>
        /// <returns><see langword="true"/> si la respuesta debe descartarse.</returns>
        private bool EsRespuestaObsoleta(int idConsultado, int? idVigente)
            => IsDisposed || idVigente != idConsultado;

        /// <summary>
        /// Indica si la selección programática de las grillas está en curso. Los
        /// dos manejadores de selección salen mientras valga <see langword="true"/>,
        /// de modo que restituir una fila no dispara el flujo inverso.
        /// </summary>
        private bool SeleccionSuspendida => _actualizandoSeleccion || _nivelActualizando > 0;

        /// <summary>
        /// Suspende los manejadores de selección mientras dure el rebindeo y la
        /// restitución de filas. La liberación restaura los valores previos, así
        /// que las supresiones anidadas no se pisan y nunca se cruza un
        /// <c>await</c>.
        /// </summary>
        /// <returns>Objeto desechable que restaura los valores al liberarse.</returns>
        private IDisposable SuspenderSeleccion() => new SuspensionSeleccion(this);

        /// <summary>
        /// Suspende los manejadores de las grillas mientras dure la mutación de
        /// datos y el rebindeo. Se apoya en un contador para que las supresiones
        /// anidadas restauren el nivel previo en lugar de forzarlo a cero.
        /// </summary>
        /// <returns>Objeto desechable que restaura el nivel al liberarse.</returns>
        private IDisposable SuspenderEventos() => new Suspension(this);

        /// <summary>
        /// Restauración de la suspensión de selección al liberar el bloque
        /// <c>using</c>. Devuelve el contador y la bandera a sus valores previos.
        /// </summary>
        private sealed class SuspensionSeleccion : IDisposable
        {
            private readonly ComprasForm _formulario;
            private readonly int _nivelPrevio;
            private readonly bool _actualizandoSeleccionPrevio;
            private bool _liberada;

            /// <summary>
            /// Incrementa el nivel de supresión y activa la bandera de selección.
            /// </summary>
            /// <param name="formulario">Formulario que suspende sus manejadores.</param>
            public SuspensionSeleccion(ComprasForm formulario)
            {
                ArgumentNullException.ThrowIfNull(formulario);

                _formulario = formulario;
                _nivelPrevio = formulario._nivelActualizando;
                _actualizandoSeleccionPrevio = formulario._actualizandoSeleccion;
                _formulario._nivelActualizando++;
                _formulario._actualizandoSeleccion = true;
            }

            /// <summary>
            /// Restaura los valores previos de la suspensión.
            /// </summary>
            public void Dispose()
            {
                if (_liberada)
                {
                    return;
                }

                _liberada = true;
                _formulario._nivelActualizando = _nivelPrevio;
                _formulario._actualizandoSeleccion = _actualizandoSeleccionPrevio;
            }
        }

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
