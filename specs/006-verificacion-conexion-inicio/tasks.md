# Tareas 006 — Verificación de conexión en el inicio
Estado: borrador (pendiente de aprobación del usuario)
Plan: `specs/006-verificacion-conexion-inicio/plan.md`

## Reglas de ejecución
- Una tarea a la vez; pruebas primero en rojo; al terminar, marcar `[x]` y **PARAR** hasta autorización.
- Solo se editan los archivos autorizados de cada tarea.
- `dotnet build` aparte, ≤ 120 s. `dotnet test --no-build --filter <grupo> --blame-hang --blame-hang-timeout 30s` (60 s en grupos con puntos 23/25) dentro de watchdog PowerShell externo ≤ 120 s por invocación, con `Start-Process -PassThru -RedirectStandardOutput/-RedirectStandardError` a archivos temporales (plan §6.1).
- Cada prueba ≤ 30 s salvo indicación. Al vencer: `FAILED — TIMEOUT / POSIBLE BLOQUEO`, evidencia antes de terminar el proceso, sin reintentar.
- Sin migraciones ni NuGet en proyectos existentes. Los proyectos nuevos de T9 y T13 están autorizados (A-1, A-2, 2026-11-01).

## Nota sobre el número de tareas (14 > 10)
La skill SDD pide proponer dividir la spec si se superan 10 tareas. La división de T6 y T8 originales viene de la revisión (tamaño ≤ 30 min). Se propone **no crear otra spec** y agrupar en dos bloques con punto de control entre ellos (D-A del plan, pendiente de aprobación del usuario):
- **Bloque A — núcleo (T1-T8)**: capas Datos, Negocio y Presentación con pruebas unitarias 1-19.
- **Bloque B — integración y cierre (T9-T14)**: pruebas 20-26 con la pantalla real, validación final.
Los RF son indivisibles entre bloques (RF-16/RF-18 exigen ambos), por eso una spec separada duplicaría requisitos.

## Bloque A — núcleo

- [ ] **T1. Sonda de conexión en CapaDatos.** RF-2, RF-9
  - Archivos: `CapaDatos/Diagnostico/SondaConexionBd.cs`, `CapaNegocio.Tests/Diagnostico/SondaConexionBdTests.cs`.
  - Contenido: `ConstruirCadena` con `SqlConnectionStringBuilder` (`Pooling=false`, `ConnectTimeout=12`); `AbrirYCerrarAsync(ct)` con `AppDbContext` dedicado; `CloseConnectionAsync`/`DisposeAsync` en `finally` solo como liberación.
  - Pruebas: punto 3 (`TcpListener` mudo, cancelación a 200 ms, intento terminado ≤ 5 s); `ConstruirCadena` conserva servidor/BD y fija los dos parámetros.
  - Hecho cuando: build limpio y pruebas en verde. **Si la prueba refuta H-2, PARAR y escalar al usuario; no se da por cumplido RF-9 solo con `Connect Timeout`.**
  - Tiempo máximo: 30 s por prueba; watchdog 120 s.

- [ ] **T2. Enums, contratos, DTO y textos.** RF-2, RF-15
  - Archivos: `CapaNegocio/Enums/EstadoConexion.cs`, `CapaNegocio/Enums/MotivoFalloConexion.cs`, `CapaNegocio/Enums/TipoEntradaRegistro.cs`, `CapaNegocio/Enums/TipoAvisoInicio.cs`, `CapaNegocio/Diagnostico/IComprobadorConexion.cs`, `CapaNegocio/Diagnostico/IRegistroErrores.cs`, `CapaNegocio/Diagnostico/IPantallaAvisoInicio.cs`, `CapaNegocio/Diagnostico/IAvisoPreparado.cs`, `CapaNegocio/Diagnostico/EntradaRegistro.cs`, `CapaNegocio/Diagnostico/TextosAvisoInicio.cs`, `CapaNegocio/DTOs/Diagnostico/AvisoInicioDto.cs`, `CapaNegocio.Tests/Diagnostico/TextosAvisoInicioTests.cs`.
  - Hecho cuando: build limpio; punto 17 (textos literales D-23, línea adicional con `Environment.NewLine`) en verde.
  - Tiempo máximo: 30 s por prueba; watchdog 120 s.

