# 👥 Módulo 03: Padrones Comerciales (Clientes y Proveedores)

## Resumen Ejecutivo
Gestiona las entidades externas con las que interactúa el negocio: clientes para la emisión de comprobantes de venta y proveedores para la adquisición de mercadería.

---

## 1. Puntos Clave para la Defensa

| Concepto | Implementación Técnica | Ubicación en Código |
|---|---|---|
| **Consumidor Final** | Cliente predeterminado en el sistema con `DNI = '0'` y nombre `Consumidor Final`. | [CapaEntidad/Cliente.cs](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/CapaEntidad/Cliente.cs) |
| **Validación de Identificadores** | DNI numérico para clientes; CUIT argentino de 11 dígitos con validación de formato para proveedores. | [CapaNegocio/ClienteNegocio.cs](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/CapaNegocio/ClienteNegocio.cs), [CapaNegocio/ProveedorNegocio.cs](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/CapaNegocio/ProveedorNegocio.cs) |
| **Unicidad de Registros** | Índices únicos en SQL Server sobre `dni` y `correo` en clientes, y `cuit` y `correo` en proveedores. | [CapaDatos/AppDbContext.cs](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/CapaDatos/AppDbContext.cs) |
| **Baja Lógica** | Ni clientes ni proveedores se eliminan físicamente (`estado = 0`), preservando los comprobantes fiscales y compras previas. | [CapaNegocio/ClienteNegocio.cs](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/CapaNegocio/ClienteNegocio.cs) |
| **Búsqueda Dinámica** | Filtrado en tiempo real en la grilla por DNI, Nombre, Apellido, CUIT o Razón Social mediante eventos `TextChanged`. | [CapaPresentacion/Clientes/ClientesForm.cs](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/CapaPresentacion/Clientes/ClientesForm.cs) |

---

## 2. Flujo de Selección de Cliente en el POS (Ventas)

```
        ┌──────────────────────────────────────────────┐
        │ El cajero inicia una venta en VentasForm     │
        └──────────────────────┬───────────────────────┘
                               │
               ¿El cliente solicita factura nominal?
               ├── NO ────────────────────────────────┐
               │                                      │
               ▼                                      ▼
    ┌─────────────────────────┐            ┌───────────────────────────┐
    │ Por defecto se asigna   │            │ Busca cliente por DNI     │
    │ DNI '0' Consumidor      │            │ o registra alta rápida en │
    │ Final                   │            │ modal ClientesForm        │
    └─────────────────────────┘            └───────────────────────────┘
```

---

## 3. Preguntas Frecuentes de Examen

- **¿Qué pasa si un cliente o proveedor cambia de correo o teléfono?**:  
  El formulario permite edición directa validando que el nuevo correo no esté duplicado por otra entidad distinta.
- **¿Puede un vendedor dar de alta clientes?**:  
  Sí, para agilizar la fila de caja, la política `PoliticaAcceso.TieneAcceso(rol, "Clientes")` permite a los vendedores consultar y registrar clientes al momento de la venta. Sin embargo, no pueden modificar configuraciones administrativas.
