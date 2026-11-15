using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Properties;
using FuntinoControlCenter.Security;
using FuntinoControlCenter.Services;
using MessageBox = FuntinoControlCenter.Forms.AppMessageService;

namespace FuntinoControlCenter.Forms
{
    public partial class DeviceStatusForm : Form
    {
        private const int HistoryPageSize = 25;
        private int currentPage = 1;
        private int pageCount = 1;
        private long historyCount;
        private bool canViewDevices;
        private bool canTestDevices;
        private bool canViewEvents;
        private bool historyBusy;
        private bool readerBusy;
        private bool posBusy;
        private bool suppressFilterChange;

        private sealed class DatabaseHealth
        {
            public bool MainAvailable { get; set; }
            public bool DevicesAvailable { get; set; }
        }

        private sealed class HistoryPage
        {
            public long TotalRows { get; set; }
            public DataTable Rows { get; set; }
        }

        public DeviceStatusForm()
        {
            InitializeComponent();
        }

        private async void DeviceStatusForm_Load(object sender, EventArgs e)
        {
            try
            {
                pnlDatabase.Visible = false;
                tlpDevices.Visible = false;
                pnlHistory.Visible = false;
                AuthorizationGuard.RequireAny("devices.view", "devices.test", "devices.events.view");
                UpdatePermissions();
                ConfigureDeviceCaptions();
                suppressFilterChange = true;
                cmbResultFilter.SelectedIndex = 0;
                suppressFilterChange = false;
                AuthorizationMonitor.Attach(this, HasDeviceAccess);

                var operations = new System.Collections.Generic.List<Task>();
                if (canViewDevices) operations.Add(LoadDatabaseHealthAsync());
                if (canViewEvents) operations.Add(LoadHistoryAsync(1));
                await Task.WhenAll(operations);
            }
            catch (UnauthorizedAccessException)
            {
                if (IsFormUnavailable()) return;
                MessageBox.ShowWarning(this, "دسترسی لازم برای مشاهدهٔ این صفحه در حساب شما فعال نیست.", "دسترسی به وضعیت دستگاه‌ها");
                Close();
            }
            catch (Exception)
            {
                if (IsFormUnavailable()) return;
                MessageBox.ShowError(this, "صفحهٔ وضعیت دستگاه‌ها بارگذاری نشد. دوباره تلاش کنید.", "بارگذاری صفحه");
            }
        }

        private void UpdatePermissions()
        {
            canViewDevices = AuthorizationGuard.Has("devices.view");
            canTestDevices = AuthorizationGuard.Has("devices.test");
            canViewEvents = AuthorizationGuard.Has("devices.events.view");

            pnlDatabase.Visible = canViewDevices;
            tlpDevices.Visible = canViewDevices || canTestDevices;
            btnTestReader.Visible = canTestDevices;
            btnTestPos.Visible = canTestDevices;
            pnlHistory.Visible = canViewEvents;
        }

        private void ConfigureDeviceCaptions()
        {
            if (!canViewDevices && !canTestDevices) return;

            lblReaderPort.Text = FormatPortCaption("کارت‌خوان", AppSettings.Current.ReaderPort, AppSettings.Current.ReaderBaud);
            lblPosPort.Text = FormatPortCaption("پایانهٔ فروش", AppSettings.Current.PosPort, AppSettings.Current.PosBaud);

            lblReaderStatus.Text = String.IsNullOrWhiteSpace(AppSettings.Current.ReaderPort)
                ? "برای کارت‌خوان هنوز درگاهی انتخاب نشده است."
                : "برای بررسی اتصال، دکمهٔ «بررسی کارت‌خوان» را بزنید.";
            lblPosStatus.Text = String.IsNullOrWhiteSpace(AppSettings.Current.PosPort)
                ? "برای پایانهٔ فروش هنوز درگاهی انتخاب نشده است."
                : "برای بررسی اتصال، دکمهٔ «بررسی پایانه» را بزنید.";
        }

