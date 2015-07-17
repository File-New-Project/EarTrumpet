using Microsoft.Win32;

namespace EarTrumpet.Services
{
    public static class UserSystemPreferencesService
    {
        public static bool IsTransparencyEnabled
        { 
            get 
            {
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
                {
                    return (int)baseKey.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize").GetValue("EnableTransparency", 0) > 0;
                }
            }
        }

        public static bool UseAccentColor
        {
            get
            {
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
                {
                    return (int)baseKey.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize").GetValue("ColorPrevalence", 0) > 0;
                }
            }
        }
    }
}
