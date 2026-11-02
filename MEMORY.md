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
- C-2: corrección puntual de alineación aplicada en `ComprasForm.cs`; no existe evidencia visual de que haya corregido el resultado en ejecución.
- La interfaz continúa sin resolverse según la comprobación del usuario. No declarar T21 ni otra mejora visual como terminada sin ejecutar la aplicación y observar el resultado.
- Seed: prechecks informados como correctos para `localhost\SQLEXPRESS / DB_SISTEMA_VENTA`; seed todavía no ejecutada según el último estado comunicado.
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
