# Plan 006 — Verificación de conexión en el inicio
Estado: borrador (pendiente de aprobación del usuario)
Spec: `specs/006-verificacion-conexion-inicio/spec.md` (aprobada 2026-11-01)

> Nota de stack: la solución compila hoy contra `net10.0` / `net10.0-windows` (no .NET 8, ver C-9). El plan usa lo disponible en ese runtime (`TimeProvider`, `CancellationTokenSource.CancelAsync`).

## 0. Diagnóstico

### 0.1 Defectos y hechos confirmados (verificados en el código)
- **C-1**: `CapaPresentacion/Inicio/inicio.cs` usa `AppDbContext`: `:1` `using CapaDatos`, `:14` `using Microsoft.EntityFrameworkCore`, `:26` campo `_db`, `:45` parámetro del constructor, `:57` asignación, `:465-466` `OpenConnectionAsync`/`CloseConnectionAsync`.
- **C-2**: bloque `catch` vacío («modo mock») en `inicio.cs:468-471`: la excepción se descarta.
- **C-3**: `inicio_Shown` es `async void` (`inicio.cs:459`), suscrito en `inicio.Designer.cs:213`.
- **C-4**: cierres de `inicio`:
  - X → `Close()` de `inicio`; hoy `inicio_FormClosed` no está suscrito (no aparece en el Designer), por lo que no se llama a `Application.Exit()`; `login` sigue oculto y el proceso no termina. Se corrige con DU-1 (suscripción en el constructor).
  - SALIR → `Application.Exit()` (`inicio.cs:405-407`).
  - CERRAR SESIÓN → `CierreSesionSolicitado` (`inicio.cs:411-412`) → `login` hace `Hide()` + `Dispose()` de `inicio` (`login.cs:91-101`, líneas 94-95); el bucle de mensajes de `login` sigue vivo.
- **C-5**: se crea un `inicio` nuevo por cada ingreso (`login.cs:68`, fábrica `Func<inicio>` en `Program.cs:58`).
- **C-6**: `login` consulta la BD al ingresar (`login.cs:59`): con BD no disponible no se llega a `inicio` a mano (origen de D-21).
- **C-7**: solo `inicio.cs` y `Program.cs` (`:1`, `:5`, `:32`) referencian `AppDbContext`/CapaDatos/EF Core en CapaPresentacion.
- **C-8**: no existe proyecto de pruebas de CapaPresentacion. `CapaNegocio.Tests` usa `net10.0`, xUnit 2.9.3 y EF Core InMemory 10.0.11.
- **C-9**: proyectos en `net10.0` / `net10.0-windows`; `TimeProvider` y `CancelAsync` disponibles.
- **C-10**: `appsettings` usa `Integrated Security=true` (sin contraseña en la cadena).
- **C-11**: MaterialSkin 2.3.1 ofrece `MaterialDialog(Form owner, string título, string texto, string botón, bool cancelar, ...)` y `MaterialMessageBox`.
- **C-12**: `TemaAplicacion` es `internal` (`CapaPresentacion/Tema/TemaAplicacion.cs:9`); no hay `InternalsVisibleTo` en la solución.

### 0.2 Hipótesis (no verificadas; cada una con su verificación)
- **H-1**: `AppDbContext` es *scoped* y resolverlo desde la raíz lo retiene de por vida. Mitigado por DT-2 (contexto dedicado por intento); no requiere verificación.
- **H-2**: `OpenConnectionAsync(ct)` aborta el intento físico al cancelar el token. Verificación: T1 (punto 3). Respaldo: `Connect Timeout=12`. Si la prueba la refuta, PARAR y escalar (no se da por cumplido RF-9 solo con `Connect Timeout`).
- **H-3**: la apertura de SqlClient tiene tramos síncronos que bloquearían el hilo de UI. Mitigado con `Task.Run` (DT-4); verificación: punto 21.
- **H-4**: MaterialSkin muestra el texto del botón en mayúsculas («ACEPTAR»). **Aceptado por el usuario (7a)**: solo se registra la observación visual; el DTO conserva «Aceptar».
- **H-5**: `MaterialDialog` respeta `Environment.NewLine` y no recorta la segunda línea. Verificación: T8 (inspección) y captura en T11. Si se recorta, PARAR y escalar.
- **H-6**: `Application.Exit()` no queda retenido por continuaciones pendientes o hilos del *thread pool*. **Riesgo residual (reviewer)**: una continuación que hace `Post` sobre el control de sincronización ya liberado tras `Application.Run`. Vigilar en el punto 26 (T13).
- **H-7**: el botón del diálogo es localizable por texto en las pruebas; si no, se usa `DialogResult`/`AcceptButton`.
- **H-8**: una continuación de `await` puede reanudarse tras liberar `inicio`. Mitigado con `EstaDisponible`; verificación: puntos 10 y 25.
- **H-9**: CERRAR SESIÓN (`Hide()` + `Dispose()`) no dispara `FormClosed` ni, por tanto, `Application.Exit()` tras DU-1. Verificación: T8 y puntos 20 y 25. Si lo dispara, PARAR y escalar al usuario.
- **H-10**: la duración de la suite completa de CapaNegocio.Tests (~740 pruebas + nuevas) no está medida; podría superar 120 s en una sola invocación. Mitigación (T7, T14): la suite completa se ejecuta por grupos `--filter` de modo que cada invocación quede ≤ 120 s bajo watchdog; si una invocación supera el límite, se registra como FAILED — TIMEOUT y se divide el grupo (no se amplía el límite).

