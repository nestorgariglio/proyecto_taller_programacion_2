---
description: Prepara la base de datos de desarrollo (migraciones EF Core + seed)
agent: build
---

Ejecutá el script `scripts/preparar-bd.ps1` desde la raíz del repositorio para preparar la base de datos de desarrollo.

Pasos:
1. Corré: `powershell -ExecutionPolicy Bypass -File .\scripts\preparar-bd.ps1`
2. Mostrame la salida completa y verificá que termine con código 0 y con la verificación "Categorías: 6 (activas: 5) | Productos: 12 (activos: 12)".
3. Si falla, revisá `scripts/README-preparar-bd.md` y explicame la causa y la solución.

Si te pasan argumentos (`$ARGUMENTS`), usalos para el modo del script (por ejemplo `-SoloDatos` para omitir las migraciones y recargar solo los datos).
