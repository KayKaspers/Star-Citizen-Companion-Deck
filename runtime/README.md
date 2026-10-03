# Laufzeit des Begleiter-Decks

Die Laufzeit besteht aus Core, Windows-Integration und WinForms/WebView2-Host.
Die lokale Web-Oberfläche liegt in web-ui. Es wird kein Webserver benötigt.

Einstieg und Startbefehle: [deutsches Projekt-README](../README.md).
Weitere Informationen:

- [Architektur](../docs/ARCHITEKTUR.md)
- [Konfiguration und Varianten](../docs/KONFIGURATION.md)
- [Herstellerdesigns und Quellen](../docs/THEMEN-RECHERCHE.md)
- [Prüfung](../evidence/PRUEFUNG-2026-10-03.md)

Start-Runtime.ps1 baut und startet den Host mit gesperrten Paketen.
Test-Runtime.ps1 prüft Core, Dateilesen und Windows-Integration;
-IncludeWebChecks ergänzt die Browserprüfungen.
Mit -DotnetPath kann eine außerhalb des Systems installierte .NET-SDK-Version
angegeben werden. Eigene Konfigurationen außerhalb des Repositorys speichern.

Die Beispielkonfigurationen positionieren bei -PlaceExternal ausschließlich
das exakt gewählte separate Begleiterfenster. Escape gibt es vor dem Schließen
zurück. Die Aurora-Beispielwerte sind vor dem ersten Einsatz live zu prüfen.
Orion und Aurora müssen separat gestartet werden. Das Deck startet sie nicht.
