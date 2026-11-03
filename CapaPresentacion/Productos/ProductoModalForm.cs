using System.Globalization;
using CapaNegocio;
using CapaNegocio.DTOs.Categorias;
using CapaNegocio.DTOs.Productos;
using CapaNegocio.Enums;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CapaPresentacion.Productos;

/// <summary>
/// Formulario modal para registrar o modificar un producto (RF-1, RF-2, RF-4, RF-5, RF-7, RF-8, RF-9).
/// </summary>
/// <remarks>
/// Los textos viajan crudos a <see cref="ProductoNegocio"/>: el Recorte y las reglas de
/// negocio (unicidad, longitudes, categoría activa, rangos) se validan allí (RNF-5). La
/// interfaz solo controla obligatoriedad y formato numérico. En edición el Código es
/// inmutable y de solo lectura (D-11) y se puede editar un producto inactivo sin
/// reactivarlo (D-20). La advertencia de PrecioVenta menor a PrecioCompra se muestra
/// antes de llamar a Negocio; si se cancela, no se persiste nada (RF-8/D-19).
/// </remarks>
public partial class ProductoModalForm : MaterialForm
{
    private readonly ProductoNegocio _productoNegocio;
    private readonly CategoriaNegocio _categoriaNegocio;
    private readonly int _idUsuario;
    private readonly ProductoRespuestaDto? _productoAEditar;
    private readonly List<CategoriaRespuestaDto> _categoriasEnCombo = new();
    private bool _guardando;

    /// <summary>
    /// Inicializa el formulario de alta o modificación de productos.
    /// </summary>
    /// <param name="productoNegocio">Servicio que crea y modifica productos.</param>
    /// <param name="categoriaNegocio">Servicio que lista las categorías activas del combo.</param>
    /// <param name="idUsuario">Usuario logueado que ejecuta la operación (auditoría, RF-20).</param>
    /// <param name="productoAEditar">Producto a editar; <c>null</c> para el alta.</param>
    public ProductoModalForm(
        ProductoNegocio productoNegocio,
        CategoriaNegocio categoriaNegocio,
        int idUsuario,
        ProductoRespuestaDto? productoAEditar = null)
    {
        ArgumentNullException.ThrowIfNull(productoNegocio);
        ArgumentNullException.ThrowIfNull(categoriaNegocio);

        _productoNegocio = productoNegocio;
        _categoriaNegocio = categoriaNegocio;
        _idUsuario = idUsuario;
        _productoAEditar = productoAEditar;

        InitializeComponent();

        Text = _productoAEditar is null ? "Nuevo producto" : "Editar producto";
        labelTitulo.Text = Text;

        // El Código solo admite dígitos en el alta; en edición queda de solo lectura (D-11).
        txtCodigo.MaxLength = 13;
        txtCodigo.KeyPress += txtCodigo_KeyPress;

        btnGuardar.Click += btnGuardar_Click;
        btnCancelar.Click += btnCancelar_Click;

        var materialSkinManager = MaterialSkinManager.Instance;
        materialSkinManager.AddFormToManage(this);

        Shown += ProductoModalForm_Shown;
    }

    private static void txtCodigo_KeyPress(object? sender, KeyPressEventArgs e)
    {
        // Formato mínimo en UI: solo dígitos y teclas de control.
        if (!char.IsControl(e.KeyChar) && (e.KeyChar < '0' || e.KeyChar > '9'))
        {
            e.Handled = true;
        }
    }

    private async void ProductoModalForm_Shown(object? sender, EventArgs e)
    {
        await CargarCategoriasActivasAsync();
        PrepararDatosEdicion();
    }

    /// <summary>
    /// Carga en el combo únicamente las categorías activas (RF-6) y conserva la
    /// relación por Id, nunca por descripción (RF-17).
    /// </summary>
    private async Task CargarCategoriasActivasAsync()
    {
        try
        {
            IReadOnlyList<CategoriaRespuestaDto> categoriasActivas =
                await _categoriaNegocio.ListarCategoriasActivasAsync();

            cboCategoria.Items.Clear();
            _categoriasEnCombo.Clear();

            foreach (CategoriaRespuestaDto categoria in categoriasActivas)
            {
                if (string.IsNullOrWhiteSpace(categoria.Descripcion))
                {
                    continue;
                }

                _categoriasEnCombo.Add(categoria);
                cboCategoria.Items.Add(categoria.Descripcion);
            }

            if (cboCategoria.Items.Count > 0)
            {
                cboCategoria.SelectedIndex = 0;
            }
        }
        catch (Exception exception)
        {
            // Caso límite: fallo o desconexión de la BD ⇒ mensaje en español y el
            // modal queda abierto conservando lo ingresado (RF-16).
            MaterialMessageBox.Show(
                this,
                $"No se pudieron cargar las categorías activas.\nDetalle: {exception.Message}",
                "Error SQL");
        }
    }

