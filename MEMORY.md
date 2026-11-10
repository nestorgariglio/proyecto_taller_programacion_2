# MEMORY.md — SISTEMA COMERCIAL G70

Memoria global del proyecto. Máximo aproximado: 50 líneas. Uso y actualización a cargo del Coordinator.

## Proyecto
- Stack: .NET 10, C#, WinForms, MaterialSkin 2, EF Core y SQL Server.
- Arquitectura: cuatro capas. Autoridad: `docs/constitution.md`.
- Specs 001–004: registradas previamente como implementadas y validadas; consultar sus documentos antes de modificar funcionalidades existentes.
- BD de desarrollo informada: `DB_SISTEMA_VENTA`, instancia `localhost\SQLEXPRESS`.

## Prioridad vigente: Spec 005 — HU-09
- HU-09 sigue siendo la prioridad. HU-10 y Spec 006 están estacionadas.
- C-1: manejo de errores de lecturas de BD implementado. Última validación: build sin errores y pruebas focalizadas de Compras 275/275 aprobadas.
- Filtrado bidireccional producto ↔ proveedor en Compras: implementado, compilado y testeado. Probado manualmente por el usuario en términos generales, queda aceptado; las mejoras de usabilidad quedan para más adelante. El historial de compras solo prioriza (nunca restringe); «Mostrar todos» (checkbox en la fila del título de cada tarjeta) lista todos los activos, sin tope ni paginación. Sin autoselección. Los requisitos RF-12, RF-66 y D-22 se reemplazan por RF-67 y D-23 (ver `specs/005-carga-compras/`). Sin verificar: registrar por primera vez una combinación nueva y que pase a ser prioritaria; revisión de Reviewer no realizada; Designer de Visual Studio.
- DECISIÓN CERRADA: la interfaz visual actual de `ComprasForm` (catálogo a la izquierda, documento de compra a la derecha, diseño aprobado por el usuario) no se rediseña. Ningún agente debe proponer rediseños; solo se permiten ajustes puntuales necesarios para integrar nuevos controles o comportamientos, conservando distribución y criterios visuales.
- BD de desarrollo: `/preparar-bd` reparado (modelo EF sin cambios pendientes, migraciones aplicadas, seed ejecutada e idempotente; verificación por claves determinísticas de la seed). La seed restablece solo sus propios registros; ver `scripts/README-preparar-bd.md`.
- Documentación/checklist de cierre: comprobar los archivos reales antes de afirmar que quedaron actualizados. `spec.md`, `plan.md`, `tasks.md` y los dos checklists se actualizaron para el filtrado bidireccional (RF-9, RF-10, RF-12, RF-15, RF-16, RF-66, RNF-3, D-22; nuevos RF-67 y D-23).
- B-1 (compuerta de proveedor) y la redacción de RF-34/RNF-9 no deben darse por resueltas ni modificarse en esta refactorización. RNF-9 y las pruebas de carga/rendimiento fueron descartadas por decisión del usuario.
- T10 de atomicidad: ejecución manual reservada al usuario. Ningún agente debe ejecutarla ni modificar la BD durante esa prueba.
- Pendiente de usabilidad en Compras (sin urgencia): mejoras sobre la selección y el filtrado, a definir por el usuario tras sus pruebas.

## Issue stock/precios al confirmar compra (HU-10, Flujo D)
- IMPLEMENTADO, COMPILADO y TESTEADO (InMemory, CompraNegocioTests 280/280); Reviewer: CON OBSERVACIONES, sin defectos. Spec en `specs/005-carga-compras/anexo-hu10-stock-precios.md`.
- Diseño: `CompraNegocio.AplicarEfectosSecundariosAsync(Compra, lineas)` dentro de la transacción de registro: con Estado=1 suma Stock y sobrescribe PrecioCompra/PrecioVenta de la línea. No existe edición/anulación de compras confirmadas (si se crea, definir ajuste por diferencia).
- SIN VERIFICAR: atomicidad/persistencia en SQL Server real (T10, del usuario); guarda Estado=false sin test directo (hook privado, sin InternalsVisibleTo); posible overflow int de Stock sin tope.

## Proveedores: razón social y CUIT concurrente (Flujo A, PR review)
- IMPLEMENTADO, COMPILADO y TESTEADO (InMemory, ProveedorNegocioTests 65/65); Reviewer: APROBADO con observaciones.
- Razón social: admite letras, dígitos, espacio y `. , ' - & / ( ) +`, 2–200 caracteres (columna varchar(200)); rechaza `@` y controles. Sin modificar el texto salvo Trim.
- CUIT duplicado concurrente: en el catch de alta/edición se re-evalúa `ValidarUnicidadCuitAsync` (patrón de `ClienteNegocio`); si existe → `CuitDuplicado`, si no o el origen cae → `ErrorPersistencia`. No se clasifica por tipo de excepción. Correo duplicado concurrente sigue como error genérico (observación, fuera de alcance).
- SIN VERIFICAR: rechazo real de `IX_PROVEEDOR_cuit` en SQL Server (reservado al usuario). Sin tests: salto de línea final (el Trim lo recorta), límite exacto de 200, rechazo de razón social en edición.

