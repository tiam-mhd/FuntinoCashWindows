namespace FuntinoControlCenter.Forms
{
    partial class EdgeConfigurationForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel tlpRoot, tlpHeader, tlpFields, tlpBusy, tlpButtons;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.PictureBox picHeader;
        private System.Windows.Forms.Label lblTitle, lblSubtitle, lblBaseUrl, lblCredential, lblOrganization, lblBranch, lblSecret, lblSecretState, lblSourceInstance, lblHelp, lblBusy;
        private System.Windows.Forms.TextBox txtBaseUrl, txtCredential, txtOrganization, txtBranch, txtSecret;
        private System.Windows.Forms.ProgressBar progressBusy;
        private FuntinoControlCenter.UI.RoundedButton btnBack, btnSave;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tlpRoot = new System.Windows.Forms.TableLayoutPanel();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.tlpHeader = new System.Windows.Forms.TableLayoutPanel();
            this.picHeader = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.btnBack = new FuntinoControlCenter.UI.RoundedButton();
            this.tlpFields = new System.Windows.Forms.TableLayoutPanel();
            this.txtBaseUrl = new System.Windows.Forms.TextBox();
            this.lblBaseUrl = new System.Windows.Forms.Label();
            this.txtCredential = new System.Windows.Forms.TextBox();
            this.lblCredential = new System.Windows.Forms.Label();
            this.txtOrganization = new System.Windows.Forms.TextBox();
            this.lblOrganization = new System.Windows.Forms.Label();
            this.txtBranch = new System.Windows.Forms.TextBox();
            this.lblBranch = new System.Windows.Forms.Label();
            this.txtSecret = new System.Windows.Forms.TextBox();
            this.lblSecret = new System.Windows.Forms.Label();
            this.lblSecretState = new System.Windows.Forms.Label();
            this.lblSourceInstance = new System.Windows.Forms.Label();
            this.lblHelp = new System.Windows.Forms.Label();
            this.tlpBusy = new System.Windows.Forms.TableLayoutPanel();
            this.progressBusy = new System.Windows.Forms.ProgressBar();
            this.lblBusy = new System.Windows.Forms.Label();
            this.tlpButtons = new System.Windows.Forms.TableLayoutPanel();
            this.btnSave = new FuntinoControlCenter.UI.RoundedButton();
            this.tlpRoot.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.tlpHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picHeader)).BeginInit();
            this.tlpFields.SuspendLayout();
            this.tlpBusy.SuspendLayout();
            this.tlpButtons.SuspendLayout();
            this.SuspendLayout();
            //
            // tlpRoot
            //
            this.tlpRoot.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(246)))));
            this.tlpRoot.ColumnCount = 1;
            this.tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Controls.Add(this.pnlHeader, 0, 0);
            this.tlpRoot.Controls.Add(this.tlpFields, 0, 1);
            this.tlpRoot.Controls.Add(this.tlpBusy, 0, 2);
            this.tlpRoot.Controls.Add(this.tlpButtons, 0, 3);
            this.tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRoot.Location = new System.Drawing.Point(0, 0);
            this.tlpRoot.Name = "tlpRoot";
            this.tlpRoot.Padding = new System.Windows.Forms.Padding(28, 22, 28, 20);
            this.tlpRoot.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpRoot.RowCount = 4;
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 92F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tlpRoot.Size = new System.Drawing.Size(960, 650);
            this.tlpRoot.TabIndex = 0;
            //
            // pnlHeader
            //
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(63)))), ((int)(((byte)(57)))));
            this.pnlHeader.Controls.Add(this.tlpHeader);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHeader.Location = new System.Drawing.Point(28, 22);
            this.pnlHeader.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(904, 80);
            this.pnlHeader.TabIndex = 0;
            //
            // tlpHeader
            //
            this.tlpHeader.ColumnCount = 3;
            this.tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 145F));
            this.tlpHeader.Controls.Add(this.picHeader, 0, 0);
            this.tlpHeader.Controls.Add(this.lblTitle, 1, 0);
            this.tlpHeader.Controls.Add(this.lblSubtitle, 1, 1);
            this.tlpHeader.Controls.Add(this.btnBack, 2, 0);
            this.tlpHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHeader.Location = new System.Drawing.Point(0, 0);
            this.tlpHeader.Name = "tlpHeader";
            this.tlpHeader.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.tlpHeader.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpHeader.RowCount = 2;
            this.tlpHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 58F));
            this.tlpHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 42F));
            this.tlpHeader.Size = new System.Drawing.Size(904, 80);
            this.tlpHeader.TabIndex = 0;
            //
            // picHeader
            //
            this.picHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picHeader.Image = global::FuntinoControlCenter.Properties.Resources.ConnectionSettings;
            this.picHeader.Location = new System.Drawing.Point(808, 12);
            this.picHeader.Margin = new System.Windows.Forms.Padding(8);
            this.picHeader.Name = "picHeader";
            this.picHeader.Size = new System.Drawing.Size(80, 25);
            this.picHeader.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picHeader.TabIndex = 0;
            this.picHeader.TabStop = false;
            //
            // lblTitle
            //
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(156, 4);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblTitle.Size = new System.Drawing.Size(641, 41);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "تنظیم ارتباط شعبه با فانتینو";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblSubtitle
            //
            this.lblSubtitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSubtitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(230)))), ((int)(((byte)(217)))));
            this.lblSubtitle.Location = new System.Drawing.Point(156, 45);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblSubtitle.Size = new System.Drawing.Size(641, 31);
            this.lblSubtitle.TabIndex = 2;
            this.lblSubtitle.Text = "نشانی امن و شناسه‌های اختصاصی این نصب را وارد کنید";
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // btnBack
            //
            this.btnBack.BackColor = System.Drawing.Color.White;
            this.btnBack.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(177)))), ((int)(((byte)(208)))), ((int)(((byte)(192)))));
            this.btnBack.BorderSize = 1;
            this.btnBack.CornerRadius = 11;
            this.btnBack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnBack.FlatAppearance.BorderSize = 0;
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnBack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(69)))), ((int)(((byte)(59)))));
            this.btnBack.Image = global::FuntinoControlCenter.Properties.Resources.Exit;
            this.btnBack.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnBack.Location = new System.Drawing.Point(13, 11);
            this.btnBack.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(135, 27);
            this.btnBack.TabIndex = 3;
            this.btnBack.Text = "بازگشت";
            this.btnBack.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnBack.UseVisualStyleBackColor = false;
            this.btnBack.Click += new System.EventHandler(this.Back_Click);
            //
            // tlpFields
            //
            this.tlpFields.BackColor = System.Drawing.Color.White;
            this.tlpFields.ColumnCount = 2;
            this.tlpFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 72F));
            this.tlpFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28F));
            this.tlpFields.Controls.Add(this.txtBaseUrl, 0, 0);
            this.tlpFields.Controls.Add(this.lblBaseUrl, 1, 0);
            this.tlpFields.Controls.Add(this.txtCredential, 0, 1);
            this.tlpFields.Controls.Add(this.lblCredential, 1, 1);
            this.tlpFields.Controls.Add(this.txtOrganization, 0, 2);
            this.tlpFields.Controls.Add(this.lblOrganization, 1, 2);
            this.tlpFields.Controls.Add(this.txtBranch, 0, 3);
            this.tlpFields.Controls.Add(this.lblBranch, 1, 3);
            this.tlpFields.Controls.Add(this.txtSecret, 0, 4);
            this.tlpFields.Controls.Add(this.lblSecret, 1, 4);
            this.tlpFields.Controls.Add(this.lblSecretState, 0, 5);
            this.tlpFields.Controls.Add(this.lblSourceInstance, 0, 6);
            this.tlpFields.Controls.Add(this.lblHelp, 0, 7);
            this.tlpFields.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpFields.Location = new System.Drawing.Point(31, 117);
            this.tlpFields.Name = "tlpFields";
            this.tlpFields.Padding = new System.Windows.Forms.Padding(20, 20, 20, 10);
            this.tlpFields.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpFields.RowCount = 8;
            this.tlpFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
            this.tlpFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
            this.tlpFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
            this.tlpFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
            this.tlpFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
            this.tlpFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tlpFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tlpFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFields.Size = new System.Drawing.Size(898, 416);
            this.tlpFields.TabIndex = 1;
            //
            // txtBaseUrl
            //
            this.txtBaseUrl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(252)))), ((int)(((byte)(250)))));
            this.txtBaseUrl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtBaseUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtBaseUrl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtBaseUrl.Location = new System.Drawing.Point(269, 32);
            this.txtBaseUrl.Margin = new System.Windows.Forms.Padding(8, 12, 8, 12);
            this.txtBaseUrl.Name = "txtBaseUrl";
            this.txtBaseUrl.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtBaseUrl.Size = new System.Drawing.Size(601, 21);
            this.txtBaseUrl.TabIndex = 0;
            //
            // lblBaseUrl
            //
            this.lblBaseUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBaseUrl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblBaseUrl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(69)))), ((int)(((byte)(58)))));
            this.lblBaseUrl.Location = new System.Drawing.Point(28, 24);
            this.lblBaseUrl.Margin = new System.Windows.Forms.Padding(10, 4, 8, 4);
            this.lblBaseUrl.Name = "lblBaseUrl";
            this.lblBaseUrl.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblBaseUrl.Size = new System.Drawing.Size(223, 54);
            this.lblBaseUrl.TabIndex = 1;
            this.lblBaseUrl.Text = "نشانی سرویس امن";
            this.lblBaseUrl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // txtCredential
            //
            this.txtCredential.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(252)))), ((int)(((byte)(250)))));
            this.txtCredential.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCredential.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCredential.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtCredential.Location = new System.Drawing.Point(269, 94);
            this.txtCredential.Margin = new System.Windows.Forms.Padding(8, 12, 8, 12);
            this.txtCredential.Name = "txtCredential";
            this.txtCredential.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtCredential.Size = new System.Drawing.Size(601, 21);
            this.txtCredential.TabIndex = 2;
            //
            // lblCredential
            //
            this.lblCredential.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCredential.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblCredential.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(69)))), ((int)(((byte)(58)))));
            this.lblCredential.Location = new System.Drawing.Point(28, 86);
            this.lblCredential.Margin = new System.Windows.Forms.Padding(10, 4, 8, 4);
            this.lblCredential.Name = "lblCredential";
            this.lblCredential.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblCredential.Size = new System.Drawing.Size(223, 54);
            this.lblCredential.TabIndex = 3;
            this.lblCredential.Text = "شناسهٔ اعتبارنامه";
            this.lblCredential.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // txtOrganization
            //
            this.txtOrganization.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(252)))), ((int)(((byte)(250)))));
            this.txtOrganization.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtOrganization.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtOrganization.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtOrganization.Location = new System.Drawing.Point(269, 156);
            this.txtOrganization.Margin = new System.Windows.Forms.Padding(8, 12, 8, 12);
            this.txtOrganization.Name = "txtOrganization";
            this.txtOrganization.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtOrganization.Size = new System.Drawing.Size(601, 21);
            this.txtOrganization.TabIndex = 4;
            //
            // lblOrganization
            //
            this.lblOrganization.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblOrganization.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblOrganization.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(69)))), ((int)(((byte)(58)))));
            this.lblOrganization.Location = new System.Drawing.Point(28, 148);
            this.lblOrganization.Margin = new System.Windows.Forms.Padding(10, 4, 8, 4);
            this.lblOrganization.Name = "lblOrganization";
            this.lblOrganization.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblOrganization.Size = new System.Drawing.Size(223, 54);
            this.lblOrganization.TabIndex = 5;
            this.lblOrganization.Text = "شناسهٔ سازمان";
            this.lblOrganization.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // txtBranch
            //
            this.txtBranch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(252)))), ((int)(((byte)(250)))));
            this.txtBranch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtBranch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtBranch.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtBranch.Location = new System.Drawing.Point(269, 218);
            this.txtBranch.Margin = new System.Windows.Forms.Padding(8, 12, 8, 12);
            this.txtBranch.Name = "txtBranch";
            this.txtBranch.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtBranch.Size = new System.Drawing.Size(601, 21);
            this.txtBranch.TabIndex = 6;
            //
            // lblBranch
            //
            this.lblBranch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBranch.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblBranch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(69)))), ((int)(((byte)(58)))));
            this.lblBranch.Location = new System.Drawing.Point(28, 210);
            this.lblBranch.Margin = new System.Windows.Forms.Padding(10, 4, 8, 4);
            this.lblBranch.Name = "lblBranch";
            this.lblBranch.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblBranch.Size = new System.Drawing.Size(223, 54);
            this.lblBranch.TabIndex = 7;
            this.lblBranch.Text = "شناسهٔ شعبه";
            this.lblBranch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // txtSecret
            //
            this.txtSecret.AccessibleDescription = "برای چرخش کلید، مقدار تازه را وارد کنید؛ کلید ذخیره‌شده نمایش داده نمی‌شود.";
            this.txtSecret.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(252)))), ((int)(((byte)(250)))));
            this.txtSecret.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSecret.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSecret.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtSecret.Location = new System.Drawing.Point(269, 280);
            this.txtSecret.Margin = new System.Windows.Forms.Padding(8, 12, 8, 12);
            this.txtSecret.Name = "txtSecret";
            this.txtSecret.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtSecret.Size = new System.Drawing.Size(601, 21);
            this.txtSecret.TabIndex = 8;
            this.txtSecret.UseSystemPasswordChar = true;
            //
            // lblSecret
            //
            this.lblSecret.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSecret.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblSecret.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(69)))), ((int)(((byte)(58)))));
            this.lblSecret.Location = new System.Drawing.Point(28, 272);
            this.lblSecret.Margin = new System.Windows.Forms.Padding(10, 4, 8, 4);
            this.lblSecret.Name = "lblSecret";
            this.lblSecret.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblSecret.Size = new System.Drawing.Size(223, 54);
            this.lblSecret.TabIndex = 9;
            this.lblSecret.Text = "کلید HMAC به‌صورت Base64";
            this.lblSecret.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblSecretState
            //
            this.lblSecretState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSecretState.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblSecretState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(86)))), ((int)(((byte)(69)))));
            this.lblSecretState.Location = new System.Drawing.Point(264, 330);
            this.lblSecretState.Name = "lblSecretState";
            this.lblSecretState.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblSecretState.Size = new System.Drawing.Size(611, 42);
            this.lblSecretState.TabIndex = 10;
            this.lblSecretState.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblSourceInstance
            //
            this.lblSourceInstance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSourceInstance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.lblSourceInstance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(73)))), ((int)(((byte)(95)))), ((int)(((byte)(85)))));
            this.lblSourceInstance.Location = new System.Drawing.Point(264, 372);
            this.lblSourceInstance.Name = "lblSourceInstance";
            this.lblSourceInstance.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblSourceInstance.Size = new System.Drawing.Size(611, 42);
            this.lblSourceInstance.TabIndex = 11;
            this.lblSourceInstance.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblHelp
            //
            this.lblHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F);
            this.lblHelp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(91)))), ((int)(((byte)(111)))), ((int)(((byte)(101)))));
            this.lblHelp.Location = new System.Drawing.Point(264, 414);
            this.lblHelp.Name = "lblHelp";
            this.lblHelp.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblHelp.Size = new System.Drawing.Size(611, 1);
            this.lblHelp.TabIndex = 12;
            this.lblHelp.Text = "کلید در فایل تنظیمات با حفاظت Windows DPAPI ذخیره می‌شود و در گزارش‌ها نمایش داده" +
    " نمی‌شود.";
            this.lblHelp.TextAlign = System.Drawing.ContentAlignment.TopRight;
            //
            // tlpBusy
            //
            this.tlpBusy.ColumnCount = 2;
            this.tlpBusy.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38F));
            this.tlpBusy.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 62F));
            this.tlpBusy.Controls.Add(this.progressBusy, 0, 0);
            this.tlpBusy.Controls.Add(this.lblBusy, 1, 0);
            this.tlpBusy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBusy.Location = new System.Drawing.Point(31, 539);
            this.tlpBusy.Name = "tlpBusy";
            this.tlpBusy.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpBusy.RowCount = 1;
            this.tlpBusy.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBusy.Size = new System.Drawing.Size(898, 28);
            this.tlpBusy.TabIndex = 2;
            //
            // progressBusy
            //
            this.progressBusy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBusy.Location = new System.Drawing.Point(565, 8);
            this.progressBusy.Margin = new System.Windows.Forms.Padding(8);
            this.progressBusy.MarqueeAnimationSpeed = 28;
            this.progressBusy.Name = "progressBusy";
            this.progressBusy.Size = new System.Drawing.Size(325, 12);
            this.progressBusy.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBusy.TabIndex = 0;
            this.progressBusy.Visible = false;
            //
            // lblBusy
            //
            this.lblBusy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBusy.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.lblBusy.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(86)))), ((int)(((byte)(69)))));
            this.lblBusy.Location = new System.Drawing.Point(3, 0);
            this.lblBusy.Name = "lblBusy";
            this.lblBusy.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblBusy.Size = new System.Drawing.Size(551, 28);
            this.lblBusy.TabIndex = 1;
            this.lblBusy.Text = "در حال ذخیرهٔ امن تنظیمات...";
            this.lblBusy.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblBusy.Visible = false;
            //
            // tlpButtons
            //
            this.tlpButtons.ColumnCount = 3;
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 165F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 165F));
            this.tlpButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.Controls.Add(this.btnSave, 0, 0);
            this.tlpButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpButtons.Location = new System.Drawing.Point(31, 573);
            this.tlpButtons.Name = "tlpButtons";
            this.tlpButtons.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpButtons.RowCount = 1;
            this.tlpButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtons.Size = new System.Drawing.Size(898, 54);
            this.tlpButtons.TabIndex = 3;
            //
            // btnSave
            //
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(89)))), ((int)(((byte)(81)))));
            this.btnSave.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(177)))), ((int)(((byte)(208)))), ((int)(((byte)(192)))));
            this.btnSave.BorderSize = 1;
            this.btnSave.CornerRadius = 11;
            this.btnSave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Image = global::FuntinoControlCenter.Properties.Resources.SaveSettings;
            this.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSave.Location = new System.Drawing.Point(738, 7);
            this.btnSave.Margin = new System.Windows.Forms.Padding(5, 7, 5, 7);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(155, 40);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "ذخیرهٔ تنظیمات";
            this.btnSave.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.Save_Click);
            //
            // EdgeConfigurationForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 650);
            this.Controls.Add(this.tlpRoot);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.MinimumSize = new System.Drawing.Size(820, 570);
            this.Name = "EdgeConfigurationForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "تنظیم ارتباط فانتینو";
            this.Load += new System.EventHandler(this.EdgeConfigurationForm_Load);
            this.tlpRoot.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.tlpHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picHeader)).EndInit();
            this.tlpFields.ResumeLayout(false);
            this.tlpFields.PerformLayout();
            this.tlpBusy.ResumeLayout(false);
            this.tlpButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

    }
}
