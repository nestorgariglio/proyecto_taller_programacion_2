using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CapaNegocio.DTOs.Compras;
using CapaNegocio.Enums;
using Microsoft.EntityFrameworkCore;

namespace CapaNegocio;

public partial class CompraNegocio
{
    /// <summary>
    /// Obtiene el historial completo de compras registradas.
    /// </summary>
    public async Task<RespuestaHistorialCompras> ObtenerHistorialAsync(string? rol)
    {
        if (!PoliticaAcceso.PuedeOperarCompras(rol))
        {
            return new RespuestaHistorialCompras
            {
                Resultado = ResultadoHistorialCompras.PermisoDenegado,
                Mensaje = MensajePermisoConsultarCompras
            };
        }

        try
        {
            List<FilaHistorial> filas = await _context.Compras
                .AsNoTracking()
                .OrderByDescending(c => c.FechaRegistro)
                .ThenByDescending(c => c.IdCompra)
                .Select(c => new FilaHistorial
                {
                    IdCompra = c.IdCompra,
                    FechaRegistro = c.FechaRegistro,
                    RazonSocialProveedor = _context.Proveedores
                        .Where(p => p.IdProveedor == c.IdProveedor)
                        .Select(p => p.RazonSocial)
                        .FirstOrDefault(),
                    Nombre = _context.Usuarios
                        .Where(u => u.IdUsuario == c.IdUsuario)
                        .Select(u => u.Nombre)
                        .FirstOrDefault(),
                    Apellido = _context.Usuarios
                        .Where(u => u.IdUsuario == c.IdUsuario)
                        .Select(u => u.Apellido)
                        .FirstOrDefault(),
                    MontoTotal = c.MontoTotal,
                    Estado = c.Estado,
                    EstadoSolicitud = c.EstadoSolicitud,
                    FechaConfirmacion = c.FechaConfirmacion,
                    FechaRecepcion = c.FechaRecepcion,
                    FechaAnulacion = c.FechaAnulacion,
                    MotivoAnulacion = c.MotivoAnulacion,
                    Observaciones = c.Observaciones
                })
                .ToListAsync();

            List<HistorialCompraDto> compras = filas
                .Select(f => new HistorialCompraDto
                {
                    IdCompra = f.IdCompra,
                    FechaRegistro = f.FechaRegistro,
                    RazonSocialProveedor = TextoProveedor(f.RazonSocialProveedor),
                    NombreUsuario = TextoUsuario(f.Nombre, f.Apellido),
                    MontoTotal = f.MontoTotal,
                    Estado = f.Estado,
                    EstadoSolicitud = f.EstadoSolicitud ?? (f.Estado ? "Recibida" : "Anulada"),
                    FechaConfirmacion = f.FechaConfirmacion,
                    FechaRecepcion = f.FechaRecepcion,
                    FechaAnulacion = f.FechaAnulacion,
                    MotivoAnulacion = f.MotivoAnulacion,
                    Observaciones = f.Observaciones
                })
                .ToList();

            if (compras.Count == 0)
            {
                return new RespuestaHistorialCompras
                {
                    Resultado = ResultadoHistorialCompras.SinCompras,
                    Mensaje = MensajeHistorialVacio
                };
            }

            return new RespuestaHistorialCompras
            {
                Resultado = ResultadoHistorialCompras.ConCompras,
                Mensaje = compras.Count == 1
                    ? MensajeUnaCompraEncontrada
                    : string.Format(CultureInfo.InvariantCulture, MensajeComprasEncontradas, compras.Count),
                Compras = compras
            };
        }
        catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
        {
            return new RespuestaHistorialCompras
            {
                Resultado = ResultadoHistorialCompras.ErrorPersistencia,
                Mensaje = MensajeErrorPersistencia
            };
        }
    }

