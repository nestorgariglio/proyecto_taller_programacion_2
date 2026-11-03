# Plan técnico — Spec 004: Administración del Padrón de Proveedores
Estado: propuesto (pendiente de aprobación del usuario)
Spec de referencia: `specs/004-abm-proveedores/spec.md` — **aprobada 2026-10-28** con decisiones D-1 (CUIT solo dígitos `^[0-9]+$`) y D-2 (respecto estricto de tipos; sin restricciones de longitud arbitrarias). Correcciones aplicadas tras revisión QA (Cuit/RazonSocial `string` non-nullable; índices únicos case-insensitive con `ToUpper()` y `HasFilter`).
Fecha: 2026-10-28

> **Gate de aprobación pendiente**: la Tarea **T1** incluye una **migración de EF Core** (`AddProveedorConstraints`) con ajuste de nulabilidad (`Cuit`/`RazonSocial` a `string` non-nullable) y creación de índices únicos filtrados (`Cuit`, `Correo`). Según `AGENTS.md`, ninguna migración se crea ni aplica sin aprobación explícita del usuario. El resto de tareas no requiere aprobaciones adicionales.

> **Nota técnica (D-2 / índices)**: para declarar índices únicos sobre `Cuit` y `Correo` SQL Server requiere que las columnas no sean `nvarchar(max)`. La migración propone `.HasMaxLength()` con valores técnicos (`Cuit` 20, `RazonSocial` 200, `Correo` 254, `Telefono` 15) que permiten indexación sin restringir datos reales; cualquier extensión de restricción debe aprobarse por separado.

---

## 1. Responsabilidades por capa

### `CapaEntidad` — ajuste de nulabilidad
- `Proveedor.cs`: `Cuit` y `RazonSocial` pasan de `string?` a `string` (non-nullable) según RF-2 y corrección post-QA. `Correo` y `Telefono` se mantienen `string?` (opcionales). No se renombra ninguna columna (D-1 intacto: `id_proveedor`, `cuit`, `razon_social`, `correo`, `telefono`, `estado`, `fecha_registro`).
- **No** se agregan entidades ni relaciones nuevas; `Estado` (`bool`) y `FechaRegistro` (`DateTime`) se conservan.

### `CapaDatos` — Fluent API + migración aprobada
- `AppDbContext.OnModelCreating`: configuración de `Proveedor` (longitudes técnicas para indexación, índices únicos filtrados con `HasFilter`, sin `nvarchar(max)`).
- `DbSet<Proveedor>` ya existe; no se agrega ni se elimina.
- Nueva migración `AddProveedorConstraints` (T1, sujeta a aprobación). Antes de generarla: verificación previa de datos existentes (duplicados de `Cuit`/`Correo`, formatos inválidos, datos con espacios); si hay violaciones, informar y definir tratamiento sin borrado físico ni limpieza automática (RNF-4).
- **No** se modifica cadena de conexión ni `scripts/seed-desarrollo.sql`.

### `CapaNegocio` — DTOs, validaciones centralizadas, mensajes
- Nuevos DTOs en `CapaNegocio/DTOs/Proveedores/` (namespace `CapaNegocio.DTOs.Proveedores`): `CrearProveedorDto`, `ModificarProveedorDto`, `ProveedorRespuestaDto`, `RespuestaCreacionProveedor`, `RespuestaModificacionProveedor`, `RespuestaBajaProveedor`, `RespuestaActivacionProveedor`, `RespuestaListadoProveedores`, `RespuestaConfirmacionBajaProveedor`; enum `FiltroEstadoProveedor`; `ResultadoCreacionProveedor`, etc.
- `ProveedorNegocio.cs`: constructor con `AppDbContext`, mapeo explícito `MapearProveedor` (proyección `Select` para listar/buscar, nunca materializar la tabla completa — RNF-2). Regexp estáticas privadas (`^[0-9]+$` para `Cuit`; `^[^\s@]+@[^\s@]+\.[A-Za-z]{2,}$` para `Correo`; `^\d+$` para `Telefono`). Validaciones por regla: `ValidarObligatorios`, `ValidarCuit`, `ValidarRazonSocial`, `ValidarCorreo`, `ValidarTelefono`, `ValidarUnicidadCuitAsync`, `ValidarUnicidadCorreoAsync`. Autoexclusión por `idProveedorAExcluir`. Mensajes en español redactados por la lógica (`RF-25`).
- `PoliticaAcceso.cs`: ya incluye `"Proveedores"` en `administrador` y `encargado` (D-8 confirmado); agregar método `PuedeAdministrarProveedores(string? rol)` (verdadero para administrador y encargado, falso para vendedor y nulo/vacío) para usar en `ProveedorNegocio`.
- **No** se tocan `UsuarioNegocio` ni `CategoriaNegocio`.

