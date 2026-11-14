using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MessageBox = FuntinoControlCenter.Forms.AppMessageService;
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
        private bool formattingAmount;
        private bool keyboardRechargeShortcut;

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
                SetStatus(CardReaderService.GetConfiguredPortStatus());
                ShowCardHistoryPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DisplayError(ex), "بررسی دسترسی", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
            }
        }

        private void CardOperationsForm_Shown(object sender, EventArgs e)
        {
            if (btnRecharge.Visible && txtAmount.Enabled)
            {
                txtAmount.Focus();
                txtAmount.SelectAll();
                return;
            }

            if (btnReadBalance.Visible) btnReadBalance.Focus();
            else if (btnReadInfo.Visible) btnReadInfo.Focus();
            else if (btnReadRemainder.Visible) btnReadRemainder.Focus();
            else if (btnReadHistory.Visible) btnReadHistory.Focus();
        }

        private void CardOperationsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F && pnlDatabaseHistory.Visible && txtHistorySearch.Enabled)
            {
                txtHistorySearch.Focus();
                txtHistorySearch.SelectAll();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            // Do not steal cursor-navigation keys while the operator edits search text.
            if (txtHistorySearch.Focused || cmbHistoryFilter.Focused || cmbHistoryFilter.DroppedDown)
                return;

            if (e.Control && pnlDatabaseHistory.Visible && e.KeyCode == Keys.Left)
            {
                if (btnPreviousHistory.Enabled) RunShortcut(e, PreviousHistory_Click);
                return;
            }
            if (e.Control && pnlDatabaseHistory.Visible && e.KeyCode == Keys.Right)
            {
                if (btnNextHistory.Enabled) RunShortcut(e, NextHistory_Click);
                return;
            }

            if (e.Control || e.Alt) return;

            switch (e.KeyCode)
            {
                case Keys.P:
                    if (txtAmount.Focused && btnRecharge.Visible && btnRecharge.Enabled)
                        RunRechargeShortcut(e);
                    break;
                case Keys.Enter:
                    if (txtAmount.Focused && btnRecharge.Visible && btnRecharge.Enabled)
                        RunRechargeShortcut(e);
                    break;
                case Keys.F1:
                    if (btnReadInfo.Visible && btnReadInfo.Enabled)
                        RunShortcut(e, ReadInfo_Click);
                    break;
                case Keys.F2:
                    if (btnReadBalance.Visible && btnReadBalance.Enabled)
                        RunShortcut(e, ReadBalance_Click);
                    break;
                case Keys.F3:
                    if (btnReadHistory.Visible && btnReadHistory.Enabled)
                        RunShortcut(e, ReadHistory_Click);
                    break;
                case Keys.F4:
                    if (btnReadRemainder.Visible && btnReadRemainder.Enabled)
                        RunShortcut(e, ReadRemainder_Click);
                    break;
                case Keys.F5:
                    if (btnDatabaseHistoryTab.Visible && btnDatabaseHistoryTab.Enabled)
                        RunShortcut(e, DatabaseHistoryTab_Click);
                    break;
                case Keys.F6:
                    if (btnCardHistoryTab.Visible && btnCardHistoryTab.Enabled)
                        RunShortcut(e, CardHistoryTab_Click);
                    break;
            }
        }

        private void RunShortcut(KeyEventArgs e, EventHandler action)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            action(this, EventArgs.Empty);
        }

        private void RunRechargeShortcut(KeyEventArgs e)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            keyboardRechargeShortcut = true;
            try
            {
                Recharge_Click(this, EventArgs.Empty);
            }
            finally
            {
                keyboardRechargeShortcut = false;
            }
        }

        private void Amount_TextChanged(object sender, EventArgs e)
        {
            if (formattingAmount) return;

            string original = txtAmount.Text ?? String.Empty;
            int caret = Math.Max(0, Math.Min(txtAmount.SelectionStart, original.Length));
            int digitsBeforeCaret = ExtractAmountDigits(original.Substring(0, caret)).Length;
            string formatted = FormatAmountForEntry(ExtractAmountDigits(original));
            if (String.Equals(original, formatted, StringComparison.Ordinal)) return;

            formattingAmount = true;
            try
            {
                txtAmount.Text = formatted;
                int formattedCaret = 0;
                int digitsPassed = 0;
                while (formattedCaret < formatted.Length && digitsPassed < digitsBeforeCaret)
                {
                    if (Char.IsDigit(formatted[formattedCaret])) digitsPassed++;
                    formattedCaret++;
                }
                txtAmount.SelectionStart = formattedCaret;
                txtAmount.SelectionLength = 0;
            }
            finally
            {
                formattingAmount = false;
            }
        }

        private static string ExtractAmountDigits(string value)
        {
            string normalized = NormalizeDigits(value ?? String.Empty);
            var digits = new StringBuilder(normalized.Length);
            foreach (char character in normalized)
                if (character >= '0' && character <= '9') digits.Append(character);
            return digits.ToString();
        }

        private static string FormatAmountForEntry(string digits)
        {
            if (String.IsNullOrEmpty(digits)) return String.Empty;
            var formatted = new StringBuilder(digits.Length + digits.Length / 3);
            for (int index = 0; index < digits.Length; index++)
            {
                if (index > 0 && (digits.Length - index) % 3 == 0) formatted.Append('٬');
                formatted.Append((char)('۰' + digits[index] - '0'));
            }
            return formatted.ToString();
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

            UpdateShortcutHint(canRecharge, canInformation, canBalance, canHistory, canRemainder, canDatabaseHistory);

            bool hasActions = canInformation || canBalance || canRemainder || canHistory || canRecharge;
            pnlActions.Visible = hasActions;
            if (!hasActions) Close();
        }

        private void UpdateShortcutHint(bool canRecharge, bool canInformation, bool canBalance, bool canHistory, bool canRemainder, bool canDatabaseHistory)
        {
            var shortcuts = new System.Collections.Generic.List<string>();
            AddShortcutHint(shortcuts, btnRecharge, canRecharge);
            AddShortcutHint(shortcuts, btnReadInfo, canInformation);
            AddShortcutHint(shortcuts, btnReadBalance, canBalance);
            AddShortcutHint(shortcuts, btnReadHistory, canHistory);
            AddShortcutHint(shortcuts, btnReadRemainder, canRemainder);
            AddShortcutHint(shortcuts, btnDatabaseHistoryTab, canDatabaseHistory);
            AddShortcutHint(shortcuts, btnCardHistoryTab, canHistory);

            if (canDatabaseHistory)
            {
                shortcuts.Add("کادر جست‌وجو: Ctrl+F");
                AddShortcutHint(shortcuts, btnSearchHistory, true);
                AddShortcutHint(shortcuts, btnPreviousHistory, true);
                AddShortcutHint(shortcuts, btnNextHistory, true);
            }

            lblAmountHint.Text = String.Join("  |  ", shortcuts.ToArray());
        }

        private static void AddShortcutHint(System.Collections.Generic.List<string> shortcuts, Control button, bool isAllowed)
        {
            if (!isAllowed || button == null || String.IsNullOrWhiteSpace(button.Text)) return;

            // The Designer caption separates the action from its shortcut with three spaces.
            int separatorIndex = button.Text.LastIndexOf("   ", StringComparison.Ordinal);
            if (separatorIndex <= 0 || separatorIndex + 3 >= button.Text.Length) return;

            string action = button.Text.Substring(0, separatorIndex).Trim();
            string keys = button.Text.Substring(separatorIndex + 3).Trim();
            if (action.Length == 0 || keys.Length == 0) return;

            shortcuts.Add(action + ": " + keys);
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
            if (String.IsNullOrWhiteSpace(txtAmount.Text))
            {
                AppMessageService.ShowWarning(this, "مبلغ شارژ را وارد کنید.", "مبلغ وارد نشده است");
                txtAmount.Focus();
                return;
            }

            if (!TryParseAmount(txtAmount.Text, out amount))
            {
                AppMessageService.ShowWarning(this, "مبلغ را فقط با رقم وارد کنید.", "مبلغ نامعتبر است");
                txtAmount.Focus();
                return;
            }

            if (amount == 0)
            {
                AppMessageService.ShowWarning(this, "مبلغ شارژ نمی‌تواند صفر باشد. مبلغی بیشتر از صفر وارد کنید.", "شارژ با مبلغ صفر انجام نمی‌شود");
                txtAmount.Focus();
                txtAmount.SelectAll();
                return;
            }

            if (amount > 4000000)
            {
                AppMessageService.ShowWarning(this, "حداکثر مبلغ هر بار شارژ ۴٬۰۰۰٬۰۰۰ تومان است.", "مبلغ بیش از سقف مجاز است");
                txtAmount.Focus();
                txtAmount.SelectAll();
                return;
            }

            if (amount % 100 != 0)
            {
                AppMessageService.ShowWarning(this, "مبلغ شارژ باید مضربی از ۱۰۰ تومان باشد.", "مبلغ نامعتبر است");
                txtAmount.Focus();
                txtAmount.SelectAll();
                return;
            }

            if (!keyboardRechargeShortcut)
            {
                DialogResult confirmation = MessageBox.Show(this,
                    "کارت را روی کارت‌خوان بگذارید. مبلغ " + amount.ToString("N0", CultureInfo.GetCultureInfo("fa-IR")) + " تومان روی کارت نوشته شود؟",
                    "تأیید شارژ کارت", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (confirmation != DialogResult.Yes) return;
            }
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
                ShowOperationFailure(ex);
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
                ShowOperationFailure(ex);
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
                ShowOperationFailure(e.Error);
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
                ShowOperationFailure(ex);
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
            txtAmount.Clear();
            UpdateCurrentCard(result.UidBytes);
            if (AuthorizationGuard.Has("cards.balance.view"))
                lblBalanceValue.Text = result.BalanceToman.ToString("N0", CultureInfo.GetCultureInfo("fa-IR")) + " تومان";
            if (AuthorizationGuard.Has("cards.details.view")) lblCardNumberValue.Text = result.Uid;

            string message = result.HistoryRecorded ? "شارژ کارت و ثبت سابقه در سامانه انجام شد." : "شارژ کارت انجام شد، اما سابقه در سامانه ثبت نشد.";
            if (!String.IsNullOrWhiteSpace(result.Warning) || !result.HistoryRecorded)
            {
                message += Environment.NewLine + "شارژ روی کارت تأیید شده است؛ برای جلوگیری از شارژ دوباره، عملیات را تکرار نکنید.";
            }
            if (!String.IsNullOrWhiteSpace(result.Warning)) message += Environment.NewLine + result.Warning;
            SetStatus(message);
            if (!String.IsNullOrWhiteSpace(result.Warning) || !result.HistoryRecorded)
                AppMessageService.ShowWarning(this, message, "شارژ کارت انجام شد؛ سوابق نیاز به بررسی دارد");
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
                ShowOperationFailure(ex);
                return false;
            }
        }

        private static string DisplayError(Exception exception)
        {
            exception = UnwrapException(exception);
            var readerException = exception as CardReaderException;
            if (readerException != null) return readerException.Message;
            var sqlException = exception as SqlException;
            if (sqlException != null)
                return "خواندن یا ثبت اطلاعات در پایگاه انجام نشد. اتصال و دسترسی پایگاه را بررسی کنید. کد خطا: " + sqlException.Number.ToString(CultureInfo.InvariantCulture);
            var unauthorized = exception as UnauthorizedAccessException;
            if (unauthorized != null) return unauthorized.Message;
            var invalidOperation = exception as InvalidOperationException;
            if (invalidOperation != null) return invalidOperation.Message;
            if (exception is TimeoutException)
                return "کارت‌خوان پاسخ نداد. کارت را روی دستگاه بگذارید و اتصال و درگاه انتخاب‌شده را بررسی کنید.";
            if (exception is System.IO.IOException)
                return "ارتباط با کارت‌خوان قطع شد. اتصال کابل و درگاه انتخاب‌شده را بررسی کنید.";
            return "عملیات انجام نشد. اتصال کارت‌خوان و تنظیمات را بررسی کنید و دوباره تلاش کنید.";
        }

        private void ShowOperationFailure(Exception exception)
        {
            Exception actual = UnwrapException(exception);
            string message = DisplayError(actual);
            SetStatus(message);
            var readerException = actual as CardReaderException;
            if (readerException != null)
            {
                string title = GetReaderFailureTitle(readerException.FailureKind);
                if (readerException.FailureKind == CardReaderFailureKind.ReaderUnavailable ||
                    readerException.FailureKind == CardReaderFailureKind.ReaderNotResponding ||
                    readerException.FailureKind == CardReaderFailureKind.RechargeResultUnknown)
                    AppMessageService.ShowError(this, message, title);
                else
                    AppMessageService.ShowWarning(this, message, title);
                return;
            }

            if (actual is UnauthorizedAccessException)
                AppMessageService.ShowWarning(this, message, "دسترسی مجاز نیست");
            else
                AppMessageService.ShowError(this, message, "عملیات انجام نشد");
        }

        private static string GetReaderFailureTitle(CardReaderFailureKind failureKind)
        {
            switch (failureKind)
            {
                case CardReaderFailureKind.NoCardOrReaderResponse: return "کارت یا کارت‌خوان پاسخ نداد";
                case CardReaderFailureKind.ReaderUnavailable: return "کارت‌خوان در دسترس نیست";
                case CardReaderFailureKind.ReaderNotResponding: return "پاسخی از کارت‌خوان نرسید";
                case CardReaderFailureKind.CardKeyRejected: return "خواندن اطلاعات کارت ممکن نشد";
                case CardReaderFailureKind.RechargeResultUnknown: return "وضعیت شارژ مشخص نیست";
                case CardReaderFailureKind.CardIdentifierUnavailable:
                case CardReaderFailureKind.CardSelectionFailed:
                case CardReaderFailureKind.CardDataUnavailable: return "خواندن کارت کامل نشد";
                case CardReaderFailureKind.CardWriteRejected: return "نوشتن روی کارت انجام نشد";
                default: return "عملیات کارت انجام نشد";
            }
        }

        private static Exception UnwrapException(Exception exception)
        {
            while (exception != null && exception.InnerException != null &&
                (exception is System.Reflection.TargetInvocationException || exception is AggregateException))
                exception = exception.InnerException;
            return exception ?? new InvalidOperationException("عملیات انجام نشد.");
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
