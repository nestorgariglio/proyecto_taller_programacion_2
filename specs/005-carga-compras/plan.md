# Plan técnico — Spec 005: Carga de Compras a Proveedores

Estado: propuesto (pendiente de aprobación del usuario) · corregido tras la **segunda revisión QA** (2026-10-29)
Spec de referencia: `specs/005-carga-compras/spec.md` — **aprobada (2026-10-29)**, con **D-1 … D-21** cerradas y sin dudas abiertas.
Fecha: 2026-10-29
Alcance técnico: `CapaNegocio` (lógica, DTOs, enums, validaciones y consultas acotadas) + `CapaPresentacion` (pantalla `ComprasForm` existente). **`CapaEntidad` y `CapaDatos` no se tocan** y **no se crean migraciones** (RNF-10).

> **Regla del proyecto**: la spec manda. Este plan implementa **solo** lo que la spec 005 exige. La interfaz no valida reglas, no arma mensajes y no calcula importes (RF-55 y RNF-13); el acceso a datos vive en `AppDbContext` (inyectado en `CapaNegocio`, mismo patrón que `ProveedorNegocio` y `ProductoNegocio`).

---

## 1. Contexto técnico y alcance

La pantalla `CapaPresentacion/Compras/ComprasForm.cs` es hoy una maqueta: título «📑 REGISTRO DE COMPRAS Y RECEPCIÓN DE STOCK», combo de proveedores escrito a mano, campo «N° Factura / Comprobante», botón «📥 Nueva Orden de Compra», un `DataGridView` con dos filas de ejemplo y **sin acceso a datos**. El esquema real ya existe (`COMPRA` y `DETALLE_COMPRA`, `decimal(18,2)`, sin comprobante y sin precio de venta) y `AppDbContext` ya expone `Compras` y `DetallesCompra`.

Este plan reemplaza la maqueta por el **registro real de compras**: búsqueda acotada de proveedores y productos, área de captura con prellenado editable, grilla de líneas con subtotales, monto total calculado, registro **transaccional** (cabecera + líneas) y **historial real**. Toda la lógica queda en una clase nueva `CompraNegocio` con DTOs propios; la pantalla solo orquesta llamadas.

**Qué no se hace** (spec, «Fuera de alcance»):
- No se escribe nada sobre `PRODUCTO` (RF-47/RF-48); la recepción de stock es HU-10 y solo se deja **un punto de extensión** dentro de la misma transacción (RF-46).
- No se agregan columnas, tablas, índices, restricciones ni migraciones (RNF-10); la unicidad producto-compra es de la lógica (RF-37).
- No se crean pantallas ni ítems de menú (RNF-7); el menú y `PoliticaAcceso` no cambian de alcance.
- No se implementa la pantalla de detalle de una compra (RF-54 se cubre en la lógica con pruebas).

---

## 2. Mapa de capas (archivo → responsabilidad)

### `CapaEntidad` — sin cambios
| Archivo | Estado |
|---|---|
| `Compra.cs` | Intacto: `IdCompra`, `IdUsuario`, `IdProveedor`, `MontoTotal`, `Estado`, `FechaRegistro` + navegaciones. |
| `DetalleCompra.cs` | Intacto: `IdDetalleCompra`, `IdCompra`, `IdProducto`, `PrecioCompra`, `Cantidad`, `Subtotal`. **No existe** `PrecioVenta` (D-2). |
| `Producto.cs`, `Proveedor.cs`, `Usuario.cs`, `Rol.cs` | Intactos. Solo se leen. |

### `CapaDatos` — sin cambios
| Archivo | Estado | Motivo |
|---|---|---|
| `AppDbContext.cs` | **Intacto** | `DbSet<Compra>` y `DbSet<DetalleCompra>` ya existen; las claves foráneas y el `decimal(18,2)` ya están en el esquema y las migraciones aplicadas. No se agrega Fluent API, ni `.HasTrigger`, ni índices (el trigger `TR_PRODUCTO_FECHA_MODIFICACION` se conserva tal cual está). |
| `Migrations/` | **Sin archivos nuevos** | RNF-10. No se ejecuta `dotnet ef migrations add` ni `database update`. |

### `CapaNegocio` — cambios
| Archivo | Cambio | Responsabilidad |
|---|---|---|
| `CompraNegocio.cs` | **Nuevo** | Único punto de reglas de compras: catálogo de mensajes, validaciones, operaciones sobre el detalle, registro transaccional, historial, compra con detalle. |
| `Compras/CalculoImportesCompra.cs` | **Nuevo** | Regla **única** de cálculo (RF-33, RF-34, RNF-1, RNF-13) y constantes de límites (D-15, D-20). Sin EF y sin mensajes. |
| `Compras/AnalizadorImporte.cs` | **Nuevo** | Parseo determinista de precios desde texto (RF-22 … RF-26, RF-28): coma o punto, un único separador, un único `-` inicial, ≤ 2 decimales, rango. Devuelve **motivo tipado**, no textos. Sin EF. |
| `Compras/AnalizadorCantidad.cs` | **Nuevo** | Parseo de cantidad solo dígitos, 1 … 1.000.000 (RF-20, RF-21). Devuelve motivo tipado. Sin EF. |
| `DTOs/Compras/*.cs` | **Nuevos** | Contratos de entrada/salida entre Presentación y Negocio (§3). |
| `Enums/*.cs` | **Nuevos** | Resultados y motivos (§3). |
| `PoliticaAcceso.cs` | **Modificado (aditivo)** | Nuevo `PuedeOperarCompras(string? rol)` = `TieneAcceso(rol, "Compras")` (RF-8, RF-59 … RF-61). No cambia la lista de módulos. |
| `ProveedorNegocio.cs`, `ProductoNegocio.cs` | **Sin cambios** | Solo se **reutiliza el patrón** de consulta y de mensajes; no se invocan desde `CompraNegocio` para no acoplar módulos ni reutilizar proyecciones completas (RNF-5). |

### `CapaPresentacion` — cambios
| Archivo | Cambio | Responsabilidad |
|---|---|---|
| `Compras/ComprasForm.cs` | **Reescrito** | Pantalla real con MaterialSkin 2: búsquedas, área de captura, grilla de líneas, total, registrar/limpiar e historial. Eventos delgados: arma DTO, llama al negocio, muestra el mensaje recibido y formatea importes/fechas/estados (RNF-12). |
| `Compras/ComprasForm.Designer.cs` | **Sin cambios** | La vista se arma por código, como el resto de las pantallas. |
| `Program.cs` | **Modificado** | `AddTransient<CompraNegocio>()` y `AddTransient<ComprasForm>()`. |
| `Inicio/inicio.cs` | **Modificado** | Recibe `ComprasForm` por DI (reemplaza `new ComprasForm()`), recarga el historial en `tab_compras.Enter` e inyecta `IdUsuario` y `rol` con `AplicarSesion(int, string?)` desde `AplicarPermisosPorRol()`. |
| `Proveedores/ProveedoresForm.cs`, `Productos/*` | **Sin cambios** | Solo se replican estilo y patrones. |

### `CapaNegocio.Tests` — cambios
| Archivo | Cambio |
|---|---|
| `CompraNegocioTests.cs` | **Nuevo**: todas las pruebas de la lógica de compras (RNF-6), incluidos los dobles de prueba (`FalloDbContext`, contexto dispuesto). |
| `PoliticaAccesoTests.cs` | **Modificado (aditivo)**: casos de `PuedeOperarCompras`. El archivo **ya existe** en `CapaNegocio.Tests/PoliticaAccesoTests.cs` (verificado); no se crea uno nuevo. |

---

## 3. Contratos de datos (DTOs y enums)

Carpeta `CapaNegocio/DTOs/Compras/` (namespace `CapaNegocio.DTOs.Compras`) y `CapaNegocio/Enums/` (namespace `CapaNegocio.Enums`). Todos los importes son `decimal` (RNF-1).

### 3.1 DTOs de selección y carga

| DTO | Propiedades y tipos | Uso |
|---|---|---|
| `ProveedorSeleccionadoDto` | `int IdProveedor`, `string RazonSocial`, `string Documento` | Proyección **mínima y propia de Compras** (RNF-5): sin correo, sin teléfono, sin `FechaRegistro`, sin entidad. `Documento` = `Proveedor.Cuit`. |
| `ProductoBuscadoDto` | `int IdProducto`, `string Codigo`, `string Nombre` | Resultado de la búsqueda de productos (RF-15). Sin precios ni stock: el prellenado es una consulta aparte. |
| `PreciosProductoDto` | `int IdProducto`, `decimal PrecioCompra`, `decimal PrecioVenta` | Valores de prellenado (RF-38). Solo valores, nunca la entidad. |
| `LineaCompraDto` | `int IdProducto`, `string CodigoProducto`, `string NombreProducto`, `int Cantidad`, `decimal PrecioCompra`, `decimal PrecioVenta`, `decimal Subtotal` | **Línea en carga** que la pantalla mantiene en memoria y envía al negocio. `PrecioVenta` vive solo aquí (no se persiste, D-2); `Subtotal` lo calcula el negocio, la pantalla nunca lo recalcula. |
| `SolicitudAgregarLineaCompraDto` | `int IdProducto`, `string? Cantidad`, `string? PrecioCompra`, `string? PrecioVenta`, `IReadOnlyList<LineaCompraDto> LineasActuales` | Agregar una línea desde el área de captura. Los valores numéricos viajan **crudos** porque la validación de formato es del negocio (RF-55). |
| `SolicitudEditarLineaCompraDto` | `int IndiceLinea`, `int IdProducto`, `string? Cantidad`, `string? PrecioCompra`, `string? PrecioVenta`, `IReadOnlyList<LineaCompraDto> LineasActuales` | Guardar la corrección de una línea existente (RF-30/RF-62). |
| `SolicitudRegistrarCompraDto` | `int? IdProveedor`, `IReadOnlyList<LineaCompraDto> Lineas` | Confirmación de la compra. `IdUsuario` y `rol` **no** viajan aquí: son parámetros del método (§4). |

### 3.2 DTOs de resultado intermedios

| DTO | Propiedades | Uso |
|---|---|---|
| `SubtotalLineaDto` | `bool EsValido`, `decimal? Subtotal` | Subtotales del área de captura (RF-65): `EsValido = false` ⇒ la pantalla muestra «—». Sin unidad de presentación. |
| `RespuestaOperacionLineaCompra` | `ResultadoOperacionLineaCompra Resultado`, `string Mensaje`, `IReadOnlyList<LineaCompraDto> Lineas`, `int? IndiceLineaAfectada`, `decimal MontoTotal`, `CampoPrecio? CampoPrecio`, `MotivoPrecioInvalido? MotivoPrecio` | Resultado de agregar/guardar línea. `Lineas` es la grilla resultante (nunca “a medias”: ante rechazo vuelve la lista intacta, RNF-11). |
| `RespuestaCompraEnCarga` | `bool Exito`, `string Mensaje`, `IReadOnlyList<LineaCompraDto> Lineas`, `decimal MontoTotal` | Resultado de quitar una línea (RF-29). `Exito` y `Mensaje` cubren el índice fuera de rango (§4.2). No toca la base (RNF-4). |

### 3.3 DTOs de respuesta

