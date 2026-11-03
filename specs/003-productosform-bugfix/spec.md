# Spec 003 — Causa raíz del renderizado de ProductosForm y unificación del botón de estado
Estado: **implementada y aprobada (2026-10-25)**

## Contexto y objetivo

La vista de Productos (`ProductosForm`) **no se ve igual** que la vista de Usuarios (`GestionUsuariosControl`): en la primera carga el campo de búsqueda y los botones aparecen como **bloques blancos**, y tras un re-render (desactivar/reactivar) se ven "medianamente bien" pero la fila de botones parece **montarse** sobre la fila de filtros.

Las correcciones anteriores de esta spec (RF-5, RF-6, RF-13, `PerformLayout()` + `Refresh()` + `Invalidate()` en cascada) atacaron **síntomas** y no la causa. La sonda de diagnóstico sobre WinForms real (144 DPI, fuera del repositorio) confirmó:

- **Causa raíz (H5)**: la librería de tema MaterialSkin 2.3.1 pinta los controles usando el `BackColor` del **padre**. `Color.Transparent` no es "sin color": es `ARGB(0, 255, 255, 255)`, es decir **alfa 0 con RGB blanco**. Al mezclarse con los colores del estado deshabilitado, el resultado es **blanco**. En `ProductosForm` todos los paneles internos tienen `BackColor = Color.Transparent`; en `GestionUsuariosControl` los paneles internos son **opacos** (RGB 50,50,50, el mismo valor del fondo principal del tema).
- **Por qué solo falla Productos**: `GestionUsuariosControl` se agrega a su pestaña **antes** de registrar el formulario principal en el gestor de tema, por lo que la librería normaliza recursivamente el fondo de sus controles. `ProductosForm` se agrega **después**, dentro de un `TabPage`, al abrir la vista en la pestaña, y **nunca** se normaliza.
- **H1 (falta de `AutoScaleDimensions`)**: parcial, **no es causa**.
- **H2 (superposición geométrica / `AutoScroll`)**: **descartada**. La sonda midió `panelFiltros.Bottom = 89` y `panelAcciones.Top = 97`: **no hay solapamiento**. El "montado" visual es el repintado transparente, no el layout.
- **H3 (código muerto)**: **confirmada**. Se invocan `Refresh()`/`PerformLayout()` sobre controles sin handle ni padre (no-op) y hay ~19 invalidaciones por ciclo de repintado, duplicadas con el manejador de carga.
- **H4**: la premisa "Categorías usa un solo botón" es **falsa**: `GestionCategoriasControl` usa dos botones ("Dar de baja" + "Reactivar"); solo `GestionUsuariosControl` usa un botón alternante.

**Objetivo**: eliminar la causa raíz (fondos transparentes → fondos opacos del tema), borrar el código muerto de repintado y **unificar a un solo botón** que alterna "Desactivar" ↔ "Reactivar", igual que Usuarios. **Decisión del usuario ya cerrada**: un botón único, no dos.

## Usuarios / actores
- **Administrador**: gestiona productos y cambia su estado (baja/reactivación).
- **Encargado**: mismas acciones que el Administrador sobre productos.
- **Vendedor**: no accede a la pestaña Productos (no ve acciones de escritura).

## Historias de usuario
- HU-1: Como Administrador/Encargado, al abrir la pestaña Productos por primera vez quiero ver la **misma estética** que la pestaña Usuarios (fondo uniforme, sin bloques blancos ni filas montadas) para poder leer los filtros y las acciones sin confusión.
- HU-2: Como Administrador/Encargado, quiero **un solo botón** que diga "Desactivar" o "Reactivar" según el producto seleccionado, para no dudar cuál de los dos botones me corresponde.

## Definiciones
- **Panel interno**: contenedor (`Panel`, `TableLayoutPanel`, `FlowLayoutPanel`) que aloja controles dentro de la vista.
- **Fondo opaco**: color con alfa 255. En el tema oscuro vigente es RGB(50,50,50), expuesto como `TemaAplicacion.FondoPrincipal`.
- **Fondo transparente**: `Color.Transparent` = `ARGB(0,255,255,255)`; alfa 0 con RGB blanco. Es el origen del blanco.
- **Primera carga**: primer `Show`/`Enter` de la pestaña Productos dentro de una sesión.
- **Re-render**: repintado posterior a una operación (desactivar/reactivar) que recarga el listado.
- **Botón de estado**: único botón de la fila de acciones que alterna "Desactivar"/"Reactivar".

