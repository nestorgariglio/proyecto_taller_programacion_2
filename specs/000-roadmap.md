# Roadmap global de specs — Sistema Comercial G70

> Documento de planificación **global** (no es una spec). Fecha de creación: **2026-10-19**.
> Autor: agente planificador (SDD). Alcance de escritura: solo `specs/`.
> Orden decidido por el usuario: **una funcionalidad a la vez, paso a paso**, cerrando primero lo que quedó a medio hacer.
> Regla de oro del usuario: *"vamos a hacer todo pero una funcionalidad a la vez… si querés planificá todo, pero una funcionalidad a la vez para que quede todo bien documentado"*.

## 1. Para qué sirve este documento

- Fijar **el orden** y **las dependencias** entre las specs futuras, para no adelantar trabajo ni dejar cabos sueltos.
- Dejar constancia de **qué está decidido** (por el usuario) y **qué queda como duda abierta** para la spec correspondiente.
- **No reemplaza** ninguna spec, plan ni tarea: cada funcionalidad se redacta, revisa, aprueba, planifica e implementa **por separado** y con **aprobación explícita del usuario en cada fase**.

## 2. Cómo se trabaja cada spec (proceso idéntico para todas)

1. **Spec** (`specs/NNN-nombre/spec.md`): solo el QUÉ y el POR QUÉ, requisitos en EARS, `Estado: borrador`.
2. **Clarificación**: toda ambigüedad se resuelve preguntando al usuario **antes** de continuar (Constitución 2: "prohibido alucinar"). Las respuestas se registran en "Dudas abiertas" como decisiones cerradas (D-n).
3. **Aprobación explícita del usuario** de la spec.
4. **Plan** (`plan.md`): CÓMO por las 4 capas (`CapaEntidad → CapaDatos → CapaNegocio → CapaPresentacion`), DTOs, cambios en `AppDbContext`, decisiones con alternativa descartada y estrategia de `dotnet test`.
5. **Tareas** (`tasks.md`): máximo 10 tareas atómicas en orden de dependencia de capas, cada una con sus RF y "Hecho cuando:".
6. **Implementación**: **una sola tarea a la vez**; tests/verificación primero en rojo, luego código en verde; `dotnet build` limpio y `dotnet test` en verde antes de cerrar cada tarea.
7. **Cierre**: validación visual, `dotnet build`, `dotnet test` y actualización de `MEMORY.md` (esta tarea la ejecuta quien tenga permiso de escritura fuera de `specs/`).

**Reglas transversales que aplican a todas las specs** (verificadas en `docs/constitution.md` y `AGENTS.md`):
- Aislamiento de 4 capas: la UI nunca toca `AppDbContext` ni SQL directo.
- Bajas **lógicas** (`Estado = false`); **prohibido** el `DELETE` físico de tablas operativas.
- DTOs sanitizados entre `CapaNegocio` y `CapaPresentacion` (las entidades puras no llegan a la UI; nada sensible en DTOs).
- Operaciones I/O `async/await` y mapeo explícito.
- **Transacciones explícitas de EF Core** en operaciones compuestas (ej. Venta + Stock) — Constitución 5, obligatorio en la spec de Ventas.
- **Preguntar antes** de crear migraciones, instalar paquetes NuGet o modificar contratos de datos.
- **El menú principal está congelado**: `inicio` (su `MaterialTabControl`, sus pestañas, su `ImageList` y sus íconos) **no se modifica en ninguna spec futura** —confirmado por el usuario el 2026-10-19—. Las funcionalidades nuevas se incorporan **dentro** de las pantallas ya existentes (ver specs 002–007) o en vistas ya maquetadas; no se agregan, quitan ni reordenan ítems del menú. Cualquier excepción requiere consulta previa y una decisión explícita del usuario.
- Español en UI, comentarios y documentación.

## 3. Estado y secuencia de specs

