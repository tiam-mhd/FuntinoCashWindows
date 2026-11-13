using System;
using System.Data;
using System.Data.SqlClient;
using FuntinoControlCenter.Database;

namespace FuntinoControlCenter.Services
{
    internal static class DeviceDiagnostics
    {
        public static int CountBusinessTables()
        {
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open(); return Convert.ToInt32(DatabaseManager.Scalar(connection, "SELECT COUNT(1) FROM sys.tables WHERE is_ms_shipped=0 AND name NOT LIKE N'App%'"));
            }
        }

        public static long CountRows(string fullName)
        {
            var parts = fullName.Split('.');
            if (parts.Length != 2 || parts[0] != "dbo") return 0;
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
                string[] focus = { "dbo.SharjTble", "dbo.PosTable", "dbo.CreditSharj", "dbo.Ticket", "dbo.Devices" };
                var items = new System.Collections.Generic.List<string>();
                foreach (string table in focus) if (tables.Contains(table, StringComparer.OrdinalIgnoreCase)) items.Add(table.Substring(4) + ": " + CountRows(table).ToString("N0"));
                return items.Count == 0 ? "دیتابیس متصل است؛ برای مشاهده‌ی داده‌ها از بخش جداول استفاده کنید." : String.Join("     •     ", items);
            }
            catch (Exception ex) { return DatabaseManager.SafeMessage(ex); }
        }

        private static string Quote(string value) { return "[" + value.Replace("]", "]]" ) + "]"; }
    }
}
