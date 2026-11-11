using System.Globalization;
using CapaDatos.Respaldo;
using CapaNegocio.DTOs.Respaldos;
using CapaNegocio.Enums;
using CapaNegocio.Respaldo;

namespace CapaNegocio;

/// <summary>
/// Reglas del respaldo y la restauración de la base de datos configurada.
/// </summary>
/// <remarks>
/// Autorización: todas las operaciones públicas validan el rol con
/// <see cref="PoliticaAcceso.PuedeGestionarRespaldos"/>. Esa regla es de la
/// aplicación y NO otorga permisos SQL: con autenticación integrada, el usuario de
/// Windows que ejecuta G70 debe tener permisos SQL propios (BACKUP DATABASE, ALTER
/// sobre la base, CREATE DATABASE / dbcreator para RESTORE WITH REPLACE y acceso a
/// master).
/// Excepción al principio 5 de la constitución: BACKUP y RESTORE no admiten
/// transacción de usuario, así que no se usan transacciones explícitas; la
/// seguridad se obtiene con backup previo verificado, revalidación por huella y
/// recuperación de MULTI_USER.
/// Concurrencia: el semáforo (<see cref="IExclusionRespaldo"/>) protege dentro del
/// proceso y <c>sp_getapplock</c> entre instancias de G70; SINGLE_USER no sustituye
/// esa coordinación. El semáforo nunca se mantiene durante el diálogo de
/// confirmación de la UI.
/// </remarks>
public class RespaldoNegocio
{
    /// <summary>Máximo de respaldos listados.</summary>
    public const int MaximoRespaldosListados = 5;

    public const string MensajeSinRespaldos = "No hay ningún backup disponible";

    public const string MensajeSinPermisoRol =
        "No tiene permiso para gestionar respaldos. Esta función solo está disponible para Administrador o Encargado.";

    public const string MensajeEstadisticasAsincronas =
        "No se puede restaurar mientras la actualización asíncrona de estadísticas esté activa en la base de datos, porque puede impedir el modo de usuario único. Pida a un administrador de la base de datos que la desactive y vuelva a intentar.";

    public const string MensajeOtraInstancia = "Otra instancia de G70 está operando sobre los respaldos. Espere a que termine e intente nuevamente.";

    public const string MensajeCancelado = "La operación fue cancelada.";

    public const string MensajeCanceladoSinCambios = "La operación fue cancelada. No se modificó la base de datos.";

    public const string MensajeOperacionEnCurso = "Ya hay una operación de respaldo o restauración en curso. Espere a que termine.";

    private const string SufijoManual = "_MANUAL.bak";
    private const string SufijoAutomatico = "_AUTO-PRE-RESTAURACION.bak";

    private readonly IRespaldoRepositorio _repositorio;
    private readonly IProveedorRutaRespaldo _rutas;
    private readonly IExclusionRespaldo _exclusion;
    private readonly TimeProvider _reloj;

