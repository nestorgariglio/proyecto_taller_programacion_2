# README - preparar-bd.ps1

## 1) ¿Qué hace?

`scripts/preparar-bd.ps1` prepara la base de datos de desarrollo en dos pasos ordenados y finaliza con una verificación de control:

- **Paso 1 (opcional si se usa `-SoloDatos`)**: Aplica las migraciones de Entity Framework Core. Antes restaura la herramienta `dotnet-ef` definida en `.config/dotnet-tools.json` con `dotnet tool restore` y luego ejecuta `dotnet ef database update --project CapaDatos --startup-project CapaDatos --context AppDbContext`. Esto crea o actualiza el esquema de `DB_SISTEMA_VENTA`.
- **Paso 2 (siempre)**: Ejecuta el script `scripts/seed-desarrollo.sql` contra la base con `sqlcmd`, usando autenticación integrada y forzando UTF-8 con `-f 65001` para evitar la lectura OEM de los caracteres con tilde. El script asigna `id_usuario` de los productos al **Administrador** (DNI `90000001`) mediante variable y es idempotente. Toda la seed se ejecuta dentro de **una única transacción** (`TRY/CATCH` con `ROLLBACK`): si algo falla no queda nada a medias y `sqlcmd -b` devuelve un código de error distinto de 0.
- **Paso 3 (verificación)**: Consulta los conteos y muestra una línea como `Categorías: 6 (activas: 5) | Productos: 201 (activos: 200)`. En caso de que los conteos sean 0, el script emite una advertencia (no compara contra valores fijos).

### Contenido de la seed (`seed-desarrollo.sql`)

Sobre una base limpia (recién migrada), deja exactamente:

| Entidad | Cantidad | Detalle |
|---|---|---|
| Roles | 3 | Administrador, Encargado, Vendedor |
| Usuarios | 4 | DNI `90000001` a `90000004` (Administrador, Encargado, Vendedora y un Encargado adicional). Solo hashes BCrypt de prueba; el script no contiene claves en texto plano |
| Categorías | 6 | 5 activas + 1 inactiva (`Kiosco`) |
| Productos | 201 | 12 base (`7790001...`), 188 de volumen (`7790002000001` a `7790002000188`, «Producto Volumen 001»…) y 1 inactivo (`7790003000001`, «Producto Inactivo Prueba») => 200 activos |
| Clientes | 12 | `CLI0000001` a `CLI0000012` (9 activos + 3 inactivos) |
| Proveedores | 11 | 3 base (2 activos + 1 inactivo «Tecnologia Norte») + 8 activos adicionales ficticios |
| Compras | 0 | La seed **no** inserta `COMPRA` ni `DETALLE_COMPRA` (RF-48: solo la aplicación registra compras) |

> **ADVERTENCIA - sobrescritura**: la seed **sobrescribe** los datos de prueba (usuarios `90000001`-`90000004`, categorías, productos `7790001/2/3...`, clientes y proveedores de la seed) en cada ejecución: restablece estado, stock, precios y nombres. **No ejecutarla durante T10 ni entre sus pasos**, porque alteraría el stock y el estado de los registros y falsearía la verificación de RF-47.

## 2) ¿Qué NO hace?

- No crea ni modifica el esquema a mano. Ese rol es exclusivo de las migraciones de EF Core.
- No borra filas. No ejecuta `DELETE`, `DROP` ni `TRUNCATE`. La semilla (`seed-desarrollo.sql`) es **idempotente** (usa `IF NOT EXISTS`, `UPDATE` si hay cambios o `IF @@ROWCOUNT=0 THEN INSERT`, así como comprobaciones con `OBJECT_ID`).
- No siembra datos al arrancar la aplicación. La carga de datos de desarrollo es una acción explícita de este script.
- No imprime la cadena de conexión completa. Solo se extraen servidor (`DataSource`) y base (`InitialCatalog`) con `SqlConnectionStringBuilder`.

## 3) Cómo funciona

