<#
    preparar-bd.ps1 - Prepara la base de datos de desarrollo del Sistema Comercial G70.

    QUÉ HACE (en ese orden):
      1. (Si no -SoloDatos) Aplica migraciones de EF Core con `dotnet ef database update`.
      2. Ejecuta seed-desarrollo.sql con sqlcmd.
      3. Verifica conteos de categorías y productos.
#>

[CmdletBinding()]
param(
    [switch]$SoloDatos,
    [string]$ArchivoSeed,
    [string]$AppSettings
)

$ErrorActionPreference = 'Stop'

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
$script:TotalPasos = if ($SoloDatos) { 2 } else { 3 }

function Escribir-Paso {
    param([Parameter(Mandatory)][int]$Numero, [Parameter(Mandatory)][string]$Texto)
    Write-Host ("`n[{0}/{1}] {2}" -f $Numero, $script:TotalPasos, $Texto) -ForegroundColor Cyan
}
function Escribir-Ok {
    param([Parameter(Mandatory)][string]$Texto)
    Write-Host ("    OK: {0}" -f $Texto) -ForegroundColor Green
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
function Invoke-PasoMigraciones {
    Push-Location $script:RaizRepo
    try {
        Escribir-Info 'dotnet tool restore'
        & dotnet tool restore | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "dotnet tool restore falló ($LASTEXITCODE)" }
        Escribir-Info 'dotnet ef database update'
        & dotnet ef database update --project CapaDatos --startup-project CapaDatos --context AppDbContext
        if ($LASTEXITCODE -ne 0) { throw "dotnet ef database update falló ($LASTEXITCODE)" }
        Escribir-Ok 'Migraciones aplicadas.'
    } finally { Pop-Location }
}
function Invoke-PasoSeed {
    param([string]$Servidor,[string]$Base,[string]$RutaSeed)
    Escribir-Info ("sqlcmd -S $Servidor -d $Base -i $RutaSeed -f 65001 -b")
    & sqlcmd -S $Servidor -E -d $Base -i $RutaSeed -f 65001 -b
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd seed falló ($LASTEXITCODE)" }
    Escribir-Ok 'Seed ejecutada (idempotente).'
}
function Invoke-PasoVerificacion {
    param([string]$Servidor,[string]$Base)
    $q = @"
SET NOCOUNT ON;
SELECT CONCAT(N'Categorías: ', (SELECT COUNT(*) FROM dbo.CATEGORIA), N' (activas: ', (SELECT COUNT(*) FROM dbo.CATEGORIA WHERE estado = 1), N') | Productos: ', (SELECT COUNT(*) FROM dbo.PRODUCTO), N' (activos: ', (SELECT COUNT(*) FROM dbo.PRODUCTO WHERE estado = 1), N')') AS resumen;
"@
    $tmp = Join-Path $env:TEMP ("verif-{0}.txt" -f [guid]::NewGuid().ToString('N'))
    try {
        & sqlcmd -S $Servidor -E -d $Base -h -1 -W -Q $q -b -o $tmp -f i:65001,o:65001
        if ($LASTEXITCODE -ne 0) { throw "sqlcmd verif falló ($LASTEXITCODE)" }
        $linea = (Get-Content -LiteralPath $tmp -Encoding UTF8) -join ' '
        $linea = $linea.Trim()
    } finally { Remove-Item $tmp -Force -ErrorAction SilentlyContinue }
    Write-Host ("    $linea") -ForegroundColor White
    $okCats = ($linea -match 'Categor\S*\s*:\s*\d+')
    $okProds = ($linea -match 'Productos\s*:\s*\d+')
    if (-not $okCats -or -not $okProds) { Write-Warning 'Verificación sin conteo esperado.'; return $false }
    if ($linea -match 'Categor\S*\s*:\s*0' -or $linea -match 'Productos\s*:\s*0') {
        Write-Warning '0 categorías o 0 productos.'
        return $false
    }
    Escribir-Ok 'Verificación correcta.'
    return $true
}

try {
    Write-Host '=== Preparación de la base de desarrollo (Sistema Comercial G70) ===' -ForegroundColor Cyan
    if ($SoloDatos) { Write-Host '    Modo -SoloDatos: se omiten migraciones.' -ForegroundColor DarkGray }
    Comprobar-Comando 'dotnet' 'Instalá .NET SDK 8.'
    Comprobar-Comando 'sqlcmd' 'Instalá SQL Server Command Line Tools.'
    $script:RutaSeed = Resolver-Archivo -Ruta $ArchivoSeed -Nombre 'seed'
    $rutaApp = Resolver-Archivo -Ruta $AppSettings -Nombre 'appsettings'
    $cadena = Obtener-CadenaSql -RutaAppSettings $rutaApp
    $dest = Obtener-DestinoSql -Cadena $cadena
    Escribir-Ok ("Servidor $($dest.Servidor), base $($dest.Base) (integrada).")
    if (-not $SoloDatos) {
        Escribir-Paso 1 'Aplicando migraciones EF Core…'
        Invoke-PasoMigraciones
    }
    $n2 = if ($SoloDatos){1}else{2}
    Escribir-Paso $n2 'Ejecutando seed…'
    Invoke-PasoSeed -Servidor $dest.Servidor -Base $dest.Base -RutaSeed $script:RutaSeed
    $n3 = if ($SoloDatos){2}else{3}
    Escribir-Paso $n3 'Verificando…'
    [void](Invoke-PasoVerificacion -Servidor $dest.Servidor -Base $dest.Base)
    Write-Host "`nListo: base preparada." -ForegroundColor Green
    exit 0
} catch {
    Write-Host ("`nERROR: {0}" -f $_.Exception.Message) -ForegroundColor Red
    exit 1
}
