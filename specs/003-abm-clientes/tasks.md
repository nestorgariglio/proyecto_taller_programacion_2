# Tareas — Spec 003: Administración del Padrón de Clientes
Plan de referencia: `specs/003-abm-clientes/plan.md` (propuesto)
Regla de trabajo: **una tarea a la vez**, pruebas primero (rojo → verde), `dotnet build` y `dotnet test` en verde antes de marcar la tarea y **parar**.

> ⚠️ **T1 requiere aprobación explícita del usuario antes de ejecutarse** (creación y aplicación de la migración `AddClienteConstraints`, según `AGENTS.md`).

- [x] **T1. `CapaDatos`: configuración Fluent de `Cliente`, verificación previa de datos y migración `AddClienteConstraints`.** RF-4, RF-7, RF-10, RF-11, RNF-4
  - Ejecutar el SQL de verificación previa del plan (duplicados de documento y correo, documentos con formato inválido, longitudes excedidas, teléfonos no numéricos) y **abortar sin modificar datos** si devuelve filas, informando al usuario.
  - En `AppDbContext.OnModelCreating`: longitudes (`Dni` 20 no unicode, `Nombre`/`Apellido` 100, `Correo` 254, `Telefono` 15 no unicode) e índices únicos filtrados (`[dni] IS NOT NULL` y `[correo] IS NOT NULL AND [correo] <> ''`), sin renombrar columnas.
  - `dotnet tool restore` y `dotnet ef migrations add AddClienteConstraints --project CapaDatos --startup-project CapaDatos --context AppDbContext`; aplicar con `dotnet ef database update …` **solo con la aprobación del usuario**.
  - Hecho cuando: la verificación previa está documentada sin violaciones (o con su tratamiento acordado), la migración está generada y aplicada, el snapshot refleja los índices y `dotnet build` compila sin errores; se comprueba contra SQL Server que un documento repetido (también con distinta capitalización) es rechazado por el índice.

- [x] **T2. `CapaNegocio`: DTOs, enums, `PoliticaAcceso` (Encargado + permiso de administración del padrón) y esqueleto de `ClienteNegocio`.** RF-27 (soporte de RF-1 a RF-28)
  - Crear `DTOs/Clientes/`: `ClienteRespuestaDto`, `CrearClienteDto`, `ModificarClienteDto`, `RespuestaCreacionCliente`, `RespuestaModificacionCliente`, `RespuestaBajaCliente`, `RespuestaActivacionCliente`, `RespuestaConfirmacionBajaCliente`, `RespuestaListadoClientes` con los campos del plan.
  - Crear enums: `ResultadoCreacionCliente`, `ResultadoModificacionCliente`, `ResultadoBajaCliente`, `ResultadoActivacionCliente`, `ResultadoConfirmacionBajaCliente`, `ResultadoListadoClientes`, `FiltroEstadoCliente`.
  - `PoliticaAcceso`: agregar `"Clientes"` al rol `encargado` (D-8) y el método `PuedeAdministrarClientes(string? rol)` (verdadero solo para administrador y encargado).
  - Crear `ClienteNegocio` con su constructor (`AppDbContext`) y el mapeo privado a `ClienteRespuestaDto` (NombreCompleto = `Nombre + " " + Apellido`).
  - Escribir/ajustar **primero** `PoliticaAccesoTests`: Encargado ahora sí tiene «Clientes»; `PuedeAdministrarClientes` verdadero para Administrador/Encargado y falso para Vendedor/rol nulo/vacío.
  - Hecho cuando: `dotnet test` pasa con los tests de `PoliticaAcceso` actualizados (el test que hoy niega «Clientes» al Encargado ya no existe en su forma anterior) y `dotnet build` compila sin errores.

- [x] **T3. `CapaNegocio`: alta de cliente con obligatoriedad, Recorte, longitudes y formatos.** RF-1, RF-2, RF-3, RF-4, RF-5, RF-6, RF-7, RF-8, RF-9
  - Escribir en `ClienteNegocioTests` (rojo) los casos de la tabla del plan para RF-1 a RF-9, incluidos los ejemplos válidos e inválidos de la spec y los mensajes en español por campo.
  - Implementar `CrearClienteAsync` con `ValidarObligatorios`, Recorte, `ValidarDocumento`, `ValidarNombreOApellido`, `ValidarCorreo`, `ValidarTelefono` (estáticas privadas, expresiones regulares estáticas) y persistencia con `Estado = true` y `FechaRegistro` por defecto; «no informado» se guarda como `null`.
  - Hecho cuando: `dotnet test` pasa con los casos de RF-1 a RF-9 en verde (éxito y fallo por cada regla) y `dotnet build` compila sin errores.