    /// <summary>
    /// Obtiene una compra específica con el detalle completo de sus líneas.
    /// </summary>
    public async Task<RespuestaCompraConDetalle> ObtenerCompraConDetalleAsync(int idCompra, string? rol)
    {
        if (!PoliticaAcceso.PuedeOperarCompras(rol))
        {
            return new RespuestaCompraConDetalle
            {
                Resultado = ResultadoCompraConDetalle.PermisoDenegado,
                Mensaje = MensajePermisoConsultarCompras
            };
        }

        try
        {
            FilaHistorial? cabecera = await _context.Compras
                .AsNoTracking()
                .Where(c => c.IdCompra == idCompra)
                .Select(c => new FilaHistorial
                {
                    IdCompra = c.IdCompra,
                    FechaRegistro = c.FechaRegistro,
                    RazonSocialProveedor = _context.Proveedores
                        .Where(p => p.IdProveedor == c.IdProveedor)
                        .Select(p => p.RazonSocial)
                        .FirstOrDefault(),
                    Nombre = _context.Usuarios
                        .Where(u => u.IdUsuario == c.IdUsuario)
                        .Select(u => u.Nombre)
                        .FirstOrDefault(),
                    Apellido = _context.Usuarios
                        .Where(u => u.IdUsuario == c.IdUsuario)
                        .Select(u => u.Apellido)
                        .FirstOrDefault(),
                    MontoTotal = c.MontoTotal,
                    Estado = c.Estado,
                    EstadoSolicitud = c.EstadoSolicitud,
                    FechaConfirmacion = c.FechaConfirmacion,
                    FechaRecepcion = c.FechaRecepcion,
                    FechaAnulacion = c.FechaAnulacion,
                    MotivoAnulacion = c.MotivoAnulacion,
                    Observaciones = c.Observaciones
                })
                .FirstOrDefaultAsync();

            if (cabecera is null)
            {
                return new RespuestaCompraConDetalle
                {
                    Resultado = ResultadoCompraConDetalle.CompraNoEncontrada,
                    Mensaje = MensajeCompraNoEncontrada
                };
            }

            List<FilaDetalle> filas = await _context.DetallesCompra
                .AsNoTracking()
                .Where(d => d.IdCompra == idCompra)
                .OrderBy(d => d.IdDetalleCompra)
                .Select(d => new FilaDetalle
                {
                    IdProducto = d.IdProducto,
                    CodigoProducto = _context.Productos
                        .Where(p => p.IdProducto == d.IdProducto)
                        .Select(p => p.Codigo)
                        .FirstOrDefault(),
                    NombreProducto = _context.Productos
                        .Where(p => p.IdProducto == d.IdProducto)
                        .Select(p => p.Nombre)
                        .FirstOrDefault(),
                    Cantidad = d.Cantidad,
                    PrecioCompra = d.PrecioCompra,
                    Subtotal = d.Subtotal
                })
                .ToListAsync();

            return new RespuestaCompraConDetalle
            {
                Resultado = ResultadoCompraConDetalle.Encontrada,
                Mensaje = string.Empty,
                Compra = new CompraConDetalleDto
                {
                    IdCompra = cabecera.IdCompra,
                    FechaRegistro = cabecera.FechaRegistro,
                    RazonSocialProveedor = TextoProveedor(cabecera.RazonSocialProveedor),
                    NombreUsuario = TextoUsuario(cabecera.Nombre, cabecera.Apellido),
                    MontoTotal = cabecera.MontoTotal,
                    Estado = cabecera.Estado,
                    EstadoSolicitud = cabecera.EstadoSolicitud ?? (cabecera.Estado ? "Recibida" : "Anulada"),
                    FechaConfirmacion = cabecera.FechaConfirmacion,
                    FechaRecepcion = cabecera.FechaRecepcion,
                    FechaAnulacion = cabecera.FechaAnulacion,
                    MotivoAnulacion = cabecera.MotivoAnulacion,
                    Observaciones = cabecera.Observaciones,
                    Lineas = filas
                        .Select(f => new LineaCompraDetalleDto
                        {
                            IdProducto = f.IdProducto,
                            CodigoProducto = TextoProducto(f.CodigoProducto),
                            NombreProducto = TextoProducto(f.NombreProducto),
                            Cantidad = f.Cantidad,
                            PrecioCompra = f.PrecioCompra,
                            Subtotal = f.Subtotal
                        })
                        .ToList()
                }
            };
        }
        catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
        {
            return new RespuestaCompraConDetalle
            {
                Resultado = ResultadoCompraConDetalle.ErrorPersistencia,
                Mensaje = MensajeErrorPersistencia
            };
        }
    }

    private static string TextoProveedor(string? razonSocial)
        => string.IsNullOrWhiteSpace(razonSocial) ? TextoSinProveedor : razonSocial.Trim();

    private static string TextoUsuario(string? nombre, string? apellido)
    {
        string nombreCompleto =
            $"{(nombre ?? string.Empty).Trim()} {(apellido ?? string.Empty).Trim()}".Trim();

        return string.IsNullOrWhiteSpace(nombreCompleto) ? TextoSinUsuario : nombreCompleto;
    }

    private static string TextoProducto(string? texto)
        => string.IsNullOrWhiteSpace(texto) ? string.Empty : texto.Trim();

    private sealed class FilaHistorial
    {
        public int IdCompra { get; set; }

        public DateTime FechaRegistro { get; set; }

        public string? RazonSocialProveedor { get; set; }

        public string? Nombre { get; set; }

        public string? Apellido { get; set; }

        public decimal MontoTotal { get; set; }

        public bool Estado { get; set; }

        public string? EstadoSolicitud { get; set; }

        public DateTime? FechaConfirmacion { get; set; }

        public DateTime? FechaRecepcion { get; set; }

        public DateTime? FechaAnulacion { get; set; }

        public string? MotivoAnulacion { get; set; }

        public string? Observaciones { get; set; }
    }

    private sealed class FilaDetalle
    {
        public int IdProducto { get; set; }

        public string? CodigoProducto { get; set; }

        public string? NombreProducto { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioCompra { get; set; }

        public decimal Subtotal { get; set; }
    }
}
