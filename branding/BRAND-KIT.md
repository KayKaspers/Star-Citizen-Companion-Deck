# Brand-Kit V2 – Star Citizen Begleiter-Deck

Stand: 03.10.2026. Überarbeiteter Entwurf zur visuellen Abnahme.

## Gestaltung

Die technische, dunkle Gestaltung orientiert sich am Brand-Kit von
[XENEON-Edge-Control-Deck](https://github.com/KayKaspers/XENEON-Edge-Control-Deck).
Übernommen werden der Anspruch an Präzision, klare Typografie, dezente
Konstruktionslinien und eine gemeinsame Logo-Systemtafel. Die konkreten
Grafiken und das Zeichen des Begleiter-Decks sind eigenständige Entwürfe.

**Produktname:** Star Citizen Begleiter-Deck. **Kürzel:** SCBD.
**Leitsatz:** Deine Ereignisse. Dein Begleiter.

Das verfeinerte „Doppelsignal“ besteht aus zwei gegenläufigen, kantigen
Orbitalbögen und einem zentralen vierstrahligen Stern. Eisblau und Bernstein
bilden ein gleichwertiges Paar für die gemeinsame Orion/Aurora-Grundlage.
Der Banner übersetzt diese Form in ein atmosphärisches Orbitalmotiv.
Die Farben legen nicht verbindlich fest, welche Stimme ausgewählt ist.

## Externe Tools und Integration

Orion und Aurora sind Tools von [Aurora Systems](https://www.aurora-systems.online/),
nicht von KayKaspers oder diesem Projekt. Das Begleiter-Deck ist eine unabhängige
Integration. Sein eigenes Branding bezeichnet ausschließlich das Deck.

Banner, Wortmarke, GitHub-Vorschau und Systemtafel nennen die Integration und
Aurora Systems ausdrücklich. Diesen Herkunftshinweis bei der Verwendung nicht
entfernen oder abschneiden. Das alleinstehende Bildzeichen bezeichnet nur das Deck;
bei einer Präsentation mit Orion oder Aurora gehört der Herkunftshinweis daneben.

## Hauptdateien

| Datei unter assets/ | Format und Zweck |
|---|---|
| banner-v2.png / .svg | 1600 × 400, Repository-/README-Banner |
| social-preview-v2.png / .svg | 1280 × 640, GitHub-Vorschaugrafik |
| logo-system-v2.png / .svg | 2000 × 2000, Logo-Systemtafel mit Varianten, Palette und Typografie |
| wortmarke-v2.png / .svg | 1200 × 280, transparente horizontale Wortmarke |
| symbol-v2.png / .svg | Farbiges Zeichen; PNG 512 × 512, SVG 128 × 128 |
| symbol-monochrom-v2.png / .svg | Helles einfarbiges Zeichen für dunkle Flächen |
| symbol-dunkel-v2.png / .svg | Dunkles einfarbiges Zeichen für helle Flächen |
| orbital-background.png | Unveränderte Hintergrundquelle aus Imagegen |

[VORSCHAU.html](VORSCHAU.html) zeigt die gesamte Familie. Die PNGs halten
Schrift und Darstellung exakt fest. SVG-Kompositionen bleiben editierbar;
Schriften und Bannerhintergrund sind für eigenständige Verwendung eingebettet.
Frühere Entwürfe ohne V2-Suffix wurden bei der Release-Bereinigung entfernt.
Das Projekt-README verwendet jetzt den V2-Banner.

## Farben

| Farbe | Hex | Verwendung |
|---|---|---|
| Nachtblau | #101E2C | Ruhige dunkle Grundfläche |
| Eisblau | #8BD3FF | Erstes Orbital-Signal und technische Akzente |
| Bernstein | #F5CE70 | Zweites Orbital-Signal und ausgewählte Details |
| Sternweiß | #F2F5F7 | Zeichenkern, Haupttitel und Text |
| Nebelgrau | #B4C5D4 | Begleittexte und dezente Beschriftung |

Akzente gezielt einsetzen. Das Orbitalmotiv gehört auf Präsentationsgrafiken;
das Ereignisprotokoll bleibt ruhig und gut lesbar. Hersteller-Themes dürfen
die Produktidentität nicht in vermeintliche Herstellerlogos umwandeln.

## Typografie

**Exo 2:** Wortmarke und große Branding-Titel, bevorzugt Gewicht 600–700.
**Inter:** Leitsatz, Angaben, Erläuterungen und Systemtafel, Gewicht 400–600.
Beide variablen Schriftdateien stammen aus dem offiziellen Google-Fonts-
Repository. Die unveränderten SIL-OFL-Lizenztexte liegen jeweils in fonts/.
Die bestehende Terminal-Schrift wird durch dieses Brand-Kit nicht geändert.

## Verwendung

Bildzeichen mindestens 32 Pixel breit anzeigen. Ringsum mindestens ein Achtel
der Zeichenbreite freilassen. Nicht verzerren, drehen oder den Stern entfernen.
Auf hellen Flächen das dunkle einfarbige Zeichen verwenden, auf dunklen Flächen
das farbige oder helle einfarbige Zeichen. Wortmarken-PNGs sind transparent.
Das Banner nicht so zuschneiden, dass Titel oder Begleiternamen verschwinden.
Für GitHub die gesonderte Vorschaugrafik verwenden.

## Quellen und Reproduktion

[QUELLEN.md](QUELLEN.md) dokumentiert Referenz, Schriftquellen und Imagegen-
Prompt. Der Hintergrund wurde mit dem eingebauten Imagegen-Werkzeug erstellt;
Logo, Rahmen, Texte und Layout sind eigene SVG-Kompositionen.

```powershell
node tools/build-brand-kit.cjs
node tools/render-brand-kit.cjs
```

Die Befehle werden vom Repository-Ordner aus ausgeführt. Voraussetzung ist
`npm ci` und der für die bestehenden Browserprüfungen verwendete Browser.
Der Render-Schritt wartet auf die Schriften und prüft die Textgrenzen.

Es wurden keine Orion-/Aurora-Roboter, Sprachdateien, Schiffe, Herstellerlogos
oder XENEON-Produktbilder übernommen. Die grafische Familie bezeichnet ein
unabhängiges Community-Projekt und behauptet keine offizielle Partnerschaft.
Commits, Pushes, Tags und Releases wurden nicht ausgeführt.
