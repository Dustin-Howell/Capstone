#define MyAppName "Fire and Ice"
#define MyAppVersion "0.0.2"
#define MyAppPublisher "RRRobust Software"
#define MyAppExeName "FireAndIce.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{DB59FCC4-0267-45C1-B96D-1C968E2B5A3F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=..\Docs\License.txt
InfoBeforeFile=..\README.md
InfoAfterFile=..\README.md
OutputDir=..\Deployable
OutputBaseFilename=Setup-FireAndIce-version_0.0.2a
Compression=lzma
SolidCompression=yes
ShowLanguageDialog=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Game executable
Source: "..\Fire and Ice\FireAndIce\bin\Release\FireAndIce.exe"; DestDir: "{app}"; Flags: ignoreversion

; Game Assets
Source: "..\Fire and Ice\FireAndIce\bin\Release\Content\*"; DestDir: "{app}\Content\"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\Fire and Ice\FireAndIce\bin\Release\Videos\*"; DestDir: "{app}\Videos\"; Flags: ignoreversion recursesubdirs createallsubdirs

; Game libraries
Source: "..\Fire and Ice\FireAndIce\bin\Release\*.dll"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

