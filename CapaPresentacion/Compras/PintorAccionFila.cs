using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CapaPresentacion.Compras
{
    /// <summary>
    /// Estado visual del botón «Deseleccionar» dibujado dentro de la fila elegida
    /// de las grillas de proveedores y productos.
    /// </summary>
    internal enum EstadoAccionFila
    {
        /// <summary>Botón en reposo: fondo blanco sobre la fila elegida.</summary>
        Reposo,

        /// <summary>El puntero está sobre el botón: tinte claro y borde de realce.</summary>
        Hover,

        /// <summary>El botón está presionado: tono más oscuro, borde interno y leve hundimiento.</summary>
        Presionado
    }

    /// <summary>
    /// Dibuja el botón «Deseleccionar» dentro de una celda de la grilla. Solo pinta
    /// el botón (forma redondeada y texto): decidir en qué fila se muestra es
    /// responsabilidad de la pantalla. Todas las medidas se expresan en píxeles
    /// lógicos (96 DPI) y se escalan con el factor recibido. Contrastes calculados
    /// (WCAG): texto reposo 5,60:1, hover 4,91:1, presionado 6,04:1; relleno
    /// contra la fila elegida 5,60:1 (reposo), 4,91:1 (hover) y 3,90:1 (presionado).
    /// </summary>
    internal static class PintorAccionFila
    {
        /// <summary>Texto del botón. No se reemplaza por una abreviatura.</summary>
        public const string TextoBoton = "Deseleccionar";

        /// <summary>
        /// Ancho lógico de la columna de acción: el texto mide 89 px a 100 % de DPI
        /// y el botón deja 4 de margen y 4 de relleno interno por lado.
        /// </summary>
        public const int AnchoColumnaLogico = 104;

        private const int MargenHorizontalLogico = 4;
        private const int MargenVerticalLogico = 3;
        private const int RellenoTextoLogico = 3;
        private const int RadioEsquinaLogico = 4;

        /// <summary>Color de la fila elegida (#BF360C); con texto blanco da 5,60:1.</summary>
        public static readonly Color ColorFilaElegida = Color.FromArgb(191, 54, 12);

        /// <summary>Color del texto de la fila elegida.</summary>
        public static readonly Color ColorTextoFilaElegida = Color.White;

        private static readonly Color FondoReposo = Color.White;
        private static readonly Color FondoHover = Color.FromArgb(255, 236, 230);
        private static readonly Color FondoPresionado = Color.FromArgb(255, 204, 188);
        private static readonly Color BordeHover = Color.FromArgb(255, 171, 145);
        private static readonly Color TextoReposo = Color.FromArgb(191, 54, 12);
        private static readonly Color TextoPresionado = Color.FromArgb(140, 40, 6);

        private const TextFormatFlags FormatoTexto =
            TextFormatFlags.HorizontalCenter
            | TextFormatFlags.VerticalCenter
            | TextFormatFlags.EndEllipsis
            | TextFormatFlags.NoPrefix
            | TextFormatFlags.NoPadding
            | TextFormatFlags.SingleLine;

        /// <summary>
        /// Calcula el rectángulo del botón dentro de una celda. Lo usan el dibujo y la
        /// detección de clic, de modo que coinciden exactamente.
        /// </summary>
        /// <param name="celda">Límites de la celda (en cualquier origen de coordenadas).</param>
        /// <param name="factor">Factor de escala (DPI actual / 96).</param>
        /// <returns>Rectángulo del botón, con margen interno respecto de la celda.</returns>
        public static Rectangle ObtenerRectanguloBoton(Rectangle celda, float factor)
        {
            int margenHorizontal = Escalar(MargenHorizontalLogico, factor);
            int margenVertical = Escalar(MargenVerticalLogico, factor);

            return new Rectangle(
                celda.X + margenHorizontal,
                celda.Y + margenVertical,
                Math.Max(0, celda.Width - (2 * margenHorizontal)),
                Math.Max(0, celda.Height - (2 * margenVertical)));
        }

        /// <summary>
        /// Dibuja el botón en la celda con el estado indicado. No modifica el estado
        /// del <see cref="Graphics"/> al terminar y libera todos los objetos GDI creados.
        /// </summary>
        /// <param name="grafico">Superficie de dibujo de la celda.</param>
        /// <param name="celda">Límites de la celda en coordenadas del <paramref name="grafico"/>.</param>
        /// <param name="estado">Estado visual del botón.</param>
        /// <param name="fuente">Fuente de la grilla (no se reduce la tipografía).</param>
        /// <param name="factor">Factor de escala (DPI actual / 96).</param>
        public static void Pintar(
            Graphics grafico, Rectangle celda, EstadoAccionFila estado, Font fuente, float factor)
        {
            ArgumentNullException.ThrowIfNull(grafico);
            ArgumentNullException.ThrowIfNull(fuente);

            Rectangle boton = ObtenerRectanguloBoton(celda, factor);

            if (boton.Width <= 0 || boton.Height <= 0)
            {
                return;
            }

            if (estado == EstadoAccionFila.Presionado)
            {
                // Leve hundimiento: queda dentro del margen vertical de la celda.
                boton.Offset(0, Math.Max(1, Escalar(1, factor)));
            }

            (Color relleno, Color texto, Color? borde, int grosorLogico) = ObtenerColores(estado);

            SmoothingMode suavizadoPrevio = grafico.SmoothingMode;
            grafico.SmoothingMode = SmoothingMode.AntiAlias;

            try
            {
                using GraphicsPath forma = CrearRectanguloRedondeado(boton, Escalar(RadioEsquinaLogico, factor));
                using SolidBrush pincel = new(relleno);
                grafico.FillPath(pincel, forma);

                if (borde is Color colorBorde)
                {
                    using Pen lapiz = new(colorBorde, Math.Max(1, Escalar(grosorLogico, factor)))
                    {
                        Alignment = PenAlignment.Inset
                    };
                    grafico.DrawPath(lapiz, forma);
                }
            }
            finally
            {
                grafico.SmoothingMode = suavizadoPrevio;
            }

            Rectangle zonaTexto = Rectangle.Inflate(boton, -Escalar(RellenoTextoLogico, factor), 0);
            TextRenderer.DrawText(grafico, TextoBoton, fuente, zonaTexto, texto, FormatoTexto);
        }

        private static (Color Relleno, Color Texto, Color? Borde, int GrosorLogico) ObtenerColores(
            EstadoAccionFila estado)
        {
            return estado switch
            {
                EstadoAccionFila.Hover => (FondoHover, TextoReposo, BordeHover, 2),
                EstadoAccionFila.Presionado => (FondoPresionado, TextoPresionado, TextoPresionado, 1),
                _ => (FondoReposo, TextoReposo, null, 0)
            };
        }

        private static GraphicsPath CrearRectanguloRedondeado(Rectangle limites, int radio)
        {
            int diametro = Math.Max(1, Math.Min(2 * radio, Math.Min(limites.Width, limites.Height)));
            Rectangle arco = new(limites.X, limites.Y, diametro, diametro);
            GraphicsPath forma = new();

            forma.AddArc(arco, 180, 90);
            arco.X = limites.Right - diametro;
            forma.AddArc(arco, 270, 90);
            arco.Y = limites.Bottom - diametro;
            forma.AddArc(arco, 0, 90);
            arco.X = limites.X;
            forma.AddArc(arco, 90, 90);
            forma.CloseFigure();

            return forma;
        }

        private static int Escalar(int valorLogico, float factor)
            => (int)Math.Round(valorLogico * factor);
    }
}
