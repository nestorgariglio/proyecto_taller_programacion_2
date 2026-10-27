using CapaNegocio;
using CapaNegocio.DTOs.Categorias;
using CapaNegocio.Enums;
using CapaPresentacion.Tema;
using MaterialSkin.Controls;

namespace CapaPresentacion;

/// <summary>
/// Control de presentación para gestionar las categorías/rubros del sistema.
/// </summary>
public partial class GestionCategoriasControl : UserControl
{
    private readonly CategoriaNegocio _categoriaNegocio;
    private IReadOnlyList<CategoriaRespuestaDto> _categorias = Array.Empty<CategoriaRespuestaDto>();
    private bool _cargando;

    /// <summary>
    /// Inicializa el control de gestión de categorías.
    /// </summary>
    /// <param name="categoriaNegocio">Servicio que ejecuta las operaciones de categorías.</param>
    public GestionCategoriasControl(CategoriaNegocio categoriaNegocio)
    {
        ArgumentNullException.ThrowIfNull(categoriaNegocio);

        _categoriaNegocio = categoriaNegocio;
        InitializeComponent();
        BackColor = TemaAplicacion.FondoPrincipal;

        comboEstado.Items.AddRange(new object[]
        {
            "Todas",
            "Activas",
            "Inactivas"
        });
        comboEstado.SelectedIndex = 0;

        txtBuscar.TextChanged += txtBuscar_TextChanged;
        comboEstado.SelectedIndexChanged += comboEstado_SelectedIndexChanged;
        gridCategorias.SelectionChanged += gridCategorias_SelectionChanged;
        btnNuevo.Click += btnNuevo_Click;
        btnEditar.Click += btnEditar_Click;
        btnDarDebaja.Click += btnDarDebaja_Click;
        btnReactivar.Click += btnReactivar_Click;
        btnActualizar.Click += btnActualizar_Click;
    }

    /// <summary>
    /// Carga todas las categorías (activas e inactivas) desde la capa de negocio
    /// y actualiza la grilla con los filtros aplicados (RF-10).
    /// </summary>
    /// <returns>Tarea que representa la carga de categorías.</returns>
    public async Task CargarCategoriasAsync()
    {
        if (_cargando)
        {
            return;
        }

        _cargando = true;
        btnActualizar.Enabled = false;

        try
        {
            _categorias = await _categoriaNegocio.ListarCategoriasAsync();
            AplicarFiltros();
        }
        catch (Exception exception)
        {
            MaterialMessageBox.Show(
                this,
                $"No se pudieron cargar las categorías.\nDetalle: {exception.Message}",
                "Error SQL");
        }
        finally
        {
            btnActualizar.Enabled = true;
            _cargando = false;
        }
    }

    private void AplicarFiltros()
    {
        string textoBusqueda = txtBuscar.Text.Trim();
        bool? estadoBuscado = comboEstado.SelectedIndex switch
        {
            1 => true,
            2 => false,
            _ => null
        };

        var categoriasFiltradas = _categorias
            .Where(categoria =>
                (!estadoBuscado.HasValue || categoria.Estado == estadoBuscado.Value) &&
                CoincideConBusqueda(categoria, textoBusqueda))
            .ToList();

        gridCategorias.DataSource = categoriasFiltradas;
        labelCantidad.Text = $"{categoriasFiltradas.Count} categoría(s)";
        ActualizarEstadoBotones();
    }

    private static bool CoincideConBusqueda(
        CategoriaRespuestaDto categoria,
        string textoBusqueda)
    {
        if (string.IsNullOrWhiteSpace(textoBusqueda))
        {
            return true;
        }

        return categoria.Descripcion?.Contains(textoBusqueda, StringComparison.OrdinalIgnoreCase) ?? false;
    }

    private void txtBuscar_TextChanged(object? sender, EventArgs e)
    {
        AplicarFiltros();
    }

    private void comboEstado_SelectedIndexChanged(object? sender, EventArgs e)
    {
        AplicarFiltros();
    }

    private async void btnActualizar_Click(object? sender, EventArgs e)
    {
        await CargarCategoriasAsync();
    }

    private async void btnNuevo_Click(object? sender, EventArgs e)
    {
        using var formulario = new CategoriaForm(_categoriaNegocio);
        Form? formularioPrincipal = FindForm();

        if (formulario.ShowDialog(formularioPrincipal) == DialogResult.OK)
        {
            await CargarCategoriasAsync();
        }
    }

    private async void btnEditar_Click(object? sender, EventArgs e)
    {
        if (gridCategorias.CurrentRow?.DataBoundItem is not CategoriaRespuestaDto categoria)
        {
            MaterialMessageBox.Show(
                this,
                "Debe seleccionar una categoría para editarla.",
                "Aviso");
            return;
        }

        using var formulario = new CategoriaForm(_categoriaNegocio, categoria);
        Form? formularioPrincipal = FindForm();

        if (formulario.ShowDialog(formularioPrincipal) == DialogResult.OK)
        {
            await CargarCategoriasAsync();
        }
    }

