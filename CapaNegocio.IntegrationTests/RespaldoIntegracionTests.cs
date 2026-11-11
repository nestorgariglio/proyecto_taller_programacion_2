using CapaDatos.Respaldo;
using CapaNegocio.DTOs.Respaldos;
using CapaNegocio.Enums;
using CapaNegocio.Respaldo;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace CapaNegocio.IntegrationTests;

/// Integración de respaldo/restauración. SOLO contra DB_SISTEMA_VENTA_TEST: antes de CADA
/// BACKUP/RESTORE/SINGLE_USER se comprueba servidor y DB_NAME(); si no coincide, se aborta sin operar.
/// Nunca imprime la cadena de conexión. Los permisos de carpeta los gestiona el usuario: ante
/// "acceso denegado" la prueba se informa como BLOQUEADA, sin tocar permisos.
[Collection(ColeccionBdIntegracion.Nombre)]
public sealed class RespaldoIntegracionTests : IDisposable
{
    private const string CuentaServicio = @"NT Service\MSSQL$SQLEXPRESS";

    /// Cantidad de veces que la guarda de RESTORE (solo sesión de master) se ejecutó y autorizó.
    internal static int GuardasRestoreEjecutadas;

    private readonly BdIntegracionFixture _fx;
    private readonly ITestOutputHelper _salida;
    private readonly string? _carpeta;

