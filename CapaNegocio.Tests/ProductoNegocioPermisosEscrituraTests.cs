using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using CapaNegocio.DTOs.Productos;
using CapaNegocio.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CapaNegocio.Tests;

/// <summary>
/// Pruebas del permiso de escritura del ABM de productos validado en
/// <c>CapaNegocio</c> (spec 002, RF-11/RF-18 con D-18-bis): el alta, la
/// modificación, la baja lógica, la reactivación y el ajuste de stock quedan
/// denegados a cualquier rol que no sea Administrador o Encargado, aunque la
/// operación se invoque fuera de la interfaz. Cada operación denegada se
/// verifica sin cambios persistidos. Complementa la clase parcial con las
/// pruebas del alta (T3/T4), edición y listado (T5), baja y reactivación (T6)
/// y ajuste de stock (T7).
/// </summary>
public partial class ProductoNegocioTests
{
    // ---------- RF-11 / RF-18: el Vendedor no puede escribir el ABM ----------

    [Fact]
    public async Task CrearProducto_Vendedor_RetornaSinPermisoYNoPersiste()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Rol rolVendedor = await SembrarRolAsync(db, "Vendedor");
        Usuario vendedor = await SembrarUsuarioConRolAsync(
            db, 90000030, "valeria.alta@example.com", "Valeria", rolVendedor.IdRol);
        var negocio = new ProductoNegocio(db);

        RespuestaCreacionProducto respuesta = await negocio.CrearProductoAsync(
            new CrearProductoDto
            {
                Codigo = "7790001000011",
                Nombre = "Teclado",
                Descripcion = "Alta intentada por un Vendedor",
                IdCategoria = bebidas.IdCategoria,
                PrecioCompra = 1000m,
                PrecioVenta = 1500m,
                IdUsuario = vendedor.IdUsuario
            });

        // RF-11/RF-18/D-18-bis: el permiso se valida en CapaNegocio, no en la UI.
        Assert.Equal(ResultadoCreacionProducto.SinPermiso, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        AssertSinPermisoEnEspanol(respuesta.Mensaje);

        // La operación rechazada no persiste nada.
        Assert.Empty(await db.Productos.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task ModificarProducto_Vendedor_RetornaSinPermisoYNoCambiaElProducto()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Rol rolVendedor = await SembrarRolAsync(db, "Vendedor");
        Usuario vendedor = await SembrarUsuarioConRolAsync(
            db, 90000031, "valeria.edicion@example.com", "Valeria", rolVendedor.IdRol);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaModificacionProducto respuesta = await negocio.ModificarProductoAsync(
            new ModificarProductoDto
            {
                IdProducto = producto.IdProducto,
                Nombre = "Teclado editado por un Vendedor",
                Descripcion = "Edición no autorizada",
                IdCategoria = bebidas.IdCategoria,
                PrecioCompra = 1m,
                PrecioVenta = 2m,
                IdUsuario = vendedor.IdUsuario
            });

        Assert.Equal(ResultadoModificacionProducto.SinPermiso, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        AssertSinPermisoEnEspanol(respuesta.Mensaje);

        // La operación rechazada no produce ninguna escritura.
        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.Equal("Teclado", persistido.Nombre);
        Assert.Equal("Teclado mecánico", persistido.Descripcion);
        Assert.Equal(1000m, persistido.PrecioCompra);
        Assert.Equal(1500m, persistido.PrecioVenta);
        Assert.Null(persistido.IdUsuarioModifica);
    }

    [Fact]
    public async Task DarBajaProducto_Vendedor_RetornaSinPermisoYElProductoSigueActivo()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Rol rolVendedor = await SembrarRolAsync(db, "Vendedor");
        Usuario vendedor = await SembrarUsuarioConRolAsync(
            db, 90000032, "valeria.baja@example.com", "Valeria", rolVendedor.IdRol);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaBajaProducto respuesta = await negocio.DarBajaProductoAsync(
            producto.IdProducto, vendedor.IdUsuario);

        Assert.Equal(ResultadoBajaProducto.SinPermiso, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        AssertSinPermisoEnEspanol(respuesta.Mensaje);

        // La baja rechazada no desactiva el producto ni registra la auditoría.
        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.True(persistido.Estado);
        Assert.Null(persistido.IdUsuarioModifica);
        Assert.Equal(1, await db.Productos.AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task ReactivarProducto_Vendedor_RetornaSinPermisoYElProductoSigueInactivo()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Rol rolVendedor = await SembrarRolAsync(db, "Vendedor");
        Usuario vendedor = await SembrarUsuarioConRolAsync(
            db, 90000033, "valeria.reactivacion@example.com", "Valeria", rolVendedor.IdRol);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario,
            estado: false);
        var negocio = new ProductoNegocio(db);

        RespuestaReactivacionProducto respuesta = await negocio.ReactivarProductoAsync(
            producto.IdProducto, vendedor.IdUsuario);

        Assert.Equal(ResultadoReactivacionProducto.SinPermiso, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        AssertSinPermisoEnEspanol(respuesta.Mensaje);

        // La reactivación rechazada deja el producto inactivo y sin auditoría.
        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.False(persistido.Estado);
        Assert.Null(persistido.IdUsuarioModifica);
        Assert.Equal(1, await db.Productos.AsNoTracking().CountAsync());
    }

    // ---------- Helpers ----------

    /// <summary>
    /// Verifica el mensaje de permiso denegado: en español y mencionando los
    /// roles autorizados (RNF-2/RF-11).
    /// </summary>
    private static void AssertSinPermisoEnEspanol(string? mensaje)
    {
        Assert.False(string.IsNullOrWhiteSpace(mensaje));
        Assert.Contains("permiso", mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Administrador", mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Encargado", mensaje, StringComparison.OrdinalIgnoreCase);
    }
}