## Proveedores: advertencia de Razón Social posiblemente duplicada (Flujo A)
- TERMINADA. IMPLEMENTADO, COMPILADO y TESTEADO (89/89 InMemory); Reviewer verificó lógica por tests y lectura. UI VERIFICADA manualmente por el usuario: REVISAR vuelve al formulario y permite editar; CONTINUAR IGUALMENTE registra el proveedor con razón social coincidente y CUIT distinto. No verificado manualmente (informado): edición sin cambio de razón social, inactivos y mensaje exacto.
- Razón Social NO es única. Advertencia no bloqueante: `RazonSocialPosiblementeDuplicada` + `Coincidencia` (razón social, CUIT, estado); el DTO trae `ConfirmarRazonSocialDuplicada` para continuar. Comparación en `CapaNegocio/ComparadorRazonSocialProveedor.cs` (Trim, espacios, mayúsculas, sufijo final S.A./SA y S.R.L./SRL; acentos y puntuación conservados). Incluye inactivos; en edición excluye al propio y solo advierte si la razón social cambió respecto de la original.
- Orden: CUIT/correo se validan antes que la advertencia; la protección de CUIT concurrente no cambió.
- Bloque `if` vacío de `CrearProveedorAsync` eliminado (IMPLEMENTADO, COMPILADO, TESTEADO 74/74; Reviewer VERIFICADO). Observaciones de calidad pendientes, sin urgencia ni aplicar sin autorización: `catch { }` vacío y `?.` innecesarios en `DesadjuntarSiEsPosible` (propuesta: dejar solo `_context.Entry(p).State = Detached` o `catch (InvalidOperationException)` comentado); `CrearProveedorAsync` largo (~45 líneas) con `!` apoyados en una validación implícita; duplicación estructural con `ModificarProveedorAsync`.

## Compras: concurrencia de lecturas sobre AppDbContext (Flujo A)
- IMPLEMENTADO, COMPILADO y TESTEADO (CompraNegocio 282/282 InMemory; 7 pruebas nuevas en `CompraNegocioLecturasConcurrentesTests.cs`). Reviewer: CON OBSERVACIONES, sin defectos; reprodujo el `InvalidOperationException` de uso concurrente con contexto compartido y su ausencia con fábrica.
- Diseño: `Program.cs` usa solo `AddDbContextFactory<AppDbContext>` (AppDbContext sigue scoped; decisión del usuario). `CompraNegocio(AppDbContext, IDbContextFactory<AppDbContext>? fabricaLectura = null)`; `EjecutarLecturaAsync` abre un contexto por lectura en BuscarProveedoresParaCompraAsync, BuscarProductosParaCompraAsync y ObtenerPreciosProductoAsync. Escrituras, transacciones, historial y comprobantes sin cambios (siguen con `_context`).
- Riesgo conocido: sin fábrica el negocio cae al contexto compartido (lo cubre solo la prueba de DI, que es réplica de Program.cs y usa reflexión sobre `_fabricaLectura`). Sin fábrica con contexto histórico/«Mostrar todos» no se probó. Lecturas de historial/detalle y validaciones de líneas siguen en `_context` (observación, fuera de alcance).
- SIN VERIFICAR: ejecución real de ComprasForm contra SQL Server (reservada al usuario: escribir rápido en ambos buscadores); DI de Program.cs literal.
- TERMINADA: el usuario comprobó en la app real que escribir lo más rápido posible en ambos buscadores no produce errores. Verificación manual del usuario; no se registró si probó con «Mostrar todos» marcado.

