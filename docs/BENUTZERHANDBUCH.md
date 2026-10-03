# Star Citizen Begleiter-Deck
Benutzerhandbuch | Version 0.1.0 | Stand: 3. Oktober 2026

Deine Begleiter-Reaktionen im Blick.

Das Begleiter-Deck zeigt die von Orion oder Aurora gemeldeten Ereignisse in einer deutschen Oberfläche. Es positioniert das originale Begleiterfenster neben dem Protokoll und ist für den Corsair XENEON EDGE mit 2560 x 720 Pixeln ausgelegt.

Orion und Aurora sind eigenständige Tools von Aurora Systems. Sie wurden nicht von KayKaspers oder Blackhole Dynamics entwickelt. Dieses Projekt ist eine unabhängige Integration; Roboter und Sprachausgabe stammen weiterhin aus dem separat gestarteten Begleiter.

Dieses Handbuch erklärt den normalen Betrieb. Persönliche Pfade, Sprachdateien und Begleiter-Assets sind nicht enthalten.

## 1 | Installation und Schnellstart
### Was du benötigst
Windows 10 ab Version 1809 oder Windows 11 auf einem x64-PC, einen als Desktop-Monitor eingerichteten XENEON EDGE und Microsoft Edge WebView2. Die .NET-Laufzeit wird mitgeliefert. Fehlt WebView2, benötigt dessen Installation eine Internetverbindung.

Orion oder Aurora musst du separat beziehen, einrichten und starten. Die benötigte Ereignisdatei wird vom jeweiligen Log-Wächter erzeugt. Das Deck installiert und startet diese Tools nicht. Orion verwendet eine männliche, Aurora eine weibliche deutsche Stimme.

### In fünf Schritten starten
1. Führe den Begleiter-Deck-Installer aus. Er installiert für dein Benutzerkonto. Eine Desktop-Verknüpfung ist optional; die Startmenü-Einträge werden angelegt.
2. Stelle in iCUE unter XENEON EDGE > Bildschirmeinrichtung den Anzeigemodus auf Desktop. Im iCUE-Widget-Modus siehst du das Deck nicht als normale Anwendung.
3. Starte Orion oder Aurora und dessen Begleiterfenster.
4. Starte Begleiter-Deck und bestätige bei der ersten Einrichtung Begleiter und Dateipfade. Hinweise zur Dateiwahl folgen auf Seite 3.
5. Starte Star Citizen. Bei neuen gemeldeten Reaktionen erscheinen Einträge im Ereignisprotokoll.

Der Installer startet die Anwendung nicht automatisch. Eine Anmeldung bei GitHub ist für die lokale Nutzung nicht erforderlich. Node.js und ein eigener Webserver werden nicht benötigt.

### Sicher beenden
Escape oder Schließen beendet das Deck regulär und gibt das externe Begleiterfenster an seine ursprüngliche Position zurück. Bei erzwungenem Prozessende ist diese Wiederherstellung nicht garantiert.

## 2 | Begleiter und Dateien einrichten
### Orion oder Aurora festlegen
Die Einrichtung legt fest, welches separat gestartete Tool das Deck verbindet. Im normalen Deck gibt es deshalb keine zusätzliche Begleiter-Auswahlliste. Zum Wechsel die Deck-Sitzung schließen und im Startmenü Begleiter-Deck einrichten öffnen.

### Die Begleiter-Ereignisdatei
Der übliche Pfad liegt unter Dokumente > VoiceAttack > Orion Log-Wächter bzw. Aurora Log-Wächter. Die Dateien heißen Orion-Companion-Events.jsonl bzw. Aurora-Companion-Events.jsonl. Windows kann den Dokumente-Ordner an einen anderen Ort umleiten; prüfe daher den tatsächlichen Ordner deines Wächters.

Wähle die Datei des passenden Begleiters. Der Dateiname muss zur gewählten Variante passen. Eine noch nicht erzeugte Ereignisdatei führt zunächst zu Nicht verbunden; starte den Wächter und warte auf ein Ereignis. Das Deck erfindet keine Reaktionen aus dem Spielprotokoll.

### Game.log ist optional
Die Einrichtung sucht bei leerem Feld auf lokalen Festplatten in typischen LIVE-Installationsordnern. Beispiel: E:\Roberts Space Industries\StarCitizen\LIVE\Game.log. Der Laufwerksbuchstabe ist frei; D: ist keine Voraussetzung.

Bei einem Treffer prüfst du den Vorschlag. Bei mehreren Treffern wählst du die gewünschte Installation. Mit Game.log suchen wiederholst du die Suche; über Auswählen kannst du einen eigenen Pfad setzen. Es findet kein vollständiger Festplattenscan statt. Die automatische Suche berücksichtigt typische LIVE-Pfade; abweichende Installationen und Testkanäle wählst du manuell.