- [x] **T4. `CapaNegocio`: unicidad de Documento y de Correo en el alta, sin materializar la tabla.** RF-10, RF-11
  - Escribir (rojo) los casos de duplicado: documento activo, documento inactivo con **mensaje neutral** (no menciona «reactiv»), diferencia solo de mayúsculas; correo duplicado activo/inactivo/mayúsculas; varios clientes sin correo aceptados.
  - Implementar `ValidarUnicidadDocumentoAsync` y `ValidarUnicidadCorreoAsync` con `AnyAsync` sobre `ToUpper()` y autoexclusión opcional; consultar el estado del existente con proyección acotada (`Select(c => new { c.Estado }).FirstOrDefaultAsync`) solo si hay coincidencia.
  - Hecho cuando: `dotnet test` pasa con RF-10 y RF-11 en verde y el código revisado no contiene `ToList()`/`ToListAsync()` sobre `DbSet<Cliente>` para comparar.

- [x] **T5. `CapaNegocio`: modificación con autoexclusión, vaciado de datos opcionales y clientes inactivos.** RF-12, RF-13, RF-14, RF-15
  - Escribir (rojo) edición sin cambiar Documento/Correo (no es duplicado de sí mismo), Documento/Correo de otro cliente, mismas validaciones de formato en edición, vaciado de Correo y Teléfono, edición de un cliente inactivo y conservación de estado y ventas asociadas.
  - Implementar `ModificarClienteAsync`: validaciones equivalentes al alta, unicidad con `idClienteAExcluir`, `ClienteNoEncontrado` si no existe, `null` para datos opcionales vacíos y sin tocar `Estado` ni las ventas.
  - Hecho cuando: `dotnet test` pasa con RF-12 a RF-15 en verde, incluida la comprobación de que el cliente inactivo sigue inactivo después de editarse.

- [x] **T6. `CapaNegocio`: listado con filtro de estado y búsquedas (padrón y POS).** RF-16, RF-17, RF-18, RF-19
  - Escribir (rojo) filtro Activos/Inactivos, coincidencia parcial por Documento y por Nombre completo insensible a mayúsculas, respeto del filtro de estado, búsqueda del POS solo activos con 25 coincidencias → 20 resultados ordenados por Nombre completo, y **los dos comportamientos de texto vacío (D-12)**: en el padrón, buscador vacío o solo con espacios → **todos** los clientes del filtro (`ConResultados`, nunca `SinResultados` por falta de texto); en el POS, texto vacío o solo con espacios → `SinResultados` **sin consultar**.
  - Implementar `ListarClientesAsync(FiltroEstadoCliente, string? textoBusqueda)` —sin filtro de texto cuando el texto es nulo, vacío o solo espacios, aplicando solo el filtro de estado— y `BuscarClientesParaPosAsync(string?)` —con corte inmediato a `SinResultados` cuando el texto no está informado—, ambos con `IQueryable`, `ToUpper().Contains`, `OrderBy(Nombre).ThenBy(Apellido).ThenBy(IdCliente)`, `Take(20)` (solo en POS) y proyección a `ClienteRespuestaDto`; los mensajes de «sin resultados» se redactan en el negocio.
  - Hecho cuando: `dotnet test` pasa con RF-16 a RF-19 en verde —incluidos el listado con buscador vacío en Activos e Inactivos y la búsqueda del POS con texto vacío— y el código revisado no materializa la tabla completa de clientes.
  - Nota D-12 (2026-10-28): el comportamiento del padrón con buscador vacío cambió respecto de la redacción original (ahora lista todo el filtro). Los casos de RF-17 deben re-ejecutarse/ajustarse; el estado de esta tarea lo decide el coordinador.

