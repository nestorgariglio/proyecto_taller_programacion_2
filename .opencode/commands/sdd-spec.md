description: SDD - Entrevista y genera la especificación funcional de cualquier feature
agent: plan

NO escribas código en ningún momento. Lee docs/constitution.md y MEMORY.md.

Carpeta de la spec: specs/$1/
Idea inicial: $ARGUMENTS

Tu trabajo:
1. Hazme preguntas de UNA en UNA para eliminar ambigüedades (casos límite, errores, qué queda fuera de esta versión). Máximo 5 preguntas.
2. Con mis respuestas, genera specs/$1/spec.md siguiendo la estructura SDD, con los requisitos en notación EARS (RF-x) y "Estado: borrador".
3. Solo define el QUÉ y el POR QUÉ: nada de stack, arquitectura ni nombres de archivos.