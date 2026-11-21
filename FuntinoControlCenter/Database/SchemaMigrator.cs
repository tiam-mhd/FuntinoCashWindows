using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using FuntinoControlCenter.Security;

namespace FuntinoControlCenter.Database
{
    internal static class SchemaMigrator
    {
        public static string InitialAdminPassword { get; private set; }

        public static string TakeInitialAdminPassword()
        {
            string password = InitialAdminPassword;
            InitialAdminPassword = null;
            return password;
        }

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
                        {
                            AddVersionIfMissing(connection, transaction, 3, "Funtino device diagnostics schema");
                            EdgeSchemaMigrator.Ensure(connection, transaction, true);
                            if (!HasVersion(connection, transaction, 4))
                            {
                                RewriteOperationalCatalogReferences(connection, transaction);
                                AddVersionIfMissing(connection, transaction, 4, "Funtino edge outbox and custom catalog references");
                            }
                            if (!HasVersion(connection, transaction, 5))
                            {
                                RewriteOperationalCatalogReferences(connection, transaction);
                                AddVersionIfMissing(connection, transaction, 5, "Funtino bracketed operational catalog references");
                            }
                            if (!HasVersion(connection, transaction, 6))
                            {
                                RewriteOperationalCatalogReferences(connection, transaction);
                                AddVersionIfMissing(connection, transaction, 6, "Restore customized device catalog references");
                            }
                        }
                        else
                        {
                            EnsureAccessControlObjects(connection, transaction);
                            AddVersionIfMissing(connection, transaction, 4, "Funtino role-based access control");
                            EnsureCardOperationObjects(connection, transaction);
                            AddVersionIfMissing(connection, transaction, 5, "Funtino card operation audit");
                            EdgeSchemaMigrator.Ensure(connection, transaction, false);
                            if (!HasVersion(connection, transaction, 6))
                            {
                                RewriteOperationalCatalogReferences(connection, transaction);
                                AddVersionIfMissing(connection, transaction, 6, "Funtino customer snapshot and edge outbox");
                            }
                            if (!HasVersion(connection, transaction, 7))
                            {
                                RewriteOperationalCatalogReferences(connection, transaction);
                                AddVersionIfMissing(connection, transaction, 7, "Funtino bracketed operational catalog references and installation metadata");
                            }
                            if (!HasVersion(connection, transaction, 8))
                            {
                                RewriteOperationalCatalogReferences(connection, transaction);
                                AddVersionIfMissing(connection, transaction, 8, "Restore customized park catalog references");
                            }
                        }
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

