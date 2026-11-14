using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Security;

namespace FuntinoControlCenter.Forms
{
    internal enum ManagementEditMode
    {
        Create,
        Update,
        Deactivate
    }

    public partial class BusinessManagementForm : Form
    {
        private const int PageSize = 20;
        private readonly BusinessManagementArea area;
        private DataTable allRows;
        private int currentPage = 1;
        private int filteredCount;

        internal BusinessManagementForm(BusinessManagementArea area)
        {
            this.area = area;
            InitializeComponent();
        }

        private void BusinessManagementForm_Load(object sender, EventArgs e)
        {
            try
            {
                BusinessManagementRepository.RequireAccess(area, "view");
                lblTitle.Text = AreaTitle(area);
                lblSubtitle.Text = AreaSubtitle(area);
                ConfigureColumns();
                ApplyPermissions();
                LoadRows();
                AuthorizationMonitor.Attach(this, delegate { return BusinessManagementRepository.HasAccess(area, "view"); });
            }
            catch (Exception ex)
            {
                AppMessageService.ShowError(this, DatabaseManager.SafeMessage(ex), "بازکردن بخش مدیریت");
                Close();
            }
        }

        private void ConfigureColumns()
        {
            colPercent.HeaderText = area == BusinessManagementArea.ControllerDevices ? "نوع قیمت" :
                area == BusinessManagementArea.DevicePromotions ? "درصد جشنواره" :
                area == BusinessManagementArea.DiscountPolicies ? "درصد تخفیف" : "درصد / سطح";
            colCode.Visible = false;
            colName.Visible = false;
            colAmount.Visible = false;
            colPercent.Visible = false;
            colStartsAt.Visible = false;
            colEndsAt.Visible = false;
            colWeekday.Visible = false;
            colMinimum.Visible = false;
            colMaximum.Visible = false;
            colActive.Visible = true;
            colDetail.Visible = true;
            cmbStatus.Visible = SupportsStatusFilter(area);

            switch (area)
            {
                case BusinessManagementArea.Attractions:
                case BusinessManagementArea.ControllerDevices:
                    colCode.Visible = colName.Visible = colAmount.Visible = true;
                    colPercent.Visible = area == BusinessManagementArea.ControllerDevices;
                    break;
                case BusinessManagementArea.Promotions:
                    colCode.Visible = colName.Visible = colPercent.Visible = colStartsAt.Visible = colEndsAt.Visible = true;
                    break;
                case BusinessManagementArea.DiscountPolicies:
                    colPercent.Visible = colWeekday.Visible = colMinimum.Visible = colMaximum.Visible = true;
                    break;
                case BusinessManagementArea.DiscountRates:
                    colCode.Visible = colName.Visible = colPercent.Visible = true;
                    colActive.Visible = false;
                    break;
                case BusinessManagementArea.Deposits:
                case BusinessManagementArea.RechargeLimit:
                case BusinessManagementArea.Tickets:
                    colAmount.Visible = true;
                    colActive.Visible = area == BusinessManagementArea.Tickets;
                    break;
                case BusinessManagementArea.HourlyPlans:
                    colName.Visible = colAmount.Visible = colActive.Visible = true;
                    break;
                case BusinessManagementArea.Schedules:
                    colStartsAt.Visible = true;
                    break;
                case BusinessManagementArea.DevicePromotions:
                    colCode.Visible = colName.Visible = colPercent.Visible = colActive.Visible = true;
                    break;
            }
        }

        private void ApplyPermissions()
        {
            bool canCreate = BusinessManagementRepository.HasAccess(area, "create") && SupportsCreate(area);
            bool canUpdate = BusinessManagementRepository.HasAccess(area, "update");
            bool canDelete = BusinessManagementRepository.HasAccess(area, "delete") && SupportsDeactivate(area);
            btnAdd.Visible = canCreate;
            btnEdit.Visible = canUpdate;
            btnArchive.Visible = canDelete;
            if (area == BusinessManagementArea.RechargeLimit)
            {
                btnAdd.Text = "تعریف سقف آغازین";
                btnEdit.Text = "ثبت سقف تازه";
            }
        }

