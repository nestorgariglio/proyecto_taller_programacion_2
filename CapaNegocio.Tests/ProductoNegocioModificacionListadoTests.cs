using System.Reflection;
using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using CapaNegocio.DTOs.Productos;
using CapaNegocio.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CapaNegocio.Tests;

/// <summary>
/// Pruebas de negocio de la edición de productos y del listado con filtros
/// (spec 002, T5): RF-9, RF-14, RF-15, RF-17 y RF-20.
/// </summary>
public partial class ProductoNegocioTests
{
    // ---------- RF-9 / RF-20: edición válida ----------

    [Fact]
    public async Task ModificarProducto_DatosValidos_ConservaCodigoFechaRegistroEstadoYStock()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Categoria lacteos = await SembrarCategoriaAsync(db, "Lácteos", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Usuario editor = await SembrarUsuarioAsync(db, 90000002, "bruno.editor@example.com", "Bruno");
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        DateTime fechaRegistroOriginal = producto.FechaRegistro;
        var negocio = new ProductoNegocio(db);

        RespuestaModificacionProducto respuesta = await negocio.ModificarProductoAsync(
            new ModificarProductoDto
            {
                IdProducto = producto.IdProducto,
                Nombre = "  Teclado mecánico  ",
                Descripcion = "  Teclado mecánico RGB  ",
                IdCategoria = lacteos.IdCategoria,
                PrecioCompra = 1200m,
                PrecioVenta = 1800m,
                IdUsuario = editor.IdUsuario
            });

        Assert.Equal(ResultadoModificacionProducto.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Producto);

        // RF-9/RF-20: se guardan los datos editados (con Recorte) y la auditoría
        // de la edición queda informada con el usuario que modificó.
        Assert.Equal("Teclado mecánico", respuesta.Producto.Nombre);
        Assert.Equal("Teclado mecánico RGB", respuesta.Producto.Descripcion);
        Assert.Equal(lacteos.IdCategoria, respuesta.Producto.IdCategoria);
        Assert.Equal("Lácteos", respuesta.Producto.NombreCategoria);
        Assert.Equal(1200m, respuesta.Producto.PrecioCompra);
        Assert.Equal(1800m, respuesta.Producto.PrecioVenta);
        Assert.Equal(editor.IdUsuario, respuesta.Producto.IdUsuarioModifica);

        Producto guardado = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);

        // D-11/RF-9: el Código es inmutable y la FechaRegistro se conserva.
        Assert.Equal("7790001000011", guardado.Codigo);
        Assert.Equal(fechaRegistroOriginal, guardado.FechaRegistro);

        // D-20: la edición no cambia el Estado.
        Assert.True(guardado.Estado);

        // El Stock tampoco se edita por esta operación (solo por el ajuste de stock).
        Assert.Equal(5, guardado.Stock);

        // RF-20: se conserva el usuario creador original y el motivo de ajuste
        // de stock no se toca desde la edición (D-21).
        Assert.Equal(creador.IdUsuario, guardado.IdUsuario);
        Assert.Null(guardado.MotivoAjusteStock);

        // D-23: la FechaModificacion la impone el trigger AFTER UPDATE del
        // servidor y EF Core no la escribe; el proveedor InMemory no ejecuta el
        // trigger, por eso no se verifica aquí (se valida contra la BD real en T13).

        // No se duplica el registro: se edita la MISMA fila.
        Assert.Equal(1, await db.Productos.AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task ModificarProducto_ConservandoSuPropioNombre_EsExitoso()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Usuario editor = await SembrarUsuarioAsync(db, 90000002, "bruno.editor@example.com", "Bruno");
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaModificacionProducto respuesta = await negocio.ModificarProductoAsync(
            new ModificarProductoDto
            {
                IdProducto = producto.IdProducto,
                Nombre = "Teclado",
                Descripcion = "Mismo nombre, otros precios",
                IdCategoria = bebidas.IdCategoria,
                PrecioCompra = 1100m,
                PrecioVenta = 1700m,
                IdUsuario = editor.IdUsuario
            });

        // RF-9: editar sin cambiar el nombre propio no se bloquea (autoexclusión;
        // el Código es inmutable y no se compara en la edición, D-11).
        Assert.Equal(ResultadoModificacionProducto.Exito, respuesta.Resultado);
        Assert.Equal(1100m, respuesta.Producto!.PrecioCompra);
        Assert.Equal(1700m, respuesta.Producto.PrecioVenta);
    }

