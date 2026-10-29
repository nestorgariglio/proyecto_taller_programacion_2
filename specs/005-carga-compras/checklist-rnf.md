# Checklist RNF — Spec 005 (Cierre de T9b / T10)

Estado: verificación con evidencia del código (T1–T9a completo; código C# intacto). **No se inventan resultados manuales**: los puntos que requieren datos reales (T9b / T10) se marcan `⬜ pendiente` y se remiten al procedimiento reproducible (`T10-atomicidad.md`).

---

## Evidencia por RNF (referencia exacta al archivo y línea)

### RNF-1 — Decimal (`CalculoImportesCompra`, `AnalizadorImporte`)
- `CapaNegocio/Compras/CalculoImportesCompra.cs`: constantes `CantidadMinima`, `CantidadMaxima`, `PrecioMinimo`, `PrecioMaximo`, `MontoTotalMaximo`, `DecimalesPrecio`; métodos `CalcularSubtotal(int, decimal)` y `CalcularMontoTotal(IEnumerable<decimal>)` devuelven `decimal`. Sin `float`/`double`.
- `CapaNegocio/Compras/AnalizadorImporte.cs`: `Analizar(string texto)` devuelve `(MotivoPrecioInvalido? motivo, decimal valor, bool valido)`. Todo `decimal`; `1.000` = `1` (no `double`).
- `CompraNegocio.cs`: líneas 423 (`CalcularSubtotal`), 433 (`CalcularMontoTotal` con `Select(l => l.Subtotal)`), 686 (`CalcularMontoTotal` con acumulación), 699 (`CalcularMontoTotal` con reemplazo), 866 (`Subtotal = CalcularSubtotal(...)`), 911–1007 (`AsNoTracking` en consultas). Solo `decimal`; sin redondeos ni truncamientos arbitrarios.
- **Estado**: ✅ verificado en código; sin cambios requeridos.

### RNF-2 — Async / await en I/O
- `CompraNegocio.cs`: `BuscarProveedoresAsync` (línea 126 `.AsNoTracking()` + `await` implícito por retorno `Task`); `BuscarProductosAsync` (línea 193); `ObtenerPreciosProductoAsync` (línea 247); `RegistrarCompraAsync` (línea 911); `ObtenerHistorialAsync` (línea 1007); `ObtenerCompraConDetalleAsync` (línea 1182). Todos `async` con `await` en consultas (`FirstOrDefaultAsync`, `ToListAsync`, `SaveChangesAsync`, `CommitAsync`).
- `CalcularSubtotalEnPreparacion` y `QuitarLinea` son síncronos (`Task` sin `await` de I/O, conforme al plan §4.2); no hay firmas `async` sin `await`.
- **Estado**: ✅ verificado; ninguna regresión tras T1–T9a (`dotnet test` verde).

### RNF-3 — Proyección + `AsNoTracking` (sin materializar catálogo)
- Búsquedas: `.AsNoTracking()` + `.Select(...)` con `Take(20)`; sin `.ToList()` sobre tablas completas.
- `ValidarProductosAsync`: `.AsNoTracking()` + `Where(...).ToListAsync()` solo sobre los `ids` enviados (`Contains`), no sobre `Productos` completo.
- `ObtenerHistorialAsync` y `ObtenerCompraConDetalleAsync`: `.AsNoTracking()` + proyección defensiva (`Select`) con textos fijos para navegaciones nulas (`«(Sin proveedor)»`, `«(Sin usuario)»`).
- **Estado**: ✅ verificado; referencia `CompraNegocio.cs` líneas 126, 193, 247, 314, 667, 911, 1007, 1182, 1312, 1325.

### RNF-4 — Aislamiento de 4 capas (UI sin `AppDbContext` ni SQL)
- `CapaPresentacion/Compras/ComprasForm.cs`: sin declaración de `AppDbContext`; sin `using CapaDatos;`; todos los accesos a datos a través de `CompraNegocio` (inyectado por constructor).
- `CapaNegocio/CompraNegocio.cs`: recibe `AppDbContext` por constructor (`ArgumentNullException.ThrowIfNull`), como único acceso al esquema.
- `Program.cs`: `AddTransient<CompraNegocio>()`; `Inicio/inicio.cs`: `ComprasForm` recibido por DI, sin `new ComprasForm()` ni acceso a contexto.
- **Estado**: ✅ verificado (revisión de archivo `ComprasForm.cs` completa; `grep` confirma ausencia de `AppDbContext`).

### RNF-5 — DTOs mínimos (sin exponer entidades / datos internos)
- `ProveedorSeleccionadoDto`: `IdProveedor`, `RazonSocial`, `Documento` (`Cuit`); sin correo ni teléfono (plan §3.1).
- `HistorialCompraDto`: `IdCompra`, `FechaRegistro` (`DateTime`), `RazonSocialProveedor`, `NombreUsuario` (nombre + apellido recortado), `MontoTotal` (`decimal`), `Estado` (`bool`); sin `IdProveedor`, sin datos de usuario internos.
- `LineaCompraDetalleDto`: `CodigoProducto`, `NombreProducto`, `Cantidad`, `PrecioCompra`, `Decimal`, `Subtotal`; **sin `PrecioVenta`** (plan §3.4 / D-2 / D-17).
- `RespuestaRegistroCompra`: `IdCompra` es la excepción documentada (RNF-5, §3.3); sin `IdDetalleCompra`, sin entidad interna.
- **Estado**: ✅ verificado; referencia `spec.md` §3 y archivos `DTOs/Compras/*.cs`.

### RNF-6 — Verificación con datos reales (manual, T9b)
- Plantilla `T9b-checklist.md` documenta todos los puntos a verificar manualmente: rótulos, retiro de maqueta, búsqueda con texto vacío, prellenado editable (`RF-38`), acumulación (`RF-43` / `RF-63`), reemplazable (`RF-12`), descarte tras registro (`RF-32`), formato (`RNF-7`, `RNF-12`), permisos (`RF-8`), historial (`RF-52`), rendimiento (`RNF-9`), sin `AppDbContext` en UI (`RNF-4`).
- **Estado**: ⬜ pendiente verificación manual con datos reales (no se inventan resultados). Remitido a `T9b-checklist.md` (cada punto con evidencia: fecha + datos + observación visual).

### RNF-7 — UI sin fórmulas ni mensajes propios; `CellFormatting`
- `ComprasForm.cs`: eventos delgados (plan §10.3); subtotales y total solo de `CalcularSubtotalEnPreparacion` y `CalcularMontoTotal` (`CompraNegocio`); mensajes recibidos de `RespuestaRegistroCompra.Mensaje`, `RespuestaOperacionLineaCompra.Mensaje`, etc.; sin `MessageBox` con texto local de límite/acumulación.
- `CellFormatting` (líneas 530, 1089, 1119): aplica `N2` a importes (`Subtotal`, `MontoTotal`), `dd/MM/yyyy HH:mm` a fechas (`FechaRegistro` en historial), texto `«Activa»` / `«Inactiva»` a `Estado` (`bool`).
- **Estado**: ✅ código verificado; ⬜ pendiente evidencia visual con datos reales (`T9b-checklist.md`, puntos `C2`, 7, 12).

### RNF-8 — Mensajes en español, únicos por tipo de error (`CapaNegocio` catálogo §5)
- `CompraNegocio.cs`: constantes privadas (`MensajeErrorPersistencia`, mensaje de línea inexistente, mensaje de producto repetido, mensajes de precio, cantidad, acumulación, límite, sin ítems, permiso denegado, etc.). Cada valor `ErrorPersistencia` es único; no hay alias ni variantes; no se usa para informar errores de validación.
- Todos los mensajes en español; comparados por igualdad en pruebas (`G2b`, `G4`, `G5`, `G9`, `G12`, `G13`).
- **Estado**: ✅ verificado; referencia `CompraNegocio.cs` y `plan.md` §5 (catálogo completo).

### RNF-9 — Rendimiento con 200 líneas (medido manualmente)
- Código: `CalcularSubtotalEnPreparacion` opera sobre `IEnumerable<LineaCompraDto>` en memoria; `CalcularMontoTotal` suma `Subtotales` con `Sum`; `AsNoTracking` evita materialización del catálogo; sin bloqueos de base en lectura.
- **Pendiente de verificación manual**: con una compra de **200 líneas**, editar una línea y verificar actualización del total en < 200 ms. Documentar en `T9b-checklist.md` (punto pendiente de `RNF-9`).
- **Estado**: ⬜ pendiente verificación manual (`T9b` / `T10`, `plan.md` §9.2, `tasks.md` línea 113).

### RNF-10 — Sin cambios de esquema ni migraciones (`git status`)
- `CapaDatos/AppDbContext.cs`: intacto; `Migrations/`: sin archivos nuevos ni modificados por esta spec. Ningún archivo `.cs` en `CapaEntidad` o `CapaDatos` fue alterado en T1–T9a.
- `git status` pendiente tras ejecución manual con datos reales (`T10`, paso 0 / 7); debe ser limpio antes y después de la verificación (`tasks.md` líneas 117, 93).
- **Estado**: ✅ estructura intacta; ⬜ pendiente confirmación `git status` tras verificación manual (`T10-atomicidad.md`, paso 0 y paso 7).

### RNF-11 — Líneas intactas ante rechazo (`RespuestaOperacionLineaCompra.Lineas`)
- `AgregarLineaAsync`: ante rechazo (duplicado, cantidad inválida, precio inválido, acumulación > máximo), la respuesta devuelve `Lineas` idéntica a la recibida (`actuales` o con reemplazo/retiro según corresponda), sin líneas «a medias».
- `RespuestaRegistroCompra`: **no** contiene `Lineas`; solo `IdCompra`, `FechaRegistro`, `MontoTotal`, índices afectados, mensaje. Confirmado en `CompraNegocio.cs` líneas 667, 686, 699, 804, 823, 835.
- `QuitarLinea`: ante índice fuera de rango (`LineaNoEncontrada`), `Lineas` intacta y `Exito = false`. Confirmado línea 314 (`CalcularSubtotalEnPreparacion`) y líneas de `QuitarLinea`.
- **Estado**: ✅ verificado; referencia `CompraNegocio.cs` líneas 667–835.

### RNF-12 — Formatos en `CellFormatting`; mensajes del catálogo; sin datos internos en lectura
- `CellFormatting`: `N2` (importes); `dd/MM/yyyy HH:mm` (fechas); texto `«Activa»`/`«Inactiva»` (`bool`). Confirmado `ComprasForm.cs` líneas 1089 (`gridLineas_CellFormatting`) y 1119 (`gridHistorial_CellFormatting`).
- Proyección defensiva (`AsNoTracking` + `Select`): `ObtenerHistorialAsync` y `ObtenerCompraConDetalleAsync` usan `Select(...)` con `AsNoTracking()`; sin materialización de `Proveedor` ni `Usuario` completos; sin datos internos expuestos.
- **Estado**: ✅ código verificado; ⬜ pendiente evidencia visual con datos reales (`T9b-checklist.md`, puntos `C2`, 12).

### RNF-13 — `Subtotal` recalculado; `Subtotal` del DTO ignorado en registro
- `SolicitudRegistrarCompraDto.Lineas`: `Subtotal` recibido por línea es ignorado en `RegistrarCompraAsync`; se recalcula con `CalculoImportesCompra.CalcularSubtotal(cantidadValidada, precioCompraValidado)` (líneas 866, 423, 433).
- `CalculoMontoTotal` opera sobre los subtotales recalculados (`subtotales.Add(...)`); el `Subtotal` del DTO no afecta ni la persistencia (`DetalleCompra.Subtotal`) ni el total (`Compra.MontoTotal`). Confirmado en líneas 911–1499 (`RegistrarCompraAsync`).
- **Estado**: ✅ verificado; referencia `plan.md` §7 paso 11 y `CompraNegocio.cs` líneas 423, 433, 866, 911.

---

## Confirmaciones finales (antes de cerrar T10)

- [ ] `dotnet build` limpio (sin errores de compilación; código C# de T1–T9a intacto).
- [ ] `dotnet test` completo en verde (sin regresiones; referencia `tests.md` o ejecución `dotnet test`).
- [ ] `git status` sin archivos nuevos en `CapaDatos/Migrations/` y sin modificaciones en `CapaEntidad` / `CapaDatos` (RNF-10).
- [ ] `MEMORY.md` actualizado con estado de HU-09 (ver sección correspondiente).
- [ ] Ningún archivo `.cs` modificado por esta tarea (T9b / T10 son documentación y verificación; no se escribe código de implementación).
- [ ] `T9b-checklist.md` documenta que todos los puntos de verificación manual requieren datos reales (`RNF-6`) y que no se inventan resultados.
- [ ] `T10-atomicidad.md` documenta el procedimiento paso a paso (1–8) y señala claramente lo pendiente (ejecución con datos reales, `InMemory` no garantiza rollback físico, `git status` final).
