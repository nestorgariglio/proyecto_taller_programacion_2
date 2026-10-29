# Tareas — Spec 005: Carga de Compras a Proveedores

Plan de referencia: `specs/005-carga-compras/plan.md` (propuesto · corregido tras la segunda revisión QA del 2026-10-29)
Spec de referencia: `specs/005-carga-compras/spec.md` — **aprobada (2026-10-29)** · ajuste del signo `-` (2026-10-29)
Regla de trabajo: **una tarea a la vez**, pruebas primero (rojo → verde), `dotnet build` y `dotnet test` en verde antes de marcar la tarea y **parar**.

> **Sin migraciones, sin paquetes NuGet nuevos y sin cambios de esquema.** `CapaEntidad` y `CapaDatos` (`AppDbContext`, `Migrations/`) quedan **intactos** (RNF-10). Ninguna tarea de esta lista requiere aprobación adicional del usuario.
>
> **Marcas**: 🔍 = incluye **verificación manual** (no cubierta por `dotnet test`, RNF-6). **T1 … T8 son 100 % automáticas**; **T9a** se cierra con `dotnet build` y `dotnet test` (su verificación visual es T9b); **T9b y T10** llevan 🔍. Total: **12 tareas** (T1–T4, T5a/T5b, T6–T8, T9a/T9b, T10).
>
> **Orden y dependencias internas**: cada tarea es ejecutable **con lo que existe al empezar**. En particular:
> - **T2 no usa el catálogo de mensajes** (nace en T3): las pruebas de T2 comparan **motivos tipados** y valores de cálculo, y no textos.
> - `CalcularSubtotalEnPreparacion` y `QuitarLinea` viven en `CompraNegocio`, así que su código y sus pruebas están en **T3**, no en T2.
> - `ValidarProveedorAsync`, `ValidarUsuarioAsync` y `ValidarPrecioRecibido` son privados: se crean en **T5a** y se ejercitan solo desde `RegistrarCompraAsync`, que nace en **T5b**.
> - El **paso 1** del plan §7 (chequeo de rol) se activa en **T8**; **T5b, T6 y T7** reciben `rol` pero **no** lo validan, y sus pruebas invocan con **`"Administrador"`** (o `"Encargado"`), de modo que activar el chequeo en T8 no las rompe.
> - El **prellenado es de pantalla**: G5 prueba **solo la acumulación** de RF-43 y el prellenado se verifica manualmente en **T9b/T10**, igual que RF-38.

Comandos de referencia:
- Compilación: `dotnet build`
- Suite completa: `dotnet test`
- Solo compras: `dotnet test --filter "FullyQualifiedName~CompraNegocioTests"`
- Solo permisos: `dotnet test --filter "FullyQualifiedName~PoliticaAccesoTests"`

---

- [x] **T1. `CapaNegocio`: enums y DTOs de Compras.** RF-55, RNF-5, RNF-12
  - Crear `CapaNegocio/Enums/`: `ResultadoBusquedaCompras`, `ResultadoPreciosProducto`, `ResultadoOperacionLineaCompra` (incluye `LineaNoEncontrada`), `ResultadoRegistroCompra`, `ResultadoHistorialCompras`, `ResultadoCompraConDetalle`, `CampoPrecio`, `MotivoPrecioInvalido` (valores exactos del plan §3.5). Un solo valor `ErrorPersistencia` por enum, sin variantes ni alias (plan §3.3).
  - Crear `CapaNegocio/DTOs/Compras/` (namespace `CapaNegocio.DTOs.Compras`): `ProveedorSeleccionadoDto`, `ProductoBuscadoDto`, `PreciosProductoDto`, `LineaCompraDto`, `SolicitudAgregarLineaCompraDto`, `SolicitudEditarLineaCompraDto`, `SolicitudRegistrarCompraDto`, `SubtotalLineaDto`, `RespuestaOperacionLineaCompra`, `RespuestaCompraEnCarga` (con `Exito` y `Mensaje`), `RespuestaBusquedaProveedoresCompra`, `RespuestaBusquedaProductosCompra`, `RespuestaPreciosProducto`, `RespuestaRegistroCompra`, `RespuestaHistorialCompras`, `RespuestaCompraConDetalle`, `HistorialCompraDto`, `CompraConDetalleDto`, `LineaCompraDetalleDto` (propiedades y tipos del plan §3.1 … §3.4).
  - Sin entidades `CapaEntidad` en los contratos; `HistorialCompraDto.FechaRegistro` es `DateTime` y `.Estado` es `bool`; `LineaCompraDetalleDto` **no** tiene `PrecioVenta`; `ProveedorSeleccionadoDto` **no** tiene correo ni teléfono.
  - Hecho cuando: `dotnet build` compila sin errores; `dotnet test` sigue en verde (sin pruebas nuevas); revisión manual de que ningún DTO expone una entidad del dominio ni datos internos.

