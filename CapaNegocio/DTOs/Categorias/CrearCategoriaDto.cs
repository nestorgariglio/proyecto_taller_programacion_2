namespace CapaNegocio.DTOs.Categorias;

public class CrearCategoriaDto
{
	/// <summary>
	/// Texto crudo del campo de la interfaz, sin recortar. El Recorte de
	/// bordes (espacios, tabuladores y saltos de línea) lo aplica CapaNegocio
	/// antes de validar y persistir.
	/// </summary>
	public string? Descripcion { get; set; }
}
