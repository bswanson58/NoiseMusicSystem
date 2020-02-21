; Script generated by the Inno Script Studio Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{CBCC3752-C35B-4D15-A31F-D5B687C2ED95}
AppName=Milk Bottle
AppVersion=0.1
;AppVerName=Milk Bottle 0.1
AppPublisher=Secret Squirrel Software
DefaultDirName={pf}\Milk Bottle
DefaultGroupName=Milk Bottle
AllowNoIcons=yes
LicenseFile=..\..\Installation\Resources\license.rtf
OutputBaseFilename=MilkBottleInstall_x64
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=X64
SetupIconFile=..\Resources\Milk Bottle.ico

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Files]
Source: "..\bin\x64\Release\MilkBottle.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\Caliburn.Micro.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\Caliburn.Micro.Platform.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\Caliburn.Micro.Platform.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\CommonServiceLocator.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\ControlzEx.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\CuttingEdge.Conditions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\MahApps.Metro.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\Microsoft.Practices.Prism.Composition.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\Microsoft.Practices.Prism.Mvvm.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\Microsoft.Practices.Prism.PubSubEvents.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\Microsoft.Practices.Prism.SharedInterfaces.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\Microsoft.Practices.Unity.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\MilkBottle.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\NAudio.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\Noise.UI.Style.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\OpenTK.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\OpenTK.dll.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\OpenTK.GLControl.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\Prism.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\Prism.Unity.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\Prism.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\ProjectMSharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\protobuf-net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\ReusableBits.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\ReusableBits.Mvvm.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\ReusableBits.Ui.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\Serilog.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\Serilog.Enrichers.Process.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\Serilog.Sinks.Console.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\Serilog.Sinks.File.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\Serilog.Sinks.RollingFile.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\System.Reactive.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\System.Reactive.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\System.Reactive.Interfaces.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\System.Reactive.Linq.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\System.Reactive.PlatformServices.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\System.Threading.Tasks.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\System.ValueTuple.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\System.Windows.Interactivity.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\TinyIpc.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\Unity.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\x64\Release\Unity.Container.dll"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\Milk Bottle"; Filename: "{app}\MilkBottle.exe"
Name: "{group}\{cm:UninstallProgram,Milk Bottle}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\Milk Bottle"; Filename: "{app}\MilkBottle.exe"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\Milk Bottle"; Filename: "{app}\MilkBottle.exe"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\MilkBottle.exe"; Description: "{cm:LaunchProgram,Milk Bottle}"; Flags: nowait postinstall skipifsilent
