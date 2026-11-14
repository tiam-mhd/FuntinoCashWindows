using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using MessageBox = FuntinoControlCenter.Forms.AppMessageService;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Services;
using FuntinoControlCenter.Security;

namespace FuntinoControlCenter.Forms
{
    public partial class ReportsForm : Form
    {
        private int page = 1, pages = 1;
        private string tableName;
        private bool deviceDatabase;
        private const int PageSize = 100;
        private const int ExcelExportLimit = 50000;
        private const int PdfExportLimit = 5000;
        public ReportsForm() { InitializeComponent(); }

        private void Generate_Click(object sender, EventArgs e) { page = 1; LoadPage(); }
        private void Previous_Click(object sender, EventArgs e) { if (page > 1) { page--; LoadPage(); } }
        private void Next_Click(object sender, EventArgs e) { if (page < pages) { page++; LoadPage(); } }
        private void TableSelectionChanged(object sender, EventArgs e)
        {
            if (!loading && cmbTables.SelectedItem != null) { tableName = Convert.ToString(cmbTables.SelectedItem); page = 1; LoadPage(); }
        }
        private void Search_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; page = 1; LoadPage(); } }
        private void ExportExcel_Click(object sender, EventArgs e) { Export(false); }
        private void ExportPdf_Click(object sender, EventArgs e) { Export(true); }

        private bool loading;
        private void ReportsForm_Load(object sender, EventArgs e)
        {
            try
            {
                AuthorizationGuard.Require("reports.view");
                AuthorizationGuard.RequireAny(PermissionCatalog.DataSchemas.Select(schema => PermissionCatalog.TableCode(schema, "view")).ToArray());
                btnExcel.Visible = btnPdf.Visible = AuthorizationGuard.Has("reports.export");
                loading = true;
                cmbDatabase.Items.Clear();
                if (TableRepository.GetTableNames(false).Count > 0) cmbDatabase.Items.Add(new DatabaseChoice("پایگاه اصلی", false));
                if (TableRepository.GetTableNames(true).Count > 0) cmbDatabase.Items.Add(new DatabaseChoice("پایگاه دستگاه‌ها", true));
                if (cmbDatabase.Items.Count == 0) { loading = false; Close(); return; }
                cmbDatabase.SelectedIndex = 0;
                deviceDatabase = ((DatabaseChoice)cmbDatabase.SelectedItem).DeviceDatabase;
                loading = false; PopulateTables();
                AuthorizationMonitor.Attach(this, HasReportAccess);
            }
            catch (Exception ex) { loading = false; MessageBox.Show(DatabaseManager.SafeMessage(ex), "آماده‌سازی گزارش", MessageBoxButtons.OK, MessageBoxIcon.Error); Close(); }
        }

        private void DatabaseChanged(object sender, EventArgs e)
        {
            if (loading || cmbDatabase.SelectedIndex < 0) return;
            var choice = cmbDatabase.SelectedItem as DatabaseChoice;
            if (choice == null) return;
            deviceDatabase = choice.DeviceDatabase; tableName = null; page = 1; PopulateTables();
        }

        private bool HasReportAccess()
        {
            if (!AuthorizationGuard.Has("reports.view")) return false;
            if (String.IsNullOrWhiteSpace(tableName))
                return AuthorizationGuard.HasAny(PermissionCatalog.DataSchemas.Select(schema => PermissionCatalog.TableCode(schema, "view")).ToArray());
            return AuthorizationGuard.Has(PermissionCatalog.TableCode(tableName.Split('.')[0], "view"));
        }

        private void PopulateTables()
        {
            loading = true; cmbTables.Items.Clear(); foreach (string table in TableRepository.GetTableNames(deviceDatabase)) cmbTables.Items.Add(table); loading = false;
            if (cmbTables.Items.Count > 0) cmbTables.SelectedIndex = 0;
            else { tableName = null; grid.DataSource = null; lblSummary.Text = "جدولی برای گزارش در دسترس نیست."; }
        }

        private void LoadPage()
        {
            if (loading || String.IsNullOrWhiteSpace(tableName)) return;
            try
            {
                AuthorizationGuard.Require("reports.view");
                var result = TableRepository.GetPage(tableName, page, PageSize, txtSearch.Text, deviceDatabase); page = result.Page; pages = result.PageCount;
                grid.DataSource = result.Data; lblPage.Text = "صفحه " + page + " / " + pages;
                lblSummary.Text = "جدول: " + tableName + "     |     تعداد نتیجه: " + result.TotalRows.ToString("N0") + "     |     پایگاه: " + (deviceDatabase ? DatabaseManager.DeviceCatalog : DatabaseManager.MainCatalog);
                lblGenerated.Text = "به‌روزرسانی: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm"); btnPrevious.Enabled = page > 1; btnNext.Enabled = page < pages;
            }
            catch (Exception ex) { MessageBox.Show(DatabaseManager.SafeMessage(ex), "گزارش بارگذاری نشد", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void Export(bool pdf)
        {
            if (String.IsNullOrWhiteSpace(tableName)) return;
            try { AuthorizationGuard.Require("reports.export"); }
            catch (Exception ex) { MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "مجوز خروجی", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = pdf ? "گزارش PDF|*.pdf" : "فایل Excel|*.xlsx";
                dialog.FileName = "Funtino_" + tableName.Replace('.', '_') + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm");
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    int maximum = pdf ? PdfExportLimit : ExcelExportLimit;
                    bool truncated; DataTable data = TableRepository.GetReportData(tableName, txtSearch.Text, maximum, out truncated, deviceDatabase);
                    if (pdf) ReportExportService.ExportPdf(data, "گزارش فانتینو | " + tableName, dialog.FileName); else ReportExportService.ExportExcel(data, "Funtino Report", dialog.FileName);
                    MessageBox.Show("خروجی با " + data.Rows.Count.ToString("N0") + " ردیف ساخته شد." + (truncated ? " خروجی این قالب به " + maximum.ToString("N0") + " ردیف محدود شد؛ فیلتر را دقیق‌تر کنید." : ""), "گزارش آماده است", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex) { MessageBox.Show(DatabaseManager.SafeMessage(ex), "ساخت گزارش ناموفق", MessageBoxButtons.OK, MessageBoxIcon.Error); }
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
