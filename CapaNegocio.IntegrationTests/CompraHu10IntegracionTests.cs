using System.Globalization;
using CapaDatos;
using CapaEntidad;
using CapaNegocio.Compras;
using CapaNegocio.DTOs.Compras;
using CapaNegocio.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CapaNegocio.IntegrationTests;

[Collection(ColeccionBdIntegracion.Nombre)]
public class CompraHu10IntegracionTests
{
    private const string MensajeErrorPersistencia = "No se pudo completar la operación por un problema con el origen de datos. Intente nuevamente.";
    private const string MensajeCantidadAcumuladaSuperaMaximo = "La cantidad acumulada supera el máximo permitido.";
    private const string NombreCkT10 = BdIntegracionFixture.NombreCkFalloSimulado;

    private readonly BdIntegracionFixture _bd;

    public CompraHu10IntegracionTests(BdIntegracionFixture bd) => _bd = bd;

    // Cada test acota sus esperas con un token de 30 s (además del CommandTimeout de 30 s).
    private static CancellationToken Ct => new CancellationTokenSource(BdIntegracionFixture.TiempoPorTest).Token;

    // ---------- Persistencia ----------

    [FactIntegracion]
    public async Task Registro_PersisteStockPreciosCabeceraYDetalle()
    {
        CancellationToken ct = Ct;
        Escenario e = await _bd.CrearEscenarioAsync(ct);
        Producto p = await _bd.CrearProductoAsync(e, 10, 5.00m, 8.00m, ct);

        RespuestaRegistroCompra r;
        await using (AppDbContext db = _bd.CrearContexto())
        {
            r = await Registrar(db, e, Linea(p, 3, 10.33m, 20.00m));
        }

        Assert.Equal(ResultadoRegistroCompra.Exito, r.Resultado);
        Assert.NotNull(r.IdCompra);

        Producto prod = await _bd.LeerProductoAsync(p.IdProducto, ct);
        Assert.Equal(13, prod.Stock);
        Assert.Equal(10.33m, prod.PrecioCompra);
        Assert.Equal(20.00m, prod.PrecioVenta);

        await using AppDbContext lectura = _bd.CrearContexto();
        Compra c = await lectura.Compras.AsNoTracking().Include(x => x.Detalles)
            .SingleAsync(x => x.IdCompra == r.IdCompra, ct);
        Assert.Equal(e.IdProveedor, c.IdProveedor);
        Assert.Equal(e.IdUsuario, c.IdUsuario);
        Assert.True(c.Estado);
        Assert.Equal(30.99m, c.MontoTotal);
        DetalleCompra d = Assert.Single(c.Detalles!);
        Assert.Equal((p.IdProducto, 3, 10.33m, 30.99m), (d.IdProducto, d.Cantidad, d.PrecioCompra, d.Subtotal));
    }

    // ---------- Rollback completo / producto inactivo durante la escritura ----------
    // Producto inexistente en plena escritura no es alcanzable: el detalle recién insertado en la misma
    // transacción lo referencia por FK. Inactivo y "0 filas" comparten la misma ruta (filas != 1).

