# Checklist visual unificado — HU-09 (Spec 005): T9b, T15, T16 y T18–T21

> **Estado del documento**: preparado por Planner (2026-11-01) y actualizado el 2026-11-02 (C-1, C-2, regla B-1, guía por tema, V-35). **Esta actualización no fue revisada por el Reviewer** (criterio del usuario: sin rondas de revisión redundantes); la entrega al usuario es para prueba manual.
> **Ejecuta**: el usuario, de forma manual. Ningún punto se da por cumplido sin una nota de lo observado.
> **Origen**: decisiones del usuario del 2026-11-01 (1a, 2, 3a, 5a y 6). Sustituye, para la verificación visual, a los checklists sueltos de T9b, T15, T16, T18, T19, T20 y T21 (T17 apartado A incluido). El punto 8 de `T9b-checklist.md` (historial embebido) queda reemplazado por **V-28**.
> **Fuera de este checklist**: T10 (atomicidad contra SQL Server, no regresión del catálogo, doble envío y permisos) y todo lo de HU-10. El rendimiento con 200 líneas (RNF-9) quedó **SIN VERIFICAR, descartado por decisión del usuario (2026-11-01)** y este checklist no mide tiempos de 200 ms ni cargas de 200 líneas.

---

## Guía de prueba por tema

**Orden recomendado**: ejecutar en el orden numérico de las secciones A a H (V-35 se ejecuta entre V-25 y V-26; V-18 y V-19 **antes** de V-26; V-34 al final; la escala 150 % de V-13/V-14 se restaura a 100 % antes de la sección D). Los puntos de layout (V-01 a V-14) pueden hacerse aparte de los funcionales, pero V-15 a V-35 conservan su orden relativo.

| Tema | Puntos V-xx |
|---|---|
| Carga (agregar líneas, selección bidireccional) | V-05, V-10, V-19, V-20, V-22, V-31, V-32, V-33 (búsquedas: V-09, V-15, V-16, V-17) |
| Edición (Editar y guardar) | V-22 (paso 2) |
| Eliminación (Quitar / Cancelar compra) | V-22 (paso 3), V-25; V-24 y V-31 usan «CANCELAR COMPRA» para limpiar |
| Cálculos (subtotal, total, decimales) | V-21, V-22, V-35 (paso 6: 3 × 10,33 = 30,99), V-12 (dos decimales) |
| Registro (éxito, descarte, doble clic) | V-26 (éxito y descarte), V-27 (doble clic) |
| Validaciones (cantidad, precios, límites, mensajes) | V-35 (cantidad, precios y límites), V-23 (máximo acumulado), V-21 (entrada inválida en el subtotal), V-24 (botón y regla de proveedor) |
| Historial y detalle (modal) | V-18, V-28, V-29, V-34; V-30 **SIN VERIFICAR** |
| Permisos por rol (Administrador / Encargado / Vendedor) | V-02 (los tres roles), V-34 (Encargado dado de baja) |

**SIN VERIFICAR dentro de la guía**: V-30 (estado «Inactiva» y desempate por número: no generable sin incumplir RF-48; decisión del usuario 2026-11-01); rechazo por permisos de la **lógica** de negocio (el Vendedor no llega a la pantalla; lo cubre `dotnet test`); fallo de lectura o de persistencia visto desde la aplicación (no se puede provocar desde la UI; cubierto por pruebas automáticas, C-1/T22; la atomicidad real está en T10); rendimiento con 200 líneas (RNF-9, descartado por decisión del usuario).

---

## 0. Cómo detectar y registrar un bloqueo (leer antes de empezar)

Política: AGENTS.md §11. El plazo corre **desde que se pulsa el botón o se inicia la acción**, medido con un reloj o cronómetro.

| Tipo de operación | Tiempo máximo |
|---|---:|
| Arranque de la aplicación (hasta ver la ventana principal) | 20 s |
| Respuesta visual sencilla (redibujo, cambio de tamaño, abrir/cerrar un modal, mostrar resultados de búsqueda) | 10 s |
| Operación funcional individual (registrar una compra, cargar el historial) | 30 s |
| Escenario completo (un punto con varios pasos) | 120 s |

**Se considera bloqueo** cualquiera de estos casos al vencer el plazo: el evento nunca termina; se pulsa un botón y la grilla nunca se actualiza; una consulta queda esperando; la ventana deja de responder (título «No responde», cursor de espera permanente); una operación asíncrona no completa; el resultado esperado nunca aparece; la aplicación queda visualmente congelada.

**Al vencer el plazo**:
1. Registrar el resultado como **`FAILED — TIMEOUT / POSIBLE BLOQUEO`**. No es «pendiente» y **no** se sigue esperando ni se amplía el plazo.
2. **No repetir el clic, no reiniciar la operación, no completar el flujo por otra vía y no saltar el paso.**
3. **Antes de cerrar la aplicación**, anotar en este orden: (1) acción en curso; (2) hora de inicio y segundos transcurridos frente al plazo; (3) resultado esperado frente al observado; (4) captura de pantalla (`Win + Shift + S`); (5) ventana activa y PID (Administrador de tareas → Detalles → proceso de la aplicación); (6) mensajes o errores visibles; (7) estado del proceso en el Administrador de tareas (Estado «No responde» o «En ejecución», CPU, memoria); (8) volcado de pila: si no se dispone de herramienta, escribir «no disponible»; (9) último paso completado correctamente; (10) clasificación: `timeout`, `aplicación no responsiva` o `error de automatización`.
4. Recién entonces cerrar la aplicación (Administrador de tareas → Finalizar tarea) y comprobar que el proceso ya no figura. Si no se puede terminar, anotarlo como fallo del mecanismo, nunca como punto superado.
5. **Operaciones con efectos secundarios (V-26 y V-27, registro de compras)**: **nunca** repetirlas tras un bloqueo sin que antes se verifique si la compra anterior quedó guardada: la comprobación (consulta de solo lectura) corre **a cargo del usuario (SSMS)**, o se omite si no hay acceso y se verifica abriendo «Ver Historial». **Ningún agente ejecuta SQL durante las pruebas.** Detener el checklist y avisar.
6. **Efectos permanentes**: C1 y C2 (y C3 de V-34, y cualquier compra de más si falla V-27) quedan guardadas para siempre: no hay baja física y la seed no las elimina. El checklist no da los mismos resultados si se repite sobre la misma base (cambian V-18, V-19, V-28 y V-31–V-34), y P1, P2 y P3 quedan con historial de proveedores. El stock y los precios del catálogo no cambian (HU-10).

7. **Regla vigente en el código, pendiente de decisión del usuario (B-1): no se agrega una línea sin proveedor elegido; si el usuario decide quitarla, se actualizarán estos pasos.** Por eso todos los puntos que cargan líneas dicen «elegir primero el proveedor» (buscarlo en «Buscar proveedor» y elegirlo en su listado). Si se intenta agregar sin proveedor, la pantalla muestra «Seleccione el proveedor...» y no agrega la línea. Esa regla no consta en la spec: no se marca como fallo.

