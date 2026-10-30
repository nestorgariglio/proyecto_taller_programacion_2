using System.Globalization;
using CapaDatos;
using CapaEntidad;
using CapaNegocio.Compras;
using CapaNegocio.DTOs.Compras;
using CapaNegocio.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CapaNegocio;

/// <summary>
/// Único punto de las reglas de Compras (spec 005): catálogo de mensajes,
/// validación de las líneas de la compra en carga y operaciones sobre el
/// detalle. Es <b>sin estado</b>: recibe las líneas actuales en cada solicitud
/// y devuelve la lista resultante (P-1). La pantalla no valida ni redacta
/// mensajes (RF-55 y RNF-13).
/// </summary>
public class CompraNegocio
{
    // ---------- Catálogo de mensajes (plan §5, RF-55) ----------

    private const string MensajeCantidadSuperaMaximo = "La cantidad supera el máximo permitido.";
    private const string MensajeCantidadInvalida = "La cantidad debe ser un número entero mayor que cero escrito solo con dígitos.";
    private const string MensajeCantidadAcumuladaSuperaMaximo = "La cantidad acumulada supera el máximo permitido.";

    private const string MensajePrecioCompraSuperaMaximo = "El precio de compra supera el máximo permitido.";
    private const string MensajePrecioVentaSuperaMaximo = "El precio de venta supera el máximo permitido.";
    private const string MensajeFormatoPrecioCompra = "El precio de compra admite solo números, con coma o punto decimal y hasta dos decimales.";
    private const string MensajeFormatoPrecioVenta = "El precio de venta admite solo números, con coma o punto decimal y hasta dos decimales.";
    private const string MensajeDecimalesPrecioCompra = "El precio de compra admite como máximo dos decimales.";
    private const string MensajeDecimalesPrecioVenta = "El precio de venta admite como máximo dos decimales.";
    private const string MensajePrecioCompraVacio = "El precio de compra es obligatorio y debe ser mayor que cero.";
    private const string MensajePrecioCompraNoPositivo = "El precio de compra debe ser mayor que cero.";
    private const string MensajePrecioVentaVacio = "El precio de venta es obligatorio y debe ser mayor que cero.";
    private const string MensajePrecioVentaNoPositivo = "El precio de venta debe ser mayor que cero.";

    private const string MensajeProveedorRequerido = "Debe seleccionar un proveedor antes de registrar la compra.";
    private const string MensajeProveedorInexistente = "El proveedor seleccionado no existe. Vuelva a buscarlo.";
    private const string MensajeProveedorInactivo = "El proveedor {0} está inactivo y no puede registrar compras.";

    private const string MensajeUsuarioSinIdentificar = "No se pudo identificar un usuario responsable válido para registrar la compra.";
    private const string MensajeUsuarioInexistente = "El usuario responsable de la compra no existe.";
    private const string MensajeUsuarioInactivo = "El usuario responsable de la compra está inactivo.";

    private const string MensajeSinItems = "La compra debe tener al menos un ítem.";
    private const string MensajeMontoTotalSuperaMaximo = "El monto total de la compra supera el máximo permitido.";

    private const string MensajeProductoInexistente = "El producto de la línea {0} no existe. Vuelva a seleccionarlo.";
    private const string MensajeProductoInactivo = "El producto {0} - {1} de la línea {2} está inactivo y no puede comprarse.";
    private const string MensajeProductoRepetido = "El producto {0} - {1} está repetido en el detalle de la compra.";
    private const string MensajeProductoDuplicadoEnEdicion = "El producto {0} - {1} ya está cargado en otra línea. Quite una de las dos líneas para acumular la cantidad.";

    private const string MensajeLineaAgregada = "Línea agregada.";
    private const string MensajeLineaActualizada = "Línea actualizada.";
    private const string MensajeAcumulacionAplicada = "Se acumuló la cantidad del producto {0}: ahora tiene {1} unidades.";
    private const string MensajeLineaInexistente = "La línea indicada no existe en el detalle de la compra.";

    private const string MensajeRegistroExitoso = "Compra N° {0} registrada exitosamente.";
    private const string MensajePermisoRegistrarCompra = "No tiene permisos para registrar compras.";
    private const string MensajePermisoConsultarCompras = "No tiene permisos para consultar las compras.";

    private const string MensajeSinProveedoresCoincidentes = "No se encontraron proveedores que coincidan con la búsqueda.";
    private const string MensajeSinProductosCoincidentes = "No se encontraron productos que coincidan con la búsqueda.";
    private const string MensajeProveedoresEncontrados = "Se encontraron {0} proveedores.";
    private const string MensajeUnProveedorEncontrado = "Se encontró 1 proveedor.";
    private const string MensajeProductosEncontrados = "Se encontraron {0} productos.";
    private const string MensajeUnProductoEncontrado = "Se encontró 1 producto.";

    private const string MensajePreciosPropuestos = "Se propusieron los precios actuales del catálogo.";
    private const string MensajeProductoNoEncontrado = "El producto indicado no existe.";
    private const string MensajeProductoInactivoAlPrellenar = "El producto indicado está inactivo y no puede comprarse.";

    private const string MensajeHistorialVacio = "Todavía no hay compras registradas.";
    private const string MensajeComprasEncontradas = "Se encontraron {0} compras.";
    private const string MensajeUnaCompraEncontrada = "Se encontró 1 compra.";
    private const string MensajeCompraNoEncontrada = "La compra indicada no existe.";

    // Respuestas de las consultas históricas producto ↔ proveedor (T11, RF-55).
    private const string MensajeSinProveedoresHistoricos = "El producto indicado no tiene proveedores con compras registradas.";
    private const string MensajeSinProductosHistoricos = "El proveedor indicado no tiene productos con compras registradas.";

    // Textos defensivos de las navegaciones nulas (§3.4, M11), idénticos en el
    // historial y en el detalle; no se formatean ni se construyen en la pantalla.
    private const string TextoSinProveedor = "(Sin proveedor)";
    private const string TextoSinUsuario = "(Sin usuario)";

    private const string MensajeErrorPersistencia = "No se pudo completar la operación por un problema con el origen de datos. Intente nuevamente.";
    private const string MensajeErrorBuscarProveedores = "No se pudo buscar proveedores por un problema con el origen de datos. Intente nuevamente.";
    private const string MensajeErrorBuscarProductos = "No se pudo buscar productos por un problema con el origen de datos. Intente nuevamente.";
    private const string MensajeErrorPreciosProducto = "No se pudieron obtener los precios del producto por un problema con el origen de datos. Intente nuevamente.";

    private const string MensajeConfirmacionCancelarCompra = "¿Confirma que desea descartar el proveedor y todas las líneas de la compra en carga?";

    private readonly AppDbContext _context;

    public CompraNegocio(AppDbContext contexto)
    {
        ArgumentNullException.ThrowIfNull(contexto);
        _context = contexto;
    }

    // ---------- Búsquedas y prellenado (RF-9 … RF-17 y RF-38 … RF-41) ----------

    /// <summary>
    /// Búsqueda rápida de proveedores para la carga de compras (RF-9 … RF-12):
    /// solo activos, coincidencia parcial por CUIT o razón social sin distinguir
    /// mayúsculas, orden determinista y tope de 20. El texto vacío devuelve el
    /// resultado vacío <b>sin consultar</b> el origen de datos (RF-10) y un fallo
    /// de lectura no propaga la excepción (RF-57).
    /// </summary>
    public async Task<RespuestaBusquedaProveedoresCompra> BuscarProveedoresAsync(string? textoBusqueda)
    {
        string? texto = textoBusqueda?.Trim();

        if (string.IsNullOrEmpty(texto))
        {
            return new RespuestaBusquedaProveedoresCompra
            {
                Resultado = ResultadoBusquedaCompras.SinResultados,
                Mensaje = string.Empty
            };
        }

        try
        {
            string patron = texto.ToUpperInvariant();

            List<ProveedorSeleccionadoDto> proveedores = await _context.Proveedores
                .AsNoTracking()
                .Where(p => p.Estado)
                .Where(p => (p.Cuit != null && p.Cuit.ToUpper().Contains(patron))
                    || (p.RazonSocial != null && p.RazonSocial.ToUpper().Contains(patron)))
                .OrderBy(p => p.RazonSocial)
                .ThenBy(p => p.IdProveedor)
                .Take(20)
                .Select(p => new ProveedorSeleccionadoDto
                {
                    IdProveedor = p.IdProveedor,
                    RazonSocial = p.RazonSocial ?? string.Empty,
                    Documento = p.Cuit ?? string.Empty
                })
                .ToListAsync();

            if (proveedores.Count == 0)
            {
                return new RespuestaBusquedaProveedoresCompra
                {
                    Resultado = ResultadoBusquedaCompras.SinResultados,
                    Mensaje = MensajeSinProveedoresCoincidentes
                };
            }

            return new RespuestaBusquedaProveedoresCompra
            {
                Resultado = ResultadoBusquedaCompras.ConResultados,
                Proveedores = proveedores,
                Mensaje = proveedores.Count == 1
                    ? MensajeUnProveedorEncontrado
                    : string.Format(CultureInfo.InvariantCulture, MensajeProveedoresEncontrados, proveedores.Count)
            };
        }
        catch (Exception ex) when (EsFalloDeOrigenDeDatos(ex))
        {
            return new RespuestaBusquedaProveedoresCompra
            {
                Resultado = ResultadoBusquedaCompras.ErrorPersistencia,
                Mensaje = MensajeErrorBuscarProveedores,
                Proveedores = Array.Empty<ProveedorSeleccionadoDto>()
            };
        }
    }

