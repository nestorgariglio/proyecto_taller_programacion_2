using CapaNegocio.DTOs.Ventas;

namespace CapaPresentacion.Ventas;

/// <summary>Coordina consultas iniciadas desde el hilo de la interfaz.</summary>
internal class BusquedaProductosVenta(Func<string, Task<IReadOnlyList<ProductoVentaDto>>> consultar)
{
    private int _version;
    public int Version => _version;

    public void Invalidar() => _version++;

    /// <summary>Consulta productos y descarta respuestas que dejaron de ser vigentes.</summary>
    /// <param name="texto">Código o nombre que se desea buscar.</param>
    /// <returns>Coincidencias vigentes, o null si la consulta quedó obsoleta.</returns>
    public async Task<IReadOnlyList<ProductoVentaDto>?> BuscarAsync(string texto)
    {
        int version = ++_version;
        try
        {
            var productos = await consultar(texto);
            return version == _version ? productos : null;
        }
        catch when (version != _version)
        {
            return null;
        }
    }
}
