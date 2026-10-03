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
| Aurora | Aurora Log-Wächter | Aurora Companion | Dokumente\VoiceAttack\Aurora Log-Wächter\Aurora-Companion-Events.jsonl |

Orion wurde live beobachtet. Aurora-Prozess/Titel/Pfad sind aufgrund der
identischen Variante vorgeschlagene Standardwerte und noch nicht live
bestätigt. Aurora wird nicht automatisch gestartet. Tatsächliche Werte mit
`-InspectProcess 'Aurora Log-Wächter'` lesen und bei Bedarf externalWindow und
companionEventsPath in einer privaten Aurora-Konfiguration anpassen.
Ereignisdateien müssen den Varianten-Dateinamen tragen; es erfolgt keine
unklare Suche über beliebige JSONL-Dateien.

Der UI-Wechsel gibt das alte Fenster zuerst zurück. Bei ausstehender
Wiederherstellung bleibt die bisherige Auswahl aktiv. Die Variante aus der
Startkonfiguration verwendet ihre eigenen konfigurierten Pfade; die andere
verwendet die obigen Standardwerte. Der UI-Wechsel gilt für diese Sitzung.
Für zwei abweichende Installationen jeweils eine eigene Startkonfiguration
verwenden. Mehrere gleichnamige Fenster werden nicht positioniert.

Eine fehlende Ereignisdatei erscheint als „Nicht verbunden“. Es werden dann
keine Game.log-Zeilen als angebliche Reaktionen ausgegeben. **Weitere Logs**
kann unabhängig davon das echte Spielprotokoll einblenden.

## Lokale Speicherung

Das Design wird im lokalen WebView2-Profil gespeichert. Konfigurationen werden
durch die Oberfläche nicht überschrieben. Begleiter und Spielprotokoll werden
nicht verändert. Diagnosen enthalten Zustands- und Fehlercodes, keine Rohlogs.
