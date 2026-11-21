using System;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Security;
using FuntinoControlCenter.Services;
using MessageBox = FuntinoControlCenter.Forms.AppMessageService;

namespace FuntinoControlCenter.Forms
{
    public partial class CardRechargeRecoveryForm : Form
    {
        private bool busy;
        private bool loaded;

        public CardRechargeRecoveryForm()
        {
            InitializeComponent();
        }

        private void CardRechargeRecoveryForm_Load(object sender, EventArgs e)
        {
            try
            {
                RequireAccess();
                AuthorizationMonitor.Attach(this, HasAccess);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.ShowWarning(this, "مجوز لازم برای بازیابی شارژ برای این حساب فعال نیست.", "دسترسی محدود است");
                Close();
            }
        }

        private async void CardRechargeRecoveryForm_Shown(object sender, EventArgs e)
        {
            if (loaded) return;
            loaded = true;
            await LoadOperationsAsync();
        }

        private static bool HasAccess()
        {
            return AuthorizationGuard.Has("cards.recharge") &&
                AuthorizationGuard.Has("data.finance.create") &&
                AuthorizationGuard.Has("operations.recharge_terms.view") &&
                AuthorizationGuard.Has("edge.events.create");
        }

        private static void RequireAccess()
        {
            AuthorizationGuard.Require("cards.recharge");
            AuthorizationGuard.Require("data.finance.create");
            AuthorizationGuard.Require("operations.recharge_terms.view");
            AuthorizationGuard.Require("edge.events.create");
        }

        private async Task LoadOperationsAsync()
        {
            if (busy || IsDisposed || Disposing) return;
            busy = true;
            btnRefresh.Enabled = false;
            btnReconcile.Enabled = false;
            loading.BeginLoading("در حال بررسی عملیات شارژ نیمه‌تمام…", global::FuntinoControlCenter.Properties.Resources.CardRecharge);
            try
            {
                RequireAccess();
                DataTable rows = await Task.Run(delegate { return CardOperationsRepository.GetUnresolvedRechargeOperations(); });
                if (IsDisposed || Disposing) return;
                gridOperations.DataSource = rows;
                UpdateSelection();
            }
            catch (UnauthorizedAccessException)
            {
                if (!IsDisposed && !Disposing)
                {
                    MessageBox.ShowWarning(this, "مجوز بازیابی عملیات برای این حساب تغییر کرده است.", "دسترسی محدود است");
                    Close();
                }
            }
            catch (Exception)
            {
                if (!IsDisposed && !Disposing)
                    MessageBox.ShowError(this, "فهرست عملیات نیمه‌تمام خوانده نشد. پایگاه عملیاتی اصلی را بررسی کنید.", "بازیابی شارژ");
            }
            finally
            {
                busy = false;
                if (!IsDisposed && !Disposing)
                {
                    loading.EndLoading();
                    btnRefresh.Enabled = true;
                    UpdateSelection();
                }
            }
        }

        private void UpdateSelection()
        {
            if (IsDisposed || Disposing) return;
            btnReconcile.Enabled = !busy && HasAccess() && gridOperations.SelectedRows.Count == 1;
        }

        private async void Reconcile_Click(object sender, EventArgs e)
        {
            try
            {
                RequireAccess();
                if (gridOperations.SelectedRows.Count != 1) return;
                long operationId = Convert.ToInt64(gridOperations.SelectedRows[0].Cells["colOperationId"].Value, CultureInfo.InvariantCulture);
                DialogResult answer = MessageBox.Show(this,
                    "کارت روی کارت‌خوان خوانده می‌شود. فقط اگر شناسه و موجودی آن با عملیات انتخاب‌شده تطبیق داشته باشد، ثبت مالی محلی تکمیل می‌شود. هیچ فرمان نوشتنی روی کارت اجرا نخواهد شد. ادامه می‌دهید؟",
                    "تطبیق عملیات شارژ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer != DialogResult.Yes) return;

                busy = true;
                btnRefresh.Enabled = false;
                btnReconcile.Enabled = false;
                gridOperations.Enabled = false;
                loading.BeginLoading("در حال خواندن کارت و تطبیق مانده؛ کارت را روی دستگاه ثابت نگه دارید…", global::FuntinoControlCenter.Properties.Resources.CardReader);
                bool completed = await Task.Run(delegate { return CardReaderService.ReconcileRecharge(operationId); });
                if (IsDisposed || Disposing) return;
                if (completed)
                    MessageBox.ShowSuccess(this, "عملیات با کارت تطبیق داده شد و ثبت مالی محلی به‌همراه رویداد همگام‌سازی تکمیل شد. کارت دوباره نوشته نشد.", "بازیابی انجام شد");
                else
                    MessageBox.ShowWarning(this, "شناسه یا موجودی کارت با عملیات تطبیق نداشت. عملیات در وضعیت نامشخص باقی ماند و کارت نوشته نشد.", "نیازمند بررسی دستی");
            }
            catch (UnauthorizedAccessException)
            {
                if (!IsDisposed && !Disposing) MessageBox.ShowWarning(this, "مجوز لازم برای بازیابی عملیات فعال نیست.", "دسترسی محدود است");
            }
            catch (CardReaderException exception)
            {
                if (!IsDisposed && !Disposing) MessageBox.ShowWarning(this, exception.Message, "خواندن کارت کامل نشد");
            }
            catch (Exception)
            {
                if (!IsDisposed && !Disposing) MessageBox.ShowError(this, "عملیات بازیابی کامل نشد. کارت را دوباره شارژ نکنید و وضعیت پایگاه را بررسی کنید.", "بازیابی شارژ");
            }
            finally
            {
                busy = false;
                if (!IsDisposed && !Disposing)
                {
                    loading.EndLoading();
                    gridOperations.Enabled = true;
                    btnRefresh.Enabled = true;
                    await LoadOperationsAsync();
                }
            }
        }

        private async void Refresh_Click(object sender, EventArgs e)
        {
            await LoadOperationsAsync();
        }

        private void GridOperations_SelectionChanged(object sender, EventArgs e)
        {
            UpdateSelection();
        }

        private void GridOperations_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Value == null || e.Value == DBNull.Value) return;
            string field = gridOperations.Columns[e.ColumnIndex].DataPropertyName;
            if (field == "AmountToman")
            {
                e.Value = PersianDigits(Convert.ToInt64(e.Value, CultureInfo.InvariantCulture).ToString("N0", CultureInfo.InvariantCulture));
                e.FormattingApplied = true;
            }
            else if (field == "State")
            {
                string state = Convert.ToString(e.Value, CultureInfo.InvariantCulture);
                switch (state)
                {
                    case "started": e.Value = "آغازشده"; break;
                    case "card_written": e.Value = "کارت نوشته‌شده"; break;
                    case "verified": e.Value = "کارت تأییدشده"; break;
                    case "unknown": e.Value = "نامشخص"; break;
                    default: break;
                }
                e.FormattingApplied = true;
            }
            else if (field == "StartedAt" && e.Value is DateTime)
            {
                e.Value = PersianDigits(DateTime.SpecifyKind((DateTime)e.Value, DateTimeKind.Local).ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture));
                e.FormattingApplied = true;
            }
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
