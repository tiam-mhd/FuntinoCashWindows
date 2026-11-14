using System;
using System.Drawing.Text;
using System.IO;

namespace FuntinoControlCenter.UI
{
    internal static class AppFontManager
    {
        private static readonly PrivateFontCollection PrivateFonts = new PrivateFontCollection();
        private static readonly object SyncRoot = new object();
        private static bool initialized;
        private static System.Drawing.FontFamily vazirmatnFamily;

        internal static System.Drawing.FontFamily VazirmatnFamily
        {
            get
            {
                EnsureVazirmatnLoaded();
                return vazirmatnFamily ?? System.Drawing.FontFamily.GenericSansSerif;
            }
        }

        internal static void EnsureVazirmatnLoaded()
        {
            lock (SyncRoot)
            {
                if (initialized) return;
                initialized = true;

                string assemblyDirectory = Path.GetDirectoryName(typeof(AppFontManager).Assembly.Location);
                string fontPath = Path.Combine(assemblyDirectory ?? AppDomain.CurrentDomain.BaseDirectory,
                    "Assets", "Fonts", "Vazirmatn-Regular.ttf");
                if (!File.Exists(fontPath)) return;

                PrivateFonts.AddFontFile(fontPath);
                foreach (System.Drawing.FontFamily family in PrivateFonts.Families)
                {
                    if (String.Equals(family.Name, "Vazirmatn", StringComparison.OrdinalIgnoreCase))
                    {
                        vazirmatnFamily = family;
                        break;
                    }
                }
            }
        }
    }
}
