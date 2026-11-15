using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Properties;
using FuntinoControlCenter.Security;
using FuntinoControlCenter.Services;
using MessageBox = FuntinoControlCenter.Forms.AppMessageService;

namespace FuntinoControlCenter.Forms
{
    public partial class ReportsForm : Form
    {
        private const int PageSize = 100;
        private const int ExcelExportLimit = 50000;
        private const int PdfExportLimit = 5000;

        private int page = 1;
        private int pages = 1;
        private string tableName;
        private string tableCaption;
        private string databaseCaption;
        private bool deviceDatabase;
        private bool suppressSelectionEvents;
        private bool operationBusy;
        private bool canExport;

        public ReportsForm()
        {
            InitializeComponent();
        }

        private async void ReportsForm_Load(object sender, EventArgs e)
        {
            try
            {
                AuthorizationGuard.Require("reports.view");
                AuthorizationGuard.RequireAny(PermissionCatalog.DataSchemas
                    .Select(schema => PermissionCatalog.TableCode(schema, "view")).ToArray());

                canExport = AuthorizationGuard.Has("reports.export");
                btnExcel.Visible = canExport;
                btnPdf.Visible = canExport;
                flpExports.Visible = canExport;

                await LoadAvailableDatabasesAsync();
                if (IsFormUnavailable()) return;
                AuthorizationMonitor.Attach(this, HasReportAccess);
            }
            catch (UnauthorizedAccessException)
            {
                if (IsFormUnavailable()) return;
                MessageBox.ShowWarning(this, "دسترسی مشاهدهٔ گزارش‌ها برای حساب شما فعال نیست.", "دسترسی به گزارش‌ها");
                Close();
            }
            catch (Exception)
            {
                if (IsFormUnavailable()) return;
                lblEmptyState.Text = "گزارش‌ها بارگذاری نشدند. ارتباط پایگاه داده را بررسی کنید و دوباره تلاش کنید.";
                lblEmptyState.Visible = true;
                ShowDataError("گزارش‌ها بارگذاری نشدند.");
            }
        }

        private async Task LoadAvailableDatabasesAsync()
        {
            SetOperationBusy(true, "در حال آماده‌سازی گزارش‌ها…");
            try
            {
                DatabaseAvailability available = await Task.Run(() => new DatabaseAvailability
                {
                    Main = TableRepository.GetTableNames(false).Count > 0,
                    Devices = TableRepository.GetTableNames(true).Count > 0
                });
                if (IsFormUnavailable()) return;

                suppressSelectionEvents = true;
                cmbDatabase.Items.Clear();
                if (available.Main) cmbDatabase.Items.Add(new DatabaseChoice("پایگاه اصلی", false));
                if (available.Devices) cmbDatabase.Items.Add(new DatabaseChoice("پایگاه دستگاه‌ها", true));

                if (cmbDatabase.Items.Count == 0)
                {
                    databaseCaption = null;
                    tableName = null;
                    tableCaption = null;
                    lblSummary.Text = "جدولی برای گزارش در دسترس نیست.";
                    lblEmptyState.Text = "در پایگاه‌های در دسترس شما، جدولی برای گزارش پیدا نشد.";
                    lblEmptyState.Visible = true;
                    grid.Visible = false;
                    return;
                }

                cmbDatabase.SelectedIndex = 0;
                DatabaseChoice choice = (DatabaseChoice)cmbDatabase.SelectedItem;
                deviceDatabase = choice.DeviceDatabase;
                databaseCaption = choice.Caption;
                await PopulateTablesWithinOperationAsync();
            }
            finally
            {
                suppressSelectionEvents = false;
                SetOperationBusy(false, null);
            }
        }

        private async void DatabaseChanged(object sender, EventArgs e)
        {
            if (suppressSelectionEvents || operationBusy || cmbDatabase.SelectedIndex < 0) return;

            DatabaseChoice choice = cmbDatabase.SelectedItem as DatabaseChoice;
            if (choice == null) return;

            deviceDatabase = choice.DeviceDatabase;
            databaseCaption = choice.Caption;
            tableName = null;
            tableCaption = null;
            page = 1;
            await PopulateTablesAsync();
        }

