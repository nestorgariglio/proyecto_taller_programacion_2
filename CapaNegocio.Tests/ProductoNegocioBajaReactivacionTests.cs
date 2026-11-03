using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using CapaNegocio.DTOs.Productos;
using CapaNegocio.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CapaNegocio.Tests;

/// <summary>
/// Pruebas de negocio de la baja lógica y de la reactivación de productos
/// (spec 002, T6): RF-12 y RF-13, con las decisiones D-12, D-13 y D-21.
/// Complementa la clase parcial con las pruebas del alta (T3/T4) y de la
/// edición/listado (T5).
/// </summary>
public partial class ProductoNegocioTests
{
    // ---------- RF-12 / D-13: baja lógica válida ----------

    [Fact]
    public async Task DarBajaProducto_ProductoActivo_LoMarcaInactivoSinEliminarLaFila()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Usuario responsableBaja = await SembrarUsuarioAsync(
            db, 90000002, "bruno.baja@example.com", "Bruno");
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        Assert.Equal(1, await db.Productos.AsNoTracking().CountAsync());

        RespuestaBajaProducto respuesta = await negocio.DarBajaProductoAsync(
            producto.IdProducto, responsableBaja.IdUsuario);

        // RF-12: la baja es lógica y la respuesta lo informa en español.
        Assert.Equal(ResultadoBajaProducto.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Producto);
        Assert.False(respuesta.Producto.Estado);
        Assert.False(string.IsNullOrWhiteSpace(respuesta.Mensaje));

        // D-13/RNF-3: la fila NO se elimina (sin DELETE físico) ni se duplica;
        // se trabaja sobre la MISMA fila.
        Assert.Equal(1, await db.Productos.AsNoTracking().CountAsync());
        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.False(persistido.Estado);

        // D-21/RF-20: la auditoría refleja el último evento (quién dio de baja)
        // y conserva la creación (usuario creador, Código y Stock).
        Assert.Equal(responsableBaja.IdUsuario, persistido.IdUsuarioModifica);
        Assert.Equal(creador.IdUsuario, persistido.IdUsuario);
        Assert.Equal("7790001000011", persistido.Codigo);
        Assert.Equal("Teclado", persistido.Nombre);
        Assert.Equal(5, persistido.Stock);