            Execute(connection, transaction, @"
IF COL_LENGTH(N'app.ApplicationUser',N'DisplayName') IS NULL
 ALTER TABLE app.ApplicationUser ADD DisplayName nvarchar(100) NULL;
IF COL_LENGTH(N'app.ApplicationUser',N'AuthorizationVersion') IS NULL
 ALTER TABLE app.ApplicationUser ADD AuthorizationVersion int NOT NULL CONSTRAINT DF_ApplicationUser_AuthorizationVersion DEFAULT(1) WITH VALUES;");

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

        private static void EnsureAccessControlObjects(SqlConnection connection, SqlTransaction transaction)
        {
            Execute(connection, transaction, @"
IF OBJECT_ID(N'app.PermissionDefinition',N'U') IS NULL
BEGIN
 CREATE TABLE app.PermissionDefinition
 (
  PermissionCode nvarchar(100) NOT NULL CONSTRAINT PK_PermissionDefinition PRIMARY KEY,
  CategoryName nvarchar(100) NOT NULL,
  DisplayName nvarchar(160) NOT NULL,
  Description nvarchar(400) NOT NULL,
  DisplayOrder int NOT NULL
 );
END;
IF OBJECT_ID(N'app.ApplicationRole',N'U') IS NULL
BEGIN
 CREATE TABLE app.ApplicationRole
 (
  RoleId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ApplicationRole PRIMARY KEY,
  RoleCode nvarchar(64) NOT NULL,
  DisplayName nvarchar(100) NOT NULL,
  Description nvarchar(400) NULL,
  IsSystem bit NOT NULL CONSTRAINT DF_ApplicationRole_IsSystem DEFAULT(0),
  IsActive bit NOT NULL CONSTRAINT DF_ApplicationRole_IsActive DEFAULT(1),
  CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_ApplicationRole_CreatedAt DEFAULT(SYSDATETIME()),
  CONSTRAINT UQ_ApplicationRole_RoleCode UNIQUE(RoleCode),
  CONSTRAINT UQ_ApplicationRole_DisplayName UNIQUE(DisplayName)
 );
END;
IF OBJECT_ID(N'app.ApplicationUserRole',N'U') IS NULL
BEGIN
 CREATE TABLE app.ApplicationUserRole
 (
  ApplicationUserId int NOT NULL,
  RoleId int NOT NULL,
  AssignedAt datetime2(0) NOT NULL CONSTRAINT DF_ApplicationUserRole_AssignedAt DEFAULT(SYSDATETIME()),
  CONSTRAINT PK_ApplicationUserRole PRIMARY KEY(ApplicationUserId,RoleId),
  CONSTRAINT FK_ApplicationUserRole_User FOREIGN KEY(ApplicationUserId) REFERENCES app.ApplicationUser(ApplicationUserId),
  CONSTRAINT FK_ApplicationUserRole_Role FOREIGN KEY(RoleId) REFERENCES app.ApplicationRole(RoleId)
 );
 CREATE INDEX IX_ApplicationUserRole_RoleId ON app.ApplicationUserRole(RoleId,ApplicationUserId);
END;
IF OBJECT_ID(N'app.ApplicationRolePermission',N'U') IS NULL
BEGIN
 CREATE TABLE app.ApplicationRolePermission
 (
  RoleId int NOT NULL,
  PermissionCode nvarchar(100) NOT NULL,
  GrantedAt datetime2(0) NOT NULL CONSTRAINT DF_ApplicationRolePermission_GrantedAt DEFAULT(SYSDATETIME()),
  CONSTRAINT PK_ApplicationRolePermission PRIMARY KEY(RoleId,PermissionCode),
  CONSTRAINT FK_ApplicationRolePermission_Role FOREIGN KEY(RoleId) REFERENCES app.ApplicationRole(RoleId),
  CONSTRAINT FK_ApplicationRolePermission_Permission FOREIGN KEY(PermissionCode) REFERENCES app.PermissionDefinition(PermissionCode)
 );
 CREATE INDEX IX_ApplicationRolePermission_PermissionCode ON app.ApplicationRolePermission(PermissionCode,RoleId);
END;");

            foreach (var permission in PermissionCatalog.All)
            {
                using (var command = new SqlCommand(@"
IF EXISTS(SELECT 1 FROM app.PermissionDefinition WHERE PermissionCode=@code)
 UPDATE app.PermissionDefinition SET CategoryName=@category,DisplayName=@name,Description=@description,DisplayOrder=@order WHERE PermissionCode=@code;
ELSE
 INSERT app.PermissionDefinition(PermissionCode,CategoryName,DisplayName,Description,DisplayOrder) VALUES(@code,@category,@name,@description,@order);", connection, transaction))
                {
                    command.Parameters.Add("@code", SqlDbType.NVarChar, 100).Value = permission.Code;
                    command.Parameters.Add("@category", SqlDbType.NVarChar, 100).Value = permission.Category;
                    command.Parameters.Add("@name", SqlDbType.NVarChar, 160).Value = permission.Name;
                    command.Parameters.Add("@description", SqlDbType.NVarChar, 400).Value = permission.Description;
                    command.Parameters.Add("@order", SqlDbType.Int).Value = permission.DisplayOrder;
                    command.ExecuteNonQuery();
                }
            }

            int administratorRoleId = EnsureRole(connection, transaction, "SystemAdministrator", "مدیر سامانه", "دسترسی کامل و غیرقابل‌حذف برای راه‌اندازی و اداره‌ی سامانه.", true, null);
            using (var command = new SqlCommand(@"
UPDATE app.ApplicationRole
SET DisplayName=N'مدیر سامانه',Description=N'دسترسی کامل و غیرقابل‌حذف برای راه‌اندازی و اداره‌ی سامانه.',IsSystem=1,IsActive=1
WHERE RoleId=@roleId", connection, transaction))
            {
                command.Parameters.Add("@roleId", SqlDbType.Int).Value = administratorRoleId;
                command.ExecuteNonQuery();
            }
            EnsureRole(connection, transaction, "StandardUser", "کاربر پایه", "نقش شروع با دسترسی محدود؛ دسترسی‌های لازم را متناسب با وظیفه‌ی کاربر اضافه کنید.", false, new[]
            {
                "dashboard.view", "reports.view", "reports.export",
                PermissionCatalog.TableCode("reference", "view"), PermissionCatalog.TableCode("device", "view")
            });

            bool administratorPermissionsChanged = false;
            foreach (string code in PermissionCatalog.Codes)
                administratorPermissionsChanged |= EnsureRolePermission(connection, transaction, administratorRoleId, code);

            if (administratorPermissionsChanged)
            {
                using (var command = new SqlCommand(@"
UPDATE u
SET AuthorizationVersion=AuthorizationVersion+1
FROM app.ApplicationUser u
WHERE EXISTS
 (SELECT 1 FROM app.ApplicationUserRole ur WHERE ur.ApplicationUserId=u.ApplicationUserId AND ur.RoleId=@roleId);", connection, transaction))
                {
                    command.Parameters.Add("@roleId", SqlDbType.Int).Value = administratorRoleId;
                    command.ExecuteNonQuery();
                }
            }

            // Preserve the current recharge role behavior while requiring the newly added
            // permission that exposes the terms used by the card recharge workflow.
            using (var command = new SqlCommand(@"
SELECT DISTINCT recharge.RoleId
FROM app.ApplicationRolePermission recharge
JOIN app.ApplicationRolePermission finance ON finance.RoleId=recharge.RoleId
WHERE recharge.PermissionCode=N'cards.recharge'
  AND finance.PermissionCode=@financeCreate
  AND (NOT EXISTS
       (SELECT 1 FROM app.ApplicationRolePermission terms WHERE terms.RoleId=recharge.RoleId AND terms.PermissionCode=@termsView)
       OR NOT EXISTS
       (SELECT 1 FROM app.ApplicationRolePermission events WHERE events.RoleId=recharge.RoleId AND events.PermissionCode=@eventsCreate));", connection, transaction))
            {
                command.Parameters.Add("@financeCreate", SqlDbType.NVarChar, 100).Value = PermissionCatalog.TableCode("finance", "create");
                command.Parameters.Add("@termsView", SqlDbType.NVarChar, 100).Value = "operations.recharge_terms.view";
                command.Parameters.Add("@eventsCreate", SqlDbType.NVarChar, 100).Value = "edge.events.create";
                var eligibleRoles = new List<int>();
                using (var reader = command.ExecuteReader())
                    while (reader.Read()) eligibleRoles.Add(reader.GetInt32(0));

                foreach (int roleId in eligibleRoles)
                {
                    bool permissionChanged = EnsureRolePermission(connection, transaction, roleId, "operations.recharge_terms.view");
                    permissionChanged |= EnsureRolePermission(connection, transaction, roleId, "edge.events.create");
                    if (!permissionChanged) continue;
                    using (var grantVersion = new SqlCommand(@"
UPDATE u SET AuthorizationVersion=AuthorizationVersion+1
FROM app.ApplicationUser u
WHERE EXISTS
 (SELECT 1 FROM app.ApplicationUserRole ur WHERE ur.ApplicationUserId=u.ApplicationUserId AND ur.RoleId=@roleId);", connection, transaction))
                    {
                        grantVersion.Parameters.Add("@roleId", SqlDbType.Int).Value = roleId;
                        grantVersion.ExecuteNonQuery();
                    }
                }
            }

            using (var command = new SqlCommand(@"
INSERT app.ApplicationUserRole(ApplicationUserId,RoleId)
SELECT u.ApplicationUserId,@roleId FROM app.ApplicationUser u
WHERE u.IsAdministrator=1 AND NOT EXISTS
 (SELECT 1 FROM app.ApplicationUserRole ur WHERE ur.ApplicationUserId=u.ApplicationUserId AND ur.RoleId=@roleId);
UPDATE app.ApplicationUser
SET IsAdministrator=0,AuthorizationVersion=AuthorizationVersion+1
WHERE IsAdministrator=1;", connection, transaction))
            {
                command.Parameters.Add("@roleId", SqlDbType.Int).Value = administratorRoleId;
                command.ExecuteNonQuery();
            }
        }

        private static void EnsureCardOperationObjects(SqlConnection connection, SqlTransaction transaction)
        {
            Execute(connection, transaction, @"
IF OBJECT_ID(N'app.CardOperationLog', N'U') IS NULL
BEGIN
 CREATE TABLE app.CardOperationLog
 (
  CardOperationLogId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_CardOperationLog PRIMARY KEY,
  ApplicationUserId int NOT NULL,
  OperationName nvarchar(40) NOT NULL,
  CardUid varchar(11) NULL,
  RequestedAmount bigint NULL,
  OperationStatus nvarchar(40) NOT NULL,
  Details nvarchar(1000) NULL,
  StartedAt datetime2(0) NOT NULL CONSTRAINT DF_CardOperationLog_StartedAt DEFAULT(SYSDATETIME()),
  CompletedAt datetime2(0) NULL
 );
 CREATE INDEX IX_CardOperationLog_StartedAt ON app.CardOperationLog(StartedAt DESC);
 CREATE INDEX IX_CardOperationLog_CardUid ON app.CardOperationLog(CardUid,StartedAt DESC);
END");
        }

        private static int EnsureRole(SqlConnection connection, SqlTransaction transaction, string code, string displayName, string description, bool system, IEnumerable<string> initialPermissions)
        {
            int roleId;
            bool created;
            using (var command = new SqlCommand(@"
DECLARE @created bit=0;
IF NOT EXISTS(SELECT 1 FROM app.ApplicationRole WHERE RoleCode=@code)
BEGIN
 INSERT app.ApplicationRole(RoleCode,DisplayName,Description,IsSystem,IsActive) VALUES(@code,@name,@description,@isSystem,1);
 SET @created=1;
END;
SELECT RoleId,@created FROM app.ApplicationRole WHERE RoleCode=@code;", connection, transaction))
            {
                command.Parameters.Add("@code", SqlDbType.NVarChar, 64).Value = code;
                command.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = displayName;
                command.Parameters.Add("@description", SqlDbType.NVarChar, 400).Value = description;
                command.Parameters.Add("@isSystem", SqlDbType.Bit).Value = system;
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read()) throw new InvalidOperationException("نقش پیش‌فرض ساخته نشد: " + code);
                    roleId = reader.GetInt32(0);
                    created = reader.GetBoolean(1);
                }
            }
            if (created && initialPermissions != null)
                foreach (string permissionCode in initialPermissions) EnsureRolePermission(connection, transaction, roleId, permissionCode);
            return roleId;
        }

        private static bool EnsureRolePermission(SqlConnection connection, SqlTransaction transaction, int roleId, string permissionCode)
        {
            using (var command = new SqlCommand(@"
IF NOT EXISTS(SELECT 1 FROM app.ApplicationRolePermission WHERE RoleId=@roleId AND PermissionCode=@code)
BEGIN
 INSERT app.ApplicationRolePermission(RoleId,PermissionCode) VALUES(@roleId,@code);
 SELECT 1;
END
ELSE SELECT 0;", connection, transaction))
            {
                command.Parameters.Add("@roleId", SqlDbType.Int).Value = roleId;
                command.Parameters.Add("@code", SqlDbType.NVarChar, 100).Value = permissionCode;
                return Convert.ToInt32(command.ExecuteScalar()) != 0;
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
 DECLARE @userId int;
 DECLARE @roleId int=(SELECT RoleId FROM app.ApplicationRole WHERE RoleCode=N'SystemAdministrator');
 INSERT app.ApplicationUser(UserName,PasswordSalt,PasswordHash,PasswordHashIterations,IsAdministrator,IsActive,MustChangePassword)
 VALUES(N'Admin',@salt,@hash,@iterations,0,1,1);
 SET @userId=CONVERT(int,SCOPE_IDENTITY());
 INSERT app.ApplicationUserRole(ApplicationUserId,RoleId) VALUES(@userId,@roleId);
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

        private static string GenerateInitialAdminPassword() { return PasswordHasher.GenerateTemporaryPassword(); }

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

        private static bool HasVersion(SqlConnection connection, SqlTransaction transaction, int version)
        {
            using (var command = new SqlCommand("SELECT CASE WHEN EXISTS(SELECT 1 FROM app.SchemaMigrationHistory WHERE MigrationVersion=@version) THEN 1 ELSE 0 END", connection, transaction))
            {
                command.Parameters.Add("@version", SqlDbType.Int).Value = version;
                return Convert.ToInt32(command.ExecuteScalar()) == 1;
            }
        }

        private static void RewriteOperationalCatalogReferences(SqlConnection connection, SqlTransaction transaction)
        {
            var modules = new List<Tuple<string, string>>();
            using (var command = new SqlCommand(@"
SELECT QUOTENAME(OBJECT_SCHEMA_NAME(o.object_id))+N'.'+QUOTENAME(o.name),m.definition
FROM sys.sql_modules m
JOIN sys.objects o ON o.object_id=m.object_id
WHERE m.definition LIKE N'%FuntinoParkOperations.%'
   OR m.definition LIKE N'%FuntinoDeviceOperations.%'
   OR m.definition LIKE N'%FuntinoParkOperationsNew.%'
   OR m.definition LIKE N'%FuntinoDeviceOperationsNew.%'
   OR CHARINDEX(N'[FuntinoParkOperations].',m.definition)>0
   OR CHARINDEX(N'[FuntinoDeviceOperations].',m.definition)>0
   OR CHARINDEX(N'[FuntinoParkOperationsNew].',m.definition)>0
   OR CHARINDEX(N'[FuntinoDeviceOperationsNew].',m.definition)>0;", connection, transaction))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                    if (!reader.IsDBNull(1)) modules.Add(Tuple.Create(reader.GetString(0), reader.GetString(1)));
            }

            foreach (Tuple<string, string> module in modules)
            {
                string definition = module.Item2
                    .Replace("[FuntinoParkOperationsNew].", "[" + DatabaseManager.MainCatalog + "].")
                    .Replace("[FuntinoDeviceOperationsNew].", "[" + DatabaseManager.DeviceCatalog + "].")
                    .Replace("FuntinoParkOperationsNew.", DatabaseManager.MainCatalog + ".")
                    .Replace("FuntinoDeviceOperationsNew.", DatabaseManager.DeviceCatalog + ".")
                    .Replace("[FuntinoParkOperations].", "[" + DatabaseManager.MainCatalog + "].")
                    .Replace("[FuntinoDeviceOperations].", "[" + DatabaseManager.DeviceCatalog + "].")
                    .Replace("FuntinoParkOperations.", DatabaseManager.MainCatalog + ".")
                    .Replace("FuntinoDeviceOperations.", DatabaseManager.DeviceCatalog + ".");
                if (String.Equals(definition, module.Item2, StringComparison.Ordinal)) continue;

                Match create = Regex.Match(definition, @"^\s*CREATE\s+(?=(PROC(?:EDURE)?|FUNCTION|VIEW|TRIGGER)\b)", RegexOptions.IgnoreCase);
                if (create.Success)
                    definition = definition.Substring(0, create.Index) + "ALTER " + definition.Substring(create.Index + create.Length);
                else if (!Regex.IsMatch(definition, @"^\s*ALTER\s+(PROC(?:EDURE)?|FUNCTION|VIEW|TRIGGER)\b", RegexOptions.IgnoreCase))
                    throw new InvalidOperationException("ارجاع پایگاه در شیء «" + module.Item1 + "» قابل به‌روزرسانی امن نبود؛ مهاجرت لغو شد.");

                using (var alter = new SqlCommand(definition, connection, transaction))
                {
                    alter.CommandTimeout = 180;
                    alter.ExecuteNonQuery();
                }
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