- [x] **T2. `CapaNegocio`: cálculo puro y analizadores numéricos + pruebas puras.** RF-7, RF-20, RF-21, RF-22, RF-23, RF-24, RF-25, RF-26, RF-28, RF-33, RF-35, RNF-1, RNF-13
  - Crear `CapaNegocio/Compras/CalculoImportesCompra.cs`: constantes de límites (`CantidadMinima`, `CantidadMaxima`, `PrecioMinimo`, `PrecioMaximo`, `MontoTotalMaximo`, `DecimalesPrecio`), `CalcularSubtotal(int, decimal)` y **una sola** firma `CalcularMontoTotal(IEnumerable<decimal>)` (sin sobrecarga por `LineaCompraDto`). Solo `decimal`, sin redondeos ni truncamientos.
  - Crear `CapaNegocio/Compras/AnalizadorImporte.cs`: devuelve **motivo tipado** (`MotivoPrecioInvalido`) y valor, **nunca textos**, con el orden del plan §6.3: `Vacio` → `FormatoInvalido` → `NoPositivo` → `DecimalesExcedidos` → `SuperaMaximo`; separador único coma o punto; **un único `-` inicial admitido** (regla de RF-25); `1.000` = 1,000; desborde de `decimal` ⇒ `SuperaMaximo`.
  - Crear `CapaNegocio/Compras/AnalizadorCantidad.cs`: solo dígitos → mayor que cero → `<= 1000000` (desborde de `int` ⇒ supera el máximo); motivo tipado.
  - Pruebas (rojo → verde) en `CapaNegocio.Tests/CompraNegocioTests.cs`, grupos **G1a** (3 × 10,33 = 30,99; 1 × 10,5 = 10,5 como `10.5m`, sin formateo; suma exacta; total con 0/1/N líneas; el precio de venta no altera el total), **G2a** (motivos del analizador: `10,33` = `10.33`; `1.000` ⇒ `DecimalesExcedidos`; `10,333` y `0,001` ⇒ `DecimalesExcedidos`; **`999999999,99` y `999999999.99` aceptados**; **`1000000000` y `1000000000.00` ⇒ `SuperaMaximo`**; **`1.000.000.000,00` ⇒ `FormatoInvalido`**; `-5` ⇒ `NoPositivo`; `5-`, `- 5`, `--5`, `+5` ⇒ `FormatoInvalido`) y **G3** (cantidad: `1` y `1000000` aceptados; `0`, vacío, `-1`, `1,5`, `1.000`, `10 0`, `dos`, `+5` y **`1.000.001` ⇒ RF-20**; **`1000001` y `99999999999` ⇒ RF-21**). Comparan **motivos y valores**, no textos.
  - Hecho cuando: `dotnet test --filter "FullyQualifiedName~CompraNegocioTests"` en verde con al menos un caso válido y uno de rechazo por cada regla de G1a–G3; ninguna prueba de esta tarea referencia constantes de mensajes ni `CompraNegocio`; ninguna comparación de importes usa `double`/`float`; ningún vector **válido** usa puntos de miles en un texto de entrada (el separador es siempre decimal, RF-25; G2a y G3 incluyen a propósito `1.000.000.000,00`, `1.000` y `1.000.001` como casos de rechazo).

