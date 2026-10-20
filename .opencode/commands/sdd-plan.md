description: SDD - Genera el plan técnico de arquitectura para una spec aprobada
agent: plan

Lee docs/constitution.md, AGENTS.md y specs/$1/spec.md.
NO escribas código.

Genera specs/$1/plan.md con:
- Archivos que se crean o modifican en cada capa (CapaEntidad, CapaDatos, CapaNegocio, CapaPresentacion) y la responsabilidad de cada uno.
- DTOs y objetos de respuesta necesarios en CapaNegocio.
- Cambios requeridos en AppDbContext (índices, relaciones, restricciones) y la migración correspondiente.
- Decisiones técnicas justificadas (con su alternativa descartada).
- Estrategia de pruebas automáticas con `dotnet test`.

Todo debe respetar la constitución y cubrir todos los RF. Indica qué RF cubre cada parte. Si la spec no está aprobada o tiene dudas abiertas, detente y avísame.