    [Fact]
    public async Task ModificarProducto_DescripcionVacia_SeGuardaNula()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaModificacionProducto respuesta = await negocio.ModificarProductoAsync(
            new ModificarProductoDto
            {
                IdProducto = producto.IdProducto,
                Nombre = "Teclado",
                Descripcion = "   ",
                IdCategoria = bebidas.IdCategoria,
                PrecioCompra = 1000m,
                PrecioVenta = 1500m,
                IdUsuario = creador.IdUsuario
            });

        // RF-5: la descripción es opcional; vacía se guarda como nula.
        Assert.Equal(ResultadoModificacionProducto.Exito, respuesta.Resultado);
        Assert.Null(respuesta.Producto!.Descripcion);

        Producto guardado = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.Null(guardado.Descripcion);
    }

    [Fact]
    public async Task ModificarProducto_PrecioVentaMenorQueCompra_NoBloqueaYExponeLaAdvertencia()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaModificacionProducto respuesta = await negocio.ModificarProductoAsync(
            new ModificarProductoDto
            {
                IdProducto = producto.IdProducto,
                Nombre = "Teclado",
                Descripcion = "Teclado con precio de venta menor al de compra",
                IdCategoria = bebidas.IdCategoria,
                PrecioCompra = 1500m,
                PrecioVenta = 1400m,
                IdUsuario = creador.IdUsuario
            });

        // RF-8/D-19: la advertencia no bloquea el guardado; la UI decide cómo avisar.
        Assert.Equal(ResultadoModificacionProducto.Exito, respuesta.Resultado);
        Assert.True(respuesta.AdvertenciaPrecioVentaMenorCompra);
    }

    // ---------- D-20: el producto inactivo es editable sin reactivarlo ----------

    [Fact]
    public async Task ModificarProducto_Inactivo_EsExitosoYSigueInactivo()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Usuario editor = await SembrarUsuarioAsync(db, 90000002, "bruno.editor@example.com", "Bruno");
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario, estado: false);
        var negocio = new ProductoNegocio(db);

        RespuestaModificacionProducto respuesta = await negocio.ModificarProductoAsync(
            new ModificarProductoDto
            {
                IdProducto = producto.IdProducto,
                Nombre = "Teclado discontinuado",
                Descripcion = "Edición de un producto inactivo",
                IdCategoria = bebidas.IdCategoria,
                PrecioCompra = 900m,
                PrecioVenta = 1300m,
                IdUsuario = editor.IdUsuario
            });

        // D-20: se puede editar un producto inactivo, pero la edición NO lo reactiva.
        Assert.Equal(ResultadoModificacionProducto.Exito, respuesta.Resultado);
        Assert.False(respuesta.Producto!.Estado);

        Producto guardado = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.False(guardado.Estado);
        Assert.Equal("Teclado discontinuado", guardado.Nombre);
        Assert.Equal(editor.IdUsuario, guardado.IdUsuarioModifica);
    }

    // ---------- D-21: la auditoría refleja el último evento ----------

    [Fact]
    public async Task ModificarProducto_SegundaEdicion_SobrescribeElUsuarioDeModificacion()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Usuario primerEditor = await SembrarUsuarioAsync(
            db, 90000002, "bruno.editor@example.com", "Bruno");
        Usuario segundoEditor = await SembrarUsuarioAsync(
            db, 90000003, "carla.editora@example.com", "Carla");
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaModificacionProducto primeraEdicion = await negocio.ModificarProductoAsync(
            new ModificarProductoDto
            {
                IdProducto = producto.IdProducto,
                Nombre = "Teclado primera edición",
                IdCategoria = bebidas.IdCategoria,
                PrecioCompra = 1000m,
                PrecioVenta = 1500m,
                IdUsuario = primerEditor.IdUsuario
            });

        RespuestaModificacionProducto segundaEdicion = await negocio.ModificarProductoAsync(
            new ModificarProductoDto
            {
                IdProducto = producto.IdProducto,
                Nombre = "Teclado segunda edición",
                IdCategoria = bebidas.IdCategoria,
                PrecioCompra = 1000m,
                PrecioVenta = 1500m,
                IdUsuario = segundoEditor.IdUsuario
            });

        Assert.Equal(ResultadoModificacionProducto.Exito, primeraEdicion.Resultado);
        Assert.Equal(ResultadoModificacionProducto.Exito, segundaEdicion.Resultado);

        // D-21: sin historial; el usuario de modificación refleja el último evento.
        Producto guardado = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.Equal(segundoEditor.IdUsuario, guardado.IdUsuarioModifica);
        Assert.Equal("Teclado segunda edición", guardado.Nombre);

        // El usuario creador nunca se sobrescribe.
        Assert.Equal(creador.IdUsuario, guardado.IdUsuario);
    }

    // ---------- RF-6/RF-9: categoría existente y activa en la edición ----------

    [Fact]
    public async Task ModificarProducto_CategoriaInactiva_RetornaCategoriaInvalidaSinCambios()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Categoria kiosco = await SembrarCategoriaAsync(db, "Kiosco", estado: false);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaModificacionProducto respuesta = await negocio.ModificarProductoAsync(
            new ModificarProductoDto
            {
                IdProducto = producto.IdProducto,
                Nombre = "Teclado en categoría inactiva",
                IdCategoria = kiosco.IdCategoria,
                PrecioCompra = 1000m,
                PrecioVenta = 1500m,
                IdUsuario = creador.IdUsuario
            });

        Assert.Equal(ResultadoModificacionProducto.CategoriaInvalida, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("inactiva", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        Producto guardado = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.Equal("Teclado", guardado.Nombre);
        Assert.Equal(bebidas.IdCategoria, guardado.IdCategoria);
        Assert.Null(guardado.IdUsuarioModifica);
    }

    [Fact]
    public async Task ModificarProducto_CategoriaInexistente_RetornaCategoriaInvalida()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaModificacionProducto respuesta = await negocio.ModificarProductoAsync(
            new ModificarProductoDto
            {
                IdProducto = producto.IdProducto,
                Nombre = "Teclado",
                IdCategoria = 999,
                PrecioCompra = 1000m,
                PrecioVenta = 1500m,
                IdUsuario = creador.IdUsuario
            });

        Assert.Equal(ResultadoModificacionProducto.CategoriaInvalida, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("categoría", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        Producto guardado = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.Equal(bebidas.IdCategoria, guardado.IdCategoria);
    }

    // ---------- RF-9: validaciones de datos en la edición ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ab")]
    [InlineData("  ab  ")]
    public async Task ModificarProducto_NombreInvalido_RechazaConMensajeEnEspanol(string nombre)
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaModificacionProducto respuesta = await negocio.ModificarProductoAsync(
            new ModificarProductoDto
            {
                IdProducto = producto.IdProducto,
                Nombre = nombre,
                IdCategoria = bebidas.IdCategoria,
                PrecioCompra = 1000m,
                PrecioVenta = 1500m,
                IdUsuario = creador.IdUsuario
            });

        Assert.Equal(ResultadoModificacionProducto.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("nombre", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        Producto guardado = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.Equal("Teclado", guardado.Nombre);
        Assert.Null(guardado.IdUsuarioModifica);
    }

    [Fact]
    public async Task ModificarProducto_NombreDeMasDe100Caracteres_RechazaConMensajeEnEspanol()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaModificacionProducto respuesta = await negocio.ModificarProductoAsync(
            new ModificarProductoDto
            {
                IdProducto = producto.IdProducto,
                Nombre = $"  {new string('x', 101)}  ",
                IdCategoria = bebidas.IdCategoria,
                PrecioCompra = 1000m,
                PrecioVenta = 1500m,
                IdUsuario = creador.IdUsuario
            });

        Assert.Equal(ResultadoModificacionProducto.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Equal("El nombre no puede superar los 100 caracteres.", respuesta.Mensaje);
    }

    [Fact]
    public async Task ModificarProducto_DescripcionDe301Caracteres_RechazaConMensajeEnEspanol()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaModificacionProducto respuesta = await negocio.ModificarProductoAsync(
            new ModificarProductoDto
            {
                IdProducto = producto.IdProducto,
                Nombre = "Teclado",
                Descripcion = $"  {new string('x', 301)}  ",
                IdCategoria = bebidas.IdCategoria,
                PrecioCompra = 1000m,
                PrecioVenta = 1500m,
                IdUsuario = creador.IdUsuario
            });

        Assert.Equal(ResultadoModificacionProducto.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Equal("La descripción no puede superar los 300 caracteres.", respuesta.Mensaje);
    }

    [Theory]
    [InlineData(-1.0, 100.0)]
    [InlineData(100.0, -1.0)]
    [InlineData(-0.01, 0.0)]
    public async Task ModificarProducto_PreciosNegativos_RechazaConMensajeEnEspanol(
        double precioCompra, double precioVenta)
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaModificacionProducto respuesta = await negocio.ModificarProductoAsync(
            new ModificarProductoDto
            {
                IdProducto = producto.IdProducto,
                Nombre = "Teclado",
                IdCategoria = bebidas.IdCategoria,
                PrecioCompra = (decimal)precioCompra,
                PrecioVenta = (decimal)precioVenta,
                IdUsuario = creador.IdUsuario
            });

        Assert.Equal(ResultadoModificacionProducto.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("precio", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        Producto guardado = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.Equal(1000m, guardado.PrecioCompra);
        Assert.Equal(1500m, guardado.PrecioVenta);
    }

    [Fact]
    public async Task ModificarProducto_PreciosEnCero_EsValido()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaModificacionProducto respuesta = await negocio.ModificarProductoAsync(
            new ModificarProductoDto
            {
                IdProducto = producto.IdProducto,
                Nombre = "Teclado",
                IdCategoria = bebidas.IdCategoria,
                PrecioCompra = 0m,
                PrecioVenta = 0m,
                IdUsuario = creador.IdUsuario
            });

        Assert.Equal(ResultadoModificacionProducto.Exito, respuesta.Resultado);
        Assert.Equal(0m, respuesta.Producto!.PrecioCompra);
        Assert.Equal(0m, respuesta.Producto.PrecioVenta);
    }

    // ---------- RF-9: producto inexistente / identificador inválido ----------

    [Fact]
    public async Task ModificarProducto_ProductoInexistente_RetornaProductoNoEncontrado()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        var negocio = new ProductoNegocio(db);

        RespuestaModificacionProducto respuesta = await negocio.ModificarProductoAsync(
            new ModificarProductoDto
            {
                IdProducto = 999,
                Nombre = "Producto inexistente",
                IdCategoria = bebidas.IdCategoria,
                PrecioCompra = 1000m,
                PrecioVenta = 1500m,
                IdUsuario = creador.IdUsuario
            });

        Assert.Equal(ResultadoModificacionProducto.ProductoNoEncontrado, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("no existe", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await db.Productos.AsNoTracking().ToListAsync());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task ModificarProducto_IdProductoInvalido_RetornaDatosInvalidos(int idProducto)
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        var negocio = new ProductoNegocio(db);

        RespuestaModificacionProducto respuesta = await negocio.ModificarProductoAsync(
            new ModificarProductoDto
            {
                IdProducto = idProducto,
                Nombre = "Teclado",
                IdCategoria = bebidas.IdCategoria,
                PrecioCompra = 1000m,
                PrecioVenta = 1500m,
                IdUsuario = creador.IdUsuario
            });

        Assert.Equal(ResultadoModificacionProducto.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("producto", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
    }

    // ---------- RF-20: usuario de modificación obligatorio y existente ----------

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task ModificarProducto_SinUsuarioIdentificado_RechazaSinPersistir(int idUsuario)
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaModificacionProducto respuesta = await negocio.ModificarProductoAsync(
            new ModificarProductoDto
            {
                IdProducto = producto.IdProducto,
                Nombre = "Teclado sin usuario",
                IdCategoria = bebidas.IdCategoria,
                PrecioCompra = 1000m,
                PrecioVenta = 1500m,
                IdUsuario = idUsuario
            });

        Assert.Equal(ResultadoModificacionProducto.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("usuario", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        Producto guardado = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.Equal("Teclado", guardado.Nombre);
        Assert.Null(guardado.IdUsuarioModifica);
    }

    [Fact]
    public async Task ModificarProducto_UsuarioInexistente_RechazaSinPersistir()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaModificacionProducto respuesta = await negocio.ModificarProductoAsync(
            new ModificarProductoDto
            {
                IdProducto = producto.IdProducto,
                Nombre = "Teclado sin usuario",
                IdCategoria = bebidas.IdCategoria,
                PrecioCompra = 1000m,
                PrecioVenta = 1500m,
                IdUsuario = 999
            });

        Assert.Equal(ResultadoModificacionProducto.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("usuario", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        Producto guardado = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.Equal("Teclado", guardado.Nombre);
        Assert.Null(guardado.IdUsuarioModifica);
    }

    // ---------- RF-14 / RF-15 / RF-17: listado con filtros ----------

    [Fact]
    public async Task ListarProductos_SinFiltros_DevuelveActivosEInactivosConNombreDeCategoria()
    {
        await using var db = await CrearContextoVacioAsync();
        CatalogoDeListado catalogo = await SembrarCatalogoDeListadoAsync(db);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto());

        // RF-14: se listan TODOS los productos, activos e inactivos, con su estado.
        Assert.Equal(4, listado.Count);
        Assert.Equal(3, listado.Count(p => p.Estado));
        Assert.Single(listado, p => !p.Estado);

        // RF-17: la categoría se identifica por Id y el nombre se resuelve por join.
        ProductoRespuestaDto teclado = listado.Single(p => p.Codigo == "7790001000011");
        Assert.Equal("Teclado", teclado.Nombre);
        Assert.Equal("Teclado mecánico", teclado.Descripcion);
        Assert.Equal(catalogo.Bebidas.IdCategoria, teclado.IdCategoria);
        Assert.Equal("Bebidas", teclado.NombreCategoria);
        Assert.Equal(1000m, teclado.PrecioCompra);
        Assert.Equal(1500m, teclado.PrecioVenta);
        Assert.Equal(5, teclado.Stock);
        Assert.True(teclado.Estado);
        Assert.NotEqual(default, teclado.FechaRegistro);

        ProductoRespuestaDto yerba = listado.Single(p => p.Codigo == "7790001000028");
        Assert.Equal("Lácteos", yerba.NombreCategoria);
        Assert.False(yerba.Estado);
    }

    [Fact]
    public async Task ListarProductos_FiltroEstadoNulo_DevuelveTodasLasFilas()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarCatalogoDeListadoAsync(db);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto { Estado = null });

        Assert.Equal(4, listado.Count);
        Assert.Contains(listado, p => p.Estado);
        Assert.Contains(listado, p => !p.Estado);
    }

    [Theory]
    [InlineData(true, 3)]
    [InlineData(false, 1)]
    public async Task ListarProductos_FiltroPorEstado_DevuelveSoloLosDelEstado(
        bool estado, int cantidadEsperada)
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarCatalogoDeListadoAsync(db);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto { Estado = estado });

        Assert.Equal(cantidadEsperada, listado.Count);
        Assert.All(listado, p => Assert.Equal(estado, p.Estado));
    }

    [Theory]
    [InlineData("7790001000011", 1)] // Código exacto
    [InlineData("7790001", 4)]       // Prefijo común a todo el catálogo
    [InlineData("0001000028", 1)]    // Parte central del Código (Yerba)
    [InlineData("000", 4)]           // Parte central presente en todos
    public async Task ListarProductos_BusquedaPorCodigo_DevuelveLasCoincidencias(
        string texto, int cantidadEsperada)
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarCatalogoDeListadoAsync(db);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto { Texto = texto });

        Assert.Equal(cantidadEsperada, listado.Count);
        Assert.All(listado, p => Assert.Contains(texto, p.Codigo!));
    }

    [Theory]
    [InlineData("teclado")]    // minúsculas contra "Teclado"
    [InlineData("TECLADO")]    // mayúsculas contra "Teclado"
    [InlineData("TeClAdO")]    // caja mezclada
    [InlineData("  teclado  ")] // con espacios en los bordes
    public async Task ListarProductos_BusquedaPorNombre_EsInsensibleAMayusculas(string texto)
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarCatalogoDeListadoAsync(db);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto { Texto = texto });

        Assert.Single(listado);
        Assert.Equal("Teclado", listado[0].Nombre);
    }

    [Theory]
    [InlineData("er", 2)]   // "Yerba" y "Leche Entera"
    [InlineData("clad", 1)] // "Teclado"
    [InlineData("PTIC", 1)] // "Mouse Óptico" (mayúsculas contra un nombre acentuado)
    public async Task ListarProductos_BusquedaPorNombreParcial_DevuelveLasCoincidencias(
        string texto, int cantidadEsperada)
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarCatalogoDeListadoAsync(db);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto { Texto = texto });

        // La búsqueda por nombre es por contenido y no distingue mayúsculas.
        Assert.Equal(cantidadEsperada, listado.Count);
        Assert.All(listado, p => Assert.Contains(
            texto.ToLowerInvariant(), (p.Nombre ?? string.Empty).ToLowerInvariant()));
    }

    [Fact]
    public async Task ListarProductos_FiltroPorCategoria_DevuelveSoloLosDeLaCategoria()
    {
        await using var db = await CrearContextoVacioAsync();
        CatalogoDeListado catalogo = await SembrarCatalogoDeListadoAsync(db);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto { IdCategoria = catalogo.Lacteos.IdCategoria });

        Assert.Equal(2, listado.Count);
        Assert.All(listado, p => Assert.Equal(
            catalogo.Lacteos.IdCategoria, p.IdCategoria));
        Assert.All(listado, p => Assert.Equal("Lácteos", p.NombreCategoria));
    }

    [Fact]
    public async Task ListarProductos_CombinacionDeFiltros_AplicaLosTresCriterios()
    {
        await using var db = await CrearContextoVacioAsync();
        CatalogoDeListado catalogo = await SembrarCatalogoDeListadoAsync(db);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto
            {
                Texto = "7790001000035",
                IdCategoria = catalogo.Bebidas.IdCategoria,
                Estado = true
            });

        // Solo el producto que cumple los tres criterios a la vez.
        Assert.Single(listado);
        Assert.Equal("Mouse Óptico", listado[0].Nombre);
        Assert.Equal(catalogo.Bebidas.IdCategoria, listado[0].IdCategoria);
        Assert.True(listado[0].Estado);
    }

    [Fact]
    public async Task ListarProductos_SinCoincidencias_DevuelveListaVacia()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarCatalogoDeListadoAsync(db);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto { Texto = "inexistente-xyz" });

        Assert.Empty(listado);
    }

    [Fact]
    public async Task ListarProductos_PreciosConDecimales_SeDevuelvenConDosDecimales()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000099", "Gaseosa", bebidas.IdCategoria, creador.IdUsuario,
            precioCompra: 0.99m, precioVenta: 1234.56m);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto());

        Assert.Single(listado);
        Assert.Equal(0.99m, listado[0].PrecioCompra);
        Assert.Equal(1234.56m, listado[0].PrecioVenta);
    }

    [Fact]
    public void ProductoRespuestaDto_NoExponeDatosSensibles()
    {
        // RF-17: el DTO de lectura solo lleva los datos operativos del ABM.
        PropertyInfo[] propiedades = typeof(ProductoRespuestaDto).GetProperties();

        Assert.DoesNotContain(propiedades, p => p.Name.Contains(
            "Clave", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(propiedades, p => p.Name.Contains(
            "Contraseña", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(propiedades, p => p.Name.Contains(
            "Hash", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(propiedades, p => p.Name.Contains(
            "Password", StringComparison.OrdinalIgnoreCase));

        // La relación con la categoría viaja por Id y con el nombre resuelto.
        Assert.Contains(propiedades, p => p.Name == nameof(ProductoRespuestaDto.IdCategoria));
        Assert.Contains(propiedades, p => p.Name == nameof(ProductoRespuestaDto.NombreCategoria));
    }

    // ---------- Helpers ----------

    private sealed class CatalogoDeListado
    {
        public Categoria Bebidas { get; init; } = null!;
        public Categoria Lacteos { get; init; } = null!;
        public Producto Teclado { get; init; } = null!;
        public Producto Mouse { get; init; } = null!;
        public Producto Yerba { get; init; } = null!;
        public Producto Leche { get; init; } = null!;
    }

    /// <summary>
    /// Siembra un catálogo mínimo representativo del listado (RF-14/RF-15):
    /// 3 productos activos y 1 inactivo, repartidos en dos categorías activas.
    /// </summary>
    private static async Task<CatalogoDeListado> SembrarCatalogoDeListadoAsync(AppDbContext db)
    {
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Categoria lacteos = await SembrarCategoriaAsync(db, "Lácteos", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);

        Producto teclado = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);

        Producto mouse = await SembrarProductoParaEdicionAsync(
            db, "7790001000035", "Mouse Óptico", bebidas.IdCategoria, creador.IdUsuario);

        Producto yerba = await SembrarProductoParaEdicionAsync(
            db, "7790001000028", "Yerba", lacteos.IdCategoria, creador.IdUsuario,
            estado: false);

        Producto leche = await SembrarProductoParaEdicionAsync(
            db, "7790001000042", "Leche Entera", lacteos.IdCategoria, creador.IdUsuario);

        return new CatalogoDeListado
        {
            Bebidas = bebidas,
            Lacteos = lacteos,
            Teclado = teclado,
            Mouse = mouse,
            Yerba = yerba,
            Leche = leche
        };
    }

    /// <summary>
    /// Siembra un producto listo para editar con valores conocidos y distintos
    /// de los que envía la edición, de modo que se detecte cualquier cambio
    /// no deseado (Código, FechaRegistro, Estado, Stock, usuario creador).
    /// </summary>
    private static async Task<Producto> SembrarProductoParaEdicionAsync(
        AppDbContext db,
        string codigo,
        string nombre,
        int idCategoria,
        int idUsuario,
        bool estado = true,
        int stock = 5,
        decimal precioCompra = 1000m,
        decimal precioVenta = 1500m)
    {
        var producto = new Producto
        {
            IdCategoria = idCategoria,
            Codigo = codigo,
            Nombre = nombre,
            Descripcion = "Teclado mecánico",
            PrecioCompra = precioCompra,
            PrecioVenta = precioVenta,
            Stock = stock,
            Estado = estado,
            IdUsuario = idUsuario
        };
        db.Productos.Add(producto);
        await db.SaveChangesAsync();
        return producto;
    }

    private static async Task<Usuario> SembrarUsuarioAsync(
        AppDbContext db, int dni, string correo, string nombre)
    {
        Rol rolAdministrador = await ObtenerOCrearRolAsync(db, "Administrador");
        var usuario = new Usuario
        {
            IdRol = rolAdministrador.IdRol,
            Dni = dni,
            Nombre = nombre,
            Apellido = "Prueba",
            Correo = correo,
            Sexo = "F",
            Clave = "hash-de-prueba",
            Estado = true
        };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return usuario;
    }
}
