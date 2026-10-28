# Plan técnico — Spec 003: Administración del Padrón de Clientes
Estado: propuesto (pendiente de aprobación del usuario)
Spec de referencia: `specs/003-abm-clientes/spec.md` — **aprobada**
Fecha: 2026-10-28

> **Gate de aprobación pendiente**: la Tarea **T1** incluye una **migración de EF Core** (`AddClienteConstraints`). Según `AGENTS.md`, no se crea ni aplica ninguna migración sin aprobación explícita del usuario. El resto de las tareas no requiere aprobaciones adicionales.

> **Cambio de requisito D-12 (aprobado 2026-10-28, buscador vacío en el padrón)**: incorporado en D-T4, en la tabla de métodos (diferencia entre `ListarClientesAsync` y `BuscarClientesParaPosAsync`), en la estrategia de tests de RF-17/RF-18/RF-19 y en la trazabilidad. En el padrón, buscador vacío ⇒ lista todo el filtro de estado; en el POS, texto vacío ⇒ lista vacía sin consultar.

---

## 1. Responsabilidades por capa

### `CapaEntidad` — **sin cambios**
- `Cliente` ya tiene todos los campos necesarios (`IdCliente`, `Dni`, `Nombre`, `Apellido`, `Correo`, `Telefono`, `Estado`, `FechaRegistro`).
- **No** se renombran columnas ni propiedades (D-1: Documento ↔ `Dni`; Nombre completo = `Nombre` + `Apellido`).
- **No** se agregan entidades ni relaciones nuevas.

### `CapaDatos` — configuración Fluent + migración aprobada
- `AppDbContext.OnModelCreating`: configuración de `Cliente` (longitudes, índices únicos filtrados). Ninguna otra entidad se toca.
- Nueva migración `AddClienteConstraints` (T1, sujeta a aprobación).
- **No** se agregan `DbSet`, ni se modifica `scripts/seed-desarrollo.sql` (la tabla CLIENTE no tiene datos semilla), ni se cambia la cadena de conexión.

### `CapaNegocio` — lógica, validaciones, DTOs y mensajes
- Nuevo `ClienteNegocio` (patrón `CategoriaNegocio`: `AppDbContext` inyectado, `AsNoTracking`, proyecciones, `AnyAsync`/`FirstOrDefaultAsync`, `async/await`).
- Nuevos DTOs en `CapaNegocio/DTOs/Clientes/` y enums en `CapaNegocio/Enums/`.
- `PoliticaAcceso`: se agrega `"Clientes"` al rol Encargado (D-8) y un método específico para las operaciones de baja/reactivación.
- **Toda** validación, decisión y redacción de mensajes (incluida la confirmación de baja y el aviso de «sin resultados») vive aquí (RF-26).
- **No** se tocan `UsuarioNegocio` ni `CategoriaNegocio`.

### `CapaPresentacion` — pantalla existente, sin tocar el menú
- `Clientes/ClientesForm.cs`: se reemplaza la grilla mockeada por datos reales y se agrega DI + `AplicarRolUsuario(string?)` (patrón `ProductosForm`).
- Nuevo modal `Clientes/ClienteForm.cs` para alta/edición (patrón `CategoriaForm`): envía los valores **crudos** al negocio y muestra el `Mensaje` devuelto.
- `Program.cs`: registro de `ClienteNegocio` y `ClientesForm`.
- `Inicio/inicio.cs`: `ClientesForm` pasa a recibir por DI (se reemplaza `new ClientesForm()`), se suscribe `tab_clientes.Enter` para recargar y se invoca `AplicarRolUsuario` desde `AplicarPermisosPorRol()`.
- **No** se crean pantallas ni ítems de menú, no se modifican pestañas/íconos, y **no** se agrega lógica de validación ni textos de error en la vista (RNF-6, RF-26).

---

## 2. DTOs, enums y mapeos

Carpeta `CapaNegocio/DTOs/Clientes/` (namespace `CapaNegocio.DTOs.Clientes`) y `CapaNegocio/Enums/` (namespace `CapaNegocio.Enums`).

