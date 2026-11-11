using CapaDatos.Respaldo;
using CapaNegocio.DTOs.Respaldos;
using CapaNegocio.Enums;
using CapaNegocio.Respaldo;
using Xunit;

namespace CapaNegocio.Tests;

public sealed class RespaldoNegocioTests : IDisposable
{
    private const string Rol = "Administrador";
    private const string Db = "DB_X";

    private readonly string _carpeta = Path.Combine(Path.GetTempPath(), "G70_resp_" + Guid.NewGuid().ToString("N"));
    private readonly SesionFalsa _sesion = new();
    private readonly RepositorioFalso _repo;
    private readonly ExclusionRespaldo _exclusion = new();
    private readonly RespaldoNegocio _negocio;

    public RespaldoNegocioTests()
    {
        Directory.CreateDirectory(_carpeta);
        _repo = new RepositorioFalso(_sesion);
        _negocio = new RespaldoNegocio(_repo, new RutaFalsa(_carpeta), _exclusion, new RelojFijo());
    }

    public void Dispose()
    {
        try { Directory.Delete(_carpeta, true); } catch (IOException) { }
    }

    // ---------- nombres y textos ----------

    [Fact]
    public void ConstruirNombreArchivo_RespetaFormatoManualYAutomatico()
    {
        var f = new DateTime(2026, 11, 11, 10, 35, 7);
        Assert.Equal("DB_X_20261111_103507_MANUAL.bak", RespaldoNegocio.ConstruirNombreArchivo(Db, f, false));
        Assert.Equal("DB_X_20261111_103507_AUTO-PRE-RESTAURACION.bak", RespaldoNegocio.ConstruirNombreArchivo(Db, f, true));
    }

    [Fact]
    public void ConstruirTextoConfirmacion_IncluyeFechaHoraNombreYAdvertencias()
    {
        var r = new RespaldoDisponibleDto { NombreArchivo = "X.bak", FechaBackup = new DateTime(2026, 11, 11, 10, 35, 0) };
        string t = RespaldoNegocio.ConstruirTextoConfirmacion(r);
        Assert.Equal(
            "Se restaurará la base de datos al estado del 11/11/2026 a las 10:35, utilizando el respaldo X.bak. "
            + "Esta operación reemplazará el estado actual de la base de datos. "
            + "Los cambios posteriores a esa fecha y hora se perderán. ¿Está seguro de que desea continuar?", t);
        Assert.DoesNotContain(_carpeta, t);
    }

    // ---------- autorización ----------