        private async void TableSelectionChanged(object sender, EventArgs e)
        {
            if (suppressSelectionEvents || operationBusy || cmbTables.SelectedItem == null) return;

            TableChoice choice = cmbTables.SelectedItem as TableChoice;
            if (choice == null) return;

            tableName = choice.FullName;
            tableCaption = choice.Caption;
            page = 1;
            await LoadPageAsync(1);
        }

        private async void Generate_Click(object sender, EventArgs e)
        {
            page = 1;
            await LoadPageAsync(page);
        }

        private async void Previous_Click(object sender, EventArgs e)
        {
            if (page > 1) await LoadPageAsync(page - 1);
        }

        private async void Next_Click(object sender, EventArgs e)
        {
            if (page < pages) await LoadPageAsync(page + 1);
        }

        private async void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            page = 1;
            await LoadPageAsync(page);
        }

        private async void ExportExcel_Click(object sender, EventArgs e)
        {
            await ExportAsync(false);
        }

        private async void ExportPdf_Click(object sender, EventArgs e)
        {
            await ExportAsync(true);
        }

        private void Back_Click(object sender, EventArgs e)
        {
            Close();
        }

        private bool HasReportAccess()
        {
            canExport = AuthorizationGuard.Has("reports.export");
            btnExcel.Visible = canExport;
            btnPdf.Visible = canExport;
            flpExports.Visible = canExport;

            if (!AuthorizationGuard.Has("reports.view")) return false;
            if (String.IsNullOrWhiteSpace(tableName))
            {
                return AuthorizationGuard.HasAny(PermissionCatalog.DataSchemas
                    .Select(schema => PermissionCatalog.TableCode(schema, "view")).ToArray());
            }

            return AuthorizationGuard.Has(PermissionCatalog.TableCode(tableName.Split('.')[0], "view"));
        }

        private async Task PopulateTablesAsync()
        {
            SetOperationBusy(true, "در حال آماده‌سازی فهرست گزارش‌ها…");
            try
            {
                await PopulateTablesWithinOperationAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                if (IsFormUnavailable()) return;
                ShowPermissionError(ex);
            }
            catch (Exception)
            {
                if (IsFormUnavailable()) return;
                lblEmptyState.Text = "فهرست گزارش‌ها بارگذاری نشد. ارتباط پایگاه داده را بررسی کنید.";
                lblEmptyState.Visible = true;
                grid.Visible = false;
                ShowDataError("فهرست گزارش‌ها بارگذاری نشد.");
            }
            finally
            {
                SetOperationBusy(false, null);
            }
        }

        private async Task PopulateTablesWithinOperationAsync()
        {
            var tables = await Task.Run(() => TableRepository.GetTableNames(deviceDatabase));
            if (IsFormUnavailable()) return;

            suppressSelectionEvents = true;
            cmbTables.Items.Clear();
            foreach (string fullName in tables)
            {
                cmbTables.Items.Add(new TableChoice(fullName));
            }

            if (cmbTables.Items.Count == 0)
            {
                tableName = null;
                tableCaption = null;
                grid.DataSource = null;
                grid.Visible = false;
                lblSummary.Text = "در این پایگاه، جدولی برای گزارش شما وجود ندارد.";
                lblGenerated.Text = String.Empty;
                lblPage.Text = "صفحهٔ ۰ از ۰";
                lblEmptyState.Text = "برای این پایگاه، داده‌ای برای نمایش گزارش در دسترس نیست.";
                lblEmptyState.Visible = true;
                return;
            }

            cmbTables.SelectedIndex = 0;
            TableChoice first = (TableChoice)cmbTables.SelectedItem;
            tableName = first.FullName;
            tableCaption = first.Caption;
            page = 1;
            await LoadPageWithinOperationAsync(1);
        }

