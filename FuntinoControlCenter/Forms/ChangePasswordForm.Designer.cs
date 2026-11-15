namespace FuntinoControlCenter.Forms
{
    partial class ChangePasswordForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlCard;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblCurrent;
        private System.Windows.Forms.Label lblNew;
        private System.Windows.Forms.Label lblConfirm;
        private System.Windows.Forms.Label lblHint;
        private global::FuntinoControlCenter.UI.RoundedBorderPanel pnlCurrentPassword;
        private global::FuntinoControlCenter.UI.RoundedBorderPanel pnlNewPassword;
        private global::FuntinoControlCenter.UI.RoundedBorderPanel pnlConfirmPassword;
        private System.Windows.Forms.TextBox txtCurrentPassword;
        private System.Windows.Forms.TextBox txtNewPassword;
        private System.Windows.Forms.TextBox txtConfirmPassword;
        private global::FuntinoControlCenter.UI.RoundedButton btnSave;
        private global::FuntinoControlCenter.UI.RoundedButton btnCancel;
        private global::FuntinoControlCenter.UI.AppLoadingIndicator loadingSave;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlCard = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblCurrent = new System.Windows.Forms.Label();
            this.lblNew = new System.Windows.Forms.Label();
            this.lblConfirm = new System.Windows.Forms.Label();
            this.lblHint = new System.Windows.Forms.Label();
            this.pnlCurrentPassword = new global::FuntinoControlCenter.UI.RoundedBorderPanel();
            this.pnlNewPassword = new global::FuntinoControlCenter.UI.RoundedBorderPanel();
            this.pnlConfirmPassword = new global::FuntinoControlCenter.UI.RoundedBorderPanel();
            this.txtCurrentPassword = new System.Windows.Forms.TextBox();
            this.txtNewPassword = new System.Windows.Forms.TextBox();
            this.txtConfirmPassword = new System.Windows.Forms.TextBox();
            this.btnSave = new global::FuntinoControlCenter.UI.RoundedButton();
            this.btnCancel = new global::FuntinoControlCenter.UI.RoundedButton();
            this.loadingSave = new global::FuntinoControlCenter.UI.AppLoadingIndicator();
            this.pnlCard.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlCard
            //
            this.pnlCard.BackColor = System.Drawing.Color.FromArgb(251, 252, 250);
            this.pnlCard.Controls.Add(this.lblTitle);
            this.pnlCard.Controls.Add(this.lblSubtitle);
            this.pnlCard.Controls.Add(this.lblCurrent);
            this.pnlCard.Controls.Add(this.pnlCurrentPassword);
            this.pnlCard.Controls.Add(this.lblNew);
            this.pnlCard.Controls.Add(this.pnlNewPassword);
            this.pnlCard.Controls.Add(this.lblConfirm);
            this.pnlCard.Controls.Add(this.pnlConfirmPassword);
            this.pnlCard.Controls.Add(this.lblHint);
            this.pnlCard.Controls.Add(this.btnSave);
            this.pnlCard.Controls.Add(this.btnCancel);
            this.pnlCard.Controls.Add(this.loadingSave);
            this.pnlCard.Location = new System.Drawing.Point(390, 35);
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Size = new System.Drawing.Size(570, 578);
            this.pnlCard.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 20F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(20, 49, 58);
            this.lblTitle.Location = new System.Drawing.Point(35, 27);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblTitle.Size = new System.Drawing.Size(500, 48);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "یک گذرواژه‌ی تازه انتخاب کنید";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblSubtitle
            //
            this.lblSubtitle.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(100, 119, 114);
            this.lblSubtitle.Location = new System.Drawing.Point(35, 78);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblSubtitle.Size = new System.Drawing.Size(500, 42);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "گذرواژه‌ی تازه، از حساب کاربری شما بهتر محافظت می‌کند.";
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblCurrent
            //
            this.lblCurrent.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9.5F, System.Drawing.FontStyle.Bold);
            this.lblCurrent.ForeColor = System.Drawing.Color.FromArgb(46, 75, 72);
            this.lblCurrent.Location = new System.Drawing.Point(40, 133);
            this.lblCurrent.Name = "lblCurrent";
            this.lblCurrent.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblCurrent.Size = new System.Drawing.Size(490, 26);
            this.lblCurrent.TabIndex = 2;
            this.lblCurrent.Text = "گذرواژه‌ی فعلی";
            this.lblCurrent.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // pnlCurrentPassword
            //
            this.pnlCurrentPassword.BackColor = System.Drawing.Color.White;
            this.pnlCurrentPassword.BorderColor = System.Drawing.Color.FromArgb(190, 213, 204);
            this.pnlCurrentPassword.BorderSize = 1;
            this.pnlCurrentPassword.Controls.Add(this.txtCurrentPassword);
            this.pnlCurrentPassword.CornerRadius = 9;
            this.pnlCurrentPassword.Location = new System.Drawing.Point(40, 158);
            this.pnlCurrentPassword.Name = "pnlCurrentPassword";
            this.pnlCurrentPassword.Size = new System.Drawing.Size(490, 42);
            this.pnlCurrentPassword.TabIndex = 0;
            this.pnlCurrentPassword.TabStop = false;
            //
            // txtCurrentPassword
            //
            this.txtCurrentPassword.BackColor = System.Drawing.Color.White;
            this.txtCurrentPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCurrentPassword.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 12F);
            this.txtCurrentPassword.ForeColor = System.Drawing.Color.FromArgb(30, 54, 56);
            this.txtCurrentPassword.Location = new System.Drawing.Point(8, 6);
            this.txtCurrentPassword.Name = "txtCurrentPassword";
            this.txtCurrentPassword.PasswordChar = '●';
            this.txtCurrentPassword.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtCurrentPassword.Size = new System.Drawing.Size(474, 29);
            this.txtCurrentPassword.TabIndex = 0;
            //
            // lblNew
            //
            this.lblNew.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9.5F, System.Drawing.FontStyle.Bold);
            this.lblNew.ForeColor = System.Drawing.Color.FromArgb(46, 75, 72);
            this.lblNew.Location = new System.Drawing.Point(40, 215);
            this.lblNew.Name = "lblNew";
            this.lblNew.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblNew.Size = new System.Drawing.Size(490, 26);
            this.lblNew.TabIndex = 3;
            this.lblNew.Text = "گذرواژه‌ی تازه";
            this.lblNew.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // pnlNewPassword
            //
            this.pnlNewPassword.BackColor = System.Drawing.Color.White;
            this.pnlNewPassword.BorderColor = System.Drawing.Color.FromArgb(190, 213, 204);
            this.pnlNewPassword.BorderSize = 1;
            this.pnlNewPassword.Controls.Add(this.txtNewPassword);
            this.pnlNewPassword.CornerRadius = 9;
            this.pnlNewPassword.Location = new System.Drawing.Point(40, 240);
            this.pnlNewPassword.Name = "pnlNewPassword";
            this.pnlNewPassword.Size = new System.Drawing.Size(490, 42);
            this.pnlNewPassword.TabIndex = 1;
            this.pnlNewPassword.TabStop = false;
            //
            // txtNewPassword
            //
            this.txtNewPassword.BackColor = System.Drawing.Color.White;
            this.txtNewPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtNewPassword.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 12F);
            this.txtNewPassword.ForeColor = System.Drawing.Color.FromArgb(30, 54, 56);
            this.txtNewPassword.Location = new System.Drawing.Point(8, 6);
            this.txtNewPassword.Name = "txtNewPassword";
            this.txtNewPassword.PasswordChar = '●';
            this.txtNewPassword.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtNewPassword.Size = new System.Drawing.Size(474, 29);
            this.txtNewPassword.TabIndex = 1;
            //
            // lblConfirm
            //
            this.lblConfirm.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9.5F, System.Drawing.FontStyle.Bold);
            this.lblConfirm.ForeColor = System.Drawing.Color.FromArgb(46, 75, 72);
            this.lblConfirm.Location = new System.Drawing.Point(40, 297);
            this.lblConfirm.Name = "lblConfirm";
            this.lblConfirm.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblConfirm.Size = new System.Drawing.Size(490, 26);
            this.lblConfirm.TabIndex = 4;
            this.lblConfirm.Text = "تکرار گذرواژه‌ی تازه";
            this.lblConfirm.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // pnlConfirmPassword
            //
            this.pnlConfirmPassword.BackColor = System.Drawing.Color.White;
            this.pnlConfirmPassword.BorderColor = System.Drawing.Color.FromArgb(190, 213, 204);
            this.pnlConfirmPassword.BorderSize = 1;
            this.pnlConfirmPassword.Controls.Add(this.txtConfirmPassword);
            this.pnlConfirmPassword.CornerRadius = 9;
            this.pnlConfirmPassword.Location = new System.Drawing.Point(40, 322);
            this.pnlConfirmPassword.Name = "pnlConfirmPassword";
            this.pnlConfirmPassword.Size = new System.Drawing.Size(490, 42);
            this.pnlConfirmPassword.TabIndex = 2;
            this.pnlConfirmPassword.TabStop = false;
            //
            // txtConfirmPassword
            //
            this.txtConfirmPassword.BackColor = System.Drawing.Color.White;
            this.txtConfirmPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtConfirmPassword.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 12F);
            this.txtConfirmPassword.ForeColor = System.Drawing.Color.FromArgb(30, 54, 56);
            this.txtConfirmPassword.Location = new System.Drawing.Point(8, 6);
            this.txtConfirmPassword.Name = "txtConfirmPassword";
            this.txtConfirmPassword.PasswordChar = '●';
            this.txtConfirmPassword.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtConfirmPassword.Size = new System.Drawing.Size(474, 29);
            this.txtConfirmPassword.TabIndex = 2;
            //
            // lblHint
            //
            this.lblHint.BackColor = System.Drawing.Color.FromArgb(237, 245, 242);
            this.lblHint.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F);
            this.lblHint.ForeColor = System.Drawing.Color.FromArgb(82, 106, 100);
            this.lblHint.Location = new System.Drawing.Point(40, 379);
            this.lblHint.Name = "lblHint";
            this.lblHint.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblHint.Size = new System.Drawing.Size(490, 54);
            this.lblHint.TabIndex = 5;
            this.lblHint.Text = "گذرواژه‌ی تازه باید دست‌کم ۸ نویسه داشته باشد. آن را در جای امن نگهداری کنید.";
            this.lblHint.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // btnSave
            //
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(18, 96, 83);
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(27, 117, 101);
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.BorderColor = System.Drawing.Color.FromArgb(13, 76, 68);
            this.btnSave.BorderSize = 1;
            this.btnSave.CornerRadius = 12;
            this.btnSave.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 10F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Image = global::FuntinoControlCenter.Properties.Resources.IconPasswordSave;
            this.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSave.Location = new System.Drawing.Point(310, 475);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(220, 54);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "ذخیره گذرواژه";
            this.btnSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSave.UseVisualStyleBackColor = false;
            //
            // btnCancel
            //
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(231, 239, 236);
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(219, 232, 226);
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.BorderColor = System.Drawing.Color.FromArgb(184, 207, 198);
            this.btnCancel.BorderSize = 1;
            this.btnCancel.CornerRadius = 10;
            this.btnCancel.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9.5F);
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(45, 76, 71);
            this.btnCancel.Image = global::FuntinoControlCenter.Properties.Resources.IconBack;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(40, 475);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(240, 54);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "بازگشت";
            this.btnCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCancel.UseVisualStyleBackColor = false;
            this.loadingSave.Location = new System.Drawing.Point(40, 539);
            this.loadingSave.Name = "loadingSave";
            this.loadingSave.Size = new System.Drawing.Size(490, 34);
            this.loadingSave.TabIndex = 6;
            this.loadingSave.Visible = false;
            //
            // ChangePasswordForm
            //
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(6, 23, 39);
            this.BackgroundImage = global::FuntinoControlCenter.Properties.Resources.ChangePasswordBackground;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(1000, 650);
            this.Controls.Add(this.pnlCard);
            this.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9.5F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChangePasswordForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "تغییر گذرواژه";
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            this.pnlCard.ResumeLayout(false);
            this.pnlCard.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
