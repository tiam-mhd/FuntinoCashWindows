using System;
using System.Windows.Forms;

namespace FuntinoControlCenter.Forms
{
    internal sealed partial class InitialCredentialDialog : Form
    {
        public InitialCredentialDialog(string userName, string password)
        {
            InitializeComponent();
            lblUserName.Text = "نام کاربری: " + (userName ?? "");
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
            catch (Exception ex)
            {
                lblCopyStatus.Text = "کپی نشد؛ گذرواژه را انتخاب کنید.";
                MessageBox.Show(this, "دسترسی به کلیپ‌بورد انجام نشد. گذرواژه را از کادر انتخاب و کپی کنید.\n" + ex.Message,
                    "کپی گذرواژه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.SelectAll();
            }
        }
    }
}
