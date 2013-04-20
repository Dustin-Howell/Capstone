#define MyAppName "Fire and Ice"
#define MyAppVersion "0.0.2a"
#define MyAppPublisher "RRRobust Software"
#define MyAppExeName "FireAndIce.exe"

; Redistributable constants
#define MyRedistFolder "..\Redist"
#define XNARedist "xnafx40_redist.msi"
#define DotNETRedist "dotNetFx40_Full_x86_x64.exe"

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
OutputBaseFilename=Setup-FireAndIce-version_{#MyAppVersion}
Compression=lzma
SolidCompression=yes
ShowLanguageDialog=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Redistributables
Source: "{#MyRedistFolder}\{#XNARedist}"; DestDir: "{tmp}"
Source: "{#MyRedistFolder}\{#DotNETRedist}"; DestDir: "{tmp}"

; Game executable
Source: "..\Fire and Ice\FireAndIce\bin\Release\FireAndIce.exe"; DestDir: "{app}"; Flags: ignoreversion

; Game Assets
Source: "..\Fire and Ice\FireAndIce\bin\Release\Content\*"; DestDir: "{app}\Content\"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\Fire and Ice\FireAndIce\bin\Release\Videos\*"; DestDir: "{app}\Videos\"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\Fire and Ice\FireAndIce\bin\Release\SoundAssets\*"; DestDir: "{app}\SoundAssets\"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\Fire and Ice\FireAndIce\bin\Release\Music\*"; DestDir: "{app}\Music\"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\Fire and Ice\FireAndIce\bin\Release\Images\*"; DestDir: "{app}\Images\"; Flags: ignoreversion recursesubdirs createallsubdirs

; Game libraries
Source: "..\Fire and Ice\FireAndIce\bin\Release\*.dll"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Redistributable installers
Filename: "{tmp}\{#DotNETRedist}"; StatusMsg: "Installing required component: .NET Framework 4.0 Client."; Parameters: "/norestart /passive"; Flags: skipifdoesntexist; Check: CheckNetFramework
Filename: "msiexec.exe"; Parameters: "/qb /i ""{tmp}\{#XNARedist}"""; StatusMsg: "Installing required component: XNA Framework Redistributable 4.0 Refresh."; Flags: skipifdoesntexist; Check: CheckXNAFramework

; Fire and Ice
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
function IsDotNetDetected: boolean;
var
    Key: string;
    Install: cardinal;
    Success: boolean;

begin
    WizardForm.StatusLabel.Caption := 'Checking for .Net Framework 4.0 Client.';
    Key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client';
    Success := RegQueryDWordValue(HKLM, Key, 'Install', Install);
    result := Success and (Install = 1);
end;

function CheckNetFramework: boolean;
begin
    if IsDotNetDetected then begin
        WizardForm.StatusLabel.Caption := '.Net Framework 4.0 Client detected.';
    end;
    result := not IsDotNetDetected;
end;

function IsXNAFrameworkDetected: boolean;
var
    Key: string;
    Install: cardinal;
    Success: boolean;

begin
    WizardForm.StatusLabel.Caption := 'Checking for XNA Framework Redistributable 4.0 Refresh.';
    if IsWin64 then begin
        Key := 'SOFTWARE\Wow6432Node\Microsoft\XNA\Framework\v4.0';
    end else begin
        Key := 'SOFTWARE\Microsoft\XNA\Framework\v4.0';
    end;
    Success := RegQueryDWordValue(HKLM, Key, 'Installed', Install);
    result := Success and (Install = 1);
end;

function CheckXNAFramework: boolean;
begin
    if IsXNAFrameworkDetected then begin
        WizardForm.StatusLabel.Caption := 'XNA Framework Redistributable 4.0 Refresh detected.';
    end;
    result := not IsXNAFrameworkDetected;
end;

