using System;
using System.Windows.Forms;
using MessageBox = FuntinoControlCenter.Forms.AppMessageService;
using FuntinoControlCenter.UI;

namespace FuntinoControlCenter.Forms
{
    internal sealed partial class InitialCredentialDialog : Form
    {
        public InitialCredentialDialog(string userName, string password)
        {
            AppFontManager.EnsureVazirmatnLoaded();
            InitializeComponent();
            lblUserName.Text = "نام کاربری این حساب: " + (userName ?? "");
            txtPassword.Text = password ?? "";
        }

        private void InitialCredentialDialog_Shown(object sender, EventArgs e) { txtPassword.SelectAll(); }

        private void CopyPassword(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(txtPassword.Text);
                lblCopyStatus.Text = "گذرواژه کپی شد.";
            }
            catch (Exception)
            {
                lblCopyStatus.Text = "کپی انجام نشد؛ رمز را از کادر انتخاب کنید.";
                MessageBox.Show(this, "اگر کپی خودکار انجام نشد، رمز را انتخاب کنید و از میانبر صفحه‌کلید برای کپی آن استفاده کنید.",
                    "راهنمای کپی گذرواژه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.SelectAll();
            }
        }
    }
}
