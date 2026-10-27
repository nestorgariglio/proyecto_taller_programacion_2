using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using CapaNegocio.DTOs.Productos;
using CapaNegocio.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CapaNegocio.Tests;

/// <summary>
/// Pruebas de la consulta de stock e inventario crítico (spec 004, T3), sobre la
/// misma clase parcial que el resto de las pruebas de productos:
/// clasificación de criticidad, filtro "Solo críticos", prioridad del escaneo
/// exacto, orden habitual, conteo del catálogo y exclusión de escrituras.
/// Cubre RF-6, RF-8, RF-11, RF-12, RF-13, RF-15, RF-21, RNF-3 y RNF-8.
/// </summary>
public partial class ProductoNegocioTests
{
    // ---------- RF-8 / RF-12: clasificación de criticidad ----------

    [Theory]
    [InlineData(0, 5, NivelCriticidadStock.Agotado)]     // stock 0 ⇒ agotado
    [InlineData(-3, 5, NivelCriticidadStock.Agotado)]    // stock negativo ⇒ agotado (D-11)
    [InlineData(4, 5, NivelCriticidadStock.BajoUmbral)]  // mayor que 0 y menor que el umbral
    [InlineData(5, 5, NivelCriticidadStock.Normal)]      // igual al umbral ⇒ no es crítico
    [InlineData(10, 5, NivelCriticidadStock.Normal)]     // por encima del umbral
    public async Task ListarProductos_ConUmbral_ClasificaElNivelDeCriticidad(
        int stock, int umbralStock, NivelCriticidadStock nivelEsperado)
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario,
            stock: stock);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto(), umbralStock);

        ProductoRespuestaDto producto = Assert.Single(listado);
        Assert.Equal(nivelEsperado, producto.NivelCriticidad);
    }

    [Fact]
    public async Task ListarProductos_ConUmbralCero_SoloLosAgotadosSonCriticos()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Agotado", bebidas.IdCategoria, creador.IdUsuario,
            stock: 0);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000028", "Negativo", bebidas.IdCategoria, creador.IdUsuario,
            stock: -2);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000035", "ConStock", bebidas.IdCategoria, creador.IdUsuario,
            stock: 1);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto(), 0);

        // RF-12: con umbral 0 solo los agotados (0 o negativos) son críticos
        // y nunca aparece el nivel "BajoUmbral".
        Assert.Equal(3, listado.Count);
        Assert.All(listado, p => Assert.NotEqual(
            NivelCriticidadStock.BajoUmbral, p.NivelCriticidad));
        Assert.Equal(2, listado.Count(
            p => p.NivelCriticidad == NivelCriticidadStock.Agotado));
        Assert.Equal(
            NivelCriticidadStock.Normal,
            listado.Single(p => p.Stock == 1).NivelCriticidad);
    }

    [Fact]
    public async Task ListarProductos_SinUmbral_SoloMarcaAgotadosYNingunBajoUmbral()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Agotado", bebidas.IdCategoria, creador.IdUsuario,
            stock: 0);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000028", "Negativo", bebidas.IdCategoria, creador.IdUsuario,
            stock: -2);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000035", "Bajo", bebidas.IdCategoria, creador.IdUsuario,
            stock: 4);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000042", "Holgado", bebidas.IdCategoria, creador.IdUsuario,
            stock: 10);
        var negocio = new ProductoNegocio(db);

        // Sin umbral (modo degradado de RF-10) el listado sigue siendo el de
        // siempre y se obtiene con la llamada de un solo argumento.
        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto());

        Assert.Equal(4, listado.Count);
        Assert.DoesNotContain(listado, p => p.NivelCriticidad == NivelCriticidadStock.BajoUmbral);
        Assert.Equal(2, listado.Count(
            p => p.NivelCriticidad == NivelCriticidadStock.Agotado));
        Assert.All(
            listado.Where(p => p.Stock > 0),
            p => Assert.Equal(NivelCriticidadStock.Normal, p.NivelCriticidad));
    }

    // ---------- RF-6 / RF-11: filtro "Solo críticos" ----------

    [Fact]
    public async Task ListarProductos_SoloCriticos_DevuelveSoloAgotadosYBajoUmbral()
    {
        await using var db = await CrearContextoVacioAsync();
        CatalogoDeStock catalogo = await SembrarCatalogoDeStockAsync(db);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto { SoloCriticos = true }, 5);

        // Agotados (0 y negativo) más el producto bajo el umbral; el stock igual
        // al umbral y el que lo supera quedan fuera (RF-11/RF-12).
        Assert.Equal(3, listado.Count);
        Assert.Contains(listado, p => p.IdProducto == catalogo.Agotado.IdProducto
            && p.NivelCriticidad == NivelCriticidadStock.Agotado);
        Assert.Contains(listado, p => p.IdProducto == catalogo.Negativo.IdProducto
            && p.NivelCriticidad == NivelCriticidadStock.Agotado);
        Assert.Contains(listado, p => p.IdProducto == catalogo.Bajo.IdProducto
            && p.NivelCriticidad == NivelCriticidadStock.BajoUmbral);
        Assert.DoesNotContain(listado, p => p.IdProducto == catalogo.IgualAlUmbral.IdProducto);
        Assert.DoesNotContain(listado, p => p.IdProducto == catalogo.Holgado.IdProducto);
    }

    [Fact]
    public async Task ListarProductos_SoloCriticosSinUmbral_DevuelveSoloLosAgotados()
    {
        await using var db = await CrearContextoVacioAsync();
        CatalogoDeStock catalogo = await SembrarCatalogoDeStockAsync(db);
        var negocio = new ProductoNegocio(db);

        // Modo degradado (RF-10): sin umbral solo el stock agotado (0 o negativo)
        // es crítico, aunque el catálogo tenga productos con stock positivo 1-4.
        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto { SoloCriticos = true });

        Assert.Equal(2, listado.Count);
        Assert.Contains(listado, p => p.IdProducto == catalogo.Agotado.IdProducto);
        Assert.Contains(listado, p => p.IdProducto == catalogo.Negativo.IdProducto);
        Assert.DoesNotContain(listado, p => p.IdProducto == catalogo.Bajo.IdProducto);
        Assert.DoesNotContain(listado, p => p.Stock > 0);
        Assert.All(listado, p => Assert.Equal(NivelCriticidadStock.Agotado, p.NivelCriticidad));
        Assert.DoesNotContain(
            listado, p => p.NivelCriticidad == NivelCriticidadStock.BajoUmbral);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    public async Task ListarProductos_SoloCriticosApagado_NoRestringeElListado(
        bool? soloCriticos)
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarCatalogoDeStockAsync(db);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto { SoloCriticos = soloCriticos }, 5);

        // Igual que Estado: null y false no restringen.
        Assert.Equal(5, listado.Count);
    }

    [Fact]
    public async Task ListarProductos_SoloCriticosConTexto_AplicaAmbosCriterios()
    {
        await using var db = await CrearContextoVacioAsync();
        CatalogoDeStock catalogo = await SembrarCatalogoDeStockAsync(db);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto { Texto = "7790001", SoloCriticos = true },
            5);

        // El prefijo coincide con los cinco productos; "Solo críticos" acota a tres.
        Assert.Equal(3, listado.Count);
        Assert.All(listado, p => Assert.Contains("7790001", p.Codigo!));
        Assert.Contains(listado, p => p.IdProducto == catalogo.Agotado.IdProducto);
        Assert.Contains(listado, p => p.IdProducto == catalogo.Negativo.IdProducto);
        Assert.Contains(listado, p => p.IdProducto == catalogo.Bajo.IdProducto);
    }

    [Fact]
    public async Task ListarProductos_SoloCriticosConCategoria_DevuelveSoloLosCriticosDeEsaCategoria()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Categoria lacteos = await SembrarCategoriaAsync(db, "Lácteos", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto agotadoEnBebidas = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Agotado en bebidas", bebidas.IdCategoria,
            creador.IdUsuario, stock: 0);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000028", "Bajo en lácteos", lacteos.IdCategoria,
            creador.IdUsuario, stock: 3);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000035", "Holgado en bebidas", bebidas.IdCategoria,
            creador.IdUsuario, stock: 10);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto
            {
                IdCategoria = bebidas.IdCategoria,
                SoloCriticos = true
            },
            5);

        ProductoRespuestaDto producto = Assert.Single(listado);
        Assert.Equal(agotadoEnBebidas.IdProducto, producto.IdProducto);
        Assert.Equal("Bebidas", producto.NombreCategoria);
    }

    [Fact]
    public async Task ListarProductos_SoloCriticosConEstado_DevuelveSoloLosCriticosDelEstado()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto criticoActivo = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Crítico activo", bebidas.IdCategoria,
            creador.IdUsuario, stock: 0);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000028", "Crítico inactivo", bebidas.IdCategoria,
            creador.IdUsuario, estado: false, stock: 2);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000035", "Holgado activo", bebidas.IdCategoria,
            creador.IdUsuario, stock: 10);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto { Estado = true, SoloCriticos = true },
            5);

        ProductoRespuestaDto producto = Assert.Single(listado);
        Assert.Equal(criticoActivo.IdProducto, producto.IdProducto);
        Assert.True(producto.Estado);
    }

    [Fact]
    public async Task ListarProductos_SoloCriticosSinCoincidencias_DevuelveListaVacia()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarCatalogoDeStockAsync(db);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto { Texto = "inexistente-xyz", SoloCriticos = true },
            5);

        // Combinación válida sin filas: lista vacía, sin excepción (RF-13).
        Assert.Empty(listado);
    }

    // ---------- RF-21 / D-12: prioridad del código exacto ----------

    [Theory]
    [InlineData("12345678")]
    [InlineData("  12345678  ")] // el Recorte se aplica antes de comparar
    public async Task ListarProductos_CodigoExacto_QuedaPrimeroAunqueSuNombreLoUbiqueDespues(
        string texto)
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        // "12345678" es un código completo de 8 dígitos y, además, es parte del
        // código de 13 dígitos: la búsqueda devuelve los dos (RF-2).
        await SembrarProductoParaEdicionAsync(
            db, "12345678", "Zeta", bebidas.IdCategoria, creador.IdUsuario, stock: 10);
        await SembrarProductoParaEdicionAsync(
            db, "9912345678000", "Alfa", bebidas.IdCategoria, creador.IdUsuario, stock: 10);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto { Texto = texto }, 5);

        Assert.Equal(2, listado.Count);
        Assert.Equal("12345678", listado[0].Codigo);
        Assert.Equal("9912345678000", listado[1].Codigo);
    }

    [Fact]
    public async Task ListarProductos_TextoQueNoEsCodigoExacto_MantieneElOrdenPorNombre()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        await SembrarProductoParaEdicionAsync(
            db, "12345678", "Zeta", bebidas.IdCategoria, creador.IdUsuario, stock: 10);
        await SembrarProductoParaEdicionAsync(
            db, "9912345678000", "Alfa", bebidas.IdCategoria, creador.IdUsuario, stock: 10);
        var negocio = new ProductoNegocio(db);

        // "1234" coincide con los dos códigos pero no es idéntico a ninguno:
        // no hay priorización y se respeta el orden por Nombre.
        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto { Texto = "1234" }, 5);

        Assert.Equal(2, listado.Count);
        Assert.Equal("Alfa", listado[0].Nombre);
        Assert.Equal("Zeta", listado[1].Nombre);
    }

    [Fact]
    public async Task ListarProductos_CodigoInexistenteYCoincidenciaParcial_MantieneElOrdenHabitual()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        // El texto buscado es parte de dos códigos (coincidencia parcial) pero no
        // es idéntico a ninguno; con el tercer producto no coincide en absoluto.
        await SembrarProductoParaEdicionAsync(
            db, "1234567890", "Zeta", bebidas.IdCategoria, creador.IdUsuario, stock: 10);
        await SembrarProductoParaEdicionAsync(
            db, "1234567891", "Alfa", bebidas.IdCategoria, creador.IdUsuario, stock: 10);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000035", "Beta", bebidas.IdCategoria, creador.IdUsuario, stock: 10);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto { Texto = "123456789" }, 5);

        // Sin coincidencia exacta de Código no hay priorización: orden Nombre → Código.
        Assert.Equal(2, listado.Count);
        Assert.Equal("Alfa", listado[0].Nombre);
        Assert.Equal("1234567891", listado[0].Codigo);
        Assert.Equal("Zeta", listado[1].Nombre);
        Assert.Equal("1234567890", listado[1].Codigo);
    }

    [Fact]
    public async Task ListarProductos_DosCodigosExactos_NoReordenaYRespetaElOrdenHabitual()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        // Situación imposible con la unicidad vigente: se siembra a propósito
        // para cubrir la rama defensiva de RF-21 en el proveedor en memoria.
        await SembrarProductoParaEdicionAsync(
            db, "12345678", "Zeta", bebidas.IdCategoria, creador.IdUsuario, stock: 10);
        await SembrarProductoParaEdicionAsync(
            db, "12345678", "Zeta Dos", bebidas.IdCategoria, creador.IdUsuario, stock: 10);
        await SembrarProductoParaEdicionAsync(
            db, "9912345678000", "Alfa", bebidas.IdCategoria, creador.IdUsuario, stock: 10);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto { Texto = "12345678" }, 5);

        // Con más de una coincidencia exacta no se fuerza ninguna prioridad.
        Assert.Equal(3, listado.Count);
        Assert.Equal("Alfa", listado[0].Nombre);
        Assert.Equal("Zeta", listado[1].Nombre);
        Assert.Equal("Zeta Dos", listado[2].Nombre);
    }

    [Fact]
    public async Task ListarProductos_CodigoExactoExcluidoPorSoloCriticos_NoSePrioriza()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        await SembrarProductoParaEdicionAsync(
            db, "12345678", "Zeta", bebidas.IdCategoria, creador.IdUsuario, stock: 10);
        Producto agotado = await SembrarProductoParaEdicionAsync(
            db, "9912345678000", "Alfa", bebidas.IdCategoria, creador.IdUsuario, stock: 0);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto { Texto = "12345678", SoloCriticos = true },
            5);

        // El código exacto quedó fuera por el filtro: no hay nada que priorizar.
        ProductoRespuestaDto producto = Assert.Single(listado);
        Assert.Equal(agotado.IdProducto, producto.IdProducto);
        Assert.Equal(NivelCriticidadStock.Agotado, producto.NivelCriticidad);
    }

    // ---------- RNF-8: orden habitual del listado ----------

    [Fact]
    public async Task ListarProductos_SinPrioridad_OrdenaPorNombreYLuegoPorCodigo()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000028", "Yerba", bebidas.IdCategoria, creador.IdUsuario, stock: 10);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Yerba", bebidas.IdCategoria, creador.IdUsuario, stock: 10);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000035", "Aceite", bebidas.IdCategoria, creador.IdUsuario, stock: 10);
        var negocio = new ProductoNegocio(db);

        List<ProductoRespuestaDto> listado = await negocio.ListarProductosAsync(
            new FiltroProductosDto(), 5);

        Assert.Equal(
            new[] { "Aceite", "Yerba", "Yerba" },
            listado.Select(p => p.Nombre).ToArray());
        Assert.Equal("7790001000011", listado[1].Codigo);
        Assert.Equal("7790001000028", listado[2].Codigo);
    }

    // ---------- RF-13: conteo del catálogo ----------

    [Fact]
    public async Task ContarProductosAsync_CatalogoVacio_DevuelveCero()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ProductoNegocio(db);

        int total = await negocio.ContarProductosAsync();

        Assert.Equal(0, total);
    }

    [Fact]
    public async Task ContarProductosAsync_ConProductos_CuentaTodoElCatalogoSinFiltros()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Activo uno", bebidas.IdCategoria, creador.IdUsuario);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000028", "Activo dos", bebidas.IdCategoria, creador.IdUsuario);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000035", "Activo tres", bebidas.IdCategoria, creador.IdUsuario);
        await SembrarProductoParaEdicionAsync(
            db, "7790001000042", "Inactivo", bebidas.IdCategoria, creador.IdUsuario,
            estado: false);
        var negocio = new ProductoNegocio(db);

        int total = await negocio.ContarProductosAsync();

        // El conteo es sin filtros: incluye activos e inactivos (RF-13).
        Assert.Equal(4, total);
    }

    // ---------- RF-15 / RNF-3: la consulta no escribe ----------

    [Fact]
    public async Task ListarProductos_NoModificaStockEstadoNiPrecios()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarCatalogoDeStockAsync(db);
        var negocio = new ProductoNegocio(db);

        var antes = await db.Productos
            .AsNoTracking()
            .OrderBy(p => p.IdProducto)
            .Select(p => new { p.IdProducto, p.Stock, p.Estado, p.PrecioCompra, p.PrecioVenta })
            .ToListAsync();

        await negocio.ListarProductosAsync(new FiltroProductosDto(), 5);

        var despues = await db.Productos
            .AsNoTracking()
            .OrderBy(p => p.IdProducto)
            .Select(p => new { p.IdProducto, p.Stock, p.Estado, p.PrecioCompra, p.PrecioVenta })
            .ToListAsync();

        // RF-15 / RNF-3: la consulta es de solo lectura.
        Assert.Equal(antes, despues);
        Assert.DoesNotContain(
            db.ChangeTracker.Entries(),
            e => e.Entity is Producto && e.State != EntityState.Unchanged);
    }

    // ---------- Helpers de la consulta de stock ----------

    /// <summary>
    /// Catálogo de cinco productos con los cortes de criticidad de RF-12:
    /// agotado (0), negativo, bajo el umbral (4 con umbral 5), igual al umbral
    /// y por encima del umbral.
    /// </summary>
    private sealed class CatalogoDeStock
    {
        public Producto Agotado { get; init; } = null!;
        public Producto Negativo { get; init; } = null!;
        public Producto Bajo { get; init; } = null!;
        public Producto IgualAlUmbral { get; init; } = null!;
        public Producto Holgado { get; init; } = null!;
    }

    /// <summary>
    /// Siembra el catálogo de cinco productos con stock 0, -2, 4, 5 y 10 (umbral
    /// de referencia 5), todos activos y de la categoría "Bebidas".
    /// </summary>
    private static async Task<CatalogoDeStock> SembrarCatalogoDeStockAsync(AppDbContext db)
    {
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);

        Producto agotado = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Agotado", bebidas.IdCategoria, creador.IdUsuario, stock: 0);
        Producto negativo = await SembrarProductoParaEdicionAsync(
            db, "7790001000028", "Negativo", bebidas.IdCategoria, creador.IdUsuario, stock: -2);
        Producto bajo = await SembrarProductoParaEdicionAsync(
            db, "7790001000035", "Bajo", bebidas.IdCategoria, creador.IdUsuario, stock: 4);
        Producto igualAlUmbral = await SembrarProductoParaEdicionAsync(
            db, "7790001000042", "Igual al umbral", bebidas.IdCategoria, creador.IdUsuario,
            stock: 5);
        Producto holgado = await SembrarProductoParaEdicionAsync(
            db, "7790001000059", "Holgado", bebidas.IdCategoria, creador.IdUsuario, stock: 10);

        return new CatalogoDeStock
        {
            Agotado = agotado,
            Negativo = negativo,
            Bajo = bajo,
            IgualAlUmbral = igualAlUmbral,
            Holgado = holgado
        };
    }
}
