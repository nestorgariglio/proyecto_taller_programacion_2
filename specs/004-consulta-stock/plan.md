# Plan técnico — Spec 004 Consulta de Stock e Inventario Crítico

> **Spec de origen**: `specs/004-consulta-stock/spec.md` — **aprobada (2026-10-26)**. Este plan implementa **solo** lo que la spec manda.
> **Alcance**: consulta de **solo lectura** sobre el listado de productos existente. **Sin migraciones, sin cambios de esquema, sin cambios de `PoliticaAcceso`, sin cambios de menú, sin paquetes NuGet nuevos.**
> **Regla del proyecto**: la interfaz no calcula reglas de negocio (RNF-6); mapeo explícito y lecturas `async/await` (RNF-7).

## 1. Responsabilidades por capa

### CapaEntidad — sin cambios
- No se toca `Producto` ni ninguna entidad. **No** se agrega stock mínimo por producto (D-2).
- La criticidad es un concepto de negocio, no un dato persistido: el enum vive en `CapaNegocio` (no en `CapaEntidad`).

### CapaDatos — sin cambios
- `AppDbContext` **no** se modifica; **no** se crean migraciones ni índices nuevos.
- La consulta sigue siendo `AsNoTracking()` sobre `DbSet<Producto>` con `OrderBy(Nombre).ThenBy(Codigo)` (RNF-8). El filtro de criticidad usa columnas existentes (`Stock`), por lo que no hace falta cambiar la persistencia.

### CapaNegocio — cambios
| Archivo | Cambio |
|---|---|
| `Enums/NivelCriticidadStock.cs` | **Nuevo**: `enum NivelCriticidadStock { Normal, BajoUmbral, Agotado }`. |
| `Stock/ResolucionUmbralStock.cs` | **Nuevo**: lógica pura. `readonly record struct ResolucionUmbralStock(int? Valor, bool EsDegradado)` con `static ResolucionUmbralStock Resolver(string? valorCrudo)`. Sin EF y sin dependencias de configuración. |
| `DTOs/Productos/FiltroProductosDto.cs` | **Modificado (aditivo)**: `bool? SoloCriticos`. |
| `DTOs/Productos/ProductoRespuestaDto.cs` | **Modificado (aditivo)**: `NivelCriticidadStock NivelCriticidad { get; set; } = NivelCriticidadStock.Normal;`. |
| `ProductoNegocio.cs` | **Modificado**: `ListarProductosAsync(FiltroProductosDto, int? umbralStock = null)` aplica "Solo críticos" en la consulta, clasifica cada fila y prioriza la coincidencia exacta de código; **nuevo** `ContarProductosAsync()` para distinguir catálogo vacío de filtros sin coincidencias. |
| `CapaNegocio.csproj` | **Sin cambios**: no se agregan paquetes de configuración. |
| `CapaNegocio.Tests/ResolucionUmbralStockTests.cs` | **Nuevo** (T1). |
| `CapaNegocio.Tests/ProductoNegocioConsultaStockTests.cs` | **Nuevo** (T3/T4), en la clase parcial `ProductoNegocioTests` con los helpers existentes. |

- **No** se toca el ABM (alta, edición, baja, reactivación), el ajuste de stock, `PoliticaAcceso`, `CategoriaNegocio` ni sus DTOs.

### CapaPresentacion — cambios
| Archivo | Cambio |
|---|---|
| `appsettings.json` | **Modificado**: se agrega `"StockCritico": { "UmbralMinimo": "5" }` (string, para ejercitar el parseo por dígitos sin depender del tipo JSON). |
| `Productos/ProductosForm.cs` | **Modificado**: recibe `IConfiguration` por DI, resuelve el umbral **una vez al abrir la vista**, agrega **dos** controles (casilla "Solo críticos" y texto del umbral), resalta filas con indicador, distingue la selección, prioriza/selecciona el escaneo exacto, muestra los dos mensajes de vacío, informa la caída de la categoría seleccionada y mantiene el aviso del modo degradado. |
| `Program.cs` | **Sin cambios**: `AddTransient<ProductosForm>()` resuelve el nuevo parámetro `IConfiguration` (ya registrado). |
| `Productos/AjustarStockModalForm.cs`, `Productos/ProductoModalForm.cs` | **Sin cambios**: leen `Stock`/`Estado`; la propiedad nueva no los afecta. |
| `Inicio/inicio.cs`, `Tema/TemaAplicacion.cs` | **Sin cambios**: se usa `TemaAplicacion.FondoPrincipal` para fondos opacos (RNF-5). |

