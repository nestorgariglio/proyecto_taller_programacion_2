# Plan técnico — Spec 001 · ABM de Categorías/Rubros

> Fase: **Plan** sobre la spec **aprobada** (`spec.md`, 12 RF, ajustada tras las decisiones del usuario D-1/D-2/D-3 y el reencuadre de acceso **D-4**). La spec manda: solo QUÉ/POR QUÉ ya resuelto; este documento define el CÓMO por capas.
> Principios aplicados: `docs/constitution.md` (4 capas, bajas lógicas, tests como puerta de entrada, español).
> **Estado del plan: vigente y aprobado (actualizado el 2026-10-19).** No queda ninguna decisión pendiente de aprobación del usuario en esta spec.
>
> **CAMBIO DE REQUISITO (reencuadre de acceso — APROBADO por el usuario el 2026-10-19, registrado como D-4 en `spec.md`; la spec está en estado `aprobada`).** La gestión de categorías **deja de ser un ítem del menú principal**: pasa a ser una **característica del módulo Productos** y su ABM se abre desde la pantalla de Productos con un botón **"Gestionar categorías"** que lo despliega en una **ventana modal**, reutilizando `GestionCategoriasControl`/`CategoriaForm`. Este reencuadre: (a) **elimina la pestaña `tab_categorias`** de `inicio` y su cableo; (b) **elimina el crash** `ArgumentException: An item with the same key has already been added. Key: products.png` (la pestaña de categorías duplicaba la `ImageKey` de `tab_productos`); (c) **no toca** `inicio.Designer.cs`/`inicio.resx` más allá de la remoción de `tab_categorias` (el menú principal queda igual); (d) **no altera** el backend (`CapaEntidad`/`CapaDatos`/`CapaNegocio` intactos). Ver §1 (CapaPresentacion), §4 (D-5/D-7) y §6 (trazabilidad RF-10/RF-12).
>
> **RONDA DE CORRECCIONES (2026-10-26) — APROBADA por el usuario.** La spec incorpora **RF-13, RF-14, RF-15** (observación O-1) y **RF-16 + RNF-6** (observación O-2), y **no** altera RF-1…RF-12 ni RNF-1…RNF-5. Este plan se actualiza con la sección **§7**, que **sustituye la mecánica de dos decisiones vigentes**: **D-3** (baja en dos pasos con el mensaje compuesto por la interfaz) queda **sustituida por O-1**, y **D-6** (comparación de unicidad en memoria) queda **sustituida por O-2**. Las filas D-3 y D-6 se conservan en §4 como historial, marcadas como sustituidas. **Sin migraciones y sin cambios en `CapaEntidad`/`CapaDatos`**: el índice único de `descripcion` sigue siendo la red de seguridad de RF-3.

## 0. Verificaciones previas (leído en el repo, NO supuesto)

| Verificación | Resultado |
|---|---|
| `CapaEntidad/Categoria.cs` | **Ya existe**: tabla `CATEGORIA` (`id_categoria`, `descripcion`, `estado`, `fecha_registro`); `Estado` con default `true`. |
| `CapaEntidad/Producto.cs` | **Ya existe** FK `IdCategoria` → `Categoria` (propiedad de navegación `Producto.Categoria`). No existe colección inversa en `Categoria`. |
| `AppDbContext` | **Ya expone** `DbSet<Categoria> Categorias` (línea 26). `OnModelCreating` **no tiene** ninguna configuración Fluent para `Categoria` → se añade en esta spec (**D-2 aprobada**): `HasMaxLength(100)` + índice único de `descripcion` (patrón exacto de `Usuario.Dni`/`Usuario.Correo`, ver §3). |
| Migraciones | La tabla `CATEGORIA` **ya fue creada** en `20260907055526_InitialMigration` y está en `AppDbContextModelSnapshot`. **Falta** la migración nueva por D-2 (nombre al estilo de las existentes: `AddUserConstraints` → `AddCategoryConstraints`); sin otros cambios de esquema. |
| `PoliticaAcceso` | **Ya existe** el módulo `"Categorías"`: administrador y encargado sí, vendedor **no** (líneas 13, 17, 21). |
| `PoliticaAccesoTests` | **Ya cubre** `"Categorías"` para los 3 roles (verde actual). |
| Pantalla de categorías | **Estado al inicio del plan** (foto previa a la implementación): no existía pestaña `tab_categorias` en `inicio`, ni `CategoriaNegocio`, ni DTOs/Enums de categorías, ni `GestionCategoriasControl`, ni `CategoriaForm`. **Posteriormente** (T1–T9) ya existen el negocio, los DTOs/Enums, `GestionCategoriasControl` y `CategoriaForm`; y **tras T11 la pestaña `tab_categorias` fue eliminada** del código por el reencuadre aprobado. |
| Combos | El único combo de categorías del sistema es `ProductosForm.cboCategoria`, hoy **mockeado** con `Items.AddRange({"Todas","Periféricos","Monitores","Audio"})` (línea 60). |
| Patrones de referencia | `UsuarioNegocio` (async + DTOs + Enums de resultado + mensajes en español), `GestionUsuariosControl`/`UsuarioForm` (ABM MaterialSkin 2 con `MaterialDialog` de confirmación y `MaterialMessageBox` de errores), registro DI en `Program.cs`. |
| Pruebas / build | `CapaNegocio.Tests`: xUnit + `Microsoft.EntityFrameworkCore.InMemory` (patrón `CrearContexto...Guid.NewGuid()`). Solución única `SistemaVentaStock.slnx` → `dotnet build` y `dotnet test` en la raíz. |

## 1. Responsabilidades por Capa

### CapaEntidad
- **Sin cambios.** La entidad `Categoria` ya cumple el modelo de dominio de la spec (`Descripcion` nullable — la validez se decide en Negocio; `Estado` default `true` = Activa; `FechaRegistro`).
- **No se añade** colección `ICollection<Producto>`: el conteo de productos activos (RF-6/RF-7) se hace por consulta, sin tocar la entidad (ver Decisión D-4).