| Respuesta | Propiedades | Enum de resultado |
|---|---|---|
| `RespuestaBusquedaProveedoresCompra` | `Resultado`, `string Mensaje`, `IReadOnlyList<ProveedorSeleccionadoDto> Proveedores` | `ResultadoBusquedaCompras`: `ConResultados`, `SinResultados`, `ErrorPersistencia` |
| `RespuestaBusquedaProductosCompra` | `Resultado`, `string Mensaje`, `IReadOnlyList<ProductoBuscadoDto> Productos` | `ResultadoBusquedaCompras` |
| `RespuestaPreciosProducto` | `Resultado`, `string Mensaje`, `PreciosProductoDto? Precios` | `ResultadoPreciosProducto`: `Exito`, `ProductoNoEncontrado`, `ProductoInactivo`, `ErrorPersistencia` |
| `RespuestaRegistroCompra` | `Resultado`, `string Mensaje`, `int? IdCompra`, `DateTime? FechaRegistro`, `decimal MontoTotal`, `int? IndiceLineaAfectada`, `int? IdProductoAfectado`, `CampoPrecio? CampoPrecio`, `MotivoPrecioInvalido? MotivoPrecio` | `ResultadoRegistroCompra`: `Exito`, `PermisoDenegado`, `ProveedorInvalido`, `UsuarioInvalido`, `ProductoInvalido`, `CantidadInvalida`, `PrecioInvalido`, `SinItems`, `MontoTotalSuperaMaximo`, `ErrorPersistencia` |
| `RespuestaHistorialCompras` | `Resultado`, `string Mensaje`, `IReadOnlyList<HistorialCompraDto> Compras` | `ResultadoHistorialCompras`: `ConCompras`, `SinCompras`, `PermisoDenegado`, `ErrorPersistencia` |
| `RespuestaCompraConDetalle` | `Resultado`, `string Mensaje`, `CompraConDetalleDto? Compra` | `ResultadoCompraConDetalle`: `Encontrada`, `CompraNoEncontrada`, `PermisoDenegado`, `ErrorPersistencia` |

> **Criterio único de error de persistencia (RF-45, RF-56, RF-57)**: cada enum de respuesta declara **un solo** valor `ErrorPersistencia` —sin variantes, alias ni valores sin uso— y `CompraNegocio` lo produce **siempre** desde el mismo filtro `EsFalloDeOrigenDeDatos(Exception)` (§4.6), previa desvinculación de lo agregado al contexto. El **texto** de registro, historial y detalle es **uno solo y compartido** (`MensajeErrorPersistencia`, §5); las búsquedas y el prellenado conservan un texto propio porque RF-57 exige identificar la operación de lectura que falló. Cada valor `ErrorPersistencia` queda ejercitado por al menos un caso de prueba (G10 para escritura; G11 para búsquedas, prellenado, historial y detalle) y ninguno se usa para informar un error de validación (RNF-8).

### 3.4 DTOs de lectura (consulta)

| DTO | Propiedades y tipos | Regla |
|---|---|---|
| `HistorialCompraDto` | `int IdCompra`, `DateTime FechaRegistro`, `string RazonSocialProveedor`, `string NombreUsuario`, `decimal MontoTotal`, `bool Estado` | RF-49/RF-50/RNF-12: fecha como `DateTime` y estado como `bool`; **sin** formateo ni textos. `NombreUsuario` = `"{Nombre} {Apellido}"` recortado (solo nombre y apellido, RNF-5). `IdCompra` es la excepción documentada de RNF-5. |
| `CompraConDetalleDto` | `int IdCompra`, `DateTime FechaRegistro`, `string RazonSocialProveedor`, `string NombreUsuario`, `decimal MontoTotal`, `bool Estado`, `IReadOnlyList<LineaCompraDetalleDto> Lineas` | Cabecera de RF-54. |
| `LineaCompraDetalleDto` | `string CodigoProducto`, `string NombreProducto`, `int Cantidad`, `decimal PrecioCompra`, `decimal Subtotal` | **Sin** `PrecioVenta` (D-2/D-17). |

> **Navegaciones nulas en las consultas de lectura (M11)**: las proyecciones de `ObtenerHistorialAsync` y `ObtenerCompraConDetalleAsync` son **defensivas**: si la navegación a `Proveedor` o a `Usuario` es nula o su texto está vacío, el dato de lectura lleva el texto fijo `«(Sin proveedor)»` o `«(Sin usuario)»` respectivamente, **sin lanzar excepción** y sin exponer ningún dato adicional del usuario (RNF-5). El texto es idéntico en historial y en detalle para la misma inconsistencia, no se formatea en el negocio y se verifica en G12 y G13 con una compra sembrada con `IdProveedor`/`IdUsuario` inexistentes (el proveedor InMemory no valida claves foráneas). En datos consistentes esos textos nunca aparecen.

### 3.5 Enums nuevos (`CapaNegocio/Enums/`)

| Enum | Valores |
|---|---|
| `ResultadoBusquedaCompras` | `ConResultados`, `SinResultados`, `ErrorPersistencia` |
| `ResultadoPreciosProducto` | `Exito`, `ProductoNoEncontrado`, `ProductoInactivo`, `ErrorPersistencia` |
| `ResultadoOperacionLineaCompra` | `Exito`, `ExitoConAcumulacion`, `LineaNoEncontrada`, `ProductoInvalido`, `ProductoDuplicadoEnEdicion`, `CantidadInvalida`, `PrecioInvalido`, `CantidadAcumuladaSuperaMaximo`, `ErrorPersistencia` |
| `ResultadoRegistroCompra` | `Exito`, `PermisoDenegado`, `ProveedorInvalido`, `UsuarioInvalido`, `ProductoInvalido`, `CantidadInvalida`, `PrecioInvalido`, `SinItems`, `MontoTotalSuperaMaximo`, `ErrorPersistencia` |
| `ResultadoHistorialCompras` | `ConCompras`, `SinCompras`, `PermisoDenegado`, `ErrorPersistencia` |
| `ResultadoCompraConDetalle` | `Encontrada`, `CompraNoEncontrada`, `PermisoDenegado`, `ErrorPersistencia` |
| `CampoPrecio` | `PrecioCompra`, `PrecioVenta` |
| `MotivoPrecioInvalido` | `Vacio`, `FormatoInvalido`, `NoPositivo`, `DecimalesExcedidos`, `SuperaMaximo` |

`CampoPrecio` + `MotivoPrecioInvalido` permiten que las pruebas distingan cada caso de RF-22 … RF-26 sin parsear textos; el **texto** sigue siendo responsabilidad exclusiva del negocio (RF-55).

### 3.6 Constantes de límites (`CalculoImportesCompra`)

| Constante | Valor | RF |
|---|---|---|
| `CantidadMinima` | `1` | D-15 / RF-20 |
| `CantidadMaxima` | `1_000_000` | D-15 / RF-21, RF-63 |
| `PrecioMinimo` | `0.01m` | D-15 / RF-22, RF-23 |
| `PrecioMaximo` | `999_999_999.99m` | D-15 / RF-24 |
| `MontoTotalMaximo` | `9_999_999_999_999_999.99m` | D-20 / RF-64 |
| `DecimalesPrecio` | `2` | D-12 / RF-26 |

---

## 4. Contratos de métodos de `CompraNegocio`

`public class CompraNegocio` con `AppDbContext` inyectado por constructor y `ArgumentNullException.ThrowIfNull` (patrón `ProveedorNegocio`). **Solo** son `async Task` los métodos que acceden al origen de datos (RNF-2); los métodos sin trabajo asíncrono son **síncronos y sin la palabra clave `async`** (M14): `CalcularSubtotal`, `CalcularMontoTotal`, `CalcularSubtotalEnPreparacion`, `QuitarLinea`, `ObtenerMensajeConfirmacionCancelarCompra`. La única excepción deliberada es `AplicarEfectosSecundariosAsync`, que devuelve `Task.CompletedTask` **sin** `async` por ser el hook de HU-10. Regla de revisión: **ninguna firma `async` sin `await`** (sin advertencia CS1998). Todas las consultas son `AsNoTracking()` con proyección (RNF-3). Los métodos de cálculo **no** tocan la base.

### 4.1 Búsquedas y prellenado

| Firma | Devuelve | RF |
|---|---|---|
| `Task<RespuestaBusquedaProveedoresCompra> BuscarProveedoresAsync(string? textoBusqueda)` | Solo activos, coincidencia parcial por `Cuit` o `RazonSocial` (`ToUpper().Contains`), `OrderBy(RazonSocial).ThenBy(IdProveedor)`, `Take(20)`, proyección a `ProveedorSeleccionadoDto`. Texto vacío ⇒ `SinResultados` **sin consultar**. | RF-9, RF-10, RF-11, RF-12 |
| `Task<RespuestaBusquedaProductosCompra> BuscarProductosAsync(string? textoBusqueda)` | Solo activos, parcial por `Codigo` o `Nombre`, `OrderBy(Nombre).ThenBy(IdProducto)`, `Take(20)`, proyección a `ProductoBuscadoDto`. Texto vacío ⇒ lista vacía **sin consultar**. | RF-15, RF-16, RF-17 |
| `Task<RespuestaPreciosProducto> ObtenerPreciosProductoAsync(int idProducto)` | `Productos.AsNoTracking().Where(IdProducto).Select(PrecioCompra, PrecioVenta, Estado)`; inexistente/inactivo ⇒ resultado propio. **No escribe** (RF-41). | RF-38, RF-39, RF-41 |

### 4.2 Cálculo (sin base de datos)

| Firma | Devuelve | RF |
|---|---|---|
| `static decimal CalculoImportesCompra.CalcularSubtotal(int cantidad, decimal precioCompra)` | `cantidad * precioCompra` en `decimal`, sin redondeo intermedio. | RF-33, RNF-1 |
| `static decimal CalculoImportesCompra.CalcularMontoTotal(IEnumerable<decimal> subtotales)` | **Única** firma del cálculo del total: suma exacta de los subtotales recibidos. La grilla pasa los `Subtotal` de las líneas agregadas y `RegistrarCompraAsync` pasa los subtotales **recalculados** con `CalcularSubtotal`; nunca existe una segunda sobrecarga por `LineaCompraDto`. | RF-7, RF-33, RF-34, RF-65, RNF-1 |
| `SubtotalLineaDto CalcularSubtotalEnPreparacion(int? idProducto, string? cantidad, string? precioCompra, string? precioVenta)` | **Síncrono**. `EsValido = true` solo si hay producto elegido y cantidad y **ambos** precios son válidos (analizadores); en ese caso `Subtotal` calculado. Sin consultar la base. | RF-34, RF-65, D-21 |
| `RespuestaCompraEnCarga QuitarLinea(IReadOnlyList<LineaCompraDto> lineas, int indice)` | **Síncrono**. Copia de la lista sin la línea indicada + monto total recalculado con `CalcularMontoTotal`. Si el índice está **fuera de rango** (`indice < 0` o `indice >= lineas.Count`) devuelve `Exito = false`, `Mensaje` de línea inexistente del catálogo (§5) y la lista **intacta** con su total. Sin base de datos. | RF-29, RNF-4 |
| `string ObtenerMensajeConfirmacionCancelarCompra()` | Texto de confirmación redactado por el negocio (la pantalla solo lo muestra). | RF-31, RF-55 |

### 4.3 Operaciones sobre el detalle

