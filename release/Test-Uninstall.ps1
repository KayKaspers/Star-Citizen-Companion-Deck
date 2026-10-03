param([string]$InnoCompiler = 'ISCC.exe', [string]$Version = '0.1.0')
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$taskRelease = Join-Path $taskRoot "artifacts\$Version"
$taskPayload = Join-Path $taskRelease 'app'
$taskTestRoot = Join-Path $taskRoot ('artifacts\uninstall-test-' + [guid]::NewGuid().ToString('N'))
$taskInstallDir = Join-Path $taskTestRoot 'installed'
New-Item -ItemType Directory -Path $taskTestRoot -Force | Out-Null
# Unique test AppId avoids touching the Maintainer's installed-product registration.
$taskTestId = '{{' + [guid]::NewGuid().ToString().ToUpperInvariant() + '}'
$taskTestGroup = 'SCBD-Installationstest-' + [guid]::NewGuid().ToString('N')
& $InnoCompiler "/DReleaseVersion=$Version-test" "/DInstallerAppId=$taskTestId" "/DPayloadDir=$taskPayload" "/DOutputDir=$taskTestRoot" "/DBootstrapFile=$(Join-Path $taskRelease 'MicrosoftEdgeWebview2Setup.exe')" (Join-Path $PSScriptRoot 'installer.iss') | Out-File (Join-Path $taskTestRoot 'compiler.log')
if ($LASTEXITCODE -ne 0) { throw 'Test-Installer konnte nicht gebaut werden.' }
$taskSetup = Join-Path $taskTestRoot "Begleiter-Deck-$Version-test-win-x64-Setup.exe"
$taskInstall = Start-Process -FilePath $taskSetup -ArgumentList @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART',('/GROUP="' + $taskTestGroup + '"'),('/DIR="' + $taskInstallDir + '"'),('/LOG="' + (Join-Path $taskTestRoot 'installation.log') + '"')) -PassThru -Wait -WindowStyle Hidden
if ($taskInstall.ExitCode -ne 0) { throw 'Testinstallation fehlgeschlagen.' }
$taskInstalledIcon = Join-Path $taskInstallDir "deck-icon-$Version-test.ico"
if ((Get-FileHash -LiteralPath $taskInstalledIcon).Hash -ne (Get-FileHash -LiteralPath (Join-Path $taskPayload 'app.ico')).Hash) { throw 'Installiertes versionsabhängiges Icon stimmt nicht mit der Vorlage überein.' }
Write-Output 'PASS versionsabhängiges großes Icon installiert'
if ((Get-FileHash -LiteralPath (Join-Path $taskInstallDir 'Benutzerhandbuch.pdf')).Hash -ne (Get-FileHash -LiteralPath (Join-Path $taskRoot 'docs\Benutzerhandbuch.pdf')).Hash) { throw 'Installiertes Handbuch stimmt nicht mit der Repository-Fassung überein.' }
Write-Output 'PASS Benutzerhandbuch vollständig installiert'
$taskGroupPath = Join-Path ([Environment]::GetFolderPath('Programs')) $taskTestGroup
$taskShortcutShell = New-Object -ComObject WScript.Shell
$taskManualShortcut = $taskShortcutShell.CreateShortcut((Join-Path $taskGroupPath 'Begleiter-Deck Benutzerhandbuch.lnk'))
if ($taskManualShortcut.TargetPath -ne (Join-Path $taskInstallDir 'Benutzerhandbuch.pdf')) { throw 'Startmenü-Handbuchverknüpfung fehlt oder zeigt auf die falsche Datei.' }
$taskAppShortcut = $taskShortcutShell.CreateShortcut((Join-Path $taskGroupPath 'Begleiter-Deck.lnk'))
if ($taskAppShortcut.IconLocation.Split(',')[0] -ne $taskInstalledIcon) { throw 'Startmenü-Verknüpfung verwendet nicht das versionsabhängige Icon.' }
Write-Output 'PASS Startmenü-Handbuch und versionsabhängiges Verknüpfungsicon'
Add-Type -Path (Join-Path $taskPayload 'Xeneon.Runtime.Core.dll')
Add-Type -Path (Join-Path $taskPayload 'Xeneon.Runtime.Windows.dll')
$taskDesktop = [Xeneon.Runtime.Windows.WindowsDesktop]::new()
$taskFixtureExe = Join-Path $taskRoot 'runtime\tests\Windows.Tests\bin\Release\net8.0-windows\Windows.Tests.exe'
$taskFixtureTitle = 'SCBD-Uninstall-' + [guid]::NewGuid().ToString('N')
$taskFixture = $null; $taskHost = $null
function Wait-TestState([scriptblock]$Condition, [string]$Label) {
    $taskTimer = [Diagnostics.Stopwatch]::StartNew()
    while (-not (& $Condition)) {
        if ($taskTimer.ElapsedMilliseconds -gt 10000) { throw "Zeitüberschreitung: $Label" }
        Start-Sleep -Milliseconds 50
    }
    Write-Output "PASS $Label"
}
try {
    $taskFixtureStart = [Diagnostics.ProcessStartInfo]::new($taskFixtureExe, "--fixture $taskFixtureTitle")
    $taskFixtureStart.UseShellExecute = $false; $taskFixtureStart.CreateNoWindow = $true
    $taskFixture = [Diagnostics.Process]::Start($taskFixtureStart)
    $taskSelector = [Xeneon.Runtime.Core.ExternalSelector]::new('Windows.Tests', [System.Management.Automation.Language.NullString]::Value, $taskFixtureTitle)
    Wait-TestState { $taskDesktop.FindWindows($taskSelector).Count -eq 1 } 'eigenes Begleiter-Testfenster erkannt'
    $taskOriginal = $taskDesktop.FindWindows($taskSelector)[0]
    $taskOriginalBounds = $taskOriginal.Bounds
    $taskDisplay = $taskDesktop.DiscoverDisplays() | Where-Object { $_.Name -eq 'XENEON EDGE' -and -not $_.Cloned } | Select-Object -First 1
    if (-not $taskDisplay) { $taskDisplay = $taskDesktop.DiscoverDisplays() | Where-Object { -not $_.Cloned } | Select-Object -First 1 }
    $taskData = Join-Path $taskTestRoot 'private-test-data'; New-Item -ItemType Directory -Path $taskData | Out-Null
    $taskGame = Join-Path $taskData 'Game.log'; Set-Content -LiteralPath $taskGame -Value '<2026-10-03T10:00:00Z> [Notice] Synthetic fixture [Player]' -Encoding utf8
    $taskEvents = Join-Path $taskData 'Aurora-Companion-Events.jsonl'
    Set-Content -LiteralPath $taskEvents -Value '{"schemaVersion":1,"type":"safety_zone_entered","timestampUtc":"2026-10-03T10:00:00Z"}' -Encoding utf8
    $taskConfig = @{schemaVersion=1;displayKey=$taskDisplay.Key;mode='Fullscreen';companionVariant='Aurora';externalWindow=@{processName='Windows.Tests';title=$taskFixtureTitle};externalPlacement='CenterInBay';gameLogPath=$taskGame;companionEventsPath=$taskEvents}
    $taskConfigFile = Join-Path $taskData 'deck.json'; $taskConfig | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $taskConfigFile -Encoding utf8
    $taskConfigHash = (Get-FileHash -LiteralPath $taskConfigFile).Hash
    $taskHost = Start-Process -FilePath (Join-Path $taskInstallDir 'Xeneon.Runtime.Host.exe') -ArgumentList @('--config',('"' + $taskConfigFile + '"'),'--place-external') -PassThru -WindowStyle Hidden
    Wait-TestState { $taskCurrent = $taskDesktop.ReadWindow([System.IntPtr]$taskOriginal.Handle); $taskCurrent -and -not $taskCurrent.Bounds.Equals($taskOriginalBounds) } 'laufendes Deck hat Testfenster positioniert'
    $taskUninstall = Start-Process -FilePath (Join-Path $taskInstallDir 'unins000.exe') -ArgumentList @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART',('/LOG="' + (Join-Path $taskTestRoot 'uninstallation.log') + '"')) -PassThru -Wait -WindowStyle Hidden
    if ($taskUninstall.ExitCode -ne 0) { throw "Deinstaller meldet Fehler $($taskUninstall.ExitCode)." }
    $taskHost.Refresh()
    if (-not $taskHost.HasExited) { throw 'Deck läuft nach der Deinstallation weiter.' }
    Write-Output 'PASS Deck nach Deinstallation vollständig beendet'
    Wait-TestState { $taskDesktop.ReadWindow([System.IntPtr]$taskOriginal.Handle).Bounds.Equals($taskOriginalBounds) } 'ursprüngliche Begleiterposition wiederhergestellt'
    if (Test-Path -LiteralPath $taskInstallDir) {
        if (@(Get-ChildItem -LiteralPath $taskInstallDir -File -Recurse).Count) { throw 'Dateien nach Deinstallation übrig.' }
    }
    Write-Output 'PASS keine Programmdateien nach Deinstallation übrig'
    if (Test-Path -LiteralPath $taskGroupPath) { throw 'Startmenü-Testordner wurde nicht entfernt.' }
    Write-Output 'PASS eigene Startmenü-Verknüpfungen entfernt'
    if ((Get-FileHash -LiteralPath $taskConfigFile).Hash -ne $taskConfigHash) { throw 'Private Testkonfiguration verändert.' }
    Write-Output 'PASS private Konfiguration unverändert erhalten'
    Write-Output "Installationsprüfung abgeschlossen: $taskTestRoot"
} finally {
    if ($taskHost) { $taskHost.Refresh(); if (-not $taskHost.HasExited) { $taskHost.CloseMainWindow() | Out-Null; $taskHost.WaitForExit(5000) | Out-Null }; $taskHost.Dispose() }
    if ($taskFixture) { $taskFixture.Refresh(); if (-not $taskFixture.HasExited) { $taskWindow = if ($taskOriginal) { $taskDesktop.ReadWindow([System.IntPtr]$taskOriginal.Handle) } else { $taskDesktop.FindWindows($taskSelector) | Select-Object -First 1 }; if ($taskWindow) { $taskDesktop.RequestClose($taskWindow) | Out-Null }; if (-not $taskFixture.WaitForExit(5000)) { $taskFixture.Kill(); $taskFixture.WaitForExit() } }; $taskFixture.Dispose() } # Test-owned fixture only.
}
