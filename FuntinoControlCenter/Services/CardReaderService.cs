using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO.Ports;
using System.Threading;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Security;

namespace FuntinoControlCenter.Services
{
    internal enum CardReaderFailureKind
    {
        NoCardOrReaderResponse,
        NoCardDetected,
        CardIdentifierUnavailable,
        CardSelectionFailed,
        CardKeyRejected,
        CardDataUnavailable,
        ReaderUnavailable,
        ReaderNotResponding,
        ReaderResponseInvalid,
        CardWriteRejected,
        RechargeResultUnknown
    }

    internal sealed class CardReaderException : Exception
    {
        public CardReaderFailureKind FailureKind { get; private set; }

        public CardReaderException(CardReaderFailureKind failureKind, string userMessage, Exception innerException = null)
            : base(userMessage, innerException)
        {
            FailureKind = failureKind;
        }
    }

    internal sealed class CardInformation
    {
        public byte[] UidBytes { get; set; }
        public string Uid { get; set; }
        public long? BalanceToman { get; set; }
        public string CardType { get; set; }
        public string ExpirationDate { get; set; }
        public int DiscountPercent { get; set; }
    }

    internal sealed class CardUsageHistoryEntry
    {
        public string Date { get; set; }
        public string Time { get; set; }
        public string SourceBlock { get; set; }
    }

    internal sealed class CardRemainder
    {
        public byte[] UidBytes { get; set; }
        public long AmountToman { get; set; }
    }

    internal sealed class CardRechargeHistoryEntry
    {
        public long AmountToman { get; set; }
        public int CashRegisterCode { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
    }

    internal sealed class CardHistoryReadResult
    {
        public byte[] UidBytes { get; set; }
        public string Uid { get; set; }
        public IList<CardUsageHistoryEntry> UsageEntries { get; set; }
        public IList<CardRechargeHistoryEntry> RechargeEntries { get; set; }
    }

    internal sealed class CardRechargeResult
    {
        public byte[] UidBytes { get; set; }
        public string Uid { get; set; }
        public long BalanceToman { get; set; }
        public bool HistoryRecorded { get; set; }
        public string Warning { get; set; }
        public RechargeOperationTerms Terms { get; set; }
    }

    internal static class CardReaderService
    {
        private static readonly object OperationLock = new object();
        private static readonly byte[] RequestCard = { 2, 0, 2, 49, 82, 97 };
        private static readonly byte[] RequestCardReply = { 2, 4, 2, 0, 4 };
        private static readonly byte[] RequestCardNoCardReply = { 2, 4, 1, 1, 4 };
        private static readonly byte[] Anticollision = { 2, 0, 2, 50, 147, 163 };
        private static readonly byte[] AnticollisionReply = { 2, 4, 5, 0 };
        private static readonly byte[] SelectCard = { 2, 0, 6, 51, 147 };
        private static readonly byte[] SelectCardReply = { 2, 4, 2, 0, 8 };
        private static readonly byte[] LoadKey = { 2, 0, 7, 53 };
        private static readonly byte[] LoadKeyReply = { 2, 4, 1 };
        private static readonly byte[] AuthenticateCommand = { 2, 0, 7, 55, 96 };
        private static readonly byte[] StatusReply = { 2, 4, 1 };
        private static readonly byte[] ReadBlock = { 2, 0, 3, 56 };
        private static readonly byte[] ReadOneBlockReply = { 2, 4, 17, 0 };
        private static readonly byte[] ReadTwoBlocksReply = { 2, 4, 33, 0 };
        private static readonly byte[] ReadThreeBlocksReply = { 2, 4, 49, 0 };
        private static readonly byte[] IncrementValue = { 2, 0, 7, 58, 193 };
        private static readonly byte[] TransferValue = { 2, 0, 2, 59 };
        private static readonly byte[] WriteTwoBlocks = { 2, 0, 35, 57 };
        private static readonly byte[] HaltCard = { 2, 0, 1, 52, 53 };
        private const int CardBalanceBlock = 8;
        private const int RechargeLogFirstBlock = 9;
        private const int CardInformationBlock = 22;
        private const int CardRemainderBlock = 24;

        public static string GetConfiguredPortStatus()
        {
            AuthorizationGuard.RequireAny("cards.details.view", "cards.balance.view", "cards.remainder.view", "cards.history.view", "cards.recharge");
            string portName = AppSettings.Current.ReaderPort;
            if (String.IsNullOrWhiteSpace(portName))
                return "کارت‌خوان انتخاب نشده است. از بخش تنظیمات دستگاه، کارت‌خوان را انتخاب کنید.";
            return "کارت‌خوان برای استفاده انتخاب شده است. کارت را روی دستگاه بگذارید و عملیات موردنظر را انتخاب کنید.";
        }