- [x] **T3. `CapaNegocio`: `CompraNegocio`, catálogo de mensajes y operaciones sobre el detalle en carga + pruebas.** RF-18, RF-20 … RF-30, RF-31, RF-34, RF-36, RF-37, RF-42, RF-43 (acumulación), RF-55, RF-62, RF-63, RF-65, RNF-1, RNF-2, RNF-3, RNF-4, RNF-11, RNF-13
  - Crear `CapaNegocio/CompraNegocio.cs`: constructor con `AppDbContext` (`ArgumentNullException.ThrowIfNull`), **catálogo de mensajes del plan §5** como constantes privadas (incluida `MensajeErrorPersistencia`, el mensaje de línea inexistente y el de producto repetido), `EsFalloDeOrigenDeDatos(Exception)`, `ValidarProductosAsync` (una consulta `Where(ids.Contains(IdProducto))` con proyección `Id, Codigo, Nombre, Estado`, `AsNoTracking`, RNF-2/RNF-3), `ValidarCantidad`, `ValidarPrecioDeTexto` (usa `AnalizadorImporte`), `ConstruirMensajeProducto`, `ConstruirMensajePrecio`, `ExisteProductoDuplicado`.
  - Implementar `AgregarLineaAsync` (orden producto → duplicado → cantidad → precio de compra → precio de venta; acumulación con los precios efectivos del DTO; RF-63 sin tocar la línea existente), `EditarLineaAsync` (índice fuera de rango ⇒ `LineaNoEncontrada` con la lista intacta, **antes** de consultar la base; luego rechazo de RF-62 sin modificar nada y reemplazo del índice indicado), `QuitarLinea` (síncrono, con `Exito`/`Mensaje` y lista intacta si el índice no existe), `CalcularSubtotalEnPreparacion` (síncrono) y `ObtenerMensajeConfirmacionCancelarCompra` (síncrono, texto de RF-31).
  - Pruebas: grupos **G1b** (preparación con «abc»/«0»/«10,333»/sin producto ⇒ inválido; preparación válida sin sumar al total; quitar recalcula; índice fuera de rango), **G2b** (los textos exactos del catálogo comparados por igualdad), **G4** (validación de línea y orden determinista) y **G5** (agregar, acumular, 999999 + 2 rechazado, 999999 + 1 aceptado, editar con reemplazo, índice inválido en edición, RF-62, nunca dos líneas del mismo producto). **G5 no prueba el prellenado**: la parte de RF-43 que cubre es solo la **acumulación** según la grilla vigente. Ante rechazo, la respuesta devuelve la lista **intacta** (RNF-11).
  - Hecho cuando: `dotnet test --filter "FullyQualifiedName~CompraNegocioTests"` en verde; ningún `ToListAsync()` sobre `Productos` para validar (RNF-3); ninguna firma `async` sin `await` (`ValidarProductosAsync`, `AgregarLineaAsync` y `EditarLineaAsync` son las únicas asíncronas aquí, RNF-2); los textos de rechazo son los del catálogo (comparados por igualdad).

- [x] **T4. `CapaNegocio`: búsquedas acotadas y prellenado de precios + pruebas.** RF-9, RF-10, RF-11, RF-12, RF-15, RF-16, RF-17, RF-38, RF-39, RF-40, RF-41, RF-57, RNF-2, RNF-3, RNF-5
  - Implementar `BuscarProveedoresAsync` (solo activos, parcial por `Cuit`/`RazonSocial` con `ToUpper().Contains`, `OrderBy(RazonSocial).ThenBy(IdProveedor)`, `Take(20)`, proyección `ProveedorSeleccionadoDto`; texto vacío ⇒ `SinResultados` **sin consultar** y con `Mensaje` vacío) y `BuscarProductosAsync` (idem por `Codigo`/`Nombre`, `OrderBy(Nombre).ThenBy(IdProducto)`; texto vacío ⇒ lista vacía **sin consultar**, `Mensaje` vacío).
  - Implementar `ObtenerPreciosProductoAsync` (proyección `PrecioCompra`, `PrecioVenta`, `Estado`; inexistente/inactivo con resultado propio; solo lectura, sin escrituras). Todo con `async/await` (RNF-2), `AsNoTracking()` y proyección (RNF-3).
  - Pruebas: grupo **G7** (parcial por documento y razón social / código y nombre, insensible a mayúsculas, solo activos, tope de 20, orden y desempate, sin coincidencias con mensaje, proyección sin correo ni teléfono) con el **mecanismo de texto vacío sin consultar del plan §9.1** (contexto dispuesto: `SinResultados`/lista vacía y **no** `ErrorPersistencia`), **G6** (precios del catálogo, precios en 0 devueltos en 0, inactivo, inexistente, y comparación de stock/precios/`IdUsuarioModifica`/`FechaModificacion`/`MotivoAjusteStock` antes y después) y la parte de **G11** de búsquedas y prellenado con contexto dispuesto ⇒ `ErrorPersistencia` (RF-57).
  - Hecho cuando: `dotnet test --filter "FullyQualifiedName~CompraNegocioTests"` en verde; ninguna búsqueda materializa el padrón ni el catálogo completo; el caso de texto vacío queda verde **con el contexto dispuesto** (prueba verificable de que no consulta); el prellenado no modifica ninguna fila.

- [x] **T5a. `CapaNegocio`: validadores de la confirmación.** RF-5, RF-14, RF-22, RF-23, RF-24, RF-26, RF-28, RNF-1
  - Implementar en `CompraNegocio` `ValidarProveedorAsync` y `ValidarUsuarioAsync` (proyección mínima `AsNoTracking`, sin releer el rol, D-14) y `ValidarPrecioRecibido` con el orden del plan §6.4: `≤ 0` ⇒ mensaje de mayor que cero (RF-22/RF-23) → más de dos decimales ⇒ mensaje de RF-26 → `> 999.999.999,99` ⇒ mensaje propio de RF-24.
  - Los tres son **privados** y solo se ejercitan a través de `RegistrarCompraAsync`, que llega en T5b: esta tarea no agrega pruebas propias (sus casos se cubren en G9 de T5b).
  - Hecho cuando: `dotnet build` limpio; `dotnet test` en verde sin pruebas nuevas; las validaciones usan proyección acotada (RNF-3) y no releen el rol de la base (D-14).

