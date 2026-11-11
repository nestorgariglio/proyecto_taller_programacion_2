using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CapaDatos;
using CapaNegocio;
using CapaPresentacion.Tema;
using CapaPresentacion.Clientes;
using CapaPresentacion.Compras;
using CapaPresentacion.Productos;
using CapaPresentacion.Proveedores;
using CapaPresentacion.Respaldos;
using CapaPresentacion.Ventas;

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

                    // La fábrica también registra AppDbContext (scoped); CompraNegocio
                    // la usa para que sus lecturas no compartan contexto entre sí.
                    services.AddDbContextFactory<AppDbContext>(options =>
                        options.UseSqlServer(connectionString));

                    services.AddTransient<UsuarioNegocio>();
                    services.AddTransient<CategoriaNegocio>();
                    services.AddTransient<ClienteNegocio>();
                    services.AddTransient<ProductoNegocio>();
                    services.AddTransient<ProveedorNegocio>();
                    services.AddTransient<CompraNegocio>();
                    services.AddTransient<ConsultaProductosVentaDatos>();
                    services.AddTransient<CarritoVentaNegocio>();

                    services.AddTransient<HomeResumenNegocio>(sp => new HomeResumenNegocio(
                        sp.GetRequiredService<IDbContextFactory<AppDbContext>>(),
                        CapaNegocio.Stock.ResolucionUmbralStock.Resolver(
                            sp.GetRequiredService<IConfiguration>()["StockCritico:UmbralMinimo"])));

                    services.AddSingleton<CapaDatos.Respaldo.IProveedorRutaRespaldo, CapaDatos.Respaldo.ProveedorRutaRespaldo>(
                        sp => new CapaDatos.Respaldo.ProveedorRutaRespaldo(
                            rutaConfigurada: sp.GetRequiredService<IConfiguration>()["Respaldos:RutaCarpeta"]));
                    services.AddSingleton<CapaDatos.Respaldo.IRespaldoRepositorio>(
                        _ => new CapaDatos.Respaldo.RespaldoRepositorio(connectionString));
                    services.AddSingleton<CapaNegocio.Respaldo.IExclusionRespaldo, CapaNegocio.Respaldo.ExclusionRespaldo>();
                    services.AddSingleton<RespaldoNegocio>(sp => new RespaldoNegocio(
                        sp.GetRequiredService<CapaDatos.Respaldo.IRespaldoRepositorio>(),
                        sp.GetRequiredService<CapaDatos.Respaldo.IProveedorRutaRespaldo>(),
                        sp.GetRequiredService<CapaNegocio.Respaldo.IExclusionRespaldo>()));
                    services.AddTransient<GenerarBackupForm>();
                    services.AddTransient<RestaurarBackupForm>();
                    services.AddTransient<Func<GenerarBackupForm>>(sp => () => sp.GetRequiredService<GenerarBackupForm>());
                    services.AddTransient<Func<RestaurarBackupForm>>(sp => () => sp.GetRequiredService<RestaurarBackupForm>());

                    services.AddTransient<GestionUsuariosControl>();
                    services.AddTransient<GestionCategoriasControl>();
                    services.AddTransient<ProductosForm>();
                    services.AddTransient<ClientesForm>();
                    services.AddTransient<ProveedoresForm>();
                    services.AddTransient<ComprasForm>();
                    services.AddTransient<VentasForm>();
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