        public static bool TestReaderPort(string portName, int baudRate, out string message)
        {
            AuthorizationGuard.Require("devices.test");
            if (String.IsNullOrWhiteSpace(portName))
            {
                message = "کارت‌خوان انتخاب نشده است. آن را از بخش تنظیمات دستگاه انتخاب کنید.";
                return false;
            }

            try
            {
                using (var port = new SerialPort(portName.Trim(), baudRate, Parity.None, 8, StopBits.One))
                {
                    port.ReadTimeout = 250;
                    port.WriteTimeout = 1500;
                    port.Open();
                    port.DiscardInBuffer();
                    port.Write(RequestCard, 0, RequestCard.Length);

                    var response = new Queue<byte>(RequestCardReply.Length);
                    DateTime deadline = DateTime.UtcNow.AddSeconds(7);
                    while (DateTime.UtcNow < deadline)
                    {
                        byte next;
                        try { next = (byte)port.ReadByte(); }
                        catch (TimeoutException) { continue; }

                        if (response.Count == RequestCardReply.Length) response.Dequeue();
                        response.Enqueue(next);
                        if (MatchesResponse(response, RequestCardReply))
                        {
                            message = "کارت‌خوان شناسایی شد و آماده‌ی استفاده است.";
                            return true;
                        }
                        if (MatchesResponse(response, RequestCardNoCardReply))
                        {
                            message = "کارت‌خوان شناسایی شد و پاسخ می‌دهد. برای خواندن کارت، کارت را روی دستگاه بگذارید.";
                            return true;
                        }
                    }
                }

                message = "کارت‌خوان پاسخ نداد. مطمئن شوید دستگاه روشن و به رایانه وصل است؛ سپس دوباره تلاش کنید.";
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                message = "امکان دسترسی به کارت‌خوان وجود ندارد. برنامه‌های دیگری را که از دستگاه استفاده می‌کنند ببندید و دوباره تلاش کنید.";
                return false;
            }
            catch (System.IO.IOException)
            {
                message = "کارت‌خوان پیدا نشد. اتصال دستگاه را بررسی کنید و آن را از تنظیمات دوباره انتخاب کنید.";
                return false;
            }
            catch (ArgumentException)
            {
                message = "تنظیمات کارت‌خوان درست نیست. آن را از بخش تنظیمات دستگاه دوباره انتخاب کنید.";
                return false;
            }
            catch (Exception)
            {
                message = "کارت‌خوان شناسایی نشد. اتصال دستگاه را بررسی کنید و دوباره تلاش کنید.";
                return false;
            }
        }

        private static bool MatchesResponse(Queue<byte> response, byte[] expected)
        {
            if (response.Count != expected.Length) return false;
            int index = 0;
            foreach (byte value in response)
            {
                if (value != expected[index++]) return false;
            }
            return true;
        }

        private static bool MatchesFrame(byte[] response, byte[] expected)
        {
            if (response == null || response.Length != expected.Length) return false;
            for (int index = 0; index < expected.Length; index++)
                if (response[index] != expected[index]) return false;
            return true;
        }

        public static CardInformation ReadInformation()
        {
            AuthorizationGuard.Require("cards.details.view");
            return ExecuteRead(CardOperationKind.Information, delegate(ReaderSession reader)
            {
                reader.Authenticate(CardInformationBlock);
                byte[] infoData = reader.ReadBlocks(CardInformationBlock, 1);
                return CreateCardInformation(reader.Uid, null, infoData);
            });
        }

        public static CardInformation ReadBalance()
        {
            AuthorizationGuard.Require("cards.balance.view");
            return ExecuteRead(CardOperationKind.Balance, delegate(ReaderSession reader)
            {
                reader.Authenticate(CardBalanceBlock);
                byte[] data = reader.ReadBlocks(CardBalanceBlock, 1);
                return CreateCardInformation(reader.Uid, data, null);
            });
        }

        public static CardRemainder ReadRemainder()
        {
            AuthorizationGuard.Require("cards.remainder.view");
            return ExecuteRead(CardOperationKind.Remainder, delegate(ReaderSession reader)
            {
                reader.Authenticate(CardRemainderBlock);
                byte[] data = reader.ReadBlocks(CardRemainderBlock, 1);
                return new CardRemainder
                {
                    UidBytes = (byte[])reader.Uid.Clone(),
                    AmountToman = ToToman(data)
                };
            });
        }

        public static CardHistoryReadResult ReadHistory()
        {
            AuthorizationGuard.Require("cards.history.view");
            return ExecuteRead(CardOperationKind.History, delegate(ReaderSession reader)
            {
                reader.Authenticate(RechargeLogFirstBlock);
                byte[] rechargeData = reader.ReadBlocks(RechargeLogFirstBlock, 2);
                var rechargeEntries = ParseRechargeHistory(rechargeData);

                reader.Authenticate(16);
                byte[] firstUsageBlock = reader.ReadBlocks(16, 3);
                reader.Authenticate(20);
                byte[] secondUsageBlock = reader.ReadBlocks(20, 3);
                var usageEntries = ParseUsageHistory(firstUsageBlock, 16);
                foreach (CardUsageHistoryEntry entry in ParseUsageHistory(secondUsageBlock, 20)) usageEntries.Add(entry);

                return new CardHistoryReadResult
                {
                    UidBytes = (byte[])reader.Uid.Clone(),
                    Uid = FormatUid(reader.Uid),
                    UsageEntries = usageEntries,
                    RechargeEntries = rechargeEntries
                };
            });
        }

