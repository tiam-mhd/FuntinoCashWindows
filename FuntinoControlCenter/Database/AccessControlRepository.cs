using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FuntinoControlCenter.Security;
using FuntinoControlCenter.Services;

namespace FuntinoControlCenter.Database
{
    internal sealed class GeneratedCredential
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    internal static class AccessControlRepository
    {
        public static void LoadAuthorization(SqlConnection connection, int userId, out int version, out List<string> permissions, out List<string> roles)
        {
            version = 0;
            permissions = new List<string>();
            roles = new List<string>();
            const string sql = @"
SELECT DISTINCT u.AuthorizationVersion,r.RoleCode,p.PermissionCode
FROM app.ApplicationUser u
LEFT JOIN app.ApplicationUserRole ur ON ur.ApplicationUserId=u.ApplicationUserId
LEFT JOIN app.ApplicationRole r ON r.RoleId=ur.RoleId AND r.IsActive=1
LEFT JOIN app.ApplicationRolePermission rp ON rp.RoleId=r.RoleId
LEFT JOIN app.PermissionDefinition p ON p.PermissionCode=rp.PermissionCode
WHERE u.ApplicationUserId=@userId AND u.IsActive=1";
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read()) throw new UnauthorizedAccessException("این حساب غیرفعال شده است یا دیگر وجود ندارد.");
                    version = reader.GetInt32(0);
                    do
                    {
                        if (!reader.IsDBNull(1) && !roles.Contains(reader.GetString(1), StringComparer.OrdinalIgnoreCase))
                            roles.Add(reader.GetString(1));
                        if (!reader.IsDBNull(2) && !permissions.Contains(reader.GetString(2), StringComparer.OrdinalIgnoreCase))
                            permissions.Add(reader.GetString(2));
                    } while (reader.Read());
                }
            }
        }

        public static void RefreshSession(AppSession session)
        {
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                int version;
                bool active;
                bool mustChange;
                using (var command = new SqlCommand("SELECT AuthorizationVersion,IsActive,MustChangePassword FROM app.ApplicationUser WHERE ApplicationUserId=@userId", connection))
                {
                    command.Parameters.Add("@userId", SqlDbType.Int).Value = session.ApplicationUserId;
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            AppSession.Clear();
                            throw new UnauthorizedAccessException("حساب کاربری دیگر در دسترس نیست.");
                        }
                        version = reader.GetInt32(0);
                        active = reader.GetBoolean(1);
                        mustChange = reader.GetBoolean(2);
                    }
                }
                if (!active)
                {
                    AppSession.Clear();
                    throw new UnauthorizedAccessException("این حساب غیرفعال شده است؛ نشست جاری پایان یافت.");
                }
                if (mustChange && !session.MustChangePassword)
                {
                    AppSession.Clear();
                    throw new UnauthorizedAccessException("گذرواژه‌ی موقت برای این حساب صادر شده است؛ دوباره وارد شوید و آن را تغییر دهید.");
                }
                if (mustChange && version != session.AuthorizationVersion)
                {
                    AppSession.Clear();
                    throw new UnauthorizedAccessException("گذرواژه‌ی حساب تغییر کرده است؛ دوباره با گذرواژه‌ی تازه وارد شوید.");
                }
                if (!mustChange && session.MustChangePassword) session.SetMustChangePassword(false);
                if (version != session.AuthorizationVersion)
                {
                    List<string> permissions, roles;
                    int currentVersion;
                    LoadAuthorization(connection, session.ApplicationUserId, out currentVersion, out permissions, out roles);
                    session.ReplaceAuthorization(currentVersion, permissions, roles);
                }
            }
        }

        public static DataTable GetUsers()
        {
            AuthorizationGuard.RequireAny("users.view", "users.update", "users.reset_password", "users.assign_roles", "users.deactivate");
            var columns = new List<string> { "u.ApplicationUserId", "u.UserName", "u.IsActive" };
            bool canView = AuthorizationGuard.Has("users.view");
            if (canView || AuthorizationGuard.Has("users.update"))
                columns.Add("COALESCE(NULLIF(u.DisplayName,N''),u.UserName) AS DisplayName");
            if (canView || AuthorizationGuard.Has("users.reset_password"))
                columns.Add("u.MustChangePassword");
            if (canView)
            {
                columns.Add("u.CreatedAt");
                columns.Add("u.LastLoginAt");
            }
            if (canView || AuthorizationGuard.Has("users.assign_roles"))
                columns.Add("STUFF((SELECT N'، ' + r.DisplayName FROM app.ApplicationUserRole ur JOIN app.ApplicationRole r ON r.RoleId=ur.RoleId WHERE ur.ApplicationUserId=u.ApplicationUserId AND r.IsActive=1 ORDER BY r.DisplayName FOR XML PATH(N''),TYPE).value(N'.',N'nvarchar(max)'),1,2,N'') AS RoleNames");
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                return DatabaseManager.Query(connection, "SELECT " + String.Join(",", columns) + " FROM app.ApplicationUser u ORDER BY u.UserName");
            }
        }

        public static DataTable GetRoles()
        {
            AuthorizationGuard.RequireAny("roles.view", "roles.update", "roles.assign_permissions", "roles.delete");
            var columns = new List<string> { "r.RoleId", "r.DisplayName", "r.IsSystem" };
            if (AuthorizationGuard.HasAny("roles.view", "roles.update")) columns.Add("r.Description");
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                return DatabaseManager.Query(connection, "SELECT " + String.Join(",", columns) + " FROM app.ApplicationRole r WHERE r.IsActive=1 ORDER BY r.IsSystem DESC,r.DisplayName");
            }
        }

        public static DataTable GetAssignableRoles()
        {
            AuthorizationGuard.Require("users.assign_roles");
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                return DatabaseManager.Query(connection, @"
SELECT r.RoleId,r.DisplayName
FROM app.ApplicationRole r
WHERE r.IsSystem=0 AND r.IsActive=1
AND NOT EXISTS
(
 SELECT 1 FROM app.ApplicationRolePermission candidate
 WHERE candidate.RoleId=r.RoleId
 AND NOT EXISTS
 (
  SELECT 1 FROM app.ApplicationUserRole ur
  JOIN app.ApplicationRole assignedRole ON assignedRole.RoleId=ur.RoleId AND assignedRole.IsActive=1
  JOIN app.ApplicationRolePermission owned ON owned.RoleId=assignedRole.RoleId
  WHERE ur.ApplicationUserId=@currentUserId AND owned.PermissionCode=candidate.PermissionCode
 )
)
ORDER BY r.DisplayName",
                    new SqlParameter("@currentUserId", SqlDbType.Int) { Value = AppSession.Current.ApplicationUserId });
            }
        }

        public static List<int> GetUserRoleIds(int userId)
        {
            AuthorizationGuard.Require("users.assign_roles");
            var result = new List<int>();
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT ur.RoleId FROM app.ApplicationUserRole ur JOIN app.ApplicationRole r ON r.RoleId=ur.RoleId WHERE ur.ApplicationUserId=@userId AND r.IsSystem=0 AND r.IsActive=1", connection))
                {
                    command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                    using (var reader = command.ExecuteReader()) while (reader.Read()) result.Add(reader.GetInt32(0));
                }
            }
            return result;
        }

        public static List<string> GetRolePermissionCodes(int roleId)
        {
            AuthorizationGuard.RequireAny("roles.view", "roles.assign_permissions");
            var result = new List<string>();
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT rp.PermissionCode FROM app.ApplicationRolePermission rp JOIN app.ApplicationRole r ON r.RoleId=rp.RoleId WHERE rp.RoleId=@roleId AND r.IsActive=1", connection))
                {
                    command.Parameters.Add("@roleId", SqlDbType.Int).Value = roleId;
                    using (var reader = command.ExecuteReader()) while (reader.Read()) result.Add(reader.GetString(0));
                }
            }
            return result;
        }

        public static GeneratedCredential CreateUser(string userName, string displayName, IEnumerable<int> roleIds)
        {
            AuthorizationGuard.Require("users.create");
            string normalizedName = NormalizeUserName(userName);
            var selectedRoles = (roleIds ?? Enumerable.Empty<int>()).Distinct().ToList();
            if (selectedRoles.Count > 0) AuthorizationGuard.Require("users.assign_roles");

            string password = PasswordHasher.GenerateTemporaryPassword();
            byte[] salt, hash;
            PasswordHasher.Create(password, out salt, out hash);
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        ValidateAssignableRoles(connection, transaction, selectedRoles);
                        int userId;
                        using (var command = new SqlCommand(@"
INSERT app.ApplicationUser(UserName,DisplayName,PasswordSalt,PasswordHash,PasswordHashIterations,IsAdministrator,IsActive,MustChangePassword)
OUTPUT INSERTED.ApplicationUserId
VALUES(@userName,@displayName,@salt,@hash,@iterations,0,1,1)", connection, transaction))
                        {
                            command.Parameters.Add("@userName", SqlDbType.NVarChar, 64).Value = normalizedName;
                            command.Parameters.Add("@displayName", SqlDbType.NVarChar, 100).Value = String.IsNullOrWhiteSpace(displayName) ? (object)DBNull.Value : displayName.Trim();
                            command.Parameters.Add("@salt", SqlDbType.VarBinary, 32).Value = salt;
                            command.Parameters.Add("@hash", SqlDbType.VarBinary, 32).Value = hash;
                            command.Parameters.Add("@iterations", SqlDbType.Int).Value = PasswordHasher.Iterations;
                            userId = Convert.ToInt32(command.ExecuteScalar());
                        }
                        AssignRoles(connection, transaction, userId, selectedRoles, false);
                        WriteAudit(connection, transaction, "user-create", "ApplicationUser", userId.ToString(), "ساخت حساب " + normalizedName);
                        transaction.Commit();
                        return new GeneratedCredential { UserId = userId, UserName = normalizedName, Password = password };
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public static void UpdateUserDisplayName(int userId, string displayName)
        {
            AuthorizationGuard.Require("users.update");
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand("UPDATE app.ApplicationUser SET DisplayName=@displayName WHERE ApplicationUserId=@userId", connection, transaction))
                        {
                            command.Parameters.Add("@displayName", SqlDbType.NVarChar, 100).Value = String.IsNullOrWhiteSpace(displayName) ? (object)DBNull.Value : displayName.Trim();
                            command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                            if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("کاربر انتخاب‌شده پیدا نشد.");
                        }
                        WriteAudit(connection, transaction, "user-update", "ApplicationUser", userId.ToString(), "تغییر نام نمایشی کاربر");
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

        public static void SetUserRoles(int userId, IEnumerable<int> roleIds)
        {
            AuthorizationGuard.Require("users.assign_roles");
            var selectedRoles = (roleIds ?? Enumerable.Empty<int>()).Distinct().ToList();
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        ValidateAssignableRoles(connection, transaction, selectedRoles);
                        AssignRoles(connection, transaction, userId, selectedRoles, true);
                        using (var command = new SqlCommand("UPDATE app.ApplicationUser SET AuthorizationVersion=AuthorizationVersion+1 WHERE ApplicationUserId=@userId", connection, transaction))
                        {
                            command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                            if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("کاربر انتخاب‌شده پیدا نشد.");
                        }
                        WriteAudit(connection, transaction, "user-roles", "ApplicationUser", userId.ToString(), "تغییر نقش‌های کاربر");
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

        public static string ResetPassword(int userId)
        {
            AuthorizationGuard.Require("users.reset_password");
            if (AppSession.Current != null && AppSession.Current.ApplicationUserId == userId)
                throw new InvalidOperationException("برای تغییر گذرواژه‌ی خودتان از بخش «تغییر گذرواژه» استفاده کنید.");
            string password = PasswordHasher.GenerateTemporaryPassword();
            byte[] salt, hash;
            PasswordHasher.Create(password, out salt, out hash);
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand(@"
UPDATE app.ApplicationUser
SET PasswordSalt=@salt,PasswordHash=@hash,PasswordHashIterations=@iterations,
    MustChangePassword=1,AuthorizationVersion=AuthorizationVersion+1
WHERE ApplicationUserId=@userId AND IsActive=1", connection, transaction))
                        {
                            command.Parameters.Add("@salt", SqlDbType.VarBinary, 32).Value = salt;
                            command.Parameters.Add("@hash", SqlDbType.VarBinary, 32).Value = hash;
                            command.Parameters.Add("@iterations", SqlDbType.Int).Value = PasswordHasher.Iterations;
                            command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                            if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("کاربر فعال انتخاب‌شده پیدا نشد.");
                        }
                        WriteAudit(connection, transaction, "user-password-reset", "ApplicationUser", userId.ToString(), "صدور گذرواژه‌ی موقت");
                        transaction.Commit();
                        return password;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public static void SetUserActive(int userId, bool active)
        {
            AuthorizationGuard.Require("users.deactivate");
            if (AppSession.Current != null && AppSession.Current.ApplicationUserId == userId && !active)
                throw new InvalidOperationException("امکان غیرفعال‌کردن حسابی که اکنون با آن وارد شده‌اید وجود ندارد.");
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        if (!active)
                        {
                            using (var command = new SqlCommand(@"
IF EXISTS(
 SELECT 1 FROM app.ApplicationUser u
 JOIN app.ApplicationUserRole ur ON ur.ApplicationUserId=u.ApplicationUserId
 JOIN app.ApplicationRole r ON r.RoleId=ur.RoleId
 WHERE u.ApplicationUserId=@userId AND r.RoleCode=N'SystemAdministrator' AND u.IsActive=1
)
AND (SELECT COUNT(1) FROM app.ApplicationUser u
     JOIN app.ApplicationUserRole ur ON ur.ApplicationUserId=u.ApplicationUserId
     JOIN app.ApplicationRole r ON r.RoleId=ur.RoleId
     WHERE r.RoleCode=N'SystemAdministrator' AND u.IsActive=1)>1
 SELECT CAST(1 AS int) ELSE SELECT CAST(0 AS int)", connection, transaction))
                            {
                                command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                                int isOneOfManyAdmins = Convert.ToInt32(command.ExecuteScalar());
                                using (var verify = new SqlCommand("SELECT COUNT(1) FROM app.ApplicationUser u JOIN app.ApplicationUserRole ur ON ur.ApplicationUserId=u.ApplicationUserId JOIN app.ApplicationRole r ON r.RoleId=ur.RoleId WHERE u.ApplicationUserId=@userId AND r.RoleCode=N'SystemAdministrator' AND u.IsActive=1", connection, transaction))
                                {
                                    verify.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                                    bool isSystemAdmin = Convert.ToInt32(verify.ExecuteScalar()) > 0;
                                    if (isSystemAdmin && isOneOfManyAdmins == 0)
                                        throw new InvalidOperationException("آخرین مدیر سامانه را نمی‌توان غیرفعال کرد.");
                                }
                            }
                        }
                        using (var command = new SqlCommand("UPDATE app.ApplicationUser SET IsActive=@active,AuthorizationVersion=AuthorizationVersion+1 WHERE ApplicationUserId=@userId", connection, transaction))
                        {
                            command.Parameters.Add("@active", SqlDbType.Bit).Value = active;
                            command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                            if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("کاربر انتخاب‌شده پیدا نشد.");
                        }
                        WriteAudit(connection, transaction, active ? "user-activate" : "user-deactivate", "ApplicationUser", userId.ToString(), active ? "فعال‌سازی حساب" : "غیرفعال‌سازی حساب");
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

        public static int CreateRole(string displayName, string description)
        {
            AuthorizationGuard.Require("roles.create");
            ValidateRoleName(displayName);
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        int id;
                        using (var command = new SqlCommand(@"
INSERT app.ApplicationRole(RoleCode,DisplayName,Description,IsSystem,IsActive)
OUTPUT INSERTED.RoleId
VALUES(@roleCode,@displayName,@description,0,1)", connection, transaction))
                        {
                            command.Parameters.Add("@roleCode", SqlDbType.NVarChar, 64).Value = "Custom-" + Guid.NewGuid().ToString("N");
                            command.Parameters.Add("@displayName", SqlDbType.NVarChar, 100).Value = displayName.Trim();
                            command.Parameters.Add("@description", SqlDbType.NVarChar, 400).Value = String.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description.Trim();
                            id = Convert.ToInt32(command.ExecuteScalar());
                        }
                        WriteAudit(connection, transaction, "role-create", "ApplicationRole", id.ToString(), "ساخت نقش " + displayName.Trim());
                        transaction.Commit();
                        return id;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public static void UpdateRoleMetadata(int roleId, string displayName, string description)
        {
            AuthorizationGuard.Require("roles.update");
            ValidateRoleName(displayName);
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand("UPDATE app.ApplicationRole SET DisplayName=@displayName,Description=@description WHERE RoleId=@roleId AND IsSystem=0", connection, transaction))
                        {
                            command.Parameters.Add("@displayName", SqlDbType.NVarChar, 100).Value = displayName.Trim();
                            command.Parameters.Add("@description", SqlDbType.NVarChar, 400).Value = String.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description.Trim();
                            command.Parameters.Add("@roleId", SqlDbType.Int).Value = roleId;
                            if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("این نقش سیستمی است یا دیگر وجود ندارد.");
                        }
                        WriteAudit(connection, transaction, "role-update", "ApplicationRole", roleId.ToString(), "ویرایش مشخصات نقش " + displayName.Trim());
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

        public static void SetRolePermissions(int roleId, IEnumerable<string> permissionCodes)
        {
            AuthorizationGuard.Require("roles.assign_permissions");
            var selected = new HashSet<string>(permissionCodes ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var known = new HashSet<string>(PermissionCatalog.Codes, StringComparer.OrdinalIgnoreCase);
            if (selected.Any(code => !known.Contains(code))) throw new InvalidOperationException("درخواست شامل مجوز ناشناخته است.");
            if (selected.Any(code => !AppSession.Current.HasPermissionCached(code)))
                throw new UnauthorizedAccessException("نمی‌توانید مجوزی را به نقش بدهید که خودتان ندارید.");
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        EnsureCustomRole(connection, transaction, roleId);
                        using (var command = new SqlCommand("DELETE FROM app.ApplicationRolePermission WHERE RoleId=@roleId", connection, transaction))
                        {
                            command.Parameters.Add("@roleId", SqlDbType.Int).Value = roleId;
                            command.ExecuteNonQuery();
                        }
                        foreach (string code in selected)
                        {
                            using (var command = new SqlCommand("INSERT app.ApplicationRolePermission(RoleId,PermissionCode) VALUES(@roleId,@code)", connection, transaction))
                            {
                                command.Parameters.Add("@roleId", SqlDbType.Int).Value = roleId;
                                command.Parameters.Add("@code", SqlDbType.NVarChar, 100).Value = code;
                                command.ExecuteNonQuery();
                            }
                        }
                        using (var command = new SqlCommand(@"
UPDATE u SET AuthorizationVersion=AuthorizationVersion+1
FROM app.ApplicationUser u JOIN app.ApplicationUserRole ur ON ur.ApplicationUserId=u.ApplicationUserId
WHERE ur.RoleId=@roleId", connection, transaction))
                        {
                            command.Parameters.Add("@roleId", SqlDbType.Int).Value = roleId;
                            command.ExecuteNonQuery();
                        }
                        WriteAudit(connection, transaction, "role-permissions", "ApplicationRole", roleId.ToString(), "تغییر مجوزهای نقش");
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

        public static void DeleteRole(int roleId)
        {
            AuthorizationGuard.Require("roles.delete");
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        EnsureCustomRole(connection, transaction, roleId);
                        using (var check = new SqlCommand("SELECT COUNT(1) FROM app.ApplicationUserRole WHERE RoleId=@roleId", connection, transaction))
                        {
                            check.Parameters.Add("@roleId", SqlDbType.Int).Value = roleId;
                            if (Convert.ToInt32(check.ExecuteScalar()) > 0) throw new InvalidOperationException("این نقش به کاربر تخصیص دارد؛ ابتدا نقش را از حساب‌ها بردارید.");
                        }
                        using (var command = new SqlCommand("DELETE FROM app.ApplicationRolePermission WHERE RoleId=@roleId; DELETE FROM app.ApplicationRole WHERE RoleId=@roleId AND IsSystem=0", connection, transaction))
                        {
                            command.Parameters.Add("@roleId", SqlDbType.Int).Value = roleId;
                            command.ExecuteNonQuery();
                        }
                        WriteAudit(connection, transaction, "role-delete", "ApplicationRole", roleId.ToString(), "حذف نقش");
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

        private static void ValidateAssignableRoles(SqlConnection connection, SqlTransaction transaction, IList<int> roleIds)
        {
            if (roleIds.Count == 0) return;
            string sql = @"
SELECT COUNT(1)
FROM app.ApplicationRole r
WHERE r.IsSystem=0 AND r.IsActive=1 AND r.RoleId IN (" + String.Join(",", roleIds.Select((id, i) => "@role" + i)) + @")
AND NOT EXISTS
(
 SELECT 1 FROM app.ApplicationRolePermission candidate
 WHERE candidate.RoleId=r.RoleId
 AND NOT EXISTS
 (
  SELECT 1 FROM app.ApplicationUserRole ur
  JOIN app.ApplicationRole assignedRole ON assignedRole.RoleId=ur.RoleId AND assignedRole.IsActive=1
  JOIN app.ApplicationRolePermission owned ON owned.RoleId=assignedRole.RoleId
  WHERE ur.ApplicationUserId=@currentUserId AND owned.PermissionCode=candidate.PermissionCode
 )
)";
            using (var command = new SqlCommand(sql, connection, transaction))
            {
                for (int i = 0; i < roleIds.Count; i++) command.Parameters.Add("@role" + i, SqlDbType.Int).Value = roleIds[i];
                command.Parameters.Add("@currentUserId", SqlDbType.Int).Value = AppSession.Current.ApplicationUserId;
                if (Convert.ToInt32(command.ExecuteScalar()) != roleIds.Count)
                    throw new InvalidOperationException("یکی از نقش‌های انتخاب‌شده وجود ندارد یا مجوزهای آن از دسترسی خودتان بیشتر است.");
            }
        }

        private static void AssignRoles(SqlConnection connection, SqlTransaction transaction, int userId, IList<int> roleIds, bool replace)
        {
            if (replace)
            {
                using (var command = new SqlCommand(@"
DELETE ur FROM app.ApplicationUserRole ur JOIN app.ApplicationRole r ON r.RoleId=ur.RoleId
WHERE ur.ApplicationUserId=@userId AND r.IsSystem=0", connection, transaction))
                {
                    command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                    command.ExecuteNonQuery();
                }
            }
            foreach (int roleId in roleIds)
            {
                using (var command = new SqlCommand("IF NOT EXISTS(SELECT 1 FROM app.ApplicationUserRole WHERE ApplicationUserId=@userId AND RoleId=@roleId) INSERT app.ApplicationUserRole(ApplicationUserId,RoleId) VALUES(@userId,@roleId)", connection, transaction))
                {
                    command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                    command.Parameters.Add("@roleId", SqlDbType.Int).Value = roleId;
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void EnsureCustomRole(SqlConnection connection, SqlTransaction transaction, int roleId)
        {
            using (var command = new SqlCommand("SELECT COUNT(1) FROM app.ApplicationRole WHERE RoleId=@roleId AND IsSystem=0", connection, transaction))
            {
                command.Parameters.Add("@roleId", SqlDbType.Int).Value = roleId;
                if (Convert.ToInt32(command.ExecuteScalar()) != 1) throw new InvalidOperationException("این نقش سیستمی است یا دیگر وجود ندارد.");
            }
        }

        private static void WriteAudit(SqlConnection connection, SqlTransaction transaction, string action, string entity, string key, string details)
        {
            using (var command = new SqlCommand("INSERT app.AuditEvent(ApplicationUserId,ActionName,EntityName,EntityKey,Details) VALUES(@userId,@action,@entity,@key,@details)", connection, transaction))
            {
                command.Parameters.Add("@userId", SqlDbType.Int).Value = AppSession.Current == null ? (object)DBNull.Value : AppSession.Current.ApplicationUserId;
                command.Parameters.Add("@action", SqlDbType.NVarChar, 80).Value = action;
                command.Parameters.Add("@entity", SqlDbType.NVarChar, 128).Value = entity;
                command.Parameters.Add("@key", SqlDbType.NVarChar, 256).Value = key;
                command.Parameters.Add("@details", SqlDbType.NVarChar, 1000).Value = details;
                command.ExecuteNonQuery();
            }
        }

        private static string NormalizeUserName(string userName)
        {
            string value = (userName ?? "").Trim();
            if (value.Length < 3 || value.Length > 64 || value.Any(Char.IsControl))
                throw new InvalidOperationException("نام کاربری باید بین ۳ تا ۶۴ نویسه و بدون نویسه‌ی کنترلی باشد.");
            return value;
        }

        private static void ValidateRoleName(string name)
        {
            string value = (name ?? "").Trim();
            if (value.Length < 2 || value.Length > 100 || value.Any(Char.IsControl))
                throw new InvalidOperationException("نام نقش باید بین ۲ تا ۱۰۰ نویسه باشد.");
        }
    }
}
