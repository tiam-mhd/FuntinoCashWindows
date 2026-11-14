using System;
using System.Windows.Forms;
using FuntinoControlCenter.Database;

namespace FuntinoControlCenter.Forms
{
    public partial class LoginForm : Form
    {
        private bool ready;
        public LoginForm() { InitializeComponent(); Load += LoginForm_Load; btnLogin.Click += BtnLogin_Click; btnSettings.Click += BtnSettings_Click; btnExit.Click += delegate { Close(); }; txtPassword.KeyDown += Password_KeyDown; }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "در حال بررسی پایگاه‌های داده و اجرای به‌روزرسانی‌های امن…";
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(232, 174, 88);
            try
            {
                SchemaMigrator.EnsureAll(); ready = true;
                lblStatus.Text = "پایگاه‌های Funtino آماده‌اند. ورود اولیه: Admin";
                lblStatus.ForeColor = System.Drawing.Color.FromArgb(95, 196, 153);
                btnLogin.Enabled = true; txtPassword.Focus();
                if (!String.IsNullOrEmpty(SchemaMigrator.InitialAdminPassword))
                    MessageBox.Show(this, "حساب اولیه ساخته شد.\nنام کاربری: Admin\nگذرواژه‌ی یک‌بارمصرف:\n" + SchemaMigrator.InitialAdminPassword + "\n\nاین گذرواژه فقط همین بار نمایش داده می‌شود. پس از ورود، آن را تغییر دهید.", "راه‌اندازی حساب مدیر", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ready = false; btnLogin.Enabled = false;
                lblStatus.Text = "پایگاه آماده نیست. اجرای سرویس SQL و وجود هر دو پایگاه Funtino را بررسی کنید.";
                lblStatus.ForeColor = System.Drawing.Color.FromArgb(230, 114, 104);
                MessageBox.Show(DatabaseManager.SafeMessage(ex), "راه‌اندازی پایگاه داده", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            using (var form = new SettingsForm()) form.ShowDialog(this);
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
            catch (Exception ex) { MessageBox.Show(DatabaseManager.SafeMessage(ex), "ورود ناموفق", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
