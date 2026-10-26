# MEMORY.md - Estado del Proyecto
Memoria entre sesiones (máximo ~50 líneas). Resume o elimina lo que ya no aporte.

## Estado Actual
- **Frontend (`CapaPresentacion`)**: 8 vistas maquetadas con MaterialSkin 2 y LiveCharts (`inicio`, `ProductosForm`, `ClientesForm`, `VentasForm`, `ProveedoresForm`, `ComprasForm`, `ReportesForm`, `InfoForm`).
- **Autenticación y BD**: `UsuarioNegocio` funcional con BCrypt y `scripts/seed-desarrollo.sql`; BD `DB_SISTEMA_VENTA` reconstruida y sembrada (roles, usuarios, 5 categorías activas + Kiosco inactiva, 12 productos activos con códigos `779...`).
- **Spec `001-abm-categorias` — implementada (T1–T10 cerradas)**: Fluent API + migración aplicada; alta/edición con `CategoriaForm`; unicidad case-insensitive (D-1/D-6); listados de activas y de todas; baja y reactivación lógicas. `CapaNegocio` completo; cierre 2026-10-19 con build limpio y tests en verde.
- **Reencuadre D-4 (aprobado 2026-10-19) — implementado**: categorías dejan de ser ítem de menú y se gestionan desde `ProductosForm` en `GestionCategoriasModalForm` (botón "Gestionar categorías" nacido oculto, gating Admin/Encargado con guard en runtime y refresco del combo al volver). Pestaña `tab_categorias` eliminada.
- **Spec `002-abm-productos` — implementada (T1–T13)**: Fluent API de `Producto` + migración `AddProductConstraints` **aplicada a la BD** (relleno de `id_usuario` + trigger `TR_PRODUCTO_FECHA_MODIFICACION`); `ProductoNegocio` (alta, unicidad de código, modificación con código inmutable, baja/reactivación, ajuste de stock, listados con filtros) y DTOs; UI con grilla y filtros reales, `ProductoModalForm`, `AjustarStockModalForm`, "Actualizar" conectado y gating por rol. T13 validada contra la BD real.
- **Spec `003-productosform-bugfix` — implementada y aprobada (2026-10-25)**: causa raíz = `BackColor = Color.Transparent` en los paneles internos de `ProductosForm` (MaterialSkin 2.3.1 pinta con el `BackColor` del padre; `Transparent` = `ARGB(0,255,255,255)` ⇒ bloques blancos). Corrección = fondos opacos `TemaAplicacion.FondoPrincipal`, `AutoScroll = false`, eliminación de todo `Refresh()`/`Invalidate()` (queda 1 `PerformLayout()` en `Load`) y unificación a **un solo `_btnEstado`** alternante "Desactivar"/"Reactivar". T-08..T-13 cerradas: build 0 errores, tests 193/193 y validación visual confirmada por el usuario. Quedan 2 mejoras opcionales no aplicadas: mensaje neutro en el guard de "sin selección" y comentario XML de `AplicarRolUsuario`.
- **Repositorio**: PR #18 documentado y sincronizado en la rama `main`.
- **Script de preparación de BD**: `scripts/preparar-bd.ps1` (+ `README-preparar-bd.md`) crea/actualiza el esquema con migraciones y siembra con `sqlcmd -f 65001`; idempotente, sin DELETE/DROP, verifica `Categorías: 6 (activas: 5) | Productos: 12 (activos: 12)`. Documentado en `AGENTS.md` y como comando `/preparar-bd`.

## Decisiones (y por qué)
- **DTOs Sanitizados**: DTOs específicos para que los hashes de contraseñas y los campos sensibles no lleguen a la UI.
- **Soft Deletes**: columna `Estado` (bool) en lugar de borrados físicos, para preservar la auditoría.

## Aprendizajes y Errores a Evitar
- Omitir `dotnet ef database update` antes de probar la UI genera excepciones de mapeo en EF Core. Los comandos `dotnet ef` requieren `dotnet tool restore` (manifiesto `.config/dotnet-tools.json`).
- **MaterialSkin 2.3.1 pinta con el `BackColor` del padre**: `Color.Transparent` en paneles internos produce bloques blancos. Regla: paneles internos siempre opacos (`TemaAplicacion.FondoPrincipal`).
- No "arreglar" el renderizado con cascadas de `Refresh()`/`PerformLayout()`/`Invalidate()`: son no-op sin handle y violan RNF-2. Diagnosticar con una sonda WinForms real (los tests no detectan esto).
- **`AppDbContext.HasTrigger("TR_PRODUCTO_FECHA_MODIFICACION")` no debe revertirse**: sin él reaparece el error SQL "target table has database triggers".
- **`sqlcmd` lee los `.sql` con la codepage OEM**: con tildes/ñ hay que guardarlos con BOM UTF-8 o pasar `-f 65001 -u`. Los `.sql` del repo van con BOM.
- Las tablas con **índice único filtrado** (p. ej. `IX_CATEGORIA_descripcion`) fallan con error 1934 sin `SET QUOTED_IDENTIFIER ON;` (lo hace el script semilla).
- **Regla del usuario**: antes de cualquier `DELETE` físico (incluso de filas creadas por error propio) **CONSULTAR AL USUARIO**. Nunca borrar por cuenta propia.
- **Regla del usuario**: cualquier modificación significativa de UI (menú, estructura de pantallas, agregar/quitar ítems) se **CONSULTA ANTES**. La interfaz estática ya está definida.
- **`ImageList` de un `TabControl`**: dos `TabPage` con la misma `ImageKey` lanzan `ArgumentException` al mostrar el menú. Cada `TabPage` con ícono lleva clave única.
- **`MaterialForm.DrawerTabControl` (MaterialSkin 2.3.1)** no reenvía a `drawerControl.BaseTabControl`: el binding ocurre una sola vez en `AddDrawerOverlayForm()` (evento `Shown`). Quitar/reinsertar `TabPage` desincroniza el snapshot y produce `KeyNotFoundException`. Se resolvió **recreando `inicio` una vez por sesión** (`login` recibe `Func<inicio>`).
- **`Form.Dispose()`** libera en cascada las formas propias (`OwnedForms`) y no dispara `FormClosed`; por eso `_inicioForm.Dispose()` también libera los `Form` del drawer sin ejecutar `Application.Exit()`.
- Asegurar que los alias de LiveCharts (`using CartesianChart = LiveCharts.WinForms.CartesianChart;`) no choquen con tipos de WPF.

## Próximos Pasos
- **Sin pendientes funcionales** en las specs 001, 002 y 003: migraciones aplicadas, build 0 errores, tests 193/193 y validación visual confirmada (003 el 2026-10-25).
- Único paso abierto: **commit/push de los cambios sin commitear** de la spec 003, si el usuario lo autoriza.
- `ProductosForm` mantiene el **gating** por rol con `PoliticaAcceso.PuedeGestionarProductos` (botones ocultos + guard en cada handler); `inicio` no se modificó en T9 de la 002.
