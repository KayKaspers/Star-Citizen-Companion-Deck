param(
    [string]$DotnetPath = 'dotnet',
    [string]$ConfigPath = (Join-Path $PSScriptRoot 'runtime.example.json'),
    [switch]$Probe,
    [string]$InspectProcess,
    [switch]$PlaceExternal
)
$ErrorActionPreference = 'Stop'
$resolvedConfig = (Resolve-Path -LiteralPath $ConfigPath).Path
Push-Location $PSScriptRoot
try {
    & $DotnetPath restore Xeneon.Runtime.Host --locked-mode
    if ($LASTEXITCODE -ne 0) { throw 'Paketwiederherstellung fehlgeschlagen. Ein .NET-8-SDK wird benötigt.' }
    & $DotnetPath build Xeneon.Runtime.Host -c Release --no-restore -warnaserror
    if ($LASTEXITCODE -ne 0) { throw 'Laufzeit-Build fehlgeschlagen.' }
    $runtimeArgs = @('--config', $resolvedConfig)
    if ($Probe) { $runtimeArgs += '--probe' }
    if ($InspectProcess) { $runtimeArgs += @('--inspect-process', $InspectProcess) }
    if ($PlaceExternal) { $runtimeArgs += '--place-external' }
    & $DotnetPath run --project Xeneon.Runtime.Host -c Release --no-build -- @runtimeArgs
    if ($LASTEXITCODE -ne 0) { throw 'Laufzeit fehlgeschlagen. Lokale Diagnosen prüfen.' }
} finally { Pop-Location }
