using Microsoft.Win32;
using System.Globalization;
using EarTrumpet.Extensions;
using System;

namespace EarTrumpet.DataModel
{
    public static class SystemSettings
    {
        static readonly string s_PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        public static bool IsTransparencyEnabled => ReadDword(s_PersonalizeKey, "EnableTransparency");
        public static bool UseAccentColor => ReadDword(s_PersonalizeKey, "ColorPrevalence");
        public static bool IsLightTheme => ReadDword(s_PersonalizeKey, "AppsUseLightTheme", 1);
        public static bool IsSystemLightTheme => LightThemeShim(ReadDword(s_PersonalizeKey, "SystemUsesLightTheme"));
        public static bool UseDynamicScrollbars => ReadDword(@"Control Panel\Accessibility", "DynamicScrollbars", 1);
        public static bool UseAccentColorOnWindowBorders => ReadDword(@"Software\Microsoft\Windows\DWM", "ColorPrevalence");
        public static bool IsRTL => CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;

        public static string BuildLabel
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
            return false; // No system theme prior to 19H1/RS6.
        }
    }
}
