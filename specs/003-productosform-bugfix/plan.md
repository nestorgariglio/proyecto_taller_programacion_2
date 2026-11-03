# Plan 003 — Corrección por causa raíz en ProductosForm (fondo opaco + botón único)

## 0. Alcance del cambio
**Un solo archivo**: `CapaPresentacion/Productos/ProductosForm.cs`.
No se toca ningún otro archivo de la solución (ni `ProductosForm.Designer.cs`, que solo contiene `InitializeComponent` vacío).

## 1. Responsabilidades por Capa

### CapaEntidad
- **Sin cambios**. `ProductoRespuestaDto` ya expone `Estado` (bool) y es suficiente.

### CapaDatos
- **Sin cambios**. `AppDbContext` (Fluent API, trigger `TR_PRODUCTO_FECHA_MODIFICACION` en `CapaDatos/AppDbContext.cs:70`) queda intacto. Quitarlo provoca el error SQL de "target table has database triggers".

### CapaNegocio
- **Sin cambios**. `DarBajaProductoAsync` / `ReactivarProductoAsync` y sus enums de resultado ya existen y se reutilizan tal cual.

### CapaPresentacion
- **`CapaPresentacion/Productos/ProductosForm.cs`**, en `ArmarInterfazMaterial()` y en los miembros de la clase:
  1. Fondos **opacos** en los 4 paneles internos (causa raíz).
  2. **Un solo** botón de estado (`_btnEstado`) con un único handler ramificado.
  3. **Eliminación** de todo el repintado manual (código muerto).
  4. `_panelAcciones.AutoScroll = false`.
- **`CapaPresentacion/Inicio/inicio.cs`**: solo lectura. Se documenta por qué Usuarios sí se ve bien (ver 2.1) pero **no se modifica**.

## 2. Estrategia técnica

### 2.1 Fondos opacos (RF-1, RF-2, RF-3, RF-4)
**Valor exacto a usar**: `BackColor = TemaAplicacion.FondoPrincipal` (RGB 50,50,50 en el tema oscuro vigente, `CapaPresentacion/Tema/TemaAplicacion.cs:29`).

| Panel | Línea actual | Valor actual | Valor nuevo |
|---|---|---|---|
| `_panelRaiz` | `ProductosForm.cs:719` | `Color.Transparent` | `TemaAplicacion.FondoPrincipal` |
| `_panelFiltros` | `ProductosForm.cs:745` | `Color.Transparent` | `TemaAplicacion.FondoPrincipal` |
| `_panelAcciones` (creación) | `ProductosForm.cs:819` | `Color.Transparent` | `TemaAplicacion.FondoPrincipal` |
| `_panelAcciones` (reconfiguración) | `ProductosForm.cs:938` | `Color.Transparent` | `TemaAplicacion.FondoPrincipal` |

`_panelPrincipal` ya está opaco (`ProductosForm.cs:709`) y el propio `UserControl` también (`ProductosForm.cs:55`).

**Por qué ese valor y no otro**: es exactamente el fondo que muestra Usuarios. `GestionUsuariosControl` se agrega a la pestaña en `inicio.cs:55`, **antes** de `materialSkinManager.AddFormToManage(this)` (`inicio.cs:64`); al registrarse el formulario, la librería normaliza recursivamente el `BackColor` de los controles ya enlazados, dejando los paneles en RGB 50,50,50. `ProductosForm` se agrega **después**, dentro del `TabPage`, vía `inicio.cs:90` → `AbrirVistaEnTab` (`inicio.cs:99-107`), y nunca pasa por esa normalización.

**Mecanismo del defecto**: la librería pinta con el `BackColor` del padre; `Color.Transparent` = `ARGB(0,255,255,255)` (alfa 0, RGB blanco). Al mezclarse con los colores del estado deshabilitado el resultado es **blanco** → los "bloques blancos" de la primera carga y el "montado" tras el re-render.

**Alternativas descartadas**:
- *Dejar `BackColor` sin asignar*: el valor por defecto de WinForms (`SystemColors.Control`, gris claro) rompe el tema oscuro.
- *Activar la normalización global del gestor de tema*: afecta a todas las vistas y queda fuera de alcance (riesgo de regresión en `inicio.cs`).
- *Repintar más* (`Refresh()`/`Invalidate()`): ya se probó y es lo que falló; no cambia el color con el que se pinta.

### 2.2 Botón único de estado (RF-5 a RF-9, RF-12)

**Inventario real de referencias (verificado en el archivo, no histórico)**:

