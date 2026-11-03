# Spec 004 — Consulta de Stock e Inventario Crítico

Estado: aprobada (2026-10-26)

> **Origen**: petición del usuario (HU-06 del tablero externo, 2026-10-26) con decisiones cerradas por él y registradas abajo como **D-1 … D-15**. Las 5 dudas de la primera redacción quedaron resueltas en **D-5 … D-9**.
> **Revisión QA (2026-10-26)**: se aplicaron las 23 observaciones del revisor. Las decisiones de conducta añadidas después de esa revisión (**D-10 … D-15**) prevalecen sobre cualquier redacción anterior; no queda ninguna duda abierta ni redacción que asocie la ausencia del parámetro con el modo degradado.
> **Dependencia**: specs `001-abm-categorias` y `002-abm-productos` (**aprobadas e implementadas**). De ellas se heredan el catálogo real, el combo de categorías activas, el filtro por estado (Todos/Activos/Inactivos), la búsqueda y las columnas actuales. La spec `003-productosform-bugfix` (**cerrada**) aporta los requisitos no funcionales de renderizado, que se incorporan literalmente y no se contradicen.
> **Regla del proyecto**: esta spec describe **solo el QUÉ y el POR QUÉ**. Los detalles técnicos (archivo y clave de configuración, nombres de controles, forma de aplicar colores, forma de obtener el umbral) corresponden al `plan.md`.
> **Nota de numeración de la spec**: el `000-roadmap.md` reservaba el 003 para Clientes y el 004 para Proveedores, pero la numeración real del repositorio ya difiere (el 003 lo ocupó el bugfix de la pantalla de Productos). Por eso esta funcionalidad se documenta como **004**, sin modificar el roadmap.

## Contexto y objetivo

El sistema ya administra un catálogo real de productos con stock y precios, pero hoy no ofrece una forma pensada para **vigilar la disponibilidad de mercadería**: la pantalla de Productos se usa para el alta, la edición, la baja y el ajuste de stock, y su listado, aunque muestra el stock, no permite reconocer de un vistazo qué está agotado o por debajo de un nivel mínimo, ni acotar el listado a esos casos. En un comercio con POS esto provoca quiebres de stock detectados tarde y reposición reactiva. La funcionalidad agrega una **consulta de stock e inventario crítico** sobre el listado de productos ya existente: búsqueda por código de barras o nombre, filtrado por categoría combinable con el estado, columnas de Código, Nombre, Categoría, Stock y PrecioVenta, **resaltado visual** de los productos agotados o por debajo de un umbral mínimo **global**, y un control **"Solo críticos"**. El objetivo es que quien atiende el mostrador o repone mercadería detecte los faltantes en el momento, sin cambiar el esquema de datos ni el comportamiento del ABM ya existente. El umbral se toma de un parámetro global de configuración de la aplicación y, si no está definido, el sistema asume **5 unidades**; un valor **definido pero inválido** degrada el criterio de forma segura y avisada.

## Usuarios / actores

- **Administrador**: consulta el stock completo, busca, filtra por categoría y estado, y usa "Solo críticos" para decidir la reposición.
- **Encargado**: mismo alcance de consulta que el Administrador; es el rol operativo natural para la reposición diaria.
- **Vendedor**: **no** accede a esta consulta en esta iteración, porque vive en la pestaña Productos y ese módulo no está habilitado para su rol (D-3). La consulta de catálogo y stock desde el módulo de Ventas **quedará cubierta por la spec 005, aún no implementada**; esta spec no la crea.
- **Sistema**: aplica la búsqueda, los filtros y el criterio de criticidad; resuelve el umbral con el valor asumido de 5 cuando el parámetro no está definido y degrada el criterio **solo** cuando el parámetro está definido con un valor inválido.

## Historias de usuario

- HU-15: Como Administrador o Encargado, quiero buscar productos por código de barras o por nombre y filtrarlos por categoría y estado, para encontrar en segundos el producto que me consultan en el mostrador.
- HU-16: Como Administrador o Encargado, quiero ver de un vistazo qué productos están agotados y cuáles quedaron por debajo de un nivel mínimo, para reponer antes de perder una venta.
- HU-17: Como Administrador o Encargado, quiero poder acotar el listado a los productos críticos, para armar la reposición sin revisar el catálogo completo.

> **Mapeo de trazabilidad**: **HU-06 del tablero externo → HU-15, HU-16 y HU-17 de esta spec**. La numeración de HU **continúa la secuencia del repositorio** (las specs 001 y 002 usaron HU-01 … HU-14); por eso la **HU-06 de la spec 001 es otra historia** y no debe confundirse con este pedido.

## Definiciones

