using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Security;

namespace FuntinoControlCenter.Services
{
    internal static class EdgeConfigurationService
    {
        public static void Save(string baseUrl, string credentialId, string organizationId, string branchId, string secretBase64)
        {
            AuthorizationGuard.Require("edge.configuration.manage");
            Uri uri;
            if (!Uri.TryCreate((baseUrl ?? String.Empty).Trim(), UriKind.Absolute, out uri) ||
                !String.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
                !String.IsNullOrEmpty(uri.UserInfo) || !String.IsNullOrEmpty(uri.Query) || !String.IsNullOrEmpty(uri.Fragment) ||
                (uri.AbsolutePath != "/" && !String.IsNullOrEmpty(uri.AbsolutePath)))
                throw new ArgumentException("نشانی سرور باید یک نشانی HTTPS معتبر و بدون اطلاعات ورود باشد.", "baseUrl");
            if (!IsValidIdentifier(credentialId, 128) || !IsValidIdentifier(organizationId, 128) || !IsValidIdentifier(branchId, 128))
                throw new ArgumentException("شناسهٔ کلید، سازمان یا شعبه معتبر نیست.");
            Guid organizationGuid, branchGuid;
            if (!Guid.TryParse(organizationId, out organizationGuid) || !Guid.TryParse(branchId, out branchGuid))
                throw new ArgumentException("شناسهٔ سازمان و شعبه باید شناسهٔ معتبر سامانهٔ مرکزی باشند.");

            AppSettings settings = AppSettings.Current;
            byte[] replacementSecret = null;
            if (!String.IsNullOrWhiteSpace(secretBase64))
            {
                try { replacementSecret = Convert.FromBase64String(secretBase64.Trim()); }
                catch (FormatException) { throw new ArgumentException("کلید HMAC باید به قالب Base64 وارد شود.", "secretBase64"); }
                if (replacementSecret.Length < 32)
                {
                    Array.Clear(replacementSecret, 0, replacementSecret.Length);
                    throw new ArgumentException("کلید HMAC باید دست‌کم ۳۲ بایت باشد.", "secretBase64");
                }
            }
            if (replacementSecret == null && !settings.EdgeSecretConfigured)
                throw new ArgumentException("برای نخستین راه‌اندازی، کلید HMAC را وارد کنید.", "secretBase64");

            string previousUrl = settings.EdgeApiBaseUrl;
            string previousCredential = settings.EdgeCredentialId;
            string previousOrganization = settings.EdgeOrganizationId;
            string previousBranch = settings.EdgeBranchId;
            byte[] previousSecret = settings.GetEdgeSecret();
            bool localSettingsSaved = false;
            try
            {
                Guid sourceInstanceId;
                if (!Guid.TryParse(settings.EdgeSourceInstanceId, out sourceInstanceId))
                    throw new InvalidOperationException("شناسهٔ پایدار این نصب معتبر نیست.");

                VerifyNoEventsBoundToAnotherConfiguration(previousOrganization, previousBranch, organizationId.Trim(), branchId.Trim());
                settings.EdgeApiBaseUrl = uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
                settings.EdgeCredentialId = credentialId.Trim();
                settings.EdgeOrganizationId = organizationId.Trim();
                settings.EdgeBranchId = branchId.Trim();
                if (replacementSecret != null) settings.SetEdgeSecret(replacementSecret);
                settings.Save();
                localSettingsSaved = true;
                try
                {
                    PersistInstallationConfiguration(sourceInstanceId, settings.EdgeOrganizationId, settings.EdgeBranchId, settings.EdgeCredentialId,
                        previousOrganization, previousBranch);
                }
                catch (Exception)
                {
                    throw new InvalidOperationException("تنظیمات در فایل محافظت‌شدهٔ این کاربر ذخیره شد، اما ثبت همهٔ پایگاه‌های عملیاتی کامل نشد. همین تنظیمات را دوباره ذخیره کنید تا ادامه یابد.");
                }
            }
            catch
            {
                if (!localSettingsSaved)
                {
                    settings.EdgeApiBaseUrl = previousUrl;
                    settings.EdgeCredentialId = previousCredential;
                    settings.EdgeOrganizationId = previousOrganization;
                    settings.EdgeBranchId = previousBranch;
                    if (previousSecret != null) settings.SetEdgeSecret(previousSecret);
                    else settings.ClearEdgeSecret();
                    try { settings.Save(); }
                    catch { }
                }
                throw;
            }
            finally
            {
                if (replacementSecret != null) Array.Clear(replacementSecret, 0, replacementSecret.Length);
                if (previousSecret != null) Array.Clear(previousSecret, 0, previousSecret.Length);
            }
        }