| # | Spec | Estado | Depende de |
|---|---|---|---|
| — | (transversal) `000-roadmap.md` | Este documento | — |
| 001 | `001-abm-categorias` — ABM de Categorías/Rubros + reencuadre de acceso | **Aprobada e implementada** (reencuadre aprobado 2026-10-19). T1–T14 **hechas y validadas** (`dotnet build` limpio, `dotnet test` 64/64). Solo queda la **validación visual del usuario**. Plan y tareas alineados con el reencuadre | — |
| 002 | `002-abm-productos` — ABM de Productos | **A crear** | 001 |
| 003 | `003-abm-clientes` — ABM de Clientes | **A crear** | — (independiente; conviene tras 002 por patrón de ABM) |
| 004 | `004-abm-proveedores` — ABM de Proveedores | **A crear** | — (independiente; conviene tras 002/003 por patrón de ABM) |
| 005 | `005-ventas` — Registro de Ventas (POS) | **A crear** | 002 (Productos) y 003 (Clientes) |
| 006 | `006-compras` — Registro de Compras | **A crear** | 002 (Productos) y 004 (Proveedores) |
| 007 | `007-reportes` — Reportes | **A crear** | 005 (Ventas) y 006 (Compras) — **al final** |
| — | **Revisión de la vista "Info"** (centro de ayuda estático) | **Ítem de cierre — no es una spec funcional** | 007 (se revisa cuando el sistema está terminado) |

> **Secuencia y nombres de carpeta CONFIRMADOS por el usuario el 2026-10-19**: `002-abm-productos` → `003-abm-clientes` → `004-abm-proveedores` → `005-ventas` → `006-compras` → `007-reportes` (los nombres descriptivos largos —"Registro de Ventas (POS)", "Registro de Compras"— se mantienen como título, pero la **carpeta** es `005-ventas` / `006-compras`). El número libre se toma igualmente en el momento de crear cada carpeta.
> **Confirmado por el usuario**: ninguna de las specs futuras (002–007) modifica el menú principal (ver regla transversal en §2 y decisión cerrada D-R2 en §13).

**Estado de partida común (verificado en el código, no supuesto, al 2026-10-19):**
- Las vistas de `CapaPresentacion` (`ProductosForm`, `ClientesForm`, `ProveedoresForm`, `VentasForm`, `ComprasForm`, `ReportesForm`) están **maquetadas con MaterialSkin 2** pero **muestran datos de prueba (mockeados)**; ninguna tiene todavía su `Negocio` ni su ABM. `InfoForm` también está maquetada, pero **no muestra datos**: es el **centro de ayuda estático** (ver §11).
- Ya existen en `CapaEntidad` y en `AppDbContext` las entidades/tablas `Cliente`, `Proveedor`, `Producto`, `Categoria`, `TipoFactura`, `Venta`, `DetalleVenta`, `Compra`, `DetalleCompra` (creadas en la migración inicial): **el esquema base ya está**, por lo que el trabajo principal es `CapaNegocio` (lógica + DTOs + tests) y `CapaPresentacion` (conectar la UI a datos reales).
- `CapaNegocio` hoy tiene: `UsuarioNegocio`, `CategoriaNegocio` y `PoliticaAcceso` (módulos por rol). **No** existen `ProductoNegocio`, `ClienteNegocio`, `ProveedorNegocio`, `VentaNegocio` ni `CompraNegocio`.
- Accesos por rol **ya definidos** en `PoliticaAcceso`: administrador → todo; encargado → Productos, Categorías, Proveedores, Compras, Reportes; vendedor → Ventas, Clientes.
- Suite de tests actual: `CapaNegocio.Tests` (`CategoriaNegocioTests`, `UsuarioNegocioTests`, `PoliticaAccesoTests`), **64/64 en verde** con `dotnet test`.

---

## 4. Spec 001 — Categorías/Rubros (reencuadre) — **implementada**

