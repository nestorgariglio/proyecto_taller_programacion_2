using CapaDatos;
using Microsoft.EntityFrameworkCore;

namespace CapaNegocio.IntegrationTests;

/// Contexto real que, justo después del primer SaveChangesAsync (cabecera y líneas, o sea entre la
/// validación y el hook de HU-10), ejecuta una acción externa. Simula un cambio concurrente sin tocar negocio.
internal sealed class ContextoConIntercepcion : AppDbContext
{
    private readonly Func<Task> _trasPrimerGuardado;
    private bool _disparado;

    public ContextoConIntercepcion(DbContextOptions<AppDbContext> options, Func<Task> trasPrimerGuardado)
        : base(options)
    {
        _trasPrimerGuardado = trasPrimerGuardado;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int filas = await base.SaveChangesAsync(cancellationToken);

        if (!_disparado)
        {
            _disparado = true;
            await _trasPrimerGuardado();
        }

        return filas;
    }
}
