using CapaNegocio;
using CapaNegocio.DTOs.Categorias;
using CapaNegocio.Enums;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CapaPresentacion;

/// <summary>
/// Formulario modal para registrar o modificar una categoría/rubro (RF-1, RF-4).
/// El texto del campo se envía en crudo a <see cref="CapaNegocio.CategoriaNegocio"/>:
/// el Recorte y las validaciones de formato (RF-2) y unicidad (RF-3, incluido el
/// mensaje específico del duplicado inactivo, D-1) los aplica la capa de negocio.
/// Ante cualquier rechazo se muestra el <c>Mensaje</c> en español sin cerrar el
/// modal y conservando lo tipeado; solo un resultado <c>Exito</c> cierra con
/// <see cref="DialogResult.OK"/> para que el listado recargue (RNF-3).
/// </summary>
public partial class CategoriaForm : MaterialForm
{
    private readonly CategoriaNegocio _categoriaNegocio;
    private readonly CategoriaRespuestaDto? _categoriaAEditar;
    private bool _guardando;

    /// <summary>
    /// Inicializa el formulario de alta o modificación de categorías.
    /// </summary>
    /// <param name="categoriaNegocio">Servicio que crea y modifica categorías.</param>
    /// <param name="categoriaAEditar">Categoría cuya descripción se precarga para modificarla.</param>
    public CategoriaForm(
        CategoriaNegocio categoriaNegocio,
        CategoriaRespuestaDto? categoriaAEditar = null)
    {
        ArgumentNullException.ThrowIfNull(categoriaNegocio);

        _categoriaNegocio = categoriaNegocio;
        _categoriaAEditar = categoriaAEditar;
        InitializeComponent();

        PrepararDatosEdicion();

        btnGuardar.Click += btnGuardar_Click;
        btnCancelar.Click += btnCancelar_Click;

        var materialSkinManager = MaterialSkinManager.Instance;
        materialSkinManager.AddFormToManage(this);
    }

    /// <summary>
    /// En modo edición precarga la descripción actual y actualiza los títulos;
    /// en modo alta el campo queda vacío (RF-1).
    /// </summary>
    private void PrepararDatosEdicion()
    {
        if (_categoriaAEditar is null)
        {
            return;
        }

        Text = "Editar categoría";
        labelTitulo.Text = "Editar categoría";
        txtDescripcion.Text = _categoriaAEditar.Descripcion ?? string.Empty;
    }

    private async void btnGuardar_Click(object? sender, EventArgs e)
    {
        if (_guardando)
        {
            return;
        }

        _guardando = true;
        btnGuardar.Enabled = false;

        try
        {
            // Texto crudo del campo: el Recorte lo aplica CapaNegocio antes de
            // validar y guardar (RF-2 / RF-4).
            string textoCrudo = txtDescripcion.Text;

            if (_categoriaAEditar is null)
            {
                var entradaCategoria = new CrearCategoriaDto
                {
                    Descripcion = textoCrudo
                };

                RespuestaCreacionCategoria respuesta =
                    await _categoriaNegocio.CrearCategoriaAsync(entradaCategoria);

                if (respuesta.Resultado != ResultadoCreacionCategoria.Exito)
                {
                    // RF-2 / RF-3 (D-1): se muestra el mensaje del negocio en
                    // español sin cerrar el modal; si el duplicado es de una
                    // categoría inactiva, su mensaje lo indica y sugiere
                    // reactivarla desde el listado (la existente no se reactiva).
                    MaterialMessageBox.Show(this, respuesta.Mensaje, "Atención");
                    txtDescripcion.Focus();
                    return;
                }

                MaterialMessageBox.Show(this, respuesta.Mensaje, "Categoría creada");
            }
            else
            {
                var entradaCategoria = new ModificarCategoriaDto
                {
                    IdCategoria = _categoriaAEditar.IdCategoria,
                    Descripcion = textoCrudo
                };

                RespuestaModificacionCategoria respuesta =
                    await _categoriaNegocio.ModificarCategoriaAsync(entradaCategoria);

                if (respuesta.Resultado != ResultadoModificacionCategoria.Exito)
                {
                    MaterialMessageBox.Show(this, respuesta.Mensaje, "Atención");
                    txtDescripcion.Focus();
                    return;
                }

                MaterialMessageBox.Show(this, respuesta.Mensaje, "Categoría modificada");
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception exception)
        {
            // Caso límite: fallo o desconexión de la base de datos -> mensaje en
            // español sin perder lo que el usuario había ingresado.
            MaterialMessageBox.Show(
                this,
                $"No se pudo guardar la categoría.\nDetalle: {exception.Message}",
                "Error SQL");
        }
        finally
        {
            _guardando = false;
            btnGuardar.Enabled = true;
        }
    }

    private void btnCancelar_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
