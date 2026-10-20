# Cambios implementados pendientes de subir a GitHub

> **Documento de documentación.** No modifica código fuente, especificaciones ni `MEMORY.md`.
> **No se ejecutó ningún `git add`, `git commit`, `git push` ni la creación de ramas.** El usuario
> subirá todo él mismo en una rama dedicada a la issue **"gestión de categorías"**.
>
> **Fecha de generación:** 2026-10-20.
> **Rama local actual:** `main`.
> **Rama de seguimiento:** `origin/main`.
> **Remoto:** `origin` → `https://github.com/nestorgariglio/proyecto_taller_programacion_2.git`.
>
> **Nota:** este documento **reemplaza** al borrador que había quedado en
> `specs/CAMBIOS-PENDIENTES-DE-SUBIR.md` (creado por el agente planner, que no tenía shell y
> reconstruyó todo por lectura indirecta). Este documento se generó **ejecutando realmente**
> `git status`, `git diff`, `git log`, `git branch`, `git remote`, `dotnet build` y `dotnet test`.

---

## 1. Resumen ejecutivo

- Este documento reúne, ordenados por tema, **todos los cambios que hoy viven en el árbol de
  trabajo local** (harness de agentes, documentación SDD, funcionalidad "Gestión de Categorías",
  corrección del crash del drawer entre sesiones, script de preparación de BD y memoria/gobernanza),
  para que el usuario los revise y los suba en una rama dedicada a la issue de categorías.
- **No hay commits locales pendientes de subir.** Las ramas `main` y `origin/main` apuntan **al
  mismo commit** (`be8cca2`). No hay divergencia (`0` adelante / `0` atrás). Ver §2.
- **Todo el trabajo pendiente está SIN COMMITEAR** (working tree): 7 archivos modificados y 17
  rutas nuevas (untracked). Ver §2 y §3.
- **No se hizo commit ni push**, ni se crearon ramas. Nada de §5 fue ejecutado.
- **Verificación técnica REAL ejecutada en esta sesión:** `dotnet build` → **0 errores**, 26
  advertencias (todas preexistentes); `dotnet test` → **64/64 superados, 0 con error**. Ver §4.

---

## 2. Estado de git (salida real)

### 2.1 Comandos ejecutados y su resultado

| Comando | Resultado |
|---|---|
| `git status` | `On branch main` / `Your branch is up to date with 'origin/main'.` · 7 archivos modificados sin stage · 17 rutas untracked · `no changes added to commit` |
| `git status --porcelain` | 7 líneas ` M` + 17 líneas `??` (ver §2.2) |
| `git diff --stat` | 7 archivos, **415 inserciones / 18 borrados** |
| `git diff --cached --stat` | **vacío** (nada en el índice/stage) |
| `git branch -a` | `* main` + `remotes/origin/{main, 1-hu-01-…, feat/2-…, feat/3-…}` · `origin/HEAD -> origin/main` |
| `git remote -v` | `origin` → `https://github.com/nestorgariglio/proyecto_taller_programacion_2.git` (fetch y push) |
| `git log --oneline -1 main` | `be8cca2 docs: actualizar README` |
| `git log --oneline -1 origin/main` | `be8cca2 docs: actualizar README` |
| `git rev-list --left-right --count origin/main...main` | `0	0` (ni adelante ni atrás) |
| `git log --oneline origin/main..main` | **vacío** → **ningún commit local falta en `origin`** |

### 2.2 Qué está commiteado vs. pendiente

**Commiteado:** nada nuevo. El último commit en `main` (= `origin/main`) es `be8cca2`. El historial
reciente:

```
be8cca2 docs: actualizar README
f65c7ee Merge pull request #20 ... vistas mockeadas para la primera entrega
eb67440 feat: unificar diseño de vistas mockeadas
1e84f44 feat: vistas mockeadas para la primera entrega
749e76b Merge pull request #19 ... gestion de cuentas de usuario
7d3012c feat: completar gestion de usuarios (#3)
...
```

**Pendiente (sin commitear), archivos MODIFICADOS (` M`):**

