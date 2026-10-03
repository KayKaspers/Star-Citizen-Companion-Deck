#ifndef ReleaseVersion
  #error ReleaseVersion fehlt
#endif
#ifndef InstallerAppId
  #define InstallerAppId "{{19EE752A-F8CA-4445-95E8-28612D0415C2}"
#endif
[Setup]
AppId={#InstallerAppId}
AppMutex=Local\StarCitizenCompanionDeck.Running
AppName=Star Citizen Begleiter-Deck
AppVersion={#ReleaseVersion}
AppPublisher=Blackhole Dynamics
AppPublisherURL=https://github.com/blackhole-dynamics/Star-Citizen-Companion-Deck
DefaultDirName={localappdata}\Programs\StarCitizenCompanionDeck
DefaultGroupName=Star Citizen Begleiter-Deck
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible and not arm64
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17763
OutputDir={#OutputDir}
OutputBaseFilename=Begleiter-Deck-{#ReleaseVersion}-win-x64-Setup
SetupIconFile=app.ico
UninstallDisplayIcon={app}\deck-icon-{#ReleaseVersion}.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
InfoBeforeFile=INSTALLATIONSHINWEISE.txt
[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
[Tasks]
Name: "desktopicon"; Description: "Desktop-Verknüpfung erstellen"; Flags: unchecked
[Files]
Source: "{#PayloadDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#BootstrapFile}"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: NeedsWebView
Source: "{#PayloadDir}\app.ico"; DestDir: "{app}"; DestName: "deck-icon-{#ReleaseVersion}.ico"; Flags: ignoreversion
[Icons]
Name: "{group}\Begleiter-Deck Benutzerhandbuch"; Filename: "{app}\Benutzerhandbuch.pdf"
Name: "{group}\Begleiter-Deck"; Filename: "{app}\Xeneon.Runtime.Host.exe"; IconFilename: "{app}\deck-icon-{#ReleaseVersion}.ico"
Name: "{group}\Begleiter-Deck einrichten"; Filename: "{app}\Xeneon.Runtime.Host.exe"; Parameters: "--configure"; IconFilename: "{app}\deck-icon-{#ReleaseVersion}.ico"
Name: "{autodesktop}\Begleiter-Deck"; Filename: "{app}\Xeneon.Runtime.Host.exe"; IconFilename: "{app}\deck-icon-{#ReleaseVersion}.ico"; Tasks: desktopicon
[Code]
function InitializeUninstall: Boolean;
var ExitCode: Integer;
begin
  Result := False;
  if not FileExists(ExpandConstant('{app}\Xeneon.Runtime.Host.exe')) then begin
    SuppressibleMsgBox('Die Programmdatei fehlt. Bitte das Deck erneut installieren und danach deinstallieren.', mbError, MB_OK, IDOK);
    Exit;
  end;
  if not Exec(ExpandConstant('{app}\Xeneon.Runtime.Host.exe'), '--shutdown', '', SW_HIDE, ewWaitUntilTerminated, ExitCode) then begin
    SuppressibleMsgBox('Das Deck konnte nicht geschlossen werden. Bitte zuerst mit Escape beenden und die Deinstallation erneut starten.', mbError, MB_OK, IDOK);
    Exit;
  end;
  if ExitCode <> 0 then begin
    SuppressibleMsgBox('Das Deck läuft noch oder kann nicht sicher geprüft werden. Die Deinstallation wurde abgebrochen; es wurden noch keine Dateien entfernt.', mbError, MB_OK, IDOK);
    Exit;
  end;
  Result := True;
end;
function NeedsWebView: Boolean;
var Version: String;
begin
  Result := not ((RegQueryStringValue(HKLM32, 'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', Version) and (Version <> '') and (Version <> '0.0.0.0')) or
                 (RegQueryStringValue(HKCU, 'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', Version) and (Version <> '') and (Version <> '0.0.0.0')));
end;
procedure CurStepChanged(CurStep: TSetupStep);
var ExitCode: Integer;
begin
  if (CurStep = ssPostInstall) and NeedsWebView then begin
    if not Exec(ExpandConstant('{tmp}\MicrosoftEdgeWebview2Setup.exe'), '/silent /install', '', SW_HIDE, ewWaitUntilTerminated, ExitCode) then
      MsgBox('WebView2 konnte nicht installiert werden. Bitte Microsoft Edge WebView2 installieren, bevor du das Deck startest.', mbError, MB_OK)
    else if ExitCode <> 0 then
      MsgBox('WebView2-Installation fehlgeschlagen. Das Deck ist installiert; bitte WebView2 separat installieren.', mbError, MB_OK);
  end;
end;