    [Theory]
    [InlineData("Vendedor")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Otro")]
    public async Task TodasLasOperaciones_SinPermiso(string? rol)
    {
        Assert.Equal(ResultadoRespaldo.SinPermiso, (await _negocio.GenerarBackupAsync(rol)).Resultado);
        Assert.Equal(ResultadoRespaldo.SinPermiso, (await _negocio.ListarRespaldosAsync(rol)).Resultado);
        Assert.Equal(ResultadoRespaldo.SinPermiso, (await _negocio.ValidarParaRestauracionAsync(rol, "x.bak")).Resultado);
        Assert.Equal(ResultadoRespaldo.SinPermiso, (await _negocio.EjecutarRestauracionAsync(rol, new ValidacionRespaldoDto())).Resultado);
        Assert.Empty(_sesion.Llamadas);
        Assert.Equal(0, _repo.SesionesAbiertas);
    }

    // ---------- generar ----------

    [Fact]
    public async Task Generar_Exito_HaceBackupYVerificacionYLiberaTodo()
    {
        RespuestaGeneracionRespaldo r = await _negocio.GenerarBackupAsync(Rol);

        Assert.Equal(ResultadoRespaldo.Exito, r.Resultado);
        Assert.Equal(Path.Combine(_carpeta, "DB_X_20261111_103507_MANUAL.bak"), r.RutaArchivo);
        Assert.Equal(2, _sesion.Llamadas.Count);
        Assert.StartsWith("BACKUP:", _sesion.Llamadas[0]);
        Assert.StartsWith("VERIFY:", _sesion.Llamadas[1]);
        Assert.True(_sesion.Liberada);
        await AssertSemaforoLibre();
    }

    [Fact]
    public async Task Generar_FallaBackup_NoEsExitoYLiberaSemaforoYSesion()
    {
        _sesion.FalloBackup = new RespaldoBdException(3201, new Exception("x"));

        RespuestaGeneracionRespaldo r = await _negocio.GenerarBackupAsync(Rol);

        Assert.Equal(ResultadoRespaldo.RutaNoDisponible, r.Resultado);
        Assert.DoesNotContain("x", r.Mensaje.Split(' '));
        Assert.DoesNotContain(_sesion.Llamadas, l => l.StartsWith("VERIFY:"));
        Assert.True(_sesion.Liberada);
        await AssertSemaforoLibre();
    }

    [Fact]
    public async Task Generar_FallaVerificacion_NoEsExito()
    {
        _sesion.FalloVerificar = new RespaldoBdException(3241, new Exception());
        RespuestaGeneracionRespaldo r = await _negocio.GenerarBackupAsync(Rol);
        Assert.NotEqual(ResultadoRespaldo.Exito, r.Resultado);
        Assert.True(_sesion.Liberada);
    }

    [Fact]
    public async Task Generar_NoSobrescribeArchivoExistente()
    {
        File.WriteAllText(Path.Combine(_carpeta, "DB_X_20261111_103507_MANUAL.bak"), "previo");

        RespuestaGeneracionRespaldo r = await _negocio.GenerarBackupAsync(Rol);

        Assert.Equal(ResultadoRespaldo.Fallo, r.Resultado);
        Assert.Empty(_sesion.Llamadas);
        Assert.Equal("previo", File.ReadAllText(Path.Combine(_carpeta, "DB_X_20261111_103507_MANUAL.bak")));
    }

    [Fact]
    public async Task Generar_SemaforoOcupado_OperacionEnCursoSinAbrirSesion()
    {
        using IDisposable? ocupado = await _exclusion.IntentarEntrarAsync(default);
        Assert.NotNull(ocupado);

        RespuestaGeneracionRespaldo r = await _negocio.GenerarBackupAsync(Rol);

        Assert.Equal(ResultadoRespaldo.OperacionEnCurso, r.Resultado);
        Assert.Equal(0, _repo.SesionesAbiertas);
    }

    [Fact]
    public async Task Generar_LockOcupado_OperacionEnCursoYSemaforoLiberado()
    {
        _repo.CodigoLock = -1;

        RespuestaGeneracionRespaldo r = await _negocio.GenerarBackupAsync(Rol);

        Assert.Equal(ResultadoRespaldo.OperacionEnCurso, r.Resultado);
        Assert.Empty(_sesion.Llamadas);
        await AssertSemaforoLibre();
    }

    [Fact]
    public async Task Generar_RutaNoDisponible_DevuelveElMensajeDelProveedor()
    {
        var negocio = new RespaldoNegocio(_repo, new RutaFalsa(null), _exclusion);
        RespuestaGeneracionRespaldo r = await negocio.GenerarBackupAsync(Rol);
        Assert.Equal(ResultadoRespaldo.RutaNoDisponible, r.Resultado);
        Assert.Equal(ProveedorRutaRespaldo.MensajeSinRaiz, r.Mensaje);
    }

    // ---------- listado ----------

    [Fact]
    public async Task Listar_Top5OrdenadoPorFechaRealYOmiteInvalidos()
    {
        var baseFecha = new DateTime(2026, 11, 1, 8, 0, 0);
        // 7 válidos: la fecha real crece con i, la fecha de archivo decrece (no debe usarse).
        for (int i = 0; i < 7; i++)
        {
            string nombre = i == 3 ? $"DB_X_{i}_AUTO-PRE-RESTAURACION.bak" : $"DB_X_{i}.bak";
            AgregarBackup(nombre, baseFecha.AddDays(i), mtime: baseFecha.AddDays(-i));
        }

        AgregarBackup("ajeno.bak", baseFecha.AddDays(30), otraBase: true);
        CrearArchivo("ilegible.bak");
        _sesion.Ilegibles.Add("ilegible.bak");

        RespuestaListadoRespaldos r = await _negocio.ListarRespaldosAsync(Rol);

        Assert.Equal(ResultadoRespaldo.Exito, r.Resultado);
        Assert.Equal(5, r.Respaldos.Count);
        Assert.Equal(new[] { 6, 5, 4, 3, 2 }, r.Respaldos.Select(x => (int)(x.FechaBackup - baseFecha).TotalDays));
        Assert.Equal("Automático", r.Respaldos.Single(x => x.NombreArchivo.Contains("AUTO")).Tipo);
        Assert.Equal("Manual", r.Respaldos[0].Tipo);
        Assert.Equal(2, r.ArchivosNoLegibles);
        Assert.Contains("2 archivos no legibles", r.Mensaje);
    }

    [Fact]
    public async Task Listar_SinValidos_MensajeNoHayBackup()
    {
        CrearArchivo("ilegible.bak");
        _sesion.Ilegibles.Add("ilegible.bak");

        RespuestaListadoRespaldos r = await _negocio.ListarRespaldosAsync(Rol);

        Assert.Equal(ResultadoRespaldo.SinRespaldosDisponibles, r.Resultado);
        Assert.Contains("No hay ningún backup disponible", r.Mensaje);
        Assert.Empty(r.Respaldos);
    }

    // ---------- restauración ----------

    [Fact]
    public async Task Restaurar_Exito_OrdenCorrectoYLiberaTodo()
    {
        ValidacionRespaldoDto v = await ValidarElegido();

        RespuestaRestauracion r = await _negocio.EjecutarRestauracionAsync(Rol, v);

        Assert.Equal(ResultadoRespaldo.Exito, r.Resultado);
        Assert.NotNull(r.RutaBackupSeguridad);
        Assert.EndsWith("_AUTO-PRE-RESTAURACION.bak", r.RutaBackupSeguridad);
        Assert.Equal("ONLINE / MULTI_USER", r.EstadoAccesoFinal);
        // VERIFY inicial = revalidación del archivo elegido; luego backup de seguridad.
        string[] esperado = { "VERIFY", "BACKUP", "VERIFY", "SINGLE_USER", "RESTORE", "MULTI_USER" };
        Assert.Equal(esperado, _sesion.Llamadas.Where(l => !l.StartsWith("LEER")).Select(l => l.Split(':')[0]));
        Assert.True(_sesion.Liberada);
        await AssertSemaforoLibre();
    }

    [Fact]
    public async Task Restaurar_FallaBackupSeguridad_NoEmiteSingleUserNiRestore()
    {
        ValidacionRespaldoDto v = await ValidarElegido();
        _sesion.FalloBackup = new RespaldoBdException(3201, new Exception());

        RespuestaRestauracion r = await _negocio.EjecutarRestauracionAsync(Rol, v);

        Assert.Equal(ResultadoRespaldo.FalloBackupSeguridad, r.Resultado);
        Assert.DoesNotContain(_sesion.Llamadas, l => l is "SINGLE_USER" or "MULTI_USER" || l.StartsWith("RESTORE"));
        Assert.True(_sesion.Liberada);
        await AssertSemaforoLibre();
    }

    [Fact]
    public async Task Restaurar_FallaVerificacionDeSeguridad_NoEmiteSingleUserNiRestore()
    {
        ValidacionRespaldoDto v = await ValidarElegido();
        _sesion.FalloVerificarSeguridad = true;

        RespuestaRestauracion r = await _negocio.EjecutarRestauracionAsync(Rol, v);

        Assert.Equal(ResultadoRespaldo.FalloBackupSeguridad, r.Resultado);
        Assert.DoesNotContain(_sesion.Llamadas, l => l is "SINGLE_USER" || l.StartsWith("RESTORE"));
    }

    [Fact]
    public async Task Restaurar_EstadisticasAsincronas_NoModificaNada()
    {
        ValidacionRespaldoDto v = await ValidarElegido();
        _sesion.Estado = new EstadoBaseDatos("ONLINE", "MULTI_USER", true);

        RespuestaRestauracion r = await _negocio.EjecutarRestauracionAsync(Rol, v);

        Assert.Equal(ResultadoRespaldo.EstadisticasAsincronasActivas, r.Resultado);
        Assert.Equal(RespaldoNegocio.MensajeEstadisticasAsincronas, r.Mensaje);
        Assert.DoesNotContain(_sesion.Llamadas, l => l.StartsWith("BACKUP") || l is "SINGLE_USER" or "MULTI_USER" || l.StartsWith("RESTORE"));
        Assert.True(_sesion.Liberada);
        await AssertSemaforoLibre();
    }

    [Fact]
    public async Task Restaurar_HuellaDistinta_Aborta()
    {
        ValidacionRespaldoDto v = await ValidarElegido();
        File.SetLastWriteTimeUtc(v.Respaldo!.RutaArchivo, v.Respaldo.Huella.FechaModificacionArchivoUtc.AddMinutes(5));

        RespuestaRestauracion r = await _negocio.EjecutarRestauracionAsync(Rol, v);

        Assert.Equal(ResultadoRespaldo.RespaldoModificado, r.Resultado);
        Assert.DoesNotContain(_sesion.Llamadas, l => l.StartsWith("BACKUP") || l is "SINGLE_USER" || l.StartsWith("RESTORE"));
        Assert.True(_sesion.Liberada);
        await AssertSemaforoLibre();
    }

    [Fact]
    public async Task Restaurar_LockOcupado_OperacionEnCurso()
    {
        ValidacionRespaldoDto v = await ValidarElegido();
        _repo.CodigoLock = -1;

        RespuestaRestauracion r = await _negocio.EjecutarRestauracionAsync(Rol, v);

        Assert.Equal(ResultadoRespaldo.OperacionEnCurso, r.Resultado);
        Assert.Contains("otra instancia", r.Mensaje, StringComparison.OrdinalIgnoreCase);
        await AssertSemaforoLibre();
    }

    [Fact]
    public async Task Restaurar_SemaforoOcupado_OperacionEnCurso()
    {
        ValidacionRespaldoDto v = await ValidarElegido();
        using IDisposable? ocupado = await _exclusion.IntentarEntrarAsync(default);

        RespuestaRestauracion r = await _negocio.EjecutarRestauracionAsync(Rol, v);

        Assert.Equal(ResultadoRespaldo.OperacionEnCurso, r.Resultado);
    }

    [Fact]
    public async Task Restaurar_FallaRestore_RecuperaMultiUserInformaRutaYNoReintenta()
    {
        ValidacionRespaldoDto v = await ValidarElegido();
        _sesion.FalloRestore = new RespaldoBdException(3154, new Exception());

        RespuestaRestauracion r = await _negocio.EjecutarRestauracionAsync(Rol, v);

        Assert.Equal(ResultadoRespaldo.FalloRestauracion, r.Resultado);
        Assert.Contains(r.RutaBackupSeguridad!, r.Mensaje);
        Assert.Single(_sesion.Llamadas, l => l.StartsWith("RESTORE"));
        Assert.Contains("MULTI_USER", _sesion.Llamadas);
        Assert.True(_sesion.Liberada);
        await AssertSemaforoLibre();
    }

    [Fact]
    public async Task Restaurar_FallaRecuperacion_SeInformaConRutaDeSeguridad()
    {
        ValidacionRespaldoDto v = await ValidarElegido();
        _sesion.FalloRestore = new RespaldoBdException(3154, new Exception());
        _sesion.EstadoTrasRecuperar = null;

        RespuestaRestauracion r = await _negocio.EjecutarRestauracionAsync(Rol, v);

        Assert.Equal(ResultadoRespaldo.FalloRestauracion, r.Resultado);
        Assert.Contains("No se pudo verificar el estado final", r.Mensaje);
        Assert.Contains(r.RutaBackupSeguridad!, r.Mensaje);
        Assert.Null(r.EstadoAccesoFinal);
        await AssertSemaforoLibre();
    }

    [Fact]
    public async Task Restaurar_BaseNoQuedaMultiUser_NoEsExito()
    {
        ValidacionRespaldoDto v = await ValidarElegido();
        _sesion.EstadoTrasRecuperar = new EstadoBaseDatos("ONLINE", "SINGLE_USER", false);

        RespuestaRestauracion r = await _negocio.EjecutarRestauracionAsync(Rol, v);

        Assert.Equal(ResultadoRespaldo.FalloRestauracion, r.Resultado);
        Assert.Equal("ONLINE / SINGLE_USER", r.EstadoAccesoFinal);
    }

    [Fact]
    public async Task Validar_ArchivoFueraDeLaCarpeta_Invalido()
    {
        string fuera = Path.Combine(Path.GetTempPath(), "fuera.bak");
        ValidacionRespaldoDto v = await _negocio.ValidarParaRestauracionAsync(Rol, fuera);
        Assert.Equal(ResultadoRespaldo.ArchivoInvalido, v.Resultado);
    }

    // ---------- cancelación ----------

    [Fact]
    public async Task Generar_CanceladoDuranteBackup_DevuelveCanceladoYLiberaTodo()
    {
        using var cts = new CancellationTokenSource();
        _sesion.AlBackup = (_, ct) => { cts.Cancel(); ct.ThrowIfCancellationRequested(); };

        RespuestaGeneracionRespaldo r = await _negocio.GenerarBackupAsync(Rol, cts.Token);

        Assert.Equal(ResultadoRespaldo.Cancelado, r.Resultado);
        Assert.Null(r.RutaArchivo);
        Assert.DoesNotContain(_sesion.Llamadas, l => l.StartsWith("VERIFY:"));
        Assert.True(_sesion.Liberada);
        await AssertSemaforoLibre();
    }

    [Fact]
    public async Task Generar_TokenYaCancelado_NoEjecutaNada()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        RespuestaGeneracionRespaldo r = await _negocio.GenerarBackupAsync(Rol, cts.Token);

        Assert.Equal(ResultadoRespaldo.Cancelado, r.Resultado);
        Assert.Empty(_sesion.Llamadas);
        await AssertSemaforoLibre();
    }

