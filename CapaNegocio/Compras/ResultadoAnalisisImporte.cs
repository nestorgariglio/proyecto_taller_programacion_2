using CapaNegocio.Enums;

namespace CapaNegocio.Compras;

/// <summary>
/// Resultado del análisis de un precio escrito como texto (RF-22 … RF-26 y
/// RF-28): el valor en decimal o el motivo tipado del rechazo. Nunca lleva
/// textos: el mensaje lo redacta la lógica de negocio (RF-55).
/// </summary>
public class ResultadoAnalisisImporte
{
    /// <summary><see langword="true"/> si el texto es un precio admitido.</summary>
    public bool EsValido { get; set; }

    /// <summary>Valor interpretado; <c>0</c> cuando el texto es inválido.</summary>
    public decimal Valor { get; set; }

    /// <summary>Motivo del rechazo; <see langword="null"/> cuando el texto es válido.</summary>
    public MotivoPrecioInvalido? Motivo { get; set; }
}
