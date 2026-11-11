using CapaDatos;
using CapaNegocio.DTOs.Home;
using CapaNegocio.Stock;
using Microsoft.EntityFrameworkCore;

namespace CapaNegocio;

/// <summary>
/// Provee indicadores agregados de ventas, clientes y stock crítico para la vista de inicio.
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
		_ahora = ahora ?? (() => DateTime.Now);
	}

	/// <summary>
	/// Obtiene los indicadores del día para la pantalla principal.
	/// </summary>
	/// <param name="ct">Token de cancelación.</param>
	/// <returns>Datos del resumen de inicio.</returns>
	public async Task<ResumenHomeDto> ObtenerResumenAsync(CancellationToken ct = default)
	{
		try
		{
			DateTime desde = _ahora().Date;
			DateTime hasta = desde.AddDays(1);

			await using AppDbContext db = await _fabrica.CreateDbContextAsync(ct);

			IQueryable<CapaEntidad.Venta> validas = db.Ventas.AsNoTracking()
				.Where(v => v.Estado && v.FechaRegistro >= desde && v.FechaRegistro < hasta);

			decimal total = await validas.SumAsync(v => (decimal?)v.MontoTotal, ct) ?? 0m;
			int atenciones = await validas.CountAsync(ct);

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
