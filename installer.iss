; ==============================================================================
; Power Grid HaYTooL - Inno Setup Kurulum Yapılandırması
; ==============================================================================

#ifndef MyAppVersion
  #define MyAppVersion "8.7"
#endif

#ifndef MyOutputDir
  #define MyOutputDir "..\Publish"
#endif

#ifndef MySourceDir
  #define MySourceDir "..\Publish"
#endif

#define MyAppName "Power Grid HaYTooL"
#define MyAppPublisher "HaYTooL"
#define MyAppURL "https://haytokoraz.github.io/Power-Grid/"
#define MyAppExeName "PowerGridHaYTooL.exe"

[Setup]
AppId={{D3F974B8-43C1-4B9F-84E3-8B35C834906F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} v{#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL=https://github.com/HaYToKoRaZ/Power-Grid
AppUpdatesURL=https://github.com/HaYToKoRaZ/Power-Grid/releases
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir={#MyOutputDir}
OutputBaseFilename=PowerGridHaYTooL_v{#MyAppVersion}_Setup
SetupIconFile=app.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequiredOverridesAllowed=dialog commandline
DisableDirPage=no
DisableProgramGroupPage=no
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}
VersionInfoVersion={#MyAppVersion}.0.0
VersionInfoCompany=HaYTooL
VersionInfoDescription=Power Grid HaYTooL Setup
VersionInfoTextVersion={#MyAppVersion}
VersionInfoCopyright=Copyright © HaYTooL. All rights reserved.
VersionInfoProductName=Power Grid HaYTooL
VersionInfoProductVersion={#MyAppVersion}.0.0

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "{#MySourceDir}\PowerGridHaYTooL.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MySourceDir}\PowerGridHaYTooL.ini"; DestDir: "{app}"; Flags: ignoreversion onlyifdoesntexist
Source: "app.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "logo.png"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent
