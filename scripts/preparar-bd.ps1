<#
    Prepara la base de datos de desarrollo del Sistema Comercial G70:
    valida el destino, aplica migraciones, ejecuta la seed y verifica los datos.
#>

[CmdletBinding()]
param(
    [switch]$SoloDatos,
    [string]$ArchivoSeed,
    [string]$AppSettings
)

$ErrorActionPreference = 'Stop'

# Codificación UTF-8 (sin BOM) para la consola y los programas nativos (evita acentos rotos).
$utf8 = New-Object System.Text.UTF8Encoding($false)
try { [Console]::OutputEncoding = $utf8; [Console]::InputEncoding = $utf8 } catch { }
$OutputEncoding = $utf8

# Único destino permitido.
$ServidorEsperado = 'localhost\SQLEXPRESS'
$BaseEsperada     = 'DB_SISTEMA_VENTA'

# Verificación de los datos que administra la seed (se identifican por claves determinísticas,
# no por el total de cada tabla). Cada entrada: Etiqueta, Consulta (devuelve UN entero) y Exacto.
$CuitsSeed = "N'30123456789',N'20123456789',N'27123456789',N'30711000001',N'30711000002',N'30711000003',N'30711000004',N'30711000005',N'30711000006',N'30711000007',N'30711000008'"
# Los nombres con tilde se arman con NCHAR para no depender de la codificación de la línea de comandos de sqlcmd.
$CategoriasSeed = "N'Bebidas',N'Almac'+NCHAR(233)+N'n',N'L'+NCHAR(225)+N'cteos',N'Limpieza',N'Frutas y Verduras'"
$ProductosSeed = "(codigo LIKE N'7790001%' OR codigo LIKE N'7790002%' OR codigo = N'7790003000001')"
$VerificacionesSeed = @(
    @{ Etiqueta = 'Usuarios de seed';                 Consulta = 'SELECT COUNT(*) FROM dbo.USUARIO WHERE dni IN (90000001,90000002,90000003,90000004)'; Exacto = 4 },
    @{ Etiqueta = 'Categorías de seed';               Consulta = "SELECT COUNT(*) FROM dbo.CATEGORIA WHERE descripcion IN ($CategoriasSeed, N'Kiosco')"; Exacto = 6 },
    @{ Etiqueta = 'Categorías de seed activas';       Consulta = "SELECT COUNT(*) FROM dbo.CATEGORIA WHERE estado = 1 AND descripcion IN ($CategoriasSeed)"; Exacto = 5 },
    @{ Etiqueta = 'Productos de seed';                Consulta = "SELECT COUNT(*) FROM dbo.PRODUCTO WHERE $ProductosSeed"; Exacto = 201 },
    @{ Etiqueta = 'Productos de seed activos';        Consulta = "SELECT COUNT(*) FROM dbo.PRODUCTO WHERE estado = 1 AND $ProductosSeed"; Exacto = 200 },
    @{ Etiqueta = 'Productos base de seed (activos)'; Consulta = "SELECT COUNT(*) FROM dbo.PRODUCTO WHERE estado = 1 AND codigo LIKE N'7790001%'"; Exacto = 12 },
    @{ Etiqueta = 'Productos activos sin precio_venta > precio_compra o sin stock'; Consulta = "SELECT COUNT(*) FROM dbo.PRODUCTO WHERE estado = 1 AND $ProductosSeed AND (precio_venta <= precio_compra OR stock <= 0)"; Exacto = 0 },
    @{ Etiqueta = 'Productos de seed sin categoría ni usuario válidos'; Consulta = "SELECT COUNT(*) FROM dbo.PRODUCTO p WHERE $ProductosSeed AND (NOT EXISTS (SELECT 1 FROM dbo.CATEGORIA c WHERE c.id_categoria = p.id_categoria) OR NOT EXISTS (SELECT 1 FROM dbo.USUARIO u WHERE u.id_usuario = p.id_usuario))"; Exacto = 0 },
    @{ Etiqueta = 'Proveedores de seed';              Consulta = "SELECT COUNT(*) FROM dbo.PROVEEDOR WHERE cuit IN ($CuitsSeed)"; Exacto = 11 },
    @{ Etiqueta = 'Proveedores de seed activos';      Consulta = "SELECT COUNT(*) FROM dbo.PROVEEDOR WHERE estado = 1 AND cuit IN ($CuitsSeed)"; Exacto = 10 },
    @{ Etiqueta = 'Clientes de seed';                 Consulta = "SELECT COUNT(*) FROM dbo.CLIENTE WHERE dni LIKE N'CLI00000__'"; Exacto = 12 },
    @{ Etiqueta = 'Clientes de seed activos';         Consulta = "SELECT COUNT(*) FROM dbo.CLIENTE WHERE estado = 1 AND dni LIKE N'CLI00000__'"; Exacto = 9 }
)

