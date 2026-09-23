using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CapaDatos;
using CapaNegocio;
using CapaPresentacion.Tema;

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

                    services.AddTransient<GestionUsuariosControl>();
                    services.AddTransient<login>();
                    services.AddTransient<inicio>();
                })
                .Build();

            var loginForm = host.Services.GetRequiredService<login>();
            Application.Run(loginForm);
        }
    }
}