- **Consulta de stock**: vista de **solo lectura** del catálogo que muestra la disponibilidad y el precio de venta. No crea, no modifica y no elimina productos ni stock.
- **Código de barras**: el Código del producto (texto numérico de 8 a 13 dígitos, único). La búsqueda lo trata como **texto**, preservando los ceros a la izquierda.
- **Stock actual**: cantidad disponible del producto. En datos válidos nunca es negativo, pero **si apareciera un valor negativo se trata como agotado** (D-11). Hoy el stock cambia solo por el **alta** (que inicia en 0) y por el **ajuste manual**; los movimientos por Ventas y Compras aún no están implementados (specs 005 y 006).
- **Umbral mínimo de stock (umbral)**: valor de corte **global** —el mismo para todos los productos— que define cuándo una existencia se considera baja. Es un **número entero mayor o igual a 0** (D-9), administrado por un **parámetro global de configuración de la aplicación**; si el parámetro **no está definido**, el sistema **asume 5 unidades** (D-10). **No** es un dato del producto: no existe ni existirá un campo de stock mínimo por producto. Se lee **al abrir la vista** y se mantiene durante esa sesión de vista (D-15).
- **Umbral en modo degradado**: situación en la que el parámetro **está definido con un valor inválido** (D-10/D-15). No se aplica ningún umbral: solo el stock agotado es crítico y el texto informativo no muestra número.
- **Producto crítico**: producto **agotado** (stock 0 o negativo) **o** producto con stock **mayor que 0 y estrictamente menor** que el umbral (D-4/D-11). El stock igual al umbral **no** es crítico.
- **Agotado**: producto con stock **0 o negativo**. Se resalta en **rojo** con el indicador **"SIN STOCK"** (D-7/D-11).
- **Bajo el umbral**: producto con stock **mayor que 0 y menor que el umbral**. Se resalta en **ámbar/naranja** con el indicador **"BAJO"** (D-7).
- **Resaltado visual**: pintado de la **fila completa** con el color del caso **más** el texto indicador en la celda de Stock; no reemplaza al número: la cantidad de stock siempre se muestra (D-7). Cuando la fila resaltada está además **seleccionada**, el color de criticidad se conserva y la selección se distingue por un **borde de 2 px**, **sin cambiar el color del texto** (que sigue blanco), no por relleno (obs. 3/RF-22/D-18).
- **Texto indicador**: leyenda corta en español dentro de la celda de Stock ("SIN STOCK" o "BAJO") que hace que la distinción no dependa solo del color (D-7).
- **Atenuación**: para productos **inactivos**, la **misma familia de color** de criticidad con **menor intensidad**, de modo que el estado se distinga pero el aviso no se oculte y la cantidad y el texto indicador sigan siendo legibles, en color y en escala de grises (D-8, obs. 20).
- **Coincidencia exacta de código**: situación en la que el texto buscado, ya recortado, es **idéntico** al Código de un producto (D-12). La unicidad del Código vigente hace que solo pueda corresponder a un producto.
- **Filtro "Solo críticos"**: control de activación que, encendido, limita el listado a los productos críticos. **Arranca apagado** y su estado **se conserva** al volver a la pestaña, como el resto de los filtros (D-13).
- **Filtro de estado**: el filtro ya existente Todos / Activos / Inactivos del listado de productos.
- **Recorte**: quitar de los bordes de un texto espacios, tabuladores y saltos de línea; el valor utilizado para buscar es siempre el recortado.

### Nota de alcance — comportamiento existente vs. delta nuevo (obs. 21)

- **Se conserva tal cual** (no se reimplementa): la búsqueda por texto sobre Código y Nombre, el filtro por categoría, el filtro de estado, las columnas P. Compra y Estado, el orden del listado, el refresco al entrar en la pestaña y el gating por rol.
- **Delta nuevo de esta spec**: el criterio de criticidad y su resaltado, el umbral global con su texto informativo y su modo degradado, el control "Solo críticos", las columnas exigidas (Código, Nombre, Categoría, Stock y PrecioVenta) garantizadas, la priorización del escaneo exacto y los dos mensajes de listado vacío.
- **No se toca**: el ABM, el ajuste de stock, la política de acceso, el menú principal ni el esquema de datos.

## Requisitos funcionales en EARS (en español)

