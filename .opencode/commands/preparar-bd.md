---
description: Prepara la base de datos de desarrollo (migraciones EF Core + seed)
agent: build
---

Ejecutá el script `scripts/preparar-bd.ps1` desde la raíz del repositorio para preparar la base de datos de desarrollo.

Pasos:
1. Corré, desde PowerShell y con stdout/stderr redirigidos al log desde el inicio del comando (AGENTS.md §6):
   `New-Item -ItemType Directory -Force -Path "$env:TEMP\G70" | Out-Null; cmd /c "powershell -ExecutionPolicy Bypass -File .\scripts\preparar-bd.ps1 $ARGUMENTS > %TEMP%\G70\preparar-bd.log 2>&1"; "EXIT=$LASTEXITCODE"`
2. No pegues el log completo. Extraé solo lo relevante con `Get-Content "$env:TEMP\G70\preparar-bd.log" -Encoding UTF8 | Select-String -Pattern 'ERROR|FALLO|ESTADO FINAL|\[OK\]|\[ERROR' | Select-Object -Last 20` y mostrame como máximo 20 líneas relevantes, el código de salida y la ruta del log (`%TEMP%\G70\preparar-bd.log`). Verificá que el código sea 0, que aparezca `ESTADO FINAL: ÉXITO` y que todas las líneas de verificación de datos muestren `[OK]` (cantidades esperadas en `scripts/README-preparar-bd.md`). No copies cadenas de conexión ni credenciales.
3. Si falla, revisá `scripts/README-preparar-bd.md` y explicame la causa y la solución.

Si te pasan argumentos (`$ARGUMENTS`), usalos para el modo del script (por ejemplo `-SoloDatos`, que no aplica migraciones y se detiene si el esquema está atrasado).
