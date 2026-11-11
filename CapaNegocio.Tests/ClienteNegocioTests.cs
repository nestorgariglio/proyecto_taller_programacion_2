using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using CapaNegocio.DTOs.Clientes;
using CapaNegocio.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CapaNegocio.Tests;

public class ClienteNegocioTests
{
    private const string MensajeDocumentoObligatorio = "El documento es obligatorio.";
    private const string MensajeNombreObligatorio = "El nombre es obligatorio.";
    private const string MensajeApellidoObligatorio = "El apellido es obligatorio.";
    private const string MensajeDocumentoFormato = "El documento debe tener entre 7 y 20 caracteres y solo admite letras sin tilde y dígitos.";
    private const string MensajeNombreFormato = "El nombre debe tener entre 2 y 100 caracteres y solo admite letras, espacios, apóstrofo y guion.";
    private const string MensajeApellidoFormato = "El apellido debe tener entre 2 y 100 caracteres y solo admite letras, espacios, apóstrofo y guion.";
    private const string MensajeCorreoFormato = "El correo no tiene un formato válido (ejemplo: nombre@dominio.com).";
    private const string MensajeTelefonoFormato = "El teléfono solo admite dígitos (entre 6 y 15).";
    private const string MensajeSinResultados = "No se encontraron clientes que coincidan con la búsqueda.";
    private const string MensajeSinClientesEnElFiltro = "No hay clientes para mostrar con el filtro seleccionado.";
    private const string MensajeClienteNoEncontrado = "El cliente indicado no existe.";
    private const string MensajeClienteInvalido = "Debe indicar un cliente válido.";
    private const string MensajePermisoDenegadoBaja = "No tiene permisos para dar de baja clientes.";
    private const string MensajePermisoDenegadoReactivacion = "No tiene permisos para reactivar clientes.";
    private const string MensajeErrorPersistencia =
        "No se pudo completar la operación por un problema con el origen de datos. Intente nuevamente.";

