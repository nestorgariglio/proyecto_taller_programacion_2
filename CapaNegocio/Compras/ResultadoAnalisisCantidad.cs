using CapaNegocio.Enums;

namespace CapaNegocio.Compras;

/// <summary>
/// Resultado del análisis de una cantidad escrita como texto.
/// </summary>
public class ResultadoAnalisisCantidad
{
    public bool EsValido { get; set; }

    public int Valor { get; set; }

    public MotivoCantidadInvalida? Motivo { get; set; }
}
