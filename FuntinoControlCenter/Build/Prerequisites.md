# Offline installer inputs

`Build-Installer.ps1` builds the Release application once, then creates a separate offline installer for each supported Windows target. Each installer embeds only the SQL engine, Windows updates, and .NET runtime media needed by that target. The destination computer does not need internet access.

Put prerequisite files at these exact paths under `Build/Prerequisites/`. This folder is ignored by Git.

| Path | Package |
|---|---|
| `Prerequisites/DotNet461/NDP461-KB3102436-x86-x64-AllOS-ENU.exe` | .NET Framework 4.6.1 offline installer |
| `Prerequisites/Sql2014/SQLEXPR_x86_ENU.exe` | SQL Server 2014 Express SP3 x86 engine media |
| `Prerequisites/Sql2014/SQLEXPR_x64_ENU.exe` | SQL Server 2014 Express SP3 x64 engine media |
| `Prerequisites/Sql2017/SQLEXPR_x64_ENU.exe` | SQL Server 2017 Express x64 engine media, not the web bootstrapper |
| `Prerequisites/Sql2019/SQLEXPR_x64_ENU.exe` | SQL Server 2019 Express x64 engine media, not the web bootstrapper |
| `Prerequisites/Windows7/Windows6.1-KB4490628-x86.msu` | Windows 7 SP1 servicing stack update, x86 |
| `Prerequisites/Windows7/Windows6.1-KB4490628-x64.msu` | Windows 7 SP1 servicing stack update, x64 |
| `Prerequisites/Windows7/Windows6.1-KB4474419-v3-x86.msu` | Windows 7 SP1 SHA-2 support update, x86 |
| `Prerequisites/Windows7/Windows6.1-KB4474419-v3-x64.msu` | Windows 7 SP1 SHA-2 support update, x64 |
| `Prerequisites/Windows81/Windows8.1-KB2919442-x64.msu` | Windows 8.1 servicing stack prerequisite, x64 |
| `Prerequisites/Windows81/Windows8.1-KB2919355-x64.msu` | Windows 8.1 Update required before .NET Framework 4.6.1, x64 |