        public static CardRechargeResult Recharge(long amountToman, int promotionCode)
        {
            AuthorizationGuard.Require("cards.recharge");
            AuthorizationGuard.Require("data.finance.create");
            RechargeOperationTerms terms = BusinessManagementRepository.ResolveRechargeTerms(amountToman, promotionCode);

            lock (OperationLock)
            {
                long operationId = CardOperationsRepository.Begin(CardOperationKind.Recharge, amountToman);
                string uid = null;
                bool transferSent = false;
                bool cardWritten = false;
                bool cardVerified = false;
                long startingBalance = 0;
                string warning = String.Empty;
                bool historyRecorded = false;
                try
                {
                    using (var reader = ReaderSession.Open())
                    {
                        uid = FormatUid(reader.Uid);
                        reader.Authenticate(CardBalanceBlock);
                        byte[] balanceData = reader.ReadBlocks(CardBalanceBlock, 1);
                        startingBalance = ToToman(balanceData);
                        uint units = checked((uint)(amountToman / 100));
                        if ((ulong)startingBalance / 100UL + units > UInt32.MaxValue)
                            throw new InvalidOperationException("موجودی کارت از حد مجاز بیشتر است و نمی‌توان آن را شارژ کرد.");
                        long expectedBalance = checked(startingBalance + amountToman);
                        CardOperationsRepository.PrepareRecharge(operationId, reader.Uid, startingBalance, expectedBalance, terms);

                        AuthorizationGuard.Require("cards.recharge");
                        AuthorizationGuard.Require("data.finance.create");
                        reader.Increment(CardBalanceBlock, units);
                        CardOperationsRepository.Update(operationId, CardOperationKind.Recharge, uid, "increment_accepted", "شارژ کارت در حال تکمیل است.");
                        AuthorizationGuard.Require("cards.recharge");
                        AuthorizationGuard.Require("data.finance.create");
                        transferSent = true;
                        reader.Transfer(CardBalanceBlock);
                        cardWritten = true;
                        try { CardOperationsRepository.Update(operationId, CardOperationKind.Recharge, uid, "card_written", "کارت‌خوان انتقال موجودی را تأیید کرد."); }
                        catch (Exception ex) { warning = AppendWarning(warning, "شارژ انجام شد، اما ثبت نتیجه‌ی آن در سوابق سامانه کامل نشد. " + FriendlyError(ex)); }

                        reader.Authenticate(CardBalanceBlock);
                        long verifiedBalance = ToToman(reader.ReadBlocks(CardBalanceBlock, 1));
                        if (verifiedBalance != expectedBalance)
                        {
                            try { CardOperationsRepository.Update(operationId, CardOperationKind.Recharge, uid, "unknown", "موجودی بازخوانی‌شده با مقدار مورد انتظار یکسان نیست؛ فرمان شارژ دوباره اجرا نشد."); }
                            catch { }
                            throw new CardReaderException(CardReaderFailureKind.RechargeResultUnknown,
                                "موجودی کارت پس از نوشتن با مبلغ مورد انتظار هم‌خوانی ندارد. دوباره شارژ نکنید؛ ابتدا کارت را بررسی کنید.");
                        }
                        cardVerified = true;
                        expectedBalance = verifiedBalance;
                        try { CardOperationsRepository.Update(operationId, CardOperationKind.Recharge, uid, "verified", "موجودی نوشته‌شده روی کارت دوباره خوانده و تأیید شد."); }
                        catch (Exception ex) { warning = AppendWarning(warning, "موجودی کارت تأیید شد، اما نتیجه‌ی تطبیق در پایگاه محلی کامل ثبت نشد. " + FriendlyError(ex)); }

                        try
                        {
                            CardOperationsRepository.RecordRecharge(operationId, reader.Uid, amountToman, terms);
                            historyRecorded = true;
                        }
                        catch (Exception ex)
                        {
                            warning = AppendWarning(warning, "شارژ انجام شد، اما سابقه‌ی آن در سامانه ثبت نشد. " + FriendlyError(ex));
                            try { CardOperationsRepository.Update(operationId, CardOperationKind.Recharge, uid, "unknown", "کارت نوشته و تأیید شد، اما ثبت محلی مالی و رویداد همگام‌سازی انجام نشد. " + FriendlyError(ex)); }
                            catch { }
                        }

                        try { reader.SaveRechargeHistory(units); }
                        catch (Exception ex) { warning = AppendWarning(warning, "شارژ انجام شد، اما سوابق کارت به‌روز نشد. " + FriendlyError(ex)); }

                        try { reader.Halt(); }
                        catch (Exception ex) { warning = AppendWarning(warning, "شارژ انجام شد. برای اطمینان، موجودی کارت را دوباره بخوانید و شارژ را تکرار نکنید. " + FriendlyError(ex)); }
                        string finalStatus = String.IsNullOrEmpty(warning) ? "completed" : "completed_with_warning";
                        try { CardOperationsRepository.Update(operationId, CardOperationKind.Recharge, uid, finalStatus, String.IsNullOrEmpty(warning) ? "شارژ کارت با موفقیت انجام شد." : warning); }
                        catch (Exception ex) { warning = AppendWarning(warning, "شارژ انجام شد، اما ثبت نتیجه‌ی آن در سوابق سامانه کامل نشد. " + FriendlyError(ex)); }

                        return new CardRechargeResult
                        {
                            UidBytes = (byte[])reader.Uid.Clone(),
                            Uid = uid,
                            BalanceToman = expectedBalance,
                            HistoryRecorded = historyRecorded,
                            Warning = warning,
                            Terms = terms
                        };
                    }
                }
                catch (Exception ex)
                {
                    bool resultUnknown = transferSent && (!cardWritten || !cardVerified);
                    string status = resultUnknown ? "unknown" : "failed";
                    string details = resultUnknown
                        ? "وضعیت شارژ مشخص نیست و ممکن است مبلغ به کارت اضافه شده باشد. دوباره شارژ نکنید؛ ابتدا موجودی کارت را بخوانید. " + FriendlyError(ex)
                        : FriendlyError(ex);
                    try { CardOperationsRepository.Update(operationId, CardOperationKind.Recharge, uid, status, details); }
                    catch { }
                    if (resultUnknown)
                        throw new CardReaderException(CardReaderFailureKind.RechargeResultUnknown,
                            "وضعیت شارژ مشخص نیست و ممکن است مبلغ به کارت اضافه شده باشد. دوباره شارژ نکنید؛ ابتدا موجودی کارت را بخوانید. اگر مشکل ادامه داشت، به مسئول سامانه اطلاع دهید.", ex);
                    throw;
                }
            }
        }

