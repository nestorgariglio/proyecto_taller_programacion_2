using System;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using CapaNegocio;
using CapaNegocio.Enums;

namespace CapaPresentacion
{
    /// <summary>
    /// Formulario de autenticación de usuarios.
    /// </summary>
    public partial class login : MaterialForm
    {
        private readonly UsuarioNegocio _usuarioNegocio;
        private readonly inicio _inicioForm;

        /// <summary>
        /// Inicializa el formulario de autenticación.
        /// </summary>
        /// <param name="usuarioNegocio">Servicio utilizado para validar las credenciales.</param>
        /// <param name="inicioForm">Formulario que se muestra después de autenticar al usuario.</param>
        public login(UsuarioNegocio usuarioNegocio, inicio inicioForm)
        {
            InitializeComponent();
            _usuarioNegocio = usuarioNegocio;
            _inicioForm = inicioForm;
            _inicioForm.CierreSesionSolicitado += inicioForm_CierreSesionSolicitado;

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(
                Primary.BlueGrey900, Primary.BlueGrey900, Primary.BlueGrey500, Accent.DeepOrange700, TextShade.WHITE
            );
        }

        private void login_Load(object sender, EventArgs e)
        {
            textbox_dni.Select();
        }

        private async void btn_ingresar_Click(object sender, EventArgs e)
        {
            string dni = textbox_dni.Text.Trim();
            string clave = textbox_clave.Text.Trim();

            if (string.IsNullOrEmpty(dni) || string.IsNullOrEmpty(clave))
            {
                MaterialMessageBox.Show(this, "Por favor complete todos los campos.", "Aviso");
                return;
            }

            var respuesta = await _usuarioNegocio.ValidarIngresoAsync(dni, clave);

            if (respuesta.Resultado == ResultadoAutenticacion.Exito)
            {
                _inicioForm.EstablecerSesionUsuario(respuesta.Usuario!);

                this.Hide();
                _inicioForm.Show();
            }
            else
            {
                MaterialMessageBox.Show(this, respuesta.Mensaje, "Atención");
            }
        }

        private void btn_cancelar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void inicioForm_CierreSesionSolicitado(object? sender, EventArgs e)
        {
            _inicioForm.Hide();
            textbox_dni.Clear();
            textbox_clave.Clear();
            Show();
            Activate();
            textbox_dni.Select();
        }
    }
}
