using CapaNegocio;
using CapaNegocio.DTOs.Roles;
using CapaNegocio.DTOs.Usuarios;
using CapaNegocio.Enums;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CapaPresentacion;

/// <summary>
/// Formulario para registrar o modificar un usuario.
/// </summary>
public partial class UsuarioForm : MaterialForm
{
    private readonly UsuarioNegocio _usuarioNegocio;
    private readonly UsuarioRespuestaDto? _usuarioAEditar;
    private bool _guardando;

    /// <summary>
    /// Inicializa el formulario de alta o modificación de usuarios.
    /// </summary>
    /// <param name="usuarioNegocio">Servicio que crea usuarios y obtiene los roles disponibles.</param>
    /// <param name="usuarioAEditar">Usuario cuyos datos se cargarán para modificarlo.</param>
    public UsuarioForm(
        UsuarioNegocio usuarioNegocio,
        UsuarioRespuestaDto? usuarioAEditar = null)
    {
        ArgumentNullException.ThrowIfNull(usuarioNegocio);

        _usuarioNegocio = usuarioNegocio;
        _usuarioAEditar = usuarioAEditar;
        InitializeComponent();

        comboSexo.Items.AddRange(new object[] { "M", "F" });
        comboSexo.SelectedIndex = -1;
        PrepararDatosEdicion();

        btnGuardar.Click += btnGuardar_Click;
        btnCancelar.Click += btnCancelar_Click;
        Shown += UsuarioForm_Shown;

        var materialSkinManager = MaterialSkinManager.Instance;
        materialSkinManager.AddFormToManage(this);
        materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
        materialSkinManager.ColorScheme = new ColorScheme(
            Primary.BlueGrey900,
            Primary.BlueGrey900,
            Primary.BlueGrey500,
            Accent.DeepOrange700,
            TextShade.WHITE);
    }

    private async void UsuarioForm_Shown(object? sender, EventArgs e)
    {
        await CargarRolesAsync();
    }

    private async Task CargarRolesAsync()
    {
        try
        {
            IReadOnlyList<RolRespuestaDto> roles = await _usuarioNegocio.ListarRolesAsync();

            comboRol.DisplayMember = nameof(RolRespuestaDto.Descripcion);
            comboRol.ValueMember = nameof(RolRespuestaDto.IdRol);
            comboRol.DataSource = roles.ToList();
            comboRol.SelectedIndex = -1;

            if (_usuarioAEditar is not null)
            {
                comboRol.SelectedItem = roles.FirstOrDefault(
                    rol => rol.IdRol == _usuarioAEditar.IdRol);
            }

            btnGuardar.Enabled = roles.Count > 0;

            if (roles.Count == 0)
            {
                MaterialMessageBox.Show(
                    this,
                    "No hay roles disponibles para asignar al usuario.",
                    "Aviso");
            }
        }
        catch (Exception exception)
        {
            btnGuardar.Enabled = false;
            MaterialMessageBox.Show(
                this,
                $"No se pudieron cargar los roles.\nDetalle: {exception.Message}",
                "Error SQL");
        }
    }

    private void PrepararDatosEdicion()
    {
        if (_usuarioAEditar is null)
        {
            return;
        }

        Text = "Editar usuario";
        labelTitulo.Text = "Editar usuario";
        txtDni.Text = _usuarioAEditar.Dni.ToString();
        txtNombre.Text = _usuarioAEditar.Nombre;
        txtApellido.Text = _usuarioAEditar.Apellido;
        comboSexo.SelectedItem = _usuarioAEditar.Sexo;
        txtCorreo.Text = _usuarioAEditar.Correo ?? string.Empty;
        txtClave.Hint = "Nueva clave (opcional)";
    }

    private async void btnGuardar_Click(object? sender, EventArgs e)
    {
        if (_guardando)
        {
            return;
        }

        if (!int.TryParse(txtDni.Text.Trim(), out int dni))
        {
            MostrarAviso("El DNI debe ser un número válido.");
            txtDni.Focus();
            return;
        }

        if (comboSexo.SelectedItem is not string sexo)
        {
            MostrarAviso("Debe seleccionar el sexo.");
            comboSexo.Focus();
            return;
        }

        if (comboRol.SelectedItem is not RolRespuestaDto rol)
        {
            MostrarAviso("Debe seleccionar un rol.");
            comboRol.Focus();
            return;
        }

        _guardando = true;
        btnGuardar.Enabled = false;

        try
        {
            string nombre = txtNombre.Text.Trim();
            string apellido = txtApellido.Text.Trim();
            string? correo = string.IsNullOrWhiteSpace(txtCorreo.Text)
                ? null
                : txtCorreo.Text.Trim();

            if (_usuarioAEditar is null)
            {
                var entradaUsuario = new CrearUsuarioDto
                {
                    Dni = dni,
                    Nombre = nombre,
                    Apellido = apellido,
                    Sexo = sexo,
                    Correo = correo,
                    Clave = txtClave.Text,
                    IdRol = rol.IdRol
                };

                RespuestaCreacion respuesta = await _usuarioNegocio.CrearUsuarioAsync(entradaUsuario);

                if (respuesta.Resultado != ResultadoCreacion.Exito)
                {
                    MaterialMessageBox.Show(this, respuesta.Mensaje, "Atención");
                    return;
                }

                MaterialMessageBox.Show(this, respuesta.Mensaje, "Usuario creado");
            }
            else
            {
                var entradaUsuario = new ModificarUsuarioDto
                {
                    IdUsuario = _usuarioAEditar.IdUsuario,
                    Dni = dni,
                    Nombre = nombre,
                    Apellido = apellido,
                    Sexo = sexo,
                    Correo = correo,
                    Clave = string.IsNullOrWhiteSpace(txtClave.Text)
                        ? null
                        : txtClave.Text,
                    IdRol = rol.IdRol
                };

                RespuestaModificacion respuesta = await _usuarioNegocio.ModificarUsuarioAsync(entradaUsuario);

                if (respuesta.Resultado != ResultadoModificacion.Exito)
                {
                    MaterialMessageBox.Show(this, respuesta.Mensaje, "Atención");
                    return;
                }

                MaterialMessageBox.Show(this, respuesta.Mensaje, "Usuario modificado");
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception exception)
        {
            MaterialMessageBox.Show(
                this,
                $"No se pudo {(_usuarioAEditar is null ? "crear" : "modificar")} el usuario.\nDetalle: {exception.Message}",
                "Error SQL");
        }
        finally
        {
            _guardando = false;
            btnGuardar.Enabled = comboRol.Items.Count > 0;
        }
    }

    private void btnCancelar_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void MostrarAviso(string mensaje)
    {
        MaterialMessageBox.Show(this, mensaje, "Aviso");
    }
}