- RF-1: CUANDO un Administrador o Encargado abre la consulta de stock, EL SISTEMA muestra el listado de productos con las **cinco columnas exigidas** —**Código, Nombre, Categoría, Stock y PrecioVenta**— y **conserva** las columnas **P. Compra** y **Estado** que el listado ya muestra hoy.
- RF-2: CUANDO el usuario escribe texto en el buscador, EL SISTEMA filtra por **coincidencia parcial y sin distinguir mayúsculas ni minúsculas** sobre el **Código** y/o el **Nombre** (alcanza con que coincida uno de los dos), aplicando Recorte al texto antes de buscar y tratando el Código como texto para no perder ceros a la izquierda.
- RF-3: CUANDO el usuario ingresa el **código de barras completo** de un producto (por tecleo o por lectura de escáner), EL SISTEMA devuelve ese producto en el resultado, sin exigir que el texto sea idéntico al Nombre.
- RF-4: CUANDO el buscador está vacío o contiene solo espacios, EL SISTEMA no restringe el listado por texto.
- RF-5: CUANDO el usuario elige una categoría en el filtro, EL SISTEMA muestra solo los productos de esa categoría, resolviendo la relación por el identificador de la categoría y no por su descripción; la opción "Todas" no restringe.
- RF-6: MIENTRAS el usuario aplica filtros, EL SISTEMA combina **simultáneamente** búsqueda por texto, categoría, estado (Todos/Activos/Inactivos) y "Solo críticos", aplicando **todos** los criterios a la vez sobre el mismo listado.
- RF-7: CUANDO el usuario cambia cualquier filtro, EL SISTEMA recalcula el listado desde los datos vigentes sin perder los demás filtros seleccionados ni el texto ya escrito.
- RF-8: EL SISTEMA resalta la **fila completa** de los productos **agotados** (stock 0 **o negativo**) en **rojo** y la de los productos **bajo el umbral** (stock mayor que 0 y menor que el umbral) en **ámbar/naranja**, y agrega en la celda de Stock el texto indicador **"SIN STOCK"** o **"BAJO"** según el caso, manteniendo siempre visible la cantidad de stock.
- RF-9: EL SISTEMA obtiene el umbral de un **parámetro global de configuración de la aplicación**, único para todo el catálogo, y lo **lee al abrir la vista**, manteniéndolo durante esa sesión de vista; SI el parámetro **no está definido**, EL SISTEMA **asume 5 unidades**; EL SISTEMA **no** guarda el umbral en el producto ni en la base de datos operativa, y cambiarlo no exige modificar productos existentes ni recrear información.
- RF-10: SI el parámetro de umbral **está definido** pero su valor es **inválido** (no cumple RF-20), ENTONCES EL SISTEMA **no bloquea** la consulta: degrada el criterio —**solo el stock agotado (0 o negativo) es crítico** y **ningún producto se marca como "BAJO"**—, mantiene operativos búsqueda, filtros y listado, y comunica el estado mediante el aviso de RF-23.
- RF-11: CUANDO el usuario activa "Solo críticos", EL SISTEMA limita el listado a los productos **agotados o bajo el umbral** y lo combina con los demás filtros vigentes; al desactivarlo vuelve a mostrar el listado según esos mismos filtros; el control **arranca apagado** y su estado **se conserva** al volver a la pestaña, como el resto de los filtros.
- RF-12: EL SISTEMA considera crítico un producto **solo** cuando está agotado (stock **0 o negativo**) o cuando su stock es **mayor que 0 y estrictamente menor** que el umbral; un stock **igual** al umbral no es crítico, y un stock negativo es crítico **aun con umbral 0**.
- RF-13: SI el catálogo **no tiene datos**, ENTONCES EL SISTEMA muestra la grilla vacía con un mensaje en español que indica que **no hay productos registrados**; SI en cambio hay datos pero **ningún producto cumple** los filtros aplicados, ENTONCES EL SISTEMA muestra la grilla vacía con un mensaje distinto que indica que **ningún producto coincide** con los filtros; en ambos casos el mensaje se muestra **sin ocultar** los encabezados de columna ni los filtros, no se trata como error y no se borran ni reinician los filtros.
- RF-14: EL SISTEMA muestra los productos **inactivos** según el filtro de estado vigente (visibles cuando el filtro es "Todos" o "Inactivos", ocultos cuando es "Activos") y **no** los oculta por su condición de stock; el resaltado **también se aplica** a los inactivos que aparezcan en el listado, con la atenuación definida, y el criterio de criticidad no altera el estado del producto.
- RF-15: EL SISTEMA mantiene la consulta de stock como **estrictamente de lectura**: no modifica stock, precios, categoría ni estado, y **no altera** el alta, la edición, la baja, la reactivación ni el ajuste de stock ya existentes, que siguen funcionando con sus mismas reglas, permisos y auditoría.
- RF-16: SI la base de datos falla o se desconecta durante la consulta, ENTONCES EL SISTEMA muestra un mensaje en español y **conserva los filtros elegidos**; si ya había un listado en pantalla, lo mantiene, y si el fallo ocurre en la **primera carga** (sin listado previo), la grilla queda vacía con el mensaje, sin perder los filtros.
- RF-17: EL SISTEMA expone en la consulta **solo** los datos necesarios para mostrarla: **sin credenciales ni hashes**, y **conservando** los identificadores técnicos que el ABM existente necesita para operar sobre la fila; los importes se presentan con dos decimales.
- RF-18: EL SISTEMA restringe la consulta a los roles que hoy acceden al módulo Productos —**Administrador y Encargado**— y **no** modifica la política de acceso vigente; el umbral se administra **fuera** de la pantalla de consulta (por configuración), por lo que la pantalla no permite editarlo.
- RF-19: MIENTRAS la consulta está abierta, EL SISTEMA muestra junto al control "Solo críticos" el **umbral vigente** como **texto informativo de solo lectura**, usando **la misma lectura** que se aplica al criterio de criticidad (RF-9), sin ofrecer desde la pantalla ningún medio para modificarlo; en modo degradado no muestra ningún número (RF-23).
- RF-20: EL SISTEMA interpreta el parámetro de umbral de forma **determinista y culturalmente invariante**: aplica Recorte al valor y acepta **solo dígitos** (sin signo, sin separadores de miles y sin punto decimal); SI el valor recortado no es un entero dentro del rango admitido y mayor o igual a 0, ENTONCES lo considera **inválido** (RF-10).
- RF-21: CUANDO el texto buscado coincide **exactamente** con un código de barras, EL SISTEMA **prioriza** ese producto —lo muestra primero en la grilla y lo deja **seleccionado**— además de aplicar el filtro por texto; SI por una inconsistencia de datos existiera **más de un producto** con ese código exacto (situación imposible con la unicidad vigente), ENTONCES EL SISTEMA **no fuerza ninguna selección** y muestra todas las coincidencias en el orden habitual.
- RF-22: CUANDO una fila **resaltada** por criticidad está además **seleccionada**, EL SISTEMA **conserva el relleno de criticidad** y distingue la selección mediante un **borde de 2 px**, **sin alterar el color del texto** —que permanece **blanco** por RNF-4/D-17—, de modo que la selección no se confunda con el **ámbar de "BAJO"**; EL SISTEMA **nunca** usa el naranja de selección de la grilla (`#FF5722`) como relleno de una fila "BAJO".
- RF-23: MIENTRAS dure el **modo degradado**, EL SISTEMA muestra un **aviso único, no bloqueante y persistente** junto al texto del umbral que indica, en español, que el umbral **no está disponible por configuración inválida** y que **solo se marca "SIN STOCK" (stock 0, o negativo si existiera ese dato inconsistente)**; el aviso **nunca es modal** y **no se repite** por cada consulta ni por cada pulsación en el buscador.
- RF-24: SI al refrescar el listado la **categoría seleccionada deja de estar disponible**, ENTONCES EL SISTEMA vuelve el filtro a **"Todas"**, recalcula el listado y lo informa en español.

