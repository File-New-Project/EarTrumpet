using EarTrumpet.Interop;
using Microsoft.Win32;
using System.Globalization;

namespace EarTrumpet.Services
{
    public static class UserSystemPreferencesService
    {
        public static bool IsTransparencyEnabled => ReadPersonalizationSetting("EnableTransparency");
        public static bool UseAccentColor => ReadPersonalizationSetting("ColorPrevalence");
        public static bool IsLightTheme => ReadPersonalizationSetting("AppsUseLightTheme");

        public static bool IsRTL => CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;

        private static bool ReadPersonalizationSetting(string key)
        {
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
            {
                return (int)baseKey.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize").GetValue(key, 0) > 0;
            }
        }
    }
}
