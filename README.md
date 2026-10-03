<p align="center"><img src="branding/assets/banner-v2.png" alt="Star Citizen Begleiter-Deck – Orion und Aurora" width="100%"></p>

# Star Citizen Begleiter-Deck

**Orion und Aurora sind externe Tools von [Aurora Systems](https://www.aurora-systems.online/).
Sie wurden nicht von KayKaspers oder diesem Projekt entwickelt. Das Begleiter-Deck
ist eine unabhängige Integration dieser separat betriebenen Tools.**

Ein deutsches Ereignis-Deck für **Orion und Aurora**, optimiert für das
**Corsair XENEON EDGE mit 2560 × 720 Pixeln**. Orion spricht mit männlicher,
Aurora mit weiblicher deutscher Stimme. Die Sprachausgabe kommt weiterhin
aus dem jeweiligen Log-Wächter.

Das Deck zeigt standardmäßig die vom gewählten Begleiter gemeldeten
Reaktionsereignisse. **Weitere Logs** ergänzt das Spielprotokoll. Große Schrift,
Textfilter, Kategorien und automatisches Scrollen helfen beim Lesen im Spiel.
Der Begleiter bleibt ein separates Windows-Fenster und wird im rechten Bereich
positioniert; beim normalen Beenden wird seine ursprüngliche Position wiederhergestellt.

## Stand

Entwicklungsstand zur Prüfung, noch keine veröffentlichte Version.
Aus dem Projekt XENEON-Edge-Control-Deck ausgegliedert; Grundlage sind dessen
Laufzeit-Arbeitspakete XEE-WP-025 und XEE-WP-026. Orion wurde am echten System
geprüft. Aurora verwendet denselben Ereignisvertrag; ihre tatsächlichen
Fenstertitel und Ausgabepfade müssen beim ersten Einsatz bestätigt werden.

## Voraussetzungen

- Windows 10 ab Version 1809 oder Windows 11, interaktiver Desktop.
- .NET 8 Desktop Runtime und Microsoft Edge WebView2 Runtime.
- Zum Bauen: .NET SDK 8.0.425 oder eine passende neuere 8.0.4xx-Version.
- Separat gestarteter Orion- oder Aurora-Log-Wächter und Star Citizen.
- XENEON EDGE in iCUE unter **Bildschirmeinrichtung → Desktop** umschalten.

## Starten

1. `runtime/runtime.orion.example.json` oder `runtime/runtime.aurora.example.json`
   in einen privaten Ordner kopieren.
2. `gameLogPath` auf die lokale `Game.log` setzen.
3. `companionEventsPath` auf die Ereignisdatei des gewählten Begleiters setzen.
   Beispiel für eine eigene Installation, mit entsprechend angepassten Pfaden:

```json
{
  "schemaVersion": 1,
  "mode": "Fullscreen",
  "companionVariant": "Orion",
  "externalWindow": {
    "processName": "Orion Log-Wächter",
    "title": "Orion Companion"
  },
  "externalPlacement": "CenterInBay",
  "gameLogPath": "D:\\Spiele\\StarCitizen\\LIVE\\Game.log",
  "companionEventsPath": "C:\\EigeneDateien\\Orion-Companion-Events.jsonl"
}
```

4. Vom Repository-Ordner starten:

```powershell
./runtime/Start-Runtime.ps1 -ConfigPath 'C:\EigeneDateien\deck.json' -PlaceExternal
```

**Escape** beendet die Anwendung, **F11** wechselt den Fenstermodus.
Die Auswahlliste **Begleiter** schaltet zwischen Orion und Aurora um. Dabei wird
zuerst das bisherige Fenster freigegeben. Das Deck startet keinen Log-Wächter.
Für die andere Variante gelten zunächst die dokumentierten Standardpfade;
abweichende Installationen mit einer eigenen Konfigurationsdatei starten.
Die Designauswahl wird lokal gespeichert. Rohtexte aus dem Spielprotokoll
werden in ihrer ursprünglichen Sprache angezeigt.

## Designs und Brand-Kit

Zusätzlich zum neutralen Deck-Design gibt es **15 Herstellerdesigns**: RSI,
Anvil, Aegis, Drake, Origin, Crusader, MISC, Argo, Banu, Consolidated Outland,
Esperia (Tevarin), Kruger, Mirai, Vanduul und Aopoa. Damit sind alle Gruppen der
vom Maintainer verlinkten deutschen Herstellerliste abgedeckt.
Die Designs variieren Farben, Rahmen, Formen und
Typografie. Sie sind eigene Interpretationen der Herstellerstile und enthalten
keine übernommenen Herstellerlogos oder Schiffsbilder.

- [Recherche und Gestaltung der Designs](docs/THEMEN-RECHERCHE.md)
- [Brand-Kit mit Zeichen, Wortmarke und Banner](branding/BRAND-KIT.md)
- [Konfiguration und Begleiterwechsel](docs/KONFIGURATION.md)
- [Architektur und technische Grenzen](docs/ARCHITEKTUR.md)
- [Prüfergebnisse](evidence/PRUEFUNG-2026-10-03.md)

## Prüfen

```powershell
npm ci
./runtime/Test-Runtime.ps1 -IncludeWebChecks
```

Node.js ab Version 22 wird nur für Browserprüfungen benötigt. Unter Windows
nutzen diese den installierten Microsoft Edge; auf anderen Systemen muss der
Playwright-Browser bereitstehen. Die eigentliche Anwendung benötigt weder
Node.js noch einen Webserver, NDF oder CDS.

## Abgrenzung

Das Deck liest lokale Dateien und übernimmt Fensterpositionierung. Es verändert
keine Begleiter-Binärdateien, bettet keine fremden WPF-Fenster ein und übernimmt
keine Sprach-, Roboter- oder sonstigen Begleiter-Assets. Ereignisse bestätigen
die Meldung des Wächters, nicht die erfolgreiche Audiowiedergabe.
Ältere Meldungen werden gekennzeichnet. Schiff und Standort werden nicht
allein aus beliebigen Entity-Namen als aktueller Spielerzustand behauptet.

Ein unabhängiges Community-Projekt. Star Citizen und die genannten Hersteller
gehören zu ihren jeweiligen Rechteinhabern; es besteht keine offizielle
Verbindung zu Cloud Imperium Games, Roberts Space Industries oder Corsair.