    [Fact]
    public async Task Restaurar_CanceladoDuranteBackupAutomatico_NoEmiteSingleUserNiRestoreYLibera()
    {
        ValidacionRespaldoDto v = await ValidarElegido();
        using var cts = new CancellationTokenSource();
        _sesion.AlBackup = (_, ct) => { cts.Cancel(); ct.ThrowIfCancellationRequested(); };
        bool fase = false;

        RespuestaRestauracion r = await _negocio.EjecutarRestauracionAsync(Rol, v, null, cts.Token, () => fase = true);

        Assert.Equal(ResultadoRespaldo.Cancelado, r.Resultado);
        Assert.False(fase);
        Assert.DoesNotContain("SINGLE_USER", _sesion.Llamadas);
        Assert.DoesNotContain(_sesion.Llamadas, l => l.StartsWith("RESTORE"));
        Assert.DoesNotContain("MULTI_USER", _sesion.Llamadas);
        Assert.True(_sesion.Liberada);
        await AssertSemaforoLibre();
    }

    [Fact]
    public async Task Restaurar_CanceladoTrasBackupAutomaticoVerificado_NoRestauraEInformaRutaDeSeguridad()
    {
        ValidacionRespaldoDto v = await ValidarElegido();
        using var cts = new CancellationTokenSource();
        _sesion.AlVerificar = nombre => { if (nombre.Contains("AUTO")) cts.Cancel(); };

        RespuestaRestauracion r = await _negocio.EjecutarRestauracionAsync(Rol, v, null, cts.Token);

        Assert.Equal(ResultadoRespaldo.Cancelado, r.Resultado);
        Assert.NotNull(r.RutaBackupSeguridad);
        Assert.DoesNotContain("SINGLE_USER", _sesion.Llamadas);
        Assert.DoesNotContain(_sesion.Llamadas, l => l.StartsWith("RESTORE"));
        Assert.True(_sesion.Liberada);
        await AssertSemaforoLibre();
    }

