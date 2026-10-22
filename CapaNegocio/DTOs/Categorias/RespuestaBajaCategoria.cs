using CapaNegocio.Enums;
namespace CapaNegocio.DTOs.Categorias;

public class RespuestaBajaCategoria
{
	public ResultadoBajaCategoria Resultado { get; set; }
	public CategoriaRespuestaDto? Categoria { get; set; }
	public string Mensaje { get; set; } = string.Empty;
}