| # | Ubicación | Referencia | Acción |
|---|---|---|---|
| 1 | `ProductosForm.cs:27` | `private MaterialButton _btnDesactivar = null!;` | renombrar a `_btnEstado` |
| 2 | `ProductosForm.cs:28` | `private MaterialButton _btnReactivar = null!;` | **eliminar** |
| 3 | `ProductosForm.cs:60-62` | `_txtBuscar.Refresh(); _btnDesactivar.Refresh(); _btnReactivar.Refresh();` (handler `Load`) | **eliminar** (ver 2.3) |
| 4 | `ProductosForm.cs:89-90` | `_btnDesactivar.Invalidate(); _btnReactivar.Invalidate();` (constructor) | **eliminar** |
| 5 | `ProductosForm.cs:232-233` | `_btnDesactivar.Refresh(); _btnReactivar.Refresh();` (fin de `CargarProductosAsync`) | **eliminar** |
| 6 | `ProductosForm.cs:322-323` | `_btnDesactivar.Visible = ...; _btnReactivar.Visible = ...;` (`AplicarRolUsuario`) | una sola línea `_btnEstado.Visible = puedeGestionarProductos;` |
| 7 | `ProductosForm.cs:457-530` | `btnDesactivar_Click` (incluye `_btnDesactivar.Enabled = false;` en `:483`) | renombrar a `btnEstado_Click` y ramificar |
| 8 | `ProductosForm.cs:532-604` | `btnReactivar_Click` (incluye `_btnReactivar.Enabled = false;` en `:558`) | **eliminar** (se fusiona en el 7) |
| 9 | `ProductosForm.cs:667-674` | comentario XML de `ActualizarEstadoBotones()` ("Desactivar solo sobre activos y Reactivar solo sobre inactivos") | reescribir |
| 10 | `ProductosForm.cs:687-688` | `_btnDesactivar.Enabled = false; _btnReactivar.Enabled = false;` (rama `producto is null`) | una sola línea `_btnEstado.Enabled = false;` |
| 11 | `ProductosForm.cs:694-695` | `_btnDesactivar.Enabled = ... && Estado == true; _btnReactivar.Enabled = ... && Estado == false;` | `_btnEstado.Enabled = puedeGestionarProductos;` + `_btnEstado.Text = producto.Estado ? "Desactivar" : "Reactivar";` |
| 12 | `ProductosForm.cs:838-851` | bloque `MaterialButton btnDesactivar` + `_btnDesactivar.Click += btnDesactivar_Click;` | bloque `MaterialButton btnEstado` + `_btnEstado.Click += btnEstado_Click;` |
| 13 | `ProductosForm.cs:853-868` | bloque `MaterialButton btnReactivar` + `_btnReactivar.Click += btnReactivar_Click;` | **eliminar** |
| 14 | `ProductosForm.cs:926-927` | `_panelAcciones.Controls.Add(btnDesactivar); ...Add(btnReactivar);` | un solo `Add(btnEstado);` |

**Patrón de referencia**: `GestionUsuariosControl.ActualizarEstadoBotones()` (`CapaPresentacion/Usuarios/GestionUsuariosControl.cs:167-173`) calcula `Text` y `Enabled` en un solo lugar, y `btnDesactivar_Click` (`:175-247`) ramifica con `if (usuario.Estado)`. Se replica ese patrón, conservando las particularidades de Productos:
- Texto según estado: `"Desactivar"` si `producto.Estado == true`, `"Reactivar"` si `false` (Usuarios usa "Activar"; Productos conserva "Reactivar" porque es el término ya usado en la vista y en los mensajes de negocio).
- Se mantienen los `MaterialDialog` de confirmación con los títulos y textos actuales ("Dar de baja producto" / "Reactivar producto").
- Se mantienen los `MaterialMessageBox` con el `Mensaje` de negocio y los títulos actuales ("Producto dado de baja" / "Producto reactivado", "Atención", "Error SQL").
- Se mantiene la recarga final (`await CargarProductosAsync()`) y el `ActualizarEstadoBotones()` del `finally`.
- Se mantiene el guard de rol al inicio del handler y el `_btnEstado.Enabled = false;` antes de la escritura.