    [Fact]
    public async Task Restaurar_CanceladoAntesDeEmpezar_NoToca()
    {
        ValidacionRespaldoDto v = await ValidarElegido();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        RespuestaRestauracion r = await _negocio.EjecutarRestauracionAsync(Rol, v, null, cts.Token);

        Assert.Equal(ResultadoRespaldo.Cancelado, r.Resultado);
        Assert.Empty(_sesion.Llamadas);
        await AssertSemaforoLibre();
    }

    [Fact]
    public async Task Restaurar_TrasIniciarFaseDestructiva_IgnoraLaCancelacion()
    {
        ValidacionRespaldoDto v = await ValidarElegido();
        using var cts = new CancellationTokenSource();

        // La UI/otro hilo solicita cancelar justo cuando comienza la fase destructiva.
        RespuestaRestauracion r = await _negocio.EjecutarRestauracionAsync(
            Rol, v, null, cts.Token, () => cts.Cancel());

        Assert.Equal(ResultadoRespaldo.Exito, r.Resultado);
        Assert.Equal(new[] { "SINGLE_USER", "RESTORE:2", "MULTI_USER" },
            _sesion.Llamadas.Where(l => l is "SINGLE_USER" or "MULTI_USER" || l.StartsWith("RESTORE:")).ToArray());
        Assert.False(_sesion.TokenSingleUser.CanBeCanceled);
        Assert.False(_sesion.TokenRestore.CanBeCanceled);
        Assert.True(_sesion.Liberada);
        await AssertSemaforoLibre();
    }