        // D-23: la FechaModificacion la impone el trigger AFTER UPDATE del
        // servidor; EF Core no la escribe (límite de InMemory).
        Assert.Null(persistido.FechaModificacion);
    }

    [Fact]
    public async Task DarBajaProducto_ProductoYaInactivo_RetornaProductoYaInactivoSinCambios()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario,
            estado: false);
        var negocio = new ProductoNegocio(db);

        RespuestaBajaProducto respuesta = await negocio.DarBajaProductoAsync(
            producto.IdProducto, creador.IdUsuario);

        // Caso límite: la operación sobre un producto ya inactivo no produce cambios.
        Assert.Equal(ResultadoBajaProducto.ProductoYaInactivo, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("inactivo", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.False(persistido.Estado);
        // No hubo escritura: la auditoría de modificación sigue vacía.
        Assert.Null(persistido.IdUsuarioModifica);
        Assert.Equal(1, await db.Productos.AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task DarBajaProducto_ProductoInexistente_RetornaProductoNoEncontrado()
    {
        await using var db = await CrearContextoVacioAsync();
        Usuario usuario = await SembrarUsuarioAsync(db);
        var negocio = new ProductoNegocio(db);

        RespuestaBajaProducto respuesta = await negocio.DarBajaProductoAsync(
            999, usuario.IdUsuario);

        Assert.Equal(ResultadoBajaProducto.ProductoNoEncontrado, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("no existe", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await db.Productos.AsNoTracking().ToListAsync());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task DarBajaProducto_IdProductoInvalido_RetornaDatosInvalidos(int idProducto)
    {
        await using var db = await CrearContextoVacioAsync();
        Usuario usuario = await SembrarUsuarioAsync(db);
        var negocio = new ProductoNegocio(db);

        RespuestaBajaProducto respuesta = await negocio.DarBajaProductoAsync(
            idProducto, usuario.IdUsuario);

        Assert.Equal(ResultadoBajaProducto.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("producto", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
    }

    // ---------- RF-20: usuario obligatorio y existente en la baja ----------

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task DarBajaProducto_SinUsuarioIdentificado_RechazaSinPersistir(int idUsuario)
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaBajaProducto respuesta = await negocio.DarBajaProductoAsync(
            producto.IdProducto, idUsuario);

        Assert.Equal(ResultadoBajaProducto.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("usuario", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.True(persistido.Estado);
        Assert.Null(persistido.IdUsuarioModifica);
    }

    [Fact]
    public async Task DarBajaProducto_UsuarioInexistente_RechazaSinPersistir()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaBajaProducto respuesta = await negocio.DarBajaProductoAsync(
            producto.IdProducto, 999);

        Assert.Equal(ResultadoBajaProducto.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("usuario", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.True(persistido.Estado);
        Assert.Null(persistido.IdUsuarioModifica);
    }

    // ---------- RF-13 / D-12: reactivación válida ----------

    [Fact]
    public async Task ReactivarProducto_CategoriaActiva_ReactivaLaMismaFilaSinDuplicar()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Usuario responsableReactivacion = await SembrarUsuarioAsync(
            db, 90000002, "bruno.reactiva@example.com", "Bruno");
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario,
            estado: false);
        var negocio = new ProductoNegocio(db);

        RespuestaReactivacionProducto respuesta = await negocio.ReactivarProductoAsync(
            producto.IdProducto, responsableReactivacion.IdUsuario);

        // RF-13: se reactiva con éxito sobre la MISMA fila.
        Assert.Equal(ResultadoReactivacionProducto.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Producto);
        Assert.True(respuesta.Producto.Estado);
        Assert.Equal("Bebidas", respuesta.Producto.NombreCategoria);
        Assert.False(string.IsNullOrWhiteSpace(respuesta.Mensaje));

        Assert.Equal(1, await db.Productos.AsNoTracking().CountAsync());

        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.True(persistido.Estado);

        // D-21/RF-20: se registra quién reactivó y se conserva la creación.
        Assert.Equal(responsableReactivacion.IdUsuario, persistido.IdUsuarioModifica);
        Assert.Equal(creador.IdUsuario, persistido.IdUsuario);
        Assert.Equal("7790001000011", persistido.Codigo);

        // D-23: la fecha de modificación la impone el trigger del servidor.
        Assert.Null(persistido.FechaModificacion);
    }

    // ---------- RF-13 / D-12: bloqueo por categoría inactiva ----------

    [Fact]
    public async Task ReactivarProducto_CategoriaInactiva_RetornaCategoriaInactivaYElProductoSigueInactivo()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria kiosco = await SembrarCategoriaAsync(db, "Kiosco", estado: false);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", kiosco.IdCategoria, creador.IdUsuario,
            estado: false);
        var negocio = new ProductoNegocio(db);

        RespuestaReactivacionProducto respuesta = await negocio.ReactivarProductoAsync(
            producto.IdProducto, creador.IdUsuario);

        // D-12/RF-13: se bloquea y se avisa que primero debe reactivarse la categoría.
        Assert.Equal(ResultadoReactivacionProducto.CategoriaInactiva, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("categoría", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("inactiva", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("reactiv", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        // El producto NO se reactiva y no se registra la operación rechazada.
        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.False(persistido.Estado);
        Assert.Null(persistido.IdUsuarioModifica);
        Assert.Equal(1, await db.Productos.AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task ReactivarProducto_CategoriaInexistente_NoReactivaYDevuelveMensajeEnEspanol()
    {
        await using var db = await CrearContextoVacioAsync();
        Usuario creador = await SembrarUsuarioAsync(db);
        // Caso defensivo: la FK no se valida en el proveedor en memoria.
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", idCategoria: 999, idUsuario: creador.IdUsuario,
            estado: false);
        var negocio = new ProductoNegocio(db);

        RespuestaReactivacionProducto respuesta = await negocio.ReactivarProductoAsync(
            producto.IdProducto, creador.IdUsuario);

        Assert.Equal(ResultadoReactivacionProducto.CategoriaInactiva, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("categoría", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.False(persistido.Estado);
    }

    [Fact]
    public async Task ReactivarProducto_ProductoYaActivo_RetornaProductoYaActivoSinCambios()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaReactivacionProducto respuesta = await negocio.ReactivarProductoAsync(
            producto.IdProducto, creador.IdUsuario);

        // Caso límite: reactivar uno ya activo no produce cambios.
        Assert.Equal(ResultadoReactivacionProducto.ProductoYaActivo, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("activo", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.True(persistido.Estado);
        Assert.Null(persistido.IdUsuarioModifica);
        Assert.Equal(1, await db.Productos.AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task ReactivarProducto_ProductoInexistente_RetornaProductoNoEncontrado()
    {
        await using var db = await CrearContextoVacioAsync();
        Usuario usuario = await SembrarUsuarioAsync(db);
        var negocio = new ProductoNegocio(db);

        RespuestaReactivacionProducto respuesta = await negocio.ReactivarProductoAsync(
            999, usuario.IdUsuario);

        Assert.Equal(ResultadoReactivacionProducto.ProductoNoEncontrado, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("no existe", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await db.Productos.AsNoTracking().ToListAsync());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task ReactivarProducto_SinUsuarioIdentificado_RechazaSinPersistir(int idUsuario)
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario,
            estado: false);
        var negocio = new ProductoNegocio(db);

        RespuestaReactivacionProducto respuesta = await negocio.ReactivarProductoAsync(
            producto.IdProducto, idUsuario);

        Assert.Equal(ResultadoReactivacionProducto.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("usuario", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.False(persistido.Estado);
        Assert.Null(persistido.IdUsuarioModifica);
    }

    [Fact]
    public async Task ReactivarProducto_UsuarioInexistente_RechazaSinPersistir()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario,
            estado: false);
        var negocio = new ProductoNegocio(db);

        RespuestaReactivacionProducto respuesta = await negocio.ReactivarProductoAsync(
            producto.IdProducto, 999);

        Assert.Equal(ResultadoReactivacionProducto.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("usuario", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.False(persistido.Estado);
        Assert.Null(persistido.IdUsuarioModifica);
    }

    // ---------- RF-12 / RF-13 / D-21: ciclo completo sobre la misma fila ----------

    [Fact]
    public async Task BajaYReactivacion_MantienenUnaSolaFilaYLaAuditoriaDelUltimoEvento()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Usuario primerResponsable = await SembrarUsuarioAsync(
            db, 90000002, "bruno.ciclo@example.com", "Bruno");
        Usuario segundoResponsable = await SembrarUsuarioAsync(
            db, 90000003, "carla.ciclo@example.com", "Carla");
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaBajaProducto baja = await negocio.DarBajaProductoAsync(
            producto.IdProducto, primerResponsable.IdUsuario);
        RespuestaReactivacionProducto reactivacion = await negocio.ReactivarProductoAsync(
            producto.IdProducto, segundoResponsable.IdUsuario);

        Assert.Equal(ResultadoBajaProducto.Exito, baja.Resultado);
        Assert.Equal(ResultadoReactivacionProducto.Exito, reactivacion.Resultado);

        // Nunca se crea un registro nuevo por cada evento (sin duplicados).
        Assert.Equal(1, await db.Productos.AsNoTracking().CountAsync());

        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.True(persistido.Estado);
        // D-21: sin historial; la auditoría refleja el último evento.
        Assert.Equal(segundoResponsable.IdUsuario, persistido.IdUsuarioModifica);
        Assert.Equal(creador.IdUsuario, persistido.IdUsuario);
    }
}