### `CapaPresentacion` — pantalla existente, sin menú nuevo
- `ProveedoresForm.cs`: reemplazar grilla mockeada por datos reales; conectar buscador (texto vacío ⇒ todo el filtro de estado elegido, D-15), filtro Activos/Inactivos (inicio Activos), alta/edición vía modal, baja/reactivación con confirmación única, acciones ocultas/deshabilitadas por rol (`AplicarRolUsuario`), rótulo columna **Documento** (D-1).
- Nuevo modal `Proveedores/ProveedorForm.cs` (patrón `CategoriaForm`): envía valores **crudos** al negocio, conserva datos en error, muestra `Mensaje` sin modificarlo, cierra solo en éxito.
- `Program.cs`: registro de `ProveedorNegocio`; registro/DI de `ProveedoresForm`. `inicio`: inyectar `ProveedoresForm`, reemplazar `new ProveedoresForm()`, suscribir `tab_proveedores.Enter` para recargar, invocar `AplicarRolUsuario` desde `AplicarPermisosPorRol()`.
- **No** se crean pantallas ni ítems de menú; no se modifica menú principal (RNF-6).

---

## 2. DTOs, enums y mapeos

Carpeta `CapaNegocio/DTOs/Proveedores/` (namespace `CapaNegocio.DTOs.Proveedores`) y `CapaNegocio/Enums/`.

### DTOs de entrada
| DTO | Campos |
|---|---|
| `CrearProveedorDto` | `Cuit` (`string`), `RazonSocial` (`string`), `Correo` (`string?`), `Telefono` (`string?`) |
| `ModificarProveedorDto` | `IdProveedor` (`int`) + `Cuit`, `RazonSocial`, `Correo`, `Telefono` |

El alta siempre nace activa (`Estado = true`). La modificación conserva `Estado` y `FechaRegistro` (RF-14).

### DTO de salida
| DTO | Campos |
|---|---|
| `ProveedorRespuestaDto` | `IdProveedor`, `Cuit`, `RazonSocial`, `Correo`, `Telefono`, `Estado` (`bool` → texto «Activo»/«Inactivo»), `FechaRegistro` |

- No se expone ninguna entidad pura ni colecciones de compras.
- Mapeo explícito privado `MapearProveedor(Proveedor p) → ProveedorRespuestaDto` usado tras persistir; en consultas `Select(p => new ProveedorRespuestaDto { ... })` para no materializar completa (RNF-2).

### Respuestas y enums
| Respuesta | Campos | Enum / casos |
|---|---|---|
| `RespuestaCreacionProveedor` | `Resultado`, `Mensaje`, `Proveedor` (`ProveedorRespuestaDto?`) | `ResultadoCreacionProveedor`: `Exito`, `DatosInvalidos`, `CuitDuplicado`, `CorreoDuplicado`, `ErrorPersistencia` |
| `RespuestaModificacionProveedor` | `Resultado`, `Mensaje`, `Proveedor` | `ResultadoModificacionProveedor`: `Exito`, `DatosInvalidos`, `ProveedorNoEncontrado`, `CuitDuplicado`, `CorreoDuplicado`, `ErrorPersistencia` |
| `RespuestaBajaProveedor` | `Resultado`, `Mensaje`, `Proveedor` | `ResultadoBajaProveedor`: `Exito`, `DatosInvalidos`, `ProveedorNoEncontrado`, `ProveedorYaInactivo`, `PermisoDenegado`, `ErrorPersistencia` |
| `RespuestaActivacionProveedor` | `Resultado`, `Mensaje`, `Proveedor` | `ResultadoActivacionProveedor`: `Exito`, `DatosInvalidos`, `ProveedorNoEncontrado`, `ProveedorYaActivo`, `PermisoDenegado`, `ErrorPersistencia` |
| `RespuestaListadoProveedores` | `Resultado`, `Mensaje`, `Proveedores` (`IReadOnlyList<ProveedorRespuestaDto>`) | `ResultadoListadoProveedores`: `ConResultados`, `SinResultados`, `ErrorPersistencia` |
| `RespuestaConfirmacionBajaProveedor` | `Resultado`, `Mensaje`, `ComprasAsociadas` (`int`) | `ResultadoConfirmacionBajaProveedor`: `ConfirmacionRequerida`, `ProveedorNoEncontrado`, `ProveedorYaInactivo`, `PermisoDenegado`, `ErrorPersistencia` |

