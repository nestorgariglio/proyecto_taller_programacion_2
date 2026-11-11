# Constitución del Sistema Comercial G70

Principios innegociables. Toda spec, plan, comando y tarea debe cumplirlos.

1. **Aislamiento Estricto de 4 Capas**:
   - `CapaEntidad`: Modelos puros.
   - `CapaDatos`: Acceso exclusivo a BD vía `AppDbContext`.
   - `CapaNegocio`: Lógica, reglas de negocio y DTOs.
   - `CapaPresentacion`: Interfaz WinForms / MaterialSkin. La UI nunca ejecuta SQL ni toca EF Core.

2. **La Spec Manda y Prohibido Alucinar**:
   - Funcionalidades nuevas y cambios amplios requieren spec y plan aprobados antes de implementar.
   - Las correcciones puntuales pueden ejecutarse sin SDD cuando el usuario haya autorizado expresamente el alcance, el comportamiento esperado y el criterio de aceptación.
   - Ningún agente inventa reglas de negocio ni amplía el alcance aprobado.
   - Si falta una definición funcional o existe una ambigüedad que afecte el resultado, el agente debe detener esa parte y consultar al usuario.

3. **Bajas Lógicas y Persistencia Segura**:
   - Prohibido el borrado físico (`DELETE`) en tablas operativas (`Estado = false`).
   - Contraseñas siempre con BCrypt.
   - Hashes nunca viajan en DTOs de lectura.

4. **Tests como Puerta de Entrada**:
   - Las reglas de `CapaNegocio` deben verificarse con pruebas automáticas.
   - Prohibido dar por terminada una tarea con pruebas pertinentes en rojo o sin compilar.
   - Los tests automatizados no sustituyen las pruebas funcionales, de integración o visuales que exijan los criterios de aceptación.

5. **Protección de Datos e Integridad**:
   - Las transacciones compuestas (ej. Venta + Stock) deben usar transacciones explícitas de EF Core (`BeginTransactionAsync` / `CommitAsync` / `RollbackAsync`).

6. **Idioma**:
   - Código, clases y variables en C# en Español / PascalCase.
   - Mensajes al usuario, interfaz y documentación en Español.