- [ ] **T3. Enmascarador y formateador.** RF-6, RF-11, RF-15
  - Archivos: `CapaNegocio/Diagnostico/EnmascaradorSecretos.cs`, `CapaNegocio/Diagnostico/FormateadorEntradaRegistro.cs`, `CapaNegocio.Tests/Diagnostico/EnmascaradorSecretosTests.cs`, `CapaNegocio.Tests/Diagnostico/FormateadorEntradaRegistroTests.cs`.
  - Contenido: oculta cadena configurada, cadena derivada de la sonda y contraseña; ignora nulos/vacíos; formato de plan §2; anti-ciclos; `AggregateException` completa.
  - Hecho cuando: puntos 8 y 14, CL-12 (entrada sin excepción) y caso de secreto vacío en verde.
  - Tiempo máximo: 30 s por prueba; watchdog 120 s.

- [ ] **T4. Registro en archivo.** RF-6, RF-7, RF-8, RF-13, RF-19
  - Archivos: `CapaNegocio/Diagnostico/RegistroErroresArchivo.cs`, `CapaNegocio.Tests/Diagnostico/RegistroErroresArchivoTests.cs`.
  - Hecho cuando: puntos 6, 9, 15, 16 y 19 en verde, y carpeta inválida → `false` sin excepción.
  - Tiempo máximo: 30 s por prueba (concurrencia ~1 MB incluida); watchdog 120 s.

- [ ] **T5. Comprobador y fase A del orquestador.** RF-1, RF-4, RF-6, RF-9, RF-10
  - Archivos: `CapaNegocio/Diagnostico/ComprobadorConexionBd.cs`, `CapaNegocio/Diagnostico/VerificacionConexionNegocio.cs` (incluye `[assembly: InternalsVisibleTo("CapaNegocio.Tests")]` y constructor `internal` con plazo), `CapaNegocio.Tests/Diagnostico/VerificacionConexionNegocioTests.cs`.
  - Contenido: plan §4 fase A (liberación de `cts` tras `intento` + `CancelAsync()` observados; valor no reconocido → `InvalidOperationException` con el valor como excepción original).
  - Hecho cuando: en verde, limitado a **clasificación y entradas de registro**: punto 1 (una comprobación, sin entrada), punto 2 (tres motivos y contenido de la entrada), punto 4 (una entrada y resultado tardío descartado) y punto 18 en su parte de clasificación/registro. Las comprobaciones de aviso pasan a T6/T7.
  - Tiempo máximo: 30 s por prueba (plazo inyectado 200 ms); watchdog 120 s.

- [ ] **T6. Fases B/C, parte 1: aviso, fallo del registro y pantalla no disponible.** RF-3, RF-4, RF-5, RF-11, RF-12, RF-13, RF-14, RF-16
  - Archivos: `CapaNegocio/Diagnostico/VerificacionConexionNegocio.cs`, `CapaNegocio.Tests/Diagnostico/VerificacionConexionNegocioTests.cs`.
  - Hecho cuando: puntos 5, 7, 10 y 11 en verde con doble de pantalla (A-3), incluido el orden «registro antes del diálogo» y DT-5.
  - Tiempo máximo: 30 s por prueba; watchdog 120 s.

- [ ] **T7. Fases B/C, parte 2: presentación, combinaciones y textos.** RF-2, RF-10, RF-11, RF-12, RF-17
  - Archivos: `CapaNegocio/Diagnostico/VerificacionConexionNegocio.cs`, `CapaNegocio.Tests/Diagnostico/VerificacionConexionNegocioTests.cs`.
  - Hecho cuando: puntos 12, 13 (cuatro casos), 17 (DTO entregado a la pantalla) y 18 en su parte de aviso en verde; suite completa de CapaNegocio.Tests sin regresiones. La suite completa se ejecuta por grupos `--filter` de modo que cada invocación quede ≤ 120 s bajo watchdog; si una invocación supera el límite, se registra como FAILED — TIMEOUT y se divide el grupo (no se amplía el límite) (H-10).
  - Tiempo máximo: 30 s por prueba; watchdog 120 s.

