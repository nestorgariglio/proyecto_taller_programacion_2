# AGENTS.md - Sistema Comercial G70
Sistema ERP y Punto de Venta (POS) en C# .NET con arquitectura en 4 capas.

## Agentes y Subagentes
- **coordinator** (Orquestador Principal): Recibe el pedido del usuario, coordina el flujo en orden estricto (`@reviewer` → `@planner` → `@implementer`), exige la aprobación explícita del usuario tras el plan y nunca escribe código directamente.
- **@reviewer**: Audita el estado del repositorio, ejecuta `dotnet build` y `dotnet test`, y valida la arquitectura (aislamiento de capas, 0 AppDbContext en UI).
- **@planner**: Diseña el plan en `tasks.md`, desglosa tareas (T1, T2...), especifica RF/RNF y **SE DETIENE OBLIGATORIAMENTE** a esperar aprobación del usuario.
- **@implementer**: Escribe el código en las capas correspondientes únicamente cuando el plan fue aprobado por el usuario.

## Stack y Estructura
- **Stack**: .NET 8 / C# WinForms, MaterialSkin 2, LiveCharts, Entity Framework Core, SQL Server, BCrypt.Net-Next.
- **Estructura**:
  - `CapaEntidad`: Modelos del dominio (`Usuario`, `Producto`, `Categoria`, `Venta`, etc.).
  - `CapaDatos`: `AppDbContext` (Fuente de verdad del esquema de BD), Fluent API y migraciones EF Core.
  - `CapaNegocio`: Lógica de negocio, validaciones, BCrypt y DTOs.
  - `CapaPresentacion`: Formularios (`Form`), Modales y Controles (`UserControl`).

## Comandos
- **Compilar**: `dotnet build`
- **Ejecutar Pruebas**: `dotnet test`
- **Aplicar Migraciones**: `dotnet ef database update --project CapaDatos --startup-project CapaDatos --context AppDbContext`
- **Preparar BD de desarrollo** (esquema + datos de prueba): `powershell -ExecutionPolicy Bypass -File .\scripts\preparar-bd.ps1`
  - Solo datos: agregar `-SoloDatos`. Detalle y resolución de problemas: `scripts/README-preparar-bd.md`.

## Convenciones
- **Idioma**: Respuestas de la IA, interfaz gráfica, comentarios y documentación siempre en **español**.
- **Aislamiento**: Respetar el aislamiento de 4 capas. La UI nunca toca `AppDbContext` ni ejecuta SQL directo.
- **DTOs**: Usar DTOs para comunicar `CapaNegocio` y `CapaPresentacion`. Las claves nunca viajan en DTOs de lectura.
- **Asincronía**: Mapeo explícito y métodos asíncronos (`async/await`) en operaciones I/O.

## Reglas de Dominio y Datos
- **Bajas Lógicas**: `Estado = true` (Activo), `Estado = false` (Inactivo).
- **Unicidad**: DNI/Correo en usuarios y Código de Barras en productos se validan en `CapaNegocio` y en `AppDbContext`.

## Forma de Trabajar
- **Haz solo lo que se pide**: No añadas funcionalidades ni refactorices código no solicitado.
- **Cambios pequeños y enfocados**: Modifica únicamente lo necesario sin reescribir lo que ya funciona.
- **Cero alucinación**: Si falta una definición o hay ambigüedad en la lógica de negocio, **PARA Y PREGUNTA**. No supongas.
- **Al terminar**: Resume qué has cambiado, qué pruebas ejecutaste y cualquier decisión que deba revisar.

## Memoria
- Al empezar, lee `MEMORY.md` para conocer el estado del proyecto y las decisiones tomadas.
- Al terminar cada tarea: actualiza `MEMORY.md` (Estado actual, Decisiones, Errores a evitar y Próximos pasos).
- Mantenlo breve (máximo ~50 líneas).
- **Seguridad**: No guardes jamás datos sensibles (claves, tokens, API keys o datos personales) en la memoria.

## Límites
- ✅ **Siempre**: Consultar `docs/constitution.md` antes de proponer cambios y mantener las respuestas e interfaz en español.
- ✅ **Siempre**: actualizar `MEMORY.md` al terminar cada tarea.
- ⚠ **Pregunta antes**: Crear migraciones, instalar paquetes NuGet o modificar contratos de datos.
- 🚫 **Nunca**: Borrar filas físicamente (`DELETE`), exponer hashes en DTOs o escribir lógica de negocio en eventos visuales de WinForms.
- 🚫 **Nunca**: Ejecutar `git commit` o `git push` de forma automática. El control de versiones es responsabilidad exclusiva del usuario.

## Verificación
- Antes de dar una tarea por terminada, verificar la compilación limpia con `dotnet build`.
- En cambios de lógica de negocio, ejecutar y validar las pruebas automáticas con `dotnet test`.

## Política de pruebas y timeouts

### Toda prueba declara, antes de ejecutarse
Precondiciones · acción · resultado esperado · tiempo máximo · criterio de éxito · criterio de fallo.
Una prueba sin esos seis datos no se ejecuta: se completa la declaración primero.

