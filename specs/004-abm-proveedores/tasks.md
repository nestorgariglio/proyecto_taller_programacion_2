# Tareas â€” Spec 004: AdministraciÃ³n del PadrÃ³n de Proveedores
Plan de referencia: `specs/004-abm-proveedores/plan.md` (propuesto)
Regla de trabajo: **una tarea a la vez**, pruebas primero (rojo â†’ verde), `dotnet build` y `dotnet test` en verde antes de marcar la tarea y **parar**.

> âš ï¸ **T1 requiere aprobaciÃ³n explÃ­cita del usuario antes de ejecutarse** (creaciÃ³n y aplicaciÃ³n de la migraciÃ³n `AddProveedorConstraints`, segÃºn `AGENTS.md`).

---

- [ ] **T1. `CapaDatos`: verificaciÃ³n previa + migraciÃ³n EF Core (`AppDbContext`) para `Proveedor` (Ã­ndices Ãºnicos filtrados + ajuste nulabilidad `Cuit`/`RazonSocial`).** RNF-4, RF-2, D-1, D-2
  - Ejecutar SQL de verificaciÃ³n previa (`SELECT cuit, razon_social, correo FROM PROVEEDOR` con `ToUpper()` / `TRIM`) para detectar duplicados de `Cuit` o `Correo` y datos con espacios; informar sin modificar ni borrar (RNF-4).
  - En `AppDbContext.OnModelCreating`: configurar `Proveedor` (`Cuit` `.HasMaxLength(20).IsUnicode(false).IsRequired()`; `RazonSocial` `.HasMaxLength(200).IsRequired()`; `Correo` `.HasMaxLength(254)`; `Telefono` `.HasMaxLength(15).IsUnicode(false)`), Ã­ndices Ãºnicos (`Cuit` con `.HasFilter("[cuit] IS NOT NULL")`; `Correo` con `.HasFilter("[correo] IS NOT NULL AND [correo] <> ''")`), sin renombrar columnas ni agregar restricciones arbitrarias de negocio.
  - `dotnet tool restore`; `dotnet ef migrations add AddProveedorConstraints --project CapaDatos --startup-project CapaDatos --context AppDbContext`; aplicar `dotnet ef database update â€¦` **solo con aprobaciÃ³n del usuario**.
  - Hecho cuando: verificaciÃ³n previa documentada sin violaciones (o tratamiento acordado); migraciÃ³n generada y aplicada; `dotnet build` limpio; SQL Server confirma que duplicado de `Cuit` (distinta capitalizaciÃ³n) y `Correo` es rechazado por el Ã­ndice; datos existentes no violan reglas o su tratamiento estÃ¡ definido.

- [x] **T2. `CapaEntidad`: ajustar `Proveedor.cs` (`Cuit` y `RazonSocial` a `string` non-nullable).** RF-2, D-2
  - Cambiar `public string? Cuit` â†’ `public string Cuit`; `public string? RazonSocial` â†’ `public string RazonSocial`; mantener `Correo`/`Telefono` como `string?`; conservar `[Table("PROVEEDOR")]` y nombres de columna.
  - Hecho cuando: `dotnet build` compila; ninguna referencia a `string?` en `Cuit`/`RazonSocial`; entidad reflejada correctamente.

- [x] **T3. `CapaNegocio`: DTOs de proveedor, enums y esqueleto de `ProveedorNegocio` + `PoliticaAcceso`.** RF-25, RF-26, D-8, D-10
  - Crear carpeta `CapaNegocio/DTOs/Proveedores/`: `CrearProveedorDto`, `ModificarProveedorDto`, `ProveedorRespuestaDto`, `RespuestaCreacionProveedor`, `RespuestaModificacionProveedor`, `RespuestaBajaProveedor`, `RespuestaActivacionProveedor`, `RespuestaListadoProveedores`, `RespuestaConfirmacionBajaProveedor`; enums `ResultadoCreacionProveedor`, `ResultadoModificacionProveedor`, `ResultadoBajaProveedor`, `ResultadoActivacionProveedor`, `ResultadoListadoProveedores`, `FiltroEstadoProveedor`.
  - `PoliticaAcceso`: agregar mÃ©todo `PuedeAdministrarProveedores(string? rol)`; confirmar que `encargado` y `administrador` ya incluyen `"Proveedores"` (D-8).
  - Crear `ProveedorNegocio` con constructor (`AppDbContext` inyectado), mÃ©todo privado `MapearProveedor`, y mÃ©todos pÃºblicos declarados (sin implementaciÃ³n completa aÃºn).
  - Escribir primero `PoliticaAccesoTests` actualizado (Encargado tiene Proveedores; `PuedeAdministrarProveedores` verdadero para Administrador/Encargado; falso para Vendedor/nulo).
  - Hecho cuando: `dotnet test` pasa con `PoliticaAccesoTests`; `dotnet build`; DTOs y `ProveedorNegocio` compilan con mÃ©todos pÃºblicos presentes.