- [x] **T5b. `CapaNegocio`: registro transaccional de la compra + pruebas.** RF-4, RF-6, RF-7, RF-14, RF-18, RF-19, RF-20, RF-21, RF-22, RF-23, RF-24, RF-26, RF-27, RF-28, RF-33, RF-37, RF-44, RF-46, RF-47, RF-48, RF-64, RNF-1, RNF-2, RNF-3, RNF-11
  - Implementar `RegistrarCompraAsync(SolicitudRegistrarCompraDto, int idUsuario, string? rol)` con los **pasos 2–13** del plan §7: proveedor → usuario → líneas (producto, duplicado, cantidad, precio de compra y precio de venta **con revalidación de escala decimal**, plan §6.4) → sin ítems → tope del total → `BeginTransactionAsync` → cabecera (`Estado = true`, `FechaRegistro = DateTime.Now`) + líneas (`IdProducto`, `Cantidad`, `PrecioCompra`, `Subtotal` recalculado, **sin** `PrecioVenta`) → **un** `SaveChangesAsync` → `AplicarEfectosSecundariosAsync` (hook que devuelve `Task.CompletedTask`) → `CommitAsync`. El `Subtotal` que llega en el DTO se ignora (RNF-13).
  - **El paso 1 del plan §7 (chequeo de rol) no se implementa aquí**: el método **recibe** `rol` pero **no lo valida**; el paso 1 se activa en **T8**. Tampoco incluye todavía el manejo de errores ni el `DisposeAsync` (T6): si la escritura falla en esta etapa, la excepción se propaga —situación transitoria que T6 cierra—. No se implementa la limpieza de pantalla tras el registro (RF-32 es de la UI, plan §9.2).
  - Pruebas: **G8** (1 y N líneas persistidas con total y subtotales correctos, fecha dentro del intervalo previo/posterior, estado activo, usuario y proveedor asignados, `Subtotal` del DTO ignorado, catálogo intacto) y **G9** (proveedor inexistente/inactivo/dado de baja después de seleccionarlo, usuario 0/inexistente/inactivo, sin líneas, producto desactivado entre carga y confirmación, **dos líneas del mismo `IdProducto` en la solicitud ⇒ `ProductoInvalido` con el mensaje de producto repetido en el detalle y 0 filas (RF-37, RNF-6)**, **precio de compra o de venta `10.333m` enviado directo al negocio ⇒ `PrecioInvalido` + `DecimalesExcedidos` y 0 filas**, **precio de compra o de venta `1000000000m` ⇒ `PrecioInvalido` + `SuperaMaximo` con el mensaje propio de RF-24 y 0 filas**, **11 líneas de `1000000` × `999999999.99m`** ⇒ «El monto total de la compra supera el máximo permitido.» con 0 filas, precedencia del primer error). **Todas las invocaciones usan `"Administrador"`** (o `"Encargado"`), para que la activación del paso 1 en T8 no altere estas pruebas.
  - Hecho cuando: `dotnet test --filter "FullyQualifiedName~CompraNegocioTests"` en verde; `git status` no muestra cambios en `CapaDatos/Migrations/`; ninguna sentencia toca `PRODUCTO`; el rechazo por decimales o por máximo de precio deja 0 filas y la solicitud y la pantalla conservan las líneas intactas (RNF-11).

- [x] **T6. `CapaNegocio`: atomicidad y fallo del origen de datos al escribir + pruebas.** RF-45, RF-46, RF-56, RNF-11
  - Envolver los pasos 7–13 de `RegistrarCompraAsync` en `try/catch/finally`: `catch` de `DbUpdateException`/`DbException`/`InvalidOperationException` (filtro `EsFalloDeOrigenDeDatos`), `RollbackAsync` **protegido** (sin relanzar), desadjuntar cabecera y líneas (`Entry(...).State = Detached`) y devolver `ErrorPersistencia` con `MensajeErrorPersistencia`; `finally` con `DisposeAsync` de la transacción. Nunca se propaga la excepción (RF-55/RF-56) y nunca se informa un mensaje de límites como error de persistencia (RNF-8). El hook de HU-10 queda **dentro** del ámbito transaccional, antes del `CommitAsync` (RF-46).
  - Declarar al final de `CapaNegocio.Tests/CompraNegocioTests.cs` el doble `FalloDbContext : AppDbContext` con `DbUpdateException` en `SaveChangesAsync` y en `SaveChanges`, y sembrar el escenario con un context **sano** sobre la misma base en memoria, según el procedimiento paso a paso del plan §9.1.
  - Pruebas: **G10** ⇒ `ErrorPersistencia` con el texto único, sin excepción propagada, `IdCompra == null`, líneas disponibles para reintentar y **0** filas en `Compras` y `DetallesCompra` verificadas con el contexto sano; la invocación usa **`"Administrador"`**. Se documenta en el archivo que el proveedor InMemory **no** ejecuta el rollback real (plan §9.1).
  - Hecho cuando: `dotnet test --filter "FullyQualifiedName~CompraNegocioTests"` en verde; G10 deja verificado el conteo 0/0 con el contexto sano; ninguna firma `async` sin `await`.

