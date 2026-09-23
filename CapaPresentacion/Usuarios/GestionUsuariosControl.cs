using CapaNegocio;
using CapaNegocio.DTOs.Usuarios;
using CapaNegocio.Enums;
using CapaPresentacion.Tema;
using MaterialSkin.Controls;

namespace CapaPresentacion;

/// <summary>
/// Control de presentación para gestionar los usuarios del sistema.
/// </summary>
public partial class GestionUsuariosControl : UserControl
{
    private readonly UsuarioNegocio _usuarioNegocio;
    private IReadOnlyList<UsuarioRespuestaDto> _usuarios = Array.Empty<UsuarioRespuestaDto>();
    private bool _cargando;

    /// <summary>
    /// Inicializa el control de gestión de usuarios.
    /// </summary>
    /// <param name="usuarioNegocio">Servicio que ejecuta las operaciones de usuarios.</param>
    public GestionUsuariosControl(UsuarioNegocio usuarioNegocio)
    {
        ArgumentNullException.ThrowIfNull(usuarioNegocio);

        _usuarioNegocio = usuarioNegocio;
        InitializeComponent();
        BackColor = TemaAplicacion.FondoPrincipal;

        comboEstado.Items.AddRange(new object[]
        {
            "Todos",
            "Activos",
            "Inactivos"
        });
        comboEstado.SelectedIndex = 0;

        txtBuscar.TextChanged += txtBuscar_TextChanged;
        comboEstado.SelectedIndexChanged += comboEstado_SelectedIndexChanged;
        gridUsuarios.SelectionChanged += gridUsuarios_SelectionChanged;
        btnNuevo.Click += btnNuevo_Click;
        btnEditar.Click += btnEditar_Click;
        btnDesactivar.Click += btnDesactivar_Click;
        btnActualizar.Click += btnActualizar_Click;
    }

    /// <summary>
    /// Carga los usuarios desde la capa de negocio y actualiza la grilla.
    /// </summary>
    /// <returns>Tarea que representa la carga de usuarios.</returns>
    public async Task CargarUsuariosAsync()
    {
        if (_cargando)
        {
            return;
        }

        _cargando = true;
        btnActualizar.Enabled = false;

        try
        {
            _usuarios = await _usuarioNegocio.ListarUsuariosAsync();
            AplicarFiltros();
        }
        catch (Exception exception)
        {
            MaterialMessageBox.Show(
                this,
                $"No se pudieron cargar los usuarios.\nDetalle: {exception.Message}",
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

        var usuariosFiltrados = _usuarios
            .Where(usuario =>
                (!estadoBuscado.HasValue || usuario.Estado == estadoBuscado.Value) &&
                CoincideConBusqueda(usuario, textoBusqueda))
            .ToList();

        gridUsuarios.DataSource = usuariosFiltrados;
        labelCantidad.Text = $"{usuariosFiltrados.Count} usuario(s)";
        ActualizarEstadoBotones();
    }

    private static bool CoincideConBusqueda(
        UsuarioRespuestaDto usuario,
        string textoBusqueda)
    {
        if (string.IsNullOrWhiteSpace(textoBusqueda))
        {
            return true;
        }

        return usuario.Dni.ToString().Contains(textoBusqueda, StringComparison.OrdinalIgnoreCase)
            || usuario.Nombre.Contains(textoBusqueda, StringComparison.OrdinalIgnoreCase)
            || usuario.Apellido.Contains(textoBusqueda, StringComparison.OrdinalIgnoreCase)
            || (usuario.Correo?.Contains(textoBusqueda, StringComparison.OrdinalIgnoreCase) ?? false);
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
        await CargarUsuariosAsync();
    }

    private async void btnNuevo_Click(object? sender, EventArgs e)
    {
        using var formulario = new UsuarioForm(_usuarioNegocio);
        Form? formularioPrincipal = FindForm();

        if (formulario.ShowDialog(formularioPrincipal) == DialogResult.OK)
        {
            await CargarUsuariosAsync();
        }
    }

    private async void btnEditar_Click(object? sender, EventArgs e)
    {
        if (gridUsuarios.CurrentRow?.DataBoundItem is not UsuarioRespuestaDto usuario)
        {
            MaterialMessageBox.Show(
                this,
                "Debe seleccionar un usuario para editarlo.",
                "Aviso");
            return;
        }

        using var formulario = new UsuarioForm(_usuarioNegocio, usuario);
        Form? formularioPrincipal = FindForm();

        if (formulario.ShowDialog(formularioPrincipal) == DialogResult.OK)
        {
            await CargarUsuariosAsync();
        }
    }

    private void gridUsuarios_SelectionChanged(object? sender, EventArgs e)
    {
        ActualizarEstadoBotones();
    }

    private void ActualizarEstadoBotones()
    {
        UsuarioRespuestaDto? usuario = gridUsuarios.CurrentRow?.DataBoundItem as UsuarioRespuestaDto;
        btnEditar.Enabled = usuario is not null;
        btnDesactivar.Enabled = usuario is not null;
        btnDesactivar.Text = usuario?.Estado == false ? "Activar" : "Desactivar";
    }

    private async void btnDesactivar_Click(object? sender, EventArgs e)
    {
        if (gridUsuarios.CurrentRow?.DataBoundItem is not UsuarioRespuestaDto usuario)
        {
            MaterialMessageBox.Show(
                this,
                "Debe seleccionar un usuario para cambiar su estado.",
                "Aviso");
            return;
        }

        if (FindForm() is not Form formularioPrincipal)
        {
            return;
        }

        string accion = usuario.Estado ? "desactivar" : "activar";
        var dialogo = new MaterialDialog(
            formularioPrincipal,
            $"{char.ToUpper(accion[0])}{accion[1..]} usuario",
            $"¿Está seguro que desea {accion} a {usuario.Nombre} {usuario.Apellido}?",
            "SÍ",
            true,
            "NO");

        if (dialogo.ShowDialog(formularioPrincipal) != DialogResult.OK)
        {
            return;
        }

        btnDesactivar.Enabled = false;

        try
        {
            if (usuario.Estado)
            {
                RespuestaBaja respuesta = await _usuarioNegocio.DesactivarUsuarioAsync(usuario.IdUsuario);

                if (respuesta.Resultado != ResultadoBaja.Exito)
                {
                    MaterialMessageBox.Show(this, respuesta.Mensaje, "Atención");
                    return;
                }

                MaterialMessageBox.Show(this, respuesta.Mensaje, "Usuario desactivado");
            }
            else
            {
                RespuestaActivacion respuesta = await _usuarioNegocio.ActivarUsuarioAsync(usuario.IdUsuario);

                if (respuesta.Resultado != ResultadoActivacion.Exito)
                {
                    MaterialMessageBox.Show(this, respuesta.Mensaje, "Atención");
                    return;
                }

                MaterialMessageBox.Show(this, respuesta.Mensaje, "Usuario activado");
            }

            await CargarUsuariosAsync();
        }
        catch (Exception exception)
        {
            MaterialMessageBox.Show(
                this,
                $"No se pudo cambiar el estado del usuario.\nDetalle: {exception.Message}",
                "Error SQL");
        }
        finally
        {
            ActualizarEstadoBotones();
        }
    }

    private void gridUsuarios_CellFormatting(
        object? sender,
        DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
        {
            return;
        }

        if (gridUsuarios.Columns[e.ColumnIndex].Name == colEstado.Name && e.Value is bool estado)
        {
            e.Value = estado ? "Activo" : "Inactivo";
            e.FormattingApplied = true;
        }
    }
}
