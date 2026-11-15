namespace FuntinoControlCenter.Forms
{
    partial class ReportsForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel tlpRoot, tlpHeader, tlpHeaderText, tlpFilters, tlpSearchInput, tlpSummary, tlpSummaryText, tlpGridCard, tlpGridHeading, tlpPager;
        private System.Windows.Forms.Panel pnlHeader, pnlGridViewport, pnlSummaryStatus;
        private FuntinoControlCenter.UI.RoundedBorderPanel pnlFilters, pnlDatabaseInput, pnlTableInput, pnlSearchInput, pnlSummaryBar, pnlGridCardBorder;
        private System.Windows.Forms.Label lblTitle, lblSubtitle, lblFilterTitle, lblDatabase, lblTable, lblSearch, lblSummary, lblGenerated, lblGridTitle, lblGridHint, lblEmptyState, lblPage, lblPageHint;
        private System.Windows.Forms.PictureBox picHeader, picSearch;
        private System.Windows.Forms.ComboBox cmbDatabase, cmbTables;
        private System.Windows.Forms.TextBox txtSearch;
        private FuntinoControlCenter.UI.RoundedButton btnBack, btnGenerate, btnExcel, btnPdf, btnPrevious, btnNext;
        private System.Windows.Forms.FlowLayoutPanel flpExports;
        private System.Windows.Forms.DataGridView grid;
        private FuntinoControlCenter.UI.AppLoadingIndicator loadingReports;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tlpRoot = new System.Windows.Forms.TableLayoutPanel();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.tlpHeader = new System.Windows.Forms.TableLayoutPanel();
            this.picHeader = new System.Windows.Forms.PictureBox();
            this.tlpHeaderText = new System.Windows.Forms.TableLayoutPanel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.btnBack = new FuntinoControlCenter.UI.RoundedButton();
            this.pnlFilters = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.tlpFilters = new System.Windows.Forms.TableLayoutPanel();
            this.lblFilterTitle = new System.Windows.Forms.Label();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.lblTable = new System.Windows.Forms.Label();
            this.lblSearch = new System.Windows.Forms.Label();
            this.pnlDatabaseInput = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.cmbDatabase = new System.Windows.Forms.ComboBox();
            this.pnlTableInput = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.cmbTables = new System.Windows.Forms.ComboBox();
            this.pnlSearchInput = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.tlpSearchInput = new System.Windows.Forms.TableLayoutPanel();
            this.picSearch = new System.Windows.Forms.PictureBox();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnGenerate = new FuntinoControlCenter.UI.RoundedButton();
            this.loadingReports = new FuntinoControlCenter.UI.AppLoadingIndicator();
            this.pnlSummaryBar = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.tlpSummary = new System.Windows.Forms.TableLayoutPanel();
            this.tlpSummaryText = new System.Windows.Forms.TableLayoutPanel();
            this.pnlSummaryStatus = new System.Windows.Forms.Panel();
            this.lblSummary = new System.Windows.Forms.Label();
            this.lblGenerated = new System.Windows.Forms.Label();
            this.flpExports = new System.Windows.Forms.FlowLayoutPanel();
            this.btnExcel = new FuntinoControlCenter.UI.RoundedButton();
            this.btnPdf = new FuntinoControlCenter.UI.RoundedButton();
            this.pnlGridCardBorder = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.tlpGridCard = new System.Windows.Forms.TableLayoutPanel();
            this.tlpGridHeading = new System.Windows.Forms.TableLayoutPanel();
            this.lblGridTitle = new System.Windows.Forms.Label();
            this.lblGridHint = new System.Windows.Forms.Label();
            this.pnlGridViewport = new System.Windows.Forms.Panel();
            this.grid = new System.Windows.Forms.DataGridView();
            this.lblEmptyState = new System.Windows.Forms.Label();
            this.tlpPager = new System.Windows.Forms.TableLayoutPanel();
            this.btnPrevious = new FuntinoControlCenter.UI.RoundedButton();
            this.lblPage = new System.Windows.Forms.Label();
            this.btnNext = new FuntinoControlCenter.UI.RoundedButton();
            this.lblPageHint = new System.Windows.Forms.Label();
            this.tlpRoot.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.tlpHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picHeader)).BeginInit();
            this.tlpHeaderText.SuspendLayout();
            this.pnlFilters.SuspendLayout();
            this.tlpFilters.SuspendLayout();
            this.pnlDatabaseInput.SuspendLayout();
            this.pnlTableInput.SuspendLayout();
            this.pnlSearchInput.SuspendLayout();
            this.tlpSearchInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSearch)).BeginInit();
            this.pnlSummaryBar.SuspendLayout();
            this.tlpSummary.SuspendLayout();
            this.tlpSummaryText.SuspendLayout();
            this.flpExports.SuspendLayout();
            this.pnlGridCardBorder.SuspendLayout();
            this.tlpGridCard.SuspendLayout();
            this.tlpGridHeading.SuspendLayout();
            this.pnlGridViewport.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.tlpPager.SuspendLayout();
            this.SuspendLayout();
            //
            // tlpRoot
            //
            this.tlpRoot.BackColor = System.Drawing.Color.Transparent;
            this.tlpRoot.ColumnCount = 1;
            this.tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Controls.Add(this.pnlHeader, 0, 0);
            this.tlpRoot.Controls.Add(this.pnlFilters, 0, 1);
            this.tlpRoot.Controls.Add(this.pnlSummaryBar, 0, 2);
            this.tlpRoot.Controls.Add(this.pnlGridCardBorder, 0, 3);
            this.tlpRoot.Controls.Add(this.tlpPager, 0, 4);
            this.tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRoot.Padding = new System.Windows.Forms.Padding(18, 14, 18, 14);
            this.tlpRoot.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpRoot.RowCount = 5;
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 78F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 126F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 84F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.tlpRoot.Name = "tlpRoot";
            this.tlpRoot.TabIndex = 0;
            //
            // pnlHeader
            //
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(20, 55, 58);
            this.pnlHeader.Controls.Add(this.tlpHeader);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHeader.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(12, 6, 12, 6);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.TabIndex = 0;
            //
            // tlpHeader
            //
            this.tlpHeader.BackColor = System.Drawing.Color.Transparent;
            this.tlpHeader.ColumnCount = 3;
            this.tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 132F));
            this.tlpHeader.Controls.Add(this.picHeader, 0, 0);
            this.tlpHeader.Controls.Add(this.tlpHeaderText, 1, 0);
            this.tlpHeader.Controls.Add(this.btnBack, 2, 0);
            this.tlpHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHeader.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpHeader.RowCount = 1;
            this.tlpHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHeader.Name = "tlpHeader";
            this.tlpHeader.TabIndex = 0;
            //
            // picHeader
            //
            this.picHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picHeader.Image = global::FuntinoControlCenter.Properties.ReportsVisualAssets.Report;
            this.picHeader.Margin = new System.Windows.Forms.Padding(4);
            this.picHeader.Name = "picHeader";
            this.picHeader.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picHeader.TabStop = false;
            //
            // tlpHeaderText
            //
            this.tlpHeaderText.ColumnCount = 1;
            this.tlpHeaderText.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHeaderText.Controls.Add(this.lblTitle, 0, 0);
            this.tlpHeaderText.Controls.Add(this.lblSubtitle, 0, 1);
            this.tlpHeaderText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHeaderText.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.tlpHeaderText.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpHeaderText.RowCount = 2;
            this.tlpHeaderText.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 58F));
            this.tlpHeaderText.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 42F));
            this.tlpHeaderText.Name = "tlpHeaderText";
            this.tlpHeaderText.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 17F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblTitle.Text = "گزارش‌های مدیریتی";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.TabIndex = 0;
            //
            // lblSubtitle
            //
            this.lblSubtitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSubtitle.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(215, 234, 226);
            this.lblSubtitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblSubtitle.Text = "اطلاعات مجاز را جست‌وجو کنید و در صورت نیاز خروجی بگیرید.";
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.TabIndex = 1;
            //
            // btnBack
            //
            this.btnBack.BackColor = System.Drawing.Color.White;
            this.btnBack.BorderColor = System.Drawing.Color.FromArgb(174, 202, 190);
            this.btnBack.BorderSize = 1;
            this.btnBack.CornerRadius = 13;
            this.btnBack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnBack.FlatAppearance.BorderSize = 0;
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9F, System.Drawing.FontStyle.Bold);
            this.btnBack.ForeColor = System.Drawing.Color.FromArgb(27, 81, 72);
            this.btnBack.Image = global::FuntinoControlCenter.Properties.ReportsVisualAssets.Back;
            this.btnBack.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnBack.Margin = new System.Windows.Forms.Padding(2, 6, 2, 6);
            this.btnBack.Name = "btnBack";
            this.btnBack.Padding = new System.Windows.Forms.Padding(9, 2, 9, 2);
            this.btnBack.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.btnBack.Size = new System.Drawing.Size(128, 52);
            this.btnBack.TabIndex = 0;
            this.btnBack.Text = "بازگشت";
            this.btnBack.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnBack.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnBack.UseVisualStyleBackColor = false;
            //
            // pnlFilters
            //
            this.pnlFilters.BackColor = System.Drawing.Color.White;
            this.pnlFilters.BorderColor = System.Drawing.Color.FromArgb(184, 211, 199);
            this.pnlFilters.BorderSize = 1;
            this.pnlFilters.CornerRadius = 18;
            this.pnlFilters.Controls.Add(this.tlpFilters);
            this.pnlFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFilters.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.pnlFilters.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.pnlFilters.Name = "pnlFilters";
            this.pnlFilters.TabIndex = 1;
            //
            // tlpFilters
            //
            this.tlpFilters.ColumnCount = 4;
            this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24F));
            this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32F));
            this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32F));
            this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12F));
            this.tlpFilters.Controls.Add(this.lblFilterTitle, 0, 0);
            this.tlpFilters.SetColumnSpan(this.lblFilterTitle, 4);
            this.tlpFilters.Controls.Add(this.lblDatabase, 0, 1);
            this.tlpFilters.Controls.Add(this.lblTable, 1, 1);
            this.tlpFilters.Controls.Add(this.lblSearch, 2, 1);
            this.tlpFilters.Controls.Add(this.pnlDatabaseInput, 0, 2);
            this.tlpFilters.Controls.Add(this.pnlTableInput, 1, 2);
            this.tlpFilters.Controls.Add(this.pnlSearchInput, 2, 2);
            this.tlpFilters.Controls.Add(this.btnGenerate, 3, 1);
            this.tlpFilters.SetRowSpan(this.btnGenerate, 2);
            this.tlpFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpFilters.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpFilters.RowCount = 3;
            this.tlpFilters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpFilters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlpFilters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tlpFilters.Name = "tlpFilters";
            this.tlpFilters.TabIndex = 0;
            //
            // lblFilterTitle
            //
            this.lblFilterTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFilterTitle.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 11F, System.Drawing.FontStyle.Bold);
            this.lblFilterTitle.ForeColor = System.Drawing.Color.FromArgb(24, 80, 72);
            this.lblFilterTitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblFilterTitle.Text = "انتخاب و جست‌وجوی گزارش";
            this.lblFilterTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblFilterTitle.Name = "lblFilterTitle";
            this.lblFilterTitle.TabIndex = 0;
            //
            // lblDatabase
            //
            this.lblDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDatabase.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F, System.Drawing.FontStyle.Bold);
            this.lblDatabase.ForeColor = System.Drawing.Color.FromArgb(54, 78, 71);
            this.lblDatabase.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblDatabase.Text = "پایگاه داده";
            this.lblDatabase.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.TabIndex = 1;
            //
            // lblTable
            //
            this.lblTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTable.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F, System.Drawing.FontStyle.Bold);
            this.lblTable.ForeColor = System.Drawing.Color.FromArgb(54, 78, 71);
            this.lblTable.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblTable.Text = "جدول گزارش";
            this.lblTable.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblTable.Name = "lblTable";
            this.lblTable.TabIndex = 2;
            //
            // lblSearch
            //
            this.lblSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSearch.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F, System.Drawing.FontStyle.Bold);
            this.lblSearch.ForeColor = System.Drawing.Color.FromArgb(54, 78, 71);
            this.lblSearch.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblSearch.Text = "جست‌وجو در جدول";
            this.lblSearch.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.TabIndex = 3;
            //
            // pnlDatabaseInput
            //
            this.pnlDatabaseInput.BackColor = System.Drawing.Color.FromArgb(247, 250, 248);
            this.pnlDatabaseInput.BorderColor = System.Drawing.Color.FromArgb(166, 198, 185);
            this.pnlDatabaseInput.BorderSize = 1;
            this.pnlDatabaseInput.CornerRadius = 10;
            this.pnlDatabaseInput.Controls.Add(this.cmbDatabase);
            this.pnlDatabaseInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDatabaseInput.Margin = new System.Windows.Forms.Padding(4, 1, 4, 2);
            this.pnlDatabaseInput.Padding = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.pnlDatabaseInput.Name = "pnlDatabaseInput";
            this.pnlDatabaseInput.TabIndex = 0;
            //
            // cmbDatabase
            //
            this.cmbDatabase.BackColor = System.Drawing.Color.FromArgb(247, 250, 248);
            this.cmbDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbDatabase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDatabase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbDatabase.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9F);
            this.cmbDatabase.FormattingEnabled = true;
            this.cmbDatabase.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cmbDatabase.Name = "cmbDatabase";
            this.cmbDatabase.TabIndex = 0;
            //
            // pnlTableInput
            //
            this.pnlTableInput.BackColor = System.Drawing.Color.FromArgb(247, 250, 248);
            this.pnlTableInput.BorderColor = System.Drawing.Color.FromArgb(166, 198, 185);
            this.pnlTableInput.BorderSize = 1;
            this.pnlTableInput.CornerRadius = 10;
            this.pnlTableInput.Controls.Add(this.cmbTables);
            this.pnlTableInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTableInput.Margin = new System.Windows.Forms.Padding(4, 1, 4, 2);
            this.pnlTableInput.Padding = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.pnlTableInput.Name = "pnlTableInput";
            this.pnlTableInput.TabIndex = 1;
            //
            // cmbTables
            //
            this.cmbTables.BackColor = System.Drawing.Color.FromArgb(247, 250, 248);
            this.cmbTables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbTables.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTables.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbTables.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9F);
            this.cmbTables.FormattingEnabled = true;
            this.cmbTables.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cmbTables.Name = "cmbTables";
            this.cmbTables.TabIndex = 1;
            //
            // pnlSearchInput
            //
            this.pnlSearchInput.BackColor = System.Drawing.Color.FromArgb(247, 250, 248);
            this.pnlSearchInput.BorderColor = System.Drawing.Color.FromArgb(166, 198, 185);
            this.pnlSearchInput.BorderSize = 1;
            this.pnlSearchInput.CornerRadius = 10;
            this.pnlSearchInput.Controls.Add(this.tlpSearchInput);
            this.pnlSearchInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSearchInput.Margin = new System.Windows.Forms.Padding(4, 1, 4, 2);
            this.pnlSearchInput.Padding = new System.Windows.Forms.Padding(3, 2, 5, 2);
            this.pnlSearchInput.Name = "pnlSearchInput";
            this.pnlSearchInput.TabIndex = 2;
            //
            // tlpSearchInput
            //
            this.tlpSearchInput.ColumnCount = 2;
            this.tlpSearchInput.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tlpSearchInput.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSearchInput.Controls.Add(this.picSearch, 0, 0);
            this.tlpSearchInput.Controls.Add(this.txtSearch, 1, 0);
            this.tlpSearchInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSearchInput.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpSearchInput.RowCount = 1;
            this.tlpSearchInput.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSearchInput.Name = "tlpSearchInput";
            this.tlpSearchInput.TabIndex = 0;
            //
            // picSearch
            //
            this.picSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picSearch.Image = global::FuntinoControlCenter.Properties.ReportsVisualAssets.Search;
            this.picSearch.Margin = new System.Windows.Forms.Padding(5);
            this.picSearch.Name = "picSearch";
            this.picSearch.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picSearch.TabStop = false;
            //
            // txtSearch
            //
            this.txtSearch.BackColor = System.Drawing.Color.FromArgb(247, 250, 248);
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSearch.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9.5F);
            this.txtSearch.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.txtSearch.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.TabIndex = 2;
            //
            // btnGenerate
            //
            this.btnGenerate.BackColor = System.Drawing.Color.FromArgb(23, 100, 91);
            this.btnGenerate.BorderColor = System.Drawing.Color.FromArgb(16, 83, 75);
            this.btnGenerate.BorderSize = 1;
            this.btnGenerate.CornerRadius = 13;
            this.btnGenerate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnGenerate.FlatAppearance.BorderSize = 0;
            this.btnGenerate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerate.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9F, System.Drawing.FontStyle.Bold);
            this.btnGenerate.ForeColor = System.Drawing.Color.White;
            this.btnGenerate.Image = global::FuntinoControlCenter.Properties.ReportsVisualAssets.ReportAction;
            this.btnGenerate.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnGenerate.Margin = new System.Windows.Forms.Padding(4, 28, 4, 2);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Padding = new System.Windows.Forms.Padding(5, 2, 7, 2);
            this.btnGenerate.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.btnGenerate.Size = new System.Drawing.Size(116, 42);
            this.btnGenerate.TabIndex = 3;
            this.btnGenerate.Text = "نمایش گزارش";
            this.btnGenerate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnGenerate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnGenerate.UseVisualStyleBackColor = false;
            //
            // loadingReports
            //
            this.loadingReports.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loadingReports.Margin = new System.Windows.Forms.Padding(0);
            this.loadingReports.Name = "loadingReports";
            this.loadingReports.Size = new System.Drawing.Size(490, 34);
            this.loadingReports.TabIndex = 0;
            this.loadingReports.Visible = false;
            //
            // pnlSummaryBar
            //
            this.pnlSummaryBar.BackColor = System.Drawing.Color.White;
            this.pnlSummaryBar.BorderColor = System.Drawing.Color.FromArgb(184, 211, 199);
            this.pnlSummaryBar.BorderSize = 1;
            this.pnlSummaryBar.CornerRadius = 16;
            this.pnlSummaryBar.Controls.Add(this.tlpSummary);
            this.pnlSummaryBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSummaryBar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.pnlSummaryBar.Padding = new System.Windows.Forms.Padding(12, 4, 12, 4);
            this.pnlSummaryBar.Name = "pnlSummaryBar";
            this.pnlSummaryBar.TabIndex = 2;
            //
            // tlpSummary
            //
            this.tlpSummary.ColumnCount = 2;
            this.tlpSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 354F));
            this.tlpSummary.Controls.Add(this.tlpSummaryText, 0, 0);
            this.tlpSummary.Controls.Add(this.flpExports, 1, 0);
            this.tlpSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSummary.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpSummary.RowCount = 1;
            this.tlpSummary.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSummary.Name = "tlpSummary";
            this.tlpSummary.TabIndex = 0;
            //
            // tlpSummaryText
            //
            this.tlpSummaryText.ColumnCount = 1;
            this.tlpSummaryText.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSummaryText.Controls.Add(this.lblSummary, 0, 0);
            this.tlpSummaryText.Controls.Add(this.pnlSummaryStatus, 0, 1);
            this.tlpSummaryText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSummaryText.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpSummaryText.RowCount = 2;
            this.tlpSummaryText.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 46F));
            this.tlpSummaryText.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 54F));
            this.tlpSummaryText.Name = "tlpSummaryText";
            this.tlpSummaryText.TabIndex = 0;
            //
            // lblSummary
            //
            this.lblSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSummary.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9.5F, System.Drawing.FontStyle.Bold);
            this.lblSummary.ForeColor = System.Drawing.Color.FromArgb(30, 77, 68);
            this.lblSummary.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblSummary.Text = "برای شروع، پایگاه داده و جدول گزارش را انتخاب کنید.";
            this.lblSummary.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.TabIndex = 0;
            //
            // lblGenerated
            //
            this.lblGenerated.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGenerated.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F);
            this.lblGenerated.ForeColor = System.Drawing.Color.FromArgb(99, 121, 111);
            this.lblGenerated.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblGenerated.Text = "";
            this.lblGenerated.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblGenerated.Name = "lblGenerated";
            this.lblGenerated.TabIndex = 1;
            //
            // pnlSummaryStatus
            //
            this.pnlSummaryStatus.BackColor = System.Drawing.Color.Transparent;
            this.pnlSummaryStatus.Controls.Add(this.lblGenerated);
            this.pnlSummaryStatus.Controls.Add(this.loadingReports);
            this.pnlSummaryStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSummaryStatus.Name = "pnlSummaryStatus";
            this.pnlSummaryStatus.TabIndex = 1;
            //
            // flpExports
            //
            this.flpExports.Controls.Add(this.btnExcel);
            this.flpExports.Controls.Add(this.btnPdf);
            this.flpExports.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpExports.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpExports.Margin = new System.Windows.Forms.Padding(2, 0, 0, 0);
            this.flpExports.Name = "flpExports";
            this.flpExports.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.flpExports.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.flpExports.WrapContents = false;
            this.flpExports.TabIndex = 1;
            //
            // btnExcel
            //
            this.btnExcel.BackColor = System.Drawing.Color.FromArgb(244, 250, 246);
            this.btnExcel.BorderColor = System.Drawing.Color.FromArgb(159, 197, 176);
            this.btnExcel.BorderSize = 1;
            this.btnExcel.CornerRadius = 12;
            this.btnExcel.FlatAppearance.BorderSize = 0;
            this.btnExcel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExcel.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F, System.Drawing.FontStyle.Bold);
            this.btnExcel.ForeColor = System.Drawing.Color.FromArgb(27, 91, 65);
            this.btnExcel.Image = global::FuntinoControlCenter.Properties.ReportsVisualAssets.ExcelExport;
            this.btnExcel.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnExcel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnExcel.Name = "btnExcel";
            this.btnExcel.Padding = new System.Windows.Forms.Padding(7, 2, 9, 2);
            this.btnExcel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.btnExcel.Size = new System.Drawing.Size(172, 44);
            this.btnExcel.TabIndex = 0;
            this.btnExcel.Text = "صفحه‌گسترده";
            this.btnExcel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnExcel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnExcel.UseVisualStyleBackColor = false;
            //
            // btnPdf
            //
            this.btnPdf.BackColor = System.Drawing.Color.FromArgb(253, 247, 245);
            this.btnPdf.BorderColor = System.Drawing.Color.FromArgb(217, 178, 168);
            this.btnPdf.BorderSize = 1;
            this.btnPdf.CornerRadius = 12;
            this.btnPdf.FlatAppearance.BorderSize = 0;
            this.btnPdf.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPdf.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F, System.Drawing.FontStyle.Bold);
            this.btnPdf.ForeColor = System.Drawing.Color.FromArgb(133, 66, 55);
            this.btnPdf.Image = global::FuntinoControlCenter.Properties.ReportsVisualAssets.PdfExport;
            this.btnPdf.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnPdf.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPdf.Name = "btnPdf";
            this.btnPdf.Padding = new System.Windows.Forms.Padding(7, 2, 9, 2);
            this.btnPdf.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.btnPdf.Size = new System.Drawing.Size(150, 44);
            this.btnPdf.TabIndex = 1;
            this.btnPdf.Text = "نسخهٔ قابل‌چاپ";
            this.btnPdf.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnPdf.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnPdf.UseVisualStyleBackColor = false;
            //
            // pnlGridCardBorder
            //
            this.pnlGridCardBorder.BackColor = System.Drawing.Color.White;
            this.pnlGridCardBorder.BorderColor = System.Drawing.Color.FromArgb(184, 211, 199);
            this.pnlGridCardBorder.BorderSize = 1;
            this.pnlGridCardBorder.CornerRadius = 18;
            this.pnlGridCardBorder.Controls.Add(this.tlpGridCard);
            this.pnlGridCardBorder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGridCardBorder.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.pnlGridCardBorder.Padding = new System.Windows.Forms.Padding(10, 8, 10, 9);
            this.pnlGridCardBorder.Name = "pnlGridCardBorder";
            this.pnlGridCardBorder.TabIndex = 3;
            //
            // tlpGridCard
            //
            this.tlpGridCard.ColumnCount = 1;
            this.tlpGridCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGridCard.Controls.Add(this.tlpGridHeading, 0, 0);
            this.tlpGridCard.Controls.Add(this.pnlGridViewport, 0, 1);
            this.tlpGridCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGridCard.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpGridCard.RowCount = 2;
            this.tlpGridCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpGridCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGridCard.Name = "tlpGridCard";
            this.tlpGridCard.TabIndex = 0;
            //
            // tlpGridHeading
            //
            this.tlpGridHeading.ColumnCount = 2;
            this.tlpGridHeading.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 48F));
            this.tlpGridHeading.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 52F));
            this.tlpGridHeading.Controls.Add(this.lblGridTitle, 0, 0);
            this.tlpGridHeading.Controls.Add(this.lblGridHint, 1, 0);
            this.tlpGridHeading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGridHeading.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpGridHeading.RowCount = 1;
            this.tlpGridHeading.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGridHeading.Name = "tlpGridHeading";
            this.tlpGridHeading.TabIndex = 0;
            //
            // lblGridTitle
            //
            this.lblGridTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGridTitle.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 10.5F, System.Drawing.FontStyle.Bold);
            this.lblGridTitle.ForeColor = System.Drawing.Color.FromArgb(24, 80, 72);
            this.lblGridTitle.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblGridTitle.Text = "نتایج گزارش";
            this.lblGridTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblGridTitle.Name = "lblGridTitle";
            this.lblGridTitle.TabIndex = 0;
            //
            // lblGridHint
            //
            this.lblGridHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGridHint.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F);
            this.lblGridHint.ForeColor = System.Drawing.Color.FromArgb(105, 126, 117);
            this.lblGridHint.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblGridHint.Text = "برای جست‌وجو، عبارت را بنویسید و Enter بزنید.";
            this.lblGridHint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblGridHint.Name = "lblGridHint";
            this.lblGridHint.TabIndex = 1;
            //
            // pnlGridViewport
            //
            this.pnlGridViewport.BackColor = System.Drawing.Color.White;
            this.pnlGridViewport.Controls.Add(this.grid);
            this.pnlGridViewport.Controls.Add(this.lblEmptyState);
            this.pnlGridViewport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGridViewport.Name = "pnlGridViewport";
            this.pnlGridViewport.TabIndex = 1;
            //
            // grid
            //
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.AllowUserToOrderColumns = true;
            this.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.grid.BackgroundColor = System.Drawing.Color.White;
            this.grid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.grid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.grid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.grid.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(28, 78, 69);
            this.grid.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9F, System.Drawing.FontStyle.Bold);
            this.grid.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            this.grid.ColumnHeadersHeight = 42;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.grid.DefaultCellStyle.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F);
            this.grid.DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(41, 58, 52);
            this.grid.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(213, 235, 225);
            this.grid.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.FromArgb(25, 63, 55);
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.EnableHeadersVisualStyles = false;
            this.grid.GridColor = System.Drawing.Color.FromArgb(222, 233, 227);
            this.grid.MultiSelect = false;
            this.grid.Name = "grid";
            this.grid.ReadOnly = true;
            this.grid.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.grid.RowHeadersVisible = false;
            this.grid.RowTemplate.Height = 36;
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grid.Visible = false;
            this.grid.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(247, 250, 248);
            this.grid.TabIndex = 0;
            //
            // lblEmptyState
            //
            this.lblEmptyState.BackColor = System.Drawing.Color.White;
            this.lblEmptyState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblEmptyState.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 11F, System.Drawing.FontStyle.Bold);
            this.lblEmptyState.ForeColor = System.Drawing.Color.FromArgb(100, 124, 114);
            this.lblEmptyState.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblEmptyState.Text = "برای دیدن گزارش، یک پایگاه و جدول انتخاب کنید.";
            this.lblEmptyState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblEmptyState.Name = "lblEmptyState";
            this.lblEmptyState.TabIndex = 1;
            //
            // tlpPager
            //
            this.tlpPager.ColumnCount = 4;
            this.tlpPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 116F));
            this.tlpPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 142F));
            this.tlpPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 116F));
            this.tlpPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPager.Controls.Add(this.btnPrevious, 0, 0);
            this.tlpPager.Controls.Add(this.lblPage, 1, 0);
            this.tlpPager.Controls.Add(this.btnNext, 2, 0);
            this.tlpPager.Controls.Add(this.lblPageHint, 3, 0);
            this.tlpPager.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPager.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpPager.RowCount = 1;
            this.tlpPager.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPager.Name = "tlpPager";
            this.tlpPager.TabIndex = 4;
            //
            // btnPrevious
            //
            this.btnPrevious.BackColor = System.Drawing.Color.White;
            this.btnPrevious.BorderColor = System.Drawing.Color.FromArgb(174, 202, 190);
            this.btnPrevious.BorderSize = 1;
            this.btnPrevious.CornerRadius = 12;
            this.btnPrevious.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPrevious.FlatAppearance.BorderSize = 0;
            this.btnPrevious.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrevious.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F, System.Drawing.FontStyle.Bold);
            this.btnPrevious.ForeColor = System.Drawing.Color.FromArgb(37, 84, 73);
            this.btnPrevious.Image = global::FuntinoControlCenter.Properties.ReportsVisualAssets.Previous;
            this.btnPrevious.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnPrevious.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.btnPrevious.TabIndex = 0;
            this.btnPrevious.Text = "قبلی";
            this.btnPrevious.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnPrevious.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnPrevious.UseVisualStyleBackColor = false;
            //
            // lblPage
            //
            this.lblPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPage.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9F, System.Drawing.FontStyle.Bold);
            this.lblPage.ForeColor = System.Drawing.Color.FromArgb(37, 84, 73);
            this.lblPage.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblPage.Text = "صفحهٔ ۰ از ۰";
            this.lblPage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPage.Name = "lblPage";
            this.lblPage.TabIndex = 1;
            //
            // btnNext
            //
            this.btnNext.BackColor = System.Drawing.Color.White;
            this.btnNext.BorderColor = System.Drawing.Color.FromArgb(174, 202, 190);
            this.btnNext.BorderSize = 1;
            this.btnNext.CornerRadius = 12;
            this.btnNext.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNext.FlatAppearance.BorderSize = 0;
            this.btnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNext.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F, System.Drawing.FontStyle.Bold);
            this.btnNext.ForeColor = System.Drawing.Color.FromArgb(37, 84, 73);
            this.btnNext.Image = global::FuntinoControlCenter.Properties.ReportsVisualAssets.Next;
            this.btnNext.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnNext.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.btnNext.Name = "btnNext";
            this.btnNext.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.btnNext.TabIndex = 2;
            this.btnNext.Text = "بعدی";
            this.btnNext.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnNext.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnNext.UseVisualStyleBackColor = false;
            //
            // lblPageHint
            //
            this.lblPageHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPageHint.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8F);
            this.lblPageHint.ForeColor = System.Drawing.Color.FromArgb(103, 123, 114);
            this.lblPageHint.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblPageHint.Text = "در هر صفحه حداکثر ۱۰۰ ردیف نمایش داده می‌شود.";
            this.lblPageHint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblPageHint.Name = "lblPageHint";
            this.lblPageHint.TabIndex = 3;
            //
            // ReportsForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::FuntinoControlCenter.Properties.ReportsVisualAssets.Background;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1320, 860);
            this.Controls.Add(this.tlpRoot);
            this.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 9F);
            this.MinimumSize = new System.Drawing.Size(1040, 740);
            this.Name = "ReportsForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "گزارش‌های مدیریتی";
            this.Load += new System.EventHandler(this.ReportsForm_Load);
            this.btnBack.Click += new System.EventHandler(this.Back_Click);
            this.btnGenerate.Click += new System.EventHandler(this.Generate_Click);
            this.cmbDatabase.SelectedIndexChanged += new System.EventHandler(this.DatabaseChanged);
            this.cmbTables.SelectedIndexChanged += new System.EventHandler(this.TableSelectionChanged);
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Search_KeyDown);
            this.btnExcel.Click += new System.EventHandler(this.ExportExcel_Click);
            this.btnPdf.Click += new System.EventHandler(this.ExportPdf_Click);
            this.btnPrevious.Click += new System.EventHandler(this.Previous_Click);
            this.btnNext.Click += new System.EventHandler(this.Next_Click);
            this.tlpRoot.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.tlpHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picHeader)).EndInit();
            this.tlpHeaderText.ResumeLayout(false);
            this.pnlFilters.ResumeLayout(false);
            this.tlpFilters.ResumeLayout(false);
            this.pnlDatabaseInput.ResumeLayout(false);
            this.pnlTableInput.ResumeLayout(false);
            this.pnlSearchInput.ResumeLayout(false);
            this.tlpSearchInput.ResumeLayout(false);
            this.tlpSearchInput.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSearch)).EndInit();
            this.pnlSummaryBar.ResumeLayout(false);
            this.tlpSummary.ResumeLayout(false);
            this.tlpSummaryText.ResumeLayout(false);
            this.pnlSummaryStatus.ResumeLayout(false);
            this.flpExports.ResumeLayout(false);
            this.pnlGridCardBorder.ResumeLayout(false);
            this.tlpGridCard.ResumeLayout(false);
            this.tlpGridHeading.ResumeLayout(false);
            this.pnlGridViewport.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.tlpPager.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