Los plazos miden la respuesta de la aplicación desde cada clic o tecla; el tiempo que el usuario tarda en escribir o elegir no cuenta para el plazo de escenario de 120 s.

**Valores de «Resultado»** a usar en la tabla de registro: `OK`, `FALLIDA` (no se cumple el criterio), `FAILED — TIMEOUT / POSIBLE BLOQUEO`, `NO EJECUTADO` (indicar motivo, por ejemplo «la base ya tiene compras»), `SIN VERIFICAR` (el punto no puede generarse por decisión documentada; V-30).

---

## 1. Preparación y datos

**Comprobaciones previas (Reviewer)**: 2026-11-01 21:37, `dotnet build` 0 errores y `dotnet test` 771/771. **2026-11-02**: el código incluye C-1 (T22, `CompraNegocio.cs`: 5 pruebas nuevas, filtro `Compra` 271/271) y C-2 (T23, `ComprasForm.cs`: alineación en filas alternadas, solo se confirma en **V-12**); build 0 errores (18 advertencias conocidas NU1701/NU190x). La suite completa de 771 se ejecuta **una vez** al final; no figura como ejecutada tras C-1/C-2.

**Seed ampliada** (`scripts/seed-desarrollo.sql`, ya corregida por el implementer; las credenciales están en `scripts/README-preparar-bd.md` y **no** se copian aquí). **Se ejecuta ANTES de empezar el checklist, una sola vez** (preparar la base con el procedimiento del README; no repetirla durante el checklist):
- Usuarios: 4 (DNI 90000001 a 90000004): Administrador, Encargado, Vendedor y un **Encargado adicional activo, sin compras** (DNI 90000004).
- Productos: 201, de los cuales **200 activos**: 12 base (`7790001000xxx`), 188 de volumen (`7790002000001` a `7790002000188`) y **1 inactivo** (`7790003000001`, «Producto Inactivo Prueba»).
- Proveedores: 11, **10 activos**: «A» = «Distribuidora Norte SRL» (CUIT 30123456789), «B» = «Logitech Argentina SA» (CUIT 20123456789) y 8 activos nuevos (CUIT que empiezan con `3071100`; razones sociales sin puntos: 4 con «SA» —«Alfa Distribuciones SA», «Gamma Alimentos SA», «Epsilon Bebidas SA», «Eta Frutas y Verduras SA»— y 4 con «SRL» —«Beta Mayorista SRL», «Delta Limpieza SRL», «Zeta Lacteos SRL», «Theta Almacen SRL»; según `scripts/seed-desarrollo.sql`); **1 inactivo** («Tecnologia Norte», CUIT 27123456789).
- Clientes: 12 (9 activos). Categorías: 6 (5 activas). Estos datos no intervienen en Compras.
- La seed no crea compras ni ejecuta `DELETE`. Antes de empezar, abrir «Ver Historial»: si ya lista compras, registrar V-18 y V-19 como `NO EJECUTADO` ("la base ya tiene compras"), exigir en V-28 que C2 y C1 sean las dos primeras filas (no "exactamente dos") y avisar al coordinador antes de V-31.
- **Compras previas (si la base ya las tiene)**: cambian los resultados esperados de V-31 a V-33, porque los proveedores «habituales» de un producto y los productos habituales de un proveedor salen del historial completo. Concretamente: (a) V-31 paso 1: P2 se autocompleta con A **solo si** A es su único proveedor histórico; si una compra previa lo compró a otro proveedor, se filtra el listado y no se autocompleta; en el paso 3, P4 deja de ser «sin proveedores» si alguna compra previa lo incluye; (b) V-32 y V-33: el listado de habituales de A puede mostrar además otros productos comprados antes, no solo P1 y P2; (c) V-19 queda `NO EJECUTADO`. Para saber qué había antes, el usuario abre «Ver Historial» y el detalle de cada compra previa (o, si tiene acceso, consulta con SSMS; ningún agente ejecuta SQL). Anotar la diferencia en Observaciones; no es fallo por sí sola.
- **Verificación opcional de la base (SSMS, a cargo del usuario)**: la consulta de solo lectura `SELECT COUNT(*) FROM COMPRA;` confirma que no hay compras previas. Si no hay acceso, se omite y se usa «Ver Historial».

**Clasificación de datos por punto**:
- **Seed actual**: la base recién preparada con la seed ampliada cubre el punto.
- **Compras del checklist**: el punto usa las compras **C1** y **C2** que se registran en V-26 y V-27 (por eso el orden importa).
- **SIN VERIFICAR**: el punto no puede generarse sin incumplir una regla de la spec (V-30, RF-48; decisión del usuario 2026-11-01) o la app no ofrece la función.

**Productos de trabajo**: antes de empezar, elegir cuatro productos activos distintos de la seed y anotar sus códigos: **P1, P2, P3 y P4**.

**Compras planificadas** (se crean una sola vez cada una):
- **C1** (V-26): proveedor **A**, líneas **P1** y **P2**.
- **C2** (V-27): proveedor **B**, líneas **P1** y **P3**.
- Resultado esperado del historial: P1 tiene **dos** proveedores (A y B), P2 tiene **uno** (A), P4 **ninguno**; los habituales de A son P1 y P2.

**Orden de ejecución obligatorio**: secciones A → H en el orden numérico; **V-35 (agregado, sección E) se ejecuta después de V-25 y antes de V-26**. V-18 y V-19 **deben** ejecutarse antes de V-26 (requieren que no haya compras).

**Carga de líneas (B-1, ver §0 punto 7)**: en todo punto que agrega líneas, **elegir primero el proveedor** (A salvo que el punto diga otro) y recién después buscar y agregar productos. Con la base sin compras, elegir un producto **no** autocompleta proveedor (no hay historial).

**Tamaños de ventana**:
- **Tamaño restaurado de diseño**: con la ventana principal **restaurada** (no maximizada), el área de la pestaña Compras corresponde a ≈1083×508 y el área útil real ≈1053×478 a 100 % de escala (diagnóstico de T21). Si el tamaño restaurado difiere, redimensionar hasta aproximarlo y **anotar el tamaño usado**.
- **150 % DPI**: Configuración de Windows → Pantalla → Escala 150 %. Puede requerir cerrar sesión de Windows; reiniciar la aplicación después del cambio.

---

## 2. Puntos de verificación

### A. Arranque y permisos

