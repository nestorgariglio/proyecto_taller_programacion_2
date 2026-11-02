# T10 — Procedimiento de verificación contra SQL Server (atomicidad, catálogo, doble envío y permisos)

> **Estado**: procedimiento corregido el 2026-11-01 (decisión del usuario, punto 2 «A aprobada») y **recortado el mismo día** por un nuevo criterio del usuario: sin experimentos adicionales ni pruebas de carga o rendimiento; solo lo imprescindible para la integridad de los datos y los criterios de aceptación de HU-09. **RNF-9 (200 líneas, < 200 ms): SIN VERIFICAR — descartada por decisión del usuario (2026-11-01)**; T10 ya no la requiere ni define experimento. **Aún no ejecutado**: no hay resultados en este documento; la tabla de la sección 10 está vacía a propósito. Se verificó contra el código vigente, pero ningún supuesto de la sección 3.4 se comprobó en un SQL Server real.
> **Quién ejecuta**: **solo el usuario**, después de que Reviewer verifique este procedimiento. **Ningún agente** ejecuta T10, crea objetos temporales ni modifica SQL Server durante la prueba. Este documento no contiene código C# ni lo modifica.

> **ADVERTENCIA (leer antes de empezar)**: este procedimiento crea una restricción temporal, `CK_FALLO_SIMULADO_DETALLE`, sobre `DETALLE_COMPRA`. **Mientras exista, TODA compra que incluya el producto P2 fallará**, también las que haga cualquier otra persona o prueba. **Hay que eliminarla aunque la prueba falle, se interrumpa o se cierre la aplicación** (paso A6). Si no estás seguro de si quedó creada, ve directo a la sección 9 («Si algo sale mal»).

---

## 1. Reglas de aislamiento y de ejecución

- **R-1 Base única**: apuntar **solo** a `localhost\SQLEXPRESS` / `DB_SISTEMA_VENTA`, con autenticación integrada (la misma cadena que usa la aplicación en `CapaPresentacion/appsettings.json`, línea 3). Antes de cualquier escritura, ejecutar `SELECT DB_NAME();` y comprobar el resultado (paso A0). **Antes de T10, el usuario confirma que la variable de entorno `ConnectionStrings__CadenaSQL` NO está definida** (en PowerShell, solo lectura: `[Environment]::GetEnvironmentVariable('ConnectionStrings__CadenaSQL','User')` y `...,'Machine')` deben devolver vacío; también en la sesión desde la que se abre la aplicación). Si estuviera definida, podría reemplazar la cadena de `appsettings.json` y la aplicación escribiría en otro destino: detener T10. El Reviewer verificó el 2026-11-02 que no estaba definida en Process/User/Machine; la confirmación del usuario sigue siendo obligatoria. La aplicación y SSMS deben apuntar al **mismo destino** (`localhost\SQLEXPRESS` / `DB_SISTEMA_VENTA`).
- **R-2 Sin seed ni preparar-bd**: **NO ejecutar `scripts/preparar-bd.ps1` ni `scripts/seed-desarrollo.sql` durante T10 ni entre sus pasos**: restablecen o modifican datos de `PRODUCTO` y falsean RF-47 (la instantánea «antes/después» dejaría de ser comparable). Si se necesita sembrar, hacerlo **antes** del paso A0 y no repetirlo.
- **R-3 Sin DELETE ni migraciones**: no se ejecuta `DELETE` físico (AGENTS.md; RNF-4) ni `dotnet ef`. Las compras que se registren **quedan para siempre** (no hay borrado físico); están marcadas «PERMANENTE» en la sección 11.
- **R-4 Solo la aplicación escribe compras** (RF-48): las filas de `COMPRA` y `DETALLE_COMPRA` las escribe únicamente la aplicación. Las únicas escrituras por SQL permitidas son la restricción temporal (DDL) y el intento transitorio con `ROLLBACK` del paso A4.2, ambos declarados en la sección 11.
- **R-5 Conexión nueva**: «conexión nueva» = otra pestaña de consulta con su propia conexión (en SSMS: *Archivo → Nuevo → Consulta de Database Engine*). Comprobar `SELECT @@TRANCOUNT;` = 0 al abrirla. **No dejar transacciones abiertas** en ninguna pestaña: bloquearían a la aplicación.
- **R-6 Lotes**: los scripts usan `GO` como separador de lotes (SSMS o `sqlcmd`). `CREATE`/`ALTER` van **en su propio lote**; se ejecutan **solos**, no junto con otras sentencias.
- **R-7 Marcadores**: `<ID_P1>`, `<ID_P2>`, `<ID_PROVEEDOR>`, `<ID_USUARIO>`, `<L1>`, `<L2>` se reemplazan **a mano por el número literal** (incluidos los `< >`). Una variable T-SQL **no sobrevive** a una conexión nueva ni a un `GO`, por eso `L1` y `L2` se anotan en la tabla de la sección 10 y se pegan como números. Si queda un marcador sin reemplazar, SQL Server devuelve error de sintaxis y no ejecuta nada.
- **R-8 Credenciales**: se usan las «credenciales del README» (`scripts/README-preparar-bd.md`); no se reproducen aquí.
- **R-9 Acciones del usuario**: la comprobación de `git status` (RNF-10) y cualquier otro comando de Git son **acción del usuario** (AGENTS.md §9); ningún agente los ejecuta.

---

## 2. Política de tiempos y protocolo de bloqueo (AGENTS.md §11)

**Límites por defecto**: arranque de la aplicación 20 s · respuesta visual sencilla 10 s · operación funcional individual 30 s · escenario completo 120 s · experimento completo 8 min. Cada prueba de la sección 4 repite sus seis datos.

**Sin ajustes de tiempo**: se aplican los límites estándar de AGENTS.md §11 (tabla anterior) a todas las pruebas; el experimento A completo (pasos A0 a A9) tiene el máximo estándar de 8 min.

**Cómo se mide el plazo**: el plazo corre desde el inicio real de la acción (clic o `F5`), con cronómetro del usuario; no desde que se recuerda comprobarlo. Hay dos «watchdogs»: el cronómetro del usuario para la aplicación y `SET LOCK_TIMEOUT 10000;` en cada pestaña SQL (los scripts de este documento lo incluyen), de modo que una espera por bloqueo de la base termina en el error 1222 en 10 s en vez de colgarse.