- **Objetivo**: cerrar el reencuadre de acceso de la gestión de categorías (deja de ser ítem del menú principal y pasa a ser una característica del módulo Productos, en modal) y terminar el ABM ya implementado en backend.
- **Estado de la documentación (2026-10-19/20)**: `spec.md` en estado **aprobada** (D-4); `plan.md` **actualizado y vigente** (sin menciones de "pendiente de aprobación", con el mecanismo del gating por rol y del refresco del combo ya explicitado) y `tasks.md` **revisado y cerrado** (T11–T14 hechas y validadas).
- **Estado de implementación**: **T1–T14 hechas y validadas**. T11 (pestaña `tab_categorias` eliminada y crash `products.png` corregido), T12 (botón "Gestionar categorías" en `ProductosForm` con gating por rol y guard defensivo en runtime) y T13 (contenedor modal `GestionCategoriasModalForm` que reutiliza `GestionCategoriasControl`) están en código; T14 (verificación técnica) **cerrada**: `dotnet build` limpio y `dotnet test` **64/64**.
- **Alcance pendiente**: **solo la validación visual del usuario** (arranque sin crash, flujo ABM completo desde el botón, botón oculto a Vendedor y visible a Admin/Encargado, combo de categorías refrescado al cerrar el modal).
- **Toca**: `CapaPresentacion/Productos/ProductosForm.cs`, `CapaPresentacion/Categorias/`, `CapaPresentacion/Program.cs`, `CapaPresentacion/Inicio/inicio.cs` (tocados: pestaña eliminada y gating por rol).
- **Riesgos/dependencias**: ninguno pendiente de backend (completo y en verde). El rol del usuario ya llega hasta `ProductosForm` mediante `AplicarRolUsuario(string?)`.
- **Referencias**: `specs/001-abm-categorias/{spec.md, plan.md, tasks.md}`.

## 5. Spec 002 — ABM de Productos

- **Objetivo**: convertir la pantalla de Productos en un ABM real: reemplazar la **grilla mockeada** y el **combo de estado mockeado** por datos reales; el **combo de categorías ya es real** (spec 001). Alta, modificación y baja lógica de productos.
- **Por qué segundo**: es la funcionalidad natural después de cerrar categorías (los productos se clasifican por categoría) y desbloquea Ventas y Compras, que operan sobre productos y stock.
- **Alcance probable**:
  - `CapaNegocio`: nuevo `ProductoNegocio` + DTOs (entrada/salida) + enums de resultado, con validaciones, alta, modificación, baja lógica y listados (todos / solo activos) con mapeo explícito.
  - `CapaPresentacion`: `ProductosForm` con grilla real (filtros por nombre/código, categoría y estado) y modal de alta/edición (patrón `CategoriaForm`/`UsuarioForm`); reutilizar el patrón del botón "Gestionar categorías" ya establecido para el acceso a categorías.
  - `CapaDatos`: evaluar defensas en `AppDbContext` (p. ej. unicidad/longitudes) **con migración aprobada por el usuario**.
- **Toca**: `CapaNegocio` (nuevo `ProductoNegocio`), `CapaPresentacion/Productos/ProductosForm.cs` (+ Designer si aplica), `CapaPresentacion/Program.cs` (DI), `AppDbContext` (si se aprueba migración), `CapaNegocio.Tests` (nuevos tests).
- **Dependencias**: spec 001 (combo de categorías real y patrón modal).
- **Riesgos**: la entidad `Producto` ya tiene FK a `Categoria`, pero **`Categoria` no tiene colección inversa** → decidir si se consulta por proyección (sin tocar `CapaEntidad`) como en el conteo de productos activos de la spec 001.
- **Dudas abiertas (para su spec)**:
  - [NECESITA ACLARACIÓN] ¿El `Codigo` del producto debe ser **único** (como DNI/correo de usuarios) y validarse en `CapaNegocio` + `AppDbContext`?
  - [NECESITA ACLARACIÓN] ¿Longitudes y formato de `Nombre`, `Descripcion`, `PrecioCompra`, `PrecioVenta`? ¿El stock admite negativos?
  - [NECESITA ACLARACIÓN] ¿El ABM incluye **ajuste manual de stock** o el stock solo cambia por Ventas/Compras?
  - [NECESITA ACLARACIÓN] Al dar de baja un producto con stock o con ventas históricas, ¿se advierte/impide, al estilo del diálogo de categorías?
  - [NECESITA ACLARACIÓN] ¿El combo de estado y la grilla deben mostrar activos e inactivos con filtro (patrón categorías)?

