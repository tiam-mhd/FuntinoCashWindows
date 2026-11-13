using System;
using System.Windows.Forms;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.Services;

namespace FuntinoControlCenter.Forms
{
    public partial class DeviceStatusForm : Form
    {
        public DeviceStatusForm()
        {
            InitializeComponent(); Load += DeviceStatusForm_Load;
            btnTestReader.Click += delegate { TestPort(AppSettings.Current.ReaderPort, AppSettings.Current.ReaderBaud, lblReaderStatus, "کارت‌خوان"); };
            btnTestPosPort.Click += delegate { TestPort(AppSettings.Current.PosPort, AppSettings.Current.PosBaud, lblPosStatus, "پورت POS"); };
            btnTestSdk.Click += TestSdk; btnRefresh.Click += delegate { RefreshEvents(); };
        }

        private void DeviceStatusForm_Load(object sender, EventArgs e)
        {
            lblReaderPort.Text = "کارت‌خوان: " + AppSettings.Current.ReaderPort + " / " + AppSettings.Current.ReaderBaud;
            lblPosPort.Text = "POS: " + AppSettings.Current.PosPort + " / " + AppSettings.Current.PosBaud;
            string main, device;
            bool mainOk = DatabaseManager.TestConnection(false, out main); bool deviceOk = DatabaseManager.TestConnection(true, out device);
            lblDbStatus.Text = (mainOk ? "● اصلی آماده" : "● اصلی قطع") + "     |     " + (deviceOk ? "● دستگاه‌ها آماده" : "● دستگاه‌ها قطع");
            lblDbStatus.ForeColor = mainOk && deviceOk ? System.Drawing.Color.FromArgb(40, 115, 86) : System.Drawing.Color.FromArgb(165, 77, 65);
            RefreshEvents();
        }

        private void TestPort(string port, int baud, Label status, string title)
        {
            string message; bool ok = HardwareService.TestPort(port, baud, out message);
            status.Text = ok ? "● دسترسی پورت تأیید شد" : "● دسترسی پورت ناموفق";
            status.ForeColor = ok ? System.Drawing.Color.FromArgb(36, 119, 83) : System.Drawing.Color.FromArgb(164, 74, 66);
            MessageBox.Show(message, title, MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning); RefreshEvents();
        }

        private void TestSdk(object sender, EventArgs e)
        {
            string message; bool ok = HardwareService.TestPosSdk(out message);
            lblPosStatus.Text = ok ? "● پاسخ تست SDK دریافت شد" : "● تست SDK کامل نشد";
            lblPosStatus.ForeColor = ok ? System.Drawing.Color.FromArgb(36, 119, 83) : System.Drawing.Color.FromArgb(164, 74, 66);
            MessageBox.Show(message, "تست SDK پایانه", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning); RefreshEvents();
        }

        private void RefreshEvents()
        {
            try
            {
                using (var connection = DatabaseManager.CreateDeviceConnection())
                {
                    connection.Open(); grid.DataSource = DatabaseManager.Query(connection, "SELECT TOP 100 EventId,EventType,PortName,IsSuccess,Details,CreatedAt FROM dbo.AppDeviceEvents ORDER BY EventId DESC");
                }
            }
            catch (Exception ex) { lblDbStatus.Text = DatabaseManager.SafeMessage(ex); }
        }
    }
}