- [x] **T4. `CapaNegocio`: `ProveedorNegocio` completo (validaciones, unicidad, baja/reactivaciÃ³n, mensajes, bÃºsquedas).** RF-1 a RF-27, RNF-2, D-1, D-4, D-6, D-15, D-16
  - Implementar `CrearProveedorAsync`: recorte (`Trim()`), obligatorios (`Cuit`, `RazonSocial`), `ValidarCuit` (`^[0-9]+$`), `ValidarRazonSocial`, `ValidarCorreo` (solo si informado), `ValidarTelefono` (solo si informado), `ValidarUnicidadCuitAsync` (`AnyAsync` + `ToUpper()` + autoexclusiÃ³n opcional), persistencia con `Estado = true`; mensajes en espaÃ±ol.
  - Implementar `ModificarProveedorAsync`: mismas validaciones, autoexclusiÃ³n (`idProveedorAExcluir`), conservaciÃ³n de `Estado` (`RF-13`), `Correo`/`Telefono` vacÃ­os â†’ `null` (`RF-12`).
  - Implementar `ListarProveedoresAsync`: filtro por `Estado` (`FiltroEstadoProveedor`); texto vacÃ­o/nulo/apenas espacios â‡’ sin filtro de texto (`D-15`); coincidir `Cuit`/`RazonSocial` parcial con `ToUpper().Contains`; `IQueryable`; `Select` a DTO; `OrderBy(p => p.RazonSocial)`.
  - Implementar `BuscarProveedoresParaPosAsync`: solo activos; texto vacÃ­o/nulo/apenas espacios â‡’ `SinResultados` inmediato sin consultar (`D-16`); `.Take(20)`; orden por `RazonSocial` ascendente.
  - Implementar baja/reactivaciÃ³n: `ContarComprasProveedorAsync` (todas, sin filtrar `Estado`), `ObtenerConfirmacionBajaProveedorAsync` (mensaje con cantidad; una sola confirmaciÃ³n), `DarBajaProveedorAsync` (`Estado = false`, sin `Remove`), `ReactivarProveedorAsync` (`Estado = true`); validar rol (`PuedeAdministrarProveedores`) para baja/reactivaciÃ³n (`D-10`).
  - Hecho cuando: `dotnet test` pasa con casos de RF-1 a RF-27 cubiertos (Ã©xito y fallo por regla); cÃ³digo revisado sin `ToList()`/`ToListAsync()` sobre `DbSet<Proveedor>` para comparar o buscar; mensajes provenientes solo del negocio (`RF-25`).

- [x] **T5. `CapaNegocio`: tests automÃ¡ticos (`ProveedorNegocioTests`) como puerta de entrada.** RF-2 a RF-27, RNF-5, D-1, D-4, D-6, D-15, D-16
  - Escribir (rojo) casos por regla: obligatoriedad (`Cuit`/`RazonSocial`), recorte (`RF-3`), formato `Cuit` (`^[0-9]+$`), `RazonSocial`, `Correo`, `Telefono`; duplicados de `Cuit` (activo/inactivo, distinta capitalizaciÃ³n, mensaje neutral si inactivo `D-4`); duplicado `Correo`; autoexclusiÃ³n (`RF-11`); vaciado opcionales (`RF-12`); ediciÃ³n de inactivo (`RF-13`); listado con filtro (vacÃ­o y informado, `D-15`); bÃºsqueda rÃ¡pida (`D-16`, `Take(20)`, orden); sin resultados (`RF-18`); baja con/sin compras (incluidas anuladas, `D-6`); reactivaciÃ³n; improcedente (`RF-23`); prohibiciÃ³n borrado fÃ­sico (`RF-24`); mensajes (`RF-25`); permisos por rol (`RF-26`); fallo de datos (`RF-27`).
  - Hecho cuando: `dotnet test` verde para todos los casos; al menos un caso de Ã©xito y uno de fallo por regla; ninguna excepciÃ³n no capturada propagada.