## Compras: no se puede deseleccionar proveedor/producto (Flujo A, en propuesta)
- Causa: `_idProveedorElegido`/`_idProductoElegido` en `ComprasForm` solo se anulan al registrar/limpiar compra (`LimpiarTrasRegistro`) o tras agregar línea (`LimpiarCaptura`, solo producto); vaciar el buscador no deselecciona y el id sigue como contexto histórico en las búsquedas. Respuestas asíncronas: el descarte por texto/contexto/«Mostrar todos» es correcto.
- IMPLEMENTADO, COMPILADO (0 errores) y TESTEADO (CompraNegocio 282/282). Reviewer: CON OBSERVACIONES. Arnés WinForms descartable con InMemory (ComprasForm real, 60/60 comprobaciones lógicas, capturas en `%TEMP%\G70\rev_quitar\out\`).
- Diseño: botones «Quitar» (`btnQuitarProveedor`, `btnQuitarProducto`) en la fila de título de cada tarjeta, entre título y «Mostrar todos». Quitar proveedor conserva líneas y deshabilita REGISTRAR; Quitar producto limpia precios y cantidad (string.Empty). Vaciar el buscador no deselecciona. Etiquetas visibles «Documento» → «CUIT» en ComprasForm (rótulo, texto inicial, encabezado de grilla); identificadores internos, DataPropertyName y DTO sin cambios.
- DEFECTO VISUAL ABIERTO (100 % DPI, ancho ~1100 px, dos columnas): el título «1 · Proveedor»/«2 · Producto» se parte y se recorta (79 px de 101 necesarios) y el botón Quitar queda de 30 px de alto. A 150 % y apilado, correcto. Ajuste sugerido por Reviewer: subir el umbral de apilado a 100 % (`UmbralApiladoCienPorCientoLogico`, 950) o ensanchar la columna del catálogo; pendiente de decisión del usuario.
- DECISIÓN DEL USUARIO: los botones permanentes «Quitar» en el título de los catálogos fueron un error de criterio UX y se reemplazan (propuesta pendiente de autorización): acción contextual «Deseleccionar» dentro de la fila elegida de cada grilla, sin controles permanentes. Mantener la semántica funcional ya autorizada (conservar líneas, textos, casillas y la otra selección). Ver propuesta vigente; no proponer de nuevo botones en el encabezado.
- «Deseleccionar» en fila (reemplazo autorizado): IMPLEMENTADO, COMPILADO (0 errores), TESTEADO (CompraNegocio 282/282). Archivos: `ComprasForm.cs`, `ComprasForm.Designer.cs`, `PintorAccionFila.cs` (nuevo). Reviewer reducido (10 min, un ancho 1300 px a 100 % DPI, InMemory): CON OBSERVACIONES. Verificado visualmente: sin selección limpio; fila elegida #BF360C con texto blanco y botón solo en ella (se traslada); hover/presionado distintos; al deseleccionar listas sin contexto y conserva la otra selección; resaltado y botón persisten sin foco; sin superposición con «Mostrar todos». Implementer aplicó los parches sin la presentación previa de AGENTS.md §5 (incumplimiento de proceso admitido).
- Prueba manual del usuario en la app real: «Deseleccionar» en fila y enlace de respaldo funcionan bien y son legibles; confirmó que la app cumple las cuatro comprobaciones consultadas (concurrencia, mensaje, legibilidad del enlace, botón en fila). TERMINADA para este alcance. Los puntos (a)-(d) siguientes quedan como observaciones históricas del Reviewer, no confirmadas como defectos en la app real; no reabrir sin nueva evidencia.
- Observaciones históricas: (a) el mensaje bajo la grilla se parte en dos líneas y se recorta cuando comparte fila con el enlace de respaldo; (b) contraste del enlace (255,187,160): Reviewer midió 1,63:1 sobre blanco, pero su arnés probablemente no aplicó el tema oscuro (Implementer midió fondo real 80,80,80 → 4,94:1); sin resolver; (c) la columna de acción se oculta por falta de ancho (medido: visible a 1366 px, oculta a 1100 px a 100 % DPI; a 1366x768 y 150 % probablemente solo se ve el enlace, SIN VERIFICAR); (d) sin verificar: 1366x768/150 %, estado con columna oculta, líneas/casillas tras deseleccionar en la nueva UI, Designer de VS, app real (usuario).
- SIN VERIFICAR: ejecución en la aplicación real (usuario), Designer de Visual Studio, clic físico, DPI intermedios. Observación cosmética: tras Quitar producto el mensaje conserva «Se propusieron los precios actuales del catálogo.».

## Agentes y hooks (.claude/agents)
- El hook Write|Edit del Coordinator (restringido a `MEMORY.md`) se aplicaba a todos los subagentes porque `settings.local.json` ejecuta la sesión con `"agent": "coordinator"`. Corregido por el usuario: el hook deja pasar a planner/reviewer/implementer (por `agent_id` y `agent_type`), que se rigen por sus propios hooks. Verificado: Coordinator sigue bloqueado fuera de `MEMORY.md`; Implementer pudo editar con Edit/Write.

## Restricciones y aprendizajes
- Git es responsabilidad exclusiva del usuario. Prohibido ejecutar cualquier comando Git, incluso de lectura.
- No ejecutar tests de carga, rendimiento ni experimentos descartados por el usuario.
- No ampliar el alcance con refactorizaciones o documentación secundaria.
- Verificar directamente el objetivo solicitado; los tests automatizados son evidencia complementaria.
- No declarar una corrección visual sin ejecutar la aplicación y observar el resultado.
- No exponer credenciales, hashes, tokens ni cadenas de conexión.
- PAUTA PERMANENTE (skills): en diagnóstico, implementación y revisión, cada agente identifica y usa todas las skills pertinentes de `.agents/skills/` (clean-code, code-quality-checklist, csharp-async, desktop-ux-patterns, dotnet-best-practices, ef-core, find-skills, grill-me, modern-csharp, sdd, windows-git-bash-testing, winforms-expert, winforms-ui-ux), sin cargar las irrelevantes. Reviewer evalúa además calidad, mantenibilidad, responsabilidades y buenas prácticas. Los informes finales indican qué skills consultó cada agente y qué criterios aplicó. Sin ampliar el alcance ni refactorizar en general.

## Referencias
- Requisitos y tareas: `specs/`.
- Política de pruebas: `docs/TESTING_POLICY.md`.
- Principios arquitectónicos: `docs/constitution.md`.