### DTOs de entrada
| DTO | Campos |
|---|---|
| `CrearClienteDto` | `Documento`, `Nombre`, `Apellido`, `Correo`, `Telefono` (todos `string?`, tal como los tipea el usuario) |
| `ModificarClienteDto` | `IdCliente` (`int`) + `Documento`, `Nombre`, `Apellido`, `Correo`, `Telefono` |

Los DTOs de entrada **no** llevan estado ni fecha: el alta siempre nace activa (RF-1) y la modificación conserva el estado (RF-15).

### DTO de salida
| DTO | Campos |
|---|---|
| `ClienteRespuestaDto` | `IdCliente`, `Documento`, `Nombre`, `Apellido`, `NombreCompleto` (calculado), `Correo`, `Telefono`, `Estado`, `FechaRegistro` |

- `Documento` se mapea desde `Cliente.Dni` y `Cliente.Dni` desde `Documento` (sin cambio de esquema, D-1).
- `NombreCompleto` es una propiedad calculada de solo lectura: `Nombre + " " + Apellido` (D-9). Se calcula sobre el DTO ya materializado, nunca en la proyección SQL (para no depender de traducción de concatenación en la consulta).
- No se expone: ninguna entidad pura, ninguna colección de ventas ni datos internos.

### Mapeo explícito
Un único método privado `MapearCliente(Cliente cliente) → ClienteRespuestaDto` (usado tras persistir) y, en las **consultas**, proyección directa `Select(c => new ClienteRespuestaDto { ... })` para no materializar la entidad completa (RNF-2).

### Respuestas y enums de resultado
| Respuesta | Campos | Enum y casos |
|---|---|---|
| `RespuestaCreacionCliente` | `Resultado`, `Mensaje`, `Cliente` (`ClienteRespuestaDto?`) | `ResultadoCreacionCliente`: `Exito`, `DatosInvalidos`, `DocumentoDuplicado`, `CorreoDuplicado`, `ErrorPersistencia` |
| `RespuestaModificacionCliente` | `Resultado`, `Mensaje`, `Cliente` | `ResultadoModificacionCliente`: `Exito`, `DatosInvalidos`, `ClienteNoEncontrado`, `DocumentoDuplicado`, `CorreoDuplicado`, `ErrorPersistencia` |
| `RespuestaBajaCliente` | `Resultado`, `Mensaje`, `Cliente` | `ResultadoBajaCliente`: `Exito`, `DatosInvalidos`, `ClienteNoEncontrado`, `ClienteYaInactivo`, `PermisoDenegado`, `ErrorPersistencia` |
| `RespuestaActivacionCliente` | `Resultado`, `Mensaje`, `Cliente` | `ResultadoActivacionCliente`: `Exito`, `DatosInvalidos`, `ClienteNoEncontrado`, `ClienteYaActivo`, `PermisoDenegado`, `ErrorPersistencia` |
| `RespuestaConfirmacionBajaCliente` | `Resultado`, `Mensaje` (texto ya redactado), `VentasAsociadas` (`int`) | `ResultadoConfirmacionBajaCliente`: `ConfirmacionRequerida`, `ClienteNoEncontrado`, `ClienteYaInactivo`, `PermisoDenegado`, `ErrorPersistencia` |
| `RespuestaListadoClientes` | `Resultado`, `Mensaje`, `Clientes` (`IReadOnlyList<ClienteRespuestaDto>`) | `ResultadoListadoClientes`: `ConResultados`, `SinResultados`, `ErrorPersistencia` |
| — | — | `FiltroEstadoCliente`: `Activos`, `Inactivos` |

- `RespuestaConfirmacionBajaCliente` existe para cumplir RF-21/RF-22/RF-26 con **un solo diálogo**: el negocio entrega el texto (con la cantidad de ventas cuando corresponde) y la vista solo lo muestra; al aceptar, la vista llama a `DarBajaClienteAsync`.
- `Mensaje` está **siempre** informado (éxito o error). La vista lo muestra sin modificarlo ni componerlo.
- Éxitos: `"Cliente {NombreCompleto} registrado exitosamente."`, `"... modificado exitosamente."`, `"... dado de baja exitosamente."`, `"... reactivado exitosamente."`.

