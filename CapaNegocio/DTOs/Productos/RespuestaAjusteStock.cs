using CapaNegocio.Enums;
namespace CapaNegocio.DTOs.Productos;

/// <summary>
/// Respuesta del ajuste manual de stock de un producto (RF-10/RF-11).
/// </summary>
public class RespuestaAjusteStock
{
	public ResultadoAjusteStock Resultado { get; set; }
	public ProductoRespuestaDto? Producto { get; set; }
	public string Mensaje { get; set; } = string.Empty;
}
