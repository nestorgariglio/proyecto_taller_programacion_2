using CapaDatos;
using CapaEntidad;
using CapaNegocio.DTOs.Ventas;

namespace CapaNegocio;

public class CarritoVentaNegocio(ConsultaProductosVentaDatos consultaProductos)
{
    public ResultadoCarritoDto QuitarProducto(CarritoVentaDto carrito, int idProducto)
    {
        if (!carrito.Lineas.Any(linea => linea.IdProducto == idProducto))
            return new(false, "El producto no está en el carrito.", carrito);

        var lineas = carrito.Lineas.Where(linea => linea.IdProducto != idProducto);
        return new(true, null, new CarritoVentaDto(lineas));
    }

    public async Task<ResultadoCarritoDto> CambiarCantidadAsync(
        CarritoVentaDto carrito, int idProducto, int cantidad)
    {
        if (cantidad <= 0)
            return new(false, "La cantidad debe ser mayor a cero.", carrito);

        var anterior = carrito.Lineas.SingleOrDefault(linea => linea.IdProducto == idProducto);
        if (anterior is null)
            return new(false, "El producto no está en el carrito.", carrito);

        if (cantidad == anterior.Cantidad)
            return new(true, null, carrito);

        if (cantidad > anterior.Cantidad)
            return await AgregarProductoAsync(carrito, idProducto, cantidad - anterior.Cantidad);

        var lineas = carrito.Lineas.Select(linea => linea.IdProducto == idProducto
            ? linea with { Cantidad = cantidad }
            : linea);
        return new(true, null, new CarritoVentaDto(lineas));
    }

    public async Task<ResultadoCarritoDto> AgregarProductoAsync(
        CarritoVentaDto carrito, int idProducto, int cantidad)
    {
        if (cantidad <= 0)
            return new(false, "La cantidad debe ser mayor a cero.", carrito);

        var producto = await ObtenerProductoAsync(idProducto);
        if (producto is null)
            return new(false, "El producto no existe o está inactivo.", carrito);

        var anterior = carrito.Lineas.SingleOrDefault(linea => linea.IdProducto == idProducto);
        long cantidadTotal = (long)(anterior?.Cantidad ?? 0) + cantidad;
        if (cantidadTotal > producto.Stock)
        {
            long disponible = Math.Max(0L, (long)producto.Stock - (anterior?.Cantidad ?? 0));
            return new(false, $"Stock insuficiente. Disponible: {disponible}", carrito);
        }

        var nuevaLinea = new LineaCarritoDto
        {
            IdProducto = producto.IdProducto,
            Codigo = producto.Codigo,
            Nombre = producto.Nombre,
            Cantidad = (int)cantidadTotal,
            PrecioUnitario = producto.PrecioVenta
        };
        var lineas = anterior is null
            ? carrito.Lineas.Append(nuevaLinea)
            : carrito.Lineas.Select(linea => linea.IdProducto == idProducto ? nuevaLinea : linea);

        return new(true, null, new CarritoVentaDto(lineas));
    }

    public async Task<IReadOnlyList<ProductoVentaDto>> BuscarProductosAsync(string? busqueda)
    {
        if (string.IsNullOrWhiteSpace(busqueda))
            return Array.Empty<ProductoVentaDto>();

        var productos = await consultaProductos.BuscarActivosAsync(busqueda.Trim());
        return productos.Select(Mapear).ToArray();
    }

    public async Task<ProductoVentaDto?> ObtenerProductoAsync(int idProducto)
    {
        if (idProducto <= 0)
            return null;

        var producto = await consultaProductos.ObtenerActivoAsync(idProducto);
        return producto is null ? null : Mapear(producto);
    }

    private static ProductoVentaDto Mapear(Producto producto) => new()
    {
        IdProducto = producto.IdProducto,
        Codigo = producto.Codigo,
        Nombre = producto.Nombre,
        Stock = producto.Stock,
        PrecioVenta = producto.PrecioVenta
    };
}
