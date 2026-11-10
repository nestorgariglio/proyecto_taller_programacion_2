using CapaEntidad;
using Microsoft.EntityFrameworkCore;

namespace CapaDatos;

public sealed class ConsultaProductosVentaDatos(DbContextOptions<AppDbContext> opciones)
{
    public async Task<List<Producto>> BuscarActivosAsync(string busqueda)
    {
        await using var db = new AppDbContext(opciones);
        return await db.Productos
            .AsNoTracking()
            .Where(p => p.Estado && (p.Codigo == busqueda ||
                (p.Nombre != null && p.Nombre.ToUpper().Contains(busqueda.ToUpper()))))
            .OrderBy(p => p.Nombre)
            .ThenBy(p => p.IdProducto)
            .ToListAsync();
    }

    public async Task<Producto?> ObtenerActivoAsync(int idProducto)
    {
        await using var db = new AppDbContext(opciones);
        return await db.Productos.AsNoTracking()
            .SingleOrDefaultAsync(p => p.Estado && p.IdProducto == idProducto);
    }
}
