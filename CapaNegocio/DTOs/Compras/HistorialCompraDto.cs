namespace CapaNegocio.DTOs.Compras;

/// <summary>
/// Fila del historial de compras (RF-49 y RF-50). La fecha viaja como
/// <see cref="DateTime"/> y el estado como <see cref="bool"/>: el formato y los
/// textos «Activa»/«Inactiva» son de la presentación (RNF-12). El número de
/// compra es la excepción documentada de RNF-5 (D-16) y del usuario solo se
/// expone nombre y apellido.
/// </summary>
public class HistorialCompraDto
{
	public int IdCompra { get; set; }

	public DateTime FechaRegistro { get; set; }

	public string RazonSocialProveedor { get; set; } = string.Empty;

	public string NombreUsuario { get; set; } = string.Empty;

	public decimal MontoTotal { get; set; }

	/// <summary><see langword="true"/> = activa; <see langword="false"/> = inactiva.</summary>
	public bool Estado { get; set; }
}