### CapaDatos
- El `DbSet<Categoria>` y la tabla `CATEGORIA` **ya existen** (migración inicial); las consultas del ABM se emiten desde `CapaNegocio` sobre ese `DbSet` (mismo patrón que `UsuarioNegocio`, que no tiene una clase de repositorio aparte).
- **Único cambio (D-2, aprobado por el usuario)**: configuración Fluent de `Categoria` en `OnModelCreating`:
  - `HasMaxLength(100)` sobre `descripcion` (respaldo de RF-2),
  - **índice único** sobre `descripcion` imitando el patrón exacto de `Usuario.Correo` (columna nullable): `HasIndex(...).IsUnique().HasFilter("[descripcion] IS NOT NULL")`. El repo **no usa collations explícitas** (verificado: ni en `AppDbContext` ni en las migraciones `AddUserConstraints`/`AddUserSex`): la unicidad case-insensitive se apoya en la colación `CI_AS` por defecto de SQL Server, el mismo supuesto que ya hace el índice único de `correo`; el check de Negocio (D-6) sigue siendo quien produce el mensaje en español.
  - Requiere **migración EF Core** (comandos y verificaciones previas en §3).
- Obligación verificada: **ningún `DELETE`** — toda baja es `Estado = false` (RNF-4 / Constitución 3).

### CapaNegocio
- Nuevo **`CategoriaNegocio`** (ctor con `AppDbContext`, operaciones `async`, mapeo explícito, mensajes de retorno en español), con:
  - `CrearCategoriaAsync(CrearCategoriaDto)` → alta con Recorte + formato + unicidad, `Estado = true` (RF-1, RF-2, RF-3): ante duplicado, el `Mensaje` distingue si la categoría existente está activa o inactiva —en inactiva, indica que existe inactiva y puede reactivarse desde el listado— y **no** la reactiva (D-1, ver Definiciones/RF-3 de la spec).
  - `ModificarCategoriaAsync(ModificarCategoriaDto)` → mismos criterios con autoexclusión (RF-4), sin tocar vínculos con productos (RF-5).
  - `ContarProductosActivosAsync(idCategoria)` → solo productos con `Estado = true` (RF-6, RF-7; caso límite: productos inactivos no cuentan). **Se conserva** (§7): es el contrato verificado por los tests de RF-6 y el colaborador interno de `ObtenerConfirmacionBajaAsync`; desde O-1 la vista ya **no** la consume directamente.
  - `DarBajaCategoriaAsync(idCategoria)` → `Estado = false` sin borrado físico; sin efecto sobre categorías ya inactivas (RF-8, RNF-4).
  - `ReactivarCategoriaAsync(idCategoria)` → `Estado = true` sin crear registro nuevo (RF-9).
  - `ListarCategoriasAsync()` → **todas** (activas e inactivas) con su estado (RF-10).
  - `ListarCategoriasActivasAsync()` → solo `Estado = true` para combos (RF-11).
- Validaciones y reglas: recorte de bordes (espacios, tabuladores y saltos de línea), longitud 3–100 tras recorte, unicidad case-insensitive autoexcluyente con mensaje que distingue duplicado activo/inactivo (D-1), mensajes en español. El **mecanismo** de la unicidad se reescribe en O-2 (§7.3): una sola consulta a la base en lugar de la comparación en memoria de D-6; el mensaje resultante no cambia.
- **DTOs** de categorías (`CapaNegocio/DTOs/Categorias/`) y **Enums** de resultado (patrón de los 4 enums ya usados por usuarios).
- `PoliticaAcceso` **no se modifica** (módulo `"Categorías"` ya resuelve RF-12 del lado de negocio; queda verificado por los tests existentes).

### CapaPresentacion
- **`GestionCategoriasControl`** (UserControl MaterialSkin 2, patrón `GestionUsuariosControl`): grilla con todas las categorías y su estado (Activo/Inactivo), filtros de búsqueda y por estado, botones **Nuevo / Editar / Dar de baja / Reactivar / Actualizar**, habilitación según la fila seleccionada (Editar siempre — también inactivas, RF-10; *Dar de baja* solo sobre activas y *Reactivar* solo sobre inactivas, según casos límite). **Sin cambios**: el control ya está construido y se reutiliza tal cual; sólo cambia **dónde se aloja** (antes en la pestaña `tab_categorias` de `inicio`, ahora dentro de un `Form` contenedor modal abierto desde `ProductosForm`).
- **`CategoriaForm`** (modal `MaterialForm`, patrón `UsuarioForm`): alta y modificación con campo de descripción; ante errores muestra el `Mensaje` en español que devuelve Negocio (RF-2, RF-3) y no cierra; en éxito devuelve `DialogResult.OK` para que el control recargue (RF-1: los combos se enteran sin reinicio, RNF-3). **Sin cambios**.
- **Diálogos de baja** con `MaterialDialog` (patrón de `GestionUsuariosControl.btnDesactivar_Click`):
  - Si `ContarProductosActivosAsync` > 0 → diálogo informando **solo la cantidad** de productos activos con continuar/cancelar (RF-7).
  - Si es 0 → confirmación simple (RF-8).
  - Cancelar ⇒ ninguna llamada de escritura ⇒ la categoría queda sin cambios (RF-7).
  - **[Sustituido por O-1 — ver §7]** Desde la ronda 2026-10-26 la vista **no** consulta el conteo ni compone estos textos: pide la confirmación a `ObtenerConfirmacionBajaAsync` y muestra el `Mensaje` recibido (RF-13/RF-15). Los dos textos finales y los botones Sí/No se mantienen idénticos (RF-7/RF-8).
