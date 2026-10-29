# T10 — Procedimiento de verificación de atomicidad (SQL Server) + checklist RNF

> **Estado**: procedimiento reproducible (según `tasks.md` líneas 93–109 y `plan.md` §9.1). **No se inventan resultados**: se indica cómo reproducir paso a paso contra SQL Server real; la ejecución con datos reales es pendiente de T10.
> **Nota crítica**: `InMemory` **no ejecuta rollback real** (`plan.md` §9.1). Por eso el procedimiento requiere una base SQL Server real (`DB_SISTEMA_VENTA`) y debe verificarse con `SELECT COUNT(*)` sobre la conexión real tras cada paso.

---

## 1. Procedimiento reproducible de atomicidad (pasos 1–8)

### Paso 0 — Preparación de referencia
- Confirmar que el esquema `COMPRA` / `DETALLE_COMPRA` está intacto (`git status` sin cambios en `CapaDatos/Migrations/`; RNF-10).
- Confirmar `dotnet build` limpio y `dotnet test` completo en verde (sin regresiones tras T1–T9a).
- Anotar el `MAX(id_compra)` actual: `SELECT MAX(id_compra) FROM COMPRA;` ⇒ `max_previo`. Si no hay filas, `max_previo = 0`.

### Paso 1 — Preparar datos base
- Elegir un proveedor activo (`P`) y dos productos activos (`P1`, `P2`) ya sembrados en la base.
- Anotar sus `id_proveedor`, `id_producto` y el `SELECT MAX(id_compra) FROM COMPRA;` ⇒ `max_previo` (antes del paso 2).

### Paso 2 — Caso bueno (transacción completa)
- Desde `ComprasForm`, registrar una compra de **dos líneas** (`P1`, `P2`) con cantidades y precios válidos.
- Verificar en SQL Server real (nueva conexión):
  - `SELECT COUNT(*) FROM COMPRA WHERE id_compra > max_previo;` ⇒ **1** fila (cabecera).
  - `SELECT COUNT(*) FROM DETALLE_COMPRA WHERE id_compra > max_previo;` ⇒ **2** filas (líneas).
  - `SELECT MontoTotal FROM COMPRA WHERE id_compra > max_previo;` ⇒ coincide con la suma de subtotales.

### Paso 3 — Restablecer referencia
- `max_previo = SELECT MAX(id_compra) FROM COMPRA;` (tras la compra del paso 2).

### Paso 4 — Provocar fallo simulado con trigger temporal
Crear el trigger sobre `DETALLE_COMPRA` (se elimina después; nunca debe quedar en producción):

```sql
CREATE TRIGGER TR_FALLO_SIMULADO_DETALLE ON DETALLE_COMPRA
AFTER INSERT AS
BEGIN
    IF EXISTS (SELECT 1 FROM inserted WHERE id_producto = <id_producto_de_P2>)
        THROW 51000, 'Fallo simulado de la verificación de atomicidad', 1;
END;
```
Nota: usar el `id_producto` real del paso 1 para `P2`.

### Paso 5 — Ejecutar caso con fallo
- Registrar desde la pantalla (`ComprasForm`) otra compra de **dos líneas** (`P1`, `P2`), en el mismo orden.
- La cabecera y la primera línea se insertan; el `INSERT` de la segunda (`P2`) debe fallar por el trigger.
- La aplicación debe mostrar (solo por `CompraNegocio`): `«No se pudo completar la operación por un problema con el origen de datos. Intente nuevamente.»` (texto único de `MensajeErrorPersistencia`, §5, RNF-8).
- **Conservar** en pantalla: proveedor elegido, líneas cargadas, precios y totales intactos (RNF-11); sin excepción propagada (RF-56); sin mensaje de límite como error de persistencia (RNF-8).

### Paso 6 — Restaurar (eliminar trigger)
```sql
DROP TRIGGER TR_FALLO_SIMULADO_DETALLE;
```
Verificar eliminación:
```sql
SELECT name FROM sys.triggers WHERE name = 'TR_FALLO_SIMULADO_DETALLE';
```
Resultado esperado: **0 filas**.

### Paso 7 — Comprobar rollback (0 / 0 y sin líneas huérfanas)
- Desde una **nueva conexión** SQL Server (para evitar transacciones abiertas en memoria):
  - `SELECT COUNT(*) FROM COMPRA WHERE id_compra > max_previo;` ⇒ **0**.
  - `SELECT COUNT(*) FROM DETALLE_COMPRA WHERE id_compra > max_previo;` ⇒ **0**.
  - `SELECT COUNT(*) FROM DETALLE_COMPRA d WHERE NOT EXISTS (SELECT 1 FROM COMPRA c WHERE c.id_compra = d.id_compra);` ⇒ **0** (sin líneas huérfanas).

> **Nota explícita**: en `InMemory` esta comprobación **no refleja rollback real**; por eso el paso 7 debe realizarse contra SQL Server real (`plan.md` §9.1, nota de `G10`). Si se ejecuta solo en InMemory, debe documentarse como pendiente con la evidencia de que `InMemory` no garantiza rollback físico.

