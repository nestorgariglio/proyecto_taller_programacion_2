using CapaDatos;
using CapaEntidad;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace CapaNegocio.IntegrationTests;

/// Una única colección: los tests de integración no corren en paralelo entre sí.
[CollectionDefinition(Nombre)]
public sealed class ColeccionBdIntegracion : ICollectionFixture<BdIntegracionFixture>
{
    public const string Nombre = "BD integracion (secuencial)";
}

public sealed record Escenario(int IdUsuario, int IdProveedor, int IdCategoria);

/// Valida el destino, comprueba que no hay migraciones pendientes (sin aplicar ninguna) y limpia
/// únicamente las filas sintéticas con el marcador propio y un CHECK residual de T10.
/// No usa EnsureCreated/EnsureDeleted/Migrate.
public sealed class BdIntegracionFixture : IAsyncLifetime
{
    /// Prefijo de todo dato sintético creado por estos tests (codigo, nombre, razon_social, descripcion).
    public const string Marcador = "IT_G70_";

    /// Nombre de la restricción temporal de T10. Se elimina en el finally del test y, si una ejecución
    /// previa quedó interrumpida, al iniciar/terminar el fixture.
    public const string NombreCkFalloSimulado = "CK_TEST_T10_ROLLBACK";

    public static readonly TimeSpan TiempoPorTest = TimeSpan.FromSeconds(30);

    private const string PatronLike = @"IT\_G70\_%";

    private string? _cadena;

    public string Cadena => _cadena ?? throw new InvalidOperationException("La integración no está activada.");

    public async Task InitializeAsync()
    {
        if (Environment.GetEnvironmentVariable(FactIntegracionAttribute.VariableActivacion) != "1"
            || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(FactIntegracionAttribute.VariableConexion)))
        {
            return;
        }

        _cadena = GuardaConexion.ObtenerCadenaValidada();
        using var cts = new CancellationTokenSource(TiempoPorTest);

        await GuardaConexion.VerificarBaseConectadaAsync(_cadena, cts.Token);

        await using (AppDbContext db = CrearContexto())
        {
            List<string> pendientes = (await db.Database.GetPendingMigrationsAsync(cts.Token)).ToList();
            if (pendientes.Count > 0)
            {
                throw new InvalidOperationException(
                    $"La BD de pruebas tiene {pendientes.Count} migración(es) pendiente(s). Restaure el esquema; los tests no migran.");
            }
        }