- `Mensaje` siempre informado (éxito o error). La pantalla solo lo muestra (`RF-25`).
- Éxitos: `"Proveedor {RazonSocial} registrado exitosamente."`, `"... modificado exitosamente."`, `"... dado de baja exitosamente."`, `"... reactivado exitosamente."`
- Confirmación de baja (única): mensaje redactado por negocio con cantidad de compras (todas, vigentes o anuladas, `D-6`). Si hay compras, el texto las informa y ofrece continuar/cancelar; nunca se bloquea la baja por compras (`D-6`).
- Mensaje de duplicado de `Cuit` con proveedor inactivo: neutral, sin sugerir reactivación ni acción (`D-4`).

### Métodos públicos de `ProveedorNegocio`
| Método | Devuelve | RF cubiertos |
|---|---|---|
| `CrearProveedorAsync(CrearProveedorDto)` | `RespuestaCreacionProveedor` | RF-1 a RF-11 |
| `ModificarProveedorAsync(ModificarProveedorDto)` | `RespuestaModificacionProveedor` | RF-12 a RF-15 |
| `ListarProveedoresAsync(FiltroEstadoProveedor, string? textoBusqueda = null)` | `RespuestaListadoProveedores` | RF-15, RF-16, RF-19 (texto vacío ⇒ sin filtro de texto, D-15) |
| `BuscarProveedoresParaPosAsync(string? textoBusqueda)` | `RespuestaListadoProveedores` | RF-17, RF-18 (texto vacío ⇒ `SinResultados` sin consultar, D-16) |
| `ContarComprasProveedorAsync(int idProveedor)` | `int` | RF-21 |
| `ObtenerConfirmacionBajaProveedorAsync(int idProveedor, string? rol)` | `RespuestaConfirmacionBajaProveedor` | RF-20, RF-21, RF-27 |
| `DarBajaProveedorAsync(int idProveedor, string? rol)` | `RespuestaBajaProveedor` | RF-20, RF-21, RF-24, RF-25, RF-27 |
| `ReactivarProveedorAsync(int idProveedor, string? rol)` | `RespuestaActivacionProveedor` | RF-22, RF-23, RF-24, RF-27 |

Validaciones privadas por regla: `ValidarCuit`, `ValidarRazonSocial`, `ValidarCorreo`, `ValidarTelefono`, `ValidarUnicidadCuitAsync`, `ValidarUnicidadCorreoAsync` (`ToUpper()` + `FirstOrDefaultAsync` con `Select` acotado si hay coincidencia). Regexp compiladas estáticas privadas.

---

## 3. Persistencia

### Configuración Fluent de `Proveedor` (T1)
En `AppDbContext.OnModelCreating`, junto a `Cliente`:

| Elemento | Configuración | Motivo técnico |
|---|---|---|
| `Cuit` | `.HasMaxLength(20).IsUnicode(false).IsRequired()` | Indexable (no LOB); obligatorio (`string` non-nullable, RF-2). |
| `RazonSocial` | `.HasMaxLength(200).IsRequired()` | Indexable; obligatorio (`string`, RF-2). |
| `Correo` | `.HasMaxLength(254)` | Indexable; opcional (`string?`). |
| `Telefono` | `.HasMaxLength(15).IsUnicode(false)` | Opcional; solo dígitos (`RF-7/8`). |
| `Estado` | sin cambio | `bool`, por defecto `true`. |
| Índice `Cuit` | `.HasIndex(p => p.Cuit).IsUnique().HasFilter("[cuit] IS NOT NULL")` | RNF-4; sin distinguir mayúsculas (colación BD + `ToUpper()` en negocio). |
| Índice `Correo` | `.HasIndex(p => p.Correo).IsUnique().HasFilter("[correo] IS NOT NULL AND [correo] <> ''")` | RNF-4; excluye no informados (`null`/vacío tras recorte). |

- Colación por defecto de SQL Server es insensible a mayúsculas: el índice rechaza duplicados con distinta capitalización; la validación previa replica con `ToUpper()` (`D-1`, `D-4`).
- **No** se renombran columnas; no se agregan restricciones de negocio sobre longitud (`D-2`).

---

## 4. Estrategia de migración / índices únicos

1. **Verificación previa** (antes de cualquier generación): consultar `Proveedores` por `Cuit` y `Correo` (con `ToUpper()`, sin materializar) para detectar duplicados de datos existentes; informar al usuario sin borrar ni limpiar. Si hay duplicados, su tratamiento se define con el coordinador (`RNF-4`).
2. **Ajuste de entidad** (`Cuit`/`RazonSocial` a `string`).
3. **Migración EF Core** `AddProveedorConstraints`: refleja `IsRequired()` en `Cuit`/`RazonSocial`, longitudes técnicas y los dos índices únicos filtrados.
4. **Aplicación** (`dotnet ef database update`) solo tras aprobación explícita (`AGENTS.md`).
5. **Validación post-migración**: intentar duplicados de `Cuit` (distinta capitalización) y `Correo` contra la BD real para confirmar que el índice rechaza ambos (`RNF-4`).

---

## 5. Riesgos y decisiones

| ID / Decisión | Estado | Impacto en este plan |
|---|---|---|
| **D-1** (CUIT: solo dígitos) | Cero | Regex `^[0-9]+$`; columna `cuit`; mensaje de rechazo en español. |
| **D-2** (respetar tipos; sin longitudes arbitrarias) | Cero | Entidad sin `[MaxLength]`; índices requieren `.HasMaxLength()` técnico (`Cuit` 20, `RazonSocial` 200, etc.) — documentado como técnico, no restrictivo de negocio. |
| **D-4** (unicidad insensible a mayúsculas; mensaje neutral si inactivo) | Cero | `ToUpper()` en `AnyAsync`; mensaje duplicado de `Cuit` no sugiere reactivación. |
| **D-6** (compras asociadas incluidas anuladas; baja nunca bloqueada) | Cero | `ContarComprasProveedorAsync` sin filtrar `Estado`; confirmación única con cantidad total. |
| **D-8** (Encargado habilitado) | Cero | `PoliticaAcceso` ya lo incluye; `PuedeAdministrarProveedores` confirma; acciones disponibles. |
| **D-10** (Vendedor: consultar/buscar/crear/editar; NO baja/reactiva) | Cero | `AplicarRolUsuario` oculta `btnDesactivar`/`btnReactivar`; `ProveedorNegocio` rechaza con `PermisoDenegado`. |
| **D-15** (buscador vacío ⇒ todo el filtro) | Cero | `ListarProveedoresAsync` con texto nulo/vacío/apenas espacios ⇒ sin filtro de texto; `ConResultados`. |
| **D-16** (búsqueda rápida texto vacío ⇒ lista vacía sin consultar) | Cero | `BuscarProveedoresParaPosAsync` corta inmediatamente a `SinResultados`; `.Take(20)`; orden por `RazonSocial`. |

- **Riesgo técnico**: datos existentes que violan `Cuit`/`Correo` duplicados o con espacios. Mitigación: verificación previa antes de migración; tratamiento documentado sin borrado físico.
- **Riesgo visual**: `ProveedoresForm` maquetada con datos mockeados; reemplazo debe conservar estilos de MaterialSkin. Mitigación: tarea T6 (armonización con `ClientesForm`/`ProductosForm`).
- **Riesgo de permisos**: `Encargado` habilitado en `PoliticaAcceso` pero podría faltar en UI. Mitigación: `AplicarRolUsuario` y `PuedeAdministrarProveedores` en T4/T6.