**Si vence el plazo** → registrar **`FAILED — TIMEOUT / POSIBLE BLOQUEO`**. No es «pendiente». **No se vuelve a hacer clic, no se reinicia la operación, no se salta el paso, no se alarga el plazo.** Diagnóstico, en este orden, **antes de cerrar nada**: (1) acción en curso; (2) hora de inicio y tiempo transcurrido frente al plazo; (3) esperado vs. observado; (4) captura de pantalla; (5) ventana activa y PID (Administrador de tareas → Detalles); (6) mensajes de consola/registro; (7) estado del proceso («No responde», CPU, memoria); (8) volcado de pila si la herramienta está instalada (si no, declararlo); (9) última operación completada bien; (10) clasificación: `timeout` / `aplicación no responsiva` / `error de automatización`. Recién después se cierra el proceso (Administrador de tareas → *Finalizar árbol de procesos*) y, **antes de continuar con cualquier cosa**, se ejecuta la sección 9.

**Operaciones con efectos** (registrar una compra): **nunca se repiten sin verificar antes** si la anterior se completó (consulta `COUNT` de la sección correspondiente). Máximo un reintento, solo de operaciones seguras (lecturas) y solo tras diagnosticar el primer fallo.

---

## 3. Referencias verificadas en el código (para no depender de la memoria)

### 3.1 Nombres reales de tablas y columnas (entidades `CapaEntidad` y `AppDbContextModelSnapshot.cs`)

| Tabla | Columnas usadas en este procedimiento |
|---|---|
| `COMPRA` | `id_compra` (IDENTITY), `id_usuario`, `id_proveedor`, `monto_total` (`decimal(18,2)`), `estado` (`bit`), `fecha_registro` (`datetime2`) |
| `DETALLE_COMPRA` | `id_detalle_compra` (IDENTITY), `id_compra`, `id_producto`, `precio_compra` (`decimal(18,2)`), `cantidad` (`int`), `subtotal` (`decimal(18,2)`). **No existe columna de precio de venta** (D-2) |
| `PRODUCTO` | `id_producto`, `codigo`, `nombre`, `stock`, `precio_compra`, `precio_venta`, `estado`, `id_usuario_modifica`, `fecha_modificacion`, `motivo_ajuste_stock` |
| `PROVEEDOR` | `id_proveedor`, `cuit`, `razon_social`, `estado` |
| `USUARIO` | `id_usuario`, `id_rol`, `nombre`, `apellido`, `estado` |

El esquema es `dbo` (por defecto de EF); el paso A0 lo confirma. Los nombres de las FK no se conocen con certeza (EF las genera); el procedimiento **no** depende de ellos. `COMPRA` y `DETALLE_COMPRA` no declaran triggers en `AppDbContext`; el único `HasTrigger` es el de `PRODUCTO` (`TR_PRODUCTO_FECHA_MODIFICACION`, `CapaDatos/AppDbContext.cs`, línea 127).

### 3.2 Flujo transaccional de `RegistrarCompraAsync` (`CapaNegocio/CompraNegocio.cs`)

| Líneas | Qué hace |
|---|---|
| 325–442 | Rol, proveedor, usuario, productos (una consulta acotada), líneas (sin repetidos), ítems y tope del total: **toda la validación ocurre antes de abrir la transacción**. Un rechazo no escribe nada |
| 445–451 | `BeginTransactionAsync()` abre la transacción explícita |
| 455–476 | Arma `Compra` con sus `Detalles`; subtotales **recalculados** (`CalcularSubtotal`, línea 427; RNF-13) |
| 479–480 | `Compras.Add(cabecera)` + **un único** `SaveChangesAsync()` |
| 483 | Gancho de HU-10 (hoy no hace nada; RF-47) |
| 486 | `CommitAsync()` |
| 498–507 | `catch` de `DbUpdateException` / `DbException` / `InvalidOperationException` (`EsFalloDeOrigenDeDatos`, línea 1728): rollback protegido, desadjunte, respuesta `ErrorPersistencia` con el mensaje único (línea 88) |
| 508–515 | `finally`: libera la transacción |

**Lecturas previas a la transacción (hallazgo I-1): corregido por C-1 (T22), validado por pruebas automáticas** (271/271 del filtro Compra; sin validación funcional en la aplicación). En T10, un fallo de lectura previo (proveedor, usuario o productos) devolvería el mismo mensaje único de persistencia (sección 3.3). No se agrega ningún experimento: el fallo de lectura no se puede provocar desde la UI.

**Nota de líneas**: los números de línea de la tabla anterior son del 2026-11-01; C-1 desplazó `CompraNegocio.cs` (hoy `BeginTransactionAsync` en 461 y `CommitAsync` en 496). Ubicar por nombre de método.

En `CapaPresentacion/Compras/ComprasForm.cs`: `RegistrarCompraAsync` (línea 850) ignora un segundo intento con `_registrando` (líneas 852–855), deshabilita `btnRegistrar` (865), muestra `respuesta.Mensaje` (878), **no limpia nada si no hay éxito** (880–883) y rehabilita en el `finally` (887–891).

### 3.3 Mensaje que debe verse en el fallo de persistencia

«No se pudo completar la operación por un problema con el origen de datos. Intente nuevamente.» (constante `MensajeErrorPersistencia`, `CompraNegocio.cs` línea 88; RF-45, RF-55, RF-56, RNF-8). La aplicación **no muestra** el número de error SQL.

### 3.4 Análisis del mecanismo de fallo (razonado, **sin ejecutar**)

**Por qué NO un trigger**: el defecto fue confirmado por Reviewer. EF Core inserta con `OUTPUT` sin `INTO`; con un trigger habilitado en la tabla SQL Server rechaza ese `INSERT` con el **error 334** *antes* de que el trigger llegue a su `THROW`. El fallo no sería el previsto y, además, el proyecto ya declara `HasTrigger` para `PRODUCTO`. Se descarta el trigger `TR_FALLO_SIMULADO_DETALLE`.