## Requisitos no funcionales

- RNF-1: La búsqueda **por tecla**, los filtros y el resaltado deben responder en **≤ 500 ms** con un catálogo de **1.000 productos**, y cada cambio de filtro debe producir **una única** actualización del listado.
- RNF-2: Todos los mensajes, avisos y errores deben estar en español.
- RNF-3: La consulta es de solo lectura: no persiste cambios, no crea movimientos de stock y no habilita ninguna escritura.
- RNF-4: El resaltado no puede depender únicamente del color: además del fondo de la fila (rojo/ámbar), la celda de Stock lleva el texto indicador "SIN STOCK"/"BAJO" y la cantidad se mantiene visible, para no excluir a usuarios con dificultad para distinguir colores y para mantener legibilidad sobre el fondo oscuro de la aplicación.
- RNF-5 (heredado literal de la spec 003): la vista mantiene **0** ocurrencias de `Color.Transparent` (todos los paneles internos con fondo opaco del tema), **0** llamadas a `Refresh()` y a `Invalidate()`, y **exactamente 1** llamada a `PerformLayout()` en la carga de la vista. Los **dos únicos controles nuevos** autorizados son la **casilla "Solo críticos"** y el **texto del umbral**, ambos con **fondo opaco**; no se agrega ningún otro control ni se rediseña la pantalla.
- RNF-6: Ninguna regla de negocio de esta funcionalidad (criterio de criticidad, lectura e interpretación del umbral, combinación de filtros) puede residir en la interfaz; la interfaz solo presenta lo que la lógica de negocio resuelve.
- RNF-7: Toda lectura contra la base de datos debe ser asíncrona y no debe bloquear la interfaz.
- RNF-8: El orden del listado se mantiene igual al actual (por Nombre y luego por Código), salvo la priorización del escaneo exacto (RF-21), para que el uso de la pantalla no cambie respecto de lo ya conocido.
- RNF-9: El umbral que se muestra en pantalla debe ser **el mismo** que se aplica al criterio de criticidad y el mismo durante toda la sesión de vista (D-15); en **modo degradado** no se muestra ningún número y el aviso persistente explica la causa y el criterio aplicado.
- RNF-10: La **atenuación** de los inactivos debe ser observable: misma familia de color que el caso correspondiente con menor intensidad, manteniendo la cantidad y el texto indicador legibles tanto en una captura a color como en **escala de grises**.

## Casos límite