| Firma | Devuelve | RF |
|---|---|---|
| `Task<RespuestaOperacionLineaCompra> AgregarLineaAsync(SolicitudAgregarLineaCompraDto solicitud)` | Valida producto (existencia y actividad, consulta acotada), cantidad, precio de compra y precio de venta en el orden de RF-27. Si el producto ya está en la grilla **acumula** cantidad y toma los precios efectivos; si la acumulación supera 1.000.000 rechaza sin tocar la línea. Rechazos y avisos con los textos del catálogo (§5). | RF-18, RF-20 … RF-27, RF-36, RF-37, RF-42, RF-43, RF-63 |
| `Task<RespuestaOperacionLineaCompra> EditarLineaAsync(SolicitudEditarLineaCompraDto solicitud)` | Primero el **índice**: si está fuera de rango (`< 0` o `>= LineasActuales.Count`) devuelve `LineaNoEncontrada` con la lista **intacta**, sin consultar la base. Si el índice es válido, igual orden de validación; si el producto elegido ya está en **otra** línea rechaza sin modificar nada; si no, **reemplaza** la línea del índice indicado. | RF-30, RF-62, RF-18, RF-20 … RF-27 |

### 4.4 Registro transaccional

| Firma | Devuelve | RF |
|---|---|---|
| `Task<RespuestaRegistroCompra> RegistrarCompraAsync(SolicitudRegistrarCompraDto solicitud, int idUsuario, string? rol)` | Valida rol → proveedor → usuario → líneas (producto, duplicado, cantidad, **precio de compra** y **precio de venta**: rango mínimo/≤ 0, **máximo de dos decimales**, máximo permitido) → ítems → tope del total; persiste cabecera + líneas en **una** transacción. Revalidación documentada en §6.4. | RF-5 … RF-8, RF-14, RF-19, RF-26, RF-27, RF-44 … RF-48, RF-59, RF-60, RF-64 |
| `private Task AplicarEfectosSecundariosAsync(Compra cabecera, CancellationToken ct = default)` | **Hook no-op** de HU-10: `return Task.CompletedTask;`, **sin** `async` (M14). Se invoca **dentro** de la transacción, después de escribir cabecera y líneas y antes del `CommitAsync`. Hoy no escribe nada. | RF-46, RF-47, RF-48 |

### 4.5 Consultas de solo lectura

| Firma | Devuelve | RF |
|---|---|---|
| `Task<RespuestaHistorialCompras> ObtenerHistorialAsync(string? rol)` | Todas las compras (activas e inactivas) con `OrderByDescending(FechaRegistro).ThenByDescending(IdCompra)`, proyección a `HistorialCompraDto` (navegaciones nulas ⇒ textos defensivos de §3.4). Rol primero. Sin paginación. | RF-49, RF-50, RF-51, RF-53(a), RF-61 |
| `Task<RespuestaCompraConDetalle> ObtenerCompraConDetalleAsync(int idCompra, string? rol)` | Cabecera + líneas (producto, cantidad, `PrecioCompra`, `Subtotal`), sin `PrecioVenta`, con la misma proyección defensiva de §3.4. Rol primero. | RF-53(b), RF-54, RF-61 |

### 4.6 Validaciones privadas (nombres propuestos)

`ValidarRolParaRegistro(string? rol)` · `ValidarProveedorAsync(int? idProveedor)` · `ValidarUsuarioAsync(int idUsuario)` · `ValidarProductosAsync(IEnumerable<int> ids)` (una sola consulta `Where(ids.Contains(...))`, acotada, que devuelve `Id, Codigo, Nombre, Estado`) · `ValidarCantidad(int cantidad)` · `ValidarPrecioDeTexto(string? texto, CampoPrecio campo)` (usa `AnalizadorImporte` y devuelve `MotivoPrecioInvalido` + mensaje del catálogo) · `ValidarPrecioRecibido(decimal valor, CampoPrecio campo)` (revalidación al confirmar: ≤ 0 → decimales → máximo, §6.4) · `ExisteProductoDuplicado(...)` · `ConstruirMensajeProducto(...)` · `ConstruirMensajePrecio(...)` · `EsFalloDeOrigenDeDatos(Exception)` (mismo criterio que `ProveedorNegocio`: `DbUpdateException`, `DbException`, `InvalidOperationException`).

> **Ubicación por tarea, para que cada tarea sea ejecutable con lo que ya existe (B2/I3/M15)**: en **T3** se crean el catálogo de mensajes (constantes privadas) y `ValidarCantidad`, `ValidarPrecioDeTexto`, `ValidarProductosAsync`, `ExisteProductoDuplicado`, `ConstruirMensajeProducto` y `ConstruirMensajePrecio`, que **se ejercitan** a través de `AgregarLineaAsync`/`EditarLineaAsync`/`CalcularSubtotalEnPreparacion`. En **T5a** se crean `ValidarProveedorAsync`, `ValidarUsuarioAsync` y `ValidarPrecioRecibido`, que solo se ejercitan a través de `RegistrarCompraAsync`, implementado en **T5b**. En **T8** se crea `ValidarRolParaRegistro` y se activa el **paso 1** del algoritmo. Ninguna tarea prueba un método privado que aún no exista ni depende de una tarea posterior.

---

## 5. Catálogo de mensajes (negocio, español, RF-55)

Los textos marcados **«spec»** son literales exigidos por la spec; los demás los redacta la lógica con el mismo criterio (RNF-8) y las pruebas los comparan por igualdad.

| Caso | RF | Resultado | Texto |
|---|---|---|---|
| Cantidad que supera el máximo | RF-21 | `CantidadInvalida` | **«La cantidad supera el máximo permitido.»** *(spec)* |
| Precio de compra fuera de rango | RF-24 | `PrecioInvalido` (`SuperaMaximo`) | **«El precio de compra supera el máximo permitido.»** *(spec)* |
| Precio de venta fuera de rango | RF-24 | `PrecioInvalido` (`SuperaMaximo`) | **«El precio de venta supera el máximo permitido.»** *(spec)* |
| Formato de precio de compra | RF-25 | `PrecioInvalido` (`FormatoInvalido`) | **«El precio de compra admite solo números, con coma o punto decimal y hasta dos decimales.»** *(spec)* |
| Formato de precio de venta | RF-25 | `PrecioInvalido` (`FormatoInvalido`) | **«El precio de venta admite solo números, con coma o punto decimal y hasta dos decimales.»** *(spec)* |
| Más de dos decimales (compra) | RF-26 | `PrecioInvalido` (`DecimalesExcedidos`) | «El precio de compra admite como máximo dos decimales.» |
| Más de dos decimales (venta) | RF-26 | `PrecioInvalido` (`DecimalesExcedidos`) | «El precio de venta admite como máximo dos decimales.» |
| Acumulación sobre el máximo | RF-63 | `CantidadAcumuladaSuperaMaximo` | **«La cantidad acumulada supera el máximo permitido.»** *(spec)* |
| Total sobre el tope | RF-64 | `MontoTotalSuperaMaximo` | **«El monto total de la compra supera el máximo permitido.»** *(spec)* |
| Cantidad vacía/cero/con texto/separadores | RF-20 | `CantidadInvalida` | «La cantidad debe ser un número entero mayor que cero escrito solo con dígitos.» |
| Precio de compra vacío | RF-22 | `PrecioInvalido` (`Vacio`) | «El precio de compra es obligatorio y debe ser mayor que cero.» |
| Precio de compra cero o negativo | RF-22 | `PrecioInvalido` (`NoPositivo`) | «El precio de compra debe ser mayor que cero.» |
| Precio de venta vacío | RF-23 | `PrecioInvalido` (`Vacio`) | «El precio de venta es obligatorio y debe ser mayor que cero.» |
| Precio de venta cero o negativo | RF-23 | `PrecioInvalido` (`NoPositivo`) | «El precio de venta debe ser mayor que cero.» |
| Sin proveedor seleccionado | RF-14 | `ProveedorInvalido` | «Debe seleccionar un proveedor antes de registrar la compra.» |
| Proveedor inexistente | RF-14 | `ProveedorInvalido` | «El proveedor seleccionado no existe. Vuelva a buscarlo.» |
| Proveedor inactivo | RF-14 | `ProveedorInvalido` | «El proveedor {RazonSocial} está inactivo y no puede registrar compras.» |
| Usuario sin identificar (`IdUsuario <= 0`) | RF-5 | `UsuarioInvalido` | «No se pudo identificar un usuario responsable válido para registrar la compra.» |
| Usuario inexistente | RF-5 | `UsuarioInvalido` | «El usuario responsable de la compra no existe.» |
| Usuario inactivo | RF-5 | `UsuarioInvalido` | «El usuario responsable de la compra está inactivo.» |
| Sin líneas | RF-19 | `SinItems` | «La compra debe tener al menos un ítem.» |
| Producto inexistente en una línea | RF-18 | `ProductoInvalido` | «El producto de la línea {n} no existe. Vuelva a seleccionarlo.» |
| Producto inactivo en una línea | RF-18 | `ProductoInvalido` | «El producto {Codigo} - {Nombre} de la línea {n} está inactivo y no puede comprarse.» |
| Producto repetido en el detalle (defensivo) | RF-37 | `ProductoInvalido` | «El producto {Codigo} - {Nombre} está repetido en el detalle de la compra.» |
| Edición con producto ya cargado en otra línea | RF-62 | `ProductoDuplicadoEnEdicion` | «El producto {Codigo} - {Nombre} ya está cargado en otra línea. Quite una de las dos líneas para acumular la cantidad.» |
| Línea agregada | RF-27 | `Exito` | «Línea agregada.» |
| Línea actualizada | RF-30 | `Exito` | «Línea actualizada.» |
| Acumulación aplicada | RF-36 | `ExitoConAcumulacion` | «Se acumuló la cantidad del producto {Codigo}: ahora tiene {Cantidad} unidades.» |
| Línea inexistente en el índice indicado | RF-29, RF-30 (defensivo) | `LineaNoEncontrada` / `Exito = false` | «La línea indicada no existe en el detalle de la compra.» |
| Registro exitoso | RF-44 | `Exito` | «Compra N° {IdCompra} registrada exitosamente.» |
| Rol no autorizado (registro) | RF-60 | `PermisoDenegado` | «No tiene permisos para registrar compras.» |
| Rol no autorizado (consultas) | RF-61 | `PermisoDenegado` | «No tiene permisos para consultar las compras.» |
| Sin proveedores coincidentes (texto no vacío) | RF-11 | `SinResultados` | «No se encontraron proveedores que coincidan con la búsqueda.» |
| Texto vacío en la búsqueda de proveedores | RF-10 | `SinResultados` | **Sin mensaje**: `Mensaje` vacío; la pantalla no lo muestra ni muestra «sin resultados» |
| Sin productos coincidentes (texto no vacío) | RF-17 | `SinResultados` | «No se encontraron productos que coincidan con la búsqueda.» |
| Texto vacío en la búsqueda de productos | RF-16 | `SinResultados` | **Sin mensaje**: `Mensaje` vacío |
| Búsqueda de proveedores con resultados | RF-9 | `ConResultados` | «Se encontraron {n} proveedores.» / «Se encontró 1 proveedor.» |
| Búsqueda de productos con resultados | RF-15 | `ConResultados` | «Se encontraron {n} productos.» / «Se encontró 1 producto.» |
| Prellenado aplicado | RF-38 | `Exito` | «Se propusieron los precios actuales del catálogo.» |
| Producto inexistente al prellenar | RF-38 | `ProductoNoEncontrado` | «El producto indicado no existe.» |
| Producto inactivo al prellenar | RF-38 | `ProductoInactivo` | «El producto indicado está inactivo y no puede comprarse.» |
| Historial vacío | RF-51 | `SinCompras` | «Todavía no hay compras registradas.» |
| Historial con datos | RF-49 | `ConCompras` | «Se encontraron {n} compras.» / «Se encontró 1 compra.» |
| Compra con detalle inexistente | RF-54 | `CompraNoEncontrada` | «La compra indicada no existe.» |
| Fallo de datos al registrar, al cargar el historial o al consultar el detalle | RF-45, RF-56, RF-57 | `ErrorPersistencia` | **«No se pudo completar la operación por un problema con el origen de datos. Intente nuevamente.»** *(texto único, constante `MensajeErrorPersistencia`; mismo criterio y mismo texto que `ProveedorNegocio` y `ClienteNegocio`)* |
| Fallo de datos al buscar proveedores | RF-57 | `ErrorPersistencia` | «No se pudo buscar proveedores por un problema con el origen de datos. Intente nuevamente.» |
| Fallo de datos al buscar productos | RF-57 | `ErrorPersistencia` | «No se pudo buscar productos por un problema con el origen de datos. Intente nuevamente.» |
| Fallo de datos al prellenar | RF-57 | `ErrorPersistencia` | «No se pudieron obtener los precios del producto por un problema con el origen de datos. Intente nuevamente.» |
| Confirmación de cancelar/limpiar | RF-31 | — (texto) | «¿Confirma que desea descartar el proveedor y todas las líneas de la compra en carga?» (título: «Cancelar compra») |

