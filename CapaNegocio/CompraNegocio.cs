using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CapaDatos;
using CapaEntidad;
using CapaNegocio.Compras;
using CapaNegocio.DTOs.Compras;
using CapaNegocio.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CapaNegocio;

/// <summary>
/// Servicio de negocio para la gestión de compras y solicitudes de reabastecimiento.
/// </summary>
public partial class CompraNegocio
{
    private readonly AppDbContext _context;
    private readonly IDbContextFactory<AppDbContext>? _fabricaLectura;
    private readonly IServicioCorreoProveedor _servicioCorreo;

    public CompraNegocio(
        AppDbContext contexto,
        IDbContextFactory<AppDbContext>? fabricaLectura = null,
        IServicioCorreoProveedor? servicioCorreo = null)
    {
        ArgumentNullException.ThrowIfNull(contexto);
        _context = contexto;
        _fabricaLectura = fabricaLectura;
        _servicioCorreo = servicioCorreo ?? new ServicioCorreoProveedor();
    }

    private async Task<T> EjecutarLecturaAsync<T>(Func<AppDbContext, Task<T>> lectura)
    {
        if (_fabricaLectura is null)
        {
            return await lectura(_context);
        }

        await using AppDbContext db = await _fabricaLectura.CreateDbContextAsync();
        return await lectura(db);
    }

    /// <summary>
    /// Registra una compra inmediata o una solicitud de reabastecimiento a proveedor.
    /// </summary>
    public async Task<RespuestaRegistroCompra> RegistrarCompraAsync(
        SolicitudRegistrarCompraDto solicitud, int idUsuario, string? rol)
    {
        ArgumentNullException.ThrowIfNull(solicitud);

        string? mensajeRol = ValidarRolParaRegistro(rol);
        if (mensajeRol is not null)
        {
            return RechazoRegistro(ResultadoRegistroCompra.PermisoDenegado, mensajeRol);
        }

        IReadOnlyList<LineaCompraDto> lineas = solicitud.Lineas ?? Array.Empty<LineaCompraDto>();
        Dictionary<int, ProductoDeCompra> productos;

        try
        {
            string? mensajeProveedor = await ValidarProveedorAsync(solicitud.IdProveedor);
            if (mensajeProveedor is not null)
            {
                return RechazoRegistro(ResultadoRegistroCompra.ProveedorInvalido, mensajeProveedor);
            }

            string? mensajeUsuario = await ValidarUsuarioAsync(idUsuario);
            if (mensajeUsuario is not null)
            {
                return RechazoRegistro(ResultadoRegistroCompra.UsuarioInvalido, mensajeUsuario);
            }

            productos = await ValidarProductosAsync(lineas.Select(l => l.IdProducto));
        }
        catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
        {
            return RechazoRegistro(ResultadoRegistroCompra.ErrorPersistencia, MensajeErrorPersistencia);
        }

        var idsVistos = new HashSet<int>();
        var subtotales = new List<decimal>(lineas.Count);
        bool esPendiente = string.Equals(solicitud.EstadoSolicitud, "Pendiente", StringComparison.OrdinalIgnoreCase);

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

            if ((long)producto.Stock + linea.Cantidad > int.MaxValue)
            {
                return RechazoRegistro(
                    ResultadoRegistroCompra.CantidadInvalida,
                    MensajeCantidadAcumuladaSuperaMaximo,
                    i,
                    linea.IdProducto);
            }

            if (!esPendiente || linea.PrecioCompra > 0m)
            {
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
            }

            if (!esPendiente || linea.PrecioVenta > 0m)
            {
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
            }

            subtotales.Add(CalculoImportesCompra.CalcularSubtotal(linea.Cantidad, linea.PrecioCompra));
        }

        if (lineas.Count == 0)
        {
            return RechazoRegistro(ResultadoRegistroCompra.SinItems, MensajeSinItems);
        }

        decimal montoTotal = CalculoImportesCompra.CalcularMontoTotal(subtotales);
        if (montoTotal > CalculoImportesCompra.MontoTotalMaximo)
        {
            return RechazoRegistro(
                ResultadoRegistroCompra.MontoTotalSuperaMaximo, MensajeMontoTotalSuperaMaximo);
        }