**Mecanismo elegido**: restricción CHECK temporal creada con `WITH NOCHECK`:

```sql
ALTER TABLE dbo.DETALLE_COMPRA WITH NOCHECK
    ADD CONSTRAINT CK_FALLO_SIMULADO_DETALLE CHECK (id_producto <> <ID_P2>);
```

- `WITH NOCHECK` es **necesario**: el paso A2 deja ya en la tabla un detalle con P2 (la compra del caso bueno); sin `NOCHECK`, SQL Server rechazaría crear la restricción por esa fila existente. Las filas nuevas **sí** se validan (la restricción queda habilitada, `is_disabled = 0`; queda marcada `is_not_trusted = 1`, lo que es esperado).
- Una restricción CHECK **no es un trigger**: no interfiere con el `OUTPUT` de EF Core, por lo que no se produce el error 334.
- Un `INSERT` de una fila con `id_producto = <ID_P2>` falla con el **error 547** («conflicto con la restricción CHECK "CK_FALLO_SIMULADO_DETALLE"»). Las filas de P1 y la cabecera `COMPRA` no se ven afectadas por la restricción.

**Qué ocurre dentro de `RegistrarCompraAsync` (confirmado por código vs. a verificar)**:
- *Confirmado por código*: toda la escritura ocurre dentro de la transacción explícita de la línea 451, con un único `SaveChangesAsync` (480). Cualquier `DbUpdateException` se captura (498), se hace rollback de la transacción (503) y se devuelve el mensaje único (506). Por eso el **estado final** esperado es 0 cabeceras y 0 detalles **sin depender** de qué sentencias se hayan ejecutado antes del error.
- *Hipótesis, a verificar en SQL Server real*: (H-1) EF Core inserta primero `COMPRA` (necesita su `id_compra` generado) y luego los detalles en el orden en que se agregaron (P1, P2). (H-2) Con `UseSqlServer(connectionString)` sin opciones (`CapaPresentacion/Program.cs`, línea 33) rige el tamaño mínimo de lote por defecto del proveedor; con 3 sentencias (1 cabecera + 2 detalles) es probable que se envíen como comandos separados, pero **no está confirmado**. (H-3) La sentencia que contiene la fila de P2 falla **completa** (una sentencia `INSERT` es atómica); si EF la envía dentro de un lote con otras, SQL Server no deshace por sí solo las anteriores, y es la **transacción explícita** la que las revierte.
- **Por eso este documento no afirma** que «la cabecera y la primera línea se insertan antes del fallo»: puede ser cierto o no según H-1/H-2 y **no es observable desde la aplicación**. Lo que sí se afirma, y se verifica, es el resultado final: **0 / 0 / 0**.
- *Qué observa el usuario*: en la aplicación, el mensaje único de la sección 3.3 y la pantalla **intacta** (proveedor, líneas, precios y totales; RNF-11, RF-56); en SQL, tras la limpieza, ninguna fila nueva.

**Cómo se descarta que el fallo sea ajeno al escenario** (la aplicación no muestra el número de error):
1. *Control negativo*: los mismos datos (P, P1, P2) se registran bien en A2 (sin mecanismo) y se registran bien en A8 (con el mecanismo eliminado). Lo único que cambia entre el fallo y el éxito es la restricción.
2. *Comprobación previa del mecanismo* (paso A4): demuestra con una inserción transitoria con `ROLLBACK` que la restricción dispara el **547 con el nombre `CK_FALLO_SIMULADO_DETALLE`** sobre P2 y no sobre P1.
3. *Triggers*: la consulta de A0 comprueba que `COMPRA` y `DETALLE_COMPRA` **no tienen triggers** (descarta el 334).

| Error SQL | Significado | ¿Es el esperado? |
|---|---|---|
| **547** con `CK_FALLO_SIMULADO_DETALLE` en el mensaje | Violación de la restricción CHECK temporal | **Sí** (solo se ve en SSMS, paso A4) |
| 547 con un nombre `FK_...` | Clave foránea (usuario, proveedor, producto o compra inexistentes) | **No**: dato ajeno al escenario |
| 334 | Trigger habilitado en la tabla + `OUTPUT` de EF | **No**: apareció un trigger en `DETALLE_COMPRA` |
| 515 | `NULL` en columna obligatoria | **No** |
| 2627 / 2601 | Clave única duplicada | **No** |
| 8115 / 8152 | Desbordamiento aritmético / truncamiento | **No** |
| 1205 / 1222 / -2 | Interbloqueo / espera de bloqueo agotada / tiempo de espera | **No** (bloqueo o red): tratar como `FAILED — TIMEOUT / POSIBLE BLOQUEO` |
| 4060 / 18456 | Base o inicio de sesión inaccesibles | **No**: la base o la cuenta no son las previstas (R-1) |

---

## 4. Declaración de las pruebas (AGENTS.md §11: seis datos por prueba)

Todas parten de: aplicación compilada y en marcha contra `DB_SISTEMA_VENTA`; usuario Administrador o Encargado con sesión abierta (credenciales del README); ninguna ejecución de seed/preparar-bd entre pasos (R-2).

