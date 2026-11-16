using CapaDatos;
using CapaEntidad;
using CapaNegocio.DTOs.Reportes;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CapaNegocio.Tests;

public class ReporteNegocioTests
{
    private sealed class Fabrica : IDbContextFactory<AppDbContext>
    {
        private readonly DbContextOptions<AppDbContext> _o;
        public Fabrica(string nombre) => _o = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(nombre).Options;
        public AppDbContext CreateDbContext() => new(_o);
    }

    private static readonly DateTime Ahora = new(2026, 11, 16, 15, 0, 0);

    [Fact]
    public async Task Reporte_CalculaTotalesMargenTicketYAgrupacionesCorrectamente()
    {
        var f = new Fabrica(Guid.NewGuid().ToString());
        await using (AppDbContext db = f.CreateDbContext())
        {
            var catBebidas = new Categoria { Descripcion = "Bebidas", Estado = true };
            var catAlmacen = new Categoria { Descripcion = "Almacén", Estado = true };
            db.Categorias.AddRange(catBebidas, catAlmacen);

            var p1 = new Producto { Codigo = "P1", Nombre = "Cola", Categoria = catBebidas, PrecioCompra = 100m, PrecioVenta = 150m, Stock = 10, Estado = true };
            var p2 = new Producto { Codigo = "P2", Nombre = "Arroz", Categoria = catAlmacen, PrecioCompra = 200m, PrecioVenta = 300m, Stock = 10, Estado = true };
            db.Productos.AddRange(p1, p2);

            // Venta 1 (Hoy): 2 x P1 = 300 venta, costo 200 -> ganancia 100
            var v1 = new Venta
            {
                IdCliente = 1,
                IdTipo = 1,
                MontoTotal = 300m,
                Estado = true,
                FechaRegistro = Ahora,
                Detalles = new List<DetalleVenta>
                {
                    new() { Producto = p1, Cantidad = 2, PrecioCompra = 100m, PrecioVenta = 150m, Subtotal = 300m }
                }
            };

            // Venta 2 (Ayer): 1 x P2 = 300 venta, costo 200 -> ganancia 100
            var v2 = new Venta
            {
                IdCliente = 1,
                IdTipo = 1,
                MontoTotal = 300m,
                Estado = true,
                FechaRegistro = Ahora.AddDays(-1),
                Detalles = new List<DetalleVenta>
                {
                    new() { Producto = p2, Cantidad = 1, PrecioCompra = 200m, PrecioVenta = 300m, Subtotal = 300m }
                }
            };

            // Venta 3 (Anulada): Debe ignorarse
            var v3 = new Venta
            {
                IdCliente = 1,
                IdTipo = 1,
                MontoTotal = 500m,
                Estado = false,
                FechaRegistro = Ahora,
                Detalles = new List<DetalleVenta>
                {
                    new() { Producto = p1, Cantidad = 5, PrecioCompra = 100m, PrecioVenta = 150m, Subtotal = 500m }
                }
            };

            // Venta 4 (Fuera de rango 7 días): 15 días atrás
            var v4 = new Venta
            {
                IdCliente = 1,
                IdTipo = 1,
                MontoTotal = 1000m,
                Estado = true,
                FechaRegistro = Ahora.AddDays(-15),
                Detalles = new List<DetalleVenta>
                {
                    new() { Producto = p1, Cantidad = 10, PrecioCompra = 100m, PrecioVenta = 150m, Subtotal = 1000m }
                }
            };

            db.Ventas.AddRange(v1, v2, v3, v4);
            await db.SaveChangesAsync();
        }

        var servicio = new ReporteNegocio(f, () => Ahora);
        ReporteMetricasDto reporte = await servicio.ObtenerReporteAsync("Últimos 7 días");

        Assert.Equal(600m, reporte.FacturacionTotal);
        Assert.Equal(200m, reporte.GananciaTotal);
        Assert.Equal(300m, reporte.TicketPromedio);
        Assert.Equal(2, reporte.CantidadVentas);

        // Evolución diaria (7 días)
        Assert.Equal(7, reporte.EvolucionVentas.Count);

        // Categorías
        Assert.Equal(2, reporte.ParticipacionCategorias.Count);
        Assert.Contains(reporte.ParticipacionCategorias, c => c.Categoria == "Bebidas" && c.Monto == 300m && c.Porcentaje == 50.0);
        Assert.Contains(reporte.ParticipacionCategorias, c => c.Categoria == "Almacén" && c.Monto == 300m && c.Porcentaje == 50.0);

        // Resumen diario
        Assert.Equal(2, reporte.ResumenDiario.Count);
    }

    [Fact]
    public async Task Reporte_SinVentas_RetornaMetricasEnCero()
    {
        var f = new Fabrica(Guid.NewGuid().ToString());
        var servicio = new ReporteNegocio(f, () => Ahora);

        ReporteMetricasDto reporte = await servicio.ObtenerReporteAsync("Últimos 7 días");

        Assert.Equal(0m, reporte.FacturacionTotal);
        Assert.Equal(0m, reporte.GananciaTotal);
        Assert.Equal(0m, reporte.TicketPromedio);
        Assert.Equal(0, reporte.CantidadVentas);
        Assert.Empty(reporte.ParticipacionCategorias);
        Assert.Empty(reporte.ResumenDiario);
    }
}
