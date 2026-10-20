description: SDD - Implementa UNA tarea específica (uso: /sdd-implement 001-nombre T1)
agent: build

Implementa SOLO la tarea $2 de specs/$1/tasks.md, siguiendo specs/$1/plan.md y docs/constitution.md.

1. Escribe primero las pruebas o la lógica base.
2. Escribe el código necesario en la capa correspondiente hasta que funcione.
3. Ejecuta `dotnet build` y `dotnet test` y muéstrame el resultado.
4. Marca $2 como hecha en tasks.md e indica qué RF cubre.

Después PÁRATE. No empieces la siguiente tarea.