1. **Orden lógico**: esquema (`EF Core`, migraciones) → datos (`seed`) → verificación (`sqlcmd`). Si las migraciones fallan, el script se detiene y no ejecuta la seed.
2. **Idempotencia**: la re-ejecución no duplica registros. Al ejecutar una segunda vez, los conteos deben mantenerse en `Categorías: 6 (activas: 5) | Productos: 201 (activos: 200)` (sobre una base limpia; si la base ya tenía otros registros, los totales los incluyen).
3. **Lectura desde appsettings**: el script lee `ConnectionStrings:CadenaSQL` de `CapaPresentacion/appsettings.json` (por defecto). Se respeta la cadena existente: `Server=localhost\SQLEXPRESS;Database=DB_SISTEMA_VENTA;Integrated Security=true;TrustServerCertificate=true;`. Solo admite autenticación integrada.
4. **UTF-8 para `sqlcmd`**: para leer/escribir correctamente tildes y `ñ`, el script usa `sqlcmd -f 65001` (entrada/salida UTF-8) y, en la verificación, vuelca temporalmente a un archivo UTF-8 para mostrarlo correctamente en la consola.
5. **Validaciones**: verifica que `dotnet` y `sqlcmd` existan en el `PATH`, que los archivos de seed y appsettings existan y que la cadena de conexión sea válida.

## 4) Por qué se hace así

- **Fuente de verdad del esquema**: EF Core (migraciones) es la única forma autorizada para crear/modificar el esquema en este proyecto.
- **Esquema y datos separados**: los datos de desarrollo se mantienen fuera del modelo (no se usa `HasData`). Esto evita acoplar datos de prueba a las migraciones y permite evolucionar la semilla sin generar migraciones innecesarias.
- **Idempotencia**: permite restaurar o actualizar el set de desarrollo de forma reproducible sin eliminar información existente.
- **Evitar mojibake**: la experiencia previa mostró que `sqlcmd` puede interpretar el `.sql` con la página de códigos OEM. Guardar con **BOM UTF-8** y forzar `-f 65001` soluciona este problema.
- **Cero DELETE/DROP**: se respeta la regla del usuario ("nunca borrar filas físicamente; preguntar antes de ejecutar DELETE/DROP").
- **Solo entorno de desarrollo**: este script no está pensado para producción.

## 5) Requisitos previos

- Windows (PowerShell 5.1 o 7).
- .NET SDK 8.
- SQL Server Express disponible en `localhost\SQLEXPRESS` y servicio en ejecución.
- `sqlcmd` disponible en el `PATH` (SQL Server Command Line Utilities).
- Herramienta `dotnet-ef` declarada en `.config/dotnet-tools.json` (se restaura con `dotnet tool restore`).
- `scripts/seed-desarrollo.sql` guardado con **BOM UTF-8**.
- `CapaPresentacion/appsettings.json` con `ConnectionStrings:CadenaSQL` válida y `Integrated Security=true`.

## 6) Uso

Desde la raíz del repositorio:

### Preparación completa (esquema + seed + verificación)

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\preparar-bd.ps1
```

### Solo datos (omite migraciones; útil cuando el esquema ya está actualizado)

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\preparar-bd.ps1 -SoloDatos
```

### Pasando rutas explícitas (opcionales)

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\preparar-bd.ps1 `
  -ArchivoSeed .\scripts\seed-desarrollo.sql `
  -AppSettings .\CapaPresentacion\appsettings.json
