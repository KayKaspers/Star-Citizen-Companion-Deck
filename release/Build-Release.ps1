param(
    [string]$DotnetPath = 'dotnet',
    [string]$InnoCompiler = 'ISCC.exe',
    [string]$Version = '0.1.0'
)
$ErrorActionPreference = 'Stop'
if ($Version -notmatch '^\d+\.\d+\.\d+(-[a-z0-9.]+)?$') { throw 'Ungültige Versionsnummer.' }
$taskRoot = Split-Path -Parent $PSScriptRoot
$taskArtifacts = Join-Path $taskRoot "artifacts\$Version"
$taskPayload = Join-Path $taskArtifacts 'app'
$taskNumericVersion = $Version.Split('-')[0]
if (Test-Path -LiteralPath $taskPayload) { throw 'Ausgabeordner existiert bereits. Für einen neuen Build einen leeren Ausgabeordner verwenden.' }
New-Item -ItemType Directory -Path $taskPayload -Force | Out-Null
Push-Location (Join-Path $taskRoot 'runtime')
try {
    & $DotnetPath restore Xeneon.Runtime.Host -r win-x64 -p:RuntimeFrameworkVersion=8.0.31 -p:NuGetLockFilePath=packages.win-x64.lock.json --locked-mode
    if ($LASTEXITCODE -ne 0) { throw 'Release-Paketwiederherstellung fehlgeschlagen.' }
    & $DotnetPath publish Xeneon.Runtime.Host -c Release -r win-x64 --self-contained true --no-restore -p:RuntimeFrameworkVersion=8.0.31 -p:ReleaseInstaller=true -p:Version=$taskNumericVersion -p:InformationalVersion=$Version -p:DebugType=None -p:DebugSymbols=false -o $taskPayload
    if ($LASTEXITCODE -ne 0) { throw 'Release-Build fehlgeschlagen.' }
} finally { Pop-Location }
Copy-Item -LiteralPath (Join-Path $taskRoot 'README.md') -Destination $taskPayload
Copy-Item -LiteralPath (Join-Path $taskRoot 'docs\Benutzerhandbuch.pdf') -Destination $taskPayload
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'DRITTANBIETER.md') -Destination $taskPayload
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'app.ico') -Destination $taskPayload
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'licenses') -Destination $taskPayload -Recurse
foreach ($taskDocumentationName in @('Microsoft.Web.WebView2.Core.xml', 'Microsoft.Web.WebView2.WinForms.xml')) {
    $taskDocumentationPath = Join-Path $taskPayload $taskDocumentationName
    if (Test-Path -LiteralPath $taskDocumentationPath) { Remove-Item -LiteralPath $taskDocumentationPath }
}
$taskBootstrap = Join-Path $taskArtifacts 'MicrosoftEdgeWebview2Setup.exe'
Invoke-WebRequest -Uri 'https://go.microsoft.com/fwlink/p/?LinkId=2124703' -OutFile $taskBootstrap
$taskSignature = Get-AuthenticodeSignature -LiteralPath $taskBootstrap
if ($taskSignature.Status -ne 'Valid' -or $taskSignature.SignerCertificate.Subject -notmatch 'Microsoft Corporation') { throw 'WebView2-Download hat keine gültige Microsoft-Signatur.' }
& $InnoCompiler "/DReleaseVersion=$Version" "/DPayloadDir=$taskPayload" "/DOutputDir=$taskArtifacts" "/DBootstrapFile=$taskBootstrap" (Join-Path $PSScriptRoot 'installer.iss')
if ($LASTEXITCODE -ne 0) { throw 'Installer-Build fehlgeschlagen.' }
Compress-Archive -Path (Join-Path $taskPayload '*') -DestinationPath (Join-Path $taskArtifacts "Begleiter-Deck-$Version-win-x64-portabel.zip")
Copy-Item -LiteralPath (Join-Path $taskRoot 'docs\Benutzerhandbuch.pdf') -Destination (Join-Path $taskArtifacts "Begleiter-Deck-$Version-Benutzerhandbuch.pdf")
$taskFiles = @(Get-ChildItem -LiteralPath $taskArtifacts -File | Where-Object { $_.Name -like 'Begleiter-Deck-*' })
$taskHashes = $taskFiles | Get-FileHash -Algorithm SHA256 | ForEach-Object { '{0}  {1}' -f $_.Hash.ToLowerInvariant(), (Split-Path $_.Path -Leaf) }
$taskHashes | Set-Content -LiteralPath (Join-Path $taskArtifacts 'SHA256SUMS.txt') -Encoding ascii
Write-Output "Release-Kandidat vorbereitet: $taskArtifacts"