- **Código de barras con ceros a la izquierda** (p. ej. `00012345`): la búsqueda lo trata como texto y no lo normaliza a número.
- **Texto de búsqueda con espacios en los bordes**: se aplica Recorte antes de buscar; si queda vacío, no restringe.
- **Búsqueda parcial de código** (menos dígitos que el código completo): se comporta como coincidencia parcial, coherente con RF-2.
- **Escaneo exacto**: el producto queda **primero y seleccionado** (RF-21), el buscador conserva el código escaneado y la grilla muestra el resultado filtrado; si el producto es crítico, se ve además resaltado.
- **Escaneo de un código inexistente**: el listado queda sin coincidencias y se muestra el mensaje de "ningún producto coincide" (RF-13), sin selección.
- **Varias coincidencias exactas por inconsistencia de datos** (imposible con la unicidad vigente): no se fuerza selección y se muestran todas en el orden habitual (RF-21).
- **Escaneo del mismo código dos veces**: se reemplaza el texto del buscador; no se acumulan resultados ni se concatenan búsquedas.
- **Stock igual al umbral**: no es crítico y no se resalta (RF-12).
- **Stock 4 con umbral 5**: bajo el umbral → fila ámbar/naranja e indicador "BAJO".
- **Stock 0**: agotado → fila roja con "SIN STOCK" y entra en "Solo críticos".
- **Stock negativo** (dato inconsistente): se trata como **agotado** (rojo, "SIN STOCK") y entra en "Solo críticos"; **no** se marca "BAJO" porque ese caso exige stock mayor que 0 (D-11/RF-8/RF-12).
- **Umbral 0**: solo los productos agotados (0 o negativos) resultan críticos; no aparece el indicador "BAJO".
- **Umbral mayor que el stock de todo el catálogo**: todos los productos con stock mayor que 0 quedan bajo el umbral y los agotados se siguen distinguiendo como tales; "Solo críticos" devuelve el catálogo completo.
- **Parámetro de umbral no definido**: se asume **5 unidades**, se muestra ese valor en el texto informativo y **no** hay aviso ni modo degradado (D-10).
- **Parámetro definido con valor inválido** —`"5.0"`, `"5,0"`, `"+5"`, `"-5"`, `"abc"`, vacío, o un número fuera del rango admitido—: modo degradado; el texto **no muestra número**, aparece el aviso de RF-23, solo se resalta el stock 0 (o negativo) y no aparece ningún "BAJO".
- **Parámetro con espacios** (`" 5 "`): se recorta y se acepta como **5** (RF-20).
- **Modo degradado mientras se teclea**: el aviso no se repite ni se abre como modal; sigue visible junto al texto del umbral (RF-23).
- **Configuración modificada con la vista abierta**: la vista conserva el valor leído al abrirse; el cambio se aplica al volver a abrir la vista (D-15).
- **Fila resaltada seleccionada**: se conserva el color de criticidad y la selección se distingue por un **borde de 2 px**, sin cambiar el color del texto (que sigue blanco, RF-22/D-18), sin confundirse con el ámbar de "BAJO" y sin usar el naranja de selección como relleno.
- **Listado filtrado únicamente por inactivos**: los inactivos conservan su resaltado atenuado (D-8); si además se activa "Solo críticos", la combinación es válida y puede devolver cero filas (RF-13).
- **Atenuación en escala de grises o con dificultad para distinguir colores**: el caso se sigue reconociendo por la intensidad del color, el texto indicador y la cantidad visible (RNF-4/RNF-10).
- **Catálogo sin datos**: grilla vacía con el mensaje de "no hay productos registrados", conservando encabezados y filtros (RF-13).
- **Filtros sin coincidencias**: grilla vacía con el mensaje de "ningún producto coincide", distinto del anterior y conservando encabezados y filtros (RF-13).
- **Catálogo de 1.000 productos**: la búsqueda por tecla se mantiene en ≤ 500 ms (RNF-1/D-14); se verifica con una base de prueba de ese volumen, midiendo desde la pulsación hasta que el listado se actualiza.
- **Categoría seleccionada que deja de estar disponible al refrescar** (p. ej. se desactivó mientras la vista estaba abierta): el filtro vuelve a "Todas", se recalcula el listado y se informa en español (RF-24).
- **Categoría existente sin productos** (escenario posible): al elegirla, el listado queda sin coincidencias y se aplica RF-13.
- **Categoría desactivada con productos** (escenario posible): sus productos siguen apareciendo con el filtro "Todas", pero **no** se pueden aislar por el filtro de categoría mientras la categoría siga inactiva, porque el filtro solo ofrece categorías activas (comportamiento heredado, no se modifica en esta spec).
- **Producto agotado que además está inactivo**: se muestra según el filtro de estado (RF-14) con resaltado atenuado; el criterio de criticidad no lo reactiva ni lo modifica.
- **Stock modificado entre dos consultas** (un ajuste manual o, en el futuro, una venta o compra): el listado refleja el último estado leído; no se conserva una copia obsoleta ni se bloquea al otro usuario.
- **"Solo críticos" al volver a la pestaña**: conserva el estado que tenía al salir (D-13); en la primera apertura de la sesión arranca apagado.
- **Fallo de base de datos con listado previo**: mensaje en español; se conservan filtros y el último listado (RF-16).
- **Fallo de base de datos en la primera carga**: grilla vacía con mensaje en español y filtros intactos (RF-16).
- **Intento de usar la consulta con un rol sin acceso al módulo Productos**: no llega a la pantalla (D-3); no se agrega ninguna excepción a la política de acceso.

## Fuera de alcance

- **Cambios de esquema y migraciones**: no se agrega ningún campo de stock mínimo (ni global ni por producto) y no se modifica la entidad ni la base de datos (D-2).
- **Modificación de la política de acceso**: los permisos por rol quedan como están; el Vendedor no obtiene acceso a Productos (D-3).
- **Edición del umbral desde la pantalla**: el umbral se administra por configuración de la aplicación y la consulta solo lo **muestra** como texto informativo, sin editarlo (D-2/D-6/RF-18/RF-19). Un umbral distinto por categoría, por producto o por sucursal queda fuera.
- **Detalles técnicos de implementación**: archivo y clave de configuración, nombres de controles, forma de obtener el umbral y forma de aplicar colores o atenuación se definen en el `plan.md`; esta spec no los fija.
- **Alertas, avisos automáticos, correos o notificaciones de stock bajo**: esta spec solo muestra y filtra; no notifica.
- **Historial o movimientos de stock** (cuándo y por qué cambió): corresponde a las specs de Ventas y Compras y a los campos de auditoría ya existentes.
- **Reportes de stock valorizado, rotación, proyección de reposición o exportación a PDF/Excel**: corresponden a la spec de Reportes.
- **Modificación del ABM de productos, del ajuste de stock, de sus permisos o de sus auditorías**: se conservan sin cambios (RF-15).
- **Rediseño de la pantalla de Productos o del menú principal**: la consulta vive **dentro** de la pantalla ya existente; el menú principal sigue congelado y solo se agregan los dos controles autorizados en RNF-5.
- **Reimplementación de la búsqueda, los filtros o las columnas existentes**: se conservan tal cual (nota de alcance, obs. 21).
- **Bloqueo de ventas por falta de stock o reserva de mercadería**: no se define aquí.
- **Consulta de catálogo y stock del Vendedor desde el módulo de Ventas**: **quedará cubierta por la spec 005, aún no implementada**; esta spec no la crea ni la modifica.

