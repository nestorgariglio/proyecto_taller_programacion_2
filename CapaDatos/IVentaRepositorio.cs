using System.Threading;
using System.Threading.Tasks;

namespace CapaDatos;

/// <summary>
/// Contrato de persistencia transaccional de ventas mediante procedimiento almacenado.
/// </summary>
public interface IVentaRepositorio
{
    /// <summary>
    /// Registra la venta y sus detalles de forma atómica en el motor de base de datos.
    /// </summary>
    /// <param name="parametros">Datos de cabecera y líneas de venta.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resultado de la operación con identificador generado o mensaje de error.</returns>
    Task<ResultadoVentaDatos> RegistrarVentaAsync(
        ParametrosRegistroVentaDatos parametros,
        CancellationToken ct = default);
}
