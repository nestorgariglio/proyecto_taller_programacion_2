# MEMORY.md - Estado del Proyecto
Memoria entre sesiones (máximo ~50 líneas). Resume o elimina lo que ya no aporte.

## Estado Actual
- **Specs 001 (`abm-categorias`), 002 (`abm-productos`), 003 (`productosform-bugfix`) y 004 (`consulta-stock`)**: implementadas y cerradas; migraciones aplicadas (`AddProductConstraints`, `AddClienteConstraints`), build limpio. Categorías se gestionan desde `ProductosForm` en `GestionCategoriasModalForm`.
- **Frontend (`CapaPresentacion`)**: 8 vistas maquetadas con MaterialSkin 2 y LiveCharts (`inicio`, `ProductosForm`, `ClientesForm`, `VentasForm`, `ProveedoresForm`, `ComprasForm`, `ReportesForm`, `InfoForm`).
- **Autenticación**: `UsuarioNegocio` funcional con BCrypt y script `seed-desarrollo.sql`. BD `DB_SISTEMA_VENTA` reconstruida con `dotnet ef database update` y sembrada (roles, usuarios, **Categorías: 7 (activas: 5)**, 12 productos activos con códigos `779...`).
- **Spec 001** (12 RF): T1–T10 completadas — SPEC IMPLEMENTADA. Ronda O-1/O-2 (T15–T17): baja en `CapaNegocio` + unicidad en BD. `dotnet build` 0 errores; `dotnet test` **237/237**; validación del revisor: **RONDA APTA**; queda validación visual del diálogo de baja.
- **Spec 003** (ABM Clientes): T1–T10 completadas — SPEC IMPLEMENTADA. `ClienteNegocio` con 8 métodos públicos, validaciones, unicidad, bordes de datos, DTOs, `PoliticaAcceso` con `"Clientes"` para Encargado. `dotnet build` 0 errores; `dotnet test` **267/267**; armonización visual C1–C3 aplicada en `ClientesForm.cs` (paleta grilla `ProductosForm`, botones `Contained` con `HighEmphasis`, panel acciones `FondoPrincipal`).
- **Spec 004** (HU-06 Consulta Stock): T1–T9 + correcciones C1/C2/C4/C5. `dotnet build` 0 errores; `dotnet test` **230/230**. T10: validación visual pendiente.
- **BD de desarrollo**: `DB_SISTEMA_VENTA` con esquema por migraciones y semilla; `scripts/preparar-bd.ps1` idempotente. Baseline: `Categorías: 7 (activas: 5) | Productos: 12 (activos: 12)`; 7.ª = `Tecnologia` (inactiva).
- **Merge HU-06 + HU-07**: conflictos resueltos conservando ambos módulos; `dotnet build` 0 errores y `dotnet test` **446/446** tras la integración.

## Decisiones clave (spec 004)
- **D-2**: umbral global por configuración, sin stock mínimo por producto ⇒ sin migración.
- **D-10**: si falta la clave se usa fallback interno **5** sin aviso; modo degradado solo si valor existe e inválido.
- **D-13**: "Solo críticos" arranca **apagado** y persiste al cambiar de pestaña.
- **D-16**: mensaje de vacío dibujado en `Paint` de la grilla, sin tercer control (RNF-5: **2** controles nuevos).
- **D-17**: agotado `#B71C1C` (inactivo `#6E2020`), bajo umbral `#F9A825` (inactivo `#8C6D0A`), texto blanco.
- **D-18**: selección en fila resaltada = **borde 2 px**, texto blanco.

## Decisiones clave (spec 001, ronda 2026-10-26)
- **O-1**: regla y texto de baja de categoría en el negocio (`ObtenerConfirmacionBajaAsync`); la vista solo presenta el mensaje.
- **O-2**: unicidad con `UPPER(descripcion)` en BD, una sola fila, sin materializar tabla (no *sargable*).

## RNF-1 (≤ 500 ms con 1.000 productos)
- Medido en `DB_SISTEMA_VENTA_PERF`: listado completo mediana **15,2 ms** / máx **36,3 ms**; "Solo críticos" 12,2 ms; `ContarProductosAsync` 0,7 ms. Cota inferior (sin pintado). Variable de entorno `ConnectionStrings__CadenaSQL` para apuntar a esa base.

## Aprendizajes y Errores a Evitar
- **MaterialSkin 2.3.1**: tipo real **`MaterialCheckbox`** (b minúscula).
- `tasks.md` de la 004 usa filtro `~ProductoNegocioConsultaStockTests` (0 coincidencias; clase es `partial class ProductoNegocioTests`).
- **`sqlcmd`**: cargar datos con `-I` (índices filtrados) y `-f 65001` (UTF-8); scripts con BOM.
- **DI**: `ProductosForm` se resuelve por DI; `IConfiguration` ya registrada.
- MaterialSkin pinta con `BackColor` del padre: **nunca `Color.Transparent`** en paneles internos.
- `AppDbContext.HasTrigger("TR_PRODUCTO_FECHA_MODIFICACION")` no revertirse; antes de `DELETE` físico, consultar al usuario.
- No materializar tablas para validar unicidad; filtrar siempre en BD.
- Al sustituir decisión, actualizar **comentarios XML** que la citan.

## Próximos Pasos
- **T10 (004)**: validación visual usuario — colores D-17, escala de grises, fila seleccionada (borde D-18), mensajes de vacío, modo degradado, categoría que desaparece y "Actualizar".
- Validación visual del diálogo de baja de categorías (ronda 001): con/sin productos activos, ya inactiva, inexistente, cancelar/confirmar.
- **Commit de la spec 004** cuando el usuario lo autorice (hoy sin commitear).
- Decisión sobre categoría inactiva `Tecnologia` (dejarla y actualizar baseline, o borrarla con autorización explícita).