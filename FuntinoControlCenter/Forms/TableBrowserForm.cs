using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Security;
using FuntinoControlCenter.Services;

namespace FuntinoControlCenter.Forms
{
    public partial class TableBrowserForm : Form
    {
        private DataTable currentData;
        private string currentTable;
        private int currentPage = 1;
        private bool loading;
        private bool currentTableHasPrimaryKey;
        private bool deviceDatabase;
        private int activeDatabaseIndex;
        private string initialTable;
        private readonly List<DatabaseChoice> databaseChoices = new List<DatabaseChoice>();
        private const int MaximumExcelRows = 50000;
        private const int MaximumPdfRows = 5000;

        public TableBrowserForm() : this(false, null) { }

        public TableBrowserForm(bool openDeviceDatabase, string openTable)
        {
            deviceDatabase = openDeviceDatabase; activeDatabaseIndex = openDeviceDatabase ? 1 : 0; initialTable = openTable;
            InitializeComponent();
        }

        private void Search_Click(object sender, EventArgs e) { if (ConfirmDiscard()) { currentPage = 1; LoadPage(false); } }
        private void Previous_Click(object sender, EventArgs e) { if (currentPage > 1 && ConfirmDiscard()) { currentPage--; LoadPage(false); } }
        private void Next_Click(object sender, EventArgs e) { if (currentPage < pageCount && ConfirmDiscard()) { currentPage++; LoadPage(false); } }
        private void Refresh_Click(object sender, EventArgs e) { LoadPage(true); }
        private void ExportExcel_Click(object sender, EventArgs e) { Export(false); }
        private void ExportPdf_Click(object sender, EventArgs e) { Export(true); }
        private void PageSize_ValueChanged(object sender, EventArgs e) { if (ConfirmDiscard()) { currentPage = 1; LoadPage(false); } }
        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var bytes = e.Value as byte[];
            if (bytes != null) { e.Value = "[داده‌ی باینری: " + bytes.Length.ToString("N0") + " بایت]"; e.FormattingApplied = true; }
        }
        private void Grid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
            MessageBox.Show("مقدار این خانه با نوع داده‌ی ستون سازگار نیست.", "اعتبارسنجی داده", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private int pageCount = 1;
        private void TableBrowserForm_Load(object sender, EventArgs e)
        {
            try
            {
                string[] tableViews = PermissionCatalog.DataSchemas.Select(schema => PermissionCatalog.TableCode(schema, "view")).ToArray();
                AuthorizationGuard.RequireAny(tableViews);
                loading = true;
                cmbDatabase.Items.Clear();
                if (TableRepository.GetTableNames(false).Count > 0) databaseChoices.Add(new DatabaseChoice("پایگاه اصلی", false));
                if (TableRepository.GetTableNames(true).Count > 0) databaseChoices.Add(new DatabaseChoice("پایگاه دستگاه‌ها", true));
                foreach (var choice in databaseChoices) cmbDatabase.Items.Add(choice);
                int selected = databaseChoices.FindIndex(choice => choice.DeviceDatabase == deviceDatabase);
                cmbDatabase.SelectedIndex = selected >= 0 ? selected : (databaseChoices.Count == 0 ? -1 : 0);
                if (cmbDatabase.SelectedIndex < 0) { Close(); return; }
                deviceDatabase = ((DatabaseChoice)cmbDatabase.SelectedItem).DeviceDatabase;
                loading = false;
                PopulateTables();
                AuthorizationMonitor.Attach(this, HasTableAccess);
            }
            catch (Exception ex) { loading = false; MessageBox.Show(DatabaseManager.SafeMessage(ex), "خواندن فهرست جدول‌ها", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void DatabaseChanged(object sender, EventArgs e)
        {
            if (loading || cmbDatabase.SelectedIndex < 0) return;
            if (!ConfirmDiscard()) { loading = true; cmbDatabase.SelectedIndex = activeDatabaseIndex; loading = false; return; }
            activeDatabaseIndex = cmbDatabase.SelectedIndex;
            var choice = cmbDatabase.SelectedItem as DatabaseChoice;
            if (choice == null) return;
            deviceDatabase = choice.DeviceDatabase; currentTable = null; currentPage = 1; PopulateTables();
        }

        private void PopulateTables()
        {
            try
            {
                loading = true; cmbTables.Items.Clear(); foreach (string name in TableRepository.GetTableNames(deviceDatabase)) cmbTables.Items.Add(name); loading = false;
                if (cmbTables.Items.Count > 0)
                {
                    int selected = String.IsNullOrWhiteSpace(initialTable) ? 0 : cmbTables.Items.IndexOf(initialTable);
                    cmbTables.SelectedIndex = selected >= 0 ? selected : 0; initialTable = null;
                }
                else { currentTable = null; currentData = null; grid.DataSource = null; lblHint.Text = "جدول قابل نمایش پیدا نشد."; UpdateOperationVisibility(); }
            }
            catch { loading = false; throw; }
        }

        private void TableChanged(object sender, EventArgs e)
        {
            if (loading || cmbTables.SelectedItem == null) return;
            if (!ConfirmDiscard())
            {
                loading = true; int index = cmbTables.Items.IndexOf(currentTable); if (index >= 0) cmbTables.SelectedIndex = index; loading = false; return;
            }
            currentTable = Convert.ToString(cmbTables.SelectedItem); currentPage = 1; LoadPage(false);
        }

        private void Search_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; if (ConfirmDiscard()) { currentPage = 1; LoadPage(false); } } }

        private bool ConfirmDiscard()
        {
            if (grid.IsCurrentCellInEditMode) grid.EndEdit();
            if (currentData == null || currentData.GetChanges() == null) return true;
            return MessageBox.Show("تغییرات ذخیره‌نشده کنار گذاشته شود؟", "تغییرات باز", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private void LoadPage(bool askDiscard)
        {
            if (askDiscard && !ConfirmDiscard()) return;
            if (String.IsNullOrEmpty(currentTable)) return;
            try
            {
                int size = Decimal.ToInt32(nudPageSize.Value);
                var page = TableRepository.GetPage(currentTable, currentPage, size, txtSearch.Text, deviceDatabase);
                currentData = page.Data; currentPage = Math.Min(Math.Max(1, currentPage), page.PageCount); pageCount = page.PageCount;
                grid.DataSource = currentData; ApplyColumnRules(); UpdateOperationVisibility();
                lblPage.Text = "صفحه " + currentPage + " از " + pageCount;
                lblHint.Text = (deviceDatabase ? DatabaseManager.DeviceCatalog : DatabaseManager.MainCatalog) + "  |  " + currentTable + "   |   " + page.TotalRows.ToString("N0") + " ردیف  |  حداکثر " + size + " ردیف در هر صفحه";
                btnPrevious.Enabled = currentPage > 1; btnNext.Enabled = currentPage < pageCount;
            }
            catch (Exception ex) { MessageBox.Show(DatabaseManager.SafeMessage(ex), "بارگذاری جدول ناموفق", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void ApplyColumnRules()
        {
            if (currentData == null) return;
            List<TableColumn> columns = TableRepository.GetColumns(currentTable, deviceDatabase);
            bool hasKey = columns.Any(c => c.PrimaryKey);
            currentTableHasPrimaryKey = hasKey;
            string schema = currentTable.Split('.')[0];
            bool canCreate = AuthorizationGuard.Has(PermissionCatalog.TableCode(schema, "create"));
            bool canUpdate = AuthorizationGuard.Has(PermissionCatalog.TableCode(schema, "update"));
            foreach (DataGridViewColumn gridColumn in grid.Columns)
            {
                TableColumn column = columns.FirstOrDefault(c => String.Equals(c.Name, gridColumn.DataPropertyName, StringComparison.OrdinalIgnoreCase));
                gridColumn.ReadOnly = column == null || column.Identity || column.Computed || column.PrimaryKey || column.IsBinary || column.TypeName == "timestamp" || column.TypeName == "rowversion";
            }
            foreach (DataGridViewRow gridRow in grid.Rows)
            {
                DataRowView view = gridRow.DataBoundItem as DataRowView;
                if (view != null)
                    gridRow.ReadOnly = view.Row.RowState == DataRowState.Added ? !canCreate : !canUpdate;
            }
            if (!hasKey && canUpdate) lblHint.Text += "  |  بدون کلید اصلی: ویرایش/حذف ردیف‌های موجود امن نیست.";
        }

        private void Grid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.RowIndex < 0 || String.IsNullOrWhiteSpace(currentTable)) return;
            DataRowView view = grid.Rows[e.RowIndex].DataBoundItem as DataRowView;
            bool isNew = view != null && view.Row.RowState == DataRowState.Added;
            string schema = currentTable.Split('.')[0];
            if (!AuthorizationGuard.Has(PermissionCatalog.TableCode(schema, isNew ? "create" : "update")))
                e.Cancel = true;
            if (!isNew && !currentTableHasPrimaryKey)
                e.Cancel = true;
        }

        private void UpdateOperationVisibility()
        {
            if (String.IsNullOrWhiteSpace(currentTable))
            {
                btnAdd.Visible = btnSave.Visible = btnDelete.Visible = btnExcel.Visible = btnPdf.Visible = false;
                return;
            }
            string schema = currentTable.Split('.')[0];
            btnAdd.Visible = AuthorizationGuard.Has(PermissionCatalog.TableCode(schema, "create"));
            btnDelete.Visible = currentTableHasPrimaryKey && AuthorizationGuard.Has(PermissionCatalog.TableCode(schema, "delete"));
            btnSave.Visible = btnAdd.Visible || AuthorizationGuard.Has(PermissionCatalog.TableCode(schema, "update"));
            btnExcel.Visible = btnPdf.Visible = AuthorizationGuard.Has("reports.export");
        }

        private bool HasTableAccess()
        {
            if (String.IsNullOrWhiteSpace(currentTable))
                return AuthorizationGuard.HasAny(PermissionCatalog.DataSchemas.Select(schema => PermissionCatalog.TableCode(schema, "view")).ToArray());
            return AuthorizationGuard.Has(PermissionCatalog.TableCode(currentTable.Split('.')[0], "view"));
        }

        private void AddRow(object sender, EventArgs e)
        {
            if (currentData == null || String.IsNullOrWhiteSpace(currentTable)) return;
            try
            {
                AuthorizationGuard.RequireTable(currentTable.Split('.')[0], "create");
                DataRow row = currentData.NewRow(); currentData.Rows.Add(row);
                var firstEditable = grid.Rows[grid.Rows.Count - 1].Cells.Cast<DataGridViewCell>().FirstOrDefault(c => !c.ReadOnly);
                if (firstEditable != null) { grid.CurrentCell = firstEditable; grid.BeginEdit(true); }
            }
            catch (Exception ex) { MessageBox.Show(DatabaseManager.SafeMessage(ex), "افزودن ردیف انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void SaveRows(object sender, EventArgs e)
        {
            if (currentData == null || String.IsNullOrWhiteSpace(currentTable)) return;
            try
            {
                grid.EndEdit(); var changed = currentData.GetChanges();
                if (changed == null) { MessageBox.Show("تغییری برای ذخیره وجود ندارد.", "ذخیره", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                int count = TableRepository.SaveChanges(currentTable, currentData, deviceDatabase);
                MessageBox.Show(count.ToString("N0") + " تغییر با موفقیت و در یک تراکنش ذخیره شد.", "ذخیره انجام شد", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadPage(false);
            }
            catch (Exception ex) { MessageBox.Show(DatabaseManager.SafeMessage(ex), "ذخیره انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void DeleteRow(object sender, EventArgs e)
        {
            if (grid.CurrentRow == null || currentData == null || String.IsNullOrWhiteSpace(currentTable)) return;
            try
            {
                AuthorizationGuard.RequireTable(currentTable.Split('.')[0], "delete");
                grid.EndEdit();
                string warning = currentData.GetChanges() == null ? "ردیف انتخاب‌شده حذف شود؟ این عملیات پس از ذخیره قابل بازگشت نیست." : "ردیف انتخاب‌شده حذف شود؟ سایر تغییرات بازشده نیز در همان تراکنش ذخیره خواهند شد.";
                if (MessageBox.Show(warning, "تأیید حذف", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                DataRowView view = grid.CurrentRow.DataBoundItem as DataRowView; if (view == null) return;
                if (view.Row.RowState == DataRowState.Added) view.Row.Delete(); else view.Row.Delete();
                TableRepository.SaveChanges(currentTable, currentData, deviceDatabase); LoadPage(false);
            }
            catch (Exception ex) { MessageBox.Show(DatabaseManager.SafeMessage(ex), "حذف انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void Export(bool pdf)
        {
            if (String.IsNullOrEmpty(currentTable)) return;
            AuthorizationGuard.Require("reports.export");
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = pdf ? "گزارش PDF|*.pdf" : "فایل Excel|*.xlsx";
                dialog.FileName = currentTable.Replace('.', '_') + "_report_" + DateTime.Now.ToString("yyyyMMdd_HHmm");
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    int maximum = pdf ? MaximumPdfRows : MaximumExcelRows;
                    bool truncated; DataTable report = TableRepository.GetReportData(currentTable, txtSearch.Text, maximum, out truncated, deviceDatabase);
                    if (pdf) ReportExportService.ExportPdf(report, "گزارش " + currentTable, dialog.FileName); else ReportExportService.ExportExcel(report, currentTable.Replace('.', '_'), dialog.FileName);
                    string note = "گزارش با " + report.Rows.Count.ToString("N0") + " ردیف ذخیره شد." + (truncated ? " خروجی برای این قالب تا " + maximum.ToString("N0") + " ردیف محدود شد؛ فیلتر دقیق‌تر انتخاب کنید." : "");
                    MessageBox.Show(note, "خروجی آماده است", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex) { MessageBox.Show(DatabaseManager.SafeMessage(ex), "ساخت خروجی ناموفق", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private sealed class DatabaseChoice
        {
            public string Caption { get; private set; }
            public bool DeviceDatabase { get; private set; }
            public DatabaseChoice(string caption, bool deviceDatabase) { Caption = caption; DeviceDatabase = deviceDatabase; }
            public override string ToString() { return Caption; }
        }
    }
}
