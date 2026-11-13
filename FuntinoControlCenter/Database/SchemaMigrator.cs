using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using FuntinoControlCenter.Security;

namespace FuntinoControlCenter.Database
{
    internal static class SchemaMigrator
    {
        public static string InitialAdminPassword { get; private set; }

        public static void EnsureAll()
        {
            InitialAdminPassword = null;
            EnsureCatalog(DatabaseManager.MainCatalog, false);
            EnsureCatalog(DatabaseManager.DeviceCatalog, true);
        }

        private static void EnsureCatalog(string catalog, bool deviceDatabase)
        {
            using (var connection = deviceDatabase ? DatabaseManager.CreateDeviceConnection() : DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        Execute(connection, transaction, @"IF OBJECT_ID(N'dbo.AppSchemaVersion', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.AppSchemaVersion (VersionNumber int NOT NULL CONSTRAINT PK_AppSchemaVersion PRIMARY KEY, AppliedAt datetime2(0) NOT NULL CONSTRAINT DF_AppSchemaVersion_AppliedAt DEFAULT(SYSDATETIME()), Description nvarchar(240) NOT NULL);
END");
                        Execute(connection, transaction, @"IF OBJECT_ID(N'dbo.AppSchemaVersion', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.AppSchemaVersion', N'Description') IS NULL ALTER TABLE dbo.AppSchemaVersion ADD Description nvarchar(240) NOT NULL CONSTRAINT DF_AppSchemaVersion_Description DEFAULT(N'upgrade');");
                        if (!deviceDatabase)
                        {
                            Execute(connection, transaction, @"IF OBJECT_ID(N'dbo.AppUsers', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.AppUsers (
  UserId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AppUsers PRIMARY KEY,
  Username nvarchar(64) NOT NULL,
  PasswordSalt varbinary(32) NOT NULL,
  PasswordHash varbinary(32) NOT NULL,
  Iterations int NOT NULL,
  IsAdmin bit NOT NULL CONSTRAINT DF_AppUsers_IsAdmin DEFAULT(0),
  IsActive bit NOT NULL CONSTRAINT DF_AppUsers_IsActive DEFAULT(1),
  MustChangePassword bit NOT NULL CONSTRAINT DF_AppUsers_MustChangePassword DEFAULT(0),
  CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_AppUsers_CreatedAt DEFAULT(SYSDATETIME()),
  LastLoginAt datetime2(0) NULL,
  CONSTRAINT UQ_AppUsers_Username UNIQUE(Username)
 );
END
IF OBJECT_ID(N'dbo.AppAuditLog', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.AppAuditLog (
  LogId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_AppAuditLog PRIMARY KEY,
  UserId int NULL,
  Action nvarchar(80) NOT NULL,
  EntityName nvarchar(128) NULL,
  EntityKey nvarchar(256) NULL,
  Details nvarchar(1000) NULL,
  CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_AppAuditLog_CreatedAt DEFAULT(SYSDATETIME())
 );
 CREATE INDEX IX_AppAuditLog_CreatedAt ON dbo.AppAuditLog(CreatedAt DESC);
END
IF COL_LENGTH(N'dbo.AppUsers', N'MustChangePassword') IS NULL ALTER TABLE dbo.AppUsers ADD MustChangePassword bit NOT NULL CONSTRAINT DF_AppUsers_MustChangePassword_v2 DEFAULT(0);
IF COL_LENGTH(N'dbo.AppUsers', N'LastLoginAt') IS NULL ALTER TABLE dbo.AppUsers ADD LastLoginAt datetime2(0) NULL;");
                        }
                        else
                        {
                            Execute(connection, transaction, @"IF OBJECT_ID(N'dbo.AppDeviceEvents', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.AppDeviceEvents (
  EventId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_AppDeviceEvents PRIMARY KEY,
  EventType nvarchar(80) NOT NULL,
  PortName nvarchar(20) NULL,
  IsSuccess bit NOT NULL,
  Details nvarchar(500) NULL,
  CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_AppDeviceEvents_CreatedAt DEFAULT(SYSDATETIME())
 );
 CREATE INDEX IX_AppDeviceEvents_CreatedAt ON dbo.AppDeviceEvents(CreatedAt DESC);
END
IF OBJECT_ID(N'dbo.AppAuditLog', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.AppAuditLog (
  LogId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_AppAuditLog PRIMARY KEY,
  UserId int NULL,
  Action nvarchar(80) NOT NULL,
  EntityName nvarchar(128) NULL,
  EntityKey nvarchar(256) NULL,
  Details nvarchar(1000) NULL,
  CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_AppAuditLog_CreatedAt DEFAULT(SYSDATETIME())
 );
 CREATE INDEX IX_AppAuditLog_CreatedAt ON dbo.AppAuditLog(CreatedAt DESC);
END");
                        }
                        AddVersionIfMissing(connection, transaction, 1, "application metadata and schema tracker");
                        AddVersionIfMissing(connection, transaction, 2, deviceDatabase ? "device audit log" : "hashed application accounts and audit log");
                        AddVersionIfMissing(connection, transaction, 3, deviceDatabase ? "device diagnostics journal" : "application account compatibility columns");
                        transaction.Commit();
                    }
                    catch { transaction.Rollback(); throw; }
                }
            }
            if (!deviceDatabase) InitialAdminPassword = EnsureInitialAdmin();
        }

        private static string EnsureInitialAdmin()
        {
            string password = GenerateInitialAdminPassword();
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var command = new SqlCommand("IF EXISTS (SELECT 1 FROM dbo.AppUsers WHERE Username=N'Admin') SELECT CAST(0 AS int); ELSE BEGIN INSERT dbo.AppUsers(Username,PasswordSalt,PasswordHash,Iterations,IsAdmin,IsActive,MustChangePassword) VALUES(N'Admin',@salt,@hash,@iterations,1,1,1); SELECT CAST(1 AS int); END", connection))
                {
                    byte[] salt, hash; PasswordHasher.Create(password, out salt, out hash);
                    command.Parameters.Add("@salt", SqlDbType.VarBinary, 32).Value = salt;
                    command.Parameters.Add("@hash", SqlDbType.VarBinary, 32).Value = hash;
                    command.Parameters.Add("@iterations", SqlDbType.Int).Value = PasswordHasher.Iterations;
                    return Convert.ToInt32(command.ExecuteScalar()) == 1 ? password : null;
                }
            }
        }

