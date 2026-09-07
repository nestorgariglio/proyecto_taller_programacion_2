using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using Microsoft.EntityFrameworkCore;
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
    public async Task ValidarIngresoAceptaLasClavesCorrespondientesALosHashes(
        int dni, string clave, string descripcionRol)
    {
        await using var db = await CrearContextoConUsuarioAsync(dni, descripcionRol);
        var negocio = new UsuarioNegocio(db);

        RespuestaAutenticacion respuesta =
            await negocio.ValidarIngresoAsync(dni.ToString(), clave);

        Assert.Equal(ResultadoAutenticacion.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Usuario);
        Assert.Equal(descripcionRol, respuesta.Usuario.Rol?.Descripcion);
    }

    [Fact]
    public async Task ValidarIngresoRechazaUnaClaveIncorrectaYRegistraElIntento()
    {
        await using var db = await CrearContextoConUsuarioAsync(90000001, "Administrador");
        var negocio = new UsuarioNegocio(db);

        RespuestaAutenticacion respuesta =
            await negocio.ValidarIngresoAsync("90000001", "ClaveIncorrecta!");

        Assert.Equal(ResultadoAutenticacion.CredencialesInvalidas, respuesta.Resultado);
        Assert.Equal(1, await db.Usuarios.Select(usuario => usuario.IntentosFallidos).SingleAsync());
    }

    [Fact]
    public async Task ValidarIngresoTrataUnHashInvalidoComoCredencialInvalida()
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
    public async Task TercerIntentoIncorrectoBloqueaElUsuarioYElLoginPosteriorNoSeEvalua()
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
            Clave = hashAlmacenado ?? ObtenerHash(dni),
            Estado = true,
            IntentosFallidos = 0
        });
        await db.SaveChangesAsync();

        return db;
    }

    private static string ObtenerHash(int dni) => dni switch
    {
        90000001 => HashAdmin,
        90000002 => HashEncargado,
        90000003 => HashVendedor,
        _ => throw new ArgumentOutOfRangeException(nameof(dni))
    };
}