### Paso 8 — Reintentar sin trigger (restauración completa)
- Eliminar el trigger temporal (ya hecho en paso 6).
- Reintentar la misma compra (`P1`, `P2`) y comprobar que se registra completa:
  - `SELECT COUNT(*) FROM COMPRA WHERE id_compra > (max_previo del paso 3);` ⇒ **1** fila.
  - `SELECT COUNT(*) FROM DETALLE_COMPRA WHERE id_compra > (max_previo del paso 3);` ⇒ **2** filas.
- Confirmar que `IdCompra`, `FechaRegistro`, `Estado`, usuario y proveedor coinciden con los datos enviados; que los subtotales coinciden con `CalculoImportesCompra.CalcularSubtotal` (RNF-13); que el `Subtotal` recibido en el DTO fue ignorado y recalculado por el negocio (RNF-13).
- Dejar constancia del resultado en esta plantilla; incluir evidencia (fecha, `max_previo` usado, conteos, mensaje recibido en pantalla).

---

## 2. Checklist RNF (evidencia del código y estructura)

Este checklist **no inventa resultados manuales**: documenta la evidencia de código disponible en T1–T9a, y señala qué puntos permanecen **pendientes de verificación manual (T9b / T10)**.

| # | RNF | Evidencia del código / estructura | Estado de evidencia (✅ verificado / ⬜ pendiente manual) | Notas / referencia exacta |
|---|---|---|---|---|
| RNF-1 | **Decimal, sin `double` / `float`** | `CalculoImportesCompra.CalcularSubtotal` y `CalcularMontoTotal` reciben y devuelven `decimal`; `AnalizadorImporte` trabaja con `decimal`; `AnalizadorCantidad` usa `int` (sin `double`). Ninguna firma usa `float` ni `double`. | ✅ Código verificado | Referencia: `CalculoImportesCompra.cs`, `AnalizadorImporte.cs`, `CompraNegocio.cs` líneas 423, 433, 686, 699. |
| RNF-2 | **Async / await en operaciones I/O** | `RegistrarCompraAsync`, `BuscarProveedoresAsync`, `BuscarProductosAsync`, `ObtenerPreciosProductoAsync`, `ObtenerHistorialAsync`, `ObtenerCompraConDetalleAsync` son `async` con `await`. `CalcularSubtotalEnPreparacion` y `QuitarLinea` son síncronos (no I/O). | ✅ Código verificado | Sin firmas `async` sin `await`. Referencia: `CompraNegocio.cs` líneas 193, 247, 911, 1007, 1182. |
| RNF-3 | **Proyección, sin materializar catálogo completo** | `AsNoTracking()` + `Select` en `BuscarProveedoresAsync`, `BuscarProductosAsync`, `ValidarProductosAsync`, `ObtenerHistorialAsync`, `ObtenerCompraConDetalleAsync`. No `ToList()` sobre tablas completas. | ✅ Código verificado | Referencia: `CompraNegocio.cs` líneas 126, 314, 667, 911, 1007, 1182, 1312. |
| RNF-4 | **Aislamiento 4 capas (UI sin `AppDbContext` ni SQL)** | `ComprasForm.cs` no declara `AppDbContext`; todos los accesos van por `CompraNegocio` (inyectado en `Program.cs` / `Inicio/inicio.cs`). `CapaPresentacion` no contiene SQL. | ✅ Código verificado | Confirmado con `grep` en archivo y con `C1` de T9b. |
| RNF-5 | **DTOs mínimos, sin exponer entidades / claves internas** | `ProveedorSeleccionadoDto`: sin correo ni teléfono; `HistorialCompraDto`: sin datos internos; `LineaCompraDetalleDto`: sin `PrecioVenta`; `RespuestaRegistroCompra`: `IdCompra` sí (excepción documentada §3.3), sin clave interna de detalle. Proyección defensiva (`AsNoTracking` + `Select`). | ✅ Código verificado | Referencia: `spec.md` §3; `CompraNegocio.cs` líneas 1165, 1221, 1312, 1345. |
| RNF-6 | **Reglas de negocio verificadas con datos reales (T9b)** | La plantilla `T9b-checklist.md` documenta los puntos a verificar manualmente (prellenado, acumulación, descarte, historial, formato, permisos). **Pendiente** hasta marcar todos con evidencia real. | ⬜ Pendiente T9b (manual) | No se inventan resultados en esta plantilla. |
| RNF-7 | **UI sin fórmulas ni mensajes propios; `CellFormatting` para formato** | `ComprasForm.cs`: eventos delgados; subtotal/total solo de `CalcularSubtotalEnPreparacion` / `CalcularMontoTotal`; mensajes solo del catálogo (constantes privadas en `CompraNegocio`). `CellFormatting` aplica `N2`, `dd/MM/yyyy HH:mm` y texto de estado (`bool`). | ✅ Código verificado; ⬜ pendiente evidencia visual T9b | Referencia: `ComprasForm.cs` líneas 530, 1089, 1119. |
| RNF-8 | **Mensajes en español, únicos por tipo de error** | Catálogo de mensajes del `plan.md` §5 (`CapaNegocio`): `MensajeErrorPersistencia` único; mensajes de límite, duplicado, permiso, producto inexistente, etc., todos en español y comparados por igualdad en pruebas. No hay alias ni variantes. | ✅ Código verificado | Referencia: `CompraNegocio.cs` constantes privadas (§5). |
| RNF-9 | **Rendimiento con 200 líneas** | Se verificará en **T9b / T10** con datos reales (compra de 200 líneas, editar una línea y percibir actualización del total en < 200 ms). Código actual usa `AsNoTracking` + cálculo síncrono; sin bloqueos de base. | ⬜ Pendiente T9b / T10 (manual) | Nota: `CalculoImportesCompra.CalcularMontoTotal` opera en memoria sobre `IEnumerable<decimal>`. |
| RNF-10 | **Sin cambios de esquema ni migraciones** | `git status` (previo a cualquier cambio): sin archivos nuevos en `CapaDatos/Migrations/`; ningún archivo `.cs` modificado en `CapaEntidad` o `CapaDatos` por esta spec. Confirmación pendiente tras T9b/T10 con datos reales. | ✅ Código (estructura intacta); ⬜ pendiente confirmación `git status` tras verificación real | Referencia: tareas T1–T9a; `plan.md` §2 (tablas sin cambios). |
| RNF-11 | **Líneas intactas ante rechazo** | `RespuestaOperacionLineaCompra.Lineas` es la lista intacta; `RespuestaRegistroCompra` no contiene `Lineas` (solo `IdCompra`, `FechaRegistro`, `MontoTotal`, mensajes, índices afectados). Código verificado en `CompraNegocio.cs` para agregar, editar, quitar, registrar. | ✅ Código verificado | Referencia: `CompraNegocio.cs` líneas 667, 686, 699, 804, 823, 835. |
| RNF-12 | **UI solo orquesta; formato en `CellFormatting`; mensajes del catálogo** | Confirmado: `ComprasForm.cs` no tiene fórmulas ni textos propios; `CellFormatting` aplica formatos; mensajes recibidos son los del catálogo (`RespuestaRegistroCompra.Mensaje`, etc.). | ✅ Código verificado; ⬜ pendiente evidencia visual T9b | Referencia: `ComprasForm.cs` líneas 530, 1089. |
| RNF-13 | **Subtotal recalculado por negocio; DTO `Subtotal` ignorado en registro** | `RegistrarCompraAsync` ignora `Subtotal` recibido (`linea.Subtotal` no se usa para persistencia); recalcula con `CalcularSubtotalEnPreparacion` (`CalculoImportesCompra.CalcularSubtotal`). Confirmado en código. | ✅ Código verificado | Referencia: `CompraNegocio.cs` líneas 866, 911; `plan.md` §7 paso 11. |

