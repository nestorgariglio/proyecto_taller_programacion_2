using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CapaDatos;
using CapaNegocio.DTOs.Compras;
using CapaNegocio.Enums;
using Microsoft.EntityFrameworkCore;

namespace CapaNegocio;

public partial class CompraNegocio
{
    /// <summary>
    /// Búsqueda rápida de proveedores activos por coincidencia parcial de CUIT o razón social.
    /// </summary>
    public async Task<RespuestaBusquedaProveedoresCompra> BuscarProveedoresAsync(string? textoBusqueda)
    {
        string? texto = textoBusqueda?.Trim();

        if (string.IsNullOrEmpty(texto))
        {
            return new RespuestaBusquedaProveedoresCompra
            {
                Resultado = ResultadoBusquedaCompras.SinResultados,
                Mensaje = string.Empty
            };
        }

        try
        {
            string patron = texto.ToUpperInvariant();

            List<ProveedorSeleccionadoDto> proveedores = await _context.Proveedores
                .AsNoTracking()
                .Where(p => p.Estado)
                .Where(p => (p.Cuit != null && p.Cuit.ToUpper().Contains(patron))
                    || (p.RazonSocial != null && p.RazonSocial.ToUpper().Contains(patron)))
                .OrderBy(p => p.RazonSocial)
                .ThenBy(p => p.IdProveedor)
                .Take(20)
                .Select(p => new ProveedorSeleccionadoDto
                {
                    IdProveedor = p.IdProveedor,
                    RazonSocial = p.RazonSocial ?? string.Empty,
                    Documento = p.Cuit ?? string.Empty
                })
                .ToListAsync();

            if (proveedores.Count == 0)
            {
                return new RespuestaBusquedaProveedoresCompra
                {
                    Resultado = ResultadoBusquedaCompras.SinResultados,
                    Mensaje = MensajeSinProveedoresCoincidentes
                };
            }

            return new RespuestaBusquedaProveedoresCompra
            {
                Resultado = ResultadoBusquedaCompras.ConResultados,
                Proveedores = proveedores,
                Mensaje = proveedores.Count == 1
                    ? MensajeUnProveedorEncontrado
                    : string.Format(CultureInfo.InvariantCulture, MensajeProveedoresEncontrados, proveedores.Count)
            };
        }
        catch (Exception ex) when (EsFalloDeOrigenDeDatos(ex))
        {
            return new RespuestaBusquedaProveedoresCompra
            {
                Resultado = ResultadoBusquedaCompras.ErrorPersistencia,
                Mensaje = MensajeErrorBuscarProveedores,
                Proveedores = Array.Empty<ProveedorSeleccionadoDto>()
            };
        }
    }

    /// <summary>
    /// Búsqueda rápida de productos activos por coincidencia parcial de código o nombre.
    /// </summary>
    public async Task<RespuestaBusquedaProductosCompra> BuscarProductosAsync(string? textoBusqueda)
    {
        string? texto = textoBusqueda?.Trim();

        if (string.IsNullOrEmpty(texto))
        {
            return new RespuestaBusquedaProductosCompra
            {
                Resultado = ResultadoBusquedaCompras.SinResultados,
                Mensaje = string.Empty
            };
        }

        try
        {
            string patron = texto.ToUpperInvariant();

            List<ProductoBuscadoDto> productos = await _context.Productos
                .AsNoTracking()
                .Where(p => p.Estado)
                .Where(p => (p.Codigo != null && p.Codigo.ToUpper().Contains(patron))
                    || (p.Nombre != null && p.Nombre.ToUpper().Contains(patron)))
                .OrderBy(p => p.Nombre)
                .ThenBy(p => p.IdProducto)
                .Take(20)
                .Select(p => new ProductoBuscadoDto
                {
                    IdProducto = p.IdProducto,
                    Codigo = p.Codigo ?? string.Empty,
                    Nombre = p.Nombre ?? string.Empty,
                    Stock = p.Stock,
                    StockMinimo = p.StockMinimo
                })
                .ToListAsync();

            if (productos.Count == 0)
            {
                return new RespuestaBusquedaProductosCompra
                {
                    Resultado = ResultadoBusquedaCompras.SinResultados,
                    Mensaje = MensajeSinProductosCoincidentes
                };
            }

            return new RespuestaBusquedaProductosCompra
            {
                Resultado = ResultadoBusquedaCompras.ConResultados,
                Productos = productos,
                Mensaje = productos.Count == 1
                    ? MensajeUnProductoEncontrado
                    : string.Format(CultureInfo.InvariantCulture, MensajeProductosEncontrados, productos.Count)
            };
        }
        catch (Exception ex) when (EsFalloDeOrigenDeDatos(ex))
        {
            return new RespuestaBusquedaProductosCompra
            {
                Resultado = ResultadoBusquedaCompras.ErrorPersistencia,
                Mensaje = MensajeErrorBuscarProductos,
                Productos = Array.Empty<ProductoBuscadoDto>()
            };
        }
    }

