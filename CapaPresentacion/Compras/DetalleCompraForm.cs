using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CapaNegocio;
using CapaNegocio.DTOs.Compras;
using CapaNegocio.Enums;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CapaPresentacion.Compras
{
    /// <summary>
    /// Modal de solo lectura con el historial de compras y su detalle (RF-49 …
    /// RF-54). Se abre en dos modos: <b>detalle</b>, con una compra elegida, que
    /// muestra su cabecera y sus líneas; y <b>lista</b>, sin compra, que lista las
    /// compras y carga el detalle de la fila seleccionada. Consume únicamente
    /// <see cref="CompraNegocio"/>: no accede al origen de datos, no calcula
    /// importes ni redacta mensajes, y muestra el <c>Mensaje</c> del negocio tal
    /// cual (RF-55 y RNF-13).
    /// </summary>
    /// <remarks>
    /// El detalle no muestra el precio de venta porque no se persiste en
    /// <c>DETALLE_COMPRA</c> (D-2 y D-17). Al cerrarse no altera la compra en
    /// carga de <see cref="ComprasForm"/>.
    /// </remarks>
    public partial class DetalleCompraForm : MaterialForm
    {
        private const string ColumnaDetalleProducto = "colDetalleProducto";
        private const string ColumnaDetallePrecioCompra = "colDetallePrecioCompra";
        private const string ColumnaDetalleSubtotal = "colDetalleSubtotal";
        private const string ColumnaListaFecha = "colListaFecha";
        private const string ColumnaListaTotal = "colListaTotal";
        private const string ColumnaListaEstado = "colListaEstado";
        private const string FormatoFecha = "dd/MM/yyyy HH:mm";
        private const string FormatoImporte = "N2";
        private const float AltoListaHistorial = 170F;

        private readonly CompraNegocio _negocio;
        private readonly string? _rol;

        /// <summary>
        /// Compra mostrada en el detalle. Cero significa modo <b>lista</b>: el
        /// modal no abre con una compra fija y espera la elección del historial.
        /// </summary>
        private int _idCompra;

        /// <summary>
        /// Suprime la selección mientras se enlaza el historial, para no cargar
        /// un detalle que el usuario no pidió.
        /// </summary>
        private bool _cargandoLista;

        /// <summary>
        /// Inicializa el modal en modo <b>lista</b>: lista las compras del
        /// historial y, al elegir una fila, muestra su detalle.
        /// </summary>
        /// <param name="negocio">Servicio que resuelve el historial y el detalle.</param>
        /// <param name="rol">Rol de la sesión con el que el negocio evalúa el permiso.</param>
        public DetalleCompraForm(CompraNegocio negocio, string? rol)
        {
            ArgumentNullException.ThrowIfNull(negocio);

            _negocio = negocio;
            _rol = rol;
            _idCompra = 0;

            InitializeComponent();

            MaterialSkinManager.Instance.AddFormToManage(this);
            BackColor = MaterialSkinManager.Instance.BackdropColor;
            AplicarEstiloGrilla(gridDetalle);
            AplicarEstiloGrilla(gridListaCompras);

            MostrarCabecera(null);

            Load += DetalleCompraForm_Load;
            gridDetalle.CellFormatting += gridDetalle_CellFormatting;
            gridListaCompras.CellFormatting += gridListaCompras_CellFormatting;
            gridListaCompras.SelectionChanged += gridListaCompras_SelectionChanged;
        }

        /// <summary>
        /// Inicializa el modal en modo <b>detalle</b> con la compra indicada.
        /// </summary>
        /// <param name="negocio">Servicio que resuelve el detalle de la compra.</param>
        /// <param name="idCompra">Compra elegida en el historial.</param>
        /// <param name="rol">Rol de la sesión con el que el negocio evalúa el permiso.</param>
        public DetalleCompraForm(CompraNegocio negocio, int idCompra, string? rol)
            : this(negocio, rol)
        {
            _idCompra = idCompra;
        }

        private async void DetalleCompraForm_Load(object? sender, EventArgs e)
        {
            if (_idCompra > 0)
            {
                AplicarModoDetalle();
                await CargarDetalleAsync();
                return;
            }

            AplicarModoLista();
            await CargarHistorialAsync();
        }

        /// <summary>
        /// Deja el modal mostrando solo la compra elegida: oculta el historial y
        /// cede su alto al detalle.
        /// </summary>
        private void AplicarModoDetalle()
        {
            Text = "Detalle de compra";
            lblTituloDetalle.Text = "Detalle de la compra";
            gridListaCompras.Visible = false;
            tblPrincipal.RowStyles[1].Height = 0F;
        }

        /// <summary>
        /// Deja el modal mostrando el historial sobre el detalle de la compra
        /// seleccionada.
        /// </summary>
        private void AplicarModoLista()
        {
            Text = "Historial de compras";
            lblTituloDetalle.Text = "Historial de compras";
            gridListaCompras.Visible = true;
            tblPrincipal.RowStyles[1].Height = AltoListaHistorial;
        }

        /// <summary>
        /// Consulta el historial de compras con el rol de la sesión y lo enlaza,
        /// sin cargar ningún detalle hasta que el usuario elija una fila. El
        /// mensaje que se muestra es siempre el que devuelve el negocio
        /// (RF-49 … RF-52).
        /// </summary>
        /// <returns>Tarea que representa la consulta del historial.</returns>
        private async Task CargarHistorialAsync()
        {
            _cargandoLista = true;

            try
            {
                RespuestaHistorialCompras respuesta =
                    await _negocio.ObtenerHistorialAsync(_rol);

                if (IsDisposed)
                {
                    return;
                }

                gridListaCompras.CurrentCell = null;
                gridListaCompras.DataSource = null;
                gridListaCompras.DataSource = respuesta.Compras.ToList();
                gridListaCompras.ClearSelection();
                gridListaCompras.CurrentCell = null;

                LimpiarDetalle();
                lblMensajeDetalle.Text = respuesta.Mensaje;
            }
            catch (Exception excepcion)
            {
                lblMensajeDetalle.Text = excepcion.Message;
                LimpiarDetalle();
            }
            finally
            {
                _cargandoLista = false;
            }
        }

        private async void gridListaCompras_SelectionChanged(object? sender, EventArgs e)
        {
            if (_cargandoLista || gridListaCompras.DataSource is null)
            {
                return;
            }

            DataGridViewRow? fila = gridListaCompras.CurrentRow;

            if (fila is null || fila.Index < 0)
            {
                return;
            }

            if (fila.DataBoundItem is not HistorialCompraDto compra
                || compra.IdCompra == _idCompra)
            {
                return;
            }

            _idCompra = compra.IdCompra;
            await CargarDetalleAsync();
        }

        /// <summary>
        /// Consulta la compra indicada y muestra su cabecera y sus líneas. El
        /// mensaje del negocio se presenta tal cual ante compra inexistente,
        /// permiso denegado o fallo de lectura (RF-53 y RF-57).
        /// </summary>
        /// <returns>Tarea que representa la consulta del detalle.</returns>
        private async Task CargarDetalleAsync()
        {
            try
            {
                RespuestaCompraConDetalle respuesta =
                    await _negocio.ObtenerCompraConDetalleAsync(_idCompra, _rol);

                lblMensajeDetalle.Text = respuesta.Mensaje;

                if (respuesta.Resultado != ResultadoCompraConDetalle.Encontrada
                    || respuesta.Compra is null)
                {
                    LimpiarDetalle();
                    return;
                }

                MostrarCabecera(respuesta.Compra);
                gridDetalle.DataSource = respuesta.Compra.Lineas.ToList();
            }
            catch (Exception excepcion)
            {
                lblMensajeDetalle.Text = excepcion.Message;
                LimpiarDetalle();
            }
        }

        /// <summary>
        /// Deja la cabecera y las líneas del detalle sin datos.
        /// </summary>
        private void LimpiarDetalle()
        {
            gridDetalle.DataSource = null;
            MostrarCabecera(null);
        }

        /// <summary>
        /// Muestra la cabecera de la compra elegida; con <see langword="null"/>
        /// deja los rótulos vacíos con «—».
        /// </summary>
        /// <param name="compra">Compra recuperada, o <see langword="null"/>.</param>
        private void MostrarCabecera(CompraConDetalleDto? compra)
        {
            if (compra is null)
            {
                lblNumeroCompra.Text = "N° compra: —";
                lblFechaDetalle.Text = "Fecha: —";
                lblEstadoDetalle.Text = "Estado: —";
                lblTotalDetalle.Text = "Total: —";
                lblProveedorDetalle.Text = "Proveedor: —";
                lblUsuarioDetalle.Text = "Usuario: —";
                return;
            }

            lblNumeroCompra.Text = $"N° compra: {compra.IdCompra}";
            lblFechaDetalle.Text =
                $"Fecha: {compra.FechaRegistro.ToString(FormatoFecha, CultureInfo.CurrentCulture)}";
            lblEstadoDetalle.Text = compra.Estado ? "Estado: Activa" : "Estado: Inactiva";
            lblTotalDetalle.Text =
                $"Total: {compra.MontoTotal.ToString(FormatoImporte, CultureInfo.CurrentCulture)}";
            lblProveedorDetalle.Text = $"Proveedor: {compra.RazonSocialProveedor}";
            lblUsuarioDetalle.Text = $"Usuario: {compra.NombreUsuario}";
        }

        // =====================================================================
        // ESTILO Y FORMATO DE PRESENTACIÓN (RNF-7 y RNF-12)
        // =====================================================================

        /// <summary>
        /// Aplica el estilo oscuro compartido por las pantallas de la aplicación a
        /// una grilla ya declarada en el diseñador.
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

        private void gridDetalle_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columna = gridDetalle.Columns[e.ColumnIndex].Name;

            if (columna == ColumnaDetalleProducto
                && e.Value is string nombreProducto
                && gridDetalle.Rows[e.RowIndex].DataBoundItem is LineaCompraDetalleDto linea)
            {
                e.Value = $"{linea.CodigoProducto} — {nombreProducto}";
                e.FormattingApplied = true;
                return;
            }

            bool esImporte =
                columna == ColumnaDetallePrecioCompra
                || columna == ColumnaDetalleSubtotal;

            if (esImporte && e.Value is decimal importe)
            {
                e.Value = importe.ToString(FormatoImporte, CultureInfo.CurrentCulture);
                e.FormattingApplied = true;
            }
        }

        /// <summary>
        /// Aplica el formato de presentación del historial: fecha y hora,
        /// importes con <c>N2</c> y estado como «Activa»/«Inactiva» (RNF-12). La
        /// grilla solo presenta los datos del negocio.
        /// </summary>
        private void gridListaCompras_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0
                || e.RowIndex >= gridListaCompras.Rows.Count
                || e.ColumnIndex >= gridListaCompras.Columns.Count)
            {
                return;
            }

            string columna = gridListaCompras.Columns[e.ColumnIndex].Name;

            if (columna == ColumnaListaFecha && e.Value is DateTime fecha)
            {
                e.Value = fecha.ToString(FormatoFecha, CultureInfo.CurrentCulture);
                e.FormattingApplied = true;
            }
            else if (columna == ColumnaListaTotal && e.Value is decimal total)
            {
                e.Value = total.ToString(FormatoImporte, CultureInfo.CurrentCulture);
                e.FormattingApplied = true;
            }
            else if (columna == ColumnaListaEstado && e.Value is bool estado)
            {
                e.Value = estado ? "Activa" : "Inactiva";
                e.FormattingApplied = true;
            }
        }
    }
}
