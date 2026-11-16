using CapaNegocio.Enums;

namespace CapaNegocio.DTOs.Compras;

public class RespuestaProveedoresDeProducto
{
	public ResultadoConsultaHistoricaCompra Resultado { get; set; }

	public string Mensaje { get; set; } = string.Empty;

	public IReadOnlyList<ProveedorDeProductoDto> Proveedores { get; set; } = Array.Empty<ProveedorDeProductoDto>();
}