**Estructura del handler único**:
1. Guard de permiso (`PoliticaAcceso.PuedeGestionarProductos(_rolActual)`) → mensaje y `return`.
2. Lectura de `CurrentRow?.DataBoundItem`; si es `null` → mensaje "Debe seleccionar un producto del listado…" y `return`.
3. `FindForm()`; si no hay form → `return`.
4. `bool estaActivo = producto.Estado;` y texto/título del diálogo según `estaActivo`.
5. `MaterialDialog` de confirmación; cancelar → `return`.
6. `_btnEstado.Enabled = false;`
7. `try`: rama `estaActivo` → `DarBajaProductoAsync`; rama contraria → `ReactivarProductoAsync`; si el resultado no es `Exito` → mensaje de negocio y `return`; si es `Exito` → mensaje y `await CargarProductosAsync()`.
8. `catch` → mensaje "No se pudo cambiar el estado del producto" con el detalle; `finally` → `ActualizarEstadoBotones()`.

**Alternativas descartadas**:
- *Mantener dos botones alternando `Visible`*: duplica handlers y controles, y el usuario ya decidió un botón único.
- *Conservar el nombre `_btnDesactivar` para el botón único*: el nombre mentiría cuando la acción es reactivar.

### 2.3 Eliminación del código muerto (RF-10, RNF-2)

| Zona | Líneas | Qué se elimina | Motivo |
|---|---|---|---|
| Constructor, handler `Load` | `ProductosForm.cs:60-62` | `Refresh()` de `_txtBuscar`, `_btnDesactivar`, `_btnReactivar` | parche; el repintado lo provoca el propio WinForms |
| Constructor, handler `Load` | `ProductosForm.cs:64-77` | `Refresh()` de `_panelPrincipal`, `_panelFiltros`, `_panelAcciones` | parche |
| Constructor, handler `Load` | `ProductosForm.cs:81` | **se conserva** como el único `PerformLayout()` válido | en `Load` el handle ya existe |
| Constructor | `ProductosForm.cs:88-90` | 3 `Invalidate()` | código muerto (sin handle/parent al construir) |
| `CargarProductosAsync` | `ProductosForm.cs:232-233` | 2 `Refresh()` | parche; el cambio de `Text`/`Enabled` ya invalida |
| `ArmarInterfazMaterial` | `ProductosForm.cs:1023` | `_panelRaiz.PerformLayout()` | sin padre todavía: no-op |
| `ArmarInterfazMaterial` | `ProductosForm.cs:1026-1030` | 5 `Refresh()` | sin handle: no-op |
| `ArmarInterfazMaterial` | `ProductosForm.cs:1035` | `_panelPrincipal.PerformLayout()` | sin handle: no-op |
| `ArmarInterfazMaterial` | `ProductosForm.cs:1036-1038` | 3 `Refresh()` | sin handle: no-op |

Resultado: **0** `Invalidate(`, **0** `Refresh(`, **1** `PerformLayout(` (`ProductosForm.cs:81`, dentro del handler `Load`). El handler `Load` se reduce a una comprobación de `_panelRaiz != null` + `PerformLayout()`.

**Alternativa descartada**: mover el `PerformLayout()` a `OnHandleCreated`/`Shown`. `Load` ya cumple (handle creado y árbol de controles armado) y no agrega código.

### 2.4 Fila de acciones sin autodesplazamiento (RF-11)
- `ProductosForm.cs:817` y `ProductosForm.cs:936`: `AutoScroll = true` → `AutoScroll = false`.
- Se conserva `WrapContents = false`, `Dock = Fill`, `Height` fijo por `RowStyles[2] = Absolute 56F` (`ProductosForm.cs:726`).
- **Nota**: el bloque `ProductosForm.cs:932-939` repite la configuración de `:812-821`. Se mantiene (no es código muerto, solo duplicado) para no ampliar el alcance, pero **ambos** deben quedar con el mismo valor.

## 3. DTOs y Mapeos
- **Sin cambios**. Se sigue usando `ProductoRespuestaDto` (solo lectura de `Estado`, `IdProducto`, `Nombre`) y `FiltroProductosDto`.

## 4. Persistencia
- **No aplica**: no hay cambios de esquema, ni de Fluent API, ni migraciones. `AppDbContext.cs:70` (`HasTrigger`) se conserva.

## 5. Decisiones técnicas (resumen)

