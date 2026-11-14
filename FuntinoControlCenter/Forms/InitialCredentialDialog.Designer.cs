namespace FuntinoControlCenter.Forms
{
    partial class InitialCredentialDialog
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblInstructions;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnCopyPassword;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblCopyStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblInstructions = new System.Windows.Forms.Label();
            this.lblUserName = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnCopyPassword = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblCopyStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.Font = new System.Drawing.Font("Tahoma", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(20, 55, 58);
            this.lblTitle.Location = new System.Drawing.Point(30, 25);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(550, 38);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "گذرواژه‌ی یک‌بارمصرف حساب";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblInstructions
            //
            this.lblInstructions.Location = new System.Drawing.Point(30, 76);
            this.lblInstructions.Name = "lblInstructions";
            this.lblInstructions.Size = new System.Drawing.Size(550, 55);
            this.lblInstructions.TabIndex = 1;
            this.lblInstructions.Text = "این گذرواژه فقط همین بار نمایش داده می‌شود. آن را کپی و در محل امن نگهداری کنید؛ پس از ورود باید گذرواژه‌ی تازه انتخاب شود.";
            this.lblInstructions.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblUserName
            //
            this.lblUserName.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.lblUserName.Location = new System.Drawing.Point(30, 139);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(550, 30);
            this.lblUserName.TabIndex = 2;
            this.lblUserName.Text = "نام کاربری";
            this.lblUserName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // txtPassword
            //
            this.txtPassword.Font = new System.Drawing.Font("Consolas", 13F, System.Drawing.FontStyle.Bold);
            this.txtPassword.Location = new System.Drawing.Point(30, 180);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.ReadOnly = true;
            this.txtPassword.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtPassword.Size = new System.Drawing.Size(550, 27);
            this.txtPassword.TabIndex = 0;
            this.txtPassword.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            //
            // btnCopyPassword
            //
            this.btnCopyPassword.BackColor = System.Drawing.Color.FromArgb(20, 89, 81);
            this.btnCopyPassword.FlatAppearance.BorderSize = 0;
            this.btnCopyPassword.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCopyPassword.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
            this.btnCopyPassword.ForeColor = System.Drawing.Color.White;
            this.btnCopyPassword.Location = new System.Drawing.Point(350, 235);
            this.btnCopyPassword.Name = "btnCopyPassword";
            this.btnCopyPassword.Size = new System.Drawing.Size(230, 42);
            this.btnCopyPassword.TabIndex = 1;
            this.btnCopyPassword.Text = "کپی گذرواژه";
            this.btnCopyPassword.UseVisualStyleBackColor = false;
            this.btnCopyPassword.Click += new System.EventHandler(this.CopyPassword);
            //
            // btnClose
            //
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Font = new System.Drawing.Font("Tahoma", 9F);
            this.btnClose.Location = new System.Drawing.Point(30, 235);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(140, 42);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "بستن";
            this.btnClose.UseVisualStyleBackColor = true;
            //
            // lblCopyStatus
            //
            this.lblCopyStatus.ForeColor = System.Drawing.Color.FromArgb(31, 111, 96);
            this.lblCopyStatus.Location = new System.Drawing.Point(185, 238);
            this.lblCopyStatus.Name = "lblCopyStatus";
            this.lblCopyStatus.Size = new System.Drawing.Size(155, 36);
            this.lblCopyStatus.TabIndex = 6;
            this.lblCopyStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // InitialCredentialDialog
            //
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(246, 250, 248);
            this.ClientSize = new System.Drawing.Size(610, 315);
            this.Controls.Add(this.lblCopyStatus);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnCopyPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblUserName);
            this.Controls.Add(this.lblInstructions);
            this.Controls.Add(this.lblTitle);
            this.Font = new System.Drawing.Font("Tahoma", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InitialCredentialDialog";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "اطلاعات ورود اولیه";
            this.Shown += new System.EventHandler(this.InitialCredentialDialog_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