- [ ] **T8. Presentación: `inicio`, ventana de aviso y DI.** RF-1, RF-2, RF-16, RF-18, RNF-1, RNF-2
  - Archivos: `CapaPresentacion/Inicio/inicio.cs`, `CapaPresentacion/Inicio/IVentanaAvisoInicio.cs`, `CapaPresentacion/Inicio/VentanaAvisoMaterial.cs`, `CapaPresentacion/Program.cs`. Sin Designer ni `.csproj`.
  - Contenido: quitar `AppDbContext` y el `catch` vacío; **DU-1: `FormClosed += inicio_FormClosed;` en el constructor**; `_cerrandose` con `if (!e.Cancel) _cerrandose = true;` y en CERRAR SESIÓN; `IVentanaAvisoInicio.Preparar(Form dueño, AvisoInicioDto aviso)`; DI con fábricas explícitas; `InternalsVisibleTo` hacia `CapaPresentacion.Tests` y `CapaPresentacion.ArnesPruebas`.
  - Verificaciones: (a) firma de `MaterialDialog` recibe `Form`; (b) H-9: inspección de `login.cs:91-101` y de `Form.Dispose` — CERRAR SESIÓN no dispara `FormClosed`/`Application.Exit()`; si lo dispara, PARAR y escalar (confirmación automática en T10/T12); (c) H-4 aceptada: solo se anota la observación; (d) H-5: inspección de `MaterialDialog` 2.3.1 (texto multilínea y alto del diálogo); si recorta, PARAR y escalar (captura confirmatoria en T11).
  - Hecho cuando: build limpio, suite existente en verde y búsqueda estática sin `AppDbContext`/`CapaDatos` fuera de `Program.cs`.
  - Tiempo máximo: build ≤ 120 s; watchdog 120 s por grupo de pruebas.

**PUNTO DE CONTROL: al terminar T8, PARAR y presentar resultado al usuario antes de T9.**

## Bloque B — integración y cierre (pantalla de inicio real, D-21)

Reglas comunes de T9-T13: cada grupo de filtro se asigna con `[Trait("Grupo", "<nombre>")]` y se invoca `--filter Grupo=<nombre>`. Todas las clases de prueba de UI pertenecen a la misma `[Collection]` que el fixture STA.

- [ ] **T9. Proyecto CapaPresentacion.Tests, infraestructura STA y punto 21.** RNF-2, RNF-3 — DESBLOQUEADA (A-1 autorizada 2026-11-01)
  - Archivos: `CapaPresentacion.Tests/CapaPresentacion.Tests.csproj`, `CapaPresentacion.Tests/Infraestructura/UiStaFixture.cs`, `CapaPresentacion.Tests/Infraestructura/ComposicionPrueba.cs`, `CapaPresentacion.Tests/Infraestructura/ComprobadorSimulado.cs`, `CapaPresentacion.Tests/Infraestructura/VentanaAvisoEspia.cs`, `CapaPresentacion.Tests/RespuestaUiTests.cs`, `SistemaVentaStock.slnx`.
  - Contenido: fixture único por proceso (`ApplicationConfiguration.Initialize()` y `TemaAplicacion.Configurar()` una vez; limpieza con `Dispose`, nunca `Application.Exit()`); sin paralelismo. Tras DU-1, cerrar `inicio` con `Close()` en el proceso de pruebas dispara `inicio_FormClosed` → `Application.Exit()`. La limpieza y las pruebas nunca invocan `Close()` sobre `inicio` en CapaPresentacion.Tests: se libera solo con `Dispose()`. La X se prueba únicamente en el arnés (T13). El fixture registra un manejador de `Application.ApplicationExit` para detectar llamadas a `Application.Exit()`.
  - Hecho cuando: punto 21 en verde (acción procesada ≤ 10 s con comprobación sin terminar).
  - Tiempo máximo: 30 s por prueba; `--filter Grupo=Infraestructura+Punto21` (`[Trait("Grupo", "Infraestructura+Punto21")]`), watchdog 120 s.

- [ ] **T10. Punto 20: una comprobación por apertura.** RF-1, CL-11 — DESBLOQUEADA
  - Archivos: `CapaPresentacion.Tests/AperturaInicioTests.cs` (clase en la misma `[Collection]` que el fixture STA).
  - Hecho cuando: dos ingresos con CERRAR SESIÓN intermedio producen exactamente dos comprobaciones y el proceso de pruebas no recibe `Application.Exit()` (H-9). La ausencia de `Application.Exit()` se detecta con un manejador de `Application.ApplicationExit` registrado por el fixture, y además `login` sigue sin liberar y el bucle STA sigue vivo.
  - Tiempo máximo: 30 s; `--filter Grupo=Punto20` (`[Trait("Grupo", "Punto20")]`), watchdog 120 s.