        private static void VerifyNoEventsBoundToAnotherConfiguration(string previousOrganizationId, string previousBranchId,
            string newOrganizationId, string newBranchId)
        {
            bool configurationChanged =
                !String.Equals((previousBranchId ?? String.Empty).Trim(), newBranchId, StringComparison.Ordinal) ||
                !String.Equals((previousOrganizationId ?? String.Empty).Trim(), newOrganizationId, StringComparison.Ordinal);
            if (!configurationChanged) return;

            AuthorizationGuard.Require("edge.configuration.manage");
            foreach (bool deviceDatabase in new[] { false, true })
            {
                using (var connection = deviceDatabase ? DatabaseManager.CreateDeviceConnection() : DatabaseManager.CreateMainConnection())
                {
                    connection.Open();
                    using (var command = new SqlCommand(@"
SELECT COUNT_BIG(1) FROM app.EdgeOutbox
WHERE Status<>'synced' AND
 ((BranchId IS NOT NULL AND BranchId<>@branch) OR
  (OrganizationId IS NOT NULL AND OrganizationId<>@organization));", connection))
                    {
                        command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = newBranchId;
                        command.Parameters.Add("@organization", SqlDbType.NVarChar, 128).Value = newOrganizationId;
                        if (Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture) > 0)
                            throw new InvalidOperationException("رویدادهای ارسال‌نشده به سازمان یا شعبهٔ دیگری تعلق دارند؛ ابتدا همان رویدادها را تعیین تکلیف کنید.");
                    }
                }
            }
        }

        private static void PersistInstallationConfiguration(Guid sourceInstanceId, string organizationId, string branchId, string credentialId,
            string previousOrganizationId, string previousBranchId)
        {
            AuthorizationGuard.Require("edge.configuration.manage");
            var session = AppSession.Current;
            if (session == null) throw new UnauthorizedAccessException("برای ذخیرهٔ تنظیمات شعبه باید وارد سامانه باشید.");
            bool configurationChanged =
                !String.Equals((previousBranchId ?? String.Empty).Trim(), branchId, StringComparison.Ordinal) ||
                !String.Equals((previousOrganizationId ?? String.Empty).Trim(), organizationId, StringComparison.Ordinal);

            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        if (configurationChanged) VerifyNoEventsBoundToAnotherConfiguration(connection, transaction, organizationId, branchId);
                        using (var command = new SqlCommand(@"
UPDATE app.EdgeInstallation WITH(UPDLOCK,HOLDLOCK)
SET SourceInstanceId=@source,OrganizationId=@organization,BranchId=@branch,CredentialId=@credential,
    UpdatedByUserId=@userId,UpdatedByUserName=@userName,UpdatedAtUtc=SYSUTCDATETIME()
WHERE StateId=1;
IF @@ROWCOUNT=0
 INSERT app.EdgeInstallation(StateId,SourceInstanceId,OrganizationId,BranchId,CredentialId,UpdatedByUserId,UpdatedByUserName)
 VALUES(1,@source,@organization,@branch,@credential,@userId,@userName);", connection, transaction))
                        {
                            command.Parameters.Add("@source", SqlDbType.UniqueIdentifier).Value = sourceInstanceId;
                            command.Parameters.Add("@organization", SqlDbType.NVarChar, 128).Value = organizationId;
                            command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                            command.Parameters.Add("@credential", SqlDbType.NVarChar, 128).Value = credentialId;
                            command.Parameters.Add("@userId", SqlDbType.Int).Value = session.ApplicationUserId;
                            command.Parameters.Add("@userName", SqlDbType.NVarChar, 64).Value = session.UserName;
                            command.ExecuteNonQuery();
                        }

                        int assignedEvents = AssignUnconfiguredEvents(connection, transaction, organizationId, branchId);

                        AuditService.Write(connection, transaction, "edge-installation-configured", "EdgeInstallation", branchId,
                            "تنظیم اتصال نصب برای سازمان " + organizationId + " و شعبه " + branchId + " به‌روزرسانی شد. " +
                            (assignedEvents > 0 ? assignedEvents.ToString(CultureInfo.InvariantCulture) + " رویداد محلیِ بدون شعبه به این شعبه نسبت داده شد." : String.Empty));
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            using (var connection = DatabaseManager.CreateDeviceConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        if (configurationChanged) VerifyNoEventsBoundToAnotherConfiguration(connection, transaction, organizationId, branchId);
                        AssignUnconfiguredEvents(connection, transaction, organizationId, branchId);
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

        private static void VerifyNoEventsBoundToAnotherConfiguration(SqlConnection connection, SqlTransaction transaction,
            string organizationId, string branchId)
        {
            using (var command = new SqlCommand(@"
SELECT COUNT_BIG(1) FROM app.EdgeOutbox WITH(UPDLOCK,HOLDLOCK)
WHERE Status<>'synced' AND
 ((BranchId IS NOT NULL AND BranchId<>@branch) OR
  (OrganizationId IS NOT NULL AND OrganizationId<>@organization));", connection, transaction))
            {
                command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                command.Parameters.Add("@organization", SqlDbType.NVarChar, 128).Value = organizationId;
                if (Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture) > 0)
                    throw new InvalidOperationException("رویدادهای ارسال‌نشده به سازمان یا شعبهٔ دیگری تعلق دارند؛ ابتدا همان رویدادها را تعیین تکلیف کنید.");
            }
        }

        private static int AssignUnconfiguredEvents(SqlConnection connection, SqlTransaction transaction, string organizationId, string branchId)
        {
            using (var command = new SqlCommand(@"
UPDATE app.EdgeOutbox
SET OrganizationId=COALESCE(OrganizationId,@organization),BranchId=COALESCE(BranchId,@branch),
    Status=CASE
      WHEN Status NOT IN ('pending','retry','needs_mapping','needs_configuration') THEN Status
      WHEN CardId IS NULL OR (PromotionCode IS NOT NULL AND PromotionId IS NULL) THEN 'needs_mapping'
      ELSE 'pending' END,
    LastError=CASE
      WHEN Status NOT IN ('pending','retry','needs_mapping','needs_configuration') THEN LastError
      WHEN CardId IS NULL OR (PromotionCode IS NOT NULL AND PromotionId IS NULL) THEN N'نگاشت کارت یا جشنواره در تصویر شعبه موجود نیست.'
      ELSE NULL END
WHERE Status<>'synced' AND (BranchId IS NULL OR OrganizationId IS NULL);
DECLARE @assigned int=@@ROWCOUNT;
IF OBJECT_ID(N'app.CardOperationLog',N'U') IS NOT NULL
 UPDATE l SET OrganizationId=COALESCE(l.OrganizationId,@organization),BranchId=COALESCE(l.BranchId,@branch)
 FROM app.CardOperationLog l INNER JOIN app.EdgeOutbox o ON o.LocalTransactionId=l.LocalTransactionId
 WHERE o.Status<>'synced' AND (l.BranchId IS NULL OR l.OrganizationId IS NULL);
SELECT @assigned;", connection, transaction))
            {
                command.Parameters.Add("@organization", SqlDbType.NVarChar, 128).Value = organizationId;
                command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        private static bool IsValidIdentifier(string value, int maxLength)
        {
            if (String.IsNullOrWhiteSpace(value) || value.Trim().Length > maxLength) return false;
            foreach (char character in value)
                if (Char.IsControl(character)) return false;
            return true;
        }
    }
}
