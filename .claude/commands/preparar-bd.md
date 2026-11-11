---
description: Prepara la base de datos de desarrollo mediante migraciones EF Core y carga de datos iniciales (seed).
argument-hint: "[-SoloDatos]"
disable-model-invocation: true
---

Ejecutá el script `scripts/preparar-bd.ps1` desde la raíz del repositorio para preparar la base de datos de desarrollo.

## Pasos

1. Verificá que estés ubicado en la raíz del repositorio y que exista `scripts/preparar-bd.ps1`.

2. Ejecutá desde PowerShell, con stdout y stderr redirigidos al log desde el inicio del comando (AGENTS.md §6), y guardá el código de salida:

   ```powershell
   New-Item -ItemType Directory -Force -Path "$env:TEMP\G70" | Out-Null
   cmd /c "powershell -ExecutionPolicy Bypass -File .\scripts\preparar-bd.ps1 $ARGUMENTS > %TEMP%\G70\preparar-bd.log 2>&1"
   "EXIT=$LASTEXITCODE"
   ```

3. No pegues el log completo. Extraé solo lo relevante y mostrame como máximo 20 líneas, el código de salida y la ruta del log:

   ```powershell
   Get-Content "$env:TEMP\G70\preparar-bd.log" -Encoding UTF8 | Select-String -Pattern 'ERROR|FALLO|ESTADO FINAL|\[OK\]|\[ERROR' | Select-Object -Last 20
   ```

4. Confirmá código de salida 0, que aparezca `ESTADO FINAL: ÉXITO` y que todas las líneas de la verificación de datos (paso 7 del script) muestren `[OK]`. Las cantidades esperadas están en `scripts/README-preparar-bd.md`. No copies cadenas de conexión ni credenciales.

5. Si falla cualquiera de las verificaciones:
   - No declares exitosa la preparación.
   - Revisá `scripts/README-preparar-bd.md`.
   - Explicame la causa del error y la solución correspondiente.
   - No vuelvas a ejecutar el script automáticamente.

## Argumentos

Los argumentos recibidos después del comando están disponibles mediante `$ARGUMENTS`.

- `/preparar-bd`: ejecuta el procedimiento completo, incluyendo migraciones EF Core y carga de datos iniciales.
- `/preparar-bd -SoloDatos`: pasa `-SoloDatos` al script. No aplica migraciones: solo recarga los datos si el esquema ya está al día; si no lo está, el script se detiene.

Pasá los argumentos recibidos al script sin modificarlos ni interpretarlos.

## Resultado final

Informá:
- Código de salida.
- Resultado de las migraciones, si corresponden.
- Resultado de la carga de datos.
- Resultado de la verificación de categorías y productos.
- Estado final: **ÉXITO** o **ERROR**.
- Ruta del log completo.

No declares éxito si no se cumplen ambas condiciones obligatorias: código de salida 0 y `ESTADO FINAL: ÉXITO` con la verificación de datos en `[OK]`.