- [ ] **T6. `CapaPresentacion`: conectar `ProveedoresForm` con datos reales, buscador, filtro, modal y acciones por rol.** RF-15 a RF-19, RF-26, RNF-6, D-10, D-15, D-16
  - Reemplazar grilla mockeada por `DataGridView` enlazado a `ProveedorRespuestaDto`; columnas: Documento (`Cuit`), RazÃ³n Social, Correo, TelÃ©fono, Estado (texto Â«ActivoÂ»/Â«InactivoÂ»), Fecha Registro; rÃ³tulo **Documento** (`D-1`).
  - Conectar `_txtBuscador`: vacÃ­o â‡’ `ListarProveedoresAsync` con filtro vigente (Activos por defecto); texto informado â‡’ filtrado parcial; respetar filtro Activos/Inactivos.
  - Filtro `Activos`/`Inactivos`: inicio `Activos`; alternar; cada cambio recarga.
  - Modal `ProveedorForm`: alta/ediciÃ³n; envÃ­a valores crudos; muestra mensaje de negocio sin cerrar en error; cierra y recarga grilla en Ã©xito; conserva datos ingresados (`RNF-7`).
  - Botones de acciÃ³n: `Nuevo` (siempre visible segÃºn rol habilitado); `Editar`; `Desactivar` (oculto/deshabilitado para Vendedor); `Reactivar` (oculto/deshabilitado para Vendedor; oculto si activo); `Actualizar`.
  - ConfirmaciÃ³n Ãºnica de baja: invocar `ObtenerConfirmacionBajaProveedorAsync`; mostrar mensaje con cantidad de compras si aplica; luego `DarBajaProveedorAsync`; nunca dos diÃ¡logos (`RF-20/21`).
  - `AplicarRolUsuario(string?)`: ocultar/deshabilitar acciones segÃºn rol (`D-10`); `tab_proveedores` visible para Administrador, Encargado (D-8), Vendedor.
  - Armonizar visualmente con `ClientesForm`/`ProductosForm` (`Dark` theme, `DataGridView` estilos, botones `MaterialButton`).
  - Hecho cuando: `dotnet build`; pantalla operativa con datos reales; grilla no vacÃ­a al entrar (buscador vacÃ­o + Activos); filtro alterna; alta/ediciÃ³n con modal; baja con confirmaciÃ³n Ãºnica (compra incluida); acciones ocultas para Vendedor; sin filas de ejemplo ni mensajes de la vista (`RF-25`).

- [x] **T7. `Scripts`: agregar proveedores de ejemplo a `scripts/preparar-bd.ps1`.** â€”
  - Insertar en el script fragmentos `INSERT INTO PROVEEDOR ...` con proveedores de prueba (activo e inactivo, con/sin correo, con/sin telÃ©fono, CUIT vÃ¡lido, RazonSocial vÃ¡lida), asegurando no duplicar `Cuit` ni `Correo` informados.
  - Hecho cuando: ejecuciÃ³n del script crea proveedores; `dotnet build`; datos disponibles en SQL Server.

- [ ] **T8. IntegraciÃ³n / validaciÃ³n final.** Todos los RF / RNF
  - `dotnet build` limpio; `dotnet test` completo verde.
  - VerificaciÃ³n visual: abrir `ProveedoresForm`; confirmar grilla real, buscador vacÃ­o, filtro Activos, alta, ediciÃ³n, baja con confirmaciÃ³n (con y sin compras), reactivaciÃ³n, acciones por rol (`Vendedor` vs `Encargado`/`Administrador`), rÃ³tulo Documento, mensajes en espaÃ±ol, sin datos mockeados.
  - Confirmar que ninguna baja es fÃ­sica (`Estado` pasa a `false`, fila conservada, compras intactas).
  - Confirmar unicidad en BD real tras datos reales (`Cuit` duplicado rechazado; `Correo` duplicado rechazado; `ToUpper()` insensible).
  - Actualizar `MEMORY.md` con estado, decisiones y errores a evitar.
  - Hecho cuando: checklist de cierre de `spec.md` cumplido; resumen entregado al coordinador; ningÃºn archivo fuera de `specs/`, `CapaEntidad/`, `CapaDatos/`, `CapaNegocio/`, `CapaPresentacion/`, `scripts/`, `tests/` modificado fuera de alcance.

---

> **Nota para el coordinador**: presentar resumen del plan y pares (T1/T2, T3/T4, T5/T6, T7/T8) hasta aprobaciÃ³n explÃ­cita del usuario. No iniciar T1 sin confirmaciÃ³n de la migraciÃ³n (`AGENTS.md`).

