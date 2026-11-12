; Inno Setup 6 - installateur Windows x64 pour GestionMagasin
#define AppName "GestionMagasin"
#define AppVersion "1.0.0"
#define AppPublisher "GestionMagasin contributors"
#define AppExeName "GestionMagasin.WinForms.exe"
#define PublishDir "..\livraison\GestionMagasin-win-x64"

[Setup]
AppId={{C7CB2F1D-0AE7-4B62-A62A-5C7C6F9D5A01}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\GestionMagasin
DefaultGroupName=GestionMagasin
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64
OutputDir=..\livraison
OutputBaseFilename=GestionMagasin-Setup-{#AppVersion}-win-x64
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
LicenseFile=..\LICENSE
Uninstallable=yes

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\GestionMagasin"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\GestionMagasin"; Filename: "{app}\{#AppExeName}"

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Lancer GestionMagasin"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Les données utilisateur dans %LOCALAPPDATA% ne sont volontairement pas supprimées.
Type: filesandordirs; Name: "{app}"
