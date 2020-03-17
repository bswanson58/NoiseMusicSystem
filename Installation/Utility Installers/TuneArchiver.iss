; Script generated by the Inno Script Studio Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define ApplicationName "Tune Archiver"
#define ApplicationVersion "v" + GetFileVersion("..\..\Album4Matter\bin\x64\Release\Album4Matter.exe")
#define GroupName "Secret Squirrel Software"
#define ExecutableName "TuneArchiver.exe"
#define Platform "x64"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{47C33175-276D-457E-AA26-5D368AAF74AF}
AppName={#ApplicationName}
AppVersion={#ApplicationVersion}
AppVerName={#ApplicationName} {#ApplicationVersion}
AppPublisher={#GroupName}
DefaultDirName={commonpf}\{#GroupName}\{#ApplicationName}
DefaultGroupName={#ApplicationName}
AllowNoIcons=yes
LicenseFile=D:\Development\Noise\Installation\Resources\license.rtf
OutputBaseFilename=Setup_{#ApplicationName}_{#ApplicationVersion}_{#Platform}
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=X64
SetupIconFile=..\..\TuneArchiver\Resources\Archiver.ico
UninstallDisplayIcon={app}\{#ExecutableName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Files]
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\TuneArchiver.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Caliburn.Micro.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Caliburn.Micro.Platform.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Caliburn.Micro.Platform.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\CommonServiceLocator.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\ControlzEx.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\CuttingEdge.Conditions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\MahApps.Metro.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Microsoft.Practices.Prism.Composition.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Microsoft.Practices.Prism.Mvvm.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Microsoft.Practices.Prism.PubSubEvents.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Microsoft.Practices.Prism.SharedInterfaces.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Microsoft.Practices.Unity.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Noise.UI.Style.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Prism.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Prism.Unity.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Prism.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\protobuf-net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\recls.NET.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\ReusableBits.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\ReusableBits.Mvvm.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\ReusableBits.Ui.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Serilog.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Serilog.Enrichers.Process.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Serilog.Sinks.Console.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Serilog.Sinks.File.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Serilog.Sinks.RollingFile.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\System.Windows.Interactivity.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\TinyIpc.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\TuneArchiver.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Unity.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Unity.Configuration.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Unity.Container.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Unity.Interception.Configuration.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Unity.Interception.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Unity.RegistrationByConvention.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\TuneArchiver\bin\{#Platform}\Release\Unity.ServiceLocation.dll"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#ApplicationName}"; Filename: "{app}\{#ExecutableName}"
Name: "{commondesktop}\{#ApplicationName}"; Filename: "{app}\{#ExecutableName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#ApplicationName}"; Filename: "{app}\{#ExecutableName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#ExecutableName}"; Description: "{cm:LaunchProgram,{#StringChange(ApplicationName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
function IsAppRunning(const FileName : string): Boolean;
var
    FSWbemLocator: Variant;
    FWMIService   : Variant;
    FWbemObjectSet: Variant;
begin
    Result := false;
    FSWbemLocator := CreateOleObject('WBEMScripting.SWBEMLocator');
    FWMIService := FSWbemLocator.ConnectServer('', 'root\CIMV2', '', '');
    FWbemObjectSet :=
      FWMIService.ExecQuery(
        Format('SELECT Name FROM Win32_Process Where Name="%s"', [FileName]));
    Result := (FWbemObjectSet.Count > 0);
    FWbemObjectSet := Unassigned;
    FWMIService := Unassigned;
    FSWbemLocator := Unassigned;
end;

function InitializeSetup: boolean;
begin
  Result := not IsAppRunning('TuneArchiver.exe');
  if not Result then
  MsgBox('Tune Archiver is running. Please close the application before running the installer ', mbError, MB_OK);
end;