# Contexto de traspaso — HU-09 (Compras)

## 1. Estado del repositorio

- Rama: `9-hu-09-carga-de-compras-a-proveedores`. HEAD: `b5d31df` («checkpoint 2: HU-09 compras a proveedores backend»). Sin commits nuevos de los subagentes.
- Última verificación: `dotnet build` 0 errores (18 advertencias preexistentes de NuGet: NU1701 LiveCharts, NU1902/NU1903 Pkcs) y `dotnet test` **771/771**.
- Cambios sin commitear (`git status --porcelain` al momento del traspaso):
  - `CapaPresentacion/Compras/ComprasForm.cs`
  - `CapaPresentacion/Compras/ComprasForm.Designer.cs`
  - `CapaPresentacion/Inicio/inicio.cs`
  - `MEMORY.md`
  - `AGENTS.md`
  - `specs/005-carga-compras/spec.md`
  - `specs/005-carga-compras/tasks.md`
  - `skills-lock.json`
  - Directorio de skills `.agents/skills/ef-core/` sin seguimiento (`??`)

## 2. Trabajo completado (HU-09)

Resumen por tareas: **T1–T9a implementadas** (enums/DTOs, `CalculoImportesCompra`, `AnalizadorImporte`/`AnalizadorCantidad`, `CompraNegocio` con búsquedas, prellenado, detalle en carga, registro transaccional con rollback, historial y detalle, RBAC, `ComprasForm` + `DetalleCompraForm`).

| Tarea | Estado |
| --- | --- |
| T1–T9a | `[x]` implementadas |
| T11, T12, T18, T19, T20 | `[x]` |
| T13 | pendiente |
| T9b | pendiente — verificación manual: pantalla con datos reales |
| T10 | pendiente — verificación manual: atomicidad contra SQL Server con trigger `TR_FALLO_SIMULADO_DETALLE` |
| T21 | pendiente de aprobación (ver §5) |

La documentación de cierre está en `specs/005-carga-compras/T9b-checklist.md`, `specs/005-carga-compras/T10-atomicidad.md` y `specs/005-carga-compras/checklist-rnf.md`.

## 3. Problema abierto: la UI sigue rota en runtime

Síntomas reales (captura del usuario):

1. `dgvLineas`: la columna **Quitar** se sale por la derecha.
2. Footer: **Registrar Compra** y **Ver Historial** solapados/tapados.

## 4. Diagnóstico definitivo (verificado)

- `ComprasForm.cs` **no** configura geometría en runtime: no hay `Load`/`SizeChanged`/`Resize` ni asignaciones a `Width`, `FillWeight`, `AutoSizeMode`, `AutoSizeColumnsMode`, `Dock` ni `MinimumSize` de las grillas. (`AplicarEstiloGrilla` solo toca colores, pero reemplaza los objetos de estilo → riesgo de perder alineación y formato.)
- Causa raíz: `tblPrincipal` es hijo **`Dock = Fill`** con `MinimumSize = 980×650` dentro de `pnlContenedor` (`AutoScroll = true`, `AutoScrollMinSize = (0,650)`, `Padding = 12`), y el `TabPage` suma `Padding = 3`. Un hijo `Fill` con mínimo mayor que el viewport **se recorta sin generar barra**.
- Área útil real: ≈**1053×478** al 100 % y ≈**911×378** al 150 % DPI (`AutoScaleMode.Font`).
- Contenido mínimo actual: 35 + 230 + 22 + 100 + 50 + 55 = **492 px** > 478 ⇒ footer fuera.
- Ancho mínimo 980 + 24 = **1004** > 911 al 150 % ⇒ se recorta la última columna (`colLineaQuitar`, índice 6).
- `colLineaEditar`/`colLineaQuitar`: `AutoSizeMode = None`, `Width = 60`, `MinimumWidth = 60`, `Resizable = false`. `FillWeight` de las 5 columnas Fill: 34/10/14/14/14 = 86 (proporcional, correcto).
- Sin `Dock = Bottom` en toda la capa de presentación.

## 5. Plan T21 revisado (PENDIENTE DE APROBACIÓN — no implementado)

