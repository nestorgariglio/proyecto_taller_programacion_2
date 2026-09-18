using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using CapaNegocio.DTOs.Roles;
using CapaNegocio.DTOs.Usuarios;
using CapaNegocio.Enums;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;
using Xunit;

namespace CapaNegocio.Tests;

public class UsuarioNegocioTests
{
    private const string HashAdmin =
        "$2a$12$eBa/mNaeqo3LSGn9hVAik.DscPJxIzpW6skQybIrVJJJtNg5n9kCi";
    private const string HashEncargado =
        "$2a$12$sD.QdOKFYYD7Nqv27DGA2eQMMuar5Fs.eVcRlct4bK7q3unW306n6";
    private const string HashVendedor =
        "$2a$12$L2JqegTxRoD8hsxuSJHRteY4piJbXTvueORhg5TSZBIBL0FuWm7eC";

    [Theory]
    [InlineData(90000001, "Admin123!", "Administrador")]
    [InlineData(90000002, "Encargado123!", "Encargado")]
    [InlineData(90000003, "Vendedor123!", "Vendedor")]
    public async Task ValidarIngreso_ClaveCorrecta_DevuelveExito(
        int dni, string clave, string descripcionRol)
    {
        await using var db = await CrearContextoConUsuarioAsync(dni, descripcionRol);
        var negocio = new UsuarioNegocio(db);

        RespuestaAutenticacion respuesta =
            await negocio.ValidarIngresoAsync(dni.ToString(), clave);

        Assert.Equal(ResultadoAutenticacion.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Usuario);
        Assert.Equal(descripcionRol, respuesta.Usuario.RolDescripcion);
    }

    [Fact]
    public async Task ListarUsuarios_DevuelveDatosSegurosOrdenadosYConRol()
    {
        await using var db = await CrearContextoConUsuariosParaListadoAsync();
        var negocio = new UsuarioNegocio(db);

        IReadOnlyList<UsuarioRespuestaDto> usuarios =
            await negocio.ListarUsuariosAsync();

        Assert.Equal(3, usuarios.Count);
        Assert.Collection(
            usuarios,
            usuario =>
            {
                Assert.Equal("Alvarez", usuario.Apellido);
                Assert.Equal("Ana", usuario.Nombre);
                Assert.Equal("Administrador", usuario.RolDescripcion);
            },
            usuario =>
            {
                Assert.Equal("Alvarez", usuario.Apellido);
                Assert.Equal("Luis", usuario.Nombre);
                Assert.Equal("Administrador", usuario.RolDescripcion);
            },
            usuario =>
            {
                Assert.Equal("Zeta", usuario.Apellido);
                Assert.Equal("Zoe", usuario.Nombre);
                Assert.Equal("Encargado", usuario.RolDescripcion);
            });

        Assert.Equal(90000002, usuarios[0].Dni);
        Assert.Null(typeof(UsuarioRespuestaDto).GetProperty(nameof(Usuario.Clave)));
    }

    [Fact]
    public async Task ListarUsuarios_SinInactivos_ExcluyeUsuariosDesactivados()
    {
        await using var db = await CrearContextoConUsuariosParaListadoAsync();
        var negocio = new UsuarioNegocio(db);

        IReadOnlyList<UsuarioRespuestaDto> usuariosActivos =
            await negocio.ListarUsuariosAsync(incluirInactivos: false);

        Assert.Equal(2, usuariosActivos.Count);
        Assert.All(usuariosActivos, usuario => Assert.True(usuario.Estado));
        Assert.DoesNotContain(usuariosActivos, usuario => usuario.Dni == 90000003);
    }

    [Fact]
    public async Task ListarRoles_DevuelveRolesOrdenados()
    {
        await using var db = await CrearContextoConRolesParaListadoAsync();
        var negocio = new UsuarioNegocio(db);

        IReadOnlyList<RolRespuestaDto> roles = await negocio.ListarRolesAsync();

        Assert.Collection(
            roles,
            rol =>
            {
                Assert.Equal("Administrador", rol.Descripcion);
                Assert.Equal(1, rol.IdRol);
            },
            rol =>
            {
                Assert.Equal("Encargado", rol.Descripcion);
                Assert.Equal(2, rol.IdRol);
            },
            rol =>
            {
                Assert.Equal("Vendedor", rol.Descripcion);
                Assert.Equal(3, rol.IdRol);
            });
    }

