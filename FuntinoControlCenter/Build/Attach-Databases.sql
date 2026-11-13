/*
  Replace @DatabaseFolder with the actual absolute folder path after extraction.
  Example: C:\FuntinoControlCenter\Databases
  Run in SSMS as a SQL Server administrator. This attaches only the *_New copies.
*/
USE [master];
GO
DECLARE @DatabaseFolder nvarchar(4000) = N'C:\FuntinoControlCenter\Databases';
DECLARE @Sql nvarchar(max);

IF DB_ID(N'SharjReportDB0216_New') IS NULL
BEGIN
    SET @Sql = N'CREATE DATABASE [SharjReportDB0216_New] ON (FILENAME=N''' + REPLACE(@DatabaseFolder + N'\SharjReportDB0216_New.mdf', N'''', N'''''') + N'''), (FILENAME=N''' + REPLACE(@DatabaseFolder + N'\SharjReportDB0216_New_log.ldf', N'''', N'''''') + N''') FOR ATTACH;';
    EXEC sys.sp_executesql @Sql;
END;
GO
DECLARE @DatabaseFolder2 nvarchar(4000) = N'C:\FuntinoControlCenter\Databases';
DECLARE @Sql2 nvarchar(max);
IF DB_ID(N'DBKart_New') IS NULL
BEGIN
    SET @Sql2 = N'CREATE DATABASE [DBKart_New] ON (FILENAME=N''' + REPLACE(@DatabaseFolder2 + N'\DBKart_New.mdf', N'''', N'''''') + N'''), (FILENAME=N''' + REPLACE(@DatabaseFolder2 + N'\DBKart_New_log.ldf', N'''', N'''''') + N''') FOR ATTACH;';
    EXEC sys.sp_executesql @Sql2;
END;
GO
