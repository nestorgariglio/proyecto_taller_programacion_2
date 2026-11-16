using CapaNegocio.Enums;

namespace CapaNegocio.Compras;

/// <summary>
/// Resultado del análisis de un precio escrito como texto.
/// </summary>
public class ResultadoAnalisisImporte
{
    public bool EsValido { get; set; }

    public decimal Valor { get; set; }

    public MotivoPrecioInvalido? Motivo { get; set; }
}