- [x] **T7. `CapaNegocio`: historial de compras y compra con detalle para Reportes + pruebas.** RF-49, RF-50, RF-51, RF-52 (contrato de datos), RF-53, RF-54, RF-57, RNF-2, RNF-3, RNF-5, RNF-12
  - Implementar `ObtenerHistorialAsync(string? rol)`: `Compras.AsNoTracking()` con `OrderByDescending(FechaRegistro).ThenByDescending(IdCompra)`, proyección a `HistorialCompraDto` (`NombreUsuario` = nombre + apellido recortado, `MontoTotal`, `Estado`), **proyección defensiva de navegaciones nulas** con los textos fijos `«(Sin proveedor)»` / `«(Sin usuario)»` (plan §3.4), `ConCompras`/`SinCompras` con mensaje del negocio y `ErrorPersistencia` con el texto único.
  - Implementar `ObtenerCompraConDetalleAsync(int idCompra, string? rol)`: cabecera + líneas ordenadas por `IdDetalleCompra` con `CodigoProducto`, `NombreProducto`, `Cantidad`, `PrecioCompra`, `Subtotal` (**sin** `PrecioVenta`), misma proyección defensiva, `CompraNoEncontrada` y `ErrorPersistencia`.
  - Pruebas: **G12** (vacío, orden descendente, desempate por `IdCompra` con la misma fecha y hora, inactivas incluidas con `Estado = false`, tipos `DateTime`/`bool`, nombre + apellido, sin datos internos, **compra con `IdProveedor`/`IdUsuario` inexistentes ⇒ textos defensivos sin excepción**) y **G13** (detalle con y sin líneas, sin precio de venta, compra inexistente, navegaciones nulas, sin modificar datos) y la parte de **G11** de historial y detalle con contexto dispuesto ⇒ `ErrorPersistencia`. Las consultas se invocan con **`"Administrador"`** (o `"Encargado"`), para que la activación del chequeo de rol en T8 no altere estas pruebas.
  - El método **recibe** `rol`; el chequeo de permisos se activa en T8. Las consultas son asíncronas (RNF-2), con `AsNoTracking()` y proyección (RNF-3).
  - Hecho cuando: `dotnet test --filter "FullyQualifiedName~CompraNegocioTests"` en verde; el orden se resuelve en la consulta y no en memoria; `LineaCompraDetalleDto` no declara `PrecioVenta`; las pruebas de navegaciones nulas no lanzan excepción.

- [x] **T8. `CapaNegocio`: permisos RBAC en negocio + pruebas.** RF-8, RF-59, RF-60, RF-61
  - Agregar `PoliticaAcceso.PuedeOperarCompras(string? rol)` = `TieneAcceso(rol, "Compras")` sin cambiar la lista de módulos (Administrador y Encargado `true`; Vendedor, `null`, `""` y rol desconocido `false`).
  - Crear `ValidarRolParaRegistro(string? rol)` y activar el chequeo como **paso 1** de `RegistrarCompraAsync` (`PermisoDenegado`, «No tiene permisos para registrar compras.», sin consultar la base) y como **primera** verificación de `ObtenerHistorialAsync` y `ObtenerCompraConDetalleAsync` (`PermisoDenegado`, «No tiene permisos para consultar las compras.», sin devolver datos). El rol **no** se relee de la base (D-14).
  - Pruebas: **`CapaNegocio.Tests/PoliticaAccesoTests.cs`** (el archivo **ya existe**: se le agregan casos de `PuedeOperarCompras` —administrador y encargado `true`; vendedor, `null`, `""`, `"   "` y desconocido `false`—, sin crear un archivo nuevo) y **G14** en `CompraNegocioTests` (registro, historial y detalle con Vendedor ⇒ `PermisoDenegado`, 0 filas y 0 datos, **sin ejecutar ninguna consulta** —se verifica con el contexto dispuesto del plan §9.1—; con rol no autorizado y datos también inválidos ⇒ **único** mensaje de permiso).
  - Hecho cuando: `dotnet test` completo en verde (las suites de **T5b, T6 y T7** no cambian porque invocan con `"Administrador"`); `dotnet test --filter "FullyQualifiedName~PoliticaAccesoTests"` en verde; ninguna consulta se ejecuta cuando el rol no está autorizado.

