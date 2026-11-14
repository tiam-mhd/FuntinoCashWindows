using System;
using System.Windows.Forms;

namespace FuntinoControlCenter.Security
{
    internal static class AuthorizationMonitor
    {
        public static void Attach(Form form, Func<bool> hasAccess)
        {
            if (form == null) throw new ArgumentNullException("form");
            bool revoked = false;
            var timer = new Timer { Interval = 3000 };

            Action closeForAuthorizationChange = delegate
            {
                if (revoked || form.IsDisposed) return;
                revoked = true;
                timer.Stop();
                if (form.Visible)
                    MessageBox.Show(form, "دسترسی‌های این نشست تغییر کرده‌اند. برای جلوگیری از نمایش اطلاعات قدیمی، این بخش بسته شد؛ در صورت نیاز دوباره وارد شوید.",
                        "به‌روزرسانی دسترسی", MessageBoxButtons.OK, MessageBoxIcon.Information);
                form.Close();
            };

            EventHandler check = delegate
            {
                if (revoked || form.IsDisposed) return;
                try
                {
                    if (AppSession.Current == null || !hasAccess()) closeForAuthorizationChange();
                }
                catch { closeForAuthorizationChange(); }
            };

            timer.Tick += check;
            Action changed = delegate
            {
                if (form.IsDisposed) return;
                try
                {
                    if (form.IsHandleCreated) form.BeginInvoke(closeForAuthorizationChange);
                    else closeForAuthorizationChange();
                }
                catch { closeForAuthorizationChange(); }
            };
            AppSession.AuthorizationStateChanged += changed;
            form.FormClosed += delegate
            {
                timer.Stop();
                timer.Dispose();
                AppSession.AuthorizationStateChanged -= changed;
            };
            timer.Start();
        }
    }
}