**V-01 — Ventana principal maximizada al iniciar.** Origen: T17 apartado A (aprobado 2026-11-01).
- Precondiciones: aplicación cerrada; seed actual (usuario Administrador). Escala 100 %.
- Pasos: iniciar la aplicación; iniciar sesión; observar la ventana principal.
- Resultado esperado: la ventana principal abre **maximizada**.
- Tiempo máximo: 20 s (arranque, desde iniciar sesión hasta ver la ventana).
- Éxito: abre maximizada dentro del plazo. Fallo: abre restaurada o con otro tamaño; o vence el plazo (`FAILED — TIMEOUT / POSIBLE BLOQUEO`).

**V-02 — Permisos por rol.** Origen: T9b punto 2; RF-8.
- Precondiciones: seed actual (Administrador, Encargado y Vendedor).
- Pasos: iniciar sesión con cada rol, uno por vez; en cada caso buscar la pestaña Compras y, si existe, recorrer sus acciones.
- Resultado esperado: Administrador y Encargado ven la pestaña y todas las acciones habilitadas (sin acciones ocultas por rol); Vendedor **no** ve la pestaña.
- Tiempo máximo: 20 s por arranque; 120 s el escenario completo.
- Éxito: los tres roles se comportan como se espera. Fallo: Vendedor accede a Compras, o Administrador/Encargado tienen acciones ocultas o deshabilitadas por rol; o vence un plazo.

### B. Pantalla y geometría a 100 % de escala

**V-03 — Rótulos, retiro de la maqueta y controles admitidos.** Origen: T9b puntos 1 y 11; RF-1, RF-2, RF-3, RNF-7.
- Precondiciones: seed actual; sesión de Administrador; pestaña Compras abierta.
- Pasos: observar la pantalla completa.
- Resultado esperado: rótulo «Registro de Compras a Proveedores»; **no** aparecen el título «REGISTRO DE COMPRAS Y RECEPCIÓN DE STOCK», el botón «Nueva Orden de Compra», un combo de proveedores escrito a mano, el campo «N° Factura / Comprobante» ni filas de ejemplo; estilo MaterialSkin coherente con el resto del sistema; acciones del pie: «REGISTRAR COMPRA», «CANCELAR COMPRA» y «Ver Historial».
- Tiempo máximo: 10 s (respuesta visual al abrir la pestaña).
- Éxito: todo lo esperado presente y nada de lo retirado. Fallo: falta un rótulo, aparece un elemento retirado o un control no admitido.

**V-04 — Sin historial embebido.** Origen: T18 apartado D; T19 apartado D; D-4 (modificado 2026-11-01).
- Precondiciones: seed actual; pestaña Compras abierta.
- Pasos: recorrer la pantalla (incluido el pie, desplazando si hay barra).
- Resultado esperado: la pantalla de compras **no** contiene una grilla de historial; el historial solo se alcanza con «Ver Historial».
- Tiempo máximo: 10 s.
- Éxito: no hay historial embebido. Fallo: aparece una grilla o un botón «Ver detalle» de historial en la pantalla.

**V-05 — Tamaño restaurado de diseño: nada fuera del área visible.** Origen: T15 (criterio 1083×508, SIN VERIFICAR); T16; T17 apartado D (parte vigente).
- Precondiciones: seed actual; ventana **restaurada** al tamaño de diseño (sección 1); escala 100 %; al menos una línea cargada (**elegir primero el proveedor A**; luego agregar P1 con cantidad 1 y los precios prellenados).
- Pasos: observar la pantalla; desplazar verticalmente si aparece barra.
- Resultado esperado: **ningún** control queda inaccesible; **no** hay barra de desplazamiento horizontal; ningún botón ni rótulo muestra texto recortado («Agregar a la compra», «REGISTRAR COMPRA», «CANCELAR COMPRA», «Ver Historial», «Cantidad», «Precio compra», «Precio venta»).
- Tiempo máximo: 10 s por redimensionado.
- Éxito: todo visible o alcanzable, sin barra horizontal ni recortes. Fallo: un control inalcanzable, barra horizontal o texto recortado. Anotar el tamaño de ventana usado.

**V-06 — Columna «Quitar» y pie dentro del área útil.** Origen: T21 apartado 10 (layout a ≈1053×478).
- Precondiciones: las de V-05.
- Pasos: sin desplazar, observar la grilla «Compra Actual» y el pie.
- Resultado esperado: la columna «Quitar» (última) se ve **completa**; los tres botones del pie («REGISTRAR COMPRA», «CANCELAR COMPRA», «Ver Historial») y «Agregar a la compra» quedan **dentro** del área visible.
- Tiempo máximo: 10 s.
- Éxito: se cumple sin desplazar. Fallo: «Quitar» recortada o algún botón fuera del área visible.
- Observación de riesgo (derivada de valores documentados, no medida): con el mínimo vertical conservado de 454 px y un área útil de ≈454 px, el margen es 0 px; si aparece una barra vertical, anotarlo y anotar si los botones siguen visibles sin desplazar.

**V-07 — Ajustes UX/UI aprobados (2026-11-01).** Origen: T21 apartado 0 (filas `30 / 240 / Percent 100 / 50`, mínimo `(850, 454)`, `gridLineas` mínimo `(0, 62)`, `Location (12, 12)`, ajuste en `OnLoad`/`OnVisibleChanged`).
- Precondiciones: las de V-05.
- Pasos: (1) observar el encabezado, el bloque de producto/proveedor, «Compra Actual» y el pie; (2) cambiar a otra pestaña y volver a Compras; (3) maximizar y restaurar.
- Resultado esperado: el título del encabezado se ve completo; el bloque de producto muestra todas sus filas hasta «Agregar a la compra» sin recorte; «Compra Actual» muestra el encabezado de la grilla y al menos una fila; el pie muestra los tres botones sin recorte; el contenido tiene un margen uniforme respecto del borde de la pestaña (sin doble margen ni contenido pegado al borde); al mostrar la pestaña por primera vez y al volver a ella la tabla ya ocupa el ancho disponible (sin quedar angosta).
- Tiempo máximo: 10 s por cambio de pestaña o de tamaño; 120 s el escenario.
- Éxito: se cumple todo lo esperado. Fallo: alguna fila o botón recortado, margen irregular, tabla angosta al aparecer o tras volver a la pestaña.

**V-08 — Maximizado: ancho completo, bloques gemelos y columnas de acción.** Origen: T21 apartado 10; T20 apartado A (vigente); T15; T16; T17 apartado D (parte vigente).
- Precondiciones: seed actual; ventana **maximizada**; al menos una línea cargada (si la pantalla está limpia, **elegir primero el proveedor A** y agregar P1).
- Pasos: observar la pantalla.
- Resultado esperado: la tabla ocupa todo el ancho; «Compra Actual» crece en alto; los bloques de producto y proveedor tienen el **mismo ancho** (50/50); «Editar» y «Quitar» se ven completos; sin barras de desplazamiento; sin huecos ni recortes.
- Tiempo máximo: 10 s.
- Éxito: se cumple todo. Fallo: hueco a la derecha, «Compra Actual» sin crecer, bloques desiguales, columnas de acción recortadas o barra de desplazamiento.