        private static string GenerateInitialAdminPassword()
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%&*+-_";
            const int length = 24;
            int limit = 256 - (256 % alphabet.Length);
            var result = new StringBuilder(length);
            var bytes = new byte[48];
            using (var random = new RNGCryptoServiceProvider())
            {
                while (result.Length < length)
                {
                    random.GetBytes(bytes);
                    for (int i = 0; i < bytes.Length && result.Length < length; i++)
                    {
                        if (bytes[i] >= limit) continue;
                        result.Append(alphabet[bytes[i] % alphabet.Length]);
                    }
                }
            }
            return result.ToString();
        }

        private static void AddVersionIfMissing(SqlConnection connection, SqlTransaction transaction, int version, string description)
        {
            using (var command = new SqlCommand("IF NOT EXISTS(SELECT 1 FROM dbo.AppSchemaVersion WHERE VersionNumber=@v) INSERT dbo.AppSchemaVersion(VersionNumber,Description) VALUES(@v,@d)", connection, transaction))
            {
                command.Parameters.Add("@v", SqlDbType.Int).Value = version;
                command.Parameters.Add("@d", SqlDbType.NVarChar, 240).Value = description;
                command.ExecuteNonQuery();
            }
        }

        private static void Execute(SqlConnection connection, SqlTransaction transaction, string sql)
        {
            using (var command = new SqlCommand(sql, connection, transaction)) command.ExecuteNonQuery();
        }
    }
}
