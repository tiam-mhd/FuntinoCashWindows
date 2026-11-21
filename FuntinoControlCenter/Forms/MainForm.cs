using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MessageBox = FuntinoControlCenter.Forms.AppMessageService;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Security;
using FuntinoControlCenter.Services;

namespace FuntinoControlCenter.Forms
{
    public partial class MainForm : Form
    {
        private bool initialCardOperationsShown;
        private bool dashboardRefreshRunning;
        private bool dashboardRefreshQueued;

        private sealed class DashboardRefreshSnapshot
        {
            public string RechargeCount { get; set; }
            public bool MainDatabaseReady { get; set; }
            public List<string> Summaries { get; private set; }

            public DashboardRefreshSnapshot()
            {
                MainDatabaseReady = true;
                Summaries = new List<string>();
            }
        }
        public MainForm() { InitializeComponent(); }

        private void OpenTables_Click(object sender, EventArgs e) { using (var form = new TableBrowserForm()) form.ShowDialog(this); RefreshDashboard(); }
        private void OpenReports_Click(object sender, EventArgs e) { using (var form = new ReportsForm()) form.ShowDialog(this); }
        private void OpenDevices_Click(object sender, EventArgs e) { using (var form = new DeviceStatusForm()) form.ShowDialog(this); RefreshDashboard(); }
        private void OpenSettings_Click(object sender, EventArgs e) { using (var form = new SettingsForm()) form.ShowDialog(this); RefreshDashboard(); }
        private void OpenOperations_Click(object sender, EventArgs e) { using (var form = new OperationsManagementForm()) form.ShowDialog(this); ApplyPermissions(); RefreshDashboard(); }
        private void OpenPassword_Click(object sender, EventArgs e) { using (var form = new ChangePasswordForm()) form.ShowDialog(this); }
        private void OpenRecharge_Click(object sender, EventArgs e) { OpenCardOperations(); }
        private void OpenTickets_Click(object sender, EventArgs e) { OpenTable("ticket.TicketTransaction"); }
        private void OpenPos_Click(object sender, EventArgs e) { OpenTable("payments.PointOfSaleTransaction"); }
        private void OpenCardReader_Click(object sender, EventArgs e) { OpenCardOperations(); }
        private void Logout_Click(object sender, EventArgs e) { Close(); }

        private void OpenCustomers_Click(object sender, EventArgs e)
        {
            try
            {
                AuthorizationGuard.Require("customers.view");
                using (var form = new CustomerFamilyForm()) form.ShowDialog(this);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.ShowWarning(this, "مجوز مشاهدهٔ مشتریان شعبه برای این حساب فعال نیست.", "دسترسی محدود است");
                ApplyPermissions();
            }
        }

        private void OpenEdgeSync_Click(object sender, EventArgs e)
        {
            try
            {
                AuthorizationGuard.Require("edge.sync.view");
                using (var form = new EdgeSyncStatusForm()) form.ShowDialog(this);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.ShowWarning(this, "مجوز مشاهدهٔ وضعیت همگام‌سازی برای این حساب فعال نیست.", "دسترسی محدود است");
                ApplyPermissions();
            }
        }

        private void OpenEdgeSettings_Click(object sender, EventArgs e)
        {
            try
            {
                AuthorizationGuard.Require("edge.configuration.manage");
                using (var form = new EdgeConfigurationForm()) form.ShowDialog(this);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.ShowWarning(this, "مجوز تنظیم ارتباط شعبه برای این حساب فعال نیست.", "دسترسی محدود است");
                ApplyPermissions();
            }
        }

        private void OpenCardRecovery_Click(object sender, EventArgs e)
        {
            try
            {
                AuthorizationGuard.Require("cards.recharge");
                AuthorizationGuard.Require(PermissionCatalog.TableCode("finance", "create"));
                AuthorizationGuard.Require("operations.recharge_terms.view");
                AuthorizationGuard.Require("edge.events.create");
                using (var form = new CardRechargeRecoveryForm()) form.ShowDialog(this);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.ShowWarning(this, "مجوز لازم برای تطبیق عملیات شارژ برای این حساب فعال نیست.", "دسترسی محدود است");
                ApplyPermissions();
            }
        }

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

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (initialCardOperationsShown) return;
            initialCardOperationsShown = true;
            bool canRechargeCard = AuthorizationGuard.Has("cards.recharge") && AuthorizationGuard.Has(PermissionCatalog.TableCode("finance", "create")) && AuthorizationGuard.Has("operations.recharge_terms.view");
            if (AuthorizationGuard.HasAny("cards.details.view", "cards.balance.view", "cards.remainder.view", "cards.history.view") || canRechargeCard)
                OpenCardOperations();
        }

