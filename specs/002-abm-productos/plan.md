# Plan técnico — Spec 002 · ABM de Productos

> Fase: **Plan** sobre la spec **aprobada por el usuario (2026-10-23)** (`spec.md`, 20 RF), cerrada con las respuestas del usuario a las 8 dudas abiertas (registradas como **D-9 … D-16**, que se suman a **D-1 … D-8**), con **D-17** (quién audita, RF-20) y con las correcciones de la **revisión QA** (**D-18, D-18-bis, D-19, D-20 y D-21**, 2026-10-22). En la **corrección final** (2026-10-22) se incorporan **D-22** (la semilla de desarrollo entra en el alcance: `id_usuario` del Administrador en `scripts/seed-desarrollo.sql`) y **D-23** (la fecha de modificación la impone un **trigger `AFTER UPDATE`** y EF Core no la escribe). La spec manda: solo QUÉ/POR QUÉ; este documento define el CÓMO por capas.
> Principios aplicados: `docs/constitution.md` (aislamiento de 4 capas, bajas lógicas, tests como puerta de entrada, español, transacciones explícitas si aplica).
> **Estado del plan: propuesto (a la espera de la aprobación del plan/tareas por parte del usuario; la spec ya está aprobada el 2026-10-23).** No hay decisiones nuevas a preguntar al usuario: las 8 dudas quedaron cerradas el 2026-10-22 (D-9…D-16), D-17 el mismo día y las correcciones QA (D-18…D-21). **La creación/ejecución de la migración `AddProductConstraints` requiere aprobación explícita del usuario** (AGENTS.md).
> **Base de la implementación patrón**: spec `001-abm-categorias` (aprobada e implementada). De ella se reutilizan sin cambios `PoliticaAcceso` (**sin tocar** para el Vendedor, D-18-bis), el patrón `CategoriaNegocio`/DTOs/Enums, el patrón de tests `CategoriaNegocioTests` y los patrones de UI `GestionCategoriasControl`/`CategoriaForm`/`GestionCategoriasModalForm`.

## 0. Verificaciones previas (leído en el repo, NO supuesto)

