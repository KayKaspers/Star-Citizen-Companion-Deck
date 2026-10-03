# Konfiguration

Private Konfigurationen liegen außerhalb des Repositorys. SchemaVersion ist 1.
Unbekannte Felder werden abgelehnt, damit Tippfehler nicht verborgen bleiben.

| Feld | Bedeutung |
|---|---|
| companionVariant | Orion oder Aurora; Startauswahl |
| companionEventsPath | Absolute lokale Ereignisdatei, passend zur Startvariante |
| gameLogPath | Absolute lokale Game.log; aktiviert das Ereignis-Terminal |
| externalWindow | Exakter Prozessname und exakter Fenstertitel; optional zusätzlich Fensterklasse |
| externalPlacement | CenterInBay behält native Begleitergröße bei; ResizeToBay fordert die Bereichsgröße an |
| displayNames | XENEON EDGE und dokumentierte alternative Schreibweise XENON EDGE |
| displayKey | Optional expliziter lokaler Anzeigename; kein stabiler Hardware-Identifier |
| mode | Fullscreen oder Windowed |
| refreshMilliseconds | 500 bis 60000; Standard 2000 |
| windowedArea / companionArea | Relative Rechtecke innerhalb des Bildschirms bzw. Anwendungsfensters |

`orionEventsPath` bleibt als ältere Orion-Konfiguration lesbar. Es darf nicht
zusammen mit companionEventsPath angegeben werden und gilt nur für Orion.

## Begleiterprofile

| Variante | Prozess | Begleiterfenster | Standard-Ereignisdatei |
|---|---|---|---|
| Orion | Orion Log-Wächter | Orion Companion | Dokumente\VoiceAttack\Orion Log-Wächter\Orion-Companion-Events.jsonl |
| Aurora | Aurora Log-Wächter | Aurora Orb | Dokumente\VoiceAttack\Aurora Log-Wächter\Aurora-Companion-Events.jsonl |

Orion und Aurora wurden live erkannt. Auroras Begleiterfenster heißt „Aurora Orb“.
Der Standard-Ereignispfad wurde für beide Varianten gefunden; Auroras Datei war
bei der ersten Prüfung noch leer. Eine echte Aurora-Reaktion bleibt zu bestätigen. Aurora wird nicht automatisch gestartet. Tatsächliche Werte mit
`-InspectProcess 'Aurora Log-Wächter'` lesen und bei Bedarf externalWindow und
companionEventsPath in einer privaten Aurora-Konfiguration anpassen.
Ereignisdateien müssen den Varianten-Dateinamen tragen; es erfolgt keine
unklare Suche über beliebige JSONL-Dateien.

Der Begleiter wird durch companionVariant in der Startkonfiguration festgelegt.
Eine Auswahl in der Oberfläche ist nicht erforderlich und wurde entfernt. Das
Deck startet die externen Tools nicht. Für einen anderen Begleiter das Deck normal
beenden und mit dessen privater Startkonfiguration neu starten.
Mehrere gleichnamige Fenster werden nicht positioniert.

Eine fehlende Ereignisdatei erscheint als „Nicht verbunden“. Es werden dann
keine Game.log-Zeilen als angebliche Reaktionen ausgegeben. **Weitere Logs**
kann unabhängig davon das echte Spielprotokoll einblenden.

## Lokale Speicherung

Das Design wird im lokalen WebView2-Profil gespeichert. Konfigurationen werden
durch die Oberfläche nicht überschrieben. Begleiter und Spielprotokoll werden
nicht verändert. Diagnosen enthalten Zustands- und Fehlercodes, keine Rohlogs.

## Einrichtung der installierten Anwendung

Beim ersten Start ohne Argumente erscheint die deutsche Einrichtung. Game.log
ist optional und wird über einen Dateidialog gewählt; ein bestimmtes Laufwerk
ist nicht vorgeschrieben. Bei einem leeren Game.log-Feld sucht die Einrichtung
automatisch auf lokalen Festplatten in typischen LIVE-Installationsordnern unter
Roberts Space Industries bzw. StarCitizen, auch unter Games, Spiele und Program Files.
Es gibt keinen vollständigen Festplattenscan. Nur vorhandene Game.log-Dateien werden
vorgeschlagen; mehrere Treffer erfordern eine Auswahl. Die Schaltfläche
„Game.log suchen“ wiederholt die Suche, „Auswählen…“ erlaubt beliebige eigene Pfade.
Die Ereignisdatei kann separat gewählt werden.
Einstellungen werden unter %LOCALAPPDATA%\StarCitizenCompanionDeck\config.json
gespeichert. --configure öffnet die Einrichtung erneut. Explizite --config-
Aufrufe bleiben möglich und werden nicht in dieses Profil übernommen.

Bei einer Deinstallation bleiben diese Benutzerdaten bewusst erhalten. Eine
Neuinstallation verwendet daher vorhandene Pfade und zeigt nicht erneut die
Ersteinrichtung. Über „Begleiter-Deck einrichten“ im Startmenü lassen sich die
Pfade jederzeit ändern. Das lokale WebView2-Profil mit der Designauswahl liegt
unter %LOCALAPPDATA%\XeneonEdge\WebView2, Diagnosen unter
%LOCALAPPDATA%\XeneonEdge\diagnostics. Diese Ordner werden ebenfalls erhalten.

Die konkreten Prüfschritte stehen in [Aurora-Live-Abnahme](AURORA-LIVE-ABNAHME.md).
