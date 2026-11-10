namespace CapaNegocio.DTOs.Ventas;

public sealed record ResultadoCarritoDto(bool Exito, string? Mensaje, CarritoVentaDto Carrito);
