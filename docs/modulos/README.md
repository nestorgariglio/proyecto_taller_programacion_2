# 🎓 Guía de Estudio Rápido para Defensa de Proyecto (Taller II)

> **Propósito**: Resumen ejecutivo y técnico de cada módulo del sistema para repasar antes de defender ante los profesores.  
> **Arquitectura base**: 3 Capas desacopladas (Presentación en Windows Forms con MaterialSkin 2.3.1, Negocio con reglas de dominio puras, Datos con EF Core 10 y Stored Procedures, Entidades compartidas).

---

## Mapa de Módulos para la Defensa

| N° | Módulo | Archivo de Estudio | Responsabilidad Clave |
|---|---|---|---|
| **01** | **Seguridad y Usuarios** | [01-autenticacion-y-usuarios.md](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/docs/modulos/01-autenticacion-y-usuarios.md) | Roles (Admin, Encargado, Vendedor), BCrypt, bloqueo por 3 intentos fallidos, matriz de accesos `PoliticaAcceso`. |
| **02** | **Catálogo y Stock** | [02-productos-y-categorias.md](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/docs/modulos/02-productos-y-categorias.md) | ABM de Categorías y Productos, código de barras único, semáforo de stock crítico configurable, baja lógica. |
| **03** | **Padrones Comerciales** | [03-clientes-y-proveedores.md](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/docs/modulos/03-clientes-y-proveedores.md) | ABM de Clientes y Proveedores, validación CUIT/DNI, consumidor final DNI '0', relación producto-proveedor. |
| **04** | **Compras y Reabastecimiento** | [04-compras-y-reabastecimiento.md](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/docs/modulos/04-compras-y-reabastecimiento.md) | Ciclo desacoplado (`Pendiente` -> `Confirmada` -> `Recibida`), cotización unitaria acordada, impacto automático en stock y costo de reposición. |
| **05** | **Ventas, Caja y Facturación** | [05-ventas-caja-y-facturacion.md](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/docs/modulos/05-ventas-caja-y-facturacion.md) | Carrito POS, procedimiento `sp_RegistrarVenta` transaccional con locks pesimistas (`UPDLOCK, HOLDLOCK`), Factura B, anulación con restitución de stock (HU-13). |
| **06** | **Reportes y Analítica** | [06-reportes-y-metricas.md](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/docs/modulos/06-reportes-y-metricas.md) | Tablero analítico (`ReportesForm`), KPIs de facturación, margen bruto real por línea, ticket promedio, gráficos LiveCharts y vistas SQL (`vw_ResumenDiarioVentas`, `vw_ParticipacionCategorias`). |
| **07** | **Respaldo y Recuperación** | [07-respaldo-y-restauracion.md](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/docs/modulos/07-respaldo-y-restauracion.md) | Backups nativos SQL Server (.bak), exclusión de bases de sistema, restauración con forzado `SINGLE_USER WITH ROLLBACK IMMEDIATE`. |

---

## 🎯 Las 5 Preguntas Típicas de los Profesores

### 1. ¿Cómo manejan la concurrencia cuando dos cajeros venden el mismo producto sin stock?
> **Respuesta**: Se delega al motor de base de datos dentro del Stored Procedure `dbo.sp_RegistrarVenta`. Al momento de validar y descontar el stock, se bloquean las filas de los productos involucrados con los hints `WITH (UPDLOCK, HOLDLOCK)`. Esto asegura que ninguna otra transacción paralela pueda leer ni modificar el stock del producto hasta que termine la venta actual. Si el stock no alcanza, se lanza una excepción y la transacción se aborta (`ROLLBACK`).

### 2. ¿Por qué el ciclo de compras está desacoplado en Pendiente, Confirmada y Recibida?
> **Respuesta**: Porque en el mundo real, cuando le enviás una solicitud de reabastecimiento a un proveedor (estado `Pendiente`), no sabés el precio final unitario ni tenés la mercadería física en el local. Cuando el proveedor responde con la cotización, el encargado ingresa los costos pactados (`Confirmada`). Recién cuando el camión llega y se verifica el bulto (`Recibida`), el sistema incrementa el stock físico y actualiza el costo de reposición del producto.

### 3. ¿Cómo se calculan las ganancias en el módulo de reportes? ¿Es un porcentaje fijo?
> **Respuesta**: No es un porcentaje estimado fijo. Cada línea de venta en `DETALLE_VENTA` guarda históricamente el `precio_compra` que tenía el producto al momento de venderse junto con el `precio_venta`. La ganancia se calcula como:  
> `Ganancia = SUM(Subtotal - (Cantidad * PrecioCompra))`  
> Esto permite obtener el margen bruto real sin distorsión, incluso si los costos de compra aumentaron semanas después.

### 4. ¿Cómo resuelven la seguridad y permisos de usuario?
> **Respuesta**: Las contraseñas nunca se guardan en texto plano; usamos `BCrypt` con salting automático (work factor 12). La autorización está centralizada en la clase estática de dominio `PoliticaAcceso`: la interfaz gráfica consulta esta política para habilitar pestañas, botones de anulación y paneles administrativos según el rol (`Administrador`, `Encargado`, `Vendedor`). Además, si un usuario falla 3 intentos de contraseña, la cuenta se bloquea automáticamente.

### 5. ¿Qué patrón de inyección de dependencias y arquitectura utilizaron?
> **Respuesta**: Usamos Arquitectura en Capas desacoplada con el contenedor oficial de .NET `Microsoft.Extensions.DependencyInjection`. En `Program.cs` se configuran las dependencias de repositorio y negocio como servicios `Transient` o `Singleton`, y se utiliza `IDbContextFactory<AppDbContext>` para crear contextos efímeros e independientes en operaciones asincrónicas, evitando problemas de concurrencia en Entity Framework.