    /// <summary>
    /// Búsqueda contextual de proveedores para compra priorizando historial y relaciones.
    /// </summary>
    public async Task<RespuestaBusquedaProveedoresCompra> BuscarProveedoresParaCompraAsync(
        string? textoBusqueda, int? idProducto, bool mostrarTodos, string? rol)
    {
        string? texto = textoBusqueda?.Trim();
        bool hayTexto = !string.IsNullOrEmpty(texto);
        bool conContexto = idProducto.HasValue && idProducto.Value > 0;

        if (conContexto && !PoliticaAcceso.PuedeOperarCompras(rol))
        {
            return new RespuestaBusquedaProveedoresCompra
            {
                Resultado = ResultadoBusquedaCompras.ErrorPersistencia,
                Mensaje = MensajePermisoConsultarCompras
            };
        }

        if (!conContexto && !mostrarTodos && !hayTexto)
        {
            return new RespuestaBusquedaProveedoresCompra
            {
                Resultado = ResultadoBusquedaCompras.SinResultados,
                Mensaje = string.Empty
            };
        }

        try
        {
            string patron = hayTexto ? texto!.ToUpperInvariant() : string.Empty;
            List<ProveedorSeleccionadoDto> resultado = await EjecutarLecturaAsync(async db =>
            {
                List<ProveedorSeleccionadoDto> lista = conContexto
                    ? await ObtenerProveedoresHistoricosAsync(db, idProducto!.Value, hayTexto, patron)
                    : new List<ProveedorSeleccionadoDto>();

                if (!conContexto || mostrarTodos)
                {
                    int[] idsHistoricos = lista.Select(p => p.IdProveedor).ToArray();
                    lista.AddRange(
                        await ObtenerProveedoresNoHistoricosAsync(db, idsHistoricos, hayTexto, patron));
                }

                return lista;
            });

            if (resultado.Count == 0)
            {
                return new RespuestaBusquedaProveedoresCompra
                {
                    Resultado = ResultadoBusquedaCompras.SinResultados,
                    Mensaje = conContexto && !mostrarTodos && !hayTexto
                        ? MensajeSinProveedoresHistoricos
                        : MensajeSinProveedoresCoincidentes
                };
            }

            return new RespuestaBusquedaProveedoresCompra
            {
                Resultado = ResultadoBusquedaCompras.ConResultados,
                Proveedores = resultado,
                Mensaje = resultado.Count == 1
                    ? MensajeUnProveedorEncontrado
                    : string.Format(CultureInfo.InvariantCulture, MensajeProveedoresEncontrados, resultado.Count)
            };
        }
        catch (Exception ex) when (EsFalloDeOrigenDeDatos(ex))
        {
            return new RespuestaBusquedaProveedoresCompra
            {
                Resultado = ResultadoBusquedaCompras.ErrorPersistencia,
                Mensaje = MensajeErrorBuscarProveedores,
                Proveedores = Array.Empty<ProveedorSeleccionadoDto>()
            };
        }
    }