    // ---------- localizador de la carpeta ----------

    [Fact]
    public void ProveedorRuta_ConSlnxFalso_CreaBackupRestoreEnLaRaiz()
    {
        string raiz = Path.Combine(_carpeta, "raiz");
        string profundo = Path.Combine(raiz, "bin", "Debug", "net10.0");
        Directory.CreateDirectory(profundo);
        File.WriteAllText(Path.Combine(raiz, ProveedorRutaRespaldo.ArchivoRaiz), "");

        ResultadoRutaRespaldo r = new ProveedorRutaRespaldo(profundo).ObtenerCarpeta();

        Assert.True(r.Disponible);
        Assert.Equal(Path.Combine(raiz, "backup_restore"), r.Carpeta);
        Assert.True(Directory.Exists(r.Carpeta));
    }

    [Fact]
    public void ProveedorRuta_SinSlnx_NoDisponibleConMensaje()
    {
        string sin = Path.Combine(_carpeta, "sin", "slnx");
        Directory.CreateDirectory(sin);

        ResultadoRutaRespaldo r = new ProveedorRutaRespaldo(sin).ObtenerCarpeta();

        Assert.False(r.Disponible);
        Assert.Null(r.Carpeta);
        Assert.Equal(ProveedorRutaRespaldo.MensajeSinRaiz, r.Mensaje);
    }