        private void ApplyPermissions()
        {
            bool canDashboard = AuthorizationGuard.Has("dashboard.view");
            string[] tableViews = PermissionCatalog.DataSchemas.Select(schema => PermissionCatalog.TableCode(schema, "view")).ToArray();
            btnTables.Visible = AuthorizationGuard.HasAny(tableViews);
            btnReports.Visible = AuthorizationGuard.Has("reports.view") && AuthorizationGuard.HasAny(tableViews);
            btnDevices.Visible = AuthorizationGuard.HasAny("devices.view", "devices.test", "devices.events.view");
            btnSettings.Visible = AuthorizationGuard.HasAny("settings.database.view", "settings.database.update", "settings.database.test", "settings.hardware.view", "settings.hardware.update");
            btnOperations.Visible = HasBusinessManagementAccess();
            btnAccess.Visible = AuthorizationGuard.HasAny("users.view", "users.create", "users.update", "users.reset_password", "users.assign_roles", "users.deactivate", "roles.view", "roles.create", "roles.update", "roles.assign_permissions", "roles.delete");
            btnPassword.Visible = AppSession.Current != null;
            btnCustomers.Visible = AuthorizationGuard.Has("customers.view");
            btnEdgeSync.Visible = AuthorizationGuard.Has("edge.sync.view");
            btnEdgeSettings.Visible = AuthorizationGuard.Has("edge.configuration.manage");
            btnCardRecovery.Visible = AuthorizationGuard.Has("cards.recharge") &&
                AuthorizationGuard.Has(PermissionCatalog.TableCode("finance", "create")) &&
                AuthorizationGuard.Has("operations.recharge_terms.view") && AuthorizationGuard.Has("edge.events.create");

            bool canRechargeCard = AuthorizationGuard.Has("cards.recharge") && AuthorizationGuard.Has(PermissionCatalog.TableCode("finance", "create")) && AuthorizationGuard.Has("operations.recharge_terms.view");
            bool canCardOperations = AuthorizationGuard.HasAny("cards.details.view", "cards.balance.view", "cards.remainder.view", "cards.history.view") || canRechargeCard;
            bool canReadCardBalance = AuthorizationGuard.Has("cards.balance.view");
            pnlCardHero.Visible = canCardOperations;
            lblBalanceTitle.Visible = lblBalanceValue.Visible = lblBalanceHint.Visible = canReadCardBalance;
            pnlCardTables.Visible = canDashboard;
            pnlCardMain.Visible = canDashboard;
            pnlCardDevice.Visible = canDashboard;
            pnlCardPorts.Visible = canDashboard;
            lblSummaryCaption.Visible = canDashboard;
            lblBusinessSummary.Visible = canDashboard;

            btnRecharge.Visible = canRechargeCard;
            btnTickets.Visible = AuthorizationGuard.Has(PermissionCatalog.TableCode("ticket", "view"));
            btnPos.Visible = AuthorizationGuard.Has(PermissionCatalog.TableCode("payments", "view"));
            btnCardReader.Visible = AuthorizationGuard.HasAny("cards.details.view", "cards.balance.view", "cards.remainder.view", "cards.history.view");
            lblQuickActions.Visible = btnRecharge.Visible || btnTickets.Visible || btnPos.Visible || btnCardReader.Visible;
        }

        private static bool HasBusinessManagementAccess()
        {
            return BusinessManagementRepository.HasAccess(BusinessManagementArea.Attractions, "view") ||
                BusinessManagementRepository.HasAccess(BusinessManagementArea.ControllerDevices, "view") ||
                BusinessManagementRepository.HasAccess(BusinessManagementArea.Promotions, "view") ||
                BusinessManagementRepository.HasAccess(BusinessManagementArea.DevicePromotions, "view") ||
                BusinessManagementRepository.HasAccess(BusinessManagementArea.DiscountPolicies, "view") ||
                BusinessManagementRepository.HasAccess(BusinessManagementArea.DiscountRates, "view") ||
                BusinessManagementRepository.HasAccess(BusinessManagementArea.Deposits, "view") ||
                BusinessManagementRepository.HasAccess(BusinessManagementArea.RechargeLimit, "view") ||
                BusinessManagementRepository.HasAccess(BusinessManagementArea.HourlyPlans, "view") ||
                BusinessManagementRepository.HasAccess(BusinessManagementArea.Schedules, "view") ||
                BusinessManagementRepository.HasAccess(BusinessManagementArea.Tickets, "view");
        }