        IDbContextTransaction? transaccion = null;
        Compra? cabecera = null;

        try
        {
            transaccion = await _context.Database.BeginTransactionAsync();

            DateTime fechaRegistro = DateTime.Now;
            bool esPendienteCabecera = string.Equals(solicitud.EstadoSolicitud, "Pendiente", StringComparison.OrdinalIgnoreCase);

            cabecera = new Compra
            {
                IdUsuario = idUsuario,
                IdProveedor = solicitud.IdProveedor!.Value,
                MontoTotal = montoTotal,
                Estado = true,
                EstadoSolicitud = esPendienteCabecera ? "Pendiente" : "Recibida",
                Observaciones = solicitud.Observaciones,
                FechaRegistro = fechaRegistro,
                FechaRecepcion = esPendienteCabecera ? null : fechaRegistro,
                Detalles = new List<DetalleCompra>(lineas.Count)
            };

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

            _context.Compras.Add(cabecera);
            await _context.SaveChangesAsync();

            if (!esPendienteCabecera)
            {
                await AplicarEfectosSecundariosAsync(cabecera, lineas);
            }

            await transaccion.CommitAsync();

            ResultadoEnvioCorreo? infoCorreo = null;
            if (esPendienteCabecera)
            {
                try
                {
                    var proveedor = await _context.Proveedores
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.IdProveedor == cabecera.IdProveedor);

                    var lineasCorreo = lineas.Select(l => new LineaDetalleSolicitudCorreoDto
                    {
                        CodigoProducto = l.CodigoProducto,
                        NombreProducto = l.NombreProducto,
                        Cantidad = l.Cantidad,
                        PrecioEstimado = l.PrecioCompra > 0 ? l.PrecioCompra : null
                    }).ToList();

                    infoCorreo = await _servicioCorreo.EnviarSolicitudReabastecimientoAsync(
                        proveedor?.Correo,
                        proveedor?.RazonSocial ?? "Proveedor",
                        cabecera.IdCompra,
                        lineasCorreo,
                        cabecera.Observaciones);
                }
                catch
                {
                }
            }

            string mensajeExito = esPendienteCabecera
                ? (infoCorreo != null
                    ? $"Solicitud N° {cabecera.IdCompra:D6} registrada exitosamente. {infoCorreo.Mensaje}"
                    : $"Solicitud de reabastecimiento N° {cabecera.IdCompra:D6} registrada en estado Pendiente.")
                : string.Format(CultureInfo.InvariantCulture, MensajeRegistroExitoso, cabecera.IdCompra);

