using System;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Security;
using MessageBox = FuntinoControlCenter.Forms.AppMessageService;

namespace FuntinoControlCenter.Forms
{
    public partial class EdgeSyncStatusForm : Form
    {
        private bool busy;
        private bool hasLoaded;

        public EdgeSyncStatusForm()
        {
            InitializeComponent();
        }

        private void EdgeSyncStatusForm_Load(object sender, EventArgs e)
        {
            try
            {
                AuthorizationGuard.Require("edge.sync.view");
                btnRetry.Visible = AuthorizationGuard.Has("edge.sync.retry");
                AuthorizationMonitor.Attach(this, delegate { return AuthorizationGuard.Has("edge.sync.view"); });
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.ShowWarning(this, "دسترسی لازم برای مشاهدهٔ وضعیت همگام‌سازی فعال نیست.", "دسترسی محدود است");
                Close();
            }
        }

        private async void EdgeSyncStatusForm_Shown(object sender, EventArgs e)
        {
            if (hasLoaded) return;
            hasLoaded = true;
            await LoadStatusAsync();
        }

        private async Task LoadStatusAsync()
        {
            if (busy || IsDisposed || Disposing) return;
            busy = true;
            btnRefresh.Enabled = false;
            btnRetry.Enabled = false;
            loading.BeginLoading("در حال دریافت وضعیت پایگاه‌ها و رویدادها…", global::FuntinoControlCenter.Properties.Resources.ConnectionSettings);
            try
            {
                EdgeSyncDiagnostics status = await Task.Run(delegate { return EdgeIntegrationRepository.GetDiagnostics(); });
                if (IsDisposed || Disposing) return;
                RenderStatus(status);
                gridEvents.DataSource = status.Events;
                UpdateRetryVisibility();
            }
            catch (UnauthorizedAccessException)
            {
                if (!IsDisposed && !Disposing)
                {
                    MessageBox.ShowWarning(this, "دسترسی مشاهدهٔ این صفحه تغییر کرده است. برای جلوگیری از نمایش اطلاعات قدیمی، پنجره بسته می‌شود.", "دسترسی محدود است");
                    Close();
                }
            }
            catch (Exception)
            {
                if (!IsDisposed && !Disposing)
                    MessageBox.ShowError(this, "وضعیت همگام‌سازی خوانده نشد. اتصال پایگاه اصلی را بررسی کنید و دوباره تلاش کنید.", "بارگذاری وضعیت");
            }
            finally
            {
                busy = false;
                if (!IsDisposed && !Disposing)
                {
                    loading.EndLoading();
                    btnRefresh.Enabled = true;
                    UpdateRetryVisibility();
                }
            }
        }