- **Contenedor modal del ABM (`GestionCategoriasModalForm`, nuevo)**: `Form` liviano (patrón modal del repo, mismo estilo MaterialSkin que `CategoriaForm`) cuyo único contenido es un `GestionCategoriasControl` alojado a `Dock = Fill`. Se abre con `ShowDialog(productosForm)` desde `ProductosForm`. Su ctor recibe `CategoriaNegocio` (o, alternativamente, el `GestionCategoriasControl` ya resuelto por DI) para poder cargar el listado con `CargarCategoriasAsync()` al abrirse. **Reutiliza el control existente sin duplicar lógica**; al cerrarse, `ProductosForm` refresca su combo de categorías (RNF-3, D-7).
- **`ProductosForm`**: 
  - **NUEVO botón "Gestionar categorías"** (MaterialButton) agregado al `panelAcciones` que `ProductosForm` crea por código en `ArmarInterfazMaterial()` (verificado: el `FlowLayoutPanel` `panelAcciones` existe en esa construcción y hoy aloja Nuevo/Editar/Desactivar/Actualizar), que abre `GestionCategoriasModalForm` con `ShowDialog(this)`. **Gating por rol (RF-12)**: el botón sólo es **visible** para Administrador/Encargado (`PoliticaAcceso.TieneAcceso(rol, "Categorías")`); para el Vendedor queda oculto. `ProductosForm` ya recibe `CategoriaNegocio` por DI (ctor actual: `ProductosForm(CategoriaNegocio)`); **hay que hacerle llegar el rol** (verificado: hoy no lo conoce): se añade un método público de la vista (p. ej. `AplicarRolUsuario(string? rol)`) invocado desde `inicio.AplicarPermisosPorRol()` —el mismo punto donde ya se resuelve `OcultarSiNoAutorizada(...)` con `_usuarioActual?.RolDescripcion`—, con lo que el botón se muestra u oculta sin tocar la firma del ctor ni el DI. Al cerrar el modal con éxito se vuelve a llamar `CargarCategoriasAsync()` para reflejar altas/bajas/reactivaciones en el combo sin reiniciar (RNF-3).
  - `cboCategoria` deja de mockearse y se puebla con `ListarCategoriasActivasAsync` (manteniendo "Todas" como primera opción para el filtro), recargándose al entrar en la pestaña (RF-11, RNF-3). ***Alcance acotado (D-3)**: solo este combo; la grilla de productos sigue mockeada (spec de productos aparte) y no se toca ningún otro combo.*
- **`inicio` (y `inicio.Designer.cs`/`inicio.resx`)**: **SE ELIMINA la pestaña `tab_categorias`** y todo su cableo:
  - quitar `tab_categorias` de `materialTabControl1.Controls.Add(...)`, de `RestaurarNavegacionBase()` (array `navegacionBase`) y su `ImageKey` (`products.png`, causa del **crash** `ArgumentException: An item with the same key has already been added. Key: products.png` al duplicar la clave de `tab_productos` en la misma `ImageList`);
  - quitar la línea `OcultarSiNoAutorizada("tab_categorias", "Categorías", rol)` y el handler `tab_categorias_Enter`;
  - quitar el campo `_gestionCategoriasControl` y su `tab_categorias.Controls.Add(...)`/`Enter` del ctor; el control **se sigue registrando por DI** (para el modal) pero **no** se inyecta en `inicio`;
  - el menú principal **queda idéntico**: no se agrega ni se quita ningún otro ítem y **no se editan** `inicio.Designer.cs`/`inicio.resx` salvo la remoción de `tab_categorias` (que sí vive en el Designer). `imageList1` conserva las demás claves intactas.
- **`Program.cs`**: **sin cambios** (ya registra `CategoriaNegocio`, `GestionCategoriasControl` y `ProductosForm` por DI). El `GestionCategoriasModalForm` se instancia directamente desde `ProductosForm` con la dependencia ya inyectada (patrón `new CategoriaForm(_categoriaNegocio)`), o se registra como `AddTransient` si se prefiere resolverlo por DI; se elige la instancia directa por ser un modal efímero y evitar sobre-ingeniería.
- Sin botón de acceso directo en el dashboard de `inicio`: no lo pide la spec (anti scope-creep).

## 2. DTOs y Mapeos

Todos viven en `CapaNegocio/DTOs/Categorias/`. **Nunca** viaja la entidad `Categoria` pura a la UI. Los DTOs de salida no exponen nada sensible (no hay credenciales en esta entidad; se mantiene la convención de DTOs sanitizados de MEMORY).

| DTO | Sentido | Campos | RF / Uso |
|---|---|---|---|
| `CrearCategoriaDto` | Presentación → Negocio | `Descripcion` (texto crudo del campo, sin recortar) | RF-1, RF-2, RF-3 (alta) |
| `ModificarCategoriaDto` | Presentación → Negocio | `IdCategoria`, `Descripcion` | RF-4 (edición) |
| `CategoriaRespuestaDto` | Negocio → Presentación | `IdCategoria`, `Descripcion`, `Estado`, `FechaRegistro` | RF-10 (grilla), RF-11 (combo: `DisplayMember = Descripcion`, `ValueMember = IdCategoria`) |
| `RespuestaCreacionCategoria` | Negocio → Presentación | `Resultado` (`ResultadoCreacionCategoria`), `Categoria?`, `Mensaje` | RF-1, RF-2, RF-3 |
| `RespuestaModificacionCategoria` | Negocio → Presentación | `Resultado` (`ResultadoModificacionCategoria`), `Categoria?`, `Mensaje` | RF-4, RF-5 |
| `RespuestaBajaCategoria` | Negocio → Presentación | `Resultado` (`ResultadoBajaCategoria`), `Categoria?`, `Mensaje` | RF-8 |
| `RespuestaActivacionCategoria` | Negocio → Presentación | `Resultado` (`ResultadoActivacionCategoria`), `Categoria?`, `Mensaje` | RF-9 |

**Enums** (`CapaNegocio/Enums/`, patrón de los existentes de usuarios): `ResultadoCreacionCategoria { Exito, DatosInvalidos, DescripcionDuplicada }`, `ResultadoModificacionCategoria { Exito, DatosInvalidos, CategoriaNoEncontrada, DescripcionDuplicada }`, `ResultadoBajaCategoria { Exito, DatosInvalidos, CategoriaNoEncontrada, CategoriaYaInactiva }`, `ResultadoActivacionCategoria { Exito, DatosInvalidos, CategoriaNoEncontrada, CategoriaYaActiva }`.

**Nota D-1**: `DescripcionDuplicada` **no** se divide en dos enums: la distinción entre duplicado activo e inactivo viaja en el campo `Mensaje` del `RespuestaCreacionCategoria`/`RespuestaModificacionCategoria` (en inactiva, sugiere reactivarla desde el listado). Un enum nuevo solo sería necesario si la UI tuviera que bifurcar la conducta, y no la bifurca: siempre rechaza.

**Mapeos** (explícitos, obligación del repo):
- Negocio → DTO: método privado estático `MapearCategoria(Categoria)` (como `MapearUsuario`) y proyección `Select` en los listados con `AsNoTracking()`.
- DTO → entidad: solo dentro de `CategoriaNegocio` (alta crea `new Categoria { Descripcion = yaRecortada, Estado = true }`; edición asigna `Descripcion` sobre la entidad cargada; **nunca** se cambia `IdCategoria` ni `Estado` en la edición).
- Presentación → Negocio: la UI solo arma los DTOs de entrada y bindea los DTOs de salida a grilla/combo.

