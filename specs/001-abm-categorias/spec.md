# Spec 001 — ABM de Categorías/Rubros
Estado: aprobada

> **Cambio de requisito (reencuadre de acceso) — APROBADO por el usuario el 2026-10-19.** La gestión de categorías deja de ser un ítem del menú principal. Las categorías son una **característica del módulo Productos** (útiles para filtrar y para definir un producto) y su ABM se abre **desde la pantalla de Productos** mediante un botón "Gestionar categorías" que lo despliega en una **ventana modal**. El menú principal **no cambia** (no se agrega ni se quita ningún ítem distinto de la pestaña de categorías eliminada). Este reencuadre, además, elimina el fallo de arranque por la clave de imagen duplicada que aportaba la pestaña de categorías. El cambio fue aprobado por el usuario el **2026-10-19**; los RF afectados (RF-10, RF-12 y RNF-1/RNF-5) ya reflejan esta decisión y **el resto de los RF no cambia de semántica**. Decisión registrada como **D-4** en «Dudas abiertas».

## Contexto y objetivo
Los productos del sistema comercial G70 se clasifican por categorías o rubros (registros con una Descripción y un Estado). Las categorías son una **característica del catálogo de productos**: sirven para clasificar, definir y filtrar productos, no como una sección autónoma de la aplicación. Esta funcionalidad permite el ABM completo (alta, modificación, baja lógica y reactivación) para clasificar productos (HU-04), abierto desde la propia pantalla de Productos, garantizando que los combos solo desplieguen categorías vigentes y que una categoría con productos activos no se desactive sin advertirle antes al usuario.

## Usuarios / actores
- **Administrador**: realiza el ABM completo de categorías (alta, modificación, baja y reactivación), al que accede desde la pantalla de Productos mediante el botón "Gestionar categorías".
- **Encargado**: realiza el ABM completo de categorías (alta, modificación, baja y reactivación), al que accede desde la pantalla de Productos mediante el botón "Gestionar categorías".
- **Vendedor**: no gestiona categorías ni ve el botón "Gestionar categorías"; solo utiliza los combos que las despliegan (únicamente activas).
- **Sistema**: valida formato, unicidad y vínculos con productos antes de persistir.

## Historias de usuario
- HU-04: Como Administrador o Encargado, quiero crear y modificar categorías de productos, para clasificar el catálogo del sistema comercial.
- HU-05: Como Administrador o Encargado, quiero dar de baja lógicamente una categoría conociendo antes cuántos productos activos la usan, para evitar dejar productos clasificados en una categoría desactivada sin saberlo.
- HU-06: Como Administrador o Encargado, quiero reactivar una categoría que fue dada de baja, para recuperarla sin tener que crear un duplicado.
- HU-07: Como Vendedor, quiero ver en los combos únicamente categorías activas, para no trabajar con categorías descontinuadas.

## Definiciones
- **Categoría / Rubro**: etiqueta de clasificación de productos, con una `Descripcion` (texto) y un `Estado` (`Estado = true (Activa)` o `Estado = false (Inactiva)`).
- **Baja lógica**: pasar el `Estado` a `false (Inactiva)`. Nunca implica borrado físico del registro.
- **Reactivación**: pasar el `Estado` a `true (Activa)` de una categoría previamente dada de baja.
- **Combos del sistema**: los selectores de categoría presentes en las pantallas del sistema (por ejemplo, alta o edición de productos).
- **Producto activo**: producto cuyo `Estado = true`.
- **Recorte**: quitar de los bordes de la descripción los espacios, tabuladores y saltos de línea; la descripción validada y guardada es siempre la ya recortada.
- **Descripción única**: no puede repetirse entre ninguna categoría, ni siquiera entre las inactivas, ignorando diferencias de mayúsculas y minúsculas ("Bebidas" y "BEBIDAS" se consideran iguales). Si el nombre ya existe en una categoría inactiva, no puede crearse otra con ese nombre: el alta se rechaza con un mensaje específico que indica que esa categoría existe inactiva y que puede reactivarse desde el listado. La reactivación es siempre una acción manual del usuario desde el listado (RF-9), nunca automática (RF-3).

