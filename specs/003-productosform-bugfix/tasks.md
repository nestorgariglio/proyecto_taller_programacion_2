# Tareas 003 — Causa raíz en ProductosForm (fondo opaco + botón único)
Estado: **completada (2026-10-25)**

Archivo único a modificar: `CapaPresentacion/Productos/ProductosForm.cs`.

## Reglas de finalización (aplican a todas las tareas)
1. `dotnet build` → **0 errores** (se admiten las 26 advertencias preexistentes).
2. `dotnet test` → **193/193**, sin pruebas nuevas ni regresiones.
3. Inspección final de `ProductosForm.cs`: `Color.Transparent` = 0; `Invalidate(` = 0; `Refresh(` = 0; `PerformLayout(` = 1; `_btnReactivar` = 0; `btnReactivar_Click` = 0; `AutoScroll = true` = 0.
4. Verificación visual por checklist (T-13).

## Tareas nuevas (pendientes, en orden de dependencia)

- [x] **T-08. Fondos opacos en los paneles internos.** RF-1, RF-2, RF-3, RF-4
  - Zona: `ArmarInterfazMaterial()` — `ProductosForm.cs:719` (`_panelRaiz`), `:745` (`_panelFiltros`), `:819` (`_panelAcciones`, creación) y `:938` (`_panelAcciones`, reconfiguración).
  - Cambio: `BackColor = Color.Transparent` → `BackColor = TemaAplicacion.FondoPrincipal` en las 4 líneas. No tocar `:709` ni `:55` (ya son opacos).
  - Hecho cuando: la búsqueda de `Color.Transparent` en `ProductosForm.cs` devuelve **0** coincidencias y `dotnet build` compila con 0 errores.

- [x] **T-09. Quitar el autodesplazamiento de la fila de acciones.** RF-11
  - Zona: `ProductosForm.cs:817` (creación de `_panelAcciones`) y `:936` (reconfiguración).
  - Cambio: `AutoScroll = true` → `AutoScroll = false` en ambas. Conservar `WrapContents = false` y `Dock = Fill`.
  - Hecho cuando: `AutoScroll = true` devuelve 0 coincidencias, `AutoScroll = false` devuelve 2, y `dotnet build` compila con 0 errores.

- [x] **T-10. Eliminar el código muerto de repintado.** RF-10, RNF-2
  - Zona y líneas exactas (verificadas):
    - Constructor, handler `Load`: quitar `Refresh()` de `:60-62` y de `:64-77`; **conservar** `_panelRaiz.PerformLayout()` de `:81`.
    - Constructor: quitar los 3 `Invalidate()` de `:88-90`.
    - `CargarProductosAsync()`: quitar los 2 `Refresh()` de `:232-233`.
    - `ArmarInterfazMaterial()`: quitar `_panelRaiz.PerformLayout()` de `:1023`, los 5 `Refresh()` de `:1026-1030`, `_panelPrincipal.PerformLayout()` de `:1035` y los 3 `Refresh()` de `:1036-1038`.
  - Hecho cuando: `Invalidate(` = 0, `Refresh(` = 0, `PerformLayout(` = 1 (el de `:81`) y `dotnet build` compila con 0 errores.

