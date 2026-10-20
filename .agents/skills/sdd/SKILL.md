---
name: sdd
description: Úsala siempre que trabajes con Spec-Driven Development en este proyecto (docs/constitution.md o cualquier archivo dentro de specs/) - redactar, revisar o cambiar specs, planes y tareas, o implementar y validar tareas de una spec.
---
# Spec-Driven Development (SDD)

## Flujo
Constitución → Spec → Clarificación → Plan → Tareas → Implementación → Validación → Cambio.

- **Aprobación explícita**: Nunca pases a la siguiente fase sin la aprobación explícita del usuario.
- **La spec manda**: Si algo no está en la spec activa, no se implementa. Si falta una decisión, detente y pregunta.
- **Cambio de requisitos**: Un cambio se realiza primero en la spec, luego en el plan y las tareas, y por último en el código C#.
- **Estructura por carpeta**: Cada funcionalidad vive en su carpeta `specs/NNN-nombre/` con `spec.md`, `plan.md` y `tasks.md`.
- **Memoria viva**: Al terminar cada fase, actualiza `MEMORY.md`.

---

## Plantilla de spec (`spec.md`)

# Spec NNN — <Nombre de la funcionalidad>
Estado: borrador | aprobada | implementada

## Contexto y objetivo
<Qué problema del sistema comercial/ERP resuelve y por qué. Un párrafo.>

## Usuarios / actores
<Quién lo utiliza en el sistema (ej. Administrador, Vendedor, Cajero).>

## Historias de usuario
- HU-1: Como <rol>, quiero <acción> para <beneficio>.

## Definiciones
<Términos del dominio comercial/POS que requieran precisión.>

## Requisitos funcionales en EARS (en español)
Genera la cantidad de requisitos secuenciales (RF-1, RF-2, ..., RF-n) que la funcionalidad requiera, utilizando las estructuras EARS:
- RF-1: CUANDO <evento>, EL SISTEMA <respuesta>.
- RF-2: SI <condición no deseada>, ENTONCES EL SISTEMA <respuesta>.
- RF-3: MIENTRAS <estado>, EL SISTEMA <respuesta>.
- RF-4: EL SISTEMA <comportamiento permanente>.
- ...
- RF-n: <siguiente requisito si aplica>.

*Nota: La spec describe únicamente el QUÉ y el POR QUÉ. Nada de stack, arquitectura, clases C# ni formularios.*

## Requisitos no funcionales
<Rendimiento de interfaz, límites de BD, tiempo de respuesta en UI WinForms, etc.>

## Casos límite
<Registros duplicados, valores nulos/negativos, concurrencia en BD, desconexión SQL.>

## Fuera de alcance
<Lo que explícitamente NO se realiza en esta iteración.>

## Criterios de finalización
<Ej.: Todos los RF validados con `dotnet test` en verde, compilación limpia con `dotnet build` y validación visual en el formulario.>

## Dudas abiertas
- [NECESITA ACLARACIÓN] <duda pendiente de confirmación por el usuario>

---

## Plan técnico (`plan.md`)

Estructura obligatoria para C# .NET 8 / 4 Capas:

1. **Responsabilidades por Capa**:
   - `CapaEntidad`: Modelos de dominio y enumeradores.
   - `CapaDatos`: Configuración en `AppDbContext` (Fluent API), consultas EF Core y migraciones.
   - `CapaNegocio`: Reglas de negocio, validaciones, BCrypt y DTOs.
   - `CapaPresentacion`: Formularios (`Form`), componentes MaterialSkin 2 y validaciones visuales.
2. **DTOs y Mapeos**: Definición de DTOs de entrada y salida entre Presentación y Negocio (sin exponer credenciales ni entidades puras).
3. **Persistencia**: Cambios requeridos en `AppDbContext` y comando de migración EF Core.
4. **Decisiones técnicas**: Justificación de arquitectura junto a las alternativas descartadas.
5. **Estrategia de tests**: Cobertura con `dotnet test` para la lógica en `CapaNegocio`.
6. **Trazabilidad**: Indicar qué Requisito Funcional (RF-x) cubre cada componente del plan.

---

## Tareas (`tasks.md`)

- [ ] **Tn. <Descripción de la tarea C#>.** RF-x, RF-y
  - Hecho cuando: <comprobación verificable, ej: test de `CapaNegocio` en verde con `dotnet test` o compilación limpia>.

- Máximo 20 a 30 minutos por tarea.
- Orden de dependencia estricto entre capas (`CapaEntidad` → `CapaDatos` → `CapaNegocio` → `CapaPresentacion`).
- Si se superan las 10 tareas, proponer dividir la spec en sub-módulos.

---

## Implementación

1. Trabajar en **una sola tarea a la vez**.
2. Escribir/preparar primero las pruebas automáticas o métodos de verificación y comprobar que fallen en rojo.
3. Escribir el código en las capas C# correspondientes hasta que compile y pase en verde.
4. Ejecutar validación técnica con `dotnet build` y `dotnet test`.
5. Marcar la tarea como hecha en `tasks.md` y **PARAR**. No iniciar la siguiente tarea sin autorización.