## 1. Responsabilidades por capa

### CapaEntidad
Sin cambios.

### CapaDatos
- **`SondaConexionBd`** (`CapaDatos/Diagnostico/SondaConexionBd.cs`):
  - `static string ConstruirCadena(string cadenaConfigurada)`: arma la cadena propia con `SqlConnectionStringBuilder` (`Pooling = false`, `ConnectTimeout = 12`).
  - `Task<bool> AbrirYCerrarAsync(CancellationToken ct)`: crea un `AppDbContext` dedicado (`UseSqlServer(cadenaDerivada)`), `OpenConnectionAsync(ct)`, devuelve `State == Open`, y en `finally` `CloseConnectionAsync` + `DisposeAsync` (solo liberación, no mecanismo de corte, ver DT-3). Sin leer ni escribir datos.

### CapaNegocio
- `Enums/`: `EstadoConexion` (Accesible, NoAccesible), `MotivoFalloConexion` (Excepcion, BaseNoAccesible, PlazoVencido), `TipoEntradaRegistro` (FalloConexion, ExcepcionInesperada), `TipoAvisoInicio` (Conexion, Generico).
- `Diagnostico/` contratos:
  - `IComprobadorConexion { Task<EstadoConexion> ComprobarAsync(CancellationToken ct); }` (respeta el token).
  - `IRegistroErrores { Task<bool> RegistrarAsync(EntradaRegistro entrada); }` (nunca lanza; `false` = fallo del registro).
  - `IPantallaAvisoInicio { bool EstaDisponible { get; } IAvisoPreparado PrepararAviso(AvisoInicioDto aviso); }`
  - `IAvisoPreparado : IDisposable { void Presentar(); }`
