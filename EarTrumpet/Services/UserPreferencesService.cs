using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace EarTrumpet.Services
{
    public static class UserPreferencesService
    {
        public static bool ShowDesktopApps
        {
            get
            {
                try
                {
                    return 1 == (int)Registry.CurrentUser.CreateSubKey(@"Software\EarTrumpet").GetValue("ShowDesktopApps");
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to get ShowDektopApps: " + e.Message);
                    return false;
                }
            }

            set
            {
                Registry.CurrentUser.CreateSubKey(@"Software\EarTrumpet").SetValue("ShowDesktopApps", value ? 1 : 0);
            }
        }
    }
}