| Ruta | ± líneas (`git diff --stat`) |
|---|---|
| `CapaDatos/AppDbContext.cs` | +9 |
| `CapaDatos/Migrations/AppDbContextModelSnapshot.cs` | +7 / -1 |
| `CapaPresentacion/Autenticacion/login.cs` | +38 / -8 |
| `CapaPresentacion/Inicio/inicio.cs` | +27 / -2 |
| `CapaPresentacion/Productos/ProductosForm.cs` | +142 / -7 |
| `CapaPresentacion/Program.cs` | +13 |
| `scripts/seed-desarrollo.sql` | +197 / -1 |

**Pendiente (sin commitear), archivos NUEVOS / untracked (`??`):**

```
.agents/
.opencode/
AGENTS.md
CapaDatos/Migrations/20261019103427_AddCategoryConstraints.Designer.cs
CapaDatos/Migrations/20261019103427_AddCategoryConstraints.cs
CapaNegocio.Tests/CategoriaNegocioTests.cs
CapaNegocio/CategoriaNegocio.cs
CapaNegocio/DTOs/Categorias/
CapaNegocio/Enums/ResultadoActivacionCategoria.cs
CapaNegocio/Enums/ResultadoBajaCategoria.cs
CapaNegocio/Enums/ResultadoCreacionCategoria.cs
CapaNegocio/Enums/ResultadoModificacionCategoria.cs
CapaPresentacion/Categorias/
MEMORY.md
docs/
opencode.json
scripts/README-preparar-bd.md
scripts/preparar-bd.ps1
specs/
```

### 2.3 Qué commits locales faltan en `origin`

**Ninguno.** `git log origin/main..main` devuelve vacío y `git rev-list --left-right --count`
devuelve `0  0`. Es decir, **no hay trabajo commiteado sin subir**: todo lo que falta subir está
**sin commitear** en el árbol de trabajo (§2.2). El aviso `warning: in the working copy of
'scripts/seed-desarrollo.sql', LF will be replaced by CRLF…` es de normalización de fin de línea,
no un error.

### 2.4 Ignorados por `.gitignore`

Verificado con `git check-ignore` que **ninguna** de las carpetas/archivos nuevos está ignorada:
`.agents/`, `.opencode/`, `specs/`, `docs/`, `opencode.json`, `MEMORY.md`, `AGENTS.md`, `scripts/`
→ todas responden **"NO ignorado"**, por lo que aparecerán en el commit cuando se agreguen.

---

## 3. Inventario de cambios por tema

### 3.1 Harness Engineering / entorno de agentes

