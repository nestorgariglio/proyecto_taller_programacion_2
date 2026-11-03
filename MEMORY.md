# MEMORY.md — SISTEMA COMERCIAL G70

Memoria global del proyecto. Máximo aproximado: 50 líneas. Uso y actualización a cargo del Coordinator.

## Proyecto
- Stack: .NET 10, C#, WinForms, MaterialSkin 2, EF Core y SQL Server.
- Arquitectura: cuatro capas. Autoridad: `docs/constitution.md`.
- Specs 001–004: registradas previamente como implementadas y validadas; consultar sus documentos antes de modificar funcionalidades existentes.
- BD de desarrollo informada: `DB_SISTEMA_VENTA`, instancia `localhost\SQLEXPRESS`.

## Prioridad vigente: Spec 005 — HU-09
- HU-09 sigue siendo la prioridad. HU-10 y Spec 006 están estacionadas.
- C-1: manejo de errores de lecturas de BD implementado. Última validación: build sin errores y pruebas focalizadas de Compras 275/275 aprobadas.
- Filtrado bidireccional producto ↔ proveedor en Compras: implementado, compilado y testeado. Probado manualmente por el usuario en términos generales, queda aceptado; las mejoras de usabilidad quedan para más adelante. El historial de compras solo prioriza (nunca restringe); «Mostrar todos» (checkbox en la fila del título de cada tarjeta) lista todos los activos, sin tope ni paginación. Sin autoselección. Los requisitos RF-12, RF-66 y D-22 se reemplazan por RF-67 y D-23 (ver `specs/005-carga-compras/`). Sin verificar: registrar por primera vez una combinación nueva y que pase a ser prioritaria; revisión de Reviewer no realizada; Designer de Visual Studio.
- DECISIÓN CERRADA: la interfaz visual actual de `ComprasForm` (catálogo a la izquierda, documento de compra a la derecha, diseño aprobado por el usuario) no se rediseña. Ningún agente debe proponer rediseños; solo se permiten ajustes puntuales necesarios para integrar nuevos controles o comportamientos, conservando distribución y criterios visuales.
- BD de desarrollo: `/preparar-bd` reparado (modelo EF sin cambios pendientes, migraciones aplicadas, seed ejecutada e idempotente; verificación por claves determinísticas de la seed). La seed restablece solo sus propios registros; ver `scripts/README-preparar-bd.md`.
- Documentación/checklist de cierre: comprobar los archivos reales antes de afirmar que quedaron actualizados. `spec.md`, `plan.md`, `tasks.md` y los dos checklists se actualizaron para el filtrado bidireccional (RF-9, RF-10, RF-12, RF-15, RF-16, RF-66, RNF-3, D-22; nuevos RF-67 y D-23).
- B-1 (compuerta de proveedor) y la redacción de RF-34/RNF-9 no deben darse por resueltas ni modificarse en esta refactorización. RNF-9 y las pruebas de carga/rendimiento fueron descartadas por decisión del usuario.
- T10 de atomicidad: ejecución manual reservada al usuario. Ningún agente debe ejecutarla ni modificar la BD durante esa prueba.
- Pendiente de usabilidad en Compras (sin urgencia): mejoras sobre la selección y el filtrado, a definir por el usuario tras sus pruebas.

## Issue stock/precios al confirmar compra (HU-10, Flujo D)
- IMPLEMENTADO, COMPILADO y TESTEADO (InMemory, CompraNegocioTests 280/280); Reviewer: CON OBSERVACIONES, sin defectos. Spec en `specs/005-carga-compras/anexo-hu10-stock-precios.md`.
- Diseño: `CompraNegocio.AplicarEfectosSecundariosAsync(Compra, lineas)` dentro de la transacción de registro: con Estado=1 suma Stock y sobrescribe PrecioCompra/PrecioVenta de la línea. No existe edición/anulación de compras confirmadas (si se crea, definir ajuste por diferencia).
- SIN VERIFICAR: atomicidad/persistencia en SQL Server real (T10, del usuario); guarda Estado=false sin test directo (hook privado, sin InternalsVisibleTo); posible overflow int de Stock sin tope.

## Restricciones y aprendizajes
- Git es responsabilidad exclusiva del usuario. Prohibido ejecutar cualquier comando Git, incluso de lectura.
- No ejecutar tests de carga, rendimiento ni experimentos descartados por el usuario.
- No ampliar el alcance con refactorizaciones o documentación secundaria.
- Verificar directamente el objetivo solicitado; los tests automatizados son evidencia complementaria.
- No declarar una corrección visual sin ejecutar la aplicación y observar el resultado.
- No exponer credenciales, hashes, tokens ni cadenas de conexión.

## Referencias
- Requisitos y tareas: `specs/`.
- Política de pruebas: `docs/TESTING_POLICY.md`.
- Principios arquitectónicos: `docs/constitution.md`.
