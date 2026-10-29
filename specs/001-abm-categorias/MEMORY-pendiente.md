# `MEMORY.md` — Constancia de volcado (Spec 001, reencuadre de acceso)

> Verificado el **2026-10-20** leyendo `MEMORY.md` y este archivo.
> **La mayor parte del contenido de este archivo YA FUE CONSOLIDADA en `MEMORY.md`** y **no queda nada de lo ya volcado por aplicar**.

## 1. Estado del volcado — lo que YA está en `MEMORY.md` (no repetir)

Todo lo referente al **reencuadre de acceso (D-4)** y al **crash `products.png`** quedó consolidado:

- **Aprendizajes (§1 del archivo original)** → aplicados en `MEMORY.md` §"Aprendizajes y Errores a Evitar":
  - Regla del usuario: **consultar antes** de cualquier `DELETE` físico (línea 21).
  - Regla del usuario: cualquier **modificación significativa de UI** se **consulta antes** (línea 22).
  - `dotnet ef` requiere antes `dotnet tool restore` (línea 23).
  - Crí**`ImageList` de un `TabControl`: dos `TabPage` con la misma `ImageKey`** → `ArgumentException … Key: products.png`; corregido eliminando `tab_categorias` (línea 24).
- **Estado Actual / Próximos Pasos (§2 del archivo original)** → consolidado en `MEMORY.md`:
  - Reencuadre de categorías (de ítem del menú a característica del módulo Productos, en **ventana modal** desde `ProductosForm`) — línea 8.
  - Crash `products.png` corregido — líneas 8 y 24.
  - Spec `001-abm-categorias` **aprobada e implementada**; **T1–T14 hechas y validadas** (`dotnet build` limpio, `dotnet test` 64/64) — líneas 7, 8, 30.
  - **Único pendiente: la validación visual del usuario** — líneas 8 y 30.
- **Fase Plan/Tareas (§3 del archivo original)** → consolidado:
  - `plan.md` actualizado (sin "pendiente de aprobación", con gating por rol y refresco del combo) y `tasks.md` cerrado — líneas 7 y 8.
  - Corte del reencuadre: T11 → T13 → T12 → T14 ✅ (2026-10-19) — línea 30.

## 2. Lo que FALTA volcar a `MEMORY.md` (agente sin permiso de escritura fuera de `specs/`)

El **roadmap global** no está registrado en `MEMORY.md`. Volcar (sugerido como ítem nuevo en §"Estado Actual"):

> - **Roadmap global creado (2026-10-19)**: `specs/000-roadmap.md` fija el orden de las specs (001 categorías → 002 Productos → 003 Clientes → 004 Proveedores → 005 Ventas → 006 Compras → 007 Reportes), los nombres de carpeta (`005-ventas`, `006-compras`), el **menú principal congelado** (ninguna spec futura 002–007 lo modifica) y la vista **`Info` como centro de ayuda ESTÁTICO** sin backend ni spec propia (revisión editorial de cierre tras Reportes). Una funcionalidad a la vez, con aprobación del usuario en cada fase. No quedan dudas abiertas del roadmap.

**Estado de permiso**: el agente planificador intentó editar `MEMORY.md` y recibió **permiso denegado** (solo escribe dentro de `specs/`). Por eso este punto queda anotado aquí para que lo aplique quien tenga permiso.

## 3. Conclusión

- **Del contenido original de este archivo: TODO consolidado** en `MEMORY.md` (sin pérdidas).
- **Lo único pendiente de volcar** es el **punto del roadmap global** de la sección §2 de este archivo.
- Sin dudas abiertas del roadmap (D-R1 orden/nombres, D-R2 menú congelado, D-R3 vista Info estática); solo siguen las dudas funcionales propias de cada spec futura (002–007).