        private static string FormatPortCaption(string deviceName, string portName, int baudRate)
        {
            if (String.IsNullOrWhiteSpace(portName))
                return deviceName + " — درگاه انتخاب نشده است";

            return deviceName + "  |  درگاه " + portName.Trim() + "  |  سرعت " + ToPersianDigits(baudRate.ToString("N0", CultureInfo.InvariantCulture));
        }

        private async Task LoadDatabaseHealthAsync()
        {
            if (!canViewDevices || IsFormUnavailable()) return;

            loadingDatabase.BeginLoading("در حال بررسی ارتباط پایگاه‌ها…", DeviceStatusVisualAssets.Database);
            try
            {
                DatabaseHealth health = await Task.Run(() =>
                {
                    string ignored;
                    return new DatabaseHealth
                    {
                        MainAvailable = DatabaseManager.TestConnection(false, out ignored),
                        DevicesAvailable = DatabaseManager.TestConnection(true, out ignored)
                    };
                });

                if (IsFormUnavailable()) return;

                lblMainDatabaseStatus.Text = health.MainAvailable ? "پایگاه اصلی: آماده" : "پایگاه اصلی: در دسترس نیست";
                lblDeviceDatabaseStatus.Text = health.DevicesAvailable ? "پایگاه دستگاه‌ها: آماده" : "پایگاه دستگاه‌ها: در دسترس نیست";
                lblMainDatabaseStatus.ForeColor = health.MainAvailable ? Palette.Success : Palette.Warning;
                lblDeviceDatabaseStatus.ForeColor = health.DevicesAvailable ? Palette.Success : Palette.Warning;
            }
            catch (UnauthorizedAccessException)
            {
                if (IsFormUnavailable()) return;
                lblMainDatabaseStatus.Text = "نمایش وضعیت پایگاه‌ها مجاز نیست.";
                lblDeviceDatabaseStatus.Text = String.Empty;
            }
            catch (Exception)
            {
                if (IsFormUnavailable()) return;
                lblMainDatabaseStatus.Text = "وضعیت پایگاه‌ها بررسی نشد. دوباره تلاش کنید.";
                lblMainDatabaseStatus.ForeColor = Palette.Warning;
                lblDeviceDatabaseStatus.Text = String.Empty;
            }
            finally
            {
                if (!IsFormUnavailable()) loadingDatabase.EndLoading();
            }
        }

        private async void TestReader_Click(object sender, EventArgs e)
        {
            if (readerBusy || !canTestDevices || IsFormUnavailable()) return;

            readerBusy = true;
            btnTestReader.Enabled = false;
            lblReaderStatus.Text = "در حال بررسی پاسخ کارت‌خوان…";
            lblReaderStatus.ForeColor = Palette.Muted;
            loadingReader.BeginLoading("در حال بررسی کارت‌خوان…", DeviceStatusVisualAssets.TestReader);

            bool shouldRefreshHistory = false;
            try
            {
                string port = AppSettings.Current.ReaderPort;
                int baud = AppSettings.Current.ReaderBaud;
                var result = await Task.Run(() =>
                {
                    string message;
                    bool success = HardwareService.TestPort(port, baud, out message);
                    return new Tuple<bool, string>(success, message);
                });

                if (IsFormUnavailable()) return;

                lblReaderStatus.Text = result.Item1
                    ? "کارت‌خوان پاسخ می‌دهد و برای کار آماده است. اگر کارت روی دستگاه نباشد، هنوز می‌توانید آن را شناسایی کنید."
                    : "کارت‌خوان پاسخ نداد.";
                lblReaderStatus.ForeColor = result.Item1 ? Palette.Success : Palette.Warning;

                if (!result.Item1)
                    MessageBox.ShowWarning(this, result.Item2, "بررسی کارت‌خوان");

                shouldRefreshHistory = canViewEvents;
            }
            catch (UnauthorizedAccessException)
            {
                if (IsFormUnavailable()) return;
                MessageBox.ShowWarning(this, "اجازهٔ بررسی کارت‌خوان برای حساب شما فعال نیست.", "دسترسی به دستگاه");
            }
            catch (Exception)
            {
                if (IsFormUnavailable()) return;
                lblReaderStatus.Text = "بررسی کارت‌خوان انجام نشد. دوباره تلاش کنید.";
                lblReaderStatus.ForeColor = Palette.Warning;
                MessageBox.ShowWarning(this, "بررسی کارت‌خوان انجام نشد. اتصال دستگاه را بررسی کنید و دوباره تلاش کنید.", "بررسی کارت‌خوان");
            }
            finally
            {
                readerBusy = false;
                if (!IsFormUnavailable())
                {
                    btnTestReader.Enabled = canTestDevices;
                    loadingReader.EndLoading();
                }
            }

            if (shouldRefreshHistory && !IsFormUnavailable()) await LoadHistoryAsync(1);
        }

