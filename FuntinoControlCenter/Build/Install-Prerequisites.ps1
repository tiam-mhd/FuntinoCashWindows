param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Windows7x86', 'Windows7x64', 'Windows8', 'Windows81', 'Windows10', 'Windows11')]
    [string]$TargetOS,

    [Parameter(Mandatory = $true)]
    [string]$AllowedWindowsAccount,

    [Parameter(Mandatory = $true)]
    [string]$ApplicationConfigPath
)

$ErrorActionPreference = 'Stop'
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$instanceName = 'FuntinoSqlServer'
if (-not (Test-Path -LiteralPath $ApplicationConfigPath -PathType Leaf)) {
    throw 'The application configuration file containing operational catalog names was not found.'
}
try {
    [xml]$applicationConfiguration = [System.IO.File]::ReadAllText($ApplicationConfigPath)
} catch {
    throw 'The application configuration file could not be read.'
}
$mainCatalogNodes = $applicationConfiguration.SelectNodes("/configuration/appSettings/add[@key='MainDatabaseCatalog']")
$deviceCatalogNodes = $applicationConfiguration.SelectNodes("/configuration/appSettings/add[@key='DeviceDatabaseCatalog']")
if ($mainCatalogNodes.Count -ne 1 -or $deviceCatalogNodes.Count -ne 1) {
    throw 'The application configuration must define exactly one main and one device catalog.'
}
$mainDatabase = $mainCatalogNodes[0].GetAttribute('value')
$deviceDatabase = $deviceCatalogNodes[0].GetAttribute('value')
if ($mainDatabase -notmatch '^[A-Za-z_][A-Za-z0-9_]{0,127}$' -or
    $deviceDatabase -notmatch '^[A-Za-z_][A-Za-z0-9_]{0,127}$' -or
    $mainDatabase -ieq $deviceDatabase) {
    throw 'The configured operational catalog names are invalid or identical.'
}
$logDirectory = Join-Path $env:ProgramData 'Funtino\ControlCenter\InstallLogs'
New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
$logFileName = 'installer-prerequisites-' + [DateTime]::Now.ToString('yyyyMMdd-HHmmss') + '.log'
$logPath = Join-Path $logDirectory $logFileName
try { Start-Transcript -Path $logPath -Force | Out-Null } catch { }

function Get-NetFxRelease {
    $release = 0
    $registryPaths = @(
        'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full',
        'HKLM:\SOFTWARE\Wow6432Node\Microsoft\NET Framework Setup\NDP\v4\Full'
    )
    foreach ($path in $registryPaths) {
        $item = Get-ItemProperty -Path $path -Name Release -ErrorAction SilentlyContinue
        if ($item -and [int]$item.Release -gt $release) { $release = [int]$item.Release }
    }
    return $release
}

function Invoke-InstallerProcess {
    param([string]$Path, [string]$Arguments, [int[]]$AcceptedExitCodes)
    Write-Host "Running: $Path $Arguments"
    $process = Start-Process -FilePath $Path -ArgumentList $Arguments -Wait -PassThru -WindowStyle Hidden
    if ($AcceptedExitCodes -notcontains [int]$process.ExitCode) {
        throw "Installer failed with exit code $($process.ExitCode): $Path. Review the Windows servicing log or SQL Server Setup Bootstrap\Log folder."
    }
    return [int]$process.ExitCode
}

function Install-NetFx461 {
    $release = Get-NetFxRelease
    if ($release -ge 394254) { return }

    $installer = Join-Path $scriptRoot 'NDP461-KB3102436-x86-x64-AllOS-ENU.exe'
    if (-not (Test-Path -LiteralPath $installer)) { throw "Required offline installer is missing: $installer" }

    $exitCode = Invoke-InstallerProcess -Path $installer -Arguments '/q /norestart' -AcceptedExitCodes @(0, 3010)
    if ($exitCode -eq 3010) { exit 3010 }
    if ((Get-NetFxRelease) -lt 394254) {
        throw '.NET Framework 4.6.1 setup completed without registering the required runtime. Review the Windows servicing logs.'
    }
}

