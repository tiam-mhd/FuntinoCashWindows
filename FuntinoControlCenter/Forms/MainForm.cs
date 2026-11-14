using System;
using System.Linq;
using System.Windows.Forms;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Security;
using FuntinoControlCenter.Services;

namespace FuntinoControlCenter.Forms
{
    public partial class MainForm : Form
    {
        public MainForm() { InitializeComponent(); }

        private void OpenTables_Click(object sender, EventArgs e) { using (var form = new TableBrowserForm()) form.ShowDialog(this); RefreshDashboard(); }
        private void OpenReports_Click(object sender, EventArgs e) { using (var form = new ReportsForm()) form.ShowDialog(this); }
        private void OpenDevices_Click(object sender, EventArgs e) { using (var form = new DeviceStatusForm()) form.ShowDialog(this); RefreshDashboard(); }
        private void OpenSettings_Click(object sender, EventArgs e) { using (var form = new SettingsForm()) form.ShowDialog(this); RefreshDashboard(); }
        private void OpenPassword_Click(object sender, EventArgs e) { using (var form = new ChangePasswordForm()) form.ShowDialog(this); }
        private void OpenRecharge_Click(object sender, EventArgs e) { OpenTable("finance.RechargeTransaction"); }
        private void OpenTickets_Click(object sender, EventArgs e) { OpenTable("ticket.TicketTransaction"); }
        private void OpenPos_Click(object sender, EventArgs e) { OpenTable("payments.PointOfSaleTransaction"); }
        private void Logout_Click(object sender, EventArgs e) { Close(); }

        private void OpenAccess_Click(object sender, EventArgs e)
        {
            try { using (var form = new AccessManagementForm()) form.ShowDialog(this); ApplyPermissions(); RefreshDashboard(); }
            catch (Exception ex) { MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "مدیریت کاربران و دسترسی‌ها", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void AuthorizationTimer_Tick(object sender, EventArgs e)
        {
            if (AppSession.Current == null) { Close(); return; }
            try { ApplyPermissions(); }
            catch (Exception ex)
            {
                MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "بررسی دسترسی", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                AppSession.Clear();
                Close();
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e) { authorizationTimer.Stop(); }

        private void MainForm_Load(object sender, EventArgs e)
        {
            lblUser.Text = AppSession.Current == null ? "کاربر" : "کاربر واردشده: " + AppSession.Current.UserName;
            try
            {
                ApplyPermissions();
                authorizationTimer.Start();
                RefreshDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "بررسی دسترسی", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
            }
        }

        private void ApplyPermissions()
        {
            bool canDashboard = AuthorizationGuard.Has("dashboard.view");
            string[] tableViews = PermissionCatalog.DataSchemas.Select(schema => PermissionCatalog.TableCode(schema, "view")).ToArray();
            btnTables.Visible = AuthorizationGuard.HasAny(tableViews);
            btnReports.Visible = AuthorizationGuard.Has("reports.view") && AuthorizationGuard.HasAny(tableViews);
            btnDevices.Visible = AuthorizationGuard.HasAny("devices.view", "devices.test", "devices.events.view");
            btnSettings.Visible = AuthorizationGuard.HasAny("settings.database.view", "settings.database.update", "settings.database.test", "settings.hardware.view", "settings.hardware.update");
            btnAccess.Visible = AuthorizationGuard.HasAny("users.view", "users.create", "users.update", "users.reset_password", "users.assign_roles", "users.deactivate", "roles.view", "roles.create", "roles.update", "roles.assign_permissions", "roles.delete");
            btnPassword.Visible = AppSession.Current != null;

            pnlCardTables.Visible = canDashboard;
            pnlCardMain.Visible = canDashboard;
            pnlCardDevice.Visible = canDashboard;
            pnlCardPorts.Visible = canDashboard;
            lblSummaryCaption.Visible = canDashboard;
            lblBusinessSummary.Visible = canDashboard;

            btnRecharge.Visible = AuthorizationGuard.Has(PermissionCatalog.TableCode("finance", "view"));
            btnTickets.Visible = AuthorizationGuard.Has(PermissionCatalog.TableCode("ticket", "view"));
            btnPos.Visible = AuthorizationGuard.Has(PermissionCatalog.TableCode("payments", "view"));
            lblQuickActions.Visible = btnRecharge.Visible || btnTickets.Visible || btnPos.Visible;
        }

        private void RefreshDashboard()
        {
            if (!AuthorizationGuard.Has("dashboard.view")) return;
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
                if (AppSession.Current == null) { Close(); return; }
                lblMainStatus.Text = "اتصال پایگاه بررسی نشد"; lblBusinessSummary.Text = DatabaseManager.SafeMessage(ex);
            }
        }

        private void OpenTable(string tableName)
        {
            string schema = (tableName ?? "").Split('.').FirstOrDefault() ?? "";
            if (!AuthorizationGuard.Has(PermissionCatalog.TableCode(schema, "view"))) return;
            using (var form = new TableBrowserForm(false, tableName)) form.ShowDialog(this);
            RefreshDashboard();
        }
    }
}