- [x] **T9a. `CapaPresentacion`: pantalla `ComprasForm` real (MaterialSkin, eventos delgados) + DI + sesión en `inicio`.** RF-1, RF-2, RF-3, RF-8, RF-12, RF-13, RF-31, RF-32, RF-38, RF-52, RF-58, RF-65 (visual), RNF-7, RNF-12, RNF-13
  - Reescribir `Compras/ComprasForm.cs` con `public ComprasForm(CompraNegocio negocio)` y los controles del plan §10.2: rótulo «Registro de Compras a Proveedores», búsqueda rápida de proveedores con proveedor elegido visible y reemplazable, búsqueda de productos, área de captura (producto, cantidad, precio de compra, precio de venta, subtotal con «—» cuando no es válido), grilla de líneas con Subtotal y botones Editar/Quitar, total general, «Registrar compra», «Cancelar compra» y grilla de historial.
  - Retirar el título «REGISTRO DE COMPRAS Y RECEPCIÓN DE STOCK», el botón «Nueva Orden de Compra», el combo de proveedores escrito a mano, el campo «N° Factura / Comprobante» y las filas de ejemplo.
  - Eventos delgados (plan §10.3): llamar al negocio, bindear respuestas y mostrar el `Mensaje` tal cual; el subtotal del área y el total salen **solo** de `CalcularSubtotalEnPreparacion` y de la única firma `CalcularMontoTotal(_lineas.Select(l => l.Subtotal))`; quitar una línea usa el `Exito`/`Mensaje` de `QuitarLinea`; `CellFormatting` con `N2`, `dd/MM/yyyy HH:mm` y «Activa»/«Inactiva». Sin `AppDbContext`, sin SQL, sin fórmulas ni mensajes propios.
  - `_btnRegistrar.Enabled = false` + bandera `_registrando` durante el registro; confirmación de limpieza con `ObtenerMensajeConfirmacionCancelarCompra()`; tras el éxito, **descarte de proveedor, líneas y captura (RF-32)** y recarga del historial (RF-52).
  - `Program.cs`: `AddTransient<CompraNegocio>()` y `AddTransient<ComprasForm>()`. `Inicio/inicio.cs`: parámetro `ComprasForm comprasForm`, campo `_comprasForm`, `AbrirVistaEnTab(_comprasForm, tab_compras)` (sin `new`), `tab_compras.Enter` para recargar el historial y `_comprasForm.AplicarSesion(_usuarioActual?.IdUsuario ?? 0, rol)` en `AplicarPermisosPorRol()`.
  - Hecho cuando: `dotnet build` limpio y `dotnet test` en verde; la pantalla no contiene `AppDbContext`, SQL, fórmulas ni textos de rechazo propios (revisión de código). La comprobación visual es **T9b**.

- [ ] **T9b. 🔍 Verificación manual de la pantalla de Compras.** RF-1, RF-2, RF-3, RF-8, RF-12, RF-13, RF-31, RF-32, RF-38, RF-43 (prellenado), RF-52, RF-58, RF-65, RNF-7, RNF-12
  - Con datos reales, verificar: rótulos y retiro de la maqueta (RF-1, RF-2, RF-3); búsquedas reales con texto vacío (sin mensaje ni «sin resultados») y con coincidencias (solo activos); **prellenado editable** al elegir producto y **vuelta a prellenar al re-agregar un producto quitado (RF-43)**; proveedor elegido visible y reemplazable sin perder líneas (RF-12, RF-13); subtotal del área con «—» y total que no cambia con la línea en preparación (RF-65); aviso de acumulación; **rechazo de la acumulación que supera 1.000.000 (RF-63, «La cantidad acumulada supera el máximo permitido.»)**; quitar/editar; confirmación de limpiar (RF-31); **descarte de proveedor y líneas después de registrar (RF-32)**; doble envío deshabilitado (RF-58); formato de fecha/estado del historial y compra recién registrada en su posición (RF-52, RNF-12); MaterialSkin coherente y solo los controles admitidos (RNF-7); Administrador y Encargado operan sin acciones ocultas (RF-8).
  - Hecho cuando: `dotnet build` limpio y `dotnet test` en verde; cada punto de la lista se registra como verificado con una nota de lo observado (sin evidencia no se considera cumplido).

