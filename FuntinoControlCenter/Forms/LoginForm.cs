using System;
using System.Windows.Forms;
using MessageBox = FuntinoControlCenter.Forms.AppMessageService;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.UI;

namespace FuntinoControlCenter.Forms
{
    public partial class LoginForm : Form
    {
        private bool ready;
        public LoginForm() { AppFontManager.EnsureVazirmatnLoaded(); InitializeComponent(); }

        private void BtnExit_Click(object sender, EventArgs e) { Close(); }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "در حال آماده‌سازی اطلاعات برنامه…";
            try
            {
                SchemaMigrator.EnsureAll(); ready = true;
                lblStatus.Text = "برنامه آماده است. نام کاربری و گذرواژه را وارد کنید.";
                lblStatus.ForeColor = System.Drawing.Color.FromArgb(95, 196, 153);
                btnLogin.Enabled = true; txtPassword.Focus();
                string initialAdminPassword = SchemaMigrator.TakeInitialAdminPassword();
                if (!String.IsNullOrEmpty(initialAdminPassword))
                    using (var credentials = new InitialCredentialDialog("Admin", initialAdminPassword))
                        credentials.ShowDialog(this);
            }
            catch (Exception)
            {
                ready = false; btnLogin.Enabled = false;
                lblStatus.Text = "اتصال به اطلاعات برنامه برقرار نشد. تنظیم ارتباط را بررسی کنید.";
                lblStatus.ForeColor = System.Drawing.Color.FromArgb(230, 114, 104);
                MessageBox.Show("برنامه نتوانست اطلاعات را آماده کند. تنظیم ارتباط را بررسی کنید و دوباره تلاش کنید.", "آماده‌سازی برنامه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            using (var form = new SettingsForm(true)) form.ShowDialog(this);
            LoginForm_Load(this, EventArgs.Empty);
        }

        private void Password_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter && btnLogin.Enabled) { e.SuppressKeyPress = true; BtnLogin_Click(sender, EventArgs.Empty); } }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (!ready) return;
            try
            {
                var session = UserRepository.Authenticate(txtUsername.Text, txtPassword.Text);
                if (session == null) { lblStatus.Text = "نام کاربری یا گذرواژه نادرست است."; lblStatus.ForeColor = System.Drawing.Color.FromArgb(230, 114, 104); txtPassword.SelectAll(); txtPassword.Focus(); return; }
                if (session.MustChangePassword)
                {
                    using (var change = new ChangePasswordForm())
                    {
                        if (change.ShowDialog(this) != DialogResult.OK) { AppSession.Clear(); lblStatus.Text = "برای ادامه، ابتدا گذرواژه‌ی اولیه را تغییر دهید."; return; }
                    }
                }
                Hide();
                using (var main = new MainForm()) main.ShowDialog();
                AppSession.Clear(); txtPassword.Clear(); Show(); Activate();
            }
            catch (Exception) { MessageBox.Show("ورود انجام نشد. اتصال برنامه را بررسی کنید و دوباره تلاش کنید.", "ورود ناموفق", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
