# Techdemo und Werbevideo V2

Stand: 3. Oktober 2026. Lokal exportiert und geprüft; noch kein Upload oder Veröffentlichung.

Die beiden Videos haben unterschiedliche Aufgaben. Die **40-Sekunden-Techdemo**
erklärt die Bedienung in vier Schritten mit großer, frontaler Oberfläche und ruhiger
Musik. Das **60-Sekunden-Werbevideo** inszeniert das Deck auf dem XENEON EDGE mit
wechselnden Bildgrößen, 13 Einstellungen und einer deutschen Erzählerstimme.

## Dateien

| Ausgabe | Lokale Exportdatei | Ziel |
|---|---|---|
| Techdemo, 40 Sekunden | `D:/Projects/SCBD-Videos/export/Begleiter-Deck-Techdemo-V2-40s-1080p.mp4` | README / GitHub |
| Werbevideo, 60 Sekunden | `D:/Projects/SCBD-Videos/export/Begleiter-Deck-Werbevideo-V2-60s-YouTube-1080p.mp4` | YouTube |

Die Vollauflösungs-Exporte verwenden 1920 × 1080 Pixel, 30 fps und H.264/AAC.
Das README-Paket verwendet `docs/media/techdemo.mp4` und die stumme animierte
Vorschau `docs/media/techdemo-vorschau.gif`. Diese bestehenden Dateien wurden durch
die V2-Techdemo ersetzt; die README-Pfade bleiben gleich.

## README

Die GIF-Vorschau kann direkt in der README erscheinen und zum vollständigen MP4
mit Ton führen. Ein passender Beschreibungstext:

> Die 40-Sekunden-Techdemo zeigt vier Bedienungsschritte: gemeldete Begleiter-Reaktionen
> live lesen, mit Kategorien eingrenzen, weitere Logs zuschalten und das Herstellerdesign
> wählen. Echte Anwendungsaufnahmen, Originalton des Begleiters und ruhige Musik.

```markdown
[![Techdemo: Begleiter-Deck in vier Schritten](docs/media/techdemo-vorschau.gif)](docs/media/techdemo.mp4)

[Techdemo als MP4 mit Ton öffnen](docs/media/techdemo.mp4).
Die animierte Vorschau ist stumm.
```

Für einen **nativen GitHub-Player mit Ton** muss die MP4 nach Freigabe im GitHub-
Markdown-Editor als Medienanhang hochgeladen werden. Die von GitHub erzeugte
`github.com/user-attachments/assets/...`-Adresse als eigenen Absatz in der README
einfügen und die gerenderte Wiedergabe prüfen. Diese Adresse ist noch nicht vorhanden.
Der lokale MP4-Link ersetzt diesen Upload nicht. Kein selbst gebautes HTML-Video
oder YouTube-iframe in die README einfügen.

Offizielle Hinweise: [Dateien an Markdown anhängen](https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/attaching-files),
[Nicht-Code-Dateien anzeigen](https://docs.github.com/en/repositories/working-with-files/using-files/working-with-non-code-files).

## YouTube

Titel, Beschreibung, Musikcredits und vier Zeitmarken stehen in `YOUTUBE-TEXT-V2.md`.
Das Werbevideo verwendet eine deutsche ElevenLabs-Erzählerstimme. Von **0:08–0:14**
und **0:35–0:42** bleibt der Originalton aus den bereitgestellten Aufnahmen frei von
zusätzlicher Erzählung. Die ruhigere Techdemo erhält keine Erzählerstimme.

## Produktionsdateien

Die editierbaren HyperFrames-Projekte liegen unter
`D:/Projects/SCBD-Videos/techdemo` und `D:/Projects/SCBD-Videos/werbevideo`.
Rohaufnahmen, Render-Caches, einzelne Musikdownloads und Sprachsegmente bleiben in
der lokalen Produktion. Das Anwendungsrepository erhält die fertige README-Demo
und ihre Vorschau; die Musik wird nicht separat weitergegeben.

Die Herkunft der Medien dokumentiert `VIDEO-MEDIEN-V2.md`. Orion und Aurora bleiben
als externe Tools von Aurora Systems gekennzeichnet. Das Deck ist ihre unabhängige
Integration und erzeugt keine eigene Begleiter-Sprachausgabe.

Ein Commit, Push, Medienupload oder YouTube-Upload erfolgt erst nach ausdrücklicher
Freigabe des Human Maintainers.