### T10-A — Atomicidad con fallo inyectado (RF-44, RF-45, RF-56, RNF-11, RNF-13)
- **Precondiciones**: un proveedor activo `P`; dos productos activos `P1` y `P2` con precios de catálogo mayores que 0; `COMPRA`/`DETALLE_COMPRA` sin restricciones ni triggers residuales (A0).
- **Acción**: pasos A0 a A9.
- **Resultado esperado**: caso bueno → 1 cabecera + 2 detalles; caso con restricción → mensaje único en pantalla, pantalla intacta, y desde conexión nueva **0 cabeceras, 0 detalles, 0 huérfanos**; reintento sin restricción → 1 cabecera + 2 detalles con subtotales coherentes con `CalculoImportesCompra.CalcularSubtotal` (RNF-13).
- **Tiempo máximo**: arranque 20 s; cada registro desde la pantalla 30 s; cada consulta/DDL 30 s (con `LOCK_TIMEOUT` 10 s); escenario completo 120 s por bloque (A2, A5, A8); experimento A completo 8 min (estándar de AGENTS.md §11).
- **Criterio de éxito**: todos los resultados esperados con evidencia registrada en la sección 10.
- **Criterio de fallo**: cualquier conteo distinto; mensaje distinto del de la sección 3.3; pantalla que pierde proveedor/líneas/precios; restricción que sigue creada al final; o vencimiento de cualquier plazo (`FAILED — TIMEOUT / POSIBLE BLOQUEO`, sin reintentar).
- **Detección de bloqueo**: cronómetro del usuario por acción; `LOCK_TIMEOUT 10000` en SSMS; si el botón «Registrar compra» sigue deshabilitado pasados 30 s, o la ventana figura «No responde», es bloqueo (sección 2).
- **Efectos permanentes**: **2 compras con 2 detalles cada una** (A2 y A8). La compra fallida no deja nada. El intento transitorio de A4 deja huecos en los contadores IDENTITY (sin filas).

### T10-B — Catálogo y otras entidades sin cambios (RF-47, RF-48)
- **Precondiciones**: ninguna ejecución de seed/preparar-bd desde la instantánea S0 hasta S1.
- **Acción**: instantánea S0 (A1) antes de la primera compra y S1 (A9) después de la última compra de T10-A; comparar.
- **Resultado esperado**: para P1 y P2, `stock`, `precio_compra`, `precio_venta`, `id_usuario_modifica`, `fecha_modificacion` y `motivo_ajuste_stock` **idénticos**; recuentos de `PRODUCTO`, `PROVEEDOR`, `USUARIO` y `CATEGORIA` idénticos.
- **Tiempo máximo**: 30 s por consulta.
- **Criterio de éxito**: S0 = S1 campo a campo. **Criterio de fallo**: cualquier diferencia (nota: `fecha_modificacion` la mantiene el trigger `TR_PRODUCTO_FECHA_MODIFICACION`; una diferencia indica un `UPDATE` sobre `PRODUCTO`).
- **Detección de bloqueo**: `LOCK_TIMEOUT 10000`. **Efectos permanentes**: ninguno (solo lectura).

### T10-C — Doble envío (RF-58)
- **Precondiciones**: restricción `CK_FALLO_SIMULADO_DETALLE` **eliminada** (A6 verificado); compra válida cargada (proveedor `P`, una línea de P1); `L3` = `MAX(id_compra)` anotado justo antes.
- **Acción**: doble clic rápido en «Registrar compra»; repetir con `Enter` rápido (dos pulsaciones) si el botón tiene el foco. Cada variante parte de una compra cargada nueva.
- **Resultado esperado**: por cada variante, **exactamente 1** compra nueva (`COUNT(*) FROM COMPRA WHERE id_compra > <L3>` = 1) y 1 detalle; el botón se deshabilita mientras el registro está en curso (ComprasForm.cs, 852–865). Nota: tras un éxito la pantalla queda vacía (RF-32), de modo que un segundo clic tardío no puede crear otra compra (falta proveedor, línea 857).
- **Tiempo máximo**: 30 s por registro; 120 s por variante.
- **Criterio de éxito**: conteo = 1 en cada variante. **Criterio de fallo**: conteo ≥ 2, o botón sin deshabilitarse, o plazo vencido.
- **Detección de bloqueo**: sección 2. **Efectos permanentes**: **1 compra por variante** (hasta 2).

### RNF-9 (200 líneas, < 200 ms) — SIN VERIFICAR (descartada por decisión del usuario, 2026-11-01)
T10 **no** la requiere ni define experimento, grabación ni medición. Los 188 productos de volumen (`7790002000001` … `7790002000188`) ya existen en la seed y quedan disponibles si el usuario quiere probar a mano; esa prueba, si se hiciera, queda fuera de T10 y no cuenta para su cierre.

### T10-E — Permisos por rol (RF-8, RF-59, RF-60, RF-61)
- **Precondiciones**: usuarios de prueba por rol (Administrador, Encargado, Vendedor) según el README.
- **Acción**: iniciar sesión con cada rol y mirar el menú/pestañas.
- **Resultado esperado**: **Administrador y Encargado**: pestaña Compras visible y todas las acciones del módulo habilitadas, sin acciones ocultas ni deshabilitadas por rol (RF-8): registrar, quitar/editar líneas, cancelar, «Ver Historial». **Vendedor**: **no ve la pestaña Compras** (RF-8; `PoliticaAcceso.PuedeOperarCompras`, `CapaNegocio/PoliticaAcceso.cs`, línea 80).
- **Tiempo máximo**: arranque 20 s; cambio de sesión y apertura de pestaña 10 s.
- **Criterio de éxito**: lo anterior para los tres roles. **Criterio de fallo**: Vendedor ve la pestaña, o un rol habilitado ve acciones ocultas/deshabilitadas.
- **Límite del alcance**: el rechazo por la **lógica** (`No tiene permisos para registrar compras.` / `No tiene permisos para consultar las compras.`, `CompraNegocio.cs` líneas 60–61; RF-60/RF-61) **no se reproduce desde la interfaz** (el Vendedor no llega a la pantalla); lo cubren las pruebas automáticas de `dotnet test`. No se inventa una prueba manual para él.
- **Efectos permanentes**: ninguno.

### T10-F — Cierre: sin cambios de esquema (RNF-10)
- **Acción**: (1) `SELECT COUNT(*) FROM __EFMigrationsHistory;` antes (en A0) y después (en A9); (2) **acción del usuario**: `git status` — sin archivos nuevos en `CapaDatos/Migrations/` ni cambios en `CapaEntidad`/`CapaDatos`.
- **Resultado esperado**: el recuento es idéntico; `git status` limpio en esas carpetas. **Tiempo máximo**: 30 s. **Éxito/Fallo**: igual/distinto. **Efectos**: ninguno.

---

## 5. Procedimiento A — Atomicidad (T10-A, T10-B, T10-F)

> Cada bloque SQL empieza con `SET LOCK_TIMEOUT 10000;`. Cada pestaña de SSMS es una conexión; **no cierres la sesión de la aplicación** entre A5 y A8.