    /// <summary>
    /// Búsqueda contextual de productos para compra asociando catálogo del proveedor e historial.
    /// </summary>
    public async Task<RespuestaBusquedaProductosCompra> BuscarProductosParaCompraAsync(
        string? textoBusqueda, int? idProveedor, bool mostrarTodos, string? rol)
    {
        string? texto = textoBusqueda?.Trim();
        bool hayTexto = !string.IsNullOrEmpty(texto);
        bool conContexto = idProveedor.HasValue && idProveedor.Value > 0;

        if (conContexto && !PoliticaAcceso.PuedeOperarCompras(rol))
        {
            return new RespuestaBusquedaProductosCompra
            {
                Resultado = ResultadoBusquedaCompras.ErrorPersistencia,
                Mensaje = MensajePermisoConsultarCompras
            };
        }

        if (!conContexto && !mostrarTodos && !hayTexto)
        {
            return new RespuestaBusquedaProductosCompra
            {
                Resultado = ResultadoBusquedaCompras.SinResultados,
                Mensaje = string.Empty
            };
        }

        try
        {
            string patron = hayTexto ? texto!.ToUpperInvariant() : string.Empty;
            List<ProductoBuscadoDto> resultado = await EjecutarLecturaAsync(async db =>
            {
                List<ProductoBuscadoDto> lista = conContexto
                    ? await ObtenerProductosHistoricosAsync(db, idProveedor!.Value, hayTexto, patron)
                    : new List<ProductoBuscadoDto>();

                if (!conContexto || mostrarTodos)
                {
                    int[] idsHistoricos = lista.Select(p => p.IdProducto).ToArray();
                    lista.AddRange(
                        await ObtenerProductosNoHistoricosAsync(db, idsHistoricos, hayTexto, patron));
                }

                return lista;
            });

            if (resultado.Count == 0)
            {
                return new RespuestaBusquedaProductosCompra
                {
                    Resultado = ResultadoBusquedaCompras.SinResultados,
                    Mensaje = conContexto && !mostrarTodos && !hayTexto
                        ? MensajeSinProductosHistoricos
                        : MensajeSinProductosCoincidentes
                };
            }

            return new RespuestaBusquedaProductosCompra
            {
                Resultado = ResultadoBusquedaCompras.ConResultados,
                Productos = resultado,
                Mensaje = resultado.Count == 1
                    ? MensajeUnProductoEncontrado
                    : string.Format(CultureInfo.InvariantCulture, MensajeProductosEncontrados, resultado.Count)
            };
        }
        catch (Exception ex) when (EsFalloDeOrigenDeDatos(ex))
        {
            return new RespuestaBusquedaProductosCompra
            {
                Resultado = ResultadoBusquedaCompras.ErrorPersistencia,
                Mensaje = MensajeErrorBuscarProductos,
                Productos = Array.Empty<ProductoBuscadoDto>()
            };
        }
    }

    /// <summary>
    /// Obtiene los precios actuales del catálogo para prellenar la línea en preparación.
    /// </summary>
    public async Task<RespuestaPreciosProducto> ObtenerPreciosProductoAsync(int idProducto)
    {
        try
        {
            var producto = await EjecutarLecturaAsync(db => db.Productos
                .AsNoTracking()
                .Where(p => p.IdProducto == idProducto)
                .Select(p => new { p.IdProducto, p.PrecioCompra, p.PrecioVenta, p.Estado })
                .FirstOrDefaultAsync());

            if (producto is null)
            {
                return new RespuestaPreciosProducto
                {
                    Resultado = ResultadoPreciosProducto.ProductoNoEncontrado,
                    Mensaje = MensajeProductoNoEncontrado
                };
            }

            if (!producto.Estado)
            {
                return new RespuestaPreciosProducto
                {
                    Resultado = ResultadoPreciosProducto.ProductoInactivo,
                    Mensaje = MensajeProductoInactivoAlPrellenar
                };
            }

            return new RespuestaPreciosProducto
            {
                Resultado = ResultadoPreciosProducto.Exito,
                Mensaje = MensajePreciosPropuestos,
                Precios = new PreciosProductoDto
                {
                    IdProducto = producto.IdProducto,
                    PrecioCompra = producto.PrecioCompra,
                    PrecioVenta = producto.PrecioVenta
                }
            };
        }
        catch (Exception ex) when (EsFalloDeOrigenDeDatos(ex))
        {
            return new RespuestaPreciosProducto
            {
                Resultado = ResultadoPreciosProducto.ErrorPersistencia,
                Mensaje = MensajeErrorPreciosProducto
            };
        }
    }

