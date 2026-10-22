# Cambio pendiente en `MEMORY.md` — Spec 001 (reencuadre de acceso)

> El agente planificador (fase "cambio de requisito") solo puede escribir dentro de `specs/`,
> por lo que **no pudo editar `MEMORY.md`** (permiso denegado). Aplicar este texto cuando se
> pueda. **El reencuadre ya fue APROBADO por el usuario el 2026-10-19** (spec 001 en estado
> `aprobada`), por lo que el texto de abajo ya es aplicable.

## 1. Sección "Aprendizajes y Errores a Evitar"

Reemplazar el bloque:

```
- **Regla del usuario**: antes de ejecutar cualquier `DELETE` físico (incluso para limpiar filas creadas por error propio), **CONSULTAR AL USUARIO primero**. Nunca borrar por cuenta propia.
- Los comandos `dotnet ef` requieren antes `dotnet tool restore` (manifiesto `.config/dotnet-tools.json`).
```

por:

```
- **Regla del usuario**: antes de ejecutar cualquier `DELETE` físico (incluso para limpiar filas creadas por error propio), **CONSULTAR AL USUARIO primero**. Nunca borrar por cuenta propia.
- **Regla del usuario**: cualquier modificación significativa de UI (menú, estructura de pantallas, agregar/quitar ítems) se **CONSULTA ANTES**. La interfaz estática ya está definida.
- Los comandos `dotnet ef` requieren antes `dotnet tool restore` (manifiesto `.config/dotnet-tools.json`).
- **`ImageList` de un `TabControl`: dos `TabPage` con la misma `ImageKey`** lanzan `ArgumentException: An item with the same key has already been added. Key: products.png` al mostrar el menú. Ocurrió en `inicio` (la pestaña `tab_categorias` duplicaba la clave de `tab_productos`); se corrigió **eliminando la pestaña** (reencuadre: categorías como característica de Productos). Regla: cada `TabPage` con ícono lleva clave de imagen única.
```

## 2. "Estado Actual" / "Próximos Pasos"

- Anotar el **reencuadre**: la gestión de categorías deja de ser un ítem del menú principal y pasa a ser una característica del módulo Productos; el ABM se abre desde el botón "Gestionar categorías" de `ProductosForm` en una ventana modal (reutiliza `GestionCategoriasControl`/`CategoriaForm`). **Aprobado por el usuario el 2026-10-19.**
- Anotar el **crash corregido**: `ArgumentException: An item with the same key has already been added. Key: products.png` (eliminado al quitar la pestaña `tab_categorias`).
- Estado de la spec: **`aprobada`** (el cambio de requisito del reencuadre fue aprobado por el usuario el 2026-10-19).
- **T11 (eliminar la pestaña `tab_categorias`) verificada como hecha en código el 2026-10-19**; **pendientes T12** (botón "Gestionar categorías" en `ProductosForm` con gating por rol, que exige llevar el rol hasta la vista), **T13** (`GestionCategoriasModalForm`) y **T14** (verificación: `dotnet build` limpio, `dotnet test` en verde y validación visual del reencuadre).
- **Roadmap global creado**: `specs/000-roadmap.md` (orden acordado: 001 categorías → 002 Productos → 003 Clientes → 004 Proveedores → 005 Ventas → 006 Compras → 007 Reportes; una funcionalidad a la vez, con aprobación del usuario en cada fase).

## 3. Fase Plan/Tareas del 2026-10-19 (cierre de documentación)

- La spec `001-abm-categorias` se mantiene en **`aprobada`** (confirmado por el usuario) y su **`plan.md` quedó actualizado**: se eliminó toda mención de "pendiente de aprobación del usuario" (encabezado y §4) y se explicitó el mecanismo real del gating por rol (método de `ProductosForm` invocado desde `inicio.AplicarPermisosPorRol()`) y el refresco del combo al cerrar el modal.
- `tasks.md` revisado: **T11 hecha en código**; **T12 y T13 pendientes** (`ProductosForm` sin el botón "Gestionar categorías"; `GestionCategoriasModalForm` no existe) y **T14** es la verificación integral (`dotnet build` limpio + `dotnet test` en verde + validación visual, incluido el arranque sin el crash `products.png`).
- `specs/000-roadmap.md` ampliado: nombres de carpeta confirmados (`005-ventas`, `006-compras`), **menú principal congelado** (ninguna spec futura lo modifica), y **nueva sección §11** sobre la vista **`Info`** documentada como **centro de ayuda ESTÁTICO** sin backend ni spec propia, con una **revisión de contenido/redacción como ítem de cierre** después de Reportes.
- **No quedan dudas abiertas del roadmap**; solo siguen las dudas funcionales propias de cada spec futura (002–007), que se resuelven antes de redactar cada una.