### A0 — Comprobaciones previas (solo lectura)
```sql
SET LOCK_TIMEOUT 10000;
SELECT DB_NAME() AS base, @@SERVERNAME AS servidor, SUSER_SNAME() AS cuenta, @@TRANCOUNT AS trancount;
-- Esperado: base = DB_SISTEMA_VENTA, servidor = ...\SQLEXPRESS, trancount = 0. Si no, DETENER.

SELECT SCHEMA_NAME(schema_id) AS esquema, name FROM sys.tables
WHERE name IN (N'COMPRA', N'DETALLE_COMPRA', N'PRODUCTO', N'PROVEEDOR', N'USUARIO');
-- Esperado: 5 filas, esquema dbo.

SELECT t.name AS trigger_name, OBJECT_NAME(t.parent_id) AS tabla, t.is_disabled
FROM sys.triggers t WHERE OBJECT_NAME(t.parent_id) IN (N'COMPRA', N'DETALLE_COMPRA');
-- Esperado: 0 filas (descarta el error 334).

SELECT name, is_disabled FROM sys.check_constraints
WHERE parent_object_id = OBJECT_ID(N'dbo.DETALLE_COMPRA');
-- Esperado: 0 filas. Si aparece CK_FALLO_SIMULADO_DETALLE: es un residuo, ir a la sección 9. Si aparecen otras, anotarlas y no tocarlas.

SELECT COUNT(*) AS migraciones FROM __EFMigrationsHistory;   -- anotar (T10-F)
```
Anotar `migraciones` en la sección 10.

### A1 — Elegir datos y tomar la instantánea S0 (solo lectura)
```sql
SET LOCK_TIMEOUT 10000;
SELECT TOP (3) id_proveedor, razon_social, cuit FROM PROVEEDOR WHERE estado = 1 ORDER BY id_proveedor;
SELECT TOP (5) id_producto, codigo, nombre FROM PRODUCTO WHERE estado = 1 ORDER BY id_producto;
SELECT TOP (1) id_usuario, nombre, apellido FROM USUARIO WHERE estado = 1 ORDER BY id_usuario;
```
Elegir `P`, `P1`, `P2` (productos **distintos** y activos; la spec prohíbe repetir producto en una compra, RF-37) y el usuario de la sesión. Anotar `<ID_PROVEEDOR>`, `<ID_P1>`, `<ID_P2>`, `<ID_USUARIO>`. Luego, **instantánea S0** (pegar los números):
```sql
SET LOCK_TIMEOUT 10000;
SELECT id_producto, stock, precio_compra, precio_venta, id_usuario_modifica, fecha_modificacion, motivo_ajuste_stock
FROM PRODUCTO WHERE id_producto IN (<ID_P1>, <ID_P2>) ORDER BY id_producto;
SELECT (SELECT COUNT(*) FROM PRODUCTO) AS productos, (SELECT COUNT(*) FROM PROVEEDOR) AS proveedores,
       (SELECT COUNT(*) FROM USUARIO) AS usuarios, (SELECT COUNT(*) FROM CATEGORIA) AS categorias;
SELECT ISNULL(MAX(id_compra), 0) AS L1 FROM COMPRA;
```
Anotar S0 (copiar el resultado a la sección 10) y **`L1`** como número.

### A2 — Caso bueno / control negativo (**PERMANENTE**: 1 compra + 2 detalles)
En la aplicación: elegir `P`; cargar dos líneas, en este orden:
- P1: cantidad `3`, precio de compra `10,33`, precio de venta `20,00` (> 0; no se guarda).
- P2: cantidad `1`, precio de compra `10,5`, precio de venta `20,00`.

Subtotales esperados por la regla de negocio: 3 × 10,33 = **30,99** y 1 × 10,5 = **10,50**; total **41,49**. Pulsar «Registrar compra». Esperado en pantalla: «Compra N° {id} registrada exitosamente.» y pantalla vacía (RF-32). Plazo: 30 s.

Desde una **conexión nueva** (con `<L1>` literal):
```sql
SET LOCK_TIMEOUT 10000;
SELECT COUNT(*) AS cabeceras FROM COMPRA WHERE id_compra > <L1>;                 -- Esperado: 1
SELECT COUNT(*) AS detalles  FROM DETALLE_COMPRA WHERE id_compra > <L1>;         -- Esperado: 2
SELECT c.id_compra, c.id_usuario, c.id_proveedor, c.monto_total, c.estado, c.fecha_registro,
       (SELECT SUM(d.subtotal) FROM DETALLE_COMPRA d WHERE d.id_compra = c.id_compra) AS suma_subtotales
FROM COMPRA c WHERE c.id_compra > <L1>;
-- Esperado: monto_total = suma_subtotales = 41.49; estado = 1; usuario y proveedor = los elegidos; fecha ≈ hora del equipo.
SELECT d.id_producto, p.codigo, d.cantidad, d.precio_compra, d.subtotal, d.cantidad * d.precio_compra AS esperado
FROM DETALLE_COMPRA d JOIN PRODUCTO p ON p.id_producto = d.id_producto
WHERE d.id_compra > <L1> ORDER BY d.id_detalle_compra;
-- Esperado: subtotal = esperado en las 2 filas (30.99 y 10.50).
```
Luego, en la aplicación, abrir «Ver Historial» (modal `DetalleCompraForm`, solo lectura): la compra aparece en su posición (más reciente primero, RF-50/RF-52), y al elegirla se ve su detalle **sin precio de venta**. Cerrar el modal: no altera nada.

Si el caso bueno falla, **detener T10**: sin línea base no se puede interpretar el fallo inyectado.

### A3 — Restablecer referencia (solo lectura)
```sql
SET LOCK_TIMEOUT 10000;
SELECT ISNULL(MAX(id_compra), 0) AS L2 FROM COMPRA;
```
Anotar **`L2`** como número.

### A4 — Crear el mecanismo y comprobarlo (¡deja la restricción creada!)

