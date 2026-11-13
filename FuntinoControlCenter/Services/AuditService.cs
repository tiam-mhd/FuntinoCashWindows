using System;
using System.Data;
using System.Data.SqlClient;
using FuntinoControlCenter.Database;

namespace FuntinoControlCenter.Services
{
    internal static class AuditService
    {
        public static void Write(string action, string entity, string key, string details)
        {
            var session = AppSession.Current;
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var command = new SqlCommand("INSERT dbo.AppAuditLog(UserId,Action,EntityName,EntityKey,Details) VALUES(@uid,@a,@e,@k,@d)", connection))
                {
                    command.Parameters.Add("@uid", SqlDbType.Int).Value = session == null ? (object)DBNull.Value : session.UserId;
                    command.Parameters.Add("@a", SqlDbType.NVarChar, 80).Value = (object)(action ?? "action") ?? DBNull.Value;
                    command.Parameters.Add("@e", SqlDbType.NVarChar, 128).Value = (object)entity ?? DBNull.Value;
                    command.Parameters.Add("@k", SqlDbType.NVarChar, 256).Value = (object)key ?? DBNull.Value;
                    command.Parameters.Add("@d", SqlDbType.NVarChar, 1000).Value = (object)details ?? DBNull.Value;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