## Criterios de finalización

- **Pruebas automáticas (`dotnet test`) en verde** para lo verificable en la lógica de negocio:
  - clasificación de criticidad, incluido **stock negativo = agotado** y "BAJO" solo con stock mayor que 0 (RF-8, RF-11, RF-12);
  - resolución del umbral: **no definido ⇒ 5**, definido inválido ⇒ modo degradado, y las reglas de parseo de RF-20 (`" 5 "` válido; `"5.0"`, `"5,0"`, `"+5"`, `"-5"`, `"abc"`, vacío y fuera de rango inválidos);
  - Recorte y combinación de filtros por texto/código, categoría, estado y criticidad (RF-2 a RF-7), incluida la detección de coincidencia exacta de código (RF-21) y la determinación de lista vacía por catálogo sin datos vs. filtros sin coincidencias (RF-13);
  - exclusión de escrituras (RF-15).
- **Validación manual/visual** (estado de interfaz, no automatizable con `dotnet test`):
  - cinco columnas exigidas visibles y P. Compra/Estado conservadas;
  - búsqueda por código de barras y por nombre; escaneo exacto deja el producto **primero y seleccionado**;
  - filtro por categoría combinado con el estado y con "Solo críticos";
  - fila completa **roja con "SIN STOCK"** (stock 0 o negativo) y **ámbar/naranja con "BAJO"**;
  - **umbral vigente visible** junto a "Solo críticos" y **sin número** en modo degradado, con el aviso único, persistente y no modal;
  - **selección sobre fila resaltada** distinguible por un **borde de 2 px** sin perder el color de criticidad ni cambiar el texto, que permanece blanco;
  - **atenuación** de inactivos verificada en captura a color y en **escala de grises**;
  - los **dos mensajes** de listado vacío (catálogo sin datos vs. filtros sin coincidencias) con encabezados y filtros visibles;
  - **conservación de filtros** (incluido "Solo críticos") al cambiar de pestaña y volver;
  - **fallo de base de datos** con listado previo y en la primera carga;
  - **≤ 500 ms** con 1.000 productos al escribir en el buscador.
- **Verificación por inspección**: la interfaz **no** calcula criticidad, **no** interpreta el umbral ni combina los filtros (RNF-6); **0** `Color.Transparent`, **0** `Refresh()`/`Invalidate()`, **exactamente 1** `PerformLayout()` en la carga, y solo los **dos** controles nuevos autorizados (RNF-5).
- **Asincronía y orden**: las lecturas se ejecutan de forma asíncrona sin bloquear la interfaz (RNF-7) y el listado conserva el orden por Nombre y Código, salvo la priorización del escaneo exacto (RNF-8).
- Compilación limpia (`dotnet build`) y suite completa en verde (`dotnet test`), sin pruebas en rojo.
- Verificación de que **no** hubo cambios de esquema ni de datos: no se crearon ni aplicaron migraciones y el stock, los precios y los estados de los productos no se alteraron por la consulta.

## Dudas abiertas

- **No quedan dudas abiertas.** Las 5 dudas de la primera redacción quedaron cerradas en **D-5 … D-9** y la última duda pendiente —el texto del umbral en modo degradado— quedó cerrada por **D-15**: en modo degradado no se muestra número, el texto informa que el umbral no está disponible por configuración inválida y que solo se marca "SIN STOCK" (stock 0 o negativo). Las **23 observaciones de QA** se resolvieron con **D-10 … D-15** y con las precisiones de los RF, RNF, casos límite y criterios de finalización de esta versión.
- **No hay contradicciones**: el valor **5** es un **fallback interno** cuando el parámetro no está definido (no un valor inicial obligatorio de configuración) y el **modo degradado** aplica **solo** cuando el parámetro está definido con un valor inválido.

### Decisiones ya cerradas por el usuario (NO re-abrir)

