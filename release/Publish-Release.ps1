param(
    [Parameter(Mandatory = $true)]
    [string]$TechdemoUrl,
    [string]$CommitNachricht = 'Release 0.1.0: Installer, deutsches Handbuch, Einrichtung und Techdemo'
)

$ErrorActionPreference = 'Stop'
$taskRepository = Split-Path -Parent $PSScriptRoot
$taskRepositoryName = 'blackhole-dynamics/Star-Citizen-Companion-Deck'
$taskVersion = '0.1.0'
$taskTag = "v$taskVersion"
$taskUtf8 = New-Object System.Text.UTF8Encoding($false)

function Invoke-DeckGit {
    & git @args
    if ($LASTEXITCODE -ne 0) { throw "Git fehlgeschlagen: $($args -join ' ')" }
}

function Invoke-DeckGh {
    & gh @args
    if ($LASTEXITCODE -ne 0) { throw "GitHub fehlgeschlagen: $($args -join ' ')" }
}

if ($TechdemoUrl -notmatch '^https://github\.com/user-attachments/assets/[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}$') {
    throw 'Die vollständige GitHub-Medienadresse einfügen, ohne Markdown-Zeichen oder Leerzeichen.'
}

Push-Location $taskRepository
try {
    $taskBranch = (Invoke-DeckGit branch --show-current).Trim()
    if ($taskBranch -ne 'main') { throw 'Dieses Skript erwartet den geprüften Branch main.' }
    $taskOrigin = (Invoke-DeckGit remote get-url origin).Trim()
    if ($taskOrigin -ne "https://github.com/$taskRepositoryName.git") {
        throw "Unerwartetes GitHub-Ziel: $taskOrigin"
    }
    Invoke-DeckGh auth status
    Invoke-DeckGit fetch origin
    $taskRemoteChanges = Invoke-DeckGit rev-list --count HEAD..origin/main
    if ([int]$taskRemoteChanges -ne 0) {
        throw 'Auf GitHub liegen neue Commits. Zuerst prüfen und übernehmen; kein Force-Push.'
    }
    & git show-ref --verify --quiet "refs/tags/$taskTag"
    if ($LASTEXITCODE -eq 0) { throw "Der lokale Tag $taskTag existiert bereits. Bestehende Tags werden nicht überschrieben." }
    $taskRemoteTag = Invoke-DeckGit ls-remote --tags origin "refs/tags/$taskTag"
    if ($taskRemoteTag) { throw "Der Tag $taskTag ist bereits auf GitHub vorhanden." }
    $taskReleases = Invoke-DeckGh release list --repo $taskRepositoryName --limit 100 --json 'tagName,isDraft'
    if (@($taskReleases | ConvertFrom-Json | Where-Object { $_.tagName -eq $taskTag }).Count -gt 0) {
        throw "Ein Release oder Entwurf für $taskTag existiert bereits. Zuerst dessen Stand prüfen."
    }

    $taskArtifacts = Join-Path $taskRepository "artifacts\$taskVersion"
    $taskChecksums = Join-Path $taskArtifacts 'SHA256SUMS.txt'
    $taskExpectedFiles = @(
        "Begleiter-Deck-$taskVersion-win-x64-Setup.exe",
        "Begleiter-Deck-$taskVersion-win-x64-portabel.zip",
        "Begleiter-Deck-$taskVersion-Benutzerhandbuch.pdf"
    )
    $taskVerifiedFiles = @()
    foreach ($taskChecksum in Get-Content -LiteralPath $taskChecksums) {
        if ($taskChecksum -notmatch '^([a-fA-F0-9]{64})  (.+)$') { throw 'Ungültige Prüfsummenliste.' }
        $taskExpectedHash = $Matches[1]
        $taskFileName = $Matches[2]
        if ($taskFileName -notin $taskExpectedFiles) { throw "Unerwarteter Release-Anhang: $taskFileName" }
        $taskFilePath = Join-Path $taskArtifacts $taskFileName
        if ((Get-FileHash -LiteralPath $taskFilePath -Algorithm SHA256).Hash -ne $taskExpectedHash) {
            throw "Prüfsumme stimmt nicht: $taskFileName"
        }
        $taskVerifiedFiles += $taskFileName
    }
    if ($taskVerifiedFiles.Count -ne 3 -or @($taskVerifiedFiles | Select-Object -Unique).Count -ne 3) {
        throw 'Es müssen genau die drei geprüften Release-Dateien vorliegen.'
    }
    $taskManualHash = (Get-FileHash -LiteralPath (Join-Path $taskRepository 'docs\Benutzerhandbuch.pdf') -Algorithm SHA256).Hash
    $taskReleaseManualHash = (Get-FileHash -LiteralPath (Join-Path $taskArtifacts "Begleiter-Deck-$taskVersion-Benutzerhandbuch.pdf") -Algorithm SHA256).Hash
    if ($taskManualHash -ne $taskReleaseManualHash) { throw 'Das Handbuch im Repository stimmt nicht mit dem Release überein.' }

    $taskReadmePath = Join-Path $taskRepository 'README.md'
    $taskReadme = [IO.File]::ReadAllText($taskReadmePath)
    $taskVideoSection = @"
## Techdemo

Die 40-Sekunden-Techdemo erklärt vier Bedienungsschritte: Begleiter-Reaktionen
live lesen, Kategorien wählen, weitere Logs zuschalten und das Herstellerdesign wechseln.

$TechdemoUrl

Mit dem Player direkt hier abspielen; das Video enthält Originalton und ruhige Musik.
[Techdemo als MP4 herunterladen](docs/media/techdemo.mp4).
[Medienquellen und Musikcredits](docs/VIDEO-MEDIEN-V2.md).

"@
    if ($taskReadme -notmatch '(?s)## Techdemo\r?\n.*?(?=## Entwicklung)') {
        throw 'Der erwartete Techdemo-Abschnitt fehlt. README zuerst prüfen.'
    }
    $taskReadme = [regex]::Replace($taskReadme, '(?s)## Techdemo\r?\n.*?(?=## Entwicklung)', $taskVideoSection + "`n")
    $taskReadme = [regex]::Replace($taskReadme,
        'Die Ausgabe \*\*0\.1\.0\*\* wird zur Veröffentlichung vorbereitet; auf GitHub wurde noch\r?\nkein Release veröffentlicht\. Unterstützt:',
        'Downloads zur Ausgabe **0.1.0**: [Installer, portables Paket und Handbuch](https://github.com/blackhole-dynamics/Star-Citizen-Companion-Deck/releases/tag/v0.1.0). Unterstützt:')
    [IO.File]::WriteAllText($taskReadmePath, ($taskReadme -replace "`r`n", "`n"), $taskUtf8)

    Invoke-DeckGit diff --check
    Invoke-DeckGit add --all
    Invoke-DeckGit diff --cached --check
    Invoke-DeckGit diff --cached --stat
    Invoke-DeckGit commit -m $CommitNachricht
    $taskCommit = (Invoke-DeckGit rev-parse HEAD).Trim()
    Invoke-DeckGit push origin main
    Invoke-DeckGit tag -a $taskTag -m 'Star Citizen Begleiter-Deck 0.1.0' $taskCommit
    Invoke-DeckGit push origin $taskTag

    $taskReleaseAssets = @($taskExpectedFiles | ForEach-Object { Join-Path $taskArtifacts $_ })
    $taskReleaseAssets += $taskChecksums
    Invoke-DeckGh release create $taskTag @taskReleaseAssets --repo $taskRepositoryName --verify-tag --draft --title 'Star Citizen Begleiter-Deck 0.1.0' --notes-file (Join-Path $PSScriptRoot 'RELEASE-NOTES.md')
    $taskRelease = (Invoke-DeckGh release view $taskTag --repo $taskRepositoryName --json 'isDraft,assets' | ConvertFrom-Json)
    if (-not $taskRelease.isDraft -or $taskRelease.assets.Count -ne 4) {
        throw 'Der Release-Entwurf hat nicht die vier vorgesehenen Anhänge. Nicht veröffentlicht.'
    }
    foreach ($taskAsset in $taskRelease.assets) {
        $taskLocalAsset = Join-Path $taskArtifacts $taskAsset.name
        if (-not (Test-Path -LiteralPath $taskLocalAsset) -or (Get-Item -LiteralPath $taskLocalAsset).Length -ne $taskAsset.size) {
            throw "Größe des hochgeladenen Anhangs stimmt nicht: $($taskAsset.name). Entwurf bleibt unveröffentlicht."
        }
    }
    Invoke-DeckGh release edit $taskTag --repo $taskRepositoryName --draft=false --latest
    Invoke-DeckGh release view $taskTag --repo $taskRepositoryName --json 'url,isDraft,tagName,assets'
    Invoke-DeckGit status --short --branch
    Write-Output "Veröffentlicht: https://github.com/$taskRepositoryName/releases/tag/$taskTag"
}
finally { Pop-Location }