## 2. DTOs y mapeos

- **`FiltroProductosDto.SoloCriticos` (`bool?`)**: mismo patrón que `Estado` (`null` = sin restricción). Es **aditivo**: los llamadores y tests existentes siguen compilando porque no lo informan. La vista envía `true` al estar encendido y `null`/`false` al estar apagado; ambos casos no restringen.
- **`ProductoRespuestaDto.NivelCriticidad` (enum)**: se elige **una clasificación excluyente** en lugar de tres banderas (`EsAgotado`, `EsBajoUmbral`, `EsNormal`) o de propiedades calculadas:
  - no admite estados contradictorios y la interfaz solo compara valores (RNF-6);
  - es **aditiva**: la grilla usa columnas con `AutoGenerateColumns = false` (no aparece una columna nueva), `AjustarStockModalForm` lee `Stock`/`Estado` y `ProductoModalForm` no la usa;
  - el valor por defecto `Normal` cubre el mapeo del ABM (`MapearProducto`) y las llamadas heredadas a `ListarProductosAsync` sin umbral.
- **Mapeo explícito**: `MapearProducto` deja `Normal`; en el listado la clasificación se asigna **después de materializar**, porque la proyección SQL no puede invocar código C#. La proyección mantiene las mismas columnas y el `join` de categoría.
- **Priorización del escaneo exacto**: no se agrega un DTO de resultado; el listado devuelto ya queda con la coincidencia exacta en la **primera posición** y la vista selecciona la primera fila (comportamiento que ya existe), con la excepción de la inconsistencia de RF-21 (ver §4c).
- **Datos sensibles**: no se agregan. Se conservan los identificadores técnicos que el ABM necesita (`IdProducto`, `IdCategoria`, `IdUsuarioModifica`) y no se exponen hashes ni credenciales (RF-17).

## 3. Persistencia

- **Sin cambios en `AppDbContext` y sin migraciones.** El criterio se expresa con la columna existente `Stock`:
  - agotado: `Stock <= 0`;
  - bajo umbral: `Stock > 0 && Stock < umbral`.
- El filtro "Solo críticos" se combina **en la misma consulta** con texto, categoría y estado (`IQueryable`), así la base devuelve solo las filas necesarias.
- El orden se mantiene en la consulta (`OrderBy(Nombre).ThenBy(Codigo)`, RNF-8); la única excepción es mover la coincidencia exacta al primer lugar después de materializar (O(n) sobre el resultado ya filtrado).
- `ContarProductosAsync()` es `AsNoTracking().CountAsync()` **sin filtros** y se invoca desde la vista **solo cuando el listado vuelve vacío**, para no sumar una consulta por tecla (RNF-1).
- La consulta no escribe: no hay `SaveChanges`, ni `DELETE`, ni movimientos de stock (RF-15, RNF-3).

## 4. Decisiones técnicas (con alternativa descartada)

### (a) Interpretación del umbral: lógica pura en `CapaNegocio`
- **Elegido**: `ResolucionUmbralStock.Resolver(string?)` en `CapaNegocio/Stock`. Reglas (RF-20):
  - `null` ⇒ `Valor = 5`, `EsDegradado = false` (D-10: falta la clave, no hay aviso);
  - en otro caso: Recorte y **solo dígitos** `'0'..'9'`, `int.TryParse` con `NumberStyles.None` y `CultureInfo.InvariantCulture`, y `>= 0`;
  - cualquier fallo (vacío, `"5.0"`, `"5,0"`, `"+5"`, `"-5"`, `"abc"`, overflow) ⇒ `Valor = null`, `EsDegradado = true`.
