using System.Globalization;
using System.Resources;

namespace FuntinoControlCenter.Properties
{
    internal static class AccessManagementVisualAssets
    {
        private static readonly ResourceManager ResourceManager = new ResourceManager(
            "FuntinoControlCenter.Properties.AccessManagementVisualAssets",
            typeof(AccessManagementVisualAssets).Assembly);

        public static System.Drawing.Bitmap Background { get { return GetBitmap("Background"); } }
        public static System.Drawing.Bitmap UserAdd { get { return GetBitmap("UserAdd"); } }
        public static System.Drawing.Bitmap UserEdit { get { return GetBitmap("UserEdit"); } }
        public static System.Drawing.Bitmap PasswordReset { get { return GetBitmap("PasswordReset"); } }
        public static System.Drawing.Bitmap UserStatus { get { return GetBitmap("UserStatus"); } }
        public static System.Drawing.Bitmap Role { get { return GetBitmap("Role"); } }
        public static System.Drawing.Bitmap Permission { get { return GetBitmap("Permission"); } }
        public static System.Drawing.Bitmap Save { get { return GetBitmap("Save"); } }
        public static System.Drawing.Bitmap Delete { get { return GetBitmap("Delete"); } }
        public static System.Drawing.Bitmap Search { get { return GetBitmap("Search"); } }

        private static System.Drawing.Bitmap GetBitmap(string name)
        {
            return ResourceManager.GetObject(name, CultureInfo.CurrentUICulture) as System.Drawing.Bitmap;
        }
    }
}
