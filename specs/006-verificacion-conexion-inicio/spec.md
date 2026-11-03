# Spec 006 — Verificación de conexión en el inicio
Estado: aprobada (2026-11-01)

## 1. Origen, contexto y objetivo
**Origen**: petición del usuario (2026-11-01) para corregir un incumplimiento de arquitectura que encontró `@reviewer` en la pantalla de inicio. Incluye la decisión funcional del usuario «Aviso y seguir», que dice textualmente: si falla la conexión, se muestra un mensaje comprensible en español, la aplicación sigue abierta y funcionando como hasta ahora, se registra la excepción completa para diagnóstico y no se muestran stack traces, cadenas de conexión ni información técnica interna. Si la conexión funciona, no se muestra nada.

**Contexto**: al mostrarse la pantalla de inicio, la propia interfaz usa directamente el contexto de base de datos para comprobar la conexión. Esto incumple el principio 1 de la constitución (aislamiento estricto de 4 capas). Además, la excepción se descarta en un bloque de captura vacío. Por eso, si la conexión falla, el usuario no ve ningún aviso y no queda ningún rastro para diagnosticar el problema.

**Defectos del diagnóstico**
- Confirmados (verificados por `@reviewer` en el código):
  - la pantalla de inicio usa `AppDbContext` directamente;
  - la excepción de la comprobación se descarta en un bloque de captura vacío;
  - para llegar a la pantalla de inicio hay que iniciar sesión, y el inicio de sesión ya consulta la base de datos. Por eso los escenarios con la base no disponible no se pueden reproducir a mano (de ahí D-21);
  - X y SALIR cierran la aplicación por completo; CERRAR SESIÓN libera la pantalla de inicio y la aplicación sigue abierta (de ahí D-22).
- Hipótesis: ninguna.

**Objetivo**: la comprobación de conexión pasa a la lógica de negocio; la interfaz solo la pide y muestra el resultado. El comportamiento de inicio se mantiene igual y las excepciones dejan de ocultarse.

> Esta spec describe solo el QUÉ y el POR QUÉ. Dónde se ubica la comprobación, cómo se nombra, el formato exacto del registro y cómo se prueba se definen en `plan.md`. La spec nombra componentes concretos solo cuando hay una decisión explícita del usuario detrás: `AppDbContext` y `Program.cs` (D-5), la ruta `%LOCALAPPDATA%` (D-1) y `Task.WaitAsync` como ejemplo de lo que no basta (D-3). Por las observaciones técnicas de la revisión, también nombra `AggregateException` (definición de «Excepción completa», CL-5 y RNF-3) y el `.csproj` de CapaPresentacion (§7).

## 2. Actores y definiciones
**Actores**
- **Usuario autenticado** (Administrador, Encargado o Vendedor): entra en la pantalla de inicio y, si hay un fallo, recibe el aviso.
- **Desarrollador / soporte**: consulta el registro para diagnosticar el fallo.

**Definiciones**
- **Comprobación de conexión**: intento de abrir y cerrar una conexión con la base de datos configurada, sin leer ni escribir datos. Va desde que la pantalla de inicio la pide hasta que recibe su resultado.
- **Fallo de conexión**: la comprobación de conexión termina de una de estas tres formas:
  - (a) se produce una excepción en cualquier momento desde que la pantalla de inicio pide la comprobación hasta que recibe el resultado, venga de la conexión o de cualquier otra parte de la petición (D-24). Se exceptúa el fallo del registro, que se rige por RF-12 a RF-14;
  - (b) la comprobación indica, sin excepción, que la base de datos no es accesible;
  - (c) vence el plazo máximo de 10 s, contado desde el inicio del intento de conexión (D-3).
