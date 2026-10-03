# Release vorbereiten

Version: **0.1.0**, Windows x64. Lokal vorbereitet, noch nicht veröffentlicht.

## Bauen

Voraussetzungen: .NET SDK 8.0.425 und Inno Setup 7.1.0. Vorher die laufende
Deck-Sitzung regulär schließen. Vom Projektordner aus:

```powershell
./runtime/Test-Runtime.ps1 -IncludeWebChecks
./release/Build-Release.ps1 -InnoCompiler 'C:\Pfad\zu\Inno Setup\ISCC.exe'
```

Die x64-Paketauflösung ist in packages.win-x64.lock.json der drei Laufzeitprojekte
fixiert. .NET 8.0.31 wird mitgeliefert. Das Skript prüft die Microsoft-Signatur des
WebView2-Bootstrappers. Eine neue Ausgabe braucht einen leeren artifacts/Versionsordner.
Build-Ausgaben werden nicht in Git aufgenommen.

Ergebnisse unter artifacts/0.1.0/:

- Begleiter-Deck-0.1.0-win-x64-Setup.exe
- Begleiter-Deck-0.1.0-win-x64-portabel.zip
- SHA256SUMS.txt
- Begleiter-Deck-0.1.0-Benutzerhandbuch.pdf

Das PDF liegt versioniert unter docs/Benutzerhandbuch.pdf. Die deutsche Textquelle
steht unter docs/BENUTZERHANDBUCH.md. tools/build-manual.py erzeugt das Dokument
mit Python/ReportLab und eingebetteter Windows-Arial-Schrift; die Ausgabe anschließend
rendern und visuell prüfen. Der Installer und das portable Paket liefern dieselbe
PDF-Fassung als Benutzerhandbuch.pdf mit. Im Startmenü gibt es einen Handbuch-Eintrag.

## Verhalten

Installer auf Deutsch, Installation im Benutzerkonto ohne Administratorrechte,
Startmenü-Verknüpfungen für Start und Einrichtung, optional Desktop-Verknüpfung.
Er startet das Deck nicht ungefragt. WebView2 wird nur nachinstalliert, wenn die
Microsoft-Registrierung keine installierte Runtime meldet. Der Download benötigt
Internet; Fehler werden angezeigt. .NET ist bereits im Anwendungspaket enthalten.

Die erste Einrichtung wählt Orion oder Aurora sowie eigene lokale Dateien.
Game.log ist optional, ohne festes Laufwerk. Die Einrichtung schlägt vorhandene
Game.log-Dateien aus typischen LIVE-Ordnern auf lokalen Festplatten vor. Mehrere
Treffer werden zur Auswahl angezeigt, fehlende Treffer können manuell ergänzt werden.
Gespeicherte Pfade werden beim Öffnen der Einrichtung nicht automatisch überschrieben.
Das Deck verändert die Dateien nicht.
Die Deinstallation entfernt Programmdateien und Verknüpfungen; private Einstellungen
bleiben erhalten. Orion/Aurora werden weder installiert noch deinstalliert.
Vor dem Entfernen der Dateien ruft der Deinstaller --shutdown auf. Dieser Modus
sendet normale Schließanforderungen ausschließlich an Deck-Prozesse derselben
Programmdatei und Windows-Sitzung, wartet auf deren Ende und beendet keine Prozesse
gewaltsam. Bei einem Fehler wird vor Dateilöschungen abgebrochen. AppMutex schützt
zusätzlich vor einer laufenden Deck-Sitzung; die Anwendung verhindert Mehrfachstarts.

## Freigabe

Vor Veröffentlichung: Installer installieren, ersten Start und Einrichtung prüfen,
Aurora-/Orion-Reaktion beobachten und normales Beenden mit Fensterwiederherstellung
bestätigen. Installer und portable Anwendung sind derzeit nicht digital signiert.
Ein zusätzlicher Test auf einem frischen Windows-Benutzerkonto ist empfohlen.

Commits, Pushes, Tags und GitHub-Releases benötigen die ausdrückliche Freigabe des
Maintainers. Dieses Skript veröffentlicht nichts.

## Deinstallation mit laufendem Deck prüfen

```powershell
./release/Test-Uninstall.ps1 -InnoCompiler 'C:\Pfad\zu\Inno Setup\ISCC.exe'
```

Die Prüfung kompiliert aus dem Kandidatenpaket einen Testinstaller mit eigener
AppId. Sie schützt dadurch bestehende Produktregistrierungen. Sie verwendet nur
eigene Logdaten und ein eigenes Begleiter-Testfenster, startet das Deck und
deinstalliert es während des Betriebs. Geprüft werden Prozessende,
Fensterwiederherstellung, vollständige Dateientfernung und Erhalt der Konfiguration.
