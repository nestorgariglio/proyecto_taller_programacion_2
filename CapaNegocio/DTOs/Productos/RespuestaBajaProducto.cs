using CapaNegocio.Enums;
namespace CapaNegocio.DTOs.Productos;

/// <summary>
/// Respuesta de la baja lógica de un producto (RF-12).
/// </summary>
public class RespuestaBajaProducto
{
	public ResultadoBajaProducto Resultado { get; set; }
	public ProductoRespuestaDto? Producto { get; set; }
	public string Mensaje { get; set; } = string.Empty;
}
