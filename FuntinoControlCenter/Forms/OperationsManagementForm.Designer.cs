namespace FuntinoControlCenter.Forms
{
    partial class OperationsManagementForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.TableLayoutPanel tlpTiles;
        private System.Windows.Forms.Panel pnlContent;
        private FuntinoControlCenter.UI.RoundedButton btnAttractions;
        private FuntinoControlCenter.UI.RoundedButton btnControllerDevices;
        private FuntinoControlCenter.UI.RoundedButton btnPromotions;
        private FuntinoControlCenter.UI.RoundedButton btnDevicePromotions;
        private FuntinoControlCenter.UI.RoundedButton btnDiscountPolicies;
        private FuntinoControlCenter.UI.RoundedButton btnDiscountRates;
        private FuntinoControlCenter.UI.RoundedButton btnDeposits;
        private FuntinoControlCenter.UI.RoundedButton btnRechargeLimit;
        private FuntinoControlCenter.UI.RoundedButton btnHourlyPlans;
        private FuntinoControlCenter.UI.RoundedButton btnSchedules;
        private FuntinoControlCenter.UI.RoundedButton btnTickets;
        private FuntinoControlCenter.UI.RoundedButton btnClose;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.tlpTiles = new System.Windows.Forms.TableLayoutPanel();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.btnAttractions = new FuntinoControlCenter.UI.RoundedButton();
            this.btnControllerDevices = new FuntinoControlCenter.UI.RoundedButton();
            this.btnPromotions = new FuntinoControlCenter.UI.RoundedButton();
            this.btnDevicePromotions = new FuntinoControlCenter.UI.RoundedButton();
            this.btnDiscountPolicies = new FuntinoControlCenter.UI.RoundedButton();
            this.btnDiscountRates = new FuntinoControlCenter.UI.RoundedButton();
            this.btnDeposits = new FuntinoControlCenter.UI.RoundedButton();
            this.btnRechargeLimit = new FuntinoControlCenter.UI.RoundedButton();
            this.btnHourlyPlans = new FuntinoControlCenter.UI.RoundedButton();
            this.btnSchedules = new FuntinoControlCenter.UI.RoundedButton();
            this.btnTickets = new FuntinoControlCenter.UI.RoundedButton();
            this.btnClose = new FuntinoControlCenter.UI.RoundedButton();
            this.pnlHeader.SuspendLayout();
            this.tlpTiles.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.SuspendLayout();

            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(18, 55, 55);
            this.pnlHeader.Controls.Add(this.btnClose);
            this.pnlHeader.Controls.Add(this.lblSubtitle);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(28, 12, 28, 12);
            this.pnlHeader.Size = new System.Drawing.Size(1320, 116);
            this.pnlHeader.TabIndex = 0;

            this.lblTitle.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 20F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(30, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblTitle.Size = new System.Drawing.Size(1000, 48);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "مدیریت عملیات شهربازی";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            this.lblSubtitle.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(215, 231, 224);
            this.lblSubtitle.Location = new System.Drawing.Point(31, 63);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblSubtitle.Size = new System.Drawing.Size(1000, 32);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "دستگاه‌ها، جشنواره‌ها، تخفیف‌ها و تنظیمات شارژ را از بخش مربوط مدیریت کنید.";
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            this.btnClose.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
            this.btnClose.BorderColor = System.Drawing.Color.FromArgb(192, 212, 203);
            this.btnClose.BorderSize = 1;
            this.btnClose.CornerRadius = 12;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(30, 73, 66);
            this.btnClose.Image = global::FuntinoControlCenter.Properties.Resources.ActionBack;
            this.btnClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClose.Location = new System.Drawing.Point(1103, 30);
            this.btnClose.Name = "btnClose";
            this.btnClose.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
            this.btnClose.Size = new System.Drawing.Size(184, 48);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "بازگشت";
            this.btnClose.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnClose.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.Close_Click);

            this.pnlContent.BackColor = System.Drawing.Color.Transparent;
            this.pnlContent.Controls.Add(this.tlpTiles);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(0, 116);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(22, 18, 22, 20);
            this.pnlContent.Size = new System.Drawing.Size(1320, 734);
            this.pnlContent.TabIndex = 1;

            this.tlpTiles.BackColor = System.Drawing.Color.Transparent;
            this.tlpTiles.ColumnCount = 3;
            this.tlpTiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.333F));
            this.tlpTiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.333F));
            this.tlpTiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.334F));
            this.tlpTiles.Controls.Add(this.btnAttractions, 0, 0);
            this.tlpTiles.Controls.Add(this.btnControllerDevices, 1, 0);
            this.tlpTiles.Controls.Add(this.btnPromotions, 2, 0);
            this.tlpTiles.Controls.Add(this.btnDevicePromotions, 0, 1);
            this.tlpTiles.Controls.Add(this.btnDiscountPolicies, 1, 1);
            this.tlpTiles.Controls.Add(this.btnDiscountRates, 2, 1);
            this.tlpTiles.Controls.Add(this.btnDeposits, 0, 2);
            this.tlpTiles.Controls.Add(this.btnRechargeLimit, 1, 2);
            this.tlpTiles.Controls.Add(this.btnHourlyPlans, 2, 2);
            this.tlpTiles.Controls.Add(this.btnSchedules, 0, 3);
            this.tlpTiles.Controls.Add(this.btnTickets, 1, 3);
            this.tlpTiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTiles.Location = new System.Drawing.Point(22, 18);
            this.tlpTiles.Name = "tlpTiles";
            this.tlpTiles.Padding = new System.Windows.Forms.Padding(6);
            this.tlpTiles.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpTiles.RowCount = 4;
            this.tlpTiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpTiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpTiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpTiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpTiles.Size = new System.Drawing.Size(1276, 696);
            this.tlpTiles.TabIndex = 0;

            this.btnAttractions.BackColor = this.btnControllerDevices.BackColor = this.btnPromotions.BackColor = this.btnDevicePromotions.BackColor = this.btnDiscountPolicies.BackColor = this.btnDiscountRates.BackColor = this.btnDeposits.BackColor = this.btnRechargeLimit.BackColor = this.btnHourlyPlans.BackColor = this.btnSchedules.BackColor = this.btnTickets.BackColor = System.Drawing.Color.White;
            this.btnAttractions.BorderColor = this.btnControllerDevices.BorderColor = this.btnPromotions.BorderColor = this.btnDevicePromotions.BorderColor = this.btnDiscountPolicies.BorderColor = this.btnDiscountRates.BorderColor = this.btnDeposits.BorderColor = this.btnRechargeLimit.BorderColor = this.btnHourlyPlans.BorderColor = this.btnSchedules.BorderColor = this.btnTickets.BorderColor = System.Drawing.Color.FromArgb(190, 211, 201);
            this.btnAttractions.BorderSize = this.btnControllerDevices.BorderSize = this.btnPromotions.BorderSize = this.btnDevicePromotions.BorderSize = this.btnDiscountPolicies.BorderSize = this.btnDiscountRates.BorderSize = this.btnDeposits.BorderSize = this.btnRechargeLimit.BorderSize = this.btnHourlyPlans.BorderSize = this.btnSchedules.BorderSize = this.btnTickets.BorderSize = 1;
            this.btnAttractions.CornerRadius = this.btnControllerDevices.CornerRadius = this.btnPromotions.CornerRadius = this.btnDevicePromotions.CornerRadius = this.btnDiscountPolicies.CornerRadius = this.btnDiscountRates.CornerRadius = this.btnDeposits.CornerRadius = this.btnRechargeLimit.CornerRadius = this.btnHourlyPlans.CornerRadius = this.btnSchedules.CornerRadius = this.btnTickets.CornerRadius = 16;
            this.btnAttractions.Dock = this.btnControllerDevices.Dock = this.btnPromotions.Dock = this.btnDevicePromotions.Dock = this.btnDiscountPolicies.Dock = this.btnDiscountRates.Dock = this.btnDeposits.Dock = this.btnRechargeLimit.Dock = this.btnHourlyPlans.Dock = this.btnSchedules.Dock = this.btnTickets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAttractions.FlatAppearance.BorderSize = this.btnControllerDevices.FlatAppearance.BorderSize = this.btnPromotions.FlatAppearance.BorderSize = this.btnDevicePromotions.FlatAppearance.BorderSize = this.btnDiscountPolicies.FlatAppearance.BorderSize = this.btnDiscountRates.FlatAppearance.BorderSize = this.btnDeposits.FlatAppearance.BorderSize = this.btnRechargeLimit.FlatAppearance.BorderSize = this.btnHourlyPlans.FlatAppearance.BorderSize = this.btnSchedules.FlatAppearance.BorderSize = this.btnTickets.FlatAppearance.BorderSize = 0;
            this.btnAttractions.FlatStyle = this.btnControllerDevices.FlatStyle = this.btnPromotions.FlatStyle = this.btnDevicePromotions.FlatStyle = this.btnDiscountPolicies.FlatStyle = this.btnDiscountRates.FlatStyle = this.btnDeposits.FlatStyle = this.btnRechargeLimit.FlatStyle = this.btnHourlyPlans.FlatStyle = this.btnSchedules.FlatStyle = this.btnTickets.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAttractions.Font = this.btnControllerDevices.Font = this.btnPromotions.Font = this.btnDevicePromotions.Font = this.btnDiscountPolicies.Font = this.btnDiscountRates.Font = this.btnDeposits.Font = this.btnRechargeLimit.Font = this.btnHourlyPlans.Font = this.btnSchedules.Font = this.btnTickets.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 11F, System.Drawing.FontStyle.Bold);
            this.btnAttractions.ForeColor = this.btnControllerDevices.ForeColor = this.btnPromotions.ForeColor = this.btnDevicePromotions.ForeColor = this.btnDiscountPolicies.ForeColor = this.btnDiscountRates.ForeColor = this.btnDeposits.ForeColor = this.btnRechargeLimit.ForeColor = this.btnHourlyPlans.ForeColor = this.btnSchedules.ForeColor = this.btnTickets.ForeColor = System.Drawing.Color.FromArgb(28, 76, 69);
            this.btnAttractions.Image = global::FuntinoControlCenter.Properties.Resources.ManagementAttractions;
            this.btnControllerDevices.Image = global::FuntinoControlCenter.Properties.Resources.ManagementControllerDevices;
            this.btnPromotions.Image = global::FuntinoControlCenter.Properties.Resources.ManagementPromotions;
            this.btnDevicePromotions.Image = global::FuntinoControlCenter.Properties.Resources.ManagementPromotionAssignments;
            this.btnDiscountPolicies.Image = global::FuntinoControlCenter.Properties.Resources.ManagementDiscountPolicies;
            this.btnDiscountRates.Image = global::FuntinoControlCenter.Properties.Resources.ManagementDiscountRates;
            this.btnDeposits.Image = global::FuntinoControlCenter.Properties.Resources.ManagementDeposits;
            this.btnRechargeLimit.Image = global::FuntinoControlCenter.Properties.Resources.ManagementRechargeLimit;
            this.btnHourlyPlans.Image = global::FuntinoControlCenter.Properties.Resources.ManagementHourlyPlans;
            this.btnSchedules.Image = global::FuntinoControlCenter.Properties.Resources.ManagementRechargeSchedule;
            this.btnTickets.Image = global::FuntinoControlCenter.Properties.Resources.ManagementTickets;
            this.btnAttractions.ImageAlign = this.btnControllerDevices.ImageAlign = this.btnPromotions.ImageAlign = this.btnDevicePromotions.ImageAlign = this.btnDiscountPolicies.ImageAlign = this.btnDiscountRates.ImageAlign = this.btnDeposits.ImageAlign = this.btnRechargeLimit.ImageAlign = this.btnHourlyPlans.ImageAlign = this.btnSchedules.ImageAlign = this.btnTickets.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAttractions.Margin = this.btnControllerDevices.Margin = this.btnPromotions.Margin = this.btnDevicePromotions.Margin = this.btnDiscountPolicies.Margin = this.btnDiscountRates.Margin = this.btnDeposits.Margin = this.btnRechargeLimit.Margin = this.btnHourlyPlans.Margin = this.btnSchedules.Margin = this.btnTickets.Margin = new System.Windows.Forms.Padding(10);
            this.btnAttractions.Padding = this.btnControllerDevices.Padding = this.btnPromotions.Padding = this.btnDevicePromotions.Padding = this.btnDiscountPolicies.Padding = this.btnDiscountRates.Padding = this.btnDeposits.Padding = this.btnRechargeLimit.Padding = this.btnHourlyPlans.Padding = this.btnSchedules.Padding = this.btnTickets.Padding = new System.Windows.Forms.Padding(18, 0, 18, 0);
            this.btnAttractions.RightToLeft = this.btnControllerDevices.RightToLeft = this.btnPromotions.RightToLeft = this.btnDevicePromotions.RightToLeft = this.btnDiscountPolicies.RightToLeft = this.btnDiscountRates.RightToLeft = this.btnDeposits.RightToLeft = this.btnRechargeLimit.RightToLeft = this.btnHourlyPlans.RightToLeft = this.btnSchedules.RightToLeft = this.btnTickets.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.btnAttractions.TextAlign = this.btnControllerDevices.TextAlign = this.btnPromotions.TextAlign = this.btnDevicePromotions.TextAlign = this.btnDiscountPolicies.TextAlign = this.btnDiscountRates.TextAlign = this.btnDeposits.TextAlign = this.btnRechargeLimit.TextAlign = this.btnHourlyPlans.TextAlign = this.btnSchedules.TextAlign = this.btnTickets.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnAttractions.TextImageRelation = this.btnControllerDevices.TextImageRelation = this.btnPromotions.TextImageRelation = this.btnDevicePromotions.TextImageRelation = this.btnDiscountPolicies.TextImageRelation = this.btnDiscountRates.TextImageRelation = this.btnDeposits.TextImageRelation = this.btnRechargeLimit.TextImageRelation = this.btnHourlyPlans.TextImageRelation = this.btnSchedules.TextImageRelation = this.btnTickets.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAttractions.UseVisualStyleBackColor = this.btnControllerDevices.UseVisualStyleBackColor = this.btnPromotions.UseVisualStyleBackColor = this.btnDevicePromotions.UseVisualStyleBackColor = this.btnDiscountPolicies.UseVisualStyleBackColor = this.btnDiscountRates.UseVisualStyleBackColor = this.btnDeposits.UseVisualStyleBackColor = this.btnRechargeLimit.UseVisualStyleBackColor = this.btnHourlyPlans.UseVisualStyleBackColor = this.btnSchedules.UseVisualStyleBackColor = this.btnTickets.UseVisualStyleBackColor = false;
            this.btnAttractions.Name = "btnAttractions"; this.btnAttractions.Text = "دستگاه‌های تفریحی"; this.btnAttractions.TabIndex = 0;
            this.btnControllerDevices.Name = "btnControllerDevices"; this.btnControllerDevices.Text = "دستگاه‌های کنترل‌شونده"; this.btnControllerDevices.TabIndex = 1;
            this.btnPromotions.Name = "btnPromotions"; this.btnPromotions.Text = "جشنواره‌ها و تخفیف‌ها"; this.btnPromotions.TabIndex = 2;
            this.btnDevicePromotions.Name = "btnDevicePromotions"; this.btnDevicePromotions.Text = "تخصیص جشنواره به دستگاه"; this.btnDevicePromotions.TabIndex = 3;
            this.btnDiscountPolicies.Name = "btnDiscountPolicies"; this.btnDiscountPolicies.Text = "قوانین تخفیف روزانه"; this.btnDiscountPolicies.TabIndex = 4;
            this.btnDiscountRates.Name = "btnDiscountRates"; this.btnDiscountRates.Text = "درصدهای سطح تخفیف"; this.btnDiscountRates.TabIndex = 5;
            this.btnDeposits.Name = "btnDeposits"; this.btnDeposits.Text = "مبلغ بیعانه"; this.btnDeposits.TabIndex = 6;
            this.btnRechargeLimit.Name = "btnRechargeLimit"; this.btnRechargeLimit.Text = "سقف هر بار شارژ"; this.btnRechargeLimit.TabIndex = 7;
            this.btnHourlyPlans.Name = "btnHourlyPlans"; this.btnHourlyPlans.Text = "گزینه‌های شارژ ساعتی"; this.btnHourlyPlans.TabIndex = 8;
            this.btnSchedules.Name = "btnSchedules"; this.btnSchedules.Text = "زمان‌بندی شارژ"; this.btnSchedules.TabIndex = 9;
            this.btnTickets.Name = "btnTickets"; this.btnTickets.Text = "مبلغ و تنظیمات بلیت"; this.btnTickets.TabIndex = 10;
            this.btnAttractions.Click += new System.EventHandler(this.Attractions_Click);
            this.btnControllerDevices.Click += new System.EventHandler(this.ControllerDevices_Click);
            this.btnPromotions.Click += new System.EventHandler(this.Promotions_Click);
            this.btnDevicePromotions.Click += new System.EventHandler(this.DevicePromotions_Click);
            this.btnDiscountPolicies.Click += new System.EventHandler(this.DiscountPolicies_Click);
            this.btnDiscountRates.Click += new System.EventHandler(this.DiscountRates_Click);
            this.btnDeposits.Click += new System.EventHandler(this.Deposits_Click);
            this.btnRechargeLimit.Click += new System.EventHandler(this.RechargeLimit_Click);
            this.btnHourlyPlans.Click += new System.EventHandler(this.HourlyPlans_Click);
            this.btnSchedules.Click += new System.EventHandler(this.Schedules_Click);
            this.btnTickets.Click += new System.EventHandler(this.Tickets_Click);

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(236, 243, 238);
            this.BackgroundImage = global::FuntinoControlCenter.Properties.Resources.SettingsBackground;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1320, 850);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9F);
            this.MinimumSize = new System.Drawing.Size(1150, 760);
            this.Name = "OperationsManagementForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "مدیریت عملیات";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.OperationsManagementForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.tlpTiles.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.ResumeLayout(false);
        }

    }
}