        public static bool ReconcileRecharge(long operationId)
        {
            AuthorizationGuard.Require("cards.recharge");
            AuthorizationGuard.Require("data.finance.create");
            AuthorizationGuard.Require("edge.events.create");
            RechargeReconciliationCandidate candidate = CardOperationsRepository.GetRechargeForReconciliation(operationId);
            if (candidate == null) throw new InvalidOperationException("عملیات نامشخص در همین شعبه پیدا نشد یا قبلاً تعیین تکلیف شده است.");
            if (candidate.CardKeyHash == null || candidate.CardKeyHash.Length != 32 || !candidate.ExpectedBalanceToman.HasValue)
            {
                CardOperationsRepository.MarkRechargeReconciliationUnknown(operationId, "اطلاعات کافی برای تطبیق پس از راه‌اندازی دوباره ثبت نشده است؛ کارت را دوباره شارژ نکنید.");
                return false;
            }

            lock (OperationLock)
            {
                using (var reader = ReaderSession.Open())
                {
                    byte[] uid = (byte[])reader.Uid.Clone();
                    reader.Authenticate(CardBalanceBlock);
                    long currentBalance = ToToman(reader.ReadBlocks(CardBalanceBlock, 1));
                    byte[] currentHash = EdgeIntegrationRepository.GetCardKeyHash(uid);
                    if (!FixedTimeEquals(currentHash, candidate.CardKeyHash) || currentBalance != candidate.ExpectedBalanceToman.Value)
                    {
                        CardOperationsRepository.MarkRechargeReconciliationUnknown(operationId,
                            "کارت خوانده‌شده یا موجودی فعلی با عملیات ثبت‌شده تطبیق ندارد؛ هیچ فرمان نوشتنی اجرا نشد.");
                        reader.Halt();
                        return false;
                    }

                    CardOperationsRepository.MarkRechargeVerifiedForReconciliation(operationId, uid, candidate.CardKeyHash);
                    CardOperationsRepository.RecordRecharge(operationId, uid, candidate.AmountToman, candidate.Terms);
                    reader.Halt();
                    return true;
                }
            }
        }

        private static T ExecuteRead<T>(CardOperationKind kind, Func<ReaderSession, T> operation)
        {
            lock (OperationLock)
            {
                long operationId = CardOperationsRepository.Begin(kind, null);
                string uid = null;
                try
                {
                    T result;
                    using (var reader = ReaderSession.Open())
                    {
                        uid = FormatUid(reader.Uid);
                        result = operation(reader);
                        reader.Halt();
                    }
                    CardOperationsRepository.Update(operationId, kind, uid, "completed", "عملیات خواندن کارت با موفقیت پایان یافت.");
                    return result;
                }
                catch (Exception ex)
                {
                    try { CardOperationsRepository.Update(operationId, kind, uid, "failed", FriendlyError(ex)); }
                    catch { }
                    throw;
                }
            }
        }

