# Constitución del Sistema Comercial G70
Principios innegociables. Toda spec, plan, comando y tarea debe cumplirlos.

1. **Aislamiento Estricto de 4 Capas**: 
   - `CapaEntidad`: Modelos puros.
   - `CapaDatos`: Acceso exclusivo a BD vía `AppDbContext`.
   - `CapaNegocio`: Lógica, reglas de negocio y DTOs.
   - `CapaPresentacion`: Interfaz WinForms / MaterialSkin.

2. **La Spec Manda y Prohibido Alucinar**: Nada se implementa si no está en la spec activa o en el plan aprobado[cite: 32, 36]. Si falta una regla de negocio o hay ambigüedad, el agente debe parar y preguntar.

3. **Bajas Lógicas y Persistencia Segura**: Prohibido el borrado físico (`DELETE`) en tablas operativas (`Estado = false`). Contraseñas siempre con BCrypt.

4. **Tests como Puerta de Entrada**: Las reglas de `CapaNegocio` deben verificarse con pruebas automáticas (`dotnet test`). Prohibido dar por terminada una tarea con pruebas en rojo.

5. **Protección de Datos e Integridad**: Las transacciones compuestas (ej. Venta + Stock) deben usar transacciones explícitas de EF Core.

6. **Idioma**: Código, clases y variables en C# en **Español / PascalCase**[cite: 32]. Mensajes al usuario en **Español**.