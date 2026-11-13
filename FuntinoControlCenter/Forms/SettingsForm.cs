using System;
using System.IO.Ports;
using System.Windows.Forms;
using FuntinoControlCenter.Database;

namespace FuntinoControlCenter.Forms
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent(); Load += SettingsForm_Load;
            chkWindowsAuth.CheckedChanged += delegate { UpdateAuthenticationFields(); };
            btnSave.Click += BtnSave_Click; btnClose.Click += delegate { Close(); };
            btnTestMain.Click += delegate { TestDatabase(false); }; btnTestDevice.Click += delegate { TestDatabase(true); };
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            var ports = HardwareService.GetPortNames();
            foreach (string port in ports) { if (!cmbReaderPort.Items.Contains(port)) cmbReaderPort.Items.Add(port); if (!cmbPosPort.Items.Contains(port)) cmbPosPort.Items.Add(port); }
            for (int i = 1; i <= 40; i++)
            {
                string port = "COM" + i.ToString(); if (!cmbReaderPort.Items.Contains(port)) cmbReaderPort.Items.Add(port); if (!cmbPosPort.Items.Contains(port)) cmbPosPort.Items.Add(port);
            }
            cmbReaderPort.Text = AppSettings.Current.ReaderPort; cmbPosPort.Text = AppSettings.Current.PosPort;
            cmbReaderBaud.Text = AppSettings.Current.ReaderBaud.ToString(); cmbPosBaud.Text = AppSettings.Current.PosBaud.ToString();
            txtSqlServer.Text = AppSettings.Current.SqlServer; chkWindowsAuth.Checked = AppSettings.Current.IntegratedSecurity;
            txtSqlUser.Text = AppSettings.Current.SqlUser; txtSqlPassword.Text = AppSettings.Current.SqlPassword;
            UpdateAuthenticationFields();
        }

        private void UpdateAuthenticationFields() { txtSqlUser.Enabled = !chkWindowsAuth.Checked; txtSqlPassword.Enabled = !chkWindowsAuth.Checked; }

        private void ReadControls()
        {
            int readerBaud, posBaud;
            if (!Int32.TryParse(cmbReaderBaud.Text, out readerBaud) || readerBaud < 1200) throw new InvalidOperationException("Baud کارت‌خوان معتبر نیست.");
            if (!Int32.TryParse(cmbPosBaud.Text, out posBaud) || posBaud < 1200) throw new InvalidOperationException("Baud پایانه POS معتبر نیست.");
            if (String.IsNullOrWhiteSpace(txtSqlServer.Text)) throw new InvalidOperationException("نام SQL Server را وارد کنید.");
            AppSettings.Current.SqlServer = txtSqlServer.Text.Trim(); AppSettings.Current.IntegratedSecurity = chkWindowsAuth.Checked;
            AppSettings.Current.SqlUser = txtSqlUser.Text.Trim(); AppSettings.Current.SqlPassword = txtSqlPassword.Text;
            AppSettings.Current.ReaderPort = cmbReaderPort.Text.Trim(); AppSettings.Current.PosPort = cmbPosPort.Text.Trim();
            AppSettings.Current.ReaderBaud = readerBaud; AppSettings.Current.PosBaud = posBaud;
        }

        private void TestDatabase(bool device)
        {
            try
            {
                ReadControls(); AppSettings.Current.Save(); string message;
                bool success = DatabaseManager.TestConnection(device, out message);
                MessageBox.Show(message, success ? "اتصال برقرار است" : "اتصال ناموفق", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "بررسی تنظیمات", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try { ReadControls(); AppSettings.Current.Save(); DialogResult = DialogResult.OK; MessageBox.Show("تنظیمات ذخیره شد. برای اجرای مهاجرت‌ها، هر دو دیتابیس _New باید Attach باشند.", "ذخیره تنظیمات", MessageBoxButtons.OK, MessageBoxIcon.Information); Close(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "ذخیره نشد", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
    }
}
