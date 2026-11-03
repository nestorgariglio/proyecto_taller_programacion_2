# Tareas — Spec 004 Consulta de Stock e Inventario Crítico

> **Spec aprobada**: `specs/004-consulta-stock/spec.md` (2026-10-26). **Plan**: `specs/004-consulta-stock/plan.md`.
> **Regla de orden**: `CapaNegocio` (lógica + DTOs + tests) → `CapaPresentacion` (configuración + controles) → resaltado/aviso → verificación.
> **Sin migraciones, sin cambios de esquema, sin cambios de `PoliticaAcceso`, sin cambios de menú, sin paquetes NuGet.**
> **Nota de capas**: cada tarea toca una sola capa, salvo **T5**, donde la lectura de configuración y el texto que la muestra son una única unidad funcional (separarlas dejaría la fila de filtros a medio construir); se justifica en el plan §4e/§4f.

- [x] **T1. Resolución pura del umbral y enum de criticidad** — RF-9, RF-10, RF-20.
  - Crear `CapaNegocio/Enums/NivelCriticidadStock.cs` (`Normal`, `BajoUmbral`, `Agotado`).
  - Crear `CapaNegocio/Stock/ResolucionUmbralStock.cs`: `Resolver(string?)` con Recorte, solo dígitos `0-9`, `int.TryParse` invariante, rango `>= 0`; `null` ⇒ `5` sin degradar; inválido ⇒ `Valor = null` y `EsDegradado = true`.
  - Crear `CapaNegocio.Tests/ResolucionUmbralStockTests.cs` con los 12 casos del plan §5.1.
  - **Hecho cuando**: `dotnet build` sin errores; `dotnet test --filter FullyQualifiedName~ResolucionUmbralStockTests` con **12/12 en verde**; `CapaNegocio.csproj` sin referencias nuevas; la clase no usa `IConfiguration`, EF ni `ConfigurationManager`.

- [x] **T2. DTOs aditivos de consulta y criticidad** — RF-8, RF-11, RF-17.
  - `FiltroProductosDto`: agregar `bool? SoloCriticos` con documentación XML (`null` = sin restricción).
  - `ProductoRespuestaDto`: agregar `NivelCriticidadStock NivelCriticidad` con valor por defecto `Normal`.
  - **Hecho cuando**: `dotnet build` sin errores; `dotnet test` con la suite completa en verde (los 193 previos + los 12 de T1), sin tocar ningún test existente; inspección: solo cambian esos dos DTO, no se agregan datos sensibles y `ProductoModalForm`/`AjustarStockModalForm` no requieren cambios.

- [x] **T3. Tests de negocio de la consulta (fase roja)** — RF-6, RF-8, RF-11, RF-12, RF-13, RF-21, RNF-3, RNF-8.
  - Crear `CapaNegocio.Tests/ProductoNegocioConsultaStockTests.cs` (clase parcial `ProductoNegocioTests`, helpers existentes) con los casos del plan §5.2: clasificación (0, negativo, igual, 4 con umbral 5, sobre, umbral 0, sin umbral), "Solo críticos" (solo / combinado con texto, categoría y estado / sin resultados), prioridad del código exacto (primero, con espacios, inexistente, dos exactos sembrados, excluido por "Solo críticos"), orden Nombre/Código, `ContarProductosAsync` (0 y N) y solo lectura.
  - **Hecho cuando**: los tests nuevos **compilan** y fallan **solo** por la funcionalidad aún ausente (no por errores de compilación ni por helpers); `dotnet test` deja los 205 previos en verde y documenta los fallos esperados de los nuevos.

