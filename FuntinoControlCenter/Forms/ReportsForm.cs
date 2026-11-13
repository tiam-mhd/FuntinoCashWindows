using System;
using System.Data;
using System.Windows.Forms;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Services;

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
        public ReportsForm()
        {
            InitializeComponent(); Load += ReportsForm_Load;
            btnGenerate.Click += delegate { page = 1; LoadPage(); };
            btnPrevious.Click += delegate { if (page > 1) { page--; LoadPage(); } };
            btnNext.Click += delegate { if (page < pages) { page++; LoadPage(); } };
            cmbTables.SelectedIndexChanged += delegate { if (!loading && cmbTables.SelectedItem != null) { tableName = Convert.ToString(cmbTables.SelectedItem); page = 1; LoadPage(); } };
            cmbDatabase.SelectedIndexChanged += DatabaseChanged;
            txtSearch.KeyDown += delegate(object s, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; page = 1; LoadPage(); } };
            btnExcel.Click += delegate { Export(false); }; btnPdf.Click += delegate { Export(true); };
        }

        private bool loading;
        private void ReportsForm_Load(object sender, EventArgs e)
        {
            try
            {
                loading = true; cmbDatabase.Items.Add("پایگاه اصلی"); cmbDatabase.Items.Add("پایگاه دستگاه‌ها"); cmbDatabase.SelectedIndex = 0; loading = false; PopulateTables();
            }
            catch (Exception ex) { loading = false; MessageBox.Show(DatabaseManager.SafeMessage(ex), "آماده‌سازی گزارش", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void DatabaseChanged(object sender, EventArgs e)
        {
            if (loading || cmbDatabase.SelectedIndex < 0) return;
            deviceDatabase = cmbDatabase.SelectedIndex == 1; tableName = null; page = 1; PopulateTables();
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
    }
}