    public RespaldoNegocio(
        IRespaldoRepositorio repositorio,
        IProveedorRutaRespaldo rutas,
        IExclusionRespaldo exclusion,
        TimeProvider? reloj = null)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        _rutas = rutas ?? throw new ArgumentNullException(nameof(rutas));
        _exclusion = exclusion ?? throw new ArgumentNullException(nameof(exclusion));
        _reloj = reloj ?? TimeProvider.System;
    }

    // ---------------------------------------------------------------- textos y nombres

    /// <summary>Nombre de archivo: <c>&lt;DB&gt;_yyyyMMdd_HHmmss_MANUAL.bak</c> o <c>..._AUTO-PRE-RESTAURACION.bak</c>.</summary>
    public static string ConstruirNombreArchivo(string baseDatos, DateTime fecha, bool automatico)
    {
        string seguro = string.Concat(baseDatos.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        return seguro + "_" + fecha.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture)
            + (automatico ? SufijoAutomatico : SufijoManual);
    }

    /// <summary>Texto de confirmación mostrado antes de restaurar (con fecha/hora exactas del backup).</summary>
    public static string ConstruirTextoConfirmacion(RespaldoDisponibleDto respaldo)
    {
        ArgumentNullException.ThrowIfNull(respaldo);
        DateTime f = respaldo.FechaBackup;
        return $"Se restaurará la base de datos al estado del {f.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} "
            + $"a las {f.ToString("HH:mm", CultureInfo.InvariantCulture)}, utilizando el respaldo {respaldo.NombreArchivo}. "
            + "Esta operación reemplazará el estado actual de la base de datos. "
            + "Los cambios posteriores a esa fecha y hora se perderán. ¿Está seguro de que desea continuar?";
    }

    // ---------------------------------------------------------------- listado

    /// <summary>
    /// Lista hasta 5 respaldos válidos (manuales y automáticos), ordenados por la
    /// fecha real del backup (BackupFinishDate) descendente. Lee el encabezado de
    /// TODOS los .bak sin pre-ordenar por fecha de archivo.
    /// </summary>
    public async Task<RespuestaListadoRespaldos> ListarRespaldosAsync(
        string? rol, IProgress<string>? progreso = null, CancellationToken ct = default)
    {
        if (!PoliticaAcceso.PuedeGestionarRespaldos(rol))
        {
            return new RespuestaListadoRespaldos { Resultado = ResultadoRespaldo.SinPermiso, Mensaje = MensajeSinPermisoRol };
        }

        ResultadoRutaRespaldo ruta = _rutas.ObtenerCarpeta();
        if (!ruta.Disponible || ruta.Carpeta is null)
        {
            return new RespuestaListadoRespaldos { Resultado = ResultadoRespaldo.RutaNoDisponible, Mensaje = ruta.Mensaje };
        }

        try
        {
            string[] archivos = await Task.Run(
                () => Directory.GetFiles(ruta.Carpeta, "*.bak", SearchOption.TopDirectoryOnly), ct);

            var validos = new List<RespaldoDisponibleDto>();
            int noLegibles = 0;

            if (archivos.Length > 0)
            {
                await using ISesionRespaldo sesion = await _repositorio.AbrirSesionAsync(ct);
                IReadOnlyList<FilaArchivoBackup> actuales = await sesion.LeerArchivosActualesAsync(ct);

                for (int i = 0; i < archivos.Length; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    progreso?.Report($"Leyendo respaldos ({i + 1} de {archivos.Length})...");

                    try
                    {
                        ArchivoValidado? validado = await ValidarArchivoAsync(sesion, archivos[i], actuales, ct);
                        if (validado is null)
                        {
                            noLegibles++;
                        }
                        else
                        {
                            validos.Add(validado.Dto);
                        }
                    }
                    catch (RespaldoBdException ex) when (!EsErrorDeAcceso(ex.NumeroError))
                    {
                        noLegibles++;
                    }
                    catch (IOException)
                    {
                        noLegibles++;
                    }
                }
            }

            List<RespaldoDisponibleDto> top = validos
                .OrderByDescending(r => r.FechaBackup)
                .Take(MaximoRespaldosListados)
                .ToList();

            string sufijo = noLegibles > 0
                ? (noLegibles == 1 ? " 1 archivo no legible." : $" {noLegibles} archivos no legibles.")
                : string.Empty;

            return new RespuestaListadoRespaldos
            {
                Resultado = top.Count == 0 ? ResultadoRespaldo.SinRespaldosDisponibles : ResultadoRespaldo.Exito,
                Mensaje = top.Count == 0 ? MensajeSinRespaldos + "." + sufijo : sufijo.Trim(),
                Respaldos = top,
                ArchivosNoLegibles = noLegibles
            };
        }
        catch (Exception ex)
        {
            (ResultadoRespaldo resultado, string mensaje) = Mapear(ex, ResultadoRespaldo.Fallo, "No se pudo leer la lista de respaldos.");
            return new RespuestaListadoRespaldos { Resultado = resultado, Mensaje = mensaje };
        }
    }

    // ---------------------------------------------------------------- generación

    /// <summary>
    /// Genera un backup manual: <c>BACKUP ... WITH CHECKSUM, INIT</c> y
    /// <c>RESTORE VERIFYONLY ... WITH CHECKSUM</c> dentro del semáforo y del lock de
    /// aplicación. Nunca sobrescribe ni borra archivos; éxito solo si ambos pasos pasan.
    /// </summary>
    public async Task<RespuestaGeneracionRespaldo> GenerarBackupAsync(string? rol, CancellationToken ct = default)
    {
        if (!PoliticaAcceso.PuedeGestionarRespaldos(rol))
        {
            return new RespuestaGeneracionRespaldo { Resultado = ResultadoRespaldo.SinPermiso, Mensaje = MensajeSinPermisoRol };
        }

        ResultadoRutaRespaldo ruta = _rutas.ObtenerCarpeta();
        if (!ruta.Disponible || ruta.Carpeta is null)
        {
            return new RespuestaGeneracionRespaldo { Resultado = ResultadoRespaldo.RutaNoDisponible, Mensaje = ruta.Mensaje };
        }

        IDisposable? salida;
        try
        {
            salida = await _exclusion.IntentarEntrarAsync(ct);
        }
        catch (OperationCanceledException)
        {
            return new RespuestaGeneracionRespaldo { Resultado = ResultadoRespaldo.Cancelado, Mensaje = MensajeCancelado };
        }

        if (salida is null)
        {
            return new RespuestaGeneracionRespaldo { Resultado = ResultadoRespaldo.OperacionEnCurso, Mensaje = MensajeOperacionEnCurso };
        }

        using (salida)
        {
            return await GenerarDentroDeExclusionAsync(ruta.Carpeta, ct);
        }
    }

    private async Task<RespuestaGeneracionRespaldo> GenerarDentroDeExclusionAsync(string carpeta, CancellationToken ct)
    {
        try
        {
            AperturaSesionExclusiva apertura = await _repositorio.AbrirSesionExclusivaAsync(ct);
            if (apertura.Sesion is null)
            {
                return new RespuestaGeneracionRespaldo
                {
                    Resultado = apertura.CodigoApplock == -1 ? ResultadoRespaldo.OperacionEnCurso : ResultadoRespaldo.Fallo,
                    Mensaje = apertura.CodigoApplock == -1 ? MensajeOtraInstancia : "No se pudo coordinar la operación de respaldo con la base de datos."
                };
            }

            await using ISesionRespaldo sesion = apertura.Sesion;

            DateTime ahora = _reloj.GetLocalNow().DateTime;
            string archivo = Path.Combine(carpeta, ConstruirNombreArchivo(_repositorio.NombreBaseDatos, ahora, automatico: false));
            if (File.Exists(archivo))
            {
                return new RespuestaGeneracionRespaldo
                {
                    Resultado = ResultadoRespaldo.Fallo,
                    Mensaje = "Ya existe un respaldo con ese nombre (no se sobrescribe). Intente nuevamente en unos segundos."
                };
            }

            await sesion.EjecutarBackupAsync(
                archivo,
                $"G70 {_repositorio.NombreBaseDatos} MANUAL {ahora.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}",
                "Backup manual generado desde G70", ct);
            await sesion.VerificarBackupAsync(archivo, ct);

            return new RespuestaGeneracionRespaldo
            {
                Resultado = ResultadoRespaldo.Exito,
                Mensaje = "El backup se generó y se verificó correctamente.",
                RutaArchivo = archivo
            };
        }
        catch (Exception ex)
        {
            (ResultadoRespaldo resultado, string mensaje) = Mapear(ex, ResultadoRespaldo.Fallo, "No se pudo generar el backup.");
            return new RespuestaGeneracionRespaldo { Resultado = resultado, Mensaje = mensaje };
        }
    }

    // ---------------------------------------------------------------- restauración

    /// <summary>
    /// Paso 1 de la restauración: valida el archivo elegido SIN tomar lock y devuelve su
    /// huella. La UI usa el resultado para pedir la confirmación.
    /// </summary>
    public async Task<ValidacionRespaldoDto> ValidarParaRestauracionAsync(
        string? rol, string rutaArchivo, CancellationToken ct = default)
    {
        if (!PoliticaAcceso.PuedeGestionarRespaldos(rol))
        {
            return new ValidacionRespaldoDto { Resultado = ResultadoRespaldo.SinPermiso, Mensaje = MensajeSinPermisoRol };
        }

        ResultadoRutaRespaldo ruta = _rutas.ObtenerCarpeta();
        if (!ruta.Disponible || ruta.Carpeta is null)
        {
            return new ValidacionRespaldoDto { Resultado = ResultadoRespaldo.RutaNoDisponible, Mensaje = ruta.Mensaje };
        }

        if (!EstaEnCarpeta(ruta.Carpeta, rutaArchivo))
        {
            return new ValidacionRespaldoDto
            {
                Resultado = ResultadoRespaldo.ArchivoInvalido,
                Mensaje = "El archivo no pertenece a la carpeta de respaldos del sistema."
            };
        }

        try
        {
            await using ISesionRespaldo sesion = await _repositorio.AbrirSesionAsync(ct);
            IReadOnlyList<FilaArchivoBackup> actuales = await sesion.LeerArchivosActualesAsync(ct);
            ArchivoValidado? validado = await ValidarArchivoAsync(sesion, Path.GetFullPath(rutaArchivo), actuales, ct);

            return validado is null
                ? new ValidacionRespaldoDto { Resultado = ResultadoRespaldo.ArchivoInvalido, Mensaje = MensajeArchivoInvalido }
                : new ValidacionRespaldoDto { Resultado = ResultadoRespaldo.Exito, Respaldo = validado.Dto };
        }
        catch (RespaldoBdException ex) when (!EsErrorDeAcceso(ex.NumeroError) && ex.NumeroError != 3201)
        {
            return new ValidacionRespaldoDto { Resultado = ResultadoRespaldo.ArchivoInvalido, Mensaje = MensajeArchivoInvalido };
        }
        catch (Exception ex)
        {
            (ResultadoRespaldo resultado, string mensaje) = Mapear(ex, ResultadoRespaldo.Fallo, "No se pudo validar el respaldo.");
            return new ValidacionRespaldoDto { Resultado = resultado, Mensaje = mensaje };
        }
    }

    private const string MensajeArchivoInvalido =
        "El archivo seleccionado no es un respaldo completo y válido de la base de datos del sistema.";

    /// <summary>
    /// Pasos 3 a 8 de la restauración (tras la confirmación del usuario): semáforo, lock
    /// de aplicación, revalidación por huella, control de estadísticas asíncronas, backup
    /// automático de seguridad, SINGLE_USER + RESTORE y recuperación de MULTI_USER. Éxito
    /// solo con evidencia de ONLINE + MULTI_USER; no hay RESTORE automático adicional.
    /// </summary>
    public async Task<RespuestaRestauracion> EjecutarRestauracionAsync(
        string? rol, ValidacionRespaldoDto validacion, IProgress<string>? progreso = null, CancellationToken ct = default,
        Action? alIniciarFaseDestructiva = null)
    {
        if (!PoliticaAcceso.PuedeGestionarRespaldos(rol))
        {
            return new RespuestaRestauracion { Resultado = ResultadoRespaldo.SinPermiso, Mensaje = MensajeSinPermisoRol };
        }

        if (validacion is null || validacion.Resultado != ResultadoRespaldo.Exito || validacion.Respaldo is null)
        {
            return new RespuestaRestauracion { Resultado = ResultadoRespaldo.ArchivoInvalido, Mensaje = MensajeArchivoInvalido };
        }

        ResultadoRutaRespaldo ruta = _rutas.ObtenerCarpeta();
        if (!ruta.Disponible || ruta.Carpeta is null)
        {
            return new RespuestaRestauracion { Resultado = ResultadoRespaldo.RutaNoDisponible, Mensaje = ruta.Mensaje };
        }

        if (!EstaEnCarpeta(ruta.Carpeta, validacion.Respaldo.RutaArchivo))
        {
            return new RespuestaRestauracion
            {
                Resultado = ResultadoRespaldo.ArchivoInvalido,
                Mensaje = "El archivo no pertenece a la carpeta de respaldos del sistema."
            };
        }

        IDisposable? salida;
        try
        {
            salida = await _exclusion.IntentarEntrarAsync(ct);
        }
        catch (OperationCanceledException)
        {
            return new RespuestaRestauracion { Resultado = ResultadoRespaldo.Cancelado, Mensaje = MensajeCanceladoSinCambios };
        }

        if (salida is null)
        {
            return new RespuestaRestauracion { Resultado = ResultadoRespaldo.OperacionEnCurso, Mensaje = MensajeOperacionEnCurso };
        }

        using (salida)
        {
            return await RestaurarDentroDeExclusionAsync(ruta.Carpeta, validacion.Respaldo, progreso, alIniciarFaseDestructiva, ct);
        }
    }

    private async Task<RespuestaRestauracion> RestaurarDentroDeExclusionAsync(
        string carpeta, RespaldoDisponibleDto respaldo, IProgress<string>? progreso, Action? alIniciarFaseDestructiva, CancellationToken ct)
    {
        string? rutaSeguridad = null;

        try
        {
            AperturaSesionExclusiva apertura = await _repositorio.AbrirSesionExclusivaAsync(ct);
            if (apertura.Sesion is null)
            {
                return new RespuestaRestauracion
                {
                    Resultado = apertura.CodigoApplock == -1 ? ResultadoRespaldo.OperacionEnCurso : ResultadoRespaldo.Fallo,
                    Mensaje = apertura.CodigoApplock == -1 ? MensajeOtraInstancia : "No se pudo coordinar la restauración con la base de datos."
                };
            }

            await using ISesionRespaldo sesion = apertura.Sesion;

            // (4) Revalidar con la huella.
            IReadOnlyList<FilaArchivoBackup> actuales = await sesion.LeerArchivosActualesAsync(ct);
            ArchivoValidado? validado;
            try
            {
                validado = await ValidarArchivoAsync(sesion, respaldo.RutaArchivo, actuales, ct);
            }
            catch (RespaldoBdException ex) when (!EsErrorDeAcceso(ex.NumeroError) && ex.NumeroError != 3201)
            {
                validado = null;
            }

            if (validado is null)
            {
                return new RespuestaRestauracion { Resultado = ResultadoRespaldo.ArchivoInvalido, Mensaje = MensajeArchivoInvalido };
            }

            if (validado.Dto.Huella != respaldo.Huella)
            {
                return new RespuestaRestauracion
                {
                    Resultado = ResultadoRespaldo.RespaldoModificado,
                    Mensaje = "El archivo de respaldo cambió después de la confirmación. No se realizó ninguna modificación; vuelva a seleccionarlo."
                };
            }

            EstadoBaseDatos? estadoInicial = await sesion.LeerEstadoAsync(ct);
            if (estadoInicial is null)
            {
                return new RespuestaRestauracion { Resultado = ResultadoRespaldo.Fallo, Mensaje = "No se pudo leer el estado de la base de datos." };
            }

            if (estadoInicial.EstadisticasAsincronas)
            {
                return new RespuestaRestauracion
                {
                    Resultado = ResultadoRespaldo.EstadisticasAsincronasActivas,
                    Mensaje = MensajeEstadisticasAsincronas
                };
            }

            List<MapeoArchivo> mapeo = validado.Archivos
                .Select(a => new MapeoArchivo(
                    a.LogicalName,
                    actuales.First(c => string.Equals(c.LogicalName, a.LogicalName, StringComparison.OrdinalIgnoreCase)).PhysicalName))
                .ToList();

            // (5) Backup automático de seguridad verificado.
            DateTime ahora = _reloj.GetLocalNow().DateTime;
            string archivoSeguridad = Path.Combine(carpeta, ConstruirNombreArchivo(_repositorio.NombreBaseDatos, ahora, automatico: true));
            try
            {
                progreso?.Report("Generando backup de seguridad previo...");
                if (File.Exists(archivoSeguridad))
                {
                    throw new IOException("Ya existe el archivo de seguridad.");
                }

                await sesion.EjecutarBackupAsync(
                    archivoSeguridad,
                    $"G70 {_repositorio.NombreBaseDatos} AUTO-PRE-RESTAURACION {ahora.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}",
                    "Backup automático de seguridad previo a una restauración (G70)", ct);
                await sesion.VerificarBackupAsync(archivoSeguridad, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                (_, string detalle) = Mapear(ex, ResultadoRespaldo.FalloBackupSeguridad, "No se pudo generar el backup de seguridad.");
                return new RespuestaRestauracion
                {
                    Resultado = ResultadoRespaldo.FalloBackupSeguridad,
                    Mensaje = "No se pudo generar y verificar el backup de seguridad previo; la base de datos no fue modificada. " + detalle
                };
            }

            // (6) Informar la ruta de seguridad.
            rutaSeguridad = archivoSeguridad;
            progreso?.Report($"Backup de seguridad creado: {rutaSeguridad}");

            // Último punto de cancelación: antes de SINGLE_USER no se ha modificado la base.
            if (ct.IsCancellationRequested)
            {
                return new RespuestaRestauracion
                {
                    Resultado = ResultadoRespaldo.Cancelado,
                    RutaBackupSeguridad = rutaSeguridad,
                    Mensaje = MensajeCanceladoSinCambios + $" Backup de seguridad disponible en: {rutaSeguridad}"
                };
            }

            // (7) SINGLE_USER + RESTORE; en toda salida se intenta volver a MULTI_USER.
            // Desde aquí la secuencia NO es cancelable: se usa CancellationToken.None.
            alIniciarFaseDestructiva?.Invoke();
            bool singleAplicado = false;
            Exception? falloRestauracion = null;
            try
            {
                progreso?.Report("Restaurando la base de datos...");
                singleAplicado = true; // se marca antes: si el ALTER falla a medias, igual se intenta recuperar.
                await sesion.EstablecerUsuarioUnicoAsync(CancellationToken.None);
                await sesion.RestaurarAsync(respaldo.RutaArchivo, mapeo, CancellationToken.None);
            }
            catch (Exception ex)
            {
                falloRestauracion = ex;
            }

            EstadoBaseDatos? estadoFinal = singleAplicado
                ? await sesion.RecuperarMultiUsuarioAsync()
                : null;
            string? textoEstado = estadoFinal is null ? null : $"{estadoFinal.Estado} / {estadoFinal.AccesoUsuarios}";

            if (falloRestauracion is not null)
            {
                (_, string detalle) = Mapear(falloRestauracion, ResultadoRespaldo.FalloRestauracion, "La restauración falló.");
                return new RespuestaRestauracion
                {
                    Resultado = ResultadoRespaldo.FalloRestauracion,
                    RutaBackupSeguridad = rutaSeguridad,
                    EstadoAccesoFinal = textoEstado,
                    Mensaje = $"{detalle} " + DescribirRecuperacion(estadoFinal)
                        + $" Backup de seguridad disponible en: {rutaSeguridad}"
                };
            }

            if (estadoFinal is null || !estadoFinal.EstaOperativa)
            {
                return new RespuestaRestauracion
                {
                    Resultado = ResultadoRespaldo.FalloRestauracion,
                    RutaBackupSeguridad = rutaSeguridad,
                    EstadoAccesoFinal = textoEstado,
                    Mensaje = "La restauración se ejecutó pero no se pudo comprobar que la base quede ONLINE y MULTI_USER. "
                        + DescribirRecuperacion(estadoFinal) + $" Backup de seguridad disponible en: {rutaSeguridad}"
                };
            }

            // (8) Éxito comprobado.
            return new RespuestaRestauracion
            {
                Resultado = ResultadoRespaldo.Exito,
                RutaBackupSeguridad = rutaSeguridad,
                EstadoAccesoFinal = textoEstado,
                Mensaje = $"La base de datos se restauró correctamente. Backup de seguridad previo: {rutaSeguridad}"
            };
        }
        catch (Exception ex)
        {
            (ResultadoRespaldo resultado, string mensaje) = Mapear(ex, ResultadoRespaldo.Fallo, "No se pudo completar la restauración.");
            return new RespuestaRestauracion
            {
                Resultado = resultado,
                Mensaje = mensaje + (rutaSeguridad is null ? string.Empty : $" Backup de seguridad disponible en: {rutaSeguridad}"),
                RutaBackupSeguridad = rutaSeguridad
            };
        }
    }

    private static string DescribirRecuperacion(EstadoBaseDatos? estado) => estado switch
    {
        null => "No se pudo verificar el estado final de la base de datos; revíselo con un administrador.",
        { EstaOperativa: true } => "La base de datos quedó ONLINE y MULTI_USER.",
        _ => $"Estado final de la base de datos: {estado.Estado} / {estado.AccesoUsuarios}; se requiere intervención de un administrador."
    };

    // ---------------------------------------------------------------- validación interna

    private sealed record ArchivoValidado(RespaldoDisponibleDto Dto, IReadOnlyList<FilaArchivoBackup> Archivos);

    private async Task<ArchivoValidado?> ValidarArchivoAsync(
        ISesionRespaldo sesion, string ruta, IReadOnlyList<FilaArchivoBackup> actuales, CancellationToken ct)
    {
        var info = new FileInfo(ruta);
        if (!info.Exists)
        {
            return null;
        }

        IReadOnlyList<FilaMetadatoBackup> cabeceras = await sesion.LeerCabecerasAsync(ruta, ct);
        if (cabeceras.Count != 1)
        {
            return null;
        }

        FilaMetadatoBackup c = cabeceras[0];
        if (!string.Equals(c.DatabaseName, _repositorio.NombreBaseDatos, StringComparison.OrdinalIgnoreCase)
            || c.BackupType != 1 || c.BackupSetGUID is null || c.BackupFinishDate is null)
        {
            return null;
        }

        await sesion.VerificarBackupAsync(ruta, ct);

        IReadOnlyList<FilaArchivoBackup> lista = await sesion.LeerListaArchivosAsync(ruta, ct);
        if (!SonCoherentes(lista, actuales))
        {
            return null;
        }

        var dto = new RespaldoDisponibleDto
        {
            NombreArchivo = info.Name,
            RutaArchivo = info.FullName,
            FechaBackup = c.BackupFinishDate.Value,
            Tipo = info.Name.EndsWith(SufijoAutomatico, StringComparison.OrdinalIgnoreCase) ? "Automático" : "Manual",
            TamanoBytes = info.Length,
            Huella = new HuellaRespaldo(c.BackupSetGUID.Value, c.BackupFinishDate.Value, info.Length, info.LastWriteTimeUtc)
        };

        return new ArchivoValidado(dto, lista);
    }

    private static bool SonCoherentes(IReadOnlyList<FilaArchivoBackup> lista, IReadOnlyList<FilaArchivoBackup> actuales)
    {
        if (lista.Count == 0 || lista.Count != actuales.Count)
        {
            return false;
        }

        var nombres = new HashSet<string>(actuales.Select(a => a.LogicalName), StringComparer.OrdinalIgnoreCase);
        return lista.Select(l => l.LogicalName).Distinct(StringComparer.OrdinalIgnoreCase).Count() == lista.Count
            && lista.All(l => nombres.Contains(l.LogicalName));
    }

    private static bool EstaEnCarpeta(string carpeta, string rutaArchivo)
    {
        if (string.IsNullOrWhiteSpace(rutaArchivo))
        {
            return false;
        }

        try
        {
            string completa = Path.GetFullPath(rutaArchivo);
            string? padre = Path.GetDirectoryName(completa);
            return padre is not null
                && string.Equals(Path.TrimEndingDirectorySeparator(padre), Path.TrimEndingDirectorySeparator(Path.GetFullPath(carpeta)), StringComparison.OrdinalIgnoreCase)
                && string.Equals(Path.GetExtension(completa), ".bak", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }

    // ---------------------------------------------------------------- errores

    private static bool EsErrorDeAcceso(int numero) => numero is 262 or 15247 or 4060 or 916 or 18456 or 229 or 230;

    /// <summary>Traduce excepciones a resultado y mensaje en español; nunca expone texto crudo de SQL.</summary>
    private static (ResultadoRespaldo Resultado, string Mensaje) Mapear(Exception ex, ResultadoRespaldo porDefecto, string accion)
    {
        switch (ex)
        {
            case OperationCanceledException:
                return (ResultadoRespaldo.Cancelado, "La operación fue cancelada.");
            case RespaldoBdException { NumeroError: 262 or 15247 }:
                return (ResultadoRespaldo.SinPermiso,
                    "La cuenta con la que se ejecuta G70 no tiene permisos en SQL Server para realizar esta operación. Solicite a un administrador de la base de datos que se los otorgue.");
            case RespaldoBdException { NumeroError: 4060 or 916 or 18456 }:
                return (ResultadoRespaldo.SinPermiso,
                    "La cuenta con la que se ejecuta G70 no tiene acceso a la base de datos del sistema (master) en SQL Server. Solicite acceso a un administrador de la base de datos.");
            case RespaldoBdException { NumeroError: 3201 }:
                return (ResultadoRespaldo.RutaNoDisponible,
                    "SQL Server no pudo acceder a la carpeta de respaldos. Verifique que la cuenta del servicio de SQL Server tenga permisos de lectura y escritura sobre ella.");
            case RespaldoBdException bd:
                return (porDefecto, $"{accion} Error de base de datos (código {bd.NumeroError}).");
            case IOException or UnauthorizedAccessException:
                return (ResultadoRespaldo.RutaNoDisponible, "No se pudo acceder a la carpeta de respaldos. Verifique sus permisos.");
            default:
                return (porDefecto, accion + " Ocurrió un error inesperado.");
        }
    }
}
