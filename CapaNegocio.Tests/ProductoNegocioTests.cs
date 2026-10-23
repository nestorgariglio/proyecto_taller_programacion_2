using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using CapaNegocio.DTOs.Productos;
using CapaNegocio.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CapaNegocio.Tests;

/// <summary>
/// Pruebas de negocio del alta de productos (spec 002, T3):
/// RF-1, RF-2, RF-4, RF-5, RF-7, RF-19 y RF-20.
/// La edición y el listado con filtros (T5) se prueban en
/// <c>ProductoNegocioModificacionListadoTests.cs</c>, sobre la misma clase parcial.
/// </summary>
public partial class ProductoNegocioTests
{
    // ---------- RF-1 / RF-2 / RF-4 / RF-5 / RF-19 / RF-20: alta válida ----------

    [Fact]
    public async Task CrearProducto_DatosValidos_PersisteActivoStockCeroYTextosRecortados()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario usuario = await SembrarUsuarioAsync(db);
        var negocio = new ProductoNegocio(db);

        RespuestaCreacionProducto respuesta = await negocio.CrearProductoAsync(
            CrearDtoValido(bebidas.IdCategoria, usuario.IdUsuario));

        // RF-1: el alta persiste con Estado = true y Stock = 0.
        Assert.Equal(ResultadoCreacionProducto.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Producto);
        Assert.True(respuesta.Producto.Estado);
        Assert.Equal(0, respuesta.Producto.Stock);

        // RF-2 / RF-4 / RF-5: los textos se guardan ya recortados.
        Assert.Equal("7790001000011", respuesta.Producto.Codigo);
        Assert.Equal("Teclado", respuesta.Producto.Nombre);
        Assert.Equal("Teclado mecánico", respuesta.Producto.Descripcion);

        // La categoría se referencia por Id y su nombre se resuelve por join (RF-17).
        Assert.Equal(bebidas.IdCategoria, respuesta.Producto.IdCategoria);
        Assert.Equal("Bebidas", respuesta.Producto.NombreCategoria);

        // RF-8: con PrecioVenta > PrecioCompra no hay advertencia.
        Assert.False(respuesta.AdvertenciaPrecioVentaMenorCompra);

        // RF-1 / RF-20: el registro queda persistido con el usuario creador.
        Producto guardado = await db.Productos.AsNoTracking().SingleAsync();
        Assert.Equal(bebidas.IdCategoria, guardado.IdCategoria);
        Assert.Equal("7790001000011", guardado.Codigo);
        Assert.Equal("Teclado", guardado.Nombre);
        Assert.Equal("Teclado mecánico", guardado.Descripcion);
        Assert.Equal(0, guardado.Stock);
        Assert.True(guardado.Estado);
        Assert.Equal(1000m, guardado.PrecioCompra);
        Assert.Equal(1500m, guardado.PrecioVenta);
        Assert.Equal(usuario.IdUsuario, guardado.IdUsuario);

        // RF-20: el usuario de modificación y la fecha de modificación quedan
        // nulos hasta la primera edición.
        Assert.Null(guardado.IdUsuarioModifica);
        Assert.Null(guardado.FechaModificacion);
        Assert.Null(guardado.MotivoAjusteStock);

