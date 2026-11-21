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
        public string EdgeApiBaseUrl { get; set; }
        public string EdgeCredentialId { get; set; }
        public string EdgeOrganizationId { get; set; }
        public string EdgeBranchId { get; set; }
        public string EdgeSourceInstanceId { get; set; }
        private byte[] readerCardKey;
        public bool ReaderCardKeyConfigured { get; private set; }
        private byte[] edgeSecret;
        public bool EdgeSecretConfigured { get; private set; }

        public byte[] GetReaderCardKey()
        {
            return readerCardKey == null ? null : (byte[])readerCardKey.Clone();
        }

        public void SetReaderCardKey(byte[] value)
        {
            if (value == null || value.Length != 6)
                throw new ArgumentException("کلید کارت باید دقیقاً شش بایت باشد.", "value");
            if (readerCardKey != null) Array.Clear(readerCardKey, 0, readerCardKey.Length);
            readerCardKey = (byte[])value.Clone();
            ReaderCardKeyConfigured = true;
        }

        public byte[] GetEdgeSecret()
        {
            return edgeSecret == null ? null : (byte[])edgeSecret.Clone();
        }

        public void SetEdgeSecret(byte[] value)
        {
            if (value == null || value.Length < 32)
                throw new ArgumentException("کلید همگام‌سازی باید دست‌کم ۳۲ بایت باشد.", "value");
            if (edgeSecret != null) Array.Clear(edgeSecret, 0, edgeSecret.Length);
            edgeSecret = (byte[])value.Clone();
            EdgeSecretConfigured = true;
        }

        public void ClearEdgeSecret()
        {
            if (edgeSecret != null) Array.Clear(edgeSecret, 0, edgeSecret.Length);
            edgeSecret = null;
            EdgeSecretConfigured = false;
        }

        public static AppSettings Current { get; private set; }
        private static string SettingsPath
        {
            get
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(localAppData, "Funtino", "ControlCenter", FileName);
            }
        }
        private static string LegacySettingsPath { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName); } }

        static AppSettings() { Current = Load(); }

        public static AppSettings Load()
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string settingsPath = File.Exists(SettingsPath) ? SettingsPath : LegacySettingsPath;
            if (File.Exists(settingsPath))
            {
                foreach (string line in File.ReadAllLines(settingsPath, Encoding.UTF8))
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
            byte[] readerKey = null;
            string protectedReaderKey = Get(values, "ReaderCardKeyProtected", "");
            if (!String.IsNullOrEmpty(protectedReaderKey))
            {
                try
                {
                    readerKey = ProtectedData.Unprotect(Convert.FromBase64String(protectedReaderKey), null, DataProtectionScope.CurrentUser);
                    if (readerKey.Length != 6) { Array.Clear(readerKey, 0, readerKey.Length); readerKey = null; }
                }
                catch { readerKey = null; }
            }
            byte[] syncSecret = null;
            string protectedEdgeSecret = Get(values, "EdgeSecretProtected", "");
            if (!String.IsNullOrEmpty(protectedEdgeSecret))
            {
                try
                {
                    syncSecret = ProtectedData.Unprotect(Convert.FromBase64String(protectedEdgeSecret), null, DataProtectionScope.CurrentUser);
                    if (syncSecret.Length < 32) { Array.Clear(syncSecret, 0, syncSecret.Length); syncSecret = null; }
                }
                catch { syncSecret = null; }
            }
            string sourceInstanceId = Get(values, "EdgeSourceInstanceId", "");
            bool createSourceInstanceId = String.IsNullOrWhiteSpace(sourceInstanceId);
            if (createSourceInstanceId) sourceInstanceId = Guid.NewGuid().ToString("D");
            var settings = new AppSettings
            {
                SqlServer = Get(values, "SqlServer", @".\FuntinoSqlServer"),
                IntegratedSecurity = String.Equals(Get(values, "IntegratedSecurity", "true"), "true", StringComparison.OrdinalIgnoreCase),
                SqlUser = Get(values, "SqlUser", ""), SqlPassword = password,
                ReaderPort = Get(values, "ReaderPort", "COM11"), PosPort = Get(values, "PosPort", "COM21"),
                ReaderBaud = ParseInt(Get(values, "ReaderBaud", "9600"), 9600), PosBaud = ParseInt(Get(values, "PosBaud", "9600"), 9600),
                EdgeApiBaseUrl = Get(values, "EdgeApiBaseUrl", ""),
                EdgeCredentialId = Get(values, "EdgeCredentialId", ""),
                EdgeOrganizationId = Get(values, "EdgeOrganizationId", ""),
                EdgeBranchId = Get(values, "EdgeBranchId", ""),
                EdgeSourceInstanceId = sourceInstanceId
            };
            settings.readerCardKey = readerKey;
            settings.ReaderCardKeyConfigured = readerKey != null;
            settings.edgeSecret = syncSecret;
            settings.EdgeSecretConfigured = syncSecret != null;
            Current = settings;
            if (createSourceInstanceId) settings.Save();
            return settings;
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
            string protectedPassword = "";
            if (!String.IsNullOrEmpty(SqlPassword))
                protectedPassword = Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(SqlPassword), null, DataProtectionScope.CurrentUser));
            string protectedReaderKey = "";
            if (ReaderCardKeyConfigured && readerCardKey != null)
                protectedReaderKey = Convert.ToBase64String(ProtectedData.Protect(readerCardKey, null, DataProtectionScope.CurrentUser));
            string protectedEdgeSecret = "";
            if (EdgeSecretConfigured && edgeSecret != null)
                protectedEdgeSecret = Convert.ToBase64String(ProtectedData.Protect(edgeSecret, null, DataProtectionScope.CurrentUser));
            var values = new Dictionary<string, string>
            {
                { "SqlServer", SqlServer ?? "" }, { "IntegratedSecurity", IntegratedSecurity ? "true" : "false" },
                { "SqlUser", SqlUser ?? "" }, { "SqlPasswordProtected", protectedPassword },
                { "ReaderCardKeyProtected", protectedReaderKey },
                { "EdgeApiBaseUrl", EdgeApiBaseUrl ?? "" }, { "EdgeCredentialId", EdgeCredentialId ?? "" },
                { "EdgeOrganizationId", EdgeOrganizationId ?? "" }, { "EdgeBranchId", EdgeBranchId ?? "" },
                { "EdgeSourceInstanceId", EdgeSourceInstanceId ?? "" }, { "EdgeSecretProtected", protectedEdgeSecret },
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