    [FactIntegracion]
    public async Task Rollback_SiUnProductoSeDesactivaTrasValidar_NoQuedaNingunEfecto()
    {
        CancellationToken ct = Ct;
        Escenario e = await _bd.CrearEscenarioAsync(ct);
        Producto p1 = await _bd.CrearProductoAsync(e, 10, 5m, 8m, ct);   // id menor: se actualiza primero
        Producto p2 = await _bd.CrearProductoAsync(e, 20, 6m, 9m, ct);

        Producto antes1 = await _bd.LeerProductoAsync(p1.IdProducto, ct);

        RespuestaRegistroCompra r;
        await using (var db = new ContextoConIntercepcion(
            _bd.CrearOpciones(), () => _bd.DesactivarProductoAsync(p2.IdProducto, ct)))
        {
            r = await Registrar(db, e, Linea(p1, 2, 11m, 15m), Linea(p2, 4, 12m, 18m));
        }

        Assert.Equal(ResultadoRegistroCompra.ErrorPersistencia, r.Resultado);
        Assert.Equal(MensajeErrorPersistencia, r.Mensaje);

        Producto a1 = await _bd.LeerProductoAsync(p1.IdProducto, ct);
        Producto a2 = await _bd.LeerProductoAsync(p2.IdProducto, ct);
        Assert.Equal((10, 5m, 8m), (a1.Stock, a1.PrecioCompra, a1.PrecioVenta));
        Assert.Equal(antes1.FechaModificacion, a1.FechaModificacion);      // el UPDATE de p1 (y su trigger) se revirtió
        Assert.False(a2.Estado);                                           // prueba de que la alteración externa ocurrió
        Assert.Equal((20, 6m, 9m), (a2.Stock, a2.PrecioCompra, a2.PrecioVenta));
        Assert.Equal(0, await _bd.ContarComprasAsync(e, ct));
        Assert.Equal(0, await _bd.ContarDetallesAsync(new[] { p1.IdProducto, p2.IdProducto }, ct));
    }

    // ---------- Desborde ----------

    [FactIntegracion]
    public async Task Desborde_PrevioALaEscritura_RechazaSinCambios()
    {
        CancellationToken ct = Ct;
        Escenario e = await _bd.CrearEscenarioAsync(ct);
        Producto p = await _bd.CrearProductoAsync(e, int.MaxValue - 1, 5m, 8m, ct);

        RespuestaRegistroCompra r;
        await using (AppDbContext db = _bd.CrearContexto())
        {
            r = await Registrar(db, e, Linea(p, 2, 10m, 12m));
        }

        Assert.Equal(ResultadoRegistroCompra.CantidadInvalida, r.Resultado);
        Assert.Equal(MensajeCantidadAcumuladaSuperaMaximo, r.Mensaje);
        Assert.Equal(0, r.IndiceLineaAfectada);
        Assert.Equal(p.IdProducto, r.IdProductoAfectado);

        Producto despues = await _bd.LeerProductoAsync(p.IdProducto, ct);
        Assert.Equal((int.MaxValue - 1, 5m, 8m), (despues.Stock, despues.PrecioCompra, despues.PrecioVenta));
        Assert.Equal(0, await _bd.ContarComprasAsync(e, ct));
        Assert.Equal(0, await _bd.ContarDetallesAsync(new[] { p.IdProducto }, ct));
    }

    [FactIntegracion]
    public async Task Desborde_EntreValidacionYEscritura_RevierteTodo()
    {
        CancellationToken ct = Ct;
        Escenario e = await _bd.CrearEscenarioAsync(ct);
        Producto p = await _bd.CrearProductoAsync(e, 10, 5m, 8m, ct);

        RespuestaRegistroCompra r;
        await using (var db = new ContextoConIntercepcion(
            _bd.CrearOpciones(), () => _bd.EstablecerStockAsync(p.IdProducto, int.MaxValue, ct)))
        {
            r = await Registrar(db, e, Linea(p, 2, 10m, 12m));
        }

        Assert.Equal(ResultadoRegistroCompra.ErrorPersistencia, r.Resultado);
        Assert.Equal(MensajeErrorPersistencia, r.Mensaje);
        Producto despues = await _bd.LeerProductoAsync(p.IdProducto, ct);
        Assert.Equal((int.MaxValue, 5m, 8m), (despues.Stock, despues.PrecioCompra, despues.PrecioVenta));
        Assert.Equal(0, await _bd.ContarComprasAsync(e, ct));
        Assert.Equal(0, await _bd.ContarDetallesAsync(new[] { p.IdProducto }, ct));
    }

    // ---------- Concurrencia ----------