**V-09 — Buscadores en su propia fila y grillas de búsqueda no colapsadas.** Origen: T19 apartado A (vigente); T18 apartado B.
- Precondiciones: seed actual; tamaño restaurado de diseño y luego maximizado.
- Pasos: escribir en «Buscar producto» un texto que devuelva varios resultados (por ejemplo, el prefijo `779`); repetir con «Buscar proveedor» (por ejemplo, `S` o un fragmento de CUIT de A).
- Resultado esperado: cada cuadro de búsqueda queda **encima** de su grilla, sin tapar su encabezado ni las primeras filas; las grillas muestran encabezado y filas (no colapsadas a una franja de ~30 px).
- Tiempo máximo: 10 s por búsqueda.
- Éxito: sin superposición y grillas legibles en ambos tamaños. Fallo: un buscador tapa la grilla o una grilla queda colapsada; o los resultados no aparecen en 10 s.

**V-10 — Scroll vertical en la grilla de productos y en «Compra Actual».** Origen: T19 apartado C (vigente; `gridLineas` en `ScrollBars.Vertical` según T21 apartado 3).
- Precondiciones: seed actual (200 productos activos); tamaño restaurado de diseño; **proveedor A elegido primero** (si la pantalla ya lo tiene de puntos anteriores, se conserva).
- Pasos: (1) buscar `779` para listar los productos; (2) agregar líneas con productos distintos hasta superar las filas visibles de «Compra Actual»; (3) buscar un producto de nombre largo.
- Resultado esperado: las dos grillas muestran barra **vertical** al superar sus filas visibles; **ninguna** muestra barra horizontal.
- Tiempo máximo: 10 s por acción; 120 s el escenario.
- Éxito: barra vertical presente y sin barra horizontal. Fallo: filas inalcanzables, sin barra vertical o con barra horizontal.
- Al terminar: cancelar la compra en preparación (V-25 se ejecuta más adelante con otra carga; aquí basta con «CANCELAR COMPRA» y confirmar).

**V-11 — Scroll vertical en la grilla de proveedores.** Origen: T19 apartado C (vigente).
- Precondiciones: seed actual ampliada (10 proveedores activos, ejecutada antes del checklist, una sola vez); si la grilla muestra pocas filas visibles, restaurar al tamaño de diseño.
- Pasos: en «Buscar proveedor» escribir `3071100` (coincide con los 8 proveedores nuevos) y observar la grilla.
- Resultado esperado: si los resultados superan las filas visibles, barra vertical y sin barra horizontal. Si caben todos sin barra, anotar el tamaño de ventana y las filas visibles; no se considera fallo.
- Tiempo máximo: 10 s.
- Éxito/Fallo: como V-10. **Ahora ejecutable** (antes: `NO EJECUTADO`).

**V-12 — Estilo de las grillas.** Origen: T18 apartado E; T21 apartado 4 (estilos con `Clone()`).
- Precondiciones: seed actual; pantalla limpia; **proveedor A elegido primero**; al menos tres líneas cargadas con precios distintos (por ejemplo cuatro, para tener filas 2.ª y 4.ª de fondo alternado).
- Pasos: seleccionar una celda de cada grilla; observar las filas y las columnas de importes de «Compra Actual». Al terminar, «CANCELAR COMPRA» → «Sí».
- Resultado esperado: se selecciona la **fila completa** y solo una; filas alternadas con color distinto; **Cantidad, Precio de compra, Precio de venta y Subtotal alineados a la derecha TAMBIÉN en las filas impares (C-2/T23: las de fondo alternado, 2.ª, 4.ª…)**, no solo en las pares; importes con dos decimales.
- Tiempo máximo: 10 s.
- Éxito: se cumple en las tres grillas (productos, proveedores, líneas). Fallo: selección por celda, selección múltiple, importes alineados a la izquierda en alguna fila (en particular las impares: C-2 no corregido) o formato distinto de dos decimales.

### C. Escala 150 %

**V-13 — Escala 150 %: solo barra vertical y pie alcanzable.** Origen: T21 apartado 10 (≈911×378).
- Precondiciones: escala de Windows 150 %; aplicación reiniciada; tamaño restaurado de diseño; seed actual; al menos una línea cargada (tras reiniciar la aplicación la pantalla está limpia: **elegir primero el proveedor A** y agregar P1).
- Pasos: observar la pestaña Compras; desplazar hasta el final.
- Resultado esperado: aparece barra **vertical** y **ninguna** horizontal; desplazando se alcanza el pie con sus tres botones; la columna «Quitar» se ve completa.
- Tiempo máximo: 20 s (arranque); 10 s por desplazamiento.
- Éxito: se cumple todo. Fallo: barra horizontal, pie inalcanzable o «Quitar» recortada.

**V-14 — Hipótesis: redimensionar con el panel desplazado.** Origen: hallazgo del Reviewer (2026-11-01), **hipótesis sin verificar**: `AjustarTamanoTablaPrincipal` fija `Location (12, 12)` sin considerar `AutoScrollPosition` del panel.
- Precondiciones: las de V-13 (con barra vertical visible).
- Pasos: (1) desplazar el panel hasta el final; (2) sin volver arriba, maximizar la ventana; (3) restaurarla; (4) volver a desplazar hasta arriba y hasta abajo.
- Resultado esperado: tras cada cambio de tamaño, el contenido queda alineado con el inicio del área desplazable: se puede volver arriba y ver el encabezado completo, y abajo se alcanza el pie; no aparecen huecos en blanco arriba ni contenido inalcanzable.
- Tiempo máximo: 10 s por cambio de tamaño; 120 s el escenario.
- Éxito: comportamiento esperado (la hipótesis **no** se confirma). Fallo: hueco en blanco, encabezado o pie inalcanzables o tabla corrida (la hipótesis **se confirma**: registrar captura y pasos exactos).
- Al terminar: volver a escala 100 % y reiniciar la aplicación antes de la sección D.

### D. Búsquedas

**V-15 — Búsqueda de proveedores.** Origen: T9b punto 3; T16; RF-9, RF-10, RF-11, RF-12.
- Precondiciones: seed actual (A, B activos; «Tecnologia Norte» inactivo).
- Pasos: (1) cuadro vacío o solo espacios; (2) `30123456` (fragmento del CUIT de A que **no** coincide con B ni con el inactivo: no usar `123456789`, que está en A y en B); (3) `logitech` (razón social de B en minúsculas); (4) `27123456789` y luego `Tecnologia`; (5) un texto sin coincidencias (por ejemplo `zzzz`).
- Resultado esperado: (1) listado vacío, sin mensaje y sin «sin resultados»; (2) aparece **solo A**; (3) aparece **solo B** (coincidencia sin distinguir mayúsculas); en ambos casos solo activos, ordenados por razón social; (4) "Tecnologia Norte" (inactivo) **no** aparece; buscar `Norte` devuelve solo "Distribuidora Norte SRL" (A); (5) listado vacío con el mensaje del negocio.
- Tiempo máximo: 10 s por búsqueda; 120 s el escenario.
- Éxito: los cinco casos como se espera. Fallo: aparece un inactivo, falta un mensaje, aparece un mensaje con el cuadro vacío o vence un plazo.