## Requisitos funcionales en EARS

- **RF-1**: EL SISTEMA asigna a **todos** los paneles internos de la vista de Productos (panel raíz, panel de filtros, contenedor de la fila de acciones y panel de acciones) un **color de fondo opaco** igual al fondo principal del tema, en lugar de un fondo transparente.
- **RF-2**: SI algún contenedor de la vista queda con un color de fondo de alfa 0 o con `Color.Transparent`, ENTONCES EL SISTEMA lo considera un defecto: ningún contenedor de la vista de Productos puede usar fondo transparente.
- **RF-3**: CUANDO la pestaña Productos se muestra por primera vez en una sesión, EL SISTEMA presenta el campo de búsqueda, los combos de estado/categoría y los botones sobre el fondo opaco del tema, **sin bloques blancos**.
- **RF-4**: CUANDO la vista se re-renderiza tras desactivar o reactivar un producto, EL SISTEMA mantiene la fila de filtros y la fila de acciones **separadas y sin superposición visual**, con la **misma estética** que en la primera carga.
- **RF-5**: EL SISTEMA ofrece **una sola acción** de cambio de estado en la fila de acciones (un único botón), en lugar de dos botones separados.
- **RF-6**: CUANDO hay un producto seleccionado, EL SISTEMA muestra el texto **"Desactivar"** si el producto está activo y **"Reactivar"** si el producto está inactivo.
- **RF-7**: SI no hay producto seleccionado, ENTONCES EL SISTEMA deshabilita el botón de estado sin lanzar excepción; MIENTRAS el rol de la sesión no pueda gestionar productos, EL SISTEMA mantiene el botón de estado oculto.
- **RF-8**: CUANDO el usuario hace clic en el botón de estado, EL SISTEMA pide confirmación en español y ejecuta la **baja lógica** si el producto seleccionado está activo, o la **reactivación** si está inactivo; al finalizar recarga el listado y recalcula el botón.
- **RF-9**: SI la operación no resulta exitosa (producto ya inactivo/activo, categoría inactiva, sin permiso, no encontrado o fallo de base de datos), ENTONCES EL SISTEMA muestra el mensaje en español de la capa de negocio sin modificar los datos ni perder los filtros ingresados.
- **RF-10**: EL SISTEMA no invoca `Refresh()`, `Invalidate()` ni `PerformLayout()` sobre controles sin handle creado ni padre asignado; se admite como máximo **un** `PerformLayout()` en el momento de carga de la vista, cuando el handle ya existe.
- **RF-11**: EL SISTEMA mantiene la fila de acciones **sin desplazamiento automático**, de modo que no se desplace ni recorte su contenido al re-renderizarse.
- **RF-12**: CUANDO los datos se asignan a la grilla, EL SISTEMA selecciona la primera fila (si hay filas) y recalcula el texto y la habilitación del botón de estado en consecuencia.
- **RF-13**: EL SISTEMA conserva sin cambios el comportamiento funcional del ABM de productos (alta, edición, baja, reactivación, ajuste de stock, filtros, refresco al entrar en la pestaña y gating por rol).

## Requisitos no funcionales
- **RNF-1**: La vista debe quedar renderizada en ≤500 ms desde que se muestra la pestaña, sin parpadeo ni repintados en cascada.
- **RNF-2**: El código de la vista no debe contener llamadas explícitas a `Invalidate()` ni a `Refresh()`, y debe contener **exactamente una** llamada a `PerformLayout()` (en el momento de carga, con handle creado). Verificable por inspección del archivo.
- **RNF-3**: La corrección no agrega dependencias ni paquetes NuGet, no crea migraciones y no modifica el esquema de base de datos.

