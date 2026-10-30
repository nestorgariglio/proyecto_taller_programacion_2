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
    /// Modal de solo lectura con la cabecera y las líneas de la compra elegida en
    /// la barra de historial (RF-53 y RF-54). Consume únicamente
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
        private const string FormatoFecha = "dd/MM/yyyy HH:mm";
        private const string FormatoImporte = "N2";

        private readonly CompraNegocio _negocio;
        private readonly string? _rol;
        private readonly int _idCompra;

        /// <summary>
        /// Inicializa el modal de detalle de compras.
        /// </summary>
        /// <param name="negocio">Servicio que resuelve el detalle de la compra.</param>
        /// <param name="idCompra">Compra elegida en el historial.</param>
        /// <param name="rol">Rol de la sesión con el que el negocio evalúa el permiso.</param>
        public DetalleCompraForm(CompraNegocio negocio, int idCompra, string? rol)
        {
            ArgumentNullException.ThrowIfNull(negocio);

            _negocio = negocio;
            _idCompra = idCompra;
            _rol = rol;

            InitializeComponent();

            MaterialSkinManager.Instance.AddFormToManage(this);
            BackColor = MaterialSkinManager.Instance.BackdropColor;
            AplicarEstiloGrilla(gridDetalle);

            MostrarCabecera(null);

            Load += DetalleCompraForm_Load;
            gridDetalle.CellFormatting += gridDetalle_CellFormatting;
        }

        private async void DetalleCompraForm_Load(object? sender, EventArgs e)
        {
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
    }
}
