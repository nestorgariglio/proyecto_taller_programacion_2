using CapaNegocio.Enums;
namespace CapaNegocio.DTOs.Categorias;

/// <summary>
/// Resultado de previsualizar la baja de una categoría (solo lectura): trae el
/// texto de confirmación y la cantidad de productos activos que el usuario
/// confirmará, la cual se reenvía a <c>DarBajaCategoriaAsync</c>.
/// </summary>
public class PrevisualizacionBajaCategoria
{
	public ResultadoPrevisualizacionBaja Resultado { get; set; }
	public int ProductosActivos { get; set; }
	public string Mensaje { get; set; } = string.Empty;
}
