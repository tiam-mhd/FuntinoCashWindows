# Funtino Control Center

Windows Forms source and the offline Windows installer build process for Funtino Control Center.

- Product version is defined once in [`Version.props`](FuntinoControlCenter/Build/Version.props).
- Build the application ZIP with Visual Studio / MSBuild and `FuntinoControlCenter/Build/Build-Windows.cmd`.
- Build six versioned offline installer variants after preparing the Microsoft prerequisite media described in [`Prerequisites.md`](FuntinoControlCenter/Build/Prerequisites.md), then run `FuntinoControlCenter/Build/Build-Installer.ps1`.
- Persian installation and compatibility notes: [`README_FA.md`](FuntinoControlCenter/README_FA.md).

Each installer creates an instance named `FuntinoSqlServer` and validates that `FuntinoParkOperations` and `FuntinoDeviceOperations` are already provisioned. Setup does not attach reference MDF/LDF files or transfer data. On the first application launch, migrations run against those operational catalogs; reference database access is a separate, reviewed, read-only transfer process. Outputs are split into Windows 7 x86 and x64, Windows 8 x64, Windows 8.1 x64, Windows 10 x64, and Windows 11 x64 packages. Their database engines are SQL Server 2014 Express SP3 for Windows 7, SQL Server 2017 Express for Windows 8/8.1, and SQL Server 2019 Express for Windows 10/11. Windows 8 and newer x86 systems are not supported.

Each package contains only its target's runtime prerequisites plus the two database seed pairs needed by the application. The large Windows 8.1 KB2919355 update is included only in the Windows 8.1 installer; Windows 7 update packages are split by architecture. See [`Prerequisites.md`](FuntinoControlCenter/Build/Prerequisites.md) for the package matrix, local database inputs, exact media, and download sources.