- **Descartado 1**: interpretar/validar en la vista. Pondría reglas de negocio en la UI (RNF-6) y no sería testeable con `dotnet test`.
- **Descartado 2**: agregar `Microsoft.Extensions.Configuration` a `CapaNegocio`. Prohibido sin aprobación (NuGet nuevo) y acopla negocio al proveedor de configuración.
- **Descartado 3**: leer configuración dentro de `ProductoNegocio` con `ConfigurationManager`. Dependencia oculta, difícil de testear y contraria a D-15 (lectura única).

### (b) Criticidad calculada en `CapaNegocio`, no en la grilla
- **Elegido**: `ProductoNegocio` clasifica cada fila con el umbral recibido, entrega `NivelCriticidad` y aplica el filtro en la consulta.
- **Descartado 1**: que la grilla compare `Stock` contra el umbral al pintar. Duplica la regla en la UI y puede divergir del filtro de la consulta.
- **Descartado 2**: calcular con `CASE` en SQL. No evita transportar el nivel al DTO y complica la traducción entre SQL Server e InMemory.

### (c) Priorización del código exacto: post-proceso en memoria
- **Elegido**: tras materializar el resultado **ya filtrado y ordenado**, se buscan las filas cuyo `Codigo` sea idéntico al texto recortado (comparación ordinal, ignorando mayúsculas):
  - exactamente **una** ⇒ se mueve a la posición 0; el resto conserva el orden;
  - **ninguna** ⇒ no se reordena;
  - **más de una** (imposible con la unicidad vigente) ⇒ **no se reordena** (RF-21).
- **Selección (presentación)**: la vista mantiene la autoselección de la primera fila que ya existe; antes de seleccionar, cuenta coincidencias exactas en el resultado y **no fuerza selección si hay más de una** (RF-21, rama de inconsistencia). Es una regla de **selección visual**, no de criticidad ni de umbral.
- **Por qué no afecta RNF-1/RNF-8**: O(n) sobre el resultado acotado (≤ 1.000 filas) y el orden relativo de las demás filas no cambia.
- **Descartado 1**: `ORDER BY CASE WHEN Codigo = @texto THEN 0 ELSE 1 END, Nombre, Codigo`. Depende de la traducción del proveedor y obliga a duplicar lógica para InMemory.
- **Descartado 2**: devolver la coincidencia exacta en un campo aparte. Cambia el contrato del listado y obliga a la vista a combinar dos fuentes.

### (d) Modo degradado sin duplicar el aviso: resultado tipado resuelto una vez
- **Elegido**: la vista resuelve el umbral **una sola vez al construir la vista**, guarda la `ResolucionUmbralStock` en un campo y:
  - pasa `resolucion.Valor` (`int?`) a `ListarProductosAsync`;
  - usa **ese mismo objeto** para el texto del umbral: `"Umbral: N unid."` o, si `EsDegradado`, un texto sin número que informa la causa y el criterio (`"SIN STOCK"`).
  El aviso es único, persistente, no modal y no se reemite por tecla.
- **Descartado 1**: `MaterialMessageBox` al detectar la degradación. Modal y repetible: prohibido por RF-23.
- **Descartado 2**: que `ListarProductosAsync` devuelva un segundo objeto con el estado del umbral. Cambia el contrato del listado y no evita el aviso repetido.
- **Descartado 3**: leer el valor por tecla. Contradice D-15 y RNF-9 (el texto puede cambiar mientras el criterio no).

### (e) Lectura de configuración en la presentación
- **Elegido**: `ProductosForm(CategoriaNegocio, ProductoNegocio, IConfiguration)` con `ArgumentNullException.ThrowIfNull`; lee `StockCritico:UmbralMinimo` en el constructor. `CapaPresentacion` ya referencia `Microsoft.Extensions.Hosting` (contiene `IConfiguration`), así que **no hay paquete nuevo**. `Program.cs` no cambia porque el contenedor resuelve el parámetro.
- **Descartado**: leer el archivo a mano. Duplica el arranque de configuración y rompe el patrón DI.

### (f) Dos controles nuevos y ninguno más
- **Elegido**: `_panelFiltros` pasa de 3 a 5 columnas (búsqueda 36 %, categoría 16 %, estado 13 %, casilla 12 %, texto 23 %). Se agregan `MaterialCheckBox` "Solo críticos" y un `Label` de texto del umbral, ambos con `BackColor = TemaAplicacion.FondoPrincipal`. El texto de degradación usa dos líneas explícitas para entrar en el ancho asignado.
- **Descartado**: agregar un contenedor/panel para agruparlos. RNF-5 autoriza **solo dos** controles nuevos; un contenedor lo excedería.