    /// <summary>
    /// En modo edición precarga los datos y deja el Código de solo lectura (D-11).
    /// El producto inactivo se edita sin reactivarlo (D-20).
    /// </summary>
    private void PrepararDatosEdicion()
    {
        if (_productoAEditar is null)
        {
            txtCodigo.ReadOnly = false;
            return;
        }

        txtCodigo.Text = _productoAEditar.Codigo ?? string.Empty;
        txtCodigo.ReadOnly = true;
        txtCodigo.Hint = "Código (no editable)";

        txtNombre.Text = _productoAEditar.Nombre ?? string.Empty;
        txtDescripcion.Text = _productoAEditar.Descripcion ?? string.Empty;
        txtPrecioCompra.Text = _productoAEditar.PrecioCompra.ToString("0.00", CultureInfo.CurrentCulture);
        txtPrecioVenta.Text = _productoAEditar.PrecioVenta.ToString("0.00", CultureInfo.CurrentCulture);

        SeleccionarCategoriaActual(_productoAEditar.IdCategoria);
    }

    private void SeleccionarCategoriaActual(int idCategoria)
    {
        for (int indice = 0; indice < _categoriasEnCombo.Count; indice++)
        {
            if (_categoriasEnCombo[indice].IdCategoria == idCategoria)
            {
                cboCategoria.SelectedIndex = indice;
                return;
            }
        }

        // La categoría del producto ya no está activa (caso límite): el combo solo
        // ofrece activas, así que se avisa sin bloquear la edición (RF-6).
        lblAvisoCategoria.Text =
            "La categoría actual del producto no está activa. Seleccione una categoría activa para guardar.";
        lblAvisoCategoria.Visible = true;
    }

