using System;
using System.Windows.Forms;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Services;

namespace FuntinoControlCenter.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent(); Load += MainForm_Load;
            btnTables.Click += delegate { using (var form = new TableBrowserForm()) form.ShowDialog(this); RefreshDashboard(); };
            btnReports.Click += delegate { using (var form = new ReportsForm()) form.ShowDialog(this); };
            btnDevices.Click += delegate { using (var form = new DeviceStatusForm()) form.ShowDialog(this); RefreshDashboard(); };
            btnSettings.Click += delegate { using (var form = new SettingsForm()) form.ShowDialog(this); RefreshDashboard(); };
            btnPassword.Click += delegate { using (var form = new ChangePasswordForm()) form.ShowDialog(this); };
            btnRecharge.Click += delegate { OpenTable("finance.RechargeTransaction"); }; btnTickets.Click += delegate { OpenTable("ticket.TicketTransaction"); }; btnPos.Click += delegate { OpenTable("payments.PointOfSaleTransaction"); };
            btnLogout.Click += delegate { Close(); };
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            lblUser.Text = AppSession.Current == null ? "کاربر" : "کاربر واردشده: " + AppSession.Current.UserName;
            bool admin = AppSession.Current != null && AppSession.Current.IsAdministrator;
            btnTables.Enabled = btnRecharge.Enabled = btnTickets.Enabled = btnPos.Enabled = admin;
            RefreshDashboard();
        }

        private void RefreshDashboard()
        {
            try
            {
                lblTableCountValue.Text = DeviceDiagnostics.CountBusinessTables().ToString("N0");
                lblTableCountCaption.Text = "جدول‌های کسب‌وکار در " + DatabaseManager.MainCatalog;
                lblMainStatus.Text = "پایگاه اصلی آماده و قابل دسترسی است";
                lblDeviceStatus.Text = "پایگاه دستگاه‌ها: " + DatabaseManager.DeviceCatalog;
                lblBusinessSummary.Text = DeviceDiagnostics.GetMainSummary();
                lblPorts.Text = "کارت‌خوان: " + AppSettings.Current.ReaderPort + "   |   POS: " + AppSettings.Current.PosPort;
            }
            catch (Exception ex)
            {
                lblMainStatus.Text = "اتصال پایگاه بررسی نشد"; lblBusinessSummary.Text = DatabaseManager.SafeMessage(ex);
            }
        }

        private void OpenTable(string tableName)
        {
            if (AppSession.Current == null || !AppSession.Current.IsAdministrator) return;
            using (var form = new TableBrowserForm(false, tableName)) form.ShowDialog(this);
            RefreshDashboard();
        }
    }
}
