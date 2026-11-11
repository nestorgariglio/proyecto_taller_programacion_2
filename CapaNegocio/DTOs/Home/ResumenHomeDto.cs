namespace CapaNegocio.DTOs.Home;

/// <summary>
/// Indicadores consolidados de la pantalla de inicio. <see cref="Disponible"/> es
/// <see langword="false"/> cuando la lectura falló; en ese caso los valores no son válidos.
/// </summary>
public class ResumenHomeDto
{
	public bool Disponible { get; set; }

	/// <summary>Suma de <c>MontoTotal</c> de las ventas válidas del día.</summary>
	public decimal VentasDelDia { get; set; }

	/// <summary>Cantidad de productos críticos (misma regla que «Solo críticos» de Productos).</summary>
	public int ProductosStockCritico { get; set; }

	/// <summary>Cantidad de ventas válidas del día (cada venta es una atención).</summary>
	public int ClientesAtendidos { get; set; }
}
