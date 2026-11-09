---
name: winforms-ui-ux
description: Guía y estándares de maquetación UX/UI para aplicaciones C# .NET Windows Forms (WinForms). Utilizar siempre que se diseñen, modifiquen o revisen formularios, TableLayoutPanel, la visualización de grillas, márgenes, paddings y comportamiento responsive.
---

# Estándares UX/UI y Maquetado para WinForms C#

## 1. Espaciado y Bordes Contenedores (Padding/Margin)
- **Margen perimetral obligatorio:** Todo contenedor principal (`Panel`, `TableLayoutPanel`) incrustado dentro de un tab o ventana DEBE incluir un `Padding` interno mínimo de **10px a 12px** en los cuatro lados (`Padding = new Padding(12)`). Jamás dejar controles colados a 0px o 1px del borde de la pantalla.
- **Espaciado entre controles:** Mantener un `Margin` constante de 6px a 8px entre campos de entrada, botones y paneles para evitar colisiones visuales.

## 2. Reglas Estrictas para TableLayoutPanel (Master-Detail)
- **Estructura de filas:**
  - **Encabezados / Títulos:** `SizeType = Absolute` (30px - 40px).
  - **Fichas de Búsqueda / Filtros / Captura:** `SizeType = Absolute` (250px - 270px máximo). Queda prohibido usar `SizeType.Percent` alto en secciones de búsqueda, para evitar que se estiren verticalmente y consuman la pantalla.
  - **Grilla Principal / Contenido Central:** `SizeType = Percent` = 100%. Absorbe todo el espacio vertical sobrante de la pantalla.
  - **Panel de Acciones / Pie:** `SizeType = Absolute` (50px - 60px).
- **Propiedad AutoSize:** En el `TableLayoutPanel` raíz, `AutoSize = false`. Usar `Dock = DockStyle.Fill`.
- **Anchos y Columnas:** Usar `ColumnStyle` en `Percent = 50%` para bloques gemelos (ej. Producto/Proveedor).

## 3. Grillas y Contenedores Dinámicos
- Las grillas principales (`DataGridView`) y sus paneles contenedores directos DEBEN configurarse con `Dock = DockStyle.Fill`.
- Definir siempre `MinimumSize` en la grilla (ej. `MinimumSize = new Size(0, 180)`) para garantizar visibilidad de cabeceras y primeras filas.

## 4. Botones y Tipografía
- Usar `AutoSize = true` con `GrowAndShrink` y `Padding` horizontal (ej. `10, 0, 10, 0`) para evitar que textos largos se recorten.
- Alturas recomendadas para botones primarios: 36px a 40px.

## 5. Responsividad y Scroll de Seguridad
- El panel exterior anfitrión (`pnlContenedor`) debe tener `AutoScroll = true`.
- El `TableLayoutPanel` interior debe definir un `MinimumSize` global (ej. `980, 650`) para que en resoluciones reducidas se active la barra de desplazamiento vertical sin deformar la interfaz ni ocultar elementos.
- La ventana anfitriona principal (`Form` de inicio) debe arrancar maximizada (`WindowState = FormWindowState.Maximized`).