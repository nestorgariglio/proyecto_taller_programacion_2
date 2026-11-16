using System.Globalization;
using CapaDatos;
using CapaEntidad;
using CapaNegocio.DTOs.Reportes;
using Microsoft.EntityFrameworkCore;

namespace CapaNegocio;

/// <summary>
/// Servicio de negocio para la generación de reportes y métricas analíticas del sistema.
/// </summary>
public class ReporteNegocio
{
    private readonly IDbContextFactory<AppDbContext> _fabrica;
    private readonly Func<DateTime> _ahora;

    public ReporteNegocio(
        IDbContextFactory<AppDbContext> fabrica,
        Func<DateTime>? ahora = null)
    {
        ArgumentNullException.ThrowIfNull(fabrica);
        _fabrica = fabrica;
        _ahora = ahora ?? (() => DateTime.Now);
    }

    /// <summary>
    /// Obtiene las métricas consolidadas según el período especificado.
    /// </summary>
    /// <param name="periodo">Nombre del período ("Últimos 7 días", "Este Mes", "Último Trimestre", "Año Actual").</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>DTO con KPIs, evolución de ventas, categorías y resumen diario.</returns>
    public async Task<ReporteMetricasDto> ObtenerReporteAsync(string? periodo, CancellationToken ct = default)
    {
        DateTime ahora = _ahora();
        (DateTime desde, DateTime hasta) = ResolverRangoFechas(periodo, ahora);

        await using AppDbContext db = await _fabrica.CreateDbContextAsync(ct);

        List<Venta> ventas = await db.Ventas.AsNoTracking()
            .Include(v => v.Detalles)!
                .ThenInclude(d => d.Producto)!
                    .ThenInclude(p => p.Categoria)
            .Where(v => v.Estado && v.FechaRegistro >= desde && v.FechaRegistro < hasta)
            .OrderBy(v => v.FechaRegistro)
            .ToListAsync(ct);

        decimal facturacionTotal = ventas.Sum(v => v.MontoTotal);
        int cantidadVentas = ventas.Count;
        decimal ticketPromedio = cantidadVentas > 0
            ? Math.Round(facturacionTotal / cantidadVentas, 2)
            : 0m;

        decimal gananciaTotal = 0m;
        foreach (Venta v in ventas)
        {
            if (v.Detalles == null) continue;
            foreach (DetalleVenta d in v.Detalles)
            {
                gananciaTotal += (d.Subtotal - (d.Cantidad * d.PrecioCompra));
            }
        }

        List<EvolucionVentaItemDto> evolucion = GenerarEvolucionVentas(ventas, periodo, desde, ahora);
        List<ParticipacionCategoriaItemDto> categorias = GenerarParticipacionCategorias(ventas);
        List<ResumenDiarioItemDto> resumenDiario = GenerarResumenDiario(ventas);

        return new ReporteMetricasDto
        {
            FacturacionTotal = facturacionTotal,
            GananciaTotal = gananciaTotal,
            TicketPromedio = ticketPromedio,
            CantidadVentas = cantidadVentas,
            EvolucionVentas = evolucion,
            ParticipacionCategorias = categorias,
            ResumenDiario = resumenDiario
        };
    }

    private static (DateTime desde, DateTime hasta) ResolverRangoFechas(string? periodo, DateTime ahora)
    {
        string normalizado = (periodo ?? string.Empty).Trim().ToLowerInvariant();

        DateTime hoy = ahora.Date;
        DateTime finDelDia = hoy.AddDays(1);

        if (normalizado.Contains("mes"))
        {
            DateTime inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            return (inicioMes, finDelDia);
        }

        if (normalizado.Contains("trimestre"))
        {
            DateTime inicioTrimestre = hoy.AddMonths(-3);
            return (inicioTrimestre, finDelDia);
        }

        if (normalizado.Contains("año") || normalizado.Contains("ano") || normalizado.Contains("actual"))
        {
            DateTime inicioAnio = new DateTime(hoy.Year, 1, 1);
            return (inicioAnio, finDelDia);
        }

        // Por defecto: Últimos 7 días
        DateTime sieteDiasAtras = hoy.AddDays(-6);
        return (sieteDiasAtras, finDelDia);
    }

