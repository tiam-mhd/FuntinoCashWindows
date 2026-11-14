using System;
using System.Windows.Forms;

namespace FuntinoControlCenter.Forms
{
    internal partial class AppMessageDialog : Form
    {
        private DialogResult primaryResult = DialogResult.OK;
        private DialogResult secondaryResult = DialogResult.Cancel;
        private DialogResult tertiaryResult = DialogResult.Abort;
        private int defaultButtonIndex = 1;

        public AppMessageDialog(string caption, string message, AppMessageKind kind, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton)
        {
            InitializeComponent();
            lblWindowTitle.Text = String.IsNullOrWhiteSpace(caption) ? "پیام سامانه" : caption;
            lblSeverity.Text = SeverityCaption(kind);
            txtMessage.Text = message ?? String.Empty;
            SetStatusImage(kind);
            ConfigureButtons(buttons);
            defaultButtonIndex = defaultButton == MessageBoxDefaultButton.Button3 ? 3 :
                (defaultButton == MessageBoxDefaultButton.Button2 ? 2 : 1);
        }

        private static string SeverityCaption(AppMessageKind kind)
        {
            switch (kind)
            {
                case AppMessageKind.Error: return "عملیات انجام نشد";
                case AppMessageKind.Warning: return "نیاز به بررسی";
                case AppMessageKind.Success: return "با موفقیت انجام شد";
                case AppMessageKind.Confirmation: return "تأیید انجام کار";
                default: return "اطلاع‌رسانی";
            }
        }

        private void SetStatusImage(AppMessageKind kind)
        {
            System.Drawing.Bitmap source;
            switch (kind)
            {
                case AppMessageKind.Error: source = Properties.Resources.MessageError; break;
                case AppMessageKind.Warning: source = Properties.Resources.MessageWarning; break;
                case AppMessageKind.Success: source = Properties.Resources.MessageSuccess; break;
                default: source = Properties.Resources.MessageInfo; break;
            }
            if (source != null) picStatus.Image = new System.Drawing.Bitmap(source);
        }

        private void ConfigureButtons(MessageBoxButtons buttons)
        {
            btnPrimary.Visible = true;
            btnSecondary.Visible = false;
            btnTertiary.Visible = false;
            btnPrimary.Text = "متوجه شدم";
            primaryResult = DialogResult.OK;
            secondaryResult = DialogResult.Cancel;
            tertiaryResult = DialogResult.Abort;

            switch (buttons)
            {
                case MessageBoxButtons.OKCancel:
                    btnPrimary.Text = "تأیید";
                    btnSecondary.Text = "انصراف";
                    btnSecondary.Visible = true;
                    primaryResult = DialogResult.OK;
                    secondaryResult = DialogResult.Cancel;
                    break;
                case MessageBoxButtons.YesNo:
                    btnPrimary.Text = "بله";
                    btnSecondary.Text = "خیر";
                    btnSecondary.Visible = true;
                    primaryResult = DialogResult.Yes;
                    secondaryResult = DialogResult.No;
                    break;
                case MessageBoxButtons.YesNoCancel:
                    btnPrimary.Text = "بله";
                    btnSecondary.Text = "خیر";
                    btnTertiary.Text = "انصراف";
                    btnSecondary.Visible = true;
                    btnTertiary.Visible = true;
                    primaryResult = DialogResult.Yes;
                    secondaryResult = DialogResult.No;
                    tertiaryResult = DialogResult.Cancel;
                    break;
                case MessageBoxButtons.RetryCancel:
                    btnPrimary.Text = "تلاش دوباره";
                    btnSecondary.Text = "انصراف";
                    btnSecondary.Visible = true;
                    primaryResult = DialogResult.Retry;
                    secondaryResult = DialogResult.Cancel;
                    break;
                case MessageBoxButtons.AbortRetryIgnore:
                    btnPrimary.Text = "توقف";
                    btnSecondary.Text = "تلاش دوباره";
                    btnTertiary.Text = "نادیده گرفتن";
                    btnSecondary.Visible = true;
                    btnTertiary.Visible = true;
                    primaryResult = DialogResult.Abort;
                    secondaryResult = DialogResult.Retry;
                    tertiaryResult = DialogResult.Ignore;
                    break;
            }

            if (buttons == MessageBoxButtons.YesNoCancel && btnTertiary.Visible)
                CancelButton = btnTertiary;
            else
                CancelButton = btnSecondary.Visible ? (IButtonControl)btnSecondary : btnPrimary;
        }

        private void AppMessageDialog_Load(object sender, EventArgs e)
        {
            if (defaultButtonIndex == 3 && btnTertiary.Visible) ActiveControl = btnTertiary;
            else if (defaultButtonIndex == 2 && btnSecondary.Visible) ActiveControl = btnSecondary;
            else ActiveControl = btnPrimary;
            AcceptButton = ActiveControl as IButtonControl;
        }

        private void BtnPrimary_Click(object sender, EventArgs e) { DialogResult = primaryResult; Close(); }
        private void BtnSecondary_Click(object sender, EventArgs e) { DialogResult = secondaryResult; Close(); }
        private void BtnTertiary_Click(object sender, EventArgs e) { DialogResult = tertiaryResult; Close(); }
    }
}