function Install-OfflineWindowsUpdate {
    param([string]$PackageName)
    $package = Join-Path $scriptRoot $PackageName
    if (-not (Test-Path -LiteralPath $package)) { throw "Required offline Windows update is missing: $package" }
    $wusa = Join-Path $env:windir 'System32\wusa.exe'
    if ($env:PROCESSOR_ARCHITEW6432) {
        $nativeWusa = Join-Path $env:windir 'Sysnative\wusa.exe'
        if (Test-Path -LiteralPath $nativeWusa) { $wusa = $nativeWusa }
    }
    $arguments = '"' + $package + '" /quiet /norestart'
    return Invoke-InstallerProcess -Path $wusa -Arguments $arguments -AcceptedExitCodes @(0, 3010, 2359301, 2359302)
}

function Ensure-Windows7SigningSupport {
    param([bool]$Is64Bit)
    if ($Is64Bit) {
        $packages = @('Windows6.1-KB4490628-x64.msu', 'Windows6.1-KB4474419-v3-x64.msu')
    } else {
        $packages = @('Windows6.1-KB4490628-x86.msu', 'Windows6.1-KB4474419-v3-x86.msu')
    }

    $restartRequired = $false
    foreach ($package in $packages) {
        $exitCode = Install-OfflineWindowsUpdate -PackageName $package
        if ($exitCode -eq 3010 -or $exitCode -eq 2359301) { $restartRequired = $true }
    }
    if ($restartRequired) { exit 3010 }
}

function Ensure-Windows81DotNetPrerequisites {
    foreach ($package in @('Windows8.1-KB2919442-x64.msu', 'Windows8.1-KB2919355-x64.msu')) {
        $exitCode = Install-OfflineWindowsUpdate -PackageName $package
        if ($exitCode -eq 3010 -or $exitCode -eq 2359301) { exit 3010 }
    }
}

function Test-NetFx35Installed {
    $registryPaths = @(
        'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5',
        'HKLM:\SOFTWARE\Wow6432Node\Microsoft\NET Framework Setup\NDP\v3.5'
    )
    foreach ($path in $registryPaths) {
        $item = Get-ItemProperty -Path $path -Name Install -ErrorAction SilentlyContinue
        if ($item -and [int]$item.Install -eq 1) { return $true }
    }
    return $false
}

function Ensure-NetFx35 {
    if (Test-NetFx35Installed) { return }

    $dism = Join-Path $env:windir 'System32\dism.exe'
    if ($env:PROCESSOR_ARCHITEW6432) {
        $nativeDism = Join-Path $env:windir 'Sysnative\dism.exe'
        if (Test-Path -LiteralPath $nativeDism) { $dism = $nativeDism }
    }
    if (-not (Test-Path -LiteralPath $dism)) { throw '.NET Framework 3.5 is required by SQL Server 2014, but DISM is unavailable.' }
    Write-Host 'Enabling the Windows .NET Framework 3.5 feature for SQL Server 2014.'
    $process = Start-Process -FilePath $dism -ArgumentList '/Online /Enable-Feature /FeatureName:NetFx3 /All /LimitAccess /NoRestart /Quiet' -Wait -PassThru -WindowStyle Hidden
    $exitCode = [int]$process.ExitCode
    if ($exitCode -eq 3010) { exit 3010 }
    if ($exitCode -ne 0) {
        throw ".NET Framework 3.5 could not be enabled from the local Windows component store (DISM exit code $exitCode). Restore the component payload from the matching Windows installation source, then run this offline installer again. Review %windir%\Logs\DISM\dism.log."
    }
    if (-not (Test-NetFx35Installed)) {
        throw '.NET Framework 3.5 was not registered after DISM completed. Restore the component payload from the matching Windows installation source, then run this offline installer again. Review %windir%\Logs\DISM\dism.log.'
    }
}

function Get-DatabaseConnection {
    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
    $builder['Data Source'] = ".\$instanceName"
    $builder['Initial Catalog'] = 'master'
    $builder['Integrated Security'] = $true
    $builder['Connect Timeout'] = 5
    $builder['Encrypt'] = $false
    return New-Object System.Data.SqlClient.SqlConnection($builder.ConnectionString)
}

