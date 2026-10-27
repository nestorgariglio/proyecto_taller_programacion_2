# MEMORY.md - Estado del Proyecto
Memoria entre sesiones (máximo ~50 líneas). Resume o elimina lo que ya no aporte.

## Estado Actual
- **Specs 001 (`abm-categorias`), 002 (`abm-productos`) y 003 (`productosform-bugfix`)**: implementadas y cerradas; migraciones aplicadas (`AddProductConstraints`), build limpio y validación visual confirmada. Las categorías se gestionan desde `ProductosForm` en `GestionCategoriasModalForm`.
- **Spec `004-consulta-stock` (HU-06 Consulta de Stock e Inventario Crítico) — implementada T1–T9 + correcciones de la fase 6 (C1, C2, C4, C5)**: sin migración, sin cambios de `PoliticaAcceso` ni del menú. `dotnet build` con **0 errores**; `dotnet test` **230/230**. **T10 abierta**: solo falta la validación visual del usuario (incluida la comprobación visual de RNF-1).
- **Spec `001-abm-categorias` — ronda de correcciones 2026-10-26 (O-1 y O-2) implementada en T15–T17**: la baja de categoría se resuelve en `CapaNegocio` (`ObtenerConfirmacionBajaAsync` + DTO `ConfirmacionBajaCategoria`) y la validación de unicidad se resuelve en la base con una sola consulta. `dotnet build` 0 errores; `dotnet test` **237/237**; validación del revisor: **RONDA APTA**; queda la validación visual del diálogo de baja.
- **BD de desarrollo**: `DB_SISTEMA_VENTA` con esquema por migraciones y semilla; `scripts/preparar-bd.ps1` idempotente. Baseline real: `Categorías: 7 (activas: 5) | Productos: 12 (activos: 12)`; la 7.ª es `Tecnologia` (inactiva, alta manual de la 001).
- **Repositorio**: PR #18 sincronizado en `main`; **los cambios de la spec 004 están sin commitear**.

## Qué quedó implementado (spec 004)
- `CapaNegocio`: `ListarProductosAsync(filtro, umbralStock = null)`, `ContarProductosAsync`, `ClasificarCriticidad`, `PriorizarCodigoExacto`, `NivelCriticidadStock`, `ResolucionUmbralStock`.
- DTOs aditivos: `FiltroProductosDto.SoloCriticos` y `ProductoRespuestaDto.NivelCriticidad`.
- `CapaPresentacion/Productos/ProductosForm.cs`: umbral desde `appsettings.json` leído **una vez** en el constructor, 2 controles nuevos (casilla "Solo críticos" + texto del umbral), resaltado D-17, indicador en `colStock`, selección por borde (D-18) y mensajes de vacío dibujados en `Paint`.
- `appsettings.json`: `StockCritico:UmbralMinimo = "5"`.

## Decisiones clave (spec 004)
- **D-2**: umbral global por configuración, sin stock mínimo por producto ⇒ sin migración.
- **D-10**: si falta la clave se usa el fallback interno **5** sin aviso; el modo degradado aplica **solo** si el valor existe y es inválido.
- **D-13**: "Solo críticos" arranca **apagado** y persiste al cambiar de pestaña.
- **D-16**: mensaje de vacío dibujado sobre la grilla en su `Paint`, sin tercer control (RNF-5: **2** controles nuevos).
- **D-17**: agotado `#B71C1C` (inactivo `#6E2020`), bajo umbral `#F9A825` (inactivo `#8C6D0A`), texto blanco en los cuatro casos.
- **D-18**: la selección sobre una fila resaltada se marca con **borde de 2 px**; el texto sigue **blanco**.

## Decisiones clave (spec 001, ronda 2026-10-26)
- **O-1**: la regla y el texto de la baja de categoría viven en el negocio (`ObtenerConfirmacionBajaAsync`); la vista solo presenta el mensaje. Sustituye a D-3 del plan 001.
- **O-2**: unicidad con `UPPER(descripcion)` en la BD, una sola fila, sin materializar la tabla. Sustituye a D-6 del plan 001; contrapartida aceptada: el `UPPER` no es *sargable*.

## RNF-1 (≤ 500 ms con 1.000 productos)
- Medido con la base aparte **`DB_SISTEMA_VENTA_PERF`** (migraciones + 1.000 productos): listado completo **mediana 15,2 ms / máx 36,3 ms**; texto "Leche" 14,2 ms; categoría 11,0 ms; "Solo críticos" 12,2 ms; `ContarProductosAsync` 0,7 ms.
- Es **cota inferior**: cubre consulta + mapeo, **sin** el pintado de la grilla. Para apuntar la app a esa base se usa la variable de entorno **`ConnectionStrings__CadenaSQL`**, sin editar `appsettings.json`.

## Aprendizajes y Errores a Evitar
- **MaterialSkin 2.3.1**: el tipo real es **`MaterialCheckbox`** (b minúscula), no `MaterialCheckBox`.
- El `tasks.md` de la 004 menciona el filtro `~ProductoNegocioConsultaStockTests`, que devuelve 0 coincidencias porque la clase es `partial class ProductoNegocioTests`; los tests sí corren y pasan.
- **`sqlcmd`**: al cargar datos usar `-I` (índices filtrados) y `-f 65001` (UTF-8); los `.sql` del repo van con BOM.
- **DI**: `ProductosForm` se resuelve por DI y `IConfiguration` ya está registrada en el host; `Program.cs` no se toca.
- MaterialSkin pinta con el `BackColor` del padre: **nunca `Color.Transparent`** en paneles internos, y sin cascadas de `Refresh()`/`Invalidate()` (regla de la 003).
- `AppDbContext.HasTrigger("TR_PRODUCTO_FECHA_MODIFICACION")` no debe revertirse. Antes de cualquier `DELETE` físico, **consultar al usuario**.
- No materializar tablas completas para validar unicidad ni justificar ese patrón por la portabilidad de los tests: filtrar siempre en la BD.
- Al sustituir una decisión de diseño, actualizar también los **comentarios XML** que la citan (quedó uno obsoleto en `ContarProductosActivosAsync`).

## Próximos Pasos
- **T10**: validación visual del usuario — colores D-17, escala de grises, fila seleccionada (borde D-18), mensajes de vacío, modo degradado con valor inválido, categoría que desaparece y "Actualizar".
- **Validación visual del diálogo de baja de categorías** (ronda 001): con/sin productos activos, categoría ya inactiva, categoría inexistente, cancelar y confirmar.
- **Opcional, fuera de alcance**: `ProductosForm.cs:843` todavía compone el texto de la baja **de productos** en la vista (equivalente a O-1, para la spec 002).
- **Commit de la spec 004** cuando el usuario lo autorice (hoy todo está sin commitear).
- Sin pendientes funcionales en 001–003; sigue abierta la decisión sobre la categoría inactiva `Tecnologia` (dejarla y actualizar el baseline, o borrarla con autorización explícita).
