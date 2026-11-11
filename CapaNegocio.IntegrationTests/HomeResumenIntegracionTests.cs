using CapaDatos;
using CapaEntidad;
using CapaNegocio.DTOs.Home;
using CapaNegocio.Stock;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CapaNegocio.IntegrationTests;

/// Verifica los KPIs de Home contra DB_SISTEMA_VENTA_TEST. Los datos de venta/cliente/producto/tipo
/// se crean dentro de UNA transaccion que se revierte siempre (sin DELETE fisico). Solo el escenario
/// minimo (usuario/categoria con marcador) usa el fixture, que ya lo limpia.
[Collection(ColeccionBdIntegracion.Nombre)]
public class HomeResumenIntegracionTests
{
    private readonly BdIntegracionFixture _bd;

    public HomeResumenIntegracionTests(BdIntegracionFixture bd) => _bd = bd;

    [FactIntegracion]
    public async Task Resumen_CoincideConSqlIndependienteYReglasDelDia()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        CancellationToken ct = cts.Token;

        Escenario e = await _bd.CrearEscenarioAsync(ct);

        await using var cn = new SqlConnection(_bd.Cadena);
        await cn.OpenAsync(ct);
        // Guarda exacta antes de operar.
        await GuardaConexion.VerificarBaseConectadaAsync(_bd.Cadena, ct);

        await using SqlTransaction tx = (SqlTransaction)await cn.BeginTransactionAsync(ct);
        try
        {
            var fabrica = new FabricaTransaccional(cn, tx);
            var negocio = new HomeResumenNegocio(fabrica, ResolucionUmbralStock.ConUmbral(5));

            ResumenHomeDto antes = await negocio.ObtenerResumenAsync(ct);
            Assert.True(antes.Disponible);

            DateTime hoy = DateTime.Today;
            await using (AppDbContext db = fabrica.CreateDbContext())
            {
                var tipo = new TipoFactura { Tipo = "B", NroFactura = 1 };
                var c1 = new Cliente { Nombre = BdIntegracionFixture.Marcador + "A", Apellido = "X", Dni = "9" + Random.Shared.Next(1000000, 9999999) };
                var c2 = new Cliente { Nombre = BdIntegracionFixture.Marcador + "B", Apellido = "X", Dni = "9" + Random.Shared.Next(1000000, 9999999) };
                db.TiposFactura.Add(tipo);
                db.Clientes.AddRange(c1, c2);
                await db.SaveChangesAsync(ct);

                Venta V(Cliente c, decimal total, DateTime f, bool estado = true) => new()
                {
                    IdCliente = c.IdCliente, IdTipo = tipo.IdTipo, MontoTotal = total,
                    MontoPago = total, MontoCambio = 0, Estado = estado, FechaRegistro = f
                };
                db.Ventas.AddRange(
                    V(c1, 100.50m, hoy),                         // incluida (00:00 inclusive)
                    V(c1, 200.25m, hoy.AddHours(13)),            // incluida: mismo cliente, atencion independiente
                    V(c2, 50m, hoy.AddDays(1).AddMilliseconds(-10)), // incluida (ultimo instante)
                    V(c2, 999m, hoy.AddDays(1)),                 // excluida: manana 00:00
                    V(c2, 888m, hoy.AddDays(-1).AddHours(12)),   // excluida: ayer
                    V(c2, 777m, hoy.AddHours(10), estado: false)); // excluida: anulada

                foreach (int stock in new[] { 4, 5 })
                {
                    string cod = BdIntegracionFixture.Marcador + Guid.NewGuid().ToString("N")[..6];
                    db.Productos.Add(new Producto
                    {
                        IdCategoria = e.IdCategoria, IdUsuario = e.IdUsuario, Estado = true, Stock = stock,
                        Codigo = cod, Nombre = cod + "_" + stock, PrecioCompra = 1m, PrecioVenta = 2m
                    });
                }
                await db.SaveChangesAsync(ct);
            }

            ResumenHomeDto despues = await negocio.ObtenerResumenAsync(ct);

            Assert.Equal(350.75m, despues.VentasDelDia - antes.VentasDelDia);
            Assert.Equal(3, despues.ClientesAtendidos - antes.ClientesAtendidos);
            Assert.Equal(1, despues.ProductosStockCritico - antes.ProductosStockCritico); // 4 si, 5 no

            // Consultas SQL independientes sobre los mismos datos y la misma transaccion.
            DateTime manana = hoy.AddDays(1);
            decimal sqlTotal = (decimal)(await Escalar(cn, tx,
                "SELECT ISNULL(SUM(monto_total),0) FROM dbo.VENTA WHERE estado=1 AND fecha_registro>=@d AND fecha_registro<@m",
                hoy, manana, ct))!;
            int sqlAtenciones = (int)(await Escalar(cn, tx,
                "SELECT COUNT(*) FROM dbo.VENTA WHERE estado=1 AND fecha_registro>=@d AND fecha_registro<@m",
                hoy, manana, ct))!;
            int sqlCriticos = (int)(await Escalar(cn, tx, "SELECT COUNT(*) FROM dbo.PRODUCTO WHERE stock<5", hoy, manana, ct))!;

            Assert.Equal(sqlTotal, despues.VentasDelDia);
            Assert.Equal(sqlAtenciones, despues.ClientesAtendidos);
            Assert.Equal(sqlCriticos, despues.ProductosStockCritico);
        }
        finally
        {
            await tx.RollbackAsync(CancellationToken.None);
        }
    }

    private static async Task<object?> Escalar(SqlConnection cn, SqlTransaction tx, string sql, DateTime d, DateTime m, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(sql, cn, tx);
        cmd.Parameters.Add("@d", System.Data.SqlDbType.DateTime2).Value = d;
        cmd.Parameters.Add("@m", System.Data.SqlDbType.DateTime2).Value = m;
        return await cmd.ExecuteScalarAsync(ct);
    }

    /// Fabrica cuyos contextos comparten la conexion y la transaccion de la prueba (no las cierran).
    private sealed class FabricaTransaccional : IDbContextFactory<AppDbContext>
    {
        private readonly SqlConnection _cn;
        private readonly SqlTransaction _tx;

        public FabricaTransaccional(SqlConnection cn, SqlTransaction tx) { _cn = cn; _tx = tx; }

        public AppDbContext CreateDbContext()
        {
            var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(_cn).Options);
            db.Database.UseTransaction(_tx);
            return db;
        }
    }
}
