using CapaNegocio.Enums;
namespace CapaNegocio.DTOs.Categorias;

public class RespuestaActivacionCategoria
{
	public ResultadoActivacionCategoria Resultado { get; set; }
	public CategoriaRespuestaDto? Categoria { get; set; }
	public string Mensaje { get; set; } = string.Empty;
}