- **D-1 (carpeta y alcance documental)**: esta funcionalidad vive en una **spec nueva**, `specs/004-consulta-stock/`. **No** se extiende `specs/003-productosform-bugfix`, que quedó cerrada como corrección de renderizado y cuyo "Fuera de alcance" excluye la lógica de negocio y las migraciones. El `000-roadmap.md` **no** se modifica: la numeración real del repositorio ya difiere de la que ese documento reservaba (Clientes en 003, Proveedores en 004).
- **D-2 (umbral mínimo)**: el umbral es **global y configurable** mediante un **parámetro de configuración de la aplicación**; el archivo y la clave concretos se detallan en el `plan.md`. **Sin migración y sin cambios de esquema**: el producto **no** tiene —ni se le agrega— un campo de stock mínimo.
- **D-3 (permisos)**: **no** se modifica la política de acceso. La consulta vive en la pestaña Productos, que hoy ven solo Administrador y Encargado; el **Vendedor no la ve** y eso se mantiene.
- **D-4 (interfaz)**: **resaltado visual** de los productos críticos **más** un control de activación **"Solo críticos"** para filtrar; el resaltado **distingue** stock 0 de stock por debajo del umbral (pero mayor que 0). No se rediseña la pantalla ni el menú.

#### Decisiones nuevas — resolución de las 5 dudas de la primera redacción (2026-10-26)

- **D-5 (umbral por defecto y degradación)** — **sustituida por D-10**: fijó un umbral de referencia de **5 unidades** y una degradación a "solo stock agotado crítico" cuando el umbral aplicable no es válido. La distinción entre **no definido ⇒ 5** y **definido inválido ⇒ degradado con aviso** quedó fijada en **D-10**, que prevalece sobre esta redacción.
- **D-6 (umbral visible)**: el umbral vigente **se muestra en pantalla como texto informativo de solo lectura**, junto al control "Solo críticos"; la pantalla no permite modificarlo.
- **D-7 (resaltado)**: se resalta la **fila completa**: **rojo** para stock 0 y **ámbar/naranja** para stock bajo el umbral, **más un texto indicador** en la celda de Stock —"**SIN STOCK**" para stock 0 y "**BAJO**" para stock bajo el umbral— para no depender solo del color; la cantidad de stock sigue visible en ambos casos.
- **D-8 (inactivos)**: el resaltado **sí aplica** a los productos inactivos que aparezcan en el listado, con **atenuación** que no oculte el aviso.
- **D-9 (tipo del umbral)**: el umbral es un **entero ≥ 0**. Un valor **con decimales**, **negativo** o **no numérico** es inválido y se degrada según D-10.

#### Decisiones nuevas — revisión QA (2026-10-26)

- **D-10 (el 5 es fallback interno, no valor inicial obligatorio — obs. 1, BLOQUEANTE)**: si el parámetro de configuración **falta**, el sistema **asume 5**. La degradación (aviso + solo stock agotado crítico) se aplica **solo cuando el parámetro existe pero su valor es inválido**. No hay dos conductas: **no definido ⇒ 5 sin aviso**; **definido inválido ⇒ degradado con aviso**. Reemplaza la redacción anterior de D-5 y de RF-9/RF-10.
- **D-11 (stock negativo = agotado — obs. 2)**: un stock **negativo** se trata como **agotado**: fila roja, "SIN STOCK" y entra en "Solo críticos"; "BAJO" exige `stock > 0`.
- **D-12 (prioridad del escaneo exacto — obs. 17)**: si el texto buscado coincide **exactamente** con un código de barras, ese producto se **prioriza y se selecciona** en la grilla además de filtrar. Con la unicidad vigente solo puede haber **una** coincidencia exacta; si por inconsistencia de datos hubiera varias, no se fuerza selección y se muestran todas en el orden habitual. Tras el escaneo se ve el listado filtrado por ese código con el producto primero y seleccionado.
- **D-13 ("Solo críticos" arranca apagado y se conserva — obs. 16)**: el control inicia **desactivado** en la primera apertura y su estado **se conserva** al volver a la pestaña, como el resto de los filtros.
- **D-14 (objetivo medible de rendimiento — obs. 5)**: la búsqueda por tecla, los filtros y el resaltado deben responder en **≤ 500 ms** con **1.000 productos**; se verifica con una base de prueba de ese volumen, midiendo desde la pulsación hasta que el listado se actualiza.
- **D-15 (lectura única del umbral y texto en modo degradado — obs. 8 y 7)**: el umbral se lee **al abrir la vista** y se mantiene durante esa **sesión de vista**; el texto informativo usa **esa misma** lectura. En **modo degradado** no se muestra ningún número: el texto informativo dice que el umbral **no está disponible por configuración inválida** y que **solo se marca "SIN STOCK" (stock 0, o negativo si existiera ese dato inconsistente)**, de forma persistente y no modal.

#### Decisiones nuevas — validación de implementación (2026-10-26)

- **D-17 (paleta definitiva)**: sobre el fondo oscuro de la grilla, agotado **`#B71C1C`** (atenuado inactivo **`#6E2020`**), bajo umbral **`#F9A825`** (atenuado inactivo **`#8C6D0A`**) y **texto blanco en los cuatro casos**; se conservan el indicador textual ("SIN STOCK"/"BAJO") y el borde de selección de **2 px**. **Aclaración**: el texto es **blanco también en la fila seleccionada**; la selección se marca con el **borde de 2 px** (coherente con RF-22 y D-18) y **no** cambiando el color del texto.
- **D-18 (selección sobre una fila resaltada)**: la selección sobre una fila resaltada se distingue por un **borde de 2 px**; el **color de texto no cambia** (se mantiene blanco). Surge de la validación de la implementación del 2026-10-26, que detectó la contradicción de letra entre RF-22 (que exigía distinguir la selección cambiando el color del texto) y D-17 (texto blanco en los cuatro casos). Alternativa descartada: **cambiar el color del texto** para marcar la selección, que rompía D-17 y el contraste sobre el relleno de criticidad. RF-22 se ajustó en consecuencia, sin renumerar.

