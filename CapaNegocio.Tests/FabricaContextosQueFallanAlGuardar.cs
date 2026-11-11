using CapaDatos;
using Microsoft.EntityFrameworkCore;

namespace CapaNegocio.Tests;

/// <summary>
/// Fábrica de apoyo: el PRIMER contexto que entrega lanza <see cref="DbUpdateException"/>
/// al guardar (simula la violación del índice único por carrera); los siguientes son normales.
/// </summary>
internal sealed class FabricaContextosQueFallanAlGuardar : IDbContextFactory<AppDbContext>
{
    private readonly FabricaContextosPruebas _interna;
    private readonly Func<AppDbContext, Task>? _antesDeFallar;
    private int _creados;

    public FabricaContextosQueFallanAlGuardar(
        FabricaContextosPruebas interna, Func<AppDbContext, Task>? antesDeFallar)
    {
        _interna = interna;
        _antesDeFallar = antesDeFallar;
    }

    public AppDbContext CreateDbContext()
    {
        AppDbContext normal = _interna.CreateDbContext();
        return Interlocked.Increment(ref _creados) == 1
            ? new ContextoQueFalla(normal, _interna, _antesDeFallar)
            : normal;
    }

    private sealed class ContextoQueFalla : AppDbContext
    {
        private readonly FabricaContextosPruebas _fabrica;
        private readonly Func<AppDbContext, Task>? _antesDeFallar;

        public ContextoQueFalla(AppDbContext modelo, FabricaContextosPruebas fabrica, Func<AppDbContext, Task>? antesDeFallar)
            : base(ObtenerOpciones(modelo))
        {
            _fabrica = fabrica;
            _antesDeFallar = antesDeFallar;
        }

        private static DbContextOptions<AppDbContext> ObtenerOpciones(AppDbContext modelo)
        {
            using (modelo) { return FabricaContextosPruebas.OpcionesDe(modelo); }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (_antesDeFallar is not null)
            {
                await using AppDbContext otro = _fabrica.CreateDbContext();
                await _antesDeFallar(otro);
            }

            throw new DbUpdateException("Violación simulada del índice único.");
        }
    }
}