## 3. Persistencia

**Situación verificada (leyendo el código)**: la tabla `CATEGORIA` **ya existe** (migración `20260907055526_InitialMigration`, presente en `AppDbContextModelSnapshot`) y `AppDbContext` ya expone `DbSet<Categoria> Categorias`. **D-2 resuelto por el usuario: SÍ migración** (AGENTS.md: "preguntar antes de crear migraciones" → preguntada y aprobada). Además de la validación en `CapaNegocio`, se añade defensa en BD en `OnModelCreating`:

- `HasMaxLength(100)` sobre `descripcion` (respaldo de RF-2),
- **índice único** sobre `descripcion` imitando el patrón exacto de `Usuario.Correo` (columna nullable): `.HasIndex(c => c.Descripcion).IsUnique().HasFilter("[descripcion] IS NOT NULL")`. El repo **no define collations explícitas** (verificado en `AppDbContext` y en las migraciones): la unicidad case-insensitive queda en la colación `CI_AS` por defecto de SQL Server, el mismo supuesto que ya hace el índice único de `correo` (ver patrón en `AddUserConstraints`),
- el índice es la red de seguridad del caso límite *"dos usuarios crean la misma descripción a la vez"*: la segunda escritura recibiría el error de unicidad de BD; el camino normal (y el que produce el **mensaje en español**, también cuando la existente está inactiva) es el check previo de Negocio (D-1/D-6).

**Comandos de migración (aprobados por el usuario):**
1. Verificación previa en la BD de desarrollo: que **no** haya descripciones `NULL`, de más de 100 caracteres ni duplicadas ignorando mayúsculas (`scripts/seed-desarrollo.sql` no carga categorías — verificado —, pero la BD real puede tener datos).
2. `dotnet ef migrations add AddCategoryConstraints --project CapaDatos --startup-project CapaDatos --context AppDbContext`
3. `dotnet ef database update --project CapaDatos --startup-project CapaDatos --context AppDbContext` → **sincronizar la BD antes de probar la UI** (aprendizaje de MEMORY: omitirlo genera excepciones de mapeo en EF Core).
4. **Límite conocido**: los tests con `InMemory` no validan índices únicos ni longitudes; esta rama se verifica con `dotnet build` + aplicación real de la migración + prueba manual de duplicado en la UI.

## 4. Decisiones técnicas

> **Dudas del usuario cerradas en la sesión de ajuste** (los números corresponden a las 3 dudas planteadas, no a las filas de esta tabla): **D-1** → el alta duplicada sobre nombre inactivo se **rechaza** con mensaje específico que sugiere reactivarla desde el listado; nunca reactivación automática. **D-2** → **sí migración** (coincide con la fila D-2 de abajo y con §3). **D-3** → RF-11/RNF-3 se cumplen en esta iteración solo con el combo de categorías de `ProductosForm`.
>
> **Cambio de requisito (reencuadre de acceso — APROBADO por el usuario el 2026-10-19, D-4 en `spec.md`)**: la gestión de categorías deja de ser un ítem del menú principal y pasa a abrirse desde `ProductosForm` en un modal (fila **D-5** reemplazada). Elimina la pestaña `tab_categorias` y, con ella, el crash `products.png`. Backend (`CapaEntidad`/`CapaDatos`/`CapaNegocio`) intacto: `PoliticaAcceso`, `CategoriaNegocio`, DTOs/Enums y la migración `AddCategoryConstraints` no cambian.

| # | Decisión | Alternativas descartadas | Por qué |
|---|---|---|---|
| D-1 | Crear **`CategoriaNegocio` propio** con sus DTOs/Enums. | (a) Meter el ABM en `UsuarioNegocio`; (b) lógica en eventos de WinForms. | (a) viola SRP y mezcla dominios; (b) está prohibido por AGENTS/Constitución ("nunca lógica de negocio en eventos visuales"). |
| D-2 | Índice único en BD **aprobado por el usuario** (§3): `HasMaxLength(100)` + índice único con filtro, imitando `Usuario.Correo`; base: unicidad en `CapaNegocio`. | (a) Confiar en la colación de la BD sin índice; (b) `EF.Functions.Collate`; (c) `IsRequired()` sobre `descripcion` para omitir el filtro. | (a) no es portable, `InMemory` no la aplica y RF-3 quedaría sin test; (b) es específico de SQL Server y no ejecutable en los tests `InMemory`; (c) cambiaría el contrato de nulabilidad de la entidad (`string?`, decisión de CapaEntidad no pedida) y el patrón del repo para columnas nullable es el filtro (`correo`), no `IsRequired`. El check en Negocio sigue siendo el que produce el **mensaje** en español. |
| D-3 | **[SUSTITUIDA por O-1 — ver §7.1/§7.2]** **Baja en dos pasos**: `ContarProductosActivosAsync` → diálogo en UI → `DarBajaCategoriaAsync`. | (a) Método único que "advierte" (imposible: `CapaNegocio` no tiene UI); (b) baja directa sin consulta de conteo. | Cumple RF-6/RF-7 (la UI decide según el conteo) manteniendo el aislamiento; la cancelación nunca llega a escribir. |
| D-4 | Conteo de productos activos por **consulta** (`_db.Productos.CountAsync(p => p.IdCategoria == id && p.Estado)`), sin `Include`. | Añadir `ICollection<Producto>` a `Categoria` + `Include`. | Evita modificar la entidad (CapaEntidad sin cambios) y evita traer filas innecesarias; mismo criterio que la navegación `Producto.Categoria` existente. |
| D-5 | **[REEMPLAZADA por el cambio de requisito]** El ABM vive en un **modal abierto desde `ProductosForm`** mediante el botón "Gestionar categorías" (gating por rol, RF-12). Reutiliza `GestionCategoriasControl` dentro de un `Form` contenedor (`GestionCategoriasModalForm`). | (a) Pestaña propia `tab_categorias` en `inicio` (**decisión original; descartada por el usuario**: las categorías son una característica de productos, no una sección del menú, y la pestaña causaba el crash de la `ImageKey` duplicada); (b) botón dentro de `ProductosForm` sin modal. | El usuario decidió que las categorías son una **característica del módulo Productos** (útiles para filtrar y definir un producto), no un ítem del menú. El modal cumple RF-10 (listado de gestión completo) y RF-12 sigue apoyado en `PoliticaAcceso` (módulo `"Categorías"` ya testeado), ahora aplicado al botón. Elimina el crash `products.png` duplicada al remover la pestaña. |
| D-6 | **[SUSTITUIDA por O-2 — ver §7.3/§7.4]** Comparación de unicidad con **`StringComparison.OrdinalIgnoreCase`** sobre las descripciones existentes. | `ToUpper()` traducido al SQL. | El catálogo es pequeño; la comparación en memoria es idéntica en SQL Server y en `InMemory` (evita diferencias de colación/soporte del proveedor) y la spec solo pide ignorar mayúsculas/minúsculas (no tildes). |
| D-7 | Refresco del combo de categorías de `ProductosForm` **al entrar en la pestaña** + **al cerrar el modal de gestión** con éxito + recarga del control tras cada operación del ABM. | Eventos globales/observer de invalidación. | Cumple RNF-3 (sin reinicio) con el mínimo acoplamiento, reutilizando el patrón `tab_productos_Enter` ya existente y añadiendo un refresco al cerrar el modal (que reemplaza el disparo que antes producía el `Enter` de `tab_categorias`). |
| D-8 | Validación "doble" de formato: chequeo en UI (foco/mensaje inmediato) **y** en Negocio (fuente de verdad). | Solo UI. | RF-2 exige que el sistema rechace el guardado; los tests de `CapaNegocio` son la puerta de entrada (Constitución 4). |