function Get-SqlProductVersion {
    $connection = Get-DatabaseConnection
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = "SELECT CONVERT(nvarchar(128), SERVERPROPERTY('ProductVersion'))"
        $version = [string]$command.ExecuteScalar()
        return [Version]$version
    } finally { $connection.Dispose() }
}

function Wait-SqlReady {
    param([int]$TimeoutSeconds = 180)
    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
    do {
        try { return (Get-SqlProductVersion) } catch { Start-Sleep -Seconds 3 }
    } while ([DateTime]::UtcNow -lt $deadline)
    throw "SQL Server instance '$instanceName' did not become ready. Review '$logPath' and the SQL Server Setup Bootstrap logs."
}

function Invoke-SqlNonQuery {
    param([string]$Catalog, [string]$Sql)
    $connection = Get-DatabaseConnection
    try {
        $connection.Open()
        if (-not [string]::IsNullOrEmpty($Catalog) -and $Catalog -ne 'master') {
            $connection.ChangeDatabase($Catalog)
        }
        $command = $connection.CreateCommand()
        $command.CommandTimeout = 120
        $command.CommandText = $Sql
        [void]$command.ExecuteNonQuery()
    } finally { $connection.Dispose() }
}

function Invoke-SqlScalar {
    param([string]$Catalog, [string]$Sql)
    $connection = Get-DatabaseConnection
    try {
        $connection.Open()
        if (-not [string]::IsNullOrEmpty($Catalog) -and $Catalog -ne 'master') {
            $connection.ChangeDatabase($Catalog)
        }
        $command = $connection.CreateCommand()
        $command.CommandTimeout = 120
        $command.CommandText = $Sql
        return $command.ExecuteScalar()
    } finally { $connection.Dispose() }
}

function Quote-SqlIdentifier {
    param([string]$Value)
    return '[' + $Value.Replace(']', ']]') + ']'
}

function Get-DatabaseDataDirectory {
    $directory = [string](Invoke-SqlScalar -Catalog 'master' -Sql "SELECT CONVERT(nvarchar(4000),SERVERPROPERTY('InstanceDefaultDataPath'))")
    if ([string]::IsNullOrWhiteSpace($directory)) {
        $masterFile = [string](Invoke-SqlScalar -Catalog 'master' -Sql "SELECT physical_name FROM sys.master_files WHERE database_id=DB_ID(N'master') AND file_id=1")
        if ([string]::IsNullOrWhiteSpace($masterFile)) { throw 'SQL Server did not report its default data directory.' }
        $directory = Split-Path -Parent $masterFile
    }
    return $directory.TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)
}