    [Fact]
    public async Task ValidarIngreso_ClaveIncorrecta_IncrementaIntentos()
    {
        await using var db = await CrearContextoConUsuarioAsync(90000001, "Administrador");
        var negocio = new UsuarioNegocio(db);

        RespuestaAutenticacion respuesta =
            await negocio.ValidarIngresoAsync("90000001", "ClaveIncorrecta!");

        Assert.Equal(ResultadoAutenticacion.CredencialesInvalidas, respuesta.Resultado);
        Assert.Equal(1, await db.Usuarios.Select(usuario => usuario.IntentosFallidos).SingleAsync());
    }

    [Fact]
    public async Task ValidarIngreso_HashInvalido_DevuelveCredencialInvalida()
    {
        await using var db = await CrearContextoConUsuarioAsync(
            90000001, "Administrador", "clave-plana-legada");
        var negocio = new UsuarioNegocio(db);

        RespuestaAutenticacion respuesta =
            await negocio.ValidarIngresoAsync("90000001", "Admin123!");

        Assert.Equal(ResultadoAutenticacion.CredencialesInvalidas, respuesta.Resultado);
        Assert.Equal(1, await db.Usuarios.Select(usuario => usuario.IntentosFallidos).SingleAsync());
    }

    [Fact]
    public async Task ValidarIngreso_TercerIntentoBloqueaUsuario()
    {
        await using var db = await CrearContextoConUsuarioAsync(90000001, "Administrador");
        var negocio = new UsuarioNegocio(db);

        for (int intento = 1; intento <= 3; intento++)
        {
            RespuestaAutenticacion respuesta =
                await negocio.ValidarIngresoAsync("90000001", "ClaveIncorrecta!");

            Assert.Equal(
                intento == 3
                    ? ResultadoAutenticacion.UsuarioBloqueado
                    : ResultadoAutenticacion.CredencialesInvalidas,
                respuesta.Resultado);
        }

        RespuestaAutenticacion respuestaPosterior =
            await negocio.ValidarIngresoAsync("90000001", "Admin123!");

        Assert.Equal(ResultadoAutenticacion.UsuarioBloqueado, respuestaPosterior.Resultado);
    }

    [Fact]
    public async Task CrearUsuario_DatosValidos_CreaUsuarioYDevuelveRespuestaSegura()
    {
        await using var db = await CrearContextoConRolAsync();
        var negocio = new UsuarioNegocio(db);
        var entrada = CrearEntradaValida();

        RespuestaCreacion respuesta = await negocio.CrearUsuarioAsync(entrada);

        Assert.Equal(ResultadoCreacion.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Usuario);
        Assert.Equal(entrada.Dni, respuesta.Usuario!.Dni);
        Assert.Equal(entrada.Nombre, respuesta.Usuario.Nombre);
        Assert.Equal(entrada.Apellido, respuesta.Usuario.Apellido);
        Assert.Equal(entrada.Sexo, respuesta.Usuario.Sexo);
        Assert.Equal(entrada.Correo, respuesta.Usuario.Correo);
        Assert.Equal(entrada.IdRol, respuesta.Usuario.IdRol);
        Assert.True(respuesta.Usuario.Estado);

        Usuario usuarioGuardado = await db.Usuarios.SingleAsync();

        Assert.NotEqual(entrada.Clave, usuarioGuardado.Clave);
        Assert.True(BC.Verify(entrada.Clave, usuarioGuardado.Clave));
    }

