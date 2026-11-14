using System;
using System.Windows.Forms;
using MessageBox = FuntinoControlCenter.Forms.AppMessageService;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Services;
using FuntinoControlCenter.Security;

namespace FuntinoControlCenter.Forms
{
    public partial class DeviceStatusForm : Form
    {
        public DeviceStatusForm()
        {
            InitializeComponent();
        }

        private void TestReaderPort_Click(object sender, EventArgs e) { TestPort(AppSettings.Current.ReaderPort, AppSettings.Current.ReaderBaud, lblReaderStatus, "کارت‌خوان"); }
        private void TestPosPort_Click(object sender, EventArgs e) { TestPort(AppSettings.Current.PosPort, AppSettings.Current.PosBaud, lblPosStatus, "پورت POS"); }
        private void Refresh_Click(object sender, EventArgs e) { RefreshEvents(); }

        private void DeviceStatusForm_Load(object sender, EventArgs e)
        {
            try
            {
                bool canView = AuthorizationGuard.Has("devices.view");
                bool canTest = AuthorizationGuard.Has("devices.test");
                bool canSeeEvents = AuthorizationGuard.Has("devices.events.view");
                AuthorizationGuard.RequireAny("devices.view", "devices.test", "devices.events.view");
                pnlReader.Visible = canView || canTest;
                pnlPos.Visible = canView || canTest;
                btnTestReader.Visible = btnTestPosPort.Visible = btnTestSdk.Visible = canTest;
                lblDbStatus.Visible = canView;
                lblHistory.Visible = grid.Visible = pnlFooter.Visible = canSeeEvents;
                if (canView || canTest)
                {
                    lblReaderPort.Text = "کارت‌خوان: " + AppSettings.Current.ReaderPort + " / " + AppSettings.Current.ReaderBaud;
                    lblPosPort.Text = "POS: " + AppSettings.Current.PosPort + " / " + AppSettings.Current.PosBaud;
                }
                if (canView)
                {
                    string main, device;
                    bool mainOk = DatabaseManager.TestConnection(false, out main); bool deviceOk = DatabaseManager.TestConnection(true, out device);
                    lblDbStatus.Text = (mainOk ? "● اصلی آماده" : "● اصلی قطع") + "     |     " + (deviceOk ? "● دستگاه‌ها آماده" : "● دستگاه‌ها قطع");
                    lblDbStatus.ForeColor = mainOk && deviceOk ? System.Drawing.Color.FromArgb(40, 115, 86) : System.Drawing.Color.FromArgb(165, 77, 65);
                }
                if (canSeeEvents) RefreshEvents();
                if (!canView && !canTest)
                {
                    pnlReader.Visible = pnlPos.Visible = false;
                    lblDbStatus.Text = "سوابق آزمایش دستگاه‌ها";
                }
                AuthorizationMonitor.Attach(this, HasDeviceAccess);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "دسترسی به وضعیت دستگاه‌ها", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
            }
        }

        private void TestPort(string port, int baud, Label status, string title)
        {
            try
            {
                string message; bool ok = HardwareService.TestPort(port, baud, out message);
                status.Text = ok ? "● دسترسی پورت تأیید شد" : "● دسترسی پورت ناموفق";
                status.ForeColor = ok ? System.Drawing.Color.FromArgb(36, 119, 83) : System.Drawing.Color.FromArgb(164, 74, 66);
                MessageBox.Show(message, title, MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                if (AuthorizationGuard.Has("devices.events.view")) RefreshEvents();
            }
            catch (Exception ex) { MessageBox.Show(this, DatabaseManager.SafeMessage(ex), title, MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void TestSdk(object sender, EventArgs e)
        {
            try
            {
                string message; bool ok = HardwareService.TestPosSdk(out message);
                lblPosStatus.Text = ok ? "● پاسخ تست SDK دریافت شد" : "● تست SDK کامل نشد";
                lblPosStatus.ForeColor = ok ? System.Drawing.Color.FromArgb(36, 119, 83) : System.Drawing.Color.FromArgb(164, 74, 66);
                MessageBox.Show(message, "تست SDK پایانه", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                if (AuthorizationGuard.Has("devices.events.view")) RefreshEvents();
            }
            catch (Exception ex) { MessageBox.Show(this, DatabaseManager.SafeMessage(ex), "تست SDK پایانه", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void RefreshEvents()
        {
            try
            {
                AuthorizationGuard.Require("devices.events.view");
                using (var connection = DatabaseManager.CreateDeviceConnection())
                {
                    connection.Open(); grid.DataSource = DatabaseManager.Query(connection, "SELECT TOP 100 DeviceDiagnosticEventId,EventType,PortName,IsSuccess,Details,CreatedAt FROM app.DeviceDiagnosticEvent ORDER BY DeviceDiagnosticEventId DESC");
                }
            }
            catch (Exception ex) { lblDbStatus.Text = DatabaseManager.SafeMessage(ex); }
        }

        private bool HasDeviceAccess()
        {
            return AuthorizationGuard.HasAny("devices.view", "devices.test", "devices.events.view");
        }
    }
}
