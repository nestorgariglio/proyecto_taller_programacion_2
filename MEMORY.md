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
