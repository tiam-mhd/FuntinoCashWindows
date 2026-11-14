using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FuntinoControlCenter.Database;

namespace FuntinoControlCenter.Services
{
    internal static class DeviceDiagnostics
    {
        public static int CountBusinessTables()
        {
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open(); return Convert.ToInt32(DatabaseManager.Scalar(connection, "SELECT COUNT(1) FROM sys.tables t INNER JOIN sys.schemas s ON s.schema_id=t.schema_id WHERE t.is_ms_shipped=0 AND s.name<>N'app'"));
            }
        }

        public static long CountRows(string fullName)
        {
            var parts = fullName.Split('.');
            if (parts.Length != 2 || parts[0].Length == 0 || parts[1].Length == 0 || String.Equals(parts[0], "app", StringComparison.OrdinalIgnoreCase)) return 0;
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT COUNT_BIG(1) FROM " + Quote(parts[0]) + "." + Quote(parts[1]), connection)) return Convert.ToInt64(command.ExecuteScalar());
            }
        }

        public static string GetMainSummary()
        {
            try
            {
                var tables = TableRepository.GetTableNames();
                string[] focus = { "finance.RechargeTransaction", "payments.PointOfSaleTransaction", "finance.CreditRechargeTransaction", "ticket.TicketTransaction", "reference.AttractionDevice" };
                var items = new System.Collections.Generic.List<string>();
                foreach (string table in focus) if (tables.Contains(table, StringComparer.OrdinalIgnoreCase)) items.Add(table.Substring(table.IndexOf('.') + 1) + ": " + CountRows(table).ToString("N0"));
                return items.Count == 0 ? "دیتابیس متصل است؛ برای مشاهده‌ی داده‌ها از بخش جداول استفاده کنید." : String.Join("     •     ", items);
            }
            catch (Exception ex) { return DatabaseManager.SafeMessage(ex); }
        }

        private static string Quote(string value) { return "[" + value.Replace("]", "]]" ) + "]"; }
    }
}
