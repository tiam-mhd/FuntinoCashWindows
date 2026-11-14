using System;
using System.Windows.Forms;

namespace FuntinoControlCenter.Forms
{
    internal partial class AppToastForm : Form
    {
        public AppToastForm(string title, string message, AppMessageKind kind)
        {
            InitializeComponent();
            lblTitle.Text = String.IsNullOrWhiteSpace(title) ? (kind == AppMessageKind.Success ? "انجام شد" : "اطلاع‌رسانی") : title;
            lblMessage.Text = message ?? String.Empty;
            System.Drawing.Bitmap source = kind == AppMessageKind.Success ? Properties.Resources.MessageSuccess : Properties.Resources.MessageInfo;
            if (source != null) picStatus.Image = new System.Drawing.Bitmap(source);
        }

        public void ShowToast(IWin32Window owner)
        {
            var control = owner as Control;
            Form parent = control == null ? null : control.FindForm();
            if (parent != null && parent.IsHandleCreated)
            {
                var location = parent.PointToScreen(new System.Drawing.Point(parent.ClientSize.Width - Width - 18, 18));
                System.Drawing.Rectangle workArea = Screen.FromControl(parent).WorkingArea;
                location.X = Math.Max(workArea.Left + 8, Math.Min(location.X, workArea.Right - Width - 8));
                location.Y = Math.Max(workArea.Top + 8, Math.Min(location.Y, workArea.Bottom - Height - 8));
                Location = location;
                Show(parent);
            }
            else
            {
                System.Drawing.Rectangle workArea = Screen.PrimaryScreen.WorkingArea;
                Location = new System.Drawing.Point(
                    workArea.Left + (workArea.Width - Width) / 2,
                    workArea.Top + (workArea.Height - Height) / 2);
                Show();
            }
        }

        private void AppToastForm_Shown(object sender, EventArgs e) { displayTimer.Start(); }
        private void DisplayTimer_Tick(object sender, EventArgs e) { displayTimer.Stop(); Close(); Dispose(); }
        private void BtnDismiss_Click(object sender, EventArgs e) { displayTimer.Stop(); Close(); Dispose(); }
    }
}
