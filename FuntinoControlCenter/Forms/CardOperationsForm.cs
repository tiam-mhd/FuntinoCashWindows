using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Security;
using FuntinoControlCenter.Services;

namespace FuntinoControlCenter.Forms
{
    public partial class CardOperationsForm : Form
    {
        private enum WorkKind
        {
            Information,
            Balance,
            Remainder,
            CardHistory,
            Recharge,
            DatabaseHistory
        }

        private sealed class WorkRequest
        {
            public WorkKind Kind { get; set; }
            public long Amount { get; set; }
            public byte[] CardUid { get; set; }
            public string Search { get; set; }
            public int Filter { get; set; }
            public int Page { get; set; }
            public int Sort { get; set; }
        }

        private sealed class WorkResult
        {
            public WorkKind Kind { get; set; }
            public object Value { get; set; }
        }

        private byte[] currentUid;
        private int historyPage = 1;
        private int historyFilter;
        private int historySort;
        private string historySearch = String.Empty;
        private int historyTotal;
        private bool showingDatabaseHistory;

        public CardOperationsForm()
        {
            InitializeComponent();
        }

        private void CardOperationsForm_Load(object sender, EventArgs e)
        {
            try
            {
                SetBusy(false);
                if (!HasAnyCardOperationAccess())
                {
                    MessageBox.Show(this, "برای استفاده از عملیات کارت مجوزی ندارید.", "دسترسی محدود", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Close();
                    return;
                }
                ApplyPermissions();
                AuthorizationMonitor.Attach(this, HasAnyCardOperationAccess);
                authorizationTimer.Start();
                SetStatus("کارت‌خوان پس از انتخاب یکی از عملیات‌ها، فقط همان یک‌بار درخواست کارت می‌فرستد.");
                ShowCardHistoryPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DisplayError(ex), "بررسی دسترسی", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
            }
        }

        private void AuthorizationTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (AppSession.Current == null)
                {
                    Close();
                    return;
                }
                ApplyPermissions();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DisplayError(ex), "بررسی دسترسی", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
            }
        }

        private void ApplyPermissions()
        {
            bool canInformation = AuthorizationGuard.Has("cards.details.view");
            bool canBalance = AuthorizationGuard.Has("cards.balance.view");
            bool canRemainder = AuthorizationGuard.Has("cards.remainder.view");
            bool canHistory = AuthorizationGuard.Has("cards.history.view");
            bool canRecharge = AuthorizationGuard.Has("cards.recharge") && AuthorizationGuard.Has(PermissionCatalog.TableCode("finance", "create"));
            bool canDatabaseHistory = canHistory && AuthorizationGuard.Has(PermissionCatalog.TableCode("finance", "view"));
            if (showingDatabaseHistory && !canDatabaseHistory) showingDatabaseHistory = false;

            btnReadInfo.Visible = canInformation;
            btnReadBalance.Visible = canBalance;
            btnReadRemainder.Visible = canRemainder;
            btnReadHistory.Visible = canHistory;
            btnRecharge.Visible = canRecharge;
            pnlAmountInput.Visible = lblAmountCaption.Visible = canRecharge;
            btnCardHistoryTab.Visible = canHistory;
            btnDatabaseHistoryTab.Visible = canDatabaseHistory;
            pnlCardHistory.Visible = canHistory && !showingDatabaseHistory;
            pnlDatabaseHistory.Visible = canDatabaseHistory && showingDatabaseHistory;

            lblCardNumberCaption.Visible = lblCardNumberValue.Visible = canInformation;
            lblCardTypeCaption.Visible = lblCardTypeValue.Visible = canInformation;
            lblDiscountCaption.Visible = lblDiscountValue.Visible = canInformation;
            lblExpirationCaption.Visible = lblExpirationValue.Visible = canInformation;
            lblBalanceCaption.Visible = lblBalanceValue.Visible = canBalance;

            bool hasActions = canInformation || canBalance || canRemainder || canHistory || canRecharge;
            pnlActions.Visible = hasActions;
            if (!hasActions) Close();
        }

        private bool HasAnyCardOperationAccess()
        {
            return AuthorizationGuard.HasAny("cards.details.view", "cards.balance.view", "cards.remainder.view", "cards.history.view") ||
                (AuthorizationGuard.Has("cards.recharge") && AuthorizationGuard.Has(PermissionCatalog.TableCode("finance", "create")));
        }