> **Ningún** mensaje de límites, decimales ni formato coincide con el genérico de persistencia (RNF-8).
>
> **Un solo texto de persistencia (M12)**: registro, historial y detalle comparten la constante `MensajeErrorPersistencia`; las tres búsquedas y el prellenado tienen su texto propio solo para que el usuario sepa qué operación falló (RF-57). No se declaran dos textos distintos para el mismo caso ni valores de enum sin uso: cada valor `ErrorPersistencia` de §3.5 tiene al menos un caso que lo produce (G10 y G11).

---

## 6. Algoritmo de validación (RF-27 y RF-28)

### 6.1 Precedencia global

1. **Rol primero** (RF-28, RF-59): `PoliticaAcceso.PuedeOperarCompras(rol)`; si es falso ⇒ `PermisoDenegado` y **no se valida nada más ni se consulta la base**.
2. **Proveedor** (RF-14).
3. **Usuario de sesión** (RF-5): `idUsuario > 0`, existe, activo. El rol **no** se relee de la base (D-14).
4. **Líneas en el orden de la grilla** (RF-28): producto → cantidad → precio de compra → precio de venta.
5. **Unicidad de producto** (RF-37): se verifica junto con la validación de producto; el primer producto repetido rechaza (garantía de la lógica, sin índice en la base).
6. **Al menos un ítem** (RF-19), después del recorrido de líneas (con cero líneas el recorrido no produce error y este paso dispara).
7. **Tope del monto total** (RF-64, D-20), último, con la misma suma que se usa para mostrar y guardar.

### 6.2 Orden al agregar o guardar una línea (RF-27)

`producto` (existencia + actividad) → `duplicado en edición` (RF-62) → `cantidad` (RF-20 → RF-21) → `precio de compra` → `precio de venta`.
Si el producto ya existe en la grilla **y no se está editando**, después de validar se intenta **acumular**: si la suma supera 1.000.000 ⇒ RF-63 (línea intacta).

### 6.3 Orden dentro de un mismo precio (RF-28)

`AnalizadorImporte.Analizar(texto)` devuelve un único motivo, evaluado en este orden exacto:

| # | Condición | Motivo | RF |
|---|---|---|---|
| 1 | `null`, vacío o solo espacios (tras Recorte) | `Vacio` | RF-22 / RF-23 |
| 2 | Más de un separador (`1.2.3`, `1.234,56`, `1,234.56`), separador sin dígitos a un lado (`10,`, `,5`), o cualquier carácter distinto de dígitos, un único separador (`,` o `.`) y un único `-` inicial | `FormatoInvalido` | RF-25 |
| 3 | Valor numérico `<= 0` (`0`, `0,00`, `-5`) | `NoPositivo` | RF-22 / RF-23 |
| 4 | Más de dos dígitos decimales (`10,333`, `0,001`, `1.000`) | `DecimalesExcedidos` | RF-26 |
| 5 | Valor `> 999.999.999,99` (incluye desborde de `decimal`) | `SuperaMaximo` | RF-24 |
| — | Caso contrario | válido, `Valor` | — |

Detalles fijados:
- El separador se interpreta **siempre** como decimal: `1.000` = 1,000 ⇒ paso 4 (RF-26/D-12).
- El signo `-` inicial se admite **solo** para poder expresar el valor negativo, y es **regla de la spec** (RF-25 y RF-28, paso 2): `-5` es un valor numérico y cae en el paso 3 (`NoPositivo`); `+5`, `5-`, `- 5`, `--5`, los espacios internos y las letras caen en el paso 2 (`FormatoInvalido`). No hay interpretación ni decisión propia del plan: la implementación se limita a lo que la spec fija y esos casos se prueban en G2a.
- Sin redondeo ni truncamiento en ningún paso (D-12, RNF-1).
- La cantidad (`AnalizadorCantidad.Analizar`) se evalúa antes, con su propio orden: **solo dígitos** (si no ⇒ RF-20) → **mayor que cero** (si no ⇒ RF-20) → **`<= 1.000.000`** (si no ⇒ RF-21; incluye desborde de `int`).

### 6.4 Revalidación al confirmar

`RegistrarCompraAsync` **no** confía en los valores de la grilla:

1. Recalcula `cantidad * precioCompra` de cada línea con `CalculoImportesCompra.CalcularSubtotal` y el total con `CalcularMontoTotal(IEnumerable<decimal>)` sobre esos subtotales recalculados; el `Subtotal` que llega en el DTO **se ignora**.
2. **Revalida la escala decimal** de cada precio recibido (RF-26): el DTO transporta `decimal`, así que un valor con tres decimales (`10.333m`) es representable y **no** puede darse por validado. Se aplica `ValidarPrecioRecibido`, con el **mismo orden interno** de §6.3 una vez descartados los pasos que dependen del texto: **(a)** `≤ 0` ⇒ mensaje de mayor que cero (RF-22/RF-23, `NoPositivo`); **(b)** más de dos decimales ⇒ mensaje propio de RF-26 (`DecimalesExcedidos`); **(c)** `> 999.999.999,99` ⇒ mensaje propio de RF-24 (`SuperaMaximo`). Los pasos «vacío» y «formato» no aplican: el DTO no transporta texto.
3. Revalida los rangos mínimos de D-15 y el tope de D-20 (RF-64).
4. El orden de la línea al confirmar es el de §6.1: producto → duplicado → cantidad → **precio de compra (≤ 0 → decimales → máximo)** → **precio de venta (mismo orden)**; el primer error corta el recorrido y devuelve `PrecioInvalido` con `CampoPrecio`, `MotivoPrecioInvalido` e `IndiceLineaAfectada`.

*Alternativa descartada*: suponer que la escala decimal ya quedó validada al agregar la línea (era el supuesto del plan anterior). RF-27 exige revalidar la compra completa al confirmar y `RegistrarCompraAsync` es un método público de la lógica: un DTO armado fuera de la pantalla podría llevar `10.333m`, que el motor redondearía a `10.33` al guardar, contra RNF-1. También se descarta reenviar los textos crudos en el registro (obligaría a la pantalla a conservar texto por línea y duplicaría la validación de formato sin aportar cobertura nueva).

### 6.5 Subtotal del área de captura (RF-65 / D-21)

`CalcularSubtotalEnPreparacion` devuelve `EsValido = false` (⇒ «—», sin sumar al total) si falta **alguno** de: producto elegido (`idProducto > 0`), cantidad válida, precio de compra válido, precio de venta válido. Es **síncrono** (M14) y usa los mismos `AnalizadorImporte`/`AnalizadorCantidad` que la validación real, sin duplicar reglas (RNF-13). No consulta la base: la validez del producto se comprobó al seleccionarlo en la búsqueda (solo activos) y se revalida al agregar y al confirmar. El **monto total general** se obtiene con `CalcularMontoTotal(_lineas.Select(l => l.Subtotal))`, es decir, **solo** con las líneas ya agregadas.

---

## 7. Algoritmo de registro transaccional (RF-44 … RF-46)

```text
RegistrarCompraAsync(solicitud, idUsuario, rol)
 1. Rol            → PoliticaAcceso.PuedeOperarCompras(rol)   [sin tocar la base; se activa en T8]
 2. Proveedor      → IdProveedor.HasValue && > 0
                     → Compras? no: Proveedores.AsNoTracking().Where(Id).Select(Id, RazonSocial, Estado)
                     → inexistente ⇒ ProveedorInvalido · inactivo ⇒ ProveedorInvalido
 3. Usuario        → idUsuario > 0
                     → Usuarios.AsNoTracking().Where(Id).Select(Id, Estado)
                     → inexistente/inactivo ⇒ UsuarioInvalido
 4. Líneas         → una sola consulta acotada: Productos.Where(ids.Contains(IdProducto)).Select(Id, Codigo, Nombre, Estado)
                     → por línea, en el orden de la grilla: existe/activo (RF-18) → duplicado (RF-37)
                       → cantidad 1..1.000.000                                    (RF-20, RF-21)
                       → precio compra: > 0 → ≤ 2 decimales → ≤ 999.999.999,99   (RF-22, RF-26, RF-24)
                       → precio venta:  > 0 → ≤ 2 decimales → ≤ 999.999.999,99   (RF-23, RF-26, RF-24)
                     → primer error ⇒ respuesta con ÍndiceLineaAfectada / IdProductoAfectado /
                                      CampoPrecio / MotivoPrecio (RF-28), sin escribir nada
 5. Ítems          → lineas.Count == 0 ⇒ SinItems
 6. Monto total    → CalcularMontoTotal(subtotales recalculados con CalcularSubtotal(cantidad, precioCompra))
                     → > 9.999.999.999.999.999,99 ⇒ MontoTotalSuperaMaximo (RF-64)
 7. Transacción    → await _context.Database.BeginTransactionAsync()
 8. Cabecera       → new Compra { IdUsuario, IdProveedor, MontoTotal, Estado = true,
                                  FechaRegistro = DateTime.Now }   [RF-4: reloj del equipo, hora del servidor de aplicación]
 9. Líneas         → por cada LineaCompraDto: new DetalleCompra { IdProducto, Cantidad,
                                  PrecioCompra, Subtotal = CalcularSubtotal(...) }  (sin PrecioVenta, D-2)
10. Escritura      → cabecera.Detalles = lineas; _context.Compras.Add(cabecera);
                     await _context.SaveChangesAsync();      [EF inserta cabecera y luego líneas; IdCompra se resuelve solo]
11. Hook HU-10     → await AplicarEfectosSecundariosAsync(cabecera);   [hoy no-op; mismo ámbito transaccional]
12. Commit         → await transaccion.CommitAsync()
13. Éxito          → RespuestaRegistroCompra { Exito, IdCompra, FechaRegistro, MontoTotal, mensaje }
14. Excepción      → rollback (try/catch RollbackAsync) + desadjuntar cabecera y líneas
                     (Entry(...).State = Detached) + ErrorPersistencia con el mensaje de RF-45/RF-56
15. finally        → DisposeAsync de la transacción
```