        private void RenderStatus(EdgeSyncDiagnostics status)
        {
            bool sameSnapshotBranch = String.Equals(status.BranchId, status.SnapshotBranchId, StringComparison.Ordinal);
            bool online = status.IsOnline && sameSnapshotBranch && status.HeartbeatUtc.HasValue && status.HeartbeatUtc.Value > DateTime.UtcNow.AddSeconds(-60);
            lblConnection.Text = "وضعیت همگام‌سازی: " + (online ? "آنلاین" : "آفلاین");
            lblConnection.ForeColor = online ? System.Drawing.Color.FromArgb(20, 110, 75) : System.Drawing.Color.FromArgb(165, 73, 55);
            lblLastResponse.Text = "آخرین پاسخ سرور: " + FormatUtc(status.LastServerResponseUtc);
            lblMainDatabase.Text = "پایگاه عملیاتی اصلی: " + (status.MainDatabaseAvailable ? "در دسترس" : "قطع است");
            lblDeviceDatabase.Text = "پایگاه عملیاتی دستگاه‌ها: " + (status.DeviceDatabaseAvailable ? "در دسترس" : "قطع است");
            lblBranch.Text = "شناسهٔ شعبه: " + (String.IsNullOrWhiteSpace(status.BranchId) ? "تنظیم نشده" : status.BranchId);
            lblSnapshot.Text = "تصویر شعبه: " + (String.IsNullOrWhiteSpace(status.SnapshotVersion) ? "دریافت نشده" : status.SnapshotVersion) +
                (sameSnapshotBranch ? String.Empty : "؛ تصویر قبلی مربوط به شعبهٔ " + (status.SnapshotBranchId ?? "نامشخص")) +
                "؛ دریافت " + FormatUtc(status.LastSnapshotAtUtc) + "؛ انقضا " + FormatUtc(status.SnapshotExpiresAtUtc);
            lblPending.Text = "رویدادهای در انتظار: " + PersianDigits(status.PendingCount.ToString("N0", CultureInfo.InvariantCulture)) +
                "؛ نیازمند نگاشت: " + PersianDigits(status.NeedsMappingCount.ToString("N0", CultureInfo.InvariantCulture)) +
                "؛ ردشده: " + PersianDigits(status.RejectedCount.ToString("N0", CultureInfo.InvariantCulture));
            lblDeadLetter.Text = "قرنطینه: " + PersianDigits(status.DeadLetterCount.ToString("N0", CultureInfo.InvariantCulture));
            lblLastSuccessfulSync.Text = "آخرین همگام‌سازی موفق: " + FormatUtc(status.LastSuccessfulSyncUtc);
            lblLastAcknowledged.Text = "آخرین رویداد تأییدشده: " +
                (status.LastAcknowledgedEventId.HasValue ? status.LastAcknowledgedEventId.Value.ToString("D") : "ندارد");
            lblError.Text = "آخرین خطا: " + (String.IsNullOrWhiteSpace(status.LastError) ? "ندارد" : status.LastError);
        }

        private void UpdateRetryVisibility()
        {
            if (IsDisposed || Disposing) return;
            bool authorized = AuthorizationGuard.Has("edge.sync.retry");
            btnRetry.Visible = authorized;
            btnRetry.Enabled = authorized && !busy && gridEvents.SelectedRows.Count == 1 && IsRetryable(gridEvents.SelectedRows[0]);
        }