    /// <summary>
    /// Consulta los proveedores que ya vendieron un determinado producto.
    /// </summary>
    public async Task<RespuestaProveedoresDeProducto> ObtenerProveedoresDeProductoAsync(int idProducto, string? rol)
    {
        if (!PoliticaAcceso.PuedeOperarCompras(rol))
        {
            return new RespuestaProveedoresDeProducto
            {
                Resultado = ResultadoConsultaHistoricaCompra.PermisoDenegado,
                Mensaje = MensajePermisoConsultarCompras
            };
        }

        if (idProducto <= 0)
        {
            return new RespuestaProveedoresDeProducto
            {
                Resultado = ResultadoConsultaHistoricaCompra.SinResultados,
                Mensaje = string.Empty
            };
        }

        try
        {
            var filas = await (
                    from detalle in _context.DetallesCompra.AsNoTracking()
                    join compra in _context.Compras.AsNoTracking()
                        on detalle.IdCompra equals compra.IdCompra
                    join proveedor in _context.Proveedores.AsNoTracking()
                        on compra.IdProveedor equals proveedor.IdProveedor
                    where detalle.IdProducto == idProducto && proveedor.Estado
                    select new
                    {
                        proveedor.IdProveedor,
                        proveedor.RazonSocial,
                        proveedor.Cuit
                    })
                .Distinct()
                .OrderBy(p => p.RazonSocial)
                .ThenBy(p => p.IdProveedor)
                .ToListAsync();

            if (filas.Count == 0)
            {
                return new RespuestaProveedoresDeProducto
                {
                    Resultado = ResultadoConsultaHistoricaCompra.SinResultados,
                    Mensaje = MensajeSinProveedoresHistoricos
                };
            }

            return new RespuestaProveedoresDeProducto
            {
                Resultado = ResultadoConsultaHistoricaCompra.ConResultados,
                Mensaje = string.Empty,
                Proveedores = filas
                    .Select(f => new ProveedorDeProductoDto
                    {
                        IdProveedor = f.IdProveedor,
                        RazonSocial = f.RazonSocial ?? string.Empty,
                        Documento = f.Cuit ?? string.Empty
                    })
                    .ToList()
            };
        }
        catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
        {
            return new RespuestaProveedoresDeProducto
            {
                Resultado = ResultadoConsultaHistoricaCompra.ErrorPersistencia,
                Mensaje = MensajeErrorPersistencia
            };
        }
    }