Decisiones del algoritmo:

- **El paso 1 se activa en T8**: en **T5a/T5b** el método recibe `rol` pero **no** lo valida (el parámetro existe desde el principio para no cambiar la firma después). Los pasos que implementa T5b son **2–13**; los pasos 14 y 15 (manejo de excepción y `finally`) llegan en **T6**. Las pruebas de T5b, T6 y T7 invocan con `"Administrador"`, así que activar el paso 1 no las altera.
- **Una sola transacción explícita** (`BeginTransactionAsync` / `DbContextTransaction`) que cubre cabecera, líneas y el hook de HU-10: si cualquiera falla, `RollbackAsync` deshace todo (RF-44, RF-45).
- **Toda la validación ocurre antes de abrir la transacción** (pasos 1 a 6): si algo se rechaza no se consulta la base para escribir ni se abre conexión, y lo ingresado se conserva en pantalla (RNF-11). La revalidación incluye la **escala decimal** de cada precio recibido (§6.4, RF-26/RF-27).
- **Un único `SaveChangesAsync`** para cabecera + líneas: EF ordena los `INSERT` por dependencia de clave foránea y obtiene `IdCompra` en la misma operación. *Alternativa descartada*: dos `SaveChangesAsync` (cabecera y luego líneas); duplica el viaje, deja una ventana con cabecera sin líneas dentro de la transacción y complica el desadjunte ante error, sin ventaja verificable.
- **Manejo de `DbUpdateException`**: se captura junto con `DbException` e `InvalidOperationException` (mismo filtro que `ProveedorNegocio`), se intenta el `RollbackAsync` (protegido, sin relanzar) y se devuelve `ErrorPersistencia`. **Nunca** se propaga la excepción a la interfaz (RF-55/RF-56) y **nunca** se informa un mensaje de límites como error de persistencia (RNF-8).
- **Hook HU-10 (`AplicarEfectosSecundariosAsync`)**: método privado que hoy devuelve `Task.CompletedTask` (sin `async`, M14) y se invoca entre la escritura y el commit. Así HU-10 (stock + precios del producto) se suma «a la misma operación» sin cambiar la forma de registrar (RF-46) y hoy no se escribe nada sobre `Producto` (RF-47/RF-48). *Alternativa descartada*: un método público separado que la pantalla llame después del registro (rompe la atomicidad y obliga a HU-10 a rehacer la transacción).
- **Proveedor y usuario**: se consultan con proyección mínima y `AsNoTracking`; no se adjuntan entidades de navegación a la cabecera (solo se asignan `IdProveedor`/`IdUsuario`) para no arrastrar `UPDATE`s accidentales (RF-48).
- **Fecha**: `DateTime.Now` del equipo de aplicación (RF-4), asignada explícitamente al construir la cabecera.
- **Estado**: `true` siempre (RF-4); no hay campo de estado en la pantalla.

---

## 8. Persistencia (sin cambios)

| Elemento | Situación |
|---|---|
| `AppDbContext` | Sin cambios: `Compras` y `DetallesCompra` ya existen; el modelo se resuelve por convenciones (mismas columnas y `decimal(18,2)` que las migraciones aplicadas). |
| Migraciones | **Ninguna**. No se ejecuta `dotnet ef migrations add` ni `database update` (RNF-10). |
| Índices | **Ninguno nuevo**, en particular **no** se agrega un índice único por compra y producto (RF-37/RNF-10). |
| Trigger `TR_PRODUCTO_FECHA_MODIFICACION` | Se conserva la configuración `HasTrigger` existente y **no** se toca `PRODUCTO` al registrar (RF-47). |
| Borrado físico | No se ejecuta `DELETE` ni `Remove` (RNF-4): quitar una línea opera solo sobre la lista en memoria. |
| Scripts | `scripts/preparar-bd.ps1` no se modifica: la verificación manual usa datos reales de proveedores/productos ya sembrados. |

---

## 9. Estrategia de pruebas (`dotnet test`)

Archivo nuevo `CapaNegocio.Tests/CompraNegocioTests.cs`. Helpers: `CrearContextoVacioAsync()` con `UseInMemoryDatabase($"CompraNegocioTests-{Guid.NewGuid()}")` + `EnsureCreatedAsync()`, y sembradores `SembrarRolAsync`, `SembrarUsuarioAsync`, `SembrarProveedorAsync`, `SembrarProductoAsync`, `SembrarCompraAsync`, siguiendo el patrón de `ProveedorNegocioTests` / `ClienteNegocioTests`. Constantes de mensajes al inicio del archivo para comparar por igualdad. Los grupos se reparten por tarea y **ninguno prueba un método que todavía no exista** (B2 e I3): los grupos de cálculo puro y de analizadores (T2) comparan **motivos tipados** y no textos; los textos exactos del catálogo se comparan en T3, T5b y T6, cuando el catálogo ya existe. **Ningún vector *válido* usa puntos de miles en un texto de entrada** (G2a y G3 incluyen a propósito `1.000.000.000,00`, `1.000` y `1.000.001` como casos de rechazo): el separador se interpreta siempre como decimal (RF-25), así que `999999999,99` es el máximo válido y `1.000.000.000,00` es un error de **formato**; la cantidad solo admite dígitos (RF-20), así que `1000000` es el máximo válido y `1.000.000` es un error de **cantidad**.

| Grupo | Tarea | Casos representativos | RF cubiertos |
|---|---|---|---|
| **G1a · Cálculo puro** (`CalculoImportesCompra`, sin EF y sin catálogo de mensajes) | T2 | 3 × 10,33 = 30,99; 1 × 10,5 = 10,5 (`10.5m`, sin formateo: el cálculo puro no produce textos); suma exacta sin diferencias de centavos con la **única** firma `CalcularMontoTotal(IEnumerable<decimal>)`; total con 0/1/N líneas; el **precio de venta no altera** el total. | RF-7, RF-33, RF-35, RNF-1, RNF-13 |
| **G2a · Analizadores** (`AnalizadorImporte` y `AnalizadorCantidad`, sin catálogo) | T2 | `10,33` = `10.33`; `1.000` ⇒ `DecimalesExcedidos`; `10,333` y `0,001` ⇒ `DecimalesExcedidos`; **`999999999,99` y `999999999.99` aceptados**; **`1000000000` y `1000000000.00` ⇒ `SuperaMaximo`**; **`1.000.000.000,00` ⇒ `FormatoInvalido`** (tiene puntos de miles); `1.234,56`, `1,234.56`, `1.2.3`, `10,`, `,5`, `abc`, `1 0` y **`5-`, `- 5`, `--5`, `+5` ⇒ `FormatoInvalido`**; `0` y **`-5` ⇒ `NoPositivo`**; vacío ⇒ `Vacio`; desborde de `decimal` ⇒ `SuperaMaximo`; `0,01` aceptado; precedencia vacío → formato → ≤ 0 → decimales → máximo (RF-25 y RF-28). | RF-22, RF-23, RF-24, RF-25, RF-26, RF-28 |
| **G2b · Textos de precio** (`AgregarLineaAsync`/`EditarLineaAsync` con el catálogo de T3) | T3 | Los mismos textos con los que la interfaz informa: «El precio de compra/venta admite solo números, con coma o punto decimal y hasta dos decimales.» (formato), «…admite como máximo dos decimales.» y «…supera el máximo permitido.»; `-5` ⇒ «…debe ser mayor que cero.»; comparación **por igualdad** contra las constantes del archivo de pruebas. | RF-22 … RF-26, RF-28, RF-55 |
| **G3 · Cantidad** | T2 | `1` y `1000000` aceptados; `0`, vacío, `-1`, `1,5`, `1.000`, `10 0`, `dos`, `+5` y **`1.000.001` ⇒ RF-20** (la cantidad solo admite dígitos); `1000001` y `99999999999` ⇒ RF-21. | RF-20, RF-21, D-10, D-15 |
| **G1b · Preparación y quita** (`CalcularSubtotalEnPreparacion`, `QuitarLinea`) | T3 | Preparación con «abc»/«0»/«10,333»/sin producto ⇒ `EsValido = false`; preparación válida ⇒ subtotal y **no** suma al total de la grilla; quitar recalcula; quitar la única línea deja total 0; **quitar un índice fuera de rango ⇒ `Exito = false`, mensaje de línea inexistente y lista intacta**. | RF-29, RF-34, RF-65, RNF-4, RNF-12, RNF-13 |
| **G4 · Validación de línea** | T3 | Producto inexistente / inactivo ⇒ `ProductoInvalido` con línea y código identificados; orden producto → duplicado → cantidad → precio compra → precio venta (caso con varios errores ⇒ solo el primero); éxito agrega y calcula subtotal. | RF-18, RF-27, RF-28 |
| **G5 · Operaciones sobre el detalle** | T3 | Agregar producto nuevo; **acumular** repetido tomando los precios efectivos (no los del catálogo) con aviso; 999999 + 2 ⇒ `CantidadAcumuladaSuperaMaximo` y línea intacta; 999999 + 1 ⇒ 1000000 aceptado; nunca dos líneas del mismo producto; **editar** reemplaza sin duplicar; **editar con índice fuera de rango ⇒ `LineaNoEncontrada` sin tocar la lista**; editar eligiendo un producto ya cargado en otra línea ⇒ rechazo sin cambios en ninguna de las dos (RF-62); volver a agregar un producto quitado: se prueba **solo la acumulación** según el estado vigente de la grilla —el **prellenado** de RF-43 es de pantalla y se verifica manualmente en T9b/T10, igual que RF-38—. | RF-29, RF-30, RF-36, RF-37, RF-42, RF-43 (solo acumulación), RF-62, RF-63 |
| **G6 · Prellenado** | T4 | `ObtenerPreciosProductoAsync` devuelve los precios del catálogo, producto con precios 0 los devuelve en 0, inactivo ⇒ `ProductoInactivo`, inexistente ⇒ `ProductoNoEncontrado`; el prellenado **no** modifica stock, precios, usuario, fecha ni motivo del producto (comparación antes/después). | RF-38, RF-39, RF-40, RF-41, RF-47 |
| **G7 · Búsquedas** | T4 | Proveedores: parcial por documento y por razón social, insensible a mayúsculas, solo activos, tope 20, orden razón social + id, sin coincidencias con mensaje; Productos: idem por código y nombre, orden nombre + id. **Texto vacío sin consultar**: se invoca con el contexto ya dispuesto (§9.1) y debe devolver `SinResultados` con lista vacía —y **no** `ErrorPersistencia`—, lo que demuestra que no se ejecutó ninguna consulta. Se verifica además que la proyección **no** traiga correo ni teléfono (RNF-5). | RF-9, RF-10, RF-11, RF-12, RF-15, RF-16, RF-17 |
| **G8 · Registro exitoso** | T5b | Compra con 1 y con N líneas: persiste cabecera (usuario, proveedor, estado activo, fecha entre el instante previo y el posterior) y todas las líneas con cantidad, `PrecioCompra` y subtotal calculado; `MontoTotal` = suma; el `Subtotal` del DTO se **ignora** y se recalcula; **no** se escribe `IdUsuarioModifica`, `FechaModificacion`, `MotivoAjusteStock`, stock ni precios del producto (RF-47/RF-48); `DetalleCompra` no tiene precio de venta. Se invoca con `"Administrador"`. | RF-4, RF-6, RF-7, RF-33, RF-44, RF-46, RF-47, RF-48 |
| **G9 · Registro rechazado** | T5b | Sin proveedor / proveedor inexistente / proveedor dado de baja después de seleccionarlo; usuario 0, inexistente e inactivo; sin líneas; producto desactivado entre la carga y la confirmación; **dos líneas del mismo `IdProducto` en la solicitud y el resto válido ⇒ `ProductoInvalido` con el mensaje de producto repetido en el detalle y 0 filas (RF-37, RNF-6)**; **precio de compra con tres decimales (`10.333m`) enviado directo al negocio ⇒ `PrecioInvalido` + `DecimalesExcedidos` con el mensaje de RF-26 y 0 filas** (idem con el precio de venta); **precio de compra o de venta `1000000000m` ⇒ `PrecioInvalido` + `SuperaMaximo` con el mensaje propio de RF-24 y 0 filas**; total sobre el tope (**11 líneas** de `1000000` × `999999999.99m`, única forma de superar 9.999.999.999.999.999,99 con los máximos de D-15); precedencia del primer error (proveedor → usuario → líneas → sin ítems). En todos: 0 compras y 0 detalles. Se invoca con `"Administrador"`. | RF-5, RF-14, RF-18, RF-19, RF-24, RF-26, RF-27, RF-28, RF-37, RF-64 |
| **G10 · Atomicidad** | T6 | Doble `FalloDbContext` sembrado y ejecutado según §9.1 ⇒ `ErrorPersistencia` con el texto único de §5 y **0** filas en `Compras` y `DetallesCompra` verificadas con el contexto sano. | RF-45, RF-56 |
| **G11 · Fallos de lectura** | T4 y T7 | Contexto dispuesto (`await db.DisposeAsync()`) ⇒ `Resultado == ErrorPersistencia` con la constante `MensajeErrorPersistencia` en búsqueda de proveedores, búsqueda de productos y prellenado (T4) —en el prellenado **`Precios == null`**, porque `RespuestaPreciosProducto` no tiene lista—, e historial y compra con detalle (T7); el resultado queda reintentable (no lanza). | RF-57 |
| **G12 · Historial** | T7 | Vacío ⇒ `SinCompras` + mensaje; con 2 compras ⇒ orden fecha descendente; misma fecha y hora ⇒ desempate por `IdCompra` descendente; incluye inactivas con `Estado = false`; `FechaRegistro` es `DateTime` y `Estado` es `bool`; `NombreUsuario` = nombre + apellido; no hay datos internos del usuario; **compra con `IdProveedor`/`IdUsuario` inexistentes ⇒ «(Sin proveedor)» y «(Sin usuario)», sin excepción** (§3.4). | RF-49, RF-50, RF-51, RF-53(a), RNF-5, RNF-12 |
| **G13 · Compra con detalle** | T7 | Con líneas ⇒ cabecera + líneas con código, nombre, cantidad, precio de compra y subtotal; **sin** `PrecioVenta`; compra sin líneas ⇒ lista vacía; id inexistente ⇒ `CompraNoEncontrada`; **la misma compra con navegaciones nulas ⇒ textos defensivos, sin excepción**; no modifica datos (comparación antes/después). | RF-53(b), RF-54, D-17 |
| **G14 · Permisos** | T8 | `PuedeOperarCompras`: administrador y encargado `true`; vendedor, `null`, `""` y desconocido `false` (en `PoliticaAccesoTests`). Registro, historial y detalle con vendedor ⇒ `PermisoDenegado`, 0 filas y sin datos, **sin ejecutar ninguna consulta**. | RF-8, RF-59, RF-60, RF-61 |

