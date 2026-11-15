using System.Drawing;
using System.Windows.Forms;

namespace FuntinoControlCenter.UI
{
    public partial class AppLoadingIndicator : UserControl
    {
        public AppLoadingIndicator()
        {
            InitializeComponent();
        }

        public void BeginLoading(string message, Image icon)
        {
            lblLoadingMessage.Text = message ?? string.Empty;
            if (icon != null) picLoading.Image = icon;
            Visible = true;
            BringToFront();
        }

        public void EndLoading()
        {
            Visible = false;
        }
    }
}
