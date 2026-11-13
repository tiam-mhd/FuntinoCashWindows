using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FuntinoControlCenter
{
    internal sealed class AppSettings
    {
        private const string FileName = "controlcenter.settings";
        public string SqlServer { get; set; }
        public bool IntegratedSecurity { get; set; }
        public string SqlUser { get; set; }
        public string SqlPassword { get; set; }
        public string ReaderPort { get; set; }
        public string PosPort { get; set; }
        public int ReaderBaud { get; set; }
        public int PosBaud { get; set; }

        public static AppSettings Current { get; private set; }
        private static string SettingsPath { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName); } }

        static AppSettings() { Current = Load(); }

        public static AppSettings Load()
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (File.Exists(SettingsPath))
            {
                foreach (string line in File.ReadAllLines(SettingsPath, Encoding.UTF8))
                {
                    int p = line.IndexOf('=');
                    if (p <= 0) continue;
                    try { values[line.Substring(0, p)] = Encoding.UTF8.GetString(Convert.FromBase64String(line.Substring(p + 1))); }
                    catch { }
                }
            }
            string encrypted = Get(values, "SqlPasswordProtected", "");
            string password = "";
            if (!String.IsNullOrEmpty(encrypted))
            {
                try { password = Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(encrypted), null, DataProtectionScope.CurrentUser)); }
                catch { password = ""; }
            }
            var settings = new AppSettings
            {
                SqlServer = Get(values, "SqlServer", ""),
                IntegratedSecurity = String.Equals(Get(values, "IntegratedSecurity", "true"), "true", StringComparison.OrdinalIgnoreCase),
                SqlUser = Get(values, "SqlUser", ""), SqlPassword = password,
                ReaderPort = Get(values, "ReaderPort", "COM11"), PosPort = Get(values, "PosPort", "COM21"),
                ReaderBaud = ParseInt(Get(values, "ReaderBaud", "9600"), 9600), PosBaud = ParseInt(Get(values, "PosBaud", "9600"), 9600)
            };
            Current = settings;
            return settings;
        }

        public void Save()
        {
            string protectedPassword = "";
            if (!String.IsNullOrEmpty(SqlPassword))
                protectedPassword = Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(SqlPassword), null, DataProtectionScope.CurrentUser));
            var values = new Dictionary<string, string>
            {
                { "SqlServer", SqlServer ?? "" }, { "IntegratedSecurity", IntegratedSecurity ? "true" : "false" },
                { "SqlUser", SqlUser ?? "" }, { "SqlPasswordProtected", protectedPassword },
                { "ReaderPort", ReaderPort ?? "COM11" }, { "PosPort", PosPort ?? "COM21" },
                { "ReaderBaud", ReaderBaud.ToString() }, { "PosBaud", PosBaud.ToString() }
            };
            using (var writer = new StreamWriter(SettingsPath, false, new UTF8Encoding(true)))
                foreach (var pair in values) writer.WriteLine(pair.Key + "=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(pair.Value)));
            Current = this;
        }

        private static string Get(Dictionary<string, string> values, string key, string fallback)
        {
            string value; return values.TryGetValue(key, out value) ? value : fallback;
        }
        private static int ParseInt(string value, int fallback) { int parsed; return Int32.TryParse(value, out parsed) ? parsed : fallback; }
    }
}