    private static List<EvolucionVentaItemDto> GenerarEvolucionVentas(
        List<Venta> ventas,
        string? periodo,
        DateTime desde,
        DateTime ahora)
    {
        string normalizado = (periodo ?? string.Empty).Trim().ToLowerInvariant();
        List<EvolucionVentaItemDto> resultado = new();

        if (normalizado.Contains("trimestre") || normalizado.Contains("año") || normalizado.Contains("ano"))
        {
            // Agrupación mensual
            var porMes = ventas
                .GroupBy(v => new DateTime(v.FechaRegistro.Year, v.FechaRegistro.Month, 1))
                .OrderBy(g => g.Key);

            foreach (var g in porMes)
            {
                resultado.Add(new EvolucionVentaItemDto
                {
                    Fecha = g.Key,
                    EtiquetaFecha = g.Key.ToString("MMM yyyy", CultureInfo.CurrentCulture),
                    TotalFacturado = g.Sum(v => v.MontoTotal)
                });
            }

            if (resultado.Count == 0)
            {
                resultado.Add(new EvolucionVentaItemDto
                {
                    Fecha = ahora,
                    EtiquetaFecha = ahora.ToString("MMM yyyy", CultureInfo.CurrentCulture),
                    TotalFacturado = 0m
                });
            }
        }
        else
        {
            // Agrupación diaria consecutiva (ideal para 7 días o Este Mes)
            DateTime fin = ahora.Date;
            for (DateTime dia = desde.Date; dia <= fin; dia = dia.AddDays(1))
            {
                decimal totalDia = ventas
                    .Where(v => v.FechaRegistro.Date == dia)
                    .Sum(v => v.MontoTotal);

                resultado.Add(new EvolucionVentaItemDto
                {
                    Fecha = dia,
                    EtiquetaFecha = dia.ToString("dd/MM"),
                    TotalFacturado = totalDia
                });
            }
        }

        return resultado;
    }

    private static List<ParticipacionCategoriaItemDto> GenerarParticipacionCategorias(List<Venta> ventas)
    {
        var detalles = ventas
            .Where(v => v.Detalles != null)
            .SelectMany(v => v.Detalles!);

        var grupos = detalles
            .GroupBy(d => d.Producto?.Categoria?.Descripcion ?? "General")
            .Select(g => new
            {
                Categoria = g.Key,
                Monto = g.Sum(d => d.Subtotal)
            })
            .OrderByDescending(x => x.Monto)
            .ToList();

        decimal granTotal = grupos.Sum(x => x.Monto);

        return grupos.Select(g => new ParticipacionCategoriaItemDto
        {
            Categoria = g.Categoria,
            Monto = g.Monto,
            Porcentaje = granTotal > 0 ? Math.Round((double)(g.Monto / granTotal) * 100, 1) : 0
        }).ToList();
    }

    private static List<ResumenDiarioItemDto> GenerarResumenDiario(List<Venta> ventas)
    {
        return ventas
            .GroupBy(v => v.FechaRegistro.Date)
            .OrderByDescending(g => g.Key)
            .Select(g =>
            {
                decimal gananciaDia = g
                    .Where(v => v.Detalles != null)
                    .SelectMany(v => v.Detalles!)
                    .Sum(d => d.Subtotal - (d.Cantidad * d.PrecioCompra));

                return new ResumenDiarioItemDto
                {
                    Fecha = g.Key,
                    FechaTexto = g.Key.ToString("dd/MM/yyyy"),
                    ComprobantesEmitidos = g.Count(),
                    VentasTotales = g.Sum(v => v.MontoTotal),
                    Ganancia = gananciaDia
                };
            })
            .ToList();
    }
}