- [x] **T7. `CapaNegocio`: confirmación única de baja, baja lógica, reactivación, ventas históricas y permiso por rol.** RF-21, RF-22, RF-23, RF-24, RF-25, RF-27
  - Escribir (rojo) los casos: confirmación con y sin ventas, cantidad de ventas que **incluye anuladas** (`Estado = false`), baja exitosa sin borrar la fila, reactivación sobre la misma fila, baja de inactivo → `ClienteYaInactivo`, reactivación de activo → `ClienteYaActivo`, y Vendedor → `PermisoDenegado` sin cambios mientras Encargado/Administrador sí pueden.
  - Implementar `ContarVentasClienteAsync` (todas las ventas del cliente, sin filtrar `Estado`), `ObtenerConfirmacionBajaClienteAsync`, `DarBajaClienteAsync` y `ReactivarClienteAsync`, con validación de rol vía `PoliticaAcceso.PuedeAdministrarClientes` y sin `Remove`/borrado físico.
  - Hecho cuando: `dotnet test` pasa con RF-21 a RF-25 y RF-27 en verde, incluida la verificación de que la cantidad de filas de `Clientes` no disminuye tras una baja.

- [x] **T8. `CapaNegocio`: traducción de fallos del origen de datos a mensajes de negocio.** RF-28
  - Escribir (rojo) al menos un caso por método principal (alta, modificación, baja, reactivación, listado) en el que la operación de datos falla y se espera `ErrorPersistencia` con mensaje en español, sin excepción propagada.
  - Implementar el `try/catch` de borde en los métodos públicos de `ClienteNegocio` (fallos al guardar y al consultar el origen de datos), devolviendo el resultado de error correspondiente y garantizando que no queden datos parciales.
  - Hecho cuando: `dotnet test` pasa con los casos de RF-28 en verde y ningún método público lanza excepciones de datos hacia la presentación.

- [x] **T9. `CapaPresentacion`: DI de `ClientesForm`, grilla real, buscador y filtro de estado con gating por rol.** RF-16, RF-17, RF-19, RF-20, RF-26, RF-27
  - Registrar `ClienteNegocio` y `ClientesForm` en `Program.cs`; inyectar `ClientesForm` en el constructor de `inicio` y reemplazar `new ClientesForm()` de `inicio_Load` por la instancia DI; agregar `AplicarRolUsuario(string?)` y su llamada desde `AplicarPermisosPorRol()` (sin tocar el menú).
  - En `ClientesForm`: eliminar el `DataTable` mockeado, enlazar la grilla a `ClienteRespuestaDto` con columnas **Documento / Nombre completo / Correo / Teléfono / Estado** (texto «Activo»/«Inactivo»), conectar el buscador y el filtro Activos/Inactivos con inicio en **Activos**, recargar al entrar en la pestaña y mostrar únicamente el `Mensaje` que devuelve el negocio cuando no hay resultados.
  - Con el buscador vacío (D-12): al abrir la pestaña, al cambiar de filtro, al pulsar Actualizar y **tras un alta exitosa** se llama a `ListarClientesAsync` con el texto vacío y el filtro vigente (Activos por defecto), de modo que la grilla muestre los clientes de ese filtro; el aviso de «sin resultados» solo se muestra cuando hay **texto informado** sin coincidencias.
  - Hecho cuando: `dotnet build` compila sin errores y, en ejecución, la pestaña Clientes muestra datos reales (sin filas de ejemplo), el filtro inicia en Activos, la grilla no queda vacía al entrar con el buscador vacío, el buscador filtra por Documento y nombre, y la pestaña es visible para Administrador, Encargado y Vendedor.