        private static bool IsRetryable(DataGridViewRow row)
        {
            if (row == null || row.Cells["colEventId"].Value == null) return false;
            string status = Convert.ToString(row.Cells["colStatus"].Value, CultureInfo.InvariantCulture);
            return String.Equals(status, "rejected", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(status, "dead_letter", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(status, "needs_mapping", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(status, "needs_configuration", StringComparison.OrdinalIgnoreCase);
        }

        private async void Refresh_Click(object sender, EventArgs e)
        {
            await LoadStatusAsync();
        }

        private async void Retry_Click(object sender, EventArgs e)
        {
            try
            {
                AuthorizationGuard.Require("edge.sync.retry");
                if (gridEvents.SelectedRows.Count != 1) return;
                Guid eventId;
                if (!Guid.TryParse(Convert.ToString(gridEvents.SelectedRows[0].Cells["colEventId"].Value, CultureInfo.InvariantCulture), out eventId)) return;
                DialogResult answer = MessageBox.Show(this,
                    "فقط همان رویداد دوباره ارسال می‌شود. عملیات کارت و مبلغ مالی دوباره اجرا نمی‌شوند. ادامه می‌دهید؟",
                    "ارسال دوبارهٔ رویداد", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer != DialogResult.Yes) return;

                busy = true;
                btnRetry.Enabled = false;
                btnRefresh.Enabled = false;
                loading.BeginLoading("در حال آماده‌کردن همان رویداد برای ارسال…", global::FuntinoControlCenter.Properties.Resources.ConnectionSettings);
                bool reset = await Task.Run(delegate { return EdgeIntegrationRepository.RetryEvent(eventId); });
                if (reset)
                    MessageBox.ShowSuccess(this, "رویداد در صف ارسال قرار گرفت. برای بررسی نتیجه، وضعیت را به‌روزرسانی کنید.", "ارسال دوباره ثبت شد");
                else
                    MessageBox.ShowWarning(this, "این رویداد در همین شعبه برای ارسال دوباره پیدا نشد.", "رویداد پیدا نشد");
            }
            catch (UnauthorizedAccessException)
            {
                if (!IsDisposed && !Disposing) MessageBox.ShowWarning(this, "مجوز ارسال دوبارهٔ رویداد برای این حساب فعال نیست.", "دسترسی محدود است");
            }
            catch (Exception)
            {
                if (!IsDisposed && !Disposing) MessageBox.ShowError(this, "رویداد برای ارسال دوباره آماده نشد. وضعیت پایگاه و نگاشت شعبه را بررسی کنید.", "ارسال رویداد");
            }
            finally
            {
                busy = false;
                if (!IsDisposed && !Disposing)
                {
                    loading.EndLoading();
                    btnRefresh.Enabled = true;
                    await LoadStatusAsync();
                }
            }
        }

        private void GridEvents_SelectionChanged(object sender, EventArgs e)
        {
        }

        private void GridEvents_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Value == null || e.Value == DBNull.Value) return;
            string field = gridEvents.Columns[e.ColumnIndex].DataPropertyName;
            if (field == "SourceDatabase")
            {
                string name = Convert.ToString(e.Value, CultureInfo.InvariantCulture);
                e.Value = String.Equals(name, DatabaseManager.MainCatalog, StringComparison.OrdinalIgnoreCase) ? "پایگاه اصلی" : "پایگاه دستگاه‌ها";
                e.FormattingApplied = true;
            }
            else if (field == "AmountRial")
            {
                long rials = Convert.ToInt64(e.Value, CultureInfo.InvariantCulture);
                e.Value = rials % 10 == 0
                    ? PersianDigits((rials / 10).ToString("N0", CultureInfo.InvariantCulture))
                    : "—";
                e.FormattingApplied = true;
            }
            else if (field == "Status")
            {
                string state = Convert.ToString(e.Value, CultureInfo.InvariantCulture);
                e.Value = TranslateStatus(state);
                e.FormattingApplied = true;
            }
            else if (field == "TransactionKind")
            {
                string kind = Convert.ToString(e.Value, CultureInfo.InvariantCulture);
                e.Value = String.Equals(kind, "top_up", StringComparison.OrdinalIgnoreCase) ? "شارژ کارت" : kind;
                e.FormattingApplied = true;
            }
            else if (field == "CreatedAtUtc" && e.Value is DateTime)
            {
                DateTime value = DateTime.SpecifyKind((DateTime)e.Value, DateTimeKind.Utc).ToLocalTime();
                e.Value = PersianDigits(value.ToString("yyyy/MM/dd  HH:mm", CultureInfo.InvariantCulture));
                e.FormattingApplied = true;
            }
            UpdateRetryVisibility();
        }

        private static string TranslateStatus(string status)
        {
            switch ((status ?? String.Empty).ToLowerInvariant())
            {
                case "pending": return "در انتظار ارسال";
                case "retry": return "آمادهٔ تلاش دوباره";
                case "needs_mapping": return "نیازمند نگاشت";
                case "needs_configuration": return "نیازمند تنظیم";
                case "rejected": return "ردشده؛ نیازمند پیگیری";
                case "dead_letter": return "قرنطینه؛ نیازمند بررسی";
                case "synced": return "تأییدشده";
                default: return status;
            }
        }

        private static string FormatUtc(DateTime? value)
        {
            return value.HasValue
                ? PersianDigits(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc).ToLocalTime().ToString("yyyy/MM/dd  HH:mm", CultureInfo.InvariantCulture))
                : "ثبت نشده";
        }

        private static string PersianDigits(string value)
        {
            if (value == null) return String.Empty;
            return value.Replace('0', '۰').Replace('1', '۱').Replace('2', '۲').Replace('3', '۳').Replace('4', '۴')
                .Replace('5', '۵').Replace('6', '۶').Replace('7', '۷').Replace('8', '۸').Replace('9', '۹');
        }

        private void Back_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
