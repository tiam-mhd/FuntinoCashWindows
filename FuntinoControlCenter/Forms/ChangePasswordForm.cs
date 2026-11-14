using System;
using System.Windows.Forms;
using FuntinoControlCenter.Database;

namespace FuntinoControlCenter.Forms
{
    public partial class ChangePasswordForm : Form
    {
        public ChangePasswordForm() { InitializeComponent(); }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (txtNewPassword.Text != txtConfirmPassword.Text) { MessageBox.Show("گذرواژه‌ی جدید و تکرار آن یکسان نیستند.", "بررسی گذرواژه", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtConfirmPassword.Focus(); return; }
            try
            {
                if (AppSession.Current == null) throw new InvalidOperationException("نشست کاربر معتبر نیست.");
                DatabaseManager.ChangePassword(AppSession.Current.ApplicationUserId, txtCurrentPassword.Text, txtNewPassword.Text);
                AppSession.Current.CompletePasswordChange();
                DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "ذخیره گذرواژه ناموفق", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
