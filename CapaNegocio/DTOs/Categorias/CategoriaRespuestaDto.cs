namespace CapaNegocio.DTOs.Categorias;

public class CategoriaRespuestaDto
{
	public int IdCategoria { get; set; }
	public string? Descripcion { get; set; }
	public bool Estado { get; set; }
	public DateTime FechaRegistro { get; set; }
}
