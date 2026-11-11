using CapaDatos;
using CapaEntidad;
using CapaNegocio.Stock;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CapaNegocio.Tests;

public class HomeResumenNegocioTests
{
    private sealed class Fabrica : IDbContextFactory<AppDbContext>
    {
        private readonly DbContextOptions<AppDbContext> _o;
        public Fabrica(string nombre) => _o = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(nombre).Options;
        public AppDbContext CreateDbContext() => new(_o);
    }

    private static readonly DateTime Ahora = new(2026, 5, 10, 15, 0, 0);

    private static Venta V(int cli, decimal t, DateTime fecha, bool est = true) =>
        new() { IdCliente = cli, IdTipo = 1, MontoTotal = t, Estado = est, FechaRegistro = fecha };

    [Fact]
    public async Task Resumen_AplicaDiaEstadoUmbralYCuentaAtenciones()
    {
        var f = new Fabrica(Guid.NewGuid().ToString());
        await using (AppDbContext db = f.CreateDbContext())
        {
            db.Ventas.AddRange(
                V(1, 10m, Ahora.Date), V(1, 20m, Ahora), V(2, 5m, Ahora.Date.AddDays(1).AddTicks(-1)),
                V(2, 99m, Ahora.Date.AddDays(1)), V(2, 99m, Ahora.Date.AddTicks(-1)), V(3, 99m, Ahora, false));
            db.Productos.AddRange(
                new Producto { Codigo = "a", Stock = 4, Estado = true }, new Producto { Codigo = "b", Stock = 5, Estado = true },
                new Producto { Codigo = "c", Stock = 0, Estado = false }, new Producto { Codigo = "d", Stock = -2, Estado = true });
            await db.SaveChangesAsync();
        }

        var r = await new HomeResumenNegocio(f, ResolucionUmbralStock.ConUmbral(5), () => Ahora).ObtenerResumenAsync();

        Assert.True(r.Disponible);
        Assert.Equal(35m, r.VentasDelDia);
        Assert.Equal(3, r.ClientesAtendidos);
        Assert.Equal(3, r.ProductosStockCritico);
    }

    [Fact]
    public async Task Resumen_SinVentas_EsCeroYModoDegradadoSoloAgotados()
    {
        var f = new Fabrica(Guid.NewGuid().ToString());
        await using (AppDbContext db = f.CreateDbContext())
        {
            db.Productos.AddRange(new Producto { Codigo = "a", Stock = 4 }, new Producto { Codigo = "c", Stock = 0 });
            await db.SaveChangesAsync();
        }

        var r = await new HomeResumenNegocio(f, ResolucionUmbralStock.Degradada(), () => Ahora).ObtenerResumenAsync();

        Assert.Equal(0m, r.VentasDelDia);
        Assert.Equal(0, r.ClientesAtendidos);
        Assert.Equal(1, r.ProductosStockCritico);
    }
}
