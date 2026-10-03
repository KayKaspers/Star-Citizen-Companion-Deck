# Release-Prüfung – 03.10.2026

## Ergebnis

Release-Kandidat **0.1.0-rc.1** lokal gebaut: deutscher Benutzer-Installer,
portable Windows-x64-Ausgabe und SHA-256-Prüfsummen. Kein GitHub-Release,
kein Tag, kein neuer Commit und kein Push durch den Agenten.

154 automatisierte Prüfungen bestanden: 45 Core, 21 Dateilesen/Parser,
43 Windows/Host, 45 Oberfläche. Build ohne Warnungen oder Fehler.
Neue Prüfungen bestätigen das Speichern einer frei gewählten lokalen Game.log,
Ablehnung ungültiger Dateien ohne Verlust der gespeicherten Konfiguration,
Begleiterbetrieb ohne Game.log und exakte Zentrierung der Erstellergruppe
bei 2560 und 1707 Pixeln Breite.

## Installer und Paket

- Inno Setup 7.1.0 aus der offiziellen Quelle, gültige Pyrsys-Signatur geprüft.
- Selbständige x64-Anwendung mit .NET 8.0.31; keine vorhandene .NET-Installation nötig.
- Signierter Microsoft-WebView2-Bootstrapper beigefügt, nur bei fehlender Runtime ausgeführt.
- Installation in einem isolierten Testordner erfolgreich, ohne Windows-Neustart.
- Installierte Anwendung ohne dotnet-Aufruf gestartet; WebView2 und beide lokalen
  Logquellen verbunden. Aurora war zu diesem Zeitpunkt beendet; das Deck meldete
  deshalb korrekt das fehlende Begleiterfenster als eingeschränkten Zustand.
- Installierte Anwendung regulär beendet; Deinstallation erfolgreich und Programmdatei entfernt.
- Portable Paketprüfung: keine privaten Konfigurationen, echten Logs oder PDB-Dateien.
- Vollständige .NET-/WebView2-SDK-Lizenztexte im Paket enthalten.

## Bereinigung

V1-Logoentwürfe, doppelte Branding-Vorschauen, alte Hersteller-Screenshots,
deren Übersicht und das veraltete Vorschauwerkzeug aus dem Repository-Arbeitsbaum
entfernt und außerhalb des Repositorys archiviert. Aktuelles V2-Brand-Kit,
Quellen, Lizenztexte, Tests, Architektur und eine aktuelle UI-Vorschau bleiben erhalten.
Builds, Installer, ZIPs und private Einstellungen werden nicht in Git aufgenommen.
Die bestehenden Git-Commits und deren Historie wurden nicht umgeschrieben.

## Grenzen vor Veröffentlichung

Installer und eigene Anwendung sind nicht digital signiert. Die fehlende-WebView2-
Nachinstallation wurde nicht ausgeführt, da die Runtime bereits vorhanden war.
Ein frisches Windows-Konto bzw. ein Rechner ohne WebView2 wurde nicht getestet.
Die echte Aurora-Fensterplatzierung wurde zuvor bestätigt; eine vollständige
Aurora-Sprachausgabe-Abnahme ist keine Aussage der synthetischen Prüfungen.

Quellen: [Inno-Setup-Installationsmodus](https://jrsoftware.org/ishelp/topic_setup_privilegesrequired.htm),
[Microsoft-WebView2-Verteilung](https://learn.microsoft.com/microsoft-edge/webview2/concepts/distribution),
[.NET-8-Veröffentlichungsdaten](https://builds.dotnet.microsoft.com/dotnet/release-metadata/8.0/releases.json).
