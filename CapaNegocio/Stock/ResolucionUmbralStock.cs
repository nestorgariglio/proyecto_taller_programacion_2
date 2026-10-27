using System.Globalization;

namespace CapaNegocio.Stock;

/// <summary>
/// Resultado de interpretar el parámetro global de configuración del umbral
/// mínimo de stock (RF-9, RF-10 y RF-20, según D-10 y D-15).
/// Es lógica pura sobre el valor crudo recibido: no depende del proveedor de
/// configuración ni del acceso a datos, de modo que la presentación es la única
/// responsable de leer la configuración.
/// Los dos estados posibles son excluyentes: o hay umbral aplicable
/// (<see cref="Valor"/> informado y <see cref="EsDegradado"/> en <c>false</c>)
/// o el criterio está degradado (<see cref="Valor"/> en <c>null</c> y
/// <see cref="EsDegradado"/> en <c>true</c>). Se construye solo por
/// <see cref="Resolver"/> o por las fábricas <see cref="ConUmbral"/> y
/// <see cref="Degradada"/>.
/// </summary>
public readonly record struct ResolucionUmbralStock
{
    /// <summary>
    /// Umbral asumido cuando el parámetro de configuración no está definido
    /// (RF-9 / D-10). Es un valor de reserva interno, no un valor inicial
    /// obligatorio de configuración.
    /// </summary>
    public const int UmbralPorDefecto = 5;

    private ResolucionUmbralStock(int? valor, bool esDegradado)
    {
        Valor = valor;
        EsDegradado = esDegradado;
    }

    /// <summary>
    /// Umbral aplicable al criterio de criticidad, o <c>null</c> en modo
    /// degradado. En modo degradado solo el stock agotado es crítico (RF-10).
    /// </summary>
    public int? Valor { get; }

    /// <summary>
    /// Indica que el parámetro estaba definido con un valor inválido (RF-10).
    /// La ausencia del parámetro <strong>no</strong> degrada: se asume
    /// <see cref="UmbralPorDefecto"/> (D-10).
    /// </summary>
    public bool EsDegradado { get; }

    /// <summary>
    /// Construye una resolución con umbral aplicable (no degradada).
    /// </summary>
    /// <param name="valor">Umbral en unidades; debe ser mayor o igual a 0 (D-9).</param>
    /// <returns>Resolución con el umbral indicado.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// El umbral es negativo, situación que <see cref="Resolver"/> nunca produce (D-9).
    /// </exception>
    public static ResolucionUmbralStock ConUmbral(int valor)
    {
        if (valor < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(valor), valor, "El umbral de stock debe ser mayor o igual a 0.");
        }

        return new ResolucionUmbralStock(valor, esDegradado: false);
    }

    /// <summary>
    /// Construye una resolución en modo degradado: sin umbral aplicable, de modo
    /// que solo el stock agotado resulta crítico (RF-10).
    /// </summary>
    /// <returns>Resolución degradada, sin valor de umbral.</returns>
    public static ResolucionUmbralStock Degradada()
    {
        return new ResolucionUmbralStock(valor: null, esDegradado: true);
    }

    /// <summary>
    /// Interpreta el valor crudo del parámetro de configuración del umbral de
    /// forma determinista y culturalmente invariante (RF-20):
    /// <list type="bullet">
    /// <item><description>
    /// <c>null</c> ⇒ <see cref="UmbralPorDefecto"/> sin degradar (D-10).
    /// </description></item>
    /// <item><description>
    /// Se aplica Recorte y se aceptan <strong>solo</strong> dígitos <c>'0'..'9'</c>,
    /// sin signo, sin separadores de miles y sin punto decimal.
    /// </description></item>
    /// <item><description>
    /// La conversión usa <see cref="NumberStyles.None"/> y
    /// <see cref="CultureInfo.InvariantCulture"/>; un entero fuera del rango
    /// admitido no produce excepción: es inválido.
    /// </description></item>
    /// <item><description>
    /// Un valor recortado no entero o negativo es inválido ⇒ modo degradado
    /// (RF-10 / D-9).
    /// </description></item>
    /// </list>
    /// </summary>
    /// <param name="valorCrudo">
    /// Valor del parámetro tal como lo entrega la configuración, o <c>null</c>
    /// cuando la clave no está definida.
    /// </param>
    /// <returns>Resolución con umbral aplicable o en modo degradado.</returns>
    public static ResolucionUmbralStock Resolver(string? valorCrudo)
    {
        if (valorCrudo is null)
        {
            return ConUmbral(UmbralPorDefecto);
        }

        string valorRecortado = valorCrudo.Trim();

        if (valorRecortado.Length == 0 || !EsSoloDigitos(valorRecortado))
        {
            return Degradada();
        }

        if (!int.TryParse(
                valorRecortado,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int valor))
        {
            return Degradada();
        }

        if (valor < 0)
        {
            return Degradada();
        }

        return ConUmbral(valor);
    }

    /// <summary>
    /// Verifica que el texto contenga únicamente dígitos <c>'0'..'9'</c> (RF-20):
    /// descarta signos, separadores, decimales y espacios internos.
    /// </summary>
    /// <param name="texto">Texto ya recortado a verificar.</param>
    /// <returns><c>true</c> cuando todos los caracteres son dígitos ASCII.</returns>
    private static bool EsSoloDigitos(string texto)
    {
        for (int indice = 0; indice < texto.Length; indice++)
        {
            if (texto[indice] < '0' || texto[indice] > '9')
            {
                return false;
            }
        }

        return true;
    }
}
