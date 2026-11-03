# Nota para `MEMORY.md` — correcciones QA de la spec 002 (2026-10-22)

> El agente planificador solo escribe dentro de `specs/`. Quien tenga permiso para editar
> `MEMORY.md` debe incorporar este resumen al aprobarse/implementarse la spec `002-abm-productos`.
> No contiene datos sensibles.

## Contexto
Tras la revisión QA de `specs/002-abm-productos`, el usuario cerró 6 hallazgos. Se corrigieron
`spec.md`, `plan.md` y `tasks.md`. **No se tocó código C#.**

## Nuevas decisiones cerradas (numeración desde D-17, sin renumerar)
- **D-18**: `Producto.id_usuario` (usuario creador) es **`NOT NULL`**. La migración `AddProductConstraints`
  debe **rellenar los 12 productos de la semilla** (`scripts/seed-desarrollo.sql`, códigos `779…`) con el
  **usuario Administrador (DNI `90000001`)** antes de aplicar el `NOT NULL`.
- **D-18-bis**: el **Vendedor NO ve la pestaña "Productos"**; el módulo "Productos" **no** se habilita en
  `PoliticaAcceso` (queda Admin/Encargado). Su lectura del catálogo es **indirecta desde Ventas** (spec 005).
  **No** se toca `PoliticaAcceso` ni el test `TieneAcceso_Vendedor_DevuelveSoloVentasYClientes` (siguen válidos).
  La restricción de escritura por rol se valida en **`CapaNegocio`** (`PuedeGestionarProductos`).
- **D-19**: la advertencia `PrecioVenta < PrecioCompra` se muestra **ANTES de guardar**; cancelar ⇒ **no se persiste nada**.
- **D-20**: un **producto inactivo es editable sin reactivarlo** (mismo patrón que Categorías).
- **D-21**: auditoría = **solo el último evento** (usuario/fecha/motivo se sobrescriben; sin historial en esta spec;
  el historial va a 005/006).

## Hallazgo 5 (sin decisión nueva, verificado en código real)
`Producto.Codigo` es `string?` → columna `codigo` **`nvarchar(max)`** ya **texto**; la semilla usa códigos de 13
dígitos `N'779…'` con ceros significativos. Conclusión: **no se cambia el tipo**; solo se restringe el largo a
**`nvarchar(13)`** con índice único filtrado (`HasMaxLength(13)`), coherente con D-9. Preserva ceros a la izquierda.

## FK `PRODUCTO → CATEGORIA`
La migración inicial tiene `onDelete: Cascade` (contradice "sin DELETE físico"). Se decide **corregir a
`Restrict`/`NoAction`** en la migración `AddProductConstraints` (decisión D-12 del plan).

## Migración nueva `AddProductConstraints`
Justificada; **requiere aprobación explícita del usuario** antes de crearse/ejecutarse (AGENTS.md).
Incluye: `HasMaxLength(13)` + índice único filtrado de `codigo` (texto), longitudes `HasMaxLength(100/300)`,
campos de auditoría (`id_usuario` **`NOT NULL`** con relleno de semilla; `id_usuario_modifica`/`fecha_modificacion`/
`motivo_ajuste_stock` nullable), corrección de la FK a `Restrict`. Sincronizar con `dotnet ef database update`
antes de probar la UI.

## Testabilidad
`dotnet test` en `CapaNegocio` cubre RF-1…RF-15, RF-17, RF-18 (permiso de escritura) y RF-20. Requieren **BD real
o validación manual**: RF-16, RF-19, los respaldos de BD de RF-2/RF-3 (índice único), el `NOT NULL` real (D-18) y la
FK `Restrict`. Los RF visuales se validan manualmente. `InMemory` no simula índice único, `NOT NULL`/FK ni el reloj del servidor.

## Dudas abiertas nuevas
Ninguna. Todas las respuestas del usuario se aplicaron; no se inventó funcionalidad.
