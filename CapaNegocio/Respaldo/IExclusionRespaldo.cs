namespace CapaNegocio.Respaldo;

/// <summary>
/// Exclusión mutua dentro del proceso para las operaciones de respaldo
/// (generar/restaurar). Protege solo dentro del proceso; entre instancias de G70
/// coordina <c>sp_getapplock</c>. No se mantiene durante diálogos de confirmación.
/// </summary>
public interface IExclusionRespaldo
{
    /// <summary>
    /// Intenta entrar sin esperar. Devuelve un liberador, o <see langword="null"/>
    /// si ya hay otra operación en curso.
    /// </summary>
    Task<IDisposable?> IntentarEntrarAsync(CancellationToken ct);
}