        private async void TestPos_Click(object sender, EventArgs e)
        {
            if (posBusy || !canTestDevices || IsFormUnavailable()) return;

            posBusy = true;
            btnTestPos.Enabled = false;
            lblPosStatus.Text = "در حال بررسی پاسخ پایانهٔ فروش…";
            lblPosStatus.ForeColor = Palette.Muted;
            loadingPos.BeginLoading("در حال بررسی پایانهٔ فروش…", DeviceStatusVisualAssets.TestPos);

            bool shouldRefreshHistory = false;
            try
            {
                string port = AppSettings.Current.PosPort;
                var result = await Task.Run(() =>
                {
                    string message;
                    bool success = HardwareService.TestPosSdk(port, out message);
                    return new Tuple<bool, string>(success, message);
                });

                if (IsFormUnavailable()) return;

                lblPosStatus.Text = result.Item1
                    ? "پایانهٔ فروش پاسخ می‌دهد و برای کار آماده است."
                    : "پایانهٔ فروش پاسخ مناسب نداد. اتصال و روشن بودن دستگاه را بررسی کنید.";
                lblPosStatus.ForeColor = result.Item1 ? Palette.Success : Palette.Warning;

                if (!result.Item1)
                    MessageBox.ShowWarning(this, GetFriendlyPosMessage(result.Item2), "بررسی پایانهٔ فروش");

                shouldRefreshHistory = canViewEvents;
            }
            catch (UnauthorizedAccessException)
            {
                if (IsFormUnavailable()) return;
                MessageBox.ShowWarning(this, "اجازهٔ بررسی پایانهٔ فروش برای حساب شما فعال نیست.", "دسترسی به دستگاه");
            }
            catch (Exception)
            {
                if (IsFormUnavailable()) return;
                lblPosStatus.Text = "بررسی پایانهٔ فروش انجام نشد. دوباره تلاش کنید.";
                lblPosStatus.ForeColor = Palette.Warning;
                MessageBox.ShowWarning(this, "بررسی پایانهٔ فروش انجام نشد. روشن بودن و اتصال دستگاه را بررسی کنید.", "بررسی پایانهٔ فروش");
            }
            finally
            {
                posBusy = false;
                if (!IsFormUnavailable())
                {
                    btnTestPos.Enabled = canTestDevices;
                    loadingPos.EndLoading();
                }
            }

            if (shouldRefreshHistory && !IsFormUnavailable()) await LoadHistoryAsync(1);
        }

        private static string GetFriendlyPosMessage(string message)
        {
            if (String.IsNullOrWhiteSpace(message))
                return "پایانهٔ فروش پاسخ مناسب نداد. روشن بودن دستگاه و اتصال آن را بررسی کنید.";

            if (message.IndexOf("فایل ارتباطی", StringComparison.Ordinal) >= 0 ||
                message.IndexOf("فایل SDK", StringComparison.OrdinalIgnoreCase) >= 0)
                return "برنامهٔ ارتباطی پایانهٔ فروش پیدا نشد. از مسئول سامانه کمک بگیرید.";

            if (message.IndexOf("انتخاب نشده", StringComparison.Ordinal) >= 0)
                return "برای پایانهٔ فروش درگاهی انتخاب نشده است. از بخش تنظیمات دستگاه‌ها آن را انتخاب کنید.";

            return "پایانهٔ فروش پاسخ مناسب نداد. روشن بودن دستگاه و اتصال آن به درگاه انتخاب‌شده را بررسی کنید.";
        }

