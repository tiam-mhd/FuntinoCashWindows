using System;
using System.Collections.Generic;
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
        private void OpenCardReader_Click(object sender, EventArgs e) { OpenDevices_Click(sender, e); }
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

            pnlCardHero.Visible = canDashboard;
            lblBalanceTitle.Visible = lblBalanceValue.Visible = lblBalanceHint.Visible = canDashboard;
            pnlCardTables.Visible = canDashboard;
            pnlCardMain.Visible = canDashboard;
            pnlCardDevice.Visible = canDashboard;
            pnlCardPorts.Visible = canDashboard;
            lblSummaryCaption.Visible = canDashboard;
            lblBusinessSummary.Visible = canDashboard;

            btnRecharge.Visible = AuthorizationGuard.Has(PermissionCatalog.TableCode("finance", "view"));
            btnTickets.Visible = AuthorizationGuard.Has(PermissionCatalog.TableCode("ticket", "view"));
            btnPos.Visible = AuthorizationGuard.Has(PermissionCatalog.TableCode("payments", "view"));
            btnCardReader.Visible = AuthorizationGuard.HasAny("devices.view", "devices.test");
            lblQuickActions.Visible = btnRecharge.Visible || btnTickets.Visible || btnPos.Visible || btnCardReader.Visible;
        }

        private void RefreshDashboard()
        {
            if (!AuthorizationGuard.Has("dashboard.view")) return;
            try
            {
                bool canViewRecharge = AuthorizationGuard.Has(PermissionCatalog.TableCode("finance", "view"));
                lblTableCountValue.Text = canViewRecharge ? DeviceDiagnostics.CountRows("finance.RechargeTransaction").ToString("N0") : "—";
                lblTableCountCaption.Text = canViewRecharge ? "تراکنش شارژ ثبت‌شده" : "دسترسی مشاهده‌ی سوابق شارژ ندارید";
                lblMainStatus.Text = "پایگاه اصلی آماده است";
                lblDeviceStatus.Text = "پایگاه دستگاه‌ها آماده است";
                lblPorts.Text = "درگاه انتخاب‌شده ـ کارت‌خوان: " + DisplayPort(AppSettings.Current.ReaderPort) + "   |   پایانه: " + DisplayPort(AppSettings.Current.PosPort);

                var summaries = new List<string>();
                AddSummaryIfPermitted(summaries, "finance", "finance.RechargeTransaction", "سوابق شارژ");
                AddSummaryIfPermitted(summaries, "ticket", "ticket.TicketTransaction", "سوابق بلیت");
                AddSummaryIfPermitted(summaries, "payments", "payments.PointOfSaleTransaction", "تراکنش‌های پایانه");
                lblBusinessSummary.Text = summaries.Count == 0
                    ? "از بخش‌های مجاز در فهرست کناری برای مشاهده‌ی سوابق استفاده کنید."
                    : String.Join("     •     ", summaries);
            }
            catch (Exception ex)
            {
                if (AppSession.Current == null) { Close(); return; }
                lblMainStatus.Text = "اتصال پایگاه بررسی نشد"; lblBusinessSummary.Text = DatabaseManager.SafeMessage(ex);
            }
        }

        private static string DisplayPort(string port)
        {
            return String.IsNullOrWhiteSpace(port) ? "انتخاب نشده" : port;
        }

        private static void AddSummaryIfPermitted(ICollection<string> summaries, string schema, string table, string caption)
        {
            if (!AuthorizationGuard.Has(PermissionCatalog.TableCode(schema, "view"))) return;
            try { summaries.Add(caption + ": " + DeviceDiagnostics.CountRows(table).ToString("N0")); }
            catch { summaries.Add(caption + ": در دسترس نیست"); }
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