- [ ] **T11. Puntos 22-24: avisos con la pantalla real.** RF-4, RF-5, RF-6, RF-9, RF-10, RF-12, RF-13 — DESBLOQUEADA
  - Archivos: `CapaPresentacion.Tests/AvisoInicioIntegracionTests.cs` (clase en la misma `[Collection]` que el fixture STA).
  - Hecho cuando: 22 (textos D-23, pantalla usable tras aceptar, entrada en archivo), 23 (aviso ≤ 12 s desde el inicio del intento, una entrada «plazo de 10 s vencido») y 24 (línea adicional, pantalla usable) en verde; captura del diálogo del punto 24 confirma H-5 (si recorta, PARAR y escalar) y se anota H-4 sin bloquear.
  - Tiempo máximo: 30 s por prueba; `--filter Grupo=Puntos22a24` (`[Trait("Grupo", "Puntos22a24")]`), `--blame-hang-timeout 60s`, watchdog 120 s.

- [ ] **T12. Punto 25: cierre de sesión durante la comprobación.** RF-16, CL-10 — DESBLOQUEADA
  - Archivos: `CapaPresentacion.Tests/CierreSesionIntegracionTests.cs` (clase en la misma `[Collection]` que el fixture STA).
  - Hecho cuando: sin diálogo, entrada en el archivo del día, vuelta a `login` y aplicación viva (sin `FormClosed`/`Application.Exit()`, H-8/H-9). La ausencia de `Application.Exit()` se detecta con un manejador de `Application.ApplicationExit` registrado por el fixture, y además `login` sigue sin liberar y el bucle STA sigue vivo.
  - Tiempo máximo: 30 s; `--filter Grupo=Punto25` (`[Trait("Grupo", "Punto25")]`), `--blame-hang-timeout 60s`, watchdog 120 s.

- [ ] **T13. Arnés aislado y punto 26: cierre completo.** RF-18, CL-13 — DESBLOQUEADA (A-2 autorizada 2026-11-01)
  - Archivos: `CapaPresentacion.ArnesPruebas/CapaPresentacion.ArnesPruebas.csproj`, `CapaPresentacion.ArnesPruebas/Program.cs`, `CapaPresentacion.Tests/CierreCompletoTests.cs`, `SistemaVentaStock.slnx`.
  - Contenido: plan §6.4 (`Application.Run(login)`, `inicio` por la fábrica, X = `inicio.Close()`, SALIR = flujo del diálogo «Salir del sistema»; protocolo `LISTO`/`SALIDA_PEDIDA`/`DIALOGO`). SALIR muestra antes el `MaterialDialog` «Salir del sistema» (`inicio.cs:396-405`). `SALIDA_PEDIDA` se emite al confirmar SALIR, y `DIALOGO` cuenta solo los formularios modales que aparezcan después de `SALIDA_PEDIDA`. Se fija una sola variante de SALIR: el flujo del diálogo, salvo imposibilidad técnica documentada antes de implementar.
  - Hecho cuando: X y SALIR terminan el proceso ≤ 10 s desde `SALIDA_PEDIDA`, sin `DIALOGO` posterior a `SALIDA_PEDIDA`; si el watchdog no logra terminar el proceso, se informa como fallo del mecanismo. Vigilar H-6.
  - Tiempo máximo: 60 s por variante; un filtro por variante (`--filter Grupo=Punto26X` y `--filter Grupo=Punto26Salir`, con su `[Trait("Grupo", ...)]`), cada una en su propio watchdog de 120 s.

- [ ] **T14. Validación final.** Todos los RF/RNF
  - Archivos: `specs/006-verificacion-conexion-inicio/tasks.md` (solo marcas).
  - Hecho cuando: build limpio; todas las suites en verde por grupos bajo watchdog (la suite completa de CapaNegocio.Tests se ejecuta por grupos `--filter` de modo que cada invocación quede ≤ 120 s bajo watchdog; si una invocación supera el límite, se registra como FAILED — TIMEOUT y se divide el grupo, no se amplía el límite; H-10); búsqueda estática de §7 de la spec con única coincidencia `Program.cs`; verificación de RNF-4: sin migraciones nuevas, sin `PackageReference` nuevos en los proyectos existentes y sin cambios en `AppDbContext`; prueba manual 1 superada (arranque ≤ 20 s, escenario ≤ 120 s, watchdog sobre PID y `Responding`); observaciones de H-4/H-5/H-6 registradas.
  - Tiempo máximo: build ≤ 120 s; cada invocación de pruebas ≤ 120 s; experimento completo ≤ 8 min.