## 5. Estrategia de tests

- **Fuente automática única**: `CapaNegocio.Tests` (xUnit + EF Core `InMemory`, patrón de `UsuarioNegocioTests` con `CrearContexto...` y base `Guid.NewGuid()`). Comando: **`dotnet test`** en la raíz (o `dotnet test CapaNegocio.Tests`).
- **Ciclo por tarea**: primero los tests nuevos en **rojo**, luego el código en **verde**; ninguna tarea se cierra con pruebas rojas (Constitución 4). Compilación verificada con **`dotnet build`**.
- **Cobertura de RF con test automático**:
  - RF-1: alta exitosa ⇒ `Estado == true`, descripción guardada recortada, registro persistido (conteo de `DbSet`).
  - RF-2: `[Theory]` con vacío / solo espacios / solo tabuladores / solo saltos de línea / `<3` tras recorte / `>100` ⇒ rechazo con mensaje en español; `"  Bebidas  "` ⇒ guardada como `"Bebidas"`; tildes y ñ aceptados.
  - RF-3: duplicado activo y duplicado con distinta caja (`"Bebidas"` vs `"BEBIDAS"`) ⇒ `DescripcionDuplicada`; duplicado sobre categoría **inactiva** ⇒ `DescripcionDuplicada` con `Mensaje` específico que indica que existe inactiva y sugiere reactivarla desde el listado, y la existente **sigue con `Estado = false`** (sin reactivación automática, D-1 de la spec).
  - RF-4: editar sin cambiar su propia descripción ⇒ éxito (autoexclusión); editar al nombre de otra (activa o inactiva) ⇒ `DescripcionDuplicada` con el mismo criterio de mensaje que RF-3.
  - RF-5: sembrar productos vinculados, cambiar la descripción ⇒ productos siguen con el mismo `IdCategoria` y el listado muestra la nueva descripción.
  - RF-6/RF-7: `ContarProductosActivosAsync` ⇒ cuenta solo `Estado = true`; con solo productos inactivos ⇒ 0.
  - RF-8: baja ⇒ `Estado == false`, **no** disminuye el conteo de filas (sin `DELETE`); sobre categoría ya inactiva ⇒ `CategoriaYaInactiva` sin cambio.
  - RF-9: reactivar ⇒ `Estado == true`; reactivar una activa ⇒ `CategoriaYaActiva` y no aparece un segundo registro con esa descripción.
  - RF-10: `ListarCategoriasAsync` ⇒ devuelve activas **e** inactivas con su estado.
  - RF-11: `ListarCategoriasActivasAsync` ⇒ ninguna con `Estado == false` (alcance de UI: combo de `ProductosForm`, D-3).
  - RF-12: **ya cubierto** por `PoliticaAccesoTests` existente (verificado en verde); se exige que siga en verde tras los cambios de UI.
- **Validación visual (UI WinForms, sin tests automáticos)**: alta, modificación, baja con diálogo de advertencia (cantidad) y simple, reactivación, listado con filtros, **apertura del ABM en modal desde el botón "Gestionar categorías" de `ProductosForm`**, botón **oculto para Vendedor** (y visible para Admin/Encargado, RF-12), alta con nombre de una categoría inactiva ⇒ mensaje específico de D-1 sin reactivarla, combo de `ProductosForm` (único combo en alcance, D-3) solo con categorías activas y refresco sin reinicio, y **arranque de la aplicación sin el crash `products.png`** (menú principal sin ítem de categorías).
- **Límite conocido**: `InMemory` no simula concurrencia real ni índices: el caso límite de concurrencia queda respaldado por el índice único de BD (D-2 aprobada, §3) y se prueba manualmente (migración aplicada + dos altas con la misma descripción).

## 6. Trazabilidad (RF → componente/capa → verificación)

| RF | Componente / capa | Verificación |
|---|---|---|
| RF-1 | `CategoriaNegocio.CrearCategoriaAsync` (CapaNegocio) · `CategoriaForm` (Presentación) | `dotnet test` + validación visual |
| RF-2 | Validación Recorte/formato en `CategoriaNegocio` · `CategoriaForm` mensajes · `HasMaxLength(100)` en `AppDbContext` (CapaDatos, respaldo) | `dotnet test` + validación visual + `dotnet build` |
| RF-3 | Unicidad case-insensitive en `CategoriaNegocio` (mensaje específico si la existente está inactiva, D-1) · índice único en `AppDbContext` · mensaje en `CategoriaForm` | `dotnet test` + validación visual |
| RF-4 | `CategoriaNegocio.ModificarCategoriaAsync` (autoexclusión) · `CategoriaForm` modo edición | `dotnet test` + validación visual |
| RF-5 | `ModificarCategoriaAsync` (vínculos intactos) + `ListarCategoriasAsync` | `dotnet test` + validación visual |
| RF-6 | `CategoriaNegocio.ContarProductosActivosAsync` | `dotnet test` |
| RF-7 | `ContarProductosActivosAsync` (cantidad) · `GestionCategoriasControl` diálogo con cantidad | `dotnet test` + validación visual |
| RF-8 | `CategoriaNegocio.DarBajaCategoriaAsync` · diálogo simple en `GestionCategoriasControl` | `dotnet test` + validación visual |
| RF-9 | `CategoriaNegocio.ReactivarCategoriaAsync` · botón *Reactivar* del control | `dotnet test` + validación visual |
| RF-10 | `CategoriaNegocio.ListarCategoriasAsync` · `GestionCategoriasControl` (grilla, filtros, edición de inactivas) alojado en el **modal `GestionCategoriasModalForm` abierto desde `ProductosForm`** | `dotnet test` + validación visual |
| RF-11 | `CategoriaNegocio.ListarCategoriasActivasAsync` · combo `cboCategoria` de `ProductosForm` (único combo en alcance, D-3) | `dotnet test` + validación visual |
| RF-12 | `PoliticaAcceso` + `PoliticaAccesoTests` (CapaNegocio, **ya existentes**) · **botón "Gestionar categorías" oculto para el Vendedor** en `ProductosForm` (gating por rol) | `dotnet test` (regresión) + validación visual |

