using CapaNegocio.Enums;
namespace CapaNegocio.DTOs.Productos;

/// <summary>
/// Respuesta de la reactivación de un producto (RF-13). El mensaje es
/// específico cuando la categoría del producto está inactiva (D-12).
/// </summary>
public class RespuestaReactivacionProducto
{
	public ResultadoReactivacionProducto Resultado { get; set; }
	public ProductoRespuestaDto? Producto { get; set; }
	public string Mensaje { get; set; } = string.Empty;
}
