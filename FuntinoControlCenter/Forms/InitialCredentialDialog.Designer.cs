namespace FuntinoControlCenter.Forms
{
    partial class InitialCredentialDialog
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlCard;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblInstructions;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.Label lblPassword;
        private global::FuntinoControlCenter.UI.RoundedBorderPanel pnlPasswordInput;
        private System.Windows.Forms.TextBox txtPassword;
        private global::FuntinoControlCenter.UI.RoundedButton btnCopyPassword;
        private global::FuntinoControlCenter.UI.RoundedButton btnClose;
        private System.Windows.Forms.Label lblCopyStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlCard = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblInstructions = new System.Windows.Forms.Label();
            this.lblUserName = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.pnlPasswordInput = new global::FuntinoControlCenter.UI.RoundedBorderPanel();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnCopyPassword = new global::FuntinoControlCenter.UI.RoundedButton();
            this.btnClose = new global::FuntinoControlCenter.UI.RoundedButton();
            this.lblCopyStatus = new System.Windows.Forms.Label();
            this.pnlCard.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlCard
            //
            this.pnlCard.BackColor = System.Drawing.Color.FromArgb(251, 252, 250);
            this.pnlCard.Controls.Add(this.lblTitle);
            this.pnlCard.Controls.Add(this.lblInstructions);
            this.pnlCard.Controls.Add(this.lblUserName);
            this.pnlCard.Controls.Add(this.lblPassword);
            this.pnlCard.Controls.Add(this.pnlPasswordInput);
            this.pnlCard.Controls.Add(this.lblCopyStatus);
            this.pnlCard.Controls.Add(this.btnCopyPassword);
            this.pnlCard.Controls.Add(this.btnClose);
            this.pnlCard.Location = new System.Drawing.Point(370, 55);
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Size = new System.Drawing.Size(590, 430);
            this.pnlCard.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 20F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(20, 49, 58);
            this.lblTitle.Location = new System.Drawing.Point(35, 25);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblTitle.Size = new System.Drawing.Size(520, 47);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "اطلاعات ورود اولیه";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblInstructions
            //
            this.lblInstructions.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9F);
            this.lblInstructions.ForeColor = System.Drawing.Color.FromArgb(91, 112, 107);
            this.lblInstructions.Location = new System.Drawing.Point(35, 78);
            this.lblInstructions.Name = "lblInstructions";
            this.lblInstructions.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblInstructions.Size = new System.Drawing.Size(520, 64);
            this.lblInstructions.TabIndex = 1;
            this.lblInstructions.Text = "این گذرواژه فقط همین‌بار نمایش داده می‌شود. آن را در جای امن نگهداری کنید؛ پس از ورود باید گذرواژه‌ی تازه بسازید.";
            this.lblInstructions.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblUserName
            //
            this.lblUserName.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 10F, System.Drawing.FontStyle.Bold);
            this.lblUserName.ForeColor = System.Drawing.Color.FromArgb(44, 76, 72);
            this.lblUserName.Location = new System.Drawing.Point(35, 151);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblUserName.Size = new System.Drawing.Size(520, 30);
            this.lblUserName.TabIndex = 2;
            this.lblUserName.Text = "نام کاربری این حساب";
            this.lblUserName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblPassword
            //
            this.lblPassword.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9F, System.Drawing.FontStyle.Bold);
            this.lblPassword.ForeColor = System.Drawing.Color.FromArgb(78, 99, 94);
            this.lblPassword.Location = new System.Drawing.Point(35, 190);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblPassword.Size = new System.Drawing.Size(520, 25);
            this.lblPassword.TabIndex = 3;
            this.lblPassword.Text = "گذرواژه‌ی موقت";
            this.lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // pnlPasswordInput
            //
            this.pnlPasswordInput.BackColor = System.Drawing.Color.FromArgb(244, 248, 246);
            this.pnlPasswordInput.BorderColor = System.Drawing.Color.FromArgb(190, 213, 204);
            this.pnlPasswordInput.BorderSize = 1;
            this.pnlPasswordInput.Controls.Add(this.txtPassword);
            this.pnlPasswordInput.CornerRadius = 9;
            this.pnlPasswordInput.Location = new System.Drawing.Point(35, 214);
            this.pnlPasswordInput.Name = "pnlPasswordInput";
            this.pnlPasswordInput.Size = new System.Drawing.Size(520, 48);
            this.pnlPasswordInput.TabIndex = 0;
            this.pnlPasswordInput.TabStop = false;
            //
            // txtPassword
            //
            this.txtPassword.BackColor = System.Drawing.Color.FromArgb(244, 248, 246);
            this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtPassword.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 14F, System.Drawing.FontStyle.Bold);
            this.txtPassword.ForeColor = System.Drawing.Color.FromArgb(20, 58, 57);
            this.txtPassword.Location = new System.Drawing.Point(8, 8);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.ReadOnly = true;
            this.txtPassword.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtPassword.Size = new System.Drawing.Size(504, 31);
            this.txtPassword.TabIndex = 0;
            this.txtPassword.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            //
            // lblCopyStatus
            //
            this.lblCopyStatus.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F);
            this.lblCopyStatus.ForeColor = System.Drawing.Color.FromArgb(31, 111, 96);
            this.lblCopyStatus.Location = new System.Drawing.Point(35, 273);
            this.lblCopyStatus.Name = "lblCopyStatus";
            this.lblCopyStatus.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblCopyStatus.Size = new System.Drawing.Size(520, 33);
            this.lblCopyStatus.TabIndex = 4;
            this.lblCopyStatus.Text = "برای نگهداری امن، گذرواژه را کپی کنید.";
            this.lblCopyStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // btnCopyPassword
            //
            this.btnCopyPassword.BackColor = System.Drawing.Color.FromArgb(18, 96, 83);
            this.btnCopyPassword.FlatAppearance.BorderSize = 0;
            this.btnCopyPassword.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(27, 117, 101);
            this.btnCopyPassword.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCopyPassword.BorderColor = System.Drawing.Color.FromArgb(13, 76, 68);
            this.btnCopyPassword.BorderSize = 1;
            this.btnCopyPassword.CornerRadius = 12;
            this.btnCopyPassword.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 10F, System.Drawing.FontStyle.Bold);
            this.btnCopyPassword.ForeColor = System.Drawing.Color.White;
            this.btnCopyPassword.Image = global::FuntinoControlCenter.Properties.Resources.IconCopy;
            this.btnCopyPassword.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCopyPassword.Location = new System.Drawing.Point(225, 329);
            this.btnCopyPassword.Name = "btnCopyPassword";
            this.btnCopyPassword.Size = new System.Drawing.Size(330, 55);
            this.btnCopyPassword.TabIndex = 1;
            this.btnCopyPassword.Text = "کپی گذرواژه";
            this.btnCopyPassword.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCopyPassword.UseVisualStyleBackColor = false;
            this.btnCopyPassword.Click += new System.EventHandler(this.CopyPassword);
            //
            // btnClose
            //
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(231, 239, 236);
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(219, 232, 226);
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.BorderColor = System.Drawing.Color.FromArgb(184, 207, 198);
            this.btnClose.BorderSize = 1;
            this.btnClose.CornerRadius = 10;
            this.btnClose.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9.5F);
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(45, 76, 71);
            this.btnClose.Image = global::FuntinoControlCenter.Properties.Resources.IconBack;
            this.btnClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClose.Location = new System.Drawing.Point(35, 329);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(170, 55);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "متوجه شدم";
            this.btnClose.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnClose.UseVisualStyleBackColor = false;
            //
            // InitialCredentialDialog
            //
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(6, 23, 39);
            this.BackgroundImage = global::FuntinoControlCenter.Properties.Resources.InitialCredentialBackground;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(1000, 540);
            this.Controls.Add(this.pnlCard);
            this.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9.5F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InitialCredentialDialog";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "گذرواژه‌ی موقت";
            this.Shown += new System.EventHandler(this.InitialCredentialDialog_Shown);
            this.pnlCard.ResumeLayout(false);
            this.pnlCard.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
