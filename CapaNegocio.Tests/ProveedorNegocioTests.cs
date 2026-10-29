using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using CapaNegocio.DTOs.Proveedores;
using CapaNegocio.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Threading;

namespace CapaNegocio.Tests;

public class ProveedorNegocioTests
{
    private const string MensajeCuitObligatorio = "El CUIT es obligatorio.";
    private const string MensajeRazonSocialObligatoria = "La Razón Social es obligatoria.";
    private const string MensajeCuitFormato = "El CUIT debe ser solo números (solo dígitos 0-9).";
    private const string MensajeRazonSocialFormato = "La Razón Social debe admitir letras, espacios internos, apóstrofo y guion.";
    private const string MensajeCorreoFormato = "El correo no tiene un formato válido (ejemplo: nombre@dominio.com).";
    private const string MensajeTelefonoFormato = "El teléfono solo admite dígitos.";
    private const string MensajeCuitDuplicado = "Ya existe un proveedor con el CUIT";
    private const string MensajeCorreoDuplicado = "Ya existe un proveedor con el correo";
    private const string MensajeSinResultados = "No se encontraron proveedores que coincidan con la búsqueda.";
    private const string MensajeSinProveedoresFiltro = "No hay proveedores para mostrar con el filtro seleccionado.";
    private const string MensajeProveedorNoEncontrado = "El proveedor indicado no existe.";
    private const string MensajeProveedorInvalido = "Debe indicar un proveedor válido.";
    private const string MensajePermisoDenegadoBaja = "No tiene permisos para dar de baja proveedores.";
    private const string MensajePermisoDenegadoReactivacion = "No tiene permisos para reactivar proveedores.";
    private const string MensajeErrorPersistencia = "No se pudo completar la operación por un problema con el origen de datos. Intente nuevamente.";

