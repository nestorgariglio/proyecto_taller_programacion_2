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
        private readonly Func<inicio> _inicioFactory;

        /// <summary>
        /// Formulario principal de la sesión activa. Es nulo mientras no hay sesión
        /// iniciada y se recrea en cada ingreso (ver <see cref="inicioForm_CierreSesionSolicitado"/>).
        /// </summary>
        private inicio? _inicioForm;

        /// <summary>
        /// Inicializa el formulario de autenticación.
        /// </summary>
        /// <param name="usuarioNegocio">Servicio utilizado para validar las credenciales.</param>
        /// <param name="inicioFactory">
        /// Fábrica que crea un formulario principal nuevo por sesión. Debe ser nueva en
        /// cada ingreso porque el drawer de MaterialSkin 2.3.1 fija su snapshot de
        /// pestañas al mostrarse y no se puede re-sincronizar.
        /// </param>
        public login(UsuarioNegocio usuarioNegocio, Func<inicio> inicioFactory)
        {
            InitializeComponent();
            _usuarioNegocio = usuarioNegocio;
            _inicioFactory = inicioFactory;

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
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
                // Hotfix: se crea un `inicio` nuevo por sesión. El drawer de
                // MaterialSkin 2.3.1 toma una foto única de las pestañas al mostrarse
                // el formulario (AddDrawerOverlayForm en Shown) y no puede re-sincronizarse
                // al cambiar de rol; reutilizar el mismo form provoca KeyNotFoundException
                // al pintar el drawer. El login, en cambio, se reutiliza.
                _inicioForm = _inicioFactory();
                _inicioForm.CierreSesionSolicitado += inicioForm_CierreSesionSolicitado;
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
            // Hotfix: se descarta el `inicio` de la sesión que termina (se desuscribe
            // su evento y se libera) para no acumular handlers ni formularios muertos.
            // En el próximo ingreso se creará un `inicio` nuevo con el drawer limpio.
            if (_inicioForm != null)
            {
                _inicioForm.CierreSesionSolicitado -= inicioForm_CierreSesionSolicitado;
                _inicioForm.Hide();
                _inicioForm.Dispose();
                _inicioForm = null;
            }

            textbox_dni.Clear();
            textbox_clave.Clear();
            Show();
            Activate();
            textbox_dni.Select();
        }
    }
}