- [ ] **T10. 🔍 Verificación final contra SQL Server, cierre del checklist y `MEMORY.md`.** Todos los RF/RNF
  - `dotnet build` limpio y `dotnet test` completo en verde.
  - **Atomicidad real (SQL Server), procedimiento reproducible** (criterio «Atomicidad comprobada», RF-44/RF-45):
    1. **Preparar**: elegir un proveedor activo (`P`) y dos productos activos (`P1`, `P2`) ya sembrados; anotar sus `id_proveedor` / `id_producto` y `SELECT MAX(id_compra) FROM COMPRA;` ⇒ `max_previo`.
    2. **Caso bueno**: registrar desde la pantalla una compra de **dos líneas** (`P1`, `P2`) y comprobar `SELECT * FROM COMPRA WHERE id_compra > max_previo;` ⇒ **1 fila** y `SELECT * FROM DETALLE_COMPRA WHERE id_compra > max_previo;` ⇒ **2 filas**.
    3. **Restablecer** el contador de referencia: `max_previo = SELECT MAX(id_compra) FROM COMPRA;`.
    4. **Provocar el fallo en el `INSERT` de la segunda línea** con un trigger temporal sobre el detalle:
       ```sql
       CREATE TRIGGER TR_FALLO_SIMULADO_DETALLE ON DETALLE_COMPRA
       AFTER INSERT AS
       BEGIN
           IF EXISTS (SELECT 1 FROM inserted WHERE id_producto = <id_producto de P2>)
               THROW 51000, 'Fallo simulado de la verificación de atomicidad', 1;
       END;
       ```
    5. **Ejecutar**: registrar por la pantalla otra compra de **dos líneas** (`P1`, `P2`) en el mismo orden. La cabecera y la primera línea se insertan y el `INSERT` de la segunda falla; la aplicación debe mostrar «No se pudo completar la operación por un problema con el origen de datos. Intente nuevamente.» y **conservar** en pantalla el proveedor, las líneas y los precios (RNF-11).
    6. **Restaurar**: `DROP TRIGGER TR_FALLO_SIMULADO_DETALLE;` y comprobar con `SELECT name FROM sys.triggers WHERE name = 'TR_FALLO_SIMULADO_DETALLE';` ⇒ 0 filas.
    7. **Comprobar el rollback**: desde una conexión nueva, `SELECT COUNT(*) FROM COMPRA WHERE id_compra > max_previo;` ⇒ **0** y `SELECT COUNT(*) FROM DETALLE_COMPRA WHERE id_compra > max_previo;` ⇒ **0**; además `SELECT COUNT(*) FROM DETALLE_COMPRA d WHERE NOT EXISTS (SELECT 1 FROM COMPRA c WHERE c.id_compra = d.id_compra);` ⇒ **0** (sin líneas huérfanas).
    8. **Reintentar sin el trigger** la misma compra y comprobar que se registra completa (1 cabecera + 2 líneas). Dejar constancia del resultado y de que el trigger temporal fue eliminado.
  - **No regresión del catálogo**: antes/después de registrar compras de prueba, `stock`, `precio_compra`, `precio_venta`, `id_usuario_modifica`, `fecha_modificacion` y `motivo_ajuste_stock` del producto no cambian (RF-47) y no se creó ni modificó ninguna otra entidad (RF-48).
  - **Rendimiento (RNF-9)**: con una compra de **200 líneas**, editar una línea y percibir la actualización del total en menos de 200 ms.
  - **Doble envío (RF-58)**: doble clic en «Registrar compra» durante el guardado ⇒ una sola compra.
  - **Descarte tras el registro (RF-32)**: después de un registro exitoso, el proveedor elegido, la grilla de líneas y el área de captura quedan vacíos y el historial muestra la compra recién registrada en su posición.
  - **Permisos (RF-8)**: Administrador y Encargado operan todo sin acciones ocultas; Vendedor no ve la pestaña y la lógica rechaza registro, historial y detalle.
  - **Sin cambios de esquema (RNF-10)**: `git status` sin archivos nuevos en `CapaDatos/Migrations/`, y el esquema de `COMPRA`/`DETALLE_COMPRA`/`PRODUCTO` sin cambios.
  - **Cierre documental**: actualizar `MEMORY.md` (≤ ~50 líneas) con el estado (spec 005 aprobada e implementada), las decisiones relevantes (negocio sin estado, cálculo único, hook de HU-10, proyección defensiva, sin migraciones), los errores a evitar (transacciones no reales en InMemory, no tocar `PRODUCTO`, no agregar índice producto-compra, no confiar en la escala decimal ya validada al agregar la línea, no escribir puntos de miles en vectores de prueba) y los próximos pasos (HU-10 sobre el hook; Reportes consumiendo `ObtenerHistorialAsync` y `ObtenerCompraConDetalleAsync`). Sin datos sensibles.
  - Hecho cuando: checklist de la spec cumplido, resultados manuales registrados en el resumen con evidencia de cada paso (incluidos los conteos 0/0 del fallo inyectado y el `DROP TRIGGER`), y `MEMORY.md` actualizado sin datos sensibles.

