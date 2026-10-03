# Architektur

Die Windows-Anwendung und ihre Web-Oberfläche sind getrennt. Core enthält
Zustände, Konfiguration, Ereignisparser, begrenztes Dateilesen und Regeln zur
Fensterpositionierung. Windows liefert Bildschirme, DPI, Fensteridentität und
Dateiidentität. Der WinForms-Host besitzt WebView2 und sendet reine
Zustandsprojektionen an die lokale Oberfläche. Der Browser greift nicht selbst
auf Windows oder Dateien zu. Es läuft kein Webserver.

Alle Platzierungen verwenden physische Pixel mit DPI-Kontext pro Monitor.
Bildschirmauflösung allein identifiziert kein EDGE. Fehlende, duplizierte oder
mehrdeutige Anzeigen werden ausdrücklich gemeldet. Das Begleiterfenster wird
als eigenes Top-Level-Fenster verschoben, ohne Parent, Fensterstil, Fokus oder
Z-Reihenfolge umzuschreiben. Handle, Prozessidentität und Startzeit schützen
gegen die Wiederverwendung eines inzwischen fremden Fensterhandles.

Das gemeinsame Orion/Aurora-Ereignisschema besteht aus schemaVersion=1, type
und timestampUtc. Optional name für Schiff und Bauplan. Unterstützt werden
Schiffserkennung, Bauplan, Spielertod, Serverfehler 30000 sowie Ein-/Austritt
von Sicherheitszone, Sperrzone und überwachtem Raum. Beide Varianten erzeugen
dieselben deutschen Texte; die Quelle trägt den gewählten Begleiternamen.
Ungültige oder unbekannte Ereignisse werden verworfen und gezählt.

Dateien werden pro Abfrage kurz mit FileShare.ReadWrite|Delete lesend geöffnet.
Pro Abfrage höchstens 256 KiB, Rückblick beim Start höchstens 64 KiB,
Zeilenpuffer höchstens 64 KiB, pro Quelle höchstens 500 Ereignisse; die Ansicht
zeigt höchstens 200 Treffer. Nur abgeschlossene UTF-8-Zeilen erscheinen.
Dateiwechsel, Kürzung, Überschreiben und Wiederverbindung beginnen eine neue
Generation. Jede Quelle hat einen eigenen Zustand und historischen Rückblick.

Weitere Logs ergänzt Game.log-Ereignisse, ohne eine sichere Zuordnung zu einer
Begleiter-Reaktion vorzutäuschen. Es gibt keine garantierte Deduplizierung der
beiden Quellen. Die Best-effort-Redaktion ausgewählter Metadaten ist keine
Anonymisierung für öffentliche Exporte. Dieses Repository enthält keine
echten Spielprotokolle oder privaten Konfigurationen.

Escape und Schließen geben verwaltete Fenster vor dem Beenden zurück. Eine
abgebrochene oder erzwungen beendete Anwendung kann diese Rückgabe nicht
garantieren. iCUE muss manuell im Desktop-Modus stehen; ein erfolgreicher
Fensterzustand bestätigt nicht den iCUE-Anzeigemodus.
