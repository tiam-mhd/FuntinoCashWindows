using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using MessageBox = FuntinoControlCenter.Forms.AppMessageService;
using FuntinoControlCenter.Database;
using FuntinoControlCenter.UI;

namespace FuntinoControlCenter.Forms
{
    public partial class LoginForm : Form
    {
        private bool ready;
        private bool initializing;
        private bool initializeAgain;
        private bool authenticating;
        public LoginForm() { AppFontManager.EnsureVazirmatnLoaded(); InitializeComponent(); }

        private void BtnExit_Click(object sender, EventArgs e) { Close(); }

        private async void LoginForm_Load(object sender, EventArgs e)
        {
            await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            if (initializing)
            {
                initializeAgain = true;
                return;
            }

            initializing = true;
            ready = false;
            SetAuthenticationControlsEnabled(false, false);
            lblStatus.Visible = false;
            loadingStatus.BeginLoading("در حال آماده‌سازی اطلاعات برنامه…", Properties.Resources.IconSignIn);
            try
            {
                await Task.Yield();
                await Task.Run(delegate { SchemaMigrator.EnsureAll(); });
                if (IsDisposed || Disposing) return;

                ready = true;
                loadingStatus.EndLoading();
                lblStatus.Visible = true;
                lblStatus.Text = "برنامه آماده است. نام کاربری و گذرواژه را وارد کنید.";
                lblStatus.ForeColor = System.Drawing.Color.FromArgb(95, 196, 153);
                SetAuthenticationControlsEnabled(true, true);
                txtPassword.Focus();
                string initialAdminPassword = SchemaMigrator.TakeInitialAdminPassword();
                if (!String.IsNullOrEmpty(initialAdminPassword))
                    using (var credentials = new InitialCredentialDialog("Admin", initialAdminPassword))
                        credentials.ShowDialog(this);
            }
            catch (Exception)
            {
                if (IsDisposed || Disposing) return;
                ready = false;
                loadingStatus.EndLoading();
                lblStatus.Visible = true;
                SetAuthenticationControlsEnabled(false, true);
                lblStatus.Text = "اتصال به اطلاعات برنامه برقرار نشد. تنظیم ارتباط را بررسی کنید.";
                lblStatus.ForeColor = System.Drawing.Color.FromArgb(230, 114, 104);
                MessageBox.Show("برنامه نتوانست اطلاعات را آماده کند. تنظیم ارتباط را بررسی کنید و دوباره تلاش کنید.", "آماده‌سازی برنامه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                initializing = false;
                if (initializeAgain && !IsDisposed && !Disposing)
                {
                    initializeAgain = false;
                    await InitializeAsync();
                }
            }
        }

        private async void BtnSettings_Click(object sender, EventArgs e)
        {
            using (var form = new SettingsForm(true)) form.ShowDialog(this);
            await InitializeAsync();
        }

        private void Password_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter && btnLogin.Enabled) { e.SuppressKeyPress = true; BtnLogin_Click(sender, EventArgs.Empty); } }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            if (!ready || authenticating) return;
            authenticating = true;
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            SetAuthenticationControlsEnabled(false, false);
            lblStatus.Visible = false;
            loadingStatus.BeginLoading("در حال بررسی نام کاربری و گذرواژه…", Properties.Resources.IconSignIn);
            try
            {
                await Task.Yield();
                var session = await Task.Run(delegate { return UserRepository.Authenticate(username, password); });
                if (IsDisposed || Disposing)
                {
                    if (session != null) AppSession.Clear();
                    return;
                }
                loadingStatus.EndLoading();
                lblStatus.Visible = true;
                if (session == null)
                {
                    lblStatus.Text = "نام کاربری یا گذرواژه درست نیست. دوباره بررسی کنید.";
                    lblStatus.ForeColor = System.Drawing.Color.FromArgb(230, 114, 104);
                    txtPassword.SelectAll();
                    txtPassword.Focus();
                    return;
                }
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
            catch (Exception)
            {
                if (!IsDisposed && !Disposing)
                    MessageBox.Show("ورود انجام نشد. ارتباط برنامه را بررسی کنید و دوباره تلاش کنید.", "ورود ناموفق", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                authenticating = false;
                if (!IsDisposed && !Disposing)
                {
                    loadingStatus.EndLoading();
                    lblStatus.Visible = true;
                    SetAuthenticationControlsEnabled(ready, true);
                }
            }
        }

        private void SetAuthenticationControlsEnabled(bool enabled, bool allowSettings)
        {
            txtUsername.Enabled = enabled;
            txtPassword.Enabled = enabled;
            btnLogin.Enabled = enabled && ready;
            btnSettings.Enabled = allowSettings;
        }
    }
}