        private async void RefreshHistory_Click(object sender, EventArgs e)
        {
            await LoadHistoryAsync(1);
        }

        private void Back_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void SearchHistory_Click(object sender, EventArgs e)
        {
            await LoadHistoryAsync(1);
        }

        private async void SearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            await LoadHistoryAsync(1);
        }

        private async void ResultFilterChanged(object sender, EventArgs e)
        {
            if (suppressFilterChange || historyBusy || !canViewEvents) return;
            await LoadHistoryAsync(1);
        }

        private async void PreviousPage_Click(object sender, EventArgs e)
        {
            if (currentPage > 1) await LoadHistoryAsync(currentPage - 1);
        }

        private async void NextPage_Click(object sender, EventArgs e)
        {
            if (currentPage < pageCount) await LoadHistoryAsync(currentPage + 1);
        }

        private async Task LoadHistoryAsync(int requestedPage)
        {
            if (!canViewEvents || historyBusy || IsFormUnavailable()) return;

            SetHistoryBusy(true, "در حال دریافت سوابق بررسی…");
            try
            {
                string search = (txtSearch.Text ?? String.Empty).Trim();
                int resultFilter = cmbResultFilter.SelectedIndex;
                int successFilter = resultFilter == 1 ? 1 : resultFilter == 2 ? 0 : -1;
                HistoryPage result = await Task.Run(() => ReadHistoryPage(requestedPage, successFilter, search));
                if (IsFormUnavailable()) return;

                int nextPageCount = GetPageCount(result.TotalRows);
                int visiblePage = Math.Min(Math.Max(1, requestedPage), nextPageCount);
                if (visiblePage != requestedPage)
                {
                    result = await Task.Run(() => ReadHistoryPage(visiblePage, successFilter, search));
                    if (IsFormUnavailable()) return;
                    nextPageCount = GetPageCount(result.TotalRows);
                }

                currentPage = visiblePage;
                pageCount = nextPageCount;
                historyCount = result.TotalRows;
                grid.DataSource = result.Rows;
                grid.Visible = result.Rows.Rows.Count > 0;
                lblEmptyState.Visible = result.Rows.Rows.Count == 0;
                lblEmptyState.Text = String.IsNullOrWhiteSpace(search) && successFilter < 0
                    ? "هنوز بررسی دستگاهی ثبت نشده است. برای ثبت نخستین مورد، یکی از دستگاه‌ها را بررسی کنید."
                    : "موردی با این جست‌وجو یا فیلتر پیدا نشد. عبارت یا فیلتر را تغییر دهید.";
                lblHistoryCount.Text = ToPersianDigits(historyCount.ToString("N0", CultureInfo.InvariantCulture)) + " بررسی";
                lblHistoryPage.Text = "صفحهٔ " + ToPersianDigits(currentPage.ToString(CultureInfo.InvariantCulture)) +
                    " از " + ToPersianDigits(pageCount.ToString(CultureInfo.InvariantCulture));
                btnPrevious.Enabled = currentPage > 1;
                btnNext.Enabled = currentPage < pageCount;
            }
            catch (UnauthorizedAccessException)
            {
                if (IsFormUnavailable()) return;
                lblEmptyState.Text = "اجازهٔ مشاهدهٔ سوابق دستگاه‌ها برای حساب شما فعال نیست.";
                lblEmptyState.Visible = true;
                grid.Visible = false;
            }
            catch (Exception)
            {
                if (IsFormUnavailable()) return;
                lblEmptyState.Text = "سوابق بارگذاری نشد. ارتباط پایگاه دستگاه‌ها را بررسی کنید و دوباره تلاش کنید.";
                lblEmptyState.Visible = true;
                grid.Visible = false;
                lblHistoryCount.Text = "سوابق در دسترس نیست";
            }
            finally
            {
                if (!IsFormUnavailable()) SetHistoryBusy(false, null);
            }
        }

        private static int GetPageCount(long totalRows)
        {
            if (totalRows <= 0) return 1;
            long count = (totalRows + HistoryPageSize - 1) / HistoryPageSize;
            return count > Int32.MaxValue ? Int32.MaxValue : (int)count;
        }