function Attach-SeedDatabase {
    param(
        [string]$DatabaseName,
        [string]$SourceDataFile,
        [string]$SourceLogFile
    )

    $escapedLiteral = $DatabaseName.Replace("'", "''")
    $escapedIdentifier = Quote-SqlIdentifier $DatabaseName
    if ([int](Invoke-SqlScalar -Catalog 'master' -Sql "SELECT CASE WHEN DB_ID(N'$escapedLiteral') IS NULL THEN 0 ELSE 1 END") -eq 1) {
        Write-Host "Database '$DatabaseName' already exists; its files and contents were left untouched."
        return
    }

    $sourceDataPath = Join-Path $scriptRoot $SourceDataFile
    $sourceLogPath = Join-Path $scriptRoot $SourceLogFile
    foreach ($source in @($sourceDataPath, $sourceLogPath)) {
        if (-not (Test-Path -LiteralPath $source -PathType Leaf)) { throw "Required reference database file is missing: $source" }
        if ((Get-Item -LiteralPath $source).Length -eq 0) { throw "Reference database file is empty: $source" }
    }

    $dataDirectory = Get-DatabaseDataDirectory
    if (-not (Test-Path -LiteralPath $dataDirectory -PathType Container)) {
        throw "SQL Server's data directory does not exist: $dataDirectory"
    }
    $targetDataPath = Join-Path $dataDirectory ($DatabaseName + '.mdf')
    $targetLogPath = Join-Path $dataDirectory ($DatabaseName + '_log.ldf')
    $dataExists = Test-Path -LiteralPath $targetDataPath -PathType Leaf
    $logExists = Test-Path -LiteralPath $targetLogPath -PathType Leaf
    if ($dataExists -xor $logExists) {
        throw "Only one database file exists for '$DatabaseName'. The installer will not overwrite it; inspect '$dataDirectory' before retrying."
    }
    if (-not $dataExists) {
        Copy-Item -LiteralPath $sourceDataPath -Destination $targetDataPath
        Copy-Item -LiteralPath $sourceLogPath -Destination $targetLogPath
        foreach ($path in @($targetDataPath, $targetLogPath)) {
            $file = Get-Item -LiteralPath $path
            if ($file.IsReadOnly) { $file.IsReadOnly = $false }
        }
    }

    $dataLiteral = $targetDataPath.Replace("'", "''")
    $logLiteral = $targetLogPath.Replace("'", "''")
    $attach = @"
CREATE DATABASE $escapedIdentifier
ON (FILENAME=N'$dataLiteral'),(FILENAME=N'$logLiteral')
FOR ATTACH;
"@
    Invoke-SqlNonQuery -Catalog 'master' -Sql $attach
    Invoke-SqlNonQuery -Catalog 'master' -Sql "ALTER DATABASE $escapedIdentifier SET COMPATIBILITY_LEVEL=110"

    $logicalDataName = [string](Invoke-SqlScalar -Catalog 'master' -Sql "SELECT TOP 1 name FROM sys.master_files WHERE database_id=DB_ID(N'$escapedLiteral') AND type_desc=N'ROWS' ORDER BY file_id")
    $logicalLogName = [string](Invoke-SqlScalar -Catalog 'master' -Sql "SELECT TOP 1 name FROM sys.master_files WHERE database_id=DB_ID(N'$escapedLiteral') AND type_desc=N'LOG' ORDER BY file_id")
    if ([string]::IsNullOrWhiteSpace($logicalDataName) -or [string]::IsNullOrWhiteSpace($logicalLogName)) {
        throw "SQL Server attached '$DatabaseName' but did not report both logical file names."
    }
    $newLogicalLogName = $DatabaseName + '_log'
    $renameFiles = ''
    if ($logicalDataName -ne $DatabaseName) {
        $renameFiles += "ALTER DATABASE $escapedIdentifier MODIFY FILE (NAME=N'" + $logicalDataName.Replace("'", "''") + "',NEWNAME=N'" + $DatabaseName.Replace("'", "''") + "');`r`n"
    }
    if ($logicalLogName -ne $newLogicalLogName) {
        $renameFiles += "ALTER DATABASE $escapedIdentifier MODIFY FILE (NAME=N'" + $logicalLogName.Replace("'", "''") + "',NEWNAME=N'" + $newLogicalLogName.Replace("'", "''") + "');`r`n"
    }
    if (-not [string]::IsNullOrEmpty($renameFiles)) { Invoke-SqlNonQuery -Catalog 'master' -Sql $renameFiles }
}

function Ensure-ApplicationAccount {
    param([string]$WindowsAccount, [string]$DatabaseName)
    $escapedLiteral = $WindowsAccount.Replace("'", "''")
    $escapedIdentifier = $WindowsAccount.Replace(']', ']]')
    $query = @"
IF SUSER_ID(N'$escapedLiteral') IS NULL
    CREATE LOGIN [$escapedIdentifier] FROM WINDOWS;
USE [$($DatabaseName.Replace(']', ']]'))];
IF DATABASE_PRINCIPAL_ID(N'$escapedLiteral') IS NULL
    CREATE USER [$escapedIdentifier] FOR LOGIN [$escapedIdentifier];
IF IS_ROLEMEMBER(N'db_owner', N'$escapedLiteral') <> 1
    ALTER ROLE [db_owner] ADD MEMBER [$escapedIdentifier];
"@
    Invoke-SqlNonQuery -Catalog 'master' -Sql $query
}

