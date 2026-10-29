# MEMORY.md - Estado del Proyecto
Memoria entre sesiones (máximo ~50 líneas). Resume o elimina lo que ya no aporte.

## Estado Actual
- **Specs 001 (`abm-categorias`), 002 (`abm-productos`), 003 (`productosform-bugfix`) y 004 (`abm-proveedores`)**: implementadas y validadas; migraciones aplicadas, build limpio, tests sin regresiones. Categorías se gestionan desde `ProductosForm`. Frontend 8 vistas maquetadas con MaterialSkin 2 y LiveCharts. Autenticación BCrypt + BD sembrada (roles, usuarios, Categorías 7/5, 12 productos activos).
- **Spec 004 HU-08 (ABM Proveedores)**: T1–T8 completadas — SPEC APROBADA 2026-10-28. Migración EF Core (índices únicos Cuit/Correo `ToUpper()`+`HasFilter`); entidad non-nullable; DTOs/enums/PoliticaAcceso; negocio RF-1…RF-27 tests 53; UI ProveedoresForm real; seed proveedores; validación final.
- **BD de desarrollo**: `DB_SISTEMA_VENTA` por migraciones + semilla; `scripts/preparar-bd.ps1` idempotente. Baseline: Categorías 7 (activas 5) | Productos 12 (activos 12).

## Decisiones clave (spec 004)
- **D-1**: CUIT `^[0-9]+$`; rótulo columna `Documento`.
- **D-2**: respetar tipos; sin límites arbitrarios; índices técnicos `HasMaxLength()`.
- **D-8**: Encargado habilita Proveedores en PoliticaAcceso.
- **D-10**: Vendedor ve baja/reactiva inhibidas; Encargado/Admin sí.
- **D-15/D-16**: buscador vacío ⇒ todo filtro; búsqueda rápida vacío ⇒ `SinResultados` sin consultar.

## Aprendizajes y Errores a Evitar
- `sqlcmd`: `-I -f 65001`; scripts con BOM.
- No materializar tablas; filtrar siempre en BD.
- Campos crudos no `Color.Transparent`; bcrypt no exponer hashes.
- Al sustituir decisión, actualizar XML citando decisión.

## Próximos Pasos
- Visualización del diálogo de baja de categorías (con/sin productos, inactiva, inexistente).
- Commit de la spec 004 cuando el usuario lo autorice.

---

## Spec 005 — HU-09 (Carga de Compras a Proveedores) — cierre T9a / T9b / T10 (2026-10-29)
- **Estado**: especificación aprobada (`spec.md` 2026-10-29, ajuste signo `-`); implementada T1–T9a (enums/DTOs, cálculo puro `CalculoImportesCompra`, `AnalizadorImporte`/`AnalizadorCantidad`, `CompraNegocio`, búsquedas `AsNoTracking` + proyección, registro transaccional con `BeginTransactionAsync`, atomicidad con `RollbackAsync` + `Detached` + `DisposeAsync`, historial y detalle defensivos, permisos RBAC, `ComprasForm` real con DI, `CellFormatting`). Sin migraciones (RNF-10); `git status` limpio en `CapaDatos/Migrations/`.
- **Decisiones clave (005)**: signo `-` inicial admitido único (`AnalizadorImporte`, RF-25); motivo cantidad independiente (`ResultadoOperacionLineaCompra` separado de `ResultadoRegistroCompra`); proyección defensiva (`AsNoTracking` + `Select` + textos fijos `«(Sin proveedor)»`/`«(Sin usuario)»`); `Subtotal` del DTO ignorado en registro (`CalculoImportesCompra.CalcularSubtotal` recalcula, RNF-13); `10.330m` aceptado (`10,33` = `10.33` decimal, no `double`); hook HU-10 (`AplicarEfectosSecundariosAsync`) dentro del ámbito transaccional, antes de `CommitAsync`; `PrecioVenta` solo en `LineaCompraDto` (no persistido, D-2); sin tocar `PRODUCTO` (RF-47, RF-48); `AppDbContext` solo en `CompraNegocio`; UI sin fórmulas ni mensajes propios.
- **Errores a evitar**: confundir texto de entrada con valor numérico (separa `string?` del DTO de `decimal` del negocio; `AnalizadorImporte` parsea, `CalculoImportesCompra` calcula); no usar `double`; no tocar `PRODUCTO`; no borrar físicamente (`Estado = false`); no agregar índice `producto+compra`; no confiar en la escala decimal ya validada al agregar la línea (revalidar `PrecioCompra`/`PrecioVenta` con `CalculoImportesCompra.DecimalesPrecio` y rango `PrecioMaximo` en `RegistrarCompraAsync`, paso 11); no escribir puntos de miles en vectores de prueba (`1.000` = rechazo por decimales; `1000000000` = rechazo por máximo).
- **Pendientes T9b / T10 (manual)**: verificación visual de `ComprasForm` (`T9b-checklist.md`: rótulos, prellenado, acumulación, descarte tras registro `RF-32`, formato `CellFormatting`, permisos); procedimiento reproducible de atomicidad contra SQL Server (`T10-atomicidad.md`: pasos 1–8 con `TR_FALLO_SIMULADO_DETALLE`, comprobación 0/0, restauración; nota `InMemory` no ejecuta rollback real); `git status` final tras verificación con datos reales; `MEMORY.md` actualizado sin datos sensibles.
- **Próximos pasos (HU-09 → HU-10 / Reportes)**: completar `T9b` con datos reales (evidencia por punto en `T9b-checklist.md`); completar `T10` con SQL Server real (`TR_FALLO_SIMULADO_DETALLE` pasos 4–8); luego HU-10 sobre el hook transaccional (`AplicarEfectosSecundariosAsync` — recepción de stock, sin modificar `PRODUCTO`); módulo de Reportes consumiendo `ObtenerHistorialAsync` y `ObtenerCompraConDetalleAsync`.
