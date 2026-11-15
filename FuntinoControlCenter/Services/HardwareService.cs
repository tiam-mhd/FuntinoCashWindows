using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using Microsoft.Win32;
using System.Reflection;
using FuntinoControlCenter.Security;

namespace FuntinoControlCenter.Services
{
    internal static class HardwareService
    {
        private sealed class UsbSerialBridgeStatus
        {
            public string Name { get; set; }
            public string PortName { get; set; }
            public int ConfigurationErrorCode { get; set; }
        }

        public static string[] GetPortNames()
        {
            AuthorizationGuard.RequireAny("settings.hardware.view", "settings.hardware.update", "devices.test");
            try { return SerialPort.GetPortNames().OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray(); }
            catch { return new string[0]; }
        }

        public static IList<string> DetectReaderPorts(int baudRate, out string message)
        {
            AuthorizationGuard.Require("devices.test");
            var successfulPorts = new List<string>();
            string[] ports = SerialPort.GetPortNames().OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray();
            UsbSerialBridgeStatus usbBridge = FindCp210xBridge();
            var cp210xPorts = FindReadyCp210xPorts(ports);
            foreach (string port in ports)
            {
                if (cp210xPorts.Contains(port))
                {
                    successfulPorts.Add(port);
                    continue;
                }

                string result;
                if (TestPort(port, baudRate, out result)) successfulPorts.Add(port);
            }

            if (successfulPorts.Count == 0)
                message = BuildReaderDetectionFailureMessage(ports, usbBridge, baudRate);
            else if (successfulPorts.Any(port => cp210xPorts.Contains(port)))
                message = "کارت‌خوان از روی اتصال به رایانه شناسایی شد. برای بررسی عملکرد، کارت را روی دستگاه بگذارید و یکی از عملیات‌ها را انجام دهید.";
            else
                message = "کارت‌خوان پاسخ می‌دهد و آماده‌ی استفاده است.";
            return successfulPorts;
        }

        private static HashSet<string> FindReadyCp210xPorts(string[] availablePorts)
        {
            var serialPorts = new HashSet<string>(availablePorts, StringComparer.OrdinalIgnoreCase);
            var recognizedPorts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (UsbSerialBridgeStatus bridge in FindCp210xBridges())
            {
                if (bridge.ConfigurationErrorCode == 0 && !String.IsNullOrWhiteSpace(bridge.PortName) && serialPorts.Contains(bridge.PortName))
                    recognizedPorts.Add(bridge.PortName);
            }
            return recognizedPorts;
        }

        private static string BuildReaderDetectionFailureMessage(string[] serialPorts, UsbSerialBridgeStatus usbBridge, int baudRate)
        {
            if (usbBridge != null && usbBridge.ConfigurationErrorCode == 28)
                return "کارت‌خوان پیدا شد، اما آماده‌ی استفاده نیست. برای راه‌اندازی دستگاه از مسئول سامانه کمک بگیرید.";

            if (usbBridge != null && usbBridge.ConfigurationErrorCode != 0)
                return "کارت‌خوان پیدا شد، اما آماده‌ی استفاده نیست. دستگاه را جدا و دوباره وصل کنید؛ اگر مشکل ادامه داشت، از مسئول سامانه کمک بگیرید.";

            if (serialPorts.Length == 0 && usbBridge != null)
                return "کارت‌خوان به رایانه وصل است، اما هنوز آماده‌ی استفاده نیست. دستگاه را جدا و دوباره وصل کنید؛ اگر مشکل ادامه داشت، از مسئول سامانه کمک بگیرید.";

            if (serialPorts.Length == 0)
                return "کارت‌خوان پیدا نشد. اتصال دستگاه را بررسی کنید و دوباره تلاش کنید.";

            return "کارت‌خوان پیدا شد، اما پاسخ نمی‌دهد. مطمئن شوید دستگاه روشن و به رایانه وصل است؛ سپس دوباره تلاش کنید.";
        }