        private void LoadRows()
        {
            allRows = BusinessManagementRepository.Load(area);
            if (area == BusinessManagementArea.RechargeLimit)
            {
                bool hasLimit = allRows.Rows.Count > 0;
                btnAdd.Visible = !hasLimit && BusinessManagementRepository.HasAccess(area, "create") && SupportsCreate(area);
                btnEdit.Visible = hasLimit && BusinessManagementRepository.HasAccess(area, "update");
            }
            currentPage = 1;
            ApplyFilterAndPage();
        }

        private void ApplyFilterAndPage()
        {
            if (allRows == null) return;
            string search = (txtSearch.Text ?? String.Empty).Trim();
            int statusFilter = Math.Max(0, cmbStatus.SelectedIndex);
            var matches = new List<DataRow>();
            foreach (DataRow row in allRows.Rows)
            {
                if (statusFilter != 0 && allRows.Columns.Contains("Active"))
                {
                    bool active = row["Active"] != DBNull.Value && Convert.ToBoolean(row["Active"], CultureInfo.InvariantCulture);
                    if ((statusFilter == 1 && !active) || (statusFilter == 2 && active)) continue;
                }

                if (search.Length > 0)
                {
                    bool contains = false;
                    foreach (DataColumn column in allRows.Columns)
                    {
                        object value = row[column];
                        if (value != DBNull.Value && Convert.ToString(value, CultureInfo.GetCultureInfo("fa-IR")).IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (!contains) continue;
                }
                matches.Add(row);
            }

            filteredCount = matches.Count;
            lblEmpty.Text = String.IsNullOrWhiteSpace(search) && statusFilter == 0
                ? "موردی برای نمایش وجود ندارد."
                : "با این جست‌وجو یا فیلتر، موردی پیدا نشد.";
            lblEmpty.Visible = filteredCount == 0;
            int pageCount = Math.Max(1, (int)Math.Ceiling((double)filteredCount / PageSize));
            currentPage = Math.Max(1, Math.Min(currentPage, pageCount));
            DataTable visibleRows = allRows.Clone();
            foreach (DataRow row in matches.Skip((currentPage - 1) * PageSize).Take(PageSize)) visibleRows.ImportRow(row);
            grid.DataSource = visibleRows;
            lblPage.Text = "صفحه‌ی " + PersianNumber(currentPage) + " از " + PersianNumber(pageCount);
            lblRowCount.Text = PersianNumber(filteredCount) + " مورد";
            btnPrevious.Enabled = currentPage > 1;
            btnNext.Enabled = currentPage < pageCount;
        }

        private void SearchChanged(object sender, EventArgs e)
        {
            currentPage = 1;
            ApplyFilterAndPage();
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            try { BusinessManagementRepository.RequireAccess(area, "view"); LoadRows(); }
            catch (Exception ex) { AppMessageService.ShowError(this, DatabaseManager.SafeMessage(ex), "تازه‌سازی اطلاعات"); }
        }

        private void Add_Click(object sender, EventArgs e)
        {
            OpenEditor(ManagementEditMode.Create, null);
        }

        private void Edit_Click(object sender, EventArgs e)
        {
            BusinessManagementRecord record = SelectedRecord();
            if (record == null)
            {
                AppMessageService.ShowWarning(this, "ابتدا یک ردیف را از فهرست انتخاب کنید.", "ردیفی انتخاب نشده است");
                return;
            }
            OpenEditor(ManagementEditMode.Update, record);
        }

        private void Archive_Click(object sender, EventArgs e)
        {
            BusinessManagementRecord record = SelectedRecord();
            if (record == null)
            {
                AppMessageService.ShowWarning(this, "ابتدا یک ردیف را از فهرست انتخاب کنید.", "ردیفی انتخاب نشده است");
                return;
            }
            OpenEditor(ManagementEditMode.Deactivate, record);
        }

        private void OpenEditor(ManagementEditMode mode, BusinessManagementRecord record)
        {
            try
            {
                string action = mode == ManagementEditMode.Create ? "create" : mode == ManagementEditMode.Update ? "update" : "delete";
                BusinessManagementRepository.RequireAccess(area, action);
                using (var form = new BusinessManagementEditForm(area, mode, record))
                {
                    if (form.ShowDialog(this) == DialogResult.OK) LoadRows();
                }
            }
            catch (Exception ex)
            {
                AppMessageService.ShowError(this, DatabaseManager.SafeMessage(ex), "ثبت تغییرات");
            }
        }

        private BusinessManagementRecord SelectedRecord()
        {
            if (grid.CurrentRow == null || grid.CurrentRow.DataBoundItem == null) return null;
            var view = grid.CurrentRow.DataBoundItem as DataRowView;
            if (view == null) return null;
            DataRow row = view.Row;
            return new BusinessManagementRecord
            {
                Id = IntValue(row, "RecordId"),
                Code = IntValue(row, "Code"),
                Name = StringValue(row, "Name"),
                Amount = LongValue(row, "Amount"),
                Percent = IntValue(row, "Percent"),
                StartsAt = DateValue(row, "StartsAt"),
                EndsAt = DateValue(row, "EndsAt"),
                Weekday = IntValue(row, "Weekday"),
                Minimum = LongValue(row, "Minimum"),
                Maximum = LongValue(row, "Maximum"),
                IsActive = BoolValue(row, "Active"),
                Extra1 = IntValue(row, "Extra1"),
                Extra2 = IntValue(row, "Extra2")
            };
        }

        private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && btnEdit.Visible) Edit_Click(sender, EventArgs.Empty);
        }

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Value == null || e.Value == DBNull.Value) return;
            string column = grid.Columns[e.ColumnIndex].DataPropertyName;
            if (column == "Percent" && area == BusinessManagementArea.ControllerDevices)
            {
                e.Value = Convert.ToInt32(e.Value, CultureInfo.InvariantCulture) == 0 ? "قیمت ثابت" : "قیمت متغیر";
                e.FormattingApplied = true;
            }
            else if (column == "Active")
            {
                e.Value = Convert.ToBoolean(e.Value, CultureInfo.InvariantCulture) ? "فعال" : "غیرفعال";
                e.FormattingApplied = true;
            }
            else if (column == "Weekday")
            {
                e.Value = WeekdayName(Convert.ToInt32(e.Value, CultureInfo.InvariantCulture));
                e.FormattingApplied = true;
            }
            else if (column == "StartsAt" || column == "EndsAt")
            {
                DateTime date;
                if (DateTime.TryParse(Convert.ToString(e.Value, CultureInfo.InvariantCulture), out date))
                {
                    e.Value = date.ToString("yyyy/MM/dd HH:mm", CultureInfo.GetCultureInfo("fa-IR"));
                    e.FormattingApplied = true;
                }
            }
            else if (column == "Amount" || column == "Minimum" || column == "Maximum" || column == "Code" || column == "Percent")
            {
                long number;
                if (Int64.TryParse(Convert.ToString(e.Value, CultureInfo.InvariantCulture), out number))
                {
                    string suffix = column == "Percent" ? "٪" : String.Empty;
                    e.Value = number.ToString("N0", CultureInfo.GetCultureInfo("fa-IR")) + suffix;
                    e.FormattingApplied = true;
                }
            }
        }

        private void Previous_Click(object sender, EventArgs e) { if (currentPage > 1) { currentPage--; ApplyFilterAndPage(); } }
        private void Next_Click(object sender, EventArgs e) { currentPage++; ApplyFilterAndPage(); }
        private void Back_Click(object sender, EventArgs e) { Close(); }

        private static bool SupportsCreate(BusinessManagementArea item)
        {
            return item == BusinessManagementArea.Attractions || item == BusinessManagementArea.ControllerDevices ||
                item == BusinessManagementArea.Promotions || item == BusinessManagementArea.DevicePromotions ||
                item == BusinessManagementArea.DiscountPolicies || item == BusinessManagementArea.RechargeLimit ||
                item == BusinessManagementArea.Schedules || item == BusinessManagementArea.Tickets;
        }

        private static bool SupportsDeactivate(BusinessManagementArea item)
        {
            return item == BusinessManagementArea.Attractions || item == BusinessManagementArea.ControllerDevices ||
                item == BusinessManagementArea.Promotions || item == BusinessManagementArea.DevicePromotions ||
                item == BusinessManagementArea.DiscountPolicies || item == BusinessManagementArea.Schedules ||
                item == BusinessManagementArea.Tickets;
        }

        private static bool SupportsStatusFilter(BusinessManagementArea item)
        {
            return item == BusinessManagementArea.Attractions || item == BusinessManagementArea.ControllerDevices ||
                item == BusinessManagementArea.DiscountPolicies || item == BusinessManagementArea.HourlyPlans ||
                item == BusinessManagementArea.Schedules || item == BusinessManagementArea.Tickets ||
                item == BusinessManagementArea.DevicePromotions;
        }

        private static string AreaTitle(BusinessManagementArea item)
        {
            switch (item)
            {
                case BusinessManagementArea.Attractions: return "مدیریت دستگاه‌های تفریحی";
                case BusinessManagementArea.ControllerDevices: return "مدیریت دستگاه‌های کنترل‌شونده";
                case BusinessManagementArea.Promotions: return "مدیریت جشنواره‌ها و تخفیف‌ها";
                case BusinessManagementArea.DevicePromotions: return "تخصیص جشنواره به دستگاه‌ها";
                case BusinessManagementArea.DiscountPolicies: return "قوانین تخفیف روزانه";
                case BusinessManagementArea.DiscountRates: return "درصدهای سطح تخفیف";
                case BusinessManagementArea.Deposits: return "مدیریت مبلغ بیعانه";
                case BusinessManagementArea.RechargeLimit: return "سقف هر بار شارژ";
                case BusinessManagementArea.HourlyPlans: return "تنظیم شارژهای ساعتی";
                case BusinessManagementArea.Schedules: return "زمان‌بندی شارژ";
                case BusinessManagementArea.Tickets: return "تنظیم مبلغ بلیت";
                default: return "مدیریت اطلاعات عملیاتی";
            }
        }

        private static string AreaSubtitle(BusinessManagementArea item)
        {
            if (item == BusinessManagementArea.Promotions) return "فهرست در هر دو پایگاه همگام می‌شود؛ پایان‌دادن به جشنواره، سوابق گذشته را نگه می‌دارد.";
            if (item == BusinessManagementArea.DiscountPolicies) return "قوانین فعال برای هر روز و بازه‌ی مبلغ بررسی می‌شوند؛ هم‌پوشانی تازه پذیرفته نمی‌شود.";
            if (item == BusinessManagementArea.Deposits) return "مبلغ در همه‌ی ردیف‌های تنظیمات برنامه ذخیره می‌شود؛ اختلاف مقدارهای فعلی در ستون توضیح دیده می‌شود.";
            if (item == BusinessManagementArea.DevicePromotions) return "برای هر دستگاه می‌توانید جشنواره را فعال یا غیرفعال کنید.";
            return "برای جست‌وجو، عبارت یا وضعیت را انتخاب کنید؛ تغییر هر ردیف در صفحه‌ی جدا انجام می‌شود.";
        }

        private static string WeekdayName(int value)
        {
            string[] names = { "شنبه", "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنجشنبه", "جمعه" };
            return value >= 1 && value <= names.Length ? names[value - 1] : "نامشخص";
        }

        private static string PersianNumber(int value) { return value.ToString("N0", CultureInfo.GetCultureInfo("fa-IR")); }
        private static int IntValue(DataRow row, string name) { return row.Table.Columns.Contains(name) && row[name] != DBNull.Value ? Convert.ToInt32(row[name], CultureInfo.InvariantCulture) : 0; }
        private static long LongValue(DataRow row, string name) { return row.Table.Columns.Contains(name) && row[name] != DBNull.Value ? Convert.ToInt64(row[name], CultureInfo.InvariantCulture) : 0L; }
        private static bool BoolValue(DataRow row, string name) { return row.Table.Columns.Contains(name) && row[name] != DBNull.Value && Convert.ToBoolean(row[name], CultureInfo.InvariantCulture); }
        private static string StringValue(DataRow row, string name) { return row.Table.Columns.Contains(name) && row[name] != DBNull.Value ? Convert.ToString(row[name], CultureInfo.InvariantCulture) : String.Empty; }
        private static DateTime DateValue(DataRow row, string name) { return row.Table.Columns.Contains(name) && row[name] != DBNull.Value ? Convert.ToDateTime(row[name], CultureInfo.InvariantCulture) : DateTime.MinValue; }
    }
}
