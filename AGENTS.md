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
