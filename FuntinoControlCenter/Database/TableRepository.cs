using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace FuntinoControlCenter.Database
{
    internal sealed class TableColumn
    {
        public string Name; public string TypeName; public short SystemTypeId; public short MaxLength; public byte Precision; public byte Scale;
        public bool Nullable; public bool Identity; public bool Computed; public bool HasDefault; public bool PrimaryKey;
        public bool IsBinary { get { return String.Equals(TypeName, "binary", StringComparison.OrdinalIgnoreCase) || String.Equals(TypeName, "varbinary", StringComparison.OrdinalIgnoreCase) || String.Equals(TypeName, "image", StringComparison.OrdinalIgnoreCase); } }
        public bool IsWritable { get { return !Identity && !Computed && !PrimaryKey && !IsBinary && !String.Equals(TypeName, "timestamp", StringComparison.OrdinalIgnoreCase) && !String.Equals(TypeName, "rowversion", StringComparison.OrdinalIgnoreCase); } }
        public bool IsSearchable { get { return SystemTypeId != 34 && SystemTypeId != 165 && SystemTypeId != 189 && SystemTypeId !=  imageTypeId && SystemTypeId != 240; } }
        private const short imageTypeId = 34;
    }

    internal sealed class TablePage
    {
        public DataTable Data;
        public int Page;
        public int PageSize;
        public int TotalRows;
        public int PageCount { get { return Math.Max(1, (int)Math.Ceiling((double)TotalRows / PageSize)); } }
    }

    internal static class TableRepository
    {
        public static List<string> GetTableNames(bool deviceDatabase = false)
        {
            using (var connection = DatabaseManager.CreateConnection(deviceDatabase))
            {
                connection.Open();
                var dt = DatabaseManager.Query(connection, "SELECT s.name + N'.' + t.name AS FullName FROM sys.tables t INNER JOIN sys.schemas s ON s.schema_id=t.schema_id WHERE t.is_ms_shipped=0 AND s.name<>N'app' ORDER BY s.name,t.name");
                var result = new List<string>(); foreach (DataRow row in dt.Rows) result.Add(Convert.ToString(row[0])); return result;
            }
        }

        public static List<TableColumn> GetColumns(string fullName, bool deviceDatabase = false)
        {
            string schema, table; SplitName(fullName, deviceDatabase, out schema, out table);
            using (var connection = DatabaseManager.CreateConnection(deviceDatabase))
            {
                connection.Open();
                const string sql = @"SELECT c.name,ty.name,c.system_type_id,c.max_length,c.precision,c.scale,c.is_nullable,c.is_identity,c.is_computed,CASE WHEN c.default_object_id=0 THEN 0 ELSE 1 END AS HasDefault,
CASE WHEN pk.column_id IS NULL THEN 0 ELSE 1 END AS IsPrimaryKey
FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id JOIN sys.columns c ON c.object_id=t.object_id JOIN sys.types ty ON ty.user_type_id=c.user_type_id
LEFT JOIN (SELECT ic.object_id,ic.column_id FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id WHERE i.is_primary_key=1) pk ON pk.object_id=t.object_id AND pk.column_id=c.column_id
WHERE s.name=@s AND t.name=@t ORDER BY c.column_id";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@s", SqlDbType.NVarChar, 128).Value = schema; command.Parameters.Add("@t", SqlDbType.NVarChar, 128).Value = table;
                    var result = new List<TableColumn>(); using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read()) result.Add(new TableColumn { Name = reader.GetString(0), TypeName = reader.GetString(1), SystemTypeId = reader.GetByte(2), MaxLength = reader.GetInt16(3), Precision = reader.GetByte(4), Scale = reader.GetByte(5), Nullable = reader.GetBoolean(6), Identity = reader.GetBoolean(7), Computed = reader.GetBoolean(8), HasDefault = reader.GetInt32(9) != 0, PrimaryKey = reader.GetInt32(10) != 0 });
                    }
                    if (result.Count == 0) throw new InvalidOperationException("جدول انتخاب‌شده در پایگاه اصلی پیدا نشد.");
                    return result;
                }
            }
        }

        public static TablePage GetPage(string fullName, int page, int pageSize, string search, bool deviceDatabase = false)
        {
            List<TableColumn> columns = GetColumns(fullName, deviceDatabase); string schema, table; SplitName(fullName, deviceDatabase, out schema, out table);
            var primary = columns.Where(c => c.PrimaryKey).ToList();
            string orderBy = primary.Count > 0 ? String.Join(",", primary.Select(c => "t." + Quote(c.Name))) : "(SELECT 1)";
            string selected = String.Join(",", columns.Select(c => "t." + Quote(c.Name)));
            var searchable = columns.Where(c => c.IsSearchable).ToList(); string predicate = "";
            var parameters = new List<SqlParameter>();
            if (!String.IsNullOrWhiteSpace(search) && searchable.Count > 0)
            {
                predicate = " WHERE " + String.Join(" OR ", searchable.Select(c => "CONVERT(nvarchar(4000), t." + Quote(c.Name) + ") LIKE @search"));
                parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar, 4000) { Value = "%" + search.Trim().Substring(0, Math.Min(search.Trim().Length, 100)) + "%" });
            }
            int total;
            using (var connection = DatabaseManager.CreateConnection(deviceDatabase))
            {
                connection.Open(); string from = " FROM " + Quote(schema) + "." + Quote(table) + " t" + predicate;
                using (var count = new SqlCommand("SELECT COUNT_BIG(1)" + from, connection))
                {
                    if (parameters.Count > 0) count.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar, 4000) { Value = parameters[0].Value });
                    total = Convert.ToInt32(count.ExecuteScalar());
                }
                string rowAlias = "__FuntinoRowNumber";
                while (columns.Any(c => String.Equals(c.Name, rowAlias, StringComparison.OrdinalIgnoreCase))) rowAlias += "X";
                string query = "SELECT " + String.Join(",", columns.Select(c => "q." + Quote(c.Name))) + " FROM (SELECT ROW_NUMBER() OVER(ORDER BY " + orderBy + ") AS " + Quote(rowAlias) + "," + selected + from + ") q WHERE q." + Quote(rowAlias) + ">@first AND q." + Quote(rowAlias) + "<=@last ORDER BY q." + Quote(rowAlias);
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters.Count > 0) command.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar, 4000) { Value = parameters[0].Value });
                    command.Parameters.Add("@first", SqlDbType.BigInt).Value = (long)Math.Max(0, page - 1) * pageSize;
                    command.Parameters.Add("@last", SqlDbType.BigInt).Value = (long)Math.Max(0, page - 1) * pageSize + pageSize;
                    using (var adapter = new SqlDataAdapter(command)) { var data = new DataTable(); adapter.Fill(data); return new TablePage { Data = data, Page = Math.Max(1, page), PageSize = pageSize, TotalRows = total }; }
                }
            }
        }

        public static DataTable GetReportData(string fullName, string search, int maxRows, out bool truncated, bool deviceDatabase = false)
        {
            if (maxRows < 1) throw new ArgumentOutOfRangeException("maxRows");
            var columns = GetColumns(fullName, deviceDatabase); string schema, table; SplitName(fullName, deviceDatabase, out schema, out table);
            var searchable = columns.Where(c => c.IsSearchable).ToList(); string predicate = ""; object searchValue = null;
            if (!String.IsNullOrWhiteSpace(search) && searchable.Count > 0)
            {
                predicate = " WHERE " + String.Join(" OR ", searchable.Select(c => "CONVERT(nvarchar(4000), t." + Quote(c.Name) + ") LIKE @search"));
                string term = search.Trim(); searchValue = "%" + term.Substring(0, Math.Min(term.Length, 100)) + "%";
            }
            var keys = columns.Where(c => c.PrimaryKey).ToList(); string order = keys.Count > 0 ? String.Join(",", keys.Select(c => "t." + Quote(c.Name))) : "(SELECT 1)";
            string from = " FROM " + Quote(schema) + "." + Quote(table) + " t" + predicate;
            using (var connection = DatabaseManager.CreateConnection(deviceDatabase))
            {
                connection.Open(); long total;
                using (var count = new SqlCommand("SELECT COUNT_BIG(1)" + from, connection))
                {
                    if (searchValue != null) count.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar, 4000) { Value = searchValue });
                    total = Convert.ToInt64(count.ExecuteScalar());
                }
                string sql = "SELECT TOP (@limit) " + String.Join(",", columns.Select(c => "t." + Quote(c.Name))) + from + " ORDER BY " + order;
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@limit", SqlDbType.Int).Value = maxRows;
                    if (searchValue != null) command.Parameters.Add(new SqlParameter("@search", SqlDbType.NVarChar, 4000) { Value = searchValue });
                    using (var adapter = new SqlDataAdapter(command)) { var data = new DataTable(); adapter.Fill(data); truncated = total > maxRows; return data; }
                }
            }
        }

        public static int SaveChanges(string fullName, DataTable data, bool deviceDatabase = false)
        {
            if (data == null) return 0;
            var columns = GetColumns(fullName, deviceDatabase); var keys = columns.Where(c => c.PrimaryKey).ToList();
            string schema, table; SplitName(fullName, deviceDatabase, out schema, out table);
            if (keys.Count == 0 && data.Rows.Cast<DataRow>().Any(r => r.RowState == DataRowState.Modified || r.RowState == DataRowState.Deleted))
                throw new InvalidOperationException("این جدول کلید اصلی ندارد؛ ویرایش و حذف برای جلوگیری از تغییر چند ردیف هم‌زمان غیرفعال است.");
            int changed = 0;
            using (var connection = DatabaseManager.CreateConnection(deviceDatabase))
            {
                connection.Open(); using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        foreach (DataRow row in data.Rows)
                        {
                            if (row.RowState == DataRowState.Added) { InsertRow(connection, transaction, schema, table, columns, row); changed++; }
                            else if (row.RowState == DataRowState.Modified) { UpdateRow(connection, transaction, schema, table, columns, keys, row); changed++; }
                            else if (row.RowState == DataRowState.Deleted) { DeleteRow(connection, transaction, schema, table, columns, keys, row); changed++; }
                        }
                        if (changed > 0) WriteAudit(connection, transaction, "table-save", schema + "." + table, changed.ToString(), "ذخیره تغییرات جدول");
                        transaction.Commit(); data.AcceptChanges(); return changed;
                    }
                    catch { transaction.Rollback(); throw; }
                }
            }
        }

        private static void InsertRow(SqlConnection connection, SqlTransaction transaction, string schema, string table, List<TableColumn> columns, DataRow row)
        {
            var fields = columns.Where(c => !c.Identity && !c.Computed && !IsRowVersion(c) && row.Table.Columns.Contains(c.Name) && row[c.Name] != DBNull.Value).ToList();
            string target = Quote(schema) + "." + Quote(table);
            string sql;
            if (fields.Count == 0) sql = "INSERT " + target + " DEFAULT VALUES";
            else sql = "INSERT " + target + "(" + String.Join(",", fields.Select(c => Quote(c.Name))) + ") VALUES(" + String.Join(",", fields.Select((c, i) => "@p" + i)) + ")";
            using (var command = new SqlCommand(sql, connection, transaction))
            {
                for (int i = 0; i < fields.Count; i++) AddParameter(command, "@p" + i, fields[i], row[fields[i].Name]);
                command.ExecuteNonQuery();
            }
        }

        private static void UpdateRow(SqlConnection connection, SqlTransaction transaction, string schema, string table, List<TableColumn> columns, List<TableColumn> keys, DataRow row)
        {
            var changed = columns.Where(c => c.IsWritable && row.Table.Columns.Contains(c.Name) && !object.Equals(row[c.Name, DataRowVersion.Current], row[c.Name, DataRowVersion.Original])).ToList();
            if (changed.Count == 0) return;
            string target = Quote(schema) + "." + Quote(table);
            string set = String.Join(",", changed.Select((c, i) => Quote(c.Name) + "=@p" + i));
            var version = columns.FirstOrDefault(IsRowVersion);
            string where = String.Join(" AND ", keys.Select((c, i) => Quote(c.Name) + "=@k" + i));
            if (version != null) where += " AND " + Quote(version.Name) + "=@rv";
            using (var command = new SqlCommand("UPDATE " + target + " SET " + set + " WHERE " + where, connection, transaction))
            {
                for (int i = 0; i < changed.Count; i++) AddParameter(command, "@p" + i, changed[i], row[changed[i].Name, DataRowVersion.Current]);
                for (int i = 0; i < keys.Count; i++) AddParameter(command, "@k" + i, keys[i], row[keys[i].Name, DataRowVersion.Original]);
                if (version != null) AddParameter(command, "@rv", version, row[version.Name, DataRowVersion.Original]);
                if (command.ExecuteNonQuery() != 1) throw new DBConcurrencyException("ردیف تغییر کرده یا حذف شده است؛ فهرست را تازه‌سازی و دوباره تلاش کنید.");
            }
        }

        private static void DeleteRow(SqlConnection connection, SqlTransaction transaction, string schema, string table, List<TableColumn> columns, List<TableColumn> keys, DataRow row)
        {
            string where = String.Join(" AND ", keys.Select((c, i) => Quote(c.Name) + "=@k" + i));
            TableColumn version = columns.FirstOrDefault(IsRowVersion);
            if (version != null) where += " AND " + Quote(version.Name) + "=@rv";
            using (var command = new SqlCommand("DELETE FROM " + Quote(schema) + "." + Quote(table) + " WHERE " + where, connection, transaction))
            {
                for (int i = 0; i < keys.Count; i++) AddParameter(command, "@k" + i, keys[i], row[keys[i].Name, DataRowVersion.Original]);
                if (version != null) AddParameter(command, "@rv", version, row[version.Name, DataRowVersion.Original]);
                if (command.ExecuteNonQuery() != 1) throw new DBConcurrencyException("ردیف تغییر کرده یا حذف شده است؛ فهرست را تازه‌سازی و دوباره تلاش کنید.");
            }
        }

        private static void AddParameter(SqlCommand command, string name, TableColumn column, object value)
        {
            SqlParameter p;
            if (column.TypeName == "decimal" || column.TypeName == "numeric")
            {
                p = command.Parameters.Add(name, SqlDbType.Decimal); p.Precision = column.Precision; p.Scale = column.Scale;
            }
            else if (column.TypeName == "timestamp" || column.TypeName == "rowversion") p = command.Parameters.Add(name, SqlDbType.VarBinary, 8);
            else if (column.TypeName == "varchar" || column.TypeName == "char" || column.TypeName == "text" || column.TypeName == "nvarchar" || column.TypeName == "nchar" || column.TypeName == "ntext")
                p = command.Parameters.Add(name, column.TypeName == "varchar" || column.TypeName == "char" || column.TypeName == "text" ? SqlDbType.VarChar : SqlDbType.NVarChar, column.MaxLength < 0 ? -1 : Math.Max(1, column.MaxLength / (column.TypeName.StartsWith("n", StringComparison.OrdinalIgnoreCase) ? 2 : 1)));
            else p = command.Parameters.Add(name, ToSqlDbType(column.TypeName));
            p.Value = value ?? DBNull.Value;
        }

        private static SqlDbType ToSqlDbType(string type)
        {
            switch ((type ?? "").ToLowerInvariant())
            {
                case "bigint": return SqlDbType.BigInt; case "int": return SqlDbType.Int; case "smallint": return SqlDbType.SmallInt; case "tinyint": return SqlDbType.TinyInt;
                case "bit": return SqlDbType.Bit; case "money": return SqlDbType.Money; case "smallmoney": return SqlDbType.SmallMoney; case "float": return SqlDbType.Float; case "real": return SqlDbType.Real;
                case "datetime": return SqlDbType.DateTime; case "datetime2": return SqlDbType.DateTime2; case "smalldatetime": return SqlDbType.SmallDateTime; case "date": return SqlDbType.Date; case "time": return SqlDbType.Time;
                case "uniqueidentifier": return SqlDbType.UniqueIdentifier; case "binary": case "varbinary": case "image": return SqlDbType.VarBinary; case "xml": return SqlDbType.Xml; case "sql_variant": return SqlDbType.Variant;
                case "timestamp": case "rowversion": return SqlDbType.Timestamp; default: return SqlDbType.NVarChar;
            }
        }

        private static void WriteAudit(SqlConnection connection, SqlTransaction transaction, string action, string entity, string key, string details)
        {
            var session = AppSession.Current;
            using (var command = new SqlCommand("INSERT app.AuditEvent(ApplicationUserId,ActionName,EntityName,EntityKey,Details) VALUES(@applicationUserId,@actionName,@entityName,@entityKey,@details)", connection, transaction))
            {
                command.Parameters.Add("@applicationUserId", SqlDbType.Int).Value = session == null ? (object)DBNull.Value : session.ApplicationUserId;
                command.Parameters.Add("@actionName", SqlDbType.NVarChar, 80).Value = action;
                command.Parameters.Add("@entityName", SqlDbType.NVarChar, 128).Value = entity;
                command.Parameters.Add("@entityKey", SqlDbType.NVarChar, 256).Value = key;
                command.Parameters.Add("@details", SqlDbType.NVarChar, 1000).Value = details;
                command.ExecuteNonQuery();
            }
        }

        private static bool IsRowVersion(TableColumn c) { return c.TypeName == "timestamp" || c.TypeName == "rowversion"; }
        private static string Quote(string identifier) { return "[" + (identifier ?? "").Replace("]", "]]" ) + "]"; }
        private static void SplitName(string fullName, bool deviceDatabase, out string schema, out string table)
        {
            string[] parts = (fullName ?? "").Split('.');
            if (parts.Length != 2 || parts.Any(p => p.Length == 0 || p.Length > 128)) throw new InvalidOperationException("نام جدول معتبر نیست.");
            schema = parts[0]; table = parts[1];
        }
    }
}
