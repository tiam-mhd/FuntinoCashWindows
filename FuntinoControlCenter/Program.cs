using System;
using System.Windows.Forms;
using MessageBox = FuntinoControlCenter.Forms.AppMessageService;
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
                MessageBox.Show("برنامه با مشکلی روبه‌رو شد و نتوانست این کار را انجام دهد. برنامه را ببندید و دوباره باز کنید؛ اگر مشکل ادامه داشت، با پشتیبان سامانه تماس بگیرید.", "خطای برنامه", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
            {
                if (e.ExceptionObject is Exception) MessageBox.Show("برنامه با مشکلی جدی روبه‌رو شد و باید بسته شود. برنامه را دوباره باز کنید؛ اگر مشکل ادامه داشت، با پشتیبان سامانه تماس بگیرید.", "خطای برنامه", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppSettings.Load();
            Application.Run(new LoginForm());
        }
    }
}
