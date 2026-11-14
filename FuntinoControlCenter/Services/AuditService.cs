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
                using (var command = new SqlCommand("INSERT app.AuditEvent(ApplicationUserId,ActionName,EntityName,EntityKey,Details) VALUES(@applicationUserId,@actionName,@entityName,@entityKey,@details)", connection))
                {
                    command.Parameters.Add("@applicationUserId", SqlDbType.Int).Value = session == null ? (object)DBNull.Value : session.ApplicationUserId;
                    command.Parameters.Add("@actionName", SqlDbType.NVarChar, 80).Value = action ?? "action";
                    command.Parameters.Add("@entityName", SqlDbType.NVarChar, 128).Value = (object)entity ?? DBNull.Value;
                    command.Parameters.Add("@entityKey", SqlDbType.NVarChar, 256).Value = (object)key ?? DBNull.Value;
                    command.Parameters.Add("@details", SqlDbType.NVarChar, 1000).Value = (object)details ?? DBNull.Value;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
