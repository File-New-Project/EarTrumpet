using Microsoft.Win32;
using System.Globalization;

namespace EarTrumpet.Misc
{
    static class SystemSettings
    {
        internal static bool IsTransparencyEnabled => ReadPersonalizationSetting("EnableTransparency");
        internal static bool UseAccentColor => ReadPersonalizationSetting("ColorPrevalence");
        internal static bool IsLightTheme => ReadPersonalizationSetting("AppsUseLightTheme");

        internal static bool IsRTL => CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;

        private static bool ReadPersonalizationSetting(string key)
        {
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
            using (var subKey = baseKey.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                return ((int)subKey.GetValue(key, 0)) > 0;
            }
        }
    }
}