    /// <summary>
    /// Consulta los productos que un determinado proveedor ya vendió al comercio.
    /// </summary>
    public async Task<RespuestaProductosDeProveedor> ObtenerProductosDeProveedorAsync(int idProveedor, string? rol)
    {
        if (!PoliticaAcceso.PuedeOperarCompras(rol))
        {
            return new RespuestaProductosDeProveedor
            {
                Resultado = ResultadoConsultaHistoricaCompra.PermisoDenegado,
                Mensaje = MensajePermisoConsultarCompras
            };
        }

        if (idProveedor <= 0)
        {
            return new RespuestaProductosDeProveedor
            {
                Resultado = ResultadoConsultaHistoricaCompra.SinResultados,
                Mensaje = string.Empty
            };
        }

        try
        {
            var filas = await (
                    from detalle in _context.DetallesCompra.AsNoTracking()
                    join compra in _context.Compras.AsNoTracking()
                        on detalle.IdCompra equals compra.IdCompra
                    join producto in _context.Productos.AsNoTracking()
                        on detalle.IdProducto equals producto.IdProducto
                    where compra.IdProveedor == idProveedor && producto.Estado
                    select new
                    {
                        producto.IdProducto,
                        producto.Codigo,
                        producto.Nombre
                    })
                .Distinct()
                .OrderBy(p => p.Nombre)
                .ThenBy(p => p.IdProducto)
                .ToListAsync();

            if (filas.Count == 0)
            {
                return new RespuestaProductosDeProveedor
                {
                    Resultado = ResultadoConsultaHistoricaCompra.SinResultados,
                    Mensaje = MensajeSinProductosHistoricos
                };
            }

            return new RespuestaProductosDeProveedor
            {
                Resultado = ResultadoConsultaHistoricaCompra.ConResultados,
                Mensaje = string.Empty,
                Productos = filas
                    .Select(f => new ProductoDeProveedorDto
                    {
                        IdProducto = f.IdProducto,
                        Codigo = f.Codigo ?? string.Empty,
                        Nombre = f.Nombre ?? string.Empty
                    })
                    .ToList()
            };
        }
        catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
        {
            return new RespuestaProductosDeProveedor
            {
                Resultado = ResultadoConsultaHistoricaCompra.ErrorPersistencia,
                Mensaje = MensajeErrorPersistencia
            };
        }
    }

    private async Task<List<ProveedorSeleccionadoDto>> ObtenerProveedoresHistoricosAsync(
        AppDbContext db, int idProducto, bool hayTexto, string patron)
    {
        var historicos = await (
                from detalle in db.DetallesCompra.AsNoTracking()
                join compra in db.Compras.AsNoTracking()
                    on detalle.IdCompra equals compra.IdCompra
                join proveedor in db.Proveedores.AsNoTracking()
                    on compra.IdProveedor equals proveedor.IdProveedor
                where detalle.IdProducto == idProducto
                    && proveedor.Estado
                    && (!hayTexto
                        || (proveedor.Cuit != null && proveedor.Cuit.ToUpper().Contains(patron))
                        || (proveedor.RazonSocial != null && proveedor.RazonSocial.ToUpper().Contains(patron)))
                group compra.FechaRegistro by new
                {
                    proveedor.IdProveedor,
                    proveedor.RazonSocial,
                    proveedor.Cuit
                } into grupo
                select new
                {
                    grupo.Key.IdProveedor,
                    grupo.Key.RazonSocial,
                    grupo.Key.Cuit,
                    UltimaCompra = grupo.Max()
                })
            .OrderByDescending(p => p.UltimaCompra)
            .ThenBy(p => p.RazonSocial)
            .ThenBy(p => p.IdProveedor)
            .ToListAsync();

        return historicos
            .Select(f => new ProveedorSeleccionadoDto
            {
                IdProveedor = f.IdProveedor,
                RazonSocial = f.RazonSocial ?? string.Empty,
                Documento = f.Cuit ?? string.Empty
            })
            .ToList();
    }

    private async Task<List<ProveedorSeleccionadoDto>> ObtenerProveedoresNoHistoricosAsync(
        AppDbContext db, int[] idsExcluidos, bool hayTexto, string patron)
    {
        return await db.Proveedores
            .AsNoTracking()
            .Where(p => p.Estado)
            .Where(p => !idsExcluidos.Contains(p.IdProveedor))
            .Where(p => !hayTexto
                || (p.Cuit != null && p.Cuit.ToUpper().Contains(patron))
                || (p.RazonSocial != null && p.RazonSocial.ToUpper().Contains(patron)))
            .OrderBy(p => p.RazonSocial)
            .ThenBy(p => p.IdProveedor)
            .Select(p => new ProveedorSeleccionadoDto
            {
                IdProveedor = p.IdProveedor,
                RazonSocial = p.RazonSocial ?? string.Empty,
                Documento = p.Cuit ?? string.Empty
            })
            .ToListAsync();
    }

