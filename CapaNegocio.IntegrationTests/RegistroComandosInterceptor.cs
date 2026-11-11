using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CapaNegocio.IntegrationTests;

/// Registra, en orden, el resultado de cada UPDATE sobre PRODUCTO emitido por el contexto:
///   "OK:n"   comando ejecutado sin error (n = filas afectadas).
///   "ERR:nnn" comando fallido con SqlException.Number = nnn (547 = violación de CHECK/FK).
/// Cubre las dos formas de escribir el hook: ExecuteUpdate (NonQuery: n real = filas afectadas) y
/// UPDATE por seguimiento de cambios (Reader con OUTPUT/SELECT por el trigger: n = 1 si el lector
/// devuelve fila, 0 si no; el WHERE es por clave, así que 0 o 1).
public sealed class RegistroComandosInterceptor : DbCommandInterceptor
{
    private static readonly Regex Update = new(@"\bUPDATE\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private readonly object _candado = new();
    private readonly List<string> _registro = new();

    public IReadOnlyList<string> UpdatesProducto
    {
        get
        {
            lock (_candado)
            {
                return _registro.ToArray();
            }
        }
    }

    private static bool EsUpdateDeProducto(DbCommand comando)
        => comando.CommandText.Contains("[PRODUCTO]", StringComparison.OrdinalIgnoreCase)
           && Update.IsMatch(comando.CommandText);

    private void Anotar(string entrada)
    {
        lock (_candado)
        {
            _registro.Add(entrada);
        }
    }

    // ---------- Reader ----------

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        if (EsUpdateDeProducto(command))
        {
            Anotar(result.HasRows ? "OK:1" : "OK:0");
        }

        return result;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
    {
        if (EsUpdateDeProducto(command))
        {
            Anotar(result.HasRows ? "OK:1" : "OK:0");
        }

        return ValueTask.FromResult(result);
    }

    // ---------- NonQuery (ExecuteUpdate) ----------

    public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
    {
        if (EsUpdateDeProducto(command))
        {
            Anotar("OK:" + result.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        return result;
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (EsUpdateDeProducto(command))
        {
            Anotar("OK:" + result.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        return ValueTask.FromResult(result);
    }

    // ---------- Fallos ----------

    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    {
        if (EsUpdateDeProducto(command))
        {
            Anotar(Describir(eventData.Exception));
        }
    }

    public override Task CommandFailedAsync(
        DbCommand command, CommandErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        if (EsUpdateDeProducto(command))
        {
            Anotar(Describir(eventData.Exception));
        }

        return Task.CompletedTask;
    }

    private static string Describir(Exception excepcion)
        => excepcion is SqlException sql
            ? "ERR:" + sql.Number.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : "ERR:" + excepcion.GetType().Name;
}
