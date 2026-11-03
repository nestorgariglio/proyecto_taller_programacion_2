namespace CapaNegocio.Enums;

/// <summary>
/// Motivo tipado del rechazo de un precio, en el orden de evaluación de
/// RF-28: vacío, formato, no positivo, decimales y máximo (RF-22 … RF-26).
/// Los textos los redacta la lógica de negocio (RF-55).
/// </summary>
public enum MotivoPrecioInvalido
{
	Vacio,
	FormatoInvalido,
	NoPositivo,
	DecimalesExcedidos,
	SuperaMaximo
}