    private async void btnGuardar_Click(object? sender, EventArgs e)
    {
        if (_guardando)
        {
            return;
        }

        // Validaciones mínimas de la interfaz: obligatoriedad y formato numérico.
        if (!ValidarFormulario(out decimal precioCompra, out decimal precioVenta, out int idCategoria))
        {
            return;
        }

        // RF-8/D-19: la advertencia se muestra ANTES de llamar a Negocio (antes de
        // persistir). Si el usuario cancela, no se guarda nada y el modal sigue abierto.
        if (!ConfirmarPrecioVentaMenorCompra(precioCompra, precioVenta))
        {
            return;
        }

        _guardando = true;
        btnGuardar.Enabled = false;

        try
        {
            if (_productoAEditar is null)
            {
                var entrada = new CrearProductoDto
                {
                    Codigo = txtCodigo.Text,
                    Nombre = txtNombre.Text,
                    Descripcion = txtDescripcion.Text,
                    IdCategoria = idCategoria,
                    PrecioCompra = precioCompra,
                    PrecioVenta = precioVenta,
                    IdUsuario = _idUsuario
                };

                RespuestaCreacionProducto respuesta = await _productoNegocio.CrearProductoAsync(entrada);

                if (respuesta.Resultado != ResultadoCreacionProducto.Exito)
                {
                    // Mensaje del negocio en español, sin cerrar el modal (RF-2/RF-4/RF-5/RF-7).
                    MaterialMessageBox.Show(this, respuesta.Mensaje, "Atención");
                    txtCodigo.Focus();
                    return;
                }

                MaterialMessageBox.Show(this, respuesta.Mensaje, "Producto creado");
            }
            else
            {
                var entrada = new ModificarProductoDto
                {
                    IdProducto = _productoAEditar.IdProducto,
                    Nombre = txtNombre.Text,
                    Descripcion = txtDescripcion.Text,
                    IdCategoria = idCategoria,
                    PrecioCompra = precioCompra,
                    PrecioVenta = precioVenta,
                    IdUsuario = _idUsuario
                };

                RespuestaModificacionProducto respuesta = await _productoNegocio.ModificarProductoAsync(entrada);

                if (respuesta.Resultado != ResultadoModificacionProducto.Exito)
                {
                    MaterialMessageBox.Show(this, respuesta.Mensaje, "Atención");
                    txtNombre.Focus();
                    return;
                }

                MaterialMessageBox.Show(this, respuesta.Mensaje, "Producto modificado");
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception exception)
        {
            // Caso límite: fallo o desconexión de la base de datos ⇒ mensaje en
            // español sin perder lo que el usuario había ingresado (RF-16).
            MaterialMessageBox.Show(
                this,
                $"No se pudo guardar el producto.\nDetalle: {exception.Message}",
                "Error SQL");
        }
        finally
        {
            _guardando = false;
            btnGuardar.Enabled = true;
        }
    }

    private bool ValidarFormulario(out decimal precioCompra, out decimal precioVenta, out int idCategoria)
    {
        precioCompra = 0m;
        precioVenta = 0m;
        idCategoria = 0;

        if (_idUsuario <= 0)
        {
            MaterialMessageBox.Show(
                this,
                "No hay un usuario identificado para registrar la operación.",
                "Atención");
            return false;
        }

        if (_productoAEditar is null)
        {
            string codigo = txtCodigo.Text.Trim();

            if (codigo.Length == 0)
            {
                MaterialMessageBox.Show(this, "Debe ingresar el código del producto.", "Atención");
                txtCodigo.Focus();
                return false;
            }

            if (codigo.Length < 8 || codigo.Length > 13)
            {
                MaterialMessageBox.Show(
                    this,
                    "El código del producto debe tener entre 8 y 13 dígitos numéricos.",
                    "Atención");
                txtCodigo.Focus();
                return false;
            }

            foreach (char caracter in codigo)
            {
                if (caracter < '0' || caracter > '9')
                {
                    MaterialMessageBox.Show(
                        this,
                        "El código del producto debe contener solo dígitos numéricos (8 a 13 caracteres).",
                        "Atención");
                    txtCodigo.Focus();
                    return false;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            MaterialMessageBox.Show(this, "Debe ingresar el nombre del producto.", "Atención");
            txtNombre.Focus();
            return false;
        }

        if (cboCategoria.SelectedIndex < 0 || cboCategoria.SelectedIndex >= _categoriasEnCombo.Count)
        {
            MaterialMessageBox.Show(this, "Debe seleccionar una categoría activa.", "Atención");
            cboCategoria.Focus();
            return false;
        }

        idCategoria = _categoriasEnCombo[cboCategoria.SelectedIndex].IdCategoria;

        // Formato numérico de los precios: las reglas de rango las valida Negocio (RF-7).
        if (!decimal.TryParse(txtPrecioCompra.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out precioCompra))
        {
            MaterialMessageBox.Show(
                this,
                "El precio de compra debe ser un número válido (por ejemplo 1234,56).",
                "Atención");
            txtPrecioCompra.Focus();
            return false;
        }

        if (!decimal.TryParse(txtPrecioVenta.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out precioVenta))
        {
            MaterialMessageBox.Show(
                this,
                "El precio de venta debe ser un número válido (por ejemplo 1234,56).",
                "Atención");
            txtPrecioVenta.Focus();
            return false;
        }

        return true;
    }

    /// <summary>
    /// Muestra la advertencia no bloqueante de PrecioVenta menor a PrecioCompra
    /// ANTES de persistir (RF-8/D-19). Devuelve <c>true</c> si corresponde guardar.
    /// </summary>
    private bool ConfirmarPrecioVentaMenorCompra(decimal precioCompra, decimal precioVenta)
    {
        if (precioVenta >= precioCompra)
        {
            return true;
        }

        using var dialogo = new MaterialDialog(
            this,
            "Advertencia de precios",
            "El precio de venta es menor que el precio de compra. ¿Desea guardar el producto igual?",
            "SÍ, GUARDAR",
            true,
            "CANCELAR");

        return dialogo.ShowDialog(this) == DialogResult.OK;
    }

    private void btnCancelar_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
