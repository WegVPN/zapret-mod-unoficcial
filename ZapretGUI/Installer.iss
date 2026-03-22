; Inno Setup Script for ZapretGUI
; Requires Inno Setup 6.0 or later

#define MyAppName "ZapretGUI"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "ZapretGUI Team"
#define MyAppExeName "ZapretGUI.exe"
#define MyAppURL "https://github.com/zapret-mod-unoficcial"

[Setup]
; Basic setup settings
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=..\LICENSE
OutputDir=Output
OutputBaseFilename=ZapretGUI-Setup-{#MyAppVersion}
SetupIconFile=Resources\zapret.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

; Require admin privileges
PrivilegesRequiredOverridesAllowed=dialog

; Language detection
UsePreviousAppDir=yes
UsePreviousGroup=yes
UsePreviousTasks=yes

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode
Name: "autostart"; Description: "Запускать при старте Windows"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "installzapret"; Description: "Установить zapret (включено по умолчанию)"; GroupDescription: "Дополнительные компоненты"; Flags: checkedonce

[Files]
; Main application files
Source: "bin\Release\net8.0-windows\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net8.0-windows\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net8.0-windows\*.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net8.0-windows\*.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion

; Include zapret if available
Source: "zapret\*"; DestDir: "{app}\zapret"; Flags: ignoreversion recursesubdirs; Tasks: installzapret

; Documentation
Source: "..\README.md"; DestDir: "{app}"; DestName: "README.txt"; Flags: isreadme

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
var
  ZapretDownloadPage: TDownloadWizardPage;
  InstallZapret: Boolean;

procedure InitializeWizard;
begin
  InstallZapret := False;
  
  // Create download page for zapret
  ZapretDownloadPage := CreateDownloadPage(SetupMessage(msgWizardPreparing), SetupMessage(msgPreparingDesc), nil);
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = wpSelectTasks then
  begin
    InstallZapret := WizardIsTaskSelected('installzapret');
  end;
end;

function NeedDownloadFiles: Boolean;
begin
  Result := InstallZapret;
end;

procedure PrepareToInstall(var NeedsRestart: Boolean; var RestartReasons: String);
var
  ResultCode: Integer;
begin
  // Check if .NET 8 Desktop Runtime is installed
  if not RegKeyExists(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v8.0') then
  begin
    if MsgBox(
      'Для работы приложения требуется .NET 8 Desktop Runtime.' + #13#10 +
      'Хотите загрузить и установить его сейчас?',
      mbConfirmation, MB_YESNO) = idYes then
    begin
      ShellExec('open', 
        'https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-desktop-8.0.0-windows-x64-installer',
        '', '', SW_SHOWNORMAL, ewNoWait, ResultCode);
    end;
  end;
end;

function InitializeSetup(): Boolean;
var
  ResultCode: Integer;
begin
  Result := True;
  
  // Check minimum Windows version (Windows 10)
  if (GetWindowsVersion < $0A000000) then
  begin
    MsgBox('Это приложение требует Windows 10 или новее.', mbError, MB_OK);
    Result := False;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Create application data directory
    CreateDir(ExpandConstant('{userappdata}\ZapretGUI'));
    CreateDir(ExpandConstant('{userappdata}\ZapretGUI\Profiles'));
    CreateDir(ExpandConstant('{userappdata}\ZapretGUI\Logs'));
    CreateDir(ExpandConstant('{userappdata}\ZapretGUI\Backups'));
  end;
end;

function IsDotNetInstalled: Boolean;
begin
  Result := RegKeyExists(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v8.0');
end;

[UninstallDelete]
Type: filesandordirs; Name: "{userappdata}\ZapretGUI"

[UninstallRun]
; Stop any running zapret processes
Filename: "taskkill"; Parameters: "/F /IM zapret.exe"; Flags: runhidden; StatusMsg: "Остановка zapret..."
Filename: "taskkill"; Parameters: "/F /IM {#MyAppExeName}"; Flags: runhidden; StatusMsg: "Остановка приложения..."

[Registry]
; Register file extension for config files (optional)
Root: HKCR; Subkey: ".zapretcfg"; ValueType: string; ValueData: "ZapretGUI.Config"; Flags: uninsdeletevalue
Root: HKCR; Subkey: "ZapretGUI.Config"; ValueType: string; ValueData: "ZapretGUI Configuration"; Flags: uninsdeletekey

[Messages]
WelcomeLabel1=Добро пожаловать в мастер установки [name/ver]
WelcomeLabel2=Этот мастер установит [name/ver] на ваш компьютер.%n%nРекомендуется закрыть все остальные приложения перед продолжением.
SetupAppRunningError=Приложение [name] уже запущено.%n%nПожалуйста, закройте его и нажмите OK для продолжения, или Отмена для выхода.

[CustomMessages]
DownloadZapret=Загрузка zapret...
ZapretDownloaded=Zapret успешно загружен
DotNetRequired=Требуется .NET 8 Desktop Runtime