## Casos límite
- **CL-1**: El listado de productos está vacío: el botón de estado queda deshabilitado sin lanzar excepción.
- **CL-2**: `CurrentRow` es `null` (sin selección): el botón de estado queda deshabilitado y conserva el texto por defecto.
- **CL-3**: Se selecciona un producto inactivo cuya categoría está inactiva: la reactivación se bloquea y se muestra el mensaje de negocio; el producto sigue inactivo y el botón vuelve a "Reactivar".
- **CL-4**: El usuario cancela el diálogo de confirmación: no se escribe nada y el botón recupera su estado habilitado.
- **CL-5**: El rol de la sesión no puede gestionar productos: el botón está oculto y, si se invocara por código, el sistema rechaza la operación con mensaje en español.
- **CL-6**: Fallo o desconexión de base de datos: se informa en español y se conservan los filtros y el listado en pantalla.
- **CL-7**: Escalado de pantalla a 144 DPI: los paneles conservan el fondo opaco y no se superponen; el texto del botón ("Desactivar"/"Reactivar") cambia de ancho sin recortarse.
- **CL-8**: Re-render inmediato tras desactivar/reactivar: el botón alterna su texto en el mismo ciclo, sin bloques blancos ni filas montadas.

## Fuera de alcance
- **`CapaDatos/AppDbContext.cs`**: no se modifica. En particular, la configuración del trigger `TR_PRODUCTO_FECHA_MODIFICACION` **debe permanecer**: sin ella reaparece el error SQL "target table has database triggers" al guardar productos.
- **CapaNegocio**: lógica, validaciones, mensajes y DTOs quedan intactos.
- **CapaEntidad** y **migraciones**: sin cambios.
- **`CapaPresentacion/Inicio/inicio.cs`**: no se modifica (ni el orden de alta de vistas ni el registro del tema).
- **`CapaPresentacion/Usuarios/GestionUsuariosControl.cs` y su `.Designer.cs`**: se usan solo como patrón de referencia.
- **`CapaPresentacion/Categorias/GestionCategoriasControl`**: conserva sus dos botones; no se unifica.
- Cambiar la estética global del tema, agregar controles nuevos o rediseñar la vista.
- Pruebas automatizadas de interfaz: el criterio visual **no es automatizable** con `dotnet test`.

## Criterios de finalización
- `dotnet build` con **0 errores** (se admiten las 26 advertencias preexistentes).
- `dotnet test` en verde: **193/193**, sin pruebas nuevas ni regresiones.
- Inspección del archivo de la vista: **0** ocurrencias de `Color.Transparent`, **0** de `Invalidate(` y de `Refresh(`, **exactamente 1** de `PerformLayout(`, y **un solo** botón de estado.
- **Verificación visual guiada por checklist** (manual, no automatizable):
  1. Primera carga de la pestaña Productos: sin bloques blancos en búsqueda ni en botones.
  2. Fila de filtros (búsqueda + estado + categoría) y fila de botones visiblemente separadas, sin montarse.
  3. Desactivar un producto activo → el botón pasa a "Reactivar"; reactivar → vuelve a "Desactivar"; sin cambios de estética ni superposición.
  4. Estética de Productos igual a la de Usuarios (mismo fondo, mismos controles).
  5. Sin producto seleccionado, el botón está deshabilitado; con rol sin permiso, está oculto.

## Dudas abiertas
- **Cerradas (2026-10-25)**: (a) la causa raíz es el fondo transparente de los paneles internos, no el layout ni el escalado; (b) la superposición geométrica queda descartada por medición; (c) se adopta **un botón único alternante**, igual que Usuarios, descartando los dos botones actuales; (d) el criterio de aceptación visual es manual.

## Historial de cambios (trazabilidad de los RF anteriores)
| RF anterior | Estado | Destino |
|---|---|---|
| RF-1 | modificado | nuevo RF-3 (sin bloques blancos, causa = fondo opaco) |
| RF-2, RF-3, RF-7 | fusionados | nuevo RF-12 (selección inicial + recálculo) |
| RF-4 | modificado | nuevo RF-7 (sin selección ⇒ deshabilitado) |
| RF-5, RF-6, RF-13 | **eliminados** | prescribían `PerformLayout`/`Refresh` masivos: parche, no causa |
| RF-8, RF-9 | reemplazados | nuevos RF-5, RF-6, RF-8 (botón único) |
| RF-10 | movido | criterios de finalización (build/test) + RF-13 |
| RF-11, RF-12 | **eliminados** | la fila ya era `Absolute 56F`; la superposición geométrica no era la causa |
| — | nuevos | RF-1, RF-2 (opacidad), RF-4 (re-render), RF-10 (código muerto), RF-11 (sin autodesplazamiento) |
