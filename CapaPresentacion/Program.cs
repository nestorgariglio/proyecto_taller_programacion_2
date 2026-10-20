using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CapaDatos;
using CapaNegocio;
using CapaPresentacion.Tema;
using CapaPresentacion.Productos;

namespace CapaPresentacion
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            TemaAplicacion.Configurar();

            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(AppContext.BaseDirectory);
                })
                .ConfigureServices((context, services) =>
                {
                    string connectionString = context.Configuration.GetConnectionString("CadenaSQL");

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(connectionString));

                    services.AddTransient<UsuarioNegocio>();
                    services.AddTransient<CategoriaNegocio>();

                    services.AddTransient<GestionUsuariosControl>();
                    services.AddTransient<GestionCategoriasControl>();
                    services.AddTransient<ProductosForm>();
                    services.AddTransient<login>();
                    services.AddTransient<inicio>();

                    // Hotfix: el drawer de MaterialSkin 2.3.1 fija su snapshot de
                    // pestañas al mostrarse el formulario por primera vez y no se
                    // puede re-sincronizar (DrawerTabControl es auto-property y
                    // AddDrawerOverlayForm es único, en Shown). Por eso el login
                    // necesita crear un `inicio` nuevo por sesión en vez de reutilizar
                    // el mismo formulario entre cierres de sesión. Se resuelve vía
                    // factory para no inyectar el contenedor directamente.
                    services.AddTransient<Func<inicio>>(sp => () => sp.GetRequiredService<inicio>());
                })
                .Build();

            var loginForm = host.Services.GetRequiredService<login>();
            Application.Run(loginForm);
        }
    }
}