    /// <summary>
    /// Búsqueda rápida de productos de la línea (RF-15 … RF-17): solo activos,
    /// parcial por código o nombre, orden por nombre e identificador, tope de 20.
    /// El texto vacío devuelve la lista vacía <b>sin consultar</b> (RF-16).
    /// </summary>
    public async Task<RespuestaBusquedaProductosCompra> BuscarProductosAsync(string? textoBusqueda)
    {
        string? texto = textoBusqueda?.Trim();

        if (string.IsNullOrEmpty(texto))
        {
            return new RespuestaBusquedaProductosCompra
            {
                Resultado = ResultadoBusquedaCompras.SinResultados,
                Mensaje = string.Empty
            };
        }

        try
        {
            string patron = texto.ToUpperInvariant();

            List<ProductoBuscadoDto> productos = await _context.Productos
                .AsNoTracking()
                .Where(p => p.Estado)
                .Where(p => (p.Codigo != null && p.Codigo.ToUpper().Contains(patron))
                    || (p.Nombre != null && p.Nombre.ToUpper().Contains(patron)))
                .OrderBy(p => p.Nombre)
                .ThenBy(p => p.IdProducto)
                .Take(20)
                .Select(p => new ProductoBuscadoDto
                {
                    IdProducto = p.IdProducto,
                    Codigo = p.Codigo ?? string.Empty,
                    Nombre = p.Nombre ?? string.Empty
                })
                .ToListAsync();

            if (productos.Count == 0)
            {
                return new RespuestaBusquedaProductosCompra
                {
                    Resultado = ResultadoBusquedaCompras.SinResultados,
                    Mensaje = MensajeSinProductosCoincidentes
                };
            }

            return new RespuestaBusquedaProductosCompra
            {
                Resultado = ResultadoBusquedaCompras.ConResultados,
                Productos = productos,
                Mensaje = productos.Count == 1
                    ? MensajeUnProductoEncontrado
                    : string.Format(CultureInfo.InvariantCulture, MensajeProductosEncontrados, productos.Count)
            };
        }
        catch (Exception ex) when (EsFalloDeOrigenDeDatos(ex))
        {
            return new RespuestaBusquedaProductosCompra
            {
                Resultado = ResultadoBusquedaCompras.ErrorPersistencia,
                Mensaje = MensajeErrorBuscarProductos,
                Productos = Array.Empty<ProductoBuscadoDto>()
            };
        }
    }

    /// <summary>
    /// Precios del catálogo para prellenar el área de captura (RF-38 … RF-41).
    /// Solo lectura: no escribe stock, precios, usuario, fecha ni motivo del
    /// producto (RF-41 y RF-47).
    /// </summary>
    public async Task<RespuestaPreciosProducto> ObtenerPreciosProductoAsync(int idProducto)
    {
        try
        {
            var producto = await _context.Productos
                .AsNoTracking()
                .Where(p => p.IdProducto == idProducto)
                .Select(p => new { p.IdProducto, p.PrecioCompra, p.PrecioVenta, p.Estado })
                .FirstOrDefaultAsync();

            if (producto is null)
            {
                return new RespuestaPreciosProducto
                {
                    Resultado = ResultadoPreciosProducto.ProductoNoEncontrado,
                    Mensaje = MensajeProductoNoEncontrado
                };
            }

            if (!producto.Estado)
            {
                return new RespuestaPreciosProducto
                {
                    Resultado = ResultadoPreciosProducto.ProductoInactivo,
                    Mensaje = MensajeProductoInactivoAlPrellenar
                };
            }

            return new RespuestaPreciosProducto
            {
                Resultado = ResultadoPreciosProducto.Exito,
                Mensaje = MensajePreciosPropuestos,
                Precios = new PreciosProductoDto
                {
                    IdProducto = producto.IdProducto,
                    PrecioCompra = producto.PrecioCompra,
                    PrecioVenta = producto.PrecioVenta
                }
            };
        }
        catch (Exception ex) when (EsFalloDeOrigenDeDatos(ex))
        {
            return new RespuestaPreciosProducto
            {
                Resultado = ResultadoPreciosProducto.ErrorPersistencia,
                Mensaje = MensajeErrorPreciosProducto
            };
        }
    }

    // ---------- Registro transaccional (RF-44 … RF-48) ----------

