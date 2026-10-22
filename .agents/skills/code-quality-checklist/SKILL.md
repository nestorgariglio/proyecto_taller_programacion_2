---
name: code-quality-checklist
description: Úsala siempre que vayas a escribir, modificar, refactorizar o verificar código C# en las capas del proyecto.
---
## Reglas
- Respetar estrictamente la jerarquía: Presentación → Negocio → Datos → Entidad.
- Toda operación de I/O a la base de datos debe ser asíncrona (`async/await`).
- Bajas lógicas obligatorias mediante `Estado = false`.
- Las contraseñas deben ser procesadas con BCrypt únicamente en `CapaNegocio`.
- Las consultas de solo lectura en `CapaDatos` deben incluir `.AsNoTracking()`.

## Checklist de revisión
- [ ] ¿Se respeta el aislamiento de 4 capas? (La UI jamás accede a `AppDbContext` ni ejecuta SQL directo).
- [ ] ¿Se filtran las consultas de lectura por activos (`Estado == true`)?
- [ ] ¿Las consultas de lectura utilizan `.AsNoTracking()` para optimizar rendimiento?
- [ ] ¿Se utilizan DTOs sanitizados (sin hashes ni contraseñas) para comunicar `CapaNegocio` y `CapaPresentacion`?
- [ ] ¿Las validaciones de unicidad (DNI/Correo/Código) se ejecutan en `CapaNegocio` antes de guardar?
- [ ] ¿Se utilizan métodos asíncronos (`ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync`) en la persistencia?
- [ ] ¿El comando `dotnet build` compila sin advertencias ni errores?
- [ ] ¿Los tests en `dotnet test` ejecutan en verde?

## Al terminar
Indica explícitamente qué puntos del checklist has comprobado, qué pruebas o comandos ejecutaste (`dotnet build` / `dotnet test`) y confirma si es necesario actualizar `MEMORY.md`.