# T9b — Checklist de verificación manual: ComprasForm

> **Estado**: plantilla de verificación (no se inventan resultados). Cada punto requiere evidencia con datos reales (§9.1, RNF-6). Se completa **con datos reales**; mientras no se marque, no se considera cumplido.
>
> **Fecha de inicio de verificación**: pendiente (debe realizarse con datos reales, no con contexto dispuesto).
> **Pantalla**: `CapaPresentacion/Compras/ComprasForm.cs` (MaterialSkin 2, eventos delgados, DI por `Program.cs` / `Inicio/inicio.cs`).
> **Referencia de código**: `CompraNegocio` (única fuente de reglas, mensajes, cálculos); `CalculoImportesCompra`; `AnalizadorImporte`; `AnalizadorCantidad`.

---

## Confirmaciones previas (sin datos; verificación de código / estructura)

| # | Confirmación | Criterio / referencia | Resultado (marcar al verificar) | Evidencia requerida (fecha + observación breve) |
|---|---|---|---|---|
| C1 | **No hay `AppDbContext` ni SQL directo en la UI** | `ComprasForm.cs` no declara ni usa `AppDbContext`; todos los accesos van por `CompraNegocio` (injected). RNF-4, RNF-12. | ☐ | Revisión de código + `grep` sin `AppDbContext` en archivo. Fecha: ____ |
| C2 | **No hay cálculos ni mensajes propios de la UI** | Subtotal y total salen solo de `CalcularSubtotalEnPreparacion` / `CalcularMontoTotal`; mensajes de rechazo son solo los del catálogo (`CapaNegocio` constantes privadas, §5). RNF-13, RNF-8. | ☐ | Revisión de `ComprasForm.cs`: sin fórmulas, sin `MessageBox` con texto propio de límite/acumulación. Fecha: ____ |
| C3 | **Todo viene de `CompraNegocio`** | Búsquedas, precios, registro, historial, detalle: llamadas a `CompraNegocio`; sin consultas EF directas. RNF-4. | ☐ | Listado de métodos invocados en `ComprasForm` vs. `CompraNegocio`. Fecha: ____ |

---

## Tabla checklist por RF / RNF

