using CapaNegocio.DTOs.Ventas;

namespace CapaPresentacion.Ventas;

/// <summary>Mantiene el carrito por vista y ordena operaciones solicitadas desde el hilo de la interfaz.</summary>
internal class EstadoCarritoVenta
{
    private Task _ultimaOperacion = Task.CompletedTask;
    public CarritoVentaDto Carrito { get; private set; } = new();

    /// <summary>Ejecuta una operación en su turno y conserva el carrito si se rechaza.</summary>
    /// <param name="operacion">Operación que recibe el carrito vigente al comenzar su turno.</param>
    /// <returns>Resultado de la operación.</returns>
    public async Task<ResultadoCarritoDto> EjecutarAsync(Func<CarritoVentaDto, Task<ResultadoCarritoDto>> operacion)
    {
        var anterior = _ultimaOperacion;
        var terminada = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _ultimaOperacion = terminada.Task;
        await anterior;
        try
        {
            var resultado = await operacion(Carrito);
            if (resultado.Exito) Carrito = resultado.Carrito;
            return resultado;
        }
        finally
        {
            terminada.SetResult();
        }
    }
}
