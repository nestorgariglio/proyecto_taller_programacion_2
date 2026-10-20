using CapaNegocio.Enums;
namespace CapaNegocio.DTOs.Categorias;

public class RespuestaModificacionCategoria
{
	public ResultadoModificacionCategoria Resultado { get; set; }
	public CategoriaRespuestaDto? Categoria { get; set; }
	public string Mensaje { get; set; } = string.Empty;
}
