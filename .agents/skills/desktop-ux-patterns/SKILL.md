---
name: desktop-ux-patterns
description: Estándares de experiencia de usuario (UX) para aplicaciones comerciales de escritorio (ERP/WinForms). Aplicar en la creación y revisión de formularios de captura, grillas de datos y flujos de trabajo.
---

# Patrones UX para Sistemas Comercial / ERP Desktop

## 1. Ergonomía y Navegación por Teclado (Keyboard-First)
- **Foco Inicial:** Todo formulario de carga debe asignar automáticamente el foco (`Focus()`) al primer campo editable al abrirse o limpiarse.
- **Tabulación:** Garantizar `TabIndex` secuencial sin saltos erráticos.
- **Acciones por Teclado:** 
  - `Enter` en campos de captura debe disparar la adición o avance de campo.
  - Teclas de acceso rápido visibles en botones (ej. `&Registrar Compra` para `Alt + R`).

## 2. Experiencia en Grillas (DataGridView UX)
- `SelectionMode = DataGridViewSelectionMode.FullRowSelect`.
- `MultiSelect = false` salvo requerimiento explícito.
- **Alineación:** Números, precios, cantidades y subtotales SIEMPRE alineados a la derecha (`DataGridViewContentAlignment.MiddleRight`).
- **Formato:** Precios y subtotales formateados como moneda (`C2` o `$ #,##0.00`).
- **Lectura:** Filas alternadas con un color de fondo ligeramente contrastado para facilitar la lectura de renglones largos.

## 3. Prevención de Errores y Retroalimentación
- **Estado de Botones:** El botón primario (ej. "REGISTRAR COMPRA") debe estar inhabilitado (`Enabled = false`) si el carrito/grilla no tiene ítems o falta seleccionar el proveedor.
- **Validación Inline:** Preferir `ErrorProvider` o etiquetas de mensaje internas antes que lanzar `MessageBox` emergentes interrumpiendo el flujo.
- **Carga Async:** Deshabilitar controles de acción durante operaciones de E/S o base de datos para evitar envíos duplicados.