namespace CapaNegocio.Enums;

/// <summary>
/// Motivo tipado del rechazo de la cantidad de una línea escrita como texto,
/// en el orden de evaluación del plan §6.3: vacío, formato, no positivo y
/// máximo (RF-20 y RF-21). Los textos los redacta la lógica de negocio (RF-55).
/// </summary>
public enum MotivoCantidadInvalida
{
	Vacio,
	FormatoInvalido,
	NoPositivo,
	SuperaMaximo
}