- [x] **T-11. Unificar los dos botones en un solo botón de estado.** RF-5, RF-6, RF-7, RF-8, RF-9, RF-12
  - **Debe hacerse en una sola pasada** para no dejar la compilación roja. Cambios:
    1. `:27-28`: dejar un solo campo `private MaterialButton _btnEstado = null!;` (eliminar `_btnReactivar`).
    2. `:838-851`: renombrar el bloque a `btnEstado` con `Text = "Desactivar"`, `Visible = false`, y `_btnEstado.Click += btnEstado_Click;`.
    3. `:853-868`: eliminar el bloque completo de `btnReactivar`.
    4. `:926-927`: dejar un único `_panelAcciones.Controls.Add(btnEstado);`.
    5. `:322-323`: una sola línea `_btnEstado.Visible = puedeGestionarProductos;`.
    6. `:667-674`: reescribir el comentario XML de `ActualizarEstadoBotones()` (un solo botón: "Desactivar" si activo, "Reactivar" si inactivo, deshabilitado sin selección).
    7. `:687-688`: una sola línea `_btnEstado.Enabled = false;`.
    8. `:694-695`: `_btnEstado.Enabled = puedeGestionarProductos;` y `_btnEstado.Text = producto.Estado ? "Desactivar" : "Reactivar";`.
    9. `:457-530`: renombrar `btnDesactivar_Click` a `btnEstado_Click` y ramificar según `producto.Estado` (ver plan §2.2): guard de rol, validación de selección, `FindForm()`, confirmación con `MaterialDialog` ("Dar de baja producto" / "Reactivar producto"), `_btnEstado.Enabled = false;`, `try` con `DarBajaProductoAsync` o `ReactivarProductoAsync`, mensaje de negocio si no es `Exito`, `MaterialMessageBox` de éxito, `await CargarProductosAsync()`, `catch` con mensaje en español y `finally` con `ActualizarEstadoBotones()`.
    10. `:532-604`: eliminar `btnReactivar_Click` (queda fusionado en el punto 9).
  - Hecho cuando: `_btnReactivar` = 0, `btnReactivar_Click` = 0, hay **un solo** `MaterialButton` con `Text = "Desactivar"` en el archivo, y `dotnet build` compila con 0 errores.

- [x] **T-12. Verificación técnica automática.** RF-13, RNF-2
  - Acciones: `dotnet build` y `dotnet test`; ejecutar la inspección de las Reglas de finalización (punto 3).
  - Hecho cuando: build con 0 errores, tests 193/193 y las 7 búsquedas de inspección dan los valores esperados.

- [x] **T-13. Verificación visual manual por checklist (sin cambios de código).** RF-1 a RF-13
  - Acciones: abrir la aplicación con un usuario Administrador, entrar en la pestaña Productos y recorrer el checklist.
  - Hecho cuando el usuario confirma los 5 puntos:
    1. Primera carga: búsqueda, combos y botones sin bloques blancos.
    2. Fila de filtros y fila de botones visiblemente separadas (sin montarse).
    3. Desactivar → el botón pasa a "Reactivar" y la vista no cambia de estética; reactivar → vuelve a "Desactivar".
    4. Estética de Productos igual a la de Usuarios.
    5. Sin selección el botón está deshabilitado; con rol sin permiso está oculto.

## Dependencias
- T-08 → T-12, T-13
- T-09 → T-12, T-13
- T-10 → T-11 (T-11 toca líneas vecinas a las que T-10 elimina; hacerlas en este orden evita conflictos de numeración)
- T-11 → T-12 → T-13

## Historial (tareas anteriores de esta spec — cerradas y sustituidas)
> Se conservan como registro. Ninguna atacó la causa raíz (fondos transparentes); todas fueron parches de repintado o de layout.
- ~~T-01~~ Fila 1 de `panelRaiz` a `Absolute 56F` — ya está aplicada en `ProductosForm.cs:725`; no era la causa (H2 descartada por medición).
- ~~T-02~~ `panelFiltros` con altura fija 56F — ya presente; sin efecto sobre el defecto.
- ~~T-03~~ `PerformLayout()` + `Refresh()` tras ensamblar la vista — parche eliminado por T-10.
- ~~T-04~~ Verificar `Height`/`WrapContents`/`AutoScroll` de `panelAcciones` — el `AutoScroll = true` que dejó se corrige en T-09.
- ~~T-05~~ Build + test 193/193 — se repite como T-12 con la verificación de inspección.
- ~~T-06/T-07~~ (correcciones A/B/C: `Invalidate()` en constructor, `Refresh()` tras `DataSource`, defensiva en `ActualizarEstadoBotones()`) — los repintados se eliminan en T-10; la defensiva se conserva en T-11.

## Notas
- No se toca `CapaEntidad`, `CapaDatos`, `CapaNegocio` ni `inicio.cs`; la lógica de negocio queda intacta.
- No se crean migraciones ni se instalan paquetes.
- `ProductosForm.Designer.cs` no se modifica (el diseñador de VS no puede abrir esta vista).
