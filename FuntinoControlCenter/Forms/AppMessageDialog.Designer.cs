namespace FuntinoControlCenter.Forms
{
    partial class AppMessageDialog
    {
        private System.ComponentModel.IContainer components = null;
        private FuntinoControlCenter.UI.RoundedBorderPanel pnlCard;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.PictureBox picStatus;
        private System.Windows.Forms.Label lblWindowTitle;
        private System.Windows.Forms.Label lblSeverity;
        private System.Windows.Forms.TextBox txtMessage;
        private FuntinoControlCenter.UI.RoundedButton btnPrimary;
        private FuntinoControlCenter.UI.RoundedButton btnSecondary;
        private FuntinoControlCenter.UI.RoundedButton btnTertiary;

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
            this.pnlCard = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.picStatus = new System.Windows.Forms.PictureBox();
            this.lblWindowTitle = new System.Windows.Forms.Label();
            this.lblSeverity = new System.Windows.Forms.Label();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnPrimary = new FuntinoControlCenter.UI.RoundedButton();
            this.btnSecondary = new FuntinoControlCenter.UI.RoundedButton();
            this.btnTertiary = new FuntinoControlCenter.UI.RoundedButton();
            ((System.ComponentModel.ISupportInitialize)(this.picStatus)).BeginInit();
            this.pnlCard.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.SuspendLayout();

            this.pnlCard.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            this.pnlCard.BorderColor = System.Drawing.Color.FromArgb(194, 215, 205);
            this.pnlCard.BorderSize = 1;
            this.pnlCard.CornerRadius = 20;
            this.pnlCard.Controls.Add(this.pnlHeader);
            this.pnlCard.Controls.Add(this.txtMessage);
            this.pnlCard.Controls.Add(this.btnPrimary);
            this.pnlCard.Controls.Add(this.btnSecondary);
            this.pnlCard.Controls.Add(this.btnTertiary);
            this.pnlCard.Location = new System.Drawing.Point(8, 8);
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Size = new System.Drawing.Size(554, 304);
            this.pnlCard.TabIndex = 0;

            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(20, 55, 58);
            this.pnlHeader.Controls.Add(this.picStatus);
            this.pnlHeader.Controls.Add(this.lblWindowTitle);
            this.pnlHeader.Controls.Add(this.lblSeverity);
            this.pnlHeader.Location = new System.Drawing.Point(1, 1);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(552, 82);
            this.pnlHeader.TabIndex = 0;

            this.picStatus.BackColor = System.Drawing.Color.Transparent;
            this.picStatus.Image = global::FuntinoControlCenter.Properties.Resources.MessageInfo;
            this.picStatus.Location = new System.Drawing.Point(468, 9);
            this.picStatus.Name = "picStatus";
            this.picStatus.Size = new System.Drawing.Size(68, 68);
            this.picStatus.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picStatus.TabIndex = 0;
            this.picStatus.TabStop = false;

            this.lblWindowTitle.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 11F, System.Drawing.FontStyle.Bold);
            this.lblWindowTitle.ForeColor = System.Drawing.Color.White;
            this.lblWindowTitle.Location = new System.Drawing.Point(20, 10);
            this.lblWindowTitle.Name = "lblWindowTitle";
            this.lblWindowTitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblWindowTitle.Size = new System.Drawing.Size(430, 33);
            this.lblWindowTitle.TabIndex = 1;
            this.lblWindowTitle.Text = "پیام سامانه";
            this.lblWindowTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            this.lblSeverity.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F, System.Drawing.FontStyle.Bold);
            this.lblSeverity.ForeColor = System.Drawing.Color.FromArgb(207, 230, 220);
            this.lblSeverity.Location = new System.Drawing.Point(20, 44);
            this.lblSeverity.Name = "lblSeverity";
            this.lblSeverity.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblSeverity.Size = new System.Drawing.Size(430, 25);
            this.lblSeverity.TabIndex = 2;
            this.lblSeverity.Text = "اطلاع‌رسانی";
            this.lblSeverity.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            this.txtMessage.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMessage.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9F);
            this.txtMessage.ForeColor = System.Drawing.Color.FromArgb(53, 72, 65);
            this.txtMessage.Location = new System.Drawing.Point(28, 101);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ReadOnly = true;
            this.txtMessage.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMessage.Size = new System.Drawing.Size(498, 126);
            this.txtMessage.TabIndex = 1;
            this.txtMessage.Text = "متن پیام";

            this.btnPrimary.BackColor = System.Drawing.Color.FromArgb(23, 112, 92);
            this.btnPrimary.BorderColor = System.Drawing.Color.FromArgb(23, 112, 92);
            this.btnPrimary.BorderSize = 1;
            this.btnPrimary.CornerRadius = 12;
            this.btnPrimary.FlatAppearance.BorderSize = 0;
            this.btnPrimary.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrimary.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F, System.Drawing.FontStyle.Bold);
            this.btnPrimary.ForeColor = System.Drawing.Color.White;
            this.btnPrimary.Location = new System.Drawing.Point(390, 249);
            this.btnPrimary.Name = "btnPrimary";
            this.btnPrimary.Size = new System.Drawing.Size(136, 42);
            this.btnPrimary.TabIndex = 2;
            this.btnPrimary.Text = "متوجه شدم";
            this.btnPrimary.UseVisualStyleBackColor = false;
            this.btnPrimary.Click += new System.EventHandler(this.BtnPrimary_Click);

            this.btnSecondary.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            this.btnSecondary.BorderColor = System.Drawing.Color.FromArgb(185, 202, 193);
            this.btnSecondary.BorderSize = 1;
            this.btnSecondary.CornerRadius = 12;
            this.btnSecondary.FlatAppearance.BorderSize = 0;
            this.btnSecondary.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSecondary.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F, System.Drawing.FontStyle.Bold);
            this.btnSecondary.ForeColor = System.Drawing.Color.FromArgb(50, 75, 68);
            this.btnSecondary.Location = new System.Drawing.Point(240, 249);
            this.btnSecondary.Name = "btnSecondary";
            this.btnSecondary.Size = new System.Drawing.Size(136, 42);
            this.btnSecondary.TabIndex = 3;
            this.btnSecondary.Text = "انصراف";
            this.btnSecondary.UseVisualStyleBackColor = false;
            this.btnSecondary.Visible = false;
            this.btnSecondary.Click += new System.EventHandler(this.BtnSecondary_Click);

            this.btnTertiary.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            this.btnTertiary.BorderColor = System.Drawing.Color.FromArgb(185, 202, 193);
            this.btnTertiary.BorderSize = 1;
            this.btnTertiary.CornerRadius = 12;
            this.btnTertiary.FlatAppearance.BorderSize = 0;
            this.btnTertiary.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTertiary.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F, System.Drawing.FontStyle.Bold);
            this.btnTertiary.ForeColor = System.Drawing.Color.FromArgb(50, 75, 68);
            this.btnTertiary.Location = new System.Drawing.Point(90, 249);
            this.btnTertiary.Name = "btnTertiary";
            this.btnTertiary.Size = new System.Drawing.Size(136, 42);
            this.btnTertiary.TabIndex = 4;
            this.btnTertiary.Text = "انصراف";
            this.btnTertiary.UseVisualStyleBackColor = false;
            this.btnTertiary.Visible = false;
            this.btnTertiary.Click += new System.EventHandler(this.BtnTertiary_Click);

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(241, 247, 243);
            this.ClientSize = new System.Drawing.Size(570, 320);
            this.Controls.Add(this.pnlCard);
            this.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AppMessageDialog";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "پیام سامانه";
            this.Load += new System.EventHandler(this.AppMessageDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picStatus)).EndInit();
            this.pnlHeader.ResumeLayout(false);
            this.pnlCard.ResumeLayout(false);
            this.pnlCard.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