try {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw 'Setup must run with administrator privileges.'
    }

    if ([string]::IsNullOrEmpty($AllowedWindowsAccount) -or $AllowedWindowsAccount.Trim().Length -eq 0 -or $AllowedWindowsAccount.Contains('"')) {
        throw 'Enter one Windows account in COMPUTER\User or DOMAIN\User format.'
    }
    try {
        $account = New-Object System.Security.Principal.NTAccount($AllowedWindowsAccount)
        [void]$account.Translate([System.Security.Principal.SecurityIdentifier])
    } catch { throw "Windows account '$AllowedWindowsAccount' could not be resolved on this computer." }

    $operatingSystem = Get-WmiObject -Class Win32_OperatingSystem
    $osVersion = New-Object Version($operatingSystem.Version)
    if ([int]$operatingSystem.ProductType -ne 1) {
        throw 'This installer supports Windows client editions only.'
    }
    $processor = Get-WmiObject -Class Win32_Processor | Select-Object -First 1
    if ($processor.Architecture -ne 0 -and $processor.Architecture -ne 9) {
        throw 'Only x86 and x64 Intel/AMD processors are supported.'
    }
    if ($osVersion.Major -lt 6 -or ($osVersion.Major -eq 6 -and $osVersion.Minor -lt 1) -or ($osVersion.Major -eq 6 -and $osVersion.Minor -eq 1 -and [int]$operatingSystem.ServicePackMajorVersion -lt 1)) {
        throw 'Windows 7 SP1 or later is required.'
    }

    $is64Bit = $operatingSystem.OSArchitecture -match '64'
    $detectedTargetOS = $null
    if ($osVersion.Major -eq 6 -and $osVersion.Minor -eq 1) {
        $windowsFamily = 'Windows7'
        if ($is64Bit) { $detectedTargetOS = 'Windows7x64' } else { $detectedTargetOS = 'Windows7x86' }
        $expectedSqlMajor = 12
    } elseif ($osVersion.Major -eq 6 -and $osVersion.Minor -eq 2) {
        if (-not $is64Bit) { throw 'Windows 8 is supported only in its 64-bit edition.' }
        $windowsFamily = 'Windows8'
        $detectedTargetOS = 'Windows8'
        $expectedSqlMajor = 14
    } elseif ($osVersion.Major -eq 6 -and $osVersion.Minor -eq 3) {
        if (-not $is64Bit) { throw 'Windows 8.1 is supported only in its 64-bit edition.' }
        $windowsFamily = 'Windows81'
        $detectedTargetOS = 'Windows81'
        $expectedSqlMajor = 14
    } elseif ($osVersion.Major -eq 10 -and [int]$operatingSystem.BuildNumber -ge 22000) {
        if (-not $is64Bit) { throw 'Windows 10 and later are supported only in their 64-bit editions.' }
        $windowsFamily = 'Windows11'
        $detectedTargetOS = 'Windows11'
        $expectedSqlMajor = 15
    } elseif ($osVersion.Major -eq 10 -and [int]$operatingSystem.BuildNumber -ge 10240) {
        if (-not $is64Bit) { throw 'Windows 10 and later are supported only in their 64-bit editions.' }
        $windowsFamily = 'Windows10'
        $detectedTargetOS = 'Windows10'
        $expectedSqlMajor = 15
    } else {
        throw "This installer supports Windows 7 SP1, Windows 8/8.1 x64, Windows 10 x64, and Windows 11 x64. Detected Windows $($operatingSystem.Caption) ($($operatingSystem.Version))."
    }

    if ($TargetOS -ne $detectedTargetOS) {
        throw "This package is for '$TargetOS', but this computer is '$detectedTargetOS'. Download or run the matching installer."
    }

    if ($windowsFamily -eq 'Windows7') { Ensure-Windows7SigningSupport -Is64Bit $is64Bit }
    if ($windowsFamily -eq 'Windows81') { Ensure-Windows81DotNetPrerequisites }
    Install-NetFx461
    if ($windowsFamily -eq 'Windows7') { Ensure-NetFx35 }

    $serviceName = 'MSSQL$' + $instanceName
    $service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
    if ($service) {
        if ($service.Status -ne 'Running') { Start-Service -Name $serviceName }
        $sqlVersion = Wait-SqlReady
        $sqlMajor = $sqlVersion.Major
        if (($expectedSqlMajor -lt 15 -and $sqlMajor -ne $expectedSqlMajor) -or ($expectedSqlMajor -eq 15 -and $sqlMajor -lt 15)) {
            throw "Windows '$windowsFamily' requires SQL Server major version $expectedSqlMajor or later as packaged. Existing instance '$instanceName' is version $sqlVersion. Remove or rename that instance before installing."
        }
        if ($expectedSqlMajor -eq 12 -and $sqlVersion -lt [Version]'12.0.6024.0') {
            throw "Windows 7 requires SQL Server 2014 SP3 (12.0.6024.0 or later). Existing instance '$instanceName' is version $sqlVersion. Upgrade or rename it before installing."
        }
    } else {
        if ($expectedSqlMajor -eq 12) {
            if ($is64Bit) {
                $sqlInstaller = Join-Path $scriptRoot 'SQL2014Express-x64.exe'
            } else {
                $sqlInstaller = Join-Path $scriptRoot 'SQL2014Express-x86.exe'
            }
        } elseif ($expectedSqlMajor -eq 14) {
            $sqlInstaller = Join-Path $scriptRoot 'SQL2017Express-x64.exe'
        } else {
            $sqlInstaller = Join-Path $scriptRoot 'SQL2019Express-x64.exe'
        }
        if (-not (Test-Path -LiteralPath $sqlInstaller)) { throw "Required offline SQL Server media is missing: $sqlInstaller" }

        $arguments = '/Q /ACTION=Install /FEATURES=SQL /INSTANCENAME=' + $instanceName + ' /SQLSYSADMINACCOUNTS="BUILTIN\Administrators" /SQLSVCSTARTUPTYPE=Automatic /TCPENABLED=0 /IACCEPTSQLSERVERLICENSETERMS /UpdateEnabled=False'
        $exitCode = Invoke-InstallerProcess -Path $sqlInstaller -Arguments $arguments -AcceptedExitCodes @(0, 3010)
        if ($exitCode -eq 3010) { exit 3010 }
        $sqlVersion = Wait-SqlReady
        $sqlMajor = $sqlVersion.Major
        if ($sqlMajor -ne $expectedSqlMajor) { throw "SQL setup completed with unexpected version $sqlVersion; expected major version $expectedSqlMajor." }
        if ($expectedSqlMajor -eq 12 -and $sqlVersion -lt [Version]'12.0.6024.0') {
            throw "SQL setup completed with version $sqlVersion, but Windows 7 requires SQL Server 2014 SP3 (12.0.6024.0 or later)."
        }
    }

    $service = Get-Service -Name $serviceName
    if ($service.Status -ne 'Running') { Start-Service -Name $serviceName }
    & "$env:windir\System32\sc.exe" config $serviceName start= auto | Out-Null

    [void](Wait-SqlReady)
    Attach-SeedDatabase -DatabaseName $mainDatabase -SourceDataFile 'FuntinoParkOperations-seed.mdf' -SourceLogFile 'FuntinoParkOperations-seed_log.ldf'
    Attach-SeedDatabase -DatabaseName $deviceDatabase -SourceDataFile 'FuntinoDeviceOperations-seed.mdf' -SourceLogFile 'FuntinoDeviceOperations-seed_log.ldf'
    Ensure-ApplicationAccount -WindowsAccount $AllowedWindowsAccount -DatabaseName $mainDatabase
    Ensure-ApplicationAccount -WindowsAccount $AllowedWindowsAccount -DatabaseName $deviceDatabase

    Write-Host "SQL Server instance '$instanceName' is ready with operational catalogs '$mainDatabase' and '$deviceDatabase'. Existing catalogs are preserved; missing catalogs use private reference copies. The application will migrate their schemas on first launch."
    try { Stop-Transcript | Out-Null } catch { }
    exit 0
} catch {
    Write-Host $_ -ForegroundColor Red
    try { Stop-Transcript | Out-Null } catch { }
    exit 1
}
