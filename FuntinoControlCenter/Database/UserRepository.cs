using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using FuntinoControlCenter.Security;

namespace FuntinoControlCenter.Database
{
    internal static class UserRepository
    {
        public static AppSession Authenticate(string username, string password)
        {
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                int applicationUserId, iterations; bool mustChange; byte[] salt, hash;
                using (var command = new SqlCommand("SELECT ApplicationUserId,PasswordSalt,PasswordHash,PasswordHashIterations,MustChangePassword FROM app.ApplicationUser WHERE UserName=@userName AND IsActive=1", connection))
                {
                    command.Parameters.Add("@userName", SqlDbType.NVarChar, 64).Value = (username ?? "").Trim();
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read()) return null;
                        applicationUserId = reader.GetInt32(0); salt = (byte[])reader[1]; hash = (byte[])reader[2]; iterations = reader.GetInt32(3);
                        mustChange = reader.GetBoolean(4);
                    }
                }
                if (!PasswordHasher.Verify(password, salt, hash, iterations)) return null;
                using (var command = new SqlCommand("UPDATE app.ApplicationUser SET LastLoginAt=SYSDATETIME() WHERE ApplicationUserId=@applicationUserId", connection))
                {
                    command.Parameters.Add("@applicationUserId", SqlDbType.Int).Value = applicationUserId; command.ExecuteNonQuery();
                }
                List<string> permissionCodes, roleCodes;
                int loadedVersion;
                AccessControlRepository.LoadAuthorization(connection, applicationUserId, out loadedVersion, out permissionCodes, out roleCodes);
                var session = new AppSession(applicationUserId, username.Trim(), mustChange, loadedVersion, permissionCodes, roleCodes);
                Services.AuditService.Write("login", "ApplicationUser", applicationUserId.ToString(), "ورود موفق");
                return session;
            }
        }

        public static DataTable GetUsers()
        {
            return AccessControlRepository.GetUsers();
        }
    }
}