        private static IList<UsbSerialBridgeStatus> FindCp210xBridges()
        {
            var bridges = new List<UsbSerialBridgeStatus>();
            try
            {
                using (RegistryKey usbDevices = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB", false))
                {
                    if (usbDevices == null) return bridges;
                    foreach (string vendorDeviceName in usbDevices.GetSubKeyNames())
                    {
                        if (!vendorDeviceName.StartsWith("VID_10C4&PID_", StringComparison.OrdinalIgnoreCase)) continue;
                        using (RegistryKey vendorDevice = usbDevices.OpenSubKey(vendorDeviceName, false))
                        {
                            if (vendorDevice == null) continue;
                            foreach (string instanceName in vendorDevice.GetSubKeyNames())
                            {
                                using (RegistryKey instance = vendorDevice.OpenSubKey(instanceName, false))
                                {
                                    if (instance == null) continue;
                                    string name = Convert.ToString(instance.GetValue("FriendlyName", null));
                                    if (String.IsNullOrWhiteSpace(name)) name = Convert.ToString(instance.GetValue("DeviceDesc", null));
                                    if (String.IsNullOrWhiteSpace(name)) name = "مبدل USB به سریال CP210x";
                                    if (name.IndexOf("CP210", StringComparison.OrdinalIgnoreCase) < 0 &&
                                        name.IndexOf("USB to UART Bridge", StringComparison.OrdinalIgnoreCase) < 0) continue;

                                    string portName = null;
                                    using (RegistryKey parameters = instance.OpenSubKey("Device Parameters", false))
                                        if (parameters != null) portName = Convert.ToString(parameters.GetValue("PortName", null));

                                    int errorCode = 0;
                                    object rawFlags = instance.GetValue("ConfigFlags", null);
                                    if (rawFlags != null && (Convert.ToInt32(rawFlags, System.Globalization.CultureInfo.InvariantCulture) & 0x40) != 0)
                                        errorCode = 28;
                                    bridges.Add(new UsbSerialBridgeStatus { Name = name, PortName = portName, ConfigurationErrorCode = errorCode });
                                }
                            }
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (System.Security.SecurityException) { }
            catch (IOException) { }
            return bridges;
        }

        private static UsbSerialBridgeStatus FindCp210xBridge()
        {
            return FindCp210xBridges().FirstOrDefault();
        }

        public static IList<string> DetectPosPorts(out string message)
        {
            AuthorizationGuard.Require("devices.test");
            string sdkPath = FindPosSdkPath();
            if (sdkPath == null)
            {
                message = "فایل ارتباطی پایانه‌ی فروش در پوشه‌ی برنامه پیدا نشد؛ تا زمان نصب SDK هیچ پورتی تأیید نمی‌شود.";
                return new List<string>();
            }

            string[] ports = SerialPort.GetPortNames().OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray();
            var successfulPorts = new List<string>();
            foreach (string port in ports)
            {
                string result;
                if (TestPosSdk(port, sdkPath, out result)) successfulPorts.Add(port);
            }

            message = successfulPorts.Count == 0
                ? (ports.Length == 0 ? "هیچ درگاه سریالی در ویندوز پیدا نشد." : "هیچ پایانه‌ی فروش پاسخ موفق نداد. اتصال SDK، سیم و روشن بودن پایانه را بررسی کنید.")
                : "فقط درگاه‌هایی نمایش داده می‌شوند که SDK پایانه کد پاسخ موفق ۰۰ را برگرداند.";
            return successfulPorts;
        }

        public static bool TestPort(string portName, int baudRate, out string message)
        {
            AuthorizationGuard.Require("devices.test");
            if (String.IsNullOrWhiteSpace(portName)) { message = "کارت‌خوان انتخاب نشده است. آن را از فهرست دستگاه‌ها انتخاب کنید."; return false; }
            bool success = CardReaderService.TestReaderPort(portName, baudRate, out message);
            Record("reader-protocol-handshake", portName, success, message);
            return success;
        }

        public static bool TestPosSdk(out string message)
        {
            AuthorizationGuard.Require("devices.test");
            return TestPosSdk(AppSettings.Current.PosPort, out message);
        }

        public static bool TestPosSdk(string portName, out string message)
        {
            AuthorizationGuard.Require("devices.test");
            string sdkPath = FindPosSdkPath();
            if (sdkPath == null)
            {
                message = "فایل ارتباطی پایانه‌ی فروش کنار برنامه پیدا نشد.";
                Record("pos-sdk-connection-test", portName, false, message);
                return false;
            }

            return TestPosSdk(portName, sdkPath, out message);
        }

        private static bool TestPosSdk(string portName, string sdkPath, out string message)
        {
            AuthorizationGuard.Require("devices.test");
            if (String.IsNullOrWhiteSpace(portName))
            {
                message = "درگاه پایانه‌ی فروش انتخاب نشده است.";
                Record("pos-sdk-connection-test", portName, false, message);
                return false;
            }

            object instance = null;
            try
            {
                Assembly assembly = Assembly.LoadFrom(sdkPath);
                Type factoryType = assembly.GetType("SSP1126.PcPos.BaseClasses.PcPosFactory", false);
                if (factoryType == null) throw new InvalidOperationException("کلاس ارتباطی پایانه در فایل SDK پیدا نشد.");
                instance = Activator.CreateInstance(factoryType);

                MethodInfo setCom = factoryType.GetMethod("SetCom", new[] { typeof(string) });
                if (setCom == null) throw new InvalidOperationException("روش انتخاب درگاه در فایل SDK پیدا نشد.");
                object selected = setCom.Invoke(instance, new object[] { portName.Trim() });
                if (selected is bool && !(bool)selected) throw new InvalidOperationException("SDK درگاه را نپذیرفت.");

                MethodInfo initialize = factoryType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(method => method.Name == "Initialization" && HasReferenceSignature(method));
                if (initialize == null) throw new InvalidOperationException("روش آماده‌سازی ارتباط در فایل SDK پیدا نشد.");
                ParameterInfo[] parameters = initialize.GetParameters();
                object language = Enum.ToObject(parameters[0].ParameterType, 0);
                object asyncType = Enum.ToObject(parameters[2].ParameterType, 0);
                initialize.Invoke(instance, new[] { language, (object)3, asyncType });

                MethodInfo connectionTest = factoryType.GetMethod("ConnectionTest", Type.EmptyTypes);
                if (connectionTest == null) throw new InvalidOperationException("روش آزمایش ارتباط در فایل SDK پیدا نشد.");
                object result = connectionTest.Invoke(instance, null);
                PropertyInfo responseCode = result == null ? null : result.GetType().GetProperty("ResponseCode");
                string code = responseCode == null ? null : Convert.ToString(responseCode.GetValue(result, null));
                bool success = String.Equals(code, "00", StringComparison.Ordinal);
                message = success
                    ? "پایانه‌ی فروش روی " + portName + " پاسخ موفق داد."
                    : "پاسخ پایانه روی " + portName + " موفق نبود" + (String.IsNullOrWhiteSpace(code) ? "." : " (کد " + code + ").");
                Record("pos-sdk-connection-test", portName, success, message);
                return success;
            }
            catch (Exception ex)
            {
                Exception cause = ex is TargetInvocationException && ex.InnerException != null ? ex.InnerException : ex;
                message = "آزمایش پایانه‌ی فروش روی " + portName + " ناموفق بود: " + FriendlyHardwareError(cause);
                Record("pos-sdk-connection-test", portName, false, message);
                return false;
            }
            finally
            {
                IDisposable disposable = instance as IDisposable;
                if (disposable != null)
                {
                    try { disposable.Dispose(); }
                    catch { }
                }
            }
        }

        private static bool HasReferenceSignature(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            return parameters.Length == 3 && parameters[0].ParameterType.IsEnum &&
                parameters[1].ParameterType == typeof(int) && parameters[2].ParameterType.IsEnum;
        }

        private static string FindPosSdkPath()
        {
            string directory = AppDomain.CurrentDomain.BaseDirectory;
            string[] candidates =
            {
                Path.Combine(directory, "External", "SSP1126.PcPos.dll"),
                Path.Combine(directory, "SSP1126.PcPos.dll")
            };
            return candidates.FirstOrDefault(File.Exists);
        }

        private static string FriendlyHardwareError(Exception exception)
        {
            if (exception is UnauthorizedAccessException || exception is IOException)
                return "پورت در برنامه‌ی دیگری باز است یا دسترسی ویندوز به آن ممکن نیست.";
            if (exception is TimeoutException)
                return "پایانه در زمان تعیین‌شده پاسخ نداد.";
            return String.IsNullOrWhiteSpace(exception.Message) ? "خطای ناشناخته" : exception.Message;
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