    private async void btnDarDebaja_Click(object? sender, EventArgs e)
    {
        if (gridCategorias.CurrentRow?.DataBoundItem is not CategoriaRespuestaDto categoria)
        {
            MaterialMessageBox.Show(
                this,
                "Debe seleccionar una categoría para darla de baja.",
                "Aviso");
            return;
        }

        if (FindForm() is not Form formularioPrincipal)
        {
            return;
        }

        btnDarDebaja.Enabled = false;

        try
        {
            // RF-13/RF-15: la regla de la baja y el texto en español los resuelve el
            // negocio; la vista solo presenta el mensaje recibido (no compone textos).
            ConfirmacionBajaCategoria confirmacion =
                await _categoriaNegocio.ObtenerConfirmacionBajaAsync(categoria.IdCategoria);

            // RF-14: si la baja no es posible (categoría inexistente o ya inactiva)
            // se informa el motivo, sin diálogo SÍ/NO y sin escribir nada.
            if (!confirmacion.PuedeDarseDeBaja)
            {
                MaterialMessageBox.Show(this, confirmacion.Mensaje, "Atención");
                return;
            }

            // RF-7: con productos activos, el mensaje del negocio informa SOLO la
            // cantidad (sin listado) y ofrece continuar o cancelar. RF-8: sin activos,
            // el negocio entrega la confirmación simple.
            var dialogo = new MaterialDialog(
                formularioPrincipal,
                "Dar de baja categoría",
                confirmacion.Mensaje,
                "SÍ",
                true,
                "NO");

            // Cancelar ⇒ sin ninguna escritura: la categoría queda sin cambios (RF-7).
            if (dialogo.ShowDialog(formularioPrincipal) != DialogResult.OK)
            {
                return;
            }

            RespuestaBajaCategoria respuesta = await _categoriaNegocio.DarBajaCategoriaAsync(categoria.IdCategoria);

            if (respuesta.Resultado != ResultadoBajaCategoria.Exito)
            {
                // Casos raros (ya inactiva / no encontrada): se muestra el mensaje
                // en español que devuelve el negocio, sin modificar nada.
                MaterialMessageBox.Show(this, respuesta.Mensaje, "Atención");
                return;
            }

            MaterialMessageBox.Show(this, respuesta.Mensaje, "Categoría desactivada");
            await CargarCategoriasAsync();
        }
        catch (Exception exception)
        {
            MaterialMessageBox.Show(
                this,
                $"No se pudo dar de baja la categoría.\nDetalle: {exception.Message}",
                "Error SQL");
        }
        finally
        {
            ActualizarEstadoBotones();
        }
    }

    private async void btnReactivar_Click(object? sender, EventArgs e)
    {
        if (gridCategorias.CurrentRow?.DataBoundItem is not CategoriaRespuestaDto categoria)
        {
            MaterialMessageBox.Show(
                this,
                "Debe seleccionar una categoría para reactivarla.",
                "Aviso");
            return;
        }

        if (FindForm() is not Form formularioPrincipal)
        {
            return;
        }

        btnReactivar.Enabled = false;

        try
        {
            // RF-9: se confirma la reactivación antes de escribir
            // (mismo patrón MaterialDialog que el diálogo de baja).
            var dialogo = new MaterialDialog(
                formularioPrincipal,
                "Reactivar categoría",
                $"¿Confirma que desea reactivar la categoría {categoria.Descripcion}?",
                "SÍ",
                true,
                "NO");

            // Cancelar ⇒ sin ninguna escritura: la categoría queda sin cambios.
            if (dialogo.ShowDialog(formularioPrincipal) != DialogResult.OK)
            {
                return;
            }

            RespuestaActivacionCategoria respuesta =
                await _categoriaNegocio.ReactivarCategoriaAsync(categoria.IdCategoria);

            if (respuesta.Resultado != ResultadoActivacionCategoria.Exito)
            {
                // Casos raros (ya activa / no encontrada): se muestra el mensaje
                // en español que devuelve el negocio, sin modificar nada.
                MaterialMessageBox.Show(this, respuesta.Mensaje, "Atención");
                return;
            }

            // El negocio reactiva sobre la MISMA fila: la grilla se recarga
            // desde la base y no puede duplicar el registro (RF-9).
            MaterialMessageBox.Show(this, respuesta.Mensaje, "Categoría reactivada");
            await CargarCategoriasAsync();
        }
        catch (Exception exception)
        {
            MaterialMessageBox.Show(
                this,
                $"No se pudo reactivar la categoría.\nDetalle: {exception.Message}",
                "Error SQL");
        }
        finally
        {
            ActualizarEstadoBotones();
        }
    }

    private void gridCategorias_SelectionChanged(object? sender, EventArgs e)
    {
        ActualizarEstadoBotones();
    }

    /// <summary>
    /// Habilita los botones de acción según la fila seleccionada:
    /// Editar siempre (también inactivas, RF-10), Dar de baja solo sobre activas
    /// y Reactivar solo sobre inactivas.
    /// </summary>
    private void ActualizarEstadoBotones()
    {
        CategoriaRespuestaDto? categoria = gridCategorias.CurrentRow?.DataBoundItem as CategoriaRespuestaDto;
        btnEditar.Enabled = categoria is not null;
        btnDarDebaja.Enabled = categoria?.Estado == true;
        btnReactivar.Enabled = categoria?.Estado == false;
    }

    private void gridCategorias_CellFormatting(
        object? sender,
        DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
        {
            return;
        }

        if (gridCategorias.Columns[e.ColumnIndex].Name == colEstado.Name && e.Value is bool estado)
        {
            e.Value = estado ? "Activo" : "Inactivo";
            e.FormattingApplied = true;
        }
    }
}