- [x] **T4. Listado con umbral, filtro de criticidad, prioridad y conteo** — RF-6, RF-8, RF-10, RF-11, RF-12, RF-13, RF-15, RF-21, RNF-3, RNF-8.
  - `ListarProductosAsync(FiltroProductosDto filtro, int? umbralStock = null)`: aplicar `SoloCriticos` en el `IQueryable` (`Stock <= 0` cuando el umbral es `null`; `Stock <= 0 || (Stock > 0 && Stock < umbral)` cuando hay valor); clasificar tras materializar; mover a la posición 0 la única coincidencia exacta de `Codigo`; conservar `OrderBy(Nombre).ThenBy(Codigo)`.
  - Agregar `ContarProductosAsync()` con `AsNoTracking().CountAsync()` sin filtros.
  - **Hecho cuando**: `dotnet test --filter FullyQualifiedName~ProductoNegocioConsultaStockTests` **todo en verde**; la suite completa en verde; `dotnet build` sin errores; inspección: el método no llama `SaveChanges` ni `DELETE`, mantiene `AsNoTracking` y no cambia el orden del resto de las filas.

- [x] **T5. Configuración del umbral y texto informativo** — RF-9, RF-10, RF-19, RF-23, RNF-5, RNF-6, RNF-9. *(validación manual)*
  - `appsettings.json`: agregar `"StockCritico": { "UmbralMinimo": "5" }`.
  - `ProductosForm`: nuevo parámetro `IConfiguration` (con `ArgumentNullException.ThrowIfNull`), campo `_resolucionUmbral` resuelto **una sola vez** en el constructor con `ResolucionUmbralStock.Resolver(_configuracion["StockCritico:UmbralMinimo"])`; `_panelFiltros` a 5 columnas (36/16/13/12/23); nuevo `Label` `_lblUmbral` con `BackColor = TemaAplicacion.FondoPrincipal`, `AutoSize = false`, `Dock = Fill`.
  - Texto: con umbral válido `"Umbral: N unid."`; en degradado, sin número, dos líneas: `"Umbral no disponible: configuración inválida."` / `"Solo se marca \"SIN STOCK\"."`.
  - **Hecho cuando**: `dotnet build` sin errores; inspección: una sola lectura de `StockCritico:UmbralMinimo`, la vista no parsea el texto (solo llama a `Resolver`), 0 `Color.Transparent`, 0 `Refresh()`/`Invalidate()`, exactamente 1 `PerformLayout()`; **checklist manual**: (a) con `"5"` se ve `Umbral: 5 unid.`; (b) con `"abc"` no se muestra número, aparece el aviso persistente y no se abre ningún modal; (c) al corregir el archivo y reabrir la vista se aplica el nuevo valor sin reiniciar la aplicación.

- [x] **T6. Casilla "Solo críticos" y paso del umbral a la consulta** — RF-6, RF-7, RF-11, RNF-9, D-13. *(validación manual)*
  - Agregar `MaterialCheckBox _chkSoloCriticos` ("Solo críticos") en la quinta columna, `BackColor = TemaAplicacion.FondoPrincipal`, **desactivada** al construir la vista.
  - `CargarProductosAsync`: `SoloCriticos = _chkSoloCriticos.Checked ? true : (bool?)null` y `await _productoNegocio.ListarProductosAsync(filtro, _resolucionUmbral.Valor)`; handler `CheckedChanged` que recarga; no reiniciar la casilla en `CargarCategoriasAsync` ni en `btnActualizar`.
  - **Hecho cuando**: `dotnet build` sin errores; inspección: la casilla solo se inicializa en `ArmarInterfazMaterial`, el filtro viaja en `FiltroProductosDto` y el umbral sale de `_resolucionUmbral`; **checklist manual**: arranca apagada, al encenderla quedan solo rojos/ámbar, se combina con texto/categoría/estado, y al cambiar de pestaña y volver conserva su estado.