### 9.1 Dobles de prueba: fallo de persistencia, consulta que no debe ocurrir y límites de InMemory

**`FalloDbContext` (G10, T6).** Se declara como clase privada anidada **al final de `CapaNegocio.Tests/CompraNegocioTests.cs`**, con el mismo patrón de `ProveedorNegocioTests`:

```csharp
private sealed class FalloDbContext : AppDbContext
{
    public FalloDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => throw new DbUpdateException("Fallo simulado del origen de datos.");
    public override int SaveChanges()
        => throw new DbUpdateException("Fallo simulado del origen de datos.");
}
```

Sembrado y ejecución, paso a paso, para que la prueba sea ejecutable:

1. Un nombre de base compartido: `var nombreBase = $"CompraNegocioTests-Fallo-{Guid.NewGuid()}";`.
2. Con un `AppDbContext` **normal** sobre ese nombre (`UseInMemoryDatabase(nombreBase)`) se siembran rol, usuario **activo**, proveedor **activo** y dos productos **activos**, con `EnsureCreatedAsync()`; se guardan los identificadores.
3. Se construye el `FalloDbContext` con **las mismas** `DbContextOptions` (mismo nombre de base: el store InMemory se comparte por nombre) y se instancia `new CompraNegocio(falloDb)`.
4. Se invoca `RegistrarCompraAsync` con una compra **válida** de dos líneas: la validación previa lee el store con normalidad y el fallo ocurre en el **primer** `SaveChangesAsync`, antes de escribir cualquier fila.
5. Se afirma: `Resultado == ErrorPersistencia`, `Mensaje == MensajeErrorPersistencia` (§5), **no** se lanza excepción, `IdCompra == null` y la solicitud con sus líneas sigue disponible para reintentar (RNF-11).
6. Con el contexto **sano** del paso 2 se afirma `Compras.Count() == 0` y `DetallesCompra.Count() == 0`.

**Texto vacío sin consultar (G7, T4).** Mecanismo ejecutable: se construye el contexto, se siembran datos, se **dispone** (`await db.DisposeAsync()`) y se invoca la búsqueda con `""` y con `"   "`. Con el contexto dispuesto, cualquier ejecución de consulta lanza `ObjectDisposedException` —derivada de `InvalidOperationException`, que `EsFalloDeOrigenDeDatos` captura— y la respuesta sería `ErrorPersistencia`. Por lo tanto, obtener `SinResultados` en proveedores, o lista vacía con `Mensaje` vacío en productos, **demuestra** que no se consultó el origen de datos. Es el mismo recurso que ya usa G11. *Alternativa descartada*: un contador dentro de un `AppDbContext` doble, porque las propiedades `DbSet` no son virtuales y habría que duplicar el modelo.

**Límites de InMemory.** El proveedor **InMemory** no soporta transacciones reales: `BeginTransactionAsync` devuelve una transacción sin efecto (EF registra `TransactionIgnoredWarning`). Por eso:

- las pruebas **no** configuran `ConfigureWarnings(w => w.Throw(InMemoryEventId.TransactionIgnoredWarning))` y no se interpreta que «no explote» como prueba de rollback;
- la atomicidad se prueba con el doble que **lanza en el primer `SaveChangesAsync`** (no queda ninguna fila escrita en el store) y se declara que **el rollback real se verifica de forma manual contra SQL Server** (§9.2 y criterio «Atomicidad comprobada» de la spec);
- InMemory **no valida claves foráneas**: por eso la prueba de navegaciones nulas (G12 y G13) es posible.

### 9.2 Qué **no** cubre `dotnet test` (verificación manual)

**Elementos fuera de `dotnet test` por no ser reglas de `CapaNegocio`**: `RNF-9` (200 líneas y < 200 ms), doble envío (RF-58), rótulos y retiro de la maqueta (RF-1, RF-2, RF-3), confirmación de cancelar/limpiar (RF-31), **descarte del proveedor y de las líneas después de registrar (RF-32)**, **rechazo visual de la acumulación que supera 1.000.000 (RF-63, «La cantidad acumulada supera el máximo permitido.»)**, **prellenado visual (RF-38 y la parte de prellenado de RF-43)**, coherencia visual (RNF-7) y formato de presentación `dd/MM/yyyy HH:mm` + «Activa»/«Inactiva» (RNF-12). La prueba del historial verifica orden y contenido, **no** el texto formateado. Todo lo anterior se verifica en **T9b** y se repite en **T10**.

**Regla de negocio vs. pantalla (I9)**: `RF-32` es **estado de pantalla**, no una regla de `CapaNegocio`. La lógica solo devuelve `Exito` con los datos de la compra registrada (RF-44, cubierto por G8 en T5b); el descarte del proveedor, de las líneas y del área de captura lo ejecuta `ComprasForm` y **no se prueba con `dotnet test`**: se verifica **manualmente en T9b** y se repite en la verificación final (T10). Ningún criterio de T3 o de T5b depende de ese descarte, y no se le atribuye a G5 ni a G8.

---

## 10. Diseño de UI (`ComprasForm`)

### 10.1 Construcción e inyección

- `public ComprasForm(CompraNegocio negocio)` con `ArgumentNullException.ThrowIfNull` (patrón `ProveedoresForm`). `Program.cs`: `AddTransient<CompraNegocio>()` y `AddTransient<ComprasForm>()`.
- `inicio`: parámetro nuevo `ComprasForm comprasForm` → campo `_comprasForm` → `AbrirVistaEnTab(_comprasForm, tab_compras)` (reemplaza `new ComprasForm()`) → `tab_compras.Enter += tab_compras_Enter` (recarga el historial) → en `AplicarPermisosPorRol()`: `_comprasForm.AplicarSesion(_usuarioActual?.IdUsuario ?? 0, rol)`.
- `public void AplicarSesion(int idUsuario, string? rol)`: guarda `_idUsuario` y `_rolActual`, y habilita la pantalla solo si `PoliticaAcceso.PuedeOperarCompras(rol)`; con rol no habilitado no hay acciones disponibles (la pestaña ya está oculta por `OcultarSiNoAutorizada`). Las operaciones de línea, el registro y el historial están disponibles **sin recortes por rol** para Administrador y Encargado (RF-8).
- **La pantalla nunca** usa `AppDbContext`, SQL ni fórmulas: solo `CompraNegocio`, `PoliticaAcceso`, `MaterialSkin` y `TemaAplicacion.FondoPrincipal`.

### 10.2 Controles (todos MaterialSkin 2, dark)

