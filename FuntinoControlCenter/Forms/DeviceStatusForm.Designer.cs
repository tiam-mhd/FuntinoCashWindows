namespace FuntinoControlCenter.Forms
{
    partial class DeviceStatusForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel tlpRoot, tlpHeader, tlpHeaderText, tlpDatabase, tlpDevices;
        private System.Windows.Forms.TableLayoutPanel tlpReaderCard, tlpReaderHeader, tlpReaderActions;
        private System.Windows.Forms.TableLayoutPanel tlpPosCard, tlpPosHeader, tlpPosActions;
        private System.Windows.Forms.TableLayoutPanel tlpHistory, tlpHistoryHeader, tlpHistoryFilters, tlpSearchInput, tlpHistoryPager;
        private System.Windows.Forms.Panel pnlHeader, pnlHistoryViewport;
        private FuntinoControlCenter.UI.RoundedBorderPanel pnlDatabase, pnlReader, pnlPos, pnlHistory, pnlSearchInput;
        private System.Windows.Forms.Label lblTitle, lblSubtitle, lblDatabaseTitle, lblMainDatabaseStatus, lblDeviceDatabaseStatus;
        private System.Windows.Forms.Label lblReaderTitle, lblReaderPort, lblReaderStatus, lblPosTitle, lblPosPort, lblPosStatus;
        private System.Windows.Forms.Label lblHistoryTitle, lblHistoryHint, lblSearch, lblResultFilter, lblHistoryCount, lblHistoryPage, lblEmptyState;
        private System.Windows.Forms.PictureBox picHeader, picDatabase, picReader, picPos, picHistory, picSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.ComboBox cmbResultFilter;
        private FuntinoControlCenter.UI.RoundedButton btnBack, btnTestReader, btnTestPos, btnRefreshHistory, btnPrevious, btnNext;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEvent, colPort, colResult, colDetails, colCreated;
        private FuntinoControlCenter.UI.AppLoadingIndicator loadingDatabase, loadingReader, loadingPos, loadingHistory;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tlpRoot = new System.Windows.Forms.TableLayoutPanel();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.tlpHeader = new System.Windows.Forms.TableLayoutPanel();
            this.picHeader = new System.Windows.Forms.PictureBox();
            this.tlpHeaderText = new System.Windows.Forms.TableLayoutPanel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.btnBack = new FuntinoControlCenter.UI.RoundedButton();
            this.pnlDatabase = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.tlpDatabase = new System.Windows.Forms.TableLayoutPanel();
            this.picDatabase = new System.Windows.Forms.PictureBox();
            this.lblDatabaseTitle = new System.Windows.Forms.Label();
            this.lblMainDatabaseStatus = new System.Windows.Forms.Label();
            this.lblDeviceDatabaseStatus = new System.Windows.Forms.Label();
            this.loadingDatabase = new FuntinoControlCenter.UI.AppLoadingIndicator();
            this.tlpDevices = new System.Windows.Forms.TableLayoutPanel();
            this.pnlReader = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.tlpReaderCard = new System.Windows.Forms.TableLayoutPanel();
            this.tlpReaderHeader = new System.Windows.Forms.TableLayoutPanel();
            this.picReader = new System.Windows.Forms.PictureBox();
            this.lblReaderTitle = new System.Windows.Forms.Label();
            this.lblReaderPort = new System.Windows.Forms.Label();
            this.lblReaderStatus = new System.Windows.Forms.Label();
            this.loadingReader = new FuntinoControlCenter.UI.AppLoadingIndicator();
            this.tlpReaderActions = new System.Windows.Forms.TableLayoutPanel();
            this.btnTestReader = new FuntinoControlCenter.UI.RoundedButton();
            this.pnlPos = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.tlpPosCard = new System.Windows.Forms.TableLayoutPanel();
            this.tlpPosHeader = new System.Windows.Forms.TableLayoutPanel();
            this.picPos = new System.Windows.Forms.PictureBox();
            this.lblPosTitle = new System.Windows.Forms.Label();
            this.lblPosPort = new System.Windows.Forms.Label();
            this.lblPosStatus = new System.Windows.Forms.Label();
            this.loadingPos = new FuntinoControlCenter.UI.AppLoadingIndicator();
            this.tlpPosActions = new System.Windows.Forms.TableLayoutPanel();
            this.btnTestPos = new FuntinoControlCenter.UI.RoundedButton();
            this.pnlHistory = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.tlpHistory = new System.Windows.Forms.TableLayoutPanel();
            this.tlpHistoryHeader = new System.Windows.Forms.TableLayoutPanel();
            this.picHistory = new System.Windows.Forms.PictureBox();
            this.lblHistoryTitle = new System.Windows.Forms.Label();
            this.lblHistoryHint = new System.Windows.Forms.Label();
            this.lblSearch = new System.Windows.Forms.Label();
            this.tlpHistoryFilters = new System.Windows.Forms.TableLayoutPanel();
            this.btnRefreshHistory = new FuntinoControlCenter.UI.RoundedButton();
            this.cmbResultFilter = new System.Windows.Forms.ComboBox();
            this.lblResultFilter = new System.Windows.Forms.Label();
            this.tlpSearchInput = new System.Windows.Forms.TableLayoutPanel();
            this.picSearch = new System.Windows.Forms.PictureBox();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.pnlSearchInput = new FuntinoControlCenter.UI.RoundedBorderPanel();
            this.pnlHistoryViewport = new System.Windows.Forms.Panel();
            this.grid = new System.Windows.Forms.DataGridView();
            this.colEvent = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPort = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colResult = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDetails = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCreated = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblEmptyState = new System.Windows.Forms.Label();
            this.loadingHistory = new FuntinoControlCenter.UI.AppLoadingIndicator();
            this.tlpHistoryPager = new System.Windows.Forms.TableLayoutPanel();
            this.btnPrevious = new FuntinoControlCenter.UI.RoundedButton();
            this.lblHistoryPage = new System.Windows.Forms.Label();
            this.btnNext = new FuntinoControlCenter.UI.RoundedButton();
            this.lblHistoryCount = new System.Windows.Forms.Label();
            this.tlpRoot.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.tlpHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picHeader)).BeginInit();
            this.tlpHeaderText.SuspendLayout();
            this.pnlDatabase.SuspendLayout();
            this.tlpDatabase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDatabase)).BeginInit();
            this.tlpDevices.SuspendLayout();
            this.pnlReader.SuspendLayout();
            this.tlpReaderCard.SuspendLayout();
            this.tlpReaderHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picReader)).BeginInit();
            this.tlpReaderActions.SuspendLayout();
            this.pnlPos.SuspendLayout();
            this.tlpPosCard.SuspendLayout();
            this.tlpPosHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPos)).BeginInit();
            this.tlpPosActions.SuspendLayout();
            this.pnlHistory.SuspendLayout();
            this.tlpHistory.SuspendLayout();
            this.tlpHistoryHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picHistory)).BeginInit();
            this.tlpHistoryFilters.SuspendLayout();
            this.tlpSearchInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSearch)).BeginInit();
            this.pnlSearchInput.SuspendLayout();
            this.pnlHistoryViewport.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.tlpHistoryPager.SuspendLayout();
            this.SuspendLayout();
            //
            // tlpRoot
            //
            this.tlpRoot.BackColor = System.Drawing.Color.Transparent;
            this.tlpRoot.BackgroundImage = FuntinoControlCenter.Properties.DeviceStatusVisualAssets.Background;
            this.tlpRoot.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.tlpRoot.ColumnCount = 1;
            this.tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Controls.Add(this.pnlHeader, 0, 0);
            this.tlpRoot.Controls.Add(this.pnlDatabase, 0, 1);
            this.tlpRoot.Controls.Add(this.tlpDevices, 0, 2);
            this.tlpRoot.Controls.Add(this.pnlHistory, 0, 3);
            this.tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRoot.Padding = new System.Windows.Forms.Padding(20, 14, 20, 14);
            this.tlpRoot.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpRoot.RowCount = 4;
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 78F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 68F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 210F));
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Name = "tlpRoot";
            this.tlpRoot.TabIndex = 0;
            //
            // pnlHeader
            //
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(19, 58, 56);
            this.pnlHeader.Controls.Add(this.tlpHeader);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHeader.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(12, 5, 12, 5);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.TabIndex = 0;
            //
            // tlpHeader
            //
            this.tlpHeader.BackColor = System.Drawing.Color.Transparent;
            this.tlpHeader.ColumnCount = 3;
            this.tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 142F));
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
            this.picHeader.Image = FuntinoControlCenter.Properties.DeviceStatusVisualAssets.Device;
            this.picHeader.Margin = new System.Windows.Forms.Padding(5);
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
            this.tlpHeaderText.RowCount = 2;
            this.tlpHeaderText.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 64F));
            this.tlpHeaderText.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 36F));
            this.tlpHeaderText.Name = "tlpHeaderText";
            this.tlpHeaderText.TabIndex = 0;
            ConfigureLabel(this.lblTitle, "وضعیت پورت‌ها و دستگاه‌ها", 14F, true, System.Drawing.Color.White, System.Drawing.ContentAlignment.MiddleRight);
            this.lblTitle.Name = "lblTitle";
            ConfigureLabel(this.lblSubtitle, "بررسی اتصال دستگاه‌ها و مشاهدهٔ نتیجهٔ آخرین آزمایش‌ها", 8F, false, System.Drawing.Color.FromArgb(207, 232, 219), System.Drawing.ContentAlignment.MiddleRight);
            this.lblSubtitle.Name = "lblSubtitle";
            //
            // btnBack
            //
            ConfigureButton(this.btnBack, "بازگشت", FuntinoControlCenter.Properties.DeviceStatusVisualAssets.Back,
                System.Drawing.Color.FromArgb(249, 252, 250), System.Drawing.Color.FromArgb(25, 69, 59), System.Drawing.Color.FromArgb(183, 211, 197), 1, 10, 128, 44);
            this.btnBack.Margin = new System.Windows.Forms.Padding(4);
            this.btnBack.Name = "btnBack";
            this.btnBack.TabIndex = 0;
            this.btnBack.Click += new System.EventHandler(this.Back_Click);
            //
            // pnlDatabase
            //
            this.pnlDatabase.BackColor = System.Drawing.Color.FromArgb(250, 253, 251);
            this.pnlDatabase.BorderColor = System.Drawing.Color.FromArgb(177, 208, 192);
            this.pnlDatabase.BorderSize = 1;
            this.pnlDatabase.CornerRadius = 15;
            this.pnlDatabase.Controls.Add(this.tlpDatabase);
            this.pnlDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDatabase.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.pnlDatabase.Padding = new System.Windows.Forms.Padding(10, 3, 10, 3);
            this.pnlDatabase.Name = "pnlDatabase";
            this.pnlDatabase.TabIndex = 1;
            //
            // tlpDatabase
            //
            this.tlpDatabase.BackColor = System.Drawing.Color.Transparent;
            this.tlpDatabase.ColumnCount = 5;
            this.tlpDatabase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpDatabase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 168F));
            this.tlpDatabase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tlpDatabase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tlpDatabase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpDatabase.Controls.Add(this.picDatabase, 0, 0);
            this.tlpDatabase.Controls.Add(this.lblDatabaseTitle, 1, 0);
            this.tlpDatabase.Controls.Add(this.lblMainDatabaseStatus, 2, 0);
            this.tlpDatabase.Controls.Add(this.lblDeviceDatabaseStatus, 3, 0);
            this.tlpDatabase.Controls.Add(this.loadingDatabase, 4, 0);
            this.tlpDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDatabase.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpDatabase.RowCount = 1;
            this.tlpDatabase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDatabase.Name = "tlpDatabase";
            this.tlpDatabase.TabIndex = 0;
            //
            // picDatabase
            //
            this.picDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picDatabase.Image = FuntinoControlCenter.Properties.DeviceStatusVisualAssets.Database;
            this.picDatabase.Margin = new System.Windows.Forms.Padding(4);
            this.picDatabase.Name = "picDatabase";
            this.picDatabase.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picDatabase.TabStop = false;
            ConfigureLabel(this.lblDatabaseTitle, "سلامت ارتباط", 9F, true, PaletteDark, System.Drawing.ContentAlignment.MiddleRight);
            this.lblDatabaseTitle.Name = "lblDatabaseTitle";
            ConfigureLabel(this.lblMainDatabaseStatus, "پایگاه اصلی: در حال بررسی", 8F, true, PaletteTeal, System.Drawing.ContentAlignment.MiddleRight);
            this.lblMainDatabaseStatus.Name = "lblMainDatabaseStatus";
            ConfigureLabel(this.lblDeviceDatabaseStatus, "پایگاه دستگاه‌ها: در حال بررسی", 8F, true, PaletteTeal, System.Drawing.ContentAlignment.MiddleRight);
            this.lblDeviceDatabaseStatus.Name = "lblDeviceDatabaseStatus";
            this.loadingDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loadingDatabase.Margin = new System.Windows.Forms.Padding(4, 12, 4, 12);
            this.loadingDatabase.Name = "loadingDatabase";
            this.loadingDatabase.Visible = false;
            //
            // tlpDevices
            //
            this.tlpDevices.BackColor = System.Drawing.Color.Transparent;
            this.tlpDevices.ColumnCount = 2;
            this.tlpDevices.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpDevices.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpDevices.Controls.Add(this.pnlReader, 0, 0);
            this.tlpDevices.Controls.Add(this.pnlPos, 1, 0);
            this.tlpDevices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpDevices.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpDevices.RowCount = 1;
            this.tlpDevices.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDevices.Name = "tlpDevices";
            this.tlpDevices.TabIndex = 2;
            //
            // pnlReader
            //
            this.pnlReader.BackColor = System.Drawing.Color.FromArgb(250, 253, 251);
            this.pnlReader.BorderColor = System.Drawing.Color.FromArgb(177, 208, 192);
            this.pnlReader.BorderSize = 1;
            this.pnlReader.CornerRadius = 16;
            this.pnlReader.Controls.Add(this.tlpReaderCard);
            this.pnlReader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlReader.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.pnlReader.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.pnlReader.Name = "pnlReader";
            this.pnlReader.TabIndex = 0;
            //
            // tlpReaderCard
            //
            this.tlpReaderCard.BackColor = System.Drawing.Color.Transparent;
            this.tlpReaderCard.ColumnCount = 1;
            this.tlpReaderCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpReaderCard.Controls.Add(this.tlpReaderHeader, 0, 0);
            this.tlpReaderCard.Controls.Add(this.lblReaderPort, 0, 1);
            this.tlpReaderCard.Controls.Add(this.lblReaderStatus, 0, 2);
            this.tlpReaderCard.Controls.Add(this.loadingReader, 0, 3);
            this.tlpReaderCard.Controls.Add(this.tlpReaderActions, 0, 4);
            this.tlpReaderCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpReaderCard.RowCount = 5;
            this.tlpReaderCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpReaderCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tlpReaderCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tlpReaderCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tlpReaderCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tlpReaderCard.Name = "tlpReaderCard";
            this.tlpReaderCard.TabIndex = 0;
            //
            // tlpReaderHeader
            //
            this.tlpReaderHeader.ColumnCount = 2;
            this.tlpReaderHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tlpReaderHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpReaderHeader.Controls.Add(this.picReader, 0, 0);
            this.tlpReaderHeader.Controls.Add(this.lblReaderTitle, 1, 0);
            this.tlpReaderHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpReaderHeader.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpReaderHeader.RowCount = 1;
            this.tlpReaderHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpReaderHeader.Name = "tlpReaderHeader";
            this.tlpReaderHeader.TabIndex = 0;
            this.picReader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picReader.Image = FuntinoControlCenter.Properties.DeviceStatusVisualAssets.Reader;
            this.picReader.Margin = new System.Windows.Forms.Padding(3);
            this.picReader.Name = "picReader";
            this.picReader.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picReader.TabStop = false;
            ConfigureLabel(this.lblReaderTitle, "کارت‌خوان", 11F, true, PaletteDark, System.Drawing.ContentAlignment.MiddleRight);
            this.lblReaderTitle.Name = "lblReaderTitle";
            ConfigureLabel(this.lblReaderPort, "درگاه کارت‌خوان", 8.5F, true, PaletteMuted, System.Drawing.ContentAlignment.MiddleRight);
            this.lblReaderPort.Name = "lblReaderPort";
            ConfigureLabel(this.lblReaderStatus, "برای بررسی اتصال، دکمهٔ پایین را بزنید.", 8F, false, PaletteMuted, System.Drawing.ContentAlignment.MiddleRight);
            this.lblReaderStatus.Name = "lblReaderStatus";
            this.loadingReader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loadingReader.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.loadingReader.Name = "loadingReader";
            this.loadingReader.Visible = false;
            //
            // tlpReaderActions
            //
            this.tlpReaderActions.ColumnCount = 2;
            this.tlpReaderActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 214F));
            this.tlpReaderActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpReaderActions.Controls.Add(this.btnTestReader, 0, 0);
            this.tlpReaderActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpReaderActions.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpReaderActions.RowCount = 1;
            this.tlpReaderActions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpReaderActions.Name = "tlpReaderActions";
            this.tlpReaderActions.TabIndex = 0;
            ConfigureButton(this.btnTestReader, "بررسی کارت‌خوان", FuntinoControlCenter.Properties.DeviceStatusVisualAssets.TestReader,
                PaletteTeal, System.Drawing.Color.White, PaletteTeal, 0, 11, 208, 40);
            this.btnTestReader.Margin = new System.Windows.Forms.Padding(0, 1, 4, 1);
            this.btnTestReader.Name = "btnTestReader";
            this.btnTestReader.TabIndex = 0;
            this.btnTestReader.Click += new System.EventHandler(this.TestReader_Click);
            //
            // pnlPos
            //
            this.pnlPos.BackColor = System.Drawing.Color.FromArgb(250, 253, 251);
            this.pnlPos.BorderColor = System.Drawing.Color.FromArgb(177, 208, 192);
            this.pnlPos.BorderSize = 1;
            this.pnlPos.CornerRadius = 16;
            this.pnlPos.Controls.Add(this.tlpPosCard);
            this.pnlPos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPos.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.pnlPos.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.pnlPos.Name = "pnlPos";
            this.pnlPos.TabIndex = 1;
            //
            // tlpPosCard
            //
            this.tlpPosCard.BackColor = System.Drawing.Color.Transparent;
            this.tlpPosCard.ColumnCount = 1;
            this.tlpPosCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPosCard.Controls.Add(this.tlpPosHeader, 0, 0);
            this.tlpPosCard.Controls.Add(this.lblPosPort, 0, 1);
            this.tlpPosCard.Controls.Add(this.lblPosStatus, 0, 2);
            this.tlpPosCard.Controls.Add(this.loadingPos, 0, 3);
            this.tlpPosCard.Controls.Add(this.tlpPosActions, 0, 4);
            this.tlpPosCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPosCard.RowCount = 5;
            this.tlpPosCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpPosCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tlpPosCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tlpPosCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tlpPosCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tlpPosCard.Name = "tlpPosCard";
            this.tlpPosCard.TabIndex = 0;
            //
            // tlpPosHeader
            //
            this.tlpPosHeader.ColumnCount = 2;
            this.tlpPosHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tlpPosHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPosHeader.Controls.Add(this.picPos, 0, 0);
            this.tlpPosHeader.Controls.Add(this.lblPosTitle, 1, 0);
            this.tlpPosHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPosHeader.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpPosHeader.RowCount = 1;
            this.tlpPosHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPosHeader.Name = "tlpPosHeader";
            this.tlpPosHeader.TabIndex = 0;
            this.picPos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picPos.Image = FuntinoControlCenter.Properties.DeviceStatusVisualAssets.Pos;
            this.picPos.Margin = new System.Windows.Forms.Padding(3);
            this.picPos.Name = "picPos";
            this.picPos.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPos.TabStop = false;
            ConfigureLabel(this.lblPosTitle, "پایانهٔ فروش", 11F, true, PaletteDark, System.Drawing.ContentAlignment.MiddleRight);
            this.lblPosTitle.Name = "lblPosTitle";
            ConfigureLabel(this.lblPosPort, "درگاه پایانهٔ فروش", 8.5F, true, PaletteMuted, System.Drawing.ContentAlignment.MiddleRight);
            this.lblPosPort.Name = "lblPosPort";
            ConfigureLabel(this.lblPosStatus, "برای بررسی اتصال، دکمهٔ پایین را بزنید.", 8F, false, PaletteMuted, System.Drawing.ContentAlignment.MiddleRight);
            this.lblPosStatus.Name = "lblPosStatus";
            this.loadingPos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loadingPos.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.loadingPos.Name = "loadingPos";
            this.loadingPos.Visible = false;
            //
            // tlpPosActions
            //
            this.tlpPosActions.ColumnCount = 2;
            this.tlpPosActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 214F));
            this.tlpPosActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPosActions.Controls.Add(this.btnTestPos, 0, 0);
            this.tlpPosActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPosActions.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpPosActions.RowCount = 1;
            this.tlpPosActions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPosActions.Name = "tlpPosActions";
            this.tlpPosActions.TabIndex = 0;
            ConfigureButton(this.btnTestPos, "بررسی پایانه", FuntinoControlCenter.Properties.DeviceStatusVisualAssets.TestPos,
                PaletteTeal, System.Drawing.Color.White, PaletteTeal, 0, 11, 208, 40);
            this.btnTestPos.Margin = new System.Windows.Forms.Padding(0, 1, 4, 1);
            this.btnTestPos.Name = "btnTestPos";
            this.btnTestPos.TabIndex = 0;
            this.btnTestPos.Click += new System.EventHandler(this.TestPos_Click);
            //
            // pnlHistory
            //
            this.pnlHistory.BackColor = System.Drawing.Color.FromArgb(250, 253, 251);
            this.pnlHistory.BorderColor = System.Drawing.Color.FromArgb(177, 208, 192);
            this.pnlHistory.BorderSize = 1;
            this.pnlHistory.CornerRadius = 16;
            this.pnlHistory.Controls.Add(this.tlpHistory);
            this.pnlHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHistory.Margin = new System.Windows.Forms.Padding(0);
            this.pnlHistory.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.pnlHistory.Name = "pnlHistory";
            this.pnlHistory.TabIndex = 3;
            //
            // tlpHistory
            //
            this.tlpHistory.ColumnCount = 1;
            this.tlpHistory.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHistory.Controls.Add(this.tlpHistoryHeader, 0, 0);
            this.tlpHistory.Controls.Add(this.tlpHistoryFilters, 0, 1);
            this.tlpHistory.Controls.Add(this.loadingHistory, 0, 2);
            this.tlpHistory.Controls.Add(this.pnlHistoryViewport, 0, 3);
            this.tlpHistory.Controls.Add(this.tlpHistoryPager, 0, 4);
            this.tlpHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHistory.RowCount = 5;
            this.tlpHistory.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tlpHistory.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 54F));
            this.tlpHistory.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tlpHistory.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHistory.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.tlpHistory.Name = "tlpHistory";
            this.tlpHistory.TabIndex = 0;
            //
            // tlpHistoryHeader
            //
            this.tlpHistoryHeader.ColumnCount = 2;
            this.tlpHistoryHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tlpHistoryHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHistoryHeader.Controls.Add(this.picHistory, 0, 0);
            this.tlpHistoryHeader.Controls.Add(this.lblHistoryTitle, 1, 0);
            this.tlpHistoryHeader.RowCount = 2;
            this.tlpHistoryHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tlpHistoryHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tlpHistoryHeader.Controls.Add(this.lblHistoryHint, 1, 1);
            this.tlpHistoryHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHistoryHeader.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpHistoryHeader.Name = "tlpHistoryHeader";
            this.tlpHistoryHeader.TabIndex = 0;
            this.picHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picHistory.Image = FuntinoControlCenter.Properties.DeviceStatusVisualAssets.History;
            this.picHistory.Margin = new System.Windows.Forms.Padding(3);
            this.picHistory.Name = "picHistory";
            this.picHistory.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picHistory.TabStop = false;
            this.tlpHistoryHeader.SetRowSpan(this.picHistory, 2);
            this.tlpHistoryHeader.SetRowSpan(this.lblHistoryTitle, 1);
            ConfigureLabel(this.lblHistoryTitle, "سوابق بررسی دستگاه‌ها", 11F, true, PaletteDark, System.Drawing.ContentAlignment.MiddleRight);
            this.lblHistoryTitle.Name = "lblHistoryTitle";
            ConfigureLabel(this.lblHistoryHint, "برای جست‌وجو، عبارت را بنویسید و کلید Enter را بزنید.", 7.5F, false, PaletteMuted, System.Drawing.ContentAlignment.MiddleRight);
            this.lblHistoryHint.Name = "lblHistoryHint";
            //
            // tlpHistoryFilters
            //
            this.tlpHistoryFilters.ColumnCount = 5;
            this.tlpHistoryFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 156F));
            this.tlpHistoryFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 148F));
            this.tlpHistoryFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 62F));
            this.tlpHistoryFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 78F));
            this.tlpHistoryFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHistoryFilters.Controls.Add(this.btnRefreshHistory, 0, 0);
            this.tlpHistoryFilters.Controls.Add(this.cmbResultFilter, 1, 0);
            this.tlpHistoryFilters.Controls.Add(this.lblResultFilter, 2, 0);
            this.tlpHistoryFilters.Controls.Add(this.lblSearch, 3, 0);
            this.tlpHistoryFilters.Controls.Add(this.pnlSearchInput, 4, 0);
            this.tlpHistoryFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHistoryFilters.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpHistoryFilters.RowCount = 1;
            this.tlpHistoryFilters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHistoryFilters.Name = "tlpHistoryFilters";
            this.tlpHistoryFilters.TabIndex = 0;
            //
            // btnRefreshHistory
            //
            ConfigureButton(this.btnRefreshHistory, "تازه‌سازی", FuntinoControlCenter.Properties.DeviceStatusVisualAssets.Refresh,
                System.Drawing.Color.FromArgb(250, 253, 251), PaletteDark, System.Drawing.Color.FromArgb(178, 207, 191), 1, 10, 148, 40);
            this.btnRefreshHistory.Margin = new System.Windows.Forms.Padding(3, 3, 2, 3);
            this.btnRefreshHistory.Name = "btnRefreshHistory";
            this.btnRefreshHistory.TabIndex = 0;
            this.btnRefreshHistory.Click += new System.EventHandler(this.RefreshHistory_Click);
            //
            // cmbResultFilter
            //
            this.cmbResultFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbResultFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbResultFilter.Font = FontRegular(8F);
            this.cmbResultFilter.FormattingEnabled = true;
            this.cmbResultFilter.Items.AddRange(new object[] { "همهٔ نتیجه‌ها", "پاسخ دریافت شد", "پاسخ دریافت نشد" });
            this.cmbResultFilter.Margin = new System.Windows.Forms.Padding(4, 6, 3, 6);
            this.cmbResultFilter.Name = "cmbResultFilter";
            this.cmbResultFilter.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cmbResultFilter.TabIndex = 1;
            this.cmbResultFilter.SelectedIndexChanged += new System.EventHandler(this.ResultFilterChanged);
            //
            // lblResultFilter
            //
            ConfigureLabel(this.lblResultFilter, "نتیجه", 8F, true, PaletteDark, System.Drawing.ContentAlignment.MiddleRight);
            this.lblResultFilter.Name = "lblResultFilter";
            ConfigureLabel(this.lblSearch, "جست‌وجو", 8F, true, PaletteDark, System.Drawing.ContentAlignment.MiddleRight);
            this.lblSearch.Name = "lblSearch";
            //
            // pnlSearchInput
            //
            this.pnlSearchInput.BackColor = System.Drawing.Color.FromArgb(246, 250, 247);
            this.pnlSearchInput.BorderColor = System.Drawing.Color.FromArgb(175, 207, 190);
            this.pnlSearchInput.BorderSize = 1;
            this.pnlSearchInput.CornerRadius = 10;
            this.pnlSearchInput.Controls.Add(this.tlpSearchInput);
            this.pnlSearchInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSearchInput.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlSearchInput.Padding = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.pnlSearchInput.Name = "pnlSearchInput";
            this.pnlSearchInput.TabIndex = 0;
            //
            // tlpSearchInput
            //
            this.tlpSearchInput.ColumnCount = 2;
            this.tlpSearchInput.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.tlpSearchInput.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSearchInput.Controls.Add(this.picSearch, 0, 0);
            this.tlpSearchInput.Controls.Add(this.txtSearch, 1, 0);
            this.tlpSearchInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSearchInput.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpSearchInput.RowCount = 1;
            this.tlpSearchInput.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSearchInput.Name = "tlpSearchInput";
            this.tlpSearchInput.TabIndex = 0;
            this.picSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picSearch.Image = FuntinoControlCenter.Properties.DeviceStatusVisualAssets.Search;
            this.picSearch.Margin = new System.Windows.Forms.Padding(2);
            this.picSearch.Name = "picSearch";
            this.picSearch.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picSearch.TabStop = false;
            this.txtSearch.BackColor = System.Drawing.Color.FromArgb(246, 250, 247);
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSearch.Font = FontRegular(9F);
            this.txtSearch.ForeColor = PaletteDark;
            this.txtSearch.Margin = new System.Windows.Forms.Padding(5, 2, 2, 2);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.txtSearch.AccessibleName = "جست‌وجوی سوابق دستگاه‌ها";
            this.txtSearch.MaxLength = 250;
            this.txtSearch.TabIndex = 2;
            this.txtSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SearchText_KeyDown);
            //
            // pnlHistoryViewport
            //
            this.pnlHistoryViewport.BackColor = System.Drawing.Color.White;
            this.pnlHistoryViewport.Controls.Add(this.grid);
            this.pnlHistoryViewport.Controls.Add(this.lblEmptyState);
            this.pnlHistoryViewport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHistoryViewport.Margin = new System.Windows.Forms.Padding(2, 1, 2, 2);
            this.pnlHistoryViewport.Name = "pnlHistoryViewport";
            this.pnlHistoryViewport.Padding = new System.Windows.Forms.Padding(1);
            this.pnlHistoryViewport.TabIndex = 0;
            //
            // grid
            //
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.AllowUserToResizeRows = false;
            this.grid.AutoGenerateColumns = false;
            this.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grid.BackgroundColor = System.Drawing.Color.White;
            this.grid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.grid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.grid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.grid.ColumnHeadersHeight = 42;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this.colEvent, this.colPort, this.colResult, this.colDetails, this.colCreated });
            this.grid.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.White,
                ForeColor = PaletteDark,
                SelectionBackColor = System.Drawing.Color.FromArgb(215, 238, 226),
                SelectionForeColor = PaletteDark,
                Font = FontRegular(8F),
                Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight,
                Padding = new System.Windows.Forms.Padding(7, 0, 7, 0)
            };
            this.grid.AlternatingRowsDefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle { BackColor = System.Drawing.Color.FromArgb(246, 250, 247) };
            this.grid.EnableHeadersVisualStyles = false;
            this.grid.GridColor = System.Drawing.Color.FromArgb(224, 235, 228);
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.ReadOnly = true;
            this.grid.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.grid.RowHeadersVisible = false;
            this.grid.RowTemplate.Height = 36;
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grid.MultiSelect = false;
            this.grid.ColumnHeadersDefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.FromArgb(225, 241, 232),
                ForeColor = PaletteDark,
                Font = FontBold(8F),
                Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight,
                Padding = new System.Windows.Forms.Padding(7, 0, 7, 0)
            };
            this.grid.Name = "grid";
            this.grid.TabIndex = 0;
            this.colEvent.DataPropertyName = "EventCaption";
            this.colEvent.HeaderText = "نوع بررسی";
            this.colEvent.Name = "colEvent";
            this.colEvent.ReadOnly = true;
            this.colEvent.FillWeight = 18F;
            this.colEvent.MinimumWidth = 125;
            this.colPort.DataPropertyName = "PortCaption";
            this.colPort.HeaderText = "درگاه";
            this.colPort.Name = "colPort";
            this.colPort.ReadOnly = true;
            this.colPort.FillWeight = 12F;
            this.colPort.MinimumWidth = 82;
            this.colPort.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle { Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter };
            this.colResult.DataPropertyName = "ResultCaption";
            this.colResult.HeaderText = "نتیجه";
            this.colResult.Name = "colResult";
            this.colResult.ReadOnly = true;
            this.colResult.FillWeight = 15F;
            this.colResult.MinimumWidth = 115;
            this.colResult.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle { Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter };
            this.colDetails.DataPropertyName = "DetailsCaption";
            this.colDetails.HeaderText = "شرح";
            this.colDetails.Name = "colDetails";
            this.colDetails.ReadOnly = true;
            this.colDetails.FillWeight = 37F;
            this.colDetails.MinimumWidth = 230;
            this.colCreated.DataPropertyName = "CreatedCaption";
            this.colCreated.HeaderText = "زمان بررسی";
            this.colCreated.Name = "colCreated";
            this.colCreated.ReadOnly = true;
            this.colCreated.FillWeight = 18F;
            this.colCreated.MinimumWidth = 145;
            this.colCreated.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle { Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter };
            //
            // lblEmptyState
            //
            this.lblEmptyState.BackColor = System.Drawing.Color.White;
            this.lblEmptyState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblEmptyState.Font = FontBold(9F);
            this.lblEmptyState.ForeColor = PaletteMuted;
            this.lblEmptyState.Name = "lblEmptyState";
            this.lblEmptyState.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblEmptyState.TabIndex = 1;
            this.lblEmptyState.Text = "هنوز بررسی دستگاهی ثبت نشده است.";
            this.lblEmptyState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblEmptyState.Visible = false;
            //
            // loadingHistory
            //
            this.loadingHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loadingHistory.Name = "loadingHistory";
            this.loadingHistory.Size = new System.Drawing.Size(520, 34);
            this.loadingHistory.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.loadingHistory.TabIndex = 2;
            this.loadingHistory.Visible = false;
            //
            // tlpHistoryPager
            //
            this.tlpHistoryPager.ColumnCount = 4;
            this.tlpHistoryPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 142F));
            this.tlpHistoryPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 142F));
            this.tlpHistoryPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 116F));
            this.tlpHistoryPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHistoryPager.Controls.Add(this.btnPrevious, 0, 0);
            this.tlpHistoryPager.Controls.Add(this.lblHistoryPage, 1, 0);
            this.tlpHistoryPager.Controls.Add(this.btnNext, 2, 0);
            this.tlpHistoryPager.Controls.Add(this.lblHistoryCount, 3, 0);
            this.tlpHistoryPager.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHistoryPager.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tlpHistoryPager.RowCount = 1;
            this.tlpHistoryPager.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHistoryPager.Name = "tlpHistoryPager";
            this.tlpHistoryPager.TabIndex = 0;
            ConfigureButton(this.btnPrevious, "پیشین", FuntinoControlCenter.Properties.DeviceStatusVisualAssets.Previous,
                System.Drawing.Color.FromArgb(250, 253, 251), PaletteDark, System.Drawing.Color.FromArgb(178, 207, 191), 1, 9, 132, 38);
            this.btnPrevious.Margin = new System.Windows.Forms.Padding(3, 4, 2, 2);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.TabIndex = 0;
            this.btnPrevious.Enabled = false;
            this.btnPrevious.Click += new System.EventHandler(this.PreviousPage_Click);
            ConfigureButton(this.btnNext, "بعدی", FuntinoControlCenter.Properties.DeviceStatusVisualAssets.Next,
                System.Drawing.Color.FromArgb(250, 253, 251), PaletteDark, System.Drawing.Color.FromArgb(178, 207, 191), 1, 9, 106, 38);
            this.btnNext.Margin = new System.Windows.Forms.Padding(3, 4, 2, 2);
            this.btnNext.Name = "btnNext";
            this.btnNext.TabIndex = 2;
            this.btnNext.Enabled = false;
            this.btnNext.Click += new System.EventHandler(this.NextPage_Click);
            ConfigureLabel(this.lblHistoryPage, "صفحهٔ ۱ از ۱", 8F, true, PaletteDark, System.Drawing.ContentAlignment.MiddleCenter);
            this.lblHistoryPage.Name = "lblHistoryPage";
            ConfigureLabel(this.lblHistoryCount, "۰ بررسی", 8F, true, PaletteMuted, System.Drawing.ContentAlignment.MiddleLeft);
            this.lblHistoryCount.Name = "lblHistoryCount";
            //
            // DeviceStatusForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(18, 69, 62);
            this.BackgroundImage = FuntinoControlCenter.Properties.DeviceStatusVisualAssets.Background;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(1420, 920);
            this.Controls.Add(this.tlpRoot);
            this.Font = FontRegular(9F);
            this.MinimumSize = new System.Drawing.Size(1100, 740);
            this.Name = "DeviceStatusForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "وضعیت پورت‌ها و دستگاه‌ها";
            this.Load += new System.EventHandler(this.DeviceStatusForm_Load);
            this.tlpRoot.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.tlpHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picHeader)).EndInit();
            this.tlpHeaderText.ResumeLayout(false);
            this.pnlDatabase.ResumeLayout(false);
            this.tlpDatabase.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picDatabase)).EndInit();
            this.tlpDevices.ResumeLayout(false);
            this.pnlReader.ResumeLayout(false);
            this.tlpReaderCard.ResumeLayout(false);
            this.tlpReaderHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picReader)).EndInit();
            this.tlpReaderActions.ResumeLayout(false);
            this.pnlPos.ResumeLayout(false);
            this.tlpPosCard.ResumeLayout(false);
            this.tlpPosHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picPos)).EndInit();
            this.tlpPosActions.ResumeLayout(false);
            this.pnlHistory.ResumeLayout(false);
            this.tlpHistory.ResumeLayout(false);
            this.tlpHistoryHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picHistory)).EndInit();
            this.tlpHistoryFilters.ResumeLayout(false);
            this.tlpSearchInput.ResumeLayout(false);
            this.tlpSearchInput.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSearch)).EndInit();
            this.pnlSearchInput.ResumeLayout(false);
            this.pnlHistoryViewport.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.tlpHistoryPager.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private static readonly System.Drawing.Color PaletteDark = System.Drawing.Color.FromArgb(24, 69, 60);
        private static readonly System.Drawing.Color PaletteTeal = System.Drawing.Color.FromArgb(20, 105, 91);
        private static readonly System.Drawing.Color PaletteMuted = System.Drawing.Color.FromArgb(91, 111, 104);

        private static System.Drawing.Font FontRegular(float size)
        {
            return new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, size, System.Drawing.FontStyle.Regular);
        }

        private static System.Drawing.Font FontBold(float size)
        {
            return new System.Drawing.Font(FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, size, System.Drawing.FontStyle.Bold);
        }

        private static void ConfigureLabel(System.Windows.Forms.Label label, string text, float size, bool bold,
            System.Drawing.Color foreColor, System.Drawing.ContentAlignment alignment)
        {
            label.Dock = System.Windows.Forms.DockStyle.Fill;
            label.Font = bold ? FontBold(size) : FontRegular(size);
            label.ForeColor = foreColor;
            label.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            label.Text = text;
            label.TextAlign = alignment;
            label.AutoEllipsis = true;
        }

        private static void ConfigureButton(FuntinoControlCenter.UI.RoundedButton button, string text, System.Drawing.Image image,
            System.Drawing.Color backColor, System.Drawing.Color foreColor, System.Drawing.Color borderColor,
            int borderSize, int cornerRadius, int width, int height)
        {
            button.BackColor = backColor;
            button.BorderColor = borderColor;
            button.BorderSize = borderSize;
            button.CornerRadius = cornerRadius;
            button.FlatAppearance.BorderSize = 0;
            button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            button.Font = FontBold(8.5F);
            button.ForeColor = foreColor;
            button.Image = image;
            button.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            button.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            button.Size = new System.Drawing.Size(width, height);
            button.Text = text;
            button.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            button.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            button.UseVisualStyleBackColor = false;
        }
    }
}
