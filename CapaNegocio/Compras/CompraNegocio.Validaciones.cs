using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CapaDatos;
using CapaNegocio.Compras;
using CapaNegocio.Enums;
using Microsoft.EntityFrameworkCore;

namespace CapaNegocio;

public partial class CompraNegocio
{
    private const string MensajeCantidadSuperaMaximo = "La cantidad supera el máximo permitido.";
    private const string MensajeCantidadInvalida = "La cantidad debe ser un número entero mayor que cero escrito solo con dígitos.";
    private const string MensajeCantidadAcumuladaSuperaMaximo = "La cantidad acumulada supera el máximo permitido.";
    private const string MensajeFalloEfectoProducto = "No se pudo aplicar el efecto de la compra sobre el producto.";

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

    private const string MensajeSinProveedoresHistoricos = "El producto indicado no tiene proveedores con compras registradas.";
    private const string MensajeSinProductosHistoricos = "El proveedor indicado no tiene productos con compras registradas.";

    private const string TextoSinProveedor = "(Sin proveedor)";
    private const string TextoSinUsuario = "(Sin usuario)";

    private const string MensajeErrorPersistencia = "No se pudo completar la operación por un problema con el origen de datos. Intente nuevamente.";
    private const string MensajeErrorBuscarProveedores = "No se pudo buscar proveedores por un problema con el origen de datos. Intente nuevamente.";
    private const string MensajeErrorBuscarProductos = "No se pudo buscar productos por un problema con el origen de datos. Intente nuevamente.";
    private const string MensajeErrorPreciosProducto = "No se pudieron obtener los precios del producto por un problema con el origen de datos. Intente nuevamente.";

    private const string MensajeConfirmacionCancelarCompra = "¿Confirma que desea descartar el proveedor y todas las líneas de la compra en carga?";

    private static string? ValidarRolParaRegistro(string? rol)
    {
        return PoliticaAcceso.PuedeOperarCompras(rol) ? null : MensajePermisoRegistrarCompra;
    }

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
                Estado = p.Estado,
                Stock = p.Stock
            })
            .ToListAsync();

        return productos.ToDictionary(p => p.IdProducto);
    }

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

    private static string ConstruirMensajeProductoInexistente(int numeroLinea)
        => string.Format(CultureInfo.InvariantCulture, MensajeProductoInexistente, numeroLinea);

    private static string ConstruirMensajeProductoInactivo(ProductoDeCompra producto, int numeroLinea)
        => string.Format(
            CultureInfo.InvariantCulture, MensajeProductoInactivo, producto.Codigo, producto.Nombre, numeroLinea);

    private static string ConstruirMensajeProductoRepetido(ProductoDeCompra producto)
        => string.Format(CultureInfo.InvariantCulture, MensajeProductoRepetido, producto.Codigo, producto.Nombre);

    private static string ConstruirMensajeProductoDuplicadoEnEdicion(ProductoDeCompra producto)
        => string.Format(
            CultureInfo.InvariantCulture, MensajeProductoDuplicadoEnEdicion, producto.Codigo, producto.Nombre);

    private static bool EsFalloDeOrigenDeDatos(Exception excepcion)
    {
        return excepcion is DbUpdateException
            || excepcion is System.Data.Common.DbException
            || excepcion is InvalidOperationException;
    }

    private sealed class ProductoDeCompra
    {
        public int IdProducto { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public bool Estado { get; set; }

        public int Stock { get; set; }
    }
}