| # | Cambio |
| --- | --- |
| 1 | `tblPrincipal`: `Dock = None` + `Anchor = Top\|Left`, `Location (0,0)`; `SizeChanged` de `pnlContenedor` → `AjustarTamanoTablaPrincipal()` con `Width = Math.Max(850, Client.Width - Padding.Horizontal)` y `Height = Math.Max(452, Client.Height - Padding.Vertical)` (invocarlo también en el constructor). |
| 2 | `pnlContenedor.AutoScrollMinSize = Size.Empty`. |
| 3 | Filas `Absolute 35 / Absolute 220 / Percent 100 / Absolute 55`; `gridLineas.MinimumSize = (0,70)`; `gridLineas.ScrollBars = ScrollBars.Vertical`. |
| 4 | `AplicarEstiloGrilla` con `Clone()` y mutación solo de color/fuente, conservando `Alignment` (importes a la derecha) y `Format = N2`. |
| 5 | Filtrado bidireccional (T21): `gridProveedores_SelectionChanged` → `async void` → `SincronizarProductosDelProveedorAsync(idProveedor)` usando `ObtenerProductosDeProveedorAsync`; bandera `_actualizandoSeleccion` + `SuspenderSeleccion()` (guarda/restaura el valor previo, **no cruza `await`**) y guarda `SeleccionSuspendida => _actualizandoSeleccion \|\| _nivelActualizando > 0`; anti-obsolescencia por id + relectura de `txtBuscarProducto.Text`; `SeleccionarProductoEnGrilla(int?)`. `ResolverProveedoresDelProductoAsync` se conserva. `CapaNegocio` no se toca. |

## 6. Decisiones geométricas tomadas (y por qué)

| Decisión | Motivo |
| --- | --- |
| **`Dock = None` + `Anchor Top\|Left`** en `tblPrincipal` | Es la única forma de que `pnlContenedor.AutoScroll` genere barra. Se descartó `Anchor` a los cuatro lados porque reproduce el recorte. |
| **Fila de búsquedas 220 px (no 190)** | `tblDatosProducto` necesita 216 px + padding; con 190 se recorta «AGREGAR A LA COMPRA». |
| **Ancho mínimo 850 px (no 900)** | Al 150 % DPI quedan ~870 útiles tras la barra vertical; con 900 aparecería scroll horizontal. |
| **Alto mínimo 452 px** | 35 + 220 + 22 + 70 + 50 + 55, deja 2 px de holgura sobre 454. |
| **`ScrollBars.Vertical` en `dgvLineas`** (en vez de `Both`) | La barra horizontal consume ancho y desplaza la última columna. |
| **`AutoScrollMinSize = Size.Empty`** | Deja que el scroll lo determine el tamaño real del hijo, en vez de un número mágico desincronizado. |

## 7. Siguientes pasos exactos (en otra PC)

1. `git status` y `git log --oneline -3` para confirmar HEAD `b5d31df` y que los cambios siguen sin commitear.
2. Leer `.agents/skills/winforms-ui-ux/SKILL.md` y `.agents/skills/desktop-ux-patterns/SKILL.md` (obligatorio antes de tocar la UI); opcional `clean-code`, `modern-csharp`, `ef-core`.
3. Pedir al usuario la aprobación de T21 (paso 3 del protocolo, pausa obligatoria).
4. Con la aprobación: `@implementer` aplica T21 en `CapaPresentacion/Compras/ComprasForm.Designer.cs` y `ComprasForm.cs` (e `inicio.cs` solo si hiciera falta).
5. `@reviewer` ejecuta `dotnet build` y `dotnet test` (esperado 771/771) y verifica: «Quitar» visible a 1053×478, los 3 botones del pie dentro del viewport, scroll solo vertical al 150 % DPI, sin cascadas de consultas.
6. **No hacer commit**: HEAD se mantiene en `b5d31df` hasta que el usuario lo autorice (AGENTS.md lo prohíbe).
7. Verificación visual final del usuario; luego actualizar `tasks.md` (marcar T21) y `MEMORY.md`.
8. Pendientes posteriores: T9b (checklist visual con datos reales), T10 (atomicidad SQL Server con `TR_FALLO_SIMULADO_DETALLE`) y toda la HU-10.

## 8. Advertencias

- No implementar HU-10 (stock/precios/transacción) ni cambiar el esquema.
- No confundir `CargarHistorialAsync` de `DetalleCompraForm` (uso interno del modal) con el método homónimo eliminado de `ComprasForm`.
- Los snapshots `Size`/`Location` del Designer están desfasados unos píxeles respecto de `RowStyles`/`ColumnStyles`: el runtime los recalcula, pero Visual Studio podría re-serializarlos al abrir el diseñador.
