; Script generated by the Inno Script Studio Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define ApplicationName "Album4Matter"
#define ApplicationVersion "v" + GetFileVersion("..\..\Album4Matter\bin\x64\Release\Album4Matter.exe")
#define GroupName "Secret Squirrel Software"
#define ExecutableName "Album4Matter.exe"
#define Platform "x64"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{0F66D2C8-D544-49FA-8FFF-34F87FF9645F}
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
SetupIconFile=..\..\Album4Matter\Resources\FlatStanley.ico
UninstallDisplayIcon={app}\Album4Matter.exe

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Album4Matter.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Album4Matter.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Caliburn.Micro.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Caliburn.Micro.Platform.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Caliburn.Micro.Platform.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\CommonServiceLocator.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\ControlzEx.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\CuttingEdge.Conditions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\MahApps.Metro.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Microsoft.Practices.Prism.Composition.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Microsoft.Practices.Prism.Mvvm.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Microsoft.Practices.Prism.PubSubEvents.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Microsoft.Practices.Prism.SharedInterfaces.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Microsoft.Practices.ServiceLocation.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Microsoft.Practices.Unity.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Noise.UI.Style.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Prism.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Prism.Unity.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Prism.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\protobuf-net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\ReusableBits.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\ReusableBits.Mvvm.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\ReusableBits.Ui.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Serilog.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Serilog.Enrichers.Process.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Serilog.Sinks.Console.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Serilog.Sinks.File.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Serilog.Sinks.RollingFile.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\System.Reactive.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\System.Reactive.Interfaces.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\System.Reactive.Linq.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\System.Threading.Tasks.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\System.ValueTuple.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\System.Windows.Interactivity.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\taglib-sharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\TinyIpc.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Unity.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Album4Matter\bin\{#Platform}\Release\Unity.Container.dll"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#ApplicationName}"; Filename: "{app}\{#ExecutableName}"
Name: "{commondesktop}\{#ApplicationName}"; Filename: "{app}\{#ExecutableName}"; Tasks: desktopicon

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
  Result := not IsAppRunning('Album4Matter.exe');
  if not Result then
  MsgBox('Flat Stanley is running. Please close the application before running the installer ', mbError, MB_OK);
end;