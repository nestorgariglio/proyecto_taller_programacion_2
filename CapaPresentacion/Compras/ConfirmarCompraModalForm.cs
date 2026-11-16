using System.ComponentModel;
using System.Globalization;
using CapaNegocio.DTOs.Compras;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CapaPresentacion.Compras;

public partial class ConfirmarCompraModalForm : MaterialForm
{
    private readonly CompraConDetalleDto _compra;
    private readonly BindingList<ItemConfirmacionLinea> _items;

    public SolicitudConfirmarCompraDto? SolicitudConfirmacion { get; private set; }

    public ConfirmarCompraModalForm(CompraConDetalleDto compra)
    {
        ArgumentNullException.ThrowIfNull(compra);
        _compra = compra;

        InitializeComponent();

        var materialSkinManager = MaterialSkinManager.Instance;
        materialSkinManager.AddFormToManage(this);

        lblInfoProveedor.Text = $"Solicitud N° {_compra.IdCompra:D6} — Proveedor: {_compra.RazonSocialProveedor}";

        _items = new BindingList<ItemConfirmacionLinea>(
            _compra.Lineas.Select(l => new ItemConfirmacionLinea
            {
                IdProducto = l.IdProducto,
                CodigoProducto = l.CodigoProducto,
                NombreProducto = l.NombreProducto,
                Cantidad = l.Cantidad,
                PrecioCompra = l.PrecioCompra > 0 ? l.PrecioCompra : 0m
            }).ToList());

        ConfigurarGrilla();

        btnConfirmar.Click += btnConfirmar_Click;
        btnCancelar.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        ActualizarTotales();
    }

    private void ConfigurarGrilla()
    {
        gridLineas.AutoGenerateColumns = false;
        gridLineas.BackgroundColor = Color.FromArgb(48, 48, 48);
        gridLineas.BorderStyle = BorderStyle.None;
        gridLineas.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        gridLineas.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        gridLineas.EnableHeadersVisualStyles = false;

        gridLineas.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            BackColor = Color.FromArgb(55, 71, 79),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.White,
            SelectionBackColor = Color.FromArgb(55, 71, 79),
            SelectionForeColor = Color.White,
            WrapMode = DataGridViewTriState.False
        };

        gridLineas.DefaultCellStyle = new DataGridViewCellStyle
        {
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            BackColor = Color.FromArgb(48, 48, 48),
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.White,
            SelectionBackColor = Color.FromArgb(255, 87, 34),
            SelectionForeColor = Color.White,
            WrapMode = DataGridViewTriState.False
        };

        gridLineas.GridColor = Color.FromArgb(80, 80, 80);

        gridLineas.Columns.Clear();

        gridLineas.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ItemConfirmacionLinea.CodigoProducto),
            HeaderText = "Código",
            FillWeight = 18F,
            ReadOnly = true
        });

        gridLineas.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ItemConfirmacionLinea.NombreProducto),
            HeaderText = "Producto",
            FillWeight = 38F,
            ReadOnly = true
        });

        var colCantidad = new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ItemConfirmacionLinea.Cantidad),
            HeaderText = "Cantidad",
            FillWeight = 14F,
            ReadOnly = true
        };
        colCantidad.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        gridLineas.Columns.Add(colCantidad);

        var colPrecio = new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ItemConfirmacionLinea.PrecioCompra),
            HeaderText = "Costo unitario ($)",
            FillWeight = 16F,
            ReadOnly = false
        };
        colPrecio.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        colPrecio.DefaultCellStyle.Format = "N2";
        gridLineas.Columns.Add(colPrecio);

        var colSubtotal = new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ItemConfirmacionLinea.Subtotal),
            HeaderText = "Subtotal",
            FillWeight = 14F,
            ReadOnly = true
        };
        colSubtotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        colSubtotal.DefaultCellStyle.Format = "C2";
        gridLineas.Columns.Add(colSubtotal);

        gridLineas.DataSource = _items;

        gridLineas.CellParsing += gridLineas_CellParsing;
        gridLineas.CellEndEdit += (_, _) => ActualizarTotales();
        gridLineas.DataError += (_, e) =>
        {
            lblMensajeError.Text = "Formato de precio inválido. Ingrese un valor numérico.";
            lblMensajeError.Visible = true;
            e.ThrowException = false;
        };
    }

    private void gridLineas_CellParsing(object? sender, DataGridViewCellParsingEventArgs e)
    {
        if (e.ColumnIndex == 3 && e.Value is string texto)
        {
            string normalizado = texto.Trim().Replace("$", string.Empty).Trim();
            if (decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal valor)
                || decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out valor))
            {
                e.Value = valor >= 0 ? valor : 0m;
                e.ParsingApplied = true;
            }
        }
    }

    private void ActualizarTotales()
    {
        _items.ResetBindings();
        decimal total = _items.Sum(i => i.Subtotal);
        lblTotalAcordado.Text = $"Total acordado: {total.ToString("C2", CultureInfo.CurrentCulture)}";
        lblMensajeError.Visible = false;
    }

    private void btnConfirmar_Click(object? sender, EventArgs e)
    {
        gridLineas.EndEdit();

        foreach (var item in _items)
        {
            if (item.PrecioCompra <= 0)
            {
                lblMensajeError.Text = $"Debe ingresar un costo unitario mayor a cero para '{item.NombreProducto}'.";
                lblMensajeError.Visible = true;
                return;
            }
        }

        SolicitudConfirmacion = new SolicitudConfirmarCompraDto
        {
            IdCompra = _compra.IdCompra,
            Observaciones = txtObservaciones.Text.Trim(),
            Lineas = _items.Select(i => new LineaConfirmacionCompraDto
            {
                IdProducto = i.IdProducto,
                PrecioCompra = i.PrecioCompra,
                PrecioVenta = 0m
            }).ToList()
        };

        DialogResult = DialogResult.OK;
        Close();
    }

    public sealed class ItemConfirmacionLinea
    {
        public int IdProducto { get; set; }
        public string CodigoProducto { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal Subtotal => Cantidad * PrecioCompra;
    }
}