    /// <summary>
    /// Registra la compra completa en <b>una</b> transacción (RF-44 … RF-48)
    /// implementando los pasos <b>2–13</b> del plan §7: proveedor → usuario →
    /// líneas (producto, repetido, cantidad, precio de compra y de venta) → sin
    /// ítems → tope del total → transacción → cabecera + líneas → <b>un</b>
    /// <c>SaveChangesAsync</c> → hook de HU-10 → commit, con los pasos 14
    /// (rollback protegido y desadjunte) y 15 (liberación de la transacción).
    /// Toda la validación ocurre <b>antes</b> de abrir la transacción: ante el
    /// primer error no se escribe nada y la solicitud conserva sus líneas
    /// (RF-28 y RNF-11).
    /// </summary>
    /// <param name="solicitud">Proveedor elegido y líneas de la grilla en carga.</param>
    /// <param name="idUsuario">Usuario de la sesión; se valida su existencia y actividad.</param>
    /// <param name="rol">
    /// Rol de la sesión. Se evalúa <b>primero</b> y sin tocar la base (RF-28): solo
    /// Administrador y Encargado pueden registrar compras (RF-59 y RF-60) y el rol
    /// no se relee del origen de datos (D-14).
    /// </param>
    /// <remarks>
    /// El <c>Subtotal</c> que llega en cada línea se <b>ignora</b> y se recalcula
    /// con <see cref="CalculoImportesCompra.CalcularSubtotal"/> (RNF-13); el
    /// precio de venta no se persiste (D-2) y la compra <b>no</b> modifica
    /// `PRODUCTO` (RF-47 y RF-48). Un fallo del origen de datos al escribir se
    /// deshace con rollback y se informa con el <b>único</b> mensaje de
    /// persistencia, sin propagar la excepción (RF-45, RF-55, RF-56 y RNF-8);
    /// un rechazo de validación nunca se informa como fallo de persistencia.
    /// </remarks>
    public async Task<RespuestaRegistroCompra> RegistrarCompraAsync(
        SolicitudRegistrarCompraDto solicitud, int idUsuario, string? rol)
    {
        ArgumentNullException.ThrowIfNull(solicitud);

        // Paso 1: rol primero (RF-28). Sin permiso no se valida nada más ni se
        // consulta el origen de datos.
        string? mensajeRol = ValidarRolParaRegistro(rol);
        if (mensajeRol is not null)
        {
            return RechazoRegistro(ResultadoRegistroCompra.PermisoDenegado, mensajeRol);
        }

        IReadOnlyList<LineaCompraDto> lineas = solicitud.Lineas ?? Array.Empty<LineaCompraDto>();

        // Paso 2: proveedor.
        string? mensajeProveedor = await ValidarProveedorAsync(solicitud.IdProveedor);
        if (mensajeProveedor is not null)
        {
            return RechazoRegistro(ResultadoRegistroCompra.ProveedorInvalido, mensajeProveedor);
        }

        // Paso 3: usuario de la sesión (sin releer el rol, D-14).
        string? mensajeUsuario = await ValidarUsuarioAsync(idUsuario);
        if (mensajeUsuario is not null)
        {
            return RechazoRegistro(ResultadoRegistroCompra.UsuarioInvalido, mensajeUsuario);
        }

        // Paso 4: una sola consulta acotada de los productos de la compra.
        Dictionary<int, ProductoDeCompra> productos =
            await ValidarProductosAsync(lineas.Select(l => l.IdProducto));

        var idsVistos = new HashSet<int>();
        var subtotales = new List<decimal>(lineas.Count);

        // Paso 4 (continuación): por línea, en el orden de la grilla.
        for (int i = 0; i < lineas.Count; i++)
        {
            LineaCompraDto linea = lineas[i];

            if (!productos.TryGetValue(linea.IdProducto, out ProductoDeCompra? producto))
            {
                return RechazoRegistro(
                    ResultadoRegistroCompra.ProductoInvalido,
                    ConstruirMensajeProductoInexistente(i + 1),
                    i,
                    linea.IdProducto);
            }

            if (!producto.Estado)
            {
                return RechazoRegistro(
                    ResultadoRegistroCompra.ProductoInvalido,
                    ConstruirMensajeProductoInactivo(producto, i + 1),
                    i,
                    linea.IdProducto);
            }

            if (!idsVistos.Add(linea.IdProducto))
            {
                return RechazoRegistro(
                    ResultadoRegistroCompra.ProductoInvalido,
                    ConstruirMensajeProductoRepetido(producto),
                    i,
                    linea.IdProducto);
            }

            string? mensajeCantidad = ValidarCantidad(linea.Cantidad);
            if (mensajeCantidad is not null)
            {
                return RechazoRegistro(
                    ResultadoRegistroCompra.CantidadInvalida, mensajeCantidad, i, linea.IdProducto);
            }

            string? mensajePrecioCompra = ValidarPrecioRecibido(
                linea.PrecioCompra, CampoPrecio.PrecioCompra, out MotivoPrecioInvalido? motivoCompra);
            if (mensajePrecioCompra is not null)
            {
                return RechazoRegistro(
                    ResultadoRegistroCompra.PrecioInvalido,
                    mensajePrecioCompra,
                    i,
                    linea.IdProducto,
                    CampoPrecio.PrecioCompra,
                    motivoCompra);
            }

            string? mensajePrecioVenta = ValidarPrecioRecibido(
                linea.PrecioVenta, CampoPrecio.PrecioVenta, out MotivoPrecioInvalido? motivoVenta);
            if (mensajePrecioVenta is not null)
            {
                return RechazoRegistro(
                    ResultadoRegistroCompra.PrecioInvalido,
                    mensajePrecioVenta,
                    i,
                    linea.IdProducto,
                    CampoPrecio.PrecioVenta,
                    motivoVenta);
            }

            // RNF-13: el subtotal se recalcula y el del DTO se descarta.
            subtotales.Add(CalculoImportesCompra.CalcularSubtotal(linea.Cantidad, linea.PrecioCompra));
        }

        // Paso 5: al menos un ítem.
        if (lineas.Count == 0)
        {
            return RechazoRegistro(ResultadoRegistroCompra.SinItems, MensajeSinItems);
        }

        // Paso 6: tope del monto total (D-20 / RF-64), con la misma suma que se guarda.
        decimal montoTotal = CalculoImportesCompra.CalcularMontoTotal(subtotales);
        if (montoTotal > CalculoImportesCompra.MontoTotalMaximo)
        {
            return RechazoRegistro(
                ResultadoRegistroCompra.MontoTotalSuperaMaximo, MensajeMontoTotalSuperaMaximo);
        }

        // Pasos 7–13: todo el tramo de escritura queda protegido (RF-45 y RF-55).
        IDbContextTransaction? transaccion = null;
        Compra? cabecera = null;

        try
        {
            // Paso 7: transacción que cubre cabecera, líneas y el hook de HU-10.
            transaccion = await _context.Database.BeginTransactionAsync();

            // Paso 8: cabecera. Solo se asignan las claves, sin navegaciones, para no
            // arrastrar actualizaciones accidentales (RF-48).
            DateTime fechaRegistro = DateTime.Now;
            cabecera = new Compra
            {
                IdUsuario = idUsuario,
                IdProveedor = solicitud.IdProveedor!.Value,
                MontoTotal = montoTotal,
                Estado = true,
                FechaRegistro = fechaRegistro,
                Detalles = new List<DetalleCompra>(lineas.Count)
            };

            // Paso 9: líneas, con el subtotal recalculado y sin precio de venta (D-2).
            for (int i = 0; i < lineas.Count; i++)
            {
                cabecera.Detalles.Add(new DetalleCompra
                {
                    IdProducto = lineas[i].IdProducto,
                    Cantidad = lineas[i].Cantidad,
                    PrecioCompra = lineas[i].PrecioCompra,
                    Subtotal = subtotales[i]
                });
            }

            // Paso 10: un único SaveChangesAsync para cabecera y líneas.
            _context.Compras.Add(cabecera);
            await _context.SaveChangesAsync();

            // Paso 11: hook de HU-10, dentro de la misma transacción y antes del commit.
            await AplicarEfectosSecundariosAsync(cabecera);

            // Paso 12: commit.
            await transaccion.CommitAsync();

            // Paso 13: éxito.
            return new RespuestaRegistroCompra
            {
                Resultado = ResultadoRegistroCompra.Exito,
                Mensaje = string.Format(CultureInfo.InvariantCulture, MensajeRegistroExitoso, cabecera.IdCompra),
                IdCompra = cabecera.IdCompra,
                FechaRegistro = fechaRegistro,
                MontoTotal = montoTotal
            };
        }
        catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
        {
            // Paso 14: rollback protegido + desadjunte de la compra y de sus líneas
            // (RNF-11). Solo se informa el texto único de persistencia: ningún
            // rechazo de validación ni de límites llega hasta aquí (RNF-8).
            await RevertirSiEsPosibleAsync(transaccion);
            DesadjuntarSiEsPosible(cabecera);

            return RechazoRegistro(ResultadoRegistroCompra.ErrorPersistencia, MensajeErrorPersistencia);
        }
        finally
        {
            // Paso 15: la transacción se libera siempre, tras commit, rollback o fallo.
            if (transaccion is not null)
            {
                await transaccion.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Punto de extensión de HU-10 (stock y precios del producto) dentro de la
    /// misma operación y del mismo ámbito transaccional (RF-46). Hoy es un
    /// <b>no-op</b>: devuelve <see cref="Task.CompletedTask"/> sin la palabra
    /// clave <c>async</c> y no escribe sobre <c>Producto</c> (RF-47 y RF-48).
    /// </summary>
    private Task AplicarEfectosSecundariosAsync(Compra cabecera, CancellationToken ct = default)
    {
        _ = cabecera;
        _ = ct;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Intenta deshacer la transacción sin relanzar: si el rollback también falla,
    /// la respuesta sigue siendo el mensaje único de persistencia (RF-55 y RNF-8).
    /// </summary>
    /// <param name="transaccion">Transacción abierta, o <see langword="null"/> si no llegó a abrirse.</param>
    private static async Task RevertirSiEsPosibleAsync(IDbContextTransaction? transaccion)
    {
        if (transaccion is null)
        {
            return;
        }

        try
        {
            await transaccion.RollbackAsync();
        }
        catch (Exception)
        {
            // Mejor esfuerzo: el fallo del rollback no se relanza ni tapa el original.
        }
    }

    /// <summary>
    /// Desadjunta la cabecera y sus líneas para que un guardado posterior del mismo
    /// contexto no las persista de forma parcial (RF-45, RNF-11). Es una limpieza de
    /// mejor esfuerzo: si el contexto ya no está disponible, no queda nada rastreado
    /// que se pueda guardar por accidente.
    /// </summary>
    /// <param name="cabecera">Compra que se intentó persistir.</param>
    private void DesadjuntarSiEsPosible(Compra? cabecera)
    {
        if (cabecera is null)
        {
            return;
        }

        try
        {
            _context.Entry(cabecera).State = EntityState.Detached;

            if (cabecera.Detalles is null)
            {
                return;
            }

            foreach (DetalleCompra detalle in cabecera.Detalles)
            {
                _context.Entry(detalle).State = EntityState.Detached;
            }
        }
        catch (InvalidOperationException)
        {
            // El contexto ya no está disponible: la compra no puede volver a
            // guardarse desde este contexto.
        }
    }

    // ---------- Detalle de la compra en carga (RF-18 … RF-37) ----------

    /// <summary>
    /// Operaciones sobre el detalle de la compra en carga (RF-18, RF-20 … RF-30,
    /// RF-36, RF-37, RF-62 y RF-63). Aún no validan el rol: el chequeo llega en T8.
    /// </summary>
    public async Task<RespuestaOperacionLineaCompra> AgregarLineaAsync(SolicitudAgregarLineaCompraDto solicitud)
    {
        ArgumentNullException.ThrowIfNull(solicitud);

        IReadOnlyList<LineaCompraDto> lineas = solicitud.LineasActuales ?? Array.Empty<LineaCompraDto>();
        int indiceNuevaLinea = lineas.Count;

        Dictionary<int, ProductoDeCompra> productos = await ValidarProductosAsync(new[] { solicitud.IdProducto });

        if (!productos.TryGetValue(solicitud.IdProducto, out ProductoDeCompra? producto))
        {
            return Rechazo(
                ResultadoOperacionLineaCompra.ProductoInvalido,
                ConstruirMensajeProductoInexistente(indiceNuevaLinea + 1),
                lineas,
                indiceNuevaLinea);
        }

        if (!producto.Estado)
        {
            return Rechazo(
                ResultadoOperacionLineaCompra.ProductoInvalido,
                ConstruirMensajeProductoInactivo(producto, indiceNuevaLinea + 1),
                lineas,
                indiceNuevaLinea);
        }

        string? mensajeCantidad = ValidarCantidadDeTexto(solicitud.Cantidad, out int cantidad);
        if (mensajeCantidad is not null)
        {
            return Rechazo(
                ResultadoOperacionLineaCompra.CantidadInvalida,
                mensajeCantidad,
                lineas,
                indiceNuevaLinea);
        }

        string? mensajePrecioCompra = ValidarPrecioDeTexto(
            solicitud.PrecioCompra,
            CampoPrecio.PrecioCompra,
            out decimal precioCompra,
            out MotivoPrecioInvalido? motivoPrecioCompra);
        if (mensajePrecioCompra is not null)
        {
            return Rechazo(
                ResultadoOperacionLineaCompra.PrecioInvalido,
                mensajePrecioCompra,
                lineas,
                indiceNuevaLinea,
                CampoPrecio.PrecioCompra,
                motivoPrecioCompra);
        }

        string? mensajePrecioVenta = ValidarPrecioDeTexto(
            solicitud.PrecioVenta,
            CampoPrecio.PrecioVenta,
            out decimal precioVenta,
            out MotivoPrecioInvalido? motivoPrecioVenta);
        if (mensajePrecioVenta is not null)
        {
            return Rechazo(
                ResultadoOperacionLineaCompra.PrecioInvalido,
                mensajePrecioVenta,
                lineas,
                indiceNuevaLinea,
                CampoPrecio.PrecioVenta,
                motivoPrecioVenta);
        }

        int indiceExistente = BuscarIndiceProducto(lineas, producto.IdProducto);

        if (indiceExistente >= 0)
        {
            // RF-36 / RF-43: se acumula sobre la línea vigente con los precios
            // recién ingresados; RF-63: si la suma supera el máximo no se toca.
            int cantidadAcumulada = lineas[indiceExistente].Cantidad + cantidad;

            if (cantidadAcumulada > CalculoImportesCompra.CantidadMaxima)
            {
                return Rechazo(
                    ResultadoOperacionLineaCompra.CantidadAcumuladaSuperaMaximo,
                    MensajeCantidadAcumuladaSuperaMaximo,
                    lineas,
                    indiceExistente);
            }

            LineaCompraDto acumulada = CrearLinea(producto, cantidadAcumulada, precioCompra, precioVenta);
            List<LineaCompraDto> conAcumulacion = Reemplazar(lineas, indiceExistente, acumulada);

            return new RespuestaOperacionLineaCompra
            {
                Resultado = ResultadoOperacionLineaCompra.ExitoConAcumulacion,
                Mensaje = string.Format(
                    CultureInfo.InvariantCulture, MensajeAcumulacionAplicada, producto.Codigo, cantidadAcumulada),
                Lineas = conAcumulacion,
                IndiceLineaAfectada = indiceExistente,
                MontoTotal = CalculoImportesCompra.CalcularMontoTotal(conAcumulacion.Select(l => l.Subtotal))
            };
        }

        LineaCompraDto nuevaLinea = CrearLinea(producto, cantidad, precioCompra, precioVenta);
        var conNuevaLinea = new List<LineaCompraDto>(lineas) { nuevaLinea };

        return new RespuestaOperacionLineaCompra
        {
            Resultado = ResultadoOperacionLineaCompra.Exito,
            Mensaje = MensajeLineaAgregada,
            Lineas = conNuevaLinea,
            IndiceLineaAfectada = conNuevaLinea.Count - 1,
            MontoTotal = CalculoImportesCompra.CalcularMontoTotal(conNuevaLinea.Select(l => l.Subtotal))
        };
    }

    /// <summary>
    /// Guardar la corrección de una línea existente (RF-30 y RF-62). El índice se
    /// valida <b>antes</b> de consultar la base; ante cualquier rechazo la lista
    /// vuelve intacta (RNF-11).
    /// </summary>
    public async Task<RespuestaOperacionLineaCompra> EditarLineaAsync(SolicitudEditarLineaCompraDto solicitud)
    {
        ArgumentNullException.ThrowIfNull(solicitud);

        IReadOnlyList<LineaCompraDto> lineas = solicitud.LineasActuales ?? Array.Empty<LineaCompraDto>();

        if (solicitud.IndiceLinea < 0 || solicitud.IndiceLinea >= lineas.Count)
        {
            return Rechazo(
                ResultadoOperacionLineaCompra.LineaNoEncontrada,
                MensajeLineaInexistente,
                lineas);
        }

        int numeroLinea = solicitud.IndiceLinea + 1;
        Dictionary<int, ProductoDeCompra> productos = await ValidarProductosAsync(new[] { solicitud.IdProducto });

        if (!productos.TryGetValue(solicitud.IdProducto, out ProductoDeCompra? producto))
        {
            return Rechazo(
                ResultadoOperacionLineaCompra.ProductoInvalido,
                ConstruirMensajeProductoInexistente(numeroLinea),
                lineas,
                solicitud.IndiceLinea);
        }

        if (!producto.Estado)
        {
            return Rechazo(
                ResultadoOperacionLineaCompra.ProductoInvalido,
                ConstruirMensajeProductoInactivo(producto, numeroLinea),
                lineas,
                solicitud.IndiceLinea);
        }

        if (ExisteProductoDuplicado(lineas, producto.IdProducto, solicitud.IndiceLinea))
        {
            // RF-62 / D-18: el producto ya está en otra línea; no se modifica nada.
            return Rechazo(
                ResultadoOperacionLineaCompra.ProductoDuplicadoEnEdicion,
                ConstruirMensajeProductoDuplicadoEnEdicion(producto),
                lineas,
                solicitud.IndiceLinea);
        }

        string? mensajeCantidad = ValidarCantidadDeTexto(solicitud.Cantidad, out int cantidad);
        if (mensajeCantidad is not null)
        {
            return Rechazo(
                ResultadoOperacionLineaCompra.CantidadInvalida,
                mensajeCantidad,
                lineas,
                solicitud.IndiceLinea);
        }

        string? mensajePrecioCompra = ValidarPrecioDeTexto(
            solicitud.PrecioCompra,
            CampoPrecio.PrecioCompra,
            out decimal precioCompra,
            out MotivoPrecioInvalido? motivoPrecioCompra);
        if (mensajePrecioCompra is not null)
        {
            return Rechazo(
                ResultadoOperacionLineaCompra.PrecioInvalido,
                mensajePrecioCompra,
                lineas,
                solicitud.IndiceLinea,
                CampoPrecio.PrecioCompra,
                motivoPrecioCompra);
        }

        string? mensajePrecioVenta = ValidarPrecioDeTexto(
            solicitud.PrecioVenta,
            CampoPrecio.PrecioVenta,
            out decimal precioVenta,
            out MotivoPrecioInvalido? motivoPrecioVenta);
        if (mensajePrecioVenta is not null)
        {
            return Rechazo(
                ResultadoOperacionLineaCompra.PrecioInvalido,
                mensajePrecioVenta,
                lineas,
                solicitud.IndiceLinea,
                CampoPrecio.PrecioVenta,
                motivoPrecioVenta);
        }

        LineaCompraDto actualizada = CrearLinea(producto, cantidad, precioCompra, precioVenta);
        List<LineaCompraDto> conReemplazo = Reemplazar(lineas, solicitud.IndiceLinea, actualizada);

        return new RespuestaOperacionLineaCompra
        {
            Resultado = ResultadoOperacionLineaCompra.Exito,
            Mensaje = MensajeLineaActualizada,
            Lineas = conReemplazo,
            IndiceLineaAfectada = solicitud.IndiceLinea,
            MontoTotal = CalculoImportesCompra.CalcularMontoTotal(conReemplazo.Select(l => l.Subtotal))
        };
    }

    /// <summary>
    /// Quitar una línea de la compra en carga (RF-29). Es síncrono y no toca la
    /// base (RNF-4): si el índice no existe, la lista vuelve intacta.
    /// </summary>
    public RespuestaCompraEnCarga QuitarLinea(IReadOnlyList<LineaCompraDto> lineas, int indice)
    {
        IReadOnlyList<LineaCompraDto> actuales = lineas ?? Array.Empty<LineaCompraDto>();

        if (indice < 0 || indice >= actuales.Count)
        {
            return new RespuestaCompraEnCarga
            {
                Exito = false,
                Mensaje = MensajeLineaInexistente,
                Lineas = actuales,
                MontoTotal = CalculoImportesCompra.CalcularMontoTotal(actuales.Select(l => l.Subtotal))
            };
        }

        var restantes = new List<LineaCompraDto>(actuales);
        restantes.RemoveAt(indice);

        return new RespuestaCompraEnCarga
        {
            Exito = true,
            Mensaje = string.Empty,
            Lineas = restantes,
            MontoTotal = CalculoImportesCompra.CalcularMontoTotal(restantes.Select(l => l.Subtotal))
        };
    }

    /// <summary>
    /// Subtotal del área de captura (RF-65 / D-21): solo es válido si hay
    /// producto elegido y la cantidad y ambos precios son válidos. Es síncrono,
    /// no consulta la base y no suma al total de la grilla.
    /// </summary>
    public SubtotalLineaDto CalcularSubtotalEnPreparacion(
        int? idProducto, string? cantidad, string? precioCompra, string? precioVenta)
    {
        if (!idProducto.HasValue || idProducto.Value <= 0)
        {
            return new SubtotalLineaDto { EsValido = false };
        }

        string? mensajeCantidad = ValidarCantidadDeTexto(cantidad, out int cantidadValidada);
        string? mensajePrecioCompra = ValidarPrecioDeTexto(
            precioCompra, CampoPrecio.PrecioCompra, out decimal precioCompraValidado, out _);
        string? mensajePrecioVenta = ValidarPrecioDeTexto(
            precioVenta, CampoPrecio.PrecioVenta, out _, out _);

        if (mensajeCantidad is not null || mensajePrecioCompra is not null || mensajePrecioVenta is not null)
        {
            return new SubtotalLineaDto { EsValido = false };
        }

        return new SubtotalLineaDto
        {
            EsValido = true,
            Subtotal = CalculoImportesCompra.CalcularSubtotal(cantidadValidada, precioCompraValidado)
        };
    }

    /// <summary>Texto de confirmación para descartar la compra en carga (RF-31).</summary>
    public string ObtenerMensajeConfirmacionCancelarCompra() => MensajeConfirmacionCancelarCompra;

    // ---------- Historial y detalle para Reportes (RF-49 … RF-54) ----------

    /// <summary>
    /// Historial completo de compras registradas, activas e inactivas (RF-49 …
    /// RF-51), ordenado por <c>FechaRegistro</c> descendente y desempatado por
    /// <c>IdCompra</c> descendente <b>en la consulta</b>. La proyección es
    /// defensiva: un proveedor o un usuario inexistente no lanza excepción y se
    /// informa con los textos fijos del negocio (§3.4).
    /// </summary>
    /// <param name="rol">
    /// Rol de la sesión. Se evalúa <b>primero</b> y sin tocar la base: solo
    /// Administrador y Encargado pueden consultar compras (RF-59 y RF-61); el rol
    /// no se relee del origen de datos (D-14).
    /// </param>
    /// <remarks>
    /// Devuelve datos crudos: la fecha viaja como <see cref="DateTime"/>, el
    /// estado como <see cref="bool"/> y el importe como <see cref="decimal"/>
    /// (RNF-12). No hay paginación ni escrituras y del usuario solo se expone
    /// nombre y apellido (RNF-5). Un fallo de lectura se informa como
    /// <c>ErrorPersistencia</c> con el texto único (RF-57).
    /// </remarks>
    public async Task<RespuestaHistorialCompras> ObtenerHistorialAsync(string? rol)
    {
        if (!PoliticaAcceso.PuedeOperarCompras(rol))
        {
            return new RespuestaHistorialCompras
            {
                Resultado = ResultadoHistorialCompras.PermisoDenegado,
                Mensaje = MensajePermisoConsultarCompras
            };
        }

        try
        {
            // Proyección escalar con subconsultas correlacionadas: la navegación
            // requerida como `c.Proveedor.RazonSocial` haría un INNER JOIN y
            // ocultaría la fila cuando la clave foránea no existe.
            List<FilaHistorial> filas = await _context.Compras
                .AsNoTracking()
                .OrderByDescending(c => c.FechaRegistro)
                .ThenByDescending(c => c.IdCompra)
                .Select(c => new FilaHistorial
                {
                    IdCompra = c.IdCompra,
                    FechaRegistro = c.FechaRegistro,
                    RazonSocialProveedor = _context.Proveedores
                        .Where(p => p.IdProveedor == c.IdProveedor)
                        .Select(p => p.RazonSocial)
                        .FirstOrDefault(),
                    Nombre = _context.Usuarios
                        .Where(u => u.IdUsuario == c.IdUsuario)
                        .Select(u => u.Nombre)
                        .FirstOrDefault(),
                    Apellido = _context.Usuarios
                        .Where(u => u.IdUsuario == c.IdUsuario)
                        .Select(u => u.Apellido)
                        .FirstOrDefault(),
                    MontoTotal = c.MontoTotal,
                    Estado = c.Estado
                })
                .ToListAsync();

            List<HistorialCompraDto> compras = filas
                .Select(f => new HistorialCompraDto
                {
                    IdCompra = f.IdCompra,
                    FechaRegistro = f.FechaRegistro,
                    RazonSocialProveedor = TextoProveedor(f.RazonSocialProveedor),
                    NombreUsuario = TextoUsuario(f.Nombre, f.Apellido),
                    MontoTotal = f.MontoTotal,
                    Estado = f.Estado
                })
                .ToList();

            if (compras.Count == 0)
            {
                return new RespuestaHistorialCompras
                {
                    Resultado = ResultadoHistorialCompras.SinCompras,
                    Mensaje = MensajeHistorialVacio
                };
            }

            return new RespuestaHistorialCompras
            {
                Resultado = ResultadoHistorialCompras.ConCompras,
                Mensaje = compras.Count == 1
                    ? MensajeUnaCompraEncontrada
                    : string.Format(CultureInfo.InvariantCulture, MensajeComprasEncontradas, compras.Count),
                Compras = compras
            };
        }
        catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
        {
            return new RespuestaHistorialCompras
            {
                Resultado = ResultadoHistorialCompras.ErrorPersistencia,
                Mensaje = MensajeErrorPersistencia
            };
        }
    }

    /// <summary>
    /// Compra registrada con todas sus líneas para Reportes (RF-53(b) y RF-54):
    /// cabecera con los mismos datos que el historial y líneas ordenadas por
    /// <c>IdDetalleCompra</c>. El detalle no incluye el precio de venta porque no
    /// se persiste (D-2 y D-17) y la proyección es defensiva igual que la del
    /// historial (§3.4).
    /// </summary>
    /// <param name="idCompra">Identificador de la compra solicitada.</param>
    /// <param name="rol">
    /// Rol de la sesión. Se evalúa <b>primero</b> y sin tocar la base: solo
    /// Administrador y Encargado pueden consultar compras (RF-59 y RF-61); el rol
    /// no se relee del origen de datos (D-14).
    /// </param>
    /// <remarks>
    /// No escribe nada y no rastrea entidades (RNF-3). Compra inexistente ⇒
    /// <c>CompraNoEncontrada</c> con el mensaje del catálogo; fallo de lectura ⇒
    /// <c>ErrorPersistencia</c> con el texto único (RF-57).
    /// </remarks>
    public async Task<RespuestaCompraConDetalle> ObtenerCompraConDetalleAsync(int idCompra, string? rol)
    {
        if (!PoliticaAcceso.PuedeOperarCompras(rol))
        {
            return new RespuestaCompraConDetalle
            {
                Resultado = ResultadoCompraConDetalle.PermisoDenegado,
                Mensaje = MensajePermisoConsultarCompras
            };
        }

        try
        {
            FilaHistorial? cabecera = await _context.Compras
                .AsNoTracking()
                .Where(c => c.IdCompra == idCompra)
                .Select(c => new FilaHistorial
                {
                    IdCompra = c.IdCompra,
                    FechaRegistro = c.FechaRegistro,
                    RazonSocialProveedor = _context.Proveedores
                        .Where(p => p.IdProveedor == c.IdProveedor)
                        .Select(p => p.RazonSocial)
                        .FirstOrDefault(),
                    Nombre = _context.Usuarios
                        .Where(u => u.IdUsuario == c.IdUsuario)
                        .Select(u => u.Nombre)
                        .FirstOrDefault(),
                    Apellido = _context.Usuarios
                        .Where(u => u.IdUsuario == c.IdUsuario)
                        .Select(u => u.Apellido)
                        .FirstOrDefault(),
                    MontoTotal = c.MontoTotal,
                    Estado = c.Estado
                })
                .FirstOrDefaultAsync();

            if (cabecera is null)
            {
                return new RespuestaCompraConDetalle
                {
                    Resultado = ResultadoCompraConDetalle.CompraNoEncontrada,
                    Mensaje = MensajeCompraNoEncontrada
                };
            }

            List<FilaDetalle> filas = await _context.DetallesCompra
                .AsNoTracking()
                .Where(d => d.IdCompra == idCompra)
                .OrderBy(d => d.IdDetalleCompra)
                .Select(d => new FilaDetalle
                {
                    CodigoProducto = _context.Productos
                        .Where(p => p.IdProducto == d.IdProducto)
                        .Select(p => p.Codigo)
                        .FirstOrDefault(),
                    NombreProducto = _context.Productos
                        .Where(p => p.IdProducto == d.IdProducto)
                        .Select(p => p.Nombre)
                        .FirstOrDefault(),
                    Cantidad = d.Cantidad,
                    PrecioCompra = d.PrecioCompra,
                    Subtotal = d.Subtotal
                })
                .ToListAsync();

            return new RespuestaCompraConDetalle
            {
                Resultado = ResultadoCompraConDetalle.Encontrada,
                Mensaje = string.Empty,
                Compra = new CompraConDetalleDto
                {
                    IdCompra = cabecera.IdCompra,
                    FechaRegistro = cabecera.FechaRegistro,
                    RazonSocialProveedor = TextoProveedor(cabecera.RazonSocialProveedor),
                    NombreUsuario = TextoUsuario(cabecera.Nombre, cabecera.Apellido),
                    MontoTotal = cabecera.MontoTotal,
                    Estado = cabecera.Estado,
                    Lineas = filas
                        .Select(f => new LineaCompraDetalleDto
                        {
                            CodigoProducto = TextoProducto(f.CodigoProducto),
                            NombreProducto = TextoProducto(f.NombreProducto),
                            Cantidad = f.Cantidad,
                            PrecioCompra = f.PrecioCompra,
                            Subtotal = f.Subtotal
                        })
                        .ToList()
                }
            };
        }
        catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
        {
            return new RespuestaCompraConDetalle
            {
                Resultado = ResultadoCompraConDetalle.ErrorPersistencia,
                Mensaje = MensajeErrorPersistencia
            };
        }
    }

    // ---------- Consultas históricas producto ↔ proveedor (T11) ----------

    /// <summary>
    /// Proveedores que ya vendieron el producto indicado, según el historial de
    /// compras (RF-12). Devuelve una fila por proveedor <b>activo</b> sin repetir
    /// y en orden determinista por razón social e identificador.
    /// </summary>
    /// <param name="idProducto">Producto consultado; si no es positivo no se consulta el origen.</param>
    /// <param name="rol">
    /// Rol de la sesión. Se evalúa <b>primero</b> y sin tocar la base: solo
    /// Administrador y Encargado pueden consultar compras (RF-59 y RF-61); el rol
    /// no se relee del origen de datos (D-14).
    /// </param>
    /// <remarks>
    /// Una sola consulta con <c>AsNoTracking</c> y <c>join</c> explícito
    /// <c>DetallesCompra → Compras → Proveedores</c>, acotada por el producto y
    /// sin filtrar por <c>Compra.Estado</c>, porque el historial incluye compras
    /// inactivas (RF-49). El <c>join</c> descarta en silencio las filas huérfanas
    /// en lugar de proyectar por navegación requerida, que lanzaría excepción
    /// (§3.4). Un fallo de lectura se informa como <c>ErrorPersistencia</c> con el
    /// texto único (RF-57).
    /// </remarks>
    public async Task<RespuestaProveedoresDeProducto> ObtenerProveedoresDeProductoAsync(
        int idProducto, string? rol)
    {
        if (!PoliticaAcceso.PuedeOperarCompras(rol))
        {
            return new RespuestaProveedoresDeProducto
            {
                Resultado = ResultadoConsultaHistoricaCompra.PermisoDenegado,
                Mensaje = MensajePermisoConsultarCompras
            };
        }

        // Identificador no válido: resultado vacío sin consultar (patrón de RF-10).
        if (idProducto <= 0)
        {
            return new RespuestaProveedoresDeProducto
            {
                Resultado = ResultadoConsultaHistoricaCompra.SinResultados,
                Mensaje = string.Empty
            };
        }

        try
        {
            var filas = await (
                    from detalle in _context.DetallesCompra.AsNoTracking()
                    join compra in _context.Compras.AsNoTracking()
                        on detalle.IdCompra equals compra.IdCompra
                    join proveedor in _context.Proveedores.AsNoTracking()
                        on compra.IdProveedor equals proveedor.IdProveedor
                    where detalle.IdProducto == idProducto && proveedor.Estado
                    select new
                    {
                        proveedor.IdProveedor,
                        proveedor.RazonSocial,
                        proveedor.Cuit
                    })
                .Distinct()
                .OrderBy(p => p.RazonSocial)
                .ThenBy(p => p.IdProveedor)
                .ToListAsync();

            if (filas.Count == 0)
            {
                return new RespuestaProveedoresDeProducto
                {
                    Resultado = ResultadoConsultaHistoricaCompra.SinResultados,
                    Mensaje = MensajeSinProveedoresHistoricos
                };
            }

            return new RespuestaProveedoresDeProducto
            {
                Resultado = ResultadoConsultaHistoricaCompra.ConResultados,
                Mensaje = string.Empty,
                Proveedores = filas
                    .Select(f => new ProveedorDeProductoDto
                    {
                        IdProveedor = f.IdProveedor,
                        RazonSocial = f.RazonSocial ?? string.Empty,
                        Documento = f.Cuit ?? string.Empty
                    })
                    .ToList()
            };
        }
        catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
        {
            return new RespuestaProveedoresDeProducto
            {
                Resultado = ResultadoConsultaHistoricaCompra.ErrorPersistencia,
                Mensaje = MensajeErrorPersistencia
            };
        }
    }

    /// <summary>
    /// Productos que el proveedor indicado ya vendió, según el historial de
    /// compras (RF-15). Devuelve una fila por producto <b>activo</b> sin repetir
    /// y en orden determinista por nombre e identificador.
    /// </summary>
    /// <param name="idProveedor">Proveedor consultado; si no es positivo no se consulta el origen.</param>
    /// <param name="rol">
    /// Rol de la sesión. Se evalúa <b>primero</b> y sin tocar la base: solo
    /// Administrador y Encargado pueden consultar compras (RF-59 y RF-61); el rol
    /// no se relee del origen de datos (D-14).
    /// </param>
    /// <remarks>
    /// Una sola consulta con <c>AsNoTracking</c> y <c>join</c> explícito
    /// <c>DetallesCompra → Compras → Productos</c>, acotada por el proveedor y
    /// sin filtrar por <c>Compra.Estado</c>, porque el historial incluye compras
    /// inactivas (RF-49). El <c>join</c> descarta en silencio las filas huérfanas
    /// (§3.4). Un fallo de lectura se informa como <c>ErrorPersistencia</c> con el
    /// texto único (RF-57).
    /// </remarks>
    public async Task<RespuestaProductosDeProveedor> ObtenerProductosDeProveedorAsync(
        int idProveedor, string? rol)
    {
        if (!PoliticaAcceso.PuedeOperarCompras(rol))
        {
            return new RespuestaProductosDeProveedor
            {
                Resultado = ResultadoConsultaHistoricaCompra.PermisoDenegado,
                Mensaje = MensajePermisoConsultarCompras
            };
        }

        // Identificador no válido: resultado vacío sin consultar (patrón de RF-16).
        if (idProveedor <= 0)
        {
            return new RespuestaProductosDeProveedor
            {
                Resultado = ResultadoConsultaHistoricaCompra.SinResultados,
                Mensaje = string.Empty
            };
        }

        try
        {
            var filas = await (
                    from detalle in _context.DetallesCompra.AsNoTracking()
                    join compra in _context.Compras.AsNoTracking()
                        on detalle.IdCompra equals compra.IdCompra
                    join producto in _context.Productos.AsNoTracking()
                        on detalle.IdProducto equals producto.IdProducto
                    where compra.IdProveedor == idProveedor && producto.Estado
                    select new
                    {
                        producto.IdProducto,
                        producto.Codigo,
                        producto.Nombre
                    })
                .Distinct()
                .OrderBy(p => p.Nombre)
                .ThenBy(p => p.IdProducto)
                .ToListAsync();

            if (filas.Count == 0)
            {
                return new RespuestaProductosDeProveedor
                {
                    Resultado = ResultadoConsultaHistoricaCompra.SinResultados,
                    Mensaje = MensajeSinProductosHistoricos
                };
            }

            return new RespuestaProductosDeProveedor
            {
                Resultado = ResultadoConsultaHistoricaCompra.ConResultados,
                Mensaje = string.Empty,
                Productos = filas
                    .Select(f => new ProductoDeProveedorDto
                    {
                        IdProducto = f.IdProducto,
                        Codigo = f.Codigo ?? string.Empty,
                        Nombre = f.Nombre ?? string.Empty
                    })
                    .ToList()
            };
        }
        catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
        {
            return new RespuestaProductosDeProveedor
            {
                Resultado = ResultadoConsultaHistoricaCompra.ErrorPersistencia,
                Mensaje = MensajeErrorPersistencia
            };
        }
    }

    /// <summary>
    /// Fila cruda de la cabecera de una compra (solo escalares, sin entidades
    /// rastreadas) antes de aplicar los textos defensivos de §3.4.
    /// </summary>
    private sealed class FilaHistorial
    {
        public int IdCompra { get; set; }

        public DateTime FechaRegistro { get; set; }

        public string? RazonSocialProveedor { get; set; }

        public string? Nombre { get; set; }

        public string? Apellido { get; set; }

        public decimal MontoTotal { get; set; }

        public bool Estado { get; set; }
    }

    /// <summary>Fila cruda de una línea del detalle, sin entidades rastreadas.</summary>
    private sealed class FilaDetalle
    {
        public string? CodigoProducto { get; set; }

        public string? NombreProducto { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioCompra { get; set; }

        public decimal Subtotal { get; set; }
    }

    /// <summary>Razón social del proveedor o el texto defensivo de §3.4.</summary>
    private static string TextoProveedor(string? razonSocial)
        => string.IsNullOrWhiteSpace(razonSocial) ? TextoSinProveedor : razonSocial.Trim();

    /// <summary>Nombre + apellido recortado, o el texto defensivo de §3.4.</summary>
    private static string TextoUsuario(string? nombre, string? apellido)
    {
        string nombreCompleto =
            $"{(nombre ?? string.Empty).Trim()} {(apellido ?? string.Empty).Trim()}".Trim();

        return nombreCompleto.Length == 0 ? TextoSinUsuario : nombreCompleto;
    }

    /// <summary>Código o nombre del producto; cadena vacía si no existe (D-17).</summary>
    private static string TextoProducto(string? texto)
        => string.IsNullOrWhiteSpace(texto) ? string.Empty : texto.Trim();

    // ---------- Validaciones privadas ----------

    /// <summary>
    /// Paso 1 del registro (RF-28, RF-59 y RF-60): solo Administrador y Encargado
    /// pueden registrar compras. No consulta el origen de datos: el rol llega por
    /// parámetro desde la sesión y no se relee de la base (D-14).
    /// </summary>
    /// <returns>
    /// El mensaje del catálogo si el rol no está autorizado, o
    /// <see langword="null"/> si puede registrar compras.
    /// </returns>
    private static string? ValidarRolParaRegistro(string? rol)
    {
        return PoliticaAcceso.PuedeOperarCompras(rol) ? null : MensajePermisoRegistrarCompra;
    }

    /// <summary>
    /// Valida el proveedor elegido antes de confirmar la compra (RF-5 y RF-14):
    /// es obligatorio, debe existir y estar activo. Consulta acotada con
    /// proyección mínima y <c>AsNoTracking</c> (RNF-3), y no consulta el origen
    /// de datos si el identificador es nulo o no positivo.
    /// </summary>
    /// <returns>
    /// El mensaje del catálogo del primer defecto, o <see langword="null"/> si el
    /// proveedor es válido. El identificador afectado es el recibido, que el
    /// llamador ya conoce. Un fallo del origen de datos no se captura aquí: lo
    /// informa el registro como <c>ErrorPersistencia</c> (RF-56).
    /// </returns>
    private async Task<string?> ValidarProveedorAsync(int? idProveedor)
    {
        if (!idProveedor.HasValue || idProveedor.Value <= 0)
        {
            return MensajeProveedorRequerido;
        }

        var proveedor = await _context.Proveedores
            .AsNoTracking()
            .Where(p => p.IdProveedor == idProveedor.Value)
            .Select(p => new { p.IdProveedor, p.RazonSocial, p.Estado })
            .FirstOrDefaultAsync();

        if (proveedor is null)
        {
            return MensajeProveedorInexistente;
        }

        if (!proveedor.Estado)
        {
            return string.Format(
                CultureInfo.InvariantCulture, MensajeProveedorInactivo, proveedor.RazonSocial ?? string.Empty);
        }

        return null;
    }

    /// <summary>
    /// Valida el usuario responsable de la compra (RF-5 y D-14): debe existir y
    /// estar activo. <b>No</b> relee el rol de la base: el rol es el de la sesión
    /// y se valida aparte (T8), así que la proyección no incluye
    /// <c>IdRol</c> ni la navegación <c>Rol</c>. Sin consultar si el
    /// identificador no es positivo.
    /// </summary>
    /// <returns>
    /// El mensaje del catálogo del primer defecto, o <see langword="null"/> si el
    /// usuario es válido. Un fallo del origen de datos no se captura aquí: lo
    /// informa el registro como <c>ErrorPersistencia</c> (RF-56).
    /// </returns>
    private async Task<string?> ValidarUsuarioAsync(int idUsuario)
    {
        if (idUsuario <= 0)
        {
            return MensajeUsuarioSinIdentificar;
        }

        var usuario = await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.IdUsuario == idUsuario)
            .Select(u => new { u.IdUsuario, u.Estado })
            .FirstOrDefaultAsync();

        if (usuario is null)
        {
            return MensajeUsuarioInexistente;
        }

        if (!usuario.Estado)
        {
            return MensajeUsuarioInactivo;
        }

        return null;
    }

    /// <summary>
    /// Revalida el precio ya numérico que transporta el DTO de registro
    /// (plan §6.4) con el orden interno de §6.3 una vez descartados «vacío» y
    /// «formato»: <c>≤ 0</c> (RF-22 / RF-23) → más de dos decimales (RF-26) →
    /// mayor que 999.999.999,99 (RF-24). No lanza por datos inválidos: devuelve
    /// el mensaje del catálogo del campo indicado.
    /// </summary>
    /// <param name="precio">Valor numérico recibido; no se redondea ni se trunca (RNF-1).</param>
    /// <param name="campo">Campo que originó el rechazo, para elegir el texto y el motivo.</param>
    /// <param name="motivo">Motivo tipado del rechazo; <see langword="null"/> si es válido.</param>
    /// <returns>
    /// El mensaje del catálogo del primer defecto, o <see langword="null"/> si el
    /// precio es válido. El identificador afectado (línea y campo) lo aporta el
    /// llamador, que recorre las líneas.
    /// </returns>
    /// <remarks>
    /// <b>Ceros finales.</b> Se cuentan los <i>decimales significativos</i>: los
    /// ceros finales no cuentan, porque el tipo <see cref="decimal"/> conserva la
    /// escala con la que se construyó el valor y <c>10.330m</c> representa
    /// exactamente el mismo número que <c>10.33m</c>. Rechazarlo sería un falso
    /// negativo: guardarlo en la columna decimal de dos decimales no pierde valor
    /// y RNF-1 prohíbe redondear o truncar <i>para corregir</i> un dato, no
    /// prohíbe aceptar una representación equivalente. Los ceros finales
    /// <i>internos</i> sí cuentan: <c>10.003m</c> tiene tres decimales
    /// significativos. En cambio, el texto de la grilla sigue la regla de RF-25
    /// del analizador (los dígitos escritos después del separador cuentan aunque
    /// sean ceros: <c>1.000</c> es «tres decimales»).
    /// </remarks>
    private static string? ValidarPrecioRecibido(
        decimal precio, CampoPrecio campo, out MotivoPrecioInvalido? motivo)
    {
        motivo = null;

        if (precio <= 0m)
        {
            motivo = MotivoPrecioInvalido.NoPositivo;
        }
        else if (ContarDecimalesSignificativos(precio) > CalculoImportesCompra.DecimalesPrecio)
        {
            motivo = MotivoPrecioInvalido.DecimalesExcedidos;
        }
        else if (precio > CalculoImportesCompra.PrecioMaximo)
        {
            motivo = MotivoPrecioInvalido.SuperaMaximo;
        }

        return motivo.HasValue ? ConstruirMensajePrecio(campo, motivo.Value) : null;
    }

    /// <summary>
    /// Cantidad de decimales significativos de un valor positivo, sin ceros
    /// finales (<c>10.330m</c> ⇒ 2; <c>10.333m</c> ⇒ 3). Multiplica por diez de
    /// forma iterativa sobre la parte fraccionaria y termina en, como máximo,
    /// 28 pasos (los decimales que caben en <see cref="decimal"/>), sin
    /// redondear ni truncar y sin desbordar: el valor con decimales nunca supera
    /// el máximo del tipo al desplazarse.
    /// </summary>
    private static int ContarDecimalesSignificativos(decimal valor)
    {
        decimal resto = valor;
        int decimales = 0;

        while (resto != decimal.Truncate(resto))
        {
            resto *= 10m;
            decimales++;
        }

        return decimales;
    }

    /// <summary>
    /// Consulta acotada y única de los productos de la compra (RF-18, RNF-2 y
    /// RNF-3): proyección mínima, <c>AsNoTracking</c> y <c>Contains</c> de los
    /// identificadores; nunca materializa el catálogo.
    /// </summary>
    private async Task<Dictionary<int, ProductoDeCompra>> ValidarProductosAsync(IEnumerable<int> idsProducto)
    {
        int[] ids = idsProducto.Distinct().ToArray();

        if (ids.Length == 0)
        {
            return new Dictionary<int, ProductoDeCompra>();
        }

        List<ProductoDeCompra> productos = await _context.Productos
            .AsNoTracking()
            .Where(p => ids.Contains(p.IdProducto))
            .Select(p => new ProductoDeCompra
            {
                IdProducto = p.IdProducto,
                Codigo = p.Codigo ?? string.Empty,
                Nombre = p.Nombre ?? string.Empty,
                Estado = p.Estado
            })
            .ToListAsync();

        return productos.ToDictionary(p => p.IdProducto);
    }

    /// <summary>
    /// Revalida el rango de una cantidad ya interpretada (RF-20 y RF-21) con las
    /// constantes de D-15. Devuelve <see langword="null"/> si es válida.
    /// </summary>
    private static string? ValidarCantidad(int cantidad)
    {
        if (cantidad < CalculoImportesCompra.CantidadMinima)
        {
            return MensajeCantidadInvalida;
        }

        if (cantidad > CalculoImportesCompra.CantidadMaxima)
        {
            return MensajeCantidadSuperaMaximo;
        }

        return null;
    }

    /// <summary>
    /// Interpreta y valida la cantidad escrita en el área de captura (RF-20 y
    /// RF-21) usando <see cref="AnalizadorCantidad"/>; devuelve el mensaje del
    /// catálogo o <see langword="null"/> si es válida.
    /// </summary>
    private static string? ValidarCantidadDeTexto(string? texto, out int cantidad)
    {
        ResultadoAnalisisCantidad analisis = AnalizadorCantidad.Analizar(texto);
        cantidad = analisis.Valor;

        if (analisis.Motivo == MotivoCantidadInvalida.SuperaMaximo)
        {
            return MensajeCantidadSuperaMaximo;
        }

        return analisis.EsValido ? ValidarCantidad(cantidad) : MensajeCantidadInvalida;
    }

    /// <summary>
    /// Interpreta y valida un precio escrito en el área de captura (RF-22 … RF-26)
    /// con <see cref="AnalizadorImporte"/> y devuelve el mensaje del catálogo para
    /// el campo indicado, o <see langword="null"/> si es válido.
    /// </summary>
    private static string? ValidarPrecioDeTexto(
        string? texto, CampoPrecio campo, out decimal precio, out MotivoPrecioInvalido? motivo)
    {
        ResultadoAnalisisImporte analisis = AnalizadorImporte.Analizar(texto);
        precio = analisis.Valor;
        motivo = analisis.Motivo;

        if (analisis.EsValido)
        {
            return null;
        }

        return ConstruirMensajePrecio(campo, analisis.Motivo!.Value);
    }

    /// <summary>Mensaje del catálogo para un precio rechazado, según campo y motivo (RF-55).</summary>
    private static string ConstruirMensajePrecio(CampoPrecio campo, MotivoPrecioInvalido motivo)
    {
        return (campo, motivo) switch
        {
            (CampoPrecio.PrecioCompra, MotivoPrecioInvalido.Vacio) => MensajePrecioCompraVacio,
            (CampoPrecio.PrecioCompra, MotivoPrecioInvalido.FormatoInvalido) => MensajeFormatoPrecioCompra,
            (CampoPrecio.PrecioCompra, MotivoPrecioInvalido.NoPositivo) => MensajePrecioCompraNoPositivo,
            (CampoPrecio.PrecioCompra, MotivoPrecioInvalido.DecimalesExcedidos) => MensajeDecimalesPrecioCompra,
            (CampoPrecio.PrecioCompra, MotivoPrecioInvalido.SuperaMaximo) => MensajePrecioCompraSuperaMaximo,
            (CampoPrecio.PrecioVenta, MotivoPrecioInvalido.Vacio) => MensajePrecioVentaVacio,
            (CampoPrecio.PrecioVenta, MotivoPrecioInvalido.FormatoInvalido) => MensajeFormatoPrecioVenta,
            (CampoPrecio.PrecioVenta, MotivoPrecioInvalido.NoPositivo) => MensajePrecioVentaNoPositivo,
            (CampoPrecio.PrecioVenta, MotivoPrecioInvalido.DecimalesExcedidos) => MensajeDecimalesPrecioVenta,
            _ => MensajePrecioVentaSuperaMaximo
        };
    }

    /// <summary>Mensaje de producto inexistente en la línea indicada (RF-18).</summary>
    private static string ConstruirMensajeProductoInexistente(int numeroLinea)
        => string.Format(CultureInfo.InvariantCulture, MensajeProductoInexistente, numeroLinea);

    /// <summary>Mensaje de producto inactivo identificado por código y línea (RF-18).</summary>
    private static string ConstruirMensajeProductoInactivo(ProductoDeCompra producto, int numeroLinea)
        => string.Format(
            CultureInfo.InvariantCulture, MensajeProductoInactivo, producto.Codigo, producto.Nombre, numeroLinea);

    /// <summary>Mensaje de producto repetido en el detalle al confirmar (RF-37, T5b).</summary>
    private static string ConstruirMensajeProductoRepetido(ProductoDeCompra producto)
        => string.Format(CultureInfo.InvariantCulture, MensajeProductoRepetido, producto.Codigo, producto.Nombre);

    /// <summary>Mensaje del rechazo de la edición cuando el producto ya está en otra línea (RF-62).</summary>
    private static string ConstruirMensajeProductoDuplicadoEnEdicion(ProductoDeCompra producto)
        => string.Format(
            CultureInfo.InvariantCulture, MensajeProductoDuplicadoEnEdicion, producto.Codigo, producto.Nombre);

    /// <summary>Índice de la línea que ya carga el producto, o <c>-1</c> si no está (RF-37).</summary>
    private static int BuscarIndiceProducto(IReadOnlyList<LineaCompraDto> lineas, int idProducto)
    {
        for (int i = 0; i < lineas.Count; i++)
        {
            if (lineas[i].IdProducto == idProducto)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>Indica si el producto ya está en el detalle, con la línea que se edita excluida (RF-37 y RF-62).</summary>
    private static bool ExisteProductoDuplicado(
        IReadOnlyList<LineaCompraDto> lineas, int idProducto, int? indiceExcluir = null)
    {
        int indice = BuscarIndiceProducto(lineas, idProducto);

        return indice >= 0 && indice != indiceExcluir;
    }

    /// <summary>Construye la línea en carga con el subtotal calculado por la regla única (RF-33 y RNF-13).</summary>
    private static LineaCompraDto CrearLinea(
        ProductoDeCompra producto, int cantidad, decimal precioCompra, decimal precioVenta)
    {
        return new LineaCompraDto
        {
            IdProducto = producto.IdProducto,
            CodigoProducto = producto.Codigo,
            NombreProducto = producto.Nombre,
            Cantidad = cantidad,
            PrecioCompra = precioCompra,
            PrecioVenta = precioVenta,
            Subtotal = CalculoImportesCompra.CalcularSubtotal(cantidad, precioCompra)
        };
    }

    /// <summary>Copia de la lista con la línea del índice reemplazada.</summary>
    private static List<LineaCompraDto> Reemplazar(
        IReadOnlyList<LineaCompraDto> lineas, int indice, LineaCompraDto linea)
    {
        var resultado = new List<LineaCompraDto>(lineas);
        resultado[indice] = linea;

        return resultado;
    }

    /// <summary>
    /// Respuesta de rechazo con la grilla **intacta** y su total (RNF-11). El
    /// índice, el campo y el motivo identifican el primer error (RF-28).
    /// </summary>
    private static RespuestaOperacionLineaCompra Rechazo(
        ResultadoOperacionLineaCompra resultado,
        string mensaje,
        IReadOnlyList<LineaCompraDto> lineas,
        int? indiceLineaAfectada = null,
        CampoPrecio? campoPrecio = null,
        MotivoPrecioInvalido? motivoPrecio = null)
    {
        return new RespuestaOperacionLineaCompra
        {
            Resultado = resultado,
            Mensaje = mensaje,
            Lineas = lineas,
            IndiceLineaAfectada = indiceLineaAfectada,
            MontoTotal = CalculoImportesCompra.CalcularMontoTotal(lineas.Select(l => l.Subtotal)),
            CampoPrecio = campoPrecio,
            MotivoPrecio = motivoPrecio
        };
    }

    /// <summary>
    /// Respuesta de rechazo del registro (RF-28): solo el primer error, sin
    /// identificador de compra y con el monto total en cero.
    /// </summary>
    private static RespuestaRegistroCompra RechazoRegistro(
        ResultadoRegistroCompra resultado,
        string mensaje,
        int? indiceLineaAfectada = null,
        int? idProductoAfectado = null,
        CampoPrecio? campoPrecio = null,
        MotivoPrecioInvalido? motivoPrecio = null)
    {
        return new RespuestaRegistroCompra
        {
            Resultado = resultado,
            Mensaje = mensaje,
            IndiceLineaAfectada = indiceLineaAfectada,
            IdProductoAfectado = idProductoAfectado,
            CampoPrecio = campoPrecio,
            MotivoPrecio = motivoPrecio
        };
    }

    /// <summary>
    /// Criterio único de fallo del origen de datos (RF-45, RF-56 y RF-57), igual
    /// que <c>ProveedorNegocio</c>. Lo usan las búsquedas (T4), el registro (T6)
    /// y el historial (T7); ninguna operación de esta tarea escribe en la base.
    /// </summary>
    private static bool EsFalloDeOrigenDeDatos(Exception excepcion)
    {
        return excepcion is DbUpdateException
            || excepcion is System.Data.Common.DbException
            || excepcion is InvalidOperationException;
    }

    /// <summary>Proyección mínima y propia de Compras para validar productos (RNF-5).</summary>
    private sealed class ProductoDeCompra
    {
        public int IdProducto { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public bool Estado { get; set; }
    }
}
