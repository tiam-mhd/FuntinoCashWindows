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
                return "درگاه کارت‌خوان تنظیم نشده است. از تنظیمات سخت‌افزار، درگاه کارت‌خوان را انتخاب و بررسی کنید.";
            return "درگاه " + portName + " در تنظیمات انتخاب شده است. پاسخ کارت‌خوان فقط هنگام اجرای عملیات یا پویش دستی بررسی می‌شود؛ برنامه کارت را در زمان بیکاری پایش نمی‌کند.";
        }

        public static bool TestReaderPort(string portName, int baudRate, out string message)
        {
            AuthorizationGuard.Require("devices.test");
            if (String.IsNullOrWhiteSpace(portName))
            {
                message = "درگاه کارت‌خوان انتخاب نشده است.";
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
                            message = "کارت‌خوان روی درگاه " + portName + " به فرمان شناسایی پاسخ داد. این آزمون حضور کارت را بررسی نمی‌کند.";
                            return true;
                        }
                        if (MatchesResponse(response, RequestCardNoCardReply))
                        {
                            message = "کارت‌خوان روی درگاه " + portName + " پاسخ داد و آماده است؛ کارتی روی آن شناسایی نشد. برای شناسایی دستگاه، کارت لازم نیست.";
                            return true;
                        }
                    }
                }

                message = "درگاه " + portName + " باز شد، اما پاسخ فرمان شناسایی کارت‌خوان دریافت نشد. برای این آزمون کارت لازم نیست؛ اتصال USB، سرعت ارتباط، درگاه و روشن بودن دستگاه را بررسی کنید.";
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                message = "درگاه " + portName + " در اختیار برنامه‌ی دیگری است یا دسترسی ویندوز به آن ممکن نیست.";
                return false;
            }
            catch (System.IO.IOException)
            {
                message = "درگاه " + portName + " در دسترس نیست. اتصال دستگاه و درگاه انتخاب‌شده را بررسی کنید.";
                return false;
            }
            catch (ArgumentException)
            {
                message = "درگاه یا سرعت ارتباط کارت‌خوان معتبر نیست. تنظیمات سخت‌افزار را بررسی کنید.";
                return false;
            }
            catch (Exception)
            {
                message = "پاسخ کارت‌خوان روی درگاه " + portName + " بررسی نشد. اتصال و تنظیمات سخت‌افزار را بررسی کنید.";
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
                            throw new InvalidOperationException("موجودی کارت از ظرفیت قابل‌ذخیره بیشتر می‌شود.");

                        AuthorizationGuard.Require("cards.recharge");
                        AuthorizationGuard.Require("data.finance.create");
                        reader.Increment(CardBalanceBlock, units);
                        CardOperationsRepository.Update(operationId, CardOperationKind.Recharge, uid, "increment_accepted", "فرمان افزایش موجودی پذیرفته شد؛ انتقال نهایی هنوز انجام نشده است.");
                        AuthorizationGuard.Require("cards.recharge");
                        AuthorizationGuard.Require("data.finance.create");
                        transferSent = true;
                        reader.Transfer(CardBalanceBlock);
                        cardWritten = true;
                        try { CardOperationsRepository.Update(operationId, CardOperationKind.Recharge, uid, "card_written", "کارت‌خوان انتقال موجودی را تأیید کرد."); }
                        catch (Exception ex) { warning = AppendWarning(warning, "ثبت وضعیت عملیات در پایگاه انجام نشد: " + FriendlyError(ex)); }

                        try
                        {
                            CardOperationsRepository.RecordRecharge(reader.Uid, amountToman, terms);
                            historyRecorded = true;
                        }
                        catch (Exception ex)
                        {
                            warning = AppendWarning(warning, "موجودی کارت افزایش یافت، اما ثبت شارژ در سوابق سامانه انجام نشد: " + FriendlyError(ex));
                        }

                        try { reader.SaveRechargeHistory(units); }
                        catch (Exception ex) { warning = AppendWarning(warning, "موجودی کارت افزایش یافت، اما سابقه‌ی روی خود کارت به‌روزرسانی نشد: " + FriendlyError(ex)); }

                        long expectedBalance = startingBalance + amountToman;
                        try
                        {
                            reader.Authenticate(CardBalanceBlock);
                            long verifiedBalance = ToToman(reader.ReadBlocks(CardBalanceBlock, 1));
                            if (verifiedBalance != expectedBalance)
                                warning = AppendWarning(warning, "پاسخ کارت‌خوان با موجودی مورد انتظار همخوانی ندارد؛ موجودی را دوباره بخوانید و شارژ را تکرار نکنید.");
                            else expectedBalance = verifiedBalance;
                        }
                        catch (Exception ex)
                        {
                            warning = AppendWarning(warning, "شارژ توسط کارت‌خوان تأیید شد، اما خواندن دوباره‌ی موجودی ممکن نشد: " + FriendlyError(ex));
                        }

                        try { reader.Halt(); }
                        catch (Exception ex) { warning = AppendWarning(warning, "شارژ کارت‌خوان تأیید شد، اما پایان ارتباط تأیید نشد؛ شارژ را تکرار نکنید. " + FriendlyError(ex)); }
                        string finalStatus = String.IsNullOrEmpty(warning) ? "completed" : "completed_with_warning";
                        try { CardOperationsRepository.Update(operationId, CardOperationKind.Recharge, uid, finalStatus, String.IsNullOrEmpty(warning) ? "شارژ کارت و ثبت عملیات پایان یافت." : warning); }
                        catch (Exception ex) { warning = AppendWarning(warning, "ثبت نتیجه‌ی نهایی در گزارش عملیات انجام نشد: " + FriendlyError(ex)); }

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
                    string status = transferSent && !cardWritten ? "unknown" : "failed";
                    string details = transferSent && !cardWritten
                        ? "پاسخ انتقال نهایی دریافت نشد؛ ممکن است موجودی تغییر کرده باشد. شارژ را تکرار نکنید و ابتدا موجودی کارت را بخوانید. " + FriendlyError(ex)
                        : FriendlyError(ex);
                    try { CardOperationsRepository.Update(operationId, CardOperationKind.Recharge, uid, status, details); }
                    catch { }
                if (transferSent && !cardWritten)
                        throw new CardReaderException(CardReaderFailureKind.RechargeResultUnknown,
                            "نتیجه‌ی شارژ مشخص نیست؛ برای جلوگیری از برداشت دوباره، شارژ را تکرار نکنید. ابتدا موجودی کارت را بخوانید. شماره‌ی پیگیری: " + operationId.ToString(CultureInfo.InvariantCulture), ex);
                    throw;
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
            if (data == null || data.Length < 4) throw new InvalidOperationException("اطلاعات موجودی کارت کامل دریافت نشد.");
            return checked((long)BitConverter.ToUInt32(data, 0) * 100L);
        }

        private static IList<CardRechargeHistoryEntry> ParseRechargeHistory(byte[] data)
        {
            if (data == null || data.Length < 32) throw new InvalidOperationException("سوابق شارژ روی کارت کامل دریافت نشد.");
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
            if (data == null || data.Length < 48) throw new InvalidOperationException("سوابق استفاده روی کارت کامل دریافت نشد.");
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
            if (exception is TimeoutException) return exception.Message;
            if (exception is UnauthorizedAccessException &&
                (exception.Message.IndexOf("مجوز", StringComparison.Ordinal) >= 0 ||
                 exception.Message.IndexOf("وارد حساب", StringComparison.Ordinal) >= 0 ||
                 exception.Message.IndexOf("گذرواژه‌ی موقت", StringComparison.Ordinal) >= 0))
                return exception.Message;
            if (exception is System.IO.IOException || exception is UnauthorizedAccessException)
                return "درگاه کارت‌خوان در دسترس نیست یا برنامه‌ی دیگری آن را باز کرده است.";
            if (exception is InvalidOperationException) return exception.Message;
            var sqlException = exception as SqlException;
            if (sqlException != null)
                return "خواندن یا ثبت اطلاعات در پایگاه انجام نشد. اتصال و دسترسی پایگاه را بررسی کنید. کد خطا: " + sqlException.Number.ToString(CultureInfo.InvariantCulture);
            return DatabaseManager.SafeMessage(exception);
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
                    throw new CardReaderException(CardReaderFailureKind.ReaderUnavailable, "درگاه کارت‌خوان تنظیم نشده است. از تنظیمات سخت‌افزار، درگاه کارت‌خوان را انتخاب و بررسی کنید.");
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
                        throw new CardReaderException(CardReaderFailureKind.ReaderUnavailable, "درگاه " + portName + " در اختیار برنامه‌ی دیگری است یا دسترسی ویندوز به آن ممکن نیست. برنامه‌ی دیگر را ببندید یا درگاه کارت‌خوان را در تنظیمات تغییر دهید.", ex);
                    if (ex is System.IO.IOException)
                        throw new CardReaderException(CardReaderFailureKind.ReaderUnavailable, "درگاه " + portName + " پیدا نشد یا کارت‌خوان به آن وصل نیست. اتصال و درگاه انتخاب‌شده را بررسی کنید.", ex);
                    if (ex is ArgumentException || ex is InvalidOperationException)
                        throw new CardReaderException(CardReaderFailureKind.ReaderUnavailable, "درگاه کارت‌خوان معتبر نیست یا در دسترس نیست. از تنظیمات، درگاه دیگری را انتخاب و بررسی کنید.", ex);
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
                ReadStatus("کارت شناسایی شد، اما اطلاعات آن با کلید ذخیره‌شده قابل‌خواندن نیست. کارت را ثابت نگه دارید؛ اگر این خطا برای همه‌ی کارت‌ها تکرار می‌شود، کلید کارت را از تنظیمات دوباره درون‌ریزی کنید.", CardReaderFailureKind.CardKeyRejected);
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
                    "کارت هنگام خواندن پاسخ نداد. ممکن است از روی کارت‌خوان برداشته شده یا درست روی آن قرار نگرفته باشد؛ کارت را ثابت بگذارید و دوباره تلاش کنید.");
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
                ReadStatus("کارت‌خوان افزایش موجودی را نپذیرفت. کارت را برندارید و دوباره موجودی را بخوانید.", CardReaderFailureKind.CardWriteRejected);
            }

            public void Transfer(int block)
            {
                EnsureOpen();
                byte blockByte = checked((byte)block);
                byte checksum = Xor(TransferValue[1], TransferValue[2], TransferValue[3], blockByte);
                Send(new[] { TransferValue[0], TransferValue[1], TransferValue[2], TransferValue[3], blockByte, checksum });
                ReadStatus("کارت‌خوان انتقال نهایی موجودی را تأیید نکرد.", CardReaderFailureKind.CardWriteRejected);
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
                ReadStatus("شارژ انجام شد، اما ثبت سابقه روی خود کارت تأیید نشد.", CardReaderFailureKind.CardDataUnavailable);
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
                    "کارت‌خوان به فرمان آغاز خواندن پاسخ نداد. اتصال دستگاه، درگاه انتخاب‌شده و سرعت ارتباط را بررسی کنید.");
                if (MatchesFrame(requestReply, RequestCardNoCardReply))
                    throw new CardReaderException(CardReaderFailureKind.NoCardDetected,
                        "کارت‌خوان روی درگاه " + port.PortName + " آماده است، اما کارتی روی آن شناسایی نشد. کارت را در مرکز دستگاه بگذارید و دوباره تلاش کنید.");
                if (!MatchesFrame(requestReply, RequestCardReply))
                    throw new CardReaderException(CardReaderFailureKind.ReaderResponseInvalid,
                        "کارت‌خوان روی درگاه " + port.PortName + " پاسخ داد، اما پاسخ آغاز خواندن معتبر نبود. سرعت ارتباط و سازگاری دستگاه را بررسی کنید.");

                Send(Anticollision);
                byte[] collideResponse = ReadFrame(AnticollisionReply, 8, TimeSpan.FromSeconds(3), CardReaderFailureKind.NoCardDetected,
                    "کارت‌خوان آماده است، اما کارتی روی آن شناسایی نشد. کارت را در مرکز دستگاه بگذارید و دوباره تلاش کنید. اگر این پیام با کارت‌های دیگر هم تکرار شد، وضعیت کارت و سطح کارت‌خوان را بررسی کنید.");
                Uid = new byte[4];
                Array.Copy(collideResponse, 4, Uid, 0, Uid.Length);

                byte[] selectFrame = new byte[10];
                Array.Copy(SelectCard, selectFrame, SelectCard.Length);
                Array.Copy(Uid, 0, selectFrame, 5, 4);
                selectFrame[9] = Xor(SelectCard[1], SelectCard[2], SelectCard[3], SelectCard[4], Uid[0], Uid[1], Uid[2], Uid[3]);
                Send(selectFrame);
                ReadFrame(SelectCardReply, SelectCardReply.Length, TimeSpan.FromSeconds(3), CardReaderFailureKind.CardSelectionFailed,
                    "شناسه‌ی کارت دریافت شد، اما ارتباط با خود کارت کامل نشد. کارت را روی کارت‌خوان ثابت بگذارید و دوباره تلاش کنید.");

                if (key == null)
                    throw new CardReaderException(CardReaderFailureKind.CardKeyRejected,
                        "کارت روی کارت‌خوان شناسایی شد، اما کلید امنیتی کارت در تنظیمات این حساب ویندوز ثبت نشده است. فایل کلید کارت را از تنظیمات درون‌ریزی کنید.");

                var keyFrame = new byte[11];
                Array.Copy(LoadKey, keyFrame, LoadKey.Length);
                Array.Copy(key, 0, keyFrame, 4, key.Length);
                keyFrame[10] = Xor(LoadKey[1], LoadKey[2], LoadKey[3], key[0], key[1], key[2], key[3], key[4], key[5]);
                Send(keyFrame);
                ReadStatus("کارت شناسایی شد، اما کلید امنیتی کارت پذیرفته نشد. ممکن است کارت آسیب‌دیده یا با این سامانه ناسازگار باشد. اگر همه‌ی کارت‌ها همین خطا را می‌دهند، کلید کارت را در تنظیمات دوباره وارد کنید.", CardReaderFailureKind.CardKeyRejected);
            }

            private void ReadStatus(string failureMessage, CardReaderFailureKind failureKind)
            {
                byte[] response = ReadFrame(StatusReply, 4, TimeSpan.FromSeconds(3), CardReaderFailureKind.ReaderNotResponding,
                    "کارت‌خوان به فرمان پاسخ نداد. کارت را ثابت نگه دارید؛ اگر دوباره تکرار شد، درگاه و اتصال دستگاه را بررسی کنید.");
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
                if (disposed || port == null || !port.IsOpen) throw new InvalidOperationException("ارتباط کارت‌خوان برقرار نیست.");
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