Ohne Game.log funktioniert die Anzeige der Begleiter-Reaktionen. Weitere Logs steht dann nicht zur Verfügung. Das Deck öffnet beide Logdateien ausschließlich lesend.

### Pfade später ändern
Schließe das Deck und öffne Begleiter-Deck einrichten im Startmenü. Dort kannst du Begleiter und Pfade neu speichern. Eine Neuinstallation überschreibt vorhandene Einstellungen nicht automatisch.

## 3 | Das Ereignisprotokoll bedienen
### Standard: nur Begleiter-Reaktionen
Nach dem Start zeigt das Deck die vom gewählten Begleiter gemeldeten Reaktionsereignisse. Diese Meldungen bestätigen ein Ereignis des Wächters, nicht die erfolgreiche Audiowiedergabe. Wenn du nichts hörst, prüfe die Audioausgabe im Begleiter selbst.

Weitere Logs blendet zusätzlich Zeilen aus Game.log ein. So bleiben die wichtigsten Reaktionen übersichtlich, während du das ausführlichere Spielprotokoll bei Bedarf hinzuschalten kannst. Rohe Spielmeldungen können englisch sein; das Deck verändert ihren Originaltext nicht.

### Kategorien und Textfilter
Alle zeigt die Einträge des gewählten Umfangs. Ereignisse, Schiff, Ort, Kampf und System grenzen die Darstellung weiter ein. Der Textfilter sucht innerhalb der Meldungen oder ihrer Quelle. Für eine vollständige Ansicht wähle Alle und leere das Filterfeld.

Auto-Scroll hält die Ansicht bei neuen Meldungen am Ende. Schalte es zum Lesen älterer Zeilen aus. Die Anzeige ist begrenzt und kein Archiv der gesamten Spielsitzung. Die ursprünglichen Dateien bleiben die vollständige Quelle.

### Ältere Meldungen verstehen
Älter kennzeichnet beim Start eingelesene frühere Meldungen. Sie sind keine neue Reaktion in diesem Moment. Eine erkannte Orts- oder Schiffsmeldung ist außerdem nicht automatisch ein verlässlich ermittelter aktueller Spielerzustand.

### Schaltflächen und Tastatur
Aktualisieren aktualisiert den beobachteten Zustand. Fenstermodus wechseln oder F11 wechselt die Fensterdarstellung. Begleiter freigeben gibt das externe Fenster zurück. Schließen oder Escape beendet die Deck-Sitzung regulär.

Das Begleiterfenster ist ein separates Windows-Fenster. Größe und Erscheinung des Roboters bestimmt der externe Begleiter. Eine Vergrößerung des Aurora-Roboters durch bloßes Vergrößern seines Windows-Fensters wird nicht unterstützt.

## 4 | Bildschirm und Designs
### Den XENEON EDGE verwenden
Windows muss den EDGE als aktiven Desktop-Monitor erkennen. Prüfe bei Anzeigeproblemen die Windows-Anzeigeeinstellungen und den Desktop-Modus in iCUE. Die Positionierung berücksichtigt die Windows-Skalierung; du musst keine Bildschirmkoordinaten von Hand eingeben.

F11 ist hilfreich, wenn du zwischen normalem Fenster und Vollbild wechseln möchtest. Bei Problemen mit dem externen Fenster nutze Begleiter freigeben oder beende das Deck regulär.

### Ein Design auswählen
Oben rechts unter Design wählst du den gewünschten Stil. Deck - Standard ist die neutrale Gestaltung. Zusätzlich gibt es 15 Interpretationen von Star-Citizen-Herstellerstilen:

RSI, Anvil Aerospace, Aegis Dynamics, Drake Interplanetary, Origin Jumpworks, Crusader Industries, MISC, Argo Astronautics, Banu, Consolidated Outland, Esperia / Tevarin, Kruger Intergalactic, Mirai, Vanduul und Aopoa.

Farben, Rahmen, Formen und Typografie verändern sich mit dem Design. Die Funktion des Protokolls und die Stimme des Begleiters bleiben gleich. Deine Designwahl wird lokal im WebView2-Profil gespeichert.

### Herkunft und Grenzen
Die Designs sind eigene Interpretationen. Sie enthalten keine übernommenen Herstellerlogos oder Schiffsbilder. Das Blackhole-Dynamics-Logo kennzeichnet den Ersteller des Decks; es kennzeichnet nicht den Hersteller von Orion oder Aurora.

Das Deck enthält keine Roboter- oder Sprach-Assets. Es verändert keine Begleiter-Binärdateien und bettet das fremde Fenster nicht in seine Anwendung ein. Beide Programme bleiben unabhängig.