# Defaults protegidos contra $PSScriptRoot vacío
if ([string]::IsNullOrWhiteSpace($PSScriptRoot)) {
    $psDir = Split-Path -Parent $MyInvocation.MyCommand.Path
} else {
    $psDir = $PSScriptRoot
}

if ([string]::IsNullOrWhiteSpace($ArchivoSeed)) {
    $ArchivoSeed = Join-Path -Path $psDir -ChildPath 'seed-desarrollo.sql'
}
if ([string]::IsNullOrWhiteSpace($AppSettings)) {
    $appDir = Join-Path -Path (Split-Path -Parent $psDir) -ChildPath 'CapaPresentacion'
    $AppSettings = Join-Path -Path $appDir -ChildPath 'appsettings.json'
}

$script:RaizRepo = Split-Path -Parent $psDir
$script:TotalPasos = 8
$script:NumPaso = 0
$script:CodigoSalida = 1
$script:Resumen = New-Object System.Collections.Generic.List[string]

function Escribir-Paso {
    param([Parameter(Mandatory)][int]$Numero, [Parameter(Mandatory)][string]$Texto)
    $script:NumPaso = $Numero
    Write-Host ("`n[{0}/{1}] {2}" -f $Numero, $script:TotalPasos, $Texto) -ForegroundColor Cyan
}
function Escribir-Ok {
    param([Parameter(Mandatory)][string]$Texto)
    Write-Host ("    OK: {0}" -f $Texto) -ForegroundColor Green
    $script:Resumen.Add(("OK  [{0}/{1}] {2}" -f $script:NumPaso, $script:TotalPasos, $Texto))
}
function Fallar {
    param([Parameter(Mandatory)][string]$Mensaje, [int]$Codigo = 1)
    if ($Codigo -le 0) { $Codigo = 1 }
    $script:CodigoSalida = $Codigo
    throw $Mensaje
}
function Escribir-Info {
    param([Parameter(Mandatory)][string]$Texto)
    Write-Host ("    {0}" -f $Texto) -ForegroundColor DarkGray
}
function Comprobar-Comando {
    param([Parameter(Mandatory)][string]$Nombre, [Parameter(Mandatory)][string]$Pista)
    if (-not (Get-Command $Nombre -ErrorAction SilentlyContinue)) {
        throw "No se encontró el comando '$Nombre' en el PATH. $Pista"
    }
}
function Resolver-Archivo {
    param([Parameter(Mandatory)][string]$Ruta, [Parameter(Mandatory)][string]$Nombre)
    try {
        $rutaResuelta = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Ruta)
    } catch {
        $rutaResuelta = [System.IO.Path]::GetFullPath($Ruta)
    }
    if (-not (Test-Path -LiteralPath $rutaResuelta -PathType Leaf)) {
        throw "No existe el archivo de $Nombre : '$rutaResuelta'."
    }
    return $rutaResuelta
}
function Obtener-CadenaSql {
    param([Parameter(Mandatory)][string]$RutaAppSettings)
    try {
        $config = Get-Content -LiteralPath $RutaAppSettings -Raw -Encoding UTF8 | ConvertFrom-Json
    } catch {
        throw "No se pudo leer/parsear el appsettings '$RutaAppSettings'. $($_.Exception.Message)"
    }
    $cadena = $null
    if ($config -and $config.ConnectionStrings) { $cadena = $config.ConnectionStrings.CadenaSQL }
    if ([string]::IsNullOrWhiteSpace($cadena)) {
        throw "El appsettings '$RutaAppSettings' no define ConnectionStrings:CadenaSQL."
    }
    return [string]$cadena
}
function Obtener-DestinoSql {
    param([Parameter(Mandatory)][string]$Cadena)
    $builder = $null
    $tipos = @('System.Data.SqlClient.SqlConnectionStringBuilder','Microsoft.Data.SqlClient.SqlConnectionStringBuilder')
    foreach ($tipo in $tipos) {
        $t = $tipo -as [type]
        if ($null -eq $t) { continue }
        try {
            $builder = New-Object -TypeName $tipo -ArgumentList $Cadena
            break
        } catch { throw "No se pudo analizar la cadena: $($_.Exception.Message)" }
    }
    if ($null -eq $builder) { throw 'Falta ensamblado SqlClient.' }
    if ([string]::IsNullOrWhiteSpace($builder.DataSource) -or [string]::IsNullOrWhiteSpace($builder.InitialCatalog)) {
        throw 'Faltan Data Source o Initial Catalog.'
    }
    if (-not $builder.IntegratedSecurity) { throw 'Solo se admite autenticación integrada.' }
    return [pscustomobject]@{ Servidor=[string]$builder.DataSource; Base=[string]$builder.InitialCatalog }
}
function Invoke-EfMigracionesLista {
    $salida = & dotnet ef migrations list --project CapaDatos --startup-project CapaDatos --context AppDbContext
    $codigo = $LASTEXITCODE
    if ($codigo -ne 0) {
        $salida | Select-Object -Last 15 | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkGray }
        Fallar "dotnet ef migrations list falló (código $codigo)." $codigo
    }
    $lista = @()
    foreach ($linea in $salida) {
        $texto = [string]$linea
        if ($texto -match '^(\d{14}_\w+)(\s+\(Pending\))?\s*$') {
            $lista += [pscustomobject]@{ Id = $Matches[1]; Pendiente = [bool]$Matches[2] }
        }
    }
    if ($lista.Count -eq 0) { Fallar 'dotnet ef migrations list no devolvió ninguna migración reconocible.' }
    return ,$lista
}
function Invoke-SqlEscalar {
    param([string]$Servidor,[string]$Base,[string]$Consulta)
    $salida = & sqlcmd -S $Servidor -E -d $Base -h -1 -W -b -Q ("SET NOCOUNT ON; " + $Consulta)
    if ($LASTEXITCODE -ne 0) { Fallar "sqlcmd falló (código $LASTEXITCODE) ejecutando: $Consulta" $LASTEXITCODE }
    $valor = ($salida | ForEach-Object { ([string]$_).Trim() } | Where-Object { $_ -ne '' } | Select-Object -First 1)
    $n = 0
    if (-not [int]::TryParse([string]$valor, [ref]$n)) { Fallar "Resultado no numérico para: $Consulta (obtenido: '$valor')." }
    return $n
}
function Invoke-PasoConexion {
    param([string]$Servidor,[string]$Base)
    & sqlcmd -S $Servidor -E -d master -b -Q "SET NOCOUNT ON; SELECT 1" | Out-Null
    if ($LASTEXITCODE -ne 0) { Fallar "No se pudo conectar a SQL Server '$Servidor' (código $LASTEXITCODE)." $LASTEXITCODE }
    $existe = Invoke-SqlEscalar -Servidor $Servidor -Base 'master' -Consulta "SELECT CASE WHEN DB_ID(N'$Base') IS NULL THEN 0 ELSE 1 END"
    if ($existe -ne 1) {
        if ($SoloDatos) { Fallar "La base '$Base' no existe. Ejecutá sin -SoloDatos para crear/preparar el esquema." }
        Escribir-Info "La base '$Base' no existe todavía: la crearán las migraciones."
    }
    Escribir-Ok "Conexión a '$Servidor' correcta."
}
function Invoke-PasoToolRestore {
    & dotnet tool restore | Out-Null
    if ($LASTEXITCODE -ne 0) { Fallar "dotnet tool restore falló (código $LASTEXITCODE)." $LASTEXITCODE }
    Escribir-Ok 'Herramientas dotnet restauradas.'
}
function Invoke-PasoCambiosModelo {
    $salida = & dotnet ef migrations has-pending-model-changes --project CapaDatos --startup-project CapaDatos --context AppDbContext
    $codigo = $LASTEXITCODE
    if ($codigo -ne 0) {
        $salida | Select-Object -Last 8 | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkGray }
        Fallar ("El modelo tiene cambios sin migración, o la comprobación falló (código $codigo). " +
                "El script NO genera migraciones.") $codigo
    }
    Escribir-Ok 'Sin cambios pendientes del modelo.'
}
function Invoke-PasoMigraciones {
    if ($SoloDatos) {
        Escribir-Info 'Modo -SoloDatos: no se aplican migraciones; solo se comprueba que el esquema esté al día.'
    } else {
        Escribir-Info 'dotnet ef database update'
        & dotnet ef database update --project CapaDatos --startup-project CapaDatos --context AppDbContext
        if ($LASTEXITCODE -ne 0) { Fallar "dotnet ef database update falló (código $LASTEXITCODE). No se ejecuta la seed." $LASTEXITCODE }
    }
    $lista = Invoke-EfMigracionesLista
    $pendientes = @($lista | Where-Object { $_.Pendiente })
    if ($pendientes.Count -gt 0) {
        $ids = ($pendientes | ForEach-Object { $_.Id }) -join ', '
        if ($SoloDatos) { Fallar "Hay migraciones pendientes en la base ($ids). Primero hay que preparar el esquema: ejecutá el script SIN -SoloDatos." }
        Fallar "Quedaron migraciones pendientes después de database update ($ids)."
    }
    Escribir-Ok ("Esquema al día ({0} migraciones aplicadas)." -f $lista.Count)
}
function Invoke-PasoSeed {
    param([string]$Servidor,[string]$Base,[string]$RutaSeed)
    Escribir-Info ("sqlcmd -S $Servidor -d $Base -i $RutaSeed -f 65001 -b")
    & sqlcmd -S $Servidor -E -d $Base -i $RutaSeed -f 65001 -b
    if ($LASTEXITCODE -ne 0) { Fallar "sqlcmd seed falló (código $LASTEXITCODE)." $LASTEXITCODE }
    Escribir-Ok 'Seed ejecutada (idempotente).'
}
function Invoke-PasoVerificacion {
    param([string]$Servidor,[string]$Base)
    $fallos = @()
    foreach ($v in $VerificacionesSeed) {
        $n = Invoke-SqlEscalar -Servidor $Servidor -Base $Base -Consulta $v.Consulta
        $estado = 'OK'
        if ($null -ne $v.Exacto -and $n -ne [int]$v.Exacto) { $estado = "ERROR (esperado $($v.Exacto))"; $fallos += $v.Etiqueta }
        elseif ($null -ne $v.Minimo -and $n -lt [int]$v.Minimo) { $estado = "ERROR (mínimo $($v.Minimo))"; $fallos += $v.Etiqueta }
        Write-Host ("    {0}: {1} [{2}]" -f $v.Etiqueta, $n, $estado) -ForegroundColor White
    }
    if ($fallos.Count -gt 0) { Fallar ("Verificación de datos fallida: " + ($fallos -join ', ') + '.') }
    Escribir-Ok 'Verificación de datos correcta.'
}

