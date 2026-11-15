namespace FuntinoControlCenter.Forms
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlCard;
        private System.Windows.Forms.Panel pnlStatus;
        private System.Windows.Forms.Label lblBrand;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblWelcome;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.Label lblPassword;
        private global::FuntinoControlCenter.UI.RoundedBorderPanel pnlUsernameInput;
        private global::FuntinoControlCenter.UI.RoundedBorderPanel pnlPasswordInput;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private global::FuntinoControlCenter.UI.RoundedButton btnLogin;
        private global::FuntinoControlCenter.UI.RoundedButton btnSettings;
        private global::FuntinoControlCenter.UI.RoundedButton btnExit;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblDatabases;
        private global::FuntinoControlCenter.UI.AppLoadingIndicator loadingStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlCard = new System.Windows.Forms.Panel();
            this.pnlStatus = new System.Windows.Forms.Panel();
            this.lblBrand = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblWelcome = new System.Windows.Forms.Label();
            this.lblUsername = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.pnlUsernameInput = new global::FuntinoControlCenter.UI.RoundedBorderPanel();
            this.pnlPasswordInput = new global::FuntinoControlCenter.UI.RoundedBorderPanel();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnLogin = new global::FuntinoControlCenter.UI.RoundedButton();
            this.btnSettings = new global::FuntinoControlCenter.UI.RoundedButton();
            this.btnExit = new global::FuntinoControlCenter.UI.RoundedButton();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblDatabases = new System.Windows.Forms.Label();
            this.loadingStatus = new global::FuntinoControlCenter.UI.AppLoadingIndicator();
            this.pnlCard.SuspendLayout();
            this.pnlStatus.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlCard
            //
            this.pnlCard.BackColor = System.Drawing.Color.FromArgb(251, 252, 250);
            this.pnlCard.Controls.Add(this.lblBrand);
            this.pnlCard.Controls.Add(this.lblSubtitle);
            this.pnlCard.Controls.Add(this.lblWelcome);
            this.pnlCard.Controls.Add(this.lblUsername);
            this.pnlCard.Controls.Add(this.pnlUsernameInput);
            this.pnlCard.Controls.Add(this.lblPassword);
            this.pnlCard.Controls.Add(this.pnlPasswordInput);
            this.pnlCard.Controls.Add(this.pnlStatus);
            this.pnlCard.Controls.Add(this.btnLogin);
            this.pnlCard.Controls.Add(this.btnSettings);
            this.pnlCard.Controls.Add(this.btnExit);
            this.pnlCard.Controls.Add(this.lblDatabases);
            this.pnlCard.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pnlCard.Location = new System.Drawing.Point(815, 100);
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Size = new System.Drawing.Size(560, 700);
            this.pnlCard.TabIndex = 0;
            //
            // lblBrand
            //
            this.lblBrand.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 27F, System.Drawing.FontStyle.Bold);
            this.lblBrand.ForeColor = System.Drawing.Color.FromArgb(20, 49, 58);
            this.lblBrand.Location = new System.Drawing.Point(34, 34);
            this.lblBrand.Name = "lblBrand";
            this.lblBrand.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblBrand.Size = new System.Drawing.Size(492, 58);
            this.lblBrand.TabIndex = 0;
            this.lblBrand.Text = "فانتینو";
            this.lblBrand.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblSubtitle
            //
            this.lblSubtitle.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 11F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(96, 117, 113);
            this.lblSubtitle.Location = new System.Drawing.Point(34, 91);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblSubtitle.Size = new System.Drawing.Size(492, 44);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "مدیریت یکپارچه‌ی مجموعه‌ی شما";
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblWelcome
            //
            this.lblWelcome.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 15F, System.Drawing.FontStyle.Bold);
            this.lblWelcome.ForeColor = System.Drawing.Color.FromArgb(29, 65, 67);
            this.lblWelcome.Location = new System.Drawing.Point(34, 158);
            this.lblWelcome.Name = "lblWelcome";
            this.lblWelcome.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblWelcome.Size = new System.Drawing.Size(492, 40);
            this.lblWelcome.TabIndex = 2;
            this.lblWelcome.Text = "ورود به حساب کاربری";
            this.lblWelcome.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblUsername
            //
            this.lblUsername.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 10F, System.Drawing.FontStyle.Bold);
            this.lblUsername.ForeColor = System.Drawing.Color.FromArgb(47, 76, 73);
            this.lblUsername.Location = new System.Drawing.Point(35, 212);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblUsername.Size = new System.Drawing.Size(490, 27);
            this.lblUsername.TabIndex = 3;
            this.lblUsername.Text = "نام کاربری";
            this.lblUsername.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // pnlUsernameInput
            //
            this.pnlUsernameInput.BackColor = System.Drawing.Color.White;
            this.pnlUsernameInput.BorderColor = System.Drawing.Color.FromArgb(190, 213, 204);
            this.pnlUsernameInput.BorderSize = 1;
            this.pnlUsernameInput.Controls.Add(this.txtUsername);
            this.pnlUsernameInput.CornerRadius = 9;
            this.pnlUsernameInput.Location = new System.Drawing.Point(35, 237);
            this.pnlUsernameInput.Name = "pnlUsernameInput";
            this.pnlUsernameInput.Size = new System.Drawing.Size(490, 42);
            this.pnlUsernameInput.TabIndex = 0;
            this.pnlUsernameInput.TabStop = false;
            //
            // txtUsername
            //
            this.txtUsername.BackColor = System.Drawing.Color.White;
            this.txtUsername.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtUsername.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 12F);
            this.txtUsername.ForeColor = System.Drawing.Color.FromArgb(31, 55, 57);
            this.txtUsername.Location = new System.Drawing.Point(8, 6);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.txtUsername.Size = new System.Drawing.Size(474, 29);
            this.txtUsername.TabIndex = 0;
            this.txtUsername.Text = "Admin";
            //
            // lblPassword
            //
            this.lblPassword.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 10F, System.Drawing.FontStyle.Bold);
            this.lblPassword.ForeColor = System.Drawing.Color.FromArgb(47, 76, 73);
            this.lblPassword.Location = new System.Drawing.Point(35, 300);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblPassword.Size = new System.Drawing.Size(490, 27);
            this.lblPassword.TabIndex = 4;
            this.lblPassword.Text = "گذرواژه";
            this.lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // pnlPasswordInput
            //
            this.pnlPasswordInput.BackColor = System.Drawing.Color.White;
            this.pnlPasswordInput.BorderColor = System.Drawing.Color.FromArgb(190, 213, 204);
            this.pnlPasswordInput.BorderSize = 1;
            this.pnlPasswordInput.Controls.Add(this.txtPassword);
            this.pnlPasswordInput.CornerRadius = 9;
            this.pnlPasswordInput.Location = new System.Drawing.Point(35, 325);
            this.pnlPasswordInput.Name = "pnlPasswordInput";
            this.pnlPasswordInput.Size = new System.Drawing.Size(490, 42);
            this.pnlPasswordInput.TabIndex = 1;
            this.pnlPasswordInput.TabStop = false;
            //
            // txtPassword
            //
            this.txtPassword.BackColor = System.Drawing.Color.White;
            this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtPassword.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 12F);
            this.txtPassword.ForeColor = System.Drawing.Color.FromArgb(31, 55, 57);
            this.txtPassword.Location = new System.Drawing.Point(8, 6);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '●';
            this.txtPassword.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtPassword.Size = new System.Drawing.Size(474, 29);
            this.txtPassword.TabIndex = 1;
            //
            // pnlStatus
            //
            this.pnlStatus.BackColor = System.Drawing.Color.FromArgb(237, 245, 242);
            this.pnlStatus.Controls.Add(this.lblStatus);
            this.pnlStatus.Controls.Add(this.loadingStatus);
            this.pnlStatus.Location = new System.Drawing.Point(35, 391);
            this.pnlStatus.Name = "pnlStatus";
            this.pnlStatus.Size = new System.Drawing.Size(490, 70);
            this.pnlStatus.TabIndex = 5;
            //
            // lblStatus
            //
            this.lblStatus.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9.5F);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(151, 111, 44);
            this.lblStatus.Location = new System.Drawing.Point(14, 8);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblStatus.Size = new System.Drawing.Size(462, 54);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "در حال آماده‌سازی اطلاعات برنامه…";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.loadingStatus.Location = new System.Drawing.Point(10, 18);
            this.loadingStatus.Name = "loadingStatus";
            this.loadingStatus.Size = new System.Drawing.Size(470, 34);
            this.loadingStatus.TabIndex = 1;
            this.loadingStatus.Visible = false;
            //
            // btnLogin
            //
            this.btnLogin.BackColor = System.Drawing.Color.FromArgb(18, 96, 83);
            this.btnLogin.Enabled = false;
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(13, 76, 68);
            this.btnLogin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(27, 117, 101);
            this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogin.BorderColor = System.Drawing.Color.FromArgb(13, 76, 68);
            this.btnLogin.BorderSize = 1;
            this.btnLogin.CornerRadius = 12;
            this.btnLogin.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 12F, System.Drawing.FontStyle.Bold);
            this.btnLogin.ForeColor = System.Drawing.Color.White;
            this.btnLogin.Image = global::FuntinoControlCenter.Properties.Resources.IconSignIn;
            this.btnLogin.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLogin.Location = new System.Drawing.Point(35, 482);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(490, 58);
            this.btnLogin.TabIndex = 2;
            this.btnLogin.Text = "ورود به سامانه";
            this.btnLogin.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnLogin.UseVisualStyleBackColor = false;
            //
            // btnSettings
            //
            this.btnSettings.BackColor = System.Drawing.Color.FromArgb(231, 239, 236);
            this.btnSettings.FlatAppearance.BorderSize = 0;
            this.btnSettings.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(219, 232, 226);
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.BorderColor = System.Drawing.Color.FromArgb(184, 207, 198);
            this.btnSettings.BorderSize = 1;
            this.btnSettings.CornerRadius = 10;
            this.btnSettings.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9.5F, System.Drawing.FontStyle.Bold);
            this.btnSettings.ForeColor = System.Drawing.Color.FromArgb(34, 75, 68);
            this.btnSettings.Image = global::FuntinoControlCenter.Properties.Resources.IconSettings;
            this.btnSettings.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSettings.Location = new System.Drawing.Point(35, 558);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(295, 50);
            this.btnSettings.TabIndex = 3;
            this.btnSettings.Text = "تنظیم ارتباط برنامه";
            this.btnSettings.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSettings.UseVisualStyleBackColor = false;
            //
            // btnExit
            //
            this.btnExit.BackColor = System.Drawing.Color.FromArgb(248, 236, 233);
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(244, 224, 219);
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.BorderColor = System.Drawing.Color.FromArgb(222, 192, 184);
            this.btnExit.BorderSize = 1;
            this.btnExit.CornerRadius = 10;
            this.btnExit.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9.5F);
            this.btnExit.ForeColor = System.Drawing.Color.FromArgb(128, 70, 59);
            this.btnExit.Image = global::FuntinoControlCenter.Properties.Resources.IconExit;
            this.btnExit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExit.Location = new System.Drawing.Point(350, 558);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(175, 50);
            this.btnExit.TabIndex = 4;
            this.btnExit.Text = "بستن برنامه";
            this.btnExit.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnExit.UseVisualStyleBackColor = false;
            //
            // lblDatabases
            //
            this.lblDatabases.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F);
            this.lblDatabases.ForeColor = System.Drawing.Color.FromArgb(122, 142, 137);
            this.lblDatabases.Location = new System.Drawing.Point(35, 632);
            this.lblDatabases.Name = "lblDatabases";
            this.lblDatabases.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblDatabases.Size = new System.Drawing.Size(490, 42);
            this.lblDatabases.TabIndex = 6;
            this.lblDatabases.Text = "مدیریت امن و یکپارچه‌ی مجموعه‌ی شما";
            this.lblDatabases.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // LoginForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(6, 23, 39);
            this.BackgroundImage = global::FuntinoControlCenter.Properties.Resources.LoginBackground;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(1440, 900);
            this.Controls.Add(this.pnlCard);
            this.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9.5F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.Name = "LoginForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "فانتینو | ورود امن";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.LoginForm_Load);
            this.btnLogin.Click += new System.EventHandler(this.BtnLogin_Click);
            this.btnSettings.Click += new System.EventHandler(this.BtnSettings_Click);
            this.btnExit.Click += new System.EventHandler(this.BtnExit_Click);
            this.txtPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Password_KeyDown);
            this.pnlStatus.ResumeLayout(false);
            this.pnlCard.ResumeLayout(false);
            this.pnlCard.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