## 6. Spec 003 — ABM de Clientes

- **Objetivo**: ABM real de clientes (hoy `ClientesForm` con grilla mockeada), en el mismo patrón ya probado (grilla + modal, baja lógica, unicidad).
- **Alcance probable**: `ClienteNegocio` + DTOs + enums; validaciones de campos obligatorios y unicidad si el usuario la define; listados; baja lógica; UI de `ClientesForm` conectada (grilla, filtros y modal de alta/edición).
- **Toca**: `CapaNegocio`, `CapaPresentacion/Clientes/ClientesForm.cs`, `Program.cs` (DI), `AppDbContext`/migración (si se aprueba), tests.
- **Dependencias**: ninguna estricta. Se ubica después de 002 por conveniencia (mismo patrón de ABM ya resuelto y el rol Vendedor ya tiene acceso a Clientes).
- **Riesgos**: es la primera pantalla de un módulo donde el **Vendedor** ya tiene acceso (en categorías no lo tenía), así que el gating por rol hay que definirlo explícitamente.
- **Dudas abiertas (para su spec)**:
  - [NECESITA ACLARACIÓN] ¿`Dni` y `Correo` de cliente son **únicos**? (AGENTS documenta unicidad de DNI/correo solo para **usuarios**.)
  - [NECESITA ACLARACIÓN] ¿Longitudes/formatos de `Dni` y `Telefono`, y campos obligatorios (`Nombre`/`Apellido`)?
  - [NECESITA ACLARACIÓN] ¿Puede el **Vendedor** crear/editar clientes o solo consultarlos? (`PoliticaAcceso` le da acceso al módulo, pero no distingue operaciones.)
  - [NECESITA ACLARACIÓN] Al dar de baja un cliente con ventas históricas, ¿se advierte/impide?

## 7. Spec 004 — ABM de Proveedores

- **Objetivo**: ABM real de proveedores (hoy `ProveedoresForm` con grilla mockeada).
- **Alcance probable**: `ProveedorNegocio` + DTOs + enums; unicidad de CUIT si el usuario la define; listados; baja lógica; UI conectada (grilla + modal).
- **Toca**: `CapaNegocio`, `CapaPresentacion/Proveedores/ProveedoresForm.cs`, `Program.cs` (DI), `AppDbContext`/migración (si se aprueba), tests.
- **Dependencias**: ninguna estricta; conviene después de 003 para reutilizar el patrón de ABM.
- **Riesgos**: proveedores se usan en Compras → la baja de un proveedor con compras históricas necesita definición (advertencia, patrón categorías).
- **Dudas abiertas (para su spec)**:
  - [NECESITA ACLARACIÓN] ¿`Cuit` único? ¿Formato/validación del CUIT?
  - [NECESITA ACLARACIÓN] ¿`Correo` único? ¿Longitudes de `RazonSocial`/`Telefono`?
  - [NECESITA ACLARACIÓN] La grilla mockeada actual muestra una columna "Rubro" que **no existe** en la entidad `Proveedor`: ¿se descarta o se agrega un rubro/condición de IVA a la entidad? (requiere aprobación de cambio de contrato de datos).

## 8. Spec 005 (`005-ventas`) — Registro de Ventas (POS)

- **Objetivo**: convertir `VentasForm` en un punto de venta real: registrar una venta con sus ítems, descontar stock y calcular cambio, sobre datos reales de Productos y Clientes.
- **Por qué después de 002 y 003**: una venta necesita productos reales (y su stock) y clientes reales.
- **Alcance probable**:
  - `CapaNegocio`: `VentaNegocio` con registro de venta **dentro de una transacción explícita de EF Core** (Venta + DetalleVenta + descuento de stock) — **obligatorio por Constitución 5**; DTOs y enums de resultado.
  - `CapaPresentacion`: `VentasForm` con búsqueda de productos, carga de ítems, totales, medio/tipo de comprobante y confirmación.
  - `CapaDatos`: sin cambios de esquema previsibles (las tablas ya existen); migración solo si el usuario aprueba algún ajuste.
