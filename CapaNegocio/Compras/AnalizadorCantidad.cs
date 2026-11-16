using System.Globalization;
using CapaNegocio.Enums;

namespace CapaNegocio.Compras;

/// <summary>
/// Interpretación determinista de la cantidad de una línea escrita como texto.
/// </summary>
public static class AnalizadorCantidad
{
    /// <summary>
    /// Analiza el texto validando formato numérico, valor positivo y límite máximo.
    /// </summary>
    public static ResultadoAnalisisCantidad Analizar(string? texto)
    {
        string recortado = (texto ?? string.Empty).Trim();

        if (recortado.Length == 0)
        {
            return Invalido(MotivoCantidadInvalida.Vacio);
        }

        foreach (char caracter in recortado)
        {
            if (!char.IsAsciiDigit(caracter))
            {
                return Invalido(MotivoCantidadInvalida.FormatoInvalido);
            }
        }

        string sinCerosIzquierda = recortado.TrimStart('0');

        if (sinCerosIzquierda.Length == 0)
        {
            return Invalido(MotivoCantidadInvalida.NoPositivo);
        }

        if (!int.TryParse(
                sinCerosIzquierda,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int valor))
        {
            return Invalido(MotivoCantidadInvalida.SuperaMaximo);
        }

        if (valor < CalculoImportesCompra.CantidadMinima)
        {
            return Invalido(MotivoCantidadInvalida.NoPositivo);
        }

        if (valor > CalculoImportesCompra.CantidadMaxima)
        {
            return Invalido(MotivoCantidadInvalida.SuperaMaximo);
        }

        return new ResultadoAnalisisCantidad
        {
            EsValido = true,
            Valor = valor,
            Motivo = null
        };
    }

    private static ResultadoAnalisisCantidad Invalido(MotivoCantidadInvalida motivo) =>
        new()
        {
            EsValido = false,
            Valor = 0,
            Motivo = motivo
        };
}
