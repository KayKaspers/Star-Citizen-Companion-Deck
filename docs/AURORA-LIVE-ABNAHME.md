# Live-Abnahme mit Aurora

Orion und Aurora sind externe Tools von [Aurora Systems](https://www.aurora-systems.online/).
Das Begleiter-Deck integriert sie unabhängig. Diese Prüfung bestätigt die echte
Aurora-Installation; synthetische Tests ersetzen sie nicht.

## Vorbereitung

1. Deck normal schließen, damit ein verwaltetes Begleiterfenster zurückgegeben wird.
2. Aurora selbst starten und ihr Begleiterfenster anzeigen lassen.
3. XENEON EDGE in iCUE auf Desktop stellen.
4. Eine private Kopie von runtime/runtime.aurora.example.json außerhalb des
   Repositorys verwenden. gameLogPath und companionEventsPath auf die tatsächlichen
   Dateien setzen. Keine Spielprotokolle oder private Konfigurationen veröffentlichen.

## Fensteridentität lesen

Vom Projektordner aus, mit einem verfügbaren .NET-8-SDK:

```powershell
./runtime/Start-Runtime.ps1 -ConfigPath './runtime/runtime.aurora.example.json' -InspectProcess 'Aurora Log-Wächter'
```

Diese Abfrage liest die Fensteridentität und bewegt keine Fenster. Den tatsächlich
beobachteten Prozess und Begleiterfenstertitel in die private Konfiguration übernehmen.
Eine dynamische WPF-Fensterklasse nicht als dauerhaften Standard festschreiben.

## Abnahmekriterien

- Das Deck erkennt Aurora eindeutig und zentriert ihr separates Fenster in der Bay.
- Ein neues, von Aurora gemeldetes Ereignis erscheint im Standardfilter „Nur Aurora“.
- Die Reaktion stammt aus Auroras Ereignisdatei; die Sprachausgabe erfolgt durch Aurora.
- „Weitere Logs“ schaltet das Spielprotokoll hinzu; Abschalten entfernt diese Zeilen wieder.
- Designwechsel und größere Schrift bleiben auf dem EDGE gut lesbar.
- „Begleiter freigeben“ und normales Beenden mit Escape stellen die ursprüngliche
  Fensterposition wieder her.
- Falls Orion ebenfalls läuft, verwaltet das Deck nur den ausgewählten Begleiter.

Nur bestätigte Prozess-/Fensternamen und Prüfergebnisse dokumentieren. Keine echten
Ereignisinhalte, persönlichen Pfade oder Audiodateien ins Repository übernehmen.

## Aktueller Stand

Am 03.10.2026 live bestätigt: Prozess „Aurora Log-Wächter“, Begleiterfenster
„Aurora Orb“, separates Steuerfenster „Aurora Log-Wächter“. Der erwartete
Aurora-Ereignisfeed existiert, war bei der Prüfung aber leer. Fensterpositionierung,
Wiederherstellung und eine echte Reaktion bleiben zur Live-Abnahme offen.

## Größenversuch am echten Fenster

Am 03.10.2026 wurde über die bestehende Windows-Platzierung eine Größe von
540 × 645 Pixeln angefordert (150 Prozent der nativen 360 × 430 Pixel). Aurora
behielt 360 × 430 Pixel bei; die Platzierung meldete korrekt unbestätigt. Der
Versuch wurde regulär beendet und CenterInBay wieder gestartet. Keine Binär-,
WPF- oder Assetänderung. Eine Größenänderung benötigt eine von Aurora selbst
unterstützte Einstellung; diese wurde bisher nicht bestätigt.
