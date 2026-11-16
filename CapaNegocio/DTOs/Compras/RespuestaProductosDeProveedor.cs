using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Compras;

public class RespuestaProductosDeProveedor
{
	public ResultadoConsultaHistoricaCompra Resultado { get; set; }

	public string Mensaje { get; set; } = string.Empty;

	public IReadOnlyList<ProductoDeProveedorDto> Productos { get; set; } = Array.Empty<ProductoDeProveedorDto>();
}
