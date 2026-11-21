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
    public partial class CustomerFamilyForm : Form
    {
        private const int PageSize = 50;
        private int currentPage = 1;
        private int pageCount = 1;
        private bool canViewFamily;
        private bool customerLoadBusy;
        private bool bindingCustomers;
        private int familyRequestVersion;

        private sealed class CustomerPageResult
        {
            public DataTable Rows { get; set; }
            public int Total { get; set; }
        }

        private sealed class FamilyPageResult
        {
            public DataTable Members { get; set; }
            public DataTable Cards { get; set; }
        }

        public CustomerFamilyForm()
        {
            InitializeComponent();
        }

        private async void CustomerFamilyForm_Load(object sender, EventArgs e)
        {
            try
            {
                AuthorizationGuard.Require("customers.view");
                canViewFamily = AuthorizationGuard.Has("families.view");
                pnlFamily.Visible = canViewFamily;
                AuthorizationMonitor.Attach(this, delegate
                {
                    return AuthorizationGuard.Has("customers.view") && (!canViewFamily || AuthorizationGuard.Has("families.view"));
                });
                await LoadSnapshotStatusAsync();
                await LoadCustomersAsync(1);
            }
            catch (UnauthorizedAccessException)
            {
                if (IsDisposed || Disposing) return;
                MessageBox.ShowWarning(this, "دسترسی مشاهدهٔ مشتریان شعبه برای این حساب فعال نیست.", "دسترسی محدود است");
                Close();
            }
            catch (Exception)
            {
                if (IsDisposed || Disposing) return;
                MessageBox.ShowError(this, "اطلاعات مشتریان بارگذاری نشد. وضعیت پایگاه عملیاتی را بررسی کنید.", "بارگذاری اطلاعات");
            }
        }

        private async Task LoadSnapshotStatusAsync()
        {
            EdgeSnapshotStatus snapshot = await Task.Run(delegate { return EdgeIntegrationRepository.GetCustomerSnapshotStatus(); });
            if (IsDisposed || Disposing) return;
            string freshness = snapshot.IsFresh ? "تازه" : "منقضی یا دریافت‌نشده";
            string version = String.IsNullOrWhiteSpace(snapshot.Version) ? "دریافت نشده" : snapshot.Version;
            string expiry = snapshot.ExpiresAtUtc.HasValue
                ? PersianDigits(DateTime.SpecifyKind(snapshot.ExpiresAtUtc.Value, DateTimeKind.Utc).ToLocalTime().ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture))
                : "ثبت نشده";
            lblSnapshot.Text = "نمایش از حافظهٔ محلی مجاز شعبه «" + snapshot.BranchId + "»؛ نسخهٔ " + version +
                "؛ پایان اعتبار " + expiry + "؛ وضعیت: " + freshness;
        }

        private async Task LoadCustomersAsync(int page)
        {
            if (customerLoadBusy || IsDisposed || Disposing) return;
            customerLoadBusy = true;
            familyRequestVersion++;
            currentPage = Math.Max(1, Math.Min(page, pageCount));
            btnSearch.Enabled = false;
            btnPrevious.Enabled = false;
            btnNext.Enabled = false;
            gridCustomers.Enabled = false;
            loadingCustomers.BeginLoading("در حال جست‌وجوی مشتریان مجاز شعبه…", global::FuntinoControlCenter.Properties.Resources.Reports);
            try
            {
                string term = txtSearch.Text;
                int requestedPage = currentPage;
                CustomerPageResult result = await Task.Run(delegate
                {
                    int total;
                    DataTable rows = EdgeIntegrationRepository.SearchCustomers(term, requestedPage, PageSize, out total);
                    return new CustomerPageResult { Rows = rows, Total = total };
                });
                if (IsDisposed || Disposing) return;
                bindingCustomers = true;
                gridCustomers.DataSource = result.Rows;
                pageCount = Math.Max(1, (int)Math.Ceiling(result.Total / (double)PageSize));
                currentPage = Math.Min(requestedPage, pageCount);
                lblCustomerPage.Text = "صفحهٔ " + PersianDigits(currentPage.ToString(CultureInfo.InvariantCulture)) +
                    " از " + PersianDigits(pageCount.ToString(CultureInfo.InvariantCulture));
                lblCustomerCount.Text = PersianDigits(result.Total.ToString("N0", CultureInfo.InvariantCulture)) + " مشتری";
                btnPrevious.Enabled = currentPage > 1;
                btnNext.Enabled = currentPage < pageCount;
                if (gridCustomers.Rows.Count > 0)
                {
                    gridCustomers.ClearSelection();
                    gridCustomers.Rows[0].Selected = true;
                    gridCustomers.CurrentCell = gridCustomers.Rows[0].Cells[0];
                }
                else
                {
                    gridFamily.DataSource = null;
                    gridCards.DataSource = null;
                    lblFamilyCustomer.Text = canViewFamily ? "برای دیدن اطلاعات خانواده، یک مشتری را انتخاب کنید." : String.Empty;
                }
                bindingCustomers = false;
                if (gridCustomers.Rows.Count > 0 && canViewFamily)
                    CustomerSelectionChanged(this, EventArgs.Empty);
            }
            catch (UnauthorizedAccessException)
            {
                if (!IsDisposed && !Disposing)
                {
                    MessageBox.ShowWarning(this, "دسترسی مشاهدهٔ مشتریان تغییر کرده است. پنجره بسته می‌شود.", "دسترسی محدود است");
                    Close();
                }
            }
            catch (Exception)
            {
                if (!IsDisposed && !Disposing)
                    MessageBox.ShowError(this, "فهرست مشتریان از تصویر شعبه خوانده نشد. پس از بررسی اتصال دوباره تلاش کنید.", "جست‌وجوی مشتریان");
            }
            finally
            {
                bindingCustomers = false;
                customerLoadBusy = false;
                if (!IsDisposed && !Disposing)
                {
                    loadingCustomers.EndLoading();
                    btnSearch.Enabled = true;
                    gridCustomers.Enabled = true;
                    btnPrevious.Enabled = currentPage > 1;
                    btnNext.Enabled = currentPage < pageCount;
                }
            }
        }

        private async void CustomerSelectionChanged(object sender, EventArgs e)
        {
            if (bindingCustomers || !canViewFamily || gridCustomers.SelectedRows.Count == 0 || IsDisposed || Disposing) return;
            DataGridViewRow row = gridCustomers.SelectedRows[0];
            string customerName = Convert.ToString(row.Cells["colCustomerName"].Value, CultureInfo.InvariantCulture);
            object householdValue = row.Cells["colHouseholdId"].Value;
            string householdId = householdValue == null || householdValue == DBNull.Value
                ? null : Convert.ToString(householdValue, CultureInfo.InvariantCulture);
            int requestVersion = ++familyRequestVersion;
            if (String.IsNullOrWhiteSpace(householdId))
            {
                gridFamily.DataSource = null;
                gridCards.DataSource = null;
                lblFamilyCustomer.Text = customerName + "؛ در تصویر شعبه خانواده‌ای ثبت نشده است.";
                return;
            }
            await LoadFamilyAsync(customerName, householdId, requestVersion);
        }

        private async Task LoadFamilyAsync(string customerName, string householdId, int requestVersion)
        {
            loadingFamily.BeginLoading("در حال دریافت اعضا و کارت‌های خانواده…", global::FuntinoControlCenter.Properties.Resources.CardHistory);
            try
            {
                FamilyPageResult result = await Task.Run(delegate
                {
                    return new FamilyPageResult
                    {
                        Members = EdgeIntegrationRepository.GetHousehold(householdId),
                        Cards = EdgeIntegrationRepository.GetHouseholdCards(householdId)
                    };
                });
                if (IsDisposed || Disposing || requestVersion != familyRequestVersion) return;
                gridFamily.DataSource = result.Members;
                gridCards.DataSource = result.Cards;
                lblFamilyCustomer.Text = customerName + "؛ اطلاعات از تصویر محلی شعبه خوانده شد.";
            }
            catch (UnauthorizedAccessException)
            {
                if (!IsDisposed && !Disposing)
                {
                    MessageBox.ShowWarning(this, "مجوز مشاهدهٔ خانواده برای این حساب تغییر کرده است.", "دسترسی محدود است");
                    pnlFamily.Visible = false;
                    canViewFamily = false;
                }
            }
            catch (Exception)
            {
                if (!IsDisposed && !Disposing && requestVersion == familyRequestVersion)
                    MessageBox.ShowError(this, "اعضای خانواده و کارت‌های مجاز از تصویر شعبه خوانده نشدند.", "نمایش خانواده");
            }
            finally
            {
                if (!IsDisposed && !Disposing && requestVersion == familyRequestVersion) loadingFamily.EndLoading();
            }
        }

        private async void Search_Click(object sender, EventArgs e)
        {
            await LoadCustomersAsync(1);
        }

        private async void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            await LoadCustomersAsync(1);
        }

        private async void Previous_Click(object sender, EventArgs e)
        {
            if (currentPage > 1) await LoadCustomersAsync(currentPage - 1);
        }

        private async void Next_Click(object sender, EventArgs e)
        {
            if (currentPage < pageCount) await LoadCustomersAsync(currentPage + 1);
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
