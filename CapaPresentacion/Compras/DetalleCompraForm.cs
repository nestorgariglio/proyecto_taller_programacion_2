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
    /// Modal para la consulta del historial de compras, visualización de detalle y
    /// gestión del ciclo de vida de operaciones comerciales (confirmación, recepción y anulación).
    /// </summary>
    public partial class DetalleCompraForm : MaterialForm
    {
        private const string ColumnaDetalleProducto = "colDetalleProducto";
        private const string ColumnaDetallePrecioCompra = "colDetallePrecioCompra";
        private const string ColumnaDetalleSubtotal = "colDetalleSubtotal";
        private const string ColumnaListaFecha = "colListaFecha";
        private const string ColumnaListaTotal = "colListaTotal";
        private const string ColumnaListaEstado = "colListaEstado";
        private const string ColumnaListaEstadoSolicitud = "colListaEstadoSolicitud";
        private const string FormatoFecha = "dd/MM/yyyy HH:mm";
        private const string FormatoImporte = "N2";
        private const float AltoListaHistorial = 170F;

        private readonly CompraNegocio _negocio;
        private readonly int _idUsuario;
        private readonly string? _rol;

        private int _idCompra;
        private bool _cargandoLista;
        private CompraConDetalleDto? _compraActual;

        private TableLayoutPanel _tblAcciones = null!;
        private MaterialButton _btnConfirmar = null!;
        private MaterialButton _btnRecibir = null!;
        private MaterialButton _btnVerCorreo = null!;
        private MaterialTextBox2 _txtMotivoAnulacion = null!;
        private MaterialButton _btnAnular = null!;

        /// <summary>
        /// Inicializa el formulario en modo lista con la sesión del usuario.
        /// </summary>
        public DetalleCompraForm(CompraNegocio negocio, string? rol, int idUsuario = 0)
        {
            ArgumentNullException.ThrowIfNull(negocio);

            _negocio = negocio;
            _idUsuario = idUsuario;
            _rol = rol;
            _idCompra = 0;

            InitializeComponent();
            InicializarAccionesOperacion();

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
        /// Inicializa el formulario en modo detalle para una compra específica.
        /// </summary>
        public DetalleCompraForm(CompraNegocio negocio, int idCompra, string? rol, int idUsuario = 0)
            : this(negocio, rol, idUsuario)
        {
            _idCompra = idCompra;
        }

        private void InicializarAccionesOperacion()
        {
            _tblAcciones = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                RowCount = 1,
                ColumnCount = 5
            };

            _tblAcciones.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _tblAcciones.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _tblAcciones.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _tblAcciones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _tblAcciones.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            _btnConfirmar = new MaterialButton
            {
                Text = "CONFIRMAR SOLICITUD",
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = false,
                HighEmphasis = true,
                Visible = false,
                Margin = new Padding(0, 4, 6, 4)
            };
            _btnConfirmar.Click += btnConfirmar_Click;

            _btnRecibir = new MaterialButton
            {
                Text = "RECIBIR MERCADERÍA",
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = false,
                HighEmphasis = true,
                Visible = false,
                Margin = new Padding(0, 4, 6, 4)
            };
            _btnRecibir.Click += btnRecibir_Click;

            _btnVerCorreo = new MaterialButton
            {
                Text = "VER SOLICITUD",
                Type = MaterialButton.MaterialButtonType.Outlined,
                NoAccentTextColor = Color.FromArgb(224, 224, 224),
                Visible = false,
                Margin = new Padding(0, 4, 10, 4)
            };
            _btnVerCorreo.Click += btnVerCorreo_Click;

            _txtMotivoAnulacion = new MaterialTextBox2
            {
                Hint = "Motivo de anulación (mín. 3 caracteres)",
                Dock = DockStyle.Fill,
                Visible = PoliticaAcceso.PuedeAnularOperaciones(_rol),
                Margin = new Padding(4, 2, 6, 2)
            };

            _btnAnular = new MaterialButton
            {
                Text = "ANULAR",
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true,
                Visible = PoliticaAcceso.PuedeAnularOperaciones(_rol),
                Enabled = false,
                Margin = new Padding(0, 4, 0, 4)
            };
            _btnAnular.Click += btnAnular_Click;

            _tblAcciones.Controls.Add(_btnConfirmar, 0, 0);
            _tblAcciones.Controls.Add(_btnRecibir, 1, 0);
            _tblAcciones.Controls.Add(_btnVerCorreo, 2, 0);
            _tblAcciones.Controls.Add(_txtMotivoAnulacion, 3, 0);
            _tblAcciones.Controls.Add(_btnAnular, 4, 0);

            tblPrincipal.SuspendLayout();
            tblPrincipal.Controls.Remove(lblMensajeDetalle);
            tblPrincipal.RowCount = 6;
            tblPrincipal.RowStyles.Clear();
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 170F));
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            tblPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));

            tblPrincipal.Controls.Add(_tblAcciones, 0, 4);
            tblPrincipal.Controls.Add(lblMensajeDetalle, 0, 5);
            tblPrincipal.ResumeLayout(false);

            ClientSize = new Size(960, 640);
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

        private void AplicarModoDetalle()
        {
            Text = "Detalle de compra";
            lblTituloDetalle.Text = "Detalle de la compra";
            gridListaCompras.Visible = false;
            tblPrincipal.RowStyles[1].Height = 0F;
        }

        private void AplicarModoLista()
        {
            Text = "Historial de compras";
            lblTituloDetalle.Text = "Historial de compras";
            gridListaCompras.Visible = true;
            tblPrincipal.RowStyles[1].Height = AltoListaHistorial;
        }

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
                ActualizarEstadoBotonesAccion(null);
                lblMensajeDetalle.Text = respuesta.Mensaje;
            }
            catch (Exception excepcion)
            {
                lblMensajeDetalle.Text = excepcion.Message;
                LimpiarDetalle();
                ActualizarEstadoBotonesAccion(null);
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
                    _compraActual = null;
                    LimpiarDetalle();
                    ActualizarEstadoBotonesAccion(null);
                    return;
                }

                _compraActual = respuesta.Compra;
                MostrarCabecera(_compraActual);
                gridDetalle.DataSource = _compraActual.Lineas.ToList();
                ActualizarEstadoBotonesAccion(_compraActual);
            }
            catch (Exception excepcion)
            {
                lblMensajeDetalle.Text = excepcion.Message;
                _compraActual = null;
                LimpiarDetalle();
                ActualizarEstadoBotonesAccion(null);
            }
        }

        private void LimpiarDetalle()
        {
            _compraActual = null;
            gridDetalle.DataSource = null;
            MostrarCabecera(null);
            ActualizarEstadoBotonesAccion(null);
        }

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
                lblMensajeDetalle.ForeColor = Color.White;
                return;
            }

            lblNumeroCompra.Text = $"N° compra: {compra.IdCompra}";
            lblFechaDetalle.Text =
                $"Fecha: {compra.FechaRegistro.ToString(FormatoFecha, CultureInfo.CurrentCulture)}";
            string estadoTexto = compra.Estado ? "Activa" : "Anulada";
            lblEstadoDetalle.Text = $"Estado: {estadoTexto} ({compra.EstadoSolicitud})";
            lblTotalDetalle.Text = compra.MontoTotal > 0
                ? $"Total: {compra.MontoTotal.ToString(FormatoImporte, CultureInfo.CurrentCulture)}"
                : "Total: — (A convenir por correo)";
            lblProveedorDetalle.Text = $"Proveedor: {compra.RazonSocialProveedor}";
            lblUsuarioDetalle.Text = $"Usuario: {compra.NombreUsuario}";

            if (!compra.Estado || string.Equals(compra.EstadoSolicitud, "Anulada", StringComparison.OrdinalIgnoreCase))
            {
                lblMensajeDetalle.Text = $"❌ Anulada el {compra.FechaAnulacion?.ToString(FormatoFecha, CultureInfo.CurrentCulture) ?? "—"}. Motivo: {compra.MotivoAnulacion}";
                lblMensajeDetalle.ForeColor = Color.OrangeRed;
            }
            else
            {
                lblMensajeDetalle.ForeColor = Color.White;
            }
        }

        private void ActualizarEstadoBotonesAccion(CompraConDetalleDto? compra)
        {
            if (compra is null)
            {
                _btnConfirmar.Visible = false;
                _btnRecibir.Visible = false;
                _btnVerCorreo.Visible = false;
                _btnAnular.Enabled = false;
                _txtMotivoAnulacion.Text = string.Empty;
                return;
            }

            bool puedeOperar = PoliticaAcceso.PuedeOperarCompras(_rol);
            bool puedeAnular = PoliticaAcceso.PuedeAnularOperaciones(_rol);
            bool esActiva = compra.Estado && !string.Equals(compra.EstadoSolicitud, "Anulada", StringComparison.OrdinalIgnoreCase);

            string estadoSolicitud = compra.EstadoSolicitud;

            _btnConfirmar.Visible = esActiva && puedeOperar && string.Equals(estadoSolicitud, "Pendiente", StringComparison.OrdinalIgnoreCase);
            _btnConfirmar.Enabled = _btnConfirmar.Visible;

            _btnRecibir.Visible = esActiva && puedeOperar && (string.Equals(estadoSolicitud, "Pendiente", StringComparison.OrdinalIgnoreCase) || string.Equals(estadoSolicitud, "Confirmada", StringComparison.OrdinalIgnoreCase));
            _btnRecibir.Enabled = _btnRecibir.Visible;

            _btnVerCorreo.Visible = string.Equals(estadoSolicitud, "Pendiente", StringComparison.OrdinalIgnoreCase) || string.Equals(estadoSolicitud, "Confirmada", StringComparison.OrdinalIgnoreCase);
            _btnVerCorreo.Enabled = _btnVerCorreo.Visible;

            _btnAnular.Visible = puedeAnular;
            _btnAnular.Enabled = esActiva && puedeAnular;
            _txtMotivoAnulacion.Visible = puedeAnular;
            _txtMotivoAnulacion.Enabled = _btnAnular.Enabled;
        }

        private async void btnConfirmar_Click(object? sender, EventArgs e)
        {
            if (_compraActual is null || !string.Equals(_compraActual.EstadoSolicitud, "Pendiente", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            using var modal = new ConfirmarCompraModalForm(_compraActual);
            if (modal.ShowDialog(this) != DialogResult.OK || modal.SolicitudConfirmacion is null)
            {
                return;
            }

            _btnConfirmar.Enabled = false;

            try
            {
                RespuestaAccionCompra respuesta =
                    await _negocio.ConfirmarSolicitudCompraAsync(modal.SolicitudConfirmacion, _idUsuario, _rol);

                if (respuesta.Exitoso)
                {
                    MessageBox.Show(respuesta.Mensaje, "Solicitud Confirmada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await CargarHistorialAsync();
                    await CargarDetalleAsync();
                }
                else
                {
                    MessageBox.Show(respuesta.Mensaje, "Error al Confirmar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                if (_compraActual is not null)
                {
                    ActualizarEstadoBotonesAccion(_compraActual);
                }
            }
        }

        private async void btnRecibir_Click(object? sender, EventArgs e)
        {
            if (_compraActual is null)
            {
                return;
            }

            DialogResult resultado = MessageBox.Show(
                $"¿Confirma la recepción de la mercadería para la compra N° {_compraActual.IdCompra:D6}?\nEsta acción incrementará el stock de los productos e impactará el catálogo.",
                "Recepción de Mercadería",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (resultado != DialogResult.Yes)
            {
                return;
            }

            _btnRecibir.Enabled = false;

            try
            {
                RespuestaAccionCompra respuesta =
                    await _negocio.RecibirCompraAsync(_compraActual.IdCompra, _idUsuario, _rol);

                if (respuesta.Exitoso)
                {
                    MessageBox.Show(respuesta.Mensaje, "Mercadería Recibida", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await CargarHistorialAsync();
                    await CargarDetalleAsync();
                }
                else
                {
                    MessageBox.Show(respuesta.Mensaje, "Error al Recibir", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                if (_compraActual is not null)
                {
                    ActualizarEstadoBotonesAccion(_compraActual);
                }
            }
        }

        private async void btnAnular_Click(object? sender, EventArgs e)
        {
            if (_compraActual is null || !_compraActual.Estado)
            {
                return;
            }

            string motivo = _txtMotivoAnulacion.Text.Trim();
            if (motivo.Length < 3)
            {
                MessageBox.Show(
                    "Ingrese un motivo de anulación de al menos 3 caracteres.",
                    "Motivo Requerido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                _txtMotivoAnulacion.Focus();
                return;
            }

            string avisoStock = string.Equals(_compraActual.EstadoSolicitud, "Recibida", StringComparison.OrdinalIgnoreCase)
                ? "\n⚠️ ATENCIÓN: La compra está en estado 'Recibida'. Se revertirá el stock de los productos si cuentan con existencias suficientes."
                : "\n(La solicitud no impactó stock, se cancelará sin alterar inventario).";

            DialogResult resultado = MessageBox.Show(
                $"¿Está seguro de anular la compra N° {_compraActual.IdCompra:D6} por {_compraActual.MontoTotal:C2}?{avisoStock}\n\nMotivo: {motivo}",
                "Confirmar Anulación de Compra",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (resultado != DialogResult.Yes)
            {
                return;
            }

            _btnAnular.Enabled = false;

            try
            {
                var solicitud = new SolicitudAnularCompraDto
                {
                    IdCompra = _compraActual.IdCompra,
                    Motivo = motivo
                };

                RespuestaAccionCompra respuesta =
                    await _negocio.AnularCompraAsync(solicitud, _idUsuario, _rol);

                if (respuesta.Exitoso)
                {
                    _txtMotivoAnulacion.Text = string.Empty;
                    MessageBox.Show(respuesta.Mensaje, "Compra Anulada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await CargarHistorialAsync();
                    await CargarDetalleAsync();
                }
                else
                {
                    MessageBox.Show(respuesta.Mensaje, "No se pudo anular la compra", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                if (_compraActual is not null)
                {
                    ActualizarEstadoBotonesAccion(_compraActual);
                }
            }
        }

        private void btnVerCorreo_Click(object? sender, EventArgs e)
        {
            if (_compraActual is null)
            {
                return;
            }

            string lineasTexto = string.Join(Environment.NewLine, _compraActual.Lineas.Select(l =>
                $" • [{l.CodigoProducto}] {l.NombreProducto} | Cantidad solicitada: {l.Cantidad}" +
                (l.PrecioCompra > 0 ? $" | Costo ref.: {l.PrecioCompra:C2}" : string.Empty)));

            string cuerpo = $"SOLICITUD DE REABASTECIMIENTO N° {_compraActual.IdCompra:D6}{Environment.NewLine}" +
                            $"Fecha de emisión: {_compraActual.FechaRegistro:dd/MM/yyyy HH:mm}{Environment.NewLine}" +
                            $"Proveedor: {_compraActual.RazonSocialProveedor}{Environment.NewLine}" +
                            $"Estado actual: {_compraActual.EstadoSolicitud}{Environment.NewLine}{Environment.NewLine}" +
                            $"PRODUCTOS SOLICITADOS:{Environment.NewLine}{lineasTexto}{Environment.NewLine}{Environment.NewLine}" +
                            $"Observaciones: {_compraActual.Observaciones ?? "Ninguna"}";

            MessageBox.Show(cuerpo, "Detalle de Solicitud a Proveedor", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

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
                e.Value = importe > 0
                    ? importe.ToString(FormatoImporte, CultureInfo.CurrentCulture)
                    : "—";
                e.FormattingApplied = true;
            }
        }

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
                e.Value = total > 0
                    ? total.ToString(FormatoImporte, CultureInfo.CurrentCulture)
                    : "—";
                e.FormattingApplied = true;
            }
            else if (columna == ColumnaListaEstado && e.Value is bool estado)
            {
                e.Value = estado ? "Activa" : "Inactiva";
                e.FormattingApplied = true;
            }
            else if (columna == ColumnaListaEstadoSolicitud && e.Value is string estadoSolicitud)
            {
                e.Value = estadoSolicitud switch
                {
                    "Pendiente" => "⏳ Pendiente",
                    "Confirmada" => "📋 Confirmada",
                    "Recibida" => "✅ Recibida",
                    "Anulada" => "❌ Anulada",
                    _ => estadoSolicitud
                };
                e.FormattingApplied = true;
            }
        }
    }
}
