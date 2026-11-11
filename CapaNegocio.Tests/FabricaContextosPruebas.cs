using CapaDatos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;

namespace CapaNegocio.Tests;

/// <summary>
/// Fábrica de contextos para pruebas: cada llamada crea un contexto nuevo sobre
/// la MISMA base InMemory (mismo nombre de base = mismos datos compartidos).
/// </summary>
internal sealed class FabricaContextosPruebas : IDbContextFactory<AppDbContext>
{
    private readonly DbContextOptions<AppDbContext> _options;

    public FabricaContextosPruebas(DbContextOptions<AppDbContext> options) => _options = options;

    /// <summary>
    /// Crea una fábrica que apunta a la misma base InMemory que <paramref name="db"/>.
    /// </summary>
    public static FabricaContextosPruebas Desde(AppDbContext db) => new(OpcionesDe(db));

    /// <summary>Opciones InMemory apuntando a la misma base que <paramref name="db"/>.</summary>
    public static DbContextOptions<AppDbContext> OpcionesDe(AppDbContext db)
    {
        string nombreBase = db.GetService<IDbContextOptions>()
            .FindExtension<InMemoryOptionsExtension>()!.StoreName;

        return new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(nombreBase).Options;
    }

    public AppDbContext CreateDbContext() => new(_options);
}
