using Microsoft.Win32;
using System.Globalization;
using EarTrumpet.Extensions;
using System;

namespace EarTrumpet.DataModel
{
    static class SystemSettings
    {
        internal static bool IsTransparencyEnabled => ReadPersonalizationSetting("EnableTransparency");
        internal static bool UseAccentColor => ReadPersonalizationSetting("ColorPrevalence");
        internal static bool IsLightTheme => ReadPersonalizationSetting("AppsUseLightTheme", 1 /* Light theme is system default */);
        internal static bool IsSystemLightTheme => LightThemeShim(ReadPersonalizationSetting("SystemUsesLightTheme"));
        internal static bool IsRTL => CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;

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

        private static bool ReadPersonalizationSetting(string valueName, int defaultValue = 0)
        {
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
            using (var subKey = baseKey.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                return subKey.GetValue<int>(valueName, defaultValue) > 0;
            }
        }

        private static bool LightThemeShim(bool registryValue)
        {
            if (Environment.OSVersion.IsGreaterThan(OSVersions.RS5_1809))
            {
                return registryValue;
            }

#if DEBUG
            return IsLightTheme;
#else
            return false; // No system theme prior to 19H1/RS6.
#endif
        }
    }
}