        private static CardInformation CreateCardInformation(byte[] uid, byte[] balanceData, byte[] infoData)
        {
            var result = new CardInformation
            {
                UidBytes = (byte[])uid.Clone(),
                Uid = FormatUid(uid),
                BalanceToman = balanceData == null ? (long?)null : ToToman(balanceData),
                CardType = "کارت عادی",
                ExpirationDate = "ثبت نشده",
                DiscountPercent = 0
            };
            if (infoData != null && infoData.Length >= 6)
            {
                bool timeLimited = infoData[0] == 162;
                bool vip = infoData[5] == 76;
                result.DiscountPercent = infoData[4] * 10;
                result.CardType = timeLimited ? (vip ? "ویژه‌ی مدت‌دار" : "مدت‌دار") : (vip ? "ویژه‌ی عادی" : "کارت عادی");
                if (timeLimited)
                    result.ExpirationDate = infoData[1].ToString("00", CultureInfo.InvariantCulture) + "/" +
                        infoData[2].ToString("00", CultureInfo.InvariantCulture) + "/" + infoData[3].ToString("00", CultureInfo.InvariantCulture);
            }
            return result;
        }

        private static long ToToman(byte[] data)
        {
            if (data == null || data.Length < 4) throw new InvalidOperationException("خواندن موجودی کارت کامل نشد. کارت را روی دستگاه ثابت نگه دارید و دوباره تلاش کنید.");
            return checked((long)BitConverter.ToUInt32(data, 0) * 100L);
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length) return false;
            int difference = 0;
            for (int i = 0; i < left.Length; i++) difference |= left[i] ^ right[i];
            return difference == 0;
        }

        private static IList<CardRechargeHistoryEntry> ParseRechargeHistory(byte[] data)
        {
            if (data == null || data.Length < 32) throw new InvalidOperationException("خواندن سوابق شارژ کارت کامل نشد. کارت را روی دستگاه ثابت نگه دارید و دوباره تلاش کنید.");
            var result = new List<CardRechargeHistoryEntry>();
            for (int record = 0; record < 2; record++)
            {
                int offset = record * 16;
                uint units = BitConverter.ToUInt32(data, offset + 1);
                if (units == 0 && data[offset] == 0) continue;
                result.Add(new CardRechargeHistoryEntry
                {
                    AmountToman = checked((long)units * 100L),
                    CashRegisterCode = data[offset],
                    Date = FormatCardDate(data[offset + 5], data[offset + 6], data[offset + 7]),
                    Time = data[offset + 8].ToString("00", CultureInfo.InvariantCulture) + ":" + data[offset + 9].ToString("00", CultureInfo.InvariantCulture)
                });
            }
            return result;
        }

        private static IList<CardUsageHistoryEntry> ParseUsageHistory(byte[] data, int firstBlock)
        {
            if (data == null || data.Length < 48) throw new InvalidOperationException("خواندن سوابق استفاده از کارت کامل نشد. کارت را روی دستگاه ثابت نگه دارید و دوباره تلاش کنید.");
            var result = new List<CardUsageHistoryEntry>();
            for (int offset = 0; offset < 48; offset += 6)
            {
                byte year = data[offset];
                byte month = data[offset + 1];
                byte day = data[offset + 2];
                byte hour = data[offset + 3];
                byte minute = data[offset + 4];
                byte second = data[offset + 5];
                if (year == 0 && month == 0 && day == 0 && hour == 0 && minute == 0 && second == 0) continue;
                result.Add(new CardUsageHistoryEntry
                {
                    Date = FormatCardDate(year, month, day),
                    Time = hour.ToString("00", CultureInfo.InvariantCulture) + ":" + minute.ToString("00", CultureInfo.InvariantCulture) + ":" + second.ToString("00", CultureInfo.InvariantCulture),
                    SourceBlock = firstBlock.ToString(CultureInfo.InvariantCulture)
                });
            }
            return result;
        }

        private static string FormatCardDate(byte year, byte month, byte day)
        {
            if (year == 0 && month == 0 && day == 0) return "ثبت نشده";
            return year.ToString("00", CultureInfo.InvariantCulture) + "/" + month.ToString("00", CultureInfo.InvariantCulture) + "/" + day.ToString("00", CultureInfo.InvariantCulture);
        }

        private static string FormatUid(byte[] uid)
        {
            return BitConverter.ToString(uid).Replace("-", String.Empty);
        }

        private static string AppendWarning(string current, string next)
        {
            return String.IsNullOrEmpty(current) ? next : current + Environment.NewLine + next;
        }

        private static string FriendlyError(Exception exception)
        {
            if (exception is CardReaderException) return exception.Message;
            if (exception is TimeoutException) return "کارت‌خوان پاسخ نداد. اتصال دستگاه را بررسی کنید و دوباره تلاش کنید.";
            if (exception is UnauthorizedAccessException) return "برای انجام این کار، دسترسی لازم وجود ندارد. به مدیر سامانه اطلاع دهید.";
            if (exception is System.IO.IOException)
                return "کارت‌خوان در دسترس نیست. اتصال دستگاه را بررسی کنید و برنامه‌های دیگری را که از آن استفاده می‌کنند ببندید.";
            if (exception is InvalidOperationException)
            {
                if (exception.Message.IndexOf("ظرفیت", StringComparison.Ordinal) >= 0)
                    return "موجودی کارت از حد مجاز بیشتر است و نمی‌توان آن را شارژ کرد.";
                return "این کار در حال حاضر انجام نشد. دوباره تلاش کنید یا به مسئول سامانه اطلاع دهید.";
            }
            var sqlException = exception as SqlException;
            if (sqlException != null)
                return "ثبت یا دریافت اطلاعات در سامانه انجام نشد. دوباره تلاش کنید؛ اگر مشکل ادامه داشت، به مسئول سامانه اطلاع دهید.";
            return "این کار انجام نشد. دوباره تلاش کنید؛ اگر مشکل ادامه داشت، به مسئول سامانه اطلاع دهید.";
        }

