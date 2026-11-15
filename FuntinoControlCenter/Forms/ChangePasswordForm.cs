using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using MessageBox = FuntinoControlCenter.Forms.AppMessageService;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.UI;

namespace FuntinoControlCenter.Forms
{
    public partial class ChangePasswordForm : Form
    {
        private bool isSaving;
        public ChangePasswordForm() { AppFontManager.EnsureVazirmatnLoaded(); InitializeComponent(); }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (isSaving) return;
            if (txtNewPassword.Text != txtConfirmPassword.Text) { MessageBox.Show("گذرواژه‌ی جدید و تکرار آن یکسان نیستند.", "بررسی گذرواژه", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtConfirmPassword.Focus(); return; }
            if (AppSession.Current == null) { MessageBox.Show("نشست کاربر معتبر نیست. دوباره وارد برنامه شوید.", "تغییر گذرواژه", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            int userId = AppSession.Current.ApplicationUserId;
            string currentPassword = txtCurrentPassword.Text;
            string newPassword = txtNewPassword.Text;
            isSaving = true;
            SetPasswordControlsEnabled(false);
            lblHint.Visible = false;
            loadingSave.BeginLoading("در حال ذخیره‌ی گذرواژه‌ی تازه…", Properties.Resources.IconPasswordSave);
            try
            {
                await Task.Yield();
                await Task.Run(delegate { DatabaseManager.ChangePassword(userId, currentPassword, newPassword); });
                if (IsDisposed || Disposing) return;
                AppSession.Current.CompletePasswordChange();
                DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex)
            {
                string message = ex is InvalidOperationException || ex is UnauthorizedAccessException
                    ? ex.Message
                    : "تغییر گذرواژه انجام نشد. اتصال برنامه را بررسی کنید و دوباره تلاش کنید.";
                if (!IsDisposed && !Disposing) MessageBox.Show(this, message, "تغییر گذرواژه انجام نشد", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isSaving = false;
                if (!IsDisposed && !Disposing)
                {
                    loadingSave.EndLoading();
                    lblHint.Visible = true;
                    SetPasswordControlsEnabled(true);
                }
            }
        }

        private void SetPasswordControlsEnabled(bool enabled)
        {
            txtCurrentPassword.Enabled = enabled;
            txtNewPassword.Enabled = enabled;
            txtConfirmPassword.Enabled = enabled;
            btnSave.Enabled = enabled;
            btnCancel.Enabled = enabled;
        }
    }
}
