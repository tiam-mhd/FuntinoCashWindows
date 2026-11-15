using System.Globalization;
using System.Resources;

namespace FuntinoControlCenter.Properties
{
    internal static class ReportsVisualAssets
    {
        private static readonly ResourceManager ResourceManager = new ResourceManager(
            "FuntinoControlCenter.Properties.ReportsVisualAssets",
            typeof(ReportsVisualAssets).Assembly);

        public static System.Drawing.Bitmap Background { get { return GetBitmap("Background"); } }
        public static System.Drawing.Bitmap Report { get { return GetBitmap("Report"); } }
        public static System.Drawing.Bitmap ReportAction { get { return GetBitmap("ReportAction"); } }
        public static System.Drawing.Bitmap Back { get { return GetBitmap("Back"); } }
        public static System.Drawing.Bitmap Search { get { return GetBitmap("Search"); } }
        public static System.Drawing.Bitmap ExcelExport { get { return GetBitmap("ExcelExport"); } }
        public static System.Drawing.Bitmap PdfExport { get { return GetBitmap("PdfExport"); } }
        public static System.Drawing.Bitmap Previous { get { return GetBitmap("Previous"); } }
        public static System.Drawing.Bitmap Next { get { return GetBitmap("Next"); } }

        private static System.Drawing.Bitmap GetBitmap(string name)
        {
            return ResourceManager.GetObject(name, CultureInfo.CurrentUICulture) as System.Drawing.Bitmap;
        }
    }
}
