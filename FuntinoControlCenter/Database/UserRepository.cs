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
                int id, iterations; bool admin, mustChange; byte[] salt, hash;
                using (var command = new SqlCommand("SELECT UserId,PasswordSalt,PasswordHash,Iterations,IsAdmin,MustChangePassword FROM dbo.AppUsers WHERE Username=@u AND IsActive=1", connection))
                {
                    command.Parameters.Add("@u", SqlDbType.NVarChar, 64).Value = (username ?? "").Trim();
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read()) return null;
                        id = reader.GetInt32(0); salt = (byte[])reader[1]; hash = (byte[])reader[2]; iterations = reader.GetInt32(3);
                        admin = reader.GetBoolean(4); mustChange = reader.GetBoolean(5);
                    }
                }
                if (!PasswordHasher.Verify(password, salt, hash, iterations)) return null;
                using (var command = new SqlCommand("UPDATE dbo.AppUsers SET LastLoginAt=SYSDATETIME() WHERE UserId=@id", connection))
                {
                    command.Parameters.Add("@id", SqlDbType.Int).Value = id; command.ExecuteNonQuery();
                }
                var session = new AppSession(id, username.Trim(), admin, mustChange);
                Services.AuditService.Write("login", "AppUsers", id.ToString(), "ورود موفق");
                return session;
            }
        }

        public static DataTable GetUsers()
        {
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                return DatabaseManager.Query(connection, "SELECT UserId,Username,IsAdmin,IsActive,MustChangePassword,CreatedAt,LastLoginAt FROM dbo.AppUsers ORDER BY Username");
            }
        }
    }
}