## Requisitos funcionales en EARS (en español)
- RF-1: CUANDO un Administrador o Encargado registra una categoría nueva con una descripción válida y no duplicada, EL SISTEMA la persiste con `Estado = true (Activa)` y la incorpora de inmediato a los combos.
- RF-2: SI la descripción está vacía, contiene solo espacios, tabuladores o saltos de línea, o si una vez aplicado el Recorte tiene menos de 3 o más de 100 caracteres, ENTONCES EL SISTEMA rechaza el guardado y muestra un mensaje en español que explica el motivo. EL SISTEMA aplica siempre el Recorte antes de validar y guarda la descripción ya recortada ("  Bebidas  " se guarda como "Bebidas").
- RF-3: SI ya existe otra categoría con la misma descripción —distinta de la que se está guardando o editando, activa o inactiva—, ignorando diferencias de mayúsculas y minúsculas, ENTONCES EL SISTEMA rechaza el guardado y muestra un mensaje de duplicado en español; cuando la categoría existente está inactiva, el mensaje lo indica expresamente y sugiere reactivarla desde el listado, y EL SISTEMA no la reactiva por su cuenta.
- RF-4: CUANDO un Administrador o Encargado modifica la descripción de una categoría existente, sea activa o inactiva (no es necesario reactivarla antes), EL SISTEMA aplica los mismos criterios de Recorte, formato y unicidad que en el alta —comparando contra las demás categorías, nunca contra la que se está editando— y guarda el cambio.
- RF-5: CUANDO se modifica la descripción de una categoría que tiene productos vinculados, EL SISTEMA muestra la nueva descripción en los listados sin alterar el vínculo con esos productos.
- RF-6: CUANDO un Administrador o Encargado solicita la baja lógica de una categoría, EL SISTEMA determina si existen productos activos vinculados a ella y actúa según el resultado (RF-7 o RF-8).
- RF-7: SI existen productos activos vinculados a la categoría que se quiere dar de baja, ENTONCES EL SISTEMA muestra un diálogo de confirmación en español que informa solo la cantidad de productos activos afectados —no un listado de productos— y ofrece continuar o cancelar; si el usuario confirma, EL SISTEMA pone `Estado = false (Inactiva)`, y si cancela, la categoría queda sin cambios.
- RF-8: SI no existen productos activos vinculados a la categoría, ENTONCES EL SISTEMA muestra una confirmación simple de la baja y, al aceptar, pone `Estado = false (Inactiva)`.
- RF-9: CUANDO un Administrador o Encargado reactiva una categoría inactiva, EL SISTEMA pone `Estado = true (Activa)` y la vuelve a habilitar en los combos.
- RF-10: CUANDO un Administrador o Encargado abre la gestión de categorías desde la pantalla de Productos —a través del botón "Gestionar categorías", que despliega el ABM en una ventana modal—, EL SISTEMA lista todas las categorías, activas e inactivas, indicando su estado, y permite consultarlas, editarlas —también las inactivas, sin necesidad de reactivarlas antes— y reactivarlas. El menú principal de la aplicación no incorpora ningún ítem de categorías y el ABM no es accesible desde él.
- RF-11: EL SISTEMA despliega únicamente categorías con `Estado = true (Activa)` en los combos de categorías. En esta iteración el alcance es el combo de categorías de la pantalla de productos (hoy mockeado con opciones fijas); el resto de combos del sistema, cuando existan, deberán cumplir este mismo requisito en su propia iteración.
- RF-12: SI un usuario con un rol distinto de Administrador o Encargado intenta realizar el ABM de categorías, ENTONCES EL SISTEMA no se lo permite. El punto de acceso es el botón "Gestionar categorías" de la pantalla de Productos: el botón no se ofrece al Vendedor (Administrador/Encargado sí) y, si un rol no autorizado lograra invocar el ABM, EL SISTEMA lo rechaza. La apertura de la ventana modal exige el mismo permiso que el ABM completo (crear, modificar, dar de baja y reactivar).

