namespace CapaNegocio.Enums;

/// <summary>
/// Resultado de las consultas históricas producto ↔ proveedor de Compras
/// (RF-9 … RF-12 y RF-15 … RF-17): proveedores que ya vendieron un producto y
/// productos que ya vendió un proveedor. Un único valor por caso, sin alias
/// (plan §3.5) e incluyendo <c>PermisoDenegado</c>, que las búsquedas no
/// declaran (RF-61).
/// </summary>
public enum ResultadoConsultaHistoricaCompra
{
	ConResultados,
	SinResultados,
	PermisoDenegado,
	ErrorPersistencia
}
