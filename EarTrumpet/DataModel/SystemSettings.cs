using Microsoft.Win32;
using System.Globalization;

namespace EarTrumpet.DataModel
{
    static class SystemSettings
    {
        internal static bool IsTransparencyEnabled => ReadPersonalizationSetting("EnableTransparency");
        internal static bool UseAccentColor => ReadPersonalizationSetting("ColorPrevalence");
        internal static bool IsLightTheme => ReadPersonalizationSetting("AppsUseLightTheme", 1 /* Light theme is system default */);

        internal static bool IsRTL => CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;

        internal static string BuildLabel
        {
            get
            {
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var subKey = baseKey.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion"))
                {
                    return (string)subKey.GetValue("BuildLabEx", "No BuildLabEx set");
                }
            }
        }

        private static bool ReadPersonalizationSetting(string key, int defaultValue = 0)
        {
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
            using (var subKey = baseKey.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                return ((int)subKey.GetValue(key, defaultValue)) > 0;
            }
        }
    }
}