    // ---------- ayudas ----------

    private async Task<ValidacionRespaldoDto> ValidarElegido()
    {
        string ruta = AgregarBackup("DB_X_20261101_080000_MANUAL.bak", new DateTime(2026, 11, 1, 8, 0, 0));
        ValidacionRespaldoDto v = await _negocio.ValidarParaRestauracionAsync(Rol, ruta);
        Assert.Equal(ResultadoRespaldo.Exito, v.Resultado);
        _sesion.Llamadas.Clear();
        return v;
    }

    private string CrearArchivo(string nombre)
    {
        string ruta = Path.Combine(_carpeta, nombre);
        File.WriteAllText(ruta, "contenido");
        return ruta;
    }

    private string AgregarBackup(string nombre, DateTime fin, DateTime? mtime = null, bool otraBase = false)
    {
        string ruta = CrearArchivo(nombre);
        if (mtime is not null)
        {
            File.SetLastWriteTimeUtc(ruta, mtime.Value);
        }

        _sesion.Cabeceras[nombre] = new FilaMetadatoBackup
        {
            DatabaseName = otraBase ? "OTRA_BD" : Db,
            BackupType = 1,
            BackupFinishDate = fin,
            BackupSetGUID = Guid.NewGuid(),
            HasBackupChecksums = true
        };
        return ruta;
    }

    private async Task AssertSemaforoLibre()
    {
        using IDisposable? entrada = await _exclusion.IntentarEntrarAsync(default);
        Assert.NotNull(entrada);
    }

    // ---------- dobles de prueba ----------