        private sealed class ReaderSession : IDisposable
        {
            private readonly SerialPort port;
            private readonly byte[] key;
            private bool disposed;
            private bool halted;
            public byte[] Uid { get; private set; }

            private ReaderSession(SerialPort serialPort, byte[] cardKey)
            {
                port = serialPort;
                key = cardKey;
            }

            public static ReaderSession Open()
            {
                string portName = AppSettings.Current.ReaderPort;
                if (String.IsNullOrWhiteSpace(portName))
                    throw new CardReaderException(CardReaderFailureKind.ReaderUnavailable, "کارت‌خوان انتخاب نشده است. از بخش تنظیمات دستگاه، آن را انتخاب کنید.");
                byte[] key = AppSettings.Current.GetReaderCardKey();

                SerialPort serialPort = null;
                try
                {
                    serialPort = new SerialPort(portName.Trim(), AppSettings.Current.ReaderBaud, Parity.None, 8, StopBits.One);
                    serialPort.ReadTimeout = 250;
                    serialPort.WriteTimeout = 1500;
                    serialPort.Open();
                    serialPort.DiscardInBuffer();
                }
                catch (Exception ex)
                {
                    if (serialPort != null) serialPort.Dispose();
                    if (key != null) Array.Clear(key, 0, key.Length);
                    if (ex is UnauthorizedAccessException)
                        throw new CardReaderException(CardReaderFailureKind.ReaderUnavailable, "امکان دسترسی به کارت‌خوان وجود ندارد. برنامه‌های دیگری را که از دستگاه استفاده می‌کنند ببندید و دوباره تلاش کنید.", ex);
                    if (ex is System.IO.IOException)
                        throw new CardReaderException(CardReaderFailureKind.ReaderUnavailable, "کارت‌خوان پیدا نشد. اتصال دستگاه را بررسی کنید یا آن را از تنظیمات دوباره انتخاب کنید.", ex);
                    if (ex is ArgumentException || ex is InvalidOperationException)
                        throw new CardReaderException(CardReaderFailureKind.ReaderUnavailable, "تنظیمات کارت‌خوان درست نیست. آن را از بخش تنظیمات دستگاه دوباره انتخاب کنید.", ex);
                    throw;
                }

            var session = new ReaderSession(serialPort, key);
            try
            {
                session.SelectAndLoadKey();
                return session;
            }
            catch
            {
                try { serialPort.Dispose(); }
                finally
                {
                    if (key != null) Array.Clear(key, 0, key.Length);
                    if (session.Uid != null) Array.Clear(session.Uid, 0, session.Uid.Length);
                }
                throw;
            }
            }

            public void Authenticate(int block)
            {
                EnsureOpen();
                byte[] frame = new byte[11];
                Array.Copy(AuthenticateCommand, frame, AuthenticateCommand.Length);
                frame[5] = checked((byte)block);
                Array.Copy(Uid, 0, frame, 6, 4);
                frame[10] = Xor(AuthenticateCommand[1], AuthenticateCommand[2], AuthenticateCommand[3], AuthenticateCommand[4], frame[5], Uid[0], Uid[1], Uid[2], Uid[3]);
                Send(frame);
                ReadStatus("خواندن این کارت ممکن نشد. کارت را روی دستگاه ثابت نگه دارید و دوباره تلاش کنید؛ اگر مشکل تکرار شد، به مسئول سامانه اطلاع دهید.", CardReaderFailureKind.CardKeyRejected);
            }

            public byte[] ReadBlocks(int firstBlock, int count)
            {
                EnsureOpen();
                if (count < 1 || count > 3) throw new ArgumentOutOfRangeException("count");
                byte countByte = checked((byte)count);
                byte blockByte = checked((byte)firstBlock);
                byte checksum = Xor(ReadBlock[1], ReadBlock[2], ReadBlock[3], blockByte, countByte);
                Send(new[] { ReadBlock[0], ReadBlock[1], ReadBlock[2], ReadBlock[3], blockByte, countByte, checksum });
                byte[] expected = count == 1 ? ReadOneBlockReply : (count == 2 ? ReadTwoBlocksReply : ReadThreeBlocksReply);
                int frameLength = 4 + 16 * count;
                byte[] response = ReadFrame(expected, frameLength, TimeSpan.FromSeconds(3), CardReaderFailureKind.CardDataUnavailable,
                    "خواندن کارت کامل نشد. کارت را روی دستگاه ثابت نگه دارید و دوباره تلاش کنید.");
                var data = new byte[16 * count];
                Array.Copy(response, 4, data, 0, data.Length);
                return data;
            }

