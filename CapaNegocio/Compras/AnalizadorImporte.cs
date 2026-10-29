using System.Globalization;
using CapaNegocio.Enums;

namespace CapaNegocio.Compras;

/// <summary>
/// Interpretación determinista de un precio escrito como texto (RF-22 … RF-26
/// y RF-28). Admite un único separador decimal —coma o punto, nunca de
/// miles— y, opcionalmente, un único «-» inicial. Devuelve un motivo tipado y
/// nunca un texto (RF-55). Sin EF Core y sin redondeos (RNF-1).
/// </summary>
public static class AnalizadorImporte
{
    private const char SeparadorComa = ',';
    private const char SeparadorPunto = '.';
    private const char SignoNegativo = '-';

    /// <summary>
    /// Analiza el texto en el orden fijado por el plan §6.3: vacío → formato
    /// → no positivo → decimales excedidos → supera el máximo. El desborde de
    /// <see cref="decimal"/> se clasifica como <see cref="MotivoPrecioInvalido.SuperaMaximo"/>.
    /// </summary>
    public static ResultadoAnalisisImporte Analizar(string? texto)
    {
        string recortado = (texto ?? string.Empty).Trim();

        if (recortado.Length == 0)
        {
            return Invalido(MotivoPrecioInvalido.Vacio);
        }

        int inicioDigitos = recortado[0] == SignoNegativo ? 1 : 0;

        if (inicioDigitos == recortado.Length)
        {
            // Solo un signo «-», sin dígitos.
            return Invalido(MotivoPrecioInvalido.FormatoInvalido);
        }

        int posicionSeparador = -1;

        for (int i = inicioDigitos; i < recortado.Length; i++)
        {
            char caracter = recortado[i];

            if (char.IsAsciiDigit(caracter))
            {
                continue;
            }

            if ((caracter == SeparadorComa || caracter == SeparadorPunto) && posicionSeparador < 0)
            {
                posicionSeparador = i;
                continue;
            }

            // Segundo separador, otro carácter, signo fuera de la posición
            // inicial o espacio interno.
            return Invalido(MotivoPrecioInvalido.FormatoInvalido);
        }

        if (posicionSeparador >= 0
            && (posicionSeparador == inicioDigitos || posicionSeparador == recortado.Length - 1))
        {
            // Separador sin dígitos a uno de sus lados: «10,» o «,5».
            return Invalido(MotivoPrecioInvalido.FormatoInvalido);
        }

        // El separador se interpreta siempre como decimal (RF-25): no hay
        // puntos de miles, así que «1.000» es 1,000.
        string normalizado = posicionSeparador >= 0
            ? recortado.Replace(SeparadorComa, SeparadorPunto)
            : recortado;

        if (!decimal.TryParse(
                normalizado,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out decimal valor))
        {
            // El formato ya está garantizado: el único fallo posible es el
            // desborde de decimal, que RF-24 clasifica como supera el máximo.
            return Invalido(MotivoPrecioInvalido.SuperaMaximo);
        }

        if (valor <= 0m)
        {
            return Invalido(MotivoPrecioInvalido.NoPositivo);
        }

        if (ContarDecimales(normalizado) > CalculoImportesCompra.DecimalesPrecio)
        {
            return Invalido(MotivoPrecioInvalido.DecimalesExcedidos);
        }

        if (valor > CalculoImportesCompra.PrecioMaximo)
        {
            return Invalido(MotivoPrecioInvalido.SuperaMaximo);
        }

        return new ResultadoAnalisisImporte { EsValido = true, Valor = valor };
    }

    private static int ContarDecimales(string textoNormalizado)
    {
        int posicion = textoNormalizado.IndexOf(SeparadorPunto);

        return posicion < 0 ? 0 : textoNormalizado.Length - posicion - 1;
    }

    private static ResultadoAnalisisImporte Invalido(MotivoPrecioInvalido motivo)
        => new() { EsValido = false, Motivo = motivo };
}