try {
    Write-Host '=== Preparación de la base de desarrollo (Sistema Comercial G70) ===' -ForegroundColor Cyan
    if ($SoloDatos) { Write-Host '    Modo -SoloDatos: no se aplican migraciones (se exige esquema al día).' -ForegroundColor DarkGray }
    Comprobar-Comando 'dotnet' 'Instalá .NET SDK.'
    Comprobar-Comando 'sqlcmd' 'Instalá SQL Server Command Line Tools.'
    $script:RutaSeed = Resolver-Archivo -Ruta $ArchivoSeed -Nombre 'seed'
    $rutaApp = Resolver-Archivo -Ruta $AppSettings -Nombre 'appsettings'
    Push-Location $script:RaizRepo
    try {
        Escribir-Paso 1 'Verificando servidor y base de destino…'
        $cadena = Obtener-CadenaSql -RutaAppSettings $rutaApp
        $dest = Obtener-DestinoSql -Cadena $cadena
        if (($dest.Servidor -ine $ServidorEsperado) -or ($dest.Base -ine $BaseEsperada)) {
            Fallar ("Destino no permitido: servidor '{0}', base '{1}'. Este script solo opera sobre '{2}' / '{3}'." -f $dest.Servidor, $dest.Base, $ServidorEsperado, $BaseEsperada)
        }
        Escribir-Ok ("Servidor $($dest.Servidor), base $($dest.Base) (integrada).")
        Escribir-Paso 2 'Verificando conexión a SQL Server…'
        Invoke-PasoConexion -Servidor $dest.Servidor -Base $dest.Base
        Escribir-Paso 3 'Restaurando herramientas (dotnet tool restore)…'
        Invoke-PasoToolRestore
        Escribir-Paso 4 'Comprobando cambios pendientes del modelo…'
        Invoke-PasoCambiosModelo
        Escribir-Paso 5 $(if ($SoloDatos) { 'Comprobando que no haya migraciones pendientes (-SoloDatos)…' } else { 'Aplicando migraciones EF Core…' })
        Invoke-PasoMigraciones
        Escribir-Paso 6 'Ejecutando seed…'
        Invoke-PasoSeed -Servidor $dest.Servidor -Base $dest.Base -RutaSeed $script:RutaSeed
        Escribir-Paso 7 'Verificando datos de la seed…'
        Invoke-PasoVerificacion -Servidor $dest.Servidor -Base $dest.Base
    } finally { Pop-Location }
    Escribir-Paso 8 'Resumen'
    foreach ($r in $script:Resumen) { Write-Host ("    {0}" -f $r) }
    Write-Host "`nESTADO FINAL: ÉXITO - base preparada." -ForegroundColor Green
    exit 0
} catch {
    Write-Host ("`nERROR: {0}" -f $_.Exception.Message) -ForegroundColor Red
    foreach ($r in $script:Resumen) { Write-Host ("    {0}" -f $r) }
    Write-Host ("    FALLO en el paso {0}/{1}" -f $script:NumPaso, $script:TotalPasos) -ForegroundColor Red
    Write-Host "`nESTADO FINAL: ERROR - la base NO quedó preparada." -ForegroundColor Red
    exit $script:CodigoSalida
}
