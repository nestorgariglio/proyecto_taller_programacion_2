using CapaDatos;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CapaNegocio.IntegrationTests;

/// <summary>
/// Fábrica para pruebas de integración sin residuos: todos los contextos comparten
/// UNA conexión y UNA transacción sobre la BD de pruebas, que se revierte al
/// liberar la fábrica. La capa de negocio detecta la transacción ambiente y
/// participa en ella (no abre otra).
/// </summary>
internal sealed class FabricaSqlServerTransaccional : IDbContextFactory<AppDbContext>, IAsyncDisposable
{
    private readonly SqlConnection _conexion;
    private readonly SqlTransaction _transaccion;
    private readonly DbContextOptions<AppDbContext> _opciones;
    private bool _liberada;

    private FabricaSqlServerTransaccional(SqlConnection conexion, SqlTransaction transaccion)
    {
        _conexion = conexion;
        _transaccion = transaccion;
        _opciones = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(conexion).Options;
    }

    public static async Task<FabricaSqlServerTransaccional> CrearAsync()
    {
        var conexion = new SqlConnection(Environment.GetEnvironmentVariable(BaseSqlServerPruebas.VariableDeEntorno));
        await conexion.OpenAsync();
        SqlTransaction transaccion = (SqlTransaction)await conexion.BeginTransactionAsync();
        return new FabricaSqlServerTransaccional(conexion, transaccion);
    }

    public AppDbContext CreateDbContext()
    {
        var db = new AppDbContext(_opciones);
        db.Database.UseTransaction(_transaccion);
        return db;
    }

    /// <summary>Inserta un producto en la transacción compartida (esquema real de la BD de pruebas).</summary>
    public async Task InsertarProductoAsync(int idCategoria, bool activo)
    {
        await using SqlCommand comando = ProductoSql.CrearInsert(_conexion, _transaccion, idCategoria, activo);
        await comando.ExecuteNonQueryAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_liberada)
        {
            return;
        }

        _liberada = true;

        try
        {
            await _transaccion.RollbackAsync();
        }
        finally
        {
            await _transaccion.DisposeAsync();
            await _conexion.DisposeAsync();
        }
    }
}

/// <summary>SQL directo para productos: el esquema real exige columnas que el modelo EF no expone.</summary>
internal static class ProductoSql
{
    public static SqlCommand CrearInsert(
        SqlConnection conexion, SqlTransaction transaccion, int idCategoria, bool activo)
    {
        var comando = new SqlCommand(
            "INSERT INTO PRODUCTO (id_categoria, nombre, stock, precio_compra, precio_venta, estado, fecha_registro, id_usuario) "
            + "VALUES (@c, @n, 0, 0, 1, @e, SYSDATETIME(), (SELECT TOP 1 id_usuario FROM USUARIO ORDER BY id_usuario))",
            conexion,
            transaccion);
        comando.Parameters.AddWithValue("@c", idCategoria);
        comando.Parameters.AddWithValue("@n", $"ITEST-P-{Guid.NewGuid():N}"[..20]);
        comando.Parameters.AddWithValue("@e", activo);
        return comando;
    }
}