**V-16 — Búsqueda de productos.** Origen: T16; RF-15, RF-16, RF-17.
- Precondiciones: seed actual.
- Pasos: (1) cuadro vacío; (2) fragmento del código de P1; (3) fragmento del nombre de P2 en otra combinación de mayúsculas; (4) texto sin coincidencias.
- Resultado esperado: (1) listado vacío sin mensaje (o el listado del proveedor elegido, si lo hubiera: ver V-33); (2) y (3) coincidencias parciales sin distinguir mayúsculas; (4) listado vacío con el mensaje del negocio.
- Tiempo máximo: 10 s por búsqueda; 120 s el escenario.
- Éxito: los cuatro casos como se espera. Fallo: resultados incorrectos, mensajes ausentes o con cuadro vacío; o vence un plazo.

**V-17 — Producto inactivo excluido de la búsqueda.** Origen: T16; RF-15.
- Precondiciones: seed actual ampliada (incluye el producto inactivo `7790003000001`, «Producto Inactivo Prueba»; seed ejecutada antes del checklist, una sola vez).
- Pasos: buscar `7790003000001` y luego `Producto Inactivo`.
- Resultado esperado: el producto inactivo **no** aparece.
- Tiempo máximo: 10 s.
- Éxito: no aparece. Fallo: aparece. **Ahora ejecutable** (antes: `NO EJECUTADO`).

### E. Carga de líneas (sin compras registradas todavía)

**V-18 — Historial vacío en el modal.** Origen: RF-51; D-4 (modificado 2026-11-01).
- Precondiciones: **seed actual sin compras** (ejecutar antes de V-26).
- Pasos: pulsar «Ver Historial»; observar; cerrar el modal.
- Resultado esperado: se abre el modal «Historial de compras» con la grilla vacía y un mensaje en español que indica que todavía no hay compras, sin filas de ejemplo ni error.
- Tiempo máximo: 10 s para abrir el modal; 30 s para que aparezca el mensaje.
- Éxito: modal vacío con el mensaje. Fallo: filas de ejemplo, error o el mensaje no aparece en plazo.

**V-19 — Proveedor sin historial.** Origen: T21 apartado 10 (comportamiento); RF-66; D-22.
- Precondiciones: **seed actual sin compras** (ejecutar antes de V-26); pantalla limpia (si hay líneas o proveedor: «CANCELAR COMPRA» → «Sí»); cuadro «Buscar producto» vacío. (Regla B-1: la línea solo puede cargarse con proveedor elegido; por eso el orden de los pasos.)
- Pasos: (1) con «Buscar producto» vacío, buscar y elegir el proveedor A en «Buscar proveedor»; (2) buscar P1 en «Buscar producto», elegirlo y agregarlo con cantidad 1 (queda una línea); (3) buscar y elegir el proveedor B en «Buscar proveedor» (reemplaza a A); (4) dejar «Buscar producto» vacío (borrar el texto si quedó alguno).
- Resultado esperado: (1) se muestran razón social y documento de A; el listado de productos queda vacío con el mensaje del negocio (A no tiene historial); (3) se muestran razón social y documento de B; la línea de P1 **se conserva** y el total no cambia (mientras «Buscar producto» tenga texto, el listado de productos sigue la búsqueda: V-33); (4) con el cuadro vacío, el listado de productos queda vacío con el mensaje del negocio (B tampoco tiene historial).
- Tiempo máximo: 10 s por acción.
- Éxito: se cumple todo. Fallo: se pierde la línea, falta el mensaje o vence el plazo. (La conservación del área de captura al cambiar de proveedor se verifica en V-32.)
- Al terminar: «CANCELAR COMPRA» → «Sí».

**V-20 — Prellenado editable y foco en «Cantidad».** Origen: T9b punto 6; T12; RF-38.
- Precondiciones: seed actual (anotar si el producto elegido tiene precios en 0 en la seed: en ese caso se prellena 0 y el rechazo posterior es el esperado por RF-22/RF-23).
- Pasos: buscar y elegir P2; observar los precios y el foco; modificar el precio de compra.
- Resultado esperado: precio de compra y de venta del catálogo cargados en sus campos; el cursor queda en «Cantidad»; los precios son editables y prevalece el valor modificado.
- Tiempo máximo: 10 s.
- Éxito: se cumple todo. Fallo: precios vacíos o no editables, foco en otro control o vence el plazo.

**V-21 — Subtotal en preparación y total.** Origen: T9b punto 10; RF-65.
- Precondiciones: seed actual; pantalla limpia; **proveedor A elegido primero**; una línea ya agregada (anotar el total).
- Pasos: con un producto elegido, escribir en «Cantidad» `0`, luego `abc`, luego un valor válido, sin pulsar «Agregar a la compra».
- Resultado esperado: con `0` y `abc` el subtotal muestra «—»; con un valor válido muestra el subtotal; el **total general no cambia** en ningún caso hasta agregar la línea.
- Tiempo máximo: 10 s por cambio.
- Éxito: se cumple todo. Fallo: subtotal con entrada inválida o total alterado por la línea en preparación.

**V-22 — Agregar, editar, quitar y volver a agregar.** Origen: T9b punto 7; T16; RF-43.
- Precondiciones: seed actual; pantalla limpia (si hay líneas o proveedor: «CANCELAR COMPRA» → «Sí»); **proveedor A elegido primero** (guardar la edición también exige proveedor elegido).
- Pasos: (1) agregar P1 y P2; (2) «Editar» la línea de P2, cambiar la cantidad y guardar; (3) «Quitar» la línea de P2; (4) volver a elegir P2.
- Resultado esperado: (2) la línea se **reemplaza** sin duplicarse y el total se recalcula; (3) la línea desaparece y el total se recalcula; (4) P2 vuelve a prellenarse con los precios del catálogo.
- Tiempo máximo: 10 s por acción; 120 s el escenario.
- Éxito: se cumple todo. Fallo: línea duplicada, total sin recalcular o prellenado ausente.

