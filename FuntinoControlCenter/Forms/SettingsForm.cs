using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Services;
using FuntinoControlCenter.Security;

namespace FuntinoControlCenter.Forms
{
    public partial class SettingsForm : Form
    {
        private readonly bool bootstrapMode;
        private bool canUpdateDatabase;
        private bool canUpdateHardware;
        private bool canTestDatabase;
        private bool canTestDevices;

        public SettingsForm() : this(false) { }

        public SettingsForm(bool bootstrapMode)
        {
            this.bootstrapMode = bootstrapMode;
            InitializeComponent();
        }

        private void WindowsAuth_CheckedChanged(object sender, EventArgs e) { UpdateAuthenticationFields(); }
        private void BtnClose_Click(object sender, EventArgs e) { Close(); }
        private void TestMain_Click(object sender, EventArgs e) { TestDatabase(false); }
        private void TestDevice_Click(object sender, EventArgs e) { TestDatabase(true); }
        private void RescanReaderPorts_Click(object sender, EventArgs e) { ScanReaderPorts(true); }
        private void RescanPosPorts_Click(object sender, EventArgs e) { ScanPosPorts(true); }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            try
            {
                if (bootstrapMode && AppSession.Current != null)
                    throw new UnauthorizedAccessException("تنظیم اتصال اولیه فقط پیش از ورود به حساب کاربری در دسترس است.");
                bool canReadDatabase = bootstrapMode || AuthorizationGuard.HasAny("settings.database.view", "settings.database.update");
                bool canViewDatabase = canReadDatabase || (!bootstrapMode && AuthorizationGuard.Has("settings.database.test"));
                bool canViewHardware = !bootstrapMode && AuthorizationGuard.HasAny("settings.hardware.view", "settings.hardware.update");
                canUpdateDatabase = bootstrapMode || AuthorizationGuard.Has("settings.database.update");
                canTestDevices = !bootstrapMode && AuthorizationGuard.Has("devices.test");
                canUpdateHardware = !bootstrapMode && AuthorizationGuard.Has("settings.hardware.update") && canTestDevices;
                canTestDatabase = bootstrapMode || AuthorizationGuard.Has("settings.database.test");
                if (!canViewDatabase && !canViewHardware)
                {
                    AuthorizationGuard.RequireAny("settings.database.view", "settings.database.update", "settings.database.test", "settings.hardware.view", "settings.hardware.update");
                    Close();
                    return;
                }
                pnlDatabase.Visible = canViewDatabase;
                pnlHardware.Visible = canViewHardware;
                btnTestMain.Visible = btnTestDevice.Visible = canTestDatabase;
                btnSave.Visible = canUpdateDatabase || canUpdateHardware;

                if (!canReadDatabase)
                {
                    lblDbSection.Text = "آزمایش اتصال پایگاه";
                    lblServer.Visible = txtSqlServer.Visible = false;
                    lblWindowsAuth.Visible = chkWindowsAuth.Visible = false;
                    lblSqlUser.Visible = txtSqlUser.Visible = false;
                    lblSqlPassword.Visible = txtSqlPassword.Visible = false;
                    lblDatabaseNames.Visible = false;
                    lblDbHint.Text = "آزمایش با تنظیم اتصال ذخیره‌شده انجام می‌شود.";
                }

                if (canViewHardware)
                {
                    cmbReaderBaud.Text = AppSettings.Current.ReaderBaud.ToString(); cmbPosBaud.Text = AppSettings.Current.PosBaud.ToString();
                    cmbReaderBaud.Enabled = cmbPosBaud.Enabled = canUpdateHardware;
                    btnRescanReader.Visible = btnRescanPos.Visible = canTestDevices;
                    pnlReaderPortInput.Visible = pnlPosPortInput.Visible = pnlReaderBaudInput.Visible = pnlPosBaudInput.Visible = canTestDevices;
                    lblReader.Visible = lblPos.Visible = lblReaderBaud.Visible = lblPosBaud.Visible = canTestDevices;
                    cmbReaderPort.Visible = cmbPosPort.Visible = canTestDevices;
                    if (canTestDevices)
                    {
                        ScanReaderPorts(false);
                        ScanPosPorts(false);
                    }
                    else
                    {
                        lblReaderPortState.Text = lblPosPortState.Text = "برای بررسی درگاه‌ها، دسترسی «آزمایش دستگاه‌ها» لازم است.";
                    }
                }
                if (canReadDatabase)
                {
                    txtSqlServer.Text = AppSettings.Current.SqlServer;
                    chkWindowsAuth.Checked = AppSettings.Current.IntegratedSecurity;
                    txtSqlUser.Text = canUpdateDatabase ? AppSettings.Current.SqlUser : "";
                    txtSqlPassword.Text = canUpdateDatabase ? AppSettings.Current.SqlPassword : "";
                    lblSqlUser.Visible = txtSqlUser.Visible = lblSqlPassword.Visible = txtSqlPassword.Visible = canUpdateDatabase;
                    txtSqlServer.ReadOnly = chkWindowsAuth.Enabled = !canUpdateDatabase;
                }
                if (canTestDatabase && !canUpdateDatabase)
                    lblDbHint.Text += " مقدارهای اتصال از تنظیمات ذخیره‌شده خوانده می‌شوند.";
                UpdateAuthenticationFields();
                if (!bootstrapMode)
                    AuthorizationMonitor.Attach(this, delegate
                    {
                        return AuthorizationGuard.HasAny("settings.database.view", "settings.database.update", "settings.database.test", "settings.hardware.view", "settings.hardware.update");
                    });
            }
            catch (Exception ex) { MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "تنظیمات", MessageBoxButtons.OK, MessageBoxIcon.Warning); Close(); }
        }

        private void UpdateAuthenticationFields() { txtSqlUser.Enabled = canUpdateDatabase && !chkWindowsAuth.Checked; txtSqlPassword.Enabled = canUpdateDatabase && !chkWindowsAuth.Checked; }

        private void ReadControls()
        {
            if (canUpdateDatabase) ReadDatabaseControls();
            if (canUpdateHardware) ReadHardwareControls();
        }

        private void ReadDatabaseControls()
        {
            if (bootstrapMode)
            {
                if (AppSession.Current != null) throw new UnauthorizedAccessException("تنظیم اتصال اولیه فقط پیش از ورود به حساب کاربری در دسترس است.");
            }
            else AuthorizationGuard.Require("settings.database.update");
            if (String.IsNullOrWhiteSpace(txtSqlServer.Text)) throw new InvalidOperationException("نام SQL Server را وارد کنید.");
            AppSettings.Current.SqlServer = txtSqlServer.Text.Trim();
            AppSettings.Current.IntegratedSecurity = chkWindowsAuth.Checked;
            AppSettings.Current.SqlUser = txtSqlUser.Text.Trim();
            AppSettings.Current.SqlPassword = txtSqlPassword.Text;
        }

        private void ReadHardwareControls()
        {
            AuthorizationGuard.Require("settings.hardware.update");
            AuthorizationGuard.Require("devices.test");
            int readerBaud, posBaud;
            if (!Int32.TryParse(cmbReaderBaud.Text, out readerBaud) || readerBaud < 1200) throw new InvalidOperationException("سرعت کارت‌خوان معتبر نیست.");
            if (!Int32.TryParse(cmbPosBaud.Text, out posBaud) || posBaud < 1200) throw new InvalidOperationException("سرعت پایانه POS معتبر نیست.");
            string readerPort = Convert.ToString(cmbReaderPort.SelectedItem);
            string posPort = Convert.ToString(cmbPosPort.SelectedItem);
            if (!String.IsNullOrWhiteSpace(readerPort) && !cmbReaderPort.Items.Contains(readerPort)) throw new InvalidOperationException("درگاه کارت‌خوان باید از فهرست درگاه‌های تأییدشده انتخاب شود.");
            if (!String.IsNullOrWhiteSpace(posPort) && !cmbPosPort.Items.Contains(posPort)) throw new InvalidOperationException("درگاه پایانه‌ی فروش باید از فهرست درگاه‌های تأییدشده انتخاب شود.");
            AppSettings.Current.ReaderPort = readerPort ?? String.Empty; AppSettings.Current.PosPort = posPort ?? String.Empty;
            AppSettings.Current.ReaderBaud = readerBaud; AppSettings.Current.PosBaud = posBaud;
        }

        private void ScanReaderPorts(bool showCompletion)
        {
            if (!canTestDevices) { AuthorizationGuard.Require("devices.test"); return; }
            try
            {
                AuthorizationGuard.RequireAny("settings.hardware.view", "settings.hardware.update");
                AuthorizationGuard.Require("devices.test");
                string preferred = Convert.ToString(cmbReaderPort.SelectedItem);
                if (String.IsNullOrWhiteSpace(preferred)) preferred = AppSettings.Current.ReaderPort;
                string message;
                var ports = HardwareService.DetectReaderPorts(ReadBaud(cmbReaderBaud, AppSettings.Current.ReaderBaud), out message);
                BindSuccessfulPorts(cmbReaderPort, ports, preferred);
                if (ports.Count == 1 && canUpdateHardware && !String.Equals(AppSettings.Current.ReaderPort, ports[0], StringComparison.OrdinalIgnoreCase))
                {
                    AppSettings.Current.ReaderPort = ports[0];
                    AppSettings.Current.Save();
                }
                lblReaderPortState.Text = BuildPortStatus(message, ports.Count, cmbReaderPort.SelectedItem != null);
                cmbReaderPort.Enabled = canUpdateHardware && ports.Count > 0;
                btnRescanReader.Enabled = true;
                if (showCompletion && ports.Count == 0)
                    MessageBox.Show(this, message, "کارت‌خوان پیدا نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                cmbReaderPort.Items.Clear();
                lblReaderPortState.Text = "بررسی کارت‌خوان انجام نشد: " + DatabaseManager.SafeMessage(ex);
                if (showCompletion) MessageBox.Show(this, lblReaderPortState.Text, "بررسی کارت‌خوان", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ScanPosPorts(bool showCompletion)
        {
            if (!canTestDevices) { AuthorizationGuard.Require("devices.test"); return; }
            try
            {
                AuthorizationGuard.RequireAny("settings.hardware.view", "settings.hardware.update");
                AuthorizationGuard.Require("devices.test");
                string preferred = Convert.ToString(cmbPosPort.SelectedItem);
                if (String.IsNullOrWhiteSpace(preferred)) preferred = AppSettings.Current.PosPort;
                string message;
                var ports = HardwareService.DetectPosPorts(out message);
                BindSuccessfulPorts(cmbPosPort, ports, preferred);
                if (ports.Count == 1 && canUpdateHardware && !String.Equals(AppSettings.Current.PosPort, ports[0], StringComparison.OrdinalIgnoreCase))
                {
                    AppSettings.Current.PosPort = ports[0];
                    AppSettings.Current.Save();
                }
                lblPosPortState.Text = BuildPortStatus(message, ports.Count, cmbPosPort.SelectedItem != null);
                cmbPosPort.Enabled = canUpdateHardware && ports.Count > 0;
                btnRescanPos.Enabled = true;
                if (showCompletion && ports.Count == 0)
                    MessageBox.Show(this, message, "پایانه‌ی فروش پیدا نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                cmbPosPort.Items.Clear();
                lblPosPortState.Text = "بررسی پایانه‌ی فروش انجام نشد: " + DatabaseManager.SafeMessage(ex);
                if (showCompletion) MessageBox.Show(this, lblPosPortState.Text, "بررسی پایانه‌ی فروش", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static void BindSuccessfulPorts(ComboBox combo, IList<string> ports, string preferredPort)
        {
            combo.BeginUpdate();
            try
            {
                combo.Items.Clear();
                foreach (string port in ports) combo.Items.Add(port);
                if (ports.Count == 1) combo.SelectedIndex = 0;
                else if (!String.IsNullOrWhiteSpace(preferredPort)) combo.SelectedItem = ports.FirstOrDefault(port => String.Equals(port, preferredPort, StringComparison.OrdinalIgnoreCase));
                if (ports.Count > 1 && combo.SelectedIndex < 0) combo.SelectedIndex = -1;
            }
            finally { combo.EndUpdate(); }
        }

        private static string BuildPortStatus(string message, int count, bool hasSelection)
        {
            if (count == 0) return message;
            if (count == 1) return message + " درگاه " + (hasSelection ? "به‌صورت خودکار انتخاب شد." : "پیدا شد.");
            return message + (hasSelection ? " درگاه ذخیره‌شده هنوز معتبر است." : " چند درگاه پیدا شد؛ یکی را انتخاب کنید.");
        }

        private static int ReadBaud(ComboBox combo, int fallback)
        {
            int baudRate;
            return Int32.TryParse(combo.Text, out baudRate) && baudRate >= 1200 ? baudRate : fallback;
        }

        private void TestDatabase(bool device)
        {
            try
            {
                if (!bootstrapMode) AuthorizationGuard.Require("settings.database.test");
                if (canUpdateDatabase) ReadDatabaseControls();
                string message;
                bool success = DatabaseManager.TestConnection(device, out message, bootstrapMode);
                MessageBox.Show(message, success ? "اتصال برقرار است" : "اتصال ناموفق", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "بررسی تنظیمات", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!canUpdateDatabase && !canUpdateHardware) throw new UnauthorizedAccessException("مجوز تغییر تنظیمات را ندارید.");
                ReadControls(); AppSettings.Current.Save(); DialogResult = DialogResult.OK;
                MessageBox.Show("تنظیمات مجاز ذخیره شد.", "ذخیره تنظیمات", MessageBoxButtons.OK, MessageBoxIcon.Information); Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "ذخیره نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
    }
}