- [x] **T7. Resaltado de filas, indicador, selección y atenuación** — RF-8, RF-14, RF-22, RNF-4, RNF-10, **D-17**. *(validación manual)*
  - Paleta **definitiva D-17** sobre fondo oscuro RGB(50,50,50): agotado `#B71C1C` (inactivo `#6E2020`), bajo umbral `#F9A825` (inactivo `#8C6D0A`), **texto blanco en los cuatro casos**. Pintado según plan §4g: `RowPrePaint` (fondo por `NivelCriticidad` y `Estado`), `CellFormatting` (`colStock` con `"{stock} · SIN STOCK"` / `"{stock} · BAJO"`, `SelectionBackColor` del nivel) y `CellPainting` (borde de selección de 2 px); conservar el formato `"Activo"/"Inactivo"` de `colEstado`.
  - **Hecho cuando**: `dotnet build` sin errores; 0 `Color.Transparent`; inspección: los cuatro hex son exactamente `#B71C1C`, `#6E2020`, `#F9A825` y `#8C6D0A`; **checklist manual**: stock 0 ⇒ fila `#B71C1C` con `0 · SIN STOCK`; stock negativo ⇒ `#B71C1C` con `-N · SIN STOCK`; stock 4 con umbral 5 ⇒ `#F9A825` con `4 · BAJO`; stock 5 ⇒ sin color; agotado inactivo ⇒ `#6E2020`; bajo umbral inactivo ⇒ `#8C6D0A`; texto blanco legible en los cuatro; captura en **escala de grises** con cantidad e indicador legibles; fila resaltada seleccionada ⇒ conserva relleno y se distingue **solo** por un borde de 2 px, con el texto blanco intacto (D-18); el resto de las filas mantiene su estilo.

- [x] **T8. Escaneo exacto y categoría que desaparece** — RF-21, RF-24, RNF-8. *(validación manual)*
  - Tras asignar `DataSource`: si hay filas y **no** hay más de una coincidencia exacta de `Codigo` con el texto recortado, seleccionar la fila 0 con `CurrentCell`; si hay más de una, dejar la grilla sin selección.
  - `CargarCategoriasAsync`: detectar que la categoría seleccionada ya no está, seleccionar "Todas", recargar e informar en español **una vez** por evento de refresco.
  - **Hecho cuando**: `dotnet build` sin errores; inspección: `CurrentCell` explícito, rama de dos coincidencias sin selección, aviso de categoría fuera del `TextChanged`; **checklist manual**: escanear un código existente ⇒ producto primero y seleccionado (si es crítico, además resaltado); escanear un código inexistente ⇒ sin selección y mensaje de "ningún producto coincide"; desactivar la categoría elegida y volver a la pestaña ⇒ filtro en "Todas", listado recalculado y aviso en español.

- [x] **T9. Mensajes de vacío y fallo de base de datos** — RF-13, RF-16, **D-16**. *(validación manual)*
  - Guardar en la vista el texto de vacío: si el listado vuelve vacío, llamar **una vez** a `ContarProductosAsync()` y elegir `"No hay productos registrados."` (0) o `"Ningún producto coincide con los filtros aplicados."` (> 0).
  - Dibujar el texto centrado sobre la grilla en su evento `Paint` con `TextRenderer.DrawText` cuando no hay filas (D-16: **sin** `Refresh()`/`Invalidate()`, **sin** un `Label` y sin ningún control nuevo); limpiarlo al haber filas.
  - Mantener el `try/catch` existente: con listado previo se conserva; en la primera carga la grilla queda vacía con el mensaje y los filtros intactos.
  - **Hecho cuando**: `dotnet build` sin errores; 0 controles nuevos fuera de los dos autorizados (RNF-5); **checklist manual**: catálogo vacío ⇒ "No hay productos registrados."; filtros sin coincidencias ⇒ el otro mensaje; en ambos se ven encabezados y filtros y no se resetean; fallo de BD con listado previo ⇒ se conserva el listado y aparece el mensaje en español; fallo en la primera carga ⇒ grilla vacía con mensaje y filtros conservados.