**V-23 — Acumulación y su máximo.** Origen: T9b punto 13; RF-36, RF-63.
- Precondiciones: seed actual; pantalla limpia; **proveedor A elegido primero**; línea de P1 con cantidad 999999.
- Pasos: (1) agregar P1 con cantidad 1; (2) agregar P1 con cantidad 2.
- Resultado esperado: (1) se acumula en la misma línea (1.000.000) con aviso; (2) se rechaza con «La cantidad acumulada supera el máximo permitido.», la línea queda intacta y el total no cambia.
- Tiempo máximo: 10 s por acción.
- Éxito: se cumple todo. Fallo: dos líneas de P1, acumulación por encima del máximo o mensaje distinto.

**V-24 — Proveedor reemplazable y estado de «REGISTRAR COMPRA».** Origen: T9b punto 3; T18 apartado E; RF-12, RF-13, RF-58.
- Precondiciones: seed actual (sin compras: V-24 corre antes de V-26, así que elegir un producto no autocompleta proveedor); pantalla limpia (si hay líneas o proveedor: «CANCELAR COMPRA» → «Sí»).
- Pasos: (1) sin proveedor ni líneas, observar «REGISTRAR COMPRA»; (2) buscar y elegir P1, escribir cantidad 1 y pulsar «Agregar a la compra» **sin proveedor elegido** (comportamiento vigente, B-1: pendiente de decisión del usuario); (3) elegir el proveedor A, con P1 aún en la captura, y pulsar «Agregar a la compra» (queda una línea); (4) reemplazar A por B.
- Resultado esperado: (1) botón **deshabilitado**; (2) la línea **no** se agrega, la pantalla muestra «Seleccione el proveedor...» junto a la búsqueda de proveedores, el foco pasa a «Buscar proveedor» y el botón sigue **deshabilitado**; (3) la línea se agrega, el botón queda **habilitado** y A visible; (4) B visible, **sin perder** líneas, cantidades ni precios, y el botón sigue habilitado.
- Tiempo máximo: 10 s por acción.
- Éxito: se cumple todo. Fallo: botón habilitado sin ítems o sin proveedor, línea agregada sin proveedor (si B-1 se mantiene como regla), o pérdida de líneas al cambiar de proveedor. Si el usuario decide quitar la regla B-1, el paso (2) y su esperado se reemplazan por «la línea se agrega sin proveedor y el botón sigue deshabilitado».

**V-25 — Cancelar compra con confirmación.** Origen: T9b punto 4; RF-31.
- Precondiciones: proveedor elegido y líneas cargadas (estado de V-24).
- Pasos: pulsar «CANCELAR COMPRA»; responder «No»; volver a pulsar y responder «Sí».
- Resultado esperado: aparece la confirmación del negocio; con «No» nada cambia; con «Sí» se descartan proveedor, líneas y captura.
- Tiempo máximo: 10 s.
- Éxito: se cumple todo. Fallo: sin confirmación, descarte con «No» o descarte incompleto con «Sí».

**V-35 — Validaciones de cantidad y precios al agregar, y decimales (punto agregado el 2026-11-02; se ejecuta entre V-25 y V-26).** Origen: RF-20, RF-21, RF-22, RF-24, RF-26, RF-27, RNF-11 (único punto manual de validaciones de importes; el resto lo cubre `dotnet test`).
- Precondiciones: seed actual; pantalla limpia (V-25 la deja limpia); **proveedor A elegido primero**; P1 elegido en la captura.
- Pasos (un solo clic en «Agregar a la compra» por caso, sin repetir): (1) cantidad `0`; (2) cantidad `1000001`; (3) cantidad `1` y precio de compra `0`; (4) cantidad `1` y precio de compra `10,333`; (5) cantidad `1` y precio de compra `1000000000`; (6) cantidad `3`, precio de compra `10,33` y precio de venta `20,00`.
- Resultado esperado: en (1) a (5) la línea **no** se agrega, aparece un mensaje del negocio en español (sin comparar el texto literal), la grilla y el total **no cambian** y lo escrito en la captura se conserva (RNF-11); en (6) se agrega la línea con subtotal **30,99** y total **30,99** (sin redondeos raros).
- Tiempo máximo: 10 s por caso; 120 s el escenario.
- Éxito: los seis casos como se espera. Fallo: se agrega una línea inválida, falta el mensaje, se pierde lo escrito o el subtotal/total difiere de 30,99; o vence un plazo.
- Al terminar: «CANCELAR COMPRA» → «Sí» (V-26 requiere pantalla limpia).

### F. Registro (efectos secundarios: no repetir sin verificar)

**V-26 — Registrar C1 y descarte posterior.** Origen: T9b punto 5; RF-32.
- Precondiciones: V-18 y V-19 ya ejecutados; pantalla limpia (si hay líneas o proveedor: «CANCELAR COMPRA» → «Sí»).
- Pasos: elegir A; agregar P1 y P2 con cantidades y precios válidos; pulsar **una vez** «REGISTRAR COMPRA».
- Resultado esperado: mensaje de éxito; proveedor, líneas y captura quedan vacíos.
- Tiempo máximo: 30 s (operación funcional).
- Éxito: compra registrada y pantalla limpia. Fallo: mensaje de error, pantalla no limpia o vence el plazo. **Ante timeout o error, no volver a registrar**: aplicar la sección 0, punto 5.

**V-27 — Doble envío con C2.** Origen: T9b punto 9; RF-58.
- Precondiciones: V-26 superado.
- Pasos: elegir B; agregar P1 y P3; hacer **doble clic** rápido en «REGISTRAR COMPRA».
- Resultado esperado: el botón se deshabilita durante el guardado; se registra **una sola** compra (se confirma en V-28: si la base no tenía compras previas (§1), exactamente C1 y C2; si ya las tenía, C2 y C1 son las dos primeras filas (orden de RF-50) y no aparece ninguna compra adicional creada durante este checklist).
- Tiempo máximo: 30 s.
- Éxito: una sola compra nueva. Fallo: dos compras nuevas, error o vence el plazo. **Ante timeout o error, no volver a registrar**: sección 0, punto 5.

### G. Historial y detalle en el modal `DetalleCompraForm`

**V-28 — Historial en el modal (reemplaza el punto 8 de `T9b-checklist.md`).** Origen: T9b punto 8 reescrito; T18 apartado D; RF-49, RF-50, RF-52, RNF-12; D-4 (modificado 2026-11-01).
- Precondiciones: **compras del checklist** (C1 y C2); una línea en preparación cargada en la pantalla de compras (para comprobar que no se altera).
- Pasos: pulsar «Ver Historial»; observar las filas; cerrar el modal.
- Resultado esperado: si la base no tenía compras previas (§1): exactamente C1 y C2; si ya las tenía: C2 y C1 son las dos primeras filas (orden de RF-50) y no aparece ninguna compra adicional creada durante este checklist; **C2 primero** y C1 después (fecha y hora descendente); columnas número, fecha y hora (`dd/MM/yyyy HH:mm`), proveedor, usuario (nombre y apellido), total y estado «Activa»; ninguna columna de comprobante ni de precio de venta; al cerrar, la compra en preparación sigue intacta; la compra registrada aparece **sin reiniciar** la aplicación.
- Tiempo máximo: 10 s para abrir el modal; 30 s para que se cargue el historial.
- Éxito: se cumple todo. Fallo: orden incorrecto, formato distinto, compras que no cumplen la condición anterior (sin compras previas: distinto de exactamente C1 y C2; con compras previas: C2 y C1 no son las dos primeras filas o aparece una compra adicional creada durante este checklist), o alteración de la compra en preparación.

