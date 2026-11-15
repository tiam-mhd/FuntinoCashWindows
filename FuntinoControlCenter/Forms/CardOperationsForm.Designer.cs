namespace FuntinoControlCenter.Forms
{
    partial class CardOperationsForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlHeader;
        private FuntinoControlCenter.UI.RoundedBorderPanel pnlSummary, pnlActions, pnlAmountInput, pnlRechargeTerms, pnlCardHistory, pnlDatabaseHistory, pnlSearchInput;
        private System.Windows.Forms.Label lblTitle, lblSubtitle, lblCardNumberCaption, lblCardNumberValue, lblBalanceCaption, lblBalanceValue;
        private System.Windows.Forms.Label lblCardTypeCaption, lblCardTypeValue, lblDiscountCaption, lblDiscountValue, lblExpirationCaption, lblExpirationValue, lblOperationStatus, lblAmountCaption, lblHistoryPages;
        private FuntinoControlCenter.UI.RoundedButton btnReadInfo, btnReadBalance, btnReadRemainder, btnReadHistory, btnRecharge, btnCardHistoryTab, btnDatabaseHistoryTab, btnSearchHistory, btnPreviousHistory, btnNextHistory, btnClose;
        private System.Windows.Forms.TextBox txtAmount, txtHistorySearch;
        private System.Windows.Forms.ComboBox cmbHistoryFilter;
        private System.Windows.Forms.DataGridView gridCardActivity, gridRechargeHistory;
        private System.Windows.Forms.DataGridViewTextBoxColumn colActivityType, colActivityAmount, colActivityDate, colActivityTime, colActivitySource;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRechargeType, colRechargeAmount, colRechargeDate, colRechargeUser, colRechargeId;
        private System.ComponentModel.BackgroundWorker operationWorker;
        private System.Windows.Forms.Timer authorizationTimer;
        private System.Windows.Forms.TableLayoutPanel tlpRoot, tlpHeader, tlpHeaderText, tlpSummary, tlpActions, tlpActionButtons, tlpHistoryHost, tlpDatabaseHistory, tlpDatabaseToolbar;
        private System.Windows.Forms.FlowLayoutPanel flpHistoryTabs;
        private System.Windows.Forms.Label lblAmountHint, lblPromotionCaption, lblRechargeTerms;
        private System.Windows.Forms.ComboBox cmbPromotion;
        private FuntinoControlCenter.UI.AppLoadingIndicator loadingOperation;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.tlpHeader = new System.Windows.Forms.TableLayoutPanel();
            this.btnClose = new FuntinoControlCenter.UI.RoundedButton();
            this.tlpHeaderText = new System.Windows.Forms.TableLayoutPanel();
            this.pnlSummary = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.lblCardNumberCaption = new System.Windows.Forms.Label();
            this.lblCardNumberValue = new System.Windows.Forms.Label();
            this.lblBalanceCaption = new System.Windows.Forms.Label();
            this.lblBalanceValue = new System.Windows.Forms.Label();
            this.lblCardTypeCaption = new System.Windows.Forms.Label();
            this.lblCardTypeValue = new System.Windows.Forms.Label();
            this.lblDiscountCaption = new System.Windows.Forms.Label();
            this.lblDiscountValue = new System.Windows.Forms.Label();
            this.lblExpirationCaption = new System.Windows.Forms.Label();
            this.lblExpirationValue = new System.Windows.Forms.Label();
            this.lblOperationStatus = new System.Windows.Forms.Label();
            this.tlpSummary = new System.Windows.Forms.TableLayoutPanel();
            this.pnlActions = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.pnlRechargeTerms = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.lblPromotionCaption = new System.Windows.Forms.Label();
            this.lblRechargeTerms = new System.Windows.Forms.Label();
            this.cmbPromotion = new System.Windows.Forms.ComboBox();
            this.loadingOperation = new FuntinoControlCenter.UI.AppLoadingIndicator();
            this.btnReadInfo = new FuntinoControlCenter.UI.RoundedButton();
            this.btnReadBalance = new FuntinoControlCenter.UI.RoundedButton();
            this.btnReadRemainder = new FuntinoControlCenter.UI.RoundedButton();
            this.btnReadHistory = new FuntinoControlCenter.UI.RoundedButton();
            this.btnRecharge = new FuntinoControlCenter.UI.RoundedButton();
            this.lblAmountCaption = new System.Windows.Forms.Label();
            this.pnlAmountInput = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.txtAmount = new System.Windows.Forms.TextBox();
            this.tlpActions = new System.Windows.Forms.TableLayoutPanel();
            this.lblAmountHint = new System.Windows.Forms.Label();
            this.tlpActionButtons = new System.Windows.Forms.TableLayoutPanel();
            this.pnlCardHistory = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.gridCardActivity = new System.Windows.Forms.DataGridView();
            this.colActivityType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colActivityAmount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colActivityDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colActivityTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colActivitySource = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlDatabaseHistory = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.pnlSearchInput = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.txtHistorySearch = new System.Windows.Forms.TextBox();
            this.cmbHistoryFilter = new System.Windows.Forms.ComboBox();
            this.btnSearchHistory = new FuntinoControlCenter.UI.RoundedButton();
            this.btnPreviousHistory = new FuntinoControlCenter.UI.RoundedButton();
            this.btnNextHistory = new FuntinoControlCenter.UI.RoundedButton();
            this.lblHistoryPages = new System.Windows.Forms.Label();
            this.gridRechargeHistory = new System.Windows.Forms.DataGridView();
            this.colRechargeType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRechargeAmount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRechargeDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRechargeUser = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRechargeId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tlpDatabaseHistory = new System.Windows.Forms.TableLayoutPanel();
            this.tlpDatabaseToolbar = new System.Windows.Forms.TableLayoutPanel();
            this.btnCardHistoryTab = new FuntinoControlCenter.UI.RoundedButton();
            this.btnDatabaseHistoryTab = new FuntinoControlCenter.UI.RoundedButton();
            this.tlpRoot = new System.Windows.Forms.TableLayoutPanel();
            this.flpHistoryTabs = new System.Windows.Forms.FlowLayoutPanel();
            this.tlpHistoryHost = new System.Windows.Forms.TableLayoutPanel();
            this.operationWorker = new System.ComponentModel.BackgroundWorker();
            this.authorizationTimer = new System.Windows.Forms.Timer(this.components);
            this.pnlHeader.SuspendLayout();
            this.tlpHeader.SuspendLayout();
            this.tlpHeaderText.SuspendLayout();
            this.pnlSummary.SuspendLayout();
            this.tlpSummary.SuspendLayout();
            this.pnlActions.SuspendLayout();
            this.pnlAmountInput.SuspendLayout();
            this.pnlRechargeTerms.SuspendLayout();
            this.tlpActions.SuspendLayout();
            this.tlpActionButtons.SuspendLayout();
            this.pnlCardHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridCardActivity)).BeginInit();
            this.pnlDatabaseHistory.SuspendLayout();
            this.pnlSearchInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridRechargeHistory)).BeginInit();
            this.tlpDatabaseHistory.SuspendLayout();
            this.tlpDatabaseToolbar.SuspendLayout();
            this.tlpRoot.SuspendLayout();
            this.flpHistoryTabs.SuspendLayout();
            this.tlpHistoryHost.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(55)))), ((int)(((byte)(58)))));
            this.pnlHeader.Controls.Add(this.tlpHeader);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHeader.Location = new System.Drawing.Point(22, 14);
            this.pnlHeader.Margin = new System.Windows.Forms.Padding(0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            this.pnlHeader.Size = new System.Drawing.Size(1156, 68);
            this.pnlHeader.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 17F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(4, 0);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblTitle.Size = new System.Drawing.Size(980, 33);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "عملیات کارت";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblSubtitle
            //
            this.lblSubtitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSubtitle.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(234)))), ((int)(((byte)(226)))));
            this.lblSubtitle.Location = new System.Drawing.Point(4, 33);
            this.lblSubtitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblSubtitle.Size = new System.Drawing.Size(980, 25);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "کارت را روی کارت‌خوان بگذارید و عملیات دلخواه را انتخاب کنید؛ دریافت کارت فقط با " +
    "درخواست شما انجام می‌شود.";
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // tlpHeader
            //
            this.tlpHeader.BackColor = System.Drawing.Color.Transparent;
            this.tlpHeader.ColumnCount = 2;
            this.tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 142F));
            this.tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHeader.Controls.Add(this.btnClose, 0, 0);
            this.tlpHeader.Controls.Add(this.tlpHeaderText, 1, 0);
            this.tlpHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHeader.Location = new System.Drawing.Point(10, 2);
            this.tlpHeader.Name = "tlpHeader";
            this.tlpHeader.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpHeader.RowCount = 1;
            this.tlpHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHeader.Size = new System.Drawing.Size(1136, 64);
            this.tlpHeader.TabIndex = 0;
            //
            // btnClose
            //
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnClose.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(185)))), ((int)(((byte)(202)))), ((int)(((byte)(193)))));
            this.btnClose.BorderSize = 1;
            this.btnClose.CornerRadius = 12;
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(75)))), ((int)(((byte)(68)))));
            this.btnClose.Image = global::FuntinoControlCenter.Properties.Resources.Exit;
            this.btnClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClose.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnClose.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.btnClose.Location = new System.Drawing.Point(998, 8);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4, 8, 4, 8);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(134, 48);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "بازگشت";
            this.btnClose.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.Close_Click);
            //
            // tlpHeaderText
            //
            this.tlpHeaderText.BackColor = System.Drawing.Color.Transparent;
            this.tlpHeaderText.ColumnCount = 1;
            this.tlpHeaderText.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHeaderText.Controls.Add(this.lblTitle, 0, 0);
            this.tlpHeaderText.Controls.Add(this.lblSubtitle, 0, 1);
            this.tlpHeaderText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHeaderText.Location = new System.Drawing.Point(3, 3);
            this.tlpHeaderText.Name = "tlpHeaderText";
            this.tlpHeaderText.RowCount = 2;
            this.tlpHeaderText.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 58F));
            this.tlpHeaderText.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 42F));
            this.tlpHeaderText.Size = new System.Drawing.Size(988, 58);
            this.tlpHeaderText.TabIndex = 8;
            //
            // pnlSummary
            //
            this.pnlSummary.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(254)))), ((int)(((byte)(253)))));
            this.pnlSummary.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(218)))), ((int)(((byte)(208)))));
            this.pnlSummary.BorderSize = 1;
            this.pnlSummary.Controls.Add(this.tlpSummary);
            this.pnlSummary.CornerRadius = 18;
            this.pnlSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSummary.Location = new System.Drawing.Point(22, 90);
            this.pnlSummary.Margin = new System.Windows.Forms.Padding(0, 8, 0, 8);
            this.pnlSummary.Name = "pnlSummary";
            this.pnlSummary.Padding = new System.Windows.Forms.Padding(12, 6, 12, 6);
            this.pnlSummary.Size = new System.Drawing.Size(1156, 100);
            this.pnlSummary.TabIndex = 1;
            //
            // lblCardNumberCaption
            //
            this.lblCardNumberCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCardNumberCaption.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F, System.Drawing.FontStyle.Bold);
            this.lblCardNumberCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(107)))), ((int)(((byte)(99)))));
            this.lblCardNumberCaption.Location = new System.Drawing.Point(864, 0);
            this.lblCardNumberCaption.Name = "lblCardNumberCaption";
            this.lblCardNumberCaption.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblCardNumberCaption.Size = new System.Drawing.Size(265, 27);
            this.lblCardNumberCaption.TabIndex = 0;
            this.lblCardNumberCaption.Text = "شماره کارت";
            this.lblCardNumberCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblCardNumberValue
            //
            this.lblCardNumberValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCardNumberValue.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 10F, System.Drawing.FontStyle.Bold);
            this.lblCardNumberValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(84)))), ((int)(((byte)(72)))));
            this.lblCardNumberValue.Location = new System.Drawing.Point(864, 27);
            this.lblCardNumberValue.Name = "lblCardNumberValue";
            this.lblCardNumberValue.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblCardNumberValue.Size = new System.Drawing.Size(265, 33);
            this.lblCardNumberValue.TabIndex = 1;
            this.lblCardNumberValue.Text = "—";
            this.lblCardNumberValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblBalanceCaption
            //
            this.lblBalanceCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBalanceCaption.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F, System.Drawing.FontStyle.Bold);
            this.lblBalanceCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(107)))), ((int)(((byte)(99)))));
            this.lblBalanceCaption.Location = new System.Drawing.Point(615, 0);
            this.lblBalanceCaption.Name = "lblBalanceCaption";
            this.lblBalanceCaption.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblBalanceCaption.Size = new System.Drawing.Size(243, 27);
            this.lblBalanceCaption.TabIndex = 2;
            this.lblBalanceCaption.Text = "موجودی";
            this.lblBalanceCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblBalanceValue
            //
            this.lblBalanceValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBalanceValue.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 15F, System.Drawing.FontStyle.Bold);
            this.lblBalanceValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(89)))), ((int)(((byte)(81)))));
            this.lblBalanceValue.Location = new System.Drawing.Point(615, 27);
            this.lblBalanceValue.Name = "lblBalanceValue";
            this.lblBalanceValue.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblBalanceValue.Size = new System.Drawing.Size(243, 33);
            this.lblBalanceValue.TabIndex = 3;
            this.lblBalanceValue.Text = "برای خواندن کارت اقدام کنید";
            this.lblBalanceValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblCardTypeCaption
            //
            this.lblCardTypeCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCardTypeCaption.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F, System.Drawing.FontStyle.Bold);
            this.lblCardTypeCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(107)))), ((int)(((byte)(99)))));
            this.lblCardTypeCaption.Location = new System.Drawing.Point(412, 0);
            this.lblCardTypeCaption.Name = "lblCardTypeCaption";
            this.lblCardTypeCaption.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblCardTypeCaption.Size = new System.Drawing.Size(197, 27);
            this.lblCardTypeCaption.TabIndex = 4;
            this.lblCardTypeCaption.Text = "نوع کارت";
            this.lblCardTypeCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblCardTypeValue
            //
            this.lblCardTypeValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCardTypeValue.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F);
            this.lblCardTypeValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(84)))), ((int)(((byte)(72)))));
            this.lblCardTypeValue.Location = new System.Drawing.Point(412, 27);
            this.lblCardTypeValue.Name = "lblCardTypeValue";
            this.lblCardTypeValue.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblCardTypeValue.Size = new System.Drawing.Size(197, 33);
            this.lblCardTypeValue.TabIndex = 5;
            this.lblCardTypeValue.Text = "—";
            this.lblCardTypeValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblDiscountCaption
            //
            this.lblDiscountCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDiscountCaption.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F, System.Drawing.FontStyle.Bold);
            this.lblDiscountCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(107)))), ((int)(((byte)(99)))));
            this.lblDiscountCaption.Location = new System.Drawing.Point(220, 0);
            this.lblDiscountCaption.Name = "lblDiscountCaption";
            this.lblDiscountCaption.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblDiscountCaption.Size = new System.Drawing.Size(186, 27);
            this.lblDiscountCaption.TabIndex = 6;
            this.lblDiscountCaption.Text = "تخفیف";
            this.lblDiscountCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblDiscountValue
            //
            this.lblDiscountValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDiscountValue.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F);
            this.lblDiscountValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(84)))), ((int)(((byte)(72)))));
            this.lblDiscountValue.Location = new System.Drawing.Point(220, 27);
            this.lblDiscountValue.Name = "lblDiscountValue";
            this.lblDiscountValue.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblDiscountValue.Size = new System.Drawing.Size(186, 33);
            this.lblDiscountValue.TabIndex = 7;
            this.lblDiscountValue.Text = "—";
            this.lblDiscountValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblExpirationCaption
            //
            this.lblExpirationCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblExpirationCaption.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F, System.Drawing.FontStyle.Bold);
            this.lblExpirationCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(107)))), ((int)(((byte)(99)))));
            this.lblExpirationCaption.Location = new System.Drawing.Point(3, 0);
            this.lblExpirationCaption.Name = "lblExpirationCaption";
            this.lblExpirationCaption.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblExpirationCaption.Size = new System.Drawing.Size(211, 27);
            this.lblExpirationCaption.TabIndex = 8;
            this.lblExpirationCaption.Text = "پایان اعتبار";
            this.lblExpirationCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblExpirationValue
            //
            this.lblExpirationValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblExpirationValue.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F);
            this.lblExpirationValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(84)))), ((int)(((byte)(72)))));
            this.lblExpirationValue.Location = new System.Drawing.Point(3, 27);
            this.lblExpirationValue.Name = "lblExpirationValue";
            this.lblExpirationValue.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblExpirationValue.Size = new System.Drawing.Size(211, 33);
            this.lblExpirationValue.TabIndex = 9;
            this.lblExpirationValue.Text = "—";
            this.lblExpirationValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblOperationStatus
            //
            this.tlpSummary.SetColumnSpan(this.lblOperationStatus, 5);
            this.lblOperationStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblOperationStatus.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F);
            this.lblOperationStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(112)))), ((int)(((byte)(105)))));
            this.lblOperationStatus.Location = new System.Drawing.Point(3, 60);
            this.lblOperationStatus.Name = "lblOperationStatus";
            this.lblOperationStatus.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblOperationStatus.Size = new System.Drawing.Size(1126, 28);
            this.lblOperationStatus.TabIndex = 10;
            this.lblOperationStatus.Text = "کارت‌خوان فقط پس از انتخاب یکی از عملیات‌ها درخواست کارت می‌فرستد.";
            this.lblOperationStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // tlpSummary
            //
            this.tlpSummary.BackColor = System.Drawing.Color.Transparent;
            this.tlpSummary.ColumnCount = 5;
            this.tlpSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24F));
            this.tlpSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22F));
            this.tlpSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18F));
            this.tlpSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17F));
            this.tlpSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19F));
            this.tlpSummary.Controls.Add(this.lblCardNumberCaption, 0, 0);
            this.tlpSummary.Controls.Add(this.lblBalanceCaption, 1, 0);
            this.tlpSummary.Controls.Add(this.lblCardTypeCaption, 2, 0);
            this.tlpSummary.Controls.Add(this.lblDiscountCaption, 3, 0);
            this.tlpSummary.Controls.Add(this.lblExpirationCaption, 4, 0);
            this.tlpSummary.Controls.Add(this.lblCardNumberValue, 0, 1);
            this.tlpSummary.Controls.Add(this.lblBalanceValue, 1, 1);
            this.tlpSummary.Controls.Add(this.lblCardTypeValue, 2, 1);
            this.tlpSummary.Controls.Add(this.lblDiscountValue, 3, 1);
            this.tlpSummary.Controls.Add(this.lblExpirationValue, 4, 1);
            this.tlpSummary.Controls.Add(this.lblOperationStatus, 0, 2);
            this.tlpSummary.SetColumnSpan(this.loadingOperation, 5);
            this.loadingOperation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loadingOperation.Name = "loadingOperation";
            this.loadingOperation.TabIndex = 11;
            this.loadingOperation.Visible = false;
            this.tlpSummary.Controls.Add(this.loadingOperation, 0, 2);
            this.tlpSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSummary.Location = new System.Drawing.Point(12, 6);
            this.tlpSummary.Name = "tlpSummary";
            this.tlpSummary.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpSummary.RowCount = 3;
            this.tlpSummary.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 31F));
            this.tlpSummary.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 38F));
            this.tlpSummary.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 31F));
            this.tlpSummary.Size = new System.Drawing.Size(1132, 88);
            this.tlpSummary.TabIndex = 0;
            //
            // pnlActions
            //
            this.pnlActions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(254)))), ((int)(((byte)(253)))));
            this.pnlActions.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(222)))), ((int)(((byte)(215)))));
            this.pnlActions.BorderSize = 1;
            this.pnlActions.Controls.Add(this.tlpActions);
            this.pnlActions.CornerRadius = 18;
            this.pnlActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlActions.Location = new System.Drawing.Point(22, 202);
            this.pnlActions.Margin = new System.Windows.Forms.Padding(0, 4, 0, 8);
            this.pnlActions.Name = "pnlActions";
            this.pnlActions.Padding = new System.Windows.Forms.Padding(14, 8, 14, 8);
            this.pnlActions.Size = new System.Drawing.Size(1156, 248);
            this.pnlActions.TabIndex = 2;
            //
            // btnReadInfo
            //
            this.btnReadInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnReadInfo.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(185)))), ((int)(((byte)(202)))), ((int)(((byte)(193)))));
            this.btnReadInfo.BorderSize = 1;
            this.btnReadInfo.CornerRadius = 14;
            this.btnReadInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnReadInfo.FlatAppearance.BorderSize = 0;
            this.btnReadInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReadInfo.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F, System.Drawing.FontStyle.Bold);
            this.btnReadInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(75)))), ((int)(((byte)(68)))));
            this.btnReadInfo.Image = global::FuntinoControlCenter.Properties.Resources.CardScan;
            this.btnReadInfo.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnReadInfo.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnReadInfo.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnReadInfo.Location = new System.Drawing.Point(455, 2);
            this.btnReadInfo.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.btnReadInfo.Name = "btnReadInfo";
            this.btnReadInfo.Size = new System.Drawing.Size(214, 57);
            this.btnReadInfo.TabIndex = 2;
            this.btnReadInfo.Text = "اطلاعات کارت   F1";
            this.btnReadInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnReadInfo.UseVisualStyleBackColor = false;
            this.btnReadInfo.Click += new System.EventHandler(this.ReadInfo_Click);
            //
            // btnReadBalance
            //
            this.btnReadBalance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnReadBalance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(185)))), ((int)(((byte)(202)))), ((int)(((byte)(193)))));
            this.btnReadBalance.BorderSize = 1;
            this.btnReadBalance.CornerRadius = 14;
            this.btnReadBalance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnReadBalance.FlatAppearance.BorderSize = 0;
            this.btnReadBalance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReadBalance.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9F, System.Drawing.FontStyle.Bold);
            this.btnReadBalance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(84)))), ((int)(((byte)(72)))));
            this.btnReadBalance.Image = global::FuntinoControlCenter.Properties.Resources.WalletBalance;
            this.btnReadBalance.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnReadBalance.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnReadBalance.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnReadBalance.Location = new System.Drawing.Point(679, 2);
            this.btnReadBalance.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.btnReadBalance.Name = "btnReadBalance";
            this.btnReadBalance.Size = new System.Drawing.Size(214, 57);
            this.btnReadBalance.TabIndex = 3;
            this.btnReadBalance.Text = "خواندن موجودی کارت   F2";
            this.btnReadBalance.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnReadBalance.UseVisualStyleBackColor = false;
            this.btnReadBalance.Click += new System.EventHandler(this.ReadBalance_Click);
            //
            // btnReadRemainder
            //
            this.btnReadRemainder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnReadRemainder.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(185)))), ((int)(((byte)(202)))), ((int)(((byte)(193)))));
            this.btnReadRemainder.BorderSize = 1;
            this.btnReadRemainder.CornerRadius = 14;
            this.btnReadRemainder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnReadRemainder.FlatAppearance.BorderSize = 0;
            this.btnReadRemainder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReadRemainder.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F, System.Drawing.FontStyle.Bold);
            this.btnReadRemainder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(75)))), ((int)(((byte)(68)))));
            this.btnReadRemainder.Image = global::FuntinoControlCenter.Properties.Resources.WalletBalance;
            this.btnReadRemainder.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnReadRemainder.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnReadRemainder.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnReadRemainder.Location = new System.Drawing.Point(5, 2);
            this.btnReadRemainder.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.btnReadRemainder.Name = "btnReadRemainder";
            this.btnReadRemainder.Size = new System.Drawing.Size(216, 57);
            this.btnReadRemainder.TabIndex = 6;
            this.btnReadRemainder.Text = "باقیمانده‌ی شارژ   F4";
            this.btnReadRemainder.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnReadRemainder.UseVisualStyleBackColor = false;
            this.btnReadRemainder.Click += new System.EventHandler(this.ReadRemainder_Click);
            //
            // btnReadHistory
            //
            this.btnReadHistory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnReadHistory.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(185)))), ((int)(((byte)(202)))), ((int)(((byte)(193)))));
            this.btnReadHistory.BorderSize = 1;
            this.btnReadHistory.CornerRadius = 14;
            this.btnReadHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnReadHistory.FlatAppearance.BorderSize = 0;
            this.btnReadHistory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReadHistory.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F, System.Drawing.FontStyle.Bold);
            this.btnReadHistory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(75)))), ((int)(((byte)(68)))));
            this.btnReadHistory.Image = global::FuntinoControlCenter.Properties.Resources.CardHistory;
            this.btnReadHistory.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnReadHistory.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnReadHistory.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnReadHistory.Location = new System.Drawing.Point(231, 2);
            this.btnReadHistory.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.btnReadHistory.Name = "btnReadHistory";
            this.btnReadHistory.Size = new System.Drawing.Size(214, 57);
            this.btnReadHistory.TabIndex = 5;
            this.btnReadHistory.Text = "سوابق کارت   F3";
            this.btnReadHistory.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnReadHistory.UseVisualStyleBackColor = false;
            this.btnReadHistory.Click += new System.EventHandler(this.ReadHistory_Click);
            //
            // btnRecharge
            //
            this.btnRecharge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(89)))), ((int)(((byte)(81)))));
            this.btnRecharge.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(89)))), ((int)(((byte)(81)))));
            this.btnRecharge.BorderSize = 1;
            this.btnRecharge.CornerRadius = 14;
            this.btnRecharge.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRecharge.FlatAppearance.BorderSize = 0;
            this.btnRecharge.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRecharge.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9.5F, System.Drawing.FontStyle.Bold);
            this.btnRecharge.ForeColor = System.Drawing.Color.White;
            this.btnRecharge.Image = global::FuntinoControlCenter.Properties.Resources.CardRecharge;
            this.btnRecharge.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRecharge.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnRecharge.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnRecharge.Location = new System.Drawing.Point(903, 2);
            this.btnRecharge.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.btnRecharge.Name = "btnRecharge";
            this.btnRecharge.Size = new System.Drawing.Size(214, 57);
            this.btnRecharge.TabIndex = 4;
            this.btnRecharge.Text = "شارژ کارت   P / Enter";
            this.btnRecharge.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnRecharge.UseVisualStyleBackColor = false;
            this.btnRecharge.Click += new System.EventHandler(this.Recharge_Click);
            //
            // lblAmountCaption
            //
            this.lblAmountCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAmountCaption.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F, System.Drawing.FontStyle.Bold);
            this.lblAmountCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(94)))), ((int)(((byte)(86)))));
            this.lblAmountCaption.Location = new System.Drawing.Point(3, 0);
            this.lblAmountCaption.Name = "lblAmountCaption";
            this.lblAmountCaption.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblAmountCaption.Size = new System.Drawing.Size(1122, 26);
            this.lblAmountCaption.TabIndex = 0;
            this.lblAmountCaption.Text = "مبلغ شارژ به تومان";
            this.lblAmountCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // pnlAmountInput
            //
            this.pnlAmountInput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(250)))), ((int)(((byte)(248)))));
            this.pnlAmountInput.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(183)))), ((int)(((byte)(202)))), ((int)(((byte)(193)))));
            this.pnlAmountInput.BorderSize = 1;
            this.pnlAmountInput.Controls.Add(this.txtAmount);
            this.pnlAmountInput.CornerRadius = 12;
            this.pnlAmountInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlAmountInput.Location = new System.Drawing.Point(0, 26);
            this.pnlAmountInput.Margin = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.pnlAmountInput.Name = "pnlAmountInput";
            this.pnlAmountInput.Padding = new System.Windows.Forms.Padding(12, 5, 12, 5);
            this.pnlAmountInput.Size = new System.Drawing.Size(1128, 70);
            this.pnlAmountInput.TabIndex = 1;
            //
            // txtAmount
            //
            this.txtAmount.BackColor = System.Drawing.Color.White;
            this.txtAmount.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtAmount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtAmount.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 27F, System.Drawing.FontStyle.Bold);
            this.txtAmount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(89)))), ((int)(((byte)(81)))));
            this.txtAmount.Location = new System.Drawing.Point(12, 5);
            this.txtAmount.Margin = new System.Windows.Forms.Padding(0);
            this.txtAmount.Name = "txtAmount";
            this.txtAmount.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtAmount.Size = new System.Drawing.Size(1104, 41);
            this.txtAmount.TabIndex = 0;
            this.txtAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtAmount.TextChanged += new System.EventHandler(this.Amount_TextChanged);
            //
            // pnlRechargeTerms
            //
            this.pnlRechargeTerms.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(250)))), ((int)(((byte)(248)))));
            this.pnlRechargeTerms.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(183)))), ((int)(((byte)(202)))), ((int)(((byte)(193)))));
            this.pnlRechargeTerms.BorderSize = 1;
            this.pnlRechargeTerms.Controls.Add(this.lblRechargeTerms);
            this.pnlRechargeTerms.Controls.Add(this.cmbPromotion);
            this.pnlRechargeTerms.Controls.Add(this.lblPromotionCaption);
            this.pnlRechargeTerms.CornerRadius = 10;
            this.pnlRechargeTerms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRechargeTerms.Location = new System.Drawing.Point(0, 100);
            this.pnlRechargeTerms.Margin = new System.Windows.Forms.Padding(0, 1, 0, 2);
            this.pnlRechargeTerms.Name = "pnlRechargeTerms";
            this.pnlRechargeTerms.Size = new System.Drawing.Size(1128, 36);
            this.pnlRechargeTerms.TabIndex = 2;
            this.pnlRechargeTerms.Visible = false;
            this.lblPromotionCaption.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F, System.Drawing.FontStyle.Bold);
            this.lblPromotionCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(81)))), ((int)(((byte)(72)))));
            this.lblPromotionCaption.Location = new System.Drawing.Point(927, 2);
            this.lblPromotionCaption.Name = "lblPromotionCaption";
            this.lblPromotionCaption.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblPromotionCaption.Size = new System.Drawing.Size(188, 28);
            this.lblPromotionCaption.TabIndex = 0;
            this.lblPromotionCaption.Text = "جشنواره‌ی این شارژ";
            this.lblPromotionCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cmbPromotion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPromotion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbPromotion.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F);
            this.cmbPromotion.FormattingEnabled = true;
            this.cmbPromotion.Location = new System.Drawing.Point(550, 3);
            this.cmbPromotion.Name = "cmbPromotion";
            this.cmbPromotion.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cmbPromotion.Size = new System.Drawing.Size(365, 27);
            this.cmbPromotion.TabIndex = 1;
            this.cmbPromotion.SelectedIndexChanged += new System.EventHandler(this.PromotionSelectionChanged);
            this.lblRechargeTerms.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F);
            this.lblRechargeTerms.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(81)))), ((int)(((byte)(72)))));
            this.lblRechargeTerms.Location = new System.Drawing.Point(12, 2);
            this.lblRechargeTerms.Name = "lblRechargeTerms";
            this.lblRechargeTerms.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblRechargeTerms.Size = new System.Drawing.Size(520, 28);
            this.lblRechargeTerms.TabIndex = 2;
            this.lblRechargeTerms.Text = "شرایط شارژ";
            this.lblRechargeTerms.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // tlpActions
            //
            this.tlpActions.BackColor = System.Drawing.Color.Transparent;
            this.tlpActions.ColumnCount = 1;
            this.tlpActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpActions.Controls.Add(this.lblAmountCaption, 0, 0);
            this.tlpActions.Controls.Add(this.pnlAmountInput, 0, 1);
            this.tlpActions.Controls.Add(this.pnlRechargeTerms, 0, 2);
            this.tlpActions.Controls.Add(this.lblAmountHint, 0, 3);
            this.tlpActions.Controls.Add(this.tlpActionButtons, 0, 4);
            this.tlpActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpActions.Location = new System.Drawing.Point(14, 8);
            this.tlpActions.Name = "tlpActions";
            this.tlpActions.RowCount = 5;
            this.tlpActions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpActions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
            this.tlpActions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tlpActions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpActions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpActions.Size = new System.Drawing.Size(1128, 232);
            this.tlpActions.TabIndex = 0;
            //
            // lblAmountHint
            //
            this.lblAmountHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAmountHint.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F);
            this.lblAmountHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(112)))), ((int)(((byte)(105)))));
            this.lblAmountHint.Location = new System.Drawing.Point(3, 136);
            this.lblAmountHint.Name = "lblAmountHint";
            this.lblAmountHint.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblAmountHint.Size = new System.Drawing.Size(1122, 25);
            this.lblAmountHint.TabIndex = 2;
            this.lblAmountHint.Text = "شارژ کارت: P / Enter  |  اطلاعات کارت: F1  |  خواندن موجودی کارت: F2  |  سوابق کارت: F3  |  باقیمانده‌ی شارژ: F4  |  سوابق سامانه: F5  |  نمایش سوابق کارت: F6  |  رفتن به کادر جست‌وجو: Ctrl+F  |  جست‌وجو: Enter  |  قبلی: Ctrl+←  |  بعدی: Ctrl+→";
            this.lblAmountHint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // tlpActionButtons
            //
            this.tlpActionButtons.BackColor = System.Drawing.Color.Transparent;
            this.tlpActionButtons.ColumnCount = 5;
            this.tlpActionButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpActionButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpActionButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpActionButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpActionButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpActionButtons.Controls.Add(this.btnRecharge, 0, 0);
            this.tlpActionButtons.Controls.Add(this.btnReadBalance, 1, 0);
            this.tlpActionButtons.Controls.Add(this.btnReadInfo, 2, 0);
            this.tlpActionButtons.Controls.Add(this.btnReadHistory, 3, 0);
            this.tlpActionButtons.Controls.Add(this.btnReadRemainder, 4, 0);
            this.tlpActionButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpActionButtons.Location = new System.Drawing.Point(3, 126);
            this.tlpActionButtons.Name = "tlpActionButtons";
            this.tlpActionButtons.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpActionButtons.RowCount = 1;
            this.tlpActionButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpActionButtons.Size = new System.Drawing.Size(1122, 61);
            this.tlpActionButtons.TabIndex = 3;
            //
            // pnlCardHistory
            //
            this.pnlCardHistory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(254)))), ((int)(((byte)(253)))));
            this.pnlCardHistory.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(222)))), ((int)(((byte)(215)))));
            this.pnlCardHistory.BorderSize = 1;
            this.pnlCardHistory.Controls.Add(this.gridCardActivity);
            this.pnlCardHistory.CornerRadius = 15;
            this.pnlCardHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCardHistory.Location = new System.Drawing.Point(0, 270);
            this.pnlCardHistory.Margin = new System.Windows.Forms.Padding(0);
            this.pnlCardHistory.Name = "pnlCardHistory";
            this.pnlCardHistory.Padding = new System.Windows.Forms.Padding(12);
            this.pnlCardHistory.Size = new System.Drawing.Size(1150, 20);
            this.pnlCardHistory.TabIndex = 5;
            this.pnlCardHistory.Visible = false;
            //
            // gridCardActivity
            //
            this.gridCardActivity.AllowUserToAddRows = false;
            this.gridCardActivity.AllowUserToDeleteRows = false;
            this.gridCardActivity.AllowUserToResizeRows = false;
            this.gridCardActivity.AutoGenerateColumns = false;
            this.gridCardActivity.BackgroundColor = System.Drawing.Color.White;
            this.gridCardActivity.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridCardActivity.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.gridCardActivity.ColumnHeadersHeight = 36;
            this.gridCardActivity.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.gridCardActivity.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colActivityType, this.colActivityAmount, this.colActivityDate, this.colActivityTime, this.colActivitySource });
            this.gridCardActivity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridCardActivity.Location = new System.Drawing.Point(12, 12);
            this.gridCardActivity.Margin = new System.Windows.Forms.Padding(0);
            this.gridCardActivity.MultiSelect = false;
            this.gridCardActivity.Name = "gridCardActivity";
            this.gridCardActivity.ReadOnly = true;
            this.gridCardActivity.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.gridCardActivity.RowHeadersVisible = false;
            this.gridCardActivity.RowTemplate.Height = 29;
            this.gridCardActivity.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridCardActivity.Size = new System.Drawing.Size(1126, 0);
            this.gridCardActivity.TabIndex = 0;
            this.colActivityType.DataPropertyName = "ActivityType";
            this.colActivityType.HeaderText = "عملیات";
            this.colActivityType.Name = "colActivityType";
            this.colActivityType.ReadOnly = true;
            this.colActivityType.Width = 190;
            this.colActivityAmount.DataPropertyName = "Amount";
            this.colActivityAmount.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.colActivityAmount.DefaultCellStyle.Format = "N0";
            this.colActivityAmount.HeaderText = "مبلغ (تومان)";
            this.colActivityAmount.Name = "colActivityAmount";
            this.colActivityAmount.ReadOnly = true;
            this.colActivityAmount.Width = 180;
            this.colActivityDate.DataPropertyName = "Date";
            this.colActivityDate.HeaderText = "تاریخ ثبت‌شده";
            this.colActivityDate.Name = "colActivityDate";
            this.colActivityDate.ReadOnly = true;
            this.colActivityDate.Width = 210;
            this.colActivityTime.DataPropertyName = "Time";
            this.colActivityTime.HeaderText = "زمان";
            this.colActivityTime.Name = "colActivityTime";
            this.colActivityTime.ReadOnly = true;
            this.colActivityTime.Width = 180;
            this.colActivitySource.DataPropertyName = "Source";
            this.colActivitySource.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colActivitySource.HeaderText = "جزئیات";
            this.colActivitySource.Name = "colActivitySource";
            this.colActivitySource.ReadOnly = true;
            //
            // pnlDatabaseHistory
            //
            this.pnlDatabaseHistory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(254)))), ((int)(((byte)(253)))));
            this.pnlDatabaseHistory.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(222)))), ((int)(((byte)(215)))));
            this.pnlDatabaseHistory.BorderSize = 1;
            this.pnlDatabaseHistory.Controls.Add(this.tlpDatabaseHistory);
            this.pnlDatabaseHistory.CornerRadius = 15;
            this.pnlDatabaseHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDatabaseHistory.Location = new System.Drawing.Point(0, 0);
            this.pnlDatabaseHistory.Margin = new System.Windows.Forms.Padding(0);
            this.pnlDatabaseHistory.Name = "pnlDatabaseHistory";
            this.pnlDatabaseHistory.Padding = new System.Windows.Forms.Padding(10);
            this.pnlDatabaseHistory.Size = new System.Drawing.Size(1150, 270);
            this.pnlDatabaseHistory.TabIndex = 6;
            this.pnlDatabaseHistory.Visible = false;
            //
            // pnlSearchInput
            //
            this.pnlSearchInput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(250)))), ((int)(((byte)(248)))));
            this.pnlSearchInput.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(183)))), ((int)(((byte)(202)))), ((int)(((byte)(193)))));
            this.pnlSearchInput.BorderSize = 1;
            this.pnlSearchInput.Controls.Add(this.txtHistorySearch);
            this.pnlSearchInput.CornerRadius = 10;
            this.pnlSearchInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSearchInput.Location = new System.Drawing.Point(747, 4);
            this.pnlSearchInput.Margin = new System.Windows.Forms.Padding(3, 4, 5, 4);
            this.pnlSearchInput.Name = "pnlSearchInput";
            this.pnlSearchInput.Padding = new System.Windows.Forms.Padding(8, 2, 8, 2);
            this.pnlSearchInput.Size = new System.Drawing.Size(374, 36);
            this.pnlSearchInput.TabIndex = 0;
            //
            // txtHistorySearch
            //
            this.txtHistorySearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtHistorySearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtHistorySearch.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F);
            this.txtHistorySearch.Location = new System.Drawing.Point(8, 2);
            this.txtHistorySearch.Margin = new System.Windows.Forms.Padding(0);
            this.txtHistorySearch.Name = "txtHistorySearch";
            this.txtHistorySearch.Tag = "Ctrl+F";
            this.txtHistorySearch.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.txtHistorySearch.Size = new System.Drawing.Size(358, 13);
            this.txtHistorySearch.TabIndex = 0;
            this.txtHistorySearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HistorySearch_KeyDown);
            //
            // cmbHistoryFilter
            //
            this.cmbHistoryFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbHistoryFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHistoryFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbHistoryFilter.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F);
            this.cmbHistoryFilter.FormattingEnabled = true;
            this.cmbHistoryFilter.Items.AddRange(new object[] {
            "همه‌ی شارژها",
            "شارژ عادی",
            "پرداخت پایانه",
            "شارژ ساعتی",
            "شارژ اعتباری"});
            this.cmbHistoryFilter.Location = new System.Drawing.Point(533, 4);
            this.cmbHistoryFilter.Margin = new System.Windows.Forms.Padding(4);
            this.cmbHistoryFilter.Name = "cmbHistoryFilter";
            this.cmbHistoryFilter.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cmbHistoryFilter.Size = new System.Drawing.Size(205, 21);
            this.cmbHistoryFilter.TabIndex = 1;
            //
            // btnSearchHistory
            //
            this.btnSearchHistory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(89)))), ((int)(((byte)(81)))));
            this.btnSearchHistory.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(89)))), ((int)(((byte)(81)))));
            this.btnSearchHistory.BorderSize = 1;
            this.btnSearchHistory.CornerRadius = 10;
            this.btnSearchHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSearchHistory.FlatAppearance.BorderSize = 0;
            this.btnSearchHistory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearchHistory.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F, System.Drawing.FontStyle.Bold);
            this.btnSearchHistory.ForeColor = System.Drawing.Color.White;
            this.btnSearchHistory.Image = global::FuntinoControlCenter.Properties.Resources.CardScan;
            this.btnSearchHistory.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSearchHistory.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSearchHistory.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.btnSearchHistory.Location = new System.Drawing.Point(375, 4);
            this.btnSearchHistory.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSearchHistory.Name = "btnSearchHistory";
            this.btnSearchHistory.Size = new System.Drawing.Size(151, 36);
            this.btnSearchHistory.TabIndex = 2;
            this.btnSearchHistory.Text = "جست‌وجو   Enter";
            this.btnSearchHistory.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnSearchHistory.UseVisualStyleBackColor = false;
            this.btnSearchHistory.Click += new System.EventHandler(this.SearchHistory_Click);
            //
            // btnPreviousHistory
            //
            this.btnPreviousHistory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnPreviousHistory.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(185)))), ((int)(((byte)(202)))), ((int)(((byte)(193)))));
            this.btnPreviousHistory.BorderSize = 1;
            this.btnPreviousHistory.CornerRadius = 10;
            this.btnPreviousHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPreviousHistory.FlatAppearance.BorderSize = 0;
            this.btnPreviousHistory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPreviousHistory.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F, System.Drawing.FontStyle.Bold);
            this.btnPreviousHistory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(75)))), ((int)(((byte)(68)))));
            this.btnPreviousHistory.Location = new System.Drawing.Point(274, 4);
            this.btnPreviousHistory.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPreviousHistory.Name = "btnPreviousHistory";
            this.btnPreviousHistory.Size = new System.Drawing.Size(95, 36);
            this.btnPreviousHistory.TabIndex = 3;
            this.btnPreviousHistory.Text = "قبلی   Ctrl+←";
            this.btnPreviousHistory.UseVisualStyleBackColor = false;
            this.btnPreviousHistory.Click += new System.EventHandler(this.PreviousHistory_Click);
            //
            // btnNextHistory
            //
            this.btnNextHistory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnNextHistory.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(185)))), ((int)(((byte)(202)))), ((int)(((byte)(193)))));
            this.btnNextHistory.BorderSize = 1;
            this.btnNextHistory.CornerRadius = 10;
            this.btnNextHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNextHistory.FlatAppearance.BorderSize = 0;
            this.btnNextHistory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNextHistory.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F, System.Drawing.FontStyle.Bold);
            this.btnNextHistory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(75)))), ((int)(((byte)(68)))));
            this.btnNextHistory.Location = new System.Drawing.Point(173, 4);
            this.btnNextHistory.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnNextHistory.Name = "btnNextHistory";
            this.btnNextHistory.Size = new System.Drawing.Size(95, 36);
            this.btnNextHistory.TabIndex = 4;
            this.btnNextHistory.Text = "بعدی   Ctrl+→";
            this.btnNextHistory.UseVisualStyleBackColor = false;
            this.btnNextHistory.Click += new System.EventHandler(this.NextHistory_Click);
            //
            // lblHistoryPages
            //
            this.lblHistoryPages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHistoryPages.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 7.5F);
            this.lblHistoryPages.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(112)))), ((int)(((byte)(105)))));
            this.lblHistoryPages.Location = new System.Drawing.Point(6, 0);
            this.lblHistoryPages.Margin = new System.Windows.Forms.Padding(3, 0, 6, 0);
            this.lblHistoryPages.Name = "lblHistoryPages";
            this.lblHistoryPages.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblHistoryPages.Size = new System.Drawing.Size(161, 44);
            this.lblHistoryPages.TabIndex = 5;
            this.lblHistoryPages.Text = "ابتدا سوابق کارت را بخوانید.";
            this.lblHistoryPages.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // gridRechargeHistory
            //
            this.gridRechargeHistory.AllowUserToAddRows = false;
            this.gridRechargeHistory.AllowUserToDeleteRows = false;
            this.gridRechargeHistory.AllowUserToResizeRows = false;
            this.gridRechargeHistory.AutoGenerateColumns = false;
            this.gridRechargeHistory.BackgroundColor = System.Drawing.Color.White;
            this.gridRechargeHistory.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridRechargeHistory.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.gridRechargeHistory.ColumnHeadersHeight = 34;
            this.gridRechargeHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.gridRechargeHistory.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colRechargeType, this.colRechargeAmount, this.colRechargeDate, this.colRechargeUser, this.colRechargeId });
            this.gridRechargeHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridRechargeHistory.Location = new System.Drawing.Point(0, 50);
            this.gridRechargeHistory.Margin = new System.Windows.Forms.Padding(0);
            this.gridRechargeHistory.MultiSelect = false;
            this.gridRechargeHistory.Name = "gridRechargeHistory";
            this.gridRechargeHistory.ReadOnly = true;
            this.gridRechargeHistory.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.gridRechargeHistory.RowHeadersVisible = false;
            this.gridRechargeHistory.RowTemplate.Height = 27;
            this.gridRechargeHistory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridRechargeHistory.Size = new System.Drawing.Size(1130, 200);
            this.gridRechargeHistory.TabIndex = 6;
            this.gridRechargeHistory.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GridRechargeHistory_ColumnHeaderMouseClick);
            this.colRechargeType.DataPropertyName = "OperationType";
            this.colRechargeType.HeaderText = "نوع شارژ";
            this.colRechargeType.Name = "colRechargeType";
            this.colRechargeType.ReadOnly = true;
            this.colRechargeType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.colRechargeType.Width = 190;
            this.colRechargeAmount.DataPropertyName = "RechargeAmount";
            this.colRechargeAmount.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.colRechargeAmount.DefaultCellStyle.Format = "N0";
            this.colRechargeAmount.HeaderText = "مبلغ (تومان)";
            this.colRechargeAmount.Name = "colRechargeAmount";
            this.colRechargeAmount.ReadOnly = true;
            this.colRechargeAmount.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.colRechargeAmount.Width = 180;
            this.colRechargeDate.DataPropertyName = "CreatedAt";
            this.colRechargeDate.DefaultCellStyle.Format = "yyyy/MM/dd HH:mm";
            this.colRechargeDate.HeaderText = "زمان ثبت";
            this.colRechargeDate.Name = "colRechargeDate";
            this.colRechargeDate.ReadOnly = true;
            this.colRechargeDate.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.colRechargeDate.Width = 210;
            this.colRechargeUser.DataPropertyName = "CreatedByUserName";
            this.colRechargeUser.HeaderText = "ثبت‌کننده";
            this.colRechargeUser.Name = "colRechargeUser";
            this.colRechargeUser.ReadOnly = true;
            this.colRechargeUser.Width = 230;
            this.colRechargeId.DataPropertyName = "RechargeTransactionId";
            this.colRechargeId.HeaderText = "شماره پیگیری";
            this.colRechargeId.Name = "colRechargeId";
            this.colRechargeId.ReadOnly = true;
            this.colRechargeId.Width = 170;
            //
            // tlpDatabaseHistory
            //
            this.tlpDatabaseHistory.BackColor = System.Drawing.Color.Transparent;
            this.tlpDatabaseHistory.ColumnCount = 1;
            this.tlpDatabaseHistory.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDatabaseHistory.Controls.Add(this.tlpDatabaseToolbar, 0, 0);
            this.tlpDatabaseHistory.Controls.Add(this.gridRechargeHistory, 0, 1);
            this.tlpDatabaseHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDatabaseHistory.Location = new System.Drawing.Point(10, 10);
            this.tlpDatabaseHistory.Name = "tlpDatabaseHistory";
            this.tlpDatabaseHistory.RowCount = 2;
            this.tlpDatabaseHistory.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tlpDatabaseHistory.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDatabaseHistory.Size = new System.Drawing.Size(1130, 250);
            this.tlpDatabaseHistory.TabIndex = 0;
            //
            // tlpDatabaseToolbar
            //
            this.tlpDatabaseToolbar.BackColor = System.Drawing.Color.Transparent;
            this.tlpDatabaseToolbar.ColumnCount = 6;
            this.tlpDatabaseToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.tlpDatabaseToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19F));
            this.tlpDatabaseToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14F));
            this.tlpDatabaseToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 9F));
            this.tlpDatabaseToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 9F));
            this.tlpDatabaseToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tlpDatabaseToolbar.Controls.Add(this.pnlSearchInput, 0, 0);
            this.tlpDatabaseToolbar.Controls.Add(this.cmbHistoryFilter, 1, 0);
            this.tlpDatabaseToolbar.Controls.Add(this.btnSearchHistory, 2, 0);
            this.tlpDatabaseToolbar.Controls.Add(this.btnPreviousHistory, 3, 0);
            this.tlpDatabaseToolbar.Controls.Add(this.btnNextHistory, 4, 0);
            this.tlpDatabaseToolbar.Controls.Add(this.lblHistoryPages, 5, 0);
            this.tlpDatabaseToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDatabaseToolbar.Location = new System.Drawing.Point(3, 3);
            this.tlpDatabaseToolbar.Name = "tlpDatabaseToolbar";
            this.tlpDatabaseToolbar.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpDatabaseToolbar.RowCount = 1;
            this.tlpDatabaseToolbar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDatabaseToolbar.Size = new System.Drawing.Size(1124, 44);
            this.tlpDatabaseToolbar.TabIndex = 0;
            //
            // btnCardHistoryTab
            //
            this.btnCardHistoryTab.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnCardHistoryTab.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(185)))), ((int)(((byte)(202)))), ((int)(((byte)(193)))));
            this.btnCardHistoryTab.BorderSize = 1;
            this.btnCardHistoryTab.CornerRadius = 11;
            this.btnCardHistoryTab.FlatAppearance.BorderSize = 0;
            this.btnCardHistoryTab.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCardHistoryTab.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F, System.Drawing.FontStyle.Bold);
            this.btnCardHistoryTab.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(75)))), ((int)(((byte)(68)))));
            this.btnCardHistoryTab.Image = global::FuntinoControlCenter.Properties.Resources.CardHistory;
            this.btnCardHistoryTab.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCardHistoryTab.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCardHistoryTab.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnCardHistoryTab.Location = new System.Drawing.Point(4, 3);
            this.btnCardHistoryTab.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCardHistoryTab.Name = "btnCardHistoryTab";
            this.btnCardHistoryTab.Size = new System.Drawing.Size(220, 36);
            this.btnCardHistoryTab.TabIndex = 3;
            this.btnCardHistoryTab.Text = "نمایش سوابق کارت   F6";
            this.btnCardHistoryTab.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCardHistoryTab.UseVisualStyleBackColor = false;
            this.btnCardHistoryTab.Click += new System.EventHandler(this.CardHistoryTab_Click);
            //
            // btnDatabaseHistoryTab
            //
            this.btnDatabaseHistoryTab.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnDatabaseHistoryTab.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(185)))), ((int)(((byte)(202)))), ((int)(((byte)(193)))));
            this.btnDatabaseHistoryTab.BorderSize = 1;
            this.btnDatabaseHistoryTab.CornerRadius = 11;
            this.btnDatabaseHistoryTab.FlatAppearance.BorderSize = 0;
            this.btnDatabaseHistoryTab.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDatabaseHistoryTab.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F, System.Drawing.FontStyle.Bold);
            this.btnDatabaseHistoryTab.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(75)))), ((int)(((byte)(68)))));
            this.btnDatabaseHistoryTab.Image = global::FuntinoControlCenter.Properties.Resources.CardRecharge;
            this.btnDatabaseHistoryTab.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDatabaseHistoryTab.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnDatabaseHistoryTab.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.btnDatabaseHistoryTab.Location = new System.Drawing.Point(202, 3);
            this.btnDatabaseHistoryTab.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnDatabaseHistoryTab.Name = "btnDatabaseHistoryTab";
            this.btnDatabaseHistoryTab.Size = new System.Drawing.Size(215, 36);
            this.btnDatabaseHistoryTab.TabIndex = 4;
            this.btnDatabaseHistoryTab.Text = "سوابق سامانه   F5";
            this.btnDatabaseHistoryTab.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnDatabaseHistoryTab.UseVisualStyleBackColor = false;
            this.btnDatabaseHistoryTab.Click += new System.EventHandler(this.DatabaseHistoryTab_Click);
            //
            // tlpRoot
            //
            this.tlpRoot.BackColor = System.Drawing.Color.Transparent;
            this.tlpRoot.ColumnCount = 1;
            this.tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Controls.Add(this.pnlHeader, 0, 0);
            this.tlpRoot.Controls.Add(this.pnlSummary, 0, 1);
            this.tlpRoot.Controls.Add(this.pnlActions, 0, 2);
            this.tlpRoot.Controls.Add(this.flpHistoryTabs, 0, 3);
            this.tlpRoot.Controls.Add(this.tlpHistoryHost, 0, 4);
            this.tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRoot.Location = new System.Drawing.Point(0, 0);
            this.tlpRoot.Name = "tlpRoot";
            this.tlpRoot.Padding = new System.Windows.Forms.Padding(22, 14, 22, 16);
            this.tlpRoot.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpRoot.RowCount = 5;
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 68F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 116F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 260F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Size = new System.Drawing.Size(1200, 772);
            this.tlpRoot.TabIndex = 0;
            //
            // flpHistoryTabs
            //
            this.flpHistoryTabs.BackColor = System.Drawing.Color.Transparent;
            this.flpHistoryTabs.Controls.Add(this.btnCardHistoryTab);
            this.flpHistoryTabs.Controls.Add(this.btnDatabaseHistoryTab);
            this.flpHistoryTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpHistoryTabs.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpHistoryTabs.Location = new System.Drawing.Point(22, 416);
            this.flpHistoryTabs.Margin = new System.Windows.Forms.Padding(0);
            this.flpHistoryTabs.Name = "flpHistoryTabs";
            this.flpHistoryTabs.Size = new System.Drawing.Size(1156, 44);
            this.flpHistoryTabs.TabIndex = 3;
            this.flpHistoryTabs.WrapContents = false;
            //
            // tlpHistoryHost
            //
            this.tlpHistoryHost.BackColor = System.Drawing.Color.Transparent;
            this.tlpHistoryHost.ColumnCount = 1;
            this.tlpHistoryHost.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHistoryHost.Controls.Add(this.pnlCardHistory, 0, 0);
            this.tlpHistoryHost.Controls.Add(this.pnlDatabaseHistory, 0, 0);
            this.tlpHistoryHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHistoryHost.Location = new System.Drawing.Point(25, 463);
            this.tlpHistoryHost.Name = "tlpHistoryHost";
            this.tlpHistoryHost.RowCount = 1;
            this.tlpHistoryHost.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHistoryHost.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpHistoryHost.Size = new System.Drawing.Size(1150, 290);
            this.tlpHistoryHost.TabIndex = 4;
            //
            // operationWorker
            //
            this.operationWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.OperationWorker_DoWork);
            this.operationWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.OperationWorker_RunWorkerCompleted);
            //
            // authorizationTimer
            //
            this.authorizationTimer.Interval = 2500;
            this.authorizationTimer.Tick += new System.EventHandler(this.AuthorizationTimer_Tick);
            //
            // CardOperationsForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(246)))), ((int)(((byte)(243)))));
            this.BackgroundImage = global::FuntinoControlCenter.Properties.Resources.CardOperationsBackground;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1200, 772);
            this.Controls.Add(this.tlpRoot);
            this.Font = new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9F);
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1000, 680);
            this.Name = "CardOperationsForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "عملیات کارت";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CardOperationsForm_FormClosing);
            this.Load += new System.EventHandler(this.CardOperationsForm_Load);
            this.Shown += new System.EventHandler(this.CardOperationsForm_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CardOperationsForm_KeyDown);
            this.pnlHeader.ResumeLayout(false);
            this.tlpHeader.ResumeLayout(false);
            this.tlpHeaderText.ResumeLayout(false);
            this.pnlSummary.ResumeLayout(false);
            this.tlpSummary.ResumeLayout(false);
            this.pnlActions.ResumeLayout(false);
            this.pnlAmountInput.ResumeLayout(false);
            this.pnlAmountInput.PerformLayout();
            this.pnlRechargeTerms.ResumeLayout(false);
            this.tlpActions.ResumeLayout(false);
            this.tlpActionButtons.ResumeLayout(false);
            this.pnlCardHistory.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridCardActivity)).EndInit();
            this.pnlDatabaseHistory.ResumeLayout(false);
            this.pnlSearchInput.ResumeLayout(false);
            this.pnlSearchInput.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridRechargeHistory)).EndInit();
            this.tlpDatabaseHistory.ResumeLayout(false);
            this.tlpDatabaseToolbar.ResumeLayout(false);
            this.tlpRoot.ResumeLayout(false);
            this.flpHistoryTabs.ResumeLayout(false);
            this.tlpHistoryHost.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