        await LimpiarAsync(cts.Token); // residuos de una ejecución interrumpida
    }

    public async Task DisposeAsync()
    {
        if (_cadena is null)
        {
            return;
        }

        using var cts = new CancellationTokenSource(TiempoPorTest);
        await LimpiarAsync(cts.Token);
    }

    /// Opciones del contexto real. Si se pasan interceptores se fija MaxBatchSize(1) para que cada
    /// UPDATE de EF sea un comando propio y el interceptor pueda registrarlo uno a uno
    /// (solo afecta a los contextos con interceptores).
    public DbContextOptions<AppDbContext> CrearOpciones(params IInterceptor[] interceptores)
    {
        bool conInterceptores = interceptores is { Length: > 0 };

        var constructor = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(Cadena, o =>
            {
                o.CommandTimeout(30);
                if (conInterceptores)
                {
                    o.MaxBatchSize(1);
                }
            });

        if (conInterceptores)
        {
            constructor.AddInterceptors(interceptores!);
        }

        return constructor.Options;
    }

    public AppDbContext CrearContexto() => new(CrearOpciones());

    // ---------- Limpieza: solo filas con marcador propio + CHECK residual de T10 ----------

    private async Task LimpiarAsync(CancellationToken ct)
    {
        // 1) CHECK residual de una ejecución interrumpida de T10 (para que su precondición no quede atascada).
        await EjecutarAsync(
            $"IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'{NombreCkFalloSimulado}' " +
            "AND parent_object_id = OBJECT_ID(N'dbo.PRODUCTO')) " +
            $"ALTER TABLE dbo.PRODUCTO DROP CONSTRAINT {NombreCkFalloSimulado};", ct);

        // 2) DELETE por orden FK, solo filas con el prefijo IT_G70_.
        const string sql = @"
SET XACT_ABORT ON;
SET LOCK_TIMEOUT 30000;
BEGIN TRAN;
DECLARE @provs TABLE (id INT);  DECLARE @usus TABLE (id INT);  DECLARE @prods TABLE (id INT);
INSERT @provs SELECT id_proveedor FROM dbo.PROVEEDOR WHERE razon_social LIKE @m ESCAPE '\';
INSERT @usus  SELECT id_usuario   FROM dbo.USUARIO   WHERE nombre       LIKE @m ESCAPE '\';
INSERT @prods SELECT id_producto  FROM dbo.PRODUCTO  WHERE codigo       LIKE @m ESCAPE '\';
DELETE FROM dbo.DETALLE_COMPRA
 WHERE id_producto IN (SELECT id FROM @prods)
    OR id_compra IN (SELECT id_compra FROM dbo.COMPRA WHERE id_proveedor IN (SELECT id FROM @provs) OR id_usuario IN (SELECT id FROM @usus));
DELETE FROM dbo.COMPRA WHERE id_proveedor IN (SELECT id FROM @provs) OR id_usuario IN (SELECT id FROM @usus);
DELETE FROM dbo.PRODUCTO   WHERE id_producto  IN (SELECT id FROM @prods);
DELETE FROM dbo.PROVEEDOR  WHERE id_proveedor IN (SELECT id FROM @provs);
DELETE FROM dbo.USUARIO    WHERE id_usuario   IN (SELECT id FROM @usus);
DELETE FROM dbo.CATEGORIA  WHERE descripcion LIKE @m ESCAPE '\';
COMMIT;";

        await using var cn = new SqlConnection(Cadena);
        await cn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, cn) { CommandTimeout = 30 };
        cmd.Parameters.AddWithValue("@m", PatronLike);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    // ---------- Datos sintéticos ----------

    private static string Sufijo() => Guid.NewGuid().ToString("N")[..6];

    public async Task<Escenario> CrearEscenarioAsync(CancellationToken ct)
    {
        await using AppDbContext db = CrearContexto();
        string s = Sufijo();

        int idRol = await db.Roles.AsNoTracking().OrderBy(r => r.IdRol).Select(r => r.IdRol).FirstAsync(ct);

        var categoria = new Categoria { Descripcion = Marcador + "Cat_" + s, Estado = true };
        var proveedor = new Proveedor
        {
            Cuit = Marcador + s,                       // varchar(13): 7 + 6
            RazonSocial = Marcador + "Proveedor_" + s,
            Estado = true
        };
        var usuario = new Usuario
        {
            IdRol = idRol,
            Dni = Random.Shared.Next(900_000_000, 2_000_000_000),
            Nombre = Marcador + "Usuario",
            Apellido = "IT",
            Sexo = "F",
            Clave = "no-es-un-hash",
            Estado = true
        };

        db.Categorias.Add(categoria);
        db.Proveedores.Add(proveedor);
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(ct);

        return new Escenario(usuario.IdUsuario, proveedor.IdProveedor, categoria.IdCategoria);
    }

    public async Task<Producto> CrearProductoAsync(
        Escenario escenario, int stock, decimal precioCompra, decimal precioVenta, CancellationToken ct)
    {
        await using AppDbContext db = CrearContexto();
        string codigo = Marcador + Sufijo();           // nvarchar(13): 7 + 6

        var producto = new Producto
        {
            IdCategoria = escenario.IdCategoria,
            Codigo = codigo,
            Nombre = Marcador + "Producto_" + codigo,
            Stock = stock,
            PrecioCompra = precioCompra,
            PrecioVenta = precioVenta,
            Estado = true,
            IdUsuario = escenario.IdUsuario
        };

        db.Productos.Add(producto);
        await db.SaveChangesAsync(ct);
        return producto;
    }

    // ---------- Lecturas del estado real ----------

    public async Task<Producto> LeerProductoAsync(int idProducto, CancellationToken ct)
    {
        await using AppDbContext db = CrearContexto();
        return await db.Productos.AsNoTracking().SingleAsync(p => p.IdProducto == idProducto, ct);
    }

    public async Task<int> ContarComprasAsync(Escenario escenario, CancellationToken ct)
    {
        await using AppDbContext db = CrearContexto();
        return await db.Compras.AsNoTracking().CountAsync(c => c.IdProveedor == escenario.IdProveedor, ct);
    }

    public async Task<int> ContarDetallesAsync(int[] idsProducto, CancellationToken ct)
    {
        await using AppDbContext db = CrearContexto();
        return await db.DetallesCompra.AsNoTracking().CountAsync(d => idsProducto.Contains(d.IdProducto), ct);
    }

    // ---------- Alteraciones externas (otro contexto/conexión) ----------

    public async Task DesactivarProductoAsync(int idProducto, CancellationToken ct)
    {
        await using AppDbContext db = CrearContexto();
        await db.Productos.Where(p => p.IdProducto == idProducto)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Estado, false), ct);
    }

    public async Task EstablecerStockAsync(int idProducto, int stock, CancellationToken ct)
    {
        await using AppDbContext db = CrearContexto();
        await db.Productos.Where(p => p.IdProducto == idProducto)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, stock), ct);
    }

    // ---------- SQL literal de los tests (metadatos, conteos y DDL temporal de T10) ----------

    public async Task<List<object?[]>> ConsultarAsync(string sql, CancellationToken ct)
    {
        await using var cn = new SqlConnection(Cadena);
        await cn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, cn) { CommandTimeout = 30 };
        await using SqlDataReader lector = await cmd.ExecuteReaderAsync(ct);

        var filas = new List<object?[]>();
        while (await lector.ReadAsync(ct))
        {
            var fila = new object?[lector.FieldCount];
            for (int i = 0; i < fila.Length; i++)
            {
                fila[i] = await lector.IsDBNullAsync(i, ct) ? null : lector.GetValue(i);
            }

            filas.Add(fila);
        }

        return filas;
    }

    public async Task<int> EscalarAsync(string sql, CancellationToken ct)
        => Convert.ToInt32((await ConsultarAsync(sql, ct))[0][0]);

    /// Solo para DDL temporal de T10 y limpieza; el destino ya fue validado por la guarda.
    public async Task EjecutarAsync(string sql, CancellationToken ct)
    {
        await using var cn = new SqlConnection(Cadena);
        await cn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, cn) { CommandTimeout = 30 };
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
