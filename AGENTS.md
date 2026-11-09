# SISTEMA COMERCIAL G70 — REGLAS GLOBALES

Contexto común de todos los agentes. Las responsabilidades particulares están en sus perfiles.

## 1. Flujos de trabajo

El Coordinator selecciona un único flujo según la solicitud:

- **A — Corrección puntual:** Coordinator → Implementer → verificación específica de Reviewer. Sin spec ni plan cuando el alcance, el comportamiento esperado y el criterio de aceptación están claros y autorizados.
- **B — Diagnóstico:** Coordinator → Reviewer → informe consolidado.
- **C — Funcionalidad nueva o cambio amplio:** Coordinator → Planner → aprobación del usuario → Implementer → Reviewer.
- **D — Funcionalidad nueva autónoma** (**Coordinator → Planner [aprobación interna de spec y plan] → Implementer → Reviewer**). Es el único flujo que exime a los agentes de consultas y aprobaciones intermedias del usuario. Su asignación otorga autorización previa para tomar decisiones de diseño, lógica e implementación, incluyendo modificaciones de código y pruebas, priorizando la arquitectura, las buenas prácticas y la eficiencia en el uso de contexto y tokens. Esta autonomía se limita al objetivo y alcance asignados: no autoriza alterar infraestructura, esquemas, contratos, dependencias ni archivos protegidos. Ante estas restricciones, el agente debe omitir la parte bloqueada, continuar con el trabajo autorizado e informar la limitación.

No convocar agentes ni generar documentos si no aportan valor al objetivo.

## 2. Verificación del objetivo

La comprobación debe corresponder exactamente a lo que pidió el usuario. Está prohibido sustituir una verificación directa por otra indirecta o más sencilla.

- **UI:** ejecutar la aplicación, reproducir el escenario y comprobar visualmente el resultado en las condiciones problemáticas. Inspeccionar código o propiedades no demuestra una corrección visual.
- **Lógica:** ejecutar el comportamiento afectado y comprobar entradas, resultados, mensajes y efectos secundarios pertinentes.
- **Persistencia:** comprobar el estado real de la BD cuando el criterio requiera demostrar que los datos se guardaron, modificaron o revirtieron.
- **Atomicidad:** provocar el fallo previsto y comprobar el estado real de la BD después del rollback.
- **Tests:** ejecutar únicamente los que aporten evidencia sobre los criterios modificados y las regresiones relevantes.

Una compilación exitosa demuestra que el código compila. Un test demuestra únicamente el comportamiento que realmente ejercita. Sin evidencia directa suficiente, informar `SIN VERIFICAR`.

## 3. Alcance y coordinación

- Ejecutar exclusivamente el alcance solicitado y autorizado.
- No agregar refactorizaciones, mejoras ni tareas secundarias.
- No reabrir decisiones aprobadas ni funcionalidades estacionadas.
- Consultar antecedentes y decisiones antes de preguntar.
- Agrupar preguntas, decisiones y autorizaciones en una sola entrega.
- No repetir análisis ni verificaciones cuya evidencia siga vigente.
- Trabajar secuencialmente cuando existan dependencias.
- No continuar una fase que requiera aprobación sin recibirla.
- Máximo dos rondas de correcciones por entrega; después, informar el bloqueo.

## 4. Arquitectura, seguridad y permisos

- Respetar `docs/constitution.md`.
- No exponer credenciales, hashes, tokens ni cadenas de conexión.
- Migraciones, cambios de esquema o contratos, paquetes, dependencias y proyectos fuera del alcance requieren autorización explícita.
- No borrar físicamente registros operativos cuando corresponda una baja lógica.
- No activar aprobaciones automáticas globales.

**GIT: PROHIBIDO ABSOLUTAMENTE.** Ningún agente ejecuta comandos Git, directos o indirectos, ni siquiera de lectura. El usuario administra ramas, commits, restauraciones y cualquier operación de versionado. No ejecutar scripts o comandos envoltorio sin inspeccionar que no invoquen Git.

## 5. Modificación de archivos
- Antes de editar, presentar en texto plano los cambios propuestos, agrupados en **AGREGADO**, **MODIFICADO** y **ELIMINADO**. Indicar archivo, ubicación y código exacto; en MODIFICADO mostrar original y reemplazo. No depender de colores ni diffs visuales.
- Esperar autorización explícita del usuario antes de aplicar cada propuesta.
- Aplicar parches puntuales, preservar código ajeno al alcance y evitar sobrescrituras completas salvo necesidad justificada.

## 6. Contexto y documentación

- `docs/constitution.md`: autoridad arquitectónica y de dominio.
- `MEMORY.md`: estado vigente y decisiones persistentes.
- `specs/`: requisitos, planes y tareas específicos.
- `docs/TESTING_POLICY.md`: consultar al definir o ejecutar pruebas, o diagnosticar bloqueos.

No duplicar estas fuentes dentro de los perfiles ni volver a leer documentos cuyo contenido pertinente ya haya sido transmitido por Coordinator.

## 7. Control de salidas y logs

- No volcar logs extensos, stack traces ni diffs gigantes en las respuestas al Coordinator.
- Antes de ejecutar comandos que previsiblemente produzcan más de 20 líneas (por ejemplo, `dotnet build`, `dotnet test` o scripts), redirigir stdout y stderr a un archivo en `%TEMP%\G70\`.
- No ejecutar primero sin redirección para guardar el log después: la redirección debe estar activa desde el inicio del comando.
- Devolver como máximo 20 líneas relevantes: resumen, error principal, cantidad de errores/advertencias, código de salida y ruta del log completo.
- El Coordinator solicita fragmentos adicionales del archivo solo cuando sean necesarios para investigar.
- No copiar secretos, credenciales ni cadenas de conexión al log resumido ni al chat.
- Mantener los logs temporales fuera del repositorio. No crear `temp/` en el proyecto para este fin.

## 8. Finalización y memoria

Distinguir siempre `IMPLEMENTADO`, `COMPILADO`, `TESTEADO` y `VERIFICADO`.

Estados: `TERMINADA` (criterios comprobados con evidencia), `PENDIENTE`, `EN PROGRESO`, `BLOQUEADA`, `SIN VERIFICAR` o `FALLIDA`.

Actualizar `MEMORY.md` al cierre cuando cambie el estado o exista una decisión persistente. Guardar el estado vigente, no el historial de ejecución.
