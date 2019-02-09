using Microsoft.Win32;
using System.Globalization;
using EarTrumpet.Extensions;
using System;

namespace EarTrumpet.DataModel
{
    static class SystemSettings
    {
        static readonly string s_PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        internal static bool IsTransparencyEnabled => ReadDword(s_PersonalizeKey, "EnableTransparency");
        internal static bool UseAccentColor => ReadDword(s_PersonalizeKey, "ColorPrevalence");
        internal static bool IsLightTheme => ReadDword(s_PersonalizeKey, "AppsUseLightTheme", 1 /* Light theme is system default */);
        internal static bool IsSystemLightTheme => LightThemeShim(ReadDword(s_PersonalizeKey, "SystemUsesLightTheme"));
        internal static bool IsRTL => CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;
        internal static bool UseDynamicScrollbars => ReadDword(@"Control Panel\Accessibility", "DynamicScrollbars");
        internal static bool UseAccentColorOnWindowBorders => ReadDword(@"Software\Microsoft\Windows\DWM", "ColorPrevalence");

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

        private static bool ReadDword(string key, string valueName, int defaultValue = 0)
        {
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
            using (var subKey = baseKey.OpenSubKey(key))
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