    [FactIntegracion]
    public async Task Concurrencia_MismoProducto_SumaStockYDejaPreciosDeUnaCompra()
    {
        const int n = 8;
        const int stockInicial = 100;
        CancellationToken ct = Ct;
        Escenario e = await _bd.CrearEscenarioAsync(ct);
        Producto p = await _bd.CrearProductoAsync(e, stockInicial, 5m, 8m, ct);

        Task<RespuestaRegistroCompra>[] tareas = Enumerable.Range(1, n)
            .Select(i => Task.Run(async () =>
            {
                await using AppDbContext db = _bd.CrearContexto();
                return await Registrar(db, e, Linea(p, i, 10m + i, 20m + i));
            }))
            .ToArray();

        RespuestaRegistroCompra[] respuestas = await Task.WhenAll(tareas);

        Assert.True(
            respuestas.All(r => r.Resultado == ResultadoRegistroCompra.Exito),
            "Resultados: " + string.Join(", ", respuestas.Select(r => r.Resultado)));

        Producto final = await _bd.LeerProductoAsync(p.IdProducto, ct);
        Assert.Equal(stockInicial + (n * (n + 1) / 2), final.Stock);
        Assert.Contains(
            Enumerable.Range(1, n),
            i => final.PrecioCompra == 10m + i && final.PrecioVenta == 20m + i);   // el par sale de una misma compra
        Assert.Equal(n, await _bd.ContarComprasAsync(e, ct));
        Assert.Equal(n, await _bd.ContarDetallesAsync(new[] { p.IdProducto }, ct));
    }

    // ---------- Trigger TR_PRODUCTO_FECHA_MODIFICACION ----------

    [FactIntegracion]
    public async Task Trigger_FechaModificacionCambiaTrasElUpdateDirecto()
    {
        CancellationToken ct = Ct;
        Escenario e = await _bd.CrearEscenarioAsync(ct);
        Producto p = await _bd.CrearProductoAsync(e, 10, 5m, 8m, ct);

        Producto antes = await _bd.LeerProductoAsync(p.IdProducto, ct);
        await Task.Delay(100, ct);
        DateTime desde = DateTime.Now.AddMinutes(-1);

        RespuestaRegistroCompra r;
        await using (AppDbContext db = _bd.CrearContexto())
        {
            r = await Registrar(db, e, Linea(p, 1, 6m, 9m));
        }

        Assert.Equal(ResultadoRegistroCompra.Exito, r.Resultado);
        Producto despues = await _bd.LeerProductoAsync(p.IdProducto, ct);
        Assert.NotNull(despues.FechaModificacion);
        Assert.True(antes.FechaModificacion is null || despues.FechaModificacion > antes.FechaModificacion);
        Assert.InRange(despues.FechaModificacion!.Value, desde, DateTime.Now.AddMinutes(1));
        Assert.Equal(antes.IdUsuarioModifica, despues.IdUsuarioModifica);     // HU-10 no toca auditoría de usuario
        Assert.Equal(antes.MotivoAjusteStock, despues.MotivoAjusteStock);
    }

    // ---------- Precisión decimal real ----------

