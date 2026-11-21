param(
    [Parameter(Mandatory=$true)]
    [string]$ApplicationPath,
    [Parameter(Mandatory=$true)]
    [string]$WindowsAccount
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $ApplicationPath -PathType Leaf)) {
    throw 'The installed application executable was not found.'
}
if ([string]::IsNullOrWhiteSpace($WindowsAccount) -or $WindowsAccount.Contains('"')) {
    throw 'The selected Windows account is not valid.'
}

$account = New-Object System.Security.Principal.NTAccount($WindowsAccount)
$sid = $account.Translate([System.Security.Principal.SecurityIdentifier]).Value
$applicationXml = [System.Security.SecurityElement]::Escape((Resolve-Path -LiteralPath $ApplicationPath).Path)
$workingDirectoryXml = [System.Security.SecurityElement]::Escape([System.IO.Path]::GetDirectoryName($ApplicationPath))
$sidXml = [System.Security.SecurityElement]::Escape($sid)
$taskName = 'Funtino Control Center Edge Sync'
$taskXml = @"
<?xml version="1.0" encoding="UTF-8"?>
<Task version="1.2" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
  <RegistrationInfo>
    <Description>همگام‌سازی امن رویدادهای شعبه در پس‌زمینه</Description>
  </RegistrationInfo>
  <Triggers>
    <LogonTrigger>
      <Enabled>true</Enabled>
      <UserId>$sidXml</UserId>
    </LogonTrigger>
  </Triggers>
  <Principals>
    <Principal id="Author">
      <UserId>$sidXml</UserId>
      <LogonType>InteractiveToken</LogonType>
      <RunLevel>LeastPrivilege</RunLevel>
    </Principal>
  </Principals>
  <Settings>
    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
    <AllowHardTerminate>false</AllowHardTerminate>
    <StartWhenAvailable>true</StartWhenAvailable>
    <Enabled>true</Enabled>
    <Hidden>true</Hidden>
    <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>
    <RestartOnFailure>
      <Interval>PT1M</Interval>
      <Count>999</Count>
    </RestartOnFailure>
  </Settings>
  <Actions Context="Author">
    <Exec>
      <Command>$applicationXml</Command>
      <Arguments>--edge-sync</Arguments>
      <WorkingDirectory>$workingDirectoryXml</WorkingDirectory>
    </Exec>
  </Actions>
</Task>
"@

$temporaryXml = [System.IO.Path]::ChangeExtension([System.IO.Path]::GetTempFileName(), '.xml')
try {
    [System.IO.File]::WriteAllText($temporaryXml, $taskXml, (New-Object System.Text.UTF8Encoding($true)))
    $arguments = '/Create /TN "' + $taskName + '" /XML "' + $temporaryXml + '" /F'
    $process = Start-Process -FilePath (Join-Path $env:windir 'System32\schtasks.exe') -ArgumentList $arguments -Wait -PassThru -WindowStyle Hidden
    if ($process.ExitCode -ne 0) {
        throw "Windows Task Scheduler registration failed with exit code $($process.ExitCode)."
    }
} finally {
    if (Test-Path -LiteralPath $temporaryXml) {
        Remove-Item -LiteralPath $temporaryXml -Force
    }
}
