# Anexo 005 - HU-10: stock y precios al confirmar compra (spec + plan + tasks)

## Hechos confirmados
- `CompraNegocio.RegistrarCompraAsync` ya abre transaccion explicita (BeginTransactionAsync / CommitAsync / RollbackAsync protegido), hace un `SaveChangesAsync` y llama al hook `AplicarEfectosSecundariosAsync` (no-op) antes del commit.
- No existe actualizacion de stock/precios, ni modificacion/anulacion de compras confirmadas (solo alta; `Compra.Estado = true` siempre). Por tanto no hay riesgo de doble incremento por re-guardado hoy.
- Precio de compra y cantidad: `LineaCompraDto` -> `DetalleCompra`. Precio de venta: solo en `LineaCompraDto.PrecioVenta` (D-2: `DetalleCompra` no lo persiste), disponible en memoria durante el registro.
- Lineas repetidas del mismo producto ya se rechazan antes de escribir (`ProductoInvalido`, "repetido"); la UI acumula en `AgregarLineaAsync`.
- `Producto`: `Stock` (int), `PrecioCompra`, `PrecioVenta` (decimal).

## Requisitos (EARS)
- RF-H10-1: Cuando se registra una compra con `Estado = true`, el sistema SHALL sumar `Cantidad` de cada linea a `PRODUCTO.Stock` de su producto.
- RF-H10-2: Cuando se registra una compra con `Estado = true`, el sistema SHALL sobrescribir `PRODUCTO.PrecioCompra` y `PRODUCTO.PrecioVenta` con los de la linea.
- RF-H10-3: Si `Estado != true`, el sistema SHALL NOT modificar `PRODUCTO`.
- RF-H10-4: Cabecera, detalle y cambios de producto SHALL persistir en la misma transaccion; ante fallo, ninguno persiste (rollback existente).
- RF-H10-5: Cada registro aplica el efecto una sola vez (el alta ocurre una vez; no hay re-guardado de compras confirmadas). Si en el futuro se agrega edicion/anulacion, debera aplicar diferencia/reversion: fuera de alcance.
- Fuera de alcance: IdUsuarioModifica/FechaModificacion/MotivoAjusteStock (no se tocan), UI, esquema.

## Plan / decision (la mas simple)
Implementar el cuerpo del hook existente: pasar `lineas` (necesarias por PrecioVenta) a `AplicarEfectosSecundariosAsync(Compra, IReadOnlyList<LineaCompraDto>)` (metodo privado, sin cambio de contrato publico). Dentro: si `!cabecera.Estado` retornar; cargar con tracking los productos por id (una consulta `Where(Contains)`), por linea `Stock += Cantidad; PrecioCompra = linea.PrecioCompra; PrecioVenta = linea.PrecioVenta;`, luego `SaveChangesAsync()` (dentro de la transaccion, antes del Commit). Si hay lineas repetidas (defensivo) se agrupan sumando cantidad con precios de la ultima. Alternativa descartada: trigger/SP o repositorio nuevo (cambio de esquema/capas).
Actualizar docs/comentarios RF-47/RF-48 del metodo y el test existente `RegistrarCompra_NoModificaElCatalogoDeProductos` (contradice el nuevo requisito).

## Tasks
1. `CapaNegocio\CompraNegocio.cs`: implementar hook + ajustar llamada (linea ~810) y XML-doc (RF-47/48 obsoletos). Hecho: compila y tests T2 pasan.
2. `CapaNegocio.Tests\CompraNegocioTests.cs`: reemplazar test RF-47/48 por 5 tests:
   a) incrementa stock (7 + 2 = 9); b) sobrescribe PrecioCompra y PrecioVenta; c) compra no confirmada (Estado false; ver riesgo) deja producto intacto; d) sin doble incremento: un registro = un incremento, y un registro rechazado/fallido (FalloDbContext) no cambia stock; e) varios productos suman cada uno y linea repetida es rechazada sin tocar stock.
3. Ejecutar `CapaNegocio.Tests` filtrado a Compra (log a %TEMP%\G70\).

## Riesgos
- `RegistrarCompraAsync` crea siempre `Estado = true`; test (c) requiere invocar el hook con una `Compra` Estado=false (hacerlo `internal` + InternalsVisibleTo seria cambio de proyecto) o verificar rechazos/fallo sin cambios. Decision: cubrir (c) via compra rechazada y via guarda `Estado` en el hook; Implementer puede omitir llamada directa si requiere tocar el .csproj.
- InMemory no simula rollback real: atomicidad solo con FalloDbContext (falla antes de escribir).
- Desbordamiento de `int` en Stock: no hay tope definido; no se agrega regla.
- Producto inactivo ya rechazado antes; PrecioVenta de compra sobrescribe aunque baje el precio (pedido).
