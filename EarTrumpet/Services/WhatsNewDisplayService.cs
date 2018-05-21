using EarTrumpet.Extensions;
using Windows.ApplicationModel;

namespace EarTrumpet.Services
{
    static class WhatsNewDisplayService
    {
        internal static void ShowIfAppropriate()
        {
            if (App.Current.HasIdentity())
            {
                var currentVersion = PackageVersionToReadableString(Package.Current.Id.Version);
                var hasShownFirstRun = false;
                var lastVersion = Windows.Storage.ApplicationData.Current.LocalSettings.Values[nameof(currentVersion)];
                if ((lastVersion == null || currentVersion != (string)lastVersion))
                {
                    Windows.Storage.ApplicationData.Current.LocalSettings.Values[nameof(currentVersion)] = currentVersion;

                    if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(nameof(hasShownFirstRun)))
                    {
                        try
                        {
                            System.Diagnostics.Process.Start("eartrumpet:");
                        }
                        catch { }
                    }
                }
            }            
        }

        private static string PackageVersionToReadableString(PackageVersion packageVersion)
        {
            return $"{packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}.{packageVersion.Revision}";
        }
    }
}
