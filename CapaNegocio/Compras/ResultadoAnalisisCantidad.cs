using CapaNegocio.Enums;

namespace CapaNegocio.Compras;

/// <summary>
/// Resultado del análisis de una cantidad escrita como texto (RF-20 y RF-21):
/// el valor entero o el motivo tipado del rechazo. Nunca lleva textos: el
/// mensaje lo redacta la lógica de negocio (RF-55).
/// </summary>
public class ResultadoAnalisisCantidad
{
    /// <summary><see langword="true"/> si el texto es una cantidad admitida.</summary>
    public bool EsValido { get; set; }

    /// <summary>Valor interpretado; <c>0</c> cuando el texto es inválido.</summary>
    public int Valor { get; set; }

    /// <summary>
    /// Motivo del rechazo: <see cref="MotivoCantidadInvalida.Vacio"/> o
    /// <see cref="MotivoCantidadInvalida.FormatoInvalido"/> (RF-20),
    /// <see cref="MotivoCantidadInvalida.NoPositivo"/> (RF-20) o
    /// <see cref="MotivoCantidadInvalida.SuperaMaximo"/> (RF-21);
    /// <see langword="null"/> cuando el texto es válido.
    /// </summary>
    public MotivoCantidadInvalida? Motivo { get; set; }
}
