using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using FuntinoControlCenter.Security;
using FuntinoControlCenter.Services;

namespace FuntinoControlCenter.Database
{
    internal static class EdgeWorkerContext
    {
        [ThreadStatic]
        private static bool active;

        public static IDisposable Enter()
        {
            bool previous = active;
            active = true;
            return new Scope(delegate { active = previous; });
        }

        public static void Require()
        {
            if (!active) throw new UnauthorizedAccessException("این عملیات فقط برای کار همگام‌سازی پس‌زمینه مجاز است.");
        }

        private sealed class Scope : IDisposable
        {
            private Action dispose;
            public Scope(Action action) { dispose = action; }
            public void Dispose()
            {
                Action action = dispose;
                dispose = null;
                if (action != null) action();
            }
        }
    }

    internal sealed class EdgeQueuedEvent
    {
        public Guid EventId { get; set; }
        public bool DeviceDatabase { get; set; }
        public int RetryCount { get; set; }
        public EdgeEventDto Payload { get; set; }
    }

    internal sealed class EdgeSyncDiagnostics
    {
        public string BranchId { get; set; }
        public string SnapshotBranchId { get; set; }
        public bool MainDatabaseAvailable { get; set; }
        public bool DeviceDatabaseAvailable { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastServerResponseUtc { get; set; }
        public DateTime? LastSuccessfulSyncUtc { get; set; }
        public DateTime? LastSnapshotAtUtc { get; set; }
        public string SnapshotVersion { get; set; }
        public DateTime? SnapshotExpiresAtUtc { get; set; }
        public Guid? LastAcknowledgedEventId { get; set; }
        public string LastError { get; set; }
        public DateTime? HeartbeatUtc { get; set; }
        public long PendingCount { get; set; }
        public long NeedsMappingCount { get; set; }
        public long RejectedCount { get; set; }
        public long DeadLetterCount { get; set; }
        public DataTable Events { get; set; }
    }

    internal sealed class EdgeSnapshotStatus
    {
        public string BranchId { get; set; }
        public string Version { get; set; }
        public DateTime? IssuedAtUtc { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public bool IsFresh { get; set; }
    }

    internal static class EdgeIntegrationRepository
    {
        private const int DefaultBatchSize = 50;

        public static byte[] GetCardKeyHash(byte[] cardUid)
        {
            if (cardUid == null || cardUid.Length != 4) throw new ArgumentException("شناسه‌ی کارت معتبر نیست.", "cardUid");
            using (var sha = SHA256.Create()) return sha.ComputeHash(cardUid);
        }

        public static string MaskCardUid(byte[] cardUid)
        {
            if (cardUid == null || cardUid.Length == 0) return null;
            string value = BitConverter.ToString(cardUid).Replace("-", String.Empty);
            return "****" + value.Substring(Math.Max(0, value.Length - 4));
        }

        public static void PrepareRecharge(long operationId, byte[] cardUid, long startingBalanceToman, long expectedBalanceToman, RechargeOperationTerms terms)
        {
            AuthorizationGuard.Require("cards.recharge");
            AuthorizationGuard.Require(PermissionCatalog.TableCode("finance", "create"));
            AuthorizationGuard.Require("edge.events.create");
            if (operationId <= 0) throw new ArgumentOutOfRangeException("operationId");
            if (terms == null) throw new ArgumentNullException("terms");
            if (expectedBalanceToman < startingBalanceToman) throw new ArgumentOutOfRangeException("expectedBalanceToman");

            byte[] cardKeyHash = GetCardKeyHash(cardUid);
            AppSettings settings = AppSettings.Current;
            Guid sourceInstanceId = GetSourceInstanceId(settings);
            string branchId = Clean(settings.EdgeBranchId, 128);
            string organizationId = Clean(settings.EdgeOrganizationId, 128);
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var command = new SqlCommand(@"
UPDATE app.CardOperationLog
SET CardUid=@maskedUid,CardKeyHash=@cardKeyHash,StartingBalanceToman=@startingBalance,
    ExpectedBalanceToman=@expectedBalance,PromotionCode=@promotionCode,PromotionName=@promotionName,
    PromotionDiscountPercent=@promotionPercent,DailyDiscountPercent=@dailyPercent,
    DiscountTierId=@discountTier,PricingVersion=@pricingVersion,OccurredAtUtc=SYSUTCDATETIME(),
    OrganizationId=@organizationId,BranchId=@branchId,SourceInstanceId=@sourceInstanceId,
    OperationState='started'
WHERE CardOperationLogId=@id AND ApplicationUserId=@userId AND OperationName=N'card-recharge';", connection))
                {
                    command.Parameters.Add("@maskedUid", SqlDbType.VarChar, 11).Value = MaskCardUid(cardUid);
                    command.Parameters.Add("@cardKeyHash", SqlDbType.VarBinary, 32).Value = cardKeyHash;
                    command.Parameters.Add("@startingBalance", SqlDbType.BigInt).Value = startingBalanceToman;
                    command.Parameters.Add("@expectedBalance", SqlDbType.BigInt).Value = expectedBalanceToman;
                    command.Parameters.Add("@promotionCode", SqlDbType.Int).Value = terms.PromotionCode;
                    command.Parameters.Add("@promotionName", SqlDbType.NVarChar, 160).Value = DbValue(Clean(terms.PromotionName, 160));
                    command.Parameters.Add("@promotionPercent", SqlDbType.Int).Value = terms.PromotionDiscountPercent;
                    command.Parameters.Add("@dailyPercent", SqlDbType.Int).Value = terms.DailyDiscountPercent;
                    command.Parameters.Add("@discountTier", SqlDbType.Int).Value = terms.DiscountTierId;
                    command.Parameters.Add("@pricingVersion", SqlDbType.NVarChar, 128).Value = GetPricingVersion(connection, branchId, terms);
                    command.Parameters.Add("@organizationId", SqlDbType.NVarChar, 128).Value = DbValue(organizationId);
                    command.Parameters.Add("@branchId", SqlDbType.NVarChar, 128).Value = DbValue(branchId);
                    command.Parameters.Add("@sourceInstanceId", SqlDbType.UniqueIdentifier).Value = sourceInstanceId;
                    command.Parameters.Add("@id", SqlDbType.BigInt).Value = operationId;
                    command.Parameters.Add("@userId", SqlDbType.Int).Value = AppSession.Current.ApplicationUserId;
                    if (command.ExecuteNonQuery() != 1) throw new UnauthorizedAccessException("عملیات شارژ برای آماده‌سازی ثبت محلی مجاز نیست.");
                }
            }
        }