    private sealed class RelojFijo : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 11, 11, 10, 35, 7, TimeSpan.Zero);

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    private sealed class RutaFalsa : IProveedorRutaRespaldo
    {
        private readonly string? _carpeta;

        public RutaFalsa(string? carpeta) => _carpeta = carpeta;

        public ResultadoRutaRespaldo ObtenerCarpeta() => _carpeta is null
            ? new ResultadoRutaRespaldo(false, null, ProveedorRutaRespaldo.MensajeSinRaiz)
            : new ResultadoRutaRespaldo(true, _carpeta, string.Empty);
    }

    private sealed class RepositorioFalso : IRespaldoRepositorio
    {
        private readonly SesionFalsa _sesion;

        public RepositorioFalso(SesionFalsa sesion) => _sesion = sesion;

        public int CodigoLock { get; set; }

        public int SesionesAbiertas { get; private set; }

        public string NombreBaseDatos => Db;

        public Task<ISesionRespaldo> AbrirSesionAsync(CancellationToken ct)
        {
            SesionesAbiertas++;
            return Task.FromResult<ISesionRespaldo>(_sesion);
        }

        public Task<AperturaSesionExclusiva> AbrirSesionExclusivaAsync(CancellationToken ct)
        {
            SesionesAbiertas++;
            return Task.FromResult(new AperturaSesionExclusiva(CodigoLock >= 0 ? _sesion : null, CodigoLock));
        }
    }

    private sealed class SesionFalsa : ISesionRespaldo
    {
        public List<string> Llamadas { get; } = new();

        public Dictionary<string, FilaMetadatoBackup> Cabeceras { get; } = new();

        public HashSet<string> Ilegibles { get; } = new();

        public bool Liberada { get; private set; }

        public Exception? FalloBackup { get; set; }

        public Action<string, CancellationToken>? AlBackup { get; set; }

        public Action<string>? AlVerificar { get; set; }

        public CancellationToken TokenSingleUser { get; private set; }

        public CancellationToken TokenRestore { get; private set; }

        public Exception? FalloVerificar { get; set; }

        public bool FalloVerificarSeguridad { get; set; }

        public Exception? FalloRestore { get; set; }

        public EstadoBaseDatos Estado { get; set; } = new("ONLINE", "MULTI_USER", false);

        public EstadoBaseDatos? EstadoTrasRecuperar { get; set; } = new("ONLINE", "MULTI_USER", false);

        private static readonly FilaArchivoBackup[] Archivos =
        {
            new() { LogicalName = "DB_X", PhysicalName = @"C:\datos\DB_X.mdf", Type = "D" },
            new() { LogicalName = "DB_X_log", PhysicalName = @"C:\datos\DB_X_log.ldf", Type = "L" }
        };

        public Task EjecutarBackupAsync(string rutaArchivo, string nombreBackup, string descripcion, CancellationToken ct)
        {
            Llamadas.Add("BACKUP:" + Path.GetFileName(rutaArchivo));
            AlBackup?.Invoke(Path.GetFileName(rutaArchivo), ct);
            if (FalloBackup is not null)
            {
                throw FalloBackup;
            }

            return Task.CompletedTask;
        }

        public Task VerificarBackupAsync(string rutaArchivo, CancellationToken ct)
        {
            string nombre = Path.GetFileName(rutaArchivo);
            Llamadas.Add("VERIFY:" + nombre);
            AlVerificar?.Invoke(nombre);
            if (nombre.Contains("AUTO") && FalloVerificarSeguridad)
            {
                throw new RespaldoBdException(3241, new Exception());
            }

            if (FalloVerificar is not null && !Ilegibles.Contains(nombre))
            {
                throw FalloVerificar;
            }

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<FilaMetadatoBackup>> LeerCabecerasAsync(string rutaArchivo, CancellationToken ct)
        {
            string nombre = Path.GetFileName(rutaArchivo);
            if (Ilegibles.Contains(nombre) || !Cabeceras.TryGetValue(nombre, out FilaMetadatoBackup? fila))
            {
                throw new RespaldoBdException(3241, new Exception());
            }

            return Task.FromResult<IReadOnlyList<FilaMetadatoBackup>>(new[] { fila });
        }

        public Task<IReadOnlyList<FilaArchivoBackup>> LeerListaArchivosAsync(string rutaArchivo, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<FilaArchivoBackup>>(Archivos);

        public Task<IReadOnlyList<FilaArchivoBackup>> LeerArchivosActualesAsync(CancellationToken ct)
            => Task.FromResult<IReadOnlyList<FilaArchivoBackup>>(Archivos);

        public Task<EstadoBaseDatos?> LeerEstadoAsync(CancellationToken ct) => Task.FromResult<EstadoBaseDatos?>(Estado);

        public Task EstablecerUsuarioUnicoAsync(CancellationToken ct)
        {
            Llamadas.Add("SINGLE_USER");
            TokenSingleUser = ct;
            return Task.CompletedTask;
        }

        public Task RestaurarAsync(string rutaArchivo, IReadOnlyList<MapeoArchivo> mapeo, CancellationToken ct)
        {
            Llamadas.Add("RESTORE:" + mapeo.Count);
            TokenRestore = ct;
            return FalloRestore is not null ? Task.FromException(FalloRestore) : Task.CompletedTask;
        }

        public Task<EstadoBaseDatos?> RecuperarMultiUsuarioAsync()
        {
            Llamadas.Add("MULTI_USER");
            return Task.FromResult(EstadoTrasRecuperar);
        }

        public ValueTask DisposeAsync()
        {
            Liberada = true;
            return ValueTask.CompletedTask;
        }
    }
}
