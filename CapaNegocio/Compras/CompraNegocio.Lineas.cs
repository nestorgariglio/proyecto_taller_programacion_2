using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CapaNegocio.Compras;
using CapaNegocio.DTOs.Compras;
using CapaNegocio.Enums;

namespace CapaNegocio;

public partial class CompraNegocio
{
    /// <summary>
    /// Agrega una nueva línea o acumula la cantidad si el producto ya existe en la carga activa.
    /// </summary>
    public async Task<RespuestaOperacionLineaCompra> AgregarLineaAsync(SolicitudAgregarLineaCompraDto solicitud)
    {
        ArgumentNullException.ThrowIfNull(solicitud);

        IReadOnlyList<LineaCompraDto> lineas = solicitud.LineasActuales ?? Array.Empty<LineaCompraDto>();
        int indiceNuevaLinea = lineas.Count;

        Dictionary<int, ProductoDeCompra> productos;
        try
        {
            productos = await ValidarProductosAsync(new[] { solicitud.IdProducto });
        }
        catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
        {
            return Rechazo(ResultadoOperacionLineaCompra.ErrorPersistencia, MensajeErrorPersistencia, lineas);
        }

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

        decimal precioCompra = 0m;
        if (solicitud.PrecioCompra is not null)
        {
            string? mensajePrecioCompra = ValidarPrecioDeTexto(
                solicitud.PrecioCompra,
                CampoPrecio.PrecioCompra,
                out precioCompra,
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
        }

        decimal precioVenta = 0m;
        if (solicitud.PrecioVenta is not null)
        {
            string? mensajePrecioVenta = ValidarPrecioDeTexto(
                solicitud.PrecioVenta,
                CampoPrecio.PrecioVenta,
                out precioVenta,
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
        }

        int indiceExistente = BuscarIndiceProducto(lineas, producto.IdProducto);

        if (indiceExistente >= 0)
        {
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
    /// Guarda la edición de una línea existente en la carga activa.
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
        Dictionary<int, ProductoDeCompra> productos;
        try
        {
            productos = await ValidarProductosAsync(new[] { solicitud.IdProducto });
        }
        catch (Exception excepcion) when (EsFalloDeOrigenDeDatos(excepcion))
        {
            return Rechazo(ResultadoOperacionLineaCompra.ErrorPersistencia, MensajeErrorPersistencia, lineas);
        }

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

        decimal precioCompra = 0m;
        if (solicitud.PrecioCompra is not null)
        {
            string? mensajePrecioCompra = ValidarPrecioDeTexto(
                solicitud.PrecioCompra,
                CampoPrecio.PrecioCompra,
                out precioCompra,
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
        }

        decimal precioVenta = 0m;
        if (solicitud.PrecioVenta is not null)
        {
            string? mensajePrecioVenta = ValidarPrecioDeTexto(
                solicitud.PrecioVenta,
                CampoPrecio.PrecioVenta,
                out precioVenta,
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
    /// Quita una línea del detalle de compra en carga según su índice.
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
    /// Calcula el subtotal en preparación para una línea si los datos ingresados son válidos.
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

    /// <summary>
    /// Mensaje de confirmación para cancelar y vaciar la compra en carga.
    /// </summary>
    public string ObtenerMensajeConfirmacionCancelarCompra() => MensajeConfirmacionCancelarCompra;

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

    private static bool ExisteProductoDuplicado(
        IReadOnlyList<LineaCompraDto> lineas, int idProducto, int? indiceExcluir = null)
    {
        int indice = BuscarIndiceProducto(lineas, idProducto);

        return indice >= 0 && indice != indiceExcluir;
    }

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

    private static List<LineaCompraDto> Reemplazar(
        IReadOnlyList<LineaCompraDto> lineas, int indice, LineaCompraDto linea)
    {
        var resultado = new List<LineaCompraDto>(lineas);
        resultado[indice] = linea;

        return resultado;
    }

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
}
