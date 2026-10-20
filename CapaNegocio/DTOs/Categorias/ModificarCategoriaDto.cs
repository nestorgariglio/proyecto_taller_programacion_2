namespace CapaNegocio.DTOs.Categorias;

public class ModificarCategoriaDto
{
	/// <summary>
	/// Identificador de la categoría existente que se desea editar.
	/// </summary>
	public int IdCategoria { get; set; }

	/// <summary>
	/// Texto crudo del campo de la interfaz, sin recortar. El Recorte de
	/// bordes (espacios, tabuladores y saltos de línea) lo aplica CapaNegocio
	/// antes de validar y persistir (mismos criterios que el alta).
	/// </summary>
	public string? Descripcion { get; set; }
}
