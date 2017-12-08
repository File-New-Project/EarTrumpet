using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace EarTrumpet.Services
{
    public static class UserPreferencesService
    {
        public static bool UseOldIcon
        {
            get
            {
                if (!Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(nameof(UseOldIcon)))
                {
                    return false;
                }
                return (bool)Windows.Storage.ApplicationData.Current.LocalSettings.Values[nameof(UseOldIcon)];
            }
            set
            {
                Windows.Storage.ApplicationData.Current.LocalSettings.Values[nameof(UseOldIcon)] = value;
            }
        }
    }
}