    [FactIntegracion]
    public async Task Precision_DecimalReal18_2_YIdaYVueltaConDosDecimales()
    {
        CancellationToken ct = Ct;

        List<object?[]> columnas = await _bd.ConsultarAsync(
            "SELECT TABLE_NAME, COLUMN_NAME, NUMERIC_PRECISION, NUMERIC_SCALE FROM INFORMATION_SCHEMA.COLUMNS " +
            "WHERE TABLE_SCHEMA = N'dbo' AND ((TABLE_NAME = N'PRODUCTO' AND COLUMN_NAME IN (N'precio_compra', N'precio_venta')) " +
            "OR (TABLE_NAME = N'COMPRA' AND COLUMN_NAME = N'monto_total') " +
            "OR (TABLE_NAME = N'DETALLE_COMPRA' AND COLUMN_NAME IN (N'precio_compra', N'subtotal'))) " +
            "ORDER BY TABLE_NAME, COLUMN_NAME;", ct);

        Assert.Equal(5, columnas.Count);   // PRODUCTO x2, COMPRA x1, DETALLE_COMPRA x2
        Assert.All(columnas, f =>
        {
            Assert.Equal(18, Convert.ToInt32(f[2]));
            Assert.Equal(2, Convert.ToInt32(f[3]));
        });

        Escenario e = await _bd.CrearEscenarioAsync(ct);
        Producto p1 = await _bd.CrearProductoAsync(e, 1, 1m, 1m, ct);
        Producto p2 = await _bd.CrearProductoAsync(e, 1, 1m, 1m, ct);
        Producto p3 = await _bd.CrearProductoAsync(e, 1, 1m, 1m, ct);

        RespuestaRegistroCompra r;
        await using (AppDbContext db = _bd.CrearContexto())
        {
            r = await Registrar(
                db, e,
                Linea(p1, 1, 0.01m, 999_999_999.99m),
                Linea(p2, 1, 1234.56m, 7890.12m),
                Linea(p3, 1, 999_999_999.99m, 0.01m));
        }

        Assert.Equal(ResultadoRegistroCompra.Exito, r.Resultado);
        Assert.Equal((0.01m, 999_999_999.99m), await PreciosAsync(p1.IdProducto, ct));
        Assert.Equal((1234.56m, 7890.12m), await PreciosAsync(p2.IdProducto, ct));
        Assert.Equal((999_999_999.99m, 0.01m), await PreciosAsync(p3.IdProducto, ct));

        await using AppDbContext lectura = _bd.CrearContexto();
        Compra c = await lectura.Compras.AsNoTracking().Include(x => x.Detalles)
            .SingleAsync(x => x.IdCompra == r.IdCompra, ct);
        Assert.Equal(0.01m + 1234.56m + 999_999_999.99m, c.MontoTotal);
        Assert.Equal(
            new[] { 0.01m, 1234.56m, 999_999_999.99m },
            c.Detalles!.OrderBy(d => d.IdDetalleCompra).Select(d => d.Subtotal).ToArray());
    }

    private async Task<(decimal, decimal)> PreciosAsync(int idProducto, CancellationToken ct)
    {
        Producto x = await _bd.LeerProductoAsync(idProducto, ct);
        return (x.PrecioCompra, x.PrecioVenta);
    }

    // ---------- T10 automatizada sobre DB_SISTEMA_VENTA_TEST (T10-A, T10-B, T10-F; T10-C y T10-E son de UI) ----------
    // Fallo inyectado DURANTE el UPDATE del segundo producto (p2): COMPRA, DETALLE_COMPRA y el UPDATE de p1 ya
    // se ejecutaron dentro de la misma transacción. El CHECK es temporal y se elimina siempre en el finally.