**A4.1 Crear la restricción (DDL temporal, lote propio, ejecutar solo este bloque)**:
```sql
SET LOCK_TIMEOUT 10000;
GO
ALTER TABLE dbo.DETALLE_COMPRA WITH NOCHECK
    ADD CONSTRAINT CK_FALLO_SIMULADO_DETALLE CHECK (id_producto <> <ID_P2>);
GO
```
> **ADVERTENCIA: a partir de aquí `CK_FALLO_SIMULADO_DETALLE` existe. Toda compra de P2 fallará hasta que ejecutes A6.** Si el `ALTER` agota el `LOCK_TIMEOUT` (error 1222): `FAILED — TIMEOUT / POSIBLE BLOQUEO`, no reintentar; aplicar la sección 9 y revisar quién tiene abierta una transacción.

**A4.2 Comprobación previa del mecanismo (solo metadatos, solo lectura)**:
```sql
SET LOCK_TIMEOUT 10000;
SELECT cc.name, OBJECT_NAME(cc.parent_object_id) AS tabla, cc.is_disabled, cc.is_not_trusted, cc.definition
FROM sys.check_constraints cc WHERE cc.name = N'CK_FALLO_SIMULADO_DETALLE';
-- Esperado: 1 fila; tabla = DETALLE_COMPRA; is_disabled = 0; is_not_trusted = 1; definition con [id_producto]<>(<ID_P2>).
```

**A4.3 Comprobación del comportamiento: inserción transitoria con `ROLLBACK`** (escritura transitoria; **ejecutar el bloque completo de una vez**, nunca solo el `BEGIN TRAN`: una transacción abierta bloquearía a la aplicación). Consume un valor IDENTITY de `COMPRA` y otro de `DETALLE_COMPRA` (quedan huecos de numeración, sin filas):
```sql
SET LOCK_TIMEOUT 10000;
BEGIN TRAN;
BEGIN TRY
    INSERT INTO COMPRA (id_usuario, id_proveedor, monto_total, estado, fecha_registro)
    VALUES (<ID_USUARIO>, <ID_PROVEEDOR>, 1.00, 1, SYSDATETIME());
    DECLARE @c INT = SCOPE_IDENTITY();

    INSERT INTO DETALLE_COMPRA (id_compra, id_producto, precio_compra, cantidad, subtotal)
    VALUES (@c, <ID_P1>, 1.00, 1, 1.00);
    PRINT 'P1: insertado (transitorio) - correcto, la restriccion no lo afecta';

    INSERT INTO DETALLE_COMPRA (id_compra, id_producto, precio_compra, cantidad, subtotal)
    VALUES (@c, <ID_P2>, 1.00, 1, 1.00);
    PRINT 'FAIL: el INSERT de P2 NO fallo - el mecanismo no funciona';
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS numero_error, ERROR_MESSAGE() AS mensaje;
END CATCH;
IF @@TRANCOUNT > 0 ROLLBACK TRAN;
SELECT @@TRANCOUNT AS trancount_final;
GO
SET LOCK_TIMEOUT 10000;
SELECT COUNT(*) AS cabeceras FROM COMPRA WHERE id_compra > <L2>;          -- Esperado: 0
SELECT COUNT(*) AS detalles  FROM DETALLE_COMPRA WHERE id_compra > <L2>;  -- Esperado: 0
```
Esperado: `numero_error = 547`, el mensaje nombra `CK_FALLO_SIMULADO_DETALLE`, `trancount_final = 0`, y `0`/`0` en los conteos. Si el número es otro, o el mensaje nombra otra restricción, el mecanismo **no** es válido: eliminarlo (A6) y **no** seguir con A5. Si el `INSERT` de P2 no falló (`PRINT 'FAIL...'`), igual: A6 y detener.

### A5 — Caso con fallo (T10-A; no debe dejar nada)
**Tras A2 la pantalla queda vacía (RF-32): hay que cargar toda la compra otra vez**, empezando por elegir el proveedor `P` (la pantalla no agrega líneas sin proveedor elegido) y luego las dos líneas. En la aplicación, con la restricción creada: registrar la **misma compra de A2** (P, P1 ×3 a 10,33; P2 ×1 a 10,5; mismo orden). Plazo: 30 s desde el clic. **Un solo clic**; no repetir (sección 2).
- Esperado en pantalla: «No se pudo completar la operación por un problema con el origen de datos. Intente nuevamente.» (única; sin excepción propagada, RF-56; sin mensaje de límite, RNF-8).
- Esperado: proveedor, líneas, precios y total **siguen en pantalla**; el botón «Registrar compra» vuelve a quedar habilitado (RNF-11).

> **No cierres la aplicación todavía y ve a A6 de inmediato.**

### A6 — Limpieza obligatoria (aunque A5 haya fallado)
```sql
SET LOCK_TIMEOUT 10000;
GO
ALTER TABLE dbo.DETALLE_COMPRA DROP CONSTRAINT CK_FALLO_SIMULADO_DETALLE;
GO
SET LOCK_TIMEOUT 10000;
SELECT name FROM sys.check_constraints WHERE name = N'CK_FALLO_SIMULADO_DETALLE';
-- Esperado: 0 filas. Si devuelve 1, la restricción SIGUE: repetir solo el DROP y volver a verificar. No continuar hasta ver 0 filas.
```
Anotar la hora de eliminación en la sección 10. **No se pasa a A7 con la restricción creada.**

### A7 — Comprobar el rollback (conexión nueva; T10-A)
```sql
SET LOCK_TIMEOUT 10000;
SELECT @@TRANCOUNT AS trancount;                                           -- Esperado: 0
SELECT COUNT(*) AS cabeceras FROM COMPRA WHERE id_compra > <L2>;           -- Esperado: 0
SELECT COUNT(*) AS detalles  FROM DETALLE_COMPRA WHERE id_compra > <L2>;   -- Esperado: 0
SELECT COUNT(*) AS huerfanos FROM DETALLE_COMPRA d
WHERE NOT EXISTS (SELECT 1 FROM COMPRA c WHERE c.id_compra = d.id_compra); -- Esperado: 0
```
Esperado: **0 / 0 / 0**. Cualquier otro valor es **FAIL** de atomicidad (RF-45): conservar la evidencia y no reintentar el registro.