### Métodos públicos de `ClienteNegocio`
| Método | Devuelve | RF |
|---|---|---|
| `CrearClienteAsync(CrearClienteDto)` | `RespuestaCreacionCliente` | RF-1 a RF-11 |
| `ModificarClienteAsync(ModificarClienteDto)` | `RespuestaModificacionCliente` | RF-12 a RF-15 |
| `ListarClientesAsync(FiltroEstadoCliente filtro, string? textoBusqueda = null)` | `RespuestaListadoClientes` | RF-16, RF-17, RF-19 (texto vacío ⇒ sin filtro de texto, D-12) |
| `BuscarClientesParaPosAsync(string? textoBusqueda)` | `RespuestaListadoClientes` | RF-18, RF-19 (texto vacío ⇒ lista vacía sin consultar, D-12) |
| `ContarVentasClienteAsync(int idCliente)` | `int` | RF-22 (D-11) |
| `ObtenerConfirmacionBajaClienteAsync(int idCliente, string? rol)` | `RespuestaConfirmacionBajaCliente` | RF-21, RF-22, RF-27 |
| `DarBajaClienteAsync(int idCliente, string? rol)` | `RespuestaBajaCliente` | RF-21, RF-22, RF-24, RF-25, RF-27 |
| `ReactivarClienteAsync(int idCliente, string? rol)` | `RespuestaActivacionCliente` | RF-23, RF-24, RF-27 |

Validaciones privadas (una por regla, para trazabilidad 1 regla ↔ 1 prueba): `ValidarDocumento`, `ValidarNombreOApellido` (recibe el nombre del campo), `ValidarCorreo`, `ValidarTelefono`, `ValidarUnicidadDocumentoAsync`, `ValidarUnicidadCorreoAsync`. Reglas con expresiones regulares **compiladas y estáticas privadas** de `ClienteNegocio` (el archivo es el único dueño de las reglas).

| Dato | Regla | Mensaje (ejemplo) |
|---|---|---|
| Documento | `^[A-Za-z0-9]{7,20}$` tras Recorte | «El documento debe tener entre 7 y 20 caracteres y solo admite letras sin tilde y dígitos.» |
| Nombre / Apellido | `^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ' -]{2,100}$` tras Recorte | «El nombre debe tener entre 2 y 100 caracteres y solo admite letras, espacios, apóstrofo y guion.» |
| Correo | `^[^\s@]+@[^\s@]+\.[A-Za-z]{2,}$` (solo si está informado) | «El correo no tiene un formato válido (ejemplo: nombre@dominio.com).» |
| Teléfono | `^\d{6,15}$` (solo si está informado) | «El teléfono solo admite dígitos (entre 6 y 15).» |
| Obligatorios | Documento, Nombre y Apellido no vacíos tras Recorte | «El documento es obligatorio.» / «El nombre es obligatorio.» / «El apellido es obligatorio.» |

«No informado» = `null`, vacío o solo espacios tras Recorte → se persiste como `null` (para que el índice único filtrado lo excluya y para que varios clientes puedan no tener el dato).

---

## 3. Persistencia

### Configuración Fluent de `Cliente` (T1)
En `AppDbContext.OnModelCreating`, junto a las configuraciones existentes de `Usuario` y `Categoria`:

| Elemento | Configuración | Motivo |
|---|---|---|
| `Dni` | `HasMaxLength(20)`, `IsUnicode(false)` | Coherente con 7-20 alfanuméricos ASCII. **Requisito técnico**: SQL Server no permite crear índices sobre `nvarchar(max)` (columna LOB), por lo que la longitud debe acotarse para poder declarar la unicidad en la base. |
| `Nombre`, `Apellido` | `HasMaxLength(100)` | Coherente con 2-100. |
| `Correo` | `HasMaxLength(254)` | Longitud estándar de dirección de correo; además debe ser indexable. |
| `Telefono` | `HasMaxLength(15)`, `IsUnicode(false)` | Coherente con 6-15 dígitos. |
| Índice de Documento | `HasIndex(c => c.Dni).IsUnique().HasFilter("[dni] IS NOT NULL")` | RNF-4. Se mantiene el filtro `IS NOT NULL` por filas históricas sin documento. |
| Índice de Correo | `HasIndex(c => c.Correo).IsUnique().HasFilter("[correo] IS NOT NULL AND [correo] <> ''")` | RNF-4 + «muchos clientes sin correo». Mismo patrón que `Usuario.Correo`, ampliado para excluir cadena vacía. |

- La colación por defecto de la base es insensible a mayúsculas, por lo que el índice único ya rechaza «abc@x.com» y «ABC@X.COM» como duplicados; la validación previa del negocio replica ese criterio con `ToUpper()` (ver D-T1).
- **No** se renombran columnas ni se agregan/quitan campos: solo longitudes e índices (D-1 intacta).

### Migración propuesta
- **Nombre**: `AddClienteConstraints` (consistente con `AddCategoryConstraints` / `AddUserConstraints`).
- **Comandos exactos** (requieren `dotnet tool restore` previo):
  1. `dotnet ef migrations add AddClienteConstraints --project CapaDatos --startup-project CapaDatos --context AppDbContext`
  2. `dotnet ef database update --project CapaDatos --startup-project CapaDatos --context AppDbContext`

### Verificación previa (obligatoria, solo lectura, sin borrar datos)
Antes de generar/aplicar la migración, ejecutar contra `DB_SISTEMA_VENTA` y **abortar** si alguna consulta devuelve filas:

```sql
-- 1) Documentos duplicados (la colación por defecto es case-insensitive)
SELECT UPPER(LTRIM(RTRIM(dni))) AS documento, COUNT(*) AS cantidad
FROM CLIENTE
WHERE dni IS NOT NULL AND LTRIM(RTRIM(dni)) <> ''
GROUP BY UPPER(LTRIM(RTRIM(dni)))
HAVING COUNT(*) > 1;

-- 2) Correos informados duplicados
SELECT UPPER(LTRIM(RTRIM(correo))) AS correo, COUNT(*) AS cantidad
FROM CLIENTE
WHERE correo IS NOT NULL AND LTRIM(RTRIM(correo)) <> ''
GROUP BY UPPER(LTRIM(RTRIM(correo)))
HAVING COUNT(*) > 1;

-- 3) Documentos que no cumplen el formato (7-20, solo A-Z/a-z/0-9)
SELECT id_cliente, dni
FROM CLIENTE
WHERE dni IS NOT NULL AND LTRIM(RTRIM(dni)) <> ''
  AND (LEN(LTRIM(RTRIM(dni))) NOT BETWEEN 7 AND 20
       OR dni COLLATE Latin1_General_100_BIN2 LIKE '%[^A-Za-z0-9]%');

-- 4) Longitudes que exceden las nuevas columnas
SELECT id_cliente, LEN(ISNULL(correo,'')) AS largo_correo,
       LEN(ISNULL(telefono,'')) AS largo_telefono,
       LEN(ISNULL(nombre,'')) AS largo_nombre,
       LEN(ISNULL(apellido,'')) AS largo_apellido
FROM CLIENTE
WHERE LEN(ISNULL(correo,'')) > 254 OR LEN(ISNULL(telefono,'')) > 15
   OR LEN(ISNULL(nombre,'')) > 100 OR LEN(ISNULL(apellido,'')) > 100;

-- 5) Teléfonos informados con caracteres no numéricos o longitud fuera de 6-15
SELECT id_cliente, telefono
FROM CLIENTE
WHERE telefono IS NOT NULL AND LTRIM(RTRIM(telefono)) <> ''
  AND (LEN(LTRIM(RTRIM(telefono))) NOT BETWEEN 6 AND 15
       OR telefono COLLATE Latin1_General_100_BIN2 LIKE '%[^0-9]%');
```