| Zona | Control | Contenido |
|---|---|---|
| Encabezado | `MaterialLabel _lblTitulo` | «Registro de Compras a Proveedores» (RF-2). Sin título de recepción de stock, sin «Nueva Orden de Compra». |
| Proveedor | `MaterialTextBox2 _txtBuscarProveedor` + `MaterialComboBox _cboResultadosProveedor` + `MaterialLabel _lblProveedorElegido` | Búsqueda por documento o razón social (vacío ⇒ **no se invoca** el negocio, RF-10); el combo lista `RazonSocial — Documento` (máx. 20); al elegir se muestra «Proveedor: {RazonSocial} — {Documento}» y **se conservan** las líneas (RF-12, RF-13). |
| Captura | `MaterialTextBox2 _txtBuscarProducto`, `MaterialComboBox _cboResultadosProducto` | Búsqueda por código o nombre; solo activos; lista `Codigo — Nombre`. Al elegir se llama a `ObtenerPreciosProductoAsync` y se **prellenan** `_txtPrecioCompra` y `_txtPrecioVenta`, ambos editables (RF-38). |
| Captura | `MaterialTextBox2 _txtCantidad`, `_txtPrecioCompra`, `_txtPrecioVenta` + `MaterialLabel _lblSubtotalPreparacion` | Texto crudo (sin normalizar, RNF-12). El subtotal del área se muestra como «—» o como importe con dos decimales, provisto por `CalcularSubtotalEnPreparacion` (RF-65). |
| Captura | `MaterialButton _btnAgregarLinea` | Texto «Agregar línea»; al editar cambia a «Guardar línea». |
| Grilla de líneas | `DataGridView _gridLineas` (ReadOnly, sin `AutoGenerateColumns`) | Columnas: Producto, Cantidad, Precio compra, Precio venta, Subtotal + `DataGridViewButtonColumn` «Editar» y «Quitar». `CellFormatting` con `N2` para importes. |
| Totales | `MaterialLabel _lblMontoTotal` | «Total: {MontoTotal}» calculado con la única firma `CalcularMontoTotal(_lineas.Select(l => l.Subtotal))`, es decir, solo sobre las líneas de la grilla (RF-7/RF-65). No editable. |
| Acciones | `MaterialButton _btnRegistrar` («Registrar compra») y `MaterialButton _btnLimpiar` («Cancelar compra») | Registrar invoca `RegistrarCompraAsync`; se **deshabilita** mientras dura la operación (RF-58). Limpiar pide confirmación con el texto del negocio y, si se acepta, limpia proveedor y líneas (RF-31). |
| Mensajes | `MaterialLabel _lblMensaje` | Muestra **tal cual** el `Mensaje` del negocio (RF-55). |
| Historial | `MaterialLabel _lblTituloHistorial` + `DataGridView _gridHistorial` + `MaterialLabel _lblMensajeHistorial` | Columnas: N° Compra, Fecha y hora, Proveedor, Usuario, Total, Estado. `CellFormatting`: `dd/MM/yyyy HH:mm` y «Activa»/«Inactiva» (RNF-12). Vacío ⇒ mensaje del negocio (RF-51). |

Disposición: `Panel` principal con `Dock = Fill`, `Padding(15)`, encabezado arriba, bloque de proveedor, `TableLayoutPanel` de captura en una fila, grilla de líneas y total, barra de acciones y, al pie, el historial. Se retira el campo «N° Factura / Comprobante» y las filas de ejemplo (RF-3, RF-2).

### 10.3 Eventos delgados (sin reglas)

| Evento | Acción |
|---|---|
| `_txtBuscarProveedor.TextChanged` / `_txtBuscarProducto.TextChanged` | Reinicia un `System.Windows.Forms.Timer` de ~350 ms (patrón `ProveedoresForm`); al vencer, si el texto está vacío **no** llama al negocio; si no, llama a la búsqueda y bindea el combo. |
| `_cboResultadosProveedor.SelectedIndexChanged` | Fija el proveedor elegido en campos locales de la pantalla y actualiza el rótulo. **No** limpia las líneas. |
| `_cboResultadosProducto.SelectedIndexChanged` | Guarda `_idProductoSeleccionado`, llama a `ObtenerPreciosProductoAsync` y escribe los precios propuestos en los textbox (editables). |
| `TextChanged` de cantidad/precios | Recalcula **solo** con `CalcularSubtotalEnPreparacion` y pinta «—» o el importe. |
| `_btnAgregarLinea.Click` | Arma `SolicitudAgregarLineaCompraDto` o `SolicitudEditarLineaCompraDto` según `_indiceEdicion`, llama al negocio, reemplaza `_lineas` por la respuesta, rebindea la grilla y muestra el mensaje. En éxito limpia el área de captura y sale de edición. |
| `_gridLineas.CellContentClick` (Editar/Quitar) | Editar: recarga el área de captura desde la línea y activa modo edición. Quitar: `QuitarLinea(_lineas, indice)`; si `Exito` es `false`, muestra el `Mensaje` recibido y **no** toca la grilla; si es `true`, rebindea con las líneas devueltas. |
| `_btnRegistrar.Click` | `_btnRegistrar.Enabled = false`; arma `SolicitudRegistrarCompraDto` con `_lineas` e invoca `RegistrarCompraAsync(_solicitud, _idUsuario, _rolActual)`. En éxito **descarta proveedor, líneas y captura** (`LimpiarTrasRegistro`, RF-32, verificación **manual**) y recarga el historial (RF-52). En rechazo: **conserva todo** y, si había una línea en edición, la mantiene (RNF-11). `finally`: vuelve a habilitar. |
| `_btnLimpiar.Click` | `MaterialMessageBox` con el texto de `ObtenerMensajeConfirmacionCancelarCompra()`; si confirma, limpia proveedor, líneas, captura y edición (RF-31). |
| `CellFormatting` de `_gridHistorial` | Formatea fecha y estado (presentación pura, RNF-12). |

---

## 11. Riesgos y mitigaciones

| Riesgo | Impacto | Mitigación |
|---|---|---|
| **Transacciones en InMemory** | El proveedor en memoria no ejecuta rollback real; una prueba de atomicidad mal diseñada daría falsa confianza | G10 usa el doble que falla en el **primer** `SaveChangesAsync` (0 filas) y se declara que el rollback real se verifica manualmente contra SQL Server (§9.1, criterio de la spec). |
| **Trigger `TR_PRODUCTO_FECHA_MODIFICACION` en `PRODUCTO`** | Cualquier `UPDATE` accidental al producto dispara el trigger y altera `FechaModificacion` (rompe RF-47) | El registro solo asigna `IdProducto` en el detalle; nunca adjunta ni modifica entidades `Producto`. G8 compara stock, precios, `IdUsuarioModifica`, `FechaModificacion` y `MotivoAjusteStock` antes/después. La configuración `HasTrigger` existente **no se toca** (spec 003). |
| **Historial sin paginación ni índice sobre `fecha_registro`** | Costo creciente (RNF-3 lo declara y no promete costo constante) | El orden lo resuelve la base (`OrderByDescending(FechaRegistro).ThenByDescending(IdCompra)`) y la pantalla no ordena; no se agrega índice (RNF-10). Verificación manual con volumen real. |
| **Doble envío (RF-58)** | Dos compras por un doble clic | `_btnRegistrar.Enabled = false` durante toda la operación + bandera `_registrando` que ignora reentradas. Verificación **manual** (la regla vive en la pantalla, RNF-6). |
| **Tope del total (RF-64) inalcanzable con 1 línea** | Una prueba “de tope” con una sola línea no puede superar `decimal(18,2)` | G9 construye **11 líneas** de 1.000.000 × 999.999.999,99 (11 productos distintos), que supera 9.999.999.999.999.999,99 con el máximo legal de D-15. |
| **Escala decimal no revalidada al confirmar (RF-27)** | Un DTO armado fuera de la pantalla podría llevar `10,333` y el motor lo redondearía al guardar (contra RNF-1) | `RegistrarCompraAsync` revalida con `ValidarPrecioRecibido` la escala de cada precio (≤ 2 decimales, RF-26) además de los rangos y el tope (§6.4); G9 lo prueba con un precio de tres decimales enviado directo al negocio y 0 filas persistidas. |
| **Índice de línea fuera de rango** | Una futura pantalla podría pedir quitar o editar una línea inexistente y provocar una excepción | `QuitarLinea` y `EditarLineaAsync` validan el índice y devuelven `Exito = false` / `LineaNoEncontrada` con la lista intacta; G1b y G5 lo prueban. |
| **Desborde de `decimal` al parsear** | `decimal.Parse` puede lanzar `OverflowException` con textos larguísimos | `AnalizadorImporte` captura el desborde y lo clasifica como `SuperaMaximo` (RF-24), sin propagar excepción. |
| **`SaveChangesAsync` parcial en SQL Server** | Podría quedar cabecera sin líneas si el commit falla | Un único `SaveChangesAsync` + `BeginTransactionAsync`/`CommitAsync` + `RollbackAsync` en el `catch`; verificación manual documentada. |
| **Navegaciones nulas al proyectar historial/detalle** | `NullReferenceException` si un proveedor o usuario no existe (datos inconsistentes) | Proyección defensiva con textos fijos `«(Sin proveedor)»` / `«(Sin usuario)»` (§3.4), sin excepción y sin datos extra; se cubre con G12 y G13 sembrando una compra con `IdProveedor`/`IdUsuario` inexistentes (InMemory no valida claves foráneas). |
| **Regresión del catálogo** | El módulo podría “aprovechar” para actualizar stock | Hook de HU-10 vacío, sin ninguna escritura sobre `Producto` (RF-47/RF-48) y prueba de no regresión en G8 + verificación manual. |
| **Pantalla instanciada con `new`** | `AplicarSesion` nunca llegaría y el registro fallaría por usuario 0 | `inicio` recibe `ComprasForm` por DI y la construye una sola vez, igual que `ProveedoresForm`; `_comprasForm.AplicarSesion` se llama desde `AplicarPermisosPorRol()`. |
| **Ruido de mensajes de búsqueda vacía** | El combo podría mostrar «sin resultados» al borrar el texto | Si el texto recortado está vacío, la pantalla **no** invoca el negocio ni limpia mensajes con error (RF-10). |

---

## 12. Decisiones técnicas (con alternativa descartada)

| # | Decisión | Alternativa descartada | Motivo |
|---|---|---|---|
| P-1 | **Negocio sin estado**: la grilla en carga vive en la pantalla y viaja en cada solicitud (`LineasActuales`) | Guardar la compra en carga dentro de `CompraNegocio` | El negocio se registra `Transient`; mantener estado en él obliga a un ciclo de vida distinto, complica las pruebas y no aporta nada. |
| P-2 | **Una clase de cálculo pura** (`CalculoImportesCompra`) usada por negocio y pantalla | Fórmula en el negocio y otra en los eventos de la UI | RNF-13 prohíbe duplicar la fórmula. |
| P-3 | **Analizadores puros** de cantidad e importe con motivo tipado | `decimal.TryParse` con `CultureInfo` y mensajes en la UI | El parseo debe aceptar coma **o** punto, un solo separador y distinguir 5 causas (RF-25/RF-26/RF-28); `TryParse` no lo hace y la UI no puede redactar mensajes. |
| P-4 | **Un `SaveChangesAsync`** dentro de una transacción explícita | Dos `SaveChangesAsync` | Menos viajes, sin ventana de cabecera sola y mismo rollback (§7). |
| P-5 | **Hook privado no-op** para HU-10 dentro de la transacción | Método público posterior o transacción propia de HU-10 | RF-46 exige extender **la misma** operación sin cambiar la forma de registrar. |
| P-6 | **Proyecciones propias de Compras** (`ProveedorSeleccionadoDto`, `ProductoBuscadoDto`, `PreciosProductoDto`) | Reutilizar `ProveedorRespuestaDto` / `ProductoRespuestaDto` | RNF-5 prohíbe exponer correo, teléfono, stock o datos internos que Compras no necesita. |
| P-7 | **Tope del total validado en negocio antes de persistir** | Confiar en el error de la base | D-20: el rechazo debe llegar con mensaje comprensible y sin intentar el guardado. |
| P-8 | **Revalidar al confirmar los rangos, la escala decimal y el tope** | Reenviar los textos crudos en el registro, o confiar en que la escala ya se validó al agregar la línea | RF-27 exige revalidar la compra completa: el DTO transporta `decimal` y un valor de tres decimales es representable, así que `ValidarPrecioRecibido` lo rechaza con el mensaje de RF-26 antes de abrir la transacción (§6.4). Duplicar el estado de texto en la pantalla no agrega cobertura. |
| P-9 | **Rol por parámetro** (`PoliticaAcceso.PuedeOperarCompras`) | Releer el rol por join con `ROL` como en `ProductoNegocio` | D-14/RF-59: el rol es el de la sesión entregado por la pantalla; releerlo contradiría la spec. |
| P-10 | **Un solo `CompraNegocio`** para registro y consultas | Separar `CompraRegistroNegocio` y `CompraConsultaNegocio` | RNF-7 (una sola pantalla) y RF-55 (un solo lugar para los mensajes); el archivo se mantiene legible por secciones. |
| P-11 | **Proyección defensiva con texto fijo** para navegaciones nulas: `«(Sin proveedor)»` y `«(Sin usuario)»` | Devolver `string.Empty` o dejar que la proyección falle | Un texto fijo hace visible la inconsistencia sin romper la grilla ni el detalle, es el mismo en historial y detalle y no expone datos nuevos (RNF-5); `string.Empty` deja la fila muda y una excepción tumbaría la carga (M11). |
| P-12 | **Índice de línea validado en el negocio**: `LineaNoEncontrada` / `Exito = false` con la lista intacta | Delegar el control del índice solo a la grilla | La lógica es un contrato público y no debe lanzar por un índice inválido; la comprobación es de dos líneas y se prueba (M13). |
| P-13 | **Un único texto de persistencia** para registro, historial y detalle (`MensajeErrorPersistencia`) | Un texto distinto por operación, o reutilizar los textos de búsqueda | Mismo criterio que `ProveedorNegocio` y `ClienteNegocio`, sin duplicar constantes; las búsquedas y el prellenado conservan texto propio porque RF-57 exige identificar la lectura que falló (M12). |

