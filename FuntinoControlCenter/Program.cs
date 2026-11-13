using System;
using System.Windows.Forms;
using FuntinoControlCenter.Forms;

namespace FuntinoControlCenter
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs e)
            {
                MessageBox.Show("خطای پیش‌بینی‌نشده در برنامه:\n" + e.Exception.Message, "خطای برنامه", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
            {
                var exception = e.ExceptionObject as Exception;
                if (exception != null) MessageBox.Show("خطای غیرقابل‌بازیابی:\n" + exception.Message, "خطای برنامه", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppSettings.Load();
            Application.Run(new LoginForm());
        }
    }
}
