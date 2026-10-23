namespace CapaNegocio.DTOs.Productos;

/// <summary>
/// Filtros del listado de productos (RF-15). Todos son opcionales: un filtro
/// sin informar no restringe el resultado.
/// </summary>
public class FiltroProductosDto
{
	/// <summary>
	/// Texto libre que se busca por contenido en el Código y/o en el Nombre,
	/// sin distinguir mayúsculas y minúsculas. Se recorta antes de buscar.
	/// </summary>
	public string? Texto { get; set; }

	/// <summary>
	/// Identificador de la categoría por la que se filtra (la relación se
	/// resuelve por Id, no por descripción). <c>null</c> = todas las categorías.
	/// </summary>
	public int? IdCategoria { get; set; }

	/// <summary>
	/// Estado por el que se filtra: <c>true</c> = activos, <c>false</c> =
	/// inactivos y <c>null</c> = todos (RF-14).
	/// </summary>
	public bool? Estado { get; set; }
}