| Verificación | Resultado |
|---|---|
| `CapaEntidad/Producto.cs` | **Ya existe**: tabla `PRODUCTO` con `id_producto` (PK identity), `id_categoria` (FK), `codigo` (**`string?`** → columna **`nvarchar(max)`**, texto), `nombre` (`string?`), `descripcion` (`string?`), `stock` (`int`), `precio_compra` (`decimal`), `precio_venta` (`decimal`), `estado` (`bool`, default `true`), `fecha_registro` (`DateTime`, default `DateTime.Now` en C#) y navegación `Categoria`. **No** tiene columnas de auditoría (`id_usuario`, `id_usuario_modifica`, `fecha_modificacion`) ni de motivo de ajuste. |
| `AppDbContext` | **Ya expone** `DbSet<Producto> Productos` (línea 27). `OnModelCreating` **no tiene** configuración Fluent para `Producto` → se añade en esta spec (D-9/D-10/D-18). |
| Snapshot / migraciones | En `AppDbContextModelSnapshot` la tabla `PRODUCTO` tiene `codigo`/`nombre`/`descripcion` como **`nvarchar(max)`** (el `codigo` **ya es texto**, cumple D-9/hallazgo 5: preserva ceros a la izquierda) y **no** hay índice único sobre `codigo` (solo `HasIndex("IdCategoria")`); `precio_compra`/`precio_venta` ya son `decimal(18,2)`. La FK `FK_PRODUCTO_CATEGORIA_id_categoria` tiene **`onDelete: ReferentialAction.Cascade`** (migración inicial `20260907055526_InitialMigration`, línea 118) → **contradice "sin DELETE físico"** y se corrige (D-18-bis, ver §3). **Falta** la migración nueva (`AddProductConstraints`, al estilo de `AddCategoryConstraints`/`AddUserConstraints`). |
| Grafo de fechas | La entidad usa default `DateTime.Now` en C# y las migraciones insertan `GETDATE()` en la semilla. **D-16** exige que la auditoría la ponga el **servidor de BD**: la **creación** con `HasDefaultValueSql("GETDATE()")` (funciona en `INSERT`) y la **modificación** con un **trigger `AFTER UPDATE`** sobre `PRODUCTO` (`GETDATE()`) que EF Core **no** escribe (mapea `ValueGeneratedOnAddOrUpdate()`); ver §3 y D-23. |
| `PoliticaAcceso` | **Ya existe** el módulo `"Productos"`: administrador y encargado **sí**, vendedor **no** (líneas 13/17/19-22). **D-18-bis** confirma que el Vendedor **NO** recibe el módulo Productos → **`PoliticaAcceso` NO se toca**. La restricción de escritura por rol se valida en **`CapaNegocio`** (ver decisión D-7 corregida). |
| `PoliticaAccesoTests` | **NO se toca**: `TieneAcceso_Vendedor_DevuelveSoloVentasYClientes` (verde actual) **sigue siendo correcto** porque el Vendedor **no** gana `"Productos"` (D-18-bis). Se agregan tests del **método de permiso de escritura de Negocio**, no de `PoliticaAcceso`. |
| `ProductosForm` | **Ya existe** con: `_cboCategoria` real (`ListarCategoriasActivasAsync`, RF-11 de la 001), `AplicarRolUsuario(string?)` (gating del botón de categorías), `ArmarInterfazMaterial()` que **crea la grilla `dgv` con `DataTable` mockeada `PROD-001…`** (líneas 283-297) y el combo `cboEstado` **mockeado** ("Activos"/"Inactivos", sin "Todos"). **No** conoce `ProductoNegocio`; su ctor hoy es `ProductosForm(CategoriaNegocio)`. |
| `inicio` | `AplicarPermisosPorRol()` (línea 232) resuelve el rol (`_usuarioActual?.RolDescripcion`) y llama `_productosForm.AplicarRolUsuario(rol)` (línea 255). `tab_productos_Enter` (línea 401) ya existe para refrescar el combo de categorías al entrar en la pestaña → es el punto natural para recargar, además, la grilla de productos. **La pestaña `tab_productos` ya está oculta para el Vendedor** (D-18-bis: no se habilita el módulo). |
| `Program.cs` | Registra `AppDbContext`, `UsuarioNegocio`, `CategoriaNegocio`, `GestionUsuariosControl`, `GestionCategoriasControl`, `ProductosForm`, `login`, `inicio` (transients) → **hace falta** registrar `ProductoNegocio`. |
| Patrones de referencia | `CategoriaNegocio` (async + Recorte + unicidad `OrdinalIgnoreCase` + autoexclusión + DTOs/Enums + mensajes en español); `GestionCategoriasControl`/`CategoriaForm`/`GestionCategoriasModalForm` (MaterialSkin 2, `MaterialDialog`, `MaterialMessageBox`); patrón DI de `Program.cs`. |
| Pruebas / build | `CapaNegocio.Tests`: xUnit + `Microsoft.EntityFrameworkCore.InMemory` (patrón `CrearContextoVacioAsync` con base `Guid.NewGuid()` y `EnsureCreatedAsync`). Solución única `SistemaVentaStock.slnx` → `dotnet build` y `dotnet test` en la raíz. Suite actual: **64/64** en verde. |
| Semilla | `scripts/seed-desarrollo.sql` carga 12 productos activos con códigos **numéricos de 13 dígitos** (`7790001000011`…) → compatibles con D-9 (8–13 dígitos). No carga productos inactivos. **Con `id_usuario` `NOT NULL` (D-18)**, los `INSERT`/`UPDATE` de `PRODUCTO` (líneas ~167-261) hoy **omiten** `id_usuario` → la semilla dejaría de ser idempotente y `preparar-bd.ps1` fallaría en bases nuevas. **D-22**: se ajusta la semilla para setear `id_usuario` resolviendo el Administrador (DNI `90000001`, patrón `@idCategoria…`) por variable, manteniéndola idempotente y re-ejecutable; y la migración debe rellenar los 12 productos con ese Administrador **antes** del `NOT NULL` (§3). |

## 1. Responsabilidades por Capa

### CapaEntidad
- **Cambio mínimo y obligatorio por D-16/D-8/D-17/D-18**: agregar a `Producto` los campos de auditoría:
  - `IdUsuario` → **`int`** (columna `id_usuario`, **`NOT NULL`**), usuario que **crea** el producto (D-18/RF-1/RF-20);
  - `IdUsuarioModifica` → `int?` (columna `id_usuario_modifica`), `null` en el alta (D-17);
  - `FechaModificacion` → `DateTime?` (columna `fecha_modificacion`), `null` en el alta (D-16);
  - `MotivoAjusteStock` → `string?` (columna `motivo_ajuste_stock`), `null` salvo en el ajuste manual (D-8/D-14: se audita con los campos de auditoría del producto, sin tabla de movimientos nueva).
- **Sin cambios** en el resto de `Producto`: `Codigo`/`Nombre`/`Descripcion` ya son `string?` (la validez la decide Negocio; **`Codigo` ya es texto** y preserva ceros a la izquierda, hallazgo 5) y `FechaRegistro` se conserva (D-8/D-17). No se añade navegación `Usuario` ni colección alguna (se consulta por Id, patrón 001).
- **Auditoría de último evento (D-21)**: los campos de auditoría se **sobrescriben** en cada operación; **no** hay historial. `FechaRegistro`/`IdUsuario` conservan la creación; `FechaModificacion`/`IdUsuarioModifica`/`MotivoAjusteStock` reflejan el **último** evento.
- **Nota sobre D-16/D-23**: el default de `FechaRegistro` en C# (`DateTime.Now`) queda como *fallback* de los tests en memoria; el valor real en SQL Server lo impone el default de columna `GETDATE()` (§3). La fecha **de modificación** también la impone el servidor, pero mediante el **trigger `AFTER UPDATE`** (D-23), no con `DateTime.Now` ni con la fecha enviada por EF Core: la propiedad se mapea `ValueGeneratedOnAddOrUpdate()` y EF **no** la escribe.

### CapaDatos
- El `DbSet<Producto>` y la tabla `PRODUCTO` **ya existen** (migración inicial); las consultas se emiten desde `CapaNegocio` sobre ese `DbSet` (mismo patrón que `CategoriaNegocio`, sin repositorio aparte).
- **Cambio en `OnModelCreating`** (Fluent API) para `Producto`, imitando el patrón exacto de `Categoria`/`Usuario`:
  - `HasMaxLength(13)` sobre `codigo` (**texto/nvarchar(13)**, D-9/hallazgo 5) + **índice único con filtro**: `.HasIndex(p => p.Codigo).IsUnique().HasFilter("[codigo] IS NOT NULL")` (respaldo de RF-2/RF-3; el check de Negocio sigue siendo quien produce el **mensaje** en español, y distingue activo/inactivo según D-1).
  - `HasMaxLength(100)` sobre `nombre` (respaldo de RF-4).
  - `HasMaxLength(300)` sobre `descripcion` (respaldo de RF-5).
  - `HasMaxLength(300)` sobre `motivo_ajuste_stock` (motivo del ajuste, RF-10).
  - `IdUsuario`: columna **`NOT NULL`** (D-18) sobre la que se crea un **default temporal** o se rellena vía migración (ver §3); no se deja `IsRequired()` sobre `Codigo` (permanece nullable).
  - `IdUsuarioModifica` / `FechaModificacion` / `MotivoAjusteStock`: columnas **nullables** (D-17/D-21).
  - `FechaRegistro`: `.HasDefaultValueSql("GETDATE()")` (D-16, reloj de BD en la creación; funciona en el `INSERT`).
  - `FechaModificacion`: `.ValueGeneratedOnAddOrUpdate()` (D-23) → EF Core **no** la escribe en el `INSERT`/`UPDATE`; el valor lo impone el **trigger `AFTER UPDATE`** del servidor (§3). **No** se usa `HasDefaultValueSql` sobre `FechaModificacion` (era la combinación contradictoria previa que D-23 reemplaza).
  - **FK `IdCategoria` (D-18-bis, alineación con "sin DELETE físico")**: corregir `.OnDelete(DeleteBehavior.Cascade)` → **`DeleteBehavior.Restrict`** (equivale a `NoAction`) en la relación `Producto → Categoria`, ya que la FK actual con `Cascade` **contradice** la Constitución (prohibido el borrado físico). Alternativa descartada: dejar el `CASCADE` documentado como deuda (rompe el principio; se prefiere alinear en esta migración, que ya toca el esquema). La baja de categorías con productos sigue siendo lógica y la valida la spec 001.
- **Migración EF Core** requerida (nombre al estilo de las existentes: **`AddProductConstraints`**); comandos y verificaciones previas en §3. Además de los cambios de esquema, la migración **crea el trigger `AFTER UPDATE`** de `fecha_modificacion` en el `Up()` y lo **dropea en el `Down()`** (vía `migrationBuilder.Sql(...)`).
- Obligación verificada: **ningún `DELETE`** — toda baja es `Estado = false` (RNF-3 / Constitución 3).

### CapaNegocio
- Nuevo **`ProductoNegocio`** (ctor con `AppDbContext`, operaciones `async`, mapeo explícito, mensajes en español), con:
  - `CrearProductoAsync(CrearProductoDto)` → Recorte + formato del Código (numérico 8–13 dígitos, D-9), Nombre 3–100 (D-10), Descripción ≤300 (D-10), categoría existente y **activa** (RF-6/D-5), precios ≥ 0 (RF-7), unicidad del Código case-insensitive con mensaje específico de inactivo (RF-3/D-1), `Estado = true`, `Stock = 0` (RF-1/D-2). Registra **auditoría de creación** con reloj de BD (RF-19/D-16) **y el usuario creador** (`IdUsuario`, RF-20/D-17); `IdUsuarioModifica` queda `null`.
  - `ModificarProductoAsync(ModificarProductoDto)` → mismos criterios **sin incluir el Código** (inmutable, D-11/RF-9); autoexclusión en la unicidad; asigna **el usuario de modificación** (`IdUsuarioModifica`, RF-20/D-17) y **NO** escribe la `FechaModificacion`: la impone el **trigger `AFTER UPDATE`** del servidor (RF-19/D-23), por lo que la propiedad se lee pero no se asigna desde el código; **conserva** `FechaRegistro` (D-8/RF-9).
  - `DarBajaProductoAsync(idProducto)` → `Estado = false` sobre la misma fila, sin `DELETE` (RF-12); respuesta `ProductoYaInactivo` si ya lo estaba (caso límite, sin cambio).
  - `ReactivarProductoAsync(idProducto)` → verifica **primero** que la categoría del producto esté **activa**; si está inactiva, **no reactiva** y devuelve un resultado específico con mensaje en español (D-12/RF-13). Reactiva sobre la misma fila (sin duplicado); `ProductoYaActivo` si ya lo estaba.
  - `AjustarStockAsync(idProducto, nuevoStock, motivo, rol)` → valida **motivo obligatorio** (texto libre, RF-10), que `nuevoStock >= 0` (mínimo 0, D-14) y **permiso Administrador/Encargado** (D-14/RF-11); fija el **valor absoluto**, guarda el motivo y **el usuario** del ajuste (RF-10/RF-19/RF-20, D-17); la **fecha** de modificación la impone el trigger del servidor (D-23).
  - `ListarProductosAsync(FiltroProductosDto)` → **todos** los productos con su estado (RF-14), aplicando filtros por búsqueda de **Código y/o Nombre**, **IdCategoria** y **Estado** (RF-15). Proyección a DTO con el **nombre de la categoría** resuelto por join (no por descripción en el DTO de entrada). `AsNoTracking`.
- **Permisos (D-18-bis/RF-11/RF-18)**: la lógica de "quién puede escribir" se centraliza en `CapaNegocio`. Regla: **escritura del ABM y ajuste de stock ⇒ Administrador o Encargado**; el **Vendedor no accede a la pantalla de Productos** (su consulta del catálogo llega desde Ventas, spec 005). `PoliticaAcceso` **no se modifica**; se agrega un método de negocio (`PuedeGestionarProductos(rol)`) que devuelve verdadero solo para Admin/Encargado y que usan las operaciones de escritura (ver decisión D-7 corregida).
- **DTOs** (`CapaNegocio/DTOs/Productos/`) y **Enums** de resultado (patrón de los de Categoría/Usuario).
- **Auditoría con reloj de servidor (D-16/D-23) y de quién (D-17, RF-20)**: la **fecha de creación** la impone el servidor por default de columna y la **fecha de modificación** por el trigger `AFTER UPDATE`; el código **no** escribe ninguna de las dos (solo el `IdUsuario`/`IdUsuarioModifica` y el `MotivoAjusteStock`). La **identidad** del usuario se toma del DTO de entrada (la UI la aporta desde `UsuarioRespuestaDto`), nunca de un valor fijado en la interfaz ni del reloj del equipo.

### CapaPresentacion
- **`ProductoModalForm`** (nuevo, modal `MaterialForm`, patrón `CategoriaForm`): alta y edición.
  - **Alta**: `txtCodigo` (numérico 8–13 dígitos), `txtNombre`, `txtDescripcion` (opcional), `cboCategoria` (**solo activas**, heredado de la 001), `txtPrecioCompra`, `txtPrecioVenta`.
  - **Edición**: precarga los datos y **el Código se muestra de solo lectura** (`ReadOnly = true`, D-11). El producto **inactivo es editable sin reactivarlo** (D-20) — la edición no cambia el `Estado`.
  - Ante error, muestra el `Mensaje` en español devuelto por Negocio y **no cierra** (patrón `CategoriaForm`).
  - **Advertencia no bloqueante (RF-8/D-19)**: si `PrecioVenta < PrecioCompra`, muestra un `MaterialDialog` de confirmación **ANTES de llamar a Negocio/persistir**; si el usuario confirma, se guarda; si **cancela**, **no se persiste nada** y el modal permanece abierto.
- **`ProductosForm`** (modificado):
  - ctor pasa a `ProductosForm(CategoriaNegocio, ProductoNegocio)`; se conectan los botones ya existentes Nuevo/Editar/Desactivar/Actualizar y se agregan **Reactivar** y **Ajustar stock** (a `panelAcciones`, patrón del botón de categorías).
  - **Grilla real**: se reemplaza el `DataTable` mockeado `PROD-001…` por el bindeo del listado de `ListarProductosAsync` (código, nombre, categoría, P. Costo, P. Venta, stock, estado en "Activo"/"Inactivo").
  - **Filtros reales**: `txtBuscar` (código y/o nombre) + `cboCategoria` (ya real) + `cboEstado` (se corrige: **Todos / Activos / Inactivos** y deja de ser mockeado, RF-15).
  - **Recarga** en `tab_productos_Enter` (patrón ya existente) y tras cada operación del ABM, sin reiniciar (patrón 001).
  - **Diálogos de baja (RF-12/D-13)**: confirmación **simple** (no hay advertencia reforzada por ventas/compras históricas).
  - **Ajuste de stock (RF-10)**: pequeño modal (o input de MaterialSkin) con `nuevo stock` (entero ≥ 0) y `motivo` obligatorio; invoca `AjustarStockAsync`.
  - **Gating por rol (RF-18/D-18-bis)**: se **extiende** `AplicarRolUsuario(string?)` para **Administrador/Encargado** ⇒ ABM completo + ajuste de stock. El **Vendedor no ve la pestaña "Productos"** (el módulo no se le habilita, D-18-bis), por lo que **no** se agrega lógica de solo lectura para él en esta pantalla; igualmente se mantiene un **guard defensivo en runtime** en cada handler de escritura que rechaza con mensaje en español si el rol no corresponde (defensa en profundidad).
  - **Guard defensivo en runtime** (lección RF-12 de la 001): cada handler de escritura valida el permiso **antes** de abrir el modal/ejecutar, y rechaza con mensaje en español si el rol no corresponde.
- **`inicio`**: **sin cambios** más allá de lo ya existente (`AplicarPermisosPorRol` ya llama `_productosForm.AplicarRolUsuario(rol)`; `tab_productos_Enter` ya recarga; la pestaña `tab_productos` ya se oculta al Vendedor). No se toca el menú (D-R2).
- **`Program.cs`**: agregar `services.AddTransient<ProductoNegocio>();` (el resto ya está).

## 2. DTOs y Mapeos

Todos viven en `CapaNegocio/DTOs/Productos/`. **Nunca** viaja la entidad `Producto` pura a la UI.

| DTO | Sentido | Campos | RF / Uso |
|---|---|---|---|
| `CrearProductoDto` | Presentación → Negocio | `Codigo`, `Nombre`, `Descripcion`, `IdCategoria`, `PrecioCompra`, `PrecioVenta`, `IdUsuario` (**obligatorio**, auditoría) | RF-1, RF-2, RF-4, RF-5, RF-6, RF-7, RF-20 |
| `ModificarProductoDto` | Presentación → Negocio | `IdProducto`, `Nombre`, `Descripcion`, `IdCategoria`, `PrecioCompra`, `PrecioVenta`, `IdUsuario` (**sin** `Codigo`: inmutable) | RF-9 |
| `AjustarStockDto` | Presentación → Negocio | `IdProducto`, `NuevoStock` (`int`), `Motivo`, `Rol` (o `IdUsuario` + `Rol`) | RF-10, RF-11 |
| `FiltroProductosDto` | Presentación → Negocio | `Texto` (busca en Código y/o Nombre), `IdCategoria` (`int?`), `Estado` (`bool?`) | RF-14, RF-15 |
| `ProductoRespuestaDto` | Negocio → Presentación | `IdProducto`, `Codigo`, `Nombre`, `Descripcion`, `IdCategoria`, `NombreCategoria`, `PrecioCompra`, `PrecioVenta`, `Stock`, `Estado`, `FechaRegistro`, `FechaModificacion`, `IdUsuarioModifica` (usuario que modificó) | RF-14, RF-15 (grilla), RF-17, RF-20 |
| `RespuestaCreacionProducto` | Negocio → Presentación | `Resultado`, `Producto?`, `Mensaje`, `AdvertenciaPrecioVentaMenorCompra` (`bool`, RF-8) | RF-1…RF-8 |
| `RespuestaModificacionProducto` | Negocio → Presentación | `Resultado`, `Producto?`, `Mensaje`, `AdvertenciaPrecioVentaMenorCompra` (`bool`) | RF-9 |
| `RespuestaBajaProducto` | Negocio → Presentación | `Resultado`, `Producto?`, `Mensaje` | RF-12 |
| `RespuestaReactivacionProducto` | Negocio → Presentación | `Resultado`, `Producto?`, `Mensaje` (mensaje específico cuando la categoría está inactiva) | RF-13 |
| `RespuestaAjusteStock` | Negocio → Presentación | `Resultado`, `Producto?`, `Mensaje` | RF-10, RF-11 |

**Enums** (`CapaNegocio/Enums/`, patrón existente):
- `ResultadoCreacionProducto { Exito, DatosInvalidos, CodigoDuplicado, CategoriaInvalida }`
- `ResultadoModificacionProducto { Exito, DatosInvalidos, ProductoNoEncontrado, CodigoDuplicado, CategoriaInvalida }`
- `ResultadoBajaProducto { Exito, DatosInvalidos, ProductoNoEncontrado, ProductoYaInactivo }`
- `ResultadoReactivacionProducto { Exito, DatosInvalidos, ProductoNoEncontrado, ProductoYaActivo, CategoriaInactiva }`
- `ResultadoAjusteStock { Exito, DatosInvalidos, ProductoNoEncontrado, MotivoRequerido, StockInvalido, SinPermiso }`

**Nota D-1**: el duplicado **no** se divide en activo/inactivo: la distinción viaja en `Mensaje` (en inactivo, sugiere reactivarlo desde el listado), igual que en `CategoriaNegocio`.

**Nota RF-8 (D-19)**: la advertencia de PrecioVenta < PrecioCompra **no bloquea** → Negocio **permite** guardar y expone la advertencia en un booleano; pero la **advertencia se muestra ANTES de persistir** (la UI compara precios ya validados y pregunta "¿guardar igual?"; si cancela, **no se llama a Negocio** y nada se persiste). Alternativa descartada: devolver un enum "requiere confirmación" y exigir un reintento (ver D-5, §4).

**Mapeos** (explícitos, obligación del repo):
- Negocio → DTO: `MapearProducto(Producto)` estático privado y proyección `Select` con `AsNoTracking()` en el listado (incluye `NombreCategoria` por join a `Categorias`).
- DTO → entidad: solo dentro de `ProductoNegocio` (alta crea `new Producto { … Estado = true, Stock = 0 }`; edición asigna sobre la entidad cargada; **nunca** se cambia `Codigo`, `FechaRegistro` ni `IdProducto`).
- Presentación → Negocio: la UI solo arma los DTOs de entrada y bindea los DTOs de salida a grilla/combos.

## 3. Persistencia

**Situación verificada (leyendo el código)**: la tabla `PRODUCTO` **ya existe** (migración `20260907055526_InitialMigration`, presente en el snapshot) y `AppDbContext` ya expone `DbSet<Producto> Productos`. En el snapshot, `codigo`/`nombre`/`descripcion` son **`nvarchar(max)`** (`codigo` **ya es texto**, preserva ceros a la izquierda) y **no** hay índice único sobre `codigo`. Se añade defensa en BD (D-9/D-10/D-16/D-18/D-18-bis), complementaria al check de Negocio:

- `HasMaxLength(13)` sobre `codigo` (**texto/nvarchar(13)**, D-9/hallazgo 5) + **índice único filtrado** `[codigo] IS NOT NULL` (patrón de `IX_CATEGORIA_descripcion`).
- `HasMaxLength(100)` sobre `nombre`; `HasMaxLength(300)` sobre `descripcion`; `HasMaxLength(300)` sobre `motivo_ajuste_stock`.
- **Auditoría de creación `NOT NULL` (D-18)**: `id_usuario` (`int`, FK a `USUARIO`) **`NOT NULL`**; `id_usuario_modifica` (nullable, FK), `fecha_modificacion` (nullable) y `motivo_ajuste_stock` (nullable, texto).
- **FK `PRODUCTO → CATEGORIA` (D-18-bis)**: la migración **corrige** el `onDelete` de `ReferentialAction.Cascade` a **`ReferentialAction.Restrict`** (`NoAction`) para alinear con la Constitución ("sin DELETE físico"). *(La baja de categorías sigue siendo lógica; spec 001.)*
- **Reloj del servidor (D-16/D-23)**:
  - `fecha_registro`: `.HasDefaultValueSql("GETDATE()")` → en SQL Server la **creación** usa el reloj del motor (funciona en el `INSERT`).
  - `fecha_modificacion`: **columna nullable** que EF **no** escribe (`.ValueGeneratedOnAddOrUpdate()`); el valor lo impone un **trigger `AFTER UPDATE` sobre `PRODUCTO`** que asigna `fecha_modificacion = GETDATE()` en cada actualización de la fila (cubre la edición del ABM y el ajuste de stock, ambos `UPDATE`). El trigger se **crea en el `Up()`** de `AddProductConstraints` y se **dropea en el `Down()`** con `migrationBuilder.Sql(...)`. **No** se usa `HasDefaultValueSql` sobre `fecha_modificacion` (combinación contradictoria reemplazada por D-23). **Límite conocido**: `InMemory` **no** ejecuta el trigger ni `GETDATE()` → los tests verifican el *comportamiento* (que la fecha de modificación quede informada y la de registro se conserve) pero **no** la fuente servidor; esta se valida contra la BD real.
- El índice único es la red de seguridad del caso límite *"dos altas con el mismo Código a la vez"*; el camino normal (y el que produce el **mensaje en español**, también para el duplicado inactivo) es el check previo de Negocio.

**Comandos de migración (⚠️ requieren aprobación previa del usuario — AGENTS.md: "preguntar antes de crear migraciones"; la spec ya fue aprobada, pero la aprobación explícita para crear/ejecutar la migración se pide antes de correr T2):**
1. **Verificación previa en la BD de desarrollo**: que `codigo` no tenga `NULL` duplicados, nulos "lógicos", ni valores de más de 13 caracteres o no numéricos, y que `nombre`/`descripcion` no superen 100/300. La semilla cumple (códigos de 13 dígitos); se verifica igual contra la BD real.
2. **Relleno de la semilla antes del `NOT NULL` (D-18)**: la migración debe **rellenar** los **12 productos de la semilla** (`scripts/seed-desarrollo.sql`, códigos `779…`) en la nueva columna `id_usuario` con el **usuario Administrador** (DNI `90000001`, creado por la semilla) antes de aplicar el `NOT NULL`, p. ej. `UPDATE dbo.PRODUCTO SET id_usuario = (SELECT id_usuario FROM dbo.USUARIO WHERE dni = 90000001) WHERE id_usuario IS NULL` (o `defaultValue` + `Sql(...)` en la migración generada). Si el relleno no encuentra el usuario (arranque desde cero), se usa un usuario de sistema existente; no se deja la columna con `NULL`.
3. `dotnet ef migrations add AddProductConstraints --project CapaDatos --startup-project CapaDatos --context AppDbContext` → genera la migración; **en el `Up()` se agrega además el `migrationBuilder.Sql(...)` que crea el trigger `AFTER UPDATE`** de `fecha_modificacion` (y el `DROP TRIGGER` correspondiente en el `Down()`).
4. `dotnet ef database update --project CapaDatos --startup-project CapaDatos --context AppDbContext` → **sincronizar la BD antes de probar la UI** (aprendizaje de MEMORY: omitirlo genera excepciones de mapeo en EF Core).
5. **Ajuste de datos necesario**: además del relleno de `id_usuario` (paso 2), los campos `id_usuario_modifica`/`fecha_modificacion`/`motivo_ajuste_stock` nacen `NULL`, sin *default* obligatorio; `fecha_modificacion` la impone el trigger, **no** un default de columna.
6. **Ajuste de la semilla (D-22, parte del entregable)**: se modifica `scripts/seed-desarrollo.sql` para que los `INSERT`/`UPDATE` de `PRODUCTO` seteen `id_usuario` resolviendo el Administrador mediante variable —`DECLARE @idUsuarioAdministrador int = (SELECT id_usuario FROM dbo.USUARIO WHERE dni = 90000001)` (patrón de `@idCategoria…`)— y **sigan siendo idempotentes y re-ejecutables** (sin `DELETE`, manteniendo el patrón `UPDATE … IF @@ROWCOUNT = 0 INSERT`). Con esto `preparar-bd.ps1` funciona en bases nuevas con `id_usuario` `NOT NULL`. Si la variable quedara `NULL`, la semilla debe fallar con mensaje claro (o reutilizar el Administrador ya creado) en lugar de insertar productos sin usuario.

## 4. Decisiones técnicas

> Numeración independiente de la de la spec. Las **decisiones de negocio** del usuario (D-1…D-17 de la spec) no se re-abren; aquí solo el CÓMO.

| # | Decisión | Alternativas descartadas | Por qué |
|---|---|---|---|
| D-1 | Crear **`ProductoNegocio` propio** con DTOs/Enums. | (a) Meter el ABM en `CategoriaNegocio`; (b) lógica en eventos de WinForms. | (a) mezcla dominios y viola SRP; (b) prohibido por AGENTS/Constitución ("nunca lógica de negocio en eventos visuales"). |
| D-2 | Índice único **filtrado** sobre `codigo` + `HasMaxLength` (patrón de `Categoria`/`Usuario.Correo`); unicidad case-insensitive en Negocio con `OrdinalIgnoreCase`. | (a) Confiar en la colación sin índice; (b) `EF.Functions.Collate`; (c) `IsRequired()` sobre `codigo`. | (a) no portable y `InMemory` no la aplica; (b) específico SQL Server y no ejecutable en tests; (c) cambiaría el contrato de nulabilidad de la entidad sin necesidad. El check en Negocio sigue produciendo el mensaje en español (D-1 de la spec). |
| D-3 | **Baja en dos pasos** (patrón 001): confirmación **simple** en UI → `DarBajaProductoAsync`. | Advertencia reforzada por ventas/compras históricas. | **D-13** lo decidió: confirmación simple (no hay `DELETE`, el historial queda intacto). |
| D-4 | Listado con **filtros en consulta** (`Where` por texto/idCat/estado) + proyección con **join a Categoría** para el nombre. | (a) Filtrar en memoria en la UI; (b) `Include(p => p.Categoria)` + mapear en memoria. | (a) viola el aislamiento (regla de negocio en la UI); (b) trae entidades y aplica la lógica de filtrado fuera de Negocio; la proyección `Select` mantiene DTOs sanitizados y `AsNoTracking`. |
| D-5 | **Advertencia RF-8 como booleano** en la respuesta (`AdvertenciaPrecioVentaMenorCompra`): Negocio permite guardar y la UI decide cómo avisar. **Corregido por D-19**: el aviso se muestra **ANTES de persistir** (la UI compara precios ya validados y pregunta "¿guardar igual?"); si cancela, **no se llama a Negocio** y **no se persiste nada**. | (a) Avisar **después** de recibir el `Exito` y borrar lo guardado si cancela (contradice D-19 y deja persistencia indebida); (b) enumerar un resultado "requiere confirmación" y exigir que la UI reintente con un flag `Confirmado`. | El requisito (D-19) dice que la advertencia no bloquea **pero** el cancelar no debe persistir: avisar antes evita persistir y revertir; el booleano en la respuesta queda como respaldo informativo. |
| D-6 | **Auditoría con reloj de BD**: `fecha_registro` con `HasDefaultValueSql("GETDATE()")` (creación, funciona en `INSERT`); `fecha_modificacion` la impone un **trigger `AFTER UPDATE`** sobre `PRODUCTO` con `GETDATE()` (D-23), **EF no la escribe** (`.ValueGeneratedOnAddOrUpdate()`), y el trigger se crea/dropea en la migración `AddProductConstraints`. | (a) `DateTime.Now` en C# (lo que hoy hace la entidad); (b) un `SELECT GETDATE()` por operación; (c) `HasDefaultValueSql("GETDATE()")` sobre `fecha_modificacion` + exclusión manual del valor en el `UPDATE` (combinación contradictoria que **D-23 reemplaza**). | (a) viola **D-16** (hora del equipo); (b) agrega un round-trip y una consulta cruda; (c) `HasDefaultValueSql` no se aplica en `UPDATE` y forzar/excluir la columna desde EF es frágil; el **trigger** deja la fuente de verdad en el servidor y vale para toda escritura (edición y ajuste de stock) sin que EF la toque. **Límite**: `InMemory` no ejecuta el trigger → el valor de servidor se verifica contra la BD real (criterio de finalización). |
| D-7 | **Permisos en Negocio, sin tocar `PoliticaAcceso` (corregida por D-18-bis)**: el Vendedor **no** accede a la pantalla de Productos (su consulta llega desde Ventas, spec 005); `PoliticaAcceso` **queda como está** (Administrador/Encargado tienen "Productos"; Vendedor no). Se agrega un método de negocio `PuedeGestionarProductos(rol)` (o `TienePermisoEscrituraProductos(rol)`) que devuelve verdadero **solo** para Administrador/Encargado; las operaciones de escritura de `ProductoNegocio` y el guard de runtime de la UI lo usan. | (a) Ampliar `PoliticaAcceso` para dar "Productos" al Vendedor y separar lectura/escritura (**contradice D-18-bis**: el módulo no se habilita y el test `TieneAcceso_Vendedor_DevuelveSoloVentasYClientes` quedaría roto); (b) evaluar el rol con `string` "hardcodeado" en varios puntos. | (a) rompe la decisión del usuario y desactualiza un test que **debe quedar intacto**; (b) duplicaría la regla. El permiso de escritura vive en `CapaNegocio` (RNF-5, defensa en profundidad y testeable) **más** el guard de runtime en la UI (lección de la 001). |
| D-8 | **`Codigo` inmutable**: `ModificarProductoDto` **no** incluye `Codigo`; la UI lo muestra `ReadOnly`. | (a) Incluir `Codigo` en el DTO y rechazarlo en Negocio si cambia (más código y mensajes nuevos). | **D-11** lo decidió: el Código no se modifica. No incluirlo en el DTO lo hace imposible por construcción (más simple y seguro que validar y rechazar). |
| D-9 | **Reactivación bloqueada con categoría inactiva**: `ReactivarProductoAsync` consulta la categoría del producto **antes** de reactivar; si está inactiva, responde `CategoriaInactiva` con mensaje en español. | Reactivar igual y avisar después (en el listado). | **D-12** lo decidió: el sistema **bloquea y avisa** que primero debe reactivarse la categoría. Bloquear antes de escribir mantiene la integridad del vínculo (ningún producto activo colgando de una categoría inactiva). |
| D-10 | **Ajuste de stock como método de Negocio** con `nuevoStock` absoluto + `motivo` + `rol`; valida motivo no vacío, `nuevoStock >= 0` y permiso Admin/Encargado; guarda `MotivoAjusteStock` + auditoría (usuario/fecha del servidor). **Último evento (D-21)**: sobrescribe el motivo/usuario/fecha anteriores. | (a) Ajuste como diferencia (+/−); (b) validar en la UI. | (a) **D-14** decidió **valor absoluto**; (b) viola RNF-5 (regla de negocio fuera de Negocio). |
| D-11 | **Gating de rol en `ProductosForm`**: `AplicarRolUsuario` aplica el ABM completo a Administrador/Encargado; el **Vendedor no ve la pestaña** (D-18-bis), y además cada handler de escritura lleva **guard defensivo en runtime**. | Rediseñar el menú o crear otra pantalla para el Vendedor. | El menú está **congelado** (D-R2) y la funcionalidad vive dentro de la pantalla existente; el guard de runtime sigue el patrón aprobado (defensa en profundidad) aunque el Vendedor no llegue a la pantalla. |
| D-12 | **FK `PRODUCTO → CATEGORIA` con `onDelete: Cascade` (preexistente) se corrige a `Restrict`/`NoAction`** en la migración `AddProductConstraints`. | (a) Dejar el `CASCADE` documentado como deuda (contradice la Constitución y permitiría borrado físico en cascada); (b) crear una migración separada solo para la FK. | La migración ya toca el esquema de `PRODUCTO`; alinear la FK con "sin DELETE físico" (Constitución 3) en el mismo cambio evita deuda abierta y no añade migraciones innecesarias. La baja de categorías sigue siendo lógica (spec 001). |
| D-13 | **Migración con relleno de semilla (D-18)**: la migración rellena `id_usuario` de los 12 productos de la semilla con el Administrador (DNI `90000001`) antes de aplicar el `NOT NULL` (ver §3, paso 2). | Dejar `id_usuario` nullable (contradice D-18) o crear un default fijo "de sistema" sin usuario real. | D-18 exige `NOT NULL`; el relleno con el Administrador de la semilla mantiene la coherencia con los datos reales del entorno y no inventa un usuario fantasma. |
| D-14 | **Semilla de desarrollo en el alcance (D-22)**: se ajusta `scripts/seed-desarrollo.sql` para que `PRODUCTO` reciba `id_usuario` resolviendo el Administrador (DNI `90000001`) por variable (patrón `@idCategoria…`), conservando la idempotencia (sin `DELETE`, `UPDATE … IF @@ROWCOUNT = 0 INSERT`). | (a) Dejarlo como tarea fuera de la spec (la semilla quedaría rota frente a `NOT NULL` y `preparar-bd.ps1` fallaría en bases nuevas); (b) hardcodear el `id_usuario` numérico sin variable. | El usuario incluyó explícitamente este ajuste **en el alcance** de la spec (D-22); resolver el usuario por DNI evita asumir el identity y mantiene la idempotencia igual que las categorías. |
| D-15 | **Trigger `AFTER UPDATE` para `fecha_modificacion` (D-23)**: el trigger se crea en el `Up()` de `AddProductConstraints` (`CREATE TRIGGER … AFTER UPDATE … SET fecha_modificacion = GETDATE()`) y se dropea en el `Down()` (`DROP TRIGGER`); la propiedad se mapea `.ValueGeneratedOnAddOrUpdate()` para que EF no la escriba. | (a) `HasDefaultValueSql("GETDATE()")` sobre `fecha_modificacion` + exclusión manual en EF (no se aplica en `UPDATE`, frágil); (b) `DateTime.Now` en C# (viola D-16). | D-23 fija el trigger como mecanismo; centraliza el reloj de servidor para **toda** escritura (edición y ajuste de stock) sin depender de lo que EF envíe, y se crea/dropea de forma versionada en la migración. |

## 5. Estrategia de tests

- **Fuente automática única**: `CapaNegocio.Tests` (xUnit + EF Core `InMemory`, patrón de `CategoriaNegocioTests`: `CrearContextoVacioAsync` con base `Guid.NewGuid()` y `EnsureCreatedAsync`). Comando: **`dotnet test`** en la raíz.
- **Ciclo por tarea**: primero los tests nuevos en **rojo**, luego el código en **verde**; ninguna tarea se cierra con pruebas rojas (Constitución 4). Compilación con **`dotnet build`**.
- **Tests de Negocio antes del código** (rojo→verde), en un archivo nuevo `ProductoNegocioTests`:
  - RF-1: alta válida ⇒ `Estado = true`, `Stock = 0`, `Codigo`/`Nombre` recortados, registro persistido (conteo de `DbSet`).
  - RF-2: `[Theory]` de Código vacío / solo espacios / 7 dígitos / 14 dígitos / con letras / con guiones / con espacios internos ⇒ rechazo con mensaje en español; `" 7790001000011 "` ⇒ guardado como `"7790001000011"`.
  - RF-3: duplicado activo y con distinta caja ⇒ `CodigoDuplicado`; duplicado sobre producto **inactivo** ⇒ `CodigoDuplicado` con `Mensaje` que menciona "inactivo"/"reactivar"/"listado" y la existente sigue `Estado = false` (sin reactivación automática, D-1).
  - RF-4: Nombre vacío / "<3" / ">100" tras recorte ⇒ rechazo; `"  Teclado  "` ⇒ `"Teclado"`.
  - RF-5: Descripción vacía ⇒ válida; Descripción de 301 caracteres ⇒ rechazo con mensaje.
  - RF-6: `IdCategoria` inexistente ⇒ `CategoriaInvalida`; `IdCategoria` de categoría **inactiva** ⇒ `CategoriaInvalida` con mensaje en español.
  - RF-7: `PrecioCompra < 0` y `PrecioVenta < 0` ⇒ rechazo; `0` ⇒ válido.
  - RF-8: `PrecioVenta < PrecioCompra` ⇒ `Exito` con `AdvertenciaPrecioVentaMenorCompra = true` (no bloquea); `PrecioVenta == PrecioCompra` ⇒ advertencia `false`.
  - RF-9: edición sin cambiar el nombre propio ⇒ éxito (autoexclusión); el `Codigo` y la `FechaRegistro` **no cambian**; se asigna `IdUsuarioModifica` (la `FechaModificacion` la impone el trigger del servidor, D-23; en memoria se verifica solo el comportamiento). **Editar un producto inactivo** sin reactivarlo ⇒ `Exito` y el `Estado` **sigue `false`** (D-20).
  - RF-10/RF-11: ajuste con motivo válido y rol Administrador/Encargado ⇒ `Exito`, stock = valor absoluto, `MotivoAjusteStock` guardado; motivo vacío/solo espacios ⇒ `MotivoRequerido`; `nuevoStock < 0` ⇒ `StockInvalido`; `nuevoStock == 0` ⇒ válido; rol Vendedor ⇒ `SinPermiso` (validado en Negocio, D-18-bis). Segundo ajuste ⇒ los campos de auditoría reflejan **solo el último evento** (D-21).
  - RF-12: baja ⇒ `Estado == false`, **no** disminuye el conteo de filas (sin `DELETE`); baja sobre inactivo ⇒ `ProductoYaInactivo`.
  - RF-13: reactivar con categoría **activa** ⇒ `Exito` sobre la misma fila (sin duplicado); reactivar con categoría **inactiva** ⇒ `CategoriaInactiva` y el producto **sigue inactivo**; reactivar uno ya activo ⇒ `ProductoYaActivo`.
  - RF-14/RF-15: `ListarProductosAsync` ⇒ devuelve activos **e** inactivos con su estado; filtro por texto (código y/o nombre), por `IdCategoria` y por `Estado` devuelve conjuntos correctos; `NombreCategoria` resuelto. **Precios con decimales** (`0,99`/`1234,56`) se persisten con dos decimales.
  - **RF-18**: se agregan tests del **método de permiso de escritura de Negocio** (`PuedeGestionarProductos`: Admin/Encargado sí; Vendedor no). **`PoliticaAccesoTests` NO se toca** (D-18-bis: el Vendedor no gana "Productos"; `TieneAcceso_Vendedor_DevuelveSoloVentasYClientes` sigue verde sin cambios).
  - **RF-20/D-18**: se verifica que la creación registre `IdUsuario` **obligatorio** (con `IdUsuarioModifica` en `null`), que un alta **sin** usuario se rechace, que la modificación asigne `IdUsuarioModifica` y conserve `FechaRegistro`, y que el ajuste de stock registre el usuario además del motivo; la identidad sale del DTO de entrada.
  - RF-19: se verifica que la creación/modificación informen las fechas de auditoría (comportamiento); la **fuente servidor** (default `GETDATE()` en creación y trigger `AFTER UPDATE` en modificación) se valida contra la BD real (límite de `InMemory`, que no ejecuta trigger).
- **Validación visual (UI WinForms, sin tests automáticos)**: alta; edición con Código no editable (incluido editar un **inactivo** sin reactivarlo, D-20); baja con confirmación simple; reactivación (y bloqueo cuando la categoría está inactiva); listado con filtros por código/nombre, categoría y estado; advertencia de PrecioVenta < PrecioCompra **antes** de guardar (cancelar = no persistir, D-19); ajuste de stock por valor absoluto con motivo y gating; **el Vendedor no ve la pestaña "Productos"** (D-18-bis); grilla y combo de estado sin datos mockeados; arranque sin errores.
- **Testabilidad explícita** (qué se cubre con `dotnet test` y qué no): con `dotnet test` en `CapaNegocio` se cubren **RF-1…RF-15, RF-17, RF-18** (permiso de escritura) **y RF-20** (auditoría de usuario sobre el motor en memoria). Requieren **verificación contra BD real o validación manual**: RF-16 (fallo de BD) y RF-19 (fuente de la fecha = servidor), más los respaldos de BD de RF-2/RF-3 (índice único filtrado `nvarchar(13)`) y de D-18 (el `NOT NULL` real solo se prueba con datos). La parte visual de RF-8/RF-12/RF-13/RF-14/RF-15 y el ocultamiento de la pestaña al Vendedor se validan **manualmente**.
- **Límite conocido**: `InMemory` no simula índices únicos, longitudes, `NOT NULL`/FK ni el trigger/reloj del servidor → unicidad, `NOT NULL` de `id_usuario`, `onDelete` de la FK y auditoría con servidor (fechas de **creación** y de **modificación**) se verifican **contra la BD real** (migración aplicada + altas duplicadas manuales + inspección de `fecha_registro`/`fecha_modificacion`/`id_usuario`, provocando un `UPDATE` para comprobar el trigger).

## 6. Trazabilidad (RF → componente/capa → verificación)

| RF | Componente / capa | Verificación |
|---|---|---|
| RF-1 | `ProductoNegocio.CrearProductoAsync` (Negocio) · `ProductoModalForm` (Presentación) | `dotnet test` + visual |
| RF-2 | Validación Recorte/formato numérico 8–13 en `ProductoNegocio` · `HasMaxLength(13)` sobre `codigo` (**texto**) en `AppDbContext` · `ProductoModalForm` | `dotnet test` + `dotnet build` + visual |
| RF-3 | Unicidad case-insensitive en `ProductoNegocio` (mensaje específico si el existente está inactivo, D-1) · índice único filtrado en `AppDbContext` | `dotnet test` + visual + BD real |
| RF-4 | Validación Nombre 3–100 en `ProductoNegocio` · `HasMaxLength(100)` · `ProductoModalForm` | `dotnet test` + visual |
| RF-5 | Validación Descripción ≤300 en `ProductoNegocio` · `HasMaxLength(300)` | `dotnet test` |
| RF-6 | Validación de categoría existente y **activa** en `ProductoNegocio` (D-5) | `dotnet test` |
| RF-7 | Validación de precios ≥ 0 en `ProductoNegocio` | `dotnet test` |
| RF-8 | `AdvertenciaPrecioVentaMenorCompra` en `ProductoNegocio` · diálogo no bloqueante **antes de persistir** en `ProductoModalForm` (D-19) | `dotnet test` + visual |
| RF-9 | `ModificarProductoAsync` (autoexclusión, Código inmutable D-11, conserva `FechaRegistro`, edita inactivo sin reactivar D-20) · `ProductoModalForm` modo edición | `dotnet test` + visual |
| RF-10 | `ProductoNegocio.AjustarStockAsync` (valor absoluto, motivo, mínimo 0, sobrescribe auditoría D-21) · modal de ajuste en `ProductosForm` | `dotnet test` + visual |
| RF-11 | `ProductoNegocio.PuedeGestionarProductos` (permiso mínimo, D-18-bis) · guard en handlers de `ProductosForm` | `dotnet test` + visual |
| RF-12 | `ProductoNegocio.DarBajaProductoAsync` · confirmación simple en `ProductosForm` (D-13) | `dotnet test` + visual |
| RF-13 | `ProductoNegocio.ReactivarProductoAsync` (bloqueo por categoría inactiva, D-12) · botón Reactivar | `dotnet test` + visual |
| RF-14 | `ProductoNegocio.ListarProductosAsync` · grilla real de `ProductosForm` | `dotnet test` + visual |
| RF-15 | Filtros de `ListarProductosAsync` (`FiltroProductosDto`) · `txtBuscar`/`cboCategoria`/`cboEstado` reales | `dotnet test` + visual |
| RF-16 | Manejo de excepción en handlers de `ProductosForm` (patrón `CargarCategoriasAsync`) | visual |
| RF-17 | DTOs sanitizados (`DTOs/Productos/`) + `NombreCategoria` por join | `dotnet test` (DTOs) + revisión |
| RF-18 | Restricción de **escritura** en `CapaNegocio` (`PuedeGestionarProductos`) · **`PoliticaAcceso` NO se toca** (D-18-bis) · `PoliticaAccesoTests` **sin cambios** | `dotnet test` + visual (pestaña oculta al Vendedor) |
| RF-19 | `HasDefaultValueSql("GETDATE()")` en `AppDbContext` (creación) + **trigger `AFTER UPDATE`** de `fecha_modificacion` creado en la migración (D-16/D-23) · asignación de auditoría de usuario en `ProductoNegocio` | BD real (ambas fechas; límite `InMemory`) + `dotnet build` |
| RF-20 | Auditoría de **quién** en `ProductoNegocio` (`IdUsuario` **NOT NULL** en alta, `IdUsuarioModifica` en edición, usuario del ajuste; último evento D-21) · campos en `Producto` · `IdUsuario` en los DTOs de entrada | `dotnet test` + BD real + visual |
| RNF-3 / FK | `onDelete` de `Producto → Categoria` corregido a `Restrict` en `AppDbContext` (D-12 del plan) | revisión + BD real (introspección de la FK) |
| D-22 (semilla) | `scripts/seed-desarrollo.sql`: `INSERT`/`UPDATE` de `PRODUCTO` con `id_usuario` resuelto por variable (`@idUsuarioAdministrador`, DNI `90000001`), idempotente y re-ejecutable (sin `DELETE`) | re-ejecución de `preparar-bd.ps1` (y `-SoloDatos`) sin duplicados ni fallos |

Todos los RF se consolidan con compilación limpia (`dotnet build`) y `dotnet test` en verde, según los Criterios de finalización de la spec.

## 7. Ronda de correcciones 2026-10-27 (O-4)

> Requisitos que implementa: **RF-21, RF-22, RF-23 y RF-24** (observación O-4), aprobados por el usuario el 2026-10-27 (ver «Ronda de correcciones (2026-10-27)» en `spec.md`). Es el **espejo** de la ronda O-1 de la spec 001, que dejó el mismo patrón cerrado y verificado (`ConfirmacionBajaCategoria` + `ObtenerConfirmacionBajaAsync`, §7 de `specs/001-abm-categorias/plan.md`).
> **No cambia la semántica de RF-1…RF-20 ni de RNF-1…RNF-7**: los textos al usuario, las validaciones, los mensajes de resultado y los flujos de pantalla se conservan; cambia **dónde** se decide (Negocio, no la vista) y **qué fuente** se usa para decidir (el estado actual en la base, no el de la grilla).
> **Sin migraciones. `CapaEntidad` y `CapaDatos` no se tocan.** `DarBajaProductoAsync`, `ReactivarProductoAsync` y el resto de `ProductoNegocio` **se conservan sin cambios** (siguen siendo la fuente de verdad del resultado y de las validaciones de permiso/estado/categoría).
> **Referencia a §4**: la fila **D-3** («baja en dos pasos: confirmación simple en UI → `DarBajaProductoAsync`») queda **parcialmente sustituida** por esta ronda en su parte de composición del diálogo y de los textos: la confirmación simple (D-13 de la spec) se mantiene, pero el título, la pregunta y la acción pasan a `CapaNegocio`. La fila D-3 se conserva en §4 como historial; no se reescribe.

### 7.1 Diseño por capas (solo lo que cambia)

**CapaEntidad — sin cambios.** `Producto` no se modifica: no se agregan propiedades calculadas ni navegaciones. El estado se lee del campo `Estado` ya existente.

**CapaDatos — sin cambios.** Ninguna configuración Fluent nueva, ninguna migración. La consulta de confirmación usa el `DbSet<Producto>` ya expuesto por `AppDbContext`, con `AsNoTracking()` y proyección mínima (mismo patrón que §1/§3).

**CapaNegocio.**
1. **Enum nuevo** `AccionCambioEstadoProducto { Ninguna, Baja, Reactivacion }` en `CapaNegocio/Enums/` (un archivo por tipo, patrón del repo).
2. **DTO nuevo** `ConfirmacionCambioEstadoProducto` en `CapaNegocio/DTOs/Productos/` (un archivo por DTO), sin entidad pura y sin datos sensibles.
3. **Método nuevo** `ObtenerConfirmacionCambioEstadoAsync(int idProducto)` en `ProductoNegocio`: única fuente de la acción aplicable, del título, del mensaje de confirmación y de la posibilidad de ejecutar (RF-21, RF-22, RF-23, RF-24).
4. **Sin cambios** en `DarBajaProductoAsync`, `ReactivarProductoAsync`, `CrearProductoAsync`, `ModificarProductoAsync`, `AjustarStockAsync`, `ListarProductosAsync`, `PuedeGestionarProductos` ni en sus DTOs/Enums.

**CapaPresentacion — `ProductosForm.btnEstado_Click`** (único punto de UI que cambia):
- **Se elimina**: la lectura de `producto.Estado` como insumo de la decisión; el `if (estaActivo)` que bifurcaba la llamada a Negocio; el ternario con los dos títulos (`"Dar de baja producto"` / `"Reactivar producto"`) y el ternario con las dos preguntas (`"¿Confirma que desea dar de baja el producto {nombre}?"` / `"¿Confirma que desea reactivar el producto {nombre}?"`).
- **Lógica nueva**: pide `ObtenerConfirmacionCambioEstadoAsync(producto.IdProducto)`; si `PuedeEjecutarse == false` muestra `Mensaje` en `MaterialMessageBox` con título `"Atención"` (aviso, **sin** diálogo SÍ/NO y **sin** escribir) y retorna; si es `true`, muestra `TituloDialogo`/`Mensaje` en el `MaterialDialog` SÍ/NO y, al confirmar, ejecuta la operación según `Accion` (`Baja` ⇒ `DarBajaProductoAsync`; `Reactivacion` ⇒ `ReactivarProductoAsync`), mostrando los resultados **exactamente** como hoy (`"Atención"` si el resultado no es `Exito`; títulos `"Producto dado de baja"` / `"Producto reactivado"` si es `Exito`).
- **Se conserva sin cambios**: el guard defensivo por rol (`PoliticaAcceso.PuedeGestionarProductos(_rolActual)` con el mensaje y el título `"Acceso denegado"`); el aviso por fila no seleccionada con su mensaje según el texto vigente del botón; `_btnEstado.Enabled = false`; el `try/catch` con `"No se pudo cambiar el estado del producto.\nDetalle: …"` y título `"Error SQL"`; el `finally` con `ActualizarEstadoBotones()`; la recarga `await CargarProductosAsync()` tras el éxito.
- **Decisión de orden (nueva)**: `_btnEstado.Enabled = false` se ejecuta **antes** de la consulta de confirmación y el flujo completo (consulta → aviso/diálogo → ejecución → recarga) va **dentro del `try` existente**, de modo que un fallo de BD al componer la confirmación caiga en el mismo `catch` con `"Error SQL"` (RF-16) y el `finally` rehabilite el botón. Antes el diálogo quedaba fuera del `try` porque no había consulta previa.
- **No se tocan** `ProductoModalForm`, `AjustarStockModalForm`, `inicio`, `Program.cs` ni el DI (`ProductoNegocio` ya está registrado por T8).

### 7.2 Contratos

**Firma exacta (CapaNegocio, pública, asíncrona, solo lectura):**

```csharp
public async Task<ConfirmacionCambioEstadoProducto> ObtenerConfirmacionCambioEstadoAsync(int idProducto)
```

**Enum nuevo `AccionCambioEstadoProducto`** (`CapaNegocio/Enums/AccionCambioEstadoProducto.cs`): `Ninguna = 0`, `Baja = 1`, `Reactivacion = 2`. `Ninguna` significa "no hay acción aplicable" y siempre viaja con `PuedeEjecutarse == false`.

**DTO nuevo `ConfirmacionCambioEstadoProducto`** (`CapaNegocio/DTOs/Productos/ConfirmacionCambioEstadoProducto.cs`):

| Campo | Tipo | Origen / significado |
|---|---|---|
| `Accion` | `AccionCambioEstadoProducto` | Acción que corresponde ejecutar si el usuario confirma (RF-21); `Ninguna` cuando no hay nada que ejecutar |
| `PuedeEjecutarse` | `bool` | `true` **solo** si `Accion != Ninguna`; la UI no recalcula la condición (RF-22) |
| `IdProducto` | `int?` | id de la fila consultada; `null` si el id es inválido o el producto no existe |
| `NombreProducto` | `string?` | `Producto.Nombre` de la fila consultada (insumo del mensaje y de la trazabilidad); `null` en los casos sin producto |
| `TituloDialogo` | `string` | título del diálogo **ya compuesto** en español (`= string.Empty`) |
| `Mensaje` | `string` | pregunta de confirmación o aviso **ya compuesto** en español, listo para mostrar (`= string.Empty`) |

**Comportamiento por caso (textos literales — deben ser idénticos a los que hoy redacta la vista):**

| Situación | `Accion` | `PuedeEjecutarse` | `IdProducto` | `NombreProducto` | `TituloDialogo` | `Mensaje` |
|---|---|---|---|---|---|---|
| `idProducto <= 0` | `Ninguna` | `false` | `null` | `null` | `""` | `Debe indicar un producto válido.` |
| no existe | `Ninguna` | `false` | `null` | `null` | `""` | `El producto indicado no existe.` |
| existe y `Estado == true` | `Baja` | `true` | el id | el nombre | `Dar de baja producto` | `¿Confirma que desea dar de baja el producto {nombre}?` |
| existe y `Estado == false` | `Reactivacion` | `true` | el id | el nombre | `Reactivar producto` | `¿Confirma que desea reactivar el producto {nombre}?` |

- Los dos textos de confirmación son **literalmente** los que hoy compone la vista (`ProductosForm.cs`, líneas 841-844): sin regresión de texto (RF-21, RF-22; RNF-2).
- Los textos de los casos sin producto coinciden con los de `DarBajaProductoAsync` (`"Debe indicar un producto válido."`, `"El producto indicado no existe."`), así el usuario ve el mismo mensaje antes o después de la operación.
- El título `"Atención"` del **aviso** (`PuedeEjecutarse == false`) es una constante de presentación que se queda en la vista, igual que en la ronda 001 y en el resto de los avisos de `ProductosForm`.

**Orden interno (1 consulta de lectura, ninguna escritura):**
1. `idProducto <= 0` ⇒ `Ninguna` / `false` sin consultar.
2. Una consulta a `Productos` con `AsNoTracking()` y proyección de `IdProducto`, `Nombre` y `Estado` (`FirstOrDefaultAsync`), filtrada por id.
3. Si no existe ⇒ `Ninguna` / `false` con `"El producto indicado no existe."`.
4. Si `Estado == true` ⇒ `Baja` + título y mensaje de baja; si `Estado == false` ⇒ `Reactivacion` + título y mensaje de reactivación.
5. **No** llama a `SaveChangesAsync` ni a `DarBajaProductoAsync`/`ReactivarProductoAsync`: pedir la confirmación **nunca** escribe (RF-24).
6. **No** valida permiso por rol, ni estado "ya inactivo/ya activo", ni categoría inactiva: esas reglas viven en las operaciones de ejecución, que corren después (RF-21, nota de alcance de la spec).

### 7.3 Alternativas descartadas

| # | Decisión | Alternativa descartada | Por qué se descarta |
|---|---|---|---|
| O-4a | La acción, el título y la pregunta se componen en `CapaNegocio` leyendo el estado actual de la base. | **(a) Situación actual**: la vista deriva la acción de `producto.Estado` (fila del listado) y redacta los textos con ternarios. | Deja la regla de negocio y el texto en español en el evento visual (viola RNF-5 y AGENTS: "nunca lógica de negocio en eventos visuales") y decide sobre un dato que puede estar desactualizado respecto de la base. |
| O-4b | La confirmación valida **solo** id, existencia y estado para decidir la acción; permiso/estado repetido/categoría inactiva se validan al ejecutar. | Validar también el permiso por rol y la categoría inactiva dentro de la confirmación. | Duplicaría reglas que ya están en `DarBajaProductoAsync`/`ReactivarProductoAsync` (dos fuentes de verdad, mensajes que podrían divergir) y la confirmación necesita el `IdUsuario`/rol, que hoy no recibe. La confirmación debe ser una lectura barata y estable; el resultado real lo sigue dando la ejecución. |
| O-4c | DTO con `Accion` (enum de 3 valores) + `PuedeEjecutarse`. | Un `Resultado` unificado (p. ej. reutilizar `ResultadoBajaProducto`/`ResultadoReactivacionProducto` o un enum nuevo con los cuatro casos). | Un único enum obligaría a mezclar dos dominios distintos (baja y reactivación) y la vista tendría que volver a mapear resultado→acción para saber qué método llamar; `Accion` expresa directamente **qué ejecutar** y `PuedeEjecutarse` la decisión, sin lógica de traducción en la UI. |
| O-4d | `TituloDialogo` y `Mensaje` viajan ya compuestos. | Devolver solo datos (nombre, estado) y dejar que la vista arme los textos. | Vuelve a poner los textos de negocio en la interfaz (motivo exacto de O-4); los literales deben quedar fijados por tests, no por la vista. |

### 7.4 Pruebas

`dotnet test` sobre `CapaNegocio.Tests` (xUnit + `InMemory`, archivo nuevo `ProductoNegocioConfirmacionCambioEstadoTests.cs`, `partial class ProductoNegocioTests`, patrón de `ProductoNegocioBajaReactivacionTests.cs`). **Total esperado: 243 = 237 actuales + 6 casos nuevos** (5 métodos, uno de ellos `[Theory]` de 2 casos). Los tests existentes de baja/reactivación **no se modifican**.

| RF | Qué lo verifica | Verificación concreta |
|---|---|---|
| RF-21 | Producto **activo** ⇒ baja, con textos exactos | *Test*: producto con `Estado = true` ⇒ `Accion = Baja`, `PuedeEjecutarse = true`, `IdProducto` y `NombreProducto` de la fila, `TituloDialogo` **igual** a `"Dar de baja producto"` y `Mensaje` **igual** a `"¿Confirma que desea dar de baja el producto {nombre}?"`. |
| RF-21 | Producto **inactivo** ⇒ reactivación, con textos exactos | *Test*: producto con `Estado = false` ⇒ `Accion = Reactivacion`, `PuedeEjecutarse = true`, `TituloDialogo` **igual** a `"Reactivar producto"` y `Mensaje` **igual** a `"¿Confirma que desea reactivar el producto {nombre}?"`. |
| RF-23 | Producto inexistente | *Test*: id 999 ⇒ `Accion = Ninguna`, `PuedeEjecutarse = false`, `IdProducto = null`, `NombreProducto = null`, `Mensaje` `"El producto indicado no existe."` y tabla sin cambios. |
| RF-23 | Identificador inválido | *Test* `[Theory]` con `0` y `-5` ⇒ `Accion = Ninguna`, `PuedeEjecutarse = false`, `Mensaje` `"Debe indicar un producto válido."` y sin consulta/escritura. |
| RF-24 | La confirmación no escribe | *Test*: sobre un producto activo (y sobre uno inactivo) se invoca la confirmación varias veces y se verifica que `Estado`, `IdUsuarioModifica`, `FechaModificacion` y `MotivoAjusteStock` quedan **intactos** y que el conteo de filas de `Productos` no cambia. |
| RF-22 | La vista ya no compone la regla ni los textos | *Inspección de `ProductosForm.btnEstado_Click`*: **0** literales `"¿Confirma que desea dar de baja el producto"` / `"¿Confirma que desea reactivar el producto"` en todo `CapaPresentacion/`, **0** usos de `producto.Estado` para decidir la acción, **1** llamada a `ObtenerConfirmacionCambioEstadoAsync`, y los textos mostrados provienen de `TituloDialogo`/`Mensaje`. *Validación visual* (usuario): diálogo y aviso con los mismos textos. |

### 7.5 Riesgos

- **Consulta extra de lectura (aceptada)**: cada clic en el botón de estado suma una consulta de una fila antes del diálogo (el flujo confirmar→ejecutar pasa de 1 a 2 lecturas y la escritura de siempre). Se acepta: volumen mínimo, y el beneficio es que la regla, la acción y los textos salen de la UI (RF-22). No se introduce transacción ni se modifica la baja/reactivación.
- **Fila desactualizada**: la decisión se toma con el estado de la base, así que la ventana de desfase se reduce a la que hay entre la confirmación y la ejecución; en ese caso la operación responde "ya está inactivo"/"ya está activo" sin efectos. Queda como límite conocido, igual que la concurrencia de altas.
- **Regresión de textos**: los textos deben verse **idénticos** a los actuales; los tests fijan los literales y la validación visual del usuario confirma el diálogo y el aviso. Se conservan el orden de botones (`"SÍ"`/`"NO"`), los títulos de los mensajes de resultado (`"Producto dado de baja"` / `"Producto reactivado"` / `"Atención"`) y el `"Error SQL"`.
- **Excepción de BD en la confirmación**: al mover la consulta dentro del `try` existente, un fallo cae en el mismo `catch` con `"Error SQL"` y el `finally` rehabilita el botón; se verifica por inspección (no hay test de UI).
- **`InMemory` no cubre la UI**: el refactor de la vista se verifica por inspección de código + validación visual; los tests cubren el contrato de Negocio (RF-21, RF-23, RF-24).