**Tratamiento si hay violaciones**: se informa al usuario el detalle y **su tratamiento se define en el plan/consulta posterior**; **no** se borra ni se corrige automáticamente ningún registro (RNF-4, spec). La tabla hoy no tiene datos semilla, por lo que se espera resultado vacío; la consulta igual se ejecuta por si la base real tiene filas.

---

## 4. Decisiones técnicas

- **D-T1 — Comparación insensible a mayúsculas traducible a SQL Server e InMemory: `ToUpper()` en ambos lados de la comparación** (`c.Dni!.ToUpper() == documento.ToUpper()`, `c.Correo!.ToUpper() == correo.ToUpper()`). Se traduce a `UPPER()` en SQL Server y funciona igual en el proveedor InMemory de los tests.
  *Alternativa descartada*: `string.Equals(..., StringComparison.OrdinalIgnoreCase)` en memoria (el patrón de `CategoriaNegocio`) — **descartada** porque exige materializar la tabla de clientes con `ToListAsync()` para comparar, lo que la spec prohíbe (RNF-2). *Alternativa descartada*: depender solo de la colación de SQL Server — descartada porque los tests con InMemory son sensibles a mayúsculas y el requisito quedaría sin cubrir.
- **D-T2 — Unicidad con consulta acotada y autoexclusión**: `AnyAsync(c => (!idExcluir.HasValue || c.IdCliente != idExcluir.Value) && c.Dni!.ToUpper() == documento)`; para el Correo, además `c.Correo != null && c.Correo != ""`. El mensaje de duplicado de Documento es **neutral**: si el cliente existente está inactivo lo informa y **no sugiere acciones** (RF-10). Para saber si el existente está inactivo se usa una proyección acotada (`Select(c => new { c.Estado }).FirstOrDefaultAsync(...)`) **solo cuando `AnyAsync` dio positivo**, nunca un listado completo.
  *Alternativa descartada*: comparar contra una lista de clientes traída a memoria — prohibida por RNF-2.
- **D-T3 — Búsquedas con `Contains` sobre `ToUpper()` y proyección acotada**: filtro por Documento o por `(Nombre + " " + Apellido)` con `Contains` sobre ambas partes en mayúsculas, respetando el filtro de estado; orden `OrderBy(Nombre).ThenBy(Apellido).ThenBy(IdCliente)` (equivale al orden por Nombre completo ascendente y es determinista ante nombres repetidos); la búsqueda del POS agrega `Where(c => c.Estado)` y `Take(20)`. Se usa `ToListAsync()` **solo sobre la proyección** `ClienteRespuestaDto` (permitido por la regla del usuario).
  *Alternativa descartada*: `EF.Functions.Like` — descartada porque su traducción difiere entre proveedores y complica la paridad InMemory/SQL Server; *Alternativa descartada*: traer coincidencias y recortar a 20 en memoria — descartada por RNF-2.
- **D-T4 — Texto de búsqueda vacío: criterio distinto en el padrón y en el POS (D-12)**: en `ListarClientesAsync`, si `textoBusqueda` es `null`, vacío o solo espacios, **no se aplica filtro de texto** y se consulta solo por el filtro de estado (`Activos`/`Inactivos`), de modo que al abrir la pantalla la grilla muestre todos los clientes del filtro. En `BuscarClientesParaPosAsync`, si el texto es `null`, vacío o solo espacios, se devuelve `SinResultados` con mensaje **sin consultar el origen de datos**. En ambos casos el texto se recorta antes de buscar cuando está informado.
- **D-T5 — Permiso de baja/reactivación dentro de `CapaNegocio`**: `PoliticaAcceso` gana `PuedeAdministrarClientes(string? rol)` (verdadero solo para `administrador` y `encargado`; `vendedor` falso). `DarBajaClienteAsync`, `ReactivarClienteAsync` y `ObtenerConfirmacionBajaClienteAsync` reciben `string? rol` y devuelven `PermisoDenegado` antes de tocar datos.
  *Alternativa descartada*: que la vista decida si muestra el botón y confíe en ello — descartada porque la regla quedaría solo en la UI (RF-27 exige rechazo en el negocio); *Alternativa descartada*: reutilizar `TieneAcceso(rol, "Clientes")` — insuficiente, porque el Vendedor también tiene acceso al módulo pero no puede dar de baja.