    [Theory]
    [InlineData("dni")]
    [InlineData("nombre")]
    [InlineData("apellido")]
    [InlineData("sexo")]
    [InlineData("clave")]
    [InlineData("rol")]
    public async Task CrearUsuario_DatoObligatorioInvalido_DevuelveDatosInvalidos(
        string datoInvalido)
    {
        await using var db = await CrearContextoConRolAsync();
        var negocio = new UsuarioNegocio(db);
        var entrada = CrearEntradaValida();

        switch (datoInvalido)
        {
            case "dni":
                entrada.Dni = 0;
                break;
            case "nombre":
                entrada.Nombre = " ";
                break;
            case "apellido":
                entrada.Apellido = string.Empty;
                break;
            case "sexo":
                entrada.Sexo = "X";
                break;
            case "clave":
                entrada.Clave = " ";
                break;
            case "rol":
                entrada.IdRol = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(datoInvalido));
        }

        RespuestaCreacion respuesta = await negocio.CrearUsuarioAsync(entrada);

        Assert.Equal(ResultadoCreacion.DatosInvalidos, respuesta.Resultado);
        Assert.Empty(await db.Usuarios.ToListAsync());
    }

    [Fact]
    public async Task CrearUsuario_DniDuplicado_DevuelveCredencialesUsadas()
    {
        await using var db = await CrearContextoConUsuarioParaCreacionAsync(
            90000001,
            "existente@correo.com");
        var negocio = new UsuarioNegocio(db);
        var entrada = CrearEntradaValida(90000001, "nuevo@correo.com");

        RespuestaCreacion respuesta = await negocio.CrearUsuarioAsync(entrada);

        Assert.Equal(ResultadoCreacion.CredencialesUsadas, respuesta.Resultado);
        Assert.Contains("DNI", respuesta.Mensaje);
        Assert.Single(await db.Usuarios.ToListAsync());
    }

    [Fact]
    public async Task CrearUsuario_CorreoDuplicado_DevuelveCredencialesUsadas()
    {
        await using var db = await CrearContextoConUsuarioParaCreacionAsync(
            90000001,
            "existente@correo.com");
        var negocio = new UsuarioNegocio(db);
        var entrada = CrearEntradaValida(90000002, "existente@correo.com");

        RespuestaCreacion respuesta = await negocio.CrearUsuarioAsync(entrada);

        Assert.Equal(ResultadoCreacion.CredencialesUsadas, respuesta.Resultado);
        Assert.Contains("correo", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Single(await db.Usuarios.ToListAsync());
    }

    [Fact]
    public async Task CrearUsuario_CorreoNulo_CreaUsuarioSinCorreo()
    {
        await using var db = await CrearContextoConRolAsync();
        var negocio = new UsuarioNegocio(db);
        var entrada = CrearEntradaValida(correo: null);

        RespuestaCreacion respuesta = await negocio.CrearUsuarioAsync(entrada);

        Assert.Equal(ResultadoCreacion.Exito, respuesta.Resultado);
        Assert.Null(respuesta.Usuario!.Correo);
        Assert.Null((await db.Usuarios.SingleAsync()).Correo);
    }

    [Fact]
    public async Task CrearUsuario_RolInexistente_DevuelveDatosInvalidos()
    {
        await using var db = await CrearContextoConRolAsync();
        var negocio = new UsuarioNegocio(db);
        var entrada = CrearEntradaValida();
        entrada.IdRol = 999;

        RespuestaCreacion respuesta = await negocio.CrearUsuarioAsync(entrada);

        Assert.Equal(ResultadoCreacion.DatosInvalidos, respuesta.Resultado);
        Assert.Empty(await db.Usuarios.ToListAsync());
    }

    [Fact]
    public async Task ModificarUsuario_DatosValidos_ActualizaDatosYConservaClave()
    {
        await using var db = await CrearContextoConUsuarioParaCreacionAsync(
            90000001,
            "existente@correo.com");
        var negocio = new UsuarioNegocio(db);
        Usuario usuarioExistente = await db.Usuarios.SingleAsync();
        string hashAnterior = usuarioExistente.Clave;

        var entrada = new ModificarUsuarioDto
        {
            IdUsuario = usuarioExistente.IdUsuario,
            Dni = 90000005,
            Nombre = "Modificado",
            Apellido = "Usuario",
            Sexo = "F",
            Correo = "modificado@correo.com",
            Clave = null,
            IdRol = usuarioExistente.IdRol
        };

        RespuestaModificacion respuesta = await negocio.ModificarUsuarioAsync(entrada);

        Assert.Equal(ResultadoModificacion.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Usuario);
        Assert.Equal(entrada.Dni, respuesta.Usuario!.Dni);
        Assert.Equal(entrada.Nombre, respuesta.Usuario.Nombre);
        Assert.Equal(entrada.Apellido, respuesta.Usuario.Apellido);
        Assert.Equal(entrada.Sexo, respuesta.Usuario.Sexo);
        Assert.Equal(entrada.Correo, respuesta.Usuario.Correo);
        Assert.Equal("Administrador", respuesta.Usuario.RolDescripcion);

        Usuario usuarioGuardado = await db.Usuarios.SingleAsync();
        Assert.Equal(hashAnterior, usuarioGuardado.Clave);
        Assert.True(BC.Verify("ClaveExistente123!", usuarioGuardado.Clave));
    }

    [Fact]
    public async Task ModificarUsuario_ClaveNueva_GuardaHashActualizado()
    {
        await using var db = await CrearContextoConUsuarioParaCreacionAsync(
            90000001,
            "existente@correo.com");
        var negocio = new UsuarioNegocio(db);
        Usuario usuarioExistente = await db.Usuarios.SingleAsync();
        string hashAnterior = usuarioExistente.Clave;

        var entrada = CrearEntradaModificacionValida(usuarioExistente.IdUsuario);
        entrada.Clave = "NuevaClave456!";

        RespuestaModificacion respuesta = await negocio.ModificarUsuarioAsync(entrada);

        Assert.Equal(ResultadoModificacion.Exito, respuesta.Resultado);

        Usuario usuarioGuardado = await db.Usuarios.SingleAsync();
        Assert.NotEqual(hashAnterior, usuarioGuardado.Clave);
        Assert.False(BC.Verify("ClaveExistente123!", usuarioGuardado.Clave));
        Assert.True(BC.Verify(entrada.Clave, usuarioGuardado.Clave));
    }

    [Fact]
    public async Task ModificarUsuario_DniDuplicado_DevuelveDatosDuplicados()
    {
        await using var db = await CrearContextoConUsuarioParaCreacionAsync(
            90000001,
            "primero@correo.com");
        Rol rol = await db.Roles.SingleAsync();
        db.Usuarios.Add(new Usuario
        {
            IdRol = rol.IdRol,
            Dni = 90000002,
            Nombre = "Segundo",
            Apellido = "Usuario",
            Sexo = "M",
            Correo = "segundo@correo.com",
            Clave = BC.HashPassword("ClaveSegundo123!")
        });
        await db.SaveChangesAsync();

        Usuario usuarioAModificar = await db.Usuarios
            .SingleAsync(usuario => usuario.Dni == 90000001);
        var entrada = CrearEntradaModificacionValida(usuarioAModificar.IdUsuario);
        entrada.Dni = 90000002;
        var negocio = new UsuarioNegocio(db);

        RespuestaModificacion respuesta = await negocio.ModificarUsuarioAsync(entrada);

        Assert.Equal(ResultadoModificacion.DatosDuplicados, respuesta.Resultado);
        Assert.Contains("DNI", respuesta.Mensaje);
        Assert.Equal(2, await db.Usuarios.CountAsync());
    }

    [Fact]
    public async Task ModificarUsuario_CorreoDuplicado_DevuelveDatosDuplicados()
    {
        await using var db = await CrearContextoConUsuarioParaCreacionAsync(
            90000001,
            "primero@correo.com");
        Rol rol = await db.Roles.SingleAsync();
        db.Usuarios.Add(new Usuario
        {
            IdRol = rol.IdRol,
            Dni = 90000002,
            Nombre = "Segundo",
            Apellido = "Usuario",
            Sexo = "M",
            Correo = "segundo@correo.com",
            Clave = BC.HashPassword("ClaveSegundo123!")
        });
        await db.SaveChangesAsync();

        Usuario usuarioAModificar = await db.Usuarios
            .SingleAsync(usuario => usuario.Dni == 90000001);
        var entrada = CrearEntradaModificacionValida(usuarioAModificar.IdUsuario);
        entrada.Correo = "segundo@correo.com";
        var negocio = new UsuarioNegocio(db);

        RespuestaModificacion respuesta = await negocio.ModificarUsuarioAsync(entrada);

        Assert.Equal(ResultadoModificacion.DatosDuplicados, respuesta.Resultado);
        Assert.Contains("correo", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, await db.Usuarios.CountAsync());
    }

    [Fact]
    public async Task ModificarUsuario_UsuarioNoEncontrado_DevuelveUsuarioNoEncontrado()
    {
        await using var db = await CrearContextoConRolAsync();
        var negocio = new UsuarioNegocio(db);
        var entrada = CrearEntradaModificacionValida(999);

        RespuestaModificacion respuesta = await negocio.ModificarUsuarioAsync(entrada);

        Assert.Equal(ResultadoModificacion.UsuarioNoEncontrado, respuesta.Resultado);
        Assert.Null(respuesta.Usuario);
    }

    [Fact]
    public async Task DesactivarUsuario_UsuarioActivo_CambiaEstadoSinEliminarlo()
    {
        await using var db = await CrearContextoConUsuarioParaCreacionAsync(
            90000001,
            "existente@correo.com");
        var negocio = new UsuarioNegocio(db);
        Usuario usuarioExistente = await db.Usuarios.SingleAsync();

        RespuestaBaja respuesta = await negocio.DesactivarUsuarioAsync(
            usuarioExistente.IdUsuario);

        Assert.Equal(ResultadoBaja.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Usuario);
        Assert.False(respuesta.Usuario!.Estado);

        Usuario? usuarioGuardado = await db.Usuarios
            .SingleOrDefaultAsync(usuario => usuario.IdUsuario == usuarioExistente.IdUsuario);
        Assert.NotNull(usuarioGuardado);
        Assert.False(usuarioGuardado!.Estado);

        RespuestaAutenticacion respuestaIngreso = await negocio.ValidarIngresoAsync(
            "90000001",
            "ClaveExistente123!");
        Assert.Equal(ResultadoAutenticacion.UsuarioInactivo, respuestaIngreso.Resultado);
    }

    [Fact]
    public async Task DesactivarUsuario_UsuarioYaInactivo_DevuelveUsuarioYaInactivo()
    {
        await using var db = await CrearContextoConUsuarioParaCreacionAsync(
            90000001,
            "existente@correo.com");
        var negocio = new UsuarioNegocio(db);
        Usuario usuarioExistente = await db.Usuarios.SingleAsync();

        await negocio.DesactivarUsuarioAsync(usuarioExistente.IdUsuario);
        RespuestaBaja respuesta = await negocio.DesactivarUsuarioAsync(
            usuarioExistente.IdUsuario);

        Assert.Equal(ResultadoBaja.UsuarioYaInactivo, respuesta.Resultado);
        Assert.NotNull(respuesta.Usuario);
        Assert.False(respuesta.Usuario!.Estado);
    }

    [Fact]
    public async Task DesactivarUsuario_UsuarioNoEncontrado_DevuelveUsuarioNoEncontrado()
    {
        await using var db = await CrearContextoConRolAsync();
        var negocio = new UsuarioNegocio(db);

        RespuestaBaja respuesta = await negocio.DesactivarUsuarioAsync(999);

        Assert.Equal(ResultadoBaja.UsuarioNoEncontrado, respuesta.Resultado);
        Assert.Null(respuesta.Usuario);
    }

    private static async Task<AppDbContext> CrearContextoConUsuarioAsync(
        int dni, string descripcionRol, string? hashAlmacenado = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"UsuarioNegocioTests-{Guid.NewGuid()}")
            .Options;
        var db = new AppDbContext(options);

        var rol = new Rol { Descripcion = descripcionRol };
        db.Roles.Add(rol);
        await db.SaveChangesAsync();

        db.Usuarios.Add(new Usuario
        {
            IdRol = rol.IdRol,
            Dni = dni,
            Nombre = "Admin",
            Apellido = "Desarrollo",
            Sexo = "M",
            Clave = hashAlmacenado ?? ObtenerHash(dni),
            Estado = true,
            IntentosFallidos = 0
        });
        await db.SaveChangesAsync();

        return db;
    }

    private static async Task<AppDbContext> CrearContextoConRolAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"UsuarioNegocioTests-{Guid.NewGuid()}")
            .Options;
        var db = new AppDbContext(options);

        db.Roles.Add(new Rol { Descripcion = "Administrador" });
        await db.SaveChangesAsync();

        return db;
    }

    private static async Task<AppDbContext> CrearContextoConUsuariosParaListadoAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"UsuarioNegocioTests-{Guid.NewGuid()}")
            .Options;
        var db = new AppDbContext(options);

        var rolAdministrador = new Rol { Descripcion = "Administrador" };
        var rolEncargado = new Rol { Descripcion = "Encargado" };
        db.Roles.AddRange(rolAdministrador, rolEncargado);
        await db.SaveChangesAsync();

        db.Usuarios.AddRange(
            new Usuario
            {
                IdRol = rolEncargado.IdRol,
                Dni = 90000001,
                Nombre = "Zoe",
                Apellido = "Zeta",
                Sexo = "F",
                Correo = "zoe@correo.com",
                Clave = "hash",
                Estado = true
            },
            new Usuario
            {
                IdRol = rolAdministrador.IdRol,
                Dni = 90000002,
                Nombre = "Ana",
                Apellido = "Alvarez",
                Sexo = "F",
                Correo = "ana@correo.com",
                Clave = "hash",
                Estado = true
            },
            new Usuario
            {
                IdRol = rolAdministrador.IdRol,
                Dni = 90000003,
                Nombre = "Luis",
                Apellido = "Alvarez",
                Sexo = "M",
                Correo = "luis@correo.com",
                Clave = "hash",
                Estado = false
            });
        await db.SaveChangesAsync();

        return db;
    }

    private static async Task<AppDbContext> CrearContextoConRolesParaListadoAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"UsuarioNegocioTests-{Guid.NewGuid()}")
            .Options;
        var db = new AppDbContext(options);

        db.Roles.AddRange(
            new Rol { IdRol = 3, Descripcion = "Vendedor" },
            new Rol { IdRol = 1, Descripcion = "Administrador" },
            new Rol { IdRol = 2, Descripcion = "Encargado" });
        await db.SaveChangesAsync();

        return db;
    }

    private static async Task<AppDbContext> CrearContextoConUsuarioParaCreacionAsync(
        int dni, string correo)
    {
        var db = await CrearContextoConRolAsync();
        Rol rol = await db.Roles.SingleAsync();

        db.Usuarios.Add(new Usuario
        {
            IdRol = rol.IdRol,
            Dni = dni,
            Nombre = "Usuario",
            Apellido = "Existente",
            Sexo = "M",
            Correo = correo,
            Clave = BC.HashPassword("ClaveExistente123!")
        });
        await db.SaveChangesAsync();

        return db;
    }

    private static CrearUsuarioDto CrearEntradaValida(
        int dni = 90000010, string? correo = "nuevo@correo.com")
    {
        return new CrearUsuarioDto
        {
            Dni = dni,
            Nombre = "Nuevo",
            Apellido = "Usuario",
            Sexo = "F",
            Correo = correo,
            Clave = "NuevaClave123!",
            IdRol = 1
        };
    }

    private static ModificarUsuarioDto CrearEntradaModificacionValida(
        int idUsuario,
        int dni = 90000010,
        string? correo = "modificado@correo.com")
    {
        return new ModificarUsuarioDto
        {
            IdUsuario = idUsuario,
            Dni = dni,
            Nombre = "Modificado",
            Apellido = "Usuario",
            Sexo = "F",
            Correo = correo,
            Clave = null,
            IdRol = 1
        };
    }

    private static string ObtenerHash(int dni) => dni switch
    {
        90000001 => HashAdmin,
        90000002 => HashEncargado,
        90000003 => HashVendedor,
        _ => throw new ArgumentOutOfRangeException(nameof(dni))
    };
}
