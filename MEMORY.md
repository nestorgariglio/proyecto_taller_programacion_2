# MEMORY.md — SISTEMA COMERCIAL G70

Memoria global del proyecto. Máximo aproximado: 50 líneas. Uso y actualización a cargo del Coordinator.

## Proyecto
- Stack: .NET 10, C#, WinForms, MaterialSkin 2, EF Core y SQL Server.
- Arquitectura: cuatro capas. Autoridad: `docs/constitution.md`.
- Specs 001–004: registradas previamente como implementadas y validadas; consultar sus documentos antes de modificar funcionalidades existentes.
- BD de desarrollo informada: `DB_SISTEMA_VENTA`, instancia `localhost\SQLEXPRESS`.

## Prioridad vigente: Spec 005 — HU-09
- HU-09 sigue siendo la prioridad. HU-10 y Spec 006 están estacionadas.
- C-1: manejo de errores de lecturas de BD implementado. Última validación informada: build sin errores y pruebas focalizadas de Compras 271/271 aprobadas.
- DECISIÓN CERRADA: la interfaz visual actual de `ComprasForm` (catálogo a la izquierda, documento de compra a la derecha, diseño aprobado por el usuario) no se rediseña. Ningún agente debe proponer rediseños; solo se permiten ajustes puntuales necesarios para integrar nuevos controles o comportamientos, conservando distribución y criterios visuales.
- BD de desarrollo: `/preparar-bd` reparado (modelo EF sin cambios pendientes, migraciones aplicadas, seed ejecutada e idempotente; verificación por claves determinísticas de la seed). La seed restablece solo sus propios registros; ver `scripts/README-preparar-bd.md`.
- Documentación/checklist de cierre: Planner terminó por límite de uso. Comprobar los archivos reales antes de afirmar que quedaron actualizados.
- B-1 (compuerta de proveedor) y la redacción de RF-34/RNF-9 no deben darse por resueltas ni modificarse en esta refactorización. RNF-9 y las pruebas de carga/rendimiento fueron descartadas por decisión del usuario.
- T10 de atomicidad: ejecución manual reservada al usuario. Ningún agente debe ejecutarla ni modificar la BD durante esa prueba.

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
