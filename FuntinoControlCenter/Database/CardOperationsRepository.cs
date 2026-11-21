using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using FuntinoControlCenter.Security;
using FuntinoControlCenter.Services;

namespace FuntinoControlCenter.Database
{
    internal enum CardOperationKind
    {
        Information,
        Balance,
        Remainder,
        History,
        Recharge
    }

    internal sealed class RechargeHistoryPage
    {
        public DataTable Rows { get; private set; }
        public int TotalCount { get; private set; }
        public int PageNumber { get; private set; }
        public int PageSize { get; private set; }

        public RechargeHistoryPage(DataTable rows, int totalCount, int pageNumber, int pageSize)
        {
            Rows = rows;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }

    internal sealed class RechargeReconciliationCandidate
    {
        public long OperationId { get; set; }
        public long AmountToman { get; set; }
        public long? StartingBalanceToman { get; set; }
        public long? ExpectedBalanceToman { get; set; }
        public byte[] CardKeyHash { get; set; }
        public RechargeOperationTerms Terms { get; set; }
        public string State { get; set; }
    }

    internal static class CardOperationsRepository
    {
        public const int HistoryPageSize = 20;

        public static long Begin(CardOperationKind kind, long? requestedAmount)
        {
            RequireOperation(kind);
            var session = AppSession.Current;
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        long id;
                        using (var command = new SqlCommand(@"
INSERT app.CardOperationLog
 (ApplicationUserId,OperationName,RequestedAmount,OperationStatus,Details,OrganizationId,BranchId,OperatorUserName,SourceInstanceId,OccurredAtUtc)
VALUES
 (@userId,@operation,@amount,N'started',N'درخواست عملیات از نرم‌افزار',@organizationId,@branchId,@userName,@sourceInstanceId,SYSUTCDATETIME());
SELECT CAST(SCOPE_IDENTITY() AS bigint);", connection, transaction))
                        {
                            command.Parameters.Add("@userId", SqlDbType.Int).Value = session.ApplicationUserId;
                            command.Parameters.Add("@operation", SqlDbType.NVarChar, 40).Value = OperationName(kind);
                            command.Parameters.Add("@amount", SqlDbType.BigInt).Value = requestedAmount.HasValue ? (object)requestedAmount.Value : DBNull.Value;
                            AppSettings settings = AppSettings.Current;
                            Guid sourceInstanceId;
                            if (!Guid.TryParse(settings.EdgeSourceInstanceId, out sourceInstanceId)) sourceInstanceId = Guid.NewGuid();
                            command.Parameters.Add("@organizationId", SqlDbType.NVarChar, 128).Value = String.IsNullOrWhiteSpace(settings.EdgeOrganizationId) ? (object)DBNull.Value : settings.EdgeOrganizationId.Trim();
                            command.Parameters.Add("@branchId", SqlDbType.NVarChar, 128).Value = String.IsNullOrWhiteSpace(settings.EdgeBranchId) ? (object)DBNull.Value : settings.EdgeBranchId.Trim();
                            command.Parameters.Add("@userName", SqlDbType.NVarChar, 64).Value = session.UserName;
                            command.Parameters.Add("@sourceInstanceId", SqlDbType.UniqueIdentifier).Value = sourceInstanceId;
                            id = Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture);
                        }
                        AuditService.Write(connection, transaction, "card-operation-start", "CardOperationLog", id.ToString(CultureInfo.InvariantCulture), OperationName(kind));
                        transaction.Commit();
                        return id;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public static void Update(long operationId, CardOperationKind kind, string cardUid, string status, string details)
        {
            RequireOperation(kind);
            if (operationId <= 0) throw new ArgumentOutOfRangeException("operationId");
            if (String.IsNullOrWhiteSpace(status)) throw new ArgumentException("وضعیت عملیات مشخص نشده است.", "status");

            bool finished = !String.Equals(status, "started", StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(status, "increment_accepted", StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(status, "transfer_sent", StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(status, "card_written", StringComparison.OrdinalIgnoreCase);
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand(@"
UPDATE app.CardOperationLog
SET CardUid=COALESCE(@cardUid,CardUid),OperationStatus=@status,Details=@details,
    OperationState=CASE
      WHEN @edgeState='card_written' THEN 'card_written'
      WHEN @edgeState='verified' THEN 'verified'
      WHEN @edgeState='unknown' THEN 'unknown'
      WHEN @edgeState='rejected' THEN 'rejected'
      ELSE OperationState END,
    CompletedAt=CASE WHEN @finished=1 THEN SYSDATETIME() ELSE CompletedAt END
WHERE CardOperationLogId=@id AND ApplicationUserId=@userId AND OperationName=@operation;", connection, transaction))
                        {
                            command.Parameters.Add("@cardUid", SqlDbType.VarChar, 11).Value = (object)MaskUid(cardUid) ?? DBNull.Value;
                            command.Parameters.Add("@status", SqlDbType.NVarChar, 40).Value = status;
                            command.Parameters.Add("@edgeState", SqlDbType.VarChar, 24).Value = NormalizeOperationState(status);
                            command.Parameters.Add("@details", SqlDbType.NVarChar, 1000).Value = (object)Limit(details, 1000) ?? DBNull.Value;
                            command.Parameters.Add("@finished", SqlDbType.Bit).Value = finished;
                            command.Parameters.Add("@id", SqlDbType.BigInt).Value = operationId;
                            command.Parameters.Add("@userId", SqlDbType.Int).Value = AppSession.Current.ApplicationUserId;
                            command.Parameters.Add("@operation", SqlDbType.NVarChar, 40).Value = OperationName(kind);
                            if (command.ExecuteNonQuery() != 1) throw new UnauthorizedAccessException("ثبت نتیجه‌ی عملیات کارت مجاز نیست.");
                        }
                        AuditService.Write(connection, transaction, "card-operation-result", "CardOperationLog", operationId.ToString(CultureInfo.InvariantCulture), status + ": " + Limit(details, 700));
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

        public static void PrepareRecharge(long operationId, byte[] cardUid, long startingBalanceToman, long expectedBalanceToman, RechargeOperationTerms terms)
        {
            RequireOperation(CardOperationKind.Recharge);
            EdgeIntegrationRepository.PrepareRecharge(operationId, cardUid, startingBalanceToman, expectedBalanceToman, terms);
        }

        public static void RecordRecharge(long operationId, byte[] cardUid, long amountToman, RechargeOperationTerms terms)
        {
            AuthorizationGuard.Require("cards.recharge");
            AuthorizationGuard.Require("data.finance.create");
            AuthorizationGuard.Require("operations.recharge_terms.view");
            AuthorizationGuard.Require("edge.events.create");
            if (operationId <= 0) throw new ArgumentOutOfRangeException("operationId");
            if (cardUid == null || cardUid.Length != 4) throw new ArgumentException("شناسه‌ی کارت معتبر نیست.", "cardUid");
            if (terms == null) throw new ArgumentNullException("terms");
            if (amountToman <= 0 || amountToman % 100 != 0 || amountToman > terms.MaximumAmount)
                throw new ArgumentOutOfRangeException("amountToman", "مبلغ شارژ با سقف یا گام مجاز تنظیم‌شده هم‌خوانی ندارد.");

            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string originalOperator = AppSession.Current.UserName;
                        if (EdgeIntegrationRepository.HasOutboxForOperation(connection, transaction, operationId))
                        {
                            transaction.Commit();
                            return;
                        }
                        using (var stateCommand = new SqlCommand(@"
SELECT OperationState,OperatorUserName FROM app.CardOperationLog
WHERE CardOperationLogId=@id AND OperationName=N'card-recharge'
  AND ((@branch=N'' AND BranchId IS NULL) OR BranchId=@branch);", connection, transaction))
                        {
                            stateCommand.Parameters.Add("@id", SqlDbType.BigInt).Value = operationId;
                            stateCommand.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = AppSettings.Current.EdgeBranchId ?? String.Empty;
                            using (var reader = stateCommand.ExecuteReader())
                            {
                                if (!reader.Read() || !String.Equals(reader.GetString(0), "verified", StringComparison.Ordinal))
                                    throw new InvalidOperationException("پیش از ثبت مالی باید موجودی نوشته‌شده روی کارت دوباره خوانده و تأیید شود.");
                                if (!reader.IsDBNull(1)) originalOperator = reader.GetString(1);
                            }
                        }
                        using (var command = new SqlCommand("business.usp_FuntinoPark_CreateRechargeTransaction", connection, transaction))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandTimeout = 30;
                            command.Parameters.Add("@Price", SqlDbType.BigInt).Value = amountToman;
                            command.Parameters.Add("@Code1", SqlDbType.Int).Value = (int)cardUid[0];
                            command.Parameters.Add("@Code2", SqlDbType.Int).Value = (int)cardUid[1];
                            command.Parameters.Add("@Code3", SqlDbType.Int).Value = (int)cardUid[2];
                            command.Parameters.Add("@Code4", SqlDbType.Int).Value = (int)cardUid[3];
                            command.Parameters.Add("@FestivalCode", SqlDbType.Int).Value = terms.PromotionCode;
                            command.Parameters.Add("@Disscount", SqlDbType.Int).Value = terms.PromotionDiscountPercent;
                            command.Parameters.Add("@Username", SqlDbType.NVarChar, 64).Value = originalOperator;
                            command.Parameters.Add("@SharjTime", SqlDbType.DateTime).Value = DBNull.Value;
                            command.Parameters.Add("@DiscountPercent", SqlDbType.Int).Value = terms.DiscountTierId;
                            command.Parameters.Add("@IsPos", SqlDbType.Bit).Value = false;
                            command.Parameters.Add("@PosResultID", SqlDbType.Int).Value = 0;
                            command.Parameters.Add("@ISHour", SqlDbType.Bit).Value = false;
                            command.Parameters.Add("@HourSharjID", SqlDbType.Int).Value = 0;
                            command.Parameters.Add("@IsCredit", SqlDbType.Bit).Value = false;
                            command.Parameters.Add("@CreditCount", SqlDbType.TinyInt).Value = 0;
                            command.ExecuteNonQuery();
                        }
                        string details = amountToman.ToString("N0", CultureInfo.InvariantCulture) + " تومان؛ تخفیف روزانه " +
                            terms.DailyDiscountPercent.ToString(CultureInfo.InvariantCulture) + " درصد";
                        if (terms.PromotionCode != 0)
                            details += "؛ جشنواره " + terms.PromotionName + " " + terms.PromotionDiscountPercent.ToString(CultureInfo.InvariantCulture) + " درصد";
                        EdgeIntegrationRepository.InsertRechargeOutbox(connection, transaction, operationId, amountToman, terms);
                        AuditService.Write(connection, transaction, "card-recharge-recorded", "RechargeTransaction", operationId.ToString(CultureInfo.InvariantCulture), details);
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

        public static DataTable GetUnresolvedRechargeOperations()
        {
            AuthorizationGuard.Require("cards.recharge");
            AuthorizationGuard.Require("data.finance.create");
            string branch = AppSettings.Current.EdgeBranchId ?? String.Empty;
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                var rows = new DataTable();
                using (var command = new SqlCommand(@"
SELECT CardOperationLogId AS OperationId,RequestedAmount AS AmountToman,OperationState AS State,StartedAt
FROM app.CardOperationLog
WHERE OperationName=N'card-recharge' AND ((@branch=N'' AND BranchId IS NULL) OR BranchId=@branch)
  AND OperationState IN ('started','card_written','verified','unknown')
ORDER BY StartedAt DESC,CardOperationLogId DESC;", connection))
                {
                    command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branch.Trim();
                    using (var adapter = new SqlDataAdapter(command)) adapter.Fill(rows);
                }
                return rows;
            }
        }

        public static RechargeReconciliationCandidate GetRechargeForReconciliation(long operationId)
        {
            AuthorizationGuard.Require("cards.recharge");
            AuthorizationGuard.Require("data.finance.create");
            AuthorizationGuard.Require("edge.events.create");
            if (operationId <= 0) throw new ArgumentOutOfRangeException("operationId");
            string branch = AppSettings.Current.EdgeBranchId ?? String.Empty;
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var command = new SqlCommand(@"
SELECT RequestedAmount,StartingBalanceToman,ExpectedBalanceToman,CardKeyHash,OperationState,
       PromotionCode,PromotionDiscountPercent,DailyDiscountPercent,DiscountTierId,PromotionName
FROM app.CardOperationLog
WHERE CardOperationLogId=@id AND OperationName=N'card-recharge'
  AND ((@branch=N'' AND BranchId IS NULL) OR BranchId=@branch)
  AND OperationState IN ('started','card_written','verified','unknown');", connection))
                {
                    command.Parameters.Add("@id", SqlDbType.BigInt).Value = operationId;
                    command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branch.Trim();
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read()) return null;
                        long amount = reader.GetInt64(0);
                        var candidate = new RechargeReconciliationCandidate
                        {
                            OperationId = operationId,
                            AmountToman = amount,
                            StartingBalanceToman = reader.IsDBNull(1) ? (long?)null : reader.GetInt64(1),
                            ExpectedBalanceToman = reader.IsDBNull(2) ? (long?)null : reader.GetInt64(2),
                            CardKeyHash = reader.IsDBNull(3) ? null : (byte[])reader[3],
                            State = reader.GetString(4),
                            Terms = new RechargeOperationTerms
                            {
                                MaximumAmount = amount,
                                PromotionCode = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                                PromotionDiscountPercent = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                                DailyDiscountPercent = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                                DiscountTierId = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                                PromotionName = reader.IsDBNull(9) ? null : reader.GetString(9)
                            }
                        };
                        return candidate;
                    }
                }
            }
        }

        public static void MarkRechargeVerifiedForReconciliation(long operationId, byte[] cardUid, byte[] expectedCardKeyHash)
        {
            AuthorizationGuard.Require("cards.recharge");
            AuthorizationGuard.Require("data.finance.create");
            AuthorizationGuard.Require("edge.events.create");
            if (cardUid == null || cardUid.Length != 4 || expectedCardKeyHash == null || expectedCardKeyHash.Length != 32)
                throw new ArgumentException("اطلاعات کارت برای تطبیق معتبر نیست.");
            byte[] actualHash = EdgeIntegrationRepository.GetCardKeyHash(cardUid);
            if (!BytesEqual(actualHash, expectedCardKeyHash))
                throw new UnauthorizedAccessException("کارت خوانده‌شده با کارت ثبت‌شده برای این عملیات یکسان نیست.");
            string branch = AppSettings.Current.EdgeBranchId ?? String.Empty;
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand(@"
UPDATE app.CardOperationLog
SET CardUid=@maskedUid,OperationState='verified',OperationStatus=N'verified',
    Details=N'پس از راه‌اندازی دوباره، موجودی کارت با عملیات ثبت‌شده تطبیق داده شد.'
WHERE CardOperationLogId=@id AND OperationName=N'card-recharge'
  AND ((@branch=N'' AND BranchId IS NULL) OR BranchId=@branch)
  AND CardKeyHash=@hash AND ExpectedBalanceToman IS NOT NULL
  AND OperationState IN ('started','card_written','verified','unknown');", connection, transaction))
                        {
                            command.Parameters.Add("@maskedUid", SqlDbType.VarChar, 11).Value = MaskUid(BitConverter.ToString(cardUid).Replace("-", String.Empty));
                            command.Parameters.Add("@id", SqlDbType.BigInt).Value = operationId;
                            command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branch.Trim();
                            command.Parameters.Add("@hash", SqlDbType.VarBinary, 32).Value = expectedCardKeyHash;
                            if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("عملیات برای همین شعبه قابل تطبیق نیست یا اطلاعات کارت تغییر کرده است.");
                        }
                        AuditService.Write(connection, transaction, "card-recharge-reconciled", "CardOperationLog", operationId.ToString(CultureInfo.InvariantCulture), "موجودی کارت با عملیات محلی تطبیق داده شد؛ فرمان نوشتن دوباره اجرا نشد.");
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

        public static void MarkRechargeReconciliationUnknown(long operationId, string details)
        {
            AuthorizationGuard.Require("cards.recharge");
            string branch = AppSettings.Current.EdgeBranchId ?? String.Empty;
            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand(@"
UPDATE app.CardOperationLog
SET OperationState='unknown',OperationStatus=N'unknown',Details=@details
WHERE CardOperationLogId=@id AND OperationName=N'card-recharge'
  AND ((@branch=N'' AND BranchId IS NULL) OR BranchId=@branch)
  AND OperationState IN ('started','card_written','verified','unknown');", connection, transaction))
                        {
                            command.Parameters.Add("@id", SqlDbType.BigInt).Value = operationId;
                            command.Parameters.Add("@branch", SqlDbType.NVarChar, 128).Value = branch.Trim();
                            command.Parameters.Add("@details", SqlDbType.NVarChar, 1000).Value = Limit(details, 1000);
                            if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("عملیات برای همین شعبه پیدا نشد.");
                        }
                        AuditService.Write(connection, transaction, "card-recharge-reconciliation-unknown", "CardOperationLog", operationId.ToString(CultureInfo.InvariantCulture), Limit(details, 700));
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

        public static RechargeHistoryPage GetRechargeHistory(byte[] cardUid, int pageNumber, string search, int operationFilter, int sortMode)
        {
            AuthorizationGuard.Require("cards.history.view");
            AuthorizationGuard.Require("data.finance.view");
            if (cardUid == null || cardUid.Length != 4) throw new ArgumentException("ابتدا کارت را بخوانید.", "cardUid");
            pageNumber = Math.Max(1, pageNumber);
            operationFilter = Math.Max(0, Math.Min(4, operationFilter));
            sortMode = Math.Max(0, Math.Min(2, sortMode));
            string searchPattern = "%" + (search ?? String.Empty).Trim() + "%";
            if (searchPattern.Length > 202) searchPattern = searchPattern.Substring(0, 202);
            int total;
            var rows = new DataTable();

            using (var connection = DatabaseManager.CreateMainConnection())
            {
                connection.Open();
                const string where = @"
WHERE CardNumberSegment1=@code1 AND CardNumberSegment2=@code2
  AND CardNumberSegment3=@code3 AND CardNumberSegment4=@code4
  AND (@kind=0
       OR (@kind=1 AND ISNULL(IsPointOfSale,0)=0 AND ISNULL(IsHourlyRecharge,0)=0 AND ISNULL(IsCreditRecharge,0)=0)
       OR (@kind=2 AND ISNULL(IsPointOfSale,0)=1)
       OR (@kind=3 AND ISNULL(IsHourlyRecharge,0)=1)
       OR (@kind=4 AND ISNULL(IsCreditRecharge,0)=1))
  AND (@search=N'%%' OR ISNULL(CreatedByUserName,N'') LIKE @search
       OR CONVERT(nvarchar(32),RechargeAmount) LIKE @search
       OR CONVERT(nvarchar(20),CreatedAt,120) LIKE @search)";
                using (var command = new SqlCommand("SELECT COUNT(1) FROM finance.RechargeTransaction " + where, connection))
                {
                    AddHistoryParameters(command, cardUid, operationFilter, searchPattern);
                    total = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
                }

                string query = @"
SELECT RechargeTransactionId,RechargeAmount,CreatedAt,CreatedByUserName,
       CASE WHEN ISNULL(IsCreditRecharge,0)=1 THEN N'اعتباری'
            WHEN ISNULL(IsHourlyRecharge,0)=1 THEN N'ساعتی'
            WHEN ISNULL(IsPointOfSale,0)=1 THEN N'پرداخت پایانه'
            ELSE N'شارژ عادی' END AS OperationType
FROM
(
 SELECT RechargeTransactionId,RechargeAmount,CreatedAt,CreatedByUserName,
        IsPointOfSale,IsHourlyRecharge,IsCreditRecharge,
        ROW_NUMBER() OVER (ORDER BY
          CASE WHEN @sort=0 THEN CreatedAt END DESC,
          CASE WHEN @sort=1 THEN RechargeAmount END DESC,
          CASE WHEN @sort=2 THEN RechargeAmount END ASC,
          RechargeTransactionId DESC) AS RowNumber
 FROM finance.RechargeTransaction " + where + @"
) AS HistoryRows
WHERE RowNumber>@firstRow AND RowNumber<=@lastRow
ORDER BY RowNumber;";
                using (var command = new SqlCommand(query, connection))
                {
                    AddHistoryParameters(command, cardUid, operationFilter, searchPattern);
                    command.Parameters.Add("@sort", SqlDbType.Int).Value = sortMode;
                    long firstRow = (long)(pageNumber - 1) * HistoryPageSize;
                    command.Parameters.Add("@firstRow", SqlDbType.BigInt).Value = firstRow;
                    command.Parameters.Add("@lastRow", SqlDbType.BigInt).Value = firstRow + HistoryPageSize;
                    using (var adapter = new SqlDataAdapter(command)) adapter.Fill(rows);
                }
            }

            return new RechargeHistoryPage(rows, total, pageNumber, HistoryPageSize);
        }

        private static void AddHistoryParameters(SqlCommand command, byte[] uid, int filter, string search)
        {
            command.Parameters.Add("@code1", SqlDbType.Int).Value = (int)uid[0];
            command.Parameters.Add("@code2", SqlDbType.Int).Value = (int)uid[1];
            command.Parameters.Add("@code3", SqlDbType.Int).Value = (int)uid[2];
            command.Parameters.Add("@code4", SqlDbType.Int).Value = (int)uid[3];
            command.Parameters.Add("@kind", SqlDbType.Int).Value = filter;
            command.Parameters.Add("@search", SqlDbType.NVarChar, 202).Value = search;
        }

        private static void RequireOperation(CardOperationKind kind)
        {
            switch (kind)
            {
                case CardOperationKind.Information: AuthorizationGuard.Require("cards.details.view"); break;
                case CardOperationKind.Balance: AuthorizationGuard.Require("cards.balance.view"); break;
                case CardOperationKind.Remainder: AuthorizationGuard.Require("cards.remainder.view"); break;
                case CardOperationKind.History: AuthorizationGuard.Require("cards.history.view"); break;
                case CardOperationKind.Recharge:
                    AuthorizationGuard.Require("cards.recharge");
                    AuthorizationGuard.Require("data.finance.create");
                    break;
                default: throw new UnauthorizedAccessException("مجوز عملیات کارت تعریف نشده است.");
            }
        }

        private static string OperationName(CardOperationKind kind)
        {
            switch (kind)
            {
                case CardOperationKind.Information: return "card-information";
                case CardOperationKind.Balance: return "card-balance";
                case CardOperationKind.Remainder: return "card-remainder";
                case CardOperationKind.History: return "card-history";
                case CardOperationKind.Recharge: return "card-recharge";
                default: return "card-operation";
            }
        }

        private static string FormatUid(byte[] uid)
        {
            return BitConverter.ToString(uid).Replace("-", String.Empty);
        }

        private static string MaskUid(string uid)
        {
            if (String.IsNullOrWhiteSpace(uid)) return null;
            var hex = new System.Text.StringBuilder(uid.Length);
            foreach (char value in uid)
                if (Uri.IsHexDigit(value)) hex.Append(Char.ToUpperInvariant(value));
            if (hex.Length < 4) return "****";
            return "****" + hex.ToString(hex.Length - 4, 4);
        }

        private static string NormalizeOperationState(string status)
        {
            switch ((status ?? String.Empty).Trim().ToLowerInvariant())
            {
                case "card_written": return "card_written";
                case "verified": return "verified";
                case "unknown":
                case "completed_with_warning": return "unknown";
                case "failed": return "rejected";
                default: return null;
            }
        }

        private static bool BytesEqual(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length) return false;
            int difference = 0;
            for (int i = 0; i < left.Length; i++) difference |= left[i] ^ right[i];
            return difference == 0;
        }

        private static string Limit(string text, int maxLength)
        {
            if (text == null || text.Length <= maxLength) return text;
            return text.Substring(0, maxLength);
        }
    }
}