### A8 — Reintento sin el mecanismo (**PERMANENTE**: 1 compra + 2 detalles)
Solo si A7 dio 0/0/0 **y** A6 verificó 0 filas: en la misma pantalla (aún con las líneas), pulsar «Registrar compra» **una vez**. Esperado: «Compra N° {id} registrada exitosamente.» y pantalla vacía (RF-32). Verificar desde una conexión nueva (con `<L2>` literal):
```sql
SET LOCK_TIMEOUT 10000;
SELECT COUNT(*) AS cabeceras FROM COMPRA WHERE id_compra > <L2>;                 -- Esperado: 1
SELECT COUNT(*) AS detalles  FROM DETALLE_COMPRA WHERE id_compra > <L2>;         -- Esperado: 2
SELECT d.id_producto, d.cantidad, d.precio_compra, d.subtotal, d.cantidad * d.precio_compra AS esperado
FROM DETALLE_COMPRA d WHERE d.id_compra > <L2> ORDER BY d.id_detalle_compra;     -- Esperado: subtotal = esperado (30.99 y 10.50)
SELECT c.monto_total, (SELECT SUM(subtotal) FROM DETALLE_COMPRA WHERE id_compra = c.id_compra) AS suma
FROM COMPRA c WHERE c.id_compra > <L2>;                                           -- Esperado: 41.49 = 41.49
```
Los subtotales deben coincidir con `CalculoImportesCompra.CalcularSubtotal` (RNF-13). Abrir «Ver Historial»: la compra aparece (RF-52) y su detalle no muestra precio de venta.

### A9 — Instantánea S1 y cierre (solo lectura)
Repetir las consultas de S0 (A1) → **S1** y comparar con S0 (T10-B). Repetir `SELECT COUNT(*) FROM __EFMigrationsHistory;` (T10-F). **Acción del usuario**: `git status` (R-9). Cerrar con una última verificación de que la restricción no existe (consulta de A6, 0 filas).

---

## 6. Procedimiento C — Doble envío (T10-C)

Con la restricción eliminada (A6 en 0 filas):
```sql
SET LOCK_TIMEOUT 10000;
SELECT ISNULL(MAX(id_compra), 0) AS L3 FROM COMPRA;     -- anotar L3 antes de cada variante
```
- **C1 (doble clic)**: cargar proveedor `P` y una línea de P1 (cantidad 1, precios válidos). Hacer **doble clic rápido** en «Registrar compra». Esperado: un solo mensaje de éxito; botón deshabilitado durante el guardado.
- **C2 (Enter rápido)**: cargar la misma compra de nuevo; con el foco en el botón, pulsar `Enter` dos veces rápido.
- Tras cada variante, con `<L3>` literal:
```sql
SET LOCK_TIMEOUT 10000;
SELECT COUNT(*) AS cabeceras FROM COMPRA WHERE id_compra > <L3>;           -- Esperado: 1
SELECT COUNT(*) AS detalles  FROM DETALLE_COMPRA WHERE id_compra > <L3>;   -- Esperado: 1
```
Si el conteo es ≥ 2: **FAIL** de RF-58; no registrar más compras y documentar. Si el plazo de 30 s vence: sección 2 (no repetir el clic: verificar primero con el `COUNT`).

---

## 7. (Sección retirada)

El antiguo procedimiento D (RNF-9, 200 líneas) fue **retirado por decisión del usuario (2026-11-01)**. RNF-9 queda «SIN VERIFICAR». La numeración de las secciones 8 a 12 se conserva para no romper referencias.

---

## 8. Procedimiento E — Permisos por rol (T10-E)

1. Iniciar sesión como **Administrador** (credenciales del README): la pestaña Compras está visible; «Registrar compra», editar, quitar, «Cancelar compra» y «Ver Historial» están habilitados y visibles (RF-8).
2. Cerrar sesión; repetir con **Encargado**: mismo resultado (RF-8).
3. Cerrar sesión; iniciar con **Vendedor**: **no aparece la pestaña Compras** (RF-8; `PoliticaAcceso.PuedeOperarCompras`).
4. Registrar cada resultado y su hora en la sección 10. Plazos: arranque 20 s, apertura de pestaña 10 s.

---

## 9. Si algo sale mal

1. **Detenerse.** No repetir el clic ni reiniciar nada (sección 2). Si hubo timeout, reunir primero la evidencia.
2. **Saber a qué base estás conectado**:
```sql
SELECT DB_NAME() AS base, @@SERVERNAME AS servidor, SUSER_SNAME() AS cuenta, @@TRANCOUNT AS trancount;
```
   Debe ser `DB_SISTEMA_VENTA` en `localhost\SQLEXPRESS`. Si es otra base o servidor, **no ejecutar nada más** (R-1).
3. **Detectar el mecanismo residual**:
```sql
SELECT cc.name, OBJECT_NAME(cc.parent_object_id) AS tabla, cc.is_disabled
FROM sys.check_constraints cc WHERE cc.name = N'CK_FALLO_SIMULADO_DETALLE';
```
4. **Quitarlo** (si devuelve una fila):
```sql
SET LOCK_TIMEOUT 10000;
GO
ALTER TABLE dbo.DETALLE_COMPRA DROP CONSTRAINT CK_FALLO_SIMULADO_DETALLE;
GO
```
   y repetir la consulta del punto 3: debe dar **0 filas**. Mientras no sea así, toda compra de P2 fallará.
5. **Transacciones abiertas** (si la aplicación se colgó o se cerró en medio de un registro): `DBCC OPENTRAN;` (solo lectura). Una conexión cerrada hace que SQL Server deshaga su transacción; si queda abierta una transacción de SSMS, cerrar esa pestaña (o `ROLLBACK` en ella).
6. **Compras fallidas que dejaron datos** (no debería ocurrir): `SELECT COUNT(*) FROM COMPRA WHERE id_compra > <L2>;` y el recuento de huérfanos de A7. Registrar el hallazgo como **defecto confirmado** con la evidencia; **no borrar filas** (regla de AGENTS.md). Informar al coordinator.
7. **Error 334 en alguna prueba**: indica un trigger habilitado en `DETALLE_COMPRA`; repetir la consulta de triggers de A0 y no continuar.

