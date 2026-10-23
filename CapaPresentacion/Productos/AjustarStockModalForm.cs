using System.Globalization;
using CapaNegocio;
using CapaNegocio.DTOs.Productos;
using CapaNegocio.Enums;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CapaPresentacion.Productos;

/// <summary>
/// Formulario modal para el ajuste manual de stock de un producto (RF-10/RF-11).
/// </summary>
/// <remarks>
/// El stock se informa como <b>valor absoluto</b> (no como diferencia) y el motivo
/// es de texto libre obligatorio. El motivo viaja crudo a <see cref="ProductoNegocio"/>:
/// el Recorte, la longitud máxima (300), el rango del stock (mayor o igual a 0) y el
/// permiso Administrador/Encargado se validan en <c>CapaNegocio</c> (RNF-5). La
/// interfaz solo controla obligatoriedad y formato entero. Ante un rechazo se muestra
/// el <c>Mensaje</c> del negocio en español sin cerrar el modal; solo un resultado
/// <c>Exito</c> cierra con <see cref="DialogResult.OK"/> para que la grilla recargue.
/// </remarks>
public partial class AjustarStockModalForm : MaterialForm
{
    private readonly ProductoNegocio _productoNegocio;
    private readonly ProductoRespuestaDto _producto;
    private readonly int _idUsuario;
    private bool _guardando;

    /// <summary>
    /// Inicializa el formulario de ajuste manual de stock.
    /// </summary>
    /// <param name="productoNegocio">Servicio que ejecuta y valida el ajuste (RF-10/RF-11).</param>
    /// <param name="producto">Producto seleccionado en la grilla, que se muestra en modo lectura.</param>
    /// <param name="idUsuario">Usuario logueado que ejecuta la operación (auditoría, RF-20).</param>
    public AjustarStockModalForm(
        ProductoNegocio productoNegocio,
        ProductoRespuestaDto producto,
        int idUsuario)
    {
        ArgumentNullException.ThrowIfNull(productoNegocio);
        ArgumentNullException.ThrowIfNull(producto);

        _productoNegocio = productoNegocio;
        _producto = producto;
        _idUsuario = idUsuario;

        InitializeComponent();

        // El motivo es de texto libre obligatorio y la longitud máxima que valida
        // Negocio es de 300 caracteres (RF-10).
        txtMotivo.MaxLength = 300;

        MostrarDatosProducto();

        btnGuardar.Click += btnGuardar_Click;
        btnCancelar.Click += btnCancelar_Click;

        var materialSkinManager = MaterialSkinManager.Instance;
        materialSkinManager.AddFormToManage(this);
    }

    /// <summary>
    /// Muestra en modo lectura el producto (Código y Nombre) y su stock actual,
    /// para que el usuario sepa qué valor absoluto está por fijar.
    /// </summary>
    private void MostrarDatosProducto()
    {
        string codigo = string.IsNullOrWhiteSpace(_producto.Codigo) ? "(sin código)" : _producto.Codigo;
        string nombre = string.IsNullOrWhiteSpace(_producto.Nombre) ? "(sin nombre)" : _producto.Nombre;

        lblProducto.Text = $"Código: {codigo}   ·   {nombre}";
        lblStockActual.Text = $"Stock actual: {_producto.Stock} unidades";
    }

    private async void btnGuardar_Click(object? sender, EventArgs e)
    {
        if (_guardando)
        {
            return;
        }

        // Validaciones mínimas de la interfaz: formato entero y obligatoriedad del
        // motivo. El rango del stock y el permiso los valida CapaNegocio (RNF-5).
        if (!ValidarFormulario(out int nuevoStock))
        {
            return;
        }

        _guardando = true;
        btnGuardar.Enabled = false;

        try
        {
            var entradaAjuste = new AjustarStockDto
            {
                IdProducto = _producto.IdProducto,
                NuevoStock = nuevoStock,
                Motivo = txtMotivo.Text,
                IdUsuario = _idUsuario
            };

            RespuestaAjusteStock respuesta = await _productoNegocio.AjustarStockAsync(entradaAjuste);

            if (respuesta.Resultado != ResultadoAjusteStock.Exito)
            {
                // Mensaje del negocio en español (motivo requerido, stock inválido,
                // sin permiso, producto inexistente) sin cerrar el modal (RF-10/RF-11).
                MaterialMessageBox.Show(this, respuesta.Mensaje, "Atención");
                txtNuevoStock.Focus();
                return;
            }

            MaterialMessageBox.Show(this, respuesta.Mensaje, "Stock ajustado");
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception exception)
        {
            // Caso límite: fallo o desconexión de la base de datos ⇒ mensaje en
            // español sin perder lo que el usuario había ingresado (RF-16).
            MaterialMessageBox.Show(
                this,
                $"No se pudo ajustar el stock.\nDetalle: {exception.Message}",
                "Error SQL");
        }
        finally
        {
            _guardando = false;
            btnGuardar.Enabled = true;
        }
    }

    private bool ValidarFormulario(out int nuevoStock)
    {
        nuevoStock = 0;

        if (!int.TryParse(
                txtNuevoStock.Text.Trim(),
                NumberStyles.Integer,
                CultureInfo.CurrentCulture,
                out nuevoStock))
        {
            MaterialMessageBox.Show(
                this,
                "El nuevo stock debe ser un número entero (por ejemplo 25).",
                "Atención");
            txtNuevoStock.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtMotivo.Text))
        {
            MaterialMessageBox.Show(this, "Debe ingresar el motivo del ajuste de stock.", "Atención");
            txtMotivo.Focus();
            return false;
        }

        return true;
    }

    private void btnCancelar_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