- [x] **T10. `CapaPresentacion`: modal de alta/edición y acciones de baja/reactivación con confirmación única.** RF-1 a RF-15 (interfaz), RF-21, RF-22, RF-23, RF-24, RF-26, RNF-6
  - Crear `Clientes/ClienteForm.cs` (patrón `CategoriaForm`): campos Documento, Nombre, Apellido, Correo y Teléfono; envía valores **crudos** al negocio, muestra el `Mensaje` devuelto sin cerrar cuando hay error y conserva lo tipeado; al guardar con éxito cierra y la grilla recarga.
  - Conectar Nuevo/Editar con el modal (Editar precarga desde la fila seleccionada), Desactivar con `ObtenerConfirmacionBajaClienteAsync` + **un único** diálogo y posterior `DarBajaClienteAsync`, y Reactivar con confirmación simple; ocultar Desactivar/Reactivar al Vendedor y deshabilitarlas según el estado de la fila.
  - Hecho cuando: `dotnet build` y `dotnet test` (suite completa) quedan en verde y la validación visual del checklist se cumple: alta (con el filtro pasando a Activos, el buscador vacío y el cliente visible junto al resto), edición de un cliente inactivo, rechazo con mensaje del negocio sin cerrar el modal, baja con y sin ventas mostrando **un solo** diálogo con la cantidad, reactivación, y acciones ocultas/deshabilitadas para el Vendedor.

---

> **Corrección visual (enmienda técnica post-merge, sin cambio de RF)** — tareas adicionales de la spec aprobada `003-abm-clientes`; referencia estándar: `ProductosForm.cs`.

- [x] **C1. `CapaPresentacion`: armonizar grilla (`DataGridView`) en `ClientesForm.cs`.** V-G1 / `ClientesForm.cs` 226–255 / `ProductosForm.cs` 1333–1357
  - Ajustar `BackgroundColor = Color.FromArgb(48,48,48)`; `ColumnHeadersDefaultCellStyle` (`BackColor = 55,71,79`, `Font = new Font("Segoe UI",11F,FontStyle.Bold)`, `ForeColor = White`); `DefaultCellStyle` (`BackColor = 48,48,48`, `Font = new Font("Segoe UI",10F)`, `SelectionBackColor = 255,87,34`, `SelectionForeColor = White`); `GridColor = 80,80,80`; `RowTemplate.Height = 36`; conservar `EnableHeadersVisualStyles = false`, `BorderStyle = None`, `CellBorderStyle = SingleHorizontal`.
  - Hecho cuando: `dotnet build` limpia y la grilla muestra la paleta de referencia (verificado visualmente al abrir `ClientesForm` con datos reales); no se alteran columnas ni `DataPropertyName`.

- [x] **C2. `CapaPresentacion`: armonizar botones (`MaterialButton`) en `ClientesForm.cs`.** V-B1 / `ClientesForm.cs` 174–217 / `ProductosForm.cs` 1180–1291
  - En `_btnNuevo`, `_btnEditar`, `_btnDesactivar`, `_btnReactivar`, `_btnActualizar`: agregar `AutoSizeMode = AutoSizeMode.GrowAndShrink`; `HighEmphasis = true`; `Type = MaterialButton.MaterialButtonType.Contained`; `Margin = new Padding(4,6,4,6)`; `Depth = 0` (por defecto); mantener `UseAccentColor = true` solo en `_btnNuevo`; ajustar `Height` a 40 (o dejar por `AutoSizeMode`); eliminar `Width` fijo donde sea compatible con auto-size.
  - Hecho cuando: los cinco botones muestran la misma altura, margen y énfasis que los de `ProductosForm`; `dotnet build` limpia.

- [x] **C3. `CapaPresentacion`: armonizar panel de acciones y mensaje vacío (`FlowLayoutPanel` + `_lblEstadoListado`).** V-L1 / V-M1 / `ClientesForm.cs` 162–172 y 30,153–159 / `ProductosForm.cs` 1308–1309 y 377
  - Panel: `BackColor = TemaAplicacion.FondoPrincipal`; `Padding = new Padding(0,2,0,2)`; `Margin = new Padding(0,4,0,4)`; conservar `FlowLayoutPanel` y `Anchor`; no migrar a `TableLayoutPanel`.
  - Mensaje: en `_lblEstadoListado` ajustar `FontType = MaterialSkinManager.fontType.Body2` (ya existe), `ForeColor = Color.Gainsboro` (o `255,87,34` solo si `respuesta.Resultado == ErrorPersistencia`, como hoy); confirmar que se usa `label` dinámico (`_lblEstadoListado.Text = respuesta.Mensaje`) y **no** se introduce `Paint`.
  - Hecho cuando: panel de acciones alineado con el estándar visual; mensaje vacío se muestra y actualiza sin cambios de comportamiento (RF-26); `dotnet build` limpia.
