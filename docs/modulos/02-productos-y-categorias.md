# 📦 Módulo 02: Catálogo de Productos y Categorías

## Resumen Ejecutivo
Administra los artículos para la venta, su clasificación jerárquica por rubro, el control de stock en tiempo real y la detección de inventario crítico.

---

## 1. Puntos Clave para la Defensa

| Concepto | Implementación Técnica | Ubicación en Código |
|---|---|---|
| **Identificación Única** | Código de barras / SKU único (`codigo`), indexado en base de datos. | [CapaDatos/AppDbContext.cs](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/CapaDatos/AppDbContext.cs) |
| **Baja Lógica** | Los productos y categorías nunca se eliminan físicamente (`estado = 0`), protegiendo la integridad referencial de comprobantes históricos. | [CapaNegocio/ProductoNegocio.cs](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/CapaNegocio/ProductoNegocio.cs) |
| **Integridad de Categorías** | No se puede desactivar ni dar de baja una categoría que contenga productos activos vinculados. | [CapaNegocio/CategoriaNegocio.cs](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/CapaNegocio/CategoriaNegocio.cs) |
| **Regla de Márgenes** | `precio_venta` debe ser estrictamente mayor que `precio_compra`. | [CapaNegocio/ProductoNegocio.cs](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/CapaNegocio/ProductoNegocio.cs) |
| **Semáforo Stock Crítico** | Resuelto mediante `ResolucionUmbralStock` con valor configurable en `appsettings.json` (por defecto 5 unidades). | [CapaNegocio/Stock/ResolucionUmbralStock.cs](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/CapaNegocio/Stock/ResolucionUmbralStock.cs) |

---

## 2. Reglas del Semáforo de Stock

En [ProductosForm.cs](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/CapaPresentacion/Productos/ProductosForm.cs) y en el Dashboard de [inicio.cs](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/CapaPresentacion/Inicio/inicio.cs):

```
       Stock <= 0           -> 🔴 AGOTADO (Rojo fuerte)
  0 <  Stock < UmbralMinimo -> 🟠 CRÍTICO (Naranja / Alerta reposición)
       Stock >= UmbralMinimo -> 🟢 NORMAL (Verde / Stock suficiente)
```

- **Resiliencia (Modo Degradado)**: Si el valor de `StockCritico:UmbralMinimo` en `appsettings.json` está vacío o corrupto, `ResolucionUmbralStock.Resolver()` entra en modo degradado sin crashear la aplicación: solo alerta productos con `Stock <= 0`.

---

## 3. Relación Producto-Proveedor (`PROVEEDOR_PRODUCTO`)

- Un producto puede ser provisto por múltiples proveedores.
- Se modela mediante la entidad intermedia `ProveedorProducto` con clave compuesta `(id_proveedor, id_producto)`.
- Permite registrar el código de producto específico del proveedor (`codigo_producto_proveedor`) y el último costo cotizado.

---

## 4. Preguntas Frecuentes de Examen

- **¿Qué pasa si un usuario intenta borrar un producto que ya tiene ventas?**:  
  El sistema aplica baja lógica (`estado = 0`). El producto deja de aparecer para nuevas ventas en el POS, pero sigue existiendo para reportes e históricos de ventas previas.
- **¿Cómo se sincroniza el combo de categorías cuando se crea una categoría nueva?**:  
  Al entrar a la pestaña `tab_productos`, el evento `tab_productos_Enter` consulta `CategoriaNegocio.ObtenerCategoriasActivasAsync()` y recarga el combo sin necesidad de reiniciar el sistema.
