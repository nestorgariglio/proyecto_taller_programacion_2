using System;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using CapaNegocio;

namespace CapaPresentacion
{
    public partial class login : MaterialForm
    {
        private readonly UsuarioNegocio _usuarioNegocio;
        private readonly inicio _inicioForm;

        // Inyectamos la CapaNegocio y el formulario de inicio
        public login(UsuarioNegocio usuarioNegocio, inicio inicioForm)
        {
            InitializeComponent();
            _usuarioNegocio = usuarioNegocio;
            _inicioForm = inicioForm;

            // Configuración visual de MaterialSkin
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
            string clave = textbox_clave.Text.Trim(); // Asegurate de que el control de la contraseña se llame textbox_clave

            if (string.IsNullOrEmpty(dni) || string.IsNullOrEmpty(clave))
            {
                MaterialMessageBox.Show(this, "Por favor complete todos los campos.", "Aviso");
                return;
            }

            // Llamamos a la CapaNegocio para validar el usuario
            var respuesta = await _usuarioNegocio.ValidarIngresoAsync(dni, clave);

            if (respuesta.Resultado == ResultadoAutenticacion.Exito)
            {
                // Enviamos los datos del usuario logueado a la pantalla de inicio
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
    }
}