```

### Cómo re-ejecutarlo de forma segura

Volvé a correrlo: `-SoloDatos` o completo. Como la seed es idempotente, no se duplicarán `ROL`, `USUARIO`, `CATEGORIA`, `PRODUCTO`, `CLIENTE` ni `PROVEEDOR`. Al re-ejecutar, los conteos deben seguir siendo `Categorías: 6 (activas: 5) | Productos: 201 (activos: 200)`.

**Importante**: cada re-ejecución restablece a su valor inicial el stock, los precios, los nombres y el estado de los registros de la seed. **No la ejecutes durante T10 ni entre sus pasos** (falsearía la verificación de RF-47) ni en medio de una verificación manual que haya modificado esos datos. Las compras registradas desde la aplicación no se borran (la seed no elimina filas).

## 7) Qué esperar en consola

La ejecución muestra pasos numerados en español, mensajes OK/info y, al final, una línea de verificación. Si todo sale bien, termina con `Listo: base preparada.` y código de salida `0`.

## 8) Comprobar el resultado en SSMS

Podés verificar con estas consultas (opcionales):

```sql
SELECT COUNT(*) AS CantCategorias, SUM(CASE WHEN estado=1 THEN 1 ELSE 0 END) AS Activas FROM dbo.CATEGORIA;
SELECT COUNT(*) AS CantProductos, SUM(CASE WHEN estado=1 THEN 1 ELSE 0 END) AS Activos FROM dbo.PRODUCTO;
SELECT descripcion, estado FROM dbo.CATEGORIA ORDER BY descripcion;
SELECT COUNT(*) AS CantClientes, SUM(CASE WHEN estado=1 THEN 1 ELSE 0 END) AS Activos FROM dbo.CLIENTE;
SELECT COUNT(*) AS CantProveedores, SUM(CASE WHEN estado=1 THEN 1 ELSE 0 END) AS Activos FROM dbo.PROVEEDOR;
SELECT COUNT(*) AS CantUsuarios FROM dbo.USUARIO WHERE dni IN (90000001, 90000002, 90000003, 90000004);
```

Resultado esperado sobre una base limpia: `CantCategorias=6`, `Activas=5`; `CantProductos=201`, `Activos=200` (12 base + 188 de volumen + 1 inactivo); `CantClientes=12`, `Activos=9`; `CantProveedores=11`, `Activos=10` (3 base + 8 adicionales); `CantUsuarios=4`. `Kiosco` debe figurar como inactiva (`estado=0`). `COMPRA` y `DETALLE_COMPRA` no reciben filas de la seed.

## 9) Solución de problemas

| Problema | Causa probable | Solución |
|---|---|---|
| `No se encontró sqlcmd` | `sqlcmd` no está en el PATH | Instalá SQL Server Command Line Tools o agregá `...\Tools\Binn` al PATH. |
| `No se pudo conectar a SQL Server` / login fallido | SQL Server (SQLEXPRESS) apagado, nombre incorrecto o autenticación no integrada | Verificá que el servicio esté iniciado. Revisá `appsettings.json`. Este script solo admite Integrated Security. |
| `dotnet tool restore falló` | Sin manifiesto o sin red para restaurar | Verificá `.config/dotnet-tools.json`. Ejecutá `dotnet tool restore` manualmente. |
| `dotnet ef database update falló` | Esquema no compilable, cadena inválida o SQL Server inaccesible | Corregí errores de compilación (`dotnet build`) o verificá conexión. No se ejecuta la seed si este paso falla. |
| `Las migraciones aún no existen` / `No existe la tabla ...` | Migraciones no aplicadas | Correte el script **completo** (sin `-SoloDatos`). |
| Tildes/ñ rotas en salida o en datos | Archivo `.sql` sin BOM UTF-8 | Guardá `seed-desarrollo.sql` con BOM UTF-8. El script fuerza `-f 65001`. |
| Política de ejecución bloquea el script | PowerShell restringido | Usá `powershell -ExecutionPolicy Bypass -File ...` |
| `Verificación devolvió 0...` | Seed corrió contra otra base o falló silenciosamente | Revisá servidor/base (`DataSource`/`InitialCatalog`) en la salida. Verificá mensajes de `sqlcmd`. |

## 10) Cuándo usar cada modo

- **Completo (sin flags)**: primera vez, esquema desactualizado o después de bajar cambios con nuevas migraciones.
- **`-SoloDatos`**: esquema ya está al día; solo querés re-sembrar o verificar los datos de desarrollo.

## 11) Limitaciones

- Solo para **entorno de desarrollo**. No usar en producción.
- La semilla cubre `ROL`, `USUARIO`, `CATEGORIA`, `PRODUCTO`, `CLIENTE` y `PROVEEDOR` (los necesarios para las pruebas manuales de los ABM y de la carga de compras). No cubre `VENTA`, `DETALLE_VENTA`, `COMPRA` ni `DETALLE_COMPRA` (RF-48).
- Sobrescribe los datos de prueba en cada ejecución (ver advertencia en la sección 1): no usarla durante T10.
- Requiere Windows/SQL Server Express en `localhost\SQLEXPRESS` con autenticación integrada.