## Requisitos no funcionales
- RNF-1: El listado de categorías, tanto en la ventana modal de gestión como en los combos, debe responder sin demoras perceptibles para el usuario.
- RNF-2: Todos los mensajes, diálogos y errores del ABM deben estar en español.
- RNF-3: El combo de categorías de la pantalla de productos —único combo en alcance en esta iteración— debe reflejar los cambios del ABM (altas, modificaciones, bajas y reactivaciones) en cuanto se realicen, sin requerir reinicio de la aplicación; los demás combos del sistema heredarán este requisito cuando existan.
- RNF-4: La baja debe ser lógica (`Estado = false (Inactiva)`); está prohibido el borrado físico de la categoría.
- RNF-5: La gestión de categorías no debe incorporar ningún ítem nuevo en el menú principal: se accede exclusivamente desde la pantalla de Productos. La pestaña de categorías que duplicaba el icono de otra sección del menú se elimina, de modo que la aplicación inicia sin el error por claves de imagen repetidas.

## Casos límite
- Dos usuarios crean al mismo tiempo categorías con la misma descripción: el segundo debe recibir el mensaje de duplicado.
- Descripción con solo espacios, tabuladores o saltos de línea, o con espacios, tabuladores o saltos de línea solo en los bordes: se aplica el Recorte antes de validar; si tras el Recorte queda vacía o con menos de 3 caracteres se rechaza, y si entre 3 y 100 caracteres se guarda ya recortada (tildes y ñ se aceptan).
- Dar de baja la única categoría activa: los combos quedarían sin opciones; el sistema no debe fallar.
- Intentar dar de baja una categoría ya inactiva: el sistema no ofrece la acción de baja sobre categorías ya inactivas, por lo que la operación no produce ningún cambio.
- Categoría con productos inactivos pero no activos vinculados: la baja no muestra la advertencia de productos (solo los activos la disparan).
- Fallo o desconexión de la base de datos durante el guardado: el sistema muestra un mensaje en español y no pierde lo que el usuario había ingresado.
- Modificación concurrente de la misma categoría por dos usuarios: el último guardado prevalece sin corromper el registro.
- Intentar reactivar una categoría que ya está activa: el sistema no debe generar un registro duplicado.

## Fuera de alcance
- Borrado físico de categorías.
- Asignación o reasignación masiva de categorías a productos.
- Jerarquía de rubros (subcategorías o categorías padre/hijo).
- Edición masiva de precios por categoría.
- Alta, modificación o baja de productos (spec `002-abm-productos`, ver `specs/000-roadmap.md`).
- Poblar o modificar combos distintos del combo de categorías de la pantalla de productos, y la grilla de productos (siguen mockeados; spec `002-abm-productos` aparte).
- Auditoría o historial de cambios de categorías.

## Criterios de finalización
- RF de negocio verificados con pruebas automáticas en verde (`dotnet test`).
- RF de interfaz verificados con validación visual en la aplicación (alta, modificación, baja con diálogo de advertencia y reactivación, apertura del ABM en modal desde el botón de la pantalla de Productos).
- Compilación limpia del proyecto (`dotnet build`).
- La aplicación inicia sin el crash `ArgumentException: An item with the same key has already been added. Key: products.png` (menú principal sin ítem de categorías).