- **Toca**: `CapaNegocio` (nuevo `VentaNegocio`), `CapaPresentacion/Ventas/VentasForm.cs`, `Program.cs` (DI), `CapaNegocio.Tests`.
- **Dependencias**: 002 (Productos/stock) y 003 (Clientes). Requiere `ProductoNegocio` para consultar productos y su stock.
- **Riesgos**: es la operación más crítica del sistema (integridad de stock y montos). El descuento de stock **debe** ser transaccional; los tests con `InMemory` **no** validan transacciones reales → verificación con BD real además de `dotnet test`.
- **Dudas abiertas (para su spec)**:
  - [NECESITA ACLARACIÓN] La entidad `Venta` **no tiene** `id_usuario` (a diferencia de `Compra`): ¿la venta registra **quién** la realizó? (implicaría cambio de entidad + migración, con aprobación).
  - [NECESITA ACLARACIÓN] `Venta.IdTipo` apunta a `TipoFactura` (que tiene `nro_factura`): ¿la venta genera/incrementa número de comprobante? ¿qué tipos de comprobante existen?
  - [NECESITA ACLARACIÓN] ¿Se puede vender **sin stock suficiente** (stock negativo) o se bloquea? ¿Se advierte?
  - [NECESITA ACLARACIÓN] ¿`MontoPago`/`MontoCambio` se exigen siempre, o la venta puede registrarse sin pago en efectivo?
  - [NECESITA ACLARACIÓN] ¿Una venta se puede **anular**? Si se anula, ¿se revierte el stock? (baja lógica de venta + reversión).
  - [NECESITA ACLARACIÓN] ¿La venta aplica descuentos/impuestos, o el `MontoTotal` es la suma directa de subtotales?

## 9. Spec 006 (`006-compras`) — Registro de Compras

- **Objetivo**: convertir `ComprasForm` en el registro real de compras a proveedores, **con su efecto en el stock** (ingreso de mercadería).
- **Alcance probable**:
  - `CapaNegocio`: `CompraNegocio` con registro de compra **en transacción explícita** (Compra + DetalleCompra + incremento de stock) — Constitución 5; DTOs y enums.
  - `CapaPresentacion`: `ComprasForm` con selección de proveedor (real, spec 004), carga de ítems (productos reales, spec 002), totales y confirmación.
  - `CapaDatos`: sin cambios de esquema previsibles (tablas ya existen); migración solo si el usuario aprueba ajustes.
- **Toca**: `CapaNegocio` (nuevo `CompraNegocio`), `CapaPresentacion/Compras/ComprasForm.cs`, `Program.cs` (DI), `CapaNegocio.Tests`.
- **Dependencias**: 002 (Productos) y 004 (Proveedores). `Compra` **sí** tiene `id_usuario` (el usuario que registra).
- **Riesgos**: simétrico a Ventas (integridad de stock); decidir si la compra **actualiza `precio_compra`** del producto y si su anulación revierte stock.
- **Dudas abiertas (para su spec)**:
  - [NECESITA ACLARACIÓN] ¿La compra **actualiza `PrecioCompra`** del producto? Si el precio varía, ¿cuál queda?
  - [NECESITA ACLARACIÓN] ¿Una compra se puede **anular**? ¿Revierte stock y montos?
  - [NECESITA ACLARACIÓN] ¿Se exige nº de factura/comprobante del proveedor (hoy no hay campo en `Compra`)? (implicaría migración).
  - [NECESITA ACLARACIÓN] ¿El `MontoTotal` se calcula de la suma de subtotales o puede editarse?

## 10. Spec 007 (`007-reportes`) — Reportes (al final)