        private HistoryPage ReadHistoryPage(int requestedPage, int successFilter, string search)
        {
            AuthorizationGuard.Require("devices.events.view");

            string escapedSearch = EscapeLike(search);
            long firstRow = ((long)Math.Max(1, requestedPage) - 1L) * HistoryPageSize + 1L;
            long lastRow = firstRow + HistoryPageSize - 1L;

            using (var connection = DatabaseManager.CreateDeviceConnection())
            {
                connection.Open();
                const string where = @"WHERE (@success IS NULL OR IsSuccess=@success)
 AND (@search=N'' OR ISNULL(PortName,N'') LIKE N'%' + @search + N'%' ESCAPE N'~'
      OR (CASE
            WHEN EventType='reader-protocol-handshake' THEN N'بررسی کارت‌خوان'
            WHEN EventType='pos-sdk-connection-test' THEN N'بررسی پایانهٔ فروش'
            ELSE N'بررسی دستگاه'
          END) LIKE N'%' + @search + N'%' ESCAPE N'~'
      OR (CASE WHEN IsSuccess=1 THEN N'پاسخ دریافت شد' ELSE N'پاسخ دریافت نشد' END)
            LIKE N'%' + @search + N'%' ESCAPE N'~'
      OR (CASE
            WHEN EventType='reader-protocol-handshake' AND IsSuccess=1 THEN N'کارت‌خوان پاسخ داد؛ نبودن کارت روی دستگاه مانع شناسایی آن نیست.'
            WHEN EventType='reader-protocol-handshake' THEN N'کارت‌خوان پاسخ نداد. روشن بودن دستگاه و اتصال آن را بررسی کنید.'
            WHEN IsSuccess=1 THEN N'پایانهٔ فروش به درخواست بررسی پاسخ داد.'
            ELSE N'پایانهٔ فروش پاسخ مناسب نداد. روشن بودن دستگاه و درگاه انتخاب‌شده را بررسی کنید.'
          END) LIKE N'%' + @search + N'%' ESCAPE N'~')";
                const string countSql = "SELECT COUNT_BIG(1) FROM app.DeviceDiagnosticEvent " + where;
                long total = Convert.ToInt64(DatabaseManager.Scalar(connection, countSql,
                    CreateSuccessParameter(successFilter), CreateSearchParameter(escapedSearch)));

                const string pageSql = @"WITH FilteredEvents AS
(
 SELECT DeviceDiagnosticEventId, EventType, PortName, IsSuccess, CreatedAt,
        ROW_NUMBER() OVER (ORDER BY DeviceDiagnosticEventId DESC) AS RowNumber
 FROM app.DeviceDiagnosticEvent
 " + where + @"
)
SELECT EventType, PortName, IsSuccess, CreatedAt
FROM FilteredEvents
WHERE RowNumber BETWEEN @firstRow AND @lastRow
ORDER BY RowNumber";

                DataTable source = DatabaseManager.Query(connection, pageSql,
                    CreateSuccessParameter(successFilter), CreateSearchParameter(escapedSearch),
                    new SqlParameter("@firstRow", SqlDbType.BigInt) { Value = firstRow },
                    new SqlParameter("@lastRow", SqlDbType.BigInt) { Value = lastRow });

                return new HistoryPage { TotalRows = total, Rows = CreateDisplayRows(source) };
            }
        }

        private static SqlParameter CreateSuccessParameter(int successFilter)
        {
            return new SqlParameter("@success", SqlDbType.Bit)
            {
                Value = successFilter < 0 ? (object)DBNull.Value : successFilter == 1
            };
        }

        private static SqlParameter CreateSearchParameter(string search)
        {
            return new SqlParameter("@search", SqlDbType.NVarChar, 500) { Value = search ?? String.Empty };
        }

        private static string EscapeLike(string value)
        {
            return (value ?? String.Empty).Replace("~", "~~").Replace("%", "~%").Replace("_", "~_").Replace("[", "~[");
        }