---

## 10. Registro de resultados (rellena el usuario; vacío a propósito)

| Paso | Fecha y hora inicio | Duración | Valores literales usados | Conteos / resultados | Mensaje observado | OK / FAIL |
|---|---|---|---|---|---|---|
| A0 base y triggers | | | | | | |
| A1 datos elegidos y S0 | | | `<ID_PROVEEDOR>`, `<ID_P1>`, `<ID_P2>`, `<ID_USUARIO>`, `L1` | | | |
| A2 caso bueno | | | `L1` | cabeceras / detalles / total | | |
| A3 referencia | | | `L2` | | | |
| A4.1 creación del CK | | | `<ID_P2>` | | | |
| A4.2 metadatos | | | | | | |
| A4.3 prueba con ROLLBACK | | | | n.º de error / trancount / 0-0 | | |
| A5 caso con fallo | | | | | | |
| A6 eliminación del CK (hora) | | | | filas en `sys.check_constraints` | | |
| A7 rollback | | | `L2` | cabeceras / detalles / huérfanos | | |
| A8 reintento | | | `L2` | cabeceras / detalles / subtotales / total | | |
| A9 S1 y migraciones | | | | S0 = S1; migraciones antes/después; `git status` (usuario) | | |
| C1 doble clic | | | `L3` | cabeceras / detalles | | |
| C2 Enter rápido | | | `L3` | cabeceras / detalles | | |
| E Administrador / Encargado / Vendedor | | | | | | |

Marcar siempre `FAILED — TIMEOUT / POSIBLE BLOQUEO` donde corresponda (sin reintento).

---

## 11. Operaciones sobre la BD (resumen para Reviewer y para el usuario)

| Paso | Tipo | Sentencia | Alcance | Riesgo |
|---|---|---|---|---|
| A0, A1, A3, A9, C (L3) | Lectura | `SELECT` sobre `sys.*`, `COMPRA`, `DETALLE_COMPRA`, `PRODUCTO`, `PROVEEDOR`, `USUARIO`, `CATEGORIA`, `__EFMigrationsHistory` | Solo consulta | Bajo (esperas acotadas por `LOCK_TIMEOUT`) |
| A2, A8 | Escritura permanente desde la aplicación | `INSERT` de `COMPRA` + 2 `DETALLE_COMPRA` (los hace la aplicación) | 1 compra con 2 detalles por paso | **Permanente**: no hay `DELETE` físico; quedan compras de prueba |
| A4.1 | DDL temporal | `ALTER TABLE dbo.DETALLE_COMPRA WITH NOCHECK ADD CONSTRAINT CK_FALLO_SIMULADO_DETALLE CHECK (id_producto <> <ID_P2>)` | Tabla `DETALLE_COMPRA` | **Alto si se olvida**: toda compra de P2 falla. Bloqueo breve de esquema al crear |
| A4.2 | Lectura | `SELECT` sobre `sys.check_constraints` | Metadatos | Bajo |
| A4.3 | Escritura transitoria (con `ROLLBACK`) | `INSERT` de `COMPRA` y 2 de `DETALLE_COMPRA` dentro de `BEGIN TRAN ... ROLLBACK` | Ninguna fila persiste | Medio: deja huecos en IDENTITY; transacción abierta si se ejecuta incompleto (ejecutar el bloque entero) |
| A5 | Escritura desde la aplicación (esperada sin efecto) | Intento de `INSERT` que debe revertirse | Ninguna fila persiste | Bajo, verificado en A7 |
| A6, sección 9 | Limpieza (DDL) | `ALTER TABLE dbo.DETALLE_COMPRA DROP CONSTRAINT CK_FALLO_SIMULADO_DETALLE` | Tabla `DETALLE_COMPRA` | Bajo; obligatorio |
| A7 | Lectura | `COUNT` y `NOT EXISTS` | Solo consulta | Bajo |
| C1, C2 | Escritura permanente desde la aplicación | `INSERT` de 1 compra + 1 detalle por variante | Hasta 2 compras | **Permanente** |
| Sección 9 (`DBCC OPENTRAN`) | Lectura | `DBCC OPENTRAN` | Servidor/base | Bajo |

Ningún paso ejecuta `DELETE`, `TRUNCATE`, `DROP TABLE`, migraciones, ni toca `PRODUCTO` por SQL.

---

## 12. Referencias de archivo (vigentes al 2026-11-01)

- Especificación: `specs/005-carga-compras/spec.md` (RF-8, RF-32, RF-36, RF-37, RF-44 a RF-48, RF-52, RF-56, RF-58, RF-59 a RF-61, RNF-4, RNF-10, RNF-11, RNF-13; D-17; RNF-9 descartada, ver arriba).
- Tareas: `specs/005-carga-compras/tasks.md` (T9b en las líneas 109–112; T10 en las líneas 114–138). Plan: `specs/005-carga-compras/plan.md` §7 (pasos transaccionales) y §9.1.
- Checklist de RNF (evidencia de código y estados): `specs/005-carga-compras/checklist-rnf.md`. Verificación visual: `specs/005-carga-compras/checklist-visual-unificado.md` (no se modifica aquí).
- Código: `CapaNegocio/CompraNegocio.cs` (`RegistrarCompraAsync` 325–516; `ObtenerHistorialAsync` 898; `ObtenerCompraConDetalleAsync` 997), `CapaNegocio/Compras/CalculoImportesCompra.cs`, `CapaNegocio/PoliticaAcceso.cs`, `CapaPresentacion/Compras/ComprasForm.cs`, `CapaPresentacion/Compras/DetalleCompraForm.cs`, `CapaPresentacion/Program.cs`, `CapaPresentacion/Inicio/inicio.cs`, `CapaDatos/AppDbContext.cs`, `CapaEntidad/Compra.cs`, `CapaEntidad/DetalleCompra.cs`, `CapaEntidad/Producto.cs`.
- Lo que antes era la sección «Checklist RNF» de este archivo está ahora **solo** en `checklist-rnf.md`, para no mantener dos copias con referencias que se desactualizan.