    public RespaldoIntegracionTests(BdIntegracionFixture fx, ITestOutputHelper salida)
    {
        _fx = fx;
        _salida = salida;

        if (Environment.GetEnvironmentVariable(FactIntegracionAttribute.VariableActivacion) == "1"
            && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(FactIntegracionAttribute.VariableConexion)))
        {
            // Subcarpeta de la carpeta real de respaldos: hereda sus permisos, no se modifica ninguno.
            ResultadoRutaRespaldo raiz = new ProveedorRutaRespaldo().ObtenerCarpeta();
            if (raiz.Disponible)
            {
                _carpeta = Path.Combine(raiz.Carpeta!, "it_g70_" + Guid.NewGuid().ToString("N")[..8]);
                Directory.CreateDirectory(_carpeta);
            }
        }
    }

    public void Dispose()
    {
        // Limpieza de los archivos de la propia prueba (no es una operación de la aplicación).
        if (_carpeta is not null && Directory.Exists(_carpeta))
        {
            try { Directory.Delete(_carpeta, true); } catch (IOException) { }
        }
    }

    [FactIntegracion]
    public async Task Backup_real_se_genera_y_verifica_y_se_lista()
    {
        RespaldoNegocio negocio = CrearNegocio();

        RespuestaGeneracionRespaldo r = await negocio.GenerarBackupAsync("Administrador");
        AbortarSiBloqueada(r.Resultado, r.Mensaje);

        Assert.Equal(ResultadoRespaldo.Exito, r.Resultado);
        Assert.True(File.Exists(r.RutaArchivo));

        RespuestaListadoRespaldos lista = await negocio.ListarRespaldosAsync("Administrador");
        Assert.Contains(lista.Respaldos, b => b.RutaArchivo == r.RutaArchivo && b.Tipo == "Manual");
    }

    [FactIntegracion]
    public async Task Restore_real_devuelve_el_estado_anterior_con_ONLINE_y_MULTI_USER()
    {
        bool asincronas = await EstadisticasAsincronasActivasAsync();
        _salida.WriteLine($"is_auto_update_stats_async_on de {GuardaConexion.BasePermitida} = {asincronas}");
        if (asincronas)
        {
            Assert.Fail("BLOQUEADA: is_auto_update_stats_async_on está activo en la BD de pruebas; el RESTORE real no puede ejecutarse. No se modificó la configuración.");
        }

        RespaldoNegocio negocio = CrearNegocio();
        RespuestaGeneracionRespaldo gen = await negocio.GenerarBackupAsync("Administrador");
        AbortarSiBloqueada(gen.Resultado, gen.Mensaje);
        Assert.Equal(ResultadoRespaldo.Exito, gen.Resultado);

        // Cambio de datos posterior al backup (fila con marcador propio).
        string descripcion = BdIntegracionFixture.Marcador + "Restore_" + Guid.NewGuid().ToString("N")[..6];
        await using (var db = _fx.CrearContexto())
        {
            db.Categorias.Add(new CapaEntidad.Categoria { Descripcion = descripcion, Estado = true });
            await db.SaveChangesAsync();
        }

        Assert.Equal(1, await ContarCategoriasAsync(descripcion));

        ValidacionRespaldoDto v = await negocio.ValidarParaRestauracionAsync("Administrador", gen.RutaArchivo!);
        Assert.Equal(ResultadoRespaldo.Exito, v.Resultado);

        RespuestaRestauracion res = await negocio.EjecutarRestauracionAsync("Administrador", v);
        AbortarSiBloqueada(res.Resultado, res.Mensaje);

        Assert.True(res.Resultado == ResultadoRespaldo.Exito, res.Mensaje);
        Assert.True(File.Exists(res.RutaBackupSeguridad));
        Assert.Equal(0, await ContarCategoriasAsync(descripcion));
        Assert.Equal("ONLINE / MULTI_USER", res.EstadoAccesoFinal);

        List<object?[]> estado = await _fx.ConsultarAsync(
            "SELECT state_desc, user_access_desc FROM sys.databases WHERE name = DB_NAME()", default);
        Assert.Equal("ONLINE", estado[0][0]);
        Assert.Equal("MULTI_USER", estado[0][1]);
    }

    [FactIntegracion]
    public async Task Archivo_alterado_y_backup_de_otra_BD_son_rechazados()
    {
        RespaldoNegocio negocio = CrearNegocio();
        RespuestaGeneracionRespaldo gen = await negocio.GenerarBackupAsync("Administrador");
        AbortarSiBloqueada(gen.Resultado, gen.Mensaje);
        Assert.Equal(ResultadoRespaldo.Exito, gen.Resultado);

        // Archivo alterado: copia con bytes del medio invertidos (la suma de verificación debe fallar).
        string alterado = Path.Combine(_carpeta!, "alterado.bak");
        File.Copy(gen.RutaArchivo!, alterado);
        await using (var fs = new FileStream(alterado, FileMode.Open, FileAccess.ReadWrite))
        {
            fs.Position = fs.Length / 2;
            var buffer = new byte[4096];
            int leidos = await fs.ReadAsync(buffer);
            for (int i = 0; i < leidos; i++) buffer[i] ^= 0xFF;
            fs.Position = fs.Length / 2;
            await fs.WriteAsync(buffer.AsMemory(0, leidos));
        }

        ValidacionRespaldoDto vAlterado = await negocio.ValidarParaRestauracionAsync("Administrador", alterado);
        Assert.Equal(ResultadoRespaldo.ArchivoInvalido, vAlterado.Resultado);

        // Backup de otra BD: el mismo archivo válido evaluado por un negocio cuya base objetivo es otra.
        var otra = new RespaldoNegocio(new RepositorioOtraBase(CrearRepositorio()), new RutaPrueba(_carpeta!), new ExclusionRespaldo());
        ValidacionRespaldoDto vOtra = await otra.ValidarParaRestauracionAsync("Administrador", gen.RutaArchivo!);
        Assert.Equal(ResultadoRespaldo.ArchivoInvalido, vOtra.Resultado);
    }

    [FactIntegracion]
    public async Task Dos_sesiones_la_segunda_recibe_menos1_y_el_lock_se_libera_tras_exito_y_tras_fallo()
    {
        IRespaldoRepositorio repo = CrearRepositorio();
        await GuardaConexion.VerificarBaseConectadaAsync(_fx.Cadena, default);

        AperturaSesionExclusiva primera = await repo.AbrirSesionExclusivaAsync(default);
        Assert.NotNull(primera.Sesion);

        AperturaSesionExclusiva segunda = await repo.AbrirSesionExclusivaAsync(default);
        Assert.Equal(-1, segunda.CodigoApplock);
        Assert.Null(segunda.Sesion);

        await primera.Sesion!.DisposeAsync(); // fin "exitoso"

        AperturaSesionExclusiva tercera = await repo.AbrirSesionExclusivaAsync(default);
        Assert.NotNull(tercera.Sesion);

        // Fin con fallo: un BACKUP a una ruta inexistente falla y la sesión se libera igual.
        await Assert.ThrowsAsync<RespaldoBdException>(() =>
            tercera.Sesion!.EjecutarBackupAsync(@"Z:\g70_no_existe\x.bak", "x", "x", default));
        await tercera.Sesion!.DisposeAsync();

        AperturaSesionExclusiva cuarta = await repo.AbrirSesionExclusivaAsync(default);
        Assert.NotNull(cuarta.Sesion);
        await cuarta.Sesion!.DisposeAsync();
    }

    [FactIntegracion]
    public async Task Lee_sin_modificar_is_auto_update_stats_async_on()
    {
        bool activo = await EstadisticasAsincronasActivasAsync();
        _salida.WriteLine(activo
            ? "is_auto_update_stats_async_on = 1: el RESTORE real queda BLOQUEADO (no se modificó)."
            : "is_auto_update_stats_async_on = 0: el RESTORE real puede ejecutarse.");
    }

    [FactIntegracion]
    public async Task Integral_backup_manual_cambios_y_restauracion_completa_devuelve_el_estado_registrado()
    {
        // Guarda previa: servidor y DB_NAME() exactos (además de la guarda en cada BACKUP/RESTORE).
        await GuardaConexion.VerificarBaseConectadaAsync(_fx.Cadena, default);
        if (await EstadisticasAsincronasActivasAsync())
        {
            Assert.Fail("BLOQUEADA: is_auto_update_stats_async_on está activo en la BD de pruebas. No se modificó la configuración.");
        }

        RespaldoNegocio negocio = CrearNegocio();

        // (1) Estado inicial real.
        EstadoDatos inicial = await LeerEstadoDatosAsync();
        _salida.WriteLine("ANTES      : " + inicial);

        // (2) Backup manual completo real, existencia, metadatos (HEADERONLY) e integridad.
        RespuestaGeneracionRespaldo gen = await negocio.GenerarBackupAsync("Administrador");
        AbortarSiBloqueada(gen.Resultado, gen.Mensaje);
        Assert.Equal(ResultadoRespaldo.Exito, gen.Resultado);
        Assert.True(File.Exists(gen.RutaArchivo));
        await using (ISesionRespaldo sesion = await CrearRepositorio().AbrirSesionAsync(default))
        {
            IReadOnlyList<FilaMetadatoBackup> cab = await sesion.LeerCabecerasAsync(gen.RutaArchivo!, default);
            Assert.Single(cab);
            Assert.Equal(GuardaConexion.BasePermitida, cab[0].DatabaseName, ignoreCase: true);
            Assert.Equal(1, cab[0].BackupType);
            Assert.True(cab[0].HasBackupChecksums);
            await sesion.VerificarBackupAsync(gen.RutaArchivo!, default); // no lanza = íntegro
            _salida.WriteLine($"BACKUP     : {Path.GetFileName(gen.RutaArchivo)} HEADERONLY db={cab[0].DatabaseName} tipo={cab[0].BackupType} checksum={cab[0].HasBackupChecksums} VERIFYONLY=OK");
        }

        // (3) Inserciones identificables respetando relaciones, más un cambio sobre un dato original.
        await GuardaConexion.VerificarBaseConectadaAsync(_fx.Cadena, default);
        Escenario esc = await _fx.CrearEscenarioAsync(default);
        CapaEntidad.Producto prod = await _fx.CrearProductoAsync(esc, 11, 5m, 9m, default);
        if (inicial.Productos.Count > 0)
        {
            await _fx.EstablecerStockAsync(inicial.Productos[0].Id, inicial.Productos[0].Stock + 7, default);
        }

        Assert.Equal(prod.Codigo, (await _fx.LeerProductoAsync(prod.IdProducto, default)).Codigo); // persistió

        // (4) El estado cambió.
        EstadoDatos modificado = await LeerEstadoDatosAsync();
        _salida.WriteLine("MODIFICADO  : " + modificado);
        Assert.NotEqual(inicial.ToString(), modificado.ToString());
        Assert.Equal(inicial.Conteos["Producto"] + 1, modificado.Conteos["Producto"]);
        Assert.Equal(inicial.Conteos["Categoria"] + 1, modificado.Conteos["Categoria"]);
        Assert.Equal(inicial.Conteos["Proveedor"] + 1, modificado.Conteos["Proveedor"]);
        Assert.Equal(inicial.Conteos["Usuario"] + 1, modificado.Conteos["Usuario"]);

        // (5) Restauración completa real: validar, semáforo + applock, revalidar, backup automático, SINGLE_USER, RESTORE, MULTI_USER.
        ValidacionRespaldoDto v = await negocio.ValidarParaRestauracionAsync("Administrador", gen.RutaArchivo!);
        Assert.Equal(ResultadoRespaldo.Exito, v.Resultado);
        RespuestaRestauracion res = await negocio.EjecutarRestauracionAsync("Administrador", v);
        AbortarSiBloqueada(res.Resultado, res.Mensaje);
        Assert.True(res.Resultado == ResultadoRespaldo.Exito, res.Mensaje);
        Assert.True(GuardasRestoreEjecutadas >= 1, "La guarda de RESTORE no se ejecutó.");
        _salida.WriteLine($"GUARDA RESTORE: ejecutada {GuardasRestoreEjecutadas} vez/veces (destino [{GuardaConexion.BasePermitida}] verificado, existe en sys.databases vía master).");

        // (6) Verificaciones.
        EstadoDatos final = await LeerEstadoDatosAsync();
        _salida.WriteLine("RESTAURADO : " + final);
        Assert.Equal(inicial.ToString(), final.ToString());
        Assert.Equal(0, await _fx.EscalarAsync($"SELECT COUNT(*) FROM dbo.PRODUCTO WHERE codigo = N'{prod.Codigo}'", default));
        List<object?[]> estado = await _fx.ConsultarAsync(
            "SELECT state_desc, user_access_desc FROM sys.databases WHERE name = DB_NAME()", default);
        Assert.Equal("ONLINE", estado[0][0]);
        Assert.Equal("MULTI_USER", estado[0][1]);
        _salida.WriteLine($"BD         : {estado[0][0]} / {estado[0][1]}; EstadoAccesoFinal={res.EstadoAccesoFinal}");

        // Sin locks residuales: un nuevo applock exclusivo se concede y se libera.
        AperturaSesionExclusiva lockNuevo = await CrearRepositorio().AbrirSesionExclusivaAsync(default);
        Assert.NotNull(lockNuevo.Sesion);
        await lockNuevo.Sesion!.DisposeAsync();

        // Conservación de ambos .bak (manual y automático).
        Assert.True(File.Exists(gen.RutaArchivo));
        Assert.True(File.Exists(res.RutaBackupSeguridad));
        RespuestaListadoRespaldos lista = await negocio.ListarRespaldosAsync("Administrador");
        Assert.Contains(lista.Respaldos, b => b.RutaArchivo == gen.RutaArchivo && b.Tipo == "Manual");
        Assert.Contains(lista.Respaldos, b => b.RutaArchivo == res.RutaBackupSeguridad && b.Tipo == "Automático");
        _salida.WriteLine($"ARCHIVOS   : manual={Path.GetFileName(gen.RutaArchivo)} auto={Path.GetFileName(res.RutaBackupSeguridad)} (existen y figuran en el listado)");
    }

    // ---------- estado de datos ----------

    private sealed record FilaProducto(int Id, string Codigo, string Nombre, int Stock, decimal PrecioCompra, decimal PrecioVenta);

    private sealed record EstadoDatos(IReadOnlyDictionary<string, int> Conteos, IReadOnlyList<FilaProducto> Productos, int StockTotal)
    {
        public override string ToString()
            => string.Join(", ", Conteos.Select(k => $"{k.Key}={k.Value}"))
               + $"; stockTotal={StockTotal}; muestra=[" + string.Join(" | ", Productos.Select(p => $"{p.Id}:{p.Codigo}:{p.Stock}:{p.PrecioCompra}:{p.PrecioVenta}")) + "]";
    }

    private async Task<EstadoDatos> LeerEstadoDatosAsync()
    {
        await using var db = _fx.CrearContexto();
        var c = new SortedDictionary<string, int>
        {
            ["Rol"] = await db.Roles.CountAsync(),
            ["Usuario"] = await db.Usuarios.CountAsync(),
            ["Cliente"] = await db.Clientes.CountAsync(),
            ["Proveedor"] = await db.Proveedores.CountAsync(),
            ["Categoria"] = await db.Categorias.CountAsync(),
            ["Producto"] = await db.Productos.CountAsync(),
            ["Compra"] = await db.Compras.CountAsync(),
            ["DetalleCompra"] = await db.DetallesCompra.CountAsync(),
            ["Venta"] = await db.Ventas.CountAsync(),
            ["DetalleVenta"] = await db.DetallesVenta.CountAsync()
        };

        // Muestra de datos originales (excluye los sintéticos IT_G70_ de las pruebas).
        List<FilaProducto> muestra = await db.Productos.AsNoTracking()
            .Where(p => !p.Codigo.StartsWith(BdIntegracionFixture.Marcador))
            .OrderBy(p => p.IdProducto).Take(3)
            .Select(p => new FilaProducto(p.IdProducto, p.Codigo, p.Nombre, p.Stock, p.PrecioCompra, p.PrecioVenta))
            .ToListAsync();

        int stockTotal = await db.Productos.Where(p => !p.Codigo.StartsWith(BdIntegracionFixture.Marcador)).SumAsync(p => p.Stock);

        return new EstadoDatos(c, muestra, stockTotal);
    }

    // ---------- ayudas ----------

    private RespaldoNegocio CrearNegocio()
        => new(CrearRepositorio(), new RutaPrueba(_carpeta ?? throw new InvalidOperationException("Carpeta de respaldos no disponible.")), new ExclusionRespaldo());

    private IRespaldoRepositorio CrearRepositorio() => new RepositorioGuardado(new RespaldoRepositorio(_fx.Cadena), _fx.Cadena);

    private async Task<bool> EstadisticasAsincronasActivasAsync()
    {
        List<object?[]> f = await _fx.ConsultarAsync(
            "SELECT CAST(is_auto_update_stats_async_on AS int) FROM sys.databases WHERE name = DB_NAME()", default);
        return Convert.ToInt32(f[0][0]) == 1;
    }

    private async Task<int> ContarCategoriasAsync(string descripcion)
    {
        await using var db = _fx.CrearContexto();
        return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(
            db.Categorias, c => c.Descripcion == descripcion);
    }

    private void AbortarSiBloqueada(ResultadoRespaldo resultado, string mensaje)
    {
        if (resultado == ResultadoRespaldo.RutaNoDisponible || resultado == ResultadoRespaldo.SinPermiso)
        {
            Assert.Fail($"BLOQUEADA por permisos de carpeta/SQL: la cuenta de servicio {CuentaServicio} debe poder escribir en '{_carpeta}'. Detalle: {mensaje}");
        }
    }

    private sealed class RutaPrueba : IProveedorRutaRespaldo
    {
        private readonly string _carpeta;

        public RutaPrueba(string carpeta) => _carpeta = carpeta;

        public ResultadoRutaRespaldo ObtenerCarpeta() => new(true, _carpeta, string.Empty);
    }

    /// Decoradores con la guarda: servidor y DB_NAME() exactos antes de cada BACKUP/RESTORE/SINGLE_USER.
    private sealed class RepositorioGuardado : IRespaldoRepositorio
    {
        private readonly IRespaldoRepositorio _interno;
        private readonly string _cadena;

        public RepositorioGuardado(IRespaldoRepositorio interno, string cadena)
        {
            _interno = interno;
            _cadena = cadena;
        }

        public string NombreBaseDatos => _interno.NombreBaseDatos;

        public async Task<ISesionRespaldo> AbrirSesionAsync(CancellationToken ct)
            => new SesionGuardada(await _interno.AbrirSesionAsync(ct), _cadena, _interno.NombreBaseDatos);

        public async Task<AperturaSesionExclusiva> AbrirSesionExclusivaAsync(CancellationToken ct)
        {
            AperturaSesionExclusiva a = await _interno.AbrirSesionExclusivaAsync(ct);
            return a.Sesion is null ? a : a with { Sesion = new SesionGuardada(a.Sesion, _cadena, _interno.NombreBaseDatos) };
        }
    }

    private sealed class RepositorioOtraBase : IRespaldoRepositorio
    {
        private readonly IRespaldoRepositorio _interno;

        public RepositorioOtraBase(IRespaldoRepositorio interno) => _interno = interno;

        public string NombreBaseDatos => "OTRA_BD_AJENA";

        public Task<ISesionRespaldo> AbrirSesionAsync(CancellationToken ct) => _interno.AbrirSesionAsync(ct);

        public Task<AperturaSesionExclusiva> AbrirSesionExclusivaAsync(CancellationToken ct) => _interno.AbrirSesionExclusivaAsync(ct);
    }

    private sealed class SesionGuardada : ISesionRespaldo
    {
        private readonly ISesionRespaldo _i;
        private readonly string _cadena;
        private readonly string _nombreBase;

        public SesionGuardada(ISesionRespaldo interna, string cadena, string nombreBase)
        {
            _i = interna;
            _cadena = cadena;
            _nombreBase = nombreBase;
        }

        private async Task GuardarAsync(CancellationToken ct)
        {
            GuardaConexion.ObtenerCadenaValidada();
            await GuardaConexion.VerificarBaseConectadaAsync(_cadena, ct);
        }

        public async Task EjecutarBackupAsync(string rutaArchivo, string nombreBackup, string descripcion, CancellationToken ct)
        {
            await GuardarAsync(ct);
            await _i.EjecutarBackupAsync(rutaArchivo, nombreBackup, descripcion, ct);
        }

        public Task VerificarBackupAsync(string rutaArchivo, CancellationToken ct) => _i.VerificarBackupAsync(rutaArchivo, ct);

        public Task<IReadOnlyList<FilaMetadatoBackup>> LeerCabecerasAsync(string rutaArchivo, CancellationToken ct) => _i.LeerCabecerasAsync(rutaArchivo, ct);

        public Task<IReadOnlyList<FilaArchivoBackup>> LeerListaArchivosAsync(string rutaArchivo, CancellationToken ct) => _i.LeerListaArchivosAsync(rutaArchivo, ct);

        public Task<IReadOnlyList<FilaArchivoBackup>> LeerArchivosActualesAsync(CancellationToken ct) => _i.LeerArchivosActualesAsync(ct);

        public Task<EstadoBaseDatos?> LeerEstadoAsync(CancellationToken ct) => _i.LeerEstadoAsync(ct);

        public async Task EstablecerUsuarioUnicoAsync(CancellationToken ct)
        {
            await GuardarAsync(ct);
            await _i.EstablecerUsuarioUnicoAsync(ct);
        }

        public async Task RestaurarAsync(string rutaArchivo, IReadOnlyList<MapeoArchivo> mapeo, CancellationToken ct)
        {
            // La base ya está en SINGLE_USER: NO se abre ninguna conexión a ella (agotaría el tiempo de
            // espera). La guarda usa solo la sesión de master ya abierta (LeerEstadoAsync) y datos en memoria.
            GuardaConexion.ObtenerCadenaValidada(); // servidor/base exactos de la cadena (sin conexión)
            string destino = "[" + _nombreBase.Replace("]", "]]") + "]"; // idéntico al que emite RESTORE DATABASE
            if (!string.Equals(destino, "[" + GuardaConexion.BasePermitida + "]", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Destino de RESTORE rechazado: no es exactamente {GuardaConexion.BasePermitida}.");
            }

            EstadoBaseDatos? estado = await _i.LeerEstadoAsync(ct); // sys.databases vía master
            if (estado is null)
            {
                throw new InvalidOperationException($"Destino de RESTORE rechazado: {GuardaConexion.BasePermitida} no existe en sys.databases.");
            }

            Interlocked.Increment(ref GuardasRestoreEjecutadas);
            await _i.RestaurarAsync(rutaArchivo, mapeo, ct);
        }

        public Task<EstadoBaseDatos?> RecuperarMultiUsuarioAsync() => _i.RecuperarMultiUsuarioAsync();

        public ValueTask DisposeAsync() => _i.DisposeAsync();
    }
}