| Ruta | Aporta | Estado |
|---|---|---|
| `AGENTS.md` | Guía operativa del repo para agentes: stack y estructura de 4 capas; comandos (`dotnet build`, `dotnet test`, migraciones, `scripts/preparar-bd.ps1` + `-SoloDatos`); convenciones (aislamiento de capas, DTOs sin claves, `async/await`); reglas de dominio (bajas lógicas, unicidad); forma de trabajo y límites. | **Nuevo / untracked** |
| `docs/constitution.md` | Constitución con 6 principios innegociables (4 capas; "la spec manda / prohibido alucinar"; bajas lógicas + BCrypt; tests como puerta; transacciones explícitas; idioma español). | **Nuevo / untracked** |
| `.agents/skills/sdd/SKILL.md` | Skill **SDD**: flujo (Constitución → Spec → Clarificación → Plan → Tareas → Implementación → Validación → Cambio) y plantillas de `spec.md`/`plan.md`/`tasks.md`. | **Nuevo / untracked** |
| `.agents/skills/code-quality-checklist/SKILL.md` | Skill de calidad C#: jerarquía de capas, I/O async, bajas lógicas, BCrypt solo en Negocio, `AsNoTracking()` en lectura y checklist de revisión. | **Nuevo / untracked** |
| `.opencode/agents/coordinator.md` | Agente primario que dirige el flujo SDD con planner/implementer/reviewer. | **Nuevo / untracked** |
| `.opencode/agents/planner.md` | Subagente planner: redacta spec/plan/tasks; solo escribe en `specs/**`. | **Nuevo / untracked** |
| `.opencode/agents/implementer.md` | Subagente implementer: ejecuta UNA tarea del plan con `dotnet build`/`dotnet test`. | **Nuevo / untracked** |
| `.opencode/agents/reviewer.md` | Subagente reviewer: revisa spec (QA) y valida RF por RF. | **Nuevo / untracked** |
| `.opencode/commands/sdd-spec.md`, `sdd-plan.md`, `sdd-tasks.md`, `sdd-status.md`, `sdd-clarify.md`, `sdd-implement.md` | Comandos del flujo SDD (atajos por fase). | **Nuevos / untracked** |
| `.opencode/commands/feature.md` | Comando para cambios pequeños que no ameritan spec completa. | **Nuevo / untracked** |
| `.opencode/commands/preparar-bd.md` | Comando `/preparar-bd` que ejecuta `scripts/preparar-bd.ps1` y verifica conteos; acepta `$ARGUMENTS` (p. ej. `-SoloDatos`). | **Nuevo / untracked** |
| `opencode.json` | Config de OpenCode: MCP remoto `context7` (`https://mcp.context7.com/mcp`) con `Authorization: Bearer ${CONTEXT7_API_KEY}`. **La clave se referencia por variable de entorno; no está hardcodeada.** | **Nuevo / untracked** |
| `.config/dotnet-tools.json` | Manifiesto de herramientas dotnet: `dotnet-ef` **10.0.11** (requiere `dotnet tool restore` antes de `dotnet ef`). | **Nuevo / untracked** · *(ya existía en el commit base? No: no figura en `git status`, ver nota)* |
| `.github/ISSUE_TEMPLATE/plantilla-hu.md` | Plantilla de Historia de Usuario del ERS ("dice qué, no cómo"). | **Ya trackeado** (no aparece en `git status`) |

> **Nota:** `.config/dotnet-tools.json` **no** aparece en `git status`, por lo que **ya está
> commiteado** en `be8cca2` (no es un cambio pendiente). Se lo menciona solo como contexto del
> harness. Lo mismo aplica a `.github/ISSUE_TEMPLATE/` (existía antes).

### 3.2 Spec-Driven Development (SDD)

| Ruta | Estado / contenido |
|---|---|
| `specs/000-roadmap.md` | Roadmap global (2026-10-19). Orden: `001 categorías → 002 Productos → 003 Clientes → 004 Proveedores → 005 Ventas → 006 Compras → 007 Reportes`; "una funcionalidad a la vez"; menú congelado (D-R2); vista `Info` estática (D-R3). **Nuevo / untracked.** |
| `specs/001-abm-categorias/spec.md` | Spec **Estado: `aprobada`**; 12 RF en EARS (RF-1…RF-12), RNF-1…RNF-5, casos límite, fuera de alcance, criterios de finalización; incluye el cambio de requisito (reencuadre, D-4, aprobado 2026-10-19). **Nuevo / untracked.** |
| `specs/001-abm-categorias/plan.md` | Plan técnico por 4 capas, DTOs/enums, persistencia (Fluent + migración `AddCategoryConstraints`), decisiones D-1…D-8 con alternativas descartadas, estrategia de tests y trazabilidad RF→componente. **Nuevo / untracked.** |
| `specs/001-abm-categorias/tasks.md` | 14 tareas (**T1–T14, todas `[x]`**). T6 reemplazada por T11/T12/T13. T14 = verificación técnica (build 0 errores / test 64/64) con validación visual pendiente del usuario. **Nuevo / untracked.** |
| `specs/001-abm-categorias/MEMORY-pendiente.md` | Texto que el planner dejó preparado para volcar en `MEMORY.md` (no pudo editarlo por permisos). **Nuevo / untracked.** Ver §3.6. |
| `specs/CAMBIOS-PENDIENTES-DE-SUBIR.md` | **Borrador del planner (sin shell). Eliminado al crear este documento en `docs/`** para no dejar duplicados. |