    private async Task<List<ProductoBuscadoDto>> ObtenerProductosHistoricosAsync(
        AppDbContext db, int idProveedor, bool hayTexto, string patron)
    {
        var asociadosCatalogo = await (
                from pp in db.ProveedoresProductos.AsNoTracking()
                join producto in db.Productos.AsNoTracking()
                    on pp.IdProducto equals producto.IdProducto
                where pp.IdProveedor == idProveedor
                    && producto.Estado
                    && (!hayTexto
                        || (producto.Codigo != null && producto.Codigo.ToUpper().Contains(patron))
                        || (producto.Nombre != null && producto.Nombre.ToUpper().Contains(patron)))
                select new ProductoBuscadoDto
                {
                    IdProducto = producto.IdProducto,
                    Codigo = producto.Codigo ?? string.Empty,
                    Nombre = producto.Nombre ?? string.Empty,
                    Stock = producto.Stock,
                    StockMinimo = producto.StockMinimo
                })
            .ToListAsync();

        var idsAsociados = asociadosCatalogo.Select(a => a.IdProducto).ToHashSet();

        var historicos = await (
                from detalle in db.DetallesCompra.AsNoTracking()
                join compra in db.Compras.AsNoTracking()
                    on detalle.IdCompra equals compra.IdCompra
                join producto in db.Productos.AsNoTracking()
                    on detalle.IdProducto equals producto.IdProducto
                where compra.IdProveedor == idProveedor
                    && producto.Estado
                    && !idsAsociados.Contains(producto.IdProducto)
                    && (!hayTexto
                        || (producto.Codigo != null && producto.Codigo.ToUpper().Contains(patron))
                        || (producto.Nombre != null && producto.Nombre.ToUpper().Contains(patron)))
                group compra.FechaRegistro by new
                {
                    producto.IdProducto,
                    producto.Codigo,
                    producto.Nombre,
                    producto.Stock,
                    producto.StockMinimo
                } into grupo
                select new
                {
                    grupo.Key.IdProducto,
                    grupo.Key.Codigo,
                    grupo.Key.Nombre,
                    grupo.Key.Stock,
                    grupo.Key.StockMinimo,
                    UltimaCompra = grupo.Max()
                })
            .OrderByDescending(p => p.UltimaCompra)
            .ThenBy(p => p.Nombre)
            .ThenBy(p => p.IdProducto)
            .ToListAsync();

        asociadosCatalogo.AddRange(historicos.Select(f => new ProductoBuscadoDto
        {
            IdProducto = f.IdProducto,
            Codigo = f.Codigo ?? string.Empty,
            Nombre = f.Nombre ?? string.Empty,
            Stock = f.Stock,
            StockMinimo = f.StockMinimo
        }));

        return asociadosCatalogo;
    }

    private async Task<List<ProductoBuscadoDto>> ObtenerProductosNoHistoricosAsync(
        AppDbContext db, int[] idsExcluidos, bool hayTexto, string patron)
    {
        return await db.Productos
            .AsNoTracking()
            .Where(p => p.Estado)
            .Where(p => !idsExcluidos.Contains(p.IdProducto))
            .Where(p => !hayTexto
                || (p.Codigo != null && p.Codigo.ToUpper().Contains(patron))
                || (p.Nombre != null && p.Nombre.ToUpper().Contains(patron)))
            .OrderBy(p => p.Nombre)
            .ThenBy(p => p.IdProducto)
            .Select(p => new ProductoBuscadoDto
            {
                IdProducto = p.IdProducto,
                Codigo = p.Codigo ?? string.Empty,
                Nombre = p.Nombre ?? string.Empty,
                Stock = p.Stock,
                StockMinimo = p.StockMinimo
            })
            .ToListAsync();
    }
}