Todos los RF se consolidan con compilación limpia (`dotnet build`) según los Criterios de finalización.

## 7. Ronda de correcciones 2026-10-26 (O-1 y O-2)

> Requisitos que implementa: **RF-13, RF-14 y RF-15** (observación O-1) y **RF-16 + RNF-6** (observación O-2), aprobados por el usuario el 2026-10-26 (ver «Ronda de correcciones (2026-10-26)» en `spec.md`). **No** cambia la semántica de RF-1…RF-12 ni de RNF-1…RNF-5: los textos al usuario, las validaciones y los flujos de pantalla se conservan; cambia **dónde** se decide (Negocio, no la vista) y **cómo** se consulta (una fila en la base, no la tabla en memoria).
> **Sin migraciones. `CapaEntidad` y `CapaDatos` no se tocan.** `ContarProductosActivosAsync` **se conserva** (contrato de RF-6/RF-7, con sus 3 tests) y pasa a ser colaborador interno del método nuevo.
> **Sustituciones**: **D-3 ⇒ O-1** (la baja deja de resolverse en dos pasos desde la UI) y **D-6 ⇒ O-2** (la unicidad deja de compararse en memoria). Las filas originales quedan en §4 como historial.

### 7.1 Diseño por capas (solo lo que cambia)

**CapaEntidad — sin cambios.** `Categoria` y `Producto` no se modifican: no se añade colección de navegación ni propiedad calculada. El conteo sigue saliendo de una consulta (D-4 vigente).

**CapaDatos — sin cambios.** Ninguna configuración Fluent nueva, ninguna migración (`AddCategoryConstraints` ya está aplicada y en el snapshot). El **índice único** de `descripcion` (§3) sigue siendo la red de seguridad del caso límite de concurrencia y **no** se toca: ni el índice ni la colación intervienen en la nueva consulta de unicidad.

**CapaNegocio.**
1. **DTO nuevo** `ConfirmacionBajaCategoria` en `CapaNegocio/DTOs/Categorias/` (un archivo por DTO, patrón del repo), sin entidad pura y sin datos sensibles.
2. **Método nuevo** `ObtenerConfirmacionBajaAsync(int idCategoria)` en `CategoriaNegocio`: única fuente del mensaje de baja y de la posibilidad de baja (RF-13, RF-14, RF-15).
3. **`ValidarUnicidadDescripcionAsync` reescrito** (firma privada y llamadores intactos): pasa de `ToListAsync()` + comparación en memoria a **una consulta filtrada** con `FirstOrDefaultAsync()` (RF-16, RNF-6).
4. **Sin cambios** en `DarBajaCategoriaAsync`, `ContarProductosActivosAsync`, `CrearCategoriaAsync`, `ModificarCategoriaAsync`, `ConstruirMensajeDuplicado` (mensaje de duplicado literal) ni en `ListarCategoriasAsync`/`ListarCategoriasActivasAsync`.

**CapaPresentacion — `GestionCategoriasControl.btnDarDebaja_Click`** (único punto de UI que cambia):
- **Se elimina**: la llamada a `ContarProductosActivosAsync`; el ternario que compone los dos textos de la baja y sus literales en español; el uso de `categoria.Descripcion` como insumo del mensaje.
- **Se conserva sin cambios**: la validación de fila seleccionada y el aviso "Debe seleccionar una categoría para darla de baja."; `btnDarDebaja.Enabled = false` durante la operación; `try/catch` con `"No se pudo dar de baja la categoría..."`; el `finally` con `ActualizarEstadoBotones()`; el `MaterialDialog` Sí/No con `"SÍ"/"NO"`; la recarga con `CargarCategoriasAsync()` tras el éxito; el `MaterialMessageBox` con el `Mensaje` del negocio cuando el resultado no es `Exito`.
- **Lógica nueva**: pide `ObtenerConfirmacionBajaAsync`; si `PuedeDarseDeBaja == false` muestra `Mensaje` en `MaterialMessageBox` (aviso, **sin** diálogo Sí/No y **sin** escribir); si es `true` muestra `Mensaje` en el `MaterialDialog` y, al confirmar, ejecuta `DarBajaCategoriaAsync` (cuyo resultado se sigue mostrando igual que hoy).
- **No se tocan** `GestionCategoriasModalForm`, `CategoriaForm`, `ProductosForm` ni `Program.cs`.

### 7.2 Contratos

**Firma exacta (CapaNegocio, pública, asíncrona):**

```csharp
public async Task<ConfirmacionBajaCategoria> ObtenerConfirmacionBajaAsync(int idCategoria)
```

**DTO nuevo `ConfirmacionBajaCategoria`** (`CapaNegocio/DTOs/Categorias/ConfirmacionBajaCategoria.cs`):

| Campo | Tipo | Origen / significado |
|---|---|---|
| `Resultado` | `ResultadoBajaCategoria` (**reutilizado**, sin enum nuevo) | `Exito` = la confirmación se obtuvo y la baja **es posible**; `DatosInvalidos` / `CategoriaNoEncontrada` / `CategoriaYaInactiva` = no se puede dar de baja |
| `Descripcion` | `string?` | `Categoria.Descripcion` de la fila consultada; `null` si no existe o si el id es inválido |
| `CantidadProductosActivos` | `int` | salida de `ContarProductosActivosAsync(idCategoria)`; `0` si no existe, ya está inactiva o no hay activos |
| `Mensaje` | `string` | mensaje en español **ya compuesto**, listo para mostrar (`= string.Empty`) |
| `PuedeDarseDeBaja` | `bool` | `true` **solo** si `Resultado == Exito`; la UI no recalcula la condición (RF-15) |