| # | RF / RNF | Punto a verificar (criterio) | Datos sugeridos para evidencia | Resultado (☐ / ✅ / ❌) | Nota de evidencia (fecha; dato usado; resultado visual) |
|---|---|---|---|---|---|
| 1 | RF-1 / RF-2 / RF-3 | Rótulos y retiro de la maqueta original (título «📑 REGISTRO DE COMPRAS…» retirado; botón «Nueva Orden de Compra» y combo manual retirados; fila de ejemplo retirada; rótulo «Registro de Compras a Proveedores» presente). MaterialSkin coherente. | Captura inicial de pantalla antes y después de `T9a`. | ☐ | Fecha: ____; pantalla: ____ |
| 2 | RF-8 | Permisos visibles: con rol `Administrador` / `Encargado` la pestaña opera; `Vendedor` no ve la pestaña o la lógica rechaza registro/historial/detalle (`PermisoDenegado`). | Iniciar sesión con cada rol; observar acceso. | ☐ | Fecha: ____; rol probado: ____ |
| 3 | RF-12 / RF-13 | Búsqueda de proveedores: parcial por documento/razón social; solo activos; ordenado; reemplazable sin perder líneas. Proveedor elegido visible. | Buscar «S.A.» / «123»; reemplazar con otro. | ☐ | Fecha: ____; texto: ____; reemplazo sin pérdida de líneas: sí/no |
| 4 | RF-31 | Confirmación de limpiar: mensaje del catálogo (`ObtenerMensajeConfirmacionCancelarCompra`) y descarte de proveedor + líneas + captura. | Pulsar «Cancelar compra» con líneas cargadas. | ☐ | Fecha: ____; mensaje visto: sí/no; líneas restantes: 0 / >0 |
| 5 | RF-32 | **Descarte tras registro exitoso**: proveedor, grilla de líneas y área de captura quedan vacíos; historial recargado con compra nueva en posición. | Registrar compra de 2 líneas; observar pantalla tras éxito. | ☐ | Fecha: ____; id_compra registrada: ____ |
| 6 | RF-38 | Prellenado editable al elegir producto (`ObtenerPreciosProductoAsync`): precios de compra / venta cargados en captura; editables; sin modificar `PRODUCTO`. | Elegir producto activo (ej. `P1`); observar precios; editar; guardar. | ☐ | Fecha: ____; id_producto: ____; precios cargados: sí/no |
| 7 | RF-43 | **Prellenado repetible / vuelta a prellenar**: al quitar un producto seleccionado y luego re-agregarlo, vuelve a prellenar (no queda vacío por haber quitado antes). | Agregar → quitar → agregar mismo producto. | ☐ | Fecha: ____; producto: ____ |
| 8 | RF-52 | Historial real: orden descendente (`FechaRegistro`); fecha en formato `CellFormatting` (`dd/MM/yyyy HH:mm`); estado «Activa»/«Inactiva»; posición de compra recién registrada correcta. | Registrar y consultar historial; observar orden y formato. | ☐ | Fecha: ____; id_compra: ____; posición observada: ____ |
| 9 | RF-58 | Doble envío deshabilitado: `_btnRegistrar.Enabled = false` + bandera `_registrando`; dos clics rápidos no generan dos registros. | Doble clic en «Registrar compra». | ☐ | Fecha: ____; compras creadas tras doble clic: 1 / 2 |
| 10 | RF-65 | Subtotal área de captura: «—» cuando `SubtotalLineaDto.EsValido == false`; total general no se altera con la línea en preparación (solo con líneas confirmadas). | Ingresar cantidad/pedido inválido (ej. `0`, `abc`); ver subtotales. | ☐ | Fecha: ____; subtotal en preparación: ____; total general: ____ |
| 11 | RNF-7 | MaterialSkin coherente: solo controles admitidos por especificación (§10.2); sin controles adicionales; sin excepciones visuales. | Revisión visual de todos los controles en `ComprasForm`. | ☐ | Fecha: ____; controles fuera de especificación: ____ |
| 12 | RNF-12 | Formato de fechas/estados: `CellFormatting` aplica `N2` a importes, `dd/MM/yyyy HH:mm` a fechas, «Activa»/«Inactiva» a estado (`bool`). No hay formateo ni textos en `CompraNegocio`. | Ver grid de líneas y historial; comprobar formatos. | ☐ | Fecha: ____; columna observada: ____ |
| 13 | RF-35 (límite acumulación) / RF-63 (máximo acumulado) | Rechazo de acumulación que supera 1.000.000 (`CantidadMaxima`): mensaje de catálogo; grilla intacta; total sin cambio. | Intentar agregar líneas que sumen > 1.000.000 (ej. 999.999 + 2). | ☐ | Fecha: ____; cantidad acumulada antes: ____; mensaje recibido: ____ |

---

## Nota sobre datos reales (RNF-6, T9b, T10)

- **No se completará esta plantilla hasta que se ejecute con datos reales** (proveedor activo, productos activos sembrados, contexto real de SQL Server, no solo InMemory). El contexto InMemory no representa el rollback real ni la persistencia completa (plan §9.1).
- Cada celda marcada debe ir acompañada de una breve nota: fecha de verificación, datos usados (id, nombre, texto buscado, cantidad, precio ingresado), resultado visual observado y, si aplica, captura o log de los conteos (`SELECT COUNT(*) FROM COMPRA/DETALLE_COMPRA`).
- Si un punto falla, debe documentarse como defecto con el mensaje recibido (del catálogo, nunca inventado) y el estado de la grilla (intacta sí/no, conforme a RNF-11).
- **No modificar código implementado** (T1–T9a): si un punto requiere ajuste, se documenta como hallazgo para T9a/T10, no se parchea la UI sin autorización.

---

## Cierre de esta plantilla

- [ ] Todos los puntos C1–C3 y 1–13 verificados con evidencia.
- [ ] Ningún resultado inventado; cada ✅ tiene nota con fecha + datos.
- [ ] Confirmado que `AppDbContext` no aparece en `ComprasForm.cs` y que los cálculos salen de `CalculoImportesCompra` / `AnalizadorImporte` / `CalculoMontoTotal`.
- [ ] Confirmado que `CellFormatting` aplica `N2` / `dd/MM/yyyy HH:mm` y que los mensajes de rechazo son los del catálogo (§5) y no textos locales de la UI.
- [ ] Confirma `dotnet build` limpio y `dotnet test` verde **antes** y **después** de la verificación manual (sin modificar implementación).