- **D-T6 — Confirmación de baja en una sola llamada**: `ObtenerConfirmacionBajaClienteAsync` valida permiso, existencia y estado, cuenta ventas **de todas las ventas asociadas (vigentes y anuladas, sin filtrar por `Estado`)**, y devuelve el texto final. La vista abre **un único** `MaterialDialog` con ese texto. *Alternativa descartada*: dos métodos (contar + confirmar) y que la vista redacte el mensaje — descartada por RF-26.
- **D-T7 — Errores de persistencia traducidos en el negocio**: los métodos públicos envuelven sus operaciones en `try/catch` (`DbUpdateException` y excepciones de acceso al origen de datos) y devuelven `ErrorPersistencia` con un mensaje en español; la vista nunca ve una excepción (RF-28).
  *Alternativa descartada*: dejar propagar la excepción a WinForms — descartada por RF-28/RNF-1.
- **D-T8 — DI y rol en `ClientesForm`** (patrón `ProductosForm`): constructor `ClientesForm(ClienteNegocio clienteNegocio)`; `Program.cs` registra `ClienteNegocio` y `ClientesForm` como transient; `inicio` recibe `ClientesForm` por constructor y reemplaza `new ClientesForm()` por la instancia DI; `AplicarPermisosPorRol()` invoca `_clientesForm.AplicarRolUsuario(rol)`. El menú no cambia: la pestaña `tab_clientes` ya existe y `OcultarSiNoAutorizada("tab_clientes", "Clientes", rol)` empieza a mostrarla al Encargado por D-8.
- **D-T9 — Estructura de la pantalla de Clientes** (dentro del `UserControl` existente): grilla real enlazada a `ClienteRespuestaDto` con columnas **Documento / Nombre completo / Correo / Teléfono / Estado** (estado como texto «Activo»/«Inactivo»); buscador del padrón; filtro **Activos/Inactivos** iniciado en Activos; botones Nuevo/Editar (los tres roles), Desactivar y **Reactivar** (solo Administrador/Encargado, ocultos para Vendedor); tras un alta exitosa la vista cambia el filtro a Activos y recarga (RF-20). Recarga al entrar en la pestaña (`Enter`) y con Actualizar.
- **D-T10 — Modal `ClienteForm`**: `MaterialForm` construido por código (patrón `CategoriaForm`), campos Documento, Nombre, Apellido, Correo y Teléfono; entrega los valores **crudos** a `CrearClienteAsync`/`ModificarClienteAsync`; si el resultado no es `Exito`, muestra `Mensaje` **sin cerrar** y conserva lo tipeado; si es `Exito`, cierra y la grilla recarga. No valida ni arma mensajes.

---

## 5. Estrategia de pruebas (`dotnet test`)

Archivos: **nuevo** `CapaNegocio.Tests/ClienteNegocioTests.cs`; **modificado** `CapaNegocio.Tests/PoliticaAccesoTests.cs` (hoy afirma que el Encargado **no** tiene «Clientes» → debe pasar a afirmar que sí, D-8). Patrón existente: `UseInMemoryDatabase($"ClienteNegocioTests-{Guid.NewGuid()}")`, `await using`, datos sembrados por test.

