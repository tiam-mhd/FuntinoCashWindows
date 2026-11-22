namespace FuntinoControlCenter.Forms
{
    partial class CustomerFamilyForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel tlpRoot, tlpHeader, tlpBody, tlpCustomers, tlpSearch, tlpPairing, tlpCustomerPager, tlpFamily;
        private System.Windows.Forms.Panel pnlHeader, pnlSnapshot, pnlSearchInput, pnlCustomerGrid, pnlFamilyGrid, pnlCardsGrid;
        private FuntinoControlCenter.UI.RoundedBorderPanel pnlCustomers, pnlFamily;
        private System.Windows.Forms.PictureBox picHeader, picSearch;
        private System.Windows.Forms.Label lblTitle, lblSubtitle, lblSnapshot, lblCustomerTitle, lblPairingStatus, lblFamilyTitle, lblFamilyCustomer, lblCardsTitle, lblCustomerPage, lblCustomerCount;
        private System.Windows.Forms.TextBox txtSearch, txtPairingCode;
        private FuntinoControlCenter.UI.RoundedButton btnBack, btnSearch, btnPairOperator, btnCancelPairing, btnPrevious, btnNext;
        private System.Windows.Forms.DataGridView gridCustomers, gridFamily, gridCards;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCustomerName, colMaskedPhone, colCustomerId, colHouseholdId, colMemberId, colSnapshotVersion;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFamilyType, colFamilyName, colRelatedCustomerId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCardId, colCardMemberId, colCardSnapshot;
        private FuntinoControlCenter.UI.AppLoadingIndicator loadingCustomers, loadingFamily, loadingPairing;
        private System.Windows.Forms.Timer tmrOperatorExpiry;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tlpRoot = new System.Windows.Forms.TableLayoutPanel();
            this.tmrOperatorExpiry = new System.Windows.Forms.Timer(this.components);
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.tlpHeader = new System.Windows.Forms.TableLayoutPanel();
            this.picHeader = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.btnBack = new FuntinoControlCenter.UI.RoundedButton();
            this.pnlSnapshot = new System.Windows.Forms.Panel();
            this.lblSnapshot = new System.Windows.Forms.Label();
            this.tlpBody = new System.Windows.Forms.TableLayoutPanel();
            this.pnlCustomers = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.tlpCustomers = new System.Windows.Forms.TableLayoutPanel();
            this.lblCustomerTitle = new System.Windows.Forms.Label();
            this.tlpSearch = new System.Windows.Forms.TableLayoutPanel();
            this.tlpPairing = new System.Windows.Forms.TableLayoutPanel();
            this.pnlSearchInput = new System.Windows.Forms.Panel();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.picSearch = new System.Windows.Forms.PictureBox();
            this.btnSearch = new FuntinoControlCenter.UI.RoundedButton();
            this.txtPairingCode = new System.Windows.Forms.TextBox();
            this.lblPairingStatus = new System.Windows.Forms.Label();
            this.btnPairOperator = new FuntinoControlCenter.UI.RoundedButton();
            this.btnCancelPairing = new FuntinoControlCenter.UI.RoundedButton();
            this.loadingPairing = new FuntinoControlCenter.UI.AppLoadingIndicator();
            this.pnlCustomerGrid = new System.Windows.Forms.Panel();
            this.gridCustomers = new System.Windows.Forms.DataGridView();
            this.colCustomerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMaskedPhone = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCustomerId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHouseholdId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMemberId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSnapshotVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.loadingCustomers = new FuntinoControlCenter.UI.AppLoadingIndicator();
            this.tlpCustomerPager = new System.Windows.Forms.TableLayoutPanel();
            this.btnNext = new FuntinoControlCenter.UI.RoundedButton();
            this.lblCustomerPage = new System.Windows.Forms.Label();
            this.btnPrevious = new FuntinoControlCenter.UI.RoundedButton();
            this.lblCustomerCount = new System.Windows.Forms.Label();
            this.pnlFamily = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.tlpFamily = new System.Windows.Forms.TableLayoutPanel();
            this.lblFamilyTitle = new System.Windows.Forms.Label();
            this.lblFamilyCustomer = new System.Windows.Forms.Label();
            this.pnlFamilyGrid = new System.Windows.Forms.Panel();
            this.gridFamily = new System.Windows.Forms.DataGridView();
            this.colFamilyType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFamilyName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRelatedCustomerId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.loadingFamily = new FuntinoControlCenter.UI.AppLoadingIndicator();
            this.lblCardsTitle = new System.Windows.Forms.Label();
            this.pnlCardsGrid = new System.Windows.Forms.Panel();
            this.gridCards = new System.Windows.Forms.DataGridView();
            this.colCardId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCardMemberId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCardSnapshot = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tlpRoot.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.tlpHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picHeader)).BeginInit();
            this.pnlSnapshot.SuspendLayout();
            this.tlpBody.SuspendLayout();
            this.pnlCustomers.SuspendLayout();
            this.tlpCustomers.SuspendLayout();
            this.tlpSearch.SuspendLayout();
            this.tlpPairing.SuspendLayout();
            this.pnlSearchInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSearch)).BeginInit();
            this.pnlCustomerGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridCustomers)).BeginInit();
            this.tlpCustomerPager.SuspendLayout();
            this.pnlFamily.SuspendLayout();
            this.tlpFamily.SuspendLayout();
            this.pnlFamilyGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridFamily)).BeginInit();
            this.pnlCardsGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridCards)).BeginInit();
            this.SuspendLayout();
            //
            // tlpRoot
            //
            this.tlpRoot.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(246)))));
            this.tlpRoot.ColumnCount = 1;
            this.tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Controls.Add(this.pnlHeader, 0, 0);
            this.tlpRoot.Controls.Add(this.pnlSnapshot, 0, 1);
            this.tlpRoot.Controls.Add(this.tlpBody, 0, 2);
            this.tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRoot.Location = new System.Drawing.Point(0, 0);
            this.tlpRoot.Name = "tlpRoot";
            this.tlpRoot.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
            this.tlpRoot.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpRoot.RowCount = 3;
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Size = new System.Drawing.Size(1200, 760);
            this.tlpRoot.TabIndex = 0;
            //
            // pnlHeader
            //
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(63)))), ((int)(((byte)(57)))));
            this.pnlHeader.Controls.Add(this.tlpHeader);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHeader.Location = new System.Drawing.Point(20, 16);
            this.pnlHeader.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1160, 78);
            this.pnlHeader.TabIndex = 0;
            //
            // tlpHeader
            //
            this.tlpHeader.ColumnCount = 3;
            this.tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 145F));
            this.tlpHeader.Controls.Add(this.picHeader, 0, 0);
            this.tlpHeader.Controls.Add(this.lblTitle, 1, 0);
            this.tlpHeader.Controls.Add(this.lblSubtitle, 1, 1);
            this.tlpHeader.Controls.Add(this.btnBack, 2, 0);
            this.tlpHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHeader.Location = new System.Drawing.Point(0, 0);
            this.tlpHeader.Name = "tlpHeader";
            this.tlpHeader.Padding = new System.Windows.Forms.Padding(8, 3, 8, 3);
            this.tlpHeader.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpHeader.RowCount = 2;
            this.tlpHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 58F));
            this.tlpHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 42F));
            this.tlpHeader.Size = new System.Drawing.Size(1160, 78);
            this.tlpHeader.TabIndex = 0;
            //
            // picHeader
            //
            this.picHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picHeader.Image = global::FuntinoControlCenter.Properties.Resources.CardHistory;
            this.picHeader.Location = new System.Drawing.Point(1072, 11);
            this.picHeader.Margin = new System.Windows.Forms.Padding(8);
            this.picHeader.Name = "picHeader";
            this.picHeader.Size = new System.Drawing.Size(72, 25);
            this.picHeader.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picHeader.TabIndex = 0;
            this.picHeader.TabStop = false;
            //
            // lblTitle
            //
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(156, 3);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblTitle.Size = new System.Drawing.Size(905, 41);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "مشتریان و خانواده‌های شعبه";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblSubtitle
            //
            this.lblSubtitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSubtitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(230)))), ((int)(((byte)(217)))));
            this.lblSubtitle.Location = new System.Drawing.Point(156, 44);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblSubtitle.Size = new System.Drawing.Size(905, 31);
            this.lblSubtitle.TabIndex = 2;
            this.lblSubtitle.Text = "جست‌وجوی نام و دیدن عضویت و کارت‌های مجاز در تصویر شعبه";
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // btnBack
            //
            this.btnBack.BackColor = System.Drawing.Color.White;
            this.btnBack.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(177)))), ((int)(((byte)(208)))), ((int)(((byte)(192)))));
            this.btnBack.BorderSize = 1;
            this.btnBack.CornerRadius = 10;
            this.btnBack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnBack.FlatAppearance.BorderSize = 0;
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnBack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(69)))), ((int)(((byte)(59)))));
            this.btnBack.Image = global::FuntinoControlCenter.Properties.Resources.Exit;
            this.btnBack.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnBack.Location = new System.Drawing.Point(13, 8);
            this.btnBack.Margin = new System.Windows.Forms.Padding(5);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(135, 31);
            this.btnBack.TabIndex = 3;
            this.btnBack.Text = "بازگشت";
            this.btnBack.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnBack.UseVisualStyleBackColor = false;
            this.btnBack.Click += new System.EventHandler(this.Back_Click);
            //
            // pnlSnapshot
            //
            this.pnlSnapshot.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(241)))), ((int)(((byte)(234)))));
            this.pnlSnapshot.Controls.Add(this.lblSnapshot);
            this.pnlSnapshot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSnapshot.Location = new System.Drawing.Point(20, 104);
            this.pnlSnapshot.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.pnlSnapshot.Name = "pnlSnapshot";
            this.pnlSnapshot.Size = new System.Drawing.Size(1160, 38);
            this.pnlSnapshot.TabIndex = 1;
            //
            // lblSnapshot
            //
            this.lblSnapshot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSnapshot.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F);
            this.lblSnapshot.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(86)))), ((int)(((byte)(69)))));
            this.lblSnapshot.Location = new System.Drawing.Point(0, 0);
            this.lblSnapshot.Name = "lblSnapshot";
            this.lblSnapshot.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.lblSnapshot.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblSnapshot.Size = new System.Drawing.Size(1160, 38);
            this.lblSnapshot.TabIndex = 0;
            this.lblSnapshot.Text = "اطلاعات از تصویر ذخیره‌شدهٔ شعبه خوانده می‌شود.";
            this.lblSnapshot.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // tlpBody
            //
            this.tlpBody.ColumnCount = 2;
            this.tlpBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpBody.Controls.Add(this.pnlCustomers, 0, 0);
            this.tlpBody.Controls.Add(this.pnlFamily, 1, 0);
            this.tlpBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBody.Location = new System.Drawing.Point(23, 155);
            this.tlpBody.Name = "tlpBody";
            this.tlpBody.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpBody.RowCount = 1;
            this.tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBody.Size = new System.Drawing.Size(1154, 586);
            this.tlpBody.TabIndex = 2;
            //
            // pnlCustomers
            //
            this.pnlCustomers.BackColor = System.Drawing.Color.White;
            this.pnlCustomers.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(221)))), ((int)(((byte)(212)))));
            this.pnlCustomers.BorderSize = 1;
            this.pnlCustomers.Controls.Add(this.tlpCustomers);
            this.pnlCustomers.CornerRadius = 15;
            this.pnlCustomers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCustomers.Location = new System.Drawing.Point(585, 0);
            this.pnlCustomers.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.pnlCustomers.Name = "pnlCustomers";
            this.pnlCustomers.Size = new System.Drawing.Size(569, 586);
            this.pnlCustomers.TabIndex = 0;
            //
            // tlpCustomers
            //
            this.tlpCustomers.ColumnCount = 1;
            this.tlpCustomers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCustomers.Controls.Add(this.lblCustomerTitle, 0, 0);
            this.tlpCustomers.Controls.Add(this.tlpSearch, 0, 1);
            this.tlpCustomers.Controls.Add(this.tlpPairing, 0, 2);
            this.tlpCustomers.Controls.Add(this.pnlCustomerGrid, 0, 3);
            this.tlpCustomers.Controls.Add(this.tlpCustomerPager, 0, 4);
            this.tlpCustomers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCustomers.Location = new System.Drawing.Point(0, 0);
            this.tlpCustomers.Name = "tlpCustomers";
            this.tlpCustomers.Padding = new System.Windows.Forms.Padding(12);
            this.tlpCustomers.RowCount = 5;
            this.tlpCustomers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tlpCustomers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 54F));
            this.tlpCustomers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpCustomers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCustomers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tlpCustomers.Size = new System.Drawing.Size(569, 586);
            this.tlpCustomers.TabIndex = 0;
            //
            // lblCustomerTitle
            //
            this.lblCustomerTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCustomerTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblCustomerTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(69)))), ((int)(((byte)(60)))));
            this.lblCustomerTitle.Location = new System.Drawing.Point(15, 12);
            this.lblCustomerTitle.Name = "lblCustomerTitle";
            this.lblCustomerTitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblCustomerTitle.Size = new System.Drawing.Size(539, 38);
            this.lblCustomerTitle.TabIndex = 0;
            this.lblCustomerTitle.Text = "فهرست مشتریان مجاز برای شعبه";
            this.lblCustomerTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // tlpSearch
            //
            this.tlpSearch.ColumnCount = 2;
            this.tlpSearch.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSearch.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 132F));
            this.tlpSearch.Controls.Add(this.pnlSearchInput, 0, 0);
            this.tlpSearch.Controls.Add(this.btnSearch, 1, 0);
            this.tlpSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSearch.Location = new System.Drawing.Point(15, 53);
            this.tlpSearch.Name = "tlpSearch";
            this.tlpSearch.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpSearch.RowCount = 1;
            this.tlpSearch.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSearch.Size = new System.Drawing.Size(539, 48);
            this.tlpSearch.TabIndex = 1;
            //
            // pnlSearchInput
            //
            this.pnlSearchInput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(251)))), ((int)(((byte)(249)))));
            this.pnlSearchInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlSearchInput.Controls.Add(this.txtSearch);
            this.pnlSearchInput.Controls.Add(this.picSearch);
            this.pnlSearchInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSearchInput.Location = new System.Drawing.Point(136, 4);
            this.pnlSearchInput.Margin = new System.Windows.Forms.Padding(4);
            this.pnlSearchInput.Name = "pnlSearchInput";
            this.pnlSearchInput.Size = new System.Drawing.Size(399, 40);
            this.pnlSearchInput.TabIndex = 0;
            //
            // txtSearch
            //
            this.txtSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(251)))), ((int)(((byte)(249)))));
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtSearch.Location = new System.Drawing.Point(0, 0);
            this.txtSearch.Margin = new System.Windows.Forms.Padding(8, 9, 4, 4);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.txtSearch.Size = new System.Drawing.Size(363, 14);
            this.txtSearch.TabIndex = 0;
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Search_KeyDown);
            //
            // picSearch
            //
            this.picSearch.Dock = System.Windows.Forms.DockStyle.Right;
            this.picSearch.Image = global::FuntinoControlCenter.Properties.Resources.CardHistory;
            this.picSearch.Location = new System.Drawing.Point(363, 0);
            this.picSearch.Margin = new System.Windows.Forms.Padding(7);
            this.picSearch.Name = "picSearch";
            this.picSearch.Size = new System.Drawing.Size(34, 38);
            this.picSearch.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picSearch.TabIndex = 1;
            this.picSearch.TabStop = false;
            //
            // btnSearch
            //
            this.btnSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(89)))), ((int)(((byte)(81)))));
            this.btnSearch.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(177)))), ((int)(((byte)(208)))), ((int)(((byte)(192)))));
            this.btnSearch.BorderSize = 1;
            this.btnSearch.CornerRadius = 10;
            this.btnSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSearch.FlatAppearance.BorderSize = 0;
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnSearch.ForeColor = System.Drawing.Color.White;
            this.btnSearch.Image = global::FuntinoControlCenter.Properties.Resources.Reports;
            this.btnSearch.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSearch.Location = new System.Drawing.Point(5, 5);
            this.btnSearch.Margin = new System.Windows.Forms.Padding(5);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(122, 38);
            this.btnSearch.TabIndex = 1;
            this.btnSearch.Text = "جست‌وجو";
            this.btnSearch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnSearch.UseVisualStyleBackColor = false;
            this.btnSearch.Click += new System.EventHandler(this.Search_Click);
            //
            // tlpPairing
            //
            this.tlpPairing.ColumnCount = 5;
            this.tlpPairing.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.tlpPairing.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112F));
            this.tlpPairing.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPairing.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tlpPairing.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 82F));
            this.tlpPairing.Controls.Add(this.btnPairOperator, 0, 0);
            this.tlpPairing.Controls.Add(this.txtPairingCode, 1, 0);
            this.tlpPairing.Controls.Add(this.lblPairingStatus, 2, 0);
            this.tlpPairing.Controls.Add(this.loadingPairing, 3, 0);
            this.tlpPairing.Controls.Add(this.btnCancelPairing, 4, 0);
            this.tlpPairing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPairing.Location = new System.Drawing.Point(15, 107);
            this.tlpPairing.Name = "tlpPairing";
            this.tlpPairing.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpPairing.RowCount = 1;
            this.tlpPairing.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPairing.Size = new System.Drawing.Size(539, 50);
            this.tlpPairing.TabIndex = 2;
            //
            // btnPairOperator
            //
            this.btnPairOperator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(89)))), ((int)(((byte)(81)))));
            this.btnPairOperator.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(177)))), ((int)(((byte)(208)))), ((int)(((byte)(192)))));
            this.btnPairOperator.BorderSize = 1;
            this.btnPairOperator.CornerRadius = 10;
            this.btnPairOperator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPairOperator.FlatAppearance.BorderSize = 0;
            this.btnPairOperator.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPairOperator.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.btnPairOperator.ForeColor = System.Drawing.Color.White;
            this.btnPairOperator.Image = global::FuntinoControlCenter.Properties.Resources.Reports;
            this.btnPairOperator.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnPairOperator.Location = new System.Drawing.Point(392, 4);
            this.btnPairOperator.Margin = new System.Windows.Forms.Padding(4);
            this.btnPairOperator.Name = "btnPairOperator";
            this.btnPairOperator.Size = new System.Drawing.Size(143, 42);
            this.btnPairOperator.TabIndex = 2;
            this.btnPairOperator.Text = "اتصال امن";
            this.btnPairOperator.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnPairOperator.UseVisualStyleBackColor = false;
            this.btnPairOperator.Click += new System.EventHandler(this.PairOperator_Click);
            //
            // txtPairingCode
            //
            this.txtPairingCode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(251)))), ((int)(((byte)(249)))));
            this.txtPairingCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPairingCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPairingCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.txtPairingCode.Location = new System.Drawing.Point(280, 7);
            this.txtPairingCode.Margin = new System.Windows.Forms.Padding(3, 7, 3, 7);
            this.txtPairingCode.Name = "txtPairingCode";
            this.txtPairingCode.ReadOnly = true;
            this.txtPairingCode.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtPairingCode.Size = new System.Drawing.Size(106, 23);
            this.txtPairingCode.TabIndex = 3;
            this.txtPairingCode.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            //
            // lblPairingStatus
            //
            this.lblPairingStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPairingStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F);
            this.lblPairingStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(73)))), ((int)(((byte)(95)))), ((int)(((byte)(85)))));
            this.lblPairingStatus.Location = new System.Drawing.Point(40, 0);
            this.lblPairingStatus.Name = "lblPairingStatus";
            this.lblPairingStatus.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblPairingStatus.Size = new System.Drawing.Size(234, 50);
            this.lblPairingStatus.TabIndex = 4;
            this.lblPairingStatus.Text = "برای خواندن مرکزی، اتصال را آغاز کنید.";
            this.lblPairingStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // loadingPairing
            //
            this.loadingPairing.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(247)))), ((int)(((byte)(243)))));
            this.loadingPairing.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.loadingPairing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loadingPairing.Location = new System.Drawing.Point(4, 4);
            this.loadingPairing.Margin = new System.Windows.Forms.Padding(4);
            this.loadingPairing.Name = "loadingPairing";
            this.loadingPairing.Size = new System.Drawing.Size(28, 42);
            this.loadingPairing.TabIndex = 5;
            this.loadingPairing.Visible = false;
            //
            // btnCancelPairing
            //
            this.btnCancelPairing.BackColor = System.Drawing.Color.White;
            this.btnCancelPairing.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(177)))), ((int)(((byte)(208)))), ((int)(((byte)(192)))));
            this.btnCancelPairing.BorderSize = 1;
            this.btnCancelPairing.CornerRadius = 10;
            this.btnCancelPairing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCancelPairing.FlatAppearance.BorderSize = 0;
            this.btnCancelPairing.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelPairing.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.btnCancelPairing.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(69)))), ((int)(((byte)(59)))));
            this.btnCancelPairing.Image = global::FuntinoControlCenter.Properties.Resources.Exit;
            this.btnCancelPairing.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancelPairing.Location = new System.Drawing.Point(4, 4);
            this.btnCancelPairing.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancelPairing.Name = "btnCancelPairing";
            this.btnCancelPairing.Size = new System.Drawing.Size(74, 42);
            this.btnCancelPairing.TabIndex = 6;
            this.btnCancelPairing.Text = "لغو";
            this.btnCancelPairing.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCancelPairing.UseVisualStyleBackColor = false;
            this.btnCancelPairing.Visible = false;
            this.btnCancelPairing.Click += new System.EventHandler(this.CancelPairing_Click);
            //
            // pnlCustomerGrid
            //
            this.pnlCustomerGrid.Controls.Add(this.gridCustomers);
            this.pnlCustomerGrid.Controls.Add(this.loadingCustomers);
            this.pnlCustomerGrid.Location = new System.Drawing.Point(354, 107);
            this.pnlCustomerGrid.Name = "pnlCustomerGrid";
            this.pnlCustomerGrid.Size = new System.Drawing.Size(200, 100);
            this.pnlCustomerGrid.TabIndex = 2;
            //
            // gridCustomers
            //
            this.gridCustomers.AllowUserToAddRows = false;
            this.gridCustomers.AllowUserToDeleteRows = false;
            this.gridCustomers.AllowUserToResizeRows = false;
            this.gridCustomers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridCustomers.BackgroundColor = System.Drawing.Color.White;
            this.gridCustomers.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridCustomers.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.gridCustomers.ColumnHeadersHeight = 40;
            this.gridCustomers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridCustomers.EnableHeadersVisualStyles = false;
            this.gridCustomers.AutoGenerateColumns = false;
            this.gridCustomers.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.gridCustomers.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(237)))), ((int)(((byte)(232)))));
            this.gridCustomers.Location = new System.Drawing.Point(0, 48);
            this.gridCustomers.MultiSelect = false;
            this.gridCustomers.Name = "gridCustomers";
            this.gridCustomers.ReadOnly = true;
            this.gridCustomers.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.gridCustomers.RowHeadersVisible = false;
            this.gridCustomers.RowTemplate.Height = 34;
            this.gridCustomers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridCustomers.Size = new System.Drawing.Size(200, 52);
            this.gridCustomers.TabIndex = 0;
            this.gridCustomers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colCustomerName, this.colMaskedPhone, this.colCustomerId, this.colHouseholdId, this.colMemberId, this.colSnapshotVersion});
            this.gridCustomers.SelectionChanged += new System.EventHandler(this.CustomerSelectionChanged);
            //
            // colCustomerName
            //
            this.colCustomerName.DataPropertyName = "DisplayName";
            this.colCustomerName.HeaderText = "نام مشتری";
            this.colCustomerName.Name = "colCustomerName";
            this.colCustomerName.ReadOnly = true;
            this.colCustomerName.FillWeight = 60F;
            //
            // colMaskedPhone
            //
            this.colMaskedPhone.DataPropertyName = "MaskedPhone";
            this.colMaskedPhone.HeaderText = "تلفن پوشیده";
            this.colMaskedPhone.Name = "colMaskedPhone";
            this.colMaskedPhone.ReadOnly = true;
            this.colMaskedPhone.FillWeight = 40F;
            //
            // colCustomerId
            //
            this.colCustomerId.DataPropertyName = "CustomerId";
            this.colCustomerId.HeaderText = "شناسهٔ مشتری";
            this.colCustomerId.Name = "colCustomerId";
            this.colCustomerId.ReadOnly = true;
            this.colCustomerId.Visible = false;
            //
            // colHouseholdId
            //
            this.colHouseholdId.DataPropertyName = "HouseholdId";
            this.colHouseholdId.HeaderText = "شناسهٔ خانواده";
            this.colHouseholdId.Name = "colHouseholdId";
            this.colHouseholdId.ReadOnly = true;
            this.colHouseholdId.Visible = false;
            //
            // colMemberId
            //
            this.colMemberId.DataPropertyName = "MemberId";
            this.colMemberId.HeaderText = "شناسهٔ عضو";
            this.colMemberId.Name = "colMemberId";
            this.colMemberId.ReadOnly = true;
            this.colMemberId.Visible = false;
            //
            // colSnapshotVersion
            //
            this.colSnapshotVersion.DataPropertyName = "SnapshotVersion";
            this.colSnapshotVersion.HeaderText = "نسخه";
            this.colSnapshotVersion.Name = "colSnapshotVersion";
            this.colSnapshotVersion.ReadOnly = true;
            this.colSnapshotVersion.Visible = false;
            //
            // loadingCustomers
            //
            this.loadingCustomers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(247)))), ((int)(((byte)(243)))));
            this.loadingCustomers.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.loadingCustomers.Dock = System.Windows.Forms.DockStyle.Top;
            this.loadingCustomers.Location = new System.Drawing.Point(0, 0);
            this.loadingCustomers.Name = "loadingCustomers";
            this.loadingCustomers.Size = new System.Drawing.Size(200, 48);
            this.loadingCustomers.TabIndex = 1;
            this.loadingCustomers.Visible = false;
            //
            // tlpCustomerPager
            //
            this.tlpCustomerPager.ColumnCount = 4;
            this.tlpCustomerPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112F));
            this.tlpCustomerPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112F));
            this.tlpCustomerPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tlpCustomerPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCustomerPager.Controls.Add(this.btnNext, 0, 0);
            this.tlpCustomerPager.Controls.Add(this.lblCustomerPage, 1, 0);
            this.tlpCustomerPager.Controls.Add(this.btnPrevious, 2, 0);
            this.tlpCustomerPager.Controls.Add(this.lblCustomerCount, 3, 0);
            this.tlpCustomerPager.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCustomerPager.Location = new System.Drawing.Point(15, 525);
            this.tlpCustomerPager.Name = "tlpCustomerPager";
            this.tlpCustomerPager.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpCustomerPager.RowCount = 1;
            this.tlpCustomerPager.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCustomerPager.Size = new System.Drawing.Size(539, 46);
            this.tlpCustomerPager.TabIndex = 3;
            //
            // btnNext
            //
            this.btnNext.BackColor = System.Drawing.Color.White;
            this.btnNext.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(177)))), ((int)(((byte)(208)))), ((int)(((byte)(192)))));
            this.btnNext.BorderSize = 1;
            this.btnNext.CornerRadius = 10;
            this.btnNext.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNext.FlatAppearance.BorderSize = 0;
            this.btnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNext.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnNext.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(69)))), ((int)(((byte)(59)))));
            this.btnNext.Image = global::FuntinoControlCenter.Properties.Resources.Reports;
            this.btnNext.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNext.Location = new System.Drawing.Point(432, 5);
            this.btnNext.Margin = new System.Windows.Forms.Padding(5);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(102, 36);
            this.btnNext.TabIndex = 0;
            this.btnNext.Text = "صفحهٔ بعد";
            this.btnNext.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnNext.UseVisualStyleBackColor = false;
            this.btnNext.Click += new System.EventHandler(this.Next_Click);
            //
            // lblCustomerPage
            //
            this.lblCustomerPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCustomerPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.lblCustomerPage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(73)))), ((int)(((byte)(95)))), ((int)(((byte)(85)))));
            this.lblCustomerPage.Location = new System.Drawing.Point(318, 0);
            this.lblCustomerPage.Name = "lblCustomerPage";
            this.lblCustomerPage.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblCustomerPage.Size = new System.Drawing.Size(106, 46);
            this.lblCustomerPage.TabIndex = 1;
            this.lblCustomerPage.Text = "صفحهٔ ۱ از ۱";
            this.lblCustomerPage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // btnPrevious
            //
            this.btnPrevious.BackColor = System.Drawing.Color.White;
            this.btnPrevious.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(177)))), ((int)(((byte)(208)))), ((int)(((byte)(192)))));
            this.btnPrevious.BorderSize = 1;
            this.btnPrevious.CornerRadius = 10;
            this.btnPrevious.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPrevious.FlatAppearance.BorderSize = 0;
            this.btnPrevious.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrevious.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnPrevious.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(69)))), ((int)(((byte)(59)))));
            this.btnPrevious.Image = global::FuntinoControlCenter.Properties.Resources.Reports;
            this.btnPrevious.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnPrevious.Location = new System.Drawing.Point(220, 5);
            this.btnPrevious.Margin = new System.Windows.Forms.Padding(5);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(90, 36);
            this.btnPrevious.TabIndex = 2;
            this.btnPrevious.Text = "صفحهٔ قبل";
            this.btnPrevious.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnPrevious.UseVisualStyleBackColor = false;
            this.btnPrevious.Click += new System.EventHandler(this.Previous_Click);
            //
            // lblCustomerCount
            //
            this.lblCustomerCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCustomerCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.lblCustomerCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(73)))), ((int)(((byte)(95)))), ((int)(((byte)(85)))));
            this.lblCustomerCount.Location = new System.Drawing.Point(3, 0);
            this.lblCustomerCount.Name = "lblCustomerCount";
            this.lblCustomerCount.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblCustomerCount.Size = new System.Drawing.Size(209, 46);
            this.lblCustomerCount.TabIndex = 3;
            this.lblCustomerCount.Text = "۰ مشتری";
            this.lblCustomerCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // pnlFamily
            //
            this.pnlFamily.BackColor = System.Drawing.Color.White;
            this.pnlFamily.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(221)))), ((int)(((byte)(212)))));
            this.pnlFamily.BorderSize = 1;
            this.pnlFamily.Controls.Add(this.tlpFamily);
            this.pnlFamily.CornerRadius = 15;
            this.pnlFamily.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFamily.Location = new System.Drawing.Point(0, 0);
            this.pnlFamily.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.pnlFamily.Name = "pnlFamily";
            this.pnlFamily.Size = new System.Drawing.Size(569, 586);
            this.pnlFamily.TabIndex = 1;
            this.pnlFamily.Visible = false;
            //
            // tlpFamily
            //
            this.tlpFamily.ColumnCount = 1;
            this.tlpFamily.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFamily.Controls.Add(this.lblFamilyTitle, 0, 0);
            this.tlpFamily.Controls.Add(this.lblFamilyCustomer, 0, 1);
            this.tlpFamily.Controls.Add(this.pnlFamilyGrid, 0, 2);
            this.tlpFamily.Controls.Add(this.lblCardsTitle, 0, 3);
            this.tlpFamily.Controls.Add(this.pnlCardsGrid, 0, 4);
            this.tlpFamily.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpFamily.Location = new System.Drawing.Point(0, 0);
            this.tlpFamily.Name = "tlpFamily";
            this.tlpFamily.Padding = new System.Windows.Forms.Padding(12);
            this.tlpFamily.RowCount = 5;
            this.tlpFamily.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tlpFamily.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpFamily.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tlpFamily.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tlpFamily.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tlpFamily.Size = new System.Drawing.Size(569, 586);
            this.tlpFamily.TabIndex = 0;
            //
            // lblFamilyTitle
            //
            this.lblFamilyTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFamilyTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblFamilyTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(69)))), ((int)(((byte)(60)))));
            this.lblFamilyTitle.Location = new System.Drawing.Point(15, 12);
            this.lblFamilyTitle.Name = "lblFamilyTitle";
            this.lblFamilyTitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblFamilyTitle.Size = new System.Drawing.Size(539, 36);
            this.lblFamilyTitle.TabIndex = 0;
            this.lblFamilyTitle.Text = "اعضای خانواده";
            this.lblFamilyTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblFamilyCustomer
            //
            this.lblFamilyCustomer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFamilyCustomer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.lblFamilyCustomer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(73)))), ((int)(((byte)(95)))), ((int)(((byte)(85)))));
            this.lblFamilyCustomer.Location = new System.Drawing.Point(15, 48);
            this.lblFamilyCustomer.Name = "lblFamilyCustomer";
            this.lblFamilyCustomer.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblFamilyCustomer.Size = new System.Drawing.Size(539, 40);
            this.lblFamilyCustomer.TabIndex = 1;
            this.lblFamilyCustomer.Text = "برای دیدن اطلاعات، یک مشتری را انتخاب کنید.";
            this.lblFamilyCustomer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // pnlFamilyGrid
            //
            this.pnlFamilyGrid.Controls.Add(this.gridFamily);
            this.pnlFamilyGrid.Controls.Add(this.loadingFamily);
            this.pnlFamilyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFamilyGrid.Location = new System.Drawing.Point(15, 91);
            this.pnlFamilyGrid.Name = "pnlFamilyGrid";
            this.pnlFamilyGrid.Size = new System.Drawing.Size(539, 241);
            this.pnlFamilyGrid.TabIndex = 2;
            //
            // gridFamily
            //
            this.gridFamily.AllowUserToAddRows = false;
            this.gridFamily.AllowUserToDeleteRows = false;
            this.gridFamily.AllowUserToResizeRows = false;
            this.gridFamily.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridFamily.BackgroundColor = System.Drawing.Color.White;
            this.gridFamily.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridFamily.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.gridFamily.ColumnHeadersHeight = 40;
            this.gridFamily.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridFamily.EnableHeadersVisualStyles = false;
            this.gridFamily.AutoGenerateColumns = false;
            this.gridFamily.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.gridFamily.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(237)))), ((int)(((byte)(232)))));
            this.gridFamily.Location = new System.Drawing.Point(0, 48);
            this.gridFamily.MultiSelect = false;
            this.gridFamily.Name = "gridFamily";
            this.gridFamily.ReadOnly = true;
            this.gridFamily.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.gridFamily.RowHeadersVisible = false;
            this.gridFamily.RowTemplate.Height = 34;
            this.gridFamily.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridFamily.Size = new System.Drawing.Size(539, 193);
            this.gridFamily.TabIndex = 0;
            this.gridFamily.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colFamilyType, this.colFamilyName, this.colRelatedCustomerId});
            //
            // colFamilyType
            //
            this.colFamilyType.DataPropertyName = "MemberType";
            this.colFamilyType.HeaderText = "نقش";
            this.colFamilyType.Name = "MemberType";
            this.colFamilyType.ReadOnly = true;
            this.colFamilyType.FillWeight = 25F;
            //
            // colFamilyName
            //
            this.colFamilyName.DataPropertyName = "DisplayName";
            this.colFamilyName.HeaderText = "نام";
            this.colFamilyName.Name = "DisplayName";
            this.colFamilyName.ReadOnly = true;
            this.colFamilyName.FillWeight = 40F;
            //
            // colRelatedCustomerId
            //
            this.colRelatedCustomerId.DataPropertyName = "RelatedCustomerId";
            this.colRelatedCustomerId.HeaderText = "شناسهٔ مشتری مرتبط";
            this.colRelatedCustomerId.Name = "RelatedCustomerId";
            this.colRelatedCustomerId.ReadOnly = true;
            this.colRelatedCustomerId.FillWeight = 35F;
            //
            // loadingFamily
            //
            this.loadingFamily.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(247)))), ((int)(((byte)(243)))));
            this.loadingFamily.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.loadingFamily.Dock = System.Windows.Forms.DockStyle.Top;
            this.loadingFamily.Location = new System.Drawing.Point(0, 0);
            this.loadingFamily.Name = "loadingFamily";
            this.loadingFamily.Size = new System.Drawing.Size(539, 48);
            this.loadingFamily.TabIndex = 1;
            this.loadingFamily.Visible = false;
            //
            // lblCardsTitle
            //
            this.lblCardsTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCardsTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblCardsTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(69)))), ((int)(((byte)(60)))));
            this.lblCardsTitle.Location = new System.Drawing.Point(15, 335);
            this.lblCardsTitle.Name = "lblCardsTitle";
            this.lblCardsTitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblCardsTitle.Size = new System.Drawing.Size(539, 36);
            this.lblCardsTitle.TabIndex = 3;
            this.lblCardsTitle.Text = "کارت‌های مجاز خانواده";
            this.lblCardsTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // pnlCardsGrid
            //
            this.pnlCardsGrid.Controls.Add(this.gridCards);
            this.pnlCardsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCardsGrid.Location = new System.Drawing.Point(15, 374);
            this.pnlCardsGrid.Name = "pnlCardsGrid";
            this.pnlCardsGrid.Size = new System.Drawing.Size(539, 197);
            this.pnlCardsGrid.TabIndex = 4;
            //
            // gridCards
            //
            this.gridCards.AllowUserToAddRows = false;
            this.gridCards.AllowUserToDeleteRows = false;
            this.gridCards.AllowUserToResizeRows = false;
            this.gridCards.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridCards.BackgroundColor = System.Drawing.Color.White;
            this.gridCards.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridCards.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.gridCards.ColumnHeadersHeight = 40;
            this.gridCards.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridCards.EnableHeadersVisualStyles = false;
            this.gridCards.AutoGenerateColumns = false;
            this.gridCards.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.gridCards.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(237)))), ((int)(((byte)(232)))));
            this.gridCards.Location = new System.Drawing.Point(0, 0);
            this.gridCards.MultiSelect = false;
            this.gridCards.Name = "gridCards";
            this.gridCards.ReadOnly = true;
            this.gridCards.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.gridCards.RowHeadersVisible = false;
            this.gridCards.RowTemplate.Height = 34;
            this.gridCards.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridCards.Size = new System.Drawing.Size(539, 197);
            this.gridCards.TabIndex = 0;
            this.gridCards.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colCardId, this.colCardMemberId, this.colCardSnapshot});
            //
            // colCardId
            //
            this.colCardId.DataPropertyName = "CardId";
            this.colCardId.HeaderText = "شناسهٔ کارت";
            this.colCardId.Name = "CardId";
            this.colCardId.ReadOnly = true;
            this.colCardId.FillWeight = 45F;
            //
            // colCardMemberId
            //
            this.colCardMemberId.DataPropertyName = "MemberId";
            this.colCardMemberId.HeaderText = "شناسهٔ عضو";
            this.colCardMemberId.Name = "MemberId";
            this.colCardMemberId.ReadOnly = true;
            this.colCardMemberId.FillWeight = 35F;
            //
            // colCardSnapshot
            //
            this.colCardSnapshot.DataPropertyName = "SnapshotVersion";
            this.colCardSnapshot.HeaderText = "نسخهٔ تصویر";
            this.colCardSnapshot.Name = "SnapshotVersion";
            this.colCardSnapshot.ReadOnly = true;
            this.colCardSnapshot.FillWeight = 20F;
            //
            // CustomerFamilyForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 760);
            this.Controls.Add(this.tlpRoot);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.MinimumSize = new System.Drawing.Size(1040, 650);
            this.Name = "CustomerFamilyForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "مشتریان و خانواده‌ها";
            this.Load += new System.EventHandler(this.CustomerFamilyForm_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CustomerFamilyForm_FormClosed);
            this.tmrOperatorExpiry.Interval = 1000;
            this.tmrOperatorExpiry.Tick += new System.EventHandler(this.OperatorSessionTimer_Tick);
            this.tlpRoot.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.tlpHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picHeader)).EndInit();
            this.pnlSnapshot.ResumeLayout(false);
            this.tlpBody.ResumeLayout(false);
            this.pnlCustomers.ResumeLayout(false);
            this.tlpCustomers.ResumeLayout(false);
            this.tlpSearch.ResumeLayout(false);
            this.tlpPairing.ResumeLayout(false);
            this.tlpPairing.PerformLayout();
            this.pnlSearchInput.ResumeLayout(false);
            this.pnlSearchInput.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSearch)).EndInit();
            this.pnlCustomerGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridCustomers)).EndInit();
            this.tlpCustomerPager.ResumeLayout(false);
            this.pnlFamily.ResumeLayout(false);
            this.tlpFamily.ResumeLayout(false);
            this.pnlFamilyGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridFamily)).EndInit();
            this.pnlCardsGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridCards)).EndInit();
            this.ResumeLayout(false);

        }

    }
}