        private async Task LoadPageAsync(int requestedPage)
        {
            if (operationBusy || String.IsNullOrWhiteSpace(tableName)) return;

            SetOperationBusy(true, "در حال دریافت اطلاعات گزارش…");
            try
            {
                AuthorizationGuard.Require("reports.view");
                await LoadPageWithinOperationAsync(requestedPage);
            }
            catch (UnauthorizedAccessException ex)
            {
                if (IsFormUnavailable()) return;
                ShowPermissionError(ex);
            }
            catch (Exception)
            {
                if (IsFormUnavailable()) return;
                ShowDataError("اطلاعات گزارش دریافت نشد.");
            }
            finally
            {
                SetOperationBusy(false, null);
            }
        }

        private async Task LoadPageWithinOperationAsync(int requestedPage)
        {
            string selectedTable = tableName;
            bool selectedDeviceDatabase = deviceDatabase;
            string search = txtSearch.Text;
            TablePage result = await Task.Run(() => TableRepository.GetPage(
                selectedTable, requestedPage, PageSize, search, selectedDeviceDatabase));
            if (IsFormUnavailable()) return;

            page = result.Page;
            pages = result.PageCount;
            grid.DataSource = result.Data;
            bool hasRows = result.Data != null && result.Data.Rows.Count > 0;
            grid.Visible = hasRows;
            lblEmptyState.Text = hasRows
                ? String.Empty
                : (String.IsNullOrWhiteSpace(search)
                    ? "در این جدول، ردیفی برای نمایش وجود ندارد."
                    : "با این عبارت، نتیجه‌ای پیدا نشد. عبارت دیگری را جست‌وجو کنید.");
            lblEmptyState.Visible = !hasRows;
            lblPage.Text = "صفحهٔ " + page.ToString("N0") + " از " + pages.ToString("N0");
            lblSummary.Text = tableCaption + "  |  " + result.TotalRows.ToString("N0") + " ردیف";
            lblGenerated.Text = "پایگاه: " + (databaseCaption ?? "—") + "  •  آخرین به‌روزرسانی: " +
                DateTime.Now.ToString("yyyy/MM/dd  HH:mm", CultureInfo.InvariantCulture);
        }