| RF | Casos a cubrir (mínimo) |
|---|---|
| RF-1 | Alta válida: persiste activo, NombreCompleto = «Nombre Apellido», sin datos de venta asociados |
| RF-2 | Documento/Nombre/Apellido ausentes, vacíos o solo espacios → `DatosInvalidos` con mensaje del campo; nada persistido |
| RF-3 | Recorte de todos los datos; se guarda recortado |
| RF-4 | Documento: 6 y 21 caracteres; con guion, punto, espacio o «Ñ» (teoría con los ejemplos de la spec); válidos «30123456», «AB1234567» |
| RF-5 / RF-6 | Nombre y Apellido: 1 y 101 caracteres; con dígitos o símbolos; válidos con tildes, ñ, espacios internos, apóstrofo y guion |
| RF-7 | Correo válido y correo `null`/vacío/solo espacios aceptado; inválidos de la spec (sin «@», sin dominio, sin punto, extensión de 1 letra, doble «@», con espacio) |
| RF-8 / RF-9 | Teléfono con guion, espacio, paréntesis o «+»; 5 y 16 dígitos; válido de 6 y de 15; `null`/vacío aceptado |
| RF-10 | Documento duplicado (activo) → `DocumentoDuplicado`; duplicado contra inactivo → mensaje neutral (no contiene «reactiv»); duplicado solo por mayúsculas/minúsculas → rechazado |
| RF-11 | Correo duplicado contra activo e inactivo; duplicado por mayúsculas; varios clientes sin correo → todos se aceptan |
| RF-12 | Modificar sin cambiar Documento/Correo (sin duplicado de sí mismo); Documento o Correo de otro cliente → rechazo; mismas validaciones de formato en edición |
| RF-13 | Vaciado de Correo y Teléfono en edición → quedan `null` y el cliente se actualiza |
| RF-14 | Editar cliente inactivo sin reactivarlo; el estado sigue inactivo |
| RF-15 | La edición no altera el estado ni las ventas asociadas |
| RF-16 | Filtro Activos devuelve solo activos; Inactivos solo inactivos; estado y NombreCompleto expuestos |
| RF-17 | Coincidencia parcial por Documento y por nombre; insensible a mayúsculas; respeta el filtro de estado; **buscador vacío/solo espacios → devuelve todos los clientes del filtro (no vacía)**, verificado en Activos y en Inactivos |
| RF-18 | Solo activos; `Take(20)` con 25 coincidencias; orden por Nombre completo; **texto vacío → lista vacía y sin consulta** (comportamiento distinto al padrón, D-12) |
| RF-19 | Sin coincidencias con **texto informado** → `SinResultados` con mensaje en padrón y en POS; con buscador vacío el padrón **no** devuelve `SinResultados` |
| RF-21 / RF-22 | Confirmación con y sin ventas; cantidad incluye **ventas anuladas** (`Estado = false`); tras la baja las ventas siguen existiendo |
| RF-23 | Reactivar pone `Estado = true` sobre la misma fila (sin filas nuevas) |
| RF-24 | Baja de inactivo → `ClienteYaInactivo` sin cambios; reactivar activo → `ClienteYaActivo` sin duplicar |
| RF-25 | Tras la baja, `db.Clientes.CountAsync()` no disminuye y el Id se mantiene |
| RF-27 | `PuedeAdministrarClientes`: Admin/Encargado `true`, Vendedor/rol nulo `false`; baja y reactivación con rol Vendedor → `PermisoDenegado` y estado sin cambios; Encargado puede ambas; `TieneAcceso("Encargado","Clientes")` `true` |
| RF-28 | Operación sobre un contexto inutilizable → `ErrorPersistencia` con mensaje en español (comportamiento verificado en el borde del negocio) |

**Advertencias de la estrategia**
- El proveedor **InMemory no valida índices únicos, colaciones ni longitudes de columna**: la unicidad se prueba en el negocio (RF-10/RF-11) y se verifica además **contra SQL Server real** al cerrar T1 (duplicado rechazado por el índice) y en la validación de cierre.
- InMemory **no** reproduce la colación insensible a mayúsculas: por eso la lógica usa `ToUpper()` explícito (D-T1) y los tests usan «maria»/«Maria».
- `CapaNegocio.Tests` apunta a `net10.0` mientras el resto de la solución declara .NET 8: es una condición preexistente, no se modifica en esta spec.

---

## 6. Trazabilidad RF → componente → tarea

