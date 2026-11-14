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
            InitialAdminPassword = EnsureInitialAdmin();
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
                        // The installed catalog is a private copy of the immutable reference database.
                        // Its data and legacy objects are migrated once, atomically, before app objects are added.
                        LegacySchemaMigrator.Apply(connection, transaction, deviceDatabase);
                        EnsureApplicationObjects(connection, transaction, deviceDatabase);
                        AddVersionIfMissing(connection, transaction, 2, "Funtino application identity and audit schema");
                        if (deviceDatabase)
                            AddVersionIfMissing(connection, transaction, 3, "Funtino device diagnostics schema");
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static void EnsureApplicationObjects(SqlConnection connection, SqlTransaction transaction, bool deviceDatabase)
        {
            Execute(connection, transaction, @"
IF OBJECT_ID(N'app.ApplicationUser', N'U') IS NULL
BEGIN
 CREATE TABLE app.ApplicationUser
 (
  ApplicationUserId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ApplicationUser PRIMARY KEY,
  UserName nvarchar(64) NOT NULL,
  PasswordSalt varbinary(32) NOT NULL,
  PasswordHash varbinary(32) NOT NULL,
  PasswordHashIterations int NOT NULL,
  IsAdministrator bit NOT NULL CONSTRAINT DF_ApplicationUser_IsAdministrator DEFAULT(0),
  IsActive bit NOT NULL CONSTRAINT DF_ApplicationUser_IsActive DEFAULT(1),
  MustChangePassword bit NOT NULL CONSTRAINT DF_ApplicationUser_MustChangePassword DEFAULT(0),
  CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_ApplicationUser_CreatedAt DEFAULT(SYSDATETIME()),
  LastLoginAt datetime2(0) NULL,
  CONSTRAINT UQ_ApplicationUser_UserName UNIQUE(UserName)
 );
END;
IF OBJECT_ID(N'app.AuditEvent', N'U') IS NULL
BEGIN
 CREATE TABLE app.AuditEvent
 (
  AuditEventId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditEvent PRIMARY KEY,
  ApplicationUserId int NULL,
  ActionName nvarchar(80) NOT NULL,
  EntityName nvarchar(128) NULL,
  EntityKey nvarchar(256) NULL,
  Details nvarchar(1000) NULL,
  CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_AuditEvent_CreatedAt DEFAULT(SYSDATETIME())
 );
 CREATE INDEX IX_AuditEvent_CreatedAt ON app.AuditEvent(CreatedAt DESC);
END;");

            if (deviceDatabase)
            {
                Execute(connection, transaction, @"
IF OBJECT_ID(N'app.DeviceDiagnosticEvent', N'U') IS NULL
BEGIN
 CREATE TABLE app.DeviceDiagnosticEvent
 (
  DeviceDiagnosticEventId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_DeviceDiagnosticEvent PRIMARY KEY,
  EventType nvarchar(80) NOT NULL,
  PortName nvarchar(20) NULL,
  IsSuccess bit NOT NULL,
  Details nvarchar(500) NULL,
  CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_DeviceDiagnosticEvent_CreatedAt DEFAULT(SYSDATETIME())
 );
 CREATE INDEX IX_DeviceDiagnosticEvent_CreatedAt ON app.DeviceDiagnosticEvent(CreatedAt DESC);
END");
            }
        }

        private static string EnsureInitialAdmin()
        {
            string password = GenerateInitialAdminPassword();
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var command = new SqlCommand(@"
IF EXISTS (SELECT 1 FROM app.ApplicationUser WHERE UserName=N'Admin') SELECT CAST(0 AS int);
ELSE
BEGIN
 INSERT app.ApplicationUser(UserName,PasswordSalt,PasswordHash,PasswordHashIterations,IsAdministrator,IsActive,MustChangePassword)
 VALUES(N'Admin',@salt,@hash,@iterations,1,1,1);
 SELECT CAST(1 AS int);
END", connection))
                {
                    byte[] salt, hash;
                    PasswordHasher.Create(password, out salt, out hash);
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
            using (var command = new SqlCommand(
                "IF NOT EXISTS(SELECT 1 FROM app.SchemaMigrationHistory WHERE MigrationVersion=@version) " +
                "INSERT app.SchemaMigrationHistory(MigrationVersion,Description) VALUES(@version,@description)",
                connection, transaction))
            {
                command.Parameters.Add("@version", SqlDbType.Int).Value = version;
                command.Parameters.Add("@description", SqlDbType.NVarChar, 240).Value = description;
                command.ExecuteNonQuery();
            }
        }

        private static void Execute(SqlConnection connection, SqlTransaction transaction, string sql)
        {
            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.CommandTimeout = 180;
                command.ExecuteNonQuery();
            }
        }
    }
}