        // RF-19: la fecha de creación queda informada (la impone el motor de BD;
        // en memoria solo se verifica el comportamiento).
        Assert.NotEqual(default, guardado.FechaRegistro);
    }

    [Fact]
    public async Task CrearProducto_CodigoConEspaciosEnLosBordes_SeGuardaRecortado()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario usuario = await SembrarUsuarioAsync(db);
        var negocio = new ProductoNegocio(db);

        CrearProductoDto entrada = CrearDtoValido(bebidas.IdCategoria, usuario.IdUsuario);
        entrada.Codigo = "\t 7790001000011 \n";

        RespuestaCreacionProducto respuesta = await negocio.CrearProductoAsync(entrada);

        Assert.Equal(ResultadoCreacionProducto.Exito, respuesta.Resultado);
        Assert.Equal("7790001000011", respuesta.Producto!.Codigo);

        Producto guardado = await db.Productos.AsNoTracking().SingleAsync();
        Assert.Equal("7790001000011", guardado.Codigo);
    }

    // ---------- RF-2: formato del Código (numérico de 8 a 13 dígitos) ----------

    [Theory]
    [InlineData("")]               // vacío
    [InlineData("   ")]            // solo espacios
    [InlineData("\t\n")]           // solo espacios en blanco
    [InlineData("1234567")]        // 7 dígitos
    [InlineData("12345678901234")] // 14 dígitos
    [InlineData("12345A7890")]     // con letras
    [InlineData("12345-7890")]     // con guion
    [InlineData("12345 7890")]     // con espacio interno
    public async Task CrearProducto_CodigoInvalido_RechazaConMensajeEnEspanol(string codigo)
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario usuario = await SembrarUsuarioAsync(db);
        var negocio = new ProductoNegocio(db);

        CrearProductoDto entrada = CrearDtoValido(bebidas.IdCategoria, usuario.IdUsuario);
        entrada.Codigo = codigo;

        RespuestaCreacionProducto respuesta = await negocio.CrearProductoAsync(entrada);

        Assert.Equal(ResultadoCreacionProducto.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("código", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await db.Productos.AsNoTracking().ToListAsync());
    }

    // ---------- RF-4: Nombre obligatorio de 3 a 100 caracteres ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    [InlineData("ab")]
    [InlineData("  ab  ")]
    public async Task CrearProducto_NombreInvalido_RechazaConMensajeEnEspanol(string nombre)
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario usuario = await SembrarUsuarioAsync(db);
        var negocio = new ProductoNegocio(db);

        CrearProductoDto entrada = CrearDtoValido(bebidas.IdCategoria, usuario.IdUsuario);
        entrada.Nombre = nombre;

        RespuestaCreacionProducto respuesta = await negocio.CrearProductoAsync(entrada);

        Assert.Equal(ResultadoCreacionProducto.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("nombre", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await db.Productos.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task CrearProducto_NombreDeMasDe100Caracteres_RechazaConMensajeEnEspanol()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario usuario = await SembrarUsuarioAsync(db);
        var negocio = new ProductoNegocio(db);

        CrearProductoDto entrada = CrearDtoValido(bebidas.IdCategoria, usuario.IdUsuario);
        entrada.Nombre = $"  {new string('x', 101)}  ";

        RespuestaCreacionProducto respuesta = await negocio.CrearProductoAsync(entrada);

        Assert.Equal(ResultadoCreacionProducto.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Equal("El nombre no puede superar los 100 caracteres.", respuesta.Mensaje);
        Assert.Empty(await db.Productos.AsNoTracking().ToListAsync());
    }

    // ---------- RF-5: Descripción opcional de hasta 300 caracteres ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public async Task CrearProducto_DescripcionVacia_EsValida(string descripcion)
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario usuario = await SembrarUsuarioAsync(db);
        var negocio = new ProductoNegocio(db);

        CrearProductoDto entrada = CrearDtoValido(bebidas.IdCategoria, usuario.IdUsuario);
        entrada.Descripcion = descripcion;

        RespuestaCreacionProducto respuesta = await negocio.CrearProductoAsync(entrada);

        Assert.Equal(ResultadoCreacionProducto.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Producto);
        // La descripción vacía se guarda como nula (campo opcional).
        Assert.Null(respuesta.Producto.Descripcion);
        Assert.Single(await db.Productos.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task CrearProducto_DescripcionDe301Caracteres_RechazaConMensajeEnEspanol()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario usuario = await SembrarUsuarioAsync(db);
        var negocio = new ProductoNegocio(db);

        CrearProductoDto entrada = CrearDtoValido(bebidas.IdCategoria, usuario.IdUsuario);
        entrada.Descripcion = $"  {new string('x', 301)}  ";

        RespuestaCreacionProducto respuesta = await negocio.CrearProductoAsync(entrada);

        Assert.Equal(ResultadoCreacionProducto.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Equal("La descripción no puede superar los 300 caracteres.", respuesta.Mensaje);
        Assert.Empty(await db.Productos.AsNoTracking().ToListAsync());
    }

    // ---------- RF-7: precios mayores o iguales a 0 ----------

    [Theory]
    [InlineData(-1.0, 100.0)]
    [InlineData(100.0, -1.0)]
    [InlineData(-0.01, 0.0)]
    public async Task CrearProducto_PreciosNegativos_RechazaConMensajeEnEspanol(
        double precioCompra, double precioVenta)
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario usuario = await SembrarUsuarioAsync(db);
        var negocio = new ProductoNegocio(db);

        CrearProductoDto entrada = CrearDtoValido(bebidas.IdCategoria, usuario.IdUsuario);
        entrada.PrecioCompra = (decimal)precioCompra;
        entrada.PrecioVenta = (decimal)precioVenta;

        RespuestaCreacionProducto respuesta = await negocio.CrearProductoAsync(entrada);

        Assert.Equal(ResultadoCreacionProducto.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("precio", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await db.Productos.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task CrearProducto_PreciosEnCero_EsValido()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario usuario = await SembrarUsuarioAsync(db);
        var negocio = new ProductoNegocio(db);

        CrearProductoDto entrada = CrearDtoValido(bebidas.IdCategoria, usuario.IdUsuario);
        entrada.PrecioCompra = 0m;
        entrada.PrecioVenta = 0m;

        RespuestaCreacionProducto respuesta = await negocio.CrearProductoAsync(entrada);

        // RF-7 / RNF-4: cero es un límite inferior válido.
        Assert.Equal(ResultadoCreacionProducto.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Producto);

        Producto guardado = await db.Productos.AsNoTracking().SingleAsync();
        Assert.Equal(0m, guardado.PrecioCompra);
        Assert.Equal(0m, guardado.PrecioVenta);
    }

    // ---------- RF-8 (no bloqueante): PrecioVenta < PrecioCompra ----------

    [Theory]
    [InlineData(1500.0, 1499.99, true)]
    [InlineData(1500.0, 1500.0, false)]
    [InlineData(1000.0, 1500.0, false)]
    public async Task CrearProducto_PrecioVentaMenorQueCompra_NoBloqueaYExponeLaAdvertencia(
        double precioCompra, double precioVenta, bool advertenciaEsperada)
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario usuario = await SembrarUsuarioAsync(db);
        var negocio = new ProductoNegocio(db);

        CrearProductoDto entrada = CrearDtoValido(bebidas.IdCategoria, usuario.IdUsuario);
        entrada.PrecioCompra = (decimal)precioCompra;
        entrada.PrecioVenta = (decimal)precioVenta;

        RespuestaCreacionProducto respuesta = await negocio.CrearProductoAsync(entrada);

        // RF-8 / D-19: la advertencia no bloquea el guardado; la UI decide
        // avisar ANTES de persistir. Negocio expone el dato.
        Assert.Equal(ResultadoCreacionProducto.Exito, respuesta.Resultado);
        Assert.Equal(advertenciaEsperada, respuesta.AdvertenciaPrecioVentaMenorCompra);
        Assert.Single(await db.Productos.AsNoTracking().ToListAsync());
    }

    // ---------- RF-20 / D-18: usuario creador obligatorio ----------

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task CrearProducto_SinUsuarioIdentificado_RechazaSinPersistir(int idUsuario)
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        await SembrarUsuarioAsync(db);
        var negocio = new ProductoNegocio(db);

        CrearProductoDto entrada = CrearDtoValido(bebidas.IdCategoria, idUsuario);

        RespuestaCreacionProducto respuesta = await negocio.CrearProductoAsync(entrada);

        Assert.Equal(ResultadoCreacionProducto.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("usuario", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await db.Productos.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task CrearProducto_UsuarioInexistente_RechazaSinPersistir()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        await SembrarUsuarioAsync(db);
        var negocio = new ProductoNegocio(db);

        CrearProductoDto entrada = CrearDtoValido(bebidas.IdCategoria, idUsuario: 999);

        RespuestaCreacionProducto respuesta = await negocio.CrearProductoAsync(entrada);

        Assert.Equal(ResultadoCreacionProducto.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("usuario", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await db.Productos.AsNoTracking().ToListAsync());
    }

    // ---------- RF-6 (adelantado de T4): categoría existente y activa ----------

    [Fact]
    public async Task CrearProducto_CategoriaInexistente_RetornaCategoriaInvalida()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario usuario = await SembrarUsuarioAsync(db);
        var negocio = new ProductoNegocio(db);

        CrearProductoDto entrada = CrearDtoValido(idCategoria: 999, usuario.IdUsuario);

        RespuestaCreacionProducto respuesta = await negocio.CrearProductoAsync(entrada);

        Assert.Equal(ResultadoCreacionProducto.CategoriaInvalida, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("categoría", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await db.Productos.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task CrearProducto_CategoriaInactiva_RetornaCategoriaInvalida()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria kiosco = await SembrarCategoriaAsync(db, "Kiosco", estado: false);
        Usuario usuario = await SembrarUsuarioAsync(db);
        var negocio = new ProductoNegocio(db);

        CrearProductoDto entrada = CrearDtoValido(kiosco.IdCategoria, usuario.IdUsuario);

        RespuestaCreacionProducto respuesta = await negocio.CrearProductoAsync(entrada);

        Assert.Equal(ResultadoCreacionProducto.CategoriaInvalida, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("inactiva", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await db.Productos.AsNoTracking().ToListAsync());
    }

    // ---------- RF-3: unicidad del Código en el alta (T4) ----------

    [Fact]
    public async Task CrearProducto_CodigoDuplicadoEnProductoActivo_RetornaCodigoDuplicado()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario usuario = await SembrarUsuarioAsync(db);
        Producto existente = await SembrarProductoAsync(
            db, "7790001000011", bebidas.IdCategoria, usuario.IdUsuario, estado: true);
        var negocio = new ProductoNegocio(db);

        // El Código llega con espacios en los bordes: tras el Recorte coincide
        // con el producto ya existente (RF-3).
        CrearProductoDto entrada = CrearDtoValido(bebidas.IdCategoria, usuario.IdUsuario);
        entrada.Codigo = "  7790001000011  ";
        entrada.Nombre = "Teclado duplicado";

        RespuestaCreacionProducto respuesta = await negocio.CrearProductoAsync(entrada);

        Assert.Equal(ResultadoCreacionProducto.CodigoDuplicado, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("código", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("existe", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        // No se persiste un segundo producto con el mismo Código.
        Assert.Single(await db.Productos.AsNoTracking().ToListAsync());

        // El producto existente no se modifica al rechazar el alta.
        Producto guardado = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == existente.IdProducto);
        Assert.Equal("Producto existente", guardado.Nombre);
        Assert.True(guardado.Estado);
        Assert.Equal(5, guardado.Stock);
    }

    [Fact]
    public async Task CrearProducto_CodigoDeProductoInactivo_RechazaConMensajeQueSugiereReactivar()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario usuario = await SembrarUsuarioAsync(db);
        Producto existente = await SembrarProductoAsync(
            db, "7790001000011", bebidas.IdCategoria, usuario.IdUsuario, estado: false);
        var negocio = new ProductoNegocio(db);

        CrearProductoDto entrada = CrearDtoValido(bebidas.IdCategoria, usuario.IdUsuario);
        entrada.Nombre = "Teclado reactivado por mi cuenta";

        RespuestaCreacionProducto respuesta = await negocio.CrearProductoAsync(entrada);

        // RF-3/D-1: se rechaza el alta y el mensaje avisa que el producto
        // existente está inactivo y que puede reactivarse desde el listado.
        Assert.Equal(ResultadoCreacionProducto.CodigoDuplicado, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("inactivo", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("reactivar", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("listado", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        // D-1: el sistema NO reactiva el producto existente por su cuenta y
        // tampoco lo modifica.
        Assert.Single(await db.Productos.AsNoTracking().ToListAsync());
        Producto guardado = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == existente.IdProducto);
        Assert.False(guardado.Estado);
        Assert.Equal("Producto existente", guardado.Nombre);
        Assert.Equal(5, guardado.Stock);
        Assert.Null(guardado.IdUsuarioModifica);
        Assert.Null(guardado.FechaModificacion);
    }

    [Fact]
    public async Task CrearProducto_CodigoNuevo_EsExitosoAunqueExistanOtrosProductos()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario usuario = await SembrarUsuarioAsync(db);
        await SembrarProductoAsync(
            db, "7790001000011", bebidas.IdCategoria, usuario.IdUsuario, estado: true);
        var negocio = new ProductoNegocio(db);

        CrearProductoDto entrada = CrearDtoValido(bebidas.IdCategoria, usuario.IdUsuario);
        entrada.Codigo = "7790001000028";

        RespuestaCreacionProducto respuesta = await negocio.CrearProductoAsync(entrada);

        Assert.Equal(ResultadoCreacionProducto.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Producto);
        Assert.Equal("7790001000028", respuesta.Producto.Codigo);
        Assert.Equal(2, await db.Productos.AsNoTracking().CountAsync());
    }

    // ---------- Helpers ----------

    private static CrearProductoDto CrearDtoValido(int idCategoria, int idUsuario)
    {
        return new CrearProductoDto
        {
            Codigo = " 7790001000011 ",
            Nombre = "  Teclado  ",
            Descripcion = "  Teclado mecánico  ",
            IdCategoria = idCategoria,
            PrecioCompra = 1000m,
            PrecioVenta = 1500m,
            IdUsuario = idUsuario
        };
    }

    private static async Task<Categoria> SembrarCategoriaAsync(
        AppDbContext db, string descripcion, bool estado)
    {
        var categoria = new Categoria { Descripcion = descripcion, Estado = estado };
        db.Categorias.Add(categoria);
        await db.SaveChangesAsync();
        return categoria;
    }

    private static async Task<Usuario> SembrarUsuarioAsync(AppDbContext db)
    {
        Rol rolAdministrador = await ObtenerOCrearRolAsync(db, "Administrador");
        var usuario = new Usuario
        {
            IdRol = rolAdministrador.IdRol,
            Dni = 90000001,
            Nombre = "Ana",
            Apellido = "Pérez",
            Correo = "ana.perez@example.com",
            Sexo = "F",
            Clave = "hash-de-prueba",
            Estado = true
        };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return usuario;
    }

    /// <summary>
    /// Obtiene el rol indicado o lo crea si aún no existe. Los usuarios de las
    /// pruebas se siembran con rol para poder validar el permiso de escritura
    /// del ABM en CapaNegocio (RF-11/RF-18/D-18-bis).
    /// </summary>
    private static async Task<Rol> ObtenerOCrearRolAsync(AppDbContext db, string descripcion)
    {
        Rol? rol = await db.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Descripcion == descripcion);

        if (rol is not null)
        {
            return rol;
        }

        var nuevoRol = new Rol { Descripcion = descripcion };
        db.Roles.Add(nuevoRol);
        await db.SaveChangesAsync();
        return nuevoRol;
    }

    private static async Task<Producto> SembrarProductoAsync(
        AppDbContext db, string codigo, int idCategoria, int idUsuario, bool estado)
    {
        var producto = new Producto
        {
            IdCategoria = idCategoria,
            Codigo = codigo,
            Nombre = "Producto existente",
            Descripcion = "Dato previo de la prueba",
            PrecioCompra = 1000m,
            PrecioVenta = 1500m,
            Stock = 5,
            Estado = estado,
            IdUsuario = idUsuario
        };
        db.Productos.Add(producto);
        await db.SaveChangesAsync();
        return producto;
    }

    private static async Task<AppDbContext> CrearContextoVacioAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"ProductoNegocioTests-{Guid.NewGuid()}")
            .Options;
        var db = new AppDbContext(options);

        await db.Database.EnsureCreatedAsync();
        return db;
    }
}