**Comportamiento por resultado (mensajes literales):**

| Situación | `Resultado` | `Descripcion` | `Cantidad` | `PuedeDarseDeBaja` | `Mensaje` |
|---|---|---|---|---|---|
| `idCategoria <= 0` | `DatosInvalidos` | `null` | 0 | `false` | `Debe indicar una categoría válida.` |
| no existe | `CategoriaNoEncontrada` | `null` | 0 | `false` | `La categoría indicada no existe.` |
| ya inactiva | `CategoriaYaInactiva` | la de la fila | 0 | `false` | `La categoría {descripcion} ya está inactiva.` |
| activa con *n* productos activos (*n* > 0) | `Exito` | la de la fila | *n* | `true` | `La categoría {descripcion} tiene {n} producto(s) activo(s) vinculados. ¿Desea desactivarla igualmente?` |
| activa sin productos activos | `Exito` | la de la fila | 0 | `true` | `¿Confirma que desea dar de baja la categoría {descripcion}?` |

- Los dos textos de `Exito` son **literalmente** los que hoy compone la vista: sin regresión de texto (RF-13, RF-8).
- Los textos de `CategoriaNoEncontrada` y `CategoriaYaInactiva` coinciden con los de `DarBajaCategoriaAsync`, así el usuario ve el mismo mensaje antes o después de la operación (RF-14).
- Tildes/ñ se conservan porque solo se interpolan valores ya validados; los textos son en español (RNF-2).

**Orden interno (2 consultas de lectura, ninguna escritura):**
1. `idCategoria <= 0` ⇒ `DatosInvalidos` sin consultar.
2. Una consulta a `Categorias` (`AsNoTracking`, filtrada por `IdCategoria`) para obtener `Descripcion` y `Estado`.
3. Si no existe ⇒ `CategoriaNoEncontrada` (sin contar). Si está inactiva ⇒ `CategoriaYaInactiva` (sin contar).
4. Si está activa ⇒ `ContarProductosActivosAsync(idCategoria)` (reutilizado, RF-6) y composición del mensaje.
5. **No** llama a `SaveChangesAsync` ni a `DarBajaCategoriaAsync`: pedir la confirmación nunca escribe (RF-13: cancelar no deja cambios).

### 7.3 Consulta de unicidad (O-2)

**Método**: `private async Task<string?> ValidarUnicidadDescripcionAsync(string descripcion, int? idCategoriaAExcluir)`. **La firma y los dos llamadores no cambian** (`CrearCategoriaAsync` en el alta, `ModificarCategoriaAsync` en la edición); `ConstruirMensajeDuplicado` tampoco.

**Normalización del parámetro, en memoria y una sola vez:**

```csharp
string descripcionNormalizada = (descripcion ?? string.Empty).ToUpperInvariant();
```

**LINQ esperado (una sola consulta, proyección mínima):**

```csharp
var duplicada = await _db.Categorias
    .AsNoTracking()
    .Where(c => (!idCategoriaAExcluir.HasValue || c.IdCategoria != idCategoriaAExcluir.Value)
        && c.Descripcion != null
        && c.Descripcion.ToUpper() == descripcionNormalizada)
    .Select(c => new { c.Descripcion, c.Estado })
    .FirstOrDefaultAsync();

if (duplicada is null)
{
    return null;
}

return ConstruirMensajeDuplicado(duplicada.Descripcion, duplicada.Estado);
```

**Traducción a SQL (SQL Server):**

```sql
SELECT TOP(1) [c].[descripcion], [c].[estado]
FROM [CATEGORIA] AS [c]
WHERE (@idExcluir IS NULL OR [c].[id_categoria] <> @idExcluir)
  AND [c].[descripcion] IS NOT NULL
  AND UPPER([c].[descripcion]) = @descripcionNormalizada;
```

- **Por qué se normaliza el parámetro en memoria**: el parámetro debe llegar ya en mayúsculas para que `UPPER(columna) = parámetro` funcione igual en SQL Server y en el proveedor `InMemory` de los tests (que evalúa `string.ToUpper()` en cliente). `ToUpperInvariant` es determinista y no depende de la cultura del hilo de la UI; con collation `CI_AS` de SQL Server, `UPPER` y `ToUpperInvariant` coinciden también en `ñ` y vocales acentuadas.
- **`c.Descripcion != null`**: la columna es nullable (entidad y §3); en SQL `UPPER(NULL) = @p` da `NULL` (no cumple) y la guarda evita además la advertencia de nulabilidad en C#. Si el compilador sigue advirtiendo, se puede usar `c.Descripcion!.ToUpper()` **sin** cambiar el SQL ni el resultado (una descripción nula nunca es duplicado).
- **`Select` + `FirstOrDefaultAsync`**: como máximo una fila y solo dos columnas; no se materializa la tabla (RNF-6). Desaparece el `ToListAsync()` de `Categorias` y toda comparación sobre la colección completa.
- **Mensaje sin cambios**: `ConstruirMensajeDuplicado` recibe la descripción **con su caja original** y el `Estado` de la fila encontrada ⇒ duplicado genérico si está activa, mensaje que sugiere reactivar si está inactiva (D-1, RF-3).
- **RNF-6 — contrapartida aceptada**: `UPPER(descripcion)` es no-sargable, así que el motor **no** puede hacer *seek* sobre el índice único y resolverá con un *scan* (de la tabla o del índice). Se acepta explícitamente: el filtrado ocurre en la base y no se transfieren ni se retienen filas en el cliente. Se verifica con `ToQueryString()`/log de EF en la revisión de la tarea.
- **Límite `InMemory`**: los tests cubren la *semántica* (case-insensitive, autoexclusión, mensaje según estado), no el plan de ejecución ni el índice, igual que en §3.

### 7.4 Alternativas descartadas