---

## Cobertura RF → tarea

| RF | Tarea | RF | Tarea | RF | Tarea |
|---|---|---|---|---|---|
| RF-1, RF-2, RF-3 | T9a, T9b 🔍 | RF-4 | T5b | RF-5, RF-6 | T5a, T5b |
| RF-7 | T2, T5b, T9a | RF-8 | T8, T9a, T9b 🔍 | RF-9, RF-10, RF-11 | T4 |
| RF-12, RF-13 | T4 (búsqueda), T9a, T9b 🔍 | RF-14 | T5a, T5b | RF-15, RF-16, RF-17 | T4 |
| RF-18 | T3, T5b | RF-19 | T5b | RF-20, RF-21 | T2, T3, T5b |
| RF-22, RF-23 | T2, T3, T5a, T5b | RF-24 | T2, T3, T5a, T5b | RF-25 | T2, T3 |
| RF-26 | T2, T3, T5a, T5b | RF-27 | T2 (orden), T3, T5b | RF-28 | T2 (orden), T3, T5b |
| RF-29, RF-30 | T3 | RF-31 | T3 (texto), T9a, T9b 🔍 | RF-32 | T9a, T9b 🔍 (no automatizable) |
| RF-33, RF-34 | T2, T3 | RF-35 | T2 | RF-36, RF-37 | T3, T5b |
| RF-38, RF-39, RF-40 | T4, T9a | RF-41 | T4 | RF-42 | T3 |
| RF-43 | T3 (acumulación), T9a/T9b 🔍 (prellenado) | RF-44 | T5b, T6 | RF-45 | T6, T10 🔍 |
| RF-46 | T5b, T6 | RF-47, RF-48 | T5b, T10 🔍 | RF-49, RF-50, RF-51 | T7 |
| RF-52 | T7 (datos), T9a, T9b 🔍 | RF-53, RF-54 | T7 | RF-55 | T2 … T9a |
| RF-56 | T6, T10 🔍 | RF-57 | T4, T7 | RF-58 | T9a, T9b 🔍 |
| RF-59, RF-60, RF-61 | T8 | RF-62, RF-63 | T3 | RF-64 | T5b |
| RF-65 | T3, T9a, T9b 🔍 | | | | |

**RNF**: RNF-1 → T2, T3, T5a; RNF-2 → T3, T4, T5b, T7; RNF-3 → T3, T4, T5b, T7; RNF-4 → T3, T7; RNF-5 → T1, T4, T7; RNF-6 → T2 … T10; RNF-7 → T9a, T9b 🔍; RNF-8 → T2 … T8; RNF-9 → T10 🔍; RNF-10 → T10 🔍 (nota global de todas las tareas); RNF-11 → T3, T5b, T6; RNF-12 → T1, T7, T9a; RNF-13 → T2, T3, T9a.

---

## Tareas con verificación manual (🔍)

| Tarea / criterio | Por qué no es automática |
|---|---|
| **T9b** completo | Rótulos, disposición MaterialSkin, retiro de la maqueta, prellenado visual (RF-38 y RF-43), confirmación de limpieza, **descarte de proveedor y líneas tras el registro (RF-32)** y presentación no son reglas de `CapaNegocio` (RNF-6, RNF-7, RF-1/2/3, RF-31, RF-32, RF-58, RNF-12). |
| **T10** · atomicidad real (SQL Server) | El proveedor InMemory no ejecuta rollback; se verifica contra SQL Server con el procedimiento reproducible de la tarea (criterio «Atomicidad comprobada»). |
| **T10** · RNF-9 (200 líneas) | Requisito de percepción visual medido manualmente con datos reales. |
| **T10** · doble envío y descarte tras el registro | Reglas de pantalla (RF-58 y RF-32). |
| **T10** · no regresión del catálogo y sin cambios de esquema | Comprobación contra la base real y `git status` (RF-47, RF-48, RNF-10). |

---

> **Nota para el coordinador**: presentar el plan y las tareas para aprobación explícita **antes** de iniciar T1. No hay migraciones ni paquetes nuevos. Esta versión incorpora las correcciones de la **segunda revisión QA**: vectores de prueba con un único separador decimal y cantidades sin puntos de miles, RF-37 y RF-24 probados al confirmar, pasos 2–13 en T5b con los roles de prueba alineados, y tareas divididas con sufijos a/b (12 tareas, sin renumerar). No iniciar una tarea sin autorización y detenerse al terminarla.
