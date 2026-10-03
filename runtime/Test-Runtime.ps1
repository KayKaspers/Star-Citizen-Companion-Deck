param([string]$DotnetPath = 'dotnet', [switch]$IncludeWebChecks)
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    & $DotnetPath restore Xeneon.Runtime.sln --locked-mode
    if ($LASTEXITCODE -ne 0) { throw 'Paketwiederherstellung fehlgeschlagen.' }
    & $DotnetPath build Xeneon.Runtime.sln -c Release --no-restore -warnaserror
    if ($LASTEXITCODE -ne 0) { throw 'Build fehlgeschlagen.' }
    & $DotnetPath run --project tests/Core.Tests -c Release --no-build
    if ($LASTEXITCODE -ne 0) { throw 'Core-Prüfungen fehlgeschlagen.' }
    & $DotnetPath run --project tests/GameLog.Tests -c Release --no-build
    if ($LASTEXITCODE -ne 0) { throw 'Protokollprüfungen fehlgeschlagen.' }
    & $DotnetPath run --project tests/Windows.Tests -c Release --no-build
    if ($LASTEXITCODE -ne 0) { throw 'Windows-Prüfungen fehlgeschlagen.' }
    if ($IncludeWebChecks) {
        & node tests/web-ui.cjs
        if ($LASTEXITCODE -ne 0) { throw 'Oberflächenprüfungen fehlgeschlagen.' }
    }
} finally { Pop-Location }
