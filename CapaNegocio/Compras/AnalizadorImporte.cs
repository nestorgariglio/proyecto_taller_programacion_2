using System.Globalization;
using CapaNegocio.Enums;

namespace CapaNegocio.Compras;

/// <summary>
/// Interpretación determinista de un precio escrito como texto.
/// </summary>
public static class AnalizadorImporte
{
    private const char SeparadorComa = ',';
    private const char SeparadorPunto = '.';
    private const char SignoNegativo = '-';

    /// <summary>
    /// Analiza el texto validando formato numérico, valor positivo, escala decimal y límites monetarios.
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

            return Invalido(MotivoPrecioInvalido.FormatoInvalido);
        }

        if (posicionSeparador >= 0
            && (posicionSeparador == inicioDigitos || posicionSeparador == recortado.Length - 1))
        {
            return Invalido(MotivoPrecioInvalido.FormatoInvalido);
        }

        string normalizado = posicionSeparador >= 0
            ? recortado.Replace(SeparadorComa, SeparadorPunto)
            : recortado;

        if (!decimal.TryParse(
                normalizado,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out decimal valor))
        {
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

        return new ResultadoAnalisisImporte
        {
            EsValido = true,
            Valor = valor,
            Motivo = null
        };
    }

    private static int ContarDecimales(string normalizado)
    {
        int indicePunto = normalizado.IndexOf(SeparadorPunto);
        return indicePunto < 0 ? 0 : normalizado.Length - indicePunto - 1;
    }

    private static ResultadoAnalisisImporte Invalido(MotivoPrecioInvalido motivo) =>
        new()
        {
            EsValido = false,
            Valor = 0m,
            Motivo = motivo
        };
}