> **OBSERVACIÓN (consistencia documental; requiere decisión del usuario, no se tocó nada):**
> `specs/000-roadmap.md` (§3 y §4) todavía describe la spec 001 como "T11 hecha en código;
> **T12–T14 pendientes**", mientras que `tasks.md` y `MEMORY.md` ya dan **T11–T14 como hechas y
> verificadas**. Es una desincronización de texto entre documentos (el roadmap quedó desactualizado
> respecto del cierre de tareas). Si el usuario quiere, conviene alinearlo antes de subir; este
> documento no lo modificó (regla de la tarea).

### 3.3 Funcionalidad "Gestión de Categorías" (reencuadre)

**Backend / `CapaNegocio`** (nuevos, untracked):

- `CapaNegocio/CategoriaNegocio.cs` — lógica del ABM: alta (recorte, formato 3–100, `Estado = true`),
  modificación, listados (todas y solo activas), conteo de productos activos, baja lógica y
  reactivación; operaciones `async`, mapeo explícito a DTO, mensajes en español.
- `CapaNegocio/DTOs/Categorias/` — `CrearCategoriaDto.cs`, `ModificarCategoriaDto.cs`,
  `CategoriaRespuestaDto.cs`, `RespuestaCreacionCategoria.cs`, `RespuestaModificacionCategoria.cs`,
  `RespuestaBajaCategoria.cs`, `RespuestaActivacionCategoria.cs`.
- `CapaNegocio/Enums/` (categorías) — `ResultadoCreacionCategoria.cs`,
  `ResultadoModificacionCategoria.cs`, `ResultadoBajaCategoria.cs`, `ResultadoActivacionCategoria.cs`.
- `CapaNegocio/PoliticaAcceso.cs` — **ya trackeado** (no aparece en `git status`); módulo
  `"Categorías"` (Admin/Encargado sí, Vendedor no); sostiene RF-12.

**Datos / `AppDbContext` + migración:**

- `CapaDatos/AppDbContext.cs` (**modificado**, +9) — Fluent API de `Categoria`:
  `Property(Descripcion).HasMaxLength(100)` e `HasIndex(Descripcion).IsUnique()
  .HasFilter("[descripcion] IS NOT NULL")` (mismo patrón que `Usuario.Correo`). **Verificado en el
  `git diff` real.**
- `CapaDatos/Migrations/20261019103427_AddCategoryConstraints.cs` (+ `.Designer.cs`) — **nuevos**.
- `CapaDatos/Migrations/AppDbContextModelSnapshot.cs` (**modificado**, +7/-1) — snapshot actualizado
  por la migración.

**Presentación / `CapaPresentacion`:**

| Ruta | Rol | Estado |
|---|---|---|
| `CapaPresentacion/Categorias/GestionCategoriasControl.cs` (+ `.Designer.cs`) | UserControl con la grilla del ABM (filtros, botones Nuevo/Editar/Dar de baja/Reactivar/Actualizar). Se reutiliza dentro del modal. | **Nuevo / untracked** |
| `CapaPresentacion/Categorias/CategoriaForm.cs` (+ `.Designer.cs`) | Modal de alta/edición de categoría (patrón `UsuarioForm`). | **Nuevo / untracked** |
| `CapaPresentacion/Categorias/GestionCategoriasModalForm.cs` (+ `.Designer.cs`) | Contenedor modal que aloja `GestionCategoriasControl` a `Dock = Fill` y carga en `Shown` (T13). Acepta `string? rol = null` (defensa en profundidad). | **Nuevo / untracked** |
| `CapaPresentacion/Productos/ProductosForm.cs` (**modificado**, +142/-7) | Botón "🏷️ Gestionar categorías" en `panelAcciones` (nace `Visible = false`), `AplicarRolUsuario(string?)`, `CargarCategoriasAsync()`, guard defensivo por rol y refresco del combo al volver del modal (T12). | **Modificado** |
| `CapaPresentacion/Inicio/inicio.cs` (**modificado**, +27/-2) | Pestaña `tab_categorias` eliminada (T11); recibe `ProductosForm` por DI; `AplicarPermisosPorRol()` invoca `_productosForm.AplicarRolUsuario(rol)`; nuevo `tab_productos_Enter` que recarga el combo (D-7). | **Modificado** |
| `CapaPresentacion/Program.cs` (**modificado**, +13) | DI: `CategoriaNegocio`, `GestionCategoriasControl`, `ProductosForm`, `login`, `inicio` y factory `Func<inicio>`. | **Modificado** |