        private void ReadInfo_Click(object sender, EventArgs e)
        {
            StartWork(WorkKind.Information, "cards.details.view", "در حال دریافت اطلاعات کارت…");
        }

        private void ReadBalance_Click(object sender, EventArgs e)
        {
            StartWork(WorkKind.Balance, "cards.balance.view", "در حال خواندن موجودی کارت…");
        }

        private void ReadRemainder_Click(object sender, EventArgs e)
        {
            StartWork(WorkKind.Remainder, "cards.remainder.view", "در حال خواندن باقیمانده‌ی آخرین شارژ…");
        }

        private void ReadHistory_Click(object sender, EventArgs e)
        {
            if (!RequirePermission("cards.history.view")) return;
            ShowCardHistoryPanel();
            StartWork(WorkKind.CardHistory, "cards.history.view", "در حال خواندن سوابق ذخیره‌شده روی کارت…");
        }

        private void Recharge_Click(object sender, EventArgs e)
        {
            if (!RequirePermission("cards.recharge") || !RequirePermission(PermissionCatalog.TableCode("finance", "create"))) return;
            long amount;
            if (!TryParseAmount(txtAmount.Text, out amount) || amount <= 0 || amount > 4000000 || amount % 100 != 0)
            {
                MessageBox.Show(this, "مبلغ باید مضربی از ۱۰۰ تومان و حداکثر ۴٬۰۰۰٬۰۰۰ تومان باشد.", "مبلغ شارژ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtAmount.Focus();
                return;
            }

            DialogResult confirmation = MessageBox.Show(this,
                "کارت را روی کارت‌خوان بگذارید. مبلغ " + amount.ToString("N0", CultureInfo.GetCultureInfo("fa-IR")) + " تومان روی کارت نوشته شود؟",
                "تأیید شارژ کارت", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (confirmation != DialogResult.Yes) return;
            StartWork(WorkKind.Recharge, "cards.recharge", "در حال شارژ کارت؛ تا نمایش نتیجه کارت را برندارید.", amount);
        }

        private void CardHistoryTab_Click(object sender, EventArgs e)
        {
            ShowCardHistoryPanel();
        }

        private void DatabaseHistoryTab_Click(object sender, EventArgs e)
        {
            if (!RequirePermission("cards.history.view") || !RequirePermission(PermissionCatalog.TableCode("finance", "view"))) return;
            ShowDatabaseHistoryPanel();
            if (currentUid == null)
            {
                SetStatus("برای دیدن سوابق سامانه، ابتدا اطلاعات، موجودی یا سوابق همین کارت را بخوانید.");
                return;
            }
            historyPage = 1;
            StartDatabaseHistoryRead();
        }

        private void SearchHistory_Click(object sender, EventArgs e)
        {
            if (!RequirePermission("cards.history.view") || !RequirePermission(PermissionCatalog.TableCode("finance", "view"))) return;
            historyPage = 1;
            historySearch = (txtHistorySearch.Text ?? String.Empty).Trim();
            historyFilter = Math.Max(0, cmbHistoryFilter.SelectedIndex);
            StartDatabaseHistoryRead();
        }

        private void PreviousHistory_Click(object sender, EventArgs e)
        {
            if (historyPage <= 1) return;
            historyPage--;
            StartDatabaseHistoryRead();
        }

        private void NextHistory_Click(object sender, EventArgs e)
        {
            int pageCount = Math.Max(1, (int)Math.Ceiling((double)historyTotal / CardOperationsRepository.HistoryPageSize));
            if (historyPage >= pageCount) return;
            historyPage++;
            StartDatabaseHistoryRead();
        }

        private void HistorySearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            SearchHistory_Click(sender, EventArgs.Empty);
        }

        private void GridRechargeHistory_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == colRechargeAmount.Index)
                historySort = historySort == 1 ? 2 : 1;
            else if (e.ColumnIndex == colRechargeDate.Index)
                historySort = 0;
            else
                return;
            historyPage = 1;
            StartDatabaseHistoryRead();
        }

        private void Close_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void CardOperationsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool hasCardAccess;
            try { hasCardAccess = HasAnyCardOperationAccess(); }
            catch { hasCardAccess = false; }
            if (operationWorker.IsBusy && hasCardAccess)
            {
                e.Cancel = true;
                MessageBox.Show(this, "عملیات کارت در حال انجام است. تا نمایش نتیجه صبر کنید.", "عملیات در جریان", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            if (!e.Cancel) authorizationTimer.Stop();
        }

        private void StartWork(WorkKind kind, string permission, string status, long amount = 0)
        {
            if (!RequirePermission(permission)) return;
            if (kind == WorkKind.Recharge && !RequirePermission(PermissionCatalog.TableCode("finance", "create"))) return;
            if (operationWorker.IsBusy)
            {
                SetStatus("درخواست قبلی هنوز در حال انجام است؛ تا پایان آن صبر کنید.");
                return;
            }

            SetBusy(true);
            SetStatus(status);
            try
            {
                operationWorker.RunWorkerAsync(new WorkRequest { Kind = kind, Amount = amount });
            }
            catch (Exception ex)
            {
                SetBusy(false);
                SetStatus(DisplayError(ex));
            }
        }

        private void StartDatabaseHistoryRead()
        {
            if (currentUid == null)
            {
                SetStatus("ابتدا کارت را بخوانید تا سوابق همان کارت جست‌وجو شود.");
                return;
            }
            if (!RequirePermission("cards.history.view") || !RequirePermission(PermissionCatalog.TableCode("finance", "view"))) return;
            if (operationWorker.IsBusy)
            {
                SetStatus("درخواست قبلی هنوز در حال انجام است؛ تا پایان آن صبر کنید.");
                return;
            }

            ShowDatabaseHistoryPanel();
            SetBusy(true);
            SetStatus("در حال جست‌وجوی سوابق شارژ این کارت…");
            try
            {
                operationWorker.RunWorkerAsync(new WorkRequest
                {
                    Kind = WorkKind.DatabaseHistory,
                    CardUid = (byte[])currentUid.Clone(),
                    Search = historySearch,
                    Filter = historyFilter,
                    Page = historyPage,
                    Sort = historySort
                });
            }
            catch (Exception ex)
            {
                SetBusy(false);
                SetStatus(DisplayError(ex));
            }
        }

        private void OperationWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var request = (WorkRequest)e.Argument;
            object result;
            switch (request.Kind)
            {
                case WorkKind.Information: result = CardReaderService.ReadInformation(); break;
                case WorkKind.Balance: result = CardReaderService.ReadBalance(); break;
                case WorkKind.Remainder: result = CardReaderService.ReadRemainder(); break;
                case WorkKind.CardHistory: result = CardReaderService.ReadHistory(); break;
                case WorkKind.Recharge: result = CardReaderService.Recharge(request.Amount); break;
                case WorkKind.DatabaseHistory:
                    result = CardOperationsRepository.GetRechargeHistory(request.CardUid, request.Page, request.Search, request.Filter, request.Sort);
                    break;
                default: throw new InvalidOperationException("نوع عملیات کارت شناخته‌شده نیست.");
            }
            e.Result = new WorkResult { Kind = request.Kind, Value = result };
        }

        private void OperationWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            SetBusy(false);
            if (e.Error != null)
            {
                SetStatus(DisplayError(e.Error));
                return;
            }
            if (e.Cancelled) return;

            try
            {
                ApplyPermissions();
                var result = (WorkResult)e.Result;
                switch (result.Kind)
                {
                    case WorkKind.Information:
                        ShowCardInformation((CardInformation)result.Value);
                        SetStatus("اطلاعات کارت دریافت شد.");
                        break;
                    case WorkKind.Balance:
                        ShowCardBalance((CardInformation)result.Value);
                        SetStatus("موجودی کارت خوانده شد.");
                        break;
                    case WorkKind.Remainder:
                        ShowCardRemainder((CardRemainder)result.Value);
                        break;
                    case WorkKind.CardHistory:
                        ShowCardHistory((CardHistoryReadResult)result.Value);
                        SetStatus("سوابق کارت دریافت شد.");
                        if (CanViewDatabaseHistory())
                        {
                            historyPage = 1;
                            StartDatabaseHistoryRead();
                        }
                        break;
                    case WorkKind.Recharge:
                        ShowRechargeResult((CardRechargeResult)result.Value);
                        break;
                    case WorkKind.DatabaseHistory:
                        ShowDatabaseHistory((RechargeHistoryPage)result.Value);
                        break;
                }
            }
            catch (Exception ex)
            {
                SetStatus(DisplayError(ex));
            }
        }

        private void ShowCardInformation(CardInformation information)
        {
            UpdateCurrentCard(information.UidBytes);
            if (!AuthorizationGuard.Has("cards.details.view")) return;
            lblCardNumberValue.Text = information.Uid;
            lblCardTypeValue.Text = information.CardType;
            lblDiscountValue.Text = information.DiscountPercent.ToString("N0", CultureInfo.GetCultureInfo("fa-IR")) + "٪";
            lblExpirationValue.Text = information.ExpirationDate;
        }

        private void ShowCardBalance(CardInformation information)
        {
            UpdateCurrentCard(information.UidBytes);
            if (AuthorizationGuard.Has("cards.balance.view") && information.BalanceToman.HasValue)
                lblBalanceValue.Text = information.BalanceToman.Value.ToString("N0", CultureInfo.GetCultureInfo("fa-IR")) + " تومان";
            if (AuthorizationGuard.Has("cards.details.view")) lblCardNumberValue.Text = information.Uid;
        }

        private void ShowCardRemainder(CardRemainder remainder)
        {
            UpdateCurrentCard(remainder.UidBytes);
            if (AuthorizationGuard.Has("cards.remainder.view"))
                SetStatus("باقیمانده‌ی آخرین شارژ: " + remainder.AmountToman.ToString("N0", CultureInfo.GetCultureInfo("fa-IR")) + " تومان");
        }

        private void ShowCardHistory(CardHistoryReadResult result)
        {
            UpdateCurrentCard(result.UidBytes);
            if (!AuthorizationGuard.Has("cards.history.view")) return;
            if (AuthorizationGuard.Has("cards.details.view")) lblCardNumberValue.Text = result.Uid;

            var rows = new DataTable();
            rows.Columns.Add("ActivityType", typeof(string));
            rows.Columns.Add("Amount", typeof(long));
            rows.Columns.Add("Date", typeof(string));
            rows.Columns.Add("Time", typeof(string));
            rows.Columns.Add("Source", typeof(string));

            foreach (CardRechargeHistoryEntry entry in result.RechargeEntries)
                rows.Rows.Add("شارژ کارت", entry.AmountToman, entry.Date, entry.Time,
                    entry.CashRegisterCode == 0 ? "صندوق ثبت نشده" : "صندوق " + entry.CashRegisterCode.ToString("N0", CultureInfo.GetCultureInfo("fa-IR")));
            foreach (CardUsageHistoryEntry entry in result.UsageEntries)
                rows.Rows.Add("استفاده از کارت", DBNull.Value, entry.Date, entry.Time, "ثبت استفاده");
            gridCardActivity.DataSource = rows;
            ShowCardHistoryPanel();
        }

        private void ShowRechargeResult(CardRechargeResult result)
        {
            UpdateCurrentCard(result.UidBytes);
            if (AuthorizationGuard.Has("cards.balance.view"))
                lblBalanceValue.Text = result.BalanceToman.ToString("N0", CultureInfo.GetCultureInfo("fa-IR")) + " تومان";
            if (AuthorizationGuard.Has("cards.details.view")) lblCardNumberValue.Text = result.Uid;

            string message = result.HistoryRecorded ? "شارژ کارت و ثبت سابقه در سامانه انجام شد." : "شارژ کارت انجام شد، اما سابقه در سامانه ثبت نشد.";
            if (!String.IsNullOrWhiteSpace(result.Warning) || !result.HistoryRecorded)
                message += Environment.NewLine + "شارژ روی کارت تأیید شده است؛ برای جلوگیری از شارژ دوباره، عملیات را تکرار نکنید.";
            if (!String.IsNullOrWhiteSpace(result.Warning)) message += Environment.NewLine + result.Warning;
            SetStatus(message);
        }

        private void ShowDatabaseHistory(RechargeHistoryPage page)
        {
            if (!CanViewDatabaseHistory()) return;
            gridRechargeHistory.DataSource = page.Rows;
            historyTotal = page.TotalCount;
            int pageCount = Math.Max(1, (int)Math.Ceiling((double)historyTotal / page.PageSize));
            lblHistoryPages.Text = "صفحه " + ToPersianDigits(page.PageNumber.ToString(CultureInfo.InvariantCulture)) + " از " +
                ToPersianDigits(pageCount.ToString(CultureInfo.InvariantCulture)) + "  |  " + ToPersianDigits(historyTotal.ToString("N0", CultureInfo.GetCultureInfo("fa-IR"))) + " مورد";
            btnPreviousHistory.Enabled = page.PageNumber > 1;
            btnNextHistory.Enabled = page.PageNumber < pageCount;
            ShowDatabaseHistoryPanel();
            SetStatus(historyTotal == 0 ? "برای این کارت سابقه‌ای در سامانه پیدا نشد." : "سوابق شارژ سامانه به‌روز شد.");
        }

        private bool CanViewDatabaseHistory()
        {
            return AuthorizationGuard.Has("cards.history.view") && AuthorizationGuard.Has(PermissionCatalog.TableCode("finance", "view"));
        }

        private void UpdateCurrentCard(byte[] uid)
        {
            if (uid == null || uid.Length != 4) return;
            bool changed = currentUid == null || !currentUid.SequenceEqual(uid);
            if (changed)
            {
                lblBalanceValue.Text = "برای خواندن کارت اقدام کنید";
                lblCardNumberValue.Text = "—";
                lblCardTypeValue.Text = "—";
                lblDiscountValue.Text = "—";
                lblExpirationValue.Text = "—";
                gridCardActivity.DataSource = null;
                gridRechargeHistory.DataSource = null;
                historyPage = 1;
                historyTotal = 0;
            }
            currentUid = (byte[])uid.Clone();
        }

        private void ShowCardHistoryPanel()
        {
            showingDatabaseHistory = false;
            pnlCardHistory.Visible = AuthorizationGuard.Has("cards.history.view");
            pnlDatabaseHistory.Visible = false;
        }

        private void ShowDatabaseHistoryPanel()
        {
            showingDatabaseHistory = true;
            pnlCardHistory.Visible = false;
            pnlDatabaseHistory.Visible = CanViewDatabaseHistory();
        }

        private void SetBusy(bool busy)
        {
            btnReadInfo.Enabled = !busy;
            btnReadBalance.Enabled = !busy;
            btnReadRemainder.Enabled = !busy;
            btnReadHistory.Enabled = !busy;
            btnRecharge.Enabled = !busy;
            btnCardHistoryTab.Enabled = !busy;
            btnDatabaseHistoryTab.Enabled = !busy;
            btnSearchHistory.Enabled = !busy;
            btnPreviousHistory.Enabled = !busy && historyPage > 1;
            btnNextHistory.Enabled = !busy && historyPage < Math.Max(1, (int)Math.Ceiling((double)historyTotal / CardOperationsRepository.HistoryPageSize));
            cmbHistoryFilter.Enabled = !busy;
            txtAmount.Enabled = !busy;
            txtHistorySearch.Enabled = !busy;
            UseWaitCursor = busy;
        }

        private void SetStatus(string message)
        {
            lblOperationStatus.Text = message ?? String.Empty;
        }

        private bool RequirePermission(string permission)
        {
            try
            {
                AuthorizationGuard.Require(permission);
                return true;
            }
            catch (Exception ex)
            {
                SetStatus(DisplayError(ex));
                return false;
            }
        }

        private static string DisplayError(Exception exception)
        {
            var sqlException = exception as SqlException;
            if (sqlException != null)
                return "خواندن یا ثبت اطلاعات در پایگاه انجام نشد. اتصال و دسترسی پایگاه را بررسی کنید. کد خطا: " + sqlException.Number.ToString(CultureInfo.InvariantCulture);
            return DatabaseManager.SafeMessage(exception);
        }

        private static bool TryParseAmount(string value, out long amount)
        {
            string normalized = NormalizeDigits(value ?? String.Empty).Replace(",", String.Empty).Replace("،", String.Empty).Replace("٬", String.Empty).Replace(" ", String.Empty).Trim();
            return Int64.TryParse(normalized, NumberStyles.None, CultureInfo.InvariantCulture, out amount);
        }

        private static string NormalizeDigits(string value)
        {
            var result = new StringBuilder(value.Length);
            foreach (char character in value)
            {
                int digit = Char.GetNumericValue(character) >= 0 && Char.GetNumericValue(character) <= 9
                    ? (int)Char.GetNumericValue(character)
                    : -1;
                result.Append(digit < 0 ? character : (char)('0' + digit));
            }
            return result.ToString();
        }

        private static string ToPersianDigits(string value)
        {
            var result = new StringBuilder(value.Length);
            foreach (char character in value)
                result.Append(character >= '0' && character <= '9' ? (char)('۰' + character - '0') : character);
            return result.ToString();
        }
    }
}
