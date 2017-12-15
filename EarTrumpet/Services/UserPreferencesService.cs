namespace EarTrumpet.Services
{
    public static class UserPreferencesService
    {
        public static bool UseOldIcon
        {
            get
            {
                if (App.HasIdentity())
                {
                    try
                    {
                        if (!Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(nameof(UseOldIcon)))
                        {
                            return false;
                        }
                        return (bool)Windows.Storage.ApplicationData.Current.LocalSettings.Values[nameof(UseOldIcon)];
                    }
                    catch
                    {
                        // In case Windows Storage APIs are not stable (seen in Dev Dashboard)
                    }
                }
                return false;
            }
            set
            {
                if (App.HasIdentity())
                {
                    try
                    {
                        Windows.Storage.ApplicationData.Current.LocalSettings.Values[nameof(UseOldIcon)] = value;
                    }
                    catch
                    {
                        // In case Windows Storage APIs are not stable (seen in Dev Dashboard)
                    }
                }
            }
        }
    }
}