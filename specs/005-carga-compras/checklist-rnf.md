# Checklist RNF — Spec 005 (Cierre de T9b / T10)

Estado: verificación con evidencia del código (T1–T9a completo; código C# intacto). **No se inventan resultados manuales**: los puntos que requieren datos reales (T9b / T10) se marcan `⬜ pendiente` y se remiten al procedimiento reproducible (`T10-atomicidad.md`, que ejecuta el usuario).

> **Referencias actualizadas el 2026-11-01** (solo números de línea, métodos, archivos y rutas; **no se cambiaron estados ni conclusiones**). Cambios de contexto que explican las correcciones: el historial ya no está embebido en `ComprasForm`; vive en el modal `DetalleCompraForm`, abierto con «Ver Historial» (D-4/RF-52 modificados 2026-11-01). Las líneas citadas son las de los archivos tal como están hoy; si el código cambia, deben rehacerse.

---

## Evidencia por RNF (referencia exacta al archivo y línea)

### RNF-1 — Decimal (`CalculoImportesCompra`, `AnalizadorImporte`)
- `CapaNegocio/Compras/CalculoImportesCompra.cs`: constantes `CantidadMinima`, `CantidadMaxima`, `PrecioMinimo`, `PrecioMaximo`, `MontoTotalMaximo`, `DecimalesPrecio` (líneas 12–27); `CalcularSubtotal(int, decimal)` (línea 33) y `CalcularMontoTotal(IEnumerable<decimal>)` (línea 41) devuelven `decimal`. Sin `float`/`double`.
- `CapaNegocio/Compras/AnalizadorImporte.cs`: `Analizar(string? texto)` (línea 23) devuelve `ResultadoAnalisisImporte`. Todo `decimal`; `1.000` = `1` (no `double`).
- `CapaNegocio/CompraNegocio.cs`: línea 427 (`CalcularSubtotal` en `RegistrarCompraAsync`), 437 (`CalcularMontoTotal` en `RegistrarCompraAsync`), 690 y 703 (`CalcularMontoTotal` en `AgregarLineaAsync`, con acumulación y con línea nueva), 808 (`CalcularMontoTotal` en `EditarLineaAsync`), 827 y 839 (`QuitarLinea`), 870 (`CalcularSubtotal` en `CalcularSubtotalEnPreparacion`), 1662 (`CalcularSubtotal` en `CrearLinea`) y 1694 (`CalcularMontoTotal` en `Rechazo`). Consultas con `AsNoTracking`: líneas 130, 197, 251, 915, 1011, 1044, 1377, 1416, 1520. Solo `decimal`; sin redondeos ni truncamientos arbitrarios.
- **Estado**: ✅ verificado en código; sin cambios requeridos.

### RNF-2 — Async / await en I/O
- `CapaNegocio/CompraNegocio.cs`: `BuscarProveedoresAsync` (línea 112), `BuscarProductosAsync` (línea 179), `ObtenerPreciosProductoAsync` (línea 246), `RegistrarCompraAsync` (línea 325), `AgregarLineaAsync` (línea 594), `EditarLineaAsync` (línea 712), `ObtenerHistorialAsync` (línea 898) y `ObtenerCompraConDetalleAsync` (línea 997). Todos `async` con `await` en consultas (`FirstOrDefaultAsync`, `ToListAsync`, `SaveChangesAsync`, `CommitAsync`).
- `CalcularSubtotalEnPreparacion` (línea 848) y `QuitarLinea` (línea 816) son síncronos (no hacen I/O, conforme al plan §4.2); no hay firmas `async` sin `await`.
- **Estado**: ✅ verificado; ninguna regresión tras T1–T9a (`dotnet test` verde).

### RNF-3 — Proyección + `AsNoTracking` (sin materializar catálogo)
- Búsquedas (`BuscarProveedoresAsync`, `BuscarProductosAsync`): `.AsNoTracking()` + `.Select(...)` con `Take(20)` (líneas 130–143 y 197–210); sin `.ToList()` sobre tablas completas.
- `ValidarProductosAsync` (línea 1510, `AsNoTracking` en 1520): `.Where(...).ToListAsync()` solo sobre los `ids` enviados (`Contains`), no sobre `Productos` completo. `ValidarProveedorAsync` (línea 1369) y `ValidarUsuarioAsync` (línea 1408) también usan `AsNoTracking`.
- `ObtenerHistorialAsync` (`AsNoTracking` en la línea 915) y `ObtenerCompraConDetalleAsync` (líneas 1011 y 1044): proyección defensiva (`Select`) con textos fijos para navegaciones nulas (`«(Sin proveedor)»`, `«(Sin usuario)»`, constantes en las líneas 85–86).
- **Estado**: ✅ verificado; referencia `CompraNegocio.cs` líneas 130, 197, 251, 915, 1011, 1044, 1377, 1416, 1520.

### RNF-4 — Aislamiento de 4 capas (UI sin `AppDbContext` ni SQL)
- `CapaPresentacion/Compras/ComprasForm.cs` y `CapaPresentacion/Compras/DetalleCompraForm.cs`: sin declaración de `AppDbContext`; sin `using CapaDatos;`; todos los accesos a datos a través de `CompraNegocio` (inyectado por constructor; `DetalleCompraForm` lo recibe de `ComprasForm`, línea 924 de `ComprasForm.cs`).
- `CapaNegocio/CompraNegocio.cs`: recibe `AppDbContext` por constructor (línea 97, `ArgumentNullException.ThrowIfNull`), como único acceso al esquema.
- `CapaPresentacion/Program.cs`: `AddTransient<CompraNegocio>()` (línea 40) y `AddTransient<ComprasForm>()` (línea 47). `CapaPresentacion/Inicio/inicio.cs`: `ComprasForm` recibido por DI (líneas 31 y 80), `AbrirVistaEnTab(_comprasForm, tab_compras)` (línea 111) y `_comprasForm.AplicarSesion(...)` (línea 284); sin `new ComprasForm()` ni acceso a contexto.
- **Estado**: ✅ verificado (`Grep` de `AppDbContext` y `using CapaDatos` sobre `CapaPresentacion/Compras/` sin coincidencias).

### RNF-5 — DTOs mínimos (sin exponer entidades / datos internos)
- `CapaNegocio/DTOs/Compras/ProveedorSeleccionadoDto.cs`: `IdProveedor`, `RazonSocial`, `Documento` (`Cuit`); sin correo ni teléfono (plan §3.1).
- `CapaNegocio/DTOs/Compras/HistorialCompraDto.cs`: `IdCompra`, `FechaRegistro` (`DateTime`), `RazonSocialProveedor`, `NombreUsuario` (nombre + apellido recortado), `MontoTotal` (`decimal`), `Estado` (`bool`); sin `IdProveedor`, sin datos de usuario internos.
- `CapaNegocio/DTOs/Compras/LineaCompraDetalleDto.cs`: `CodigoProducto`, `NombreProducto`, `Cantidad`, `PrecioCompra`, `Subtotal`; **sin `PrecioVenta`** (plan §3.4 / D-2 / D-17).
- `CapaNegocio/DTOs/Compras/RespuestaRegistroCompra.cs`: `IdCompra` es la excepción documentada (RNF-5, §3.3); sin `IdDetalleCompra`, sin entidad interna.
- **Estado**: ✅ verificado; referencia `spec.md` §3 y `CapaNegocio/DTOs/Compras/*.cs`.

### RNF-6 — Verificación con datos reales (manual, T9b)
- Plantilla `T9b-checklist.md` documenta los puntos a verificar manualmente: rótulos, retiro de maqueta, búsqueda con texto vacío, prellenado editable (`RF-38`), acumulación (`RF-43` / `RF-63`), reemplazable (`RF-12`), descarte tras registro (`RF-32`), formato (`RNF-7`, `RNF-12`), permisos (`RF-8`), historial (`RF-52`), rendimiento (`RNF-9`), sin `AppDbContext` en UI (`RNF-4`). Su punto 8 (historial embebido en la pantalla) está **obsoleto**; la verificación vigente del historial es **V-28** de `checklist-visual-unificado.md` (historial en el modal `DetalleCompraForm`; `tasks.md`, entrada T9b).
- **Estado**: ⬜ pendiente verificación manual con datos reales (no se inventan resultados). Remitido a `T9b-checklist.md` y a `checklist-visual-unificado.md` (cada punto con evidencia: fecha + datos + observación visual).

### RNF-7 — UI sin fórmulas ni mensajes propios; `CellFormatting`
- `CapaPresentacion/Compras/ComprasForm.cs`: eventos delgados (plan §10.3); subtotal del área solo de `CalcularSubtotalEnPreparacion` (línea 636) y total solo de `CalculoImportesCompra.CalcularMontoTotal(_lineas.Select(linea => linea.Subtotal))` (línea 826); mensajes recibidos de las respuestas del negocio (p. ej. `respuesta.Mensaje`, línea 878) o del catálogo; sin `MessageBox` con texto local de límite/acumulación (el texto de confirmación de limpiar viene de `ObtenerMensajeConfirmacionCancelarCompra()`, línea 904).
- `CellFormatting`: en `ComprasForm.cs`, `gridLineas_CellFormatting` (línea 1208) aplica `N2` a `PrecioCompra`, `PrecioVenta` y `Subtotal` y compone «código — nombre» del producto. El formato de **fecha** `dd/MM/yyyy HH:mm` (`FormatoFecha`, línea 36), de **importes** `N2` (`FormatoImporte`, línea 37) y el texto **«Activa»/«Inactiva»** de `Estado` (`bool`) se aplican en `CapaPresentacion/Compras/DetalleCompraForm.cs`: `gridListaCompras_CellFormatting` (línea 343; fecha 356, importe 361, estado 366) y `gridDetalle_CellFormatting` (línea 309; importes 327–335).
- **Estado**: ✅ código verificado; ⬜ pendiente evidencia visual con datos reales (`T9b-checklist.md`, puntos `C2`, 7, 12; historial: V-28 de `checklist-visual-unificado.md`).

### RNF-8 — Mensajes en español, únicos por tipo de error (`CapaNegocio` catálogo §5)
- `CapaNegocio/CompraNegocio.cs`: constantes privadas (líneas 21–93: `MensajeErrorPersistencia` en la línea 88, mensaje de línea inexistente, de producto repetido, de precio, cantidad, acumulación, límite, sin ítems, permiso denegado, etc.). Cada valor `ErrorPersistencia` es único; no hay alias ni variantes; no se usa para informar errores de validación.
- Todos los mensajes en español; comparados por igualdad en pruebas (`G2b`, `G4`, `G5`, `G9`, `G12`, `G13`).
- **Estado**: ✅ verificado; referencia `CompraNegocio.cs` líneas 21–93 y `plan.md` §5 (catálogo completo).

### RNF-9 — Rendimiento con 200 líneas (medido manualmente)
- Código: `CalcularSubtotalEnPreparacion` (`CompraNegocio.cs`, línea 848) opera en memoria sobre los textos recibidos; `CalcularMontoTotal` (`CalculoImportesCompra.cs`, línea 41) suma `Subtotales`; `AsNoTracking` evita materialización del catálogo; sin bloqueos de base en lectura. Nota de lectura del código: `EditarLineaAsync` (línea 712) valida el producto con una consulta acotada (`ValidarProductosAsync`, línea 727) al guardar la edición.
- **Nota (2026-11-01)**: la verificación manual con 200 líneas y < 200 ms fue **descartada por decisión del usuario**; `T10-atomicidad.md` ya no la incluye.
- **Estado**: SIN VERIFICAR (descartada por decisión del usuario, 2026-11-01).

### RNF-10 — Sin cambios de esquema ni migraciones (`git status`)
- `CapaDatos/AppDbContext.cs`: intacto; `CapaDatos/Migrations/`: sin archivos nuevos ni modificados por esta spec. Ningún archivo `.cs` en `CapaEntidad` o `CapaDatos` fue alterado en T1–T9a.
- `git status` es **acción del usuario** (AGENTS.md §9; ningún agente ejecuta Git), pendiente tras ejecución manual con datos reales (`T10-atomicidad.md`, T10-F, pasos A0 y A9; `tasks.md`, entrada T10, ítem «Sin cambios de esquema (RNF-10)»).
- **Estado**: ✅ estructura intacta; ⬜ pendiente confirmación `git status` tras verificación manual (`T10-atomicidad.md`, T10-F).

### RNF-11 — Líneas intactas ante rechazo (`RespuestaOperacionLineaCompra.Lineas`)
- `AgregarLineaAsync` (línea 594): ante rechazo (duplicado, cantidad inválida, precio inválido, acumulación > máximo, líneas 673–678), la respuesta se arma con `Rechazo(...)` (línea 1680), que devuelve `Lineas` idéntica a la recibida, sin líneas «a medias».
- `RespuestaRegistroCompra` (`CapaNegocio/DTOs/Compras/RespuestaRegistroCompra.cs`): **no** contiene `Lineas`; solo `Resultado`, `Mensaje`, `IdCompra`, `FechaRegistro`, `MontoTotal`, índice/producto/campo/motivo afectados. `RechazoRegistro` (línea 1704) arma el rechazo del registro.
- `QuitarLinea` (línea 816): ante índice fuera de rango (`LineaNoEncontrada`), `Lineas` intacta y `Exito = false` (líneas 820–829). `CalcularSubtotalEnPreparacion` está en la línea 848.
- **Estado**: ✅ verificado; referencia `CompraNegocio.cs` líneas 594–841 y 1680–1721.

### RNF-12 — Formatos en `CellFormatting`; mensajes del catálogo; sin datos internos en lectura
- `CellFormatting`: `N2` (importes), `dd/MM/yyyy HH:mm` (fechas) y texto `«Activa»`/`«Inactiva»` (`bool`). `CapaPresentacion/Compras/ComprasForm.cs`: `gridLineas_CellFormatting` (línea 1208; importes de la grilla de líneas). `CapaPresentacion/Compras/DetalleCompraForm.cs`: `gridListaCompras_CellFormatting` (línea 343; fecha, importe y estado del historial) y `gridDetalle_CellFormatting` (línea 309; importes del detalle). `ComprasForm.cs` ya no tiene `gridHistorial_CellFormatting`: el historial en pantalla se retiró y vive en el modal.
- Proyección defensiva (`AsNoTracking` + `Select`): `ObtenerHistorialAsync` (línea 898) y `ObtenerCompraConDetalleAsync` (línea 997) usan `Select(...)` con `AsNoTracking()`; sin materialización de `Proveedor` ni `Usuario` completos; sin datos internos expuestos.
- **Estado**: ✅ código verificado; ⬜ pendiente evidencia visual con datos reales (`T9b-checklist.md`, puntos `C2`, 12; historial: V-28 de `checklist-visual-unificado.md`).

### RNF-13 — `Subtotal` recalculado; `Subtotal` del DTO ignorado en registro
- `SolicitudRegistrarCompraDto.Lineas`: `Subtotal` recibido por línea es ignorado en `RegistrarCompraAsync`; se recalcula con `CalculoImportesCompra.CalcularSubtotal(linea.Cantidad, linea.PrecioCompra)` (`CompraNegocio.cs`, línea 427) y se guarda el valor recalculado (`Subtotal = subtotales[i]`, línea 474). La pantalla obtiene el subtotal del área de `CalcularSubtotalEnPreparacion` (línea 870) y el de cada línea de `CrearLinea` (línea 1662).
- `CalcularMontoTotal` opera sobre los subtotales recalculados (`subtotales.Add(...)`, línea 427; `CalcularMontoTotal(subtotales)`, línea 437); el `Subtotal` del DTO no afecta ni la persistencia (`DetalleCompra.Subtotal`) ni el total (`Compra.MontoTotal`). Confirmado en las líneas 325–516 (`RegistrarCompraAsync`).
- **Estado**: ✅ verificado; referencia `plan.md` §7 paso 11 y `CompraNegocio.cs` líneas 427, 437, 474, 870, 1662.

---

## Confirmaciones finales (antes de cerrar T10)

- [ ] `dotnet build` limpio (sin errores de compilación; código C# de T1–T9a intacto).
- [ ] `dotnet test` completo en verde (sin regresiones; referencia `tests.md` o ejecución `dotnet test`).
- [ ] `git status` sin archivos nuevos en `CapaDatos/Migrations/` y sin modificaciones en `CapaEntidad` / `CapaDatos` (RNF-10). **Acción del usuario** (AGENTS.md §9).
- [ ] `MEMORY.md` actualizado con estado de HU-09 (ver sección correspondiente).
- [ ] Ningún archivo `.cs` modificado por esta tarea (T9b / T10 son documentación y verificación; no se escribe código de implementación).
- [ ] `T9b-checklist.md` documenta que todos los puntos de verificación manual requieren datos reales (`RNF-6`) y que no se inventan resultados.
- [ ] `T10-atomicidad.md` documenta el procedimiento paso a paso (A0–A9 y los procedimientos de doble envío y permisos; RNF-9 queda SIN VERIFICAR por decisión del usuario y T10 ya no la incluye) y señala claramente lo pendiente (ejecución del usuario con datos reales, `InMemory` no garantiza rollback físico, `git status` final como acción del usuario).