- **Resultado tardío**: lo que el intento cancelado produce después de vencido el plazo (éxito, fallo o excepción). No es un nuevo fallo de conexión ni una excepción inesperada. Se descarta sin registrarlo y sin avisar (RF-10, D-3).
- **Motivo del fallo**: cuál de las tres formas anteriores ocurrió: «excepción», «base de datos no accesible» o «plazo de 10 s vencido».
- **Excepción inesperada**: excepción que ocurre después de que la pantalla de inicio recibe el resultado de la comprobación, mientras se trata esa comprobación. Por ejemplo, al procesar el resultado, al preparar o presentar el aviso, mientras el aviso está visible o después de cerrarlo. No es un fallo del registro ni un resultado tardío (D-24). El tratamiento termina cuando el código que trata el resultado de la comprobación devuelve el control a la pantalla de inicio, tras cerrarse el aviso si lo hubo. Las excepciones posteriores de otras funciones de la pantalla (por ejemplo, los KPIs) no son excepciones inesperadas de esta spec (§6).
- **Excepción original**: la excepción que causó el error que se intenta registrar, con su cadena de excepciones internas. En un fallo de conexión de tipo (a), es la excepción que lo causó; en una excepción inesperada, es esa misma excepción. Los fallos de conexión de tipo (b) y (c) no tienen excepción original (D-9).
- **Excepción completa**: tipo, mensaje y stack trace de una excepción y de toda su cadena de excepciones internas, incluidas todas las internas de una excepción agregada (`AggregateException`). Se guarda tal cual, salvo los datos secretos, que se reemplazan por `[OCULTO]` (D-6, D-11).
- **Datos secretos**: la cadena de conexión configurada y la contraseña que contiene. Con autenticación integrada no hay contraseña en la cadena, así que solo se oculta la cadena. Nunca se guardan en el registro ni se muestran en ningún aviso. Si aparecen en el mensaje o en el stack trace de cualquier excepción de la cadena (incluidas las internas), en cualquier tipo de entrada, solo ese dato se reemplaza por `[OCULTO]` y el resto se guarda tal cual (D-6, D-11). El usuario de BD, el nombre del servidor y el nombre de la base de datos no son datos secretos (D-6).
- **Información técnica interna**: stack trace, tipo de excepción, mensaje original de la excepción, datos secretos, nombre del servidor, de la base de datos o del usuario de BD y rutas de archivos. Nunca aparece en ningún aviso. En el registro sí puede aparecer, salvo los datos secretos (D-6, D-11).
- **Aviso de fallo de conexión**: diálogo modal en español con el título «Error de conexión», el mensaje «No se pudo conectar con la base de datos.» y el botón «Aceptar». Cuando corresponde (RF-12), lleva además, en una línea aparte, «No se pudo guardar el registro del error» (D-2, D-23).
- **Aviso genérico de error inesperado**: diálogo modal, distinto del aviso de fallo de conexión, con el título «Error», el mensaje «Se produjo un error inesperado al iniciar» y el botón «Aceptar», sin detalles técnicos. Cuando corresponde (RF-12), lleva además, en una línea aparte, «No se pudo guardar el registro del error» (D-8, D-23).
- **Cierre de sesión**: el usuario pulsa CERRAR SESIÓN. La pantalla de inicio se libera y la aplicación sigue en ejecución (D-22).
- **Cierre completo de la aplicación**: el usuario pulsa X o SALIR en la pantalla de inicio. La aplicación termina (D-22).
- **Pantalla de inicio no disponible**: la pantalla de inicio se está cerrando, ya está cerrada o ya fue liberada cuando el sistema está por presentar un aviso, aunque la comprobación haya terminado antes. Puede deberse a un cierre de sesión o a un cierre completo de la aplicación (D-14, D-22).
- **Carpeta de registro**: `%LOCALAPPDATA%\SistemaComercialG70\Logs\` (D-1).
- **Archivo de registro del día**: archivo de texto `errores-AAAA-MM-DD.log`, dentro de la carpeta de registro. La fecha es la local del equipo en el instante en que se registra la entrada (D-1, D-7).
- **Entrada de registro**: bloque de texto que se agrega al archivo de registro del día por cada fallo de conexión o excepción inesperada. Cada entrada indica su tipo: «fallo de conexión» o «excepción inesperada». El separador entre entradas y el formato exacto los define `plan.md`.
- **Fallo del registro**: no se puede crear la carpeta de registro, o no se puede crear o escribir el archivo de registro del día. Que dos registros simultáneos del mismo proceso coincidan en el mismo archivo no es un fallo del registro (RF-19).

## 3. Requisitos funcionales (EARS)
- RF-1: CUANDO se muestra la pantalla de inicio, EL SISTEMA comprueba la conexión con la base de datos una sola vez por cada apertura de la pantalla.
- RF-2: EL SISTEMA hace la comprobación de conexión a través de CapaNegocio y CapaDatos. CapaNegocio redacta el título, el mensaje, el texto del botón y la línea adicional de los dos avisos (D-23). La interfaz solo pide la comprobación y muestra lo que recibe (D-5).
- RF-3: SI la comprobación de conexión tiene éxito, ENTONCES EL SISTEMA no muestra ningún mensaje y la pantalla de inicio funciona como hasta ahora, salvo que después ocurra una excepción inesperada (RF-11).
- RF-4: SI se produce un fallo de conexión, ENTONCES EL SISTEMA muestra el aviso de fallo de conexión. Excepciones: si la pantalla de inicio no está disponible, se aplica RF-16; si antes de presentar el diálogo ocurrió además una excepción inesperada, se aplica RF-11 (D-18, D-20).
- RF-5: SI se produce un fallo de conexión o una excepción inesperada, con o sin fallo del registro, ENTONCES EL SISTEMA mantiene abierta la aplicación y, cuando el usuario cierra el aviso que se haya mostrado, permite seguir usándola como hasta ahora (D-2, D-8). Se exceptúa el cierre completo de la aplicación que pida el propio usuario (RF-18).
- RF-6: SI se produce un fallo de conexión, ENTONCES EL SISTEMA intenta guardar una entrada en el archivo de registro del día.
  - Si lo consigue, la entrada indica el tipo «fallo de conexión» y contiene la fecha y hora local del equipo, el motivo del fallo y, si existe, la excepción original completa.
  - Si el fallo no tiene excepción original (motivo «base de datos no accesible» o «plazo de 10 s vencido»), la entrada contiene solo el tipo, la fecha y hora local y el motivo.
  - La fecha del nombre del archivo y la hora de la entrada se toman del mismo instante: el momento de registrar (D-6, D-7, D-9).
- RF-7: CUANDO EL SISTEMA guarda una entrada de registro, SI la carpeta de registro o el archivo de registro del día no existen, ENTONCES EL SISTEMA los crea, incluidas las carpetas intermedias.
- RF-8: CUANDO EL SISTEMA guarda una entrada de registro, SI el archivo de registro del día ya existe, ENTONCES EL SISTEMA agrega la entrada al final sin borrar ni sobrescribir lo anterior.
- RF-9: SI la comprobación de conexión no termina en 10 s contados desde el inicio del intento de conexión, ENTONCES EL SISTEMA:
  - cancela o termina de verdad ese intento (no basta con dejar de esperarlo);
  - lo trata como fallo de conexión con motivo «plazo de 10 s vencido»;
  - presenta el aviso que corresponda, como máximo, 10 s después del inicio del intento de conexión más el tiempo de escribir el registro.
- RF-10: Por cada comprobación de conexión, EL SISTEMA muestra como máximo un aviso de fallo de conexión y guarda como máximo una entrada de fallo de conexión.
  - Un resultado tardío (éxito, fallo o excepción que llega después de vencido el plazo) no se registra ni genera aviso (D-3).
  - Este límite no incluye la entrada de una excepción inesperada (RF-11): en una misma comprobación puede guardarse una entrada de fallo de conexión y, además, una de excepción inesperada (D-18).
- RF-11: SI se produce una excepción inesperada, ENTONCES EL SISTEMA:
  - no la descarta en silencio;
  - intenta guardar una sola entrada de registro de esa excepción. Si lo consigue, la entrada indica el tipo «excepción inesperada» y contiene la fecha y hora local y la excepción completa, con la fecha del archivo y la hora tomadas del mismo instante. Esta entrada se suma a la de fallo de conexión que corresponda por RF-10, si la hubo (D-18, D-20);
  - no cierra la aplicación;
  - no muestra un segundo aviso por la misma comprobación;
  - si todavía no se ha mostrado ningún aviso, muestra el aviso genérico de error inesperado, también cuando antes hubo un fallo de conexión cuyo aviso aún no se había presentado (D-20). Las excepciones a esta regla son: la pantalla de inicio no está disponible (RF-16), o la excepción ocurrió al presentar el aviso, con el aviso visible o después de cerrarlo (RF-17) (D-8, D-17).
- RF-12: Salvo que la pantalla de inicio no esté disponible (RF-16, D-16):
  - para los fallos de conexión y las excepciones inesperadas ocurridos antes de presentar el diálogo, EL SISTEMA hace todos los intentos de guardar sus entradas antes de presentar el único aviso de esa comprobación;
  - SI cualquiera de esos intentos produce un fallo del registro, ENTONCES EL SISTEMA agrega a ese mismo aviso (de conexión o genérico) la línea «No se pudo guardar el registro del error», sin ruta ni detalle técnico;
  - nunca muestra un segundo diálogo para informar el fallo del registro, ni usa una ubicación alternativa u otro mecanismo para recuperar el registro.

  La excepción inesperada que ocurre al presentar el diálogo, con el diálogo visible o después de cerrarlo se rige por RF-17 (D-4, D-13, D-15, D-17, D-18, D-20).
- RF-13: SI ocurre un fallo del registro, ENTONCES EL SISTEMA lo contiene sin provocar otra excepción no controlada y sin impedir que se muestre el aviso que corresponda (D-4, D-13).
- RF-14: SI ocurre un fallo del registro, ENTONCES EL SISTEMA mantiene como causa del error informado el error que se intentaba registrar: en un fallo de conexión, su motivo y su excepción original si existe; en una excepción inesperada, esa excepción. La excepción que surge al intentar registrar no reemplaza ni hace perder ese error (D-4, D-13).
- RF-15: EL SISTEMA no incluye información técnica interna en ninguno de los dos avisos y no guarda datos secretos en el registro. SI el mensaje o el stack trace de cualquier excepción de la cadena (incluidas las internas), en una entrada de cualquier tipo, contienen la contraseña de la cadena de conexión configurada o la propia cadena, ENTONCES EL SISTEMA reemplaza solo ese dato por `[OCULTO]` y guarda el resto tal cual. Con autenticación integrada, solo se oculta la cadena de conexión (D-6, D-8, D-11).
- RF-16: SI se produce un fallo de conexión o una excepción inesperada Y la pantalla de inicio no está disponible, ENTONCES EL SISTEMA:
  - no muestra ningún aviso (ni el de conexión ni el genérico), aunque la comprobación haya terminado antes de que la pantalla dejara de estar disponible;
  - no accede a ningún control de la pantalla;
  - intenta guardar la entrada que corresponda (RF-6, RF-11) y, mientras la aplicación siga en ejecución (por ejemplo, tras un cierre de sesión), completa ese intento aunque la pantalla ya se haya liberado;
  - SI además ocurre un fallo del registro, ENTONCES tampoco muestra ningún aviso y contiene el fallo sin excepción no controlada (RF-13).

  Cuando la pantalla deja de estar disponible por un cierre completo de la aplicación, se aplica RF-18 (D-10, D-12, D-14, D-16, D-22).
- RF-17: SI se produce una excepción inesperada al presentar el aviso (de conexión o genérico), mientras ese aviso está visible o después de cerrarlo, ENTONCES EL SISTEMA:
  - guarda su entrada después (RF-11);
  - no muestra ningún otro diálogo, ni el aviso genérico ni uno para informar un fallo del registro;
  - no cierra la aplicación y no deja ninguna excepción no controlada;
  - SI además ese registro produce un fallo del registro, ENTONCES lo contiene sin informarlo al usuario (RF-13, RF-14) (D-17).
- RF-18: CUANDO el usuario hace un cierre completo de la aplicación (X o SALIR) con una comprobación en curso o todavía sin tratar, EL SISTEMA abandona la comprobación y su tratamiento sin retrasar la salida y no muestra ningún aviso. En ese caso no se garantiza el registro, como tampoco cuando el proceso termina por cualquier otra causa (D-22).
- RF-19: CUANDO dos registros del mismo proceso coinciden en el tiempo sobre el mismo archivo de registro del día (por ejemplo, al cerrar sesión y volver a entrar en menos de 10 s), EL SISTEMA guarda las dos entradas completas, sin perder ninguna ni mezclar su contenido, y ninguna cuenta como fallo del registro (observación técnica de la revisión).

## 4. Requisitos no funcionales
- RNF-1: Ningún formulario, evento, control ni servicio de CapaPresentacion usa `AppDbContext` ni accede directamente a CapaDatos (principio 1 de la constitución). Única excepción (D-5): `Program.cs` puede registrar `AppDbContext` durante el arranque, exclusivamente como raíz de composición de dependencias, y no puede usarlo para nada más.
- RNF-2: Mientras dura la comprobación de conexión, la pantalla de inicio se muestra y la interfaz sigue respondiendo.
- RNF-3: Pruebas automáticas con `dotnet test`. Reglas comunes:
  - **Tiempo máximo**: 30 s por prueba (operación funcional individual, `AGENTS.md`), salvo donde se indique otro límite.
  - **Criterio de fallo**: el resultado observado no coincide con el esperado, o vence el tiempo máximo. Al vencer el tiempo, el resultado es `FAILED — TIMEOUT / POSIBLE BLOQUEO`, con el diagnóstico obligatorio y sin reintentar.
  - **Detección de bloqueo**: las pruebas de integración (puntos 20 a 26) y cualquier otra que `plan.md` identifique con riesgo de bloqueo se ejecutan en un proceso aislado con watchdog externo, según «Timeout real, no declarativo» de `AGENTS.md`.
  - **Simulación**: los errores se simulan solo desde la prueba, sustituyendo la comprobación de conexión o el registro. No se añade a producción ningún mecanismo para provocar errores artificiales (D-21).

  Las pruebas cubren:
  1. Éxito sin mensaje (RF-3).
  2. Fallo por excepción, por base no accesible y por plazo vencido, cada uno con su entrada de tipo «fallo de conexión» y su motivo. Sin excepción original, la entrada contiene solo tipo, fecha y hora local y motivo (RF-4, RF-6, CL-2, CL-12).
  3. Cancelación real del intento al vencer el plazo: el intento queda cancelado o terminado, no solo sin esperar (RF-9).
  4. Un solo aviso y una sola entrada de fallo de conexión por comprobación, sin contar la entrada de una excepción inesperada (D-18). Incluye un resultado tardío (éxito, fallo o excepción) después del plazo, que no se registra ni genera aviso (RF-10, CL-4).
  5. Excepción inesperada, también después de una comprobación con éxito: se registra una vez, no se propaga y no cierra la aplicación. Si no se había mostrado ningún aviso, se muestra el aviso genérico (título «Error», mensaje «Se produjo un error inesperado al iniciar», botón «Aceptar») sin detalles técnicos. No hay segundo aviso por la misma comprobación (RF-3, RF-5, RF-11, RF-15).
  6. Creación de la carpeta y del archivo del día, y agregado al final de un archivo que ya existe (RF-7, RF-8, CL-6).
  7. Fallo del registro de un error ocurrido antes de presentar el diálogo, tanto en un fallo de conexión como en una excepción inesperada (RF-12, RF-13, RF-14, D-13, D-15):
     - el intento de escritura ocurre antes de presentar el diálogo;
     - se muestra un único diálogo (de conexión o genérico, según corresponda) con la línea adicional;
     - no hay segundo diálogo ni ubicación alternativa;
     - no hay excepción no controlada;
     - se conserva el error original (en la excepción inesperada, la propia excepción).

     Este orden no se exige para la excepción inesperada ocurrida al presentar el diálogo, con el diálogo visible o después de cerrarlo (punto 12, D-17).
  8. Ningún aviso contiene información técnica interna, y ninguna entrada contiene datos secretos. Se comprueba con excepciones cuyo mensaje o stack trace, en la excepción principal o en una interna, incluyen la contraseña de la cadena configurada o la propia cadena, tanto en entradas de fallo de conexión como de excepción inesperada: solo ese dato se reemplaza por `[OCULTO]` y el resto queda intacto. También se comprueba que, con autenticación integrada, solo se oculta la cadena (RF-15, D-11).
  9. Fecha del nombre del archivo y hora de la entrada en hora local y tomadas del mismo instante (RF-6, RF-11, D-7).
  10. Pantalla de inicio no disponible por cierre de sesión (también si la comprobación terminó antes), con fallo de conexión y con excepción inesperada: no se muestra ningún aviso (ni de conexión ni genérico), el registro se intenta y se completa aunque la pantalla esté liberada, no se accede a controles de la pantalla y no hay excepción no controlada (RF-16, CL-10, D-10, D-12, D-14, D-22).
  11. Pantalla de inicio no disponible por cierre de sesión, con fallo del registro, tanto con fallo de conexión como con excepción inesperada: no se muestra ningún aviso (ni siquiera con la línea adicional), el registro se intentó y no hay excepción no controlada (RF-13, RF-16, CL-10, D-16, D-22).
  12. Excepción inesperada al presentar el aviso, con el aviso visible o después de cerrarlo, tanto con el aviso de conexión como con el genérico (RF-11, RF-13, RF-14, RF-17, CL-9, D-17):
      - la entrada de la excepción se registra después;
      - no se muestra ningún otro diálogo;
      - la aplicación no se cierra y no hay excepción no controlada;
      - si ese registro falla, el fallo se contiene sin informarlo al usuario y sin segundo diálogo.
  13. Fallo de conexión seguido de una excepción inesperada antes de presentar el diálogo (RF-4, RF-10, RF-11, RF-12, CL-17, D-18, D-20):
      - se intentan dos entradas (una de fallo de conexión y otra de excepción inesperada), ambas antes del diálogo;
      - se muestra un solo diálogo, el aviso genérico;
      - la línea adicional aparece si falló cualquiera de los dos registros. Se cubren los cuatro casos: falla solo el primero, falla solo el segundo, fallan ambos, y no falla ninguno (sin línea).
  14. Cadena completa de excepciones internas, incluida una `AggregateException` con varias internas: todas quedan en la entrada (CL-5).
  15. Varias entradas el mismo día: quedan en el mismo archivo, en el orden en que ocurrieron (CL-7).
  16. Entradas en días locales distintos: cada una va al archivo de su día (CL-8).
  17. Textos de los avisos: títulos, mensajes, botón y línea adicional los proporciona CapaNegocio y coinciden literalmente con D-23. La pantalla los muestra sin cambiarlos y sin añadir información (RF-2, D-23).
  18. Clasificación según D-24, con CL-14 y CL-16:
      - una excepción entre la petición y la recepción del resultado produce el aviso de conexión y una entrada de tipo «fallo de conexión» con motivo «excepción»;
      - una excepción posterior a la recepción del resultado produce el aviso genérico y una entrada de tipo «excepción inesperada» (RF-4, RF-11).
  19. Dos registros simultáneos del mismo proceso sobre el mismo archivo: las dos entradas quedan completas y sin mezclarse, y ninguna se trata como fallo del registro ni produce la línea adicional (RF-19, CL-15).

  Pruebas automáticas de integración con la pantalla de inicio real y la comprobación de conexión simulada (D-21). Sustituyen a las antiguas pruebas manuales 2, 3, 4 y 6:
  20. Una sola comprobación por cada apertura de la pantalla, también al cerrar sesión y volver a entrar en la misma ejecución: dos aperturas producen exactamente dos comprobaciones (RF-1, CL-11).
  21. Con la comprobación simulada sin terminar, la interfaz procesa una acción del usuario en 10 s o menos (respuesta visual sencilla). Si no lo hace, se considera hilo de UI bloqueado (RNF-2).
  22. (antigua manual 2) BD no disponible: aparece el aviso de fallo de conexión con los textos de D-23. Después de «Aceptar», la pantalla sigue usable y la entrada queda en el archivo del día (RF-4, RF-5, RF-6, CL-1).
  23. (antigua manual 3) BD que no responde: el aviso aparece 12 s o menos después del inicio del intento de conexión. Ese margen es el plazo de 10 s más el tiempo de escribir el registro, con tolerancia declarada. Hay una sola entrada, con motivo «plazo de 10 s vencido» (RF-9, RF-10, CL-3).
  24. (antigua manual 4) Fallo del registro simulado: el aviso muestra la línea adicional y, después de «Aceptar», la pantalla sigue usable (RF-12, RF-13, CL-9).
  25. (antigua manual 6) Cierre de sesión durante la comprobación con una BD que no responde: no aparece ningún diálogo, la entrada queda en el archivo del día y la aplicación sigue funcionando y vuelve al inicio de sesión (RF-16, CL-10, D-22). Las variantes de este cierre con excepción inesperada y con fallo del registro quedan cubiertas por los puntos 10 y 11.
  26. Cierre completo (X o SALIR) con una comprobación simulada que no termina: el proceso termina en 10 s o menos desde que se pide la salida, sin ningún diálogo. El registro no se verifica porque no está garantizado. Se ejecuta en un proceso aislado y el watchdog mide el plazo (RF-18, CL-13, D-22).
- RNF-4: No hay migraciones, paquetes NuGet nuevos ni cambios en el esquema de BD. El registro usa solo un archivo de texto local.

## 5. Casos límite
- CL-1: Servidor SQL apagado o inaccesible: se muestra el aviso de fallo de conexión, se guarda el registro y la aplicación sigue usable.
- CL-2: Base de datos inexistente o credenciales rechazadas: se muestra el aviso de fallo de conexión sin revelar el motivo técnico y se guarda el registro.
- CL-3: La comprobación supera los 10 s: el aviso aparece al vencer el plazo, más el tiempo de escribir el registro, sin esperar a que el intento termine por sí solo.
- CL-4: Después de vencido el plazo llega un resultado tardío (éxito, fallo o excepción): no se registra, no se muestra un segundo aviso y no se guarda una segunda entrada (D-3).
- CL-5: Excepción con excepciones internas anidadas, incluida una `AggregateException` con varias internas: el registro conserva toda la cadena.
- CL-6: La carpeta de registro no existe (primera ejecución): se crea y se escribe el archivo del día.
- CL-7: Varios fallos el mismo día: todos quedan en el mismo archivo, en el orden en que ocurrieron.
- CL-8: Fallos en días distintos según la fecha local del equipo: cada día tiene su propio archivo. Cada entrada va al archivo del día del instante en que se registró.
- CL-9: No se puede escribir el registro ni crear su carpeta (sin permisos, disco lleno, archivo bloqueado) tras un fallo de conexión o una excepción inesperada ocurridos antes de presentar el diálogo: el único aviso que corresponda (de conexión o genérico) muestra la línea adicional, sin segundo diálogo, y la aplicación sigue usable (D-15, D-18). Si la excepción inesperada ocurre al presentar el diálogo, con el diálogo visible o después de cerrarlo, el fallo del registro no se informa al usuario (D-17).
- CL-10: El usuario cierra sesión y la pantalla de inicio ya no está disponible cuando el sistema va a presentar el aviso, con fallo de conexión o con excepción inesperada, aunque la comprobación haya terminado antes. Resultado:
  - no se muestra ningún aviso, ni de conexión ni genérico;
  - el registro se intenta y se completa mientras la aplicación siga en ejecución;
  - si además falla el registro, tampoco se muestra ningún aviso;
  - no se accede a controles de la pantalla y la aplicación no falla.

  Si la pantalla dejó de estar disponible por un cierre completo de la aplicación, se aplica CL-13 (D-10, D-12, D-14, D-16, D-22).
- CL-11: La pantalla de inicio se abre varias veces en la misma ejecución de la aplicación (por ejemplo, al cerrar sesión y volver a entrar): cada apertura hace una sola comprobación.
- CL-12: Fallo de conexión sin excepción (base no accesible o plazo vencido): la entrada contiene solo el tipo, la fecha y hora local y el motivo (D-9).
- CL-13: Cierre completo (X o SALIR) con la comprobación en curso: la salida no se retrasa, no se muestra ningún aviso y no se garantiza el registro (D-22).
- CL-14: Excepción entre la petición de la comprobación y la recepción de su resultado, aunque no venga de la conexión: es un fallo de conexión con motivo «excepción» y se muestra el aviso de conexión (D-24).
- CL-15: Cierre de sesión y nuevo ingreso en menos de 10 s, con dos comprobaciones que registran a la vez: las dos entradas se guardan completas y ninguna cuenta como fallo del registro (RF-19).
- CL-16: Comprobación con éxito seguida de una excepción inesperada antes de mostrarse ningún aviso: se muestra el aviso genérico y se registra la excepción (RF-3, RF-11).
- CL-17: Fallo de conexión seguido de una excepción inesperada antes de presentar el aviso: se muestra un único diálogo, el aviso genérico, con dos entradas independientes. La línea adicional aparece si falla cualquiera de las dos escrituras (D-18, D-20).

## 6. Fuera de alcance
- KPIs y gráficos del dashboard.
- Otros formularios y módulos, y cualquier reintento automático de conexión.
- Cambios en el comportamiento de la aplicación después del aviso (no se deshabilitan módulos ni se cierra la sesión).
- `MEMORY.md` y documentación general.
- Paquetes NuGet, migraciones y cambios de esquema.
- Ubicación alternativa o recuperación del registro cuando falla su escritura.
- Rotación, limpieza o tamaño máximo de los archivos de registro.
- Garantizar el registro cuando la aplicación se cierra por completo (X o SALIR) o cuando el proceso termina por cualquier otra causa (D-22).
- Mecanismos en el código de producción para provocar errores artificiales (D-21).

## 7. Criterios de finalización
- `dotnet build` sin errores y `dotnet test` en verde con todas las pruebas de RNF-3, sin regresiones.
- Búsqueda estática de referencias a `AppDbContext` y a CapaDatos en los archivos `*.cs` de CapaPresentacion, sin `bin/` ni `obj/`: la única coincidencia es `Program.cs` (`using CapaDatos` y el registro de `AppDbContext`) (D-5). La referencia de proyecto del `.csproj` (`CapaPresentacion.csproj:20`) se permite solo para esa raíz de composición, o se elimina si basta la referencia transitiva; lo decide `plan.md`.
- Prueba manual (única que se mantiene, D-21):
  1. BD disponible.
     - Precondiciones: BD de desarrollo preparada y disponible (`scripts/preparar-bd.ps1`), un usuario de prueba activo y la aplicación compilada.
     - Acción: iniciar la aplicación, iniciar sesión y quedarse en la pantalla de inicio.
     - Resultado esperado: la pantalla de inicio se muestra; durante al menos 12 s no aparece ningún diálogo y no se agrega ninguna entrada al archivo de registro del día.
     - Tiempo máximo: arranque 20 s o menos; escenario completo 120 s o menos.
     - Criterio de éxito: se cumple todo el resultado esperado dentro de los plazos.
     - Criterio de fallo: aparece algún diálogo, se agrega una entrada o vence un plazo. Si vence un plazo, el resultado es `FAILED — TIMEOUT / POSIBLE BLOQUEO`, con el diagnóstico obligatorio y sin reintentar. El bloqueo se detecta con un watchdog externo que comprueba el PID y si el proceso responde (`Responding`).
- Las antiguas pruebas manuales 2, 3, 4 y 6 se sustituyen por los puntos 22 a 25 de RNF-3 (D-21). La antigua prueba manual 5 (excepción inesperada) queda cubierta solo por pruebas automáticas (D-19).

## 8. Decisiones
- D-1 (resuelta 2026-11-01, usuario): registro en un archivo de texto local, sin paquetes. Ruta `%LOCALAPPDATA%\SistemaComercialG70\Logs\`; un archivo por día, `errores-AAAA-MM-DD.log`; las carpetas se crean si no existen; si el archivo ya existe, se agrega al final. Cubre RF-6, RF-7, RF-8 y RNF-4.
- D-2 (resuelta 2026-11-01, usuario): el aviso es un diálogo modal que se cierra con «Aceptar» y después la aplicación sigue usable. D-23 precisa los textos y D-22 trata el cierre completo de la aplicación. Cubre RF-4, RF-5 y la definición de «Aviso de fallo de conexión».
- D-3 (resuelta 2026-11-01, usuario): la comprobación dura como máximo 10 s y, al vencer el plazo, se trata como fallo de conexión. El timeout es real: no basta con dejar de esperar (por ejemplo, con `Task.WaitAsync`) si el intento sigue en curso; el intento queda cancelado o terminado. Una comprobación no genera más de un aviso ni más de un registro. Precisado por D-18/D-20: como máximo una entrada de fallo de conexión, más la de una excepción inesperada si la hay. Cubre RF-9, RF-10, CL-3, CL-4, la definición de «Resultado tardío» y RNF-3 puntos 3, 4 y 23.
- D-4 (resuelta 2026-11-01, usuario): si no se puede escribir el registro, incluida la creación de su carpeta:
  1. se muestra el aviso normal de fallo de conexión con la línea «No se pudo guardar el registro del error», sin ruta ni detalle técnico. Excepciones: pantalla no disponible (D-16), o excepción inesperada antes del diálogo (D-18, D-20). Para el caso de excepción inesperada, ver D-13;
  2. no se usa una ubicación alternativa ni otro mecanismo de recuperación;
  3. el fallo del registro nunca provoca otra excepción no controlada, no cierra la aplicación ni impide el aviso original;
  4. no se pierde ni se reemplaza la excepción original de conexión;
  5. no se guardan ni se muestran cadenas de conexión, contraseñas ni credenciales (D-6 precisa qué son «credenciales»);
  6. la aplicación sigue abierta y funcionando.

  Cubre RF-5, RF-12, RF-13, RF-14 y RF-15.
- D-5 (resuelta 2026-11-01, usuario), textual: «Acepto que Program.cs, exclusivamente como raíz de composición de dependencias, pueda registrar AppDbContext. Esto no constituye una excepción para la lógica de presentación: ningún formulario, evento ni servicio de CapaPresentacion puede utilizar AppDbContext ni acceder directamente a CapaDatos. La comprobación de conexión debe realizarse a través de CapaNegocio y CapaDatos, respetando el aislamiento de las cuatro capas. Limitá esta excepción exclusivamente al registro de dependencias en el arranque.» Cubre RF-2, RNF-1 y el criterio de búsqueda estática de §7.
- D-6 (resuelta 2026-11-01, usuario): en el registro se guarda la excepción completa tal cual (tipo, mensaje, excepciones internas y stack trace). Nunca se guardan contraseñas ni la cadena de conexión; D-11 indica qué hacer si aparecen dentro de la excepción. El usuario de BD, el servidor y el nombre de la base no son credenciales: pueden quedar en el registro, pero nunca en el aviso al usuario. Cubre RF-6, RF-15 y las definiciones de «Excepción completa», «Datos secretos» e «Información técnica interna».
- D-7 (resuelta 2026-11-01, usuario): la fecha del nombre del archivo y la hora de cada entrada usan la hora local del equipo. Cubre RF-6, RF-11, CL-8, la definición de «Archivo de registro del día» y RNF-3 puntos 9 y 16.
- D-8 (resuelta 2026-11-01, usuario): si ocurre una excepción inesperada antes de mostrarse ningún aviso, se muestra un aviso genérico: diálogo modal, distinto del de conexión, con el texto «Se produjo un error inesperado al iniciar», sin detalles técnicos, que se cierra con «Aceptar». Además, la excepción se registra una vez, la aplicación no se cierra y nunca hay un segundo aviso por la misma comprobación. D-23 precisa el título. Cubre RF-5, RF-11, RF-15, RNF-3 punto 5 y la definición de «Aviso genérico de error inesperado».
- D-9 (resuelta 2026-11-01, usuario): en un fallo de conexión sin excepción, la entrada lleva solo la fecha y hora local y el motivo («base de datos no accesible» o «plazo de 10 s vencido»), además de la indicación de tipo común a todas las entradas. Cubre RF-6, CL-12, RNF-3 punto 2 y la definición de «Excepción original» (fallos sin excepción original).
- D-10 (resuelta 2026-11-01, usuario): si el usuario cierra la pantalla de inicio antes de que termine la comprobación, la aplicación no falla. D-12 amplía y precisa qué avisos se omiten, el registro y el acceso a controles; D-14 define qué se entiende por pantalla cerrada; D-22 distingue el cierre de sesión del cierre completo. Cubre RF-4, RF-16 y CL-10.
- D-11 (resuelta 2026-11-01, usuario): si el mensaje o el stack trace a registrar contienen una contraseña o la cadena de conexión, solo ese dato se reemplaza por `[OCULTO]` y el resto se guarda tal cual. Precisa D-6. Se aplica a toda la cadena de excepciones y a los dos tipos de entrada. Cubre RF-15, RNF-3 punto 8 y las definiciones de «Excepción completa», «Datos secretos» e «Información técnica interna».
- D-12 (resuelta 2026-11-01, usuario), textual: «Pantalla de inicio cerrada: Si la pantalla de inicio ya se cerró antes de completarse la operación, no mostrar ningún aviso, ni de conexión ni genérico. Registrar igualmente el fallo o excepción y no intentar acceder a controles de una pantalla cerrada.» Amplía D-10. D-14 define qué es una pantalla cerrada y cuándo se evalúa; D-16 fija su prevalencia sobre el fallo del registro; D-22 limita la garantía del registro. Cubre RF-4, RF-11, RF-16, CL-10 y RNF-3 punto 10.
- D-13 (resuelta 2026-11-01, usuario), textual: «Fallo del registro durante un error inesperado: Mostrar el aviso genérico «Se produjo un error inesperado al iniciar» y agregar la línea «No se pudo guardar el registro del error». Sin rutas ni detalles técnicos. Aplicar el mismo criterio de fallo del registro que D-4: sin ubicaciones alternativas, sin excepciones no controladas y conservando la excepción original.» Cubre RF-12, RF-13, RF-14, CL-9, RNF-3 punto 7 y la definición de «Excepción original».
- D-14 (resuelta 2026-11-01, usuario), textual: «D-10 / D-12: La pantalla se considera cerrada si está cerrándose, cerrada o ya fue liberada cuando el sistema está por presentar el aviso. Aunque la comprobación haya terminado antes, si la pantalla ya no está disponible, no se muestra ningún diálogo. El registro se intenta igualmente.» Precisa D-10 y D-12. Cubre la definición de «Pantalla de inicio no disponible», RF-4, RF-11, RF-16, CL-10, RNF-3 punto 10 y la antigua prueba manual 6, sustituida por RNF-3 punto 25 (D-21).
- D-15 (resuelta 2026-11-01, usuario), textual: «Fallo del registro: El intento de escritura del log debe realizarse antes de mostrar el único diálogo correspondiente a la comprobación. Si falla, el mensaje definitivo incorpora: «No se pudo guardar el registro del error». Esto aplica tanto al aviso de conexión como al aviso genérico. Nunca se muestra un segundo diálogo.» Precisa D-4 y D-13 (primero el registro, después el diálogo único). Solo se aplica a los errores ocurridos antes de presentar el diálogo (D-17). Si hay más de un intento de registro antes del diálogo, ver D-18 y D-20. Cubre RF-12, CL-9 y RNF-3 puntos 7 y 13.
- D-16 (resuelta 2026-11-01, usuario), textual: «Si la pantalla ya está cerrada, prevalece D-12: no se muestra ningún aviso, aunque también haya fallado el registro.» Cubre RF-12, RF-16, CL-10 y RNF-3 punto 11.
- D-17 (resuelta 2026-11-01, usuario), textual: «Registrar después, sin avisar: D-15 aplica solo a errores ocurridos antes del diálogo. Este se registra después y, si el registro falla, no se informa al usuario (no hay segundo diálogo). La aplicación no se cierra y no hay excepción no controlada.» Limita D-15 a los errores ocurridos antes de presentar el diálogo. Fija la regla para la excepción inesperada ocurrida al presentar el diálogo, con el diálogo visible o, según la observación técnica de la revisión, después de cerrarlo. Cubre RF-11, RF-12, RF-17, CL-9 y RNF-3 puntos 7 y 12.
- D-18 (resuelta 2026-11-01, usuario), textual: «Dos entradas, línea si falla una: Se guardan las dos entradas, la del fallo de conexión y la del error inesperado. El aviso genérico lleva la línea si falló cualquiera de los dos registros. Sigue habiendo un solo diálogo.» Precisa D-15 cuando en la misma comprobación hay un fallo de conexión y una excepción inesperada antes del diálogo. Cubre RF-4, RF-10, RF-11, RF-12, CL-9, CL-17 y RNF-3 puntos 4 y 13.
- D-19 (resuelta 2026-11-01, usuario), textual: «acepto que la prueba manual 5 quede cubierta exclusivamente por las pruebas automáticas de RNF-3, puntos 5 y 7». Cubre la antigua prueba manual 5, retirada de §7.
  > Nota del planner (no es texto del usuario): los puntos 12, 13 y 18 de RNF-3, añadidos por D-17, D-18/D-20 y D-24, también cubren la excepción inesperada.
- D-20 (resuelta 2026-11-01, usuario), textual: «Si ocurre un fallo de conexión y después una excepción inesperada antes del aviso, mostrar el aviso genérico. Conservar las dos entradas de registro independientes. Agregar la línea «No se pudo guardar el registro del error» si falla cualquiera de las dos escrituras. Un solo diálogo.» Confirma D-18 y fija qué aviso se muestra. Cubre RF-4, RF-11, RF-12, CL-17 y RNF-3 punto 13.
- D-21 (resuelta 2026-11-01, usuario), textual: «Reemplazar las pruebas manuales imposibles por pruebas automáticas de integración que utilicen la pantalla de inicio real con la comprobación de conexión simulada. No modificar el comportamiento de producción para provocar errores artificiales. Mantener la prueba manual de conexión disponible.» La prueba manual que se mantiene es la 1, «BD disponible». Cubre §7, §6 (sin mecanismos artificiales) y RNF-3 puntos 20 a 26.
- D-22 (resuelta 2026-11-01, usuario), textual: «Garantizar el registro cuando se cierra sesión y la aplicación permanece abierta. Si el usuario cierra completamente la aplicación mediante X o SALIR, abandonar la comprobación sin retrasar la salida. No garantizar el registro cuando termina el proceso.» Cubre las definiciones de «Cierre de sesión» y «Cierre completo de la aplicación», RF-5, RF-16, RF-18, CL-10, CL-13, §6 y RNF-3 puntos 10, 11, 25 y 26.
- D-23 (resuelta 2026-11-01, usuario), textual: «Conexión: título «Error de conexión» y mensaje «No se pudo conectar con la base de datos.» Error inesperado: título «Error» y mensaje «Se produjo un error inesperado al iniciar». Botón «Aceptar» en ambos.» Precisa D-2 y D-8. Cubre las definiciones de los dos avisos, RF-2 y RNF-3 puntos 5, 17 y 22.
- D-24 (resuelta 2026-11-01, usuario), textual: «Todo error ocurrido desde que la pantalla solicita la comprobación hasta que recibe su resultado se clasifica como fallo de conexión y utiliza el aviso de conexión. Las excepciones posteriores al resultado siguen utilizando el aviso genérico.» Cubre las definiciones de «Fallo de conexión» y «Excepción inesperada», RF-4, RF-11, CL-14, CL-16 y RNF-3 punto 18.

## 9. Dudas abiertas
Ninguna.
