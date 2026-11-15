using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MessageBox = FuntinoControlCenter.Forms.AppMessageService;
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
        private bool isDeviceScanRunning;
        private bool isDatabaseTestRunning;
        private bool initialScanStarted;

        private sealed class PortScanResult
        {
            public IList<string> Ports { get; set; }
            public string Message { get; set; }
        }

        public SettingsForm() : this(false) { }

        public SettingsForm(bool bootstrapMode)
        {
            this.bootstrapMode = bootstrapMode;
            InitializeComponent();
        }

        private void WindowsAuth_CheckedChanged(object sender, EventArgs e) { UpdateAuthenticationFields(); }
        private void BtnClose_Click(object sender, EventArgs e) { Close(); }
        private async void TestMain_Click(object sender, EventArgs e) { await TestDatabaseAsync(false); }
        private async void TestDevice_Click(object sender, EventArgs e) { await TestDatabaseAsync(true); }
        private async void RescanReaderPorts_Click(object sender, EventArgs e) { await ScanPortsAsync(true, true, false); }
        private async void RescanPosPorts_Click(object sender, EventArgs e) { await ScanPortsAsync(true, false, true); }

        private void ImportReaderKey_Click(object sender, EventArgs e)
        {
            try
            {
                AuthorizationGuard.Require("settings.hardware.update");
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Title = "انتخاب فایل کلید کارت";
                    dialog.Filter = "فایل کلید کارت (*.bin)|*.bin|همه‌ی فایل‌ها (*.*)|*.*";
                    dialog.CheckFileExists = true;
                    dialog.Multiselect = false;
                    if (dialog.ShowDialog(this) != DialogResult.OK) return;

                    byte[] encoded = File.ReadAllBytes(dialog.FileName);
                    if (encoded.Length != 6)
                    {
                        Array.Clear(encoded, 0, encoded.Length);
                        throw new InvalidOperationException("فایل کلید کارت معتبر نیست؛ اندازه‌ی فایل باید شش بایت باشد.");
                    }
                    for (int i = 0; i < encoded.Length; i++)
                    {
                        encoded[i] = (byte)(encoded[i] ^ 0x12);
                        encoded[i] = (byte)(encoded[i] ^ 0x38);
                    }
                    try
                    {
                        AppSettings.Current.SetReaderCardKey(encoded);
                        AppSettings.Current.Save();
                    }
                    finally { Array.Clear(encoded, 0, encoded.Length); }
                    lblReaderKeyState.Text = "کلید امنیتی کارت برای همین حساب ویندوز به‌صورت رمزگذاری‌شده ذخیره شد.";
                    MessageBox.Show(this, lblReaderKeyState.Text, "کلید کارت آماده است", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "درون‌ریزی کلید کارت", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

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
                    lblReaderKeyState.Text = AppSettings.Current.ReaderCardKeyConfigured
                        ? "کلید کارت به‌صورت رمزگذاری‌شده برای همین حساب ویندوز ذخیره شده است."
                        : "برای خواندن کارت‌ها، فایل کلید کارت را یک‌بار درون‌ریزی کنید.";
                    btnImportReaderKey.Visible = canUpdateHardware;
                    cmbReaderBaud.Text = AppSettings.Current.ReaderBaud.ToString(); cmbPosBaud.Text = AppSettings.Current.PosBaud.ToString();
                    cmbReaderBaud.Enabled = cmbPosBaud.Enabled = canUpdateHardware;
                    btnRescanReader.Visible = btnRescanPos.Visible = canTestDevices;
                    pnlReaderPortInput.Visible = pnlPosPortInput.Visible = pnlReaderBaudInput.Visible = pnlPosBaudInput.Visible = canTestDevices;
                    lblReader.Visible = lblPos.Visible = lblReaderBaud.Visible = lblPosBaud.Visible = canTestDevices;
                    cmbReaderPort.Visible = cmbPosPort.Visible = canTestDevices;
                    if (canTestDevices)
                    {
                        lblReaderPortState.Text = "در حال آماده‌سازی برای شناسایی کارت‌خوان…";
                        lblPosPortState.Text = "در حال آماده‌سازی برای شناسایی پایانه‌ی فروش…";
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

        private async void SettingsForm_Shown(object sender, EventArgs e)
        {
            if (!canTestDevices || initialScanStarted) return;
            initialScanStarted = true;
            await ScanPortsAsync(false, true, true);
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
            if (cmbReaderPort.Items.Count > 1 && String.IsNullOrWhiteSpace(readerPort))
                throw new InvalidOperationException("چند کارت‌خوان پاسخ داده‌اند؛ یکی از درگاه‌های تأییدشده را انتخاب کنید.");
            if (!String.IsNullOrWhiteSpace(readerPort) && !cmbReaderPort.Items.Contains(readerPort)) throw new InvalidOperationException("درگاه کارت‌خوان باید از فهرست درگاه‌های تأییدشده انتخاب شود.");
            if (!String.IsNullOrWhiteSpace(posPort) && !cmbPosPort.Items.Contains(posPort)) throw new InvalidOperationException("درگاه پایانه‌ی فروش باید از فهرست درگاه‌های تأییدشده انتخاب شود.");
            if (!String.IsNullOrWhiteSpace(readerPort)) AppSettings.Current.ReaderPort = readerPort;
            AppSettings.Current.PosPort = posPort ?? String.Empty;
            AppSettings.Current.ReaderBaud = readerBaud; AppSettings.Current.PosBaud = posBaud;
        }

        private async Task ScanPortsAsync(bool showCompletion, bool scanReader, bool scanPos)
        {
            if (!canTestDevices) { AuthorizationGuard.Require("devices.test"); return; }
            if (isDeviceScanRunning) return;

            isDeviceScanRunning = true;
            btnRescanReader.Enabled = btnRescanPos.Enabled = false;
            btnSave.Enabled = false;
            try
            {
                AuthorizationGuard.RequireAny("settings.hardware.view", "settings.hardware.update");
                AuthorizationGuard.Require("devices.test");
                if (scanReader) await ScanReaderPortsCoreAsync(showCompletion);
                if (scanPos && !IsDisposed && !Disposing) await ScanPosPortsCoreAsync(showCompletion);
            }
            catch (Exception)
            {
                if (!IsDisposed && !Disposing)
                    MessageBox.Show(this, "بررسی دستگاه‌ها کامل نشد. اتصال دستگاه‌ها را بررسی کنید و دوباره تلاش کنید.", "بررسی دستگاه‌ها", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                isDeviceScanRunning = false;
                if (!IsDisposed && !Disposing)
                {
                    btnRescanReader.Enabled = btnRescanPos.Enabled = canTestDevices;
                    btnSave.Enabled = (canUpdateDatabase || canUpdateHardware) && !isDatabaseTestRunning;
                    cmbReaderBaud.Enabled = cmbPosBaud.Enabled = canUpdateHardware;
                    cmbReaderPort.Enabled = canUpdateHardware && cmbReaderPort.Items.Count > 0;
                    cmbPosPort.Enabled = canUpdateHardware && cmbPosPort.Items.Count > 0;
                }
            }
        }

        private async Task ScanReaderPortsCoreAsync(bool showCompletion)
        {
            string preferred = Convert.ToString(cmbReaderPort.SelectedItem);
            if (String.IsNullOrWhiteSpace(preferred)) preferred = AppSettings.Current.ReaderPort;
            int baudRate = ReadBaud(cmbReaderBaud, AppSettings.Current.ReaderBaud);
            loadingReader.BeginLoading("در حال پیدا کردن کارت‌خوان…", Properties.Resources.CardReader);
            lblReaderPortState.Visible = false;
            cmbReaderPort.Enabled = cmbReaderBaud.Enabled = false;
            try
            {
                await Task.Yield();
                PortScanResult result = await Task.Run(delegate
                {
                    string message;
                    IList<string> ports = HardwareService.DetectReaderPorts(baudRate, out message);
                    return new PortScanResult { Ports = ports, Message = message };
                });
                if (IsDisposed || Disposing) return;

                BindSuccessfulPorts(cmbReaderPort, result.Ports, preferred);
                if (result.Ports.Count == 1 && canUpdateHardware && !String.Equals(AppSettings.Current.ReaderPort, result.Ports[0], StringComparison.OrdinalIgnoreCase))
                {
                    AppSettings.Current.ReaderPort = result.Ports[0];
                    AppSettings.Current.Save();
                }
                lblReaderPortState.Text = BuildPortStatus(result.Message, result.Ports.Count, cmbReaderPort.SelectedItem != null);
                if (showCompletion && result.Ports.Count == 0)
                    MessageBox.Show(this, result.Message, "کارت‌خوان پیدا نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                if (IsDisposed || Disposing) return;
                cmbReaderPort.Items.Clear();
                lblReaderPortState.Text = "کارت‌خوان پیدا نشد. اتصال دستگاه و تنظیمات ارتباط را بررسی کنید و دوباره بزنید.";
                if (showCompletion) MessageBox.Show(this, lblReaderPortState.Text, "بررسی کارت‌خوان", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                {
                    loadingReader.EndLoading();
                    lblReaderPortState.Visible = true;
                }
            }
        }

        private async Task ScanPosPortsCoreAsync(bool showCompletion)
        {
            string preferred = Convert.ToString(cmbPosPort.SelectedItem);
            if (String.IsNullOrWhiteSpace(preferred)) preferred = AppSettings.Current.PosPort;
            loadingPos.BeginLoading("در حال پیدا کردن پایانه‌ی فروش…", Properties.Resources.PointOfSale);
            lblPosPortState.Visible = false;
            cmbPosPort.Enabled = cmbPosBaud.Enabled = false;
            try
            {
                await Task.Yield();
                PortScanResult result = await Task.Run(delegate
                {
                    string message;
                    IList<string> ports = HardwareService.DetectPosPorts(out message);
                    return new PortScanResult { Ports = ports, Message = message };
                });
                if (IsDisposed || Disposing) return;

                BindSuccessfulPorts(cmbPosPort, result.Ports, preferred);
                if (result.Ports.Count == 1 && canUpdateHardware && !String.Equals(AppSettings.Current.PosPort, result.Ports[0], StringComparison.OrdinalIgnoreCase))
                {
                    AppSettings.Current.PosPort = result.Ports[0];
                    AppSettings.Current.Save();
                }
                lblPosPortState.Text = BuildPortStatus(result.Message, result.Ports.Count, cmbPosPort.SelectedItem != null);
                if (showCompletion && result.Ports.Count == 0)
                    MessageBox.Show(this, result.Message, "پایانه‌ی فروش پیدا نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                if (IsDisposed || Disposing) return;
                cmbPosPort.Items.Clear();
                lblPosPortState.Text = "پایانه‌ی فروش پیدا نشد. اتصال دستگاه و تنظیمات ارتباط را بررسی کنید و دوباره بزنید.";
                if (showCompletion) MessageBox.Show(this, lblPosPortState.Text, "بررسی پایانه‌ی فروش", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                {
                    loadingPos.EndLoading();
                    lblPosPortState.Visible = true;
                }
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

        private async Task TestDatabaseAsync(bool device)
        {
            if (isDatabaseTestRunning) return;
            try
            {
                if (!bootstrapMode) AuthorizationGuard.Require("settings.database.test");
                if (canUpdateDatabase) ReadDatabaseControls();

                isDatabaseTestRunning = true;
                btnTestMain.Enabled = btnTestDevice.Enabled = btnSave.Enabled = false;
                lblDbHint.Visible = false;
                loadingDatabase.BeginLoading(device ? "در حال بررسی ارتباط با پایگاه دستگاه‌ها…" : "در حال بررسی ارتباط با پایگاه اصلی…", Properties.Resources.ConnectionSettings);
                await Task.Yield();

                var result = await Task.Run(delegate
                {
                    string message;
                    bool success = DatabaseManager.TestConnection(device, out message, bootstrapMode);
                    return new Tuple<bool, string>(success, message);
                });
                if (IsDisposed || Disposing) return;
                MessageBox.Show(this, result.Item2, result.Item1 ? "ارتباط برقرار است" : "ارتباط برقرار نشد", MessageBoxButtons.OK,
                    result.Item1 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                if (!IsDisposed && !Disposing) MessageBox.Show(this, ex.Message, "بررسی تنظیمات", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                isDatabaseTestRunning = false;
                if (!IsDisposed && !Disposing)
                {
                    loadingDatabase.EndLoading();
                    lblDbHint.Visible = true;
                    btnTestMain.Enabled = btnTestDevice.Enabled = canTestDatabase;
                    btnSave.Enabled = (canUpdateDatabase || canUpdateHardware) && !isDeviceScanRunning;
                }
            }
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