Download the official Microsoft packages from [.NET Framework 4.6.1](https://www.microsoft.com/en-us/download/details.aspx?id=49982), [SQL Server 2014 SP3 Express](https://www.microsoft.com/en-us/download/details.aspx?id=57473), [SQL Server 2017 Express](https://www.microsoft.com/en-us/download/details.aspx?id=55994), [SQL Server 2019 Express](https://www.microsoft.com/en-us/download/details.aspx?id=108837), [Windows 7 servicing stack update KB4490628](https://support.microsoft.com/en-us/topic/servicing-stack-update-for-windows-7-sp1-and-windows-server-2008-r2-sp1-march-12-2019-b4dc0cff-d4f2-a408-0cb1-cb8e918feeba), [Windows 7 SHA-2 update KB4474419](https://support.microsoft.com/en-us/topic/sha-2-code-signing-support-update-for-windows-server-2008-r2-windows-7-and-windows-server-2008-september-23-2019-84a8aad5-d8d9-2d5c-6d78-34f9aa5f8339), [Windows 8.1 KB2919442](https://www.microsoft.com/en-us/download/details.aspx?id=42162), and [Windows 8.1 Update KB2919355](https://www.microsoft.com/en-us/download/details.aspx?id=42327). For SQL Server Express, choose **Download Media**, then **Express Core** and the required architecture; do not use the small web bootstrapper.

The builder checks Microsoft Authenticode signatures on SQL/.NET executable media and published SHA-256 hashes on Windows update packages where Microsoft publishes them.

## Installer variants

| Installer output | Supported target | Bundled runtime prerequisites |
|---|---|---|
| `FuntinoControlCenter-Setup-Windows7-x86-<version>.exe` | Windows 7 SP1 x86 | .NET 4.6.1, SQL Server 2014 Express SP3 x86, x86 KB4490628 + KB4474419 |
| `FuntinoControlCenter-Setup-Windows7-x64-<version>.exe` | Windows 7 SP1 x64 | .NET 4.6.1, SQL Server 2014 Express SP3 x64, x64 KB4490628 + KB4474419 |
| `FuntinoControlCenter-Setup-Windows8-x64-<version>.exe` | Windows 8 x64 | .NET 4.6.1, SQL Server 2017 Express x64 |
| `FuntinoControlCenter-Setup-Windows8.1-x64-<version>.exe` | Windows 8.1 x64 | .NET 4.6.1, SQL Server 2017 Express x64, KB2919442 + KB2919355 |
| `FuntinoControlCenter-Setup-Windows10-x64-<version>.exe` | Windows 10 x64 | .NET 4.6.1 offline media, SQL Server 2019 Express x64 |
| `FuntinoControlCenter-Setup-Windows11-x64-<version>.exe` | Windows 11 x64 | SQL Server 2019 Express x64; Windows supplies .NET Framework 4.8 or later |

The Windows 7 packages are split by architecture so each contains only its matching SQL engine and update files. Windows 8 and 8.1 are split because only Windows 8.1 needs the 724 MB KB2919355 package. Windows 11 includes .NET Framework 4.8 or 4.8.1 with the OS, so the separate .NET installer is not bundled there. The prerequisite script verifies the detected Windows version and architecture against the selected installer before installing SQL Server.

## Build

The build computer needs Visual Studio or Build Tools with the .NET Framework 4.6.1 Developer Pack, plus Inno Setup 6. The local seed files below are also required. Run the command from the repository root in PowerShell:

```powershell
Set-Location FuntinoControlCenter\Build
./Build-Installer.ps1
```

The versioned installers and a combined SHA-256 manifest are written to `Build/Output/`. Change the single semantic version in `Version.props` for each release. Runtime media, local build tools, and generated output are ignored by Git.

## Runtime behavior

All installations use the named SQL instance `FuntinoSqlServer`. Windows 7 uses SQL Server 2014 because later SQL Server releases do not support it; SQL Server 2014 requires .NET Framework 3.5, enabled from the Windows 7 component store. Windows 8 and 8.1 use SQL Server 2017. Windows 10 and Windows 11 use SQL Server 2019. Windows 8 and newer x86 systems are not supported.

On Windows 7, setup installs the servicing stack and SHA-2 update matching the package architecture. On Windows 8.1, setup applies KB2919442 and then KB2919355. If an update requests a restart, restart the computer and launch the same installer again; completed steps are skipped or treated as installed.

The installer requires these immutable seed inputs under `FuntinoControlCenter/LocalOnly/WorkspaceRoot/Reference/Databases/`:

- `SharjReportDB0216.mdf` and `SharjReportDB0216_log.ldf`
- `DBKart.mdf` and `DBKart_log.ldf`

The names `FuntinoParkOperations` and `FuntinoDeviceOperations` are the default operational catalogs; `App.config` can set different custom catalog names. The installer leaves an existing catalog and its files untouched. If a catalog is missing, it copies the immutable reference MDF/LDF into SQL Server's data directory and attaches that private copy under the configured name. It never renames an existing catalog or creates a `New` catalog. A partial pair of detached operational files stops setup instead of being overwritten.

On first application startup, additive, numbered migrations run against the operational copies only. Each database is migrated in its own SQL transaction; the two databases are not enlisted in a distributed transaction. The migration validates the expected legacy schema, captures table row counts, preserves legacy table/column contracts through `compat.Legacy*` views, adds application-owned objects under the `app` schema, and records migration versions transactionally. A failed migration rolls back that database's migration and can be retried. The installer grants the selected Windows account access to both operational catalogs.

Before an upgrade, back up both operational catalogs separately, for example with SQL Server Management Studio or `sqlcmd`:

```sql
BACKUP DATABASE [FuntinoParkOperations]
  TO DISK = N'D:\Backups\FuntinoParkOperations.bak' WITH CHECKSUM;
BACKUP DATABASE [FuntinoDeviceOperations]
  TO DISK = N'D:\Backups\FuntinoDeviceOperations.bak' WITH CHECKSUM;
RESTORE VERIFYONLY FROM DISK = N'D:\Backups\FuntinoParkOperations.bak' WITH CHECKSUM;
RESTORE VERIFYONLY FROM DISK = N'D:\Backups\FuntinoDeviceOperations.bak' WITH CHECKSUM;
```

For recovery, stop the application and its `Funtino Control Center Edge Sync` scheduled task, preserve the current operational catalogs, then restore each backup to the configured catalog name using SQL Server Management Studio. Restore the main and device backups as a pair from the same backup time. Do not restore the seed MDF/LDF over an operational catalog. After restore, start SQL Server and the application; the versioned migrations resume from the history stored in each restored database. Do not run these migration or restore steps against production as part of a build.

The branch connection form stores the endpoint, credential, organization, branch, and a stable installation ID. The HMAC key is protected with Windows DPAPI for the selected Windows user and is never written to the SQL metadata table or logs. The installer registers a hidden Scheduled Task for that user to start `--edge-sync` at logon and restart it after failure. The worker reads only the configured operational catalogs and continues retrying persisted outbox events without blocking the WinForms UI.

SQL Server 2014 reached end of support in July 2024. Windows 7 and Windows 8/8.1 are also out of Microsoft support. This installer provides application/runtime compatibility; it does not restore security servicing for those operating systems or SQL Server 2014.
