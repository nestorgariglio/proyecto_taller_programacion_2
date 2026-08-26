# SistemaVentaStock — Taller de Programación 2 (G70)

Sistema de gestión comercial de escritorio (WinForms, C#, .NET) orientado a comercios minoristas. Resuelve control de inventario, compras a proveedores, punto de venta (POS) en red local (LAN) y trazabilidad de operaciones.

> Materia: Taller de Programación 2 — FaCENA, UNNE. Trabajo grupal (G70).
> La especificación completa de requisitos (ERS) se encuentra en el documento interno del equipo y no se versiona en este repositorio.

## Stack

- **Lenguaje / Framework:** C# · .NET 10 (`net10.0-windows`) · Windows Forms
- **IDE:** Visual Studio Community (compilación en Windows / Tiny10 VM)
- **Base de datos:** SQL Server Express (motor relacional con transacciones ACID). Acceso vía **EF Core** (`Microsoft.EntityFrameworkCore.SqlServer`) + `BCrypt.Net-Next` para hash de claves.
- **Arquitectura:** En capas — UI (WinForms) / Lógica de Negocio / Acceso a Datos (EF Core + repositorios DAO), manejo centralizado de excepciones y `log.txt`.

## Estructura del proyecto

```
proyecto_taller_programacion_2/
├── .vs/                                        # Caché de Visual Studio (ignorado por git)
├── proyecto_taller_programacion_2/
│   ├── Program.cs                              # Punto de entrada
│   ├── Form1.cs / Form1.Designer.cs            # Form inicial (template)
│   └── proyecto_taller_programacion_2.csproj   # Proyecto WinForms (.NET 10)
├── proyecto_taller_programacion_2.slnx         # Solución
├── .gitignore
└── README.md
```

> La estructura crecerá por capas a medida que avancen los sprints (ver Roadmap).

## Requisitos

- Windows 10/11 con **Visual Studio Community 2022+** (workload ".NET Desktop Development").
- **SQL Server Express** (o LocalDB para desarrollo liviano) instalado y accesible en LAN.
- .NET SDK 10.

## Cómo compilar y ejecutar

1. Abrir `proyecto_taller_programacion_2.slnx` en Visual Studio.
2. Restaurar paquetes NuGet (Build → Restore).
3. Configurar la cadena de conexión a SQL Server en `App.config` (se agregará en el Sprint 1).
4. Aplicar las migraciones EF Core (Package Manager Console → `Update-Database`) para crear el esquema y datos semilla.
5. Compilar y ejecutar (F5).

> En Fedora el proyecto no se compila directamente (WinForms requiere Windows). Usá la VM con Tiny10 + Visual Studio como entorno de compilación.

## Roadmap (6 sprints — 12 semanas)

| Sprint | Foco | Entregables principales |
|--------|------|--------------------------|
| **1** | Arquitectura base, seguridad y roles | Esquema BD (8 tablas), Login con hash + bloqueo a 3 intentos, RBAC y menú dinámico |
| **2** | Catálogos e inventario |ABM de Categorías y Productos (código de barras, precios, stock), búsqueda y alertas críticas |
| **3** | Terceros y compras | ABM Clientes/Proveedores, registro de Compras (cabecera + detalle) con incremento atómico de stock y actualización de precios |
| **4** | POS y facturación | Interfaz POS por teclado/lector, validación estricta de stock (`SELECT … FOR UPDATE`), cálculo de vuelto, emisión de ticket |
| **5** | Anulaciones y trazabilidad | Anulación de compras/ventas solo Admin con reversión de stock, bajas lógicas estrictas (`Estado = 0`), logs y rollback |
| **6** | Reportes y pulido | Histórico filtrado por fechas/estado, reimpresión de tickets, pruebas LAN, documentación y manual de usuario |

Roles: **Administrador** (gestión total + anulaciones), **Encargado de Compras** (catálogo, proveedores y compras), **Cajero** (POS y clientes).

## Reglas de negocio clave

- Bajas lógicas obligatorias (`Estado = 0/1`), prohibido `DELETE` físico.
- Inmutabilidad del comprobante: `DocumentoCliente` y `NombreCliente` se copian en la cabecera de `VENTA`.
- Compras y ventas bajo transacción atómica (ACID) con `ROLLBACK` ante fallos.
- Validación estricta de stock: `Stock >= Cantidad` antes de agregar al carrito/confirmar.

## Contribuir

Trabajo en equipo — coordinar branches por sprint/feature y mensajes de commit descriptivos.

## Licencia

Proyecto académico — uso educativo.