**V-29 — Detalle de una compra en el modal.** Origen: T12; T16; RF-53, RF-54; D-17 (modificado 2026-11-01).
- Precondiciones: compras del checklist (C1).
- Pasos: en el modal del historial, seleccionar C1.
- Resultado esperado: se muestran número, fecha y hora, proveedor A, usuario, total y estado, y las líneas de P1 y P2 con código, nombre, cantidad, precio de compra y subtotal; **sin** precio de venta; sin acciones de edición, impresión ni exportación.
- Tiempo máximo: 30 s (carga del detalle).
- Éxito: se cumple todo. Fallo: falta un dato, aparece el precio de venta o existe una acción de edición.

**V-30 — Estado «Inactiva» y desempate por número.** Origen: RF-49, RF-50, RNF-12.
- **Estado: SIN VERIFICAR (no generable sin incumplir RF-48; decisión del usuario 2026-11-01).** Una compra inactiva y dos compras con idéntica fecha y hora solo podrían crearse insertando compras desde la seed o por SQL, y las compras solo las escribe la aplicación (RF-48). La UI no ofrece dar de baja una compra ni fijar la fecha; no se inventa un procedimiento. Lo único que la UI permite es registrar dos compras seguidas, cuyas fechas pueden coincidir en el minuto mostrado (`dd/MM/yyyy HH:mm`) pero no se garantiza ni se fuerza; si ocurre al ejecutar V-26 y V-27, anotar si el orden del historial (V-28) respeta el número descendente, sin darlo por cumplido el punto. El orden y el estado quedan cubiertos por `dotnet test`.
- Tiempo máximo: no aplica.
- Registro: `SIN VERIFICAR`.

### H. Selección bidireccional producto ↔ proveedor

> **Vigencia D-23 (2026-11-02)**: los resultados esperados de **V-19, V-31, V-32 y V-33** que mencionan autocompletar el proveedor, filtrar a los históricos o que la búsqueda de productos pasa al catálogo global quedan **sustituidos**: elegir un producto prioriza sus proveedores históricos (sin autocompletar ninguno), «Mostrar todos» amplía a todos los activos sin tope, el texto se aplica sobre el listado vigente, el elegido que no figura en el listado se conserva y se avisa, y un proveedor sin historial deja el listado de productos vacío solo mientras la casilla esté apagada. Se reescriben los pasos al ejecutar la verificación visual de D-23.

**V-31 — Producto → proveedores (uno, varios, ninguno).** Origen: T12; T16; RF-12, RF-13.
- Precondiciones: compras del checklist (C1 y C2); pantalla limpia (si hay líneas o proveedor: «CANCELAR COMPRA» → «Sí»); cuadros de búsqueda vacíos.
- Pasos (el proveedor aquí **no** se elige primero a propósito: en (1) lo autocompleta la pantalla, por lo que se cumple B-1): (1) buscar y elegir P2 sin proveedor elegido; luego escribir cantidad 1 y pulsar «Agregar a la compra» (queda una línea de P2); (2) **sin cancelar**, buscar y elegir P4; (3) pulsar «CANCELAR COMPRA», responder «Sí», y buscar y elegir P1.
- Resultado esperado: (1) se **autocompleta** el proveedor A (visible y reemplazable) y la línea de P2 se agrega; (2) A sigue elegido y la línea de P2 **se conserva** (elegir otro producto no descarta las líneas ya agregadas), y el listado de proveedores queda **libre** (P4 no tiene historial); (3) tras cancelar no hay proveedor ni líneas; el listado de proveedores se **filtra** a A y B y **no** se autocompleta.
- Tiempo máximo: 10 s por elección; 120 s el escenario.
- Éxito: los tres casos como se espera. Fallo: autocompletado incorrecto, filtrado incorrecto o pérdida de líneas o del proveedor en (2).
- Al terminar: «CANCELAR COMPRA» → «Sí» (queda P1 en la captura; V-32 empieza con pantalla limpia).

**V-32 — Proveedor → productos habituales.** Origen: T21 apartado 10 (comportamiento); RF-66; D-22.
- Precondiciones: compras del checklist; pantalla limpia (si hay líneas, proveedor o producto elegido: «CANCELAR COMPRA» → «Sí»); cuadros «Buscar producto» y «Buscar proveedor» vacíos.
- Pasos: (1) escribir en «Buscar producto» el código de P3 y elegirlo (el cuadro **queda con ese texto**; P3 solo fue comprado a B, así que se autocompleta B); (2) **borrar** el texto de «Buscar producto» (P3 sigue elegido en la captura); (3) escribir `30123456789` en «Buscar proveedor» y elegir A (reemplaza a B).
- Resultado esperado: (1) B queda elegido; (2) el cuadro queda vacío y P3 sigue elegido; (3) el listado de productos muestra P1 y P2 (habituales de A); P3 no está en ese listado pero **sigue elegido** con sus rótulos y su captura intactos. En este punto no hay líneas (la conservación de líneas al cambiar de proveedor se verifica en V-19, V-24 y V-31).
- Tiempo máximo: 10 s por paso.
- Éxito: se cumple todo. Fallo: listado distinto, pérdida del producto elegido, o vence el plazo.
- No cancelar al terminar: V-33 parte de este estado (proveedor A elegido, P3 en la captura, habituales de A visibles).

