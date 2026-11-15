namespace FuntinoControlCenter.UI
{
    partial class AppLoadingIndicator
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.PictureBox picLoading;
        private System.Windows.Forms.Label lblLoadingMessage;
        private System.Windows.Forms.ProgressBar progressLoading;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.picLoading = new System.Windows.Forms.PictureBox();
            this.lblLoadingMessage = new System.Windows.Forms.Label();
            this.progressLoading = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.picLoading)).BeginInit();
            this.SuspendLayout();
            //
            // picLoading
            //
            this.picLoading.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.picLoading.Image = global::FuntinoControlCenter.Properties.Resources.ConnectionSettings;
            this.picLoading.Location = new System.Drawing.Point(454, 5);
            this.picLoading.Name = "picLoading";
            this.picLoading.Size = new System.Drawing.Size(24, 24);
            this.picLoading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLoading.TabIndex = 0;
            this.picLoading.TabStop = false;
            //
            // lblLoadingMessage
            //
            this.lblLoadingMessage.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.lblLoadingMessage.Font = new System.Drawing.Font(global::FuntinoControlCenter.UI.AppFontManager.VazirmatnFamily, 8.5F, System.Drawing.FontStyle.Bold);
            this.lblLoadingMessage.ForeColor = System.Drawing.Color.FromArgb(35, 83, 72);
            this.lblLoadingMessage.Location = new System.Drawing.Point(132, 1);
            this.lblLoadingMessage.Name = "lblLoadingMessage";
            this.lblLoadingMessage.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lblLoadingMessage.Size = new System.Drawing.Size(310, 32);
            this.lblLoadingMessage.TabIndex = 1;
            this.lblLoadingMessage.Text = "در حال انجام کار…";
            this.lblLoadingMessage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // progressLoading
            //
            this.progressLoading.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Bottom;
            this.progressLoading.Location = new System.Drawing.Point(8, 10);
            this.progressLoading.MarqueeAnimationSpeed = 28;
            this.progressLoading.Name = "progressLoading";
            this.progressLoading.Size = new System.Drawing.Size(100, 14);
            this.progressLoading.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressLoading.TabIndex = 2;
            //
            // AppLoadingIndicator
            //
            this.BackColor = System.Drawing.Color.FromArgb(239, 247, 243);
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.picLoading);
            this.Controls.Add(this.lblLoadingMessage);
            this.Controls.Add(this.progressLoading);
            this.Name = "AppLoadingIndicator";
            this.Size = new System.Drawing.Size(490, 34);
            ((System.ComponentModel.ISupportInitialize)(this.picLoading)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
