using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using FuntinoControlCenter.Security;
using FuntinoControlCenter.Services;

namespace FuntinoControlCenter.Database
{
    internal static class DatabaseManager
    {
        public static readonly string MainCatalog = GetCatalog("MainDatabaseCatalog", "FuntinoParkOperations");
        public static readonly string DeviceCatalog = GetCatalog("DeviceDatabaseCatalog", "FuntinoDeviceOperations");

        public static SqlConnection CreateMainConnection() { return new SqlConnection(BuildConnectionString(MainCatalog)); }
        public static SqlConnection CreateDeviceConnection() { return new SqlConnection(BuildConnectionString(DeviceCatalog)); }
        public static SqlConnection CreateConnection(bool deviceDatabase) { return deviceDatabase ? CreateDeviceConnection() : CreateMainConnection(); }

        private static string BuildConnectionString(string catalog)
        {
            if (catalog != MainCatalog && catalog != DeviceCatalog) throw new InvalidOperationException("نام پایگاه داده خارج از فهرست مجاز است.");
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = String.IsNullOrWhiteSpace(AppSettings.Current.SqlServer) ? @".\FuntinoSqlServer" : AppSettings.Current.SqlServer.Trim(),
                InitialCatalog = catalog,
                ConnectTimeout = 8,
                ApplicationName = "Funtino Control Center",
                PersistSecurityInfo = false,
                TrustServerCertificate = true
            };
            if (String.IsNullOrWhiteSpace(builder.DataSource)) throw new InvalidOperationException("نام SQL Server را در تنظیمات وارد کنید.");
            if (AppSettings.Current.IntegratedSecurity) builder.IntegratedSecurity = true;
            else { builder.UserID = AppSettings.Current.SqlUser ?? ""; builder.Password = AppSettings.Current.SqlPassword ?? ""; }
            return builder.ConnectionString;
        }

        private static string GetCatalog(string settingName, string fallback)
        {
            string configured = ConfigurationManager.AppSettings[settingName];
            if (String.IsNullOrWhiteSpace(configured)) configured = fallback;
            configured = configured.Trim();
            if (configured.Length > 128 || !IsSafeCatalogName(configured))
                throw new ConfigurationErrorsException("نام پایگاه داده در پیکربندی برنامه معتبر نیست.");
            return configured;
        }

        private static bool IsSafeCatalogName(string value)
        {
            if (String.IsNullOrWhiteSpace(value) || !(Char.IsLetter(value[0]) || value[0] == '_')) return false;
            for (int index = 1; index < value.Length; index++)
                if (!Char.IsLetterOrDigit(value[index]) && value[index] != '_') return false;
            return true;
        }

        public static string ResolveCatalogReferences(string commandText)
        {
            if (commandText == null) return null;
            return commandText
                .Replace("[FuntinoParkOperations].", "[" + MainCatalog + "].")
                .Replace("[FuntinoDeviceOperations].", "[" + DeviceCatalog + "].")
                .Replace("FuntinoParkOperations.", MainCatalog + ".")
                .Replace("FuntinoDeviceOperations.", DeviceCatalog + ".");
        }

        public static bool TestConnection(bool deviceDatabase, out string message, bool bootstrapMode = false)
        {
            if (bootstrapMode)
            {
                if (AppSession.Current != null) throw new UnauthorizedAccessException("تنظیم اتصال اولیه فقط پیش از ورود به حساب کاربری در دسترس است.");
            }
            else AuthorizationGuard.RequireAny("settings.database.test", "devices.view");
            try
            {
                using (var connection = deviceDatabase ? CreateDeviceConnection() : CreateMainConnection()) connection.Open();
                message = deviceDatabase ? "ارتباط با پایگاه دستگاه‌ها برقرار شد." : "ارتباط با پایگاه اصلی برقرار شد.";
                return true;
            }
            catch (Exception ex) { message = SafeMessage(ex); return false; }
        }

        public static string SafeMessage(Exception ex)
        {
            if (ex is SqlException)
            {
                var sql = (SqlException)ex;
                if (sql.Number == 4060 || sql.Number == 18456 || sql.Number == 53 || sql.Number == -1)
                    return "ارتباط برقرار نشد. اجرای سرویس FuntinoSqlServer و وجود پایگاه‌های عملیاتی جدید را بررسی کنید. جزئیات: " + ex.Message;
            }
            return ex.Message;
        }

        public static DataTable Query(SqlConnection connection, string sql, params SqlParameter[] parameters)
        {
            using (var command = new SqlCommand(ResolveCatalogReferences(sql), connection))
            {
                if (parameters != null) command.Parameters.AddRange(parameters);
                using (var adapter = new SqlDataAdapter(command))
                {
                    var table = new DataTable(); adapter.Fill(table); return table;
                }
            }
        }

        public static object Scalar(SqlConnection connection, string sql, params SqlParameter[] parameters)
        {
            using (var command = new SqlCommand(ResolveCatalogReferences(sql), connection))
            {
                if (parameters != null) command.Parameters.AddRange(parameters);
                return command.ExecuteScalar();
            }
        }

        public static void ChangePassword(int userId, string currentPassword, string newPassword)
        {
            if (AppSession.Current == null || AppSession.Current.ApplicationUserId != userId)
                throw new UnauthorizedAccessException("فقط می‌توانید گذرواژه‌ی حساب خودتان را تغییر دهید.");
            if (newPassword == null || newPassword.Length < 8) throw new InvalidOperationException("گذرواژه جدید باید دست‌کم ۸ نویسه داشته باشد.");
            using (var connection = CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        byte[] salt, storedHash; int iterations;
                        using (var command = new SqlCommand("SELECT PasswordSalt, PasswordHash, PasswordHashIterations FROM app.ApplicationUser WHERE ApplicationUserId=@id AND IsActive=1", connection, transaction))
                        {
                            command.Parameters.Add("@id", SqlDbType.Int).Value = userId;
                            using (var reader = command.ExecuteReader())
                            {
                                if (!reader.Read()) throw new InvalidOperationException("کاربر فعال یافت نشد.");
                                salt = (byte[])reader[0]; storedHash = (byte[])reader[1]; iterations = Convert.ToInt32(reader[2]);
                            }
                        }
                        if (!FuntinoControlCenter.Security.PasswordHasher.Verify(currentPassword, salt, storedHash, iterations))
                            throw new InvalidOperationException("گذرواژه فعلی درست نیست.");
                        byte[] newSalt, newHash; Security.PasswordHasher.Create(newPassword, out newSalt, out newHash);
                        using (var command = new SqlCommand("UPDATE app.ApplicationUser SET PasswordSalt=@salt, PasswordHash=@hash, PasswordHashIterations=@iterations, MustChangePassword=0, AuthorizationVersion=AuthorizationVersion+1 WHERE ApplicationUserId=@id", connection, transaction))
                        {
                            command.Parameters.Add("@salt", SqlDbType.VarBinary, 32).Value = newSalt;
                            command.Parameters.Add("@hash", SqlDbType.VarBinary, 32).Value = newHash;
                            command.Parameters.Add("@iterations", SqlDbType.Int).Value = Security.PasswordHasher.Iterations;
                            command.Parameters.Add("@id", SqlDbType.Int).Value = userId;
                            if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("ذخیره گذرواژه انجام نشد.");
                        }
                        AuditService.Write(connection, transaction, "password-change", "ApplicationUser", userId.ToString(), "تغییر گذرواژه");
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
    }
}