### Límites iniciales
| Operación | Máximo |
|---|---:|
| Arranque de aplicación | 20 s |
| Respuesta visual sencilla | 10 s |
| Operación funcional individual | 30 s |
| Escenario completo | 120 s |
| Experimento completo | 8 min |

Ajustables con justificación técnica escrita en el informe; **nunca espera indefinida**. El plazo corre desde el inicio real de la operación, no desde que el agente recuerda comprobarla.

### Qué detecta el timeout
El timeout es criterio de fallo de la aplicación, no un límite de clics. Casos a cubrir explícitamente:
1. Evento que nunca termina.
2. Botón pulsado y grilla que nunca se actualiza.
3. Consulta que espera indefinidamente.
4. Hilo de UI que deja de responder.
5. Operación asíncrona que no completa.
6. Resultado esperado que nunca aparece.
7. Aplicación visualmente congelada.

Al vencer: **`FAILED — TIMEOUT / POSIBLE BLOQUEO`**. No es "pendiente", no se sigue esperando y no se alarga el plazo para ver si termina.

### Diagnóstico obligatorio al vencer (en este orden)
1. Acción en curso.
2. Hora de inicio y tiempo transcurrido frente al plazo.
3. Resultado esperado vs observado.
4. Captura de imagen de la aplicación.
5. Ventana activa y PID.
6. Logs, excepciones y salida de consola.
7. Estado del proceso (vivo/terminado, respondiendo, hilos, CPU, memoria).
8. *Stack dump* cuando sea técnicamente posible, para identificar el hilo bloqueado (si la herramienta no está instalada, se declara).
9. Última operación completada correctamente.
10. Clasificación: `timeout` / `aplicación no responsiva` / `error de automatización`.

**No matar el proceso antes de recopilar la evidencia.** Después: cancelar, cerrar y liberar. Si el watchdog no logra terminar el proceso, se informa como fallo del mecanismo, nunca como prueba superada.

### Prohibición de continuar tras un timeout
No repetir el clic, no reiniciar la operación, no completar el flujo por otra vía, no saltar el paso fallido, no ocultar el fallo en el informe. Las operaciones con efectos secundarios (por ejemplo registrar una compra) **nunca** se repiten sin verificar antes si la anterior se completó. **Máximo un reintento automático**, solo para operaciones seguras y solo tras diagnosticar el primer fallo.

### Responsabilidades
- **Coordinator**: fija los límites de cada delegación, supervisa, recibe los informes de timeout, no deja avanzar una fase con pruebas fallidas y decide cuándo escalar.
- **Reviewer**: ejecuta las pruebas, verifica el comportamiento real, espera dentro de los límites, registra el fallo y recopila la evidencia al vencer, e investiga la causa. **No repara la aplicación.**
- **Planner**: documenta los defectos, los incorpora al diagnóstico y al plan, y separa confirmados de hipótesis.
- **Implementer**: implementa las correcciones autorizadas y mantiene estas instrucciones operativas. **No sustituye al Reviewer en la validación.**

### Timeout real, no declarativo
Un `Task.WaitAsync`, un `CancellationToken` o un `timeout` pasado a una API **no** garantizan que la operación subyacente se haya detenido: solo dejan de esperarla. Si la aplicación puede quedar bloqueada, la prueba se ejecuta en un **proceso aislado con watchdog externo** que mide el plazo, captura evidencia, termina el proceso de prueba y no reintenta:

```powershell
$p = Start-Process <exe-o-script> -PassThru              # 1. lanzar en proceso propio (el watchdog es otro proceso)
if ($p.WaitForExit(30000)) { <evaluar criterio de éxito> }  # 2. espera acotada a N ms
# 3. evidencia ANTES de matar: PID, ventana activa, Responding, captura, logs, ultima operacion
& taskkill.exe /PID $p.Id /T /F                           # 4. terminar el arbol completo
if (Get-Process -Id $p.Id -ErrorAction SilentlyContinue) { Stop-Process -Id $p.Id -Force }
# 5. registrar "FAILED — TIMEOUT / POSIBLE BLOQUEO" con inicio, duracion, esperado vs observado, intentos=1
```

Límites reales comprobados en este equipo (no supuestos):
- La herramienta `shell` de OpenCode **sí** corta la llamada al vencer su `timeout` (por defecto 120 s) y termina el árbol cuando el proceso se lanzó como hijo directo o con `Start-Process`.
- Un proceso **desvinculado** (`cmd /c start`, tarea programada, servicio) **sobrevive** a ese corte. Por eso el watchdog que verifica el PID es obligatorio y no reemplazable por el plazo de la herramienta.
- `opencode.json` no ofrece un límite global de duración para `shell` ni para una ejecución de agente; solo `mcp.timeout`, `webfetch.timeout` y `experimental.location_idle_timeout`. El único refuerzo automático posible es un *plugin* de servidor (`ctx.shell.hook("create.before")`) o la interrupción externa vía `POST /api/session/{id}/interrupt`; añadirlos requiere autorización explícita del usuario.
