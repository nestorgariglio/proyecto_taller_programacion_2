using System.Globalization;
using CapaNegocio.Enums;

namespace CapaNegocio.Compras;

/// <summary>
/// Interpretación determinista de la cantidad de una línea escrita como texto
/// (RF-20 y RF-21): solo dígitos, mayor que cero y hasta
/// <see cref="CalculoImportesCompra.CantidadMaxima"/>. Devuelve un motivo
/// tipado y nunca un texto (RF-55). Sin EF Core.
/// </summary>
public static class AnalizadorCantidad
{
    /// <summary>
    /// Analiza el texto en el orden fijado por el plan §6.3: vacío → solo
    /// dígitos (RF-20) → mayor que cero (RF-20) → hasta el máximo (RF-21). El
    /// desborde de <see cref="int"/> se clasifica como
    /// <see cref="MotivoCantidadInvalida.SuperaMaximo"/>.
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
                // Separadores de miles o decimales, signos, espacios internos y letras.
                return Invalido(MotivoCantidadInvalida.FormatoInvalido);
            }
        }

        // Los ceros a la izquierda no cambian el valor y evitan que una cadena
        // larga de ceros parezca un desborde.
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
            // Solo dígitos y sin ceros a la izquierda: el único fallo posible
            // es el desborde de int, que RF-21 clasifica como supera el máximo.
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

        return new ResultadoAnalisisCantidad { EsValido = true, Valor = valor };
    }

    private static ResultadoAnalisisCantidad Invalido(MotivoCantidadInvalida motivo)
        => new() { EsValido = false, Motivo = motivo };
}
