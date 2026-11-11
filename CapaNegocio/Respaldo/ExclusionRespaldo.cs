namespace CapaNegocio.Respaldo;

/// <summary>Implementación Singleton con <see cref="SemaphoreSlim"/>(1,1) y <c>WaitAsync(0)</c>.</summary>
public sealed class ExclusionRespaldo : IExclusionRespaldo
{
    private readonly SemaphoreSlim _semaforo = new(1, 1);

    /// <inheritdoc />
    public async Task<IDisposable?> IntentarEntrarAsync(CancellationToken ct)
    {
        if (!await _semaforo.WaitAsync(0, ct))
        {
            return null;
        }

        return new Liberador(_semaforo);
    }

    private sealed class Liberador : IDisposable
    {
        private SemaphoreSlim? _semaforo;

        public Liberador(SemaphoreSlim semaforo) => _semaforo = semaforo;

        public void Dispose() => Interlocked.Exchange(ref _semaforo, null)?.Release();
    }
}