---

## 3. Estado pendiente y notas para completar T10

- [ ] **Paso 2 (caso bueno)**: pendiente ejecución con datos reales (SQL Server). Requiere `P`, `P1`, `P2` con datos consistentes y `SELECT COUNT(*)` confirmatorio. Si no se ejecuta, documentar explícitamente como pendiente con la causa (ej. «sin conexión SQL Server real en este entorno»).
- [ ] **Paso 4–8 (trigger y rollback)**: pendiente ejecución con datos reales contra SQL Server. Si se realiza solo en InMemory, documentar la evidencia (conteos 0/0 en InMemory) y la nota de que **InMemory no garantiza rollback físico** (`plan.md` §9.1, `G10`, nota de `Memory.md` o de esta plantilla).
- [ ] **RNF-6, RNF-9**: pendiente verificación manual con datos reales (prellenado visual, acumulación, 200 líneas, percepción < 200 ms, descarte tras registro, formato `CellFormatting`).
- [ ] **RNF-10**: pendiente confirmación `git status` (debe ser limpio tras ejecución manual con datos reales; sin archivos en `Migrations/`).
- [ ] **No modificar código implementado** (T1–T9a) para completar T10; si un hallazgo requiere cambio, debe documentarse como defecto para revisión, no como parche no autorizado.

---

## 4. Referencias de archivo

- Especificación aprobada: `specs/005-carga-compras/spec.md` (2026-10-29, ajuste signo `-`).
- Tareas: `specs/005-carga-compras/tasks.md` (T1–T10; líneas 93–119 para T10; 89–91 para T9b).
- Plan técnico: `specs/005-carga-compras/plan.md` (§2 capas intactas; §3 contratos; §5 catálogo; §6 análisis de precios; §7 pasos transaccionales 2–13; §9.1 procedimientos de verificación manual; §10 presentación).
- Código (sin modificaciones para T9b/T10): `CapaNegocio/CompraNegocio.cs`, `CapaNegocio/Compras/CalculoImportesCompra.cs`, `CapaNegocio/Compras/AnalizadorImporte.cs`, `CapaNegocio/Compras/AnalizadorCantidad.cs`, `CapaPresentacion/Compras/ComprasForm.cs`, `CapaPresentacion/Inicio/inicio.cs`, `Program.cs`.