### (g) Resaltado, indicador y selección — **D-17 (decisión cerrada)**
- **Elegido**: paleta **definitiva** sobre el fondo oscuro RGB(50,50,50), aplicada en `RowPrePaint` (fondo de fila) y `CellFormatting` (`SelectionBackColor` del nivel + texto del indicador en `colStock`). La selección sobre una fila resaltada se dibuja como **borde de 2 px** en `CellPainting`, conservando el relleno (RF-22); **el color del texto no cambia** y sigue blanco (D-18). *(El `BackgroundColor` actual de la grilla es `Color.FromArgb(48,48,48)`; la paleta y el contraste no dependen de esa diferencia.)*
  - Agotado: **`#B71C1C`**; agotado inactivo (atenuado): **`#6E2020`**.
  - Bajo umbral: **`#F9A825`** (ámbar amarillo; se elige en lugar de `#EF6C00` para no confundirse con el naranja de selección de la grilla **`#FF5722`**); bajo umbral inactivo (atenuado): **`#8C6D0A`**.
  - **Texto blanco en los cuatro casos**; la cantidad y el indicador se mantienen legibles en color y en escala de grises (RNF-4, RNF-10).
  - Indicador en la celda de Stock: `"{stock} · SIN STOCK"` / `"{stock} · BAJO"`; la cantidad siempre visible.
- **Descartado 1**: cambiar `DefaultCellStyle.SelectionBackColor` por fila. Borra el color de criticidad al seleccionar (obs. 3).
- **Descartado 2**: deshabilitar la selección. Rompe el ABM, que depende de `CurrentRow`.
- **Descartado 3 (D-17)**: conservar `#EF6C00` para "bajo umbral". Queda demasiado cerca del naranja de selección `#FF5722` y la fila baja se confundiría con una fila seleccionada.

### (h) Dos mensajes de vacío sin un tercer control — **D-16 (decisión cerrada)**
- **Elegido**: si el listado vuelve vacío, la vista consulta `ContarProductosAsync()` una vez y guarda el mensaje en un campo:
  - total 0 ⇒ `"No hay productos registrados."`;
  - total > 0 ⇒ `"Ningún producto coincide con los filtros aplicados."`.
  El mensaje se **dibuja sobre el área de la grilla** en su evento `Paint` cuando no hay filas (`TextRenderer.DrawText` centrado), sin agregar un control y **sin** `Refresh()`/`Invalidate()` (el rebindeo del `DataSource` ya repinta).
- **Descartado 1 (D-16)**: agregar un `Label` de mensaje. Sería un tercer control y **no se autoriza**: RNF-5 se mantiene con **2** controles nuevos.
- **Descartado 2**: cambiar el retorno de `ListarProductosAsync` a un objeto con `Productos` + `CatalogoVacio`. Rompería los tests y consumidores existentes.
- **Descartado 3**: deducir el mensaje solo por "hay filtros activos". No distingue un catálogo vacío con filtros aplicados.

### (i) Aviso de categoría que desaparece
- **Elegido**: al reconstruir el combo, si había una categoría seleccionada y ya no está, se selecciona "Todas", se recarga el listado y se muestra **un** mensaje informativo en español por ese evento de refresco (no por tecla).
- **Descartado**: volver a "Todas" en silencio. La spec exige informar (RF-24).

### (j) Persistencia de "Solo críticos"
- **Elegido**: el estado vive en el control de la vista; se inicializa apagado **una sola vez** en `ArmarInterfazMaterial` y **no** se reinicia en `CargarCategoriasAsync` ni en `CargarProductosAsync`, igual que el texto y los combos. La vista es una instancia que persiste entre pestañas.
- **Descartado**: guardar el estado en la sesión o en configuración. Fuera de alcance y sin requisito.

## 5. Estrategia de tests (`dotnet test`)

Suite actual: **230/230** en verde, incluidos los casos nuevos de esta spec (§5.1, §5.2) y los 2 de la corrección C5.

