#define ProductName "Funtino Control Center"
#define PublisherName "Funtino"
#define FileVersion ProductVersion + ".0"

[Setup]
AppId={{AE60164E-556D-4A82-BC01-3ED1874B8B65}
AppName={#ProductName}
AppVersion={#ProductVersion}
AppVerName={#ProductName} {#TargetLabel} {#ProductVersion}
AppPublisher={#PublisherName}
DefaultDirName={autopf}\Funtino\ControlCenter
DefaultGroupName=Funtino Control Center
DisableProgramGroupPage=yes
PrivilegesRequired=admin
ArchitecturesAllowed={#TargetArchitectures}
OutputBaseFilename=FuntinoControlCenter-Setup-{#TargetLabel}-{#ProductVersion}
SetupLogging=yes
Compression=lzma2/normal
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\FuntinoControlCenter.exe
VersionInfoVersion={#FileVersion}
VersionInfoProductVersion={#ProductVersion}
VersionInfoCompany={#PublisherName}
VersionInfoDescription=Funtino Control Center installer
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "..\bin\Release\*"; DestDir: "{app}"; Excludes: "*.pdb"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "Install-Prerequisites.ps1"; DestDir: "{tmp}"; DestName: "Install-Funtino-Prerequisites.ps1"; Flags: dontcopy
Source: "..\LocalOnly\WorkspaceRoot\Reference\Databases\SharjReportDB0216.mdf"; DestDir: "{tmp}"; DestName: "FuntinoParkOperations-seed.mdf"; Flags: dontcopy
Source: "..\LocalOnly\WorkspaceRoot\Reference\Databases\SharjReportDB0216_log.ldf"; DestDir: "{tmp}"; DestName: "FuntinoParkOperations-seed_log.ldf"; Flags: dontcopy
Source: "..\LocalOnly\WorkspaceRoot\Reference\Databases\DBKart.mdf"; DestDir: "{tmp}"; DestName: "FuntinoDeviceOperations-seed.mdf"; Flags: dontcopy
Source: "..\LocalOnly\WorkspaceRoot\Reference\Databases\DBKart_log.ldf"; DestDir: "{tmp}"; DestName: "FuntinoDeviceOperations-seed_log.ldf"; Flags: dontcopy

#if TargetOS == "Windows7x86"
Source: "Prerequisites\DotNet461\NDP461-KB3102436-x86-x64-AllOS-ENU.exe"; DestDir: "{tmp}"; DestName: "NDP461-KB3102436-x86-x64-AllOS-ENU.exe"; Flags: dontcopy
Source: "Prerequisites\Sql2014\SQLEXPR_x86_ENU.exe"; DestDir: "{tmp}"; DestName: "SQL2014Express-x86.exe"; Flags: dontcopy
Source: "Prerequisites\Windows7\Windows6.1-KB4490628-x86.msu"; DestDir: "{tmp}"; DestName: "Windows6.1-KB4490628-x86.msu"; Flags: dontcopy
Source: "Prerequisites\Windows7\Windows6.1-KB4474419-v3-x86.msu"; DestDir: "{tmp}"; DestName: "Windows6.1-KB4474419-v3-x86.msu"; Flags: dontcopy
#endif

#if TargetOS == "Windows7x64"
Source: "Prerequisites\DotNet461\NDP461-KB3102436-x86-x64-AllOS-ENU.exe"; DestDir: "{tmp}"; DestName: "NDP461-KB3102436-x86-x64-AllOS-ENU.exe"; Flags: dontcopy
Source: "Prerequisites\Sql2014\SQLEXPR_x64_ENU.exe"; DestDir: "{tmp}"; DestName: "SQL2014Express-x64.exe"; Flags: dontcopy
Source: "Prerequisites\Windows7\Windows6.1-KB4490628-x64.msu"; DestDir: "{tmp}"; DestName: "Windows6.1-KB4490628-x64.msu"; Flags: dontcopy
Source: "Prerequisites\Windows7\Windows6.1-KB4474419-v3-x64.msu"; DestDir: "{tmp}"; DestName: "Windows6.1-KB4474419-v3-x64.msu"; Flags: dontcopy
#endif

#if TargetOS == "Windows8"
Source: "Prerequisites\DotNet461\NDP461-KB3102436-x86-x64-AllOS-ENU.exe"; DestDir: "{tmp}"; DestName: "NDP461-KB3102436-x86-x64-AllOS-ENU.exe"; Flags: dontcopy
Source: "Prerequisites\Sql2017\SQLEXPR_x64_ENU.exe"; DestDir: "{tmp}"; DestName: "SQL2017Express-x64.exe"; Flags: dontcopy
#endif

#if TargetOS == "Windows81"
Source: "Prerequisites\DotNet461\NDP461-KB3102436-x86-x64-AllOS-ENU.exe"; DestDir: "{tmp}"; DestName: "NDP461-KB3102436-x86-x64-AllOS-ENU.exe"; Flags: dontcopy
Source: "Prerequisites\Sql2017\SQLEXPR_x64_ENU.exe"; DestDir: "{tmp}"; DestName: "SQL2017Express-x64.exe"; Flags: dontcopy
Source: "Prerequisites\Windows81\Windows8.1-KB2919442-x64.msu"; DestDir: "{tmp}"; DestName: "Windows8.1-KB2919442-x64.msu"; Flags: dontcopy
Source: "Prerequisites\Windows81\Windows8.1-KB2919355-x64.msu"; DestDir: "{tmp}"; DestName: "Windows8.1-KB2919355-x64.msu"; Flags: dontcopy
#endif

#if TargetOS == "Windows10"
Source: "Prerequisites\DotNet461\NDP461-KB3102436-x86-x64-AllOS-ENU.exe"; DestDir: "{tmp}"; DestName: "NDP461-KB3102436-x86-x64-AllOS-ENU.exe"; Flags: dontcopy
Source: "Prerequisites\Sql2019\SQLEXPR_x64_ENU.exe"; DestDir: "{tmp}"; DestName: "SQL2019Express-x64.exe"; Flags: dontcopy
#endif

#if TargetOS == "Windows11"
Source: "Prerequisites\Sql2019\SQLEXPR_x64_ENU.exe"; DestDir: "{tmp}"; DestName: "SQL2019Express-x64.exe"; Flags: dontcopy
#endif

[Icons]
Name: "{group}\Funtino Control Center"; Filename: "{app}\FuntinoControlCenter.exe"
Name: "{autodesktop}\Funtino Control Center"; Filename: "{app}\FuntinoControlCenter.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\FuntinoControlCenter.exe"; Description: "Launch Funtino Control Center"; Flags: nowait postinstall skipifsilent; Check: IsSelectedUserCurrent

[Code]
var
  DatabaseUserPage: TInputQueryWizardPage;

procedure InitializeWizard;
var
  InitialAccount: String;
begin
  DatabaseUserPage := CreateInputQueryPage(wpSelectDir,
    'Database access',
    'Choose the Windows account that will use Funtino Control Center',
    'The installer grants this account access to the Funtino application databases.');
  DatabaseUserPage.Add('Windows account (COMPUTER\User or DOMAIN\User):', False);
  InitialAccount := GetEnv('USERDOMAIN');
  if InitialAccount = '' then
    InitialAccount := GetComputerNameString();
  InitialAccount := InitialAccount + '\' + GetUserNameString();
  DatabaseUserPage.Values[0] := InitialAccount;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  if CurPageID = DatabaseUserPage.ID then begin
    if Trim(DatabaseUserPage.Values[0]) = '' then begin
      MsgBox('Enter the Windows account that will use this installation.', mbError, MB_OK);
      Result := False;
    end else if Pos('"', DatabaseUserPage.Values[0]) > 0 then begin
      MsgBox('The Windows account cannot contain a quotation mark.', mbError, MB_OK);
      Result := False;
    end;
  end;
end;

function IsSelectedUserCurrent: Boolean;
var
  DomainName: String;
  CurrentAccount: String;
begin
  DomainName := GetEnv('USERDOMAIN');
  if DomainName = '' then
    DomainName := GetComputerNameString();
  CurrentAccount := DomainName + '\' + GetUserNameString();
  Result := CompareText(Trim(DatabaseUserPage.Values[0]), CurrentAccount) = 0;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  PowerShellPath, Arguments, ScriptPath, WindowsAccount, TargetOSName: String;
  ResultCode: Integer;
begin
  Result := '';
  NeedsRestart := False;
  try
    ExtractTemporaryFile('Install-Funtino-Prerequisites.ps1');
    ExtractTemporaryFile('FuntinoParkOperations-seed.mdf');
    ExtractTemporaryFile('FuntinoParkOperations-seed_log.ldf');
    ExtractTemporaryFile('FuntinoDeviceOperations-seed.mdf');
    ExtractTemporaryFile('FuntinoDeviceOperations-seed_log.ldf');
#if TargetOS == "Windows7x86"
    ExtractTemporaryFile('NDP461-KB3102436-x86-x64-AllOS-ENU.exe');
    ExtractTemporaryFile('SQL2014Express-x86.exe');
    ExtractTemporaryFile('Windows6.1-KB4490628-x86.msu');
    ExtractTemporaryFile('Windows6.1-KB4474419-v3-x86.msu');
#endif
#if TargetOS == "Windows7x64"
    ExtractTemporaryFile('NDP461-KB3102436-x86-x64-AllOS-ENU.exe');
    ExtractTemporaryFile('SQL2014Express-x64.exe');
    ExtractTemporaryFile('Windows6.1-KB4490628-x64.msu');
    ExtractTemporaryFile('Windows6.1-KB4474419-v3-x64.msu');
#endif
#if TargetOS == "Windows8"
    ExtractTemporaryFile('NDP461-KB3102436-x86-x64-AllOS-ENU.exe');
    ExtractTemporaryFile('SQL2017Express-x64.exe');
#endif
#if TargetOS == "Windows81"
    ExtractTemporaryFile('NDP461-KB3102436-x86-x64-AllOS-ENU.exe');
    ExtractTemporaryFile('SQL2017Express-x64.exe');
    ExtractTemporaryFile('Windows8.1-KB2919442-x64.msu');
    ExtractTemporaryFile('Windows8.1-KB2919355-x64.msu');
#endif
#if TargetOS == "Windows10"
    ExtractTemporaryFile('NDP461-KB3102436-x86-x64-AllOS-ENU.exe');
    ExtractTemporaryFile('SQL2019Express-x64.exe');
#endif
#if TargetOS == "Windows11"
    ExtractTemporaryFile('SQL2019Express-x64.exe');
#endif
  except
    Result := 'Setup could not extract its bundled prerequisite files. Free disk space and retry.';
    Exit;
  end;

  if IsWin64 then
    PowerShellPath := ExpandConstant('{win}\Sysnative\WindowsPowerShell\v1.0\powershell.exe')
  else
    PowerShellPath := ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe');
  ScriptPath := ExpandConstant('{tmp}\Install-Funtino-Prerequisites.ps1');
  WindowsAccount := Trim(DatabaseUserPage.Values[0]);
#if TargetOS == "Windows7x86"
  TargetOSName := 'Windows7x86';
#endif
#if TargetOS == "Windows7x64"
  TargetOSName := 'Windows7x64';
#endif
#if TargetOS == "Windows8"
  TargetOSName := 'Windows8';
#endif
#if TargetOS == "Windows81"
  TargetOSName := 'Windows81';
#endif
#if TargetOS == "Windows10"
  TargetOSName := 'Windows10';
#endif
#if TargetOS == "Windows11"
  TargetOSName := 'Windows11';
#endif
  Arguments := '-NoProfile -NonInteractive -ExecutionPolicy Bypass -File ' + AddQuotes(ScriptPath) +
    ' -TargetOS ' + AddQuotes(TargetOSName) +
    ' -AllowedWindowsAccount ' + AddQuotes(WindowsAccount);

  if not Exec(PowerShellPath, Arguments, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then begin
    Result := 'Windows PowerShell could not start. Check that Windows PowerShell is enabled.';
    Exit;
  end;
  if ResultCode = 3010 then begin
    Result := 'A prerequisite requested a Windows restart. Restart the computer, then run this installer again. Setup logs are under %ProgramData%\Funtino\ControlCenter\InstallLogs.';
    Exit;
  end;
  if ResultCode <> 0 then begin
    Result := 'SQL Server or a required runtime could not be installed. Review the latest installer-prerequisites-*.log under %ProgramData%\Funtino\ControlCenter\InstallLogs and the SQL Server Setup logs, fix the reported issue, then retry.';
    Exit;
  end;
end;
