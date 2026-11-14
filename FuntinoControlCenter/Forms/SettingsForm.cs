using System;
using System.IO.Ports;
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
                canUpdateHardware = !bootstrapMode && AuthorizationGuard.Has("settings.hardware.update");
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
                    var ports = HardwareService.GetPortNames();
                    foreach (string port in ports) { if (!cmbReaderPort.Items.Contains(port)) cmbReaderPort.Items.Add(port); if (!cmbPosPort.Items.Contains(port)) cmbPosPort.Items.Add(port); }
                    for (int i = 1; i <= 40; i++)
                    {
                        string port = "COM" + i.ToString(); if (!cmbReaderPort.Items.Contains(port)) cmbReaderPort.Items.Add(port); if (!cmbPosPort.Items.Contains(port)) cmbPosPort.Items.Add(port);
                    }
                    cmbReaderPort.Text = AppSettings.Current.ReaderPort; cmbPosPort.Text = AppSettings.Current.PosPort;
                    cmbReaderBaud.Text = AppSettings.Current.ReaderBaud.ToString(); cmbPosBaud.Text = AppSettings.Current.PosBaud.ToString();
                    cmbReaderPort.Enabled = cmbPosPort.Enabled = cmbReaderBaud.Enabled = cmbPosBaud.Enabled = canUpdateHardware;
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
            int readerBaud, posBaud;
            if (!Int32.TryParse(cmbReaderBaud.Text, out readerBaud) || readerBaud < 1200) throw new InvalidOperationException("سرعت کارت‌خوان معتبر نیست.");
            if (!Int32.TryParse(cmbPosBaud.Text, out posBaud) || posBaud < 1200) throw new InvalidOperationException("سرعت پایانه POS معتبر نیست.");
            AppSettings.Current.ReaderPort = cmbReaderPort.Text.Trim(); AppSettings.Current.PosPort = cmbPosPort.Text.Trim();
            AppSettings.Current.ReaderBaud = readerBaud; AppSettings.Current.PosBaud = posBaud;
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