        private static DataTable CreateDisplayRows(DataTable source)
        {
            var rows = new DataTable();
            rows.Columns.Add("EventCaption", typeof(string));
            rows.Columns.Add("PortCaption", typeof(string));
            rows.Columns.Add("ResultCaption", typeof(string));
            rows.Columns.Add("DetailsCaption", typeof(string));
            rows.Columns.Add("CreatedCaption", typeof(string));

            foreach (DataRow row in source.Rows)
            {
                string eventType = Convert.ToString(row["EventType"], CultureInfo.InvariantCulture);
                string port = row["PortName"] == DBNull.Value ? "—" : Convert.ToString(row["PortName"], CultureInfo.InvariantCulture);
                bool success = Convert.ToBoolean(row["IsSuccess"], CultureInfo.InvariantCulture);
                DateTime created = Convert.ToDateTime(row["CreatedAt"], CultureInfo.InvariantCulture);
                string eventCaption = GetEventCaption(eventType);
                string resultCaption = success ? "پاسخ دریافت شد" : "پاسخ دریافت نشد";
                string details = GetHistoryDetails(eventType, success);
                string createdCaption = ToPersianDigits(created.ToString("yyyy/MM/dd  HH:mm", CultureInfo.InvariantCulture));
                rows.Rows.Add(eventCaption, port, resultCaption, details, createdCaption);
            }

            return rows;
        }

        private static string GetEventCaption(string eventType)
        {
            if (String.Equals(eventType, "reader-protocol-handshake", StringComparison.OrdinalIgnoreCase)) return "بررسی کارت‌خوان";
            if (String.Equals(eventType, "pos-sdk-connection-test", StringComparison.OrdinalIgnoreCase)) return "بررسی پایانهٔ فروش";
            return "بررسی دستگاه";
        }

        private static string GetHistoryDetails(string eventType, bool success)
        {
            bool isReader = String.Equals(eventType, "reader-protocol-handshake", StringComparison.OrdinalIgnoreCase);
            if (isReader && success) return "کارت‌خوان پاسخ داد؛ نبودن کارت روی دستگاه مانع شناسایی آن نیست.";
            if (isReader) return "کارت‌خوان پاسخ نداد. روشن بودن دستگاه و اتصال آن را بررسی کنید.";
            if (success) return "پایانهٔ فروش به درخواست بررسی پاسخ داد.";
            return "پایانهٔ فروش پاسخ مناسب نداد. روشن بودن دستگاه و درگاه انتخاب‌شده را بررسی کنید.";
        }

        private void SetHistoryBusy(bool busy, string message)
        {
            historyBusy = busy;
            txtSearch.Enabled = !busy;
            cmbResultFilter.Enabled = !busy;
            btnRefreshHistory.Enabled = !busy;
            btnPrevious.Enabled = !busy && currentPage > 1;
            btnNext.Enabled = !busy && currentPage < pageCount;

            if (busy) loadingHistory.BeginLoading(message, DeviceStatusVisualAssets.Refresh);
            else loadingHistory.EndLoading();
        }

        private bool HasDeviceAccess()
        {
            if (IsFormUnavailable()) return false;
            bool hasAny = AuthorizationGuard.HasAny("devices.view", "devices.test", "devices.events.view");
            UpdatePermissions();
            return hasAny;
        }

        private bool IsFormUnavailable()
        {
            return IsDisposed || Disposing;
        }

        private static string ToPersianDigits(string value)
        {
            if (String.IsNullOrEmpty(value)) return value;
            return value.Replace('0', '۰').Replace('1', '۱').Replace('2', '۲').Replace('3', '۳')
                .Replace('4', '۴').Replace('5', '۵').Replace('6', '۶').Replace('7', '۷')
                .Replace('8', '۸').Replace('9', '۹');
        }

        private static class Palette
        {
            public static readonly System.Drawing.Color Success = System.Drawing.Color.FromArgb(26, 119, 92);
            public static readonly System.Drawing.Color Warning = System.Drawing.Color.FromArgb(160, 83, 55);
            public static readonly System.Drawing.Color Muted = System.Drawing.Color.FromArgb(91, 111, 104);
        }
    }
}