- **Objetivo**: reemplazar los reportes mockeados de `ReportesForm` por reportes reales.
- **Por qué al final**: todo reporte depende de datos reales de Ventas y Compras, que recién existen tras las specs 005 y 006.
- **Alcance probable**: consultas de `CapaNegocio` (solo lectura, `AsNoTracking`, proyección a DTOs de reporte) y gráficos/tablas en `ReportesForm` (LiveCharts ya maquetado).
- **Toca**: `CapaNegocio` (nuevo `ReporteNegocio` o métodos de consulta), `CapaPresentacion/Reportes/ReportesForm.cs`, `Program.cs` (DI), `CapaNegocio.Tests`.
- **Dependencias**: 005 (Ventas) y 006 (Compras) — **imprescindibles**.
- **Riesgos**: definir el **catálogo exacto de reportes** y sus filtros de período antes de planificar; evitar reportes especulativos.
- **Dudas abiertas (para su spec)**:
  - [NECESITA ACLARACIÓN] ¿Qué reportes exactamente? (p. ej. ventas por período, productos más vendidos, stock bajo/valorizado, compras por proveedor, ventas por vendedor…). Los gráficos mockeados sugieren "ventas por fecha" y "ganancia", pero **no se asume**.
  - [NECESITA ACLARACIÓN] ¿Filtros por rango de fechas, por categoría, por vendedor, por proveedor?
  - [NECESITA ACLARACIÓN] ¿Exportación a PDF/Excel o solo visualización en pantalla?
  - [NECESITA ACLARACIÓN] ¿Qué rol puede ver Reportes (hoy: administrador y encargado)?

---

## 11. Vista "Info" — centro de ayuda ESTÁTICO (revisión de cierre, sin spec funcional)

- **Qué es (verificado en el código el 2026-10-19)**: `CapaPresentacion/Info/InfoForm.cs` es un `UserControl` armado por código en `ArmarInterfazMaterial()` que funciona como **centro de ayuda ESTÁTICO**: una tarjeta de encabezado ("ℹ️ CENTRO DE AYUDA Y GUÍA DEL SISTEMA" / "Documentación rápida de operación y niveles de acceso (Sistema G70)"), una tarjeta de **módulos operativos** (Ventas/POS, Productos, Clientes & Proveedores) y una tarjeta de **permisos y roles** (Administrador, Encargado, Vendedor).
- **No tiene backend**: no consulta `AppDbContext` ni `CapaNegocio`, no recibe datos por DI (ctor sin parámetros), no muestra datos de la base ni escribe nada. Su contenido son **textos fijos** en `MaterialLabel` dentro de `MaterialCard`.
- **Ubicación en el menú**: la pestaña `tab_info` está **entre "Reportes" y "Salir"** (`inicio.Designer.cs`: orden `… tab_reportes, tab_info, tab_salir`). Hoy **no** se oculta por rol (verificado: `AplicarPermisosPorRol()` no llama a `OcultarSiNoAutorizada` para `tab_info`), por lo que la ven todos los roles.
- **Decisión del usuario (2026-10-19)**: la vista Info se documenta como **vista estática**. **NO lleva spec propia de backend** (no hay `InfoNegocio`, ni DTOs, ni consultas, ni migraciones) y **no se le agrega lógica nueva** en el roadmap.
- **Ítem de cierre (revisión de contenido)**: después de la spec **007 (Reportes)** —es decir, con el sistema ya terminado— se realiza **una sola revisión de contenido y redacción** de `InfoForm` para que sus textos reflejen el sistema real: módulos efectivamente existentes, niveles de acceso reales (los de `PoliticaAcceso`) y cualquier texto que hoy quede desactualizado o prometa algo que el sistema no hace. Es una **revisión editorial**, no una funcionalidad: no crea carpeta `specs/NNN-...`, no toca `CapaEntidad`/`CapaDatos`/`CapaNegocio` y no modifica el menú (la pestaña `tab_info` ya existe y no se mueve de lugar).
- **Alcance explícito de esa revisión de cierre**: ajustar títulos, descripciones de módulos y la descripción de roles/permisos para que coincidan con el sistema entregado. **Fuera de alcance**: convertirla en una vista dinámica, agregarle reportes, búsquedas, enlaces a pantallas o cualquier dato de BD (eso requeriría una spec nueva y aprobación explícita del usuario).
- **Aclaración de orden**: la pestaña Info está **antes** de "Salir" en el menú, pero su revisión va **al final** del roadmap (tras Reportes); ese desfase entre posición en el menú y momento de revisión es intencional y no convierte a Info en una spec funcional.