        private async void RefreshDashboard()
        {
            if (dashboardRefreshRunning)
            {
                dashboardRefreshQueued = true;
                return;
            }

            dashboardRefreshRunning = true;
            bool summaryWasVisible = lblBusinessSummary.Visible;
            bool loadingStarted = false;
            try
            {
                if (!AuthorizationGuard.Has("dashboard.view")) return;
                bool canViewRecharge = AuthorizationGuard.Has(PermissionCatalog.TableCode("finance", "view"));
                bool canViewTickets = AuthorizationGuard.Has(PermissionCatalog.TableCode("ticket", "view"));
                bool canViewPos = AuthorizationGuard.Has(PermissionCatalog.TableCode("payments", "view"));
                string readerPort = DisplayPort(AppSettings.Current.ReaderPort);
                string posPort = DisplayPort(AppSettings.Current.PosPort);
                loadingStarted = true;
                lblBusinessSummary.Visible = false;
                loadingDashboard.BeginLoading("در حال به‌روزرسانی نمای کلی سامانه…", Properties.Resources.ConnectionSettings);
                await Task.Yield();
                DashboardRefreshSnapshot snapshot = await Task.Run(delegate
                {
                    var result = new DashboardRefreshSnapshot();
                    if (canViewRecharge)
                    {
                        result.RechargeCount = ReadDashboardCount("finance.RechargeTransaction", result);
                        result.Summaries.Add("سوابق شارژ: " + result.RechargeCount);
                    }
                    else result.RechargeCount = "—";

                    if (canViewTickets) result.Summaries.Add("سوابق بلیت: " + ReadDashboardCount("ticket.TicketTransaction", result));
                    if (canViewPos) result.Summaries.Add("تراکنش‌های پایانه: " + ReadDashboardCount("payments.PointOfSaleTransaction", result));
                    return result;
                });
                if (IsDisposed || Disposing) return;

                lblTableCountValue.Text = snapshot.RechargeCount;
                lblTableCountCaption.Text = canViewRecharge ? "تراکنش شارژ ثبت‌شده" : "دسترسی مشاهده‌ی سوابق شارژ ندارید";
                lblMainStatus.Text = snapshot.MainDatabaseReady ? "پایگاه اصلی آماده است" : "ارتباط با پایگاه اصلی نیاز به بررسی دارد";
                lblDeviceStatus.Text = "پایگاه دستگاه‌ها آماده است";
                lblPorts.Text = "درگاه انتخاب‌شده ـ کارت‌خوان: " + readerPort + "   |   پایانه: " + posPort;
                lblBusinessSummary.Text = snapshot.Summaries.Count == 0
                    ? "از بخش‌های مجاز در فهرست کناری برای مشاهده‌ی سوابق استفاده کنید."
                    : String.Join("     •     ", snapshot.Summaries.ToArray());
            }
            catch (Exception)
            {
                if (IsDisposed || Disposing) return;
                if (AppSession.Current == null) { Close(); return; }
                lblMainStatus.Text = "ارتباط با اطلاعات سامانه برقرار نشد";
                lblBusinessSummary.Text = "اتصال شبکه و وضعیت سرویس پایگاه داده را بررسی کنید و دوباره تلاش کنید.";
            }
            finally
            {
                dashboardRefreshRunning = false;
                if (!IsDisposed && !Disposing)
                {
                    if (loadingStarted)
                    {
                        loadingDashboard.EndLoading();
                        try { lblBusinessSummary.Visible = summaryWasVisible && AuthorizationGuard.Has("dashboard.view"); }
                        catch { lblBusinessSummary.Visible = false; }
                    }
                    if (dashboardRefreshQueued)
                    {
                        dashboardRefreshQueued = false;
                        RefreshDashboard();
                    }
                }
            }
        }

        private static string ReadDashboardCount(string table, DashboardRefreshSnapshot snapshot)
        {
            try
            {
                return DeviceDiagnostics.CountRows(table).ToString("N0");
            }
            catch
            {
                snapshot.MainDatabaseReady = false;
                return "در دسترس نیست";
            }
        }

        private static string DisplayPort(string port)
        {
            return String.IsNullOrWhiteSpace(port) ? "انتخاب نشده" : port;
        }

        private void OpenTable(string tableName)
        {
            string schema = (tableName ?? "").Split('.').FirstOrDefault() ?? "";
            if (!AuthorizationGuard.Has(PermissionCatalog.TableCode(schema, "view"))) return;
            using (var form = new TableBrowserForm(false, tableName)) form.ShowDialog(this);
            RefreshDashboard();
        }

        private void OpenCardOperations()
        {
            bool canRechargeCard = AuthorizationGuard.Has("cards.recharge") && AuthorizationGuard.Has(PermissionCatalog.TableCode("finance", "create"));
            if (!AuthorizationGuard.HasAny("cards.details.view", "cards.balance.view", "cards.remainder.view", "cards.history.view") && !canRechargeCard)
            {
                MessageBox.Show(this, "برای عملیات کارت مجوزی ندارید.", "دسترسی محدود", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (var form = new CardOperationsForm()) form.ShowDialog(this);
            RefreshDashboard();
        }
    }
}
