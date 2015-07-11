#define AppVer GetFileVersion('..\Build\Release\EarTrumpet.exe')

[Setup]
AppName=Ear Trumpet
AppVersion={#AppVer}
VersionInfoVersion={#AppVer}
DefaultDirName={pf}\Ear Trumpet
DefaultGroupName=Ear Trumpet
SetupIconFile=..\..\EarTrumpet\Application.ico
UninstallDisplayIcon={app}\EarTrumpet.exe
UninstallDisplayName=Ear Trumpet
Compression=lzma2
SolidCompression=yes
OutputDir=..\Setup
OutputBaseFilename=Ear Trumpet Setup
SourceDir=..\Build\Release
DisableProgramGroupPage=yes
AllowNetworkDrive=no
AllowUNCPath=no
DisableReadyPage=yes
DisableStartupPrompt=yes
DisableWelcomePage=yes

[Files]
Source: "EarTrumpet.exe"; DestDir: "{app}"; Flags: replacesameversion
Source: "EarTrumpet.exe.config"; DestDir: "{app}"; Flags: replacesameversion
Source: "EarTrumpet.Interop.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\Redists\vcredist_x86.exe"; DestDir: "{tmp}";

[Icons]
Name: "{commonstartmenu}\Ear Trumpet"; Filename: "{app}\EarTrumpet.exe"
Name: "{commonstartup}\Ear Trumpet"; Filename: "{app}\EarTrumpet.exe"

[Run]
Filename: "{tmp}\vcredist_x86.exe"; Parameters: "/install /passive /norestart"; StatusMsg: Installing VC++ 2013 runtime...
Filename: "{app}\EarTrumpet.exe"; Description: "Run Ear Trumpet"; Flags: "postinstall nowait runasoriginaluser"