**Reencuadre (qué cambió de enfoque):**

- La gestión de categorías **dejó de ser un ítem del menú principal** y pasó a ser una
  **característica del módulo Productos**, abierta con un botón en `ProductosForm` en **ventana
  modal** (decisión D-4, aprobada 2026-10-19).
- **Pestaña `tab_categorias` eliminada** de `inicio` (T11): cableo, `navegacionBase`, `ImageKey`
  y handler `Enter`. El menú principal queda igual (no se agrega ni quita otro ítem).
- **Crash `products.png` corregido por esa eliminación:** la pestaña de categorías duplicaba la
  `ImageKey` de `tab_productos` en la misma `ImageList`, lo que producía
  `ArgumentException: An item with the same key has already been added. Key: products.png`.
- **Gating por rol (RF-12):** botón oculto para Vendedor, visible para Admin/Encargado, más un
  **guard defensivo** en runtime (mensaje "Acceso no autorizado…") y defensa en profundidad en el
  propio modal.

### 3.4 Corrección del bug entre sesiones (crash `sales.png` / `users.png`)

**Síntoma:** `KeyNotFoundException: 'sales.png'` en `MaterialDrawer.Paint` al cambiar de sesión
(rol Encargado → Vendedor, o arranque limpio de Vendedor).

**Causa raíz (documentada en `MEMORY.md` y confirmada por la lógica del cambio):** en MaterialSkin
2.3.1, `MaterialForm.DrawerTabControl` es un **auto-property que no reenvía a
`drawerControl.BaseTabControl`**; el binding real ocurre **una sola vez** dentro de
`AddDrawerOverlayForm()`, invocado solo desde el evento `Shown`. `preProcessIcons()` llena los
diccionarios de íconos con las claves `ImageKey` de las `TabPage` **del momento**; al cambiar de rol
se quitan/reinsertan `TabPage` y el snapshot del drawer queda desincronizado → `Paint` indexa una
clave no vista y lanza la excepción.

**Solución implementada (recrear `inicio` por sesión):**

- `CapaPresentacion/Program.cs` (**verificado en el `git diff`**) — registra la factory
  `services.AddTransient<Func<inicio>>(sp => () => sp.GetRequiredService<inicio>())`, con comentario
  que explica el hotfix.
- `CapaPresentacion/Autenticacion/login.cs` (**verificado en el `git diff`**) — recibe
  `Func<inicio> _inicioFactory` en vez de un `inicio` fijo; en cada ingreso exitoso crea un `inicio`
  nuevo, suscribe `CierreSesionSolicitado` y llama `EstablecerSesionUsuario`. Al "Cerrar sesión", el
  `inicio` viejo se desuscribe, se oculta y se `Dispose()`a, y se reutiliza el mismo `login`.

### 3.5 Script de preparación de BD

| Ruta | Aporta | Estado |
|---|---|---|
| `scripts/preparar-bd.ps1` | Crea/actualiza el esquema con migraciones EF Core y luego siembra datos; idempotente, **sin `DELETE`/`DROP`**; verifica conteos (`Categorías: 6 (activas: 5) / Productos: 12 (activos: 12)`); soporta `-SoloDatos`. | **Nuevo / untracked** |
| `scripts/README-preparar-bd.md` | Documentación del script y resolución de problemas. | **Nuevo / untracked** |
| `scripts/seed-desarrollo.sql` | Semilla de desarrollo (roles, usuarios, categorías y productos con códigos `779…`); **guardado con BOM UTF-8** (verificado: primeros bytes `EF BB BF`) por el problema de `sqlcmd`/codepage OEM. | **Modificado** (+197/-1) |
| `.opencode/commands/preparar-bd.md` | Comando `/preparar-bd` que ejecuta el script y verifica la salida. | **Nuevo / untracked** |