---

## 13. Trazabilidad RF → archivo / método / tarea

| RF | Capa / archivo / método | Tarea | Prueba |
|---|---|---|---|
| RF-1, RF-2, RF-3 | Presentación: `ComprasForm.ArmarInterfazMaterial` (rótulos y retiro de la maqueta) | T9a | Manual/visual (T9b y T10) |
| RF-4 | `CompraNegocio.RegistrarCompraAsync` (`FechaRegistro = DateTime.Now`, `Estado = true`) | T5b | G8 |
| RF-5, RF-6 | `CompraNegocio.ValidarUsuarioAsync` + asignación en la cabecera | T5a, T5b | G9, G8 |
| RF-7 | `CalculoImportesCompra.CalcularMontoTotal` + `_lblMontoTotal` | T2, T5b, T9a | G1a |
| RF-8 | `PoliticaAcceso.PuedeOperarCompras` + `ComprasForm.AplicarSesion` | T8, T9a | G14 + manual (T9b) |
| RF-9, RF-10, RF-11 | `CompraNegocio.BuscarProveedoresAsync` | T4 | G7 |
| RF-12, RF-13 | `ComprasForm` (`_lblProveedorElegido`, selección sin limpiar líneas) | T9a | Manual/visual (T9b) |
| RF-14 | `CompraNegocio.ValidarProveedorAsync` | T5a, T5b | G9 |
| RF-15, RF-16, RF-17 | `CompraNegocio.BuscarProductosAsync` | T4 | G7 |
| RF-18 | `CompraNegocio.ValidarProductosAsync` + `AgregarLineaAsync`/`EditarLineaAsync` + revalidación en `RegistrarCompraAsync` | T3, T5b | G4, G9 |
| RF-19 | `RegistrarCompraAsync` (paso 5) | T5b | G9 |
| RF-20, RF-21 | `AnalizadorCantidad` + `ValidarCantidad` | T2, T3, T5b | G3, G4, G9 |
| RF-22, RF-23 | `AnalizadorImporte` (`Vacio`, `NoPositivo`) + `ValidarPrecioDeTexto`/`ValidarPrecioRecibido` | T2, T3, T5a, T5b | G2a, G2b, G9 |
| RF-24 | `AnalizadorImporte` (`SuperaMaximo`) + `ValidarPrecioRecibido` + mensajes propios | T2, T3, T5a, T5b | G2a, G2b, G9 |
| RF-25 | `AnalizadorImporte` (`FormatoInvalido`, con un único `-` inicial admitido) + textos exactos | T2, T3 | G2a, G2b |
| RF-26 | `AnalizadorImporte` (`DecimalesExcedidos`) + `ValidarPrecioRecibido` | T2, T3, T5a, T5b | G2a, G2b, G9 |
| RF-27 | `AgregarLineaAsync`/`EditarLineaAsync` (orden) + `RegistrarCompraAsync` (revalidación de §6.4) | T3, T5b | G4, G8, G9 |
| RF-28 | Orden determinista en `CompraNegocio` (§6.1 y §6.3) | T3, T5b | G2b, G4, G9 |
| RF-29 | `CompraNegocio.QuitarLinea` | T3 | G1b, G5 |
| RF-30 | `CompraNegocio.EditarLineaAsync` | T3 | G5 |
| RF-31 | `ObtenerMensajeConfirmacionCancelarCompra` + `_btnLimpiar` | T3, T9a | Manual/visual (T9b) |
| RF-32 | `ComprasForm.btnRegistrar_Click` (limpieza tras éxito); **estado de pantalla** | T9a | Manual/visual (T9b y T10; no automatizable, §9.2) |
| RF-33, RF-34 | `CalculoImportesCompra.CalcularSubtotal` / `CalcularMontoTotal` / `CalcularSubtotalEnPreparacion` | T2, T3 | G1a, G1b |
| RF-35 | `CalcularSubtotal` (no usa `PrecioVenta`) | T2 | G1a |
| RF-36 | `AgregarLineaAsync` (rama de acumulación) | T3 | G5 |
| RF-37 | `ExisteProductoDuplicado` en agregar, editar y registro | T3, T5b | G5, G9 |
| RF-38, RF-39, RF-40 | `ObtenerPreciosProductoAsync` + prellenado en `ComprasForm` | T4, T9a | G6 + manual (T9b) |
| RF-41 | `ObtenerPreciosProductoAsync` (solo lectura) | T4 | G6 |
| RF-42, RF-43 | `AgregarLineaAsync` (toma los valores efectivos del DTO); el prellenado de RF-43 es de pantalla | T3 (acumulación), T9a/T9b 🔍 (prellenado) | G5 + manual |
| RF-44 | `RegistrarCompraAsync` + transacción (commit) | T5b, T6 | G8, G10 |
| RF-45 | `catch` + `RollbackAsync` | T6 | G10 + manual (SQL Server, T10) |
| RF-46 | `AplicarEfectosSecundariosAsync` (hook que devuelve `Task.CompletedTask`) | T5b | G8 (no escribe catálogo) |
| RF-47, RF-48 | Ausencia de escrituras sobre `Producto` y otras tablas | T5b | G6, G8 + manual |
| RF-49, RF-50, RF-51 | `CompraNegocio.ObtenerHistorialAsync` | T7 | G12 |
| RF-52 | `ComprasForm` recarga el historial tras el registro | T7 (datos), T9a | Manual/visual (T9b) |
| RF-53, RF-54 | `ObtenerHistorialAsync` + `ObtenerCompraConDetalleAsync` | T7 | G12, G13 |
| RF-55 | Mensajes y validaciones concentrados en `CompraNegocio` | T2 … T9a | Todos los grupos (igualdad de textos) |
| RF-56 | `catch` de `RegistrarCompraAsync` | T6 | G10 |
| RF-57 | `catch` de búsquedas, prellenado, historial y detalle | T4, T7 | G11 |
| RF-58 | `_btnRegistrar.Enabled` + bandera `_registrando` | T9a | Manual/visual (T9b) |
| RF-59, RF-60 | `PuedeOperarCompras` + `RegistrarCompraAsync` (paso 1) | T8 | G14 |
| RF-61 | `PuedeOperarCompras` + `ObtenerHistorialAsync` / `ObtenerCompraConDetalleAsync` | T8 | G14 |
| RF-62 | `EditarLineaAsync` (rechazo sin cambios) | T3 | G5 |
| RF-63 | `AgregarLineaAsync` (tope de acumulación) | T3 | G5 |
| RF-64 | `RegistrarCompraAsync` (paso 6) | T5b | G9 |
| RF-65 | `CalcularSubtotalEnPreparacion` + `_lblSubtotalPreparacion` | T3, T9a | G1b + manual (T9b) |
| RNF-1 | `decimal` en todo el flujo; `AnalizadorImporte` sin redondeos | T2, T3, T5a | G1a, G2a, G2b |
| RNF-2 | `async/await` **solo** en los métodos con acceso a datos; los demás síncronos (§4) | T3, T4, T5b, T7 | G4, G7, G8, G11 |
| RNF-3 | `AsNoTracking` + proyección + `Take(20)` + `Contains` de ids | T3, T4, T5b, T7 | G4, G7, G12 |
| RNF-4 | Sin `Remove`/`DELETE`; quitar línea en memoria | T3, T7 | G1b, G5, G13 |
| RNF-5 | DTOs propios sin correo, teléfono, stock ni entidades | T1, T4, T7 | G7, G12 |
| RNF-6 | Alcance de `dotnet test` limitado a `CapaNegocio`; lo excluido se lista en §9.2 | T2 … T10 | Suite completa + §9.2 |
| RNF-7 | `ComprasForm` con MaterialSkin, sin pantallas ni menú nuevos | T9a | Manual/visual (T9b) |
| RNF-8 | Catálogo de mensajes (§5) distinto por caso | T2 … T8 | Igualdad de textos en todos los grupos |
| RNF-9 | 200 líneas < 200 ms | T10 | Manual |
| RNF-10 | Sin migraciones ni índices | T10 | Revisión de `git status` de `CapaDatos/Migrations` |
| RNF-11 | La solicitud y la pantalla conservan las líneas intactas ante rechazo | T3, T5b, T6 | G1b, G4, G5, G9, G10 |
| RNF-12 | Datos de lectura `DateTime`/`bool`; formato solo en la UI | T1, T7, T9a | G12 + manual (T9b) |
| RNF-13 | Fórmula única en `CalculoImportesCompra` | T2, T3, T9a | G1a + inspección |

---

## 14. Notas para el coordinador

- No hay migraciones, paquetes NuGet nuevos ni cambios de esquema: la única aprobación necesaria es la de este plan y la de `tasks.md`.
- Esta versión incorpora las correcciones de la **segunda revisión QA** de `plan.md` y `tasks.md`: los **vectores de prueba** usan un único separador decimal y cantidades sin puntos de miles (`999999999,99` y `1000000` como máximos válidos; `1.000.000.000,00` y `1.000.001` como errores de formato y de cantidad), RF-37 y RF-24 se prueban al confirmar, T5b implementa los **pasos 2–13** con los roles de prueba alineados y las tareas se dividen con sufijos **a/b** sin renumerar.
- El orden de las tareas respeta `CapaEntidad` (sin cambios) → `CapaDatos` (sin cambios) → `CapaNegocio` (DTOs → cálculo → operaciones de línea → búsquedas → validadores → registro → atomicidad → consultas → permisos) → `CapaPresentacion` (pantalla → verificación visual) → verificación, con **12 tareas** (T1–T4, T5a/T5b, T6, T7, T8, T9a/T9b y T10, que incluye el cierre de `MEMORY.md`).
- No se escribe código C# en esta fase.