---

## 12. Riesgos y dependencias transversales

- **Migraciones EF Core**: cualquier cambio de esquema requiere **aprobación previa del usuario** (AGENTS) y ejecutar `dotnet ef database update` **antes** de probar la UI (aprendizaje de `MEMORY.md`).
- **Datos de prueba**: `scripts/preparar-bd.ps1` es idempotente y sin `DELETE`; cada spec que agregue datos semilla debe mantener ese contrato y sus verificaciones de conteo.
- **Tests con `InMemory`**: no validan índices únicos, longitudes ni transacciones reales → lo que dependa de eso (unicidad, Venta+Stock, Compra+Stock) se verifica también contra la BD real (validación manual documentada en el plan).
- **Datos mockeados**: deben eliminarse módulo por módulo, en la spec que corresponda; **prohibido** dejarlos "a medias" entre specs.
- **UI estática**: la interfaz está definida; **el menú principal no se toca en ninguna spec futura** (decisión confirmada D-R2, §13). Cualquier modificación significativa de UI (estructura de pantallas, agregar/quitar ítems dentro de una vista) se **consulta antes** al usuario (regla registrada en la spec 001).
- **Contratos de datos**: cambios en entidades (`CapaEntidad`) se consultan antes (p. ej. `Venta.id_usuario`, rubro de proveedor, nº de factura de compra).
- **`MEMORY.md`**: la actualización de la memoria fuera de `specs/` la realiza quien tenga permiso de escritura (el agente planificador solo escribe en `specs/`); los textos pendientes se dejan preparados en `specs/001-abm-categorias/MEMORY-pendiente.md`.

## 13. Decisiones cerradas del roadmap (confirmadas por el usuario el 2026-10-19)

- **D-R1 (orden y nombres)**: se confirma el orden **y los nombres de carpeta**: `002-abm-productos` → `003-abm-clientes` → `004-abm-proveedores` → `005-ventas` → `006-compras` → `007-reportes`. Se mantiene, además, la regla de **una funcionalidad a la vez** con aprobación explícita del usuario en cada fase (spec → plan → tareas).
- **D-R2 (menú principal congelado)**: se confirma que **ninguna spec futura (002–007) modifica el menú principal** (ni pestañas, ni ítems, ni íconos, ni orden). El reencuadre de categorías de la spec 001 lo deja igual y las funcionalidades nuevas viven dentro de las pantallas ya existentes. Regla transversal registrada en §2.
- **D-R3 (vista Info = vista estática)**: la vista `Info` se documenta como **centro de ayuda estático**, sin backend, sin spec propia y sin lógica nueva; queda como **único ítem de cierre** del roadmap una **revisión de contenido y redacción** tras la spec 007 (ver §11).
- **D-R4 (spec 001 aprobada e implementada)**: la spec `001-abm-categorias` está **`aprobada`** e **implementada** (T1–T14 hechas y validadas), con su **`plan.md` y `tasks.md` alineados y cerrados** al reencuadre aprobado (D-4) el 2026-10-19; T11–T14 hechas, `dotnet build` limpio y `dotnet test` 64/64. **Único pendiente: la validación visual del usuario.**

## 14. Dudas abiertas del roadmap

- **Ninguna.** Las dos dudas abiertas del roadmap (orden/nombres de las specs y congelamiento del menú principal) quedaron **cerradas por el usuario el 2026-10-19** y se registran arriba como D-R1 y D-R2; la vista Info quedó como decisión D-R3.
- Siguen abiertas, **para su propia spec**, las dudas funcionales ya listadas en §5 a §10 (productos, clientes, proveedores, ventas, compras y reportes). Cada una se resolverá con el usuario **antes** de redactar/cerrar la spec correspondiente (Constitución 2: prohibido alucinar).
