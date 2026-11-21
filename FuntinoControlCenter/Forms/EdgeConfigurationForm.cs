using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using FuntinoControlCenter.Security;
using FuntinoControlCenter.Services;
using MessageBox = FuntinoControlCenter.Forms.AppMessageService;

namespace FuntinoControlCenter.Forms
{
    public partial class EdgeConfigurationForm : Form
    {
        public EdgeConfigurationForm()
        {
            InitializeComponent();
        }

        private void EdgeConfigurationForm_Load(object sender, EventArgs e)
        {
            try
            {
                AuthorizationGuard.Require("edge.configuration.manage");
                AppSettings settings = AppSettings.Current;
                txtBaseUrl.Text = settings.EdgeApiBaseUrl;
                txtCredential.Text = settings.EdgeCredentialId;
                txtOrganization.Text = settings.EdgeOrganizationId;
                txtBranch.Text = settings.EdgeBranchId;
                lblSecretState.Text = settings.EdgeSecretConfigured
                    ? "کلید HMAC برای این حساب ویندوز ذخیره شده است؛ برای چرخش، کلید تازه را وارد کنید."
                    : "کلید HMAC هنوز برای این حساب ویندوز تنظیم نشده است.";
                lblSourceInstance.Text = "شناسهٔ پایدار این نصب: " + settings.EdgeSourceInstanceId;
                AuthorizationMonitor.Attach(this, delegate { return AuthorizationGuard.Has("edge.configuration.manage"); });
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.ShowWarning(this, "مجوز تنظیم ارتباط فانتینو برای این حساب فعال نیست.", "دسترسی محدود است");
                Close();
            }
        }

        private async void Save_Click(object sender, EventArgs e)
        {
            string baseUrl = txtBaseUrl.Text;
            string credentialId = txtCredential.Text;
            string organizationId = txtOrganization.Text;
            string branchId = txtBranch.Text;
            string secret = txtSecret.Text;
            try
            {
                AuthorizationGuard.Require("edge.configuration.manage");
                SetSaving(true);
                await Task.Run(delegate
                {
                    EdgeConfigurationService.Save(baseUrl, credentialId, organizationId, branchId, secret);
                });
                if (IsDisposed || Disposing) return;
                txtSecret.Clear();
                lblSecretState.Text = "تنظیمات ذخیره شد. کلید در فایل پیکربندی به‌صورت رمز‌شده نگه‌داری می‌شود.";
                MessageBox.ShowSuccess(this, "تنظیمات ارتباط این شعبه ذخیره شد. worker همگام‌سازی آن‌ها را در چرخهٔ بعدی می‌خواند.", "تنظیمات ذخیره شد");
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.ShowWarning(this, "مجوز تنظیم ارتباط تغییر کرده است؛ تنظیمات ذخیره نشد.", "دسترسی محدود است");
            }
            catch (ArgumentException exception)
            {
                MessageBox.ShowWarning(this, exception.Message, "بررسی تنظیمات");
            }
            catch (InvalidOperationException exception)
            {
                if (!IsDisposed && !Disposing)
                    MessageBox.ShowWarning(this, exception.Message, "تنظیمات ذخیره نشد");
            }
            catch (Exception)
            {
                if (!IsDisposed && !Disposing)
                    MessageBox.ShowError(this, "تنظیمات ذخیره نشد. ارتباط پایگاه عملیاتی و دسترسی پوشهٔ تنظیمات را بررسی کنید.", "ذخیرهٔ تنظیمات");
            }
            finally
            {
                secret = null;
                if (!IsDisposed && !Disposing)
                {
                    txtSecret.Clear();
                    SetSaving(false);
                }
            }
        }

        private void SetSaving(bool saving)
        {
            btnSave.Enabled = !saving;
            btnBack.Enabled = !saving;
            txtBaseUrl.Enabled = !saving;
            txtCredential.Enabled = !saving;
            txtOrganization.Enabled = !saving;
            txtBranch.Enabled = !saving;
            txtSecret.Enabled = !saving;
            lblBusy.Visible = saving;
            progressBusy.Visible = saving;
        }

        private void Back_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
