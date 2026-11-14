using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using FuntinoControlCenter.Security;

namespace FuntinoControlCenter.Services
{
    internal static class HardwareService
    {
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
            foreach (string port in ports)
            {
                string result;
                if (TestPort(port, baudRate, out result)) successfulPorts.Add(port);
            }

            if (successfulPorts.Count == 0)
                message = ports.Length == 0
                    ? "هیچ درگاه سریالی در ویندوز پیدا نشد. اتصال کارت‌خوان را بررسی کنید."
                    : "هیچ کارت‌خوانی پاسخ معتبر نداد. یک کارت را روی دستگاه بگذارید و دوباره پویش کنید؛ اگر کارت حاضر است، اتصال و سرعت ارتباط را بررسی کنید.";
            else
                message = "فقط درگاه‌هایی نمایش داده می‌شوند که به درخواست شناسایی کارت‌خوان پاسخ معتبر دادند. برای این آزمون، کارت باید روی دستگاه باشد؛ کارت در زمان بیکاری پایش نمی‌شود.";
            return successfulPorts;
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
            if (String.IsNullOrWhiteSpace(portName)) { message = "درگاه انتخاب نشده است."; return false; }
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
