# SistemaVentaStock — Taller de Programación 2 (G70)

Sistema de gestión comercial de escritorio (WinForms, C#, .NET) orientado a comercios minoristas. Resuelve control de inventario, compras a proveedores, punto de venta (POS) en red local (LAN) y trazabilidad de operaciones.

> Materia: Taller de Programación 2 — FaCENA, UNNE. Trabajo grupal (G70).
> La especificación completa de requisitos (ERS) se encuentra en el documento interno del equipo y no se versiona en este repositorio.

## Stack

- **Lenguaje / Framework:** C# · .NET 10 (`net10.0-windows`) · Windows Forms
- **IDE:** Visual Studio Community (compilación en Windows / Tiny10 VM)
- **Base de datos:** SQL Server Express, con acceso mediante **EF Core** (`Microsoft.EntityFrameworkCore.SqlServer`). Las claves se almacenan con `BCrypt.Net-Next` 4.2.0.
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

- Windows 10/11 con **Visual Studio Community 2022+** (workload ".NET Desktop Development").
- **SQL Server Express** (o LocalDB para desarrollo liviano) instalado y accesible en LAN.
- .NET SDK 10.0.400.

## Cómo compilar y ejecutar

1. Abrir `SistemaVentaStock.slnx` en Visual Studio.
2. Restaurar paquetes NuGet (Build → Restore).
3. Configurar la cadena de conexión a SQL Server en `CapaPresentacion/appsettings.json`.
4. Aplicar las migraciones EF Core para crear o actualizar el esquema de la base de datos; este paso no carga datos de desarrollo.
5. Ejecutar `scripts/seed-desarrollo.sql` cuando se necesiten datos de desarrollo.
6. Compilar y ejecutar (F5).

> En Fedora el proyecto no se compila directamente (WinForms requiere Windows). Usá la VM con Tiny10 + Visual Studio como entorno de compilación.

## Migraciones de EF Core

Desde la raíz del repositorio, restaurá la herramienta local y aplicá las migraciones:

```powershell
dotnet tool restore
dotnet ef database update --project CapaDatos --startup-project CapaDatos --context AppDbContext
```

La migración inicial ya está versionada en `CapaDatos/Migrations`, por lo que normalmente no hace falta volver a ejecutar `migrations add`. Si cambiás el modelo, creá una nueva con `dotnet ef migrations add NombreDescriptivo --project CapaDatos --startup-project CapaDatos --context AppDbContext --output-dir Migrations`. `database update` crea o actualiza el esquema, pero no carga datos de desarrollo.

La factory de diseño usa preferentemente `ConnectionStrings__CadenaSQL` y, si no está definida, `CapaPresentacion/appsettings.json`.

## Autenticación y seed de desarrollo

Las claves de usuarios se almacenan como hashes BCrypt; `UsuarioNegocio` valida el ingreso con `BCrypt.Verify` y nunca compara la clave ingresada con texto plano. El proyecto de negocio declara directamente `BCrypt.Net-Next` 4.2.0, compatible con `net10.0`.

`scripts/seed-desarrollo.sql` conserva la transacción y el upsert de roles y usuarios, y asigna hashes BCrypt válidos a los usuarios de desarrollo con DNI `90000001`, `90000002` y `90000003`. El script no contiene las claves de esos usuarios en texto plano; ejecutalo después de aplicar las migraciones para cargar los datos de prueba.

Credenciales de desarrollo: `90000001 / Admin123!`, `90000002 / Encargado123!` y `90000003 / Vendedor123!`. Son ficticias y no deben reutilizarse fuera de la base local.

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