## 5 | Probleme gezielt beheben
### Das Deck ist auf dem EDGE nicht sichtbar
Prüfe in iCUE den Modus Desktop und in Windows, ob der Monitor aktiv ist. Schließe die Deck-Sitzung regulär und starte sie danach erneut. Eine bereits laufende Sitzung verhindert einen zweiten Start.

### Der Roboter fehlt oder steht an der falschen Stelle
Starte den gewählten Begleiter separat und öffne dessen Begleiterfenster. Für Orion wird Orion Companion, für Aurora Aurora Orb erkannt. Mehrere passende Fenster verhindern eine eindeutige Zuordnung; schließe überzählige Fenster. Prüfe auch, ob in der Einrichtung die richtige Variante gewählt ist.

### Keine neuen Reaktionen erscheinen
Wähle Alle, leere den Textfilter und prüfe den Pfad zur Begleiter-Ereignisdatei. Beobachte, ob der Wächter tatsächlich neue Einträge in diese Datei schreibt. Ein laufendes Spiel alleine garantiert keine Begleiter-Reaktion. Weitere Logs benötigt eine vorhandene Game.log.

### Ereignisse erscheinen, aber es gibt keine Stimme
Das Deck meldet Ereignisse und spielt keine eigene Begleiter-Stimme ab. Prüfe Ton, Ausgabegerät und Einstellungen in Orion oder Aurora. Ein Eintrag ist kein Nachweis, dass ein Ton ausgegeben wurde.

### Nach einer Neuinstallation erscheint keine Einrichtung
Das ist bei vorhandener gültiger Konfiguration erwartetes Verhalten. Nutze Begleiter-Deck einrichten im Startmenü. Neuinstallieren ist zum Ändern der Pfade nicht notwendig.

### Die Deinstallation kann das Deck nicht schließen
Beende die Anwendung mit Escape und versuche es erneut. Bei einem Fehler bricht der Deinstaller vor Dateilöschungen ab. Fehlt die Programmdatei, installiere das Deck zuerst erneut in denselben Ordner und deinstalliere anschließend. Orion und Aurora werden nicht mit entfernt.

### Hilfe im Repository anfordern
Nenne die Deck-Version, Windows-Version, den gewählten Begleiter und die sichtbare Meldung. Teile keine privaten Logdateien oder persönlichen Pfade ungeprüft. Das Repository und der Begleiter-Hersteller sind auf Seite 7 verlinkt.

## 6 | Einstellungen, Updates und Kontakte
### Was lokal gespeichert wird
Pfade und Begleiter liegen in %LOCALAPPDATA%\StarCitizenCompanionDeck\config.json. Die Designwahl liegt im Profil %LOCALAPPDATA%\XeneonEdge\WebView2. Diagnosen werden unter %LOCALAPPDATA%\XeneonEdge\diagnostics abgelegt. %LOCALAPPDATA% kannst du in die Adresszeile des Windows-Explorers eingeben.

Bei der Deinstallation bleiben diese Benutzerdaten bewusst erhalten. Programmdateien und Verknüpfungen werden entfernt. Spielprotokolle und externe Begleiter bleiben unangetastet. Alte ausdrücklich verwendete eigene Konfigurationen liegen weiterhin an ihrem selbst gewählten Speicherort.

### Einrichtung vollständig zurücksetzen
Beende das Deck. Sichere config.json und benenne diese Datei beispielsweise in config.json.bak um. Beim nächsten normalen Start erscheint die Einrichtung erneut. Zum bloßen Ändern der Pfade verwende stattdessen den Startmenü-Eintrag. Das WebView2-Profil ist für einen Pfadwechsel nicht zu löschen.

### Eine neue Version installieren
Beende das Deck regulär und installiere die neue Ausgabe in denselben Installationsordner. Deine vorhandenen Pfade bleiben erhalten. Der Installer liefert das Handbuch mit; im Startmenü findest du Begleiter-Deck Benutzerhandbuch. Bei der portablen Ausgabe liegt Benutzerhandbuch.pdf neben der Programmdatei.

Installer und Anwendung sind für diese erste Ausgabe nicht digital signiert. Die veröffentlichten Prüfsummen dienen zum Vergleich der heruntergeladenen Dateien. Beziehe die Dateien ausschließlich über das Projekt-Repository.

### Projekt und Hersteller
Repository: https://github.com/blackhole-dynamics/Star-Citizen-Companion-Deck
Orion und Aurora: https://www.aurora-systems.online/

Star Citizen und die genannten Hersteller gehören zu ihren jeweiligen Rechteinhabern. Dieses Community-Projekt ist nicht offiziell mit Cloud Imperium Games, Roberts Space Industries, Corsair oder Aurora Systems verbunden.

Erstellt von Blackhole Dynamics / KayKaspers. Handbuch für Version 0.1.0.
