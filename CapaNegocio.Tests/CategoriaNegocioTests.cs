using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using CapaNegocio.DTOs.Categorias;
using CapaNegocio.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CapaNegocio.Tests;

public class CategoriaNegocioTests
{
    [Fact]
    public async Task CrearCategoria_DescripcionValida_PersisteActivaYRecortada()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new CategoriaNegocio(db);

        RespuestaCreacionCategoria respuesta = await negocio.CrearCategoriaAsync(
            new CrearCategoriaDto { Descripcion = "  Bebidas  " });

        Assert.Equal(ResultadoCreacionCategoria.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Categoria);
        Assert.True(respuesta.Categoria.Estado);
        Assert.Equal("Bebidas", respuesta.Categoria.Descripcion);

        // RF-1: el registro queda persistido en la base de datos.
        Categoria? guardada = await db.Categorias
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.IdCategoria == respuesta.Categoria.IdCategoria);
        Assert.NotNull(guardada);
        Assert.True(guardada.Estado);
        Assert.Equal("Bebidas", guardada.Descripcion);
    }

    [Fact]
    public async Task CrearCategoria_ConTildesYEnie_SeAceptaLaDescripcion()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new CategoriaNegocio(db);

        RespuestaCreacionCategoria respuesta = await negocio.CrearCategoriaAsync(
            new CrearCategoriaDto { Descripcion = "\t Ñandú Ácido \n" });

        Assert.Equal(ResultadoCreacionCategoria.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Categoria);
        Assert.Equal("Ñandú Ácido", respuesta.Categoria.Descripcion);
        Assert.True(respuesta.Categoria.Estado);
    }

    [Theory]
    [InlineData("", "La descripción de la categoría no puede estar vacía.")]
    [InlineData("   ", "La descripción de la categoría no puede estar vacía.")]
    [InlineData("\t\n  \t", "La descripción de la categoría no puede estar vacía.")]
    [InlineData(" \t ab \n", "La descripción debe tener al menos 3 caracteres.")]
    public async Task CrearCategoria_DescripcionInvalida_RechazaConMensajeEnEspanol(
        string descripcion, string mensajeEsperado)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new CategoriaNegocio(db);

        RespuestaCreacionCategoria respuesta = await negocio.CrearCategoriaAsync(
            new CrearCategoriaDto { Descripcion = descripcion });

        Assert.Equal(ResultadoCreacionCategoria.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Categoria);
        Assert.Equal(mensajeEsperado, respuesta.Mensaje);
        Assert.Empty(await db.Categorias.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task CrearCategoria_DescripcionDeMasDe100Caracteres_RechazaConMensajeEnEspanol()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new CategoriaNegocio(db);

        RespuestaCreacionCategoria respuesta = await negocio.CrearCategoriaAsync(
            new CrearCategoriaDto { Descripcion = $"  {new string('x', 101)}  " });

        Assert.Equal(ResultadoCreacionCategoria.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Categoria);
        Assert.Equal("La descripción no puede superar los 100 caracteres.", respuesta.Mensaje);
        Assert.Empty(await db.Categorias.AsNoTracking().ToListAsync());
    }

    // ---------- RF-3: unicidad case-insensitive en el alta ----------

    [Fact]
    public async Task CrearCategoria_DuplicadoDeActiva_RechazaDescripcionDuplicada()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        var negocio = new CategoriaNegocio(db);

        RespuestaCreacionCategoria respuesta = await negocio.CrearCategoriaAsync(
            new CrearCategoriaDto { Descripcion = "Bebidas" });

        Assert.Equal(ResultadoCreacionCategoria.DescripcionDuplicada, respuesta.Resultado);
        Assert.Null(respuesta.Categoria);
        Assert.False(string.IsNullOrWhiteSpace(respuesta.Mensaje));
        Assert.Single(await db.Categorias.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task CrearCategoria_DuplicadoDeActivaDistintaCaja_RechazaDescripcionDuplicada()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        var negocio = new CategoriaNegocio(db);

        RespuestaCreacionCategoria respuesta = await negocio.CrearCategoriaAsync(
            new CrearCategoriaDto { Descripcion = "bEbIdAs" });

        Assert.Equal(ResultadoCreacionCategoria.DescripcionDuplicada, respuesta.Resultado);
        Assert.Null(respuesta.Categoria);
        Assert.Single(await db.Categorias.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task CrearCategoria_DuplicadoSobreInactiva_MensajeEspecificoYNoLaReactiva()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria kiosco = await SembrarCategoriaAsync(db, "Kiosco", estado: false);
        var negocio = new CategoriaNegocio(db);

        RespuestaCreacionCategoria respuesta = await negocio.CrearCategoriaAsync(
            new CrearCategoriaDto { Descripcion = "KIOSCO" });

        // D-1: se rechaza con mensaje específico que indica que está inactiva
        // y sugiere reactivarla desde el listado.
        Assert.Equal(ResultadoCreacionCategoria.DescripcionDuplicada, respuesta.Resultado);
        Assert.Null(respuesta.Categoria);
        Assert.Contains("inactiva", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("reactivar", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("listado", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        // La categoría existente sigue inactiva (sin reactivación automática)
        // y no se creó ningún registro nuevo.
        Assert.Single(await db.Categorias.AsNoTracking().ToListAsync());
        Categoria? persistida = await db.Categorias
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.IdCategoria == kiosco.IdCategoria);
        Assert.NotNull(persistida);
        Assert.False(persistida.Estado);
    }

    // ---------- RF-4: modificación con autoexclusión ----------

    [Fact]
    public async Task ModificarCategoria_SinCambiarSuPropiaDescripcion_Exito()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        var negocio = new CategoriaNegocio(db);

        RespuestaModificacionCategoria respuesta = await negocio.ModificarCategoriaAsync(
            new ModificarCategoriaDto { IdCategoria = bebidas.IdCategoria, Descripcion = "Bebidas" });

        // Autoexclusión: la comparación nunca incluye a la categoría que se edita.
        Assert.Equal(ResultadoModificacionCategoria.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Categoria);
        Assert.Equal("Bebidas", respuesta.Categoria.Descripcion);
        Assert.Single(await db.Categorias.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task ModificarCategoria_DescripcionConEspaciosLaterales_AplicaElRecorte()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        var negocio = new CategoriaNegocio(db);

        RespuestaModificacionCategoria respuesta = await negocio.ModificarCategoriaAsync(
            new ModificarCategoriaDto { IdCategoria = bebidas.IdCategoria, Descripcion = "\t Bebidas con Gas \n" });

        Assert.Equal(ResultadoModificacionCategoria.Exito, respuesta.Resultado);
        Assert.Equal("Bebidas con Gas", respuesta.Categoria!.Descripcion);

        Categoria? persistida = await db.Categorias
            .AsNoTracking()
            .SingleAsync(c => c.IdCategoria == bebidas.IdCategoria);
        Assert.Equal("Bebidas con Gas", persistida.Descripcion);
    }

    [Fact]
    public async Task ModificarCategoria_DescripcionDemasiadoCorta_RechazaSinCambios()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebibas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        var negocio = new CategoriaNegocio(db);

        RespuestaModificacionCategoria respuesta = await negocio.ModificarCategoriaAsync(
            new ModificarCategoriaDto { IdCategoria = bebibas.IdCategoria, Descripcion = "  ab  " });

        Assert.Equal(ResultadoModificacionCategoria.DatosInvalidos, respuesta.Resultado);
        Assert.Equal("La descripción debe tener al menos 3 caracteres.", respuesta.Mensaje);

        Categoria? persistida = await db.Categorias
            .AsNoTracking()
            .SingleAsync(c => c.IdCategoria == bebibas.IdCategoria);
        Assert.Equal("Bebidas", persistida.Descripcion);
    }

    [Fact]
    public async Task ModificarCategoria_HaciaElNombreDeOtraActiva_RechazaDescripcionDuplicada()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Categoria lacteos = await SembrarCategoriaAsync(db, "Lácteos", estado: true);
        var negocio = new CategoriaNegocio(db);

        RespuestaModificacionCategoria respuesta = await negocio.ModificarCategoriaAsync(
            new ModificarCategoriaDto { IdCategoria = lacteos.IdCategoria, Descripcion = "BEBIDAS" });

        Assert.Equal(ResultadoModificacionCategoria.DescripcionDuplicada, respuesta.Resultado);
        Assert.Null(respuesta.Categoria);
        Assert.False(string.IsNullOrWhiteSpace(respuesta.Mensaje));

        // Ninguna de las dos categorías cambió.
        var persistidas = await db.Categorias.AsNoTracking().ToListAsync();
        Assert.Equal(2, persistidas.Count);
        Assert.Equal("Bebidas", persistidas.Single(c => c.IdCategoria == bebidas.IdCategoria).Descripcion);
        Assert.Equal("Lácteos", persistidas.Single(c => c.IdCategoria == lacteos.IdCategoria).Descripcion);
    }

    [Fact]
    public async Task ModificarCategoria_HaciaElNombreDeOtraInactiva_MensajeEspecificoYNoLaReactiva()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebibas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Categoria kiosco = await SembrarCategoriaAsync(db, "Kiosco", estado: false);
        var negocio = new CategoriaNegocio(db);

        RespuestaModificacionCategoria respuesta = await negocio.ModificarCategoriaAsync(
            new ModificarCategoriaDto { IdCategoria = bebibas.IdCategoria, Descripcion = "kiosco" });

        Assert.Equal(ResultadoModificacionCategoria.DescripcionDuplicada, respuesta.Resultado);
        Assert.Contains("inactiva", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("reactivar", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("listado", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        // La existente sigue inactiva y la editada conserva su nombre.
        Categoria? kioscoPersistido = await db.Categorias
            .AsNoTracking()
            .SingleAsync(c => c.IdCategoria == kiosco.IdCategoria);
        Categoria? bebibasPersistida = await db.Categorias
            .AsNoTracking()
            .SingleAsync(c => c.IdCategoria == bebibas.IdCategoria);
        Assert.False(kioscoPersistido.Estado);
        Assert.Equal("Bebidas", bebibasPersistida.Descripcion);
    }

    [Fact]
    public async Task ModificarCategoria_IdInexistente_RetornaCategoriaNoEncontrada()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new CategoriaNegocio(db);

        RespuestaModificacionCategoria respuesta = await negocio.ModificarCategoriaAsync(
            new ModificarCategoriaDto { IdCategoria = 999, Descripcion = "Cualquier Cosa" });

        Assert.Equal(ResultadoModificacionCategoria.CategoriaNoEncontrada, respuesta.Resultado);
        Assert.Empty(await db.Categorias.AsNoTracking().ToListAsync());
    }

    // ---------- RF-5: edición con productos vinculados ----------

    [Fact]
    public async Task ModificarCategoria_ConProductosVinculados_MantieneElVinculoYNuevaDescripcionEnListado()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        var producto = new Producto
        {
            IdCategoria = bebidas.IdCategoria,
            Nombre = "Agua Mineral 500ml",
            Stock = 10,
            PrecioVenta = 1500m
        };
        db.Productos.Add(producto);
        await db.SaveChangesAsync();
        var negocio = new CategoriaNegocio(db);

        RespuestaModificacionCategoria respuesta = await negocio.ModificarCategoriaAsync(
            new ModificarCategoriaDto { IdCategoria = bebidas.IdCategoria, Descripcion = "Bebidas Variadas" });

        Assert.Equal(ResultadoModificacionCategoria.Exito, respuesta.Resultado);

        // RF-5: el producto conserva su IdCategoria (sin DELETE ni reasignación)
        // y, navegando Producto.Categoria, ve la nueva descripción.
        Producto? productoPersistido = await db.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.Equal(1, await db.Productos.AsNoTracking().CountAsync());
        Assert.Equal(bebidas.IdCategoria, productoPersistido.IdCategoria);
        Assert.NotNull(productoPersistido.Categoria);
        Assert.Equal("Bebidas Variadas", productoPersistido.Categoria.Descripcion);

        // RF-5: el listado devuelve la nueva descripción.
        IReadOnlyList<CategoriaRespuestaDto> listado = await negocio.ListarCategoriasAsync();
        CategoriaRespuestaDto categoriaListada = listado
            .Single(c => c.IdCategoria == bebidas.IdCategoria);
        Assert.Equal("Bebidas Variadas", categoriaListada.Descripcion);
    }

    // ---------- RF-10: listado completo (activas e inactivas) ----------

    [Fact]
    public async Task ListarCategoriasAsync_DevuelveActivasEInactivasConSuEstado()
    {
        await using var db = await CrearContextoVacioAsync();
        // Mismo reparto de la semilla de desarrollo: 5 activas + 1 inactiva.
        await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        await SembrarCategoriaAsync(db, "Lácteos", estado: true);
        await SembrarCategoriaAsync(db, "Panadería", estado: true);
        await SembrarCategoriaAsync(db, "Golosinas", estado: true);
        await SembrarCategoriaAsync(db, "Limpieza", estado: true);
        await SembrarCategoriaAsync(db, "Kiosco", estado: false);
        var negocio = new CategoriaNegocio(db);

        IReadOnlyList<CategoriaRespuestaDto> listado = await negocio.ListarCategoriasAsync();

        Assert.Equal(6, listado.Count);
        Assert.Equal(6, listado.Select(c => c.IdCategoria).Distinct().Count());
        Assert.Equal(5, listado.Count(c => c.Estado));
        Assert.Contains(listado, c => c.Descripcion == "Kiosco" && !c.Estado);
        Assert.DoesNotContain(listado, c => string.IsNullOrWhiteSpace(c.Descripcion));
    }

    // ---------- RF-11: listado solo de activas (para combos) ----------

    [Fact]
    public async Task ListarCategoriasActivasAsync_NoDevuelveNingunaInactiva()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        await SembrarCategoriaAsync(db, "Lácteos", estado: true);
        await SembrarCategoriaAsync(db, "Panadería", estado: true);
        await SembrarCategoriaAsync(db, "Golosinas", estado: true);
        await SembrarCategoriaAsync(db, "Limpieza", estado: true);
        await SembrarCategoriaAsync(db, "Kiosco", estado: false);
        var negocio = new CategoriaNegocio(db);

        IReadOnlyList<CategoriaRespuestaDto> listado = await negocio.ListarCategoriasActivasAsync();

        Assert.Equal(5, listado.Count);
        Assert.All(listado, c => Assert.True(c.Estado));
        Assert.DoesNotContain(listado, c => c.Descripcion == "Kiosco");
    }

    // ---------- RF-6/RF-7: conteo de productos activos vinculados ----------

    [Fact]
    public async Task ContarProductosActivosAsync_CuentaSoloLosProductosActivos()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        await SembrarProductoAsync(db, bebidas.IdCategoria, estado: true);
        await SembrarProductoAsync(db, bebidas.IdCategoria, estado: true);
        await SembrarProductoAsync(db, bebidas.IdCategoria, estado: false);
        await SembrarProductoAsync(db, bebidas.IdCategoria, estado: false);
        await SembrarProductoAsync(db, bebidas.IdCategoria, estado: false);
        var negocio = new CategoriaNegocio(db);

        int conteo = await negocio.ContarProductosActivosAsync(bebidas.IdCategoria);

        // RF-7: la advertencia informa solo la cantidad de productos ACTIVOS.
        Assert.Equal(2, conteo);
    }

    [Fact]
    public async Task ContarProductosActivosAsync_SoloProductosInactivosVinculados_RetornaCero()
    {
        // Caso límite de la spec: categoría con productos inactivos pero no
        // activos ⇒ la baja no muestra la advertencia de productos.
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        await SembrarProductoAsync(db, bebidas.IdCategoria, estado: false);
        await SembrarProductoAsync(db, bebidas.IdCategoria, estado: false);
        var negocio = new CategoriaNegocio(db);

        int conteo = await negocio.ContarProductosActivosAsync(bebidas.IdCategoria);

        Assert.Equal(0, conteo);
    }

    [Fact]
    public async Task ContarProductosActivosAsync_NoCuentaProductosDeOtraCategoria()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Categoria lacteos = await SembrarCategoriaAsync(db, "Lácteos", estado: true);
        await SembrarProductoAsync(db, bebidas.IdCategoria, estado: true);
        await SembrarProductoAsync(db, lacteos.IdCategoria, estado: true);
        await SembrarProductoAsync(db, lacteos.IdCategoria, estado: true);
        var negocio = new CategoriaNegocio(db);

        int conteo = await negocio.ContarProductosActivosAsync(bebidas.IdCategoria);

        // Solo los productos vinculados a la categoría consultada.
        Assert.Equal(1, conteo);
    }

    // ---------- RF-8: baja lógica (sin DELETE) ----------

    [Fact]
    public async Task DarBajaCategoria_Activa_PoneEstadoFalseSinBorrarFilas()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        await SembrarProductoAsync(db, bebidas.IdCategoria, estado: true);
        var negocio = new CategoriaNegocio(db);

        RespuestaBajaCategoria respuesta = await negocio.DarBajaCategoriaAsync(
            bebidas.IdCategoria);

        Assert.Equal(ResultadoBajaCategoria.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Categoria);
        Assert.False(respuesta.Categoria.Estado);

        // Baja lógica sobre la MISMA fila: mismo Id, sin DELETE ni duplicado,
        // y los vínculos con productos quedan intactos.
        Categoria? persistida = await db.Categorias
            .AsNoTracking()
            .SingleAsync(c => c.IdCategoria == bebidas.IdCategoria);
        Assert.False(persistida.Estado);
        Assert.Equal(1, await db.Categorias.AsNoTracking().CountAsync());
        Assert.Equal(1, await db.Productos.AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task DarBajaCategoria_YaInactiva_RetornaCategoriaYaInactivaSinCambios()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria kiosco = await SembrarCategoriaAsync(db, "Kiosco", estado: false);
        await SembrarProductoAsync(db, kiosco.IdCategoria, estado: true);
        var negocio = new CategoriaNegocio(db);

        RespuestaBajaCategoria respuesta = await negocio.DarBajaCategoriaAsync(
            kiosco.IdCategoria);

        // Caso límite: la operación sobre una categoría ya inactiva no produce cambios.
        Assert.Equal(ResultadoBajaCategoria.CategoriaYaInactiva, respuesta.Resultado);
        Assert.False(string.IsNullOrWhiteSpace(respuesta.Mensaje));

        Categoria? persistida = await db.Categorias
            .AsNoTracking()
            .SingleAsync(c => c.IdCategoria == kiosco.IdCategoria);
        Assert.False(persistida.Estado);
        Assert.Equal(1, await db.Categorias.AsNoTracking().CountAsync());
        Assert.Equal(1, await db.Productos.AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task DarBajaCategoria_IdInexistente_RetornaCategoriaNoEncontrada()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new CategoriaNegocio(db);

        RespuestaBajaCategoria respuesta = await negocio.DarBajaCategoriaAsync(999);

        Assert.Equal(ResultadoBajaCategoria.CategoriaNoEncontrada, respuesta.Resultado);
        Assert.Empty(await db.Categorias.AsNoTracking().ToListAsync());
    }

    // ---------- RF-9: reactivación (misma fila, sin duplicado) ----------

    [Fact]
    public async Task ReactivarCategoria_Inactiva_PoneEstadoTrueEnLaMismaFila()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria kiosco = await SembrarCategoriaAsync(db, "Kiosco", estado: false);
        await SembrarProductoAsync(db, kiosco.IdCategoria, estado: true);
        var negocio = new CategoriaNegocio(db);

        RespuestaActivacionCategoria respuesta = await negocio.ReactivarCategoriaAsync(
            kiosco.IdCategoria);

        Assert.Equal(ResultadoActivacionCategoria.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Categoria);
        Assert.True(respuesta.Categoria.Estado);

        // Reactivación sobre la MISMA fila: mismo Id, sin registro duplicado
        // y los vínculos con productos siguen intactos.
        Categoria? persistida = await db.Categorias
            .AsNoTracking()
            .SingleAsync(c => c.IdCategoria == kiosco.IdCategoria);
        Assert.True(persistida.Estado);
        Assert.Equal(1, await db.Categorias.AsNoTracking().CountAsync());
        Assert.Equal(1, await db.Productos.AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task ReactivarCategoria_YaActiva_RetornaCategoriaYaActivaSinDuplicado()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        var negocio = new CategoriaNegocio(db);

        RespuestaActivacionCategoria respuesta = await negocio.ReactivarCategoriaAsync(
            bebidas.IdCategoria);

        // Caso límite: reactivar una categoría ya activa no genera duplicado.
        Assert.Equal(ResultadoActivacionCategoria.CategoriaYaActiva, respuesta.Resultado);
        Assert.False(string.IsNullOrWhiteSpace(respuesta.Mensaje));

        Categoria? persistida = await db.Categorias
            .AsNoTracking()
            .SingleAsync(c => c.IdCategoria == bebidas.IdCategoria);
        Assert.True(persistida.Estado);
        Assert.Equal("Bebidas", persistida.Descripcion);
        Assert.Equal(1, await db.Categorias.AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task ReactivarCategoria_IdInexistente_RetornaCategoriaNoEncontrada()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new CategoriaNegocio(db);

        RespuestaActivacionCategoria respuesta = await negocio.ReactivarCategoriaAsync(999);

        Assert.Equal(ResultadoActivacionCategoria.CategoriaNoEncontrada, respuesta.Resultado);
        Assert.Empty(await db.Categorias.AsNoTracking().ToListAsync());
    }

    private static async Task<Categoria> SembrarCategoriaAsync(
        AppDbContext db, string descripcion, bool estado)
    {
        var categoria = new Categoria { Descripcion = descripcion, Estado = estado };
        db.Categorias.Add(categoria);
        await db.SaveChangesAsync();
        return categoria;
    }

    private static async Task<Producto> SembrarProductoAsync(
        AppDbContext db, int idCategoria, bool estado)
    {
        var producto = new Producto
        {
            IdCategoria = idCategoria,
            Nombre = $"Producto {Guid.NewGuid()}",
            Stock = 10,
            PrecioVenta = 1500m,
            Estado = estado
        };
        db.Productos.Add(producto);
        await db.SaveChangesAsync();
        return producto;
    }

    private static async Task<AppDbContext> CrearContextoVacioAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"CategoriaNegocioTests-{Guid.NewGuid()}")
            .Options;
        var db = new AppDbContext(options);

        await db.Database.EnsureCreatedAsync();
        return db;
    }
}