| RF | Componente / método | Tarea |
|---|---|---|
| RF-1 | `ClienteNegocio.CrearClienteAsync` | T3 |
| RF-2, RF-3 | `ValidarObligatorios`, Recorte en `CrearClienteAsync`/`ModificarClienteAsync` | T3 |
| RF-4, RF-5, RF-6 | `ValidarDocumento`, `ValidarNombreOApellido` | T3 |
| RF-7, RF-8, RF-9 | `ValidarCorreo`, `ValidarTelefono` | T3 |
| RF-10, RF-11 | `ValidarUnicidadDocumentoAsync`, `ValidarUnicidadCorreoAsync` | T4 |
| RF-12, RF-13, RF-14, RF-15 | `ModificarClienteAsync` | T5 |
| RF-16, RF-17 | `ListarClientesAsync` (texto vacío ⇒ sin filtro de texto, D-12) + `ClientesForm` (grilla/buscador/filtro) | T6, T9 |
| RF-18, RF-19 | `BuscarClientesParaPosAsync`, `RespuestaListadoClientes.Mensaje` | T6 |
| RF-20 | `ClientesForm` (filtro a Activos tras alta) | T9, T10 |
| RF-21, RF-22 | `ObtenerConfirmacionBajaClienteAsync`, `ContarVentasClienteAsync`, `DarBajaClienteAsync` | T7 |
| RF-23 | `ReactivarClienteAsync` | T7 |
| RF-24 | `DarBajaClienteAsync` / `ReactivarClienteAsync` (ramas YaInactivo/YaActivo) + gating de botones | T7, T10 |
| RF-25 | Bajas lógicas en ambos métodos (sin `Remove`/`DELETE`) | T7 |
| RF-26 | Todos los mensajes en `ClienteNegocio`; vista solo muestra `Mensaje` | T2–T10 |
| RF-27 | `PoliticaAcceso` + parámetro `rol` + `AplicarRolUsuario` + `OcultarSiNoAutorizada` | T2, T7, T9, T10 |
| RF-28 | `try/catch` en métodos públicos de `ClienteNegocio` | T8 |
| RNF-2 | `IQueryable` + `AnyAsync`/`FirstOrDefaultAsync` + proyecciones + `Take(20)` | T4, T6 |
| RNF-4 | Configuración Fluent + migración `AddClienteConstraints` + SQL de verificación | T1 |
| RNF-5 | `ClienteNegocioTests` + `PoliticaAccesoTests` | T2–T8 |
| RNF-6 | `ClientesForm` dentro de la pestaña existente; sin tocar menú | T9, T10 |

---

## 7. Riesgos y mitigaciones

1. **Migración con datos reales desconocidos** (longitudes e índices): se ejecuta el SQL de verificación previa y se aborta sin modificar datos si hay violaciones (T1). La tabla no tiene semilla, pero la base real puede tener filas.
2. **Índice único case-insensitive vs validación del negocio**: ambos criterios coinciden (`UPPER`/colación por defecto). Si la base usara una colación *case-sensitive*, el índice sería más estricto que la validación; se documenta en T1 al aplicar la migración.
3. **`ToUpper()` impide el uso de índices en las búsquedas**: aceptable para el volumen de un padrón de taller; se prioriza paridad de comportamiento SQL/InMemory y el cumplimiento de RNF-2 (sin materializar tablas). Si el padrón creciera, se revisaría en una spec posterior (fuera de alcance).
4. **Pestaña Clientes con DI**: si `inicio` se resuelve antes de registrar `ClientesForm`, el arranque falla. Mitigación: agregar los registros en `Program.cs` junto a los existentes (T9) y compilar/ejecutar antes de tocar la UI.
5. **Encargado**: `PoliticaAccesoTests` actual afirma lo contrario de D-8; se corrige en la misma tarea que el cambio de política (T2) para no dejar la suite en rojo.
6. **Validación visual**: la suite no cubre WinForms; el cierre requiere el checklist manual de la pantalla (grilla, filtros, modal, gating por rol, confirmación única).