    private static async Task<AppDbContext> CrearContextoVacioAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"ProveedorNegocioTests-{Guid.NewGuid()}")
            .Options;
        var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    private static async Task<Proveedor> SembrarProveedorAsync(AppDbContext db, string cuit, string razonSocial, string? correo = null, string? telefono = null, bool estado = true)
    {
        var p = new Proveedor { Cuit = cuit, RazonSocial = razonSocial, Correo = correo, Telefono = telefono, Estado = estado, FechaRegistro = DateTime.UtcNow.AddDays(-1) };
        db.Proveedores.Add(p);
        await db.SaveChangesAsync();
        return p;
    }

    private static async Task<Compra> SembrarCompraAsync(AppDbContext db, int idProveedor, bool anulada = false)
    {
        var c = new Compra
        {
            IdProveedor = idProveedor,
            MontoTotal = 1000,
            Estado = anulada ? false : true,
            FechaRegistro = DateTime.UtcNow.AddDays(-2),
            IdUsuario = 1
        };
        db.Compras.Add(c);
        await db.SaveChangesAsync();
        return c;
    }

    // RF-1: alta exitosa
    [Fact]
    public async Task CrearProveedor_AltaValida_PersisteActivoYDevuelveDto()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.CrearProveedorAsync(new CrearProveedorDto { Cuit = "30123456789", RazonSocial = "Logitech Argentina SA", Correo = "contacto@logitech.com", Telefono = "3624112233" });
        Assert.Equal(ResultadoCreacionProveedor.Exito, resp.Resultado);
        Assert.NotNull(resp.Proveedor);
        Assert.True(resp.Proveedor.Estado);
        Assert.Equal("30123456789", resp.Proveedor.Cuit);
        Assert.Equal("Logitech Argentina SA", resp.Proveedor.RazonSocial);
        Assert.Equal("contacto@logitech.com", resp.Proveedor.Correo);
        Assert.Equal("3624112233", resp.Proveedor.Telefono);
        Assert.Contains("Logitech Argentina SA", resp.Mensaje);
        Assert.Equal(1, await db.Proveedores.CountAsync());
    }

    // RF-2: obligatorio vacío
    [Theory]
    [InlineData(null, "Logitech", MensajeCuitObligatorio)]
    [InlineData("", "Logitech", MensajeCuitObligatorio)]
    [InlineData("   ", "Logitech", MensajeCuitObligatorio)]
    [InlineData("30123456789", null, MensajeRazonSocialObligatoria)]
    [InlineData("30123456789", "", MensajeRazonSocialObligatoria)]
    [InlineData("30123456789", "   ", MensajeRazonSocialObligatoria)]
    public async Task CrearProveedor_ObligatoriosVacios_Rechaza(string? cuit, string? razon, string mensajeEsperado)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.CrearProveedorAsync(new CrearProveedorDto { Cuit = cuit, RazonSocial = razon });
        Assert.Equal(ResultadoCreacionProveedor.DatosInvalidos, resp.Resultado);
        Assert.Equal(mensajeEsperado, resp.Mensaje);
        Assert.Equal(0, await db.Proveedores.CountAsync());
    }

    // RF-3: recorte
    [Fact]
    public async Task CrearProveedor_RecorteGuardaDatosRecortados()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.CrearProveedorAsync(new CrearProveedorDto { Cuit = "  30123456789  ", RazonSocial = "  Logitech  ", Correo = "  contacto@logitech.com  ", Telefono = "  3624112233  " });
        Assert.Equal(ResultadoCreacionProveedor.Exito, resp.Resultado);
        Assert.NotNull(resp.Proveedor);
        Assert.Equal("30123456789", resp.Proveedor.Cuit);
        Assert.Equal("Logitech", resp.Proveedor.RazonSocial);
        Assert.Equal("contacto@logitech.com", resp.Proveedor.Correo);
        Assert.Equal("3624112233", resp.Proveedor.Telefono);
    }

    // RF-4: documento formato
    [Theory]
    [InlineData("30-123-456", MensajeCuitFormato)]
    [InlineData("30123456A", MensajeCuitFormato)]
    public async Task CrearProveedor_CuitFormatoInvalido_Rechaza(string cuit, string mensaje)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.CrearProveedorAsync(new CrearProveedorDto { Cuit = cuit, RazonSocial = "Logitech" });
        Assert.Equal(ResultadoCreacionProveedor.DatosInvalidos, resp.Resultado);
        Assert.Contains(mensaje, resp.Mensaje);
    }

    // RF-5: razon social formato / vacío
    [Theory]
    [InlineData("", MensajeRazonSocialObligatoria)]
    [InlineData("Logitech1", MensajeRazonSocialFormato)]
    public async Task CrearProveedor_RazonSocialInvalida_Rechaza(string razon, string mensaje)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.CrearProveedorAsync(new CrearProveedorDto { Cuit = "30123456789", RazonSocial = razon });
        Assert.Equal(ResultadoCreacionProveedor.DatosInvalidos, resp.Resultado);
        Assert.Contains(mensaje, resp.Mensaje);
    }

    [Fact]
    public async Task CrearProveedor_RazonSocialValida_Acepta()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.CrearProveedorAsync(new CrearProveedorDto { Cuit = "30123456789", RazonSocial = "Logitech Argentina SA" });
        Assert.Equal(ResultadoCreacionProveedor.Exito, resp.Resultado);
    }

    // RF-6: correo formato
    [Theory]
    [InlineData("contacto@logitech", MensajeCorreoFormato)]
    [InlineData("@mail.com", MensajeCorreoFormato)]
    [InlineData("contacto@mail", MensajeCorreoFormato)]
    [InlineData("contacto@@mail.com", MensajeCorreoFormato)]
    [InlineData("contacto perez@mail.com", MensajeCorreoFormato)]
    [InlineData("cliente@dominio.c", MensajeCorreoFormato)]
    public async Task CrearProveedor_CorreoInvalido_Rechaza(string correo, string mensaje)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.CrearProveedorAsync(new CrearProveedorDto { Cuit = "30123456789", RazonSocial = "Logitech", Correo = correo });
        Assert.Equal(ResultadoCreacionProveedor.DatosInvalidos, resp.Resultado);
        Assert.Contains(mensaje, resp.Mensaje);
    }

    [Fact]
    public async Task CrearProveedor_CorreoValido_Acepta()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.CrearProveedorAsync(new CrearProveedorDto { Cuit = "30123456789", RazonSocial = "Logitech", Correo = "ventas@dominio.com.ar" });
        Assert.Equal(ResultadoCreacionProveedor.Exito, resp.Resultado);
    }

    // RF-7 / RF-8: teléfono solo dígitos
    [Theory]
    [InlineData("011-4555-0000", MensajeTelefonoFormato)]
    [InlineData("011 4555 0000", MensajeTelefonoFormato)]
    [InlineData("(011) 4555 0000", MensajeTelefonoFormato)]
    [InlineData("+541145550000", MensajeTelefonoFormato)]
    public async Task CrearProveedor_TelefonoConCaracteres_Rechaza(string tel, string mensaje)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.CrearProveedorAsync(new CrearProveedorDto { Cuit = "30123456789", RazonSocial = "Logitech", Telefono = tel });
        Assert.Equal(ResultadoCreacionProveedor.DatosInvalidos, resp.Resultado);
        Assert.Contains(mensaje, resp.Mensaje);
    }

    [Fact]
    public async Task CrearProveedor_TelefonoValido_Acepta()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.CrearProveedorAsync(new CrearProveedorDto { Cuit = "30123456789", RazonSocial = "Logitech", Telefono = "3624112233" });
        Assert.Equal(ResultadoCreacionProveedor.Exito, resp.Resultado);
    }

    // RF-9: unicidad CUIT (activo / inactivo / capitalización / mensaje neutral)
    [Fact]
    public async Task CrearProveedor_CuitDuplicadoActivo_Rechaza()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarProveedorAsync(db, "30123456789", "Existente", estado: true);
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.CrearProveedorAsync(new CrearProveedorDto { Cuit = "30123456789", RazonSocial = "Nuevo" });
        Assert.Equal(ResultadoCreacionProveedor.CuitDuplicado, resp.Resultado);
        Assert.Contains(MensajeCuitDuplicado, resp.Mensaje);
        Assert.Equal(1, await db.Proveedores.CountAsync());
    }

    [Fact]
    public async Task CrearProveedor_CuitDuplicadoInactivo_MensajeNeutral()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarProveedorAsync(db, "30123456789", "Inactivo", estado: false);
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.CrearProveedorAsync(new CrearProveedorDto { Cuit = "30123456789", RazonSocial = "Nuevo" });
        Assert.Equal(ResultadoCreacionProveedor.CuitDuplicado, resp.Resultado);
        Assert.Contains(MensajeCuitDuplicado, resp.Mensaje);
        Assert.Contains("inactivo", resp.Mensaje, System.StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("reactiv", resp.Mensaje, System.StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("12345678901", "12345678901")]
    public async Task CrearProveedor_CuitCapitalizacionRechaza(string existente, string nuevo)
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarProveedorAsync(db, existente, "Existente");
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.CrearProveedorAsync(new CrearProveedorDto { Cuit = nuevo, RazonSocial = "Nuevo" });
        Assert.Equal(ResultadoCreacionProveedor.CuitDuplicado, resp.Resultado);
    }

    // RF-10 / RF-11: unicidad correo
    [Fact]
    public async Task CrearProveedor_CorreoDuplicadoRechaza()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarProveedorAsync(db, "10000000000", "Beta", correo: "dup@mail.com");
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.CrearProveedorAsync(new CrearProveedorDto { Cuit = "20000000000", RazonSocial = "Beta", Correo = "dup@mail.com" });
        Assert.Equal(ResultadoCreacionProveedor.CorreoDuplicado, resp.Resultado);
        Assert.Contains(MensajeCorreoDuplicado, resp.Mensaje);
    }

    [Fact]
    public async Task CrearProveedor_MuchosSinCorreo_SeAceptan()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ProveedorNegocio(db);
        for (int i = 1; i <= 3; i++)
        {
            var razon = "Proveedor" + (char)('A' + i - 1);
            var resp = await negocio.CrearProveedorAsync(new CrearProveedorDto { Cuit = $"{i}000000000", RazonSocial = razon });
            Assert.Equal(ResultadoCreacionProveedor.Exito, resp.Resultado);
        }
        Assert.Equal(3, await db.Proveedores.CountAsync());
    }

    // RF-11: modificación con autoexclusión
    [Fact]
    public async Task ModificarProveedor_SinCambiarCuitNiCorreo_NoEsDuplicado()
    {
        await using var db = await CrearContextoVacioAsync();
        var existente = await SembrarProveedorAsync(db, "30123456789", "Logitech", correo: "logi@mail.com");
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.ModificarProveedorAsync(new ModificarProveedorDto { IdProveedor = existente.IdProveedor, Cuit = "30123456789", RazonSocial = "Logitech Actualizado", Correo = "logi@mail.com" });
        Assert.Equal(ResultadoModificacionProveedor.Exito, resp.Resultado);
    }

    [Fact]
    public async Task ModificarProveedor_CuitDeOtro_RechazaDuplicado()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarProveedorAsync(db, "30123456789", "Alpha");
        var propio = await SembrarProveedorAsync(db, "99999999999", "Beta");
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.ModificarProveedorAsync(new ModificarProveedorDto { IdProveedor = propio.IdProveedor, Cuit = "30123456789", RazonSocial = "Beta" });
        Assert.Equal(ResultadoModificacionProveedor.CuitDuplicado, resp.Resultado);
    }

    // RF-12: vaciado de correo / teléfono
    [Fact]
    public async Task ModificarProveedor_VaciarCorreoYTelefono_LosGuardaComoNulos()
    {
        await using var db = await CrearContextoVacioAsync();
        var p = await SembrarProveedorAsync(db, "30123456789", "Logitech", correo: "logi@mail.com", telefono: "3624112233");
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.ModificarProveedorAsync(new ModificarProveedorDto { IdProveedor = p.IdProveedor, Cuit = "30123456789", RazonSocial = "Logitech", Correo = "", Telefono = "" });
        Assert.Equal(ResultadoModificacionProveedor.Exito, resp.Resultado);
        Assert.NotNull(resp.Proveedor);
        Assert.Null(resp.Proveedor.Correo);
        Assert.Null(resp.Proveedor.Telefono);
    }

    // RF-13: modificación de inactivo sin reactivar
    [Fact]
    public async Task ModificarProveedor_Inactivo_ConservaEstadoInactivo()
    {
        await using var db = await CrearContextoVacioAsync();
        var p = await SembrarProveedorAsync(db, "30123456789", "Logitech", estado: false);
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.ModificarProveedorAsync(new ModificarProveedorDto { IdProveedor = p.IdProveedor, Cuit = "30123456789", RazonSocial = "Logitech Editado" });
        Assert.Equal(ResultadoModificacionProveedor.Exito, resp.Resultado);
        Assert.NotNull(resp.Proveedor);
        Assert.False(resp.Proveedor.Estado);
        var guardado = await db.Proveedores.AsNoTracking().SingleAsync(x => x.IdProveedor == p.IdProveedor);
        Assert.False(guardado.Estado);
    }

    // RF-14: conservación de estado y compras al modificar
    [Fact]
    public async Task ModificarProveedor_ConCompras_ConservaEstadoYCompras()
    {
        await using var db = await CrearContextoVacioAsync();
        var p = await SembrarProveedorAsync(db, "30123456789", "Logitech", estado: true);
        await SembrarCompraAsync(db, p.IdProveedor);
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.ModificarProveedorAsync(new ModificarProveedorDto { IdProveedor = p.IdProveedor, Cuit = "30123456789", RazonSocial = "Logitech Editado" });
        Assert.Equal(ResultadoModificacionProveedor.Exito, resp.Resultado);
        Assert.True(resp.Proveedor!.Estado);
        Assert.Equal(1, await db.Compras.CountAsync(c => c.IdProveedor == p.IdProveedor));
    }

    // RF-15 / RF-16: listado con filtro y búsqueda parcial
    [Fact]
    public async Task ListarProveedores_FiltroActivos_VaciaYTextoParcial()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarProveedorAsync(db, "11111111111", "Alpha", estado: true);
        await SembrarProveedorAsync(db, "22222222222", "Beta", estado: false);
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.ListarProveedoresAsync(FiltroEstadoProveedor.Activos, "Alpha");
        Assert.Equal(ResultadoListadoProveedores.ConResultados, resp.Resultado);
        Assert.Single(resp.Proveedores);
        Assert.Equal("11111111111", resp.Proveedores![0].Cuit);
    }

    // RF-17: búsqueda rápida Take(20) y orden por RazonSocial
    [Fact]
    public async Task BuscarProveedoresParaPos_OrdenadoYTop20_ConResultados()
    {
        await using var db = await CrearContextoVacioAsync();
        for (int i = 1; i <= 22; i++)
        {
            await SembrarProveedorAsync(db, $"{i:D11}", $"Proveedor{i:D2}", estado: true);
        }
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.BuscarProveedoresParaPosAsync("Proveedor");
        Assert.Equal(ResultadoListadoProveedores.ConResultados, resp.Resultado);
        Assert.True(resp.Proveedores!.Count <= 20);
        // orden ascendente por RazonSocial
        Assert.True(resp.Proveedores![0].RazonSocial.CompareTo(resp.Proveedores![1].RazonSocial) <= 0);
    }

    [Fact]
    public async Task BuscarProveedoresParaPos_TextoVacio_SinResultadosSinConsultar()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.BuscarProveedoresParaPosAsync("   ");
        Assert.Equal(ResultadoListadoProveedores.SinResultados, resp.Resultado);
        Assert.Empty(resp.Proveedores!);
    }

    // RF-18: sin resultados con texto informado
    [Fact]
    public async Task BuscarProveedoresParaPos_SinCoincidencias_SinResultados()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarProveedorAsync(db, "11111111111", "Alpha");
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.BuscarProveedoresParaPosAsync("NonExistent");
        Assert.Equal(ResultadoListadoProveedores.SinResultados, resp.Resultado);
        Assert.Contains(MensajeSinResultados, resp.Mensaje);
    }

    // RF-19: visibilidad tras alta
    [Fact]
    public async Task ListarProveedores_TrasAlta_ProveedorActivoVisible()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.CrearProveedorAsync(new CrearProveedorDto { Cuit = "30123456789", RazonSocial = "Visibles" });
        Assert.Equal(ResultadoCreacionProveedor.Exito, resp.Resultado);
        var listado = await negocio.ListarProveedoresAsync(FiltroEstadoProveedor.Activos, null);
        Assert.Contains(listado.Proveedores!, p => p.Cuit == "30123456789");
    }

    // RF-20 / RF-21: baja con confirmación y compras asociadas
    [Fact]
    public async Task ObtenerConfirmacionBaja_ConCompras_MensajeConCantidad()
    {
        await using var db = await CrearContextoVacioAsync();
        var p = await SembrarProveedorAsync(db, "30123456789", "Logitech");
        await SembrarCompraAsync(db, p.IdProveedor);
        await SembrarCompraAsync(db, p.IdProveedor, anulada: true);
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.ObtenerConfirmacionBajaProveedorAsync(p.IdProveedor, "Administrador");
        Assert.Equal(ResultadoConfirmacionBajaProveedor.ConfirmacionRequerida, resp.Resultado);
        Assert.Equal(2, resp.ComprasAsociadas);
        Assert.Contains("2 compras", resp.Mensaje);
    }

    [Fact]
    public async Task DarBajaProveedor_Confirmado_ConCompras_ConservaEstadoInactivoYCompras()
    {
        await using var db = await CrearContextoVacioAsync();
        var p = await SembrarProveedorAsync(db, "30123456789", "Logitech");
        await SembrarCompraAsync(db, p.IdProveedor);
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.DarBajaProveedorAsync(p.IdProveedor, "Administrador");
        Assert.Equal(ResultadoBajaProveedor.Exito, resp.Resultado);
        Assert.False(resp.Proveedor!.Estado);
        Assert.Equal(1, await db.Compras.CountAsync(c => c.IdProveedor == p.IdProveedor));
    }

    // RF-22 / RF-23: reactivación y baja/reactivación improcedente
    [Fact]
    public async Task ReactivarProveedor_Inactivo_Exito()
    {
        await using var db = await CrearContextoVacioAsync();
        var p = await SembrarProveedorAsync(db, "30123456789", "Logitech", estado: false);
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.ReactivarProveedorAsync(p.IdProveedor, "Administrador");
        Assert.Equal(ResultadoActivacionProveedor.Exito, resp.Resultado);
        Assert.True(resp.Proveedor!.Estado);
    }

    [Fact]
    public async Task DarBajaProveedor_Inactivo_Improcedente()
    {
        await using var db = await CrearContextoVacioAsync();
        var p = await SembrarProveedorAsync(db, "30123456789", "Logitech", estado: false);
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.DarBajaProveedorAsync(p.IdProveedor, "Administrador");
        Assert.Equal(ResultadoBajaProveedor.ProveedorYaInactivo, resp.Resultado);
        Assert.Equal(1, await db.Proveedores.CountAsync());
    }

    [Fact]
    public async Task ReactivarProveedor_Activo_Improcedente()
    {
        await using var db = await CrearContextoVacioAsync();
        var p = await SembrarProveedorAsync(db, "30123456789", "Logitech", estado: true);
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.ReactivarProveedorAsync(p.IdProveedor, "Administrador");
        Assert.Equal(ResultadoActivacionProveedor.ProveedorYaActivo, resp.Resultado);
        var proveedor = await db.Proveedores.AsNoTracking().FirstAsync(x => x.IdProveedor == p.IdProveedor);
        Assert.True(proveedor.Estado);
    }

    // RF-24: prohibición borrado físico
    [Fact]
    public async Task DarBajaProveedor_NoBorraFila()
    {
        await using var db = await CrearContextoVacioAsync();
        var p = await SembrarProveedorAsync(db, "30123456789", "Logitech");
        var negocio = new ProveedorNegocio(db);
        await negocio.DarBajaProveedorAsync(p.IdProveedor, "Administrador");
        Assert.Equal(1, await db.Proveedores.CountAsync());
        Assert.NotNull(await db.Proveedores.AsNoTracking().FirstOrDefaultAsync(x => x.IdProveedor == p.IdProveedor));
    }

    // RF-25: mensajes redactados por negocio
    [Fact]
    public async Task Mensajes_ProvienenDelNegocio_NoDeUI()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.CrearProveedorAsync(new CrearProveedorDto { Cuit = "30123456789", RazonSocial = "Logitech" });
        Assert.Equal(ResultadoCreacionProveedor.Exito, resp.Resultado);
        Assert.Equal("Proveedor Logitech registrado exitosamente.", resp.Mensaje);
    }

    // RF-26: permisos por rol
    [Theory]
    [InlineData("Vendedor", false)]
    [InlineData("Encargado", true)]
    [InlineData("Administrador", true)]
    public async Task DarBajaProveedor_PermisoPorRol(string rol, bool permitido)
    {
        await using var db = await CrearContextoVacioAsync();
        var p = await SembrarProveedorAsync(db, "30123456789", "Logitech");
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.DarBajaProveedorAsync(p.IdProveedor, rol);
        if (permitido)
            Assert.Equal(ResultadoBajaProveedor.Exito, resp.Resultado);
        else
            Assert.Equal(ResultadoBajaProveedor.PermisoDenegado, resp.Resultado);
    }

    [Theory]
    [InlineData("Vendedor", false)]
    [InlineData("Encargado", true)]
    public async Task ReactivarProveedor_PermisoPorRol(string rol, bool permitido)
    {
        await using var db = await CrearContextoVacioAsync();
        var p = await SembrarProveedorAsync(db, "30123456789", "Logitech", estado: false);
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.ReactivarProveedorAsync(p.IdProveedor, rol);
        if (permitido)
            Assert.Equal(ResultadoActivacionProveedor.Exito, resp.Resultado);
        else
            Assert.Equal(ResultadoActivacionProveedor.PermisoDenegado, resp.Resultado);
    }

    // RF-27: fallo del origen de datos — se simula con contexto que lanza DbUpdateException
    private class FalloDbContext : AppDbContext
    {
        public FalloDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new DbUpdateException("Simulado");
        }
    }

    [Fact]
    public async Task CrearProveedor_FalloPersistencia_DevuelveErrorPersistencia()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"Fallo-{Guid.NewGuid()}")
            .Options;
        var db = new FalloDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var negocio = new ProveedorNegocio(db);
        var resp = await negocio.CrearProveedorAsync(new CrearProveedorDto { Cuit = "30123456789", RazonSocial = "Logitech" });
        Assert.Equal(ResultadoCreacionProveedor.ErrorPersistencia, resp.Resultado);
        Assert.Equal(MensajeErrorPersistencia, resp.Mensaje);
    }
}
