# SistemaVentaStock — Taller de Programación 2 (G70)

Sistema de gestión comercial de escritorio (WinForms, C#, .NET) orientado a comercios minoristas. Resuelve control de inventario, compras a proveedores, punto de venta (POS) en red local (LAN) y trazabilidad de operaciones.

> Materia: Taller de Programación 2 — FaCENA, UNNE. Trabajo grupal (G70).

## Stack

- **Lenguaje / Framework:** C# · .NET 10 (`net10.0-windows`) · Windows Forms
- **IDE:** Visual Studio Community (compilación y ejecución en Windows)
- **Base de datos:** SQL Server Express o LocalDB, con acceso mediante **EF Core** (`Microsoft.EntityFrameworkCore.SqlServer`). Las claves se almacenan con `BCrypt.Net-Next` 4.2.0.
- **Tests:** xUnit y EF Core InMemory para probar la lógica de negocio sin depender de una base externa.
- **Arquitectura:** En capas: interfaz WinForms, lógica de negocio y acceso a datos con EF Core.

## Estructura del proyecto

```
proyecto_taller_programacion_2/
├── CapaEntidad/                                # Entidades del dominio
├── CapaDatos/                                  # EF Core, DbContext y migraciones
├── CapaNegocio/                                # Lógica de negocio
├── CapaPresentacion/                           # UI WinForms y configuración
├── CapaNegocio.Tests/                          # Pruebas automatizadas
├── scripts/seed-desarrollo.sql                 # Datos de desarrollo
├── SistemaVentaStock.slnx                      # Solución principal
├── global.json                                 # SDK fijado
└── README.md
```

> La estructura crecerá por capas a medida que avancen los sprints (ver Roadmap).

## Requisitos

- Windows 10/11 con una versión de **Visual Studio** compatible con .NET 10 y el workload ".NET Desktop Development".
- **SQL Server Express** o LocalDB instalado y disponible localmente.
- .NET SDK 10.0.400.
- Herramienta `dotnet-ef` 10.0.11.

## Cómo compilar y ejecutar

1. Clonar el repositorio y abrir `SistemaVentaStock.slnx` en Visual Studio.
2. Restaurar los paquetes NuGet desde Visual Studio o desde PowerShell:

   ```powershell
   dotnet restore
   ```

3. Configurar la cadena de conexión a SQL Server en `CapaPresentacion/appsettings.json`.
4. Aplicar las migraciones de EF Core para crear o actualizar el esquema de la base de datos.
5. Ejecutar `scripts/seed-desarrollo.sql` para cargar los datos de desarrollo.
6. Compilar y ejecutar con F5 desde Visual Studio.

## Migraciones de EF Core

Desde PowerShell, instalar la herramienta de EF Core una sola vez:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.11
```

Desde la raíz del repositorio, aplicar las migraciones pendientes:

```powershell
dotnet ef database update --project CapaDatos --startup-project CapaDatos --context AppDbContext
```

Las migraciones versionadas se encuentran en `CapaDatos/Migrations`. `database update` crea o actualiza el esquema, pero no carga datos de desarrollo.

Si cambiás una entidad o una regla del modelo, generá una nueva migración:

```powershell
dotnet ef migrations add NombreDescriptivo `
  --project CapaDatos `
  --startup-project CapaDatos `
  --context AppDbContext `
  --output-dir Migrations
```

La factory de diseño usa preferentemente `ConnectionStrings__CadenaSQL` y, si no está definida, `CapaPresentacion/appsettings.json`.

### Configuración de SQL Server

La configuración predeterminada utiliza una instancia local de SQL Server Express:

```json
{
  "ConnectionStrings": {
    "CadenaSQL": "Server=localhost\\SQLEXPRESS;Database=DB_SISTEMA_VENTA;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

Si se utiliza LocalDB, reemplazar el servidor por `(localdb)\\MSSQLLocalDB`.

La cadena debe apuntar a una instancia accesible con las credenciales del usuario de Windows.

## Autenticación y seed de desarrollo

Las claves de usuarios se almacenan como hashes BCrypt; `UsuarioNegocio` valida el ingreso con `BCrypt.Verify` y nunca compara la clave ingresada con texto plano. El proyecto de negocio declara directamente `BCrypt.Net-Next` 4.2.0, compatible con `net10.0`.

`scripts/seed-desarrollo.sql` conserva la transacción y el upsert de roles y usuarios, y asigna hashes BCrypt válidos a los usuarios de desarrollo con DNI `90000001`, `90000002` y `90000003`. El script no contiene las claves de esos usuarios en texto plano; ejecutalo después de aplicar las migraciones para cargar los datos de prueba.

Desde PowerShell, ubicado en la raíz del repositorio, ejecutarlo con:

```powershell
sqlcmd -S ".\SQLEXPRESS" -E -b -i ".\scripts\seed-desarrollo.sql"
```

`-E` utiliza la autenticación integrada de Windows, `-b` hace que `sqlcmd` informe un error si el script falla y `-i` indica el archivo SQL de entrada.

Credenciales de desarrollo: `90000001 / Admin123!`, `90000002 / Encargado123!` y `90000003 / Vendedor123!`. Son ficticias y no deben reutilizarse fuera de la base local.

## Tests

Los tests automatizados verifican autenticación, bloqueo por intentos fallidos, altas, modificaciones, bajas lógicas, activaciones, validaciones, duplicados y permisos por rol.

Ejecutarlos desde la raíz del repositorio:

```powershell
dotnet test CapaNegocio.Tests\CapaNegocio.Tests.csproj
```

Estos tests utilizan EF Core InMemory, por lo que no requieren iniciar SQL Server.

## Roadmap (6 sprints — 12 semanas)

| Sprint | Foco | Entregables principales |
|--------|------|--------------------------|
| **1** | Arquitectura base, seguridad y roles | Esquema BD inicial, login con hash + bloqueo a 3 intentos, RBAC y menú dinámico |
| **2** | Catálogos e inventario | ABM de Categorías y Productos (código de barras, precios, stock), búsqueda y alertas críticas |
| **3** | Terceros y compras | ABM Clientes/Proveedores, registro de Compras (cabecera + detalle) con incremento atómico de stock y actualización de precios |
| **4** | POS y facturación | Interfaz POS por teclado/lector, validación estricta de stock (`SELECT … FOR UPDATE`), cálculo de vuelto, emisión de ticket |
| **5** | Anulaciones y trazabilidad | Anulación de compras/ventas solo Admin con reversión de stock, bajas lógicas estrictas (`Estado = 0`), logs y rollback |
| **6** | Reportes y pulido | Histórico filtrado por fechas/estado, reimpresión de tickets, pruebas LAN, documentación y manual de usuario |

Roles: **Administrador** (gestión total + anulaciones), **Encargado** (catálogo, proveedores y compras), **Vendedor** (POS y clientes).

## Reglas de negocio clave

- Bajas lógicas obligatorias (`Estado = 0/1`), prohibido `DELETE` físico.
- Inmutabilidad del comprobante: `DocumentoCliente` y `NombreCliente` se copian en la cabecera de `VENTA`.
- Compras y ventas bajo transacción atómica (ACID) con `ROLLBACK` ante fallos.
- Validación estricta de stock: `Stock >= Cantidad` antes de agregar al carrito/confirmar.

## Contribuir

Trabajo en equipo — coordinar branches por sprint/feature y mensajes de commit descriptivos.

## Licencia

Proyecto académico — uso educativo.