            return new RespuestaRegistroCompra
            {
                Resultado = ResultadoRegistroCompra.Exito,
                Mensaje = mensajeExito,
                IdCompra = cabecera.IdCompra,
                FechaRegistro = fechaRegistro,
                MontoTotal = montoTotal,
                EstadoSolicitud = cabecera.EstadoSolicitud,
                InfoCorreo = infoCorreo
            };
        }
        catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
        {
            await RevertirSiEsPosibleAsync(transaccion);
            DesadjuntarSiEsPosible(cabecera);

            return RechazoRegistro(ResultadoRegistroCompra.ErrorPersistencia, MensajeErrorPersistencia);
        }
        finally
        {
            if (transaccion is not null)
            {
                await transaccion.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Confirma los precios acordados con el proveedor para una solicitud en estado Pendiente.
    /// </summary>
    public async Task<RespuestaAccionCompra> ConfirmarSolicitudCompraAsync(
        SolicitudConfirmarCompraDto solicitud, int idUsuario, string? rol)
    {
        ArgumentNullException.ThrowIfNull(solicitud);

        if (!PoliticaAcceso.PuedeOperarCompras(rol))
        {
            return new RespuestaAccionCompra
            {
                Exitoso = false,
                Mensaje = MensajePermisoRegistrarCompra
            };
        }

        string? mensajeUsuario = await ValidarUsuarioAsync(idUsuario);
        if (mensajeUsuario is not null)
        {
            return new RespuestaAccionCompra
            {
                Exitoso = false,
                Mensaje = mensajeUsuario
            };
        }

        if (solicitud.Lineas == null || solicitud.Lineas.Count == 0)
        {
            return new RespuestaAccionCompra
            {
                Exitoso = false,
                Mensaje = "Debe especificar los costos confirmados para cada línea de la solicitud."
            };
        }

        foreach (var linea in solicitud.Lineas)
        {
            if (linea.PrecioCompra <= 0)
            {
                return new RespuestaAccionCompra
                {
                    Exitoso = false,
                    Mensaje = "El precio de compra acordado debe ser mayor a cero para todas las líneas."
                };
            }
        }

        IDbContextTransaction? transaccion = null;
        try
        {
            transaccion = await _context.Database.BeginTransactionAsync();

            var compra = await _context.Compras
                .Include(c => c.Detalles)
                .FirstOrDefaultAsync(c => c.IdCompra == solicitud.IdCompra);

            if (compra is null)
            {
                return new RespuestaAccionCompra
                {
                    Exitoso = false,
                    Mensaje = MensajeCompraNoEncontrada
                };
            }

            if (!compra.Estado || !string.Equals(compra.EstadoSolicitud, "Pendiente", StringComparison.OrdinalIgnoreCase))
            {
                return new RespuestaAccionCompra
                {
                    Exitoso = false,
                    Mensaje = $"Solo se pueden confirmar solicitudes en estado Pendiente. El estado actual es '{compra.EstadoSolicitud}'."
                };
            }

            decimal nuevoMontoTotal = 0;
            var detalles = compra.Detalles ?? new List<DetalleCompra>();
            foreach (var detalle in detalles)
            {
                var confirmada = solicitud.Lineas.FirstOrDefault(l => l.IdProducto == detalle.IdProducto);
                if (confirmada != null)
                {
                    detalle.PrecioCompra = confirmada.PrecioCompra;
                    detalle.Subtotal = CalculoImportesCompra.CalcularSubtotal(detalle.Cantidad, confirmada.PrecioCompra);
                }
                nuevoMontoTotal += detalle.Subtotal;
            }

            compra.MontoTotal = nuevoMontoTotal;
            compra.EstadoSolicitud = "Confirmada";
            compra.FechaConfirmacion = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(solicitud.Observaciones))
            {
                compra.Observaciones = string.IsNullOrWhiteSpace(compra.Observaciones)
                    ? solicitud.Observaciones.Trim()
                    : $"{compra.Observaciones} | Confirmación: {solicitud.Observaciones.Trim()}";
            }

            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();

            return new RespuestaAccionCompra
            {
                Exitoso = true,
                IdCompra = compra.IdCompra,
                EstadoSolicitud = "Confirmada",
                Mensaje = $"Solicitud N° {compra.IdCompra:D6} confirmada exitosamente. Monto total recalculado: {compra.MontoTotal:C2}."
            };
        }
        catch (Exception ex) when (EsFalloDeOrigenDeDatos(ex))
        {
            await RevertirSiEsPosibleAsync(transaccion);
            return new RespuestaAccionCompra
            {
                Exitoso = false,
                Mensaje = MensajeErrorPersistencia
            };
        }
        finally
        {
            if (transaccion != null)
            {
                await transaccion.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Marca una compra (Pendiente o Confirmada) como Recibida e incrementa de forma atómica el stock de los productos.
    /// </summary>
    public async Task<RespuestaAccionCompra> RecibirCompraAsync(int idCompra, int idUsuario, string? rol)
    {
        if (!PoliticaAcceso.PuedeOperarCompras(rol))
        {
            return new RespuestaAccionCompra
            {
                Exitoso = false,
                Mensaje = MensajePermisoRegistrarCompra
            };
        }

        string? mensajeUsuario = await ValidarUsuarioAsync(idUsuario);
        if (mensajeUsuario is not null)
        {
            return new RespuestaAccionCompra
            {
                Exitoso = false,
                Mensaje = mensajeUsuario
            };
        }

        IDbContextTransaction? transaccion = null;
        try
        {
            transaccion = await _context.Database.BeginTransactionAsync();

            var compra = await _context.Compras
                .Include(c => c.Detalles)
                .FirstOrDefaultAsync(c => c.IdCompra == idCompra);

            if (compra is null)
            {
                return new RespuestaAccionCompra
                {
                    Exitoso = false,
                    Mensaje = MensajeCompraNoEncontrada
                };
            }

            if (!compra.Estado)
            {
                return new RespuestaAccionCompra
                {
                    Exitoso = false,
                    Mensaje = "La compra se encuentra inactiva o anulada y no puede recibirse."
                };
            }

            if (string.Equals(compra.EstadoSolicitud, "Recibida", StringComparison.OrdinalIgnoreCase))
            {
                return new RespuestaAccionCompra
                {
                    Exitoso = false,
                    Mensaje = "La compra ya fue recibida anteriormente."
                };
            }

            if (!string.Equals(compra.EstadoSolicitud, "Pendiente", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(compra.EstadoSolicitud, "Confirmada", StringComparison.OrdinalIgnoreCase))
            {
                return new RespuestaAccionCompra
                {
                    Exitoso = false,
                    Mensaje = $"Solo se pueden recibir compras en estado Pendiente o Confirmada. El estado actual es '{compra.EstadoSolicitud}'."
                };
            }

            var detalles = compra.Detalles ?? new List<DetalleCompra>();

            if (_context.Database.IsRelational())
            {
                foreach (var detalle in detalles)
                {
                    int filas = await _context.Productos
                        .Where(p => p.IdProducto == detalle.IdProducto
                            && p.Estado
                            && p.Stock <= int.MaxValue - detalle.Cantidad)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(p => p.Stock, p => p.Stock + detalle.Cantidad)
                            .SetProperty(p => p.PrecioCompra, p => detalle.PrecioCompra > 0 ? detalle.PrecioCompra : p.PrecioCompra)
                            .SetProperty(p => p.FechaModificacion, DateTime.Now)
                            .SetProperty(p => p.IdUsuarioModifica, idUsuario));

                    if (filas != 1)
                    {
                        throw new InvalidOperationException("No se pudo actualizar el stock del producto al recibir la compra.");
                    }
                }
            }
            else
            {
                foreach (var detalle in detalles)
                {
                    var producto = await _context.Productos.FindAsync(detalle.IdProducto);
                    if (producto == null || !producto.Estado || (long)producto.Stock + detalle.Cantidad > int.MaxValue)
                    {
                        throw new InvalidOperationException("No se pudo actualizar el stock del producto al recibir la compra.");
                    }

                    producto.Stock += detalle.Cantidad;
                    if (detalle.PrecioCompra > 0)
                    {
                        producto.PrecioCompra = detalle.PrecioCompra;
                    }
                    producto.FechaModificacion = DateTime.Now;
                    producto.IdUsuarioModifica = idUsuario;
                }
            }

            compra.EstadoSolicitud = "Recibida";
            compra.FechaRecepcion = DateTime.Now;

            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();

            return new RespuestaAccionCompra
            {
                Exitoso = true,
                IdCompra = compra.IdCompra,
                EstadoSolicitud = "Recibida",
                Mensaje = $"Mercadería de la compra N° {compra.IdCompra:D6} recibida exitosamente. Stock actualizado en catálogo."
            };
        }
        catch (Exception ex) when (EsFalloDeOrigenDeDatos(ex))
        {
            await RevertirSiEsPosibleAsync(transaccion);
            return new RespuestaAccionCompra
            {
                Exitoso = false,
                Mensaje = MensajeErrorPersistencia
            };
        }
        finally
        {
            if (transaccion != null)
            {
                await transaccion.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Anula una compra o solicitud de compra con reversión de stock si ya estaba Recibida. Exclusivo para Encargado.
    /// </summary>
    public async Task<RespuestaAccionCompra> AnularCompraAsync(SolicitudAnularCompraDto solicitud, int idUsuario, string? rol)
    {
        ArgumentNullException.ThrowIfNull(solicitud);

        if (!PoliticaAcceso.PuedeAnularOperaciones(rol))
        {
            return new RespuestaAccionCompra
            {
                Exitoso = false,
                Mensaje = "No tiene permisos para anular compras. Solo el Encargado puede anular operaciones comerciales."
            };
        }

        string? mensajeUsuario = await ValidarUsuarioAsync(idUsuario);
        if (mensajeUsuario is not null)
        {
            return new RespuestaAccionCompra
            {
                Exitoso = false,
                Mensaje = mensajeUsuario
            };
        }

        string motivo = solicitud.Motivo?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(motivo) || motivo.Length < 3 || motivo.Length > 300)
        {
            return new RespuestaAccionCompra
            {
                Exitoso = false,
                Mensaje = "El motivo de anulación es obligatorio y debe tener entre 3 y 300 caracteres."
            };
        }

        IDbContextTransaction? transaccion = null;
        try
        {
            transaccion = await _context.Database.BeginTransactionAsync();

            var compra = await _context.Compras
                .Include(c => c.Detalles)
                .FirstOrDefaultAsync(c => c.IdCompra == solicitud.IdCompra);

            if (compra is null)
            {
                return new RespuestaAccionCompra
                {
                    Exitoso = false,
                    Mensaje = MensajeCompraNoEncontrada
                };
            }

            if (!compra.Estado || string.Equals(compra.EstadoSolicitud, "Anulada", StringComparison.OrdinalIgnoreCase))
            {
                return new RespuestaAccionCompra
                {
                    Exitoso = false,
                    Mensaje = "La compra ya se encuentra anulada."
                };
            }

            bool eraRecibida = string.Equals(compra.EstadoSolicitud, "Recibida", StringComparison.OrdinalIgnoreCase);
            var detalles = compra.Detalles ?? new List<DetalleCompra>();

            if (eraRecibida)
            {
                var idsProductos = detalles.Select(d => d.IdProducto).ToList();
                var productos = await _context.Productos
                    .Where(p => idsProductos.Contains(p.IdProducto))
                    .ToListAsync();
                var productosMap = productos.ToDictionary(p => p.IdProducto);

                foreach (var detalle in detalles)
                {
                    if (!productosMap.TryGetValue(detalle.IdProducto, out var prod))
                    {
                        await RevertirSiEsPosibleAsync(transaccion);
                        return new RespuestaAccionCompra
                        {
                            Exitoso = false,
                            Mensaje = $"No se encontró el producto de la línea ({detalle.IdProducto}) para revertir existencias."
                        };
                    }

                    if (prod.Stock < detalle.Cantidad)
                    {
                        await RevertirSiEsPosibleAsync(transaccion);
                        return new RespuestaAccionCompra
                        {
                            Exitoso = false,
                            Mensaje = $"No se puede anular la compra porque el producto '{prod.Nombre}' tiene stock insuficiente ({prod.Stock}) para revertir las {detalle.Cantidad} unidades recibidas."
                        };
                    }
                }

                if (_context.Database.IsRelational())
                {
                    foreach (var detalle in detalles)
                    {
                        int filas = await _context.Productos
                            .Where(p => p.IdProducto == detalle.IdProducto && p.Stock >= detalle.Cantidad)
                            .ExecuteUpdateAsync(s => s
                                .SetProperty(p => p.Stock, p => p.Stock - detalle.Cantidad)
                                .SetProperty(p => p.FechaModificacion, DateTime.Now)
                                .SetProperty(p => p.IdUsuarioModifica, idUsuario));

                        if (filas != 1)
                        {
                            throw new InvalidOperationException("Fallo al revertir el stock de la compra recibida.");
                        }
                    }
                }
                else
                {
                    foreach (var detalle in detalles)
                    {
                        var prod = productosMap[detalle.IdProducto];
                        prod.Stock -= detalle.Cantidad;
                        prod.FechaModificacion = DateTime.Now;
                        prod.IdUsuarioModifica = idUsuario;
                    }
                }
            }

            compra.Estado = false;
            compra.EstadoSolicitud = "Anulada";
            compra.IdUsuarioAnula = idUsuario;
            compra.FechaAnulacion = DateTime.Now;
            compra.MotivoAnulacion = motivo;

            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();

            string detalleReversion = eraRecibida ? " Existencias revertidas en catálogo." : " No requirió ajuste de existencias.";
            return new RespuestaAccionCompra
            {
                Exitoso = true,
                IdCompra = compra.IdCompra,
                EstadoSolicitud = "Anulada",
                Mensaje = $"Compra N° {compra.IdCompra:D6} anulada exitosamente.{detalleReversion}"
            };
        }
        catch (Exception ex) when (EsFalloDeOrigenDeDatos(ex))
        {
            await RevertirSiEsPosibleAsync(transaccion);
            return new RespuestaAccionCompra
            {
                Exitoso = false,
                Mensaje = MensajeErrorPersistencia
            };
        }
        finally
        {
            if (transaccion != null)
            {
                await transaccion.DisposeAsync();
            }
        }
    }

    private async Task AplicarEfectosSecundariosAsync(
        Compra cabecera, IReadOnlyList<LineaCompraDto> lineas, CancellationToken ct = default)
    {
        if (!cabecera.Estado || lineas.Count == 0)
        {
            return;
        }

        var porProducto = new SortedDictionary<int, (long Cantidad, decimal PrecioCompra, decimal PrecioVenta)>();
        foreach (LineaCompraDto linea in lineas)
        {
            long cantidadPrevia = porProducto.TryGetValue(linea.IdProducto, out var previo) ? previo.Cantidad : 0L;
            porProducto[linea.IdProducto] = (cantidadPrevia + linea.Cantidad, linea.PrecioCompra, linea.PrecioVenta);
        }

        List<int> ids = porProducto.Keys.ToList();

        if (_context.Database.IsRelational())
        {
            try
            {
                foreach (KeyValuePair<int, (long Cantidad, decimal PrecioCompra, decimal PrecioVenta)> par in porProducto)
                {
                    if (par.Value.Cantidad < 1 || par.Value.Cantidad > int.MaxValue)
                    {
                        throw new InvalidOperationException(MensajeFalloEfectoProducto);
                    }

                    int idProducto = par.Key;
                    int cantidad = (int)par.Value.Cantidad;
                    decimal precioCompra = par.Value.PrecioCompra;
                    decimal precioVenta = par.Value.PrecioVenta;

                    int filas = await _context.Productos
                        .Where(p => p.IdProducto == idProducto
                            && p.Estado
                            && p.Stock <= int.MaxValue - cantidad)
                        .ExecuteUpdateAsync(
                            s => s
                                .SetProperty(p => p.Stock, p => p.Stock + cantidad)
                                .SetProperty(p => p.PrecioCompra, precioCompra)
                                .SetProperty(p => p.PrecioVenta, precioVenta),
                            ct);

                    if (filas != 1)
                    {
                        throw new InvalidOperationException(MensajeFalloEfectoProducto);
                    }
                }
            }
            finally
            {
                foreach (var entrada in _context.ChangeTracker.Entries<Producto>()
                             .Where(e => ids.Contains(e.Entity.IdProducto))
                             .ToList())
                {
                    entrada.State = EntityState.Detached;
                }
            }

            return;
        }

        List<Producto> productos = await _context.Productos
            .Where(p => ids.Contains(p.IdProducto))
            .ToListAsync(ct);

        if (productos.Count != porProducto.Count)
        {
            throw new InvalidOperationException(MensajeFalloEfectoProducto);
        }

        foreach (Producto producto in productos)
        {
            var efecto = porProducto[producto.IdProducto];
            long nuevoStock = (long)producto.Stock + efecto.Cantidad;
            if (!producto.Estado || efecto.Cantidad < 1 || nuevoStock > int.MaxValue)
            {
                throw new InvalidOperationException(MensajeFalloEfectoProducto);
            }

            producto.Stock = (int)nuevoStock;
            producto.PrecioCompra = efecto.PrecioCompra;
            producto.PrecioVenta = efecto.PrecioVenta;
        }

        await _context.SaveChangesAsync(ct);
    }

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
        }
    }

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
        }
    }

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
}