- [ ] **T10. Verificación final de la spec** — RF-1, RF-2, RF-3, RF-4, RF-5, RF-7, RF-15, RF-17, RF-18, RNF-1 a RNF-10. *(manual + inspección)*
  - **Hecho cuando**: `dotnet build` con **0 errores**; `dotnet test` con **toda la suite en verde** (sin rojos ni omitidos); inspección con conteo exacto: **0** `Color.Transparent`, **0** `Refresh()`/`Invalidate()`, **exactamente 1** `PerformLayout()`, **2** controles nuevos, mensaje de vacío solo en `Paint` (D-16), paleta exacta `#B71C1C`/`#6E2020`/`#F9A825`/`#8C6D0A` (D-17), selección por borde de 2 px con texto blanco (D-18), `ProductosForm` sin cálculo de criticidad ni parseo del umbral; `git status` sin migraciones ni cambios en `AppDbContext`, `PoliticaAcceso` o el menú; **checklist manual completo** del plan §5.3, incluidos: cinco columnas exigidas más P. Compra/Estado, búsqueda por nombre y por código, filtros combinados, los dos mensajes, aviso de degradado, atenuación en escala de grises, conservación de filtros al cambiar de pestaña, fallo de BD en ambos escenarios y **≤ 500 ms con 1.000 productos**. La medición de RNF-1 se hace contra la base aparte **`DB_SISTEMA_VENTA_PERF`** (1.000 productos) apuntando la app con la variable de entorno **`ConnectionStrings__CadenaSQL`**, sin editar `appsettings.json`; el resultado ya obtenido (**mediana 15,2 ms / máx 36,3 ms**) cubre la parte de **consulta + mapeo** (cota inferior, plan §5.4), por lo que lo que resta de esta tarea es la **comprobación visual** del pintado de la grilla.

## Correcciones (fase 6, validación de implementación)

> Veredicto de la validación RF por RF: **implementación apta** con 4 hallazgos menores. Se corrigen 3 en código y se agregan 2 tests; **RF-22** queda resuelto por redacción en `spec.md` (fuera de esta fase).

- [x] **C1. Texto del aviso degradado** — RF-23 · hallazgo: el aviso no aclaraba que "SIN STOCK" incluye stock 0 o negativo. `ObtenerTextoUmbral` devuelve ahora `"Umbral no disponible: configuración inválida."` / `"Solo se marca \"SIN STOCK\" (0 o negativo)."`, manteniendo las dos líneas, sin número y sin modal. El modo normal (`Umbral: N unid.`) no cambia.
- [x] **C2. Fallo de BD en la primera carga** — RF-16 · hallazgo: `CargarCategoriasAsync` retornaba antes de que el listado guardara su mensaje, así que con la BD caída al abrir la pestaña la grilla quedaba vacía sin texto. El `catch` de categorías deja ahora `_mensajeVacio = MensajeErrorCarga` (dibujado por el `Paint` de la grilla vacía) y conserva un único modal de error en español.
- [x] **C4. "Actualizar" detecta la categoría desaparecida** — RF-24 · hallazgo: la detección vivía solo en `CargarCategoriasAsync`. `btnActualizar_Click` reutiliza `CargarCategoriasAsync` (repuebla las activas por Id, detecta la desaparición, recarga el listado y avisa una sola vez por acción) sin perder texto, estado ni "Solo críticos"; con una categoría vigente no emite ningún aviso.
- [x] **C5. Tests de negocio faltantes** — RF-10, RF-21 · hallazgo: ramas sin cobertura. Se agregan en `ProductoNegocioConsultaStockTests`: `ListarProductos_SoloCriticosSinUmbral_DevuelveSoloLosAgotados` (modo degradado: solo `Stock <= 0`, sin `BajoUmbral` ni stock positivo) y `ListarProductos_CodigoInexistenteYCoincidenciaParcial_MantieneElOrdenHabitual` (coincidencia parcial sin código exacto ⇒ orden Nombre→Código, sin priorización). Suite en **230/230**.

> **T10 sigue pendiente**: validación visual y medición de RNF-1 (≤ 500 ms con 1.000 productos).
