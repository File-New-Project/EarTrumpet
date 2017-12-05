using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.System;

namespace EarTrumpet.Services
{
    public static class WhatsNewDisplayService
    {
        internal static void ShowIfAppropriate()
        {
            if (App.HasIdentity())
            {
                var currentVersion = PackageVersionToReadableString(Package.Current.Id.Version);
                var hasShownFirstRun = false;
                var lastVersion = Windows.Storage.ApplicationData.Current.LocalSettings.Values[nameof(currentVersion)];
                if ((lastVersion != null && currentVersion == (string)lastVersion))
                {
                    return; 
                }

                Windows.Storage.ApplicationData.Current.LocalSettings.Values[nameof(currentVersion)] = currentVersion;

                var versionArray = lastVersion?.ToString().Split('.');
                if (versionArray?.Length > 2 && (versionArray[0] == Package.Current.Id.Version.Major.ToString() && versionArray[1] == Package.Current.Id.Version.Minor.ToString()))
                {
                    return;
                }

                if (!Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(nameof(hasShownFirstRun)))
                {
                    return;
                }

                try
                {
                    System.Diagnostics.Process.Start("eartrumpet:");
                }
                catch { }
            }            
        }

        private static string PackageVersionToReadableString(PackageVersion packageVersion)
        {
            return $"{packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}.{packageVersion.Revision}";
        }
    }
}