Documentado además en `AGENTS.md` §Comandos y en `MEMORY.md` (sección "Script de preparación de BD").

### 3.6 Memoria y gobernanza

| Ruta | Estado | Nota |
|---|---|---|
| `MEMORY.md` | **Nuevo / untracked** (la memoria aún no estaba versionada). Actualizado con: estado actual (frontend, autenticación, spec 001 T1–T10 + reencuadre T11–T14, repositorio/PR #18), decisiones (DTOs sanitizados, soft deletes), aprendizajes (crash `ImageList`/`products.png`, `DrawerTabControl`, semántica de `Dispose()` de WinForms, reglas del usuario) y próximos pasos. | |
| `AGENTS.md` (§Comandos) | **Nuevo / untracked.** Incluye el comando de preparación de BD (`scripts/preparar-bd.ps1` + `-SoloDatos` + referencia al README). | |
| `specs/001-abm-categorias/MEMORY-pendiente.md` | **Nuevo / untracked.** Texto que el planner dejó preparado (no pudo editar `MEMORY.md` por permisos). | **OBSERVACIÓN:** al leer `MEMORY.md`, su contenido (regla de UI a consultar antes; lección del `ImageList` con clave duplicada; notas de estado/próximos pasos) **ya está volcado** en `MEMORY.md`. Es probable que este archivo quede obsoleto → **decisión del usuario** (archivar o no incluirlo en el commit). |

---

## 4. Verificación técnica REAL (re-ejecutada en esta sesión)

Ejecutada en el árbol de trabajo actual (con todos los cambios aplicados, sin commitear):

| Verificación | Resultado real |
|---|---|
| `dotnet build` | **0 Errores**, **26 Advertencia(s)**, tiempo ~14 s |
| Advertencias | Todas **preexistentes**: `NU1902`/`NU1903` (`System.Security.Cryptography.Pkcs` 9.0.11 con vulnerabilidad conocida), `NU1701` (`LiveCharts*` restaurado para .NETFramework en vez de `net10.0-windows7.0`), `CS8981` (tipos `login`/`inicio` en minúsculas), `CS8622`/`CS8600` (nulabilidad) y `CS0168` (`ex` sin usar en `inicio.cs:421`). **Ninguna advertencia nueva** por el reencuadre. |
| `dotnet test` | `Correctas! - Con error: 0, Superado: 64, Omitido: 0, Total: 64, Duración: 8 s` — suite `CapaNegocio.Tests` (`net10.0`). **Sin regresiones.** |

Salida cruda relevante de `dotnet test`:

```
Serie de pruebas para ...\CapaNegocio.Tests\bin\Debug\net10.0\CapaNegocio.Tests.dll (.NETCoreApp,Version=v10.0)
1 archivos de prueba en total coincidieron con el patrón especificado.
Correctas! - Con error:     0, Superado:    64, Omitido:     0, Total:    64, Duración: 8 s - CapaNegocio.Tests.dll (net10.0)
```

> **Validación visual (UI WinForms) — PENDIENTE del usuario.** No es automatizable: arranque sin
> crash `products.png`; flujo ABM completo desde el botón "🏷️ Gestionar categorías"; botón oculto
> a Vendedor y visible a Admin/Encargado; combo de categorías refrescado al cerrar el modal y al
> entrar en la pestaña; menú principal sin ítem de categorías; cambio de sesión sin crash
> `sales.png`/`users.png`.

---

## 5. Guía para el usuario (NO ejecutada)

> **Nada de esta sección fue ejecutado.** Son pasos sugeridos para que el usuario los corra por su
> cuenta en una rama dedicada a la issue "gestión de categorías". El nombre de rama es una
> sugerencia; ajustalo a tu convención.

1. **Confirmar el estado real** (opcional, ya verificado en §2):
   ```
   git status
   git diff --stat
   git log --oneline origin/main..main
   ```
2. **Crear la rama dedicada** (a partir de `main`):
   ```
   git checkout -b feature/gestion-categorias
   ```
3. **Revisar e incluir los archivos** de §3. Recordá que **nada está en stage** hoy:
   ```
   git add AGENTS.md MEMORY.md docs/ specs/ .agents/ .opencode/ opencode.json
   git add CapaNegocio/ CapaDatos/ CapaPresentacion/ scripts/
   git status
   ```
   > **OBSERVACIÓN:** verificá que **no se cuelen** archivos generados (`bin/`, `obj/`, `.vs/`,
   > `*.user`) —hoy están en `.gitignore`, pero conviene confirmarlo con `git status`— y decidí qué
   > hacer con `specs/001-abm-categorias/MEMORY-pendiente.md` (§3.6) y con el borrador ya eliminado
   > `specs/CAMBIOS-PENDIENTES-DE-SUBIR.md` (hoy ya no existe en disco).
4. **Mensaje de commit sugerido (en español):**
   ```
   feat(categorias): gestión de categorías como característica de Productos

   - ABM de categorías en CapaNegocio (DTOs/enums, validaciones, baja lógica, reactivación)
   - Unicidad/longitud en AppDbContext + migración AddCategoryConstraints
   - Presentación: GestionCategoriasControl, CategoriaForm y GestionCategoriasModalForm
   - Reencuadre: ABM abierto desde ProductosForm (botón con gating por rol); pestaña
     tab_categorias eliminada (corrige el crash 'products.png')
   - Hotfix: recrear `inicio` por sesión vía Func<inicio> (crash 'sales.png'/'users.png')
   - Script de preparación de BD y comandos/documentación del harness de agentes
   - Docs SDD: roadmap y spec 001 (spec/plan/tasks); MEMORY.md actualizada

   dotnet build: 0 errores | dotnet test: 64/64
   ```
5. **Subir la rama y abrir el PR** (cuando lo decidas):
   ```
   git push -u origin feature/gestion-categorias
   ```

---

## 6. Pendientes / próximos pasos

- **Validación visual del usuario (bloqueante para cerrar los RF de UI):** arranque sin crash
  `products.png`; flujo ABM completo desde el botón "🏷️ Gestionar categorías"; botón oculto a
  Vendedor y visible a Admin/Encargado; combo de categorías activas refrescado al cerrar el modal y
  al entrar en la pestaña; menú principal sin ítem de categorías; y cambio de sesión sin crash
  `sales.png`/`users.png`.
- **Especificaciones 002–007 (según `specs/000-roadmap.md`), aún "A crear":** `002-abm-productos`,
  `003-abm-clientes`, `004-abm-proveedores`, `005-ventas`, `006-compras`, `007-reportes`. Hoy solo
  existe la carpeta `specs/001-abm-categorias/` (verificado). Cada una tiene sus **dudas abiertas**
  listadas en el roadmap (se resuelven antes de redactarlas).
- **Ítem de cierre del roadmap:** revisión editorial de la vista `Info` (centro de ayuda estático)
  **después** de la spec 007.
- **Dudas/observaciones que requieren decisión del usuario:**
  1. Confirmar el alcance real de lo NO commiteado con `git status`/`git diff` (§2).
  2. Resolver la desincronización de `specs/000-roadmap.md` §3/§4 (dice "T12–T14 pendientes") vs.
     `tasks.md`/`MEMORY.md` (T11–T14 hechas) — §3.2.
  3. Decidir qué hacer con `specs/001-abm-categorias/MEMORY-pendiente.md` (probablemente obsoleto,
     §3.6).
  4. Decidir si se versionan las carpetas del harness (`.agents/`, `.opencode/`, `docs/`, `specs/`,
     `MEMORY.md`) en el mismo commit/PR o en uno aparte — **no están ignoradas**, así que se
     incluirán si se hace `git add` de la raíz.
