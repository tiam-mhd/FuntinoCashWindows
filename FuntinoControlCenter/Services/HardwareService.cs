using System;
using System.IO.Ports;

namespace FuntinoControlCenter.Services
{
    internal static class HardwareService
    {
        public static string[] GetPortNames()
        {
            try { return SerialPort.GetPortNames(); } catch { return new string[0]; }
        }

        public static bool TestPort(string portName, int baudRate, out string message)
        {
            if (String.IsNullOrWhiteSpace(portName)) { message = "پورت انتخاب نشده است."; return false; }
            try
            {
                using (var port = new SerialPort(portName.Trim(), baudRate, Parity.None, 8, StopBits.One))
                {
                    port.ReadTimeout = 500; port.WriteTimeout = 500; port.Open(); port.Close();
                }
                message = "پورت " + portName + " باز و بسته شد؛ دسترسی سیستم‌عامل تأیید شد.";
                Record("serial-open-close", portName, true, message);
                return true;
            }
            catch (Exception ex)
            {
                message = "دسترسی به " + portName + " ناموفق بود: " + ex.Message;
                Record("serial-open-close", portName, false, message);
                return false;
            }
        }

        public static bool TestPosSdk(out string message)
        {
            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "External", "SSP1126.PcPos.dll");
                if (!System.IO.File.Exists(path)) path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SSP1126.PcPos.dll");
                var assembly = System.Reflection.Assembly.LoadFrom(path);
                Type factory = assembly.GetType("SSP1126.PcPos.BaseClasses.PcPosFactory", false);
                if (factory == null) { message = "فایل SDK بار شد اما کلاس PcPosFactory پیدا نشد."; return false; }
                object instance = Activator.CreateInstance(factory);
                var setPort = factory.GetMethod("SetCom", new Type[] { typeof(string) });
                if (setPort != null) setPort.Invoke(instance, new object[] { AppSettings.Current.PosPort });
                var test = factory.GetMethod("ConnectionTest", Type.EmptyTypes);
                if (test == null) { message = "SDK شناسایی شد؛ نسخه‌ی آن متد تست بدون پارامتر ندارد، ارتباط واقعی تأیید نشد."; return false; }
                object result = test.Invoke(instance, null);
                message = "متد تست SDK اجرا شد. نتیجه: " + Convert.ToString(result);
                Record("pos-sdk-connection-test", AppSettings.Current.PosPort, true, message);
                return true;
            }
            catch (Exception ex)
            {
                message = "تست SDK/POS ناموفق بود: " + (ex.InnerException == null ? ex.Message : ex.InnerException.Message);
                Record("pos-sdk-connection-test", AppSettings.Current.PosPort, false, message);
                return false;
            }
        }

        private static void Record(string eventType, string port, bool success, string details)
        {
            try
            {
                using (var connection = Database.DatabaseManager.CreateDeviceConnection())
                {
                    connection.Open();
                    using (var command = new System.Data.SqlClient.SqlCommand("INSERT app.DeviceDiagnosticEvent(EventType,PortName,IsSuccess,Details) VALUES(@eventType,@portName,@isSuccess,@details)", connection))
                    {
                        command.Parameters.Add("@eventType", System.Data.SqlDbType.NVarChar, 80).Value = eventType;
                        command.Parameters.Add("@portName", System.Data.SqlDbType.NVarChar, 20).Value = (object)port ?? DBNull.Value;
                        command.Parameters.Add("@isSuccess", System.Data.SqlDbType.Bit).Value = success;
                        command.Parameters.Add("@details", System.Data.SqlDbType.NVarChar, 500).Value = details.Length > 500 ? details.Substring(0, 500) : details;
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }
    }
}