            public void Increment(int block, uint value)
            {
                EnsureOpen();
                byte[] bytes = BitConverter.GetBytes(value);
                byte blockByte = checked((byte)block);
                byte checksum = Xor(IncrementValue[1], IncrementValue[2], IncrementValue[3], IncrementValue[4], blockByte, bytes[0], bytes[1], bytes[2], bytes[3]);
                var frame = new byte[11];
                Array.Copy(IncrementValue, frame, IncrementValue.Length);
                frame[5] = blockByte;
                Array.Copy(bytes, 0, frame, 6, 4);
                frame[10] = checksum;
                Send(frame);
                ReadStatus("شارژ کارت انجام نشد. کارت را روی دستگاه نگه دارید و موجودی را دوباره بررسی کنید.", CardReaderFailureKind.CardWriteRejected);
            }

            public void Transfer(int block)
            {
                EnsureOpen();
                byte blockByte = checked((byte)block);
                byte checksum = Xor(TransferValue[1], TransferValue[2], TransferValue[3], blockByte);
                Send(new[] { TransferValue[0], TransferValue[1], TransferValue[2], TransferValue[3], blockByte, checksum });
                ReadStatus("وضعیت شارژ تأیید نشد. دوباره شارژ نکنید؛ ابتدا موجودی کارت را بخوانید.", CardReaderFailureKind.CardWriteRejected);
            }

            public void SaveRechargeHistory(uint addedUnits)
            {
                EnsureOpen();
                Authenticate(RechargeLogFirstBlock);
                byte[] oldData = ReadBlocks(RechargeLogFirstBlock, 2);
                byte[] amount = BitConverter.GetBytes(addedUnits);
                PersianCalendar calendar = new PersianCalendar();
                DateTime now = DateTime.Now;
                byte[] date =
                {
                    checked((byte)(calendar.GetYear(now) % 100)),
                    checked((byte)calendar.GetMonth(now)),
                    checked((byte)calendar.GetDayOfMonth(now)),
                    checked((byte)calendar.GetHour(now)),
                    checked((byte)calendar.GetMinute(now))
                };
                byte cashRegisterCode = 0;
                const byte qn = 2;
                byte checksum = Xor(WriteTwoBlocks[1], WriteTwoBlocks[2], WriteTwoBlocks[3], RechargeLogFirstBlock, qn,
                    cashRegisterCode, amount[0], amount[1], amount[2], amount[3], date[0], date[1], date[2], date[3], date[4],
                    oldData[0], oldData[1], oldData[2], oldData[3], oldData[4], oldData[5], oldData[6], oldData[7],
                    oldData[8], oldData[9], oldData[10], oldData[11], oldData[12], oldData[13], oldData[14], oldData[15],
                    oldData[26], oldData[27], oldData[28], oldData[29], oldData[30], oldData[31]);

                var frame = new List<byte>(39);
                frame.AddRange(WriteTwoBlocks);
                frame.Add(RechargeLogFirstBlock);
                frame.Add(qn);
                frame.Add(cashRegisterCode);
                frame.AddRange(amount);
                frame.AddRange(date);
                for (int i = 10; i < 16; i++) frame.Add(oldData[i]);
                for (int i = 0; i < 10; i++) frame.Add(oldData[i]);
                for (int i = 26; i < 31; i++) frame.Add(oldData[i]);
                frame.Add(oldData[31]);
                frame.Add(checksum);
                Send(frame.ToArray());
                ReadStatus("شارژ انجام شد، اما سوابق کارت به‌روز نشد.", CardReaderFailureKind.CardDataUnavailable);
            }

            public void Halt()
            {
                if (disposed || halted || !port.IsOpen) return;
                Send(HaltCard);
                halted = true;
            }

