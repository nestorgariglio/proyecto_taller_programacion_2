using CapaNegocio.Enums;
namespace CapaNegocio.DTOs.Categorias;

public class ConfirmacionBajaCategoria
{
	public ResultadoBajaCategoria Resultado { get; set; }
	public string? Descripcion { get; set; }
	public int CantidadProductosActivos { get; set; }
	public string Mensaje { get; set; } = string.Empty;
	public bool PuedeDarseDeBaja { get; set; }
}
