using CapaNegocio;
using CapaNegocio.DTOs.Clientes;
using CapaNegocio.Enums;
using MaterialSkin;
using MaterialSkin.Controls;

namespace CapaPresentacion.Clientes;

/// <summary>
/// Formulario modal para registrar o modificar un cliente del padrón (RF-1 a RF-15).
/// Los textos de los campos se envían en crudo a <see cref="ClienteNegocio"/>: el
/// Recorte, la obligatoriedad, los formatos (RF-3) y la unicidad (RF-4) los aplica
/// la lógica de negocio. Ante cualquier rechazo se muestra el <c>Mensaje</c> del
/// negocio sin cerrar el modal y conservando lo tipeado; solo un resultado
/// <c>Exito</c> cierra con <see cref="DialogResult.OK"/> para que la grilla recargue
/// (RNF-3). La vista no valida ni redacta mensajes de negocio (RF-26).
/// </summary>
public partial class ClienteForm : MaterialForm
{
    private readonly ClienteNegocio _clienteNegocio;
    private readonly ClienteRespuestaDto? _clienteAEditar;
    private bool _guardando;

    /// <summary>
    /// Inicializa el formulario de alta o modificación de clientes.
    /// </summary>
    /// <param name="clienteNegocio">Servicio que crea y modifica clientes.</param>
    /// <param name="clienteAEditar">
    /// Cliente cuyos datos se precargan para modificarlo; <see langword="null"/> en el alta.
    /// </param>
    public ClienteForm(ClienteNegocio clienteNegocio, ClienteRespuestaDto? clienteAEditar = null)
    {
        ArgumentNullException.ThrowIfNull(clienteNegocio);

        _clienteNegocio = clienteNegocio;
        _clienteAEditar = clienteAEditar;
        InitializeComponent();

        PrepararDatosEdicion();

        btnGuardar.Click += btnGuardar_Click;
        btnCancelar.Click += btnCancelar_Click;

        var materialSkinManager = MaterialSkinManager.Instance;
        materialSkinManager.AddFormToManage(this);
    }

    /// <summary>
    /// En modo edición precarga los cinco campos con los datos actuales del cliente
    /// y actualiza los títulos; en modo alta los campos quedan vacíos (RF-1).
    /// </summary>
    private void PrepararDatosEdicion()
    {
        if (_clienteAEditar is null)
        {
            return;
        }

        Text = "Editar cliente";
        labelTitulo.Text = "Editar cliente";

        txtDocumento.Text = _clienteAEditar.Documento ?? string.Empty;
        txtNombre.Text = _clienteAEditar.Nombre ?? string.Empty;
        txtApellido.Text = _clienteAEditar.Apellido ?? string.Empty;
        txtCorreo.Text = _clienteAEditar.Correo ?? string.Empty;
        txtTelefono.Text = _clienteAEditar.Telefono ?? string.Empty;
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
            // Valores crudos de los campos: el Recorte y las validaciones los aplica
            // CapaNegocio antes de validar y guardar (RF-3 / RF-4).
            if (_clienteAEditar is null)
            {
                var entradaCliente = new CrearClienteDto
                {
                    Documento = txtDocumento.Text,
                    Nombre = txtNombre.Text,
                    Apellido = txtApellido.Text,
                    Correo = txtCorreo.Text,
                    Telefono = txtTelefono.Text
                };

                RespuestaCreacionCliente respuesta =
                    await _clienteNegocio.CrearClienteAsync(entradaCliente);

                if (respuesta.Resultado != ResultadoCreacionCliente.Exito)
                {
                    // RF-26: se muestra el mensaje del negocio (validación o
                    // duplicado) sin cerrar el modal y conservando lo tipeado.
                    MaterialMessageBox.Show(this, respuesta.Mensaje, "Atención");
                    txtDocumento.Focus();
                    return;
                }

                MaterialMessageBox.Show(this, respuesta.Mensaje, "Cliente creado");
            }
            else
            {
                var entradaCliente = new ModificarClienteDto
                {
                    IdCliente = _clienteAEditar.IdCliente,
                    Documento = txtDocumento.Text,
                    Nombre = txtNombre.Text,
                    Apellido = txtApellido.Text,
                    Correo = txtCorreo.Text,
                    Telefono = txtTelefono.Text
                };

                RespuestaModificacionCliente respuesta =
                    await _clienteNegocio.ModificarClienteAsync(entradaCliente);

                if (respuesta.Resultado != ResultadoModificacionCliente.Exito)
                {
                    MaterialMessageBox.Show(this, respuesta.Mensaje, "Atención");
                    txtDocumento.Focus();
                    return;
                }

                MaterialMessageBox.Show(this, respuesta.Mensaje, "Cliente modificado");
            }

            DialogResult = DialogResult.OK;
            Close();
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