### 5.1 `ResolucionUmbralStockTests` (lógica pura, sin EF)
| Caso | Entrada | Esperado |
|---|---|---|
| No definido | `null` | `5`, no degradado |
| Con espacios | `" 5 "` | `5`, no degradado |
| Vacío | `""` | degradado |
| Solo espacios | `"   "` | degradado |
| Decimal con punto | `"5.0"` | degradado |
| Decimal con coma | `"5,0"` | degradado |
| Signo positivo | `"+5"` | degradado |
| Signo negativo | `"-5"` | degradado |
| No numérico | `"abc"` | degradado |
| Fuera de rango | `"2147483648"` | degradado |
| Cero | `"0"` | `0`, no degradado |
| Valor alto | `"1000"` | `1000`, no degradado |

### 5.2 `ProductoNegocioConsultaStockTests` (EF InMemory, helpers de `ProductoNegocioTests`)
- **Criticidad (RF-8, RF-12)**: stock 0 ⇒ `Agotado`; negativo ⇒ `Agotado`; igual al umbral ⇒ `Normal`; 4 con umbral 5 ⇒ `BajoUmbral`; 5 con umbral 5 ⇒ `Normal`; 10 con umbral 5 ⇒ `Normal`; umbral 0 ⇒ solo 0/negativos críticos; sin umbral (degradado) ⇒ solo agotados críticos y ningún `BajoUmbral`.
- **"Solo críticos" (RF-6, RF-11)**: encendido devuelve agotados + bajo umbral; `false`/`null` no restringe; combinado con texto, con categoría y con estado; combinación sin filas ⇒ lista vacía.
- **Prioridad (RF-21)**: el código exacto queda primero (aunque su Nombre lo ubicaría después); texto con espacios alrededor del código ⇒ primero; código inexistente ⇒ orden habitual; dos códigos exactos sembrados en InMemory ⇒ **no** se reordena; código exacto excluido por "Solo críticos" ⇒ sin priorización.
- **Orden (RNF-8)**: sin prioridad se respeta Nombre y luego Código.
- **Conteo (RF-13)**: `ContarProductosAsync()` = 0 con catálogo vacío y N con filas.
- **Solo lectura (RF-15, RNF-3)**: el listado no modifica stock, estado ni precios (comparación antes/después).
- **Cobertura agregada en la fase 6 (corrección C5)**:
  - `ListarProductos_SoloCriticosSinUmbral_DevuelveSoloLosAgotados` — cubre **RF-10** en modo degradado: con `SoloCriticos` encendido y `umbralStock` `null`, el listado devuelve únicamente `Stock <= 0` (agotados), sin `BajoUmbral` ni filas con stock positivo.
  - `ListarProductos_CodigoInexistenteYCoincidenciaParcial_MantieneElOrdenHabitual` — cubre **RF-21** (rama "ninguna coincidencia exacta"): un texto que solo coincide de forma parcial no dispara la priorización y el resultado conserva el orden Nombre → Código (RNF-8).

### 5.3 Lo que **no** cubre InMemory (validación manual)
- La grilla: colores, borde de selección, indicador, atenuación y legibilidad en escala de grises (RNF-4, RNF-10).
- La traducción real del predicado a SQL Server y el rendimiento con 1.000 productos (RNF-1).
- Lectura real de `appsettings.json`, texto del umbral y aviso persistente del modo degradado (D-15, RNF-9).
- Los mensajes de vacío dibujados, la caída de la categoría seleccionada y el fallo de BD en la primera carga (RF-13, RF-16, RF-24).
- La conservación de "Solo críticos" al cambiar de pestaña (D-13).
- RNF-5: conteos por inspección del archivo (0 `Color.Transparent`, 0 `Refresh()`/`Invalidate()`, 1 `PerformLayout()`, 2 controles nuevos).

### 5.4 Verificación de RNF-1 (rendimiento con 1.000 productos)

Medición **ya ejecutada** en la capa de negocio, sin tocar la base de desarrollo:

