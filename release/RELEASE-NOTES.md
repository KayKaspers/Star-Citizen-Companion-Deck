# Star Citizen Begleiter-Deck 0.1.0

Ein deutsches Ereignis-Deck für separat gestartete Orion-/Aurora-Begleiter,
optimiert für den Corsair XENEON EDGE.

- Standardmäßig nur vom Begleiter gemeldete Reaktionen; weitere Spiel-Logs zuschaltbar.
- Große Meldungsschrift, Kategorien, Textfilter und automatisches Scrollen.
- Verspielte Orbitalgestaltung, 15 Herstellerdesigns und Blackhole-Dynamics-Logo.
- DPI-sichere Windows-Fensterpositionierung und Rückgabe beim normalen Beenden.
- Deutsche Ersteinrichtung mit frei wählbaren lokalen Dateien, ohne festes Laufwerk.
- Automatische Vorschläge für Game.log aus typischen LIVE-Installationen auf lokalen Laufwerken.
- Größeres, scharfes App-Icon für Desktop, Startmenü und Programmdatei.
- Versionsabhängiger Icon-Dateiname für Verknüpfungen, um alte Windows-Icon-Caches zu umgehen.
- Gespeicherte Pfade bleiben nach Deinstallation erhalten; „Begleiter-Deck einrichten“ öffnet die Einrichtung erneut.
- Deutsches PDF-Benutzerhandbuch im Repository, Installer und portablen Paket; im Startmenü direkt erreichbar.
- Benutzer-Installer mit .NET-Laufzeit und portable x64-Ausgabe.
- Deinstallation schließt das Deck regulär und wartet auf das Ende; bei Fehlern Abbruch vor Dateilöschungen.
- Schutz gegen mehrere gleichzeitig laufende Deck-Instanzen derselben Windows-Sitzung.

**Orion und Aurora sind externe Tools von [Aurora Systems](https://www.aurora-systems.online/).
Sie wurden nicht von KayKaspers oder diesem Projekt entwickelt. Das Deck ist eine
unabhängige Integration und enthält keine Roboter- oder Sprach-Assets.**

Windows 10 ab 1809 oder Windows 11 (x64), WebView2 und iCUE-Desktop-Modus erforderlich.
Wenn WebView2 fehlt, benötigt der Installer Internet. Orion oder Aurora separat starten.
Die Vergrößerung des Aurora-Roboters über die Windows-Fenstergröße wird nicht unterstützt.
Ereignismeldungen bestätigen keine tatsächliche Audiowiedergabe. Bei erzwungenem
Beenden ist die Fensterwiederherstellung nicht garantiert.

Installer und Anwendung sind nicht digital signiert. Die vollständige Live-Abnahme einer echten Aurora-Reaktion inklusive Audiowiedergabe steht noch aus.
