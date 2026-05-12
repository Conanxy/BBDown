#define AppName "BBDown GUI"
#ifndef AppVersion
#define AppVersion "0.0.0"
#endif
#ifndef SourceDir
#define SourceDir "..\..\publish\win-x64"
#endif
#ifndef OutputBaseFilename
#define OutputBaseFilename "BBDown.GUI_setup"
#endif

[Setup]
AppId={{BC85F530-BC7C-480D-8B9E-2D1ED6F10032}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=BBDown
AppPublisherURL=https://github.com/Conanxy/BBDown
AppSupportURL=https://github.com/Conanxy/BBDown/issues
DefaultDirName={autopf}\BBDown GUI
DefaultGroupName=BBDown GUI
DisableProgramGroupPage=yes
OutputDir=..\..\installer
OutputBaseFilename={#OutputBaseFilename}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\BBDown GUI"; Filename: "{app}\BBDown.GUI.exe"
Name: "{autodesktop}\BBDown GUI"; Filename: "{app}\BBDown.GUI.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\BBDown.GUI.exe"; Description: "{cm:LaunchProgram,BBDown GUI}"; Flags: nowait postinstall skipifsilent