    [FactIntegracion]
    public async Task T10_Atomicidad_FalloEnUpdateDelSegundoProducto_RollbackCompletoYReintento()
    {
        const int stockInicialP2 = 20;
        const decimal precioCompraP2Fallo = 1_000_000.00m;   // viola CHECK (precio_compra < 999999.00) y es válido para el servicio
        const decimal precioVentaP2Fallo = 1_500_000.00m;
        CancellationToken ct = Ct;
        string existeCk =
            $"SELECT COUNT(*) FROM sys.check_constraints WHERE name = N'{NombreCkT10}' " +
            "AND parent_object_id = OBJECT_ID(N'dbo.PRODUCTO')";

        // A0: sin residuo y sin triggers en COMPRA/DETALLE_COMPRA (descarta el error 334); instantánea de migraciones.
        Assert.Equal(0, await _bd.EscalarAsync(existeCk, ct));
        Assert.Equal(0, await _bd.EscalarAsync(
            "SELECT COUNT(*) FROM sys.triggers WHERE OBJECT_NAME(parent_id) IN (N'COMPRA', N'DETALLE_COMPRA')", ct));
        int migracionesAntes = await _bd.EscalarAsync("SELECT COUNT(*) FROM [__EFMigrationsHistory]", ct);

        Escenario e = await _bd.CrearEscenarioAsync(ct);
        Producto p1 = await _bd.CrearProductoAsync(e, 10, 5m, 8m, ct);
        Producto p2 = await _bd.CrearProductoAsync(e, stockInicialP2, 6m, 9m, ct);
        Assert.True(p1.IdProducto < p2.IdProducto);   // el hook actualiza en orden ascendente de id: la línea 2 falla después de la 1
        int[] ids = { p1.IdProducto, p2.IdProducto };
        LineaCompraDto[] lineas =
        {
            Linea(p1, 3, 10.33m, 20.00m),                        // subtotal 30,99
            Linea(p2, 1, precioCompraP2Fallo, precioVentaP2Fallo) // subtotal 1.000.000,00
        };
        const decimal montoEsperado = 1_000_030.99m;

        // A1: línea base: estado original de ambos productos (stock, precios y FechaModificacion) y S0 del catálogo (T10-B).
        Producto antes1 = await _bd.LeerProductoAsync(p1.IdProducto, ct);
        Producto antes2 = await _bd.LeerProductoAsync(p2.IdProducto, ct);
        const string sqlCatalogo =
            "SELECT (SELECT COUNT(*) FROM dbo.PRODUCTO), (SELECT COUNT(*) FROM dbo.PROVEEDOR), " +
            "(SELECT COUNT(*) FROM dbo.USUARIO), (SELECT COUNT(*) FROM dbo.CATEGORIA)";
        object?[] s0 = (await _bd.ConsultarAsync(sqlCatalogo, ct))[0];

        // A2: restricción temporal elegida por el usuario (valida las filas existentes al crearla).
        var registro = new RegistroComandosInterceptor();
        RespuestaRegistroCompra fallo;
        try
        {
            await _bd.EjecutarAsync(
                $"ALTER TABLE dbo.PRODUCTO WITH NOCHECK ADD CONSTRAINT {NombreCkT10} CHECK (precio_compra < 999999.00);", ct);

            List<object?[]> meta = await _bd.ConsultarAsync(
                "SELECT is_disabled FROM sys.check_constraints " +
                $"WHERE name = N'{NombreCkT10}' AND parent_object_id = OBJECT_ID(N'dbo.PRODUCTO')", ct);
            Assert.Single(meta);
            Assert.False(Convert.ToBoolean(meta[0][0]));   // habilitada

            // A3: registro con fallo inyectado.
            await using (var db = new AppDbContext(_bd.CrearOpciones(registro)))
            {
                fallo = await Registrar(db, e, lineas);
            }
        }
        finally
        {
            await _bd.EjecutarAsync(
                $"IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'{NombreCkT10}' " +
                $"AND parent_object_id = OBJECT_ID(N'dbo.PRODUCTO')) " +
                $"ALTER TABLE dbo.PRODUCTO DROP CONSTRAINT {NombreCkT10};", Ct);
        }

        // A4: sin residuo del CHECK.
        Assert.Equal(0, await _bd.EscalarAsync(existeCk, ct));

        // A5: el resultado es ErrorPersistencia con el mensaje único.
        Assert.Equal(ResultadoRegistroCompra.ErrorPersistencia, fallo.Resultado);
        Assert.Equal(MensajeErrorPersistencia, fallo.Mensaje);

        // A6: el UPDATE de p1 se ejecutó antes de que fallara el de p2 (error 547). Límite: no se puede leer
        // un dato sin commit desde otra conexión sin bloquearse; esta es la evidencia del orden.
        Assert.Equal(new[] { "OK:1", "ERR:547" }, registro.UpdatesProducto.ToArray());

        // A7: rollback real: estado de BD idéntico al original (incluida FechaModificacion revertida por el trigger).
        Producto despues1 = await _bd.LeerProductoAsync(p1.IdProducto, ct);
        Producto despues2 = await _bd.LeerProductoAsync(p2.IdProducto, ct);
        Assert.Equal(
            (antes1.Stock, antes1.PrecioCompra, antes1.PrecioVenta, antes1.FechaModificacion),
            (despues1.Stock, despues1.PrecioCompra, despues1.PrecioVenta, despues1.FechaModificacion));
        Assert.Equal(
            (antes2.Stock, antes2.PrecioCompra, antes2.PrecioVenta, antes2.FechaModificacion),
            (despues2.Stock, despues2.PrecioCompra, despues2.PrecioVenta, despues2.FechaModificacion));
        Assert.Equal(0, await _bd.ContarComprasAsync(e, ct));
        Assert.Equal(0, await _bd.ContarDetallesAsync(ids, ct));
        Assert.Equal(0, await _bd.EscalarAsync(
            "SELECT COUNT(*) FROM dbo.DETALLE_COMPRA d WHERE NOT EXISTS (SELECT 1 FROM dbo.COMPRA c WHERE c.id_compra = d.id_compra)", ct));

        // A8: reintento sin la restricción: éxito, 1 cabecera + 2 detalles y stocks correctos.
        await using (AppDbContext db = _bd.CrearContexto())
        {
            Assert.Equal(ResultadoRegistroCompra.Exito, (await Registrar(db, e, lineas)).Resultado);
        }

        await AssertCompraT10Async(e, ids, compras: 1, detalles: 2, montoEsperado, ct);
        Assert.Equal(13, (await _bd.LeerProductoAsync(p1.IdProducto, ct)).Stock);
        Assert.Equal(stockInicialP2 + 1, (await _bd.LeerProductoAsync(p2.IdProducto, ct)).Stock);

        // A9: T10-B (catálogo sin altas/bajas) y T10-F (sin cambios de esquema).
        object?[] s1 = (await _bd.ConsultarAsync(sqlCatalogo, ct))[0];
        Assert.Equal(s0.Select(x => Convert.ToInt32(x)), s1.Select(x => Convert.ToInt32(x)));
        Assert.Equal(migracionesAntes, await _bd.EscalarAsync("SELECT COUNT(*) FROM [__EFMigrationsHistory]", ct));
    }