---

## 6. Esquema de tareas (resumen)

| Tarea | Capa(s) | RF clave | Dependencias |
|---|---|---|---|
| **T1** Migración EF Core + verificación previa (`AppDbContext` + índices + ajuste nulabilidad `Cuit`/`RazonSocial`) | Datos | RNF-4, RF-2, D-1, D-2 | Ninguna (primera) — sujeta a aprobación |
| **T2** Entidad (`Proveedor.cs`: `Cuit`/`RazonSocial` a `string`) | Entidad | RF-2 | T1 (migración reflejada) |
| **T3** DTOs + enums + `PoliticaAcceso` (`ProveedorNegocio` esqueleto) | Negocio | RF-25, RF-26, D-8, D-10 | T1, T2 |
| **T4** `ProveedorNegocio` (validaciones, unicidad, baja, reactivación, mensajes) | Negocio | RF-1 a RF-14, RF-20 a RF-27 | T3 |
| **T5** Tests `ProveedorNegocio` (red a verde por RF) | Negocio (tests) | RF-2 a RF-27, RNF-5 | T4 |
| **T6** `ProveedoresForm` (grilla real, buscador, filtro, modal, acciones por rol) | Presentación | RF-15 a RF-19, RF-26, RNF-6 | T4, T5 |
| **T7** Seed proveedores (`scripts/preparar-bd.ps1`) | Scripts / Datos | — | T1 |
| **T8** Validación visual + build + tests verdes | Integración | Todos | T6, T7 |

---

## 7. Criterios de cierre por tarea

- **T1**: verificación previa documentada (sin borrado), migración generada (`dotnet ef migrations add`), aplicada (`dotnet ef database update`) solo con aprobación; `dotnet build`; índices confirmados contra SQL Server (duplicado rechazado); datos existentes no violan reglas (o tratamiento definido).
- **T2**: `Cuit` y `RazonSocial` son `string` (no `string?`); `dotnet build`; entidad compilada sin errores.
- **T3**: DTOs en carpeta correcta; `RespuestaCreacionProveedor`/etc. con campos requeridos; `PoliticaAcceso` actualizado (`PuedeAdministrarProveedores`); `dotnet test` pasa en `PoliticaAccesoTests`; `dotnet build`.
- **T4**: `ProveedorNegocio` compila; todos los métodos públicos presentes; validaciones por regla; mensajes en español; sin `ToList()`/`ToListAsync()` sobre tabla completa para unicidad o búsqueda; `dotnet test` verde (T5 debe seguir, pero T4 debe ser compilable y con métodos públicos disponibles).
- **T5**: `dotnet test` pasa con casos de éxito y fallo por cada RF cubierto (obligatoriedad, recorte, formatos, unicidad, autoexclusión, baja/reactivación, búsqueda, sin resultados, permisos, fallo de datos); ninguna excepción propagada.
- **T6**: `dotnet build`; pantalla operativa con datos reales; grilla no mockeada; buscador vacío muestra todo el filtro (Activos por defecto); filtro Activos/Inactivos alterna; alta/edición con modal; baja con confirmación única (compra incluida si aplica); acciones ocultas para Vendedor; no hay mensajes redactados por la vista.
- **T7**: `scripts/preparar-bd.ps1` contiene inserción de proveedores de ejemplo; `dotnet build`; datos disponibles tras ejecución del script.
- **T8**: `dotnet build` limpia; `dotnet test` completo verde; verificación visual cumplida (grilla, buscador vacío, filtro, alta, edición, baja con confirmación, reactivación, acciones por rol, rótulo Documento); ninguna fila mockeada; ninguna baja física.

---

*Nota para el coordinador*: presentar resúmenes y pares hasta aprobación explícita del usuario. No iniciar implementación hasta que el usuario apruebe `plan.md` + `tasks.md`. No escribir código C# en esta fase.
