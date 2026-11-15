using System.Globalization;
using System.Resources;

namespace FuntinoControlCenter.Properties
{
    internal static class DeviceStatusVisualAssets
    {
        private static readonly ResourceManager ResourceManager = new ResourceManager(
            "FuntinoControlCenter.Properties.DeviceStatusVisualAssets",
            typeof(DeviceStatusVisualAssets).Assembly);

        public static System.Drawing.Bitmap Background { get { return GetBitmap("Background"); } }
        public static System.Drawing.Bitmap Device { get { return GetBitmap("StatusReader"); } }
        public static System.Drawing.Bitmap Back { get { return GetBitmap("Back"); } }
        public static System.Drawing.Bitmap Reader { get { return GetBitmap("TestReader"); } }
        public static System.Drawing.Bitmap Pos { get { return GetBitmap("TestPos"); } }
        public static System.Drawing.Bitmap Database { get { return GetBitmap("DatabaseConnected"); } }
        public static System.Drawing.Bitmap History { get { return GetBitmap("History"); } }
        public static System.Drawing.Bitmap TestReader { get { return GetBitmap("TestReader"); } }
        public static System.Drawing.Bitmap TestPos { get { return GetBitmap("TestPos"); } }
        public static System.Drawing.Bitmap Refresh { get { return GetBitmap("Refresh"); } }
        public static System.Drawing.Bitmap Search { get { return GetBitmap("Search"); } }
        public static System.Drawing.Bitmap Previous { get { return GetBitmap("Previous"); } }
        public static System.Drawing.Bitmap Next { get { return GetBitmap("Next"); } }

        private static System.Drawing.Bitmap GetBitmap(string name)
        {
            return ResourceManager.GetObject(name, CultureInfo.CurrentUICulture) as System.Drawing.Bitmap;
        }
    }
}