        private async Task ExportAsync(bool pdf)
        {
            if (operationBusy || String.IsNullOrWhiteSpace(tableName)) return;

            try
            {
                AuthorizationGuard.Require("reports.export");
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowPermissionError(ex);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = pdf
                    ? "گزارش قابل‌چاپ (*.pdf)|*.pdf"
                    : "صفحه‌گسترده (*.xlsx)|*.xlsx";
                dialog.DefaultExt = pdf ? "pdf" : "xlsx";
                dialog.AddExtension = true;
                dialog.FileName = CreateExportFileName();
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                string selectedTable = tableName;
                string selectedCaption = tableCaption;
                string search = txtSearch.Text;
                bool selectedDeviceDatabase = deviceDatabase;
                string path = dialog.FileName;
                int maximum = pdf ? PdfExportLimit : ExcelExportLimit;

                SetOperationBusy(true, pdf
                    ? "در حال ساخت گزارش قابل‌چاپ…"
                    : "در حال آماده‌سازی صفحه‌گسترده…");
                try
                {
                    ExportResult result = await Task.Run(() =>
                    {
                        bool truncated;
                        DataTable data = TableRepository.GetReportData(
                            selectedTable, search, maximum, out truncated, selectedDeviceDatabase);

                        if (pdf)
                        {
                            ReportExportService.ExportPdf(data, "گزارش فانتینو | " + selectedCaption, path);
                        }
                        else
                        {
                            ReportExportService.ExportExcel(data, "گزارش فانتینو", path);
                        }

                        return new ExportResult(data.Rows.Count, truncated);
                    });
                    if (IsFormUnavailable()) return;

                    string message = "گزارش با " + result.RowCount.ToString("N0") + " ردیف ذخیره شد.";
                    if (result.Truncated)
                    {
                        message += " برای کوتاه‌تر شدن زمان آماده‌سازی، این خروجی حداکثر " +
                            maximum.ToString("N0") + " ردیف دارد؛ عبارت جست‌وجو را دقیق‌تر کنید.";
                    }
                    MessageBox.Show(this, message, "گزارش آماده است", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (UnauthorizedAccessException ex)
                {
                    if (IsFormUnavailable()) return;
                    ShowPermissionError(ex);
                }
                catch (Exception)
                {
                    if (IsFormUnavailable()) return;
                    MessageBox.ShowError(this, "ذخیرهٔ گزارش انجام نشد. فضای ذخیره‌سازی و دسترسی پوشه را بررسی کنید.", "ذخیرهٔ گزارش ناموفق بود");
                }
                finally
                {
                    SetOperationBusy(false, null);
                }
            }
        }

        private string CreateExportFileName()
        {
            string raw = "گزارش_" + (tableName ?? "داده‌ها").Replace('.', '_') + "_" +
                DateTime.Now.ToString("yyyyMMdd_HHmm", CultureInfo.InvariantCulture);
            foreach (char invalid in Path.GetInvalidFileNameChars()) raw = raw.Replace(invalid, '_');
            return raw;
        }

        private void SetOperationBusy(bool busy, string message)
        {
            if (IsDisposed || Disposing) return;

            operationBusy = busy;
            if (busy)
            {
                lblGenerated.Visible = false;
                loadingReports.BeginLoading(message, ReportsVisualAssets.Report);
            }
            else
            {
                loadingReports.EndLoading();
                lblGenerated.Visible = true;
            }

            cmbDatabase.Enabled = !busy && cmbDatabase.Items.Count > 0;
            cmbTables.Enabled = !busy && cmbTables.Items.Count > 0;
            txtSearch.Enabled = !busy && !String.IsNullOrWhiteSpace(tableName);
            btnGenerate.Enabled = !busy && !String.IsNullOrWhiteSpace(tableName);
            btnExcel.Enabled = !busy && canExport && !String.IsNullOrWhiteSpace(tableName);
            btnPdf.Enabled = !busy && canExport && !String.IsNullOrWhiteSpace(tableName);
            btnPrevious.Enabled = !busy && page > 1;
            btnNext.Enabled = !busy && page < pages;
        }

        private bool IsFormUnavailable()
        {
            return IsDisposed || Disposing;
        }

        private void ShowPermissionError(Exception ex)
        {
            string message = String.IsNullOrWhiteSpace(ex.Message)
                ? "دسترسی لازم برای این کار در حساب شما فعال نیست."
                : ex.Message;
            MessageBox.ShowWarning(this, message, "دسترسی به گزارش‌ها");
        }

        private void ShowDataError(string message)
        {
            MessageBox.ShowError(this, message + " ارتباط پایگاه داده را بررسی کنید و دوباره تلاش کنید.", "گزارش در دسترس نیست");
        }

        private sealed class DatabaseAvailability
        {
            public bool Main;
            public bool Devices;
        }

        private sealed class DatabaseChoice
        {
            public string Caption { get; private set; }
            public bool DeviceDatabase { get; private set; }

            public DatabaseChoice(string caption, bool deviceDatabase)
            {
                Caption = caption;
                DeviceDatabase = deviceDatabase;
            }

            public override string ToString()
            {
                return Caption;
            }
        }

        private sealed class TableChoice
        {
            public string FullName { get; private set; }
            public string Caption { get; private set; }

            public TableChoice(string fullName)
            {
                FullName = fullName;
                string[] parts = (fullName ?? String.Empty).Split(new[] { '.' }, 2);
                Caption = parts.Length == 2
                    ? PermissionCatalog.SchemaCaption(parts[0]) + "  |  " + parts[1]
                    : fullName;
            }

            public override string ToString()
            {
                return Caption;
            }
        }

        private sealed class ExportResult
        {
            public int RowCount { get; private set; }
            public bool Truncated { get; private set; }

            public ExportResult(int rowCount, bool truncated)
            {
                RowCount = rowCount;
                Truncated = truncated;
            }
        }
    }
}