    private async Task AssertCompraT10Async(Escenario e, int[] ids, int compras, int detalles, decimal montoEsperado, CancellationToken ct)
    {
        Assert.Equal(compras, await _bd.ContarComprasAsync(e, ct));
        Assert.Equal(detalles, await _bd.ContarDetallesAsync(ids, ct));

        await using AppDbContext db = _bd.CrearContexto();
        List<Compra> lista = await db.Compras.AsNoTracking().Include(c => c.Detalles)
            .Where(c => c.IdProveedor == e.IdProveedor).ToListAsync(ct);
        Assert.All(lista, c =>
        {
            Assert.Equal(montoEsperado, c.MontoTotal);
            Assert.Equal(montoEsperado, c.Detalles!.Sum(d => d.Subtotal));
            Assert.Equal(
                new[] { 30.99m, 1_000_000.00m },
                c.Detalles!.OrderBy(d => d.IdDetalleCompra).Select(d => d.Subtotal).ToArray());
        });
    }

    // ---------- Utilidades ----------

    private static LineaCompraDto Linea(Producto p, int cantidad, decimal precioCompra, decimal precioVenta) => new()
    {
        IdProducto = p.IdProducto,
        CodigoProducto = p.Codigo ?? string.Empty,
        NombreProducto = p.Nombre ?? string.Empty,
        Cantidad = cantidad,
        PrecioCompra = precioCompra,
        PrecioVenta = precioVenta,
        Subtotal = CalculoImportesCompra.CalcularSubtotal(cantidad, precioCompra)
    };

    private static Task<RespuestaRegistroCompra> Registrar(AppDbContext db, Escenario e, params LineaCompraDto[] lineas)
        => new CompraNegocio(db).RegistrarCompraAsync(
            new SolicitudRegistrarCompraDto { IdProveedor = e.IdProveedor, Lineas = lineas },
            e.IdUsuario,
            "Administrador");
}