- `TextosAvisoInicio`: textos literales de D-23 y la línea adicional.
- `EntradaRegistro` (interno al negocio; no viaja a la UI).
- `EnmascaradorSecretos`: recibe la cadena configurada; oculta con `[OCULTO]` la cadena configurada, la cadena derivada de la sonda (`SondaConexionBd.ConstruirCadena`) y la contraseña si existe (`SqlConnectionStringBuilder.Password`). Ignora valores nulos o vacíos (evita `string.Replace("")`). Reemplaza primero los secretos más largos.
- `FormateadorEntradaRegistro`: excepción completa (tipo, mensaje, stack trace) con toda la cadena de internas, todas las internas de `AggregateException`, protección anti-ciclos; aplica el enmascarador a mensaje y stack trace de cada nivel.
- `RegistroErroresArchivo`: `ObtenerCarpetaPredeterminada()` (`%LOCALAPPDATA%\SistemaComercialG70\Logs\`); `SemaphoreSlim` estático (RF-19); dentro del bloqueo toma `TimeProvider.GetLocalNow()` una sola vez (nombre de archivo y hora de la entrada del mismo instante); `Directory.CreateDirectory`; `FileStream` (`FileMode.Append`, `FileAccess.Write`, `FileShare.Read`); una sola escritura UTF-8 por entrada; cualquier excepción → `false`.
- `ComprobadorConexionBd : IComprobadorConexion`: recibe la cadena configurada, crea `SondaConexionBd` y mapea `true/false` a `EstadoConexion`.
- `VerificacionConexionNegocio`:
  - constructor público `(IComprobadorConexion, IRegistroErrores)` con plazo fijo de 10 s;
  - constructor `internal` `(IComprobadorConexion, IRegistroErrores, TimeSpan plazo)` solo para pruebas unitarias, expuesto a `CapaNegocio.Tests` con `[assembly: InternalsVisibleTo("CapaNegocio.Tests")]` declarado en `VerificacionConexionNegocio.cs` (no requiere tocar `CapaNegocio.csproj`);
  - `Task EjecutarAsync(IPantallaAvisoInicio pantalla)`: nunca lanza (algoritmo §4).

### CapaPresentacion
- `inicio.cs`:
  - quita `using CapaDatos`, `using Microsoft.EntityFrameworkCore`, `_db` y el parámetro `AppDbContext`; inyecta `VerificacionConexionNegocio` e `IVentanaAvisoInicio`;
  - **DU-1**: en el constructor, `FormClosed += inicio_FormClosed;` (handler existente en `inicio.cs:474`, sin modificarlo);
  - implementa `IPantallaAvisoInicio`: `EstaDisponible = !_cerrandose && !IsDisposed && !Disposing && IsHandleCreated`;
  - `_cerrandose`: en `FormClosing`, `if (!e.Cancel) _cerrandose = true;`; en CERRAR SESIÓN se asigna antes de invocar `CierreSesionSolicitado`;
  - `inicio_Shown`: `ActiveControl = null; await _verificacion.EjecutarAsync(this);` sin `catch` (DT-6); se elimina el `catch` vacío;
  - `PrepararAviso` delega en `IVentanaAvisoInicio`.
- `Inicio/IVentanaAvisoInicio.cs`: `IAvisoPreparado Preparar(Form dueño, AvisoInicioDto aviso)`.
- `Inicio/VentanaAvisoMaterial.cs`: crea `MaterialDialog(dueño, Titulo, Mensaje, TextoBoton, false)`; `Presentar()` = `ShowDialog(dueño)`; `Dispose()` libera el diálogo. Verificar en T8 que la firma recibe `Form`.
- `Program.cs` (solo raíz de composición de dependencias y el atributo de ensamblado `InternalsVisibleTo`):
  - `IRegistroErrores` singleton con fábrica explícita (`RegistroErroresArchivo` + formateador + enmascarador con la cadena configurada + `TimeProvider.System`);
  - `IComprobadorConexion` transient con fábrica (`new ComprobadorConexionBd(connectionString)`): Program.cs no nombra tipos de CapaDatos aparte de `AppDbContext` (D-5);
  - `VerificacionConexionNegocio` transient con fábrica explícita (constructor público);
  - `IVentanaAvisoInicio` transient → `VentanaAvisoMaterial`;
  - `[assembly: InternalsVisibleTo("CapaPresentacion.Tests")]` y `[assembly: InternalsVisibleTo("CapaPresentacion.ArnesPruebas")]` para acceder a `TemaAplicacion` (C-12). No altera comportamiento.
- `CapaPresentacion.csproj:20` (referencia a CapaDatos) se conserva: la necesita `Program.cs` para registrar `AppDbContext` (D-5). No se edita el `.csproj`.

## 2. DTOs y mapeos
- **`AvisoInicioDto`** (`CapaNegocio/DTOs/Diagnostico/AvisoInicioDto.cs`, solo lectura): `Tipo`, `Titulo`, `Mensaje`, `TextoBoton`, `IncluyeLineaRegistro`.
  - Conexión: «Error de conexión» / «No se pudo conectar con la base de datos.» / «Aceptar».
  - Genérico: «Error» / «Se produjo un error inesperado al iniciar» / «Aceptar».
  - Con fallo del registro: `Mensaje` += `Environment.NewLine` + «No se pudo guardar el registro del error».
  - Sin información técnica, sin claves ni rutas.
- **`EntradaRegistro`** (interno): `Tipo`, `Motivo?`, `Excepcion?`.
- **Formato de entrada** (texto UTF-8):
  ```
  ===== ENTRADA DE REGISTRO =====
  Fecha y hora (local): AAAA-MM-DD HH:mm:ss.fff zzz
  Tipo: fallo de conexión | excepción inesperada
  Motivo: excepción | base de datos no accesible | plazo de 10 s vencido   (solo fallo de conexión)
  --- Excepción [1] ---
  Tipo: ...
  Mensaje: ...
  Stack trace: ...
  --- Excepción [1.1] --- (internas; en AggregateException [1.1], [1.2], ...)
  ===== FIN DE ENTRADA =====
  ```
  Sin excepción original: solo cabecera, fecha, tipo, motivo y cierre (CL-12).

## 3. Persistencia
- Sin cambios en `AppDbContext`, sin migraciones, sin cambios de esquema.
- Sin paquetes NuGet nuevos en proyectos existentes. `SqlConnectionStringBuilder` llega por la dependencia ya presente de EF Core SqlServer.
- Proyectos de prueba nuevos con versiones ya usadas en el repo: autorizados (A-1, A-2).

## 4. Algoritmo de `EjecutarAsync`
**Regla de hilos**: el orquestador no usa `ConfigureAwait(false)` (vuelve al hilo de UI para `EstaDisponible`, `PrepararAviso` y `Presentar`); `RegistroErroresArchivo` usa `ConfigureAwait(false)` internamente.

**Fase A — comprobación (D-24: todo error aquí es fallo de conexión con motivo «excepción»)**
1. `cts` para el intento; `ctsRetardo` propio para el `Task.Delay(plazo, ctsRetardo.Token)`.
2. `intento = Task.Run(() => comprobador.ComprobarAsync(cts.Token))`.
3. `await Task.WhenAny(intento, retardo)`:
   - **gana el plazo**: `cancelacion = cts.CancelAsync()`; `Task.WhenAll(intento, cancelacion).ContinueWith(t => { observar t.Exception; cts.Dispose(); }, TaskScheduler.Default)`. `cts` se libera solo cuando terminan `intento` y la tarea de `CancelAsync()`, ambas observadas. El resultado tardío se descarta sin registrar (RF-10). Fallo con motivo `PlazoVencido`.
   - **gana el intento**: se cancela `ctsRetardo`; se lee el resultado (`Accesible` → éxito; `NoAccesible` → fallo `BaseNoAccesible`; excepción → fallo `Excepcion` con la excepción original); `cts` también se libera.
   - **valor no reconocido**: se crea `InvalidOperationException` con el valor recibido y se registra como excepción original (fallo `Excepcion`).
4. Todo el tramo va en `try/catch`: cualquier excepción → fallo `Excepcion`.
5. Si hubo fallo: `registroOk &= await RegistrarSeguro(entradaFallo)` **antes** de cualquier diálogo (RF-12).

**Fase B — preparación del aviso (posterior al resultado: excepciones = inesperadas)**
1. Si éxito → fin (RF-3).
2. Si `!pantalla.EstaDisponible` → fin sin diálogo ni acceso a controles (RF-16).
3. `try { preparado = pantalla.PrepararAviso(dtoConexion(registroOk)) }`
   `catch (ex1)` → `registroOk &= RegistrarSeguro(inesperada ex1)`; si `EstaDisponible`, `try { preparado = PrepararAviso(dtoGenerico(registroOk)) } catch (ex2) { RegistrarSeguro(inesperada ex2); sin diálogo (DT-5) }`.
   - Variante CL-16/CL-17: la excepción inesperada entre el resultado y la presentación siempre produce el genérico con la línea si falló cualquier registro previo.

**Fase C — presentación (RF-17)**
- `try { using (preparado) preparado.Presentar(); } catch (ex) { RegistrarSeguro(inesperada ex); sin otro diálogo }`.

**Barrera final**: `try/catch` exterior que registra y nunca relanza (RF-11, RF-13).
**`RegistrarSeguro`**: llama a `IRegistroErrores.RegistrarAsync`; si este lanzara pese al contrato, lo captura y devuelve `false` conservando la entrada original (RF-14).
**Cierre completo (RF-18)**: sin código específico; `Application.Exit()` termina el bucle y la tarea pendiente se abandona (verificación punto 26, H-6).
**Cierre de sesión**: no cancela la comprobación; el registro se completa (RF-16); `EstaDisponible` evita el diálogo.

## 5. Decisiones técnicas
| ID | Decisión | Alternativa descartada |
|---|---|---|
| DT-1 | Orquestador en CapaNegocio con la UI como `IPantallaAvisoInicio` (RF-2). | Lógica en `inicio_Shown`: viola RF-2 y no es testeable sin UI. |
| DT-2 | Sonda con `AppDbContext` dedicado por intento. | Resolver el `AppDbContext` scoped de la raíz (H-1). |
| DT-3 | Timeout real en capas: token en `OpenConnectionAsync(ct)` (H-2) + `Pooling=false` + `Connect Timeout=12` como respaldo. `DisposeAsync` en `finally` es solo liberación, no capa de corte (se ejecuta cuando `OpenConnectionAsync` ya volvió). | `Task.WaitAsync` (D-3: solo deja de esperar). |
| DT-4 | `Task.Run` para el intento (H-3, RNF-2). | `await` directo en el hilo de UI. |
| DT-5 | Si falla también preparar el genérico, se registra y no hay diálogo (**aprobada por el usuario**). | Recurrir a `MessageBox` nativo: segundo mecanismo no especificado. |
| DT-6 | `inicio_Shown` sin `catch`: `EjecutarAsync` nunca lanza. | `catch` en la UI: vuelve a ocultar errores. |
| DT-7 | Plazo inyectable solo vía constructor `internal` de pruebas unitarias (A-4). | Configuración en `appsettings`: cambia producción. |
| DT-8 | Registro en CapaNegocio (archivo local, sin NuGet). | Serilog/NLog: NuGet prohibido (RNF-4). |
| DT-9 | `SemaphoreSlim` estático + una escritura por entrada (RF-19). | `lock` sobre archivo / `FileShare.None` con reintentos: fallos espurios. |
| DT-10 | Fallo del registro simulado desde la prueba (carpeta inválida o doble de `IRegistroErrores`). | Interruptor en producción (prohibido, D-21). |
| DT-11 | Doble de `IPantallaAvisoInicio` solo en pruebas unitarias (A-3). Integración con `inicio` real. | Doble también en integración: contradice D-21. |
| DT-12 | `VerificacionConexionNegocio` registrado con fábrica explícita en `Program.cs`. | Dejar a DI elegir constructor: ambiguo y frágil. |
| DT-13 | `InternalsVisibleTo` declarado en `.cs` (sin editar `.csproj`). | Hacer públicos `TemaAplicacion`/constructor de pruebas. |

## 6. Estrategia de pruebas (`dotnet test`)

### 6.1 Reglas comunes
- Cada prueba declara precondiciones, acción, resultado esperado, tiempo máximo, criterio de éxito y criterio de fallo (`AGENTS.md`).
- Tiempo máximo por prueba: **30 s** salvo indicación. Al vencer: `FAILED — TIMEOUT / POSIBLE BLOQUEO`, diagnóstico obligatorio (10 puntos de `AGENTS.md`), sin reintentar.
- `dotnet build` se ejecuta aparte (≤ 120 s) y las pruebas con `--no-build`.
- Ejecución: `dotnet test <proyecto> --no-build --filter <grupo> --blame-hang --blame-hang-timeout 30s` (60 s para grupos con puntos 23/25) dentro de un **watchdog PowerShell externo ≤ 120 s por invocación**: `Start-Process -PassThru -RedirectStandardOutput <tmp>\out.txt -RedirectStandardError <tmp>\err.txt`; `WaitForExit(120000)`; al vencer, evidencia (PID, `Responding`, captura, ventana activa, logs de los archivos redirigidos, volcado de `--blame-hang`) **antes** de `taskkill /PID /T /F` y verificación de que el PID ya no existe.
- `[Fact(Timeout=...)]` no cuenta como timeout real.
- Detección de bloqueo: el watchdog (proceso distinto) detecta no finalización; `--blame-hang` produce el *dump* del hilo bloqueado; en puntos 21-26 la prueba además verifica `Responding` del hilo de UI mediante una acción con plazo de 10 s.

### 6.2 CapaNegocio.Tests (unitarias, puntos 1-19; doble de pantalla permitido, A-3)
- `SondaConexionBdTests`: `TcpListener` local que acepta y no responde; cancelación a los 200 ms; el intento termina (no solo deja de esperarse) en ≤ 5 s tras cancelar (punto 3, H-2). Máx. 30 s. Fallo: la tarea sigue viva o el socket sigue aceptado tras 5 s → H-2 refutada → PARAR.
- `TextosAvisoInicioTests` (17), `EnmascaradorSecretosTests` y `FormateadorEntradaRegistroTests` (8, 14), `RegistroErroresArchivoTests` (6, 9, 15, 16, 19 con `TimeProvider` propio en UTC-3 y concurrencia de entradas de ~1 MB en carpeta temporal única por prueba).
- `VerificacionConexionNegocioTests`: dobles de comprobador, registro y pantalla; plazo 200 ms; puntos 1, 2, 4, 5, 7, 10, 11, 12, 13, 18. Máx. 30 s cada una; detección de bloqueo por `--blame-hang` + watchdog.

### 6.3 CapaPresentacion.Tests (integración, puntos 20-25; `inicio` real, D-21)
- **Infraestructura** (fixture de colección, una por proceso): hilo STA dedicado con bucle de mensajes (`Application.Run(ApplicationContext)`); `ApplicationConfiguration.Initialize()` (o sus llamadas equivalentes si el generador no lo emite en un proyecto de biblioteca) y `TemaAplicacion.Configurar()` **una sola vez por proceso**; limpieza con `Dispose` (cierre y liberación de formularios, `ApplicationContext.ExitThread()`), **nunca `Application.Exit()`** en este proceso.
- Tras DU-1, cerrar `inicio` con `Close()` en el proceso de pruebas dispara `inicio_FormClosed` → `Application.Exit()`. La limpieza y las pruebas nunca invocan `Close()` sobre `inicio` en CapaPresentacion.Tests: se libera solo con `Dispose()`. La X se prueba únicamente en el arnés (T13).
- Todas las clases de prueba de UI pertenecen a la misma `[Collection]` que el fixture STA.
- DI de prueba que replica `Program.cs` con EF InMemory (usuario activo sembrado), comprobador simulado (controlable: éxito, fallo, sin terminar), registro real en carpeta temporal (o en carpeta inválida para el punto 24) y espía de `IVentanaAvisoInicio` que delega en `VentanaAvisoMaterial` real y anota el DTO recibido.
- Ingreso por el flujo real de `login` (controles localizados con `Controls.Find`, `PerformClick`); diálogos detectados con `Application.OpenForms`; sin paralelismo (`DisableTestParallelization`).
- Grupos de ejecución (cada uno ≤ 120 s de watchdog): `Infraestructura+Punto21`, `Punto20`, `Puntos22a24`, `Punto25`. Cada grupo de filtro se asigna con `[Trait("Grupo", "<nombre>")]` y se invoca `--filter Grupo=<nombre>`. T13 usa un filtro por variante (X y SALIR), cada una en su propio watchdog de 120 s.

### 6.4 Punto 26 (cierre completo, proceso aislado)
- `CapaPresentacion.ArnesPruebas` (WinExe) replica la composición de producción: `Application.Run(login)`; `inicio` creado por la fábrica tras el ingreso; X = `inicio.Close()` sobre ese `inicio`; SALIR = flujo del diálogo «Salir del sistema». Comprobador simulado que nunca termina.
- SALIR muestra antes el `MaterialDialog` «Salir del sistema» (`inicio.cs:396-405`). `SALIDA_PEDIDA` se emite al confirmar SALIR, y `DIALOGO` cuenta solo los formularios modales que aparezcan después de `SALIDA_PEDIDA`. Se fija una sola variante de SALIR: el flujo del diálogo, salvo imposibilidad técnica documentada antes de implementar.
- Protocolo por consola: `LISTO` (inicio mostrado), `SALIDA_PEDIDA` (con marca de tiempo; en SALIR, al confirmar el diálogo), `DIALOGO` (si aparece algún formulario modal después de `SALIDA_PEDIDA`).
- `CierreCompletoTests` (en CapaPresentacion.Tests) lanza el arnés con watchdog: éxito = proceso terminado ≤ 10 s desde `SALIDA_PEDIDA` y sin `DIALOGO`; plazo total de la prueba 60 s. Vigila H-6 (continuación con `Post` tras `Application.Run`).

### 6.5 Estática y manual
- Búsqueda estática (§7 de la spec) en T14.
- Prueba manual 1 (BD disponible) en T14, con watchdog sobre PID y `Responding`.

## 7. Trazabilidad
| Requisito | Componente | Prueba |
|---|---|---|
| RF-1 | `inicio_Shown` | 1, 20 |
| RF-2, RNF-1 | `VerificacionConexionNegocio`, `TextosAvisoInicio`, `inicio.cs` sin `AppDbContext` | 17, estática |
| RF-3 | Fase B paso 1 | 1, 5 |
| RF-4, RF-6 | Fase A, `RegistroErroresArchivo` | 2, 18, 22 |
| RF-5 | Fases B/C, `inicio` | 5, 22, 24 |
| RF-7, RF-8 | `RegistroErroresArchivo` | 6 |
| RF-9 | Fase A, `SondaConexionBd` | 3, 23 |
| RF-10 | Fase A (resultado tardío) | 4, 23 |
| RF-11 | Fases B/C, barrera | 5, 13, 18 |
| RF-12–RF-14 | `RegistrarSeguro`, `AvisoInicioDto.IncluyeLineaRegistro` | 7, 13, 24 |
| RF-15 | `EnmascaradorSecretos`, `FormateadorEntradaRegistro` | 8, 14 |
| RF-16 | `EstaDisponible`, `_cerrandose` | 10, 11, 25 |
| RF-17 | Fase C | 12 |
| RF-18 | DU-1 (`FormClosed`), SALIR | 26 |
| RF-19 | `SemaphoreSlim` estático | 19 |
| RNF-2 | `Task.Run` | 21 |
| RNF-3 | §6 | 1-26 |
| RNF-4 | §3 | estática |

## 8. Decisiones del usuario sobre el plan (2026-11-01)
Texto del usuario: «Apruebo: 1a, 2 sí, 3 sí, 4 sí, 5 sí, 6 sí, 7a. Para A-3, la simulación de pantalla debe limitarse a las pruebas unitarias. Las pruebas de integración deben seguir utilizando la pantalla de inicio real, conforme a D-21. Aplicá todas las correcciones técnicas de Reviewer y escribí plan.md y tasks.md.»
- **DU-1 (1a)**: en el constructor de `inicio` se suscribe `FormClosed += inicio_FormClosed` para que la X cierre la aplicación (RF-18). Se verifica (T8 y puntos 20/25) que CERRAR SESIÓN no dispara `FormClosed` ni `Application.Exit()` (H-9); si lo dispara, PARAR y escalar.
- **A-1 (resuelta, aprobada)**: crear `CapaPresentacion.Tests` (`net10.0-windows`, `UseWindowsForms`) con Microsoft.NET.Test.Sdk 17.14.1, xunit 2.9.3, xunit.runner.visualstudio 3.1.4, Microsoft.EntityFrameworkCore.InMemory 10.0.11; referencias a CapaPresentacion, CapaNegocio y CapaDatos; agregado a `SistemaVentaStock.slnx`. El usuario confirma que no contradice RNF-4.
- **A-2 (resuelta, aprobada)**: crear `CapaPresentacion.ArnesPruebas` (WinExe `net10.0-windows`, EF InMemory 10.0.11, referencias a las 3 capas) en el `.slnx`.
- **A-3 (resuelta, aprobada con límite)**: doble de `IPantallaAvisoInicio` solo en CapaNegocio.Tests. Puntos 20-26 usan la pantalla de inicio real (D-21).
- **A-4 (resuelta, aprobada)**: plazo inyectable solo para pruebas unitarias (constructor `internal`, DT-7/DT-12).
- **A-5 (resuelta, 7a)**: se acepta «ACEPTAR» en mayúsculas en pantalla; el DTO conserva «Aceptar» (D-23). H-4 es comportamiento aceptado: solo se registra la observación visual.
- **DT-5 (aprobada)**: si falla también la preparación del aviso genérico, se registra y no se muestra diálogo.
- D-A (pendiente de aprobación del usuario junto con el plan): 14 tareas en dos bloques (T1-T8 núcleo; T9-T13 pruebas de integración; T14 validación) con punto de control entre bloques, sin dividir la spec, porque RF-16 y RF-18 requieren ambos bloques.

## 9. Riesgos
- H-2 refutada → RF-9 no cumplible con el diseño actual: PARAR y escalar (T1).
- H-5 (recorte de la segunda línea) → PARAR y escalar (T8/T11).
- H-6 residual (continuación con `Post` tras `Application.Run`) → vigilar en el punto 26 (T13).
- H-9 (CERRAR SESIÓN dispara `FormClosed`) → PARAR y escalar (T8).
- Pruebas de UI en STA pueden ser frágiles en CI sin escritorio interactivo: se ejecutan solo en sesión interactiva local.