    // RF-1: alta válida.
    [Fact]
    public async Task CrearCliente_AltaValida_PersisteActivoYDevuelveNombreCompleto()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "30123456",
            Nombre = "Juan",
            Apellido = "Pérez",
            Correo = "juan.perez@mail.com",
            Telefono = "3624112233"
        });

        Assert.Equal(ResultadoCreacionCliente.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Cliente);
        Assert.True(respuesta.Cliente.Estado);
        Assert.Equal("30123456", respuesta.Cliente.Documento);
        Assert.Equal("Juan Pérez", respuesta.Cliente.NombreCompleto);
        Assert.Equal("juan.perez@mail.com", respuesta.Cliente.Correo);
        Assert.Equal("3624112233", respuesta.Cliente.Telefono);
        Assert.Equal("Cliente Juan Pérez registrado exitosamente.", respuesta.Mensaje);

        Cliente? guardado = await db.Clientes
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.IdCliente == respuesta.Cliente.IdCliente);

        Assert.NotNull(guardado);
        Assert.True(guardado.Estado);
        Assert.Equal("30123456", guardado.Dni);
        Assert.Equal("Juan", guardado.Nombre);
        Assert.Equal("Pérez", guardado.Apellido);
        Assert.NotEqual(default, guardado.FechaRegistro);
        Assert.Equal(1, await db.Clientes.CountAsync());
    }

    // RF-1: el alta nace activa aunque no se informe estado (el DTO no lo lleva).
    [Fact]
    public async Task CrearCliente_SinCorreoNiTelefono_LosGuardaComoNulos()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "AB1234567",
            Nombre = "María José",
            Apellido = "O'Connor"
        });

        Assert.Equal(ResultadoCreacionCliente.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Cliente);
        Assert.Null(respuesta.Cliente.Correo);
        Assert.Null(respuesta.Cliente.Telefono);
        Assert.Equal("María José O'Connor", respuesta.Cliente.NombreCompleto);

        Cliente guardado = await db.Clientes.AsNoTracking().SingleAsync();
        Assert.Null(guardado.Correo);
        Assert.Null(guardado.Telefono);
    }

    // RF-2: campos obligatorios ausentes, vacíos o solo espacios.
    [Theory]
    [InlineData(null, "Juan", "Pérez", MensajeDocumentoObligatorio)]
    [InlineData("", "Juan", "Pérez", MensajeDocumentoObligatorio)]
    [InlineData("   ", "Juan", "Pérez", MensajeDocumentoObligatorio)]
    [InlineData("\t\n  \t", "Juan", "Pérez", MensajeDocumentoObligatorio)]
    [InlineData("30123456", null, "Pérez", MensajeNombreObligatorio)]
    [InlineData("30123456", "", "Pérez", MensajeNombreObligatorio)]
    [InlineData("30123456", "   ", "Pérez", MensajeNombreObligatorio)]
    [InlineData("30123456", "\t \n", "Pérez", MensajeNombreObligatorio)]
    [InlineData("30123456", "Juan", null, MensajeApellidoObligatorio)]
    [InlineData("30123456", "Juan", "", MensajeApellidoObligatorio)]
    [InlineData("30123456", "Juan", "   ", MensajeApellidoObligatorio)]
    [InlineData("30123456", "Juan", "\r\n\t", MensajeApellidoObligatorio)]
    public async Task CrearCliente_CampoObligatorioNoInformado_RechazaSinPersistir(
        string? documento, string? nombre, string? apellido, string mensajeEsperado)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = documento,
            Nombre = nombre,
            Apellido = apellido
        });

        Assert.Equal(ResultadoCreacionCliente.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Cliente);
        Assert.Equal(mensajeEsperado, respuesta.Mensaje);
        Assert.Empty(await db.Clientes.AsNoTracking().ToListAsync());
    }

    // RF-3: Recorte de todos los datos antes de validar y guardar.
    [Fact]
    public async Task CrearCliente_ConEspaciosEnLosBordes_GuardaTodosLosDatosRecortados()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "  30123456  ",
            Nombre = "\t  Juan  \n",
            Apellido = "  Pérez-López  ",
            Correo = "  juan.perez@mail.com  ",
            Telefono = "  3624112233  "
        });

        Assert.Equal(ResultadoCreacionCliente.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Cliente);
        Assert.Equal("30123456", respuesta.Cliente.Documento);
        Assert.Equal("Juan", respuesta.Cliente.Nombre);
        Assert.Equal("Pérez-López", respuesta.Cliente.Apellido);
        Assert.Equal("Juan Pérez-López", respuesta.Cliente.NombreCompleto);
        Assert.Equal("juan.perez@mail.com", respuesta.Cliente.Correo);
        Assert.Equal("3624112233", respuesta.Cliente.Telefono);

        Cliente guardado = await db.Clientes.AsNoTracking().SingleAsync();
        Assert.Equal("30123456", guardado.Dni);
        Assert.Equal("Juan", guardado.Nombre);
        Assert.Equal("Pérez-López", guardado.Apellido);
        Assert.Equal("juan.perez@mail.com", guardado.Correo);
        Assert.Equal("3624112233", guardado.Telefono);
    }

    // RF-4: documentos válidos.
    [Theory]
    [InlineData("30123456")]
    [InlineData("AB1234567")]
    [InlineData("ABCDEFG")]          // 7 caracteres
    [InlineData("12345678901234567890")] // 20 caracteres
    [InlineData("  30123456  ")]     // se recorta antes de validar
    public async Task CrearCliente_DocumentoValido_SeAcepta(string documento)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = documento,
            Nombre = "Juan",
            Apellido = "Pérez"
        });

        Assert.Equal(ResultadoCreacionCliente.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Cliente);
    }

    // RF-4: documentos inválidos de la spec.
    // Nota: el ejemplo «3012345» de la spec dice «6 caracteres», pero tiene 7 y
    // cumple `^[A-Za-z0-9]{7,20}$`; por eso el caso de longitud corta usa
    // «301234» (6 caracteres reales). Se reporta como observación de la spec.
    [Theory]
    [InlineData("301234")]                     // 6 caracteres
    [InlineData("301234567890123456789")]      // 21 caracteres
    [InlineData("30-123-456")]
    [InlineData("30 123 456")]
    [InlineData("30123456Ñ")]
    [InlineData("30123456.")]
    [InlineData("3012345$")]
    public async Task CrearCliente_DocumentoConFormatoInvalido_RechazaSinPersistir(string documento)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = documento,
            Nombre = "Juan",
            Apellido = "Pérez"
        });

        Assert.Equal(ResultadoCreacionCliente.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Cliente);
        Assert.Equal(MensajeDocumentoFormato, respuesta.Mensaje);
        Assert.Empty(await db.Clientes.AsNoTracking().ToListAsync());
    }

    // RF-5 y RF-6: nombres y apellidos válidos de la spec.
    [Theory]
    [InlineData("Juan", "Pérez")]
    [InlineData("María José", "De la Fuente")]
    [InlineData("O'Connor", "O'Neill")]
    [InlineData("Pérez-López", "Ñandú")]
    [InlineData("Ángel", "Ürsula")]
    public async Task CrearCliente_NombreYApellidoValidos_SeAceptan(string nombre, string apellido)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "30123456",
            Nombre = nombre,
            Apellido = apellido
        });

        Assert.Equal(ResultadoCreacionCliente.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Cliente);
        Assert.Equal($"{nombre} {apellido}", respuesta.Cliente.NombreCompleto);
    }

    // RF-5: nombre inválido (longitud y caracteres).
    [Theory]
    [InlineData("J")]
    [InlineData("Juan2")]
    [InlineData("Juan@")]
    [InlineData("Juan_Pérez")]
    public async Task CrearCliente_NombreInvalido_RechazaConMensajeDelCampo(string nombre)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "30123456",
            Nombre = nombre,
            Apellido = "Pérez"
        });

        Assert.Equal(ResultadoCreacionCliente.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Cliente);
        Assert.Equal(MensajeNombreFormato, respuesta.Mensaje);
        Assert.Empty(await db.Clientes.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task CrearCliente_NombreDe101Caracteres_RechazaConMensajeDelCampo()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "30123456",
            Nombre = new string('a', 101),
            Apellido = "Pérez"
        });

        Assert.Equal(ResultadoCreacionCliente.DatosInvalidos, respuesta.Resultado);
        Assert.Equal(MensajeNombreFormato, respuesta.Mensaje);
        Assert.Empty(await db.Clientes.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task CrearCliente_NombreDe100Caracteres_SeAcepta()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);
        string nombre = new string('a', 100);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "30123456",
            Nombre = nombre,
            Apellido = "Pérez"
        });

        Assert.Equal(ResultadoCreacionCliente.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Cliente);
        Assert.Equal(nombre, respuesta.Cliente.Nombre);
    }

    // RF-6: apellido inválido (longitud y caracteres).
    [Theory]
    [InlineData("P")]
    [InlineData("Pérez1")]
    [InlineData("Pérez&")]
    [InlineData("Pérez#López")]
    public async Task CrearCliente_ApellidoInvalido_RechazaConMensajeDelCampo(string apellido)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "30123456",
            Nombre = "Juan",
            Apellido = apellido
        });

        Assert.Equal(ResultadoCreacionCliente.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Cliente);
        Assert.Equal(MensajeApellidoFormato, respuesta.Mensaje);
        Assert.Empty(await db.Clientes.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task CrearCliente_ApellidoDe101Caracteres_RechazaConMensajeDelCampo()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "30123456",
            Nombre = "Juan",
            Apellido = new string('b', 101)
        });

        Assert.Equal(ResultadoCreacionCliente.DatosInvalidos, respuesta.Resultado);
        Assert.Equal(MensajeApellidoFormato, respuesta.Mensaje);
        Assert.Empty(await db.Clientes.AsNoTracking().ToListAsync());
    }

    // RF-7: correos válidos.
    [Theory]
    [InlineData("juan.perez@mail.com")]
    [InlineData("cliente1@dominio.com.ar")]
    [InlineData("  juan.perez@mail.com  ")]
    public async Task CrearCliente_CorreoValido_SeAcepta(string correo)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "30123456",
            Nombre = "Juan",
            Apellido = "Pérez",
            Correo = correo
        });

        Assert.Equal(ResultadoCreacionCliente.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Cliente);
        Assert.Equal(correo.Trim(), respuesta.Cliente.Correo);
    }

    // RF-7: correo no informado se guarda como null, sin validar formato ni unicidad.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public async Task CrearCliente_CorreoNoInformado_SeGuardaComoNulo(string? correo)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "30123456",
            Nombre = "Juan",
            Apellido = "Pérez",
            Correo = correo
        });

        Assert.Equal(ResultadoCreacionCliente.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Cliente);
        Assert.Null(respuesta.Cliente.Correo);

        Cliente guardado = await db.Clientes.AsNoTracking().SingleAsync();
        Assert.Null(guardado.Correo);
    }

    // RF-7: correos inválidos de la spec.
    [Theory]
    [InlineData("juan.perez")]
    [InlineData("@mail.com")]
    [InlineData("juan@mail")]
    [InlineData("juan@@mail.com")]
    [InlineData("juan perez@mail.com")]
    [InlineData("cliente@dominio.c")]
    [InlineData("cliente@dominio.123")]
    public async Task CrearCliente_CorreoConFormatoInvalido_RechazaSinPersistir(string correo)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "30123456",
            Nombre = "Juan",
            Apellido = "Pérez",
            Correo = correo
        });

        Assert.Equal(ResultadoCreacionCliente.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Cliente);
        Assert.Equal(MensajeCorreoFormato, respuesta.Mensaje);
        Assert.Empty(await db.Clientes.AsNoTracking().ToListAsync());
    }

    // RF-8 y RF-9: teléfonos válidos por longitud.
    [Theory]
    [InlineData("123456")]              // 6 dígitos
    [InlineData("123456789012345")]     // 15 dígitos
    [InlineData("  3624112233  ")]      // se recorta antes de validar
    public async Task CrearCliente_TelefonoValido_SeAcepta(string telefono)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "30123456",
            Nombre = "Juan",
            Apellido = "Pérez",
            Telefono = telefono
        });

        Assert.Equal(ResultadoCreacionCliente.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Cliente);
        Assert.Equal(telefono.Trim(), respuesta.Cliente.Telefono);
    }

    // RF-8 y RF-9: teléfono no informado se guarda como null.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public async Task CrearCliente_TelefonoNoInformado_SeGuardaComoNulo(string? telefono)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "30123456",
            Nombre = "Juan",
            Apellido = "Pérez",
            Telefono = telefono
        });

        Assert.Equal(ResultadoCreacionCliente.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Cliente);
        Assert.Null(respuesta.Cliente.Telefono);

        Cliente guardado = await db.Clientes.AsNoTracking().SingleAsync();
        Assert.Null(guardado.Telefono);
    }

    // RF-8 y RF-9: teléfonos inválidos de la spec (caracteres y longitud).
    [Theory]
    [InlineData("3624-112233")]
    [InlineData("3624 112233")]
    [InlineData("(03624) 411233")]
    [InlineData("+543624112233")]
    [InlineData("12345")]                // 5 dígitos
    [InlineData("1234567890123456")]     // 16 dígitos
    [InlineData("3624a112233")]
    public async Task CrearCliente_TelefonoConFormatoInvalido_RechazaSinPersistir(string telefono)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "30123456",
            Nombre = "Juan",
            Apellido = "Pérez",
            Telefono = telefono
        });

        Assert.Equal(ResultadoCreacionCliente.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Cliente);
        Assert.Equal(MensajeTelefonoFormato, respuesta.Mensaje);
        Assert.Empty(await db.Clientes.AsNoTracking().ToListAsync());
    }

    // RF-10: documento duplicado contra un cliente activo.
    [Fact]
    public async Task CrearCliente_DocumentoDuplicadoConClienteActivo_RechazaSinPersistir()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "30123456", "existente@mail.com", estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "30123456",
            Nombre = "Juan",
            Apellido = "Pérez"
        });

        Assert.Equal(ResultadoCreacionCliente.DocumentoDuplicado, respuesta.Resultado);
        Assert.Null(respuesta.Cliente);
        Assert.Equal("Ya existe un cliente con el documento \"30123456\".", respuesta.Mensaje);
        Assert.Equal(1, await db.Clientes.CountAsync());
    }

    // RF-10: documento duplicado contra un cliente inactivo, con mensaje neutral (D-4).
    [Fact]
    public async Task CrearCliente_DocumentoDuplicadoConClienteInactivo_MensajeNeutralSinSugerencias()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "30123456", null, estado: false);
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "30123456",
            Nombre = "Juan",
            Apellido = "Pérez"
        });

        Assert.Equal(ResultadoCreacionCliente.DocumentoDuplicado, respuesta.Resultado);
        Assert.Null(respuesta.Cliente);
        Assert.Equal(
            "Ya existe un cliente con el documento \"30123456\", actualmente inactivo.",
            respuesta.Mensaje);
        Assert.DoesNotContain("reactiv", respuesta.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, await db.Clientes.CountAsync());

        // El cliente existente conserva su estado: el alta no reactiva nada.
        Cliente existente = await db.Clientes.AsNoTracking().SingleAsync();
        Assert.False(existente.Estado);
    }

    // RF-10: documento duplicado solo por diferencia de mayúsculas/minúsculas.
    [Theory]
    [InlineData("ab1234567", "AB1234567")]
    [InlineData("AB1234567", "ab1234567")]
    [InlineData("Ab1234567", "aB1234567")]
    public async Task CrearCliente_DocumentoDuplicadoSoloPorCapitalizacion_Rechaza(
        string documentoExistente, string documentoNuevo)
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, documentoExistente, null, estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = documentoNuevo,
            Nombre = "Juan",
            Apellido = "Pérez"
        });

        Assert.Equal(ResultadoCreacionCliente.DocumentoDuplicado, respuesta.Resultado);
        Assert.Null(respuesta.Cliente);
        Assert.Contains(documentoNuevo, respuesta.Mensaje, StringComparison.Ordinal);
        Assert.Equal(1, await db.Clientes.CountAsync());
    }

    // RF-10: el documento se compara ya recortado contra el valor guardado.
    [Fact]
    public async Task CrearCliente_DocumentoDuplicadoConEspaciosEnLosBordes_Rechaza()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "30123456", null, estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "  30123456  ",
            Nombre = "Juan",
            Apellido = "Pérez"
        });

        Assert.Equal(ResultadoCreacionCliente.DocumentoDuplicado, respuesta.Resultado);
        Assert.Equal("Ya existe un cliente con el documento \"30123456\".", respuesta.Mensaje);
        Assert.Equal(1, await db.Clientes.CountAsync());
    }

    // RF-11: correo duplicado contra un cliente activo, inactivo o con otra capitalización.
    [Theory]
    [InlineData("existente@mail.com", "existente@mail.com", true)]
    [InlineData("existente@mail.com", "existente@mail.com", false)]
    [InlineData("Existente@Mail.com", "existente@MAIL.com", true)]
    [InlineData("existente@mail.com", "EXISTENTE@MAIL.COM", false)]
    public async Task CrearCliente_CorreoDuplicado_RechazaSinPersistir(
        string correoExistente, string correoNuevo, bool estadoExistente)
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "30123456", correoExistente, estadoExistente);
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "AB1234567",
            Nombre = "Juan",
            Apellido = "Pérez",
            Correo = correoNuevo
        });

        Assert.Equal(ResultadoCreacionCliente.CorreoDuplicado, respuesta.Resultado);
        Assert.Null(respuesta.Cliente);
        Assert.Equal($"Ya existe un cliente con el correo \"{correoNuevo}\".", respuesta.Mensaje);
        Assert.Equal(1, await db.Clientes.CountAsync());

        // El cliente existente no se modifica.
        Cliente existente = await db.Clientes.AsNoTracking().SingleAsync();
        Assert.Equal(correoExistente, existente.Correo);
        Assert.Equal(estadoExistente, existente.Estado);
    }

    // RF-11: muchos clientes pueden quedar sin correo (null, vacío o solo espacios).
    [Fact]
    public async Task CrearCliente_VariosClientesSinCorreo_SeAceptan()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        string?[] correos = { null, "", "   ", "\t\n" };

        for (int indice = 0; indice < correos.Length; indice++)
        {
            RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
            {
                Documento = $"DOC000{indice + 1}",
                Nombre = "Cliente",
                Apellido = "Sin Correo",
                Correo = correos[indice]
            });

            Assert.Equal(ResultadoCreacionCliente.Exito, respuesta.Resultado);
            Assert.NotNull(respuesta.Cliente);
            Assert.Null(respuesta.Cliente.Correo);
        }

        Assert.Equal(correos.Length, await db.Clientes.CountAsync());
        Assert.All(
            await db.Clientes.AsNoTracking().ToListAsync(),
            cliente => Assert.Null(cliente.Correo));
    }

    // RF-11: un cliente con correo y otro sin correo conviven sin conflicto.
    [Fact]
    public async Task CrearCliente_UnClienteConCorreoYOtraSinCorreo_SeAceptan()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "30123456", "existente@mail.com", estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente conCorreo = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "1111111",
            Nombre = "Ana",
            Apellido = "Gómez",
            Correo = "ana.gomez@mail.com"
        });

        RespuestaCreacionCliente sinCorreo = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "2222222",
            Nombre = "Luis",
            Apellido = "Sosa",
            Correo = "   "
        });

        Assert.Equal(ResultadoCreacionCliente.Exito, conCorreo.Resultado);
        Assert.Equal(ResultadoCreacionCliente.Exito, sinCorreo.Resultado);
        Assert.Null(sinCorreo.Cliente!.Correo);
        Assert.Equal(3, await db.Clientes.CountAsync());
    }

    // El alta evalúa primero el Documento y después el Correo.
    [Fact]
    public async Task CrearCliente_DocumentoYCorreoDuplicados_InformaPrimeroElDocumento()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "30123456", "existente@mail.com", estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "30123456",
            Nombre = "Juan",
            Apellido = "Pérez",
            Correo = "existente@mail.com"
        });

        Assert.Equal(ResultadoCreacionCliente.DocumentoDuplicado, respuesta.Resultado);
        Assert.Equal("Ya existe un cliente con el documento \"30123456\".", respuesta.Mensaje);
        Assert.Equal(1, await db.Clientes.CountAsync());
    }

    // El alta evalúa el Correo cuando el Documento es válido y único.
    [Fact]
    public async Task CrearCliente_DocumentoUnicoYCorreoDuplicado_InformaElCorreo()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "30123456", "existente@mail.com", estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "AB1234567",
            Nombre = "Juan",
            Apellido = "Pérez",
            Correo = "existente@mail.com"
        });

        Assert.Equal(ResultadoCreacionCliente.CorreoDuplicado, respuesta.Resultado);
        Assert.Equal("Ya existe un cliente con el correo \"existente@mail.com\".", respuesta.Mensaje);
        Assert.Equal(1, await db.Clientes.CountAsync());
    }

    // RF-12: editar sin cambiar Documento ni Correo no es duplicado de sí mismo.
    [Fact]
    public async Task ModificarCliente_SinCambiarDocumentoNiCorreo_SeGuardaConExito()
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente existente = await SembrarClienteAsync(db, "30123456", "existente@mail.com", estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaModificacionCliente respuesta = await negocio.ModificarClienteAsync(new ModificarClienteDto
        {
            IdCliente = existente.IdCliente,
            Documento = "30123456",
            Nombre = "Juan",
            Apellido = "Pérez",
            Correo = "existente@mail.com",
            Telefono = "3624112233"
        });

        Assert.Equal(ResultadoModificacionCliente.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Cliente);
        Assert.Equal("Juan Pérez", respuesta.Cliente.NombreCompleto);
        Assert.Equal("Cliente Juan Pérez modificado exitosamente.", respuesta.Mensaje);

        Cliente guardado = await db.Clientes.AsNoTracking().SingleAsync();
        Assert.Equal("Juan", guardado.Nombre);
        Assert.Equal("Pérez", guardado.Apellido);
        Assert.Equal("30123456", guardado.Dni);
        Assert.Equal("existente@mail.com", guardado.Correo);
        Assert.Equal("3624112233", guardado.Telefono);
    }

    // RF-12: Documento de otro cliente.
    [Fact]
    public async Task ModificarCliente_DocumentoDeOtroCliente_RechazaSinCambios()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "30123456", "otro@mail.com", estado: true);
        Cliente propio = await SembrarClienteAsync(db, "AB1234567", "propio@mail.com", estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaModificacionCliente respuesta = await negocio.ModificarClienteAsync(new ModificarClienteDto
        {
            IdCliente = propio.IdCliente,
            Documento = "30123456",
            Nombre = "Juan",
            Apellido = "Pérez",
            Correo = "propio@mail.com"
        });

        Assert.Equal(ResultadoModificacionCliente.DocumentoDuplicado, respuesta.Resultado);
        Assert.Null(respuesta.Cliente);
        Assert.Equal("Ya existe un cliente con el documento \"30123456\".", respuesta.Mensaje);
        await VerificarClienteSinCambiosAsync(db, propio.IdCliente, "AB1234567", "propio@mail.com");
    }

    // RF-12: Correo de otro cliente.
    [Fact]
    public async Task ModificarCliente_CorreoDeOtroCliente_RechazaSinCambios()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "30123456", "otro@mail.com", estado: true);
        Cliente propio = await SembrarClienteAsync(db, "AB1234567", "propio@mail.com", estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaModificacionCliente respuesta = await negocio.ModificarClienteAsync(new ModificarClienteDto
        {
            IdCliente = propio.IdCliente,
            Documento = "AB1234567",
            Nombre = "Juan",
            Apellido = "Pérez",
            Correo = "OTRO@MAIL.COM"
        });

        Assert.Equal(ResultadoModificacionCliente.CorreoDuplicado, respuesta.Resultado);
        Assert.Null(respuesta.Cliente);
        Assert.Equal("Ya existe un cliente con el correo \"OTRO@MAIL.COM\".", respuesta.Mensaje);
        await VerificarClienteSinCambiosAsync(db, propio.IdCliente, "AB1234567", "propio@mail.com");
    }

    // RF-12: el Documento y el Correo propios, con otra capitalización, no son duplicados
    // de sí mismos.
    [Fact]
    public async Task ModificarCliente_DocumentoYCorreoPropiosConOtraCapitalizacion_SeGuardan()
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente propio = await SembrarClienteAsync(db, "ab1234567", "Propio@Mail.com", estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaModificacionCliente respuesta = await negocio.ModificarClienteAsync(new ModificarClienteDto
        {
            IdCliente = propio.IdCliente,
            Documento = "AB1234567",
            Nombre = "Juan",
            Apellido = "Pérez",
            Correo = "PROPIO@MAIL.COM"
        });

        Assert.Equal(ResultadoModificacionCliente.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Cliente);
        Assert.Equal("AB1234567", respuesta.Cliente.Documento);
        Assert.Equal("PROPIO@MAIL.COM", respuesta.Cliente.Correo);

        Cliente guardado = await db.Clientes.AsNoTracking().SingleAsync();
        Assert.Equal("AB1234567", guardado.Dni);
        Assert.Equal("PROPIO@MAIL.COM", guardado.Correo);
    }

    // RF-12: mismas validaciones de obligatoriedad y formato que en el alta.
    [Theory]
    [InlineData(null, "Juan", "Pérez", "existente@mail.com", null, "El documento es obligatorio.")]
    [InlineData("30123456", "", "Pérez", "existente@mail.com", null, "El nombre es obligatorio.")]
    [InlineData("30123456", "Juan", "   ", "existente@mail.com", null, "El apellido es obligatorio.")]
    [InlineData("301234", "Juan", "Pérez", "existente@mail.com", null, MensajeDocumentoFormato)]
    [InlineData("30123456", "Juan2", "Pérez", "existente@mail.com", null, MensajeNombreFormato)]
    [InlineData("30123456", "Juan", "Pérez&", "existente@mail.com", null, MensajeApellidoFormato)]
    [InlineData("30123456", "Juan", "Pérez", "existente@", null, MensajeCorreoFormato)]
    [InlineData("30123456", "Juan", "Pérez", "existente@mail.com", "3624-112233", MensajeTelefonoFormato)]
    public async Task ModificarCliente_DatosInvalidos_RechazaSinCambios(
        string? documento, string? nombre, string? apellido, string? correo, string? telefono,
        string mensajeEsperado)
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente propio = await SembrarClienteAsync(db, "30123456", "existente@mail.com", estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaModificacionCliente respuesta = await negocio.ModificarClienteAsync(new ModificarClienteDto
        {
            IdCliente = propio.IdCliente,
            Documento = documento,
            Nombre = nombre,
            Apellido = apellido,
            Correo = correo,
            Telefono = telefono
        });

        Assert.Equal(ResultadoModificacionCliente.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Cliente);
        Assert.Equal(mensajeEsperado, respuesta.Mensaje);
        await VerificarClienteSinCambiosAsync(db, propio.IdCliente, "30123456", "existente@mail.com");
    }

    // RF-13: vaciar Correo y Teléfono los deja como no informados.
    [Theory]
    [InlineData("", "")]
    [InlineData("   ", "   ")]
    [InlineData("\t\n", "\t\n")]
    [InlineData(null, null)]
    public async Task ModificarCliente_VaciarCorreoYTelefono_QuedanNulos(string? correo, string? telefono)
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente propio = await SembrarClienteAsync(db, "30123456", "existente@mail.com", estado: true);
        propio.Telefono = "3624112233";
        await db.SaveChangesAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaModificacionCliente respuesta = await negocio.ModificarClienteAsync(new ModificarClienteDto
        {
            IdCliente = propio.IdCliente,
            Documento = "30123456",
            Nombre = "Juan",
            Apellido = "Pérez",
            Correo = correo,
            Telefono = telefono
        });

        Assert.Equal(ResultadoModificacionCliente.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Cliente);
        Assert.Null(respuesta.Cliente.Correo);
        Assert.Null(respuesta.Cliente.Telefono);

        Cliente guardado = await db.Clientes.AsNoTracking().SingleAsync();
        Assert.Null(guardado.Correo);
        Assert.Null(guardado.Telefono);
    }

    // RF-14: editar un cliente inactivo no exige reactivarlo y conserva su estado.
    [Fact]
    public async Task ModificarCliente_ClienteInactivo_SeGuardaYSigueInactivo()
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente inactivo = await SembrarClienteAsync(db, "30123456", "existente@mail.com", estado: false);
        var negocio = new ClienteNegocio(db);

        RespuestaModificacionCliente respuesta = await negocio.ModificarClienteAsync(new ModificarClienteDto
        {
            IdCliente = inactivo.IdCliente,
            Documento = "30123456",
            Nombre = "Juan",
            Apellido = "Pérez",
            Correo = "existente@mail.com"
        });

        Assert.Equal(ResultadoModificacionCliente.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Cliente);
        Assert.False(respuesta.Cliente.Estado);

        Cliente guardado = await db.Clientes.AsNoTracking().SingleAsync();
        Assert.False(guardado.Estado);
        Assert.Equal("Juan", guardado.Nombre);
    }

    // RF-15: la edición conserva el estado, la fecha de registro y las ventas asociadas.
    [Fact]
    public async Task ModificarCliente_ConVentaAsociada_ConservaVentaEstadoYFechaRegistro()
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente propio = await SembrarClienteAsync(db, "30123456", "existente@mail.com", estado: true);

        DateTime fechaRegistroOriginal = new(2026, 1, 15, 10, 30, 0, DateTimeKind.Unspecified);
        propio.FechaRegistro = fechaRegistroOriginal;

        var venta = new Venta
        {
            IdCliente = propio.IdCliente,
            IdTipo = 1,
            MontoTotal = 1500m,
            MontoPago = 2000m,
            MontoCambio = 500m,
            Estado = true
        };
        db.Ventas.Add(venta);
        await db.SaveChangesAsync();

        var negocio = new ClienteNegocio(db);

        RespuestaModificacionCliente respuesta = await negocio.ModificarClienteAsync(new ModificarClienteDto
        {
            IdCliente = propio.IdCliente,
            Documento = "30123456",
            Nombre = "Juan Carlos",
            Apellido = "Pérez",
            Correo = "nuevo@mail.com"
        });

        Assert.Equal(ResultadoModificacionCliente.Exito, respuesta.Resultado);

        Cliente guardado = await db.Clientes.AsNoTracking().SingleAsync();
        Assert.True(guardado.Estado);
        Assert.Equal(fechaRegistroOriginal, guardado.FechaRegistro);
        Assert.Equal("nuevo@mail.com", guardado.Correo);

        Venta ventaGuardada = await db.Ventas.AsNoTracking().SingleAsync();
        Assert.Equal(propio.IdCliente, ventaGuardada.IdCliente);
        Assert.True(ventaGuardada.Estado);
        Assert.Equal(1, await db.Ventas.CountAsync());
    }

    // El cliente indicado no existe.
    [Fact]
    public async Task ModificarCliente_IdInexistente_DevuelveClienteNoEncontrado()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaModificacionCliente respuesta = await negocio.ModificarClienteAsync(new ModificarClienteDto
        {
            IdCliente = 999,
            Documento = "30123456",
            Nombre = "Juan",
            Apellido = "Pérez"
        });

        Assert.Equal(ResultadoModificacionCliente.ClienteNoEncontrado, respuesta.Resultado);
        Assert.Null(respuesta.Cliente);
        Assert.Equal("El cliente indicado no existe.", respuesta.Mensaje);
        Assert.Equal(0, await db.Clientes.CountAsync());
    }

    // Identificador no válido.
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ModificarCliente_IdInvalido_DevuelveDatosInvalidos(int idCliente)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaModificacionCliente respuesta = await negocio.ModificarClienteAsync(new ModificarClienteDto
        {
            IdCliente = idCliente,
            Documento = "30123456",
            Nombre = "Juan",
            Apellido = "Pérez"
        });

        Assert.Equal(ResultadoModificacionCliente.DatosInvalidos, respuesta.Resultado);
        Assert.Null(respuesta.Cliente);
        Assert.Equal("Debe indicar un cliente válido.", respuesta.Mensaje);
    }

    // RF-3 y RF-12: la edición también recorta todos los datos.
    [Fact]
    public async Task ModificarCliente_ConEspaciosEnLosBordes_GuardaRecortado()
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente propio = await SembrarClienteAsync(db, "30123456", "existente@mail.com", estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaModificacionCliente respuesta = await negocio.ModificarClienteAsync(new ModificarClienteDto
        {
            IdCliente = propio.IdCliente,
            Documento = "  30123456  ",
            Nombre = "  Juan  ",
            Apellido = "\tPérez-López\n",
            Correo = "  juan.perez@mail.com  ",
            Telefono = "  3624112233  "
        });

        Assert.Equal(ResultadoModificacionCliente.Exito, respuesta.Resultado);
        Assert.NotNull(respuesta.Cliente);
        Assert.Equal("30123456", respuesta.Cliente.Documento);
        Assert.Equal("Juan Pérez-López", respuesta.Cliente.NombreCompleto);
        Assert.Equal("juan.perez@mail.com", respuesta.Cliente.Correo);
        Assert.Equal("3624112233", respuesta.Cliente.Telefono);
    }

    // RF-16: filtro Activos devuelve solo activos y expone Documento, Nombre completo y Estado.
    [Fact]
    public async Task ListarClientes_FiltroActivos_DevuelveSoloActivos()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        await SembrarClienteAsync(db, "DOC0002", "Beto", "Sosa", null, estado: false);
        var negocio = new ClienteNegocio(db);

        RespuestaListadoClientes respuesta = await negocio.ListarClientesAsync(
            FiltroEstadoCliente.Activos, "DOC");

        Assert.Equal(ResultadoListadoClientes.ConResultados, respuesta.Resultado);
        Assert.Single(respuesta.Clientes);
        Assert.Equal("DOC0001", respuesta.Clientes[0].Documento);
        Assert.Equal("Ana Gómez", respuesta.Clientes[0].NombreCompleto);
        Assert.True(respuesta.Clientes[0].Estado);
        Assert.False(string.IsNullOrWhiteSpace(respuesta.Mensaje));
    }

    // RF-16: filtro Inactivos devuelve solo inactivos.
    [Fact]
    public async Task ListarClientes_FiltroInactivos_DevuelveSoloInactivos()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        await SembrarClienteAsync(db, "DOC0002", "Beto", "Sosa", null, estado: false);
        var negocio = new ClienteNegocio(db);

        RespuestaListadoClientes respuesta = await negocio.ListarClientesAsync(
            FiltroEstadoCliente.Inactivos, "DOC");

        Assert.Equal(ResultadoListadoClientes.ConResultados, respuesta.Resultado);
        Assert.Single(respuesta.Clientes);
        Assert.Equal("DOC0002", respuesta.Clientes[0].Documento);
        Assert.Equal("Beto Sosa", respuesta.Clientes[0].NombreCompleto);
        Assert.False(respuesta.Clientes[0].Estado);
    }

    // RF-17: coincidencia parcial por Documento.
    [Fact]
    public async Task ListarClientes_BusquedaParcialPorDocumento_DevuelveCoincidencias()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "30123456", "Ana", "Gómez", null, estado: true);
        await SembrarClienteAsync(db, "AB1234567", "Beto", "Sosa", null, estado: true);
        await SembrarClienteAsync(db, "9999999", "Carla", "Díaz", null, estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaListadoClientes respuesta = await negocio.ListarClientesAsync(
            FiltroEstadoCliente.Activos, "123456");

        Assert.Equal(ResultadoListadoClientes.ConResultados, respuesta.Resultado);
        Assert.Equal(2, respuesta.Clientes.Count);
        Assert.Contains(respuesta.Clientes, c => c.Documento == "30123456");
        Assert.Contains(respuesta.Clientes, c => c.Documento == "AB1234567");
    }

    // RF-17: coincidencia parcial por Nombre y por Apellido, sin distinguir mayúsculas.
    [Theory]
    [InlineData("jua")]
    [InlineData("JUA")]
    [InlineData("maria")]
    [InlineData("sosa")]
    [InlineData("SOSA")]
    public async Task ListarClientes_BusquedaParcialPorNombreOApellido_IgnoraMayusculas(string texto)
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "DOC0001", "Juan", "Pérez", null, estado: true);
        await SembrarClienteAsync(db, "DOC0002", "Maria", "Sosa", null, estado: true);
        await SembrarClienteAsync(db, "DOC0003", "Carla", "Díaz", null, estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaListadoClientes respuesta = await negocio.ListarClientesAsync(
            FiltroEstadoCliente.Activos, texto);

        Assert.Equal(ResultadoListadoClientes.ConResultados, respuesta.Resultado);
        Assert.Single(respuesta.Clientes);
    }

    // RF-17: la búsqueda respeta el filtro de estado.
    [Fact]
    public async Task ListarClientes_BusquedaRespetaElFiltroDeEstado()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "DOC0001", "Juan", "Pérez", null, estado: true);
        await SembrarClienteAsync(db, "DOC0002", "Juancho", "Pérez", null, estado: false);
        var negocio = new ClienteNegocio(db);

        RespuestaListadoClientes activos = await negocio.ListarClientesAsync(
            FiltroEstadoCliente.Activos, "juan");

        RespuestaListadoClientes inactivos = await negocio.ListarClientesAsync(
            FiltroEstadoCliente.Inactivos, "juan");

        Assert.Equal(ResultadoListadoClientes.ConResultados, activos.Resultado);
        Assert.Single(activos.Clientes);
        Assert.Equal("DOC0001", activos.Clientes[0].Documento);

        Assert.Equal(ResultadoListadoClientes.ConResultados, inactivos.Resultado);
        Assert.Single(inactivos.Clientes);
        Assert.Equal("DOC0002", inactivos.Clientes[0].Documento);
    }

    // D-12: con el buscador vacío, el padrón no es una búsqueda: lista todos los
    // clientes del filtro Activos y ninguno inactivo.
    [Fact]
    public async Task ListarClientes_BuscadorVacio_FiltroActivos_DevuelveTodosLosActivos()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "DOC0003", "Carla", "Díaz", null, estado: true);
        await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        await SembrarClienteAsync(db, "DOC0002", "Beto", "Sosa", null, estado: true);
        await SembrarClienteAsync(db, "DOC0004", "Inés", "Paz", null, estado: false);
        await SembrarClienteAsync(db, "DOC0005", "Julián", "Rey", null, estado: false);
        var negocio = new ClienteNegocio(db);

        RespuestaListadoClientes respuesta = await negocio.ListarClientesAsync(
            FiltroEstadoCliente.Activos, "");

        Assert.Equal(ResultadoListadoClientes.ConResultados, respuesta.Resultado);
        Assert.Equal(3, respuesta.Clientes.Count);
        Assert.All(respuesta.Clientes, cliente => Assert.True(cliente.Estado));
        Assert.Equal(
            new[] { "Ana Gómez", "Beto Sosa", "Carla Díaz" },
            respuesta.Clientes.Select(cliente => cliente.NombreCompleto).ToArray());
        Assert.NotEqual(MensajeSinResultados, respuesta.Mensaje);
        Assert.False(string.IsNullOrWhiteSpace(respuesta.Mensaje));
    }

    // D-12: con el buscador vacío, el filtro Inactivos lista todos los inactivos.
    [Fact]
    public async Task ListarClientes_BuscadorVacio_FiltroInactivos_DevuelveTodosLosInactivos()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        await SembrarClienteAsync(db, "DOC0002", "Beto", "Sosa", null, estado: false);
        await SembrarClienteAsync(db, "DOC0003", "Carla", "Díaz", null, estado: false);
        var negocio = new ClienteNegocio(db);

        RespuestaListadoClientes respuesta = await negocio.ListarClientesAsync(
            FiltroEstadoCliente.Inactivos, "");

        Assert.Equal(ResultadoListadoClientes.ConResultados, respuesta.Resultado);
        Assert.Equal(2, respuesta.Clientes.Count);
        Assert.All(respuesta.Clientes, cliente => Assert.False(cliente.Estado));
        Assert.Equal(
            new[] { "Beto Sosa", "Carla Díaz" },
            respuesta.Clientes.Select(cliente => cliente.NombreCompleto).ToArray());
    }

    // D-12: null, vacío y solo espacios se comportan igual (no aplican filtro de texto).
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public async Task ListarClientes_BuscadorVacioOSoloEspacios_DevuelveConResultados(string? texto)
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        await SembrarClienteAsync(db, "DOC0002", "Beto", "Sosa", null, estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaListadoClientes respuesta = await negocio.ListarClientesAsync(
            FiltroEstadoCliente.Activos, texto);

        Assert.Equal(ResultadoListadoClientes.ConResultados, respuesta.Resultado);
        Assert.Equal(2, respuesta.Clientes.Count);
        Assert.NotEqual(MensajeSinResultados, respuesta.Mensaje);
    }

    // D-12: padrón sin clientes en el filtro elegido → lista vacía con aviso propio,
    // sin usar el mensaje de «sin resultados» de las búsquedas.
    [Fact]
    public async Task ListarClientes_BuscadorVacio_SinClientesEnElFiltro_DevuelveConResultadosVacio()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: false);
        var negocio = new ClienteNegocio(db);

        RespuestaListadoClientes respuesta = await negocio.ListarClientesAsync(
            FiltroEstadoCliente.Activos, "   ");

        Assert.Equal(ResultadoListadoClientes.ConResultados, respuesta.Resultado);
        Assert.Empty(respuesta.Clientes);
        Assert.Equal(MensajeSinClientesEnElFiltro, respuesta.Mensaje);
    }

    // RF-17: el texto de búsqueda se recorta antes de consultar.
    [Fact]
    public async Task ListarClientes_TextoConEspaciosEnLosBordes_SeRecorta()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaListadoClientes respuesta = await negocio.ListarClientesAsync(
            FiltroEstadoCliente.Activos, "\t  gómez  \n");

        Assert.Equal(ResultadoListadoClientes.ConResultados, respuesta.Resultado);
        Assert.Single(respuesta.Clientes);
        Assert.Equal("Ana Gómez", respuesta.Clientes[0].NombreCompleto);
    }

    // RF-17: las tildes no son requisito de negocio; se exige coincidencia exacta de
    // caracteres (la insensibilidad a tildes depende de la colación del origen).
    [Fact]
    public async Task ListarClientes_BusquedaConTildesYEnie_ExigeCaracteresExactos()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "DOC0001", "Ñandú", "Pérez", null, estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaListadoClientes conEnie = await negocio.ListarClientesAsync(
            FiltroEstadoCliente.Activos, "Ñand");

        RespuestaListadoClientes sinEnie = await negocio.ListarClientesAsync(
            FiltroEstadoCliente.Activos, "Nand");

        Assert.Equal(ResultadoListadoClientes.ConResultados, conEnie.Resultado);
        Assert.Single(conEnie.Clientes);
        Assert.Equal(ResultadoListadoClientes.SinResultados, sinEnie.Resultado);
        Assert.Empty(sinEnie.Clientes);
    }

    // RF-17: dos clientes con el mismo Nombre completo se devuelven ambos y se
    // distinguen por Documento.
    [Fact]
    public async Task ListarClientes_DosClientesConMismoNombreCompleto_DevuelveAmbos()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "DOC0001", "Juan", "Pérez", null, estado: true);
        await SembrarClienteAsync(db, "DOC0002", "Juan", "Pérez", null, estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaListadoClientes respuesta = await negocio.ListarClientesAsync(
            FiltroEstadoCliente.Activos, "juan pérez");

        Assert.Equal(ResultadoListadoClientes.ConResultados, respuesta.Resultado);
        Assert.Equal(2, respuesta.Clientes.Count);
        Assert.All(respuesta.Clientes, c => Assert.Equal("Juan Pérez", c.NombreCompleto));
        Assert.Equal(
            new[] { "DOC0001", "DOC0002" },
            respuesta.Clientes.Select(c => c.Documento).OrderBy(d => d).ToArray());
    }

    // RF-19: sin coincidencias en el padrón → SinResultados con mensaje del negocio.
    [Fact]
    public async Task ListarClientes_SinCoincidencias_DevuelveSinResultadosConMensaje()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaListadoClientes respuesta = await negocio.ListarClientesAsync(
            FiltroEstadoCliente.Activos, "inexistente");

        Assert.Equal(ResultadoListadoClientes.SinResultados, respuesta.Resultado);
        Assert.Empty(respuesta.Clientes);
        Assert.False(string.IsNullOrWhiteSpace(respuesta.Mensaje));
        Assert.Equal(MensajeSinResultados, respuesta.Mensaje);
    }

    // RF-18: la búsqueda del POS devuelve solo clientes activos.
    [Fact]
    public async Task BuscarClientesParaPos_SoloDevuelveActivos()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        await SembrarClienteAsync(db, "DOC0002", "Ana María", "Gómez", null, estado: false);
        var negocio = new ClienteNegocio(db);

        RespuestaListadoClientes respuesta = await negocio.BuscarClientesParaPosAsync("ana");

        Assert.Equal(ResultadoListadoClientes.ConResultados, respuesta.Resultado);
        Assert.Single(respuesta.Clientes);
        Assert.Equal("DOC0001", respuesta.Clientes[0].Documento);
        Assert.True(respuesta.Clientes[0].Estado);
    }

    // RF-18: con 25 coincidencias devuelve 20, ordenadas por Nombre completo ascendente.
    [Fact]
    public async Task BuscarClientesParaPos_Con25Coincidencias_Devuelve20Ordenadas()
    {
        await using var db = await CrearContextoVacioAsync();

        // Se siembran en orden inverso para que el orden no dependa de la inserción.
        for (int indice = 24; indice >= 0; indice--)
        {
            char letra = (char)('A' + indice);
            await SembrarClienteParaPosAsync(db, letra);
        }

        var negocio = new ClienteNegocio(db);

        RespuestaListadoClientes respuesta = await negocio.BuscarClientesParaPosAsync("cliente");

        Assert.Equal(ResultadoListadoClientes.ConResultados, respuesta.Resultado);
        Assert.Equal(20, respuesta.Clientes.Count);

        string[] nombresEsperados = Enumerable.Range(0, 20)
            .Select(indice => $"{(char)('A' + indice)} Cliente")
            .ToArray();

        Assert.Equal(nombresEsperados, respuesta.Clientes.Select(c => c.NombreCompleto).ToArray());
        Assert.Equal("A Cliente", respuesta.Clientes[0].NombreCompleto);
        Assert.Equal("T Cliente", respuesta.Clientes[19].NombreCompleto);
    }

    // B2 (RF-18, D-7): el orden es por Nombre completo, no por Nombre suelto.
    [Fact]
    public async Task BuscarClientesParaPos_OrdenaPorNombreCompletoYNoPorNombreSuelto()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "DOC0001", "Ana", "Zeta", null, estado: true);
        await SembrarClienteAsync(db, "DOC0002", "Ana María", "Benítez", null, estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaListadoClientes respuesta = await negocio.BuscarClientesParaPosAsync("Ana");

        Assert.Equal(ResultadoListadoClientes.ConResultados, respuesta.Resultado);
        Assert.Equal(2, respuesta.Clientes.Count);
        Assert.Equal("Ana María Benítez", respuesta.Clientes[0].NombreCompleto);
        Assert.Equal("Ana Zeta", respuesta.Clientes[1].NombreCompleto);
    }

    // B2 (RF-18, D-7): el tope de 20 se aplica sobre el orden por Nombre completo.
    [Fact]
    public async Task BuscarClientesParaPos_Con25ClientesNumerados_DevuelveLos20PrimerosPorNombreCompleto()
    {
        await using var db = await CrearContextoVacioAsync();

        // Se siembran en orden inverso para que el orden no dependa de la inserción.
        for (int numero = 25; numero >= 1; numero--)
        {
            await SembrarClienteAsync(
                db, $"DOC{numero:0000}", $"Cliente {numero:00}", "Prueba", null, estado: true);
        }

        var negocio = new ClienteNegocio(db);

        RespuestaListadoClientes respuesta = await negocio.BuscarClientesParaPosAsync("Cliente");

        Assert.Equal(ResultadoListadoClientes.ConResultados, respuesta.Resultado);
        Assert.Equal(20, respuesta.Clientes.Count);
        Assert.Equal("Cliente 01 Prueba", respuesta.Clientes[0].NombreCompleto);
        Assert.Equal("Cliente 20 Prueba", respuesta.Clientes[19].NombreCompleto);

        string[] fueraDelTope = Enumerable.Range(21, 5)
            .Select(numero => $"Cliente {numero:00} Prueba")
            .ToArray();

        Assert.All(
            fueraDelTope,
            nombre => Assert.DoesNotContain(
                respuesta.Clientes,
                cliente => cliente.NombreCompleto == nombre));
    }

    // RF-18: texto vacío o solo espacios devuelve lista vacía, sin consultar la base.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public async Task BuscarClientesParaPos_TextoVacioOSoloEspacios_DevuelveSinResultados(string? texto)
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaListadoClientes respuesta = await negocio.BuscarClientesParaPosAsync(texto);

        Assert.Equal(ResultadoListadoClientes.SinResultados, respuesta.Resultado);
        Assert.Empty(respuesta.Clientes);
        Assert.Equal(MensajeSinResultados, respuesta.Mensaje);
    }

    // RF-19: sin coincidencias en la búsqueda del POS → el mismo mensaje del negocio.
    [Fact]
    public async Task BuscarClientesParaPos_SinCoincidencias_DevuelveSinResultadosConMensaje()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaListadoClientes respuesta = await negocio.BuscarClientesParaPosAsync("inexistente");

        Assert.Equal(ResultadoListadoClientes.SinResultados, respuesta.Resultado);
        Assert.Empty(respuesta.Clientes);
        Assert.Equal(MensajeSinResultados, respuesta.Mensaje);
    }

    // RF-18: la búsqueda del POS también encuentra por Documento.
    [Fact]
    public async Task BuscarClientesParaPos_PorDocumento_DevuelveElCliente()
    {
        await using var db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "30123456", "Ana", "Gómez", null, estado: true);
        await SembrarClienteAsync(db, "AB1234567", "Beto", "Sosa", null, estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaListadoClientes respuesta = await negocio.BuscarClientesParaPosAsync("3012");

        Assert.Equal(ResultadoListadoClientes.ConResultados, respuesta.Resultado);
        Assert.Single(respuesta.Clientes);
        Assert.Equal("30123456", respuesta.Clientes[0].Documento);
    }

    // RF-21: confirmación única sin ventas → texto de confirmación simple.
    [Fact]
    public async Task ObtenerConfirmacionBaja_SinVentas_DevuelveConfirmacionSimple()
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente cliente = await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaConfirmacionBajaCliente respuesta =
            await negocio.ObtenerConfirmacionBajaClienteAsync(cliente.IdCliente, "Administrador");

        Assert.Equal(ResultadoConfirmacionBajaCliente.ConfirmacionRequerida, respuesta.Resultado);
        Assert.Equal(0, respuesta.VentasAsociadas);
        Assert.Equal("¿Desea dar de baja al cliente Ana Gómez?", respuesta.Mensaje);
    }

    // RF-22/D-11: la cantidad informada incluye las ventas anuladas (Estado = false).
    [Fact]
    public async Task ObtenerConfirmacionBaja_ConVentas_IncluyeLasAnuladas()
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente cliente = await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        await SembrarVentaAsync(db, cliente.IdCliente, estado: true);
        await SembrarVentaAsync(db, cliente.IdCliente, estado: true);
        await SembrarVentaAsync(db, cliente.IdCliente, estado: false);
        var negocio = new ClienteNegocio(db);

        RespuestaConfirmacionBajaCliente respuesta =
            await negocio.ObtenerConfirmacionBajaClienteAsync(cliente.IdCliente, "Encargado");

        Assert.Equal(ResultadoConfirmacionBajaCliente.ConfirmacionRequerida, respuesta.Resultado);
        Assert.Equal(3, respuesta.VentasAsociadas);
        Assert.Equal(
            "El cliente Ana Gómez tiene 3 ventas asociadas. ¿Desea darle de baja de todos modos?",
            respuesta.Mensaje);
    }

    // RF-22: con una sola venta el mensaje se redacta en singular.
    [Fact]
    public async Task ObtenerConfirmacionBaja_ConUnaVenta_InformaEnSingular()
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente cliente = await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        await SembrarVentaAsync(db, cliente.IdCliente, estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaConfirmacionBajaCliente respuesta =
            await negocio.ObtenerConfirmacionBajaClienteAsync(cliente.IdCliente, "Administrador");

        Assert.Equal(1, respuesta.VentasAsociadas);
        Assert.Equal(
            "El cliente Ana Gómez tiene 1 venta asociada. ¿Desea darle de baja de todos modos?",
            respuesta.Mensaje);
    }

    // RF-21: la confirmación no modifica el cliente.
    [Fact]
    public async Task ObtenerConfirmacionBaja_NoModificaElCliente()
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente cliente = await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        await SembrarVentaAsync(db, cliente.IdCliente, estado: true);
        var negocio = new ClienteNegocio(db);

        await negocio.ObtenerConfirmacionBajaClienteAsync(cliente.IdCliente, "Administrador");

        Assert.Equal(1, await db.Clientes.CountAsync());
        Cliente guardado = await db.Clientes
            .AsNoTracking()
            .SingleAsync(c => c.IdCliente == cliente.IdCliente);
        Assert.True(guardado.Estado);
    }

    // D-11: cuenta todas las ventas del cliente (vigentes y anuladas) y solo las suyas.
    [Fact]
    public async Task ContarVentasCliente_CuentaVigentesYAnuladasSoloDelCliente()
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente clienteConVentas = await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        Cliente otroCliente = await SembrarClienteAsync(db, "DOC0002", "Beto", "Sosa", null, estado: true);
        await SembrarVentaAsync(db, clienteConVentas.IdCliente, estado: true);
        await SembrarVentaAsync(db, clienteConVentas.IdCliente, estado: true);
        await SembrarVentaAsync(db, clienteConVentas.IdCliente, estado: false);
        await SembrarVentaAsync(db, otroCliente.IdCliente, estado: true);
        var negocio = new ClienteNegocio(db);

        Assert.Equal(3, await negocio.ContarVentasClienteAsync(clienteConVentas.IdCliente));
        Assert.Equal(1, await negocio.ContarVentasClienteAsync(otroCliente.IdCliente));
    }

    // RF-21/RF-25: baja lógica sobre la misma fila, sin borrar el registro.
    [Fact]
    public async Task DarBaja_ClienteActivo_MarcaInactivoSinBorrarLaFila()
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente cliente = await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        int idCliente = cliente.IdCliente;
        int filasAntes = await db.Clientes.CountAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaBajaCliente respuesta = await negocio.DarBajaClienteAsync(idCliente, "Administrador");

        Assert.Equal(ResultadoBajaCliente.Exito, respuesta.Resultado);
        Assert.Equal("Cliente Ana Gómez dado de baja exitosamente.", respuesta.Mensaje);
        Assert.NotNull(respuesta.Cliente);
        Assert.Equal(idCliente, respuesta.Cliente!.IdCliente);
        Assert.False(respuesta.Cliente.Estado);

        Assert.Equal(filasAntes, await db.Clientes.CountAsync());
        Cliente guardado = await db.Clientes
            .AsNoTracking()
            .SingleAsync(c => c.IdCliente == idCliente);
        Assert.False(guardado.Estado);
        Assert.Equal("DOC0001", guardado.Dni);
        Assert.Equal("Ana", guardado.Nombre);
        Assert.Equal("Gómez", guardado.Apellido);
    }

    // RF-22: tras la baja, las ventas históricas siguen existiendo intactas.
    [Fact]
    public async Task DarBaja_ClienteConVentas_ConservaLasVentasHistoricas()
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente cliente = await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        await SembrarVentaAsync(db, cliente.IdCliente, estado: true);
        await SembrarVentaAsync(db, cliente.IdCliente, estado: false);
        int ventasAntes = await db.Ventas.CountAsync();
        var negocio = new ClienteNegocio(db);

        await negocio.DarBajaClienteAsync(cliente.IdCliente, "Administrador");

        Assert.Equal(ventasAntes, await db.Ventas.CountAsync());
        Assert.Equal(
            2,
            await db.Ventas.AsNoTracking().CountAsync(v => v.IdCliente == cliente.IdCliente));
    }

    // RF-23: reactivación sobre la misma fila, sin duplicar.
    [Fact]
    public async Task Reactivar_ClienteInactivo_MarcaActivoSobreLaMismaFila()
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente cliente = await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: false);
        int idCliente = cliente.IdCliente;
        int filasAntes = await db.Clientes.CountAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaActivacionCliente respuesta = await negocio.ReactivarClienteAsync(idCliente, "Encargado");

        Assert.Equal(ResultadoActivacionCliente.Exito, respuesta.Resultado);
        Assert.Equal("Cliente Ana Gómez reactivado exitosamente.", respuesta.Mensaje);
        Assert.NotNull(respuesta.Cliente);
        Assert.Equal(idCliente, respuesta.Cliente!.IdCliente);
        Assert.True(respuesta.Cliente.Estado);

        Assert.Equal(filasAntes, await db.Clientes.CountAsync());
        Cliente guardado = await db.Clientes
            .AsNoTracking()
            .SingleAsync(c => c.IdCliente == idCliente);
        Assert.True(guardado.Estado);
        Assert.Equal("DOC0001", guardado.Dni);
    }

    // RF-24: dar de baja un cliente ya inactivo no produce cambios.
    [Fact]
    public async Task DarBaja_ClienteYaInactivo_DevuelveClienteYaInactivoSinCambios()
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente cliente = await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: false);
        var negocio = new ClienteNegocio(db);

        RespuestaBajaCliente respuesta = await negocio.DarBajaClienteAsync(cliente.IdCliente, "Administrador");

        Assert.Equal(ResultadoBajaCliente.ClienteYaInactivo, respuesta.Resultado);
        Assert.Equal("El cliente Ana Gómez ya está inactivo.", respuesta.Mensaje);
        Assert.Null(respuesta.Cliente);
        Assert.Equal(1, await db.Clientes.CountAsync());
        Assert.False((await db.Clientes.AsNoTracking()
            .SingleAsync(c => c.IdCliente == cliente.IdCliente)).Estado);
    }

    // RF-24: reactivar un cliente ya activo no produce cambios ni duplicados.
    [Fact]
    public async Task Reactivar_ClienteYaActivo_DevuelveClienteYaActivoSinCambios()
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente cliente = await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaActivacionCliente respuesta =
            await negocio.ReactivarClienteAsync(cliente.IdCliente, "Administrador");

        Assert.Equal(ResultadoActivacionCliente.ClienteYaActivo, respuesta.Resultado);
        Assert.Equal("El cliente Ana Gómez ya está activo.", respuesta.Mensaje);
        Assert.Null(respuesta.Cliente);
        Assert.Equal(1, await db.Clientes.CountAsync());
        Assert.True((await db.Clientes.AsNoTracking()
            .SingleAsync(c => c.IdCliente == cliente.IdCliente)).Estado);
    }

    // RF-24: cliente inexistente.
    [Fact]
    public async Task DarBaja_ClienteInexistente_DevuelveClienteNoEncontrado()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaBajaCliente respuesta = await negocio.DarBajaClienteAsync(999, "Administrador");

        Assert.Equal(ResultadoBajaCliente.ClienteNoEncontrado, respuesta.Resultado);
        Assert.Equal(MensajeClienteNoEncontrado, respuesta.Mensaje);
        Assert.Null(respuesta.Cliente);
        Assert.Equal(0, await db.Clientes.CountAsync());
    }

    // RF-24: cliente inexistente al reactivar.
    [Fact]
    public async Task Reactivar_ClienteInexistente_DevuelveClienteNoEncontrado()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaActivacionCliente respuesta =
            await negocio.ReactivarClienteAsync(999, "Administrador");

        Assert.Equal(ResultadoActivacionCliente.ClienteNoEncontrado, respuesta.Resultado);
        Assert.Equal(MensajeClienteNoEncontrado, respuesta.Mensaje);
        Assert.Null(respuesta.Cliente);
        Assert.Equal(0, await db.Clientes.CountAsync());
    }

    // RF-24: identificador inválido en la baja.
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task DarBaja_IdInvalido_DevuelveDatosInvalidos(int idCliente)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaBajaCliente respuesta = await negocio.DarBajaClienteAsync(idCliente, "Administrador");

        Assert.Equal(ResultadoBajaCliente.DatosInvalidos, respuesta.Resultado);
        Assert.Equal(MensajeClienteInvalido, respuesta.Mensaje);
        Assert.Null(respuesta.Cliente);
    }

    // RF-24: identificador inválido en la reactivación.
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Reactivar_IdInvalido_DevuelveDatosInvalidos(int idCliente)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaActivacionCliente respuesta =
            await negocio.ReactivarClienteAsync(idCliente, "Administrador");

        Assert.Equal(ResultadoActivacionCliente.DatosInvalidos, respuesta.Resultado);
        Assert.Equal(MensajeClienteInvalido, respuesta.Mensaje);
        Assert.Null(respuesta.Cliente);
    }

    // RF-24: la confirmación informa que el cliente no existe.
    [Fact]
    public async Task ObtenerConfirmacionBaja_ClienteInexistente_DevuelveClienteNoEncontrado()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaConfirmacionBajaCliente respuesta =
            await negocio.ObtenerConfirmacionBajaClienteAsync(999, "Administrador");

        Assert.Equal(ResultadoConfirmacionBajaCliente.ClienteNoEncontrado, respuesta.Resultado);
        Assert.Equal(MensajeClienteNoEncontrado, respuesta.Mensaje);
        Assert.Equal(0, respuesta.VentasAsociadas);
    }

    // RF-24: identificador inválido en la confirmación.
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task ObtenerConfirmacionBaja_IdInvalido_DevuelveClienteNoEncontrado(int idCliente)
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaConfirmacionBajaCliente respuesta =
            await negocio.ObtenerConfirmacionBajaClienteAsync(idCliente, "Administrador");

        Assert.Equal(ResultadoConfirmacionBajaCliente.ClienteNoEncontrado, respuesta.Resultado);
    }

    // RF-24: la confirmación de baja de un cliente inactivo se rechaza.
    [Fact]
    public async Task ObtenerConfirmacionBaja_ClienteYaInactivo_DevuelveClienteYaInactivo()
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente cliente = await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: false);
        var negocio = new ClienteNegocio(db);

        RespuestaConfirmacionBajaCliente respuesta =
            await negocio.ObtenerConfirmacionBajaClienteAsync(cliente.IdCliente, "Administrador");

        Assert.Equal(ResultadoConfirmacionBajaCliente.ClienteYaInactivo, respuesta.Resultado);
        Assert.Equal("El cliente Ana Gómez ya está inactivo.", respuesta.Mensaje);
        Assert.Equal(0, respuesta.VentasAsociadas);
    }

    // RF-27/D-10: el Vendedor —y un rol nulo, vacío o desconocido— no puede dar de baja.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("vendedor")]
    [InlineData("Vendedor")]
    [InlineData("desconocido")]
    public async Task DarBaja_RolNoAutorizado_DevuelvePermisoDenegadoSinCambios(string? rol)
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente cliente = await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaBajaCliente respuesta = await negocio.DarBajaClienteAsync(cliente.IdCliente, rol);

        Assert.Equal(ResultadoBajaCliente.PermisoDenegado, respuesta.Resultado);
        Assert.Equal(MensajePermisoDenegadoBaja, respuesta.Mensaje);
        Assert.Null(respuesta.Cliente);
        Assert.True((await db.Clientes.AsNoTracking()
            .SingleAsync(c => c.IdCliente == cliente.IdCliente)).Estado);
    }

    // RF-27/D-10: el Vendedor —y un rol nulo, vacío o desconocido— no puede reactivar.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("vendedor")]
    [InlineData("Vendedor")]
    [InlineData("desconocido")]
    public async Task Reactivar_RolNoAutorizado_DevuelvePermisoDenegadoSinCambios(string? rol)
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente cliente = await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: false);
        var negocio = new ClienteNegocio(db);

        RespuestaActivacionCliente respuesta =
            await negocio.ReactivarClienteAsync(cliente.IdCliente, rol);

        Assert.Equal(ResultadoActivacionCliente.PermisoDenegado, respuesta.Resultado);
        Assert.Equal(MensajePermisoDenegadoReactivacion, respuesta.Mensaje);
        Assert.Null(respuesta.Cliente);
        Assert.False((await db.Clientes.AsNoTracking()
            .SingleAsync(c => c.IdCliente == cliente.IdCliente)).Estado);
    }

    // RF-27: el Vendedor no obtiene ni la confirmación de baja (no se consultan las ventas).
    [Theory]
    [InlineData(null)]
    [InlineData("vendedor")]
    public async Task ObtenerConfirmacionBaja_RolNoAutorizado_DevuelvePermisoDenegado(string? rol)
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente cliente = await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        await SembrarVentaAsync(db, cliente.IdCliente, estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaConfirmacionBajaCliente respuesta =
            await negocio.ObtenerConfirmacionBajaClienteAsync(cliente.IdCliente, rol);

        Assert.Equal(ResultadoConfirmacionBajaCliente.PermisoDenegado, respuesta.Resultado);
        Assert.Equal(MensajePermisoDenegadoBaja, respuesta.Mensaje);
        Assert.Equal(0, respuesta.VentasAsociadas);
    }

    // RF-27: el permiso se valida antes que cualquier consulta de datos.
    [Fact]
    public async Task DarBaja_RolNoAutorizado_ValidaElPermisoAntesQueLaExistencia()
    {
        await using var db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaBajaCliente respuesta = await negocio.DarBajaClienteAsync(999, "vendedor");

        Assert.Equal(ResultadoBajaCliente.PermisoDenegado, respuesta.Resultado);
        Assert.Equal(MensajePermisoDenegadoBaja, respuesta.Mensaje);
    }

    // RF-27/D-5: Administrador y Encargado pueden dar de baja y reactivar.
    [Theory]
    [InlineData("Administrador")]
    [InlineData("Encargado")]
    public async Task BajaYReactivacion_EncargadoYAdministrador_Pueden(string rol)
    {
        await using var db = await CrearContextoVacioAsync();
        Cliente cliente = await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaBajaCliente baja = await negocio.DarBajaClienteAsync(cliente.IdCliente, rol);

        Assert.Equal(ResultadoBajaCliente.Exito, baja.Resultado);
        Assert.False((await db.Clientes.AsNoTracking()
            .SingleAsync(c => c.IdCliente == cliente.IdCliente)).Estado);

        RespuestaActivacionCliente activacion =
            await negocio.ReactivarClienteAsync(cliente.IdCliente, rol);

        Assert.Equal(ResultadoActivacionCliente.Exito, activacion.Resultado);
        Assert.True((await db.Clientes.AsNoTracking()
            .SingleAsync(c => c.IdCliente == cliente.IdCliente)).Estado);
        Assert.Equal(1, await db.Clientes.CountAsync());
    }

    // RF-28: un fallo al guardar el alta se informa en español y no deja datos parciales.
    [Fact]
    public async Task CrearCliente_ConFalloAlGuardar_DevuelveErrorPersistenciaSinPersistir()
    {
        await using var db = await CrearContextoConFalloAlGuardarAsync();
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "30123456",
            Nombre = "Ana",
            Apellido = "Gómez"
        });

        Assert.Equal(ResultadoCreacionCliente.ErrorPersistencia, respuesta.Resultado);
        Assert.Equal(MensajeErrorPersistencia, respuesta.Mensaje);
        Assert.Null(respuesta.Cliente);
        Assert.Equal(0, await db.Clientes.CountAsync());
        Assert.Empty(db.ChangeTracker.Entries<Cliente>());
    }

    // RF-28: un fallo al guardar la modificación no altera el cliente persistido.
    [Fact]
    public async Task ModificarCliente_ConFalloAlGuardar_DevuelveErrorPersistenciaSinCambios()
    {
        await using var db = await CrearContextoConFalloAlGuardarAsync();
        Cliente cliente = await SembrarClienteConGuardadoSincronicoAsync(db, estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaModificacionCliente respuesta = await negocio.ModificarClienteAsync(
            new ModificarClienteDto
            {
                IdCliente = cliente.IdCliente,
                Documento = "DOC9999",
                Nombre = "Ana",
                Apellido = "Paz"
            });

        Assert.Equal(ResultadoModificacionCliente.ErrorPersistencia, respuesta.Resultado);
        Assert.Equal(MensajeErrorPersistencia, respuesta.Mensaje);
        Assert.Null(respuesta.Cliente);

        Cliente guardado = await db.Clientes
            .AsNoTracking()
            .SingleAsync(c => c.IdCliente == cliente.IdCliente);
        Assert.Equal("DOC0001", guardado.Dni);
        Assert.Equal("Gómez", guardado.Apellido);
    }

    // RF-28: un fallo al guardar la baja deja al cliente activo.
    [Fact]
    public async Task DarBaja_ConFalloAlGuardar_DevuelveErrorPersistenciaSinCambios()
    {
        await using var db = await CrearContextoConFalloAlGuardarAsync();
        Cliente cliente = await SembrarClienteConGuardadoSincronicoAsync(db, estado: true);
        var negocio = new ClienteNegocio(db);

        RespuestaBajaCliente respuesta = await negocio.DarBajaClienteAsync(
            cliente.IdCliente, "Administrador");

        Assert.Equal(ResultadoBajaCliente.ErrorPersistencia, respuesta.Resultado);
        Assert.Equal(MensajeErrorPersistencia, respuesta.Mensaje);
        Assert.Null(respuesta.Cliente);
        Assert.True((await db.Clientes.AsNoTracking()
            .SingleAsync(c => c.IdCliente == cliente.IdCliente)).Estado);
    }

    // RF-28: un fallo al guardar la reactivación deja al cliente inactivo.
    [Fact]
    public async Task Reactivar_ConFalloAlGuardar_DevuelveErrorPersistenciaSinCambios()
    {
        await using var db = await CrearContextoConFalloAlGuardarAsync();
        Cliente cliente = await SembrarClienteConGuardadoSincronicoAsync(db, estado: false);
        var negocio = new ClienteNegocio(db);

        RespuestaActivacionCliente respuesta = await negocio.ReactivarClienteAsync(
            cliente.IdCliente, "Administrador");

        Assert.Equal(ResultadoActivacionCliente.ErrorPersistencia, respuesta.Resultado);
        Assert.Equal(MensajeErrorPersistencia, respuesta.Mensaje);
        Assert.Null(respuesta.Cliente);
        Assert.False((await db.Clientes.AsNoTracking()
            .SingleAsync(c => c.IdCliente == cliente.IdCliente)).Estado);
    }

    // B1 (RF-10, caso límite «Duplicado concurrente»): si otra alta ganó la carrera,
    // el segundo alta recibe el mensaje de documento duplicado del flujo normal.
    [Fact]
    public async Task CrearCliente_ConDocumentoDuplicadoPorCarrera_DevuelveDocumentoDuplicadoSinPersistir()
    {
        const string documento = "30123456";

        await using var db = await CrearContextoConConflictoConcurrenteAsync(new Cliente
        {
            Dni = documento,
            Nombre = "Ana",
            Apellido = "Ganadora",
            Estado = true
        });
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = documento,
            Nombre = "Ana",
            Apellido = "Perdedora"
        });

        Assert.Equal(ResultadoCreacionCliente.DocumentoDuplicado, respuesta.Resultado);
        Assert.Equal($"Ya existe un cliente con el documento \"{documento}\".", respuesta.Mensaje);
        Assert.Null(respuesta.Cliente);
        Assert.Equal(1, await db.Clientes.CountAsync());
        Assert.Equal("Ganadora", (await db.Clientes.AsNoTracking().SingleAsync()).Apellido);
        Assert.Empty(db.ChangeTracker.Entries<Cliente>());
    }

    // B1 (RF-11, caso límite «Duplicado concurrente»): dos correos iguales en carreras
    // distintas se informan con el mensaje de correo duplicado del flujo normal.
    [Fact]
    public async Task CrearCliente_ConCorreoDuplicadoPorCarrera_DevuelveCorreoDuplicadoSinPersistir()
    {
        const string correo = "existente@mail.com";

        await using var db = await CrearContextoConConflictoConcurrenteAsync(new Cliente
        {
            Dni = "DOC0009",
            Nombre = "Cliente",
            Apellido = "Ganador",
            Correo = correo,
            Estado = true
        });
        var negocio = new ClienteNegocio(db);

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "30123456",
            Nombre = "Ana",
            Apellido = "Gómez",
            Correo = correo
        });

        Assert.Equal(ResultadoCreacionCliente.CorreoDuplicado, respuesta.Resultado);
        Assert.Equal($"Ya existe un cliente con el correo \"{correo}\".", respuesta.Mensaje);
        Assert.Null(respuesta.Cliente);
        Assert.Equal(1, await db.Clientes.CountAsync());

        // El único cliente persistido es el inyectado por el doble: el del alta no se guardó.
        Cliente persistido = await db.Clientes.AsNoTracking().SingleAsync();
        Assert.Equal("DOC0009", persistido.Dni);
        Assert.Empty(db.ChangeTracker.Entries<Cliente>());
    }

    // B1 (RF-12, caso límite «Duplicado concurrente»): la edición rechazada informa el
    // duplicado y el cliente original conserva su documento anterior.
    [Fact]
    public async Task ModificarCliente_ConDocumentoDuplicadoPorCarrera_DevuelveDocumentoDuplicadoSinCambios()
    {
        const string documentoOriginal = "30123456";
        const string documentoNuevo = "30999999";

        await using var db = await CrearContextoConConflictoConcurrenteAsync(new Cliente
        {
            Dni = documentoNuevo,
            Nombre = "Cliente",
            Apellido = "Ganador",
            Estado = true
        });

        // El cliente a editar se siembra con el guardado sincrónico, que el doble no intercepta.
        var cliente = new Cliente
        {
            Dni = documentoOriginal,
            Nombre = "Ana",
            Apellido = "Gómez",
            Estado = true
        };
        db.Clientes.Add(cliente);
        db.SaveChanges();

        var negocio = new ClienteNegocio(db);

        RespuestaModificacionCliente respuesta = await negocio.ModificarClienteAsync(
            new ModificarClienteDto
            {
                IdCliente = cliente.IdCliente,
                Documento = documentoNuevo,
                Nombre = "Ana",
                Apellido = "Gómez"
            });

        Assert.Equal(ResultadoModificacionCliente.DocumentoDuplicado, respuesta.Resultado);
        Assert.Equal($"Ya existe un cliente con el documento \"{documentoNuevo}\".", respuesta.Mensaje);
        Assert.Null(respuesta.Cliente);

        Cliente guardado = await db.Clientes
            .AsNoTracking()
            .SingleAsync(c => c.IdCliente == cliente.IdCliente);

        Assert.Equal(documentoOriginal, guardado.Dni);

        // El cliente inyectado por el doble quedó persistido: la carrera ocurrió al guardar.
        Assert.Equal(2, await db.Clientes.CountAsync());
    }

    // RF-28: un fallo al consultar el padrón se informa sin propagar la excepción.
    [Fact]
    public async Task ListarClientes_ConContextoDesechado_DevuelveErrorPersistenciaConListaVacia()
    {
        AppDbContext db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        var negocio = new ClienteNegocio(db);
        await db.DisposeAsync();

        RespuestaListadoClientes respuesta = await negocio.ListarClientesAsync(
            FiltroEstadoCliente.Activos, "ana");

        Assert.Equal(ResultadoListadoClientes.ErrorPersistencia, respuesta.Resultado);
        Assert.Equal(MensajeErrorPersistencia, respuesta.Mensaje);
        Assert.Empty(respuesta.Clientes);
    }

    // RF-28: un fallo al consultar la búsqueda rápida se informa sin propagar la excepción.
    [Fact]
    public async Task BuscarClientesParaPos_ConContextoDesechado_DevuelveErrorPersistenciaConListaVacia()
    {
        AppDbContext db = await CrearContextoVacioAsync();
        await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        var negocio = new ClienteNegocio(db);
        await db.DisposeAsync();

        RespuestaListadoClientes respuesta = await negocio.BuscarClientesParaPosAsync("ana");

        Assert.Equal(ResultadoListadoClientes.ErrorPersistencia, respuesta.Resultado);
        Assert.Equal(MensajeErrorPersistencia, respuesta.Mensaje);
        Assert.Empty(respuesta.Clientes);
    }

    // RF-28: un fallo al consultar la confirmación de baja se informa en español.
    [Fact]
    public async Task ObtenerConfirmacionBaja_ConContextoDesechado_DevuelveErrorPersistencia()
    {
        AppDbContext db = await CrearContextoVacioAsync();
        Cliente cliente = await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        var negocio = new ClienteNegocio(db);
        await db.DisposeAsync();

        RespuestaConfirmacionBajaCliente respuesta =
            await negocio.ObtenerConfirmacionBajaClienteAsync(cliente.IdCliente, "Administrador");

        Assert.Equal(ResultadoConfirmacionBajaCliente.ErrorPersistencia, respuesta.Resultado);
        Assert.Equal(MensajeErrorPersistencia, respuesta.Mensaje);
        Assert.Equal(0, respuesta.VentasAsociadas);
    }

    // RF-28: el conteo de ventas informa el fallo con la convención documentada (-1).
    [Fact]
    public async Task ContarVentasCliente_ConContextoDesechado_DevuelveConvencionDeError()
    {
        AppDbContext db = await CrearContextoVacioAsync();
        Cliente cliente = await SembrarClienteAsync(db, "DOC0001", "Ana", "Gómez", null, estado: true);
        var negocio = new ClienteNegocio(db);
        await db.DisposeAsync();

        int cantidad = await negocio.ContarVentasClienteAsync(cliente.IdCliente);

        Assert.Equal(-1, cantidad);
    }

    // RF-28: las validaciones de entrada siguen devolviendo su resultado propio.
    [Fact]
    public async Task CrearCliente_ConContextoRotoYDocumentoVacio_DevuelveDatosInvalidos()
    {
        AppDbContext db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);
        await db.DisposeAsync();

        RespuestaCreacionCliente respuesta = await negocio.CrearClienteAsync(new CrearClienteDto
        {
            Documento = "   ",
            Nombre = "Ana",
            Apellido = "Gómez"
        });

        Assert.Equal(ResultadoCreacionCliente.DatosInvalidos, respuesta.Resultado);
        Assert.Equal(MensajeDocumentoObligatorio, respuesta.Mensaje);
    }

    // RF-28: las validaciones de entrada de la edición siguen devolviendo su resultado propio.
    [Fact]
    public async Task ModificarCliente_ConContextoRotoEIdInvalido_DevuelveDatosInvalidos()
    {
        AppDbContext db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);
        await db.DisposeAsync();

        RespuestaModificacionCliente respuesta = await negocio.ModificarClienteAsync(
            new ModificarClienteDto
            {
                IdCliente = 0,
                Documento = "DOC0001",
                Nombre = "Ana",
                Apellido = "Gómez"
            });

        Assert.Equal(ResultadoModificacionCliente.DatosInvalidos, respuesta.Resultado);
        Assert.Equal(MensajeClienteInvalido, respuesta.Mensaje);
    }

    // RF-28: el permiso denegado no se convierte en error de persistencia.
    [Fact]
    public async Task OperacionesDeBajaConContextoRoto_PermisoDenegadoSeMantiene()
    {
        AppDbContext db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);
        await db.DisposeAsync();

        RespuestaBajaCliente baja = await negocio.DarBajaClienteAsync(1, "vendedor");
        RespuestaActivacionCliente activacion = await negocio.ReactivarClienteAsync(1, "vendedor");
        RespuestaConfirmacionBajaCliente confirmacion =
            await negocio.ObtenerConfirmacionBajaClienteAsync(1, "vendedor");

        Assert.Equal(ResultadoBajaCliente.PermisoDenegado, baja.Resultado);
        Assert.Equal(MensajePermisoDenegadoBaja, baja.Mensaje);
        Assert.Equal(ResultadoActivacionCliente.PermisoDenegado, activacion.Resultado);
        Assert.Equal(MensajePermisoDenegadoReactivacion, activacion.Mensaje);
        Assert.Equal(ResultadoConfirmacionBajaCliente.PermisoDenegado, confirmacion.Resultado);
        Assert.Equal(MensajePermisoDenegadoBaja, confirmacion.Mensaje);
    }

    // RF-28: el identificador inválido de la baja se valida antes de tocar el origen de datos.
    [Fact]
    public async Task DarBaja_ConContextoRotoEIdInvalido_DevuelveDatosInvalidos()
    {
        AppDbContext db = await CrearContextoVacioAsync();
        var negocio = new ClienteNegocio(db);
        await db.DisposeAsync();

        RespuestaBajaCliente respuesta = await negocio.DarBajaClienteAsync(0, "Administrador");

        Assert.Equal(ResultadoBajaCliente.DatosInvalidos, respuesta.Resultado);
        Assert.Equal(MensajeClienteInvalido, respuesta.Mensaje);
    }

    /// <summary>
    /// Contexto de prueba cuyo guardado asincrónico siempre falla, para verificar la
    /// traducción de fallos del origen de datos (RF-28) sin una base real.
    /// </summary>
    private sealed class ContextoConFalloAlGuardar : AppDbContext
    {
        public ContextoConFalloAlGuardar(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new DbUpdateException("Fallo simulado del origen de datos.");
        }
    }

    private static async Task<AppDbContext> CrearContextoConFalloAlGuardarAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"ClienteNegocioTests-Fallo-{Guid.NewGuid()}")
            .Options;
        var db = new ContextoConFalloAlGuardar(options);

        await db.Database.EnsureCreatedAsync();
        return db;
    }

    /// <summary>
    /// Doble de <see cref="AppDbContext"/> que simula una carrera entre dos altas: al
    /// intentar guardar inserta el cliente duplicado desde otro contexto sobre la misma
    /// base InMemory —la verificación previa no lo ve, la re-evaluación posterior sí— y
    /// después lanza <see cref="DbUpdateException"/>, como haría el índice único.
    /// </summary>
    private sealed class ContextoConConflictoConcurrente : AppDbContext
    {
        private readonly DbContextOptions<AppDbContext> _options;
        private readonly Cliente _duplicadoConcurrente;
        private bool _conflictoProvocado;

        public ContextoConConflictoConcurrente(
            DbContextOptions<AppDbContext> options,
            Cliente duplicadoConcurrente)
            : base(options)
        {
            _options = options;
            _duplicadoConcurrente = duplicadoConcurrente;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (!_conflictoProvocado)
            {
                _conflictoProvocado = true;

                // Se copia el cliente en otro contexto para no compartir la instancia
                // rastreada por el contexto bajo prueba.
                await using var otroContexto = new AppDbContext(_options);
                otroContexto.Clientes.Add(new Cliente
                {
                    Dni = _duplicadoConcurrente.Dni,
                    Nombre = _duplicadoConcurrente.Nombre,
                    Apellido = _duplicadoConcurrente.Apellido,
                    Correo = _duplicadoConcurrente.Correo,
                    Telefono = _duplicadoConcurrente.Telefono,
                    Estado = _duplicadoConcurrente.Estado
                });
                await otroContexto.SaveChangesAsync();
            }

            throw new DbUpdateException(
                "Fallo simulado por conflicto de unicidad (carrera entre altas).");
        }
    }

    private static async Task<AppDbContext> CrearContextoConConflictoConcurrenteAsync(
        Cliente duplicadoConcurrente)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"ClienteNegocioTests-Conflicto-{Guid.NewGuid()}")
            .Options;
        var db = new ContextoConConflictoConcurrente(options, duplicadoConcurrente);

        await db.Database.EnsureCreatedAsync();
        return db;
    }

    /// <summary>
    /// Siembra un cliente usando el guardado sincrónico, que el doble de prueba no
    /// intercepta, para poder probar el fallo del guardado asincrónico (RF-28).
    /// </summary>
    private static Task<Cliente> SembrarClienteConGuardadoSincronicoAsync(AppDbContext db, bool estado)
    {
        var cliente = new Cliente
        {
            Dni = "DOC0001",
            Nombre = "Ana",
            Apellido = "Gómez",
            Estado = estado
        };

        db.Clientes.Add(cliente);
        db.SaveChanges();
        return Task.FromResult(cliente);
    }

    /// <summary>
    /// Siembra una venta del cliente indicado para las pruebas de historial (D-11).
    /// </summary>
    private static async Task<Venta> SembrarVentaAsync(AppDbContext db, int idCliente, bool estado)
    {
        var venta = new Venta
        {
            IdCliente = idCliente,
            IdTipo = 1,
            MontoPago = 100m,
            MontoCambio = 0m,
            MontoTotal = 100m,
            Estado = estado
        };

        db.Ventas.Add(venta);
        await db.SaveChangesAsync();
        return venta;
    }

    private static async Task SembrarClienteParaPosAsync(AppDbContext db, char letra)
    {
        await SembrarClienteAsync(
            db, $"DOC{letra}0001", letra.ToString(), "Cliente", null, estado: true);
    }

    private static async Task<Cliente> SembrarClienteAsync(
        AppDbContext db, string? documento, string? correo, bool estado)
    {
        return await SembrarClienteAsync(db, documento, "Cliente", "Existente", correo, estado);
    }

    private static async Task<Cliente> SembrarClienteAsync(
        AppDbContext db, string? documento, string nombre, string apellido, string? correo, bool estado)
    {
        var cliente = new Cliente
        {
            Dni = documento,
            Nombre = nombre,
            Apellido = apellido,
            Correo = correo,
            Estado = estado
        };

        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();
        return cliente;
    }

    /// <summary>
    /// Comprueba que una edición rechazada no dejó cambios en el cliente sembrado.
    /// </summary>
    private static async Task VerificarClienteSinCambiosAsync(
        AppDbContext db, int idCliente, string? documentoEsperado, string? correoEsperado)
    {
        Cliente cliente = await db.Clientes
            .AsNoTracking()
            .SingleAsync(c => c.IdCliente == idCliente);

        Assert.Equal(documentoEsperado, cliente.Dni);
        Assert.Equal(correoEsperado, cliente.Correo);
        Assert.Equal("Cliente", cliente.Nombre);
        Assert.Equal("Existente", cliente.Apellido);
        Assert.Null(cliente.Telefono);
    }

    private static async Task<AppDbContext> CrearContextoVacioAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"ClienteNegocioTests-{Guid.NewGuid()}")
            .Options;
        var db = new AppDbContext(options);

        await db.Database.EnsureCreatedAsync();
        return db;
    }
}
