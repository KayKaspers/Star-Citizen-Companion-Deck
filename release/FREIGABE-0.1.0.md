# Freigabevorbereitung 0.1.0

Stand: 3. Oktober 2026. Veröffentlichung vom Maintainer ausdrücklich freigegeben; noch nicht veröffentlicht.

## Vorgesehene Veröffentlichung

Repository: KayKaspers/Star-Citizen-Companion-Deck. Vorgesehener Tag: v0.1.0.
Titel: Star Citizen Begleiter-Deck 0.1.0. Beschreibung: release/RELEASE-NOTES.md.

Dateien aus artifacts/0.1.0:

- Begleiter-Deck-0.1.0-win-x64-Setup.exe
- Begleiter-Deck-0.1.0-win-x64-portabel.zip
- Begleiter-Deck-0.1.0-Benutzerhandbuch.pdf
- SHA256SUMS.txt

Den heruntergeladenen WebView2-Bootstrapper nicht als eigenen Release-Anhang
hochladen; der Installer enthält ihn bereits. Build- und Testordner bleiben
außerhalb der Git-Historie.

## Abgeschlossen

- Anwendungstests: 45 Core, 21 Log, 54 Windows/Host und 45 Web, insgesamt 165.
- Version 0.1.0 mit gesperrter Paketauflösung und .NET 8.0.31 gebaut.
- Installer, portables Paket und separates PDF erzeugt; SHA256-Prüfsummen geprüft.
- Handbuch: 7 Seiten, eingebettete Schrift, deutsche Umlaute, 2 klickbare Links;
  alle Seiten gerendert und visuell geprüft. README-Link vorhanden.
- PDF identisch im Repository, Installationspaket, ZIP und Release-Anhang.
- Isolierte Installation mit eigener AppId und eigenem Startmenü-Ordner: 10
  Prüfungen bestanden, einschließlich Handbuchinstallation und Verknüpfung,
  Icon-Datei, normalem Prozessende bei Deinstallation, Fensterwiederherstellung,
  vollständiger Entfernung der Programmdateien und Verknüpfungen sowie Erhalt
  privater Testeinstellungen.
- Keine privaten Konfigurationen, Game.log, Begleiter-Events oder PDB-Dateien
  im portablen Paket. Microsoft-WebView2-Bootstrapper-Signatur geprüft.
- Originale Orion-/Aurora-Assets und Programme nicht in das Paket aufgenommen;
  Herstellerhinweis in README, Handbuch und Release-Beschreibung.

## Vor Veröffentlichung offen

Der Maintainer hat die konkrete Ausgabe bestätigt und die Veröffentlichung freigegeben.
Commit und Push führt er selbst aus. Der Medienupload erfolgt ebenfalls durch ihn. Siehe release/VEROEFFENTLICHEN.md und release/Publish-Release.ps1. Noch kein Commit, Push, Tag oder Release durch den Agenten ausgeführt.
Ein zusätzlicher Test auf einem frischen Windows-Benutzerkonto ohne WebView2
bleibt empfohlen; die lokale Maschine besitzt WebView2 bereits. Installer und
Anwendung sind nicht digital signiert. Die echte Begleiter-/Audiowiedergabe ist
nicht durch synthetische Integrationstests bewiesen.

Der Maintainer hat RC4 im vorherigen Test als passend bestätigt. Der Funktionscode
von 0.1.0 entspricht RC4; hinzu kommen das Handbuch und seine Paketintegration.
