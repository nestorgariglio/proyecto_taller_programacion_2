using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Productos;

/// <summary>
/// Datos de lectura de un producto para la interfaz (RF-14, RF-15, RF-17, RF-20).
/// No expone datos sensibles y la categoría se identifica por su Id, con el
/// nombre resuelto por la lógica de negocio.
/// </summary>
public class ProductoRespuestaDto
{
	public int IdProducto { get; set; }
	public string? Codigo { get; set; }
	public string? Nombre { get; set; }
	public string? Descripcion { get; set; }
	public int IdCategoria { get; set; }
	public string? NombreCategoria { get; set; }
	public decimal PrecioCompra { get; set; }
	public decimal PrecioVenta { get; set; }
	public int Stock { get; set; }
	public bool Estado { get; set; }
	public DateTime FechaRegistro { get; set; }
	public DateTime? FechaModificacion { get; set; }
	public int? IdUsuarioModifica { get; set; }

	/// <summary>
	/// Clasificación excluyente de criticidad con el umbral global vigente
	/// (RF-8, RF-11): <c>Normal</c>, <c>BajoUmbral</c> o <c>Agotado</c> —este
	/// último incluye el stock negativo (D-11)—. La calcula la lógica de negocio
	/// y la interfaz solo la compara para resaltar la fila, sin recalcularla.
	/// Vale <c>Normal</c> por defecto: cubre el mapeo del ABM y las consultas
	/// sin umbral. No expone datos sensibles (RF-17).
	/// </summary>
	public NivelCriticidadStock NivelCriticidad { get; set; } = NivelCriticidadStock.Normal;
}
