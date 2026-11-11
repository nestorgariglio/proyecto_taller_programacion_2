namespace CapaNegocio.DTOs.Home;

/// <summary>
/// Indicadores cuantitativos para el panel de inicio.
/// </summary>
public class ResumenHomeDto
{
	public bool Disponible { get; set; }

	public decimal VentasDelDia { get; set; }

	public int ProductosStockCritico { get; set; }

	public int ClientesAtendidos { get; set; }
}