        public static bool HasOutboxForOperation(SqlConnection connection, SqlTransaction transaction, long operationId)
        {
            AuthorizationGuard.Require("cards.recharge");
            AuthorizationGuard.Require("edge.events.create");
            using (var command = new SqlCommand(@"
SELECT CASE WHEN EXISTS
 (SELECT 1 FROM app.EdgeOutbox o INNER JOIN app.CardOperationLog l ON l.LocalTransactionId=o.LocalTransactionId
  WHERE l.CardOperationLogId=@id AND ((@branch=N'' AND l.BranchId IS NULL) OR l.BranchId=@branch)) THEN 1 ELSE 0 END;", connection, transaction))
            {
                command.Parameters.Add("@id", SqlDbType.BigInt).Value = operationId;
                command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = AppSettings.Current.EdgeBranchId ?? String.Empty;
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) == 1;
            }
        }

        public static void InsertRechargeOutbox(SqlConnection connection, SqlTransaction transaction, long operationId, long amountToman, RechargeOperationTerms terms)
        {
            AuthorizationGuard.Require("cards.recharge");
            AuthorizationGuard.Require(PermissionCatalog.TableCode("finance", "create"));
            AuthorizationGuard.Require("edge.events.create");
            if (amountToman <= 0) throw new ArgumentOutOfRangeException("amountToman");
            if (terms == null) throw new ArgumentNullException("terms");

            Guid eventId;
            Guid localTransactionId;
            Guid sourceInstanceId;
            string organizationId;
            string branchId;
            string pricingVersion;
            byte[] cardKeyHash;
            long? balanceAfterRial;
            int promotionCode;
            int promotionDiscountPercent;
            int dailyDiscountPercent;
            int discountTierId;
            int originalUserId;
            string originalUserName;
            DateTime occurredAtUtc;
            using (var command = new SqlCommand(@"
SELECT EventId,LocalTransactionId,SourceInstanceId,OrganizationId,BranchId,PricingVersion,CardKeyHash,
       ExpectedBalanceToman,PromotionCode,PromotionDiscountPercent,DailyDiscountPercent,DiscountTierId,OccurredAtUtc,
       ApplicationUserId,OperatorUserName
FROM app.CardOperationLog
WHERE CardOperationLogId=@id AND OperationName=N'card-recharge'
  AND ((@branch=N'' AND BranchId IS NULL) OR BranchId=@branch)
  AND OperationState IN ('verified','locally_committed','pending_sync');", connection, transaction))
            {
                command.Parameters.Add("@id", SqlDbType.BigInt).Value = operationId;
                command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = AppSettings.Current.EdgeBranchId ?? String.Empty;
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read()) throw new InvalidOperationException("عملیات فیزیکی شارژ هنوز به‌طور کامل بررسی نشده است.");
                    eventId = reader.GetGuid(0);
                    localTransactionId = reader.GetGuid(1);
                    sourceInstanceId = reader.IsDBNull(2) ? GetSourceInstanceId(AppSettings.Current) : reader.GetGuid(2);
                    organizationId = reader.IsDBNull(3) ? null : reader.GetString(3);
                    branchId = reader.IsDBNull(4) ? null : reader.GetString(4);
                    pricingVersion = reader.IsDBNull(5) ? BuildLocalPricingVersion(terms) : reader.GetString(5);
                    cardKeyHash = reader.IsDBNull(6) ? null : (byte[])reader[6];
                    long expectedBalance = reader.IsDBNull(7) ? 0 : reader.GetInt64(7);
                    balanceAfterRial = expectedBalance > 0 && expectedBalance <= Int64.MaxValue / 10 ? (long?)(expectedBalance * 10) : null;
                    promotionCode = reader.IsDBNull(8) ? terms.PromotionCode : reader.GetInt32(8);
                    promotionDiscountPercent = reader.IsDBNull(9) ? terms.PromotionDiscountPercent : reader.GetInt32(9);
                    dailyDiscountPercent = reader.IsDBNull(10) ? terms.DailyDiscountPercent : reader.GetInt32(10);
                    discountTierId = reader.IsDBNull(11) ? terms.DiscountTierId : reader.GetInt32(11);
                    occurredAtUtc = DateTime.SpecifyKind(reader.GetDateTime(12), DateTimeKind.Utc);
                    originalUserId = reader.GetInt32(13);
                    originalUserName = reader.IsDBNull(14) ? AppSession.Current.UserName : reader.GetString(14);
                }
            }

            if (HasOutboxForOperation(connection, transaction, operationId))
            {
                SetOperationState(connection, transaction, operationId, "locally_committed", "locally_committed");
                return;
            }
            if (cardKeyHash == null || cardKeyHash.Length != 32) throw new InvalidOperationException("شناسه‌ی امن کارت برای رویداد ثبت نشده است.");

            string cardId = null;
            string householdId = null;
            string memberId = null;
            string promotionId = null;
            if (!String.IsNullOrWhiteSpace(branchId))
            {
                using (var mapping = new SqlCommand(@"
SELECT CardId,HouseholdId,MemberId FROM app.EdgeCardMapping
WHERE BranchId=@branch AND CardKeyHash=@hash;", connection, transaction))
                {
                    mapping.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                    mapping.Parameters.Add("@hash", SqlDbType.VarBinary, 32).Value = cardKeyHash;
                    using (var reader = mapping.ExecuteReader())
                        if (reader.Read())
                        {
                            cardId = reader.GetString(0);
                            householdId = reader.IsDBNull(1) ? null : reader.GetString(1);
                            memberId = reader.IsDBNull(2) ? null : reader.GetString(2);
                        }
                }
                if (promotionCode > 0)
                {
                    using (var promotion = new SqlCommand("SELECT PromotionId FROM app.EdgePromotionMapping WHERE BranchId=@branch AND PromotionCode=@code", connection, transaction))
                    {
                        promotion.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                        promotion.Parameters.Add("@code", SqlDbType.Int).Value = promotionCode;
                        object mapped = promotion.ExecuteScalar();
                        if (mapped != null && mapped != DBNull.Value) promotionId = Convert.ToString(mapped, CultureInfo.InvariantCulture);
                    }
                }
            }

            long localSequence = AllocateSequence(connection, transaction, sourceInstanceId, cardKeyHash);
            long amountRial = EdgeAmountConverter.ToRial(amountToman);
            string status = String.IsNullOrWhiteSpace(branchId) || String.IsNullOrWhiteSpace(organizationId)
                ? "needs_configuration"
                : (String.IsNullOrWhiteSpace(cardId) || (promotionCode > 0 && String.IsNullOrWhiteSpace(promotionId)) ? "needs_mapping" : "pending");
            string readinessMessage = status == "needs_configuration" ? "شناسه‌ی سازمان و شعبه برای این نصب تنظیم نشده است."
                : status == "needs_mapping" ? "نگاشت کارت یا جشنواره در تصویر شعبه موجود نیست."
                : null;
            string sourceDatabase = DatabaseManager.MainCatalog;
            using (var command = new SqlCommand(@"
INSERT app.EdgeOutbox
(EventId,LocalTransactionId,SourceInstanceId,SourceDatabase,OrganizationId,BranchId,CardId,HouseholdId,MemberId,
 TransactionKind,AmountRial,CurrencyCode,GrossAmountRial,DiscountAmountRial,NetAmountRial,PromotionId,PromotionCode,
 PromotionDiscountPercent,DailyDiscountPercent,DiscountTierId,PricingVersion,OccurredAtUtc,LocalSequence,BalanceAfterRial,
 OperationResult,ApplicationUserId,OperatorUserName,CardKeyHash,Status,LastError)
VALUES
(@eventId,@localTransactionId,@sourceInstanceId,@sourceDatabase,@organizationId,@branchId,@cardId,@householdId,@memberId,
 'top_up',@amountRial,'IRR',@grossAmountRial,0,@netAmountRial,@promotionId,@promotionCode,
 @promotionDiscountPercent,@dailyDiscountPercent,@discountTierId,@pricingVersion,@occurredAtUtc,@localSequence,@balanceAfterRial,
 'succeeded',@userId,@userName,@cardKeyHash,@status,@lastError);", connection, transaction))
            {
                command.Parameters.Add("@eventId", SqlDbType.UniqueIdentifier).Value = eventId;
                command.Parameters.Add("@localTransactionId", SqlDbType.UniqueIdentifier).Value = localTransactionId;
                command.Parameters.Add("@sourceInstanceId", SqlDbType.UniqueIdentifier).Value = sourceInstanceId;
                command.Parameters.Add("@sourceDatabase", SqlDbType.NVarChar, 128).Value = sourceDatabase;
                command.Parameters.Add("@organizationId", SqlDbType.NVarChar, 128).Value = DbValue(organizationId);
                command.Parameters.Add("@branchId", SqlDbType.NVarChar, 128).Value = DbValue(branchId);
                command.Parameters.Add("@cardId", SqlDbType.NVarChar, 128).Value = DbValue(cardId);
                command.Parameters.Add("@householdId", SqlDbType.NVarChar, 128).Value = DbValue(householdId);
                command.Parameters.Add("@memberId", SqlDbType.NVarChar, 128).Value = DbValue(memberId);
                command.Parameters.Add("@amountRial", SqlDbType.BigInt).Value = amountRial;
                command.Parameters.Add("@grossAmountRial", SqlDbType.BigInt).Value = amountRial;
                command.Parameters.Add("@netAmountRial", SqlDbType.BigInt).Value = amountRial;
                command.Parameters.Add("@promotionId", SqlDbType.NVarChar, 128).Value = DbValue(promotionId);
                command.Parameters.Add("@promotionCode", SqlDbType.Int).Value = promotionCode == 0 ? (object)DBNull.Value : promotionCode;
                command.Parameters.Add("@promotionDiscountPercent", SqlDbType.Int).Value = promotionDiscountPercent;
                command.Parameters.Add("@dailyDiscountPercent", SqlDbType.Int).Value = dailyDiscountPercent;
                command.Parameters.Add("@discountTierId", SqlDbType.Int).Value = discountTierId;
                command.Parameters.Add("@pricingVersion", SqlDbType.NVarChar, 128).Value = pricingVersion;
                command.Parameters.Add("@occurredAtUtc", SqlDbType.DateTime2).Value = occurredAtUtc;
                command.Parameters.Add("@localSequence", SqlDbType.BigInt).Value = localSequence;
                command.Parameters.Add("@balanceAfterRial", SqlDbType.BigInt).Value = balanceAfterRial.HasValue ? (object)balanceAfterRial.Value : DBNull.Value;
                command.Parameters.Add("@userId", SqlDbType.Int).Value = originalUserId;
                command.Parameters.Add("@userName", SqlDbType.NVarChar, 64).Value = originalUserName;
                command.Parameters.Add("@cardKeyHash", SqlDbType.VarBinary, 32).Value = cardKeyHash;
                command.Parameters.Add("@status", SqlDbType.VarChar, 24).Value = status;
                command.Parameters.Add("@lastError", SqlDbType.NVarChar, 500).Value = DbValue(readinessMessage);
                command.ExecuteNonQuery();
            }

            using (var command = new SqlCommand(@"
UPDATE app.CardOperationLog
SET LocalSequence=@sequence,OperationStatus=N'locally_committed',OperationState='locally_committed',
    BalanceAfterRial=@balanceAfter,CompletedAt=COALESCE(CompletedAt,SYSDATETIME())
WHERE CardOperationLogId=@id AND ApplicationUserId=@userId;", connection, transaction))
            {
                command.Parameters.Add("@sequence", SqlDbType.BigInt).Value = localSequence;
                command.Parameters.Add("@balanceAfter", SqlDbType.BigInt).Value = balanceAfterRial.HasValue ? (object)balanceAfterRial.Value : DBNull.Value;
                command.Parameters.Add("@id", SqlDbType.BigInt).Value = operationId;
                command.Parameters.Add("@userId", SqlDbType.Int).Value = AppSession.Current.ApplicationUserId;
                if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("وضعیت ثبت محلی عملیات شارژ ذخیره نشد.");
            }
        }

        public static Guid GetLocalTransactionId(long operationId)
        {
            AuthorizationGuard.Require("cards.recharge");
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT LocalTransactionId FROM app.CardOperationLog WHERE CardOperationLogId=@id AND ApplicationUserId=@userId", connection))
                {
                    command.Parameters.Add("@id", SqlDbType.BigInt).Value = operationId;
                    command.Parameters.Add("@userId", SqlDbType.Int).Value = AppSession.Current.ApplicationUserId;
                    object value = command.ExecuteScalar();
                    if (value == null || value == DBNull.Value) throw new InvalidOperationException("شناسه‌ی پایدار عملیات پیدا نشد.");
                    return (Guid)value;
                }
            }
        }

        public static string GetSnapshotCursor(string branchId)
        {
            EdgeWorkerContext.Require();
            string branch = Clean(branchId, 128);
            if (String.IsNullOrWhiteSpace(branch)) return null;
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT TOP(1) CursorValue FROM app.EdgeSnapshotState WHERE StateId=1 AND BranchId=@branch", connection))
                {
                    command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branch;
                    object value = command.ExecuteScalar();
                    return value == null || value == DBNull.Value ? null : Convert.ToString(value, CultureInfo.InvariantCulture);
                }
            }
        }

        public static void ReplaceSnapshot(EdgeBranchSnapshotDto snapshot, string expectedBranchId)
        {
            EdgeWorkerContext.Require();
            DateTime issuedAtUtc;
            DateTime expiresAtUtc;
            ValidateSnapshot(snapshot, expectedBranchId, out issuedAtUtc, out expiresAtUtc);
            string branchId = snapshot.BranchId.Trim();
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        using (var previous = new SqlCommand("SELECT IssuedAtUtc FROM app.EdgeSnapshotState WITH(UPDLOCK,HOLDLOCK) WHERE StateId=1 AND BranchId=@branch", connection, transaction))
                        {
                            previous.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                            object existingIssuedAt = previous.ExecuteScalar();
                            if (existingIssuedAt != null && existingIssuedAt != DBNull.Value &&
                                DateTime.SpecifyKind(Convert.ToDateTime(existingIssuedAt, CultureInfo.InvariantCulture), DateTimeKind.Utc) > issuedAtUtc)
                                throw new EdgeApiException("stale-snapshot", false);
                        }

                        DeleteAllRows(connection, transaction, "app.EdgeCustomerCache");
                        DeleteAllRows(connection, transaction, "app.EdgeHouseholdCache");
                        DeleteAllRows(connection, transaction, "app.EdgeMemberCache");
                        DeleteAllRows(connection, transaction, "app.EdgeCardMapping");
                        DeleteAllRows(connection, transaction, "app.EdgeDeviceMapping");
                        DeleteAllRows(connection, transaction, "app.EdgePromotionMapping");

                        foreach (EdgeCustomerDto customer in snapshot.Customers)
                            InsertSnapshotRow(connection, transaction,
                                "INSERT app.EdgeCustomerCache(BranchId,CustomerId,DisplayName,HouseholdId,SnapshotVersion) VALUES(@branch,@id,@name,@related,@version)",
                                branchId, customer.CustomerId, customer.DisplayName, customer.HouseholdId, snapshot.Version);
                        foreach (EdgeHouseholdDto household in snapshot.Households)
                            InsertSnapshotRow(connection, transaction,
                                "INSERT app.EdgeHouseholdCache(BranchId,HouseholdId,DisplayName,GuardianCustomerId,SnapshotVersion) VALUES(@branch,@id,@name,@related,@version)",
                                branchId, household.HouseholdId, household.DisplayName, household.GuardianCustomerId, snapshot.Version);
                        foreach (EdgeMemberDto member in snapshot.Members)
                            InsertSnapshotRow(connection, transaction,
                                "INSERT app.EdgeMemberCache(BranchId,MemberId,HouseholdId,CustomerId,DisplayName,SnapshotVersion) VALUES(@branch,@id,@related,@customer,@name,@version)",
                                branchId, member.MemberId, member.HouseholdId, member.CustomerId, member.DisplayName, snapshot.Version);

                        foreach (EdgeCardMappingDto card in snapshot.Cards)
                        {
                            byte[] hash = GetCardKeyHash(ParseCardUid(card.LocalCardUid));
                            using (var command = new SqlCommand(@"
INSERT app.EdgeCardMapping(BranchId,CardKeyHash,CardId,HouseholdId,MemberId,SnapshotVersion)
VALUES(@branch,@hash,@cardId,@household,@member,@version);", connection, transaction))
                            {
                                command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                                command.Parameters.Add("@hash", SqlDbType.VarBinary, 32).Value = hash;
                                command.Parameters.Add("@cardId", SqlDbType.NVarChar, 128).Value = card.CardId.Trim();
                                command.Parameters.Add("@household", SqlDbType.NVarChar, 128).Value = DbValue(Clean(card.HouseholdId, 128));
                                command.Parameters.Add("@member", SqlDbType.NVarChar, 128).Value = DbValue(Clean(card.MemberId, 128));
                                command.Parameters.Add("@version", SqlDbType.NVarChar, 128).Value = snapshot.Version.Trim();
                                command.ExecuteNonQuery();
                            }
                        }

                        foreach (EdgeDeviceMappingDto device in snapshot.Devices)
                            InsertMappingRow(connection, transaction,
                                "INSERT app.EdgeDeviceMapping(BranchId,SourceDeviceId,DeviceId,AttractionId,SnapshotVersion) VALUES(@branch,@source,@central,@attraction,@version)",
                                branchId, device.SourceDeviceId, device.DeviceId, device.AttractionId, snapshot.Version);
                        foreach (EdgePricingRuleDto rule in snapshot.PricingRules)
                            if (rule.PromotionCode.HasValue && !String.IsNullOrWhiteSpace(rule.PromotionId))
                                using (var command = new SqlCommand(@"
INSERT app.EdgePromotionMapping(BranchId,PromotionCode,PromotionId,PricingVersion,SnapshotVersion)
VALUES(@branch,@code,@promotion,@pricing,@snapshot);", connection, transaction))
                                {
                                    command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                                    command.Parameters.Add("@code", SqlDbType.Int).Value = rule.PromotionCode.Value;
                                    command.Parameters.Add("@promotion", SqlDbType.NVarChar, 128).Value = rule.PromotionId.Trim();
                                    command.Parameters.Add("@pricing", SqlDbType.NVarChar, 128).Value = rule.PricingVersion.Trim();
                                    command.Parameters.Add("@snapshot", SqlDbType.NVarChar, 128).Value = snapshot.Version.Trim();
                                    command.ExecuteNonQuery();
                                }

                        using (var command = new SqlCommand(@"
DELETE app.EdgeSnapshotState WHERE StateId=1;
INSERT app.EdgeSnapshotState(StateId,BranchId,SnapshotVersion,IssuedAtUtc,ExpiresAtUtc,CursorValue,ValidatedAtUtc)
VALUES(1,@branch,@version,@issued,@expires,@cursor,SYSUTCDATETIME());
UPDATE app.EdgeSyncState
SET BranchId=@branch,IsOnline=1,LastServerResponseUtc=SYSUTCDATETIME(),LastSnapshotAtUtc=SYSUTCDATETIME(),
    SnapshotVersion=@version,SnapshotExpiresAtUtc=@expires,LastError=NULL,HeartbeatUtc=SYSUTCDATETIME()
WHERE StateId=1;", connection, transaction))
                        {
                            command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                            command.Parameters.Add("@version", SqlDbType.NVarChar, 128).Value = snapshot.Version.Trim();
                            command.Parameters.Add("@issued", SqlDbType.DateTime2).Value = issuedAtUtc;
                            command.Parameters.Add("@expires", SqlDbType.DateTime2).Value = expiresAtUtc;
                            command.Parameters.Add("@cursor", SqlDbType.NVarChar, 1000).Value = DbValue(Clean(snapshot.Cursor, 1000));
                            command.ExecuteNonQuery();
                        }

                        RefreshOutboxMappings(connection, transaction, branchId);
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

        public static IList<EdgeQueuedEvent> GetPendingEvents(bool deviceDatabase, string branchId, int batchSize)
        {
            EdgeWorkerContext.Require();
            if (String.IsNullOrWhiteSpace(branchId)) return new List<EdgeQueuedEvent>();
            batchSize = Math.Max(1, Math.Min(100, batchSize));
            using (var connection = deviceDatabase ? DatabaseManager.CreateDeviceConnection() : DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        if (!deviceDatabase) RefreshOutboxMappings(connection, transaction, branchId);
                        UpdateReadiness(connection, transaction, branchId);
                        var result = new List<EdgeQueuedEvent>();
                        using (var command = new SqlCommand(@"
SELECT TOP(@take) EventId,SourceInstanceId,LocalTransactionId,SourceDatabase,OrganizationId,BranchId,CardId,
       HouseholdId,MemberId,DeviceId,SourceDeviceId,TransactionKind,AmountRial,CurrencyCode,GrossAmountRial,
       DiscountAmountRial,NetAmountRial,PromotionId,PricingVersion,OccurredAtUtc,LocalSequence,BalanceAfterRial,OperationResult,RetryCount
FROM app.EdgeOutbox
WHERE Status IN ('pending','retry') AND BranchId=@branch
  AND OrganizationId IS NOT NULL AND CardId IS NOT NULL
  AND (NextAttemptAtUtc IS NULL OR NextAttemptAtUtc<=SYSUTCDATETIME())
ORDER BY CreatedAtUtc,EventId;", connection, transaction))
                        {
                            command.Parameters.Add("@take", SqlDbType.Int).Value = batchSize;
                            command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                            using (var reader = command.ExecuteReader())
                                while (reader.Read()) result.Add(ReadQueuedEvent(reader, deviceDatabase));
                        }
                        transaction.Commit();
                        return result;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public static void MarkBatchAttempt(IList<EdgeQueuedEvent> events)
        {
            EdgeWorkerContext.Require();
            UpdateBatch(events, @"
UPDATE app.EdgeOutbox SET RetryCount=RetryCount+1,LastAttemptAtUtc=SYSUTCDATETIME(),
    NextAttemptAtUtc=DATEADD(second,@retrySeconds,SYSUTCDATETIME())
WHERE EventId=@eventId AND Status IN ('pending','retry');");
            foreach (EdgeQueuedEvent item in events.Where(row => !row.DeviceDatabase))
                SetLocalOperationState(Guid.Parse(item.Payload.LocalTransactionId), "pending_sync");
        }

        public static void RecordBatchFailure(IList<EdgeQueuedEvent> events, string safeCode, int retrySeconds, bool deadLetter = false)
        {
            EdgeWorkerContext.Require();
            string error = Clean(safeCode, 500) ?? "network-unavailable";
            UpdateBatch(events, @"
UPDATE app.EdgeOutbox SET Status=CASE WHEN @deadLetter=1 THEN 'dead_letter' ELSE 'pending' END,LastError=@safeError,
    NextAttemptAtUtc=CASE WHEN @deadLetter=1 THEN NULL ELSE DATEADD(second,@retrySeconds,SYSUTCDATETIME()) END
WHERE EventId=@eventId AND Status IN ('pending','retry');", error, retrySeconds, deadLetter);
            SetTransportState(false, null, error, false);
        }

        public static void ApplyAcknowledgements(IList<EdgeQueuedEvent> sentEvents, EdgeAcknowledgementResponse response)
        {
            EdgeWorkerContext.Require();
            if (sentEvents == null || sentEvents.Count == 0) return;
            EdgeAcknowledgementBatch batch = EdgeAcknowledgementProcessor.Validate(sentEvents.Select(item => item.EventId).ToList(), response);
            var byId = sentEvents.GroupBy(item => item.EventId).ToDictionary(group => group.Key, group => group.First());
            foreach (EdgeAcknowledgementDto result in batch.ValidResults)
            {
                Guid eventId = Guid.Parse(result.EventId);
                ApplyAcknowledgement(byId[eventId], result.Status, result.Reason);
            }
            var missingIds = new HashSet<Guid>(batch.MissingEventIds);
            var missing = sentEvents.Where(item => missingIds.Contains(item.EventId)).ToList();
            if (missing.Count > 0) RecordBatchFailure(missing, "missing-or-invalid-acknowledgement", 30);
            SetTransportState(true, null, null, batch.ValidResults.Count > 0);
        }

        public static void SaveHeartbeat()
        {
            EdgeWorkerContext.Require();
            SetTransportState(null, null, null, false);
        }

        public static void SetTransportState(bool? online, DateTime? serverResponseUtc, string safeError, bool syncSucceeded)
        {
            EdgeWorkerContext.Require();
            foreach (bool deviceDatabase in new[] { false, true })
            {
                try
                {
                    using (var connection = deviceDatabase ? DatabaseManager.CreateDeviceConnection() : DatabaseManager.CreateMainConnection())
                    {
                        connection.Open();
                        using (var command = new SqlCommand(@"
UPDATE app.EdgeSyncState
SET IsOnline=COALESCE(@online,IsOnline),
    LastServerResponseUtc=COALESCE(@response,LastServerResponseUtc),
    LastSuccessfulSyncUtc=CASE WHEN @success=1 THEN SYSUTCDATETIME() ELSE LastSuccessfulSyncUtc END,
    LastError=CASE WHEN @hasError=1 THEN @error WHEN @clearError=1 THEN NULL ELSE LastError END,
    HeartbeatUtc=SYSUTCDATETIME()
WHERE StateId=1;", connection))
                        {
                            command.Parameters.Add("@online", SqlDbType.Bit).Value = online.HasValue ? (object)online.Value : DBNull.Value;
                            command.Parameters.Add("@response", SqlDbType.DateTime2).Value = serverResponseUtc.HasValue ? (object)serverResponseUtc.Value : DBNull.Value;
                            command.Parameters.Add("@success", SqlDbType.Bit).Value = syncSucceeded;
                            command.Parameters.Add("@hasError", SqlDbType.Bit).Value = !String.IsNullOrWhiteSpace(safeError);
                            command.Parameters.Add("@clearError", SqlDbType.Bit).Value = String.IsNullOrWhiteSpace(safeError) && online == true;
                            command.Parameters.Add("@error", SqlDbType.NVarChar, 500).Value = DbValue(Clean(safeError, 500));
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch when (deviceDatabase)
                {
                    // A device database outage is reported by the diagnostic screen independently.
                }
            }
        }

        public static EdgeSyncDiagnostics GetDiagnostics()
        {
            AuthorizationGuard.Require("edge.sync.view");
            string branchId = Clean(AppSettings.Current.EdgeBranchId, 128);
            var result = new EdgeSyncDiagnostics { BranchId = branchId, Events = CreateEventTable() };
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                result.MainDatabaseAvailable = true;
                using (var command = new SqlCommand(@"
SELECT BranchId,IsOnline,LastServerResponseUtc,LastSuccessfulSyncUtc,LastSnapshotAtUtc,SnapshotVersion,
       SnapshotExpiresAtUtc,LastAcknowledgedEventId,LastError,HeartbeatUtc
FROM app.EdgeSyncState WHERE StateId=1;", connection))
                using (var reader = command.ExecuteReader())
                    if (reader.Read())
                    {
                        result.SnapshotBranchId = reader.IsDBNull(0) ? null : reader.GetString(0);
                        result.IsOnline = reader.GetBoolean(1);
                        result.LastServerResponseUtc = NullableDate(reader, 2);
                        result.LastSuccessfulSyncUtc = NullableDate(reader, 3);
                        result.LastSnapshotAtUtc = NullableDate(reader, 4);
                        result.SnapshotVersion = reader.IsDBNull(5) ? null : reader.GetString(5);
                        result.SnapshotExpiresAtUtc = NullableDate(reader, 6);
                        result.LastAcknowledgedEventId = reader.IsDBNull(7) ? (Guid?)null : reader.GetGuid(7);
                        result.LastError = reader.IsDBNull(8) ? null : reader.GetString(8);
                        result.HeartbeatUtc = NullableDate(reader, 9);
                    }
            }
            foreach (bool deviceDatabase in new[] { false, true })
            {
                try
                {
                    using (var connection = deviceDatabase ? DatabaseManager.CreateDeviceConnection() : DatabaseManager.CreateMainConnection())
                    {
                        connection.Open();
                        if (deviceDatabase) result.DeviceDatabaseAvailable = true;
                        using (var command = new SqlCommand(@"
SELECT
 SUM(CASE WHEN Status IN ('pending','retry','needs_configuration','needs_mapping') THEN 1 ELSE 0 END),
 SUM(CASE WHEN Status IN ('needs_mapping','needs_configuration') THEN 1 ELSE 0 END),
 SUM(CASE WHEN Status='rejected' THEN 1 ELSE 0 END),
 SUM(CASE WHEN Status='dead_letter' THEN 1 ELSE 0 END)
FROM app.EdgeOutbox WHERE ((@branch IS NULL AND BranchId IS NULL) OR BranchId=@branch);", connection))
                    {
                        command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = (object)branchId ?? DBNull.Value;
                        using (var reader = command.ExecuteReader())
                            if (reader.Read())
                            {
                                result.PendingCount += ReadCount(reader, 0);
                                result.NeedsMappingCount += ReadCount(reader, 1);
                                result.RejectedCount += ReadCount(reader, 2);
                                result.DeadLetterCount += ReadCount(reader, 3);
                            }
                    }
                    using (var command = new SqlCommand(@"
SELECT TOP(100) CONVERT(nvarchar(128),EventId),CONVERT(nvarchar(128),LocalTransactionId),TransactionKind,
       AmountRial,Status,RetryCount,LastError,CreatedAtUtc
FROM app.EdgeOutbox WHERE ((@branch IS NULL AND BranchId IS NULL) OR BranchId=@branch) AND Status<>'synced'
ORDER BY CreatedAtUtc DESC;", connection))
                    {
                        command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = (object)branchId ?? DBNull.Value;
                        using (var adapter = new SqlDataAdapter(command))
                        {
                            var rows = new DataTable();
                            adapter.Fill(rows);
                            foreach (DataRow row in rows.Rows)
                            {
                                DataRow output = result.Events.NewRow();
                                output["SourceDatabase"] = deviceDatabase ? DatabaseManager.DeviceCatalog : DatabaseManager.MainCatalog;
                                output["EventId"] = row[0]; output["LocalTransactionId"] = row[1]; output["TransactionKind"] = row[2];
                                output["AmountRial"] = row[3]; output["Status"] = row[4]; output["RetryCount"] = row[5];
                                output["LastError"] = row[6]; output["CreatedAtUtc"] = row[7];
                                result.Events.Rows.Add(output);
                            }
                        }
                    }
                    }
                }
                catch when (deviceDatabase)
                {
                    result.DeviceDatabaseAvailable = false;
                }
            }
            return result;
        }

        public static bool RetryEvent(Guid eventId)
        {
            AuthorizationGuard.Require("edge.sync.retry");
            string branchId = Clean(AppSettings.Current.EdgeBranchId, 128);
            if (String.IsNullOrWhiteSpace(branchId)) return false;
            foreach (bool deviceDatabase in new[] { false, true })
            {
                using (var connection = deviceDatabase ? DatabaseManager.CreateDeviceConnection() : DatabaseManager.CreateMainConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                    {
                        try
                        {
                            if (!deviceDatabase) RefreshOutboxMappings(connection, transaction, branchId);
                            using (var command = new SqlCommand(@"
UPDATE app.EdgeOutbox
SET Status=CASE
      WHEN OrganizationId IS NULL OR BranchId IS NULL THEN 'needs_configuration'
      WHEN CardId IS NULL OR (PromotionCode IS NOT NULL AND PromotionId IS NULL) THEN 'needs_mapping'
      ELSE 'pending' END,
    RetryCount=0,NextAttemptAtUtc=NULL,LastError=NULL,Acknowledgement=NULL,AcknowledgedAtUtc=NULL
WHERE EventId=@eventId AND BranchId=@branch AND Status IN ('rejected','dead_letter','needs_mapping','needs_configuration','pending','retry');", connection, transaction))
                            {
                                command.Parameters.Add("@eventId", SqlDbType.UniqueIdentifier).Value = eventId;
                                command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                                bool changed = command.ExecuteNonQuery() == 1;
                                if (changed && !deviceDatabase)
                                {
                                    using (var operation = new SqlCommand(@"
UPDATE l SET OperationState='pending_sync',OperationStatus=N'در انتظار ارسال دوباره'
FROM app.CardOperationLog l INNER JOIN app.EdgeOutbox o ON o.LocalTransactionId=l.LocalTransactionId
WHERE o.EventId=@eventId;", connection, transaction))
                                    {
                                        operation.Parameters.Add("@eventId", SqlDbType.UniqueIdentifier).Value = eventId;
                                        operation.ExecuteNonQuery();
                                    }
                                }
                                transaction.Commit();
                                if (changed) return true;
                            }
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            return false;
        }

        public static DataTable SearchCustomers(string search, int pageNumber, int pageSize, out int totalCount)
        {
            AuthorizationGuard.Require("customers.view");
            string branchId = RequireBranch();
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, Math.Min(100, pageSize));
            string term = (search ?? String.Empty).Trim();
            if (term.Length > 120) term = term.Substring(0, 120);
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var command = new SqlCommand(@"
SELECT COUNT(1) FROM app.EdgeCustomerCache
WHERE BranchId=@branch AND (@term=N'' OR DisplayName LIKE @pattern OR CustomerId LIKE @pattern);", connection))
                {
                    command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                    command.Parameters.Add("@term", SqlDbType.NVarChar, 120).Value = term;
                    command.Parameters.Add("@pattern", SqlDbType.NVarChar, 250).Value = "%" + term + "%";
                    totalCount = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
                }
                var rows = new DataTable();
                using (var command = new SqlCommand(@"
SELECT CustomerId,DisplayName,HouseholdId,SnapshotVersion
FROM
(
 SELECT CustomerId,DisplayName,HouseholdId,SnapshotVersion,
        ROW_NUMBER() OVER(ORDER BY DisplayName,CustomerId) AS RowNumber
 FROM app.EdgeCustomerCache
 WHERE BranchId=@branch AND (@term=N'' OR DisplayName LIKE @pattern OR CustomerId LIKE @pattern)
) AS Customers
WHERE RowNumber>@first AND RowNumber<=@last ORDER BY RowNumber;", connection))
                {
                    command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                    command.Parameters.Add("@term", SqlDbType.NVarChar, 120).Value = term;
                    command.Parameters.Add("@pattern", SqlDbType.NVarChar, 250).Value = "%" + term + "%";
                    command.Parameters.Add("@first", SqlDbType.BigInt).Value = (long)(pageNumber - 1) * pageSize;
                    command.Parameters.Add("@last", SqlDbType.BigInt).Value = (long)pageNumber * pageSize;
                    using (var adapter = new SqlDataAdapter(command)) adapter.Fill(rows);
                }
                return rows;
            }
        }

        public static EdgeSnapshotStatus GetCustomerSnapshotStatus()
        {
            AuthorizationGuard.Require("customers.view");
            string branchId = RequireBranch();
            var result = new EdgeSnapshotStatus { BranchId = branchId };
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var command = new SqlCommand(@"
SELECT SnapshotVersion,IssuedAtUtc,ExpiresAtUtc
FROM app.EdgeSnapshotState WHERE StateId=1 AND BranchId=@branch;", connection))
                {
                    command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                    using (var reader = command.ExecuteReader())
                        if (reader.Read())
                        {
                            result.Version = reader.GetString(0);
                            result.IssuedAtUtc = NullableDate(reader, 1);
                            result.ExpiresAtUtc = NullableDate(reader, 2);
                            result.IsFresh = result.ExpiresAtUtc.HasValue && result.ExpiresAtUtc.Value > DateTime.UtcNow;
                        }
                }
            }
            return result;
        }

        public static DataTable GetHousehold(string householdId)
        {
            AuthorizationGuard.Require("customers.view");
            AuthorizationGuard.Require("families.view");
            string branchId = RequireBranch();
            string household = Clean(householdId, 128);
            if (String.IsNullOrWhiteSpace(household)) throw new ArgumentException("شناسه‌ی خانواده مشخص نشده است.", "householdId");
            var result = new DataTable();
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var command = new SqlCommand(@"
SELECT N'خانواده' AS MemberType,HouseholdId AS MemberId,DisplayName,GuardianCustomerId AS RelatedCustomerId
FROM app.EdgeHouseholdCache WHERE BranchId=@branch AND HouseholdId=@household
UNION ALL
SELECT N'عضو' AS MemberType,MemberId,DisplayName,CustomerId
FROM app.EdgeMemberCache WHERE BranchId=@branch AND HouseholdId=@household
ORDER BY MemberType,DisplayName;", connection))
                {
                    command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                    command.Parameters.Add("@household", SqlDbType.NVarChar, 128).Value = household;
                    using (var adapter = new SqlDataAdapter(command)) adapter.Fill(result);
                }
            }
            return result;
        }

        public static DataTable GetHouseholdCards(string householdId)
        {
            AuthorizationGuard.Require("families.view");
            string branchId = RequireBranch();
            string household = Clean(householdId, 128);
            var result = new DataTable();
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var command = new SqlCommand(@"
SELECT CardId,HouseholdId,MemberId,SnapshotVersion
FROM app.EdgeCardMapping WHERE BranchId=@branch AND HouseholdId=@household ORDER BY MemberId,CardId;", connection))
                {
                    command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                    command.Parameters.Add("@household", SqlDbType.NVarChar, 128).Value = household;
                    using (var adapter = new SqlDataAdapter(command)) adapter.Fill(result);
                }
            }
            return result;
        }

        private static void ApplyAcknowledgement(EdgeQueuedEvent item, string status, string reason)
        {
            using (var connection = item.DeviceDatabase ? DatabaseManager.CreateDeviceConnection() : DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        string storedStatus = status == "rejected" ? "rejected" : "synced";
                        using (var command = new SqlCommand(@"
UPDATE app.EdgeOutbox
SET Status=@status,Acknowledgement=@ack,AcknowledgedAtUtc=SYSUTCDATETIME(),LastError=@reason,NextAttemptAtUtc=NULL
WHERE EventId=@eventId;", connection, transaction))
                        {
                            command.Parameters.Add("@status", SqlDbType.VarChar, 24).Value = storedStatus;
                            command.Parameters.Add("@ack", SqlDbType.VarChar, 16).Value = status;
                            command.Parameters.Add("@reason", SqlDbType.NVarChar, 500).Value = DbValue(SafeServerReason(reason));
                            command.Parameters.Add("@eventId", SqlDbType.UniqueIdentifier).Value = item.EventId;
                            command.ExecuteNonQuery();
                        }
                        if (!item.DeviceDatabase)
                        {
                            using (var command = new SqlCommand(@"
UPDATE app.CardOperationLog SET OperationState=@state,OperationStatus=@state
WHERE LocalTransactionId=@localId;", connection, transaction))
                            {
                                command.Parameters.Add("@state", SqlDbType.VarChar, 24).Value = storedStatus == "synced" ? "synced" : "rejected";
                                command.Parameters.Add("@localId", SqlDbType.UniqueIdentifier).Value = Guid.Parse(item.Payload.LocalTransactionId);
                                command.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            if (status != "rejected") SetLastAcknowledged(item.EventId);
        }

        private static void SetLastAcknowledged(Guid eventId)
        {
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var command = new SqlCommand(@"
UPDATE app.EdgeSyncState SET LastAcknowledgedEventId=@eventId,LastSuccessfulSyncUtc=SYSUTCDATETIME()
WHERE StateId=1;", connection))
                {
                    command.Parameters.Add("@eventId", SqlDbType.UniqueIdentifier).Value = eventId;
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void SetLocalOperationState(Guid localTransactionId, string state)
        {
            EdgeWorkerContext.Require();
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var command = new SqlCommand(@"
UPDATE app.CardOperationLog SET OperationState=@state,OperationStatus=@state
WHERE LocalTransactionId=@localId AND OperationName=N'card-recharge';", connection))
                {
                    command.Parameters.Add("@state", SqlDbType.VarChar, 24).Value = state;
                    command.Parameters.Add("@localId", SqlDbType.UniqueIdentifier).Value = localTransactionId;
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void SetOperationState(SqlConnection connection, SqlTransaction transaction, long operationId, string state, string status)
        {
            using (var command = new SqlCommand(@"
UPDATE app.CardOperationLog SET OperationState=@state,OperationStatus=@status,CompletedAt=SYSDATETIME()
WHERE CardOperationLogId=@id AND OperationName=N'card-recharge';", connection, transaction))
            {
                command.Parameters.Add("@state", SqlDbType.VarChar, 24).Value = state;
                command.Parameters.Add("@status", SqlDbType.NVarChar, 40).Value = status;
                command.Parameters.Add("@id", SqlDbType.BigInt).Value = operationId;
                command.ExecuteNonQuery();
            }
        }

        private static void UpdateBatch(IList<EdgeQueuedEvent> events, string sql, string error = null, int retrySeconds = 30, bool deadLetter = false)
        {
            if (events == null) return;
            foreach (EdgeQueuedEvent item in events)
            {
                using (var connection = item.DeviceDatabase ? DatabaseManager.CreateDeviceConnection() : DatabaseManager.CreateMainConnection())
                {
                    connection.Open();
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add("@eventId", SqlDbType.UniqueIdentifier).Value = item.EventId;
                        command.Parameters.Add("@retrySeconds", SqlDbType.Int).Value = Math.Max(1, Math.Min(3600, retrySeconds));
                        if (sql.IndexOf("@safeError", StringComparison.Ordinal) >= 0)
                            command.Parameters.Add("@safeError", SqlDbType.NVarChar, 500).Value = DbValue(error);
                        if (sql.IndexOf("@deadLetter", StringComparison.Ordinal) >= 0)
                            command.Parameters.Add("@deadLetter", SqlDbType.Bit).Value = deadLetter;
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private static void ApplyMappingToOutbox(SqlConnection connection, SqlTransaction transaction, string branchId)
        {
            if (String.IsNullOrWhiteSpace(branchId)) return;
            RefreshOutboxMappings(connection, transaction, branchId);
            UpdateReadiness(connection, transaction, branchId);
        }

        private static void RefreshOutboxMappings(SqlConnection connection, SqlTransaction transaction, string branchId)
        {
            if (String.IsNullOrWhiteSpace(branchId)) return;
            using (var command = new SqlCommand(@"
UPDATE o
SET CardId=m.CardId,
    HouseholdId=COALESCE(o.HouseholdId,m.HouseholdId),
    MemberId=COALESCE(o.MemberId,m.MemberId)
FROM app.EdgeOutbox o
INNER JOIN app.EdgeCardMapping m ON m.BranchId=o.BranchId AND m.CardKeyHash=o.CardKeyHash
WHERE o.BranchId=@branch AND o.Status IN ('pending','retry','needs_mapping','needs_configuration','rejected');
UPDATE o SET PromotionId=p.PromotionId,PricingVersion=p.PricingVersion
FROM app.EdgeOutbox o
INNER JOIN app.EdgePromotionMapping p ON p.BranchId=o.BranchId AND p.PromotionCode=o.PromotionCode
WHERE o.BranchId=@branch AND o.PromotionCode IS NOT NULL
  AND o.Status IN ('pending','retry','needs_mapping','needs_configuration','rejected');", connection, transaction))
            {
                command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                command.ExecuteNonQuery();
            }
        }

        private static void UpdateReadiness(SqlConnection connection, SqlTransaction transaction, string branchId)
        {
            using (var command = new SqlCommand(@"
UPDATE app.EdgeOutbox
SET Status=CASE
  WHEN OrganizationId IS NULL OR BranchId IS NULL THEN 'needs_configuration'
  WHEN CardId IS NULL OR (PromotionCode IS NOT NULL AND PromotionId IS NULL) THEN 'needs_mapping'
  ELSE 'pending' END,
    LastError=CASE
      WHEN OrganizationId IS NULL OR BranchId IS NULL THEN N'شناسه‌ی سازمان یا شعبه تنظیم نشده است.'
      WHEN CardId IS NULL OR (PromotionCode IS NOT NULL AND PromotionId IS NULL) THEN N'نگاشت کارت یا جشنواره در تصویر شعبه موجود نیست.'
      ELSE NULL END
WHERE BranchId=@branch AND Status IN ('pending','retry','needs_configuration','needs_mapping');", connection, transaction))
            {
                command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                command.ExecuteNonQuery();
            }
        }

        private static EdgeQueuedEvent ReadQueuedEvent(SqlDataReader reader, bool deviceDatabase)
        {
            var payload = new EdgeEventDto
            {
                EventId = reader.GetGuid(0).ToString("D"),
                SourceInstanceId = reader.GetGuid(1).ToString("D"),
                LocalTransactionId = reader.GetGuid(2).ToString("D"),
                SourceDatabase = reader.GetString(3),
                OrganizationId = reader.GetString(4),
                BranchId = reader.GetString(5),
                CardId = reader.GetString(6),
                HouseholdId = reader.IsDBNull(7) ? null : reader.GetString(7),
                MemberId = reader.IsDBNull(8) ? null : reader.GetString(8),
                DeviceId = reader.IsDBNull(9) ? null : reader.GetString(9),
                SourceDeviceId = reader.IsDBNull(10) ? null : reader.GetString(10),
                TransactionKind = reader.GetString(11),
                AmountRial = reader.GetInt64(12),
                CurrencyCode = reader.GetString(13),
                GrossAmountRial = reader.GetInt64(14),
                DiscountAmountRial = reader.GetInt64(15),
                NetAmountRial = reader.GetInt64(16),
                PromotionId = reader.IsDBNull(17) ? null : reader.GetString(17),
                PricingVersion = reader.GetString(18),
                OccurredAt = DateTime.SpecifyKind(reader.GetDateTime(19), DateTimeKind.Utc).ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture),
                LocalSequence = reader.GetInt64(20),
                BalanceAfterRial = reader.IsDBNull(21) ? (long?)null : reader.GetInt64(21),
                OperationResult = reader.GetString(22)
            };
            return new EdgeQueuedEvent { EventId = Guid.Parse(payload.EventId), DeviceDatabase = deviceDatabase, RetryCount = reader.GetInt32(23), Payload = payload };
        }

        private static void AllocateSequence(SqlConnection connection, SqlTransaction transaction, Guid sourceInstanceId, byte[] cardKeyHash, out long sequence)
        {
            using (var command = new SqlCommand(@"
DECLARE @current bigint;
SELECT @current=LastSequence FROM app.EdgeCardSequence WITH(UPDLOCK,HOLDLOCK)
WHERE SourceInstanceId=@source AND CardKeyHash=@hash;
IF @current IS NULL
BEGIN
 SET @current=1;
 INSERT app.EdgeCardSequence(SourceInstanceId,CardKeyHash,LastSequence) VALUES(@source,@hash,@current);
END
ELSE
BEGIN
 SET @current=@current+1;
 UPDATE app.EdgeCardSequence SET LastSequence=@current WHERE SourceInstanceId=@source AND CardKeyHash=@hash;
END;
SELECT @current;", connection, transaction))
            {
                command.Parameters.Add("@source", SqlDbType.UniqueIdentifier).Value = sourceInstanceId;
                command.Parameters.Add("@hash", SqlDbType.VarBinary, 32).Value = cardKeyHash;
                sequence = Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        private static long AllocateSequence(SqlConnection connection, SqlTransaction transaction, Guid sourceInstanceId, byte[] cardKeyHash)
        {
            long sequence;
            AllocateSequence(connection, transaction, sourceInstanceId, cardKeyHash, out sequence);
            return sequence;
        }

        private static string GetPricingVersion(SqlConnection connection, string branchId, RechargeOperationTerms terms)
        {
            if (!String.IsNullOrWhiteSpace(branchId))
            {
                using (var command = new SqlCommand("SELECT TOP(1) SnapshotVersion FROM app.EdgeSnapshotState WHERE StateId=1 AND BranchId=@branch AND ExpiresAtUtc>SYSUTCDATETIME()", connection))
                {
                    command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                    object value = command.ExecuteScalar();
                    if (value != null && value != DBNull.Value) return Convert.ToString(value, CultureInfo.InvariantCulture);
                }
            }
            return BuildLocalPricingVersion(terms);
        }

        private static string BuildLocalPricingVersion(RechargeOperationTerms terms)
        {
            return "local-" + terms.DiscountTierId.ToString(CultureInfo.InvariantCulture) + "-" +
                terms.DailyDiscountPercent.ToString(CultureInfo.InvariantCulture) + "-" +
                terms.PromotionCode.ToString(CultureInfo.InvariantCulture) + "-" +
                terms.PromotionDiscountPercent.ToString(CultureInfo.InvariantCulture);
        }

        private static Guid GetSourceInstanceId(AppSettings settings)
        {
            Guid sourceInstanceId;
            if (settings == null || !Guid.TryParse(settings.EdgeSourceInstanceId, out sourceInstanceId))
                throw new InvalidOperationException("شناسه‌ی این نصب معتبر نیست؛ تنظیمات برنامه را دوباره ذخیره کنید.");
            return sourceInstanceId;
        }

        private static string RequireBranch()
        {
            string branch = Clean(AppSettings.Current.EdgeBranchId, 128);
            if (String.IsNullOrWhiteSpace(branch)) throw new InvalidOperationException("شناسه‌ی شعبه برای مشاهده‌ی اطلاعات مشتری تنظیم نشده است.");
            return branch;
        }

        internal static void ValidateSnapshot(EdgeBranchSnapshotDto snapshot, string expectedBranchId, out DateTime issuedAtUtc, out DateTime expiresAtUtc)
        {
            if (snapshot == null || String.IsNullOrWhiteSpace(snapshot.Version) || String.IsNullOrWhiteSpace(snapshot.BranchId) ||
                String.IsNullOrWhiteSpace(expectedBranchId) || !String.Equals(snapshot.BranchId.Trim(), expectedBranchId.Trim(), StringComparison.Ordinal) ||
                snapshot.Devices == null || snapshot.PricingRules == null || snapshot.Customers == null || snapshot.Households == null ||
                snapshot.Members == null || snapshot.Cards == null || snapshot.Version.Trim().Length > 128)
                throw new EdgeApiException("invalid-snapshot", false);
            if (!TryParseUtc(snapshot.IssuedAt, out issuedAtUtc) || !TryParseUtc(snapshot.ExpiresAt, out expiresAtUtc) ||
                expiresAtUtc <= issuedAtUtc || expiresAtUtc <= DateTime.UtcNow || issuedAtUtc > DateTime.UtcNow.AddMinutes(10))
                throw new EdgeApiException("invalid-snapshot-time", false);
            if (snapshot.Cursor != null && snapshot.Cursor.Length > 1000) throw new EdgeApiException("invalid-snapshot", false);

            var customerIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var customerHouseholds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (EdgeCustomerDto row in snapshot.Customers)
                if (row == null || !ValidId(row.CustomerId, 128) || !ValidText(row.DisplayName, 160) || !customerIds.Add(row.CustomerId.Trim()))
                    throw new EdgeApiException("invalid-snapshot", false);
                else customerHouseholds.Add(row.CustomerId.Trim(), Clean(row.HouseholdId, 128));
            var householdIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (EdgeHouseholdDto row in snapshot.Households)
                if (row == null || !ValidId(row.HouseholdId, 128) || !ValidText(row.DisplayName, 160) || !householdIds.Add(row.HouseholdId.Trim()))
                    throw new EdgeApiException("invalid-snapshot", false);

            var memberIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var memberHouseholds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (EdgeMemberDto row in snapshot.Members)
                if (row == null || !ValidId(row.MemberId, 128) || !ValidId(row.HouseholdId, 128) || !ValidText(row.DisplayName, 160) || !memberIds.Add(row.MemberId.Trim()))
                    throw new EdgeApiException("invalid-snapshot", false);
                else memberHouseholds.Add(row.MemberId.Trim(), row.HouseholdId.Trim());

            foreach (EdgeCustomerDto row in snapshot.Customers)
                if (!String.IsNullOrWhiteSpace(row.HouseholdId) && !householdIds.Contains(row.HouseholdId.Trim()))
                    throw new EdgeApiException("invalid-snapshot", false);
            foreach (EdgeHouseholdDto row in snapshot.Households)
                if (!String.IsNullOrWhiteSpace(row.GuardianCustomerId) && !customerIds.Contains(row.GuardianCustomerId.Trim()))
                    throw new EdgeApiException("invalid-snapshot", false);
            foreach (EdgeMemberDto row in snapshot.Members)
            {
                if (!householdIds.Contains(row.HouseholdId.Trim()) ||
                    (!String.IsNullOrWhiteSpace(row.CustomerId) && !customerIds.Contains(row.CustomerId.Trim())))
                    throw new EdgeApiException("invalid-snapshot", false);
                string customerHousehold;
                if (!String.IsNullOrWhiteSpace(row.CustomerId) && customerHouseholds.TryGetValue(row.CustomerId.Trim(), out customerHousehold) &&
                    !String.IsNullOrWhiteSpace(customerHousehold) && !String.Equals(customerHousehold, row.HouseholdId, StringComparison.OrdinalIgnoreCase))
                    throw new EdgeApiException("invalid-snapshot", false);
            }

            var cardKeys = new HashSet<string>(StringComparer.Ordinal);
            var centralCardIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (EdgeCardMappingDto row in snapshot.Cards)
            {
                if (row == null || !ValidId(row.CardId, 128) || !centralCardIds.Add(row.CardId.Trim()))
                    throw new EdgeApiException("invalid-snapshot", false);
                byte[] uid;
                try { uid = ParseCardUid(row.LocalCardUid); }
                catch { throw new EdgeApiException("invalid-snapshot", false); }
                string hash;
                using (var sha = SHA256.Create()) hash = Convert.ToBase64String(sha.ComputeHash(uid));
                if (!cardKeys.Add(hash)) throw new EdgeApiException("invalid-snapshot", false);
                if (!String.IsNullOrWhiteSpace(row.HouseholdId) && !householdIds.Contains(row.HouseholdId.Trim()))
                    throw new EdgeApiException("invalid-snapshot", false);
                if (!String.IsNullOrWhiteSpace(row.MemberId))
                {
                    string memberHousehold;
                    if (!memberHouseholds.TryGetValue(row.MemberId.Trim(), out memberHousehold) ||
                        String.IsNullOrWhiteSpace(row.HouseholdId) ||
                        !String.Equals(memberHousehold, row.HouseholdId.Trim(), StringComparison.OrdinalIgnoreCase))
                        throw new EdgeApiException("invalid-snapshot", false);
                }
            }
            var localDeviceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var centralDeviceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (EdgeDeviceMappingDto row in snapshot.Devices)
                if (row == null || !ValidId(row.SourceDeviceId, 128) || !ValidId(row.DeviceId, 128) ||
                    !localDeviceIds.Add(row.SourceDeviceId.Trim()) || !centralDeviceIds.Add(row.DeviceId.Trim()))
                    throw new EdgeApiException("invalid-snapshot", false);
            var promotionCodes = new HashSet<int>();
            foreach (EdgePricingRuleDto row in snapshot.PricingRules)
                if (row == null || !ValidId(row.PricingVersion, 128) ||
                    (row.PromotionCode.HasValue && (!ValidId(row.PromotionId, 128) || !promotionCodes.Add(row.PromotionCode.Value))))
                    throw new EdgeApiException("invalid-snapshot", false);
        }

        private static bool TryParseUtc(string value, out DateTime result)
        {
            DateTimeOffset parsed;
            bool success = DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out parsed);
            result = success ? parsed.UtcDateTime : DateTime.MinValue;
            return success;
        }

        private static byte[] ParseCardUid(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) throw new FormatException();
            string hex = value.Trim().Replace("-", String.Empty).Replace(":", String.Empty);
            if (hex.Length != 8) throw new FormatException();
            var result = new byte[4];
            for (int i = 0; i < result.Length; i++) result[i] = Byte.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return result;
        }

        private static bool ValidId(string value, int maxLength)
        {
            return !String.IsNullOrWhiteSpace(value) && value.Trim().Length <= maxLength && value.IndexOf('\0') < 0;
        }

        private static bool ValidText(string value, int maxLength)
        {
            return !String.IsNullOrWhiteSpace(value) && value.Trim().Length <= maxLength && value.IndexOf('\0') < 0;
        }

        private static void DeleteAllRows(SqlConnection connection, SqlTransaction transaction, string table)
        {
            // Table names are selected only from the fixed migration-owned allow-list above.
            using (var command = new SqlCommand("DELETE FROM " + table, connection, transaction))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void InsertSnapshotRow(SqlConnection connection, SqlTransaction transaction, string sql, string branchId,
            string id, string name, string relatedId, string version)
        {
            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                command.Parameters.Add("@id", SqlDbType.NVarChar, 128).Value = id.Trim();
                command.Parameters.Add("@name", SqlDbType.NVarChar, 160).Value = name.Trim();
                command.Parameters.Add("@related", SqlDbType.NVarChar, 128).Value = DbValue(Clean(relatedId, 128));
                command.Parameters.Add("@version", SqlDbType.NVarChar, 128).Value = version.Trim();
                command.ExecuteNonQuery();
            }
        }

        private static void InsertSnapshotRow(SqlConnection connection, SqlTransaction transaction, string sql, string branchId,
            string id, string householdId, string customerId, string name, string version)
        {
            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                command.Parameters.Add("@id", SqlDbType.NVarChar, 128).Value = id.Trim();
                command.Parameters.Add("@related", SqlDbType.NVarChar, 128).Value = householdId.Trim();
                command.Parameters.Add("@customer", SqlDbType.NVarChar, 128).Value = DbValue(Clean(customerId, 128));
                command.Parameters.Add("@name", SqlDbType.NVarChar, 160).Value = name.Trim();
                command.Parameters.Add("@version", SqlDbType.NVarChar, 128).Value = version.Trim();
                command.ExecuteNonQuery();
            }
        }

        private static void InsertMappingRow(SqlConnection connection, SqlTransaction transaction, string sql, string branchId,
            string sourceId, string centralId, string relatedId, string version)
        {
            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branchId;
                command.Parameters.Add("@source", SqlDbType.NVarChar, 128).Value = sourceId.Trim();
                command.Parameters.Add("@central", SqlDbType.NVarChar, 128).Value = centralId.Trim();
                command.Parameters.Add("@attraction", SqlDbType.NVarChar, 128).Value = DbValue(Clean(relatedId, 128));
                command.Parameters.Add("@version", SqlDbType.NVarChar, 128).Value = version.Trim();
                command.ExecuteNonQuery();
            }
        }

        private static long ReadCount(SqlDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? 0 : Convert.ToInt64(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
        }

        private static DateTime? NullableDate(SqlDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? (DateTime?)null : DateTime.SpecifyKind(reader.GetDateTime(ordinal), DateTimeKind.Utc);
        }

        private static DataTable CreateEventTable()
        {
            var table = new DataTable();
            table.Columns.Add("SourceDatabase", typeof(string));
            table.Columns.Add("EventId", typeof(string));
            table.Columns.Add("LocalTransactionId", typeof(string));
            table.Columns.Add("TransactionKind", typeof(string));
            table.Columns.Add("AmountRial", typeof(long));
            table.Columns.Add("Status", typeof(string));
            table.Columns.Add("RetryCount", typeof(int));
            table.Columns.Add("LastError", typeof(string));
            table.Columns.Add("CreatedAtUtc", typeof(DateTime));
            return table;
        }

        private static int ReadCountFromRow(SqlDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? 0 : Convert.ToInt32(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
        }

        private static string Clean(string value, int maxLength)
        {
            if (value == null) return null;
            string result = value.Trim();
            if (result.Length > maxLength) result = result.Substring(0, maxLength);
            return result.IndexOf('\0') >= 0 ? result.Replace("\0", String.Empty) : result;
        }

        private static string SafeServerReason(string reason)
        {
            string value = Clean(reason, 80);
            if (String.IsNullOrWhiteSpace(value)) return null;
            foreach (char character in value)
                if (!((character >= 'a' && character <= 'z') || (character >= 'A' && character <= 'Z') ||
                      (character >= '0' && character <= '9') || character == '-' || character == '_' || character == '.'))
                    return "server-rejected";
            return value;
        }

        private static object DbValue(object value) { return value ?? DBNull.Value; }

        private static string DbValue(string value) { return String.IsNullOrEmpty(value) ? (string)null : value; }
    }
}
