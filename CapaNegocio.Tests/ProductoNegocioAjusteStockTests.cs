using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using CapaNegocio.DTOs.Productos;
using CapaNegocio.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CapaNegocio.Tests;

/// <summary>
/// Pruebas de negocio del ajuste manual de stock (spec 002, T7): RF-10, RF-11,
/// RF-18, RF-19 y RF-20, con D-14 (valor absoluto, motivo obligatorio y mínimo
/// 0), D-18-bis (el permiso de escritura se valida en Negocio) y D-21 (la
/// auditoría refleja el último evento). Complementa la clase parcial con las
/// pruebas del alta (T3/T4), de la edición y el listado (T5) y de la baja y la
/// reactivación (T6).
/// </summary>
public partial class ProductoNegocioTests
{
    // ---------- RF-10 / RF-20: ajuste válido por Administrador ----------

    [Fact]
    public async Task AjustarStock_AdministradorConMotivoValido_FijaElValorAbsolutoYRegistraLaAuditoria()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Rol rolAdministrador = await SembrarRolAsync(db, "Administrador");
        Usuario administrador = await SembrarUsuarioConRolAsync(
            db, 90000010, "andrea.admin@example.com", "Andrea", rolAdministrador.IdRol);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        DateTime fechaRegistroOriginal = producto.FechaRegistro;
        var negocio = new ProductoNegocio(db);

        RespuestaAjusteStock respuesta = await negocio.AjustarStockAsync(new AjustarStockDto
        {
            IdProducto = producto.IdProducto,
            NuevoStock = 25,
            Motivo = "  Recuento físico de inventario  ",
            IdUsuario = administrador.IdUsuario
        });

        // RF-10/RF-11: un Administrador puede ajustar y la operación es exitosa.
        Assert.Equal(ResultadoAjusteStock.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Producto);
        Assert.Equal(25, respuesta.Producto.Stock);
        Assert.False(string.IsNullOrWhiteSpace(respuesta.Mensaje));