**V-33 — La búsqueda manda y cambios rápidos.** Origen: T21 apartados 7 y 10 (riesgos i y ii); RF-15, RF-16, RF-17; D-22.
- Precondiciones: estado de V-32 (proveedor A elegido, listado de habituales visible).
- Pasos (se indica en cada paso a qué cuadro o listado se refiere): (1) en el cuadro **«Buscar producto»** escribir un fragmento de P4; (2) **vaciar «Buscar producto»**; (3) en el cuadro **«Buscar proveedor»** escribir `123456789` para que el listado de **proveedores** muestre A y B; con «Buscar producto» **vacío**, seleccionar B y volver a seleccionar A rápidamente en ese listado de proveedores (A→B→A); (4) escribir otra vez el fragmento de P4 en **«Buscar producto»** y repetir A→B→A en el listado de **proveedores**; (5) **vaciar «Buscar producto»**.
- Resultado esperado: (1) el listado de **productos** muestra el **catálogo global** (coincidencias de P4) con A aún visible como proveedor elegido; (2) vuelve el listado de productos habituales de A (P1 y P2), sin parpadeo; (3) A queda elegido y el listado de productos sigue mostrando los habituales de A; (4) el listado de productos sigue correspondiendo al texto escrito en «Buscar producto» (no lo pisa el proveedor) y A queda elegido; (5) aparece el listado de habituales del proveedor elegido al final (A); la pantalla no se congela ni entra en parpadeo repetido.
- Tiempo máximo: 10 s por acción; 120 s el escenario.
- Éxito: se cumple todo. Fallo: el proveedor pisa la búsqueda, listado incoherente con el último texto o con el último proveedor, parpadeo repetido o congelamiento (en ese caso, sección 0).
- **Hallazgo I-2 (hipótesis, no confirmada)**: si durante A→B→A rápido (pasos 3 o 4) aparece el mensaje «problema con el origen de datos», registrarlo como **hallazgo I-2** (posible causa: un solo contexto de datos compartido con consultas concurrentes) con captura y pasos exactos, **no repetir** el A→B→A de ese paso y pasar al siguiente solo si la pantalla sigue operativa; no se clasifica como fallo definitivo del punto sin evidencia adicional.
- Nota: «sin consulta al vaciar» y «una consulta y cero cascadas al cambiar de proveedor» **no son observables a simple vista**; quedan cubiertos por la revisión estática del Reviewer (T21 apartado 10).

**V-34 — Encargado dado de baja con compras en el historial (PERMANENTE: registra la compra C3).** Origen: caso límite «Usuario del historial que ya no está activo» (`spec.md`, línea 379); RF-49, RNF-5. Verificado leyendo el código: la app sí ofrece la baja de usuarios (`GestionUsuariosControl`, botón «Desactivar»/«Activar» → `UsuarioNegocio.DesactivarUsuarioAsync`, baja lógica `Estado = false`); no se ejecutó nada.
- Precondiciones: V-26 a V-33 ya ejecutados (este punto va **al final** porque crea una compra más: C3); seed ampliada (usuario Encargado adicional 90000004, activo y sin compras); credenciales en el README; una sesión con acceso a la gestión de usuarios (Administrador).
- Pasos: (1) iniciar sesión como el usuario 90000004 y registrar **una** compra C3 (elegir **primero** el proveedor y luego una línea cualquiera; un solo clic en «REGISTRAR COMPRA»); (2) cerrar sesión; (3) iniciar sesión como Administrador, abrir la gestión de usuarios y desactivar al usuario 90000004 (confirmar); (4) en Compras, abrir «Ver Historial» y seleccionar C3.
- Resultado esperado: C3 se lista primero (la más reciente) con el **nombre y apellido** del usuario 90000004, sin error y sin ningún otro dato del usuario; el detalle de C3 se abre normalmente. (Opcional: reactivar al usuario con «Activar» para dejarlo como estaba; la compra C3 es permanente.)
- Tiempo máximo: 30 s para registrar C3; 10 s para abrir el modal; 30 s para cargar historial y detalle.
- Éxito: se cumple todo. Fallo: nombre ausente o reemplazado, error, otros datos del usuario visibles o vence un plazo. Ante timeout o error al registrar, no repetir: sección 0, punto 5.

---

## 3. Registro de resultados

| ID | Origen | Datos | Fecha | Resultado | Observaciones |
|---|---|---|---|---|---|
| V-01 | T17-A | Seed actual | | | |
| V-02 | T9b-2 | Seed actual | | | |
| V-03 | T9b-1, T9b-11 | Seed actual | | | |
| V-04 | T18-D, T19-D | Seed actual | | | |
| V-05 | T15, T16, T17-D | Seed actual | | | |
| V-06 | T21 §10 | Seed actual | | | |
| V-07 | T21 §0 (ajustes UX/UI) | Seed actual | | | |
| V-08 | T21 §10, T20-A, T15, T16, T17-D | Seed actual | | | |
| V-09 | T19-A, T18-B | Seed actual | | | |
| V-10 | T19-C | Seed actual | | | |
| V-11 | T19-C | Seed actual ampliada | | | |
| V-12 | T18-E, T21 §4 | Seed actual | | | |
| V-13 | T21 §10 (150 %) | Seed actual | | | |
| V-14 | Hipótesis del Reviewer (T21) | Seed actual | | | |
| V-15 | T9b-3, T16 | Seed actual | | | |
| V-16 | T16 | Seed actual | | | |
| V-17 | T16 | Seed actual ampliada | | | |
| V-18 | RF-51, D-4 | Seed actual (sin compras) | | | |
| V-19 | T21 §10, RF-66 | Seed actual (sin compras) | | | |
| V-20 | T9b-6, T12 | Seed actual | | | |
| V-21 | T9b-10 | Seed actual | | | |
| V-22 | T9b-7, T16 | Seed actual | | | |
| V-23 | T9b-13 | Seed actual | | | |
| V-24 | T9b-3, T18-E | Seed actual | | | |
| V-25 | T9b-4 | Seed actual | | | |
| V-35 | RF-20…RF-27, RNF-11 (agregado 2026-11-02; se ejecuta aquí, antes de V-26) | Seed actual | | | |
| V-26 | T9b-5 | Seed actual (crea C1) | | | |
| V-27 | T9b-9 | Seed actual (crea C2) | | | |
| V-28 | T9b-8 reescrito, T18-D | Compras del checklist | | | |
| V-29 | T12, T16, D-17 | Compras del checklist | | | |
| V-30 | RF-49, RF-50 | No generable sin incumplir RF-48 | | SIN VERIFICAR | Decisión del usuario 2026-11-01 |
| V-31 | T12, T16 | Compras del checklist | | | |
| V-32 | T21 §10, RF-66 | Compras del checklist | | | |
| V-33 | T21 §7 y §10 | Compras del checklist | | | |
| V-34 | Spec l.379, RF-49 | Compras del checklist + usuario 90000004 (crea C3) | | | |

**Resumen de datos** (35 puntos en total: V-01 a V-35): 28 puntos con la seed ampliada (incluidos V-11, V-17 y el agregado V-35), 6 con las compras creadas en el propio checklist (V-28, V-29, V-31, V-32, V-33 y V-34) y **1 SIN VERIFICAR** (V-30, no generable sin incumplir RF-48; decisión del usuario 2026-11-01).

**Cierre**: el checklist se considera completo cuando todos los puntos tienen fecha, resultado y observación. Un punto `NO EJECUTADO` deja **PENDIENTE** la tarea de origen; un punto `SIN VERIFICAR` por decisión documentada (V-30) no bloquea el cierre y debe constar así en el informe; un `FALLIDA` o `FAILED — TIMEOUT / POSIBLE BLOQUEO` la deja **FALLIDA** hasta su corrección y nueva verificación.