### Notas de trazabilidad (para el plan técnico)

- **D-2 + D-5 + D-10 + D-15** implican que el plan debe definir **dónde y cuándo** se lee el parámetro, la resolución "no definido ⇒ 5" y el modo degradado solo para valores inválidos, y cómo se garantiza que el texto informativo use la misma lectura que el criterio.
- **D-4 + D-7 + D-8 + D-11 + obs. 3 + obs. 20** implican que el plan debe resolver cómo se comunica la clasificación de criticidad a la presentación sin que esta calcule reglas (RNF-6), y cómo se aplican relleno, indicador, atenuación y selección sin que se pisen entre sí.
- **D-12 + D-13 + D-14 + RF-24** implican que el plan debe detallar la priorización/selección tras el escaneo, la persistencia del estado de "Solo críticos", la estrategia de rendimiento con 1.000 productos y el refresco del filtro de categoría cuando desaparece.
- **D-3** implica que **no** se toca la política de acceso ni sus pruebas, ni se agrega gating nuevo: la consulta hereda el acceso existente al módulo.
- **RNF-5** hereda los requisitos de la spec 003: no se reabren ni se "arreglan" con cascadas de refresco/repintado.

## Historial de cambios (trazabilidad de los RF)

### Primera redacción y cierre de dudas (2026-10-26)

| RF de la primera redacción | Estado | Destino |
|---|---|---|
| RF-1 a RF-7 | sin cambios en esa ronda | RF-1 a RF-7 (búsqueda, filtros, columnas) |
| RF-8 | ampliado | mismo RF-8 + **D-7** (fila completa rojo/ámbar) y texto indicador "SIN STOCK"/"BAJO" |
| RF-9 | precisado | mismo RF-9 + **D-5** (umbral de referencia 5, luego sustituida por **D-10**) |
| RF-10 | precisado | mismo RF-10 + **D-5** (solo stock agotado crítico, luego sustituida por **D-10**) y **D-9** (decimales inválidos) |
| RF-11 a RF-13 | sin cambios en esa ronda | RF-11 a RF-13 ("Solo críticos", criterio estricto, listado vacío) |
| RF-14 | ampliado | mismo RF-14 + **D-8** (resaltado atenuado en inactivos) |
| RF-15 a RF-18 | sin cambios en esa ronda | RF-15 a RF-18 (solo lectura, sin regresión, BD caída, roles) |
| — | nuevos | RF-19 (**D-6**, umbral visible) y RF-20 (**D-9**, entero ≥ 0) |

### Revisión QA (2026-10-26) — numeración estable, sin renumerar

| RF | Estado | Cambio |
|---|---|---|
| RF-1 | modificado | enumeración de las 5 columnas exigidas y conservación de P. Compra y Estado (obs. 14) |
| RF-3 | sin cambios | sigue cubriendo el ingreso del código completo; la priorización va a RF-21 |
| RF-8 | modificado | stock **negativo** se resalta como agotado y "BAJO" exige stock > 0 (D-11) |
| RF-9 | **reescrito** | lectura al abrir la vista; **no definido ⇒ 5**; sin mención a degradación por ausencia (D-10/D-15) |
| RF-10 | **reescrito** | degradación **solo** si el parámetro está definido con valor inválido (D-10) |
| RF-11 | modificado | "Solo críticos" arranca apagado y se conserva al volver (D-13) |
| RF-12 | modificado | crítico = agotado (0 o negativo) o stock > 0 menor que el umbral (D-11) |
| RF-13 | modificado | dos mensajes distintos: catálogo sin datos vs. filtros sin coincidencias (obs. 15) |
| RF-16 | modificado | fallo de BD en primera carga sin listado previo (obs. 23) |
| RF-17 | modificado | "sin claves" = sin credenciales/hashes; se conservan identificadores del ABM (obs. 18) |
| RF-19 | modificado | misma lectura que el criterio; sin número en modo degradado (D-15) |
| RF-20 | modificado | parseo determinista e invariante: solo dígitos, Recorte, rango (obs. 9) |
| RF-21 | **nuevo** | priorización y selección ante coincidencia exacta de código (D-12) |
| RF-22 a RF-24 | **nuevos** | selección sobre fila resaltada (obs. 3), aviso único/persistente (obs. 4), categoría que desaparece (obs. 10) |

### Validación de implementación (2026-10-26) — numeración estable, sin renumerar

| RF | Estado | Cambio |
|---|---|---|
| RF-22 | **reescrito** | la selección se distingue por un **borde de 2 px** conservando el relleno de criticidad; **no** se exige cambiar el color del texto, que permanece **blanco** (D-17). Se prohíbe usar el naranja de selección `#FF5722` como relleno de una fila "BAJO" (D-18) |