        // RNF-3: se trabaja sobre la MISMA fila, sin DELETE ni filas nuevas.
        Assert.Equal(1, await db.Productos.AsNoTracking().CountAsync());

        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);

        // RF-10/D-14: el stock queda en el valor absoluto indicado y el motivo
        // se guarda recortado.
        Assert.Equal(25, persistido.Stock);
        Assert.Equal("Recuento físico de inventario", persistido.MotivoAjusteStock);

        // RF-20/D-21: se registra el usuario del ajuste y se conserva la creación.
        Assert.Equal(administrador.IdUsuario, persistido.IdUsuarioModifica);
        Assert.Equal(creador.IdUsuario, persistido.IdUsuario);
        Assert.Equal(fechaRegistroOriginal, persistido.FechaRegistro);

        // D-23: la FechaModificacion la impone el trigger AFTER UPDATE del
        // servidor; EF Core no la escribe (límite del proveedor InMemory).
        Assert.Null(persistido.FechaModificacion);
    }

    [Fact]
    public async Task AjustarStock_EncargadoConMotivoValido_EsExitoso()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Rol rolEncargado = await SembrarRolAsync(db, "Encargado");
        Usuario encargado = await SembrarUsuarioConRolAsync(
            db, 90000011, "enrique.encargado@example.com", "Enrique", rolEncargado.IdRol);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaAjusteStock respuesta = await negocio.AjustarStockAsync(new AjustarStockDto
        {
            IdProducto = producto.IdProducto,
            NuevoStock = 2,
            Motivo = "Merma por rotura",
            IdUsuario = encargado.IdUsuario
        });

        // RF-11: el Encargado también puede ajustar el stock.
        Assert.Equal(ResultadoAjusteStock.Exito, respuesta.Resultado);
        Assert.Equal(2, respuesta.Producto!.Stock);

        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.Equal(2, persistido.Stock);
        Assert.Equal("Merma por rotura", persistido.MotivoAjusteStock);
        Assert.Equal(encargado.IdUsuario, persistido.IdUsuarioModifica);
    }

    // ---------- RF-11 / D-18-bis: el Vendedor no puede ajustar stock ----------

    [Fact]
    public async Task AjustarStock_Vendedor_RetornaSinPermisoYNoCambiaElStock()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Rol rolVendedor = await SembrarRolAsync(db, "Vendedor");
        Usuario vendedor = await SembrarUsuarioConRolAsync(
            db, 90000012, "valeria.vendedor@example.com", "Valeria", rolVendedor.IdRol);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaAjusteStock respuesta = await negocio.AjustarStockAsync(new AjustarStockDto
        {
            IdProducto = producto.IdProducto,
            NuevoStock = 40,
            Motivo = "Intento de ajuste no autorizado",
            IdUsuario = vendedor.IdUsuario
        });

        // RF-11/D-18-bis: el permiso se valida en CapaNegocio, no en la UI.
        Assert.Equal(ResultadoAjusteStock.SinPermiso, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("permiso", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Administrador", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Encargado", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        // La operación rechazada no produce ninguna escritura.
        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.Equal(5, persistido.Stock);
        Assert.Null(persistido.MotivoAjusteStock);
        Assert.Null(persistido.IdUsuarioModifica);
    }

    [Fact]
    public async Task AjustarStock_RolNoAutorizado_RetornaSinPermisoYNoCambiaElStock()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Rol rolAjeno = await SembrarRolAsync(db, "Auditor");
        Usuario usuarioAjeno = await SembrarUsuarioConRolAsync(
            db, 90000013, "aurelio.auditor@example.com", "Aurelio", rolAjeno.IdRol);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaAjusteStock respuesta = await negocio.AjustarStockAsync(new AjustarStockDto
        {
            IdProducto = producto.IdProducto,
            NuevoStock = 40,
            Motivo = "Intento de ajuste de un rol ajeno",
            IdUsuario = usuarioAjeno.IdUsuario
        });

        Assert.Equal(ResultadoAjusteStock.SinPermiso, respuesta.Resultado);
        Assert.Null(respuesta.Producto);

        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.Equal(5, persistido.Stock);
        Assert.Null(persistido.MotivoAjusteStock);
        Assert.Null(persistido.IdUsuarioModifica);
    }

    // ---------- RF-10 / D-14: valor absoluto y mínimo 0 ----------

    [Theory]
    [InlineData(12)] // sube el stock
    [InlineData(2)]  // baja el stock
    public async Task AjustarStock_ValorAbsoluto_SubeOBajaElStockAlValorIndicado(int nuevoStock)
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Rol rolAdministrador = await SembrarRolAsync(db, "Administrador");
        Usuario administrador = await SembrarUsuarioConRolAsync(
            db, 90000014, "andrea.absoluto@example.com", "Andrea", rolAdministrador.IdRol);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario, stock: 5);
        var negocio = new ProductoNegocio(db);

        RespuestaAjusteStock respuesta = await negocio.AjustarStockAsync(new AjustarStockDto
        {
            IdProducto = producto.IdProducto,
            NuevoStock = nuevoStock,
            Motivo = "Ajuste por inventario",
            IdUsuario = administrador.IdUsuario
        });

        // D-14: el stock NO se incrementa ni decrementa: se fija en el valor absoluto.
        Assert.Equal(ResultadoAjusteStock.Exito, respuesta.Resultado);

        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.Equal(nuevoStock, persistido.Stock);
    }

    [Fact]
    public async Task AjustarStock_Cero_EsValido()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Rol rolEncargado = await SembrarRolAsync(db, "Encargado");
        Usuario encargado = await SembrarUsuarioConRolAsync(
            db, 90000015, "enrique.cero@example.com", "Enrique", rolEncargado.IdRol);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaAjusteStock respuesta = await negocio.AjustarStockAsync(new AjustarStockDto
        {
            IdProducto = producto.IdProducto,
            NuevoStock = 0,
            Motivo = "Sin unidades en depósito",
            IdUsuario = encargado.IdUsuario
        });

        // D-14: 0 es el límite inferior válido (no se admite stock negativo).
        Assert.Equal(ResultadoAjusteStock.Exito, respuesta.Resultado);
        Assert.Equal(0, respuesta.Producto!.Stock);

        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.Equal(0, persistido.Stock);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-50)]
    public async Task AjustarStock_StockNegativo_RetornaStockInvalidoSinPersistir(int nuevoStock)
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Rol rolAdministrador = await SembrarRolAsync(db, "Administrador");
        Usuario administrador = await SembrarUsuarioConRolAsync(
            db, 90000016, "andrea.negativo@example.com", "Andrea", rolAdministrador.IdRol);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaAjusteStock respuesta = await negocio.AjustarStockAsync(new AjustarStockDto
        {
            IdProducto = producto.IdProducto,
            NuevoStock = nuevoStock,
            Motivo = "Ajuste inválido",
            IdUsuario = administrador.IdUsuario
        });

        Assert.Equal(ResultadoAjusteStock.StockInvalido, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("stock", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("negativo", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.Equal(5, persistido.Stock);
        Assert.Null(persistido.MotivoAjusteStock);
        Assert.Null(persistido.IdUsuarioModifica);
    }

    // ---------- RF-10: motivo obligatorio ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public async Task AjustarStock_MotivoVacio_RetornaMotivoRequeridoSinPersistir(string motivo)
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Rol rolAdministrador = await SembrarRolAsync(db, "Administrador");
        Usuario administrador = await SembrarUsuarioConRolAsync(
            db, 90000017, "andrea.motivo@example.com", "Andrea", rolAdministrador.IdRol);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaAjusteStock respuesta = await negocio.AjustarStockAsync(new AjustarStockDto
        {
            IdProducto = producto.IdProducto,
            NuevoStock = 10,
            Motivo = motivo,
            IdUsuario = administrador.IdUsuario
        });

        Assert.Equal(ResultadoAjusteStock.MotivoRequerido, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("motivo", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.Equal(5, persistido.Stock);
        Assert.Null(persistido.MotivoAjusteStock);
    }

    [Fact]
    public async Task AjustarStock_MotivoDeMasDe300Caracteres_RechazaSinPersistir()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Rol rolAdministrador = await SembrarRolAsync(db, "Administrador");
        Usuario administrador = await SembrarUsuarioConRolAsync(
            db, 90000018, "andrea.motivo.largo@example.com", "Andrea", rolAdministrador.IdRol);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaAjusteStock respuesta = await negocio.AjustarStockAsync(new AjustarStockDto
        {
            IdProducto = producto.IdProducto,
            NuevoStock = 10,
            Motivo = $"  {new string('x', 301)}  ",
            IdUsuario = administrador.IdUsuario
        });

        // El motivo es texto libre, pero no puede superar la longitud de la
        // columna (300 caracteres, T2/AppDbContext).
        Assert.Equal(ResultadoAjusteStock.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("motivo", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("300", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.Equal(5, persistido.Stock);
        Assert.Null(persistido.MotivoAjusteStock);
    }

    // ---------- RF-10: producto y usuario obligatorios y existentes ----------

    [Fact]
    public async Task AjustarStock_ProductoInexistente_RetornaProductoNoEncontrado()
    {
        await using var db = await CrearContextoVacioAsync();
        Rol rolAdministrador = await SembrarRolAsync(db, "Administrador");
        Usuario administrador = await SembrarUsuarioConRolAsync(
            db, 90000019, "andrea.inexistente@example.com", "Andrea", rolAdministrador.IdRol);
        var negocio = new ProductoNegocio(db);

        RespuestaAjusteStock respuesta = await negocio.AjustarStockAsync(new AjustarStockDto
        {
            IdProducto = 999,
            NuevoStock = 10,
            Motivo = "Ajuste sobre un producto inexistente",
            IdUsuario = administrador.IdUsuario
        });

        Assert.Equal(ResultadoAjusteStock.ProductoNoEncontrado, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("no existe", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await db.Productos.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task AjustarStock_UsuarioInexistente_RechazaSinPersistir()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaAjusteStock respuesta = await negocio.AjustarStockAsync(new AjustarStockDto
        {
            IdProducto = producto.IdProducto,
            NuevoStock = 10,
            Motivo = "Ajuste con usuario inexistente",
            IdUsuario = 999
        });

        Assert.Equal(ResultadoAjusteStock.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("usuario", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.Equal(5, persistido.Stock);
        Assert.Null(persistido.MotivoAjusteStock);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task AjustarStock_SinUsuarioIdentificado_RechazaSinPersistir(int idUsuario)
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaAjusteStock respuesta = await negocio.AjustarStockAsync(new AjustarStockDto
        {
            IdProducto = producto.IdProducto,
            NuevoStock = 10,
            Motivo = "Ajuste sin usuario identificado",
            IdUsuario = idUsuario
        });

        Assert.Equal(ResultadoAjusteStock.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("usuario", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);

        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.Equal(5, persistido.Stock);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task AjustarStock_IdProductoInvalido_RetornaDatosInvalidos(int idProducto)
    {
        await using var db = await CrearContextoVacioAsync();
        Rol rolAdministrador = await SembrarRolAsync(db, "Administrador");
        Usuario administrador = await SembrarUsuarioConRolAsync(
            db, 90000020, "andrea.id.invalido@example.com", "Andrea", rolAdministrador.IdRol);
        var negocio = new ProductoNegocio(db);

        RespuestaAjusteStock respuesta = await negocio.AjustarStockAsync(new AjustarStockDto
        {
            IdProducto = idProducto,
            NuevoStock = 10,
            Motivo = "Ajuste con producto inválido",
            IdUsuario = administrador.IdUsuario
        });

        Assert.Equal(ResultadoAjusteStock.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Producto);
        Assert.Contains("producto", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
    }

    // ---------- D-21: la auditoría refleja solo el último evento ----------

    [Fact]
    public async Task AjustarStock_SegundoAjuste_SobrescribeLaAuditoriaDelPrimero()
    {
        await using var db = await CrearContextoVacioAsync();
        Categoria bebidas = await SembrarCategoriaAsync(db, "Bebidas", estado: true);
        Usuario creador = await SembrarUsuarioAsync(db);
        Rol rolAdministrador = await SembrarRolAsync(db, "Administrador");
        Rol rolEncargado = await SembrarRolAsync(db, "Encargado");
        Usuario administrador = await SembrarUsuarioConRolAsync(
            db, 90000021, "andrea.ultimo@example.com", "Andrea", rolAdministrador.IdRol);
        Usuario encargado = await SembrarUsuarioConRolAsync(
            db, 90000022, "enrique.ultimo@example.com", "Enrique", rolEncargado.IdRol);
        Producto producto = await SembrarProductoParaEdicionAsync(
            db, "7790001000011", "Teclado", bebidas.IdCategoria, creador.IdUsuario);
        var negocio = new ProductoNegocio(db);

        RespuestaAjusteStock primerAjuste = await negocio.AjustarStockAsync(new AjustarStockDto
        {
            IdProducto = producto.IdProducto,
            NuevoStock = 30,
            Motivo = "Alta de mercadería",
            IdUsuario = administrador.IdUsuario
        });

        RespuestaAjusteStock segundoAjuste = await negocio.AjustarStockAsync(new AjustarStockDto
        {
            IdProducto = producto.IdProducto,
            NuevoStock = 4,
            Motivo = "Merma detectada en el recuento",
            IdUsuario = encargado.IdUsuario
        });

        Assert.Equal(ResultadoAjusteStock.Exito, primerAjuste.Resultado);
        Assert.Equal(ResultadoAjusteStock.Exito, segundoAjuste.Resultado);

        // Sin historial de movimientos: una sola fila con la auditoría del último evento.
        Assert.Equal(1, await db.Productos.AsNoTracking().CountAsync());

        Producto persistido = await db.Productos.AsNoTracking()
            .SingleAsync(p => p.IdProducto == producto.IdProducto);
        Assert.Equal(4, persistido.Stock);
        Assert.Equal("Merma detectada en el recuento", persistido.MotivoAjusteStock);
        Assert.Equal(encargado.IdUsuario, persistido.IdUsuarioModifica);
        Assert.Equal(creador.IdUsuario, persistido.IdUsuario);
    }

    // ---------- Helpers ----------

    private static async Task<Rol> SembrarRolAsync(AppDbContext db, string descripcion)
    {
        var rol = new Rol { Descripcion = descripcion };
        db.Roles.Add(rol);
        await db.SaveChangesAsync();
        return rol;
    }

    private static async Task<Usuario> SembrarUsuarioConRolAsync(
        AppDbContext db, int dni, string correo, string nombre, int idRol)
    {
        var usuario = new Usuario
        {
            IdRol = idRol,
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

/// <summary>
/// Pruebas del permiso de gestión (escritura) de productos (spec 002, T7):
/// RF-11 y RF-18 con D-18-bis. Solo Administrador y Encargado pueden gestionar
/// productos; el Vendedor no (su lectura del catálogo llega desde Ventas,
/// spec 005). No se modifica la política de módulos ni sus pruebas existentes.
/// </summary>
public class PoliticaAccesoProductosTests
{
    [Theory]
    [InlineData("Administrador")]
    [InlineData("Encargado")]
    [InlineData("  aDmInIsTrAdOr ")]
    public void PuedeGestionarProductos_RolesAutorizados_DevuelveTrue(string rol)
    {
        Assert.True(PoliticaAcceso.PuedeGestionarProductos(rol));
    }

    [Theory]
    [InlineData("Vendedor")]
    [InlineData("  vendedor  ")]
    [InlineData("Auditor")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void PuedeGestionarProductos_RolesNoAutorizados_DevuelveFalse(string? rol)
    {
        Assert.False(PoliticaAcceso.PuedeGestionarProductos(rol));
    }
}
