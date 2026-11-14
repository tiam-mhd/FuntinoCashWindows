namespace FuntinoControlCenter.Forms
{
    partial class AppToastForm
    {
        private System.ComponentModel.IContainer components = null;
        private FuntinoControlCenter.UI.RoundedBorderPanel pnlToast;
        private System.Windows.Forms.PictureBox picStatus;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblMessage;
        private FuntinoControlCenter.UI.RoundedButton btnDismiss;
        private System.Windows.Forms.Timer displayTimer;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null) components.Dispose();
                if (picStatus != null && picStatus.Image != null) picStatus.Image.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlToast = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.picStatus = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblMessage = new System.Windows.Forms.Label();
            this.btnDismiss = new FuntinoControlCenter.UI.RoundedButton();
            this.displayTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.picStatus)).BeginInit();
            this.pnlToast.SuspendLayout();
            this.SuspendLayout();

            this.pnlToast.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            this.pnlToast.BorderColor = System.Drawing.Color.FromArgb(194, 215, 205);
            this.pnlToast.BorderSize = 1;
            this.pnlToast.CornerRadius = 16;
            this.pnlToast.Controls.Add(this.picStatus);
            this.pnlToast.Controls.Add(this.lblTitle);
            this.pnlToast.Controls.Add(this.lblMessage);
            this.pnlToast.Controls.Add(this.btnDismiss);
            this.pnlToast.Location = new System.Drawing.Point(7, 7);
            this.pnlToast.Name = "pnlToast";
            this.pnlToast.Size = new System.Drawing.Size(436, 106);
            this.pnlToast.TabIndex = 0;

            this.picStatus.Image = global::FuntinoControlCenter.Properties.Resources.MessageSuccess;
            this.picStatus.Location = new System.Drawing.Point(12, 18);
            this.picStatus.Name = "picStatus";
            this.picStatus.Size = new System.Drawing.Size(70, 70);
            this.picStatus.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picStatus.TabIndex = 0;
            this.picStatus.TabStop = false;

            this.lblTitle.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(28, 80, 66);
            this.lblTitle.Location = new System.Drawing.Point(94, 14);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblTitle.Size = new System.Drawing.Size(290, 27);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "عملیات انجام شد";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            this.lblMessage.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F);
            this.lblMessage.ForeColor = System.Drawing.Color.FromArgb(77, 96, 87);
            this.lblMessage.Location = new System.Drawing.Point(94, 43);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblMessage.Size = new System.Drawing.Size(290, 49);
            this.lblMessage.TabIndex = 2;
            this.lblMessage.Text = "پیام";
            this.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            this.btnDismiss.BackColor = System.Drawing.Color.FromArgb(246, 249, 247);
            this.btnDismiss.BorderColor = System.Drawing.Color.FromArgb(214, 225, 219);
            this.btnDismiss.BorderSize = 1;
            this.btnDismiss.CornerRadius = 8;
            this.btnDismiss.FlatAppearance.BorderSize = 0;
            this.btnDismiss.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDismiss.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 10F, System.Drawing.FontStyle.Regular);
            this.btnDismiss.ForeColor = System.Drawing.Color.FromArgb(86, 102, 94);
            this.btnDismiss.Location = new System.Drawing.Point(391, 13);
            this.btnDismiss.Name = "btnDismiss";
            this.btnDismiss.Size = new System.Drawing.Size(30, 28);
            this.btnDismiss.TabIndex = 3;
            this.btnDismiss.Text = "×";
            this.btnDismiss.UseVisualStyleBackColor = false;
            this.btnDismiss.Click += new System.EventHandler(this.BtnDismiss_Click);

            this.displayTimer.Interval = 3500;
            this.displayTimer.Tick += new System.EventHandler(this.DisplayTimer_Tick);

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(241, 247, 243);
            this.ClientSize = new System.Drawing.Size(450, 120);
            this.Controls.Add(this.pnlToast);
            this.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AppToastForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "اعلان";
            this.Shown += new System.EventHandler(this.AppToastForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.picStatus)).EndInit();
            this.pnlToast.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
