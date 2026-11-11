# POLÍTICA DE PRUEBAS, TIMEOUTS Y DIAGNÓSTICO

## 1. Principio

Cada prueba debe comprobar un criterio concreto de aceptación. No ejecutar pruebas por acumular resultados verdes. La evidencia debe corresponder al objetivo solicitado: compilar o ejecutar tests no sustituye una comprobación visual o funcional cuando esta es necesaria.

## 2. Declaración previa

Antes de ejecutar una prueba o escenario, declarar:

- Precondiciones.
- Acción.
- Resultado esperado.
- Tiempo máximo.
- Criterios de éxito y fallo.

Una declaración por ejecución o escenario es suficiente. No repetirla para cada test interno de una misma suite.

## 3. Límites iniciales

| Operación | Máximo |
|---|---:|
| Arranque de aplicación | 20 s |
| Respuesta visual sencilla | 10 s |
| Operación funcional individual | 30 s |
| Escenario completo | 120 s |
| Experimento completo | 8 min |

El tiempo comienza cuando se inicia realmente la operación. Nunca se permite una espera indefinida. Un límite solo puede ampliarse con justificación técnica y autorización cuando corresponda.

## 4. Ejecución según el objetivo

- **Tests automatizados:** registrar comando, filtro, duración, código de salida y errores.
- **Pruebas funcionales:** ejecutar el flujo afectado y comprobar el resultado observable.
- **Pruebas visuales:** ejecutar la aplicación, reproducir el escenario y observar la pantalla en las condiciones indicadas.
- **Pruebas de persistencia:** consultar el estado real de la BD cuando el criterio lo requiera.
- **Pruebas de atomicidad:** provocar el fallo controlado y comprobar que no queden efectos parciales.

No ejecutar suites completas cuando las pruebas focalizadas aportan evidencia suficiente. Reservar la regresión completa para los puntos de control acordados o cuando el alcance lo justifique.

## 5. Timeouts y detección de bloqueos

Contemplar, cuando corresponda:

- Evento que no termina.
- Operación asíncrona que no completa.
- Grilla o pantalla que no se actualiza.
- Consulta que queda esperando.
- Hilo de UI que deja de responder.
- Aplicación visualmente congelada.
- Resultado esperado que nunca aparece.

Al vencer el plazo registrar exactamente:

`FAILED — TIMEOUT / POSIBLE BLOQUEO`

No clasificarlo como pendiente ni seguir esperando.

## 6. Watchdog reutilizable

El mecanismo común previsto es `scripts/watchdog.ps1`, mantenido como herramienta del proyecto y ejecutado por Reviewer con autorización de shell.

**Estado de instalación:** este paquete no contiene `scripts/watchdog.ps1`. Hasta que el usuario lo cree/autorice y se compruebe su funcionamiento, ningún agente debe afirmar que dispone de un watchdog externo funcional. No improvisar un watchdog de una sola línea ni reemplazarlo por `WaitAsync`.

Cuando exista y haya sido validado, el watchdog debe:

1. Lanzar o supervisar el proceso de prueba aislado.
2. Medir el tiempo desde el inicio real de la operación.
3. Esperar solo hasta el límite declarado.
4. Antes de terminar, intentar recopilar acción, hora/duración, esperado frente a observado, captura/ventana/PID, logs/excepciones, estado del proceso, stack dump si es posible y última operación correcta.
5. Finalizar el proceso y sus descendientes.
6. Comprobar que el proceso y los descendientes dejaron de existir.
7. Registrar el fallo si excedió el plazo o si no pudo detener el árbol de procesos.

Si el watchdog no está disponible, detener la prueba que dependa de él e informar el requisito pendiente. Un `Task.WaitAsync`, `CancellationToken` o timeout del llamador no demuestra que la operación subyacente haya terminado.

## 7. Reintentos y efectos secundarios

- No repetir clics ni reiniciar la operación ciegamente.
- No completar el flujo por otra vía ni saltar el paso fallido.
- Las operaciones con efectos secundarios nunca se repiten sin comprobar antes si la operación anterior se completó.
- Se permite un único reintento para operaciones seguras, después de diagnosticar el primer fallo.
- No ocultar ni reclasificar un timeout para continuar.

## 8. Redirección obligatoria de logs extensos

Antes de ejecutar comandos que puedan producir más de 20 líneas, redirigir stdout y stderr a `%TEMP%\G70\` desde el inicio. No mostrar el log completo en el chat del Coordinator.

Ejemplo PowerShell para una compilación:

```powershell
$dir = Join-Path $env:TEMP 'G70'
New-Item -ItemType Directory -Force -Path $dir | Out-Null
$log = Join-Path $dir ('build-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.log')
dotnet build *> $log
$code = $LASTEXITCODE
Get-Content $log -Tail 20
"EXIT_CODE=$code"
"LOG_FILE=$log"
exit $code
```

El agente devuelve el código de salida, un resumen breve y la ruta del log. El Coordinator solicita líneas adicionales solo si necesita investigar el error.

## 9. Seguridad de las pruebas

- No ejecutar seed, SQL de escritura, migraciones ni operaciones destructivas sin autorización explícita.
- Antes de operar sobre una BD, comprobar servidor y base de destino sin exponer credenciales.
- No ejecutar pruebas que alteren datos reales o de desarrollo sin conocer sus efectos y contar con autorización.
- Los procedimientos de atomicidad que el usuario haya reservado para ejecución manual quedan exclusivamente a cargo del usuario.

## 10. Pruebas de integración contra SQL Server (`G70_TEST_CONNECTION`)

- **Propósito:** cadena de conexión que usan los proyectos `CapaNegocio.IntegrationTests`. Debe apuntar **exclusivamente** a la base `DB_SISTEMA_VENTA_TEST`; si apunta a otra base, las pruebas no se ejecutan.
- **Formato (sin credenciales reales):** `Server=<servidor>\<instancia>;Database=DB_SISTEMA_VENTA_TEST;Integrated Security=true;TrustServerCertificate=true;`
- **Definición:** variable de entorno del proceso o del usuario; nunca se versiona ni se imprime en logs o informes.
- **Si falta o no apunta a la base de pruebas:** las pruebas quedan **omitidas** (no pasadas) con el motivo `G70_TEST_CONNECTION no definida o no apunta a DB_SISTEMA_VENTA_TEST: prueba NO ejecutada`. Una omisión no es evidencia de que la prueba haya pasado.
- **Residuos:** las pruebas que usan la fábrica transaccional revierten todo; las de unicidad dejan filas `ITEST-%` (no se borran).