## Dudas abiertas
- **No quedan dudas abiertas pendientes.** La última duda (el reencuadre de acceso) quedó **cerrada por el usuario el 2026-10-19** y se registra abajo como decisión cerrada **D-4**.
- **D-4 (reencuadre de acceso — aprobado por el usuario el 2026-10-19)**: la gestión de categorías deja de ser un ítem del menú principal: es una característica del módulo Productos. El ABM se abre desde un botón "Gestionar categorías" en la pantalla de Productos, que lo despliega en una ventana modal (listado de gestión + modal de alta/edición + baja/reactivación, reutilizando lo ya construido). Consecuencias confirmadas por el usuario: (a) el menú principal queda igual y no incorpora ningún ítem de categorías; (b) el botón no se ofrece al Vendedor (Administrador/Encargado sí, según RF-12); (c) este reencuadre elimina el fallo de arranque por la clave de imagen duplicada (`products.png`) que aportaba la pestaña de categorías. Afecta RF-10, RF-12 y RNF-1/RNF-5; no altera la semántica del resto de los RF. *(En `plan.md` esta decisión corresponde a las filas D-5/D-7 técnicas; el encabezado y esas filas del plan ya fueron alineados con este reencuadre en la fase de Plan del 2026-10-19 —allí ya no queda ninguna mención de «pendiente de aprobación del usuario»—.)*
- Decisiones cerradas con el usuario durante la entrevista original (siguen vigentes): listado completo con reactivación; descripción única ignorando mayúsculas/minúsculas; el ABM lo realizan Administrador y Encargado (el Vendedor no); la baja nunca se bloquea, sino que advierte con diálogo de confirmación si hay productos activos vinculados; descripción de 3 a 100 caracteres, admitiendo tildes y ñ; el diálogo de baja informa solo la cantidad de productos activos afectados (sin listado de productos); la unicidad de la descripción abarca categorías activas e inactivas (ante un nombre existente inactivo el alta se rechaza con un mensaje específico que distingue ese caso y sugiere reactivarla desde el listado; la reactivación es manual, nunca automática); los espacios extremos (espacios, tabuladores y saltos de línea de los bordes) se recortan antes de validar y la descripción se guarda ya recortada; la descripción se puede editar también en categorías inactivas, sin necesidad de reactivarlas antes.
- Cerradas en la sesión de ajuste posterior a la entrevista: (D-1) duplicado sobre categoría inactiva ⇒ rechazo con mensaje específico que sugiere reactivarla, sin reactivación automática; (D-2) sí migración: se añade la configuración de unicidad/longitud en la base de datos; (D-3) RF-11/RNF-3 se cumplen en esta iteración únicamente con el combo de categorías de la pantalla de productos.
- **[PENDIENTE — fuera de mi permiso de escritura]** Actualización de `MEMORY.md` para el reencuadre de acceso: este agente solo puede escribir dentro de `specs/`, por lo que el cambio de `MEMORY.md` quedó **bloqueado por permisos**. El texto exacto a incorporar figura al final de este archivo (`specs/001-abm-categorias/MEMORY-pendiente.md`) para que lo aplique quien tenga permiso.

## Apéndice — Cambio pendiente en `MEMORY.md` (texto exacto)

> Agente planificador (solo `specs/`): no pudo editar `MEMORY.md` (permiso denegado). Aplicar el siguiente cambio cuando corresponda:

1. En **"Aprendizajes y Errores a Evitar"**, reemplazar el bloque que hoy contiene la regla de no borrar:

```
- **Regla del usuario**: antes de ejecutar cualquier `DELETE` físico (incluso para limpiar filas creadas por error propio), **CONSULTAR AL USUARIO primero**. Nunca borrar por cuenta propia.
- Los comandos `dotnet ef` requieren antes `dotnet tool restore` (manifiesto `.config/dotnet-tools.json`).
```

por:

```
- **Regla del usuario**: antes de ejecutar cualquier `DELETE` físico (incluso para limpiar filas creadas por error propio), **CONSULTAR AL USUARIO primero**. Nunca borrar por cuenta propia.
- **Regla del usuario**: cualquier modificación significativa de UI (menú, estructura de pantallas, agregar/quitar ítems) se **CONSULTA ANTES**. La interfaz estática ya está definida.
- Los comandos `dotnet ef` requieren antes `dotnet tool restore` (manifiesto `.config/dotnet-tools.json`).
- **`ImageList` de un `TabControl`: dos `TabPage` con la misma `ImageKey`** lanzan `ArgumentException: An item with the same key has already been added. Key: products.png` al mostrar el menú. Ocurrió en `inicio` (la pestaña `tab_categorias` duplicaba la clave de `tab_productos`); se corrigió **eliminando la pestaña** (reencuadre: categorías como característica de Productos). Regla: cada `TabPage` con ícono lleva clave de imagen única.
```

2. Anotar el reencuadre (categorías desde Productos; sin ítem de menú) y el crash corregido en "Estado Actual"/"Próximos Pasos" al aprobarse el cambio.
