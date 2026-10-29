using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using CapaNegocio.DTOs.Productos;
using CapaNegocio.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CapaNegocio.Tests;

/// <summary>
/// Pruebas de negocio de la confirmación del cambio de estado de productos
/// (spec 002, ronda 2026-10-27, T14): RF-21, RF-22, RF-23 y RF-24.
/// La acción aplicable, el título del diálogo, el mensaje de confirmación y la
/// posibilidad de ejecutar se componen en <c>ProductoNegocio</c> a partir del
/// estado <b>actual</b> del producto en la base, sin escribir nada.
/// Complementa la clase parcial con las pruebas del alta (T3/T4), de la
/// edición/listado (T5), del ajuste de stock (T7) y de la baja/reactivación (T6).
/// </summary>
public partial class ProductoNegocioTests
{
    // ---------- RF-21: producto activo ⇒ baja lógica ----------

    [Fact]
    public async Task ObtenerConfirmacionCambioEstado_ProductoActivo_ProponeBajaConTextosExactos()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario,
            estado: true);
        var negocio = new ProductoNegocio(db);

        ConfirmacionCambioEstadoProducto confirmacion =
            await negocio.ObtenerConfirmacionCambioEstadoAsync(producto.IdProducto);

        // RF-21/RF-22: la acción aplicable y los textos ya compuestos en español
        // salen de Negocio, no de la vista (mismos literales que antes redactaba
        // la grilla: sin regresión de texto, RNF-2).
        Assert.Equal(AccionCambioEstadoProducto.Baja, confirmacion.Accion);
        Assert.True(confirmacion.PuedeEjecutarse);
        Assert.Equal(producto.IdProducto, confirmacion.IdProducto);
        Assert.Equal("Teclado", confirmacion.NombreProducto);
        Assert.Equal("Dar de baja producto", confirmacion.TituloDialogo);
        Assert.Equal(
            "¿Confirma que desea dar de baja el producto Teclado?",
            confirmacion.Mensaje);
    }

    // ---------- RF-21: producto inactivo ⇒ reactivación ----------

    [Fact]
    public async Task ObtenerConfirmacionCambioEstado_ProductoInactivo_ProponeReactivacionConTextosExactos()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario,
            estado: false);
        var negocio = new ProductoNegocio(db);

        ConfirmacionCambioEstadoProducto confirmacion =
            await negocio.ObtenerConfirmacionCambioEstadoAsync(producto.IdProducto);

        // RF-21: el estado actual de la base decide la acción; los textos son
        // los mismos que mostraba la vista.
        Assert.Equal(AccionCambioEstadoProducto.Reactivacion, confirmacion.Accion);
        Assert.True(confirmacion.PuedeEjecutarse);
        Assert.Equal(producto.IdProducto, confirmacion.IdProducto);
        Assert.Equal("Teclado", confirmacion.NombreProducto);
        Assert.Equal("Reactivar producto", confirmacion.TituloDialogo);
        Assert.Equal(
            "¿Confirma que desea reactivar el producto Teclado?",
            confirmacion.Mensaje);
    }

    // ---------- RF-23: producto inexistente ----------

    [Fact]
    public async Task ObtenerConfirmacionCambioEstado_ProductoInexistente_RetornaNingunaSinEscribir()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        ConfirmacionCambioEstadoProducto confirmacion =
            await negocio.ObtenerConfirmacionCambioEstadoAsync(999);

        // RF-23: se informa el motivo con un aviso (sin diálogo) y no se escribe.
        Assert.Equal(AccionCambioEstadoProducto.Ninguna, confirmacion.Accion);
        Assert.False(confirmacion.PuedeEjecutarse);
        Assert.Null(confirmacion.IdProducto);
        Assert.Null(confirmacion.NombreProducto);
        Assert.Equal(string.Empty, confirmacion.TituloDialogo);
        Assert.Equal("El producto indicado no existe.", confirmacion.Mensaje);

        // La tabla queda intacta: una sola fila, la sembrada, y sigue activa.
        Producto persistido = await db.Productos.AsNoTracking().SingleAsync();
        Assert.True(persistido.Estado);
        Assert.Null(persistido.IdUsuarioModifica);
        Assert.Null(persistido.FechaModificacion);
        Assert.Null(persistido.MotivoAjusteStock);
    }

    // ---------- RF-23: identificador inválido ----------

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task ObtenerConfirmacionCambioEstado_IdProductoInvalido_RetornaNingunaSinEscribir(
        int idProducto)
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        ConfirmacionCambioEstadoProducto confirmacion =
            await negocio.ObtenerConfirmacionCambioEstadoAsync(idProducto);

        // RF-23: el identificador inválido no se consulta ni se escribe.
        Assert.Equal(AccionCambioEstadoProducto.Ninguna, confirmacion.Accion);
        Assert.False(confirmacion.PuedeEjecutarse);
        Assert.Null(confirmacion.IdProducto);
        Assert.Null(confirmacion.NombreProducto);
        Assert.Equal(string.Empty, confirmacion.TituloDialogo);
        Assert.Equal("Debe indicar un producto válido.", confirmacion.Mensaje);

        Producto persistido = await db.Productos.AsNoTracking().SingleAsync();
        Assert.True(persistido.Estado);
    }

    // ---------- RF-24: la confirmación es solo lectura ----------

    [Fact]
    public async Task ObtenerConfirmacionCambioEstado_NoEscribeNiModificaElProducto()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Usuario responsable = await SembrarUsuarioAsync(
            db, 90000002, "bruno.confirmacion@example.com", "Bruno");

        // Se siembran un producto activo y otro inactivo, ambos con datos de
        // auditoría ya informados para que la comparación no sea trivial.
        Producto activo = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario,
            estado: true);
        Producto inactivo = await SembrarProductoParaEdicionAsync(
            db, "7790001000028", "Mouse", bebidas.IdCategoria, creador.IdUsuario,
            estado: false);

        DateTime fechaAjusteActivo = new(2026, 10, 20, 9, 30, 0, DateTimeKind.Unspecified);
        activo.IdUsuarioModifica = responsable.IdUsuario;
        activo.FechaModificacion = fechaAjusteActivo;
        activo.MotivoAjusteStock = "Ajuste previo del activo";
        inactivo.IdUsuarioModifica = responsable.IdUsuario;
        inactivo.FechaModificacion = fechaAjusteActivo;
        inactivo.MotivoAjusteStock = "Ajuste previo del inactivo";
        await db.SaveChangesAsync();

        // Valores "antes": se releen sin seguimiento para comparar el estado real.
        Producto antesActivo = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == activo.IdProducto);
        Producto antesInactivo = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == inactivo.IdProducto);
        int filasAntes = await db.Productos.AsNoTracking().CountAsync();

        var negocio = new ProductoNegocio(db);

        // RF-24: pedir la confirmación varias veces (para ambos productos) no
        // escribe ni modifica la fila: cancelar deja todo como estaba.
        await negocio.ObtenerConfirmacionCambioEstadoAsync(activo.IdProducto);
        await negocio.ObtenerConfirmacionCambioEstadoAsync(activo.IdProducto);
        await negocio.ObtenerConfirmacionCambioEstadoAsync(inactivo.IdProducto);
        await negocio.ObtenerConfirmacionCambioEstadoAsync(inactivo.IdProducto);
        await negocio.ObtenerConfirmacionCambioEstadoAsync(999);
        await negocio.ObtenerConfirmacionCambioEstadoAsync(0);

        Producto despuesActivo = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == activo.IdProducto);
        Producto despuesInactivo = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == inactivo.IdProducto);

        Assert.Equal(antesActivo.Estado, despuesActivo.Estado);
        Assert.Equal(antesActivo.IdUsuarioModifica, despuesActivo.IdUsuarioModifica);
        Assert.Equal(antesActivo.FechaModificacion, despuesActivo.FechaModificacion);
        Assert.Equal(antesActivo.MotivoAjusteStock, despuesActivo.MotivoAjusteStock);

        Assert.Equal(antesInactivo.Estado, despuesInactivo.Estado);
        Assert.Equal(antesInactivo.IdUsuarioModifica, despuesInactivo.IdUsuarioModifica);
        Assert.Equal(antesInactivo.FechaModificacion, despuesInactivo.FechaModificacion);
        Assert.Equal(antesInactivo.MotivoAjusteStock, despuesInactivo.MotivoAjusteStock);

        // Sin alta, baja ni duplicado de filas (RNF-3).
        Assert.Equal(filasAntes, await db.Productos.AsNoTracking().CountAsync());
        Assert.Equal(2, filasAntes);
    }
}
