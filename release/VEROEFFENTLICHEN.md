# Veröffentlichung durch den Maintainer

Der Maintainer führt Commit und Push selbst aus. `Publish-Release.ps1` ist dafür
vorbereitet und wurde vom Agenten nicht ausgeführt. Es übernimmt eine vorhandene
GitHub-Medienadresse in die README, prüft die lokalen Release-Prüfsummen, erstellt
Commit und Push, setzt Tag v0.1.0 und veröffentlicht die vier Release-Anhänge.
Bestehende Tags und Releases werden nicht überschrieben; es gibt keinen Force-Push.

## Techdemo zuerst selbst hochladen

1. [README im GitHub-Webeditor öffnen](https://github.com/KayKaspers/Star-Citizen-Companion-Deck/edit/main/README.md).
2. Im Textfeld ans Ende gehen. Unter dem Textfeld „Attach files … selecting“ wählen.
3. `docs/media/techdemo.mp4` aus dem lokalen Repository auswählen (1,44 MB).
4. Die erzeugte `https://github.com/user-attachments/assets/...`-Adresse kopieren.
5. Im Webeditor **keinen Commit erstellen**. Der lokale Stand wird anschließend
   durch das PowerShell-Skript vollständig übernommen; der Webeditor enthält noch
   die ältere README. Änderungen dort abbrechen.

Die Datei im Git-Repository oder ein Release-Download erzeugt allein keinen
nativen README-Player. GitHub unterstützt Medienanhänge ausdrücklich auch in
READMEs. [GitHub-Ankündigung](https://github.blog/changelog/2021-05-13-video-uploads-now-generally-available/),
[Dateien anhängen](https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/attaching-files).

## PowerShell-Aufruf

```powershell
$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath 'D:\Projects\Star-Citizen-Companion-Deck'
$techdemoUrl = (Read-Host 'GitHub-Medienadresse der hochgeladenen Techdemo').Trim()
.\release\Publish-Release.ps1 -TechdemoUrl $techdemoUrl
```

Voraussetzung: Git und GitHub CLI sind bereits installiert; GitHub CLI ist als
KayKaspers angemeldet. Der geprüfte Branch ist main. Vor dem Aufruf alle Dateien
im angezeigten Repository-Status prüfen: `git add --all` nimmt den gesamten
vorbereiteten Stand einschließlich neuer Dateien und der Bereinigung auf.

Falls eine Netzwerkaktion nach dem Commit scheitert, zuerst `git status` und den
GitHub-Release-Stand prüfen. Das Skript absichtlich nicht blind wiederholen; ein
bereits angelegter Tag oder Entwurf bleibt erhalten. Der Entwurf wird erst nach
Prüfung seiner vier hochgeladenen Dateien veröffentlicht.

## Geprüfter Stand

Am 03.10.2026 erneut: 45 Core-, 21 Log-, 54 Windows-/Host- und 45 Web-Prüfungen,
insgesamt 165 bestanden; Build ohne Warnungen oder Fehler. Paket-Audit: 482 Dateien
im ZIP identisch mit dem getesteten Payload, Web-Oberfläche identisch mit Quelle,
keine privaten Konfigurationen, Protokolle oder Begleiterprogramme aufgenommen.
Handbuch mit sieben Seiten im Repository, Payload, ZIP und Release-Anhang identisch.
Die bereits dokumentierten zehn isolierten Installationsprüfungen bleiben gültig.

Der getestete Installer wurde vor dem späteren Release-Commit gebaut. Sein
Versionszusatz referenziert den damaligen Ausgangscommit 5faa3db; der geprüfte
Funktionscode entspricht dem vorbereiteten Stand. Änderungen am Veröffentlichungstext
und Video-Link ersetzen keinen App-Build. Die bekannte fehlende Signierung und
die noch offene vollständige echte Aurora-Abnahme stehen in den Release-Hinweisen.