- **Método**: base aparte **`DB_SISTEMA_VENTA_PERF`** creada con las migraciones vigentes (`dotnet ef database update` con `ConnectionStrings__CadenaSQL`) y poblada con **1.000 productos**: 200 con stock ≤ 0, 200 con 1–4, 600 con 5–200, sobre 5 categorías, 178 con "Leche" en el nombre. No se agregan datos semilla ni migraciones al repositorio.
- **Cómo apuntar la app a esa base**: definir la variable de entorno **`ConnectionStrings__CadenaSQL`** antes de ejecutar; la configuración por entorno tiene precedencia sobre `appsettings.json`, así que **no** hay que editar el archivo.
- **Escenarios medidos** (consulta + mapeo + post-procesos, sin pintado de la grilla):

| Escenario | Filas | Medición |
|---|---|---|
| Listado completo sin filtros | 1.000 | mediana **15,2 ms** / máx **36,3 ms** |
| Texto `"Leche"` | 178 | **14,2 ms** |
| Filtro por categoría | 400 | **11,0 ms** |
| "Solo críticos" umbral 5 | 400 | **12,2 ms** |
| "Solo críticos" umbral `null` (degradado) | 200 | **7,7 ms** |
| `ContarProductosAsync()` | — | **0,7 ms** |

- **Veredicto**: **cumple** el límite de 500 ms (D-14) **como cota inferior**: lo medido es la consulta, el mapeo y los post-procesos; el costo del **pintado** de la grilla queda cubierto por la comprobación visual de T10.

## 6. Trazabilidad RF → capa / archivo / método / tarea

| RF | Capa | Archivo / método | Tarea |
|---|---|---|---|
| RF-1 (cinco columnas + P. Compra/Estado) | Presentación | `ProductosForm.ArmarInterfazMaterial` (columnas existentes) | T10 |
| RF-2, RF-3, RF-4 (búsqueda) | Negocio / Presentación | `ListarProductosAsync` y vista existentes | T10 (regresión) |
| RF-5 (categoría) | Negocio / Presentación | existente | T10 |
| RF-6 (combinación de filtros) | Negocio / Presentación | `ListarProductosAsync` + `_chkSoloCriticos` | T3, T4, T6 |
| RF-7 (recalcular sin perder filtros) | Presentación | handlers existentes + nuevo handler | T6, T10 |
| RF-8 (resaltado + indicador + negativo) | Negocio / Presentación | `NivelCriticidad` + `RowPrePaint`/`CellFormatting` (paleta D-17) | T2, T4, T7 |
| RF-9 (umbral; no definido ⇒ 5) | Negocio / Presentación | `Resolver` + lectura en ctor | T1, T5 |
| RF-10 (degradado solo si definido inválido) | Negocio / Presentación | `Resolver` + `umbralStock: null` | T1, T4, T5 |
| RF-11 ("Solo críticos") | Negocio / Presentación | `SoloCriticos` + casilla | T2, T4, T6 |
| RF-12 (criterio estricto) | Negocio | `ListarProductosAsync` | T4 |
| RF-13 (dos mensajes de vacío) | Negocio / Presentación | `ContarProductosAsync` + `Paint` de la grilla | T3, T4, T9 |
| RF-14 (inactivos y atenuación) | Presentación | paleta atenuada D-17 (`#6E2020`, `#8C6D0A`) | T7 |
| RF-15 (solo lectura) | Negocio | listado y ABM intactos | T4, T10 |
| RF-16 (fallo de BD) | Presentación | `try/catch` de carga y mensaje de vacío | T9 |
| RF-17 (DTOs sin datos sensibles) | Negocio | `ProductoRespuestaDto` | T2, T10 |
| RF-18 (roles) | `PoliticaAcceso` | sin cambios; gating existente | T10 |
| RF-19 (texto del umbral) | Presentación | `_lblUmbral` | T5 |
| RF-20 (parseo determinista) | Negocio | `ResolucionUmbralStock` | T1 |
| RF-21 (código exacto primero y seleccionado) | Negocio / Presentación | reorden + autoselección | T3, T4, T8 |
| RF-22 (selección sobre resaltado) | Presentación | `CellPainting` (borde) | T7 |
| RF-23 (aviso único y persistente) | Presentación | texto del umbral | T5 |
| RF-24 (categoría que desaparece) | Presentación | `CargarCategoriasAsync` | T8 |
| RNF-1 (≤ 500 ms con 1.000) | Negocio / Presentación | consulta única + medición manual | T4, T10 |
| RNF-4, RNF-10 (color + texto + grayscale) | Presentación | paleta D-17 e indicador | T7, T10 |
| RNF-5 (renderizado y 2 controles) | Presentación | `ProductosForm` | T5–T9, T10 |
| RNF-6 (sin reglas en la UI) | Negocio / Presentación | clasificación en Negocio | T4, T10 (inspección) |
| RNF-7 (asincronía) | Negocio / Presentación | `await` existentes | T10 |
| RNF-8 (orden) | Negocio | `OrderBy/ThenBy` + reorden | T4, T8 |
| RNF-9 (mismo umbral mostrado y aplicado) | Presentación | resolución cacheada | T5, T6 |

