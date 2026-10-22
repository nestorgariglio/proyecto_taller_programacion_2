using CapaNegocio.Enums;
namespace CapaNegocio.DTOs.Categorias;

public class RespuestaCreacionCategoria
{
	public ResultadoCreacionCategoria Resultado { get; set; }
	public CategoriaRespuestaDto? Categoria { get; set; }
	public string Mensaje { get; set; } = string.Empty;
}
