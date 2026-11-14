using System;
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
                int applicationUserId, iterations; bool isAdministrator, mustChange; byte[] salt, hash;
                using (var command = new SqlCommand("SELECT ApplicationUserId,PasswordSalt,PasswordHash,PasswordHashIterations,IsAdministrator,MustChangePassword FROM app.ApplicationUser WHERE UserName=@userName AND IsActive=1", connection))
                {
                    command.Parameters.Add("@userName", SqlDbType.NVarChar, 64).Value = (username ?? "").Trim();
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read()) return null;
                        applicationUserId = reader.GetInt32(0); salt = (byte[])reader[1]; hash = (byte[])reader[2]; iterations = reader.GetInt32(3);
                        isAdministrator = reader.GetBoolean(4); mustChange = reader.GetBoolean(5);
                    }
                }
                if (!PasswordHasher.Verify(password, salt, hash, iterations)) return null;
                using (var command = new SqlCommand("UPDATE app.ApplicationUser SET LastLoginAt=SYSDATETIME() WHERE ApplicationUserId=@applicationUserId", connection))
                {
                    command.Parameters.Add("@applicationUserId", SqlDbType.Int).Value = applicationUserId; command.ExecuteNonQuery();
                }
                var session = new AppSession(applicationUserId, username.Trim(), isAdministrator, mustChange);
                Services.AuditService.Write("login", "ApplicationUser", applicationUserId.ToString(), "ورود موفق");
                return session;
            }
        }

        public static DataTable GetUsers()
        {
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                return DatabaseManager.Query(connection, "SELECT ApplicationUserId,UserName,IsAdministrator,IsActive,MustChangePassword,CreatedAt,LastLoginAt FROM app.ApplicationUser ORDER BY UserName");
            }
        }
    }
}