| Decisión | Alternativa descartada | Motivo |
|---|---|---|
| Fondo opaco `TemaAplicacion.FondoPrincipal` en los 4 paneles | `Color.Transparent`; no asignar `BackColor`; normalización global | elimina la causa raíz y replica el valor que ya usa Usuarios |
| Un botón `_btnEstado` con `Text`/`Enabled` calculados en `ActualizarEstadoBotones()` | dos botones alternando `Visible` | patrón ya probado en Usuarios; menos controles y un solo handler |
| Un handler `btnEstado_Click` ramificado por `producto.Estado` | dos handlers separados | evita duplicar guard, confirmación y manejo de errores |
| Borrar todo el repintado manual | mantener `Refresh()`/`Invalidate()` "por si acaso" | eran no-ops o parches; RNF-2 verificable |
| `AutoScroll = false` en la fila de acciones | dejar `AutoScroll = true` | decisión del usuario; la fila tiene altura fija y los botones entran a 1077 px |

## 6. Riesgos
- **Diseñador de Visual Studio**: `ProductosForm` **no** se puede abrir en el diseñador (su constructor recibe dependencias, `ProductosForm.cs:47`, sin constructor sin parámetros). Todo cambio va en `ArmarInterfazMaterial()`; `ProductosForm.Designer.cs` se mantiene a mano y **no se modifica**.
- **`AppDbContext` intocable**: quitar `HasTrigger("TR_PRODUCTO_FECHA_MODIFICACION")` (`CapaDatos/AppDbContext.cs:70`) reintroduce el error SQL de tabla con triggers al guardar productos.
- **Ancho del botón**: `MaterialButton` con `AutoSizeMode.GrowAndShrink` recalcula su ancho al cambiar `Text` ("Desactivar" ≈112 px, "Reactivar" ≈110 px). Con `AutoScroll = false`, si la ventana es muy angosta el último botón podría recortarse; a 1077 px los 6 botones entran. Comportamiento aceptado; se verifica en el checklist.
- **Compilación intermedia**: la unificación del botón debe hacerse en **una sola pasada** (campos, construcción, `ActualizarEstadoBotones`, `AplicarRolUsuario` y `Controls.Add`); si se parte en dos, queda una compilación roja entre tareas.
- **Regresión funcional**: el riesgo real es perder la rama de reactivación al fusionar handlers. Se cubre con la verificación del checklist (punto 3) y con `dotnet test` 193/193 (aunque los tests no cubren UI).

## 7. Estrategia de tests
- **No se agregan pruebas**: el proyecto de pruebas cubre `CapaNegocio` y el defecto es exclusivamente de presentación (fondo de pintado). Escribir un test de UI requeriría infraestructura nueva, fuera de alcance.
- **Verificación automática disponible**:
  1. `dotnet build` → 0 errores (26 advertencias preexistentes).
  2. `dotnet test` → 193/193.
  3. Inspección por búsqueda de texto en `ProductosForm.cs`: `Color.Transparent` = 0; `Invalidate(` = 0; `Refresh(` = 0; `PerformLayout(` = 1; `_btnReactivar` = 0; `btnReactivar_Click` = 0; `AutoScroll = true` = 0.
- **Verificación manual (obligatoria, no automatizable)**: checklist de 5 puntos de los criterios de finalización de la spec (primera carga sin blancos, filas separadas, alternancia del botón, estética igual a Usuarios, habilitación/ocultamiento por selección y rol).

## 8. Trazabilidad

| RF | Componente del plan |
|---|---|
| RF-1 | 2.1 (tabla de 4 paneles) |
| RF-2 | 2.1 + verificación por inspección |
| RF-3 | 2.1 + 2.3 (sin repintados que oculten el defecto) |
| RF-4 | 2.1 + 2.4 |
| RF-5 | 2.2 (inventario 1, 2, 12, 13, 14) |
| RF-6 | 2.2 (inventario 11) |
| RF-7 | 2.2 (inventario 6, 10, 11) |
| RF-8 | 2.2 (inventario 7, 8) |
| RF-9 | 2.2 (mensajes de negocio conservados) |
| RF-10 | 2.3 (tabla de limpieza) |
| RF-11 | 2.4 |
| RF-12 | 2.2 (inventario 10, 11) — la selección inicial ya existe en `ProductosForm.cs:226-230` y no se modifica |
| RF-13 | 1 (sin cambios en el resto de las capas) + 7 |
| RNF-2 | 2.3 (resultado: 0 `Invalidate`, 0 `Refresh`, 1 `PerformLayout`) |

## 9. Orden de ejecución
1. Fondos opacos (2.1).
2. `AutoScroll = false` (2.4).
3. Limpieza de código muerto (2.3).
4. Unificación del botón (2.2), en una sola pasada.
5. Verificación técnica (build + test + inspección).
6. Verificación visual por checklist.

Cada paso se valida con `dotnet build` antes de pasar al siguiente; los pasos 1–4 no cambian comportamiento funcional, solo presentación.
