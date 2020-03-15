; Script generated by the Inno Script Studio Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{CBCC3752-C35B-4D15-A31F-D5B687C2ED95}

#define ApplicationName "Milk Bottle"
#define ApplicationVersion GetFileVersion("..\bin\x64\Release\MilkBottle.exe")
#define ApplicationGroup "Secret Squirrel Software"
#define Platform "x64"

AppVersion={#ApplicationVersion}
AppVerName={#ApplicationName} {#ApplicationVersion}
OutputBaseFilename=Setup_{#ApplicationName}_v{#ApplicationVersion}_{#Platform}
VersionInfoVersion={#ApplicationVersion}

AppName={#ApplicationName}
AppPublisher={#ApplicationGroup}
DefaultDirName={commonpf}\{#ApplicationGroup}\{#ApplicationName}
UsePreviousAppDir=yes
DefaultGroupName={#ApplicationName}
AllowNoIcons=yes
LicenseFile=..\..\Installation\Resources\license.rtf
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=X64
SetupIconFile=..\Resources\Milk Bottle.ico
UninstallDisplayIcon={app}\MilkBottle.exe

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Files]
Source: "..\bin\{#Platform}\Release\MilkBottle.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\Caliburn.Micro.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\Caliburn.Micro.Platform.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\Caliburn.Micro.Platform.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\CommonServiceLocator.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\ControlzEx.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\CuttingEdge.Conditions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\LanguageExt.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\LiteDB.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\MahApps.Metro.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\Microsoft.Practices.Prism.Composition.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\Microsoft.Practices.Prism.Mvvm.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\Microsoft.Practices.Prism.PubSubEvents.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\Microsoft.Practices.Prism.SharedInterfaces.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\Microsoft.Practices.Unity.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\MilkBottle.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\MoreLinq.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\NAudio.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\Noise.UI.Style.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\OpenTK.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\OpenTK.dll.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\OpenTK.GLControl.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\Prism.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\Prism.Unity.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\Prism.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\protobuf-net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\ReusableBits.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\ReusableBits.Mvvm.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\ReusableBits.Ui.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\Serilog.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\Serilog.Enrichers.Process.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\Serilog.Sinks.Console.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\Serilog.Sinks.File.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\Serilog.Sinks.RollingFile.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\System.Buffers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\System.Memory.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\System.Memory.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\System.Numerics.Vectors.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\System.Reactive.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\System.Reactive.Interfaces.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\System.Reactive.Linq.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\System.Reactive.PlatformServices.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\System.Threading.Tasks.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\System.ValueTuple.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\System.Windows.Interactivity.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\TinyIpc.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\Unity.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\{#Platform}\Release\Unity.Container.dll"; DestDir: "{app}"; Flags: ignoreversion

Source: "..\..\lib\ProjectMSharp\v0.1.0.0\{#Platform}\ProjectMSharp.dll"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
; VC++ redistributable runtime. Extracted by VC2017RedistNeedsInstall(), if needed.
Source: "..\..\Installation\Prerequisites\VC Runtime\VC_redist.{#Platform}.exe"; DestDir: {tmp}; Flags: dontcopy

[Icons]
Name: "{group}\Milk Bottle"; Filename: "{app}\MilkBottle.exe"
Name: "{group}\{cm:UninstallProgram,Milk Bottle}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\Milk Bottle"; Filename: "{app}\MilkBottle.exe"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\Milk Bottle"; Filename: "{app}\MilkBottle.exe"; Tasks: quicklaunchicon

[Run]
Filename: "{tmp}\VC_redist.{#Platform}.exe"; StatusMsg: "{cm:InstallingVC2019redist}"; Parameters: "/quiet"; Check: VC2019RedistNeedsInstall ; Flags: waituntilterminated
Filename: "{app}\MilkBottle.exe"; Description: "{cm:LaunchProgram,Milk Bottle}"; Flags: nowait postinstall skipifsilent

[CustomMessages]
InstallingVC2019redist=Installing Visual C++ runtime

[Code]
function VC2019RedistNeedsInstall: Boolean;
var 
  Version: String;
begin
  if (RegQueryStringValue(HKEY_LOCAL_MACHINE, 'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64', 'Version', Version)) then
  begin
    // Is the installed version at least 14.24 ? 
    Log('VC Redist Version check : found ' + Version);
    Result := (CompareStr(Version, 'v14.24.28127')<0);
  end
  else 
  begin
    // Not even an old version installed
    Result := True;
  end;
  if (Result) then
  begin
    ExtractTemporaryFile('VC_redist.x64.exe');
  end;
end;

function IsVCRedist32BitNeeded(): boolean;
var
  strVersion: string;
begin
  if (RegQueryStringValue(HKEY_LOCAL_MACHINE,
    'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x86', 'Version', strVersion)) then
  begin
    // Is the installed version at least 14.24 ? 
    Log('VC Redist x86 Version : found ' + strVersion);
    Result := (CompareStr(strVersion, 'v14.24.28127') < 0);
  end
  else
  begin
    if (RegQueryStringValue(HKEY_LOCAL_MACHINE,
      'SOFTWARE\WOW6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\x86', 'Version', strVersion)) then
    begin
      // Is the installed version at least 14.24 ? 
      Log('VC Redist x86 Version : found ' + strVersion);
      Result := (CompareStr(strVersion, 'v14.24.28127') < 0);
    end
    else
    begin
      // Not even an old version installed
      Log('VC Redist x86 is not already installed');
      Result := True;
    end;
  end;
end;

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
  Result := not IsAppRunning('milkbottle.exe');
  if not Result then
  MsgBox('Milk Bottle is running. Please close the application before running the installer ', mbError, MB_OK);
end;