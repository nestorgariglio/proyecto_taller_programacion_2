# 📊 Módulo 06: Tablero de Reportes y Analítica Comercial

## Resumen Ejecutivo
Provee visualización analítica en tiempo real del rendimiento económico del negocio, combinando KPIs agregados, gráficos interactivos con LiveCharts y vistas SQL Server para alto desempeño.

---

## 1. Puntos Clave para la Defensa

| Concepto | Implementación Técnica | Ubicación en Código |
|---|---|---|
| **Vistas SQL Nativas (HU-15)** | `dbo.vw_ResumenDiarioVentas` y `dbo.vw_ParticipacionCategorias` para delegar agregaciones al motor relacional. | [scripts/seed-desarrollo.sql](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/scripts/seed-desarrollo.sql), [CapaDatos/Migrations/20261116193554_AddVistasReportes.cs](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/CapaDatos/Migrations/20261116193554_AddVistasReportes.cs) |
| **Margen Bruto Real** | Calculado línea por línea: `Ganancia = SUM(Subtotal - (Cantidad * PrecioCompra))`. | [CapaNegocio/ReporteNegocio.cs](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/CapaNegocio/ReporteNegocio.cs) |
| **Filtros Temporales** | Soporta períodos: `Últimos 7 días`, `Este Mes`, `Último Trimestre` y `Año Actual`. | [CapaNegocio/ReporteNegocio.cs](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/CapaNegocio/ReporteNegocio.cs) |
| **Gráficos Dinámicos** | Gráfico de barras para evolución de ventas (`CartesianChart`) y gráfico de torta para cuota por categoría (`PieChart`). | [CapaPresentacion/Reportes/ReportesForm.cs](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/CapaPresentacion/Reportes/ReportesForm.cs) |
| **Refresco Automático** | Al ingresar a la pestaña `tab_reportes`, el evento `tab_reportes_Enter` recarga los datos si el rol tiene acceso. | [CapaPresentacion/Inicio/inicio.cs](file:///home/nestorgariglio/repositories/university/proyecto_taller_programacion_2/CapaPresentacion/Inicio/inicio.cs) |

---

## 2. Vistas SQL Creadas en el Motor

### A. `dbo.vw_ResumenDiarioVentas`
Agrupa las ventas válidas (`estado = 1`) por día:
- `fecha`: Día calendario (`DATE`).
- `comprobantes_emitidos`: Total de tickets emitidos ese día (`COUNT(DISTINCT v.id_venta)`).
- `total_ventas`: Monto total facturado (`SUM(v.monto_total)`).
- `ganancia_total`: Margen bruto obtenido (`SUM(dv.subtotal - (dv.cantidad * dv.precio_compra))`).
- `ticket_promedio`: `total_ventas / comprobantes_emitidos`.

### B. `dbo.vw_ParticipacionCategorias`
Cruza `DETALLE_VENTA` con `PRODUCTO` y `CATEGORIA`:
- `categoria`: Nombre descriptivo de la categoría.
- `total_transacciones`: Cantidad de ventas que incluyeron productos de este rubro.
- `unidades_vendidas`: Suma total de unidades despachadas.
- `monto_total`: Facturación generada por la categoría.

---

## 3. Arquitectura de Datos del Reporte

```
       [ Base de Datos SQL Server ]
                    │
         (Vistas SQL y Tablas VENTA/DETALLE)
                    │
                    ▼
     [ CapaNegocio: ReporteNegocio.cs ]
                    │
         (Filtra período, calcula ticket promedio,
          formatea cronologías y porcentajes)
                    │
                    ▼
     [ DTO: ReporteMetricasDto ]
        ├── FacturacionTotal, GananciaTotal, TicketPromedio
        ├── List<EvolucionVentaItemDto> (Barras)
        ├── List<ParticipacionCategoriaItemDto> (Torta)
        └── List<ResumenDiarioItemDto> (Grilla)
                    │
                    ▼
  [ CapaPresentacion: ReportesForm.cs (LiveCharts) ]
```

---

## 4. Preguntas Frecuentes de Examen

- **¿Qué pasa con las ventas anuladas en los reportes?**:  
  Las consultas filtran estrictamente `WHERE v.estado = 1`. Las ventas anuladas quedan registradas para auditoría pero se excluyen totalmente de la facturación y de las ganancias.
- **¿Por qué usar LiveCharts?**:  
  LiveCharts se integra con WinForms y WPF, permitiendo animaciones fluidas, etiquetas interactivas de montos y porcentajes sin sobrecargar el hilo de UI.
