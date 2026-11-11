using CapaDatos;
using CapaNegocio.DTOs.Home;
using CapaNegocio.Stock;
using Microsoft.EntityFrameworkCore;

namespace CapaNegocio;

/// <summary>
/// Calcula los tres indicadores de la pantalla de inicio con consultas agregadas
/// (sin contadores persistentes) y un contexto por operación.
/// </summary>
public class HomeResumenNegocio
{
	private readonly IDbContextFactory<AppDbContext> _fabrica;
	private readonly ResolucionUmbralStock _umbral;
	private readonly Func<DateTime> _ahora;

	public HomeResumenNegocio(
		IDbContextFactory<AppDbContext> fabrica,
		ResolucionUmbralStock umbral,
		Func<DateTime>? ahora = null)
	{
		ArgumentNullException.ThrowIfNull(fabrica);
		_fabrica = fabrica;
		_umbral = umbral;
		// Venta.FechaRegistro se guarda con DateTime.Now (hora local).
		_ahora = ahora ?? (() => DateTime.Now);
	}

	/// <summary>
	/// Obtiene el resumen. Un fallo de lectura devuelve <c>Disponible = false</c>
	/// sin propagar el detalle técnico; la cancelación sí se propaga.
	/// </summary>
	public async Task<ResumenHomeDto> ObtenerResumenAsync(CancellationToken ct = default)
	{
		try
		{
			// Día local: [hoy 00:00, mañana 00:00).
			DateTime desde = _ahora().Date;
			DateTime hasta = desde.AddDays(1);

			await using AppDbContext db = await _fabrica.CreateDbContextAsync(ct);

			// Venta válida = Estado activo (anulada/no concretada = Estado false).
			IQueryable<CapaEntidad.Venta> validas = db.Ventas.AsNoTracking()
				.Where(v => v.Estado && v.FechaRegistro >= desde && v.FechaRegistro < hasta);

			decimal total = await validas.SumAsync(v => (decimal?)v.MontoTotal, ct) ?? 0m;
			int atenciones = await validas.CountAsync(ct);

			// Misma regla que ProductoNegocio (SoloCriticos), sin filtro de estado.
			IQueryable<CapaEntidad.Producto> productos = db.Productos.AsNoTracking();
			int criticos;
			if (_umbral.Valor.HasValue)
			{
				int umbral = _umbral.Valor.Value;
				criticos = await productos.CountAsync(p => p.Stock <= 0 || (p.Stock > 0 && p.Stock < umbral), ct);
			}
			else
			{
				criticos = await productos.CountAsync(p => p.Stock <= 0, ct);
			}

			return new ResumenHomeDto
			{
				Disponible = true,
				VentasDelDia = total,
				ClientesAtendidos = atenciones,
				ProductosStockCritico = criticos
			};
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception)
		{
			return new ResumenHomeDto { Disponible = false };
		}
	}
}