            private void SelectAndLoadKey()
            {
                Send(RequestCard);
                byte[] requestReply = ReadFrame(new byte[] { 2, 4 }, RequestCardReply.Length, TimeSpan.FromSeconds(7), CardReaderFailureKind.ReaderNotResponding,
                    "کارت‌خوان پاسخ نداد. مطمئن شوید دستگاه روشن و به رایانه وصل است؛ سپس دوباره تلاش کنید.");
                if (MatchesFrame(requestReply, RequestCardNoCardReply))
                    throw new CardReaderException(CardReaderFailureKind.NoCardDetected,
                        "کارتی روی کارت‌خوان نیست. کارت را روی دستگاه بگذارید و دوباره تلاش کنید.");
                if (!MatchesFrame(requestReply, RequestCardReply))
                    throw new CardReaderException(CardReaderFailureKind.ReaderResponseInvalid,
                        "ارتباط با کارت‌خوان کامل نشد. دستگاه را جدا و دوباره وصل کنید و تلاش کنید.");

                Send(Anticollision);
                byte[] collideResponse = ReadFrame(AnticollisionReply, 8, TimeSpan.FromSeconds(3), CardReaderFailureKind.NoCardDetected,
                    "کارتی روی کارت‌خوان نیست. کارت را روی دستگاه بگذارید و دوباره تلاش کنید.");
                Uid = new byte[4];
                Array.Copy(collideResponse, 4, Uid, 0, Uid.Length);

                byte[] selectFrame = new byte[10];
                Array.Copy(SelectCard, selectFrame, SelectCard.Length);
                Array.Copy(Uid, 0, selectFrame, 5, 4);
                selectFrame[9] = Xor(SelectCard[1], SelectCard[2], SelectCard[3], SelectCard[4], Uid[0], Uid[1], Uid[2], Uid[3]);
                Send(selectFrame);
                ReadFrame(SelectCardReply, SelectCardReply.Length, TimeSpan.FromSeconds(3), CardReaderFailureKind.CardSelectionFailed,
                    "کارت شناسایی شد، اما خواندن آن کامل نشد. کارت را روی دستگاه ثابت نگه دارید و دوباره تلاش کنید.");

                if (key == null)
                    throw new CardReaderException(CardReaderFailureKind.CardKeyRejected,
                        "خواندن این کارت ممکن نشد. به مسئول سامانه اطلاع دهید.");

                var keyFrame = new byte[11];
                Array.Copy(LoadKey, keyFrame, LoadKey.Length);
                Array.Copy(key, 0, keyFrame, 4, key.Length);
                keyFrame[10] = Xor(LoadKey[1], LoadKey[2], LoadKey[3], key[0], key[1], key[2], key[3], key[4], key[5]);
                Send(keyFrame);
                ReadStatus("خواندن این کارت ممکن نشد. کارت را بردارید، دوباره روی دستگاه بگذارید و تلاش کنید؛ اگر مشکل تکرار شد، به مسئول سامانه اطلاع دهید.", CardReaderFailureKind.CardKeyRejected);
            }

            private void ReadStatus(string failureMessage, CardReaderFailureKind failureKind)
            {
                byte[] response = ReadFrame(StatusReply, 4, TimeSpan.FromSeconds(3), CardReaderFailureKind.ReaderNotResponding,
                    "کارت‌خوان پاسخ نداد. اتصال دستگاه را بررسی کنید و دوباره تلاش کنید.");
                if (response[3] != 0) throw new CardReaderException(failureKind, failureMessage);
            }

            private byte[] ReadFrame(byte[] prefix, int frameLength, TimeSpan timeout, CardReaderFailureKind failureKind, string timeoutMessage)
            {
                EnsureOpen();
                var window = new Queue<byte>(prefix.Length);
                DateTime deadline = DateTime.UtcNow.Add(timeout);
                while (DateTime.UtcNow < deadline)
                {
                    byte next;
                    try { next = (byte)port.ReadByte(); }
                    catch (TimeoutException) { continue; }
                    if (window.Count == prefix.Length) window.Dequeue();
                    window.Enqueue(next);
                    if (window.Count == prefix.Length && HasPrefix(window, prefix))
                    {
                        byte[] response = new byte[frameLength];
                        window.CopyTo(response, 0);
                        for (int i = prefix.Length; i < frameLength; i++) response[i] = ReadByteBefore(DateTime.UtcNow.AddSeconds(2), failureKind, timeoutMessage);
                        return response;
                    }
                }
                throw new CardReaderException(failureKind, timeoutMessage);
            }

            private byte ReadByteBefore(DateTime deadline, CardReaderFailureKind failureKind, string timeoutMessage)
            {
                while (DateTime.UtcNow < deadline)
                {
                    try { return (byte)port.ReadByte(); }
                    catch (TimeoutException) { }
                }
                throw new CardReaderException(failureKind, timeoutMessage);
            }

            private static bool HasPrefix(Queue<byte> window, byte[] prefix)
            {
                int index = 0;
                foreach (byte value in window)
                {
                    if (value != prefix[index++]) return false;
                }
                return index == prefix.Length;
            }

            private void Send(byte[] frame)
            {
                EnsureOpen();
                port.DiscardInBuffer();
                port.Write(frame, 0, frame.Length);
                Thread.Sleep(20);
            }

            private void EnsureOpen()
            {
                if (disposed || port == null || !port.IsOpen) throw new InvalidOperationException("ارتباط با کارت‌خوان برقرار نیست. دستگاه را بررسی کنید و دوباره تلاش کنید.");
            }

            public void Dispose()
            {
                if (disposed) return;
                try { Halt(); }
                catch { }
                disposed = true;
                try { if (port.IsOpen) port.Close(); }
                finally
                {
                    port.Dispose();
                    if (key != null) Array.Clear(key, 0, key.Length);
                    if (Uid != null) Array.Clear(Uid, 0, Uid.Length);
                }
            }
        }

        private static byte Xor(params byte[] values)
        {
            byte result = 0;
            for (int i = 0; i < values.Length; i++) result ^= values[i];
            return result;
        }
    }
}