## 7. Decisiones cerradas y riesgos abiertos

### 7.1 Decisiones cerradas por el usuario (2026-10-26) — no re-abrir

- **D-16 (RF-13 — mensaje de vacío)**: se **confirma** dibujarlo sobre la grilla en su evento `Paint` con `TextRenderer.DrawText` centrado. **No** se autoriza un tercer control: RNF-5 se mantiene con **2** controles nuevos (casilla "Solo críticos" y texto del umbral). Alternativa descartada y documentada en §4h: `Label` de mensaje.
- **D-17 (RF-8/RF-22 — paleta)**: paleta definitiva sobre el fondo oscuro RGB(50,50,50): agotado **`#B71C1C`** (atenuado **`#6E2020`**), bajo umbral **`#F9A825`** (atenuado **`#8C6D0A`**), texto blanco en los cuatro casos, indicador textual y borde de selección de 2 px. Alternativa descartada y documentada en §4g: `#EF6C00` por su cercanía con el naranja de selección `#FF5722`.
- **D-18 (RF-22 — selección sobre una fila resaltada)**: la selección se marca con un **borde de 2 px** sobre el relleno de criticidad y **el color del texto no cambia** (sigue **blanco**, como en D-17). Surge de la fase 6, que detectó la contradicción entre RF-22 (que pedía distinguir la selección cambiando el color del texto) y D-17; RF-22 se ajustó sin renumerar. **Alternativa descartada**: cambiar el color del texto al seleccionar; rompía D-17, el contraste sobre el relleno de criticidad y la legibilidad en escala de grises (RNF-4/RNF-10), y no era coherente con la paleta cerrada.

**No quedan puntos que requieran decisión del usuario.** Las dudas futuras se resuelven con lo ya cerrado en `spec.md` (D-1 … D-15) y en este plan (D-16, D-17, D-18).

### 7.2 Riesgos abiertos (con mitigación, no son preguntas)

1. **Rama de dos códigos exactos (RF-21)**: imposible con la unicidad vigente. *Mitigación*: se implementa la defensa (no reordenar y no forzar selección), se cubre con InMemory sembrando dos códigos iguales y se verifica por inspección en la vista.
2. **Persistencia de "Solo críticos" (D-13)**: depende de que `inicio` conserve la instancia de la vista; hoy es así. *Mitigación*: la casilla solo se inicializa en `ArmarInterfazMaterial` y se verifica manualmente al cambiar de pestaña; si la vista se recreara, se externaliza el estado en una tarea aparte.
3. **Rendimiento con 1.000 productos (RNF-1)**: requiere una base de prueba de ese volumen; no se agregan datos semilla ni migraciones. *Mitigación*: base aparte `DB_SISTEMA_VENTA_PERF` (§5.4) con la consulta+mapeo ya medidos (mediana 15,2 ms) y comprobación manual del pintado de tecla a listado actualizado en T10; una sola consulta por cambio de filtro.
4. **Traducción a SQL Server**: InMemory no valida el SQL generado. *Mitigación*: verificación manual obligatoria contra la base real antes de cerrar T10.
5. **Sin NuGet nuevo**: *Mitigación*: si apareciera la necesidad de agregar paquetes de configuración a `CapaNegocio`, se consulta antes; el plan no los requiere.
6. **Ancho del texto de degradado**: el aviso de dos líneas debe entrar en la columna del 23 %. *Mitigación*: verificación visual en T5 y ajuste de la fuente/ancho dentro de la misma columna, sin agregar controles.