| # | Decisión | Alternativa descartada | Por qué se descarta |
|---|---|---|---|
| O-1a | La confirmación de baja se compone en `CapaNegocio` y la vista solo la presenta. | **(a) Situación actual**: la vista llama `ContarProductosActivosAsync` y compone el texto con un ternario. | Deja la regla de negocio y el texto en español en la interfaz (viola RF-15 y la regla de AGENTS "nunca lógica de negocio en eventos visuales"), y obliga a la vista a coordinar dos consultas separadas. |
| O-1b | Reutilizar `ResultadoBajaCategoria` en el DTO nuevo. | Enum nuevo `ResultadoConfirmacionBaja` con los mismos valores. | Duplicaría un enum idéntico y obligaría a mapear entre ambos sin que la UI bifurque conductas distintas; el enum existente ya expresa los cuatro casos. |
| O-2a | Comparar en la base con `UPPER(columna) = parámetro normalizado`. | `c.Descripcion == descripcion` apoyado en la colación `CI_AS` de SQL Server. | `InMemory` es case-sensitive: rompería los tests existentes (`bEbIdAs`, autoexclusión) y obligaría a migrar la estrategia de pruebas a SQL Server real; además la unicidad quedaría dependiente de la colación de la BD (no portable). |
| O-2b | `UPPER(...) = parámetro`. | `EF.Functions.Like(c.Descripcion, descripcion)`. | `%`, `_` y `[` en la descripción se interpretan como comodines: "100% Natural" produciría falsos positivos/negativos sin escapado manual; el soporte depende del proveedor. |
| O-2c | Dos consultas en `ObtenerConfirmacionBajaAsync` (categoría + `ContarProductosActivosAsync`). | Una sola consulta con subconsulta de conteo (`_db.Productos.Count(...)` proyectado). | Duplicaría la regla de conteo de RF-6 en vez de reutilizar el método que ya está cubierto por 3 tests; se acepta la consulta extra (§7.6). |

### 7.5 Pruebas

`dotnet test` sobre `CapaNegocio.Tests` (xUnit + `InMemory`). **Total esperado: 237 = 230 actuales + 7 casos nuevos** (6 métodos, uno de ellos `[Theory]` de 2 casos). Los tests existentes de unicidad y de `ContarProductosActivosAsync` **no se modifican**.

| RF / RNF | Qué lo verifica | Verificación concreta |
|---|---|---|
| RF-13 | Mensaje compuesto con y sin productos activos | *Test*: categoría activa con 2 productos activos y 1 inactivo ⇒ `Resultado = Exito`, `Descripcion = "Bebidas"`, `CantidadProductosActivos = 2`, `PuedeDarseDeBaja = true` y `Mensaje` **igual** al literal con la cantidad. *Test*: sin productos activos ⇒ `Cantidad = 0`, `PuedeDarseDeBaja = true` y `Mensaje` **igual** a la confirmación simple. *Test*: solo productos inactivos ⇒ mismo mensaje simple (caso límite). Nunca se escribe: `Estado` de la categoría y conteo de filas quedan intactos (consultar no da de baja). |
| RF-14 | Categoría inexistente / ya inactiva / id inválido | *Test*: id 999 ⇒ `CategoriaNoEncontrada`, `PuedeDarseDeBaja = false`, `Descripcion = null`, `Mensaje` "La categoría indicada no existe." y tabla sin cambios. *Test*: inactiva ⇒ `CategoriaYaInactiva`, `PuedeDarseDeBaja = false`, `Mensaje` "La categoría {x} ya está inactiva.", `Estado` sigue `false`. *Test* `[Theory]` con `0` y `-5` ⇒ `DatosInvalidos`, `PuedeDarseDeBaja = false`, mensaje en español. |
| RF-15 | La vista ya no compone la regla ni el texto | *Inspección de `GestionCategoriasControl.btnDarDebaja_Click`*: **0** llamadas a `ContarProductosActivosAsync`, **0** literales de negocio de la baja (`"producto(s) activo(s)"`, `"¿Confirma que desea dar de baja"`), **1** llamada a `ObtenerConfirmacionBajaAsync`, y el texto mostrado proviene de `Mensaje`. *Validación visual* (usuario): diálogo y aviso con los mismos textos. |
| RF-16 | Unicidad en la base, case-insensitive y con autoexclusión | *Tests existentes sin cambios*: duplicado de activa, duplicado con `bEbIdAs`, duplicado sobre inactiva con mensaje específico y sin reactivar, edición conservando su propia descripción, edición hacia el nombre de otra. *Inspección*: una sola consulta con `FirstOrDefaultAsync` en `ValidarUnicidadDescripcionAsync`. |
| RNF-6 | No se materializa la tabla ni se filtra en el cliente | *Inspección*: **0** `ToListAsync` sobre `Categorias` en el método y **0** comparaciones `string.Equals(..., OrdinalIgnoreCase)` sobre colecciones; `ToQueryString()`/log muestra `TOP(1)` y `UPPER(...)`. |
| RNF-2 | Mensajes en español | Los literales de los *tests* anteriores (texto exacto de cada caso). |

### 7.6 Riesgos

- **Consulta extra en la baja (aceptada)**: el flujo confirmar→bajar pasa de 2 a 3 consultas (categoría de la confirmación + conteo + categoría que ya leía `DarBajaCategoriaAsync`). Se acepta: cada consulta devuelve una fila o un entero, el volumen es mínimo y el beneficio es que la regla y el texto salgan de la UI (RF-15). No se introduce transacción ni se modifica `DarBajaCategoriaAsync`.
- **Carrera conteo/baja (mitigada, no eliminada)**: el mensaje y la posibilidad de baja se resuelven en una sola operación de negocio, pero entre la confirmación y la baja puede cambiar el padrón de productos activos. No se bloquea la baja (decisión original de la spec: siempre advierte, nunca impide) y `DarBajaCategoriaAsync` sigue siendo la fuente de verdad del resultado final. Queda documentado como límite conocido, igual que la concurrencia de altas.
- **`UPPER()` no aprovecha el índice**: cada alta/edición hace un *scan* sobre `CATEGORIA` (RNF-6 lo acepta explícitamente). Con un catálogo de decenas de filas es irrelevante; si creciera mucho habría que evaluar una columna normalizada persistida, opción que hoy queda fuera de alcance.
- **Regresión visual del diálogo**: los textos deben verse idénticos. Los *tests* fijan los literales y la validación visual del usuario confirma el diálogo y el aviso; se conservan el título, el orden de botones y el `MaterialMessageBox` de los resultados.
- **`InMemory` y `ToUpper()`**: si el proveedor dejara de traducir `ToUpper()`, la suite fallaría en rojo en desarrollo (detección temprana) y no en producción. No se cambia la estrategia de pruebas.
